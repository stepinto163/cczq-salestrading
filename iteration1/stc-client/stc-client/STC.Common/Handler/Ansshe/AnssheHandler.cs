using netapi.Api;
using STC.Common.Api;
using STC.Common.Model.Ans;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace STC.Common.Handler.Ansshe
{
    public class AnssheHandler : IAnssheHandler
    {

        private bool isSubscribeForANS = false;

        private event Action<ANSMonitorInfo> On_ANSMonitor;

        private bool isSubscribeForANSSummary = false;

        private event Action<ANSSummary> On_ANSSummary;

        private static AnssheHandler ansHandler = new AnssheHandler();
        public static IAnssheHandler Instance { get { return ansHandler; } }

        private AnssheHandler()
        {
            // ignore
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
                        ANSSummary ansSummary = BaseHelper.ConvertStcMessage<ANSSummary>(new string[] { ApiConsts.Topic_stc_ans_monitor_total }, obj);

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
            if (netMsg != null)
            {
                if (reg.IsMatch(netMsg.Topic))
                {
                    if (netMsg.Data.StartsWith(ApiConsts.BRACKET))
                    {
                        List<ANSMonitorInfo> data = BaseHelper.ConvertStcMessage<List<ANSMonitorInfo>>(new string[] { netMsg.Topic }, obj);
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
                        ANSMonitorInfo ansMonitorInfo = BaseHelper.ConvertStcMessage<ANSMonitorInfo>(new string[] { netMsg.Topic }, obj);

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
        /// 做市详情查询接口s
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
    }
}
