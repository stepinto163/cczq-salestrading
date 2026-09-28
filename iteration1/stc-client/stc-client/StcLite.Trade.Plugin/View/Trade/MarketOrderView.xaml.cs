using com.gateway.log;
using BDB.Trade.Plugin.Handler;
using BDB.Trade.Plugin.Utils;
using BDB.Trade.Plugin.ViewModel.Data;
using BDB.Trade.Plugin.ViewModel.Trade;
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
using BDB.Price.Plugin.Model;
using BDB.Trade.Plugin.ViewModel.Trade.Card;
using BDB.Price.Plugin.Chart.Deal;
using STC.Common.Utils;
using ClipboardHelper = BDB.Trade.Plugin.Utils.ClipboardHelper;
using STC.Common.Model.Admin;
using STC.Common.ViewModel.Data;
using STC.Common.Model.QQ;
using STC.Common.Model.Fix.Message;
using STC.Common.Handler.QQ;
using System.Windows.Threading;
using STC.Common.Model.Base;
using BDB.Trade.Plugin.View.Trade.Card;

namespace BDB.Trade.Plugin.View.Trade
{
    /// <summary>
    /// MarketOrdrView.xaml 的交互逻辑
    /// </summary>
    public partial class MarketOrderView : UserControl
    {
        private MarketOrderViewModel _marketOrderViewModel;

        public MarketOrderView()
        {
            InitializeComponent();

            MinRowCount = 3;

            //_marketOrderViewModel = new MarketOrderViewModel();
            this.DataContext = _marketOrderViewModel = new MarketOrderViewModel();
            _marketOrderViewModel.ExecChart = this.execChartView;

            this.SizeChanged += MarketOrderView_SizeChanged; ;

            this.Unloaded += MarketOrderView_Unloaded;

            this.Loaded += MarketOrderView_Loaded;
        }

        private void MarketOrderView_Loaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= MarketOrderView_Loaded;

            if (_marketOrderViewModel != null && _marketOrderViewModel.ShowModel == TabShowModel.Card && _marketOrderViewModel.CardShowType == Price.Plugin.Model.CardShowType.InCardPage)
            {
                LogHelper.Warn("Load SecurityId:" + _marketOrderViewModel.SearchText);
                string key = PageCardHandler.GetWindowHashKey(this);
                 
                CommonHandler.Instance.AddLoadNum(key);
            }
        }

        private void MarketOrderView_Unloaded(object sender, RoutedEventArgs e)
        {
            this.Unloaded -= MarketOrderView_Unloaded;

            //if (_marketOrderViewModel != null && _marketOrderViewModel.ShowModel == TabShowModel.Card)
            //    _marketOrderViewModel.Dispose();
        }

        public void SetShowModel(TabShowModel showModel)
        {
            _marketOrderViewModel.ShowModel = showModel;
        }

        public void Init(TabShowModel showModel, string security)
        {
           

            _marketOrderViewModel.ShowModel = showModel;
            _marketOrderViewModel.SearchText = security;
            

            _marketOrderViewModel.Init(this);


            if (showModel == TabShowModel.Card)
            {
                SetScrollVisibility("Hidden");
            }

            
        }

        //单独弹窗初始化
        public void Init(TabShowModel showModel, string security, CardShowType cardShowType)
        {

            _marketOrderViewModel.CardShowType = cardShowType;
            _marketOrderViewModel.ShowModel = showModel;
            _marketOrderViewModel.SearchText = security;


            _marketOrderViewModel.Init(this);


            if (showModel == TabShowModel.Card)
            {
                SetScrollVisibility("Hidden");

            }


        }


        public void SetScrollVisibility(string  visibility)
        {
            
            _marketOrderViewModel.HorizontalScrollBarVisibility = visibility;
            // dgMarket.VerticalScrollBarVisibility = dgMarket.HorizontalScrollBarVisibility = visibility;
        }

        //public void RefreshMarketData()
        //{
        //    _marketOrderViewModel.RefreshMarketData();
        //}

        #region 卡片适配逻辑

        public int MinRowCount { get; set; }


