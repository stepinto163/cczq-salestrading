using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using STC.Common.Handler.Base;
using STC.Common.Model.Admin;
using STC.Common.Model.Base;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.TBook;
using System.Data;
using System.Windows.Input;
using STC.Mmshe.Plugin.Utils;
using System.Collections;
using System.Windows.Controls;
using System.Windows;
using STC.Mmshe.Plugin.View.HomePage;
using STC.Common.Model.MMshe;
using STC.Mmshe.Plugin.View.Component;
using STC.Common.Handler.Admin;
using STC.Common.Utils;
using System.Collections.Concurrent;
using System.Threading;
using STC.Mmshe.Plugin.View;
using System.Windows.Data;
using Ats.Foundation.Message;
using Newtonsoft.Json;
using com.gateway.log;
using System.Windows.Documents;
using System.Windows.Media;
using STC.Common.Handler.Ans;
using STC.Common.Model.Ans;
using STC.Mmshe.Plugin.Utils.Data;
using STC.Mmshe.Plugin.Command;
using STC.Mmshe.Plugin.ViewModel;
using STC.Common.ViewModel.Data;
using STC.Common.DataProcesser;
using STC.Common.View.UI;
using STC.Frame.CustomUI;
using STC.Common.View.Component;
using STC.Common.ViewModel;
using STC.Frame.Common;
using STC.Common.Model;

namespace STC.Mmshe.Plugin.ViewModel
{
    /// <summary>
    /// 交易所报价MMListBoxView数据内容
    /// </summary>
    public class MmsheListBoxViewModel : ViewModelBase
    {
        public ConcurrentDictionary<string, string> mainWinList = new ConcurrentDictionary<string, string>();

        public string groupID = string.Empty;
        public MMsheGroupInfo SelectedGroup = null;
        public ICommand AddBondCommand { get; set; }

        public ICommand UpdateAllEnableCommand { get; set; }

        public ICommand OpenCommand { get; set; }

        public ICommand CloseCommand { get; set; }

        public ICommand ToMoveCommand { get; set; }

        public ICommand ToManualCommand { get; set; }

        public ICommand ToAutoCommand { get; set; }

        public ICommand ToDelCommand { get; set; }

        public ICommand ConfigCommand { get; set; }

        public ICommand DetailCommand { get; set; }

        public ICommand BatchUpdateCommand { get; set; }


        internal object lockObj = new object();

        StringBuilder filterBuffer = new StringBuilder();

        #region 表格字段
        public const string SECURITY_ID_FIELD = "securityID";
        public const string PARAMS_FIELD = "Params";
        public const string SYMBOL_FIELD = "symbol";
        public const string SETTLE_TYPE_FIELD = "settletype";
        public const string ENABLE_FIELD = "enable";
        public const string REMAINGING_TIME_FIELD = "RemainingTime";
        public const string REMAINGING_TIME_CONVERT_FIELD = "RemaingingTime_convert";
        public const string BOND_TYPE_FIELD = "BondType";
        public const string SECURITY_TERM_FIELD = "SecurityTerm";
        public const string LAST_OPERATION_FIELD = "LastOperation";
        public const string LAST_OPERATION_TIME_FIELD = "LastOperationTime";
        public const string UPDATE_TIME_FIELD = "updatetime";
        public const string ANS_PAUSE_STATUS_FIELD = "pauseStatus";
        public const string ANS_MM_STATUS_FIELD = "MMStatus";
        public const string ANS_VALID_QUOTA_DURATION_FIELD = "ValidQuotaDuration";
        public const string ANS_UPDATETIME_FIELD = "AnsUpdatetime";
        public const string ANS_NEW_BOND_SIGN_FIELD = "newBondSign";
        public const string ANS_SORT_STATUS_FIELD = "sortForStatus";
        public const string MY_BID_HIGHLIGHT_FIELD = "MyBid_Highlight";
        public const string MY_BID_FIELD = "MyBid";
        public const string MY_PRICE_SPREAD = "MyPriceSpread";
        public const string MY_BID_MOTHOD_FIELD = "MyBidMethod";
        public const string MY_BID_QTY_FIELD = "MyBidQty";
        public const string BEST_CFETS_BID_SAME_FIELD = "BestCFETS_Bid_Same";
        public const string BEST_CFETS_BID_PARTYNAME = "BestCFETS_Bid_PartyName";
        public const string BEST_CFETS_BID_QTY_FIELD = "BestCFETS_BidQTY";
        public const string BEST_CFETS_BID_FIELD = "BestCFETS_Bid";
        public const string BEST_CFETS_BIDNETPX_FIELD = "BestCFETS_BidNetPx";
        public const string BEST_CFETS_OFR_FIELD = "BestCFETS_Ofr";
        public const string BEST_CFETS_OFRNETPX_FIELD = "BestCFETS_OfrNetPx";
        public const string BEST_CFETS_OFR_QTY_FIELD = "BestCFETS_OfrQTY";
        public const string BEST_CFETS_OFR_PARTYNAME = "BestCFETS_Ofr_PartyName";
        public const string BEST_CFETS_ISRESVERSE = "BestCFETS_ISResverse";
        public const string BEST_CFETS_OFR_SAME_FIELD = "BestCFETS_Ofr_Same";

        public const string BEST_BID_SAME_FIELD = "Best_Bid_Same";
        public const string BEST_BID_QTY_FIELD = "Best_BidQTY";
        public const string BEST_BID_FIELD = "Best_Bid";
        public const string BEST_OFR_FIELD = "Best_Ofr";
        public const string BEST_BIDNETPX_FIELD = "Best_BidNetPx";
        public const string BEST_OFRNETPX_FIELD = "Best_OfrNetPx";
        public const string BEST_OFR_QTY_FIELD = "Best_OfrQTY";
        public const string BEST_ISRESVERSE = "Best_ISResverse";
        public const string BEST_OFR_SAME_FIELD = "Best_Ofr_Same";

        public const string MY_OFR_HIGHLIGHT_FIELD = "MyOfr_Highlight";
        public const string MY_OFR_FIELD = "MyOfr";
        public const string MY_OFR_MOTHOD_FIELD = "MyOfrMethod";
        public const string MY_OFR_QTY_FIELD = "MyOfrQty";
        public const string IS_NEW_BOND = "IsNewBond";
        public const string ISSUER_RATING_CURRENT_NPY_FIELD = "IssuerRatingCurrentNPY";
        public const string BOND_SUB_TYPE_FIELD = "BondSubType";
        public const string GOODS_TYPE_FIELD = "GoodsType";
        public const string ENT_COR_FIELD = "Ent_Cor";
        public const string BOND_CATEGORY_FIELD = "bond_category";
        public const string VALUATION_FIELD = "Valuation";
        public const string VALUATION_TYPE_FIELD = "ValuationType";//中债估值类型1-行权；2-到期（或者空）
        public const string VALUATION_EXE_FIELD = "ExerciseValuation";//行权
        public const string VALUATION_MAT_FIELD = "MaturityValuation";//到期
        public const string ISSUER_FIELD = "Issuer";
        public const string PUBLISHER_FIELD = "Publisher";
        public const string ISCHECKED_FIELD = "IsChecked";
        public const string INF1_FIELD = "inf1";
        public const string INF2_FIELD = "inf2";
        public const string INF5_FIELD = "inf5";
        public const string SECURITY_TYPE_ID_FIELD = "SecurityTypeId";
        public const string SECURITY_DESC_FIELD = "SecurityDesc";
        public const string VENUE_SORT_FIELD = "VenueSort";//个人设置自定义Venue顺序
        public const string ISINNER_FIELD = "IsInner";//是否是内部
        public const string BID_QUOTE_ID_FIELD = "BidQuoteID";
        public const string OFR_QUOTE_ID_FIELD = "OfrQuoteID";
        public const string MAXFLOOR_FIELD = "MaxFloor";
        public const string IS_MY_BID_DIFFPX_FIELD = "IsMyBidDiffPx";
        public const string IS_MY_OFR_DIFFPX_FIELD = "IsMyOfrDiffPx";
        public const string CFETS_BID_ENTYRID_FIELD = "CfetsBidEntryID";
        public const string CFETS_OFR_ENTYRID_FIELD = "CfetsOfrEntryID";
        public const string BID_ENTYRID_FIELD = "BidEntryID";
        public const string OFR_ENTYRID_FIELD = "OfrEntryID";
        public const string MY_BID_LEVEL_VISIBLE = "MyBidLevelVisible";
        public const string MY_OFR_LEVEL_VISIBLE = "MyOfrLevelVisible";
        public const string MY_BID_LEVEL = "MyBidLevel";
        public const string MY_OFR_LEVEL = "MyOfrLevel";
        public const string VENUES_FIELD = "Venues";
        public const string VENUETYPEGW_FIELD = "VenueTypeGW";
        public const string USERNAME_FIELD = "UserName";
        public const string BidYieldPrice_FIELD = "BidYieldPrice";
        public const string OfrYieldPrice_FIELD = "OfrYieldPrice";
        public const string BidYieldToExecutionPrice_FIELD = "BidYieldToExecutionPrice";
        public const string OfrYieldToExecutionPrice_FIELD = "OfrYieldToExecutionPrice";
        #endregion

