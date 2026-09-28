using com.gateway.log;
using BDB.Trade.Plugin.Cache;
using BDB.Trade.Plugin.Handler;
using BDB.Trade.Plugin.Handler.DataProcesser;
using BDB.Trade.Plugin.Model;
using BDB.Trade.Plugin.Utils;
using BDB.Trade.Plugin.View.Trade.Card;
using BDB.Trade.Plugin.ViewModel.Data;
using BDB.Trade.Plugin.ViewModel.Trade;
using BDB.Trade.Plugin.ViewModel.Trade.Card;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

using STC.Common.Model.Fix.Message;
using STC.Common.Utils;
using STC.Common.Model;
using STC.Common.Model.Admin;
using STC.Common.Model.Base;
using STC.Common.Handler.Base;
using HomePageDataHandler = BDB.Trade.Plugin.Handler.HomePageDataHandler;
using STC.Common.ViewModel.Data;
using BDB.Common.Model.OHLC;

namespace BDB.Trade.Plugin.ViewModel.Trade.Card
{
    public class PageCardItemViewModel : BaseViewModel
    {
        private List<PriceAndOrderView> priceAndOrderViewsList = new List<PriceAndOrderView>();
        private List<PriceAndOrderViewModel> priceAndOrderViewModelList = new List<PriceAndOrderViewModel>();
        private FrameworkElement ownUI;//PageCardItemView
        private string currentGroupId;
        private int currentPageNum;

        //更新债券信息
        private Thread _RefreshBaseDataThread = null;

        public List<PriceAndOrderViewModel> GetPriceAndOrderViewModelList()
        {
            return priceAndOrderViewModelList;
        }

        /// <summary>
        /// 分页切换重置数据
        /// </summary>
        public void ResetData(string groupId, int pageIndex, List<HomeGroupSecurityIDsInfo> securityIdList)
        {
            currentGroupId = groupId;
            currentPageNum = pageIndex;
            int index = 0;
            foreach (PriceAndOrderViewModel priceAndOrderViewModel in priceAndOrderViewModelList)
            {
                priceAndOrderViewModel.PageIndex = currentPageNum;
                //先清理数据
                priceAndOrderViewModel.OnDelete();


                //再重新获取数据
                HomeGroupSecurityIDsInfo securityIDsInfo = index >= securityIdList.Count ? null : securityIdList[index];

                if (securityIDsInfo == null || string.IsNullOrEmpty(securityIDsInfo.securityid))
                {
                    //添加面板
                    priceAndOrderViewModel.CustomSort = decimal.MaxValue;

                    priceAndOrderViewModel.InitByPageChange(null, string.Empty, string.Empty, groupId);
                }
                else
                {

                    priceAndOrderViewModel.PriceModel.Security = securityIDsInfo.securityid;
                    int sort = SortCache.Instance.GetSettleById(groupId, securityIDsInfo.securityid);
                    if (sort >= 0)
                        priceAndOrderViewModel.CustomSort = sort;
                    else
                        priceAndOrderViewModel.CustomSort = securityIdList.Count;

                    priceAndOrderViewModel.InitByPageChange(null, securityIDsInfo.securityid, securityIDsInfo.id, groupId);

                    //FirstSet(priceAndOrderViewModel.PriceModel.Security);

                    //priceAndOrderViewModel.InitData();

                    ////priceAndOrderViewModel.SetInitStatus(false);

                    //priceAndOrderViewModel.InitMarketData();

                    //CalBaseData(priceAndOrderViewModel);
                }

                index++;
            }

            RegistData();
        }


        public void ClearUI(bool clearCard)
        {
            if (ownUI != null)
            {
                //if (clearCard)
                //{
                //    Grid grid = ownUI.FindName("grid") as Grid;
                //    //foreach (UIElement ue in grid.Children)
                //    //{
                //    //    if (ue is PriceAndOrderView)
                //    //    {
                //    //        PriceAndOrderView priceAndOrderView = ue as PriceAndOrderView;
                //    //        (priceAndOrderView.marketView.execChartView.Parent as Grid).Children.Remove(priceAndOrderView.marketView.execChartView);
                //    //        priceAndOrderView.marketView.execChartView.model = null;
                //    //        priceAndOrderView.marketView.execChartView.Dispose();
                //    //    }
                //    //}
                //    grid.Children.Clear();
                //}

                if (priceAndOrderViewModelList.Count > 0)
                {
                    foreach (PriceAndOrderViewModel priceAndOrderViewModel in priceAndOrderViewModelList)
                    {
                        priceAndOrderViewModel.OnDelete();
                    }

                    if (clearCard)
                    {
                        foreach (PriceAndOrderViewModel priceAndOrderViewModel in priceAndOrderViewModelList)
                        {
                            priceAndOrderViewModel.OneBrokerPriceViewModel.Dispose();
                            priceAndOrderViewModel.TwoBrokerPriceViewModel.Dispose();
                            priceAndOrderViewModel.ThreeBrokerPriceViewModel.Dispose();
                            priceAndOrderViewModel.FourBrokerPriceViewModel.Dispose();
                            priceAndOrderViewModel.FiveBrokerPriceViewModel.Dispose();
                            priceAndOrderViewModel.SixBrokerPriceViewModel.Dispose();

                            priceAndOrderViewModel.Dispose();
                        }

                        foreach (PriceAndOrderView priceAndOrderView in priceAndOrderViewsList)
                        {
                            (priceAndOrderView.marketView.DataContext as MarketOrderViewModel).Dispose();
                        }

                        priceAndOrderViewsList.Clear();
                        priceAndOrderViewModelList.Clear();
                    }
                }
            }
        }

