using System;
using System.Collections;

using Atf.MQIII;
using Ats.Foundation.Message;
using Ats.Foundation.Message.Serializer;
using com.adaptiveMQ2.message;
using STC.Frame.Utils;
using STC.Common.Utils;

namespace STC.XBond.Plugin.ServerApi
{

    public class BaseCEDAServer
    {
        private int timeOut = 30000;
        public int TimeOut
        {
            get { return timeOut; }
            set
            {
                timeOut = value;
            }
        }
        #region IBaseDALServer


        private string guid = Guid.NewGuid().ToString();

        public event Action<object> OnCEDAData;
        protected void onData(object o)
        {
            if (OnCEDAData != null && o != null)
            {
                OnCEDAData(o);
            }
        }

        #endregion

        #region IMessageProcessor 成员

        public void ProcessMessage(com.adaptiveMQ2.message.Message msg)
        {
            try
            {
                AtsMsg<object> o = CEDAToAtsMsg(msg);
                if (o != null)
                    onData(o.Body);
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
            }

        }

        #endregion

        #region 辅助方法
        private ArrayList al = ArrayList.Synchronized(new ArrayList());
        public void SubScribe(string svrId, string[] topics)
        {
            if (topics != null)
            {
                LogHelper.Debug(string.Format("Type:{2},RegisterHandler:[svrId:{0},guid:{1}]", svrId, guid, this.GetType()));

                MessageHandler handler = MQIIIManager.Instance.RegisterHandler(svrId, guid);
                bool isExist = false;
                foreach (var a in al)
                {
                    if (a == handler)
                    {
                        isExist = true;
                    }
                }
                if (!isExist)
                {
                    al.Add(handler);
                    handler.MQMessage += handler_MQMessage;
                }

                if (handler != null)
                {
                    handler.SubscribeTopics(topics);
                }
            }
        }
        void handler_MQMessage(string appName, string topic, com.adaptiveMQ2.message.Message msg)
        {
            ProcessMessage(msg);
        }
        /// <summary>
        /// 退订topic
        /// </summary>
        /// <param name="topic">topic 字符串</param>
        public void UnSubScribe(string svrId, string[] topics)
        {
            if (topics != null)
            {
                MessageHandler handler = MQIIIManager.Instance.RegisterHandler(svrId, guid);
                if (handler != null)
                {
                    handler.UnsubscribeTopics(topics);
                    foreach (var topic in topics)
                    {
                        LogHelper.Info(string.Format("UnSubScribe:{0},{1}", svrId, topic));
                    }

                }
            }
        }

        public void SubScribeWithImage(string[] topics, string svrId, string signalID)
        {
            if (topics != null)
            {
                LogHelper.Debug(string.Format("Type:{2},RegisterHandler:[svrId:{0},guid:{1}]", svrId, guid, this.GetType()));

                MessageHandler handler = MQIIIManager.Instance.RegisterHandler(svrId, guid);
                bool isExist = false;
                foreach (var a in al)
                {
                    if (a == handler)
                    {
                        isExist = true;
                    }
                }
                if (!isExist)
                {
                    al.Add(handler);
                    handler.MQMessage += handler_MQMessage;
                }

                if (handler != null)
                {
                    handler.subscribe(topics, svrId, signalID);
                    foreach (var topic in topics)
                    {
                        LogHelper.Info(string.Format("SubScribe:{0},{1}", svrId, topic));
                    }
                }
            }
        }

        public virtual AtsMsg<object> CEDAToAtsMsg(Message msg)
        {
            AtsMsg<object> o = null;
            return o;
        }

        #endregion
        protected void doMsg(com.adaptiveMQ2.message.Message msg)
        {
            LogHelper.Debug(msg.Destination);
            MessageBody oMessageBody = msg.MessageBody;
            try
            {
                IEnumerator it = oMessageBody.Values;
                while (it.MoveNext())
                {
                    MessageRecord.CField f = it.Current as MessageRecord.CField;
                    LogHelper.Debug(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "rec:[" + f.nPosition + "]" + f.oField);
                }
            }
            catch (Exception e)
            {
                LogHelper.Error(e);
            }
        }
        #region 请求响应
        protected AtsMsg<T> RequestMessage<T>(string svrId, string topic, string paras, string parasType)
        {
            return RequestMessage<T>(svrId, topic, paras, parasType, "");
        }

