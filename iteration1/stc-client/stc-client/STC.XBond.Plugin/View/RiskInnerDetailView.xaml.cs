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

namespace STC.XBond.Plugin.View
{
    /// <summary>
    /// RiskInnerDetailView.xaml 的交互逻辑
    /// </summary>
    public partial class RiskInnerDetailView : UserControl
    {
        public RiskInnerDetailView()
        {
            InitializeComponent();
        }

        // 过滤属性
        public static readonly DependencyProperty LimitHeightProperty = DependencyProperty.Register("LimitActualHeight", typeof(string),
            typeof(RiskInnerDetailView), new PropertyMetadata(string.Empty));

        public string LimitActualHeight
        {
            get { return (string)GetValue(LimitHeightProperty); }
            set
            {
                SetValue(LimitHeightProperty, value);


                RaiseEvent(new RoutedEventArgs { RoutedEvent = LimitHeightChangedEvent });
            }
        }

        public static readonly RoutedEvent LimitHeightChangedEvent = EventManager.RegisterRoutedEvent("LimitHeightChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(RiskInnerDetailView));

        public event RoutedEventHandler LimitHeightChanged
        {
            add { AddHandler(LimitHeightChangedEvent, value); }
            remove { RemoveHandler(LimitHeightChangedEvent, value); }
        }

        private void bdLimit_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            LimitActualHeight = e.NewSize.Height.ToString();
        }
    }
}
