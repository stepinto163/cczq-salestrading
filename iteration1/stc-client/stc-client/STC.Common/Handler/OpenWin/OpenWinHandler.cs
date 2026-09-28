using com.gateway.log;
using netapi.Api;
using Newtonsoft.Json;
using STC.Common.Api;
using STC.Common.Model.OHLC;
using STC.Common.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace STC.Common.Handler.OpenWin
{
    public class OpenWinHandler : IOpenWinHandler
    {
        private OpenWinHandler() { }
        private static OpenWinHandler server = new OpenWinHandler();
        public static OpenWinHandler Instance { get { return server; } }

        /// <summary>
        /// key: securityID ;  value:回调函数
        /// </summary>
        private ConcurrentDictionary<string, Action<object>> _UpdateEmsOrderCallbackDic = new ConcurrentDictionary<string, Action<object>>();


        public void SubscribePriceWin(Action<object> callBack)
        {
            if (callBack != null)
            {
                string registerKey = $"priceWin#{callBack.Target}#{callBack.Method.Name}";

                if (_UpdateEmsOrderCallbackDic.ContainsKey("priceWin") == false)
                {
                    _UpdateEmsOrderCallbackDic["priceWin"] = callBack;

                    // 还没有注册， 先注册数据
                    RequestPriceWin();
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

        public void UnSubscribePriceWin(Action<object> callBack)
        {
            if (callBack != null)
            {
                string registerKey = $"priceWin#{callBack.Target}#{callBack.Method.Name}";

                if (_UpdateEmsOrderCallbackDic.ContainsKey("priceWin") == false)
                {
                }
                else
                {
                    int index = Array.IndexOf(_UpdateEmsOrderCallbackDic["priceWin"].GetInvocationList(), (Action<object>)callBack);
                    if (index < 0)
                    {
                        _UpdateEmsOrderCallbackDic["priceWin"] -= callBack;
                    }
                }
            }
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

        private void RequestPriceWin()
        {
            System.Threading.Tasks.Task.Factory.StartNew(() =>
            {
                ApiManager.Instance.GetDefaultApi().SubScribeWithImage(Instance_OnPriceWinMsg,
                    new string[] { $"L.{LocalEnv.UserInfo.UserName}.{ApiConsts.Topic_PriceWin}" }, ApiConsts.LocalSvr, "");
            });
        }

        private void Instan_OnClipboard(object obj)
        {
            var thread = new Thread(() =>
            {
                NetMessage netMsg = obj as NetMessage;
                if (netMsg != null)
                {
                    string topic = netMsg.Topic.ToString();
                    if (topic.StartsWith($"L.{LocalEnv.UserInfo.UserName}.clipboard"))
                    {
                        _UpdateEmsOrderCallbackDic["clipboard"]?.Invoke(netMsg.Data.ToString());
                    }
                }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.IsBackground = true;
            thread.Start();
        }


        //复制
        private void RequestClipboard()
        {
            System.Threading.Tasks.Task.Factory.StartNew(() =>
            {
                ApiManager.Instance.GetDefaultApi().SubScribe(Instan_OnClipboard,
                    new string[] { $"L.{LocalEnv.UserInfo.UserName}.{ApiConsts.Topic_Clipboard}" }, ApiConsts.LocalSvr, "");
            });
        }

        public void SubscribeClipboard(Action<object> callBack)
        {
            if (callBack != null)
            {
                string registerKey = $"clipboard#{callBack.Target}#{callBack.Method.Name}";

                if (_UpdateEmsOrderCallbackDic.ContainsKey("clipboard") == false)
                {
                    _UpdateEmsOrderCallbackDic["clipboard"] = callBack;

                    // 还没有注册， 先注册数据
                    RequestClipboard();
                }
                else
                {
                    int index = Array.IndexOf(_UpdateEmsOrderCallbackDic["clipboard"].GetInvocationList(), (Action<string>)callBack);
                    if (index < 0)
                    {
                        _UpdateEmsOrderCallbackDic["clipboard"] += callBack;
                    }
                }
            }
        }

        public void UnSubscribeClipboard(Action<object> callBack)
        {
            if (callBack != null)
            {
                string registerKey = $"clipboard#{callBack.Target}#{callBack.Method.Name}";

                if (_UpdateEmsOrderCallbackDic.ContainsKey("clipboard") == false)
                {
                }
                else
                {
                    int index = Array.IndexOf(_UpdateEmsOrderCallbackDic["clipboard"].GetInvocationList(), (Action<object>)callBack);
                    if (index < 0)
                    {
                        _UpdateEmsOrderCallbackDic["clipboard"] -= callBack;
                    }
                }
            }
        }
    }
}
