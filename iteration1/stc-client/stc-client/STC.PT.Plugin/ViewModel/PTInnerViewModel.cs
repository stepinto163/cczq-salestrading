using com.gateway.log;
using netapi.Api;
using Newtonsoft.Json;
using STC.Common.ColumnSetting;
using STC.Common.Convertor;
using STC.Common.DataProcesser;
using STC.Common.Handler.Admin;
using STC.Common.Handler.Base;
using STC.Common.Handler.Cal;
using STC.Common.Handler.PT;
using STC.Common.Model.Admin;
using STC.Common.Model.Base;
using STC.Common.Model.Cal;
using STC.Common.Model.CRM;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.PT;
using STC.Common.Model.Table;
using STC.Common.Utils;
using STC.Common.View.Component;
using STC.Common.View.UI;
using STC.PT.Plugin.Handler.Process;
using STC.PT.Plugin.Model;
using STC.PT.Plugin.Utils;
using STC.PT.Plugin.View;
using STC.PT.Plugin.View.Component;
using STC.PT.Plugin.View.ExecManage;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using CommonUtils = STC.Common.Utils.CommonUtils;
using CustomExecDataGrid = STC.PT.Plugin.View.Component.CustomExecDataGrid;

namespace STC.PT.Plugin.ViewModel
{
    public class PTInnerViewModel : ViewModelBase
    {
        #region 构造
        string sendMsg = "手工下发操作只对匹配状态为<全部匹配>的成交记录生效。";
        private string _SelectedProductId = null;
        /// <summary>
        /// 选中恒生编号，以,隔开
        /// </summary>
        public string SelectedProductId
        {
            get { return _SelectedProductId; }
            set
            {
                if (_SelectedProductId != value)
                {
                    _SelectedProductId = value;
                    this.OnPropertyChanged();
                }
            }
        }
        private bool _RefreshBtnEnable = true;

        public bool RefreshBtnEnable
        {
            get { return _RefreshBtnEnable; }
            set
            {
                _RefreshBtnEnable = value;
                this.OnPropertyChanged();
            }
        }
        public PTInnerView ptInnerView;
        public CustomExecDataGrid Post_dataGrid;
        private int PageMaxCount = 2000; 
        private int pageindex = 0;
        public int PageIndex
        {
            get
            {
                return pageindex;
            }

            set
            {
                pageindex = value;
            }
        }
        private Common.View.Component.PageControl _PageControl;
        public Common.View.Component.PageControl PageControl
        {
            get
            {
                return _PageControl;
            }

            set
            {
                _PageControl = value;
                this.OnPropertyChanged();
            }
        }
        private string _PageVisible = "false";
        /// <summary>
        /// 订单明细分页是否可以见
        /// </summary>
        public string PageVisible
        {
            get
            {
                return _PageVisible;
            }

            set
            {
                _PageVisible = value;
                this.OnPropertyChanged();
            }
        }
        private int totalCount = 0;
        public int TotalCount
        {
            get
            {
                return totalCount;
            }

            set
            {
                totalCount = value;
                this.OnPropertyChanged();
            }
        }
        private string _ExecIDHintText = "搜索成交单号";