        /// <summary>
        /// 我的订单所有选中个数
        /// </summary>
        private int _checkedCount = 0;
        public int CheckedCount
        {
            get
            {
                return _checkedCount;
            }
            set
            {
                if (_checkedCount != value)
                {
                    _checkedCount = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 0: 显示 1：不显示
        /// </summary>
        private Visibility isHiddenOperationGrid = Visibility.Hidden;

        public Visibility IsHiddenOperationGrid
        {
            get
            {
                return isHiddenOperationGrid;
            }

            set
            {
                if (isHiddenOperationGrid != value)
                {
                    isHiddenOperationGrid = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 批量操作按钮是否可用
        /// </summary>
        private bool _BatchOperateBtnEnanble = true;

        public bool BatchOperateBtnEnable
        {
            get
            {
                return _BatchOperateBtnEnanble;
            }
            set
            {
                if (_BatchOperateBtnEnanble != value)
                {
                    _BatchOperateBtnEnanble = value;
                    this.OnPropertyChanged();
                }
            }
        }

        //存储最新的CT行情， key: secutirtyID; Value: MarketDataSnapshotFullRefresh
        private Dictionary<string, MarketDataSnapshotFullRefresh> _LastCTPriceDic = new Dictionary<string, MarketDataSnapshotFullRefresh>();
        //存储CT初级处理数据（把MarketDataSnapshotFullRefresh转换成DataView数据结构）
        private ConcurrentDictionary<string, DataView> baseCTPriceDataViewDic = new ConcurrentDictionary<string, DataView>();
        private ConcurrentDictionary<string, MarketDataSnapshotFullRefresh> baseCTPriceDataDic = new ConcurrentDictionary<string, MarketDataSnapshotFullRefresh>();

        //更新行情数据
        private Thread _CalcMDDataThread = null;
        private bool _CalcMDDataThreadDispose = false;

        //监控勾选状态
        private Thread _RefreshCheckBoxThread = null;
        private bool _RefreshCheckBoxThreadDispose = false;

        //存储最新order
        private ConcurrentDictionary<string, ExecutionReport> _LastOrderDic = new ConcurrentDictionary<string, ExecutionReport>();

        private ConcurrentDictionary<string, STC.Common.Model.MMshe.MMshe> _MMDic = new ConcurrentDictionary<string, STC.Common.Model.MMshe.MMshe>();
        private Thread _RefreshBondsThread = null;
        private bool _RefreshBondsThreadDispose = false;
        //private AsyncThreadQueue<MM> _ProcessBondsThreadQueue;
        //private AsyncThreadQueue<string> _ProcessMyOrdersThreadQueue;
        private AsyncThreadQueue<ANSMonitorInfo> _ProcessAnsMonitorInfThreadQueue;

        private ConcurrentDictionary<string, int> bondWithOutInfoList = new ConcurrentDictionary<string, int>();
        //private List<string> _notExistBaseInfoList = new List<string>();
        private Thread _RefreshBaseDataThread = null;
        private bool _isRefreshBaseDataThreadDispose = false;

        // 是否正在收集做市义务债券信息
        private bool _isCollectingAnsMonitor = false;
        private List<string> _needAnsMonitorList = new List<string>();

        private DataView _BondView;

        public DataView BondView
        {
            get
            {
                lock (this.lockObj)
                {
                    if (_BondView == null)
                    {
                        _BondView = new DataTable().DefaultView;
                        _BondView.Table.Columns.Add(SECURITY_ID_FIELD, typeof(string));
                        _BondView.Table.Columns.Add(PARAMS_FIELD, typeof(OrderParamProp));
                        _BondView.Table.Columns.Add(SYMBOL_FIELD, typeof(string));
                        _BondView.Table.Columns.Add(SETTLE_TYPE_FIELD, typeof(string));
                        _BondView.Table.Columns.Add(ENABLE_FIELD, typeof(bool));
                        _BondView.Table.Columns[ENABLE_FIELD].DefaultValue = false;
                        _BondView.Table.Columns.Add(REMAINGING_TIME_FIELD, typeof(string));
                        _BondView.Table.Columns.Add(REMAINGING_TIME_CONVERT_FIELD, typeof(int));
                        _BondView.Table.Columns.Add(BOND_TYPE_FIELD, typeof(string));
                        _BondView.Table.Columns.Add(SECURITY_TERM_FIELD, typeof(string));
                        _BondView.Table.Columns.Add(LAST_OPERATION_FIELD, typeof(string));
                        _BondView.Table.Columns.Add(LAST_OPERATION_TIME_FIELD, typeof(string));
                        _BondView.Table.Columns.Add(UPDATE_TIME_FIELD, typeof(string));
                        _BondView.Table.Columns.Add(ANS_PAUSE_STATUS_FIELD, typeof(string));
                        _BondView.Table.Columns.Add(ANS_MM_STATUS_FIELD, typeof(string));
                        _BondView.Table.Columns.Add(ANS_VALID_QUOTA_DURATION_FIELD, typeof(long));
                        _BondView.Table.Columns.Add(ANS_UPDATETIME_FIELD, typeof(string));
                        _BondView.Table.Columns.Add(ANS_NEW_BOND_SIGN_FIELD, typeof(string));
                        _BondView.Table.Columns.Add(ANS_SORT_STATUS_FIELD, typeof(string));
                        _BondView.Table.Columns.Add(MY_BID_HIGHLIGHT_FIELD, typeof(string));
                        _BondView.Table.Columns.Add(MY_BID_FIELD, typeof(decimal));
                        _BondView.Table.Columns.Add(MY_BID_QTY_FIELD, typeof(long));
                        _BondView.Table.Columns.Add(MY_PRICE_SPREAD, typeof(decimal));
                        _BondView.Table.Columns.Add(MY_BID_MOTHOD_FIELD, typeof(string));
                        _BondView.Table.Columns.Add(BEST_CFETS_BID_SAME_FIELD, typeof(string));
                        _BondView.Table.Columns.Add(BEST_CFETS_BID_PARTYNAME, typeof(string));
                        _BondView.Table.Columns.Add(BEST_CFETS_BID_QTY_FIELD, typeof(long));
                        _BondView.Table.Columns.Add(BEST_CFETS_BID_FIELD, typeof(decimal));
                        _BondView.Table.Columns.Add(BEST_CFETS_OFR_FIELD, typeof(decimal));
                        _BondView.Table.Columns.Add(BEST_CFETS_BIDNETPX_FIELD, typeof(decimal));
                        _BondView.Table.Columns.Add(BEST_CFETS_OFRNETPX_FIELD, typeof(decimal));
                        _BondView.Table.Columns.Add(BEST_CFETS_OFR_QTY_FIELD, typeof(long));
                        _BondView.Table.Columns.Add(BEST_CFETS_OFR_PARTYNAME, typeof(string));
                        _BondView.Table.Columns.Add(BEST_CFETS_OFR_SAME_FIELD, typeof(string));
                        _BondView.Table.Columns.Add(BEST_CFETS_ISRESVERSE, typeof(string));
                        _BondView.Table.Columns.Add(BEST_BID_SAME_FIELD, typeof(string));
                        _BondView.Table.Columns.Add(BEST_BID_QTY_FIELD, typeof(long));
                        _BondView.Table.Columns.Add(BEST_BID_FIELD, typeof(decimal));
                        _BondView.Table.Columns.Add(BEST_OFR_FIELD, typeof(decimal));
                        _BondView.Table.Columns.Add(BEST_BIDNETPX_FIELD, typeof(decimal));
                        _BondView.Table.Columns.Add(BEST_OFRNETPX_FIELD, typeof(decimal));
                        _BondView.Table.Columns.Add(BEST_OFR_QTY_FIELD, typeof(long));
                        _BondView.Table.Columns.Add(BEST_OFR_SAME_FIELD, typeof(string));
                        _BondView.Table.Columns.Add(BEST_ISRESVERSE, typeof(string));
                        _BondView.Table.Columns.Add(MY_OFR_HIGHLIGHT_FIELD, typeof(string));
                        _BondView.Table.Columns.Add(MY_OFR_FIELD, typeof(decimal));
                        _BondView.Table.Columns.Add(MY_OFR_MOTHOD_FIELD, typeof(string));
                        _BondView.Table.Columns.Add(MY_OFR_QTY_FIELD, typeof(long));
                        _BondView.Table.Columns.Add(IS_NEW_BOND, typeof(bool));
                        _BondView.Table.Columns[IS_NEW_BOND].DefaultValue = false;
                        _BondView.Table.Columns.Add(ISSUER_RATING_CURRENT_NPY_FIELD, typeof(string));
                        _BondView.Table.Columns.Add(BOND_SUB_TYPE_FIELD, typeof(string));
                        _BondView.Table.Columns.Add(GOODS_TYPE_FIELD, typeof(string));
                        _BondView.Table.Columns.Add(ENT_COR_FIELD, typeof(string));
                        _BondView.Table.Columns.Add(BOND_CATEGORY_FIELD, typeof(string));
                        _BondView.Table.Columns.Add(VALUATION_FIELD, typeof(string));
                        _BondView.Table.Columns.Add(VALUATION_TYPE_FIELD, typeof(string));
                        _BondView.Table.Columns.Add(VALUATION_EXE_FIELD, typeof(string));
                        _BondView.Table.Columns.Add(VALUATION_MAT_FIELD, typeof(string));
                        _BondView.Table.Columns.Add(ISSUER_FIELD, typeof(string));
                        _BondView.Table.Columns.Add(PUBLISHER_FIELD, typeof(string));
                        _BondView.Table.Columns.Add(ISCHECKED_FIELD, typeof(bool));
                        _BondView.Table.Columns[ISCHECKED_FIELD].DefaultValue = false;
                        _BondView.Table.Columns.Add(INF1_FIELD, typeof(string));
                        _BondView.Table.Columns.Add(INF2_FIELD, typeof(string));
                        _BondView.Table.Columns.Add(INF5_FIELD, typeof(string));
                        _BondView.Table.Columns.Add(SECURITY_TYPE_ID_FIELD, typeof(string));
                        _BondView.Table.Columns.Add(SECURITY_DESC_FIELD, typeof(string));
                        _BondView.Table.Columns.Add(VENUE_SORT_FIELD, typeof(string));
                        _BondView.Table.Columns.Add(ISINNER_FIELD, typeof(string));
                        _BondView.Table.Columns.Add(BID_QUOTE_ID_FIELD, typeof(string));
                        _BondView.Table.Columns.Add(OFR_QUOTE_ID_FIELD, typeof(string));
                        _BondView.Table.Columns.Add(MAXFLOOR_FIELD, typeof(string));
                        _BondView.Table.Columns.Add(IS_MY_BID_DIFFPX_FIELD, typeof(string));
                        _BondView.Table.Columns.Add(IS_MY_OFR_DIFFPX_FIELD, typeof(string));
                        _BondView.Table.Columns.Add(CFETS_BID_ENTYRID_FIELD, typeof(string));
                        _BondView.Table.Columns.Add(CFETS_OFR_ENTYRID_FIELD, typeof(string));
                        _BondView.Table.Columns.Add(BID_ENTYRID_FIELD, typeof(string));
                        _BondView.Table.Columns.Add(OFR_ENTYRID_FIELD, typeof(string));
                        _BondView.Table.Columns.Add(MY_BID_LEVEL_VISIBLE, typeof(string));
                        _BondView.Table.Columns.Add(MY_OFR_LEVEL_VISIBLE, typeof(string));
                        _BondView.Table.Columns.Add(MY_BID_LEVEL, typeof(string));
                        _BondView.Table.Columns.Add(MY_OFR_LEVEL, typeof(string));
                        _BondView.Table.Columns.Add(VENUES_FIELD, typeof(string));
                        _BondView.Table.Columns.Add(VENUETYPEGW_FIELD, typeof(string));
                        _BondView.Table.Columns.Add(USERNAME_FIELD, typeof(string));
                        _BondView.Table.Columns.Add(BidYieldPrice_FIELD, typeof(decimal));
                        _BondView.Table.Columns.Add(OfrYieldPrice_FIELD, typeof(decimal));
                        _BondView.Table.Columns.Add(BidYieldToExecutionPrice_FIELD, typeof(decimal));
                        _BondView.Table.Columns.Add(OfrYieldToExecutionPrice_FIELD, typeof(decimal));
                        _BondView.Table.PrimaryKey = new DataColumn[] {
                            _BondView.Table.Columns[SECURITY_ID_FIELD]
                        };

                        this.OnPropertyChanged();
                    }

                    return this._BondView;
                }
            }
        }

        //private string _originalSelectedItemSecurityID;

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
                this.OnPropertyChanged();
            }
        }

        private int _selectedIndex;
        public int SelectedIndex
        {
            get
            {
                return _selectedIndex;
            }

            set
            {
                if (_selectedIndex > -1 && this.BondView.Count > 0 && value == -1)
                {
                    if (_selectedIndex >= this.BondView.Count)
                    {
                        _selectedIndex = 0;
                    }
                }
                else
                {
                    _selectedIndex = value;
                }
                this.OnPropertyChanged();
            }
        }
        private WindowManager windowManager;

        #region 我的订单全选逻辑
        // 是否选中全选多选框
        private bool _IsAllChecked = false;
        public bool IsAllChecked
        {
            get
            {
                return _IsAllChecked;
            }

            set
            {
                if (_IsAllChecked != value)
                {
                    _IsAllChecked = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 是否刷新checkbox选中状态
        /// </summary>
        private volatile bool isAdjustAllChkStatus = true;

        /// <summary>
        /// 是否点击全选checkbox
        /// </summary>
        private volatile bool _isCheckBoxAllClick = false;

        public int checkCount = 0;
        public void CheckBoxAllValueChange(bool isAllChecked)
        {
            _isCheckBoxAllClick = true;
            int checkedCount = 0;

            int manualCount_bid = 0;
            int defendCount_bid = 0;
            int algorithmCount_bid = 0;
            int apiCount_bid = 0;
            int manualCount_ofr = 0;
            int defendCount_ofr = 0;
            int algorithmCount_ofr = 0;
            int apiCount_ofr = 0;

            int settlTypeT0Count = 0;
            int settlTypeT1Count = 0;

            lock (this.lockObj)
            {
                foreach (DataRowView item in this.BondView)
                {
                    item[ISCHECKED_FIELD] = isAllChecked;
                    if (isAllChecked)
                    {
                        checkedCount++;
                        string settlType = item[SETTLE_TYPE_FIELD].ToString();
                        if ("1" == settlType)
                        {
                            settlTypeT0Count++;
                        }
                        else if ("2" == settlType)
                        {
                            settlTypeT1Count++;
                        }

                        string bidModel = item[MY_BID_MOTHOD_FIELD].ToString();
                        string ofrModel = item[MY_OFR_MOTHOD_FIELD].ToString();

                        if (bidModel == "Manual")
                        {
                            manualCount_bid++;
                        }
                        else if (bidModel == "Defend")
                        {
                            defendCount_bid++;
                        }
                        else if (bidModel == "Algorithm")
                        {
                            algorithmCount_bid++;
                        }
                        else if (bidModel == "API")
                        {
                            apiCount_bid++;
                        }

                        if (ofrModel == "Manual")
                        {
                            manualCount_ofr++;
                        }
                        else if (ofrModel == "Defend")
                        {
                            defendCount_ofr++;
                        }
                        else if (ofrModel == "Algorithm")
                        {
                            algorithmCount_ofr++;
                        }
                        else if (ofrModel == "API")
                        {
                            apiCount_ofr++;
                        }
                    }
                }
                checkCount = checkedCount;
            }

            if (checkedCount > 0)
            {
                if (checkedCount == settlTypeT0Count)
                {
                    SettlTypeIndex = 0;
                }
                else if (checkedCount == settlTypeT1Count)
                {
                    SettlTypeIndex = 1;
                }
                else
                {
                    SettlTypeIndex = -1;
                }


                if (checkedCount == manualCount_bid)
                {
                    BidQuoteModel = "Manual";
                }
                else if (checkedCount == defendCount_bid)
                {
                    BidQuoteModel = "Defend";
                }
                else if (checkedCount == algorithmCount_bid)
                {
                    BidQuoteModel = "Algorithm";
                }
                else if (checkedCount == apiCount_bid)
                {
                    BidQuoteModel = "API";
                }
                else
                {
                    BidQuoteModel = "--";
                }

                if (checkedCount == manualCount_ofr)
                {
                    OfrQuoteModel = "Manual";
                }
                else if (checkedCount == defendCount_ofr)
                {
                    OfrQuoteModel = "Defend";
                }
                else if (checkedCount == algorithmCount_ofr)
                {
                    OfrQuoteModel = "Algorithm";
                }
                else if (checkedCount == apiCount_ofr)
                {
                    OfrQuoteModel = "API";
                }
                else
                {
                    OfrQuoteModel = "--";
                }
            }

            CheckedCount = checkedCount;
            if (checkedCount > 0)
            {
                IsHiddenOperationGrid = Visibility.Visible;
            }
            else
            {
                IsHiddenOperationGrid = Visibility.Hidden;
            }
            _isCheckBoxAllClick = false;
        }


        private void RefreshCheckBox()
        {
            try
            {
                while (!_RefreshCheckBoxThreadDispose)
                {
                    if (isAdjustAllChkStatus && mainWinList.Count > 0)
                    {
                        isAdjustAllChkStatus = false;
                        Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                        {
                            this.AdjustAllChkStatus();
                        }));
                    }
                    Thread.Sleep(500);
                }
            }
            catch (ThreadAbortException)
            {
                // ignore.
            }
            catch (Exception e)
            {
                LogHelper.Error(e);
            }
        }

        public void AdjustAllChkStatus()
        {
            int checkableCount = 0;
            int checkedCount = 0;

            int manualCount_bid = 0;
            int defendCount_bid = 0;
            int algorithmCount_bid = 0;
            int apiCount_bid = 0;
            int manualCount_ofr = 0;
            int defendCount_ofr = 0;
            int algorithmCount_ofr = 0;
            int apiCount_ofr = 0;

            int settlTypeT0Count = 0;
            int settlTypeT1Count = 0;

            lock (this.lockObj)
            {
                foreach (DataRowView item in this.BondView)
                {
                    checkableCount++;
                    if (!(item[ISCHECKED_FIELD] is System.DBNull)
                    && (bool)item[ISCHECKED_FIELD] == true)
                    {
                        checkedCount++;
                        string settlType = item[SETTLE_TYPE_FIELD].ToString();
                        if ("1" == settlType)
                        {
                            settlTypeT0Count++;
                        }
                        else if ("2" == settlType)
                        {
                            settlTypeT1Count++;
                        }
                        string bidModel = item[MY_BID_MOTHOD_FIELD].ToString();
                        string ofrModel = item[MY_OFR_MOTHOD_FIELD].ToString();

                        if (bidModel == "Manual")
                        {
                            manualCount_bid++;
                        }
                        else if (bidModel == "Defend")
                        {
                            defendCount_bid++;
                        }
                        else if (bidModel == "Algorithm")
                        {
                            algorithmCount_bid++;
                        }
                        else if (bidModel == "API")
                        {
                            apiCount_bid++;
                        }

                        if (ofrModel == "Manual")
                        {
                            manualCount_ofr++;
                        }
                        else if (ofrModel == "Defend")
                        {
                            defendCount_ofr++;
                        }
                        else if (ofrModel == "Algorithm")
                        {
                            algorithmCount_ofr++;
                        }
                        else if (ofrModel == "API")
                        {
                            apiCount_ofr++;
                        }
                    }
                }

                checkCount = checkedCount;
            }

            if (checkedCount > 0)
            {
                if (checkedCount == settlTypeT0Count)
                {
                    SettlTypeIndex = 0;
                }
                else if (checkedCount == settlTypeT1Count)
                {
                    SettlTypeIndex = 1;
                }
                else
                {
                    SettlTypeIndex = -1;
                }
                if (checkedCount == manualCount_bid)
                {
                    BidQuoteModel = "Manual";
                }
                else if (checkedCount == defendCount_bid)
                {
                    BidQuoteModel = "Defend";
                }
                else if (checkedCount == algorithmCount_bid)
                {
                    BidQuoteModel = "Algorithm";
                }
                else if (checkedCount == apiCount_bid)
                {
                    BidQuoteModel = "API";
                }
                else
                {
                    BidQuoteModel = "--";
                }

                if (checkedCount == manualCount_ofr)
                {
                    OfrQuoteModel = "Manual";
                }
                else if (checkedCount == defendCount_ofr)
                {
                    OfrQuoteModel = "Defend";
                }
                else if (checkedCount == algorithmCount_ofr)
                {
                    OfrQuoteModel = "Algorithm";
                }
                else if (checkedCount == apiCount_ofr)
                {
                    OfrQuoteModel = "API";
                }
                else
                {
                    OfrQuoteModel = "--";
                }
            }

            IsAllChecked = checkableCount == checkedCount && checkableCount > 0;

            CheckedCount = checkedCount;
            if (checkedCount > 0)
            {
                IsHiddenOperationGrid = Visibility.Visible;
            }
            else
            {
                IsHiddenOperationGrid = Visibility.Hidden;
            }

        }

        private void BondView_ListChanged(object sender, System.ComponentModel.ListChangedEventArgs e)
        {
            if (_isCheckBoxAllClick == true)
            {
                return;
            }

            if (e.PropertyDescriptor == null
                || (e.PropertyDescriptor != null && e.PropertyDescriptor.Name == ISCHECKED_FIELD)
                || (e.PropertyDescriptor != null && e.PropertyDescriptor.Name == MY_BID_MOTHOD_FIELD)
                || (e.PropertyDescriptor != null && e.PropertyDescriptor.Name == MY_OFR_MOTHOD_FIELD))
            {
                isAdjustAllChkStatus = true;
            }
        }

        #endregion

        private bool _Enable_Unify;

        public bool Enable_Unify
        {
            get { return _Enable_Unify; }
            set
            {
                _Enable_Unify = value;
                this.OnPropertyChanged();
            }
        }

        private ConcurrentDictionary<string, string> startBonds = new ConcurrentDictionary<string, string>();


        /// <summary>
        /// 构造函数
        /// </summary>
        public MmsheListBoxViewModel(string grpID)
        {
            if (_CalcMDDataThread == null)
            {
                _CalcMDDataThread = new Thread(CalcMDData);
                _CalcMDDataThread.IsBackground = true;
                _CalcMDDataThread.Start();
            }

            if (_RefreshCheckBoxThread == null)
            {
                _RefreshCheckBoxThread = new Thread(RefreshCheckBox);
                _RefreshCheckBoxThread.IsBackground = true;
                _RefreshCheckBoxThread.Start();
            }

            //// 注册基础数据
            if (_RefreshBaseDataThread == null)
            {
                _RefreshBaseDataThread = new Thread(RefreshBaseData);
                _RefreshBaseDataThread.IsBackground = true;
                _RefreshBaseDataThread.Start();
            }
            //BaseHandler.Instance.SubscribeLoadDataStatus(onloadData);

            // 更新Bond
            if (_RefreshBondsThread == null)
            {
                _RefreshBondsThread = new Thread(RefreshMMData);
                _RefreshBondsThread.IsBackground = true;
                _RefreshBondsThread.Start();
            }

            this.AddBondCommand = new RelayCommand(this.AddBond);
            this.UpdateAllEnableCommand = new RelayCommand<string>(this.UpdateAllEnable);

            this.OpenCommand = new RelayCommand(this.ToOpen);
            this.CloseCommand = new RelayCommand(this.ToClose);
            this.ToManualCommand = new RelayCommand(this.ToManual);
            this.ToAutoCommand = new RelayCommand(this.ToAuto);
            this.ToMoveCommand = new RelayCommand(this.ToMove);
            this.ToDelCommand = new RelayCommand(this.ToDel);
            this.ConfigCommand = new RelayCommand(this.ConfigClick);
            this.DetailCommand = new RelayCommand(this.DetailClick);
            this.BatchUpdateCommand = new RelayCommand(this.BatchUpdateClick);
            windowManager = WindowManager.Instance;
            // Todo: 注册清零信号, 暂时先不清零
            BaseHandler.Instance.Init(Instance_OnClear);

            BindingOperations.EnableCollectionSynchronization(this.BondView, this.lockObj);
            this.BondView.ListChanged += BondView_ListChanged;

            this.Init(grpID);
        }

        private void BatchUpdateClick()
        {
            OrderParamProp orderParam = null;
            List<DataRowView> updateList = new List<DataRowView>();
            lock (this.lockObj)
            {
                foreach (DataRowView item in this.BondView)
                {
                    if (!(item[ISCHECKED_FIELD] is System.DBNull)
                    && (bool)item[ISCHECKED_FIELD] == true)
                    {
                        updateList.Add(item);
                        if (orderParam == null)
                        {
                            orderParam = (OrderParamProp)(item[MmsheListBoxViewModel.PARAMS_FIELD] as OrderParamProp).Clone();
                        }
                        else
                        {
                            OrderParamProp itemParam = item[MmsheListBoxViewModel.PARAMS_FIELD] as OrderParamProp;
                            if (!itemParam.bidQuoteModel.Equals(orderParam.bidQuoteModel))
                            {
                                orderParam.bidQuoteModel = "多";
                            }
                            if (!itemParam.ofrQuoteModel.Equals(orderParam.ofrQuoteModel))
                            {
                                orderParam.ofrQuoteModel = "多";
                            }
                            if (!itemParam.bidBaseReSendValue.Equals(orderParam.bidBaseReSendValue))
                            {
                                orderParam.bidBaseReSendValue = "多";
                            }
                            if (!itemParam.ofrBaseReSendValue.Equals(orderParam.ofrBaseReSendValue))
                            {
                                orderParam.ofrBaseReSendValue = "多";
                            }
                            if (!itemParam.bidBaseQty.Equals(orderParam.bidBaseQty))
                            {
                                orderParam.bidBaseQty = "多";
                            }
                            if (!itemParam.ofrBaseQty.Equals(orderParam.ofrBaseQty))
                            {
                                orderParam.ofrBaseQty = "多";
                            }
                            if (!itemParam.baseSettleType.Equals(orderParam.baseSettleType))
                            {
                                orderParam.baseSettleType = "多";
                            }
                            //if (!itemParam.maxFloorEnable.Equals(orderParam.maxFloorEnable))
                            //{
                            //    orderParam.maxFloorEnable = "多";
                            //}
                            //if (!itemParam.maxFloor.Equals(orderParam.maxFloor))
                            //{
                            //    orderParam.maxFloor = "多";
                            //}
                            if (!itemParam.bidDefenReferMarketType.Equals(orderParam.bidDefenReferMarketType))
                            {
                                orderParam.bidDefenReferMarketType = "多";
                            }
                            if (!itemParam.bidDefenReferLevel.Equals(orderParam.bidDefenReferLevel))
                            {
                                orderParam.bidDefenReferLevel = "多";
                            }
                            if (!itemParam.ofrDefenReferMarketType.Equals(orderParam.ofrDefenReferMarketType))
                            {
                                orderParam.ofrDefenReferMarketType = "多";
                            }
                            if (!itemParam.ofrDefenReferLevel.Equals(orderParam.ofrDefenReferLevel))
                            {
                                orderParam.ofrDefenReferLevel = "多";
                            }
                            if (!itemParam.bidAlgoDefenValue.Equals(orderParam.bidAlgoDefenValue))
                            {
                                orderParam.bidAlgoDefenValue = "多";
                            }
                            if (!itemParam.ofrAlgoDefenValue.Equals(orderParam.ofrAlgoDefenValue))
                            {
                                orderParam.ofrAlgoDefenValue = "多";
                            }
                            //if (!itemParam.wallEnable.Equals(orderParam.wallEnable))
                            //{
                            //    orderParam.wallEnable = "多";
                            //}
                            //if (!itemParam.bidAlgoSafeValue.Equals(orderParam.bidAlgoSafeValue))
                            //{
                            //    orderParam.bidAlgoSafeValue = "多";
                            //}
                            //if (!itemParam.ofrAlgoSafeValue.Equals(orderParam.ofrAlgoSafeValue))
                            //{
                            //    orderParam.ofrAlgoSafeValue = "多";
                            //}
                            //if (!itemParam.bidAlgoTrigValue.Equals(orderParam.bidAlgoTrigValue))
                            //{
                            //    orderParam.bidAlgoTrigValue = "多";
                            //}
                            //if (!itemParam.ofrAlgoTrigValue.Equals(orderParam.ofrAlgoTrigValue))
                            //{
                            //    orderParam.ofrAlgoTrigValue = "多";
                            //}
                            if (!itemParam.defenCDCEnable.Equals(orderParam.defenCDCEnable))
                            {
                                orderParam.defenCDCEnable = "多";
                            }
                            if (!itemParam.bidDefenCDCValue.Equals(orderParam.bidDefenCDCValue))
                            {
                                orderParam.bidDefenCDCValue = "多";
                            }
                            if (!itemParam.ofrDefenCDCValue.Equals(orderParam.ofrDefenCDCValue))
                            {
                                orderParam.ofrDefenCDCValue = "多";
                            }
                            //if (!itemParam.algoMidType.Equals(orderParam.algoMidType))
                            //{
                            //    orderParam.algoMidType = "多";
                            //}
                            //if (!itemParam.algoMidOffsetValue.Equals(orderParam.algoMidOffsetValue))
                            //{
                            //    orderParam.algoMidOffsetValue = "多";
                            //}
                            //if (!itemParam.algoValue.Equals(orderParam.algoValue))
                            //{
                            //    orderParam.algoValue = "多";
                            //}
                            //if (!itemParam.bidBaseWidenValue.Equals(orderParam.bidBaseWidenValue))
                            //{
                            //    orderParam.bidBaseWidenValue = "多";
                            //}
                            //if (!itemParam.ofrBaseWidenValue.Equals(orderParam.ofrBaseWidenValue))
                            //{
                            //    orderParam.ofrBaseWidenValue = "多";
                            //}
                            //if (!itemParam.BaseERReSendType.Equals(orderParam.BaseERReSendType))
                            //{
                            //    orderParam.BaseERReSendType = "多";
                            //}
                            //if (!itemParam.dealWidenLowerTime.Equals(orderParam.dealWidenLowerTime))
                            //{
                            //    orderParam.dealWidenLowerTime = "多";
                            //}
                            //if (!itemParam.dealWidenUpperTime.Equals(orderParam.dealWidenUpperTime))
                            //{
                            //    orderParam.dealWidenUpperTime = "多";
                            //}
                            //if (!itemParam.posFakEnable.Equals(orderParam.posFakEnable))
                            //{
                            //    orderParam.posFakEnable = "多";
                            //}
                            //if (!itemParam.posFakSpread.Equals(orderParam.posFakSpread))
                            //{
                            //    orderParam.posFakSpread = "多";
                            //}
                            if (!itemParam.dealClose.Equals(orderParam.dealClose))
                            {
                                orderParam.dealClose = "多";
                            }
                        }
                    }
                }
            }
            BatchDetailView odView = new BatchDetailView();
            odView.Owner = Application.Current.MainWindow;
            orderParam.Venuetypegw = orderParam.Venues = SelectedGroup.venuetypegw;
            BatchDetailViewModel odVM = new BatchDetailViewModel(orderParam);
            odVM.WinTitle = checkCount + "";
            odVM.NeedPrompt = false;

            odVM.MyWindow = odView;
            odVM.updateList.AddRange(updateList);
            odView.DataContext = odVM;
            if (odView.ShowDialog() == true)
            {
                //odVM.OrderParamReq.ofrQuoteModel = odVM.OrderParamReq.bidQuoteModel;
                //UpdateInf1(odVM.OrderParamReq);
            }
        }

        private void DetailClick()
        {
            if (this.BondView == null || this.BondView.Count == 0)
            {
                return;
            }

            string bond = CommonUtils.ConvertToString(this.SelectedItem[SECURITY_ID_FIELD]);
            bool enable = CommonUtils.ConvertToString(this.SelectedItem[ENABLE_FIELD]).Equals("1") ? true : false;
            OrderParamProp mmParams = this.SelectedItem[PARAMS_FIELD] as OrderParamProp;
            if (mmParams != null)
            {
                MMOrderControlModel model = new MMOrderControlModel()
                {
                    enable = enable,
                    securityID = bond,
                    Params = mmParams
                };
                string securityID = CommonUtils.ConvertToString(model.securityID);
                if (windowManager.IsExist($"{PopupTypeEnum.OrderControl.ToString()}-{securityID}") == false && !string.IsNullOrEmpty(securityID))
                {
                    NoBorderWin popOrderWin = new NoBorderWin(securityID);
                    popOrderWin.Title = securityID;
                    popOrderWin.Closed += PopOrderWin_Closed;
                    MMsheOrderControl orderControl = new MMsheOrderControl();
                    orderControl.securityID = securityID;
                    orderControl.Venue = SelectedGroup.venues;
                    orderControl.popupBtn.Visibility = Visibility.Collapsed;
                    orderControl.btnFixFloatButton.Visibility = Visibility.Visible;
                    orderControl.btnShutDown.Visibility = Visibility.Visible;
                    popOrderWin.SetMainControl(orderControl);
                    popOrderWin.ResizeMode = ResizeMode.CanResize;
                    popOrderWin.Height = 550;
                    popOrderWin.Width = 348;
                    popOrderWin.MinWidth = 348;
                    popOrderWin.MaxWidth = 348;
                    popOrderWin.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;

                    popOrderWin.WindowStyle = WindowStyle.None;
                    popOrderWin.WindowState = WindowState.Normal;
                    popOrderWin.Owner = null;

                    popOrderWin.Show();
                    windowManager.OpenWindow($"{PopupTypeEnum.OrderControl.ToString()}-{securityID}", popOrderWin);
                }
            }
        }

        private void PopOrderWin_Closed(object sender, EventArgs e)
        {
            if (sender != null && sender is PopupWindow)
            {
                PopupWindow popupWindow = sender as PopupWindow;
                if (popupWindow != null)
                {
                    if (popupWindow.Content is MainView)
                    {
                        MainView mainView = popupWindow.Content as MainView;
                        mainView.Dispose();
                    }
                }
            }
            else if (sender != null && sender is NoBorderWin)
            {
                NoBorderWin popupWindow = sender as NoBorderWin;
                if (popupWindow != null)
                {
                    if (popupWindow.Content is System.Windows.Controls.Frame
                        && (popupWindow.Content as System.Windows.Controls.Frame).Content is MMsheOrderControl)
                    {
                        MMsheOrderControl MMsheOrderControl = (popupWindow.Content as System.Windows.Controls.Frame).Content as MMsheOrderControl;
                        MMsheOrderControl.UnregistMM();
                    }
                }

            }
        }

        private void ConfigClick()
        {
            if (this.BondView == null || this.BondView.Count == 0)
            {
                return;
            }

            string securityID = CommonUtils.ConvertToString(this.SelectedItem[SECURITY_ID_FIELD]);
            bool enable = CommonUtils.ConvertToString(this.SelectedItem[ENABLE_FIELD]).Equals("1") ? true : false;
            OrderParamProp mmParams = this.SelectedItem[PARAMS_FIELD] as OrderParamProp;
            if (mmParams != null)
            {
                MMOrderControlModel vm = new MMOrderControlModel()
                {
                    enable = enable,
                    securityID = securityID,
                    Params = mmParams
                };
                OrderParamProp param = mmParams;

                if (param != null)
                {
                    if (string.IsNullOrEmpty(param.securityid))
                    {
                        param.securityid = securityID;
                    }

                    if (!string.IsNullOrEmpty(param.securityid))
                    {
                        BondsInfoPo bondsInfoPo = CommonUtils.GetBondsInfoPoBySecurityID(param.securityid);
                        String today = DateTimeEx.ServerTime.ToString("yyyyMMdd");

                        if (bondsInfoPo == null)
                        {
                            return;
                        }

                        if (today.CompareTo(bondsInfoPo.Maturity_Date.ToString()) > 0)
                        {
                            return;
                        }
                        if ("BNC".Equals(bondsInfoPo.symbolType))
                        {
                            param.IsInterest = "true";
                        }
                        else
                        {
                            param.IsInterest = "false";
                        }
                        if (string.IsNullOrEmpty(param.symbol))
                        {
                            param.symbol = bondsInfoPo.Short_Name;
                        }
                        //if (string.IsNullOrEmpty(param.exeBondFollow))
                        //{
                        //    if (!string.IsNullOrEmpty(bondsInfoPo.Option_Type) &&
                        //    ((!bondsInfoPo.Option_Type.Equals("NON") && bondsInfoPo.Option_Date != null) ||
                        //    (bondsInfoPo.Option_Type.Equals("ETS") && bondsInfoPo.Option_Date == null)))
                        //    {
                        //        param.exeBondFollow = "0";
                        //    }
                        //    else
                        //    {
                        //        param.exeBondFollow = "1";
                        //    }
                        //}
                        //RiskQtyBySecurityReplyPo replyPo = MMsheCommonUtils.QueryRiskQtyBySecurity(param.securityid);
                        //if (replyPo.IsSuccess() && replyPo.Data != null)
                        //{
                        //    param.riskMinQtySize = replyPo.Data.riskMinQtySize;
                        //    param.riskMaxQtyRatio = replyPo.Data.riskMaxQtyRatio;
                        //}
                        OrderDetailView odView = new OrderDetailView();

                        OrderDetailViewModel odVM;
                        odVM = new OrderDetailViewModel(param);
                        odVM.SelectedItemRow = vm;
                        odVM.MyWindow = odView;

                        odView.DataContext = odVM;
                        odView.ShowDialog();
                    }

                }
            }
        }

        private void ToDel()
        {
            foreach (DataRowView drv in this.BondView)
            {
                if (!(drv[ISCHECKED_FIELD] is System.DBNull)
                    && (bool)drv[ISCHECKED_FIELD] == true)
                {
                    string enable = CommonUtils.ConvertToString(drv[ENABLE_FIELD]);
                    if ("True" == enable)
                    {
                        CustomMessageBox.Warning("选中债券内存在未关闭策略，请确认关闭后再删除");
                        return;
                    }
                }
            }

            BatchOperateBtnEnable = false;

            string text = "确定要删除选中策略吗？";
            string title = "批量删除";
            if (CustomMessageBox.Question(text, title) == true)
            {
                List<STC.Common.Model.MMshe.MMshe> delMMList = new List<STC.Common.Model.MMshe.MMshe>();
                lock (lockObj)
                {
                    foreach (DataRowView drv in this.BondView)
                    {
                        if (!(drv[ISCHECKED_FIELD] is System.DBNull)
                            && (bool)drv[ISCHECKED_FIELD] == true)
                        {
                            string securityID = CommonUtils.ConvertToString(drv[SECURITY_ID_FIELD]);
                            string symbol = CommonUtils.ConvertToString(drv[SYMBOL_FIELD]);
                            string settltype = CommonUtils.ConvertToString(drv[SETTLE_TYPE_FIELD]);

                            STC.Common.Model.MMshe.MMshe request = new STC.Common.Model.MMshe.MMshe();
                            request.securityid = securityID;
                            request.groupid = groupID;
                            request.settltype = settltype;
                            request.venues = CommonUtils.ConvertToString(drv[VENUES_FIELD]);
                            request.venuetypegw = CommonUtils.ConvertToString(drv[VENUETYPEGW_FIELD]);
                            delMMList.Add(request);
                        }
                    }
                }

                ThreadPool.QueueUserWorkItem(o =>
                {
                    MMsheReq updateReq = new MMsheReq();
                    updateReq.Action = "3"; // 删除
                    updateReq.mmList = delMMList;
                    PResult pResult = MMsheHandler.Instance.mmUpdate(updateReq);
                    if (pResult == null)
                    {
                        Prompt2Second("警告", "批量删除失败");
                    }
                    else
                    {
                        if (pResult != null && !pResult.IsSuccess())
                        {
                            Prompt2Second("警告", pResult.Msg);
                        }
                        else if (pResult != null && pResult.IsSuccess())
                        {
                            //删除history.data下相关节点
                            foreach (var item in delMMList)
                            {
                                MMsheCommonUtils.ClearSection(item.securityid);
                            }
                        }
                    }
                });
            }

            BatchOperateBtnEnable = true;
        }

        private void ToMove()
        {
            BatchOperateBtnEnable = false;
            ChangeGroupView changeGroupView = new ChangeGroupView(this.groupID);
            if (changeGroupView.ShowDialog() == true)
            {
                string newGroupID = (changeGroupView.DataContext as ChangeGroupViewModel)?.SelectedGroup.GroupID;
                List<STC.Common.Model.MMshe.MMshe> delMMList = new List<STC.Common.Model.MMshe.MMshe>();
                lock (lockObj)
                {
                    foreach (DataRowView drv in this.BondView)
                    {
                        if (!(drv[ISCHECKED_FIELD] is System.DBNull)
                            && (bool)drv[ISCHECKED_FIELD] == true)
                        {
                            string securityID = CommonUtils.ConvertToString(drv[SECURITY_ID_FIELD]);
                            string symbol = CommonUtils.ConvertToString(drv[SYMBOL_FIELD]);
                            string settltype = CommonUtils.ConvertToString(drv[SETTLE_TYPE_FIELD]);
                            OrderParamProp mmParams = drv[PARAMS_FIELD] as OrderParamProp;


                            STC.Common.Model.MMshe.MMshe request = new STC.Common.Model.MMshe.MMshe();
                            request.securityid = securityID;
                            request.symbol = symbol;
                            request.groupid = groupID;
                            request.settltype = settltype;
                            request.terminal = string.Format("STClient_v{0}", LocalEnv.ServerInfo.Version);
                            request.enable = "0";
                            request.venuetypegw = request.venues = mmParams.Venuetypegw;
                            //request.ip = 
                            request.Params = mmParams?.MMsheOrderParamReq;
                            delMMList.Add(request);
                        }
                    }
                }

                ThreadPool.QueueUserWorkItem(o =>
                {
                    MMsheReq delReq = new MMsheReq();
                    delReq.Action = "3"; // 删除
                    delReq.mmList = delMMList;
                    PResult pResult = MMsheHandler.Instance.mmUpdate(delReq);
                    if (pResult == null)
                    {
                        Prompt2Second("警告", $"移动至组【{MMsheGroupHandler.Instance.GetGroupInfoByGroupID(groupID).groupname}】失败");
                    }
                    else
                    {
                        if (!pResult.IsSuccess())
                        {
                            Prompt2Second("警告", pResult.Msg);
                        }
                        else
                        {
                            MMsheReq addReq = new MMsheReq();
                            addReq.Action = "1"; // 添加
                            foreach (STC.Common.Model.MMshe.MMshe mm in delMMList)
                            {
                                mm.Action = "1";
                                mm.groupid = newGroupID;
                            }
                            addReq.mmList = delMMList;
                            PResult pResult1 = MMsheHandler.Instance.mmUpdate(addReq);

                            if (pResult1 == null)
                            {
                                Prompt2Second("警告", $"移动至组【{MMsheGroupHandler.Instance.GetGroupInfoByGroupID(groupID).groupname}】失败");
                            }
                            else
                            {
                                if (!pResult.IsSuccess())
                                {
                                    Prompt2Second("警告", pResult.Msg);
                                }
                            }
                        }
                    }
                });
            }
            BatchOperateBtnEnable = true;
        }

        private void ToAuto()
        {
            BatchOperateBtnEnable = false;
            lock (lockObj)
            {
                foreach (DataRowView drv in this.BondView)
                {
                    if (!(drv[ISCHECKED_FIELD] is System.DBNull)
                        && (bool)drv[ISCHECKED_FIELD] == true)
                    {
                        MMsheOrderParamReq req = drv[PARAMS_FIELD] as MMsheOrderParamReq;
                        string securityID = CommonUtils.ConvertToString(drv[SECURITY_ID_FIELD]);
                        string symbol = CommonUtils.ConvertToString(drv[SYMBOL_FIELD]);
                        string settltype = CommonUtils.ConvertToString(drv[SETTLE_TYPE_FIELD]);
                        OrderParamProp mmParams = drv[PARAMS_FIELD] as OrderParamProp;

                        ThreadPool.QueueUserWorkItem(o =>
                        {
                            if (req == null)
                            {
                                req = mmParams?.MMsheOrderParamReq == null ? new MMsheOrderParamReq() : mmParams?.MMsheOrderParamReq;
                                req.securityid = securityID;
                                req.baseSettleType = settltype;
                                req.symbol = symbol;
                            }
                            req.bidQuoteModel = "2";
                            req.ofrQuoteModel = "2";
                            PResult result = STC.Common.Handler.Admin.MMsheHandler.Instance.mmUpdateParams(req);
                        });

                    }
                }
            }
            BatchOperateBtnEnable = true;
        }

        private void ToManual()
        {
            BatchOperateBtnEnable = false;
            lock (lockObj)
            {
                foreach (DataRowView drv in this.BondView)
                {
                    if (!(drv[ISCHECKED_FIELD] is System.DBNull)
                        && (bool)drv[ISCHECKED_FIELD] == true)
                    {
                        MMsheOrderParamReq req = drv[PARAMS_FIELD] as MMsheOrderParamReq;
                        string securityID = CommonUtils.ConvertToString(drv[SECURITY_ID_FIELD]);
                        string symbol = CommonUtils.ConvertToString(drv[SYMBOL_FIELD]);
                        string settltype = CommonUtils.ConvertToString(drv[SETTLE_TYPE_FIELD]);
                        OrderParamProp mmParams = drv[PARAMS_FIELD] as OrderParamProp;

                        ThreadPool.QueueUserWorkItem(o =>
                        {
                            if (req == null)
                            {
                                req = mmParams?.MMsheOrderParamReq == null ? new MMsheOrderParamReq() : mmParams?.MMsheOrderParamReq;
                                req.securityid = securityID;
                                req.baseSettleType = settltype;
                                req.symbol = symbol;
                            }
                            req.bidQuoteModel = "1";
                            req.ofrQuoteModel = "1";
                            PResult result = STC.Common.Handler.Admin.MMsheHandler.Instance.mmUpdateParams(req);
                        });

                    }
                }
            }
            BatchOperateBtnEnable = true;
        }

        private void ToClose()
        {
            BatchOperateBtnEnable = false;

            ThreadPool.QueueUserWorkItem(o =>
            {
                if (this.BondView.Count == 0)
                {
                    return;
                }
                List<MMsheUpdateEnableReq> requestList = new List<MMsheUpdateEnableReq>();
                string macID = EngineHelper.GetMacByNetworkInterface();
                string insideIP = EngineHelper.GetIp();
                string hardDiskID = EngineHelper.Getharddiskid2();
                lock (lockObj)
                {
                    foreach (DataRowView drv in this.BondView)
                    {
                        if (!(drv[ISCHECKED_FIELD] is System.DBNull)
                                && (bool)drv[ISCHECKED_FIELD] == true
                                && (bool)drv[ENABLE_FIELD] == true)
                        {
                            string securityID = CommonUtils.ConvertToString(drv[SECURITY_ID_FIELD]);
                            string settltype = CommonUtils.ConvertToString(drv[SETTLE_TYPE_FIELD]);

                            MMsheUpdateEnableReq req = new MMsheUpdateEnableReq();
                            req.securityid = securityID;
                            req.settltype = settltype;
                            req.enable = "0";
                            req.macID = macID;
                            req.insideIP = insideIP;
                            req.hardDiskID = hardDiskID;
                            req.venuetypegw = CommonUtils.ConvertToString(drv[MmsheListBoxViewModel.VENUETYPEGW_FIELD]);
                            req.venues = CommonUtils.ConvertToString(drv[MmsheListBoxViewModel.VENUES_FIELD]);

                            requestList.Add(req);
                        }
                    }
                }

                foreach (MMsheUpdateEnableReq req in requestList)
                {
                    PResult pResult = MMsheHandler.Instance.mmUpdateEnable(req);
                    if (pResult == null)
                    {
                        Prompt2Second("警告", "关闭失败");
                    }
                    else
                    {
                        if (pResult != null && !pResult.IsSuccess())
                        {
                            Prompt2Second("警告", pResult.Msg);
                        }
                    }
                    if (LocalEnv.ConfInfo.MMCloseTimeInterval > (11))
                    {
                        Thread.Sleep(LocalEnv.ConfInfo.MMCloseTimeInterval);
                    }
                    else
                    {
                        if (LocalEnv.ConfInfo.MMCloseTimeInterval != -1)
                        {
                            Thread.Sleep(11);
                        }
                    }
                    //Console.WriteLine("-------------------------" + DateTimeEx.ServerTime.ToString("yyyy-MM-dd HH:mm:ss.fff"));
                }
            });
            BatchOperateBtnEnable = true;
        }

        private void ToOpen()
        {
            BatchOperateBtnEnable = false;
            ThreadPool.QueueUserWorkItem(o =>
            {
                if (this.BondView.Count == 0)
                {
                    return;
                }
                List<MMsheUpdateEnableReq> requestList = new List<MMsheUpdateEnableReq>();
                string macID = EngineHelper.GetMacByNetworkInterface();
                string insideIP = EngineHelper.GetIp();
                string hardDiskID = EngineHelper.Getharddiskid2();
                lock (lockObj)
                {
                    foreach (DataRowView drv in this.BondView)
                    {
                        if (!(drv[ISCHECKED_FIELD] is System.DBNull)
                            && (bool)drv[ISCHECKED_FIELD] == true
                            && (bool)drv[ENABLE_FIELD] == false
                            && CommonUtils.ConvertToString(drv[REMAINGING_TIME_FIELD]) != "不可用")
                        {
                            string securityID = CommonUtils.ConvertToString(drv[SECURITY_ID_FIELD]);
                            string settltype = CommonUtils.ConvertToString(drv[SETTLE_TYPE_FIELD]);

                            MMsheUpdateEnableReq req = new MMsheUpdateEnableReq();
                            req.securityid = securityID;
                            req.settltype = settltype;
                            req.enable = "1";
                            req.macID = macID;
                            req.insideIP = insideIP;
                            req.hardDiskID = hardDiskID;
                            req.venuetypegw = CommonUtils.ConvertToString(drv[MmsheListBoxViewModel.VENUETYPEGW_FIELD]);
                            req.venues = CommonUtils.ConvertToString(drv[MmsheListBoxViewModel.VENUES_FIELD]);

                            requestList.Add(req);
                        }
                    }
                }
                foreach (MMsheUpdateEnableReq req in requestList)
                {
                    PResult pResult = MMsheHandler.Instance.mmUpdateEnable(req);
                    if (pResult == null)
                    {
                        Prompt2Second("警告", "开启失败");
                    }
                    else
                    {
                        if (pResult != null && !pResult.IsSuccess())
                        {
                            Prompt2Second("警告", pResult.Msg);
                        }
                    }
                    if (LocalEnv.ConfInfo.MMOpenTimeInterval > (11))
                    {
                        Thread.Sleep(LocalEnv.ConfInfo.MMOpenTimeInterval);
                    }
                    else
                    {
                        Thread.Sleep(11);
                    }
                    //Console.WriteLine("-------------------------"+DateTimeEx.ServerTime.ToString("yyyy-MM-dd HH:mm:ss.fff"));
                }
            });

            BatchOperateBtnEnable = true;
        }

        private void Instance_OnClear(ClearPo obj)
        {
            LogHandler.Info("MM List Clear");
            if (obj != null && "Y".Equals(obj.IsClear))
            {
                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    this.Clear();
                    this.Init(this.groupID);
                }));
            }
        }

        ~MmsheListBoxViewModel()
        {
            LogHandler.Info("mmListBox Dispose");
            this.Dispose();
        }

        new public void Dispose()
        {
            Clear();

            BaseHandler.Instance.UnSubscribe(Instance_OnClear);

            if (_RefreshBaseDataThread != null)
            {
                LogHandler.Info("mmListBox Dispose RefreshBaseDataThread");
                //_RefreshBaseDataThread.Abort();
                _isRefreshBaseDataThreadDispose = true;
                _RefreshBaseDataThread = null;
            }


            if (_CalcMDDataThread != null)
            {
                LogHandler.Info("mmListBox Dispose CalcMDDataThread");
                //_CalcMDDataThread.Abort();
                _CalcMDDataThreadDispose = true;
                _CalcMDDataThread = null;
            }


            if (_RefreshCheckBoxThread != null)
            {
                LogHandler.Info("mmListBox Dispose RefreshCheckBoxThread");
                //_RefreshCheckBoxThread.Abort();
                _RefreshCheckBoxThreadDispose = true;
                _RefreshCheckBoxThread = null;
            }


            if (_RefreshBondsThread != null)
            {
                LogHandler.Info("mmListBox Dispose RefreshBondsThread");
                //_RefreshBondsThread.Abort();
                _RefreshBondsThreadDispose = true;
                _RefreshBondsThread = null;
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

            lock (_LastCTPriceDic)
            {
                if (_LastCTPriceDic != null)
                {
                    _LastCTPriceDic.Clear();
                    _LastCTPriceDic = null;
                }
            }

            //if (_ProcessBondsThreadQueue != null)
            //{
            //    _ProcessBondsThreadQueue.Clear();
            //    _ProcessBondsThreadQueue.Dispose();
            //    _ProcessBondsThreadQueue = null;
            //}
            if (_ProcessAnsMonitorInfThreadQueue != null)
            {
                _ProcessAnsMonitorInfThreadQueue.Clear();
                _ProcessAnsMonitorInfThreadQueue.Dispose();
                _ProcessAnsMonitorInfThreadQueue = null;
            }

            base.Dispose();
        }


        public void Clear()
        {
            if (_BondView != null)
            {
                foreach (DataRowView bond in _BondView)
                {
                    string securityID = CommonUtils.ConvertToString(bond[SECURITY_ID_FIELD]);
                    PriceHandler.Instance.UnregisterCTPrice(securityID, Instance_OnMDPrice);
                    DataView removeDV;
                    if (baseCTPriceDataViewDic.ContainsKey(securityID))
                    {
                        lock (baseCTPriceDataViewDic[securityID])
                        {
                            baseCTPriceDataViewDic.TryRemove(securityID, out removeDV);
                        }
                    }

                    MarketDataSnapshotFullRefresh ctPrice;
                    baseCTPriceDataDic.TryRemove(securityID, out ctPrice);
                    //EmsOrdersHandler.Instance.UnregistEMSOrderBySecurityID(securityID, EmsOrderProcesser.Processer.Instance_OnEmsOrders);
                }
            }

            MyOrderProcesser.Processer.OnOwnMMSHEOrdersChange -= Instance_OnOrder;

            MMsheGroupHandler.Instance.OnMMBonds -= Instance_OnMM;

            AnsDataHandler.Instance.OnAnsMonitorInfo -= Instance_OnAnsMonitorInfo;

            lock (this.lockObj)
            {
                if (_BondView != null)
                {
                    _BondView.Table.Rows.Clear();
                }
            }
            if (_LastCTPriceDic != null)
            {
                lock (_LastCTPriceDic)

                    _LastCTPriceDic.Clear();
            }

            //if (_ProcessBondsThreadQueue != null)
            //{
            //    _ProcessBondsThreadQueue.Clear();
            //}
            if (_ProcessAnsMonitorInfThreadQueue != null)
            {
                _ProcessAnsMonitorInfThreadQueue.Clear();
            }
        }

        #region 过滤
        private string _rowSettlTypeFilterStr = string.Empty;
        StringBuilder filterSB = new StringBuilder();
        /// <summary>
        /// 过滤数据
        /// </summary>
        public void FilterData(string SearchText, StringBuilder filterBondType, StringBuilder filterCreditBondType,
            StringBuilder PolicyFinancialBond, StringBuilder filterRemainingTime, StringBuilder filterNewBondSign,
            StringBuilder filterRating, StringBuilder filterSettlType, StringBuilder openClose, StringBuilder mmStatus, StringBuilder isPauseStatus)
        {
            // 初始化过滤条件
            filterSB.Remove(0, filterSB.Length);

            // 添加债券代码过滤
            #region 筛选债券代码
            if (SearchText.Length > 0)
            {
                this.filterSB.Append($"{SECURITY_ID_FIELD} like '%{SearchText}%'");
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

            // 添加新发国债过滤
            #region 筛选新发国债
            if (filterNewBondSign.Length > 0)
            {
                if (filterSB.Length > 0)
                {
                    filterSB.Append(" and ");
                }
                filterSB.Append($"({filterNewBondSign})");
            }

            #endregion

            // 政策性金融债
            if (PolicyFinancialBond.Length > 0)
            {
                if (filterSB.Length > 0)
                {
                    filterSB.Append(" and ");
                }
                filterSB.Append($"({PolicyFinancialBond})");
            }

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
            if (filterSettlType != null && filterSettlType.Length > 0)
            {
                //_rowSettlTypeFilterStr = filterSettlType.ToString();
                if (filterSB.Length > 0)
                {
                    filterSB.Append(" and ");
                }
                filterSB.Append(filterSettlType);
            }

            #endregion

            #region 策略状态
            if (openClose != null && openClose.Length > 0)
            {
                if (filterSB.Length > 0)
                {
                    filterSB.Append(" and ");
                }
                filterSB.Append($"({openClose})");
            }
            #endregion

            #region 做市状态
            if (mmStatus != null && mmStatus.Length > 0)
            {
                if (filterSB.Length > 0)
                {
                    filterSB.Append(" and ");
                }
                filterSB.Append($"({mmStatus})");
            }

            #endregion
            #region 断报券
            if (isPauseStatus != null && isPauseStatus.Length > 0)
            {
                if (filterSB.Length > 0)
                {
                    filterSB.Append(" and ");
                }
                filterSB.Append($"({isPauseStatus})");
            }
            #endregion

            //if (this.SelectedItem != null) _originalSelectedItemSecurityID = CommonUtils.ConvertToString(this.SelectedItem[SECURITY_ID_FIELD]);
            this.BondView.RowFilter = filterSB.ToString();

            if (this.BondView.Count > 0)
            {
                this.SelectedItem = this.BondView[0];
            }

            //if (this.SelectedItem == null && this.BondView.Count > 0)
            //{
            //    bool isFind = false;
            //    foreach (DataRowView drv in this.BondView)
            //    {
            //        if (CommonUtils.ConvertToString(drv[SECURITY_ID_FIELD]) == _originalSelectedItemSecurityID)
            //        {
            //            this.SelectedItem = drv;
            //            isFind = true;
            //            break;
            //        }
            //    }
            //    if (!isFind)
            //    {
            //        this.SelectedItem = this.BondView[0];
            //    }
            //}
        }
        #endregion

        private void Init(string groupID)
        {
            this.groupID = groupID;
            SelectedGroup = MMsheGroupHandler.Instance.GetGroupInfoByGroupID(groupID);
            ProcessInf1();

            // 订阅我的订单
            MyOrderProcesser.Processer.OnOwnMMSHEOrdersChange += Instance_OnOrder;

            if (_ProcessAnsMonitorInfThreadQueue == null)
            {
                _ProcessAnsMonitorInfThreadQueue = new AsyncThreadQueue<ANSMonitorInfo>(UpdateAnsMonitorInfAsync);
            }
            AnsDataHandler.Instance.OnAnsMonitorInfo += Instance_OnAnsMonitorInfo;


            MMsheGroupHandler.Instance.OnMMBonds += Instance_OnMM;
            List<STC.Common.Model.MMshe.MMshe> mmList = MMsheGroupHandler.Instance.GetMMsByGroupID(this.groupID);
            List<STC.Common.Model.MMshe.MMshe> cloneMMList = mmList?.ToList();
            LogHelper.Info($"---------------------{groupID}:{cloneMMList?.Count}");
            if (cloneMMList != null && cloneMMList.Count > 0)
            {
                isUpdatingMMData = true;

                for (int i = cloneMMList.Count - 1; i >= 0; i--)
                {
                    Instance_OnMM(cloneMMList[i]);
                }
            }
            isUpdatingMMData = false;
        }

        private void Instance_OnAnsMonitorInfo(ANSMonitorInfo obj)
        {
            if (_ProcessAnsMonitorInfThreadQueue != null)
            {
                _ProcessAnsMonitorInfThreadQueue.Enqueue(obj);
            }
        }

        private void UpdateAnsMonitorInfAsync(ANSMonitorInfo obj)
        {
            lock (this.lockObj)
            {
                string securityID = obj.securityId;
                string settleType = obj.settleType;
                /*DataRow dr = this.BondView.Table.Rows.Find(new object[] { obj.securityId });
                if (dr != null)
                {
                    string oldUpdateTime = CommonUtils.ConvertToString(dr[ANS_UPDATETIME_FIELD]);
                    if (oldUpdateTime.CompareTo(obj.updateTime) > 0)
                    {
                        return;
                    }
                    dr.SetValue(ANS_PAUSE_STATUS_FIELD, obj.pauseStatus);
                    dr.SetValue(ANS_MM_STATUS_FIELD, obj.monitorStatus);
                    dr.SetValue(ANS_VALID_QUOTA_DURATION_FIELD, obj.quoteTimeLength);
                    dr.SetValue(ANS_UPDATETIME_FIELD, obj.updateTime);
                    dr.SetValue(ANS_NEW_BOND_SIGN_FIELD, obj.newBondSign);
                    dr.SetValue(ANS_SORT_STATUS_FIELD, $"{obj.monitorStatus}:{obj.pauseStatus}");
                }*/
                //else
                //{
                //    QueryANSMonitorInfo(dr, obj.securityId);
                //}
            }
        }

        public void UpdateMoniValue()
        {
            lock (lockObj)
            {
                foreach (DataRow dr in BondView.Table.Rows)
                {
                    string securityid = dr[SECURITY_ID_FIELD].ConvertToString();
                    string venue = dr[VENUES_FIELD].ConvertToString();
                    Instance_OnOrder($"{securityid}#{venue}", "1");
                    Instance_OnOrder($"{securityid}#{venue}", "2");
                }
            }
        }

        private bool isUpdatingBaseData = false;
        private void RefreshBaseData()
        {
            try
            {
                while (!_isRefreshBaseDataThreadDispose)
                {
                    try
                    {
                        #region 刷新基础数据
                        if (bondWithOutInfoList.Count > 0 && isUpdatingBaseData == false && mainWinList.Count > 0)
                        {
                            isUpdatingBaseData = true;
                            List<BondsInfoPo> updateList = new List<BondsInfoPo>();
                            foreach (KeyValuePair<string, int> pair in bondWithOutInfoList)
                            {
                                BondsInfoPo bondsInfoPo = CommonUtils.GetBondsInfoPoBySecurityID(pair.Key);
                                if (CommonUtils.HasCacheBondInfo(pair.Key))
                                {
                                    updateList.Add(bondsInfoPo);
                                    int count = 0;
                                    bool flag = this.bondWithOutInfoList.TryRemove(pair.Key, out count);
                                }
                                else
                                {
                                    this.bondWithOutInfoList[pair.Key] = pair.Value + 1;
                                    if (pair.Value > 1000)
                                    {
                                        int count = 0;
                                        bool flag = this.bondWithOutInfoList.TryRemove(pair.Key, out count);
                                        //_notExistBaseInfoList.Add(pair.Key);
                                    }
                                }
                            }
                            if (Application.Current != null)
                            {
                                try
                                {
                                    if (updateList != null && updateList.Count > 0)
                                    {
                                        Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                                        {
                                            Sub_RefreshBaseDataAsync(updateList);
                                        }));
                                    }
                                    else
                                    {
                                        isUpdatingBaseData = false;
                                    }
                                }
                                catch (Exception e)
                                {
                                    LogHelper.Error(e);
                                    isUpdatingBaseData = false;
                                }
                            }
                            else
                            {
                                isUpdatingBaseData = false;
                            }
                        }
                        #endregion

                        #region 刷新行情
                        if (baseCTPriceDataDic.Count > 0 && isUpdatingMdData == false && mainWinList.Count > 0)
                        {
                            isUpdatingMdData = true;
                            if (Application.Current != null)
                            {
                                try
                                {
                                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                                    {
                                        Sub_RefreshMdDataAsync();
                                    }));
                                }
                                catch (Exception e)
                                {
                                    LogHelper.Error(e);
                                    isUpdatingMdData = false;
                                }
                            }
                            else
                            {
                                isUpdatingMdData = false;
                            }

                        }
                        #endregion

                        #region 刷新订单
                        if (_LastOrderDic.Count > 0 && isUpdatingMyOrders == false && mainWinList.Count > 0)
                        {
                            isUpdatingMyOrders = true;

                            if (Application.Current != null)
                            {
                                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                                {
                                    Sub_RefreshMyOrdersAsync();

                                }));
                            }
                            else
                            {
                                isUpdatingMyOrders = false;
                            }

                        }
                        #endregion
                        Thread.Sleep(500);
                    }
                    catch (Exception e)
                    {
                        LogHandler.Error(e);
                    }
                }
            }
            catch (ThreadAbortException)
            {
                //ignore
            }
            catch (Exception e)
            {
                LogHandler.Error(e);
            }
        }

        //private void onloadData(bool flag)
        //{
        //    List<string> tempList;
        //    lock (_notExistBaseInfoList)
        //    {
        //        tempList = _notExistBaseInfoList.ToList();
        //        _notExistBaseInfoList.Clear();
        //    }
        //    if (tempList != null && tempList.Count > 0)
        //    {
        //        foreach (string securityID in tempList)
        //        {
        //            if (bondWithOutInfoList.ContainsKey(securityID) == false)
        //            {
        //                _ProcessCacheBondInfoThreadQueue.Enqueue(securityID);
        //                bondWithOutInfoList[securityID] = 0;
        //            }
        //        }
        //    }
        //}


        private void Sub_RefreshBaseDataAsync(List<BondsInfoPo> list)
        {
            try
            {
                if (list != null && list.Count > 0)
                {
                    foreach (BondsInfoPo bondInfo in list)
                    {
                        if (bondInfo != null)
                        {
                            BindBaseData(bondInfo);
                            //await System.Windows.Threading.Dispatcher.Yield();
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
                isUpdatingBaseData = false;
            }
        }

        private void BindBaseData(BondsInfoPo bondsInfoPo)
        {
            lock (this.lockObj)
            {
                DataRow dr = this.BondView.Table.Rows.Find(new object[] { bondsInfoPo.Bond_ID });
                if (dr != null)
                {
                    if (bondsInfoPo != null)
                    {
                        dr.SetValue(SYMBOL_FIELD, bondsInfoPo.Short_Name);// 代码名称
                        if (bondsInfoPo.MarketIndicator == "4")
                        {
                            String today = DateTimeEx.ServerTime.ToString("yyyyMMdd");
                            if (today.CompareTo(bondsInfoPo.Maturity_Date.ToString()) > 0)
                            {
                                dr.SetValue(REMAINGING_TIME_FIELD, "不可用");//剩余时间
                            }
                            else
                            {
                                dr.SetValue(REMAINGING_TIME_FIELD, bondsInfoPo.RemaingingTime);//剩余时间
                            }


                            dr.SetValue(REMAINGING_TIME_CONVERT_FIELD, ConvertRemainingTime(bondsInfoPo.RemaingingTime));//剩余时间转换成时间段
                            dr.SetValue(BOND_TYPE_FIELD, CommonUtils.GetBondCategoryName(bondsInfoPo.Bond_Subtype, bondsInfoPo.Ent_Cor));// 债券类型 
                        }
                        else
                        {
                            dr.SetValue(REMAINGING_TIME_FIELD, "不可用");
                            //dr[REMAINGING_TIME_CONVERT_FIELD] = string.Empty;
                            dr.SetValue(BOND_TYPE_FIELD, string.Empty);
                        }
                        dr.SetValue(PUBLISHER_FIELD, bondsInfoPo.CN_Short_Name);//发行人

                        dr.SetValue(ISSUER_RATING_CURRENT_NPY_FIELD, bondsInfoPo.Issuer_Rating_Current_NPY); // 评级

                        dr.SetValue(BOND_SUB_TYPE_FIELD, bondsInfoPo.Bond_Subtype); //债券子类
                        dr.SetValue(GOODS_TYPE_FIELD, bondsInfoPo.SECURITY_TYPE);  //债券大类
                        dr.SetValue(ENT_COR_FIELD, bondsInfoPo.Ent_Cor); //Ent_Cor
                        dr.SetValue(ISSUER_FIELD, bondsInfoPo.issuer);
                        dr.SetValue(BOND_CATEGORY_FIELD, MMsheCommonUtils.GetBondCategoryName(bondsInfoPo.Security_Type_Id));
                        //drv[OTHER_FIELD] = bondsInfoPo.Rating_Current + "||(" + bondsInfoPo.SECURITY_TYPE + ":" + bondsInfoPo.Bond_Subtype + ":" + bondsInfoPo.Ent_Cor + ")";
                        dr.SetValue(SECURITY_TYPE_ID_FIELD, bondsInfoPo.Security_Type_Id);
                        dr.SetValue(SECURITY_DESC_FIELD, bondsInfoPo.SECURITY_DEC); 
                        dr.SetValue(SECURITY_TERM_FIELD, bondsInfoPo.Security_Term?.Replace('年', 'Y'));
                        BindIsSameTagToUI(bondsInfoPo.Bond_ID, dr);
                    }
                    else
                    {
                        dr.SetValue(SYMBOL_FIELD, string.Empty);// 代码名称
                        dr.SetValue(REMAINGING_TIME_FIELD, "不可用");//剩余时间
                        dr.SetValue(REMAINGING_TIME_CONVERT_FIELD, 0);//剩余时间转换成时间段
                        dr.SetValue(ISSUER_RATING_CURRENT_NPY_FIELD, string.Empty); // 评级
                        dr.SetValue(BOND_TYPE_FIELD, string.Empty);// 债券类型 
                        dr.SetValue(BOND_SUB_TYPE_FIELD, string.Empty); //债券子类
                        dr.SetValue(GOODS_TYPE_FIELD, string.Empty);  //债券大类
                        dr.SetValue(ENT_COR_FIELD, string.Empty); //Ent_Cor
                        dr.SetValue(BOND_CATEGORY_FIELD, string.Empty);
                        dr.SetValue(ISSUER_FIELD, string.Empty);
                        dr.SetValue(PUBLISHER_FIELD, string.Empty);
                        dr.SetValue(SECURITY_TYPE_ID_FIELD, string.Empty);
                        dr.SetValue(SECURITY_DESC_FIELD, string.Empty);
                        dr.SetValue(SECURITY_TERM_FIELD, string.Empty);
                    }
                }
            }
        }


        private DataView CreateBaseDataView()
        {
            DataView baseDV = new DataTable().DefaultView;
            baseDV.Table.Columns.Add(CTPriceControler.KEY_FIELD, typeof(string));
            baseDV.Table.Columns.Add(CTPriceControler.SECURITY_ID_FIELD, typeof(string));
            baseDV.Table.Columns.Add(CTPriceControler.TOTAL_QTY_FIELD, typeof(long));
            baseDV.Table.Columns.Add(CTPriceControler.TIME_FIELD, typeof(string));
            baseDV.Table.Columns.Add(CTPriceControler.SOURCE_FIELD, typeof(string));
            baseDV.Table.Columns.Add(CTPriceControler.QTY_FIELD, typeof(long));
            baseDV.Table.Columns.Add(CTPriceControler.ENTRY_PX_FIELD, typeof(string));
            baseDV.Table.Columns.Add(CTPriceControler.Yield_Field, typeof(string));
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
            baseDV.Table.Columns.Add(VALUATION_FIELD, typeof(string));
            baseDV.Table.Columns.Add(VALUATION_TYPE_FIELD, typeof(decimal));
            baseDV.Table.Columns.Add(VALUATION_EXE_FIELD, typeof(string));
            baseDV.Table.Columns.Add(VALUATION_MAT_FIELD, typeof(string));
            baseDV.Table.Columns.Add(CTPriceControler.VENUE_SORT_FIELD, typeof(string));
            baseDV.Table.Columns.Add(CTPriceControler.ISINNER_FIELD, typeof(string));
            baseDV.Table.Columns.Add(CTPriceControler.INFO3_FIELD, typeof(string));
            baseDV.Table.Columns.Add(CTPriceControler.INFO4_FIELD, typeof(string));
            baseDV.Table.Columns.Add(CTPriceControler.IsInnerInComplete_Field, typeof(string));
            baseDV.Table.Columns.Add(CTPriceControler.YieldNum_Field, typeof(decimal));
            baseDV.Table.Columns.Add(CTPriceControler.MARKETINDICATOR_FIELD, typeof(string));
            baseDV.Table.Columns.Add(CTPriceControler.BOND_TYPE_FIELD, typeof(string));
            //_BaseDV.Sort = $"{VENUE_TYPE_GW_FIELD},{SOURCE_FIELD},{ENTRY_PX_FIELD},{MD_PRICE_LEVEL_FIELD}";
            baseDV.Sort = $"{CTPriceControler.SIDE_FIELD},{CTPriceControler.MD_PRICE_LEVEL_FIELD}";
            baseDV.Table.PrimaryKey = new DataColumn[] { baseDV.Table.Columns[CTPriceControler.KEY_FIELD] };

            return baseDV;
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
            if (_LastCTPriceDic != null)
            {
                lock (_LastCTPriceDic)
                {
                    this._LastCTPriceDic[obj.SecurityID] = obj;
                }
            }

        }


        public void Instance_OnMM(STC.Common.Model.MMshe.MMshe mm)
        {
            if (mm.groupid == this.groupID)
            {
                //LogHelper.Debug($"111》》》》》{groupID}:{mm.Action}:{mm.securityid}:{mm.createtime}");
                //if (_ProcessBondsThreadQueue != null)
                //{
                //    _ProcessBondsThreadQueue.Enqueue(mm);
                //}
                string key = $"{mm.securityid}#{mm.createtime}";
                if (_MMDic.ContainsKey(key) == false)
                {
                    _MMDic[key] = mm;
                }
                else
                {
                    STC.Common.Model.MMshe.MMshe oldMM;
                    bool flag = _MMDic.TryGetValue(key, out oldMM);
                    if (flag == false || (oldMM != null && mm.updatetime.CompareTo(oldMM.updatetime) >= 0))
                    {
                        _MMDic[key] = mm;
                    }
                }
            }
        }

        private bool isUpdatingMMData = true;
        private void RefreshMMData()
        {
            while (!_RefreshBondsThreadDispose)
            {
                try
                {
                    if (_MMDic.Count > 0 && mainWinList.Count > 0 && isUpdatingMMData == false)
                    {
                        isUpdatingMMData = true;

                        if (Application.Current != null)
                        {
                            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                            {
                                OnMMData();

                            }));
                        }
                        else
                        {
                            isUpdatingMMData = false;
                        }
                    }
                    Thread.Sleep(200);
                }
                catch (Exception ex)
                {
                    LogHandler.Error(ex);
                }
            }
        }

        private async void OnMMData()
        {
            bool isNew = false;
            IOrderedEnumerable<STC.Common.Model.MMshe.MMshe> securityIDArr = _MMDic.Values.OrderBy(x => x.createtime);
            foreach (STC.Common.Model.MMshe.MMshe sortedMM in securityIDArr)
            {
                if (mainWinList.Count <= 0)
                {
                    break;
                }

                STC.Common.Model.MMshe.MMshe mm;
                string key = $"{sortedMM.securityid}#{sortedMM.createtime}";
                if (_MMDic.TryRemove(key, out mm))
                {
                    bool flag;
                    //LogHelper.Debug($"》》》》》{groupID}:{sortedMM.Action}:{sortedMM.securityid}:{sortedMM.createtime}");
                    UpdateBondList(mm, out flag);
                    if (flag)
                    {
                        isNew = true;
                    }
                    await System.Windows.Threading.Dispatcher.Yield(System.Windows.Threading.DispatcherPriority.Render);
                }
            }
            if (isNew && this.BondView.Count > 0)
            {
                this.SelectedItem = this.BondView[0];
            }
            isUpdatingMMData = false;
        }

        /// <summary>
        /// 更新债券列表
        /// </summary>
        /// <param name="bond"></param>
        private void UpdateBondList(STC.Common.Model.MMshe.MMshe obj, out bool isNew)
        {
            isNew = false;
            if (obj == null) return;
            try
            {
                lock (this.lockObj)
                {
                    //DataRow bondInfosItems = this.BondView.Table.Rows.Find(new object[] { obj.securityid, obj.settltype, obj.venues, obj.venuetypegw });
                    DataRow bondInfosItems = this.BondView.Table.Rows.Find(new object[] { obj.securityid });

                    if (bondInfosItems != null && !obj.venues.Equals(bondInfosItems[VENUES_FIELD] + "")) 
                    {
                        return;
                    }

                    if (bondInfosItems != null )
                    {
                        string updatetime = CommonUtils.ConvertToString(bondInfosItems[UPDATE_TIME_FIELD]);
                        if (updatetime.CompareTo(obj.updatetime) > 0)
                        {
                            // 忽略比较老的数据
                            return;
                        }

                        if ("3" == obj.Action)
                        {
                            // 删除bond
                            string securityID = obj.securityid;
                            // 取消订阅
                            PriceHandler.Instance.UnregisterCTPrice(securityID, Instance_OnMDPrice);
                            DataView removeDV;
                            if (baseCTPriceDataViewDic.ContainsKey(securityID))
                            {
                                lock (baseCTPriceDataViewDic[securityID])
                                {
                                    baseCTPriceDataViewDic.TryRemove(securityID, out removeDV);
                                }
                            }
                            //EmsOrdersHandler.Instance.UnregistEMSOrderBySecurityID(securityID, EmsOrderProcesser.Processer.Instance_OnEmsOrders);
                            MarketDataSnapshotFullRefresh ctPrice;
                            baseCTPriceDataDic.TryRemove(securityID, out ctPrice);

                            lock (this.lockObj)
                            {
                                int index = this.SelectedIndex;
                                DataRow removeDR = _BondView.Table.Rows.Find(new object[] { securityID });
                                if (removeDR != null)
                                {
                                    this.BondView.Table.Rows.Remove(removeDR);
                                    if (startBonds.ContainsKey(obj.securityid))
                                    {
                                        string removeValue;
                                        startBonds.TryRemove(obj.securityid, out removeValue);

                                        Enable_Unify = startBonds.Count > 0 ? true : false;
                                    }

                                    if (index >= 0 && _BondView.Count > 0)
                                    {
                                        if (index == _BondView.Count)
                                            this.SelectedItem = this.BondView[index - 1];
                                        else
                                            this.SelectedItem = this.BondView[index];
                                    }
                                    else
                                    {
                                        this.SelectedItem = null;
                                    }

                                    int count = 0;
                                    this.bondWithOutInfoList.TryRemove(securityID, out count);
                                }
                            }
                            return;
                        }
                    }

                    if ("3" == obj.Action)
                    {
                        return;
                    }

                    if (bondInfosItems == null)
                    {
                        bondInfosItems = this.BondView.Table.NewRow();

                        bondInfosItems.SetValue(SECURITY_ID_FIELD, obj.securityid);// 代码
                        bondInfosItems.SetValue(SYMBOL_FIELD, obj.symbol);//代码名称
                        bondInfosItems.SetValue(VENUES_FIELD, obj.venues);

                        // 添加行
                        this.BondView.Table.Rows.InsertAt(bondInfosItems, 0);

                        // 选择行
                        if (this.SelectedItem == null && this.BondView.Count > 0)
                        {
                            this.SelectedItem = this.BondView[0];
                        }
                        isNew = true;

                        // 基础数据
                        if (CommonUtils.HasCacheBondInfo(obj.securityid))
                        {
                            BondsInfoPo bondsInfo = CommonUtils.GetBondsInfoPoBySecurityID(obj.securityid);
                            BindBaseData(bondsInfo);
                        }
                        else
                        {
                            if (bondWithOutInfoList.ContainsKey(obj.securityid) == false)
                            {
                                bondWithOutInfoList[obj.securityid] = 0;
                            }
                        }

                        // 我的bid， 我的ofr
                        bondInfosItems.SetValue(MY_BID_FIELD, 100000000m);
                        bondInfosItems.SetValue(MY_BID_QTY_FIELD, 0);
                        bondInfosItems.SetValue(MY_OFR_FIELD, 100000000m);
                        bondInfosItems.SetValue(MY_OFR_QTY_FIELD, 0);
                        bondInfosItems.SetValue(BidYieldPrice_FIELD, 100000000m);
                        bondInfosItems.SetValue(BidYieldToExecutionPrice_FIELD, 100000000m);
                        bondInfosItems.SetValue(OfrYieldPrice_FIELD, 100000000m);
                        bondInfosItems.SetValue(OfrYieldToExecutionPrice_FIELD, 100000000m);
                        Instance_OnOrder($"{obj.securityid}#{obj.venues}", "1");
                        Instance_OnOrder($"{obj.securityid}#{obj.venues}", "2");

                        //做市义务债券详情

                        lock (_needAnsMonitorList)
                        {
                            if (_needAnsMonitorList.Contains(obj.securityid) == false)
                            {
                                _needAnsMonitorList.Add(obj.securityid);
                            }
                        }
                        if (_isCollectingAnsMonitor == false)
                        {
                            ThreadPoolHelper.InvokeThreadPool((o) =>
                            {
                                _isCollectingAnsMonitor = true;
                                Thread.Sleep(600);
                                lock (_needAnsMonitorList)
                                {
                                    if (_needAnsMonitorList != null && _needAnsMonitorList.Count > 0)
                                    {
                                        AnsDataHandler.Instance.GetMonitorInfo(_needAnsMonitorList);
                                    }
                                    _needAnsMonitorList.Clear();
                                }
                                _isCollectingAnsMonitor = false;
                            });
                        }

                    }

                    if (bondInfosItems != null)
                    {
                        string settltype = obj.settltype;

                        bondInfosItems.SetValue(ENABLE_FIELD, obj.enable == "1" ? true : false);// 开关
                        if (obj.enable == "1")
                        {
                            if (!startBonds.ContainsKey(obj.securityid))
                            {
                                startBonds[obj.securityid] = obj.enable;
                            }
                        }
                        else
                        {
                            if (startBonds.ContainsKey(obj.securityid))
                            {
                                string removeValue;
                                startBonds.TryRemove(obj.securityid, out removeValue);
                            }
                        }
                        Enable_Unify = startBonds.Count > 0 ? true : false;

                        bondInfosItems.SetValue(LAST_OPERATION_FIELD, obj.remark);
                        bondInfosItems.SetValue(UPDATE_TIME_FIELD, obj.updatetime);
                        bondInfosItems.SetValue(INF1_FIELD, obj.inf1);
                        if (string.IsNullOrEmpty(obj.inf2) == false)
                        {
                            Inf2 inf2 = JsonConvert.DeserializeObject<Inf2>(obj.inf2);
                            bondInfosItems.SetValue(INF2_FIELD, inf2.ofr == true ? "ofr" : "bid");
                        }
                        bondInfosItems.SetValue(INF5_FIELD, obj.inf5);

                        OrderParamProp orderParam = new OrderParamProp(obj.Params);
                        //OrderDetailViewModel.Request2Data(obj.Params, orderParam);
                        bondInfosItems[PARAMS_FIELD] = orderParam;//参数
                        if (obj.Params != null)
                        {

                            bondInfosItems.SetValue(MY_BID_MOTHOD_FIELD, obj.Params.bidQuoteModel);
                            bondInfosItems.SetValue(MY_OFR_MOTHOD_FIELD, obj.Params.ofrQuoteModel);
                            bondInfosItems.SetValue(LAST_OPERATION_TIME_FIELD, obj.Params.lastOptTime);
                            //bondInfosItems.SetValue(BidYieldPrice_FIELD, CommonUtils.ConvertToDecimal(obj.Params.bidYieldPrice));
                            //bondInfosItems.SetValue(OfrYieldPrice_FIELD, CommonUtils.ConvertToDecimal(obj.Params.ofrYieldPrice));
                            //bondInfosItems.SetValue(BidYieldToExecutionPrice_FIELD, CommonUtils.ConvertToDecimal(obj.Params.bidYieldToExecutionPrice));
                            //bondInfosItems.SetValue(OfrYieldToExecutionPrice_FIELD, CommonUtils.ConvertToDecimal(obj.Params.ofrYieldToExecutionPrice));

                            settltype = obj.Params.baseSettleType;
                        }
                        if (obj.Params.maxFloorIsEnable)
                        {
                            bondInfosItems.SetValue(MAXFLOOR_FIELD, obj.Params.maxFloor);
                        }

                        bool isChange = bondInfosItems.SetValue(SETTLE_TYPE_FIELD, settltype);// 清算速度
                        if (isChange)
                        {
                            //订阅price
                            PriceHandler.Instance.RegisterCTPrice(obj.securityid, Instance_OnMDPrice);
                            //Console.WriteLine($"注册1-{obj.securityid}");
                        }
                        bondInfosItems.SetValue(MY_BID_LEVEL_VISIBLE, LocalEnv.ConfInfo.MMPriceLevelVisible);
                        bondInfosItems.SetValue(MY_OFR_LEVEL_VISIBLE, LocalEnv.ConfInfo.MMPriceLevelVisible);
                        bondInfosItems.SetValue(VENUETYPEGW_FIELD, obj.venuetypegw);
                        bondInfosItems.SetValue(USERNAME_FIELD, obj.username);
                    }
                }
            }
            catch (Exception e)
            {
                LogHandler.Error(e);
            }
        }

        // <summary>
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

        private void ShowConfigOrderParamWindow(string securityID, string symbol, string remainingTime)
        {
            //OrderDetailView odView = new OrderDetailView();
            //OrderDetailViewModel odVM = new OrderDetailViewModel(securityID,symbol);

            //odVM.WinTitle = securityID + "  " + symbol + "  " + remainingTime;
            //odVM.MyWindow = odView;
            //odView.DataContext = odVM;
            //odView.ShowDialog();

            //string stringJson = odVM.ReqestJsonString;

        }


        public async void AddBond()
        {
            try
            {
                STC.Mmshe.Plugin.View.HomePage.AddBondWindow addBondWind = new STC.Mmshe.Plugin.View.HomePage.AddBondWindow();
                addBondWind.Owner = Application.Current.MainWindow;
                if (addBondWind.ShowDialog() == false)
                {
                    return;
                }

                OrderDetailView odView = new OrderDetailView();
                odView.Owner = Application.Current.MainWindow;
                OrderParamProp orderParam = new OrderParamProp(null);
                orderParam.Venuetypegw = orderParam.Venues = addBondWind.Venue;
                orderParam = MMsheCommonUtils.OrderParamPropToDefault(orderParam);
                orderParam.securityid = addBondWind.securityID;
                orderParam.symbol = addBondWind.symbol;
                PResult result = MMsheCommonUtils.QueryCirculSizeBySecurity(addBondWind.securityID);
                if (result.IsSuccess())
                {
                    orderParam.circulSize = result.Info1;
                }
                else
                {
                    return;
                }
                BondsInfoPo bondsInfoPo = CommonUtils.GetBondsInfoPoBySecurityID(orderParam.securityid);

                if (bondsInfoPo == null)
                {
                    return;
                }
                if ("BNC".Equals(bondsInfoPo.symbolType))
                {
                    orderParam.IsInterest = "true";
                }
                else
                {
                    orderParam.IsInterest = "false";
                }
                //if (!string.IsNullOrEmpty(bondsInfoPo.Option_Type) &&
                //            ((!bondsInfoPo.Option_Type.Equals("NON") && bondsInfoPo.Option_Date != null) ||
                //            (bondsInfoPo.Option_Type.Equals("ETS") && bondsInfoPo.Option_Date == null)))
                //{
                //    orderParam.wallEnable = "0";
                //    orderParam.CDCValueType = "1";
                //    orderParam.exeBondFollow = "0";
                //}
                //RiskQtyBySecurityReplyPo replyPo = MMsheCommonUtils.QueryRiskQtyBySecurity(addBondWind.securityID);
                //if (replyPo.IsSuccess() && replyPo.Data != null)
                //{
                //    orderParam.riskMinQtySize = replyPo.Data.riskMinQtySize;
                //    orderParam.riskMaxQtyRatio = replyPo.Data.riskMaxQtyRatio;
                //}
                //else
                //{
                //    return;
                //}
                OrderDetailViewModel odVM = new OrderDetailViewModel(orderParam);
                odVM.NeedPrompt = false;
                MMOrderControlModel vm = new MMOrderControlModel()
                {
                    enable = false,
                    securityID = addBondWind.securityID,
                    Params = orderParam
                };
                //DataTable dt = new DataTable();
                //dt.Columns.Add("securityID", typeof(string));
                //dt.Columns.Add("symbol", typeof(string));
                //dt.Columns.Add("Params", typeof(OrderParamProp));
                //DataRow row = dt.NewRow();
                //row["securityID"] = addBondWind.securityID;
                //row["symbol"] = addBondWind.symbol;
                //row["Params"] = orderParam;
                //dt.Rows.Add(row);
                odVM.SelectedItemRow = vm;
                odVM.IsAddBond = true;

                odVM.MyWindow = odView;
                odView.DataContext = odVM;

                if (odView.ShowDialog() == true)
                {
                    MMsheReq updateReq = new MMsheReq();
                    updateReq.Action = "1"; // 添加
                    STC.Common.Model.MMshe.MMshe request = new STC.Common.Model.MMshe.MMshe();
                    request.securityid = addBondWind.securityID;
                    request.venues = odVM.Data.Venues;
                    request.venuetypegw = odVM.Data.Venuetypegw;
                    request.symbol = addBondWind.symbol;
                    request.Action = "1"; // 添加
                    request.remark = $"{addBondWind.securityID}  {addBondWind.symbol}  {addBondWind.remainingTime}";
                    request.terminal = string.Format("STClient_v{0}", LocalEnv.ServerInfo.Version);
                    request.groupid = groupID;
                    request.settltype = odVM.OrderParamReq.baseSettleType;
                    request.Params = odVM.OrderParamReq;
                    request.enable = "0";
                    updateReq.mmList = new List<STC.Common.Model.MMshe.MMshe>();
                    updateReq.mmList.Add(request);
                    PResult pResult = await Utils.Data.MMsheGroupHandler.Instance.AsyncUpdateSecurityids(updateReq);
                    Prompt2Seconds prompt = new Prompt2Seconds();
                    prompt.PromptTitle = "新增债券";
                    prompt.Owner = Application.Current.MainWindow;
                    if (pResult != null && pResult.IsSuccess())
                    {
                        //CustomMessageBox.Info($"新增成功！", "新增债券");
                        prompt.PromptContext = "新增成功";
                    }
                    else
                    {
                        //CustomMessageBox.Warning(pResult == null ? "新增失败！" : pResult.Msg, "新增债券");
                        prompt.PromptContext = pResult == null ? "新增失败！" : pResult.Msg;
                    }
                    prompt.Show();
                }
            }
            catch (Exception e)
            {
                LogHandler.Error(e);
            }
        }

        /// <summary>
        /// 删除行
        /// </summary>
        public async void DelBond()
        {
            if (this.BondView == null || this.BondView.Count == 0)
            {
                return;
            }

            string securityID = CommonUtils.ConvertToString(this.SelectedItem[SECURITY_ID_FIELD]);
            string settlType = CommonUtils.ConvertToString(this.SelectedItem[SETTLE_TYPE_FIELD]);

            MMsheReq updateReq = new MMsheReq();
            updateReq.Action = "3"; // 添加
            STC.Common.Model.MMshe.MMshe request = new STC.Common.Model.MMshe.MMshe();
            request.securityid = securityID;
            request.venues = request.venuetypegw = SelectedGroup.venues;
            request.groupid = groupID;
            request.settltype = settlType;
            updateReq.mmList = new List<STC.Common.Model.MMshe.MMshe>();
            updateReq.mmList.Add(request);
            PResult pResult = await Utils.Data.MMsheGroupHandler.Instance.AsyncUpdateSecurityids(updateReq);
            if (pResult != null && pResult.IsSuccess())
            {
                CustomMessageBox.Info($"{securityID}删除成功", "删除债券");

                if (WindowManager.Instance.IsExist($"{PopupTypeEnum.OrderControl.ToString()}-{securityID}"))
                {
                    WindowManager.Instance.CloseWindow($"{PopupTypeEnum.OrderControl.ToString()}-{securityID}");
                }

                MMsheCommonUtils.ClearSection(securityID);
            }
            else
            {
                CustomMessageBox.Warning(pResult == null ? "删除失败" : pResult.Msg, "删除债券");
            }
        }

        public async void MMUpdateEnable(MMsheUpdateEnableReq req)
        {
            PResult result = null;
            await Task.Run(() =>
            {
                result = MMsheHandler.Instance.mmUpdateEnable(req);
            });
            string promptTitle;
            if ("1" == req.enable)
            {
                promptTitle = $"债券策略[{req.securityid}]报价开启";
            }
            else
            {
                promptTitle = $"债券策略[{req.securityid}]报价关闭";
            }

            if (result == null)
            {
                Prompt2Second(promptTitle, "失败");
            }
            else
            {
                if (result != null && !result.IsSuccess())
                {
                    Prompt2Second(promptTitle, result.Msg);
                }
            }
        }

        private async void UpdateAllEnable(string enable)
        {
            PResult result = null;
            await Task.Run(() =>
            {
                MMsheUpdateAllEnableReq updateAllEnableReq = new MMsheUpdateAllEnableReq();
                updateAllEnableReq.enable = enable;
                updateAllEnableReq.groupid = this.groupID;
                updateAllEnableReq.macID = EngineHelper.GetMacByNetworkInterface();
                updateAllEnableReq.insideIP = EngineHelper.GetIp();
                updateAllEnableReq.hardDiskID = EngineHelper.Getharddiskid2();
                result = MMsheHandler.Instance.mmUpdateAllEnable(updateAllEnableReq);
            });

            Prompt2Seconds prompt = new Prompt2Seconds();
            if ("1" == enable)
            {
                prompt.PromptTitle = "债券策略报价开启";
            }
            else
            {
                prompt.PromptTitle = "债券策略报价关闭";
            }
            prompt.Owner = Application.Current.MainWindow;
            if (result != null && result.IsSuccess())
            {
                prompt.PromptContext = "成功";
            }
            else
            {
                if (result == null)
                {
                    prompt.PromptContext = "失败";
                }
                else
                {
                    prompt.PromptContext = result.Msg;
                }
            }
            prompt.Show();

        }

        /// <summary>
        /// 我的订单回调
        /// </summary>
        /// <param name="obj"></param>
        private void Instance_OnOrder(string key, string side)
        {
            //UpdateMyOrders(obj);
            ThreadPoolHelper.InvokeThreadPool((o) =>
            {
                string securityid = key.Split('#')[0];
                string venue = key.Split('#')[1];
                ExecutionReport er = MyOrderProcesser.Processer.GetLstMMSHEOrderBySecurityID(securityid, venue, side,true);
                _LastOrderDic[$"{securityid}#{venue}#{side}"] = er;
            });
        }

        private bool isUpdatingMdData = false;
        private async void Sub_RefreshMdDataAsync()
        {
            try
            {
                string[] securityIDArr = baseCTPriceDataDic.Keys.ToArray();
                foreach (string key in securityIDArr)
                {
                    if (mainWinList.Count <= 0)
                    {
                        break;
                    }

                    DataView priceBaseData = null;
                    bool flag = baseCTPriceDataViewDic.TryGetValue(key, out priceBaseData);

                    MarketDataSnapshotFullRefresh removeCtPrice = null;
                    bool flag_CTPrice = baseCTPriceDataDic.TryRemove(key, out removeCtPrice);

                    if (flag == true && flag_CTPrice == true)
                    {
                        UpdateBestPrice(priceBaseData, removeCtPrice);
                    }
                    await System.Windows.Threading.Dispatcher.Yield(System.Windows.Threading.DispatcherPriority.Render);
                }
            }
            catch (Exception e)
            {
                LogHelper.Error(e);
            }
            finally
            {

            }

            isUpdatingMdData = false;
        }

        /// <summary>
        /// 根据筛选条件计算最优价格
        /// </summary>
        /// <param name="priceBaseData"></param>
        private async void UpdateBestPrice(DataView priceBaseData, MarketDataSnapshotFullRefresh obj)
        {
            try
            {
                if (priceBaseData == null)
                {
                    LogHelper.Info("if (priceBaseData == null)");
                    return;
                }

                if (obj == null)
                {
                    LogHelper.Info("if (obj == null)");
                    return;
                }

                string securityID = CommonUtils.ConvertToString(obj.SecurityID);
                string currentSettlType = null;
                string venue = null;
                lock (this.lockObj)
                {
                    try
                    {
                        DataRow[] drs = this.BondView.Table.Select($"{SECURITY_ID_FIELD} = '{securityID}'");
                        if (drs != null && drs.Length > 0)
                        {
                            currentSettlType = drs[0][SETTLE_TYPE_FIELD].ToString();
                            venue = drs[0][VENUES_FIELD].ToString();
                        }
                    }
                    catch (Exception e)
                    {
                        LogHandler.Error($"双边做市{securityID}更新最优价格 ", e);
                    }
                }

                // bid最优价格
                BestPrice1 bestBid = null;
                // ofr 最优价格
                BestPrice1 bestOfr = null;
                // cefts bid 最优价格
                BestPrice1 bestCeftsBid = null;
                // cefts ofr 最优价格
                BestPrice1 bestCeftsOfr = null;
                // myBidLevel, myOfrLevel, key: side#quoteid; value:info3 or info4
                Dictionary<string, string> myBidOfrLevelDic = new Dictionary<string, string>();

                await Task.Run(() => {

                    lock (priceBaseData)
                    {
                        // bid最优价格
                        bestBid = CalcBestPrice(priceBaseData, "0", null, currentSettlType);
                        // ofr 最优价格
                        bestOfr = CalcBestPrice(priceBaseData, "1", null, currentSettlType);
                        // cefts bid 最优价格
                        bestCeftsBid = CalcBestPrice(priceBaseData, "0", venue, currentSettlType);
                        // cefts ofr 最优价格
                        bestCeftsOfr = CalcBestPrice(priceBaseData, "1", venue, currentSettlType);
                        // myBidOfrLevel
                        myBidOfrLevelDic = CalcInfo3Info4(priceBaseData);
                    }
                });

                lock (this.lockObj)
                {
                    DataRow[] drs = this.BondView.Table.Select($"{SECURITY_ID_FIELD} = '{securityID}'");
                    if (drs != null && drs.Length > 0)
                    {
                        foreach (DataRow item in drs)
                        {
                            if (item != null)
                            {
                                // 中债估值
                                item.SetValue(VALUATION_TYPE_FIELD, obj.CCDCValuaType);
                                if (!string.IsNullOrEmpty(obj.CCDCValuation2))
                                {
                                    item.SetValue(VALUATION_FIELD, obj.CCDCValuation2);
                                    if (!string.IsNullOrEmpty(obj.CCDCValuaType) && obj.CCDCValuaType.Equals("1"))
                                    {
                                        item.SetValue(VALUATION_EXE_FIELD, obj.CCDCValuation2);
                                    }
                                    string[] value2 = obj.CCDCValuation2.Split('|');
                                    if (value2 != null && value2.Count() > 0)
                                    {
                                        if (!string.IsNullOrEmpty(obj.CCDCValuaType) && "1".Equals(obj.CCDCValuaType))
                                        {
                                            item.SetValue(VALUATION_EXE_FIELD, value2[0]);
                                        }
                                        if (value2.Count() == 2)
                                        {
                                            if (!string.IsNullOrEmpty(value2[1]))
                                            {
                                                item.SetValue(VALUATION_MAT_FIELD, value2[1]);
                                            }
                                            else
                                            {
                                                item.SetValue(VALUATION_MAT_FIELD, value2[0]);
                                            }
                                        }
                                        else
                                        {
                                            item.SetValue(VALUATION_MAT_FIELD, value2[0]);
                                        }
                                    }
                                }

                                checkQuoteID(item, bestCeftsBid, bestCeftsOfr, bestBid, bestOfr);
                                // 更新表格(双边)
                                BindBestCEFTSPriceToUI(item, bestCeftsBid, bestCeftsOfr);
                                // 市场最优
                                BindBestPriceToUI(item, bestBid, bestOfr);
                                // 更新最优价是否和我的订单一样标志
                                BindIsSameTagToUI(securityID, item);

                                // mybidlevel, myofrlevel
                                string bidQuoteID = CommonUtils.ConvertToString(item[BID_QUOTE_ID_FIELD]);
                                string ofrQuoteID = CommonUtils.ConvertToString(item[OFR_QUOTE_ID_FIELD]);
                                if (!string.IsNullOrEmpty(bidQuoteID) && myBidOfrLevelDic.ContainsKey($"0#{bidQuoteID}"))
                                {
                                    item[MY_BID_LEVEL] = myBidOfrLevelDic[$"0#{bidQuoteID}"];
                                }
                                if (!string.IsNullOrEmpty(ofrQuoteID) && myBidOfrLevelDic.ContainsKey($"1#{ofrQuoteID}"))
                                {
                                    item[MY_OFR_LEVEL] = myBidOfrLevelDic[$"1#{ofrQuoteID}"];
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
            }
        }

        private void checkQuoteID(DataRow item, BestPrice1 bidCfetsBP,
            BestPrice1 ofrCfetsBP, BestPrice1 bidBP,
            BestPrice1 ofrBP)
        {
            item.SetValue(CFETS_BID_ENTYRID_FIELD, bidCfetsBP.quoteId);
            item.SetValue(CFETS_OFR_ENTYRID_FIELD, ofrCfetsBP.quoteId);
            item.SetValue(BID_ENTYRID_FIELD, bidBP.quoteId);
            item.SetValue(OFR_ENTYRID_FIELD, ofrBP.quoteId);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="side">0: 买 1:卖</param>
        /// <returns></returns>
        private BestPrice1 CalcBestPrice(DataView priceBaseData, string side, string venuesGW = null, string settltype = null)
        {
            string quoteid = string.Empty;
            string bestPx = string.Empty; 
            string bestNetPx = string.Empty;
            int bestQty = 0;
            string bestT0 = string.Empty;
            string bestYuan = string.Empty;
            string bestPartName = string.Empty;
            string hasFirm = "0";
            string hasNonFirm = "0";

            string venuesGWRowFilter = string.IsNullOrEmpty(venuesGW) ? string.Empty : $" and {CTPriceControler.SOURCE_FIELD}= '{venuesGW}'";
            string settltypeFilter = string.IsNullOrEmpty(settltype) ? string.Empty :
                ("2".Equals(settltype) ? ($" and ({CTPriceControler.SETTL_TYPE_FIELD}= '{settltype}' or {CTPriceControler.SETTL_TYPE_FIELD}= '{3}')") : ($" and {CTPriceControler.SETTL_TYPE_FIELD}= '{settltype}'"));
            //string bidSort = $"{CTPriceControler.SIDE_FIELD},{CTPriceControler.ISINCOMPLETE_FIELD},{CTPriceControler.MD_PRICE_LEVEL_FIELD}";
            //string ofrSort = $"{CTPriceControler.SIDE_FIELD},{CTPriceControler.ISINCOMPLETE_FIELD},{CTPriceControler.MD_PRICE_LEVEL_FIELD}";
            string bidSort = $"{CTPriceControler.SIDE_FIELD},{CTPriceControler.NET_PRICE_FIELD} Desc";
            string ofrSort = $"{CTPriceControler.SIDE_FIELD},{CTPriceControler.NET_PRICE_FIELD}";
            string filter = string.Empty;
            if (side == "0")
            {
                priceBaseData.Sort = bidSort;
                //filter = $"{CTPriceControler.ISINCOMPLETE_FIELD} = '0' and {CTPriceControler.SIDE_FIELD} = '0'{venuesGWRowFilter}{settltypeFilter}";
                filter = $"{CTPriceControler.SIDE_FIELD} = '0'{venuesGWRowFilter}{settltypeFilter}";
                // bid最优价格
                if (this._rowSettlTypeFilterStr.Length > 0)
                {
                    filter = $"{filter} and ({this._rowSettlTypeFilterStr})";
                }
                priceBaseData.RowFilter = filter;
            }
            else if (side == "1")
            {
                priceBaseData.Sort = ofrSort;
                //filter = $"{CTPriceControler.ISINCOMPLETE_FIELD} = '0' and {CTPriceControler.SIDE_FIELD} = '1'{venuesGWRowFilter}{settltypeFilter}";
                filter = $"{CTPriceControler.SIDE_FIELD} = '1'{venuesGWRowFilter}{settltypeFilter}";
                // bid最优价格
                if (this._rowSettlTypeFilterStr.Length > 0)
                {
                    filter = $"{filter} and ({this._rowSettlTypeFilterStr})";
                }
                priceBaseData.RowFilter = filter;
            }

            if (priceBaseData.Count > 0)
            {
                int bestPxCount = 0;
                bestPx = CommonUtils.ConvertToString(priceBaseData[0][CTPriceControler.Yield_Field]);
                bestNetPx = CommonUtils.ConvertToString(priceBaseData[0][CTPriceControler.NET_PRICE_FIELD]);
                decimal bestPx_decimal = CommonUtils.ConvertToDecimal(bestPx);
                if (CommonUtils.IsNumeric(bestPx))
                {
                    foreach (DataRowView drv in priceBaseData)
                    {

                        string px = CommonUtils.ConvertToString(drv[CTPriceControler.Yield_Field]);
                        if (CommonUtils.IsNumeric(px))
                        {
                            decimal px_decimal = CommonUtils.ConvertToDecimal(px);
                            int qty = CommonUtils.ConvertToInt(CommonUtils.ConvertToString(drv[CTPriceControler.QTY_FIELD]));
                            string settlType = CommonUtils.ConvertToString(CommonUtils.ConvertToString(drv[CTPriceControler.SETTL_TYPE_FIELD]));
                            string yuanSettlType = CommonUtils.ConvertToString(CommonUtils.ConvertToString(drv[CTPriceControler.SETTL_TYPE_YUAN_FIELD]));
                            string firmFlag = CommonUtils.ConvertToString(CommonUtils.ConvertToString(drv[CTPriceControler.FIRM_TYPE_FIELD]));
                            if (bestPx_decimal == px_decimal)
                            {
                                bestPxCount++;
                                bestQty += qty;
                                if (settlType.Contains("1"))
                                {
                                    bestT0 = "1";
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
                                quoteid = CommonUtils.ConvertToString(drv[CTPriceControler.KEY_FIELD]);
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
                        string px = CommonUtils.ConvertToString(drv[CTPriceControler.Yield_Field]);
                        if (CommonUtils.IsNumeric(px) == false)
                        {
                            int qty = CommonUtils.ConvertToInt(CommonUtils.ConvertToString(drv[CTPriceControler.QTY_FIELD]));
                            string settlType = CommonUtils.ConvertToString(CommonUtils.ConvertToString(drv[CTPriceControler.SETTL_TYPE_FIELD]));
                            string yuanSettlType = CommonUtils.ConvertToString(CommonUtils.ConvertToString(drv[CTPriceControler.SETTL_TYPE_YUAN_FIELD]));
                            string firmFlag = CommonUtils.ConvertToString(CommonUtils.ConvertToString(drv[CTPriceControler.FIRM_TYPE_FIELD]));
                            bestPxCount++;
                            bestQty += qty;
                            if (settlType.Contains("1"))
                            {
                                bestT0 = "1";
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
                            quoteid = CommonUtils.ConvertToString(drv[CTPriceControler.KEY_FIELD]);
                        }
                        else
                        {
                            break;
                        }
                    }
                }
                if (bestPxCount > 1)
                {
                    bestPartName = "多家";
                }
                else
                {
                    bestPartName = CommonUtils.ConvertToString(priceBaseData[0][CTPriceControler.PARTY_NAME_FIELD]);
                }
            }

            return new BestPrice1()
            {
                bestPx = bestPx,
                bestNetPx = bestNetPx,
                bestQty = bestQty,
                bestT0 = bestT0,
                bestYuan = bestYuan,
                isAllFirm = (hasFirm == "1" && hasNonFirm == "0") ? "1" : "0",
                partyName = bestPartName,
                quoteId = quoteid
            };
        }

        private Dictionary<string, string> CalcInfo3Info4(DataView priceBaseData)
        {
            Dictionary<string, string> levelDic = new Dictionary<string, string>();
            priceBaseData.RowFilter = $"{CTPriceControler.VENUE_TYPE_GW_FIELD}= '{Common.Model.Fix.MessageDictionary.FieldsEnum.VenueTypeGW.QDMESP}'";
            foreach (DataRowView dr in priceBaseData)
            {
                string quoteID = dr[CTPriceControler.KEY_FIELD].ConvertToString();

                if (!string.IsNullOrEmpty(quoteID))
                {
                    string info3 = dr[CTPriceControler.INFO3_FIELD].ConvertToString();
                    levelDic[$"0#{quoteID}"] = info3;

                    string info4 = dr[CTPriceControler.INFO4_FIELD].ConvertToString();
                    levelDic[$"1#{quoteID}"] = info4;
                }
            }
            return levelDic;
        }

        private void CalcMDData()
        {
            while (!_CalcMDDataThreadDispose)
            {
                try
                {

                    if (_LastCTPriceDic != null && _LastCTPriceDic.Count > 0)
                    {
                        KeyValuePair<string, MarketDataSnapshotFullRefresh>[] lastMdPriceArr;
                        lock (_LastCTPriceDic)
                        {
                            lastMdPriceArr = this._LastCTPriceDic.ToArray();
                            this._LastCTPriceDic.Clear();
                        }

                        foreach (KeyValuePair<string, MarketDataSnapshotFullRefresh> MdPricePair in lastMdPriceArr)
                        {
                            //Console.WriteLine($"更新{MdPricePair.Key}");
                            if (baseCTPriceDataViewDic.ContainsKey(MdPricePair.Key) == false)
                            {
                                baseCTPriceDataViewDic[MdPricePair.Key] = CreateBaseDataView();
                            }

                            DataView priceBaseData = null;
                            bool flag = baseCTPriceDataViewDic.TryGetValue(MdPricePair.Key, out priceBaseData);
                            if (flag == true && priceBaseData != null)
                            {
                                baseCTPriceDataDic[MdPricePair.Key] = MdPricePair.Value;
                                lock (priceBaseData)
                                {
                                    CTPriceControler.UpdateDataToBaseDV(priceBaseData, MdPricePair.Value);
                                }
                            }
                        }
                    }

                    Thread.Sleep(200);

                }
                catch (Exception ex)
                {
                    LogHandler.Error(ex);
                }
            }
        }

        private bool isUpdatingMyOrders = false;
        private async void Sub_RefreshMyOrdersAsync()
        {
            try
            {
                string[] securityIDArr = _LastOrderDic.Keys.ToArray();

                foreach (string key in securityIDArr)
                {
                    if (mainWinList.Count <= 0)
                    {
                        break;
                    }

                    UpdateMyOrders(key);
                    await System.Windows.Threading.Dispatcher.Yield(System.Windows.Threading.DispatcherPriority.Render);
                }
            }
            catch (Exception e)
            {
                LogHelper.Error(e);
            }
            finally
            {
                isUpdatingMyOrders = false;
            }
        }

        private string getVenue(string securityIDAndSide)
        {
            string result = "";
            int firstIndex = securityIDAndSide.IndexOf('#');
            if (firstIndex != -1)
            {
                int secondIndex = securityIDAndSide.IndexOf('#', firstIndex + 1);
                if (secondIndex != -1)
                {
                    result = securityIDAndSide.Substring(firstIndex + 1, secondIndex - firstIndex - 1);                   
                }
            }
            return result;
        }

        /// <summary>
        /// 更新我的订单
        /// </summary>
        /// <param name="obj"></param>
        private void UpdateMyOrders(string securityIDAndSide)
        {
            try
            {
                lock (this.lockObj)
                {
                    string securityID = securityIDAndSide.Substring(0, securityIDAndSide.IndexOf("#"));
                    string venue = getVenue(securityIDAndSide);
                    DataRow bondInfosItems = this.BondView.Table.Rows.Find(new object[] { securityID});
                    if (bondInfosItems != null && !venue.Equals(bondInfosItems[VENUES_FIELD] + "")) 
                    {
                        return;
                    }
                    if (bondInfosItems != null)
                    {
                        // 更新我的bid和我的ofr和价差
                        ExecutionReport bidOrder = null;
                        ExecutionReport ofrOrder = null;
                        if (_LastOrderDic.ContainsKey(securityIDAndSide) && securityIDAndSide.EndsWith("1"))
                        {
                            _LastOrderDic.TryRemove(securityIDAndSide, out bidOrder);
                            BindMyBidOrders(bondInfosItems, bidOrder);
                        }
                        if (_LastOrderDic.ContainsKey(securityIDAndSide) && securityIDAndSide.EndsWith("2"))
                        {
                            _LastOrderDic.TryRemove(securityIDAndSide, out ofrOrder);
                            BindMyOfrOrders(bondInfosItems, ofrOrder);
                        }
                        //价差
                        if (bondInfosItems != null)
                        {
                            BindMyPriceSpreadOrders(bondInfosItems);
                        }

                        // 更新最优价是否和我的订单一样标志
                        BindIsSameTagToUI(securityID, bondInfosItems);
                    }
                    else
                    {
                        // 更新我的bid和我的ofr
                        ExecutionReport bidOrder = null;
                        ExecutionReport ofrOrder = null;
                        if (_LastOrderDic.ContainsKey(securityIDAndSide) && securityIDAndSide.EndsWith("1"))
                        {
                            _LastOrderDic.TryRemove(securityIDAndSide, out bidOrder);
                        }
                        if (_LastOrderDic.ContainsKey(securityIDAndSide) && securityIDAndSide.EndsWith("2"))
                        {
                            _LastOrderDic.TryRemove(securityIDAndSide, out ofrOrder);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                LogHandler.Error(e);
            }
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
            #region ofr
            if (ofrBP != null)
            {
                // 最优ofr price
                if (CommonUtils.IsNumeric(ofrBP.bestPx))
                {
                    decimal bestOfrPx = CommonUtils.ConvertToDecimal(ofrBP.bestPx);
                    item.SetValue(BEST_OFR_FIELD, bestOfrPx);
                    item.SetValue(BEST_OFRNETPX_FIELD, CommonUtils.ConvertToDecimal(ofrBP.bestNetPx));
                }
                else
                {
                    item.SetValue(BEST_OFR_FIELD, 100000000m);
                }


                // 最优ofr qty
                item.SetValue(BEST_OFR_QTY_FIELD, ofrBP.bestQty);
            }
            #endregion

            #region bid
            if (bidBP != null)
            {
                // 最优bid price
                if (CommonUtils.IsNumeric(bidBP.bestPx))
                {
                    decimal bestBidPx = CommonUtils.ConvertToDecimal(bidBP.bestPx);
                    item.SetValue(BEST_BID_FIELD, bestBidPx);
                    item.SetValue(BEST_BIDNETPX_FIELD, CommonUtils.ConvertToDecimal(bidBP.bestNetPx));
                }
                else
                {
                    item.SetValue(BEST_BID_FIELD, 100000000m);
                }


                // 最优bid qty
                item.SetValue(BEST_BID_QTY_FIELD, bidBP.bestQty);
            }
            #endregion

            // 倒挂
            if (bidBP != null && ofrBP != null
               && CommonUtils.IsNumeric(bidBP.bestPx)
               && CommonUtils.IsNumeric(ofrBP.bestPx)
               && CommonUtils.ConvertToDecimal(bidBP.bestPx) <= CommonUtils.ConvertToDecimal(ofrBP.bestPx))
            {
                item.SetValue(BEST_ISRESVERSE, "1");
            }
            else
            {
                item.SetValue(BEST_ISRESVERSE, "0");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <param name="bidBP"></param>
        /// <param name="ofrBP"></param>
        private void BindBestCEFTSPriceToUI(DataRow item,
            BestPrice1 bidBP,
            BestPrice1 ofrBP)
        {
            #region ofr
            if (ofrBP != null)
            {
                item.SetValue(BEST_CFETS_OFR_PARTYNAME, ofrBP.partyName);
                // 最优ofr price
                if (CommonUtils.IsNumeric(ofrBP.bestPx))
                {
                    decimal bestOfrPx = CommonUtils.ConvertToDecimal(ofrBP.bestPx);
                    item.SetValue(BEST_CFETS_OFR_FIELD, bestOfrPx);
                    item.SetValue(BEST_CFETS_OFRNETPX_FIELD, CommonUtils.ConvertToDecimal(ofrBP.bestNetPx));
                }
                else
                {
                    item.SetValue(BEST_CFETS_OFR_FIELD, 100000000m);
                }


                // 最优ofr qty
                item.SetValue(BEST_CFETS_OFR_QTY_FIELD, ofrBP.bestQty);
            }
            #endregion

            #region bid
            if (bidBP != null)
            {
                item.SetValue(BEST_CFETS_BID_PARTYNAME, bidBP.partyName);
                // 最优bid price
                if (CommonUtils.IsNumeric(bidBP.bestPx))
                {
                    decimal bestBidPx = CommonUtils.ConvertToDecimal(bidBP.bestPx);
                    item.SetValue(BEST_CFETS_BID_FIELD, bestBidPx);
                    item.SetValue(BEST_CFETS_BIDNETPX_FIELD, CommonUtils.ConvertToDecimal(bidBP.bestNetPx));
                }
                else
                {
                    item.SetValue(BEST_CFETS_BID_FIELD, 100000000m);
                }


                // 最优bid qty
                item.SetValue(BEST_CFETS_BID_QTY_FIELD, bidBP.bestQty);
            }
            #endregion

            // 倒挂
            if (bidBP != null && ofrBP != null
               && CommonUtils.IsNumeric(bidBP.bestPx)
               && CommonUtils.IsNumeric(ofrBP.bestPx)
               && CommonUtils.ConvertToDecimal(bidBP.bestPx) <= CommonUtils.ConvertToDecimal(ofrBP.bestPx))
            {
                item.SetValue(BEST_CFETS_ISRESVERSE, "1");
            }
            else
            {
                item.SetValue(BEST_CFETS_ISRESVERSE, "0");
            }
        }

        private void BindMyBidOrders(DataRow item, ExecutionReport bidOrder)
        {
            // 我的Bid订单
            if (bidOrder != null)
            {
                string myLstBidPrice = CommonUtils.ConvertToString(bidOrder.Price);
                decimal myBidOrderPrice = CommonUtils.ConvertToDecimal(myLstBidPrice);
                item.SetValue(MY_BID_FIELD, myBidOrderPrice);
                item.SetValue(BidYieldPrice_FIELD, string.IsNullOrEmpty(bidOrder.Yield) ? 100000000m : CommonUtils.ConvertToDecimal(bidOrder.Yield));
                if ("7".Equals(bidOrder.PriceType))
                {
                    item.SetValue(BidYieldToExecutionPrice_FIELD, string.IsNullOrEmpty(bidOrder.YieldToExecution) ? 100000000m : CommonUtils.ConvertToDecimal(bidOrder.YieldToExecution));

                }
                else
                {
                    BondsInfoPo bondsInfoPo = CommonUtils.GetBondsInfoPoBySecurityID(bidOrder.SecurityID);
                    if (bondsInfoPo != null && !string.IsNullOrEmpty(bondsInfoPo.Option_Type) &&
                                ((!bondsInfoPo.Option_Type.Equals("NON") && bondsInfoPo.Option_Date != null) ||
                                (bondsInfoPo.Option_Type.Equals("ETS") && bondsInfoPo.Option_Date == null)))
                    {
                        item.SetValue(BidYieldToExecutionPrice_FIELD, string.IsNullOrEmpty(bidOrder.YieldToExecution) ? 100000000m : CommonUtils.ConvertToDecimal(bidOrder.YieldToExecution));
                    }
                }
                string myLstBidQty = CommonUtils.ConvertToString(bidOrder.LeavesQty);
                int myBidOrderQty = CommonUtils.ConvertToInt(myLstBidQty);
                item.SetValue(MY_BID_QTY_FIELD, myBidOrderQty);
                item.SetValue(BID_QUOTE_ID_FIELD, bidOrder.QuoteID + "0");
                if (!string.IsNullOrEmpty(bidOrder.MaxFloor))
                {
                    item.SetValue(MAXFLOOR_FIELD, CommonUtils.ConvertToInt(CommonUtils.ConvertToString(bidOrder.MaxFloor)));
                }
                else { item.SetValue(MAXFLOOR_FIELD, ""); }
            }
            else
            {
                item.SetValue(MY_BID_FIELD, 100000000m);
                item.SetValue(MY_BID_QTY_FIELD, 0);
                item.SetValue(MAXFLOOR_FIELD, "");
                item.SetValue(BidYieldPrice_FIELD, 100000000m);
                item.SetValue(BidYieldToExecutionPrice_FIELD, 100000000m);
            }
        }

        private void BindMyPriceSpreadOrders(DataRow item)
        {
            if (!item[MY_BID_FIELD].ToString().Equals("100000000") && !item[MY_OFR_FIELD].ToString().Equals("100000000"))
            {
                decimal price1 = CommonUtils.ConvertToDecimal(item[MY_BID_FIELD].ToString());
                decimal price2 = CommonUtils.ConvertToDecimal(item[MY_OFR_FIELD].ToString());
                item.SetValue(MY_PRICE_SPREAD, (price2 - price1));
            }
            else
            {
                item.SetValue(MY_PRICE_SPREAD, 100000000m);
            }

        }
        private void BindMyOfrOrders(DataRow item, ExecutionReport ofrOrder)
        {
            // 我的Ofr订单
            if (ofrOrder != null)
            {
                string myLstOfrPrice = CommonUtils.ConvertToString(ofrOrder.Price);
                decimal myOfrOrderPrice = CommonUtils.ConvertToDecimal(myLstOfrPrice);
                item.SetValue(MY_OFR_FIELD, myOfrOrderPrice);
                item.SetValue(OfrYieldPrice_FIELD, string.IsNullOrEmpty(ofrOrder.Yield) ? 100000000m : CommonUtils.ConvertToDecimal(ofrOrder.Yield));
                if ("7".Equals(ofrOrder.PriceType))
                {
                    item.SetValue(OfrYieldToExecutionPrice_FIELD, string.IsNullOrEmpty(ofrOrder.YieldToExecution) ? 100000000m : CommonUtils.ConvertToDecimal(ofrOrder.YieldToExecution));
                }
                else
                {
                    BondsInfoPo bondsInfoPo = CommonUtils.GetBondsInfoPoBySecurityID(ofrOrder.SecurityID);
                    if (bondsInfoPo != null && !string.IsNullOrEmpty(bondsInfoPo.Option_Type) &&
                                ((!bondsInfoPo.Option_Type.Equals("NON") && bondsInfoPo.Option_Date != null) ||
                                (bondsInfoPo.Option_Type.Equals("ETS") && bondsInfoPo.Option_Date == null)))
                    {
                        item.SetValue(OfrYieldToExecutionPrice_FIELD, string.IsNullOrEmpty(ofrOrder.YieldToExecution) ? 100000000m : CommonUtils.ConvertToDecimal(ofrOrder.YieldToExecution));
                    }
                }
                string myLstOfrQty = CommonUtils.ConvertToString(ofrOrder.LeavesQty);
                int myOfrOrderQty = CommonUtils.ConvertToInt(myLstOfrQty);
                item.SetValue(MY_OFR_QTY_FIELD, myOfrOrderQty);
                item.SetValue(OFR_QUOTE_ID_FIELD, ofrOrder.QuoteID + "1");
            }
            else
            {
                item.SetValue(MY_OFR_FIELD, 100000000m);
                item.SetValue(MY_OFR_QTY_FIELD, 0);
                item.SetValue(OfrYieldPrice_FIELD, 100000000m);
                item.SetValue(OfrYieldToExecutionPrice_FIELD, 100000000m);
            }
        }

        /// <summary>
        /// 更新最优价格是否和我的订单价格一样标志
        /// </summary>
        /// <param name="securityID"></param>
        /// <param name="item"></param>
        private void BindIsSameTagToUI(string securityID, DataRow item)
        {
            item.SetValue(IS_MY_BID_DIFFPX_FIELD, "0");
            item.SetValue(IS_MY_OFR_DIFFPX_FIELD, "0");
            // 最优价格 我的订单 比较， 是否相同
            string myLstBidPrice = CommonUtils.ConvertToString(item[MY_BID_FIELD]);
            decimal myBidOrderPrice = CommonUtils.ConvertToDecimal(myLstBidPrice);
            string bidquoteID = CommonUtils.ConvertToString(item[BID_QUOTE_ID_FIELD]);
            string ofrquoteID = CommonUtils.ConvertToString(item[OFR_QUOTE_ID_FIELD]);
            if (myBidOrderPrice != 100000000m)
            {
                // bid
                decimal bestBidPrice = CommonUtils.ConvertToDecimal(item[BEST_BID_FIELD].ToString());
                decimal ceftsBidPrice = CommonUtils.ConvertToDecimal(item[BEST_CFETS_BID_FIELD].ToString());
                string bidID = CommonUtils.ConvertToString(item[BID_ENTYRID_FIELD]);
                string bidCfetsQuoteID = CommonUtils.ConvertToString(item[CFETS_BID_ENTYRID_FIELD]);
                if (bidquoteID.Equals(bidCfetsQuoteID) && myBidOrderPrice.CompareTo(ceftsBidPrice) != 0)
                {
                    item.SetValue(IS_MY_BID_DIFFPX_FIELD, "1");
                }
                else
                {
                    item.SetValue(IS_MY_BID_DIFFPX_FIELD, "0");
                }

                if (CommonUtils.IsNumeric(item[BEST_BID_FIELD].ToString())
                    && CommonUtils.IsNumeric(myLstBidPrice))
                {
                    // (市场最优)
                    if (bestBidPrice == myBidOrderPrice || bidquoteID.Equals(bidID))
                    {
                        item.SetValue(BEST_BID_SAME_FIELD, "1");
                    }
                    else
                    {
                        item.SetValue(BEST_BID_SAME_FIELD, "0");
                    }

                    //高亮提醒
                    string securityType = CommonUtils.ConvertToString(item[GOODS_TYPE_FIELD]);
                    //判断是bnc还是bco
                    if (CommonUtils.IsRate(securityType))
                    {
                        if (string.IsNullOrEmpty(MainViewModel.BncMoniValue))
                        {
                            item.SetValue(MY_BID_HIGHLIGHT_FIELD, "0");
                        }
                        else
                        {
                            decimal moniValue = CommonUtils.ConvertToDecimal(MainViewModel.BncMoniValue);
                            if (Math.Abs(bestBidPrice - myBidOrderPrice) <= moniValue * 0.01m)
                            {
                                item.SetValue(MY_BID_HIGHLIGHT_FIELD, "1");
                            }
                            else
                            {
                                item.SetValue(MY_BID_HIGHLIGHT_FIELD, "0");
                            }
                        }

                    }
                    else if (securityType == Consts.BCO)
                    {
                        if (string.IsNullOrEmpty(MainViewModel.BcoMoniValue))
                        {
                            item.SetValue(MY_BID_HIGHLIGHT_FIELD, "0");
                        }
                        else
                        {
                            decimal moniValue = CommonUtils.ConvertToDecimal(MainViewModel.BcoMoniValue);
                            if (Math.Abs(bestBidPrice - myBidOrderPrice) <= moniValue * 0.01m)
                            {
                                item.SetValue(MY_BID_HIGHLIGHT_FIELD, "1");
                            }
                            else
                            {
                                item.SetValue(MY_BID_HIGHLIGHT_FIELD, "0");
                            }
                        }
                    }
                    else
                    {
                        item.SetValue(MY_BID_HIGHLIGHT_FIELD, "0");
                    }
                }
                else
                {
                    item.SetValue(BEST_BID_SAME_FIELD, "0");
                    item.SetValue(MY_BID_HIGHLIGHT_FIELD, "0");
                }

                // (双边最优)
                if ((ceftsBidPrice == myBidOrderPrice || bidquoteID.Equals(bidCfetsQuoteID))
                    && CommonUtils.IsNumeric(item[BEST_CFETS_BID_FIELD].ToString())
                    && CommonUtils.IsNumeric(myLstBidPrice))
                {
                    item.SetValue(BEST_CFETS_BID_SAME_FIELD, "1");
                }
                else
                {
                    item.SetValue(BEST_CFETS_BID_SAME_FIELD, "0");
                }
            }
            else
            {
                item.SetValue(BEST_BID_SAME_FIELD, "0");
                item.SetValue(BEST_CFETS_BID_SAME_FIELD, "0");
                item.SetValue(MY_BID_HIGHLIGHT_FIELD, "0");
            }

            string myLstOfrPrice = CommonUtils.ConvertToString(item[MY_OFR_FIELD]);
            decimal myOfrOrderPrice = CommonUtils.ConvertToDecimal(myLstOfrPrice);
            if (myOfrOrderPrice != 100000000m)
            {
                // ofr
                decimal bestOfrPrice = CommonUtils.ConvertToDecimal(item[BEST_OFR_FIELD].ToString());
                decimal ceftsOfrPrice = CommonUtils.ConvertToDecimal(item[BEST_CFETS_OFR_FIELD].ToString());
                string ofrID = CommonUtils.ConvertToString(item[OFR_ENTYRID_FIELD]);
                string ofrCfetsQuoteID = CommonUtils.ConvertToString(item[CFETS_OFR_ENTYRID_FIELD]);
                if (ofrquoteID.Equals(ofrCfetsQuoteID) && myOfrOrderPrice.CompareTo(ceftsOfrPrice) != 0)
                {
                    item.SetValue(IS_MY_OFR_DIFFPX_FIELD, "1");
                }
                else
                {
                    item.SetValue(IS_MY_OFR_DIFFPX_FIELD, "0");
                }
                if (CommonUtils.IsNumeric(item[BEST_OFR_FIELD].ToString())
                    && CommonUtils.IsNumeric(myLstOfrPrice))
                {
                    // (市场最优)
                    if (bestOfrPrice == myOfrOrderPrice || ofrquoteID.Equals(ofrID))
                    {
                        item.SetValue(BEST_OFR_SAME_FIELD, "1");
                    }
                    else
                    {
                        item.SetValue(BEST_OFR_SAME_FIELD, "0");
                    }

                    //高亮提醒
                    string securityType = CommonUtils.ConvertToString(item[GOODS_TYPE_FIELD]);
                    //判断是bnc还是bco
                    if (CommonUtils.IsRate(securityType))
                    {
                        if (string.IsNullOrEmpty(MainViewModel.BncMoniValue))
                        {
                            item.SetValue(MY_OFR_HIGHLIGHT_FIELD, "0");
                        }
                        else
                        {
                            decimal moniValue = CommonUtils.ConvertToDecimal(MainViewModel.BncMoniValue);
                            if (Math.Abs(bestOfrPrice - myOfrOrderPrice) <= moniValue * 0.01m)
                            {
                                item.SetValue(MY_OFR_HIGHLIGHT_FIELD, "1");
                            }
                            else
                            {
                                item.SetValue(MY_OFR_HIGHLIGHT_FIELD, "0");
                            }
                        }

                    }
                    else if (securityType == Consts.BCO)
                    {
                        if (string.IsNullOrEmpty(MainViewModel.BcoMoniValue))
                        {
                            item.SetValue(MY_OFR_HIGHLIGHT_FIELD, "0");
                        }
                        else
                        {
                            decimal moniValue = CommonUtils.ConvertToDecimal(MainViewModel.BcoMoniValue);
                            if (Math.Abs(bestOfrPrice - myOfrOrderPrice) <= moniValue * 0.01m)
                            {
                                item.SetValue(MY_OFR_HIGHLIGHT_FIELD, "1");
                            }
                            else
                            {
                                item.SetValue(MY_OFR_HIGHLIGHT_FIELD, "0");
                            }
                        }
                    }
                    else
                    {
                        item.SetValue(MY_OFR_HIGHLIGHT_FIELD, "0");
                    }
                }
                else
                {
                    item.SetValue(BEST_OFR_SAME_FIELD, "0");
                    item.SetValue(MY_OFR_HIGHLIGHT_FIELD, "0");
                }

                // (双边最优)
                if ((ceftsOfrPrice == myOfrOrderPrice || ofrquoteID.Equals(ofrCfetsQuoteID))
                    && CommonUtils.IsNumeric(item[BEST_CFETS_OFR_FIELD].ToString())
                    && CommonUtils.IsNumeric(myLstOfrPrice))
                {
                    item.SetValue(BEST_CFETS_OFR_SAME_FIELD, "1");
                }
                else
                {
                    item.SetValue(BEST_CFETS_OFR_SAME_FIELD, "0");
                }
            }
            else
            {
                item.SetValue(BEST_OFR_SAME_FIELD, "0");
                item.SetValue(BEST_CFETS_OFR_SAME_FIELD, "0");
                item.SetValue(MY_OFR_HIGHLIGHT_FIELD, "0");
            }


        }


        private void Prompt2Second(string title, string text)
        {
            if (Application.Current.Dispatcher.CheckAccess())
            {
                Sub_Prompt2Second(title, text);
            }
            else
            {
                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    Sub_Prompt2Second(title, text);
                }));
            }
        }

        private void Sub_Prompt2Second(string title, string text)
        {
            Prompt2Seconds prompt = new Prompt2Seconds();
            prompt.PromptTitle = title;
            prompt.PromptContext = text;
            prompt.Owner = Application.Current.MainWindow;
            prompt.Show();
        }

        #region 整组

        private bool _IsUnify = false;

        public bool IsUnify
        {
            get { return _IsUnify; }
            set
            {
                _IsUnify = value;
                this.OnPropertyChanged();
            }
        }

        private MMsheOrderParamReq _mmOrderParamReq;

        public MMsheOrderParamReq MMsheOrderParamReq
        {
            get { return _mmOrderParamReq; }
            set { _mmOrderParamReq = value; }
        }


        private void ProcessInf1()
        {
            MMsheGroupInfo gi = MMsheGroupHandler.Instance.GetGroupInfoByGroupID(this.groupID);
            if (gi.inf1 == MMsheConstants.MM_GROPE_TYPE_UNIFY)
            {
                IsUnify = true;

                if (gi.inf2 != null)
                {
                    MMsheOrderParamReq = JsonConvert.DeserializeObject<MMsheOrderParamReq>(gi.inf2);
                }
                else
                {
                    MMsheOrderParamReq = new MMsheOrderParamReq();
                    MMsheOrderParamReq.InitData();
                }
                _LastModeType = ModeType = string.IsNullOrEmpty(MMsheOrderParamReq.bidQuoteModel) ? "Defend" : MMsheOrderParamReq.bidQuoteModel;
            }
        }

        internal string _LastModeType;
        private string _ModeType;

        public string ModeType
        {
            get { return _ModeType; }
            set
            {
                if (_ModeType != value)
                {
                    _ModeType = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private Dictionary<string, string> _ModeTypeList = new Dictionary<string, string>();

        public Dictionary<string, string> ModeTypeList
        {
            get
            {
                if (_ModeTypeList.Count == 0)
                {
                    _ModeTypeList["Defend"] = "跟随";
                    //_ModeTypeList["Algorithm"] = "算法";
                }

                return _ModeTypeList;
            }
            set { _ModeTypeList = value; }
        }

        internal bool isUpdateModeType()
        {
            return _LastModeType != this.ModeType;
        }

        internal async void UpdateInf2()
        {
            // 更新组策略类型
            MMsheGroupInfo gi = MMsheGroupHandler.Instance.GetGroupInfoByGroupID(this.groupID);
            MMsheGroupReq request = new MMsheGroupReq();
            request.id = gi.id;//组名
            request.groupname = gi.groupname;//组名
            request.action = "2";//更新
            request.ordersn = gi.ordersn.ToString();//顺序
            request.terminal = LocalEnv.ServerInfo.Version; // 终端
            request.inf1 = gi.inf1;

            MMsheOrderParamReq param = null;
            if (gi.inf2 != null)
            {
                param = JsonConvert.DeserializeObject<MMsheOrderParamReq>(gi.inf2);
            }
            else
            {
                param = new MMsheOrderParamReq();
                param.InitData();
            }

            if (param != null)
            {
                param.bidQuoteModel = this.ModeType;
                param.ofrQuoteModel = this.ModeType;
                request.inf2 = JsonConvert.SerializeObject(param);
            }

            PResult pResult = await Utils.Data.MMsheGroupHandler.Instance.AsyncUpdateGroups(request, true);
            if (pResult == null || !pResult.IsSuccess())
            {
                this.ModeType = _LastModeType;
                string errerMsg = pResult != null ? pResult.Msg : "失败";
                CommonUtils.Prompt2Second("切换整组策略类型失败", errerMsg);

            }
            else
            {
                _LastModeType = this.ModeType;
            }
        }

        internal void OpenGroupDetailView()
        {
            GroupDetailView odView = new GroupDetailView();
            odView.Owner = Application.Current.MainWindow;
            MMsheGroupInfo gi = MMsheGroupHandler.Instance.GetGroupInfoByGroupID(this.groupID);
            //OrderParamProp orderParam = new OrderParamProp(null);
            //orderParam = MMsheCommonUtils.OrderParamPropToDefault(orderParam);
            //orderParam.bidQuoteModel = orderParam.ofrQuoteModel = "Defend";
            // ---组不要了
            //orderParam.securityid = null;
            //orderParam.symbol = addBondWind.symbol;
            //PResult result = MMsheCommonUtils.QueryCirculSizeBySecurity(addBondWind.securityID);
            //if (result.IsSuccess())
            //{
            //    orderParam.circulSize = result.Info1;
            //}
            //else
            //{
            //    return;
            //}
            // ---组不要了
            MMsheOrderParamReq param = null;
            try
            {
                if (string.IsNullOrEmpty(gi.inf2) == false)
                {
                    param = JsonConvert.DeserializeObject<MMsheOrderParamReq>(gi.inf2);
                }
            }
            catch
            {
                param = new MMsheOrderParamReq();
                param.bidQuoteModel = param.ofrQuoteModel = "Defend";
                param.InitData();
                param.dealClose = "0";
                param.venues = param.venuetypegw = SelectedGroup.venues;
            }


            OrderParamProp orderParam = new OrderParamProp(param);

            GroupDetailViewModel odVM = new GroupDetailViewModel(orderParam);
            odVM.WinTitle = gi.groupname;
            odVM.NeedPrompt = false;
            odVM.IsAddBond = true;

            odVM.MyWindow = odView;
            odView.DataContext = odVM;
            if (odView.ShowDialog() == true)
            {
                odVM.OrderParamReq.ofrQuoteModel = odVM.OrderParamReq.bidQuoteModel;
                UpdateInf1(odVM.OrderParamReq);
            }
        }

        internal async void UpdateInf1(MMsheOrderParamReq param)
        {
            // 更新组策略类型
            MMsheGroupInfo gi = MMsheGroupHandler.Instance.GetGroupInfoByGroupID(this.groupID);
            MMsheGroupReq request = new MMsheGroupReq();
            request.id = gi.id;//组名
            request.groupname = gi.groupname;//组名
            request.action = "2";//更新
            request.ordersn = gi.ordersn.ToString();//顺序
            request.terminal = LocalEnv.ServerInfo.Version; // 终端
            request.inf1 = gi.inf1;
            request.inf2 = JsonConvert.SerializeObject(param);

            PResult pResult = await Utils.Data.MMsheGroupHandler.Instance.AsyncUpdateGroups(request, true);
            if (pResult == null || !pResult.IsSuccess())
            {
                this.ModeType = _LastModeType;
                string errerMsg = pResult != null ? pResult.Msg : "失败";
                CommonUtils.Prompt2Second("组策略配置", errerMsg);

            }
            else
            {
                this.ModeType = _LastModeType = param.bidQuoteModel;
            }
        }
        #endregion


        #region 
        private int _SettlTypeIndex;

        public int SettlTypeIndex
        {
            get { return _SettlTypeIndex; }
            set
            {
                _SettlTypeIndex = value;
                this.OnPropertyChanged();
            }
        }
        public int LastSettlTypeIndex;
        private string _bidQuoteModel;

        public string BidQuoteModel
        {
            get { return _bidQuoteModel; }
            set
            {
                _bidQuoteModel = value;
                this.OnPropertyChanged();
            }
        }

        private string _ofrQuoteModel;

        public string OfrQuoteModel
        {
            get { return _ofrQuoteModel; }
            set
            {
                _ofrQuoteModel = value;
                this.OnPropertyChanged();
            }
        }

        //自定义
        private Dictionary<string, string> _bidStrategyModeList = new Dictionary<string, string>();
        public Dictionary<string, string> bidStrategyModeList
        {
            get
            {
                _bidStrategyModeList.Clear();

                //if (!string.IsNullOrEmpty(bidBasePrice))
                {
                    //decimal value;
                    //if (!decimal.TryParse(manualBid, out value)) value = 0;
                    _bidStrategyModeList.Add("Manual", "手动");
                }

                //if (!string.IsNullOrEmpty(bidDefenCalPrice))
                {
                    //decimal value;
                    //if (!decimal.TryParse(bidDefenCalPrice, out value)) value = 0;
                    _bidStrategyModeList.Add("Defend", "跟随");
                }

                //if (!string.IsNullOrEmpty(bidAlgoCalPrice))
                /*{
                    _bidStrategyModeList.Add("Algorithm", "算法");
                }

                if ("1".Equals(LocalEnv.ConfInfo.openMMDHideSet))
                {
                    //decimal value = 0;
                    //decimal qty = 0;
                    //if (!decimal.TryParse(apiBid, out value)) value = 0;
                    //if (!decimal.TryParse(apiBidQty, out qty)) qty = 0;
                    _bidStrategyModeList.Add("API", "接口 ");
                }*/

                return _bidStrategyModeList;
            }
        }

        //自定义
        private Dictionary<string, string> _ofrStrategyModeList = new Dictionary<string, string>();
        public Dictionary<string, string> ofrStrategyModeList
        {
            get
            {
                _ofrStrategyModeList.Clear();

                //if (!string.IsNullOrEmpty(ofrBasePrice))
                {
                    //decimal value;
                    //if (!decimal.TryParse(manualOfr, out value)) value = 0;
                    _ofrStrategyModeList.Add("Manual", "手动");
                }

                //if (!string.IsNullOrEmpty(ofrDefenCalPrice))
                {
                    //decimal value;
                    //if (!decimal.TryParse(ofrDefenCalPrice, out value)) value = 0;
                    _ofrStrategyModeList.Add("Defend", "跟随");
                }

                //if (!string.IsNullOrEmpty(ofrAlgoCalPrice))
               /* {
                    //decimal value = 0;
                    //if (!decimal.TryParse(ofrAlgoCalPrice, out value)) value = 0;
                    _ofrStrategyModeList.Add("Algorithm", "算法");
                }
                if ("1".Equals(LocalEnv.ConfInfo.openMMDHideSet))
                {
                    //decimal value = 0;
                    //decimal qty = 0;
                    //if (!decimal.TryParse(apiOfr, out value)) value = 0;
                    //if (!decimal.TryParse(apiOfrQty, out qty)) qty = 0;
                    _ofrStrategyModeList.Add("API", "接口");
                }*/
                return _ofrStrategyModeList;
            }
        }

        private List<QuoteModel> _quoteModelList = new List<QuoteModel>();
        public List<QuoteModel> quoteModelList
        {
            get
            {
                if (_quoteModelList.Count == 0)
                {
                    _quoteModelList.Add(new QuoteModel()
                    {
                        bid = bidStrategyModeList["Manual"],
                        ofr = ofrStrategyModeList["Manual"],
                    });

                    _quoteModelList.Add(new QuoteModel()
                    {
                        bid = bidStrategyModeList["Defend"],
                        ofr = ofrStrategyModeList["Defend"],
                    });

                    /*_quoteModelList.Add(new QuoteModel()
                    {
                        bid = bidStrategyModeList["Algorithm"],
                        ofr = ofrStrategyModeList["Algorithm"],
                    });
                    if ("1".Equals(LocalEnv.ConfInfo.openMMDHideSet))
                    {
                        _quoteModelList.Add(new QuoteModel()
                        {
                            bid = bidStrategyModeList["API"],
                            ofr = ofrStrategyModeList["API"],
                        });
                    }*/
                }
                return _quoteModelList;
            }
        }

        #endregion

        /// <summary>
        /// 打开tradebook界面
        /// </summary>
        public void OpenTradeBook(string securityID)
        {
            if (!string.IsNullOrEmpty(securityID))
            {
                // 使用插件形式打开深度行情页面
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
    }
}
