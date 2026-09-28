using com.gateway.log;
using Microsoft.Win32;
using STC.Common.Data;
using STC.Common.Handler.SystemParam;
using STC.Common.Handler.Table;
using STC.Common.Model.Export;
using STC.Common.Model.Table;
using STC.Common.Utils;
using STC.Common.View.Component;
using STC.Common.View.UI;
using STC.Common.ViewModel;
using STC.InnerBoard.Plugin.Utils;
using STC.InnerBoard.Plugin.View;
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

namespace STC.InnerBoard.Plugin.ViewModel
{
    /// <summary>
    /// 风控记录 HistoryBrokerListView的数据内容
    /// </summary>
    public class HistoryBrokerListViewModel : ViewModelBase
    {
        public ICommand AddCommand { get { return new ComRelayCommand(OnAddClick); } }
        public ICommand DelCommand { get { return new ComRelayCommand(OnDelClick) ; } }


        private HistoryBrokerListView historyView;

        public HistoryBrokerListView HistoryView
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
        private BrokerReq request = new BrokerReq();
        public BrokerReq Request
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

        /// <summary>
        /// 选中的行数据
        /// </summary>
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
        public const string senderId_FIELD = "senderId";
        public const string senderName_FIELD = "senderName";
        public const string channel_FIELD = "channel";
        public const string text_FIELD = "text";
        public const string receiverID_FIELD = "receiverID";
        public const string receiverName_FIELD = "receiverName";
        public const string updateTime_FIELD = "updateTime";
        public const string AddBtnVisible_FIELD = "AddBtnVisible";
        public const string DelBtnVisible_FIELD = "DelBtnVisible";
        #endregion
        private DataView _BrokerHisDV;
        public DataView BrokerHisDV
        {
            get
            {
                lock (LockObj)
                {
                    if (_BrokerHisDV == null)
                    {
                        _BrokerHisDV = new DataTable().DefaultView;
                        _BrokerHisDV.Table.Columns.Add(KEY_FIELD, typeof(string));
                        _BrokerHisDV.Table.Columns.Add(senderId_FIELD, typeof(string));
                        _BrokerHisDV.Table.Columns.Add(senderName_FIELD, typeof(string));
                        _BrokerHisDV.Table.Columns.Add(channel_FIELD, typeof(string));
                        _BrokerHisDV.Table.Columns.Add(text_FIELD, typeof(string));
                        _BrokerHisDV.Table.Columns.Add(receiverID_FIELD, typeof(string));
                        _BrokerHisDV.Table.Columns.Add(receiverName_FIELD, typeof(string));
                        _BrokerHisDV.Table.Columns.Add(updateTime_FIELD, typeof(string));
                        _BrokerHisDV.Table.Columns.Add(AddBtnVisible_FIELD, typeof(string));
                        _BrokerHisDV.Table.Columns.Add(DelBtnVisible_FIELD, typeof(string));

                        _BrokerHisDV.Sort = $"{updateTime_FIELD} desc";
                        //_BrokerHisDV.Table.PrimaryKey = new DataColumn[] { _BrokerHisDV.Table.Columns[KEY_FIELD] };
                    }
                    return this._BrokerHisDV;
                }
            }
        }

        private ThreadQueue<BrokerQuoteRecord> _ProcessThreadQueue;
        public HistoryBrokerListViewModel()
        {
            BindingOperations.EnableCollectionSynchronization(BrokerHisDV, LockObj);
            _ProcessThreadQueue = new ThreadQueue<BrokerQuoteRecord>(UpdateData);
        
        }

