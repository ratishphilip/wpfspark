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

// -------------------------------------------------------------------------------
// 
// This file is part of the WPFSpark project: http://wpfspark.codeplex.com/
// 
// WPFSpark v1.3.1
//
// -------------------------------------------------------------------------------

using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Shapes;

namespace WPFSpark
{
    /// <summary>
    /// Class which provides the implementation of a custom window
    /// </summary>
    [TemplatePart(Name = "PART_Minimize", Type = typeof(Button))]
    [TemplatePart(Name = "PART_Restore", Type = typeof(Button))]
    [TemplatePart(Name = "PART_Maximize", Type = typeof(Button))]
    [TemplatePart(Name = "PART_Close", Type = typeof(Button))]
    [TemplatePart(Name = "PART_TitleBar", Type = typeof(Border))]
    [TemplatePart(Name = "PART_Resize", Type = typeof(Grid))]
    public class SparkWindow : Window
    {
        #region Enums

        private enum ResizeDirection
        {
            Left = 1,
            Right = 2,
            Top = 3,
            TopLeft = 4,
            TopRight = 5,
            Bottom = 6,
            BottomLeft = 7,
            BottomRight = 8,
        }

        #endregion

        #region Fields

        private Button _minimizeButton;
        private Button _restoreButton;
        private Button _maximizeButton;
        private Button _closeButton;
        private Border _titleBar;
        private HwndSource _hwndSource;
        private Grid _resizeGrid;
        private readonly Cursor _defaultCursor;

        #endregion

        #region Dependency Properties

        #region TitleImage

        /// <summary>
        /// TitleImage Dependency Property
        /// </summary>
        public static readonly DependencyProperty TitleImageProperty =
            DependencyProperty.Register("TitleImage", typeof(UIElement), typeof(SparkWindow),
                new PropertyMetadata(null, OnTitleImageChanged));

        /// <summary>
        /// Gets or sets the TitleImage property. This dependency property 
        /// indicates the image content shown in the title bar.
        /// </summary>
        public UIElement TitleImage
        {
            get { return (UIElement)GetValue(TitleImageProperty); }
            set { SetValue(TitleImageProperty, value); }
        }

        /// <summary>
        /// Handles changes to the TitleImage property.
        /// </summary>
        /// <param name="d">SparkWindow</param>
        /// <param name="e">DependencyProperty changed event arguments</param>
        private static void OnTitleImageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SparkWindow win = (SparkWindow)d;
            UIElement oldTitleImage = (UIElement)e.OldValue;
            UIElement newTitleImage = win.TitleImage;
            win.OnTitleImageChanged(oldTitleImage, newTitleImage);
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the TitleImage property.
        /// </summary>
        /// <param name="oldTitleImage">Old Value</param>
        /// <param name="newTitleImage">New Value</param>
        void OnTitleImageChanged(UIElement oldTitleImage, UIElement newTitleImage)
        {

        }

        #endregion

        #region TitleImageMargin

        /// <summary>
        /// TitleImageMargin Dependency Property
        /// </summary>
        public static readonly DependencyProperty TitleImageMarginProperty =
            DependencyProperty.Register("TitleImageMargin", typeof(Thickness), typeof(SparkWindow),
                new PropertyMetadata(new Thickness(), OnTitleImageMarginChanged));

        /// <summary>
        /// Gets or sets the TitleImageMargin property. This dependency property 
        /// indicates the margin of the image shown in the title bar.
        /// </summary>
        public Thickness TitleImageMargin
        {
            get { return (Thickness)GetValue(TitleImageMarginProperty); }
            set { SetValue(TitleImageMarginProperty, value); }
        }

        /// <summary>
        /// Handles changes to the TitleImageMargin property.
        /// </summary>
        /// <param name="d">SparkWindow</param>
		/// <param name="e">DependencyProperty changed event arguments</param>
        private static void OnTitleImageMarginChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var win = (SparkWindow)d;
            var oldTitleImageMargin = (Thickness)e.OldValue;
            var newTitleImageMargin = win.TitleImageMargin;
            win.OnTitleImageMarginChanged(oldTitleImageMargin, newTitleImageMargin);
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the TitleImageMargin property.
        /// </summary>
		/// <param name="oldTitleImageMargin">Old Value</param>
		/// <param name="newTitleImageMargin">New Value</param>
        void OnTitleImageMarginChanged(Thickness oldTitleImageMargin, Thickness newTitleImageMargin)
        {

        }

        #endregion

        #region TitleMargin

        /// <summary>
        /// TitleMargin Dependency Property
        /// </summary>
        public static readonly DependencyProperty TitleMarginProperty =
            DependencyProperty.Register("TitleMargin", typeof(Thickness), typeof(SparkWindow),
                new FrameworkPropertyMetadata(new Thickness(4, 0, 0, 0), OnTitleMarginChanged));

        /// <summary>
        /// Gets or sets the TitleMargin property. This dependency property 
        /// indicates the margin of the window title in the title bar.
        /// </summary>
        public Thickness TitleMargin
        {
            get { return (Thickness)GetValue(TitleMarginProperty); }
            set { SetValue(TitleMarginProperty, value); }
        }

