// Copyright (c) 2017 Ratish Philip 
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
// WPFSpark v1.3.1
// 

using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Xaml.Behaviors;

namespace WPFSpark
{
    /// <summary>
    /// Defines the Drag Behavior in the OldFluidWrapPanel using the Mouse
    /// </summary>
    public class FluidMouseDragBehavior : Behavior<UIElement>
    {
        #region Fields

        FluidWrapPanel _parentFwPanel;
        UIElement _fwPanelChild;

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
            if (_fwPanelChild != null)
            {
                _fwPanelChild.PreviewMouseDown += OnPreviewMouseDown;
                _fwPanelChild.PreviewMouseMove += OnPreviewMouseMove;
                _fwPanelChild.PreviewMouseUp   += OnPreviewMouseUp;
            }
        }

        /// <summary>
        /// Get the parent OldFluidWrapPanel and check if the AssociatedObject is
        /// hosted inside a ListBoxItem (this scenario will occur if the OldFluidWrapPanel
        /// is the ItemsPanel for a ListBox).
        /// </summary>
        private void GetParentPanel()
        {
            _fwPanelChild = AssociatedObject;

            while (_fwPanelChild != null)
            {
                // Get the visual parent of the current item
                var parent = VisualTreeHelper.GetParent(_fwPanelChild) as UIElement;

                if (parent == null)
                {
                    _fwPanelChild = null;
                }
                else if (parent is FluidWrapPanel)
                {
                    _parentFwPanel = (FluidWrapPanel) parent;
                    // Search finished
                    return;
                }
                else
                {
                    _fwPanelChild = parent;
                }
            }
        }

        protected override void OnDetaching()
        {
            if ((AssociatedObject as FrameworkElement) == null)
                return;

            ((FrameworkElement) AssociatedObject).Loaded -= OnAssociatedObjectLoaded;
            if (_fwPanelChild != null)
            {
                _fwPanelChild.PreviewMouseDown -= OnPreviewMouseDown;
                _fwPanelChild.PreviewMouseMove -= OnPreviewMouseMove;
                _fwPanelChild.PreviewMouseUp -= OnPreviewMouseUp;
            }
        }

        #endregion

        #region Event Handlers

        async void OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton != DragButton)
                return;

            var position = e.GetPosition(_fwPanelChild);
            await _parentFwPanel.BeginFluidDragAsync(_fwPanelChild, position);
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

            var position = e.GetPosition(_fwPanelChild);
            var positionInParent = e.GetPosition(_parentFwPanel);
            await _parentFwPanel.FluidDragAsync(_fwPanelChild, position, positionInParent);
        }

        private async void OnPreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton != DragButton)
                return;

            var position = e.GetPosition(_fwPanelChild);
            var positionInParent = e.GetPosition(_parentFwPanel);
            await _parentFwPanel.EndFluidDragAsync(_fwPanelChild, position, positionInParent);
        }

        #endregion
    }
}
