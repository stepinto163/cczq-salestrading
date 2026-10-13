using Ats.Foundation.Message;
using com.gateway.connector;
using com.gateway.connector.tcp;
using com.gateway.log;
using common.netapi.Api;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace netapi.Api
{
    public class GWApi : AbstractApi
    {
        private string _apiKey = "1";
        public override string ApiKey { get { return _apiKey; } set { _apiKey = value; } }

        protected GateWayApi _gateWayApi = null;
        protected Dictionary<string, IMessage> _notifyDic = null;

        ConcurrentDictionary<string, Action<object>> _dic = new System.Collections.Concurrent.ConcurrentDictionary<string, Action<object>>();

        public event Action<object> OnData;
        public event Action<object> OnConnectStatus;

        private JsonSerializerSettings _jsonSerializerSettings = null;
        public GWApi()
        {
            _jsonSerializerSettings = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };
        }


        #region IApi
        public override void NewApi()
        {
            _gateWayApi = new GateWayApi();
            _notifyDic = new Dictionary<string, IMessage>();
            GWEventlistener eventlistener = new GWEventlistener();
            eventlistener.OnEventChange += Eventlistener_OnEventChange;
            _gateWayApi.Eventlistener = eventlistener;

            _gateWayApi.isCacheImage = true;
            List<string> lt = new List<string>();
            lt.Add("stc.md");
            lt.Add("stc.tbook.t");
            lt.Add("stc.tbook.systemparas");
            lt.Add("stc.ohlc");
            _gateWayApi.cacheImageTopics.AddRange(lt);
        }

        public override object GetApiConnect()
        {
            return _gateWayApi;
        }

        public override void Init()
        { }

        public override void SetGzip(bool isGzip)
        {
            _gateWayApi.IsGzip = isGzip;
        }

        public override void SubScribeConStatus(Action<object> callBack)
        {
            if (callBack != null && (OnConnectStatus == null || Array.IndexOf(OnConnectStatus.GetInvocationList(), callBack) < 0))
            {
                OnConnectStatus += callBack;

            }
        }

        public override void UnSubScribeConStatus(Action<Object> callBack)
        {
            if (callBack != null && (OnConnectStatus == null || Array.IndexOf(OnConnectStatus.GetInvocationList(), callBack) > 0))
            {
                OnConnectStatus -= callBack;

            }
        }



        private void Eventlistener_OnEventChange(int statusCode)
        {
            //NetMessage stcMessage = new NetMessage();
            //stcMessage.Data = statusCode.ToString();
            //stcMessage.Topic = BaseConsts.CONNECT_STATUS_TOPIC;

            //OnData?.Invoke(stcMessage);
            
            switch (statusCode)
            {
                case IEventListener.CONNECTION_CLOSED:
                    Console.WriteLine("CONNECTION_CLOSED");
                    break;

                case IEventListener.CONNECTION_LOGIN_SUCCESS:
                    Console.WriteLine("CONNECTION_LOGIN_SUCCESS");
                    break;

                case IEventListener.CONNECTION_SUCCESS:
                    Console.WriteLine("CONNECTION_SUCCESS");
                    break;

                case IEventListener.CONNECTION_LOGOUT:
                    Console.WriteLine("CONNECTION_LOGOUT");
                    break;
            }
            OnConnectStatus?.Invoke(statusCode);
        }

        public override void DealMessage(object topic, object data)
        {
            //ProcessMsg(topic as string, data as string);
        }

        public override object Login(LoginParam loginParam, ILoginHandler loginHandler = null)
        {
            LocalIntegrationProfile profile = LocalIntegrationProfile.Current;
            if (profile.Enabled)
            {
                profile.ValidateGateway(loginParam.Host);
                if (loginParam.isEncrypt)
                {
                    throw new InvalidOperationException("Local simulator login expects an unencrypted test password.");
                }
                LogHelper.Info("LOCAL LOGIN host={0}, port={1}, user={2}", loginParam.Host, loginParam.Port, loginParam.UserName);
                _gateWayApi.IsGzip = false;
                string response = _gateWayApi.connect(loginParam.Host, loginParam.Port,
                    loginParam.UserName, loginParam.Pwd, false, 1, new Dictionary<string, object>());
                string localReplyText = profile.CompleteLogin(response, loginParam.UserName);
                var reply = Newtonsoft.Json.Linq.JObject.Parse(localReplyText);
                if ((string)reply["Code"] == "0001")
                {
                    UserName = loginParam.UserName;
                    Sid = (string)reply["sid"];
                }
                return localReplyText;
            }
            LogHelper.Info("Login:{0}", loginParam.ToString());
            
            object result = loginHandler.Login(loginParam.UserName, loginParam.Pwd, loginParam.isEncrypt,
                loginParam.Host, loginParam.Port.ToString(), loginParam.Protocal, loginParam.OtherParam);
            

            return result;




            /**


            int intProtocal = 0;

            if (loginParam.Protocal == "TCP" || loginParam.Protocal == "PROTOCOL_TCP")
            {
                intProtocal = 1;
            }
            else if (loginParam.Protocal == "TCPS" || loginParam.Protocal == "PROTOCOL_TCPS")
            {
                intProtocal = 2;
            }
            else if (loginParam.Protocal == "HTTP" || loginParam.Protocal == "PROTOCOL_HTTP")
            {
                intProtocal = 3;
            }
            else if (loginParam.Protocal == "HTTPS" || loginParam.Protocal == "PROTOCOL_HTTPS")
            {
                intProtocal = 4;
            }
            //if (loginParam.isGzip)
            //    _gateWayApi.IsGzip = true;
            //else
            //    _gateWayApi.IsGzip = false;

            Dictionary<string, object> extendParams = new Dictionary<string, object>();
            extendParams.Add("isManage", "N");// 客户端登录 Y-管理端 , N-客户端
            extendParams.Add("mac", loginParam.Mac);


            string pwd = loginParam.Pwd;
            if (!loginParam.isEncrypt)
            {
                //外围系统登录验证方式
                pwd = EncrypterHelper.AesEncrypt(loginParam.Pwd, null);
            }
            extendParams.Add("password", pwd);
            extendParams.Add("isEncrypt", "Y");


            //内部系统登录验证方式
            Dictionary<string, object> pwdDic = null;
            if (loginParam.isEncrypt)
            {
                pwdDic = JsonConvert.DeserializeObject<Dictionary<string, object>>(loginParam.Pwd);//MsgHelper.Deserialize<Dictionary<string, object>>(loginParam.Pwd);
            }
            else
            {
                pwdDic = EncrypterHelper.getEncryptDic(loginParam.Pwd);
            }

            if (pwdDic != null && pwdDic.Count == 2)
            {
                if (extendParams.ContainsKey("isEncrypt"))
                    extendParams.Remove("isEncrypt");
                extendParams.Add("isEncrypt", "Y");
                foreach (var key in pwdDic.Keys)
                {
                    if (extendParams.ContainsKey(key))
                        extendParams.Remove(key);
                    extendParams.Add(key, pwdDic[key]);
                }


            }


            //LogHelper.Info("Login:{0}", loginParam.ToString());
            Console.WriteLine("Login:{0}", loginParam.ToString());
            string loginResult = _gateWayApi.connect(loginParam.Host, loginParam.Port, loginParam.UserName, loginParam.Pwd, loginParam.isEncrypt, intProtocal, extendParams);
            //NetapiLoginResult result = string.IsNullOrEmpty(loginResult) ? null : MsgHelper.Deserialize<NetapiLoginResult>(loginResult);

            return loginResult;

            **/
        }

        public override T Request<T>(string svrId, string topic, object paras)
        {
            LocalIntegrationProfile profile = LocalIntegrationProfile.Current;
            if (!profile.UsesRealService(svrId, topic))
            {
                return profile.Reply<T>(svrId, topic, paras, UserName);
            }
            string content = string.Empty;
            if (paras is string)
                content = paras as string;
            else
                content = string.IsNullOrEmpty(content) ? JsonConvert.SerializeObject(paras, _jsonSerializerSettings) : content;

            // 发布请求可能带内部服务凭据；插件层已有脱敏业务日志，底层不重复输出原文。
            string loggedContent = topic == "ficc.qtrade.quote.publish" ? "[publish request omitted]" : content;
            LogHelper.Info(string.Format("Request:{0},{1},{2}", svrId, topic, loggedContent));
            T t = _gateWayApi.requestSync<T>(svrId, topic, content);

            string result = t != null?JsonConvert.SerializeObject(t):string.Empty;

            LogHelper.Info(string.Format("Result:{0},{1},{2}", svrId, topic, (result.Length > 2000 ? (result.Substring(0, 2000) + "... too long"):result)));

            return t;
        }

        public override T Request<T>(string svrId, string topic, object paras, int waitTime)
        {
            LocalIntegrationProfile profile = LocalIntegrationProfile.Current;
            if (!profile.UsesRealService(svrId, topic))
            {
                return profile.Reply<T>(svrId, topic, paras, UserName);
            }
            string content = string.Empty;
            if (paras is string)
                content = paras as string;
            else
                content = string.IsNullOrEmpty(content) ? JsonConvert.SerializeObject(paras, _jsonSerializerSettings) : content;

            string loggedContent = topic == "ficc.qtrade.quote.publish" ? "[publish request omitted]" : content;
            LogHelper.Info(string.Format("Request:{0},{1},{2},{3}", svrId, topic, loggedContent, waitTime));
            T t = _gateWayApi.requestSync<T>(svrId, topic, content, waitTime);

            string result = t != null ? JsonConvert.SerializeObject(t) : string.Empty;

            LogHelper.Info(string.Format("Result:{0},{1},{2}", svrId, topic, (result.Length > 2000 ? (result.Substring(0, 2000) + "... too long") : result)));

            return t;
        }


        public override void SubScribeWithImage(Action<object> callBack, string[] topics, string svrId, string signalID)
        {
            //if (callBack != null && (OnData == null || Array.IndexOf(OnData.GetInvocationList(), callBack) < 0))
            //{
            //    OnData += callBack;
            //}



            DealSubscribe(callBack, topics, svrId, true);
        }

        public override void SubScribe(Action<object> callBack, string[] topics, string svrId, string signalID)
        {
            DealSubscribe(callBack, topics, svrId, false);
        }


        private void DealSubscribe(Action<object> callBack, string[] topics, string svrId, bool withImage)
        {
            if (topics != null)
            {
                lock (_notifyDic)
                {
                    foreach (string topic in topics)
                    {
                        LocalIntegrationProfile profile = LocalIntegrationProfile.Current;
                        if (!profile.UsesRealService(svrId, topic))
                        {
                            profile.Subscribe(svrId, topic, UserName);
                            continue;
                        }
                        if (!_notifyDic.ContainsKey(topic))
                        {
                            GWNotifyMessage message = new GWNotifyMessage(LogFilterTopList);
                            message.OnMsg += callBack;
                            if (withImage)
                                _gateWayApi.subscribe(svrId, topic, message);
                            else
                                _gateWayApi.subscribe(topic, message);

                            _notifyDic.Add(topic, message);

                            LogHelper.Info(string.Format("{0}:{1},{2}", withImage ? "SubScribeWithImage" : "SubScribe", svrId, topic));

                            //LogHelper.Info(string.Format("SubScribeWithImage:{0},{1}", svrId, topic));
                        }
                    }
                }

            }
        }

        public override void UnSubScribe(string svrId, string[] topics)
        {
            if (topics != null)
            {
                lock (_notifyDic)
                {
                    foreach (string topic in topics)
                    {
                        if (_notifyDic.ContainsKey(topic))
                        {

                            bool isRemove = _gateWayApi.unSubscribe(topic, _notifyDic[topic]);
                            _notifyDic.Remove(topic);

                            LogHelper.Info(string.Format("UnSubScribe:{0},{1}", svrId, topic));
                        }
                    }
                }
            }
        }
        #endregion




        private void ProcessMsg(string topic, string data)
        {
            NetMessage stcMessage = new NetMessage();

            bool needLog = true;
            if (LogFilterTopList != null && LogFilterTopList.Count > 0)
            {
                foreach (string filterTopic in LogFilterTopList)
                {
                    if (topic.StartsWith(filterTopic))
                    {
                        needLog = false;
                    }
                }
            }
            
            if (needLog)
            {
                LogHelper.Info("topic:{0}, body:{1}", topic, data);
            }

            stcMessage.Topic = topic;
            stcMessage.Data = data;

            OnData?.Invoke(stcMessage);

        }


        public override void DisConnect()
        {
            if (_notifyDic != null)
                _notifyDic.Clear();

            if (OnData != null)
                OnData = null;

            if (OnConnectStatus != null)
                OnConnectStatus = null;

            if (_gateWayApi != null)
                _gateWayApi.disConnect();
        }


    }

    public class GWNotifyMessage : IMessage
    {
        List<string> LogFilterTopList;

        public event Action<object> OnMsg;

        public GWNotifyMessage(List<string> logFilterTopList)
        {
            LogFilterTopList = logFilterTopList;
        }

        public void onMessage(string topic, string t)
        {
            ProcessMsg(topic, t);
        }

        private void ProcessMsg(string topic, string data)
        {
            NetMessage stcMessage = new NetMessage();

            bool needLog = true;
            if (LogFilterTopList != null && LogFilterTopList.Count > 0)
            {
                foreach (string filterTopic in LogFilterTopList)
                {
                    if (topic.StartsWith(filterTopic))
                    {
                        needLog = false;
                    }
                }
            }

            if (needLog)
            {
                LogHelper.Info("topic:{0}, body:{1}", topic, data);
            }

            stcMessage.Topic = topic;
            stcMessage.Data = data;

            OnMsg?.Invoke(stcMessage);

        }
    }

    public class GWEventlistener : IEventListener
    {
        public event Action<int> OnEventChange;

        public override void onEvent(int code)
        {

            switch (code)
            {
                case IEventListener.CONNECTION_CLOSED:
                    Console.WriteLine("CONNECTION_CLOSED");
                    OnEventChange?.Invoke(code);
                    break;

                case IEventListener.CONNECTION_LOGIN_SUCCESS:
                    Console.WriteLine("CONNECTION_LOGIN_SUCCESS");
                    OnEventChange?.Invoke(code);
                    break;

                case IEventListener.CONNECTION_SUCCESS:
                    Console.WriteLine("CONNECTION_SUCCESS");
                    OnEventChange?.Invoke(code);
                    break;

                case IEventListener.CONNECTION_LOGOUT:
                    Console.WriteLine("CONNECTION_LOGOUT");
                    OnEventChange?.Invoke(code);
                    break;
            }

        }
    }
}
