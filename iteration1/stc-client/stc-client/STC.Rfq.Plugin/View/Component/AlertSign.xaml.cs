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

namespace STC.Rfq.Plugin.View.Component
{
    /// <summary>
    /// AlertSign.xaml 的交互逻辑
    /// </summary>
    public partial class AlertSign : UserControl
    {
        public static readonly DependencyProperty ToolTipTextProperty = DependencyProperty.Register("ToolTipText", typeof(string),
            typeof(AlertSign), new PropertyMetadata("5秒内与全市场最新成交不一致 偏离较大", new PropertyChangedCallback(OnAlertSignPropertyChange)));

        public AlertSign()
        {
            InitializeComponent();
        }

        public string ToolTipText
        {
            get { return (string)GetValue(ToolTipTextProperty); }
            set { SetValue(ToolTipTextProperty, value); }
        }

        static void OnAlertSignPropertyChange(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            if (args.NewValue == null) return;
            AlertSign source = (AlertSign)sender;

            string newValue = args.NewValue.ToString();
            source.btAlert.ToolTip = newValue;
        }
    }
}
