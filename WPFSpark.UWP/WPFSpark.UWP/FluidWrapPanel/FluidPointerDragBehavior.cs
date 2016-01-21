// Copyright (c) 2015 Ratish Philip 
//
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal 
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is 
// furnished to do so, subject to the following conditions: 
// 
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software. 
// 
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, 
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE. 
//
// This file is part of the WPFSpark project: https://github.com/ratishphilip/wpfspark
//
// WPFSpark.UWP v1.2
// 

using Windows.Devices.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Microsoft.Xaml.Interactivity;

namespace WPFSpark
{
    /// <summary>
    /// Defines the Drag Behavior in the FluidWrapPanel using the Mouse/Touch/Pen
    /// </summary>
    public class FluidPointerDragBehavior : Behavior<UIElement>
    {
        #region Enums

        /// <summary>
        /// The various types of Pointers that can participate in FluidDrag
        /// </summary>
        public enum DragButtonType
        {
            MouseLeftButton,
            MouseMiddleButton,
            MouseRightButton,
            Pen,
            Touch
        }

        #endregion

        #region Fields

        FluidWrapPanel _parentFwPanel;
        ListBoxItem _parentLbItem;

        #endregion

        #region Dependency Properties

        #region DragButton

        /// <summary>
        /// DragButton Dependency Property
        /// </summary>
        public static readonly DependencyProperty DragButtonProperty =
            DependencyProperty.Register("DragButton", typeof(DragButtonType), typeof(FluidPointerDragBehavior),
                new PropertyMetadata(DragButtonType.MouseLeftButton));

        /// <summary>
        /// Gets or sets the DragButton property. This dependency property 
        /// indicates which Mouse button should participate in the drag interaction.
        /// </summary>
        public DragButtonType DragButton
        {
            get { return (DragButtonType)GetValue(DragButtonProperty); }
            set { SetValue(DragButtonProperty, value); }
        }

        #endregion

        #endregion

        #region Overrides

        /// <summary>
        /// When the Behavior is attached to the UIElement
        /// </summary>
        protected override void OnAttached()
        {
            if ((AssociatedObject as FrameworkElement) == null)
                return;
            // Subscribe to the Loaded event
            ((FrameworkElement)AssociatedObject).Loaded += OnAssociatedObjectLoaded;
        }

