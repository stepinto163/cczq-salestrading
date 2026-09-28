using Ats.Foundation.Message;
using com.gateway.log;
using BDB.Trade.Plugin.CustomeUI;
using BDB.Trade.Plugin.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using BDB.Price.Plugin.Cache;
using BDB.Price.Plugin.View.Trade.Card;
using STC.Common.Model.Admin;
using STC.Common.Utils;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.GW;
using STC.Common.Handler.GW;
using STC.Common.Handler.TBook;
using STC.Common.Model.Inner;
using STC.Common.Handler.Inner;
using STC.Common.Handler.Admin;
using STC.Frame.Common;
using STC.Common.Model;
using STC.Common.Handler.Base;
/// <summary>
/// 待办事项, 消息提醒
/// </summary>
namespace BDB.Trade.Plugin.CustomUI
{
    /// <summary>
    /// MsgDetailUI.xaml 的交互逻辑
    /// </summary>
    public partial class MsgDetailUI : UserControl
    {
        #region 重构逻辑
        private int _MaxCapacity = 500;
        private string _currentStartTime = null;
        private int _currentPageNum = 0;
        public event Action<string, string, int> OnScrollChanged;
        private Thread _reafreshThread = null;
        private bool _isRefresh = true;

        private CollectionViewSource _filteredMsgView = new CollectionViewSource();
        public CollectionViewSource FilteredMsgView
        {
            get { return _filteredMsgView; }
            set { _filteredMsgView = value; }
        }

        private string _filterLevel = string.Empty;

        public MsgDetailUI()
        {
            InitializeComponent();
            try
            {
                this._filteredMsgView.IsLiveFilteringRequested = true;
                this._filteredMsgView.IsLiveSortingRequested = false;
                this._filteredMsgView.Source = showData;
                this._filteredMsgView.Filter += _filteredMsgView_Filter;

                this.Unloaded += MsgDetailUI_Unloaded;

                // 时间倒序
                SortDescription timeDesc = new SortDescription("UpdateTime", ListSortDirection.Descending);
                this._filteredMsgView.SortDescriptions.Add(timeDesc);

                dgMsg.ItemsSource = FilteredMsgView.View;

                dgMsg.AddHandler(ScrollViewer.ScrollChangedEvent, new ScrollChangedEventHandler(ScrollChangeed));
            }
            catch (Exception e)
            {
                LogHelper.Error(e);
            }
        }

        private void MsgDetailUI_Unloaded(object sender, RoutedEventArgs e)
        {
            this.Unloaded -= MsgDetailUI_Unloaded;
            //if (_reafreshThread != null)
            //{
            //    _isRefresh = false;
            //    _reafreshThread.Abort();
            //    _reafreshThread = null;
            //}
        }

        public void dispose()
        {
            if (_reafreshThread != null)
            {
                _isRefresh = false;
                _reafreshThread.Abort();
                _reafreshThread = null;
            }
        }

        private bool _isLoad = false;
        public bool IsLoad { get { return _isLoad; } set { _isLoad = value; } }
        private bool _isNeedFilter = true;

        private void _filteredMsgView_Filter(object sender, FilterEventArgs e)
        {
            if (_isNeedFilter)
            {
                var msgPo = e.Item as MsgPo;

                e.Accepted = string.IsNullOrEmpty(_filterLevel) || _filterLevel.Equals(msgPo.Level);
            }
        }

        private void ScrollChangeed(object sender, ScrollChangedEventArgs e)
        {
            ScrollViewer sv = e.OriginalSource as ScrollViewer;

            if (sv.ScrollableHeight > 0 && Math.Round(sv.ScrollableHeight, 1) == Math.Round(e.VerticalOffset, 1) && !_isLoad)
            {
                List<MsgPo> msgList = showData.ToList();
                msgList.Sort(new TimeSort());
                MsgPo lastMsg = msgList.Last();


                _isLoad = true;
                _currentPageNum++;
                OnScrollChanged?.Invoke(CurrentType, lastMsg.UpdateTime, _currentPageNum);

                if (OnScrollChanged == null)
                    _isLoad = false;
            }

        }

        #endregion



        private ConcurrentDictionary<string, MsgPo> msgDic = new ConcurrentDictionary<string, MsgPo>();
        private ObservableCollection<MsgPo> showData = new ObservableCollection<MsgPo>();
        private Thread _refreshThread = null;
        private object LockDict = new object();
        /// <summary>
        /// 1：消息提醒
        /// 2：待办事项
        /// </summary>
        private string CurrentType = "1";

        private bool isInit = true;

        private void RefereshTimeOut()
        {

        }