        /// <summary>
        /// Handles changes to the TitleMargin property.
        /// </summary>
        /// <param name="d">SparkWindow</param>
        /// <param name="e">DependencyProperty changed event arguments</param>
        private static void OnTitleMarginChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SparkWindow win = (SparkWindow)d;
            Thickness oldTitleMargin = (Thickness)e.OldValue;
            Thickness newTitleMargin = win.TitleMargin;
            win.OnTitleMarginChanged(oldTitleMargin, newTitleMargin);
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the TitleMargin property.
        /// </summary>
        /// <param name="oldTitleMargin">Old Value</param>
        /// <param name="newTitleMargin">New Value</param>
        protected virtual void OnTitleMarginChanged(Thickness oldTitleMargin, Thickness newTitleMargin)
        {
            //TextBlock tb = GetChildControl<TextBlock>("PART_TitleText");

            //UpdateTriggerMargin(tb, newTitleMargin);
        }

        #endregion

        #region TitleEffect

        /// <summary>
        /// TitleEffect Dependency Property
        /// </summary>
        public static readonly DependencyProperty TitleEffectProperty =
            DependencyProperty.Register("TitleEffect", typeof(Effect), typeof(SparkWindow), 
                new FrameworkPropertyMetadata(OnTitleEffectChanged));

        /// <summary>
        /// Gets or sets the TitleEffect property. This dependency property 
        /// indicates the Effect to be applied on the Title TextBlock.
        /// </summary>
        public Effect TitleEffect
        {
            get { return (Effect)GetValue(TitleEffectProperty); }
            set { SetValue(TitleEffectProperty, value); }
        }

        /// <summary>
        /// Handles changes to the TitleEffect property.
        /// </summary>
        /// <param name="d">SparkWindow</param>
        /// <param name="e">DependencyProperty changed event arguments</param>
        private static void OnTitleEffectChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SparkWindow win = (SparkWindow)d;
            Effect oldTitleEffect = (Effect)e.OldValue;
            Effect newTitleEffect = win.TitleEffect;
            win.OnTitleEffectChanged(oldTitleEffect, newTitleEffect);
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the TitleEffect property.
        /// </summary>
        /// <param name="oldTitleEffect">Old Value</param>
        /// <param name="newTitleEffect">New Value</param>
        protected void OnTitleEffectChanged(Effect oldTitleEffect, Effect newTitleEffect)
        {

        }

        #endregion

        #region TitleBackground

        /// <summary>
        /// TitleBackground Dependency Property
        /// </summary>
        public static readonly DependencyProperty TitleBackgroundProperty =
            DependencyProperty.Register("TitleBackground", typeof(Brush), typeof(SparkWindow),
                new PropertyMetadata(Brushes.Transparent, OnTitleBackgroundChanged));

        /// <summary>
        /// Gets or sets the TitleBackground property. This dependency property 
        /// indicates the background of the title.
        /// </summary>
        public Brush TitleBackground
        {
            get { return (Brush)GetValue(TitleBackgroundProperty); }
            set { SetValue(TitleBackgroundProperty, value); }
        }

        /// <summary>
        /// Handles changes to the TitleBackground property.
        /// </summary>
        /// <param name="d">SparkWindow</param>
        /// <param name="e">DependencyProperty changed event arguments</param>
        private static void OnTitleBackgroundChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SparkWindow win = (SparkWindow)d;
            Brush oldTitleBackground = (Brush)e.OldValue;
            Brush newTitleBackground = win.TitleBackground;
            win.OnTitleBackgroundChanged(oldTitleBackground, newTitleBackground);
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the TitleBackground property.
        /// </summary>
        /// <param name="oldTitleBackground">Old Value</param>
        /// <param name="newTitleBackground">New Value</param>
        void OnTitleBackgroundChanged(Brush oldTitleBackground, Brush newTitleBackground)
        {

        }

        #endregion

        #region WindowFrameMode

        /// <summary>
        /// WindowFrameMode Dependency Property
        /// </summary>
        public static readonly DependencyProperty WindowFrameModeProperty =
            DependencyProperty.Register("WindowFrameMode", typeof(WindowMode), typeof(SparkWindow),
                new FrameworkPropertyMetadata(WindowMode.Pane, OnWindowFrameModeChanged));

        /// <summary>
        /// Gets or sets the WindowFrameMode property. This dependency property 
        /// indicates the mode of the window frame.
        /// </summary>
        public WindowMode WindowFrameMode
        {
            get { return (WindowMode)GetValue(WindowFrameModeProperty); }
            set { SetValue(WindowFrameModeProperty, value); }
        }

        /// <summary>
        /// Handles changes to the WindowFrameMode property.
        /// </summary>
        /// <param name="d">SparkWindow</param>
        /// <param name="e">DependencyProperty changed event arguments</param>
        private static void OnWindowFrameModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SparkWindow win = (SparkWindow)d;
            WindowMode oldWindowFrameMode = (WindowMode)e.OldValue;
            WindowMode newWindowFrameMode = win.WindowFrameMode;
            win.OnWindowFrameModeChanged(oldWindowFrameMode, newWindowFrameMode);
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the WindowFrameMode property.
        /// </summary>
        /// <param name="oldWindowFrameMode">Old Value</param>
        /// <param name="newWindowFrameMode">New Value</param>
        protected void OnWindowFrameModeChanged(WindowMode oldWindowFrameMode, WindowMode newWindowFrameMode)
        {
            UpdateWindowFrame(newWindowFrameMode);
        }

        #endregion

        #region SystemBorderBrush