        public void Init(FrameworkElement ownUI, List<HomeGroupSecurityIDsInfo> securityIdList, int row, int col, string groupId, int showPageNum)
        {
            string key = PageCardHandler.GetWindowHashKey(ownUI);
            currentGroupId = groupId;
            this.ownUI = ownUI;
            currentPageNum = showPageNum;

            Grid grid = SetGrid(row, col);

            int index = 0;
            for (int r = 0; r < row; r++)
            {
                for (int c = 0; c < col; c++)
                {
                    PriceAndOrderView priceAndOrderView = null;
                    PriceAndOrderViewModel priceAndOrderViewModel = null;
                    if (priceAndOrderViewsList.Count > index)
                    {
                        priceAndOrderView = priceAndOrderViewsList[index];
                        priceAndOrderViewModel = priceAndOrderViewModelList[index];
                        CommonHandler.Instance.AddLoadNum(key);
                        CommonHandler.Instance.AddLoadNum(key);
                    }
                    else
                    {
                        priceAndOrderView = new PriceAndOrderView();
                        priceAndOrderViewsList.Add(priceAndOrderView);
                        priceAndOrderViewModel = new PriceAndOrderViewModel();
                        priceAndOrderViewModelList.Add(priceAndOrderViewModel);
                        priceAndOrderView.DataContext = priceAndOrderViewModel;
                    }
                    priceAndOrderView.bdFirst.BorderThickness = new Thickness(2);
                    priceAndOrderViewModel.PageIndex = currentPageNum;
                    priceAndOrderViewModel.RowIndex = r + 1;
                    priceAndOrderViewModel.ColIndex = c + 1;

                   

                    Grid.SetColumn(priceAndOrderView, c);
                    Grid.SetRow(priceAndOrderView, r);
                    if(priceAndOrderView.Parent == null)
                    {
                        grid.Children.Add(priceAndOrderView);
                    }

                    //再重新获取数据
                    HomeGroupSecurityIDsInfo securityIDsInfo = index >= securityIdList.Count ? null : securityIdList[index];

                    if (securityIDsInfo == null || string.IsNullOrEmpty(securityIDsInfo.securityid))
                    {
                        //添加面板
                        priceAndOrderViewModel.CustomSort = decimal.MaxValue;

                        priceAndOrderViewModel.InitByPageChange(priceAndOrderView, string.Empty, string.Empty, groupId);
                        priceAndOrderViewModel.SetMarketShowModel(TabShowModel.Card);
                    }
                    else
                    {
                        priceAndOrderViewModel.PriceModel.Security = securityIDsInfo.securityid;
                        int sort = SortCache.Instance.GetSettleById(groupId, securityIDsInfo.securityid);
                        if (sort >= 0)
                            priceAndOrderViewModel.CustomSort = sort;
                        else
                            priceAndOrderViewModel.CustomSort = securityIdList.Count;

                        // 初始化每个card 初始化
                        priceAndOrderViewModel.InitByPageChange(priceAndOrderView, securityIDsInfo.securityid, securityIDsInfo.id, groupId);
                        // 切换成交为卡片模式
                        priceAndOrderViewModel.SetMarketShowModel(TabShowModel.Card);
                        //    // 注册数据
                        //    FirstSet(priceAndOrderViewModel.PriceModel.Security);
                        //    // 计算数据（行情）
                        //    priceAndOrderViewModel.InitData();
                        //    // 计算数据 （成交）
                        //    priceAndOrderViewModel.InitMarketData();
                        //    // 计算数据 （基础）
                        //    CalBaseData(priceAndOrderViewModel);
                     }

                    index++;
                }
            }
        }

        /// <summary>
        /// UI加载完，开始初始化数据
        /// </summary>
        public void LoadData()
        {
            this.InitUI();

            RegistData();
        }

        private void RegistData()
        {
            foreach (PriceAndOrderViewModel priceAndOrderViewModel in priceAndOrderViewModelList)
            {
                Task.Factory.StartNew(() => 
                {
                    Application.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Send,new Action(() =>
                    {
                        LogHelper.Warn("PageCardItemViewModel LoadData: security:" + priceAndOrderViewModel.PriceModel.Security);

                        FirstSet(priceAndOrderViewModel.PriceModel.Security);

                        priceAndOrderViewModel.InitMarketData();

                        priceAndOrderViewModel.InitData();

                        CalBaseData(priceAndOrderViewModel);

                    }));
                });
            }
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

                    priceAndOrderViewModel.PriceModel.SymbolName = bondsInfoPo.Short_Name;
                    priceAndOrderViewModel.PriceModel.RemainingTime = bondsInfoPo.RemaingingTime;
                    priceAndOrderViewModel.PriceModel.MarketIndicator = bondsInfoPo.MarketIndicator;

