using com.gateway.log;
using Microsoft.Win32;
using STC.Common.Handler.Admin;
using STC.Common.Handler.Risk;
using STC.Common.Handler.SystemParam;
using STC.Common.Model.Admin;
using STC.Common.Model.Export;
using STC.Common.Model.Risk;
using STC.Common.Utils;
using STC.Common.View.Component;
using STC.Common.View.UI;
using STC.XBond.Plugin.Utils;
using STC.XBond.Plugin.View;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace STC.XBond.Plugin.ViewModel
{
    /// <summary>
    /// 风控记录 HistoryRiskView的数据内容
    /// </summary>
    public class HistoryRiskViewModel : ViewModelBase
    {
        public ICommand ExportCommand { get { return new RelayCommand(OnExportClick); } }

        private HistoryRiskView historyView;

        public HistoryRiskView HistoryView
        {
            get
            {
                return historyView;
            }

            set
            {
                historyView = value;
            }
        }
        private RiskHistoryReq request = new RiskHistoryReq();
        public RiskHistoryReq Request
        {
            get
            {
                return request;
            }

            set
            {
                request = value;
                this.OnPropertyChanged();
            }
        }
        private int totalCount;
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
        private int index = 0;
        public int Index
        {
            get
            {
                return index;
            }

            set
            {
                index = value;
            }
        }
        private PageControl _pageControl;
        public PageControl PageControl
        {
            get
            {
                return _pageControl;
            }

            set
            {
                _pageControl = value;
                this.OnPropertyChanged();
            }
        }
        private string pageVisible;

        public string PageVisible
        {
            get
            {
                return pageVisible;
            }

            set
            {
                pageVisible = value;
                this.OnPropertyChanged();
            }
        }
        private object lockObj = new object();
        public object LockObj
        {
            get
            {
                return lockObj;
            }
        }
        #region '历史消息'表格字段
        public const string KEY_FIELD = "Key";
        public const string ORDERID_FIELD = "Orderid";
        public const string CLORDID_TYPE_FIELD = "Clordid";
        public const string QUOTEID_FIELD = "Quoteid";
        public const string MDREQID_FIELD = "MdReqid";
        public const string VENUE_TYPEGW_FIELD = "Venuetypegw";
        public const string VENUES_FIELD = "Venues";
        public const string SECURITYID_FIELD = "Securityid";
        public const string SYMBOL_FIELD = "Symbol";
        public const string SYMBOL_TYPE_FIELD = "Symboltype";
        public const string MARKERINDICATOR_FIELD = "Marketindicator";
        public const string PRATYSUBID_FIELD = "Partysubid";
        public const string VENUEUSERSID_FIELD = "Venueusersid";
        public const string USERNAME_FIELD = "Username";
        public const string RULE_FIELD = "Rule";
        public const string SIDE_FIELD = "Side";
        public const string PRICE_FIELD = "Price";
        public const string ORDERQTY_FIELD = "Orderqty";
        public const string ORDTYPE_FIELD = "Ordtype";
        public const string SETTLTYPE_FIELD = "SettlType";
        public const string CURRENCY_FIELD = "Currency";
        public const string TRADEDATE_FIELD = "Tradedate";
        public const string VALIDUNTITIME_FIELD = "Validuntitime";
        public const string TIMEINFORCE_FIELD = "Timeinforce";
        public const string TERMINAL_FIELD = "Terminal";
        public const string REMARK_FIELD = "Remark";
        public const string CREATETIME_FIELD = "Createtime";
        public const string UPDATETIME_FIELD = "Updatetime";
        public const string PARTYSUBNAME_FIELD = "Partysubname";
        public const string YIELD_FIELD = "Yield";
        public const string NETPRICE_FIELD = "Netprice";
        public const string PARTYNAME_FIELD = "Partyname";
        public const string USERID_FIELD = "Userid";
        public const string FIRMFLAG_FIELD = "Firmflag";
        public const string WHOLEFLAG_FIELD = "Wholeflag";
        public const string NAME_FIELD = "Name";
        public const string QUANTID_FIELD = "Quantid";
        public const string VENUESRDMNUM_FIELD = "Venuesrdmnum";
        public const string SELFSELECTVENUES_FIELD = "Selfselectvenues";
        public const string VENUESSELECTF_FIELD = "Venuesselectf";
        public const string VENUES_SELECT_DISPLAY_FIELD = "VenuesSelectDisplay";
        public const string RISKMANUALCK_FIELD = "Riskmanualck";
        public const string CROSSVENUES_FIELD = "Crossvenues";
        public const string INSTITUTIONID_FIELD = "Institutionid";
        public const string INSTITUTIONNAME_FIELD = "Institutionname";
        public const string TRADERID_FIELD = "Traderid";
        public const string TRADERNAME_FIELD = "Tradername";
        public const string TYPE_FIELD = "Type";
        public const string INDICATOR_FIELD = "Indicator";
        public const string INDICATORVALUE_FIELD = "Indicatorvalue";
        public const string INDICATORLIMITVALUE_FIELD = "Indicatorlimitvalue";
        public const string INDICATORSOURCE_FIELD = "Indicatorsource";
        public const string RESULT_FIELD = "Result";
        #endregion
        private DataView _RiskDV;
        public DataView RiskDV
        {
            get
            {
                lock (LockObj)
                {
                    if (_RiskDV == null)
                    {
                        _RiskDV = new DataTable().DefaultView;
                        _RiskDV.Table.Columns.Add(KEY_FIELD, typeof(string));
                        _RiskDV.Table.Columns.Add(ORDERID_FIELD, typeof(string));
                        _RiskDV.Table.Columns.Add(CLORDID_TYPE_FIELD, typeof(string));
                        _RiskDV.Table.Columns.Add(QUOTEID_FIELD, typeof(string));
                        _RiskDV.Table.Columns.Add(MDREQID_FIELD, typeof(string));
                        _RiskDV.Table.Columns.Add(VENUE_TYPEGW_FIELD, typeof(string));
                        _RiskDV.Table.Columns.Add(VENUES_FIELD, typeof(string));
                        _RiskDV.Table.Columns.Add(SECURITYID_FIELD, typeof(string));
                        _RiskDV.Table.Columns.Add(SYMBOL_FIELD, typeof(string));
                        _RiskDV.Table.Columns.Add(SYMBOL_TYPE_FIELD, typeof(string));
                        _RiskDV.Table.Columns.Add(MARKERINDICATOR_FIELD, typeof(string));
                        _RiskDV.Table.Columns.Add(PRATYSUBID_FIELD, typeof(string));
                        _RiskDV.Table.Columns.Add(VENUEUSERSID_FIELD, typeof(string));
                        _RiskDV.Table.Columns.Add(USERNAME_FIELD, typeof(string));
                        _RiskDV.Table.Columns.Add(RULE_FIELD, typeof(string));
                        _RiskDV.Table.Columns.Add(SIDE_FIELD, typeof(string));
                        _RiskDV.Table.Columns.Add(PRICE_FIELD, typeof(string));
                        _RiskDV.Table.Columns.Add(ORDERQTY_FIELD, typeof(string));
                        _RiskDV.Table.Columns.Add(ORDTYPE_FIELD, typeof(string));
                        _RiskDV.Table.Columns.Add(SETTLTYPE_FIELD, typeof(string));
                        _RiskDV.Table.Columns.Add(CURRENCY_FIELD, typeof(string));
                        _RiskDV.Table.Columns.Add(TRADEDATE_FIELD, typeof(string));
                        _RiskDV.Table.Columns.Add(VALIDUNTITIME_FIELD, typeof(string));
                        _RiskDV.Table.Columns.Add(TIMEINFORCE_FIELD, typeof(string));
                        _RiskDV.Table.Columns.Add(TERMINAL_FIELD, typeof(string));
                        _RiskDV.Table.Columns.Add(REMARK_FIELD, typeof(string));
                        _RiskDV.Table.Columns.Add(CREATETIME_FIELD, typeof(string));
                        _RiskDV.Table.Columns.Add(UPDATETIME_FIELD, typeof(string));
                        _RiskDV.Table.Columns.Add(PARTYSUBNAME_FIELD, typeof(string));
                        _RiskDV.Table.Columns.Add(YIELD_FIELD, typeof(decimal));
                        _RiskDV.Table.Columns.Add(NETPRICE_FIELD, typeof(decimal));
                        _RiskDV.Table.Columns.Add(PARTYNAME_FIELD, typeof(string));
                        _RiskDV.Table.Columns.Add(USERID_FIELD, typeof(string));
                        _RiskDV.Table.Columns.Add(FIRMFLAG_FIELD, typeof(string));
                        _RiskDV.Table.Columns.Add(WHOLEFLAG_FIELD, typeof(string));
                        _RiskDV.Table.Columns.Add(NAME_FIELD, typeof(string));
                        _RiskDV.Table.Columns.Add(QUANTID_FIELD, typeof(string));
                        _RiskDV.Table.Columns.Add(VENUESRDMNUM_FIELD, typeof(string));
                        _RiskDV.Table.Columns.Add(SELFSELECTVENUES_FIELD, typeof(string));
                        _RiskDV.Table.Columns.Add(VENUESSELECTF_FIELD, typeof(string));
                        _RiskDV.Table.Columns.Add(RISKMANUALCK_FIELD, typeof(string));
                        _RiskDV.Table.Columns.Add(CROSSVENUES_FIELD, typeof(string));
                        _RiskDV.Table.Columns.Add(INSTITUTIONID_FIELD, typeof(string));
                        _RiskDV.Table.Columns.Add(INSTITUTIONNAME_FIELD, typeof(string));
                        _RiskDV.Table.Columns.Add(TRADERID_FIELD, typeof(string));
                        _RiskDV.Table.Columns.Add(TRADERNAME_FIELD, typeof(string));
                        _RiskDV.Table.Columns.Add(TYPE_FIELD, typeof(string));
                        _RiskDV.Table.Columns.Add(INDICATOR_FIELD, typeof(string));
                        _RiskDV.Table.Columns.Add(INDICATORVALUE_FIELD, typeof(string));
                        _RiskDV.Table.Columns.Add(INDICATORLIMITVALUE_FIELD, typeof(string));
                        _RiskDV.Table.Columns.Add(INDICATORSOURCE_FIELD, typeof(string));
                        _RiskDV.Table.Columns.Add(RESULT_FIELD, typeof(string));
                        _RiskDV.Table.Columns.Add(VENUES_SELECT_DISPLAY_FIELD, typeof(string));
                        _RiskDV.Sort = $"{CREATETIME_FIELD} desc";
                        //_RiskDV.Table.PrimaryKey = new DataColumn[] { _RiskDV.Table.Columns[KEY_FIELD] };
                    }
                    return this._RiskDV;
                }
            }
        }
        private string day = "";
        private string today = "";
        private ThreadQueue<RiskMsg> _ProcessThreadQueue;
        public HistoryRiskViewModel()
        {
            BindingOperations.EnableCollectionSynchronization(RiskDV, LockObj);
            _ProcessThreadQueue = new ThreadQueue<RiskMsg>(UpdateData);
            day = DateTimeEx.ServerTime.AddDays(-1).ToString("yyyy-MM-dd");
            today = DateTimeEx.ServerTime.ToString("yyyy-MM-dd");
            request.dealTimeBegin = day; ;
            request.dealTimeEnd = today; ;
        }

        ~HistoryRiskViewModel()
        {
            if (_ProcessThreadQueue != null)
            {
                _ProcessThreadQueue.Dispose();
            }
        }

        public void Init()
        {
            System.Threading.ThreadPool.QueueUserWorkItem(o =>
            {
                try
                {
                    historyView.ShowMask(true);
                    Index = 0;
                    RequestData();
                }
                catch (Exception ex)
                {
                    LogHelper.Error(ex);
                }
            });
        }
        internal void clear()
        {
            try
            {
                lock (RiskDV)
                {
                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        foreach (DataRow dr in this.RiskDV.Table.Rows)
                        {
                            dr.BeginEdit();
                            dr.Delete();
                            dr.EndEdit();
                        }
                        //this.RiskDV.Table.Rows.Clear();
                        this.RiskDV.Table.AcceptChanges();
                    }));
                }
            }
            catch (Exception e)
            {
                LogHelper.Error(e);
            }
        }
        private void UpdateData(RiskMsg obj)
        {
            try
            {
                lock (this.LockObj)
                {
                    DataRow dr = this.RiskDV.Table.NewRow();
                    dr.BeginEdit();
                    dr[KEY_FIELD] = obj.clordid;
                    dr[ORDERID_FIELD] = obj.orderid;
                    dr[CLORDID_TYPE_FIELD] = obj.clordid;
                    dr[QUOTEID_FIELD] = obj.quoteid;
                    dr[MDREQID_FIELD] = obj.mdReqid;
                    dr[VENUE_TYPEGW_FIELD] = obj.venuetypegw;
                    dr[VENUES_FIELD] = obj.venues;
                    dr[SECURITYID_FIELD] = obj.securityid;
                    dr[SYMBOL_FIELD] = obj.symbol;
                    dr[SYMBOL_TYPE_FIELD] = obj.symboltype;
                    dr[MARKERINDICATOR_FIELD] = obj.marketindicator;
                    dr[PRATYSUBID_FIELD] = obj.partysubid;
                    dr[VENUEUSERSID_FIELD] = obj.venueusersid;
                    dr[USERNAME_FIELD] = obj.username;
                    dr[RULE_FIELD] = obj.rule;
                    dr[SIDE_FIELD] = obj.side;
                    dr[PRICE_FIELD] = obj.price;
                    dr[ORDERQTY_FIELD] = obj.orderqty;
                    dr[ORDTYPE_FIELD] = obj.ordtype;
                    dr[SETTLTYPE_FIELD] = obj.settltype;
                    dr[CURRENCY_FIELD] = obj.currency;
                    dr[TRADEDATE_FIELD] = obj.tradedate;
                    dr[VALIDUNTITIME_FIELD] = obj.createtime;
                    dr[TIMEINFORCE_FIELD] = obj.timeinforce;
                    dr[TERMINAL_FIELD] = obj.terminal;
                    dr[REMARK_FIELD] = obj.remark;
                    dr[CREATETIME_FIELD] = obj.createtime;
                    dr[UPDATETIME_FIELD] = obj.updatetime;
                    dr[PARTYSUBNAME_FIELD] = obj.partysubname;
                    dr[YIELD_FIELD] = obj.yield;
                    dr[NETPRICE_FIELD] = obj.netprice;
                    dr[PARTYNAME_FIELD] = obj.partyname;
                    dr[USERID_FIELD] = obj.userid;
                    dr[FIRMFLAG_FIELD] = obj.firmflag;
                    dr[WHOLEFLAG_FIELD] = obj.wholeflag;
                    dr[NAME_FIELD] = obj.name;
                    dr[QUANTID_FIELD] = obj.quantid;
                    dr[VENUESRDMNUM_FIELD] = obj.venuesrdmnum;
                    dr[SELFSELECTVENUES_FIELD] = obj.selfselectvenues;
                    dr[VENUESSELECTF_FIELD] = obj.venuesselectf;
                    dr[RISKMANUALCK_FIELD] = obj.riskmanualck;
                    dr[CROSSVENUES_FIELD] = obj.crossvenues;
                    dr[INSTITUTIONID_FIELD] = obj.institutionid;
                    dr[INSTITUTIONNAME_FIELD] = obj.institutionname;
                    dr[TRADERID_FIELD] = obj.traderid;
                    dr[TRADERNAME_FIELD] = obj.tradername;
                    dr[TYPE_FIELD] = obj.type;
                    dr[INDICATOR_FIELD] = obj.indicator;
                    dr[INDICATORVALUE_FIELD] = obj.indicatorvalue;
                    dr[INDICATORLIMITVALUE_FIELD] = obj.indicatorlimitvalue;
                    dr[INDICATORSOURCE_FIELD] = obj.indicatorsource;
                    dr[RESULT_FIELD] = obj.result;
                    string venuesSelectDisplay = "TradeBook";
                    if (obj.venuesselectf == "2")
                    {
                        if (string.IsNullOrEmpty(obj.venuesrdmnum) == false)
                        {
                            venuesSelectDisplay = $"默认+{obj.venuesrdmnum}家";
                        }
                        else
                        {
                            venuesSelectDisplay = "默认";
                        }
                    }
                    else if (obj.venuesselectf == "3")
                    {
                        StringBuilder displaySB = new StringBuilder();
                        string[] arr = obj.selfselectvenues.Split(',');
                        foreach (string v in arr)
                        {
                            if (string.IsNullOrEmpty(v) == false)
                            {
                                string v_str = ComboSourceUtil.Instance.FindEnum(v.Substring(v.IndexOf('-') + 1), "venues");
                                displaySB.Append($"<{v_str}>");
                            }
                        }
                        venuesSelectDisplay = $"指定{displaySB}";
                    }
                    if (obj.venues.Equals("IDB"))
                    {
                        venuesSelectDisplay = "";
                    }
                    dr[VENUES_SELECT_DISPLAY_FIELD] = venuesSelectDisplay;

                    this.RiskDV.Table.Rows.Add(dr);
                    dr.EndEdit();
                    this.RiskDV.Table.AcceptChanges();
                }
            }
            catch (Exception e)
            {
                LogHelper.Error("UpdateMsg Method", e);
            }
        }
        public void RequestData()
        {
            try
            {
                LogHelper.Info("风控记录加载开始");
                request.pageSize = CommonConstants.pageCount.ToString();

                request.pageNum = Index.ToString();
                request.userName = LocalEnv.UserInfo.UserName;
                //request.userName = "li.fang";
                ///获取成交
                var reply = RiskHandler.Instance.queryRisk(request);

                if (reply != null && reply.Datas != null)
                {
                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        TotalCount = reply.totalSize;
                        PageControl.RecordCount = reply.totalSize;
                        if (TotalCount > CommonConstants.pageCount)
                        {
                            PageVisible = "0";
                        }
                        else
                        {
                            PageVisible = "";
                        }
                        foreach (RiskMsg msg in reply.Datas)
                        {
                            UpdateData(msg);
                            //_ProcessThreadQueue.Enqueue(msg);
                        }
                        LogHelper.Info("风控记录加载完成");
                    }));
                }
                else
                {
                    LogHelper.Info("风控记录加载完成");
                }
            }
            catch (Exception e)
            {
                LogHelper.Error("RequestMsg", e);
            }
            finally
            {
                historyView.IsLoad = true;
                historyView.ShowMask(false);
            }
        }
        internal void ProcessMsgsFilterCondition(Dictionary<string, IList> dealsFilterDic)
        {
            if (dealsFilterDic.ContainsKey("TransactDate"))
            {
                IList dateList = dealsFilterDic["TransactDate"];
                if (dateList.Count > 0)
                {
                    DateTime begin = Convert.ToDateTime(dateList[0].ToString());
                    DateTime end = Convert.ToDateTime(dateList[1].ToString());
                    bool status = TimeUtils.Instance.TimeLengthMoreThanMonth(begin, end);
                    if (status)
                    {
                        showMessage("界面不支持半年以上风控信息筛选，请修改日期区间");
                        return;
                    }
                    Request.dealTimeBegin = dateList[0].ToString();
                    Request.dealTimeEnd = dateList[1].ToString();
                }
                else
                {
                    Request.dealTimeBegin = day; ;
                    Request.dealTimeEnd = today; ;
                }
            }
            if (dealsFilterDic.ContainsKey("Types"))
            {
                IList venuesList = dealsFilterDic["Types"];
                StringBuilder inFilterSB_venues = new StringBuilder();
                if (venuesList.Count > 0)
                {
                    foreach (ListBoxItem item in venuesList)
                    {
                        if (inFilterSB_venues.Length > 0)
                        {
                            inFilterSB_venues.Append(",");
                        }

                        if (item.Content.ToString() == Constants.ONEWARN)
                        {
                            inFilterSB_venues.Append("1");
                        }
                        else if (item.Content.ToString() == Constants.TWOWARN)
                        {
                            inFilterSB_venues.Append("3");
                        }
                        else if (item.Content.ToString() == Constants.FORBIDWARN)
                        {
                            inFilterSB_venues.Append("2");
                        }
                    }
                }
                if (inFilterSB_venues.Length > 0)
                {
                    Request.type = inFilterSB_venues.ToString();
                }
                else
                {
                    Request.type = "";
                }
            }
            if (dealsFilterDic.ContainsKey("Venues"))
            {
                IList venuesList = dealsFilterDic["Venues"];
                StringBuilder inFilterSB_venues = new StringBuilder();
                Request.rule = "";
                if (venuesList.Count > 0)
                {
                    foreach (ListBoxItem item in venuesList)
                    {
                        if (inFilterSB_venues.Length > 0)
                        {
                            inFilterSB_venues.Append(",");
                        }

                        if (item.Content.ToString() == Constants.IDB)
                        {
                            inFilterSB_venues.Append("IDB");
                        }
                        else if (item.Content.ToString() == Constants.QUANT)
                        {
                            inFilterSB_venues.Append("QUANT");
                        }
                        else if (item.Content.ToString() == Constants.MANUAL || item.Content.ToString() == Constants.INNER)
                        {
                            inFilterSB_venues.Append("MANUAL");

                            //List<string> allVenues = SystemParamHandler.Instance.GetAllVenue();
                            //foreach (string venue in allVenues)
                            //{
                            //    if (!venue.Equals("QDMESP"))
                            //        inFilterSB_venues.Append("," + venue);
                            //}

                        }
                        else if (item.Content.ToString() == Constants.QDMESP)
                        {
                            inFilterSB_venues.Append("QDMESP");
                            //Request.rule = "MM";
                        }
                    }
                }
                if (inFilterSB_venues.Length > 0)
                {
                    Request.venues = inFilterSB_venues.ToString();
                }
                else
                {
                    Request.venues = "";
                }
            }
            if (dealsFilterDic.ContainsKey("Firmflag"))
            {
                IList venuesList = dealsFilterDic["Firmflag"];
                StringBuilder inFilterSB_venues = new StringBuilder();
                if (venuesList.Count > 0)
                {
                    foreach (ListBoxItem item in venuesList)
                    {
                        if (inFilterSB_venues.Length > 0)
                        {
                            inFilterSB_venues.Append(",");
                        }

                        if (item.Content.ToString() == Constants.FIRM)
                        {
                            inFilterSB_venues.Append("1");
                        }
                        else if (item.Content.ToString() == Constants.NONFIRM)
                        {
                            inFilterSB_venues.Append("0");
                        }
                    }
                }
                if (inFilterSB_venues.Length > 0)
                {
                    Request.firmflag = inFilterSB_venues.ToString();
                }
                else
                {
                    Request.firmflag = "";
                }
            }
            if (dealsFilterDic.ContainsKey("Status"))
            {
                IList statusList = dealsFilterDic["Status"];
                if (statusList.Count > 0)
                {
                    CheckBox clickedBox = (CheckBox)statusList[0];
                    if (clickedBox.IsChecked.Value)
                    {
                        Request.operationResult = "取消报价";
                    }
                    else
                    {
                        Request.operationResult = "";
                    }
                }
            }
            
            System.Threading.ThreadPool.QueueUserWorkItem(o =>
            {
                try
                {
                    clear();
                    historyView.ShowMask(true);
                    Index = 0;
                    RequestData();
                }
                catch (Exception ex)
                {
                    LogHelper.Error(ex);
                }
            });
        }
        public static List<KeyValuePair<string, string>> SideItemSource
        {
            get
            {
                return ComboSourceUtil.Instance.GetComboSource("Side");
            }
        }

        public static List<KeyValuePair<string, string>> SettleTypeItemSource
        {
            get
            {
                return ComboSourceUtil.Instance.GetComboSource("SettleType");
            }
        }

        public static List<KeyValuePair<string, string>> VenuesItemSource
        {
            get
            {
                return ComboSourceUtil.Instance.GetComboSource("Venues");
            }
        }
        /// <summary>
        /// 提示消息弹框
        /// </summary>
        /// <param name="message"></param>
        private void showMessage(string message)
        {
            MessageBoxGUI me = new MessageBoxGUI();
            me.setWarnMsg(message);
            me.ShowDialog();
        }

        private DataTable ExportDV;
        private string fileName = "";
        AutoResetEvent autoEvent = new AutoResetEvent(false);
        private void OnExportClick()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "(文本文件*.csv)|*.csv";
            saveFileDialog.FileName = DateTime.Now.ToString("yyyyMMddHHmmss") + "_历史风控记录";
            saveFileDialog.AddExtension = true;
            saveFileDialog.RestoreDirectory = true;
            if ((bool)(saveFileDialog.ShowDialog()))
            {
                fileName = saveFileDialog.FileName;
                ExportDV = RiskDV.Table.Clone();
                ThreadPool.QueueUserWorkItem(new WaitCallback(RequestExportMethod), autoEvent);
            }
        }
        private void RequestExportMethod(object state)
        {
            lock (ExportDV.Rows.SyncRoot)
            {
                RequestExport();
            }
            autoEvent.Set();
        }
        public void RequestExport()
        {
            StreamWriter sw = null;
            try
            {
                sw = ExcelHelper.Instance.createStream(fileName);
                if (sw != null)
                {
                    LogHelper.Info("fileName:" + fileName);
                    //N:已申请,0:处理中，1,2:已确认(其中1为部分确认,2完全确认)，4:已撤销，8:失败
                    //E(Broker B确认),F(Broker B撤销),G（Broker A确认），H（Broker A撤销）
                    //request.orderStatus = "N,0,1";
                    RiskHistoryReq req = new RiskHistoryReq();
                    req.userName = LocalEnv.UserInfo.UserName;
                    req.pageSize = "1";
                    req.pageNum = "1";
                    req.dealTimeBegin = Request.dealTimeBegin;
                    req.dealTimeEnd = Request.dealTimeEnd;
                    req.type = Request.type;
                    req.venues = Request.venues;
                    req.firmflag = Request.firmflag;
                    req.operationResult = Request.operationResult;
                    req.orderid = Request.orderid;
                    req.securityid = Request.securityid;
                    ///获取订单 
                    var reply = RiskHandler.Instance.queryRisk(req);

                    if (reply != null && reply.Datas != null)
                    {
                        int maxCount = reply.totalSize;
                        int pageNum = 0;
                        if (maxCount > 0)
                        {
                            pageNum = maxCount % CommonConstants.pageCount == 0 ? (maxCount / CommonConstants.pageCount) : (maxCount / CommonConstants.pageCount + 1);
                        }
                        else
                            pageNum = 0;
                        int count = 0;
                        bool status = true;
                        for (int i = 0; i < pageNum; i++)
                        {
                            req.pageNum = i.ToString();
                            req.pageSize = CommonConstants.pageCount.ToString();
                            var reply1 = RiskHandler.Instance.queryRisk(req);
                            if (reply1 != null && reply1.Datas != null && reply1.Datas.Count > 0)
                            {
                                foreach (RiskMsg obj in reply1.Datas)
                                {                                   
                                    ExportPo po = toExportPO(obj);
                                    status = ExcelHelper.Instance.SaveDataToCSVFile(po, fileName, sw, count, Constants.HISRisks);
                                    count++;
                                }
                            }
                        }
                        Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                        {
                            if (count == maxCount)
                            {
                                showMessage("导出成功");
                            }
                            else
                            {
                                showMessage("导出失败");
                            }
                        }));
                    }
                    if (sw != null)
                    {
                        sw.Dispose();
                    }
                }
                else
                {
                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        showMessage("导出失败");
                    }));
                }
            }
            catch (Exception e)
            {
                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    showMessage("导出失败");
                }));
                LogHelper.Error("RequestExportOrders", e);
            }
            finally
            {
                if (sw != null)
                {
                    sw.Dispose();
                }
            }
        }
        private ExportPo toExportPO(RiskMsg obj)
        {
            ExportPo po = new ExportPo();
            po.Orderid = obj.orderid;
            po.SecurityID = obj.securityid;
            po.Side = ComboSourceUtil.Instance.FindEnum(obj.side, "side");
            po.FirmFlag = ComboSourceUtil.Instance.FindEnum(obj.firmflag, "firmflag");
            po.Price = obj.price;
            po.Qty = CommonUtils.ConvertToLong(obj.orderqty) == 0 ? "" : CommonUtils.ConvertToLong(obj.orderqty).ToString();
            po.SettlType = ComboSourceUtil.Instance.FindEnum(obj.settltype, "settletype_myorder");
            po.Veneues = ComboSourceUtil.Instance.FindEnum(obj.venues, "venues");
            po.CrossVenues = ComboSourceUtil.Instance.FindEnum(obj.crossvenues, "crossvenues");
            po.Type = ComboSourceUtil.Instance.FindEnum(obj.type, "risktype");
            po.Indicator = obj.indicator;
            po.Indicatorvalue = obj.indicatorvalue;
            po.Indicatorlimitvalue = obj.indicatorlimitvalue;
            po.Indicatorsource = obj.indicatorsource;
            po.Result = obj.result;
            string venuesSelectDisplay = "TradeBook";
            if (obj.venuesselectf == "2")
            {
                if (string.IsNullOrEmpty(obj.venuesrdmnum) == false)
                {
                    venuesSelectDisplay = $"默认+{obj.venuesrdmnum}家";
                }
                else
                {
                    venuesSelectDisplay = "默认";
                }
            }
            else if (obj.venuesselectf == "3")
            {
                StringBuilder displaySB = new StringBuilder();
                string[] arr = obj.selfselectvenues.Split(',');
                foreach (string v in arr)
                {
                    if (string.IsNullOrEmpty(v) == false)
                    {
                        string v_str = ComboSourceUtil.Instance.FindEnum(v.Substring(v.IndexOf('-') + 1), "venues");
                        displaySB.Append($"<{v_str}>");
                    }
                }
                venuesSelectDisplay = $"指定{displaySB}";
            }
            if (obj.venues.Equals("IDB"))
            {
                venuesSelectDisplay = "";
            }
            po.VenuesSelectDisplay = venuesSelectDisplay;
            po.Createtime = TimeUtils.Instance.ToTime(obj.createtime);
            po.Tradedate = TimeUtils.Instance.ToDate(obj.tradedate);
            po.UpdateTime = TimeUtils.Instance.ToTime(obj.updatetime);
            return po;
        }
    }
}
