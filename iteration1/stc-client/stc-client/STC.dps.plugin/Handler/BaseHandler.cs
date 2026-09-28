using com.gateway.connector;
using com.gateway.connector.tcp;
using com.gateway.log;
using Newtonsoft.Json;
using STC.Common.Model.Base;
using STC.dps.plugin.Common;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static dps.plugin.GWPlugin;

namespace STC.dps.plugin.Handler
{
    public class BaseHandler : IBaseHandler
    {
        private BaseHandler() { }
        private static IBaseHandler baseHandler = new BaseHandler();
        public static IBaseHandler Instance { get { return baseHandler; } }
        public GateWayApiWraper gww { get; set; }
        public string UserId { get; set; }
        private ConcurrentDictionary<string, BrokerAccount> _CacheBroker = new ConcurrentDictionary<string, BrokerAccount>();

        public void Init(GateWayApiWraper gww)
        {
            this.gww = gww;
            UserId = (string)gww.LoginResult["userId"];
            gww.GateWayApi.subscribe(Constants.Topic_stc_base_mqAccount, new BrokerMessage(gww));
            queryAccount();
        }


        public BrokerAccount IsContainBroker(string Venue, string SecurityType)
        {
            BrokerAccount broker = null;
            if (string.IsNullOrEmpty(SecurityType))
                SecurityType = "利率债";
            string key = string.Format("{0}-{1}", getVenueId(Venue), getTp(SecurityType));
            if (_CacheBroker.ContainsKey(key))
                broker = _CacheBroker[key];
            return broker;
        }

        private void queryAccount()
        {
            BrokerAccountReq req = new BrokerAccountReq();
            req.userId = UserId;
            string content = JsonConvert.SerializeObject(req);
            BrokerAccountReplyPo reply = gww.GateWayApi.requestSync<BrokerAccountReplyPo>(Constants.BaseSvr, Constants.Topic_stc_base_queryAccount, content);
            if (reply != null && reply.Datas != null)
            {
                LogHelper.Info("reply：" + reply.Datas.Count);
                CacheBroker(reply.Datas);
            }
        }

        public void CacheBroker(List<BrokerAccount> list)
        {
            lock (_CacheBroker)
            {
                foreach (BrokerAccount obj in list)
                {
                    string broker = string.Format("{0}-{1}", obj.orgId, getTp(obj.astParentTp));
                    _CacheBroker.AddOrUpdate(broker, obj, (key, value) => obj);
                }
            }
        }

        private string getTp(string s)
        {
            string tp = "";
            switch (s)
            {
                case "利率债":
                    tp = "BNC";
                    break;
                case "信用债":
                    tp = "BOC";
                    break;
                default:
                    tp = s;
                    break;
            }
            return tp;
        }

        private string getVenueId(string s)
        {
            string brokerid = "";
            switch (s)
            {
                case "TPSC":
                    brokerid = "1";
                    break;
                case "CNEX":
                    brokerid = "2";
                    break;
                case "CCTB":
                    brokerid = "3";
                    break;
                case "PTCN":
                    brokerid = "4";
                    break;
                case "CTTJ":
                    brokerid = "5";
                    break;
                case "UEDA":
                    brokerid = "6";
                    break;
            }
            return brokerid;
        }

        class BrokerMessage : IMessage
        {
            GateWayApiWraper gww = null;
            public BrokerMessage(GateWayApiWraper gww)
            {
                this.gww = gww;
            }
            public void onMessage(string topic, string t)
            {
                List<BrokerAccount> list = JsonConvert.DeserializeObject<List<BrokerAccount>>(t);
                if (list != null && list.Count > 0)
                {
                    LogHelper.Info("BrokerMessage onMessage:" + list.Count);
                    BaseHandler.Instance.CacheBroker(list);
                }
            }
        }
    }
}