        /// <summary>
        /// SystemBorderBrush Dependency Property
        /// </summary>
        public static readonly DependencyProperty SystemBorderBrushProperty =
            DependencyProperty.Register("SystemBorderBrush", typeof(Brush), typeof(SparkWindow),
                new PropertyMetadata(Brushes.White, OnSystemBorderBrushChanged));

        /// <summary>
        /// Gets or sets the SystemBorderBrush property. This dependency property 
        /// indicates the color of the border of the system buttons.
        /// </summary>
        public Brush SystemBorderBrush
        {
            get { return (Brush)GetValue(SystemBorderBrushProperty); }
            set { SetValue(SystemBorderBrushProperty, value); }
        }

        /// <summary>
        /// Handles changes to the SystemBorderBrush property.
        /// </summary>
        /// <param name="d">SparkWindow</param>
        /// <param name="e">DependencyProperty changed event arguments</param>
        private static void OnSystemBorderBrushChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SparkWindow win = (SparkWindow)d;
            Brush oldSystemBorderBrush = (Brush)e.OldValue;
            Brush newSystemBorderBrush = win.SystemBorderBrush;
            win.OnSystemBorderBrushChanged(oldSystemBorderBrush, newSystemBorderBrush);
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the SystemBorderBrush property.
        /// </summary>
        /// <param name="oldSystemBorderBrush">Old Value</param>
        /// <param name="newSystemBorderBrush">New Value</param>
        void OnSystemBorderBrushChanged(Brush oldSystemBorderBrush, Brush newSystemBorderBrush)
        {

        }

        #endregion

        #region SystemBorderThickness

        /// <summary>
        /// SystemBorderThickness Dependency Property
        /// </summary>
        public static readonly DependencyProperty SystemBorderThicknessProperty =
            DependencyProperty.Register("SystemBorderThickness", typeof(Thickness), typeof(SparkWindow),
                new PropertyMetadata(new Thickness(0.5), OnSystemBorderThicknessChanged));

        /// <summary>
        /// Gets or sets the SystemBorderThickness property. This dependency property 
        /// indicates the thickness of the border of the System buttons.
        /// </summary>
        public Thickness SystemBorderThickness
        {
            get { return (Thickness)GetValue(SystemBorderThicknessProperty); }
            set { SetValue(SystemBorderThicknessProperty, value); }
        }

        /// <summary>
        /// Handles changes to the SystemBorderThickness property.
        /// </summary>
        /// <param name="d">SparkWindow</param>
        /// <param name="e">DependencyProperty changed event arguments</param>
        private static void OnSystemBorderThicknessChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SparkWindow win = (SparkWindow)d;
            Thickness oldSystemBorderThickness = (Thickness)e.OldValue;
            Thickness newSystemBorderThickness = win.SystemBorderThickness;
            win.OnSystemBorderThicknessChanged(oldSystemBorderThickness, newSystemBorderThickness);
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the SystemBorderThickness property.
        /// </summary>
        /// <param name="oldSystemBorderThickness">Old Value</param>
        /// <param name="newSystemBorderThickness">New Value</param>
        void OnSystemBorderThicknessChanged(Thickness oldSystemBorderThickness, Thickness newSystemBorderThickness)
        {

        }

        #endregion

        #region SystemForeground

        /// <summary>
        /// SystemForeground Dependency Property
        /// </summary>
        public static readonly DependencyProperty SystemForegroundProperty =
            DependencyProperty.Register("SystemForeground", typeof(Brush), typeof(SparkWindow),
                new PropertyMetadata(Brushes.White, OnSystemForegroundChanged));

        /// <summary>
        /// Gets or sets the SystemForeground property. This dependency property 
        /// indicates the foreground of the content of the system buttons.
        /// </summary>
        public Brush SystemForeground
        {
            get { return (Brush)GetValue(SystemForegroundProperty); }
            set { SetValue(SystemForegroundProperty, value); }
        }

        /// <summary>
        /// Handles changes to the SystemForeground property.
        /// </summary>
        /// <param name="d">SparkWindow</param>
        /// <param name="e">DependencyProperty changed event arguments</param>
        private static void OnSystemForegroundChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SparkWindow win = (SparkWindow)d;
            Brush oldSystemForeground = (Brush)e.OldValue;
            Brush newSystemForeground = win.SystemForeground;
            win.OnSystemForegroundChanged(oldSystemForeground, newSystemForeground);
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the SystemForeground property.
        /// </summary>
        /// <param name="oldSystemForeground">Old Value</param>
        /// <param name="newSystemForeground">New Value</param>
        void OnSystemForegroundChanged(Brush oldSystemForeground, Brush newSystemForeground)
        {

        }

        #endregion

        #region SystemBackground

        /// <summary>
        /// SystemBackground Dependency Property
        /// </summary>
        public static readonly DependencyProperty SystemBackgroundProperty =
            DependencyProperty.Register("SystemBackground", typeof(Brush), typeof(SparkWindow),
                new PropertyMetadata(Brushes.White, OnSystemBackgroundChanged));

        /// <summary>
        /// Gets or sets the SystemBackground property. This dependency property 
        /// indicates the background of the content of the system buttons.
        /// </summary>
        public Brush SystemBackground
        {
            get { return (Brush)GetValue(SystemBackgroundProperty); }
            set { SetValue(SystemBackgroundProperty, value); }
        }