                    priceAndOrderViewModel.AdapterFuture(bondsInfoPo.Bond_ID);
                }
                else
                {
                    bondWithOutInfoList[priceAndOrderViewModel.PriceModel.Security] = null;
                }
            }
        }


        private Grid SetGrid(int row, int col)
        {
            Grid grid = ownUI.FindName("grid") as Grid;
            //grid.Margin = new Thickness(2, -2, 2, -2);
            grid.Margin = new Thickness(0);
            if (row != grid.RowDefinitions.Count || col != grid.ColumnDefinitions.Count)
            {
                grid.RowDefinitions.Clear();
                for (int i = 0; i < row; i++)
                {
                    RowDefinition rd = new RowDefinition();
                    rd.Height = new GridLength(1, GridUnitType.Star);
                    grid.RowDefinitions.Add(rd);
                }
                grid.ColumnDefinitions.Clear();
                for (int i = 0; i < col; i++)
                {
                    ColumnDefinition cd = new ColumnDefinition();
                    cd.Width = new GridLength(1, GridUnitType.Star);
                    grid.ColumnDefinitions.Add(cd);
                }
            }
            grid.Children.Clear();
            return grid;
        }




        #region 业务处理

        private object _lockObj = new object();

        private AsyncThreadQueue<ExecutionReport> _ProcessLastExecutionsThreadQueue;
        //private AsyncThreadQueue<ExecutionReport> _ProcessMyOrdersThreadQueue;
        private AsyncThreadQueue<MarketDataSnapshotFullRefresh> _ProcessCtPriceThreadQueue;
        private AsyncThreadQueue<OhlcMsg> _ProcessOHLCThreadQueue;


        #region clear

        ~PageCardItemViewModel()
        {
            this.Dispose();

            if (_RefreshBaseDataThread != null)
            {
                _RefreshBaseDataThread.Abort();
                _RefreshBaseDataThread = null;
            }
        }

        new public void Dispose()
        {
            OnClear(false);
        }



        private void OnClear(bool isClear)
        {
            BaseHandler.Instance.UnSubscribe(Instance_OnClear);

            // 订阅我的订单
            //MyOrderProcesser.Processer.OnOwnOrders -= Instance_OnOrder;

            BondChangeHandler.Instance.OnSelectSettleChange -= Instance_OnMainSelectSettleChange;
            HomePageDataHandler.Instance.OnFavoritesChanged -= Instance_OnFavoritesChanged;
            BondChangeHandler.Instance.OnOneSettleChange -= Instance_OnOneSettleChange;
            BondChangeHandler.Instance.OnSortChange -= Instance_OnSortChange;

            LastExecutionProcesser.Processer.OnLastExecutions -= Instance_OnLastExecutions;

            hadInit = false;

            List<string> securityList = new List<string>();


            foreach (PriceAndOrderViewModel priceAndOrderViewModel in priceAndOrderViewModelList)
            {
                if (!securityList.Contains(priceAndOrderViewModel.PriceModel.Security))
                    securityList.Add(priceAndOrderViewModel.PriceModel.Security);
            }

            foreach (string security in securityList)
            {
                PriceHandler.Instance.UnregisterCTPrice(security, Instance_OnMDPrice);
                OhlcHandler.Instance.UnSubscribeOhlc(security, onOHLC);
            }


            if (isClear)
            {
                //if (_ProcessMyOrdersThreadQueue != null)
                //{
                //    _ProcessMyOrdersThreadQueue.Clear();
                //}

                if (_ProcessLastExecutionsThreadQueue != null)
                {
                    _ProcessLastExecutionsThreadQueue.Clear();
                }

                if (_ProcessCtPriceThreadQueue != null)
                {
                    _ProcessCtPriceThreadQueue.Clear();
                }
                if (_ProcessOHLCThreadQueue != null)
                {
                    _ProcessOHLCThreadQueue.Clear();
                }
            }
            else
            {
                //if (_ProcessMyOrdersThreadQueue != null)
                //{
                //    _ProcessMyOrdersThreadQueue.Dispose();
                //}

                if (_ProcessLastExecutionsThreadQueue != null)
                {
                    _ProcessLastExecutionsThreadQueue.Dispose();
                }

                if (_ProcessCtPriceThreadQueue != null)
                {
                    _ProcessCtPriceThreadQueue.Dispose();
                }
                if (_ProcessOHLCThreadQueue != null)
                {
                    _ProcessOHLCThreadQueue.Dispose();
                }
            }
        }


        private void Instance_OnClear(ClearPo obj)
        {
            LogHelper.Info(string.Format("PageCardItemViewModel Clear, GroupId:{0}", currentGroupId));
            if (obj != null && "Y".Equals(obj.IsClear))
            {
                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    lock (this._lockObj)
                    {
                        this.OnClear(true);

                        foreach (PriceAndOrderViewModel priceAndOrderViewModel in priceAndOrderViewModelList)
                        {
                            priceAndOrderViewModel.OnClear();
                        }

                        InitUI();

                        //foreach (PriceAndOrderViewModel priceAndOrderViewModel in priceAndOrderViewModelList)
                        //{
                        //    FirstSet(priceAndOrderViewModel.PriceModel.Security);

                        //    priceAndOrderViewModel.InitData();
                        //}

                        //增加延迟订阅，给退订完成留时间
                        ResetSub();
                    }
                }));

            }
        }

        private void ResetSub()
        {
            ThreadPool.QueueUserWorkItem(o =>
            {
                try
                {
                    Thread.Sleep(1000);

                    foreach (PriceAndOrderViewModel priceAndOrderViewModel in priceAndOrderViewModelList)
                    {
                        Thread.Sleep(100);

                        Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                        {
                            FirstSet(priceAndOrderViewModel.PriceModel.Security);

                            priceAndOrderViewModel.InitData();
                        }));
                    }
                }
                catch (Exception e)
                {
                    LogHelper.Error("ResetSub", e);
                }
            });
        }

        #endregion


        #region 刷新Bond信息

        private ConcurrentDictionary<string, object> bondWithOutInfoList = new ConcurrentDictionary<string, object>();

        private void RefreshBaseData()
        {
            // 刷新基础数据, 收到数据刷新一次结束

            while (true)
            {
                try
                {
                    Thread.Sleep(500);
                    if (CommonUtils.IsEndofLoadFromDB() && bondWithOutInfoList.Count > 0)
                    {
                        onloadData(true);
                    }
                    else if (CommonUtils.IsEndofLoadFromDB() && bondWithOutInfoList.Count <= 0)
                    {
                        break;
                    }
                }
                catch (ThreadAbortException)
                {
                    //ignore
                    Thread.Sleep(500);
                }
                catch (Exception e)
                {
                    LogHelper.Error(e);
                }
            }
        }

        private void onloadData(bool flag)
        {
            List<string> removeList = new List<string>();
            foreach (string securityID in this.bondWithOutInfoList.Keys)
            {
                BondsInfoPo bondsInfoPo = CommonUtils.GetBondsInfoPoBySecurityID(securityID);
                if (bondsInfoPo != null)
                {
                    foreach (PriceAndOrderViewModel priceAndOrderViewModel in priceAndOrderViewModelList)
                    {
                        Application.Current.Dispatcher.Invoke((Action)(() =>
                        {
                            if (priceAndOrderViewModel.PriceModel.Security == bondsInfoPo.Bond_ID)
                            {
                                CalBaseData(priceAndOrderViewModel);
                                removeList.Add(bondsInfoPo.Bond_ID);
                            }
                        }));
                    }
                }
                Thread.Sleep(50);
            }

            foreach (string securityID in removeList)
            {
                object removeOjb = null;
                bondWithOutInfoList.TryRemove(securityID, out removeOjb);
            }
        }

        #endregion

        private bool hadInit = false;
        private void InitUI()
        {
            // 注册基础数据
            BaseHandler.Instance.SubscribeLoadDataStatus(onloadData);
            if (_RefreshBaseDataThread == null)
            {
                _RefreshBaseDataThread = new Thread(RefreshBaseData);
                _RefreshBaseDataThread.IsBackground = true;
                _RefreshBaseDataThread.Start();
            }

            // 订阅我的订单
            //MyOrderProcesser.Processer.OnOwnOrders += Instance_OnOrder;

            //if (_ProcessMyOrdersThreadQueue == null)
            //{
            //    _ProcessMyOrdersThreadQueue = new AsyncThreadQueue<ExecutionReport>(UpdateMyOrdersAsync);
            //}

            if (_ProcessLastExecutionsThreadQueue == null)
            {
                _ProcessLastExecutionsThreadQueue = new AsyncThreadQueue<ExecutionReport>(UpdateLastExecutionsAsync);
            }

            if (_ProcessCtPriceThreadQueue == null)
            {
                _ProcessCtPriceThreadQueue = new AsyncThreadQueue<MarketDataSnapshotFullRefresh>(UpdateMDData);
            }

            if (_ProcessOHLCThreadQueue == null)
            {
                _ProcessOHLCThreadQueue = new AsyncThreadQueue<OhlcMsg>(UpdateOHLC);
            }

            if (!hadInit)
            {
                BaseHandler.Instance.Init(Instance_OnClear);
                BondChangeHandler.Instance.OnSelectSettleChange += Instance_OnMainSelectSettleChange;
                HomePageDataHandler.Instance.OnFavoritesChanged += Instance_OnFavoritesChanged;
                BondChangeHandler.Instance.OnOneSettleChange += Instance_OnOneSettleChange;
                BondChangeHandler.Instance.OnSortChange += Instance_OnSortChange;

                LastExecutionProcesser.Processer.OnLastExecutions += Instance_OnLastExecutions;

                hadInit = true;
            }
            //List<string> securityList = new List<string>();

            //foreach (PriceAndOrderViewModel priceAndOrderViewModel in priceAndOrderViewModelList)
            //{
            //    if (!securityList.Contains(priceAndOrderViewModel.PriceModel.Security))
            //        securityList.Add(priceAndOrderViewModel.PriceModel.Security);
            //}

            //foreach (string security in securityList)
            //{
            //    FirstSet(security);
            //}
        }

        private void FirstSet(string security)
        {
            PriceHandler.Instance.RegisterCTPrice(security, Instance_OnMDPrice);

            #region 订阅OHLC
            bool isFuture = CommonUtils.IsFutureBond(security);
            if (isFuture)
            {
                //PriceHandler.Instance.RegisterCTPrice(security, Instance_OnMDPrice);
            }
            else
            {
                OhlcHandler.Instance.SubscribeOhlc(security, onOHLC);
            }
            #endregion

            ExecutionReport obj = new ExecutionReport();
            obj.SecurityID = security;

            //Instance_OnOrder(obj);

            MarketExecutionsItem lastExeItem = LastExecutionProcesser.Processer.GetLastExecutionsItem(security);
            if (lastExeItem != null)
            {
                ExecutionReport temOrder = new ExecutionReport();
                temOrder.SecurityID = security;
                temOrder.Side = lastExeItem.Side == CommonConstants.SIDE_X ? CommonConstants.SIDE_OFR_TWO : lastExeItem.Side == CommonConstants.SIDE_Y ? CommonConstants.SIDE_BID_ONE : lastExeItem.Side == CommonConstants.SIDE_Z ? CommonConstants.SIDE_TRD_THREE : lastExeItem.Side;
                temOrder.LastPx = lastExeItem.ExecutePricePercent.ToString();
                temOrder.Venues = lastExeItem.SourceTrader;

                Instance_OnLastExecutions(temOrder);
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
            List<PriceAndOrderViewModel> updateData = priceAndOrderViewModelList.Where((o) => { return o.PriceModel.Security == data.securityid; }).ToList();

            foreach (PriceAndOrderViewModel priceAndOrderViewModel in updateData)
                priceAndOrderViewModel.onOHLC(data);

        }

        #endregion

        #region 债券crud


        private void Instance_OnFavoritesChanged(FavoriteTypeEnum typeEnum, string groupID, string securityID, string bondId)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                lock (_lockObj)
                {
                    if (currentGroupId == groupID)
                    {
                        if (typeEnum == FavoriteTypeEnum.AddBond || typeEnum == FavoriteTypeEnum.AddBondFromTable)
                        {
                            HomeGroupInfo homeGroupInfo = HomePageDataHandler.Instance.GetHomeGroupInfo(groupID);
                            if (homeGroupInfo != null && homeGroupInfo.securityids != null && LayoutSetttingCache.Instance.CurrentUpdatePanle != null)
                            {

                                //foreach (HomeGroupSecurityIDsInfo info in homeGroupInfo.securityids)
                                {
                                    //IList addPriceViewModel = priceAndOrderViewModelList.Where(o => string.IsNullOrEmpty(o.Id) 
                                    //    && o.PriceModel.Security.Equals(securityID) && !string.IsNullOrEmpty(o.GroupId) && o.GroupId.Equals(groupID)).ToList();

                                    IList addPriceViewModel = priceAndOrderViewModelList.Where(o => string.IsNullOrEmpty(o.Id)
                                           && o.GroupId.Equals(groupID) 
                                           && o.PageIndex == LayoutSetttingCache.Instance.CurrentUpdatePanle.PageIndex
                                           && o.RowIndex == LayoutSetttingCache.Instance.CurrentUpdatePanle.Row
                                           && o.ColIndex == LayoutSetttingCache.Instance.CurrentUpdatePanle.Col).ToList();

                                    if (addPriceViewModel != null && addPriceViewModel.Count > 0)
                                    {
                                        PriceAndOrderViewModel newCardItem = addPriceViewModel[0] as PriceAndOrderViewModel;

                                        newCardItem.PriceModel.Security = securityID;
                                        //newCardItem.HomeGroupSecurityIDsInfo = info;//HomePageDataHandler.Instance.GetHomeGroupSecurityIDsInfo(groupID, bondId);
                                        newCardItem.Id = bondId;//newCardItem.HomeGroupSecurityIDsInfo.id;
                                        newCardItem.GroupId = groupID;
                                        newCardItem.CustomSort = priceAndOrderViewsList.Count;


                                        IList addViewList = priceAndOrderViewsList.Where(delegate (PriceAndOrderView view)
                                        {
                                            PriceAndOrderViewModel newViewModel = view.DataContext as PriceAndOrderViewModel;
                                            if (newViewModel.PriceModel.Security.Equals(securityID) && newViewModel.Id.Equals(bondId) && newViewModel.GroupId.Equals(groupID))
                                            {
                                                return true;
                                            }
                                            else
                                                return false;
                                        }).ToList();

                                        if (addViewList != null && addViewList.Count > 0)
                                        {
                                            newCardItem.Init(addViewList[0] as PriceAndOrderView, securityID, bondId, groupID);

                                            CalBaseData(newCardItem);

                                            newCardItem.InitData();

                                            newCardItem.InitMarketData();

                                            PriceHandler.Instance.RegisterCTPrice(securityID, Instance_OnMDPrice);

                                            #region 订阅OHLC
                                            bool isFuture = CommonUtils.IsFutureBond(securityID);
                                            if (isFuture)
                                            {
                                                //PriceHandler.Instance.RegisterCTPrice(security, Instance_OnMDPrice);
                                            }
                                            else
                                            {
                                                OhlcHandler.Instance.SubscribeOhlc(securityID, onOHLC);
                                            }
                                            #endregion


                                            QcPagePo updatePage = new QcPagePo();
                                            updatePage.Index = currentPageNum;
                                            updatePage.GroupId = groupID;

                                            QcPanelPo panelPo = new QcPanelPo();
                                            panelPo.Id = bondId;
                                            panelPo.BondCode = securityID;

                                            updatePage.Panels.Add(panelPo);

                                            QuoteConfigHandler.Instance.NotifyAddPanl(updatePage);


                                            //break;
                                        }
                                    }
                                }

                                

                            }

                        }
                        else if (typeEnum == FavoriteTypeEnum.DelBond || typeEnum == FavoriteTypeEnum.DelBondFromTable)//TODO:删除逻辑重构
                        {

                            PriceAndOrderViewModel removeItem = null;
                            foreach (PriceAndOrderViewModel cardItem in priceAndOrderViewModelList)
                            {
                                if (cardItem.GroupId == groupID && cardItem.Id == bondId)
                                {
                                    removeItem = cardItem;
                                    break;
                                }
                            }
                            if (removeItem != null)
                            {
                                //priceAndOrderViewModelList.Remove(removeItem);
                                removeItem.OnDelete();
                                PriceHandler.Instance.UnregisterCTPrice(removeItem.PriceModel.Security, Instance_OnMDPrice);
                                
                                OhlcHandler.Instance.UnSubscribeOhlc(removeItem.PriceModel.Security, onOHLC);
                            }
                        }
                        else if (typeEnum == FavoriteTypeEnum.AddBondFromCard)
                        {
                            //HomeGroupInfo homeGroupInfo = HomePageDataHandler.Instance.GetHomeGroupInfo(groupID);
                            //if (homeGroupInfo != null)
                            //{
                            //    PriceAndOrderViewModel newCardItem = new PriceAndOrderViewModel();
                            //    newCardItem.GroupId = homeGroupInfo.id;
                            //    newCardItem.CustomSort = _priceAndOrderViewModelItems.Count + 2;
                            //    //newCardItem.Security = securityID;

                            //    CalBaseData(newCardItem);

                            //    _priceAndOrderViewModelItems.Add(newCardItem);
                            //}
                        }


                        //更新SQLite
                        //if (typeEnum == FavoriteTypeEnum.AddBond || typeEnum == FavoriteTypeEnum.AddBondFromTable || typeEnum == FavoriteTypeEnum.DelBond)
                        if (typeEnum == FavoriteTypeEnum.AddBond || typeEnum == FavoriteTypeEnum.DelBond)
                        {
                            QuoteConfigPo quoteConfigPo = QuoteConfigHandler.Instance.GetConfigByGroupId(groupID);//QueryConfig(groupID);
                            if (quoteConfigPo != null)
                            {
                                QcPagePo updatePage = null;
                                foreach (QcPagePo pagePo in quoteConfigPo.Pages)
                                {
                                    if (pagePo.Index == currentPageNum)
                                    {
                                        updatePage = pagePo;
                                        pagePo.IsSeleted = true;
                                        break;
                                    }
                                    else
                                    {
                                        pagePo.IsSeleted = false;
                                    }
                                }

                                updatePage.Panels = new List<QcPanelPo>();

                                bool isDeleteAll = true;
                                foreach (PriceAndOrderViewModel viewModel in priceAndOrderViewModelList)
                                {
                                    QcPanelPo panelPo = new QcPanelPo();
                                    panelPo.Id = viewModel.Id;
                                    panelPo.BondCode = viewModel.PriceModel.Security;
                                    panelPo.Row = viewModel.RowIndex;
                                    panelPo.Col = viewModel.ColIndex;

                                    updatePage.Panels.Add(panelPo);

                                    if (!string.IsNullOrEmpty(viewModel.PriceModel.Security))
                                    {
                                        isDeleteAll = false;
                                    }
                                }

                                if (typeEnum == FavoriteTypeEnum.DelBond && isDeleteAll && updatePage.Index > 1)
                                {
                                    quoteConfigPo.Pages.Remove(updatePage);

                                    int index = 1;
                                    foreach (QcPagePo page in quoteConfigPo.Pages)
                                    {
                                        page.Index = index;
                                        index++;
                                    }

                                    QuoteConfigHandler.Instance.NotifyDeletePage(updatePage);
                                }

                                
                            }
                            else
                            {
                                quoteConfigPo = new QuoteConfigPo();
                                quoteConfigPo.CurrentDisplayMode = DisplayMode.Card;
                                QcPagePo updatePage = new QcPagePo();
                                updatePage.Index = currentPageNum;
                                updatePage.IsSeleted = true;
                                quoteConfigPo.Pages = new List<QcPagePo>();
                                quoteConfigPo.Pages.Add(updatePage);
                                foreach (PriceAndOrderViewModel viewModel in priceAndOrderViewModelList)
                                {
                                    QcPanelPo panelPo = new QcPanelPo();
                                    panelPo.Id = viewModel.Id;
                                    panelPo.BondCode = viewModel.PriceModel.Security;
                                    panelPo.Row = viewModel.RowIndex;
                                    panelPo.Col = viewModel.ColIndex;

                                    updatePage.Panels.Add(panelPo);
                                }
                            }

                            QuoteConfigHandler.Instance.SaveConfig(quoteConfigPo, groupID);
                        }

                    }
                }
            }));
        }

        #endregion

        #region 结算方式改变

        public void Instance_OnMainSelectSettleChange(OperationType operationType, string groupId, object obj)
        {
            if ((operationType == OperationType.Card || operationType == OperationType.RemerberToCard) && currentGroupId == groupId)
            {
                foreach (PriceAndOrderView cardPriceView in priceAndOrderViewsList)
                {
                    PriceAndOrderViewModel priceAndOrderViewModel = cardPriceView.DataContext as PriceAndOrderViewModel;
                    if (priceAndOrderViewModel != null)
                    {
                        if (operationType == OperationType.Card)
                        {
                            priceAndOrderViewModel.SetSettle(operationType, obj);
                            priceAndOrderViewModel.SetBroker(operationType, obj);
                            priceAndOrderViewModel.SetExecType(operationType, obj);
                        }
                        else
                        {
                            string cacheSettle = SettleCache.Instance.GetSettleById(currentGroupId, priceAndOrderViewModel.Id);
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
        }


        private void Instance_OnOneSettleChange(OperationType operationType, string groupId, string id, List<string> settle)
        {
            if (operationType == OperationType.CardToTable || operationType == OperationType.TableToCard && currentGroupId == groupId)
            {
                foreach (PriceAndOrderView cardPriceView in priceAndOrderViewsList)
                {
                    PriceAndOrderViewModel priceAndOrderViewModel = cardPriceView.DataContext as PriceAndOrderViewModel;
                    if (priceAndOrderViewModel != null && priceAndOrderViewModel.GroupId == groupId && priceAndOrderViewModel.Id == id)
                    {
                        priceAndOrderViewModel.SetSettle(operationType, ConditionHelper.SettleListToDic(settle));

                        if (settle != null && settle.Count > 0)
                            SettleCache.Instance.AddOrUpdate(groupId, id, settle[0]);
                    }
                }
                

                if (settle != null && settle.Count > 0)
                {
                    RememberHelper.Instance.WriteCustomSettle(currentGroupId);
                }

              
            }
        }


        private void Instance_OnSortChange(OperationType operationType, string groupdId)
        {
            if (currentGroupId == groupdId && operationType == OperationType.TableToCard)
            {
                lock (_lockObj)
                {
                    foreach (PriceAndOrderViewModel item in priceAndOrderViewModelList)
                    {
                        if (!string.IsNullOrEmpty(item.Id))
                        {
                            int sort = SortCache.Instance.GetSettleById(item.GroupId, item.Id);
                            item.CustomSort = sort;
                        }
                    }

                    //CollectionView.SortDescriptions.Clear();
                    //CollectionView.SortDescriptions.Add(new SortDescription("CustomSort", ListSortDirection.Ascending));
                }
            }
        }

        #endregion



        #endregion

        #region 报价

        private void Instance_OnMDPrice(MarketDataSnapshotFullRefresh obj)
        {
            if (_ProcessCtPriceThreadQueue != null)
                _ProcessCtPriceThreadQueue.Enqueue(obj);
        }

        private void UpdateMDData(MarketDataSnapshotFullRefresh data)
        {
            List<PriceAndOrderViewModel> updateData = priceAndOrderViewModelList.Where((o) => { return o.PriceModel.Security == data.SecurityID; }).ToList();

            foreach (PriceAndOrderViewModel priceAndOrderViewModel in updateData)
                priceAndOrderViewModel.OnPrice(data, true);

        }





        #endregion


        //#region 我的订单

        ///// <summary>
        ///// 我的订单回调
        ///// </summary>
        ///// <param name="obj"></param>
        //private void Instance_OnOrder(ExecutionReport obj)
        //{
        //    //UpdateMyOrders(obj);
        //    if (_ProcessMyOrdersThreadQueue != null)
        //    {
        //        _ProcessMyOrdersThreadQueue.Enqueue(obj);
        //    }
        //}


        //private void UpdateMyOrdersAsync(ExecutionReport obj)
        //{
        //    List<PriceAndOrderViewModel> updateData = priceAndOrderViewModelList.Where((o) => { return o.PriceModel.Security == obj.SecurityID; }).ToList();

        //    foreach (PriceAndOrderViewModel priceAndOrderViewModel in updateData)
        //        priceAndOrderViewModel.OnOrder(obj);

        //}

        //#endregion

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
            List<PriceAndOrderViewModel> updateData = priceAndOrderViewModelList.Where((o) => { return o.PriceModel.Security == obj.SecurityID; }).ToList();

            foreach (PriceAndOrderViewModel priceAndOrderViewModel in updateData)
                priceAndOrderViewModel.OnLastExec(obj);

        }

        #endregion

        public void Drop(int prevRowIndex, int index, ListView dgData)
        {
            //lock (_lockObj)
            //{
            //    PriceAndOrderViewModel newItem = new PriceAndOrderViewModel();

            //    ListCollectionView listCollectionView = CollectionView as ListCollectionView;

            //    PriceAndOrderViewModel old = listCollectionView.GetItemAt(prevRowIndex) as PriceAndOrderViewModel;

            //    ConvertObject.CopyPriceAndOrderViewModel(old, newItem);

            //    string removeKey = old.Id;

            //    PriceAndOrderViewModel removeItem = null;
            //    foreach (PriceAndOrderViewModel cardItem in _priceAndOrderViewModelItems)
            //    {
            //        if (!string.IsNullOrEmpty(cardItem.Id) && cardItem.Id == removeKey)
            //        {
            //            removeItem = cardItem;
            //        }
            //    }

            //    _priceAndOrderViewModelItems.Remove(removeItem);

            //    if (prevRowIndex > index)
            //        newItem.CustomSort = index;
            //    else
            //        newItem.CustomSort = index + 1;

            //    _priceAndOrderViewModelItems.Add(newItem);

            //    listCollectionView = CollectionView as ListCollectionView;

            //    int flag = -1;
            //    PriceAndOrderViewModel nullCard = null;
            //    for (int i = 0; i < listCollectionView.Count; i++)
            //    {
            //        PriceAndOrderViewModel item = listCollectionView.GetItemAt(i) as PriceAndOrderViewModel;
            //        if (!string.IsNullOrEmpty(item.Id))
            //        {
            //            flag++;
            //            item.CustomSort = flag;

            //            SortCache.Instance.AddOrUpdate(item.GroupId, item.Id, flag);
            //        }
            //        else
            //            nullCard = item;
            //    }
            //    nullCard.CustomSort = decimal.MaxValue;

            //    PriceAndOrderViewModel nullInnerCard = null;
            //    foreach (PriceAndOrderViewModel item in _priceAndOrderViewModelItems)
            //    {
            //        if (!string.IsNullOrEmpty(item.Id))
            //        {
            //            int sort = SortCache.Instance.GetSettleById(item.GroupId, item.Id);
            //            item.CustomSort = sort;

            //        }
            //        else
            //            nullInnerCard = item;
            //    }
            //    nullInnerCard.CustomSort = decimal.MaxValue;

            //    dgData.SelectedIndex = index;

            //    string sortStr = RememberHelper.Instance.WriteCustomSort(_currentGroupId);//WriteCustomSort();

            //    BondChangeHandler.Instance.OnSortChanged(OperationType.CardToTable, _currentGroupId);
            //}

        }



        #region 过滤

        public void FilterData()
        {
            //CollectionView.Filter = predicate();
        }

        private Predicate<object> predicate()
        {
            return new Predicate<object>(NeedShow);
        }

        private bool NeedShow(object item)
        {
            PriceAndOrderViewModel cardItem = item as PriceAndOrderViewModel;
            bool result = false;

            Dictionary<string, IList> conditionDic = MainConditionCache.Instance.GetConditionById(currentGroupId) as Dictionary<string, IList>;

            if (conditionDic == null)
                return true;

            if (conditionDic.ContainsKey(CommonConstants.SECURITYID))
            {
                IList securityList = conditionDic[CommonConstants.SECURITYID];
                if (securityList.Count > 0)
                {
                    string securityId = securityList[0] as string;
                    if (cardItem.PriceModel.Security != securityId)
                        return result;
                }
            }
            else
            {
                result = true;
            }

            if (conditionDic.ContainsKey(CommonConstants.REMAINING_TIME))
            {
                IList remaingTimeList = conditionDic[CommonConstants.REMAINING_TIME];
                foreach (ListBoxItem lbi in remaingTimeList)
                {
                    if (lbi.Content.ToString() == CommonConstants.ALL)
                    {
                        result = true;

                        break;
                    }

                    if ((lbi.Content.ToString() == "0~1Y" && (cardItem.RemaingingTimeConvert > 0 && cardItem.RemaingingTimeConvert < 365))
                        || (lbi.Content.ToString() == "1~3Y" && (cardItem.RemaingingTimeConvert >= 365 && cardItem.RemaingingTimeConvert < (365 * 3)))
                        || (lbi.Content.ToString() == "3~5Y" && (cardItem.RemaingingTimeConvert >= (365 * 3) && cardItem.RemaingingTimeConvert < (365 * 5)))
                        || (lbi.Content.ToString() == "5~7Y" && (cardItem.RemaingingTimeConvert >= (365 * 5) && cardItem.RemaingingTimeConvert < (365 * 7)))
                        || (lbi.Content.ToString() == "7~10Y" && (cardItem.RemaingingTimeConvert >= (365 * 7) && cardItem.RemaingingTimeConvert < (365 * 10)))
                        || (lbi.Content.ToString() == ">10Y" && cardItem.RemaingingTimeConvert >= (365 * 10)))
                    {
                        result = true;
                        break;
                    }
                    else
                        result = false;
                }
            }
            else
            {
                result = true;
            }

            if (result)
            {
                if (conditionDic.ContainsKey(CommonConstants.BOND_TYPE))
                {
                    IList bondList = conditionDic[CommonConstants.BOND_TYPE];
                    foreach (ListBoxItem lbi in bondList)
                    {
                        if (lbi.Content.ToString() == CommonConstants.ALL)
                        {
                            result = true;

                            break;
                        }

                        if ((lbi.Content.ToString() == "国债" && cardItem.BondCategory == "A")
                            || (lbi.Content.ToString() == "央票" && cardItem.BondCategory == "B")
                            || (lbi.Content.ToString() == "金融债" && cardItem.BondCategory == "C")
                            || (lbi.Content.ToString() == "地方债" && cardItem.BondCategory == "D"))
                        {
                            result = true;

                            break;
                        }
                        else
                        {
                            result = false;
                        }
                    }
                }
            }



            return result;
        }

        #endregion


    }
}
