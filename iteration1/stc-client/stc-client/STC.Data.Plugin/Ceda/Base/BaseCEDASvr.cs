using Ats.Foundation.Message;
using STC.Common.Api;
using STC.Common.Model.Base;
using STC.Data.Plugin.Model.Base;
using STC.Frame.Utils;

using System;

namespace STC.Data.Plugin.Ceda.Base
{
    public class BaseCEDASvr : BaseCEDAServer, IBaseCEDASvr
    {
        private static IBaseCEDASvr instance = new BaseCEDASvr();
        public static IBaseCEDASvr Instance { get { return instance; } }
        private BaseCEDASvr() { }

        public event Action<ClearPo> OnClear;
        private string SvrId { get { return "BaseSvr"; } }

        private const string topic_Clear = "STC.base.clear";
        public string Topic_Clear { get { return topic_Clear; } }
        private const string topic_Bonds_Search = "STC.base.search";
        private const string topic_Valuation = "STC.base.valuation";
        public string Topic_Bonds_Search { get { return topic_Bonds_Search; } }

        public void SubScribe(string[] topics)
        {
            SubScribe(SvrId, topics);
        }

        public void SubScribeWithImage(string[] topics)
        {
            SubScribeWithImage(topics, SvrId, LocalEnv.UserInfo.SessionID);
        }

        public void UnSubScribePricing(string[] topics)
        {
            UnSubScribe(SvrId, topics);
        }

        public BondsSearchReplyPo SearchBonds(BondsSearchRequestPo request)
        {
            string content = MsgHelper.Serializer(request);
            AtsMsg<BondsSearchReplyPo> msg = RequestMessage<BondsSearchReplyPo>(SvrId, topic_Bonds_Search, content, "JSON");
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
                        if (msg.Destination.Equals(Topic_Clear))
                        {
                            m.Body = MsgHelper.Deserialize<ClearPo>(body, Ats.Foundation.Message.Serializer.MsgSerializerType.Json);
                        }
                    }                    
                }
                LogHelper.Info("topic:{0}, body:{1}", msg.Destination, body);

                if (msg.MessageBody.haseField(7) != 0)
                {
                    m.Header.SendTime = msg.MessageBody.getString(7);
                }

                if (msg.Destination.Equals(Topic_Clear))
                {
                    OnClear(m.Body as ClearPo);
                }
            }

            return m;
        }


        public ValuationReplyPo Valuation(ValuationRequestPo request)
        {
            string content = MsgHelper.Serializer(request);
            AtsMsg<ValuationReplyPo> msg = RequestMessage<ValuationReplyPo>(SvrId, topic_Valuation, content, "JSON");
            if (msg != null)
            {
                if (msg.Body != null)
                {
                    return msg.Body;
                }
            }
            return null;
        }

    }
}