        private void MarketOrderView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (_marketOrderViewModel.ShowModel == TabShowModel.Card)
            {
                if (dgMarket.ActualHeight > 0)
                {
                    HeightChanged(dgMarket.ActualHeight);
                }
            }
        }

        public void HeightChanged(double height)
        {
            int row = (int)(height / 20);
            if (row <= 0)
            {
                return;
            }
            _marketOrderViewModel.RowHeight = height / row;
            //MinRowCount = row;
            //ObservableCollection<MarketExecutionsItem> observableCollection = _marketOrderViewModel.FilteredMarketExecutions.Source as ObservableCollection<MarketExecutionsItem>;
            //for (int i = observableCollection.Count - 1; i >= MinRowCount; i--)
            //{
            //    if (string.IsNullOrEmpty(dataSource[i].Broker))
            //    {
            //        dataSource.RemoveAt(i);
            //    }
            //}
            //for (int i = dataSource.Count; i < MinRowCount; i++)
            //{
            //    dataSource.Add(new DealtPo());
            //}
            //RefreshDataSource();
            //Console.WriteLine("DealtUI_HeightChanged: Height={0},row={1}", height, MinRowCount);
        }

        #endregion


        private void dgMarket_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            //bool isFuture = CommonUtils.IsFutureBond(_marketOrderViewModel.SearchText);

            //if (isFuture)
            {
                var scrollViewer = e.OriginalSource as ScrollViewer;
                if (scrollViewer != null)
                {
                    if (scrollViewer.VerticalOffset == 0)
                    {
                        // 滚动条在顶部
                        _marketOrderViewModel.HideTip();
                    }
                    else
                    {
                        // 滚动条不在顶部, 你的处理代码
                        _marketOrderViewModel.RefreshTip();
                    }
                }
            }
        }

        

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var scrollViewer = _marketOrderViewModel.GetDescendantByType(dgMarket, typeof(ScrollViewer)) as ScrollViewer;
            scrollViewer?.ScrollToTop();
        }

        int i_bid = 0;
        bool isDouble_bid = false;
        bool isCtrlBid = false;
        private async void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            isCtrlBid = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);
            i_bid += 1;
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(0, 0, 0, 0, System.Windows.Forms.SystemInformation.DoubleClickTime);
            timer.Tick += (s, e1) => { timer.IsEnabled = false; i_bid = 0; UIElement_Click(sender, e); isDouble_bid = false; isCtrlBid = false; };
            timer.IsEnabled = true;
            if (i_bid % 2 == 0)
            {
                timer.IsEnabled = false;
                i_bid = 0;
                isDouble_bid = true;

                CopyExeData(sender);
            }
        }

        private void UIElement_Click(object sender, MouseButtonEventArgs e)
        {
            if (isDouble_bid)
                return;

            if (isCtrlBid)
            {
                CopyExeData(sender);
            }
            else
            {
                CopyExeData_Single(sender);
            }
        }

        private void CopyExeData_Single(object sender)
        {
            if (sender is Border)
            {
                Border border = sender as Border;
                MarketExecutionsItem mei = border.DataContext as MarketExecutionsItem;
                if (mei != null)
                {
                    string remainingTime = string.Empty;
                    string securityID = mei.SecurityID;
                    string symbol = mei.Symbol;
                    string source = ComboSourceUtil.Instance.FindEnum(mei.SourceTrader, "venues_exe");
                    string side = ComboSourceUtil.Instance.FindEnum(mei.Side, "side");
                    string px = (mei.ExecutePricePercent_Display == CommonConstants.NO_PRICE || mei.ExecutePricePercent_Display == CommonConstants.NO_BROKER) ? string.Empty : mei.ExecutePricePercent_Display;
                    string time = CommonUtils.ConvertToDateTime(mei.Time).ToString("HH:mm:ss");
                    string val_yield = null;
                    if (this.Parent != null && this.Parent is FrameworkElement)
                    {
                        FrameworkElement fe = this.Parent as FrameworkElement;
                        if (fe != null && fe.DataContext != null && fe.DataContext is PriceAndOrderViewModel)
                        {
                            PriceAndOrderViewModel priceAndOrderViewModel = fe.DataContext as PriceAndOrderViewModel;
                            remainingTime = priceAndOrderViewModel.PriceModel.RemainingTime;
                            val_yield = priceAndOrderViewModel.PriceModel.CCDCValuation;
                        }
                    }

                    StringBuilder html = new StringBuilder();
                    StringBuilder plainTxt = new StringBuilder();

                    string newPx_html = string.Empty;
                    string newPx_Txt = string.Empty;
                    if (!string.IsNullOrEmpty(px))
                    {
                        newPx_html = $"&#9;{px}";
                        newPx_Txt = $" {px}";
                    }

                    html.Append($"<p>{remainingTime}&#9;{securityID}&#9;{symbol}&#9;{source}&#9;{side}{newPx_html}</p>");

                    plainTxt.Append($"{remainingTime} {securityID} {symbol} {source} {side}{newPx_Txt}");

                    

                    //复制
                    ClipboardHelper.CopyToClipboard(html.ToString(), plainTxt.ToString());

                    //if (LocalEnv.UserInfo.QeubeeFingPermissions == "1")
                    //{
                    //    string source_number = ComboSourceUtil.Instance.FindEnum(source, "venues_cn_tonumber");
                    //    FQHelper.SendHttpQQ(side, null, plainTxt.ToString(), securityID, symbol, string.Empty, source_number, mei.ExecutePricePercent_Display, mei.SettlType);
                    //    //await ResulfApiHelper.ManualSendMessageToQQRequest(qq, plainTxt.ToString());
                    //}

                    if (plainTxt.Length > 0)
                        CommonUtils.Prompt2Second("复制", "复制成功");
                }
            }
        }

        private void CopyExeData(object sender)
        {
            if (sender is Border)
            {
                Border border = sender as Border;
                MarketExecutionsItem mei = border.DataContext as MarketExecutionsItem;
                if (mei != null)
                {
                    string remainingTime = string.Empty;
                    string securityID = mei.SecurityID;
                    string symbol = mei.Symbol;
                    string source = ComboSourceUtil.Instance.FindEnum(mei.SourceTrader, "venues_exe");
                    string side = ComboSourceUtil.Instance.FindEnum(mei.Side, "side");
                    string px = (mei.ExecutePricePercent_Display == CommonConstants.NO_PRICE || mei.ExecutePricePercent_Display == CommonConstants.NO_BROKER) ? string.Empty : mei.ExecutePricePercent_Display;
                    string time = CommonUtils.ConvertToDateTime(mei.Time).ToString("HH:mm:ss");
                    string val_yield = null;
                        if (this.Parent != null && this.Parent is FrameworkElement)
                        {
                            FrameworkElement fe = this.Parent as FrameworkElement;
                            if (fe != null && fe.DataContext != null && fe.DataContext is PriceAndOrderViewModel)
                            {
                                PriceAndOrderViewModel priceAndOrderViewModel = fe.DataContext as PriceAndOrderViewModel;
                                remainingTime = priceAndOrderViewModel.PriceModel.RemainingTime;
                                val_yield = priceAndOrderViewModel.PriceModel.CCDCValuation;
                            }
                        }

                    StringBuilder html = new StringBuilder();
                    StringBuilder plainTxt = new StringBuilder();

                    string newPx_html = string.Empty;
                    string newPx_Txt = string.Empty;
                    if (!string.IsNullOrEmpty(px))
                    {
                        newPx_html = $"&#9;{px}";
                        newPx_Txt = $" {px}";
                    }

                    html.Append($"<p>[{remainingTime}&#9;{securityID}&#9;{symbol}&#9;{source}&#9;{side}{newPx_html}],{side}&#9;{securityID}{newPx_html}</p>");

                    plainTxt.Append($"[{remainingTime} {securityID} {symbol} {source} {side}{newPx_Txt}],{side} {securityID}{newPx_Txt}");
                    StringBuilder fqplainTxt = new StringBuilder();
                    string ccdc = CommonConstants.CDCValueTxt + val_yield;
                    fqplainTxt.Append($"[{remainingTime} {securityID} {symbol} {source} {side}{newPx_Txt} {ccdc}],{side} {securityID}{newPx_Txt}");
                    //复制
                    ClipboardHelper.CopyToClipboard(html.ToString(), plainTxt.ToString());

                    if (LocalEnv.UserInfo.QeubeeFingPermissions == "1")
                    {
                        string source_number = ComboSourceUtil.Instance.FindEnum(source, "venues_cn_tonumber");
                        //如果使用WebFinQ发送，则调用WebFinQ接口，否则调用QQ接口
                        if (LocalEnv.ConfInfo.IsForceUseWebFinQ == "1")
                        {
                            FQHelper.SendHttpQQ(side, null, fqplainTxt.ToString(), securityID, symbol, string.Empty, source_number, mei.ExecutePricePercent_Display, mei.SettlType, val_yield, mei.Time);
                        }
                        else
                        {
                            FQHelper.SendHttpQQ(side, null, plainTxt.ToString(), securityID, symbol, string.Empty, source_number, mei.ExecutePricePercent_Display, mei.SettlType, val_yield, mei.Time);
                        }
                        //await ResulfApiHelper.ManualSendMessageToQQRequest(qq, plainTxt.ToString());
                    }
                }
            }
        }

        //private void TitleBorker_Click(object sender, RoutedEventArgs e)
        //{
        //    CheckBox checkBox = sender as CheckBox;
        //    string tag = checkBox.Tag as string;
        //    if (!string.IsNullOrEmpty(tag))
        //    {
        //        if (tag == "1")
        //        {
        //            cbChart.IsChecked = false;
        //            dgMarket.Visibility = Visibility.Visible;
        //            topButton.Visibility = Visibility.Visible;
        //            execChartView.Visibility = Visibility.Collapsed;
        //        }
        //        else if (tag == "2")
        //        {
        //            cbList.IsChecked = false;
        //            dgMarket.Visibility = Visibility.Collapsed;
        //            topButton.Visibility = Visibility.Collapsed;
        //            execChartView.Visibility = Visibility.Visible;
        //        }
        //    }
        //}

        public void SetExecType(int execType)
        {
            _marketOrderViewModel.MarketOrderModel.SelectedIndex = execType;
        }

        private void execChartView_Loaded(object sender, RoutedEventArgs e)
        {
            ResetChartView();
        }

        private void lbPirceType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lbPirceType.SelectedIndex == 1)
            {
                ResetChartView();
            }
        }

        /// <summary>
        /// 图形有时会不显示
        /// </summary>
        private void ResetChartView()
        {
            ExecChart ec = this.execChartView;
            if (ec != null && ec.Parent != null)
            {
                Grid parent = ec.Parent as Grid;
                parent.Children.Remove(ec);
                parent.Children.Add(ec);
                parent.Children.Remove(btnPop);
                parent.Children.Add(btnPop);
            }
        }

        private void btnPop_Click(object sender, RoutedEventArgs e)
        {
            if (this._marketOrderViewModel != null && !string.IsNullOrEmpty(this._marketOrderViewModel.SearchText))
            {
                Module m = new Module();

                m.modulename = m.id = this._marketOrderViewModel.SearchText;

                if (CommonUtils.IsFutureBond(this._marketOrderViewModel.SearchText))
                {
                    string url = string.Empty;
                    if ("PRD".Equals(LocalEnv.ConfInfo.LoginEnv))
                    {
                        url = "https://qb3.idbhost.com:28888/bdb/analyse/curve";
                        //url = STC.Frame.Update.HostHelper.ReplaceIp(url);
                    }
                    else
                    {
                        url = "http://172.16.137.84:1011/#/analyse/curve";
                        //url = STC.Frame.Update.HostHelper.ReplaceIp(url);
                    }

                    m.inf2 = $"{url}/{this._marketOrderViewModel.SearchText}";//"http://192.168.130.202:8091/#/analyse/curve/220220.IB";
                }
                else
                {
                    m.inf2 = $"{ LocalEnv.ConfInfo.KLineUrl}/{this._marketOrderViewModel.SearchText}";//"http://192.168.130.202:8091/#/analyse/curve/220220.IB";
                }

                AppUtils.Application.PluginManager.Application.BroadCast("PopUpWebApp", m);
            }
        }

        //int i = 0;

        //private void xxx_Click(object sender, RoutedEventArgs e)
        //{
        //    i++;
        //    ExecutionReport ex = new ExecutionReport();
        //    ex.ExecID = "abcd" + i;
        //    ex.Side = "1"; // 方向
        //    ex.SecurityID = "230023.IB"; // 代码
        //    ex.Symbol = "230023.IB";// 名称
        //    ex.LastPx = "2.26"; //用于显示成交价
        //    ex.Venues = "ODM"; // 来源
        //    ex.TransactTime =DateTimeEx.GetNow().ToString( "yyyy-MM-dd HH:mm:ss");// 时间
        //    ex.Info1 = "123456";
        //    ex.OrdStatus = "2";
        //    _marketOrderViewModel.Instance_OnMarketExecutions(ex);
        //this.execChartView.UpdateLayout();

        //ResetChartView();
        //Panel.SetZIndex(this.execChartView, 1);
        //}
    }
}