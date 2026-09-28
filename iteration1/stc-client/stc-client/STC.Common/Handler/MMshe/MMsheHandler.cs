using com.gateway.log;
using netapi.Api;
using Newtonsoft.Json;
using STC.Common.Api;
using STC.Common.Dispatcher;
using STC.Common.Model.Admin;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.MMshe;
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
    public class MMsheHandler : Dispatcher.AsyncBase<object>, IMMsheHandler
    {
        private static MMsheHandler mmHandler = new MMsheHandler();
        private bool isSubscribeForMM = false;
        private bool isSubscribeForMMSysStatus = false;

        private event Action<MMshe> On_MM;
        private event Action<MMsheSysStatus> On_MMSysStatus;
        

        public static IMMsheHandler Instance { get { return mmHandler; } }

        private MMsheHandler()
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

        public void UnsubscribeMMSysStatus(Action<MMsheSysStatus> onMsgCallBack, List<string> userNameList)
        {
            if (onMsgCallBack != null && On_MMSysStatus != null)
                On_MMSysStatus -= onMsgCallBack;

            if (On_MMSysStatus == null)
            {
                isSubscribeForMMSysStatus = false;
                string[] topics = new string[userNameList.Count];
                for (int i = 0; i < userNameList.Count; i++)
                {
                    topics[i] = string.Format(ApiConsts.Topic_stc_mmshe_sysStatus, userNameList[i]);
                }
                ApiManager.Instance.GetDefaultApi().UnSubScribe(ApiConsts.MMSHESvr, topics);
            }
        }

        /// <summary>
        /// 订阅做市状态接口
        /// </summary>
        /// <param name="onMsgCallBack"></param>
        public void SubscribeMMSysStatus(Action<MMsheSysStatus> onMsgCallBack, List<string> userNameList)
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
                            topics[i] = string.Format(ApiConsts.Topic_stc_mmshe_sysStatus, userNameList[i]);
                        }
                        ApiManager.Instance.GetDefaultApi().SubScribeWithImage(OnMMSysStatus,
                            topics,
                            ApiConsts.MMSHESvr, string.Empty);
                    }
                    isSubscribeForMMSysStatus = true;
                }
            }
        }

        Regex reg = new Regex(ApiConsts.Topic_stc_mmshe_sysStatus_regex);
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

                    MMsheSysStatus mmSysStatus = BaseHelper.ConvertStcMessage<MMsheSysStatus>(new string[] { netMsg.Topic }, netMsg);

                    On_MMSysStatus?.Invoke(mmSysStatus);
                }
            }
        }

        public void UnSubscribeMM(Action<MMshe> onMsgCallBack)
        {
            if (onMsgCallBack != null && On_MM != null)
                On_MM -= onMsgCallBack;
        }

        /// <summary>
        /// 订阅当前用户下所有关注组债券
        /// </summary>
        /// <param name="onMsgCallBack"></param>
        public void SubscribeMM(Action<MMshe> onMsgCallBack)
        {
            if (onMsgCallBack != null && (On_MM == null || !On_MM.GetInvocationList().Contains(onMsgCallBack)))
                On_MM += onMsgCallBack;

            lock (this)
            {
                if (!isSubscribeForMM)
                {
                    ApiManager.Instance.GetDefaultApi().SubScribeWithImage(OnMM,
                        new string[] { string.Format(ApiConsts.Topic_stc_mmshe, LocalEnv.UserInfo.UserName ) },
                        ApiConsts.MMSHESvr, string.Empty);
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
            if (netMsg.Topic.StartsWith(ApiConsts.Topic_stc_mmshe_regex))
                this.add(obj);
        }
		
        public override void process(object t)
        {
            try
            {
                NetMessage netMsg = t as NetMessage;
                if (netMsg == null)
                    return;
                if (netMsg.Topic.StartsWith(ApiConsts.Topic_stc_mmshe_regex))
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
                if (netMsg.Topic.StartsWith(ApiConsts.Topic_stc_mmshe_regex))
                {
                    LogHandler.Info($"OnMM:{JsonConvert.SerializeObject(netMsg)}");
                }
                if (netMsg.Topic.StartsWith(ApiConsts.Topic_stc_mmshe_regex)
                    && netMsg.Topic.EndsWith(LocalEnv.UserInfo.UserName))
                {
                  if (netMsg.Data.StartsWith(ApiConsts.BRACKET))
                    {
                        List<MMshe> data = BaseHelper.ConvertStcMessage<List<MMshe>>(new string[] { netMsg.Topic }, netMsg);
                        if (data != null && data.Count > 0)
                        {
                            foreach(MMshe mm in data)
                            {
                                On_MM?.Invoke(mm);
                            }
                        }
                    }
                    else
                    {
                        MMshe mm = BaseHelper.ConvertStcMessage<MMshe>(new string[] { netMsg.Topic }, netMsg);

                        On_MM?.Invoke(mm);
                    }
                }
                
            }
        }


        // 关注组操作
        public PResult mmGroupUpdate(MMsheGroupReq request)
        {
            return ApiManager.Instance.GetDefaultApi().Request<PResult>(ApiConsts.MMSHESvr, ApiConsts.Topic_stc_updateMMsheGroup, request);//ClientApi.Instance.homeGroupUpdate(request);
        }

        // 关注组查询
        public MMsheGroupReplyPo mmGroupQuery()
        {
            return ApiManager.Instance.GetDefaultApi().Request<MMsheGroupReplyPo>(ApiConsts.MMSHESvr, ApiConsts.Topic_stc_queryMMsheGroup, null);//ClientApi.Instance.homeGroupQuery();
        }

        // 更新债券
        public PResult mmUpdate(MMsheReq request)
        {
            return ApiManager.Instance.GetDefaultApi().Request<PResult>(ApiConsts.MMSHESvr, ApiConsts.Topic_stc_updateMMshe, request);
        }

        // 更新下单参数
        public PResult mmUpdateParams(MMsheOrderParamReq request)
        {
            return ApiManager.Instance.GetDefaultApi().Request<PResult>(ApiConsts.MMSHESvr, ApiConsts.Topic_stc_mmshe_updateParams, request);
        }

        // 更新下单参数-发送基准券
        public PResult mmUpdateSecurityCdc(MMsheOrderParamReq request)
        {
            return ApiManager.Instance.GetDefaultApi().Request<PResult>(ApiConsts.MMSHESvr, ApiConsts.Topic_stc_mmshe_updateSecurityCdc, request);
        }

        // 单债券开关接口
        public PResult mmUpdateEnable(MMsheUpdateEnableReq request)
        {
            return ApiManager.Instance.GetDefaultApi().Request<PResult>(ApiConsts.MMSHESvr, ApiConsts.Topic_stc_mmshe_updateEnable, request);
        }

        public UserSettingReplyPo queryMMUserSetting()
        {
            return ApiManager.Instance.GetDefaultApi().Request<UserSettingReplyPo>(ApiConsts.MMSHESvr, ApiConsts.Topic_stc_mmshe_queryMMUserSetting, null);
        }

        public PResult updateMMUserSetting(MMsheUserSetting userSetting)
        {
            return ApiManager.Instance.GetDefaultApi().Request<PResult>(ApiConsts.MMSHESvr, ApiConsts.Topic_stc_mmshe_updateMMUserSetting, userSetting);
        }

        // 全局开关接口
        public PResult mmUpdateAllEnable(MMsheUpdateAllEnableReq request)
        {
            return ApiManager.Instance.GetDefaultApi().Request<PResult>(ApiConsts.MMSHESvr, ApiConsts.Topic_stc_mmshe_updateAllEnable, request);
        }

        // 一键拉宽接口
        public PResult mmUpdateWidenSetting(List<MMsheUpdateWidenSettingReq> request)
        {
            return ApiManager.Instance.GetDefaultApi().Request<PResult>(ApiConsts.MMSHESvr, ApiConsts.Topic_stc_mmshe_updateWidenSetting, request);
        }

        // 获取一键拉宽设置接口
        public MMsheQueryWidenSettingReplyPo mmQueryWidenSetting()
        {
            return ApiManager.Instance.GetDefaultApi().Request<MMsheQueryWidenSettingReplyPo>(ApiConsts.MMSHESvr, ApiConsts.Topic_stc_mmshe_queryWidenSetting, null);
        }

        // 取消一键拉宽接口
        public PResult mmCancelWidenSetting(MMsheCancelWidenSettingReq request)
        {
            return ApiManager.Instance.GetDefaultApi().Request<PResult>(ApiConsts.MMSHESvr, ApiConsts.Topic_stc_mmshe_cancelWidenSetting, request);
        }

        /// <summary>
        /// 做市债券指标检查接口
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public MMsheCheckSecurityUserReplyPo mmCheckSecurityUser(List<MMsheCheckSecurityUserReq> request)
        {
            return ApiManager.Instance.GetDefaultApi().Request<MMsheCheckSecurityUserReplyPo>(ApiConsts.MMSHESvr, ApiConsts.Topic_stc_mmshe_checkSecurityUser, request);
        }

        /// <summary>
        /// 根据securityID获取债券余额
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public MMsheQuerySecurityCirulSizeReplyPo mmQueryBondBalance(string request)
        {
            return ApiManager.Instance.GetDefaultApi().Request<MMsheQuerySecurityCirulSizeReplyPo>(ApiConsts.MMSHESvr, ApiConsts.Topic_stc_mmshe_QueryBondBalance, request);
        }

        public RiskQtyBySecurityReplyPo mmQueryRiskQty(string request)
        {
            return ApiManager.Instance.GetDefaultApi().Request<RiskQtyBySecurityReplyPo>(ApiConsts.MMSHESvr, ApiConsts.Topic_stc_mmshe_QueryRiskQty, request);
        }


        #region 交易所剔除行情和恢复

        // 双边剔除行情
        public PResult mmFilterQuote(MMsheFilterQuoteReq request)
        {
            return ApiManager.Instance.GetDefaultApi().Request<PResult>(ApiConsts.MMSHESvr, ApiConsts.Topic_stc_mmshe_FilterQuote, request);
        }

        #endregion

        public STC.Common.Model.MM.MMBondParamsSettingReplyPo mmQuerySingleBondSetting()
        {
            return ApiManager.Instance.GetDefaultApi().Request<STC.Common.Model.MM.MMBondParamsSettingReplyPo>(ApiConsts.MMSHESvr, ApiConsts.Topic_stc_mmshe_querySingleBondSetting, null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="groupid"></param>
        /// <param name="enable">0停止，1启动</param>
        /// <returns></returns>
        public PResult HandleUnifyGroup(MMsheUnifyGroupPo request)
        {
            //string request = string.Format("{{\"groupId\":\"{0}\",\"enable\":\"{1}\"}}", groupid, enable);
            return ApiManager.Instance.GetDefaultApi().Request<PResult>(ApiConsts.MMSHESvr, ApiConsts.Topic_stc_HandleUnifyGroup, request);
        }
    }
}
