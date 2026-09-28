using com.gateway.log;
using Dragablz;
using BDB.Trade.Plugin.Cache;
using BDB.Trade.Plugin.Handler;
using BDB.Trade.Plugin.Handler.DataProcesser;
using BDB.Trade.Plugin.Model;
using BDB.Trade.Plugin.View.Trade;
using BDB.Trade.Plugin.ViewModel.Data;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

using STC.Common.Model;
using STC.Common.Model.Admin;
using STC.Common.Model.Fix.Message;
using STC.Common.Utils;
using HomePageDataHandler = BDB.Trade.Plugin.Handler.HomePageDataHandler;
using STC.Common.Model.Base;
using STC.Common.Model.TBook;
using STC.Common.Api;
using Newtonsoft.Json;
using STC.Common.View.Component;
using STC.Common.Handler.MarketExec;
using STC.Common.Handler.Base;
using STC.Common.Data;
using STC.Common.ViewModel.Data;
using ExecutionsHandler = BDB.Trade.Plugin.Handler.ExecutionsHandler;
using EmsOrdersHandler = BDB.Trade.Plugin.Handler.EmsOrdersHandler;
using BDB.Common.Model.OHLC;
using STC.Common.Model.MM;
using System.IO;
using STC.Frame.Utils.ExcelHelper;

namespace BDB.Trade.Plugin.ViewModel.Trade
{
    public class PriceTableViewModel : BaseViewModel
    {
        private object lockObj = new object();
        
        #region 表格字段

        public const string Best_Bid_CN = "最优Bid";
        public const string Best_Ofr_CN = "最优Ofr";

        public const string KEY_FIELD = "Key";
        public const string CUSTOMER_SORT_FIELD = "CustomerSort";
        public const string SECURITY_DB_ID_FIELD = "SecurityDbID";
        public const string SECURITY_ID_FIELD = "SecurityID";
        public const string SETTLE_TYPE_FIELD = "SettleType";
        public const string SYMBOL_FIELD = "Symbol";
        public const string REMAING_TIME_FIELD = "RemaingingTime";
        public const string REMAINGING_TIME_CONVERT_FIELD = "RemaingingTimeConvert";
        public const string BOND_TYPE_FILED = "BondType";
        public const string BOND_SUB_TYPE_FIELD = "BondSubType";
        public const string GOODS_TYPE_FIELD = "GoodsType";

        public const string SETTLE_ROW_FIELD = "SettleRow";

        public const string ENT_COR_FIELD = "Ent_Cor";
        public const string BOND_CATEGORY_FIELD = "bond_category";

        public const string MY_BID_ORDERID_FIELD = "MyBidOrderId";
        public const string MY_BID_FIELD = "MyBid";
        public const string MY_BID_QTY_FIELD = "MyBidQty";
        public const string MY_BID_LIMIT_TIME_FIELD = "MyBidLimitTime";
        public const string MY_BID_LIMIT_TIME_FLAG_FIELD = "MyBidLimitTimeFlag";
        public const string BID_BEST_SETTLE_FIELD = "BidBestSettle";
        public const string BID_BEST_SETTLE_FWD_FIELD = "BidBestSettleFwd";
        public const string BID_BEST_FLAG_TYPE_FIELD = "BidBestFlagType";//Bid最优价角标
        public const string BID_BEST_BG_FIELD = "BidBestBg";
        public const string BID_BEST_FIELD = "BidBest";//Bid最优价
        public const string BID_BEST_SORT_FIELD = "BidBestSort";//Bid最优价排序
        public const string BID_BEST_QTY_FIELD = "BidBestQty";
        public const string BID_BEST_FWD_FLAG_FIELD = "BidBestFwdFlag";
        public const string BID_BEST_ARROW_FIELD = "BidBestArrow";

        public const string OFR_BEST_FIELD = "OfrBest";//Ofr最优价
        public const string OFR_BEST_SORT_FIELD = "OfrBestSort";//Ofr最优价排序
        public const string OFR_BEST_QTY_FIELD = "OfrBestQty";
        public const string OFR_BEST_BG_FIELD = "OfrBestBg";
        public const string OFR_BEST_FWD_FLAG_FIELD = "OfrBestFwdFlag";
        public const string OFR_BEST_ARROW_FIELD = "OfrBestArrow";
        public const string OFR_BEST_FLAG_TYPE_FIELD = "OfrBestFlagType";//Ofr最优价角标
        public const string OFR_BEST_SETTLE_FIELD = "OfrBestSettle";
        public const string OFR_BEST_SETTLE_FWD_FIELD = "OfrBestSettleFwd";
        public const string MY_OFR_LIMIT_TIME_FIELD = "MyOfrLimitTime";

        public const string MY_OFR_ORDERID_FIELD = "MyOfrOrderId";
        public const string MY_OFR_FIELD = "MyOfr";
        public const string MY_OFR_QTY_FIELD = "MyOfrQty";

        public const string LAST_EXEC_PRICE_SIDE_FIELD = "LastExecPriceSide";//最新成交涨跌方向, 1:up 2:down

        public const string LAST_EXEC_PRICE_SIDE_FLAG_FIELD = "LastExecPriceSideFlag";//上一笔成交方向
        public const string LAST_EXEC_PRICE_FIELD = "LastExecPrice";
        public const string LAST_EXEC_PRICE_DISPLAY_FIELD = "LastExecPriceDisplay";
        public const string LAST_EXEC_SOURCE_FIELD = "Source";
        public const string LAST_EXEC_TIME_FIELD = "LastExecTime";
        public const string EXEC_AMOUNT_FIELD = "ExecAmount";
        public const string CREATETIME_FIELD = "CreateTime";
        public const string LAST_ACTION_TIME_FIELD = "LastActionTime";
        public const string VALUATION_FIELD = "Valuation";

        private const string IS_BID_SAME_FIELD = "IsBidSame";
        private const string IS_OFR_SAME_FIELD = "IsOfrSame";

        private const string LAST_EXEC_PRICE_BP_FIELD = "LastExecPriceBP";
        public const string LAST_EXEC_PRICE_DISPLAY_SIDE_FIELD = "LastExecPriceDisplaySide";
        public const string LAST_EXEC_PRICE_DISPLAY_SIDE_BRUSH_FIELD = "LastExecPriceDisplaySideBrush";
        public const string LAST_EXEC_PRICE_TKN_GVN_BRUSH_FIELD = "LastExecPriceTknGvnBrush";
        public const string EXEC_ORDER_NUM_FILED = "ExecOrderNum";
        //是否倒挂
        public const string UPSIDE_FILED = "UpSide";

        public const string IS_SHOW_SETTLE_FILED = "IsShowSettle";

        public const string MARKET_DOWNUP_BRUSH_FILED = "MarketDownUpBrush";
        public const string MARKET_DOWNUP_FIELD = "MarketDownUp";
        public const string RECTANGLE_ONE_FIELD = "RectangleOne";
        public const string RECTANGLE_TWO_FIELD = "RectangleTwo";
        public const string RECTANGLE_THREE_FIELD = "RectangleThree";
        public const string RECTANGLE_FOUR_FIELD = "RectangleFour";
        public const string RECTANGLE_FIVE_FIELD = "RectangleFive";

        public const string MATURITY_DATE_FIELD = "MaturityDate";
        public const string OUTSTANDING_AMOUNT_FIELD = "OutstandingAmount";

        #endregion

        #region 拖拽

        private string _dragTxt = string.Empty;

        public string DragTxt { get { return _dragTxt; } set { _dragTxt = value;  this.OnPropertyChanged(); } }

        private DataRowView _selectedDataRow;

        public DataRowView SelectedDataRow
        {
            get { return _selectedDataRow; }
            set
            {
                _selectedDataRow = value;

                DragTxt = string.Format("{0} {1} {2}", _selectedDataRow[SECURITY_ID_FIELD], _selectedDataRow[SETTLE_TYPE_FIELD], _selectedDataRow[SYMBOL_FIELD]);
            }
        }

        #endregion

        private DataView _priceTableView;
        public DataView PriceTableView
        {
            get
            {
                lock (lockObj)
                {
                    if (_priceTableView == null)
                    {
                        _priceTableView = new DataTable().DefaultView;
                        _priceTableView.Table.Columns.Add(KEY_FIELD, typeof(string));
                        _priceTableView.Table.Columns.Add(CUSTOMER_SORT_FIELD, typeof(decimal));
                        _priceTableView.Table.Columns.Add(SECURITY_ID_FIELD, typeof(string));

                        _priceTableView.Table.Columns.Add(SECURITY_DB_ID_FIELD, typeof(string));

                        _priceTableView.Table.Columns.Add(SETTLE_TYPE_FIELD, typeof(string));

                        _priceTableView.Table.Columns.Add(SETTLE_ROW_FIELD, typeof(string));

                        _priceTableView.Table.Columns.Add(SYMBOL_FIELD, typeof(string));
                        _priceTableView.Table.Columns.Add(REMAING_TIME_FIELD, typeof(string));
                        _priceTableView.Table.Columns.Add(REMAINGING_TIME_CONVERT_FIELD, typeof(decimal));
                        _priceTableView.Table.Columns.Add(BOND_TYPE_FILED, typeof(string));

                        _priceTableView.Table.Columns.Add(BOND_SUB_TYPE_FIELD, typeof(string));
                        _priceTableView.Table.Columns.Add(GOODS_TYPE_FIELD, typeof(string));

                        _priceTableView.Table.Columns.Add(ENT_COR_FIELD, typeof(string));
                        _priceTableView.Table.Columns.Add(BOND_CATEGORY_FIELD, typeof(string));

                        _priceTableView.Table.Columns.Add(MY_BID_ORDERID_FIELD, typeof(string));
                        _priceTableView.Table.Columns.Add(MY_BID_FIELD, typeof(string));
                        _priceTableView.Table.Columns.Add(MY_BID_QTY_FIELD, typeof(string));
                        _priceTableView.Table.Columns.Add(MY_BID_LIMIT_TIME_FIELD, typeof(string));
                        _priceTableView.Table.Columns.Add(BID_BEST_FLAG_TYPE_FIELD, typeof(string));
                        _priceTableView.Table.Columns.Add(MY_BID_LIMIT_TIME_FLAG_FIELD, typeof(string));
                        _priceTableView.Table.Columns.Add(BID_BEST_SETTLE_FIELD, typeof(string));
                        _priceTableView.Table.Columns.Add(BID_BEST_SETTLE_FWD_FIELD, typeof(string));
                        _priceTableView.Table.Columns.Add(BID_BEST_BG_FIELD, typeof(string));
                        _priceTableView.Table.Columns.Add(BID_BEST_FIELD, typeof(string));
                        _priceTableView.Table.Columns.Add(BID_BEST_SORT_FIELD, typeof(decimal));
                        _priceTableView.Table.Columns.Add(BID_BEST_QTY_FIELD, typeof(string));
                        _priceTableView.Table.Columns.Add(BID_BEST_FWD_FLAG_FIELD, typeof(string));
                        _priceTableView.Table.Columns.Add(BID_BEST_ARROW_FIELD, typeof(string));

                        _priceTableView.Table.Columns.Add(OFR_BEST_FIELD, typeof(string));
                        _priceTableView.Table.Columns.Add(OFR_BEST_SORT_FIELD, typeof(decimal));
                        _priceTableView.Table.Columns.Add(OFR_BEST_QTY_FIELD, typeof(string));
                        _priceTableView.Table.Columns.Add(OFR_BEST_BG_FIELD, typeof(string));
                        _priceTableView.Table.Columns.Add(OFR_BEST_FWD_FLAG_FIELD, typeof(string));
                        _priceTableView.Table.Columns.Add(OFR_BEST_ARROW_FIELD, typeof(string));
                        _priceTableView.Table.Columns.Add(OFR_BEST_FLAG_TYPE_FIELD, typeof(string));
                        _priceTableView.Table.Columns.Add(OFR_BEST_SETTLE_FIELD, typeof(string));
                        _priceTableView.Table.Columns.Add(OFR_BEST_SETTLE_FWD_FIELD, typeof(string));
                        _priceTableView.Table.Columns.Add(MY_OFR_LIMIT_TIME_FIELD, typeof(string));

                        _priceTableView.Table.Columns.Add(MY_OFR_ORDERID_FIELD, typeof(string));
                        _priceTableView.Table.Columns.Add(MY_OFR_FIELD, typeof(string));
                        _priceTableView.Table.Columns.Add(MY_OFR_QTY_FIELD, typeof(string));
                        _priceTableView.Table.Columns.Add(LAST_EXEC_PRICE_FIELD, typeof(decimal));
                        _priceTableView.Table.Columns.Add(LAST_EXEC_SOURCE_FIELD, typeof(string));

                        _priceTableView.Table.Columns.Add(LAST_EXEC_PRICE_DISPLAY_FIELD, typeof(string));
                        _priceTableView.Table.Columns.Add(LAST_EXEC_PRICE_SIDE_FIELD, typeof(string));
                        _priceTableView.Table.Columns.Add(LAST_EXEC_PRICE_SIDE_FLAG_FIELD, typeof(string));
                        _priceTableView.Table.Columns.Add(EXEC_AMOUNT_FIELD, typeof(string));
                        _priceTableView.Table.Columns.Add(CREATETIME_FIELD, typeof(string));
                        _priceTableView.Table.Columns.Add(LAST_ACTION_TIME_FIELD, typeof(string));
                        _priceTableView.Table.Columns.Add(LAST_EXEC_TIME_FIELD, typeof(string));

                        _priceTableView.Table.Columns.Add(IS_BID_SAME_FIELD, typeof(string));
                        _priceTableView.Table.Columns.Add(IS_OFR_SAME_FIELD, typeof(string));

                        _priceTableView.Table.Columns.Add(LAST_EXEC_PRICE_BP_FIELD, typeof(string));
                        _priceTableView.Table.Columns.Add(LAST_EXEC_PRICE_DISPLAY_SIDE_BRUSH_FIELD, typeof(string));
                        _priceTableView.Table.Columns.Add(LAST_EXEC_PRICE_DISPLAY_SIDE_FIELD, typeof(string));

                        _priceTableView.Table.Columns.Add(EXEC_ORDER_NUM_FILED, typeof(int));
                        _priceTableView.Table.Columns.Add(LAST_EXEC_PRICE_TKN_GVN_BRUSH_FIELD, typeof(string)); 

                        _priceTableView.Table.Columns.Add(UPSIDE_FILED, typeof(string));
                        _priceTableView.Table.Columns.Add(IS_SHOW_SETTLE_FILED, typeof(string));

                        _priceTableView.Table.Columns.Add(MARKET_DOWNUP_BRUSH_FILED, typeof(string));
                        _priceTableView.Table.Columns.Add(MARKET_DOWNUP_FIELD, typeof(string));
                        _priceTableView.Table.Columns.Add(RECTANGLE_ONE_FIELD, typeof(string));
                        _priceTableView.Table.Columns.Add(RECTANGLE_TWO_FIELD, typeof(string));
                        _priceTableView.Table.Columns.Add(RECTANGLE_THREE_FIELD, typeof(string));
                        _priceTableView.Table.Columns.Add(RECTANGLE_FOUR_FIELD, typeof(string));
                        _priceTableView.Table.Columns.Add(RECTANGLE_FIVE_FIELD, typeof(string));
                        _priceTableView.Table.Columns.Add(VALUATION_FIELD, typeof(string));

                        _priceTableView.Table.Columns.Add(OUTSTANDING_AMOUNT_FIELD, typeof(decimal));
                        _priceTableView.Table.Columns.Add(MATURITY_DATE_FIELD, typeof(string));

                        _priceTableView.Sort = $"{CUSTOMER_SORT_FIELD} asc";
                        _priceTableView.Table.PrimaryKey = new DataColumn[] {
                            _priceTableView.Table.Columns[KEY_FIELD],
                        };
                        this.OnPropertyChanged();
                    }
                    return this._priceTableView;
                }
            }
        }


