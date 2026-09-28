using netapi.Api;
using STC.Common.Api;
using STC.Common.Dispatcher;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.Quant;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Handler.Quant
{
    public class QuantHandler : IQuantHandler
    {
        private static QuantHandler quantHandler = new QuantHandler();
        private bool isSubscribeForMM = false;
        private bool isSubscribeForParas = false;
        private bool isSubscribeForQuantLog = false;
        private bool isSubscribeForIndicator = false;
        public static QuantHandler Instance { get { return quantHandler; } }
        private event Action<List<TTbookQuantStrategys>> On_MM;
        private event Action<TTbookQuantParameter> On_Paras;
        private event Action<TTbookQuantLog> On_QuanLog;
        private event Action<TbookQuantIndicator> On_Indicator;
        private string UserName = ApiManager.Instance.GetDefaultApi().UserName;
        private QuantHandler()
        { }

        public void UnSubscribeMM(Action<List<TTbookQuantStrategys>> onMsgCallBack)
        {
            if (onMsgCallBack != null && On_MM != null)
                On_MM -= onMsgCallBack;
        }
        /// <summary>
        /// 订阅当前用户下所有关注组策略
        /// </summary>
        /// <param name="onMsgCallBack"></param>
        public void SubscribeMM(Action<List<TTbookQuantStrategys>> onMsgCallBack, string UserName)
        {
            this.UserName = UserName;
            if (onMsgCallBack != null && (On_MM == null || !On_MM.GetInvocationList().Contains(onMsgCallBack)))
                On_MM += onMsgCallBack;

            lock (this)
            {
                if (!isSubscribeForMM)
                {
                    string[] topic = new string[] { $"{ApiConsts.Topic_stc_Strategy}**.{UserName}" };
                    ApiManager.Instance.GetDefaultApi().SubScribeWithImage(DataBus.Instance.Instanse_OnMessage, topic
                       ,
                        // new string[] { ApiConsts.Topic_stc_Strategy },
                        ApiConsts.QuantSvr, string.Empty);
                    isSubscribeForMM = true;
                }
            }
        }
        private void OnMM(object obj)
        {
            DataBus.Instance.add(obj);
        }

        public void ProcessMM(NetMessage netMsg)
        {
            if (netMsg != null)
            {
                // string[] topic = new string[] { $"{ApiConsts.Topic_stc_Strategy}{UserName}." };
                // if (netMsg.Topic.Contains(ApiConsts.Topic_stc_Strategy) && netMsg.Topic.Contains(UserName))
                if (!string.IsNullOrWhiteSpace(this.UserName) && !"*".Equals(this.UserName))
                {
                    if (!netMsg.Topic.EndsWith(this.UserName))
                        return;
                }
                if (netMsg.Topic.StartsWith(ApiConsts.Topic_stc_Strategy))
                {
                    if (netMsg.Data.StartsWith(ApiConsts.BRACKET))
                    {
                        List<TTbookQuantStrategys> data = BaseHelper.ConvertStcMessage<List<TTbookQuantStrategys>>(new string[] { netMsg.Topic }, netMsg);
                        if (data != null && data.Count > 0)
                        {
                            On_MM?.Invoke(data);
                            //foreach (TTbookQuantStrategys mm in data)
                            //{
                            //    On_MM?.Invoke(mm);
                            //}
                        }
                    }
                    else
                    {
                        List<TTbookQuantStrategys> data = new List<TTbookQuantStrategys>();
                        TTbookQuantStrategys mm = BaseHelper.ConvertStcMessage<TTbookQuantStrategys>(new string[] { netMsg.Topic }, netMsg);
                        data.Add(mm);
                        On_MM?.Invoke(data);
                    }
                }
            }
        }

        /// <summary>
        /// 订阅退订策略下所有参数
        /// </summary>
        /// <param name="onMsgCallBack"></param>
        public void UnSubscribeParas(Action<TTbookQuantParameter> onMsgCallBack)
        {
            if (onMsgCallBack != null && On_Paras != null)
                On_Paras -= onMsgCallBack;
        }

        public void SubscribeParas(Action<TTbookQuantParameter> onMsgCallBack)
        {
            if (onMsgCallBack != null && (On_Paras == null || !On_Paras.GetInvocationList().Contains(onMsgCallBack)))
                On_Paras += onMsgCallBack;

            lock (this)
            {
                if (!isSubscribeForParas)
                {
                    //string[] topic = new string[] { $"{ApiConsts.Topic_stc_Paras}{UserName}.**" };
                    string[] topic = new string[] { $"{ApiConsts.Topic_stc_Paras}**.{UserName}" };
                    ApiManager.Instance.GetDefaultApi().SubScribeWithImage(DataBus.Instance.Instanse_OnMessage, topic, ApiConsts.QuantSvr, string.Empty);
                    isSubscribeForParas = true;
                }
            }
        }
        private void OnParas(object obj)
        {
            DataBus.Instance.add(obj);
        }

        public void ProcessParas(NetMessage netMsg)
        {
            if (netMsg != null)
            {
                //string[] topic = new string[] { $"{ApiConsts.Topic_stc_Paras}{UserName}." };
                //if (netMsg.Topic.Contains(topic[0]))
                //if (netMsg.Topic.Contains(ApiConsts.Topic_stc_Paras) && netMsg.Topic.Contains(UserName))
                if (!string.IsNullOrWhiteSpace(this.UserName) && !"*".Equals(this.UserName))
                {
                    if (!netMsg.Topic.EndsWith(this.UserName))
                        return;
                }
                if (netMsg.Topic.StartsWith(ApiConsts.Topic_stc_ParasCheck))
                    return;
                if (netMsg.Topic.StartsWith(ApiConsts.Topic_stc_Paras))
                {
                    if (netMsg.Data.StartsWith(ApiConsts.BRACKET))
                    {
                        List<TTbookQuantParameter> data = BaseHelper.ConvertStcMessage<List<TTbookQuantParameter>>(new string[] { netMsg.Topic }, netMsg);
                        if (data != null && data.Count > 0)
                        {
                            foreach (TTbookQuantParameter mm in data)
                            {
                                On_Paras?.Invoke(mm);
                            }
                        }
                    }
                    else
                    {
                        TTbookQuantParameter mm = BaseHelper.ConvertStcMessage<TTbookQuantParameter>(new string[] { netMsg.Topic }, netMsg);

                        On_Paras?.Invoke(mm);
                    }
                }

            }
        }


        /// <summary>
        /// 订阅退订策略下所有参数
        /// </summary>
        /// <param name="onMsgCallBack"></param>
        public void UnSubscribeQuantLog(Action<TTbookQuantLog> onMsgCallBack)
        {
            if (onMsgCallBack != null && On_QuanLog != null)
                On_QuanLog -= onMsgCallBack;
        }

        public void SubscribeQuantLog(Action<TTbookQuantLog> onMsgCallBack)
        {
            if (onMsgCallBack != null && (On_QuanLog == null || !On_QuanLog.GetInvocationList().Contains(onMsgCallBack)))
                On_QuanLog += onMsgCallBack;

            lock (this)
            {
                if (!isSubscribeForQuantLog)
                {
                    //string[] topic = new string[] { $"{ApiConsts.Topic_stc_QuantLog}**" };
                    string[] topic = new string[] { $"{ApiConsts.Topic_stc_QuantLog}**.{UserName}" };
                    ApiManager.Instance.GetDefaultApi().SubScribeWithImage(DataBus.Instance.Instanse_OnMessage, topic, ApiConsts.QuantSvr, string.Empty);
                    isSubscribeForQuantLog = true;
                }
            }
        }
        private void OnQuantLog(object obj)
        {
            DataBus.Instance.add(obj);
        }

        public void ProcessQuantLog(NetMessage netMsg)
        {
            if (netMsg != null)
            {
                //string[] topic = new string[] { $"{ApiConsts.Topic_stc_QuantLog}" };
                //if (netMsg.Topic.Contains(topic[0]))
                if (!string.IsNullOrWhiteSpace(this.UserName) && !"*".Equals(this.UserName))
                {
                    if (!netMsg.Topic.EndsWith(this.UserName))
                        return;
                }
                if (netMsg.Topic.StartsWith(ApiConsts.Topic_stc_QuantLog))
                {
                    if (netMsg.Data.StartsWith(ApiConsts.BRACKET))
                    {
                        List<TTbookQuantLog> data = BaseHelper.ConvertStcMessage<List<TTbookQuantLog>>(new string[] { netMsg.Topic }, netMsg);
                        if (data != null && data.Count > 0)
                        {
                            foreach (TTbookQuantLog mm in data)
                            {
                                On_QuanLog?.Invoke(mm);
                            }
                        }
                    }
                    else
                    {
                        TTbookQuantLog mm = BaseHelper.ConvertStcMessage<TTbookQuantLog>(new string[] { netMsg.Topic }, netMsg);

                        On_QuanLog?.Invoke(mm);
                    }
                }
            }
        }

        /// <summary>
        /// 订阅退订策略下所有指标
        /// </summary>
        /// <param name="onMsgCallBack"></param>
        public void UnSubscribeIndicator(Action<TbookQuantIndicator> onMsgCallBack)
        {
            if (onMsgCallBack != null && On_Indicator != null)
                On_Indicator -= onMsgCallBack;
        }

        public void SubscribeIndicator(Action<TbookQuantIndicator> onMsgCallBack)
        {
            if (onMsgCallBack != null && (On_Indicator == null || !On_Indicator.GetInvocationList().Contains(onMsgCallBack)))
                On_Indicator += onMsgCallBack;

            lock (this)
            {
                if (!isSubscribeForIndicator)
                {
                    //string[] topic = new string[] { $"{ApiConsts.Topic_stc_Paras}{UserName}.**" };
                    string[] topic = new string[] { $"{ApiConsts.Topic_stc_Indicator}**.{UserName}" };
                    ApiManager.Instance.GetDefaultApi().SubScribeWithImage(DataBus.Instance.Instanse_OnMessage, topic, ApiConsts.QuantSvr, string.Empty);
                    isSubscribeForIndicator = true;
                }
            }
        }
        private void OnIndicator(object obj)
        {
            DataBus.Instance.add(obj);
        }

        public void ProcessIndicator(NetMessage netMsg)
        {
            if (netMsg != null)
            {
                //if (netMsg.Topic.Contains(ApiConsts.Topic_stc_Indicator) && netMsg.Topic.Contains(UserName))
                if (!string.IsNullOrWhiteSpace(this.UserName) && !"*".Equals(this.UserName))
                {
                    if (!netMsg.Topic.EndsWith(this.UserName))
                        return;
                }
                if (netMsg.Topic.StartsWith(ApiConsts.Topic_stc_Indicator))
                {
                    if (netMsg.Data.StartsWith(ApiConsts.BRACKET))
                    {
                        List<TbookQuantIndicator> data = BaseHelper.ConvertStcMessage<List<TbookQuantIndicator>>(new string[] { netMsg.Topic }, netMsg);
                        if (data != null && data.Count > 0)
                        {
                            foreach (TbookQuantIndicator mm in data)
                            {
                                On_Indicator?.Invoke(mm);
                            }
                        }
                    }
                    else
                    {
                        TbookQuantIndicator mm = BaseHelper.ConvertStcMessage<TbookQuantIndicator>(new string[] { netMsg.Topic }, netMsg);

                        On_Indicator?.Invoke(mm);
                    }
                }
            }
        }

        // 关注组操作
        public PResult quantGroupUpdate(QuantGroupReq id)
        {
            return ApiManager.Instance.GetDefaultApi().Request<PResult>(ApiConsts.QuantSvr, ApiConsts.Topic_stc_IUDquantGroup, id);//ClientApi.Instance.homeGroupUpdate(request);
        }

        // 关注组查询
        public TbookQuantGroupsReplyPo quantGroupQuery()
        {
            return ApiManager.Instance.GetDefaultApi().Request<TbookQuantGroupsReplyPo>(ApiConsts.QuantSvr, ApiConsts.Topic_stc_queryGroups, null);//ClientApi.Instance.homeGroupQuery();
        }

        // 策略操作
        public PResult quantStrategyUpdate(QuantStrategyReq req)
        {
            return ApiManager.Instance.GetDefaultApi().Request<PResult>(ApiConsts.QuantSvr, ApiConsts.Topic_stc_IUDquantStrategy, req);//ClientApi.Instance.homeGroupUpdate(request);
        }

        // 参数操作
        public TTbookQuantParameterReplyAlter quantParasUpdate(TTbookQuantParameterRequest req)
        {
            return ApiManager.Instance.GetDefaultApi().Request<TTbookQuantParameterReplyAlter>(ApiConsts.QuantSvr, ApiConsts.Topic_stc_IUDquantParas, req);//ClientApi.Instance.homeGroupUpdate(request);
        }

        //策略开关接口
        public PResult quantUpdateEnable(QuantStrategyReq req)
        {
            return ApiManager.Instance.GetDefaultApi().Request<PResult>(ApiConsts.QuantSvr, ApiConsts.Topic_stc_UpdateEnable, req);//ClientApi.Instance.homeGroupUpdate(request);
        }

        //策略日志查询
        public TbookQuantLogReplyPo QuantLogQuery(QuantLogSelReq req)
        {
            return ApiManager.Instance.GetDefaultApi().Request<TbookQuantLogReplyPo>(ApiConsts.QuantSvr, ApiConsts.Topic_stc_SelQuantLog, req);
        }

        //查询模版策略接口
        public TbookQuantTempletReplyPo QueryTemplet()
        {
            return ApiManager.Instance.GetDefaultApi().Request<TbookQuantTempletReplyPo>(ApiConsts.QuantSvr, ApiConsts.Topic_stc_Seltemplet, null);
        }

        //查询策略指标
        public TbookQuantIndicatorReplyPo QueryIndicator(TbookQuantIndicatorReq req)
        {
            return ApiManager.Instance.GetDefaultApi().Request<TbookQuantIndicatorReplyPo>(ApiConsts.QuantSvr, ApiConsts.Topic_stc_SelIndicator, req);
        }

        public TTbookQuantStrategysReplyPo QuantAllStrategys()
        {
            return ApiManager.Instance.GetDefaultApi().Request<TTbookQuantStrategysReplyPo>(ApiConsts.QuantSvr, ApiConsts.Topic_stc_SelQuantStrategys, null);
        }

        //全撤全停
        public PResult disableStrategys(List<TTbookQuantStrategys> list)
        {
            return ApiManager.Instance.GetDefaultApi().Request<PResult>(ApiConsts.QuantSvr, ApiConsts.Topic_stc_disableStrategys, list);
        }
    }
}
