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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;

namespace WPFSpark
{
    /// <summary>
    /// The main Panel which contains the Pivot Items
    /// </summary>
    [ContentProperty("NotifiableChildren")]
    public class PivotPanel : Canvas, INotifiableParent
    {
        #region Fields

        private readonly Grid _rootGrid;
        private readonly PivotHeaderPanel _headerPanel;
        private List<PivotItem> _pivotItems;
        private PivotItem _currPivotItem;

        #endregion

        #region Dependency Properties

        #region ContentBackground

        /// <summary>
        /// ContentBackground Dependency Property
        /// </summary>
        public static readonly DependencyProperty ContentBackgroundProperty =
            DependencyProperty.Register("ContentBackground", typeof(Brush), typeof(PivotPanel),
                new FrameworkPropertyMetadata(Brushes.Transparent, FrameworkPropertyMetadataOptions.AffectsRender | 
                                              FrameworkPropertyMetadataOptions.SubPropertiesDoNotAffectRender));

        /// <summary>
        /// Gets or sets the ContentBackground property. This dependency property 
        /// indicates the background color of the Content.
        /// </summary>
        public Brush ContentBackground
        {
            get { return (Brush)GetValue(ContentBackgroundProperty); }
            set { SetValue(ContentBackgroundProperty, value); }
        }

        #endregion

        #region HeaderBackground

        /// <summary>
        /// HeaderBackground Dependency Property
        /// </summary>
        public static readonly DependencyProperty HeaderBackgroundProperty =
            DependencyProperty.Register("HeaderBackground", typeof(Brush), typeof(PivotPanel),
                new FrameworkPropertyMetadata(Brushes.Transparent, FrameworkPropertyMetadataOptions.AffectsRender |
                                              FrameworkPropertyMetadataOptions.SubPropertiesDoNotAffectRender));

        /// <summary>
        /// Gets or sets the HeaderBackground property. This dependency property 
        /// indicates the background brush of the Header.
        /// </summary>
        public Brush HeaderBackground
        {
            get { return (Brush)GetValue(HeaderBackgroundProperty); }
            set { SetValue(HeaderBackgroundProperty, value); }
        }

        #endregion

        #region HeaderHeight

        /// <summary>
        /// HeaderHeight Dependency Property
        /// </summary>
        public static readonly DependencyProperty HeaderHeightProperty =
            DependencyProperty.Register("HeaderHeight", typeof(GridLength), typeof(PivotPanel),
                new FrameworkPropertyMetadata(new GridLength(0.1, GridUnitType.Star), OnHeaderHeightChanged));

        /// <summary>
        /// Gets or sets the HeaderHeight property. This dependency property 
        /// indicates the Height of the header.
        /// </summary>
        public GridLength HeaderHeight
        {
            get { return (GridLength)GetValue(HeaderHeightProperty); }
            set { SetValue(HeaderHeightProperty, value); }
        }

        /// <summary>
        /// Handles changes to the HeaderHeight property.
        /// </summary>
        /// <param name="d">PivotPanel</param>
        /// <param name="e">DependencyProperty changed event arguments</param>
        private static void OnHeaderHeightChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var panel = (PivotPanel)d;
            var oldHeaderHeight = (GridLength)e.OldValue;
            var newHeaderHeight = panel.HeaderHeight;
            panel.OnHeaderHeightChanged(oldHeaderHeight, newHeaderHeight);
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the HeaderHeight property.
        /// </summary>
        /// <param name="oldHeaderHeight">Old Value</param>
        /// <param name="newHeaderHeight">New Value</param>
        protected void OnHeaderHeightChanged(GridLength oldHeaderHeight, GridLength newHeaderHeight)
        {
            if ((_rootGrid != null) && (_rootGrid.RowDefinitions.Count > 0))
            {
                _rootGrid.RowDefinitions[0].Height = newHeaderHeight;
            }
        }

        #endregion

        #region ItemsSource

        /// <summary>
        /// ItemsSource Dependency Property
        /// </summary>
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register("ItemsSource", typeof(ObservableCollection<PivotItem>), typeof(PivotPanel),
                new FrameworkPropertyMetadata(OnItemsSourceChanged));

