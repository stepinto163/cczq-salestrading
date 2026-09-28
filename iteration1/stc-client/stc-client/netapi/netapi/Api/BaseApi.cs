using Ats.Foundation.Message;
using com.gateway.connector;
using com.gateway.connector.tcp;
using com.gateway.log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace netapi.Api
{
    public class BaseApi
    {
        protected GateWayApi _gateWayApi = null;
        protected Dictionary<string, IMessage> _notifyDic = null;
        public int TimeOut { get; set; }

        public event Action<int> OnConStatusChange;
        

        public void Init()
        {
            _gateWayApi = new GateWayApi();
            _notifyDic = new Dictionary<string, IMessage>();
            Eventlistener eventlistener = new Eventlistener();
            eventlistener.OnEventChange += Eventlistener_OnEventChange;
            _gateWayApi.Eventlistener = eventlistener;
        }

        private void Eventlistener_OnEventChange(int statusCode)
        {
            OnConStatusChange?.Invoke(statusCode);
        }

        /// <summary>
        /// 登录
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        /// <param name="name"></param>
        /// <param name="pwd"></param>
        /// <param name="isEncryption"></param>
        /// <param name="protocal"></param>
        /// <returns></returns>
        public string Connect(string ip, int port, string name, string pwd, bool isEncryption, int protocal)
        {
            return _gateWayApi.connect(ip, port, name, pwd, isEncryption, protocal);
        }

        /// <summary>
        /// 请求响应
        /// </summary>
        public T Request<T>(string serverName, string methodName, string content)
        {
            return _gateWayApi.requestSync<T>(serverName, methodName, content);
        }


        public void SubScribe(string svrId, string[] topics)
        {
            if (topics != null)
            {
                foreach (string topic in topics)
                {
                    if (!_notifyDic.ContainsKey(topic))
                    {
                        NotifyMessage message = new NotifyMessage();
                        message.OnMsg += OnMessage;
                        _gateWayApi.subscribe(topic, message);
                        _notifyDic.Add(topic, message);

                        LogHelper.Info(string.Format("SubScribe:{0},{1}", svrId, topic));
                    }
                }
                
            }
        }

        public void SubScribeWithImage(string[] topics, string svrId, string signalID)
        {
            if (topics != null)
            {
                foreach (string topic in topics)
                {
                    if (!_notifyDic.ContainsKey(topic))
                    {
                        NotifyMessage message = new NotifyMessage();
                        message.OnMsg += OnMessage;
                        _gateWayApi.subscribe(svrId, topic, message);
                        _notifyDic.Add(topic, message);

                        LogHelper.Info(string.Format("SubScribeWithImage:{0},{1}", svrId, topic));
                    }
                }

            }
        }

        public void UnSubScribe(string svrId, string[] topics)
        {
            if (topics != null)
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

        public void OnMessage(string topic, string data)
        {
            DealMsg(topic, data);
        }

        public virtual void DealMsg(string topic, string data)
        {

        }
    }

    public class NotifyMessage : IMessage
    {
        public event Action<string, string> OnMsg;

        public void onMessage(string topic, string t)
        {
            OnMsg?.Invoke(topic, t);
            Console.WriteLine("--------------------------------topic:" + topic + ",content:" + t);
        }
    }

    public class Eventlistener : IEventListener
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

            }

        }
    }
}
