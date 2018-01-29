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
using System.Windows.Controls;

namespace WPFSpark
{
    /// <summary>
    /// Class which encapsulates the header and content
    /// for each Pivot item.
    /// </summary>
    public class PivotItem : ContentControl
    {
        #region Fields

        PivotPanel parent = null;

        #endregion

        #region Dependency Properties

        #region PivotHeader

        /// <summary>
        /// PivotHeader Dependency Property
        /// </summary>
        public static readonly DependencyProperty PivotHeaderProperty =
            DependencyProperty.Register("PivotHeader", typeof(FrameworkElement), typeof(PivotItem),
                new FrameworkPropertyMetadata(OnPivotHeaderChanged));

        /// <summary>
        /// Gets or sets the PivotHeader property. This dependency property 
        /// indicates the header for the PivotItem.
        /// </summary>
        public FrameworkElement PivotHeader
        {
            get { return (FrameworkElement)GetValue(PivotHeaderProperty); }
            set { SetValue(PivotHeaderProperty, value); }
        }

        /// <summary>
        /// Handles changes to the PivotHeader property.
        /// </summary>
        /// <param name="d">PivotItem</param>
        /// <param name="e">DependencyProperty changed event arguments</param>
        private static void OnPivotHeaderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var item = (PivotItem)d;
            var oldPivotHeader = (FrameworkElement)e.OldValue;
            var newPivotHeader = item.PivotHeader;
            item.OnPivotHeaderChanged(oldPivotHeader, newPivotHeader);
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the PivotHeader property.
        /// </summary>
        /// <param name="oldPivotHeader">Old Value</param>
        /// <param name="newPivotHeader">New Value</param>
        protected void OnPivotHeaderChanged(FrameworkElement oldPivotHeader, FrameworkElement newPivotHeader)
        {
            parent?.UpdatePivotItemHeader(this);
            (newPivotHeader as IPivotHeader)?.SetActive(false);
        }

        #endregion

        #region PivotContent

        /// <summary>
        /// PivotContent Dependency Property
        /// </summary>
        public static readonly DependencyProperty PivotContentProperty =
            DependencyProperty.Register("PivotContent", typeof(FrameworkElement), typeof(PivotItem),
                new FrameworkPropertyMetadata(OnPivotContentChanged));

        /// <summary>
        /// Gets or sets the PivotContent property. This dependency property 
        /// indicates the content of the PivotItem.
        /// </summary>
        public FrameworkElement PivotContent
        {
            get { return (FrameworkElement)GetValue(PivotContentProperty); }
            set { SetValue(PivotContentProperty, value); }
        }

        /// <summary>
        /// Handles changes to the PivotContent property.
        /// </summary>
        /// <param name="d">PivotItem</param>
        /// <param name="e">DependencyProperty changed event arguments</param>
        private static void OnPivotContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var item = (PivotItem)d;
            var oldPivotContent = (FrameworkElement)e.OldValue;
            var newPivotContent = item.PivotContent;
            item.OnPivotContentChanged(oldPivotContent, newPivotContent);
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the PivotContent property.
        /// </summary>
        /// <param name="oldPivotContent">Old Value</param>
        /// <param name="newPivotContent">New Value</param>
        protected void OnPivotContentChanged(FrameworkElement oldPivotContent, FrameworkElement newPivotContent)
        {
            if (newPivotContent == null)
                return;

            parent?.UpdatePivotItemContent(this);
            newPivotContent.Visibility = Visibility.Collapsed;
        }

        #endregion

        #endregion

        #region APIs

        /// <summary>
        /// Sets the parent PivotPanel of the Pivot Item
        /// </summary>
        /// <param name="panel">PivotPanel</param>
        public void SetParent(PivotPanel panel)
        {
            parent = panel;
        }

        /// <summary>
        /// Activates/Deactivates the Pivot Header and Pivot Content
        /// based on the 'isActive' flag.
        /// </summary>
        /// <param name="isActive">Flag to indicate whether the Pivot Header and Pivot Content should be Activated or Decativated</param>
        public void SetActive(bool isActive)
        {
            (PivotHeader as IPivotHeader)?.SetActive(isActive);

            if (PivotContent == null)
                return;

            var content = PivotContent as IPivotContent;
            if (content != null)
                content.SetActive(isActive);
            else
                PivotContent.Visibility = isActive ? Visibility.Visible : Visibility.Collapsed;
        }

        /// <summary>
        /// Initializes the PivotItem
        /// </summary>
        public void Initialize()
        {
            // Set the header as inactive
            (PivotHeader as IPivotHeader)?.SetActive(false);

            // Make the PivotContent invisible
            if (PivotContent != null)
            {
                PivotContent.Visibility = Visibility.Collapsed;
            }
        }

        #endregion
    }
}