        /// <summary>
        /// Gets or sets the ItemsSource property. This dependency property 
        /// indicates the bindable collection.
        /// </summary>
        public ObservableCollection<PivotItem> ItemsSource
        {
            get { return (ObservableCollection<PivotItem>)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        /// <summary>
        /// Handles changes to the ItemsSource property.
        /// </summary>
        /// <param name="d">PivotPanel</param>
        /// <param name="e">DependencyProperty changed event arguments</param>
        private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var panel = (PivotPanel)d;
            var oldItemsSource = (ObservableCollection<PivotItem>)e.OldValue;
            var newItemsSource = panel.ItemsSource;
            panel.OnItemsSourceChanged(oldItemsSource, newItemsSource);
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the ItemsSource property.
        /// </summary>
        /// <param name="oldItemsSource">Old Value</param>
        /// <param name="newItemsSource">New Value</param>
        protected void OnItemsSourceChanged(ObservableCollection<PivotItem> oldItemsSource, ObservableCollection<PivotItem> newItemsSource)
        {
            ClearItemsSource();

            if (newItemsSource != null)
                AddItems(newItemsSource.ToList());
        }

        #endregion

        #endregion

        #region Properties

        /// <summary>
        /// Property used to set the Content Property for the FluidWrapPanel
        /// </summary>
        public NotifiableUIElementCollection NotifiableChildren { get; }

        #endregion

        #region Construction / Initialization

        public PivotPanel()
        {
            NotifiableChildren = new NotifiableUIElementCollection(this, this);

            // Create the root grid that will hold the header panel and the contents
            _rootGrid = new Grid();

            RowDefinition rd = new RowDefinition
            {
                Height = HeaderHeight
            };
            _rootGrid.RowDefinitions.Add(rd);

            rd = new RowDefinition
            {
                Height = new GridLength(1, GridUnitType.Star)
            };
            _rootGrid.RowDefinitions.Add(rd);

            Binding backgroundBinding = new Binding
            {
                Source = Background
            };
            _rootGrid.SetBinding(BackgroundProperty, backgroundBinding);

            _rootGrid.Width = ActualWidth;
            _rootGrid.Height = ActualHeight;

            _rootGrid.HorizontalAlignment = HorizontalAlignment.Stretch;
            _rootGrid.VerticalAlignment = VerticalAlignment.Stretch;

            // Create the header panel
            _headerPanel = new PivotHeaderPanel
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch
            };

            _headerPanel.HeaderSelected += OnHeaderSelected;
            _rootGrid.Children.Add(_headerPanel);

            Children.Add(_rootGrid);

            _pivotItems = new List<PivotItem>();

            SizeChanged += (s, e) =>
                {
                    if (_rootGrid != null)
                    {
                        _rootGrid.Width = ActualWidth;
                        _rootGrid.Height = ActualHeight;
                    }
                };
        }

        #endregion

        #region APIs

        /// <summary>
        /// Adds a PivotItem to the PivotPanel's Children collection
        /// </summary>
        /// <param name="item">PivotItem</param>
        public int AddChild(PivotItem item)
        {
            if (_pivotItems == null)
                _pivotItems = new List<PivotItem>();

            _pivotItems.Add(item);

            item.SetParent(this);

            if (item.PivotHeader != null)
                _headerPanel.AddChild(item.PivotHeader);

            if (item.PivotContent != null)
            {
                Grid.SetRow(item.PivotContent, 1);
                // Set the item to its initial state
                item.Initialize();
                _rootGrid.Children.Add(item.PivotContent);
            }

            return _pivotItems.Count - 1;
        }

        /// <summary>
        /// Adds the newly assigned PivotHeader of the PivotItem to the PivotPanel
        /// </summary>
        /// <param name="item">PivotItem</param>
        internal void UpdatePivotItemHeader(PivotItem item)
        {
            if ((_pivotItems.Contains(item)) && (item.PivotHeader != null) && (!_headerPanel.Contains(item.PivotHeader)))
            {
                _headerPanel.AddChild(item.PivotHeader);
                // Activate the First Pivot Item.
                ActivateFirstPivotItem();
            }
        }

        /// <summary>
        /// Adds the newly assigned PivotContent of the PivotItem to the PivotPanel
        /// </summary>
        /// <param name="item">PivotItem</param>
        internal void UpdatePivotItemContent(PivotItem item)
        {
            if ((_pivotItems.Contains(item)) && (item.PivotContent != null) && (!_rootGrid.Children.Contains(item.PivotContent)))
            {
                Grid.SetRow(item.PivotContent, 1);
                _rootGrid.Children.Add(item.PivotContent);
                // Activate the First Pivot Item.
                ActivateFirstPivotItem();
            }
        }

        /// <summary>
        /// Adds a list of PivotItems to the PivotPanel's Children collection
        /// </summary>
        /// <param name="items">List of PivotItems</param>
        public void AddItems(List<PivotItem> items)
        {
            if (items == null)
                return;

            foreach (var item in items)
            {
                AddChild(item);
            }

            ActivateFirstPivotItem();
        }

        /// <summary>
        /// Sets the DataContext for the PivotContent of each of the PivotItems.
        /// </summary>
        /// <param name="context">Data Context</param>
        public void SetDataContext(object context)
        {
            if ((_pivotItems == null) || (_pivotItems.Count == 0))
                return;

            foreach (var item in _pivotItems)
            {
                item.PivotContent.DataContext = context;
            }
        }

        /// <summary>
        /// Resets the location of the header items so that the 
        /// first child that was added is moved to the beginning.
        /// </summary>
        public void Reset()
        {
            _headerPanel?.Reset();
        }

        public void SelectHeaderByName(string headerName)
        {
            if (String.IsNullOrWhiteSpace(headerName))
                return;

            var pivotItem = _pivotItems.FirstOrDefault(p => p.PivotHeader.Name == headerName);
            if ((pivotItem == null) || (pivotItem == _currPivotItem))
                return;

            (pivotItem.PivotHeader as PivotHeaderControl)?.SelectHeader();
        }
        #endregion

        #region Event Handlers

        /// <summary>
        /// Handles the event raised when a header item is selected
        /// </summary>
        /// <param name="sender">Header item</param>
        /// <param name="e">Event Args</param>
        void OnHeaderSelected(object sender, EventArgs e)
        {
            var headerItem = sender as FrameworkElement;
            if (headerItem == null)
                return;

            // Find the PivotItem whose header was selected
            var pItem = _pivotItems.FirstOrDefault(p => p.PivotHeader == headerItem);

            if ((pItem == null) || (pItem == _currPivotItem))
                return;

            _currPivotItem?.SetActive(false);
            pItem.SetActive(true);
            _currPivotItem = pItem;
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Sets the First Pivot item as active
        /// </summary>
        private void ActivateFirstPivotItem()
        {
            // Set the first item as active
            if ((_pivotItems != null) && (_pivotItems.Count > 0))
            {
                _pivotItems.First().SetActive(true);
                _currPivotItem = _pivotItems.First();
            }
        }

        /// <summary>
        /// Removes all the Pivot Items from the Children collection
        /// </summary>
        void ClearItemsSource()
        {
            if ((_pivotItems == null) || (_pivotItems.Count == 0))
                return;

            _headerPanel?.ClearHeader();

            if (_rootGrid != null)
            {
                foreach (var item in _pivotItems)
                {
                    _rootGrid.Children.Remove(item.PivotContent);
                }
            }

            _pivotItems.Clear();
        }

        #endregion

        #region INotifiableParent Members

        /// <summary>
        /// Adds the child to the Panel through XAML
        /// </summary>
        /// <param name="child">Child to be added</param>
        /// <returns>Index of the child in the collection</returns>
        public int AddChild(UIElement child)
        {
            var pItem = child as PivotItem;
            if (pItem != null)
            {
                return AddChild(pItem);
            }

            return -1;
        }

        #endregion
    }
}
