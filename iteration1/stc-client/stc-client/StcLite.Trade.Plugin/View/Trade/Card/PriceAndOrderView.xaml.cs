using com.gateway.log;
using System;
using System.Collections;
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
using System.Windows.Threading;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;
using BDB.Price.Plugin.Model;
using BDB.Price.Plugin.Cache;
using BDB.Price.Plugin.View.Trade.Card;
using BDB.Price.Plugin.Handler;
using STC.Common.Utils;
using STC.Common.Model.Fix.Message;

using STC.Common.Model.Admin;
using BDB.Trade.Plugin.ViewModel.Trade.Card;
using BDB.Trade.Plugin.ViewModel.Data;
using BDB.Trade.Plugin.Handler;
using BDB.Trade.Plugin.Cache;
using HomePageDataHandler = BDB.Trade.Plugin.Handler.HomePageDataHandler;
using BDB.Trade.Plugin.Component;
using BDB.Trade.Plugin.Handler.DataProcesser;
using STC.Common.Handler.Base;
using STC.Common.Model.Base;
using BDB.Trade.Plugin.ViewModel.Adapter;
using BDB.Trade.Plugin.View.Order;
using BDB.Trade.Plugin.ViewModel.Trade;
using STC.Common.Data;
using BDB.Common.Model.OHLC;
using STC.Common.Model.QQ;
using STC.Common.Handler.QQ;
using STC.Common.Interface;
using System.Collections.Concurrent;

namespace BDB.Trade.Plugin.View.Trade.Card
{
    /// <summary>
    /// PriceAndOrderView.xaml 的交互逻辑
    /// </summary>
    public partial class PriceAndOrderView : UserControl, IPriceAndOrderViewInterface
    {


        public CardShowType CardMode
        {
            get { return (CardShowType)GetValue(CardModeProperty); }
            set { SetValue(CardModeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CardMode.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CardModeProperty =
            DependencyProperty.Register("CardMode", typeof(CardShowType), typeof(PriceAndOrderView), new PropertyMetadata(CardShowType.InCardPage));



        public PriceAndOrderView()
        {
            InitializeComponent();

            //brokerFilter.SetSelectModel(SelectionMode.Single);

            //settleFilter.SetButtonProperty(23, 20);
            //brokerFilter.SetButtonProperty(44, 24);

            //settleFilter.LayoutTransform = new ScaleTransform(1.2, 1.2);

            this.Unloaded += PriceAndOrderView_Unloaded;

            this.Loaded += PriceAndOrderView_Loaded;

            oneBroker.OnClickBroker += OneBroker_OnClickBroker;
            twoBroker.OnClickBroker += OneBroker_OnClickBroker;
            threeBroker.OnClickBroker += OneBroker_OnClickBroker;
            fourBroker.OnClickBroker += OneBroker_OnClickBroker;
            fiveBroker.OnClickBroker += OneBroker_OnClickBroker;
            sixBroker.OnClickBroker += OneBroker_OnClickBroker;
        }

        public PriceAndOrderView(string security = "")
        {
            InitializeComponent();

            //brokerFilter.SetSelectModel(SelectionMode.Single);

            //settleFilter.SetButtonProperty(23, 20);
            //brokerFilter.SetButtonProperty(44, 24);

            //settleFilter.LayoutTransform = new ScaleTransform(1.2, 1.2);

            this.Unloaded += PriceAndOrderView_Unloaded;

            this.Loaded += PriceAndOrderView_Loaded;

            oneBroker.OnClickBroker += OneBroker_OnClickBroker;
            twoBroker.OnClickBroker += OneBroker_OnClickBroker;
            threeBroker.OnClickBroker += OneBroker_OnClickBroker;
            fourBroker.OnClickBroker += OneBroker_OnClickBroker;
            fiveBroker.OnClickBroker += OneBroker_OnClickBroker;
            sixBroker.OnClickBroker += OneBroker_OnClickBroker;

            if (security.ToLower() == "tradebook")
            {
                btn_AddBond.Visibility = Visibility.Collapsed;
            }
        }

        private void OneBroker_OnClickBroker(string txt)
        {
            brokerFilter.SetBrokerShow(txt);
        }

        private void PriceAndOrderView_Loaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= PriceAndOrderView_Loaded;

            PriceAndOrderViewModel priceAndOrderViewModel = this.DataContext as PriceAndOrderViewModel;
            if (priceAndOrderViewModel != null && priceAndOrderViewModel.ShowModel == TabShowModel.Card && priceAndOrderViewModel.CardShowType == Price.Plugin.Model.CardShowType.InCardPage)
            {
                string key = PageCardHandler.GetWindowHashKey(this);

                CommonHandler.Instance.AddLoadNum(key);
            }

        }

        private void PriceAndOrderView_Unloaded(object sender, RoutedEventArgs e)
        {
            this.Unloaded -= PriceAndOrderView_Unloaded;

            PriceAndOrderViewModel priceAndOrderViewModel = this.DataContext as PriceAndOrderViewModel;
            if (priceAndOrderViewModel != null)
            {
                priceAndOrderViewModel.Dispose();
            }

            if (_ProcessCtPriceThreadQueue != null)
            {
                _ProcessCtPriceThreadQueue.Dispose();
            }

            if (_ProcessLastExecutionsThreadQueue != null)
            {
                _ProcessLastExecutionsThreadQueue.Dispose();
            }

            if (_ProcessOHLCThreadQueue != null)
            {
                _ProcessOHLCThreadQueue.Dispose();
            }

        }

        

        ContextMenu contextMenu;

        private void SetContextMenu()
        {
            bool result = PermisionHandler.Instance.GetIdcTraderPermission();
            if (contextMenu == null && result)
            {
                contextMenu = (ContextMenu)this.FindResource("OrderContextMenu");

            }
        }

        private bool _isBid = false;
        private void BdBid_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            SetContextMenu();

            _isBid = true;
            if (contextMenu != null)
            {
                contextMenu.IsOpen = true;
            }
        }

        private void BdOfr_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            SetContextMenu();

            _isBid = false;
            if (contextMenu != null)
            {
                contextMenu.IsOpen = true;
            }
        }

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void SeSearch_SearchChanged(object sender, object e)
        {
            SeBondPo seBondPo = e as SeBondPo;

            PriceAndOrderViewModel priceAndOrderViewModel = this.DataContext as PriceAndOrderViewModel;

            if (priceAndOrderViewModel != null && seBondPo != null && !string.IsNullOrEmpty(seBondPo.BondCode) && !seBondPo.BondCode.Equals(priceAndOrderViewModel.PriceModel.Security))
            //&& !string.IsNullOrEmpty(seBondPo.ShortName))
            {
                if (priceAndOrderViewModel.CardShowType == Price.Plugin.Model.CardShowType.InCardPage)
                {
                    HomeGroupSecurityidsUpdateReq request = new HomeGroupSecurityidsUpdateReq();
                    request.securityid = seBondPo.BondCode;
                    request.symbol = seBondPo.ShortName;
                    request.action = "1"; // 添加
                    request.remark = $"{request.securityid}  {request.symbol}  {seBondPo.LeftTenor}";
                    request.terminal = string.Format("STCLite_v{0}", LocalEnv.ServerInfo.Version);
                    request.remainingTime = seBondPo.LeftTenor;

                    request.groupid = priceAndOrderViewModel.GroupId;

                    LayoutSetttingCache.Instance.CurrentUpdatePanle = null;
                    if (!string.IsNullOrEmpty(priceAndOrderViewModel.GroupId) && !string.IsNullOrEmpty(priceAndOrderViewModel.Id))
                    {
                        HomePageDataHandler.Instance.DelAndAddBond(priceAndOrderViewModel.GroupId, priceAndOrderViewModel.Id, priceAndOrderViewModel.PriceModel.Security,
                            request, OnAddCallback);
                    }
                    else
                    {
                        HomePageDataHandler.Instance.AddBond(request, OnAddCallback);
                    }
                }
                else
                {
                    ResetCardWind(priceAndOrderViewModel, seBondPo);
                }
            }
            else
            {
                if (priceAndOrderViewModel != null)
                {
                    if (priceAndOrderViewModel.CardShowType == Price.Plugin.Model.CardShowType.InCardPage)
                    {
                        if (string.IsNullOrEmpty(priceAndOrderViewModel.Id))
                        {
                            priceAndOrderViewModel.PriceModel.AddBondVisibility = Visibility.Visible;
                            priceAndOrderViewModel.PriceModel.BondGridVisibility = Visibility.Collapsed;
                            priceAndOrderViewModel.PriceModel.SearchVisibility = Visibility.Collapsed;
                        }
                        else
                        {
                            priceAndOrderViewModel.PriceModel.AddBondVisibility = Visibility.Collapsed;
                            priceAndOrderViewModel.PriceModel.BondGridVisibility = Visibility.Visible;
                            priceAndOrderViewModel.PriceModel.SearchVisibility = Visibility.Collapsed;
                        }
                    }
                    else if (priceAndOrderViewModel.CardShowType == Price.Plugin.Model.CardShowType.AloneWindow)
                    {
                        priceAndOrderViewModel.PriceModel.AddBondVisibility = Visibility.Collapsed;
                        priceAndOrderViewModel.PriceModel.BondGridVisibility = Visibility.Visible;
                        priceAndOrderViewModel.PriceModel.SearchVisibility = Visibility.Collapsed;

                        priceAndOrderViewModel.PriceModel.EditVisibility = Visibility.Visible;
                    }


                    if (priceAndOrderViewModel != null && seBondPo != null && !string.IsNullOrEmpty(seBondPo.BondCode) && priceAndOrderViewModel.CardShowType == Price.Plugin.Model.CardShowType.AloneWindow)
                    {
                        ResetCardWind(priceAndOrderViewModel, seBondPo);
                    }
                }
            }
        }