        ~HistoryBrokerListViewModel()
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
                lock (BrokerHisDV)
                {
                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        foreach (DataRow dr in this.BrokerHisDV.Table.Rows)
                        {
                            dr.BeginEdit();
                            dr.Delete();
                            dr.EndEdit();
                        }
                        //this.BrokerHisDV.Table.Rows.Clear();
                        this.BrokerHisDV.Table.AcceptChanges();
                    }));
                }
            }
            catch (Exception e)
            {
                LogHelper.Error(e);
            }
        }
        private void UpdateData(BrokerQuoteRecord obj)
        {
            try
            {
                lock (this.LockObj)
                {
                    DataRow dr = this.BrokerHisDV.Table.NewRow();
                    dr.BeginEdit();
                    dr[KEY_FIELD] = obj.id;
                    dr[senderId_FIELD] = obj.senderId;
                    dr[senderName_FIELD] = obj.senderName;
                    dr[channel_FIELD] = obj.channel;
                    dr[text_FIELD] = obj.text;
                    dr[receiverID_FIELD] = obj.receiverID;
                    dr[receiverName_FIELD] = obj.receiverName;
                    dr[updateTime_FIELD] = obj.updateTime;
                    if (!string.IsNullOrEmpty(obj.IfWhiteList) && "1".Equals(obj.IfWhiteList))
                    {
                        setAddBtnVisible(dr, true);
                    }
                    else
                    {
                        setAddBtnVisible(dr, false);
                    }
                    //if (STC.Common.Utils.CommonUtils.BrokerDic.ContainsKey(obj.senderId))
                    //{
                    //    setAddBtnVisible(dr, true);
                    //}
                    //else
                    //{
                    //    setAddBtnVisible(dr, false);
                    //}
                    this.BrokerHisDV.Table.Rows.Add(dr);
                    dr.EndEdit();
                    this.BrokerHisDV.Table.AcceptChanges();
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
                request.PageSize = CommonConstants.pageCount.ToString();
                request.PageNum = Index.ToString();
                request.UserName = LocalEnv.UserInfo.UserName;
                request.Channel = "FINQ";
                ///获取成交
                BrokerQuoteRecordReply reply = TableHandler.Instance.queryBrokerHis(request);
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
                        foreach (BrokerQuoteRecord msg in reply.Datas)
                        {
                            UpdateData(msg);
                            //_ProcessThreadQueue.Enqueue(msg);
                        }
                    }));
                }
            }
            catch (Exception e)
            {
                LogHelper.Error("RequestMsg", e);
            }
            finally
            {
             
                historyView.ShowMask(false);
            }
        }

        internal void ProcessMsgsFilterCondition(Dictionary<string, IList> dealsFilterDic)
        {
            Request.IfWhiteList = "";
            if (dealsFilterDic.ContainsKey("Status"))
            {
                IList statusList = dealsFilterDic["Status"];
                StringBuilder inFilterSB_venues = new StringBuilder();
                if (statusList.Count == 1)
                {
                    foreach (ListBoxItem item in statusList)
                    {
                        if (inFilterSB_venues.Length > 0)
                        {
                            inFilterSB_venues.Append(",");
                        }

                        if (item.Content.ToString() == Constants.ADDED)
                        {
                            Request.IfWhiteList = "1";
                        }
                        else if (item.Content.ToString() == Constants.UNADD)
                        {
                            Request.IfWhiteList = "0";
                        }
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

        private async void OnAddClick()
        {
            if (this.SelectedItem != null)
            {
                string qq = this.SelectedItem[senderId_FIELD] + "";
                string qqName = this.SelectedItem[senderName_FIELD] + "";
                string info = string.Format("是否将以下来源加入自动发布名单?\r\n个人QQ {0}({1})", qq, qqName);
                if (!Common.View.Component.CustomMessageBox.Question(info, "加入"))
                {
                    return;
                }
                else
                {
                    BrokerWhiteList req = new BrokerWhiteList();
                    req.brokerId = this.SelectedItem[senderId_FIELD] + "";
                    req.brokerName = this.SelectedItem[senderName_FIELD] + "";
                    req.whiteListEnable = 1;
                    req.username = LocalEnv.UserInfo.UserName;
                    req.userId = LocalEnv.UserInfo.UserId;
                    req.whiteListType = "FINQ";
                    req.updateType = "";
                    req.action = 1;
                    BrokerWhiteReplyPo reply = await UpdateBrokerWhite(req);
                    if (reply != null)
                    {
                        if (reply.Code == "0000")
                        {

                            STC.Common.View.Component.CustomMessageBox.Info($"加入成功!\r\n{reply.Msg}", "提示", null);
                            setAddBtnVisible(null, true);
                            System.Threading.ThreadPool.QueueUserWorkItem(o =>
                            {
                                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                                {
                                    DesignHandler.Instance.SendBrokerSettingChange(req);
                                }));
                            });
                        }
                        else
                        {

                            STC.Common.View.Component.CustomMessageBox.Error($"加入失败!\r\n{reply.Msg}", "提示", null);
                        }
                    }
                }
            }
        }

        private void setAddBtnVisible(DataRow dr, bool isVisible)
        {
            if (dr != null)
            {
                if (isVisible)
                {
                    dr[AddBtnVisible_FIELD] = "false";
                    dr[DelBtnVisible_FIELD] = "true";
                }
                else
                {
                    dr[AddBtnVisible_FIELD] = "true";
                    dr[DelBtnVisible_FIELD] = "false";
                }
            }
            else
            {
                if (isVisible)
                {
                    this.SelectedItem[AddBtnVisible_FIELD] = "false";
                    this.SelectedItem[DelBtnVisible_FIELD] = "true";
                }
                else
                {
                    this.SelectedItem[AddBtnVisible_FIELD] = "true";
                    this.SelectedItem[DelBtnVisible_FIELD] = "false";
                }
            }
        }

        public async Task<BrokerWhiteReplyPo> UpdateBrokerWhite(BrokerWhiteList req)
        {
            BrokerWhiteReplyPo reply = null;

            await Task.Run(() =>
            {
                reply = TableHandler.Instance.brokerWhite(req);
            });
            return reply;
        }

        private async void OnDelClick()
        {
            if (this.SelectedItem != null)
            {
                string qq = this.SelectedItem[senderId_FIELD] + "";
                string info = string.Format("是否将?\r\n个人QQ{0}从自动发布名单取消", qq);
                if (!Common.View.Component.CustomMessageBox.Question(info, "取消"))
                {
                    return;
                }
                else
                {
                    BrokerWhiteList req = new BrokerWhiteList();
                    if (STC.Common.Utils.CommonUtils.BrokerDic.ContainsKey(qq))
                    {
                        BrokerWhiteList bw = STC.Common.Utils.CommonUtils.BrokerDic[qq];
                        req.id = bw.id;
                        req.brokerId = bw.brokerId;
                        req.brokerName = bw.brokerName;
                        req.whiteListType = bw.whiteListType;
                        req.updateType = bw.updateType;
                        req.username = LocalEnv.UserInfo.UserName;
                        req.userId = LocalEnv.UserInfo.UserId;
                    }
                    req.action = 3;
                    BrokerWhiteReplyPo reply = await UpdateBrokerWhite(req);
                    if (reply != null)
                    {
                        if (reply.Code == "0000")
                        {
                            STC.Common.View.Component.CustomMessageBox.Info($"取消成功!\r\n{reply.Msg}", "提示", null);
                            setAddBtnVisible(null, false);
                            System.Threading.ThreadPool.QueueUserWorkItem(o =>
                            {
                                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                                {
                                    DesignHandler.Instance.SendBrokerSettingChange(req);
                                }));
                            });
                        }
                        else
                        {
                            STC.Common.View.Component.CustomMessageBox.Error($"取消失败!\r\n{reply.Msg}", "提示", null);
                        }
                    }
                }
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

      
    }
}