        /// <summary>
        /// When the Behavior is detached from the UIElement
        /// </summary>
        protected override void OnDetaching()
        {
            if ((AssociatedObject as FrameworkElement) == null)
                return;

            ((FrameworkElement)AssociatedObject).Loaded -= OnAssociatedObjectLoaded;
            if (_parentLbItem != null)
            {
                _parentLbItem.PointerPressed -= OnPointerPressed;
                _parentLbItem.PointerMoved -= OnPointerMoved;
                _parentLbItem.PointerReleased -= OnPointerReleased;
            }
            else
            {
                AssociatedObject.PointerPressed -= OnPointerPressed;
                AssociatedObject.PointerMoved -= OnPointerMoved;
                AssociatedObject.PointerReleased -= OnPointerReleased;
            }
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Get the parent FluidWrapPanel and check if the AssociatedObject is
        /// hosted inside a ListBoxItem (this scenario will occur if the FluidWrapPanel
        /// is the ItemsPanel for a ListBox).
        /// </summary>
        private void GetParentPanel()
        {
            if ((AssociatedObject as FrameworkElement) == null)
                return;

            var ancestor = (FrameworkElement)AssociatedObject;

            while (ancestor != null)
            {
                if (ancestor is ListBoxItem)
                {
                    _parentLbItem = ancestor as ListBoxItem;
                }

                if (ancestor is FluidWrapPanel)
                {
                    _parentFwPanel = ancestor as FluidWrapPanel;
                    // No need to go further up
                    return;
                }

                // Find the visual ancestor of the current item
                ancestor = VisualTreeHelper.GetParent(ancestor) as FrameworkElement;
            }
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handler for the Load event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnAssociatedObjectLoaded(object sender, RoutedEventArgs e)
        {
            // Get the parent OldFluidWrapPanel and check if the AssociatedObject is
            // hosted inside a ListBoxItem (this scenario will occur if the OldFluidWrapPanel
            // is the ItemsPanel for a ListBox).
            GetParentPanel();

            // Subscribe to the Mouse down/move/up events
            if (_parentLbItem != null)
            {
                _parentLbItem.PointerPressed += OnPointerPressed;
                _parentLbItem.PointerMoved += OnPointerMoved;
                _parentLbItem.PointerReleased += OnPointerReleased;
            }
            else
            {
                AssociatedObject.PointerPressed += OnPointerPressed;
                AssociatedObject.PointerMoved += OnPointerMoved;
                AssociatedObject.PointerReleased += OnPointerReleased;
            }
        }

        /// <summary>
        /// Handler for Pointer Pressed event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void OnPointerPressed(object sender, PointerRoutedEventArgs e)
        {
            var ptrPt = _parentLbItem != null ? e.GetCurrentPoint(_parentLbItem) : e.GetCurrentPoint(AssociatedObject);
            var isValidPointer = (((e.Pointer.PointerDeviceType == PointerDeviceType.Mouse)) &&
                                   (((DragButton == DragButtonType.MouseLeftButton) && (ptrPt.Properties.IsLeftButtonPressed)) ||
                                   ((DragButton == DragButtonType.MouseRightButton) && (ptrPt.Properties.IsRightButtonPressed)) ||
                                   ((DragButton == DragButtonType.MouseMiddleButton) && (ptrPt.Properties.IsMiddleButtonPressed)))) ||
                                 ((e.Pointer.PointerDeviceType == PointerDeviceType.Pen) && (DragButton == DragButtonType.Pen)) ||
                                 ((e.Pointer.PointerDeviceType == PointerDeviceType.Touch) && (DragButton == DragButtonType.Touch));

            if (!isValidPointer)
                return;

            // Get the location with respect to the parent
            var position = ptrPt.RawPosition;

            var fElem = AssociatedObject as FrameworkElement;
            if ((fElem != null) && (_parentFwPanel != null))
            {
                await _parentFwPanel.BeginFluidDragAsync(_parentLbItem ?? AssociatedObject, position, e.Pointer);
            }
        }

        /// <summary>
        /// Handler for Pointer Moved event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void OnPointerMoved(object sender, PointerRoutedEventArgs e)
        {
            var ptrPt = _parentLbItem != null ? e.GetCurrentPoint(_parentLbItem) : e.GetCurrentPoint(AssociatedObject);
            var isValidPointer = (((e.Pointer.PointerDeviceType == PointerDeviceType.Mouse)) &&
                                   (((DragButton == DragButtonType.MouseLeftButton) && (ptrPt.Properties.IsLeftButtonPressed)) ||
                                   ((DragButton == DragButtonType.MouseRightButton) && (ptrPt.Properties.IsRightButtonPressed)) ||
                                   ((DragButton == DragButtonType.MouseMiddleButton) && (ptrPt.Properties.IsMiddleButtonPressed)))) ||
                                 ((e.Pointer.PointerDeviceType == PointerDeviceType.Pen) && (DragButton == DragButtonType.Pen)) ||
                                 ((e.Pointer.PointerDeviceType == PointerDeviceType.Touch) && (DragButton == DragButtonType.Touch));

            if (!isValidPointer)
                return;

            // Get the location
            var position = ptrPt.RawPosition;

            var fElem = AssociatedObject as FrameworkElement;
            if ((fElem == null) || (_parentFwPanel == null))
                return;

            // Get the location with respect to the parent
            var positionInParent = e.GetCurrentPoint(_parentFwPanel).RawPosition;
            await _parentFwPanel.FluidDragAsync(_parentLbItem ?? AssociatedObject, position, positionInParent);
        }

        /// <summary>
        /// Handler for Pointer Released event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void OnPointerReleased(object sender, PointerRoutedEventArgs e)
        {
            var ptrPt = _parentLbItem != null ? e.GetCurrentPoint(_parentLbItem) : e.GetCurrentPoint(AssociatedObject);
            var isValidPointer = ((e.Pointer.PointerDeviceType == PointerDeviceType.Mouse) &&
                                   ((DragButton == DragButtonType.MouseLeftButton) || (DragButton == DragButtonType.MouseRightButton) ||
                                    (DragButton == DragButtonType.MouseMiddleButton))) ||
                                 ((e.Pointer.PointerDeviceType == PointerDeviceType.Pen) && (DragButton == DragButtonType.Pen)) ||
                                 ((e.Pointer.PointerDeviceType == PointerDeviceType.Touch) && (DragButton == DragButtonType.Touch));

            if (!isValidPointer)
                return;

            // Get the location
            var position = ptrPt.RawPosition;

            var fElem = AssociatedObject as FrameworkElement;
            if ((fElem == null) || (_parentFwPanel == null))
                return;

            // Get the location with respect to the parent
            var positionInParent = e.GetCurrentPoint(_parentFwPanel).RawPosition;
            await _parentFwPanel.EndFluidDragAsync(_parentLbItem ?? AssociatedObject, position, positionInParent, e.Pointer);
        }

        #endregion
    }
}
