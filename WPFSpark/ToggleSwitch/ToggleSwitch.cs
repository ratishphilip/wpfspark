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
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Data;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using Microsoft.Win32;
using static System.String;

namespace WPFSpark
{
    [TemplatePart(Name = "PART_RootCanvas", Type = typeof(Canvas))]
    [TemplatePart(Name = "PART_ContentGrid", Type = typeof(Grid))]
    [TemplatePart(Name = "PART_CheckedBorder", Type = typeof(Border))]
    [TemplatePart(Name = "PART_CheckedContent", Type = typeof(TextBlock))]
    [TemplatePart(Name = "PART_UncheckedBorder", Type = typeof(Border))]
    [TemplatePart(Name = "PART_UncheckedContent", Type = typeof(TextBlock))]
    [TemplatePart(Name = "PART_OuterBorder", Type = typeof(ClipBorder))]
    [TemplatePart(Name = "PART_CheckedKeyFrame", Type = typeof(EasingThicknessKeyFrame))]
    [TemplatePart(Name = "PART_UncheckedKeyFrame", Type = typeof(EasingThicknessKeyFrame))]
    [TemplatePart(Name = "PART_ThumbContentGrid", Type = typeof(Grid))]
    [TemplatePart(Name = "PART_OuterGrid", Type = typeof(Grid))]
    public class ToggleSwitch : ToggleButton
    {
        #region Enums

        /// <summary>
        /// Enum which defines whether the ToggleSwitch should update it's checked background
        /// witht the current Windows Accent color or user specified Accent color.
        /// </summary>
        public enum AccentModeType
        {
            System = 0,
            User = 1,
        }

        #endregion

        #region Fields

        private Canvas _rootCanvas;
        private Grid _contentGrid;
        private Border _checkedBorder;
        private TextBlock _checkedContent;
        private Border _unCheckedBorder;
        private TextBlock _unCheckedContent;
        private ToggleSwitchProxy _tsProxy;
        private ClipBorder _outerBorder;
        private EasingThicknessKeyFrame _checkedKeyFrame;
        private EasingThicknessKeyFrame _uncheckedKeyFrame;
        private Grid _thumbContentGrid;
        private Grid _outerGrid;
        private bool _isCalculatingLayout;

        #endregion

        #region Constants

        private const double DefaultThumbWidthRatio = 0.4;
        private const double MinThumbWidthRatio = 0.1;
        private const double MaxThumbWidthRatio = 0.9;

        #endregion

        #region Dependency Properties

        #region CheckedText

        /// <summary>
        /// CheckedText Dependency Property
        /// </summary>
        public static readonly DependencyProperty CheckedTextProperty =
            DependencyProperty.Register("CheckedText", typeof(string), typeof(ToggleSwitch),
                new PropertyMetadata(Empty));

        /// <summary>
        /// Gets or sets the CheckedText property. This dependency property 
        /// indicates the on text.
        /// </summary>
        public string CheckedText
        {
            get { return (string)GetValue(CheckedTextProperty); }
            set { SetValue(CheckedTextProperty, value); }
        }

        #endregion

        #region CheckedTextEffect

        /// <summary>
        /// CheckedTextEffect Dependency Property
        /// </summary>
        public static readonly DependencyProperty CheckedTextEffectProperty =
            DependencyProperty.Register("CheckedTextEffect", typeof(Effect), typeof(ToggleSwitch));

        /// <summary>
        /// Gets or sets the CheckedTextEffect property. This dependency property 
        /// indicates the effect to be applied on the Checked Text.
        /// </summary>
        public Effect CheckedTextEffect
        {
            get { return (Effect)GetValue(CheckedTextEffectProperty); }
            set { SetValue(CheckedTextEffectProperty, value); }
        }

        #endregion

        #region CheckedBackground

        /// <summary>
        /// CheckedBackground Dependency Property
        /// </summary>
        public static readonly DependencyProperty CheckedBackgroundProperty =
            DependencyProperty.Register("CheckedBackground", typeof(Brush), typeof(ToggleSwitch),
                new PropertyMetadata(Brushes.White));

        /// <summary>
        /// Gets or sets the CheckedBackground property. This dependency property 
        /// indicates Background of the Checked Text.
        /// </summary>
        public Brush CheckedBackground
        {
            get { return (Brush)GetValue(CheckedBackgroundProperty); }
            set { SetValue(CheckedBackgroundProperty, value); }
        }

        #endregion

        #region CheckedForeground

        /// <summary>
        /// CheckedForeground Dependency Property
        /// </summary>
        public static readonly DependencyProperty CheckedForegroundProperty =
            DependencyProperty.Register("CheckedForeground", typeof(Brush), typeof(ToggleSwitch),
                new PropertyMetadata(Brushes.Black));

        /// <summary>
        /// Gets or sets the CheckedForeground property. This dependency property 
        /// indicates Foreground of the Checked Text.
        /// </summary>
        public Brush CheckedForeground
        {
            get { return (Brush)GetValue(CheckedForegroundProperty); }
            set { SetValue(CheckedForegroundProperty, value); }
        }

        #endregion

        #region CheckedToolTip

        /// <summary>
        /// CheckedToolTip Dependency Property
        /// </summary>
        public static readonly DependencyProperty CheckedToolTipProperty =
            DependencyProperty.Register("CheckedToolTip", typeof(string), typeof(ToggleSwitch),
                new FrameworkPropertyMetadata(Empty, OnCheckedToolTipChanged));

        /// <summary>
        /// Gets or sets the CheckedToolTip property. This dependency property 
        /// indicates the tooltip of the ToggleSwitch when the control is in Checked state.
        /// </summary>
        public string CheckedToolTip
        {
            get { return (string)GetValue(CheckedToolTipProperty); }
            set { SetValue(CheckedToolTipProperty, value); }
        }

