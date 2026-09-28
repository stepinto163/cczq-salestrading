using com.gateway.log;
using BDB.Trade.Plugin.Cache;
using BDB.Trade.Plugin.Handler;
using BDB.Trade.Plugin.Handler.DataProcesser;
using BDB.Trade.Plugin.Utils;
using BDB.Trade.Plugin.View.Trade.Card;
using BDB.Trade.Plugin.ViewModel.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using STC.Common.Model.Fix.Message;
using STC.Common.Utils;
using STC.Common.Model;
using STC.Common.Model.Admin;
using HomePageDataHandler = BDB.Trade.Plugin.Handler.HomePageDataHandler;
using STC.Common.Model.Base;
using STC.Common.Handler.Base;
using STC.Common.ViewModel.Data;

namespace BDB.Trade.Plugin.ViewModel.Trade.Card
{
    public class NewCardPriceViewModel : BaseViewModel
    {
        private FrameworkElement _ownUI = null;
        private object _lock = new object();

        private string _currentGroupId = string.Empty;

        private ObservableCollection<PriceAndOrderViewModel> _priceAndOrderViewModelItems = new ObservableCollection<PriceAndOrderViewModel>();
        public ObservableCollection<PriceAndOrderViewModel> PriceAndOrderViewModelItems { get { return _priceAndOrderViewModelItems; } set { _priceAndOrderViewModelItems = value; this.OnPropertyChanged(); } }

        private ICollectionView _collectionView = null;

        public ICollectionView CollectionView
        {
            get
            {
                if (_collectionView == null)
                    _collectionView = CollectionViewSource.GetDefaultView(PriceAndOrderViewModelItems);

                return _collectionView;
            }
        }

        #region 拖拽

        private string _dragTxt = string.Empty;

        public string DragTxt { get { return _dragTxt; } set { _dragTxt = value; this.OnPropertyChanged(); } }

        private PriceAndOrderViewModel _selectedListViewItem;

        public PriceAndOrderViewModel SelectedListViewItem
        {
            get { return _selectedListViewItem; }
            set
            {
                _selectedListViewItem = value;

                DragTxt = string.Format("{0} {1} {2}", _selectedListViewItem.PriceModel.Security, _selectedListViewItem.PriceModel.SymbolName, _selectedListViewItem.PriceModel.RemainingTime);

            }
        }

        #endregion

