using STC.Common.Utils;
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
    /// CommonView.xaml 的交互逻辑
    /// </summary>
    public partial class CommonView : UserControl
    {

        public CommonView()
        {
            InitializeComponent();

            if (LocalEnv.ServerInfo.AppName.ToLower() != "bdb")
            {
                myTradeGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(8) });
                myTradeGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(32, GridUnitType.Star) });

                Button btnVer = new Button();
                myTradeGrid.Children.Add(btnVer);
                btnVer.Width = 8;
                btnVer.Height = 40;
                btnVer.Margin = new Thickness(1, 0, 1, 0);
                btnVer.Content = ">";
                btnVer.FontSize = 8;
                btnVer.Style = FindResource("SplitButtonStyle") as Style;
                btnVer.Click += BtnVer_Click;
                Panel.SetZIndex(btnVer, 1);
                Grid.SetRow(btnVer, 1);
                Grid.SetColumn(btnVer,1);

                GridSplitter gridSplitter = new GridSplitter();
                gridSplitter.Background = FindResource("BackgroundBrush") as SolidColorBrush;
                gridSplitter.Style = FindResource("VGridSplitterStyle2") as Style;
                myTradeGrid.Children.Add(gridSplitter);
                Grid.SetColumn(gridSplitter, 1);

                MarketExecution marketExecution = new MarketExecution();
                marketExecution.Background = FindResource("BackgroundBrush") as SolidColorBrush;
                myTradeGrid.Children.Add(marketExecution);
                Grid.SetColumn(marketExecution, 2);
            }
        }


        private bool _verClose = false;
        private void BtnVer_Click(object sender, RoutedEventArgs e)
        {
            Button btnVer = sender as Button;
            if (!_verClose)
            {
                myTradeGrid.ColumnDefinitions[2].Width = new GridLength(0);
                _verClose = true;

                btnVer.Content = "<";

            }
            else
            {
                myTradeGrid.ColumnDefinitions[0].Width = new GridLength(70, GridUnitType.Star);
                myTradeGrid.ColumnDefinitions[2].Width = new GridLength(30, GridUnitType.Star);
                _verClose = false;

                btnVer.Content = ">";
            }
        }
    }
}