        private void ResetCardWind(PriceAndOrderViewModel priceAndOrderViewModel, SeBondPo seBondPo)
        {
            CardWinCache.Instance.DeleteBySecurity(priceAndOrderViewModel.PriceModel.Security);


            CardWindow window = CommonUtils.GetParentObject<CardWindow>(this) as CardWindow;
            if (window != null)
            {
                window.Reset(seBondPo.BondCode);
            }


            priceAndOrderViewModel.MsgSettingItem = MsgSettingHandler.Instance.GetMsgSettingItemBySecurtiyID(seBondPo.BondCode);

        }

        private void seSearch_AfterDeleted(object sender, object e)
        {
        }


        public void OnAddCallback(PResult result, string securityId, string id)
        {
            if (result != null && result.IsSuccess())
            {
                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    PriceAndOrderViewModel priceAndOrderViewModel = this.DataContext as PriceAndOrderViewModel;
                    priceAndOrderViewModel.PriceModel.Security = securityId;

                    Model.QcPanelPoEx qcPanelPoEx = new Model.QcPanelPoEx();
                    qcPanelPoEx.BondCode = securityId;
                    qcPanelPoEx.GroupId = priceAndOrderViewModel.GroupId;
                    qcPanelPoEx.Row = priceAndOrderViewModel.RowIndex;
                    qcPanelPoEx.Col = priceAndOrderViewModel.ColIndex;
                    qcPanelPoEx.Id = priceAndOrderViewModel.Id;
                    qcPanelPoEx.PageIndex = priceAndOrderViewModel.PageIndex;

                    LayoutSetttingCache.Instance.CurrentUpdatePanle = qcPanelPoEx;

                }));
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            PriceAndOrderViewModel priceAndOrderViewModel = this.DataContext as PriceAndOrderViewModel;
            priceAndOrderViewModel.OnAddClick();

