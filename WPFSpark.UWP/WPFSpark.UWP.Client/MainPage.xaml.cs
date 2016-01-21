using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace WPFSpark.UWP.Client
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private Random _random = new Random();
        private Brush[] _brushes;

        #region UseRandomChildSize

        /// <summary>
        /// UseRandomChildSize Dependency Property
        /// </summary>
        public static readonly DependencyProperty UseRandomChildSizeProperty =
            DependencyProperty.Register("UseRandomChildSize", typeof(bool), typeof(MainPage),
                new PropertyMetadata(false, OnRandomChildSizeChanged));

        private static void OnRandomChildSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var window = d as MainPage;
            window?.RefreshFluidWrapPanel();
        }

        /// <summary>
        /// Gets or sets the UseRandomChildSize property. This dependency property 
        /// indicates whether the children should be of different size or same size.
        /// </summary>
        public bool UseRandomChildSize
        {
            get { return (bool)GetValue(UseRandomChildSizeProperty); }
            set { SetValue(UseRandomChildSizeProperty, value); }
        }

        #endregion

        public MainPage()
        {
            this.InitializeComponent();

            _brushes = new Brush[] {
                                        new SolidColorBrush(Color.FromArgb(255, 76, 217, 100)),
                                        new SolidColorBrush(Color.FromArgb(255, 0, 122, 255)),
                                        new SolidColorBrush(Color.FromArgb(255, 255, 150, 0)),
                                        new SolidColorBrush(Color.FromArgb(255, 255, 45, 85)),
                                        new SolidColorBrush(Color.FromArgb(255, 88, 86, 214)),
                                        new SolidColorBrush(Color.FromArgb(255, 255, 204, 0)),
                                        new SolidColorBrush(Color.FromArgb(255, 142, 142, 147)),
                                      };

            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            OrientationCB.ItemsSource = new List<string> { "Horizontal", "Vertical" };
            OrientationCB.SelectedIndex = 0;
            RefreshFluidWrapPanel();
        }

        private void RefreshFluidWrapPanel()
        {
            var items = new ObservableCollection<UIElement>();
            var count = _random.Next(10, 20);
            for (var i = 0; i < count; i++)
            {
                var brush = _brushes[_random.Next(_brushes.Length)];
                //var factor = 1;
                var factorWidth = UseRandomChildSize ? _random.Next(1, 3) : 1;
                var factorHeight = UseRandomChildSize ? _random.Next(1, 3) : 1;

                var ctrl = new FluidItemControl
                {
                    Width = factorWidth * panel.ItemWidth,
                    Height = factorHeight * panel.ItemHeight,
                    Fill = brush,
                    Data = (i + 1).ToString()
                };

                items.Add(ctrl);
            }

            panel.ItemsSource = items;
        }

        private void OnOrientationChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (OrientationCB.SelectedValue as string)
            {
                case "Horizontal":
                    scrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
                    scrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
                    panel.Orientation = Orientation.Horizontal;
                    break;
                case "Vertical":
                    scrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
                    scrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Disabled;
                    panel.Orientation = Orientation.Vertical;
                    break;
            }
        }

        private void OnRefresh(object sender, RoutedEventArgs e)
        {
            RefreshFluidWrapPanel();
        }
    }
}
