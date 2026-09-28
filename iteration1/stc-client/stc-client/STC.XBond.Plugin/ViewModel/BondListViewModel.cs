using STC.Common.Data;
using STC.Common.Handler.Base;
using STC.Common.Handler.TBook;
using STC.Common.Model.Base;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.TBook;
using STC.Common.Utils;
using STC.Common.ViewModel;
using STC.XBond.Plugin.Utils;
using STC.XBond.Plugin.View;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace STC.XBond.Plugin.ViewModel
{
    /// <summary>
    /// XBond列表 BondListView的数据内容
    /// </summary>
    public class BondListViewModel : ViewModelBase
    {
        private object lockObj = new object();

        #region 表格字段
        private const string KEY_FIELD = "Key";
        private const string SECURITY_ID_FIELD = "securityID";
        private const string SYMBOL_FIELD = "symbol";
        private const string EXEC_VALUE_FIELD = "ExecValue";
        private const string OFR_PRICE_FILED = "OfrPrice";
        private const string OFR_QTY_FIELD = "OfrQty";
        private const string BEST_OFR_TRADER_FIELD = "BestOfrTrader";
        private const string BEST_BID_TRADER_FIELD = "BestBidTrader";
        private const string BID_QTY_FIELD = "BidQty";
        private const string BEST_BID_PRICE_FIELD = "BestBidPrice";
        private const string BEST_BID_QTY_FIELD = "BestBidQty";
        private const string BEST_OFR_PRICE_FIELD = "BestOfrPrice";
        private const string BEST_OFR_QTY_FIELD = "BestOfrQty";
        private const string LAST_ACTION_STATUS_FIELD = "LastActionStatus";
        private const string LAST_ACTION_TIME_FIELD = "LastActionTime";
        private const string CREATETIME_FIELD = "CreateTime";
        private const string VENUE_FIELD = "Venue";
        private const string VENUE_TYPEGW_FIELD = "VenueTypeGW";
        private const string REMAINGING_TIME_FIELD = "RemaingingTime";
        private const string REMAINGING_TIME_CONVERT_FIELD = "RemaingingTime_convert";
        private const string SWITCH_FIELD = "Switch";
        private const string BID_PRICE_FIELD = "BidPrice";
        private const string SETTLE_TYPE_FIELD = "SettleType";
        private const string RATING_CURRENT_FIELD = "RatingCurrent";
        private const string INFO_FIELD = "Info";
        private const string BID_ORDER_TIME_FIELD = "BidOrderTime";
        private const string OFR_ORDER_TIME_FIELD = "OfrOrderTime";
        private const string SECURITY_TYPE_FIELD = "SecurityType";
        #endregion

        private DataView _BondView;
        public DataView BondView
        {
            get
            {
                lock(lockObj)
                {
                    if (_BondView == null)
                    {
                        _BondView = new DataTable().DefaultView;
                        _BondView.Table.Columns.Add(KEY_FIELD, typeof(string));
                        _BondView.Table.Columns.Add(SECURITY_ID_FIELD, typeof(string));
                        _BondView.Table.Columns.Add(SYMBOL_FIELD, typeof(string));
                        _BondView.Table.Columns.Add(EXEC_VALUE_FIELD, typeof(decimal));
                        _BondView.Table.Columns.Add(OFR_PRICE_FILED, typeof(decimal));
                        _BondView.Table.Columns.Add(OFR_QTY_FIELD, typeof(long));
                        _BondView.Table.Columns.Add(BEST_OFR_TRADER_FIELD, typeof(string));
                        _BondView.Table.Columns.Add(BEST_BID_TRADER_FIELD, typeof(string));
                        _BondView.Table.Columns.Add(BID_QTY_FIELD, typeof(long));
                        _BondView.Table.Columns.Add(BEST_BID_PRICE_FIELD, typeof(decimal));
                        _BondView.Table.Columns.Add(BEST_BID_QTY_FIELD, typeof(long));
                        _BondView.Table.Columns.Add(BEST_OFR_PRICE_FIELD, typeof(decimal));
                        _BondView.Table.Columns.Add(BEST_OFR_QTY_FIELD, typeof(long));
                        _BondView.Table.Columns.Add(LAST_ACTION_STATUS_FIELD, typeof(string));
                        _BondView.Table.Columns.Add(LAST_ACTION_TIME_FIELD, typeof(string));
                        _BondView.Table.Columns.Add(CREATETIME_FIELD, typeof(string));
                        _BondView.Table.Columns.Add(VENUE_FIELD, typeof(string));
                        _BondView.Table.Columns.Add(VENUE_TYPEGW_FIELD, typeof(string));
                        _BondView.Table.Columns.Add(REMAINGING_TIME_FIELD, typeof(string));
                        _BondView.Table.Columns.Add(REMAINGING_TIME_CONVERT_FIELD, typeof(string));
                        _BondView.Table.Columns.Add(SWITCH_FIELD, typeof(bool));
                        _BondView.Table.Columns.Add(BID_PRICE_FIELD, typeof(decimal));
                        _BondView.Table.Columns.Add(SETTLE_TYPE_FIELD, typeof(string));
                        _BondView.Table.Columns.Add(RATING_CURRENT_FIELD, typeof(string));
                        _BondView.Table.Columns.Add(INFO_FIELD, typeof(string));
                        _BondView.Table.Columns.Add(BID_ORDER_TIME_FIELD, typeof(string));
                        _BondView.Table.Columns.Add(OFR_ORDER_TIME_FIELD, typeof(string));
                        _BondView.Table.Columns.Add(SECURITY_TYPE_FIELD, typeof(string));

                        _BondView.Sort = $"{CREATETIME_FIELD} desc";
                        _BondView.Table.PrimaryKey = new DataColumn[] {
                            _BondView.Table.Columns[KEY_FIELD],
                            _BondView.Table.Columns[SETTLE_TYPE_FIELD],
                            _BondView.Table.Columns[VENUE_FIELD],
                            _BondView.Table.Columns[VENUE_TYPEGW_FIELD]
                        };
                        this.OnPropertyChanged();
                    }
                    return this._BondView;
                }
            }
        }

        private ThreadQueue<Bond> _ProcessBondsThreadQueue;
        private ThreadQueue<Common.Model.Fix.Message.MarketDataSnapshotFullRefresh> _ProcessDMPriceThreadQueue;
        private ThreadQueue<Common.Model.Fix.Message.ExecutionReport> _ProcessMyOrdersThreadQueue;
      
        private string _searchText = string.Empty;
        public string SearchText
        {
            get
            {
                return _searchText;
            }

            set
           {
                if (_searchText != value)
                {
                    _searchText = value;
                    this.OnPropertyChanged();
                    FilterData();
                }
            }
        }

        StringBuilder filterSB = new StringBuilder();
        StringBuilder filterBondType = new StringBuilder();
        /// <summary>
        /// 生成债券类型过滤条件
        /// </summary>
        /// <param name="selectedItems">界面选择的过滤条件</param>
        public void FilterBondType(System.Collections.IList selectedItems)
        {
            filterBondType.Remove(0, filterBondType.Length);
            if (selectedItems != null && selectedItems.Count > 0)
            {
                StringBuilder inFilterSB = new StringBuilder();
                string notInFilter = string.Empty;

                foreach (ListBoxItem lbi in selectedItems)
                {
                    if (lbi.Content.ToString() == "国债")
                    {
                        AddListFilter(inFilterSB,"'100001','100011','100054','100053'");
                    }
                    else if (lbi.Content.ToString() == "央行票据")
                    {
                        AddListFilter(inFilterSB, "'100002'");
                    }
                    else if (lbi.Content.ToString() == "金融债")
                    {
                        AddListFilter(inFilterSB, "'100003','100055'");
                    }
                    else if (lbi.Content.ToString() == "短融")
                    {
                        AddListFilter(inFilterSB, "'100006','100057','100029','100024'");
                    }
                    else if (lbi.Content.ToString() == "中票")
                    {
                        AddListFilter(inFilterSB, "'100010'");
                    }
                    else if (lbi.Content.ToString() == "同业存单")
                    {
                        AddListFilter(inFilterSB, "'100041'");
                    }
                    else if (lbi.Content.ToString() == "企业债")
                    {
                        AddListFilter(inFilterSB, "'100004'");
                    }
                    else if (lbi.Content.ToString() == "其他")
                    {
                        notInFilter = $"{SECURITY_TYPE_FIELD} not in ('100001','100011','100054','100053','100002','100003','100055','100006', '100057', '100029', '100024','100010','100041','100004') or {SECURITY_TYPE_FIELD} is null";
                    }
                }
                if (inFilterSB.Length > 0)
                {
                    if (filterBondType.Length > 0)
                    {
                        filterBondType.Append(" or ");
                    }
                    filterBondType.Append($"{SECURITY_TYPE_FIELD} in ({inFilterSB})");
                }
                if (notInFilter.Length > 0)
                {
                    if (filterBondType.Length > 0)
                    {
                        filterBondType.Append(" or ");
                    }
                    filterBondType.Append(notInFilter);
                }
            }
            FilterData();
        }

        private void AddListFilter(StringBuilder filterSB, string listFilterItems)
        {
            if (filterSB.Length > 0)
            {
                filterSB.Append(",");
            }
            filterSB.Append(listFilterItems);
        }

        StringBuilder filterRemainingTime = new StringBuilder();
        /// <summary>
        /// 生成剩余时间过滤条件
        /// </summary>
        /// <param name="selectedItems">界面选择的过滤条件</param>
        public void FilterRemainingTime(System.Collections.IList selectedItems)
        {
            filterRemainingTime.Remove(0, filterRemainingTime.Length);
            if (selectedItems != null && selectedItems.Count > 0)
            {
                StringBuilder infilterSB = new StringBuilder();
                foreach (ListBoxItem lbi in selectedItems)
                {
                    if (lbi.Content.ToString() != "全部")
                    {
                        AddListFilter(infilterSB, $"'{lbi.Content}'");
                    }
                }
                if (infilterSB.Length > 0)
                {
                    this.filterRemainingTime.Append($"({REMAINGING_TIME_CONVERT_FIELD} in ({infilterSB}))");
                }
            }
            this.FilterData();
        }

        StringBuilder filterRating = new StringBuilder();
        public void FilterRating(System.Collections.IList selectedItems)
        {
            this.filterRating.Remove(0, this.filterRating.Length);
            if (selectedItems != null && selectedItems.Count > 0)
            {
                StringBuilder inFilterSB = new StringBuilder();
                string notInFilter = string.Empty;
                foreach (ListBoxItem lbi in selectedItems)
                {
                    switch (lbi.Content.ToString())
                    {
                        case "AAA+":
                        case "AAA":
                        case "AAA-":
                        case "AA+":
                        case "AA":
                        case "AA-":
                        case "A+":
                        case "A":
                        case "A-":
                            AddListFilter(inFilterSB, $"'{lbi.Content}'");
                            break;
                        case "其他":
                            notInFilter = $"{RATING_CURRENT_FIELD} not in ('AAA+','AAA','AAA-','AA+','AA','AA-','A+','A','A-') or {RATING_CURRENT_FIELD} is null";
                            break;
                    }
                }
                if (inFilterSB.Length > 0)
                {
                    if (filterRating.Length > 0)
                    {
                        filterRating.Append(" or ");
                    }
                    filterRating.Append($"{RATING_CURRENT_FIELD} in ({inFilterSB})");
                }
                if (notInFilter.Length > 0)
                {
                    if (filterRating.Length > 0)
                    {
                        filterRating.Append(" or ");
                    }
                    filterRating.Append(notInFilter);
                }
            }
            this.FilterData();
        }

        StringBuilder filterSettlType = new StringBuilder();
        public void FilterSettlType(System.Collections.IList selectedItems)
        {
            filterSettlType.Remove(0, filterSettlType.Length);
            if (selectedItems != null && selectedItems.Count > 0)
            {
                StringBuilder inFilterSB = new StringBuilder();
                foreach (ListBoxItem lbi in selectedItems)
                {
                    if (lbi.Content.ToString() == "T+0")
                    {
                        AddListFilter(inFilterSB, "'1'");
                    }
                    else if (lbi.Content.ToString() == "T+1")
                    {
                        AddListFilter(inFilterSB, "'2'");
                    }
                }
                if (inFilterSB.Length > 0)
                {
                    filterSettlType.Append($"{SETTLE_TYPE_FIELD} in ({inFilterSB})");
                }
            }
            this.FilterData();
        }

        private DataRowView _selectedItem;
        public DataRowView SelectedItem
        {
            get
            {
                return _selectedItem;
            }

            set
            {
                _selectedItem = value;
            }
        }

        // 开关按钮
        private ICommand OpenCloseBondCommand { get { return new RelayCommand(OpenCloseBond); } }

        // 打开深度行情按钮
        public ICommand OpenTradeBookCommand { get { return new RelayCommand(OpenTradeBook); } }

        // 债券列表
        private List<Bond> _bondList = new List<Bond>();

        /// <summary>
        /// 构造函数 
        /// </summary>
        public BondListViewModel()
        {
            // Todo: 注册清零信号, 暂时先不清零
            BaseHandler.Instance.Init(Instance_OnClear);

            BindingOperations.EnableCollectionSynchronization(this.BondView, lockObj);
        }

        private void Instance_OnClear(ClearPo obj)
        {
            LogHandler.Info("BondListViewModel Clear");
            if (obj != null && "Y".Equals(obj.IsClear))
            {
                Application.Current.Dispatcher.BeginInvoke((Action)(() => {
                    //this.Clear();
                    //this.Init();
                }));
            }
        }

        ~BondListViewModel()
        {
            this.Dispose();
        }

        new public void Dispose()
        {
            BaseHandler.Instance.UnSubscribe(Instance_OnClear);
            Clear();
            base.Dispose();
        }

        public void Clear()
        {
            if (_BondView != null)
            {
                foreach (DataRowView bond in _BondView)
                {
                    PriceHandler.Instance.UnregisterH0Price(CommonUtils.ConvertToString(bond[SECURITY_ID_FIELD]), Instance_OnMDPrice);
                }
            }

            OrdersHandler.Instance.OnOwnOrders -= Instance_OnOrder;
            TBookHandler.Instance.UnSubscribeBond(Instance_OnBondList);

            lock (this.lockObj)
            {
                if (_BondView != null)
                {
                    _BondView.Dispose();
                    _BondView = null;
                }
            }
            if (_ProcessDMPriceThreadQueue != null)
            {
                _ProcessDMPriceThreadQueue.Dispose();
                _ProcessDMPriceThreadQueue = null;
            }
            if (_ProcessMyOrdersThreadQueue != null)
            {
                _ProcessMyOrdersThreadQueue.Dispose();
                _ProcessMyOrdersThreadQueue = null;
            }
            if (_ProcessBondsThreadQueue != null)
            {
                _ProcessBondsThreadQueue.Dispose();
                _ProcessBondsThreadQueue = null;
            }
        }

        public void Init()
        {
            _ProcessMyOrdersThreadQueue = new ThreadQueue<ExecutionReport>(UpdateMyOrdersAsync);
            _ProcessDMPriceThreadQueue = new ThreadQueue<MarketDataSnapshotFullRefresh>(UpdateMDPriceAsync);
            _ProcessBondsThreadQueue = new ThreadQueue<Bond>(UpdateBondAsync);

            System.Threading.ThreadPool.QueueUserWorkItem(o =>
            {
                SortedDictionary<string, SimpleBondsInfoPo> bondsInfoPoDic = null;
                while (bondsInfoPoDic == null || bondsInfoPoDic.Count == 0)
                {
                    bondsInfoPoDic = CommonUtils.GetAllBaseBondsInfo();
                    Thread.Sleep(200);
                }
                Thread.Sleep(1000);

                //#region  创建测试数据
                //this.GenerateTestData();
                //return;
                //#endregion

                #region 收Price消息
                // --------- 收Price消息 ----------------
                // 订阅列表
                TBookHandler.Instance.Init(null, Instance_OnBondList);

                // 订阅我的订单
                OrdersHandler.Instance.OnOwnOrders += Instance_OnOrder;
                // -----------------------------------------
                #endregion

            });
        }

        /// <summary>
        /// 过滤数据
        /// </summary>
        private void FilterData()
        {
            // 初始化过滤条件
            filterSB.Remove(0, filterSB.Length);

            // 添加债券代码过滤
            #region 筛选债券代码
            if (this.SearchText.Length > 0)
            {
                this.filterSB.Append($"{SECURITY_ID_FIELD} like '%{this.SearchText}%'");
            }
            #endregion

            // 添加债券类型过滤
            #region 筛选债券类型
            if (filterBondType.Length > 0)
            {
                if (filterSB.Length > 0)
                {
                    filterSB.Append(" and ");
                }

                filterSB.Append($"({filterBondType})");
            }
            #endregion

            // 添加剩余时间过滤
            #region 筛选剩余时间
            if (filterRemainingTime.Length > 0)
            {
                if (filterSB.Length > 0)
                {
                    filterSB.Append(" and ");
                }
                filterSB.Append($"({filterRemainingTime})");
            }

            #endregion

            // 添加评级
            #region 筛选评级
            if (filterRating.Length > 0)
            {
                if (filterSB.Length > 0)
                {
                    filterSB.Append(" and ");
                }
                filterSB.Append($"({filterRating})");
            }
            #endregion

            // 添加清算速度
            #region 筛选清算速度
            if (filterSettlType.Length > 0)
            {
                if (filterSB.Length > 0)
                {
                    filterSB.Append(" and ");
                }
                filterSB.Append(filterSettlType);
            }
            #endregion

            this.BondView.RowFilter = filterSB.ToString();
        }

        private void Instance_OnBondList(List<Bond> bondList)
        {
            if (bondList.Count == 0)
            {
                return;
            }
            //// 删除
            //List<BondInfosItem> removeList = new List<BondInfosItem>();
            //foreach (BondInfosItem bondItem in this.Bonds)
            //{
            //    if (bondList.Any<Bond>(bond => (bond.securityid == bondItem.Key)) == false)
            //    {
            //        removeList.Add(bondItem);
            //        //取消订阅
            //        MDHandler.Instance.UnSubscribe(bondItem.securityID, Instance_OnCTPrice, Instance_OnMDPrice);
            //    }
            //}
            //foreach(BondInfosItem removeBondItem in removeList)
            //{
            //    this.Bonds.Remove(removeBondItem);
            //}
            // 添加和更新
            if (bondList != null && bondList.Count > 0)
            {
                foreach(Bond bondObj in bondList)
                {
                    //UpdateBond(bondObj);
                    if (_ProcessBondsThreadQueue != null)
                    {
                        _ProcessBondsThreadQueue.Enqueue(bondObj);
                    }
                }
            }
        }

        /// <summary>
        /// 我的订单回调
        /// </summary>
        /// <param name="obj"></param>
        private void Instance_OnOrder(ExecutionReport obj)
        {
            //UpdateMyOrders(obj);
            if(_ProcessMyOrdersThreadQueue != null)
            {
                _ProcessMyOrdersThreadQueue.Enqueue(obj);
            }
        }

        /// <summary>
        /// 做市报价（SOR）回调
        /// </summary>
        /// <param name="obj"></param>
        private void Instance_OnMDPrice(Common.Model.Fix.Message.MarketDataSnapshotFullRefresh obj)
        {
            //UpdateMDPrice(obj);
            if (_ProcessDMPriceThreadQueue != null)
            {
                _ProcessDMPriceThreadQueue.Enqueue(obj);
            }
        }

        private void UpdateBondAsync(Bond obj)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                UpdateBond(obj);
            }));
        }

        /// <summary>
        /// 更新债券列表
        /// </summary>
        /// <param name="bond"></param>
        private void UpdateBond(Bond obj)
        {
            try
            {
                lock(lockObj)
                {
                    DataRow bondInfosItems = this.BondView.Table.Rows.Find(new object[] { obj.securityid,obj.settltype, obj.venues, obj.venuetypegw });
                    
                    if (bondInfosItems == null)
                    {
                        // 删除该项
                        if (obj.action == "0")
                        {
                            //取消订阅
                            PriceHandler.Instance.UnregisterH0Price(obj.securityid, Instance_OnMDPrice);
                            return;
                        }

                        bondInfosItems = this.BondView.Table.NewRow();
                        // OpenClsoe
                        bondInfosItems[KEY_FIELD] = obj.securityid;
                        bondInfosItems[SETTLE_TYPE_FIELD] = obj.settltype; // 清算速度
                        bondInfosItems[SECURITY_ID_FIELD] = obj.securityid; // 代码
                        bondInfosItems[VENUE_FIELD] = obj.venues; 
                        bondInfosItems[VENUE_TYPEGW_FIELD] = obj.venuetypegw;
                        BondsInfoPo bondsInfoPo = CommonUtils.GetBondsInfoPoBySecurityID(obj.securityid);
                        if (bondsInfoPo != null)
                        {
                            bondInfosItems[SYMBOL_FIELD] = bondsInfoPo.Short_Name;// 代码名称
                            bondInfosItems[REMAINGING_TIME_FIELD] = bondsInfoPo.RemaingingTime;//剩余时间
                            bondInfosItems[RATING_CURRENT_FIELD] = bondsInfoPo.Rating_Current; // 评级
                            bondInfosItems[SECURITY_TYPE_FIELD] = bondsInfoPo.Security_Type_Id;// 债券类型 
                        }
                        bondInfosItems[REMAINGING_TIME_CONVERT_FIELD] = ConvertRemainingTime(CommonUtils.ConvertToString(bondInfosItems[REMAINGING_TIME_FIELD]));//剩余时间转换成时间段

                        this.BondView.Table.Rows.Add(bondInfosItems);

                        //订阅price
                        //MDHandler.Instance.Init(obj.securityid, Instance_OnCTPrice, Instance_OnMDPrice);
                        PriceHandler.Instance.RegisterH0Price(obj.securityid, Instance_OnMDPrice);

                        //// 更新我的订单数据
                        //ExecutionReport er = OrdersHandler.Instance.GetCacheOrders(obj.securityid, obj.settltype, obj.venues, obj.venuetypegw);
                        //if (er != null)
                        //{
                        //    UpdateMyOrders(er);
                        //}
                    }

                    if (bondInfosItems != null)
                    {

                        // 删除该项
                        if (obj.action == "0")
                        {
                            //取消订阅
                            PriceHandler.Instance.UnregisterH0Price(obj.securityid, Instance_OnMDPrice);
                            this.BondView.Table.Rows.Remove(bondInfosItems);
                            return;
                        }

                        bondInfosItems[SWITCH_FIELD] = obj.enable == "1" ? true : false; // 开关
                        bondInfosItems[LAST_ACTION_STATUS_FIELD] = obj.remark;
                        bondInfosItems[INFO_FIELD] = obj.inf1;
                        bondInfosItems[LAST_ACTION_TIME_FIELD] = obj.updatetime;
                        bondInfosItems[EXEC_VALUE_FIELD] = CommonUtils.ConvertToDecimal(obj.netqty);  // 今日净成交
                        bondInfosItems[CREATETIME_FIELD] = obj.createtime;
                    }
                    this.BondView.Table.AcceptChanges();
                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
            }
            
        }

        private void UpdateMDPriceAsync(MarketDataSnapshotFullRefresh obj)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                UpdateMDPrice(obj);
            }));
        }

        /// <summary>
        /// 更新行情
        /// </summary>
        /// <param name="obj"></param>
        private void UpdateMDPrice(MarketDataSnapshotFullRefresh obj)
        {
            try
            {
                if (obj.NoMDEntries.Count > 0)
                {
                    GNoMDEntries bidNoMdEntries = null;
                    GNoMDEntries ofrNoMdEntries = null;
                    ChooseBestPrice(obj.NoMDEntries,
                                        ref bidNoMdEntries,
                                        ref ofrNoMdEntries);

                    lock(this.lockObj)
                    {
                        DataRow[] drs = this.BondView.Table.Select($"{KEY_FIELD} = '{obj.SecurityID}'");
                        if (drs != null && drs.Length > 0)
                        {
                            foreach (DataRow item in drs)
                            {
                                if (item != null)
                                {
                                    // ofr
                                    if (ofrNoMdEntries != null)
                                    {
                                        item[BEST_OFR_TRADER_FIELD] = ofrNoMdEntries.Venues;
                                        item[BEST_OFR_PRICE_FIELD] = CommonUtils.ConvertToDecimal(ofrNoMdEntries.MDEntryPx);
                                        item[BEST_OFR_QTY_FIELD] = CommonUtils.ConvertToLong(ofrNoMdEntries.MDEntrySize);
                                    }
                                    else
                                    {
                                        item[BEST_OFR_TRADER_FIELD] = "-";
                                        item[BEST_OFR_PRICE_FIELD] = decimal.Zero;
                                        item[BEST_OFR_QTY_FIELD] = 0;
                                    }

                                    // bid
                                    if (bidNoMdEntries != null)
                                    {
                                        item[BEST_BID_TRADER_FIELD] = bidNoMdEntries.Venues;
                                        item[BEST_BID_PRICE_FIELD] = CommonUtils.ConvertToDecimal(bidNoMdEntries.MDEntryPx);
                                        item[BEST_BID_QTY_FIELD] = CommonUtils.ConvertToLong(bidNoMdEntries.MDEntrySize);
                                    }
                                    else
                                    {
                                        item[BEST_BID_TRADER_FIELD] = "-";
                                        item[BEST_BID_PRICE_FIELD] = decimal.Zero;
                                        item[BEST_BID_QTY_FIELD] = 0;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        /// <summary>
        /// 把基础剩余时间转换成时间段
        /// 如：2Y+5D -> 1~3Y
        /// </summary>
        /// <param name="remainingTime">基础剩余时间</param>
        /// <returns>筛选时间段</returns>
        private string ConvertRemainingTime(string remainingTime)
        {
            string remainingTimeLevel = string.Empty;
            decimal totalYear = decimal.Zero;
            if (string.IsNullOrEmpty(remainingTime) == false)
            {
                string[] splitArr = remainingTime.Split('+');
                foreach (string subTime in splitArr)
                {
                    int yearIndex = subTime.IndexOf("Y");
                    if (yearIndex > 0)
                    {
                        string num_Str = subTime.Substring(0, yearIndex);
                        totalYear += CommonUtils.ConvertToDecimal(num_Str);
                    }

                    break;
                }
            }

            if (totalYear <= 1)
            {
                remainingTimeLevel = "0~1Y";
            }
            else if (totalYear <= 3)
            {
                remainingTimeLevel = "1~3Y";
            }
            else if (totalYear <= 5)
            {
                remainingTimeLevel = "3~5Y";
            }
            else if (totalYear <= 7)
            {
                remainingTimeLevel = "5~7Y";
            }
            else if (totalYear <= 10)
            {
                remainingTimeLevel = "7~10Y";
            }
            else
            {
                remainingTimeLevel = ">10Y";
            }
            return remainingTimeLevel;
        }

        /// <summary>
        /// 打开tradebook界面
        /// </summary>
        public void OpenTradeBook()
        {
            if (this.SelectedItem != null)
            {
                TradeBookView view = new TradeBookView();
                QuotationListViewModel viewModel = new QuotationListViewModel();
                viewModel.SecurityID = CommonUtils.ConvertToString(this.SelectedItem[SECURITY_ID_FIELD]);
                viewModel.Symbol = CommonUtils.ConvertToString(this.SelectedItem[SYMBOL_FIELD]);
                viewModel.FullName = CommonUtils.ConvertToString(this.SelectedItem[SYMBOL_FIELD]);//TODO： 这个还没有
                view.DataContext = viewModel;

                TabPageHelper.OpenTabPage(viewModel.SecurityID, view, -1);
            }
        }

        
        private void ChooseBestPrice(List<GNoMDEntries> gNoMDEntriesList,
            ref GNoMDEntries bestBidItem,
            ref GNoMDEntries bestOfrItem)
        {
            decimal bestBidPrice = decimal.MaxValue;
            decimal bestOfrPrice = decimal.MinValue;
            if (gNoMDEntriesList != null && gNoMDEntriesList.Count > 0)
            {
                foreach (GNoMDEntries item in gNoMDEntriesList)
                {
                    // ofr
                    if (item.MDEntryType == "1")
                    {
                        if (CommonUtils.ConvertToDecimal(item.MDEntryPx) > bestOfrPrice)
                        {
                            bestOfrPrice = CommonUtils.ConvertToDecimal(item.MDEntryPx);
                        }
                    }
                    // bid
                    else if (item.MDEntryType == "0")
                    {
                        if (CommonUtils.ConvertToDecimal(item.MDEntryPx) < bestBidPrice)
                        {
                            bestBidPrice = CommonUtils.ConvertToDecimal(item.MDEntryPx);
                        }
                    }
                }

                // bid
                var bestBidList = gNoMDEntriesList.Where(item => item.MDEntryType == "0" && CommonUtils.ConvertToDecimal(item.MDEntryPx) == bestBidPrice);
                if (bestBidList != null && bestBidList.Count() > 0)
                {
                    long bestBidQty = bestBidList.Sum(item => CommonUtils.ConvertToLong(item.MDEntrySize));
                    string bestBidVenues = "-";
                    int bestBidCount = bestBidList.Count();
                    string firstBestBidVenues = bestBidList.First().Venues;
                    int xbondBestBidCount = bestBidList.Count(item => item.Venues.ToUpper() == firstBestBidVenues);
                    if (bestBidCount == 1)
                    {
                        bestBidVenues = firstBestBidVenues;
                    }
                    else if (bestBidCount > 1)
                    {
                        if (xbondBestBidCount == bestBidCount)
                        {
                            bestBidVenues = firstBestBidVenues;
                        }
                        else
                        {
                            bestBidVenues = "多家";
                        }
                    }
                    if (bestBidVenues != "-")
                    {
                        bestBidItem = new GNoMDEntries();
                        bestBidItem.Venues = bestBidVenues;
                        bestBidItem.MDEntrySize = bestBidQty.ToString();
                        bestBidItem.MDEntryPx = bestBidPrice.ToString();
                    }
                }
                

                // ofr
                var bestOfrList = gNoMDEntriesList.Where(item => item.MDEntryType == "1" && CommonUtils.ConvertToDecimal(item.MDEntryPx) == bestOfrPrice);
                if (bestOfrList != null && bestOfrList.Count() > 0)
                {
                    long bestOfrQty = bestOfrList.Sum(item => CommonUtils.ConvertToLong(item.MDEntrySize));
                    string bestOfrVenues = "-";
                    string firstBestOfrVenues = bestOfrList.First().Venues;
                    int bestOfrCount = bestOfrList.Count();
                    int xbondBestOfrCount = bestOfrList.Count(item => item.Venues.ToUpper() == firstBestOfrVenues);
                    if (bestOfrCount == 1)
                    {
                        bestOfrVenues = firstBestOfrVenues;
                    }
                    else if (bestOfrCount > 1)
                    {
                        if (xbondBestOfrCount == bestOfrCount)
                        {
                            bestOfrVenues = firstBestOfrVenues;
                        }
                        else
                        {
                            bestOfrVenues = "多家";
                        }
                    }
                    if (bestOfrVenues != "-")
                    {
                        bestOfrItem = new GNoMDEntries();
                        bestOfrItem.Venues = bestOfrVenues;
                        bestOfrItem.MDEntrySize = bestOfrQty.ToString();
                        bestOfrItem.MDEntryPx = bestOfrPrice.ToString();
                    }
                }
            }
        }

        private void UpdateMyOrdersAsync(ExecutionReport obj)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                UpdateMyOrders(obj);
            }));
        }


        /// <summary>
        /// 更新我的订单
        /// </summary>
        /// <param name="obj"></param>
        private void UpdateMyOrders(ExecutionReport obj)
        {
            try
            {
                

                lock (this.lockObj)
                {
                    
                    DataRow bondInfosItems = this.BondView.Table.Rows.Find(new object[] { obj.SecurityID, obj.SettlType, obj.Venues, obj.VenueTypeGW });

                    if (bondInfosItems != null)
                    {
                        if ((obj.Side == "1" && CommonUtils.ConvertToString(bondInfosItems[BID_ORDER_TIME_FIELD]).CompareTo(obj.TransactTime) > 0)
                                   || (obj.Side == "2" && CommonUtils.ConvertToString(bondInfosItems[OFR_ORDER_TIME_FIELD]).CompareTo(obj.TransactTime) > 0))
                        {
                            // 如果时间早， 就不更新了。
                            return;
                        }

                        // ?? TBookHandler.Instance.GetOrderBySymbol(obj.Symbol);
                        // 1: 买入 bid
                        if (obj.Side == "1")
                        {
                            bondInfosItems[BID_ORDER_TIME_FIELD] = obj.TransactTime; // 发送时间
                            if ((obj.ExecType == "0" || obj.ExecType == "1" || obj.ExecType == "6") && obj.AlgoName == "XBOND")
                            {
                                bondInfosItems[BID_PRICE_FIELD] = CommonUtils.ConvertToDecimal(obj.Price);
                                bondInfosItems[BID_QTY_FIELD] = CommonUtils.ConvertToLong(obj.OrderQty) - CommonUtils.ConvertToLong(obj.CumQty);
                            }
                            else
                            {
                                bondInfosItems[BID_PRICE_FIELD] = decimal.Zero;
                                bondInfosItems[BID_QTY_FIELD] = 0;
                            }
                        }
                        // 2: 卖出 ofr
                        else if (obj.Side == "2")
                        {
                            bondInfosItems[OFR_ORDER_TIME_FIELD] = obj.TransactTime; // 发送时间
                            if ((obj.ExecType == "0" || obj.ExecType == "1" || obj.ExecType == "6") && obj.AlgoName == "XBOND")
                            {
                                bondInfosItems[OFR_PRICE_FILED] = CommonUtils.ConvertToDecimal(obj.Price);
                                bondInfosItems[OFR_QTY_FIELD] = CommonUtils.ConvertToLong(obj.OrderQty) - CommonUtils.ConvertToLong(obj.CumQty);
                            }
                            else
                            {
                                bondInfosItems[OFR_PRICE_FILED] = decimal.Zero;
                                bondInfosItems[OFR_QTY_FIELD] = 0;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void OpenCloseBond()
        {
            //System.Threading.ThreadPool.QueueUserWorkItem(o =>
            //{
                List<BondOperationPo> bondOperationPoList = new List<BondOperationPo>();
                BondOperationPo bondOperationPo = new BondOperationPo();
                bondOperationPo.Enable = (bool)this.SelectedItem[SWITCH_FIELD] == true ? "0" : "1";
                bondOperationPo.SecurityID = CommonUtils.ConvertToString(this.SelectedItem[SECURITY_ID_FIELD]);
                bondOperationPo.SettlType = CommonUtils.ConvertToString(this.SelectedItem[SETTLE_TYPE_FIELD]);
                bondOperationPo.Symbol = CommonUtils.ConvertToString(this.SelectedItem[SYMBOL_FIELD]);
                bondOperationPoList.Add(bondOperationPo);
                PResult result = TBookHandler.Instance.OpenCloseBond(bondOperationPoList);
            //});
        }

        private Dictionary<string, bool> _cacheBondStatusDic = new Dictionary<string, bool>();
        public void PauseAndPlayBonds(bool isPause)
        {

            if (isPause)
            {
                _cacheBondStatusDic.Clear();

                foreach (DataRowView item in this.BondView)
                {
                    _cacheBondStatusDic[CommonUtils.ConvertToString(item[SECURITY_ID_FIELD])] = (bool)item[SWITCH_FIELD];
                }

                string resultMsg = string.Empty;
                bool isSuccess = StartAndStopAll("0", out resultMsg);
                if (isSuccess)
                {
                    //MessageBox.Show("暂停所有债券成功!", "X-bond做市");
                }
                else
                {
                    MessageBox.Show(string.Format("暂停所有债券代码失败, 失败原因:{0}", resultMsg), "X-bond做市");
                }
            }
            else
            {
                List<BondOperationPo> bondOperationPoList = new List<BondOperationPo>();
                foreach (DataRowView item in this.BondView)
                {
                    string securityID = CommonUtils.ConvertToString(item[SECURITY_ID_FIELD]);
                    if (_cacheBondStatusDic.ContainsKey(securityID))
                    {
                        BondOperationPo bondOperationPo = new BondOperationPo();
                        bondOperationPo.Enable = _cacheBondStatusDic[securityID] == true ? "1" : "0";
                        bondOperationPo.SecurityID = securityID;
                        bondOperationPo.SettlType = CommonUtils.ConvertToString(item[SETTLE_TYPE_FIELD]);
                        bondOperationPo.Symbol = CommonUtils.ConvertToString(item[SYMBOL_FIELD]);
                        bondOperationPoList.Add(bondOperationPo);
                    }
                }
                if (bondOperationPoList.Count > 0)
                {
                    PResult pResult = TBookHandler.Instance.OpenCloseBond(bondOperationPoList);
                    if (pResult != null)
                    {
                        LogHandler.Info(pResult.ToString());
                        if (pResult.IsSuccess())
                        {
                            //MessageBox.Show("启动所有债券成功!", "X-bond做市");
                        }
                        else
                        {
                            MessageBox.Show(string.Format("启动所有债券失败,失败原因:{0}", pResult.ToString()), "X-bond做市");
                        }
                    }
                }
            }
        }

        public void StopAll()
        {
            string message = string.Empty;
            bool isSuccess = this.StartAndStopAll("0", out message);
            if (isSuccess)
            {
                //MessageBox.Show("停止所有债券成功!", "X-bond做市");
            }
            else
            {
                MessageBox.Show(string.Format("停止所有债券代码失败, 失败原因:{0}", message), "X-bond做市");
            }
        }

        public void StartAll()
        {
            string message = string.Empty;
            bool isSuccess = this.StartAndStopAll("1", out message);
            if (isSuccess)
            {
                //MessageBox.Show("启动所有债券成功!", "X-bond做市");
            }
            else
            {
                MessageBox.Show(string.Format("启动所有债券代码失败, 失败原因:{0}", message), "X-bond做市");
            }
        }

        /// <summary>
        /// 根据enable，启动或者停止所有债券
        /// </summary>
        /// <param name="startAndStopFlag">0:stop; 1:start</param>
        private bool StartAndStopAll(string enable, out string message)
        {
            message = string.Empty;
            List<BondOperationPo> bondOperationPoList = new List<BondOperationPo>();

            foreach (DataRowView item in this.BondView)
            {
                BondOperationPo bondOperationPo = new BondOperationPo();
                bondOperationPo.Enable = enable;
                bondOperationPo.SecurityID = CommonUtils.ConvertToString(item[SECURITY_ID_FIELD]);
                bondOperationPo.SettlType = CommonUtils.ConvertToString(item[SETTLE_TYPE_FIELD]);
                bondOperationPo.Symbol = CommonUtils.ConvertToString(item[SYMBOL_FIELD]);
                bondOperationPoList.Add(bondOperationPo);
            }
            PResult result = TBookHandler.Instance.OpenCloseBond(bondOperationPoList);
            if (result != null)
            {
                message = result.ToString();
                LogHandler.Info(result.ToString());
                return result.IsSuccess();
            }
            return false;
        }
        
        #region 生成测试数据
        private void GenerateTestData()
        {
            System.Threading.ThreadPool.QueueUserWorkItem(o =>
            {
                while(true)
                { 
                // 债券列表
                Bond bond1 = new Bond();
                bond1.closeby = "kong2";
                bond1.createtime = DateTimeEx.ServerTime.ToString("hh:mm:ss");
                bond1.enable = "0";
                bond1.ip = "172.16.97.119";
                bond1.remark = "手动关闭";
                bond1.securityid = "170210.IB";
                bond1.settltype = "2";
                bond1.symbol = "17国开10";
                bond1.terminal = "";
                bond1.updatetime = DateTimeEx.ServerTime.ToString("hh:mm:ss");
                bond1.username = "";
                bond1.venues = "ODM";
                bond1.venuetypegw = "ODM";
                UpdateBond(bond1);

                Bond bond2 = new Bond();
                bond2.closeby = "kong2";
                bond2.createtime = DateTimeEx.ServerTime.ToString("hh:mm:ss");
                bond2.enable = "1";
                bond2.ip = "172.16.97.119";
                bond2.remark = "发出报价";
                bond2.securityid = "180210.IB";
                bond2.settltype = "2";
                bond2.symbol = "19信地03";
                bond2.terminal = "";
                bond2.updatetime = DateTimeEx.ServerTime.ToString("hh:mm:ss");
                bond2.username = "";
                bond2.venues = "ODM";
                bond2.venuetypegw = "ODM";
                UpdateBond(bond2);

                //18新大陆
                Bond bond3 = new Bond();
                bond3.closeby = "kong2";
                bond3.createtime = DateTimeEx.ServerTime.ToString("hh:mm:ss");
                bond3.enable = "1";
                bond3.ip = "172.16.97.119";
                bond3.remark = "双边撤单";
                bond3.securityid = "155116.SH";
                bond3.settltype = "2";
                bond3.symbol = "18新大陆";
                bond3.terminal = "";
                bond3.updatetime = DateTimeEx.ServerTime.ToString("hh:mm:ss");
                bond3.username = "";
                bond3.venues = "ODM";
                bond3.venuetypegw = "ODM";
                UpdateBond(bond3);

                this._bondList.Add(bond1);
                this._bondList.Add(bond2);
                this._bondList.Add(bond3);

                //TODO: 行情走势(CT，现无)

                // 行情数据(SOR)

                MarketDataSnapshotFullRefresh mdSnapshot = new MarketDataSnapshotFullRefresh();
                mdSnapshot.MDBookType = "0";
                mdSnapshot.MDReqID = "";
                mdSnapshot.MarketIndicator = "4";
                // ofr
                mdSnapshot.NoMDEntries = new List<GNoMDEntries>();
                GNoMDEntries gNoMdEntries = new GNoMDEntries();
                gNoMdEntries.MDEntryDate = DateTimeEx.ServerTime.ToString("yyyyMMdd"); // 报价日期（yyyyMMdd）
                gNoMdEntries.MDEntryID = "679733511237750784"; //报价编号(对应报价更新里的MDEntryID)
                gNoMdEntries.MDEntryPx = "5.25"; //价格：债券指收益率
                gNoMdEntries.NetPrice = "5.25"; //净价
                gNoMdEntries.MDEntrySize = "1000";//量
                gNoMdEntries.MDEntryTime = DateTimeEx.ServerTime.ToString("hh:mm:ss.fff");//时间
                gNoMdEntries.MDEntryType = "1";//报价类型 0：bid； 1：ofr
                gNoMdEntries.MDReqID = "R20191125160231709";//报价请求编号(对应报价更新里的MDReqid)
                gNoMdEntries.MDPriceLevel = "1";//
                gNoMdEntries.SettlType = "2";//清算类型
                gNoMdEntries.Text = "1000(*)";//
                gNoMdEntries.VenueTypeGW = "SOR";
                gNoMdEntries.Venues = "TPSC";
                gNoMdEntries.Yield = "5.25";// 收益率
                mdSnapshot.NoMDEntries.Add(gNoMdEntries);

                // bid
                GNoMDEntries gNoMdEntries1 = new GNoMDEntries();
                gNoMdEntries1.MDEntryDate = DateTimeEx.ServerTime.ToString("yyyyMMdd"); // 报价日期（yyyyMMdd）
                gNoMdEntries1.MDEntryID = "679733511237750784"; //报价编号(对应报价更新里的MDEntryID)
                gNoMdEntries1.MDEntryPx = "5.25"; //价格：债券指收益率
                gNoMdEntries1.NetPrice = "5.25"; //净价
                gNoMdEntries1.MDEntrySize = "1000";//量
                gNoMdEntries1.MDEntryTime = DateTimeEx.ServerTime.ToString("hh:mm:ss.fff");//时间
                gNoMdEntries1.MDEntryType = "0";//报价类型 0：bid； 1：ofr
                gNoMdEntries1.MDReqID = "R20191125160231709";//报价请求编号(对应报价更新里的MDReqid)
                gNoMdEntries1.MDPriceLevel = "1";//
                gNoMdEntries1.SettlType = "2";//清算类型
                gNoMdEntries1.Text = "1000(*)";//
                gNoMdEntries1.VenueTypeGW = "SOR";
                gNoMdEntries1.Venues = "TPSC";
                gNoMdEntries1.Yield = "5.25";// 收益率
                mdSnapshot.NoMDEntries.Add(gNoMdEntries1);

                mdSnapshot.SecurityID = "155116.SH";
                mdSnapshot.StandardHeader.MsgType = "W";
                mdSnapshot.VenueTypeGW = "SOR";
                mdSnapshot.Venues = "TPSC";
                UpdateMDPrice(mdSnapshot);

                // 我的订单
                Common.Model.Fix.Message.ExecutionReport er = new Common.Model.Fix.Message.ExecutionReport();
                er.AvgPx = "0"; // 成交价格
                er.ClOrdID = string.Format($"c1"); //客户订单编号
                er.CumQty = "0";// 总成交量
                er.LeavesQty = "1000"; //剩余量
                er.OrdStatus = "0"; // 订单状态 :N,0( N,0(挂单成功 ), 1(部分成交 ),2( 完全成交),4( 撤销),8( 拒绝 ),C( 过期 )
                er.OrdType = "2"; // 订单类型2limit
                er.OrderID = string.Format($"s1"); // 订单号
                er.OrderQty = "1000"; // （订单量：债券位为万 ）
                er.Price = "5.2"; //价格: 债券指收益率
                er.Side = "1"; // 方向 1:买入 2：卖
                er.StandardHeader.MsgType = "8";
                er.SecurityID = "155116.SH"; // 代码
                er.Symbol = "155116.SH"; // 简称
                er.TimeInForce = "0"; //有效时间 DAY("0"),IOC("3")
                er.VenueTypeGW = "SOR"; //渠道
                er.Venues = "TPSC"; // 经纪商
                UpdateMyOrders(er);

                Common.Model.Fix.Message.ExecutionReport er1 = new Common.Model.Fix.Message.ExecutionReport();
                er1.AvgPx = "0"; // 成交价格
                er1.ClOrdID = string.Format($"c1"); //客户订单编号
                er1.CumQty = "0";// 总成交量
                er1.LeavesQty = "1000"; //剩余量
                er1.OrdStatus = "0"; // 订单状态 :N,0( N,0(挂单成功 ), 1(部分成交 ),2( 完全成交),4( 撤销),8( 拒绝 ),C( 过期 )
                er1.OrdType = "2"; // 订单类型2limit
                er1.OrderID = string.Format($"s1"); // 订单号
                er1.OrderQty = "1000"; // （订单量：债券位为万 ）
                er1.Price = "5.2"; //价格: 债券指收益率
                er1.Side = "2"; // 方向 1:买入 2：卖
                er1.StandardHeader.MsgType = "8";
                er1.SecurityID = "155116.SH"; // 代码
                er1.Symbol = "155116.SH"; // 简称
                er1.TimeInForce = "0"; //有效时间 DAY("0"),IOC("3")
                er1.VenueTypeGW = "SOR"; //渠道
                er1.Venues = "TPSC"; // 经纪商
                UpdateMyOrders(er1);
                    Thread.Sleep(1000);
                }
            });
        }


        #endregion

    }
}
