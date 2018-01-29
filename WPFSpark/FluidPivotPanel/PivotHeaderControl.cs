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

using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace WPFSpark
{
    /// <summary>
    /// Class which implements the IPivotHeader interface
    /// and represents the header item in text form.
    /// </summary>
    public class PivotHeaderControl : ContentControl, IPivotHeader
    {
        #region Dependency Properties

        #region ActiveForeground

        /// <summary>
        /// ActiveForeground Dependency Property
        /// </summary>
        public static readonly DependencyProperty ActiveForegroundProperty =
            DependencyProperty.Register("ActiveForeground", typeof(Brush), typeof(PivotHeaderControl),
                new FrameworkPropertyMetadata(Brushes.Black, OnActiveForegroundChanged));

        /// <summary>
        /// Gets or sets the ActiveForeground property. This dependency property 
        /// indicates the foreground color of the Header Item when it is active.
        /// </summary>
        public Brush ActiveForeground
        {
            get { return (Brush)GetValue(ActiveForegroundProperty); }
            set { SetValue(ActiveForegroundProperty, value); }
        }

        /// <summary>
        /// Handles changes to the ActiveForeground property.
        /// </summary>
        /// <param name="d">PivotHeaderControl</param>
        /// <param name="e">DependencyProperty changed event arguments</param>
        private static void OnActiveForegroundChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var header = (PivotHeaderControl)d;
            var oldActiveForeground = (Brush)e.OldValue;
            var newActiveForeground = header.ActiveForeground;
            header.OnActiveForegroundChanged(oldActiveForeground, newActiveForeground);
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the ActiveForeground property.
        /// </summary>
        /// <param name="oldActiveForeground">Old Value</param>
        /// <param name="newActiveForeground">New Value</param>
        protected void OnActiveForegroundChanged(Brush oldActiveForeground, Brush newActiveForeground)
        {
            if (IsActive)
            {
                Foreground = newActiveForeground;
            }
        }

        #endregion

        #region InactiveForeground

        /// <summary>
        /// InactiveForeground Dependency Property
        /// </summary>
        public static readonly DependencyProperty InactiveForegroundProperty =
            DependencyProperty.Register("InactiveForeground", typeof(Brush), typeof(PivotHeaderControl),
                new FrameworkPropertyMetadata(Brushes.DarkGray, OnInactiveForegroundChanged));

        /// <summary>
        /// Gets or sets the InactiveForeground property. This dependency property 
        /// indicates the foreground color when the Header Item is inactive.
        /// </summary>
        public Brush InactiveForeground
        {
            get { return (Brush)GetValue(InactiveForegroundProperty); }
            set { SetValue(InactiveForegroundProperty, value); }
        }

        /// <summary>
        /// Handles changes to the InactiveForeground property.
        /// </summary>
        /// <param name="d">PivotHeaderControl</param>
        /// <param name="e">DependencyProperty changed event arguments</param>
        private static void OnInactiveForegroundChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var header = (PivotHeaderControl)d;
            var oldInactiveForeground = (Brush)e.OldValue;
            var newInactiveForeground = header.InactiveForeground;
            header.OnInactiveForegroundChanged(oldInactiveForeground, newInactiveForeground);
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the InactiveForeground property.
        /// </summary>
        /// <param name="oldInactiveForeground">Old Value</param>
        /// <param name="newInactiveForeground">New Value</param>
        protected void OnInactiveForegroundChanged(Brush oldInactiveForeground, Brush newInactiveForeground)
        {
            if (!IsActive)
            {
                Foreground = newInactiveForeground;
            }
        }

        #endregion

        #region DisabledForeground

        /// <summary>
        /// DisabledForeground Dependency Property
        /// </summary>
        public static readonly DependencyProperty DisabledForegroundProperty =
            DependencyProperty.Register("DisabledForeground", typeof(Brush), typeof(PivotHeaderControl),
                new FrameworkPropertyMetadata(Brushes.Black));

        /// <summary>
        /// Gets or sets the DisabledForeground property. This dependency property 
        /// indicates the foreground color when the HeaderControl is disabled.
        /// </summary>
        public Brush DisabledForeground
        {
            get { return (Brush)GetValue(DisabledForegroundProperty); }
            set { SetValue(DisabledForegroundProperty, value); }
        }

        #endregion
        
        #region IsActive

        /// <summary>
        /// IsActive Dependency Property
        /// </summary>
        public static readonly DependencyProperty IsActiveProperty =
            DependencyProperty.Register("IsActive", typeof(bool), typeof(PivotHeaderControl),
                new FrameworkPropertyMetadata(false, OnIsActiveChanged));

        /// <summary>
        /// Gets or sets the IsActive property. This dependency property 
        /// indicates whether the Header Item is currently active.
        /// </summary>
        public bool IsActive
        {
            get { return (bool)GetValue(IsActiveProperty); }
            set { SetValue(IsActiveProperty, value); }
        }

        /// <summary>
        /// Handles changes to the IsActive property.
        /// </summary>
        /// <param name="d">PivotHeaderControl</param>
        /// <param name="e">DependencyProperty changed event arguments</param>
        private static void OnIsActiveChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var header = (PivotHeaderControl)d;
            var oldIsActive = (bool)e.OldValue;
            var newIsActive = header.IsActive;
            header.OnIsActiveChanged(oldIsActive, newIsActive);
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the IsActive property.
        /// </summary>
        /// <param name="oldIsActive">Old Value</param>
        /// <param name="newIsActive">New Value</param>
        protected void OnIsActiveChanged(bool oldIsActive, bool newIsActive)
        {
            Foreground = IsEnabled ? (newIsActive ? ActiveForeground : InactiveForeground) : DisabledForeground;
        }

        #endregion

        #endregion

        #region Construction / Initialization

        /// <summary>
        /// Ctor
        /// </summary>
        public PivotHeaderControl()
        {
            // By default, the header will be inactive
            IsActive = false;
            Foreground = InactiveForeground;
            // This control will raise the HeaderItemSelected event on Mouse Left Button down
            MouseLeftButtonDown += OnMouseDown;
            // Keep track of the change in IsEnabled property
            var dpd = DependencyPropertyDescriptor.FromProperty(IsEnabledProperty,
                typeof(PivotHeaderControl));
            dpd.AddValueChanged(this, (s, e) =>
            {
                Foreground = DisabledForeground;
            });
        }

        #endregion

        #region IPivotHeader Members

        /// <summary>
        /// Activates/Deactivates the Pivot Header based on the 'isActive' flag.
        /// </summary>
        /// <param name="isActive">Flag to indicate whether the Pivot Header and Pivot Content should be Activated or Deactivated</param>
        public void SetActive(bool isActive)
        {
            IsActive = isActive;
        }

        public void SelectHeader()
        {
            if (IsEnabled)
            {
                HeaderItemSelected?.Invoke(this, new EventArgs());
            }
        }

        public event EventHandler HeaderItemSelected;

        #endregion

        #region EventHandlers

        /// <summary>
        /// Handler for the mouse down event
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event Args</param>
        void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            SelectHeader();
        }

        #endregion
    }
}