        public string ExecIDHintText
        {
            get
            {
                return _ExecIDHintText;
            }

            set
            {
                if (_ExecIDHintText != value)
                {
                    _ExecIDHintText = value;
                    this.OnPropertyChanged();
                }
            }
        }
        /// <summary>
        /// 债券查询条件
        /// </summary>
        private string _searchText;

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
                    if (isInit)
                    {
                        RefreshQueryData();
                    }
                    //IniFileHelper.WriteValue(WinIniKey, FilterBondText, _searchText + "");
                }
            }
        }


        private string _SearchExecIdText = string.Empty;
        public string SearchExecIdText 
        {
            get 
            {
                return _SearchExecIdText;
            }
            set 
            {
                if (_SearchExecIdText != value)
                {
                    _SearchExecIdText = value;
                    this.OnPropertyChanged();
                    //IniFileHelper.WriteValue(WinIniKey, FilterExecId, _SearchExecIdText + "");
                }
            }
        }

        private bool _SendedIsCheck = false;
        public bool SendedIsCheck
        {
            get
            {
                return _SendedIsCheck;
            }
            set
            {
                if (_SendedIsCheck != value)
                {
                    _SendedIsCheck = value;
                    this.OnPropertyChanged();
                    if (isInit)
                    {
                        RefreshQueryData();
                    }
                }
            }
        }

        private bool _HistoryIsCheck = false;
        public bool HistoryIsCheck
        {
            get
            {
                return _HistoryIsCheck;
            }
            set
            {
                if (_HistoryIsCheck != value)
                {
                    _HistoryIsCheck = value;
                    this.OnPropertyChanged();
                    if (isInit)
                    {
                        RefreshQueryData();
                    }
                }
            }
        }

        private DateTime? createTimeBegin;
        /// <summary>
        /// 创建开始日期
        /// </summary>
        public DateTime? CreateTimeBegin
        {
            get { return createTimeBegin; }
            set
            {
                bool isupdate = false;
                if (value != createTimeBegin) 
                {
                    isupdate = true;
                }
                createTimeBegin = value;
                CheckTimeLimitSelected(true);
                CheckTimeLimitSelected();
                this.OnPropertyChanged();
                if (isInit && isupdate && !isProgSetTime && checkCreateDateSeleted())
                {
                    RefreshQueryData();
                }
                //IniFileHelper.WriteValue(WinIniKey, FilterCreateTimeBegin, createTimeBegin != null?createTimeBegin.Value.ToString("yyyy-MM-dd"):"");
            }
        }

        private DateTime? createTimeEnd;
        /// <summary>
        /// 创建结束日期
        /// </summary>
        public DateTime? CreateTimeEnd
        {
            get { return createTimeEnd; }
            set
            {
                bool isupdate = false;
                if (value != createTimeEnd)
                {
                    isupdate = true;
                }
                createTimeEnd = value;
                CheckTimeLimitSelected(false);
                CheckTimeLimitSelected();
                this.OnPropertyChanged();
                if (isInit && isupdate && !isProgSetTime && checkCreateDateSeleted())
                {
                    RefreshQueryData();
                }
                //IniFileHelper.WriteValue(WinIniKey, FilterCreateTimeEnd, createTimeEnd != null?createTimeEnd.Value.ToString("yyyy-MM-dd"):"");
            }
        }
        private void CheckTimeLimitSelected(bool isStart)
        {
            if (CreateTimeBegin.HasValue && CreateTimeEnd.HasValue) 
            {
                if (isStart)
                {
                    if (CreateTimeBegin.Value.Date > CreateTimeEnd.Value.Date)
                    {
                        CreateTimeEnd = CreateTimeBegin;
                    }
                }
                else 
                {
                    if (CreateTimeEnd.Value.Date < CreateTimeBegin.Value.Date)
                    {
                        CreateTimeBegin = CreateTimeEnd;
                    }
                }
            }
        }
        private DateTime? _CurrentDate;
        public DateTime? CurrentDate
        {
            get { return _CurrentDate; }
            set
            {
                _CurrentDate = value;
                this.OnPropertyChanged();
            }
        }
        #region
        private DateTime? _TradeTimeBegin;
        /// <summary>
        /// 创建开始日期
        /// </summary>
        public DateTime? TradeTimeBegin
        {
            get { return _TradeTimeBegin; }
            set
            {
                bool isupdate = false;
                if (value != _TradeTimeBegin)
                {
                    isupdate = true;
                }
                _TradeTimeBegin = value;
                CheckTradeLimitSelected(true);
                CheckTradeLimitSelected();
                this.OnPropertyChanged();
                if (isInit && isupdate && !isProgSetTime && checkTradeDateSeleted())
                {
                    RefreshQueryData();
                }
                //IniFileHelper.WriteValue(WinIniKey, FilterTradeTimeBegin, _TradeTimeBegin != null? _TradeTimeBegin.Value.ToString("yyyy-MM-dd"):"");
            }
        }

        private DateTime? _TradeTimeEnd;
        /// <summary>
        /// 创建结束日期
        /// </summary>
        public DateTime? TradeTimeEnd
        {
            get { return _TradeTimeEnd; }
            set
            {
                bool isupdate = false;
                if (value != _TradeTimeEnd)
                {
                    isupdate = true;
                }
                _TradeTimeEnd = value;
                CheckTradeLimitSelected(false);
                //CheckTradeLimitSelected();
                this.OnPropertyChanged();
                if (isInit && isupdate && !isProgSetTime && checkTradeDateSeleted())
                {
                    RefreshQueryData();
                }
                //IniFileHelper.WriteValue(WinIniKey, FilterTradeTimeEnd, _TradeTimeEnd != null ? _TradeTimeEnd.Value.ToString("yyyy-MM-dd"):"");
            }
        }
        private int _TradeLimitSelectedIndex = 0;
        public int TradeLimitSelectedIndex
        {
            get { return _TradeLimitSelectedIndex; }
            set
            {
                _TradeLimitSelectedIndex = value;
                isProgSetTime = true;
                switch (value.ToString())
                {
                    case "0":
                    case "today":
                        this.TradeTimeBegin = DateTime.Now;
                        this.TradeTimeEnd = DateTime.Now;
                        this.CreateTimeBegin = null;
                        this.CreateTimeEnd = null;
                        TimeLimitSelectedIndex = -1;
                        break;
                    case "1":
                    case "tomorrow":
                        GetSettleTypeFromServer("2",false);
                        this.CreateTimeBegin = null;
                        this.CreateTimeEnd = null;
                        TimeLimitSelectedIndex = -1;
                        break;
                    case "2":
                    case "forward":
                        GetSettleTypeFromServer("2", true);
                        this.CreateTimeBegin = null;
                        this.CreateTimeEnd = null;
                        TimeLimitSelectedIndex = -1;
                        break;
                }
                IniFileHelper.WriteValue(WinIniKey, FilterTradeLimitIndex, _TradeLimitSelectedIndex +"");

                isProgSetTime = false;


                if (isInit && _TradeLimitSelectedIndex != -1)
                {
                    RefreshQueryData();
                }
                this.OnPropertyChanged();
            }
        }
        private void CheckTradeLimitSelected(bool isStart)
        {
            if (TradeTimeBegin.HasValue && TradeTimeEnd.HasValue)
            {
                if (isStart)
                {
                    if (TradeTimeBegin.Value.Date > TradeTimeEnd.Value.Date)
                    {
                        TradeTimeEnd = TradeTimeBegin;
                    }
                }
                else
                {
                    if (TradeTimeEnd.Value.Date < TradeTimeBegin.Value.Date)
                    {
                        TradeTimeBegin = TradeTimeEnd;
                    }
                }
            }
        }
        private void CheckTradeLimitSelected()
        {
            if (isSendTradeDateReq)
                return;
            switch (TradeLimitSelectedIndex.ToString())
            {
                case "0":
                case "today":
                    if (!isProgSetTime)
                    {
                        if (this.TradeTimeBegin != DateTime.Now || this.TradeTimeEnd != DateTime.Now)
                        {
                            TradeLimitSelectedIndex = -1;
                        }
                    }
                    break;
                case "1":
                case "tomorrow":
                    if (!isProgSetTime)
                    {
                        if (this.TradeTimeBegin != DateTime.Now.AddDays(+1) || this.TradeTimeEnd != DateTime.Now.AddDays(+1))
                        {
                            TradeLimitSelectedIndex = -1;
                        }
                    }
                    break;
                case "2":
                case "forward":
                    if (!isProgSetTime)
                    {
                        if (this.TradeTimeBegin != DateTime.Now.AddDays(+2) || this.TradeTimeEnd != null)
                        {
                            TradeLimitSelectedIndex = -1;
                        }
                    }
                    break;
            }
        }
        #endregion
        private Visibility loadmaskvisible = Visibility.Collapsed;
        /// <summary>
        /// 订单明细分页是否可以见
        /// </summary>
        public Visibility LoadMaskVisible
        {
            get
            {
                return loadmaskvisible;
            }

            set
            {
                loadmaskvisible = value;
                this.OnPropertyChanged(nameof(LoadMaskVisible));
            }
        }
        private int timelimitselectedindex = 0;
        private bool isProgSetTime = false;
        public int TimeLimitSelectedIndex
        {
            get { return timelimitselectedindex; }
            set
            {
                int old = timelimitselectedindex;
                timelimitselectedindex = value;
                isProgSetTime = true;
                switch (value.ToString())
                {
                    case "0":
                    case "today":
                        this.CreateTimeBegin = DateTime.Now;
                        this.CreateTimeEnd = DateTime.Now;
                        this.TradeTimeBegin = null;
                        this.TradeTimeEnd = null;
                        TradeLimitSelectedIndex = -1;
                        break;
                    case "1":
                    case "last7days":
                        this.CreateTimeBegin = DateTime.Now.AddDays(-6);
                        this.CreateTimeEnd = DateTime.Now;
                        this.TradeTimeBegin = null;
                        this.TradeTimeEnd = null;
                        TradeLimitSelectedIndex = -1;
                        break;
                    case "2":
                    case "last30days":
                        this.CreateTimeBegin = DateTime.Now.AddDays(-29);
                        this.CreateTimeEnd = DateTime.Now;
                        this.TradeTimeBegin = null;
                        this.TradeTimeEnd = null;
                        TradeLimitSelectedIndex = -1;
                        break;
                }

                IniFileHelper.WriteValue(WinIniKey, FilterTimeLimitIndex, timelimitselectedindex + "");
                isProgSetTime = false;


                if (isInit && timelimitselectedindex != -1)
                {
                    RefreshQueryData();
                }
                this.OnPropertyChanged();
            }
        }
        private void CheckTimeLimitSelected()
        {
            switch (TimeLimitSelectedIndex.ToString())
            {
                case "0":
                case "today":
                    if (!isProgSetTime)
                    {
                        if (this.CreateTimeBegin != DateTime.Now || this.CreateTimeEnd != DateTime.Now)
                        {
                            TimeLimitSelectedIndex = -1;
                        }
                    }
                    break;
                case "1":
                case "last7days":
                    if (!isProgSetTime)
                    {
                        if (this.CreateTimeBegin != DateTime.Now.AddDays(-6) || this.CreateTimeEnd != DateTime.Now)
                        {
                            TimeLimitSelectedIndex = -1;
                        }
                    }
                    break;
                case "2":
                case "last30days":
                    if (!isProgSetTime)
                    {
                        if (this.CreateTimeBegin != DateTime.Now.AddDays(-29) || this.CreateTimeEnd != DateTime.Now)
                        {
                            TimeLimitSelectedIndex = -1;
                        }
                    }
                    break;
            }
        }

        private bool _isMe;
        // 选择我
        public bool IsMe
        {
            get { return _isMe; }
            set
            {
                if (_isMe != value)
                {
                    _isMe = value;
                    this.OnPropertyChanged();
                    if (isInit)
                    {
                        RefreshQueryData();
                    }
                }
                //if (this.Mv != null && this.Mv.View != null)
                //{
                //    this.Mv.View.Refresh();
                //}
            }
        }
        private Dictionary<string, List<string>> _focusTagDic;
        /// <summary>
        /// 筛选字典
        /// </summary>
        public Dictionary<string, List<string>> FocusTagDic
        {
            get
            {
                return _focusTagDic;
            }
            set
            {
                _focusTagDic = value;
                this.OnPropertyChanged();
                StringBuilder stringBuilder = new StringBuilder();
                //if (_focusTagDic.ContainsKey("DEL_FLAG"))
                //{
                //    stringBuilder.Clear();
                //    IList List = _focusTagDic["DEL_FLAG"];
                //    foreach (string item in List)
                //    {
                //        if (stringBuilder.Length > 0)
                //        {
                //            stringBuilder.Append(",").Append(item);
                //        }
                //        else 
                //        { 
                //            stringBuilder.Append(item);
                //        }
                //    }
                //    IniFileHelper.WriteValue(WinIniKey, FilterDelFlag, stringBuilder.ToString());
                //}
                //if (_focusTagDic.ContainsKey("MATCH_STATUS"))
                //{
                //    stringBuilder.Clear();
                //    IList List = _focusTagDic["MATCH_STATUS"];
                //    foreach (string item in List)
                //    {
                //        if (stringBuilder.Length > 0)
                //        {
                //            stringBuilder.Append(",").Append(item);
                //        }
                //        else
                //        {
                //            stringBuilder.Append(item);
                //        }
                //    }
                //    IniFileHelper.WriteValue(WinIniKey, FilterMatchStatus, stringBuilder.ToString());
                //}
                if (_focusTagDic.ContainsKey("TRADE_DATE"))
                {
                    stringBuilder.Clear();
                    IList List = _focusTagDic["TRADE_DATE"];
                    foreach (string item in List)
                    {
                        if (stringBuilder.Length > 0)
                        {
                            stringBuilder.Append(",").Append(item);
                        }
                        else
                        {
                            stringBuilder.Append(item);
                        }
                    }
                    IniFileHelper.WriteValue(WinIniKey, FilterTradeDate, stringBuilder.ToString());

                }
                //if (_focusTagDic.ContainsKey("LISTED_MARKET"))
                //{
                //    stringBuilder.Clear();
                //    IList List = _focusTagDic["LISTED_MARKET"];
                //    foreach (string item in List)
                //    {
                //        if (stringBuilder.Length > 0)
                //        {
                //            stringBuilder.Append(",").Append(item);
                //        }
                //        else
                //        {
                //            stringBuilder.Append(item);
                //        }
                //    }
                //    IniFileHelper.WriteValue(WinIniKey, FilterListedMarket, stringBuilder.ToString());

                //}
                //if (_focusTagDic.ContainsKey("ISSUE_TYPE"))
                //{
                //    stringBuilder.Clear();
                //    IList List = _focusTagDic["ISSUE_TYPE"];
                //    foreach (string item in List)
                //    {
                //        if (stringBuilder.Length > 0)
                //        {
                //            stringBuilder.Append(",").Append(item);
                //        }
                //        else
                //        {
                //            stringBuilder.Append(item);
                //        }
                //    }
                //    IniFileHelper.WriteValue(WinIniKey, FilterIssueType, stringBuilder.ToString());

                //}
            }
        }
        private string _startPrice;

        public string StartPrice
        {
            get { return _startPrice; }
            set
            {
                _startPrice = value;
                this.OnPropertyChanged();
                if (this.Mv != null && this.Mv.View != null)
                    this.Mv.View.Refresh();
            }
        }
        private string _endPrice;

        public string EndPrice
        {
            get { return _endPrice; }
            set
            {
                _endPrice = value;
                this.OnPropertyChanged();
                if (this.Mv != null && this.Mv.View != null)
                    this.Mv.View.Refresh();

            }
        }

        private bool _IsIRSFilter = false;
        public bool IsIRSFilter
        {
            get { return _IsIRSFilter; }
            set 
            { 
                _IsIRSFilter = value;
                this.OnPropertyChanged();
            }
        }

        private bool _IsSingleRow = false;
        public bool IsSingleRow
        {
            get { return _IsSingleRow; }
            set
            {
                _IsSingleRow = value;

                if (value && SelectedItem != null && isCurrentAfterDate(SelectedItem.Settldate))
                {
                    CopyDealVisible = true;
                }
                else 
                {
                    CopyDealVisible = false;
                }
                this.OnPropertyChanged();
            }
        }
        private bool _CopyDealVisible = false;
        public bool CopyDealVisible
        {
            get { return _CopyDealVisible; }
            set
            {
                _CopyDealVisible = value;
                this.OnPropertyChanged();
            }
        }

        private string _focusTags;

        public string FocusTags
        {
            get { return _focusTags; }
            set
            {
                if (_focusTags != value)
                {
                    _focusTags = value;

                    try
                    {
                        if (!string.IsNullOrEmpty(value))
                        {
                            FocusTagDic = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(value);
                        }
                        else
                        {
                            FocusTagDic = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(CustomFilterPanel.DEFAULT_FOCUSTAGS);
                        }
                    }
                    catch (Exception)
                    {
                        FocusTagDic = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(CustomFilterPanel.DEFAULT_FOCUSTAGS);
                    }

                    this.OnPropertyChanged();
                    if (this.Mv != null && this.Mv.View != null)
                    {
                        this.Mv.View.Refresh();
                    }
                }
            }
        }

        private CollectionViewSource mv = new CollectionViewSource();
        public CollectionViewSource Mv
        {
            get { return mv; }
            set
            {
                mv = value;
                this.OnPropertyChanged(nameof(Mv));
            }
        }

        public static List<KeyValuePair<string, string>> SideItemSource
        {
            get
            {
                return ComboSourceUtil.Instance.GetComboSource("side_pt");
            }
        }

        public static List<KeyValuePair<string, string>> MatchstatusItemSource
        {
            get
            {
                return ComboSourceUtil.Instance.GetComboSource("Matchstatus");
            }
        }

        public static List<KeyValuePair<string, string>> RelatestatusItemSource
        {
            get
            {
                return ComboSourceUtil.Instance.GetComboSource("Relatestatus");
            }
        }

        public static List<KeyValuePair<string, string>> SendstatusItemSource
        {
            get
            {
                return ComboSourceUtil.Instance.GetComboSource("Sendstatus");
            }
        }

        public static List<KeyValuePair<string, string>> TagItemSource
        {
            get
            {
                return ComboSourceUtil.Instance.GetComboSource("tag1_pt");
            }
        }
        public static List<KeyValuePair<string, string>> QuotemethodItemSource
        {
            get
            {
                return ComboSourceUtil.Instance.GetComboSource("quotemethod_pt");
            }
        }
        public static List<KeyValuePair<string, string>> BusinessTypeItemSource
        {
            get
            {
                return ComboSourceUtil.Instance.GetComboSource("businesstype");
            }
        }
        public static List<KeyValuePair<string, string>> BookItemSource
        {
            get
            {
                return PTExecOrderProcesser.Instance.BookDict.ToList();
            }
        }
        private ObservableCollection<CrmInst> _ClientInstList = new ObservableCollection<CrmInst>();

        public ObservableCollection<CrmInst> ClientInstList
        {
            get
            {
                return _ClientInstList;
            }
        }

        private ObservableCollection<CrmContact> _ClientList = new ObservableCollection<CrmContact>();

        public ObservableCollection<CrmContact> ClientList
        {
            get
            {
                return _ClientList;
            }
        }
        private Dictionary<string, object> _InvestorstItemsSource = new Dictionary<string, object>();
        public Dictionary<string, object> InvestorstItemsSource
        {
            get { return _InvestorstItemsSource; }
            set
            {
                if (_InvestorstItemsSource != value)
                {
                    _InvestorstItemsSource = value;
                    this.OnPropertyChanged();
                }
            }
        }
        private string _SelectedInvestorstItem = null;
        public string SelectedInvestorstItem
        {
            get { return _SelectedInvestorstItem; }
            set
            {
                _SelectedInvestorstItem = value;
                this.OnPropertyChanged();
                if (this.Mv != null && this.Mv.View != null) 
                    this.Mv.View.Refresh();
                //IniFileHelper.WriteValue(WinIniKey, FilterInvestorstItem, _SelectedInvestorstItem + "");
            }
        }
        private Dictionary<string, object> _TradersItemsSource = new Dictionary<string, object>();
        public Dictionary<string, object> TradersItemsSource
        {
            get { return _TradersItemsSource; }
            set
            {
                if (_TradersItemsSource != value)
                {
                    _TradersItemsSource = value;
                    this.OnPropertyChanged();
                }
            }
        }
        private string _SelectedTradersItem = null;
        public string SelectedTradersItem
        {
            get { return _SelectedTradersItem; }
            set
            {
                _SelectedTradersItem = value;
                this.OnPropertyChanged();
                if (this.Mv != null && this.Mv.View != null)
                    this.Mv.View.Refresh();
                //IniFileHelper.WriteValue(WinIniKey, FilterTradersItem, _SelectedTradersItem + "");
            }
        }

        private string _SelectedTradersText = null;
        public string SelectedTradersText
        {
            get { return _SelectedTradersText; }
            set
            {
                _SelectedTradersText = value;
                this.OnPropertyChanged();
                if (this.Mv != null && this.Mv.View != null)
                    this.Mv.View.Refresh();
               
            }
        }
        
        /// <summary>
        /// 选中的行数据
        /// </summary>
        private PtExecordersData _selectedItem;
        public PtExecordersData SelectedItem
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
        private ObservableCollection<TreeNodeModel> _TreeComboxItemsSource = new ObservableCollection<TreeNodeModel>();
        public ObservableCollection<TreeNodeModel> MultiTreeComboxItemsSource
        {
            get { return _TreeComboxItemsSource; }
            set
            {
                if (_TreeComboxItemsSource != value)
                {
                    _TreeComboxItemsSource = value;
                    this.OnPropertyChanged("MultiTreeComboxItemsSource");
                }
            }
        }
        private string _SelectedBookItem = null;
        public string SelectedBookItem
        {
            get { return _SelectedBookItem; }
            set
            {
                _SelectedBookItem = value;
                this.OnPropertyChanged();
                if (this.Mv != null && this.Mv.View != null)
                    this.Mv.View.Refresh();
                if (ptInnerView.init)
                {
                    IniFileHelper.WriteValue(WinIniKey, FilterBookItem, _SelectedBookItem + "");
                }
            }
        }
        private string _SelectedBookText = null;
        public string SelectedBookText
        {
            get { return _SelectedBookText; }
            set
            {
                _SelectedBookText = value;
                this.OnPropertyChanged();
                if (ptInnerView.init)
                {
                    IniFileHelper.WriteValue(WinIniKey, FilterBookItemText, _SelectedBookText + "");
                }
            }
        }

        
        private bool _Incomplete;

        public bool Incomplete
        {
            get { return _Incomplete; }
            set
            {
                if (_Incomplete != value)
                {
                    _Incomplete = value;
                    this.OnPropertyChanged();
                    if (this.Mv != null && this.Mv.View != null)
                    {
                        this.Mv.View.Refresh();
                    }
                }
            }
        }
        private int _TradeDateSelectedIndex = -1;
        public int TradeDateSelectedIndex
        {
            get { return _TradeDateSelectedIndex; }
            set
            {
                _TradeDateSelectedIndex = value;
                this.OnPropertyChanged(); 
                if (this.Mv != null && this.Mv.View != null)
                {
                    this.Mv.View.Refresh();
                }
            }
        }
        private bool _IsInvestor = false;
        public bool IsInvestor

        {
            get { return _IsInvestor; }
            set
            {
                _IsInvestor = value;
                this.OnPropertyChanged();
            }
        }
        #endregion



        #region 事件定义
        public ICommand EditCommand { get { return new RelayCommand(EditClick); } }
        public ICommand DeleteCommand { get { return new RelayCommand(DeleteClick); } }
        public ICommand SendCommand { get { return new RelayCommand(SendClick); } }

        public ICommand SendFQCommand { get { return new RelayCommand(SendFQClick); } }
        public ICommand HistoryCommand { get { return new RelayCommand(HistoryClick); } }

        public ICommand EnterCommand { get { return new RelayCommand(EnterClick); } }
        public ICommand MannelSendCommand { get { return new RelayCommand(MannelSendClick); } }
        public ICommand CancelSendCommand { get { return new RelayCommand(CancelSendClick); } }
        public ICommand BatchRecognizeCommand { get { return new RelayCommand(BatchRecognizeClick); } }

        public ICommand ImportCommand { get { return new RelayCommand(ImportExcel); } }

        public ICommand ExportCommand { get { return new RelayCommand(ExportExcel); } }
        public ICommand ExportHisCommand { get { return new RelayCommand(ExportExcelHis); } }
        public ICommand CopyDealEntryCommand { get { return new RelayCommand(CopyDealEntry); } }
        public ICommand AddIntentionQuoteCommand { get { return new RelayCommand(AddIntentionQuoteClick); } }
        public ICommand RelationCommand { get { return new RelayCommand(RelationClick); } }

        public ICommand RefreshCommand { get { return new ComRelayCommand(OnRefreshCommand); } }

        #endregion
        #region 批量识别窗口
        private void BatchRecognizeClick()
        {
            /*if (LocalEnv.ConfInfo.IsPTBrokerExec)
            {*/
                if (!WindowManager.Instance.IsExist("BatchRecognizeView"))
                {
                    BatchRecognizeView win = new BatchRecognizeView();
                    WindowManager.Instance.OpenOnlyOneWindow("BatchRecognizeView", win);
                }
           /* }
            else 
            {
                if (!WindowManager.Instance.IsExist("RecognizeDealWin"))
                {
                    RecognizeDealWin win = new RecognizeDealWin();
                    WindowManager.Instance.OpenOnlyOneWindow("RecognizeDealWin", win);
                }
            }*/
        }
        #endregion
        #region 数据集合定义

        /// <summary>
        /// 我的订单资源锁
        /// </summary>
        private object _lockObj = new object();
        public object LockObj
        {
            get
            {
                return _lockObj;
            }
        }
        private ObservableCollection<PtExecordersData> _ptOrders = new ObservableCollection<PtExecordersData>();

        public ObservableCollection<PtExecordersData> PTOrders
        {
            get
            {
                if (_ptOrders == null)
                    _ptOrders = new ObservableCollection<PtExecordersData>();

                return _ptOrders;
            }

            set
            {
                _ptOrders = value;
                this.OnPropertyChanged(nameof(PTOrders)); 
                if (this.Mv != null && this.Mv.View != null)
                    this.Mv.View.Refresh();
            }
        }
        private ObservableCollection<TotalExecDataPo> _TotalOrders = new ObservableCollection<TotalExecDataPo>();

        public ObservableCollection<TotalExecDataPo> TotalOrders
        {
            get
            {
                if (_TotalOrders == null)
                    _TotalOrders = new ObservableCollection<TotalExecDataPo>();

                return _TotalOrders;
            }

            set
            {
                _TotalOrders = value;
                this.OnPropertyChanged(nameof(TotalOrders));
                this.TotalMV.View.Refresh();
            }
        }
        private CollectionViewSource _TotalMV = new CollectionViewSource();
        public CollectionViewSource TotalMV
        {
            get { return _TotalMV; }
            set
            {
                _TotalMV = value;
                this.OnPropertyChanged(nameof(TotalMV));
            }
        }
        /// <summary>
         /// 选中的行数据
         /// </summary>
        private TotalExecDataPo _TotalSelectedItem;
        public TotalExecDataPo TotalSelectedItem
        {
            get
            {
                return _TotalSelectedItem;
            }
            set
            {
                _TotalSelectedItem = value;
                this.OnPropertyChanged();
            }
        }
        #endregion
        #region 静态字段
        public const string _ModelId = "4019";
        public const string _INNERTypeModelId = "6001";
        public bool ManageYourselfViewAll = false;
        public string user = ApiManager.Instance.GetDefaultApi().UserName;
        public bool isSales = false;
        private readonly string WinIniKey = $"STC.PT.Plugin_{LocalEnv.UserInfo.UserName}_HomePageFilter";
        private readonly string FilterBondText = "FilterBondText";
        private readonly string FilterExecId = "FilterExecId";
        private readonly string FilterCreateTimeBegin = "FilterCreateTimeBegin";
        private readonly string FilterCreateTimeEnd = "FilterCreateTimeEnd";
        private readonly string FilterTimeLimitIndex = "FilterTimeLimitIndex";
        private readonly string FilterInvestorstItem = "FilterInvestorstItem";
        private readonly string FilterTradersItem = "FilterTradersItem";
        private readonly string FilterVenue = "FilterVenue";
        private readonly string FilterBookItem = "FilterBookItem";
        private readonly string FilterBookItemText = "FilterBookItemText";
        private readonly string FilterDelFlag = "FilterDelFlag";
        private readonly string FilterTradeTimeBegin = "FilterTradeTimeBegin";
        private readonly string FilterTradeTimeEnd = "FilterTradeTimeEnd";
        private readonly string FilterTradeLimitIndex = "FilterTradeLimitIndex";
        private readonly string FilterMatchStatus = "FilterMatchStatus";
        private readonly string FilterListedMarket = "FilterListedMarket";
        private readonly string FilterIssueType = "FilterIssueType";
        private readonly string FilterTradeDate = "FilterTradeDate";
        #endregion

        public PTInnerViewModel()
        {
            BaseHandler.Instance.SubscribeLoadDataStatus(onloadData);
            //订阅来自我的订单的录入成交消息
            PTHandler.Instance.SubscribeSendPT(OnShow);
            isInit = false;
            isInit = true;
            MultiTreeComboxItemsSource = STC.Common.Handler.PP.PPHandler.Instance.GetTradeTBookGroup();
        }
        #region 加载缓存筛选项
        /// <summary>
        /// 筛选项
        /// </summary>
        private void reLoadCacheFilters()
        {
            //string bondText = IniFileHelper.ReadValue(WinIniKey, FilterBondText);
            //if (!string.IsNullOrWhiteSpace(bondText))
            //    SearchText = bondText;
            //string execid = IniFileHelper.ReadValue(WinIniKey, FilterExecId);
            //if (!string.IsNullOrWhiteSpace(execid))
            //    SearchExecIdText = execid;   
            //string timelimitindex = IniFileHelper.ReadValue(WinIniKey, FilterTimeLimitIndex);
            //if (!string.IsNullOrWhiteSpace(timelimitindex))
            //    TimeLimitSelectedIndex = int.Parse(timelimitindex);
            //else
            //{
            //    TimeLimitSelectedIndex = 0;
            //}
            TimeLimitSelectedIndex = 0;
            string createtimebegin = IniFileHelper.ReadValue(WinIniKey, FilterCreateTimeBegin);
            DateTime createtimebeginresult;
            if (!string.IsNullOrWhiteSpace(createtimebegin) && DateTime.TryParseExact(createtimebegin, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out createtimebeginresult))
                CreateTimeBegin = createtimebeginresult;
            string createtimeend = IniFileHelper.ReadValue(WinIniKey, FilterCreateTimeEnd);
            DateTime createtimeendresult;
            if (!string.IsNullOrWhiteSpace(createtimeend) && DateTime.TryParseExact(createtimeend, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out createtimeendresult))
                CreateTimeEnd = createtimeendresult;
            //string investorstitem = IniFileHelper.ReadValue(WinIniKey, FilterInvestorstItem);

            //if (!string.IsNullOrWhiteSpace(investorstitem))
            //{
            //    SelectedInvestorstItem = investorstitem;
            //    //this.ptInnerView.investorstcb.SelectedValues = investorstitem;
            //}
            string traderitem = IniFileHelper.ReadValue(WinIniKey, FilterTradersItem);
            if (!string.IsNullOrWhiteSpace(traderitem))
                SelectedTradersItem = traderitem;
            //this.ptInnerView.traderscb.SelectedValues = traderitem;
            //string venues = IniFileHelper.ReadValue(WinIniKey, FilterVenue);
            //if (!string.IsNullOrWhiteSpace(venues))
            //    this.ptInnerView.VenuesMscb.SelectedValues = venues;
            /* string bookitem = IniFileHelper.ReadValue(WinIniKey, FilterBookItem);
             if (!string.IsNullOrWhiteSpace(bookitem))
             {
                 SelectedBookItem = bookitem;
                 //this.ptInnerView.bookmcb.SetSelection(bookitem);
                 string bookitemtext = IniFileHelper.ReadValue(WinIniKey, FilterBookItemText);
                 if (!string.IsNullOrWhiteSpace(bookitemtext))
                     SelectedBookText = bookitemtext;
             }*/
            //string delflag = IniFileHelper.ReadValue(WinIniKey, FilterDelFlag);
            //if (!string.IsNullOrWhiteSpace(delflag))
            //{
            //    ObservableCollection<OptionItem> list = this.ptInnerView.delflagfliter.ItemsSource;
            //    List<string> defaultList = delflag.Split(',').ToList();
            //    foreach (OptionItem item in list)
            //    {
            //        if (defaultList.Contains(item.Key))
            //        {
            //            item.IsSelected = true;
            //        }
            //    }
            //}
            ObservableCollection<OptionItem> list = this.ptInnerView.delflagfliter.ItemsSource;
            foreach (OptionItem item in list)
            {
                if ("0".Equals(item.Key))
                {
                    item.IsSelected = true;
                }
            }
            TradeTimeBegin = null;
            TradeTimeEnd = null;
            //string tradetimebegin = IniFileHelper.ReadValue(WinIniKey, FilterTradeTimeBegin);
            //DateTime tradetimebeginresult;
            //if (!string.IsNullOrWhiteSpace(tradetimebegin) && DateTime.TryParseExact(tradetimebegin, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out tradetimebeginresult))
            //    TradeTimeBegin = tradetimebeginresult;

            //string tradetimeend = IniFileHelper.ReadValue(WinIniKey, FilterTradeTimeEnd);
            //DateTime tradetimeendresult;
            //if (!string.IsNullOrWhiteSpace(tradetimeend) && DateTime.TryParseExact(tradetimeend, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out tradetimeendresult))
            //    TradeTimeEnd = tradetimeendresult;
            /*string tradelimitindex = IniFileHelper.ReadValue(WinIniKey, FilterTradeLimitIndex);
            if (!string.IsNullOrWhiteSpace(tradelimitindex))
                TradeLimitSelectedIndex = int.Parse(tradelimitindex);
            else
            {
                if (string.IsNullOrWhiteSpace(tradetimebegin) && string.IsNullOrWhiteSpace(tradetimeend))
                    TradeLimitSelectedIndex = 0;
            }*/
            //string matchstatus = IniFileHelper.ReadValue(WinIniKey, FilterMatchStatus);
            //if (!string.IsNullOrWhiteSpace(matchstatus))
            //{
            //    ObservableCollection<OptionItem> list = this.ptInnerView.matchstausfliter.ItemsSource;
            //    List<string> defaultList = matchstatus.Split(',').ToList();
            //    foreach (OptionItem item in list)
            //    {
            //        if (defaultList.Contains(item.Key))
            //        {
            //            item.IsSelected = true;
            //        }
            //    }
            //}
            //string listmarket = IniFileHelper.ReadValue(WinIniKey, FilterListedMarket);
            //if (!string.IsNullOrWhiteSpace(listmarket))
            //{
            //    ObservableCollection<OptionItem> list = this.ptInnerView.listmarketfilter.ItemsSource;
            //    List<string> defaultList = listmarket.Split(',').ToList();
            //    foreach (OptionItem item in list)
            //    {
            //        if (defaultList.Contains(item.Key))
            //        {
            //            item.IsSelected = true;
            //        }
            //    }
            //}
            //string issuetype = IniFileHelper.ReadValue(WinIniKey, FilterIssueType);
            //if (!string.IsNullOrWhiteSpace(issuetype))
            //{
            //    ObservableCollection<OptionItem> list = this.ptInnerView.issuetypefilter.ItemsSource;
            //    List<string> defaultList = issuetype.Split(',').ToList();
            //    foreach (OptionItem item in list)
            //    {
            //        if (defaultList.Contains(item.Key))
            //        {
            //            item.IsSelected = true;
            //        }
            //    }
            //}
            /*string tradedate = IniFileHelper.ReadValue(WinIniKey, FilterTradeDate);
            if (!string.IsNullOrWhiteSpace(tradedate))
            {
                ObservableCollection<OptionItem> list = this.ptInnerView.tradedatefliter.ItemsSource;
                List<string> defaultList = tradedate.Split(',').ToList();
                foreach (OptionItem item in list)
                {
                    if (defaultList.Contains(item.Key))
                    {
                        item.IsSelected = true;
                    }
                }
            }*/
        }
        #endregion
        #region 初始化
        internal void load()
        {
            this.Mv.Source = PTOrders;
            this.Mv.IsLiveFilteringRequested = true;
            this.Mv.IsLiveSortingRequested = true;
            this.Mv.View.CollectionChanged += View_CollectionChanged;

            this.Mv.SortDescriptions.Add(new SortDescription("UpdateTime", ListSortDirection.Descending));

            this.TotalMV.Source = TotalOrders;
            this.TotalMV.IsLiveFilteringRequested = true;
            this.TotalMV.IsLiveSortingRequested = true;
            this.TotalMV.SortDescriptions.Add(new SortDescription("SecurityID", ListSortDirection.Descending));

            SetMvFilter();

        }

        public bool isLoad = false;
        private void onloadData(bool obj)
        {
            if (obj)
            {
                if (!isLoad)
                {
                    isLoad = true;
                    ThreadPoolHelper.InvokeThreadPool((o) =>
                    {
                        Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background, (Action)(() =>
                        {
                            // 查询持仓组合
                            Init();
                        }));
                    });
                }
            }
        }
        public void Init()
        {
            try
            {
                List<ModulesAction> Action = AdminHandler.Instance.GetModulesActionById(_ModelId);
                if (Action != null && Action.Count > 0)
                {
                    foreach (ModulesAction m in Action)
                    {
                        if ("401902".Equals(m.id))//管理自己&查看全部
                        {
                            user = "*";
                            ManageYourselfViewAll = true;
                        }
                        else if ("401903".Equals(m.id))//管理全部
                        {
                            user = "*";
                        }
                        else
                        {//401901管理自己
                        }
                        break;
                    }
                    PTHandler.Instance.SubscribeExec(Instance_OnExec, user);
                }
                List<ModulesAction> ActionINNER = AdminHandler.Instance.GetModulesActionById(_INNERTypeModelId);
                if (ActionINNER != null && ActionINNER.Count > 0)
                {
                    foreach (ModulesAction m in ActionINNER)
                    {
                        if ("600102".Equals(m.id))//代客销售角色
                        {
                            isSales = true;
                        }
                        else
                        {
                            isSales = false;
                        }
                        break;
                    }
                }
                IsInvestor = CommonUtils.IsInvestor();
                GetAllBook();
                PtExecOrderQueryReq req = new PtExecOrderQueryReq();
                req.pageNum = PageIndex;
                req.pageSize = PageMaxCount;
                req.securityid = this.SearchText;
                req.venueexecid = this.SearchExecIdText;
                //req.send = SendedIsCheck ? true : false;
                //req.histOrder = HistoryIsCheck ? true : false;
                req.createTimeBegin = this.CreateTimeBegin.HasValue ? $"{this.CreateTimeBegin.Value.ToString("yyyy-MM-dd")}" : "";
                req.createTimeEnd = this.CreateTimeEnd.HasValue ? $"{this.CreateTimeEnd.Value.ToString("yyyy-MM-dd")}" : "";
                req.tradedateBegin = this.TradeTimeBegin.HasValue ? $"{this.TradeTimeBegin.Value.ToString("yyyyMMdd")}" : "";
                req.tradedateEnd = this.TradeTimeEnd.HasValue ? $"{this.TradeTimeEnd.Value.ToString("yyyyMMdd")}" : ""; 
                /*if (IsInvestor)
                {
                    req.partysubid = LocalEnv.UserInfo.UserId;
                }*/
                //req.userId = this.IsMe ? LocalEnv.UserInfo.UserId : "";
                CurrentDate = DateTime.Today;
                ExecOrderQuery(req);
                PTInnerSettingViewModel.Instance.getTabStr();
                this.ptInnerView.init = true;
                //风控检查试算
                ExecOrderRiskCheckAsync(req);
            }
            catch (Exception e)
            {
                LogHelper.Error("ExecuterOrderHandler init:", e);
            }
        }
        public async Task<PResult> ExecOrderRiskCheckAsync(PtExecOrderQueryReq req)
        {
            if (!string.IsNullOrEmpty(user) && !"*".Equals(user))
                req.userName = user;
            req.pageSize = PageMaxCount * 3;
            PtExecOrderQueryReq ptExecOrderQueryReq = req.Clone();
            ptExecOrderQueryReq.sendstatus = "0,2";
            PResult reply = null;
            await Task.Run(() =>
            {
                reply = PTHandler.Instance.ExecOrderRiskCheck(req);
            });

            return reply;
        }
        private void setDefaultParam() 
        {
            //SearchText = "";
            SendedIsCheck = false;
            HistoryIsCheck = false;
            IsMe = true;
            reLoadCacheFilters();
        }
        public void GetAllBook()
        {


            PTExecOrderProcesser.Instance.BookDict.Clear();
            PTExecOrderProcesser.Instance.BookAllDict.Clear();
            PTExecOrderProcesser.Instance.BookList.Clear();
            string username = "";
            if (!ManageYourselfViewAll && "*".Equals(user))
            {
                username = "";
            }
            else 
            {
                username = LocalEnv.UserInfo.UserName;
            }
            ObservableCollection<TBookBookTreeItemModel> reply = null;
            if (string.IsNullOrEmpty(username))
            {
                reply = AdminHandler.Instance.GetTAllBookCollection();
                if (reply != null && reply.Count() > 0)
                {
                    foreach (TBookBookTreeItemModel pbm in reply)
                    {
                        PTExecOrderProcesser.Instance.BookList.Add(pbm);
                        AssembleData(pbm);
                    }
                }
            }
            else 
            {
                reply = AdminHandler.Instance.GetTCurrentBookCollection(username);
                if (reply != null && reply.Count() > 0)
                {
                    foreach (TBookBookTreeItemModel pbm in reply)
                    {
                        AssembleData(pbm);
                    }
                }
            }

            Application.Current.Dispatcher.Invoke((Action)(() =>
            {
                PTExecOrderProcesser.Instance.InvestorsList.Clear();
                PTExecOrderProcesser.Instance.BookAllDict = AdminHandler.Instance.GetAllBookDic();
                _InvestorstItemsSource.Clear();
                Dictionary<string, object> dic = new Dictionary<string, object>();
                if (Common.Utils.CommonUtils.IsInvestor())
                {
                    dic.Add(LocalEnv.UserInfo.UserName, LocalEnv.UserInfo.FullName);
                }
                else
                {
                    List<StcTbookUsers> investorsList = Common.Utils.CommonUtils.GetInvestors();
                    foreach (StcTbookUsers res in investorsList)
                    {
                        dic.Add(res.Username, res.Name);
                        PTExecOrderProcesser.Instance.InvestorsList.Add(res);
                    }
                }
                this.InvestorstItemsSource = dic;
                this.ptInnerView.investorstcb.ItemsSource = this.InvestorstItemsSource;
                //this.OnPropertyChanged("InvestorstItemsSource");

                _TradersItemsSource.Clear();
                Dictionary<string, object> tradersdic = new Dictionary<string, object>();
                PTExecOrderProcesser.Instance.UserList.Clear();
                STCUserReplyPo stcUserReplyPo = AdminHandler.Instance.GetCachestcUserQuery();
                if (stcUserReplyPo == null)
                {
                    stcUserReplyPo = AdminHandler.Instance.stcUserQuery();
                }
                if (stcUserReplyPo != null && stcUserReplyPo.Code == "0000" && stcUserReplyPo.Datas.Count > 0)
                {
                    foreach (StcTbookUsers po in stcUserReplyPo.Datas)
                    {
                        PTExecOrderProcesser.Instance.UserList.Add(po);
                        tradersdic.Add(po.Username, po.Name);
                    }
                }
               /* if (!Common.Utils.CommonUtils.IsInvestor())
                {
                    List<StcTbookUsers> tradersList = Common.Utils.CommonUtils.GetTraders();

                    foreach (StcTbookUsers res in tradersList)
                    {
                        tradersdic.Add(res.Username, res.Name);
                    }
                }
                else 
                {
                    List<StcTbookUsers> traders = Common.Utils.CommonUtils.GetTraders();
                    List<string> traderList = AdminHandler.Instance.GetTraderList(LocalEnv.UserInfo.UserId);
                    if (traderList != null)
                    {
                        List<StcTbookUsers> list = traders.Where(d => traderList.Contains(d.Userid)).ToList();//多条件筛选，新功能增加
                        if (list != null && list.Count > 0)
                        {
                            foreach (StcTbookUsers res in list)
                            {
                                tradersdic.Add(res.Username, res.Name);
                            }
                        }
                    }
                }*/
                this.TradersItemsSource = tradersdic;
                this.ptInnerView.traderscb.ItemsSource = this.TradersItemsSource;
                setDefaultParam();
            }));
        }
        private void AssembleData(TBookBookTreeItemModel po)
        {
            if ("A".Equals(po.BookType))
            {
                if (!PTExecOrderProcesser.Instance.BookDict.ContainsKey(po.Id))
                {
                    PTExecOrderProcesser.Instance.BookDict.Add(po.Id, po.Name);
                }
            }
            else
            {
                if (po.Nodes != null && po.Nodes.Count > 0)
                {
                    foreach (TBookBookTreeItemModel tb2 in po.Nodes)
                    {
                        AssembleData(tb2);
                    }
                }
            }
        }

        #endregion

        #region 过滤
        private Dictionary<string, string> venueDic = new Dictionary<string, string>();
        public void refashFilterVenue(string key)
        {
            venueDic.Clear();
            if (FilterDic == null)
            {
                return;
            }
            StringBuilder stringBuilder = new StringBuilder();
            //if ("Venues".Equals(key) && FilterDic.ContainsKey("Venues"))
            //{
            //    IList venuesList = FilterDic["Venues"];
            //    if (venuesList.Count > 0)
            //    {
            //        foreach (string item in venuesList)
            //        {
            //            if (stringBuilder.Length > 0)
            //            {
            //                stringBuilder.Append(",").Append(item);
            //            }
            //            else
            //            {
            //                stringBuilder.Append(item);
            //            }
            //            if (item == "All")
            //            {
            //                break;
            //            }

            //            if (item == "自谈")
            //            {
            //                if (!venueDic.ContainsKey("MANUAL"))
            //                    venueDic.Add("MANUAL", "MANUAL");
            //                if (!venueDic.ContainsKey("QUANT"))
            //                    venueDic.Add("QUANT", "QUANT");
            //                if (!venueDic.ContainsKey("IDB"))
            //                    venueDic.Add("IDB", "IDB");
            //            }
            //            else if (item == CommonConstants.XBond)
            //            {
            //                if (!venueDic.ContainsKey("ODM"))
            //                    venueDic.Add("ODM", "ODM");
            //            }
            //            else if (item == CommonConstants.AllBroker && item == "AllBroker")
            //            {
            //                if (!venueDic.ContainsKey("CNEX"))
            //                    venueDic.Add("CNEX", "CNEX");
            //                if (!venueDic.ContainsKey("TPSC"))
            //                    venueDic.Add("TPSC", "TPSC");
            //                if (!venueDic.ContainsKey("CTTJ"))
            //                    venueDic.Add("CTTJ", "CTTJ");
            //                if (!venueDic.ContainsKey("PTCN"))
            //                    venueDic.Add("PTCN", "PTCN");
            //                if (!venueDic.ContainsKey("CCTB"))
            //                    venueDic.Add("CCTB", "CCTB");
            //                if (!venueDic.ContainsKey("UEDA"))
            //                    venueDic.Add("UEDA", "UEDA");
            //            }
            //            else if (item == CommonConstants.CNEX)
            //            {
            //                if (!venueDic.ContainsKey("CNEX"))
            //                {
            //                    venueDic.Add("CNEX", "CNEX");
            //                }
            //            }
            //            else if (item == CommonConstants.TPSC)
            //            {
            //                if (!venueDic.ContainsKey("TPSC"))
            //                {
            //                    venueDic.Add("TPSC", "TPSC");
            //                }
            //            }
            //            else if (item == CommonConstants.CTTJ)
            //            {
            //                if (!venueDic.ContainsKey("CTTJ"))
            //                {
            //                    venueDic.Add("CTTJ", "CTTJ");
            //                }
            //            }
            //            else if (item == CommonConstants.PTCN)
            //            {
            //                if (!venueDic.ContainsKey("PTCN"))
            //                {
            //                    venueDic.Add("PTCN", "PTCN");
            //                }
            //            }
            //            else if (item == CommonConstants.CCTB)
            //            {
            //                if (!venueDic.ContainsKey("CCTB"))
            //                {
            //                    venueDic.Add("CCTB", "CCTB");
            //                }
            //            }
            //            else if (item == CommonConstants.UEDA)
            //            {
            //                if (!venueDic.ContainsKey("UEDA"))
            //                {
            //                    venueDic.Add("UEDA", "UEDA");
            //                }
            //            }
            //            else if (item.Equals("双边"))
            //            {
            //                if (!venueDic.ContainsKey("QDMESP"))
            //                {
            //                    venueDic.Add("QDMESP", "QDMESP");
            //                }
            //            }
            //            else if (item.Equals(CommonConstants.QDM))
            //            {
            //                if (!venueDic.ContainsKey("QDM"))
            //                {
            //                    venueDic.Add("QDM", "QDM");
            //                }
            //            }
            //            else if (item.Equals(CommonConstants.NDM))
            //            {
            //                if (!venueDic.ContainsKey("NDM"))
            //                {
            //                    venueDic.Add("NDM", "NDM");
            //                }
            //            }
            //        }
            //    }
            //    IniFileHelper.WriteValue(WinIniKey, FilterVenue, stringBuilder.ToString());
            //}
            //if ("investors".Equals(key) && FilterDic.ContainsKey("investors"))
            //{
            //    stringBuilder.Clear();
            //    IList List = FilterDic["investors"];
            //    foreach (string item in List)
            //    {
            //        if (stringBuilder.Length > 0)
            //        {
            //            stringBuilder.Append(",").Append(item);
            //        }
            //        else
            //        {
            //            stringBuilder.Append(item);
            //        }
            //    }
            //    IniFileHelper.WriteValue(WinIniKey, FilterInvestorstItem, stringBuilder.ToString());
            //}

            if ("traders".Equals(key) && FilterDic.ContainsKey("traders"))
            {
                stringBuilder.Clear();
                IList List = FilterDic["traders"];
                foreach (string item in List)
                {
                    if (stringBuilder.Length > 0)
                    {
                        stringBuilder.Append(",").Append(item);
                    }
                    else
                    {
                        stringBuilder.Append(item);
                    }
                }
                IniFileHelper.WriteValue(WinIniKey, FilterTradersItem, stringBuilder.ToString());
            }
            if ("createDateStart".Equals(key) && FilterDic.ContainsKey("createDateStart"))
            {
                IList List = FilterDic["createDateStart"];
                DateTime createDateStart = Convert.ToDateTime(List[0]);       
                IniFileHelper.WriteValue(WinIniKey, FilterCreateTimeBegin, stringBuilder != null ? createDateStart.Date.ToString("yyyy-MM-dd") : "");
            }
            if ("createDateEnd".Equals(key) && FilterDic.ContainsKey("createDateEnd"))
            {
                stringBuilder.Clear();
                IList List = FilterDic["createDateEnd"];
                DateTime createDateEnd = Convert.ToDateTime(List[0]);
                IniFileHelper.WriteValue(WinIniKey, FilterCreateTimeEnd, createDateEnd != null ? createDateEnd.Date.ToString("yyyy-MM-dd") : "");

            }
            //if (FilterDic.ContainsKey("traders"))
            //{
            //    stringBuilder.Clear();
            //    IList List = FilterDic["traders"];
            //    foreach (string item in List)
            //    {
            //        stringBuilder.Append(item).Append(",");
            //    }
            //    IniFileHelper.WriteValue(WinIniKey, FilterTradersItem, stringBuilder.ToString());
            //}
        }

        private void SetMvFilter()
        {
            //CollectionView collectionView = (CollectionView)Mv.View;
            // 设置过滤条件
            this.Mv.View.Filter = item =>
            {
                PtExecordersData data = item as PtExecordersData;
                if (FilterDic != null)
                {
                    if (FilterDic.ContainsKey("investors"))
                    {
                        IList List = FilterDic["investors"];

                        if (List.Count > 0 )
                        {
                            List<string> inverstorList = new List<string>();
                            foreach (string inverstor in List)
                            {
                                inverstorList.Add(inverstor);
                            }
                            string[] arr = null;
                            if (!string.IsNullOrEmpty(data.Partymanagerid))
                            {
                                arr = data.Partymanagerid.Split(CommonConstants.BAR_CHAR, StringSplitOptions.RemoveEmptyEntries);
                            }
                            if (!inverstorList.Any(o => arr != null && arr.Contains(o)))
                                return false;
                        }
                    }
                    //if (FilterDic.ContainsKey("traders"))
                    //{
                    //    IList List = FilterDic["traders"];
                    //    if (List.Count > 0 && !List.Contains(data.PartySubName))
                    //    {
                    //        return false;
                    //    }
                    //}
                }
                if (venueDic.Count > 0) 
                {
                    if (!venueDic.ContainsKey(data.Venues)) 
                    {
                        return false;
                    }
                }
                if (!string.IsNullOrEmpty(this.SelectedTradersText) && !"ALL".Equals(this.SelectedTradersText))
                {
                    string[] bookidArray = this.SelectedTradersText.Split(',');
                    if (bookidArray.Length > 0)
                    {
                        if (!bookidArray.Contains(data.PartySubName))
                            return false;
                    }
                }
                if (!string.IsNullOrEmpty(this.SelectedBookItem))
                {
                    string[] bookidArray = this.SelectedBookItem.Split(',');
                    if (bookidArray.Length > 0)
                    {
                        string[] arr = null;
                        if (!string.IsNullOrEmpty(data.AccountId))
                        {
                            arr = data.AccountId.Split(CommonConstants.BAR_CHAR, StringSplitOptions.RemoveEmptyEntries);
                        }
                        if (!bookidArray.Any(o => arr != null && arr.Contains(o)))
                            return false;
                    }
                }

                bool result = true;
                if (this._focusTagDic != null)
                {

                    if (_focusTagDic.ContainsKey("DEL_FLAG"))
                    {
                        string key = "DEL_FLAG";
                        result = result && (!_focusTagDic.ContainsKey(key) || _focusTagDic[key].Count == 0 || _focusTagDic[key].Contains(data.Delflag));
                        if (!result)
                        {
                            return false;
                        }
                    }
                    if (_focusTagDic.ContainsKey("MATCH_STATUS"))
                    {
                        string key = "MATCH_STATUS";
                        result = result && (!_focusTagDic.ContainsKey(key) || _focusTagDic[key].Count == 0 || _focusTagDic[key].Contains(data.Matchstatus));
                        if (!result)
                        {
                            return false;
                        }
                    }
                    if (_focusTagDic.ContainsKey("TRADE_DATE"))
                    {
                        string key = "TRADE_DATE";
                        List<string> binaryList = new List<string>();
                        string[] arr = null;
                        if (!string.IsNullOrEmpty(data.DateFilter))
                        {
                            arr = data.DateFilter.Split(CommonConstants.BAR_CHAR, StringSplitOptions.RemoveEmptyEntries);
                        }
                        foreach (string ic in _focusTagDic[key])
                        {
                            binaryList.Add(ic);

                        }
                        result = result && (!_focusTagDic.ContainsKey(key) || _focusTagDic[key].Count == 0 || binaryList.Any(o => arr != null && arr.Contains(o)));
                        if (!result)
                        {
                            return false;
                        }
                    }
                }

                if (data.Sign != 0 && this._focusTagDic != null)
                {
                    foreach (KeyValuePair<string, string> pair in data.SignDic)
                    {
                        CheckSign(data, pair.Key, pair.Value, ref result, this._focusTagDic);

                        if (!result)
                            return result;
                    }
                }

                if (data.Sign1 != 0 && this._focusTagDic != null)
                {
                    foreach (KeyValuePair<string, string> pair in data.SignDic1)
                    {
                        CheckSign(data, pair.Key, pair.Value, ref result, this._focusTagDic);

                        if (!result)
                            return result;
                    }
                }

                if (data.Sign2 != 0 && this._focusTagDic != null)
                {
                    foreach (KeyValuePair<string, string> pair in data.SignDic2)
                    {
                        CheckSign(data, pair.Key, pair.Value, ref result, this._focusTagDic);

                        if (!result)
                            return result;
                    }
                }

                if (data.Sign3 != 0 && this._focusTagDic != null)
                {
                    foreach (KeyValuePair<string, string> pair in data.SignDic3)
                    {
                        CheckSign(data, pair.Key, pair.Value, ref result, this._focusTagDic);

                        if (!result)
                            return result;
                    }
                }
                if (result)
                {
                    if (BaseHandler.Instance.GetQuoteTagDic() != null)
                    {
                        foreach (QuoteTag qt in BaseHandler.Instance.GetQuoteTagDic().Values)
                        {
                            if (this._focusTagDic != null && this._focusTagDic.ContainsKey(qt.tagType) && this._focusTagDic[qt.tagType].Count != 0)
                            {
                                if (qt.signField == "sign" && data.Sign == 0)
                                {
                                    return false;
                                }
                                else if (qt.signField == "sign1" && data.Sign1 == 0)
                                {
                                    return false;
                                }
                                else if (qt.signField == "sign2" && data.Sign2 == 0)
                                {
                                    return false;
                                }
                                else if (qt.signField == "sign3" && data.Sign3 == 0)
                                {
                                    return false;
                                }
                            }

                        }
                    }
                }
                else
                {
                    return false;
                }
                return result;
                
            };
        }

        private void View_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            OnTotal();
        }
        string[] rankingtype_arr = new string[] { "00001", "00010", "00011", "00100", "00101", "00110" };
        string[] ratingCurrent_arr = new string[] { "00001", "00010", "00011", "00100", "00101", "00110" };
        string[] cdcImplitedRating_arr = new string[] { "00001", "00010", "00011", "00100", "00101", "00110", "01000" };

        private void CheckSign(PtExecordersData data, string key, string value, ref bool result, Dictionary<string, List<string>> focusTagDic, string type = null)
        {
            if (key == Consts.ISSUER)
            {
                List<string> binaryList = new List<string>();
                string[] arr = null;
                if (!string.IsNullOrEmpty(data.IssuerCode))
                {
                    arr = data.IssuerCode.Split(CommonConstants.BAR_CHAR, StringSplitOptions.RemoveEmptyEntries);
                }
                if (focusTagDic.ContainsKey(key))
                {
                    foreach (string ic in focusTagDic[key])
                    {
                        ChildTag ct = CommonUtils.GetIssuer(ic);
                        if (ct != null)
                        {
                            binaryList.Add(ct.childTag);
                        }

                    }
                }
                result = result && (!focusTagDic.ContainsKey(key) || focusTagDic[key].Count == 0 || binaryList.Any(item => arr != null && arr.Contains(item)));
                //result = result && (!focusTagDic.ContainsKey(key) || focusTagDic[key].Count == 0 || focusTagDic[key].Any(item => arr != null && arr.Contains(item)));
            }
            else if (key == Consts.GUARANTOR)
            {
                List<string> binaryList = new List<string>();
                string[] arr = null;
                if (!string.IsNullOrEmpty(data.WarranterCode))
                {
                    arr = data.WarranterCode.Split(CommonConstants.BAR_CHAR, StringSplitOptions.RemoveEmptyEntries);
                }
                if (focusTagDic.ContainsKey(key))
                {
                    foreach (string ic in focusTagDic[key])
                    {
                        ChildTag ct = CommonUtils.GetGuarantor(ic);
                        if (ct != null)
                        {
                            binaryList.Add(ct.childTag);
                        }
                    }
                }

                result = result && (!focusTagDic.ContainsKey(key) || focusTagDic[key].Count == 0 || binaryList.Any(item => arr != null && arr.Contains(item)));
                //result = result && (!focusTagDic.ContainsKey(key) || focusTagDic[key].Count == 0 || focusTagDic[key].Any(item => arr != null && arr.Contains(item)));
            }
            else if (key == Consts.DQTYPE)
            {
                string tempDqtype = value;

                bool isme = false;
                /*if (type == "1")
                {
                    isme = data.IsMe_InnerBid;
                }
                else if (type == "2")
                {
                    isme = data.IsMe_InnerOfr;
                }
                else
                {
                    isme = data.IsMe;
                }

                if (_innerType == "2" && isme == false)
                {
                    tempDqtype = "0000";
                }*/
                result = result && (!focusTagDic.ContainsKey(key) || focusTagDic[key].Count == 0 || focusTagDic[key].Contains(tempDqtype));
            }
            else if (key == Consts.CUM_TYPE)
            {
                string cumType = "00";

                if (type == "1")
                {
                    if (data.SignDic_InnerBid1 != null && data.SignDic_InnerBid1.ContainsKey(Consts.CUM_TYPE))
                        cumType = data.SignDic_InnerBid1[Consts.CUM_TYPE];
                }
                else if (type == "2")
                {
                    if (data.SignDic_InnerOfr1 != null && data.SignDic_InnerOfr1.ContainsKey(Consts.CUM_TYPE))
                        cumType = data.SignDic_InnerOfr1[Consts.CUM_TYPE];
                }
                else
                {
                    if (data.SignDic1 != null && data.SignDic1.ContainsKey(Consts.CUM_TYPE))
                        cumType = data.SignDic1[Consts.CUM_TYPE];
                }

                string ifExe = "00";
                if (type == "1")
                {
                    if (data.SignDic_InnerBid3 != null && data.SignDic_InnerBid3.ContainsKey(Consts.IF_EXERCISE))
                        ifExe = data.SignDic_InnerBid3[Consts.IF_EXERCISE];
                }
                else if (type == "2")
                {
                    if (data.SignDic_InnerOfr3 != null && data.SignDic_InnerOfr3.ContainsKey(Consts.IF_EXERCISE))
                        ifExe = data.SignDic_InnerOfr3[Consts.IF_EXERCISE];
                }
                else
                {
                    if (data.SignDic3 != null && data.SignDic3.ContainsKey(Consts.IF_EXERCISE))
                        ifExe = data.SignDic3[Consts.IF_EXERCISE];
                }

                result = result && (!focusTagDic.ContainsKey(key) || focusTagDic[key].Count == 0 || focusTagDic[key].Contains(cumType) || (focusTagDic[key].Contains("11") && ifExe == "10"));
            }
            else if (key == Consts.ORGTYPE)
            {
                result = result && (!focusTagDic.ContainsKey(key) || focusTagDic[key].Count == 0 || focusTagDic[key].Contains(value) || (focusTagDic[key].Contains("0000000000000001") && value == "0000000000000000"));
            }
            else if (key == Consts.BOND_SUB_TYPE)
            {
                int bondSubTypeCount = 0;
                if (focusTagDic.ContainsKey(key))
                {
                    List<string> subBondTypeList = focusTagDic[key].ToList();
                    subBondTypeList.RemoveAll(item => item == "1011" || item == "1100" || item == "1101" || item == "1110" || item == "1111");
                    bondSubTypeCount = subBondTypeList.Count;
                }

                result = result
                    && (!focusTagDic.ContainsKey(key) || bondSubTypeCount == 0 || bondSubTypeCount == focusTagDic[key].Count)
                    && (!focusTagDic.ContainsKey(key) || focusTagDic[key].Count == 0 || focusTagDic[key].Contains(value))
                    && (!focusTagDic.ContainsKey(Consts.BONDTYPE) || focusTagDic[Consts.BONDTYPE].Count == 0 || !focusTagDic.ContainsKey(key) || focusTagDic[Consts.BONDTYPE].Count == focusTagDic[key].Count);
            }
            else if (key == Consts.RANKING_TYPE)
            {
                result = result
                   && (!focusTagDic.ContainsKey(key) || focusTagDic[key].Count == 0) || focusTagDic[key].Contains(value)
                   || (focusTagDic[key].Contains("xx") && !rankingtype_arr.Contains(value) && !"BNC".Equals(data.Symboltype));
            }
            else if (key == Consts.RATING_CURRENT)
            {
                result = result
                   && (!focusTagDic.ContainsKey(key) || focusTagDic[key].Count == 0) || focusTagDic[key].Contains(value)
                   || (focusTagDic[key].Contains("xx") && !ratingCurrent_arr.Contains(value) && !"BNC".Equals(data.Symboltype));
            }
            else if (key == Consts.CDC_IMPLITED_RATING)
            {
                result = result
                   && (!focusTagDic.ContainsKey(key) || focusTagDic[key].Count == 0) || focusTagDic[key].Contains(value)
                   || (focusTagDic[key].Contains("xx") && !cdcImplitedRating_arr.Contains(value) && !"BNC".Equals(data.Symboltype));
            }
            else if (key == Consts.ISSUER_RATING_CURRENT_CBR
                           || key == Consts.CSI_IMPLIED_RATING)
            {
                result = result
                   && (!_focusTagDic.ContainsKey(key) || _focusTagDic[key].Count == 0) || _focusTagDic[key].Contains(value)
                   || (_focusTagDic[key].Contains("01000") && value == "00000" && !"BNC".Equals(data.Symboltype)); // 00000也算其他
            }
            else
            {
                //result = result && (!focusTagDic.ContainsKey(key) || focusTagDic[key].Count == 0 || focusTagDic[key].Contains(InnerBoardFilterPanel_New.ALL) || focusTagDic[key].Contains(value));
                result = result && (!focusTagDic.ContainsKey(key) || focusTagDic[key].Count == 0 || focusTagDic[key].Contains(value));
            }
        }

        #endregion
        public bool getShowEdit(TbookPtExecorders order) 
        {
            if (!"0".Equals(order.Entrytype) && "1".Equals(IsCheckOwn(order.Username)))
                return false;
            if (!(("0".Equals(order.Entrytype) && string.IsNullOrEmpty(order.Username)) || (!string.IsNullOrEmpty(order.Username) && order.Username.Equals(ApiManager.Instance.GetDefaultApi().UserName))) )
                return false;
            if (isCurrentBeforeDate(order.Tradedate, false))
                return false;
            if ("1".Equals(order.Sendstatus))
                return false;
            if ("1".Equals(order.Delflag))
                return false;
            return true;
        }
        public bool getShowSend(TbookPtExecorders order)
        {
            //if ("1".Equals(IsCheckOwn(order.Username, order.Entrytype)))
            //    return false;
            //if (isCurrentBeforeDate(order.Tradedate, true))
            //    return false;
            if ("1".Equals(order.Sendstatus))
                return false;
            if (!"7".Equals(order.Matchstatus))
                return false;
            //if (!"3".Equals(order.Approvalstatus))
            //    return false;
            if ("1".Equals(order.Delflag))
                return false;
            return true;
        }
        public bool getShowDeleted(TbookPtExecorders order)
        {
            if ("1".Equals(IsCheckOwn(order.Username)))
                return false;
            if ("1".Equals(order.Sendstatus))
                return false;
            if (!"5".Equals(order.Matchstatus))
                return false;
            if ("1".Equals(order.Delflag))
                return false;
            if ("0".Equals(order.Entrytype))
                return false;
            return true;
        }
        public string IsCheckOwn(string Username)
        {
            //如果权限是：管理自己&查看全部，如果不是自己的策略，就不能操作
            string isOwn = "0";
            if (ManageYourselfViewAll && (Username == null || !Username.Equals(ApiManager.Instance.GetDefaultApi().UserName)))
            {
                isOwn = "1";
            }
            return isOwn;
        }
        string format = "yyyy-MM-dd";
        string formatyyyyMMdd = "yyyyMMdd";
        string[] formatArr = new[] { "yyyy-MM-dd HH:mm:ss.fff", "yyyy-MM-dd HH:mm:ss", "yyyy-MM-dd" };
        public PtExecOrderQueryReq request;
        private void Instance_OnExec(List<TbookPtExecorders> list)
        {

            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                foreach (TbookPtExecorders order in list)
                {
                    if (request != null)
                    {
                        /*if (!string.IsNullOrEmpty(request.bondType))
                        {
                            if ((request.bondType.Equals("bond") && !"4".Equals(order.Marketindicator)) || (request.bondType.Equals("irs") && !"2".Equals(order.Marketindicator)))
                            {
                                checkIsExistOrder(order);
                                return;
                            }
                        }
                        if (!string.IsNullOrEmpty(request.approvalstatus) && request.approvalstatus != order.Approvalstatus)
                        {
                            checkIsExistOrder(order);
                            return;
                        }
                        if (!string.IsNullOrEmpty(request.institutionid) && request.institutionid != order.Institutionid)
                        {
                            checkIsExistOrder(order);
                            return;
                        }
                        if (!string.IsNullOrEmpty(request.traderid) && request.securityid != order.Traderid)
                        {
                            checkIsExistOrder(order);
                            return;
                        }*/
                        if (!string.IsNullOrEmpty(request.venueexecid) && request.venueexecid != order.Venueexecid)
                        {
                            checkIsExistOrder(order);
                            return;
                        }
                        /*if (!request.send && "1".Equals(order.Sendstatus))
                        {
                            checkIsExistOrder(order);
                            return;
                        }*/

                        if (!string.IsNullOrEmpty(request.securityid) && request.securityid != order.Securityid)
                        {
                            checkIsExistOrder(order);
                            return;
                        }
                        /*if (!string.IsNullOrEmpty(request.settltype))
                        {
                            if ((request.settltype.Equals("F") && !order.Settltype.Contains("+")) || (!request.settltype.Equals("F") && !request.settltype.Equals(order.Settltype)))
                            {
                                checkIsExistOrder(order);
                                return;
                            }
                        }*/
                        if (!string.IsNullOrEmpty(request.userId) && LocalEnv.UserInfo.UserId != order.Userid)
                        {
                            checkIsExistOrder(order);
                            return;
                        }
                        if (!string.IsNullOrEmpty(request.createTimeBegin))
                        {
                            DateTime dateTime;
                            DateTime dateTime2;
                            if (DateTime.TryParseExact(request.createTimeBegin, format, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out dateTime) &&
                                DateTime.TryParseExact(order.Createtime, formatArr, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out dateTime2))
                            {
                                if (DateTime.Compare(dateTime.Date, dateTime2.Date) > 0)
                                {
                                    checkIsExistOrder(order);
                                    return;
                                }
                            }
                        }
                        if (!string.IsNullOrEmpty(request.createTimeEnd))
                        {
                            DateTime dateTime;
                            DateTime dateTime2;
                            if (DateTime.TryParseExact(request.createTimeEnd, format, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out dateTime) && 
                                DateTime.TryParseExact(order.Createtime, formatArr, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out dateTime2))
                                if (DateTime.Compare(dateTime.Date, dateTime2.Date) < 0)
                                {
                                    checkIsExistOrder(order);
                                    return;
                                }
                        }
                        if (!string.IsNullOrEmpty(request.tradedateBegin))
                        {
                            DateTime dateTime;
                            DateTime dateTime2;
                            if (DateTime.TryParseExact(request.tradedateBegin, formatyyyyMMdd, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out dateTime) && 
                                DateTime.TryParseExact(order.Tradedate, formatyyyyMMdd, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out dateTime2))
                                if (DateTime.Compare(dateTime.Date, dateTime2.Date) > 0)
                                {
                                    checkIsExistOrder(order);
                                    return;
                                }
                        }
                        if (!string.IsNullOrEmpty(request.tradedateEnd))
                        {
                            DateTime dateTime;
                            DateTime dateTime2;
                            if (DateTime.TryParseExact(request.tradedateEnd, formatyyyyMMdd, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out dateTime) && 
                                DateTime.TryParseExact(order.Tradedate, formatyyyyMMdd, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out dateTime2))
                                if (DateTime.Compare(dateTime.Date, dateTime2.Date) < 0)
                                {
                                    checkIsExistOrder(order);
                                    return;
                                }
                        }
                        if (!string.IsNullOrWhiteSpace(this.user) && !"*".Equals(this.user))
                        {
                            bool isDiscard = false;
                            if (!order.Username.Equals(this.user))
                            {
                                string partymanagerid = order.Partymanagerid;
                                if (string.IsNullOrEmpty(partymanagerid))
                                    isDiscard = true;
                                else
                                {
                                    string[] partymanageridArr = partymanagerid.Split('|');
                                    if (!partymanageridArr.Contains(user))
                                    {
                                        isDiscard = true;
                                    }
                                }
                            }
                            if (isDiscard) 
                            {
                                checkIsExistOrder(order);
                                return;
                            }
                        }
                    }
                    TotalCount++;
                    OnOrder(order,true);
                    refreshTotalBySecurityID(order.Securityid);
                    this.Mv.View?.Refresh();
                }
            }));

        }

        private void checkIsExistOrder(TbookPtExecorders order)
        {
            PtExecordersData dataRow;
            lock (_lockObj)
            {
                dataRow = this.PTOrders.FirstOrDefault(o => o.ExecID == order.Execid);
                if (dataRow != null)
                {
                    this.PTOrders.Remove(dataRow);
                }
            }
        }

        public void OnOrder(TbookPtExecorders order,bool ispublish)
        {
            try
            {
                PtExecordersData dataRow;
                lock (_lockObj)
                {
                    dataRow = this.PTOrders.FirstOrDefault(o => o.ExecID == order.Execid);
                }
                bool isold = false;
                if (dataRow != null)
                {
                    /*if ("1".Equals(order.Delflag))
                    {   //删除
                        lock (_lockObj)
                        {
                            PTOrders.Remove(dataRow);
                        }
                        return;
                    }*/
                    isold = true;
                }
                else
                {
                    dataRow = new PtExecordersData();
                    dataRow.ExecID = order.Execid;
                    dataRow.IsUpdated = false;
                }

                dataRow.ClOrdID = order.Clordid;
                dataRow.Relatestatus = order.Relatestatus;
                dataRow.Sendstatus = order.Sendstatus;
                dataRow.SecurityID = order.Securityid;
                dataRow.Symboltype = "BNC".Equals(order.Symboltype) ? "利率债" : "信用债";
                dataRow.Side = order.Side;
                dataRow.LastPx = string.IsNullOrEmpty(order.Yield) ? "0" : order.Yield;
                dataRow.LastPx_Display = Common.Utils.CommonUtils.FormatPrice(order.Yield + "");// 用于显示的成交价
                dataRow.NetPrice = Common.Utils.CommonUtils.ConvertToString(order.Netprice);
                if (!ispublish)
                {
                    if (!string.IsNullOrEmpty(order.Partymanagername))
                    {
                        var itemsName = order.Partymanagername.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries).Distinct().ToList();
                        dataRow.Partymanagername = string.Join("", itemsName.Select(item => item + "|"));
                    }
                    if (!string.IsNullOrEmpty(order.Partymanagerid))
                    {
                        var items = order.Partymanagerid.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries).Distinct().ToList();
                        dataRow.Partymanagerid = string.Join("", items.Select(item => item + "|"));
                    }
                    dataRow.Matchstatus = order.Matchstatus;
                }
                if (IsInvestor)
                {
                    bool isDisplay = true;
                    if (!LocalEnv.UserInfo.UserName.Equals(order.Username) && !LocalEnv.UserInfo.UserId.Equals(order.Partysubid))
                    {
                        isDisplay = false;
                    }
                    if (isDisplay)
                    {
                        dataRow.LastQty = Common.Utils.CommonUtils.ConvertToString(order.Qty);
                        if (!ispublish)
                        {
                            dataRow.MatchQty = order.Matchqty == null ? "" : CommonUtils.ConvertToString(order.Matchqty);
                        }
                        dataRow.ShowDeleteBtn = getShowDeleted(order) ? "0" : "1";//"0".Equals("0".Equals(IsCheckOwn(order.Username, order.Entrytype)) ? "1".Equals(order.Delflag) ? "1" : "0" : "1") ? "1".Equals(order.Sendstatus) ? "1" : "0" : "1";
                        dataRow.ShowSendBtn = getShowSend(order) ? "0" : "1";/* "0".Equals("0".Equals(IsCheckOwn(order.Username, order.Entrytype)) ?
                            (isCurrentBeforeDate(order.Tradedate, true) ?
                            "1" : ("1".Equals(order.Sendstatus) ?
                            ("3".Equals(order.Approvalstatus) ? "0" : "1") : "0")) : "1") ? 
                            "1".Equals(order.Delflag) ? "1" : "0" : "1";*/
                        dataRow.ShowEditBtn = getShowEdit(order)?"0":"1"; // "0".Equals("0".Equals(IsCheckOwn(order.Username, "1")) ? (isCurrentBeforeDate(order.Tradedate, false) ? "1" : ("1".Equals(order.Sendstatus) ? "1" : "0")) : "1") ? "1".Equals(order.Delflag) ? "1" : "0" : "1";
                        dataRow.ShowHistoryBtn = "0";
                    }
                    else 
                    {
                        dataRow.LastQty = "";
                        dataRow.MatchQty = "";
                        dataRow.ShowDeleteBtn = "1";
                        dataRow.ShowSendBtn = "1";
                        dataRow.ShowEditBtn = "1";
                        dataRow.ShowHistoryBtn = "1";
                        dataRow.Partymanagername = "--";
                        dataRow.Partymanagerid = "--";

                    }
                }
                else
                {
                    dataRow.LastQty = Common.Utils.CommonUtils.ConvertToString(order.Qty);
                    if (!ispublish)
                    {
                        dataRow.MatchQty = order.Matchqty == null ? "" : CommonUtils.ConvertToString(order.Matchqty);
                    }
                    dataRow.ShowDeleteBtn = getShowDeleted(order) ? "0" : "1";
                    dataRow.ShowSendBtn = getShowSend(order) ? "0" : "1";
                    //dataRow.ShowSendBtn_FQ = "0".Equals("0".Equals(IsCheckOwn(order.Username, order.Entrytype)) ? ((dataRow.ShowSendBtn == "0" && LocalEnv.ConfInfo.IsFQ) ? "0" : "1"):"1") ? "1".Equals(order.Delflag) ? "1" : "0" : "1";
                    dataRow.ShowSendBtn_FQ = "1";
                    dataRow.ShowEditBtn = getShowEdit(order) ? "0" : "1";

                    dataRow.ShowHistoryBtn = "0";
                }
                dataRow.ShowRelationBtn = "1".Equals(order.Sendstatus) ? "1" : "0";
                string tradedate = order.Tradedate;
                DateTime tradeTime = CommonUtils.GetDate(tradedate);
                if (tradeTime != null)
                {
                    string formatted = tradeTime.ToString("yyyy-MM-dd");
                    dataRow.TradeDate = formatted;
                }
                else 
                {
                    dataRow.TradeDate = order.Tradedate;
                }
                dataRow.ExecutionTime = order.Transacttime;
                dataRow.SettlType = string.IsNullOrEmpty(order.Settltype) ? "" : order.Settltype;
                dataRow.StrategyOrderId = order.Strategyorderid;
                dataRow.Venueexecid = order.Venueexecid;
                dataRow.Venueid = order.Venueid;
                dataRow.Symbol = order.Symbol;
                dataRow.Symboltype = order.Symboltype;
                dataRow.Tag1 = order.Tag1;
                dataRow.InstitutionName = order.Institutionname;
                dataRow.InstitutionID = order.Institutionid;
                dataRow.TraderName = order.Tradername;
                dataRow.TraderID = order.Traderid;
                dataRow.Venuetypegw = order.Venuetypegw;
                dataRow.Venues = order.Venues;
                dataRow.Quotemethod = order.Quotemethod;
                dataRow.CounterPartyId = order.Counterpartyid;
                dataRow.CounterPartyName = order.Counterpartyname;
                dataRow.Counterpartyfullname = order.Counterpartyfullname;
                dataRow.CounterPartySubId = order.Counterpartysubid;
                dataRow.CounterPartySubName = order.Counterpartysubname;
                dataRow.PartySubName = order.Partysubname;
                dataRow.PartySubId = order.Partysubid;
                if (!ispublish)
                {
                    dataRow.AccountId = order.Accountid;
                    dataRow.AccountName = order.Accountname;
                }
                dataRow.Remark = order.Remark;
                dataRow.UpdateTime = order.Updatetime;
                dataRow.Seatnumber = order.Seatnumber;
                dataRow.Approvalstatus = order.Approvalstatus;
                dataRow.Approvalorderid = order.Approvalorderid;
                dataRow.Approvaltime = order.Approvaltime;
                dataRow.Approvalterminal = order.Approvalterminal;
                dataRow.IsMe = order.Userid == LocalEnv.UserInfo.UserId;
                dataRow.Username = order.Username;
                dataRow.Settldate = order.Settldate;
                if (!string.IsNullOrEmpty(order.Inf1))
                {
                    if (JsonSplit.IsJson(order.Inf1))
                    {
                        ParserInf inf1po = CommonUtils.DeserializeObject<ParserInf>(order.Inf1);
                        if (inf1po != null)
                        {
                            dataRow.Sign = inf1po.Sign;
                            dataRow.Sign1 = inf1po.Sign1;
                            dataRow.Sign2 = inf1po.Sign2;
                            dataRow.Sign3 = inf1po.Sign3;
                        }
                    }
                }
                dataRow.MaturityDays = order.MaturityDays;
                dataRow.IssuerCode = order.IssuerCode;
                dataRow.WarranterCode = order.WarranterCode;
                dataRow.DayOffStr = order.dayOffStr;
                dataRow.Blocknum = order.Blocknum;
                dataRow.Createtime = order.Createtime;
                dataRow.BusinessType = order.BusinessType;
                dataRow.Bookid = order.Bookid;
                dataRow.BookName = order.BookName;
                dataRow.MarketIndicator = order.Marketindicator;
                dataRow.Offsetflag = order.Offsetflag;
                dataRow.FullPrice = Common.Utils.CommonUtils.ConvertToString(order.Dirtyprice);
                dataRow.Realcounterpartyname = order.Realcounterpartyname;
                dataRow.TrdSubType = order.TrdSubType;
                dataRow.TrdType = order.TrdType;
                dataRow.SettlAmount = order.SettlAmount != null ? order.SettlAmount.Value:decimal.Zero;
                dataRow.BridgePaymentFixedRate = order.BridgePaymentFixedRate;
                dataRow.Couponrate = order.Couponrate;
                dataRow.Yield2 = order.Yield2 != null ? order.Yield2.Value:decimal.Zero;
                dataRow.CleanPrice2 = order.CleanPrice2 !=null ?order.CleanPrice2.Value:decimal.Zero;
                dataRow.FullPrice2 = order.FullPrice2 != null ? order.FullPrice2.Value:decimal.Zero;
                dataRow.Yieldtoexecution2 = order.Yieldtoexecution2 != null ? order.Yieldtoexecution2.Value :decimal.Zero;
                dataRow.TrdMatchId = order.TrdMatchId;
                dataRow.AllocText = order.AllocText;
                if (string.IsNullOrEmpty(order.BookName) && !string.IsNullOrEmpty(order.Bookid) && PTExecOrderProcesser.Instance.BookAllDict.ContainsKey(order.Bookid))
                {
                    string bookName = "";
                    if (PTExecOrderProcesser.Instance.BookAllDict.TryGetValue(order.Bookid, out bookName))
                    {
                        dataRow.BookName = bookName;
                    }
                }

                string yieldtoexecution = Common.Utils.CommonUtils.FormatPrice(order.Yieldtoexecution + "");
                if (order.Yieldtoexecution.HasValue&& order.Yieldtoexecution.Value.CompareTo(decimal.Parse("0"))!=0) 
                {
                    dataRow.Yieldtoexecution = yieldtoexecution;
                }
                BondsInfoPo bondsInfoPo = Common.Utils.CommonUtils.GetBondsInfoPoBySecurityID(order.Securityid);
                if (bondsInfoPo != null)
                {
                    if (string.IsNullOrEmpty(bondsInfoPo.RemaingingTime) && "4".Equals(bondsInfoPo.MarketIndicator))
                    {
                        dataRow.RemainingTime = "已到期";
                    }
                    else
                    {
                        dataRow.RemainingTime = bondsInfoPo.RemaingingTime;
                        dataRow.RemaingingTime_convert = CommonUtils.ConvertRemainingTime(bondsInfoPo.RemaingingTime);
                    }
                    if (!string.IsNullOrEmpty(bondsInfoPo.Option_Type) &&
                            ((!bondsInfoPo.Option_Type.Equals("NON") && bondsInfoPo.Option_Date != null) ||
                            (bondsInfoPo.Option_Type.Equals("ETS") && bondsInfoPo.Option_Date == null)))
                    {
                        dataRow.IsExeCdcType = "0";
                    }
                }
                dataRow.Entrytype = "0".Equals(order.Entrytype) ? "接口" : "手工";
                dataRow.Delflag = order.Delflag;
                dataRow.DateFilter = getDateFilter(dataRow);
                dataRow.Riskflag = order.Riskflag;
                dataRow.RiskRemark = order.Riskremark;
                dataRow.Sseinstitutioncode = order.Sseinstitutioncode;
                dataRow.Szedealercode = order.Szedealercode;
                dataRow.Szetradingentitycode = order.Szetradingentitycode;
                dataRow.Szetradercode = order.Szetradercode;
                dataRow.Matchstatus = order.Matchstatus;
                dataRow.Toqq = order.Toqq;
                dataRow.MinChange = CommonUtils.GetMinChangeBySecurityID(order.Securityid);
                if (!isold)
                {
                    lock (_lockObj)
                    {
                        PTOrders.Insert(0, dataRow);
                    }
                }
                else 
                {
                    dataRow.IsUpdated = true;
                }
                //}
            }
            catch (Exception e)
            {
                LogHelper.Error("OnOrder", e);
            }
        }
        private string getDateFilter(PtExecordersData dataRow) 
        {
            string result = "";
            List<string> resultList = new List<string>();
            const string yyyyMMddformat = "yyyyMMdd";
            if (!string.IsNullOrEmpty(dataRow.ExecutionTime)) 
            {
                const string format = "yyyy-MM-dd HH:mm:ss.fff";

                DateTime parsedDate;
                if (DateTime.TryParseExact(dataRow.ExecutionTime, format,
                                                    System.Globalization.CultureInfo.InvariantCulture,
                                                    System.Globalization.DateTimeStyles.None,
                                                    out parsedDate))
                {
                    if (parsedDate.Date == DateTime.Today) 
                    {
                        resultList.Add("0001");
                    }
                }
            }
            if (!string.IsNullOrEmpty(dataRow.TradeDate))
            {
                DateTime parsedDate;
                if (DateTime.TryParseExact(dataRow.TradeDate, yyyyMMddformat,
                                                    System.Globalization.CultureInfo.InvariantCulture,
                                                    System.Globalization.DateTimeStyles.None,
                                                    out parsedDate))
                {
                    if (parsedDate.Date == DateTime.Today)
                    {
                        resultList.Add("0010");
                    }
                    else if (parsedDate.Date == DateTime.Today.AddDays(1))
                    {
                        resultList.Add("0011");
                    }
                    else if (parsedDate.Date > DateTime.Today.AddDays(1))
                    {
                        resultList.Add("0100");
                    }
                }
            }
            if (resultList.Count > 0) 
            {
                result = string.Join("|", resultList);
            }
            return result;
        }
        private bool isCurrentBeforeDate(string tradeDate,bool iscurrent) 
        {
            bool status = false;
            DateTime date;
            if (DateTime.TryParseExact(tradeDate, "yyyyMMdd", null, System.Globalization.DateTimeStyles.None, out date))
            {
                DateTime today = DateTime.Today;
                if (iscurrent)
                {
                    if (date == today)
                    {
                        status = false;
                    }
                    else
                    {
                        status = true;
                    }
                }
                else
                {
                    if (date >= today)
                    {
                        status = false;
                    }
                    else
                    {
                        status = true;
                    }
                }
            }
            else
            {
                status = true;
            }
            return status;
        }
        private bool isCurrentAfterDate(string settlDate)
        {
            bool status = false;
            DateTime date;
            if (DateTime.TryParseExact(settlDate, "yyyyMMdd", null, System.Globalization.DateTimeStyles.None, out date))
            {
                DateTime today = DateTime.Today;
                if (date >= today)
                {
                    status = true;
                }
                else
                {
                    status = false;
                }
            }
            else
            {
                status = false;
            }
            return status;
        }
        private Dictionary<string, string> ParseSign(string signField, long sign)
        {
            Dictionary<string, QuoteTag> quoteTagDic = BaseHandler.Instance.GetQuoteTagDic();
            string a = CommonUtils.ToBinary(sign);
            if (!string.IsNullOrEmpty(a))
            {
                Dictionary<string, string> dic = new Dictionary<string, string>();
                foreach (KeyValuePair<string, QuoteTag> pair in quoteTagDic)
                {
                    if (pair.Value.signField == signField)
                    {
                        dic[pair.Value.tagType] = a.Substring(pair.Value.tagTypeStartIndex, pair.Value.tagTypeLength);
                    }
                }
                return dic;
            }
            return null;
        }

        /// <summary>
        /// 过滤数据
        /// </summary>
        private StringBuilder filterSB = new StringBuilder();
        private Dictionary<string, IList> _FilterDic = new Dictionary<string, IList>();
        public Dictionary<string, IList> FilterDic
        {
            get { return _FilterDic; }
            set 
            {  
                _FilterDic = value;
                this.OnPropertyChanged();
                if (this.Mv != null && this.Mv.View != null)
                {
                    this.Mv.View.Refresh();
                }
            }
        }
        #region 后端请求查询
        private void OnRefreshCommand()
        {
            RefreshQueryData();
        }
        public void RefreshQueryData() 
        {
            if (!this.ptInnerView.init || !checkCreateDateSeleted() || !checkTradeDateSeleted())
            {
                return;
            }
            clear();
            QueryData();
        }
        private void QueryData()
        {
            PtExecOrderQueryReq req = new PtExecOrderQueryReq();
            req.pageNum = PageIndex;
            req.pageSize = PageMaxCount;
            req.securityid = this.SearchText;
            req.venueexecid = this.SearchExecIdText;
            //req.send = SendedIsCheck ? true : false;
            //req.histOrder = HistoryIsCheck ? true : false;
            req.createTimeBegin = this.CreateTimeBegin.HasValue ? $"{this.CreateTimeBegin.Value.ToString("yyyy-MM-dd")}" : "";
            req.createTimeEnd = this.CreateTimeEnd.HasValue ? $"{this.CreateTimeEnd.Value.ToString("yyyy-MM-dd")}" : "";
            req.tradedateBegin = this.TradeTimeBegin.HasValue ? $"{this.TradeTimeBegin.Value.ToString("yyyyMMdd")}" : "";
            req.tradedateEnd = this.TradeTimeEnd.HasValue ? $"{this.TradeTimeEnd.Value.ToString("yyyyMMdd")}" : "";
            /*if (IsInvestor)
            {
                req.partysubid = LocalEnv.UserInfo.UserId;
            }*/
            //req.userId = this.IsMe?LocalEnv.UserInfo.UserId:"";
            ThreadPoolHelper.InvokeThreadPool((o) =>
            {
                Task.Run(() =>
                {
                    ExecOrderQuery(req);
                });
            });
        }

        public bool isInit = false;
        public async void ExecOrderQuery(PtExecOrderQueryReq req)
        {
            RefreshBtnEnable = false;
            if (!string.IsNullOrEmpty(user) && !"*".Equals(user))
                req.userName = user;
            request = req.Clone();
            request.sendstatus = "";
            //bool flag = true;
            //while (flag)
            //{
            try
            {
                PtExecordersReplyPo reply = await ExecOrderQueryAsync(req);
                if (reply == null || reply.Datas == null)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        LoadMaskVisible = Visibility.Collapsed;
                    });
                }
                if (reply != null && reply.Datas != null)
                {
                    Application.Current.Dispatcher.Invoke((Action)(() =>
                    {
                        TotalCount = reply.totalSize;
                        PageControl.RecordCount = TotalCount;
                        PageControl.PageSize = PageMaxCount;
                        if (TotalCount > PageMaxCount)
                        {
                            PageVisible = "true";
                        }
                        else
                        {
                            PageVisible = "false";
                        }
                        foreach (TbookPtExecorders eo in reply.Datas)
                        {
                            OnOrder(eo,false);
                        }
                        //if (reply.Datas.Count < req.pageSize)
                        //{
                        //    flag = false;
                        //}
                    }));
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                    });
                }
                //req.pageNum += 1;

            }
            catch (Exception e)
            {
                //flag = false;
                LogHelper.Error("ExecOrderQuery error:", e);
            }
            finally
            {
                //}
                Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background, (Action)(() =>
                {
                    LoadMaskVisible = Visibility.Collapsed;
                    OnTotal();
                }));

                RefreshBtnEnable = true;

            }
        }
        public async Task<PtExecordersReplyPo> ExecOrderQueryAsync(PtExecOrderQueryReq req)
        {
            PtExecordersReplyPo reply = null;
            await Task.Run(() =>
            {
                reply = PTHandler.Instance.ExecOrderQuery(req);
            });

            return reply;
        }
        #endregion


        #region 手工下发
        private void MannelSendClick()
        {
            if (this.Post_dataGrid != null && this.Post_dataGrid.SelectedItems != null && this.Post_dataGrid.SelectedItems.Count > 0)
            {
                bool result = Common.View.Component.CustomMessageBox.Question1(sendMsg, "手工下发", "", "确认");
                if (result == true)
                {
                    PtExecOrderSaveReq req = new PtExecOrderSaveReq();
                    IEnumerable<PtExecordersData> list = (this.Post_dataGrid.SelectedItems as IList).Cast<PtExecordersData>();
                    foreach (PtExecordersData pt in list)
                    {
                        if (!"1".Equals(pt.Sendstatus))
                        {
                            req.execIdList.Add(pt.ExecID);
                        }
                    }
                    req.isSend = true;
                    ThreadPool.QueueUserWorkItem(o =>
                    {
                        PtExecSendReplyPo reply = PTHandler.Instance.ExecOrderSend(req);
                        Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                        {
                            //if (reply != null && "0000".Equals(reply.Code))
                            //{
                            //    string msg = "";
                            //    if (!string.IsNullOrEmpty(msg))
                            //    {
                            //        showMessage(msg);
                            //    }
                            //    else
                            //    {
                            //        showMessage("手工下发成功!");
                            //    }
                            //}
                            //else
                            //{
                            //    showMessage("手工下发失败:" + (reply != null ? reply.Msg : ""));
                            //}
                        }));
                    });
                }
            }
            else 
            {
                CustomMessageBox.Info("未选中需要手工下发的记录!", "手工下发");
            }
            /*BatchManualSendView sendView = new BatchManualSendView();
            sendView.setSales(isSales);
            WindowManager.Instance.OpenWindow("手工下发", sendView);*/
        }
        private void CancelSendClick() 
        {
            if (this.Post_dataGrid != null && this.Post_dataGrid.SelectedItems != null && this.Post_dataGrid.SelectedItems.Count > 0)
            {
                bool result = Common.View.Component.CustomMessageBox.Question("确定要撤销下发吗？", "撤销下发");
                if (result == true)
                {
                    PtExecOrderSaveReq req = new PtExecOrderSaveReq();
                    IEnumerable<PtExecordersData> list = (this.Post_dataGrid.SelectedItems as IList).Cast<PtExecordersData>();
                    foreach (PtExecordersData pt in list)
                    {
                        if ("1".Equals(pt.Sendstatus))
                        {
                            req.execIdList.Add(pt.ExecID);
                        }
                    }
                    req.isSend = false;
                    ThreadPool.QueueUserWorkItem(o =>
                    {
                        PtExecSendReplyPo reply = PTHandler.Instance.ExecOrderSend(req);
                        Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                        {
                            /*if (reply != null && "0000".Equals(reply.Code))
                            {
                                string msg = "";
                                if (!string.IsNullOrEmpty(msg))
                                {
                                    showMessage(msg);
                                }
                                else
                                {
                                    showMessage("撤销下发成功!");
                                }
                            }
                            else
                            {
                                showMessage("撤销下发失败:" + (reply != null ? reply.Msg : ""));
                            }*/
                        }));
                    });
                }
            }
            else
            {
                CustomMessageBox.Info("未选中需要撤销下发的记录!", "撤销下发");
            }
        }
        #endregion

        #region 录入
        public void EnterClick()
        {
            //if (LocalEnv.ConfInfo.IsPTBrokerExec)
            {
                ExecManualEntry manual = new ExecManualEntry(true,null);
                WindowManager.Instance.OpenWindow("ExecManualEntry_手工录入", manual);
            }
            /*else
            {
                ManualEntry manual = new ManualEntry();
                manual.IsAdd = true;
                manual.isSales = isSales;
                manual.SetTradeStartDate();
                WindowManager.Instance.OpenWindow("ManualEntry_手工录入", manual);
            }*/

        }
        #endregion

        #region 发送
        private bool checkSendField(PtExecordersData drv)
        {
            if (isSales)
            {
                if (string.IsNullOrEmpty(drv.InstitutionID))
                {
                    CustomMessageBox.Info("缺少客户机构!", "下发");
                    return false;
                }
                if (string.IsNullOrEmpty(drv.TraderID))
                {
                    CustomMessageBox.Info("缺少客户!", "下发");
                    return false;
                }
            }
            if (string.IsNullOrEmpty(drv.BusinessType))
            {
                CustomMessageBox.Info("缺少业务类别!", "下发");
                return false;
            }
            return true;
        }

        public void SendClick()
        {
            PtExecordersData drv = this.SelectedItem;
            bool result = Common.View.Component.CustomMessageBox.Question1(sendMsg, "手工下发", "", "确认");
            if (result == true)
            {
                /*if (!checkSendField(drv))
                    return;*/
                PtExecOrderSaveReq req = new PtExecOrderSaveReq();
                req.execIdList.Add(drv.ExecID);
                req.isSend = true;
                ThreadPool.QueueUserWorkItem(o =>
                {
                    PtExecSendReplyPo reply = PTHandler.Instance.ExecOrderSend(req);
                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        /*if (reply != null && "0000".Equals(reply.Code))
                        {
                            string msg = "";
                            List<string> unSendOrder = new List<string>();
                            if (reply.Datas.ContainsKey("unSendOrder") && reply.Datas.TryGetValue("unSendOrder", out unSendOrder) && unSendOrder.Count() > 0)
                            {
                                string unsendmsg = string.Join("\n", unSendOrder);
                                msg = string.Format("下发未成功:\n{0}", unsendmsg);
                            }
                            List<string> sendOrder = new List<string>();
                            if (reply.Datas.ContainsKey("sendOrder") && reply.Datas.TryGetValue("sendOrder", out sendOrder) && sendOrder.Count() > 0)
                            {
                                string sendmsg = string.Join("\n", sendOrder);
                                msg = string.Format("已成功:\n{0}", sendmsg);
                            }
                            if (!string.IsNullOrEmpty(msg))
                            {
                                showMessage(msg);
                            }
                            else
                            {
                                showMessage("下发成功!");
                            }
                        }
                        else
                        {
                            showMessage("下发失败:" + (reply != null ? reply.Msg : "")) ;
                        }*/
                    }));
                });
            }
        }
        #endregion

        #region 发送FQ
        public void SendFQClick()
        {
            PtExecordersData drv = SelectedItem;
            if (!WindowManager.Instance.IsExist(drv.ExecID + "_FQ"))
            {
                ManualEntry manual = new ManualEntry();
                manual.Title = "推送成交:" + drv.ExecID;
                manual.IsAdd = false;
                manual.drv = drv;
                manual.isSales = isSales;
                manual.IsSendFQ = true;
                WindowManager.Instance.OpenWindow(drv.ExecID + "_FQ", manual);
            }
        }

        #endregion

        #region 修改事件
        private void EditClick()
        {
            PtExecordersData drv = SelectedItem;
            /*if (LocalEnv.ConfInfo.IsPTBrokerExec)
            {*/
                ExecManualEntry manual = new ExecManualEntry(false, drv);
                manual.Title = "修改成交:" + drv.ExecID;
                manual.IsAdd = false;
                manual.drv = drv;
                manual.isSales = isSales;
                WindowManager.Instance.OpenWindow("ExecManualEntry_" + drv.ExecID, manual);
            /*}
            else
            {
                ManualEntry manual = new ManualEntry();
                manual.Title = "修改成交:" + drv.ExecID;
                manual.IsAdd = false;
                manual.drv = drv;
                manual.isSales = isSales;
                WindowManager.Instance.OpenWindow("ExecManualEntry_" + drv.ExecID, manual);
            }*/
        }
        #endregion
        #region 线下成交
        private void OnShow(DataRowView obj)
        {
            if (obj != null)
                OrderEditClick(obj);
        }
        private void OrderEditClick(DataRowView drv)
        {
            ManualEntry manual = new ManualEntry();
            manual.Title = "线下成交";
            manual.IsAdd = false;
            manual.OrderDrv = drv;
            manual.isSales = isSales;
            WindowManager.Instance.OpenWindow("ExecManualEntry_" + drv[MyOrderProcesser.KEY_ORDER_FIELD]+"", manual);
        }
        #endregion
        #region 订单弹出成交录入

        #endregion

        #region 删除事件
        private StringBuilder deleteWarnMsg = new StringBuilder();
        private void DeleteClick()
        {
            string text = "确定删除选中的成交吗？";
            string title = "删除";
            if (CustomMessageBox.Question(text, title) == true)
            {
                PtExecordersData drv = this.SelectedItem;
                string exid = drv.ExecID + "";
                PtExecOrderSaveReq req = new PtExecOrderSaveReq();
                req.execid = exid;
                ThreadPool.QueueUserWorkItem(o =>
                {
                    PResult reply = PTHandler.Instance.ExecOrderDelete(req);
                    if (reply == null || "0000".Equals(reply.Code))
                    {

                    }
                });
            }
        }
        #endregion

        #region clear
        public void clear()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                PageIndex = 0;
                if (PageControl != null)
                {
                    PageControl.Page = 1;
                }
                if (isInit)
                {
                    PTOrders.Clear();
                    if (this.Mv.View != null)
                        this.Mv.View.Refresh();
                }
                LoadMaskVisible = Visibility.Visible;
            });
        }
        #endregion

        #region 提示消息弹框
        /// <summary>
        /// 提示消息弹框
        /// </summary>
        /// <param name="message"></param>
        public void showMessage(string message)
        {
            SuccessBoxView me = new SuccessBoxView();
            me.setWarnMsg(message);
            me.ShowDialog();
        }
        #endregion
        #region 导出事件
        private async void ExportExcel()
        {
            try
            {
                //创建一个保存文件式的对话框
                System.Windows.Forms.SaveFileDialog sfd = new System.Windows.Forms.SaveFileDialog();                              //设置这个对话框的起始保存路径
                sfd.InitialDirectory = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                sfd.RestoreDirectory = true;
                sfd.FileName = $"交易记录导出_{DateTime.Now.ToString("yyyy_MM_dd")}";
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
                        Prompt2Seconds prompt = new Prompt2Seconds();
                        prompt.Owner = Application.Current.MainWindow;
                        prompt.PromptTitle = "批量导出";
                        prompt.PromptContext = "当前只支持xls文件和xlsx文件";
                        prompt.Show();
                        return;
                    }

                    List<string> errorList = await Sub_ExportExcel(sfd.FileName, false);

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

                if (string.IsNullOrEmpty(promptTxt) == false)
                {
                    Prompt2Seconds prompt = new Prompt2Seconds();
                    prompt.Owner = Application.Current.MainWindow;
                    prompt.PromptTitle = "批量导出";
                    prompt.PromptContext = promptTxt;
                    prompt.Show();
                }
            }
            catch (Exception e)
            {
                Prompt2Seconds prompt = new Prompt2Seconds();
                prompt.Owner = Application.Current.MainWindow;
                prompt.PromptTitle = "批量导出";
                prompt.PromptContext = e.Message;
                prompt.Show();

                LogHelper.Error(e);
            }
        }
        private async void ExportExcelHis()
        {
            try
            {
                //创建一个保存文件式的对话框
                System.Windows.Forms.SaveFileDialog sfd = new System.Windows.Forms.SaveFileDialog();                              //设置这个对话框的起始保存路径
                sfd.InitialDirectory = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                sfd.RestoreDirectory = true;
                sfd.FileName = $"交易审批历史导出_{DateTime.Now.ToString("yyyy_MM_dd")}";
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
                        Prompt2Seconds prompt = new Prompt2Seconds();
                        prompt.Owner = Application.Current.MainWindow;
                        prompt.PromptTitle = "批量导出";
                        prompt.PromptContext = "当前只支持xls文件和xlsx文件";
                        prompt.Show();
                        return;
                    }

                    List<string> errorList = await Sub_ExportExcel(sfd.FileName,true);

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

                if (string.IsNullOrEmpty(promptTxt) == false)
                {
                    Prompt2Seconds prompt = new Prompt2Seconds();
                    prompt.Owner = Application.Current.MainWindow;
                    prompt.PromptTitle = "批量导出";
                    prompt.PromptContext = promptTxt;
                    prompt.Show();
                }
            }
            catch (Exception e)
            {
                Prompt2Seconds prompt = new Prompt2Seconds();
                prompt.Owner = Application.Current.MainWindow;
                prompt.PromptTitle = "批量导出";
                prompt.PromptContext = e.Message;
                prompt.Show();

                LogHelper.Error(e);
            }
        }

        private Task<List<string>> Sub_ExportExcel(string filePath,bool isapprovalhis)
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
                    Dictionary<string, DataTable> dataDic = new Dictionary<string, DataTable>();

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        DataTable dt = NewGenterateExeclToDataTable(isapprovalhis);
                        string name = "";
                        if (isapprovalhis)
                        {
                            name = "审批历史";
                        }
                        else 
                        {
                            name = "交易记录";
                        }
                        dataDic[name] = dt;
                        Dictionary<string, int> result = helper.DataTableToExcel(dataDic, true);
                        if (result == null) 
                            errorlist.Add("无数据导出");

                    });
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
        private static bool ShouldSkip(string columnName) =>"序号".Equals(columnName) || "操作".Equals(columnName);
        static FormatDateTimeConvertor formatDateTimeConvertor = new FormatDateTimeConvertor();
        Dictionary<string, Func<PtExecordersData, object>> valueResolvers = new Dictionary<string, Func<PtExecordersData, object>>
        {
            ["录入方式"] = o => o.Entrytype,
            ["渠道"] = o => ComboSourceUtil.Instance.FindEnum(o.Venues, "venues_exept"),
            ["成交日"] = o => formatDateTimeConvertor.Convert(o.ExecutionTime, null, "yyyy-MM-dd",null),
            ["代码"] = o => o.SecurityID,
            ["简称"] = o => o.Symbol,
            ["剩余期限"] = o => o.RemainingTime,
            ["方向"] = o => ComboSourceUtil.Instance.FindEnum(o.Side, "side_pt"),
            ["风控检查标记"] = o => ComboSourceUtil.Instance.FindEnum(o.Riskflag, "riskflag"),
            ["风控检查备注"] = o => o.RiskRemark,
            ["匹配状态"] = o => ComboSourceUtil.Instance.FindEnum(o.Matchstatus, "matchstatus"),
            ["下发交易指令状态"] = o => ComboSourceUtil.Instance.FindEnum(o.Sendstatus, "sendstatus"),
            ["投资经理"] = o => o.Partymanagername,
            ["成交价(%)"] = o => o.LastPx,
            ["代付费率"] = o => o.BridgePaymentFixedRate,
            ["过券费率"] = o => o.Couponrate,
            ["费后收益率(%)"] = o => o.Yield2,
            ["费收净价(元)"] = o => o.CleanPrice2,
            ["费收全价(元)"] = o => o.FullPrice2,
            ["结算金额(元)"] = o => o.SettlAmount,
            ["成交量(万)"] = o => o.LastQty,
            ["已匹配量(万)"] = o => o.MatchQty,
            ["交易日期"] = o => o.TradeDate,
            ["清算速度"] = o => o.SettlType,
            ["发单对手方"] = o => o.CounterPartyName,
            ["对手交易员"] = o => o.CounterPartySubName,
            ["上交所机构代码"] = o => o.Sseinstitutioncode,
            ["深交所-交易商代码"] = o => o.Szedealercode,
            ["深交所-交易主体代码"] = o => o.Szetradingentitycode,
            ["深交所-交易员代码"] = o => o.Szetradercode,
            ["终端对手方"] = o => o.Realcounterpartyname,
            ["本方交易员"] = o => o.PartySubName,
            ["费后行权收益率"] = o => o.Yieldtoexecution2,
            ["约定号"] = o => o.TrdMatchId,
            ["状态"] = o => ComboSourceUtil.Instance.FindEnum(o.Delflag, "delflag"),
            ["债券类型"] = o => o.Symboltype,
            ["净价(元)"] = o => o.NetPrice,
            ["自动生产的备注"] = o => o.AllocText,
            ["我的备注"] = o => o.Remark,
            ["客户机构"] = o => o.InstitutionName,
            ["投组账户"] = o => string.Format("{0}-{1}", o.AccountId, o.AccountName),
            ["清算日期"] = o => o.Settldate,
            ["业务类别"] = o => ComboSourceUtil.Instance.FindEnum(o.BusinessType, "businesstype"),
            ["组合"] = o => o.BookName,
            ["审批状态"] = o => ComboSourceUtil.Instance.FindEnum(o.Approvalstatus, "Approvalstatus"),
            ["审批单号"] = o => o.Approvalorderid,
            ["审批系统"] = o => o.Approvalterminal,
            ["成交时间"] = o => formatDateTimeConvertor.Convert(o.ExecutionTime, null, "yyyy-MM-dd HH:mm:ss", null),
            ["更新时间"] = o => formatDateTimeConvertor.Convert(o.UpdateTime, null, "yyyy-MM-dd HH:mm:ss", null),
            ["成交编号"] = o => o.Venueexecid,
            ["关联号"] = o => o.Blocknum,
            ["STC成交编号"] = o => o.ExecID,
            ["STC订单号"] = o => o.StrategyOrderId,
            ["标签"] = o => ComboSourceUtil.Instance.FindEnum(o.Tag1, "tag1_pt"),
            ["报价编号"] = o => o.Venueid,
            ["订单关联状态"] = o => ComboSourceUtil.Instance.FindEnum(o.Relatestatus, "Relatestatus"),
            ["报价方式"] = o => ComboSourceUtil.Instance.FindEnum(o.Quotemethod, "Quotemethod")
        };
        private DataTable NewGenterateExeclToDataTable(bool isapprovalhis) 
        {
            DataTable dt = new DataTable();
            ObservableCollection<ColumnsInfo> columnsList = Post_dataGrid.GetColumnsInfos();
            if (columnsList == null || (Mv.View == null))
                return dt;
            foreach (ColumnsInfo column in columnsList)
            {
                if (!ShouldSkip(column.ColumnName)&& column.IsVisible)
                    dt.Columns.Add(column.ColumnName);
            }
            foreach (PtExecordersData order in Mv.View)
            {
                DataRow newDR = dt.NewRow();
                foreach (DataColumn column in dt.Columns)
                {
                    string columnName = column.ColumnName;
                    Func<PtExecordersData, object> resolver;
                    if (valueResolvers.TryGetValue(columnName,out resolver))
                    {
                        newDR[columnName] = resolver(order);
                    }
                }
                dt.Rows.Add(newDR);
            }
            return dt;
        }

        private DataTable GenerateExcelDataTable(bool isapprovalhis)
        {

            DataTable dt = new DataTable();
            dt.Columns.Add("债券代码");
            dt.Columns.Add("债券简称");
            dt.Columns.Add("剩余期限");
            dt.Columns.Add("方向");
            dt.Columns.Add("成交价(%)");
            dt.Columns.Add("净价(元)");
            dt.Columns.Add("成交量(万)");
            dt.Columns.Add("清算速度");
            dt.Columns.Add("客户机构");
            dt.Columns.Add("客户");
            dt.Columns.Add("本方交易员");
            dt.Columns.Add("对手方");
            dt.Columns.Add("对手方交易员");
            dt.Columns.Add("投组账号");
            dt.Columns.Add("组合编号");
            dt.Columns.Add("组合名称");
            dt.Columns.Add("渠道");
            dt.Columns.Add("交易日期");
            dt.Columns.Add("清算日期");
            dt.Columns.Add("成交时间");
            dt.Columns.Add("更新时间");
            dt.Columns.Add("成交编号");
            dt.Columns.Add("STC成交编号");
            dt.Columns.Add("订单编号");
            if (isapprovalhis)
            {
                dt.Columns.Add("下发状态");
                dt.Columns.Add("审批状态");
                dt.Columns.Add("系统");
                dt.Columns.Add("审批单号");
                dt.Columns.Add("获取时间");
            }
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (Mv.View != null)
                {
                    if (isapprovalhis)
                    {
                        List<string> req = new List<string>();
                        foreach (PtExecordersData mm in Mv.View)
                        {
                            req.Add(mm.ExecID);
                        }
                        //ThreadPool.QueueUserWorkItem(o =>
                        //{
                        //Application.Current.Dispatcher.BeginInvoke((Action)(async () =>
                        //{
                        PtExecordersReplyPo result = PTHandler.Instance.ExecOrderApprovalHistQuery(req);
                        if (result != null && "0000".Equals(result.Code))
                        {
                            foreach (TbookPtExecorders order in result.Datas)
                            {
                                DataRow newDR = dt.NewRow();
                                newDR["债券代码"] = order.Securityid;
                                newDR["债券简称"] = order.Symbol;
                                BondsInfoPo bondsInfoPo = Common.Utils.CommonUtils.GetBondsInfoPoBySecurityID(order.Securityid);
                                if (bondsInfoPo != null)
                                {
                                    newDR["剩余期限"] = bondsInfoPo.RemaingingTime;
                                }
                                newDR["方向"] = ComboSourceUtil.Instance.FindEnum(order.Side, "side");
                                newDR["成交价(%)"] = order.Yield;
                                newDR["净价(元)"] = order.Netprice;
                                newDR["成交量(万)"] = order.Qty;
                                newDR["清算速度"] = order.Settltype;
                                newDR["客户机构"] = order.Institutionname;
                                newDR["客户"] = order.Tradername;
                                newDR["本方交易员"] = order.Partysubname;
                                newDR["对手方"] = order.Counterpartyname;
                                newDR["对手方交易员"] = order.Counterpartysubname;
                                newDR["投组账号"] = order.Accountname;
                                newDR["组合编号"] = order.Bookid;
                                newDR["组合名称"] = order.BookName;
                                newDR["渠道"] = order.Venues;
                                newDR["交易日期"] = order.Tradedate;
                                newDR["清算日期"] = order.Settldate;
                                newDR["成交时间"] = order.Transacttime;
                                newDR["更新时间"] = order.Updatetime;
                                newDR["成交编号"] = order.Venueexecid;
                                newDR["STC成交编号"] = order.Execid;
                                newDR["订单编号"] = order.Strategyorderid;
                                newDR["下发状态"] = ComboSourceUtil.Instance.FindEnum(order.Sendstatus, "sendstatus");
                                newDR["审批状态"] = ComboSourceUtil.Instance.FindEnum(order.Approvalstatus, "approvalstatus");
                                newDR["系统"] = order.Approvalterminal;
                                newDR["审批单号"] = order.Approvalorderid;
                                newDR["获取时间"] = order.Approvaltime;
                                dt.Rows.Add(newDR);
                            }
                        }
                        //}));
                        //});
                    }
                    else
                    {
                        foreach (PtExecordersData order in Mv.View)
                        {
                            DataRow newDR = dt.NewRow();
                            newDR["债券代码"] = order.SecurityID;
                            newDR["债券简称"] = order.Symbol;
                            BondsInfoPo bondsInfoPo = Common.Utils.CommonUtils.GetBondsInfoPoBySecurityID(order.SecurityID);
                            if (bondsInfoPo != null)
                            {
                                newDR["剩余期限"] = bondsInfoPo.RemaingingTime;
                            }
                            newDR["方向"] = ComboSourceUtil.Instance.FindEnum(order.Side, "side");
                            newDR["成交价(%)"] = order.LastPx;
                            newDR["净价(元)"] = order.NetPrice;
                            newDR["成交量(万)"] = order.LastQty;
                            newDR["清算速度"] = order.SettlType;
                            newDR["客户机构"] = order.InstitutionName;
                            newDR["客户"] = order.TraderName;
                            newDR["本方交易员"] = order.PartySubName;
                            newDR["对手方"] = order.CounterPartyName;
                            newDR["对手方交易员"] = order.CounterPartySubName;
                            newDR["投组账号"] = order.AccountName;
                            newDR["组合编号"] = order.Bookid;
                            newDR["组合名称"] = order.BookName;
                            newDR["渠道"] = order.Venues;
                            newDR["交易日期"] = order.TradeDate;
                            newDR["清算日期"] = order.Settldate;
                            newDR["成交时间"] = order.ExecutionTime;
                            newDR["更新时间"] = order.UpdateTime;
                            newDR["成交编号"] = order.Venueexecid;
                            newDR["STC成交编号"] = order.ExecID;
                            newDR["订单编号"] = order.StrategyOrderId;
                            dt.Rows.Add(newDR);
                        }
                    }
                }
            });
            return dt;
        }

        #endregion
        #region 克隆成交
        private void CopyDealEntry()
        {
            PtExecordersData drv = SelectedItem.Copy();
            //drv.ExecID = "";
            drv.BusinessType = "";
            drv.Seatnumber = "";
            drv.Quotemethod = "";
            drv.Remark = "";
            drv.Bookid = "";
            drv.Username = "";
            ManualEntry manual = new ManualEntry();
            manual.Title = "克隆成交";
            manual.IsAdd = true;
            manual.drv = drv;
            manual.isSales = isSales;
            WindowManager.Instance.OpenWindow("ManualEntry_" + drv.ExecID, manual);
        }
        #endregion
        #region 导入事件
        private async void ImportExcel()
        {
            try
            {
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
                        Prompt2Seconds prompt = new Prompt2Seconds();
                        prompt.Owner = Application.Current.MainWindow;
                        prompt.PromptTitle = "批量导入";
                        prompt.PromptContext = "当前只支持xls文件和xlsx文件";
                        prompt.Show();
                        return;
                    }
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
                if (string.IsNullOrEmpty(promptTxt) == false)
                {
                    showMessage(promptTxt);
                }
            }
            catch (Exception e)
            {
                showMessage(e.Message);
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
                    DataTable dt = null;
                    if (dic.ContainsKey(0))
                    {
                        dt = helper.ExcelToDataTable(0);
                    }
                    if (dt != null && dt.Rows.Count > 0)
                    {
                        List<PtExecOrderSaveReq> list = new List<PtExecOrderSaveReq>();
                        int i = 0;
                        foreach (DataRow dr in dt.Rows)
                        {
                            i++;
                            PtExecOrderSaveReq req = new PtExecOrderSaveReq();
                            foreach (DataColumn dc in dt.Columns)
                            {
                                if (dc.ColumnName == "债券代码")
                                {
                                    string securityid = dr[dc.ColumnName] != null ? dr[dc.ColumnName].ConvertToString().Trim() : "";
                                    if (string.IsNullOrEmpty(securityid))
                                    {
                                        errorlist.Add(string.Format("第{0}行债券代码不能为空", i));
                                    }
                                    List<SimpleBondsInfoPo> selectDatas = CommonUtils.GetBondsInfoPo(securityid.ToUpper(), 20, null);
                                    if (selectDatas.Count == 1)
                                    {
                                        req.securityid = selectDatas[0].Bond_ID;
                                    }
                                    else 
                                    {
                                        errorlist.Add(string.Format("第{0}行找不到债券{1}", i, securityid));
                                    }
                                    continue;
                                }
                                if (dc.ColumnName == "债券简称")
                                {
                                    req.symbol = dr[dc.ColumnName] != null ? dr[dc.ColumnName].ConvertToString() : "";
                                    if (string.IsNullOrEmpty(req.symbol))
                                    {
                                        errorlist.Add(string.Format("第{0}行债券简称不能为空", i));
                                    }
                                }
                                if (dc.ColumnName == "方向")
                                {
                                    string side = dr[dc.ColumnName] != null ? dr[dc.ColumnName].ConvertToString():"";
                                    req.side = "Bid".Equals(side) ? "1" : "2";
                                    if (!"Bid".Equals(side) && !"Ofr".Equals(side))
                                    {
                                        errorlist.Add(string.Format("第{0}行方向设置错误，有效取值：Bid 、Ofr", i));
                                    }
                                    continue;
                                }
                                if (dc.ColumnName == "成交价(%)")
                                {
                                    req.price = dr[dc.ColumnName] != null ? dr[dc.ColumnName].ConvertToString() : "";
                                    req.yield = dr[dc.ColumnName] != null ? dr[dc.ColumnName].ConvertToString() : "";
                                    if (!CheckPrice(req.price))
                                    {
                                        errorlist.Add(string.Format("第{0}行成交价(%)设置错误，取值需要在(0, 999999]范围内", i));
                                    }
                                    continue;
                                }
                                if (dc.ColumnName == "净价(元)")
                                {
                                    req.netprice = dr[dc.ColumnName] != null ? dr[dc.ColumnName].ConvertToString() : "";
                                    if (!CheckPrice(req.price))
                                    {
                                        errorlist.Add(string.Format("第{0}行净价(元)设置错误，取值需要在(0, 999999]范围内", i));
                                    }
                                    continue;
                                }
                                if (dc.ColumnName == "成交量(万)")
                                {
                                    req.qty = dr[dc.ColumnName] != null ? dr[dc.ColumnName].ConvertToString() : "";
                                    if (!CheckQty(req.qty))
                                    {
                                        errorlist.Add(string.Format("第{0}行成交量(万)设置错误，取值需要在[1, 9999999]范围内", i));
                                    }
                                    continue;
                                }
                                if (dc.ColumnName == "清算速度")
                                {
                                    req.settltype = dr[dc.ColumnName] != null ? dr[dc.ColumnName].ConvertToString() : "";
                                    if (!"+0".Equals(req.settltype) && !"+1".Equals(req.settltype))
                                    {
                                        errorlist.Add(string.Format("第{0}行清算速度设置错误，有效取值：+0、+1", i));
                                    }
                                    continue;
                                }
                                if (dc.ColumnName == "客户机构")
                                {
                                    string inst = dr[dc.ColumnName] != null ? dr[dc.ColumnName].ConvertToString() : "";
                                    if (!string.IsNullOrEmpty(inst))
                                    {
                                        List<CrmInst> selectDatas = CommonUtils.GetInstitutionsInfoPo(inst, 1, null);
                                        if (selectDatas.Count > 0)
                                        {
                                            req.institutionname = selectDatas[0].shortnamecn;
                                            req.institutionid = selectDatas[0].idbkey;
                                        }
                                        else 
                                        {
                                            errorlist.Add(string.Format("第{0}行客户机构不存在", i));
                                        }
                                    }
                                    continue;
                                }
                                if (dc.ColumnName == "客户")
                                {
                                    string name = dr[dc.ColumnName] != null ? dr[dc.ColumnName].ConvertToString() : "";
                                    if (!string.IsNullOrEmpty(name))
                                    {
                                        List<CrmContact> selectDatas = CommonUtils.GetContactInfoPo(name, "");
                                        if (selectDatas.Count > 0)
                                        {
                                            req.tradername = selectDatas[0].namecn;
                                            req.traderid = selectDatas[0].idbkey;
                                        }
                                        else
                                        {
                                            errorlist.Add(string.Format("第{0}行客户不存在", i));
                                        }
                                    }
                                    continue;
                                }
                                if (dc.ColumnName == "本方交易员")
                                {
                                    req.partysubname = dr[dc.ColumnName] != null ? dr[dc.ColumnName].ConvertToString() : "";
                                    continue;
                                }
                                if (dc.ColumnName == "对手方")
                                {
                                    req.counterpartyname = dr[dc.ColumnName] != null ? dr[dc.ColumnName].ConvertToString() : "";
                                    continue;
                                }
                                if (dc.ColumnName == "对手方交易员")
                                {
                                    req.counterpartysubname = dr[dc.ColumnName] != null ? dr[dc.ColumnName].ConvertToString() : "";

                                    continue;
                                }
                                if (dc.ColumnName == "投组账号")
                                {
                                    req.accountname = dr[dc.ColumnName] != null ? dr[dc.ColumnName].ConvertToString() : "";
                                    continue;
                                }
                                if (dc.ColumnName == "组合编号")
                                {
                                    string book = dr[dc.ColumnName] != null ? dr[dc.ColumnName].ConvertToString() : "";
                                    if (!string.IsNullOrEmpty(book) && PTExecOrderProcesser.Instance.BookDict.ContainsKey(book))
                                    {
                                        string value = "";
                                        if (PTExecOrderProcesser.Instance.BookDict.TryGetValue(book, out value))
                                        {
                                            req.bookname = value;
                                        }
                                        req.bookid = book;
                                    }
                                    else 
                                    {
                                        errorlist.Add(string.Format("第{0}行组合编号不存在", i));
                                    }
                                    continue;
                                }
                                if (dc.ColumnName == "组合名称")
                                {
                                    string book = dr[dc.ColumnName] != null ? dr[dc.ColumnName].ConvertToString() : "";
                                    if (string.IsNullOrEmpty(book) || string.IsNullOrEmpty(req.bookid) || string.IsNullOrEmpty(req.bookname) || (!string.IsNullOrEmpty(req.bookname) && !req.bookname.Equals(book)))
                                    {
                                        errorlist.Add(string.Format("第{0}行组合名称设置错误", i));
                                    }
                                    continue;
                                }
                                if (dc.ColumnName == "渠道")
                                {
                                    req.venues = dr[dc.ColumnName] != null ? dr[dc.ColumnName].ConvertToString() : "";
                                    continue;
                                }
                                if (dc.ColumnName == "交易日期")
                                {
                                    req.tradedate = dr[dc.ColumnName] != null ? dr[dc.ColumnName].ConvertToString() : "";
                                    if (!CheckDate(req.tradedate, "yyyyMMdd"))
                                    {
                                        errorlist.Add(string.Format("第{0}行交易日期格式不正确，格式应为yyyyMMdd", i));
                                    }
                                    continue;
                                }
                                //if (dc.ColumnName == "清算日期")
                                //{
                                //    req.settldate = dr[dc.ColumnName] != null ? dr[dc.ColumnName].ConvertToString() : "";
                                //    if (!CheckDate(req.settldate, "yyyyMMdd"))
                                //    {
                                //        errorlist.Add(string.Format("第{0}行清算日期格式不正确，格式应为yyyyMMdd", i));
                                //    }
                                //    continue;
                                //}
                                //if (dc.ColumnName == "成交时间")
                                //{
                                //    req.transacttime = dr[dc.ColumnName] != null ? dr[dc.ColumnName].ConvertToString() : "";
                                //    req.settldate = dr[dc.ColumnName] != null ? dr[dc.ColumnName].ConvertToString() : "";
                                //    if (!CheckDate(req.settldate, "yyyy-MM-dd HH:mm:ss"))
                                //    {
                                //        errorlist.Add(string.Format("第{0}行成交时间格式不正确，格式应为yyyy-MM-dd HH:mm:ss", i));
                                //    }
                                //    continue;
                                //}
                                if (dc.ColumnName == "成交编号")
                                {
                                    req.venueexecid = dr[dc.ColumnName] != null ? dr[dc.ColumnName].ConvertToString() : "";
                                    continue;
                                }
                            }
                            req.name = LocalEnv.UserInfo.FullName;
                            req.username = LocalEnv.UserInfo.UserName;
                            req.action = "4";
                            list.Add(req);
                        }
                        if (errorlist.Count > 0)
                        {
                            return errorlist;
                        }
                        if (list.Count > 0)
                        {
                            foreach (PtExecOrderSaveReq req in list)
                            {
                                PResult result = PTHandler.Instance.ExecOrderSave(req);
                                if (result == null || !"0000".Equals(result.Code))
                                {
                                    errorlist.Add(result != null ? string.Format("{0}导入失败，原因：{1}", req.venueexecid, result.Msg) : string.Format("{0}导入失败", req.venueexecid));
                                }
                            }
                        }
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
        private bool CheckDate(string date, string formatString)
        {
            DateTime dt;
            if (DateTime.TryParseExact(date, formatString, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out dt))
            {
                return true;
            }
            return false;
        }
        private bool CheckPrice(string basePrice)
        {
            if (string.IsNullOrEmpty(basePrice)
                || !CommonUtils.IsNumber(basePrice, 6, 4)
                || CommonUtils.ConvertToDecimal(basePrice) < 0
                || CommonUtils.ConvertToDecimal(basePrice) > 999999)
            {
                return false;
            }
            return true;
        }
        private bool CheckQty(string BaseQty)
        {
            if (string.IsNullOrEmpty(BaseQty)
                || !CommonUtils.IsNumber(BaseQty, 6, 0)
                || CommonUtils.ConvertToLong(BaseQty) < 1
                || CommonUtils.ConvertToLong(BaseQty) > 9999999)
            {
                return false;
            }
            return true;
        }
        #endregion

        #region 历史记录
        private void HistoryClick()
        {
            if (SelectedItem != null)
            {
                string execid = SelectedItem.ExecID;
                SelectedItem.IsUpdated = false;
                ExecHistoryRecordView view = new ExecHistoryRecordView(execid);
                WindowManager.Instance.OpenWindow("ExecHistoryRecord_" + execid, view);
            }
        }
        #endregion

        #region 成交统计
        public void OnTotal()
        {
            try
            {
                TotalOrders.Clear();
                foreach (PtExecordersData order in mv.View)
                {
                    refreshTotal(order);
                }
            }
            catch (Exception e) 
            {
                LogHelper.Error("OnTotal", e);
            }
            finally
            {
            }
        }
        private void refreshTotal(PtExecordersData order) 
        {
            TotalExecDataPo dataRow = this.TotalOrders.FirstOrDefault(o => o.SecurityID == order.SecurityID);

            bool isold = false;
            if (dataRow != null)
            {
                isold = true;
            }
            else
            {
                dataRow = new TotalExecDataPo();
                dataRow.SecurityID = order.SecurityID;
                dataRow.Symbol = order.Symbol;
            }
            if (!"1".Equals(order.Delflag))
            {
                dataRow.BidLastQty = "1".Equals(order.Side) ? decimal.Add(dataRow.BidLastQty, CommonUtils.ConvertToDecimal(order.LastQty)) : dataRow.BidLastQty;
                dataRow.OfrLastQty = "2".Equals(order.Side) ? decimal.Add(dataRow.OfrLastQty, CommonUtils.ConvertToDecimal(order.LastQty)) : dataRow.OfrLastQty;
                dataRow.NettingLastQty = decimal.Subtract(dataRow.OfrLastQty, dataRow.BidLastQty);
            }
            if (!isold)
            {
                TotalOrders.Insert(0, dataRow);
            }
        }

        private void refreshTotalBySecurityID(string securityID) 
        {
            List<PtExecordersData> list = PTOrders.Where(item => item.SecurityID.Equals(securityID)).ToList();
            TotalExecDataPo dataRow = this.TotalOrders.FirstOrDefault(o => o.SecurityID == securityID);
            if (dataRow != null) 
            {
                dataRow.BidLastQty = 0;
                dataRow.OfrLastQty = 0;
                dataRow.NettingLastQty = 0;
            }
            foreach (PtExecordersData order in list) 
            {
                refreshTotal(order);
            }
        }
        #endregion

        #region 产生个需记录
        private void AddIntentionQuoteClick()
        {
            PtExecordersData drv = SelectedItem.Copy();
            //AddIntentionQuoteView manual = new AddIntentionQuoteView(drv);
            //WindowManager.Instance.OpenWindow("AddIntentionQuote_" + drv.ExecID, manual);
            MatchIntentionView view = new MatchIntentionView(drv, CommonUtils.IsInvestor(), true);
            WindowManager.Instance.OpenWindow("MatchIntention_" + drv.ExecID, view);
        }
        #endregion

        private void RelationClick() 
        {
            if (SelectedItem != null)
            {
                PtExecordersData row = SelectedItem.Copy();
                string execid = row.ExecID;
                MatchIntentionView view = new MatchIntentionView(row,IsInvestor);
                view.ViewClosed += async (sender, args) =>
                {
                    PtExecOrderQueryReq req = new PtExecOrderQueryReq();
                    req.execid = execid;
                    PtExecordersReplyPo reply = await ExecOrderQueryAsync(req);
                    if (reply != null && reply.Datas != null && reply.Datas.Count > 0)
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            OnOrder(reply.Datas[0],false);
                        });
                    }
                };
                WindowManager.Instance.OpenWindow("MatchIntention_" + execid, view);

            }
        }

        private bool checkTradeDateSeleted() 
        {
            if (TradeTimeBegin.HasValue && TradeTimeEnd.HasValue && TimeUtils.Instance.TimeLengthMoreThanDays(TradeTimeBegin.Value, TradeTimeEnd.Value, 30))
            {
                TradeTimeEnd = TradeTimeBegin.Value.AddDays(30);
                //showMessage("界面不支持30天以上交易日期筛选");
                return false;
            }
            return true;
        }
        private bool checkCreateDateSeleted()
        {
            if (CreateTimeBegin.HasValue && CreateTimeEnd.HasValue && TimeUtils.Instance.TimeLengthMoreThanDays(CreateTimeBegin.Value, CreateTimeEnd.Value, 30))
            {
                CreateTimeEnd = CreateTimeBegin.Value.AddDays(30);
                /*showMessage("界面不支持30天以上成交日期筛选");*/
                return false;
            }
            return true;
        }
        public void selecteItemByExecID(string execid) 
        {
            foreach (PtExecordersData order in mv.View)
            {
                if (execid.Equals(order.ExecID)) 
                {
                    SelectedItem = order;
                }
            }
        }
        private bool isSendTradeDateReq = false;
        private void GetSettleTypeFromServer(string settleType,bool isForward)
        {
            try
            {
                isSendTradeDateReq = true;
                CalWeekDateReq requst = new CalWeekDateReq();
                requst.settlementType = settleType;

                CalWeekDateResult result = CalHandler.Instance.calSettlementDate(requst);
                if (result != null && result.Data != null)
                {
                    string tradedate = result.Data.settlementDate;
                    if (isForward)
                    {
                        if (!string.IsNullOrEmpty(tradedate))
                        {
                            this.TradeTimeBegin = CommonUtils.GetDate(tradedate).AddDays(+1);
                            this.TradeTimeEnd = null;
                        }
                        else
                        {
                            this.TradeTimeBegin = DateTime.Now.AddDays(+2);
                            this.TradeTimeEnd = null;
                        }
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(tradedate))
                        {
                            this.TradeTimeBegin = CommonUtils.GetDate(tradedate);
                            this.TradeTimeEnd = CommonUtils.GetDate(tradedate);
                        }
                        else
                        {
                            this.TradeTimeBegin = DateTime.Now.AddDays(+1);
                            this.TradeTimeEnd = DateTime.Now.AddDays(+1);
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
                isSendTradeDateReq = false;
            }
        }

        public void ResetFilterPanel()
        {
            Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background, (Action)(() =>
            {
                //this.Mv.View.Filter = null;
                TimeLimitSelectedIndex = 0;

                TradeTimeBegin = null;
                TradeTimeEnd = null;

                this.ptInnerView.ClearFilter();
                this.SearchText = string.Empty;
            }));
        }
    }

}
