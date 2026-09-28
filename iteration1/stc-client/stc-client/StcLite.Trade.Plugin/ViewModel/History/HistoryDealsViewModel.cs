using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows;
using BDB.Trade.Plugin.CustomeUI;
using System.Windows.Data;
using System.Collections;
using System.Windows.Controls;
using BDB.Trade.Plugin.View;
using System.Windows.Input;
using System.Threading;
using Microsoft.Win32;
using System.IO;
using com.gateway.log;
using STC.Common.Model;
using STC.Common.Model.TBook;
using STC.Common.Model.Fix.Message;
using STC.Common.Utils;
using STC.Common.Handler.MarketExec;
using STC.Common.View.UI;
using BDB.Trade.Plugin.Utils;
using STC.Common.Model.Export;
using TimeUtils = BDB.Trade.Plugin.Utils.TimeUtils;
using STC.Frame.UI;

namespace BDB.Trade.Plugin.ViewModel
{
    public class HistoryDealsViewModel : BaseViewModel
    {
        public ICommand ExportCommand { get { return new ComRelayCommand(OnExportClick); } }

        private HistoryDealsView historyView;

        public HistoryDealsView HistoryView
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
        private QueryOrderReq request = new QueryOrderReq();
        public QueryOrderReq Request
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
        private object lockObj_Executions = new object();

        

        public object LockObj_Executions
        {
            get
            {
                return lockObj_Executions;
            }
        }

        

        #region '我的成交'表格字段
        public const string KEY_FIELD = "Key";
        public const string SIDE_FIELD = "Side";
        public const string SECURITYID_FIELD = "SecurityID";
        public const string SYMBOL_FIELD = "Symbol";
        public const string LASTPX_FIELD = "LastPx";
        public const string NETPRICE_FIELD = "NetPrice";
        public const string NETPRICE1_FIELD = "NetPrice1";
        public const string DIRTYPRICE_FIELD = "DirtyPrice";
        public const string SETTLTYPE_FIELD = "SettlType";
        public const string CUMQTY_FIELD = "CumQty";
        public const string LASTQTY_FIELD = "LastQty";
        public const string DELIVERYAMOUNT_FIELD = "DeliveryAmount";
        public const string COUNTERPARTYNAME_FIELD = "CounterPartyName";
        public const string COUNTERPARTYSUBNAME_FIELD = "CounterPartySubName";
        public const string EXECID_FIELD = "ExecID";
        public const string VENUES_FIELD = "Venues";
        public const string PARTYSUBNAME_FIELD = "PartySubName";
        public const string EXECUTIONTIME_FIELD = "ExecutionTime";
        public const string TRADEDATE_FIELD = "TradeDate";

        public const string CREATE_DATE_FIELD = "createDate";// 创建日期

        public const string ORDERID_FIELD = "OrderID";
        public const string REFORDERID_FIELD = "RefOrderID";
        public const string SYNC_STATUS_FIELD = "SyncStatus";
        public const string CLORDID_FIELD = "ClOrdID";
        public const string VENUES_SELECT_DISPLAY_FIELD = "VenuesSelectDisplay";
        public const string REMARK_FIELD = "Remark";

        public const string STRATEGY_ORDER_ID_FIELD = "StrategyOrderId";
        #endregion

