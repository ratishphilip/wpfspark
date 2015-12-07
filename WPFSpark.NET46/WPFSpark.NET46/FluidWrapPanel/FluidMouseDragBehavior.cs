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
// WPFSpark v1.2
// 

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interactivity;
using System.Windows.Media;

namespace WPFSpark
{
    /// <summary>
    /// Defines the Drag Behavior in the OldFluidWrapPanel using the Mouse
    /// </summary>
    public class FluidMouseDragBehavior : Behavior<UIElement>
    {
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
            DependencyProperty.Register("DragButton", typeof(MouseButton), typeof(FluidMouseDragBehavior),
                new FrameworkPropertyMetadata(MouseButton.Left));

        /// <summary>
        /// Gets or sets the DragButton property. This dependency property 
        /// indicates which Mouse button should participate in the drag interaction.
        /// </summary>
        public MouseButton DragButton
        {
            get { return (MouseButton)GetValue(DragButtonProperty); }
            set { SetValue(DragButtonProperty, value); }
        }

        #endregion

        #endregion

        #region Overrides

        /// <summary>
        /// 
        /// </summary>
        protected override void OnAttached()
        {
            if ((AssociatedObject as FrameworkElement) == null)
                return;
            // Subscribe to the Loaded event
            ((FrameworkElement)AssociatedObject).Loaded += OnAssociatedObjectLoaded;
        }

        private void OnAssociatedObjectLoaded(object sender, RoutedEventArgs e)
        {
            // Get the parent OldFluidWrapPanel and check if the AssociatedObject is
            // hosted inside a ListBoxItem (this scenario will occur if the OldFluidWrapPanel
            // is the ItemsPanel for a ListBox).
            GetParentPanel();

            // Subscribe to the Mouse down/move/up events
            if (_parentLbItem != null)
            {
                _parentLbItem.PreviewMouseDown += OnPreviewMouseDown;
                _parentLbItem.PreviewMouseMove += OnPreviewMouseMove;
                _parentLbItem.PreviewMouseUp   += OnPreviewMouseUp;
            }
            else
            {
                AssociatedObject.PreviewMouseDown += OnPreviewMouseDown;
                AssociatedObject.PreviewMouseMove += OnPreviewMouseMove;
                AssociatedObject.PreviewMouseUp   += OnPreviewMouseUp;
            }
        }

        /// <summary>
        /// Get the parent OldFluidWrapPanel and check if the AssociatedObject is
        /// hosted inside a ListBoxItem (this scenario will occur if the OldFluidWrapPanel
        /// is the ItemsPanel for a ListBox).
        /// </summary>
        private void GetParentPanel()
        {
            if ((AssociatedObject as FrameworkElement) == null)
                return;

            var ancestor = (FrameworkElement) AssociatedObject;

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

        protected override void OnDetaching()
        {
            if ((AssociatedObject as FrameworkElement) == null)
                return;

            ((FrameworkElement) AssociatedObject).Loaded -= OnAssociatedObjectLoaded;
            if (_parentLbItem != null)
            {
                _parentLbItem.PreviewMouseDown -= OnPreviewMouseDown;
                _parentLbItem.PreviewMouseMove -= OnPreviewMouseMove;
                _parentLbItem.PreviewMouseUp -= OnPreviewMouseUp;
            }
            else
            {
                AssociatedObject.PreviewMouseDown -= OnPreviewMouseDown;
                AssociatedObject.PreviewMouseMove -= OnPreviewMouseMove;
                AssociatedObject.PreviewMouseUp -= OnPreviewMouseUp;
            }
        }

        #endregion

        #region Event Handlers

        async void OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton != DragButton)
                return;

            var position = _parentLbItem != null ? e.GetPosition(_parentLbItem) : e.GetPosition(AssociatedObject);

            var fElem = AssociatedObject as FrameworkElement;
            if ((fElem != null) && (_parentFwPanel != null))
            {
                await _parentFwPanel.BeginFluidDragAsync(_parentLbItem ?? AssociatedObject, position);
            }
        }

        private async void OnPreviewMouseMove(object sender, MouseEventArgs e)
        {
            var isDragging = false;

            switch (DragButton)
            {
                case MouseButton.Left:
                    if (e.LeftButton == MouseButtonState.Pressed)
                    {
                        isDragging = true;
                    }
                    break;
                case MouseButton.Middle:
                    if (e.MiddleButton == MouseButtonState.Pressed)
                    {
                        isDragging = true;
                    }
                    break;
                case MouseButton.Right:
                    if (e.RightButton == MouseButtonState.Pressed)
                    {
                        isDragging = true;
                    }
                    break;
                case MouseButton.XButton1:
                    if (e.XButton1 == MouseButtonState.Pressed)
                    {
                        isDragging = true;
                    }
                    break;
                case MouseButton.XButton2:
                    if (e.XButton2 == MouseButtonState.Pressed)
                    {
                        isDragging = true;
                    }
                    break;
            }

            if (!isDragging)
                return;

            var position = _parentLbItem != null ? e.GetPosition(_parentLbItem) : e.GetPosition(AssociatedObject);

            var fElem = AssociatedObject as FrameworkElement;
            if ((fElem == null) || (_parentFwPanel == null))
                return;

            var positionInParent = e.GetPosition(_parentFwPanel);
            await _parentFwPanel.FluidDragAsync(_parentLbItem ?? AssociatedObject, position, positionInParent);
        }

        private async void OnPreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton != DragButton)
                return;

            var position = _parentLbItem != null ? e.GetPosition(_parentLbItem) : e.GetPosition(AssociatedObject);

            var fElem = AssociatedObject as FrameworkElement;
            if ((fElem == null) || (_parentFwPanel == null))
                return;

            var positionInParent = e.GetPosition(_parentFwPanel);
            await _parentFwPanel.EndFluidDragAsync(_parentLbItem ?? AssociatedObject, position, positionInParent);
        }

        #endregion
    }
}
