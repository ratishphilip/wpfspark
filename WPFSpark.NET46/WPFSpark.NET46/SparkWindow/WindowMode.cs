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
// This file is part of the WPFSpark project: http://wpfspark.codeplex.com/
// 
// WPFSpark v1.3
//


namespace WPFSpark
{
    /// <summary>
    /// Defines the various modes a SparkWindow can be in.
    /// </summary>
    public enum WindowMode
    {
        // Fullscreen with Minimize and Close button
        Pane = 0,
        // Fullscreen with close button
        PaneCanClose = 1,
        // Non-Fullscreen, fixed-size with Close button
        CanClose = 2,
        // Non-Fullscreen, fixed-size with Minimize and Close button
        CanMinimize = 3,
        // Non-Fullscreen, fixed-size with Minimize, Maximize and Close button
        CanMaximize = 4,
        // Child Window (Non-Fullscreen, fixed-size) with no system buttons
        ChildWindow = 5
    }
}
