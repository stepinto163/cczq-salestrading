using STC.Common.Handler.Base;
using STC.Common.Model.Admin;
using STC.Common.Model.Base;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.TBook;
using STC.Common.DataProcesser;
using STC.Common.ViewModel.Data;
using STC.Frame.Common;
using STC.XBond.Plugin.Utils;
using STC.XBond.Plugin.Utils.DataProcesser;
using STC.XBond.Plugin.View;
using STC.XBond.Plugin.View.Component;
using STC.XBond.Plugin.View.HomePage;
using STC.XBond.Plugin.ViewModel.Data;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using STC.Common.ViewModel;
using STC.Common.Utils;
using STC.Common.Data;
using STC.Common.Model;
using System.Threading.Tasks;
using STC.Common.Handler.Admin;
using System.Diagnostics;
using STC.Common.View.Order;
using com.gateway.log;
using System.IO;
using Newtonsoft.Json;
using STC.Frame.Utils.ExcelHelper;
using STC.Common.Api;

namespace STC.XBond.Plugin.ViewModel.HomePage
{
    /// <summary>
    /// 主页FavoriteBondsListBoxView的数据内容
    /// </summary>
    public class FavoriteBondsListBoxViewModel : ViewModelBase
    {
        public string groupID = string.Empty;

        private object lockObj = new object();

        #region 表格字段
        private const string KEY_FIELD = "Key";
        public const string SECURITY_ID_FIELD = "securityID";
        public const string SYMBOL_FIELD = "symbol";
        private const string EXEC_VALUE_FIELD = "ExecValue";
        //private const string OFR_PRICE_FILED = "OfrPrice";
        //private const string OFR_QTY_FIELD = "OfrQty";
        //private const string BEST_OFR_TRADER_FIELD = "BestOfrTrader";
        //private const string BEST_BID_TRADER_FIELD = "BestBidTrader";
        //private const string BID_QTY_FIELD = "BidQty";
        public const string BEST_BID_PRICE_FIELD = "BestBidPrice";
        private const string BEST_BID_PRICE_LEFT_FIELD = "BestBidPx_Left";
        private const string BEST_BID_PRICE_RIGHT_FIELD = "BestBidPx_Right";
        public const string BEST_BID_QTY_FIELD = "BestBidQty";
        public const string BEST_OFR_PRICE_FIELD = "BestOfrPrice";
        private const string BEST_OFR_PRICE_LEFT_FIELD = "BestOfrPx_Left";
        private const string BEST_OFR_PRICE_RIGHT_FIELD = "BestOfrPx_Right";
        public const string BEST_OFR_QTY_FIELD = "BestOfrQty";
        private const string LAST_ACTION_STATUS_FIELD = "LastActionStatus";
        private const string LAST_ACTION_TIME_FIELD = "LastActionTime";
        private const string CREATETIME_FIELD = "CreateTime";
        //private const string VENUE_FIELD = "Venue";
        //private const string VENUE_TYPEGW_FIELD = "VenueTypeGW";
        public const string REMAINGING_TIME_FIELD = "RemainingTime";
        private const string REMAINGING_TIME_CONVERT_FIELD = "RemaingingTime_convert";
        //private const string SWITCH_FIELD = "Switch";
        //private const string BID_PRICE_FIELD = "BidPrice";
        //private const string BID_SETTLE_TYPE_FIELD = "BidSettleType";
        //private const string OFR_SETTLE_TYPE_FIELD = "OfrSettleType";
        private const string ISSUER_RATING_CURRENT_NPY_FIELD = "IssuerRatingCurrentNPY";
        private const string ID_FIELD = "id";
        //private const string BID_ORDER_TIME_FIELD = "BidOrderTime";
        //private const string OFR_ORDER_TIME_FIELD = "OfrOrderTime";
        private const string BOND_TYPE_FIELD = "BondType";
        private const string BOND_SUB_TYPE_FIELD = "BondSubType";
        private const string GOODS_TYPE_FIELD = "GoodsType";
        private const string ENT_COR_FIELD = "Ent_Cor";
        private const string BOND_CATEGORY_FIELD = "bond_category";
        private const string LAST_MARKET_PX_VENUES_FIELD = "LastMarketPx_Venues";
        private const string DISPLAY_LAST_MARKET_PX_FIELD = "Display_LastMarketPx";
        private const string LAST_MARKET_PX_FIELD = "LastMarketPx";
        public const string VALUATION_FIELD = "Valuation";
        private const string IS_BID_SAME_FIELD = "IsBidSame";
        private const string IS_OFR_SAME_FIELD = "IsOfrSame";
        private const string BEST_BID_SETTLTYPE_TAG_FIELD = "BestBidSettlTypeTag";
        private const string BEST_BID_SETTLTYPE_YUAN_TAG_FIELD = "BestBidYuanSettlTypeTag";
        private const string BEST_OFR_SETTLTYPE_TAG_FIELD = "BestOfrSettlTypeTag";
        private const string BEST_OFR_SETTLTYPE_YUAN_TAG_FIELD = "BestOfrYuanSettlTypeTag";
        private const string LAST_SETTLTYPE_TAG_FIELD = "LastSettlTypeTag";
        private const string BEST_BID_FLAGTYPE_FIELD = "BestBidFlagType";
        private const string BEST_OFR_FLAGTYPE_FIELD = "BestOfrFlagType";
        // 角标判定标识（来自行情数据：本人/本机构是否有报价），由 BindBestPriceToUI 写入，GenerateFlagType 读取
        private const string BID_HAS_ME_FIELD = "BidHasMe";
        private const string BID_HAS_OUR_FIELD = "BidHasOur";
        private const string OFR_HAS_ME_FIELD = "OfrHasMe";
        private const string OFR_HAS_OUR_FIELD = "OfrHasOur";
        //private const string OTHER_FIELD = "Other";
        private const string ISALLFIRM_BID = "IsAllFirm_Bid";
        private const string ISALLFIRM_OFR = "IsAllFirm_Ofr";


        public const string MY_BID_FIELD = "MyBid";
        public const string MY_BID_QTY_FIELD = "MyBidQty";
        public const string MY_OFR_FIELD = "MyOfr";
        public const string MY_OFR_QTY_FIELD = "MyOfrQty";

        public const string BID_TXT_FIELD = "BidTxt";
        public const string BID_MAX_T1_QTY_FIELD = "BidMaxT1Qty";
        public const string BID_BBO_FIELD = "BidBBO";
        public const string OFR_TXT_FIELD = "OfrTxt";
        public const string OFR_MAX_T1_QTY_FIELD = "OfrMaxT1Qty";
        public const string OFR_BBO_FIELD = "OfrBBO";

        public const string KLINE_WIDTH_FIELD = "KlineWidth";
        public const string MARKET_INDICATOR_FIELD = "MarketIndicator";

        #endregion

        private DataView _BondView;
        public DataView BondView
        {
            get
            {
                lock (lockObj)
                {
                    if (_BondView == null)
                    {
                        _BondView = new DataTable().DefaultView;
                        _BondView.Table.Columns.Add(KEY_FIELD, typeof(string));
                        _BondView.Table.Columns.Add(SECURITY_ID_FIELD, typeof(string));
                        _BondView.Table.Columns.Add(SYMBOL_FIELD, typeof(string));
                        _BondView.Table.Columns.Add(EXEC_VALUE_FIELD, typeof(decimal));
                        //_BondView.Table.Columns.Add(OFR_PRICE_FILED, typeof(decimal));
                        //_BondView.Table.Columns.Add(OFR_QTY_FIELD, typeof(long));
                        //_BondView.Table.Columns.Add(BEST_OFR_TRADER_FIELD, typeof(string));
                        //_BondView.Table.Columns.Add(BEST_BID_TRADER_FIELD, typeof(string));
                        //_BondView.Table.Columns.Add(BID_QTY_FIELD, typeof(long));
                        _BondView.Table.Columns.Add(BEST_BID_PRICE_FIELD, typeof(decimal));
                        _BondView.Table.Columns.Add(BEST_BID_PRICE_LEFT_FIELD, typeof(string));
                        _BondView.Table.Columns.Add(BEST_BID_PRICE_RIGHT_FIELD, typeof(string));
                        _BondView.Table.Columns.Add(BEST_BID_QTY_FIELD, typeof(decimal));
                        _BondView.Table.Columns.Add(BEST_OFR_PRICE_FIELD, typeof(decimal));
                        _BondView.Table.Columns.Add(BEST_OFR_PRICE_LEFT_FIELD, typeof(string));
                        _BondView.Table.Columns.Add(BEST_OFR_PRICE_RIGHT_FIELD, typeof(string));
                        _BondView.Table.Columns.Add(BEST_OFR_QTY_FIELD, typeof(decimal));
                        _BondView.Table.Columns.Add(LAST_ACTION_STATUS_FIELD, typeof(string));
                        _BondView.Table.Columns.Add(LAST_ACTION_TIME_FIELD, typeof(string));
                        _BondView.Table.Columns.Add(CREATETIME_FIELD, typeof(string));
                        //_BondView.Table.Columns.Add(VENUE_FIELD, typeof(string));
                        //_BondView.Table.Columns.Add(VENUE_TYPEGW_FIELD, typeof(string));
                        _BondView.Table.Columns.Add(REMAINGING_TIME_FIELD, typeof(string));
                        _BondView.Table.Columns.Add(REMAINGING_TIME_CONVERT_FIELD, typeof(int));
                        //_BondView.Table.Columns.Add(SWITCH_FIELD, typeof(bool));
                        //_BondView.Table.Columns.Add(BID_PRICE_FIELD, typeof(decimal));
                        //_BondView.Table.Columns.Add(BID_SETTLE_TYPE_FIELD, typeof(string));
                        //_BondView.Table.Columns.Add(OFR_SETTLE_TYPE_FIELD, typeof(string));
                        _BondView.Table.Columns.Add(ISSUER_RATING_CURRENT_NPY_FIELD, typeof(string));
                        _BondView.Table.Columns.Add(ID_FIELD, typeof(string));
                        //_BondView.Table.Columns.Add(BID_ORDER_TIME_FIELD, typeof(string));
                        //_BondView.Table.Columns.Add(OFR_ORDER_TIME_FIELD, typeof(string));
                        _BondView.Table.Columns.Add(BOND_TYPE_FIELD, typeof(string));
                        _BondView.Table.Columns.Add(BOND_SUB_TYPE_FIELD, typeof(string));
                        _BondView.Table.Columns.Add(GOODS_TYPE_FIELD, typeof(string));
                        _BondView.Table.Columns.Add(ENT_COR_FIELD, typeof(string));
                        _BondView.Table.Columns.Add(BOND_CATEGORY_FIELD, typeof(string));
                        _BondView.Table.Columns.Add(LAST_MARKET_PX_VENUES_FIELD, typeof(string));
                        _BondView.Table.Columns.Add(DISPLAY_LAST_MARKET_PX_FIELD, typeof(string));
                        _BondView.Table.Columns.Add(LAST_MARKET_PX_FIELD, typeof(decimal));
                        _BondView.Table.Columns.Add(VALUATION_FIELD, typeof(decimal));
                        _BondView.Table.Columns.Add(IS_BID_SAME_FIELD, typeof(string));
                        _BondView.Table.Columns.Add(IS_OFR_SAME_FIELD, typeof(string));
                        _BondView.Table.Columns.Add(BEST_BID_SETTLTYPE_TAG_FIELD, typeof(string));
                        _BondView.Table.Columns.Add(BEST_BID_SETTLTYPE_YUAN_TAG_FIELD, typeof(string));
                        _BondView.Table.Columns.Add(BEST_OFR_SETTLTYPE_TAG_FIELD, typeof(string));
                        _BondView.Table.Columns.Add(BEST_OFR_SETTLTYPE_YUAN_TAG_FIELD, typeof(string));
                        _BondView.Table.Columns.Add(LAST_SETTLTYPE_TAG_FIELD, typeof(string));
                        _BondView.Table.Columns.Add(BEST_BID_FLAGTYPE_FIELD, typeof(string));
                        _BondView.Table.Columns.Add(BEST_OFR_FLAGTYPE_FIELD, typeof(string));
                        _BondView.Table.Columns.Add(BID_HAS_ME_FIELD, typeof(bool));
                        _BondView.Table.Columns.Add(BID_HAS_OUR_FIELD, typeof(bool));
                        _BondView.Table.Columns.Add(OFR_HAS_ME_FIELD, typeof(bool));
                        _BondView.Table.Columns.Add(OFR_HAS_OUR_FIELD, typeof(bool));
                        //_BondView.Table.Columns.Add(OTHER_FIELD, typeof(string));
                        _BondView.Table.Columns.Add(ISALLFIRM_BID, typeof(string));
                        _BondView.Table.Columns.Add(ISALLFIRM_OFR, typeof(string));


                        _BondView.Table.Columns.Add(MY_OFR_FIELD, typeof(string));
                        _BondView.Table.Columns.Add(MY_OFR_QTY_FIELD, typeof(string));
                        _BondView.Table.Columns.Add(MY_BID_FIELD, typeof(string));
                        _BondView.Table.Columns.Add(MY_BID_QTY_FIELD, typeof(string));

                        _BondView.Table.Columns.Add(BID_TXT_FIELD, typeof(string));
                        _BondView.Table.Columns[BID_TXT_FIELD].DefaultValue = "- -";
                        _BondView.Table.Columns.Add(BID_MAX_T1_QTY_FIELD, typeof(string));
                        _BondView.Table.Columns[BID_MAX_T1_QTY_FIELD].DefaultValue = "- -";
                        _BondView.Table.Columns.Add(BID_BBO_FIELD, typeof(string));
                        _BondView.Table.Columns[BID_BBO_FIELD].DefaultValue = "- -";
                        _BondView.Table.Columns.Add(OFR_TXT_FIELD, typeof(string));
                        _BondView.Table.Columns[OFR_TXT_FIELD].DefaultValue = "- -";
                        _BondView.Table.Columns.Add(OFR_MAX_T1_QTY_FIELD, typeof(string));
                        _BondView.Table.Columns[OFR_MAX_T1_QTY_FIELD].DefaultValue = "- -";
                        _BondView.Table.Columns.Add(OFR_BBO_FIELD, typeof(string));
                        _BondView.Table.Columns[OFR_BBO_FIELD].DefaultValue = "- -";

                        _BondView.Table.Columns.Add(KLINE_WIDTH_FIELD, typeof(int));
                        _BondView.Table.Columns.Add(MARKET_INDICATOR_FIELD, typeof(string));

                        _BondView.Sort = $"{CREATETIME_FIELD} desc";
                        //_BondView.Table.PrimaryKey = new DataColumn[] {
                        //    _BondView.Table.Columns[KEY_FIELD],
                        //    _BondView.Table.Columns[SETTLE_TYPE_FIELD],
                        //    _BondView.Table.Columns[VENUE_FIELD],
                        //    _BondView.Table.Columns[VENUE_TYPEGW_FIELD]
                        //};
                        _BondView.Table.PrimaryKey = new DataColumn[] {
                            _BondView.Table.Columns[KEY_FIELD]
                        };
                        this.OnPropertyChanged(nameof(BondView));
                    }
                    return this._BondView;
                }
            }
        }

        //存储最新的CT行情， key: secutirtyID; Value: MarketDataSnapshotFullRefresh
        private Dictionary<string, MarketDataSnapshotFullRefresh> _LastCTPriceDic = new Dictionary<string, MarketDataSnapshotFullRefresh>();
        //存储CT初级处理数据（把MarketDataSnapshotFullRefresh转换成DataView数据结构）
        private ConcurrentDictionary<string, DataView> baseCTPriceDataViewDic = new ConcurrentDictionary<string, DataView>();

        //更新行情数据
        private Thread RefreshDataThread = null;
        private FrameworkElement _ownUI = null;
        private ThreadQueue<HomeGroupSecurityIDsInfo> _ProcessBondsThreadQueue;
        //private ThreadQueue<Common.Model.Fix.Message.MarketDataSnapshotFullRefresh> _ProcessDMPriceThreadQueue;
        private ThreadQueue<string>_ProcessMyOrdersThreadQueue;
        private AsyncThreadQueue<ExecutionReport> _ProcessLastExecutionsThreadQueue;

        private Thread _RefreshDefaultSelectThread = null;

        private Thread _RefreshBaseDataThread = null;

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

        public static ICommand CopyBidCommand { get; set; }
        public static ICommand CopyOfrCommand { get; set; }
        public static ICommand CopyDoubleCommand { get; set; }
        /// <summary>
        /// 导出
        /// </summary>
        public ICommand ExportCommand { get; set; }

        /// <summary>
        /// 导入
        /// </summary>
        public ICommand ImportCommand { get; set; }