        public void Clear()
        {
            this.Dispatcher.Invoke(() =>
            {
                msgDic.Clear();
                showData.Clear();

                //dgMsg.DataContext = null;

                lbCondition.SelectedIndex = 0;
            });
        }

        public void SetCurrentType(string type)
        {
            CurrentType = type;

            if (CurrentType.Equals("1"))
            {
                btnbatchClear.Visibility = Visibility.Visible;
            }
            else
            {
                btnbatchClear.Content = "批量忽略";

                lbUrgent.Visibility = Visibility.Collapsed;

                _reafreshThread = new Thread(RefreshTime);
                _reafreshThread.IsBackground = true;
                _reafreshThread.Start();
            }
        }

        public ObservableCollection<MsgPo> ShowData { get { return showData; } }
        public List<MsgPo> GetAllData()
        {
            return msgDic.Values.ToList();
        }

        public List<MsgPo> GetRealNotShowData()
        {
            return msgDic.Values.Where(delegate (MsgPo msgPo)
            {
                if (msgPo.IsPopShow)
                    return false;
                else
                    return true;
            }).ToList();
        }

        public int GetShowMsgCountDetail(bool filterAll, string warnLevel)
        {
            List<MsgPo> selectMsgs = msgDic.Values.Where(delegate (MsgPo msgPo)
            {
                if (filterAll)
                {
                    if (!string.IsNullOrEmpty(msgPo.Level) && (msgPo.Level.Equals("1") || msgPo.Level.Equals("2")))
                        //&& (string.IsNullOrEmpty(msgPo.readFlag) || msgPo.readFlag.Equals("0")))//!msgPo.readFlag.Contains(LocalEnv.UserInfo.UserId)))
                        return true;
                    else
                        return false;
                }
                else
                {
                    if (!string.IsNullOrEmpty(msgPo.Level) && (msgPo.Level.Equals(warnLevel)))
                        //&& (string.IsNullOrEmpty(msgPo.readFlag) || msgPo.readFlag.Equals("0")))//!msgPo.readFlag.Contains(LocalEnv.UserInfo.UserId)))
                        return true;
                    else
                        return false;
                }

            }).ToList();

            return selectMsgs.Count;
        }

        public int GetShowMsgCount(bool filterAll, string warnLevel)
        {
            ObservableCollection<MsgPo> Tmp = new ObservableCollection<MsgPo>(msgDic.Values);
            if (!string.IsNullOrEmpty(warnLevel))
            {
                ObservableCollection<MsgPo> targetTmp = new ObservableCollection<MsgPo>(Tmp.Where(p => p.Level.Equals(warnLevel) && p.ReadMsgVisible == Visibility.Visible));
                return targetTmp == null ? 0 : targetTmp.Count;
            }
            else
            {
                ObservableCollection<MsgPo> targetTmp = new ObservableCollection<MsgPo>(Tmp.Where(p => p.ReadMsgVisible == Visibility.Visible));
                return targetTmp == null ? 0 : targetTmp.Count;
            }
        }

        private void SetStartTime(string endTime)
        {
            if (string.IsNullOrEmpty(_currentStartTime) || _currentStartTime.CompareTo(endTime) > 0)
                _currentStartTime = endTime;
        }

        private void DealAllMsg(MsgPo msgPo, Dictionary<string, TTbookSystemMessage> removeDic, Dictionary<string, MsgPo> updateDic)
        {
            int selectIndex = lbCondition.SelectedIndex;
            if (!string.IsNullOrEmpty(msgPo.ReadFlag))
            {
                MsgPo tem = null;
                string[] arr = msgPo.ReadFlag.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                if (arr != null && arr.Contains(LocalEnv.UserInfo.UserName))//全局消息的删除操作，看readflag是否包含当前账号
                {
                    if (msgDic.ContainsKey(msgPo.Id) || (!string.IsNullOrEmpty(_filterLevel) && !_filterLevel.Equals(msgPo.Level)))
                    {
                        removeDic.Add(msgPo.Id, null);

                        if (msgDic.ContainsKey(msgPo.Id))
                            msgDic.TryRemove(msgPo.Id, out tem);
                    }
                }
                else if (selectIndex == 0 || (selectIndex == 1 && msgPo.Level.Equals("1")) || (selectIndex == 2 && msgPo.Level.Equals("2"))
                    || (selectIndex == 3 && msgPo.Level.Equals("3")))
                {
                    if (msgDic.ContainsKey(msgPo.Id))
                    {
                        updateDic.Add(msgPo.Id, msgPo);
                    }
                    else
                    {
                        CheckCacheCapacity(true);

                        showData.Add(msgPo);
                    }
                    msgDic.AddOrUpdate(msgPo.Id, msgPo, (key, value) => msgPo);
                }
            }
            else
            {
                if (selectIndex == 0 || (selectIndex == 1 && msgPo.Level.Equals("1")) || (selectIndex == 2 && msgPo.Level.Equals("2"))
                    || (selectIndex == 3 && msgPo.Level.Equals("3")))
                {
                    if (msgDic.ContainsKey(msgPo.Id))
                    {
                        updateDic.Add(msgPo.Id, msgPo);
                    }
                    else
                    {
                        CheckCacheCapacity(true);

                        showData.Add(msgPo);
                    }
                }

                msgDic.AddOrUpdate(msgPo.Id, msgPo, (key, value) => msgPo);
            }
        }