            seSearch.SetFocus();
        }

        private void OrderMenuItme_Click(object sender, RoutedEventArgs e)
        {
            PriceMatchParam priceMatchParam = new PriceMatchParam();
            PriceAndOrderViewModel priceAndOrderViewModel = this.DataContext as PriceAndOrderViewModel;
            if (_isBid)
            {
                priceMatchParam.Side = "BUY";
                priceMatchParam.Price = priceAndOrderViewModel.PriceModel.BestBid;
                priceMatchParam.UpdataPrice = false;
                priceMatchParam.CustomFirmAmount = priceAndOrderViewModel.PriceModel.BestBidAmount;
            }
            else
            {
                priceMatchParam.Side = "SELL";
                priceMatchParam.Price = priceAndOrderViewModel.PriceModel.BestOfr;
                priceMatchParam.UpdataPrice = false;
                priceMatchParam.CustomFirmAmount = priceAndOrderViewModel.PriceModel.BestOfrAmount;
            }

            priceMatchParam.IsFirm = true;
            priceMatchParam.Symbol = priceAndOrderViewModel.PriceModel.Security;
            priceMatchParam.NonfirmTime = string.Empty;
            priceMatchParam.OperationType = "1";
            //设置默认结算方式
            List<string> settleList = settleFilter.GetSelectContent();
            priceMatchParam.SettlType = (settleList != null && settleList.Count > 0) ? settleList[0] : CommonConstants.ALL_PARTY;
            //priceMatchParam.NetPrice = GetNetPrice(drv);
            SendPriceViewLite view = new SendPriceViewLite(priceMatchParam);

            view.Show();
        }

        int i = 0;
        bool isDouble = false;
        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            i += 1;
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(0, 0, 0, 0, System.Windows.Forms.SystemInformation.DoubleClickTime);
            timer.Tick += (s, e1) => { timer.IsEnabled = false; i = 0; UIElement_Click(sender, e); isDouble = false; };
            timer.IsEnabled = true;
            if (i % 2 == 0)
            {
                timer.IsEnabled = false;
                i = 0;
                isDouble = true;

                //双击时执行的代码
                //MessageBox.Show("DoubleClick!");

                PriceAndOrderViewModel priceAndOrderViewModel = this.DataContext as PriceAndOrderViewModel;
                if (priceAndOrderViewModel != null && string.IsNullOrEmpty(priceAndOrderViewModel.PriceModel.Security) == false)
                {
                    CommonUtils.OpenSmallBondWin(priceAndOrderViewModel.PriceModel.Security);
                }
            }
        }

        private void UIElement_Click(object sender, MouseButtonEventArgs e)
        {
            if (isDouble)
                return;
            PriceAndOrderViewModel priceAndOrderViewModel = this.DataContext as PriceAndOrderViewModel;

            if (priceAndOrderViewModel != null && priceAndOrderViewModel.ShowModel == TabShowModel.Card && priceAndOrderViewModel.CardShowType == Price.Plugin.Model.CardShowType.InCardPage)
            {
                priceAndOrderViewModel.OnAddClick();

                
                seSearch.SearchBox.Text = priceAndOrderViewModel.PriceModel.Security;
               
                seSearch.SetFocus();
            }
        }

        private Color _greenColor = (Color)System.Windows.Media.ColorConverter.ConvertFromString("#193D37");

        private void Grid_MouseEnter(object sender, MouseEventArgs e)
        {
            //Grid grid = sender as Grid;
            //grid.Background = new SolidColorBrush(_greenColor);
            PriceAndOrderViewModel priceAndOrderViewModel = this.DataContext as PriceAndOrderViewModel;

            if (priceAndOrderViewModel != null && priceAndOrderViewModel.ShowModel == TabShowModel.Card && priceAndOrderViewModel.CardShowType == Price.Plugin.Model.CardShowType.InCardPage)
            {
                Cursor = Cursors.Hand;
            }
        }

        private void Grid_MouseLeave(object sender, MouseEventArgs e)
        {
            //Grid grid = sender as Grid;
            //grid.Background = new SolidColorBrush(Colors.Transparent);

            Cursor = Cursors.Arrow;
        }


        #region table展示适配逻辑
        private AsyncThreadQueue<MarketDataSnapshotFullRefresh> _ProcessCtPriceThreadQueue;
        private AsyncThreadQueue<ExecutionReport> _ProcessLastExecutionsThreadQueue;
        private AsyncThreadQueue<OhlcMsg> _ProcessOHLCThreadQueue;

        public void Init(TabShowModel tabShowModel, string security, string groupId)
        {
            if (tabShowModel == TabShowModel.Card)
            {
                btnDelete.Visibility = Visibility.Visible;
            }
            else
            {
                InitTableModel(security, groupId);
            }
        }


        public void InitTableModel(string security, string groupId)
        {
            //brokerFilter.LayoutTransform = new ScaleTransform(1.2, 1.3);

            if (_ProcessCtPriceThreadQueue == null)
            {
                _ProcessCtPriceThreadQueue = new AsyncThreadQueue<MarketDataSnapshotFullRefresh>(UpdateMDData);
            }
            if (_ProcessLastExecutionsThreadQueue == null)
            {
                _ProcessLastExecutionsThreadQueue = new AsyncThreadQueue<ExecutionReport>(UpdateLastExecutionsAsync);
            }
            if (_ProcessOHLCThreadQueue == null)
            {
                _ProcessOHLCThreadQueue = new AsyncThreadQueue<OhlcMsg>(UpdateOHLC);
            }

            PriceAndOrderViewModel priceAndOrderViewModel = new PriceAndOrderViewModel();
            priceAndOrderViewModel.CardShowType = this.CardMode;
            this.DataContext = priceAndOrderViewModel;
            priceAndOrderViewModel.InitByPageChange(this, string.Empty, string.Empty, groupId);

            bdFirst.BorderThickness = new Thickness(0);
            btnDelete.Visibility = Visibility.Collapsed;

            priceAndOrderViewModel.ShowModel = TabShowModel.Table;

            BondChangeHandler.Instance.OnSelectBondChange += Instance_OnSelectBondChange;

            BondChangeHandler.Instance.OnSelectSettleChange += Instance_OnMainSelectSettleChange;

            if (security.ToLower() != "tradebook")
                PriceHandler.Instance.RegisterCTPrice(security, Instance_OnMDPrice);

            priceAndOrderViewModel.MsgSettingItem = MsgSettingHandler.Instance.GetMsgSettingItemBySecurtiyID(security);


            LastExecutionProcesser.Processer.OnLastExecutions += Instance_OnLastExecutions;

            BaseHandler.Instance.Init(Instance_OnClear);
        }

        private object _lockObj = new object();
        private void Instance_OnClear(ClearPo obj)
        {
            LogHelper.Info(string.Format("PriceAndOrderView Clear"));
            if (obj != null && "Y".Equals(obj.IsClear))
            {
                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    lock (this._lockObj)
                    {
                        PriceAndOrderViewModel priceAndOrderViewModel = this.DataContext as PriceAndOrderViewModel;
                        if (priceAndOrderViewModel != null)
                        {
                            priceAndOrderViewModel.OnClear();
                        }

                    }
                }));

            }
        }

        #region OHLC

        public void onOHLC(OhlcMsg ohlcMsg)
        {
            if (_ProcessOHLCThreadQueue != null)
                _ProcessOHLCThreadQueue.Enqueue(ohlcMsg);
        }

        private void UpdateOHLC(OhlcMsg data)
        {
            // CommonUtils._ChangeTab_ManualResetEvent.WaitOne();
            if (_priceAndOrderViewModel != null && _priceAndOrderViewModel.PriceModel != null && _priceAndOrderViewModel.PriceModel.Security == data.securityid)
            {
                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    _priceAndOrderViewModel.onOHLC(data);
                }));
            }
        }

        #endregion

        #region 最新成交

        private void Instance_OnLastExecutions(ExecutionReport obj)
        {
            if (obj != null && _ProcessLastExecutionsThreadQueue != null)
            {
                _ProcessLastExecutionsThreadQueue.Enqueue(obj);
            }
        }

        private void UpdateLastExecutionsAsync(ExecutionReport obj)
        {
            //CommonUtils._ChangeTab_ManualResetEvent.WaitOne();
            if (_priceAndOrderViewModel != null && _priceAndOrderViewModel.PriceModel != null && _priceAndOrderViewModel.PriceModel.Security == obj.SecurityID)
            {
                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    _priceAndOrderViewModel.OnLastExec(obj);
                }));
            }
        }

        #endregion


        private void Instance_OnMDPrice(MarketDataSnapshotFullRefresh obj)
        {
            if (_ProcessCtPriceThreadQueue != null)
                _ProcessCtPriceThreadQueue.Enqueue(obj);

        }

        private void UpdateMDData(MarketDataSnapshotFullRefresh data)
        {
            //CommonUtils._ChangeTab_ManualResetEvent.WaitOne();
            if (_priceAndOrderViewModel != null && _priceAndOrderViewModel.PriceModel != null && _priceAndOrderViewModel.PriceModel.Security == data.SecurityID)
            {
                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    _priceAndOrderViewModel.OnPrice(data, true);
                }));
            }
        }


        ConcurrentStack<string[]> bondChangedStack = new ConcurrentStack<string[]>();
        volatile bool isSelectBondChanging = false;
        volatile string lastBondUpdateTime = string.Empty;

        private void Sub_OnSelectBondChange(string groupId, string id, string security, string settle)
        {
            PriceAndOrderViewModel priceAndOrderViewModel = this.DataContext as PriceAndOrderViewModel;

            if (priceAndOrderViewModel != null)
            {
                PriceHandler.Instance.UnregisterCTPrice(priceAndOrderViewModel.PriceModel.Security, Instance_OnMDPrice);
                OhlcHandler.Instance.UnSubscribeOhlc(priceAndOrderViewModel.PriceModel.Security, onOHLC);

                priceAndOrderViewModel.MsgSettingItem = null;

                //先清理数据
                priceAndOrderViewModel.OnDelete();

                priceAndOrderViewModel.PriceModel.Security = security;
                int sort = SortCache.Instance.GetSettleById(groupId, security);
                //if (sort >= 0)
                //    priceAndOrderViewModel.CustomSort = sort;
                //else
                //    priceAndOrderViewModel.CustomSort = securityIdList.Count;

                //priceAndOrderViewModel.Init(priceAndOrderView, securityIDsInfo.securityid, securityIDsInfo.id, groupId);
                priceAndOrderViewModel.InitByPageChange(this, security, id, groupId);

                priceAndOrderViewModel.InitData();

                priceAndOrderViewModel.InitMarketDataByCardWind();

                CalBaseData(priceAndOrderViewModel);

                PriceHandler.Instance.RegisterCTPrice(security, Instance_OnMDPrice);
                OhlcHandler.Instance.SubscribeOhlc(priceAndOrderViewModel.PriceModel.Security, onOHLC);

                priceAndOrderViewModel.MsgSettingItem = MsgSettingHandler.Instance.GetMsgSettingItemBySecurtiyID(security);

                //if (actionFrom == BondUpdateType.ChangeFocusRow || actionFrom == BondUpdateType.ChangeRowSettle || actionFrom == BondUpdateType.ChangeRowSettleSignal)
                {
                    List<string> settleList = new List<string>();
                    settleList.Add(settle);

                    ChangeOweSettle(settleList);
                }
            }
        }

        private void Instance_OnSelectBondChange(BondUpdateType actionFrom, string groupId, string id, string security, string settle, bool needRefresh)
        {
            bondChangedStack.Push(new string[] { groupId, id, security, settle, DateTimeEx.ServerTimeNow.ToString("yyyyMMdd HH:mm:ss.fff") });

            if (isSelectBondChanging == false)
            {
                isSelectBondChanging = true;
                ThreadPoolHelper.InvokeThreadPool(o =>
                {
                    try
                    {
                        while (bondChangedStack.Count > 0)
                        {
                            Console.WriteLine($"Count:{bondChangedStack.Count}");
                            string[] result;
                            bondChangedStack.TryPop(out result);
                            if (result != null)
                            {
                                if (result[4].CompareTo(lastBondUpdateTime) > 0)
                                {
                                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                                    {
                                        string groupId_param = result[0];
                                        string id_param = result[1];
                                        string security_param = result[2];
                                        string settle_param = result[3];
                                        lastBondUpdateTime = result[4];

                                        Sub_OnSelectBondChange(groupId_param, id_param, security_param, settle_param);
                                    }));
                                }
                                else
                                {
                                    Console.WriteLine($"省略:{result[4]}");
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        LogHelper.Error(e);
                    }
                    finally
                    {
                        isSelectBondChanging = false;
                    }

                });
            }
        }

        public void ChangeOweSettle(List<string> settleList)
        {
            List<ListBoxItem> itemList = new List<ListBoxItem>();

            if (settleList != null)
            {
                foreach (string settle in settleList)
                {
                    ListBoxItem listBoxItem = new ListBoxItem();
                    listBoxItem.Content = settle;
                    itemList.Add(listBoxItem);
                }
            }
            else
            {
                ListBoxItem allItem = new ListBoxItem();
                allItem.Content = CommonConstants.ALL_PARTY;

                itemList.Add(allItem);
            }

            Dictionary<string, IList> filterDic = new Dictionary<string, IList>();
            filterDic[CommonConstants.SETTLE] = itemList;

            SettleFilterView settleFilter = this.FindName("settleFilter") as SettleFilterView;
            settleFilter.SetItemSelectedStatus(filterDic);
        }


        private void CalBaseData(PriceAndOrderViewModel priceAndOrderViewModel)
        {
            if (priceAndOrderViewModel != null && !string.IsNullOrEmpty(priceAndOrderViewModel.PriceModel.Security))
            {
                BondsInfoPo bondsInfoPo = CommonUtils.GetBondsInfoPoBySecurityID(priceAndOrderViewModel.PriceModel.Security);
                if (bondsInfoPo != null)
                {
                    priceAndOrderViewModel.RemaingingTimeConvert = CommonUtils.ConvertRemainingTime(bondsInfoPo.RemaingingTime);//剩余时间转换成时间段
                    //cardItem.BondType = CommonUtils.GetBondCategoryName(bondsInfoPo.Bond_Subtype, bondsInfoPo.Ent_Cor);// 债券类型
                    priceAndOrderViewModel.BondCategory = CommonUtils.GetBondCategory(bondsInfoPo.Bond_Subtype, bondsInfoPo.Ent_Cor);
                }
            }
        }


        #region 全局结算/Broker方式改变

        public void Instance_OnMainSelectSettleChange(OperationType operationType, string groupId, object obj)
        {
            if (operationType == OperationType.Card || operationType == OperationType.RemerberToCard)
            {
                PriceAndOrderViewModel priceAndOrderViewModel = this.DataContext as PriceAndOrderViewModel;
                if (priceAndOrderViewModel != null && priceAndOrderViewModel.GroupId == groupId)
                {
                    if (operationType == OperationType.Card)
                    {
                        priceAndOrderViewModel.SetSettle(operationType, obj);
                        priceAndOrderViewModel.SetBroker(operationType, obj);
                        priceAndOrderViewModel.SetExecType(operationType, obj);
                    }
                    else
                    {
                        string cacheSettle = SettleCache.Instance.GetSettleById(groupId, priceAndOrderViewModel.Id);
                        if (!string.IsNullOrEmpty(cacheSettle))
                        {
                            List<string> list = new List<string>();
                            list.Add(cacheSettle);
                            priceAndOrderViewModel.SetSettle(operationType, ConditionHelper.SettleListToDic(list));

                            //priceAndOrderViewModel.SetBroker(operationType, obj);
                        }
                    }
                }
            }
        }

        #endregion

        #endregion


        private void bdTitle_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            PriceAndOrderViewModel priceAndOrderViewModel = this.DataContext as PriceAndOrderViewModel;
            if (priceAndOrderViewModel != null && priceAndOrderViewModel.CardShowType == Price.Plugin.Model.CardShowType.AloneWindow)
            {
                Window window = CommonUtils.GetParentObject<Window>(this) as Window;
                if (window != null)
                {
                    try
                    {
                        window.DragMove();
                    }
                    catch { }
                }
            }
        }

        int i_bid = 0;
        bool isDouble_bid = false;
        bool isCtrlBid = false;
        private void bdBid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            isCtrlBid = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);
            i_bid += 1;
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(0, 0, 0, 0, System.Windows.Forms.SystemInformation.DoubleClickTime);
            timer.Tick += (s, e1) => { timer.IsEnabled = false; i_bid = 0; UIElement_Bid_Click(sender, e); isDouble_bid = false; isCtrlBid = false; };
            timer.IsEnabled = true;
            if (i_bid % 2 == 0)
            {
                timer.IsEnabled = false;
                i_bid = 0;
                isDouble_bid = true;

                //双击时执行的代码
                //MessageBox.Show("DoubleClick!");

                CopyPointOrderData("1");
            }
        }

        private void UIElement_Bid_Click(object sender, MouseButtonEventArgs e)
        {
            if (isDouble_bid)
                return;

            if (isCtrlBid)
            {
                Console.WriteLine("1");
                CopyOrderData("1");
            }
            else
            {
                Console.WriteLine("2");
                CopyOrderData();
            }
        }

        int i_ofr = 0;
        bool isDouble_ofr = false;
        bool isCtrlOfr = false;
        private void bdOfr_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            isCtrlOfr = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);
            i_ofr += 1;
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(0, 0, 0, 0, System.Windows.Forms.SystemInformation.DoubleClickTime);
            timer.Tick += (s, e1) => { timer.IsEnabled = false; i_ofr = 0; UIElement_Ofr_Click(sender, e); isDouble_ofr = false; isCtrlOfr = false; };
            timer.IsEnabled = true;
            if (i_ofr % 2 == 0)
            {
                timer.IsEnabled = false;
                i_ofr = 0;
                isDouble_ofr = true;

                //双击时执行的代码
                //MessageBox.Show("DoubleClick!");

                CopyPointOrderData("2");
            }
        }

        private void UIElement_Ofr_Click(object sender, MouseButtonEventArgs e)
        {
            if (isDouble_ofr)
                return;

            if (isCtrlOfr)
            {
                CopyOrderData("2");
            }
            else
            {
                CopyOrderData();
            }
        }

        private async void CopyOrderData(string side = null)
        {
            List<STC.Common.Model.QQ.DataMessage> data = new List<STC.Common.Model.QQ.DataMessage>();
            PriceAndOrderViewModel priceAndOrderViewModel = this.DataContext as PriceAndOrderViewModel;
            if (string.IsNullOrEmpty(priceAndOrderViewModel.PriceModel.Security) || priceAndOrderViewModel.PriceModel.Security.ToLower() == "tradebook")
            {
                return;
            }
            if (!string.IsNullOrEmpty(side))
            {
                StringBuilder htmlBuilder = new StringBuilder();
                StringBuilder plainTxtBuilder = new StringBuilder();

                List<BrokerPriceViewModel> brokerList = GetBestBrokerList(side);
                foreach (BrokerPriceViewModel brokerPriceViewModel in brokerList)
                {
                    List<dynamic> paramList = new List<dynamic>();
                    dynamic d = new System.Dynamic.ExpandoObject();
                    d.side = side;
                    d.isCtrl = true;
                    d.isNeedSource = true;
                    d.isBestInvoke = true;
                    d.html = string.Empty;
                    d.plainTxt = string.Empty;
                    paramList.Add(d);
                    brokerPriceViewModel.Sub_CopyOrderData(paramList);

                    if (htmlBuilder.Length > 0)
                        htmlBuilder.Append("</br>");
                    htmlBuilder.Append(d.html);
                    
                    plainTxtBuilder.AppendLine(d.plainTxt);

                    if (LocalEnv.UserInfo.QeubeeFingPermissions == "1")
                    {
                        data.Add(await brokerPriceViewModel.GenDataMessage(d.side, d.type));
                    }
                }

                //复制
                ClipboardHelper.CopyToClipboard(htmlBuilder.ToString(), plainTxtBuilder.ToString());

                if (LocalEnv.UserInfo.QeubeeFingPermissions == "1")
                {
                    SendHttpQQ(plainTxtBuilder.ToString(), data);
                    //await ResulfApiHelper.ManualSendMessageToQQRequest(qq, plainTxt.ToString());
                }
                else
                {
                    CommonUtils.Prompt2Second("复制", "复制成功");
                }
            }
            else
            {
                string html;
                string plainTxt;
                string html_double;
                string plainTxt_double;
                string info;

                string securityId = priceAndOrderViewModel.PriceModel.Security;
                GetDoubleText(out html_double, out plainTxt_double);

                if (side == "1")
                {
                    string s = "GVN";
                    string px = priceAndOrderViewModel.PriceModel.BestBid;
                    info = $",    {s} {securityId} {px}";

                    html = $"<p>[{html_double}]{info}</p>";
                    plainTxt = $"[{plainTxt_double}]{info}";
                }
                else if (side == "2")
                {
                    string s = "TKN";
                    string px = priceAndOrderViewModel.PriceModel.BestOfr;
                    info = $",    {s} {securityId} {px}";

                    html = $"<p>[{html_double}]{info}</p>";
                    plainTxt = $"[{plainTxt_double}]{info}";
                }
                else
                {
                    html = $"<p>{html_double}</p>";
                    plainTxt = $"{plainTxt_double}";
                }


                //复制
                ClipboardHelper.CopyToClipboard(html, plainTxt);
            }
        }

        private void GenQtyAndSettle(string side, StringBuilder sb, BrokerPriceViewModel bpvm, string px)
        {
            if (side == "1")
            {
                if (bpvm.Bid.ConvertToDecimal() == px.ConvertToDecimal())
                {
                    if (sb.Length > 0)
                    {
                        sb.Append("+");
                    }

                    if (bpvm.BidCornerMarketText.Length > 0)
                    {
                        sb.Append($"[{bpvm.BidCornerMarketText}]");
                    }
                    else
                    {
                        sb.Append(bpvm.BidAmount);
                    }
                }
            }
            if (side == "2")
            {
                if (bpvm.Ofr.ConvertToDecimal() == px.ConvertToDecimal())
                {
                    if (sb.Length > 0)
                    {
                        sb.Append("+");
                    }

                    if (bpvm.OfrCornerMarketText.Length > 0)
                    {
                        sb.Append($"[{bpvm.OfrCornerMarketText}]");
                    }
                    else
                    {
                        sb.Append(bpvm.OfrAmount);
                    }
                }
            }
        }

        private void GetDoubleText(out string html, out string plainTxt)
        {
            PriceAndOrderViewModel priceAndOrderViewModel = this.DataContext as PriceAndOrderViewModel;

            string bidQty = priceAndOrderViewModel.PriceModel.BestBidAmount;
            string bidPx = priceAndOrderViewModel.PriceModel.BestBid;
            string bidSettle = priceAndOrderViewModel.PriceModel.BidSettleTypeCopy.Length > 0 ? $"({priceAndOrderViewModel.PriceModel.BidSettleTypeCopy})" : string.Empty;
            string bidV = priceAndOrderViewModel.PriceModel.BestBidVenue;

            string ofrQty = priceAndOrderViewModel.PriceModel.BestOfrAmount;
            string ofrPx = priceAndOrderViewModel.PriceModel.BestOfr;
            string ofrSettle = priceAndOrderViewModel.PriceModel.OfrSettleTypeCopy.Length > 0 ? $"({priceAndOrderViewModel.PriceModel.OfrSettleTypeCopy})" : string.Empty;
            string ofrV = priceAndOrderViewModel.PriceModel.BestOfrVenue;

            string securityId = priceAndOrderViewModel.PriceModel.Security;
            string symbolName = priceAndOrderViewModel.PriceModel.SymbolName;
            string remainingTime = priceAndOrderViewModel.PriceModel.RemainingTime;

            string cdcValue = CommonConstants.CDCValueTxt + priceAndOrderViewModel.PriceModel.CCDCValuation;


            string bidQtyAndSettle = string.Empty;
            if (bidPx == "- -")
            {
                bidQtyAndSettle = $"{bidQty}{bidSettle}";
            }
            else
            {
                if (string.IsNullOrEmpty(priceAndOrderViewModel.PriceModel.BidTxt))
                {
                    StringBuilder bidSb = new StringBuilder();
                    GenQtyAndSettle("1", bidSb, priceAndOrderViewModel.OneBrokerPriceViewModel, bidPx);
                    GenQtyAndSettle("1", bidSb, priceAndOrderViewModel.TwoBrokerPriceViewModel, bidPx);
                    GenQtyAndSettle("1", bidSb, priceAndOrderViewModel.ThreeBrokerPriceViewModel, bidPx);
                    GenQtyAndSettle("1", bidSb, priceAndOrderViewModel.FourBrokerPriceViewModel, bidPx);
                    GenQtyAndSettle("1", bidSb, priceAndOrderViewModel.FiveBrokerPriceViewModel, bidPx);
                    GenQtyAndSettle("1", bidSb, priceAndOrderViewModel.SixBrokerPriceViewModel, bidPx);
                    bidQtyAndSettle = bidSb.ToString();
                }
                else
                {
                    bidQtyAndSettle = priceAndOrderViewModel.PriceModel.BidTxt;
                }
            }

            string ofrQtyAndSettle = string.Empty;
            if (ofrPx == "- -")
            {
                ofrQtyAndSettle = $"{ofrQty}{ofrSettle}";
            }
            else
            {
                if (string.IsNullOrEmpty(priceAndOrderViewModel.PriceModel.OfrTxt))
                {
                    StringBuilder ofrSb = new StringBuilder();
                    GenQtyAndSettle("2", ofrSb, priceAndOrderViewModel.OneBrokerPriceViewModel, ofrPx);
                    GenQtyAndSettle("2", ofrSb, priceAndOrderViewModel.TwoBrokerPriceViewModel, ofrPx);
                    GenQtyAndSettle("2", ofrSb, priceAndOrderViewModel.ThreeBrokerPriceViewModel, ofrPx);
                    GenQtyAndSettle("2", ofrSb, priceAndOrderViewModel.FourBrokerPriceViewModel, ofrPx);
                    GenQtyAndSettle("2", ofrSb, priceAndOrderViewModel.FiveBrokerPriceViewModel, ofrPx);
                    GenQtyAndSettle("2", ofrSb, priceAndOrderViewModel.SixBrokerPriceViewModel, ofrPx);
                    ofrQtyAndSettle = ofrSb.ToString();
                }
                else
                {
                    ofrQtyAndSettle = priceAndOrderViewModel.PriceModel.OfrTxt;
                }
            }

            // todo 时间？？？

            //string v = string.Empty;
            //if (!isBest)
            //{
            //    v = $"{bidV}/{ofrV}";               
            //}

            //if (bidQty.EndsWith("K"))
            //{
            //    bidQty = bidQty.Replace("K", "000");
            //}

            //if (ofrQty.EndsWith("K"))
            //{
            //    ofrQty = ofrQty.Replace("K", "000");
            //}

            html = $"{remainingTime}&#9;{securityId}&#9;{symbolName}&#9;{bidPx}/{ofrPx}&#9;{bidQtyAndSettle}/{ofrQtyAndSettle}&#9;{cdcValue}";
            plainTxt = $"{remainingTime}    {securityId}    {symbolName}    {bidPx}/{ofrPx}    {bidQtyAndSettle}/{ofrQtyAndSettle}    {cdcValue}";
        }

        private List<BrokerPriceViewModel> GetBestBrokerList(string side)
        {
            PriceAndOrderViewModel priceAndOrderViewModel = this.DataContext as PriceAndOrderViewModel;
            List<BrokerPriceViewModel> brokerList = new List<BrokerPriceViewModel>();
            if (side == "1")
            {
                if (priceAndOrderViewModel.OneBrokerPriceViewModel.Bid == priceAndOrderViewModel.PriceModel.BestBid)
                    brokerList.Add(priceAndOrderViewModel.OneBrokerPriceViewModel);
                if (priceAndOrderViewModel.TwoBrokerPriceViewModel.Bid == priceAndOrderViewModel.PriceModel.BestBid)
                    brokerList.Add(priceAndOrderViewModel.TwoBrokerPriceViewModel);
                if (priceAndOrderViewModel.ThreeBrokerPriceViewModel.Bid == priceAndOrderViewModel.PriceModel.BestBid)
                    brokerList.Add(priceAndOrderViewModel.ThreeBrokerPriceViewModel);
                if (priceAndOrderViewModel.FourBrokerPriceViewModel.Bid == priceAndOrderViewModel.PriceModel.BestBid)
                    brokerList.Add(priceAndOrderViewModel.FourBrokerPriceViewModel);
                if (priceAndOrderViewModel.FiveBrokerPriceViewModel.Bid == priceAndOrderViewModel.PriceModel.BestBid)
                    brokerList.Add(priceAndOrderViewModel.FiveBrokerPriceViewModel);
                if (priceAndOrderViewModel.SixBrokerPriceViewModel.Bid == priceAndOrderViewModel.PriceModel.BestBid)
                    brokerList.Add(priceAndOrderViewModel.SixBrokerPriceViewModel);
            }
            else if (side == "2")
            {
                if (priceAndOrderViewModel.OneBrokerPriceViewModel.Ofr == priceAndOrderViewModel.PriceModel.BestOfr)
                    brokerList.Add(priceAndOrderViewModel.OneBrokerPriceViewModel);
                if (priceAndOrderViewModel.TwoBrokerPriceViewModel.Ofr == priceAndOrderViewModel.PriceModel.BestOfr)
                    brokerList.Add(priceAndOrderViewModel.TwoBrokerPriceViewModel);
                if (priceAndOrderViewModel.ThreeBrokerPriceViewModel.Ofr == priceAndOrderViewModel.PriceModel.BestOfr)
                    brokerList.Add(priceAndOrderViewModel.ThreeBrokerPriceViewModel);
                if (priceAndOrderViewModel.FourBrokerPriceViewModel.Ofr == priceAndOrderViewModel.PriceModel.BestOfr)
                    brokerList.Add(priceAndOrderViewModel.FourBrokerPriceViewModel);
                if (priceAndOrderViewModel.FiveBrokerPriceViewModel.Ofr == priceAndOrderViewModel.PriceModel.BestOfr)
                    brokerList.Add(priceAndOrderViewModel.FiveBrokerPriceViewModel);
                if (priceAndOrderViewModel.SixBrokerPriceViewModel.Ofr == priceAndOrderViewModel.PriceModel.BestOfr)
                    brokerList.Add(priceAndOrderViewModel.SixBrokerPriceViewModel);
            }
            return brokerList;
        }

        public async void CopyPointOrderData(string side)
        {
            PriceAndOrderViewModel priceAndOrderViewModel = this.DataContext as PriceAndOrderViewModel;
            if (string.IsNullOrEmpty(priceAndOrderViewModel.PriceModel.Security) || priceAndOrderViewModel.PriceModel.Security.ToLower() == "tradebook")
            {
                return;
            }
            if (priceAndOrderViewModel.BrokerIndex == 0)
            {
                StringBuilder htmlBuilder = new StringBuilder();
                StringBuilder plainTxtBuilder = new StringBuilder();
                string html_onlyCopy = string.Empty;
                string plainTxt_onlyCopy = string.Empty;
                decimal maxQty = -1;

                List<STC.Common.Model.QQ.DataMessage> data = new List<STC.Common.Model.QQ.DataMessage>();


                List<BrokerPriceViewModel> brokerList = GetBestBrokerList(side);
                foreach (BrokerPriceViewModel brokerPriceViewModel in brokerList)
                {
                    List<dynamic> paramList = new List<dynamic>();
                    dynamic d = new System.Dynamic.ExpandoObject();
                    d.side = side;
                    d.type = null;
                    d.isNeedSource = true;
                    d.isBestInvoke = true;
                    d.html = string.Empty;
                    d.plainTxt = string.Empty;
                    paramList.Add(d);
                    brokerPriceViewModel.Sub_CopyPointOrderData(paramList);

                    if (htmlBuilder.Length > 0)
                        htmlBuilder.Append("</br>");
                    htmlBuilder.Append(d.html);

                    plainTxtBuilder.AppendLine(d.plainTxt);

                    string q = d.Qty;
                    decimal brokerQty = q.ConvertToDecimal();
                    if (brokerQty > maxQty)
                    {
                        maxQty = brokerQty;
                        html_onlyCopy = d.html_onlyCopy;
                        plainTxt_onlyCopy = d.plainTxt_onlyCopy;
                    }

                    if (LocalEnv.UserInfo.QeubeeFingPermissions == "1")
                    {
                        data.Add(await brokerPriceViewModel.GenDataMessage(d.side, d.type));
                    }
                }

                //复制
                ClipboardHelper.CopyToClipboard(html_onlyCopy, plainTxt_onlyCopy);

                if (LocalEnv.UserInfo.QeubeeFingPermissions == "1")
                {
                    string lstsettle = string.Join(",",ConditionHelper.GetMainSettle(settleFilter.FilterDic).ToArray());

                    SendHttpQQ(plainTxtBuilder.ToString(), data, lstsettle);
                    //await ResulfApiHelper.ManualSendMessageToQQRequest(qq, plainTxt.ToString());
                }
                else
                {
                    CommonUtils.Prompt2Second("复制", "复制成功");
                }
            }
            else
            {
                #region old
                string html;
                string plainTxt;
                string html_double;
                string plainTxt_double;
                GetDoubleText(out html_double, out plainTxt_double);

                //PriceAndOrderViewModel priceAndOrderViewModel = this.DataContext as PriceAndOrderViewModel;

                string securityId = priceAndOrderViewModel.PriceModel.Security;
                string symbolName = priceAndOrderViewModel.PriceModel.SymbolName;

                string s = string.Empty;
                string qty = string.Empty;
                string px = string.Empty;
                if (side == "1")//GVN
                {
                    s = "GVN";

                    if (priceAndOrderViewModel.PriceModel.BestBid != "- -")
                    {
                        BrokerPriceViewModel bidBrokerPriceViewModel = CalT1Broker("1", priceAndOrderViewModel.PriceModel.BestBid);
                        if (priceAndOrderViewModel.BrokerIndex == 0)//全部
                        {
                            if (bidBrokerPriceViewModel != null && bidBrokerPriceViewModel.BidAmount != "- -" && bidBrokerPriceViewModel.BidAmount != "***")
                            {
                                if (bidBrokerPriceViewModel.BidT1Qty > 0)
                                {
                                    qty = bidBrokerPriceViewModel.BidT1Qty.ToString();
                                }
                            }
                        }
                        else//最优
                        {
                            qty = priceAndOrderViewModel.FourBrokerPriceViewModel.BidAmount;
                        }


                        if (bidBrokerPriceViewModel != null && bidBrokerPriceViewModel.Bid != "- -" && bidBrokerPriceViewModel.Bid != "***")
                        {
                            px = bidBrokerPriceViewModel.Bid;
                        }
                    }
                }
                else if (side == "2")//TKN
                {
                    s = "TKN";

                    if (priceAndOrderViewModel.PriceModel.BestOfr != "- - ")
                    {
                        BrokerPriceViewModel ofrBrokerPriceViewModel = CalT1Broker("2", priceAndOrderViewModel.PriceModel.BestOfr);

                        if (priceAndOrderViewModel.BrokerIndex == 0)
                        {
                            if (ofrBrokerPriceViewModel != null && ofrBrokerPriceViewModel.OfrAmount != "- -" && ofrBrokerPriceViewModel.OfrAmount != "***")
                            {
                                if (ofrBrokerPriceViewModel.OfrT1Qty > 0)
                                {
                                    qty = ofrBrokerPriceViewModel.OfrT1Qty.ToString();
                                }
                            }
                        }
                        else
                        {
                            qty = priceAndOrderViewModel.FourBrokerPriceViewModel.OfrAmount;
                        }

                        if (ofrBrokerPriceViewModel != null && ofrBrokerPriceViewModel.Ofr != "- -" && ofrBrokerPriceViewModel.Ofr != "***")
                        {
                            px = ofrBrokerPriceViewModel.Ofr;
                        }
                    }
                }

                string htmlPx = string.Empty;
                string txtPx = string.Empty;
                if (px.Length > 0)
                {
                    htmlPx = $"&#9;{px}";
                    txtPx = $" {px}";
                }

                string htmlQty = string.Empty;
                string txtQty = string.Empty;
                if (qty.Length > 0)
                {
                    htmlQty = $"&#9;{qty}w";
                    txtQty = $" {qty}w";
                }

                html = $"<p>[{html_double}],<br>{s}&#9;{securityId}&#9;{symbolName}{htmlPx}{htmlQty}</p>";

                plainTxt = $"[{plainTxt_double}],\r\n{s} {securityId} {symbolName}{txtPx}{txtQty}";

                //复制
                ClipboardHelper.CopyToClipboard(html.ToString(), plainTxt.ToString());

                if (plainTxt.Length > 0)
                    CommonUtils.Prompt2Second("复制", "复制成功");
                #endregion
            }


        }

        private void SendHttpQQ(string content, List<STC.Common.Model.QQ.DataMessage> qqdata, string selSettle = "")
        {
            SendFinQFriendReq_Best qqsmm = new SendFinQFriendReq_Best();
            qqsmm.content = content;
            qqsmm.fromQB = LocalEnv.UserInfo.UserName;
            qqsmm.data = qqdata;
            qqsmm.openChat = true;
            qqsmm.dataType = 1;
            qqsmm.fromQBAccountID = LocalEnv.UserInfo.UserId;

            //如果使用WebFinQ发送，则调用WebFinQ接口，否则调用QQ接口
            if (LocalEnv.ConfInfo.IsForceUseWebFinQ == "1")
            {
                qqsmm.UpdateTime = DateTimeEx.ConvertStringToDateTime((this.DataContext as PriceAndOrderViewModel).UpdateTime).ToLongTimeString();
                //综合最优新增用户选择的清算速度,临时使用变量 fromQBAccountID 传递
                qqsmm.fromQBAccountID = selSettle;
                AppUtils.Application.PluginManager.Application.BroadCast("ConvertSendFinQFriendReq_BestToWebFinQ", qqsmm);
            }
            else
            {
                PResult result = QQHandler.Instance.SendQQ_Best(qqsmm);
                if (result != null && result.IsSuccess())
                {
                    CommonUtils.Prompt2Second("金融Q", "已成功推送至金融Q");
                }
                else
                {
                    CommonUtils.Prompt2Second("金融Q", (result == null || string.IsNullOrEmpty(result.Msg)) ? "发送金融Q失败" : result.Msg);
                }
            }

            //string msg = ResulfApiHelper.GenMsg(content, securityId, symbolName, side_txt, join, px, vol_delivery, qq);
            //LogHelper.Debug(msg);
            //await ResulfApiHelper.SendMessageToQQ(msg);
        }

        private BrokerPriceViewModel CalT1Broker(string side, string px)
        {
            PriceAndOrderViewModel priceAndOrderViewModel = this.DataContext as PriceAndOrderViewModel;
            BrokerPriceViewModel bestT1BrokerPriceViewModel = null;
            if (side == "1")
            {
                for (int i = 0; i < 6; i++)
                {
                    BrokerPriceViewModel currentBrokerPriceViewModel = priceAndOrderViewModel.GetBrokerPriceViewModelByIndex(i);
                    if (currentBrokerPriceViewModel.Bid.ConvertToDecimal() == px.ConvertToDecimal())
                    {
                        if (bestT1BrokerPriceViewModel == null)
                        {
                            bestT1BrokerPriceViewModel = currentBrokerPriceViewModel;
                        }
                        else
                        {
                            if (currentBrokerPriceViewModel.BidT1Qty > bestT1BrokerPriceViewModel.BidT1Qty)
                            {
                                bestT1BrokerPriceViewModel = currentBrokerPriceViewModel;
                            }
                        }
                    }
                }
            }
            else if (side == "2")
            {
                for (int i = 0; i < 6; i++)
                {
                    BrokerPriceViewModel currentBrokerPriceViewModel = priceAndOrderViewModel.GetBrokerPriceViewModelByIndex(i);
                    if (currentBrokerPriceViewModel.Ofr.ConvertToDecimal() == px.ConvertToDecimal())
                    {
                        if (bestT1BrokerPriceViewModel == null)
                        {
                            bestT1BrokerPriceViewModel = currentBrokerPriceViewModel;
                        }
                        else
                        {
                            if (currentBrokerPriceViewModel.OfrT1Qty > bestT1BrokerPriceViewModel.OfrT1Qty)
                            {
                                bestT1BrokerPriceViewModel = currentBrokerPriceViewModel;
                            }
                        }
                    }
                }
            }
            return bestT1BrokerPriceViewModel;
        }

        public void CopyPointOrderData1(string side)
        {
            StringBuilder html = new StringBuilder();
            StringBuilder plainTxt = new StringBuilder();

            PriceAndOrderViewModel priceAndOrderViewModel = this.DataContext as PriceAndOrderViewModel;

            string securityId = priceAndOrderViewModel.PriceModel.Security;
            string symbolName = priceAndOrderViewModel.PriceModel.SymbolName;
            string remainingTime = priceAndOrderViewModel.PriceModel.RemainingTime;

            string s = string.Empty;
            string qty = string.Empty;
            string px = string.Empty;
            string settle = string.Empty;
            if (side == "1")//GVN
            {
                s = "GVN";

                qty = priceAndOrderViewModel.PriceModel.BestBidAmount;
                px = priceAndOrderViewModel.PriceModel.BestBid;
                settle = priceAndOrderViewModel.PriceModel.BidSettleTypeCopy;

            }
            else if (side == "2")//TKN
            {
                s = "TKN";

                qty = priceAndOrderViewModel.PriceModel.BestOfrAmount;
                px = priceAndOrderViewModel.PriceModel.BestOfr;
                settle = priceAndOrderViewModel.PriceModel.OfrSettleTypeCopy;
            }


            if (settle.Length == 0)
            {
                string htmlSettle = string.Empty;
                string txtSettle = string.Empty;
                if (settle.Length > 0)
                {
                    htmlSettle = $"&#9;{settle}";
                    txtSettle = $" {settle}";
                }

                if (qty.EndsWith("K"))
                {
                    qty = qty.Replace("K", "000") + "w";
                }

                html.Append($"<p>{s}&#9;{securityId}&#9;{symbolName}&#9;{px}{htmlSettle}&#9;{qty}</p>");

                plainTxt.Append($"{s} {securityId} {symbolName} {px}{txtSettle} {qty}");
            }
            else
            {
                string bidQty = priceAndOrderViewModel.PriceModel.BestBidAmount;
                string bidPx = priceAndOrderViewModel.PriceModel.BestBid;
                string bidSettle = priceAndOrderViewModel.PriceModel.BidSettleTypeCopy.Length > 0 ? $"({priceAndOrderViewModel.PriceModel.BidSettleTypeCopy})" : string.Empty;
                string bidV = priceAndOrderViewModel.PriceModel.BestBidVenue;

                string ofrQty = priceAndOrderViewModel.PriceModel.BestOfrAmount;
                string ofrPx = priceAndOrderViewModel.PriceModel.BestOfr;
                string ofrSettle = priceAndOrderViewModel.PriceModel.OfrSettleTypeCopy.Length > 0 ? $"({priceAndOrderViewModel.PriceModel.OfrSettleTypeCopy})" : string.Empty;
                string ofrV = priceAndOrderViewModel.PriceModel.BestOfrVenue;



                // todo 时间？？？

                //string v = $"{bidV}/{ofrV}";

                if (bidQty.EndsWith("K"))
                {
                    bidQty = bidQty.Replace("K", "000");
                }

                if (ofrQty.EndsWith("K"))
                {
                    ofrQty = ofrQty.Replace("K", "000");
                }

                html.Append($"<p>[{remainingTime}&#9;{securityId}&#9;{symbolName}&#9;{bidPx}/{ofrPx}&#9;{bidQty}{bidSettle}/{ofrQty}{ofrSettle}],    {s} {securityId} {px}</p>");

                plainTxt.Append($"[{remainingTime}    {securityId}    {symbolName}    {bidPx}/{ofrPx}    {bidQty}{bidSettle}/{ofrQty}{ofrSettle}],    {s} {securityId} {px}");
            }

            //复制
            ClipboardHelper.CopyToClipboard(html.ToString(), plainTxt.ToString());

            if (plainTxt.Length > 0)
                CommonUtils.Prompt2Second("复制", "复制成功");
        }

        public void CopyPointOrderData2(string side)
        {
            StringBuilder html = new StringBuilder();
            StringBuilder plainTxt = new StringBuilder();

            PriceAndOrderViewModel priceAndOrderViewModel = this.DataContext as PriceAndOrderViewModel;

            string s = string.Empty;
            string qty = string.Empty;
            string px = string.Empty;
            string settle = string.Empty;

            string securityId = priceAndOrderViewModel.PriceModel.Security;
            string symbolName = priceAndOrderViewModel.PriceModel.SymbolName;
            if (side == "1")//GVN
            {
                s = "GVN";

                qty = priceAndOrderViewModel.PriceModel.BestBidAmount;
                px = priceAndOrderViewModel.PriceModel.BestBid;
                settle = priceAndOrderViewModel.PriceModel.BidSettleTypeCopy;

            }
            else if (side == "2")//TKN
            {
                s = "TKN";

                qty = priceAndOrderViewModel.PriceModel.BestOfrAmount;
                px = priceAndOrderViewModel.PriceModel.BestOfr;
                settle = priceAndOrderViewModel.PriceModel.OfrSettleTypeCopy;
            }

            //html.Append($"<p><span style=\"color: {color}\">{s}</span>&#9;{securityId}&#9;{symbolName}&#9;{px}&#9;{qty}&#9;{settle}</p>");
            string htmlSettle = string.Empty;
            string txtSettle = string.Empty;
            if (settle.Length > 0)
            {
                htmlSettle = $"&#9;{settle}";
                txtSettle = $" {settle}";
            }

            if (qty.EndsWith("K"))
            {
                qty = qty.Replace("K", "000") + "w";
            }

            html.Append($"<p>{s}&#9;{securityId}&#9;{symbolName}&#9;{px}{htmlSettle}&#9;{qty}</p>");

            plainTxt.Append($"{s} {securityId} {symbolName} {px}{txtSettle} {qty}");
            
            //复制
            ClipboardHelper.CopyToClipboard(html.ToString(), plainTxt.ToString());

            if (plainTxt.Length > 0)
                CommonUtils.Prompt2Second("复制", "复制成功");
        }

        private void bdBp_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            CopyOrderData();
        }

        int i_exe = 0;
        bool isDouble_exe = false;
        bool isCtrlExe = false;
        private void BestExec_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            isCtrlExe = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);
            i_exe += 1;
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(0, 0, 0, 0, System.Windows.Forms.SystemInformation.DoubleClickTime);
            timer.Tick += (s, e1) => { timer.IsEnabled = false; i_exe = 0; UIElement_Click_Exe(sender, e); isDouble_exe = false; isCtrlExe = false; };
            timer.IsEnabled = true;
            if (i_exe % 2 == 0)
            {
                timer.IsEnabled = false;
                i_exe = 0;
                isDouble_exe = true;

                CopyExeData();
            }
        }

        private void UIElement_Click_Exe(object sender, MouseButtonEventArgs e)
        {
            if (isDouble_exe)
                return;

            if (isCtrlExe)
            {
                CopyExeData();
            }
            else
            {
                CopyExeData_single();
            }
        }

        private void CopyExeData_single()
        {
            StringBuilder html = new StringBuilder();
            StringBuilder plainTxt = new StringBuilder();

            PriceAndOrderViewModel priceAndOrderViewModel = this.DataContext as PriceAndOrderViewModel;
            if (string.IsNullOrEmpty(priceAndOrderViewModel.PriceModel.Security) || priceAndOrderViewModel.PriceModel.Security.ToLower() == "tradebook")
            {
                return;
            }

            string securityId = priceAndOrderViewModel.PriceModel.Security;
            string symbol = priceAndOrderViewModel.PriceModel.SymbolName;
            string remainingTime = priceAndOrderViewModel.PriceModel.RemainingTime;
            string source = priceAndOrderViewModel.PriceModel.LastExecSource;
            string side = priceAndOrderViewModel.PriceModel.LastExecSideShow;
            string px = (priceAndOrderViewModel.PriceModel.LastExecPrice == CommonConstants.NO_PRICE || priceAndOrderViewModel.PriceModel.LastExecPrice == CommonConstants.NO_BROKER) ? string.Empty : priceAndOrderViewModel.PriceModel.LastExecPrice;

            string cdcValue = CommonConstants.CDCValueTxt + priceAndOrderViewModel.PriceModel.CCDCValuation;

            string t = string.Empty;
            if (!string.IsNullOrEmpty(priceAndOrderViewModel.PriceModel.LastExecTime))
            {
                t = CommonUtils.ConvertToDateTime(priceAndOrderViewModel.PriceModel.LastExecTime).ToString("HH:mm:ss");
            }

            string htmlTime = string.Empty;
            string txtTime = string.Empty;
            if (t.Length > 0)
            {
                htmlTime = $"&#9;{t}";
                txtTime = $" {t}";
            }

            string newPx_html = string.Empty;
            string newPx_Txt = string.Empty;
            if (!string.IsNullOrEmpty(px))
            {
                newPx_html = $"&#9;{px}";
                newPx_Txt = $" {px}";
            }

            html.Append($"<p>{remainingTime}&#9;{securityId}&#9;{symbol}&#9;{source}&#9;{side}{newPx_html}</p>");

            plainTxt.Append($"{remainingTime} {securityId} {symbol} {source} {side}{newPx_Txt}");

            //复制
            ClipboardHelper.CopyToClipboard(html.ToString(), plainTxt.ToString());

            if (plainTxt.Length > 0)
                CommonUtils.Prompt2Second("复制", "复制成功");
        }

        private async void CopyExeData()
        {
            StringBuilder html = new StringBuilder();
            StringBuilder plainTxt = new StringBuilder();

            PriceAndOrderViewModel priceAndOrderViewModel = this.DataContext as PriceAndOrderViewModel;
            if (string.IsNullOrEmpty(priceAndOrderViewModel.PriceModel.Security) || priceAndOrderViewModel.PriceModel.Security.ToLower() == "tradebook")
            {
                return;
            }


            string securityId = priceAndOrderViewModel.PriceModel.Security;
            string symbol = priceAndOrderViewModel.PriceModel.SymbolName;
            string remainingTime = priceAndOrderViewModel.PriceModel.RemainingTime;
            string source = priceAndOrderViewModel.PriceModel.LastExecSource;
            string side = priceAndOrderViewModel.PriceModel.LastExecSideShow;
            string px = (priceAndOrderViewModel.PriceModel.LastExecPrice == CommonConstants.NO_PRICE || priceAndOrderViewModel.PriceModel.LastExecPrice == CommonConstants.NO_BROKER) ? string.Empty : priceAndOrderViewModel.PriceModel.LastExecPrice;

            string cdcValue = CommonConstants.CDCValueTxt + priceAndOrderViewModel.PriceModel.CCDCValuation;

            string t = string.Empty;
            if (!string.IsNullOrEmpty(priceAndOrderViewModel.PriceModel.LastExecTime))
            {
                t = CommonUtils.ConvertToDateTime(priceAndOrderViewModel.PriceModel.LastExecTime).ToString("HH:mm:ss");
            }

            string htmlTime = string.Empty;
            string txtTime = string.Empty;
            if (t.Length > 0)
            {
                htmlTime = $"&#9;{t}";
                txtTime = $" {t}";
            }

            string newPx_html = string.Empty;
            string newPx_Txt = string.Empty;
            if (!string.IsNullOrEmpty(px))
            {
                newPx_html = $"&#9;{px}";
                newPx_Txt = $" {px}";
            }

            html.Append($"<p>[{remainingTime}&#9;{securityId}&#9;{symbol}&#9;{source}&#9;{side}{newPx_html}],{side}&#9;{securityId}{newPx_html}</p>");

            if (LocalEnv.ConfInfo.IsForceUseWebFinQ == "1")
            {
                plainTxt.Append($"[{remainingTime} {securityId} {symbol} {source} {side}{newPx_Txt} {cdcValue}]");
            }
            else
            {
                plainTxt.Append($"[{remainingTime} {securityId} {symbol} {source} {side}{newPx_Txt}],{side} {securityId}{newPx_Txt}");
            }

            //复制
            ClipboardHelper.CopyToClipboard(html.ToString(), plainTxt.ToString());

            if (LocalEnv.UserInfo.QeubeeFingPermissions == "1")
            {
                if (LocalEnv.ConfInfo.IsForceUseWebFinQ == "1")
                {
                    SendHttpQQ(side, null, plainTxt.ToString(), securityId, symbol, string.Empty, priceAndOrderViewModel.PriceModel, t);
                }
                else
                {
                    SendHttpQQ(side, null, plainTxt.ToString(), securityId, symbol, string.Empty, priceAndOrderViewModel.PriceModel);
                }
                //await ResulfApiHelper.ManualSendMessageToQQRequest(qq, plainTxt.ToString());
            }
        }

        private async void SendHttpQQ(string side, string type, string content, string securityId, string symbolName, string qq, PriceModel pm, string updatetime = "")
        {
            BondsInfoPo bondsInfo = CommonUtils.GetBondsInfoPoBySecurityID(securityId);
            string[,] vol_delivery = null;
            string side_txt = side == "1" ? "bid" : "ofr";
            string reserve_side = side == "1" ? "ofr" : "bid";
            string bond_type = string.Empty;
            if (bondsInfo != null)
            {
                bond_type = bondsInfo.SECURITY_TYPE;
            }

            string join = string.Empty;
            if (type == "1")
            {
                join = $"join_{side_txt}";
            }
            else if (type == "2")
            {
                join = $"join_{reserve_side}";
            }

                Dispatcher.CurrentDispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Send, new Action(() =>
                {
                    SendFinQFriendReq qqsmm = new SendFinQFriendReq();
                    qqsmm.content = content;
                    qqsmm.dataType = 2;
                    qqsmm.data = new STC.Common.Model.QQ.DataMessage();

                    qqsmm.data.id = Guid.NewGuid().ToString("N");
                    qqsmm.data.comp_id = ComboSourceUtil.Instance.FindEnum(pm.LastExecSource, "venues_cn_tonumber");
                    qqsmm.data.code = securityId;
                    qqsmm.data.bond_type = bond_type;
                    qqsmm.data.name = symbolName;
                    qqsmm.data.side = side_txt;
                    qqsmm.data.join = join;
                    qqsmm.data.price = (pm.LastExecPrice == CommonConstants.NO_PRICE || pm.LastExecPrice == CommonConstants.NO_BROKER) ? string.Empty : pm.LastExecPrice;
                    qqsmm.data.vol_delivery = vol_delivery;
                    qqsmm.data.price_type = "3";
                    //qqsmm.data.settl_type = ResulfApiHelper.ConvertSettleType(pm.SettlType);

                    qqsmm.fromQB = LocalEnv.UserInfo.UserName;
                    qqsmm.openChat = true;
                    qqsmm.fromQBAccountID = LocalEnv.UserInfo.UserId;
                    qqsmm.data.val_yield = pm.CCDCValuation;
                    //如果使用WebFinQ发送，则调用WebFinQ接口，否则调用QQ接口
                    if (LocalEnv.ConfInfo.IsForceUseWebFinQ == "1")
                    {
                        qqsmm.UpdateTime = updatetime;
                        AppUtils.Application.PluginManager.Application.BroadCast("ConvertSendFinQFriendReqToWebFinQ", qqsmm);
                    }
                    else
                    {
                        PResult result = QQHandler.Instance.SendQQ(qqsmm);
                        if (result != null && result.IsSuccess())
                        {
                            CommonUtils.Prompt2Second("金融Q", "已成功推送至金融Q");
                        }
                        else
                        {
                            CommonUtils.Prompt2Second("金融Q", (result == null || string.IsNullOrEmpty(result.Msg)) ? "发送金融Q失败" : result.Msg);
                        }
                    }
                }));

            //string msg = ResulfApiHelper.GenMsg(content, securityId, symbolName, side_txt, join, px, vol_delivery, qq);
            //LogHelper.Debug(msg);
            //await ResulfApiHelper.SendMessageToQQ(msg);
        }



        private void Bid_CopySecurityID_mi_Click(object sender, RoutedEventArgs e)
        {
            PriceAndOrderViewModel priceAndOrderViewModel = this.DataContext as PriceAndOrderViewModel;
            string securityId = priceAndOrderViewModel.PriceModel.Security;
            ClipboardHelper.CopyToClipboard(securityId, securityId);
        }

        private void Bid_CopySymbolName_mi_Click(object sender, RoutedEventArgs e)
        {
            PriceAndOrderViewModel priceAndOrderViewModel = this.DataContext as PriceAndOrderViewModel;
            string symbolName = priceAndOrderViewModel.PriceModel.SymbolName;
            ClipboardHelper.CopyToClipboard(symbolName, symbolName);
        }

        private void Bid_CopySingleClick_mi_Click(object sender, RoutedEventArgs e)
        {
            CopyOrderData();
        }

        private void Bid_CopyDoubleClick_mi_Click(object sender, RoutedEventArgs e)
        {
            CopyPointOrderData("1");
        }

        private void Bid_CopyCtrlSingleClick_mi_Click(object sender, RoutedEventArgs e)
        {
            CopyOrderData("1");
        }

        private void Ofr_CopySecurityID_mi_Click(object sender, RoutedEventArgs e)
        {
            PriceAndOrderViewModel priceAndOrderViewModel = this.DataContext as PriceAndOrderViewModel;
            string securityId = priceAndOrderViewModel.PriceModel.Security;
            ClipboardHelper.CopyToClipboard(securityId, securityId);
        }

        private void Ofr_CopySymbolName_mi_Click(object sender, RoutedEventArgs e)
        {
            PriceAndOrderViewModel priceAndOrderViewModel = this.DataContext as PriceAndOrderViewModel;
            string symbolName = priceAndOrderViewModel.PriceModel.SymbolName;
            ClipboardHelper.CopyToClipboard(symbolName, symbolName);
        }

        private void Ofr_CopySingleClick_mi_Click(object sender, RoutedEventArgs e)
        {
            CopyOrderData();
        }

        private void Ofr_CopyDoubleClick_mi_Click(object sender, RoutedEventArgs e)
        {
            CopyPointOrderData("2");
        }

        private void Ofr_CopyCtrlSingleClick_mi_Click(object sender, RoutedEventArgs e)
        {
            CopyOrderData("2");
        }

        private void QBLinkBtn_Click(object sender, RoutedEventArgs e)
        {
            PriceAndOrderViewModel priceAndOrderViewModel = this.DataContext as PriceAndOrderViewModel;
            if (priceAndOrderViewModel != null && string.IsNullOrEmpty(priceAndOrderViewModel.PriceModel.Security) == false)
            {
                CommonUtils.OpenSmallBondWin(priceAndOrderViewModel.PriceModel.Security);
            }
        }

        public void SelectionChange(string groudId, string id, string security, string settle, bool needRefresh)
        {
            Instance_OnSelectBondChange(BondUpdateType.ChangeFocusRow, groudId, id, security, settle, needRefresh);
        }

        private void seSearch_LostFocus(object sender, RoutedEventArgs e)
        {
            SeSearch_SearchChanged(null, null);
        }

        PriceAndOrderViewModel _priceAndOrderViewModel;
        private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.DataContext is PriceAndOrderViewModel)
            {
                _priceAndOrderViewModel = this.DataContext as PriceAndOrderViewModel;
            }
            else
            {
                _priceAndOrderViewModel = null;
            }
        }
    }
}
