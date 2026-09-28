using netapi.Api;
using Newtonsoft.Json;
using STC.Common.Api;
using STC.Common.Dispatcher;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.Monitor;
using STC.Common.Model.Risk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Handler.Monitor
{
    public class MonitorHandler : IMonitorHandler
    {
        private static IMonitorHandler server = new MonitorHandler();
        public static IMonitorHandler Instance { get { return server; } }

        private event Action<Dictionary<string, monitormodel>> OnMonitorInfoCallBack;
        private event Action<Dictionary<string, Dictionary<string, monitormodel>>> OnMonitorMsgInfoCallBack;

        private bool isSubscribeForMonitor = false;
        private bool isSubscribeForMonitorMsg = false;
        private object _lockObj = new object();
        private MonitorHandler()
        {
        }


        /// <summary>
        /// 订阅Zookeeper
        /// </summary>
        /// <param name="onMsgCallBack"></param>
        public void SubscribeMonitorInfo(Action<Dictionary<string, monitormodel>> callBack)
        {
            if (callBack != null)
            {
                OnMonitorInfoCallBack += callBack;
            }

            lock (this)
            {
                if (!isSubscribeForMonitor)
                {
                    string[] topics = new string[1];
                    topics[0] = $"{ApiConsts.Topic_stc_MonitorInfo}";
                    ApiManager.Instance.GetDefaultApi().SubScribeWithImage(DataBus.Instance.Instanse_OnMessage, topics, ApiConsts.MonitorSvr, string.Empty);

                    isSubscribeForMonitor = true;
                }
            }
        }

        /// <summary>
        /// 退订 Zookeeper
        /// </summary>
        /// <param name="callBack"></param>
        public void UnSubscribeMonitorInfo(Action<Dictionary<string, monitormodel>> callBack)
        {
            if (callBack != null)
            {
                OnMonitorInfoCallBack -= callBack;
            }

            
            if (callBack == null)
            {
                string[] topics = new string[1];
                topics[0] = $"{ApiConsts.Topic_stc_MonitorInfo}";
                ApiManager.Instance.GetDefaultApi().UnSubScribe(ApiConsts.MonitorSvr, topics);
                isSubscribeForMonitor = false;
            }
        }

        #region 查询所有节点信息
        /// <summary>
        /// 查询Zookeeper所有的信息MonitorMsginfo
        /// </summary>
        /// <param name="onMsgCallBack"></param>
        public void SubscribeMonitorMsginfo(Action<Dictionary<string, Dictionary<string, monitormodel>>> callBack)
        {
            if (callBack != null)
            {
                OnMonitorMsgInfoCallBack += callBack;
            }

            lock (this)
            {
                if (!isSubscribeForMonitorMsg)
                {
                    string[] topics = new string[1];
                    topics[0] = $"{ApiConsts.Topic_stc_MonitorMsginfo}";
                    ApiManager.Instance.GetDefaultApi().SubScribeWithImage(DataBus.Instance.Instanse_OnMessage, topics, ApiConsts.MonitorSvr, string.Empty);

                    isSubscribeForMonitorMsg = true;
                }
            }
        }

        public void UnSubscribeMonitorMsginfo(Action<Dictionary<string, Dictionary<string, monitormodel>>> callBack)
        {
            if (callBack != null)
            {
                OnMonitorMsgInfoCallBack -= callBack;
            }

            //if (callBack == null)
            //{
                string[] topics = new string[1];
                topics[0] = $"{ApiConsts.Topic_stc_MonitorMsginfo}";
                ApiManager.Instance.GetDefaultApi().UnSubScribe(ApiConsts.MonitorSvr, topics);
                isSubscribeForMonitorMsg = false;
            //}
        }
        /// <summary>
        /// 缓存
        /// </summary>
        /// <param name="obj"></param>
        private void OnMonitorMsgInfo(object obj)
        {
            DataBus.Instance.add(obj);
        }

        public void ProcessMonitorMsgInfo(NetMessage netMsg)
        {
            if (netMsg != null)
            {
                if (netMsg.Topic.Contains(ApiConsts.Topic_stc_MonitorMsginfo))
                {
                    netMsg.Data = netMsg.Data.Replace("current connections", "currentconnections").Replace("current runinfo", "currentruninfo");
                    Dictionary<string, Dictionary<string, monitormodel>> mir = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, monitormodel>>>(netMsg.Data.ToString());
                    OnMonitorMsgInfoCallBack?.Invoke(mir);
                }
            }
        }
        #endregion


        /// <summary>
        /// 当日Zookeeper回调
        /// </summary>
        /// <param name="obj"></param>
        private void OnMonitorInfo(object obj)
        {
            DataBus.Instance.add(obj);
        }

        public void ProcessMonitorInfo(NetMessage netMsg)
        {
            if (netMsg != null)
            {
                if (netMsg.Topic.Contains(ApiConsts.Topic_stc_MonitorInfo))
                {
                    netMsg.Data = netMsg.Data.Replace("current connections", "currentconnections").Replace("current runinfo", "currentruninfo");
                    Dictionary<string, monitormodel> mir = JsonConvert.DeserializeObject<Dictionary<string, monitormodel>>(netMsg.Data.ToString());
                    OnMonitorInfoCallBack?.Invoke(mir);
                }
            }
        }

        /// <summary>
        /// 修改  req.actuib = "2"
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public PResult SetMonitorSvrinfo(MonitorInfoModel req)
        {
            return ApiManager.Instance.GetDefaultApi().Request<PResult>(ApiConsts.MonitorSvr, ApiConsts.Topic_stc_MonitorSvrInfo, req);
        }

        /// <summary>
        /// 查询最新的节点状态
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public LastZookeeperResult GetLastZookeeper(MonitorInfoReq req)
        {
            return ApiManager.Instance.GetDefaultApi().Request<LastZookeeperResult>(ApiConsts.MonitorSvr, ApiConsts.Topic_stc_MonitorSvrInfo, req);
        }

        /// <summary>
        /// 查询数据库节点信息  
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public MonitorInfoDetailResult QueryMonitorSvrinfo(MonitorInfoReq req)
        {
            return ApiManager.Instance.GetDefaultApi().Request<MonitorInfoDetailResult>(ApiConsts.MonitorSvr, ApiConsts.Topic_stc_MonitorSvrInfo, req);
        }
    }
}
