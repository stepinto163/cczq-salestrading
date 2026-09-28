using STC.Common.DataProcesser;
using STC.Common.Utils;
using STC.Common.ViewModel.MyTrading;
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

namespace STC.Common.View.MyTrading
{
    /// <summary>
    /// MyExes.xaml 的交互逻辑
    /// </summary>
    public partial class MyExes : UserControl
    {


        public string TradeTabSelectedItem
        {
            get { return (string)GetValue(TradeTabSelectedItemProperty); }
            set { SetValue(TradeTabSelectedItemProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TradeTabSelectedItem.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TradeTabSelectedItemProperty =
            DependencyProperty.Register("TradeTabSelectedItem", typeof(string), typeof(MyExes), new PropertyMetadata(string.Empty));


        public string TotalCountExecutionsHeader
        {
            get { return (string)GetValue(TotalCountExecutionsHeaderProperty); }
            set { SetValue(TotalCountExecutionsHeaderProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TotalCountExecutionsHeader.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TotalCountExecutionsHeaderProperty =
            DependencyProperty.Register("TotalCountExecutionsHeader", typeof(string), typeof(MyExes), new PropertyMetadata("我的成交"));




        public string IsMyExecutionsShowRedDot
        {
            get { return (string)GetValue(IsMyExecutionsShowRedDotProperty); }
            set { SetValue(IsMyExecutionsShowRedDotProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsMyExecutionsShowRedDot.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsMyExecutionsShowRedDotProperty =
            DependencyProperty.Register("IsMyExecutionsShowRedDot", typeof(string), typeof(MyExes), new PropertyMetadata(string.Empty));



        MyExesViewModel _myTraderViewModel = new MyExesViewModel();
        ContextMenu contextMenu;

        public MyExes()
        {
            InitializeComponent();
            this.DataContext = _myTraderViewModel;
            MyExecutionProcesser.Processer.OnLastExecutions += Processer_OnLastExecutions;
        }

        internal void Processer_OnLastExecutions(object obj)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                int cnt = int.Parse(obj.ToString());
                if (cnt == 0)
                {
                    IsMyExecutionsShowRedDot = "";
                    TotalCountExecutionsHeader = "我的成交";
                }
                else
                {
                    try
                    {
                        _myTraderViewModel.RefreshID();

                        if (TradeTabSelectedItem == "MyExes_TabItem")
                        {
                            IsMyExecutionsShowRedDot = "";
                        }
                        else
                        {
                            IsMyExecutionsShowRedDot = "+0";
                        }
                    }
                    catch
                    {
                        IsMyExecutionsShowRedDot = "+0";
                    }

                    TotalCountExecutionsHeader = $"我的成交({cnt})";
                }
            });
        }

        private void UserControl_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {

        }

        private void copyRow_Click(object sender, RoutedEventArgs e)
        {
            //我的成交
            string value = _myTraderViewModel.CopyRowText(this.MyExecutions_Intention_dataGrid.SelectedItems, "Deals");
            ClipboardHelper.CopyToClipboard(value);
            MyExecutions_Intention_dataGrid.UnselectAll();
        }

        private void copySTCCode_Click(object sender, RoutedEventArgs e)
        {
            //我的成交
            string value = _myTraderViewModel.CopySTCCodeText(this.MyExecutions_Intention_dataGrid.SelectedItems, "Deals");
            ClipboardHelper.CopyToClipboard(value);
            MyExecutions_Intention_dataGrid.UnselectAll();
        }

        private void myExeFilter_FilterDicChanged(object sender, RoutedEventArgs e)
        {
            if (_myTraderViewModel != null)
            {
                _myTraderViewModel.ProcessMyExecutionsFilterConditon(this.myExeFilter.FilterDic);
            }
        }

        private void SetContextMenu()
        {
            if (contextMenu == null)
            {
                contextMenu = (ContextMenu)this.FindResource("CopyContextMenu");
            }
        }

        private void MyExecutions_dataGrid_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (this.MyExecutions_Intention_dataGrid.SelectedItems.Count > 0)
            {
                SetContextMenu();
                contextMenu.IsOpen = true;
            }
        }
    }
}