        /// <summary>
        /// Handles changes to the SystemBackground property.
        /// </summary>
        /// <param name="d">SparkWindow</param>
        /// <param name="e">DependencyProperty changed event arguments</param>
        private static void OnSystemBackgroundChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SparkWindow win = (SparkWindow)d;
            Brush oldSystemBackground = (Brush)e.OldValue;
            Brush newSystemBackground = win.SystemBackground;
            win.OnSystemBackgroundChanged(oldSystemBackground, newSystemBackground);
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the SystemBackground property.
        /// </summary>
        /// <param name="oldSystemBackground">Old Value</param>
        /// <param name="newSystemBackground">New Value</param>
        void OnSystemBackgroundChanged(Brush oldSystemBackground, Brush newSystemBackground)
        {

        }

        #endregion

        #region SystemMouseOverBorderBrush

        /// <summary>
        /// SystemMouseOverBorderBrush Dependency Property
        /// </summary>
        public static readonly DependencyProperty SystemMouseOverBorderBrushProperty =
            DependencyProperty.Register("SystemMouseOverBorderBrush", typeof(Brush), typeof(SparkWindow),
                new PropertyMetadata(Brushes.White, OnSystemMouseOverBorderBrushChanged));

        /// <summary>
        /// Gets or sets the SystemMouseOverBorderBrush property. This dependency property 
        /// indicates the color of the border of the system buttons.
        /// </summary>
        public Brush SystemMouseOverBorderBrush
        {
            get { return (Brush)GetValue(SystemMouseOverBorderBrushProperty); }
            set { SetValue(SystemMouseOverBorderBrushProperty, value); }
        }

        /// <summary>
        /// Handles changes to the SystemMouseOverBorderBrush property.
        /// </summary>
        /// <param name="d">SparkWindow</param>
        /// <param name="e">DependencyProperty changed event arguments</param>
        private static void OnSystemMouseOverBorderBrushChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SparkWindow win = (SparkWindow)d;
            Brush oldSystemMouseOverBorderBrush = (Brush)e.OldValue;
            Brush newSystemMouseOverBorderBrush = win.SystemMouseOverBorderBrush;
            win.OnSystemMouseOverBorderBrushChanged(oldSystemMouseOverBorderBrush, newSystemMouseOverBorderBrush);
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the SystemMouseOverBorderBrush property.
        /// </summary>
        /// <param name="oldSystemMouseOverBorderBrush">Old Value</param>
        /// <param name="newSystemMouseOverBorderBrush">New Value</param>
        void OnSystemMouseOverBorderBrushChanged(Brush oldSystemMouseOverBorderBrush, Brush newSystemMouseOverBorderBrush)
        {

        }

        #endregion

        #region SystemMouseOverForeground

        /// <summary>
        /// SystemMouseOverForeground Dependency Property
        /// </summary>
        public static readonly DependencyProperty SystemMouseOverForegroundProperty =
            DependencyProperty.Register("SystemMouseOverForeground", typeof(Brush), typeof(SparkWindow),
                new PropertyMetadata(Brushes.White, OnSystemMouseOverForegroundChanged));

        /// <summary>
        /// Gets or sets the SystemMouseOverForeground property. This dependency property 
        /// indicates the foreground of the content of the system buttons.
        /// </summary>
        public Brush SystemMouseOverForeground
        {
            get { return (Brush)GetValue(SystemMouseOverForegroundProperty); }
            set { SetValue(SystemMouseOverForegroundProperty, value); }
        }

        /// <summary>
        /// Handles changes to the SystemMouseOverForeground property.
        /// </summary>
        /// <param name="d">SparkWindow</param>
        /// <param name="e">DependencyProperty changed event arguments</param>
        private static void OnSystemMouseOverForegroundChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SparkWindow win = (SparkWindow)d;
            Brush oldSystemMouseOverForeground = (Brush)e.OldValue;
            Brush newSystemMouseOverForeground = win.SystemMouseOverForeground;
            win.OnSystemMouseOverForegroundChanged(oldSystemMouseOverForeground, newSystemMouseOverForeground);
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the SystemMouseOverForeground property.
        /// </summary>
        /// <param name="oldSystemMouseOverForeground">Old Value</param>
        /// <param name="newSystemMouseOverForeground">New Value</param>
        void OnSystemMouseOverForegroundChanged(Brush oldSystemMouseOverForeground, Brush newSystemMouseOverForeground)
        {

        }

        #endregion

        #region SystemMouseOverBackground

        /// <summary>
        /// SystemMouseOverBackground Dependency Property
        /// </summary>
        public static readonly DependencyProperty SystemMouseOverBackgroundProperty =
            DependencyProperty.Register("SystemMouseOverBackground", typeof(Brush), typeof(SparkWindow),
                new PropertyMetadata(Brushes.White, OnSystemMouseOverBackgroundChanged));

        /// <summary>
        /// Gets or sets the SystemMouseOverBackground property. This dependency property 
        /// indicates the background of the content of the system buttons.
        /// </summary>
        public Brush SystemMouseOverBackground
        {
            get { return (Brush)GetValue(SystemMouseOverBackgroundProperty); }
            set { SetValue(SystemMouseOverBackgroundProperty, value); }
        }

