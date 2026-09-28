using com.gateway.log;
using netapi.Api;
using STC.Common.Api;
using STC.Common.Dispatcher;
using STC.Common.Model.Admin;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.MM;
using STC.Common.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace STC.Common.Handler.Admin
{
    /// <summary>
    /// MMSvr接口类
    /// </summary>
    public class MMHandler : Dispatcher.AsyncBase<object>, IMMHandler 
    {
        private static MMHandler mmHandler = new MMHandler();
        private bool isSubscribeForMM = false;
        private bool isSubscribeForMMSysStatus = false;

        private event Action<MM> On_MM;
        private event Action<MMSysStatus> On_MMSysStatus;
        

        public static IMMHandler Instance { get { return mmHandler; } }

        private MMHandler()
        {
            this.start();
        }

        public void UnSubscribeAll(bool isSubscribe)
        {
            if (On_MM != null)
                On_MM = null;
            if (On_MMSysStatus != null)
                On_MMSysStatus = null;

            isSubscribeForMM = isSubscribe;
            isSubscribeForMMSysStatus = isSubscribe;
        }

        public void UnsubscribeMMSysStatus(Action<MMSysStatus> onMsgCallBack, List<string> userNameList)
        {
            if (onMsgCallBack != null && On_MMSysStatus != null)
                On_MMSysStatus -= onMsgCallBack;

            if (On_MMSysStatus == null)
            {
                isSubscribeForMMSysStatus = false;
                string[] topics = new string[userNameList.Count];
                for (int i = 0; i < userNameList.Count; i++)
                {
                    topics[i] = string.Format(ApiConsts.Topic_stc_mm_sysStatus, userNameList[i]);
                }
                ApiManager.Instance.GetDefaultApi().UnSubScribe(ApiConsts.MMSvr, topics);
            }
        }

        /// <summary>
        /// 订阅做市状态接口
        /// </summary>
        /// <param name="onMsgCallBack"></param>
        public void SubscribeMMSysStatus(Action<MMSysStatus> onMsgCallBack, List<string> userNameList)
        {
            if (onMsgCallBack != null && (On_MMSysStatus == null || !On_MMSysStatus.GetInvocationList().Contains(onMsgCallBack)))
                On_MMSysStatus += onMsgCallBack;

            lock (this)
            {
                if (!isSubscribeForMMSysStatus)
                {
                    if (userNameList != null && userNameList.Count > 0)
                    {
                        string[] topics = new string[userNameList.Count];
                        for (int i = 0; i < userNameList.Count; i++)
                        {
                            topics[i] = string.Format(ApiConsts.Topic_stc_mm_sysStatus, userNameList[i]);
                        }
                        ApiManager.Instance.GetDefaultApi().SubScribeWithImage(OnMMSysStatus,
                            topics,
                            ApiConsts.MMSvr, string.Empty);
                    }
                    isSubscribeForMMSysStatus = true;
                }
            }
        }

        Regex reg = new Regex(ApiConsts.Topic_stc_mm_sysStatus_regex);
        private void OnMMSysStatus(object obj)
        {
            NetMessage netMsg = obj as NetMessage;
            if (netMsg == null)
                return;
            if (reg.IsMatch(netMsg.Topic))
                this.add(obj);
        }

        public void ProcessMMSysStatus(NetMessage netMsg)
        {
            if (netMsg != null)
            {
                if (reg.IsMatch(netMsg.Topic))
                {
                    MMSysStatus mmSysStatus = BaseHelper.ConvertStcMessage<MMSysStatus>(new string[] { netMsg.Topic }, netMsg);

                    On_MMSysStatus?.Invoke(mmSysStatus);
                }
            }
        }

        public void UnSubscribeMM(Action<MM> onMsgCallBack)
        {
            if (onMsgCallBack != null && On_MM != null)
                On_MM -= onMsgCallBack;
        }

        /// <summary>
        /// 订阅当前用户下所有关注组债券
        /// </summary>
        /// <param name="onMsgCallBack"></param>
        public void SubscribeMM(Action<MM> onMsgCallBack)
        {
            if (onMsgCallBack != null && (On_MM == null || !On_MM.GetInvocationList().Contains(onMsgCallBack)))
                On_MM += onMsgCallBack;

            lock (this)
            {
                if (!isSubscribeForMM)
                {
                    ApiManager.Instance.GetDefaultApi().SubScribeWithImage(OnMM,
                        new string[] { string.Format(ApiConsts.Topic_stc_mm, LocalEnv.UserInfo.UserName ) },
                        ApiConsts.MMSvr, string.Empty);
                    //ClientApi.Instance.subscribeSystemMessageForAdmin(Instance_OnAdminMsg);
                    isSubscribeForMM = true;
                }
            }
        }


        private void OnMM(object obj)
        {
            NetMessage netMsg = obj as NetMessage;
            if (netMsg == null)
                return;
            if (netMsg.Topic.StartsWith(ApiConsts.Topic_stc_mm_regex))
                this.add(obj);
        }

        public override void process(object t)
        {
            try
            {
                NetMessage netMsg = t as NetMessage;
                if (netMsg == null)
                    return;
                if (netMsg.Topic.StartsWith(ApiConsts.Topic_stc_mm_regex))
                {
                    this.ProcessMM(netMsg);
                }
                else if (reg.IsMatch(netMsg.Topic))
                {
                    this.ProcessMMSysStatus(netMsg);
                }
            }
            catch (Exception e)
            {
                LogHelper.Error(e);
            }
        }

        public void ProcessMM(NetMessage netMsg)
        {
            if (netMsg != null)
            {
                if (netMsg.Topic.StartsWith(ApiConsts.Topic_stc_mm_regex)
                    && netMsg.Topic.Contains(LocalEnv.UserInfo.UserName))
                {
                    if (netMsg.Data.StartsWith(ApiConsts.BRACKET))
                    {
                        List<MM> data = BaseHelper.ConvertStcMessage<List<MM>>(new string[] { netMsg.Topic }, netMsg);
                        if (data != null && data.Count > 0)
                        {
                            foreach (MM mm in data)
                            {
                                On_MM?.Invoke(mm);
                            }
                        }
                    }
                    else
                    {
                        MM mm = BaseHelper.ConvertStcMessage<MM>(new string[] { netMsg.Topic }, netMsg);

                        On_MM?.Invoke(mm);
                    }
                }

            }
        }

        // 关注组操作
        public PResult mmGroupUpdate(MMGroupReq request)
        {
            return ApiManager.Instance.GetDefaultApi().Request<PResult>(ApiConsts.MMSvr, ApiConsts.Topic_stc_updateMMGroup, request);//ClientApi.Instance.homeGroupUpdate(request);
        }

        // 关注组查询
        public MMGroupReplyPo mmGroupQuery()
        {
            return ApiManager.Instance.GetDefaultApi().Request<MMGroupReplyPo>(ApiConsts.MMSvr, ApiConsts.Topic_stc_queryMMGroup, null);//ClientApi.Instance.homeGroupQuery();
        }

        // 更新债券
        public PResult mmUpdate(MMReq request)
        {
            return ApiManager.Instance.GetDefaultApi().Request<PResult>(ApiConsts.MMSvr, ApiConsts.Topic_stc_updateMM, request);
        }

        // 更新下单参数
        public PResult mmUpdateParams(MMOrderParamReq request)
        {
            return ApiManager.Instance.GetDefaultApi().Request<PResult>(ApiConsts.MMSvr, ApiConsts.Topic_stc_updateParams, request);
        }

        // 更新下单参数-发送基准券
        public PResult mmUpdateSecurityCdc(MMOrderParamReq request)
        {
            return ApiManager.Instance.GetDefaultApi().Request<PResult>(ApiConsts.MMSvr, ApiConsts.Topic_stc_updateSecurityCdc, request);
        }

        // 单债券开关接口
        public PResult mmUpdateEnable(MMUpdateEnableReq request)
        {
            return ApiManager.Instance.GetDefaultApi().Request<PResult>(ApiConsts.MMSvr, ApiConsts.Topic_stc_updateEnable, request);
        }

        public UserSettingReplyPo queryMMUserSetting()
        {
            return ApiManager.Instance.GetDefaultApi().Request<UserSettingReplyPo>(ApiConsts.MMSvr, ApiConsts.Topic_stc_queryMMUserSetting, null);
        }

        public PResult updateMMUserSetting(UserSetting userSetting)
        {
            return ApiManager.Instance.GetDefaultApi().Request<PResult>(ApiConsts.MMSvr, ApiConsts.Topic_stc_updateMMUserSetting, userSetting);
        }

        // 全局开关接口
        public PResult mmUpdateAllEnable(MMUpdateAllEnableReq request)
        {
            return ApiManager.Instance.GetDefaultApi().Request<PResult>(ApiConsts.MMSvr, ApiConsts.Topic_stc_updateAllEnable, request);
        }

        // 一键拉宽接口
        public PResult mmUpdateWidenSetting(List<MMUpdateWidenSettingReq> request)
        {
            return ApiManager.Instance.GetDefaultApi().Request<PResult>(ApiConsts.MMSvr, ApiConsts.Topic_stc_updateWidenSetting, request);
        }

        // 获取一键拉宽设置接口
        public MMQueryWidenSettingReplyPo mmQueryWidenSetting()
        {
            return ApiManager.Instance.GetDefaultApi().Request<MMQueryWidenSettingReplyPo>(ApiConsts.MMSvr, ApiConsts.Topic_stc_queryWidenSetting, null);
        }

        // 取消一键拉宽接口
        public PResult mmCancelWidenSetting(MMCancelWidenSettingReq request)
        {
            return ApiManager.Instance.GetDefaultApi().Request<PResult>(ApiConsts.MMSvr, ApiConsts.Topic_stc_cancelWidenSetting, request);
        }

        /// <summary>
        /// 做市债券指标检查接口
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public MMCheckSecurityUserReplyPo mmCheckSecurityUser(List<MMCheckSecurityUserReq> request)
        {
            return ApiManager.Instance.GetDefaultApi().Request<MMCheckSecurityUserReplyPo>(ApiConsts.MMSvr, ApiConsts.Topic_stc_checkSecurityUser, request);
        }

        /// <summary>
        /// 根据securityID获取债券余额
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public MMQuerySecurityCirulSizeReplyPo mmQueryBondBalance(string request)
        {
            return ApiManager.Instance.GetDefaultApi().Request<MMQuerySecurityCirulSizeReplyPo>(ApiConsts.MMSvr, ApiConsts.Topic_stc_QueryBondBalance, request);
        }

        public RiskQtyBySecurityReplyPo mmQueryRiskQty(string request)
        {
            return ApiManager.Instance.GetDefaultApi().Request<RiskQtyBySecurityReplyPo>(ApiConsts.MMSvr, ApiConsts.Topic_stc_QueryRiskQty, request);
        }


        #region 双边剔除行情和恢复

        // 双边剔除行情
        public PResult mmFilterQuote(MMFilterQuoteReq request)
        {
            return ApiManager.Instance.GetDefaultApi().Request<PResult>(ApiConsts.MMSvr, ApiConsts.Topic_stc_mm_FilterQuote, request);
        }

        #endregion

        public DealCloseEnableReply queryDealCloseEnableSetting()
        {
            return ApiManager.Instance.GetDefaultApi().Request<DealCloseEnableReply>(ApiConsts.MMSvr, ApiConsts.Topic_stc_mm_QueryDealCloseEnable, null);
        }

        public QueryMMSecurityidHistReplyPo QueryMMSecurityidHist(QueryMMSecurityidHistRequest request)
        {
            return ApiManager.Instance.GetDefaultApi().Request<QueryMMSecurityidHistReplyPo>(ApiConsts.MMSvr, ApiConsts.Topic_stc_QueryMMSecurityidHist, request);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="groupid"></param>
        /// <param name="enable">0停止，1启动</param>
        /// <returns></returns>
        public PResult HandleUnifyGroup(string groupid, string enable)
        {
            string request = string.Format("{{\"groupId\":\"{0}\",\"enable\":\"{1}\"}}",groupid, enable);
            return ApiManager.Instance.GetDefaultApi().Request<PResult>(ApiConsts.MMSvr, ApiConsts.Topic_stc_HandleUnifyGroup, request);
        }

        public MMBondParamsSettingReplyPo mmQuerySingleBondSetting()
        {
            return ApiManager.Instance.GetDefaultApi().Request<MMBondParamsSettingReplyPo>(ApiConsts.MMSvr, ApiConsts.Topic_stc_querySingleBondSetting, null);
        }
    }
}
