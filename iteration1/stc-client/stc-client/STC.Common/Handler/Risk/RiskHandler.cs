using netapi.Api;
using STC.Common.Api;
using STC.Common.Dispatcher;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.Risk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Handler.Risk
{
    /// <summary>
    /// 风险监控
    /// </summary>
    public class RiskHandler : IRiskHandler
    {
        private bool isSubscribeForRisk = false;
        private List<RiskIndicator> cacheRisk = new List<RiskIndicator>();
        private object _lockObj = new object();
        private event Action<List<RiskIndicator>> OnRisk;
        private List<string> _subTopic = new List<string>();

        /// <summary>
        /// RTL信息列表
        /// </summary>
        public static Dictionary<string, string> DictAllRTLMsg = new Dictionary<string, string>();
        private RiskHandler() { }
        private static IRiskHandler server = new RiskHandler();


        public static IRiskHandler Instance { get { return server; } }

        /// <summary>
        /// 取消所有订阅
        /// </summary>
        /// <param name="isSubscribe"></param>
        public void UnSubscribeAll(bool isSubscribe)
        {
            if (OnRisk != null)
                OnRisk = null;

            isSubscribeForRisk = isSubscribe;
        }

        /// <summary>
        /// 取消回调接口更新数据
        /// </summary>
        /// <param name="onRiskCallBack"></param>
        public void UnSubscribe(Action<List<RiskIndicator>> onRiskCallBack)
        {
            if (OnRisk != null && OnRisk.GetInvocationList().Contains(onRiskCallBack))
                OnRisk -= onRiskCallBack;
        }

        /// <summary>
        /// 取消订阅指定topic的数据
        /// </summary>
        /// <param name="onRiskCallBack"></param>
        /// <param name="topic"></param>
        public void UnSubscribe(Action<List<RiskIndicator>> onRiskCallBack, string topic)
        {
            if (OnRisk != null && OnRisk.GetInvocationList().Contains(onRiskCallBack))
                OnRisk -= onRiskCallBack;

            if (_subTopic.Contains(topic))
            {
                ApiManager.Instance.GetDefaultApi().UnSubScribe(ApiConsts.RiskSvr, new string[] { topic });
                //ClientApi.Instance.UnSubScribe(ApiConsts.RiskSvr, new string[] { topic });
                _subTopic.Remove(topic);
            }
        }

        /// <summary>
        /// 根据指定topic订阅风控指标数据
        /// </summary>
        /// <param name="onRiskCallBack"></param>
        /// <param name="topic"></param>
        public void Subscribe(Action<List<RiskIndicator>> onRiskCallBack, string topic)
        {
            if (onRiskCallBack != null && (OnRisk == null || !OnRisk.GetInvocationList().Contains(onRiskCallBack)))
                OnRisk += onRiskCallBack;

            if (!_subTopic.Contains(topic))
            {
                ApiManager.Instance.GetDefaultApi().SubScribeWithImage(DataBus.Instance.Instanse_OnMessage, new string[] { topic },
                        ApiConsts.RiskSvr, "");
                //ClientApi.Instance.subscribeForRiskNew(Instance_OnRisk, topic);
                _subTopic.Add(topic);
            }
            else
                Instance_OnRisk(cacheRisk);
        }

        private void Instance_OnRisk(object risk)
        {
            DataBus.Instance.add(risk);
        }

        public void ProcessRisk(NetMessage netMsg)
        {
            List<RiskIndicator> data = BaseHelper.ConvertStcMessage<List<RiskIndicator>>(new string[] { $"{ApiConsts.Topic_stc_risk_prefix}.**" }, netMsg);
            if (data != null)
                Instance_OnRisk(data);
        }

        /// <summary>
        /// 订阅当前组或当前用户的风控指标数据
        /// </summary>
        /// <param name="onRiskCallBack"></param>
        public void Init(Action<List<RiskIndicator>> onRiskCallBack)
        {
            if (onRiskCallBack != null && (OnRisk == null || !OnRisk.GetInvocationList().Contains(onRiskCallBack)))
                OnRisk += onRiskCallBack;

            if (!isSubscribeForRisk)
            {
                ApiManager.Instance.GetDefaultApi().SubScribeWithImage(DataBus.Instance.Instanse_OnMessage,
                    new string[] { string.Format("{0}{1}", ApiConsts.Topic_stc_risk_prefix, string.IsNullOrEmpty(ApiManager.Instance.GetDefaultApi().GroupId) ? ApiManager.Instance.GetDefaultApi().UserName : ApiManager.Instance.GetDefaultApi().GroupId) },
                        ApiConsts.RiskSvr, "");
                //ClientApi.Instance.subscribeForRisk(Instance_OnRisk);
                isSubscribeForRisk = true;
            }
            else
                Instance_OnRisk(cacheRisk);
        }

        /// <summary>
        /// 对应init订阅的数据回调接口
        /// </summary>
        /// <param name="risk"></param>
        private void Instance_OnRisk(List<RiskIndicator> risk)
        {
            lock (_lockObj)
            {
                cacheRisk = risk;
                OnRisk?.Invoke(risk);
            }
        }

        /// <summary>
        /// 请求历史风控指标数据
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public RiskHistoryReplyPo queryRisk(RiskHistoryReq req)
        {
            return ApiManager.Instance.GetDefaultApi().Request<RiskHistoryReplyPo>(ApiConsts.RiskSvr, ApiConsts.Topic_stc_riskLogprefix, req); //ClientApi.Instance.queryRiskHistory(req);
        }

        #region 特殊审批黑名单
        /// <summary>
        /// 新增/修改 特殊审批黑名单
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public PResult SetRisFilter(SetRisFilterReq req)
        {
            return ApiManager.Instance.GetDefaultApi().Request<PResult>(ApiConsts.RiskSvr, ApiConsts.Topic_stc_BlackList_SetRisFilter, req);
        }

        /// <summary>
        /// 查询当前用户的设置的特殊审批黑名单
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public RisBlackResult QueryRisFilter(QueryRisFilterReq req)
        {
            return ApiManager.Instance.GetDefaultApi().Request<RisBlackResult>(ApiConsts.RiskSvr, ApiConsts.Topic_stc_BlackList_QueryRisFilter, req);
        }

        /// <summary>
        /// 下载查询所有黑名单
        /// </summary>
        /// <returns></returns>
        public RisBlackResult QueryRisBlack()
        {
            RisBlackResult res = ApiManager.Instance.GetDefaultApi().Request<RisBlackResult>(ApiConsts.RiskSvr, ApiConsts.Topic_stc_BlackList_QueryRisBlack, "{}");
            CacheRisBlack(res);
            return res;
        }

        public async void CacheRisBlack(RisBlackResult reply)
        {
            await Task.Run(() =>
            {
                if (reply != null && reply.IsSuccess())
                {
                    if (reply.Datas != null && reply.Datas.Count > 0)
                    {
                        foreach (RisBlackModel pbm in reply.Datas)
                        {
                            if (pbm.enable != null)
                            {
                                if (pbm.code != null)
                                {
                                    if (!DictAllRTLMsg.ContainsKey(pbm.code))
                                        DictAllRTLMsg.Add(pbm.code, pbm.message);
                                }
                            }
                        }
                    }
                }
            });
        }

        #endregion

        public BatchCheckRiskReplyPo BatchCheckRisk(List<NewOrderSingle> req)
        {
            return ApiManager.Instance.GetDefaultApi().Request<BatchCheckRiskReplyPo>(ApiConsts.RiskSvr, ApiConsts.Topic_RiskSvr_BatchCheckRisk, req);
        }
    }
}
