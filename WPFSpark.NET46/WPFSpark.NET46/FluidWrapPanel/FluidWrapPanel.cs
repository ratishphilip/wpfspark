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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace WPFSpark
{
    public sealed class FluidWrapPanel : Panel
    {
        #region Constants

        public const double NORMAL_SCALE = 1.0d;
        public const double DRAG_SCALE_DEFAULT = 1.3d;
        public const double NORMAL_OPACITY = 1.0d;
        public const double DRAG_OPACITY_DEFAULT = 0.6d;
        public const double OPACITY_MIN = 0.1d;
        public const double DEFAULT_ITEM_WIDTH = 10.0;
        public const double DEFAULT_ITEM_HEIGHT = 10.0;
        public const Int32 Z_INDEX_NORMAL = 0;
        public const Int32 Z_INDEX_INTERMEDIATE = 1;
        public const Int32 Z_INDEX_DRAG = 10;
        public static TimeSpan DEFAULT_ANIMATION_TIME_WITHOUT_EASING = TimeSpan.FromMilliseconds(200);
        public static TimeSpan DEFAULT_ANIMATION_TIME_WITH_EASING = TimeSpan.FromMilliseconds(400);
        public static TimeSpan FIRST_TIME_ANIMATION_DURATION = TimeSpan.FromMilliseconds(320);

        #endregion

        #region Structures

        private struct BitSize
        {
            internal int Width;
            internal int Height;
        }

        #endregion

        #region Fields

        private Point _dragStartPoint;
        private UIElement _dragElement;
        private UIElement _lastDragElement;
        //private readonly ObservableCollection<UIElement> FluidItems;
        private bool _isOptimized;
        private Size _panelSize;
        private int _cellsPerLine;
        private Dictionary<UIElement, Rect> _bounds;
        private UIElement _lastExchangeElement;

        #endregion

        #region Dependency Properties

        #region DragEasing

        /// <summary>
        /// DragEasing Dependency Property
        /// </summary>
        public static readonly DependencyProperty DragEasingProperty =
            DependencyProperty.Register("DragEasing", typeof(EasingFunctionBase), typeof(FluidWrapPanel));

        /// <summary>
        /// Gets or sets the DragEasing property. This dependency property 
        /// indicates the Easing function to be used when the user stops dragging the child and releases it.
        /// </summary>
        public EasingFunctionBase DragEasing
        {
            get { return (EasingFunctionBase)GetValue(DragEasingProperty); }
            set { SetValue(DragEasingProperty, value); }
        }

        #endregion

        #region DragOpacity

        /// <summary>
        /// DragOpacity Dependency Property
        /// </summary>
        public static readonly DependencyProperty DragOpacityProperty =
            DependencyProperty.Register("DragOpacity", typeof(double), typeof(FluidWrapPanel),
                new FrameworkPropertyMetadata(DRAG_OPACITY_DEFAULT, OnDragOpacityChanged, CoerceDragOpacity));

        /// <summary>
        /// Gets or sets the DragOpacity property. This dependency property 
        /// indicates the opacity of the child being dragged.
        /// </summary>
        public double DragOpacity
        {
            get { return (double)GetValue(DragOpacityProperty); }
            set { SetValue(DragOpacityProperty, value); }
        }

        /// <summary>
        /// Coerces the FluidDrag Opacity to an acceptable value
        /// </summary>
        /// <param name="d">Dependency Object</param>
        /// <param name="value">Value</param>
        /// <returns>Coerced Value</returns>
        private static object CoerceDragOpacity(DependencyObject d, object value)
        {
            var opacity = (double)value;

            if (opacity < OPACITY_MIN)
            {
                opacity = OPACITY_MIN;
            }
            else if (opacity > NORMAL_OPACITY)
            {
                opacity = NORMAL_OPACITY;
            }

            return opacity;
        }

        /// <summary>
        /// Handles changes to the DragOpacity property.
        /// </summary>
        /// <param name="d">FluidWrapPanel</param>
        /// <param name="e">DependencyProperty changed event arguments</param>
        private static void OnDragOpacityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        }

        // TODO: Add ValidateDragOpacity
        #endregion

        #region DragScale

        /// <summary>
        /// DragScale Dependency Property
        /// </summary>
        public static readonly DependencyProperty DragScaleProperty =
            DependencyProperty.Register("DragScale", typeof(double), typeof(FluidWrapPanel),
                new FrameworkPropertyMetadata(DRAG_SCALE_DEFAULT));

        /// <summary>
        /// Gets or sets the DragScale property. This dependency property 
        /// indicates the factor by which the child should be scaled when it is dragged.
        /// </summary>
        public double DragScale
        {
            get { return (double)GetValue(DragScaleProperty); }
            set { SetValue(DragScaleProperty, value); }
        }

        #endregion

        #region ElementEasing

        /// <summary>
        /// ElementEasing Dependency Property
        /// </summary>
        public static readonly DependencyProperty ElementEasingProperty =
            DependencyProperty.Register("ElementEasing", typeof(EasingFunctionBase), typeof(FluidWrapPanel));

        /// <summary>
        /// Gets or sets the ElementEasing property. This dependency property 
        /// indicates the Easing Function to be used when the elements are rearranged.
        /// </summary>
        public EasingFunctionBase ElementEasing
        {
            get { return (EasingFunctionBase)GetValue(ElementEasingProperty); }
            set { SetValue(ElementEasingProperty, value); }
        }

        #endregion

        #region FluidItems

        /// <summary>
        /// FluidItems Read-Only Dependency Property
        /// </summary>
        public static readonly DependencyPropertyKey FluidItemsPropertyKey =
            DependencyProperty.RegisterReadOnly("FluidItems", typeof(ObservableCollection<UIElement>), typeof(FluidWrapPanel),
                new FrameworkPropertyMetadata(null));

        /// <summary>
        /// Map the FluidItemsProperty to the FluidItemsPropertyKey's DependencyProperty property.
        /// </summary>
        public static readonly DependencyProperty FluidItemsProperty = FluidItemsPropertyKey.DependencyProperty;

        /// <summary>
        /// Gets the FluidItems property. This dependency property 
        /// indicates the observable list of FluidWrapPanel's children.
        /// NOTE: This property can be set internally only (or by a class deriving from this class)
        /// </summary>
        public ObservableCollection<UIElement> FluidItems
        {
            get { return (ObservableCollection<UIElement>)GetValue(FluidItemsProperty); }
            private set { SetValue(FluidItemsPropertyKey, value); }
        }

        #endregion

        #region IsComposing

        /// <summary>
        /// IsComposing Dependency Property
        /// </summary>
        public static readonly DependencyProperty IsComposingProperty =
            DependencyProperty.Register("IsComposing", typeof(bool), typeof(FluidWrapPanel));

        /// <summary>
        /// Gets or sets the IsComposing property. This dependency property 
        /// indicates if the FluidWrapPanel is in Composing mode.
        /// </summary>
        public bool IsComposing
        {
            get { return (bool)GetValue(IsComposingProperty); }
            set { SetValue(IsComposingProperty, value); }
        }

        #endregion

        #region ItemHeight

        /// <summary>
        /// ItemHeight Dependency Property
        /// </summary>
        public static readonly DependencyProperty ItemHeightProperty =
            DependencyProperty.Register("ItemHeight", typeof(double), typeof(FluidWrapPanel),
                new FrameworkPropertyMetadata(DEFAULT_ITEM_HEIGHT,
                    FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender), OnValidateItemHeight);

        /// <summary>
        /// Gets or sets the ItemHeight property. This dependency property 
        /// indicates the height of each item.
        /// </summary>
        public double ItemHeight
        {
            get { return (double)GetValue(ItemHeightProperty); }
            set { SetValue(ItemHeightProperty, value); }
        }

        /// <summary>
        /// Checks if the given ItemHeight is a valid positive value
        /// </summary>
        /// <param name="value">Height</param>
        /// <returns></returns>
        private static bool OnValidateItemHeight(object value)
        {
            var height = (double)value;
            return height > 0.0;
        }

        #endregion

        #region ItemWidth

        /// <summary>
        /// ItemWidth Dependency Property
        /// </summary>
        public static readonly DependencyProperty ItemWidthProperty =
            DependencyProperty.Register("ItemWidth", typeof(double), typeof(FluidWrapPanel),
                new FrameworkPropertyMetadata(DEFAULT_ITEM_WIDTH,
                    FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender), OnValidateItemWidth);

        /// <summary>
        /// Gets or sets the ItemWidth property. This dependency property 
        /// indicates the width of each item.
        /// </summary>
        public double ItemWidth
        {
            get { return (double)GetValue(ItemWidthProperty); }
            set { SetValue(ItemWidthProperty, value); }
        }

        /// <summary>
        /// Checks if the given ItemWidth is a valid positive value
        /// </summary>
        /// <param name="value">Width</param>
        /// <returns></returns>
        private static bool OnValidateItemWidth(object value)
        {
            var width = (double)value;
            return width > 0.0;
        }

        #endregion

        #region ItemsSource

        /// <summary>
        /// ItemsSource Dependency Property
        /// </summary>
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register("ItemsSource", typeof(IEnumerable), typeof(FluidWrapPanel),
                new FrameworkPropertyMetadata(OnItemsSourceChanged));

        /// <summary>
        /// Gets or sets the ItemsSource property. This dependency property 
        /// indicates the bindable collection.
        /// </summary>
        public IEnumerable ItemsSource
        {
            get { return (ObservableCollection<UIElement>)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        /// <summary>
        /// Handles changes to the ItemsSource property.
        /// </summary>
        /// <param name="d">FluidWrapPanel</param>
        /// <param name="e">DependencyProperty changed event arguments</param>
        private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var panel = (FluidWrapPanel)d;
            var oldItemsSource = (ObservableCollection<UIElement>)e.OldValue;
            var newItemsSource = panel.ItemsSource;
            panel.OnItemsSourceChanged(oldItemsSource, newItemsSource);
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the ItemsSource property.
        /// </summary>
        /// <param name="oldItemsSource">Old Value</param>
        /// <param name="newItemsSource">New Value</param>
        private void OnItemsSourceChanged(IEnumerable oldItemsSource, IEnumerable newItemsSource)
        {
            // Clear the previous items in the Children property
            ClearItemsSource();

            // Add the new children
            foreach (UIElement child in newItemsSource)
            {
                Children.Add(child);
            }

            // Refresh Layout
            InvalidateVisual();
        }

        #endregion

        #region OptimizeChildPlacement

        /// <summary>
        /// OptimizeChildPlacement Dependency Property
        /// </summary>
        public static readonly DependencyProperty OptimizeChildPlacementProperty =
            DependencyProperty.Register("OptimizeChildPlacement", typeof(bool), typeof(FluidWrapPanel),
                new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary>
        /// Gets or sets the OptimizeChildPlacement property. This dependency property 
        /// indicates whether the placement of the children is optimized. 
        /// If set to true, the child is placed at the first available position from 
        /// the beginning of the FluidWrapPanel. 
        /// If set to false, each child occupies the same (or greater) row and/or column
        /// than the previous child.
        /// </summary>
        public bool OptimizeChildPlacement
        {
            get { return (bool)GetValue(OptimizeChildPlacementProperty); }
            set { SetValue(OptimizeChildPlacementProperty, value); }
        }

        #endregion

        #region Orientation

        /// <summary>
        /// Orientation Dependency Property
        /// </summary>
        public static readonly DependencyProperty OrientationProperty =
            DependencyProperty.Register("Orientation", typeof(Orientation), typeof(FluidWrapPanel),
                new FrameworkPropertyMetadata(Orientation.Horizontal, OnOrientationChanged));

        /// <summary>
        /// Gets or sets the Orientation property. This dependency property 
        /// indicates the orientation of arrangement of items in the panel.
        /// </summary>
        public Orientation Orientation
        {
            get { return (Orientation)GetValue(OrientationProperty); }
            set { SetValue(OrientationProperty, value); }
        }

        /// <summary>
        /// Handles changes to the Orientation property.
        /// </summary>
        /// <param name="d">FluidWrapPanel</param>
        /// <param name="e">DependencyProperty changed event arguments</param>
        private static void OnOrientationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var panel = (FluidWrapPanel)d;
            var oldOrientation = (Orientation)e.OldValue;
            var newOrientation = panel.Orientation;
            panel.OnOrientationChanged(oldOrientation, newOrientation);
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the Orientation property.
        /// </summary>
        /// <param name="oldOrientation">Old Value</param>
        /// <param name="newOrientation">New Value</param>
        private void OnOrientationChanged(Orientation oldOrientation, Orientation newOrientation)
        {
            InvalidateVisual();
        }

        #endregion

        #endregion

        #region Construction / Initialization

        /// <summary>
        /// Default Ctor
        /// </summary>
        public FluidWrapPanel()
        {
            FluidItems = new ObservableCollection<UIElement>();
            _bounds = new Dictionary<UIElement, Rect>();
            _lastDragElement = null;
            _lastExchangeElement = null;
        }

        #endregion

        #region Overrides

        protected override Size MeasureOverride(Size availableSize)
        {
            var availableItemSize = new Size(Double.PositiveInfinity, Double.PositiveInfinity);

            // Iterate through all the UIElements in the Children collection
            for (var i = 0; i < InternalChildren.Count; i++)
            {
                var child = InternalChildren[i];
                if (child == null)
                    continue;

                // Ask the child how much size it needs
                child.Measure(availableItemSize);
                // Check if the child is already added to the fluidElements collection
                if (FluidItems.Contains(child))
                    continue;

                // Add the child to the fluidElements collection
                FluidItems.Add(child);
                // Initialize its RenderTransform
                child.RenderTransform = CreateTransform(-ItemWidth, -ItemHeight, NORMAL_SCALE, NORMAL_SCALE);
            }

            var cellSize = new Size(ItemWidth, ItemHeight);

            if ((availableSize.Width < 0.0d) || (availableSize.Width.IsZero())
                || (availableSize.Height < 0.0d) || (availableSize.Height.IsZero())
                || !FluidItems.Any())
                return cellSize;

            // Calculate how many unit cells can fit in the given width (or height) when the 
            // Orientation is Horizontal (or Vertical)
            _cellsPerLine = CalculateCellsPerLine(availableSize, cellSize, Orientation);
            // Convert the children's dimensions from Size to BitSize
            var childData = FluidItems.Select(child => new BitSize
            {
                Width = Math.Max(1, (int)Math.Floor((child.DesiredSize.Width / cellSize.Width) + 0.5)),
                Height = Math.Max(1, (int)Math.Floor((child.DesiredSize.Height / cellSize.Height) + 0.5))
            }).ToList();
             
            // If all the children have the same size as the cellSize then use optimized code
            // when a child is being dragged
            _isOptimized = !childData.Any(c => (c.Width != 1) || (c.Height != 1));

            var matrixWidth = 0;
            var matrixHeight = 0;
            if (Orientation == Orientation.Horizontal)
            {
                // If the maximum width required by a child is more than the calculated cellsPerLine, then
                // the matrix width should be the maximum width of that child
                matrixWidth = Math.Max(childData.Max(s => s.Width), _cellsPerLine);
                // For purpose of calculating the true size of the panel, the height of the matrix must
                // be set to the cumulative height of all the children
                matrixHeight = childData.Sum(s => s.Height);
            }
            else
            {
                // For purpose of calculating the true size of the panel, the width of the matrix must
                // be set to the cumulative width of all the children
                matrixWidth = childData.Sum(s => s.Width);
                // If the maximum height required by a child is more than the calculated cellsPerLine, then
                // the matrix height should be the maximum height of that child
                matrixHeight = Math.Max(childData.Max(s => s.Height), _cellsPerLine);
            }

            // Create FluidBitMatrix to calculate the size required by the panel
            var matrix = new FluidBitMatrix(matrixHeight, matrixWidth, Orientation);

            var startIndex = 0L;

            foreach (var child in childData)
            {
                var location = matrix.FindRegion(startIndex, child.Width, child.Height);
                if (location.IsValid())
                {
                    matrix.SetRegion(location, child.Width, child.Height);
                }

                // Update the startIndex so that the next child occupies a location the same (or greater)
                // row and/or column as this child
                if (!OptimizeChildPlacement)
                {
                    startIndex = (Orientation == Orientation.Horizontal) ? location.Row : location.Col;
                }
            }

            var matrixSize = matrix.GetFilledMatrixDimensions();

            return new Size(matrixSize.Width * cellSize.Width, matrixSize.Height * cellSize.Height);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            var cellSize = new Size(ItemWidth, ItemHeight);

            if ((finalSize.Width < 0.0d) || (finalSize.Width.IsZero())
                || (finalSize.Height < 0.0d) || (finalSize.Height.IsZero()))
            {
                finalSize = cellSize;
            }

            _panelSize = finalSize;

            if (!FluidItems.Any())
            {
                return finalSize;
            }

            // Calculate how many unit cells can fit in the given width (or height) when the 
            // Orientation is Horizontal (or Vertical)
            _cellsPerLine = CalculateCellsPerLine(finalSize, cellSize, Orientation);
            // Convert the children's dimensions from Size to BitSize
            var childData = FluidItems.ToDictionary(child => child, child => new BitSize
            {
                Width = Math.Max(1, (int)Math.Floor((child.DesiredSize.Width / cellSize.Width) + 0.5)),
                Height = Math.Max(1, (int)Math.Floor((child.DesiredSize.Height / cellSize.Height) + 0.5))
            }).ToList();

            // If all the children have the same size as the cellSize then use optimized code
            // when a child is being dragged
            _isOptimized = !childData.Any(c => (c.Value.Width != 1) || (c.Value.Height != 1));

            // Calculate matrix dimensions
            var matrixWidth = 0;
            var matrixHeight = 0;
            if (Orientation == Orientation.Horizontal)
            {
                // If the maximum width required by a child is more than the calculated cellsPerLine, then
                // the matrix width should be the maximum width of that child
                matrixWidth = Math.Max(childData.Max(s => s.Value.Width), _cellsPerLine);
                // For purpose of calculating the true size of the panel, the height of the matrix must
                // be set to the cumulative height of all the children
                matrixHeight = childData.Sum(s => s.Value.Height);
            }
            else
            {
                // For purpose of calculating the true size of the panel, the width of the matrix must
                // be set to the cumulative width of all the children
                matrixWidth = childData.Sum(s => s.Value.Width);
                // If the maximum height required by a child is more than the calculated cellsPerLine, then
                // the matrix height should be the maximum height of that child
                matrixHeight = Math.Max(childData.Max(s => s.Value.Height), _cellsPerLine);
            }

            // Create FluidBitMatrix to calculate the size required by the panel
            var matrix = new FluidBitMatrix(matrixHeight, matrixWidth, Orientation);

            var startIndex = 0L;
            _bounds.Clear();

            foreach (var child in childData)
            {
                var location = matrix.FindRegion(startIndex, child.Value.Width, child.Value.Height);
                if (location.IsValid())
                {
                    // Set the bits
                    matrix.SetRegion(location, child.Value.Width, child.Value.Height);
                    // Arrange the child
                    child.Key.Arrange(new Rect(0, 0, child.Key.DesiredSize.Width, child.Key.DesiredSize.Height));
                    // Convert MatrixCell location to actual location
                    var pos = new Point(location.Col * cellSize.Width, location.Row * cellSize.Height);
                    _bounds[child.Key] = new Rect(pos, child.Key.DesiredSize);

                    if (child.Key != _dragElement)
                    {
                        // Animate the child to the new location
                        CreateTransitionAnimation(child.Key, pos);
                    }
                }

                // Update the startIndex so that the next child occupies a location the same (or greater)
                // row and/or column as this child
                if (!OptimizeChildPlacement)
                {
                    startIndex = (Orientation == Orientation.Horizontal) ? location.Row : location.Col;
                }
            }

            return finalSize;
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Removes all the children from the FluidWrapPanel
        /// </summary>
        private void ClearItemsSource()
        {
            FluidItems.Clear();
            Children.Clear();
        }

        /// <summary>
        /// Provides the index of the child (in the OldFluidWrapPanel's children) from the given point
        /// </summary>
        /// <param name="point">Point</param>
        /// <param name="dragElement">The element being dragged</param>
        /// <param name="children">List of OldFluidWrapPanel children</param>
        /// <returns>Index</returns>
        private int GetIndexFromPoint(Point point, UIElement dragElement, IList<UIElement> children)
        {
            if ((children == null) || (!children.Any()))
                return -1;

            // Optimization: If all the children have the same size as the _cellSize,
            // then use optimized code for finding the Index from point
            if (_isOptimized)
            {
                return GetIndexFromPoint(point);
            }

            foreach (var item in _bounds.Where(item => item.Value.Contains(point)))
            {
                return children.IndexOf(item.Key);
            }

            return -1;
        }

        /// <summary>
        /// Provides the index of the child (in the OldFluidWrapPanel's children) from the given point
        /// </summary>
        /// <param name="point">Point</param>
        /// <returns>Index</returns>
        internal int GetIndexFromPoint(Point point)
        {
            if ((point.X < 0.00D) || (point.X > _panelSize.Width) ||
                (point.Y < 0.00D) || (point.Y > _panelSize.Height))
                return -1;

            int row;
            int column;

            GetCellFromPoint(point, out row, out column);
            return GetIndexFromCell(row, column);
        }

        /// <summary>
        /// Provides the row and column of the child based on its location in the OldFluidWrapPanel
        /// </summary>
        /// <param name="point">Location of the child in the parent</param>
        /// <param name="row">Row</param>
        /// <param name="column">Column</param>
        internal void GetCellFromPoint(Point point, out int row, out int column)
        {
            row = column = -1;

            if ((point.X < 0.00D) ||
                (point.X > _panelSize.Width) ||
                (point.Y < 0.00D) ||
                (point.Y > _panelSize.Height))
            {
                return;
            }

            row = (int)(point.Y / ItemHeight);
            column = (int)(point.X / ItemWidth);
        }

        /// <summary>
        /// Provides the index of the child (in the OldFluidWrapPanel's children) from the given row and column
        /// </summary>
        /// <param name="row">Row</param>
        /// <param name="column">Column</param>
        /// <returns>Index</returns>
        internal int GetIndexFromCell(int row, int column)
        {
            if ((row < 0) || (column < 0))
                return -1;

            var result = -1;
            switch (Orientation)
            {
                case Orientation.Horizontal:
                    result = (_cellsPerLine * row) + column;
                    break;
                case Orientation.Vertical:
                    result = (_cellsPerLine * column) + row;
                    break;
            }

            return result;
        }

        /// <summary>
        /// Provides the row and column of the child based on its index in the OldFluidWrapPanel.Children
        /// </summary>
        /// <param name="index">Index</param>
        /// <param name="row">Row</param>
        /// <param name="column">Column</param>
        internal void GetCellFromIndex(int index, out int row, out int column)
        {
            row = column = -1;

            if (index < 0)
                return;

            switch (Orientation)
            {
                case Orientation.Horizontal:
                    row = (int)(index / (double)_cellsPerLine);
                    column = (int)(index % (double)_cellsPerLine);
                    break;
                case Orientation.Vertical:
                    column = (int)(index / (double)_cellsPerLine);
                    row = (int)(index % (double)_cellsPerLine);
                    break;
            }
        }

        private void CreateTransitionAnimation(UIElement element, Point pos, bool showEasing = true)
        {
            //Dispatcher.BeginInvoke(new Action(() =>
            //{
            Storyboard transition;

            // Is the child being animated the same as the child which was last dragged?
            if (element == _lastDragElement)
            {
                if (!showEasing)
                {
                    // Create the Storyboard for the transition
                    transition = CreateTransition(element, pos, FIRST_TIME_ANIMATION_DURATION,
                        null);
                }
                else
                {
                    // Is easing function specified for the animation?
                    var duration = (DragEasing != null)
                        ? DEFAULT_ANIMATION_TIME_WITH_EASING
                        : DEFAULT_ANIMATION_TIME_WITHOUT_EASING;
                    // Create the Storyboard for the transition
                    transition = CreateTransition(element, pos, duration, DragEasing);
                }

                // When the user releases the drag child, it's Z-Index is set to 1 so that 
                // during the animation it does not go below other elements.
                // After the animation has completed set its Z-Index to 0
                transition.Completed += (s, e) =>
                {
                    if (_lastDragElement != null)
                    {
                        _lastDragElement.SetValue(ZIndexProperty, Z_INDEX_NORMAL);
                        _lastDragElement = null;
                    }
                };
            }
            else // It is a non-dragElement
            {
                if (!showEasing)
                {
                    // Create the Storyboard for the transition
                    transition = CreateTransition(element, pos, FIRST_TIME_ANIMATION_DURATION,
                        null);
                }
                else
                {
                    // Is easing function specified for the animation?
                    var duration = (ElementEasing != null)
                        ? DEFAULT_ANIMATION_TIME_WITH_EASING
                        : DEFAULT_ANIMATION_TIME_WITHOUT_EASING;
                    // Create the Storyboard for the transition
                    transition = CreateTransition(element, pos, duration, ElementEasing);
                }
            }

            // Start the animation
            transition.Begin();
            //}));
        }

        #endregion

        #region FluidDrag Helpers

        /// <summary>
        /// Handler for the event when the user starts dragging the dragElement.
        /// </summary>
        /// <param name="child">UIElement being dragged</param>
        /// <param name="position">Position in the child where the user clicked</param>
        internal async Task BeginFluidDragAsync(UIElement child, Point position)
        {
            if ((child == null) || (!IsComposing))
                return;

            // Call the event handler core on the Dispatcher. (Improves efficiency!)
            await Dispatcher.InvokeAsync(() =>
            {
                child.Opacity = DragOpacity;
                child.SetValue(ZIndexProperty, Z_INDEX_DRAG);
                // Capture further mouse events
                child.CaptureMouse();
                _dragElement = child;
                _lastDragElement = null;

                // Since we are scaling the dragElement by DragScale, the clickPoint also shifts
                _dragStartPoint = new Point(position.X * DragScale, position.Y * DragScale);
            });
        }

        /// <summary>
        /// Handler for the event when the user drags the dragElement.
        /// </summary>
        /// <param name="child">UIElement being dragged</param>
        /// <param name="position">Position where the user clicked w.r.t. the UIElement being dragged</param>
        /// <param name="positionInParent">Position where the user clicked w.r.t. the FluidWrapPanel (the parentFWPanel of the UIElement being dragged</param>
        internal async Task FluidDragAsync(UIElement child, Point position, Point positionInParent)
        {
            if ((child == null) || (!IsComposing) || (_dragElement == null))
                return;

            // Call the event handler core on the Dispatcher. (Improves efficiency!)
            await Dispatcher.InvokeAsync(() =>
            {
                _dragElement.RenderTransform = CreateTransform(positionInParent.X - _dragStartPoint.X,
                                                               positionInParent.Y - _dragStartPoint.Y,
                                                               DragScale,
                                                               DragScale);

                // Get the index in the fluidElements list corresponding to the current mouse location
                var currentPt = positionInParent;
                var index = GetIndexFromPoint(currentPt, _dragElement, FluidItems);

                //if (index == _originalDragIndex)
                //    return;

                Debug.WriteLine(
                    $"Current Pt: {currentPt.ToString()} OldIndex: {FluidItems.IndexOf(_dragElement)} NewIndex: {index}");
                // If no valid cell index is obtained, add the child to the end of the 
                // fluidElements list.
                if ((index == -1) || (index >= FluidItems.Count))
                {
                    index = FluidItems.Count - 1;
                }

                var element = FluidItems[index];

                if (_dragElement == element)
                {
                    _lastExchangeElement = null;
                    return;
                }

                if (element == _lastExchangeElement)
                {
                    return;
                }

                _lastExchangeElement = element;
                var dragCellIndex = FluidItems.IndexOf(_dragElement);
                FluidItems.RemoveAt(dragCellIndex);
                FluidItems.Insert(index, _dragElement);
                Debug.WriteLine("Invalidating");
                InvalidateVisual();
            });
        }

        /// <summary>
        /// Handler for the event when the user stops dragging the dragElement and releases it.
        /// </summary>
        /// <param name="child">UIElement being dragged</param>
        /// <param name="position">Position where the user clicked w.r.t. the UIElement being dragged</param>
        /// <param name="positionInParent">Position where the user clicked w.r.t. the FluidWrapPanel (the parentFWPanel of the UIElement being dragged</param>
        internal async Task EndFluidDragAsync(UIElement child, Point position, Point positionInParent)
        {
            if ((child == null) || (!IsComposing) || (_dragElement == null))
                return;

            // Call the event handler core on the Dispatcher. (Improves efficiency!)
            await Dispatcher.InvokeAsync(() =>
            {
                _dragElement.RenderTransform = CreateTransform(positionInParent.X - _dragStartPoint.X,
                                                               positionInParent.Y - _dragStartPoint.Y,
                                                               DragScale,
                                                               DragScale);

                child.Opacity = NORMAL_OPACITY;
                // Z-Index is set to 1 so that during the animation it does not go below other elements.
                child.SetValue(ZIndexProperty, Z_INDEX_INTERMEDIATE);
                // Release the mouse capture
                child.ReleaseMouseCapture();

                // Reference used to set the Z-Index to 0 during the UpdateFluidLayout
                _lastDragElement = _dragElement;

                _dragElement = null;
                _lastExchangeElement = null;

                InvalidateVisual();
            });
        }

        #endregion

        #region Static Helpers

        /// <summary>
        /// Calculates the number of child items that can be accommodated in a single line
        /// </summary>
        private static int CalculateCellsPerLine(Size panelSize, Size cellSize, Orientation panelOrientation)
        {
            var count = (panelOrientation == Orientation.Horizontal) ? panelSize.Width / cellSize.Width :
                panelSize.Height / cellSize.Height;
            return Math.Max(1, (Int32)Math.Floor(count/* + 0.5*/));
        }

        /// <summary>
        /// Creates a TransformGroup based on the given Translation, Scale and Rotation
        /// </summary>
        /// <param name="transX">Translation in the X-axis</param>
        /// <param name="transY">Translation in the Y-axis</param>
        /// <param name="scaleX">Scale factor in the X-axis</param>
        /// <param name="scaleY">Scale factor in the Y-axis</param>
        /// <param name="rotAngle">Rotation</param>
        /// <returns>TransformGroup</returns>
        internal static TransformGroup CreateTransform(double transX, double transY, double scaleX, double scaleY, double rotAngle = 0.0D)
        {
            var translation = new TranslateTransform
            {
                X = transX,
                Y = transY
            };

            var scale = new ScaleTransform
            {
                ScaleX = scaleX,
                ScaleY = scaleY
            };

            //var rotation = new RotateTransform
            //{
            //    Angle = rotAngle
            //};

            var transform = new TransformGroup();
            // THE ORDER OF TRANSFORM IS IMPORTANT
            // First, scale, then rotate and finally translate
            transform.Children.Add(scale);
            //transform.Children.Add(rotation);
            transform.Children.Add(translation);

            return transform;
        }

        /// <summary>
        /// Creates the storyboard for animating a child from its old location to the new location.
        /// The Translation and Scale properties are animated.
        /// </summary>
        /// <param name="element">UIElement for which the storyboard has to be created</param>
        /// <param name="newLocation">New location of the UIElement</param>
        /// <param name="period">Duration of animation</param>
        /// <param name="easing">Easing function</param>
        /// <returns>Storyboard</returns>
        internal static Storyboard CreateTransition(UIElement element, Point newLocation, TimeSpan period, EasingFunctionBase easing)
        {
            var duration = new Duration(period);

            // Animate X
            var translateAnimationX = new DoubleAnimation
            {
                To = newLocation.X,
                Duration = duration
            };
            if (easing != null)
                translateAnimationX.EasingFunction = easing;

            Storyboard.SetTarget(translateAnimationX, element);
            Storyboard.SetTargetProperty(translateAnimationX,
                new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[1].(TranslateTransform.X)"));

            // Animate Y
            var translateAnimationY = new DoubleAnimation
            {
                To = newLocation.Y,
                Duration = duration
            };
            if (easing != null)
                translateAnimationY.EasingFunction = easing;

            Storyboard.SetTarget(translateAnimationY, element);
            Storyboard.SetTargetProperty(translateAnimationY,
                new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[1].(TranslateTransform.Y)"));

            // Animate ScaleX
            var scaleAnimationX = new DoubleAnimation
            {
                To = 1.0D,
                Duration = duration
            };
            if (easing != null)
                scaleAnimationX.EasingFunction = easing;

            Storyboard.SetTarget(scaleAnimationX, element);
            Storyboard.SetTargetProperty(scaleAnimationX,
                new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[0].(ScaleTransform.ScaleX)"));

            // Animate ScaleY
            var scaleAnimationY = new DoubleAnimation
            {
                To = 1.0D,
                Duration = duration
            };
            if (easing != null)
                scaleAnimationY.EasingFunction = easing;

            Storyboard.SetTarget(scaleAnimationY, element);
            Storyboard.SetTargetProperty(scaleAnimationY,
                new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[0].(ScaleTransform.ScaleY)"));

            var sb = new Storyboard
            {
                Duration = duration
            };
            sb.Children.Add(translateAnimationX);
            sb.Children.Add(translateAnimationY);
            sb.Children.Add(scaleAnimationX);
            sb.Children.Add(scaleAnimationY);

            return sb;
        }

        #endregion
    }
}
