using com.gateway.log;
using Newtonsoft.Json;
using STC.Common.Model.Admin;
using STC.Common.Model.Base;
using STC.Common.Utils;
using STC.Common.View.Card;
using STC.Common.View.UI;
using STC.Common.ViewModel;
using STC.Common.ViewModel.Card;
using STC.XBond.Plugin.View;
using STC.XBond.Plugin.View.Book;
using STC.XBond.Plugin.View.HomePage;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace STC.XBond.Plugin.Command
{
    public enum PopupTypeEnum
    {
        /// <summary>
        /// 主页
        /// </summary>
        HomePage,

        /// <summary>
        /// 历史记录
        /// </summary>
        HistoryRecords,

        /// <summary>
        /// 深度行情
        /// </summary>
        TradeBook,

        /// <summary>
        /// 渠道执行
        /// </summary>
        VenuesExecution,

        /// <summary>
        /// 市场成交
        /// </summary>
        MarketExecution,

        /// <summary>
        /// 我的订单
        /// </summary>
        MyOrders,

        /// <summary>
        /// 我的成交
        /// </summary>
        MyExecutions,

        /// <summary>
        /// 我的已撤订单
        /// </summary>
        MyCancelOrders,

        /// <summary>
        /// 交易包管理
        /// </summary>
        BookManagement,

        /// <summary>
        /// 行情小窗
        /// </summary>
        QuotesMiniWindows,
        /// <summary>
        /// Rfq插件
        /// </summary>
        RfqOrderList,
    }

    /// <summary>
    /// 窗口弹出命令执行器
    /// </summary>
    public class PopupWindowCommand : ICommand
    {
        WindowManager windowManager = WindowManager.Instance;//new WindowManager();

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            try
            {
                if (parameter is object[])
                {
                    object[] args = parameter as object[];
                    PopupTypeEnum popupTypeEnum = (PopupTypeEnum)args[0];
                    if (popupTypeEnum == PopupTypeEnum.TradeBook)
                    {
                        string securtiyID = (string)args[1];
                        if (windowManager.IsExist($"{PopupTypeEnum.TradeBook.ToString()}-{securtiyID}") == false)
                        {
                            System.Windows.Application.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, new Action(() =>
                            {
                                BorderWin myTradingPopupWin = new BorderWin();
                                myTradingPopupWin.SetTitleAndKey($"{PopupTypeEnum.TradeBook.ToString()}-{securtiyID}", securtiyID, "STC.XBond.Plugin");
                                myTradingPopupWin.SetCommandParameter(JsonConvert.SerializeObject(parameter));
                                myTradingPopupWin.Closed += MyTradingPopupWin_Closed;
                                TradeBookView view = new TradeBookView();
                                QuotationListViewModel viewModel = new QuotationListViewModel();
                                if (securtiyID != null)
                                {
                                    viewModel.SecurityID = securtiyID;
                                    BondsInfoPo bondsInfoPo = CommonUtils.GetBondsInfoPoBySecurityID(securtiyID);
                                    if (bondsInfoPo != null)
                                    {
                                        viewModel.Symbol = bondsInfoPo.Short_Name;
                                        viewModel.SecurityID = bondsInfoPo.Bond_ID;
                                        viewModel.Symbol = bondsInfoPo.Short_Name;
                                        viewModel.FullName = bondsInfoPo.Short_Name;
                                        viewModel.MarketIndicator = bondsInfoPo.MarketIndicator;


                                        view.UpdateeLastExectu(viewModel.SecurityID);
                                        view.ChangeVisibility();
                                        view.QList.SetQtyHeaderText(bondsInfoPo.MarketIndicator);
                                        view.LastExecutions.UpdatePriceType(viewModel.PriceTypeIndex);
                                        //view.LastExecutions.SecurtityID = string.IsNullOrEmpty(bondsInfoPo.Bond_ID) ? "xxxxx" : bondsInfoPo.Bond_ID;
                                        //view.LastExecutions.SetSecurtityID(bondsInfoPo.Bond_ID);

                                        if (!string.IsNullOrEmpty(bondsInfoPo.Option_Type) &&
                                            ((!bondsInfoPo.Option_Type.Equals("NON") && bondsInfoPo.Option_Date != null) ||
                                            (bondsInfoPo.Option_Type.Equals("ETS") && bondsInfoPo.Option_Date == null)))
                                        {
                                            viewModel.ShowExeRadioVisible = System.Windows.Visibility.Visible;
                                        }
                                        else
                                        {
                                            viewModel.ShowExeRadioVisible = System.Windows.Visibility.Collapsed;
                                        }
                                    }
                                }

                                view.DataContext = viewModel;
                                view.PopupBtn.Visibility = System.Windows.Visibility.Collapsed;
                                myTradingPopupWin.SetMainControl(view);
                                myTradingPopupWin.Width = 1340;
                                myTradingPopupWin.Height = 500;
                                myTradingPopupWin.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;

                                windowManager.OpenOnlyOneWindow($"STC.XBond.Plugin-{PopupTypeEnum.TradeBook.ToString()}-{securtiyID}", myTradingPopupWin);
                            }));
                        }
                    }
                    else if (popupTypeEnum == PopupTypeEnum.QuotesMiniWindows)
                    {
                        string securtiyID = (string)args[1];
                        OpenQuotesMiniWindows(securtiyID);
                    }
                    else if (popupTypeEnum == PopupTypeEnum.HomePage)
                    {
                        if (args[1].GetType() == typeof(string))
                        {
                            //恢复首页窗口
                            if (windowManager.IsExist($"{PopupTypeEnum.HomePage.ToString()}-{args[1]}") == false)
                            {
                                BorderWin myTradingPopupWin = new BorderWin();
                                myTradingPopupWin.SetTitleAndKey($"{PopupTypeEnum.HomePage.ToString()}-{args[1]}", args[3].ToString(), "STC.XBond.Plugin");
                                myTradingPopupWin.SetCommandParameter($"[{((int)PopupTypeEnum.HomePage).ToString()},\"{args[3]}\",1,\"{args[3].ToString()}\"]");
                                //FavoriteBondsListBox favoriteBondsLB = new FavoriteBondsListBox();
                                //favoriteBondsLB.Init(gi);

                                OneHomePageView oneHomePageView = new OneHomePageView();
                                oneHomePageView.Init(args[1].ToString());
                                myTradingPopupWin.SetMainControl(oneHomePageView);// favoriteBondsLB;
                                myTradingPopupWin.Width = 1340;
                                myTradingPopupWin.Height = 500;
                                myTradingPopupWin.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
                                windowManager.OpenOnlyOneWindow($"STC.XBond.Plugin-{PopupTypeEnum.HomePage.ToString()}-{args[1]}", myTradingPopupWin);
                            }
                        }
                        else
                        {
                            //HomeGroupInfo gi = (HomeGroupInfo)args[1];
                            System.Windows.Controls.Button btn = (System.Windows.Controls.Button)args[1];

                            Grid grid = CommonUtils.GetParentObject<Grid>(btn);
                            HomePageTabControl hptc = CommonUtils.FindVisualChild<HomePageTabControl>(grid);
                            //HomePageTabControl hptc = CommonUtils.GetParentObject<HomePageTabControl>(btn);
                            HomePageTabItemWithClose hptiwc = hptc.SelectedItem as HomePageTabItemWithClose;
                            HomeGroupInfo gi = null;
                            if (hptiwc != null && hptiwc.Tag != null && hptiwc.Tag is HomeGroupInfo)
                            {
                                gi = hptiwc.Tag as HomeGroupInfo;
                                if (windowManager.IsExist($"{PopupTypeEnum.HomePage.ToString()}-{gi.id}") == false)
                                {
                                    BorderWin myTradingPopupWin = new BorderWin();
                                    myTradingPopupWin.SetTitleAndKey($"{PopupTypeEnum.HomePage.ToString()}-{gi.id}", gi.groupname, "STC.XBond.Plugin");
                                    myTradingPopupWin.SetCommandParameter($"[{((int)PopupTypeEnum.HomePage).ToString()},\"{gi.id}\",1,\"{gi.groupname}\"]");
                                    //FavoriteBondsListBox favoriteBondsLB = new FavoriteBondsListBox();
                                    //favoriteBondsLB.Init(gi);

                                    OneHomePageView oneHomePageView = new OneHomePageView();
                                    oneHomePageView.Init(gi);
                                    myTradingPopupWin.SetMainControl(oneHomePageView);// favoriteBondsLB;
                                    myTradingPopupWin.Width = 1340;
                                    myTradingPopupWin.Height = 500;
                                    myTradingPopupWin.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
                                    windowManager.OpenOnlyOneWindow($"STC.XBond.Plugin-{PopupTypeEnum.HomePage.ToString()}-{gi.id}", myTradingPopupWin);
                                }
                            }
                        }
                    }
                }
                else if (parameter is PopupTypeEnum)
                {
                    PopupTypeEnum popupTypeEnum = (PopupTypeEnum)parameter;
                    if (popupTypeEnum == PopupTypeEnum.MarketExecution)
                    {
                        if (windowManager.IsExist(PopupTypeEnum.MarketExecution.ToString()) == false)
                        {
                            BorderWin marketExecutionPopupWin = new BorderWin();
                            marketExecutionPopupWin.SetTitleAndKey(PopupTypeEnum.MarketExecution.ToString(), "市场成交", "STC.XBond.Plugin");
                            marketExecutionPopupWin.SetCommandParameter(JsonConvert.SerializeObject(parameter));

                            MarketExecution marketExecution = new MarketExecution();
                            marketExecution.PopupBtn.Visibility = System.Windows.Visibility.Collapsed;
                            marketExecutionPopupWin.SetMainControl(marketExecution);
                            marketExecutionPopupWin.Width = 400;
                            marketExecutionPopupWin.Height = 300;
                            marketExecutionPopupWin.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
                            windowManager.OpenOnlyOneWindow($"STC.XBond.Plugin-{PopupTypeEnum.MarketExecution.ToString()}", marketExecutionPopupWin);
                        }
                    }
                    else if (popupTypeEnum == PopupTypeEnum.MyOrders)
                    {
                        if (windowManager.IsExist(PopupTypeEnum.MyOrders.ToString()) == false)
                        {
                            BorderWin myTradingPopupWin = new BorderWin();
                            myTradingPopupWin.SetTitleAndKey(PopupTypeEnum.MyOrders.ToString(), "我的交易", "STC.XBond.Plugin");
                            myTradingPopupWin.SetCommandParameter(JsonConvert.SerializeObject(parameter));
                            MyTrading myTrading = new MyTrading();
                            myTrading.btnFullScreen.Visibility = System.Windows.Visibility.Collapsed;
                            myTradingPopupWin.SetMainControl(myTrading);
                            myTradingPopupWin.Width = 1340;
                            myTradingPopupWin.Height = 500;
                            myTradingPopupWin.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
                            windowManager.OpenOnlyOneWindow($"STC.XBond.Plugin-{PopupTypeEnum.MyOrders.ToString()}", myTradingPopupWin);
                        }
                    }
                    else if (popupTypeEnum == PopupTypeEnum.MyExecutions)
                    {
                        if (windowManager.IsExist(PopupTypeEnum.MyExecutions.ToString()) == false)
                        {
                            BorderWin myTradingPopupWin = new BorderWin();
                            myTradingPopupWin.SetTitleAndKey(PopupTypeEnum.MyExecutions.ToString(), "我的交易", "STC.XBond.Plugin");
                            myTradingPopupWin.SetCommandParameter(JsonConvert.SerializeObject(parameter));
                            MyTrading myTrading = new MyTrading();
                            myTrading.MyExecutions_TabItem.IsSelected = true;
                            myTrading.btnFullScreen.Visibility = System.Windows.Visibility.Collapsed;
                            myTradingPopupWin.SetMainControl(myTrading);
                            myTradingPopupWin.Width = 1340;
                            myTradingPopupWin.Height = 500;
                            myTradingPopupWin.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
                            windowManager.OpenOnlyOneWindow($"STC.XBond.Plugin-{PopupTypeEnum.MyExecutions.ToString()}", myTradingPopupWin);
                        }
                    }
                    else if (popupTypeEnum == PopupTypeEnum.MyCancelOrders)
                    {
                        if (windowManager.IsExist(PopupTypeEnum.MyCancelOrders.ToString()) == false)
                        {
                            BorderWin myTradingPopupWin = new BorderWin();
                            myTradingPopupWin.SetTitleAndKey(PopupTypeEnum.MyCancelOrders.ToString(), "我的交易", "STC.XBond.Plugin");
                            myTradingPopupWin.SetCommandParameter(JsonConvert.SerializeObject(parameter));
                            MyTrading myTrading = new MyTrading();
                            myTrading.MyCancels_TabItem.IsSelected = true;
                            myTrading.btnFullScreen.Visibility = System.Windows.Visibility.Collapsed;
                            myTradingPopupWin.SetMainControl(myTrading);
                            myTradingPopupWin.Width = 1340;
                            myTradingPopupWin.Height = 500;
                            myTradingPopupWin.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
                            windowManager.OpenOnlyOneWindow($"STC.XBond.Plugin-{PopupTypeEnum.MyCancelOrders.ToString()}", myTradingPopupWin);
                        }
                    }
                    else if (popupTypeEnum == PopupTypeEnum.RfqOrderList)
                    {
                        BorderWin myTradingPopupWin = new BorderWin();
                        myTradingPopupWin.SetTitleAndKey(PopupTypeEnum.RfqOrderList.ToString(), "我的交易", "STC.XBond.Plugin");
                        myTradingPopupWin.SetCommandParameter(JsonConvert.SerializeObject(parameter));
                        MyTrading myTrading = new MyTrading();
                        myTrading.MyRfq_TabItem.IsSelected = true;
                        myTrading.btnFullScreen.Visibility = System.Windows.Visibility.Collapsed;
                        myTradingPopupWin.SetMainControl(myTrading);
                        myTradingPopupWin.Width = 1340;
                        myTradingPopupWin.Height = 500;
                        myTradingPopupWin.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
                        windowManager.OpenOnlyOneWindow($"STC.XBond.Plugin-{PopupTypeEnum.RfqOrderList.ToString()}", myTradingPopupWin);
                    }

                    else if (popupTypeEnum == PopupTypeEnum.HistoryRecords)
                    {
                        if (windowManager.IsExist(PopupTypeEnum.HistoryRecords.ToString()) == false)
                        {
                            BorderWin popupWin = new BorderWin();
                            popupWin.SetTitleAndKey(PopupTypeEnum.HistoryRecords.ToString(), "历史数据", "STC.XBond.Plugin");
                            popupWin.SetCommandParameter(JsonConvert.SerializeObject(parameter));
                            HistoryView historyView = new HistoryView();
                            historyView.PopupBtn.Visibility = System.Windows.Visibility.Collapsed;
                            popupWin.SetMainControl(historyView);
                            popupWin.Width = 1340;
                            popupWin.Height = 500;
                            popupWin.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
                            windowManager.OpenOnlyOneWindow($"STC.XBond.Plugin-{PopupTypeEnum.HistoryRecords.ToString()}", popupWin);
                        }
                    }
                    else if (popupTypeEnum == PopupTypeEnum.VenuesExecution)
                    {
                        if (windowManager.IsExist(PopupTypeEnum.VenuesExecution.ToString()) == false)
                        {
                            BorderWin popupWin = new BorderWin();
                            popupWin.SetTitleAndKey(PopupTypeEnum.VenuesExecution.ToString(), "渠道执行", "STC.XBond.Plugin");
                            popupWin.SetCommandParameter(JsonConvert.SerializeObject(parameter));
                            VenuesExecution venuesExecution = new VenuesExecution();
                            venuesExecution.btnFullScreen.Visibility = System.Windows.Visibility.Collapsed;
                            popupWin.SetMainControl(venuesExecution);
                            popupWin.Width = 1340;
                            popupWin.Height = 500;
                            popupWin.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
                            windowManager.OpenOnlyOneWindow($"STC.XBond.Plugin-{PopupTypeEnum.VenuesExecution.ToString()}", popupWin);
                        }
                    }
                    else if (popupTypeEnum == PopupTypeEnum.BookManagement)
                    {
                        if (windowManager.IsExist(PopupTypeEnum.BookManagement.ToString()) == false)
                        {
                            BorderWin bookManagementWin = new BorderWin();
                            bookManagementWin.SetTitleAndKey(PopupTypeEnum.BookManagement.ToString(), "交易包管理", "STC.XBond.Plugin");
                            bookManagementWin.SetCommandParameter(JsonConvert.SerializeObject(parameter));
                            BookManagementView bookManagementView = new BookManagementView();
                            bookManagementView.PopupBtn.Visibility = System.Windows.Visibility.Collapsed;
                            bookManagementWin.SetMainControl(bookManagementView);
                            bookManagementWin.Width = 1340;
                            bookManagementWin.Height = 500;
                            bookManagementWin.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
                            windowManager.OpenOnlyOneWindow($"STC.XBond.Plugin-{PopupTypeEnum.BookManagement.ToString()}", bookManagementWin);
                        }
                    }
                }
            }
            catch(Exception e)
            {
                LogHelper.Error($"PopupWindowCommand {parameter}",e);
            }
        }


        private async Task OpenQuotesMiniWindows(string securtiyID)
        {
            await Task.Run(() =>
            {
                if (windowManager.IsExist($"{PopupTypeEnum.TradeBook.ToString()}-{securtiyID}") == false)
                {
                    PopupNoBorder myTradingPopupWin = new PopupNoBorder();
                    myTradingPopupWin.Closed += MyTradingPopupWin_Closed;
                    myTradingPopupWin.Title = securtiyID;

                    PriceAndOrderView tradeBookView = new PriceAndOrderView();
                    PriceAndOrderViewModel viewModel = new PriceAndOrderViewModel();
                    if (securtiyID != null)
                    {
                        viewModel.SecurityID = securtiyID;
                        BondsInfoPo bondsInfoPo = CommonUtils.GetBondsInfoPoBySecurityID(securtiyID);
                        if (bondsInfoPo != null)
                        {
                            viewModel.Symbol = bondsInfoPo.Short_Name;
                        }
                    }

                    tradeBookView.DataContext = viewModel;
                    myTradingPopupWin.SetMainControl(tradeBookView);
                    myTradingPopupWin.Width = 290;
                    myTradingPopupWin.Height = 460;
                    windowManager.OpenOnlyOneWindow($"STC.XBond.Plugin-{PopupTypeEnum.QuotesMiniWindows.ToString()}-{securtiyID}", myTradingPopupWin);

                }
            });
        }

        /// <summary>
        /// '我的交易'弹出窗口关闭事件
        /// </summary>
        /// <param name="sender">'我的交易'弹出窗口实例</param>
        /// <param name="e">表示包含事件数据的类的基类，并提供要用于不包含事件数据的事件的值。</param>
        private void MyTradingPopupWin_Closed(object sender, EventArgs e)
        {
            BorderWin popupWindow = sender as BorderWin;

            if (popupWindow.GetMainControl() != null 
                && popupWindow.GetMainControl() is TradeBookView)
            {
                TradeBookView tradeBookView = popupWindow.GetMainControl() as TradeBookView;
                if (tradeBookView != null && tradeBookView.DataContext != null
                    && tradeBookView.DataContext is QuotationListViewModel)
                {
                    QuotationListViewModel quotationListViewModel = tradeBookView.DataContext as QuotationListViewModel;
                    quotationListViewModel.Dispose();
                    //quotationListViewModel = null;
                }
            }
           
        }
    }
}
