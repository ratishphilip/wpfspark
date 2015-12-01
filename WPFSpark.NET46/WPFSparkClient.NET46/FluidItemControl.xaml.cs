using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WPFSparkClient.NET46
{
    /// <summary>
    /// Interaction logic for FluidItemControl.xaml
    /// </summary>
    public partial class FluidItemControl : UserControl
    {
        #region Fill

        /// <summary>
        /// Fill Dependency Property
        /// </summary>
        public static readonly DependencyProperty FillProperty =
            DependencyProperty.Register("Fill", typeof(Brush), typeof(FluidItemControl),
                new FrameworkPropertyMetadata(Brushes.Transparent));

        /// <summary>
        /// Gets or sets the Fill property. This dependency property 
        /// indicates the fill color of the inner content.
        /// </summary>
        public Brush Fill
        {
            get { return (Brush)GetValue(FillProperty); }
            set { SetValue(FillProperty, value); }
        }

        #endregion

        #region Data

        /// <summary>
        /// Data Dependency Property
        /// </summary>
        public static readonly DependencyProperty DataProperty =
            DependencyProperty.Register("Data", typeof(string), typeof(FluidItemControl),
                new FrameworkPropertyMetadata(string.Empty));

        /// <summary>
        /// Gets or sets the Data property. This dependency property 
        /// indicates the data to be displayed.
        /// </summary>
        public string Data
        {
            get { return (string)GetValue(DataProperty); }
            set { SetValue(DataProperty, value); }
        }

        #endregion

        public FluidItemControl()
        {
            InitializeComponent();
        }
    }
}