        private DataView _ExecutionsDV;
        public DataView ExecutionsDV
        {
            get
            {
                lock (LockObj_Executions)
                {
                    if (_ExecutionsDV == null)
                    {
                        _ExecutionsDV = new DataTable().DefaultView;
                        _ExecutionsDV.Table.Columns.Add(KEY_FIELD, typeof(string));
                        _ExecutionsDV.Table.Columns.Add(SIDE_FIELD, typeof(string));
                        _ExecutionsDV.Table.Columns.Add(SECURITYID_FIELD, typeof(string));
                        _ExecutionsDV.Table.Columns.Add(SYMBOL_FIELD, typeof(string));
                        _ExecutionsDV.Table.Columns.Add(LASTPX_FIELD, typeof(decimal));
                        _ExecutionsDV.Table.Columns.Add(NETPRICE_FIELD, typeof(string));
                        _ExecutionsDV.Table.Columns.Add(NETPRICE1_FIELD, typeof(decimal));
                        _ExecutionsDV.Table.Columns.Add(DIRTYPRICE_FIELD, typeof(decimal));
                        _ExecutionsDV.Table.Columns.Add(SETTLTYPE_FIELD, typeof(string));
                        _ExecutionsDV.Table.Columns.Add(CUMQTY_FIELD, typeof(long));
                        _ExecutionsDV.Table.Columns.Add(LASTQTY_FIELD, typeof(long));
                        _ExecutionsDV.Table.Columns.Add(DELIVERYAMOUNT_FIELD, typeof(decimal));
                        _ExecutionsDV.Table.Columns.Add(COUNTERPARTYNAME_FIELD, typeof(string));
                        _ExecutionsDV.Table.Columns.Add(COUNTERPARTYSUBNAME_FIELD, typeof(string));
                        _ExecutionsDV.Table.Columns.Add(EXECID_FIELD, typeof(string));
                        _ExecutionsDV.Table.Columns.Add(VENUES_FIELD, typeof(string));
                        _ExecutionsDV.Table.Columns.Add(PARTYSUBNAME_FIELD, typeof(string)); 
                        _ExecutionsDV.Table.Columns.Add(EXECUTIONTIME_FIELD, typeof(string));
                        _ExecutionsDV.Table.Columns.Add(TRADEDATE_FIELD, typeof(string));
                        _ExecutionsDV.Table.Columns.Add(CREATE_DATE_FIELD, typeof(string));
                        _ExecutionsDV.Table.Columns.Add(ORDERID_FIELD, typeof(string));
                        _ExecutionsDV.Table.Columns.Add(REFORDERID_FIELD, typeof(string));
                        _ExecutionsDV.Table.Columns.Add(SYNC_STATUS_FIELD, typeof(string));
                        _ExecutionsDV.Table.Columns.Add(CLORDID_FIELD, typeof(string));
                        _ExecutionsDV.Table.Columns.Add(VENUES_SELECT_DISPLAY_FIELD, typeof(string));
                        _ExecutionsDV.Table.Columns.Add(REMARK_FIELD, typeof(string));
                        _ExecutionsDV.Table.Columns.Add(STRATEGY_ORDER_ID_FIELD,typeof(string));

                        _ExecutionsDV.Sort = $"{EXECUTIONTIME_FIELD} desc";
                        _ExecutionsDV.Table.PrimaryKey = new DataColumn[] { _ExecutionsDV.Table.Columns[KEY_FIELD] };
                    }
                    return this._ExecutionsDV;
                }
            }
        }
        private string day = "";
        private string today = "";
        private Utils.ThreadQueue<ExecutionReport> _ProcessExecutionThreadQueue;
        public HistoryDealsViewModel()
        {
            BindingOperations.EnableCollectionSynchronization(ExecutionsDV, LockObj_Executions);
            _ProcessExecutionThreadQueue = new Utils.ThreadQueue<ExecutionReport>(UpdateExecutions);
            day = DateTimeEx.ServerTime.AddDays(-1).ToString("yyyy-MM-dd");
            today = DateTimeEx.ServerTime.ToString("yyyy-MM-dd");
            request.dealTimeBegin = day;
            request.dealTimeEnd = today;
        }
        public void Init()
        {
            System.Threading.ThreadPool.QueueUserWorkItem(o =>
            {
                historyView.IsLoad = false;
                historyView.ShowMask(true);
                Index = 0;
                RequestMarketExecutions();
            });
        }
        internal void clear()
        {
            try
            {
                lock (ExecutionsDV)
                {
                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        foreach (DataRow dr in this.ExecutionsDV.Table.Rows)
                        {
                            dr.BeginEdit();
                            dr.Delete();
                            dr.EndEdit();
                        }
                        //this.ExecutionsDV.Table.Rows.Clear();
                        this.ExecutionsDV.Table.AcceptChanges();
                    }));
                }
            }
            catch (Exception e)
            {
                LogHelper.Error(e);
            }
        }
        public void RequestMarketExecutions()
        {
            try
            {
                //N:已申请,0:处理中，1,2:已确认(其中1为部分确认,2完全确认)，4:已撤销，8:失败
                //E(Broker B确认),F(Broker B撤销),G（Broker A确认），H（Broker A撤销）
                request.userName = LocalEnv.UserInfo.UserName;
                request.pageSize = CommonConstants.pageCount;
                request.orderStatus = "1,2";
                request.pageNum = Index;
                ///获取成交
                var reply = MarketExecHandler.Instance.QueryExecOrderForEMS(request);

                if (reply != null && reply.datas != null)
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
                        foreach (ExecutionReport obj in reply.datas)
                        {
                            lock (string.Intern(obj.OrderID))
                            {
                                if (obj.VenueTypeGW != null && !obj.VenueTypeGW.ToLower().Equals(STC.Common.Api.ApiConsts.CDH.ToLower()))
                                {
                                    UpdateExecutions(obj);
                                    //_ProcessExecutionThreadQueue.Enqueue(obj);
                                }
                            }
                        }
                    }));
                }
            }
            catch (Exception e)
            {
                LogHelper.Error("RequestExecutions", e);
            }
            finally
            {
                historyView.ShowMask(false);
                historyView.IsLoad = true;
            }
        }
        private void UpdateExecutions(ExecutionReport obj)
        {
            try
            {
                lock (this.LockObj_Executions)
                {
                    long start = DateTime.Now.Ticks;
                    DataRow dr = this.ExecutionsDV.Table.Rows.Find(obj.OrderID);
                    if (dr != null)
                    {
                    }
                    else
                    {
                        dr = this.ExecutionsDV.Table.NewRow();
                        dr.BeginEdit();
                        dr[KEY_FIELD] = obj.OrderID; // 成交编号
                        this.ExecutionsDV.Table.Rows.Add(dr);
                    }

                    dr[SIDE_FIELD] = obj.Side; // 方向
                    dr[SECURITYID_FIELD] = obj.SecurityID; // 代码
                    dr[SYMBOL_FIELD] = obj.Symbol; // 简称，可能要从基本数据里取
                    dr[LASTPX_FIELD] = CommonUtils.ConvertToDecimal(obj.Yield); // 成交价；
                    dr[NETPRICE_FIELD] = CommonUtils.ConvertToDecimal(obj.NetPrice) == 0 ? "--" : string.Format("{0:N4}", CommonUtils.ConvertToDecimal(obj.NetPrice)); // 净价
                    dr[NETPRICE1_FIELD] = CommonUtils.ConvertToDecimal(obj.NetPrice);
                    decimal dirtyPrice = CommonUtils.ConvertToDecimal(obj.DirtyPrice);
                    dr[DIRTYPRICE_FIELD] = dirtyPrice;// 全价
                    dr[SETTLTYPE_FIELD] = obj.SettlType; // 清算速度
                    dr[CUMQTY_FIELD] = CommonUtils.ConvertToLong(obj.CumQty);//总成交量
                    long lastQty = CommonUtils.ConvertToLong(obj.LastQty);
                    dr[LASTQTY_FIELD] = lastQty; // 成交量
                    dr[DELIVERYAMOUNT_FIELD] = dirtyPrice * lastQty;//结算金额,结算金额=全价 *成交量
                    dr[COUNTERPARTYNAME_FIELD] = obj.CounterPartyName;//对手方
                    dr[COUNTERPARTYSUBNAME_FIELD] = obj.CounterPartySubName;// 对手交易员
                    dr[EXECID_FIELD] = obj.OrderID;//成交编号
                    dr[VENUES_FIELD] = obj.Venues;// 交易场所
                    dr[PARTYSUBNAME_FIELD] = obj.PartySubName;//本方交易员
                    dr[EXECUTIONTIME_FIELD] = obj.TransactTime;// 成交时间
                    dr[TRADEDATE_FIELD] = obj.TradeDate;// 成交时间
                    dr[CREATE_DATE_FIELD] = obj.TransactTime;// 成交时间
                    dr[ORDERID_FIELD] = obj.OrderID;//我的订单号 
                    dr[REFORDERID_FIELD] = obj.CounterPartyOrdID;//对手订单号
                    dr[SYNC_STATUS_FIELD] = ""; // TODO: 同步状态未确定先填空值obj.SynchFlag
                    dr[CLORDID_FIELD] = obj.ExecID; //clOrdID
                    dr[STRATEGY_ORDER_ID_FIELD] = obj.StrategyOrderId;
                    // 渠道显示
                    string venuesSelectDisplay = "TradeBook";
                    if (obj.VenuesSelectF == "2")
                    {
                        if (string.IsNullOrEmpty(obj.VenuesRdmNum) == false)
                        {
                            venuesSelectDisplay = $"默认+{obj.VenuesRdmNum}家";
                        }
                        else
                        {
                            venuesSelectDisplay = "默认";
                        }
                    }
                    else if (obj.VenuesSelectF == "3")
                    {
                        StringBuilder displaySB = new StringBuilder();
                        string[] arr = obj.SelfSelectVenues.Split(',');
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
                    dr[VENUES_SELECT_DISPLAY_FIELD] = venuesSelectDisplay;
                    string remark = "";
                    if (obj.VenueTypeGW.Equals("INNER") && !obj.Venues.Equals("MANUAL") && !obj.Venues.Equals("IDB") && !obj.Venues.Equals("QUANT"))
                    {
                        remark = "渠道执行";
                    }
                    dr[REMARK_FIELD] = remark;
                    dr.EndEdit();
                    this.ExecutionsDV.Table.AcceptChanges();
                    long end = DateTime.Now.Ticks;
                    Console.WriteLine("UpdateExecutions time :" + (end - start) / 10000000.0);
                }
            }
            catch (Exception e)
            {
                LogHelper.Error("UpdateExecutions Method", e);
            }
        }

        internal void ProcessDealsFilterCondition(Dictionary<string, IList> dealsFilterDic)
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
                        showMessage("界面不支持半年以上成交信息筛选，请修改日期区间");
                        return;
                    }
                    Request.dealTimeBegin = dateList[0].ToString();
                    Request.dealTimeEnd = dateList[1].ToString();
                }
                else
                {
                    Request.dealTimeBegin = day;
                    Request.dealTimeEnd = today;
                }
            }
            IList venuesList = dealsFilterDic["Venues"];
            StringBuilder inFilterSB_venues = new StringBuilder();
            if (venuesList.Count > 0)
            {
                foreach (ListBoxItem item in venuesList)
                {
                    if (inFilterSB_venues.Length > 0)
                    {
                        inFilterSB_venues.Append(",");
                    }

                    if (item.Content.ToString() == CommonConstants.TBook)
                    {
                        inFilterSB_venues.Append("IDB,MANUAL,QUANT");
                    }
                    else if (item.Content.ToString().Equals("XBOND"))
                    {
                        inFilterSB_venues.Append("ODM");
                    }
                    else if (item.Content.ToString() == CommonConstants.TPSC)
                    {
                        inFilterSB_venues.Append("TPSC");
                    }
                    else if (item.Content.ToString() == CommonConstants.CTTJ)
                    {
                        inFilterSB_venues.Append("CTTJ");
                    }
                    else if (item.Content.ToString() == CommonConstants.PTCN)
                    {
                        inFilterSB_venues.Append("PTCN");
                    }
                    else if (item.Content.ToString() == CommonConstants.CCTB)
                    {
                        inFilterSB_venues.Append("CCTB");
                    }
                    else if (item.Content.ToString() == CommonConstants.CNEX)
                    {
                        inFilterSB_venues.Append("CNEX");
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
            if (dealsFilterDic.ContainsKey("Status"))
            {
                IList statusList = dealsFilterDic["Status"];
                if (statusList.Count > 0)
                {
                    CheckBox clickedBox = (CheckBox)statusList[0];
                    if (clickedBox.IsChecked.Value)
                    {
                        Request.synchflag = "S,C";
                    }
                    else
                    {
                        Request.synchflag = "";
                    }
                }
            }

            clear();
            System.Threading.ThreadPool.QueueUserWorkItem(o =>
            {
                try
                {
                    historyView.ShowMask(true);
                    Index = 0;
                    RequestMarketExecutions();
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
            MessageBoxGUI22 me = new MessageBoxGUI22();
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
            saveFileDialog.FileName = DateTime.Now.ToString("yyyyMMddHHmmss") + "_历史成交记录";
            saveFileDialog.AddExtension = true;
            saveFileDialog.RestoreDirectory = true;
            if ((bool)(saveFileDialog.ShowDialog()))
            {
                fileName = saveFileDialog.FileName;
                ExportDV = ExecutionsDV.Table.Clone();
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
                    QueryOrderReq req = new QueryOrderReq();
                    req.userName = LocalEnv.UserInfo.UserName;
                    req.orderStatus = "1,2";
                    req.pageSize = 1;
                    req.pageNum = 1;
                    req.dealTimeBegin = Request.dealTimeBegin;
                    req.dealTimeEnd = Request.dealTimeEnd;
                    req.execid = Request.execid;
                    req.strategyorderid = Request.strategyorderid;
                    req.securityid = Request.securityid;
                    req.counterpartyname = Request.counterpartyname;
                    req.venues = Request.venues;
                    req.synchflag = Request.synchflag;
                    req.symboltype = Request.symboltype;
                    ///获取订单 
                    var reply = MarketExecHandler.Instance.QueryExecOrderForEMS(req);

                    if (reply != null && reply.datas != null)
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
                            req.pageNum = i;
                            req.pageSize = CommonConstants.pageCount;
                            var reply1 = MarketExecHandler.Instance.QueryExecOrderForEMS(req);
                            if (reply1 != null && reply1.datas != null && reply1.datas.Count > 0)
                            {
                                foreach (ExecutionReport obj in reply1.datas)
                                {
                                    lock (string.Intern(obj.OrderID))
                                    {
                                        //UpdateExportOrders(obj);                                    
                                        ExportPo po = toExportOrder(obj);
                                        status = ExcelHelper.Instance.SaveDataToCSVFile(po, fileName, sw, count, CommonConstants.HISDeals);
                                        count++;
                                    }
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
        private ExportPo toExportOrder(ExecutionReport obj)
        {
            ExportPo po = new ExportPo();
            po.ExecID = obj.OrderID;
            po.ClOrdID = obj.ExecID;
            po.Side = ComboSourceUtil.Instance.FindEnum(obj.Side, "side");
            po.SecurityID = obj.SecurityID;
            po.Symbol = obj.Symbol;
            po.LastPx = string.Format("{0:N4}", decimal.Parse(obj.Yield));
            po.NetPrice = CommonUtils.ConvertToDecimal(obj.NetPrice) == 0 ? "--" : string.Format("{0:N4}", CommonUtils.ConvertToDecimal(obj.NetPrice)); //string.Format("{0:N4}", decimal.Parse(obj.NetPrice));
            po.LastQty = CommonUtils.ConvertToLong(obj.LastQty) == 0 ? "" : CommonUtils.ConvertToLong(obj.LastQty).ToString();
            po.SettlType = ComboSourceUtil.Instance.FindEnum(obj.SettlType, "settletype");
            po.CounterPartyName = obj.CounterPartyName;
            po.CounterPartySubName = obj.CounterPartySubName;
            po.SyncStatus = "";//ComboSourceUtil.Instance.FindEnum(obj.SynchFlag, "synchflag")
            po.StrategyOrderId = obj.StrategyOrderId;
            po.RefOrderID = obj.CounterPartyOrdID;
            string remark = "";
            if (obj.VenueTypeGW.Equals("INNER") && !obj.Venues.Equals("MANUAL") && !obj.Venues.Equals("IDB") && !obj.Venues.Equals("QUANT"))
            {
                remark = "渠道执行";
            }
            po.Remark = remark;
            po.CrossVenues = ComboSourceUtil.Instance.FindEnum(obj.CrossVenues, "crossvenues");           
            po.VenuesSelectDisplay = ComboSourceUtil.Instance.FindEnum(obj.Venues, "venues_exe");
            po.ExecutionTime = TimeUtils.Instance.ToTime(obj.TransactTime);
            po.ExecutionDate = obj.TradeDate;//TimeUtils.Instance.ToDate(obj.TransactTime)
            po.Createdate = TimeUtils.Instance.ToDate(obj.TransactTime);
            return po;
        }
    }
}