        private void DealRFQMsg(MsgPo msgPo, Dictionary<string, TTbookSystemMessage> removeDic, Dictionary<string, MsgPo> updateDic)
        {
            MsgPo tem = null;
            string[] arr = msgPo.ReadFlag.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
            if (arr != null)//全局消息的删除操作
            {
                if (msgDic.ContainsKey(msgPo.Id))
                {
                    removeDic.Add(msgPo.Id, null);

                    if (msgDic.ContainsKey(msgPo.Id))
                        msgDic.TryRemove(msgPo.Id, out tem);
                }
            }
        }

        private void CheckCacheCapacity(bool checkShowData)
        {
            if (checkShowData)
            {
                if (showData.Count >= _MaxCapacity)
                {
                    List<MsgPo> msgList = showData.ToList();
                    msgList.Sort(new TimeSort());
                    MsgPo removeMsg = msgList.Last();
                    showData.Remove(removeMsg);
                    MsgPo temMsg = null;
                    msgDic.TryRemove(removeMsg.Id, out temMsg);
                }
            }

            int count = CurrentType.Equals("1") ? _MaxCapacity * 3 : _MaxCapacity * 2;
            if (msgDic.Keys.Count >= count)
            {
                List<MsgPo> msgList = msgDic.Values.ToList();
                msgList.Sort(new TimeSort());

                MsgPo lastMsg = msgList.Last();
                MsgPo temMsg = null;
                msgDic.TryRemove(lastMsg.Id, out temMsg);
                if (temMsg != null)
                    showData.Remove(temMsg);
            }
        }

        private void DealIndividualMsg(MsgPo msgPo, Dictionary<string, TTbookSystemMessage> removeDic, Dictionary<string, MsgPo> updateDic)
        {
            int selectIndex = lbCondition.SelectedIndex;
            MsgPo tem = null;
            if (msgDic.ContainsKey(msgPo.Id))
            {
                if (msgPo.DelFlag.Equals("1") || (!string.IsNullOrEmpty(msgPo.ReadFlag) && msgPo.ReadFlag.Contains(LocalEnv.UserInfo.UserName))
                    || (!string.IsNullOrEmpty(_filterLevel) && !_filterLevel.Equals(msgPo.Level)))
                    removeDic.Add(msgPo.Id, null);
                else
                    updateDic.Add(msgPo.Id, msgPo);
            }
            else if (!string.IsNullOrEmpty(msgPo.DelFlag) && !msgPo.DelFlag.Equals("1") && !(!string.IsNullOrEmpty(msgPo.ReadFlag) && msgPo.ReadFlag.Contains(LocalEnv.UserInfo.UserName)))
            {
                if (selectIndex == 0 || (selectIndex == 1 && msgPo.Level.Equals("1")) || (selectIndex == 2 && msgPo.Level.Equals("2"))
                    || (selectIndex == 3 && msgPo.Level.Equals("3")))
                {
                    CheckCacheCapacity(true);
                    showData.Add(msgPo);
                }
            }

            if (!string.IsNullOrEmpty(msgPo.DelFlag) && msgPo.DelFlag.Equals("1") || (!string.IsNullOrEmpty(msgPo.ReadFlag) && msgPo.ReadFlag.Contains(LocalEnv.UserInfo.UserName)))
                msgDic.TryRemove(msgPo.Id, out tem);
            else
            {
                if (!msgDic.ContainsKey(msgPo.Id))
                    CheckCacheCapacity(false);

                msgDic.AddOrUpdate(msgPo.Id, msgPo, (key, value) => msgPo);
            }
        }