        private DataRowView _selectedItem_Symbol;

        public DataRowView SelectedItem_Symbol
        {
            get
            {
                return _selectedItem_Symbol;
            }
            set
            {
                _selectedItem_Symbol = value;
                this.OnPropertyChanged();
            }
        }

        #region 关联字段

        private string _selectRowSettle;
        public string SelectRowSettle
        {
            get { return _selectRowSettle; }

            set
            {
                _selectRowSettle = value;
            }
        }

        #endregion


        private string _groupId;
        public string GroupID { get { return _groupId; } set { _groupId = value; } }

        private AsyncThreadQueue<HomeGroupSecurityIDsInfo> _ProcessBondsThreadQueue;

        private AsyncThreadQueue<MarketDataSnapshotFullRefresh> _ProcessCtPriceThreadQueue;
        private AsyncThreadQueue<ExecutionReport> _ProcessMyOrdersThreadQueue;
        private AsyncThreadQueue<ExecutionReport> _ProcessLastExecutionsThreadQueue;
        private AsyncThreadQueue<OhlcMsg> _ohlcThreadQueue;
        private string _rowFilterStr = string.Empty;
        private Dictionary<string, IList> _currentFilterDic = null;
        private FrameworkElement _ownUI = null;
        private int _initBondNum = -1;
        
        private Dictionary<string, string> _rowSettleFilterDic = new Dictionary<string, string>();
        
        private string _searchText = string.Empty;
        public string SearchText
        {
            get { return _searchText; }
            set
            {
                _searchText = value;

                this.ProcessMyOrdersFilterCondition(_currentFilterDic);
            }
        }

        //更新债券信息
        private Thread _RefreshBaseDataThread = null;
        //更新行情数据
        private Thread _RefreshDataThread = null;

        public System.Windows.Input.ICommand DeleteCommand { get; set; }

        ~PriceTableViewModel()
        {
            this.Dispose();
        }

        new public void Dispose()
        {
            Clear(false);
            if (_RefreshBaseDataThread != null)
            {
                _RefreshBaseDataThread.Abort();
                _RefreshBaseDataThread = null;
            }

            if (_RefreshDataThread != null)
            {
                LogHelper.Info("QuotationListViewModel Dispose RefreshDataThread:");
                _RefreshDataThread.Abort();
                _RefreshDataThread = null;
            }

            // 无需清空表格
            //lock (this.lockObj)
            //{
            //    if (PriceTableView != null)
            //    {
            //        PriceTableView.Table.Clear();
            //        PriceTableView.Dispose();
            //        _priceTableView = null;
            //    }
            //}

            if (_ProcessMyOrdersThreadQueue != null)
            {
                _ProcessMyOrdersThreadQueue.Clear();
                _ProcessMyOrdersThreadQueue.Dispose();
                _ProcessMyOrdersThreadQueue = null;
            }

            if (_ProcessCtPriceThreadQueue != null)
            {
                _ProcessCtPriceThreadQueue.Clear();
                _ProcessCtPriceThreadQueue.Dispose();
                _ProcessCtPriceThreadQueue = null;
            }

            if (_ProcessBondsThreadQueue != null)
            {
                _ProcessBondsThreadQueue.Clear();
                _ProcessBondsThreadQueue.Dispose();
                _ProcessBondsThreadQueue = null; 
            }
            if (_ProcessLastExecutionsThreadQueue != null)
            {
                _ProcessLastExecutionsThreadQueue.Clear();
                _ProcessLastExecutionsThreadQueue.Dispose();
                _ProcessLastExecutionsThreadQueue = null;
            }
            if (_ohlcThreadQueue != null)
            {
                _ohlcThreadQueue.Clear();
                _ohlcThreadQueue.Dispose();
                _ohlcThreadQueue = null;
            }
            base.Dispose();
        }


        public void Clear(bool isClear)
        {
            _initBondNum = -1;
            _isSelected = false;

            DesignHandler.Instance.OnSettingChange -= Instance_OnSettingChange;
            HomePageDataHandler.Instance.OnFavoritesChanged -= Instance_OnFavoritesChanged;
            LastExecutionProcesser.Processer.OnLastExecutions -= Processer_OnLastExecutions;

            BondChangeHandler.Instance.OnOneSettleChange -= Instance_OnOneSettleChange;
            BondChangeHandler.Instance.OnSortChange -= Instance_OnSortChange;

            foreach (DataRowView bond in _priceTableView)
            {
                string securityID = CommonUtils.ConvertToString(bond[SECURITY_ID_FIELD]);
                //PriceHandler.Instance.UnregisterH0Price(CommonUtils.ConvertToString(bond[SECURITY_ID_FIELD]), Instance_OnMDPrice);
                PriceHandler.Instance.UnregisterCTPrice(securityID, Instance_OnMDPrice);

                OhlcHandler.Instance.UnSubscribeOhlc(securityID, OnOHLC);
                //EmsOrdersHandler.Instance.UnregistEMSOrderBySecurityID(securityID, EmsOrderProcesser.Processer.Instance_OnEmsOrders);
            }

            MyOrderProcesser.Processer.OnOwnOrders -= Instance_OnOrder;

            lock (this.lockObj)
            {
                if (_priceTableView != null && !isClear)
                {
                    _priceTableView.Table.Rows.Clear();
                }
            }

            if (_ProcessMyOrdersThreadQueue != null)
            {
                _ProcessMyOrdersThreadQueue.Clear();
            }

            if (_ProcessCtPriceThreadQueue != null)
            {
                _ProcessCtPriceThreadQueue.Clear();
            }

            if (_ProcessBondsThreadQueue != null)
            {
                _ProcessBondsThreadQueue.Clear(); 
            }
            if (_ProcessLastExecutionsThreadQueue != null)
            {
                _ProcessLastExecutionsThreadQueue.Clear();
            }
            if (_ohlcThreadQueue != null)
            {
                _ohlcThreadQueue.Clear();
            }
        }

