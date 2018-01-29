using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WPFSpark;
using Timer = System.Timers.Timer;

namespace WPFSparkClient.NET46
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : SparkWindow
    {
        #region Enums

        public enum AppMode
        {
            SprocketControl,
            ToggleSwitch,
            FluidWrapPanel,
            SparkWindow,
            FluidPivotPanel,
            FluidProgressBar,
            FluidStatusBar
        }

        enum SplitViewMenuWidth
        {
            Narrow = 48,
            Wide = 240
        }

        #endregion

        #region Fields

        Timer timer1 = new Timer(70);
        Timer timer2 = new Timer(70);

        bool isBGWorking = false;
        BackgroundWorker bgWorker;
        private Random _rnd = new Random();
        private Brush[] _brushes;

        #endregion

        #region Dependency Properties

        #region CurrentAppMode

        /// <summary>
        /// CurrentAppMode Dependency Property
        /// </summary>
        public static readonly DependencyProperty CurrentAppModeProperty =
            DependencyProperty.Register("CurrentAppMode", typeof(AppMode), typeof(MainWindow),
                new FrameworkPropertyMetadata(AppMode.SprocketControl));

        /// <summary>
        /// Gets or sets the CurrentAppMode property. This dependency property 
        /// indicates the current application mode.
        /// </summary>
        public AppMode CurrentAppMode
        {
            get { return (AppMode)GetValue(CurrentAppModeProperty); }
            set { SetValue(CurrentAppModeProperty, value); }
        }

        #endregion

        #region AppTitle

        /// <summary>
        /// AppTitle Dependency Property
        /// </summary>
        public static readonly DependencyProperty AppTitleProperty =
            DependencyProperty.Register("AppTitle", typeof(string), typeof(MainWindow),
                new FrameworkPropertyMetadata(string.Empty));

        /// <summary>
        /// Gets or sets the AppTitle property. This dependency property 
        /// indicates the title to be displayed based on user selection.
        /// </summary>
        public string AppTitle
        {
            get { return (string)GetValue(AppTitleProperty); }
            set { SetValue(AppTitleProperty, value); }
        }

        #endregion

        #endregion

        public MainWindow()
        {
            InitializeComponent();

            InitializeSprocketControls();

            _brushes = new Brush[] {
                                        new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4cd964")),
                                        new SolidColorBrush((Color)ColorConverter.ConvertFromString("#007aff")),
                                        new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ff9600")),
                                        new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ff2d55")),
                                        new SolidColorBrush((Color)ColorConverter.ConvertFromString("#5856d6")),
                                        new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ffcc00")),
                                        new SolidColorBrush((Color)ColorConverter.ConvertFromString("#8e8e93")),
                                      };

            SizeChanged += (o, a) =>
            {
                switch (WindowState)
                {
                    case WindowState.Maximized:
                        SplitViewMenu.Width = (int)SplitViewMenuWidth.Wide;
                        break;

                    case WindowState.Normal:
                        SplitViewMenu.Width = (int)SplitViewMenuWidth.Narrow;
                        break;
                }

                RootGrid.ColumnDefinitions[0] = new ColumnDefinition { Width = new GridLength(SplitViewMenu.Width) };
                RootGrid.InvalidateVisual();
            };

            // Enable the tooltip for SplitView menu buttons only if the SplitView width is narrow
            SplitViewMenu.SizeChanged += (o, a) =>
            {
                var isNarrowMenu = (int)SplitViewMenu.Width == (int)SplitViewMenuWidth.Narrow;
                ToolTipService.SetIsEnabled(SprocketButton, isNarrowMenu);
                ToolTipService.SetIsEnabled(ToggleSwitchButton, isNarrowMenu);
                ToolTipService.SetIsEnabled(FWPButton, isNarrowMenu);
                ToolTipService.SetIsEnabled(SparkWindowButton, isNarrowMenu);
                ToolTipService.SetIsEnabled(FPPButton, isNarrowMenu);
                ToolTipService.SetIsEnabled(FPBButton, isNarrowMenu);
                ToolTipService.SetIsEnabled(FSBButton, isNarrowMenu);
            };

            Loaded += (s, e) =>
            {
                DataContext = this;
                SprocketButton.IsChecked = true;
                OrientationCB.ItemsSource = new List<string> { "Horizontal", "Vertical" };
                OrientationCB.SelectedIndex = 0;
            };

            InitializeFluidPivotPanel();

            InitializeFluidProgressBars();
        }

        // -------------------------------------------------------------------------------------
        // SprocketControl
        // -------------------------------------------------------------------------------------

        #region SprocketControl

        private void InitializeSprocketControls()
        {
            timer1.Elapsed += timer1_Elapsed;
            timer2.Elapsed += timer2_Elapsed;

            bgWorker = new BackgroundWorker
            {
                WorkerSupportsCancellation = true,
                WorkerReportsProgress = true
            };
            bgWorker.DoWork += DoWork;
            bgWorker.RunWorkerCompleted += OnWorkCompleted;
            bgWorker.ProgressChanged += OnProgress;
        }

        async void timer1_Elapsed(object sender, ElapsedEventArgs e)
        {
            await Dispatcher.InvokeAsync(() =>
            {
                sprocketControl3.Progress++;

                if (sprocketControl3.Progress >= 100)
                {
                    timer1.Enabled = false;
                    button1.IsEnabled = true;
                }
            });
        }

        async void timer2_Elapsed(object sender, ElapsedEventArgs e)
        {
            await Dispatcher.InvokeAsync(() =>
            {
                sprocketControl4.Progress++;

                if (sprocketControl4.Progress >= 100)
                {
                    timer2.Enabled = false;
                    button2.IsEnabled = true;
                }
            });
        }

        async private void button1_Click(object sender, RoutedEventArgs e)
        {
            await Dispatcher.InvokeAsync(() =>
            {
                button1.IsEnabled = false;
                sprocketControl3.Progress = 0;
                textBlock1.Visibility = System.Windows.Visibility.Visible;
                timer1.Enabled = true;
            });
        }

        async private void button2_Click(object sender, RoutedEventArgs e)
        {
            await Dispatcher.InvokeAsync(() =>
            {
                button2.IsEnabled = false;
                sprocketControl4.Progress = 0;
                textBlock2.Visibility = Visibility.Visible;
                timer2.Enabled = true;
            });
        }

        #endregion

        // -------------------------------------------------------------------------------------
        // SparkWindow
        // -------------------------------------------------------------------------------------

        #region SparkWindow

        private void LaunchSparkWindow(object sender, RoutedEventArgs e)
        {
            SparkWindow win = new SparkWindow
            {
                Width = 800,
                Height = 600,
                Title = "Sample SparkWindow",
                AllowsTransparency = true,
                WindowStyle = WindowStyle.None,
                Content = new Grid
                {
                    Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#DBE6ECF0"))
                }
            };


            win.ShowDialog();
        }

        #endregion

        // -------------------------------------------------------------------------------------
        // FluidWrapPanel
        // -------------------------------------------------------------------------------------

        #region FluidWrapPanel

        private Random _random = new Random();

        #region UseRandomChildSize

        /// <summary>
        /// UseRandomChildSize Dependency Property
        /// </summary>
        public static readonly DependencyProperty UseRandomChildSizeProperty =
            DependencyProperty.Register("UseRandomChildSize", typeof(bool), typeof(MainWindow),
                new FrameworkPropertyMetadata(false, OnRandomChildSizeChanged));

        private static void OnRandomChildSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var window = d as MainWindow;
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

        #endregion

        // -------------------------------------------------------------------------------------
        // FluidPivotPanel
        // -------------------------------------------------------------------------------------

        #region FluidPivotPanel

        public class TextMessage : INotifyPropertyChanged
        {
            #region INotifyPropertyChanged Members

            public event PropertyChangedEventHandler PropertyChanged;

            #endregion

            #region MainText

            private string _mainText = string.Empty;

            /// <summary>
            /// Gets or sets the MainText property. This observable property 
            /// indicates the main text.
            /// </summary>
            public string MainText
            {
                get { return _mainText; }
                set
                {
                    if (_mainText == value)
                        return;

                    _mainText = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MainText)));
                }
            }

            #endregion

            #region SubText

            private string subText = string.Empty;

            /// <summary>
            /// Gets or sets the SubText property. This observable property 
            /// indicates the sub text.
            /// </summary>
            public string SubText
            {
                get { return subText; }
                set
                {
                    if (subText == value)
                        return;

                    subText = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SubText)));
                }
            }

            #endregion
        }

        private void InitializeFluidPivotPanel()
        {
            var items = new ObservableCollection<PivotItem>();

            var colors = new[] { "first", "second", "disabled", "fourth" };
            var brushes = new Brush[] { Brushes.Orange, Brushes.Green, Brushes.Blue, Brushes.Magenta, Brushes.Cyan };
            var data = new List<List<TextMessage>>
            {
                new List<TextMessage>()
                {
                    new TextMessage {MainText = "design one", SubText = "Lorem ipsum dolor sit amet"},
                    new TextMessage {MainText = "design two", SubText = "consectetur adipisicing elit"},
                    new TextMessage {MainText = "design three", SubText = "sed do eiusmod tempor incididunt"},
                    new TextMessage {MainText = "design four", SubText = "ut labore et dolore magna aliqua"},
                    new TextMessage {MainText = "design five", SubText = "Ut enim ad minim veniam"},
                    new TextMessage {MainText = "design six", SubText = "quis nostrud exercitation ullamco laboris"},
                    new TextMessage {MainText = "design seven", SubText = "nisi ut aliquip ex ea commodo consequat"}
                },
                new List<TextMessage>()
                {
                    new TextMessage {MainText = "runtime one", SubText = "Lorem ipsum dolor sit amet"},
                    new TextMessage {MainText = "runtime two", SubText = "consectetur adipisicing elit"},
                    new TextMessage {MainText = "runtime three", SubText = "sed do eiusmod tempor incididunt"},
                    new TextMessage {MainText = "runtime four", SubText = "ut labore et dolore magna aliqua"},
                    new TextMessage {MainText = "runtime five", SubText = "Ut enim ad minim veniam"},
                    new TextMessage {MainText = "runtime six", SubText = "quis nostrud exercitation ullamco laboris"},
                    new TextMessage {MainText = "runtime seven", SubText = "nisi ut aliquip ex ea commodo consequat"}
                },
                new List<TextMessage>()
                {
                    new TextMessage {MainText = "method one", SubText = "Lorem ipsum dolor sit amet"},
                    new TextMessage {MainText = "method two", SubText = "consectetur adipisicing elit"},
                    new TextMessage {MainText = "method three", SubText = "sed do eiusmod tempor incididunt"},
                    new TextMessage {MainText = "method four", SubText = "ut labore et dolore magna aliqua"},
                    new TextMessage {MainText = "method five", SubText = "Ut enim ad minim veniam"},
                    new TextMessage {MainText = "method six", SubText = "quis nostrud exercitation ullamco laboris"},
                    new TextMessage {MainText = "method seven", SubText = "nisi ut aliquip ex ea commodo consequat"}
                },
                new List<TextMessage>()
                {
                    new TextMessage {MainText = "solution one", SubText = "Lorem ipsum dolor sit amet"},
                    new TextMessage {MainText = "solution two", SubText = "consectetur adipisicing elit"},
                    new TextMessage {MainText = "solution three", SubText = "sed do eiusmod tempor incididunt"},
                    new TextMessage {MainText = "solution four", SubText = "ut labore et dolore magna aliqua"},
                    new TextMessage {MainText = "solution five", SubText = "Ut enim ad minim veniam"},
                    new TextMessage {MainText = "solution six", SubText = "quis nostrud exercitation ullamco laboris"},
                    new TextMessage {MainText = "solution seven", SubText = "nisi ut aliquip ex ea commodo consequat"}
                }
            };

            for (var i = 0; i < colors.Count(); i++)
            {
                var tb = new PivotHeaderControl
                {
                    FontFamily = new FontFamily("Segoe UI"),
                    FontWeight = FontWeights.Light,
                    ActiveForeground = Brushes.White,
                    InactiveForeground = new SolidColorBrush(Color.FromRgb(48, 48, 48)),
                    DisabledForeground = Brushes.Red,
                    FontSize = 42,
                    Content = colors[i],
                    Margin = new Thickness(20, 0, 0, 0),
                    IsEnabled = !((i > 0) && ((i % 2) == 0))
                };

                var pci = new PivotContentControl();
                var lb = new ListBox
                {
                    FontFamily = new FontFamily("Segoe UI"),
                    FontSize = 24,
                    FontWeight = FontWeights.Light,
                    Foreground = brushes[i],
                    Background = new SolidColorBrush(Color.FromRgb(16, 16, 16)),
                    BorderThickness = new Thickness(0),
                    ItemTemplate = (DataTemplate)this.Resources["ListBoxItemTemplate"],
                    ItemsSource = data[i],
                };
                ScrollViewer.SetHorizontalScrollBarVisibility(lb, ScrollBarVisibility.Disabled);
                lb.HorizontalAlignment = HorizontalAlignment.Stretch;
                lb.VerticalAlignment = VerticalAlignment.Stretch;
                lb.Margin = new Thickness(30, 10, 10, 10);
                pci.Content = lb;

                var pi = new PivotItem { PivotHeader = tb, PivotContent = pci };
                //pi.SetActive(false);
                items.Add(pi);
            }

            RootPivotPanel.ItemsSource = items;

            RootPivotPanel.Background = new SolidColorBrush(Color.FromRgb(16, 16, 16));
        }

        private void OnResetFluidPivotPanel(object sender, RoutedEventArgs e)
        {
            RootPivotPanel.Reset();
        }

        #endregion

        // -------------------------------------------------------------------------------------
        // FluidProgressBar
        // -------------------------------------------------------------------------------------

        #region FluidProgressBar

        private void InitializeFluidProgressBars()
        {
            ProgressBarA.Visibility = Visibility.Collapsed;
            ProgressBarB.Visibility = Visibility.Collapsed;
        }

        #endregion

        // -------------------------------------------------------------------------------------
        // FluidStatusBar
        // -------------------------------------------------------------------------------------

        #region FluidStatusBar

        void DoWork(object sender, DoWorkEventArgs e)
        {
            if (bgWorker.CancellationPending)
                return;

            StatusMessage msg = new StatusMessage("Verifying Code!", true);
            bgWorker.ReportProgress(0, msg);

            Thread.Sleep(200 + _rnd.Next(600));

            if (bgWorker.CancellationPending)
                return;

            msg.Message = "Verifying : 10%";
            msg.IsAnimated = true;
            bgWorker.ReportProgress(0, msg);

            Thread.Sleep(100 + _rnd.Next(400));

            for (int i = 1; i < 10; i++)
            {
                if (bgWorker.CancellationPending)
                    return;

                msg.Message = String.Format("Verifying : {0}%", (i + 1) * 10);
                msg.IsAnimated = false;
                bgWorker.ReportProgress(0, msg);
                Thread.Sleep(200 + _rnd.Next(200));
            }

            Thread.Sleep(750);

            if (bgWorker.CancellationPending)
                return;

            msg.Message = "Compiling Code!";
            msg.IsAnimated = true;
            bgWorker.ReportProgress(0, msg);

            Thread.Sleep(300 + _rnd.Next(200));
            if (bgWorker.CancellationPending)
                return;

            msg.Message = "Compiling : 10%";
            msg.IsAnimated = true;
            bgWorker.ReportProgress(0, msg);

            Thread.Sleep(100 + _rnd.Next(200));

            for (int i = 1; i < 10; i++)
            {
                if (bgWorker.CancellationPending)
                    return;

                msg.Message = String.Format("Compiling : {0}%", (i + 1) * 10);
                msg.IsAnimated = false;
                bgWorker.ReportProgress(0, msg);
                Thread.Sleep(300);
            }

            Thread.Sleep(200 + _rnd.Next(600));

            if (bgWorker.CancellationPending)
                return;

            msg.Message = "Linking!";
            msg.IsAnimated = true;
            bgWorker.ReportProgress(0, msg);

            Thread.Sleep(200 + _rnd.Next(600));
            if (bgWorker.CancellationPending)
                return;

            msg.Message = "Linking : 10%";
            msg.IsAnimated = true;
            bgWorker.ReportProgress(0, msg);

            Thread.Sleep(100 + _rnd.Next(300));

            for (int i = 1; i < 10; i++)
            {
                if (bgWorker.CancellationPending)
                    return;

                msg.Message = String.Format("Linking : {0}%", (i + 1) * 10);
                msg.IsAnimated = false;
                bgWorker.ReportProgress(0, msg);
                Thread.Sleep(200 + _rnd.Next(100));
            }

            Thread.Sleep(200 + _rnd.Next(300));

            if (bgWorker.CancellationPending)
                return;

            msg.Message = "Build Completed!";
            msg.IsAnimated = true;
            bgWorker.ReportProgress(0, msg);
        }

        void OnProgress(object sender, ProgressChangedEventArgs e)
        {
            StatusMessage msg = e.UserState as StatusMessage;
            if (msg != null)
            {
                customStatusBar.SetStatus(msg.Message, msg.IsAnimated);
            }
        }

        void OnWorkCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            isBGWorking = false;
            StartBtn.IsEnabled = true;
            DirectionCB.IsEnabled = true;
            StopBtn.IsEnabled = false;
        }

        private void DirectionCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (DirectionCB.SelectedIndex)
            {
                case 1: // Right
                    customStatusBar.FadeOutDirection = StatusDirection.Right;
                    customStatusBar.FadeOutDistance = 300;
                    customStatusBar.FadeOutDuration = new Duration(TimeSpan.FromSeconds(1));
                    customStatusBar.MoveDuration = new Duration(TimeSpan.FromSeconds(0.5));
                    break;
                case 2: // Up
                    customStatusBar.FadeOutDirection = StatusDirection.Up;
                    customStatusBar.FadeOutDistance = 50;
                    customStatusBar.FadeOutDuration = new Duration(TimeSpan.FromSeconds(0.75));
                    customStatusBar.MoveDuration = new Duration(TimeSpan.FromSeconds(0.35));
                    break;
                case 3: // Down
                    customStatusBar.FadeOutDirection = StatusDirection.Down;
                    customStatusBar.FadeOutDistance = 50;
                    customStatusBar.FadeOutDuration = new Duration(TimeSpan.FromSeconds(0.75));
                    customStatusBar.MoveDuration = new Duration(TimeSpan.FromSeconds(0.35));
                    break;
                case 0: // Left
                default:
                    customStatusBar.FadeOutDirection = StatusDirection.Left;
                    customStatusBar.FadeOutDistance = 300;
                    customStatusBar.FadeOutDuration = new Duration(TimeSpan.FromSeconds(1));
                    customStatusBar.MoveDuration = new Duration(TimeSpan.FromSeconds(0.5));
                    break;
            }
        }

        private void StartBtn_Click(object sender, RoutedEventArgs e)
        {
            StartBtn.IsEnabled = false;
            DirectionCB.IsEnabled = false;

            bgWorker.RunWorkerAsync();
            isBGWorking = true;

            StopBtn.IsEnabled = true;
        }

        private void StopBtn_Click(object sender, RoutedEventArgs e)
        {
            StopBtn.IsEnabled = false;

            if (isBGWorking)
            {
                bgWorker.CancelAsync();
            }
        }

        #endregion

        // -------------------------------------------------------------------------------------
        // SplitView Menu
        // -------------------------------------------------------------------------------------

        #region SplitView Menu

        private int GetColumnZeroWidth()
        {
            // determine how wide column zero should be based on window size
            // if window is maximized, column zero width is equal to current menu width.
            // if window is normal, column zero width is equal to narrow menu width
            return WindowState == WindowState.Maximized
                       ? (int)SplitViewMenu.Width
                       : (int)SplitViewMenuWidth.Narrow;

        }

        private void OnMenuButtonClicked(object sender, RoutedEventArgs e)
        {
            // toggle menu width
            SplitViewMenu.Width = (int)SplitViewMenu.Width == (int)SplitViewMenuWidth.Narrow
                                      ? (int)SplitViewMenuWidth.Wide
                                      : (int)SplitViewMenuWidth.Narrow;

            // reset column width in the column definition based on window size
            RootGrid.ColumnDefinitions[0].Width = new GridLength(GetColumnZeroWidth());
        }

        private void OnSprocketControl(object sender, RoutedEventArgs e)
        {
            sprocketControl1.Visibility = Visibility.Visible;
            sprocketControl2.Visibility = Visibility.Visible;

            SplitViewMenu.Width = GetColumnZeroWidth();
            CurrentAppMode = AppMode.SprocketControl;
            AppTitle = Enum.GetName(typeof (AppMode), CurrentAppMode);
        }

        private void OnHideSprocketControl(object sender, RoutedEventArgs e)
        {
            // Hide the indeterminate sprocket controls so that they do not 
            // consume CPU when not visible
            sprocketControl1.Visibility = Visibility.Collapsed;
            sprocketControl2.Visibility = Visibility.Collapsed;
        }

        private void OnToggleSwitch(object sender, RoutedEventArgs e)
        {
            SplitViewMenu.Width = GetColumnZeroWidth();
            CurrentAppMode = AppMode.ToggleSwitch;
            AppTitle = Enum.GetName(typeof (AppMode), CurrentAppMode);
        }

        private void OnFluidWrapPanel(object sender, RoutedEventArgs e)
        {
            SplitViewMenu.Width = GetColumnZeroWidth();
            CurrentAppMode = AppMode.FluidWrapPanel;
            AppTitle = Enum.GetName(typeof (AppMode), CurrentAppMode);
            RefreshFluidWrapPanel();
        }

        private void OnSparkWindow(object sender, RoutedEventArgs e)
        {
            SplitViewMenu.Width = GetColumnZeroWidth();
            CurrentAppMode = AppMode.SparkWindow;
            AppTitle = Enum.GetName(typeof (AppMode), CurrentAppMode);
        }

        private void OnFluidPivotPanel(object sender, RoutedEventArgs e)
        {
            SplitViewMenu.Width = GetColumnZeroWidth();
            CurrentAppMode = AppMode.FluidPivotPanel;
            AppTitle = Enum.GetName(typeof (AppMode), CurrentAppMode);
        }

        private void OnFluidProgressBar(object sender, RoutedEventArgs e)
        {
            ProgressBarA.Visibility = Visibility.Visible;
            ProgressBarB.Visibility = Visibility.Visible;

            SplitViewMenu.Width = GetColumnZeroWidth();
            CurrentAppMode = AppMode.FluidProgressBar;
            AppTitle = Enum.GetName(typeof (AppMode), CurrentAppMode);
        }

        private void OnHideProgressBar(object sender, RoutedEventArgs e)
        {
            // Hide the progress bars so that they do not 
            // consume CPU when not visible
            ProgressBarA.Visibility = Visibility.Collapsed;
            ProgressBarB.Visibility = Visibility.Collapsed;
        }

        private void OnFluidStatusBar(object sender, RoutedEventArgs e)
        {
            SplitViewMenu.Width = GetColumnZeroWidth();
            CurrentAppMode = AppMode.FluidStatusBar;
            AppTitle = Enum.GetName(typeof (AppMode), CurrentAppMode);
        }

        #endregion
    }
}