        public void OnMsg(List<TTbookSystemMessage> sysMsg)
        {
            this.Dispatcher.Invoke(() =>
            {
                int selectIndex = lbCondition.SelectedIndex;
                Dictionary<string, MsgPo> updateDic = new Dictionary<string, MsgPo>();
                Dictionary<string, TTbookSystemMessage> removeDic = new Dictionary<string, TTbookSystemMessage>();
                int total = isInit ? (sysMsg.Count > _MaxCapacity ? _MaxCapacity : sysMsg.Count) : sysMsg.Count;

                lock (showData)
                {
                    for (int i = 0; i < total; i++)
                    {
                        TTbookSystemMessage msg = sysMsg[i];
                        if (msg.content != null && msg.type.Equals(CurrentType))
                        {
                            MsgPo msgPo = ObjectConvert.SystemMsgToShowMsg(msg, this);

                            if (msgPo != null && !(!string.IsNullOrEmpty(msgPo.Content) && (msgPo.Content.Contains("CDH") || msgPo.Content.Contains("SDN") || msgPo.Content.Contains("MD") || msgPo.Content.Contains("EMS"))))//过滤CDH、SDN消息
                            {
                                // 加入删除逻辑
                                string isdel = msg.delflag ?? "0";
                                if (isdel == "1")
                                {
                                    if (msgDic.ContainsKey(msg.id))
                                    {
                                        removeDic.Add(msg.id, msg);
                                        MsgPo tem = null;
                                        msgDic.TryRemove(msg.id, out tem);
                                    }

                                    continue;
                                }

                                if (CurrentType.Equals("1"))
                                {
                                    if (msgPo.username.Equals("*"))
                                        DealAllMsg(msgPo, removeDic, updateDic);
                                    else
                                        DealIndividualMsg(msgPo, removeDic, updateDic);
                                }
                            }
                            else
                            {
                                if (msgDic.ContainsKey(msg.id))
                                {
                                    if (msg.delflag.Equals("1"))
                                    {
                                        removeDic.Add(msg.id, msg);
                                        MsgPo tem = null;
                                        msgDic.TryRemove(msg.id, out tem);
                                    }
                                }
                            }

                        }
                    }

                    if (updateDic.Count > 0 || removeDic.Count > 0)
                    {
                        //this._filteredMsgView.IsLiveSortingRequested = false;
                        List<MsgPo> removeList = new List<MsgPo>();
                        int index = 0;
                        foreach (MsgPo ms in showData)
                        {
                            if (updateDic.ContainsKey(ms.Id))
                            {
                                ObjectConvert.UpdateMsg(ms, updateDic[ms.Id]);
                                index++;

                            }
                            else if (removeDic.ContainsKey(ms.Id))
                                removeList.Add(ms);
                        }
                        if (removeList.Count > 0)
                        {
                            foreach (MsgPo ms in removeList)
                            {
                                showData.Remove(ms);
                            }
                        }
                    }



                    if (isInit)
                    {
                        isInit = false;
                        this._filteredMsgView.IsLiveSortingRequested = true;
                    }
                }
            });
        }

        public void RefreshData()
        {
            List<MsgPo> tem = showData.ToList();
            tem.Sort(new MsgSort());
            showData = new ObservableCollection<MsgPo>(tem);
            dgMsg.DataContext = null;
            dgMsg.DataContext = showData;
        }



        private void lbCondition_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (isInit)
                return;

            _isNeedFilter = false;
            this._filteredMsgView.IsLiveFilteringRequested = false;
            this._filteredMsgView.IsLiveSortingRequested = false;
            showData.Clear();


            ///
            int selectIndex = lbCondition.SelectedIndex;
            if (selectIndex == 0)
                _filterLevel = string.Empty;
            else if (selectIndex == 1)
                _filterLevel = "1";
            else if (selectIndex == 2)
                _filterLevel = "2";
            else if (selectIndex == 3)
                _filterLevel = "3";

            List<MsgPo> temDatas = msgDic.Values.Where(delegate (MsgPo msgPo)
            {
                if (string.IsNullOrEmpty(_filterLevel) || _filterLevel.Equals(msgPo.Level))
                    return true;
                else
                    return false;
            }).ToList();

            showData = new ObservableCollection<MsgPo>(temDatas);
            
            _filteredMsgView.Source = showData;
            dgMsg.ItemsSource = FilteredMsgView.View;
            //foreach (MsgPo msgPo in temDatas)
            //{
            //    if (string.IsNullOrEmpty(_filterLevel) || _filterLevel.Equals(msgPo.Level))
            //        showData.Add(msgPo);
            //}

            this._filteredMsgView.IsLiveFilteringRequested = true;
            this._filteredMsgView.IsLiveSortingRequested = true;
            _isNeedFilter = true;
            _filteredMsgView.View.Refresh();

        }