        private void Instance_OnClear(ClearPo obj)
        {
            LogHelper.Info(string.Format("PriceTableViewModel Clear, GroupId:{0}", this.GroupID));
            if (obj != null && "Y".Equals(obj.IsClear))
            {
                try
                {
                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        lock (this.lockObj)
                        {
                            ExecutionReport clearMsg = new ExecutionReport();
                            clearMsg.SecurityID = CommonConstants.NO_PRICE;

                            foreach (DataRow item in this.PriceTableView.Table.Rows)
                            {
                                string securityID = item[SECURITY_ID_FIELD] as string;
                                //    // 中债估值
                                item[VALUATION_FIELD] = CommonConstants.NO_PRICE;
                                //}

                                // 更新最优价
                                BindBestPriceToUI(item, null, false);

                                // 更新最优价是否和我的订单一样标志，同时更新角标,MyBid/MyOfr
                                //BindIsSameTagToUI(CommonConstants.NO_PRICE, item);

                                //更新最新成交
                                UpdateLastMarketExecutions(clearMsg, item);

                                // 更新成交笔数 
                                item[EXEC_ORDER_NUM_FILED] = 0;
                            }


                            Clear(true);

                            Init(_ownUI, false);


                            List<string> securityIdList = new List<string>();
                            
                            foreach (DataRow item in this.PriceTableView.Table.Rows)
                            {
                                string securityID = item[SECURITY_ID_FIELD] as string;

                                if (!securityIdList.Contains(securityID))
                                {
                                    securityIdList.Add(securityID);
                                }
                            }
                            //增加延迟订阅，给退订完成留时间
                            ResetSub(securityIdList);

                            //foreach (DataRow item in this.PriceTableView.Table.Rows)
                            //{
                            //    string securityID = item[SECURITY_ID_FIELD] as string;

                            //    PriceHandler.Instance.RegisterCTPrice(securityID, Instance_OnMDPrice);

                            //    RequestMarketExecutions(securityID);
                            //}
                        }
                    }));
                }
                catch (Exception e)
                {
                    LogHelper.Error("PriceTableViewModel clear exception:" + e.Message);
                }
            }
        }

        private void ResetSub(List<string> securityIdList)
        {
            ThreadPool.QueueUserWorkItem(o =>
            {
                try
                {
                    Thread.Sleep(2000);

                    foreach (string securityID in securityIdList)
                    {
                        Thread.Sleep(100);
                        PriceHandler.Instance.RegisterCTPrice(securityID, Instance_OnMDPrice);

                        #region 订阅OHLC

                        OhlcHandler.Instance.SubscribeOhlc(securityID, OnOHLC);

                        #endregion

                        RequestMarketExecutions(securityID);
                    }
                }
                catch (Exception e)
                {
                    LogHelper.Error("ResetSub", e);
                }
            });
        }

        public void Init(FrameworkElement ownUI, bool needSyncBondData)
        {
            _ownUI = ownUI;

            bool result = !string.IsNullOrEmpty(LocalEnv.UserInfo.IdcTradePermissions) && LocalEnv.UserInfo.IdcTradePermissions == "1";

            DataGrid dgDataGrid = _ownUI.FindName("dgData") as DataGrid;
            foreach (DataGridColumn gridColumn in dgDataGrid.Columns)
            {
                if (gridColumn.Header != null && (gridColumn.Header.ToString() == "MyOfr" || gridColumn.Header.ToString() == "MyBid"))
                {
                    gridColumn.Visibility = result ? Visibility.Visible : Visibility.Collapsed;
                }
            }

            BaseHandler.Instance.Init(Instance_OnClear);

            BondChangeHandler.Instance.OnOneSettleChange += Instance_OnOneSettleChange;
            BondChangeHandler.Instance.OnSortChange += Instance_OnSortChange;

            BindingOperations.EnableCollectionSynchronization(this.PriceTableView, lockObj);
            DesignHandler.Instance.OnSettingChange += Instance_OnSettingChange;
            HomePageDataHandler.Instance.OnFavoritesChanged += Instance_OnFavoritesChanged;
            if (_RefreshDataThread == null)
            {
                _RefreshDataThread = new Thread(RefreshMDData);
                _RefreshDataThread.IsBackground = true;
                _RefreshDataThread.Start();
            }


            //// 注册基础数据
            //BaseHandler.Instance.SubscribeLoadDataStatus(onloadData);
            if (_RefreshBaseDataThread == null)
            {
                _RefreshBaseDataThread = new Thread(RefreshBaseData);
                _RefreshBaseDataThread.IsBackground = true;
                _RefreshBaseDataThread.Start();
            }


            if (_ProcessBondsThreadQueue == null)
            {
                _ProcessBondsThreadQueue = new AsyncThreadQueue<HomeGroupSecurityIDsInfo>(UpdateBondAsync);
            }

            if (_ProcessMyOrdersThreadQueue == null)
            {
                _ProcessMyOrdersThreadQueue = new AsyncThreadQueue<ExecutionReport>(UpdateMyOrdersAsync);
            }

            if (_ProcessCtPriceThreadQueue == null) 
            {
                _ProcessCtPriceThreadQueue = new AsyncThreadQueue<MarketDataSnapshotFullRefresh>(UpdateMDData);
            }

            if (_ProcessLastExecutionsThreadQueue == null)
            {
                _ProcessLastExecutionsThreadQueue = new AsyncThreadQueue<ExecutionReport>(UpdateLastExecutionsAsync);
            }

            if (_ohlcThreadQueue == null)
            {
                _ohlcThreadQueue = new AsyncThreadQueue<OhlcMsg>(UpdateOHLC);
            }

            // 订阅我的订单
            MyOrderProcesser.Processer.OnOwnOrders += Instance_OnOrder;
            LastExecutionProcesser.Processer.OnLastExecutions += Processer_OnLastExecutions;
            
            this.DeleteCommand = new STC.Common.Command.DelegateCommand(new Action<object>(OnDeleteCommand));
            if (needSyncBondData)
                SyncBonds(false);

        }



        private void Instance_OnOneSettleChange(OperationType operationType, string groupId, string id, List<string> settle)
        {
            if (operationType == OperationType.CardToTable && groupId == this.GroupID && settle != null && settle.Count > 0)
            {
                lock (lockObj)
                {
                    DataRow dr = PriceTableView.Table.Rows.Find(id);
                    if (dr != null)
                    {
                        string oldSettle = dr[SETTLE_TYPE_FIELD] as string;

                        if (oldSettle == settle[0])
                            return;

                        IsClickRowSettle = true;

                        _isChangeMainSettle = true;
                        this.OnRowSettleChange(dr, settle[0]);
                        _isChangeMainSettle = false;

                        //不管从card/table触发修改单个settle，都从这里记录
                        foreach (DataRow dataRow in this.PriceTableView.Table.Rows)
                        {
                            string temSettle = dataRow[SETTLE_TYPE_FIELD] as string;
                            string temId = dataRow[KEY_FIELD] as string;

                            SettleCache.Instance.AddOrUpdate(this.GroupID, temId, temSettle);
                        }
                    }
                }
            }
        }

        private bool _isSelected = false;

        private void SetDefaultSelected()
        {
            if (_initExecInfo && !_isSelected && _initBondNum > 0 && PriceTableView.Table.Rows.Count == _initBondNum)
            {
                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    object obj = _ownUI.FindName("dgData");
                    if (obj != null)
                    {
                        DataGrid dg = obj as DataGrid;

                        TabablzControl tabControl = CommonUtils.GetParentObject<TabablzControl>(dg);
                        TableAndCardView tableAndCardView = CommonUtils.GetParentObject<TableAndCardView>(dg);
                        TabItem tabItem = null;
                        if (tableAndCardView != null)
                            tabItem = tableAndCardView.Parent as TabItem;

                        if (tabControl != null && tabItem != null)
                        {
                            TabItem selectItem = tabControl.SelectedItem as TabItem;
                            if (selectItem == tabItem)
                            {
                                if (dg != null)
                                {
                                    dg.SelectedIndex = 0;


                                }
                            }

                            _isSelected = true;
                        }

                    }
                }));
            }
        }


        private void UpdateMDData(MarketDataSnapshotFullRefresh data)
        {
            lock (this.lockObj)
            {
                try
                {
                    string securityId = data.SecurityID;


                    CalBestPrice(securityId, true);

                    UpdateExecNum(data);
                }
                catch (Exception ex)
                {
                    LogHelper.Error(ex);
                }
            }
        }

        private void UpdateExecNum(MarketDataSnapshotFullRefresh data)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                lock (this.lockObj)
                {
                    string securityID = data.SecurityID;
                    DataRow[] drs = null;
                    if (!string.IsNullOrEmpty(securityID) && CommonUtils.IsFutureBond(securityID))
                    {
                        drs = this.PriceTableView.Table.Select($"{SECURITY_ID_FIELD} = '{securityID}'");

                        if (drs != null && drs.Length > 0)
                        {
                            foreach (DataRow item in drs)
                            {
                                item[EXEC_ORDER_NUM_FILED] = CommonUtils.ConvertToInt(data.Volume);
                            }
                        }
                    }
                }
            }));
        }

        private void RefreshMDData()
        {
            while (true)
            {
                try
                {
                    SetExecInfo();

                    SetDefaultSelected();

                    Thread.Sleep(200);
                }
                catch (ThreadAbortException ex)
                {
                    Thread.Sleep(200);
                }
                catch (Exception ex)
                {
                    LogHelper.Error(ex);
                }
            }
        }

        private bool _initExecInfo = false;
        private void SetExecInfo()
        {
            if (!_initExecInfo && ExecutionsHandler.Instance.EndInit() && _initBondNum > 0 && PriceTableView.Table.Rows.Count == _initBondNum)
            {
                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    lock (lockObj)
                    {
                        foreach (DataRow dr in PriceTableView.Table.Rows)
                        {
                            string security = dr[SECURITY_ID_FIELD] as string;

                            ExecutionReport temOrder = GetFirstExecutionReport(security);

                            if (temOrder != null)
                            {
                                UpdateLastMarketExecutions(temOrder, dr);
                            }
                        }
                    }
                }));

                _initExecInfo = true;
            }
        }


        #region 刷新Bond信息

        private ConcurrentDictionary<string, object> bondWithOutInfoList = new ConcurrentDictionary<string, object>();

        private void RefreshBaseData()
        {
            // 刷新基础数据, 收到数据刷新一次结束

            while (true)
            {
                try
                {
                    if (CommonUtils.IsEndofLoadFromDB() && bondWithOutInfoList.Count > 0)
                    {
                        onloadData(true);
                    }
                    //if (bondWithOutInfoList.Count <= 0)
                    //{
                    //    break;
                    //}
                    Thread.Sleep(500);
                }
                catch (ThreadAbortException)
                {
                    Thread.Sleep(500);
                    //ignore
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
                    removeList.Add(bondsInfoPo.Bond_ID);
                    Sub_OnLoadData(bondsInfoPo);
                }
                Thread.Sleep(100);
            }

            //foreach (string securityID in this.bondWithOutInfoList)
            //{
            //    BondsInfoPo bondsInfoPo = CommonUtils.GetBondsInfoPoBySecurityID(securityID);
            //    if (bondsInfoPo != null)
            //    {
            //        removeList.Add(bondsInfoPo.Bond_ID);
            //        Sub_OnLoadData(bondsInfoPo);
            //    }
            //}

            foreach (string securityID in removeList)
            {
                object removeOjb = null;
                bondWithOutInfoList.TryRemove(securityID, out removeOjb);
            }
        }

        private void Sub_OnLoadData(BondsInfoPo bondsInfoPo)
        {
            if (Application.Current != null && bondsInfoPo != null)
            {
                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    lock (this.lockObj)
                    {
                        bool isFuture = CommonUtils.IsFutureBond(bondsInfoPo.Bond_ID);
                        DataRow[] drs = this.PriceTableView.Table.Select($"{SECURITY_ID_FIELD} = '{bondsInfoPo.Bond_ID}'");
                        if (drs != null && drs.Length > 0)
                        {
                            foreach (DataRow dr in drs)
                            {
                                if (bondsInfoPo != null)
                                {
                                    dr[SYMBOL_FIELD] = bondsInfoPo.Short_Name;// 代码名称
                                    dr[REMAING_TIME_FIELD] = isFuture? CommonConstants.NO_PRICE : bondsInfoPo.RemaingingTime;//剩余时间
                                    dr[IS_SHOW_SETTLE_FILED] = isFuture ? "0" : "1";
                                    dr[REMAINGING_TIME_CONVERT_FIELD] = CommonUtils.ConvertRemainingTime(bondsInfoPo.RemaingingTime);//剩余时间转换成时间段
                                    dr[BOND_TYPE_FILED] = isFuture ? "国债期货" : CommonUtils.GetBondCategoryName(bondsInfoPo.Bond_Subtype, bondsInfoPo.Ent_Cor);// 债券类型 
                                    dr[BOND_SUB_TYPE_FIELD] = bondsInfoPo.Bond_Subtype; //债券子类
                                    dr[GOODS_TYPE_FIELD] = bondsInfoPo.SECURITY_TYPE;  //债券大类
                                    dr[ENT_COR_FIELD] = bondsInfoPo.Ent_Cor; //Ent_Cor
                                    dr[BOND_CATEGORY_FIELD] = CommonUtils.GetBondCategory(bondsInfoPo.Bond_Subtype, bondsInfoPo.Ent_Cor);
                                    // 中债估值
                                    string valuation = PriceHandler.Instance.GetValuation(bondsInfoPo.Bond_ID, string.Empty);
                                    if (CommonUtils.HasCDCAuth(bondsInfoPo.Bond_ID))
                                    {
                                        if (string.IsNullOrEmpty(valuation))
                                            dr[VALUATION_FIELD] = CommonConstants.NO_PRICE;
                                        else
                                            dr[VALUATION_FIELD] = CommonUtils.FormatNum(valuation);
                                    }
                                    else
                                    {
                                        dr[VALUATION_FIELD] = CommonConstants.NO_PERMISSION;
                                    }

                                    if (bondsInfoPo.Issue_Amount.HasValue)
                                    {
                                        dr[OUTSTANDING_AMOUNT_FIELD] = bondsInfoPo.Issue_Amount / 10000;
                                    }
                                    else
                                    {
                                        dr[OUTSTANDING_AMOUNT_FIELD] = 0;
                                    }
                                    
                                    dr[MATURITY_DATE_FIELD] = CommonUtils.ConvertToDateTime2(bondsInfoPo.Maturity_Date).ToString("yyyy-MM-dd");

                                    // 市场最新成交
                                    if (ExecutionsHandler.Instance.EndInit())
                                    {
                                        ExecutionReport temOrder = GetFirstExecutionReport(bondsInfoPo.Bond_ID);

                                        if (temOrder != null)
                                        {
                                            UpdateLastMarketExecutions(temOrder, dr);
                                        }
                                    }
                                }
                                else
                                {
                                    dr[SYMBOL_FIELD] = string.Empty;// 代码名称
                                    dr[REMAING_TIME_FIELD] = string.Empty;//剩余时间
                                    dr[REMAINGING_TIME_CONVERT_FIELD] = 0;//剩余时间转换成时间段
                                    dr[BOND_TYPE_FILED] = string.Empty;// 债券类型 
                                    dr[BOND_SUB_TYPE_FIELD] = string.Empty; //债券子类
                                    dr[GOODS_TYPE_FIELD] = string.Empty;  //债券大类
                                    dr[ENT_COR_FIELD] = string.Empty; //Ent_Cor
                                    dr[BOND_CATEGORY_FIELD] = string.Empty;

                                    dr[IS_SHOW_SETTLE_FILED] = string.Empty;
                                    dr[OUTSTANDING_AMOUNT_FIELD] = 0;
                                    dr[MATURITY_DATE_FIELD] = string.Empty;
                                }
                            }
                        }
                    }
                }));
            }
        }
        
        #endregion

        private void Instance_OnFavoritesChanged(FavoriteTypeEnum actionEnum, string groupID, string securityID, string bondId)
        {
            if (actionEnum == FavoriteTypeEnum.AddBond || actionEnum == FavoriteTypeEnum.AddBondFromTable || actionEnum == FavoriteTypeEnum.DelBond 
                || actionEnum == FavoriteTypeEnum.DelBondFromTable || actionEnum == FavoriteTypeEnum.AddBondFromCard)
            {
                if (groupID == this._groupId)
                {
                    this.SyncBonds(false);
                }
            }
        }


        public void SyncBonds(bool isUpdateFromServer)
        {
            ThreadPool.QueueUserWorkItem(o =>
            {
                IOrderedEnumerable<KeyValuePair<string, HomeGroupInfo>> dic = HomePageDataHandler.Instance.AsyncRequestGroups(isUpdateFromServer);
                HomeGroupInfo homeGroupInfo = HomePageDataHandler.Instance.GetHomeGroupInfo(dic, _groupId);
                if (homeGroupInfo != null)
                {
                    _initBondNum = homeGroupInfo.securityids.Count;
                    Instance_OnBondList(homeGroupInfo.securityids);
                }
            });
        }

        private void Instance_OnBondList(List<HomeGroupSecurityIDsInfo> bondList)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                lock (this.lockObj)
                {
                    //删除
                    List<DataRow> removeList = new List<DataRow>();

                    if (bondList != null && bondList.Count > 0)
                    {
                        foreach (DataRowView drv in this.PriceTableView)
                        {
                            string securityID = CommonUtils.ConvertToString(drv[KEY_FIELD]);
                            if (bondList.Any<HomeGroupSecurityIDsInfo>(bond => (bond.id == securityID)) == false)
                            {
                                removeList.Add(drv.Row);
                            }
                        }
                    }
                    else
                    {
                        foreach (DataRowView drv in this.PriceTableView)
                        {
                            removeList.Add(drv.Row);
                        }
                    }

                    if (removeList.Count > 0)
                    {
                        List<string> removeSecurityList = new List<string>();
                        foreach (DataRow removeDr in removeList)
                        {
                            string securityID = CommonUtils.ConvertToString(removeDr[SECURITY_ID_FIELD]);
                            this.PriceTableView.Table.Rows.Remove(removeDr);

                            removeSecurityList.Add(securityID);
                        }

                        foreach (string securityID in removeSecurityList)
                        {
                            DataRow[] drs = this.PriceTableView.Table.Select($"{SECURITY_ID_FIELD} = '{securityID}'");
                            if (drs == null || drs.Length == 0)
                            {
                                // 取消订阅
                                PriceHandler.Instance.UnregisterCTPrice(securityID, Instance_OnMDPrice);

                                //DataView removeDV;//todo_lily:
                                //EmsOrdersHandler.Instance.UnregistEMSOrderBySecurityID(securityID, EmsOrderProcesser.Processer.Instance_OnEmsOrders);
                            }
                        }

                        if (PriceTableView.Table.Rows.Count > 0)
                        {
                            DataGrid dgDataGrid = _ownUI.FindName("dgData") as DataGrid;
                            dgDataGrid.SelectedIndex = 0;
                            string security = (dgDataGrid.SelectedItem as DataRowView)[SECURITY_ID_FIELD] as string;
                            string id = (dgDataGrid.SelectedItem as DataRowView)[KEY_FIELD] as string;
                            BondChangeHandler.Instance.OnBondChange(BondUpdateType.ChangeFocusRow, this.GroupID, id, security, string.Empty, false);
                        }
                        else
                        {
                            BondChangeHandler.Instance.OnBondChange(BondUpdateType.ChangeFocusRow, string.Empty, string.Empty, string.Empty, string.Empty, false);
                        }
                    }

                    // 添加和更新
                    if (bondList != null && bondList.Count > 0)
                    {
                        foreach (HomeGroupSecurityIDsInfo bondObj in bondList)
                        {
                            if (_ProcessBondsThreadQueue != null)
                            {
                                _ProcessBondsThreadQueue.Enqueue(bondObj);
                            }
                        }
                    }
                }
            }));

        }

        #region 实时报价/成交通知

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
            if (Application.Current != null)
            {
                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    UpdateMyOrders(obj);
                }));
            }
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
                    DataRow[] drs = this.PriceTableView.Table.Select($"{SECURITY_ID_FIELD} = '{obj.SecurityID}'");
                    if (drs != null && drs.Length > 0)
                    {
                        foreach (DataRow dr in drs)
                        {
                            BindIsSameTagToUI(obj.SecurityID, dr);

                            //dr.AcceptChanges();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                LogHelper.Error(e);
            }
        }

        /// <summary>
        /// 更新最优价格是否和我的订单价格一样标志
        /// </summary>
        /// <param name="securityID"></param>
        /// <param name="item"></param>
        private void BindIsSameTagToUI(string securityID, DataRow item)
        {
            bool isFuture = CommonUtils.IsFutureBond(securityID);
            // 我的订单
            //DataRowView bidOrder = MyOrderProcesser.Processer.GetLstOrderBySecurityID(securityID, "1");
            string settle = item[SETTLE_TYPE_FIELD] as string;

            string filterStr = CTPriceControler.GetAloneSettleFilter(settle);

            DataRow bidOrder = null;// MyOrderProcesser.Processer.GetMyBestOrderBySecurityId(securityID, CommonConstants.SIDE_BID_ONE, "0", filterStr);//this._rowFilterStr);//item[SETTLE_TYPE_FIELD] as string);
            if (bidOrder != null)
            {
                // 最优价格 我的订单 比较， 是否相同
                decimal bestBidPrice = CommonUtils.ConvertToDecimal(item[BID_BEST_FIELD].ToString());
                string myLstBidPrice = CommonUtils.ConvertToString(bidOrder[MyOrderProcesser.PRICE_ORDER_FIELD]);
                decimal myBidOrderPrice = CommonUtils.ConvertToDecimal(myLstBidPrice);
                string isPoint = CommonUtils.ConvertToString(bidOrder[MyOrderProcesser.ISPOINT_FILED].ToString());

                if (string.IsNullOrEmpty(isPoint) || isPoint == "0")
                {
                    if (bestBidPrice == myBidOrderPrice && CommonUtils.IsNumeric(item[BID_BEST_FIELD].ToString()) && CommonUtils.IsNumeric(myLstBidPrice))
                    {
                        item[IS_BID_SAME_FIELD] = "1";
                        item[BID_BEST_FLAG_TYPE_FIELD] = "1";
                    }
                    else
                    {
                        item[IS_BID_SAME_FIELD] = "0";
                        item[BID_BEST_FLAG_TYPE_FIELD] = "2";
                    }
                }
                else
                {
                    item[IS_BID_SAME_FIELD] = "0";

                    item[BID_BEST_FLAG_TYPE_FIELD] = string.Empty;
                }

            }
            else
            {
                //是否包含我的订单
                bidOrder = null;//MyOrderProcesser.Processer.GetMyOrderByCondition(securityID, CommonConstants.SIDE_BID_ONE, filterStr, string.Empty);//this._rowFilterStr);

                if (bidOrder != null)
                {
                    DataRow temDr = MyOrderProcesser.Processer.GetMyOrderByCondition(securityID, CommonConstants.SIDE_BID_ONE, filterStr, "0");
                    if (temDr != null)
                        item[BID_BEST_FLAG_TYPE_FIELD] = "2";
                    else
                        item[BID_BEST_FLAG_TYPE_FIELD] = string.Empty;

                }
                else
                {
                    item[IS_BID_SAME_FIELD] = "0";

                    item[BID_BEST_FLAG_TYPE_FIELD] = string.Empty;
                }

            }

            //更新MyBid
            bidOrder = null;// MyOrderProcesser.Processer.GetMyBestOrderBySecurityId(securityID, CommonConstants.SIDE_BID_ONE, string.Empty, filterStr);
            if (bidOrder != null)
            {
                item[MY_BID_ORDERID_FIELD] = bidOrder[MyOrderProcesser.KEY_ORDER_FIELD];
                string tem = CommonUtils.ConvertToString(bidOrder[MyOrderProcesser.PRICE_ORDER_FIELD]);
                item[MY_BID_FIELD] = isFuture ? CommonUtils.FormatNumFuture(tem) : CommonUtils.FormatNum(tem);
                string dealQty = bidOrder[MyOrderProcesser.EXECQTY_ORDER_FIELD].ToString();
                string qty = bidOrder[MyOrderProcesser.QTY_ORDER_FIELD].ToString();

                item[MY_BID_QTY_FIELD] = PriceCalHelper.Instance.CalSurplusQty(qty, dealQty);
            }
            else
            {
                item[MY_BID_ORDERID_FIELD] = string.Empty;
                item[MY_BID_FIELD] = string.Empty;
                item[MY_BID_QTY_FIELD] = string.Empty;
            }


            DataRow ofrOrder = null;// MyOrderProcesser.Processer.GetMyBestOrderBySecurityId(securityID, CommonConstants.SIDE_OFR_TWO, "0", filterStr);//this._rowFilterStr);
            if (ofrOrder != null)
            {
                // 最优价格 我的订单 比较， 是否相同
                decimal bestOfrPrice = CommonUtils.ConvertToDecimal(item[OFR_BEST_FIELD].ToString());
                string myLstOfrPrice = CommonUtils.ConvertToString(ofrOrder[MyOrderProcesser.PRICE_ORDER_FIELD]);
                decimal myOfrOrderPrice = CommonUtils.ConvertToDecimal(myLstOfrPrice);

                string isPoint = CommonUtils.ConvertToString(ofrOrder[MyOrderProcesser.ISPOINT_FILED].ToString());

                if (string.IsNullOrEmpty(isPoint) || isPoint == "0")
                {
                    if (bestOfrPrice == myOfrOrderPrice && CommonUtils.IsNumeric(item[OFR_BEST_FIELD].ToString()) && CommonUtils.IsNumeric(myLstOfrPrice))
                    {
                        item[IS_OFR_SAME_FIELD] = "1";
                        item[OFR_BEST_FLAG_TYPE_FIELD] = "1";
                    }
                    else
                    {
                        item[IS_OFR_SAME_FIELD] = "0";
                        item[OFR_BEST_FLAG_TYPE_FIELD] = "2";
                    }
                }
                else
                {
                    item[IS_OFR_SAME_FIELD] = "0";

                    item[OFR_BEST_FLAG_TYPE_FIELD] = string.Empty;
                }

            }
            else
            {
                //是否包含我的订单
                ofrOrder = null;//MyOrderProcesser.Processer.GetMyOrderByCondition(securityID, CommonConstants.SIDE_OFR_TWO, filterStr, string.Empty);//this._rowFilterStr);

                if (ofrOrder != null)
                {
                    DataRow temDr = MyOrderProcesser.Processer.GetMyOrderByCondition(securityID, CommonConstants.SIDE_OFR_TWO, filterStr, "0");
                    if (temDr != null)
                        item[OFR_BEST_FLAG_TYPE_FIELD] = "2";
                    else
                        item[OFR_BEST_FLAG_TYPE_FIELD] = string.Empty;

                }
                else
                {
                    item[OFR_BEST_FLAG_TYPE_FIELD] = string.Empty;
                    item[IS_OFR_SAME_FIELD] = "0";
                }
            }

            //更新MyOfr
            ofrOrder = null;// MyOrderProcesser.Processer.GetMyBestOrderBySecurityId(securityID, CommonConstants.SIDE_OFR_TWO, string.Empty, filterStr);
            if (ofrOrder != null)
            {
                item[MY_OFR_ORDERID_FIELD] = ofrOrder[MyOrderProcesser.KEY_ORDER_FIELD];
                string tem = CommonUtils.ConvertToString(ofrOrder[MyOrderProcesser.PRICE_ORDER_FIELD]);
                item[MY_OFR_FIELD] = isFuture ? CommonUtils.FormatNumFuture(tem) : CommonUtils.FormatNum(tem);
                //item[MY_OFR_FIELD] = ofrOrder[MyOrderProcesser.PRICE_ORDER_FIELD];

                string dealQty = ofrOrder[MyOrderProcesser.EXECQTY_ORDER_FIELD].ToString();
                string qty = ofrOrder[MyOrderProcesser.QTY_ORDER_FIELD].ToString();

                item[MY_OFR_QTY_FIELD] = PriceCalHelper.Instance.CalSurplusQty(qty, dealQty);
            }
            else
            {
                item[MY_OFR_ORDERID_FIELD] = string.Empty;
                item[MY_OFR_FIELD] = string.Empty;
                item[MY_OFR_QTY_FIELD] = string.Empty;
            }
        }

        private void OnOHLC(OhlcMsg obj)
        {
            if (_ohlcThreadQueue != null)
            {
                _ohlcThreadQueue.Enqueue(obj);
            }
        }

        private void UpdateOHLC(OhlcMsg obj)
        {
            // 市场最新成交
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                lock (lockObj)
                {
                    if (obj != null && !CommonUtils.IsFutureBond(obj.securityid))
                    {
                        DataRow[] drs = this.PriceTableView.Table.Select($"{SECURITY_ID_FIELD} = '{obj.securityid}'");
                        if (drs != null && drs.Length > 0)
                        {
                            foreach (DataRow dr in drs)
                            {
                                int execNum = 0;
                                //更新成交数
                                if (int.TryParse(obj.inf1, out execNum))
                                {
                                    //MarketCount market = JsonConvert.DeserializeObject<MarketCount>(obj.Info1);

                                    dr[EXEC_ORDER_NUM_FILED] = execNum;//obj.numTrades;

                                }
                                else
                                {
                                    dr[EXEC_ORDER_NUM_FILED] = 0;// CommonConstants.NO_PRICE;
                                }
                            }
                        }
                    }
                }
            }));
        }


        private void Processer_OnLastExecutions(ExecutionReport obj)
        {
            if (_ProcessLastExecutionsThreadQueue != null)
            {
                _ProcessLastExecutionsThreadQueue.Enqueue(obj);
            }
        }

        private void UpdateLastExecutionsAsync(ExecutionReport obj)
        {
            // 市场最新成交
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                UpdateLastExecutions(obj);
            }));
        }

        private void UpdateLastExecutions(ExecutionReport obj)
        {
            lock (lockObj)
            {
                if (obj != null)
                {
                    DataRow[] drs = this.PriceTableView.Table.Select($"{SECURITY_ID_FIELD} = '{obj.SecurityID}'");
                    if (drs != null && drs.Length > 0)
                    {
                        foreach (DataRow dr in drs)
                        {
                            UpdateLastMarketExecutions(obj, dr);
                        }
                    }
                }
            }
        }

        private void UpdateLastMarketSide(ExecutionReport obj, DataRow dr, bool isFuture)
        {
            if (dr != null)
            {
                ObservableCollection<MarketExecutionsItem> lastExeItem = LastExecutionProcesser.Processer.GetDatasBySecurityID(obj.SecurityID);

                List<MarketExecutionsItem> copyLastExeItem = lastExeItem.ToList();

                MarketExecutionsItem lastMarketExeItem = LastExecutionProcesser.Processer.GetLastExecutionsItem(obj.SecurityID);

                if (lastMarketExeItem != null)
                {
                    dr[MARKET_DOWNUP_FIELD] = lastMarketExeItem.DownUp;
                    dr[MARKET_DOWNUP_BRUSH_FILED] = lastMarketExeItem.DownUpBrush;
                }
                else
                {
                    dr[MARKET_DOWNUP_FIELD] = CommonConstants.NO_UP_DOWN;
                    dr[MARKET_DOWNUP_BRUSH_FILED] = "0";
                }

                int index = 0;


                dr[RECTANGLE_ONE_FIELD] = dr[RECTANGLE_TWO_FIELD] = dr[RECTANGLE_THREE_FIELD] = dr[RECTANGLE_FOUR_FIELD] = dr[RECTANGLE_FIVE_FIELD] = "0";

                foreach (MarketExecutionsItem item in copyLastExeItem)
                {
                    if (isFuture)
                    {
                        switch (index)
                        {
                            case 0:
                                dr[RECTANGLE_ONE_FIELD] = (item.Side == "9" || item.Side == "3" || item.Side == "5" || item.Side == "8") ? CommonConstants.SIDE_FUTURE_FOUR : (item.Side == "10" || item.Side == "4" || item.Side == "6" || item.Side == "7")?CommonConstants.SIDE_FUTURE_FIVE: string.Empty;
                                break;
                            case 1:
                                dr[RECTANGLE_TWO_FIELD] = (item.Side == "9" || item.Side == "3" || item.Side == "5" || item.Side == "8") ? CommonConstants.SIDE_FUTURE_FOUR : (item.Side == "10" || item.Side == "4" || item.Side == "6" || item.Side == "7") ? CommonConstants.SIDE_FUTURE_FIVE : string.Empty;
                                break;
                            case 2:
                                dr[RECTANGLE_THREE_FIELD] = (item.Side == "9" || item.Side == "3" || item.Side == "5" || item.Side == "8") ? CommonConstants.SIDE_FUTURE_FOUR : (item.Side == "10" || item.Side == "4" || item.Side == "6" || item.Side == "7") ? CommonConstants.SIDE_FUTURE_FIVE : string.Empty;
                                break;
                            case 3:
                                dr[RECTANGLE_FOUR_FIELD] = (item.Side == "9" || item.Side == "3" || item.Side == "5" || item.Side == "8") ? CommonConstants.SIDE_FUTURE_FOUR : (item.Side == "10" || item.Side == "4" || item.Side == "6" || item.Side == "7") ? CommonConstants.SIDE_FUTURE_FIVE : string.Empty;
                                break;
                            case 4:
                                dr[RECTANGLE_FIVE_FIELD] = (item.Side == "9" || item.Side == "3" || item.Side == "5" || item.Side == "8") ? CommonConstants.SIDE_FUTURE_FOUR : (item.Side == "10" || item.Side == "4" || item.Side == "6" || item.Side == "7") ? CommonConstants.SIDE_FUTURE_FIVE : string.Empty;
                                break;
                        }
                    }
                    else
                    {
                        switch (index)
                        {
                            case 0:
                                dr[RECTANGLE_ONE_FIELD] = (item.Side == "Y" || item.Side == "1") ? CommonConstants.SIDE_BID_ONE : (item.Side == "X" || item.Side == "2") ? CommonConstants.SIDE_OFR_TWO : (item.Side == "Z" || item.Side == "3") ? CommonConstants.SIDE_TRD_THREE : string.Empty;
                                break;
                            case 1:
                                dr[RECTANGLE_TWO_FIELD] = (item.Side == "Y" || item.Side == "1") ? CommonConstants.SIDE_BID_ONE : (item.Side == "X" || item.Side == "2") ? CommonConstants.SIDE_OFR_TWO : (item.Side == "Z" || item.Side == "3") ? CommonConstants.SIDE_TRD_THREE : string.Empty;
                                break;
                            case 2:
                                dr[RECTANGLE_THREE_FIELD] = (item.Side == "Y" || item.Side == "1") ? CommonConstants.SIDE_BID_ONE : (item.Side == "X" || item.Side == "2") ? CommonConstants.SIDE_OFR_TWO : (item.Side == "Z" || item.Side == "3") ? CommonConstants.SIDE_TRD_THREE : string.Empty;
                                break;
                            case 3:
                                dr[RECTANGLE_FOUR_FIELD] = (item.Side == "Y" || item.Side == "1") ? CommonConstants.SIDE_BID_ONE : (item.Side == "X" || item.Side == "2") ? CommonConstants.SIDE_OFR_TWO : (item.Side == "Z" || item.Side == "3") ? CommonConstants.SIDE_TRD_THREE : string.Empty;
                                break;
                            case 4:
                                dr[RECTANGLE_FIVE_FIELD] = (item.Side == "Y" || item.Side == "1") ? CommonConstants.SIDE_BID_ONE : (item.Side == "X" || item.Side == "2") ? CommonConstants.SIDE_OFR_TWO : (item.Side == "Z" || item.Side == "3") ? CommonConstants.SIDE_TRD_THREE : string.Empty;
                                break;
                        }
                    }
                    index++;

                    if (index >= 5)
                        break;
                }

                if (index == 0)
                {
                    dr[RECTANGLE_ONE_FIELD] = dr[RECTANGLE_TWO_FIELD] = dr[RECTANGLE_THREE_FIELD] = dr[RECTANGLE_FOUR_FIELD] = dr[RECTANGLE_FIVE_FIELD] = string.Empty;
                }
            }
        }

        private void UpdateLastMarketExecutions(ExecutionReport obj, DataRow dr)
        {
            string side = obj.Side == CommonConstants.SIDE_X ? CommonConstants.SIDE_OFR_TWO : obj.Side == CommonConstants.SIDE_Y ? CommonConstants.SIDE_BID_ONE : obj.Side;
            string security = obj.SecurityID;

            bool isFuture = CommonUtils.IsFutureBond(security);

            //更新最新成交
            LastPriceParam lastPriceParam = LastExecutionProcesser.Processer.CalLastExe(obj);

            dr[LAST_EXEC_PRICE_SIDE_FIELD] = lastPriceParam.UpDown == CommonConstants.SIDE_BID_ONE ? CommonConstants.UP : lastPriceParam.UpDown == CommonConstants.SIDE_OFR_TWO ? CommonConstants.DOWN : CommonConstants.NO_UP_DOWN;

            decimal bpd = 0;
            if (decimal.TryParse(lastPriceParam.Bp, out bpd))
            {
                dr[LAST_EXEC_PRICE_BP_FIELD] = (isFuture ? string.Empty : (bpd > 0 ? "+" : "-")) + PriceCalHelper.Instance.RemoveZero(Math.Abs(bpd).ToString()) + (isFuture ? string.Empty : "bp");//PriceCalHelper.Instance.RemoveZero(Math.Abs(bpd).ToString());

                if (isFuture)
                {
                    dr[LAST_EXEC_PRICE_DISPLAY_SIDE_BRUSH_FIELD] = lastPriceParam.UpDown == CommonConstants.SIDE_BID_ONE ? CommonConstants.SIDE_OFR_TWO  : lastPriceParam.UpDown == CommonConstants.SIDE_OFR_TWO ? CommonConstants.SIDE_BID_ONE : "0";
                }
                else
                {
                    if (bpd > 0)
                    {
                        dr[LAST_EXEC_PRICE_DISPLAY_SIDE_BRUSH_FIELD] = CommonConstants.SIDE_BID_ONE;
                    }
                    else
                    {
                        dr[LAST_EXEC_PRICE_DISPLAY_SIDE_BRUSH_FIELD] = CommonConstants.SIDE_OFR_TWO;
                    }
                }
            }
            else
            {
                dr[LAST_EXEC_PRICE_BP_FIELD] = lastPriceParam.Bp;
                dr[LAST_EXEC_PRICE_DISPLAY_SIDE_BRUSH_FIELD] = "0";//CommonConstants.SIDE_OFR_TWO;
            }
            //dr[LAST_EXEC_PRICE_BP_FIELD] = lastPriceParam.Bp;
            dr[LAST_EXEC_PRICE_FIELD] = string.IsNullOrEmpty(obj.LastPx) ? decimal.MinValue : obj.LastPx.ConvertToDecimal();
            dr[LAST_EXEC_PRICE_DISPLAY_FIELD] = string.IsNullOrEmpty(obj.LastPx) ? null : isFuture? CommonUtils.FormatNumFuture(obj.LastPx) : CommonUtils.FormatNum(obj.LastPx);
            
            dr[LAST_EXEC_PRICE_DISPLAY_SIDE_FIELD] = obj.Side;//lastPriceParam.Side;
            //dr[LAST_EXEC_PRICE_DISPLAY_SIDE_BRUSH_FIELD] = lastPriceParam.UpDown;

            

            if (obj.Side == CommonConstants.SIDE_OFR_TWO || obj.Side == CommonConstants.SIDE_X)
            {
                dr[LAST_EXEC_PRICE_TKN_GVN_BRUSH_FIELD] = CommonConstants.SIDE_BID_ONE;
                dr[LAST_EXEC_PRICE_DISPLAY_SIDE_FIELD] = CommonConstants.TKN;
            }
            else if (obj.Side == CommonConstants.SIDE_BID_ONE || obj.Side == CommonConstants.SIDE_Y)
            {
                dr[LAST_EXEC_PRICE_TKN_GVN_BRUSH_FIELD] = CommonConstants.SIDE_OFR_TWO;
                dr[LAST_EXEC_PRICE_DISPLAY_SIDE_FIELD] = CommonConstants.GVN;
            }
            else if (obj.Side == CommonConstants.SIDE_TRD_THREE || obj.Side == CommonConstants.SIDE_Z)
            {
                dr[LAST_EXEC_PRICE_TKN_GVN_BRUSH_FIELD] = CommonConstants.SIDE_TRD_THREE;
                dr[LAST_EXEC_PRICE_DISPLAY_SIDE_FIELD] = CommonConstants.TRD;
            }
            else
            {
                dr[LAST_EXEC_PRICE_TKN_GVN_BRUSH_FIELD] = string.Empty;
                dr[LAST_EXEC_PRICE_DISPLAY_SIDE_FIELD] = string.Empty;
            }

            if (isFuture)
            {
                dr[LAST_EXEC_PRICE_DISPLAY_SIDE_FIELD] = string.Empty;//CommonConstants.NO_PRICE;

                dr[LAST_EXEC_SOURCE_FIELD] = string.Empty;//CommonConstants.NO_PRICE;
            }
            else
                dr[LAST_EXEC_SOURCE_FIELD] = ComboSourceUtil.Instance.FindEnum(lastPriceParam.Source, "venues");

            dr[LAST_EXEC_TIME_FIELD] = obj.TransactTime;

            //更新价格趋势
            UpdateLastMarketSide(obj, dr, isFuture);

        }


        private void UpdateBondAsync(HomeGroupSecurityIDsInfo obj)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                UpdateBond(obj);
            }));
        }


        private void ResetSort()
        {
            foreach (DataRow dataRow in this.PriceTableView.Table.Rows)
            {
                string id = dataRow[KEY_FIELD] as string;
                int insertIndex = SortCache.Instance.GetSettleById(GroupID, id);
                if (insertIndex >= 0)
                {
                    dataRow[CUSTOMER_SORT_FIELD] = insertIndex;
                }
            }
        }

        /// <summary>
        /// 更新债券列表
        /// </summary>
        /// <param name="bond"></param>
        private void UpdateBond(HomeGroupSecurityIDsInfo obj)
        {
            try
            {
                lock (lockObj)
                {
                    //DataRow bondInfosItems = this.BondView.Table.Rows.Find(new object[] { obj.securityid, obj.settltype, obj.venues, obj.venuetypegw });
                    DataRow bondInfosItems = this.PriceTableView.Table.Rows.Find(new object[] { obj.id });

                    if (bondInfosItems == null)
                    {
                        ResetSort();

                        bondInfosItems = this.PriceTableView.Table.NewRow();
                        //bondInfosItems[KEY_FIELD] = obj.id;
                        bondInfosItems[SETTLE_ROW_FIELD] = CommonConstants.ALL_PARTY;
                        // OpenClsoe
                        bondInfosItems[KEY_FIELD] = obj.id;
                        bondInfosItems[SECURITY_DB_ID_FIELD] = obj.id;
                        
                        bondInfosItems[SETTLE_TYPE_FIELD] = CommonConstants.ALL_PARTY; // 清算速度
                        
                        bondInfosItems[SECURITY_ID_FIELD] = obj.securityid; // 代码

                        bondInfosItems[MARKET_DOWNUP_FIELD] = CommonConstants.NO_UP_DOWN;
                        
                        // 市场最新成交
                        if (ExecutionsHandler.Instance.EndInit())
                        {
                            ExecutionReport temOrder = GetFirstExecutionReport(obj.securityid);

                            if (temOrder != null)
                            {
                                UpdateLastMarketExecutions(temOrder, bondInfosItems);

                            }
                        }

                        int cacheSort = SortCache.Instance.GetSettleById(GroupID, obj.id);
                        if (cacheSort >= 0)
                            bondInfosItems[CUSTOMER_SORT_FIELD] = cacheSort;
                        else
                            bondInfosItems[CUSTOMER_SORT_FIELD] = PriceTableView.Table.Rows.Count;

                        string cacheSettle = SettleCache.Instance.GetSettleById(GroupID, obj.id);
                        if (!string.IsNullOrEmpty(cacheSettle))
                        {
                            bondInfosItems[SETTLE_TYPE_FIELD] = cacheSettle;

                            string filterStr = CTPriceControler.GetAloneSettleFilter(cacheSettle);
                            
                            _rowSettleFilterDic[obj.id] = filterStr;
                        }


                        int insertIndex = SortCache.Instance.GetSettleById(obj.groupId, obj.id);

                        if (insertIndex < 0)
                        {
                            insertIndex = QuoteConfigHandler.Instance.GetSecurityIndexById();
                        }

                        LogHelper.Info(string.Format("GroupId:{0}, add security:{1}, id:{2}, insertIndex:{3}", obj.groupId, obj.securityid, obj.id, insertIndex));

                        if (insertIndex < 0 || insertIndex >= PriceTableView.Count)
                        {
                            this.PriceTableView.Table.Rows.Add(bondInfosItems);

                            insertIndex = this.PriceTableView.Table.Rows.Count;
                        }
                        else
                        {
                            this.PriceTableView.Table.Rows.InsertAt(bondInfosItems, insertIndex);
                        }

                        bondInfosItems[CUSTOMER_SORT_FIELD] = insertIndex;

                        LayoutSetttingCache.Instance.CurrentUpdatePanle = null;
                        bondWithOutInfoList[obj.securityid] = null;

                        //订阅price
                        PriceHandler.Instance.RegisterCTPrice(obj.securityid, Instance_OnMDPrice);

                        #region 订阅OHLC

                        OhlcHandler.Instance.SubscribeOhlc(obj.securityid, OnOHLC);

                        #endregion
                    }

                    if (bondInfosItems != null)
                    {
                        // 如果更新时间是一样的, 就不要更新了
                        if (CommonUtils.ConvertToString(bondInfosItems[LAST_ACTION_TIME_FIELD]) == obj.updatetime)
                        {
                            return;
                        }

                        bondInfosItems[LAST_ACTION_TIME_FIELD] = obj.updatetime;
                        //bondInfosItems[EXEC_VALUE_FIELD] = CommonUtils.ConvertToDecimal(obj.netqty);  // 今日净成交
                        bondInfosItems[CREATETIME_FIELD] = obj.createtime;

                        bool isFuture = CommonUtils.IsFutureBond(obj.securityid);
                        bondInfosItems[IS_SHOW_SETTLE_FILED] = isFuture ? "0" : string.Empty;

                    }
                    this.PriceTableView.Table.AcceptChanges();


                    RequestMarketExecutions(obj.securityid);
                }
            }
            catch (Exception e)
            {
                LogHelper.Error(e);
            }
        }


        #region 各个card自己加载自己的数据

        private ConcurrentDictionary<string, bool> sendStatusDic = new ConcurrentDictionary<string, bool>();
        public void RequestMarketExecutions(string securityId)
        {
            bool isLoad = MarketExecCache.Instance.GetLoadStatus(securityId);

            if (isLoad)
            {
                List<ExecutionReport> execList = MarketExecCache.Instance.GetMarketExecByCondition(securityId, string.Empty);
                
                if (execList != null && execList.Count > 0)
                {
                    execList.Sort(new ExecComparer());
                    ExecutionsHandler.Instance.Instance_OnExecOrder(execList.Last());
                }
            }
            else
            {

                bool isFuture = CommonUtils.IsFutureBond(securityId);

                if (sendStatusDic.ContainsKey(securityId))
                {
                    bool isSend = sendStatusDic[securityId];
                    if (isSend)
                    {
                        return;
                    }
                }

                sendStatusDic.AddOrUpdate(securityId, true, (key, value) => true);



                Task.Run(() =>
                {
                    try
                    {
                        QueryOrderReq request = new QueryOrderReq();
                        string today = DateTimeEx.ServerTime.ToString("yyyy-MM-dd");
                        request.dealTimeBegin = today;
                        request.dealTimeEnd = today;
                        request.venuetypegw = isFuture ? ApiConsts.SDN : ApiConsts.CDH;
                        //request.venuetypegw = "CDH";
                        request.securityid = securityId;

                        List<string> permissionBroker = PermisionHandler.Instance.GetBondPermission();

                        foreach (string broker in permissionBroker)
                        {
                            request.venues += string.Format("{0},", broker);
                        }

                        if (!string.IsNullOrEmpty(request.venues))
                        {
                            request.venues += "CTP,";
                            request.venues = request.venues.Substring(0, request.venues.Length - 1);
                        }
                        //N:已申请,0:处理中，1,2:已确认(其中1为部分确认,2完全确认)，4:已撤销，8:失败
                        //E(Broker B确认),F(Broker B撤销),G（Broker A确认），H（Broker A撤销）
                        request.orderStatus = "1,2";
                        int index = 0;
                        request.pageSize = MarketExecCache.Instance.Limit;

                        bool flag = true;
                        while (flag)
                        {
                            request.pageNum = index;
                            ///获取成交
                            //LogHelper.Warn(string.Format("PriceTableViewModel RequestMarketExecutions request:{0}", JsonConvert.SerializeObject(request)));
                            var reply = MarketExecHandler.Instance.QueryExecOrderForEMS(request);
                            //LogHelper.Warn(string.Format("PriceTableViewModel RequestMarketExecutions request:{0}", JsonConvert.SerializeObject(request)));
                            if (reply == null || reply.totalSize == 0 || reply.datas == null)
                            {
                                flag = false;
                            }
                            else if ((reply != null && reply.datas != null) || isFuture)
                            {
                                if (reply.datas.Count < request.pageSize || isFuture)
                                {
                                    flag = false;

                                    MarketExecCache.Instance.AddOrUpdateStatus(SearchText);

                                    RefreshMarketData(securityId);
                                }
                                else
                                {
                                    index++;

                                    Thread.Sleep(100);
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        LogHelper.Error("RequestExecutions", e);
                    }
                    finally
                    {
                        sendStatusDic.AddOrUpdate(securityId, false, (key, value) => false);
                    }
                });
            }

        }

        //从源头获取全量数据刷新
        public void RefreshMarketData(string securityId)
        {
            if (!string.IsNullOrEmpty(securityId))
            {
                bool isFuture = CommonUtils.IsFutureBond(securityId);
                List<ExecutionReport> exeList = MarketExecCache.Instance.GetMarketExecByCondition(securityId, isFuture ? ApiConsts.SDN : ApiConsts.CDH);
                foreach (ExecutionReport er in exeList)
                {
                    ExecutionsHandler.Instance.Instance_OnExecOrder(er);
                }
            }
        }

        #endregion


        #region 处理CT报价更新

        /// <summary>
        /// 做市报价（SOR）回调
        /// </summary>
        /// <param name="obj"></param>
        private void Instance_OnMDPrice(MarketDataSnapshotFullRefresh obj)
        {
            if (_ProcessCtPriceThreadQueue != null)
                _ProcessCtPriceThreadQueue.Enqueue(obj);

        }

        private string GetFilterSettle(string id)
        {
            string result = string.Empty;

            if (_rowSettleFilterDic.ContainsKey(id))
            {
                result = _rowSettleFilterDic[id];

            }
            else
                result = _rowFilterStr;

            return result;
        }

        private void CalBestPrice(string securityID, bool needRefreshAnimation)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                lock (this.lockObj)
                {
                    DataRow[] drs = null;
                    if (!string.IsNullOrEmpty(securityID))
                    {
                        drs = this.PriceTableView.Table.Select($"{SECURITY_ID_FIELD} = '{securityID}'");
                        
                        if (drs != null && drs.Length > 0)
                        {
                            foreach (DataRow item in drs)
                            {
                                string filterStr = GetFilterSettle(item[KEY_FIELD] as string);
                                BilateralBestPrice bestPrice = PriceHandler.Instance.CalBestPrice(securityID, filterStr, string.Empty);
                                
                                // 中债估值
                                string valuation = PriceHandler.Instance.GetValuation(securityID, string.Empty);
                                if (CommonUtils.HasCDCAuth(securityID))
                                {
                                    if (string.IsNullOrEmpty(valuation))
                                        item[VALUATION_FIELD] = CommonConstants.NO_PRICE;
                                    else
                                        item[VALUATION_FIELD] = CommonUtils.FormatNum(valuation);
                                }
                                else
                                {
                                    item[VALUATION_FIELD] = CommonConstants.NO_PERMISSION;
                                }
                                
                                //}

                                // 更新表格
                                BindBestPriceToUI(item, bestPrice, needRefreshAnimation);

                                // 更新最优价是否和我的订单一样标志，同时更新角标
                                //BindIsSameTagToUI(securityID, item);

                            }
                        }
                    }
                    else
                    {
                        DataRowCollection dataRowCollection = PriceTableView.Table.Rows;
                        foreach (DataRow item in dataRowCollection)
                        {
                            string filterStr = GetFilterSettle(item[KEY_FIELD] as string);
                            securityID = item[SECURITY_ID_FIELD] as string;
                            BilateralBestPrice bestPrice = PriceHandler.Instance.CalBestPrice(securityID, filterStr, string.Empty);
                            
                            // 中债估值
                            string valuation = PriceHandler.Instance.GetValuation(securityID, string.Empty);
                            if (CommonUtils.HasCDCAuth(securityID))
                            {
                                if (string.IsNullOrEmpty(valuation))
                                    item[VALUATION_FIELD] = CommonConstants.NO_PRICE;
                                else
                                    item[VALUATION_FIELD] = CommonUtils.FormatNum(valuation);
                            }
                            else
                            {
                                item[VALUATION_FIELD] = CommonConstants.NO_PERMISSION;
                            }
                            //}

                            // 更新表格
                            BindBestPriceToUI(item, bestPrice, needRefreshAnimation);

                            // 更新最优价是否和我的订单一样标志，同时更新角标
                            //BindIsSameTagToUI(securityID, item);

                        }
                    }
                }
            }));

        }

        private void StartPriceAnimation(string columnName, string name, DataRow item, bool useAnimation)
        {
            DataGrid dgDataGrid = _ownUI.FindName("dgData") as DataGrid;

            int flag = -1;
            foreach (DataGridColumn gridColumn in dgDataGrid.Columns)
            {
                flag++;
                if (gridColumn.Header != null && gridColumn.Header.ToString() == columnName)
                {
                    break;
                }
            }

            DataTable dataTable = PriceTableView.ToTable();

            int index = -1;
            foreach (DataRow dr in dataTable.Rows)
            {
                index++;
                if (dr[KEY_FIELD] == item[KEY_FIELD])
                {
                    break;
                }
            }

            if (index >= 0 && flag >= 0)
            {
                DataGridRow row = (DataGridRow)dgDataGrid.ItemContainerGenerator.ContainerFromIndex(index);//.ContainerFromIndex(0);

                if (row != null)
                {
                    ContentPresenter contentProesenter = dgDataGrid.Columns[flag].GetCellContent(row) as ContentPresenter;

                    if (contentProesenter != null)
                    {
                        Border border = contentProesenter.ContentTemplate.FindName(name, contentProesenter) as Border;

                        if (border != null)
                            AnimationHelper.Instance.StartAnimation(border, name, item[UPSIDE_FILED] as string, useAnimation);
                    }
                }
            }
        }


        private void BindBestPriceToUI(DataRow item, BilateralBestPrice bestPrice, bool needRefreshAnimation)
        {
            //计算倒挂
            string upSide = PriceCalHelper.Instance.IsUpSide(bestPrice) ? "1" : string.Empty;
            bool isFuture = CommonUtils.IsFutureBond(CommonUtils.ConvertToString(item[SECURITY_ID_FIELD]));
            if (isFuture)
            {
                upSide = string.Empty;
            }

            item[UPSIDE_FILED] = upSide;
            string setttle = CommonUtils.ConvertToString(item[SETTLE_TYPE_FIELD]);
            #region ofr
            if (bestPrice != null && bestPrice.OfrBest != null)
            {
                // 最优ofr price
                if (CommonUtils.IsNumeric(bestPrice.OfrBest.bestPx))
                {
                    decimal bestOfrPx = isFuture? CommonUtils.ConvertToDecimal(CommonUtils.formatString(bestPrice.OfrBest.bestPx, 3)) : CommonUtils.ConvertToDecimal(CommonUtils.FormatNum(bestPrice.OfrBest.bestPx));
                    item[OFR_BEST_FIELD] = bestOfrPx;
                    item[OFR_BEST_SORT_FIELD] = bestOfrPx;

                    string bestOfrPx_string = bestOfrPx.ToString("N4");
                    int index = bestOfrPx_string.IndexOf(".");
                }
                else if (!string.IsNullOrEmpty(bestPrice.OfrBest.bestPx))
                {
                    item[OFR_BEST_FIELD] = bestPrice.OfrBest.bestPx;
                    item[OFR_BEST_SORT_FIELD] = decimal.MinValue;
                } 
                else
                {
                    item[OFR_BEST_FIELD] = 0;
                    item[OFR_BEST_SORT_FIELD] = decimal.Zero;
                }


                // 最优ofr qty
                item[OFR_BEST_QTY_FIELD] = bestPrice.OfrBest.bestQty == 0 ? CommonConstants.NO_PRICE : bestPrice.OfrBest.bestQty.ToString();
                // 清算速度
                ////item[BEST_OFR_SETTLTYPE_TAG_FIELD] = ofrBP.bestT0;
                item[OFR_BEST_SETTLE_FIELD] = (string.IsNullOrEmpty(setttle) || setttle==CommonConstants.ALL_PARTY || setttle == CommonConstants.T1_EN) ? bestPrice.OfrBest.bestT0 : string.Empty;
                item[OFR_BEST_SETTLE_FWD_FIELD] = (string.IsNullOrEmpty(setttle) || setttle == CommonConstants.ALL_PARTY || setttle == CommonConstants.FWD) ? bestPrice.OfrBest.bestYuan: string.Empty;

                if (needRefreshAnimation)
                    StartPriceAnimation(Best_Ofr_CN, "bdUpSideOfr", item, true);
                else
                    StartPriceAnimation(Best_Ofr_CN, "bdUpSideOfr", item, false);
            }
            else
            {
                item[OFR_BEST_FIELD] = item[OFR_BEST_QTY_FIELD] = CommonConstants.NO_PRICE;
                item[OFR_BEST_SORT_FIELD] = decimal.MinValue;
                item[OFR_BEST_SETTLE_FWD_FIELD] = string.Empty;
                item[OFR_BEST_SETTLE_FIELD] = string.Empty;

                StartPriceAnimation(Best_Ofr_CN, "bdUpSideOfr", item, false);

            }
            #endregion

            #region bid
            if (bestPrice != null && bestPrice.BidBest != null)
            {
                // 最优bid price
                if (CommonUtils.IsNumeric(bestPrice.BidBest.bestPx))
                {
                    decimal bestBidPx = isFuture ? CommonUtils.ConvertToDecimal(CommonUtils.formatString(bestPrice.BidBest.bestPx, 3)) : CommonUtils.ConvertToDecimal(CommonUtils.FormatNum(bestPrice.BidBest.bestPx));
                    item[BID_BEST_FIELD] = bestBidPx;
                    item[BID_BEST_SORT_FIELD] = bestBidPx;

                    string bestBidPx_string = bestBidPx.ToString("N4");
                    int index = bestBidPx_string.IndexOf(".");
                }
                else if (!string.IsNullOrEmpty(bestPrice.BidBest.bestPx))
                {
                    item[BID_BEST_FIELD] = bestPrice.BidBest.bestPx;
                    item[BID_BEST_SORT_FIELD] = decimal.MinValue;
                }
                else
                {
                    item[BID_BEST_FIELD] = 0;
                    item[BID_BEST_SORT_FIELD] = decimal.Zero;
                }

                item[BID_BEST_SETTLE_FWD_FIELD] = (string.IsNullOrEmpty(setttle) || setttle == CommonConstants.ALL_PARTY || setttle == CommonConstants.FWD) ? bestPrice.BidBest.bestYuan : string.Empty;
                // 最优bid qty
                item[BID_BEST_QTY_FIELD] = bestPrice.BidBest.bestQty == 0 ? CommonConstants.NO_PRICE : bestPrice.BidBest.bestQty.ToString();
                // 清算速度
                item[BID_BEST_SETTLE_FIELD] = (string.IsNullOrEmpty(setttle) || setttle == CommonConstants.ALL_PARTY || setttle == CommonConstants.T1_EN) ? bestPrice.BidBest.bestT0 : string.Empty;

                if (needRefreshAnimation)
                    StartPriceAnimation(Best_Bid_CN, "bdUpSideBid", item, true);
                else
                    StartPriceAnimation(Best_Bid_CN, "bdUpSideBid", item, false);
            }
            else
            {
                item[BID_BEST_FIELD] = item[BID_BEST_QTY_FIELD] = CommonConstants.NO_PRICE;
                item[BID_BEST_SORT_FIELD] = decimal.MinValue;
                item[BID_BEST_SETTLE_FWD_FIELD] = string.Empty;
                item[BID_BEST_SETTLE_FIELD] = string.Empty;

                StartPriceAnimation(Best_Bid_CN, "bdUpSideBid", item, false);
            }
            #endregion

            

        }

        #endregion

        #endregion


        #region Filter

        StringBuilder filter_orders = new StringBuilder();
        /// <summary>
        /// 处理我的订单的过滤条件
        /// 全局过滤条件设置
        /// </summary>
        /// <param name="filterDic"></param>
        public void ProcessMyOrdersFilterCondition(Dictionary<string, IList> filterDic)
        {
            lock (lockObj)
            {
                _currentFilterDic = PriceCalHelper.Instance.CopyDictionary(filterDic);//filterDic;
                filter_orders.Clear();

                // Broker
                if (filterDic.ContainsKey(CommonConstants.BROKER))
                {
                    IList venuesList = filterDic[CommonConstants.BROKER];
                    StringBuilder inFilterSB_venues = ConditionHelper.GetBrokerCondition(venuesList);

                    if (inFilterSB_venues.Length > 0)
                    {
                        if (filter_orders.Length > 0)
                        {
                            filter_orders.Append($" and ({inFilterSB_venues})");
                        }
                        else
                        {
                            filter_orders.Append($"({inFilterSB_venues})");
                        }
                    }
                }


                //过滤CTPriceControler

                if (filterDic != null && filterDic.ContainsKey(CommonConstants.SETTLE))
                {
                    StringBuilder bestCacheSettleFitlerBuf = new StringBuilder();
                    IList settlTypeList = filterDic[CommonConstants.SETTLE];
                    if (settlTypeList.Count > 0)
                    {
                        _rowSettleFilterDic.Clear();

                        bestCacheSettleFitlerBuf = ConditionHelper.GetSettleCondition(settlTypeList);
                    }

                    if (bestCacheSettleFitlerBuf.Length > 0)
                    {
                        if (filter_orders.Length > 0)
                        {
                            filter_orders.Append($" and ({bestCacheSettleFitlerBuf})");
                        }
                        else
                        {
                            filter_orders.Append($"({bestCacheSettleFitlerBuf})");
                        }
                    }
                }
                else
                {
                    foreach (DataRow dr in this.PriceTableView.Table.Rows)
                    {
                        string rowKey = dr[KEY_FIELD] as string;
                        string settle = dr[SETTLE_TYPE_FIELD] as string;
                        string filterStr = CTPriceControler.GetAloneSettleFilter(settle);

                        if (_currentFilterDic != null && _currentFilterDic.ContainsKey(CommonConstants.BROKER))
                        {
                            IList venuesList = _currentFilterDic[CommonConstants.BROKER];
                            StringBuilder sbBroker = ConditionHelper.GetBrokerCondition(venuesList);

                            if (sbBroker.Length > 0)
                            {
                                filterStr = !string.IsNullOrEmpty(filterStr) ? string.Format("{0} and ({1})", filterStr, sbBroker) : sbBroker.ToString();
                            }
                        }

                        _rowSettleFilterDic[rowKey] = filterStr;
                    }
                }

                if (filter_orders.Length > 0)
                {
                    _rowFilterStr = filter_orders.ToString();
                }
                else
                    _rowFilterStr = string.Empty;

                StringBuilder sbTable = ConditionHelper.GetPriceTableViewCondition(filterDic);

                PriceTableView.RowFilter = sbTable.ToString();

                FilterOrderData();
            }
            
        }

        private bool _isChangeMainSettle = false;
        public bool IsChangeMainSettle { get { return _isChangeMainSettle; } set { _isChangeMainSettle = value; } }

        private bool _isClickRowSettle = false;

        public bool IsClickRowSettle
        {
            get { return _isClickRowSettle; }
            set { _isClickRowSettle = value; }
        }

        /// <summary>
        /// 过滤我的订单数据
        /// </summary>
        private void FilterOrderData()
        {

            StringBuilder tempFilter = new StringBuilder();
            if (this.SearchText.Length > 0)
            {
                tempFilter.Append($"{SECURITY_ID_FIELD} like '%{this.SearchText}%'");
            }


            if (_currentFilterDic != null && _currentFilterDic.ContainsKey("Settle"))
            {
                IList settlTypeList = _currentFilterDic["Settle"];
                if (settlTypeList.Count > 0)
                {
                    string showSettle = PriceCalHelper.Instance.GetShowSettle(settlTypeList);

                    DataRowCollection rows = this.PriceTableView.Table.Rows;
                    _isChangeMainSettle = true;


                    for (int i = 0; i < rows.Count; i++)
                    {
                        rows[i][SETTLE_TYPE_FIELD] = showSettle;

                    }

                    CalBestPrice(string.Empty, false);

                }
            }
            else
                CalBestPrice(string.Empty, false);


            _isChangeMainSettle = false;
            //CalBestPrice(string.Empty, false);

        }

        #endregion

        //手动修改过滤条件时触发
        private void RefreshLastExe(string security)
        {
            ExecutionReport temOrder = GetFirstExecutionReport(security);

            this.Processer_OnLastExecutions(temOrder);
        }

        private ExecutionReport GetFirstExecutionReport(string security)
        {
            ExecutionReport temOrder = null;
            // 市场最新成交
            MarketExecutionsItem lastExeItem = LastExecutionProcesser.Processer.GetLastExecutionsItem(security);
            if (lastExeItem != null)
            {

                temOrder = new ExecutionReport();

                temOrder.SecurityID = security;
                temOrder.Side = lastExeItem.Side == CommonConstants.SIDE_X ? CommonConstants.SIDE_OFR_TWO : lastExeItem.Side == CommonConstants.SIDE_Y ? CommonConstants.SIDE_BID_ONE : lastExeItem.Side;
                temOrder.LastPx = lastExeItem.ExecutePricePercent.ToString();
                temOrder.Info1 = lastExeItem.Info1;
                temOrder.Venues = lastExeItem.SourceTrader;
            }

            return temOrder;
        }

        public List<string> GetAllSettle(string selectId)
        {
            List<string> result = new List<string>();
            lock (lockObj)
            {
                foreach (DataRow dr in this.PriceTableView.Table.Rows)
                {
                    string settle = dr[SETTLE_TYPE_FIELD] as string;
                    string id = dr[KEY_FIELD] as string;

                    SettleCache.Instance.AddOrUpdate(this.GroupID, id, settle);

                    if (id != selectId && !result.Contains(settle))
                        result.Add(settle);
                }

                RememberHelper.Instance.WriteCustomSettle(GroupID);
            }

            return result;
        }

        public void SetRemenberSettle(Dictionary<string, IList> conditionDic)
        {
            _currentFilterDic = conditionDic;
            ConcurrentDictionary<string, string> securityIdDic = SettleCache.Instance.GetSettleByGroupId(GroupID);
            if (securityIdDic != null)
            {
                lock (lockObj)
                {
                    _isChangeMainSettle = true;
                    foreach (KeyValuePair<string, string> kv in securityIdDic)
                    {
                        DataRow dr = PriceTableView.Table.Rows.Find(kv.Key);
                        if (dr != null)
                        {
                            this.OnRowSettleChange(dr, kv.Value);
                        }
                    }
                    _isChangeMainSettle = false;
                }
                
            }
        }

        //单行计算
        public void OnRowSettleChange(DataRow drv, string setttle)
        {
            if (drv != null)
            {
                lock (lockObj)
                {
                    string rowKey = drv[KEY_FIELD] as string;
                    string securityID = drv[SECURITY_ID_FIELD] as string;
                    //string settle = drv[SETTLE_TYPE_FIELD] as string;
                    string filterStr = CTPriceControler.GetAloneSettleFilter(setttle);

                    if (_currentFilterDic != null && _currentFilterDic.ContainsKey(CommonConstants.BROKER))
                    {
                        IList venuesList = _currentFilterDic[CommonConstants.BROKER];
                        StringBuilder sbBroker = ConditionHelper.GetBrokerCondition(venuesList);

                        if (sbBroker.Length > 0)
                        {
                            filterStr = !string.IsNullOrEmpty(filterStr) ? string.Format("{0} and ({1})", filterStr, sbBroker) : sbBroker.ToString();
                        }
                    }

                    _rowSettleFilterDic[rowKey] = filterStr;
                    

                    BilateralBestPrice bestPrice = PriceHandler.Instance.CalBestPrice(securityID, filterStr, string.Empty);

                    //    // 中债估值
                    if(!CommonUtils.HasCDCAuth(securityID))
                    {
                        drv[VALUATION_FIELD] = CommonConstants.NO_PERMISSION;
                    }
                    else
                    {
                        string valuation = PriceHandler.Instance.GetValuation(securityID, string.Empty);
                        drv[VALUATION_FIELD] = CommonUtils.FormatNum(valuation);
                    }
                    
                    //}
                    drv[SETTLE_TYPE_FIELD] = setttle;
                    // 更新表格
                    BindBestPriceToUI(drv, bestPrice, false);

                    // 更新最优价是否和我的订单一样标志，同时更新角标
                    //BindIsSameTagToUI(securityID, drv);

                }
            }
        }

        private void OnDeleteCommand(object item)
        {
            if (item != null)
            {
                DataRow[] foundRows = PriceTableView.Table.Select($"SecurityDbID = '{item as string}'");
                if (foundRows.Length > 0)
                {
                    DataRow row = foundRows[0];
                    string id = row[SECURITY_DB_ID_FIELD] as string;
                    string security = row[SECURITY_ID_FIELD] as string;
                    string symbol = row[SYMBOL_FIELD] as string;
                    string text = string.Format("是否确定删除该债券({0} {1})？", security, symbol);
                    string title = "删除债券";
                    if (CustomMessageBox.Question(text, title, null, 164, 316) == true)
                    {
                        HomePageDataHandler.Instance.DelBond(DisplayMode.List, this.GroupID, id, security);
                    }
                }
            }
        }

        public void Drop(int prevRowIndex, int index, DataGrid dgData)
        {
            lock (lockObj)
            {
                DataRow newRow = PriceTableView.Table.NewRow();

                DataTable dataTable = PriceTableView.ToTable();
                CommonUtils.CopyDataRow(dataTable.Rows[prevRowIndex], newRow);

                if (prevRowIndex > index)
                    newRow[CUSTOMER_SORT_FIELD] = index - 1;
                else
                    newRow[CUSTOMER_SORT_FIELD] = index;

                string removeKey = dataTable.Rows[prevRowIndex][KEY_FIELD] as string;

                DataRow removeRow = null;

                foreach (DataRow dr in PriceTableView.Table.Rows)
                {
                    if (dr[KEY_FIELD].ToString() == removeKey)
                    {
                        removeRow = dr;
                        break;
                    }
                }

                if (removeRow != null)
                {
                    PriceTableView.Table.Rows.Remove(removeRow);
                    PriceTableView.Table.Rows.Add(newRow);

                    dataTable = PriceTableView.ToTable();

                    int flag = -1;
                    
                    foreach (DataRow dr in dataTable.Rows)
                    {
                        flag++;

                        dr[CUSTOMER_SORT_FIELD] = flag;
                        string id = dr[KEY_FIELD] as string;

                        SortCache.Instance.AddOrUpdate(GroupID, id, flag);
                    }


                    foreach (DataRow dr in PriceTableView.Table.Rows)
                    {
                        string id = dr[KEY_FIELD] as string;

                        int sort = SortCache.Instance.GetSettleById(GroupID, id);

                        dr[CUSTOMER_SORT_FIELD] = sort;
                    }
                    
                    dgData.SelectedIndex = index;

                    string sortStr = RememberHelper.Instance.WriteCustomSort(this.GroupID);//this.WriteCustomSort();

                    BondChangeHandler.Instance.OnSortChanged(OperationType.TableToCard, this.GroupID);
                }
            }
            
        }

        private void Instance_OnSortChange(OperationType operationType, string groupdId)
        {
            if (GroupID == groupdId && operationType == OperationType.CardToTable)
            {
                lock (lockObj)
                {
                    foreach (DataRow dr in PriceTableView.Table.Rows)
                    {
                        string id = dr[KEY_FIELD] as string;

                        int sort = SortCache.Instance.GetSettleById(GroupID, id);

                        dr[CUSTOMER_SORT_FIELD] = sort;
                    }
                }
            }
        }

        public void UserCustomerSort()
        {
            ConcurrentDictionary<string, int> dic = SortCache.Instance.GetSettleByGroupId(this.GroupID);
            if (dic != null)
            {
                lock (lockObj)
                {
                    foreach (DataRow dr in PriceTableView.Table.Rows)
                    {
                        string id = dr[KEY_FIELD] as string;
                        if (dic.ContainsKey(id))
                        {
                            dr[CUSTOMER_SORT_FIELD] = dic[id];
                        }
                    }

                    BindingListCollectionView cv = (BindingListCollectionView)CollectionViewSource.GetDefaultView(PriceTableView);

                    cv.SortDescriptions.Clear();
                    
                    PriceTableView.Sort = $"{CUSTOMER_SORT_FIELD} asc";
                }
            }
        }


        public int GetRowIndex(DataGridRow dataGridRow)
        {
            return PriceTableView.Table.Rows.IndexOf((dataGridRow.Item as DataRowView).Row);
        }

        /// <summary>
        /// 接收个人设置 弹窗设置 改变事件
        /// </summary>
        /// <param name="obj"></param>
        private void Instance_OnSettingChange(object obj)
        {
            if (obj is NewSettingMsg)
            {
                NewSettingMsg msg = (NewSettingMsg)obj;
                if (msg.key == "SettingBpMode")
                {
                    lock (lockObj)
                    {
                        foreach (DataRow dr in PriceTableView.Table.Rows)
                        {
                            string security = dr[SECURITY_ID_FIELD] as string;

                            ExecutionReport temOrder = GetFirstExecutionReport(security);

                            if (temOrder != null)
                            {
                                UpdateLastMarketExecutions(temOrder, dr);
                            }
                        }
                    }
                }
            }
        }

        public async void ExportTXT()
        {
            int totalSize = 0;
            int successCount = 0;
            int failCount = 0;

            //this.ExportEnabled = false;
            System.Windows.Forms.SaveFileDialog saveFileDialog = new System.Windows.Forms.SaveFileDialog();
            try
            {
                saveFileDialog.Filter = "(文本文件*.txt)|*.txt";
                saveFileDialog.FileName = "债券";
                saveFileDialog.AddExtension = true;
                saveFileDialog.RestoreDirectory = true;
                if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    string fileName = saveFileDialog.FileName;
                    using (StreamWriter writer = CSVHelper.Instance.createStream(fileName))
                    {
                        await Task.Run(() =>
                        {
                            foreach (DataRowView item in PriceTableView)
                            {
                                try
                                {
                                    writer.WriteLine($"{item[SECURITY_ID_FIELD]} {item[SYMBOL_FIELD]}");
                                    successCount++;
                                }
                                catch (Exception e)
                                {
                                    LogHelper.Error(e);
                                    failCount++;
                                }
                                totalSize++;
                            }
                        });
                        // 释放资源
                        writer.Flush();
                        writer.Close();
                    }
                    CustomMessageBox.Info($"《{saveFileDialog.FileName}》已保存。总条数：{totalSize}   成功：{successCount}  失败：{failCount}");
                }
            }
            catch (Exception e)
            {
                LogHelper.Error(e);
            }
            finally
            {
                //this.ExportEnabled = true;
            }
        }
    }
}
