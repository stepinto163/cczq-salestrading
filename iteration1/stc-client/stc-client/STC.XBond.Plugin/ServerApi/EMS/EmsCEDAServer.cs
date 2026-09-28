using Ats.Foundation.Message;
using STC.Common.Model.Fix.Message;
using STC.Frame.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.XBond.Plugin.ServerApi.EMS
{
    public class EmsCEDAServer : BaseCEDAServer, IEmsCEDAServer
    {
        //服务
        private string SvrId { get { return "EMSSvr"; } }

        // 请求
        private const string topic_stc_ems_placeorder = "stc.ems.o.n";
        private const string topic_stc_ems_cancelorder = "stc.ems.o.c";

        //// 订阅
        private const string topic_stc_ems_executionreport = "stc.ems.e.**";
        private string[] stc_ems_executionreport_topics;
        private const string topic_stc_ems_cancelreject = "stc.ems.c";
        private string[] stc_ems_cancelreject_topics;
        private const string topic_stc_ems_constatus = "stc.ems.t.*";
        private string[] stc_ems_constatu_topics;

        private const string topic_stc_ems_executionreport_cdh = "stc.ems.e.cdh.**";
        private const string topic_stc_ems_executionreport_own = "stc.ems.e.odm.**";

        public event Action<ExecutionReport> OnExecuteOrder;
        public event Action<ExecutionReport> OnOwnExecuteOrder;

        //public event Action<List<SessionStatus>> OnEMSConStatus;
        //public event Action<ExecutionReport> OnEMSExecutionReport;
        //public event Action<OrderCancelReject> OnEMSCancelReject;

        private EmsCEDAServer() { }
        private static IEmsCEDAServer server = new EmsCEDAServer();
        public static IEmsCEDAServer Instance { get { return server; } }

        public string Topic_stc_ems_placeorder
        {
            get
            {
                return topic_stc_ems_placeorder;
            }
        }

        public string Topic_stc_ems_cancelorder
        {
            get
            {
                return topic_stc_ems_cancelorder;
            }
        }

        public string Topic_stc_ems_executionreport
        {
            get
            {
                return topic_stc_ems_executionreport;
            }
        }

        public string Topic_stc_ems_cancelreject
        {
            get
            {
                return topic_stc_ems_cancelreject;
            }
        }

        public string Topic_stc_ems_constatus
        {
            get
            {
                return topic_stc_ems_constatus;
            }
        }

        //public void SubScribe<T>(string[] topics, Action<T> msgCallback)
        //{
        //    SubScribe<T>(SvrId, topics, msgCallback);
        //}

        //public void SubScribeWithImage<T>(string[] topics, Action<T> msgCallback)
        //{
        //    SubScribeWithImage<T>(topics, SvrId, msgCallback);
        //}

        //public void UnSubScribePricing<T>(string[] topics)
        //{
        //    UnSubScribe<T>(SvrId, topics);
        //}

        public PResult PlaceOrderForEMS(NewOrderSingle order)
        {
            string content = MsgHelper.Serializer(order);
            AtsMsg<PResult> msg = RequestMessage<PResult>(SvrId, topic_stc_ems_placeorder, content, "JSON");
            if (msg != null)
            {
                if (msg.Body != null)
                {
                    return msg.Body;
                }
            }
            return null;
        }

        public PResult CancelOrderForEMS(OrderCancelRequest orderCancelRequset)
        {
            string content = MsgHelper.Serializer(orderCancelRequset);
            AtsMsg<PResult> msg = RequestMessage<PResult>(SvrId, topic_stc_ems_cancelorder, content, "JSON");
            if (msg != null)
            {
                if (msg.Body != null)
                {
                    return msg.Body;
                }
            }
            return null;
        }

        /// <summary>
        /// 成交订单通知
        /// stc.ems.e.x1.x2,	其中x1为对应venueGW,x2为债券代码（统一为代码+IB/SH/SE)：如
        /// stc.ems.e.sor.190205.IB
        /// stc.ems.e.cdh.190205.IB
        /// 如果想接受所有成交信息，包括CDH的成交行情，可使用stc.ems.e.**，统配订阅
        /// </summary>
        /// <param name="topics"></param>
        /// <param name="msgCallback"></param>
        public void SubscribeForEMSExecutionReport(Action<string, ExecutionReport> msgCallback)
        {
            if (stc_ems_executionreport_topics == null)
            {
                stc_ems_executionreport_topics = new string[] { topic_stc_ems_executionreport };
            }
            //this.Subscribe(this.SvrId, stc_ems_executionreport_topics, msgCallback);
        }

        /// <summary>
        /// 撤销拒绝通知
        /// topic:stc.ems.c
        /// </summary>
        /// <param name="msgCallback">消息回调方法</param>
        public void SubscribeForEMSCancelReject(Action<string, OrderCancelReject> msgCallback)
        {
            if (stc_ems_cancelreject_topics == null)
            {
                stc_ems_cancelreject_topics = new string[] { this.Topic_stc_ems_cancelreject };
            }
            //this.Subscribe<OrderCancelReject>(this.SvrId, stc_ems_cancelreject_topics, msgCallback);
        }

        /// <summary>
        /// 连接状态通知
        /// stc.ems.t.x1_TRADE, 其中x1为对应venueGW
        /// stc.ems.t.sor_TRADE
        /// 订阅所有，可使用stc.ems.t.*
        /// </summary>
        /// <param name="topics">主题</param>
        /// <param name="msgCallback">消息回调方法</param>
        public void SubscribeForEMSConStatus(Action<string, SessionStatus> msgCallback)
        {
            if (stc_ems_constatu_topics == null)
            {
                stc_ems_constatu_topics = new string[] { topic_stc_ems_constatus };
            }
            //this.SubscribeWithImage<SessionStatus>(this.SvrId, stc_ems_constatu_topics, msgCallback);
        }

        public void Init(string sessionID)
        {
            throw new NotImplementedException();
        }

        public void UnsubscribAll()
        {
            UnSubScribe(new string[] { topic_stc_ems_executionreport_cdh });
            UnSubScribe(new string[] { topic_stc_ems_executionreport_own });
        }

        public void SubScribeWithImage(string[] topics)
        {
            SubScribeWithImage(topics, SvrId, LocalEnv.UserInfo.SessionID);
        }

        public void UnSubScribe(string[] topics)
        {
            UnSubScribe(SvrId, topics);
        }

        public void SubscribeEMSExceptionReport()
        {
            SubScribeWithImage(new string[] { topic_stc_ems_executionreport_cdh });
        }

        public void SubscribeEMSOwnExecuteOrder()
        {
            SubScribeWithImage(new string[] { topic_stc_ems_executionreport_own });
        }


        public override AtsMsg<object> CEDAToAtsMsg(com.adaptiveMQ2.message.Message msg)
        {
            AtsMsg<object> m = AtsMsgHelper.CreateAtsMessage<object>();
            if (msg != null)
            {
                
                string body = "";
                bool isZip = false;
                string msgType = "JSON";
                //if (msg.MessageBody.haseField(4) != 0)
                //{
                //    msgType = msg.MessageBody.getString(4);
                //}
                if (msg.MessageBody.haseField(5) != 0)
                {
                    isZip = msg.MessageBody.getInt(5) == 1; //后台数据是否压缩
                }
                if (msg.MessageBody.haseField(3) != 0)
                {
                    if (isZip)
                    {
                        body = msg.MessageBody.getGZIPString(3); //压缩
                    }
                    else
                    {
                        body = msg.MessageBody.getString(3); //不压缩
                    }
                }
                if (!string.IsNullOrEmpty(body))
                {
                    if (msgType == "JSON" || msgType == "JSONLIST")
                    {
                        if (msg.Destination.StartsWith(topic_stc_ems_executionreport_cdh.Trim('*'))
                            || msg.Destination.StartsWith(topic_stc_ems_executionreport_own.Trim('*')))
                        {
                            m.Body = MsgHelper.Deserialize<ExecutionReport>(body, Ats.Foundation.Message.Serializer.MsgSerializerType.Json);
                        }
                        //else if (msg.Destination.StartsWith(Topic_Prefix_Order))
                        //{
                        //    m.Body = MsgHelper.Deserialize<OrderPo>(body, Ats.Foundation.Message.Serializer.MsgSerializerType.Json);
                        //}
                        //else if (msg.Destination.Equals(Topic_OpenClose))
                        //{
                        //    m.Body = MsgHelper.Deserialize<OpenClosePo>(body, Ats.Foundation.Message.Serializer.MsgSerializerType.Json);
                        //}
                        //else if (msg.Destination.StartsWith(Topic_Prefix_SysMsg))
                        //{
                        //    m.Body = MsgHelper.Deserialize<SysMsgPo>(body, Ats.Foundation.Message.Serializer.MsgSerializerType.Json);
                        //}
                        //else if (msg.Destination.StartsWith(Topic_Prefix_SystemParameter))
                        //{
                        //    m.Body = MsgHelper.Deserialize<SystemParameter>(body, Ats.Foundation.Message.Serializer.MsgSerializerType.Json);
                        //}
                    }
                    //else if (msgType == "ATSFEED")
                    //{
                    //    m.Body = MsgHelper.Deserialize<GoodsPricePo>(body,
                    //        Ats.Foundation.Message.Serializer.MsgSerializerType.ATSFeed);
                    //}
                }
                LogHelper.Info("topic:{0}, body:{1}", msg.Destination, body);

                Console.WriteLine("Ems topic:{0}, body:{1}", msg.Destination, body);

                if (msg.MessageBody.haseField(7) != 0)
                {
                    m.Header.SendTime = msg.MessageBody.getString(7);
                }
            }
            if (msg.Destination.StartsWith(topic_stc_ems_executionreport_cdh.Trim('*')))
            {
                OnExecuteOrder?.Invoke(m.Body as ExecutionReport);
            }
            else if (msg.Destination.StartsWith(topic_stc_ems_executionreport_own.Trim('*')))
            {
                OnOwnExecuteOrder?.Invoke(m.Body as ExecutionReport);
            }
            //else if (msg.Destination.Equals(Topic_OpenClose))
            //{
            //    OnOpenClose(m.Body as OpenClosePo);
            //}
            //else if (msg.Destination.StartsWith(Topic_Prefix_SysMsg))
            //{
            //    OnSysMsg(m.Body as SysMsgPo);
            //}
            //else if (msg.Destination.StartsWith(Topic_Prefix_SystemParameter))
            //{
            //    OnSystemParameter(m.Body as SystemParameter);
            //}


            return m;
        }
    }
}