        /// <summary>
        /// Handles changes to the CheckedToolTip property.
        /// </summary>
        /// <param name="d">ToggleSwitch</param>
        /// <param name="e">DependencyProperty changed event arguments</param>
        private static void OnCheckedToolTipChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ToggleSwitch tSwitch = (ToggleSwitch)d;
            string oldCheckedToolTip = (string)e.OldValue;
            string newCheckedToolTip = tSwitch.CheckedToolTip;
            tSwitch.OnCheckedToolTipChanged(oldCheckedToolTip, newCheckedToolTip);
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the CheckedToolTip property.
        /// </summary>
        /// <param name="oldCheckedToolTip">Old Value</param>
        /// <param name="newCheckedToolTip">New Value</param>
        protected void OnCheckedToolTipChanged(string oldCheckedToolTip, string newCheckedToolTip)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                if ((IsChecked == true) && (!IsNullOrWhiteSpace(newCheckedToolTip)))
                {
                    ToolTip = new ToolTip() { Content = newCheckedToolTip };
                }
            }));
        }

        #endregion

        #region CheckedMargin

        /// <summary>
        /// CheckedMargin Read-Only Dependency Property
        /// </summary>
        public static readonly DependencyPropertyKey CheckedMarginPropertyKey =
            DependencyProperty.RegisterReadOnly("CheckedMargin", typeof(Thickness), typeof(ToggleSwitch),
                new UIPropertyMetadata(null));

        /// <summary>
        /// Map the CheckedMarginProperty to the CheckedMarginPropertyKey's DependencyProperty property.
        /// </summary>
        public static readonly DependencyProperty CheckedMarginProperty = CheckedMarginPropertyKey.DependencyProperty;

        /// <summary>
        /// Gets the CheckedMargin property. This dependency property 
        /// indicates the Margin of the PART_ContentGrid when
        /// the ToggleSwitch is in Checked state.
        /// NOTE: This property can be set internally only (or by a class deriving from this class)
        /// </summary>
        public Thickness CheckedMargin
        {
            get { return (Thickness)GetValue(CheckedMarginProperty); }
            protected set { SetValue(CheckedMarginPropertyKey, value); }
        }

        #endregion

        #region CornerRadius

        /// <summary>
        /// CornerRadius Dependency Property
        /// </summary>
        public static readonly DependencyProperty CornerRadiusProperty =
            DependencyProperty.Register("CornerRadius", typeof(CornerRadius), typeof(ToggleSwitch),
                new FrameworkPropertyMetadata(new CornerRadius(), OnCornerRadiusChanged));

        /// <summary>
        /// Gets or sets the CornerRadius property. This dependency property 
        /// indicates the CornerRadius of the Outer ClipBorder of the Toggleswitch..
        /// </summary>
        public CornerRadius CornerRadius
        {
            get { return (CornerRadius)GetValue(CornerRadiusProperty); }
            set { SetValue(CornerRadiusProperty, value); }
        }

        /// <summary>
        /// Handles changes to the CornerRadius property.
        /// </summary>
        private static void OnCornerRadiusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = (ToggleSwitch)d;
            var oldCornerRadius = (CornerRadius)e.OldValue;
            var newCornerRadius = ctrl.CornerRadius;
            ctrl.OnCornerRadiusChanged(oldCornerRadius, newCornerRadius);
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the CornerRadius property.
        /// </summary>
        protected void OnCornerRadiusChanged(CornerRadius oldCornerRadius, CornerRadius newCornerRadius)
        {
            // Update the Layout
            if (!_isCalculatingLayout)
            {
                InvalidateVisual();
            }
        }

        /// <summary>
        /// Coerces the CornerRadius value.
        /// </summary>
        private static object CoerceCornerRadius(DependencyObject d, object value)
        {
            var ctrl = (ToggleSwitch)d;
            var desiredCornerRadius = (CornerRadius)value;

            return GetCoercedCornerRadius(ctrl.Width, ctrl.Height, ctrl.BorderThickness, desiredCornerRadius);
        }

        #endregion

        #region IsCheckedLeft

        /// <summary>
        /// IsCheckedLeft Dependency Property
        /// </summary>
        public static readonly DependencyProperty IsCheckedLeftProperty =
            DependencyProperty.Register("IsCheckedLeft", typeof(bool), typeof(ToggleSwitch),
                new FrameworkPropertyMetadata(true, OnIsCheckedLeftChanged));

        /// <summary>
        /// Gets or sets the IsCheckedLeft property. This dependency property 
        /// indicates whether the content for the Checked state should appear in the left side.
        /// </summary>
        public bool IsCheckedLeft
        {
            get { return (bool)GetValue(IsCheckedLeftProperty); }
            set { SetValue(IsCheckedLeftProperty, value); }
        }

        /// <summary>
        /// Handles changes to the IsCheckedLeft property.
        /// </summary>
        /// <param name="d">ToggleSwitch</param>
        /// <param name="e">DependencyProperty changed event arguments</param>
        private static void OnIsCheckedLeftChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ToggleSwitch tSwitch = (ToggleSwitch)d;
            bool oldIsCheckedLeft = (bool)e.OldValue;
            bool newIsCheckedLeft = tSwitch.IsCheckedLeft;
            tSwitch.OnIsCheckedLeftChanged(oldIsCheckedLeft, newIsCheckedLeft);
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the IsCheckedLeft property.
        /// </summary>
        /// <param name="oldIsCheckedLeft">Old Value</param>
        /// <param name="newIsCheckedLeft">New Value</param>
        protected void OnIsCheckedLeftChanged(bool oldIsCheckedLeft, bool newIsCheckedLeft)
        {
            UpdateToggleSwitchContents(newIsCheckedLeft);
        }

        #endregion

        #region ThumbBackground

        /// <summary>
        /// ThumbBackground Dependency Property
        /// </summary>
        public static readonly DependencyProperty ThumbBackgroundProperty =
            DependencyProperty.Register("ThumbBackground", typeof(Brush), typeof(ToggleSwitch),
                new PropertyMetadata((Brushes.Black)));

        /// <summary>
        /// Gets or sets the ThumbBackground property. This dependency property 
        /// indicates the Background of the Thumb.
        /// </summary>
        public Brush ThumbBackground
        {
            get { return (Brush)GetValue(ThumbBackgroundProperty); }
            set { SetValue(ThumbBackgroundProperty, value); }
        }

        #endregion

        #region ThumbBorderBrush

        /// <summary>
        /// ThumbBorderBrush Dependency Property
        /// </summary>
        public static readonly DependencyProperty ThumbBorderBrushProperty =
            DependencyProperty.Register("ThumbBorderBrush", typeof(Brush), typeof(ToggleSwitch),
                new FrameworkPropertyMetadata(Brushes.Gray,
                    FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.SubPropertiesDoNotAffectRender));

        /// <summary>
        /// Gets or sets the ThumbBorderBrush property. This dependency property 
        /// indicates the BorderBrush of the Thumb.
        /// </summary>
        public Brush ThumbBorderBrush
        {
            get { return (Brush)GetValue(ThumbBorderBrushProperty); }
            set { SetValue(ThumbBorderBrushProperty, value); }
        }

        #endregion

        #region ThumbBorderThickness

        /// <summary>
        /// ThumbBorderThickness Dependency Property
        /// </summary>
        public static readonly DependencyProperty ThumbBorderThicknessProperty =
            DependencyProperty.Register("ThumbBorderThickness", typeof(Thickness), typeof(ToggleSwitch),
                new FrameworkPropertyMetadata(new Thickness(),
                    FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender,
                    OnThumbBorderThicknessChanged));

        /// <summary>
        /// Gets or sets the ThumbBorderThickness property. This dependency property 
        /// indicates the BorderThickness of the Thumb.
        /// </summary>
        public Thickness ThumbBorderThickness
        {
            get { return (Thickness)GetValue(ThumbBorderThicknessProperty); }
            set { SetValue(ThumbBorderThicknessProperty, value); }
        }

        /// <summary>
        /// Handles changes to the ThumbBorderThickness property.
        /// </summary>
        /// <param name="d">ToggleSwitch</param>
		/// <param name="e">DependencyProperty changed event arguments</param>
        private static void OnThumbBorderThicknessChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = (ToggleSwitch)d;
            var oldThumbBorderThickness = (Thickness)e.OldValue;
            var newThumbBorderThickness = ctrl.ThumbBorderThickness;
            ctrl.OnThumbBorderThicknessChanged(oldThumbBorderThickness, newThumbBorderThickness);
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the ThumbBorderThickness property.
        /// </summary>
		/// <param name="oldThumbBorderThickness">Old Value</param>
		/// <param name="newThumbBorderThickness">New Value</param>
        void OnThumbBorderThicknessChanged(Thickness oldThumbBorderThickness, Thickness newThumbBorderThickness)
        {
            // Update the Layout
            //InvalidateVisual();
        }

        #endregion

        #region ThumbCornerRadius

        /// <summary>
        /// ThumbCornerRadius Read-Only Dependency Property
        /// </summary>
        public static readonly DependencyPropertyKey ThumbCornerRadiusPropertyKey =
            DependencyProperty.RegisterReadOnly("ThumbCornerRadius", typeof(CornerRadius), typeof(ToggleSwitch),
                new FrameworkPropertyMetadata(new CornerRadius()));

        /// <summary>
        /// Map the ThumbCornerRadiusProperty to the ThumbCornerRadiusPropertyKey's DependencyProperty property.
        /// </summary>
        public static readonly DependencyProperty ThumbCornerRadiusProperty = ThumbCornerRadiusPropertyKey.DependencyProperty;

        /// <summary>
        /// Gets the ThumbCornerRadius property. This dependency property 
        /// indicates the corner radius of the Thumb.
        /// NOTE: This property can be set internally only (or by a class deriving from this class)
        /// </summary>
        public CornerRadius ThumbCornerRadius
        {
            get { return (CornerRadius)GetValue(ThumbCornerRadiusProperty); }
            protected set { SetValue(ThumbCornerRadiusPropertyKey, value); }
        }

        #endregion

        #region ThumbGlowBrush

        /// <summary>
        /// ThumbGlowBrush Dependency Property
        /// </summary>
        public static readonly DependencyProperty ThumbGlowBrushProperty =
            DependencyProperty.Register("ThumbGlowBrush", typeof(Brush), typeof(ToggleSwitch),
                new FrameworkPropertyMetadata(Brushes.Transparent));

        /// <summary>
        /// Gets or sets the ThumbGlowBrush property. This dependency property 
        /// indicates the Brush for the Glow in the Thumb which is shown when 
        /// the mouse hovers over the ToggleSwitch.
        /// </summary>
        public Brush ThumbGlowBrush
        {
            get { return (Brush)GetValue(ThumbGlowBrushProperty); }
            set { SetValue(ThumbGlowBrushProperty, value); }
        }

        #endregion

        #region ThumbShineBrush

        /// <summary>
        /// ThumbShineBrush Dependency Property
        /// </summary>
        public static readonly DependencyProperty ThumbShineBrushProperty =
            DependencyProperty.Register("ThumbShineBrush", typeof(Brush), typeof(ToggleSwitch),
                new FrameworkPropertyMetadata(Brushes.Transparent));

        /// <summary>
        /// Gets or sets the ThumbShineBrush property. This dependency property 
        /// indicates the Brush for the shine on the Thumb.
        /// </summary>
        public Brush ThumbShineBrush
        {
            get { return (Brush)GetValue(ThumbShineBrushProperty); }
            set { SetValue(ThumbShineBrushProperty, value); }
        }

        #endregion

        #region ThumbWidthRatio

        /// <summary>
        /// ThumbWidthRatio Dependency Property
        /// </summary>
        public static readonly DependencyProperty ThumbWidthRatioProperty =
            DependencyProperty.Register("ThumbWidthRatio", typeof(double), typeof(ToggleSwitch),
                new FrameworkPropertyMetadata(DefaultThumbWidthRatio, OnThumbWidthRatioChanged, CoerceThumbWidthRatio));

        /// <summary>
        /// Gets or sets the ThumbWidthRatio property. This dependency property 
        /// indicates the ratio of the Width of the Thumb to the Width of the ToggleSwitch.
        /// It should be a value between 0.1 and 0.9, inclusive.
        /// </summary>
        public double ThumbWidthRatio
        {
            get { return (double)GetValue(ThumbWidthRatioProperty); }
            set { SetValue(ThumbWidthRatioProperty, value); }
        }

        /// <summary>
        /// Handles changes to the ThumbWidthRatio property.
        /// </summary>
        private static void OnThumbWidthRatioChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = (ToggleSwitch)d;
            var oldThumbWidthRatio = (double)e.OldValue;
            var newThumbWidthRatio = ctrl.ThumbWidthRatio;
            ctrl.OnThumbWidthRatioChanged(oldThumbWidthRatio, newThumbWidthRatio);
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the ThumbWidthRatio property.
        /// </summary>
        protected void OnThumbWidthRatioChanged(double oldThumbWidthRatio, double newThumbWidthRatio)
        {
            // Update Layout
            if (!AutoThumbWidthRatio)
            {
                InvalidateVisual();
            }
        }

        /// <summary>
        /// Coerces the ThumbWidthRatio value.
        /// </summary>
        private static object CoerceThumbWidthRatio(DependencyObject d, object value)
        {
            var desiredThumbWidthRatio = (double)value;

            if (desiredThumbWidthRatio < MinThumbWidthRatio)
            {
                desiredThumbWidthRatio = MinThumbWidthRatio;
            }

            if (desiredThumbWidthRatio > MaxThumbWidthRatio)
            {
                desiredThumbWidthRatio = MaxThumbWidthRatio;
            }

            return desiredThumbWidthRatio;
        }

        #endregion

        #region ThumbWidth

        /// <summary>
        /// ThumbWidth Read-Only Dependency Property
        /// </summary>
        public static readonly DependencyPropertyKey ThumbWidthPropertyKey =
            DependencyProperty.RegisterReadOnly("ThumbWidth", typeof(double), typeof(ToggleSwitch),
                new FrameworkPropertyMetadata(0.0,
                    FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary>
        /// Map the ThumbWidthProperty to the ThumbWidthPropertyKey's DependencyProperty property.
        /// </summary>
        public static readonly DependencyProperty ThumbWidthProperty = ThumbWidthPropertyKey.DependencyProperty;

        /// <summary>
        /// Gets the ThumbWidth property. This dependency property 
        /// indicates the width of the Thumb.
        /// NOTE: This property can be set internally only (or by a class deriving from this class)
        /// </summary>
        public double ThumbWidth
        {
            get { return (double)GetValue(ThumbWidthProperty); }
            protected set { SetValue(ThumbWidthPropertyKey, value); }
        }

        #endregion

        #region ThumbHeight

        /// <summary>
        /// ThumbHeight Read-Only Dependency Property
        /// </summary>
        public static readonly DependencyPropertyKey ThumbHeightPropertyKey =
            DependencyProperty.RegisterReadOnly("ThumbHeight", typeof(double), typeof(ToggleSwitch),
                new FrameworkPropertyMetadata(0.0));

        /// <summary>
        /// Map the ThumbHeightProperty to the ThumbHeightPropertyKey's DependencyProperty property.
        /// </summary>
        public static readonly DependencyProperty ThumbHeightProperty = ThumbHeightPropertyKey.DependencyProperty;

        /// <summary>
        /// Gets the ThumbHeight property. This dependency property 
        /// indicates the calculated Height of the Thumb.
        /// NOTE: This property can be set internally only (or by a class deriving from this class)
        /// </summary>
        public double ThumbHeight
        {
            get { return (double)GetValue(ThumbHeightProperty); }
            protected set { SetValue(ThumbHeightPropertyKey, value); }
        }

        #endregion

        #region UncheckedBackground

        /// <summary>
        /// UncheckedBackground Dependency Property
        /// </summary>
        public static readonly DependencyProperty UncheckedBackgroundProperty =
            DependencyProperty.Register("UncheckedBackground", typeof(Brush), typeof(ToggleSwitch),
                new PropertyMetadata(Brushes.White));

        /// <summary>
        /// Gets or sets the UncheckedBackground property. This dependency property 
        /// indicates the Background of the Unchecked Text.
        /// </summary>
        public Brush UncheckedBackground
        {
            get { return (Brush)GetValue(UncheckedBackgroundProperty); }
            set { SetValue(UncheckedBackgroundProperty, value); }
        }

        #endregion

        #region UncheckedForeground

        /// <summary>
        /// UncheckedForeground Dependency Property
        /// </summary>
        public static readonly DependencyProperty UncheckedForegroundProperty =
            DependencyProperty.Register("UncheckedForeground", typeof(Brush), typeof(ToggleSwitch),
                new PropertyMetadata(Brushes.Black));

        /// <summary>
        /// Gets or sets the UncheckedForeground property. This dependency property 
        /// indicates the Foreground of the Unchecked Text.
        /// </summary>
        public Brush UncheckedForeground
        {
            get { return (Brush)GetValue(UncheckedForegroundProperty); }
            set { SetValue(UncheckedForegroundProperty, value); }
        }

        #endregion

        #region UncheckedText

        /// <summary>
        /// UncheckedText Dependency Property
        /// </summary>
        public static readonly DependencyProperty UncheckedTextProperty =
            DependencyProperty.Register("UncheckedText", typeof(string), typeof(ToggleSwitch),
                new PropertyMetadata(Empty));

        /// <summary>
        /// Gets or sets the UncheckedText property. This dependency property 
        /// indicates the off text.
        /// </summary>
        public string UncheckedText
        {
            get { return (string)GetValue(UncheckedTextProperty); }
            set { SetValue(UncheckedTextProperty, value); }
        }

        #endregion

        #region UncheckedTextEffect

        /// <summary>
        /// UncheckedTextEffect Dependency Property
        /// </summary>
        public static readonly DependencyProperty UncheckedTextEffectProperty =
            DependencyProperty.Register("UncheckedTextEffect", typeof(Effect), typeof(ToggleSwitch));

        /// <summary>
        /// Gets or sets the UncheckedTextEffect property. This dependency property 
        /// indicates the effect to be applied on the Unchecked Text.
        /// </summary>
        public Effect UncheckedTextEffect
        {
            get { return (Effect)GetValue(UncheckedTextEffectProperty); }
            set { SetValue(UncheckedTextEffectProperty, value); }
        }

        #endregion

        #region UncheckedToolTip

        /// <summary>
        /// UncheckedToolTip Dependency Property
        /// </summary>
        public static readonly DependencyProperty UncheckedToolTipProperty =
            DependencyProperty.Register("UncheckedToolTip", typeof(string), typeof(ToggleSwitch),
                new FrameworkPropertyMetadata(Empty, OnUncheckedToolTipChanged));

        /// <summary>
        /// Gets or sets the UncheckedToolTip property. This dependency property 
        /// indicates the tooltip for the control when it is in Unchecked state.
        /// </summary>
        public string UncheckedToolTip
        {
            get { return (string)GetValue(UncheckedToolTipProperty); }
            set { SetValue(UncheckedToolTipProperty, value); }
        }

        /// <summary>
        /// Handles changes to the UncheckedToolTip property.
        /// </summary>
        /// <param name="d">ToggleSwitch</param>
        /// <param name="e">DependencyProperty changed event arguments</param>
        private static void OnUncheckedToolTipChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ToggleSwitch tSwitch = (ToggleSwitch)d;
            string oldUncheckedToolTip = (string)e.OldValue;
            string newUncheckedToolTip = tSwitch.UncheckedToolTip;
            tSwitch.OnUncheckedToolTipChanged(oldUncheckedToolTip, newUncheckedToolTip);
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the UncheckedToolTip property.
        /// </summary>
        /// <param name="oldUncheckedToolTip">Old Value</param>
        /// <param name="newUncheckedToolTip">New Value</param>
        protected void OnUncheckedToolTipChanged(string oldUncheckedToolTip, string newUncheckedToolTip)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                if ((IsChecked == false) && (!IsNullOrWhiteSpace(newUncheckedToolTip)))
                {
                    ToolTip = new ToolTip() { Content = newUncheckedToolTip };
                }
            }));
        }

        #endregion

        #region UncheckedMargin

        /// <summary>
        /// UncheckedMargin Read-Only Dependency Property
        /// </summary>
        public static readonly DependencyPropertyKey UncheckedMarginPropertyKey =
            DependencyProperty.RegisterReadOnly("UncheckedMargin", typeof(Thickness), typeof(ToggleSwitch),
                new UIPropertyMetadata(new Thickness(-50, 0, 50, 0)));

        /// <summary>
        /// Map the UncheckedMarginProperty to the UncheckedMarginPropertyKey's DependencyProperty property.
        /// </summary>
        public static readonly DependencyProperty UncheckedMarginProperty = UncheckedMarginPropertyKey.DependencyProperty;

        /// <summary>
        /// Gets the UncheckedMargin property. This dependency property 
        /// indicates the Margin of the PART_ContentGrid when
        /// the ToggleSwitch is in Unchecked state.
        /// NOTE: This property can be set internally only (or by a class deriving from this class)
        /// </summary>
        public Thickness UncheckedMargin
        {
            get { return (Thickness)GetValue(UncheckedMarginProperty); }
            protected set { SetValue(UncheckedMarginPropertyKey, value); }
        }

        #endregion

        #region OptimizeRendering

        /// <summary>
        /// OptimizeRendering Dependency Property
        /// </summary>
        public static readonly DependencyProperty OptimizeRenderingProperty =
            DependencyProperty.Register("OptimizeRendering", typeof(bool), typeof(ToggleSwitch),
                new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary>
        /// Gets or sets the OptimizeRendering property. This dependency property 
        /// indicates whether the ClipBorder surrounding the ToggleSwitch should enable
        /// OptimizeClipRendering to prevent gaps between the border and the clipped content.
        /// Set this value to False, if the ToggleSwitch Background is Transparent or 
        /// has partial transparency. Otherwise, if the Background is opaque, 
        /// set this value to True for better rendering.
        /// </summary>
        public bool OptimizeRendering
        {
            get { return (bool)GetValue(OptimizeRenderingProperty); }
            set { SetValue(OptimizeRenderingProperty, value); }
        }

        #endregion

        #region OptimizeThumbRendering

        /// <summary>
        /// OptimizeThumbRendering Dependency Property
        /// </summary>
        public static readonly DependencyProperty OptimizeThumbRenderingProperty =
            DependencyProperty.Register("OptimizeThumbRendering", typeof(bool), typeof(ToggleSwitch),
                new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary>
        /// Gets or sets the OptimizeThumbRendering property. This dependency property 
        /// indicates whether the ClipBorder surrounding the ToggleSwitch Thumb should enable
        /// OptimizeClipRendering to prevent gaps between the border and the clipped content.
        /// Set this value to False, if the ToggleSwitch ThumbBackground is Transparent or 
        /// has partial transparency. Otherwise, if the ThumbBackground is opaque, 
        /// set this value to True for better rendering.
        /// </summary>
        public bool OptimizeThumbRendering
        {
            get { return (bool)GetValue(OptimizeThumbRenderingProperty); }
            set { SetValue(OptimizeThumbRenderingProperty, value); }
        }

        #endregion

        #region AutoThumbWidthRatio

        /// <summary>
        /// AutoThumbWidthRatio Dependency Property
        /// </summary>
        public static readonly DependencyProperty AutoThumbWidthRatioProperty =
            DependencyProperty.Register("AutoThumbWidthRatio", typeof(bool), typeof(ToggleSwitch),
                new FrameworkPropertyMetadata(true));

        /// <summary>
        /// Gets or sets the AutoThumbWidthRatio property. This dependency property 
        /// indicates whether the ThumbWidthRatio should be automatically calculated
        /// based on the Width, Height and BorderThickness of the ToggleSwitch. This property has
        /// higher precedence over the ThumbWidthRatio property. If this property is set to true,
        /// any value for ThumbWidthRatio, set by the user, will be ignored and ThumbWidth will
        /// be equal to ThumbHeight.
        /// </summary>
        public bool AutoThumbWidthRatio
        {
            get { return (bool)GetValue(AutoThumbWidthRatioProperty); }
            set { SetValue(AutoThumbWidthRatioProperty, value); }
        }

        #endregion

        #region AccentMode

        /// <summary>
        /// AccentMode Dependency Property
        /// </summary>
        public static readonly DependencyProperty AccentModeProperty =
            DependencyProperty.Register("AccentMode", typeof(AccentModeType), typeof(ToggleSwitch),
                new FrameworkPropertyMetadata(AccentModeType.System));

        /// <summary>
        /// Gets or sets the AccentMode property. This dependency property 
        /// indicates whether the ToggleSwitch should display the accent color (either
        /// obtained from system or specified by the user).
        /// </summary>
        public AccentModeType AccentMode
        {
            get { return (AccentModeType)GetValue(AccentModeProperty); }
            set { SetValue(AccentModeProperty, value); }
        }

        #endregion

        #region AccentBrush

        /// <summary>
        /// AccentBrush Dependency Property
        /// </summary>
        public static readonly DependencyProperty AccentBrushProperty =
            DependencyProperty.Register("AccentBrush", typeof(Brush), typeof(ToggleSwitch),
                new FrameworkPropertyMetadata(Brushes.Transparent, OnAccentBrushChanged, CoerceAccentBrush));

        /// <summary>
        /// Gets or sets the AccentBrush property. This dependency property 
        /// indicates he accent color for the ToggleSwitch Background.
        /// </summary>
        public Brush AccentBrush
        {
            get { return (Brush)GetValue(AccentBrushProperty); }
            set { SetValue(AccentBrushProperty, value); }
        }

        /// <summary>
        /// Handles changes to the AccentBrush property.
        /// </summary>
        private static void OnAccentBrushChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var toggleSwitch = (ToggleSwitch)d;
            var oldAccentBrush = (Brush)e.OldValue;
            var newAccentBrush = toggleSwitch.AccentBrush;
            toggleSwitch.OnAccentBrushChanged(oldAccentBrush, newAccentBrush);
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the AccentBrush property.
        /// </summary>
        protected void OnAccentBrushChanged(Brush oldAccentBrush, Brush newAccentBrush)
        {
        }

        /// <summary>
        /// Coerces the AccentBrush value.
        /// </summary>
        private static object CoerceAccentBrush(DependencyObject d, object value)
        {
            var toggleSwitch = (ToggleSwitch)d;
            var desiredAccentBrush = (Brush)value;

            if (toggleSwitch.AccentMode == AccentModeType.System)
            {
                var accentColor = GetAccentColor();
                if (accentColor != null)
                {
                    desiredAccentBrush = new SolidColorBrush(accentColor.Value);    
                }
            }

            return desiredAccentBrush;
        }

        #endregion

        #region ThumbAccentBrush

        /// <summary>
        /// ThumbAccentBrush Dependency Property
        /// </summary>
        public static readonly DependencyProperty ThumbAccentBrushProperty =
            DependencyProperty.Register("ThumbAccentBrush", typeof(Brush), typeof(ToggleSwitch),
                new FrameworkPropertyMetadata(Brushes.Transparent));

        /// <summary>
        /// Gets or sets the ThumbAccentBrush property. This dependency property 
        /// indicates the accent color for the ToggleSwitch Thumb.
        /// </summary>
        public Brush ThumbAccentBrush
        {
            get { return (Brush)GetValue(ThumbAccentBrushProperty); }
            set { SetValue(ThumbAccentBrushProperty, value); }
        }

        #endregion

        #endregion

        #region Construction

        /// <summary>
        /// Static ctor
        /// </summary>
        static ToggleSwitch()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ToggleSwitch), new FrameworkPropertyMetadata(typeof(ToggleSwitch)));
        }

        /// <summary>
        /// Default ctor
        /// </summary>
        public ToggleSwitch()
        {
            _isCalculatingLayout = false;

            // ===================================================================================
            // NOTE: In order to ensure proper disposal of the ToggleSwitch,
            // the following events must be unsubscribed to by calling the 'Cleanup' method from
            // window hosting the ToggleSwitch
            // ===================================================================================
            // Subscribe to the BorderThickness changed event to update the layout
            var dpd = DependencyPropertyDescriptor.FromProperty(BorderThicknessProperty, typeof(ToggleSwitch));
            dpd?.AddValueChanged(this, OnThicknessChanged);
            // Subscribe to the UserPreferenceChanged event to know if the user changes accent color
            SystemEvents.UserPreferenceChanged += OnUserPreferenceChanged;
        }

        #endregion

        #region Overrides

        /// <summary>
        /// Override which is called when the template is applied
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            // Get all the controls in the template
            GetTemplateParts();
        }

        /// <summary>
        /// Updates the layout during the Arrange phase
        /// </summary>
        /// <param name="arrangeBounds">Final size of the control</param>
        /// <returns>Final size</returns>
        protected override Size ArrangeOverride(Size arrangeBounds)
        {
            CalculateLayout(arrangeBounds.Width, arrangeBounds.Height);
            return base.ArrangeOverride(arrangeBounds);
        }

        /// <summary>
        /// Overridden handler for the event when the ToggleSwitch becomes Checked
        /// </summary>
        /// <param name="e">RoutedEventArgs</param>
        protected override void OnChecked(RoutedEventArgs e)
        {
            // Hide the tooltip if it is displayed
            var tt = (ToolTip)ToolTipService.GetToolTip(this);
            if (tt != null)
                tt.IsOpen = false;

            base.OnChecked(e);

            // Set the ToggleSwitch's ToolTip to CheckedToolTip property if it 
            // is not null or empty, else set the ToolTip to null.
            ToolTip = !IsNullOrWhiteSpace(CheckedToolTip) ? new ToolTip() { Content = CheckedToolTip } : null;
        }

        /// <summary>
        /// Overridden handler for the event when the ToggleSwitch becomes Unchecked
        /// </summary>
        /// <param name="e">RoutedEventArgs</param>
        protected override void OnUnchecked(RoutedEventArgs e)
        {
            // Hide the tooltip if it is displayed
            var tt = (ToolTip)ToolTipService.GetToolTip(this);
            if (tt != null)
                tt.IsOpen = false;

            base.OnUnchecked(e);

            // Set the ToggleSwitch's ToolTip to UncheckedToolTip property if it 
            // is not null or empty, else set the ToolTip to null.
            ToolTip = !IsNullOrWhiteSpace(UncheckedToolTip) ? new ToolTip() { Content = UncheckedToolTip } : null;
        }

        #endregion

        #region Helpers

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
        /// Gets the required controls in the template
        /// </summary>
        protected void GetTemplateParts()
        {
            // PART_RootCanvas
            _rootCanvas = GetChildControl<Canvas>("PART_RootCanvas");
            // PART_ContentGrid
            _contentGrid = GetChildControl<Grid>("PART_ContentGrid");
            // PART_CheckedBorder
            _checkedBorder = GetChildControl<Border>("PART_CheckedBorder");
            // PART_CheckedContent
            _checkedContent = GetChildControl<TextBlock>("PART_CheckedContent");
            // PART_UncheckedBorder
            _unCheckedBorder = GetChildControl<Border>("PART_UncheckedBorder");
            // PART_UncheckedContent
            _unCheckedContent = GetChildControl<TextBlock>("PART_UncheckedContent");
            // PART_OuterBorder
            _outerBorder = GetChildControl<ClipBorder>("PART_OuterBorder");
            if (_outerBorder != null)
            {
                // TS_Proxy will be within PART_OuterBorder's Resources if 
                // the ToggleSwitchBasicTemplate is applied to ToggleSwitch
                _tsProxy = _outerBorder.Resources["TS_Proxy"] as ToggleSwitchProxy;
                if (_tsProxy == null)
                {
                    // Otherwise, TS_Proxy will be within PART_OuterGrid's Resources if 
                    // the ToggleSwitchModernTemplate is applied to ToggleSwitch
                    _outerGrid = GetChildControl<Grid>("PART_OuterGrid");
                    if (_outerGrid != null)
                    {
                        _tsProxy = _outerGrid.Resources["TS_Proxy"] as ToggleSwitchProxy;
                    }
                }
            }
            // PART_CheckedKeyFrame
            _checkedKeyFrame = GetChildControl<EasingThicknessKeyFrame>("PART_CheckedKeyFrame");
            // PART_UncheckedKeyFrame
            _uncheckedKeyFrame = GetChildControl<EasingThicknessKeyFrame>("PART_UncheckedKeyFrame");
            // PART_ThumbContentGrid
            _thumbContentGrid = GetChildControl<Grid>("PART_ThumbContentGrid");
        }

        /// <summary>
        /// Updates the contents of the ToggleSwitch based on IsCheckedLeft flag.
        /// If it is true, then the Checked content would be in the left side, 
        /// the Unchecked content in the right side and vice-versa if it is false.
        /// </summary>
        /// <param name="isCheckedLeft"></param>
        private void UpdateToggleSwitchContents(bool isCheckedLeft)
        {
            if (isCheckedLeft)
            {
                if (_checkedBorder != null)
                    Grid.SetColumn(_checkedBorder, 0);
                if (_checkedContent != null)
                    Grid.SetColumn(_checkedContent, 0);
                if (_unCheckedBorder != null)
                    Grid.SetColumn(_unCheckedBorder, 2);
                if (_unCheckedContent != null)
                    Grid.SetColumn(_unCheckedContent, 3);
            }
            else
            {
                if (_checkedBorder != null)
                    Grid.SetColumn(_checkedBorder, 2);
                if (_checkedContent != null)
                    Grid.SetColumn(_checkedContent, 3);
                if (_unCheckedBorder != null)
                    Grid.SetColumn(_unCheckedBorder, 0);
                if (_unCheckedContent != null)
                    Grid.SetColumn(_unCheckedContent, 0);
            }
        }

        /// <summary>
        /// Calculates the layout for the ToggleSwitch -
        /// 1. Calculates ThumbWidthRatio if AutoThumbWidthRatio is set to true
        /// 2. Calculates the coerced CornerRadius of the ToggleSwitch
        /// 3. Calculates the width and margin of the PART_ContentGrid and also the 
        ///    width of the columns within it.
        ///    The following calculation is used: (Here p is the value of ThumbWidth)
        ///        p = [10, 90]
        ///        Slide distance = 1 - p
        ///        Total Width = (1 - p) + p + (1 - p) = (2 - p)
        ///        Left = (1 - p)/(2 - p)
        ///        Right = (1 - p)/(2 - p)
        ///        CenterLeft = 0.5 - Left
        ///        CenterRight = 0.5 - Right
        /// 4. Update GridContent children based on the IsCheckedLeft property
        /// 5. Calculates the values for the CheckedMargin and UncheckedMargin
        /// 6. Calculates the Height of the Thumb which can fit the current ToggleSwitch
        ///    dimensions.
        /// 7a. Calculates the ThumbCornerRadius based on the ToggleSwitch's CornerRadius
        ///     & BorderThickness and the ThumbBorderThickness.
        /// 7b. Coerces the ThumbCornerRadius based on the Thumb Width, Height & 
        ///     ThumbBorderThickness
        /// 8. Update the animation target values of the Checked and Unchecked Visual
        ///    States (defined in the default ToggleSwitch ControlTemplate)
        /// </summary>
        private void CalculateLayout(double ctrlWidth, double ctrlHeight)
        {
            if ((_rootCanvas == null) || (_contentGrid == null))
                return;

            var borders = BorderThickness;
            var corners = CornerRadius;
            var autoThumb = AutoThumbWidthRatio;
            var isCheckedLeft = IsCheckedLeft;

            var innerRect = new Rect(0, 0, ctrlWidth, ctrlHeight).Deflate(borders);

            // 1. Should ThumbWidthRatio be calculated automatically?
            if (autoThumb)
            {
                // ThumbWidth should be same as ThumbHeight
                ThumbWidthRatio = innerRect.Height / innerRect.Width;
            }

            // 2. Calculate the coerced CornerRadius of the ToggleSwitch
            _isCalculatingLayout = true; // Set the flag so that when CornerRadius changes, InvalidateVisual is not called again 
            CornerRadius = GetCoercedCornerRadius(ctrlWidth, ctrlHeight, borders, corners);
            _isCalculatingLayout = false;

            // 3. Calculate the relative width of the ContentGrid Columns
            var leftRight = (1 - ThumbWidthRatio) / (2 - ThumbWidthRatio);
            var centerLeftRight = 0.5 - leftRight;
            _contentGrid.ColumnDefinitions[0].Width = new GridLength(leftRight, GridUnitType.Star);
            _contentGrid.ColumnDefinitions[1].Width = new GridLength(centerLeftRight, GridUnitType.Star);
            _contentGrid.ColumnDefinitions[2].Width = new GridLength(centerLeftRight, GridUnitType.Star);
            _contentGrid.ColumnDefinitions[3].Width = new GridLength(leftRight, GridUnitType.Star);

            // Calculate the dimensions of the ContentGrid
            _contentGrid.Width = (2 - ThumbWidthRatio) * innerRect.Width;
            _contentGrid.Height = innerRect.Height;

            // 4. Update GridContent children based on the IsCheckedLeft property
            UpdateToggleSwitchContents(isCheckedLeft);

            // 5. Calculate the Slide distance and set the Checked and Unchecked margins
            var slideDistance = Math.Round(leftRight * _contentGrid.Width);
            if (IsCheckedLeft)
            {
                CheckedMargin = new Thickness(0, 0, 0, 0);
                UncheckedMargin = new Thickness(-slideDistance, 0, slideDistance, 0);
                if (IsChecked == false)
                {
                    _contentGrid.Margin = UncheckedMargin;
                }
            }
            else
            {
                CheckedMargin = new Thickness(-slideDistance, 0, slideDistance, 0);
                UncheckedMargin = new Thickness(0, 0, 0, 0);
                if (IsChecked == true)
                {
                    _contentGrid.Margin = CheckedMargin;
                }
            }

            // 6. Calculate the appropriate dimensions of the Thumb 
            ThumbWidth = (centerLeftRight * 2) * _contentGrid.Width;
            ThumbHeight = _contentGrid.Height;

            // 7. Calculate the ThumbCornerRadius and Coerce it to appropriate value
            CalculateCoercedThumbCornerRadius();

            // Calculate the dimensions of the ThumbContent Grid
            _thumbContentGrid.Width = ThumbWidth - ThumbBorderThickness.Left - ThumbBorderThickness.Right;
            _thumbContentGrid.Height = ThumbHeight - ThumbBorderThickness.Top - ThumbBorderThickness.Bottom;

            // 8. IMPORTANT: The following code is required to refresh the binding value of the Checked & 
            // Unchecked Visual State animations (defined in the ToggleSwitch default ControlTemplate)
            // ==== Start ============================================================================

            if (_tsProxy != null)
            {
                _tsProxy.CheckedMargin = CheckedMargin;
                _tsProxy.UncheckedMargin = UncheckedMargin;
                if (_checkedKeyFrame != null)
                {
                    // NOTE: This commented code will also work when the application is running, but it will not work in the VS Designer.
                    // Therefore the existing binding has to be replaced with a new one instead of refreshing the binding target.

                    //var binding = BindingOperations.GetBindingExpression(_checkedKeyFrame, EasingThicknessKeyFrame.ValueProperty);
                    //binding?.UpdateTarget();

                    var binding = new Binding
                    {
                        Source = _tsProxy,
                        Path = new PropertyPath("CheckedMargin")
                    };
                    BindingOperations.SetBinding(_checkedKeyFrame, EasingThicknessKeyFrame.ValueProperty, binding);
                }
                if (_uncheckedKeyFrame != null)
                {
                    // NOTE: This commented code will also work when the application is running, but it will not work in the VS Designer.
                    // Therefore the existing binding has to be replaced with a new one instead of refreshing the binding target.

                    //var binding = BindingOperations.GetBindingExpression(_uncheckedKeyFrame, EasingThicknessKeyFrame.ValueProperty);
                    //binding?.UpdateTarget();

                    var binding = new Binding
                    {
                        Source = _tsProxy,
                        Path = new PropertyPath("UncheckedMargin")
                    };
                    BindingOperations.SetBinding(_uncheckedKeyFrame, EasingThicknessKeyFrame.ValueProperty, binding);
                }
            }

            // Refresh the Visual state of the ToggleSwitch
            VisualStateManager.GoToState(this, "Indeterminate", false);
            VisualStateManager.GoToState(this, (IsChecked == true) ? "Checked" : "Unchecked", false);

            // ==== End ==============================================================================
        }

        /// <summary>
        /// Calculates the CornerRadius of the Thumb based on the following formula
        /// 
        ///     CR(T) + (BT(T) / 2) = CR(TS) - (BT(TS) / 2)
        /// 
        /// where
        ///     CR(T) = Corner Radius of the Thumb
        ///     BT(T) = Border Thickness of the Thumb
        ///     CR(TS) = Corner Radius of the ToggleSwitch
        ///     BT(TS) = Border Thickness of the ToggleSwitch
        ///   
        /// NOTE: If the BorderThickness of the Thumb or the ToggleSwitch is not 
        /// uniform, then this may NOT produce accurate results.
        /// </summary>
        private void CalculateCoercedThumbCornerRadius()
        {
            var topLeft = CornerRadius.TopLeft - (BorderThickness.Left / 2) - (ThumbBorderThickness.Left / 2);
            var topRight = CornerRadius.TopRight - (BorderThickness.Right / 2) - (ThumbBorderThickness.Right / 2);
            var bottomRight = CornerRadius.BottomRight - (BorderThickness.Right / 2) - (ThumbBorderThickness.Right / 2);
            var bottomLeft = CornerRadius.BottomLeft - (BorderThickness.Left / 2) - (ThumbBorderThickness.Left / 2);

            var desiredCornerRadius = new CornerRadius(topLeft, topRight, bottomRight, bottomLeft);
            // Get the Coerced value
            ThumbCornerRadius = GetCoercedCornerRadius(ThumbWidth, ThumbHeight, ThumbBorderThickness,
                desiredCornerRadius);
        }

        /// <summary>
        /// Calculates the optimal corner radius of the control based on the Width, Height,
        /// BorderThickness and the desired CornerRadius
        /// </summary>
        /// <param name="ctrlWidth">Width of the control</param>
        /// <param name="ctrlHeight">Height of the control</param>
        /// <param name="ctrlBorderThickness">Border Thickness of the control</param>
        /// <param name="desiredCornerRadius">CornerRadius desired for the control</param>
        /// <returns>Optimized CornerRadius</returns>
        private static CornerRadius GetCoercedCornerRadius(double ctrlWidth, double ctrlHeight,
            Thickness ctrlBorderThickness, CornerRadius desiredCornerRadius)
        {
            var width = ctrlWidth - (ctrlBorderThickness.Left / 2) - (ctrlBorderThickness.Right / 2);
            var height = ctrlHeight - (ctrlBorderThickness.Top / 2) - (ctrlBorderThickness.Bottom / 2);
            var topLeft = desiredCornerRadius.TopLeft;
            var topRight = desiredCornerRadius.TopRight;
            var bottomRight = desiredCornerRadius.BottomRight;
            var bottomLeft = desiredCornerRadius.BottomLeft;

            // Compare the corners against the smaller of the two - Width or Height.
            // Because if we optimize the CornerRadii against the smaller, it will
            // obviously fit in the larger
            if (width > height)
            {
                if ((topLeft + bottomLeft) > height)
                {
                    var top = (topLeft / (topLeft + bottomLeft)) * height;
                    var bottom = (bottomLeft / (topLeft + bottomLeft)) * height;
                    topLeft = top;
                    bottomLeft = bottom;
                }

                if ((topRight + bottomRight) > height)
                {
                    var top = (topRight / (topRight + bottomRight)) * height;
                    var bottom = (bottomRight / (topRight + bottomRight)) * height;
                    topRight = top;
                    bottomRight = bottom;
                }
            }
            else
            {
                if ((topLeft + topRight) > width)
                {
                    var left = (topLeft / (topLeft + topRight)) * width;
                    var right = (topRight / (topLeft + topRight)) * width;
                    topLeft = left;
                    topRight = right;
                }

                if ((bottomLeft + bottomRight) > width)
                {
                    var left = (bottomLeft / (bottomLeft + bottomRight)) * width;
                    var right = (bottomRight / (bottomLeft + bottomRight)) * width;
                    bottomLeft = left;
                    bottomRight = right;
                }
            }

            return new CornerRadius(topLeft, topRight, bottomRight, bottomLeft);
        }

        #endregion

        #region InterOps

        [DllImport("dwmapi.dll")]
        private static extern int DwmIsCompositionEnabled([MarshalAs(UnmanagedType.Bool)] out bool pfEnabled);

        [DllImport("dwmapi.dll", PreserveSig = false)]
        private static extern void DwmGetColorizationColor(out uint colorizationColor, [MarshalAs(UnmanagedType.Bool)]out bool colorizationOpaqueBlend);

        /// <summary>
        /// Obtains the current Accent Color
        /// </summary>
        /// <returns>Current Accent Color</returns>
        public static Color? GetAccentColor()
        {
            bool isEnabled;
            var hr1 = DwmIsCompositionEnabled(out isEnabled);
            if ((hr1 != 0) || !isEnabled) // 0 means S_OK.
                return null;

            try
            {
                uint colorizationColor;
                bool opaqueBlend;
                DwmGetColorizationColor(out colorizationColor, out opaqueBlend);

                // Convert colorization color parameter to Color.
                var colorbytes = new byte[4];
                colorbytes[0] = (byte)((0xFF000000 & colorizationColor) >> 24); // A
                // When the color preference is set to be automatically picked from background
                // the the Alpha value is returned as 0. In that case set it to 255 to get the solid color
                if (colorbytes[0] == 0)
                    colorbytes[0] = 255;
                colorbytes[1] = (byte)((0x00FF0000 & colorizationColor) >> 16); // R
                colorbytes[2] = (byte)((0x0000FF00 & colorizationColor) >> 8); // G
                colorbytes[3] = (byte)(0x000000FF & colorizationColor); // B
                var targetColor = Color.FromArgb(colorbytes[0], colorbytes[1], colorbytes[2], colorbytes[3]);

                return targetColor;
            }
            catch
            {
                return null;
            }
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handles the Thickness Changed Event to update the layout
        /// </summary>
        /// <param name="sender">ToggleSwitch</param>
        /// <param name="e">EventArgs</param>
        private void OnThicknessChanged(object sender, EventArgs e)
        {
            // Update Layout
            InvalidateVisual();
        }

        /// <summary>
        /// Handles the UserPreferenceChanged System Event
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">EventArgs</param>
        private void OnUserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
        {
            // The Category is General when the user changes color preference
            if ((AccentMode == AccentModeType.System) &&(e.Category == UserPreferenceCategory.General))
            {
                // Set the AccentBrush to any value to reload the system accent color into ToggleSwitch
                AccentBrush = Brushes.Red;
            }
        }

        #endregion

        #region APIs

        /// <summary>
        /// This method should be called by the Window hosting the ToggleSwitch, when the window is closed.
        /// This will ensure proper disposal of the ToggleSwitch object.
        /// </summary>
        public void Cleanup()
        {
            var dpd = DependencyPropertyDescriptor.FromProperty(BorderThicknessProperty, typeof(ToggleSwitch));
            dpd?.RemoveValueChanged(this, OnThicknessChanged);

            SystemEvents.UserPreferenceChanged -= OnUserPreferenceChanged;
        }

        #endregion
    }

    /// <summary>
    /// Proxy class to provide the properties of the ToggleSwitch as Freezable.
    /// These properties are used in the animations which happen during Visual
    /// State changes.
    /// </summary>
    internal class ToggleSwitchProxy : Freezable
    {
        #region Freezable Overrides

        protected override Freezable CreateInstanceCore()
        {
            return new ToggleSwitchProxy();
        }

        #endregion

        #region CheckedMargin

        /// <summary>
        /// CheckedMargin Dependency Property
        /// </summary>
        public static readonly DependencyProperty CheckedMarginProperty =
            DependencyProperty.Register("CheckedMargin", typeof(Thickness), typeof(ToggleSwitchProxy),
                new FrameworkPropertyMetadata(new Thickness()));

        /// <summary>
        /// Gets or sets the CheckedMargin property. This dependency property 
        /// indicates the frozen value of the CheckedMargin property of ToggleSwitch.
        /// It is used to bind the value of the KeyFrame for the animation (which
        /// is shown when the ToggleSwitch goes to the Checked Visual State)
        /// </summary>
        public Thickness CheckedMargin
        {
            get { return (Thickness)GetValue(CheckedMarginProperty); }
            set { SetValue(CheckedMarginProperty, value); }
        }

        #endregion

        #region UncheckedMargin

        /// <summary>
        /// UncheckedMargin Dependency Property
        /// </summary>
        public static readonly DependencyProperty UncheckedMarginProperty =
            DependencyProperty.Register("UncheckedMargin", typeof(Thickness), typeof(ToggleSwitchProxy),
                new FrameworkPropertyMetadata(new Thickness()));

        /// <summary>
        /// Gets or sets the UncheckedMargin property. This dependency property 
        /// indicates the frozen value of the Unchecked property of ToggleSwitch.
        /// It is used to bind the value of the KeyFrame for the animation (which
        /// is shown when the ToggleSwitch goes to the Unchecked Visual State)
        /// </summary>
        public Thickness UncheckedMargin
        {
            get { return (Thickness)GetValue(UncheckedMarginProperty); }
            set { SetValue(UncheckedMarginProperty, value); }
        }

        #endregion
    }
}
