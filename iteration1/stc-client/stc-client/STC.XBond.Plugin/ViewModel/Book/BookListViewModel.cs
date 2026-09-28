using com.gateway.log;
using Microsoft.Win32;
using STC.Common.Api;
using STC.Common.Handler.Admin;
using STC.Common.Handler.Base;
using STC.Common.Handler.BookManagement;
using STC.Common.Model.Base;
using STC.Common.Model.BookManagement;
using STC.Common.Model.Fix.Message;
using STC.Common.Utils;
using STC.XBond.Plugin.Utils;
using STC.XBond.Plugin.View.Book;
using STC.XBond.Plugin.View.Component;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlTypes;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace STC.XBond.Plugin.ViewModel
{
    /// <summary>
    /// BookListView的数据内容
    /// </summary>
    public class BookListViewModel : ViewModelBase
    {
        /// <summary>
        /// 交易包管理模块权限
        /// </summary>
        private readonly string MODULEID = "3010";

        /// <summary>
        /// 查看本人(默认)
        /// （即该用户只可查看本人线下发起的交易包，不可修改、删除、发送）
        /// </summary>
        private readonly string MODULEACTIONID1 = "301001";

        /// <summary>
        /// 查看所有
        /// （即该用户可查看所有交易。不可修改、删除、发送）
        /// </summary>
        private readonly string MODULEACTIONID2 = "301002";

        /// <summary>
        /// 管理
        /// （即该用户具有查看、修改、删除、发送功能）
        /// </summary>
        private readonly string MODULEACTIONID3 = "301003";

        /// <summary>
        /// 是否已初始化
        /// </summary>
        public bool IsInit = false;

        private string _MainFromDateTime;

        public string MainFromDateTime
        {
            get
            {
                return _MainFromDateTime;
            }
            set
            {
                if (_MainFromDateTime != value)
                {
                    _MainFromDateTime = value;
                    this.OnPropertyChanged();
                }
            }
        }


        /// <summary>
        /// 包状态 下拉框选择项
        /// </summary>
        private string _ExecType;

        public string ExecType
        {
            get
            {
                return _ExecType;
            }
            set
            {
                if (_ExecType != value)
                {
                    _ExecType = value;
                    this.OnPropertyChanged();

                    _PageNum = 0;
                    this.RefreshDatas();
                    this.FilterData();
                }
            }
        }

        /// <summary>
        ///  成交方式过滤
        /// </summary>
        private string _Terminal;

        public string Terminal
        {
            get
            {
                return _Terminal;
            }
            set
            {
                if (_Terminal != value)
                {
                    _Terminal = value;
                    this.OnPropertyChanged();

                    _PageNum = 0;
                    this.RefreshDatas();
                    this.FilterData();
                }
            }
        }


        /// <summary>
        /// 多种过滤
        /// </summary>
        private string _MainSearchTextFilterType = "BlockNum";

        public string MainSearchTextFilterType
        {
            get

            {
                return _MainSearchTextFilterType;
            }
            set
            {
                if (_MainSearchTextFilterType != value)
                {
                    this._MainSearchTextFilterType = value;
                    this.OnPropertyChanged();

                    _PageNum = 0;
                    this.RefreshDatas();
                    FilterData();
                }
            }
        }

        private string _MainSearchText = string.Empty;

        public string MainSearchText
        {
            get
            {
                return _MainSearchText;
            }
            set
            {
                if (_MainSearchText != value)
                {
                    _MainSearchText = value;
                    this.OnPropertyChanged();
                    _PageNum = 0;
                    this.RefreshDatas();
                    FilterData();
                }
            }
        }

        /// <summary>
        /// 页码
        /// </summary>
        private int _PageNum = 0;

        public int PageNum
        {
            get
            {
                return _PageNum;
            }
            set
            {
                if (_PageNum != value)
                {
                    _PageNum = value;
                    this.OnPropertyChanged();

                    this.RefreshDatas();
                }
            }
        }

        private int _CurrentPageNum = 0;

        public int CurrentPageNum
        {
            get
            {
                return _CurrentPageNum;
            }
            set
            {
                if (_CurrentPageNum != value)
                {
                    _CurrentPageNum = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private string _SelectBlockNum = string.Empty;

        public string SelectBlockNum
        {
            get
            {
                return _SelectBlockNum;
            }
            set
            {
                //if (_SelectExeID != value)
                //{
                    _SelectBlockNum = value;
                    this.OnPropertyChanged();

                    this.RefreshSubDatas();
                //}
            }
        }

        /// <summary>
        /// 主表总共记录数
        /// </summary>
        private int _TotalCount = 0;

        public int TotalCount
        {
            get
            {
                return _TotalCount;
            }
            set
            {
                _TotalCount = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// 是否显示loading图标
        /// </summary>
        private Visibility _subloadMaskVisiblity = Visibility.Collapsed;

        public Visibility SubloadMaskVisiblity
        {
            get
            {
                return _subloadMaskVisiblity;
            }
            set
            {
                if (_subloadMaskVisiblity != value)
                {
                    _subloadMaskVisiblity = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 是否显示loading图标
        /// </summary>
        private Visibility _loadMaskVisiblity = Visibility.Collapsed;

        public Visibility LoadMaskVisiblity
        {
            get
            {
                return _loadMaskVisiblity;
            }
            set
            {
                if (_loadMaskVisiblity != value)
                {
                    _loadMaskVisiblity = value;
                    this.OnPropertyChanged();
                }
            }
        }


        /// <summary>
        /// 主表导出状态按钮
        /// </summary>
        private bool _ExportEnabled = true;

        public bool ExportEnabled
        {
            get
            {
                return _ExportEnabled;
            }
            set
            {
                if (_ExportEnabled != value)
                {
                    _ExportEnabled = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 子表导出状态按钮
        /// </summary>
        private bool _SubExportEnabled = true;

        public bool SubExportEnabled
        {
            get
            {
                return _SubExportEnabled;
            }
            set
            {
                if (_SubExportEnabled != value)
                {
                    _SubExportEnabled = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 是否显示操作字段
        /// </summary>
        private Visibility _ShowOps = Visibility.Collapsed;

        public Visibility ShowOps
        {
            get
            {
                return _ShowOps;
            }
            set
            {

                    _ShowOps = value;
                    this.OnPropertyChanged();

            }
        }


        //private string _SubSearchTextFilterType = string.Empty;

        //public string SubSearchTextFilterType
        //{
        //    get

        //    {
        //        return _SubSearchTextFilterType;
        //    }
        //    set
        //    {
        //        if (_SubSearchTextFilterType != value)
        //        {
        //            this._SubSearchTextFilterType = value;
        //            this.OnPropertyChanged();
        //            FilterSubData();
        //        }
        //    }
        //}

        //private string _SubSearchText = string.Empty;

        //public string SubSearchText
        //{
        //    get
        //    {
        //        return _SubSearchText;
        //    }
        //    set
        //    {
        //        if (_SubSearchText != value)
        //        {
        //            _SubSearchText = value;
        //            this.OnPropertyChanged();
        //            FilterSubData();
        //        }
        //    }
        //}

        private object mainCurrentPageLock = new object();
        private DataView _MainCurrentPageDV;

        public DataView MainCurrentPageDV
        {
            get
            {
                lock (mainCurrentPageLock)
                {
                    if (_MainCurrentPageDV == null)
                    {
                        _MainCurrentPageDV = BookHandler.Instance.MainBookDV.Table.Clone().DefaultView;

                        _MainCurrentPageDV.Sort = $"{BookHandler.UPDATE_TIME_FIELD } desc";

                        this.OnPropertyChanged();
                    }
                    else {
                        if (string.IsNullOrEmpty(_MainCurrentPageDV.Sort))
                        {
                            _MainCurrentPageDV.Sort = $"{BookHandler.UPDATE_TIME_FIELD } desc";
                        }
                    }
                    return this._MainCurrentPageDV;
                }
            }
        }

        private object subCurrentPageLock = new object();
        private DataView _SubCurrentPageDV;

        public DataView SubCurrentPageDV
        {
            get
            {
                lock (subCurrentPageLock)
                {
                    if (_SubCurrentPageDV == null)
                    {
                        _SubCurrentPageDV = BookHandler.Instance.SubBookDV.Table.Clone().DefaultView;

                        _SubCurrentPageDV.Sort = $"{BookHandler.UPDATE_TIME_FIELD } desc";

                        this.OnPropertyChanged();
                    }

                }
                return this._SubCurrentPageDV;
            }
        }

        private DataRowView _SelectedItem_MainBooks;

        public DataRowView SelectedItem_MainBooks
        {
            get
            {
                return _SelectedItem_MainBooks;
            }
            set
            {
                if (_SelectedItem_MainBooks != value)
                {
                    _SelectedItem_MainBooks = value;
                }
            }
        }

        private DataRowView _Last_MainBooks;

        public DataRowView Last_MainBooks
        {
            get
            {
                return _Last_MainBooks;
            }
            set
            {
                if (_Last_MainBooks != value)
                {
                    _Last_MainBooks = value;
                }
            }
        }

        public ICommand SendCommand { get; set; }

        public ICommand EditCommand { get; set; }

        public ICommand DelCommand { get; set; }

        /// <summary>
        /// 线下补录
        /// </summary>
        public ICommand UnderlineCommand { get; set; }

        /// <summary>
        /// 主表导出
        /// </summary>
        public ICommand ExportCommand { get; set; }

        /// <summary>
        /// 子表导出
        /// </summary>
        public ICommand SubExportCommand { get; set; }

        private ThreadQueue<List<Common.Model.Fix.Message.ExecutionReport>> _ProcessBlocksThreadQueue;

        /// <summary>
        /// 构造函数
        /// </summary>
        public BookListViewModel()
        {
            SetPermission();

            BaseHandler.Instance.Init(Instance_OnClear);
            BookHandler.Instance.OnBlocks += Instance_OnBlocks;

            this.SendCommand = new RelayCommand(SendBook);

            this.EditCommand = new RelayCommand(EditBook);

            this.DelCommand = new RelayCommand(DelBook);

            this.UnderlineCommand = new RelayCommand(Underline);

            this.ExportCommand = new RelayCommand(MainExport);

            this.SubExportCommand = new RelayCommand(DetailExport);

            BindingOperations.EnableCollectionSynchronization(MainCurrentPageDV, mainCurrentPageLock);

            BindingOperations.EnableCollectionSynchronization(SubCurrentPageDV, subCurrentPageLock);

            if (_ProcessBlocksThreadQueue == null)
            {
                _ProcessBlocksThreadQueue = new ThreadQueue<List<Common.Model.Fix.Message.ExecutionReport>>(UpdateBlocks);
            }

            // 设置今日交易包事件
            this.MainFromDateTime = DateTimeEx.ServerTime.ToString("yyyy-MM-dd");
        }

        /// <summary>
        /// 实时交易包数据回调
        /// </summary>
        /// <param name="objList"></param>
        private void Instance_OnBlocks(List<ExecutionReport> objList)
        {
            if (_ProcessBlocksThreadQueue != null)
            {
                bool isAllow = AdminHandler.Instance.CheckPermission(MODULEID, MODULEACTIONID1);
                if (isAllow)
                {
                    bool isexist = false;
                    foreach (ExecutionReport obj in objList)
                    {
                        if (obj.PartySubID.Equals(LocalEnv.UserInfo.UserId) || obj.CounterPartySubID.Equals(LocalEnv.UserInfo.UserId))
                        {
                            isexist = true;
                            break;
                        }
                    }
                    if (isexist)
                    {
                        _ProcessBlocksThreadQueue.Enqueue(objList);
                    }
                }
                else
                {
                    _ProcessBlocksThreadQueue.Enqueue(objList);
                }
            }
        }

        /// <summary>
        /// 更新交易包数据
        /// </summary>
        /// <param name="objList"></param>
        private void UpdateBlocks(List<ExecutionReport> objList)
        {
            try
            {
                if (objList != null && objList.Count > 0)
                {
                    //List<string> usedList = new List<string>();
                    //foreach (ExecutionReport report in objList)
                    //{
                    //    if (report.ParentFlag == "1")
                    //    {
                    //        if (report.SalesAgentFlag == "1")
                    //        {
                    //            //foreach (ExecutionReport subReport in _originalDatas)
                    //            for (int i = objList.Count - 1; i >= 0; i--)
                    //            {

                    //                ExecutionReport subReport = objList[i];
                    //                if (usedList.Contains(subReport.ExecID) == false
                    //                               && (subReport.PartySubID == report.PartySubID || (report.Terminal != "2" && subReport.PartySubID == subReport.UserID))
                    //                               && subReport.Side != report.Side
                    //                               && subReport.SalesAgentFlag == "1"
                    //                               && subReport.ParentFlag != "1")
                    //                {
                    //                    usedList.Add(subReport.ExecID);
                    //                }
                    //            }
                    //        }
                    //    }
                    //}
                    IEnumerable<ExecutionReport> mainExecReportList = objList.Where(item => item.ParentFlag == "1");
                    if (mainExecReportList == null || mainExecReportList.Count() == 0)
                    {
                        return;
                    }
                    ExecutionReport mainExecReport = mainExecReportList.First();
                    if (string.IsNullOrEmpty(mainExecReport.BlockNum))
                    {
                        LogHandler.Info("UpdateBlocks, blockNum is Empty!");
                        return;
                    }
                    if (this.SelectBlockNum == mainExecReport.BlockNum)
                    {
                        // 更新子列表
                        this.SubCurrentPageDV.Table.Rows.Clear();
                        //if (mainExecReport.ActionFlag != "D")
                        //{
                        foreach (ExecutionReport obj in objList)
                        {
                            DataRow dr = this.SubCurrentPageDV.Table.NewRow();
                            dr[BookHandler.BLOCK_NUM_FIELD] = obj.BlockNum;
                            dr[BookHandler.EXEC_ID_FIELD] = obj.ExecID;
                            dr[BookHandler.DEALTYPE_FIELD] = obj.DealType;
                            dr[BookHandler.SIDE_FIELD] = obj.Side;
                            dr[BookHandler.SECURITY_ID_FIELD] = obj.SecurityID;
                            dr[BookHandler.SYMBOL_FIELD] = obj.Symbol;
                            dr[BookHandler.YIELD_FIELD] = CommonUtils.ConvertToDecimal(obj.Yield);
                            dr[BookHandler.NETPRICE_FIELD] = CommonUtils.ConvertToDecimal(obj.NetPrice);
                            dr[BookHandler.DIRTYPRICE_FIELD] = CommonUtils.ConvertToDecimal(obj.DirtyPrice);
                            dr[BookHandler.QTY_FIELD] = CommonUtils.ConvertToDecimal(obj.LastQty);
                            dr[BookHandler.SETTLE_TYPE_FIELD] = obj.SettlType;
                            dr[BookHandler.TRADE_DATE_FIELD] = obj.TradeDate;
                            dr[BookHandler.SETTLE_DATE_FIELD] = obj.SettlType;
                            
                            dr[BookHandler.CREATE_DATE_FIELD] = CommonUtils.ParseTimeString(obj.TransactTime);

                            dr[BookHandler.PARTY_SUB_NAME_FIELD] = obj.PartySubName;
                            dr[BookHandler.COUNTER_PARTY_NAME_FILED] = obj.CounterPartyName;
                            dr[BookHandler.COUNTER_PARTY_SUB_NAME_FIELD] = obj.CounterPartySubName;
                            dr[BookHandler.NUM_FIELD] = obj.RecordNum;
                            dr[BookHandler.CLOSE_BY_FIELD] = obj.UpdateUser;
                            dr[BookHandler.UPDATE_TIME_FIELD] = obj.UpdateTime;
                            dr[BookHandler.SEND_TIME_FIELD] = obj.SendingTime;

                            this._SubCurrentPageDV.Table.Rows.Add(dr);
                        }
                        //}
                    }

                    DataRow findDr = this.MainCurrentPageDV.Table.Rows.Find(mainExecReport.BlockNum);

                    if (findDr == null)
                    {
                        //if (mainExecReport.ActionFlag == "D")
                        //{
                        //    return;
                        //}
                        //else
                        //{
                        // 如果是第一页， 可添加；    可以删除， 更新
                        // 如果非第一页， 不可添加； 可以删除， 更新
                        if (this.PageNum <= 1 && mainExecReport.TransactTime.StartsWith(DateTimeEx.ServerTime.ToString("yyyy-MM-dd")))
                        {
                            findDr = this.MainCurrentPageDV.Table.NewRow();
                            findDr[BookHandler.OPSENABLED_FIELD] = false;
                            findDr[BookHandler.BLOCK_NUM_FIELD] = mainExecReport.BlockNum;
                            this.MainCurrentPageDV.Table.Rows.Add(findDr);
                        }
                        //}
                    }

                    if (findDr != null)
                    {
                        //if (mainExecReport.ActionFlag == "D")
                        //{
                        //    this.MainCurrentPageDV.Table.Rows.Remove(findDr);
                        //    if (this.MainCurrentPageDV.Table.Rows.Count > 0)
                        //    {
                        //        this.MainCurrentPageDV.Table.Rows[0][BookHandler.ISEXPANDED_FIELD] = true;
                        //    }
                        //}
                        //else
                        //{
                        if (mainExecReport.ExecType == "23" || mainExecReport.ExecType == "24")
                        {
                            findDr[BookHandler.OPSENABLED_FIELD] = false;
                        }
                        else
                        {
                            findDr[BookHandler.OPSENABLED_FIELD] = true;
                        }
                        findDr[BookHandler.ISEXPANDED_FIELD] = false;
                        findDr[BookHandler.BLOCK_NUM_FIELD] = mainExecReport.BlockNum;
                        findDr[BookHandler.EXEC_TYPE_FIELD] = mainExecReport.ExecType;
                        findDr[BookHandler.SYNC_FLAG_FIELD] = mainExecReport.SynchFlag;
                        findDr[BookHandler.SIDE_FIELD] = mainExecReport.Side;
                        findDr[BookHandler.SECURITY_ID_FIELD] = mainExecReport.SecurityID;
                        findDr[BookHandler.SYMBOL_FIELD] = mainExecReport.Symbol;
                        findDr[BookHandler.PARTY_SUB_NAME_FIELD] = mainExecReport.PartySubName;
                        findDr[BookHandler.INFO7_FIELD] = mainExecReport.Info7;
                        if (!string.IsNullOrEmpty(mainExecReport.Info7))
                        {
                            findDr[BookHandler.ISTIP_FIELD] = "0";
                        }
                        else {
                            findDr[BookHandler.ISTIP_FIELD] = "1";
                        }
                        if (CommonUtils.ConvertToLong(mainExecReport.ParentFlagQty) != CommonUtils.ConvertToDecimal(mainExecReport.LastQty) && !"25".Equals(mainExecReport.ExecType))
                        {
                            findDr[BookHandler.COUNTER_PARTY_NAME_FILED] = "多家";
                            findDr[BookHandler.COUNTER_PARTY_SUB_NAME_FIELD] = "--";
                            findDr[BookHandler.YIELD_FIELD] = "--";
                            findDr[BookHandler.PRICE_FIELD] = "--";
                            findDr[BookHandler.SETTLE_TYPE_FIELD] = "--";
                            findDr[BookHandler.TRADE_DATE_FIELD] = "--";
                            findDr[BookHandler.SETTLE_DATE_FIELD] = "--";
                            findDr[BookHandler.EXEC_ID_FIELD] = "--";
                        }
                        else
                        {
                            findDr[BookHandler.COUNTER_PARTY_NAME_FILED] = mainExecReport.CounterPartyName;
                            findDr[BookHandler.COUNTER_PARTY_SUB_NAME_FIELD] = mainExecReport.CounterPartySubName;
                            findDr[BookHandler.YIELD_FIELD] = CommonUtils.ConvertToDecimal(mainExecReport.Yield).ToString("N4");
                            findDr[BookHandler.PRICE_FIELD] = CommonUtils.ConvertToDecimal(mainExecReport.Price).ToString("N4");
                            findDr[BookHandler.SETTLE_TYPE_FIELD] = ComboSourceUtil.Instance.FindEnum(mainExecReport.SettlType, "settletype");
                            findDr[BookHandler.TRADE_DATE_FIELD] = mainExecReport.TradeDate;
                            findDr[BookHandler.SETTLE_DATE_FIELD] = mainExecReport.SettlDate;
                            findDr[BookHandler.EXEC_ID_FIELD] = mainExecReport.ExecID;
                        }

                        findDr[BookHandler.CREATE_DATE_FIELD] = CommonUtils.ParseTimeString(mainExecReport.TransactTime);

                        findDr[BookHandler.PARENTFLAGQTY_FIELD] = CommonUtils.ConvertToLong(mainExecReport.ParentFlagQty);
                        findDr[BookHandler.QTY_FIELD] = CommonUtils.ConvertToDecimal(mainExecReport.LastQty);
                        findDr[BookHandler.TERMINAL_FIELD] = mainExecReport.Terminal;
                        findDr[BookHandler.NUM_FIELD] = mainExecReport.RecordNum;
                        findDr[BookHandler.CLOSE_BY_FIELD] = mainExecReport.UpdateUser;
                        findDr[BookHandler.UPDATE_TIME_FIELD] = mainExecReport.UpdateTime;
                        findDr[BookHandler.SEND_TIME_FIELD] = mainExecReport.SendingTime;
                        // }
                    }
                }
            }
            catch(Exception e)
            {
                LogHandler.Error(e);
            }
        }

        /// <summary>
        /// 析构函数
        /// </summary>
        ~BookListViewModel()
        {
            this.Dispose();
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public new void Dispose()
        {
           this.ClearData();

            lock (this.mainCurrentPageLock)
            {
                if (this._MainCurrentPageDV != null)
                {
                    this._MainCurrentPageDV.Table.Dispose();
                    this._MainCurrentPageDV.Dispose();
                    //this._MainCurrentPageDV.Table = null;
                    this._MainCurrentPageDV = null;
                }
            }

            lock (this.subCurrentPageLock)
            {
                if (this._SubCurrentPageDV != null)
                {
                    this._SubCurrentPageDV.Table.Dispose();
                    this._SubCurrentPageDV.Dispose();
                    //this._SubCurrentPageDV.Table = null;
                    this._SubCurrentPageDV = null;
                }
            }
        }

        /// <summary>
        /// 清理数据
        /// </summary>
        public void ClearData()
        {
            this.IsInit = false;

            lock (this.mainCurrentPageLock)
            {
                if (this._MainCurrentPageDV != null)
                {
                    this._MainCurrentPageDV.Table.Clear();
                }
            }

            lock (this.subCurrentPageLock)
            {
                if (this._SubCurrentPageDV != null)
                {
                    this._SubCurrentPageDV.Table.Clear();
                }
            }
        }

        /// <summary>
        /// 清零
        /// </summary>
        /// <param name="obj"></param>
        private void Instance_OnClear(ClearPo obj)
        {
            LogHandler.Info("BookListViewModel Clear");
            if (obj != null && "Y".Equals(obj.IsClear))
            {
                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    this.ClearData();
                    this.Init();


                }));
            }
        }

        /// <summary>
        /// 初始化
        /// </summary>
        public void Init()
        {
            this.IsInit = true;
            // MainCurrentPageDV
            this.MainFromDateTime = DateTimeEx.ServerTime.ToString("yyyy-MM-dd");
            this.PageNum = 0;

            this.RefreshDatas();
            //this.GenerateTestDate();
        }

        /// <summary>
        /// 处理terminal过滤
        /// </summary>
        /// <param name=""></param>
        /// <param name=""></param>
        public void ProcessTerminalFilterConditon(Dictionary<string, IList> filterDic)
        {
            IList terminalList = filterDic["terminal"];
            StringBuilder inFilterSB_terminal = new StringBuilder();
            if (terminalList.Count > 0)
            {
                foreach (ListBoxItem item in terminalList)
                {
                    if (item.Name == "All")
                    {
                        inFilterSB_terminal.Remove(0, inFilterSB_terminal.Length);
                        break;
                    }

                    if (inFilterSB_terminal.Length > 0)
                    {
                        inFilterSB_terminal.Append(",");
                    }

                    if (item.Content.ToString() == Constants.ONLINE)
                    {
                        inFilterSB_terminal.Append("0");
                    }
                    else if (item.Content.ToString() == Constants.VENUES)
                    {
                        inFilterSB_terminal.Append("1");
                    }
                    else if (item.Content.ToString() == Constants.UNDERLINE)
                    {
                        inFilterSB_terminal.Append("2");
                    }
                }
            }

            this.Terminal = inFilterSB_terminal.ToString();

            _PageNum = 0;
            RefreshDatas();
        }

        StringBuilder filter_terminal = new StringBuilder();
        /// <summary>
        ///  处理terminal过滤
        /// </summary>
        /// <param name="filterDic"></param>
        public void ProcessExeTypeFilterConditon(Dictionary<string, IList> filterDic)
        {
            filter_terminal.Remove(0, filter_terminal.Length);
            // venues
            IList terminalList = filterDic["terminal"];
            StringBuilder inFilterSB_terminal = new StringBuilder();
            if (terminalList.Count > 0)
            {
                foreach (ListBoxItem item in terminalList)
                {
                    if (item.Name == "All")
                    {
                        inFilterSB_terminal.Remove(0, inFilterSB_terminal.Length);
                        break;
                    }

                    if (inFilterSB_terminal.Length > 0)
                    {
                        inFilterSB_terminal.Append(",");
                    }

                    if (item.Content.ToString() == Constants.ONLINE)
                    {
                        inFilterSB_terminal.Append("'0'");
                    }
                    else if (item.Content.ToString() == Constants.VENUES)
                    {
                        inFilterSB_terminal.Append("'1'");
                    }
                    else if (item.Content.ToString() == Constants.UNDERLINE)
                    {
                        inFilterSB_terminal.Append("'2'");
                    }
                }
            }

            if (inFilterSB_terminal.Length > 0)
            {
                filter_terminal.Append($"{BookHandler.TERMINAL_FIELD} in ({inFilterSB_terminal})");
            }


            FilterData();
        }

        /// <summary>
        /// 打开明细显示
        /// </summary>
        public void selectBlockBySTCExecID()
        {
            try
            {
                if (SelectedItem_MainBooks != null && this.SelectedItem_MainBooks.Row.RowState != DataRowState.Detached)
                {
                    string newBlockNum = CommonUtils.ConvertToString(this.SelectedItem_MainBooks[BookHandler.BLOCK_NUM_FIELD]);
                    string lastBlockNum = string.Empty;
                    if (this.Last_MainBooks != null &&  this.Last_MainBooks.Row.RowState != DataRowState.Detached)
                    {
                        lastBlockNum = CommonUtils.ConvertToString(this.Last_MainBooks[BookHandler.BLOCK_NUM_FIELD]);
                    }
                    if (newBlockNum != lastBlockNum)
                    {
                        DataRowView tempDRV = this.Last_MainBooks;
                        this.Last_MainBooks = this.SelectedItem_MainBooks;
                        if (tempDRV != null && tempDRV.Row.RowState != DataRowState.Deleted)
                        {
                            tempDRV[BookHandler.ISEXPANDED_FIELD] = false;
                        }
                    }
                    this.SelectBlockNum = newBlockNum;
                    this.SelectedItem_MainBooks[BookHandler.ISEXPANDED_FIELD] = true;

                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
            }
        }

        /// <summary>
        /// 关闭明细显示
        /// </summary>
        public void Collapsed()
        {
            try
            {
                if (SelectedItem_MainBooks != null && this.SelectedItem_MainBooks.Row.RowState != DataRowState.Detached)
                {
                    string newBlockNum = CommonUtils.ConvertToString(this.SelectedItem_MainBooks[BookHandler.BLOCK_NUM_FIELD]);
                    string lastBlockNum = string.Empty;
                    if (this.Last_MainBooks != null)
                    {
                        lastBlockNum = CommonUtils.ConvertToString(this.Last_MainBooks[BookHandler.BLOCK_NUM_FIELD]);
                    }
                    if (newBlockNum == lastBlockNum)
                    {
                        this.SelectedItem_MainBooks[BookHandler.ISEXPANDED_FIELD] = true;
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
            }
        }

        /// <summary>
        /// 过滤主表
        /// </summary>
        public void FilterData()
        {
            StringBuilder filterSB = new StringBuilder();

            // 包状态
            if (string.IsNullOrEmpty(this.ExecType) == false)
            {
                if (filterSB.Length > 0)
                {
                    filterSB.Append(" and ");
                }
                if (this.ExecType == "ALL")
                {
                    // ignore
                }
                else if (this.ExecType == "INCOMPLETE")
                {
                    filterSB.Append($"{BookHandler.EXEC_TYPE_FIELD} in ('20','21','23')");
                }
                else if (this.ExecType == "COMPLETE")
                {
                    filterSB.Append($"{BookHandler.EXEC_TYPE_FIELD} in ('22','8')");
                }
                else
                {
                    filterSB.Append($"{BookHandler.EXEC_TYPE_FIELD} = '{this.ExecType}'");
                }
            }

            // terminal 成交方式
            if (this.filter_terminal.Length > 0)
            {
                if (filterSB.Length > 0)
                {
                    filterSB.Append(" and ");
                }

                filterSB.Append(this.filter_terminal);
            }

            // 按种类筛选
            if (string.IsNullOrEmpty(this.MainSearchText) == false)
            {
                if (filterSB.Length > 0)
                {
                    filterSB.Append(" and ");
                }

                string text = this.MainSearchText.Replace("'", "''").Replace("\"", "''");
                if (this.MainSearchTextFilterType == "BlockNum") // 交易包号
                {
                    filterSB.Append($"{BookHandler.BLOCK_NUM_FIELD} like '%{text}%'");
                }
                else if (this.MainSearchTextFilterType == "ExeID") // STC成交单号
                {
                    filterSB.Append($"{BookHandler.EXEC_ID_FIELD} like '%{text}%'");
                }
                else if (this.MainSearchTextFilterType == "Side") // 方向 
                {
                    string sideType = GetSearchTextByFilterType("Side");
                    filterSB.Append($"{BookHandler.SIDE_FIELD} = '{sideType}'");
                }
                else if (this.MainSearchTextFilterType == "SecurityID") // 债券代码
                {
                    filterSB.Append($"{BookHandler.SECURITY_ID_FIELD} like '%{text}%'");
                }
                else if (this.MainSearchTextFilterType == "UserName") //交易员
                {
                    filterSB.Append($"({BookHandler.PARTY_SUB_NAME_FIELD} like '%{text}%' or  {BookHandler.COUNTER_PARTY_SUB_NAME_FIELD} like '%{text}%')");
                }
            }

            this.MainCurrentPageDV.RowFilter = filterSB.ToString();
        }

        /// <summary>
        /// 发送按钮逻辑
        /// </summary>
        public void SendBook()
        {
            // 重发包
            if (SelectedItem_MainBooks != null && SelectedItem_MainBooks.Row.RowState != DataRowState.Detached)
            {
                string blocknum = CommonUtils.ConvertToString(this.SelectedItem_MainBooks[BookHandler.BLOCK_NUM_FIELD]);
                BlockReSendReq resendReq = new BlockReSendReq();
                resendReq.blocknum = blocknum;
                PResult result = BookMgtHandler.Instance.BlockReSend(resendReq);//ClientApi.Instance.BlockReSend(resendReq);

                if (result.IsSuccess() == false)
                {
                    Common.View.Component.CustomMessageBox.Warning(result.Msg);
                }
                else
                {
                    Common.View.Component.CustomMessageBox.Info("发送成功！");
                }
            }
        }

        /// <summary>
        /// 删除按钮逻辑
        /// </summary>
        public void DelBook()
        {
            // 删除包
            
            if (SelectedItem_MainBooks != null && SelectedItem_MainBooks.Row.RowState != DataRowState.Detached)
            {
                string blocknum = CommonUtils.ConvertToString(this.SelectedItem_MainBooks[BookHandler.BLOCK_NUM_FIELD]);
                if(Common.View.Component.CustomMessageBox.Question($"确定删除当前交易包{blocknum}?"))
                {
                    BlockDeleteReq deleteReq = new BlockDeleteReq();
                    deleteReq.blocknum = blocknum;
                    PResult result = BookMgtHandler.Instance.BlockDelete(deleteReq);

                    if (result.IsSuccess() == false)
                    {
                        Common.View.Component.CustomMessageBox.Warning(result.Msg);
                    }
                }
            }
        }

        /// <summary>
        /// 编辑按钮逻辑
        /// </summary>
        public void EditBook()
        {
            string terminal = CommonUtils.ConvertToString(this.SelectedItem_MainBooks[BookHandler.TERMINAL_FIELD]);
            string blocknum = CommonUtils.ConvertToString(this.SelectedItem_MainBooks[BookHandler.BLOCK_NUM_FIELD]);
            //BookEditView bookEditView;
            //if (terminal == "2")
            //{
            //    bookEditView = new BookEditView(blocknum, true);
            //}
            //else
            //{
            //    bookEditView = new BookEditView(blocknum);
            //}
            //bookEditView.Owner = Application.Current.MainWindow;
            //bookEditView.Show();


            if (!WindowManager.Instance.IsExist(WindowKeyHelper.GetBookWinKey(blocknum)))
            {
                BookEditView bookEditView;
                if (terminal == "2")
                {
                    bookEditView = new BookEditView(blocknum, true);
                }
                else
                {
                    bookEditView = new BookEditView(blocknum);
                }
                bookEditView.Owner = Application.Current.MainWindow;

                WindowManager.Instance.OpenOnlyOneWindow(WindowKeyHelper.GetBookWinKey(blocknum), bookEditView);
                //sendPriceView.Owner = Application.Current.MainWindow;
                //sendPriceView.Show();
            }


        }

        /// <summary>
        /// 线下补入按钮逻辑
        /// </summary>
        public void Underline()
        {
            BookEditView bookEditView = new BookEditView(null, true);
            bookEditView.Owner = Application.Current.MainWindow;
            bookEditView.Show();
        }

        StringBuilder filter_dealType = new StringBuilder();

        /// <summary>
        /// 生成明细表过滤条件
        /// </summary>
        /// <param name="filterDic"></param>
        public void ProcessSubDealTypeFilterCondition(Dictionary<string, IList> filterDic)
        {
            filter_dealType.Remove(0, filter_dealType.Length);
            // venues
            IList dealTypeList = filterDic["dealtype"];
            StringBuilder inFilterSB_dealtype = new StringBuilder();
            if (dealTypeList.Count > 0)
            {
                foreach (ListBoxItem item in dealTypeList)
                {
                    if (item.Name == "All")
                    {
                        inFilterSB_dealtype.Remove(0, inFilterSB_dealtype.Length);
                        break;
                    }

                    if (inFilterSB_dealtype.Length > 0)
                    {
                        inFilterSB_dealtype.Append(",");
                    }

                    if (item.Content.ToString() == Constants.TRANSFERINTERNAL)
                    {
                        inFilterSB_dealtype.Append("'1'");
                    }
                    else if (item.Content.ToString() == Constants.EXCHANGE)
                    {
                        inFilterSB_dealtype.Append("'2'");
                    }
                }
            }

            if (inFilterSB_dealtype.Length > 0)
            {
                filter_dealType.Append($"{BookHandler.DEALTYPE_FIELD} in ({inFilterSB_dealtype})");
            }


            FilterSubData();
        }

        /// <summary>
        /// 过滤明细表数据
        /// </summary>
        public void FilterSubData()
        {
            StringBuilder filterSB = new StringBuilder();
            // 类型
            if (this.filter_dealType.Length > 0)
            {
                filterSB.Append(this.filter_dealType);
            }

            //// 按种类筛选
            //if (string.IsNullOrEmpty(this.SubSearchText) == false)
            //{
            //    if (filterSB.Length > 0)
            //    {
            //        filterSB.Append(" and ");
            //    }

            //    if (this.SubSearchTextFilterType == "BlockNum") // 交易包号
            //    {
            //        filterSB.Append($"{BookHandler.BLOCK_NUM_FIELD} like '%{this.SubSearchText}%'");
            //    }
            //    else if (this.SubSearchTextFilterType == "ExeID") // STC成交单号
            //    {
            //        filterSB.Append($"{BookHandler.EXEC_ID_FIELD} like '%{this.SubSearchText}%'");
            //    }
            //    else if (this.SubSearchTextFilterType == "Side") // 方向 
            //    {
            //        filterSB.Append($"{BookHandler.SIDE_FIELD} like '%{this.SubSearchText}%'");
            //    }
            //    else if (this.SubSearchTextFilterType == "SecurityID") // 债券代码
            //    {
            //        filterSB.Append($"{BookHandler.SECURITY_ID_FIELD} like '%{this.SubSearchText}%'");
            //    }
            //    else if (this.SubSearchTextFilterType == "UserName") //交易员
            //    {
            //        filterSB.Append($"({BookHandler.PARTY_SUB_NAME_FIELD} like '%{this.SubSearchText}%' or  {BookHandler.COUNTER_PARTY_SUB_NAME_FIELD} like '%{this.SubSearchText}%')");
            //    }
            //}

            this.SubCurrentPageDV.RowFilter = filterSB.ToString();

        }
        bool isflag = false;

        /// <summary>
        /// 刷新主表数据
        /// </summary>
        public async void RefreshDatas()
        {
            if (IsInit == false)
            {
                return;
            }
            this.LoadMaskVisiblity = Visibility.Visible;
            this.MainCurrentPageDV.Table.Rows.Clear();

            await Task.Run(() =>
            {
                try
                {
                    BlockQueryReq request = new BlockQueryReq();
                    // 开始日期 ，结束日期
                    request.dealTimeBegin = DateTimeEx.ServerTime.ToString("yyyyMMdd");
                    request.dealTimeEnd = DateTimeEx.ServerTime.ToString("yyyyMMdd");

                    // 包状态
                    if (this.ExecType != "ALL")
                    {
                        request.exectype = this.ExecType;
                    }

                    // 成交方式
                    request.terminal = this.Terminal;

                    // 多种过滤方式
                    request.blocknum = GetSearchTextByFilterType("BlockNum");
                    request.execid = GetSearchTextByFilterType("ExeID");
                    request.side = GetSearchTextByFilterType("Side");
                    request.securityid = GetSearchTextByFilterType("SecurityID");
                    request.partysubname = GetSearchTextByFilterType("UserName");

                    // 页号
                    request.pageNum = this.PageNum;
                    request.pageSize = 200;
                    bool isAllow = AdminHandler.Instance.CheckPermission(MODULEID, MODULEACTIONID1);
                    if (isAllow)
                    {
                        request.userid = LocalEnv.UserInfo.UserId;
                        request.username = LocalEnv.UserInfo.UserName;
                    }

                    BlockQueryReplyPo blockQueryReplyPo = BookMgtHandler.Instance.BlockQuery(request);

                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        this.CurrentPageNum = this.PageNum + 1;
                        if (blockQueryReplyPo != null && blockQueryReplyPo.datas != null && blockQueryReplyPo.datas.Count > 0)
                        {
                            this.TotalCount = blockQueryReplyPo.totalSize;
                            
                            foreach (ExecutionReport obj in blockQueryReplyPo.datas)
                            {
                                BlockNumQueryReq request1 = new BlockNumQueryReq();
                                request1.blocknum = obj.BlockNum;
                                request1.pageSize = 5000;
                                request1.pageNum = 0;
                                if (obj.ParentFlag != "1")
                                {
                                    continue;
                                }
                                if (string.IsNullOrEmpty(obj.BlockNum))
                                {
                                    continue;
                                }
                                if (this.MainCurrentPageDV.Table.Rows.Find(obj.BlockNum) != null)
                                {
                                    continue;
                                }

                                DataRow dr = this.MainCurrentPageDV.Table.NewRow();

                                if (obj.ExecType == "23" || obj.ExecType == "24")
                                {
                                    dr[BookHandler.OPSENABLED_FIELD] = false;
                                }
                                else
                                {
                                    dr[BookHandler.OPSENABLED_FIELD] = true;
                                }
                                dr[BookHandler.ISEXPANDED_FIELD] = false;
                                dr[BookHandler.BLOCK_NUM_FIELD] = obj.BlockNum;
                                dr[BookHandler.EXEC_TYPE_FIELD] = obj.ExecType;
                                dr[BookHandler.SYNC_FLAG_FIELD] = obj.SynchFlag;
                                dr[BookHandler.SIDE_FIELD] = obj.Side;
                                dr[BookHandler.SECURITY_ID_FIELD] = obj.SecurityID;
                                dr[BookHandler.SYMBOL_FIELD] = obj.Symbol;
                                dr[BookHandler.PARTY_SUB_NAME_FIELD] = obj.PartySubName;
                                dr[BookHandler.INFO7_FIELD] = obj.Info7;
                                if (!string.IsNullOrEmpty(obj.Info7))
                                {
                                    dr[BookHandler.ISTIP_FIELD] = "0";
                                }
                                else
                                {
                                    dr[BookHandler.ISTIP_FIELD] = "1";
                                }
                                if (CommonUtils.ConvertToLong(obj.ParentFlagQty) != CommonUtils.ConvertToDecimal(obj.LastQty)&& !"25".Equals(obj.ExecType))
                                {
                                    dr[BookHandler.COUNTER_PARTY_NAME_FILED] = "多家";
                                    dr[BookHandler.COUNTER_PARTY_SUB_NAME_FIELD] = "--";
                                    dr[BookHandler.YIELD_FIELD] = "--";
                                    dr[BookHandler.PRICE_FIELD] = "--";
                                    dr[BookHandler.SETTLE_TYPE_FIELD] = "--";
                                    dr[BookHandler.TRADE_DATE_FIELD] = "--";
                                    dr[BookHandler.SETTLE_DATE_FIELD] = "--";
                                    dr[BookHandler.EXEC_ID_FIELD] = "--";
                                }
                                else
                                {
                                    dr[BookHandler.COUNTER_PARTY_NAME_FILED] = obj.CounterPartyName;
                                    dr[BookHandler.COUNTER_PARTY_SUB_NAME_FIELD] = obj.CounterPartySubName;
                                    dr[BookHandler.YIELD_FIELD] = CommonUtils.ConvertToDecimal(obj.Yield).ToString("N4");
                                    dr[BookHandler.PRICE_FIELD] = CommonUtils.ConvertToDecimal(obj.Price).ToString("N4");
                                    dr[BookHandler.SETTLE_TYPE_FIELD] = ComboSourceUtil.Instance.FindEnum(obj.SettlType, "settletype");
                                    dr[BookHandler.TRADE_DATE_FIELD] = obj.TradeDate;
                                    dr[BookHandler.SETTLE_DATE_FIELD] = obj.SettlDate;
                                    dr[BookHandler.EXEC_ID_FIELD] = obj.ExecID;
                                }
                                dr[BookHandler.CREATE_DATE_FIELD] = CommonUtils.ParseTimeString(obj.TransactTime);
                                dr[BookHandler.PARENTFLAGQTY_FIELD] = CommonUtils.ConvertToLong(obj.ParentFlagQty);
                                dr[BookHandler.QTY_FIELD] = CommonUtils.ConvertToDecimal(obj.LastQty);
                                dr[BookHandler.TERMINAL_FIELD] = obj.Terminal;
                                dr[BookHandler.NUM_FIELD] = obj.RecordNum;
                                dr[BookHandler.CLOSE_BY_FIELD] = obj.UpdateUser;
                                dr[BookHandler.UPDATE_TIME_FIELD] = obj.UpdateTime;
                                dr[BookHandler.SEND_TIME_FIELD] = obj.SendingTime;
                                this.MainCurrentPageDV.Table.Rows.Add(dr);
                            }

                            if (this.MainCurrentPageDV.Count > 0)
                            {
                                this.SelectedItem_MainBooks = this.Last_MainBooks = this.MainCurrentPageDV[0];
                                this.SelectedItem_MainBooks[BookHandler.ISEXPANDED_FIELD] = true;
                            }
                        }
                        else
                        {
                            this.TotalCount = 0;
                        }
                        isflag = true;
                        RefreshSubDatas();
                    }));
                }
                catch (Exception e)
                {
                    LogHelper.Error("RefreshDatas", e);
                }
            });

            this.LoadMaskVisiblity = Visibility.Collapsed;
        }

        /// <summary>
        /// 刷新明细表数据
        /// </summary>
        public async void RefreshSubDatas()
        {
            this.SubloadMaskVisiblity = Visibility.Visible;
            this.SubCurrentPageDV.Table.Rows.Clear();

            string blockNum = string.Empty;
            if (this.SelectedItem_MainBooks != null && this._SelectedItem_MainBooks.Row.RowState != DataRowState.Detached)
            {
                blockNum = CommonUtils.ConvertToString(this.SelectedItem_MainBooks[BookHandler.BLOCK_NUM_FIELD]);
            }

            if (string.IsNullOrEmpty(blockNum) == false)
            {
                await Task.Run(() =>
                {
                    try
                    {
                        BlockNumQueryReq request = new BlockNumQueryReq();
                        request.blocknum = blockNum;
                        request.pageSize = 5000;
                        request.pageNum = 0;
                        if (isflag)
                        {
                            request.flag = "1";
                        }
                        else
                        {
                            request.flag = "2";
                        }

                        BlockNumQueryPo blockNumQueryPo = BookMgtHandler.Instance.BlockNumQuery(request);

                        if (blockNumQueryPo != null && blockNumQueryPo.datas != null && blockNumQueryPo.datas.Count > 0)
                        {
                            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                            {
                                foreach (ExecutionReport obj in blockNumQueryPo.datas)
                                {
                                    if (this.SubCurrentPageDV.Table.Rows.Find(obj.ExecID) != null)
                                    {
                                        continue;
                                    }

                                    DataRow dr = this.SubCurrentPageDV.Table.NewRow();
                                    dr[BookHandler.BLOCK_NUM_FIELD] = obj.BlockNum;
                                    dr[BookHandler.EXEC_ID_FIELD] = obj.ExecID;
                                    dr[BookHandler.DEALTYPE_FIELD] = obj.DealType;
                                    dr[BookHandler.SIDE_FIELD] = obj.Side;
                                    dr[BookHandler.SECURITY_ID_FIELD] = obj.SecurityID;
                                    dr[BookHandler.SYMBOL_FIELD] = obj.Symbol;
                                    dr[BookHandler.YIELD_FIELD] = CommonUtils.ConvertToDecimal(obj.Yield);
                                    dr[BookHandler.NETPRICE_FIELD] = CommonUtils.ConvertToDecimal(obj.NetPrice);
                                    dr[BookHandler.DIRTYPRICE_FIELD] = CommonUtils.ConvertToDecimal(obj.DirtyPrice);
                                    dr[BookHandler.QTY_FIELD] = CommonUtils.ConvertToDecimal(obj.LastQty);
                                    dr[BookHandler.SETTLE_TYPE_FIELD] = obj.SettlType;
                                    dr[BookHandler.TRADE_DATE_FIELD] = obj.TradeDate;
                                    dr[BookHandler.SETTLE_DATE_FIELD] = obj.SettlDate;
                                    dr[BookHandler.CREATE_DATE_FIELD] = CommonUtils.ParseTimeString(obj.TransactTime);
                                    dr[BookHandler.PARTY_SUB_NAME_FIELD] = obj.PartySubName;
                                    dr[BookHandler.COUNTER_PARTY_NAME_FILED] = obj.CounterPartyName;
                                    dr[BookHandler.COUNTER_PARTY_SUB_NAME_FIELD] = obj.CounterPartySubName;
                                    dr[BookHandler.NUM_FIELD] = obj.RecordNum;
                                    dr[BookHandler.CLOSE_BY_FIELD] = obj.UpdateUser;
                                    dr[BookHandler.UPDATE_TIME_FIELD] = obj.UpdateTime;
                                    dr[BookHandler.SEND_TIME_FIELD] = obj.SendingTime;

                                    this._SubCurrentPageDV.Table.Rows.Add(dr);
                                }
                            }));
                        }

                        isflag = false;
                    }
                    catch (Exception e)
                    {
                        LogHelper.Error("RefreshSubDatas", e);
                    }
                });
            }

            this.SubloadMaskVisiblity = Visibility.Collapsed;
        }

        /// <summary>
        /// 根据过滤类型， 获取对应的SearchText
        /// </summary>
        /// <param name="filterType"></param>
        /// <returns></returns>
        private string GetSearchTextByFilterType(string filterType)
        {

            if (this.MainSearchTextFilterType.Equals(filterType, StringComparison.OrdinalIgnoreCase))
            {
                if (this.MainSearchTextFilterType.Equals("side", StringComparison.OrdinalIgnoreCase))
                {
                    if (this.MainSearchText == "买"
                       || this.MainSearchText == "买入"
                       || this.MainSearchText == "1"
                       || this.MainSearchText.Equals("Bid", StringComparison.OrdinalIgnoreCase)
                       || this.MainSearchText.Equals("B", StringComparison.OrdinalIgnoreCase)
                       || this.MainSearchText.Equals("Bi", StringComparison.OrdinalIgnoreCase)
                       || this.MainSearchText.Equals("id", StringComparison.OrdinalIgnoreCase)
                        || this.MainSearchText.Equals("d", StringComparison.OrdinalIgnoreCase)
                         || this.MainSearchText.Equals("i", StringComparison.OrdinalIgnoreCase)
                       )
                    {
                        return "1";
                    }
                    else if (this.MainSearchText == "卖"
                        || this.MainSearchText == "卖出"
                        || this.MainSearchText == "1"
                        || this.MainSearchText.Equals("Ofr", StringComparison.OrdinalIgnoreCase)
                        || this.MainSearchText.Equals("O", StringComparison.OrdinalIgnoreCase)
                        || this.MainSearchText.Equals("Of", StringComparison.OrdinalIgnoreCase)
                        || this.MainSearchText.Equals("fr", StringComparison.OrdinalIgnoreCase)
                        || this.MainSearchText.Equals("f", StringComparison.OrdinalIgnoreCase)
                        || this.MainSearchText.Equals("r", StringComparison.OrdinalIgnoreCase))
                    {
                        return "2";
                    }
                    return this.MainSearchText;
                }
                else
                {
                    return this.MainSearchText;
                }
            }
            return string.Empty;
        }

        /// <summary>
        /// 主表导出
        /// </summary>
        private async void MainExport()
        {

            int totalSize = 0;
            int successCount = 0;
            int failCount = 0;

            this.ExportEnabled = false;
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            try
            {
                saveFileDialog.Filter = "(文本文件*.csv)|*.csv";
                saveFileDialog.FileName = DateTimeEx.ServerTime.ToString("yyyyMMddHHmmss") + "_当日交易包母订单记录";
                saveFileDialog.AddExtension = true;
                saveFileDialog.RestoreDirectory = true;
                if ((bool)(saveFileDialog.ShowDialog()))
                {
                    string fileName = saveFileDialog.FileName;
                    using (StreamWriter writer = ExcelHelper.Instance.createStream(fileName))
                    {
                        // 输出表头
                        writer.WriteLine(GetHeaderLine(mainExportDic));

                        // 请求并输出数据
                        BlockQueryReq request = new BlockQueryReq();
                        bool isAllow = AdminHandler.Instance.CheckPermission(MODULEID, MODULEACTIONID1);
                        if (isAllow)
                        {
                            request.userid = LocalEnv.UserInfo.UserId;
                            request.username = LocalEnv.UserInfo.UserName;
                        }
                        // 开始日期 ，结束日期
                        request.dealTimeBegin = DateTimeEx.ServerTime.ToString("yyyyMMdd");
                        request.dealTimeEnd = DateTimeEx.ServerTime.ToString("yyyyMMdd");

                        // 包状态
                        if (this.ExecType != "ALL")
                        {
                            request.exectype = this.ExecType;
                        }

                        // 成交方式
                        request.terminal = this.Terminal;

                        // 多种过滤方式
                        request.blocknum = GetSearchTextByFilterType("BlockNum");
                        request.execid = GetSearchTextByFilterType("ExeID");
                        request.side = GetSearchTextByFilterType("Side");
                        request.securityid = GetSearchTextByFilterType("SecurityID");
                        request.partysubname = GetSearchTextByFilterType("UserName");
                        request.pageSize = 200;

                        await Task.Run(() =>
                        {
                            ExportToCSV(request, writer, mainExportDic, ref totalSize, ref successCount, ref failCount);
                        });
                        // 释放资源
                        writer.Flush();
                        writer.Close();
                    }
                    Common.View.Component.CustomMessageBox.Info($"《{saveFileDialog.FileName}》已保存。总条数：{totalSize}   成功：{successCount}  失败：{failCount}");
                }
            }
            catch (Exception e)
            {
                LogHelper.Error(e);
            }
            finally
            {
                this.ExportEnabled = true;
            }
        }

        private string GetHeaderLine(List<KeyValuePair<string, string>> list)
        {
            StringBuilder headerSb = new StringBuilder();
            foreach(KeyValuePair<string, string> pair in list)
            {
                if (headerSb.Length > 0)
                {
                    headerSb.Append(",");
                }
                headerSb.Append(pair.Value);
            }

            return headerSb.ToString();
        }

        List<KeyValuePair<string, string>> mainExportDic = new List<KeyValuePair<string, string>>()
        {
            new KeyValuePair<string, string>("BlockNum", "交易包号"),
            new KeyValuePair<string, string>("ExecType","包状态"),
            new KeyValuePair<string, string>("SynchFlag","发送状态" ),
            new KeyValuePair<string, string>("Side","方向" ),
            new KeyValuePair<string, string>("SecurityID","代码" ),
            new KeyValuePair<string, string>("Symbol","简称" ),
            new KeyValuePair<string, string>("PartySubName","本方交易员" ),
            new KeyValuePair<string, string>("CounterPartyName","对手交易机构名称" ),
            new KeyValuePair<string, string>("CounterPartySubName","对手交易员" ),
            new KeyValuePair<string, string>("Yield","收益率" ),
            new KeyValuePair<string, string>("LastQty","量(万)" ),
            new KeyValuePair<string, string>("SettlType","清算速度" ),
            new KeyValuePair<string, string>("TradeDate","成交日期" ),
            new KeyValuePair<string, string>("SettlDate","交割日期" ),
            new KeyValuePair<string, string>("ExecID","STC成交单号" ),
            new KeyValuePair<string, string>("Price","STC成交价" ),
            new KeyValuePair<string, string>("Terminal","成交方式" ),
            new KeyValuePair<string, string>("RecordNum","笔数" ),
            new KeyValuePair<string, string>("UpdateUser","最后更新人" ),
            new KeyValuePair<string, string>("UpdateTime","最后更新时间" ),
            new KeyValuePair<string, string>("SendingTime","最后发送时间" ),
        };

        List<KeyValuePair<string, string>> subExportDic = new List<KeyValuePair<string, string>>()
        {
            new KeyValuePair<string, string>("BlockNum","交易包号" ),
            new KeyValuePair<string, string>( "ExecID","成交单号"),
            new KeyValuePair<string, string>( "DealType","类型"),
            new KeyValuePair<string, string>("Side","方向" ),
           new KeyValuePair<string, string>( "SecurityID","代码" ),
           new KeyValuePair<string, string>( "Symbol","简称"),
            new KeyValuePair<string, string>( "Yield","收益率" ),
           new KeyValuePair<string, string>(  "NetPrice","净价" ),
            new KeyValuePair<string, string>(  "DirtyPrice","全价"),
            new KeyValuePair<string, string>(  "LastQty","量(万)"),
           new KeyValuePair<string, string>( "SettlType","清算速度" ),
            new KeyValuePair<string, string>(  "TradeDate","成交日期" ),
           new KeyValuePair<string, string>(  "SettlDate","交割日期" ),
            new KeyValuePair<string, string>(  "PartySubName","本方交易员" ),
           new KeyValuePair<string, string>(  "CounterPartyName","对手交易机构名称" ),
            new KeyValuePair<string, string>( "CounterPartySubName","对手交易员"),
            new KeyValuePair<string, string>(  "UpdateUser","最后更新人" ),
            new KeyValuePair<string, string>(  "UpdateTime","最后更新时间" ),
            new KeyValuePair<string, string>(  "SendingTime","最后发送时间"),
        };

        /// <summary>
        /// 导出csv文件
        /// </summary>
        /// <param name="request"></param>
        /// <param name="writer"></param>
        /// <param name="list"></param>
        /// <param name="totalSize"></param>
        /// <param name="succeedCount"></param>
        /// <param name="failedCount"></param>
        private void ExportToCSV(BlockQueryReq request, StreamWriter writer, List<KeyValuePair<string, string>> list, ref int totalSize, ref int succeedCount, ref int failedCount)
        {
            // 页号
            int index = 0;

            bool flag = true;
            while (flag)
            {
                request.pageNum = index;
                ///获取订单 
               BlockQueryReplyPo reply = BookMgtHandler.Instance.BlockQuery(request);

                if (reply == null)
                {
                    flag = false;
                    break;
                }
                totalSize = reply.totalSize;

                if (reply != null && reply.datas != null)
                {
                    foreach (Common.Model.Fix.Message.ExecutionReport obj in reply.datas)
                    {
                        StringBuilder lineSB = new StringBuilder();
                        try
                        {
                            foreach (KeyValuePair<string, string> pair in list)
                            {
                                string field = pair.Key;

                                object fieldValue = typeof(ExecutionReport).GetProperty(field).GetValue(obj);

                                string value = CommonUtils.ConvertToString(fieldValue);

                                if (field.Equals("DealType", StringComparison.OrdinalIgnoreCase))
                                {
                                    value = ComboSourceUtil.Instance.FindEnum(value, "dealtype");
                                }
                                else if (field.Equals("ExecType", StringComparison.OrdinalIgnoreCase))
                                {
                                    value = ComboSourceUtil.Instance.FindEnum(value, "ExecType");
                                }
                                else if (field.Equals("Side", StringComparison.OrdinalIgnoreCase))
                                {
                                    value = ComboSourceUtil.Instance.FindEnum(value, "Side");
                                }
                                else if (field.Equals("SettlType", StringComparison.OrdinalIgnoreCase))
                                {
                                    value = ComboSourceUtil.Instance.FindEnum(value, "settletype_tooltip");
                                }
                                else if (field.Equals("Terminal", StringComparison.OrdinalIgnoreCase))
                                {
                                    value = ComboSourceUtil.Instance.FindEnum(value, "Terminal");
                                }
                                else if (field.Equals("ExecType", StringComparison.OrdinalIgnoreCase))
                                {
                                    value = ComboSourceUtil.Instance.FindEnum(value, "ExecType");
                                }
                                else if (field.Equals("SynchFlag", StringComparison.OrdinalIgnoreCase))
                                {
                                    value = ComboSourceUtil.Instance.FindEnum(value, "SyncFlag");
                                }

                                if (lineSB.Length > 0)
                                {
                                    lineSB.Append(",");
                                }

                                lineSB.Append($"\"{value}\"");
                            }
                            writer.WriteLine(lineSB);
                            succeedCount++;
                        }
                        catch(Exception e)
                        {
                            LogHelper.Error(e);
                            failedCount++;
                        }

                    }
                    if (reply.datas.Count < request.pageSize)
                    {
                        flag = false;
                    }
                    else
                    {
                        index++;
                    }
                }
            }
        }

        /// <summary>
        /// 明细部分导出
        /// </summary>
        private async void DetailExport()
        {
            int totalSize = 0;
            int successCount = 0;
            int failCount = 0;
            this.SubExportEnabled = false;
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            try
            {
                saveFileDialog.Filter = "(文本文件*.csv)|*.csv";
                saveFileDialog.FileName = DateTimeEx.ServerTime.ToString("yyyyMMddHHmmss") + "_当日交易包子订单记录";
                saveFileDialog.AddExtension = true;
                saveFileDialog.RestoreDirectory = true;
                if ((bool)(saveFileDialog.ShowDialog()))
                {
                    string fileName = saveFileDialog.FileName;
                    using (StreamWriter writer = ExcelHelper.Instance.createStream(fileName))
                    {
                        // 输出表头
                        writer.WriteLine(GetHeaderLine(subExportDic));

                        // 请求并输出数据
                        BlockQueryReq request = new BlockQueryReq();
                        bool isAllow = AdminHandler.Instance.CheckPermission(MODULEID, MODULEACTIONID1);
                        if (isAllow)
                        {
                            request.userid = LocalEnv.UserInfo.UserId;
                            request.username = LocalEnv.UserInfo.UserName;
                        }
                        request.parentflag = "";
                        // 开始日期 ，结束日期
                        request.dealTimeBegin = DateTimeEx.ServerTime.ToString("yyyyMMdd");
                        request.dealTimeEnd = DateTimeEx.ServerTime.ToString("yyyyMMdd");

                        // 包状态
                        if (this.ExecType != "ALL")
                        {
                            request.exectype = this.ExecType;
                        }

                        // 成交方式
                        request.terminal = this.Terminal;

                        // 多种过滤方式
                        string blockNum = GetSearchTextByFilterType("BlockNum");
                        request.blocknum = string.IsNullOrEmpty(blockNum) ? this.SelectBlockNum : blockNum;
                        request.execid = GetSearchTextByFilterType("ExeID");
                        request.side = GetSearchTextByFilterType("Side");
                        request.securityid = GetSearchTextByFilterType("SecurityID");
                        request.partysubname = GetSearchTextByFilterType("UserName");
                        request.pageSize = 200;

                        await Task.Run(() =>
                        {
                            ExportToCSV(request, writer, subExportDic, ref totalSize, ref successCount, ref failCount);
                        });
                        // 释放资源
                        writer.Flush();
                        writer.Close();
                    }

                    Common.View.Component.CustomMessageBox.Info($"《{saveFileDialog.FileName}》已保存。总条数：{totalSize}   成功：{successCount}  失败：{failCount}");
                }
            }
            catch (Exception e)
            {
                LogHelper.Error(e);
            }
            finally
            {
                this.SubExportEnabled = true;
            }
        }

       /// <summary>
       /// 权限设置
       /// </summary>
        private void SetPermission()
        {
            bool isAllow = AdminHandler.Instance.CheckPermission(MODULEID, MODULEACTIONID3);
            if (isAllow)
            {
                ShowOps = Visibility.Visible;
            }
            else
            {
                ShowOps = Visibility.Collapsed;
            }
        }


        #region 测试数据
        Random rd = new Random();
        string[] testExecTypeArr = new string[] { "20", "21", "22", "23" };
        private string GetTestExecType()
        {
            return testExecTypeArr[rd.Next(testExecTypeArr.Length)];
        }

        string[] testSyncFlagArr = new string[] { "20", "21", "22", "23" };
        private string getTestSyncflag()
        {
            return testSyncFlagArr[rd.Next(testSyncFlagArr.Length)];
        }

        string[] testSideArr = new string[] { "买入", "卖出" };
        private string getTestSide()
        {
            return testSideArr[rd.Next(testSideArr.Length)];
        }

        string[] testTerminalType = new string[] { "0", "1", "2" };
        private string getTestTerminal()
        {
            return testTerminalType[rd.Next(testTerminalType.Length)];
        }

        string[] testDealTypeArr = new string[] { "0", "1" };
        private string getTestDealType()
        {
            return testDealTypeArr[rd.Next(testDealTypeArr.Length)];
        }

        private void GenerateTestDate()
        {
            // 主表
            for (int i = 0; i < 2000; i++)
            {
                DataRow dr = _MainCurrentPageDV.Table.NewRow();
                dr[BookHandler.BLOCK_NUM_FIELD] = $"{i}";
                dr[BookHandler.EXEC_TYPE_FIELD] = GetTestExecType();
                dr[BookHandler.SYNC_FLAG_FIELD] = getTestSyncflag();
                dr[BookHandler.SIDE_FIELD] = getTestSide();
                dr[BookHandler.SECURITY_ID_FIELD] = "180028";
                dr[BookHandler.SYMBOL_FIELD] = "18国债28";
                dr[BookHandler.PARTY_SUB_NAME_FIELD] = "李四";
                dr[BookHandler.COUNTER_PARTY_NAME_FILED] = "国利";
                dr[BookHandler.COUNTER_PARTY_SUB_NAME_FIELD] = "张三";
                dr[BookHandler.YIELD_FIELD] = "3.345";
                dr[BookHandler.QTY_FIELD] = "5000";
                dr[BookHandler.SETTLE_TYPE_FIELD] = "1";
                dr[BookHandler.TRADE_DATE_FIELD] = DateTime.Now.ToString("yyyy-MM-dd");
                dr[BookHandler.SETTLE_DATE_FIELD] = DateTime.Now.ToString("yyyy-MM-dd");
                dr[BookHandler.EXEC_ID_FIELD] = $"Main-{i}";
                dr[BookHandler.PRICE_FIELD] = "3.13";
                dr[BookHandler.TERMINAL_FIELD] = getTestTerminal();
                dr[BookHandler.NUM_FIELD] = "3";
                dr[BookHandler.CLOSE_BY_FIELD] = "yxl11";
                dr[BookHandler.UPDATE_TIME_FIELD] = DateTime.Now.ToString("HH:mm:ss.fff");
                dr[BookHandler.SEND_TIME_FIELD] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                this._MainCurrentPageDV.Table.Rows.Add(dr);
            }

            //明细表
            for (int i = 0; i < 2000; i++)
            {
                DataRow dr = this.SubCurrentPageDV.Table.NewRow();
                dr[BookHandler.BLOCK_NUM_FIELD] = $"{i}";
                dr[BookHandler.EXEC_ID_FIELD] = $"Main-{i}";
                dr[BookHandler.DEALTYPE_FIELD] = getTestDealType();
                dr[BookHandler.SIDE_FIELD] = getTestSide();
                dr[BookHandler.SECURITY_ID_FIELD] = "180028";
                dr[BookHandler.SYMBOL_FIELD] = "18国债28";
                dr[BookHandler.YIELD_FIELD] = "3.345";
                dr[BookHandler.NETPRICE_FIELD] = "109";
                dr[BookHandler.DIRTYPRICE_FIELD] = "120";
                dr[BookHandler.QTY_FIELD] = "5000";
                dr[BookHandler.SETTLE_TYPE_FIELD] = "1";
                dr[BookHandler.TRADE_DATE_FIELD] = DateTime.Now.ToString("yyyy-MM-dd");
                dr[BookHandler.SETTLE_DATE_FIELD] = DateTime.Now.ToString("yyyy-MM-dd");
                dr[BookHandler.PARTY_SUB_NAME_FIELD] = "李四";
                dr[BookHandler.COUNTER_PARTY_NAME_FILED] = "国利";
                dr[BookHandler.COUNTER_PARTY_SUB_NAME_FIELD] = "张三";
                dr[BookHandler.NUM_FIELD] = "3";
                dr[BookHandler.CLOSE_BY_FIELD] = "yxl11";
                dr[BookHandler.UPDATE_TIME_FIELD] = DateTime.Now.ToString("HH:mm:ss.fff");
                dr[BookHandler.SEND_TIME_FIELD] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");

                //dr[BookHandler.PRICE_FIELD] = "3.13";
                //dr[BookHandler.TERMINAL_FIELD] = getTestTerminal();

                this._SubCurrentPageDV.Table.Rows.Add(dr);
            }
        }
        #endregion
    }
}
