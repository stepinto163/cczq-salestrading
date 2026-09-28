using com.gateway.connector;
using com.gateway.connector.proto;
using com.gateway.connector.tcp;
using com.gateway.connector.utils;
using com.gateway.log;
using DotNetty.Transport.Channels;
using dps.connector;
using dps.plugin;
using dps.tcp;
using Newtonsoft.Json;
using STC.Common.Api;
using STC.Common.Handler.PrePaidList;
using STC.Common.Handler.SystemParam;
using STC.Common.Model.Fix;
using STC.Common.Model.TBook;
using STC.dps.plugin.Common;
using STC.dps.plugin.Handler;
using STC.dps.plugin.Model.Fix.Message;
using STC.dps.plugin.PrePaidList;
using STC.dps.plugin.QM;
using STC.dps.plugin.Utils;
using StcLite.dps.plugin.QM;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using static dps.plugin.GWPlugin;

namespace STC.dps.plugin
{
    /// <summary>
    /// STC Lite 业务插件
    /// </summary>
    public class STCPlugin : IPlugin, SessionListener
    {
        #region 字段
        GWPlugin gwPlugin = null;
        

        #endregion

        #region 插件
        private int _tag = 3;
        public int Tag { get { return _tag; } set { _tag = value; } }


        public string PluginKey { get { return "StcLiteSvr"; } }

        public void BeforeTerminating()
        {
            LogHelper.Info("STCPlugin BeforeTerminating");
        }
        public void OnLoading()
        {
            PluginManager.addFilterInvoke(new FilterInvoke());
            SessionManager.addSessionListener(this);
            gwPlugin = PluginManager.getPlugin("*") as GWPlugin;
            List<string> lt = new List<string>();
            lt.Add("stc.md");
            lt.Add("stc.tbook.t");
            lt.Add("stc.tbook.systemparas");
            lt.Add("stc.ohlc");
            gwPlugin.cacheImageTopics.AddRange(lt);
            DpsLocalEnv.Load();
            LogHelper.Info("STCPlugin OnLoading");
        }
        //我的订单 //我的成交
        //public static List<string> ltTopics = new List<string> { "stc.tbook.s", "stc.ems.e" };

