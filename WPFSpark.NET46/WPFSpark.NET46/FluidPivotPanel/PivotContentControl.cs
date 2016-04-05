// Copyright (c) 2016 Ratish Philip 
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
// WPFSpark v1.2.1
// 

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace WPFSpark
{
    /// <summary>
    /// Implementation of the IPivotContent interface
    /// </summary>
    public class PivotContentControl : ContentControl, IPivotContent
    {
        #region Fields

        Storyboard _fadeInSb;

        #endregion

        #region Dependency Properties

        #region AnimateContent

        /// <summary>
        /// AnimateContent Dependency Property
        /// </summary>
        public static readonly DependencyProperty AnimateContentProperty =
            DependencyProperty.Register("AnimateContent", typeof(bool), typeof(PivotContentControl),
                new FrameworkPropertyMetadata(true));

        /// <summary>
        /// Gets or sets the AnimateContent property. This dependency property 
        /// indicates whether the content should be animated upon activation.
        /// </summary>
        public bool AnimateContent
        {
            get { return (bool)GetValue(AnimateContentProperty); }
            set { SetValue(AnimateContentProperty, value); }
        }

        #endregion 

        #endregion

        #region Construction / Initialization

        public PivotContentControl()
        {
            var slideInAnimation = new ThicknessAnimation
            {
                From = new Thickness(200, 0, 0, 0),
                To = new Thickness(0, 0, 0, 0),
                Duration = new Duration(TimeSpan.FromSeconds(0.3))
            };
            Storyboard.SetTargetProperty(slideInAnimation, new PropertyPath(FrameworkElement.MarginProperty));
            Storyboard.SetTarget(slideInAnimation, this);

            slideInAnimation.EasingFunction = new CubicEase
                            {
                                EasingMode = EasingMode.EaseOut
                            };

            _fadeInSb = new Storyboard();
            _fadeInSb.Children.Add(slideInAnimation);
        }

        #endregion

        #region IPivotContent Members

        public void SetActive(bool isActive)
        {
            if (isActive)
            {
                Visibility = Visibility.Visible;
                if (AnimateContent)
                    _fadeInSb.Begin();
            }
            else
            {
                Visibility = Visibility.Collapsed;
            }
        }

        #endregion
    }
}
