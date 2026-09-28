using netapi.Api;
using STC.Common.Api;
using STC.Common.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BDB.Price.Plugin.Handler
{
    public class OpenWinHandler
    {
        private static OpenWinHandler server = new OpenWinHandler();
        public static OpenWinHandler Instance { get { return server; } }

        private ConcurrentDictionary<string, Action<object>> SendQQMsgCallbackDic = new ConcurrentDictionary<string, Action<object>>();

        /// <summary>
        /// key: securityID ;  value:回调函数
        /// </summary>
        private ConcurrentDictionary<string, Action<object>> _UpdateEmsOrderCallbackDic = new ConcurrentDictionary<string, Action<object>>();

        public OpenWinHandler()
        {

        }

        public void SubscribeSendQQMsg(Action<object> callBack)
        {
            if (callBack != null)
            {
                string registerKey = $"SendQQMsg#{callBack.Target}#{callBack.Method.Name}";

                if (SendQQMsgCallbackDic.ContainsKey("SendQQMsg") == false)
                {
                    SendQQMsgCallbackDic["SendQQMsg"] = callBack;

                    // 还没有注册， 先注册数据
                    RequestPriceWin();
                }
                else
                {
                    int index = Array.IndexOf(SendQQMsgCallbackDic["SendQQMsg"].GetInvocationList(), (Action<string>)callBack);
                    if (index < 0)
                    {
                        SendQQMsgCallbackDic["SendQQMsg"] += callBack;
                    }
                }
            }
        }

        private void RequestPriceWin()
        {
            System.Threading.Tasks.Task.Factory.StartNew(() =>
            {
                ApiManager.Instance.GetDefaultApi().SubScribeWithImage(Instance_SendQQMsg,
                    new string[] { $"L.{LocalEnv.UserInfo.UserName}.{ApiConsts.Topic_SendQQMsg}" }, ApiConsts.LocalSvr, "");
            });
        }

        private void Instance_SendQQMsg(object obj)
        {
            System.Threading.Tasks.Task.Factory.StartNew(() =>
            {
                NetMessage netMsg = obj as NetMessage;
                if (netMsg != null)
                {
                    string topic = netMsg.Topic.ToString();
                    if (topic.StartsWith($"L.{LocalEnv.UserInfo.UserName}.SendQQMsg"))
                    {
                        SendQQMsgCallbackDic["SendQQMsg"]?.Invoke(netMsg.Data.ToString());
                    }
                }
            });
        }

        /// <summary>
        /// 订阅Excel打开小窗事件
        /// </summary>
        /// <param name="callBack"></param>
        public void SubscribePriceWin(Action<object> callBack)
        {
            if (callBack != null)
            {
                string registerKey = $"priceWin#{callBack.Target}#{callBack.Method.Name}";

                if (_UpdateEmsOrderCallbackDic.ContainsKey("priceWin") == false)
                {
                    _UpdateEmsOrderCallbackDic["priceWin"] = callBack;

                    // 还没有注册， 先注册数据
                    RequestOpenPriceWin();
                }
                else
                {
                    int index = Array.IndexOf(_UpdateEmsOrderCallbackDic["priceWin"].GetInvocationList(), (Action<string>)callBack);
                    if (index < 0)
                    {
                        _UpdateEmsOrderCallbackDic["priceWin"] += callBack;
                    }
                }
            }
        }


        private void RequestOpenPriceWin()
        {
            System.Threading.Tasks.Task.Factory.StartNew(() =>
            {
                ApiManager.Instance.GetDefaultApi().SubScribeWithImage(Instance_OnPriceWinMsg,
                    new string[] { $"L.{LocalEnv.UserInfo.UserName}.{ApiConsts.Topic_PriceWin}" }, ApiConsts.LocalSvr, "");
            });
        }

        private void Instance_OnPriceWinMsg(object obj)
        {
            System.Threading.Tasks.Task.Factory.StartNew(() =>
            {
                NetMessage netMsg = obj as NetMessage;
                if (netMsg != null)
                {
                    string topic = netMsg.Topic.ToString();
                    if (topic.StartsWith($"L.{LocalEnv.UserInfo.UserName}.priceWin"))
                    {
                        _UpdateEmsOrderCallbackDic["priceWin"]?.Invoke(netMsg.Data.ToString());
                    }
                }
            });
        }
    }
}
