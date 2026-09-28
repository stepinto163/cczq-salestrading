using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using STC.Frame.Utils;
using Ats.Foundation.Message;
using STC.Common.Model.Fix.Message;

namespace STC.XBond.Plugin.ServerApi.MD
{
    public class MDCEDAServer : BaseCEDAServer, IMDCEDAServer
    {
        private string SvrId { get { return "MDSvr"; } }
        private const string topic_stc_md_price_prefix = "stc.md.";

        private MDCEDAServer() { }
        private static IMDCEDAServer server = new MDCEDAServer();
        public static IMDCEDAServer Instance { get { return server; } }

        public event Action<MarketDataSnapshotFullRefresh> OnPrice;

        private string getTopic(string gw, string pair, string tenor,
            string type)
        {
            string topicPreFix = topic_stc_md_price_prefix + gw.ToLower() + ".";
            return topicPreFix + getRic(gw, pair, tenor, type);
        }

        private string getRic(string gw, string pair, string tenor,
            string type)
        {
            string ric = "";

            ric = string.Format("{0}{1}{2}={3}", pair.Replace(".", ""), tenor == null ? "" : tenor, type, gw.ToLower());
            return ric;
        }

        public void SubscribeMDPrice(string symbol)
        {
            //xbond坐市报价
            string sorTopic = getTopic("SOR", symbol, string.Empty, string.Empty);
            //xbond坐市具体债券行情走势报价
            string ctTopic = getTopic("CT", symbol, string.Empty, string.Empty);
            sorTopic = "stc.md.sor.**";
            SubScribeWithImage(new string[] { sorTopic }, SvrId, LocalEnv.UserInfo.SessionID);

            SubScribeWithImage(new string[] { ctTopic }, SvrId, LocalEnv.UserInfo.SessionID);
        }

        public void UnsubscribAll(List<string> symbols)
        {
            foreach (string symbol in symbols)
            {
                UnSubScribe(SvrId, new string[] { getTopic("CT", symbol, string.Empty, string.Empty) });
                UnSubScribe(SvrId, new string[] { getTopic("SOR", symbol, string.Empty, string.Empty) });
            }
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
                        if (msg.Destination.StartsWith(topic_stc_md_price_prefix))
                        {
                            m.Body = MsgHelper.Deserialize<MarketDataSnapshotFullRefresh>(body, Ats.Foundation.Message.Serializer.MsgSerializerType.Json);
                        }

                    }
                }
                LogHelper.Info("topic:{0}, body:{1}", msg.Destination, body);

                Console.WriteLine("MD topic:{0}, body:{1}", msg.Destination, body);

                if (msg.MessageBody.haseField(7) != 0)
                {
                    m.Header.SendTime = msg.MessageBody.getString(7);
                }

                if (msg.Destination.StartsWith(topic_stc_md_price_prefix))

                    OnPrice?.Invoke(m.Body as MarketDataSnapshotFullRefresh);
            }

            return m;
        }
    }
}
