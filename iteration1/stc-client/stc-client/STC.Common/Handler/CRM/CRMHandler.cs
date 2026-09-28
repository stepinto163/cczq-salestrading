using netapi.Api;
using STC.Common.Api;
using STC.Common.Dispatcher;
using STC.Common.Model.CRM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Handler.CRM
{
    public class CRMHandler : ICRMHandler
    {
        private CRMHandler() { }
        private static ICRMHandler crmHandler = new CRMHandler();

        //Crm交易员
        private event Action<CrmContactReplyModel> OnCrmContactMsg;
        //Crm机构
        private event Action<CrmInstReplyModel> OnCrmInstMsg;
        //Crm账号
        private event Action<CrmAccountReplyModel> OnCrmAccountMsg;
        public void SubscribeCrmContact(Action<CrmContactReplyModel> klineCallBack)
        {
            if ((OnCrmContactMsg != null && !OnCrmContactMsg.GetInvocationList().Contains(klineCallBack)) || OnCrmContactMsg == null)
                OnCrmContactMsg += klineCallBack;

            System.Threading.Tasks.Task.Factory.StartNew(() =>
            {
                string topic = string.Format(ApiConsts.TOPIC_CRM_CONTACTNOTIFY);
                ApiManager.Instance.GetDefaultApi().SubScribeWithImage(DataBus.Instance.Instanse_OnMessage,
                    new string[] { topic }, ApiConsts.CRMSvr, "");
            });
        }


        public void SubscribeCrmInstMsg(Action<CrmInstReplyModel> klineCallBack)
        {
            if ((OnCrmInstMsg != null && !OnCrmInstMsg.GetInvocationList().Contains(klineCallBack)) || OnCrmInstMsg == null)
                OnCrmInstMsg += klineCallBack;

            System.Threading.Tasks.Task.Factory.StartNew(() =>
            {
                string topic = string.Format(ApiConsts.TOPIC_CRM_INSTNOTIFY);
                ApiManager.Instance.GetDefaultApi().SubScribeWithImage(DataBus.Instance.Instanse_OnMessage,
                    new string[] { topic }, ApiConsts.CRMSvr, "");
            });
        }

        public void SubscribeOnCrmAccountMsg(Action<CrmAccountReplyModel> klineCallBack)
        {
            if ((OnCrmAccountMsg != null && !OnCrmAccountMsg.GetInvocationList().Contains(klineCallBack)) || OnCrmAccountMsg == null)
                OnCrmAccountMsg += klineCallBack;

            System.Threading.Tasks.Task.Factory.StartNew(() =>
            {
                string topic = string.Format(ApiConsts.TOPIC_CRM_ACCOUNTNOTIFY);
                ApiManager.Instance.GetDefaultApi().SubScribeWithImage(DataBus.Instance.Instanse_OnMessage,
                    new string[] { topic }, ApiConsts.CRMSvr, "");
            });
        }


        public void ProcessCrmContactReplyModel(NetMessage netMsg)
        {
            if (netMsg != null)
            {
                //Crm账号
                if (netMsg.Topic.StartsWith(ApiConsts.TOPIC_CRM_ACCOUNTNOTIFY))
                {
                    CrmAccountReplyModel crm = STC.Common.Utils.CommonUtils.DeserializeObject<CrmAccountReplyModel>(netMsg.Data.ToString());
                    OnCrmAccountMsg?.Invoke(crm);
                }
                //Crm交易员
                if (netMsg.Topic.StartsWith(ApiConsts.TOPIC_CRM_CONTACTNOTIFY))
                {
                    CrmContactReplyModel crm = STC.Common.Utils.CommonUtils.DeserializeObject<CrmContactReplyModel>(netMsg.Data.ToString());
                    OnCrmContactMsg.Invoke(crm);
                }
                //Crm机构
                if (netMsg.Topic.StartsWith(ApiConsts.TOPIC_CRM_INSTNOTIFY))
                {
                    CrmInstReplyModel crm = STC.Common.Utils.CommonUtils.DeserializeObject<CrmInstReplyModel>(netMsg.Data.ToString());
                    OnCrmInstMsg.Invoke(crm);
                }


            }
        }

        public static ICRMHandler Instance { get { return crmHandler; } }

        public CrmContactReply QueryContact(CRMRequestParamPo request)
        {
            return ApiManager.Instance.GetDefaultApi().Request<CrmContactReply>(ApiConsts.CRMSvr, ApiConsts.TOPIC_CRM_CONTACT, request);
        }

        public CrmInstReply QueryInst(CRMRequestParamPo request)
        {
            return ApiManager.Instance.GetDefaultApi().Request<CrmInstReply>(ApiConsts.CRMSvr, ApiConsts.TOPIC_CRM_INST, request);
        }

        public BrokerSearchReplyPo QueryBroker(CRMRequestParamPo request)
        {
            return ApiManager.Instance.GetDefaultApi().Request<BrokerSearchReplyPo>(ApiConsts.CRMSvr, ApiConsts.TOPIC_BROKER_SEARCH, request);
        }

    }


    public class CrmContactReplyModel
    {
        public string Code { get; set; }
        public string Msg { get; set; }
        public bool success { get; set; }

        /// <summary>
        /// A D U
        /// </summary>
        public string eventType { get; set; }
        public CrmContact body { get; set; }
    }

    public class CrmInstReplyModel
    {
        public string Code { get; set; }
        public string Msg { get; set; }
        public bool success { get; set; }
        /// <summary>
        /// A D U
        /// </summary>
        public string eventType { get; set; }
        public CrmInst body { get; set; }
    }

    public class CrmAccountReplyModel
    {
        public string Code { get; set; }
        public string Msg { get; set; }
        public bool success { get; set; }

        /// <summary>
        /// A D U
        /// </summary>
        public string eventType { get; set; }
        public BrokerInfoPo body { get; set; }
    }
}
