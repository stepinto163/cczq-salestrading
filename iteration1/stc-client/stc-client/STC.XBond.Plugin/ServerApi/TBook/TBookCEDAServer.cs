using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using STC.Frame.Utils;
using Ats.Foundation.Message;
using STC.XBond.Plugin.Model;
using STC.Common.Model.Fix.Message;

namespace STC.XBond.Plugin.ServerApi.TBook
{
    public class TBookCEDAServer : BaseCEDAServer, ITBookCEDAServer
    {
        public event Action<ExecutionReport> OnOrder;

        private const string topic_stc_tbook_xbond = "stc.tbook.mm.xbond";
        private const string topic_stc_tbook_order = "stc.tbook.s.**";
        private const string topic_stc_tbook_prefix = "stc.tbook.s";
        private const string topic_stc_tbook_cancel = "stc.tbook.o.c";
        private const string topic_stc_tbook_placeOrder = "stc.tbook.o.n";

        private TBookCEDAServer() { }
        private static ITBookCEDAServer server = new TBookCEDAServer();
        public static ITBookCEDAServer Instance { get { return server; } }


        private string SvrId { get { return "TBookSvr"; } }

        public void SubScribeWithImage(string[] topics)
        {
            SubScribeWithImage(topics, SvrId, LocalEnv.UserInfo.SessionID);
        }

        public void UnSubScribe(string[] topics)
        {
            UnSubScribe(SvrId, topics);
        }

        public void UnsubscribAll()
        {
            UnSubScribe(new string[] { topic_stc_tbook_order });
        }

        public void SubscribeOrder()
        {
            SubScribeWithImage(new string[] { topic_stc_tbook_order });
        }

        public List<Bond> GetBond()
        {
            //string content = MsgHelper.Serializer(request);
            AtsMsg<List<Bond>> msg = RequestMessage<List<Bond>>(SvrId, topic_stc_tbook_xbond, "", "JSON");
            if (msg != null)
            {
                if (msg.Body != null)
                {
                    return msg.Body;
                }
            }
            return null;
        }

        public TBookCancelReplyPo CancelOrder(TBookCancelPo cancelPo)
        {
            string content = MsgHelper.Serializer(cancelPo);
            AtsMsg<TBookCancelReplyPo> msg = RequestMessage<TBookCancelReplyPo>(SvrId, topic_stc_tbook_cancel, content, "JSON");
            if (msg != null)
            {
                if (msg.Body != null)
                {
                    return msg.Body;
                }
            }
            return null;
        }

        public PResult PlaceOrder(NewOrderSingle newOrder)
        {
            string content = MsgHelper.Serializer(newOrder);
            AtsMsg<PResult> msg = RequestMessage<PResult>(SvrId, topic_stc_tbook_placeOrder, content, "JSON");
            if (msg != null)
            {
                if (msg.Body != null)
                {
                    return msg.Body;
                }
            }
            return null;
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
                        if (msg.Destination.StartsWith(topic_stc_tbook_prefix))
                        {
                            m.Body = MsgHelper.Deserialize<ExecutionReport>(body, Ats.Foundation.Message.Serializer.MsgSerializerType.Json);
                        }

                    }
                }
                LogHelper.Info("topic:{0}, body:{1}", msg.Destination, body);

                //Console.WriteLine("TBook topic:{0}, body:{1}", msg.Destination, body);

                if (msg.MessageBody.haseField(7) != 0)
                {
                    m.Header.SendTime = msg.MessageBody.getString(7);
                }

                if (msg.Destination.StartsWith(topic_stc_tbook_prefix))
                    OnOrder?.Invoke(m.Body as ExecutionReport);
            }
            return m;
        }

        
    }
}
