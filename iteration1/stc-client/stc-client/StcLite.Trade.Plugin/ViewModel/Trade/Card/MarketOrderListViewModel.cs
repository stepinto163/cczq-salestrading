using com.gateway.log;
using BDB.Trade.Plugin.Handler;
using BDB.Trade.Plugin.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using STC.Common.Model;
using STC.Common.Utils;
using STC.Common.Model.Base;
using STC.Common.Handler.Base;

using STC.Common.Model.Fix.Message;
using STC.Common.Data;
using ExecutionsHandler = BDB.Trade.Plugin.Handler.ExecutionsHandler;
using BDB.Common.Model.OHLC;

namespace BDB.Trade.Plugin.ViewModel.Trade.Card
{
    public class MarketOrderListViewModel : BaseViewModel
    {
        private object _lockObj = new object();
        #region 表格字段
        public const string KEY_FIELD = "Key";//Source + price
        public const string SECURITY_ID_FIELD = "SecurityID";
        public const string SETTLE_TYPE_FIELD = "SettleType";

        public const string BID_SETTLE_TYPE_FIELD = "BidSettleType";
        public const string OFR_SETTLE_TYPE_FIELD = "OfrSettleType";

        public const string SETTLE_FWD_TYPE_FIELD = "SettleFwdType";
        public const string SHOW_SETTLE_TYPE_FILED = "ShowSettleType";
        public const string BID_FIELD = "Bid";
        public const string BID_AMOUNT_FIELD = "BidAmount";
        public const string OFR_FIELD = "Ofr";
        public const string OFR_AMOUNT_FIELD = "OfrAmount";
        public const string SOURCE_FIELD = "Venues";//"Source";
        public const string UPDATE_TIME_FIELD = "UpdateTime";
        public const string DELETE_FLAG_FIELD = "DeleteFlag";

        public const string TRADE_PERMISSION_FLAG_FIELD = "TradePermissionFlag";

        public const string SHOW_SETTLE_COLUMN_FLAG_FILED = "ShowSettleColumnFlag";

        public const string BID_CORNER_MARKET_FIELD = "BidCornerMarket";
        public const string OFR_CORNER_MARKET_FIELD = "OfrCornerMarket";

        public const string SORT_FIELD = "BrokerSort";

        public const string BID_CORNER_MARKET_TEXT_FIELD = "BidCornerMarketText";
        public const string OFR_CORNER_MARKET_TEXT_FIELD = "OfrCornerMarketText";

        public const string BID_BARGAIN_FLAG_FIELD = "BidBargainFlag";
        public const string OFR_BARGAIN_FLAG_FIELD = "OfrBargainFlag";
        public const string ALL_IS_NULL_FLAG_FIELD = "AllIsNullFlag";//标识securityId为空时的显示标志



        private DataView _bidTableView;
        public DataView BidTableView
        {
            get
            {
                lock (_lockObj)
                {
                    if (_bidTableView == null)
                    {
                        _bidTableView = new DataTable().DefaultView;
                        _bidTableView.Table.Columns.Add(KEY_FIELD, typeof(string));
                        _bidTableView.Table.Columns.Add(SECURITY_ID_FIELD, typeof(string));

                        _bidTableView.Table.Columns.Add(BID_SETTLE_TYPE_FIELD, typeof(string));
                        _bidTableView.Table.Columns.Add(OFR_SETTLE_TYPE_FIELD, typeof(string));

                        _bidTableView.Table.Columns.Add(SORT_FIELD, typeof(string));
                        _bidTableView.Table.Columns.Add(SETTLE_TYPE_FIELD, typeof(string));
                        _bidTableView.Table.Columns.Add(SHOW_SETTLE_TYPE_FILED, typeof(string));
                        _bidTableView.Table.Columns.Add(SETTLE_FWD_TYPE_FIELD, typeof(string));
                        _bidTableView.Table.Columns.Add(BID_FIELD, typeof(string));
                        _bidTableView.Table.Columns.Add(BID_AMOUNT_FIELD, typeof(string));

                        _bidTableView.Table.Columns.Add(TRADE_PERMISSION_FLAG_FIELD, typeof(string));
                        _bidTableView.Table.Columns.Add(SHOW_SETTLE_COLUMN_FLAG_FILED, typeof(string));

                        _bidTableView.Table.Columns.Add(SOURCE_FIELD, typeof(string));
                        _bidTableView.Table.Columns.Add(UPDATE_TIME_FIELD, typeof(string));
                        _bidTableView.Table.Columns.Add(DELETE_FLAG_FIELD, typeof(string));
                        _bidTableView.Table.Columns.Add(BID_CORNER_MARKET_FIELD, typeof(string));
                        _bidTableView.Table.Columns.Add(BID_CORNER_MARKET_TEXT_FIELD, typeof(string));

                        _bidTableView.Table.Columns.Add(OFR_FIELD, typeof(string));
                        _bidTableView.Table.Columns.Add(OFR_AMOUNT_FIELD, typeof(string));

                        _bidTableView.Table.Columns.Add(OFR_CORNER_MARKET_FIELD, typeof(string));
                        _bidTableView.Table.Columns.Add(OFR_CORNER_MARKET_TEXT_FIELD, typeof(string));

                        _bidTableView.Table.Columns.Add(BID_BARGAIN_FLAG_FIELD, typeof(string));
                        _bidTableView.Table.Columns.Add(OFR_BARGAIN_FLAG_FIELD, typeof(string));

                        _bidTableView.Table.Columns.Add(ALL_IS_NULL_FLAG_FIELD, typeof(string));

                        _bidTableView.Sort = $"{SORT_FIELD} asc";
                        _bidTableView.Table.PrimaryKey = new DataColumn[] {
                            _bidTableView.Table.Columns[KEY_FIELD],
                        };
                        this.OnPropertyChanged();
                    }
                    return this._bidTableView;
                }
            }
        }