        public void Init(string groupId, FrameworkElement ownUI)
        {
            _currentGroupId = groupId;
            long start = DateTime.Now.Ticks;
            HomeGroupInfo homeGroupInfo = HomePageDataHandler.Instance.GetHomeGroupInfo(groupId);
            if (homeGroupInfo != null && homeGroupInfo.securityids != null)
            {
                Dictionary<string, string> dic = new Dictionary<string, string>();
                foreach (PriceAndOrderViewModel paov in PriceAndOrderViewModelItems)
                {
                    if (!dic.ContainsKey(paov.Id))
                        dic.Add(paov.Id, groupId);
                }

                //for (int i = 0; i < 30; i++)
                {
                    //int i = 0;
                    foreach (HomeGroupSecurityIDsInfo securityIDsInfo in homeGroupInfo.securityids)
                    {
                        //bool result = dic.ContainsKey(securityIDsInfo.id);

                        //if (!result)
                        {
                            PriceAndOrderViewModel priceAndOrderViewModel = new PriceAndOrderViewModel();
                            priceAndOrderViewModel.Id = securityIDsInfo.id;
                            priceAndOrderViewModel.GroupId = securityIDsInfo.groupId;
                            priceAndOrderViewModel.PriceModel.Security = securityIDsInfo.securityid;
                            int sort = SortCache.Instance.GetSettleById(securityIDsInfo.groupId, securityIDsInfo.id);
                            if (sort >= 0)
                                priceAndOrderViewModel.CustomSort = sort;
                            else
                                priceAndOrderViewModel.CustomSort = PriceAndOrderViewModelItems.Count;

                            priceAndOrderViewModel.Init(ownUI, securityIDsInfo.securityid, securityIDsInfo.id, securityIDsInfo.groupId);

                            CalBaseData(priceAndOrderViewModel);
                            
                            PriceAndOrderViewModelItems.Add(priceAndOrderViewModel);

                            
                        }

                        //if (i == 10)
                        //    break;

                        //i++;
                    }

                    //添加面板
                    PriceAndOrderViewModel addView = new PriceAndOrderViewModel();
                    addView.Id = string.Empty;
                    addView.GroupId = groupId;

                    addView.CustomSort = decimal.MaxValue;

                    addView.Init(ownUI, string.Empty, string.Empty, groupId);

                    PriceAndOrderViewModelItems.Add(addView);

                }
            }

            CollectionView.SortDescriptions.Add(new SortDescription("CustomSort", ListSortDirection.Ascending));


            this.InitUI(ownUI);

            //long end = DateTime.Now.Ticks;

            //Console.WriteLine("执行一次Init耗时:{0} 秒" + (end - start) / 10000000.0);

            //LogHelper.Info("-------------执行一次Init耗时:{0} 秒" + (end - start) / 10000000.0);

            
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


        #region 业务处理

        private object _lockObj = new object();

        private AsyncThreadQueue<ExecutionReport> _ProcessLastExecutionsThreadQueue;
        private AsyncThreadQueue<ExecutionReport> _ProcessMyOrdersThreadQueue;
        private AsyncThreadQueue<MarketDataSnapshotFullRefresh> _ProcessCtPriceThreadQueue;


        #region clear

        ~NewCardPriceViewModel()
        {
            this.Dispose();
        }

        new public void Dispose()
        {
            OnClear(false);
        }

        private void OnClear(bool isClear)
        {
            BaseHandler.Instance.UnSubscribe(Instance_OnClear);

            // 订阅我的订单
            MyOrderProcesser.Processer.OnOwnOrders -= Instance_OnOrder;

            BondChangeHandler.Instance.OnSelectSettleChange -= Instance_OnMainSelectSettleChange;
            HomePageDataHandler.Instance.OnFavoritesChanged -= Instance_OnFavoritesChanged;
            BondChangeHandler.Instance.OnOneSettleChange -= Instance_OnOneSettleChange;
            BondChangeHandler.Instance.OnSortChange -= Instance_OnSortChange;

            LastExecutionProcesser.Processer.OnLastExecutions -= Instance_OnLastExecutions;

            HomePageDataHandler.Instance.OnFavoritesChanged -= Instance_OnFavoritesChanged;

            List<string> securityList = new List<string>();
            foreach (PriceAndOrderViewModel priceAndOrderViewModel in _priceAndOrderViewModelItems)
            {
                if (!securityList.Contains(priceAndOrderViewModel.PriceModel.Security))
                    securityList.Add(priceAndOrderViewModel.PriceModel.Security);
                
            }

            foreach (string security in securityList)
            {
                PriceHandler.Instance.UnregisterCTPrice(security, Instance_OnMDPrice);
            }


            if (isClear)
            {
                if (_ProcessMyOrdersThreadQueue != null)
                {
                    _ProcessMyOrdersThreadQueue.Clear();
                }

                if (_ProcessLastExecutionsThreadQueue != null)
                {
                    _ProcessLastExecutionsThreadQueue.Clear();
                }

                if (_ProcessCtPriceThreadQueue != null)
                {
                    _ProcessCtPriceThreadQueue.Clear();
                }
            }
            else
            {
                if (_ProcessMyOrdersThreadQueue != null)
                {
                    _ProcessMyOrdersThreadQueue.Dispose();
                }

                if (_ProcessLastExecutionsThreadQueue != null)
                {
                    _ProcessLastExecutionsThreadQueue.Dispose();
                }

                if (_ProcessCtPriceThreadQueue != null)
                {
                    _ProcessCtPriceThreadQueue.Dispose();
                }
            }

            

        }


        private void Instance_OnClear(ClearPo obj)
        {
            LogHelper.Info(string.Format("NewCardPriceViewModel Clear, GroupId:{0}", this._currentGroupId));
            if (obj != null && "Y".Equals(obj.IsClear))
            {
                try
                {
                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        lock (this._lock)
                        {
                            this.OnClear(true);
                            
                            foreach (PriceAndOrderViewModel priceAndOrderViewModel in _priceAndOrderViewModelItems)
                            {
                                priceAndOrderViewModel.OnClear();
                            }
                            

                            InitUI(_ownUI);
                        }
                    }));
                }
                catch (Exception e)
                {
                    LogHelper.Error("PriceTableViewModel clear exception:" + e.Message);
                }
            }
        }


        private void InitUI(FrameworkElement ownUI)
        {
            _ownUI = ownUI;

            BaseHandler.Instance.Init(Instance_OnClear);

            // 订阅我的订单
            MyOrderProcesser.Processer.OnOwnOrders += Instance_OnOrder;

            if (_ProcessMyOrdersThreadQueue == null)
            {
                _ProcessMyOrdersThreadQueue = new AsyncThreadQueue<ExecutionReport>(UpdateMyOrdersAsync);
            }

            if (_ProcessLastExecutionsThreadQueue == null)
            {
                _ProcessLastExecutionsThreadQueue = new AsyncThreadQueue<ExecutionReport>(UpdateLastExecutionsAsync);
            }

            if (_ProcessCtPriceThreadQueue == null)
            {
                _ProcessCtPriceThreadQueue = new AsyncThreadQueue<MarketDataSnapshotFullRefresh>(UpdateMDData);
            }

            BondChangeHandler.Instance.OnSelectSettleChange += Instance_OnMainSelectSettleChange;
            HomePageDataHandler.Instance.OnFavoritesChanged += Instance_OnFavoritesChanged;
            BondChangeHandler.Instance.OnOneSettleChange += Instance_OnOneSettleChange;
            BondChangeHandler.Instance.OnSortChange += Instance_OnSortChange;

            LastExecutionProcesser.Processer.OnLastExecutions += Instance_OnLastExecutions;
            

            List<string> securityList = new List<string>();

            foreach (PriceAndOrderViewModel priceAndOrderViewModel in _priceAndOrderViewModelItems)
            {
                if (!securityList.Contains(priceAndOrderViewModel.PriceModel.Security))
                    securityList.Add(priceAndOrderViewModel.PriceModel.Security);
            }

            foreach (string security in securityList)
            {
                FirstSet(security);
            }
        }

        private void FirstSet(string security)
        {
            PriceHandler.Instance.RegisterCTPrice(security, Instance_OnMDPrice);

            ExecutionReport obj = new ExecutionReport();
            obj.SecurityID = security;

            Instance_OnOrder(obj);

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

        #region 债券crud
        

        private void Instance_OnFavoritesChanged(FavoriteTypeEnum typeEnum, string groupID, string securityID, string bondId)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                lock (_lock)
                {
                    if (_currentGroupId == groupID)
                    {
                        if (typeEnum == FavoriteTypeEnum.AddBond)
                        {
                            HomeGroupInfo homeGroupInfo = HomePageDataHandler.Instance.GetHomeGroupInfo(groupID);
                            if (homeGroupInfo != null && homeGroupInfo.securityids != null)
                            {
                                foreach (HomeGroupSecurityIDsInfo info in homeGroupInfo.securityids)
                                {
                                    if (_priceAndOrderViewModelItems.Any(o => o.Id != null && o.Id.Equals(info.id)) == false)
                                    {
                                        PriceAndOrderViewModel newCardItem = new PriceAndOrderViewModel();
                                        newCardItem.PriceModel.Security = info.securityid;
                                        //newCardItem.HomeGroupSecurityIDsInfo = info;//HomePageDataHandler.Instance.GetHomeGroupSecurityIDsInfo(groupID, bondId);
                                        newCardItem.Id = info.id;//newCardItem.HomeGroupSecurityIDsInfo.id;
                                        newCardItem.GroupId = info.groupId;
                                        newCardItem.CustomSort = _priceAndOrderViewModelItems.Count;

                                        newCardItem.Init(_ownUI, info.securityid, info.id, info.groupId);

                                        CalBaseData(newCardItem);

                                        _priceAndOrderViewModelItems.Add(newCardItem);

                                        PriceHandler.Instance.RegisterCTPrice(info.securityid, Instance_OnMDPrice);
                                    }
                                }

                                //int index = 0;
                                //CardItem nullItem = null;
                                //foreach (CardItem cardItem in CardItems)
                                //{
                                //    if (!string.IsNullOrEmpty(cardItem.Id))
                                //        cardItem.CustomSort = index;
                                //    else
                                //        nullItem = cardItem;

                                //    index++;
                                //}

                                //nullItem.CustomSort = index;
                            }

                        }
                        else if (typeEnum == FavoriteTypeEnum.DelBond)
                        {

                            PriceAndOrderViewModel removeItem = null;
                            foreach (PriceAndOrderViewModel cardItem in _priceAndOrderViewModelItems)
                            {
                                if (cardItem.GroupId == groupID && cardItem.Id == bondId)
                                {
                                    removeItem = cardItem;
                                    break;
                                }
                            }
                            if (removeItem != null)
                            {
                                _priceAndOrderViewModelItems.Remove(removeItem);
                                PriceHandler.Instance.UnregisterCTPrice(removeItem.PriceModel.Security, Instance_OnMDPrice);
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
                    }
                }
            }));
        }

        #endregion

        #region 结算方式改变

        public void Instance_OnMainSelectSettleChange(OperationType operationType, string groupId, object obj)
        {
            if ((operationType == OperationType.Card || operationType == OperationType.RemerberToCard) && _currentGroupId == groupId)
            {
                ListView listView = _ownUI.FindName("FavoriteBondslistBox") as ListView;
                foreach (var item in listView.Items)
                {
                    ListViewItem viewItem = (ListViewItem)listView.ItemContainerGenerator.ContainerFromItem(item);
                    if (viewItem != null)
                    {
                        ContentPresenter contentPresenter = CommonUtils.FindVisualChild<ContentPresenter>(viewItem);

                        if (contentPresenter != null)
                        {
                            DataTemplate myDataTemplate = contentPresenter.ContentTemplate;

                            PriceAndOrderView cardPriceView = myDataTemplate.FindName("cpvCard", contentPresenter) as PriceAndOrderView;
                            if (cardPriceView != null)
                            {
                                PriceAndOrderViewModel priceAndOrderViewModel = cardPriceView.DataContext as PriceAndOrderViewModel;
                                if (priceAndOrderViewModel != null)
                                {
                                    if (operationType == OperationType.Card)
                                    {
                                        priceAndOrderViewModel.SetSettle(operationType, obj);

                                    }
                                    else
                                    {
                                        string cacheSettle = SettleCache.Instance.GetSettleById(_currentGroupId, priceAndOrderViewModel.Id);
                                        if (!string.IsNullOrEmpty(cacheSettle))
                                        {
                                            List<string> list = new List<string>();
                                            list.Add(cacheSettle);
                                            priceAndOrderViewModel.SetSettle(operationType, ConditionHelper.SettleListToDic(list));
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                //foreach (PriceAndOrderViewModel priceAndOrderViewModel in _priceAndOrderViewModelItems)
                //        priceAndOrderViewModel.OnSettleChange(operationType, obj);
                
            }
        }


        private void Instance_OnOneSettleChange(OperationType operationType, string groupId, string id, List<string> settle)
        {
            if (operationType == OperationType.CardToTable || operationType == OperationType.TableToCard && _currentGroupId == groupId)
            {
                ListView listView = _ownUI.FindName("FavoriteBondslistBox") as ListView;
                foreach (var item in listView.Items)
                {
                    ListViewItem viewItem = (ListViewItem)listView.ItemContainerGenerator.ContainerFromItem(item);
                    if (viewItem != null)
                    {
                        ContentPresenter contentPresenter = CommonUtils.FindVisualChild<ContentPresenter>(viewItem);

                        if (contentPresenter != null)
                        {
                            DataTemplate myDataTemplate = contentPresenter.ContentTemplate;

                            PriceAndOrderView cardPriceView = myDataTemplate.FindName("cpvCard", contentPresenter) as PriceAndOrderView;
                            if (cardPriceView != null)
                            {
                                PriceAndOrderViewModel priceAndOrderViewModel = cardPriceView.DataContext as PriceAndOrderViewModel;
                                if (priceAndOrderViewModel != null && priceAndOrderViewModel.GroupId == groupId && priceAndOrderViewModel.Id == id)
                                {
                                    priceAndOrderViewModel.SetSettle(operationType, ConditionHelper.SettleListToDic(settle));

                                    if (settle != null && settle.Count > 0)
                                        SettleCache.Instance.AddOrUpdate(groupId, id, settle[0]);
                                }
                            }
                        }
                    }
                }

                if (settle != null && settle.Count > 0)
                {
                    RememberHelper.Instance.WriteCustomSettle(_currentGroupId);
                }

                //List<PriceAndOrderViewModel> updateData = _priceAndOrderViewModelItems.Where((o) => { return o.GroupId == groupId && o.Id == id; }).ToList();

                //foreach (PriceAndOrderViewModel priceAndOrderViewModel in updateData)
                //    priceAndOrderViewModel.OnSettleChange(operationType, ConditionHelper.SettleListToDic(settle));
            }
        }


        private void Instance_OnSortChange(OperationType operationType, string groupdId)
        {
            if (_currentGroupId == groupdId && operationType == OperationType.TableToCard)
            {
                lock (_lock)
                {
                    foreach (PriceAndOrderViewModel item in _priceAndOrderViewModelItems)
                    {
                        if (!string.IsNullOrEmpty(item.Id))
                        {
                            int sort = SortCache.Instance.GetSettleById(item.GroupId, item.Id);
                            item.CustomSort = sort;
                        }
                    }
                    
                    CollectionView.SortDescriptions.Clear();
                    CollectionView.SortDescriptions.Add(new SortDescription("CustomSort", ListSortDirection.Ascending));
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
            lock (_lock)
            {
                List<PriceAndOrderViewModel> updateData = _priceAndOrderViewModelItems.Where((o) => { return o.PriceModel.Security == data.SecurityID; }).ToList();

                foreach (PriceAndOrderViewModel priceAndOrderViewModel in updateData)
                    priceAndOrderViewModel.OnPrice(data, false);
            }
        }


        


        #endregion


        #region 我的订单

        /// <summary>
        /// 我的订单回调
        /// </summary>
        /// <param name="obj"></param>
        private void Instance_OnOrder(ExecutionReport obj)
        {
            //UpdateMyOrders(obj);
            if (_ProcessMyOrdersThreadQueue != null)
            {
                _ProcessMyOrdersThreadQueue.Enqueue(obj);
            }
        }


        private void UpdateMyOrdersAsync(ExecutionReport obj)
        {
            lock (_lock)
            {
                List<PriceAndOrderViewModel> updateData = _priceAndOrderViewModelItems.Where((o) => { return o.PriceModel.Security == obj.SecurityID; }).ToList();

                foreach (PriceAndOrderViewModel priceAndOrderViewModel in updateData)
                    priceAndOrderViewModel.OnOrder(obj);
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
            //lock (_lock)
            {
                List<PriceAndOrderViewModel> updateData = _priceAndOrderViewModelItems.Where((o) => { return o.PriceModel.Security == obj.SecurityID; }).ToList();

                foreach (PriceAndOrderViewModel priceAndOrderViewModel in updateData)
                    priceAndOrderViewModel.OnLastExec(obj);
            }
        }

        #endregion

        public void Drop(int prevRowIndex, int index, ListView dgData)
        {
            lock (_lock)
            {
                PriceAndOrderViewModel newItem = new PriceAndOrderViewModel();

                ListCollectionView listCollectionView = CollectionView as ListCollectionView;

                PriceAndOrderViewModel old = listCollectionView.GetItemAt(prevRowIndex) as PriceAndOrderViewModel;

                ConvertObject.CopyPriceAndOrderViewModel(old, newItem);

                string removeKey = old.Id;

                PriceAndOrderViewModel removeItem = null;
                foreach (PriceAndOrderViewModel cardItem in _priceAndOrderViewModelItems)
                {
                    if (!string.IsNullOrEmpty(cardItem.Id) && cardItem.Id == removeKey)
                    {
                        removeItem = cardItem;
                    }
                }

                _priceAndOrderViewModelItems.Remove(removeItem);

                if (prevRowIndex > index)
                    newItem.CustomSort = index;
                else
                    newItem.CustomSort = index + 1;

                _priceAndOrderViewModelItems.Add(newItem);

                listCollectionView = CollectionView as ListCollectionView;

                int flag = -1;
                PriceAndOrderViewModel nullCard = null;
                for (int i = 0; i < listCollectionView.Count; i++)
                {
                    PriceAndOrderViewModel item = listCollectionView.GetItemAt(i) as PriceAndOrderViewModel;
                    if (!string.IsNullOrEmpty(item.Id))
                    {
                        flag++;
                        item.CustomSort = flag;

                        SortCache.Instance.AddOrUpdate(item.GroupId, item.Id, flag);
                    }
                    else
                        nullCard = item;
                }
                nullCard.CustomSort = decimal.MaxValue;

                PriceAndOrderViewModel nullInnerCard = null;
                foreach (PriceAndOrderViewModel item in _priceAndOrderViewModelItems)
                {
                    if (!string.IsNullOrEmpty(item.Id))
                    {
                        int sort = SortCache.Instance.GetSettleById(item.GroupId, item.Id);
                        item.CustomSort = sort;

                    }
                    else
                        nullInnerCard = item;
                }
                nullInnerCard.CustomSort = decimal.MaxValue;

                dgData.SelectedIndex = index;

                string sortStr = RememberHelper.Instance.WriteCustomSort(_currentGroupId);//WriteCustomSort();

                BondChangeHandler.Instance.OnSortChanged(OperationType.CardToTable, _currentGroupId);
            }

        }

        #endregion


        #region 过滤

        public void FilterData()
        {
            CollectionView.Filter = predicate();
        }
        
        private Predicate<object> predicate()
        {
            return new Predicate<object>(NeedShow);
        }

        private bool NeedShow(object item)
        {
            PriceAndOrderViewModel cardItem = item as PriceAndOrderViewModel;
            bool result = false;

            Dictionary<string, IList> conditionDic = MainConditionCache.Instance.GetConditionById(_currentGroupId) as Dictionary<string, IList>;

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
