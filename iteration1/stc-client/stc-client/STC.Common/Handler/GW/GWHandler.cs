using Ats.Foundation.Message;
using netapi.Api;
using STC.Common.Api;
using STC.Common.Model.Fix.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Handler.GW
{
    public class GWHandler : IGWHandler
    {
        private static GWHandler gwHandler = new GWHandler();
        public static IGWHandler Instance { get { return gwHandler; } }


        private GWHandler()
        {

        }

        public PResult PubEvent(string userName, string eventName, string content, bool isLocal)
        {
            string stopic = string.Format("{0}.{1}.{2}", isLocal?"L":"S", userName, eventName);
            return this.SendForGW(stopic, content, isLocal);
        }

        public PResult SendForGW(string topic, string content, bool isLocal = false)
        {
            PResult result = new PResult();

            string serverKey = isLocal ? ApiConsts.LocalSvr : ApiConsts.GateWay;
            Dictionary<string, string> hm = new Dictionary<string, string>();
            hm.Add("topic", topic);
            hm.Add("content", content);

            result = ApiManager.Instance.GetDefaultApi().Request<PResult>(serverKey, ApiConsts.Topic_GW_Nofity, hm);

            return result;
        }

        public void SubscribeGW(string topic, Action<object> action)
        {
            ApiManager.Instance.GetDefaultApi().SubScribe(action, new string[] { topic }, ApiConsts.GateWay, "");
        }
        
    }
}