        protected AtsMsg<T> RequestMessage<T>(string svrId, string topic, string paras, string parasType, string fixType)
        {
            LogHelper.Info(string.Format("Request:{0},{1},{2}", svrId, topic, paras));
            AtsMsg<T> atsMsg = AtsMsgHelper.CreateAtsMessage<T>();
            atsMsg.Header.Code = "0000";
            atsMsg.Header.Source = svrId;
            string body = "";
            try
            {
                if (MQIIIManager.Instance.RequestEnabled(svrId))
                {
                    com.adaptiveMQ2.message.Message oMsgRequest = new com.adaptiveMQ2.message.Message();
                    oMsgRequest.SignalID = LocalEnv.UserInfo.SessionID;
                    oMsgRequest.Destination = topic;
                    MessageBody record = oMsgRequest.MessageBody;
                    record.addString(4, parasType);

                    if (paras.Length >= com.adaptiveMQ2.message.Message.MAX_GROUP_FIELD_LENGTH)
                    {
                        byte[] buf = com.adaptiveMQ2.utils.Utils.gzipStr(paras);
                        if (buf.Length >= com.adaptiveMQ2.message.Message.MAX_SEND_LENGTH)
                        {
                            record.addInt(5, 3);
                            record.setBlobField((short)1, buf);
                        }
                        else
                        {
                            record.addInt(5, 1);
                            record.addBytes((short)3, buf);
                        }
                    }
                    else
                    {
                        record.addInt(5, 0);
                        record.addString(3, paras);
                    }

                    record.addString(7, DateTime.Now.ToString("yyyyMMdd HH:mm:ss"));
                    if (!string.IsNullOrEmpty(fixType))
                    {
                        record.addString(12, fixType);
                    }
                    com.adaptiveMQ2.message.Message msg = MQIIIManager.Instance.RequestMessage(svrId, oMsgRequest, timeOut);
                    if (msg != null)
                    {
                        if (msg.MessageBody.haseField(9) != 0)
                        {
                            atsMsg.Header.Code = "0008";
                            atsMsg.Header.Source = svrId;

                            return atsMsg;
                        }
                        //doMsg(msg);
                        int isZip = 0;
                        string msgType = parasType;
                        if (msg.MessageBody.haseField(4) != 0)
                        {
                            msgType = msg.MessageBody.getString(4);
                        }
                        if (msg.MessageBody.haseField(5) != 0)
                        {
                            isZip = msg.MessageBody.getInt(5); //后台数据是否压缩
                        }
                        if (isZip == 1)
                        {
                            body = msg.MessageBody.getGZIPString(3); //压缩
                        }
                        else if (isZip == 3)
                        {
                            if (msg.MessageBody.haseField(1) != 0)
                                body = com.adaptiveMQ2.utils.Utils.ungzipByte(msg.MessageBody.getBlobField());
                        }
                        else
                        {
                            body = msg.MessageBody.getString(3); //不压缩
                        }

                        if (msg.MessageBody.haseField(11) != 0)
                        {
                            if (!msg.MessageBody.getInt(11).Equals(1)) //成功解析
                            {
                                if (msg.MessageBody.haseField(10) != 0)
                                {
                                    atsMsg.Header.Code = msg.MessageBody.getString(10);

                                }
                                atsMsg.Header.Exception = body;
                            }
                        }
                        if (!string.IsNullOrEmpty(body))
                        {
                            if (msgType == "JSON" || msgType == "JSONLIST")
                            {
                                atsMsg.Body = MsgHelper.Deserialize<T>(body, MsgSerializerType.Json);
                            }
                        }
                        if (msg.MessageBody.haseField(7) != 0)
                        {
                            atsMsg.Header.SendTime = msg.MessageBody.getString(7);
                        }
                    }
                    else
                    {
                        atsMsg.Header.Code = "0005";
                        atsMsg.Header.Source = svrId;
                    }
                }
                else
                {
                    atsMsg.Header.Code = "0007";
                    atsMsg.Header.Source = svrId;
                }
            }
            catch (Exception ex)
            {
                atsMsg.Header.Code = "0006";

                LogHelper.Error(ex);
            }
            LogHelper.Info(string.Format("Result:{0},{1},{2},{3}", atsMsg.Header.Code, svrId, topic, body));
            return atsMsg;
        }

        #endregion

    }
}
