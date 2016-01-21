using System;
using System.Collections.Generic;
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

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace WPFSpark.UWP.Client
{
    public sealed partial class FluidItemControl : UserControl
    {
        #region Fill

        /// <summary>
        /// Fill Dependency Property
        /// </summary>
        public static readonly DependencyProperty FillProperty =
            DependencyProperty.Register("Fill", typeof(Brush), typeof(FluidItemControl),
                new PropertyMetadata(new SolidColorBrush(Colors.Transparent)));

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
                new PropertyMetadata(string.Empty));

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
            this.InitializeComponent();
        }
    }
}
