using com.gateway.log;
using Newtonsoft.Json;
using STC.Common.Data;
using STC.Common.Handler.Admin;
using STC.Common.Handler.Drp;
using STC.Common.Handler.TBook;
using STC.Common.Model.Admin;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.PT;
using STC.Common.Utils;
using STC.Common.View.Drp.Component.Broker;
using STC.Common.View.UI;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace STC.Common.Handler.PT
{
    public class TransactionPopupHandler
    {
        private static TransactionPopupHandler transHandler = new TransactionPopupHandler();
        public static TransactionPopupHandler Instance { get { return transHandler; } }

        /// <summary>
        /// 缓存我的订单(订阅)
        /// </summary>
        private ConcurrentDictionary<string, TTbookSystemMessage> _CacheGRMsg = new ConcurrentDictionary<string, TTbookSystemMessage>();
        
        /// <summary>
        /// 处理我的订单消息的线程队列
        /// </summary>
        private ConcurrentQueue<PopupERCollect> ERQueue = new ConcurrentQueue<PopupERCollect>();
        private ConcurrentQueue<ExecutionReport> GrERQueue = new ConcurrentQueue<ExecutionReport>();
        private ConcurrentQueue<List<TTbookSystemMessage>> MsgERQueue = new ConcurrentQueue<List<TTbookSystemMessage>>();

        private AutoResetEvent _autoResetEvent = new AutoResetEvent(false);
        private object _queueLock = new object();
        private object _grqueueLock = new object();
        private object _msgqueueLock = new object();

        private object _dictLock = new object();
        private object _grLock = new object(); 
        private Thread RefreshDataThread = null;
        private Thread RefreshGrOrderThread = null;
        private AutoResetEvent _grautoResetEvent = new AutoResetEvent(false);
        private AutoResetEvent _msgautoResetEvent = new AutoResetEvent(false);
        private Thread RefreshAdminMsgThread = null;
        public void loadData()
        {
            Task.Factory.StartNew(() =>
            {
                if (RefreshDataThread == null)
                {
                    RefreshDataThread = new Thread(RefreshER);
                    RefreshDataThread.IsBackground = true;
                    RefreshDataThread.Start();
                }

                if (RefreshGrOrderThread == null)
                {
                    RefreshGrOrderThread = new Thread(RefreshGrOrder);
                    RefreshGrOrderThread.IsBackground = true;
                    RefreshGrOrderThread.Start();
                }


                if (RefreshAdminMsgThread == null)
                {
                    RefreshAdminMsgThread = new Thread(RefreshAdminMsg);
                    RefreshAdminMsgThread.IsBackground = true;
                    RefreshAdminMsgThread.Start();
                }
                

                AdminHandler.Instance.Init(OnAdminMsg);

                //订阅TradeBook订单状态消息处理GR相关弹窗
                TBookHandler.Instance.Init(Instance_OnOrder, null);

                //订阅GR订单
                GRHandler.Instance.SubscribeGrExec(OnSubscribeGrExecOrder);
            });
        }

        public bool StopRefreshER = false;

        public TransactionPopupHandler()
        {

        }

        /// <summary>
        /// 更新回调订单
        /// </summary>
        /// <param name="er"></param>
        private void OnSubscribeGrExecOrder(List<ExecutionReport> lster)
        {
            if (lster == null)
                return;

            try
            {
                lock (_grLock)
                {
 
                    foreach (ExecutionReport er in lster)
                    {
                        GrERQueue.Enqueue(er);
                        _grautoResetEvent.Set();
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex.Message);
            }
        }


        private void RefreshGrOrder()
        {
            while (true)
            {
                try
                {
                    if (StopRefreshER)
                    {
                        break;
                    }

                    ExecutionReport er = null;

                    lock (_grqueueLock)
                    {
                        if (GrERQueue.TryDequeue(out er))
                        {
                            procGrOrder(er);

                            while (GrERQueue.Count > 0)
                            {
                                er = null;
                                GrERQueue.TryDequeue(out er);
                                procGrOrder(er);
                            }
                        }
                    }

                    if (GrERQueue.Count == 0)
                        _grautoResetEvent.WaitOne();
                }
                catch (ThreadAbortException ex)
                {
                    LogHelper.Debug("RefreshER Normal Abort:", ex);
                }
                catch (Exception ex)
                {
                    LogHelper.Error("RefreshER exception:", ex);
                }
            }
        }

        private void procGrOrder(ExecutionReport er)
        {
            ThreadPoolHelper.InvokeThreadPool((o) =>
            {
                try
                {
                    if (er.CommandType == "0" || (er.ParamCloseById ?? string.Empty) != LocalEnv.UserInfo.UserId || (er.BrokerUnread ?? string.Empty).Split(',').Contains(LocalEnv.UserInfo.UserId))
                    {
                        if (er.CommandType == "34")
                        {
                            return;
                        }

                        if ((er.ParamCloseById ?? string.Empty) == LocalEnv.UserInfo.UserId)
                        {
                            return;
                        }

                        List<ExecutionReport> list = new List<ExecutionReport>();
                        if (string.IsNullOrEmpty(er.BlockNum))
                        {
                            list.Add(er);
                        }
                        else
                        {
                            lock (_dictLock)
                            {
                                if (DrpHandler.Instance.DictGROrders.ContainsKey(er.BlockNum))
                                {
                                    if (DrpHandler.Instance.DictGROrders.TryGetValue(er.BlockNum, out list))
                                    {
                                        if (list.Any(_ => _.ExecID == er.ExecID))
                                        {
                                            int indexof = list.IndexOf(list.Where(_ => _.ExecID == er.ExecID).FirstOrDefault());
                                            list[indexof] = er;
                                        }
                                        else
                                        {
                                            list.Add(er);
                                        }

                                        DrpHandler.Instance.DictGROrders[er.BlockNum] = list;
                                    }
                                }
                                else
                                {
                                    DrpHandler.Instance.DictGROrders.TryAdd(er.BlockNum, new List<ExecutionReport>() { er });
                                }

                                DrpHandler.Instance.DictGROrders.TryGetValue(er.BlockNum, out list);
                            }
                        }

                        if (list.Count > 0)
                        {
                            PopupERCollect popupERCollect = new PopupERCollect();
                            popupERCollect.list.AddRange(list);
                            popupERCollect.isUpdate = true;

                            InsertEnqueue(popupERCollect);
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.Error("procGrOrder exception:", ex);
                }
            });
        }

        private void RefreshER()
        {
            while (true)
            {
                try
                {
                    if (StopRefreshER)
                    {
                        break;
                    }

                    PopupERCollect collect = null;

                    lock (_queueLock)
                    {
                        if (ERQueue.TryDequeue(out collect))
                        {
                            DrpHandler.Instance.ProcPTListBrokerMsg(collect.list, collect.isUpdate, collect.isFromHistory, collect.isFromTbook);

                            while (ERQueue.Count > 0)
                            {
                                collect = null;
                                ERQueue.TryDequeue(out collect);
                                DrpHandler.Instance.ProcPTListBrokerMsg(collect.list, collect.isUpdate, collect.isFromHistory, collect.isFromTbook);
                            }
                        }
                    }

                    if (ERQueue.Count == 0)
                        _autoResetEvent.WaitOne();
                }
                catch (ThreadAbortException ex)
                {
                    LogHelper.Debug("RefreshER Normal Abort:", ex);
                }
                catch (Exception ex)
                {
                    LogHelper.Error("RefreshER exception:", ex);
                }
            }
        }

        /// <summary>
        /// 系统消息处理线程
        /// </summary>
        private void RefreshAdminMsg()
        {
            while (true)
            {
                try
                {
                    if (StopRefreshER)
                    {
                        break;
                    }

                    List<TTbookSystemMessage> er = null;

                    lock (_msgqueueLock)
                    {
                        if (MsgERQueue.TryDequeue(out er))
                        {
                            Process(er);

                            while (MsgERQueue.Count > 0)
                            {
                                er = null;
                                MsgERQueue.TryDequeue(out er);
                                Process(er);
                            }
                        }
                    }

                    if (MsgERQueue.Count == 0)
                        _msgautoResetEvent.WaitOne();
                }
                catch (ThreadAbortException ex)
                {
                    LogHelper.Debug("RefreshAdminMsg Normal Abort:", ex);
                }
                catch (Exception ex)
                {
                    LogHelper.Error("RefreshAdminMsg exception:", ex);
                }
            }
        }


        private void Process(List<TTbookSystemMessage> sysMsg)
        {
            if (sysMsg != null && sysMsg.Count > 0)
            {
                foreach (TTbookSystemMessage message in sysMsg)
                {
                    if ("3".Equals(message.type))
                    {
                        BrokerSendQQReq req = JsonConvert.DeserializeObject<BrokerSendQQReq>(message.content);
                        if ("0".Equals(message.delflag))
                        {
                            if (req.Content.Count > 0)
                            {
                                PResult result = JsonConvert.DeserializeObject<PResult>(req.Content[0].Text);
                                string errorMsg = result.Msg;
                                if (!string.IsNullOrEmpty(errorMsg))
                                {
                                    Task.Factory.StartNew(() => 
                                    { 
                                        Application.Current.Dispatcher.BeginInvoke((Action)(async () =>
                                        {
                                            BrokerMessageBoxGUI msgBox = new BrokerMessageBoxGUI();
                                            msgBox.setWarnMsg(errorMsg, false);
                                            //msgBox.Owner = Application.Current.MainWindow;
                                            msgBox.Topmost = true;
                                            msgBox.Show();
                                        }));
                                    });
                                }
                            }
                        }
                    }
                }
            }
        }


        public void OnAdminMsg(List<TTbookSystemMessage> sysMsg)
        {
            lock (_msgqueueLock)
            {
                MsgERQueue.Enqueue(sysMsg);
                _msgautoResetEvent.Set();
            }
        }

        /// <summary>
        /// 更新我的订单数据
        /// </summary>
        /// <param name="obj"></param>
        private void Instance_OnOrder(Common.Model.Fix.Message.ExecutionReport obj)
        {
            ThreadPoolHelper.InvokeThreadPool((o) =>
            {
                Application.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Send, (Action)(() =>
                {
                    if (obj != null)
                    {
                        //经纪商消息弹窗
                        if ("BROKER".Equals(obj.AlgoName) && (obj.OrdStatus.Equals("N") || obj.OrdStatus.Equals("2") || obj.OrdStatus.Equals("4") || obj.OrdStatus.Equals("8")))
                        {
                            if (obj.OrdStatus.Equals("N"))
                            {
                                if (!DrpHandler.Instance.DictTBookOrders.ContainsKey(obj.OrderID))
                                {
                                    DrpHandler.Instance.DictTBookOrders.Add(obj.OrderID, obj);
                                }
                            }
                            else if (obj.OrdStatus.Equals("2"))
                            {
                                if (!DrpHandler.Instance.DictTBookOrders.ContainsKey(obj.OrderID))
                                {
                                    DrpHandler.Instance.DictTBookOrders.Add(obj.OrderID, obj);
                                }
                                else
                                {
                                    DrpHandler.Instance.DictTBookOrders[obj.OrderID].Info2 = obj.Info2;
                                }
                            }
                            else
                            {
                                if ((obj.TimeInForce ?? "0") == "3")
                                {
                                    //只有撤单弹窗
                                    PopupERCollect popupERCollect = new PopupERCollect();
                                    popupERCollect.list = new List<ExecutionReport>() { obj };
                                    popupERCollect.isUpdate = false;
                                    popupERCollect.isFromTbook = true;
                                    popupERCollect.isFromHistory = false;
                                    InsertEnqueue(popupERCollect);
                                }
                            }
                        }
                    }
                }));
            });
        }

        public void InsertEnqueue(PopupERCollect list)
        {
            try
            {
                lock (_queueLock)
                {
                    ERQueue.Enqueue(list);
                    _autoResetEvent.Set();
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex.Message);
            }
        }

    }
}
