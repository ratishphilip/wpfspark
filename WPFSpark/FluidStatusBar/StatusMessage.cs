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

namespace WPFSpark
{
    /// <summary>
    /// Class encapsulating the status message that has to be
    /// displayed in the FluidStatusBar
    /// </summary>
    public class StatusMessage
    {
        #region Properties

        /// <summary>
        /// The message to be displayed in the FluidStatusBar
        /// </summary>
        public string Message { get; set; }
        /// <summary>
        /// Flag to indicate whether to animate the fade out of
        /// the currently displayed message before showing the 
        /// new message.
        /// </summary>
        public bool IsAnimated { get; set; }

        #endregion

        #region Construction / Initialization

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="message">The message to be displayed in the FluidStatusBar</param>
        /// <param name="isAnimated">Flag to indicate whether to animate the fade out of 
        /// the currently displayed message before showing the new message.</param>
        public StatusMessage(string message, bool isAnimated = false)
        {
            Message = message;
            IsAnimated = isAnimated;
        }

        public bool IsEmpty()
        {
            return String.IsNullOrWhiteSpace(Message);
        }

        public void Update(string message, bool isAnimated = false)
        {
            Message = message;
            IsAnimated = isAnimated;
        }

        public static StatusMessage Empty => new StatusMessage(string.Empty, false);

        #endregion
    }
}
