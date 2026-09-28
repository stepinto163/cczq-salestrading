using STC.Common.Utils;
using STC.Frame.Common;
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

namespace BDB.Trade.Plugin.View
{
    /// <summary>
    /// HistoryView.xaml 的交互逻辑
    /// </summary>
    public partial class HistoryView : UserControl
    {
        HistoryOrderView orderView;
        HistoryDealsView dealView;
        // HistoryRiskView riskView;
        HistoryMsgView msgView;
        // HistoryAgencyView agencyView;
        public string isClickNum = "1";
        public HistoryView()
        {
            InitializeComponent();
            orderView = new HistoryOrderView();
            dealView = new HistoryDealsView();
            // riskView = new HistoryRiskView();
            msgView = new HistoryMsgView();
            // agencyView = new HistoryAgencyView();

            historyFrame.Content = orderView;
            //orderView.load();

            isClickNum = "1";
            AppUtils.Application.Panel.SelectionChanged += Panel_SelectionChanged;
        }
        private int _selectPluginKey = 0;
        private void Panel_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TabControl tc = sender as TabControl;
            TabItem tabItem = tc.SelectedItem as TabItem;
            if (tabItem != null && tabItem.Tag != null)
            {
                int pluginKey = (int)tabItem.Tag;
                if ((int)tabItem.Tag != _selectPluginKey && (int)tabItem.Tag == (int)PluginKeyEnum.HistoryPlugin)
                {
                    Load();
                }

                _selectPluginKey = pluginKey;
            }
        }

        private void Load()
        {
            if (isClickNum.Equals("1") && orderView != null)
            {
                if (orderView.IsLoad)
                {
                    Console.WriteLine("IsLoad is true");
                    orderView.load();
                }
                else
                {
                    Console.WriteLine("IsLoad is false");
                }
            }
            else if (isClickNum.Equals("2") && dealView != null)
            {
                if (dealView.IsLoad)
                    dealView.load();
            }
            //else if (isClickNum.Equals("3") && riskView != null)
            //{
            //    if (riskView.IsLoad)
            //        riskView.load();
            //}
            else if (isClickNum.Equals("3") && msgView != null)
            {
                if (msgView.IsLoad)
                    msgView.load();
            }
            //else if (isClickNum.Equals("5") && agencyView != null)
            //{
            //    if (agencyView.IsLoad)
            //        agencyView.load();
            //}
        }

        private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {

        }

        private void ContentPresenter_MouseEnter(object sender, MouseEventArgs e)
        {

        }

        private void clock_img_MouseEnter(object sender, MouseEventArgs e)
        {

        }

        private void clock_img_MouseLeave(object sender, MouseEventArgs e)
        {

        }

        private void ordersRB_Click(object sender, RoutedEventArgs e)
        {
            historyFrame.Content = orderView;
            if (orderView.IsLoad)
            {
                Console.WriteLine("IsLoad is true");
                orderView.load();
            }
            else
            {
                Console.WriteLine("IsLoad is false");
            }
            isClickNum = "1";
        }

        private void dealsRB_Click(object sender, RoutedEventArgs e)
        {
            historyFrame.Content = dealView;
            if (dealView.IsLoad)
                dealView.load();
            isClickNum = "2";
        }

        //private void risksRB_Click(object sender, RoutedEventArgs e)
        //{
        //    historyFrame.Content = riskView;
        //    if (riskView.IsLoad)
        //        riskView.load();
        //    isClickNum = "3";
        //}

        private void messagesRB_Click(object sender, RoutedEventArgs e)
        {
            historyFrame.Content = msgView;
            if (msgView.IsLoad)
                msgView.load();
            isClickNum = "3";
        }

        //private void AgenciesRB_Click(object sender, RoutedEventArgs e)
        //{
        //    historyFrame.Content = agencyView;
        //    if (agencyView.IsLoad)
        //        agencyView.load();
        //    isClickNum = "5";
        //}

        public void SelectOrderView()
        {
            ordersRB.IsChecked = true;
            historyFrame.Content = orderView;
        }

        public void SelectDealView()
        {
            dealsRB.IsChecked = true;
            historyFrame.Content = dealView;
        }
    }
}