        public MessageWrapper invoke(IChannelHandlerContext ctx, SystemMessage sMsg, Proto msg)
        {
            string sessionId = msg.SessionId;
            GateWayApiWraper gww = gwPlugin.getGateWayApiWraper(sessionId);
            if (gww != null && !gww.IsFirst && !gww.GateWayApi.IsConnected)
            {
                ctx.CloseAsync();
                return null;
            }
            byte[] body = msg.Body;
            string content = body != null ? Encoding.UTF8.GetString(body) : "";
            if (msg.Cmd == Cmd.SEND && msg.Format == Format.REQUEST)
            {
                GateWayApi client = gww.GateWayApi;
                Dictionary<String, String> hm = JsonConvert.DeserializeObject<Dictionary<string, string>>(content);
                string serverName = hm["serverName"];
                string method = hm["method"];
                content = hm["content"];
                LogHelper.Info("STCPlugin:{0}:{1}:{2}", serverName, method, content);
                if ("subscribe".Equals(method))
                {// 订阅
                    Topic topic = new Topic();
                    topic.SessionId = msg.SessionId;
                    topic.TopicStr = content;
                    topic.IMessage = new Message(msg);
                    bool flag = TopicManager.add(msg.SessionId, topic);
                    return new MessageWrapper(MessageProtocol.REPLY, msg.SessionId, generateReplySuccess(msg.SessionId, msg.Cmd, msg.Seq));
                }
                else if ("unsubscribe".Equals(method))
                {

                    Topic topic = TopicManager.get(msg.SessionId, content);

                    if (topic != null)
                    {
                        TopicManager.remove(msg.SessionId, topic);
                    }

                    return new MessageWrapper(MessageProtocol.REPLY, msg.SessionId, generateReplySuccess(msg.SessionId, msg.Cmd, msg.Seq));
                }
                else
                {
                    LogHelper.Info("STCPlugin:{0}:{1}:{2}", serverName, method, content);
                    if (method.Equals(Constants.Topic_pre_activation))
                    {//预埋单激活/撤销
                        Event t = JsonConvert.DeserializeObject<Event>(content, new JsonSerializerSettings
                        {
                            TypeNameHandling = TypeNameHandling.All
                        });
                        EventManager.instance.msg = msg;
                        EventManager.instance.GateWayApi = gww.GateWayApi;
                        if (t != null && !string.IsNullOrWhiteSpace(t.name))
                        {
                            string eventname = EventUtils.getEventName(userName, t.name);
                            t.name = eventname;
                            EventUtils.onPublishOrder(t);
                        }
                        return new MessageWrapper(MessageProtocol.REPLY, msg.SessionId, generateReplySuccess(msg.SessionId, msg.Cmd, msg.Seq));
                    }

                    //模拟下单  
                    NewOrderSingle newOrder = JsonConvert.DeserializeObject<NewOrderSingle>(content);
                    string isSimulate = newOrder.Info5;
                    string terminal = newOrder.Terminal;
                    bool needSimulate = SystemParamHelper.isSimulate(terminal) || (!string.IsNullOrEmpty(isSimulate) && isSimulate.Equals("1"));
                    LogHelper.Info("needSimulate:" + needSimulate);
                    if (needSimulate)
                    {
                        LocalPlugin localPlugin = PluginManager.getPlugin("LocalSvr") as LocalPlugin;

                        localPlugin.notify(false, Constants.TOPIC_SIMULATE_ORDER, content);
                        return new MessageWrapper(MessageProtocol.REPLY, msg.SessionId, generateReplySuccess(msg.SessionId, msg.Cmd, msg.Seq));
                    }
                    //撤销订单
                    else if (method.Equals(Constants.Topic_stc_tbook_cancelOrder))
                    {
                        STC.dps.plugin.Model.Fix.Message.OrderCancelRequest cancelOrder = JsonConvert.DeserializeObject<STC.dps.plugin.Model.Fix.Message.OrderCancelRequest>(content);
                        if ("1".Equals(ConfInfo.IsOpenQM))//开启qm发消息
                        {
                            if (ProcessWatcher.IsExitProess() == null)//qm不在线
                            {
                                return new MessageWrapper(MessageProtocol.REPLY, msg.SessionId, QMRsp.generateReplyFailQM(msg.SessionId, msg.Cmd, msg.Seq));
                            }
                            STC.Common.Model.Fix.Message.ExecutionReport po = OrdersHandler.Instance.IsContainID(cancelOrder.OrderID);
                            if (po == null)
                            {
                                LogHelper.Warn(cancelOrder.OrderID + "is not exit!");
                            }
                            //if (BaseHandler.Instance.IsContainBroker(po.Venues, po.SecurityType) == null)
                            //{
                            //    return new MessageWrapper(MessageProtocol.REPLY, msg.SessionId, QMRsp.generateReplyFailBroker(msg.SessionId, msg.Cmd, msg.Seq));
                            //}
                        }
                        gww.GateWayApi.requestAsync(Constants.TBookSvr, Constants.Topic_stc_tbook_cancelOrder, content, Consts.WaitTime, new RspCallBack(msg));
                    }
                    else
                    {//TBook下单
                        if ("1".Equals(ConfInfo.IsOpenQM))//开启qm发消息
                        {
                            if (ProcessWatcher.IsExitProess() == null)//qm不在线
                            {
                                return new MessageWrapper(MessageProtocol.REPLY, msg.SessionId, QMRsp.generateReplyFailQM(msg.SessionId, msg.Cmd, msg.Seq));
                            }
                            //if (BaseHandler.Instance.IsContainBroker(newOrder.Venues, newOrder.SecurityType) == null)
                            //{
                            //    return new MessageWrapper(MessageProtocol.REPLY, msg.SessionId, QMRsp.generateReplyFailBroker(msg.SessionId, msg.Cmd, msg.Seq));
                            //}
                            newOrder.Info2 = OrdersHandler.Instance.getVersionByVenue(newOrder.SelfSelectVenues) + 1 + "";
                            content = JsonConvert.SerializeObject(newOrder);
                        }
                        gww.GateWayApi.requestAsync(Constants.TBookSvr, Constants.Topic_stc_tbook_placeOrder, content, Consts.WaitTime, new RspCallBack(msg));
                    }
                }

            }
            //业务处理

            return null;
        }
        private string userName = "";
        public void sessionCreated(Session session)
        {
            LogHelper.Info("session...."+ userName);
            if (string.IsNullOrEmpty(userName))
            {
                userName = session.UserName;
                ThreadPool.QueueUserWorkItem(o =>
                {
                    string sessionId = session.SessionId;
                    GateWayApiWraper gww = gwPlugin.getGateWayApiWraper(sessionId);                 

                    string emsTopic = string.Format("{0}{1}.**", Constants.Topic_stc_tbook_prefix, userName);
                    //gww.GateWayApi.subscribe(emsTopic, new EventMessage(generateNotify(sessionId, null)));
                    EventManager.instance.seesionid = sessionId;
                    gww.GateWayApi.subscribe("stc.base.clear", new BaseClearMessage(gww));
                    LogHelper.Debug("SystemParamHandler.Instance.Init start");
                    gww.GateWayApi.subscribe(ApiConsts.TBookSvr, ApiConsts.Topic_stc_tbook_system_param, new SystemParamMessage(gww));
                    LogHelper.Debug("SystemParamHandler.Instance.Init end");

                    if ("1".Equals(ConfInfo.IsOpenQM))//是否启用
                    {
                        string topic = string.Format("{0}*.{1}.**", Constants.Topic_stc_ems_prefix, userName);
                        gww.GateWayApi.subscribe(topic, new QMMessage(generateNotify(sessionId, null)));
                        OrdersHandler.Instance.Init(gww.GateWayApi, userName);
                        //BaseHandler.Instance.Init(gww);
                    }
                });
            }
        }

