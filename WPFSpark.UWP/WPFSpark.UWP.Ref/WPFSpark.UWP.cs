namespace WPFSpark
{
    public partial class FluidPointerDragBehavior : Microsoft.Xaml.Interactivity.Behavior<Windows.UI.Xaml.UIElement>
    {
        public static readonly Windows.UI.Xaml.DependencyProperty DragButtonProperty;
        public FluidPointerDragBehavior() { }
        public WPFSpark.FluidPointerDragBehavior.DragButtonType DragButton { get { return default(WPFSpark.FluidPointerDragBehavior.DragButtonType); } set { } }
        protected override void OnAttached() { }
        protected override void OnDetaching() { }
        public enum DragButtonType
        {
            MouseLeftButton = 0,
            MouseMiddleButton = 1,
            MouseRightButton = 2,
            Pen = 3,
            Touch = 4,
        }
    }
    public sealed partial class FluidWrapPanel : Windows.UI.Xaml.Controls.Panel
    {
        public static System.TimeSpan DEFAULT_ANIMATION_TIME_WITH_EASING;
        public static System.TimeSpan DEFAULT_ANIMATION_TIME_WITHOUT_EASING;
        public const double DEFAULT_ITEM_HEIGHT = 10;
        public const double DEFAULT_ITEM_WIDTH = 10;
        public const double DRAG_OPACITY_DEFAULT = 0.6;
        public const double DRAG_SCALE_DEFAULT = 1.3;
        public static readonly Windows.UI.Xaml.DependencyProperty DragEasingProperty;
        public static readonly Windows.UI.Xaml.DependencyProperty DragOpacityProperty;
        public static readonly Windows.UI.Xaml.DependencyProperty DragScaleProperty;
        public static readonly Windows.UI.Xaml.DependencyProperty ElementEasingProperty;
        public static System.TimeSpan FIRST_TIME_ANIMATION_DURATION;
        public static readonly Windows.UI.Xaml.DependencyProperty FluidItemsProperty;
        public static readonly Windows.UI.Xaml.DependencyProperty IsComposingProperty;
        public static readonly Windows.UI.Xaml.DependencyProperty ItemHeightProperty;
        public static readonly Windows.UI.Xaml.DependencyProperty ItemsSourceProperty;
        public static readonly Windows.UI.Xaml.DependencyProperty ItemWidthProperty;
        public const double NORMAL_OPACITY = 1;
        public const double NORMAL_SCALE = 1;
        public const double OPACITY_MIN = 0.1;
        public static readonly Windows.UI.Xaml.DependencyProperty OptimizeChildPlacementProperty;
        public static readonly Windows.UI.Xaml.DependencyProperty OrientationProperty;
        public const int Z_INDEX_DRAG = 10;
        public const int Z_INDEX_INTERMEDIATE = 1;
        public const int Z_INDEX_NORMAL = 0;
        public FluidWrapPanel() { }
        public Windows.UI.Xaml.Media.Animation.EasingFunctionBase DragEasing { get { return default(Windows.UI.Xaml.Media.Animation.EasingFunctionBase); } set { } }
        public double DragOpacity { get { return default(double); } set { } }
        public double DragScale { get { return default(double); } set { } }
        public Windows.UI.Xaml.Media.Animation.EasingFunctionBase ElementEasing { get { return default(Windows.UI.Xaml.Media.Animation.EasingFunctionBase); } set { } }
        public System.Collections.ObjectModel.ObservableCollection<Windows.UI.Xaml.UIElement> FluidItems { get { return default(System.Collections.ObjectModel.ObservableCollection<Windows.UI.Xaml.UIElement>); } }
        public bool IsComposing { get { return default(bool); } set { } }
        public double ItemHeight { get { return default(double); } set { } }
        public System.Collections.IEnumerable ItemsSource { get { return default(System.Collections.IEnumerable); } set { } }
        public double ItemWidth { get { return default(double); } set { } }
        public bool OptimizeChildPlacement { get { return default(bool); } set { } }
        public Windows.UI.Xaml.Controls.Orientation Orientation { get { return default(Windows.UI.Xaml.Controls.Orientation); } set { } }
        protected override Windows.Foundation.Size ArrangeOverride(Windows.Foundation.Size finalSize) { return default(Windows.Foundation.Size); }
        protected override Windows.Foundation.Size MeasureOverride(Windows.Foundation.Size availableSize) { return default(Windows.Foundation.Size); }
    }
    public static partial class Utils
    {
        public static Windows.Foundation.Size CollapseThickness(this Windows.UI.Xaml.Thickness thick) { return default(Windows.Foundation.Size); }
        public static Windows.Foundation.Rect Deflate(this Windows.Foundation.Rect rect, Windows.UI.Xaml.Thickness thick) { return default(Windows.Foundation.Rect); }
        public static Windows.Foundation.Rect Inflate(this Windows.Foundation.Rect rect, Windows.UI.Xaml.Thickness thick) { return default(Windows.Foundation.Rect); }
        public static bool IsCloseTo(this double value1, double value2) { return default(bool); }
        public static bool IsCloseTo(this Windows.Foundation.Point point1, Windows.Foundation.Point point2) { return default(bool); }
        public static bool IsCloseTo(this Windows.Foundation.Rect rect1, Windows.Foundation.Rect rect2) { return default(bool); }
        public static bool IsCloseTo(this Windows.Foundation.Size size1, Windows.Foundation.Size size2) { return default(bool); }
        public static bool IsEqualTo(this Windows.UI.Xaml.Media.Brush brush, Windows.UI.Xaml.Media.Brush otherBrush) { return default(bool); }
        public static bool IsGreaterThan(this double value1, double value2) { return default(bool); }
        public static bool IsLessThan(double value1, double value2) { return default(bool); }
        public static bool IsNaN(double value) { return default(bool); }
        public static bool IsOne(this double value) { return default(bool); }
        public static bool IsOpaqueSolidColorBrush(this Windows.UI.Xaml.Media.Brush brush) { return default(bool); }
        public static bool IsUniform(this Windows.UI.Xaml.CornerRadius corner) { return default(bool); }
        public static bool IsUniform(this Windows.UI.Xaml.Thickness thick) { return default(bool); }
        public static bool IsValid(this Windows.UI.Xaml.CornerRadius corner, bool allowNegative, bool allowNaN, bool allowPositiveInfinity, bool allowNegativeInfinity) { return default(bool); }
        public static bool IsValid(this Windows.UI.Xaml.Thickness thick, bool allowNegative, bool allowNaN, bool allowPositiveInfinity, bool allowNegativeInfinity) { return default(bool); }
        public static bool IsZero(this double value) { return default(bool); }
        public static bool IsZero(this Windows.UI.Xaml.CornerRadius corner) { return default(bool); }
        public static bool IsZero(this Windows.UI.Xaml.Thickness thick) { return default(bool); }
        public static double RoundLayoutValue(double value, double dpiScale) { return default(double); }
    }
}