        private bool _ExportEnabled = true;

        public bool ExportEnabled
        {
            get
            {
                return _ExportEnabled;
            }
            set
            {
                this._ExportEnabled = value;
                this.OnPropertyChanged();
            }
        }

        private bool _ImportEnabled = true;

        public bool ImportEnabled
        {
            get
            {
                return _ImportEnabled;
            }
            set
            {
                this._ImportEnabled = value;
                this.OnPropertyChanged();
            }
        }

        private Visibility _MaskVisibility = Visibility.Collapsed;

        public Visibility MaskVisibility
        {
            get
            {
                return this._MaskVisibility;
            }

            set
            {
                this._MaskVisibility = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// 自定义债券归类， 便于筛选
        /// A: 国债
        /// B: 央票
        /// C: 金融债
        /// D: 地方政府债
        /// E: 短融
        /// F: 中票
        /// G: 企业债
        /// H: 公司债
        /// I:   NCD
        /// J:PPN
        /// K:ABS
        /// L:CRM
        /// </summary>
        /// <param name="bond_subType"></param>
        /// <param name="ent_cor"></param>
        /// <returns></returns>
        public static string GetBondCategory(string bond_subType, string ent_cor)
        {
            string bondCategoryName = CommonUtils.GetBondCategoryName(bond_subType, ent_cor);
            switch(bondCategoryName)
            {
                case "国债":
                    return "A";
                case "央票":
                    return "B";
                case "金融债":
                    return "C";
                case "地方政府债":
                    return "D";
                case "短融":
                    return "E";
                case "中票":
                    return "F";
                case "企业债":
                    return "G";
                case "公司债":
                    return "H";
                case "NCD":
                    return "I";
                case "PPN":
                    return "J";
                case "ABS":
                    return "K";
                case "CRM":
                    return "L";
                default:
                    return string.Empty;
            }
        }

        StringBuilder filterSB = new StringBuilder();
        StringBuilder filterBondType = new StringBuilder();
        /// <summary>
        /// 生成债券类型过滤条件
        /// 利率债 BNC
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
                    if (lbi.Content.ToString() == "利率债")
                    {
                        inFilterSB.Remove(0, inFilterSB.Length);
                        notInFilter = $"{GOODS_TYPE_FIELD} = 'BNC'";
                        break;
                    }

                    if (lbi.Content.ToString() == "国债")
                    {
                        AddListFilter(inFilterSB, "'A'");
                    }
                    else if (lbi.Content.ToString() == "央票")
                    {
                        AddListFilter(inFilterSB, "'B'");
                    }
                    else if (lbi.Content.ToString() == "金融债")
                    {
                        AddListFilter(inFilterSB, "'C'");
                    }
                    else if (lbi.Content.ToString() == "地方政府债")
                    {
                        AddListFilter(inFilterSB, "'D'");
                    }
                }
                if (inFilterSB.Length > 0)
                {
                    if (filterBondType.Length > 0)
                    {
                        filterBondType.Append(" or ");
                    }
                    filterBondType.Append($"{BOND_CATEGORY_FIELD} in ({inFilterSB})");
                }
                if (notInFilter.Length > 0)
                {
                    if (filterBondType.Length > 0)
                    {
                        filterBondType.Append(" or ");
                    }
                    filterBondType.Append(notInFilter);
                }
                if (filterBondType.Length > 0)
                {
                    if (filterBondType.ToString().Contains("BNC") == false)
                    {
                        filterBondType.Append($" and { GOODS_TYPE_FIELD} = 'BNC'");
                    }
                }
                
            }
            FilterData();
        }


        StringBuilder filterCreditBondType = new StringBuilder();
        /// <summary>
        /// 生成信用债过滤条件
        /// 不是利率债BNC的都归为信用债（信用债:BCO，ABS，NCD）
        /// </summary>
        /// <param name="selectedItems"></param>
        public void FilterCreditBondType(System.Collections.IList selectedItems)
        {
            filterCreditBondType.Remove(0, filterCreditBondType.Length);
            if (selectedItems != null && selectedItems.Count > 0)
            {
                StringBuilder inFilterSB = new StringBuilder();
                string notInFilter = string.Empty;

                foreach (ListBoxItem lbi in selectedItems)
                {
                    if (lbi.Content.ToString() == "信用债")
                    {
                        inFilterSB.Remove(0, inFilterSB.Length);
                        notInFilter = $"({GOODS_TYPE_FIELD} <> 'BNC')";
                        break;
                    }
                    // 信用债
                    if (lbi.Content.ToString() == "短融")
                    {
                        AddListFilter(inFilterSB, "'E'");
                    }
                    else if (lbi.Content.ToString() == "中票")
                    {
                        AddListFilter(inFilterSB, "'F'"); 
                    }
                    else if (lbi.Content.ToString() == "企业债")
                    {
                       AddListFilter(inFilterSB, "'G'");
                    }
                    else if (lbi.Content.ToString() == "公司债")
                    {
                        AddListFilter(inFilterSB, "'H'");
                    }
                    else if (lbi.Content.ToString() == "NCD")
                    {
                        AddListFilter(inFilterSB, "'I'");
                    }
                    else if (lbi.Content.ToString() == "PPN")
                    {
                        AddListFilter(inFilterSB, "'J'");
                    }
                    else if (lbi.Content.ToString() == "ABS")
                    {
                        AddListFilter(inFilterSB, "'K'");
                    }
                    else if (lbi.Content.ToString() == "CRM")
                    {
                        AddListFilter(inFilterSB, "'L'");
                    }
                    else if (lbi.Content.ToString() == "其他")
                    {
                        AddListFilter(inFilterSB, "''");
                    }
                }

                if (inFilterSB.Length > 0)
                {
                    if (filterCreditBondType.Length > 0)
                    {
                        filterCreditBondType.Append(" or ");
                    }
                    filterCreditBondType.Append($"{BOND_CATEGORY_FIELD} in ({inFilterSB})");
                }
                if (notInFilter.Length > 0)
                {
                    if (filterCreditBondType.Length > 0)
                    {
                        filterCreditBondType.Append(" or ");
                    }
                    filterCreditBondType.Append(notInFilter);
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
                foreach (ListBoxItem lbi in selectedItems)
                {
                    if (lbi.Content.ToString() == CommonConstants.ALL)
                    {
                        filterRemainingTime.Remove(0, filterRemainingTime.Length);
                        break;
                    }

                    if (filterRemainingTime.Length > 0)
                    {
                        filterRemainingTime.Append(" or ");
                    }

                    if (lbi.Content.ToString() == "0~1Y")
                    {
                        filterRemainingTime.Append($"({REMAINGING_TIME_CONVERT_FIELD} >= 0 and {REMAINGING_TIME_CONVERT_FIELD} < {365 * 1})");
                    }
                    else if (lbi.Content.ToString() == "1~3Y")
                    {
                        filterRemainingTime.Append($"({REMAINGING_TIME_CONVERT_FIELD} >= {365 * 1} and {REMAINGING_TIME_CONVERT_FIELD} < {365 * 3})");
                    }
                    else if (lbi.Content.ToString() == "3~5Y")
                    {
                        filterRemainingTime.Append($"({REMAINGING_TIME_CONVERT_FIELD} >= {365 * 3} and {REMAINGING_TIME_CONVERT_FIELD} < {365 * 5})");
                    }
                    else if (lbi.Content.ToString() == "5~7Y")
                    {
                        filterRemainingTime.Append($"({REMAINGING_TIME_CONVERT_FIELD} >= {365 * 5} and {REMAINGING_TIME_CONVERT_FIELD} < {365 * 7})");
                    }
                    else if (lbi.Content.ToString() == "7~10Y")
                    {
                        filterRemainingTime.Append($"({REMAINGING_TIME_CONVERT_FIELD} >= {365 * 7} and {REMAINGING_TIME_CONVERT_FIELD} < {365 * 10})");
                    }
                    else if (lbi.Content.ToString() == ">10Y")
                    {
                        filterRemainingTime.Append($"({REMAINGING_TIME_CONVERT_FIELD} >= {365 * 10})");
                    }
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
                        case "AA+":
                        case "AA":
                        case "AA-":
                        case "A+":
                            AddListFilter(inFilterSB, $"'{lbi.Content}'");
                            break;
                        case "其他":
                            notInFilter = $"{ISSUER_RATING_CURRENT_NPY_FIELD} not in ('AAA+','AAA','AA+','AA','AA-','A+') or {ISSUER_RATING_CURRENT_NPY_FIELD} is null";
                            break;
                    }
                }
                if (inFilterSB.Length > 0)
                {
                    if (filterRating.Length > 0)
                    {
                        filterRating.Append(" or ");
                    }
                    filterRating.Append($"{ISSUER_RATING_CURRENT_NPY_FIELD} in ({inFilterSB})");
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

        private volatile bool _NeedUpdate = false;
        private string _rowFilterStr = string.Empty;
        public void FilterData(Dictionary<string, IList> filterDic)
        {
            StringBuilder filterBuffer = new StringBuilder();


            StringBuilder firmFlagFilterBuf = new StringBuilder();
            if (filterDic != null && filterDic.ContainsKey("FirmFlag"))
            {
                IList firmTypeList = filterDic["FirmFlag"];
                if (firmTypeList.Count > 0)
                {
                    foreach (System.Windows.Controls.ListBoxItem firmType in firmTypeList)
                    {
                        if (firmFlagFilterBuf.Length > 0)
                        {
                            firmFlagFilterBuf.Append(" or ");
                        }

                        if (firmType.Content.ToString() == "firm")
                        {
                            firmFlagFilterBuf.Append($"{CTPriceControler.FIRM_TYPE_FIELD} like '%1%'");
                        }
                        else if (firmType.Content.ToString() == "nonfirm")
                        {
                            firmFlagFilterBuf.Append($"{CTPriceControler.FIRM_TYPE_FIELD} like '%0%'");
                        }
                    }
                }
            }

            StringBuilder venuesFilterBuf = new StringBuilder();
            if (filterDic != null && filterDic.ContainsKey("Venues"))
            {
                IList venuesList = filterDic["Venues"];
                if (venuesList.Count > 0)
                {
                    foreach (System.Windows.Controls.ListBoxItem venues in venuesList)
                    {
                        if (venuesFilterBuf.Length > 0)
                        {
                            venuesFilterBuf.Append(" or ");
                        }

                        switch (venues.Content.ToString())
                        {
                            case "IDB":
                                venuesFilterBuf.Append($"{CTPriceControler.SOURCE_FIELD} = 'IDB'");
                                break;
                            case "量化":
                                venuesFilterBuf.Append($"{CTPriceControler.SOURCE_FIELD} = 'QUANT'");
                                break;
                            case "人工":
                                venuesFilterBuf.Append($"{CTPriceControler.SOURCE_FIELD} = 'MANUAL'");
                                break;
                            case "内部":
                                venuesFilterBuf.Append($"({CTPriceControler.SOURCE_FIELD} = 'IDB' or { CTPriceControler.SOURCE_FIELD} = 'QUANT' or {CTPriceControler.SOURCE_FIELD} = 'MANUAL')");
                                break;
                            case "X-Bond":
                                venuesFilterBuf.Append($"{CTPriceControler.SOURCE_FIELD} = 'ODM'");
                                break;
                            case "上新债":
                                venuesFilterBuf.Append($"{CTPriceControler.SOURCE_FIELD} = 'SSE'");
                                break;
                            case "上固收":
                                venuesFilterBuf.Append($"{CTPriceControler.SOURCE_FIELD} = 'SSF'");
                                break;
                            case "深固收":
                                venuesFilterBuf.Append($"{CTPriceControler.SOURCE_FIELD} = 'SZE'");
                                break;
                            case "双边":
                                venuesFilterBuf.Append($"{CTPriceControler.SOURCE_FIELD} = 'QDMESP'");
                                break;
                            case "经纪商":
                                venuesFilterBuf.Append($"({CTPriceControler.SOURCE_FIELD} = 'CNEX' or {CTPriceControler.SOURCE_FIELD} = 'TPSC' or {CTPriceControler.SOURCE_FIELD} = 'CCTB' or {CTPriceControler.SOURCE_FIELD} = 'CTTJ' or {CTPriceControler.SOURCE_FIELD} = 'PTCN' or {CTPriceControler.SOURCE_FIELD} = 'UEDA')");
                                break;
                            case CommonConstants.CNEX:
                                venuesFilterBuf.Append($"{CTPriceControler.SOURCE_FIELD} = 'CNEX'");
                                break;
                            case CommonConstants.TPSC:
                                venuesFilterBuf.Append($"{CTPriceControler.SOURCE_FIELD} = 'TPSC'");
                                break;
                            case CommonConstants.CCTB:
                                venuesFilterBuf.Append($"{CTPriceControler.SOURCE_FIELD} = 'CCTB'");
                                break;
                            case CommonConstants.CTTJ:
                                venuesFilterBuf.Append($"{CTPriceControler.SOURCE_FIELD} = 'CTTJ'");
                                break;
                            case CommonConstants.PTCN:
                                venuesFilterBuf.Append($"{CTPriceControler.SOURCE_FIELD} = 'PTCN'");
                                break;
                            case CommonConstants.UEDA:
                                venuesFilterBuf.Append($"{CTPriceControler.SOURCE_FIELD} = 'UEDA'");
                                break;
                        }
                    }
                }
            }

            StringBuilder settlTypeFilterBuf = new StringBuilder();
            if (filterDic != null && filterDic.ContainsKey("SettlType"))
            {
                IList settlTypeList = filterDic["SettlType"];
                if (settlTypeList.Count > 0)
                {
                    foreach (System.Windows.Controls.ListBoxItem settlType in settlTypeList)
                    {
                        if (settlTypeFilterBuf.Length > 0)
                        {
                            settlTypeFilterBuf.Append(" or ");
                        }

                        if (settlType.Content.ToString() == "T+1")
                        {
                            settlTypeFilterBuf.Append($"({CTPriceControler.SETTL_TYPE_FIELD} like '%2%' or {CTPriceControler.SETTL_TYPE_FIELD} like '%3%')");
                        }
                        else if (settlType.Content.ToString() == "T+0")
                        {
                            settlTypeFilterBuf.Append($"{CTPriceControler.SETTL_TYPE_FIELD} like '%1%'");
                        }
                        else
                        {
                            settlTypeFilterBuf.Append($"{CTPriceControler.SETTL_TYPE_YUAN_FIELD} <> ''");
                        }
                    }
                }
            }

            if (firmFlagFilterBuf.Length > 0)
            {
                if (filterBuffer.Length > 0)
                {
                    filterBuffer.Append($" and ({firmFlagFilterBuf})");
                }
                else
                {
                    filterBuffer.Append($"({firmFlagFilterBuf})");
                }
            }


            if (venuesFilterBuf.Length > 0)
            {
                if (filterBuffer.Length > 0)
                {
                    filterBuffer.Append($" and ({venuesFilterBuf})");
                }
                else
                {
                    filterBuffer.Append($"({venuesFilterBuf})");
                }
            }

            if (settlTypeFilterBuf.Length > 0)
            {
                if (filterBuffer.Length > 0)
                {
                    filterBuffer.Append($" and ({settlTypeFilterBuf})");
                }
                else
                {
                    filterBuffer.Append($"({settlTypeFilterBuf})");
                }
            }

            this._rowFilterStr = filterBuffer.ToString();
            _NeedUpdate = true;

            // 根据清算速度调整数据 todo
            foreach (DataRowView drv in this.BondView)
            {
                string securtiyID = CommonUtils.ConvertToString(drv[SECURITY_ID_FIELD]);

                // 获取缓存数据
                MarketDataSnapshotFullRefresh obj = PriceHandler.Instance.GetCacheCTPrice(securtiyID);

                // 计算最优价格
                lock (lockObj)
                {
                    CalculateBestPrice(obj, drv.Row);
                }
            }
        }

        private string OldgSecurityID = string.Empty;
        private string NewSecurityID = string.Empty;
        private DataRowView _selectedItem;
        public DataRowView SelectedItem
        {
            get
            {
                return _selectedItem;
            }

            set
            {
                //if (SelectedItem != null && value.Row.Table.Rows.Contains(value.Row))
                //{
                //    NewSecurityID = _selectedItem["securityID"] as string;
                //}
                if (value != null)
                {
                    NewSecurityID = value["securityID"] as string;
                }
                _selectedItem = value;

                if ((OldgSecurityID ?? "") != (NewSecurityID ?? ""))
                {
                    //触发事件
                    Common.Handler.Base.BaseHandler.Instance.PublishAppMsg(new PResult() { Code = "TriggerHomePageListCommand", Info1 = NewSecurityID });
                }
            }
        }

        private Visibility _noRight = Visibility.Collapsed;
        public Visibility NoRight
        {
            get { return _noRight; }
            set
            {
                _noRight = value;
                this.OnPropertyChanged(nameof(NoRight));
            }
        }

        private Visibility _enterMarket = Visibility.Hidden;
        public Visibility EnterMarket
        {
            get { return _enterMarket; }
            set
            {
                _enterMarket = value;
                this.OnPropertyChanged(nameof(EnterMarket));
            }
        }

        private double _noRightWidth = 104;
        public double NoRightWidth
        {
            get { return _noRightWidth; }
            set
            {
                _noRightWidth = value;
                this.OnPropertyChanged(nameof(NoRightWidth));
            }
        }


        // 债券列表
        private List<Bond> _bondList = new List<Bond>();

        public FavoriteBondsListBox MainView;

        /// <summary>
        /// 构造函数 
        /// </summary>
        public FavoriteBondsListBoxViewModel()
        {
            if (RefreshDataThread == null || !RefreshDataThread.IsAlive)
            {
                RefreshDataThread = new Thread(RefreshMDData);
                RefreshDataThread.IsBackground = true;
                RefreshDataThread.Start();
            }

            bool isAllow = AdminHandler.Instance.CheckPermission("1003", null);
            if (isAllow)
            {
                bool isNotAllow = AdminHandler.Instance.CheckPermission("4002", "400201");
                if (isNotAllow)
                {
                    NoRight = Visibility.Collapsed;
                }
                else
                {
                    NoRight = Visibility.Visible;
                }
            }
            else
            {
                NoRightWidth = 0;
            }
            if("1".Equals(LocalEnv.ConfInfo.IsShowEnterMarket))
                EnterMarket = Visibility.Visible;


            CopyBidCommand = new RelayCommand<DataRowView>(CopyBid);
            CopyOfrCommand = new RelayCommand<DataRowView>(CopyOfr);
            CopyDoubleCommand = new RelayCommand<DataRowView>(CopyDouble);

            ExportCommand = new ComRelayCommand(ExportCSV);
            ImportCommand = new ComRelayCommand(ImportCSV);


            // Todo: 注册清零信号, 暂时先不清零
            BaseHandler.Instance.Init(Instance_OnClear);

            BindingOperations.EnableCollectionSynchronization(this.BondView, lockObj);

            BaseHandler.Instance.SubscribeLoadDataStatus(OnLoadDataStatus);

            //订阅最后成交处理完成事件，当获取历史记录完成后更新界面的最后成交
            BaseHandler.Instance.SubscribeProctMarketExecutionsStatus(OnLoadtMarketExecutionsStatus);
        }

        private void Instance_OnClear(ClearPo obj)
        {
            //LogHandler.Info("FavoriteBondsListBoxViewModel Clear");
            if (obj != null && "Y".Equals(obj.IsClear))
            {
                Application.Current.Dispatcher.BeginInvoke((Action)(() => {
                    string securityID = string.Empty;

                    if (SelectedItem != null)
                    {
                        securityID = SelectedItem.Row["securityID"].ToString();
                    }
                    
                    this.Clear();
                    this.Init(_ownUI);
                    if (securityID != null)
                    {
                        this.MainView.OnUpdateSelectedItem(securityID);
                    }
                }));
            }
        }

        ~FavoriteBondsListBoxViewModel()
        {
            this.Dispose();
        }

        new public void Dispose()
        {
            Clear();
            if (_RefreshBaseDataThread != null)
            {
                _RefreshBaseDataThread.Abort();
                _RefreshBaseDataThread = null;
            }

            if (RefreshDataThread != null)
            {
                LogHandler.Info("QuotationListViewModel Dispose RefreshDataThread:");
                RefreshDataThread.Abort();
                RefreshDataThread = null;
            }

            if (_RefreshDefaultSelectThread != null)
            {
                LogHandler.Info("QuotationListViewModel Dispose RefreshDefaultSelectThread:");
                _RefreshDefaultSelectThread.Abort();
                _RefreshDefaultSelectThread = null;
            }

            lock (this.lockObj)
            {
                if (_BondView != null)
                {
                    _BondView.Table.Clear();
                    _BondView.Dispose();
                    _BondView = null;
                }
            }
            //if (_ProcessDMPriceThreadQueue != null)
            //{
            //    _ProcessDMPriceThreadQueue.Clear();
            //    _ProcessDMPriceThreadQueue.Dispose();
            //    _ProcessDMPriceThreadQueue = null;
            //}
       
            lock(_LastCTPriceDic)
                if (_LastCTPriceDic != null)
                {
                    _LastCTPriceDic.Clear();
                    _LastCTPriceDic = null;
                }
            
            if (_ProcessMyOrdersThreadQueue != null)
            {
                _ProcessMyOrdersThreadQueue.Clear();
                _ProcessMyOrdersThreadQueue.Dispose();
                _ProcessMyOrdersThreadQueue = null;
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

            cancellationTokenSource?.Cancel();

            base.Dispose();
        }


        public void Clear()
        {
            _initBondNum = -1;
            HomePageDataHandler.OnFavoritesChanged -= HomePageDataHandler_OnFavoritesChanged;
            EmsOrderProcesser.Processer.DetailDT.RowDeleting -= DetailDT_RowDeleting;
            EmsOrderProcesser.Processer.DetailDT.RowChanged -= DetailDT_RowChanged;
            LastExecutionProcesser.Processer.OnLastExecutions -= Processer_OnLastExecutions;

            foreach (DataRowView bond in _BondView)
            {
                string securityID = CommonUtils.ConvertToString(bond[SECURITY_ID_FIELD]);
                //PriceHandler.Instance.UnregisterH0Price(CommonUtils.ConvertToString(bond[SECURITY_ID_FIELD]), Instance_OnMDPrice);
                PriceHandler.Instance.UnregisterCTPrice(securityID, Instance_OnMDPrice);
                DataView removeDV;
                baseCTPriceDataViewDic.TryRemove(securityID, out removeDV);
                EmsOrdersHandler.Instance.UnregistEMSOrderBySecurityID(securityID, EmsOrderProcesser.Processer.Instance_OnEmsOrders);
                bond["Display_LastMarketPx"] = string.Empty;
                bond["LastMarketPx_Venues"] = string.Empty;
                bond["LastSettlTypeTag"] = string.Empty;
                bond["Valuation"] = 0; 
            }

            MyOrderProcesser.Processer.OnOwnOrdersChange -= Instance_OnOrder;

            lock (this.lockObj)
            {
                if (_BondView != null)
                {
                    _BondView.Table.Rows.Clear();
                }
            }
            //if (_ProcessDMPriceThreadQueue != null)
            //{
            //    _ProcessDMPriceThreadQueue.Clear();
            //}
            lock(_LastCTPriceDic)
                if (_LastCTPriceDic != null)
                {
                    _LastCTPriceDic.Clear();
                }
            
            if (_ProcessMyOrdersThreadQueue != null)
            {
                _ProcessMyOrdersThreadQueue.Clear();
            }
            if (_ProcessBondsThreadQueue != null)
            {
                _ProcessBondsThreadQueue.Clear();
            }
            if (_ProcessLastExecutionsThreadQueue != null)
            {
                _ProcessLastExecutionsThreadQueue.Clear();
            }

            //cancellationTokenSource?.Cancel();
        }

        public void BindSecurtitiesFirst()
        {
            IOrderedEnumerable<KeyValuePair<string, HomeGroupInfo>> groupList = HomePageDataHandler.RequestGroups(false);
            if (groupList != null)
            {
                foreach (KeyValuePair<string, HomeGroupInfo> group in groupList)
                {
                    if (group.Key == this.groupID)
                    {
                        _initBondNum = group.Value.securityids.Count;
                        foreach (HomeGroupSecurityIDsInfo obj in group.Value.securityids)
                        {
                            UpdateBond(obj);
                        }
                    }
                }
            }
        }


        /// <summary>
        /// 安全线程队列
        /// </summary>
        private static CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        // 创建一个队列来存放最后成交数据
        private static BlockingCollection<ExecutionReport> queue = new BlockingCollection<ExecutionReport>();
        // 使用 SemaphoreSlim 限制并发线程数量为 50
        private static SemaphoreSlim semaphore = new SemaphoreSlim(10);


        public void Init(FrameworkElement ownUI)
        {
            _ownUI = ownUI;

            BindSecurtitiesFirst();

            HomePageDataHandler.OnFavoritesChanged += HomePageDataHandler_OnFavoritesChanged;
            if (_ProcessBondsThreadQueue == null)
            {
                _ProcessBondsThreadQueue = new ThreadQueue<HomeGroupSecurityIDsInfo>(UpdateBondAsync);
            }
           
            if (_ProcessMyOrdersThreadQueue == null)
            {
                _ProcessMyOrdersThreadQueue = new ThreadQueue<string>(UpdateMyOrdersAsync);
            }

            //if (_ProcessDMPriceThreadQueue == null)
            //{
            //    _ProcessDMPriceThreadQueue = new ThreadQueue<MarketDataSnapshotFullRefresh>(UpdateMDPriceAsync);
            //}

            //if (_ProcessLastExecutionsThreadQueue == null)
            //{
            //    _ProcessLastExecutionsThreadQueue = new AsyncThreadQueue<ExecutionReport>(UpdateLastExecutionsAsync);
            //}

            //处理最后成交数据
            Task.Factory.StartNew(() => DoProcLastExecutionQueue(cancellationTokenSource.Token), TaskCreationOptions.LongRunning);

            //// 注册基础数据
            //BaseHandler.Instance.SubscribeLoadDataStatus(onloadData);
            //if (_RefreshBaseDataThread == null)
            //{
            //    _RefreshBaseDataThread = new Thread(RefreshBaseData);
            //    _RefreshBaseDataThread.IsBackground = true;
            //    _RefreshBaseDataThread.Start();
            //}

            if (_RefreshDefaultSelectThread == null)
            {
                _RefreshDefaultSelectThread = new Thread(RefreshDefaultSelect);
                _RefreshDefaultSelectThread.IsBackground = true;
                _RefreshDefaultSelectThread.Start();
            }


            // 订阅我的订单
            MyOrderProcesser.Processer.OnOwnOrdersChange += Instance_OnOrder;
            // -----------------------------------------
            EmsOrderProcesser.Processer.DetailDT.RowChanged += DetailDT_RowChanged; ;
            EmsOrderProcesser.Processer.DetailDT.RowDeleting += DetailDT_RowDeleting; ;
            LastExecutionProcesser.Processer.OnLastExecutions += Processer_OnLastExecutions;
        }

        private void RefreshDefaultSelect()
        {
            while (true)
            {
                try
                {
                    SetDefaultSelected();

                    if (_isSelected)
                        break;

                    Thread.Sleep(200);
                }
                catch (ThreadAbortException ex)
                {

                }
                catch (Exception ex)
                {
                    LogHandler.Error(ex);
                }
            }
        }

        private bool _isSelected = false;
        private int _initBondNum = -1;
        private void SetDefaultSelected()
        {
            if (!_isSelected && _initBondNum > 0 && _BondView.Table.Rows.Count == _initBondNum)
            {
                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    object obj = _ownUI.FindName("FavoriteBondslistBox");
                    if (obj != null)
                    {
                        ListView dg = obj as ListView;

                        if (dg != null)
                        {
                            dg.SelectedIndex = 0;


                        }

                        _isSelected = true;
                    }
                }));
            }
        }

        //private void RefreshBaseData()
        //{
        //    // 刷新基础数据, 收到数据刷新一次结束
        //    try
        //    {
        //        while (true)
        //        {
        //            try
        //            {
        //                if (CommonUtils.IsEndofLoadFromDB())
        //                {
        //                    onloadData(true);
        //                }
        //                Thread.Sleep(500);
        //            }
        //            catch (Exception e)
        //            {
        //                LogHandler.Error(e);
        //            }
        //        }
        //    }
        //    catch (ThreadAbortException)
        //    {
        //        //ignore
        //    }
        //    catch (Exception e)
        //    {
        //        LogHandler.Error(e);
        //    }
        //}


        bool isLoadBond = false;
        private void OnLoadDataStatus(bool obj)
        {
            if (obj)
            {
                isLoadBond = true;

                onloadData(true);
            }
        }


        private ConcurrentDictionary<string, object> bondWithOutInfoList = new ConcurrentDictionary<string, object>();

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
            }