        /// <summary>
        /// 处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDeal_Click(object sender, RoutedEventArgs e)
        {
            MsgPo msgPo = dgMsg.Items.CurrentItem as MsgPo;
            if (msgPo != null)
            {
                //设置消息为已读
                SetReadflag(msgPo);

                //增加消息复制
                if (!string.IsNullOrEmpty(msgPo.SignalDetail))
                {
                    try
                    {
                        SystemMessageDetail systemMessageDetail = MsgHelper.Deserialize<SystemMessageDetail>(msgPo.SignalDetail, Ats.Foundation.Message.Serializer.MsgSerializerType.Json);
                        if (!string.IsNullOrEmpty(systemMessageDetail.EventName) && systemMessageDetail.EventName.ToUpper() == ("clipboard").ToUpper())
                            Trade.Plugin.Utils.ClipboardHelper.CopyToClipboard(systemMessageDetail.Data);
                    }
                    catch (Exception ex)
                    {
                    }
                }


                if (msgPo.ContentType.Equals(AdminConsts.Admin_ER))
                {
                    ExecutionReport er = null;
                    if (!string.IsNullOrEmpty(msgPo.Detail))
                    {
                        er = MsgHelper.Deserialize<ExecutionReport>(msgPo.Detail);
                    }

                    if (!WindowManager.Instance.IsExist(WindowKeyHelper.GetExecBackWinKey(er != null ? er.ExecID : string.Empty)))
                    {
                        ExecuteReplyView executeReplyView = null;

                        if (msgPo.Level.Equals("3"))
                            executeReplyView = new ExecuteReplyView(msgPo, true);
                        else
                            executeReplyView = new ExecuteReplyView(msgPo);

                        WindowManager.Instance.OpenOnlyOneWindow(WindowKeyHelper.GetExecBackWinKey(er != null ? er.ExecID : string.Empty), executeReplyView);
                        //executeReplyView.Show();
                    }
                }
                else if (msgPo.ContentType.Equals(AdminConsts.Admin_IT))
                {
                    //点价提醒
                    //Common.Handler.Drp.DrpHandler.Instance.ShowTrayNotify(new Common.View.Drp.NotificationContent() { Id = msgPo.Id, ContentType = msgPo.ContentType, Detail = msgPo.Detail, IsShow = msgPo.IsShow ?? true, DelFlag = msgPo.DelFlag ?? "0", Message = msgPo.MessageContent.content, Title = msgPo.HeaderTitle });
                }

                else if (msgPo.ContentType.Equals(AdminConsts.Admin_EV))
                {
                    ThreadPool.QueueUserWorkItem(o =>
                    {
                        try
                        {
                            SignalMsgPo signalMsgPo = MsgHelper.Deserialize<SignalMsgPo>(msgPo.SignalDetail);
                            STC.Common.Handler.Base.BaseHandler.Instance.PublishAppMsg(new PResult() { Code = "PopupBDBQuotesMiniWindows", Info1 = signalMsgPo.data });
                        }
                        catch (Exception ex)
                        {
                            LogHelper.Error(ex);
                        }

                    });
                }
                else
                {
                    if (!WindowManager.Instance.IsExist(WindowKeyHelper.GetPriceMatchWinKey(msgPo.Id)))
                    {
                        PriceMatchView priceMatchView = new PriceMatchView(msgPo);
                        WindowManager.Instance.OpenOnlyOneWindow(WindowKeyHelper.GetExecBackWinKey(msgPo.Id), priceMatchView);
                    }
                }
            }
        }
        
        private void btnKnow_Click(object sender, RoutedEventArgs e)
        {
            MsgPo msgPo = dgMsg.Items.CurrentItem as MsgPo;
            KnowMsg(msgPo);
        }

