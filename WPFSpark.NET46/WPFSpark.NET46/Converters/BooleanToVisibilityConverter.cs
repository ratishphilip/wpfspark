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
// WPFSpark v1.3
// 

using System;
using System.Windows;
using System.Windows.Data;

namespace WPFSpark
{
    /// <summary>
    /// Value converter that translates true to <see cref="Visibility.Visible"/> and false to
    /// <see cref="Visibility.Collapsed"/> and vice-versa, when IsReverse is false. When IsReverse is true then
    /// it translates true to <see cref="Visibility.Collapsed"/> and false to
    /// <see cref="Visibility.Visible"/> and vice=versa.
    /// NOTE: IsReverse is false by default.
    /// </summary>
    public sealed class BooleanToVisibilityConverter : IValueConverter
    {
        public bool IsReverse { get; set; }

        public BooleanToVisibilityConverter()
        {
            IsReverse = false;
        }

        #region IValueConverter Members

        /// <summary>
        /// Converts boolean? to Visibility
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var result = Visibility.Collapsed;

            if (value is bool)
            {
                result = (IsReverse ^ (bool)value) ? Visibility.Visible : Visibility.Collapsed;
            }

            return result;
        }

        /// <summary>
        /// Converts Visibility to boolean?
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var result = false;

            if (value is Visibility)
            {
                result = IsReverse ^ ((Visibility)value == Visibility.Visible);
            }

            return result;
        }

        #endregion
    }
}
