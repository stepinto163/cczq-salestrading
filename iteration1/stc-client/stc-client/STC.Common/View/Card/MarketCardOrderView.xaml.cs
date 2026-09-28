using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using Newtonsoft.Json;
using STC.Common.Model.Chart;
using STC.Common.Utils;
using STC.Common.ViewModel;
using STC.Common.ViewModel.Card;

namespace STC.Common.View.Card
{
    /// <summary>
    /// MarketOrdrView.xaml 的交互逻辑
    /// </summary>
    public partial class MarketCardOrderView : UserControl
    {
        private LastExecutionViewModel _lastExecutionViewModel = new LastExecutionViewModel();
        private string LastSecurtityID = string.Empty;

        public MarketCardOrderView()
        {
            InitializeComponent();
            this.DataContext = _lastExecutionViewModel;

            this.Unloaded += MarketOrderView_Unloaded;
            this.SizeChanged += MarketCardOrderView_SizeChanged;

        }

        private void MarketCardOrderView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Tab_DgPrice.Width = e.NewSize.Width;
            if (this.DataContext != null && this.DataContext is LastExecutionViewModel)
            {
                LastExecutionViewModel viewModel = (LastExecutionViewModel)this.DataContext;
                if (int.Parse(viewModel.OhlcHeight) > 0)
                {
                    if (e.NewSize.Height > 0)
                    {
                        if (e.NewSize.Height - 24 > 0)
                        {
                            Tab_DgPrice.Height = e.NewSize.Height - 24;
                        }
                        else
                        {
                            Tab_DgPrice.Height = 24;
                        }
                    }
                }
                else
                {
                    Tab_DgPrice.Height = e.NewSize.Height;
                }
            }
        }

        private void MarketOrderView_Unloaded(object sender, RoutedEventArgs e)
        {
            //this.Unloaded -= MarketOrderView_Unloaded;

            //if (_lastExecutionViewModel != null)
            //    _lastExecutionViewModel.Dispose();
        }

        private void MarketExecution_Initialized(object sender, EventArgs e)
        {
            
        }

        public static readonly DependencyProperty SecurtityIDProperty = DependencyProperty.Register("SecurtityID", typeof(string),
            typeof(MarketCardOrderView), new PropertyMetadata("", new PropertyChangedCallback(OnSecurtityIDPropertyChange)));


        static async void OnSecurtityIDPropertyChange(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            if (args.NewValue == null) return;
            MarketCardOrderView source = (MarketCardOrderView)sender;
            if (source.DataContext != null && source.DataContext is LastExecutionViewModel)
            {
                LastExecutionViewModel viewModel = (LastExecutionViewModel)source.DataContext;
                //viewModel.SearchText = args.NewValue.ToString();
                viewModel.SetItemSource(args.NewValue.ToString());
            }
        }

        public async void SetSecurtityID(string securtityID, bool isReset = false)
        {
            //if ((string.IsNullOrEmpty(LastSecurtityID)) || (securtityID != LastSecurtityID))
            //{
                //LastSecurtityID = securtityID;

                    //if (isReset)
                    //{
                    //    _lastExecutionViewModel = new LastExecutionViewModel();
                    //    this.DataContext = _lastExecutionViewModel;
                    //}
                LastExecutionViewModel viewModel = (LastExecutionViewModel)this.DataContext;
                //viewModel.SearchText = securtityID;
                viewModel.SetItemSource(securtityID);
            //}
        }

        public string SecurtityID
        {
            get { return (string)GetValue(SecurtityIDProperty); }
            set { SetValue(SecurtityIDProperty, value); }
        }

        #region 新增成交图形插件处理功能
        private async void lbPirceType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.DataContext != null && this.DataContext is LastExecutionViewModel)
            {
                LastExecutionViewModel viewModel = (LastExecutionViewModel)this.DataContext;

                if (viewModel.ListTypeIndex == 0)
                {
                    Tab_DgPrice.SelectedIndex = 0;
                }
                else
                {
                    // 图表模式
                    Tab_DgPrice.SelectedIndex = 1;
                    if (viewModel.LinePageInstance == null)
                    {
                        // 动态加载外部程序集
                        viewModel.LoadAssembly();
                        MainFrame.Content = viewModel.LinePageInstance;
                    }

                    await viewModel.UpdateChartLineData();
                }
            }
        }

        #endregion

        private void btnKline_Click(object sender, RoutedEventArgs e)
        {
            string securityID = _lastExecutionViewModel.SearchText;
            
            string url = AppDomain.CurrentDomain.BaseDirectory + $"bin/HTML/KLine/KLineView.html?timestamp={new Random(10000).NextDouble()}";
            //string url = $"/STC.Chrome.Plugin;component/Html/KLine/KLineView.html?timestamp={new Random(10000).NextDouble()}";
            Common.Model.Chrome.ChromeWinModel cw = new Common.Model.Chrome.ChromeWinModel() { IsOpenInMainTabPage = true, Url = url, IsShowAddressBar = true, IsShowStatusBar = false };
            cw.Key = "JsC#KLineDemo-" + securityID;
            cw.Title = "K线分析";
            cw.UseChromePluginObject = true;
            cw.SecurityId = securityID;
            cw.CallBackObjectInstance.Add("cefJS", "KLineViewModel");
            //下面采用异步方式，前端Js也是异步调用的，因而在key后面加上| 告诉插件要异步注册到chrome中
            //cw.CallBackObjectInstance.Add("csharpTest|", "JavascriptCallCharpTest");
            AppUtils.Application.PluginManager.Application.BroadCast("OpenKLine", cw);
            
        }

        public void UpdatePriceType(int priceTypeIndex)
        {
            if (priceTypeIndex == 0) 
            {
                _lastExecutionViewModel.IsYieldType = "true";
            }
            else if (priceTypeIndex == 1)
            {
                _lastExecutionViewModel.IsYieldType = "false";
            }
        }
    }
}