        #endregion




        private Visibility ohlcVisibility = Visibility.Visible;

        public Visibility OhlcVisibility
        {
            get { return ohlcVisibility; }
            set
            {
                ohlcVisibility = value;

                this.OnPropertyChanged();
            }
        }


        #region Other
        private string _orderCount = CommonConstants.NO_PRICE;

        public string OrderCount
        {
            get
            {
                return _orderCount;
            }

            set
            {
                _orderCount = value;
                this.OnPropertyChanged();

            }
        }


        private string _closeYield = CommonConstants.NO_PRICE;

        public string CloseYield
        {
            get
            {
                return _closeYield;
            }

            set
            {
                _closeYield = value;
                this.OnPropertyChanged();

            }
        }

        private string _highYield = CommonConstants.NO_PRICE;

        public string HighYield
        {
            get
            {
                return _highYield;
            }

            set
            {
                _highYield = value;
                this.OnPropertyChanged();

            }
        }

        private string _lowYield = CommonConstants.NO_PRICE;

        public string LowYield
        {
            get
            {
                return _lowYield;
            }

            set
            {
                _lowYield = value;
                this.OnPropertyChanged();

            }
        }

        private string _openYld = CommonConstants.NO_PRICE;

        public string OpenYld
        {
            get
            {
                return _openYld;
            }

            set
            {
                _openYld = value;
                this.OnPropertyChanged();

            }
        }


        private string _searchText = string.Empty;

        public string SearchText
        {
            get
            {
                return _searchText;
            }

            set
            {
                //if (_searchText != value)
                {
                    _searchText = value;
                    this.OnPropertyChanged(nameof(SearchText));
                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        //GetData(0);
                    }));
                }
            }
        }
        #endregion


        public void ClearData(bool isShowLabel)
        {
            lock (this._lockObj)
            {
                this.OpenYld = this.LowYield = this.OrderCount = this.HighYield = this.CloseYield = CommonConstants.NO_PRICE;
               
                if (!isShowLabel)
                {
                    OhlcVisibility = Visibility.Collapsed;

                    this.SearchText = string.Empty;
                }
                else
                {
                    OhlcVisibility = Visibility.Visible;
                }
            

                if (BidTableView != null)
                {
                    BidTableView.Table.Clear();
                }
            }
        }

        private void Instance_OnClear(ClearPo obj)
        {
            LogHelper.Info(string.Format("MarketOrderViewListModel Clear, security:{0}", this.SearchText));
            if (obj != null && "Y".Equals(obj.IsClear))
            {

                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    ClearData(true);
                }));

            }
        }

        public void Init()
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                if (!string.IsNullOrEmpty(SearchText))
                {
                    if (ohlcVisibility == Visibility.Collapsed)
                    {
                        OhlcVisibility = Visibility.Visible;
                    }
                }

                BaseHandler.Instance.Init(Instance_OnClear);

                try
                {
                    #region 收MarketExec消息0
                    ExecutionsHandler.Instance.OnMarketExecutions += Instance_OnMarketExecutions;
                    #endregion


                    #region 订阅OHLC

                    OhlcHandler.Instance.SubscribeOhlc(this.SearchText, onOHLC);

                    #endregion

                }
                catch (Exception e)
                {
                    LogHelper.Error(e);
                }
            }));
        }

        public void onOHLC(OhlcMsg ohlcMsg)
        {

            if (ohlcMsg.securityid.ToLower() == SearchText.ToLower())
            {
                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    if (ohlcMsg.close != 0)
                        this.CloseYield = ohlcMsg.close.ToString();

                    if (ohlcMsg.high != 0)
                        this.HighYield = ohlcMsg.high.ToString();

                    if (ohlcMsg.numTrades != 0)
                        this.OrderCount = ohlcMsg.numTrades.ToString();

                    if (ohlcMsg.low != 0)
                        this.LowYield = ohlcMsg.low.ToString();

                    if (ohlcMsg.open != 0)
                        this.OpenYld = ohlcMsg.open.ToString();

                    if (ohlcVisibility == Visibility.Collapsed)
                    {
                        OhlcVisibility = Visibility.Visible;
                    }
                }));
            }
        }


        /// <summary>
        /// 收取成交数据
        /// </summary>
        /// <param name="obj"></param>
        private void Instance_OnMarketExecutions(ExecutionReport obj)
        {
            if (obj.SecurityID.ToLower() == SearchText.ToLower())
            {
                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                }));
            }
        }


    }
}