        /// <summary>
        /// Handles changes to the SystemMouseOverBackground property.
        /// </summary>
        /// <param name="d">SparkWindow</param>
        /// <param name="e">DependencyProperty changed event arguments</param>
        private static void OnSystemMouseOverBackgroundChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SparkWindow win = (SparkWindow)d;
            Brush oldSystemMouseOverBackground = (Brush)e.OldValue;
            Brush newSystemMouseOverBackground = win.SystemMouseOverBackground;
            win.OnSystemMouseOverBackgroundChanged(oldSystemMouseOverBackground, newSystemMouseOverBackground);
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the SystemMouseOverBackground property.
        /// </summary>
        /// <param name="oldSystemMouseOverBackground">Old Value</param>
        /// <param name="newSystemMouseOverBackground">New Value</param>
        void OnSystemMouseOverBackgroundChanged(Brush oldSystemMouseOverBackground, Brush newSystemMouseOverBackground)
        {

        }

        #endregion

        #region SystemPressedBorderBrush

        /// <summary>
        /// SystemPressedBorderBrush Dependency Property
        /// </summary>
        public static readonly DependencyProperty SystemPressedBorderBrushProperty =
            DependencyProperty.Register("SystemPressedBorderBrush", typeof(Brush), typeof(SparkWindow),
                new PropertyMetadata(Brushes.White, OnSystemPressedBorderBrushChanged));

        /// <summary>
        /// Gets or sets the SystemPressedBorderBrush property. This dependency property 
        /// indicates the color of the border of the system buttons.
        /// </summary>
        public Brush SystemPressedBorderBrush
        {
            get { return (Brush)GetValue(SystemPressedBorderBrushProperty); }
            set { SetValue(SystemPressedBorderBrushProperty, value); }
        }

        /// <summary>
        /// Handles changes to the SystemPressedBorderBrush property.
        /// </summary>
        /// <param name="d">SparkWindow</param>
        /// <param name="e">DependencyProperty changed event arguments</param>
        private static void OnSystemPressedBorderBrushChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SparkWindow win = (SparkWindow)d;
            Brush oldSystemPressedBorderBrush = (Brush)e.OldValue;
            Brush newSystemPressedBorderBrush = win.SystemPressedBorderBrush;
            win.OnSystemPressedBorderBrushChanged(oldSystemPressedBorderBrush, newSystemPressedBorderBrush);
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the SystemPressedBorderBrush property.
        /// </summary>
        /// <param name="oldSystemPressedBorderBrush">Old Value</param>
        /// <param name="newSystemPressedBorderBrush">New Value</param>
        void OnSystemPressedBorderBrushChanged(Brush oldSystemPressedBorderBrush, Brush newSystemPressedBorderBrush)
        {

        }

        #endregion

        #region SystemPressedBorderThickness

        /// <summary>
        /// SystemPressedBorderThickness Dependency Property
        /// </summary>
        public static readonly DependencyProperty SystemPressedBorderThicknessProperty =
            DependencyProperty.Register("SystemPressedBorderThickness", typeof(Thickness), typeof(SparkWindow),
                new PropertyMetadata(new Thickness(0.5), OnSystemPressedBorderThicknessChanged));

        /// <summary>
        /// Gets or sets the SystemPressedBorderThickness property. This dependency property 
        /// indicates the thickness of the border of the System buttons.
        /// </summary>
        public Thickness SystemPressedBorderThickness
        {
            get { return (Thickness)GetValue(SystemPressedBorderThicknessProperty); }
            set { SetValue(SystemPressedBorderThicknessProperty, value); }
        }

        /// <summary>
        /// Handles changes to the SystemPressedBorderThickness property.
        /// </summary>
        /// <param name="d">SparkWindow</param>
        /// <param name="e">DependencyProperty changed event arguments</param>
        private static void OnSystemPressedBorderThicknessChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SparkWindow win = (SparkWindow)d;
            Thickness oldSystemPressedBorderThickness = (Thickness)e.OldValue;
            Thickness newSystemPressedBorderThickness = win.SystemPressedBorderThickness;
            win.OnSystemPressedBorderThicknessChanged(oldSystemPressedBorderThickness, newSystemPressedBorderThickness);
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the SystemPressedBorderThickness property.
        /// </summary>
        /// <param name="oldSystemPressedBorderThickness">Old Value</param>
        /// <param name="newSystemPressedBorderThickness">New Value</param>
        void OnSystemPressedBorderThicknessChanged(Thickness oldSystemPressedBorderThickness, Thickness newSystemPressedBorderThickness)
        {

        }

        #endregion

        #region SystemPressedForeground

        /// <summary>
        /// SystemPressedForeground Dependency Property
        /// </summary>
        public static readonly DependencyProperty SystemPressedForegroundProperty =
            DependencyProperty.Register("SystemPressedForeground", typeof(Brush), typeof(SparkWindow),
                new PropertyMetadata(Brushes.White, OnSystemPressedForegroundChanged));

        /// <summary>
        /// Gets or sets the SystemPressedForeground property. This dependency property 
        /// indicates the foreground of the content of the system buttons.
        /// </summary>
        public Brush SystemPressedForeground
        {
            get { return (Brush)GetValue(SystemPressedForegroundProperty); }
            set { SetValue(SystemPressedForegroundProperty, value); }
        }

        /// <summary>
        /// Handles changes to the SystemPressedForeground property.
        /// </summary>
        /// <param name="d">SparkWindow</param>
        /// <param name="e">DependencyProperty changed event arguments</param>
        private static void OnSystemPressedForegroundChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SparkWindow win = (SparkWindow)d;
            Brush oldSystemPressedForeground = (Brush)e.OldValue;
            Brush newSystemPressedForeground = win.SystemPressedForeground;
            win.OnSystemPressedForegroundChanged(oldSystemPressedForeground, newSystemPressedForeground);
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the SystemPressedForeground property.
        /// </summary>
        /// <param name="oldSystemPressedForeground">Old Value</param>
        /// <param name="newSystemPressedForeground">New Value</param>
        void OnSystemPressedForegroundChanged(Brush oldSystemPressedForeground, Brush newSystemPressedForeground)
        {

        }

