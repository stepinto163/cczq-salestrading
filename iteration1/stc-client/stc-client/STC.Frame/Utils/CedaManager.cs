using System;
using System.Collections.Generic;
using System.Threading;
using com.adaptiveMQ2.client;
using com.adaptiveMQ2.event_c;
using com.adaptiveMQ2.message;
using com.adaptiveMQ2.utils;
using log = IDC.Frame.Utils.LogHelper;

namespace IDC.Frame.Utils
{
    public class CedaManager : IEventListener, IMessageListener
    {
        public event Action<Message> OnCedaMessage;

        private ClientInfo clientInfo;

        private IClientConnection _conn;
        private IClientSession _session;
        private List<BaseDestination> topics = new List<BaseDestination>();
        private MessageRequestor requestor;
        private MessageSender sender;
        private string sid = "";

        public bool isStopRequest = false;

        public void StartDefaultServer()
        {

        }

        public bool IsConnected
        {
            get { return _conn != null && _conn.Connected; }
        }

        public void Connect(ClientInfo info)
        {

            try
            {
                log.Info(com.adaptiveMQ2.utils.Consts.VERSION);
                this.clientInfo = info;
                _conn = ClientConnectionFactory.createConnection(info);
                _conn.addEventListener(this);
                if (info.LoginMessage != null)
                {
                    _conn.setLoginListener(this);
                }
                _conn.start();
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }
        public void ReConnect()
        {
            if (!isStopRequest)
            {
                ThreadPool.QueueUserWorkItem(o =>
                                                 {
                                                     if (!isStopRequest)
                                                     {
                                                         Thread.Sleep(1000);
                                                         log.Info("ReConnect");
                                                         Connect(this.clientInfo);
                                                         if (IsConnected)
                                                         {
                                                             log.Info("ReSubscribe Topic");
                                                             _session = _conn.createSession();
                                                             _session.subscribe(topics, this);

                                                         }
                                                     }
                                                 });
            }
        }

        public void Disconnect()
        {
            isStopRequest = true;

            if (_session != null)
            {
                _session = null;
                topics.Clear();
            }
            if (_conn != null)
            {
                try
                {

                    _conn.close();
                    _conn.stop();
                    _conn.removeEventListener(this);
                    _conn = null;
                }
                catch (Exception ex)
                {
                    log.Error(ex);
                }
            }
        }

        public void Subscribe(string topic)
        {
            List<string> topics = new List<string>() { topic };
            Subscribe(topics);
        }

        public void Subscribe(List<string> topic)
        {
            if (topic == null || topic.Count == 0)
            {
                log.Warn("Topic is null,so can't subscribe!");
                return;
            }
            foreach (string t in topic)
            {
                topics.Add(new BaseDestination(t));
            }
            if (_conn != null && _conn.Connected)
            {
                _session = _conn.createSession();
                _session.subscribe(topics, this);
            }
        }

        public void SubscribeWithImage(string topic, string svrId)
        {
            List<string> topics = new List<string>() { topic };
            SubscribeWithImage(topics, svrId, sid);
        }

        public void SubscribeWithImage(List<string> topic, string svrId, string signalId)
        {
            if (topic == null || topic.Count == 0)
            {
                log.Warn("Topic is null,so can't subscribeWithImage!");
                return;
            }
            foreach (string t in topic)
            {
                topics.Add(new BaseDestination(t));
            }
            if (_conn != null && _conn.Connected)
            {
                _session = _conn.createSession();
                _session.subscribe(topics, svrId, signalId, this);
            }
        }

        public void SendMessage(Message msg)
        {
            if (_conn != null && _conn.Connected)
            {
                _session = _conn.createSession();
                
                sender = _session.createProducer(msg.Destination);
                msg.SignalID = sid;
                sender.send(msg);
            }
        }

        public Message Request(Message reqMsg)
        {
            Message replyMsg = null;
            if (IsConnected)
            {
                _session = _conn.createSession();
                requestor = _session.createRequest();
                reqMsg.SignalID = sid;
                replyMsg = requestor.request(reqMsg, 10000);
            }
            return replyMsg;
        }

        #region IMessageListener 成员

        public void onMessage(com.adaptiveMQ2.message.Message msg)
        {
            string stopic = msg.Destination.getName();
            if (stopic.Equals(ConstsMessage.TOPIC_ATS_LOGIN))
            {
                Dictionary<string, object> map = CedaObject.GetLoginResult(msg);
                if (map != null)
                {
                    foreach (string key in map.Keys)
                    {
                        switch (key.ToLower())
                        {
                            case "sid":
                                sid = map[key] + "";
                                if (clientInfo!=null)
                                {
                                    clientInfo.SID = sid;
                                }
                                break;
                        }
                    }
                }
            }

            if (OnCedaMessage != null)
            {
                OnCedaMessage(msg);
            }
        }

        #endregion

        #region  IEventListener
        public override void onEvent(int nCode)
        {
            switch (nCode)
            {
                case IEventListener.CONNECTION_CONNECTING:
                    log.Info("Event: CONNECTION_CONNECTING");
                    break;
                case IEventListener.CONNECTION_CONNECTED:
                    log.Info("Event: CONNECTION_CONNECTED");
                    break;
                case IEventListener.CONNECTION_CLOSED:
                    log.Info("Event: CONNECTION_CLOSED");
                    ReConnect();
                    break;
                case IEventListener.CONNECTION_RECONNECT:
                    log.Info("Event: CONNECTION_RECONNECTED");
                    break;
                case IEventListener.CONNECTION_LOGINING:
                    log.Info("Event: CONNECTION_LOGINING");
                    break;
                case IEventListener.CONNECTION_LOGIN_SUCCESS:
                    log.Info("Event: CONNECTION_LOGIN_SUCCESS");
                    break;
                case IEventListener.CONNECTION_IO_EXCEPTION:
                    log.Info("Event: CONNECTION_IO_EXCEPTION");
                    break;
                case IEventListener.CONNECTION_LOST:
                    log.Info("Event: CONNECTION_LOST");
                    break;
                case IEventListener.CONNECTION_TIMEOUT:
                    log.Info("Event: CONNECTION_TIMEOUT");
                    break;
            }
        }
        #endregion       
    }
}