            foreach(string securityID in removeList)
            {
                object removeObj = null;
                bondWithOutInfoList.TryRemove(securityID, out removeObj);
            }
        }

        private void Sub_OnLoadData(BondsInfoPo bondsInfoPo)
        {
            if (Application.Current != null && bondsInfoPo != null)
            {
                Application.Current.Dispatcher.Invoke((Action)(() =>
                {
                    lock (this.lockObj)
                    {
                        DataRow dr = this.BondView.Table.Rows.Find(new object[] { bondsInfoPo.Bond_ID });
                        if (dr != null)
                        {
                            if (bondsInfoPo != null)
                            {

                                if (string.IsNullOrEmpty(bondsInfoPo.Short_Name))
                                {
                                    dr[SYMBOL_FIELD] = "";
                                }
                                else
                                {
                                    dr[SYMBOL_FIELD] = bondsInfoPo.Short_Name;// 代码名称
                                }

                                if (bondsInfoPo.MarketIndicator == "4")
                                {
                                    dr[REMAINGING_TIME_FIELD] = string.IsNullOrEmpty(bondsInfoPo.RemaingingTime)?"已到期":bondsInfoPo.RemaingingTime;//剩余时间
                                    dr[REMAINGING_TIME_CONVERT_FIELD] = ConvertRemainingTime(bondsInfoPo.RemaingingTime);//剩余时间转换成时间段
                                    dr[BOND_TYPE_FIELD] = CommonUtils.GetBondCategoryName(bondsInfoPo.Bond_Subtype, bondsInfoPo.Ent_Cor);// 债券类型 
                                }
                                else
                                {
                                    dr[REMAINGING_TIME_FIELD] = string.Empty;
                                    //dr[REMAINGING_TIME_CONVERT_FIELD] = string.Empty;
                                    dr[BOND_TYPE_FIELD] = string.Empty;
                                }
                                dr[ISSUER_RATING_CURRENT_NPY_FIELD] = bondsInfoPo.Issuer_Rating_Current_NPY; // 评级
                                
                                dr[BOND_SUB_TYPE_FIELD] = bondsInfoPo.Bond_Subtype; //债券子类
                                dr[GOODS_TYPE_FIELD] = bondsInfoPo.SECURITY_TYPE;  //债券大类
                                dr[ENT_COR_FIELD] = bondsInfoPo.Ent_Cor; //Ent_Cor
                                dr[BOND_CATEGORY_FIELD] = GetBondCategory(bondsInfoPo.Bond_Subtype, bondsInfoPo.Ent_Cor);
                                //drv[OTHER_FIELD] = bondsInfoPo.Rating_Current + "||(" + bondsInfoPo.SECURITY_TYPE + ":" + bondsInfoPo.Bond_Subtype + ":" + bondsInfoPo.Ent_Cor + ")";
                            }
                            else
                            {
                                dr[SYMBOL_FIELD] = "";// 代码名称
                                dr[REMAINGING_TIME_FIELD] = string.Empty;//剩余时间
                                dr[REMAINGING_TIME_CONVERT_FIELD] = 0;//剩余时间转换成时间段
                                dr[ISSUER_RATING_CURRENT_NPY_FIELD] = string.Empty; // 评级
                                dr[BOND_TYPE_FIELD] = string.Empty;// 债券类型 
                                dr[BOND_SUB_TYPE_FIELD] = string.Empty; //债券子类
                                dr[GOODS_TYPE_FIELD] = string.Empty;  //债券大类
                                dr[ENT_COR_FIELD] = string.Empty; //Ent_Cor
                                dr[BOND_CATEGORY_FIELD] = string.Empty;
                            }
                        }
                    }
                }));
            }
        }

        private void HomePageDataHandler_OnFavoritesChanged(HomePageDataHandler.FavoriteTypeEnum favorityType, string groupID, string securityID)
        {
            if (favorityType == HomePageDataHandler.FavoriteTypeEnum.AddBond
                || favorityType == HomePageDataHandler.FavoriteTypeEnum.DelBond
                || favorityType == HomePageDataHandler.FavoriteTypeEnum.DelGroup
                || favorityType == HomePageDataHandler.FavoriteTypeEnum.BatchImport)
            {
                if (groupID == this.groupID)
                {
                    SyncBonds(false);
                }

                if (favorityType == HomePageDataHandler.FavoriteTypeEnum.DelGroup)
                {
                    string value = string.Empty;
                    HomePageDataHandler.CatchHomePageDisplayModeDic.TryRemove(groupID, out value);
                    HomePageDataHandler.WriteHomePageDisplayMode();
                }
            }
        }

        private void Processer_OnLastExecutions(ExecutionReport obj)
        {
            //if (_ProcessLastExecutionsThreadQueue != null)
            //{
            //    _ProcessLastExecutionsThreadQueue.Enqueue(obj);
            //}

            if (obj != null && !string.IsNullOrEmpty(obj.SecurityID ?? ""))
            {
                queue.Add(obj);
            }
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
                this.filterSB.Append($"({SECURITY_ID_FIELD} like '%{this.SearchText}%' or {SYMBOL_FIELD} like '%{this.SearchText}%')");
            }
            #endregion

            // 添加债券类型过滤
            #region 筛选债券类型
            if (filterBondType.Length > 0 || filterCreditBondType.Length > 0)
            {
                if (filterSB.Length > 0)
                {
                    filterSB.Append(" and ");
                }
                string orStr = (filterBondType.Length > 0 && filterCreditBondType.Length > 0) ? " or " : string.Empty;
                filterSB.Append($"({filterBondType}{orStr}{filterCreditBondType})");
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

            //// 添加清算速度
            //#region 筛选清算速度
            //if (filterSettlType.Length > 0)
            //{
            //    if (filterSB.Length > 0)
            //    {
            //        filterSB.Append(" and ");
            //    }
            //    filterSB.Append(filterSettlType);
            //}
            //#endregion

            this.BondView.RowFilter = filterSB.ToString();

        }

        private void Instance_OnBondList(List<HomeGroupSecurityIDsInfo> bondList)
        {
            //删除
            List<DataRow> removeList = new List<DataRow>();
            lock (this.lockObj)
            {
                foreach (DataRowView drv in this.BondView)
                {
                    string securityID = CommonUtils.ConvertToString(drv[KEY_FIELD]);
                    if (bondList == null || bondList.Any<HomeGroupSecurityIDsInfo>(bond => (bond.securityid == securityID)) == false)
                    {
                        removeList.Add(drv.Row);

                        // 取消订阅
                        //PriceHandler.Instance.UnregisterH0Price(securityID, Instance_OnMDPrice);
                        PriceHandler.Instance.UnregisterCTPrice(securityID, Instance_OnMDPrice);
                        DataView removeDV;
                        baseCTPriceDataViewDic.TryRemove(securityID, out removeDV);
                        EmsOrdersHandler.Instance.UnregistEMSOrderBySecurityID(securityID, EmsOrderProcesser.Processer.Instance_OnEmsOrders);
                    }
                }
            }
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                lock (this.lockObj)
                {
                    foreach (DataRow removeDr in removeList)
                    {
                        string securityID = removeDr[SECURITY_ID_FIELD].ToString();
                        this.BondView.Table.Rows.Remove(removeDr);
                        
                        object removeObject = null;
                        this.bondWithOutInfoList.TryRemove(securityID, out removeObject);
                    }
                }
            }));


            // 添加和更新
            if (bondList != null && bondList.Count > 0)
            {
                foreach (HomeGroupSecurityIDsInfo bondObj in bondList)
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
        private void Instance_OnOrder(string securityid, string side)
        {
            //UpdateMyOrders(obj);
            if (_ProcessMyOrdersThreadQueue != null)
            {
                _ProcessMyOrdersThreadQueue.Enqueue(securityid);
            }
        }

        /// <summary>
        /// 做市报价（SOR）回调
        /// </summary>
        /// <param name="obj"></param>
        private void Instance_OnMDPrice(Common.Model.Fix.Message.MarketDataSnapshotFullRefresh obj)
        {
            //UpdateMDPrice(obj);
            //if (_ProcessDMPriceThreadQueue != null)
            //{
            //    _ProcessDMPriceThreadQueue.Enqueue(obj);
            //}
            //Console.WriteLine($"原始更新{obj.SecurityID}");
            lock (_LastCTPriceDic)
            {
                this._LastCTPriceDic[obj.SecurityID] = obj;
            }
        }

        private object LockUpd = new object();
        private object LockDict = new object();
        private void CalcMdData()
        {
            try
            {
                List<string> keylist;
                Dictionary<string, MarketDataSnapshotFullRefresh> tmpCTPriceDic;
                if (_LastCTPriceDic != null)
                {
                    lock (_LastCTPriceDic)
                    {
                        keylist = this._LastCTPriceDic.Keys.ToList();
                        if (keylist.Count == 0)
                            return;

                        tmpCTPriceDic = new Dictionary<string, MarketDataSnapshotFullRefresh>(_LastCTPriceDic);
                        this._LastCTPriceDic.Clear();

                    }

                    if (keylist != null && keylist.Count > 0)
                    {
                        for (int i = 0; i < keylist.Count; i++)
                        {
                            string key = keylist[i];

                            if (baseCTPriceDataViewDic.ContainsKey(key) == false)
                            {
                                baseCTPriceDataViewDic[key] = CreateBaseDataView();
                            }


                            DataView priceBaseData = null;
                            bool flag = baseCTPriceDataViewDic.TryGetValue(key, out priceBaseData);
                            if (flag == true)
                            {
                                lock (priceBaseData)
                                {
                                    CTPriceControler.UpdateDataToBaseDV(priceBaseData, tmpCTPriceDic[key]);
                                }

                                UpdateBestPrice(priceBaseData, tmpCTPriceDic[key]);
                            }

                        }

                    }
                }
            }
            catch (Exception ex)
            {
                LogHandler.Error("CalcMdData:" + ex.Message);
            }
        }

        /// <summary>
        /// 根据筛选条件计算最优价格
        /// </summary>
        /// <param name="priceBaseData"></param>
        private async void UpdateBestPrice(DataView priceBaseData, MarketDataSnapshotFullRefresh obj)
        {
            if (priceBaseData == null)
                return;

            await Task.Run(() =>
            {
                System.Windows.Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    string securityID = CommonUtils.ConvertToString(obj.SecurityID);
                    // bid最优价格
                    BestPrice1 currentBestBid = null;
                    BestPrice1 currentBestOfr = null;
                    BestPrice1 allBestBid = null;
                    BestPrice1 allBestOfr = null;
                    //BestPrice1 bestBid
                    lock (priceBaseData)
                    {
                        currentBestBid = CalcBestPrice(priceBaseData, "0", this._rowFilterStr);
                        // ofr 最优价格
                        currentBestOfr = CalcBestPrice(priceBaseData, "1", this._rowFilterStr);

                        if (this._rowFilterStr.Length > 0)
                        {
                            allBestBid = CalcBestPrice(priceBaseData, "0");
                            // ofr 最优价格
                            allBestOfr = CalcBestPrice(priceBaseData, "1");
                        }
                    }

                    lock (lockObj)
                    {
                        try
                        {
                            DataRow[] drs = this.BondView.Table.Select($"{KEY_FIELD} = '{securityID}'");
                            if (drs != null && drs.Length > 0)
                            {
                                foreach (DataRow item in drs)
                                {
                                    if (item != null)
                                    {
                                        // 中债估值
                                        item[VALUATION_FIELD] = CommonUtils.ConvertToDecimal(obj.CCDCValuation);
                                        if (this._rowFilterStr.Length > 0)
                                        {
                                            item[BID_BBO_FIELD] = allBestBid == null || string.IsNullOrEmpty(allBestBid.bestPx) ? "- -" : allBestBid.bestPx;
                                            item[BID_TXT_FIELD] = allBestBid == null || string.IsNullOrEmpty(allBestBid.text) ? "- -" : allBestBid.text;
                                            item[BID_MAX_T1_QTY_FIELD] = allBestBid == null ? "- -" : allBestBid.T1Qty.ToString();
                                            item[OFR_BBO_FIELD] = allBestOfr == null || string.IsNullOrEmpty(allBestOfr.bestPx) ? "- -" : allBestOfr.bestPx;
                                            item[OFR_TXT_FIELD] = allBestOfr == null || string.IsNullOrEmpty(allBestOfr.text) ? "- -" : allBestOfr.text;
                                            item[OFR_MAX_T1_QTY_FIELD] = allBestOfr == null ? "- -" : allBestOfr.T1Qty.ToString();
                                        }
                                        else
                                        {
                                            item[BID_BBO_FIELD] = currentBestBid == null || string.IsNullOrEmpty(currentBestBid.bestPx) ? "- -" : currentBestBid.bestPx;
                                            item[BID_TXT_FIELD] = currentBestBid == null || string.IsNullOrEmpty(currentBestBid.text) ? "- -" : currentBestBid.text;
                                            item[BID_MAX_T1_QTY_FIELD] = currentBestBid == null ? "- -" : currentBestBid.T1Qty.ToString();
                                            item[OFR_BBO_FIELD] = currentBestOfr == null || string.IsNullOrEmpty(currentBestOfr.bestPx) ? "- -" : currentBestOfr.bestPx;
                                            item[OFR_TXT_FIELD] = currentBestOfr == null || string.IsNullOrEmpty(currentBestOfr.text) ? "- -" : currentBestOfr.text;
                                            item[OFR_MAX_T1_QTY_FIELD] = currentBestOfr == null ? "- -" : currentBestOfr.T1Qty.ToString();
                                        }

                                        // 更新表格
                                        BindBestPriceToUI(item, currentBestBid, currentBestOfr);

                                        // 更新最优价是否和我的订单一样标志
                                        BindIsSameTagToUI(securityID, item);

                                        // 更新角标
                                        GenerateFlagType(item);
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            LogHandler.Error("UpdateBestPrice:" + ex.Message);
                        }
                    }
                }));
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="side">0: 买 1:卖</param>
        /// <returns></returns>
        private BestPrice1 CalcBestPrice(DataView priceBaseData, string side, string customFilter = null)
        {
            string bargain = string.Empty;
            StringBuilder sb = new StringBuilder();
            string bestVenue = string.Empty;
            string venues = string.Empty;
            string bestPx = string.Empty;
            decimal bestQty = 0;
            string bestT0 = string.Empty;
            string bestT1 = string.Empty;
            string bestYuan = string.Empty;
            string hasFirm = "0";
            string hasNonFirm = "0";
            string priceType = string.Empty;
            decimal T1Qty = 0;
            decimal hugeT1Qty = 0;
            // 角标判定（按 xc 口径，从行情数据判定，整侧任意价位有报价即算）：
            // hasMe：本侧是否有“本人”报价（IsMe=true）；hasOur：是否有“本机构”报价（Source∈{IDB,QUANT,MANUAL}）
            bool hasMe = false;
            bool hasOur = false;

            string bidSort = $"{CTPriceControler.SIDE_FIELD},{CTPriceControler.MD_PRICE_LEVEL_FIELD}";
            string ofrSort = $"{CTPriceControler.SIDE_FIELD},{CTPriceControler.MD_PRICE_LEVEL_FIELD}";
            string filter = string.Empty;
            try
            {
                if (side == "0")
                {
                    priceBaseData.Sort = bidSort;
                    filter = $"{CTPriceControler.SIDE_FIELD} = '0'";
                    // bid最优价格
                    if (string.IsNullOrEmpty(customFilter) == false && customFilter.Length > 0)
                    {
                        filter = $"{filter} and ({customFilter})";
                    }
                    priceBaseData.RowFilter = filter;
                }
                else if (side == "1")
                {
                    priceBaseData.Sort = ofrSort;
                    filter = $"{CTPriceControler.SIDE_FIELD} = '1'";
                    // bid最优价格
                    if (string.IsNullOrEmpty(customFilter) == false && customFilter.Length > 0)
                    {
                        filter = $"{filter} and ({customFilter})";
                    }
                    priceBaseData.RowFilter = filter;
                }

                // 角标判定：整侧扫描，任意价位有本人/本机构来源的报价即标记（与最优价无关）。
                foreach (DataRowView drv in priceBaseData)
                {
                    if (!hasMe && CommonUtils.ConvertToString(drv[CTPriceControler.ISME_FIELD]).Equals("True", StringComparison.OrdinalIgnoreCase))
                    {
                        hasMe = true;
                    }
                    if (!hasOur)
                    {
                        string src = CommonUtils.ConvertToString(drv[CTPriceControler.SOURCE_FIELD]);
                        if (src == "IDB" || src == "QUANT" || src == "MANUAL")
                        {
                            hasOur = true;
                        }
                    }
                    if (hasMe && hasOur)
                    {
                        break;
                    }
                }

                if (priceBaseData.Count > 0)
                {
                    bestPx = CommonUtils.ConvertToString(priceBaseData[0][CTPriceControler.Yield_Field]);
                    bestVenue = CommonUtils.ConvertToString(priceBaseData[0][CTPriceControler.SOURCE_FIELD]);
                    //priceType = CommonUtils.ConvertToString(priceBaseData[0][CTPriceControler.PRICETYPE_FIELD]);
                    decimal bestPx_decimal = CommonUtils.ConvertToDecimal(bestPx);

                    if (CommonUtils.IsNumeric(bestPx))
                    {
                        foreach (DataRowView drv in priceBaseData)
                        {
                            string currentVenues = CommonUtils.ConvertToString(drv[CTPriceControler.SOURCE_FIELD]);
                            string px = CommonUtils.ConvertToString(drv[CTPriceControler.Yield_Field]);
                            if (CommonUtils.IsNumeric(px))
                            {
                                decimal px_decimal = CommonUtils.ConvertToDecimal(px);
                                decimal qty = CommonUtils.ConvertToDecimal(CommonUtils.ConvertToString(drv[CTPriceControler.QTY_FIELD]));
                                string settlType = CommonUtils.ConvertToString(CommonUtils.ConvertToString(drv[CTPriceControler.SETTL_TYPE_FIELD]));
                                string yuanSettlType = CommonUtils.ConvertToString(CommonUtils.ConvertToString(drv[CTPriceControler.SETTL_TYPE_YUAN_FIELD]));
                                string firmFlag = CommonUtils.ConvertToString(CommonUtils.ConvertToString(drv[CTPriceControler.FIRM_TYPE_FIELD]));
                                string text = CommonUtils.ConvertToString(CommonUtils.ConvertToString(drv[CTPriceControler.TEXT_FIELD]));
                                string barginFlag = CommonUtils.ConvertToString(CommonUtils.ConvertToString(drv[CTPriceControler.BARGAIN_FLAG_FIELD]));

                                if (bestPx_decimal == px_decimal)
                                {

                                    if (sb.Length > 0)
                                    {
                                        if (venues != currentVenues)
                                        {
                                            sb.Append("]");
                                        }
                                        sb.Append("+");
                                    }

                                    if (venues != currentVenues)
                                    {
                                        sb.Append("[");
                                        T1Qty = 0;
                                    }

                                    T1Qty += qty;

                                    if (hugeT1Qty < T1Qty)
                                    {
                                        hugeT1Qty = T1Qty;
                                    }

                                    if (!string.IsNullOrEmpty(text))
                                        sb.Append(string.Format("{0}", text));//string.Format("{0}({1})", mdEntry.MDEntrySize, mdEntry.Text));
                                    else
                                        sb.Append(qty);


                                    bargain = CalBar(bargain, barginFlag);

                                    bestQty += qty;
                                    string[] settleArr = settlType.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                                    foreach (string str in settleArr)
                                    {
                                        if (str == "2")
                                        {
                                            bestT1 = "1";
                                        }

                                        if (str == "1")
                                        {
                                            bestT0 = "1";
                                        }
                                    }
                                    if (yuanSettlType == "4" || yuanSettlType.Length > 1)
                                    {
                                        bestYuan = "1";
                                    }
                                    if (firmFlag == "1")
                                    {
                                        hasFirm = "1";
                                    }
                                    if (firmFlag == "0")
                                    {
                                        hasNonFirm = "1";
                                    }
                                    venues = currentVenues;
                                }
                                else
                                {
                                    break;
                                }
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                    else
                    {
                        foreach (DataRowView drv in priceBaseData)
                        {
                            string currentVenues = CommonUtils.ConvertToString(drv[CTPriceControler.SOURCE_FIELD]);
                            string px = CommonUtils.ConvertToString(drv[CTPriceControler.ENTRY_PX_FIELD]);
                            if (CommonUtils.IsNumeric(px) == false)
                            {
                                decimal qty = CommonUtils.ConvertToDecimal(CommonUtils.ConvertToString(drv[CTPriceControler.QTY_FIELD]));
                                string settlType = CommonUtils.ConvertToString(CommonUtils.ConvertToString(drv[CTPriceControler.SETTL_TYPE_FIELD]));
                                string yuanSettlType = CommonUtils.ConvertToString(CommonUtils.ConvertToString(drv[CTPriceControler.SETTL_TYPE_YUAN_FIELD]));
                                string firmFlag = CommonUtils.ConvertToString(CommonUtils.ConvertToString(drv[CTPriceControler.FIRM_TYPE_FIELD]));
                                string text = CommonUtils.ConvertToString(CommonUtils.ConvertToString(drv[CTPriceControler.TEXT_FIELD]));
                                string barginFlag = CommonUtils.ConvertToString(CommonUtils.ConvertToString(drv[CTPriceControler.BARGAIN_FLAG_FIELD]));

                                if (sb.Length > 0)
                                {
                                    if (venues != currentVenues)
                                    {
                                        sb.Append("]");
                                    }
                                    sb.Append("+");
                                }

                                if (!string.IsNullOrEmpty(text))
                                    sb.Append(string.Format("{0}", text));//string.Format("{0}({1})", mdEntry.MDEntrySize, mdEntry.Text));
                                else
                                    sb.Append(qty);

                                if (venues != currentVenues)
                                {
                                    sb.Append("[");
                                }

                                bargain = CalBar(bargain, barginFlag);

                                bestQty += qty;

                                string[] settleArr = settlType.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                                foreach (string str in settleArr)
                                {
                                    if (str == "2")
                                    {
                                        bestT1 = "1";
                                    }

                                    if (str == "1")
                                    {
                                        bestT0 = "1";
                                    }
                                }


                                //if (settlType.Contains("1"))
                                //{
                                //    bestT0 = "1";
                                //}
                                if (yuanSettlType == "4" || yuanSettlType.Length > 1)
                                {
                                    bestYuan = "1";
                                }
                                if (firmFlag == "1")
                                {
                                    hasFirm = "1";
                                }
                                if (firmFlag == "0")
                                {
                                    hasNonFirm = "1";
                                }

                                venues = currentVenues;
                            }
                            else
                            {
                                break;
                            }
                        }

                        //if (priceType == PriceType.Intention && !CommonUtils.IsNumeric(bestPx))
                        //{
                        //    bestPx = side == "0" ? "Bid" : "Ofr";
                        //}
                    }
                }
            }
            catch (Exception e)
            {
                LogHandler.Info($"filter:{filter}");
                LogHandler.Error(e);
            }


            if (sb.Length > 0)
            {
                sb.Append("]");
            }

            return new BestPrice1()
            {
                bestPx = bestPx,
                bestQty = bestQty,
                bestT0 = bestT0,
                bestT1 = bestT1,
                bestYuan = bestYuan,
                isAllFirm = (hasFirm == "1" && hasNonFirm == "0") ? "1" : "0",
                venue = bestVenue,
                priceType = priceType,
                text = sb.ToString(),
                bargain = bargain,
                T1Qty = hugeT1Qty,
                hasMe = hasMe,
                hasOur = hasOur
            };
        }

        private string CalBar(string bargainFlag, string newBarginFlag)
        {
            string result = bargainFlag;

            if (string.IsNullOrEmpty(bargainFlag))
                result = newBarginFlag;
            else if (bargainFlag != newBarginFlag && newBarginFlag == "2")
                result = "2";


            return result;
        }


        private void RefreshMDData()
        {
            while(true)
            {
                try
                {
                    CalcMdData();

                    Thread.Sleep(500);
                } 
                catch (Exception ex)
                {
                    LogHandler.Error(ex);
                }
            }
        }

        private DataView CreateBaseDataView()
        {
            DataView baseDV = new DataTable().DefaultView;
            baseDV.Table.Columns.Add(CTPriceControler.KEY_FIELD, typeof(string));
            baseDV.Table.Columns.Add(CTPriceControler.SECURITY_ID_FIELD, typeof(string));
            baseDV.Table.Columns.Add(CTPriceControler.TOTAL_QTY_FIELD, typeof(decimal));
            baseDV.Table.Columns.Add(CTPriceControler.TIME_FIELD, typeof(string));
            baseDV.Table.Columns.Add(CTPriceControler.SOURCE_FIELD, typeof(string));
            baseDV.Table.Columns.Add(CTPriceControler.QTY_FIELD, typeof(decimal));
            baseDV.Table.Columns.Add(CTPriceControler.ENTRY_PX_FIELD, typeof(string));
            baseDV.Table.Columns.Add(CTPriceControler.NET_PRICE_FIELD, typeof(string));
            baseDV.Table.Columns.Add(CTPriceControler.SETTL_TYPE_FIELD, typeof(string));
            baseDV.Table.Columns.Add(CTPriceControler.SETTL_TYPE_YUAN_FIELD, typeof(string));
            baseDV.Table.Columns.Add(CTPriceControler.SIDE_FIELD, typeof(string));
            baseDV.Table.Columns.Add(CTPriceControler.IS_BEST_FIELD, typeof(bool));
            baseDV.Table.Columns.Add(CTPriceControler.FLAG_TYPE_FIELD, typeof(string));
            baseDV.Table.Columns.Add(CTPriceControler.DELETE_FLAG_FIELD, typeof(string));
            baseDV.Table.Columns.Add(CTPriceControler.FIRM_TYPE_FIELD, typeof(string));
            baseDV.Table.Columns.Add(CTPriceControler.VENUE_TYPE_GW_FIELD, typeof(string));
            baseDV.Table.Columns.Add(CTPriceControler.TEXT_FIELD, typeof(string));
            baseDV.Table.Columns.Add(CTPriceControler.MD_PRICE_LEVEL_FIELD, typeof(int));
            baseDV.Table.Columns.Add(CTPriceControler.PARTY_SUB_NAME_FIELD, typeof(string));
            baseDV.Table.Columns.Add(CTPriceControler.BARGAIN_FLAG_FIELD, typeof(string));
            baseDV.Table.Columns.Add(CTPriceControler.WHOLE_FLAG_FIELD, typeof(string));
            baseDV.Table.Columns.Add(CTPriceControler.INSTITUTION_NAME_FIELD, typeof(string));
            baseDV.Table.Columns.Add(CTPriceControler.SHOWTRADER_FIELD, typeof(string));
            baseDV.Table.Columns.Add(CTPriceControler.ISME_FIELD, typeof(bool));
            baseDV.Table.Columns.Add(CTPriceControler.ISINCOMPLETE_FIELD, typeof(string));

            baseDV.Table.Columns.Add(CTPriceControler.TRADERNAME_FIELD, typeof(string));

            baseDV.Table.Columns.Add(CTPriceControler.TRANSACT_TIME_FILED, typeof(string));
            baseDV.Table.Columns.Add(CTPriceControler.DELIVERY_TYPE_FILED, typeof(string));
            baseDV.Table.Columns.Add(CTPriceControler.CLEARING_METHOD_FILED, typeof(string));
            baseDV.Table.Columns.Add(CTPriceControler.QUOTE_TYPE_FILED, typeof(string)); 

            baseDV.Table.Columns.Add(CTPriceControler.CUSTOMER_SOURCE_FIELD, typeof(string));
            baseDV.Table.Columns.Add(CTPriceControler.PARTY_NAME_FIELD, typeof(string)); 

            baseDV.Table.Columns.Add(CTPriceControler.IS_PERMISSION_FIELD, typeof(bool));

            baseDV.Table.Columns.Add(CTPriceControler.IS_MORE_FIELD, typeof(bool));

            baseDV.Table.Columns.Add(CTPriceControler.QUOTE_STATUS_FIELD, typeof(string));

            baseDV.Table.Columns.Add(CTPriceControler.VENUE_SORT_FIELD, typeof(string));

            baseDV.Table.Columns.Add(CTPriceControler.ISINNER_FIELD, typeof(string));
            //国债期货- 增加MarketIndicator
            baseDV.Table.Columns.Add(CTPriceControler.MARKETINDICATOR_FIELD, typeof(string));
            baseDV.Table.Columns.Add(CTPriceControler.MIN_CHANGE_FIELD, typeof(string));

            baseDV.Table.Columns.Add(CTPriceControler.Yield_Field, typeof(string));
            baseDV.Table.Columns.Add(CTPriceControler.CalFlag_Field, typeof(string));
            baseDV.Table.Columns.Add(CTPriceControler.PriceType_Field, typeof(string));
            baseDV.Table.Columns.Add(CTPriceControler.CalFlagFieldTip_Field, typeof(string));
            baseDV.Table.Columns.Add(CTPriceControler.CalFlagNetPriceTip_Field, typeof(string));
            baseDV.Table.Columns.Add(CTPriceControler.IsInnerInComplete_Field, typeof(string));
            baseDV.Table.Columns.Add(CTPriceControler.YieldNum_Field, typeof(decimal));
            baseDV.Table.Columns.Add(CTPriceControler.BOND_TYPE_FIELD, typeof(string));
            //_BaseDV.Sort = $"{VENUE_TYPE_GW_FIELD},{SOURCE_FIELD},{ENTRY_PX_FIELD},{MD_PRICE_LEVEL_FIELD}";
            baseDV.Sort = $"{CTPriceControler.SIDE_FIELD},{CTPriceControler.MD_PRICE_LEVEL_FIELD}";
            baseDV.Table.PrimaryKey = new DataColumn[] { baseDV.Table.Columns[CTPriceControler.KEY_FIELD] };

            return baseDV;
        }


        private void UpdateBondAsync(HomeGroupSecurityIDsInfo obj)
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
        private void UpdateBond(HomeGroupSecurityIDsInfo obj)
        {
            try
            {
                lock (lockObj)
                {
                    //DataRow bondInfosItems = this.BondView.Table.Rows.Find(new object[] { obj.securityid, obj.settltype, obj.venues, obj.venuetypegw });
                    DataRow bondInfosItems = this.BondView.Table.Rows.Find(new object[] { obj.securityid });

                    if (bondInfosItems == null)
                    {
                        bondInfosItems = this.BondView.Table.NewRow();
                        bondInfosItems[ID_FIELD] = obj.id;
                        // OpenClsoe
                        bondInfosItems[KEY_FIELD] = obj.securityid;
                        //bondInfosItems[SETTLE_TYPE_FIELD] = obj.settltype; // 清算速度
                        bondInfosItems[SECURITY_ID_FIELD] = obj.securityid; // 代码
                                                                            //bondInfosItems[VENUE_FIELD] = obj.venues;
                                                                            //bondInfosItems[VENUE_TYPEGW_FIELD] = obj.venuetypegw;

                        bool isPermission = AdminHandler.Instance.CheckPermisionById("4000", "4014");
                        bondInfosItems[KLINE_WIDTH_FIELD] = isPermission ? 26 : 0;

                        ////// 市场最新成交
                        ////MarketExecutionsItem lastExeItem = LastExecutionProcesser.Processer.GetLastExecutionsItem(obj.securityid);
                        ////if (lastExeItem != null)
                        ////{
                        ////    bondInfosItems[LAST_MARKET_PX_VENUES_FIELD] = lastExeItem.SourceTrader;
                        ////    bondInfosItems[DISPLAY_LAST_MARKET_PX_FIELD] = lastExeItem.ExecutePricePercent_Display;
                        ////    bondInfosItems[LAST_MARKET_PX_FIELD] = lastExeItem.ExecutePricePercent;
                        ////    string settltype_tag = ComboSourceUtil.Instance.FindEnum(lastExeItem.SettlType, "settletype");

                        ////    if (CommonUtils.IsBrokerSecurity(obj.securityid))
                        ////        bondInfosItems[LAST_SETTLTYPE_TAG_FIELD] = settltype_tag;
                        ////    else
                        ////        bondInfosItems[LAST_SETTLTYPE_TAG_FIELD] = string.Empty;

                        ////}
                        ///



                        bondInfosItems[REMAINGING_TIME_FIELD] = "";
                        bondInfosItems[SYMBOL_FIELD] = obj.symbol;

                        this.BondView.Table.Rows.Add(bondInfosItems);

                        bondWithOutInfoList[obj.securityid] = null;

                        //订阅price
                        //PriceHandler.Instance.RegisterH0Price(obj.securityid, Instance_OnMDPrice);
                        PriceHandler.Instance.RegisterCTPrice(obj.securityid, Instance_OnMDPrice);
                        //Console.WriteLine($"注册1-{obj.securityid}");
                        EmsOrdersHandler.Instance.RegistEMSOrdersBySecurityID(obj.securityid, EmsOrderProcesser.Processer.Instance_OnEmsOrders);

                        //市场最新成交
                        //ExecutionReport removeExe;
                        //ExecutionsHandler.Instance.ProcedGetLastExecutionDic.TryRemove(obj.securityid, out removeExe);
                        LastExecutionProcesser.Processer.AddGetLastExecutionQueue(obj.securityid);
                        // 更新我的订单数据 todo
                        //ExecutionReport er = OrdersHandler.Instance.GetCacheOrders(obj.securityid, obj.settltype, obj.venues, obj.venuetypegw);
                        //if (er != null)
                        //{
                        //    UpdateMyOrders(er);
                        //}
                    }

                    if (bondInfosItems != null)
                    {
                        // 如果更新时间是一样的, 就不要更新了
                        if (CommonUtils.ConvertToString(bondInfosItems[LAST_ACTION_TIME_FIELD]) == obj.updatetime)
                        {
                            return;
                        }

                        //bondInfosItems[SWITCH_FIELD] = obj.enable == "1" ? true : false; // 开关
                        bondInfosItems[LAST_ACTION_STATUS_FIELD] = obj.remark;
                        
                        bondInfosItems[LAST_ACTION_TIME_FIELD] = obj.updatetime;
                        //bondInfosItems[EXEC_VALUE_FIELD] = CommonUtils.ConvertToDecimal(obj.netqty);  // 今日净成交
                        bondInfosItems[CREATETIME_FIELD] = obj.createtime;
                    }
                    this.BondView.Table.AcceptChanges();

                    //自动更新债券编码
                    if (isLoadBond)
                    {
                        Task.Run(() =>
                        {
                            BondsInfoPo bondsInfoPo = CommonUtils.GetBondsInfoPoBySecurityID(obj.securityid);
                            if (bondsInfoPo != null)
                            {
                                Sub_OnLoadData(bondsInfoPo);
                            }
                        });
                    }
                }
            }
            catch (Exception e)
            {
                LogHandler.Error(e);
            }
        }
        

        /// <summary>
        /// 计算最优价格
        /// </summary>
        /// <param name="obj">报价数据</param>
        /// <param name="item">更新行</param>
        private void CalculateBestPrice(MarketDataSnapshotFullRefresh obj, DataRow item)
        {
            if(obj == null || item == null || baseCTPriceDataViewDic.ContainsKey(obj.SecurityID) == false)
            {
                return;
            }

            // 要根据筛选的清算速度算出最优价
            DataView priceBaseData = null;
            bool flag = baseCTPriceDataViewDic.TryGetValue(obj.SecurityID, out priceBaseData);
            if (flag == false)
            {
                return;
            }

            BestPrice1 currentBestBid = null;
            BestPrice1 currentBestOfr = null;
            BestPrice1 allBestBid = null;
            BestPrice1 allBestOfr = null;
            lock (priceBaseData)
            {
                // bid最优价格
                currentBestBid = CalcBestPrice(priceBaseData, "0", this._rowFilterStr);
                // ofr 最优价格
                currentBestOfr = CalcBestPrice(priceBaseData, "1", this._rowFilterStr);

                if (this._rowFilterStr.Length > 0)
                {
                    allBestBid = CalcBestPrice(priceBaseData, "0");
                    // ofr 最优价格
                    allBestOfr = CalcBestPrice(priceBaseData, "1");
                }
            }

            // 更新表格
            BindBestPriceToUI(item, currentBestBid, currentBestOfr);

            if (this._rowFilterStr.Length > 0)
            {
                item[BID_BBO_FIELD] = allBestBid == null || string.IsNullOrEmpty(allBestBid.bestPx) ? "- -" : allBestBid.bestPx;
                item[BID_TXT_FIELD] = allBestBid == null || string.IsNullOrEmpty(allBestBid.text) ? "- -" : allBestBid.text;
                item[BID_MAX_T1_QTY_FIELD] = allBestBid == null ? "- -" : allBestBid.T1Qty.ToString();
                item[OFR_BBO_FIELD] = allBestOfr == null || string.IsNullOrEmpty(allBestOfr.bestPx) ? "- -" : allBestOfr.bestPx;
                item[OFR_TXT_FIELD] = allBestOfr == null || string.IsNullOrEmpty(allBestOfr.text) ? "- -" : allBestOfr.text;
                item[OFR_MAX_T1_QTY_FIELD] = allBestOfr == null ? "- -" : allBestOfr.T1Qty.ToString();
            }
            else
            {
                item[BID_BBO_FIELD] = currentBestBid == null || string.IsNullOrEmpty(currentBestBid.bestPx) ? "- -" : currentBestBid.bestPx;
                item[BID_TXT_FIELD] = currentBestBid == null || string.IsNullOrEmpty(currentBestBid.text) ? "- -" : currentBestBid.text;
                item[BID_MAX_T1_QTY_FIELD] = currentBestBid == null ? "- -" : currentBestBid.T1Qty.ToString();
                item[OFR_BBO_FIELD] = currentBestOfr == null || string.IsNullOrEmpty(currentBestOfr.bestPx) ? "- -" : currentBestOfr.bestPx;
                item[OFR_TXT_FIELD] = currentBestOfr == null || string.IsNullOrEmpty(currentBestOfr.text) ? "- -" : currentBestOfr.text;
                item[OFR_MAX_T1_QTY_FIELD] = currentBestOfr == null ? "- -" : currentBestOfr.T1Qty.ToString();
            }

            // 更新最优价是否和我的订单一样标志
            BindIsSameTagToUI(obj.SecurityID, item);

            // 更新角标
            GenerateFlagType(item);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <param name="bidBP"></param>
        /// <param name="ofrBP"></param>
        private void BindBestPriceToUI(DataRow item,
            BestPrice1 bidBP,
            BestPrice1 ofrBP)
        {
            string min_change = CommonUtils.GetMinChangeBySecurityID(item[SECURITY_ID_FIELD].ToString());
            #region ofr
            if (ofrBP != null)
            {
                // 最优ofr price
                if (CommonUtils.IsNumeric(ofrBP.bestPx))
                {
                    decimal bestOfrPx = CommonUtils.ConvertToDecimal(ofrBP.bestPx);
                    item[BEST_OFR_PRICE_FIELD] = bestOfrPx;

                    string bestOfrPx_string = bestOfrPx.ToString("F4");
                    //根据最小价格变动单位决定价格显示位数
                    int digit = CommonUtils.GetDigit(min_change);
                    bestOfrPx_string = bestOfrPx.ToString("F"+digit);

                    int index = bestOfrPx_string.IndexOf(".");
                    if(index>=0)
                    {
                        item[BEST_OFR_PRICE_LEFT_FIELD] = bestOfrPx_string.Substring(0, index);
                        item[BEST_OFR_PRICE_RIGHT_FIELD] = bestOfrPx_string.Substring(index);
                    }
                    else
                    {
                        item[BEST_OFR_PRICE_LEFT_FIELD] = bestOfrPx_string;
                        item[BEST_OFR_PRICE_RIGHT_FIELD] = string.Empty;
                    }
                    
                }
                else
                {
                    item[BEST_OFR_PRICE_FIELD] = -1m;
                    item[BEST_OFR_PRICE_LEFT_FIELD] = "0";
                    item[BEST_OFR_PRICE_RIGHT_FIELD] = ".0000";
                }
                

                // 最优ofr qty
                item[BEST_OFR_QTY_FIELD] = ofrBP.bestQty;
                // 清算速度

                string securityID = item[SECURITY_ID_FIELD].ToString();
                if (CommonUtils.IsBrokerSecurity(securityID))
                {
                    item[BEST_OFR_SETTLTYPE_TAG_FIELD] = ofrBP.bestT0;
                    item[BEST_OFR_SETTLTYPE_YUAN_TAG_FIELD] = ofrBP.bestYuan;
                }
                else
                {
                    item[BEST_OFR_SETTLTYPE_TAG_FIELD] = string.Empty;
                    item[BEST_OFR_SETTLTYPE_YUAN_TAG_FIELD] = string.Empty;
                }

                    // 闪电标志
                    item[ISALLFIRM_OFR] = ofrBP.isAllFirm;
                    // 角标判定标识（行情维度：本人/本机构是否有报价）
                    item[OFR_HAS_ME_FIELD] = ofrBP.hasMe;
                    item[OFR_HAS_OUR_FIELD] = ofrBP.hasOur;
            }
            #endregion

            #region bid
            if (bidBP != null)
            {
                // 最优bid price
                if (CommonUtils.IsNumeric(bidBP.bestPx))
                {
                    decimal bestBidPx = CommonUtils.ConvertToDecimal(bidBP.bestPx);
                    item[BEST_BID_PRICE_FIELD] = bestBidPx;

                    string bestBidPx_string = bestBidPx.ToString("F4");
                    //根据最小价格变动单位决定价格显示位数
                    int digit = CommonUtils.GetDigit(min_change);
                    bestBidPx_string = bestBidPx.ToString("F" + digit);

                    int index = bestBidPx_string.IndexOf(".");
                    if (index >= 0)
                    {
                        item[BEST_BID_PRICE_LEFT_FIELD] = bestBidPx_string.Substring(0, index);
                        item[BEST_BID_PRICE_RIGHT_FIELD] = bestBidPx_string.Substring(index);
                    }
                    else
                    {
                        item[BEST_BID_PRICE_LEFT_FIELD] = bestBidPx_string;
                        item[BEST_BID_PRICE_RIGHT_FIELD] = string.Empty;
                    }
                }
                else
                {
                    item[BEST_BID_PRICE_FIELD] = -1m;
                    item[BEST_BID_PRICE_LEFT_FIELD] = "0";
                    item[BEST_BID_PRICE_RIGHT_FIELD] = ".0000";
                }
                

                // 最优bid qty
                item[BEST_BID_QTY_FIELD] = bidBP.bestQty;
                // 清算速度
                string securityID = item[SECURITY_ID_FIELD].ToString();
                if (CommonUtils.IsBrokerSecurity(securityID))
                {
                    item[BEST_BID_SETTLTYPE_TAG_FIELD] = bidBP.bestT0;
                    item[BEST_BID_SETTLTYPE_YUAN_TAG_FIELD] = bidBP.bestYuan;
                }
                else
                {
                    item[BEST_BID_SETTLTYPE_TAG_FIELD] = string.Empty;
                    item[BEST_BID_SETTLTYPE_YUAN_TAG_FIELD] = string.Empty;
                }

                // 闪电标志
                item[ISALLFIRM_BID] = bidBP.isAllFirm;
                // 角标判定标识（行情维度：本人/本机构是否有报价）
                item[BID_HAS_ME_FIELD] = bidBP.hasMe;
                item[BID_HAS_OUR_FIELD] = bidBP.hasOur;
            }
            #endregion
        }

        /// <summary>
        /// 更新最优价格是否和我的订单价格一样标志
        /// </summary>
        /// <param name="securityID"></param>
        /// <param name="item"></param>
        private void BindIsSameTagToUI(string securityID, DataRow item)
        {
            // 我的订单
            DataRowView bidOrder = MyOrderProcesser.Processer.GetLstOrderBySecurityID(securityID, "1");
            if (bidOrder != null)
            {
                // 最优价格 我的订单 比较， 是否相同
                decimal bestBidPrice = CommonUtils.ConvertToDecimal(item[BEST_BID_PRICE_FIELD].ToString());
                string myLstBidPrice = CommonUtils.ConvertToString(bidOrder[MyOrderProcesser.PRICE_ORDER_FIELD]);
                decimal myBidOrderPrice = CommonUtils.ConvertToDecimal(myLstBidPrice);

                if (bestBidPrice == myBidOrderPrice && CommonUtils.IsNumeric(item[BEST_BID_PRICE_FIELD].ToString()) && CommonUtils.IsNumeric(myLstBidPrice))
                {
                    item[IS_BID_SAME_FIELD] = "1";
                }
                else
                {
                    item[IS_BID_SAME_FIELD] = "0";
                }
            }
            else
            {
                if (item != null)
                    item[IS_BID_SAME_FIELD] = "0";
            }

            DataRowView ofrOrder = MyOrderProcesser.Processer.GetLstOrderBySecurityID(securityID, "2");
            if (ofrOrder != null)
            {
                // 最优价格 我的订单 比较， 是否相同
                decimal bestOfrPrice = CommonUtils.ConvertToDecimal(item[BEST_OFR_PRICE_FIELD].ToString());
                string myLstOfrPrice = CommonUtils.ConvertToString(ofrOrder[MyOrderProcesser.PRICE_ORDER_FIELD]);
                decimal myOfrOrderPrice = CommonUtils.ConvertToDecimal(myLstOfrPrice);

                if (bestOfrPrice == myOfrOrderPrice && CommonUtils.IsNumeric(item[BEST_OFR_PRICE_FIELD].ToString()) && CommonUtils.IsNumeric(myLstOfrPrice))
                {
                    item[IS_OFR_SAME_FIELD] = "1";
                }
                else
                {
                    item[IS_OFR_SAME_FIELD] = "0";
                }
            }
            else
            {
                if (item != null)
                    item[IS_OFR_SAME_FIELD] = "0";
            }

            if (item != null)
                UpdateMyBidOfr(securityID, item);
        }

        private void UpdateMyBidOfr(string securityID, DataRow item)
        {
            //更新MyBid
            DataRow bidOrder = MyOrderProcesser.Processer.GetMyBestOrderBySecurityId(securityID, CommonConstants.SIDE_BID_ONE, string.Empty, string.Empty);
            if (bidOrder != null)
            {
                item[MY_BID_FIELD] = bidOrder[MyOrderProcesser.PRICE_ORDER_FIELD];
                string dealQty = bidOrder[MyOrderProcesser.EXECQTY_ORDER_FIELD].ToString();
                string qty = bidOrder[MyOrderProcesser.QTY_ORDER_FIELD].ToString();

                item[MY_BID_QTY_FIELD] = PriceCalHelper.Instance.CalSurplusQty(qty, dealQty);
            }
            else
            {
                item[MY_BID_FIELD] = string.Empty;
                item[MY_BID_QTY_FIELD] = string.Empty;
            }

            //更新MyOfr
            DataRow ofrOrder = MyOrderProcesser.Processer.GetMyBestOrderBySecurityId(securityID, CommonConstants.SIDE_OFR_TWO, string.Empty, string.Empty);
            if (ofrOrder != null)
            {
                item[MY_OFR_FIELD] = ofrOrder[MyOrderProcesser.PRICE_ORDER_FIELD];

                string dealQty = ofrOrder[MyOrderProcesser.EXECQTY_ORDER_FIELD].ToString();
                string qty = ofrOrder[MyOrderProcesser.QTY_ORDER_FIELD].ToString();

                item[MY_OFR_QTY_FIELD] = PriceCalHelper.Instance.CalSurplusQty(qty, dealQty);
            }
            else
            {
                item[MY_OFR_FIELD] = string.Empty;
                item[MY_OFR_QTY_FIELD] = string.Empty;
            }
        }

        /// <summary>
        /// 把基础剩余时间转换成时间段
        /// 如：2Y+5D -> 2 * 365
        /// </summary>
        /// <param name="remainingTime">基础剩余时间</param>
        /// <returns>筛选时间段</returns>
        private int ConvertRemainingTime(string remainingTime)
        {
            string remainingTimeLevel = string.Empty;
            int convertTime = 0;
            if (string.IsNullOrEmpty(remainingTime) == false)
            {
                string[] splitArr = remainingTime.Split(CommonConstants.PLUS_CHAR, StringSplitOptions.RemoveEmptyEntries);
                if (splitArr != null && splitArr.Length > 0)
                {
                    //string firstTime_str = splitArr[0].Trim();
                    //if (firstTime_str.EndsWith("Y"))
                    //{
                    //    string yearStr = firstTime_str.Replace("Y", "");
                    //    decimal year = CommonUtils.ConvertToDecimal(yearStr);
                    //    convertTime = (int)(365 * year);
                    //}
                    //else if (firstTime_str.EndsWith("D"))
                    //{
                    //    string dayStr = firstTime_str.Replace("D", "");
                    //    int day = CommonUtils.ConvertToInt(dayStr);
                    //    convertTime = day;
                    //}

                    for (int i = 0; i < splitArr.Length; i++)
                    {
                        string data;
                        data = splitArr[i].Trim();
                        if (data.EndsWith("Y"))
                        {
                            string yearStr = data.Replace("Y", "");
                            decimal year = CommonUtils.ConvertToDecimal(yearStr);
                            convertTime = convertTime + (int)(365 * year);
                        }
                        else if (data.EndsWith("D"))
                        {
                            string dayStr = data.Replace("D", "");
                            int day = CommonUtils.ConvertToInt(dayStr);
                            convertTime = convertTime + day;
                        }
                    }
                }
            }

            return convertTime;
        }

        /// <summary>
        /// 打开tradebook界面
        /// </summary>
        public void OpenTradeBook()
        {
            if (this.SelectedItem != null)
            {
                // TradeBookView view = new TradeBookView();
                // QuotationListViewModel viewModel = new QuotationListViewModel();
                // viewModel.SecurityID = CommonUtils.ConvertToString(this.SelectedItem[SECURITY_ID_FIELD]);
                // viewModel.Symbol = CommonUtils.ConvertToString(this.SelectedItem[SYMBOL_FIELD]);
                // viewModel.FullName = CommonUtils.ConvertToString(this.SelectedItem[SYMBOL_FIELD]);//TODO： 这个还没有
                // view.DataContext = viewModel;

                //TabPageHelper.OpenTabPage(viewModel.SecurityID, view, -1);

                // 使用插件形式打开深度行情页面
                string securityID = CommonUtils.ConvertToString(this.SelectedItem[SECURITY_ID_FIELD]);
                int tradeBookPluginKey = (int)PluginKeyEnum.TradeBookPlugin;

                IPlugin plugin = AppUtils.Application.PluginManager.GetPlugin(tradeBookPluginKey);
                if (plugin != null)
                {
                    plugin.PluginData = securityID;
                    AppUtils.Application.PluginManager.EnablePlugin(tradeBookPluginKey);
                    plugin.PluginData = null;
                    // pluginKey是22，表示TradeBook插件， 可以多次打开
                    // 到load方法中判断 是否需要新建页面
                    if (plugin.PluginKey == tradeBookPluginKey)
                    {
                        plugin.Enabled = false;
                    }
                }
            }
        }

        private void UpdateMyOrdersAsync(string securityID)
        {
            if (Application.Current != null)
            {
                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    UpdateMyOrders(securityID);
                }));
            }
        }


        /// <summary>
        /// 更新我的订单
        /// </summary>
        /// <param name="obj"></param>
        private void UpdateMyOrders(string securityID)
        {
            try
            {
                lock (this.lockObj)
                {

                    DataRow bondInfosItems = this.BondView.Table.Rows.Find(new object[] { securityID });

                    if (bondInfosItems != null)
                    {
                        BindIsSameTagToUI(securityID, bondInfosItems);
                        // 本人订单变化时重新计算小三角，否则本人下单/撤单后角标不刷新
                        GenerateFlagType(bondInfosItems);
                    }
                }
            }
            catch (Exception e)
            {
                LogHandler.Error(e);
            }
        }

        //private void UpdateLastExecutionsAsync(ExecutionReport obj)
        //{
        //    UpdateLastExecutions(obj);
        //}
        private object lockobj = new object();
        /// <summary>
        /// 待获取和计算的最新成交
        /// </summary>
        /// <param name="cancellationToken"></param>
        private void DoProcLastExecutionQueue(CancellationToken cancellationToken)
        {
            try
            {
                Common.Model.Fix.Message.ExecutionReport obj = null;
                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        semaphore.Wait(); // 控制最多只能有 5 个并发线程
                        // 尝试从队列中取数据，等待10ms避免阻塞主线程
                        obj = queue.Take(cancellationToken);

                        string primaryKey = netapi.Api.ApiManager.Instance.GetDefaultApi().getTopic(Common.Api.ApiConsts.Topic_stc_md_prefix, ApiConsts.CT, LocalEnv.UserInfo.Termids, obj.SecurityID, string.Empty, string.Empty);
                        if (STC.Common.Utils.PriceHandler.Instance._UpdateCTCallbackDic.ContainsKey(primaryKey))
                        {
                            // 创建一个任务来处理数据
                            Task.Factory.StartNew(()=>
                            {
                                try
                                {
                                    UpdateLastExecutions(obj);
                                }
                                finally
                                {
                                    semaphore.Release(); // 处理完数据后，释放信号量
                                    //LogHelper.Error($"正常释放信号量,当前个数 {semaphore.CurrentCount}");
                                }
                            });
                        }
                        else
                        {
                            semaphore.Release(); // 不符合要求的数据，直接释放信号量
                            //LogHelper.Error($"不符合要求的数据，直接释放信号量,当前个数 {semaphore.CurrentCount}");
                        }
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Error(ex.Message);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Expected exception on cancellation
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex.Message);
            }
        }
        

        /// <summary>
        /// 更新最近成交
        /// </summary>
        /// <param name="obj"></param>
        private void UpdateLastExecutions(ExecutionReport obj)
        {
            System.Windows.Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                DataRow dr = this.BondView.Table.Rows.Find(obj.SecurityID);
                if (dr != null)
                {
                    dr[LAST_MARKET_PX_VENUES_FIELD] = obj.Venues ?? "";

                    if (obj.SecurityID.EndsWith(".SH") || obj.SecurityID.EndsWith(".SZ"))
                    {
                        dr[DISPLAY_LAST_MARKET_PX_FIELD] = CommonUtils.FormatPriceN3(obj.NetPrice ?? ""); //用于显示成交价
                        dr[LAST_MARKET_PX_FIELD] = CommonUtils.ConvertToDecimal(obj.NetPrice ?? ""); // 成交价
                    }
                    else
                    {
                        dr[DISPLAY_LAST_MARKET_PX_FIELD] = CommonUtils.FormatPrice(obj.Yield ?? ""); //用于显示成交价
                        dr[LAST_MARKET_PX_FIELD] = CommonUtils.ConvertToDecimal(obj.Yield ?? ""); // 成交价
                    }

                    if (!string.IsNullOrEmpty(obj.SettlType))
                    {
                        string settltype_tag = ComboSourceUtil.Instance.FindEnum(obj.SettlType, "settletype");
                        if (CommonUtils.IsBrokerSecurity(obj.SecurityID))
                            dr[LAST_SETTLTYPE_TAG_FIELD] = settltype_tag;
                        else
                            dr[LAST_SETTLTYPE_TAG_FIELD] = string.Empty;
                    }

                   // this.BondView.Table.AcceptChanges();
                }
            }));
        }

        private void DetailDT_RowDeleting(object sender, DataRowChangeEventArgs e)
        {
            lock (this.lockObj)
            {
                if (e.Row != null)
                {
                    string securityID = e.Row[EmsOrderProcesser.SECURITY_ID_FIELD].ToString();
                    DataRow[] drs = this.BondView.Table.Select($"{KEY_FIELD} = '{securityID}'");
                    if (drs != null && drs.Length > 0)
                    {
                        foreach (DataRow item in drs)
                        {
                            if (item != null)
                            {
                                GenerateFlagType(item);
                            }
                        }
                    }
                }
            }
        }


        private void DetailDT_RowChanged(object sender, DataRowChangeEventArgs e)
        {
            if (e.Action == DataRowAction.Add)
            {
                lock (this.lockObj)
                {
                    if (e.Row != null)
                    {
                        string securityID = e.Row[EmsOrderProcesser.SECURITY_ID_FIELD].ToString();
                        DataRow[] drs = this.BondView.Table.Select($"{KEY_FIELD} = '{securityID}'");
                        if (drs != null && drs.Length > 0)
                        {
                            foreach (DataRow item in drs)
                            {
                                if (item != null)
                                {
                                    GenerateFlagType(item);
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 生成角标
        /// </summary>
        /// <param name="drv"></param>
        /// <returns>角标标记</returns>
        private void GenerateFlagType(DataRow dr)
        {
            try
            {
                string securtityID = CommonUtils.ConvertToString(dr[SECURITY_ID_FIELD]);

                // 角标标志（与 xc QuotationListViewModel 口径一致）：
                // 本人/本机构是否有报价来自行情数据（CTPriceControler）：
                //   本人  = 行情行 IsMe=true；
                //   本机构 = 行情行 Source∈{IDB,QUANT,MANUAL}。
                // 这两个标识在 BindBestPriceToUI 时按方向写入 item。
                // 不再用 EMS 订单计数判定——EMS 通道对本机构其他人不下发数据，会导致“本机构有报价”角标算不出来。
                bool hasBidMe = dr[BID_HAS_ME_FIELD] is bool && (bool)dr[BID_HAS_ME_FIELD];
                bool hasBidOur = dr[BID_HAS_OUR_FIELD] is bool && (bool)dr[BID_HAS_OUR_FIELD];
                bool hasOfrMe = dr[OFR_HAS_ME_FIELD] is bool && (bool)dr[OFR_HAS_ME_FIELD];
                bool hasOfrOur = dr[OFR_HAS_OUR_FIELD] is bool && (bool)dr[OFR_HAS_OUR_FIELD];

                // 本人维度兜底：本人刚下单时行情可能还没回来，以本人订单表为准补充。
                bool myBid = hasBidMe
                    || MyOrderProcesser.Processer.GetLstOrderBySecurityID(securtityID, "1") != null;
                bool myOfr = hasOfrMe
                    || MyOrderProcesser.Processer.GetLstOrderBySecurityID(securtityID, "2") != null;

                dr[BEST_BID_FLAGTYPE_FIELD] = GetFlagTypeValue(myBid, hasBidOur);
                dr[BEST_OFR_FLAGTYPE_FIELD] = GetFlagTypeValue(myOfr, hasOfrOur);
            }
            catch(Exception e)
            {
                LogHandler.Error(e);
            }
        }

        /// <summary>
        /// 根据“本人有报价”和“本机构其他人有报价”计算小三角角标类型。
        /// 1：本机构和本人都有报价；2：仅本人有报价；3：仅本机构其他人有报价；0：无报价。
        /// </summary>
        private string GetFlagTypeValue(bool isMine, bool isParty)
        {
            if (isMine && isParty)
            {
                return "1";
            }
            if (isMine)
            {
                return "2";
            }
            if (isParty)
            {
                return "3";
            }
            return "0";
        }

        public async void AddAndUpdateBond(HomeGroupSecurityidsUpdateReq req)
        {
            try
            {
                req.groupid = groupID;
                PResult result = await HomePageDataHandler.AsyncUpdateSecurityids(req);
                if (result != null && result.Code == "0000")
                {
                    Common.View.Component.CustomMessageBox.Info($"{req.remark}\r\n 关注债券新增成功！", "新增债券");
                    //SyncBonds(true);
                    await HomePageDataHandler.AsyncRequestGroups(true);
                    SyncBonds(false);
                    HomePageDataHandler.Notify(HomePageDataHandler.FavoriteTypeEnum.AddBond, this.groupID, req.securityid);
                }
                else
                {
                    Common.View.Component.CustomMessageBox.Warning(result.Msg, "新增债券");
                }
            }
            catch (Exception e)
            {
                LogHandler.Error(e);
            }
        }


        private Dictionary<string, string> DictHomeGroupInfo = new Dictionary<string, string>();

        public async void SyncBonds(bool isUpdateFromServer)
        {
            IOrderedEnumerable<KeyValuePair<string, HomeGroupInfo>> dic = await HomePageDataHandler.AsyncRequestGroups(isUpdateFromServer);
            HomeGroupInfo homeGroupInfo = HomePageDataHandler.GetHomeGroupInfo(dic, groupID);
            if (homeGroupInfo != null)
            {
                Instance_OnBondList(homeGroupInfo.securityids);
            }
            
        }

        public async void DelBond()
        {
            if (SelectedItem == null)
            {
                return;
            }
            string id = CommonUtils.ConvertToString(SelectedItem[FavoriteBondsListBoxViewModel.ID_FIELD]);
            string securityID = CommonUtils.ConvertToString(SelectedItem[FavoriteBondsListBoxViewModel.SECURITY_ID_FIELD]);

            HomeGroupSecurityidsUpdateReq req = new HomeGroupSecurityidsUpdateReq();
            req.groupid = groupID;
            req.id = id;
            req.action = "3"; //删除;
            PResult result = await HomePageDataHandler.AsyncUpdateSecurityids(req);

            if (result != null && result.Code == "0000")
            {
                //// 取消订阅
                //PriceHandler.Instance.UnregisterH0Price(securityID, Instance_OnMDPrice);

                //// 删除该项
                //lock (lockObj)
                //{
                //    this.BondView.Table.Rows.Remove(SelectedItem.Row);
                //}

                await HomePageDataHandler.AsyncRequestGroups(true);
                HomePageDataHandler.Notify(HomePageDataHandler.FavoriteTypeEnum.DelBond, groupID, securityID);

                // 清空搜索框
                this.SearchText = string.Empty;
            }

        }

        public void SortDv(string columnName, MyListSortDirection sortDirection)
        {
            string sortDir = sortDirection == MyListSortDirection.Descending ? "desc" : "asc";
            this.BondView.Sort = $"{columnName} {sortDir}";
        }

        public string ConvertColumnNameFromHeader(string header)
        {
            switch (header)
            {
                case "债券基本信息":
                case "债券代码":
                    return SECURITY_ID_FIELD;
                case "剩余期限":
                    return REMAINGING_TIME_CONVERT_FIELD;
                default:
                    return null;
            }

        }

        private void CopyBid(DataRowView item)
        {
            try
            {
                if (item == null)
                {
                    if (SelectedItem == null)
                    {
                        Common.View.Component.CustomMessageBox.Info("请先选择债券！", "复制bid");
                        return;
                    }
                    else
                    {
                        item = this.SelectedItem;
                    }
                }
                
                string securityID = CommonUtils.ConvertToString(item[SECURITY_ID_FIELD]);
                string securityName = CommonUtils.ConvertToString(item[SYMBOL_FIELD]);
                decimal bid_px_decimal = CommonUtils.ConvertToDecimal(CommonUtils.ConvertToString(item[BEST_BID_PRICE_FIELD]));
                string bid_px = bid_px_decimal < 0 ? "--" : bid_px_decimal.ToString("N4") + "%";
                string bid_qty = CommonUtils.ConvertToString(item[BEST_BID_QTY_FIELD]);
                bid_qty = bid_qty == "0" ? "--" : bid_qty;

                StringBuilder sb = new StringBuilder();
                sb.Append("【复制bid】 ");
                sb.Append(securityID);
                sb.Append(" ");
                sb.Append(securityName);
                sb.Append(" ");
                sb.Append("bid ");
                sb.Append(bid_px);
                sb.Append(" ");
                sb.Append(bid_qty);
                sb.Append("w ");

                StringBuilder html = new StringBuilder();
                html.Append($"<p>{securityID}&#9;{securityName}&#9;bid&#9;{bid_px}&#9;{bid_qty}w&#9;</p>");

                StringBuilder plainTxt = new StringBuilder();
                plainTxt.Append($"{securityID}\t{securityName}\tbid\t{bid_px}\t{bid_qty}w\t");

                //复制
                ClipboardHelper.CopyToClipboard(html.ToString(), plainTxt.ToString());

                //展示
                HomePageCopyWindow copyWin = new HomePageCopyWindow();
                copyWin.tbRefer.Text = sb.ToString();
                copyWin.Show();
            }
            catch (Exception e)
            {
                LogHandler.Error(e);
            }
        }

        private void CopyOfr(DataRowView item)
        {
            try
            {
                if (item == null)
                {
                    if (SelectedItem == null)
                    {
                        Common.View.Component.CustomMessageBox.Info("请先选择债券！", "复制ofr");
                        return;
                    }
                    else
                    {
                        item = this.SelectedItem;
                    }
                }

                string securityID = CommonUtils.ConvertToString(item[SECURITY_ID_FIELD]);
                string securityName = CommonUtils.ConvertToString(item[SYMBOL_FIELD]);
                decimal ofr_px_decimal = CommonUtils.ConvertToDecimal(CommonUtils.ConvertToString(item[BEST_OFR_PRICE_FIELD]));
                string ofr_px = ofr_px_decimal < 0 ? "--" : ofr_px_decimal.ToString("N4") + "%";
                string ofr_qty = CommonUtils.ConvertToString(item[BEST_OFR_QTY_FIELD]);
                ofr_qty = ofr_qty == "0" ? "--" : ofr_qty;

                StringBuilder sb = new StringBuilder();
                sb.Append("【复制ofr】 ");
                sb.Append(securityID);
                sb.Append(" ");
                sb.Append(securityName);
                sb.Append(" ");
                sb.Append("ofr ");
                sb.Append(ofr_px);
                sb.Append(" ");
                sb.Append(ofr_qty);
                sb.Append("w ");

                StringBuilder html = new StringBuilder();
                html.Append($"<p>{securityID}&#9;{securityName}&#9;ofr&#9;{ofr_px}&#9;{ofr_qty}w&#9;</p>");

                StringBuilder plainTxt = new StringBuilder();
                plainTxt.Append($"{securityID}\t{securityName}\tofr\t{ofr_px}\t{ofr_qty}w\t");

                //复制
                ClipboardHelper.CopyToClipboard(html.ToString(), plainTxt.ToString());

                //展示
                HomePageCopyWindow copyWin = new HomePageCopyWindow();
                copyWin.tbRefer.Text = sb.ToString();
                copyWin.Show();

            }
            catch (Exception e)
            {
                LogHandler.Error(e);
            }
        }

        private void CopyDouble(DataRowView item)
        {
            try
            {
                if (item == null)
                {
                    if (SelectedItem == null)
                    {
                        Common.View.Component.CustomMessageBox.Info("请先选择债券！", "复制双边");
                        return;
                    }
                    else
                    {
                        item = this.SelectedItem;
                    }
                }

                string securityID = CommonUtils.ConvertToString(item[SECURITY_ID_FIELD]);
                string securityName = CommonUtils.ConvertToString(item[SYMBOL_FIELD]);
                decimal bid_px_decimal = CommonUtils.ConvertToDecimal(CommonUtils.ConvertToString(item[BEST_BID_PRICE_FIELD]));
                string bid_px = bid_px_decimal < 0 ? "--" : bid_px_decimal.ToString("N4") + "%";
                string bid_qty = CommonUtils.ConvertToString(item[BEST_BID_QTY_FIELD]);
                bid_qty = bid_qty == "0" ? "--" : bid_qty;
                decimal ofr_px_decimal = CommonUtils.ConvertToDecimal(CommonUtils.ConvertToString(item[BEST_OFR_PRICE_FIELD]));
                string ofr_px = ofr_px_decimal < 0 ? "--" : ofr_px_decimal.ToString("N4") + "%";
                string ofr_qty = CommonUtils.ConvertToString(item[BEST_OFR_QTY_FIELD]);
                ofr_qty = ofr_qty == "0" ? "--" : ofr_qty;

                StringBuilder sb = new StringBuilder();
                sb.Append("【复制双边】 ");
                sb.Append(securityID);
                sb.Append(" ");
                sb.Append(securityName);
                sb.Append(" ");
                sb.Append("bid ");
                sb.Append(bid_px);
                sb.Append(" ");
                sb.Append(bid_qty);
                sb.Append("w / ofr ");
                sb.Append(ofr_px);
                sb.Append(" ");
                sb.Append(ofr_qty);
                sb.Append("w ");

                StringBuilder html = new StringBuilder();
                html.Append($"<p>{securityID}&#9;{securityName}&#9;bid&#9;{bid_px}&#9;{bid_qty}w&#9;/&#9;ofr&#9;{ofr_px}&#9;{ofr_qty}w&#9;</p>");

                StringBuilder plainTxt = new StringBuilder();
                plainTxt.Append($"{securityID}\t{securityName}\tbid\t{bid_px}\t{bid_qty}w\t/\tofr\t{ofr_px}\t{ofr_qty}w\t");

                //复制
                ClipboardHelper.CopyToClipboard(html.ToString(), plainTxt.ToString());

                //展示
                HomePageCopyWindow copyWin = new HomePageCopyWindow();
                copyWin.tbRefer.Text = sb.ToString();
                copyWin.Show();

            }
            catch (Exception e)
            {
                LogHandler.Error(e);
            }
            
        }

        /// <summary>
        /// 打开行情小窗
        /// </summary>
        public async void OpenSecurity()
        {
            if (SelectedItem == null)
            {
                return;
            }
            string securityID = CommonUtils.ConvertToString(SelectedItem[FavoriteBondsListBoxViewModel.SECURITY_ID_FIELD]);
            TradeBookHandler.TBookViewHandler.Instance.OpenQuotesMiniWindows(securityID);
        }


        #region 市场成交处理完成,重新获取最后成交

        private void OnLoadtMarketExecutionsStatus(bool isok)
        {
            if (isok)
            {
                IOrderedEnumerable<KeyValuePair<string, HomeGroupInfo>> groupList = HomePageDataHandler.RequestGroups(false);
                if (groupList != null)
                {
                    foreach (KeyValuePair<string, HomeGroupInfo> group in groupList)
                    {
                        if (group.Key == this.groupID)
                        {
                            _initBondNum = group.Value.securityids.Count;
                            foreach (HomeGroupSecurityIDsInfo obj in group.Value.securityids)
                            {
                                //获取最优价
                                LastExecutionProcesser.Processer.AddGetLastExecutionQueue(obj.securityid);
                            }
                        }
                    }
                }
            }
        }
        #endregion

        #region 生成测试数据
        private void GenerateTestData()
        {
            System.Threading.ThreadPool.QueueUserWorkItem(o =>
            {
                while (true)
                {
                    //// 债券列表
                    //Bond bond1 = new Bond();
                    //bond1.closeby = "kong2";
                    //bond1.createtime = DateTime.Now.ToString("hh:mm:ss");
                    //bond1.enable = "0";
                    //bond1.ip = "172.16.97.119";
                    //bond1.remark = "手动关闭";
                    //bond1.securityid = "170210.IB";
                    //bond1.settltype = "2";
                    //bond1.symbol = "17国开10";
                    //bond1.terminal = "";
                    //bond1.updatetime = DateTime.Now.ToString("hh:mm:ss");
                    //bond1.username = "";
                    //bond1.venues = "ODM";
                    //bond1.venuetypegw = "ODM";
                    //UpdateBond(bond1);

                    //Bond bond2 = new Bond();
                    //bond2.closeby = "kong2";
                    //bond2.createtime = DateTime.Now.ToString("hh:mm:ss");
                    //bond2.enable = "1";
                    //bond2.ip = "172.16.97.119";
                    //bond2.remark = "发出报价";
                    //bond2.securityid = "180210.IB";
                    //bond2.settltype = "2";
                    //bond2.symbol = "19信地03";
                    //bond2.terminal = "";
                    //bond2.updatetime = DateTime.Now.ToString("hh:mm:ss");
                    //bond2.username = "";
                    //bond2.venues = "ODM";
                    //bond2.venuetypegw = "ODM";
                    //UpdateBond(bond2);

                    ////18新大陆
                    //Bond bond3 = new Bond();
                    //bond3.closeby = "kong2";
                    //bond3.createtime = DateTime.Now.ToString("hh:mm:ss");
                    //bond3.enable = "1";
                    //bond3.ip = "172.16.97.119";
                    //bond3.remark = "双边撤单";
                    //bond3.securityid = "155116.SH";
                    //bond3.settltype = "2";
                    //bond3.symbol = "18新大陆";
                    //bond3.terminal = "";
                    //bond3.updatetime = DateTime.Now.ToString("hh:mm:ss");
                    //bond3.username = "";
                    //bond3.venues = "ODM";
                    //bond3.venuetypegw = "ODM";
                    //UpdateBond(bond3);

                    //this._bondList.Add(bond1);
                    //this._bondList.Add(bond2);
                    //this._bondList.Add(bond3);

                    //TODO: 行情走势(CT，现无)

                    // 行情数据(SOR)

                    MarketDataSnapshotFullRefresh mdSnapshot = new MarketDataSnapshotFullRefresh();
                    mdSnapshot.MDBookType = "0";
                    mdSnapshot.MDReqID = "";
                    mdSnapshot.MarketIndicator = "4";
                    // ofr
                    mdSnapshot.NoMDEntries = new List<GNoMDEntries>();
                    GNoMDEntries gNoMdEntries = new GNoMDEntries();
                    gNoMdEntries.MDEntryDate = DateTime.Now.ToString("yyyyMMdd"); // 报价日期（yyyyMMdd）
                    gNoMdEntries.MDEntryID = "679733511237750784"; //报价编号(对应报价更新里的MDEntryID)
                    gNoMdEntries.MDEntryPx = "5.25"; //价格：债券指收益率
                    gNoMdEntries.NetPrice = "5.25"; //净价
                    gNoMdEntries.MDEntrySize = "1000";//量
                    gNoMdEntries.MDEntryTime = DateTime.Now.ToString("hh:mm:ss.fff");//时间
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
                    gNoMdEntries1.MDEntryDate = DateTime.Now.ToString("yyyyMMdd"); // 报价日期（yyyyMMdd）
                    gNoMdEntries1.MDEntryID = "679733511237750784"; //报价编号(对应报价更新里的MDEntryID)
                    gNoMdEntries1.MDEntryPx = "5.25"; //价格：债券指收益率
                    gNoMdEntries1.NetPrice = "5.25"; //净价
                    gNoMdEntries1.MDEntrySize = "1000";//量
                    gNoMdEntries1.MDEntryTime = DateTime.Now.ToString("hh:mm:ss.fff");//时间
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
                    DataView dv = CreateBaseDataView();
                    CTPriceControler.UpdateDataToBaseDV(dv, mdSnapshot);
                    UpdateBestPrice(dv, mdSnapshot);

                    Thread.Sleep(1000);
                }
            });
        }


        #endregion

        #region 导入导出
        public async void ImportCSV()
        {

            try
            {
                //if (MMGroupHandler.Instance.HasOpen())
                //{
                //    Prompt2SecondsMsg("请先关闭做市中的债券");
                //    return;
                //}
                // 1.弹框
                //创建一个保存文件式的对话框
                System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog();                              //设置这个对话框的起始保存路径
                ofd.InitialDirectory = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                ofd.RestoreDirectory = true;
                //设置保存的文件的类型，注意过滤器的语法
                ofd.Filter = "Excel Documents|*.xls;*.xlsx";
                ofd.DefaultExt = ".xlsx";
                //调用ShowDialog()方法显示该对话框，该方法的返回值代表用户是否点击了确定按钮
                string promptTxt = string.Empty;
                if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    if (!ofd.FileName.EndsWith(".xls", true, System.Globalization.CultureInfo.CurrentCulture)
                        && !ofd.FileName.EndsWith(".xlsx", true, System.Globalization.CultureInfo.CurrentCulture))
                    {
                        CommonUtils.Prompt2Second("批量导入", "当前只支持xls文件和xlsx文件");
                        return;
                    }

                    // 2.等待
                    this.MaskVisibility = Visibility.Visible;

                    // 3.导入操作
                    List<string> errorList = await Sub_ImportExcel(ofd.FileName);


                    if (errorList != null && errorList.Count > 0)
                    {
                        StringBuilder errors = new StringBuilder();
                        foreach (string error in errorList)
                        {
                            if (errors.Length > 0)
                            {
                                errors.AppendLine();
                            }
                            errors.Append(error);
                        }
                        promptTxt = errors.ToString();
                    }
                    else
                    {
                        promptTxt = "导入成功";
                    }
                }
                else
                {
                    promptTxt = "取消导入";
                }

                this.MaskVisibility = Visibility.Collapsed;
                if (string.IsNullOrEmpty(promptTxt) == false)
                {
                    CommonUtils.Prompt2Second("批量导入", promptTxt);
                }
            }
            catch (Exception e)
            {
                this.MaskVisibility = Visibility.Collapsed;
                CommonUtils.Prompt2Second("批量导入", e.Message);
                LogHelper.Error(e);
            }
        }

        private Task<List<string>> Sub_ImportExcel(string filePath)
        {
            return Task<List<string>>.Run(async () =>
            {
                List<string> errorlist = new List<string>();
                STC.Frame.Utils.ExcelHelper.ExcelHelper helper = null;
                try
                {
                    // 1 检查文件是否被占用
                    bool isFileLock = STC.Frame.Utils.ExcelHelper.ExcelHelper.CheckFileIsLock(filePath);
                    if (isFileLock)
                    {
                        errorlist.Add($"{filePath} 被占用，请关闭后再试。");
                        return errorlist;
                    }

                    helper = new STC.Frame.Utils.ExcelHelper.ExcelHelper(filePath);

                    Dictionary<int, string> dic = helper.ReturnSheetList();

                    if (dic.Count > 20)
                    {
                        errorlist.Add("分组总数应不超过20个");
                        return errorlist;
                    }

                    // 3.读文件
                    if (dic.Count > 0)
                    {
                        //1.记录需删除组
                        List<string> removeGroupList = new List<string>();
                        IOrderedEnumerable<KeyValuePair<string, HomeGroupInfo>> groupList = HomePageDataHandler.RequestGroups(false);
                        foreach (KeyValuePair<string, HomeGroupInfo> existGroupPair in groupList)
                        {
                            removeGroupList.Add(existGroupPair.Key);
                        }

                        int ordersn = 0;
                        Dictionary<string, List<string>> importDic = new Dictionary<string, List<string>>();
                        foreach (KeyValuePair<int, string> groupPair in dic)
                        {
                            // 2.添加新组
                            //HomePageDataHandler.AddGroup(groupPair.Value);
                            HomeGroupUpdateReq request = new HomeGroupUpdateReq();
                            request.groupname = groupPair.Value;//组名
                            request.action = "1";//新增
                            request.ordersn = ordersn.ToString();//顺序
                            request.terminal = LocalEnv.ServerInfo.Version; // 终端
                            request.username = LocalEnv.UserInfo.UserName;
                            request.closeby = LocalEnv.UserInfo.UserName;
                            GroupMsgPo groupMsgPo = new GroupMsgPo();
                            groupMsgPo.Tag = "主页";
                            groupMsgPo.IsCareGroup = "true";
                            groupMsgPo.IsAllInstGroup = "false";
                            groupMsgPo.IsDynamicsGroup = "false";
                            request.inf2 = JsonConvert.SerializeObject(groupMsgPo);
                            request.type = CommonConstants.HOME_Type;
                            HomeGroupUpdateReplyPo pResult = AdminHandler.Instance.homeGroupUpdate(request);
                            if (pResult != null && pResult.Code == "0000" && pResult.Datas != null && !string.IsNullOrEmpty(pResult.Datas.id))
                            {
                                

                                if (!importDic.ContainsKey(pResult.Datas.id))
                                {
                                    importDic[pResult.Datas.id] = new List<string>();
                                }
                                    // 3.向组里导入债券
                                DataTable importDT = helper.ExcelToDataTable(groupPair.Key);
                                foreach(DataRow dr in importDT.Rows)
                                {
                                    HomeGroupSecurityidsUpdateReq addBondRequest = new HomeGroupSecurityidsUpdateReq();
                                    addBondRequest.securityid = CommonUtils.ConvertToString(dr[SECURITY_ID_ExcelField]);
                                    addBondRequest.symbol = CommonUtils.ConvertToString(dr[SYMBOL_ExcelField]);
                                    addBondRequest.action = "1"; // 添加
                                    addBondRequest.remark = $"{addBondRequest.securityid}  {addBondRequest.symbol}";
                                    addBondRequest.terminal = string.Format("STClient_v{0}", LocalEnv.ServerInfo.Version);
                                    addBondRequest.groupid = pResult.Datas.id;
                                    addBondRequest.type = CommonConstants.HOME_Type;
                                    PResult result = HomePageDataHandler.UpdateSecurityids(addBondRequest);
                                    if (result != null && result.IsSuccess())
                                    {
                                        importDic[pResult.Datas.id].Add(addBondRequest.securityid);
                                    }
                                }
                            }
                            ordersn++;
                        }

                        // 4.删除原有组
                        List<string> updateRemoveGroupList = new List<string>();
                        foreach (string removeGroupID in removeGroupList)
                        {
                            HomeGroupUpdateReq request = new HomeGroupUpdateReq();
                            request.id = removeGroupID;//组id
                            request.action = "3";//删除
                            request.username = LocalEnv.UserInfo.UserName;
                            PResult pResult = HomePageDataHandler.UpdateGroups(request);
                            if (pResult != null && pResult.IsSuccess())
                            {
                                updateRemoveGroupList.Add(removeGroupID);
                            }
                        }

                        // 5.更新界面
                        HomePageDataHandler.RequestGroups(true);

                        foreach (KeyValuePair<string, List<string>> pair in importDic)
                        {
                            HomePageDataHandler.Notify(HomePageDataHandler.FavoriteTypeEnum.AddGroup, pair.Key, string.Empty);

                            HomePageDataHandler.Notify(HomePageDataHandler.FavoriteTypeEnum.BatchImport, pair.Key, string.Empty);

                            foreach (string securityID in pair.Value)
                            {
                                HomePageDataHandler.Notify(HomePageDataHandler.FavoriteTypeEnum.AddBond, pair.Key, securityID);
                            }
                        }
                        foreach (string removeGroupID in updateRemoveGroupList)
                        {
                            HomePageDataHandler.Notify(HomePageDataHandler.FavoriteTypeEnum.DelGroup, removeGroupID, string.Empty);
                        }
                    }
                    else
                    {
                        errorlist.Add("没有可导入组");
                    }
                }
                catch (Exception e)
                {
                    errorlist.Add(e.Message);
                    LogHelper.Error(e);
                }
                finally
                {
                    if (helper != null)
                        helper.Dispose();
                }
                return errorlist;
            });
        }


        private async void ExportCSV()
        {
            try
            {
                //创建一个保存文件式的对话框
                System.Windows.Forms.SaveFileDialog sfd = new System.Windows.Forms.SaveFileDialog();                              //设置这个对话框的起始保存路径
                sfd.InitialDirectory = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                sfd.RestoreDirectory = true;
                sfd.FileName = $"关注组导出_{DateTime.Now.ToString("yyyy_MM_dd")}";
                //设置保存的文件的类型，注意过滤器的语法
                sfd.Filter = "Excel Documents|*.xls;*.xlsx;";
                sfd.DefaultExt = ".xlsx";
                //调用ShowDialog()方法显示该对话框，该方法的返回值代表用户是否点击了确定按钮
                string promptTxt = string.Empty;
                if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    if (!sfd.FileName.EndsWith(".xls", true, System.Globalization.CultureInfo.CurrentCulture)
                        && !sfd.FileName.EndsWith(".xlsx", true, System.Globalization.CultureInfo.CurrentCulture))
                    {
                        CommonUtils.Prompt2Second("批量导出", "当前只支持xls文件和xlsx文件");
                        return;
                    }

                    this.MaskVisibility = Visibility.Visible;

                    List<string> errorList = await Sub_ExportExcel(sfd.FileName);

                    if (errorList != null && errorList.Count > 0)
                    {
                        StringBuilder errors = new StringBuilder();
                        foreach (string error in errorList)
                        {
                            if (errors.Length > 0)
                            {
                                errors.AppendLine();
                            }
                            errors.Append(error);
                        }
                        promptTxt = errors.ToString();
                    }
                    else
                    {
                        promptTxt = "导出成功";
                    }
                }
                else
                {
                    promptTxt = "取消导出";
                }
                this.MaskVisibility = Visibility.Collapsed;

                if (string.IsNullOrEmpty(promptTxt) == false)
                {
                    CommonUtils.Prompt2Second("批量导出", promptTxt);
                    //CustomMessageBox.Info(promptTxt, "批量导入");
                }
            }
            catch (Exception e)
            {
                this.MaskVisibility = Visibility.Collapsed;
                CommonUtils.Prompt2Second("批量导出", e.Message);
                //CustomMessageBox.Info(promptTxt, "批量导入");

                LogHelper.Error(e);
            }
        }

        private Task<List<string>> Sub_ExportExcel(string filePath)
        {
            return Task.Run(() =>
            {
                List<string> errorlist = new List<string>();
                STC.Frame.Utils.ExcelHelper.ExcelHelper helper = null;
                try
                {
                    // 1 检查文件是否被占用
                    if (File.Exists(filePath))
                    {
                        bool isFileLock = STC.Frame.Utils.ExcelHelper.ExcelHelper.CheckFileIsLock(filePath);
                        if (isFileLock)
                        {
                            errorlist.Add($"{filePath} 被占用，请关闭后再试。");
                            return errorlist;
                        }

                        File.Delete(filePath);
                    }


                    helper = new STC.Frame.Utils.ExcelHelper.ExcelHelper(filePath);

                    IOrderedEnumerable<KeyValuePair<string, HomeGroupInfo>> groupList = HomePageDataHandler.RequestGroups(false);

                    if (groupList != null)
                    {
                        Dictionary<string, DataTable> dataDic = new Dictionary<string, DataTable>();
                        foreach (KeyValuePair<string, HomeGroupInfo> pair in groupList)
                        {
                            DataTable dt = new DataTable();
                            dt.Columns.Add(SECURITY_ID_ExcelField);
                            dt.Columns.Add(SYMBOL_ExcelField);
                            dt.Columns.Add(ExcelExportUtils.Excel_IsHomeField);

                            foreach (HomeGroupSecurityIDsInfo info in pair.Value.securityids)
                            {
                                DataRow newDR = dt.NewRow();
                                newDR[SECURITY_ID_ExcelField] = info.securityid;
                                newDR[SYMBOL_ExcelField] = info.symbol;
                                newDR[ExcelExportUtils.Excel_IsHomeField] = "是";
                                 dt.Rows.InsertAt(newDR, 0);
                            }

                            dataDic[pair.Value.groupname] = dt;
                        }
                        Dictionary<string, int> result = helper.DataTableToExcel(dataDic, true);
                        if (result == null) errorlist.Add("无数据导出");
                    }
                    else
                    {
                        errorlist.Add("无数据导出");
                    }
                }
                catch (Exception e)
                {
                    errorlist.Add(e.Message);
                    LogHelper.Error(e);
                }
                finally
                {
                    if (helper != null)
                        helper.Dispose();
                }
                return errorlist;
            });
        }
        
        public const string SECURITY_ID_ExcelField = "债券代码";
        public const string SYMBOL_ExcelField = "债券简称";
        #endregion
    }
}