        #endregion

        #region SystemPressedBackground

        /// <summary>
        /// SystemPressedBackground Dependency Property
        /// </summary>
        public static readonly DependencyProperty SystemPressedBackgroundProperty =
            DependencyProperty.Register("SystemPressedBackground", typeof(Brush), typeof(SparkWindow),
                new PropertyMetadata(Brushes.White, OnSystemPressedBackgroundChanged));

        /// <summary>
        /// Gets or sets the SystemPressedBackground property. This dependency property 
        /// indicates the background of the content of the system buttons.
        /// </summary>
        public Brush SystemPressedBackground
        {
            get { return (Brush)GetValue(SystemPressedBackgroundProperty); }
            set { SetValue(SystemPressedBackgroundProperty, value); }
        }

        /// <summary>
        /// Handles changes to the SystemPressedBackground property.
        /// </summary>
        /// <param name="d">SparkWindow</param>
        /// <param name="e">DependencyProperty changed event arguments</param>
        private static void OnSystemPressedBackgroundChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SparkWindow win = (SparkWindow)d;
            Brush oldSystemPressedBackground = (Brush)e.OldValue;
            Brush newSystemPressedBackground = win.SystemPressedBackground;
            win.OnSystemPressedBackgroundChanged(oldSystemPressedBackground, newSystemPressedBackground);
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the SystemPressedBackground property.
        /// </summary>
        /// <param name="oldSystemPressedBackground">Old Value</param>
        /// <param name="newSystemPressedBackground">New Value</param>
        void OnSystemPressedBackgroundChanged(Brush oldSystemPressedBackground, Brush newSystemPressedBackground)
        {

        }

        #endregion

        #endregion

        #region Construction / Initialization

