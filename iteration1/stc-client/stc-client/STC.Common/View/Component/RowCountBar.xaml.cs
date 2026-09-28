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

namespace STC.Common.View.Component
{
    /// <summary>
    /// RowCountBar.xaml 的交互逻辑
    /// </summary>
    public partial class RowCountBar : UserControl
    {

        public int RowCount
        {
            get { return (int)GetValue(RowCountProperty); }
            set { SetValue(RowCountProperty, value); }
        }

        // Using a DependencyProperty as the backing store for RowCount.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RowCountProperty =
            DependencyProperty.Register("RowCount", typeof(int), typeof(RowCountBar), new PropertyMetadata(0, onRowCountChanged));

        private static void onRowCountChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            RowCountBar source = d as RowCountBar;
            string newValue = e.NewValue != null ? e.NewValue.ToString() : string.Empty;
            if (source.rowCount_run.Text != newValue)
                source.rowCount_run.Text = newValue;
        }

        public int BidRowCount
        {
            get { return (int)GetValue(BidRowCountProperty); }
            set { SetValue(BidRowCountProperty, value); }
        }

        // Using a DependencyProperty as the backing store for BidRowCount.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BidRowCountProperty =
            DependencyProperty.Register("BidRowCount", typeof(int), typeof(RowCountBar), new PropertyMetadata(0, onBidRowCountChanged));

        private static void onBidRowCountChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            RowCountBar source = d as RowCountBar;
            string newValue = e.NewValue != null ? e.NewValue.ToString() : string.Empty;
            if (source.bidRowCount_run.Text != newValue)
                source.bidRowCount_run.Text = newValue;
        }

        public int OfrRowCount
        {
            get { return (int)GetValue(OfrRowCountProperty); }
            set { SetValue(OfrRowCountProperty, value); }
        }

        // Using a DependencyProperty as the backing store for OfrRowCount.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty OfrRowCountProperty =
            DependencyProperty.Register("OfrRowCount", typeof(int), typeof(RowCountBar), new PropertyMetadata(0, onOfrRowCountChanged));

        private static void onOfrRowCountChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            RowCountBar source = d as RowCountBar;
            string newValue = e.NewValue != null ? e.NewValue.ToString() : string.Empty;
            if (source.ofrRowCount_run.Text != newValue)
                source.ofrRowCount_run.Text = newValue;
        }

        public RowCountBar()
        {
            InitializeComponent();
        }
    }
}
