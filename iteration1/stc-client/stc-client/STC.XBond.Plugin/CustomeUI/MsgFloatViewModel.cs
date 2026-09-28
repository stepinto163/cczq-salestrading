using Ats.Foundation.Message;
using com.gateway.log;
using Newtonsoft.Json;
using STC.Common.Data.DataModel;
using STC.Common.Handler.Admin;
using STC.Common.Handler.FinQ;
using STC.Common.Handler.GW;
using STC.Common.Handler.Inner;
using STC.Common.Model;
using STC.Common.Model.Admin;
using STC.Common.Model.Base;
using STC.Common.Model.Drp;
using STC.Common.Model.FinQ;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.GW;
using STC.Common.Model.Inner;
using STC.Common.Utils;
using STC.Common.View.FQ;
using STC.Common.ViewModel.Order;
using STC.XBond.Plugin.CustomUI;
using STC.XBond.Plugin.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace STC.XBond.Plugin.CustomeUI
{
    /// <summary>
    /// MsgFloatView的数据内容
    /// </summary>
    public class MsgFloatViewModel : BaseViewModel
    {
        private readonly string IDB = "IDB";
        private FrameworkElement _myOwn = null;
        private AsyncThreadQueue<MsgPo> _msgTask = null;
        private ObservableCollection<MsgPo> _showData = new ObservableCollection<MsgPo>();

        //private object _lock = new object();

        private Color _redBig = (Color)System.Windows.Media.ColorConverter.ConvertFromString("#E63034");
        private Color _redSmall = (Color)System.Windows.Media.ColorConverter.ConvertFromString("#E63034");

        private Color _orangeBig = (Color)System.Windows.Media.ColorConverter.ConvertFromString("#ED6D00");
        private Color _orangeSmall = (Color)System.Windows.Media.ColorConverter.ConvertFromString("#ED6D00");

        private Color _greenBig = (Color)System.Windows.Media.ColorConverter.ConvertFromString("#136C5E");
        private Color _greenSmall = (Color)System.Windows.Media.ColorConverter.ConvertFromString("#136C5E");

        private Color _redBest = (Color)System.Windows.Media.ColorConverter.ConvertFromString("#E63034");
        private Color _orangeBest = (Color)System.Windows.Media.ColorConverter.ConvertFromString("#ED6D00");
        private Thread _reafreshThread = null;

        private Color _nonfirmFAKBig = (Color)System.Windows.Media.ColorConverter.ConvertFromString("#452222");
        private Color _nonfirmFAKSmall = (Color)System.Windows.Media.ColorConverter.ConvertFromString("#222223");

        public ObservableCollection<MsgPo> ShowData { get { return _showData; } }

        /// <summary>
        /// 构造函数
        /// </summary>
        public MsgFloatViewModel()
        {
            //_msgTask = new AsyncThreadQueue<MsgPo>(OnMsg);
            _reafreshThread = new Thread(RefreshTime);
            _reafreshThread.IsBackground = true;

        }

        #region 属性

        public ICommand PopQuickDealCommand { get { return new RelayCommand(PopQuickDeal); } }

        public ICommand PopDealCommand { get { return new RelayCommand(PopDealClick); } }
        public ICommand PopKnowCommand { get { return new RelayCommand(PopKnowClick); } }
        public ICommand PopLookCommand { get { return new RelayCommand(PopLookClick); } }

        public ICommand PopCopyCommand { get { return new RelayCommand(PopCopyClick); } }

        public ICommand PopSendCommand { get { return new RelayCommand(PopSendClick); } }

        public ICommand PopOpenBookCommand { get { return new RelayCommand(PopCpenBook); } }

        #endregion

        /// <summary>
        /// '快速成交'按钮操作
        /// </summary>
        private void PopQuickDeal()
        {
            DataGrid dgMsg = _myOwn.FindName("dgMsg") as DataGrid;
            if (dgMsg != null)
            {
                MsgPo msgPo = dgMsg.SelectedItem as MsgPo; //dgMsg.Items.CurrentItem as MsgPo;
                if (msgPo != null)
                {
                    //设置消息为已读
                    SetReadflag(msgPo);

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
                                    innerOrderReq.OrderQty = priceMatch.iocOrder.Surplus > priceMatch.order.Surplus ? priceMatch.order.Surplus.ToString() : priceMatch.iocOrder.Surplus.ToString();

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
        }

        /// <summary>
        /// ‘去处理’按钮操作
        /// </summary>
        public void PopDealClick()
        {
            DataGrid dgMsg = _myOwn.FindName("dgMsg") as DataGrid;
            if (dgMsg != null)
            {
                MsgPo msgPo = dgMsg.SelectedItem as MsgPo; //dgMsg.Items.CurrentItem as MsgPo;
                
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
                                 ClipboardHelper.CopyToClipboard(systemMessageDetail.Data);
                        }
                        catch (Exception ex)
                        {
                        }
                    }

                    if (msgPo.ContentType.Equals(AdminConsts.Admin_ER))
                    {
                        ExecuteReplyView executeReplyView = new ExecuteReplyView(msgPo);
                        executeReplyView.ShowDialog();
                    }
                    else if (msgPo.ContentType.Equals(AdminConsts.Admin_IT))
                    {
                        //点价提醒
                        Common.Handler.Drp.DrpHandler.Instance.ShowTrayNotify(new Common.View.Drp.NotificationContent() { Id = msgPo.Id, ContentType = msgPo.ContentType, Detail = msgPo.Detail, IsShow = msgPo.IsShow ?? true, DelFlag = msgPo.DelFlag ?? "0", Message = msgPo.MessageContent.content, Title = msgPo.HeaderTitle });
                    }
                    else if (msgPo.ContentType.Equals(AdminConsts.Admin_EV))
                    {
                        ThreadPool.QueueUserWorkItem(o =>
                        {
                            try
                            {
                                if (string.IsNullOrEmpty(msgPo.SignalDetail))
                                {
                                    LogHelper.Error("信号消息SignalDetail为空!");
                                }
                                else
                                {
                                    if (JsonSplit.IsJson(msgPo.SignalDetail))
                                    {
                                        SignalMsgPo signalMsgPo = MsgHelper.Deserialize<SignalMsgPo>(msgPo.SignalDetail);
                                        Common.Handler.Base.BaseHandler.Instance.PublishAppMsg(new PResult() { Code = "PopupBDBQuotesMiniWindows", Info1 = signalMsgPo.data });
                                    }
                                    else
                                    {
                                        LogHelper.Error("信号消息SignalDetail格式错误!");
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                LogHelper.Error(ex);
                            }
                        });
                    }
                    else if (msgPo.ContentType.Contains("RFQ"))
                    {
                        //RFQ
                        ProcRFQOrder(msgPo.Detail.ToString());
                    }
                    else if (msgPo.ContentType.Equals("DQ"))
                    {
                        //定向回价 价格匹配，点击 待办事项，处理按钮
                        ProcDrpPMOrder(msgPo);
                    }
                    else if (msgPo.ContentType.Equals("DQB"))
                    {
                        //定向回价 拒绝 回价功能
                        ProcDrpCancelOrder(msgPo);
                    }
                    else if (msgPo.ContentType.Equals("TableSvr_FOCUS_TAG"))
                    {
                        lock (_showData)
                        {
                            _showData.Remove(msgPo);
                        }
                        //内部看板订阅
                        ProcTableSvrFocusTag(msgPo);
                    }
                    else if (msgPo.ContentType.Equals("Quant_Focus_Tag"))
                    {
                        lock (_showData)
                        {
                            _showData.Remove(msgPo);
                        }
                        //策略控制台消息
                        ProcQuantFocusTag(msgPo);
                    }
                    else if (msgPo.ContentType.Equals("TableSvr_FOCUS_TAG_RW"))
                    {
                        lock (_showData)
                        {
                            _showData.Remove(msgPo);
                        }
                        //内部看板订阅
                        ProcTableSvrFocusTag_RW(msgPo);
                    }
                    else if ("TableSvr_Inquiry_Fail".Equals(msgPo.ContentType))
                    {
                        // 询价失败
                        lock (_showData)
                        {
                            _showData.Remove(msgPo);
                        }
                        ProcInquiryFail(msgPo);
                    }
                    else if (msgPo.ContentType.Equals(AdminConsts.PriceMatch))
                    {
                        PriceMatchView priceMatchView = new PriceMatchView(msgPo);
                        priceMatchView.ShowDialog();
                    }
                    else if (msgPo.ContentType.Equals("TableSvr_ORDER_UNREF_Msg"))
                    {
                        //TableSvr Unref 消息提醒
                        ProcTableSvrOrderUnRefMsg(msgPo);
                    }
                    else if (msgPo.ContentType.Equals(AdminConsts.Admin_ST))
                    {
                        if (msgPo.HeaderTitle == "[客户发来新报价]" && msgPo.MessageContent != null && !string.IsNullOrEmpty(msgPo.MessageContent.id))
                        {
                            //弹报价编辑页面
                            ProcChatMsg(msgPo);
                        }
                    }
                    else if (msgPo.ContentType.Equals("CUST_MD"))
                    {
                        //新增GROUP 发送消息给网页
                        Common.Handler.Base.BaseHandler.Instance.PublishAppMsg(new PResult() { Code = "pubJS", Info1 = "市场最优", Info2 = "TagGroupSelection", Info3 = msgPo.groupname });

                        if ((_myOwn as MsgFloatView).Visibility == Visibility.Visible)
                        {
                            //是否是浮动窗口
                            KnowMsg(msgPo);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// '知道了'按钮操作
        /// </summary>
        private void PopKnowClick()
        {
            DataGrid dgMsg = _myOwn.FindName("dgMsg") as DataGrid;
            if (dgMsg != null)
            {
                MsgPo msgPo = dgMsg.SelectedItem as MsgPo;
                if (msgPo != null)
                    this.KnowMsg(msgPo);
            }
        }

        private void PopCopyClick()
        {
            DataGrid dgMsg = _myOwn.FindName("dgMsg") as DataGrid;

            if (dgMsg != null)
            {
                MsgPo msgPo = dgMsg.SelectedItem as MsgPo;

                if (msgPo != null)
                {
                    if (msgPo.ContentType == "BDB")
                    {
                        if (!string.IsNullOrEmpty(msgPo.SignalDetail))
                        {
                            SystemMessageDetail systemMessageDetail = MsgHelper.Deserialize<SystemMessageDetail>(msgPo.SignalDetail, Ats.Foundation.Message.Serializer.MsgSerializerType.Json);
                            if (!string.IsNullOrEmpty(systemMessageDetail.EventName) && systemMessageDetail.EventName.ToUpper() == "clipboard".ToUpper())
                            {
                                 ClipboardHelper.CopyToClipboard(systemMessageDetail.Data);

                                CommonUtils.Prompt2Second("复制", "复制成功");
                            }
                        }
                    }
                    else
                    {
                        if (msgPo.ContentType == "TableSvr_FOCUS_TAG_RW")
                        {
                            this.ProcTableSvrFocusTag_RW_Copy(msgPo);
                        }
                        else
                        {
                            this.ProcTableSvrFocusTag_Copy(msgPo);
                        }
                    }
                }
            }
        }


        private void PopSendClick()
        {
            DataGrid dgMsg = _myOwn.FindName("dgMsg") as DataGrid;
            if (dgMsg != null)
            {
                MsgPo msgPo = dgMsg.SelectedItem as MsgPo;
                if (msgPo != null)
                {
                    if (msgPo.ContentType == "TableSvr_FOCUS_TAG_RW")
                    {
                        this.ProcTableSvrFocusTag_RW_Copy(msgPo, false);
                    }
                    else
                    {
                        this.ProcTableSvrFocusTag_Copy(msgPo, false);
                    }
                }
            }

            FQSendWindow fqSendWindow = new FQSendWindow();
            fqSendWindow.Owner = Application.Current.MainWindow;
            fqSendWindow.Show();
        }

        /// <summary>
        /// 处理知道了的消息
        /// </summary>
        /// <param name="msgPo"></param>
        public void KnowMsg(MsgPo msgPo)
        {
            if (msgPo != null)
            {
                //设置消息为已读
                SetReadflag(msgPo);

                if (msgPo.ContentType.Equals(AdminConsts.Admin_IT)
                    || "BDB".Equals(msgPo.ContentType))
                {
                    lock (_showData)
                    {
                        _showData.Remove(msgPo);
                    }
                }
                else
                {
                    ThreadPool.QueueUserWorkItem(o =>
                    {
                        try
                        {
                            TBatchMsgReq batchMsgReq = new TBatchMsgReq();
                            batchMsgReq.delflag = "1";
                            batchMsgReq.readflag = "1";
                            batchMsgReq.id = new List<string>();

                            batchMsgReq.id.Add(msgPo.Id);

                            PResult result = AdminHandler.Instance.BatchUpdateSysMsg(batchMsgReq);

                            if (result != null && !result.IsSuccess())
                            { 
                                LogHelper.Error("处理消息失败:" + result.Code ?? "" + result.Msg ?? "");
                            }

                            //无论是否删除成功，都要移除
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                lock(_showData)
                                {
                                    MsgPo removeMsgPo = _showData.FirstOrDefault(Item => Item.Id == msgPo.Id);
                                    if (removeMsgPo != null)
                                    {
                                        _showData.Remove(removeMsgPo);
                                    }
                                }
                            });
                            
                        }
                        catch (Exception ex)
                        {
                            LogHelper.Error(ex);
                        }

                    });
                }
            }
        }

        /// <summary>
        /// '查看'按钮操作
        /// </summary>
        private void PopLookClick()
        {
            DataGrid dgMsg = _myOwn.FindName("dgMsg") as DataGrid;
            if (dgMsg != null)
            {
                MsgPo msgPo = dgMsg.SelectedItem as MsgPo;
                if (msgPo != null)
                {
                    //设置消息为已读
                    SetReadflag(msgPo);

                    ExecuteReplyView executeReplyView = new ExecuteReplyView(msgPo);
                    executeReplyView.Show();
                }
            }
        }

        /// <summary>
        /// 打开BDB行情小窗
        /// </summary>
        private void PopCpenBook()
        {
            Task.Run(() =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    DataGrid dgMsg = _myOwn.FindName("dgMsg") as DataGrid;
                    if (dgMsg != null)
                    {
                        MsgPo msgPo = dgMsg.SelectedItem as MsgPo;
                        if (msgPo != null && !string.IsNullOrEmpty(msgPo.Content))
                        {
                            //设置消息为已读
                            SetReadflag(msgPo);
                            string securityID = msgPo.MessageContent.securityid;
                            Common.Handler.Base.BaseHandler.Instance.PublishAppMsg(new PResult() { Code = "PopupBDBQuotesMiniWindows", Info1 = securityID });
                        }
                    }
                });
            });
        }

        /// <summary>
        /// 清空数据
        /// </summary>
        public void ClearMsg()
        {
            lock(_showData)
            {
                _showData.Clear();
            }
        }

        /// <summary>
        /// 得到消息的显示状态
        /// </summary>
        /// <returns></returns>
        public bool GetStatus()
        {
            lock (_showData)
            {
                return !_showData.Any(msgPo => (msgPo.Storyboard != null && msgPo.Storyboard.GetCurrentState() != ClockState.Stopped) || msgPo.TimeOut);
            }
        }

        /// <summary>
        /// 漂浮框显示定时器
        /// </summary>
        private void RefreshTime()
        {

            while (true)
            {
                try
                {
                    //lock (_lock)
                    {
                        List<MsgPo> selectMsg = null;
                        lock (_showData)
                        {
                            selectMsg = _showData.Where(delegate (MsgPo msgPo)
                            {
                                return msgPo != null && !string.IsNullOrEmpty(msgPo.ContentType) && msgPo.ContentType.Equals(AdminConsts.Admin_IT) && !msgPo.TimeOut;
                            }).ToList();
                        }

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
                                    TimeSpan diffTime = DateTimeEx.GetNow() - transactTime;

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

                        lock(_showData)
                        {
                            selectMsg = _showData.Where(delegate (MsgPo msgPo)
                            {
                                bool isTrue = false;
                                DateTime dt;
                                TimeSpan diffspan = DateTimeEx.ServerTimeNow - DateTime.Now;
                                if (msgPo != null)
                                {
                                    if (!string.IsNullOrEmpty(msgPo.ContentType) && !msgPo.ContentType.Equals(AdminConsts.Admin_IT) && DateTime.TryParse(msgPo.UpdateTime, out dt))
                                    {

                                        TimeSpan diffTime = DateTimeEx.GetNow().AddSeconds(diffspan.TotalSeconds) - dt;
                                        isTrue = diffTime.TotalSeconds > 60;
                                    }
                                }
                                else
                                    return false;
                                return msgPo.TimeOut || isTrue;
                            }).ToList();
                        }

                        if (selectMsg != null && selectMsg.Count > 0 && Application.Current != null)
                        {
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                lock (_showData)
                                {
                                    foreach (MsgPo msgPo in selectMsg)
                                    {
                                        _showData.Remove(msgPo);
                                    }
                                }
                            });
                        }

                        ClosePopup();

                        Thread.Sleep(1000);
                    }
                }
                catch (ThreadAbortException ex)
                {
                    LogHandler.Error(ex);
                    Thread.Sleep(1000);
                }
                catch (Exception otherEx)
                {
                    LogHandler.Error(otherEx);
                    Thread.Sleep(1000);
                }
            }
        }

        /// <summary>
        /// 关闭漂浮框
        /// </summary>
        private void ClosePopup()
        {
            if (_showData.Count == 0 && Application.Current != null)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Popup popup = _myOwn.Parent as Popup;
                    if (popup != null && popup.IsOpen)
                    {
                        popup.IsOpen = false;
                        popup.StaysOpen = false;
                    }
                });
            }
        }

        /// <summary>
        /// 处理漂浮框颜色动画效果
        /// </summary>
        public void OnStoryboard()
        {
            List<MsgPo> selectMsg = null;

            lock (_showData)
            {
                selectMsg = _showData.Where(delegate (MsgPo msgPo)
                {
                    return msgPo.TimeOut;
                }).ToList();
            }

            if (selectMsg != null && selectMsg.Count > 0)
            {
                foreach (MsgPo msgPo in selectMsg)
                {
                    _showData.Remove(msgPo);
                }
            }

            ClosePopup();
        }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="own"></param>
        public void Init(FrameworkElement own)
        {
            _myOwn = own;

            _reafreshThread.Start();
        }

        /// <summary>
        /// 处理待办事项和消息提醒数据
        /// </summary>
        /// <param name="data">代办事项和消息提醒数据</param>
        public void OnMsg(MsgPo data)
        {
            if (data != null)
            {
                if (data.MsgType != null && data.MsgType.Equals("1") && data.username.Equals("*") && !string.IsNullOrEmpty(data.ReadFlag))
                {
                    string[] arr = data.ReadFlag.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                    if (arr != null && !arr.Contains(LocalEnv.UserInfo.UserName))//全局消息的删除操作，看readflag是否包含当前账号
                        return;
                }

                AddOrUpdateMsg(data);
            }
        }

        /// <summary>
        ///  处理未读代办事项和消息提醒数据
        /// </summary>
        /// <param name="data">代办事项和消息提醒数据</param>
        private void AddOrUpdateMsg(MsgPo data)
        {
            MsgPo removeMsgPo = _showData.FirstOrDefault(Item => Item.Id == data.Id);

            if (removeMsgPo == null)
            {
                if (!string.IsNullOrEmpty(data.DelFlag) && !data.DelFlag.Equals("1"))
                {
                    CustomeProperty(data);

                    lock(_showData)
                    {
                        if (_showData.Count > 0)
                        {
                            if (_showData.Count >= 10)
                                _showData.RemoveAt(9);

                            _showData.Insert(0, data);
                        }
                        else
                            _showData.Add(data);
                    }
                }
            }
            else
            {
                DealIndividualMsg(data, removeMsgPo);
            }
        }

        /// <summary>
        /// 根据代办事项和消息提醒数据更新界面展示
        /// </summary>
        /// <param name="data"></param>
        private void CustomeProperty(MsgPo data)
        {
            PriceMatchDetail priceMatchDetail = null;
            if (data.Detail != null)
            {
                priceMatchDetail = MsgHelper.Deserialize<PriceMatchDetail>(data.Detail);

                if (!string.IsNullOrEmpty(data.Venue) && data.Venue.Equals(IDB))
                {
                    data.PopDealVisibility = Visibility.Collapsed;
                    data.LookVisibility = Visibility.Visible;
                }
                else
                {
                    data.LookVisibility = Visibility.Collapsed;
                    data.PopDealVisibility = Visibility.Visible;
                }
            }

            if (!string.IsNullOrEmpty(data.ReadFlag))
            {
                string[] arr = data.ReadFlag.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                if (arr != null && arr.Contains(LocalEnv.UserInfo.UserName))
                {
                    data.ReadMsgVisible = Visibility.Collapsed;
                }
                else
                {
                    data.ReadMsgVisible = Visibility.Visible;
                }
            }
            else
            {
                data.ReadMsgVisible = Visibility.Visible;
            }



            if (data.MsgType.Equals("1"))
            {
                data.PopDealVisibility = Visibility.Collapsed;
                data.LookVisibility = Visibility.Collapsed;

                if (data.Level.Equals("1"))
                {
                    data.Status = "";
                    data.StatusWidth = 50;
                    data.StatusBrush = (Brush)Application.Current.MainWindow.FindResource("ColorBrush.Red7");// new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#E63034"));
                    data.StatusVisibility = Visibility.Visible;
                }
                else if (data.Level.Equals("2"))
                {
                    data.Status = "";
                    data.StatusWidth = 80;
                    data.StatusBrush = (Brush)Application.Current.MainWindow.FindResource("ColorBrush.Orange5");//new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#ED6D00"));
                    data.StatusVisibility = Visibility.Visible;
                }
                if (data.HeaderTitle.Contains("连接中断"))
                    data.Status = "";
                else if (data.HeaderTitle.Contains("下单失败"))
                    data.Status = "请知悉";
                else if (data.HeaderTitle.Contains("撤单失败"))
                    data.Status = "";
                else if (data.HeaderTitle.Contains("订单超时"))
                    data.Status = "";
            }
            else
            {
                if (data.Content.EndsWith("中断") || data.Content.EndsWith("恢复"))
                {
                    data.PopDealVisibility = Visibility.Collapsed;
                }
                else
                {
                    data.PopDealVisibility = Visibility.Visible;
                }

                if (!string.IsNullOrEmpty(data.HeaderTitle) && data.HeaderTitle.Contains("成交"))
                {
                    data.StatusVisibility = Visibility.Collapsed;
                    data.Status = string.Empty;
                }
                else
                {
                    data.Status = "";
                    data.StatusBrush = (Brush)Application.Current.MainWindow.FindResource("ColorBrush.Red7");//new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#E63034"));
                    data.StatusVisibility = Visibility.Visible;
                    data.StatusWidth = 80;
                }


            }

            if (data.ContentType != null)
            {
                if (data.ContentType.Equals(AdminConsts.Admin_EV))//信号消息
                {
                    data.LookVisibility = Visibility.Collapsed;
                }


                if (!data.ContentType.Equals(AdminConsts.Admin_IT))
                {
                    if (data.ContentType == AdminConsts.Admin_ST)
                        data.ShowHeight = 80;
                    else
                        data.ShowHeight = 104;

                    data.UpdateTimeVisibility = Visibility.Visible;
                    data.TimeOutTxtVisibility = Visibility.Collapsed;
                    data.ValueTimeVisibility = Visibility.Collapsed;

                    //ColorAnimation ca = new ColorAnimation();
                    //ca.Duration = new Duration(TimeSpan.FromSeconds(60));

                    if (data.Level.Equals("1")) //紧急
                    {
                        Console.WriteLine("red");
                        //ca.From = _redBig;
                        //ca.To = _redSmall;
                        data.ShowBG = (Brush)Application.Current.MainWindow.FindResource("ColorBrush.Red-17");
                        data.ShowBorderBrush = (Brush)Application.Current.MainWindow.FindResource("ColorBrush.Red13");
                    }
                    else if (data.Level.Equals("2")) //警告
                    {
                        //ca.From = _redBig;
                        //ca.To = _redSmall;
                        data.ShowBG = (Brush)Application.Current.MainWindow.FindResource("ColorBrush.Red-17");
                        data.ShowBorderBrush = (Brush)Application.Current.MainWindow.FindResource("ColorBrush.Red13");
                    }
                    else if (data.Level.Equals("3")) //普通
                    {
                       // Console.WriteLine("green");
                        //ca.From = _greenBig;
                        //ca.To = _greenSmall;
                        data.ShowBG = (Brush)Application.Current.MainWindow.FindResource("ColorBrush.BlackishGreen");
                        data.ShowBorderBrush = (Brush)Application.Current.MainWindow.FindResource("ColorBrush.Green");
                    }

                    if (data.ContentType.Equals(AdminConsts.Admin_ER))
                    {
                        ExecutionReport deal = MsgHelper.Deserialize<ExecutionReport>(data.Detail);
                        if (deal != null)
                        {
                            if (deal.Side.ToUpper().Equals("SELL") || deal.Side.Equals("2"))
                            //if (data.Level == AdminConsts.Admin_Normal)
                            {
                                data.ShowBG = (Brush)Application.Current.MainWindow.FindResource("ColorBrush.Blue-12");
                                data.ShowBorderBrush = (Brush)Application.Current.MainWindow.FindResource("ColorBrush.Blue8");
                            }
                            else
                            {
                                data.ShowBG = (Brush)Application.Current.MainWindow.FindResource("ColorBrush.Orange-9");
                                data.ShowBorderBrush = (Brush)Application.Current.MainWindow.FindResource("ColorBrush.Orange4");
                            }
                        }
                    }
                    else if (data.ContentType.Equals(AdminConsts.PriceMatch))
                    {
                        if (priceMatchDetail != null && priceMatchDetail.order != null)
                        {
                            //if (priceMatchDetail.order.Side.ToUpper().Equals("SELL") || priceMatchDetail.order.Side.Equals("2"))
                            if (data.Level == AdminConsts.Admin_Normal || data.Level == "3")
                            {
                                data.ShowBG = (Brush)Application.Current.MainWindow.FindResource("ColorBrush.BlackishGreen");
                                data.ShowBorderBrush = (Brush)Application.Current.MainWindow.FindResource("ColorBrush.Green");

                            }
                            else
                            {
                                data.ShowBG = (Brush)Application.Current.MainWindow.FindResource("ColorBrush.Red-17");
                                data.ShowBorderBrush = (Brush)Application.Current.MainWindow.FindResource("ColorBrush.Red13");
                            }
                        }
                    }
                    else if (data.ContentType.Equals("DQB") || data.ContentType.Equals("DQ"))
                    {
                        //定向回价拒绝
                        //定向回价
                        data.BtnShowTxt = "清除";
                        data.KnowVisibility = Visibility.Visible;
                        data.LookVisibility = Visibility.Collapsed;
                        data.PopQuickDealVisibility = Visibility.Collapsed;
                        data.PopDealVisibility = Visibility.Collapsed;
                        data.DealVisibility = Visibility.Visible;
                        data.BtnDealShowTxt = "回价";
                        data.ShowBG = (Brush)Application.Current.MainWindow.FindResource("ColorBrush.Black8");
                        data.ShowBorderBrush = (Brush)Application.Current.MainWindow.FindResource("ColorBrush.Gray21");


                        //新增参数
                        data.TbContentVisibility = Visibility.Collapsed;
                        data.DQVisibility = Visibility.Visible;
                        data.UpdateTimeVisibility = Visibility.Visible;
                        DRPDetail dr = Newtonsoft.Json.JsonConvert.DeserializeObject<DRPDetail>(data.Detail);
                        //原始报价信息
                        //data.DQSecurityID = dr.order.symbol;
                        if (dr.order.symbolType.ToUpper() == "BCO")
                        {
                            data.DQSecurityID = dr.order.securityid;
                        }
                        else
                        {
                            data.DQSecurityID = dr.order.symbol;
                        }

                        data.DQSide = (dr.order.side.ToUpper().Equals("BUY") || dr.order.side.Equals("1")) ? "1" : "2";
                        data.DQShowSide = data.DQSide == "1" ? "Bid" : "Ofr";

                        string tmpprice = dr.order.price.ToString() == "--" ? "--" : CommonUtils.FormatNum(dr.order.price.ToString());
                        if ((CommonUtils.ConvertToDecimal(tmpprice).ToString("F4") == "-1.0000") || (CommonUtils.ConvertToDecimal(tmpprice).ToString("F4") == "999.0000"))
                        {
                            data.DQShowPrice = "--";
                        }
                        else
                        {
                            data.DQShowPrice = tmpprice.ToString() == "--" ? "--" : CommonUtils.FormatNum(tmpprice.ToString());
                        }
                        data.DQAmount = dr.order.qty.ToString() == "0" ? "--" : (dr.order.qty - dr.order.cumQty).ToString();
                        
                        if (dr.order.marketIndicator.ToString().Trim() == "2")
                            data.DQFirmType = "";
                        else
                            data.DQFirmType = dr.order.firmFlag == "1" ? $"Firm" : $"NF";

                        data.DQName = dr.order.name;
                        data.DQForegroundBrush = data.DQSide == "1" ? (Brush)Application.Current.MainWindow.FindResource("ColorBrush.Orange7") : (Brush)Application.Current.MainWindow.FindResource("ColorBrush.Blue8");

                        //建议： dqOrderInfo2
                        if (dr.iocOrder.dqOrderInfo2 != null)
                        {
                            if (dr.iocOrder.dqOrderInfo2.bargainYield != null)
                                data.BargainYield = dr.iocOrder.dqOrderInfo2.bargainYield.ToString() == "--" ? "--" : CommonUtils.FormatNum(dr.iocOrder.dqOrderInfo2.bargainYield.ToString());
                            else
                                data.BargainYield = "--";
                            if (dr.iocOrder.dqOrderInfo2.bargainQty != null)
                                data.BargainQty = dr.iocOrder.dqOrderInfo2.bargainQty.ToString() == "0" ? "--" : dr.iocOrder.dqOrderInfo2.bargainQty.ToString();
                            else
                                data.BargainQty = "--";
                        }

                        //我的
                        data.IocDQSide = (dr.iocOrder.side.ToUpper().Equals("BUY") || dr.iocOrder.side.Equals("1")) ? "1" : "2";
                        data.IocDQShowSide = data.IocDQSide == "1" ? "Bid" : "Ofr";
                        data.IocDQShowPrice = dr.iocOrder.price.ToString() == "--" ? "--" : CommonUtils.FormatNum(dr.iocOrder.price.ToString());
                        data.IocDQAmount = dr.iocOrder.qty.ToString() == "0" ? "--" : dr.iocOrder.qty.ToString();
                        data.IocDQForegroundBrush = data.IocDQSide == "1" ? (Brush)Application.Current.MainWindow.FindResource("ColorBrush.Orange7") : (Brush)Application.Current.MainWindow.FindResource("ColorBrush.Blue8");
                        data.IocDQName = dr.iocOrder.name;
                        //UI显示控制
                        data.HeaderTitle = data.ContentType.Equals("DQ") ? "定向回价[收到回价]" : "定向回价[价格讨论]"; ;
                        data.DQBVisibility = Visibility.Visible;
                        //1 - 议价，2 - 成交，3 - 拒绝  4-撤销 5 - 报价方撤单
                        if (dr.iocOrder.dqOrderInfo2 != null)
                        {
                            if (dr.iocOrder.dqOrderInfo2.actionType == "1")
                            {
                                data.PopDealVisibility = Visibility.Visible;
                            }
                            if (dr.iocOrder.dqOrderInfo2.actionType == "3")
                            {
                                data.HeaderTitle = "定向回价[已忽略]";
                                data.DQBVisibility = Visibility.Collapsed;
                            }
                            if (dr.iocOrder.dqOrderInfo2.actionType == "4")
                            {
                                data.HeaderTitle = "定向回价[已撤销]";
                                data.DQBVisibility = Visibility.Collapsed;
                                data.DQVisibility = Visibility.Collapsed;
                                data.TbContentVisibility = Visibility.Visible;
                                data.BtnDealShowTxt = "查看";
                            }
                            if (dr.iocOrder.dqOrderInfo2.actionType == "5")
                            {
                                data.HeaderTitle = "定向回价[报价已撤]";
                                data.DQBVisibility = Visibility.Collapsed;
                                data.DQVisibility = Visibility.Collapsed;
                                data.TbContentVisibility = Visibility.Visible;
                                data.BtnDealShowTxt = "查看";
                            }
                        }
                        if (dr.iocOrder.rejCode == "10010")
                        {
                            data.HeaderTitle = "定向回价[报价已撤]";
                            data.DQBVisibility = Visibility.Collapsed;
                            data.DQVisibility = Visibility.Collapsed;
                            data.TbContentVisibility = Visibility.Visible;
                            data.BtnDealShowTxt = "查看";
                        }
                        if (dr.iocOrder.rejCode == "11015")
                        {
                            data.HeaderTitle = "定向回价[已超时]";
                            data.DQBVisibility = Visibility.Collapsed;
                            data.BtnDealShowTxt = "回价";
                        }
                        if (dr.iocOrder.rejCode == "11013")
                        {
                            data.HeaderTitle = "定向回价[已撤销]";
                            data.DQBVisibility = Visibility.Collapsed;
                            data.DQVisibility = Visibility.Collapsed;
                            data.TbContentVisibility = Visibility.Visible;
                            data.BtnDealShowTxt = "查看";
                        }
                        //修改Grid的Content;
                        //if (data.IocDQName == LocalEnv.UserInfo.UserName)
                        //{
                        //    data.IocDQName = "我的";
                        //}

                        data.Content = $"{data.DQName}：{data.DQSecurityID} {data.DQShowSide} {data.DQShowPrice} {data.DQAmount}万 {data.DQFirmType}";
                        data.Content += $"\r\n{data.IocDQName}：{data.IocDQShowSide} {data.IocDQShowPrice} {data.IocDQAmount}万";
                        if (data.DQBVisibility == Visibility.Visible && data.ContentType.Equals("DQB"))
                        {
                            data.Content += $"\r\n{data.DQName}：建议 {data.DQShowSide} {data.BargainYield} {data.BargainQty}万";
                        }
                    }
                    else if (data.ContentType.Equals("OD"))
                    {
                        //data.ShowBG = (Brush)Application.Current.MainWindow.FindResource("ColorBrush.Orange-9");
                        //data.ShowBorderBrush = (Brush)Application.Current.MainWindow.FindResource("ColorBrush.Orange4");

                        data.ShowBG = (Brush)Application.Current.MainWindow.FindResource("ColorBrush.Red-17");
                        data.ShowBorderBrush = (Brush)Application.Current.MainWindow.FindResource("ColorBrush.Red13");
                    }
                    else if (data.ContentType.ToUpper().Equals(AdminConsts.Admin_MM))
                    {
                        data.ShowBG = (Brush)Application.Current.MainWindow.FindResource("ColorBrush.Red-17");
                        data.ShowBorderBrush = (Brush)Application.Current.MainWindow.FindResource("ColorBrush.Red13");
                        data.BtnShowTxt = "清除";
                        data.HeaderTitle = data.HeaderTitle;
                        data.KnowVisibility = Visibility.Visible;
                        data.LookVisibility = Visibility.Collapsed;
                        data.PopQuickDealVisibility = Visibility.Collapsed;
                        data.PopDealVisibility = Visibility.Collapsed;
                        data.DealVisibility = Visibility.Visible;
                        data.BtnDealShowTxt = "处理";
                        MMBreakAlarmModel model = Newtonsoft.Json.JsonConvert.DeserializeObject<MMBreakAlarmModel>(data.Detail);
                        if (model != null)
                        {
                            if (model.passiveCount > 0)
                            {
                                data.Content += $"\r\n被动关闭债券：{model.passiveCount} 只";
                            }
                            if (model.dangerCount > 0)
                            {
                                data.Content += $"\r\n断报券危险：{model.dangerCount} 只";
                            }
                            if ("2".Equals(model.status))
                            {
                                data.Content += $"\r\n做市状态：需关注";
                            }
                        }
                    }
                    else if (data.ContentType.Equals("TableSvr_ORDER_UNREF_Msg"))
                    {
                        //TableSvr Unref 消息提醒  //1 C 紧急  //2 A 警告  //3 N普通
                        if (data.Level == AdminConsts.Admin_Normal || data.Level == "3")
                        {
                            data.ShowBG = (Brush)Application.Current.MainWindow.FindResource("ColorBrush.BlackishGreen");
                            data.ShowBorderBrush = (Brush)Application.Current.MainWindow.FindResource("ColorBrush.Green");
                        }
                        else
                        {
                            data.ShowBG = (Brush)Application.Current.MainWindow.FindResource("ColorBrush.Orange-9");
                            data.ShowBorderBrush = (Brush)Application.Current.MainWindow.FindResource("ColorBrush.Orange4");
                            data.PopDealVisibility = Visibility.Visible;
                        }
                    }
                    else if (data.ContentType.Equals("Quant_Focus_Tag") || data.ContentType.Equals("TableSvr_FOCUS_TAG")
                        || data.ContentType.EndsWith("TableSvr_FOCUS_TAG_RW"))
                    {
                        if (data.ContentType == "TableSvr_FOCUS_TAG" || data.ContentType == "TableSvr_FOCUS_TAG_RW")
                        {
                            data.CopyVisibility = Visibility.Visible;
                            data.SendVisibility = LocalEnv.ConfInfo.IsFQ && AdminHandler.Instance.CheckPermisionById(CommonConstants.MODELID_5000, CommonConstants.CODE_5002) ? Visibility.Visible : Visibility.Collapsed;
                        }
                    }
                    else if (data.ContentType.Equals("BDB"))
                    {
                        data.BDBVisiblitity = Visibility.Visible;
                        data.KnowVisibility = Visibility.Collapsed;
                        data.PopDealVisibility = Visibility.Collapsed;
                        data.ShowBG = (Brush)Application.Current.MainWindow.FindResource("Green-7");
                        data.ShowBorderBrush = (Brush)Application.Current.MainWindow.FindResource("Green-3");
                        data.HeaderTitleFColor = (Brush)Application.Current.MainWindow.FindResource("Green-2");
                        data.ContentFColor = (Brush)Application.Current.MainWindow.FindResource("White-1");
                    }
                    else
                    {
                        //data.InnerColorAnimation = ca;
                        data.LookVisibility = Visibility.Collapsed;
                    }


                    //data.ShowBG = (Brush)Application.Current.MainWindow.FindResource("ColorBrush.Blue-12");
                    //data.ShowBorderBrush = (Brush)Application.Current.MainWindow.FindResource("ColorBrush.Blue8");
                }
                else
                {
                    data.ShowHeight = 80;

                    data.Status = "";
                    data.ValueTimeVisibility = Visibility.Visible;
                    data.ValueTime = string.Empty;//"倒计时";
                    data.StatusBrush = (Brush)Application.Current.MainWindow.FindResource("ColorBrush.Orange8");
                    data.UpdateTimeBrush = (Brush)Application.Current.MainWindow.FindResource("ColorBrush.Red7");
                    data.UpdateTimeVisibility = Visibility.Collapsed;
                    data.TimeOutTxtVisibility = Visibility.Visible;

                    data.PopQuickDealVisibility = Visibility.Visible;
                    data.KnowVisibility = Visibility.Collapsed;

                    data.ShowBG = (Brush)Application.Current.MainWindow.FindResource("ColorBrush.Red8");

                    if (priceMatchDetail != null && priceMatchDetail.iocOrder != null)
                    {
                        DateTime transactTime = CommonUtils.ConvertToDateTime(priceMatchDetail.iocOrder.TransactTime);
                        TimeSpan diffTime = DateTimeEx.GetNow() - transactTime;

                        double d = 0;
                        if (double.TryParse(priceMatchDetail.iocOrder.IocTimeOut, out d))
                        {
                            double result = d - diffTime.TotalSeconds;
                            data.TimeOutTxt = string.Format("{0}", result >= 0 ? result.ToString("N0") : "0");//string.Format("{0}秒", result >= 0 ? result.ToString("N0") : "0");
                        }


                        ColorAnimation ca = new ColorAnimation();
                        ca.Duration = new Duration(TimeSpan.FromSeconds(0.7));

                        ca.From = _nonfirmFAKBig;
                        ca.To = _nonfirmFAKSmall;
                        //data.InnerColorAnimation = ca;


                    }


                }
            }



            if (!string.IsNullOrEmpty(data.ReadFlag) && data.ReadFlag.Contains(LocalEnv.UserInfo.UserId))
            {
                data.PopDealVisibility = Visibility.Collapsed;
            }
            else
            {
                if (!data.MsgType.Equals("1"))
                    data.PopDealVisibility = Visibility.Visible;
            }

            if (string.IsNullOrEmpty(data.CloseBy))
            {
                data.GridHeight = new GridLength(0);
                data.CloseByVisibility = Visibility.Collapsed;
            }

            //刷新纪录数
            SetMsgUIRefreshCount();
        }

        /// <summary>
        /// 处理无效数据
        /// </summary>
        /// <param name="data"></param>
        /// <param name="selectMsg"></param>
        private void DealIndividualMsg(MsgPo data, MsgPo selectMsg)
        {
            if (data.DelFlag.Equals("1") || (!string.IsNullOrEmpty(data.ReadFlag) && data.ReadFlag.Contains(LocalEnv.UserInfo.UserName)))
            {
                lock (_showData)
                {
                    _showData.Remove(selectMsg);
                }
            }
            else
            {
                Brush bg = selectMsg.ShowBG;
                Brush border = selectMsg.ShowBorderBrush;
                double showHeight = selectMsg.ShowHeight;
                ObjectConvert.UpdateMsg(selectMsg, data);
                selectMsg.ShowBG = bg;
                selectMsg.ShowBorderBrush = border;
                selectMsg.ShowHeight = showHeight;

                CustomeProperty(selectMsg);
            }
        }

        /// <summary>
        /// 根据id搜索数据
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public MsgPo GetMsgById(string id)
        {
            return _showData.FirstOrDefault(Item => Item.Id == id);
        }

        public void ProcRFQOrder(string detail)
        {
            Common.Handler.Base.BaseHandler.Instance.PublishAppMsg(new PResult() { Code = "RFQMsgPo", Msg = detail });
        }

        /// <summary>
        ///  定向回价 价格匹配，点击 待办事项，处理按钮
        /// </summary>
        /// <param name="msgPo"></param>
        public void ProcDrpPMOrder(MsgPo msgPo)
        {
            //DQ
            string key = Newtonsoft.Json.JsonConvert.DeserializeObject<DRPDetail>(msgPo.Detail).order.orderId;
            TMessageContent detail = Common.Handler.Drp.DrpHandler.Instance.GetContent(key);
            if (detail != null)
            {
                Common.Handler.Base.BaseHandler.Instance.PublishAppMsg(new PResult() { Code = "DRPMsgPo", Info1 = "MessageContent", Msg = JsonConvert.SerializeObject(detail) });
            }
        }

        /// <summary>
        /// 定向回价 拒绝 回价功能
        /// </summary>
        /// <param name="msgPo"></param>
        public void ProcDrpCancelOrder(MsgPo msgPo)
        {
            if (!string.IsNullOrEmpty(msgPo.Detail))
            {
                DRPDetail dr = Newtonsoft.Json.JsonConvert.DeserializeObject<DRPDetail>(msgPo.Detail);
                string price = dr.iocOrder.price.ToString("N4");
                string qty = dr.iocOrder.qty.ToString();

                //建议价
                string bargainyield = string.Empty;
                string bargainqty = string.Empty;
                if (dr.iocOrder.dqOrderInfo2 != null)
                {
                    if (dr.iocOrder.dqOrderInfo2.bargainYield != null)
                        price = dr.iocOrder.dqOrderInfo2.bargainYield.ToString() == "--" ? "--" : CommonUtils.FormatNum(dr.iocOrder.dqOrderInfo2.bargainYield.ToString());

                    if (dr.iocOrder.dqOrderInfo2.bargainQty != null)
                        qty = dr.iocOrder.dqOrderInfo2.bargainQty.ToString() == "0" ? "--" : dr.iocOrder.dqOrderInfo2.bargainQty.ToString();

                    bargainyield = price;
                    bargainqty = qty;

                    if (dr.iocOrder.dqOrderInfo2.actionType != "1")
                    {
                        bargainyield = "";
                        bargainqty = "";
                    }
                    
                    //1 - 议价，2 - 成交，3 - 拒绝  4-撤销  5-报价方撤销 6超时
                    if (dr.iocOrder.dqOrderInfo2.actionType == "4")
                    {
                        Common.Handler.Base.BaseHandler.Instance.PublishAppMsg(new PResult() { Code = "DRPMsgPo", Info1 = "MsgPo", Msg = JsonConvert.SerializeObject(msgPo) });
                        return;
                    }
                }

                //if (dr.iocOrder.rejCode == "10005")
                //{
                //    // "定向回价[已超时]";
                //    Common.Handler.Base.BaseHandler.Instance.PublishAppMsg(new PResult() { Code = "DRPMsgPo", Info1 = "MsgPo", Msg = JsonConvert.SerializeObject(msgPo) });
                //    return;
                //}


                // 回价反向
                bool side = (dr.iocOrder.side.ToUpper() == "SELL" || dr.iocOrder.side == "2") ? true : false;

                DRPParamInfoModel dRPParam = new DRPParamInfoModel() { OrderId = dr.iocOrder.orderId };

                Common.View.Order.SendPointPriceView spv = new Common.View.Order.SendPointPriceView(dr.iocOrder.symbol, side, price, qty, dr.iocOrder.settlType, dr.iocOrder.netPrice.ToString("N4"),
                 dr.iocOrder.venue, dr.iocOrder.wholeflag, dr.iocOrder.firmFlag == "1" ? false : true, dr.iocOrder.quoteId, null, null, null, null, null, null, dr.iocOrder.venueGW, dr.iocOrder.iocTimeOut, null, true, $"{dr.order.roleName}-{dr.order.partysubname}", dr.order.price.ToString("N4"), 
                 dr.order.qty.ToString() == "0" ? "--" : (dr.order.qty - dr.order.cumQty).ToString(), dr.order.wholeflag, bargainyield, dRPParam);
                spv.ShowDialog();
            }
        }

        public void ProcChatMsg(MsgPo msgPo)
        {
            Common.Handler.Base.BaseHandler.Instance.PublishAppMsg(new PResult() { Code = "ChatMsg", Msg = msgPo.MessageContent.id });
        }

        /// <summary>
        ///  内部看板订阅处理
        /// </summary>
        /// <param name="detail"></param>
        public void ProcTableSvrFocusTag(MsgPo msgPo)
        {
            Common.Handler.Base.BaseHandler.Instance.PublishAppMsg(new PResult() { Code = "TableSvr_FOCUS_TAG", Msg = JsonConvert.SerializeObject(msgPo.MessageContent) });
        }

        /// <summary>
        ///  需求工作台订阅处理
        /// </summary>
        /// <param name="detail"></param>
        public void ProcTableSvrFocusTag_RW(MsgPo msgPo)
        {
            Common.Handler.Base.BaseHandler.Instance.PublishAppMsg(new PResult() { Code = "TableSvr_FOCUS_TAG_RW", Msg = JsonConvert.SerializeObject(msgPo.MessageContent) });
        }

        /// <summary>
        ///  内部看板订阅复制
        /// </summary>
        /// <param name="detail"></param>
        public void ProcTableSvrFocusTag_Copy(MsgPo msgPo, bool needPrompt = true)
        {
            msgPo.MessageContent.infos = needPrompt ? "1" : "0";
            Common.Handler.Base.BaseHandler.Instance.PublishAppMsg(new PResult() { Code = "TableSvr_FOCUS_TAG_COPY", Msg = JsonConvert.SerializeObject(msgPo.MessageContent), Info1 = needPrompt ? "1" : "0" });
        }

        /// <summary>
        /// 需求工作台订阅复制
        /// </summary>
        /// <param name="detail"></param>
        public void ProcTableSvrFocusTag_RW_Copy(MsgPo msgPo, bool needPrompt = true)
        {
            msgPo.MessageContent.infos = needPrompt ? "1" : "0";
            Common.Handler.Base.BaseHandler.Instance.PublishAppMsg(new PResult() { Code = "TableSvr_FOCUS_TAG_RW_COPY", Msg = JsonConvert.SerializeObject(msgPo.MessageContent), Info1 = needPrompt ? "1" : "0" });
        }

        /// <summary>
        ///  策略控制台消息
        /// </summary>
        /// <param name="detail"></param>
        public void ProcQuantFocusTag(MsgPo msgPo)
        {
            Common.Handler.Base.BaseHandler.Instance.PublishAppMsg(new PResult() { Code = "Quant_Focus_Tag", Msg = JsonConvert.SerializeObject(msgPo.MessageContent) });
        }

        public void ProcInquiryFail(MsgPo msgPo)
        {
            Common.Handler.Base.BaseHandler.Instance.PublishAppMsg(new PResult() { Code = "TableSvr_Inquiry_Fail", Msg = JsonConvert.SerializeObject(msgPo.MessageContent) });
        }

        /// <summary>
        /// 设置消息为已读
        /// </summary>
        /// <param name="msgPo"></param>
        /// <returns></returns>
        public void SetReadflag(MsgPo msgPo)
        {
            msgPo.InnerColorAnimation = null;
            Common.Handler.Base.BaseHandler.Instance.PublishAppMsg(new PResult() { Code = "MsgPoReadFlag", Info1 = "MsgPo", Msg = Newtonsoft.Json.JsonConvert.SerializeObject(msgPo) });
        }

        /// <summary>
        /// 消息记录数
        /// </summary>
        /// <param name="msgPo"></param>
        /// <returns></returns>
        public void SetMsgUIRefreshCount()
        {
            Common.Handler.Base.BaseHandler.Instance.PublishAppMsg(new PResult() { Code = "MsgPoReadFlag", Info1 = "MsgUIRefreshCount", Msg = "" });
        }

        /// <summary>
        /// 处理 TableSvr Unref 消息提醒
        /// </summary>
        /// <param name="detail"></param>
        public void ProcTableSvrOrderUnRefMsg(MsgPo msgPo)
        {
            Common.Handler.Base.BaseHandler.Instance.PublishAppMsg(new PResult() { Code = "TableSvrOrderUnRefMsg", Msg = JsonConvert.SerializeObject(msgPo.MessageContent) });
        }

        internal void SendQQ(MsgPo msgPo)
        {
            if (msgPo != null)
            {
                //  || msgPo.ContentType == "TableSvr_FOCUS_TAG_RW" 没有需求工作台
                if ((msgPo.ContentType == "TableSvr_FOCUS_TAG")
                    && msgPo.HeaderTitle != null && msgPo.HeaderTitle.Contains("提醒")
                    && msgPo.Content != null && msgPo.Content.Contains("报价"))
                {
                    DetailOrder deal = MsgHelper.Deserialize<DetailOrder>(msgPo.Detail);

                    if (deal != null && deal.username != LocalEnv.UserInfo.UserName)
                    {
                        string receiver = CommonUtils.GetQQNumber(deal.username);
                        string message = string.Empty;

                        string remainingTime = string.Empty;
                        BondsInfoPo bondsInfoPo = Common.Utils.CommonUtils.GetBondsInfoPoBySecurityID(deal.securityid);
                        if (bondsInfoPo != null && string.IsNullOrEmpty(remainingTime))
                        {
                            remainingTime = bondsInfoPo.RemaingingTime;
                        }
                        string symbol = deal.symbol;
                        string securityid = deal.securityid;
                        string side = deal.side == "1" ? "Bid" : "Ofr";
                        string price = !deal.price.HasValue ? string.Empty : $" {deal.price.Value.ToString("#.####")}w";
                        string qty = !deal.orderqty.HasValue ? string.Empty : $" {deal.orderqty.Value.ToString("#")}w";
                        string settlType = string.Empty;
                        if (!string.IsNullOrEmpty(deal.settltype)
                            && deal.settltype != "2"
                            && deal.settltype != "3"
                            && deal.settltype != "2,3")
                        {
                            settlType = " " + ComboSourceUtil.Instance.FindEnum(deal.settltype, "settletype");
                        }
                        message = $"{remainingTime} {symbol} {securityid} {side}{price}{qty}";

                        if (!string.IsNullOrEmpty(receiver) && !string.IsNullOrEmpty(LocalEnv.UserInfo.QQNumber))
                        {
                            ThreadPoolHelper.InvokeThreadPool(o =>
                            {
                                SendDesktopFinQReq request = new SendDesktopFinQReq();
                                request.Username = LocalEnv.UserInfo.UserName;
                                request.sender = LocalEnv.UserInfo.QQNumber;
                                request.receiver = receiver;
                                request.send_direct = false;
                                request.clear_edit = true;
                                request.channel = "0";
                                request.message = message;
                                Common.Model.Fix.Message.PResult result = FinQHandler.Instance.SendDesktopFinQ(request);

                                if (result != null && !result.IsSuccess())
                                {
                                    string errorMsg = "失败";
                                    if (!string.IsNullOrEmpty(result.Msg))
                                        errorMsg = result.Msg;
                                    CommonUtils.Prompt2Second("发起客户单聊QQ", errorMsg);
                                }
                            });
                            ClipboardHelper.CopyToClipboard(message);
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(LocalEnv.UserInfo.QQNumber))
                                CommonUtils.Prompt2Second("发起客户单聊QQ", "请联系管理员设置本用户对应QQ号码!");
                            else if (string.IsNullOrEmpty(receiver))
                                CommonUtils.Prompt2Second("发起客户单聊QQ", "没有找到对应交易员的QQ号码！");
                        }
                    }
                }
            }
        }
    
    
        internal void IgnoreMsgs()
        {
            TBatchMsgReq batchMsgReq = new TBatchMsgReq();
            batchMsgReq.delflag = "1";
            batchMsgReq.readflag = "1";

            batchMsgReq.id = new List<string>();

            foreach (MsgPo msgPo in _showData)
            {
                batchMsgReq.id.Add(msgPo.Id);
            }

            UpdateMsg(batchMsgReq);
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
                    //Application.Current.Dispatcher.Invoke(() =>
                    //{
                    //    btnbatchClear.IsEnabled = true;
                    //});
                }

            });
        }

        private void RemoveMsg(TBatchMsgReq request)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                List<MsgPo> removeList = new List<MsgPo>();
                foreach (MsgPo ms in _showData)
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
                        _showData.Remove(msgPo);

                        //if (msgDic.ContainsKey(msgPo.Id))
                        //{
                        //    MsgPo tem = null;
                        //    msgDic.TryRemove(msgPo.Id, out tem);
                        //}
                    }
                }
            });
        }
    }
}
