using com.gateway.log;
using Microsoft.Win32;
using STC.Common.Handler.Admin;
using STC.Common.Model.Admin;
using STC.Common.Model.Export;
using STC.Common.Utils;
using STC.Common.View.Component;
using STC.Common.View.UI;
using STC.XBond.Plugin.CustomUI;
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
    /// 历史消息HistoryMsgView的数据内容
    /// </summary>
    public class HistoryMsgViewModel : ViewModelBase
    {
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
                    if ("系统消息".Equals(_searchText))
                    {
                        Request.contentP2 = "ST";
                    }
                    else if ("订单".Equals(_searchText))
                    {
                        Request.contentP2 = "OD";
                    }
                    else
                    {
                        Request.contentP2 = _searchText;
                    }
                    clear();
                    System.Threading.ThreadPool.QueueUserWorkItem(o =>
                    {
                        try
                        {
                            historyView.ShowMask(true);
                            Index = 0;
                            RequestMsg();
                        }
                        catch (Exception ex)
                        {
                            LogHelper.Error(ex);
                        }
                    });
                }
            }
        }
        public ICommand ExportCommand { get { return new RelayCommand(OnExportClick); } }
        private HistoryMsgView historyView;

        public HistoryMsgView HistoryView
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
        private SystemMessageReq request = new SystemMessageReq();
        public SystemMessageReq Request
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
        public const string CLOSEBY_FIELD = "Closeby";
        public const string CONTENT_TYPE_FIELD = "ContentType";
        public const string READ_FLAG_FIELD = "ReadFlag";
        public const string CONTENT_FIELD = "Content";
        public const string CREATE_TIME_FIELD = "CreateTime";
        public const string BEGIN_TIME_FIELD = "BeginTime";
        public const string END_TIME_FIELD = "EndTime";
        public const string DEL_FLAG_FIELD = "DelFlag";
        public const string UPDATE_TIEM_FIELD = "UpdateTime";
        public const string MSG_TYPE_FIELD = "MsgType";
        public const string LEVEL_FIELD = "Level";
        public const string HEADER_TITLE_FIELD = "HeaderTitle";
        public const string SECURITYID_FIELD = "SecurityID";
        public const string SOURCE_FIELD = "Source";
        public const string STRATEGYORDERID_FIELD = "StrategyOrderId";
        #endregion
        private DataView _MsgDV;
        public DataView MsgDV
        {
            get
            {
                lock (LockObj)
                {
                    if (_MsgDV == null)
                    {
                        _MsgDV = new DataTable().DefaultView;
                        _MsgDV.Table.Columns.Add(KEY_FIELD, typeof(string));
                        _MsgDV.Table.Columns.Add(CLOSEBY_FIELD, typeof(string));
                        _MsgDV.Table.Columns.Add(CONTENT_TYPE_FIELD, typeof(string));
                        _MsgDV.Table.Columns.Add(READ_FLAG_FIELD, typeof(string));
                        _MsgDV.Table.Columns.Add(CONTENT_FIELD, typeof(string));
                        _MsgDV.Table.Columns.Add(CREATE_TIME_FIELD, typeof(string));
                        _MsgDV.Table.Columns.Add(BEGIN_TIME_FIELD, typeof(string));
                        _MsgDV.Table.Columns.Add(END_TIME_FIELD, typeof(string));
                        _MsgDV.Table.Columns.Add(DEL_FLAG_FIELD, typeof(string));
                        _MsgDV.Table.Columns.Add(UPDATE_TIEM_FIELD, typeof(string));
                        _MsgDV.Table.Columns.Add(MSG_TYPE_FIELD, typeof(string));
                        _MsgDV.Table.Columns.Add(LEVEL_FIELD, typeof(string));
                        _MsgDV.Table.Columns.Add(HEADER_TITLE_FIELD, typeof(string));
                        _MsgDV.Table.Columns.Add(SECURITYID_FIELD, typeof(string));
                        _MsgDV.Table.Columns.Add(SOURCE_FIELD, typeof(string));
                        _MsgDV.Table.Columns.Add(STRATEGYORDERID_FIELD, typeof(string));
                        _MsgDV.Sort = $"{UPDATE_TIEM_FIELD} desc";
                        //_MsgDV.Table.PrimaryKey = new DataColumn[] { _MsgDV.Table.Columns[KEY_FIELD] };
                    }
                    return this._MsgDV;
                }
            }
        }
        private string day = "";
        private ThreadQueue<MsgPo> _ProcessThreadQueue;
        public HistoryMsgViewModel()
        {
            BindingOperations.EnableCollectionSynchronization(MsgDV, LockObj);
            _ProcessThreadQueue = new ThreadQueue<MsgPo>(UpdateMsg);
            day = DateTimeEx.ServerTime.ToString("yyyy-MM-dd");
            request.begintime = day + " 00:00:00"; ;
            request.endtime = day + " 23:59:59"; ;
        }

        ~HistoryMsgViewModel()
        {
            if (_ProcessThreadQueue != null)
            {
                _ProcessThreadQueue.Dispose();
            }
        }

        public void Init()
        {
            Index = 0;
            System.Threading.ThreadPool.QueueUserWorkItem(o =>
            {
                historyView.IsLoad = false;
                historyView.ShowMask(true);
                RequestMsg();
            });
        }
        internal void clear()
        {
            try
            {
                lock (MsgDV)
                {
                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        foreach (DataRow dr in this.MsgDV.Table.Rows)
                        {
                            dr.BeginEdit();
                            dr.Delete();
                            dr.EndEdit();
                        }
                        //this.MsgDV.Table.Rows.Clear();
                        this.MsgDV.Table.AcceptChanges();
                    }));
                }
            }
            catch (Exception e)
            {
                LogHelper.Error(e);
            }
        }
        private void UpdateMsg(MsgPo obj)
        {
            try
            {
                lock (this.LockObj)
                {
                    long start = DateTime.Now.Ticks;
                    DataRow dr =  this.MsgDV.Table.NewRow();
                    dr.BeginEdit();
                    dr[KEY_FIELD] = obj.Id; 
                    this.MsgDV.Table.Rows.Add(dr);
                    dr[CLOSEBY_FIELD] = obj.CloseBy;
                    dr[CONTENT_TYPE_FIELD] = obj.ContentType;
                    dr[READ_FLAG_FIELD] = obj.ReadFlag;
                    dr[CONTENT_FIELD] = obj.Content;
                    dr[CREATE_TIME_FIELD] = obj.createtime;
                    dr[DEL_FLAG_FIELD] = obj.DelFlag;

                    dr[UPDATE_TIEM_FIELD] = obj.UpdateTime;
                    dr[MSG_TYPE_FIELD] = obj.MsgType;
                    dr[LEVEL_FIELD] = obj.Level;
                    dr[HEADER_TITLE_FIELD] = obj.HeaderTitle;
                    dr[SECURITYID_FIELD] = obj.MessageContent.securityid;
                    dr[SOURCE_FIELD] = obj.source;
                    dr[STRATEGYORDERID_FIELD] = obj.MessageContent.id;
                    dr.EndEdit();
                    this.MsgDV.Table.AcceptChanges();
                    long end = DateTime.Now.Ticks;
                    Console.WriteLine("UpdateMsg time :" + (end - start) / 10000000.0);
                }
            }
            catch (Exception e)
            {
                LogHelper.Error("UpdateMsg Method", e);
            }
        }
        public void RequestMsg()
        {
            try
            {
                LogHelper.Info("历史消息加载开始");
                request.username = LocalEnv.UserInfo.UserName;
                request.pageSize = CommonConstants.pageCount;

                request.pageNo = Index;
                request.type = "1";
                request.sort = "1";
                //request.displayAll = "1";
                var reply = AdminHandler.Instance.SearchSysMsgmvt(request);

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
                        foreach (TTbookSystemMessage msg in reply.Datas)
                        {
                            lock (string.Intern(msg.id))
                            {
                                if (msg.content != null && msg.type.Equals("1"))
                                {
                                    MsgPo msgPo = ObjectConvert.HisSystemMsgToShowMsg(msg);
                                    if (msgPo != null)
                                    {
                                        UpdateMsg(msgPo);
                                        //_ProcessThreadQueue.Enqueue(msgPo);
                                    }
                                }
                            }
                        }
                        LogHelper.Info("历史消息加载完成");
                    }));                      
                }
                else
                {
                    LogHelper.Info("历史消息加载完成");
                }
            }
            catch (Exception e)
            {
                LogHelper.Error("RequestMsg", e);
            }
            finally
            {
                historyView.ShowMask(false);
                historyView.IsLoad = true;
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
                    TimeSpan ts1 = new TimeSpan(begin.Ticks);
                    TimeSpan ts2 = new TimeSpan(end.Ticks);
                    TimeSpan ts = ts1.Subtract(ts2).Duration();
                    int dateDiff = ts.Days;
                    if (dateDiff > 6)
                    {
                        showMessage("界面不支持7天以上消息筛选，请修改日期区间");
                        return;
                    }
                    Request.begintime = dateList[0].ToString() + " 00:00:00";
                    Request.endtime = dateList[1].ToString() + " 23:59:59";
                }
                else
                {
                    Request.begintime = day + " 00:00:00";
                    Request.endtime = day + " 23:59:59";
                }
            }
            IList venuesList = dealsFilterDic["ContentType"];
            StringBuilder inFilterSB_venues = new StringBuilder();
            if (venuesList.Count > 0)
            {
                foreach (ListBoxItem item in venuesList)
                {
                    if (inFilterSB_venues.Length > 0)
                    {
                        inFilterSB_venues.Append(",");
                    }

                    if (item.Content.ToString() == Constants.URGENT)
                    {
                        inFilterSB_venues.Append("\"type\":\"" +AdminConsts.Admin_Critical+"\"");
                    }
                    else if (item.Content.ToString() == Constants.MAJOR)
                    {
                        inFilterSB_venues.Append("\"type\":\"" + AdminConsts.Admin_Warn + "\"");
                    }
                    else if (item.Content.ToString() == Constants.GENERAL)
                    {
                        inFilterSB_venues.Append("\"type\":\"" + AdminConsts.Admin_Normal + "\"");
                    }              
                }
            }

            if (inFilterSB_venues.Length > 0)
            {
                Request.contentP1 = string.Format(inFilterSB_venues.ToString());
            }
            else
            {
                Request.contentP1 = "";
            }
            
            lock (MsgDV)
            {
                foreach (DataRow dr in this.MsgDV.Table.Rows)
                {
                    dr.BeginEdit();
                    dr.Delete();
                    dr.EndEdit();
                }
                //this.MsgDV.Table.Rows.Clear();
                this.MsgDV.Table.AcceptChanges();

            }
            System.Threading.ThreadPool.QueueUserWorkItem(o =>
            {
                try
                {
                    historyView.ShowMask(true);
                    Index = 0;
                    RequestMsg();
                }
                catch (Exception ex)
                {
                    LogHelper.Error(ex);
                }
            });
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
            saveFileDialog.FileName = DateTime.Now.ToString("yyyyMMddHHmmss") + "_历史消息记录";
            saveFileDialog.AddExtension = true;
            saveFileDialog.RestoreDirectory = true;
            if ((bool)(saveFileDialog.ShowDialog()))
            {
                fileName = saveFileDialog.FileName;
                ExportDV = MsgDV.Table.Clone();
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
                    SystemMessageReq req = new SystemMessageReq();
                    req.username = LocalEnv.UserInfo.UserName;
                    req.pageSize = 1;
                    req.pageNo = 1;
                    req.type = "1";
                    req.sort = "1";
                    //request.displayAll = "1";
                    req.begintime = Request.begintime;
                    req.endtime = Request.endtime;
                    req.contentP1 = Request.contentP1;
                    req.contentP2 = Request.contentP2;
                    ///获取订单 
                    var reply = AdminHandler.Instance.SearchSysMsgmvt(req);

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
                            req.pageNo = i;
                            req.pageSize = CommonConstants.pageCount;
                            var reply1 = AdminHandler.Instance.SearchSysMsgmvt(req);
                            if (reply1 != null && reply1.Datas != null && reply1.Datas.Count > 0)
                            {
                                foreach (TTbookSystemMessage obj in reply1.Datas)
                                {
                                    if (obj.content != null && obj.type.Equals("1"))
                                    {
                                        MsgPo msgPo = ObjectConvert.HisSystemMsgToShowMsg(obj);
                                        if (msgPo != null)
                                        {
                                            ExportPo po = toExport(msgPo);
                                            status = ExcelHelper.Instance.SaveDataToCSVFile(po, fileName, sw, count, Constants.HISMsgs);
                                            count++;
                                        }
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
        private ExportPo toExport(MsgPo obj)
        {
            ExportPo po = new ExportPo();
            po.ContentType = ComboSourceUtil.Instance.FindEnum(obj.ContentType, "contenttype");
            po.Level = ComboSourceUtil.Instance.FindEnum(obj.Level, "level");
            po.SecurityID = obj.MessageContent.securityid;
            po.HeaderTitle = obj.HeaderTitle;
            po.Content = obj.Content;
            po.Source = obj.source;
            po.StrategyOrderId = obj.MessageContent.id;           
            po.UpdateTime = TimeUtils.Instance.ToTime(obj.UpdateTime);
            po.UpdateDate = TimeUtils.Instance.ToDate(obj.UpdateTime);
            return po;
        }
    }
}