        /// <summary>
        /// 清除或我知道了
        /// </summary>
        /// <param name="msgPo"></param>
        private void KnowMsg(MsgPo msgPo)
        {
            if (msgPo != null)
            {
                //设置消息为已读
                SetReadflag(msgPo);

                //nonfirm的价格匹配 忽略调用TBookSvr的删除接口
                if (msgPo.MessageContent != null && !string.IsNullOrEmpty(msgPo.MessageContent.contentType) && msgPo.MessageContent.contentType.Equals(AdminConsts.PriceMatch)
                    && msgPo.Content.ToUpper().Contains("NONFIRM"))
                {
                    PriceMatchDetail priceMatchDetail = null;
                    try
                    {
                        priceMatchDetail = MsgHelper.Deserialize<PriceMatchDetail>(msgPo.MessageContent.detail);
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Error(ex);
                    }

                    if (priceMatchDetail != null && priceMatchDetail.order != null && !string.IsNullOrEmpty(priceMatchDetail.order.OrderId))
                    {
                        NewOrderSingle order = new NewOrderSingle();
                        order.AlgoName = "CROSS";
                        order.StrategyOrderId = priceMatchDetail.order.OrderId;

                        ThreadPool.QueueUserWorkItem(o =>
                        {
                            try
                            {
                                PResult result = TBookHandler.Instance.updateOrder(order);

                            }
                            catch (Exception ex)
                            {
                                LogHelper.Error(ex);
                            }
                            finally
                            {
                                this.Dispatcher.Invoke(() =>
                                {
                                    btnbatchClear.IsEnabled = true;
                                });
                            }

                        });
                    }
                }
                else if (msgPo.ContentType.Equals(AdminConsts.Admin_IT))
                {
                    PriceMatchDetail priceMatch = null;
                    if (!string.IsNullOrEmpty(msgPo.Detail))
                    {
                        priceMatch = MsgHelper.Deserialize<PriceMatchDetail>(msgPo.Detail);
                    }

                    if (priceMatch != null && priceMatch.iocOrder != null)
                    {

                        ThreadPool.QueueUserWorkItem(o =>
                        {
                            try
                            {
                                InnerOrderReq innerOrderReq = new InnerOrderReq();
                                innerOrderReq.OrderID = priceMatch.iocOrder.OrderId;

                                InnerHandler.Instance.FakCancelOrder(innerOrderReq);
                            }
                            catch (Exception ex)
                            {
                                LogHelper.Error(ex);
                            }
                            finally
                            {
                                this.Dispatcher.Invoke(() =>
                                {
                                    btnbatchClear.IsEnabled = true;
                                });
                            }

                        });
                    }
                }
                else
                {
                    TBatchMsgReq batchMsgReq = new TBatchMsgReq();
                    batchMsgReq.delflag = "1";
                    batchMsgReq.readflag = "1";
                    batchMsgReq.id = new List<string>();

                    batchMsgReq.id.Add(msgPo.Id);

                    UpdateMsg(batchMsgReq);
                }
            }
        }