        public void sessionDestroyed(Session session)
        {
            cancelQuantOrder(session);
        }

        #endregion

        #region 方法 

        #region 量化订单撤销

        

        private void cancelQuantOrder(Session session)
        {
            string clientType = session.getAttribute("clientType")+"";
            string quantID = session.getAttribute("quantID") + "";

            LogHelper.Info(session.UserName+":"+ clientType);
            if (string.IsNullOrEmpty(clientType))
            {
                OrderMassCancelRequest omcr = new OrderMassCancelRequest();
                GateWayApiWraper gww = gwPlugin.getOrCreateGateWayWraper(session.UserName);
                string content;
                if (!string.IsNullOrEmpty(quantID))
                {          
                    omcr.MassCancelRequestType = MessageDictionary.FieldsEnum.MassCancelRequestType.CANCEL_STRATEGY_ORDERS;
                    omcr.CreateBy = gww.UserName;
                    omcr.QuantID = quantID;
                    omcr.Text = "dps cancel quant order";
                    LogHelper.Info("cancelQuantOrder:" + omcr.CreateBy+":"+quantID);
                    content = JsonConvert.SerializeObject(omcr);
                    gww.GateWayApi.requestAsync(Constants.TBookSvr, Constants.Topic_stc_tbook_massCancelOrder, content, Consts.WaitTime, null);
                }
                Dictionary<string, object> mmReq = new Dictionary<string, object>();
                mmReq.Add("Enable","0");
                mmReq.Add("CreateBy", omcr.CreateBy);
                LogHelper.Info("cancelMMOrder:" + omcr.CreateBy);
                content = JsonConvert.SerializeObject(mmReq);
                gww.GateWayApi.requestAsync(Constants.MMSvr, Constants.Topic_stc_mm_cancelQuant, content, Consts.WaitTime, null);

            }
        }
        #endregion

        #region 日末清零
        class BaseClearMessage : IMessage
        {
            GateWayApiWraper gww = null;
            public BaseClearMessage(GateWayApiWraper gww) {
                this.gww = gww;
            }
            public void onMessage(string topic, string t)
            {
                gww.GateWayApi.htCacheImagClear();
            }
        }
        #endregion


        #region 系统参数
        class SystemParamMessage : IMessage
        {
            GateWayApiWraper gww = null;
            public SystemParamMessage(GateWayApiWraper gww)
            {
                this.gww = gww;
            }
            public void onMessage(string topic, string t)
            {
                try
                {
                    LogHelper.Debug(string.Format("topic:{0}, conent:{1}", topic, t));
                    UserSetting userSetting = JsonConvert.DeserializeObject<UserSetting>(t);
                    SystemParamHandler.Instance.SetUserSetting(userSetting);
                }
                catch (Exception e)
                {
                    LogHelper.Error(e.Message);
                }
            }
        }
        #endregion

        #endregion

    }

    public class FilterInvoke : IFilterInvoke
    {
        public bool invoke(IChannelHandlerContext ctx, SystemMessage sMsg, Proto msg, string pluginKey, string method, string content)
        {
            bool flag = true;
            string sessionId = msg.SessionId;
            if (method.Equals(Constants.Topic_stc_tbook_placeOrder)) {
                Session session = SessionManager.getSession(sessionId);
                if (session != null) {
                   NewOrderSingle nos= JsonConvert.DeserializeObject<NewOrderSingle>(content);
                    string quauntID = nos.QuantID;
                    if (!string.IsNullOrEmpty(quauntID)) {
                        session.setAttribute("quantID", quauntID);
                    }
                }
            }
            return flag;
        }
    }
}
