using com.gateway.log;
using netapi.Api;
using STC.Common.Api;
using STC.Common.Dispatcher;
using STC.Common.Model;
using STC.Common.Model.Ans;
using STC.Common.Model.Fix.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace STC.Common.Handler.Ans
{
    public class AnsHandler : AsyncBase<object>, IAnsHandler
    {

        private bool isSubscribeForANS = false;

        private event Action<ANSMonitorInfo> On_ANSMonitor;

        private bool isSubscribeForANSSummary = false;

        private event Action<ANSSummary> On_ANSSummary;

        private static AnsHandler ansHandler = new AnsHandler();
        public static IAnsHandler Instance { get { return ansHandler; } }

        private AnsHandler()
        {
            this.start();
        }

        public void UnSubscribeAll(bool isSubscribe)
        {
            if (On_ANSMonitor != null)
                On_ANSMonitor = null;

            if (On_ANSSummary != null)
                On_ANSSummary = null;

            isSubscribeForANS = isSubscribe;
            isSubscribeForANSSummary = isSubscribe;
        }

        /// <summary>
        /// 取消订阅做市义务汇总实时接口
        /// </summary>
        /// <param name="onMsgCallBack"></param>
        public void UnSubscribeAnsSummary(Action<ANSSummary> onMsgCallBack)
        {
            if (onMsgCallBack != null && On_ANSMonitor != null)
                On_ANSSummary -= onMsgCallBack;

            if (On_ANSSummary == null)
            {
                ApiManager.Instance.GetDefaultApi().UnSubScribe(ApiConsts.MMSvr, new string[] { ApiConsts.Topic_stc_ans_monitor_total });
                isSubscribeForANSSummary = false;
            }
        }


        /// <summary>
        /// 订阅做市义务汇总实时接口
        /// </summary>
        /// <param name="onMsgCallBack"></param>
        public void SubscribeAnsSummary(Action<ANSSummary> onMsgCallBack)
        {
            if (onMsgCallBack != null && (On_ANSSummary == null || !On_ANSSummary.GetInvocationList().Contains(onMsgCallBack)))
                On_ANSSummary += onMsgCallBack;

            lock (this)
            {
                if (!isSubscribeForANSSummary)
                {
                    ApiManager.Instance.GetDefaultApi().SubScribeWithImage(OnANSSummary2,
                        new string[] { ApiConsts.Topic_stc_ans_monitor_total },
                        ApiConsts.AnsSvr, string.Empty);
                    isSubscribeForANSSummary = true;
                }
            }
        }

        private void OnANSSummary2(object obj)
        {
            NetMessage netMsg = obj as NetMessage;
            if (netMsg == null)
                return;
            if (netMsg.Topic.StartsWith(ApiConsts.Topic_stc_ans_monitor_total))
                this.add(netMsg);
        }

        private void ProcessANSSummary(NetMessage netMsg)
        {
            if (netMsg != null)
            {
                if (netMsg.Topic == ApiConsts.Topic_stc_ans_monitor_total)
                {
                    //if (netMsg.Data.StartsWith(ApiConsts.BRACKET))
                    //{
                    //    List<ANSSummary> data = BaseHelper.ConvertStcMessage<List<ANSSummary>>(new string[] { ApiConsts.Topic_stc_ans_monitor_total }, obj);
                    //    if (data != null && data.Count > 0)
                    //    {
                    //        foreach (ANSSummary ansSummary in data)
                    //        {
                    //            On_ANSSummary?.Invoke(ansSummary);
                    //        }
                    //    }
                    //}
                    //else
                    {
                        ANSSummary ansSummary = BaseHelper.ConvertStcMessage<ANSSummary>(new string[] { ApiConsts.Topic_stc_ans_monitor_total }, netMsg);

                        On_ANSSummary?.Invoke(ansSummary);
                    }
                }

            }
        }

        /// <summary>
        /// 取消订阅订阅做市义务债券详细实时接口
        /// </summary>
        /// <param name="onMsgCallBack"></param>
        public void UnSubscribeAnsMonitorInfo(Action<ANSMonitorInfo> onMsgCallBack)
        {
            if (onMsgCallBack != null && On_ANSMonitor != null)
                On_ANSMonitor -= onMsgCallBack;
        }


        /// <summary>
        /// 订阅做市义务债券详细实时接口
        /// </summary>
        /// <param name="onMsgCallBack"></param>
        /// <param name="userNameList"></param>
        public void SubscribeAnsMonitorInfo(Action<ANSMonitorInfo> onMsgCallBack, List<string> userNameList)
        {
            if (onMsgCallBack != null && (On_ANSMonitor == null || !On_ANSMonitor.GetInvocationList().Contains(onMsgCallBack)))
                On_ANSMonitor += onMsgCallBack;

            lock (this)
            {
                if (!isSubscribeForANS)
                {
                    if (userNameList != null && userNameList.Count > 0)
                    {
                        string[] topics = new string[userNameList.Count];
                        for (int i = 0; i < userNameList.Count; i++)
                        {
                            topics[i] = string.Format(ApiConsts.Topic_stc_ans_username_detail, userNameList[i]);
                        }
                        ApiManager.Instance.GetDefaultApi().SubScribeWithImage(OnANSMonitor,
                            topics,
                            ApiConsts.AnsSvr, string.Empty);
                    }
                    isSubscribeForANS = true;
                }
            }
        }

        Regex reg = new Regex(ApiConsts.Topic_stc_ans_username_detail_regex);
        private void OnANSMonitor(object obj)
        {
            NetMessage netMsg = obj as NetMessage;
            if (netMsg == null)
                return;
            if (reg.IsMatch(netMsg.Topic))
                this.add(netMsg);
        }

        private void ProcessANSMonitor(NetMessage netMsg)
        {
            if (netMsg != null)
            {
                if (reg.IsMatch(netMsg.Topic))
                {
                    if (netMsg.Data.StartsWith(ApiConsts.BRACKET))
                    {
                        List<ANSMonitorInfo> data = BaseHelper.ConvertStcMessage<List<ANSMonitorInfo>>(new string[] { netMsg.Topic }, netMsg);
                        if (data != null && data.Count > 0)
                        {
                            foreach (ANSMonitorInfo ansMonitorInfo in data)
                            {
                                On_ANSMonitor?.Invoke(ansMonitorInfo);
                            }
                        }
                    }
                    else
                    {
                        ANSMonitorInfo ansMonitorInfo = BaseHelper.ConvertStcMessage<ANSMonitorInfo>(new string[] { netMsg.Topic }, netMsg);

                        On_ANSMonitor?.Invoke(ansMonitorInfo);
                    }
                }

            }
        }

        /// <summary>
        /// 查询做市义务债券详细
        /// </summary>
        /// <param name="usename">登录用户名</param>
        /// <returns></returns>
        public QueryAnsMonitorReplyPo QueryAnsMonitorInfo(string usename, string securitys)
        {
            return ApiManager.Instance.GetDefaultApi().Request<QueryAnsMonitorReplyPo>(ApiConsts.AnsSvr,
                string.Format(ApiConsts.Topic_stc_ans_username_detail, usename), securitys);
        }

        /// <summary>
        /// 做市详情查询接口
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public QueryANSDetailReplyPo QueryANSDetail(QueryANSDetailReq request)
        {
            return ApiManager.Instance.GetDefaultApi().Request<QueryANSDetailReplyPo>(ApiConsts.AnsSvr, ApiConsts.Topic_stc_ans_queryANSDetail, request);
        }

        /// <summary>
        /// 做市概要查询接口
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public QueryANSOutlineReplyPo QueryANSOutline(QueryANSOutlineReq request)
        {
            return ApiManager.Instance.GetDefaultApi().Request<QueryANSOutlineReplyPo>(ApiConsts.AnsSvr, ApiConsts.Topic_stc_ans_queryANSOutline, request);
        }

        public override void process(object t)
        {
            try
            {
                NetMessage netMsg = t as NetMessage;
                if (netMsg == null)
                    return;
                if (netMsg.Topic.StartsWith(ApiConsts.Topic_stc_ans_monitor_total))
                {
                    ProcessANSSummary(netMsg);
                }
                else if (reg.IsMatch(netMsg.Topic))
                {
                    ProcessANSMonitor(netMsg);
                }
            }
            catch(Exception e)
            {
                LogHelper.Error(e);
            }
        }

        public PResult IgnoreBreak(IgnoreBreakReq request)
        {
            return ApiManager.Instance.GetDefaultApi().Request<PResult>(ApiConsts.AnsSvr, ApiConsts.Topic_stc_ans_ignoreBreak, request);
        }
    }
}
