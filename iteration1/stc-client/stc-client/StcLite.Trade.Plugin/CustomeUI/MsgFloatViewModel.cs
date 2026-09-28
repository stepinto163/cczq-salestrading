using Ats.Foundation.Message;
using com.gateway.log;
using BDB.Trade.Plugin.CustomeUI;
using BDB.Trade.Plugin.CustomUI;
using BDB.Trade.Plugin.Utils;
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
using BDB.Price.Plugin.View.Trade.Card;
using BDB.Price.Plugin.Cache;
using STC.Common.Model;
using STC.Common.Utils;
using STC.Common.Model.Admin;
using STC.Common.Model.Inner;
using STC.Common.Handler.Inner;
using STC.Common.Model.GW;
using STC.Common.Handler.GW;
using STC.Common.Model.Fix.Message;
using STC.Common.Handler.Admin;

namespace BDB.XBond.Plugin.CustomeUI
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

        private Color _redBig = (Color)System.Windows.Media.ColorConverter.ConvertFromString("#136C5E");
        private Color _redSmall = (Color)System.Windows.Media.ColorConverter.ConvertFromString("#136C5E");

        private Color _orangeBig = (Color)System.Windows.Media.ColorConverter.ConvertFromString("#282828");
        private Color _orangeSmall = (Color)System.Windows.Media.ColorConverter.ConvertFromString("#50282828");

        private Color _redBest = (Color)System.Windows.Media.ColorConverter.ConvertFromString("#E63034");
        private Color _orangeBest = (Color)System.Windows.Media.ColorConverter.ConvertFromString("#ED6D00");
        private Thread _reafreshThread = null;

        private Color _nonfirmFAKBig = (Color)System.Windows.Media.ColorConverter.ConvertFromString("#452222");
        private Color _nonfirmFAKSmall = (Color)System.Windows.Media.ColorConverter.ConvertFromString("#222223");

        public ObservableCollection<MsgPo> ShowData { get { return _showData; } }
        private bool _isRefresh = true;

        /// <summary>
        /// 构造函数
        /// </summary>
        public MsgFloatViewModel()
        {
            _msgTask = new AsyncThreadQueue<MsgPo>(OnMsg);
            _reafreshThread = new Thread(RefreshTime);
            _reafreshThread.IsBackground = true;

        }

        ~MsgFloatViewModel()
        {
            _isRefresh = false;
            if (_reafreshThread != null)
            {
                _reafreshThread.Abort();
                _reafreshThread = null;
            }

            if (_msgTask != null)
            {
                _msgTask.Clear();
                _msgTask.Dispose();
            }
        }


        #region 属性

        public ICommand PopQuickDealCommand { get { return new RelayCommand(PopQuickDeal); } }

        public ICommand PopDealCommand { get { return new RelayCommand(PopDealClick); } }
        public ICommand PopKnowCommand { get { return new RelayCommand(PopKnowClick); } }
        public ICommand PopLookCommand { get { return new RelayCommand(PopLookClick); } }

        public ICommand PopCopyCommand { get { return new RelayCommand(PopCopyClick); } }

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
                    //    //设置消息为已读
                    //    SetReadflag(msgPo);

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
                    //SetReadflag(msgPo);

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
                        ExecuteReplyView executeReplyView = new ExecuteReplyView(msgPo);
                        executeReplyView.ShowDialog();
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
                    else if (msgPo.ContentType.Equals("Quant_Focus_Tag"))
                    {
                        lock(_showData)
                        {
                            _showData.Remove(msgPo);
                        }
                        //策略控制台消息
                        //ProcQuantFocusTag(msgPo);
                    }

                    else if (msgPo.ContentType.Equals(AdminConsts.PriceMatch))
                    {
                        PriceMatchView priceMatchView = new PriceMatchView(msgPo);
                        priceMatchView.ShowDialog();
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

        /// <summary>
        /// 处理知道了的消息
        /// </summary>
        /// <param name="msgPo"></param>
        public void KnowMsg(MsgPo msgPo)
        {
            if (msgPo != null)
            {
                //设置消息为已读
                //SetReadflag(msgPo);

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

                            if (result != null && result.IsSuccess())
                            {
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
                            else if (msgPo.ContentType.Equals("DQB") || msgPo.ContentType.Equals("DQ"))
                            {
                                Application.Current.Dispatcher.Invoke(() =>
                                {
                                    lock (_showData)
                                    {
                                        MsgPo removeMsgPo = _showData.FirstOrDefault(Item => Item.Id == msgPo.Id);
                                        if (removeMsgPo != null)
                                        {
                                            _showData.Remove(removeMsgPo);
                                        }
                                    }
                                });
                            }

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
                    //SetReadflag(msgPo);

                    ExecuteReplyView executeReplyView = new ExecuteReplyView(msgPo);
                    executeReplyView.Show();
                }
            }
        }

        private void PopCopyClick()
        {
            DataGrid dgMsg = _myOwn.FindName("dgMsg") as DataGrid;
            if (dgMsg != null)
            {
                MsgPo msgPo = dgMsg.SelectedItem as MsgPo;
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
        }

        private void PopCpenBook()
        {
            DataGrid dgMsg = _myOwn.FindName("dgMsg") as DataGrid;
            if (dgMsg != null)
            {
                MsgPo msgPo = dgMsg.SelectedItem as MsgPo;
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

            while (_isRefresh)
            {
                try
                {
                    Thread.Sleep(1000);
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
                                    TimeSpan diffTime = DateTimeEx.ServerTimeNow - transactTime;

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
                                if (!string.IsNullOrEmpty(msgPo.ContentType) && !msgPo.ContentType.Equals(AdminConsts.Admin_IT) && DateTime.TryParse(msgPo.UpdateTime, out dt))
                                {

                                    TimeSpan diffTime = DateTimeEx.GetNow().AddSeconds(diffspan.TotalSeconds) - dt;
                                    isTrue = diffTime.TotalSeconds > 60;
                                }

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
                    data.StatusBrush = (Brush)_myOwn.FindResource("ColorBrush.Red7");// new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#E63034"));
                    data.StatusVisibility = Visibility.Visible;
                }
                else if (data.Level.Equals("2"))
                {
                    data.Status = "";
                    data.StatusWidth = 80;
                    data.StatusBrush = (Brush)_myOwn.FindResource("ColorBrush.Orange5");//new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#ED6D00"));
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
                data.PopDealVisibility = Visibility.Visible;

                if (!string.IsNullOrEmpty(data.HeaderTitle) && data.HeaderTitle.Contains("成交"))
                {
                    data.StatusVisibility = Visibility.Collapsed;
                    data.Status = string.Empty;
                }
                else
                {
                    data.Status = "";
                    data.StatusBrush = (Brush)_myOwn.FindResource("ColorBrush.Red7");//new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#E63034"));
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

                    ColorAnimation ca = new ColorAnimation();
                    ca.Duration = new Duration(TimeSpan.FromSeconds(60));

                    if (data.Level.Equals("1")) //紧急
                    {
                        ca.From = _redBig;
                        ca.To = _redSmall;

                    }
                    else if (data.Level.Equals("2")) //警告
                    {
                        ca.From = _orangeBig;
                        ca.To = _orangeSmall;
                    }
                    else if (data.Level.Equals("3")) //普通
                    {
                        ca.From = _orangeBig;
                        ca.To = _orangeSmall;
                    }

                    ca.From = _redBig;
                    ca.To = _redSmall;


                    if (data.ContentType.Equals(AdminConsts.Admin_ER))
                    {
                        ExecutionReport deal = MsgHelper.Deserialize<ExecutionReport>(data.Detail);
                        if (deal != null)
                        {
                            if (deal.Side.ToUpper().Equals("SELL") || deal.Side.Equals("2"))
                            //if (data.Level == AdminConsts.Admin_Normal)
                            {
                                data.ShowBG = (Brush)_myOwn.FindResource("ColorBrush.Blue-12");
                                data.ShowBorderBrush = (Brush)_myOwn.FindResource("ColorBrush.Blue8");
                            }
                            else
                            {
                                data.ShowBG = (Brush)_myOwn.FindResource("ColorBrush.Orange-9");
                                data.ShowBorderBrush = (Brush)_myOwn.FindResource("ColorBrush.Orange4");
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
                                data.ShowBG = (Brush)_myOwn.FindResource("ColorBrush.BlackishGreen");
                                data.ShowBorderBrush = (Brush)_myOwn.FindResource("ColorBrush.Green");

                            }
                            else
                            {
                                data.ShowBG = (Brush)_myOwn.FindResource("ColorBrush.Red-17");
                                data.ShowBorderBrush = (Brush)_myOwn.FindResource("ColorBrush.Red13");
                            }
                        }
                    }
                    else if (data.ContentType.Equals("OD"))
                    {
                        //data.ShowBG = (Brush)_myOwn.FindResource("ColorBrush.Orange-9");
                        //data.ShowBorderBrush = (Brush)_myOwn.FindResource("ColorBrush.Orange4");

                        data.ShowBG = (Brush)_myOwn.FindResource("ColorBrush.Red-17");
                        data.ShowBorderBrush = (Brush)_myOwn.FindResource("ColorBrush.Red13");
                    }
                    else if (data.ContentType.Equals("BDB"))
                    {
                        data.BDBVisiblitity = Visibility.Visible;
                        data.KnowVisibility = Visibility.Collapsed;
                        data.PopDealVisibility = Visibility.Collapsed;
                        data.ShowBG = (Brush)_myOwn.FindResource("Green-7");
                        data.ShowBorderBrush = (Brush)_myOwn.FindResource("Green-3");
                        data.HeaderTitleFColor = (Brush)_myOwn.FindResource("Green-2");
                        data.ContentFColor = (Brush)_myOwn.FindResource("White-1");
                    }
                    else
                    {
                        data.InnerColorAnimation = ca;
                        data.LookVisibility = Visibility.Collapsed;
                    }


                    //data.ShowBG = (Brush)_myOwn.FindResource("ColorBrush.Blue-12");
                    //data.ShowBorderBrush = (Brush)_myOwn.FindResource("ColorBrush.Blue8");
                }
                else
                {
                    data.ShowHeight = 80;

                    data.Status = "";
                    data.ValueTimeVisibility = Visibility.Visible;
                    data.ValueTime = string.Empty;//"倒计时";
                    data.StatusBrush = (Brush)_myOwn.FindResource("ColorBrush.Orange8");
                    data.UpdateTimeBrush = (Brush)_myOwn.FindResource("ColorBrush.Red7");
                    data.UpdateTimeVisibility = Visibility.Collapsed;
                    data.TimeOutTxtVisibility = Visibility.Visible;

                    data.PopQuickDealVisibility = Visibility.Visible;
                    data.KnowVisibility = Visibility.Collapsed;

                    data.ShowBG = (Brush)_myOwn.FindResource("ColorBrush.Red8");

                    if (priceMatchDetail != null && priceMatchDetail.iocOrder != null)
                    {
                        DateTime transactTime = CommonUtils.ConvertToDateTime(priceMatchDetail.iocOrder.TransactTime);
                        TimeSpan diffTime = DateTimeEx.ServerTimeNow - transactTime;

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
            //SetMsgUIRefreshCount();
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
    }
}