        /// <summary>
        /// Static ctor
        /// </summary>
        static SparkWindow()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SparkWindow), new FrameworkPropertyMetadata(typeof(SparkWindow)));
        }

        /// <summary>
        /// Ctor
        /// </summary>
        public SparkWindow()
        {
            _defaultCursor = Cursor;
            SourceInitialized += OnWindowSourceInitialized;
            
            RoutedEventHandler handler = null;
            handler = (s, e) =>
            {
                Loaded -= handler;
                IntPtr handle = (new WindowInteropHelper(this)).Handle;
                HwndSource.FromHwnd(handle)?.AddHook(WindowProc);
                _hwndSource = (HwndSource)PresentationSource.FromVisual(this);
                this.EnableBlur();
            };

            Loaded += handler;
        }

        #endregion

        #region Overrides

        /// <summary>
        /// Override which is called when the template is applied
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            // Detach previously attached event handlers, if any
            Unsubscribe();

            // Get all the controls in the template
            GetTemplateParts();
        }

        /// <summary>
        /// Handles the closing event
        /// </summary>
        /// <param name="e">CancelEventArgs</param>
        protected override void OnClosing(CancelEventArgs e)
        {
            if (_minimizeButton != null)
                _minimizeButton.Click -= OnMinimize;

            if (_restoreButton != null)
                _restoreButton.Click -= OnRestore;

            if (_maximizeButton != null)
                _maximizeButton.Click -= OnMaximize;

            if (_closeButton != null)
                _closeButton.Click -= OnClose;

            if (_titleBar != null)
                _titleBar.MouseLeftButtonDown -= OnTitleBarMouseDown;

            base.OnClosing(e);
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Detach previously attached event handlers, if any
        /// </summary>
        private void Unsubscribe()
        {
            // PART_Minimize
            if (_minimizeButton != null)
            {
                _minimizeButton.Click -= OnMinimize;
            }

            // PART_Restore
            if (_restoreButton != null)
            {
                _restoreButton.Click -= OnRestore;
            }

            // PART_Maximize
            if (_maximizeButton != null)
            {
                _maximizeButton.Click -= OnMaximize;
            }

            // PART_Close
            if (_closeButton != null)
            {
                _closeButton.Click -= OnClose;
            }

            // PART_TitleBar
            if (_titleBar != null)
            {
                _titleBar.MouseLeftButtonDown -= OnTitleBarMouseDown;
            }

            // PART_Resize
            if (_resizeGrid != null)
            {
                foreach (var resizeRectangle in _resizeGrid.Children.OfType<Rectangle>())
                {
                    resizeRectangle.MouseEnter -= OnResizeMouseEnter;
                    resizeRectangle.MouseLeave -= OnResizeMouseLeave;
                    resizeRectangle.PreviewMouseLeftButtonDown -= OnResizeMouseLeftButtonDown;
                }
            }

            SourceInitialized -= OnWindowSourceInitialized;
        }

        /// <summary>
        /// Gets the required controls in the template
        /// </summary>
        protected void GetTemplateParts()
        {
            // PART_Minimize
            _minimizeButton = GetChildControl<Button>("PART_Minimize");
            if (_minimizeButton != null)
            {
                _minimizeButton.Click += OnMinimize;
            }

            // PART_Restore
            _restoreButton = GetChildControl<Button>("PART_Restore");
            if (_restoreButton != null)
            {
                _restoreButton.Click += OnRestore;
            }

            // PART_Maximize
            _maximizeButton = GetChildControl<Button>("PART_Maximize");
            if (_maximizeButton != null)
            {
                _maximizeButton.Click += OnMaximize;
            }

            // PART_Close
            _closeButton = GetChildControl<Button>("PART_Close");
            if (_closeButton != null)
            {
                _closeButton.Click += OnClose;
            }

            // PART_TitleBar
            _titleBar = GetChildControl<Border>("PART_TitleBar");
            if (_titleBar != null)
            {
                _titleBar.MouseLeftButtonDown += OnTitleBarMouseDown;
            }

            // PART_Resize
            _resizeGrid = GetChildControl<Grid>("PART_Resize");
            if (_resizeGrid != null)
            {
                foreach (Rectangle resizeRectangle in _resizeGrid.Children.OfType<Rectangle>())
                {
                    resizeRectangle.MouseEnter += OnResizeMouseEnter;
                    resizeRectangle.MouseLeave += OnResizeMouseLeave;
                    resizeRectangle.PreviewMouseLeftButtonDown += OnResizeMouseLeftButtonDown;
                }
            }

            // Update the system control buttons in the window frame
            UpdateWindowFrame(WindowFrameMode);
        }

        /// <summary>
        /// Update the system control buttons in the window frame
        /// </summary>
        /// <param name="winMode">Window mode</param>
        private void UpdateWindowFrame(WindowMode winMode)
        {
            switch (winMode)
            {
                // Only close button should be visible if the mode is CanClose/PaneCanClose
                case WindowMode.CanClose:
                case WindowMode.PaneCanClose:
                    if (_minimizeButton != null)
                        _minimizeButton.Visibility = Visibility.Collapsed;
                    if (_maximizeButton != null)
                        _maximizeButton.Visibility = Visibility.Collapsed;
                    if (_restoreButton != null)
                        _restoreButton.Visibility = Visibility.Collapsed;
                    break;

                // All buttons - minimize, maximize and close will be visible if the mode is CanMaximize
                case WindowMode.CanMaximize:
                    if (_minimizeButton != null)
                    {
                        _minimizeButton.Visibility = Visibility.Visible;
                        Grid.SetColumn(_minimizeButton, 1);
                    }
                    if (_maximizeButton != null)
                    {
                        _maximizeButton.Visibility = Visibility.Visible;
                    }
                    break;
                // All buttons - minimize, maximize and close will be hidden if the mode is CanMaximize
                case WindowMode.ChildWindow:
                    if (_minimizeButton != null)
                        _minimizeButton.Visibility = Visibility.Collapsed;
                    if (_maximizeButton != null)
                        _maximizeButton.Visibility = Visibility.Collapsed;
                    if (_restoreButton != null)
                        _restoreButton.Visibility = Visibility.Collapsed;
                    if (_closeButton != null)
                        _closeButton.Visibility = Visibility.Collapsed;
                    break;
                // Only minimize and close buttons should be visible if the mode is Pane/CanMinimize
                case WindowMode.Pane:
                case WindowMode.CanMinimize:
                default:
                    if (_minimizeButton != null)
                    {
                        _minimizeButton.Visibility = Visibility.Visible;
                        Grid.SetColumn(_minimizeButton, 2);
                    }
                    if (_maximizeButton != null)
                        _maximizeButton.Visibility = Visibility.Collapsed;
                    if (_restoreButton != null)
                        _restoreButton.Visibility = Visibility.Collapsed;
                    break;

            }

            // If the mode is Pane/PaneCanClose then the window should be in maximized state
            if ((WindowFrameMode == WindowMode.Pane) || (WindowFrameMode == WindowMode.PaneCanClose))
            {
                SystemCommands.MaximizeWindow(this);
            }
        }

        /// <summary>
        /// Generic method to get a control from the template
        /// </summary>
        /// <typeparam name="T">Type of the control</typeparam>
        /// <param name="ctrlName">Name of the control in the template</param>
        /// <returns>Control</returns>
        protected T GetChildControl<T>(string ctrlName) where T : DependencyObject
        {
            var ctrl = GetTemplateChild(ctrlName) as T;
            return ctrl;
        }

        /// <summary>
        /// Toggles the state of the window to maximized if the state is normal else vice-versa
        /// </summary>
        private void ToggleMaximize()
        {
            switch (WindowState)
            {
                case WindowState.Maximized:
                    SystemCommands.RestoreWindow(this);
                    break;
                case WindowState.Normal:
                    WindowState = WindowState.Maximized;
                    break;
            }
        }

        /// <summary>
        /// Hook for window's messages
        /// </summary>
        /// <param name="hwnd"></param>
        /// <param name="msg"></param>
        /// <param name="wParam"></param>
        /// <param name="lParam"></param>
        /// <param name="handled"></param>
        /// <returns></returns>
        private static IntPtr WindowProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case 0x0024:
                    MonitorHelper.WmGetMinMaxInfo(hwnd, lParam);
                    handled = true;
                    break;
            }

            return (System.IntPtr)0;
        }

        /// <summary>
        /// Sends the Window Message for resize
        /// </summary>
        /// <param name="direction">Resize Direction</param>
        private void ResizeWindow(ResizeDirection direction)
        {
            if (_hwndSource == null)
                return;

            SendMessage(_hwndSource.Handle, 0x112, (IntPtr)(61440 + direction), IntPtr.Zero);
        }

        #endregion

        #region DLLImports

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, UInt32 msg, IntPtr wParam, IntPtr lParam);

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handler for the SourceInitialized event
        /// </summary>
        /// <param name="sender">Window</param>
        /// <param name="e">EventArgs</param>
        private void OnWindowSourceInitialized(object sender, EventArgs e)
        {
            IntPtr handle = (new WindowInteropHelper(this)).Handle;
            HwndSource.FromHwnd(handle)?.AddHook(WindowProc);
            _hwndSource = (HwndSource)PresentationSource.FromVisual(this);
        }

        /// <summary>
        /// Handles the MouseDown event on the title bar.
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">MouseButtonEventArgs</param>
        void OnTitleBarMouseDown(object sender, MouseButtonEventArgs e)
        {
            // If the user has clicked on the title bar twice then toggle the 
            // state of the window (if window is maximizable)
            if (WindowFrameMode == WindowMode.CanMaximize && e.ClickCount == 2)
            {
                ToggleMaximize();
                return;
            }

            // Allow the user to drag the window to a new location
            DragMove();
        }

        /// <summary>
        /// Overridable event handler for the event raised when Minimize button is clicked
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">RoutedEventArgs</param>
        protected virtual void OnMinimize(object sender, RoutedEventArgs e)
        {
            SystemCommands.MinimizeWindow(this);
        }

        /// <summary>
        /// Overridable event handler for the event raised when Restore button is clicked
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">RoutedEventArgs</param>
        protected virtual void OnRestore(object sender, RoutedEventArgs e)
        {
            SystemCommands.RestoreWindow(this);
        }

        /// <summary>
        /// Overridable event handler for the event raised when Maximize button is clicked
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">RoutedEventArgs</param>
        protected virtual void OnMaximize(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Maximized;
        }

        /// <summary>
        /// Overridable event handler for the event raised when Close button is clicked
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">RoutedEventArgs</param>
        protected virtual void OnClose(object sender, RoutedEventArgs e)
        {
            // Unsubscribe to the events
            Unsubscribe();
            // Close the window
            SystemCommands.CloseWindow(this);
        }

        /// <summary>
        /// Handler for the Mouse Enter event in any of the Resize rectangles
        /// </summary>
        /// <param name="sender">Rectangle</param>
        /// <param name="e">MouseEventArgs</param>
        private void OnResizeMouseEnter(object sender, MouseEventArgs e)
        {
            if (ResizeMode != ResizeMode.CanResize)
                return;

            var rectangle = sender as Rectangle;
            if (rectangle == null)
                return;

            switch (rectangle.Name)
            {
                case "ResizeN":
                    Cursor = Cursors.SizeNS;
                    break;
                case "ResizeS":
                    Cursor = Cursors.SizeNS;
                    break;
                case "ResizeW":
                    Cursor = Cursors.SizeWE;
                    break;
                case "ResizeE":
                    Cursor = Cursors.SizeWE;
                    break;
                case "ResizeNW":
                    Cursor = Cursors.SizeNWSE;
                    break;
                case "ResizeNE":
                    Cursor = Cursors.SizeNESW;
                    break;
                case "ResizeSW":
                    Cursor = Cursors.SizeNESW;
                    break;
                case "ResizeSE":
                    Cursor = Cursors.SizeNWSE;
                    break;
            }
        }

        /// <summary>
        /// Handler for the Mouse Leave event in any of the Resize rectangles
        /// </summary>
        /// <param name="sender">Rectangle</param>
        /// <param name="e">MouseEventArgs</param>
        private void OnResizeMouseLeave(object sender, MouseEventArgs e)
        {
            if (ResizeMode != ResizeMode.CanResize)
                return;

            if (Mouse.LeftButton != MouseButtonState.Pressed)
            {
                Cursor = _defaultCursor;
            }
        }

        /// <summary>
        /// Handler for the Preview Mouse Left Button Down event in any of the 
        /// Resize rectangles
        /// </summary>
        /// <param name="sender">Rectangle</param>
        /// <param name="e">MouseButtonEventArgs</param>
        private void OnResizeMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (ResizeMode != ResizeMode.CanResize)
                return;

            var rectangle = sender as Rectangle;
            if (rectangle == null)
                return;

            switch (rectangle.Name)
            {
                case "ResizeN":
                    Cursor = Cursors.SizeNS;
                    ResizeWindow(ResizeDirection.Top);
                    break;
                case "ResizeS":
                    Cursor = Cursors.SizeNS;
                    ResizeWindow(ResizeDirection.Bottom);
                    break;
                case "ResizeW":
                    Cursor = Cursors.SizeWE;
                    ResizeWindow(ResizeDirection.Left);
                    break;
                case "ResizeE":
                    Cursor = Cursors.SizeWE;
                    ResizeWindow(ResizeDirection.Right);
                    break;
                case "ResizeNW":
                    Cursor = Cursors.SizeNWSE;
                    ResizeWindow(ResizeDirection.TopLeft);
                    break;
                case "ResizeNE":
                    Cursor = Cursors.SizeNESW;
                    ResizeWindow(ResizeDirection.TopRight);
                    break;
                case "ResizeSW":
                    Cursor = Cursors.SizeNESW;
                    ResizeWindow(ResizeDirection.BottomLeft);
                    break;
                case "ResizeSE":
                    Cursor = Cursors.SizeNWSE;
                    ResizeWindow(ResizeDirection.BottomRight);
                    break;
            }
        }

        #endregion
    }
}
