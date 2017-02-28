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
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

        public const double NormalScale = 1.0d;
        public const double DragScaleDefault = 1.0d;
        public const double NormalOpacity = 1.0d;
        public const double DragOpacityDefault = 0.6d;
        public const double OpacityMin = 0.1d;
        public const double DefaultItemWidth = 10.0;
        public const double DefaultItemHeight = 10.0;
        public const int ZIndexNormal = 0;
        public const int ZIndexIntermediate = 1;
        public const int ZIndexDrag = 10;
        public static TimeSpan DefaultAnimationTimeWithoutEasing = TimeSpan.FromMilliseconds(200);
        public static TimeSpan DefaultAnimationTimeWithEasing = TimeSpan.FromMilliseconds(600);
        public static TimeSpan FirstTimeAnimationDuration = TimeSpan.FromMilliseconds(320);

        #endregion

        #region Structures

        /// <summary>
        /// Structure to store the bit-normalized dimensions
        /// of the FluidWrapPanel's children.
        /// </summary>
        private struct BitSize
        {
            internal int Width;
            internal int Height;
        }

        /// <summary>
        /// Structure to store the location and the bit-normalized
        /// dimensions of the FluidWrapPanel's children.
        /// </summary>
        private struct BitInfo
        {
            internal long Row;
            internal long Col;
            internal int Width;
            internal int Height;

            /// <summary>
            /// Checks if the bit-normalized width and height
            /// are equal to 1.
            /// </summary>
            /// <returns>True if yes otherwise False</returns>
            internal bool IsUnitSize()
            {
                return (Width == 1) && (Height == 1);
            }

            /// <summary>
            /// Checks if the given location is within the 
            /// bit-normalized bounds
            /// </summary>
            /// <param name="row">Row</param>
            /// <param name="col">Column</param>
            /// <returns>True if yes otherwise False</returns>
            internal bool Contains(long row, long col)
            {
                return (row >= Row) && (row < Row + Height) &&
                       (col >= Col) && (col < Col + Width);
            }
        }

        #endregion

        #region Fields

        private Point _dragStartPoint;
        private UIElement _dragElement;
        private UIElement _lastDragElement;
        private bool _isOptimized;
        private Size _panelSize;
        private int _cellsPerLine;
        private UIElement _lastExchangedElement;
        private int _maxCellRows;
        private int _maxCellCols;
        private Dictionary<UIElement, BitInfo> _fluidBits;

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
                new FrameworkPropertyMetadata(DragOpacityDefault, OnDragOpacityChanged, CoerceDragOpacity));

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

            if (opacity < OpacityMin)
            {
                opacity = OpacityMin;
            }
            else if (opacity > NormalOpacity)
            {
                opacity = NormalOpacity;
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

        #endregion

        #region DragScale

        /// <summary>
        /// DragScale Dependency Property
        /// </summary>
        public static readonly DependencyProperty DragScaleProperty =
            DependencyProperty.Register("DragScale", typeof(double), typeof(FluidWrapPanel),
                new FrameworkPropertyMetadata(DragScaleDefault));

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
                new FrameworkPropertyMetadata(DefaultItemHeight,
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
                new FrameworkPropertyMetadata(DefaultItemWidth,
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
            get { return (IEnumerable)GetValue(ItemsSourceProperty); }
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
            var oldItemsSource = (IEnumerable)e.OldValue;
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
            _fluidBits = new Dictionary<UIElement, BitInfo>();
            _lastDragElement = null;
            _lastExchangedElement = null;
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
                child.RenderTransform = CreateTransform(-child.DesiredSize.Width, -child.DesiredSize.Height,
                                                        NormalScale, NormalScale);
            }

            // Unit size of a cell
            var cellSize = new Size(ItemWidth, ItemHeight);

            if ((availableSize.Width < 0.0d) || (availableSize.Width.IsZero())
                || (availableSize.Height < 0.0d) || (availableSize.Height.IsZero())
                || !FluidItems.Any())
            {
                return cellSize;
            }

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

            int matrixWidth;
            int matrixHeight;
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
                var width = child.Width;
                var height = child.Height;

                MatrixCell cell;
                if (matrix.TryFindRegion(startIndex, width, height, out cell))
                {
                    matrix.SetRegion(cell, width, height);
                }
                else
                {
                    // If code reached here, it means that the child is too big to be accommodated
                    // in the matrix. Normally this should not occur!
                    throw new ApplicationException("Measure Phase: Unable to accommodate child in the panel!");
                }

                if (!OptimizeChildPlacement)
                {
                    // Update the startIndex so that the next child occupies a location which has 
                    // the same (or greater) row and/or column as this child
                    startIndex = (Orientation == Orientation.Horizontal) ? cell.Row : cell.Col;
                }
            }

            // Calculate the true size of the matrix
            var matrixSize = matrix.GetFilledMatrixDimensions();
            // Calculate the size required by the panel
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
            });

            // If all the children have the same size as the cellSize then use optimized code
            // when a child is being dragged
            _isOptimized = !childData.Values.Any(c => (c.Width != 1) || (c.Height != 1));

            // Calculate matrix dimensions
            int matrixWidth;
            int matrixHeight;
            if (Orientation == Orientation.Horizontal)
            {
                // If the maximum width required by a child is more than the calculated cellsPerLine, then
                // the matrix width should be the maximum width of that child
                matrixWidth = Math.Max(childData.Values.Max(s => s.Width), _cellsPerLine);
                // For purpose of calculating the true size of the panel, the height of the matrix must
                // be set to the cumulative height of all the children
                matrixHeight = childData.Values.Sum(s => s.Height);
            }
            else
            {
                // For purpose of calculating the true size of the panel, the width of the matrix must
                // be set to the cumulative width of all the children
                matrixWidth = childData.Values.Sum(s => s.Width);
                // If the maximum height required by a child is more than the calculated cellsPerLine, then
                // the matrix height should be the maximum height of that child
                matrixHeight = Math.Max(childData.Values.Max(s => s.Height), _cellsPerLine);
            }

            // Create FluidBitMatrix to calculate the size required by the panel
            var matrix = new FluidBitMatrix(matrixHeight, matrixWidth, Orientation);

            var startIndex = 0L;
            _fluidBits.Clear();

            foreach (var child in childData)
            {
                var width = child.Value.Width;
                var height = child.Value.Height;

                MatrixCell cell;
                if (matrix.TryFindRegion(startIndex, width, height, out cell))
                {
                    // Set the bits
                    matrix.SetRegion(cell, width, height);
                    // Arrange the child
                    child.Key.Arrange(new Rect(0, 0, child.Key.DesiredSize.Width, child.Key.DesiredSize.Height));
                    // Convert MatrixCell location to actual location
                    var pos = new Point(cell.Col * cellSize.Width, cell.Row * cellSize.Height);

                    BitInfo info;
                    info.Row = cell.Row;
                    info.Col = cell.Col;
                    info.Width = width;
                    info.Height = height;
                    _fluidBits.Add(child.Key, info);

                    if (!ReferenceEquals(child.Key, _dragElement))
                    {
                        // Animate the child to the new location
                        CreateTransitionAnimation(child.Key, pos);
                    }
                }
                else
                {
                    // If code reached here, it means that the child is too big to be accommodated
                    // in the matrix. Normally this should not occur!
                    throw new ApplicationException("Arrange Phase: Unable to accommodate child in the panel!");
                }

                if (!OptimizeChildPlacement)
                {
                    // Update the startIndex so that the next child occupies a location which has 
                    // the same (or greater) row and/or column as this child
                    startIndex = (Orientation == Orientation.Horizontal) ? cell.Row : cell.Col;
                }
            }

            _maxCellRows = (int)Math.Max(1, Math.Floor(_panelSize.Height / ItemHeight));
            _maxCellCols = (int)Math.Max(1, Math.Floor(_panelSize.Width / ItemWidth));

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
        /// Provides the index of the child corresponding to the given point
        /// </summary>
        /// <param name="point">Point</param>
        /// <returns>Index of the child</returns>
        private int GetIndexFromPoint(Point point)
        {
            if ((point.X < 0.00D) || (point.X > _panelSize.Width) ||
                (point.Y < 0.00D) || (point.Y > _panelSize.Height) ||
                !FluidItems.Any())
                return -1;

            // Get the row and column of the cell corresponding 
            // to this location
            var row = (int)(point.Y / ItemHeight);
            var column = (int)(point.X / ItemWidth);

            // Get the index for the cell based on Orientation
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
        /// Gets the list of children overlapped by the given element when it is
        /// moved to the given cell location.
        /// </summary>
        /// <param name="element">UIElement</param>
        /// <param name="cell">Cell location</param>
        /// <returns>List of overlapped UIElements</returns>
        private List<UIElement> GetOverlappedChildren(UIElement element, MatrixCell cell)
        {
            var result = new List<UIElement>();
            var info = _fluidBits[element];

            for (var row = 0; row < info.Height; row++)
            {
                for (var col = 0; col < info.Width; col++)
                {
                    var item = _fluidBits.Where(t => t.Value.Contains(cell.Row + row, cell.Col + col)).Select(t => t.Key).FirstOrDefault();
                    if ((item != null) && !ReferenceEquals(item, element) && (!result.Contains(item)))
                    {
                        result.Add(item);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Gets the list of cell locations which are vacated when the given element is
        /// moved to the given cell location.
        /// </summary>
        /// <param name="element">UIElement</param>
        /// <param name="cell">Cell location</param>
        /// <returns>List of cell locations</returns>
        private List<MatrixCell> GetVacatedCells(UIElement element, MatrixCell cell)
        {
            var result = new List<MatrixCell>();

            var info = _fluidBits[element];
            var baseRow = info.Row;
            var baseCol = info.Col;
            var width = info.Width;
            var height = info.Height;

            var minRow = cell.Row;
            var maxRow = minRow + height;
            var minCol = cell.Col;
            var maxCol = minCol + width;

            for (var i = 0; i < height; i++)
            {
                for (var j = 0; j < width; j++)
                {
                    var row = baseRow + i;
                    var col = baseCol + j;

                    var isInside = (row >= minRow) && (row < maxRow) &&
                                   (col >= minCol) && (col < maxCol);

                    if (!isInside)
                        result.Add(new MatrixCell(row, col));
                }
            }

            return result;
        }

        /// <summary>
        /// Checks if the given UIElement can fit in the given cell location
        /// </summary>
        /// <param name="element">UIElement</param>
        /// <param name="cell">Cell location</param>
        /// <returns>True if the UIElement fits otherwise False.</returns>
        private bool IsValidCellPosition(UIElement element, MatrixCell cell)
        {
            if (!cell.IsValid())
                return false;

            var info = _fluidBits[element];

            return (cell.Row + info.Height <= _maxCellRows) &&
                   (cell.Col + info.Width <= _maxCellCols);
        }

        /// <summary>
        /// Gets the top left cell location corresponding to the given position
        /// </summary>
        /// <param name="info">Bit Information</param>
        /// <param name="position">Position where the user clicked w.r.t. the UIElement being dragged</param>
        /// <param name="positionInParent">Position where the user clicked w.r.t. the FluidWrapPanel</param>
        /// <returns></returns>
        private MatrixCell GetCellFromPoint(BitInfo info, Point position, Point positionInParent)
        {
            var row = (int)(positionInParent.Y / ItemHeight);
            var col = (int)(positionInParent.X / ItemWidth);

            // If the item is not having unit size, then calculate the top left cell location
            if (!info.IsUnitSize())
            {
                row -= (int)(position.Y / ItemHeight);
                col -= (int)(position.X / ItemWidth);
            }

            // Bounds check
            if ((row < 0) ||
                (row > _maxCellRows) ||
                (col < 0) ||
                (col > _maxCellCols))
            {
                return MatrixCell.InvalidCell();
            }

            return new MatrixCell(row, col);
        }

        /// <summary>
        /// Creates the animation for moving the element to the given position.
        /// </summary>
        /// <param name="element">UIElement to be animated</param>
        /// <param name="pos">Final position of the UIElement</param>
        /// <param name="showEasing">Flag to indicate whether easing should be applied</param>
        private void CreateTransitionAnimation(UIElement element, Point pos, bool showEasing = true)
        {
            Storyboard transition;

            // Is the child being animated the same as the child which was last dragged?
            if (ReferenceEquals(element, _lastDragElement))
            {
                if (!showEasing)
                {
                    // Create the Storyboard for the transition
                    transition = CreateTransition(element, pos, FirstTimeAnimationDuration,
                        null);
                }
                else
                {
                    // Is easing function specified for the animation?
                    var duration = (DragEasing != null)
                        ? DefaultAnimationTimeWithEasing
                        : DefaultAnimationTimeWithoutEasing;
                    // Create the Storyboard for the transition
                    transition = CreateTransition(element, pos, duration, DragEasing);
                }

                // When the user releases the drag child, it's Z-Index is set to 1 so that 
                // during the animation it does not go below other elements.
                // After the animation has completed set its Z-Index to 0
                transition.Completed += (s, e) =>
                {
                    if (_lastDragElement == null)
                        return;

                    _lastDragElement.SetValue(ZIndexProperty, ZIndexNormal);
                    _lastDragElement = null;
                };
            }
            else // It is a non-dragElement
            {
                if (!showEasing)
                {
                    // Create the Storyboard for the transition
                    transition = CreateTransition(element, pos, FirstTimeAnimationDuration,
                        null);
                }
                else
                {
                    // Is easing function specified for the animation?
                    var duration = (ElementEasing != null)
                        ? DefaultAnimationTimeWithEasing
                        : DefaultAnimationTimeWithoutEasing;
                    // Create the Storyboard for the transition
                    transition = CreateTransition(element, pos, duration, ElementEasing);
                }
            }

            // Start the animation
            transition.Begin();
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
                child.SetValue(ZIndexProperty, ZIndexDrag);
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
        /// <param name="positionInParent">Position where the user clicked w.r.t. the FluidWrapPanel</param>
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

                // Are all the children are of unit cell size?
                if (_isOptimized)
                {
                    // Get the index of the dragElement
                    var dragCellIndex = FluidItems.IndexOf(_dragElement);

                    // Get the index in the fluidElements list corresponding to the current mouse location
                    var index = GetIndexFromPoint(positionInParent);

                    // If no valid cell index is obtained (happens when the dragElement is dragged outside
                    // the FluidWrapPanel), add the child to the end of the fluidElements list.
                    if ((index == -1) || (index >= FluidItems.Count))
                    {
                        index = FluidItems.Count - 1;
                    }

                    // If both indices are same no need to process further
                    if (index == dragCellIndex)
                        return;

                    // Move the dragElement to the new index
                    FluidItems.RemoveAt(dragCellIndex);
                    FluidItems.Insert(index, _dragElement);

                    // Refresh the FluidWrapPanel
                    InvalidateVisual();
                }
                // Children are not having unit cell size
                else
                {
                    // Refresh the FluidWrapPanel only if the dragElement
                    // can be successfully placed in the new location
                    if (TryFluidDrag(position, positionInParent))
                    {
                        InvalidateVisual();
                    }
                }
            });
        }

        /// <summary>
        /// Handles the situation when the user drags a dragElement which does not have 
        /// unit size dimension. It checks if the dragElement can fit in the new location and
        /// the rest of the children can be rearranged successfully in the remaining space.
        /// </summary>
        /// <param name="position">Position of the pointer within the dragElement</param>
        /// <param name="positionInParent">Position of the pointer w.r.t. the FluidWrapPanel</param>
        /// <returns>True if successful otherwise False</returns>
        private bool TryFluidDrag(Point position, Point positionInParent)
        {
            // Get the index of the dragElement
            var dragCellIndex = FluidItems.IndexOf(_dragElement);

            // Convert the current location to MatrixCell which indicates the top left cell of the dragElement
            var currentCell = GetCellFromPoint(_fluidBits[_dragElement], position, positionInParent);

            // Check if the item being dragged can fit in the new cell location
            if (!IsValidCellPosition(_dragElement, currentCell))
                return false;

            // Get the list of cells vacated when the dragElement moves to the new cell location
            var vacatedCells = GetVacatedCells(_dragElement, currentCell);
            // If none of the cells are vacated, no need to proceed further
            if (vacatedCells.Count == 0)
            {
                _lastExchangedElement = null;
                return false;
            }

            // Get the list of children overlapped by the 
            var overlappedChildren = GetOverlappedChildren(_dragElement, currentCell);
            var dragInfo = _fluidBits[_dragElement];
            // If there is only one overlapped child and its dimension matches the 
            // dimensions of the dragElement, then exchange their indices
            if (overlappedChildren.Count == 1)
            {
                var element = overlappedChildren[0];
                var info = _fluidBits[element];
                var dragCellCount = info.Width * info.Height;
                if ((info.Width == dragInfo.Width) && (info.Height == dragInfo.Height))
                {
                    //if (ReferenceEquals(_dragElement, element))
                    //{
                    //    _lastExchangedElement = null;
                    //    return false;
                    //}

                    // If user moves the dragElement back to the lastExchangedElement's position, then it can
                    // be exchanged again only if the dragElement has vacated all the cells occupied by it in 
                    // the previous location.
                    if (ReferenceEquals(element, _lastExchangedElement) && (vacatedCells.Count != dragCellCount))
                    {
                        return false;
                    }

                    // Exchange the dragElement and the overlapped element
                    _lastExchangedElement = element;
                    var index = FluidItems.IndexOf(element);
                    // To prevent an IndexOutOfRangeException during the exchange
                    // Remove the item with higher index first followed by the lower index item and then
                    // Insert the items in the lower index first and then in the higher index
                    if (index > dragCellIndex)
                    {
                        FluidItems.RemoveAt(index);
                        FluidItems.RemoveAt(dragCellIndex);
                        FluidItems.Insert(dragCellIndex, element);
                        FluidItems.Insert(index, _dragElement);
                    }
                    else
                    {
                        FluidItems.RemoveAt(dragCellIndex);
                        FluidItems.RemoveAt(index);
                        FluidItems.Insert(index, _dragElement);
                        FluidItems.Insert(dragCellIndex, element);
                    }

                    return true;
                }
            }

            // Since there are multiple overlapped children, we need to rearrange all the children
            // Create a temporary matrix to check if all the children are placed successfully
            // when the dragElement is moved to the new cell location
            var tempMatrix = new FluidBitMatrix(_maxCellRows, _maxCellCols, Orientation);

            // First set the cells corresponding to dragElement's cells in new location
            tempMatrix.SetRegion(currentCell, dragInfo.Width, dragInfo.Height);
            // Try to fit the remaining items
            var startIndex = 0L;
            var tempFluidBits = new Dictionary<UIElement, BitInfo>();
            // Add the new bit information for dragElement
            dragInfo.Row = currentCell.Row;
            dragInfo.Col = currentCell.Col;
            tempFluidBits[_dragElement] = dragInfo;

            // Try placing the rest of the children in the matrix
            foreach (var item in _fluidBits.Where(t => !ReferenceEquals(t.Key, _dragElement)))
            {
                var width = item.Value.Width;
                var height = item.Value.Height;

                MatrixCell cell;
                if (tempMatrix.TryFindRegion(startIndex, width, height, out cell))
                {
                    // Set the bits
                    tempMatrix.SetRegion(cell, width, height);
                    // Capture the bit information
                    BitInfo newinfo;
                    newinfo.Row = cell.Row;
                    newinfo.Col = cell.Col;
                    newinfo.Width = width;
                    newinfo.Height = height;
                    tempFluidBits.Add(item.Key, newinfo);
                }
                else
                {
                    // No suitable location was found to fit the current item. So the children cannot be
                    // successfully placed after moving dragElement to new cell location. So dragElement
                    // will not be moved.
                    return false;
                }

                // Update the startIndex so that the next child occupies a location the same (or greater)
                // row and/or column as this child
                if (!OptimizeChildPlacement)
                {
                    startIndex = (Orientation == Orientation.Horizontal) ? cell.Row : cell.Col;
                }
            }

            // All the children have been successfully readjusted, so now 
            // Re-Index the children based on the panel's orientation
            var tempFluidItems = new List<UIElement>();
            if (Orientation == Orientation.Horizontal)
            {
                for (var row = 0; row < _maxCellRows; row++)
                {
                    for (var col = 0; col < _maxCellCols; col++)
                    {
                        var item = tempFluidBits.Where(t => t.Value.Contains(row, col))
                                                .Select(t => t.Key).FirstOrDefault();
                        if ((item != null) && (!tempFluidItems.Contains(item)))
                        {
                            tempFluidItems.Add(item);
                        }
                    }
                }
            }
            else
            {
                for (var col = 0; col < _maxCellCols; col++)
                {
                    for (var row = 0; row < _maxCellRows; row++)
                    {
                        var item = tempFluidBits.Where(t => t.Value.Contains(row, col))
                                                .Select(t => t.Key).FirstOrDefault();
                        if ((item != null) && (!tempFluidItems.Contains(item)))
                        {
                            tempFluidItems.Add(item);
                        }
                    }
                }
            }

            // Update the new indices in FluidItems
            FluidItems.Clear();
            foreach (var fluidItem in tempFluidItems)
            {
                FluidItems.Add(fluidItem);
            }

            // Clean up
            tempFluidItems.Clear();
            tempFluidBits.Clear();

            return true;
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

                child.Opacity = NormalOpacity;
                // Z-Index is set to 1 so that during the animation it does not go below other elements.
                child.SetValue(ZIndexProperty, ZIndexIntermediate);
                // Release the mouse capture
                child.ReleaseMouseCapture();

                // Reference used to set the Z-Index to 0 during the UpdateFluidLayout
                _lastDragElement = _dragElement;

                _dragElement = null;
                _lastExchangedElement = null;

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
            return Math.Max(1, (Int32)Math.Floor(count));
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
