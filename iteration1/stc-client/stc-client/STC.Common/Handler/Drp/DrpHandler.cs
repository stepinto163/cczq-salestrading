using Ats.Foundation.Message;
using com.gateway.log;
using netapi.Api;
using Newtonsoft.Json;
using STC.Common.Api;
using STC.Common.Handler.Admin;
using STC.Common.Model.Admin;
using STC.Common.Model.Drp;
using STC.Common.Model.Fix.Message;
using STC.Common.Utils;
using STC.Common.View.Drp;
using STC.Common.View.Drp.Component;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace STC.Common.Handler.Drp
{
    public class DrpHandler
    {
        private static DrpHandler instance = new DrpHandler();
        public static DrpHandler Instance
        {
            get { return instance; }
        }

        private static System.Windows.Forms.Screen mainScreen = System.Windows.Forms.Screen.PrimaryScreen;
        private List<NotificationWindow> _dialogs = new List<NotificationWindow>();
       
        public DrpHandler()
        {

        }

        public void InitScreen()
        {
            Application.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Send, (Action)(() =>
            {
                _notificationManager.InitScreen(NotificationManager.GetMainScreen());
            }));
        }

        public readonly NotificationManager _notificationManager = NotificationManager.Instance;
        public void ShowTrayNotify(NotificationContent data)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(async () =>
            {
                data.CurScreen = NotificationManager.GetMainScreen();
                //初始化强提醒显示区
                if (!_notificationManager.LstScreen.ContainsKey(data.CurScreen.DeviceName))
                    _notificationManager.InitScreen(data.CurScreen);
                _notificationManager.Show(data);
                //await System.Windows.Threading.Dispatcher.Yield(System.Windows.Threading.DispatcherPriority.Render);
            }));
        }

        private object LockDict = new object();
        private Dictionary<string, TMessageContent> DictDq = new Dictionary<string, TMessageContent>();
        private Dictionary<string, DRPDetail> DictDRPDetail = new Dictionary<string, DRPDetail>();


        public void ProcDQMsg(List<TTbookSystemMessage> sysMsg, bool isHistory = false)
        {
            if (sysMsg == null)
                return;

            Application.Current.Dispatcher.BeginInvoke((Action)(async () =>
            {
                int total = sysMsg.Count > 500 ? 500 : sysMsg.Count;

                for (int i = 0; i < total; i++)
                {
                    TTbookSystemMessage msg = sysMsg[i];

                    bool isRemove = false;
                    if (msg.content != null)
                    {
                        if (string.IsNullOrEmpty(msg.content))
                        {
                            isRemove = true;
                        }

                        if (!JsonSplit.IsJson(msg.content))
                        {
                            isRemove = true;
                        }

                        if (isRemove)
                        {
                            //删除消息
                            string isdel = msg.delflag ?? "0";
                            if (isdel == "1")
                            {

                            }
                        }
                        else
                        {
                            try
                            {
                                TMessageContent detail = MsgHelper.Deserialize<TMessageContent>(msg.content, Ats.Foundation.Message.Serializer.MsgSerializerType.Json);
                                if (detail.contentType != null)
                                {
                                    string key = string.Empty;
                                    //保存最新消息，提供给我的订单查询和处理回调
                                    switch (detail.contentType)
                                    {
                                        case "DQ":
                                        case "DQB":
                                            DRPDetail dd = Newtonsoft.Json.JsonConvert.DeserializeObject<DRPDetail>(detail.detail);
                                            key = dd.order.orderId;
                                            //定向回价
                                            //议价或拒绝消息更新
                                            lock (LockDict)
                                            {
                                                //如果已经包含定向回价
                                                if (DictDq.ContainsKey(key))
                                                {
                                                    //首先更新新的 ValidUntilTime
                                                    DRPDetail oridd = DictDRPDetail[key];
                                                    foreach (DRPValue dr in dd.iocOrderDqList)
                                                    {
                                                        DRPValue srcdr = oridd.iocOrderDqList.FirstOrDefault(o => o.orderId == dr.orderId);
                                                        if (srcdr != null)
                                                        {
                                                            dr.ValidUntilTime = srcdr.ValidUntilTime;
                                                        }
                                                    }

                                                    detail.creatTime = DictDq[key].creatTime;
                                                    DictDq[key] = detail;
                                                    for (int jj = 0; jj < dd.iocOrderDqList.Count; jj++)
                                                    {
                                                        if (!string.IsNullOrEmpty(dd.iocOrderDqList[jj].iocTimeOut))
                                                        {
                                                            int icotimeout = 0;
                                                            int.TryParse(dd.iocOrderDqList[jj].iocTimeOut.ToString(), out icotimeout);
                                                            if (string.IsNullOrEmpty(dd.iocOrderDqList[jj].ValidUntilTime))
                                                                dd.iocOrderDqList[jj].ValidUntilTime = DateTimeEx.GetNow().AddSeconds(icotimeout).ToString("yyyy-MM-dd HH:mm:ss.fff");
                                                        }
                                                    }

                                                    DictDRPDetail[key] = dd;
                                                }
                                                else
                                                {
                                                    detail.creatTime = DateTimeEx.GetNow().ToString("yyyy-MM-dd HH:mm:ss.fff");
                                                    DictDq[dd.order.orderId] = detail;

                                                    for (int jj = 0; jj < dd.iocOrderDqList.Count; jj++)
                                                    {
                                                        if (!string.IsNullOrEmpty(dd.iocOrderDqList[jj].iocTimeOut))
                                                        {
                                                            int icotimeout = 0;
                                                            int.TryParse(dd.iocOrderDqList[jj].iocTimeOut.ToString(), out icotimeout);
                                                            dd.iocOrderDqList[jj].ValidUntilTime = DateTimeEx.GetNow().AddSeconds(icotimeout).ToString("yyyy-MM-dd HH:mm:ss.fff");
                                                        }
                                                    }

                                                    DictDRPDetail[dd.order.orderId] = dd;
                                                }
                                            }

                                            //发布消息
                                            Common.Handler.Base.BaseHandler.Instance.PublishAppMsg(new PResult() { Code = "DRPMsgPo", Info1 = "TMessageContent", Msg = JsonConvert.SerializeObject(detail) });
                                            break;
                                        case "ER":
                                            //接收成交回报，用于关闭被点价窗口
                                            ExecutionReport er = Newtonsoft.Json.JsonConvert.DeserializeObject<ExecutionReport>(detail.detail);
                                            lock (LockDict)
                                            {
                                                if (!string.IsNullOrEmpty(er.QuoteID) && DictDq.ContainsKey(er.QuoteID))
                                                {
                                                    if (er.LeavesQty == "0")
                                                    {
                                                        DictDq.Remove(er.QuoteID);
                                                        DictDRPDetail.Remove(er.QuoteID);
                                                    }
                                                }
                                            }

                                            //发布消息
                                            Common.Handler.Base.BaseHandler.Instance.PublishAppMsg(new PResult() { Code = "DRPMsgPo", Info1 = "TMessageContent", Msg = JsonConvert.SerializeObject(detail) });
                                            break;
                                    }

                                    //更新detail.detail
                                    if (DictDRPDetail.ContainsKey(key))
                                    {
                                        detail.detail = JsonConvert.SerializeObject(DictDRPDetail[key]);
                                    }

                                    if (detail.contentType.Equals("DQ"))
                                    {
                                        //定向回价
                                        if (detail.show ?? true)
                                        {
                                            ShowTrayNotify(new Common.View.Drp.NotificationContent() { Id = detail.id, ContentType = detail.contentType, Detail = detail.detail, IsShow = detail.show ?? true, DelFlag = msg.delflag ?? "0", Message = JsonConvert.SerializeObject(msg), Title = detail.title, Type = NotificationType.NotificationWindow });
                                        }
                                    }
                                    else if (detail.contentType.Equals("DQB"))
                                    {
                                        //议价或拒绝消息更新
                                        ShowTrayNotify(new Common.View.Drp.NotificationContent() { Id = detail.id, ContentType = detail.contentType, Detail = detail.detail, IsShow = detail.show ?? true, DelFlag = msg.delflag ?? "0", Message = JsonConvert.SerializeObject(msg), Title = detail.title, Type = NotificationType.NotificationWindow });
                                    }
                                    else if (detail.contentType.Equals("IT"))
                                    {
                                        //点价提醒,同时更新
                                        if (!isHistory)
                                        {
                                            PointPriceDetailModel ppdm = Newtonsoft.Json.JsonConvert.DeserializeObject<PointPriceDetailModel>(detail.detail);
                                            ppdm.iocOrder.validuntitime = STC.Common.Utils.DateTimeEx.GetNow().AddSeconds(int.Parse(ppdm.iocOrder.iocTimeOut)).ToString("yyyy-MM-dd HH:mm:ss.fff");
                                            ShowTrayNotify(new Common.View.Drp.NotificationContent() { Id = detail.id, ContentType = detail.contentType, Detail = JsonConvert.SerializeObject(ppdm), IsShow = detail.show ?? true, DelFlag = msg.delflag ?? "0", Message = JsonConvert.SerializeObject(msg), Title = detail.title, Type = NotificationType.NofirmPointPriceView });
                                        }
                                        else
                                        {
                                            //如果客户端退出，则从transactTime计算出本地的过期时间
                                            PointPriceDetailModel ppdm = Newtonsoft.Json.JsonConvert.DeserializeObject<PointPriceDetailModel>(detail.detail);
                                            DateTime newdelaytime = new DateTime(CommonUtils.ConvertToDateTime(ppdm.iocOrder.transactTime).AddSeconds(int.Parse(ppdm.iocOrder.iocTimeOut)).Ticks - LocalEnv.ServerInfo.Add_ServerTicks);
                                            ppdm.iocOrder.validuntitime = newdelaytime.ToString("yyyy-MM-dd HH:mm:ss.fff");
                                            ShowTrayNotify(new Common.View.Drp.NotificationContent() { Id = detail.id, ContentType = detail.contentType, Detail = JsonConvert.SerializeObject(ppdm), IsShow = detail.show ?? true, DelFlag = msg.delflag ?? "0", Message = JsonConvert.SerializeObject(msg), Title = detail.title, Type = NotificationType.NofirmPointPriceView });
                                        }
                                    }
                                    else if (detail.contentType.Equals("ER"))
                                    {
                                        //接收成交回报，用于关闭被点价窗口
                                        ShowTrayNotify(new Common.View.Drp.NotificationContent() { Id = detail.id, ContentType = detail.contentType, Detail = detail.detail, IsShow = detail.show ?? true, DelFlag = msg.delflag ?? "0", Message = JsonConvert.SerializeObject(msg), Title = detail.title, Type = NotificationType.ER });
                                    }
                                    else if (detail.contentType.Equals("PM"))
                                    {
                                        //接收价格匹配消息，用于拒绝点价时，接收拒绝消息，关闭被点价窗口 "delflag": "1",
                                        ShowTrayNotify(new Common.View.Drp.NotificationContent() { Id = detail.id, ContentType = detail.contentType, Detail = detail.detail, IsShow = detail.show ?? true, DelFlag = msg.delflag ?? "0", Message = JsonConvert.SerializeObject(msg), Title = detail.title, Type = NotificationType.PM });
                                    }
                                    else if (detail.contentType.ToUpper().Equals("TABLESVR_ORDER_UNREF"))
                                    {
                                        //日初UnRef消息
                                        if (!string.IsNullOrEmpty(msg.id))
                                            detail.id = msg.id;
                                        ShowTrayNotify(new Common.View.Drp.NotificationContent() { Id = detail.id, ContentType = detail.contentType, Detail = detail.detail, IsShow = detail.show ?? true, DelFlag = msg.delflag ?? "0", Message = JsonConvert.SerializeObject(msg), Title = detail.title, Type = NotificationType.LastWorkDayUnRefView });
                                    }
                                    else if (detail.contentType.ToUpper().Equals(AdminConsts.Admin_MM))
                                    {
                                        //双边断报消息，关闭窗口 "delflag": "1",
                                        ShowTrayNotify(new Common.View.Drp.NotificationContent() { Id = msg.id, ContentType = detail.contentType, Detail = detail.detail, IsShow = detail.show ?? true, DelFlag = msg.delflag ?? "0", Message = JsonConvert.SerializeObject(msg), Title = detail.title, Type = NotificationType.ANS });
                                    }

                                }
                            }
                            catch (Exception ex)
                            {
                                LogHelper.Error(ex);
                            }
                        }
                    }
                }

                await System.Windows.Threading.Dispatcher.Yield(System.Windows.Threading.DispatcherPriority.Render);
            }));
        }

        /// <summary>
        /// 根据消息ID获取消息体内容
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public TMessageContent GetContent(string key)
        {
            lock (LockDict)
            {
                if (DictDq.ContainsKey(key))
                {
                    TMessageContent _tmessageContent = DictDq[key];
                    if (DictDRPDetail.ContainsKey(key))
                        _tmessageContent.detail = JsonConvert.SerializeObject(DictDRPDetail[key]);
                    return _tmessageContent;
                }
                else
                    return null;
            }
        }

        /// <summary>
        /// 根据消息ID获取消息体内容
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public DRPDetail GetDRPDetail(string key)
        {
            lock (LockDict)
            {
                if (DictDRPDetail.ContainsKey(key))
                    return DictDRPDetail[key];
                else
                    return null;
            }
        }

        private void Dialog_Closed(object sender, EventArgs e)
        {
            var closedDialog = sender as NotificationWindow;
            _dialogs.Remove(closedDialog);
        }


        /// <summary>
        /// 查询报价日志
        /// </summary>
        /// <param name="QueryDqOrderLog"></param>
        /// <returns></returns>
        public DRPQueryOrderLogResult QueryDqOrderLog(DRPQueryOrderLogReq req)
        {
            return ApiManager.Instance.GetDefaultApi().Request<DRPQueryOrderLogResult>(ApiConsts.AdminSvr, ApiConsts.Topic_stc_queryDQOrderLog, req);
        }

        /// <summary>
        /// 收到点价反馈
        /// </summary>
        public Dictionary<string, ExecutionReport> DictTBookOrders = new Dictionary<string, ExecutionReport>();
        public ConcurrentDictionary<string, List<ExecutionReport>> DictGROrders = new ConcurrentDictionary<string, List<ExecutionReport>>();
        private object lockObject = new object();
        
        /// <summary>
        /// 被点价窗口
        /// </summary>
        public ConcurrentDictionary<string, string> DictIntentionB = new ConcurrentDictionary<string, string>();
        
        /// <summary>
        /// 我的客户发来意向
        /// </summary>
        public ConcurrentDictionary<string, string> DictIntention = new ConcurrentDictionary<string, string>();

        public void ShowPTListBrokerNotify(NotificationContent data)
        {
            Application.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Send, (Action)(() =>
            {
                data.CurScreen = NotificationManager.GetMainScreen();
                _notificationManager.ShowPTListBrokerUI(data);
            }));
        }

        private object lockDict = new object();
        private List<string> Ctlist = new List<string>() { "0", "1", "2", "3", "11" ,"23", "24", "25", "26", "27", "28", "29", "34", "47", "48" };
        private List<string> Bstatus = new List<string>() { "0","1" };
        public void ProcPTListBrokerMsg(List<ExecutionReport> list, bool isUpdate = true, bool isFromHistory = false, bool isFromTbook = false)
        {
            //string s = FileHelper.ReadFullText("c:\\temp\\1.txt"); //441822929 244462757
            //list = new List<ExecutionReport>() { CommonUtils.DeserializeObject<ExecutionReport>(s) };
            //list[0].PartySubID = LocalEnv.UserInfo.UserId;
            //list[0].PartySubName = LocalEnv.UserInfo.UserName;
            lock (lockObject)
            {
                #region 注释
                ////--------------------------------------------------------------------------------------------------
                ////CommandType值类型：												OrdStatus
                ////--------------------------------------------------------------------------------------------------
                ////Default(0, "默认值"),											Frozen("I"),
                ////Confirm(1, "确认"),												Newing("N"),
                ////ModifyPrice(2, "修改价格"),										Pending_Newing("S"),
                ////ModifyQty(3, "修改数量"),										New("0"),
                ////ModifyBondCode(4, "修改债券代码"),								Partially_Filled("1"),
                ////ModifyFandian(5, "修改返点"),									Filled("2"),
                ////ModifyBridgeFlag(6, "修改桥状态"),								Intention("3"),
                ////AddBridge(7, "添加桥"),											Cancelled("4"),
                ////ModifyBridgeB(8, "修改加桥按钮B状态"),							WAIT_FOR_PLEDGE_BOND("5"),
                ////ModifyBridgeO(9, "修改加桥按钮O状态"),							Pending_Cancel("6"),
                ////DeleteBridge(10, "删除桥"),										WAIT_FOR_PLEDGE_BOND_Timeout("7"),
                ////DeleteOrder(11, "删除成交单"),									Rejected("8"),
                ////ModifySteA(12, "修改结算类型A"),								Expired("C"),
                ////ModifySteB(13, "修改结算类型B"),								BrokerBConfirm("E"),
                ////ModifyBridgeInfo(14, "修改桥要素"),								BrokerBCanceled("F"),
                ////ModifySendto(15, "修改A发给"),									BrokerAConfirm("G"),
                ////ModifySendto2(16, "修改B发给"),									BrokerACanceled("H"),
                ////ModifyBrokerA(17, "修改BrokerA"),								BrokerManual("I"),
                ////ModifyBrokerB(18, "修改BrokerB"),								BrokerDelete("D");
                ////ModifyTradeA(19, "修改TraderA"),								
                ////ModifyTradeB(20, "修改TraderB"),								
                ////ModifyBrokerageA(21, "修改BridgeA是否免佣金"),								
                ////ModifyBrokerageB(22, "修改BridgeA是否免佣金"),								
                ////DCS(23, "DCS"),								
                ////BrokerAConfirm(24, "A确认"),								
                ////BrokerAReject(25, "A拒绝"),								
                ////BrokerBConfirm(26, "B确认"),								
                ////BPartiallyFilled(27, "B部分确认"),								
                ////BrokerBReject(28, "B拒绝"),								
                ////Newing(29, "新意向"),								
                ////BookBridge(30, "预约桥"),								
                ////ChangeBridgeAdd(31, "换桥加桥"),								
                ////ModifySend(32, "修改A、B发给"),								
                ////ChangeBridgeBook(33, "换桥预约桥"),								
                ////ForbidRefreshOrder(34, "市场成交操作"),								
                ////CancelForbidRefreshOrder(35, "取消不报市场"),								
                ////ManualSendDealA(36, "手动发送TraderA成交"),								
                ////ManualSendDealB(37, "手动发送TraderB成交"),								
                ////UpdateBondCode(38, "更新债券代码"),								
                ////ModifyIssueDir(39, "修改发给方向"),								
                ////ModifyIssueChannel(40, "修改发给渠道"),								
                ////ModifyIssueInstTrader(41, "修改发单机构"),								
                ////ModifyIssueRemark(42, "修改发单给备注"),								
                ////ModifyIssueProxyFee(43, "修改代付费"),								
                ////CancelIssueProxyFee(44, "取消代付费"),								
                ////ModifyIssueState(45, "开启发给标准化"),								
                ////ModifySteAB(46, "同时修改结算类型A、B"),								
                ////SendDetailQQMsgA(47, "发送单条明细QQ消息A"),								
                ////SendDetailQQMsgB(48, "发送单条明细QQ消息B"),								
                ////UpdateDetailMark(49, "标记/取消标记明细"),								
                ////UpdateInstExecorderState(50, "修改发单机构状态")
                ////UpdateInstExecorderState(51, "日终处理")
                #endregion

                try
                {
                    //From TBook
                    //popupERCollect.isUpdate = false;
                    //popupERCollect.isFromTbook = true;
                    //popupERCollect.isFromHistory = false;

                    //From 订单 
                    //popupERCollect.isUpdate = true;
                    //popupERCollect.isFromTbook = false;
                    //popupERCollect.isFromHistory = false;

                    //From 历史
                    //popupERCollect.isFromHistory = true;
                    //popupERCollect.isFromTbook = false;
                    //popupERCollect.isFromHistory = false;

                    //判断不是Tbook，则是 GR订单消息或点击历史订单
                    if (!isFromTbook)
                    {
                        // From Gr订单  GetBrokerABStatus: 0可操作确认 1待确认，2部分确认，3拒绝，4全部确认
                        if (isUpdate)
                        {
                            //判断订单消息是否是线下成交单更新成交信息,并且有未确认订单
                            if (list.Where(a => "1".Equals(a.EntryType) && LocalEnv.UserInfo.IsMyself(a.CounterPartySubID) && Bstatus.Contains(GetBrokerABStatus(a, true))).ToList().Count > 0)
                            {
                                //跳出条件判断，显示或处理线下成交单
                            }
                            else
                            {
                                //如果不是线下成交单，则根据CommandType和OrderStatus处理
                                List<ExecutionReport> brokerNotList = list.Where(a => Ctlist.Contains(a.CommandType ?? "-1")).ToList();
                                if (brokerNotList.Count == 0)
                                {
                                    return;
                                }

                                List<ExecutionReport> brokerBList = list.Where(a => !(!Ctlist.Contains(a.CommandType ?? "-1") && (GetBrokerABStatus(a, true) != "0"))).ToList();
                                if (brokerBList.Count == 0)
                                    return;
                            }
                        }

                        //From 历史
                        if (isFromHistory)
                        {
                            //判断brokerA是否有未处理订单
                            List<ExecutionReport> brokeaProcList = list.Where(a => GetBrokerABStatus(a, false) == "0").ToList();
                            List<ExecutionReport> brokeaBProcList = list.Where(a => Bstatus.Contains(GetBrokerABStatus(a, true))).ToList();
                            if (brokeaProcList.Count == 0 && brokeaBProcList.Count == 0)
                                return;
                        }
                    }

                    lock (lockDict)
                    {
                        //我的客户被点价
                        string key = $"Broker{list[0].StrategyOrderId}";
                        if (!DictIntentionB.ContainsKey(key))
                        {
                            DictIntentionB.TryAdd(key, key);
                        }
                    }

                    //线下录入
                    //UnderlineExecTpicUI
                    List<ExecutionReport> tempList = list.Where(a => "1".Equals(a.EntryType) && LocalEnv.UserInfo.IsMyself(a.CounterPartySubID)).ToList();
                    if (tempList != null && tempList.Count() > 0)
                    {
                        if (!(tempList[0].OrdStatus == "D" && tempList[0].DelFlag == "1"))
                        {
                            ShowPTListBrokerNotify(new Common.View.Drp.NotificationContent() { Id = tempList[0].ExecID, ContentType = "ET", Detail = "", IsShow = true, DelFlag = "0", Message = JsonConvert.SerializeObject(list), Title = "", Type = NotificationType.BROKER });
                        }
                        return;
                    }

                    #region //点价失败
                    //OrderMatchFailedUI
                    tempList = list.Where(a => (("8".Equals(a.OrdStatus) || "4".Equals(a.OrdStatus)) && CommonUtils.ConvertToLong(a.CumQty) >= 0) && LocalEnv.UserInfo.IsMyself(a.UserID)).ToList();
                    if (tempList != null && tempList.Count() > 0)
                    {
                        //TBook撤单，显示 没有量，IDB撤单不显示窗口     
                        if (list.Count == 1 && list[0].OrdStatus == "4" && list[0].Rejecter == "TBook")
                        {
                            ShowPTListBrokerNotify(new Common.View.Drp.NotificationContent() { Id = tempList[0].OrderID, ContentType = "OMF", Detail = "", IsShow = true, DelFlag = "0", Message = JsonConvert.SerializeObject(list), Title = "", Type = NotificationType.BROKER });
                            return;
                        }
                    }
                    #endregion


                    #region 我的客户发来意向 (当点价成功)
                    //我的客户发来意向窗口 一个窗口有多个 
                    //IntentionUI  isOrder = true  收到消息   && a.CommandType!="34"

                    tempList = list.Where(a => ("2".Equals(a.OrdStatus) || "4".Equals(a.OrdStatus) || "N".Equals(a.OrdStatus)) && a.CommandType != "34" && CommonUtils.ConvertToLong(a.CumQty ?? "-1") < 0 && LocalEnv.UserInfo.IsMyself(a.UserID)).ToList();
                    if (tempList != null && tempList.Count() > 0)
                    {
                        if ("4".Equals(tempList[0].OrdStatus))
                            return;

                        lock (lockDict)
                        {
                            string key = $"Broker{tempList[0].StrategyOrderId}";
                            //我的客户被点价
                            if (!DictIntention.ContainsKey(key))
                            {
                                DictIntention.TryAdd(key, key);
                            }
                        }

                        ShowPTListBrokerNotify(new Common.View.Drp.NotificationContent() { Id = tempList[0].StrategyOrderId, ContentType = "IN", Detail = "", IsShow = !isUpdate, DelFlag = "0", Message = JsonConvert.SerializeObject(list), Title = "", Type = NotificationType.BROKER });
                    }

                    #region 全部为终态关掉我的意向弹框
                    //关闭我的客户发来意向弹窗
                    tempList = list.Where(a => ("F".Equals(a.OrdStatus) || "C".Equals(a.OrdStatus) || "E".Equals(a.OrdStatus) || "G".Equals(a.OrdStatus) || "H".Equals(a.OrdStatus)) && LocalEnv.UserInfo.IsMyself(a.PartySubID)).ToList();
                    if (tempList != null && tempList.Count > 0)
                    {
                        ShowPTListBrokerNotify(new Common.View.Drp.NotificationContent() { Id = tempList[0].StrategyOrderId, ContentType = "CIN", Detail = "", IsShow = true, DelFlag = "1", Message = JsonConvert.SerializeObject(list), Title = "", Type = NotificationType.BROKER });
                    }
                    #endregion

                    #endregion

                    #region 如果是线下成交，不展示
                    if (list.Count(p => p.OrdStatus == "I") == list.Count)
                        return;
                    #endregion

                    #region 收到点价反馈

                    //收到点价反馈 一个窗口有多个,只有收到点价信息时，才弹出，否则不弹出
                    //FeedBackAUI    tempList:"F", "H", "E" 

                    //string s = FileHelper.ReadFullText("d:\\1.txt");
                    //list = new List<ExecutionReport>() { CommonUtils.DeserializeObject<ExecutionReport>(s) };

                    int normalcnt = list.Where(p => p.CommandType == "34" && LocalEnv.UserInfo.IsMyself(p.PartySubID)).Count();
                    int totalcnt = list.Where(p => LocalEnv.UserInfo.IsMyself(p.PartySubID)).Count();
                    //如果是自己点价,则显示我的客户被点价
                    tempList = list.Where(a => !"I".Equals(a.OrdStatus) && !"C".Equals(a.OrdStatus) && !"D".Equals(a.OrdStatus) && CommonUtils.ConvertToLong(a.LastQty) > 0 && LocalEnv.UserInfo.IsMyself(a.CounterPartySubID)).ToList();
                    int isselfpoint = tempList.Count;

                    if (normalcnt != totalcnt && list.Count() > 0)
                    {
                        //判断是否是仅仅是BrokerA点价，还没有BrokerB回复
                        if (!(list.Count(p => p.OrdStatus == "2" && p.OrdStatus != "I" && p.OrdStatus != "C" && p.OrdStatus != "D") == list.Count))
                        {
                            //如果拒绝或用户直接删除订单要显示点价反馈窗口  
                            if (list.Count(o => o.OrdStatus == "F" || o.OrdStatus == "C") == list.Count)
                            {
                                ShowPTListBrokerNotify(new Common.View.Drp.NotificationContent() { Id = $"{list[0].StrategyOrderId}", ContentType = "FB", Detail = "", IsShow = true, DelFlag = "0", Message = JsonConvert.SerializeObject(list), Title = "", Type = NotificationType.BROKER });
                                if (isselfpoint == 0)
                                    return;
                            }

                            //如果 DelFlag = "1" OrdStatus = "C" 不弹出
                            if (list.Count(o => o.DelFlag == "1" || o.OrdStatus == "F") == list.Count && list.Count == 1)
                            {
                                if (isselfpoint == 0)
                                    return;
                            }

                            //如果删除的订单有多个关联订单，判断是否打开窗口，如果没有打开则不显示，如果打开则显示已删除
                            if (list.Count(o => o.DelFlag == "1" || o.OrdStatus == "F") == list.Count && list.Count > 1)
                            {
                                if (isselfpoint == 0)
                                {
                                    ShowPTListBrokerNotify(new Common.View.Drp.NotificationContent() { Id = $"{list[0].StrategyOrderId}", ContentType = "FB", Detail = "", IsShow = false, DelFlag = "0", Message = JsonConvert.SerializeObject(list), Title = "", Type = NotificationType.BROKER });
                                    return;
                                }
                            }

                            ShowPTListBrokerNotify(new Common.View.Drp.NotificationContent() { Id = $"{list[0].StrategyOrderId}", ContentType = "FB", Detail = "", IsShow = true, DelFlag = "0", Message = JsonConvert.SerializeObject(list), Title = "", Type = NotificationType.BROKER });
                            if (isselfpoint == 0)
                                return;
                        }
                    }

                    #endregion


                    #region 被点价
                    //我的客户被点价
                    //IntentionBUI
                    tempList = list.Where(a => !"I".Equals(a.OrdStatus) && !"C".Equals(a.OrdStatus) && !"D".Equals(a.OrdStatus) && CommonUtils.ConvertToLong(a.LastQty) > 0 && LocalEnv.UserInfo.IsMyself(a.CounterPartySubID)).ToList();
                    if (tempList != null && tempList.Count() > 0)
                    {
                        //增删改桥不弹出窗口
                        if (list.Count(a => string.IsNullOrEmpty(a.CommandType) || a.CommandType == "6" || a.CommandType == "7" || a.CommandType == "10" || a.CommandType == "30" || a.CommandType == "31" || a.CommandType == "31" || a.CommandType == "33" || a.CommandType == "35") > 0)
                            return;

                        ShowPTListBrokerNotify(new Common.View.Drp.NotificationContent() { Id = tempList[0].StrategyOrderId, ContentType = "INB", Detail = "", IsShow = true, DelFlag = "0", Message = JsonConvert.SerializeObject(tempList), Title = "", Type = NotificationType.BROKER });
                    }
                    #endregion
                }
                catch (Exception ex)
                {
                    LogHelper.Info($"ProcPTListBrokerMsg Failed:{ex.Message}");
                }
            }
        }

        /// <summary>
        /// 获取BrokerAStatus 或 BrokerBStatus , 默认获取BrokerBStatus
        /// </summary>
        /// <param name="er"></param>
        /// <param name="isGetBrokerB">true</param>
        /// <returns>状态 0可操作确认 1待确认，2部分确认，3拒绝，4全部确认</returns>
        private string GetBrokerABStatus(ExecutionReport er, bool isGetBrokerB = true)
        {
            /// BrokerBStatus状态 :0可操作确认 1待确认，2部分确认，3拒绝，4全部确认
            bool IsPartlyConfirm = !string.IsNullOrWhiteSpace(er.MatchQTY) && !string.IsNullOrWhiteSpace(er.OrderQty) && er.MatchQTY != er.OrderQty;
            string BrokerAStatus = "0";
            string BrokerBStatus = "0";
            switch (er.OrdStatus)
            {
                case "0":
                case "1":
                case "2":
                    BrokerAStatus = "1";
                    BrokerBStatus = LocalEnv.UserInfo.IsMyself(er.CounterPartySubID) ? "0" : "1";
                    break;
                case "I":

                    BrokerAStatus = "4";
                    BrokerBStatus = !LocalEnv.UserInfo.IsMyself(er.CounterPartySubID) ? "1" : "0";
                    break;
                case "E":
                    BrokerAStatus = LocalEnv.UserInfo.IsMyself(er.PartySubID) ? "0" : "1";
                    BrokerBStatus = IsPartlyConfirm ? "2" : "4";
                    break;
                case "H":
                    BrokerAStatus = "3";
                    BrokerBStatus = IsPartlyConfirm ? "2" : "4";
                    break;
                case "F":
                    BrokerAStatus = "4";
                    BrokerBStatus = "3";
                    break;
                case "G":
                    BrokerAStatus = "4";
                    BrokerBStatus = IsPartlyConfirm ? "2" : "4";
                    break;
            }
            
            return isGetBrokerB ? BrokerBStatus : BrokerAStatus;
        }
    }
}