        private void UpdateMsg(TBatchMsgReq request)
        {
            ThreadPool.QueueUserWorkItem(o =>
            {
                try
                {
                    PResult result = AdminHandler.Instance.BatchUpdateSysMsg(request);

                    if (result != null && result.IsSuccess())
                    {
                        RemoveMsg(request);
                    }
                    else
                    {
                        //定向回价 已忽略消息可能删除失败
                        RemoveMsg(request);
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.Error(ex);
                }
                finally
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        btnbatchClear.IsEnabled = true;
                    });
                }

            });
        }

        private void RemoveMsg(TBatchMsgReq request)
        {
            this.Dispatcher.Invoke(() =>
            {
                List<MsgPo> removeList = new List<MsgPo>();
                foreach (MsgPo ms in showData)
                {
                    foreach (string id in request.id)
                    {
                        if (ms.Id.Equals(id))
                            removeList.Add(ms);
                    }
                }
                if (removeList.Count > 0)
                {
                    foreach (MsgPo msgPo in removeList)
                    {
                        showData.Remove(msgPo);

                        if (msgDic.ContainsKey(msgPo.Id))
                        {
                            MsgPo tem = null;
                            msgDic.TryRemove(msgPo.Id, out tem);
                        }
                    }
                }
            });
        }
        private void btnbatchClear_Click(object sender, RoutedEventArgs e)
        {
            btnbatchClear.IsEnabled = false;
            //List<MsgPo> normalMsg = showData.Where(delegate(MsgPo msgPo)
            //{
            //    if (msgPo.Level.Equals("3"))
            //        return true;
            //    else
            //        return false;
            //}).ToList();

            if (showData != null && showData.Count > 0)
            {
                TBatchMsgReq batchMsgReq = new TBatchMsgReq();
                batchMsgReq.delflag = "1";
                batchMsgReq.readflag = "1";

                batchMsgReq.id = new List<string>();

                foreach (MsgPo msgPo in showData)
                {
                    batchMsgReq.id.Add(msgPo.Id);
                }

                UpdateMsg(batchMsgReq);

            }
            else
                btnbatchClear.IsEnabled = true;
        }

        private void btnHistory_Click(object sender, RoutedEventArgs e)
        {
            int historyKey = (int)PluginKeyEnum.HistoryPlugin;
            TabItem selectItem = AppUtils.Application.Panel.SelectedItem as TabItem;
            bool isExit = false;
            if (selectItem != null)
            {
                int selectPluginkey = (int)selectItem.Tag;
                if (historyKey != selectPluginkey)
                {
                    foreach (TabItem tabItem in AppUtils.Application.Panel.Items)
                    {
                        selectPluginkey = (int)tabItem.Tag;
                        if (historyKey == selectPluginkey)
                        {
                            AppUtils.Application.Panel.SelectedItem = tabItem;
                            isExit = true;
                            break;
                        }
                    }
                }
                else
                    return;
            }

            if (!isExit)
            {
                IPlugin plugin = AppUtils.Application.PluginManager.GetPlugin((int)PluginKeyEnum.HistoryPlugin);
                if (plugin != null)
                {
                    AppUtils.Application.PluginManager.EnablePlugin((int)PluginKeyEnum.HistoryPlugin);

                    if (AppUtils.Application.Panel.Items.Count > 0)
                    {
                        foreach (TabItem tabItem in AppUtils.Application.Panel.Items)
                        {
                            int selectPluginkey = (int)selectItem.Tag;
                            if (historyKey == selectPluginkey)
                            {
                                AppUtils.Application.Panel.SelectedItem = tabItem;
                                isExit = true;
                                break;
                            }
                        }
                    }
                }
            }
        }

        private void RefreshTime()
        {
            while (_isRefresh)
            {
                try
                {
                    Thread.Sleep(1000);

                    lock (showData)
                    {
                        List<MsgPo> selectMsg = showData.Where(delegate (MsgPo msgPo)
                        {
                            return msgPo.ContentType.Equals(AdminConsts.Admin_IT) && !msgPo.TimeOut;
                        }).ToList();

                        if (selectMsg != null && selectMsg.Count > 0)
                        {
                            foreach (MsgPo msgPo in selectMsg)
                            {
                                PriceMatchDetail priceMatch = null;
                                if (!string.IsNullOrEmpty(msgPo.Detail))
                                {
                                    priceMatch = MsgHelper.Deserialize<PriceMatchDetail>(msgPo.Detail);
                                }

                                if (priceMatch != null && priceMatch.iocOrder != null)
                                {
                                    DateTime transactTime = CommonUtils.ConvertToDateTime(priceMatch.iocOrder.TransactTime);
                                    TimeSpan diffTime;
                                    try
                                    {
                                        diffTime = DateTimeEx.ServerTimeNow - transactTime;
                                    }
                                    catch
                                    {
                                        diffTime = DateTimeEx.ServerTime - transactTime;
                                    }

                                    double d = 0;
                                    if (double.TryParse(priceMatch.iocOrder.IocTimeOut, out d))
                                    {
                                        double result = d - (diffTime.TotalSeconds > 0 ? diffTime.TotalSeconds : 0);
                                        msgPo.TimeOutTxt = string.Format("{0}", result >= 0 ? result.ToString("N0") : "0");

                                        if (result < 0)
                                        {
                                            Application.Current.Dispatcher.Invoke(() =>
                                            {
                                                msgPo.TimeOut = true;

                                            });
                                        }
                                    }
                                    else
                                        msgPo.TimeOut = true;

                                }
                            }
                        }

                        selectMsg = showData.Where(delegate (MsgPo msgPo)
                        {
                            return msgPo.TimeOut;
                        }).ToList();

                        if (selectMsg != null && selectMsg.Count > 0 && Application.Current != null)
                        {
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                foreach (MsgPo msgPo in selectMsg)
                                    showData.Remove(msgPo);

                            });
                        }

                    }

                }
                catch (ThreadAbortException ex)
                {
                    //LogHandler.Error(ex);
                    Thread.Sleep(100);
                }
                catch (Exception otherEx)
                {
                    LogHandler.Error(otherEx);
                }
            }
        }

        private void BtnPopDeal_Click(object sender, RoutedEventArgs e)
        {
            MsgPo msgPo = dgMsg.Items.CurrentItem as MsgPo;
            if (msgPo != null)
            {
                if (msgPo.ContentType.Equals(AdminConsts.Admin_IT))
                {
                    PriceMatchDetail priceMatch = null;
                    if (!string.IsNullOrEmpty(msgPo.Detail))
                    {
                        priceMatch = MsgHelper.Deserialize<PriceMatchDetail>(msgPo.Detail);
                    }

                    if (priceMatch != null && priceMatch.iocOrder != null)
                    {
                        ThreadPool.QueueUserWorkItem(o =>
                        {
                            try
                            {
                                InnerOrderReq innerOrderReq = new InnerOrderReq();
                                innerOrderReq.StrategyOrderId = priceMatch.iocOrder.OrderId;
                                innerOrderReq.OrderQty = priceMatch.iocOrder.Surplus > priceMatch.order.Surplus ? priceMatch.order.Surplus.ToString() : priceMatch.iocOrder.Surplus.ToString();//priceMatch.iocOrder.Qty.ToString();

                                InnerHandler.Instance.FakTransactionOrder(innerOrderReq);

                            }
                            catch (Exception ex)
                            {
                                LogHelper.Error(ex);
                            }
                        });
                    }
                }
            }
        }

        private void DgMsg_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (!string.IsNullOrEmpty(CurrentType) && CurrentType == "2")
                this.btnDeal_Click(null, null);
        }

       

        private void dgMsg_MouseUp(object sender, MouseButtonEventArgs e)
        {
            MsgPo msgPo = dgMsg.Items.CurrentItem as MsgPo;
            if (msgPo != null)
            {
                msgPo.InnerColorAnimation = null;
                SetReadflag(msgPo);
            }
        }

        /// <summary>
        /// 设置消息为已读
        /// </summary>
        /// <param name="msgPo"></param>
        /// <returns></returns>
        private void SetReadflag(MsgPo msgPo)
        {
            msgPo.InnerColorAnimation = null;
            BaseHandler.Instance.PublishAppMsg(new PResult() { Code = "MsgPoReadFlag", Info1 = "MsgPo", Msg = Newtonsoft.Json.JsonConvert.SerializeObject(msgPo) });
        }

        /// <summary>
        /// 设置消息为已读
        /// </summary>
        /// <param name="msgPo"></param>
        /// <returns></returns>
        public void ProcSetReadflag(MsgPo msgPo)
        {
            if (msgPo == null)
            {
                return;
            }

            Application.Current.Dispatcher.BeginInvoke((Action)(async () =>
            {
                msgPo.ReadMsgVisible = Visibility.Collapsed;

                //更新DQB
                if (msgDic.ContainsKey(msgPo.Id))
                {
                    foreach (MsgPo ms in showData)
                    {
                        if (ms.Id == msgPo.Id)
                        {
                            ms.ReadMsgVisible = msgPo.ReadMsgVisible;
                            ms.MessageContent.detail = msgPo.Detail;
                            break;
                        }
                    }

                    msgDic.AddOrUpdate(msgPo.Id, msgPo, (key, value) => msgPo);
                }

                await System.Windows.Threading.Dispatcher.Yield(System.Windows.Threading.DispatcherPriority.Render);
            }));
        }

        /// <summary>
        /// 设置全部消息为已读
        /// </summary>
        public void SetAllReaded()
        {
            lock (LockDict)
            {
                foreach (var v in msgDic)
                {
                    v.Value.ReadMsgVisible = Visibility.Visible;
                }

                foreach (MsgPo ms in showData)
                {
                    ms.ReadMsgVisible = Visibility.Collapsed;
                }
            }
        }

        private void btnCopy_Click(object sender, RoutedEventArgs e)
        {
            MsgPo msgPo = dgMsg.Items.CurrentItem as MsgPo;

            if (msgPo != null && !string.IsNullOrEmpty(msgPo.SignalDetail))
            {
                SystemMessageDetail systemMessageDetail = MsgHelper.Deserialize<SystemMessageDetail>(msgPo.SignalDetail, Ats.Foundation.Message.Serializer.MsgSerializerType.Json);
                if (!string.IsNullOrEmpty(systemMessageDetail.EventName) && systemMessageDetail.EventName.ToUpper() == "clipboard".ToUpper())
                {
                    Trade.Plugin.Utils.ClipboardHelper.CopyToClipboard(systemMessageDetail.Data);

                    CommonUtils.Prompt2Second("复制", "复制成功");
                }
            }
        }

        private void btnOpenBook_Click(object sender, RoutedEventArgs e)
        {
            MsgPo msgPo = dgMsg.Items.CurrentItem as MsgPo;

            if (msgPo != null && !string.IsNullOrEmpty(msgPo.Content))
            {
                string securityID = msgPo.MessageContent.securityid;

                object obj = CardWinCache.Instance.GetBySecurity(securityID);
                if (obj == null)
                {
                    CardWindow cardWindow = new CardWindow();
                    cardWindow.SecurityId = securityID;
                    cardWindow.Show();

                    CardWinCache.Instance.AddOrUpdate(securityID, cardWindow);
                }
                else
                {
                    CardWindow cardWindow = obj as CardWindow;
                    cardWindow.Activate();
                }
            }
        }
    }
}
