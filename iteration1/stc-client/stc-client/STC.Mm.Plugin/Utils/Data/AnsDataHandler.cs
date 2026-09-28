using com.gateway.log;
using netapi.Api;
using STC.Common.Api;
using STC.Common.Handler.Ans;
using STC.Common.Handler.Base;
using STC.Common.Model.Ans;
using STC.Common.Model.Fix.Message;
using STC.Common.Utils;
using STC.Frame.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace STC.Mm.Plugin.Utils.Data
{
    public class AnsDataHandler
    {
        // 订阅做市义务债券详情
        private readonly Object m_OnAnsMonitorInfoEventLock = new Object();
        private Action<ANSMonitorInfo> _OnAnsMonitorInfo;
        public event Action<ANSMonitorInfo> OnAnsMonitorInfo
        {
            add
            {
                lock (m_OnAnsMonitorInfoEventLock)
                {
                    if (_OnAnsMonitorInfo == null || !_OnAnsMonitorInfo.GetInvocationList().Contains(value))
                        _OnAnsMonitorInfo += value;
                }
                System.Threading.ThreadPool.QueueUserWorkItem(o =>
                {
                    foreach (var obj in _CacheAnsMonitorInfoDic)
                    {
                        _OnAnsMonitorInfo?.Invoke(obj.Value);
                    }
                });
            }
            remove
            {
                lock (m_OnAnsMonitorInfoEventLock)
                {
                    _OnAnsMonitorInfo -= value;
                }
            }
        }

        /// <summary>
        /// 缓存做市义务债券详情
        /// </summary>
        private ConcurrentDictionary<string, ANSMonitorInfo> _CacheAnsMonitorInfoDic = new ConcurrentDictionary<string, ANSMonitorInfo>();


        // 订阅做市义务统计
        private readonly Object m_OnAnsSummaryEventLock = new Object();
        private Action<ANSSummary> _OnAnsSummary;
        public event Action<ANSSummary> OnAnsSummary
        {
            add
            {
                lock (m_OnAnsSummaryEventLock)
                {
                    if (_OnAnsSummary == null || !_OnAnsSummary.GetInvocationList().Contains(value))
                        _OnAnsSummary += value;
                }
                if (_CacheAnsSummary != null)
                {
                    System.Threading.ThreadPool.QueueUserWorkItem(o =>
                    {
                        _OnAnsSummary?.Invoke(_CacheAnsSummary);
                    });
                }

            }
            remove
            {
                lock (m_OnAnsSummaryEventLock)
                {
                    _OnAnsSummary -= value;
                }
            }
        }

        private ANSSummary _CacheAnsSummary = null;

        /// <summary>
        /// 构造函数
        /// </summary>
        private AnsDataHandler()
        {
            BaseHandler.Instance.Init(Instance_OnClear);
        }

        private volatile bool isInit = false;

        /// <summary>
        /// 初始化
        /// </summary>
        public void init()
        {
            if (isInit == false)
            {
                AnsHandler.Instance.SubscribeAnsSummary(OnSubscribeAnsSummary);

                AnsHandler.Instance.SubscribeAnsMonitorInfo(Instance_OnANSMonitorInfo, new List<string>() { "*" });

                //RequestAnsMonitorInfo();

                isInit = true;
            }
        }

        private void OnSubscribeAnsSummary(ANSSummary obj)
        {
            _CacheAnsSummary = obj;
            _OnAnsSummary?.Invoke(obj);
        }

        /// <summary>
        /// 清零数据回调方法
        /// </summary>
        /// <param name="obj"></param>
        private void Instance_OnClear(Common.Model.Base.ClearPo obj)
        {
            LogHandler.Info("AnsDataHandler Clear");
            if (obj != null && "Y".Equals(obj.IsClear))
            {
                _CacheAnsMonitorInfoDic.Clear();
                isInit = false;

                ThreadPool.QueueUserWorkItem(o =>
                {
                    try
                    {
                        init();
                    }
                    catch (Exception e)
                    {
                        LogHelper.Error(e);
                    }
                });
            }
        }

        /// <summary>
        /// 做市业务债券详细信息回调方法
        /// </summary>
        /// <param name="obj"></param>
        private void Instance_OnANSMonitorInfo(ANSMonitorInfo obj)
        {
            string primarykey = obj.securityId;
            lock (string.Intern(primarykey))
            {
                if (_CacheAnsMonitorInfoDic.ContainsKey(primarykey))
                {
                    string oldUpdateTime = _CacheAnsMonitorInfoDic[primarykey].updateTime;
                    if (oldUpdateTime.CompareTo(obj.updateTime) > 0)
                    {
                        // ignore
                    }
                    else
                    {
                        _CacheAnsMonitorInfoDic[primarykey] = obj;
                    }
                }
                else
                {
                    _CacheAnsMonitorInfoDic[primarykey] = obj;
                }
            }
            _OnAnsMonitorInfo?.Invoke(_CacheAnsMonitorInfoDic[primarykey]);

        }

        /// <summary>
        /// 测试方法
        /// </summary>
        /// <param name="obj"></param>
        public void TestOnAnsMonitorInfo(ANSMonitorInfo obj)
        {
            this.Instance_OnANSMonitorInfo(obj);
        }

        /// <summary>
        /// AnsDataHandler实例
        /// </summary>
        private static AnsDataHandler server = new AnsDataHandler();
        public static AnsDataHandler Instance { get { return server; } }

        /// <summary>
        /// 请求做市业务债券详细信息
        /// </summary>
        private void RequestAnsMonitorInfo(string securitys)
        {
            System.Threading.ThreadPool.QueueUserWorkItem(o =>
            {
                try
                {
                    ///获取做市业务债券详细信息
                    QueryAnsMonitorReplyPo reply = AnsHandler.Instance.QueryAnsMonitorInfo(LocalEnv.UserInfo.UserName, securitys);

                    if (reply != null && reply.Datas != null)
                    {
                        foreach (ANSMonitorInfo obj in reply.Datas)
                        {
                            Instance_OnANSMonitorInfo(obj);
                        }
                    }
                }
                catch (Exception e)
                {
                    LogHelper.Error(e);
                }
            });
        }

        /// <summary>
        /// 获取做市业务债券详细信息
        /// </summary>
        /// <param name="securityID"></param>
        /// <param name="settlType"></param>
        /// <returns></returns>
        public void GetMonitorInfo(List<string> securitysList)
        {
            StringBuilder securitySB = new StringBuilder();
            if (securitysList != null && securitysList.Count > 0)
            {
                foreach(string securityID in securitysList)
                {
                    if (_CacheAnsMonitorInfoDic.ContainsKey(securityID) == false)
                    {
                        if (securitySB.Length > 0)
                        {
                            securitySB.Append(CommonConstants.COMMA_CHAR);
                        }
                        securitySB.Append(securityID);
                    }
                    else
                    {
                        _OnAnsMonitorInfo?.Invoke(_CacheAnsMonitorInfoDic[securityID]);
                    }
                }
            }
            
            if (securitySB.Length > 0)
            {
                string securitys = securitySB.ToString();
                System.Threading.ThreadPool.QueueUserWorkItem(o =>
                {
                    try
                    {
                        RequestAnsMonitorInfo(securitys);
                    }
                    catch (Exception e)
                    {
                        LogHelper.Error(e);
                    }
                });
            }
        }
    }
}
