using Ats.Foundation.Message;
using com.gateway.log;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using BDB.Price.Plugin.Handler;
using STC.Common.Model.Admin;
using STC.Common.Utils;
using STC.Common.Handler.Admin;
using STC.Common.Handler.Base;
using STC.Common.Model.Base;
using STC.Common.Model.Fix.Message;
using STC.Frame.CustomUI;

namespace BDB.Trade.Plugin.CustomUI
{
    /// <summary>
    /// MsgUI.xaml 的交互逻辑
    /// </summary>
    public partial class MsgUI : UserControl
    {
        private bool _isSub = false;
        private bool _isRun = false;
        private bool _isRunRefresh = true;
        private bool isInit = true;
        private AsyncThreadQueue<List<TTbookSystemMessage>> msgTask = null;
        private bool needRefresh = false;
        private bool isGridChange = false;
        private object lockObj = new object();

        public MsgUI()
        {
            InitializeComponent();

            btnClose_Click(null, null);


            msgUI.SetCurrentType("1");   //消息提醒
            dealMsgUI.SetCurrentType("2"); //待办事项

            btnMsg.AddHandler(Button.MouseMoveEvent, new MouseEventHandler(Btn_MouseMove), true);
            btnMsg.AddHandler(Button.MouseLeftButtonUpEvent, new MouseButtonEventHandler(Btn_MouseUp), true);

            //dealMsgUI.OnScrollChanged += DealMsgUI_OnScrollChanged;
            //msgUI.OnScrollChanged += DealMsgUI_OnScrollChanged;


            //Thread thread = new Thread(Simulate);
            //thread.IsBackground = true;
            //thread.Start();

            this.Unloaded += MsgUI_Unloaded;

        }

        private void MsgUI_Unloaded(object sender, RoutedEventArgs e)
        {
            if (dealMsgUI != null)
            {
                dealMsgUI.dispose();
            }

            if (msgUI != null)
            {
                msgUI.dispose();
            }

            if (msgTask != null)
            {
                msgTask.Clear();
                msgTask.Dispose();
            }

            _isRunRefresh = false;
        }

        private void Simulate()
        {
            while (true)
            {
                for (int i = 0; i < 100000; i++)
                {
                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        TTbookSystemMessage msg = new TTbookSystemMessage();
                        msg.closeby = "TBook";
                        msg.content = "{\"content\":\"220220.IB Ofr 7.0565 5000万 (NonFirm) 已匹配到报价（13笔）\",\"contentType\":\"PM\",\"detail\":\"{\\\"Prices\\\":[{\\\"Date\\\":\\\"20230515\\\",\\\"Id\\\":\\\"s1107713967549054977\\\",\\\"InstitutionID\\\":\\\"\\\",\\\"InstitutionName\\\":\\\"\\\",\\\"InternalFlag\\\":\\\"1\\\",\\\"IsFirmFlag\\\":false,\\\"MDReqId\\\":\\\"\\\",\\\"NetPrice\\\":120.0004,\\\"PartySubID\\\":\\\"1087706657527431168\\\",\\\"Qty\\\":5000,\\\"SN\\\":1,\\\"SettlType\\\":\\\"2\\\",\\\"SettlTypeLt\\\":[\\\"2\\\"],\\\"Time\\\":\\\"16:59:50.000\\\",\\\"TraderID\\\":\\\"\\\",\\\"TraderName\\\":\\\"\\\",\\\"ValidFlag\\\":true,\\\"Value\\\":0.586,\\\"Venue\\\":\\\"MANUAL\\\",\\\"VenueGW\\\":\\\"INNER\\\",\\\"WholeFlag\\\":\\\"0\\\",\\\"Yield\\\":0.586,\\\"isForbit\\\":false,\\\"leftQty\\\":5000,\\\"lockBeginTime\\\":0,\\\"lockQty\\\":0},{\\\"Date\\\":\\\"20230515\\\",\\\"Id\\\":\\\"s1107714083693527041\\\",\\\"InstitutionID\\\":\\\"\\\",\\\"InstitutionName\\\":\\\"\\\",\\\"InternalFlag\\\":\\\"1\\\",\\\"IsFirmFlag\\\":false,\\\"MDReqId\\\":\\\"\\\",\\\"NetPrice\\\":120.0004,\\\"PartySubID\\\":\\\"1087706657527431168\\\",\\\"Qty\\\":5000,\\\"SN\\\":1,\\\"SettlType\\\":\\\"2\\\",\\\"SettlTypeLt\\\":[\\\"2\\\"],\\\"Time\\\":\\\"17:00:17.000\\\",\\\"TraderID\\\":\\\"\\\",\\\"TraderName\\\":\\\"\\\",\\\"ValidFlag\\\":true,\\\"Value\\\":0.586,\\\"Venue\\\":\\\"MANUAL\\\",\\\"VenueGW\\\":\\\"INNER\\\",\\\"WholeFlag\\\":\\\"0\\\",\\\"Yield\\\":0.586,\\\"isForbit\\\":false,\\\"leftQty\\\":5000,\\\"lockBeginTime\\\":0,\\\"lockQty\\\":0},{\\\"Date\\\":\\\"20230515\\\",\\\"Id\\\":\\\"s1107714238257823745\\\",\\\"InstitutionID\\\":\\\"\\\",\\\"InstitutionName\\\":\\\"\\\",\\\"InternalFlag\\\":\\\"1\\\",\\\"IsFirmFlag\\\":false,\\\"MDReqId\\\":\\\"\\\",\\\"NetPrice\\\":120.0004,\\\"PartySubID\\\":\\\"1087706657527431168\\\",\\\"Qty\\\":5000,\\\"SN\\\":1,\\\"SettlType\\\":\\\"2\\\",\\\"SettlTypeLt\\\":[\\\"2\\\"],\\\"Time\\\":\\\"17:00:54.000\\\",\\\"TraderID\\\":\\\"\\\",\\\"TraderName\\\":\\\"\\\",\\\"ValidFlag\\\":true,\\\"Value\\\":0.586,\\\"Venue\\\":\\\"MANUAL\\\",\\\"VenueGW\\\":\\\"INNER\\\",\\\"WholeFlag\\\":\\\"0\\\",\\\"Yield\\\":0.586,\\\"isForbit\\\":false,\\\"leftQty\\\":5000,\\\"lockBeginTime\\\":0,\\\"lockQty\\\":0},{\\\"Date\\\":\\\"20230515\\\",\\\"Id\\\":\\\"s1107714872323342337\\\",\\\"InstitutionID\\\":\\\"\\\",\\\"InstitutionName\\\":\\\"\\\",\\\"InternalFlag\\\":\\\"1\\\",\\\"IsFirmFlag\\\":true,\\\"MDReqId\\\":\\\"\\\",\\\"NetPrice\\\":120.0004,\\\"PartySubID\\\":\\\"1087706657527431168\\\",\\\"Qty\\\":5000,\\\"SN\\\":1,\\\"SettlType\\\":\\\"2\\\",\\\"SettlTypeLt\\\":[\\\"2\\\"],\\\"Time\\\":\\\"17:03:25.000\\\",\\\"ValidFlag\\\":true,\\\"Value\\\":0.586,\\\"Venue\\\":\\\"MANUAL\\\",\\\"VenueGW\\\":\\\"INNER\\\",\\\"WholeFlag\\\":\\\"0\\\",\\\"Yield\\\":0.586,\\\"isForbit\\\":false,\\\"leftQty\\\":5000,\\\"lockBeginTime\\\":0,\\\"lockQty\\\":0},{\\\"Date\\\":\\\"20230515\\\",\\\"Id\\\":\\\"s1107714906070712321\\\",\\\"InstitutionID\\\":\\\"\\\",\\\"InstitutionName\\\":\\\"\\\",\\\"InternalFlag\\\":\\\"1\\\",\\\"IsFirmFlag\\\":false,\\\"MDReqId\\\":\\\"\\\",\\\"NetPrice\\\":120.0004,\\\"PartySubID\\\":\\\"1087706657527431168\\\",\\\"Qty\\\":5000,\\\"SN\\\":1,\\\"SettlType\\\":\\\"2\\\",\\\"SettlTypeLt\\\":[\\\"2\\\"],\\\"Time\\\":\\\"17:03:33.000\\\",\\\"ValidFlag\\\":true,\\\"Value\\\":0.586,\\\"Venue\\\":\\\"MANUAL\\\",\\\"VenueGW\\\":\\\"INNER\\\",\\\"WholeFlag\\\":\\\"0\\\",\\\"Yield\\\":0.586,\\\"isForbit\\\":false,\\\"leftQty\\\":5000,\\\"lockBeginTime\\\":0,\\\"lockQty\\\":0},{\\\"Date\\\":\\\"20230515\\\",\\\"Id\\\":\\\"1107715194995343365\\\",\\\"InternalFlag\\\":\\\"2\\\",\\\"IsFirmFlag\\\":true,\\\"MDReqId\\\":\\\"\\\",\\\"NetPrice\\\":120.0000,\\\"Qty\\\":150000,\\\"SN\\\":2,\\\"SettlType\\\":\\\"2\\\",\\\"SettlTypeLt\\\":[\\\"2\\\"],\\\"Time\\\":\\\"17:04:41.817\\\",\\\"ValidFlag\\\":true,\\\"Value\\\":0.5860,\\\"Venue\\\":\\\"ODM\\\",\\\"VenueGW\\\":\\\"ODM\\\",\\\"Yield\\\":0.5860,\\\"isForbit\\\":false,\\\"leftQty\\\":150000,\\\"lockBeginTime\\\":0,\\\"lockQty\\\":0},{\\\"Date\\\":\\\"20230515\\\",\\\"Id\\\":\\\"1107715194995343367\\\",\\\"InternalFlag\\\":\\\"2\\\",\\\"IsFirmFlag\\\":true,\\\"MDReqId\\\":\\\"\\\",\\\"NetPrice\\\":100.0000,\\\"Qty\\\":102000,\\\"SN\\\":2,\\\"SettlType\\\":\\\"2\\\",\\\"SettlTypeLt\\\":[\\\"2\\\"],\\\"Time\\\":\\\"17:04:41.817\\\",\\\"ValidFlag\\\":true,\\\"Value\\\":2.7689,\\\"Venue\\\":\\\"ODM\\\",\\\"VenueGW\\\":\\\"ODM\\\",\\\"Yield\\\":2.7689,\\\"isForbit\\\":false,\\\"leftQty\\\":102000,\\\"lockBeginTime\\\":0,\\\"lockQty\\\":0},{\\\"ClearingMethod\\\":\\\"13\\\",\\\"Date\\\":\\\"20230515\\\",\\\"DeliveryType\\\":\\\"0\\\",\\\"Id\\\":\\\"230515020410005266\\\",\\\"InternalFlag\\\":\\\"2\\\",\\\"IsFirmFlag\\\":false,\\\"MDReqId\\\":\\\"\\\",\\\"NetPrice\\\":100.0000,\\\"PartySubID\\\":\\\"bdyhdealerfzzq\\\",\\\"Qty\\\":50000,\\\"SN\\\":9,\\\"SettlType\\\":\\\"2\\\",\\\"SettlTypeLt\\\":[\\\"2\\\"],\\\"Time\\\":\\\"14:14:53.081\\\",\\\"ValidFlag\\\":true,\\\"Value\\\":2.7689,\\\"Venue\\\":\\\"QDMESP\\\",\\\"VenueGW\\\":\\\"QDMESP\\\",\\\"Yield\\\":2.7689,\\\"isForbit\\\":false,\\\"leftQty\\\":50000,\\\"lockBeginTime\\\":0,\\\"lockQty\\\":0},{\\\"Date\\\":\\\"20230515\\\",\\\"Id\\\":\\\"1107715194995343368\\\",\\\"InternalFlag\\\":\\\"2\\\",\\\"IsFirmFlag\\\":true,\\\"MDReqId\\\":\\\"\\\",\\\"NetPrice\\\":99.7000,\\\"Qty\\\":2000,\\\"SN\\\":2,\\\"SettlType\\\":\\\"2\\\",\\\"SettlTypeLt\\\":[\\\"2\\\"],\\\"Time\\\":\\\"17:04:41.817\\\",\\\"ValidFlag\\\":true,\\\"Value\\\":2.8055,\\\"Venue\\\":\\\"ODM\\\",\\\"VenueGW\\\":\\\"ODM\\\",\\\"Yield\\\":2.8055,\\\"isForbit\\\":false,\\\"leftQty\\\":2000,\\\"lockBeginTime\\\":0,\\\"lockQty\\\":0},{\\\"Date\\\":\\\"20230515\\\",\\\"Id\\\":\\\"1107715194995343369\\\",\\\"InternalFlag\\\":\\\"2\\\",\\\"IsFirmFlag\\\":true,\\\"MDReqId\\\":\\\"\\\",\\\"NetPrice\\\":98.1230,\\\"Qty\\\":20000,\\\"SN\\\":2,\\\"SettlType\\\":\\\"2\\\",\\\"SettlTypeLt\\\":[\\\"2\\\"],\\\"Time\\\":\\\"17:04:41.817\\\",\\\"ValidFlag\\\":true,\\\"Value\\\":2.9997,\\\"Venue\\\":\\\"ODM\\\",\\\"VenueGW\\\":\\\"ODM\\\",\\\"Yield\\\":2.9997,\\\"isForbit\\\":false,\\\"leftQty\\\":20000,\\\"lockBeginTime\\\":0,\\\"lockQty\\\":0},{\\\"Date\\\":\\\"20230515\\\",\\\"Id\\\":\\\"1107715194995343370\\\",\\\"InternalFlag\\\":\\\"2\\\",\\\"IsFirmFlag\\\":true,\\\"MDReqId\\\":\\\"\\\",\\\"NetPrice\\\":97.7225,\\\"Qty\\\":1000,\\\"SN\\\":2,\\\"SettlType\\\":\\\"2\\\",\\\"SettlTypeLt\\\":[\\\"2\\\"],\\\"Time\\\":\\\"17:04:41.817\\\",\\\"ValidFlag\\\":true,\\\"Value\\\":3.0500,\\\"Venue\\\":\\\"ODM\\\",\\\"VenueGW\\\":\\\"ODM\\\",\\\"Yield\\\":3.0500,\\\"isForbit\\\":false,\\\"leftQty\\\":1000,\\\"lockBeginTime\\\":0,\\\"lockQty\\\":0},{\\\"BargainFlag\\\":\\\"0\\\",\\\"Date\\\":\\\"20230515\\\",\\\"Id\\\":\\\"32202201\\\",\\\"InternalFlag\\\":\\\"2\\\",\\\"IsFirmFlag\\\":false,\\\"MDReqId\\\":\\\"\\\",\\\"NetPrice\\\":97.6268,\\\"Qty\\\":4000,\\\"SN\\\":4,\\\"SettlType\\\":\\\"2\\\",\\\"SettlTypeLt\\\":[\\\"2\\\"],\\\"Time\\\":\\\"16:01:59.997\\\",\\\"ValidFlag\\\":true,\\\"Value\\\":3.0575,\\\"Venue\\\":\\\"CCTB\\\",\\\"VenueGW\\\":\\\"CDH\\\",\\\"Yield\\\":3.0575,\\\"isForbit\\\":true,\\\"leftQty\\\":4000,\\\"lockBeginTime\\\":0,\\\"lockQty\\\":0},{\\\"BargainFlag\\\":\\\"1\\\",\\\"Date\\\":\\\"20230515\\\",\\\"Id\\\":\\\"1107714524443574273\\\",\\\"InternalFlag\\\":\\\"2\\\",\\\"IsFirmFlag\\\":false,\\\"MDReqId\\\":\\\"\\\",\\\"NetPrice\\\":97.6268,\\\"Qty\\\":2000,\\\"SN\\\":5,\\\"SettlType\\\":\\\"3\\\",\\\"SettlTypeLt\\\":[\\\"3\\\"],\\\"Time\\\":\\\"17:02:02.968\\\",\\\"ValidFlag\\\":true,\\\"Value\\\":3.0575,\\\"Venue\\\":\\\"PTCN\\\",\\\"VenueGW\\\":\\\"CDH\\\",\\\"Yield\\\":3.0575,\\\"isForbit\\\":false,\\\"leftQty\\\":2000,\\\"lockBeginTime\\\":0,\\\"lockQty\\\":0}],\\\"contentType\\\":\\\"A\\\",\\\"order\\\":{\\\"algoName\\\":\\\"CROSS\\\",\\\"avgPx2\\\":0,\\\"bBPx\\\":0.5860,\\\"bBTime\\\":\\\"2023-05-15 17:04:41.817\\\",\\\"bBVol\\\":150000,\\\"bOPx\\\":3.0565,\\\"bOTime\\\":\\\"2023-05-15 17:02:02.968\\\",\\\"bOVol\\\":5000,\\\"bizType\\\":\\\"9\\\",\\\"bizTypeId\\\":\\\"\\\",\\\"cancelQty2\\\":0,\\\"clOrderId\\\":\\\"202305151705066112\\\",\\\"cross\\\":true,\\\"crossVenues\\\":\\\"1\\\",\\\"cumQty\\\":0,\\\"cumQty2\\\":0,\\\"currency\\\":\\\"\\\",\\\"delflag\\\":\\\"0\\\",\\\"dqReject\\\":false,\\\"dqTimeOut\\\":false,\\\"emptyPrice\\\":false,\\\"emptyQty\\\":false,\\\"execType2\\\":\\\"null\\\",\\\"execType4\\\":\\\"null\\\",\\\"firmFlag\\\":\\\"0\\\",\\\"handlinst\\\":\\\"1\\\",\\\"harddiskid\\\":\\\"kimtigo SSD 512GB\\\",\\\"info1\\\":\\\"{\\\\\\\"ValidTimeType\\\\\\\":\\\\\\\"0|\\\\\\\",\\\\\\\"ValidUntilTime\\\\\\\":\\\\\\\"2023-05-15 16:30:00\\\\\\\"}\\\",\\\"info2\\\":\\\"{\\\\\\\"IF_SUB_BOND\\\\\\\":\\\\\\\"BID\\\\\\\"}\\\",\\\"insideip\\\":\\\"192.168.129.12\\\",\\\"iocTimeOutLockFlag\\\":false,\\\"lastQty2\\\":0,\\\"leftQty2\\\":0,\\\"lockQty\\\":false,\\\"macid\\\":\\\"00-BE-43-B8-4B-A7\\\",\\\"marketIndicator\\\":\\\"4\\\",\\\"name\\\":\\\"yxl1\\\",\\\"netPrice\\\":71.1418,\\\"openQty2\\\":0,\\\"orderId\\\":\\\"s1107715294870110209\\\",\\\"orderStatus\\\":\\\"Newing\\\",\\\"orderType\\\":\\\"Limit\\\",\\\"partysubname\\\":\\\"yxl1\\\",\\\"price\\\":7.0565,\\\"price2\\\":0,\\\"priceAndOffSet\\\":7.0565,\\\"priceCondition\\\":{\\\"PriceInversion\\\":\\\"0\\\"},\\\"qdOrder\\\":false,\\\"qty\\\":5000,\\\"quantID\\\":\\\"\\\",\\\"riskManualCk\\\":\\\"\\\",\\\"roleId\\\":\\\"864870962095534080\\\",\\\"roleName\\\":\\\"交易员\\\",\\\"sN\\\":2,\\\"salesAgentflag\\\":\\\"\\\",\\\"securityid\\\":\\\"22国开20\\\",\\\"selfSelectVenues\\\":\\\"\\\",\\\"settlType\\\":\\\"2,3\\\",\\\"side\\\":\\\"Sell\\\",\\\"symbol\\\":\\\"220220.IB\\\",\\\"symbolType\\\":\\\"BNC\\\",\\\"terminal\\\":\\\"STClient_V1.7.0\\\",\\\"text\\\":\\\"\\\",\\\"time\\\":1684141506654,\\\"timeInForce\\\":\\\"DAY\\\",\\\"transactTime\\\":\\\"2023-05-15 17:05:06.654\\\",\\\"unOpenQty\\\":5000,\\\"updateTime\\\":\\\"2023-05-15 17:05:06.654\\\",\\\"userID\\\":\\\"907635487131398144\\\",\\\"userName\\\":\\\"yxl1\\\",\\\"validuntitime\\\":\\\"2023-05-15 22:30:00\\\",\\\"valueDate\\\":\\\"\\\",\\\"valueDate2\\\":\\\"\\\",\\\"venue\\\":\\\"MANUAL\\\",\\\"venueGW\\\":\\\"INNER\\\",\\\"venues\\\":[\\\"MANUAL\\\"],\\\"venuesSelectF\\\":\\\"1\\\",\\\"venueusersid\\\":\\\"\\\",\\\"wholeFlag\\\":\\\"0\\\"}}\",\"id\":\"s1107715294870110209\",\"securityid\":\"220220.IB\",\"show\":true,\"strategyOrderId\":\"s1107715294870110209\",\"title\":\"价格匹配\",\"type\":\"A\"}";
                        msg.createtime = "2023-05-15 17:05:06";
                        msg.delflag = "0";
                        msg.id = i.ToString();
                        msg.readflag = "";
                        msg.type = "2";
                        msg.updatetime = "2023-05-15 17:05:06";
                        msg.username = "yxl";
                        OnAdminMsg(new List<TTbookSystemMessage> { msg });

                    }));

                    Thread.Sleep(2000);
                }
                Thread.Sleep(60000 * 10);
            }
        }



        private void DealMsgUI_OnScrollChanged(string msgType, string startTime, int pageNum)
        {
            ThreadPool.QueueUserWorkItem(o =>
            {
                try
                {
                    SystemMessageReq req = SetSystemMsgReq(msgType, string.Empty, startTime, 0);

                    TQueryMessage systemMessage = AdminHandler.Instance.SearchSysMsg(req);


                    if (systemMessage != null && systemMessage.Datas != null && systemMessage.Datas.Count > 0)
                    {
                        //去除临界值
                        if (systemMessage.Datas.Count == 1 && !string.IsNullOrEmpty(systemMessage.Datas[0].createtime)
                            && systemMessage.Datas[0].createtime.Contains(startTime))
                            return;

                        this.Dispatcher.Invoke(() =>
                        {
                            if (msgType.Equals("1"))//消息提醒
                                msgUI.OnMsg(systemMessage.Datas);
                            else if (msgType.Equals("2"))//待办事项
                                dealMsgUI.OnMsg(systemMessage.Datas);

                            SetShowLevel();
                        });
                    }

                }
                catch (Exception e)
                {
                    LogHelper.Error(e);
                }
                finally
                {
                    if (msgType.Equals("1"))//消息提醒
                        msgUI.IsLoad = false;
                    else if (msgType.Equals("2"))//待办事项
                        dealMsgUI.IsLoad = false;
                }

            });
        }

        public event Action<object> OnDragMove;
        private void Btn_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                e.Handled = false;

                OnDragMove?.Invoke(sender);
            }
        }

        private void TextBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            OnDragMove?.Invoke(sender);
        }

        public void DealDoubleClick()
        {
            btnMsg_MouseDoubleClick(null, null);
        }

        private void btnMsg_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            btnMsg_MouseUp(sender, null);

            msgFloatView.ClearMsg();
            SetPopShow(false, null);

            SetTabPageIndex();
        }

        public void OpenWait()
        {
            this.Dispatcher.Invoke(() =>
            {
                btnMsg_MouseDoubleClick(null, null);

                tcPlugin.TabIndex = 1;
            });
        }

        private void Btn_MouseUp(object sender, MouseButtonEventArgs e)
        {
        }

        public void Clear()
        {
            BaseHandler.Instance.UnSubscribe(Instance_OnClear);

            AdminHandler.Instance.UnSubscribeOrder(OnAdminMsg);

            msgUI.Clear();
            dealMsgUI.Clear();
            msgFloatView.ClearMsg();
            if (msgTask != null)
            {
                msgTask.Clear();
                msgTask = null;
            }
        }

        private void Instance_OnClear(ClearPo clearPo)
        {
            LogHandler.Info("MsgUI Clear");
            if (clearPo != null && "Y".Equals(clearPo.IsClear))
            {
                this.Dispatcher.BeginInvoke((Action)(() =>
                {
                    msgUI.Clear();
                    dealMsgUI.Clear();
                    msgFloatView.ClearMsg();

                    SetShowLevel();

                    isInit = true;
                    if (msgTask != null)
                        msgTask.Clear();

                    Init(false);
                }));
            }
        }

        private SystemMessageReq SetSystemMsgReq(string msgType, string startTime, string endTime, int pageNum)
        {
            SystemMessageReq req = new SystemMessageReq();
            req.type = msgType;
            //req.closeby = "admin";
            req.username = LocalEnv.UserInfo.UserName;
            req.begintime = DateTime.Now.ToString(AdminConsts.YYYYMMdd) + " " + (string.IsNullOrEmpty(startTime) ? "00:00:00" : startTime);
            req.endtime = DateTime.Now.ToString(AdminConsts.YYYYMMdd) + " " + (string.IsNullOrEmpty(endTime) ? "23:59:59" : endTime);
            req.pageSize = 1000;
            req.pageNo = pageNum;
            req.sort = "1";
            req.displayAll = "0";

            return req;
        }



        public void Init(bool isSubscribe)
        {
            if (msgTask == null)
                msgTask = new AsyncThreadQueue<List<TTbookSystemMessage>>(Process, 100);

            ThreadPool.QueueUserWorkItem(o =>
            {
                try
                {
                    if (!_isSub)
                    {
                        BaseHandler.Instance.Init(Instance_OnClear);

                        _isSub = true;
                    }
                    //SystemMessageReq req = new SystemMessageReq();
                    ////req.type = "1";
                    ////req.closeby = "admin";
                    //req.username = LocalEnv.UserInfo.UserName;
                    //req.begintime = DateTime.Now.ToString(AdminConsts.YYYYMMdd) + " 00:00:00";
                    //req.endtime = DateTime.Now.ToString(AdminConsts.YYYYMMdd) + " 23:59:59";
                    //req.pageSize = 1000;
                    //req.pageNo = 0;
                    SystemMessageReq req = SetSystemMsgReq("1", "00:00:00", string.Empty, 0);
                    SystemMessageReq dealReq = SetSystemMsgReq("2", "00:00:00", string.Empty, 0);

                    if (isSubscribe)
                    {
                        AdminHandler.Instance.Init(OnAdminMsg);
                        //订阅App消息
                        BaseHandler.Instance.SubscribeAppMsg(OnAppMsg);
                    }

                    TQueryMessage systemMessage = AdminHandler.Instance.SearchSysMsg(req);
                    TQueryMessage dealSystemMessage = AdminHandler.Instance.SearchSysMsg(dealReq);

                    this.Dispatcher.Invoke(() =>
                    {
                        if (systemMessage != null && systemMessage.Datas != null)
                        {
                            msgUI.OnMsg(systemMessage.Datas);
                            dealMsgUI.OnMsg(systemMessage.Datas);
                        }
                        if (dealSystemMessage != null && dealSystemMessage.Datas != null)
                        {
                            dealMsgUI.OnMsg(dealSystemMessage.Datas);
                        }

                        SetAllReaded();

                        SetShowLevel();

                    });

                    RunRefresh();
                    isInit = false;

                }
                catch (Exception e)
                {
                    LogHelper.Error(e);
                }

            });
        }


        private void RunRefresh()
        {
            if (!_isRun)
            {
                _isRun = true;
            }
            else
                return;

            ThreadPool.QueueUserWorkItem(o =>
            {
                try
                {
                    while (_isRunRefresh)
                    {
                        Thread.Sleep(1000);

                        if (needRefresh || isGridChange)
                        {
                            this.Dispatcher.Invoke(() =>
                            {
                                lock (lockObj)
                                {
                                    SetShowLevel();

                                    isGridChange = false;
                                    needRefresh = false;
                                }
                            });
                        }

                        bool isOk = msgFloatView.GetStatus();
                        //this.Dispatcher.Invoke(() =>
                        //{
                        //    popMsg.IsOpen = false;
                        //});
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.Error(ex);
                }

            });
        }

        private void Process(List<TTbookSystemMessage> sysMsg)
        {
            if (sysMsg != null && sysMsg.Count > 0)
            {
                this.Dispatcher.Invoke(() =>
                {
                    lock (lockObj)
                    {
                        needRefresh = true;

                        if (sysMsg != null && sysMsg.Count > 0)
                        {
                            TTbookSystemMessage msg = sysMsg[0];
                            if (msg.content != null && msg.type.Equals("2") && tcPlugin.SelectedIndex == 1)
                                tcPlugin.SelectedIndex = 0;
                        }

                        //sysMsg[0].content = "{\"content\":\"是否恢复上个工作日订单 3 条\",\"contentType\":\"TableSvr_ORDER_UNREF\",\"needSend\":false,\"remark\":\"{\\\"type\\\":\\\"RATE\\\",\\\"orderTotal\\\":\\\"3\\\"}\",\"show\":true,\"title\":\"利率债UNREF提醒\",\"type\":\"N\"}";
                        msgUI.OnMsg(sysMsg); //消息提醒
                        dealMsgUI.OnMsg(sysMsg); //待办事项
                        
                        lock (sysMsg)
                        {
                            List<TTbookSystemMessage> IsPopShowMsg = new List<TTbookSystemMessage>();
                            List<TTbookSystemMessage> IsPreRemoveMsg = new List<TTbookSystemMessage>();
                            foreach (TTbookSystemMessage tsm in sysMsg)
                            {
                                // 删除逻辑
                                string isdel = tsm.delflag ?? "0";
                                if (isdel == "1")
                                {
                                    IsPreRemoveMsg.Add(tsm);
                                    continue;
                                }

                                if (string.IsNullOrEmpty(tsm.content))
                                {
                                    IsPopShowMsg.Add(tsm);
                                    continue;
                                }
                                if (!JsonSplit.IsJson(tsm.content))
                                {
                                    IsPopShowMsg.Add(tsm);
                                    continue;
                                }

                                MsgPo msgPo = ObjectConvert.SystemMsgToShowMsg(tsm, this);

                                if (msgPo != null && !(!string.IsNullOrEmpty(msgPo.Content) && (msgPo.Content.Contains("CDH") || msgPo.Content.Contains("SDN"))))//过滤CDH、SDN消息
                                {
                                    if (msgPo.IsShow ?? false)
                                    {
                                        if (msgPo.ContentType != null)
                                        {
                                            switch (msgPo.ContentType.ToUpper())
                                            {
                                                case AdminConsts.Admin_IT:
                                                    //过滤 Nonfirm点价待确认
                                                    break;
                                                default:

                                                    IsPopShowMsg.Add(tsm);
                                                    break;
                                            }
                                        }
                                    }
                                }
                            }

                            if (IsPopShowMsg.Count > 0)
                            {
                                SetPopShow(true, IsPopShowMsg); //弹出
                            }

                            // 删除窗口
                            if (IsPreRemoveMsg.Count > 0)
                            {
                                foreach (TTbookSystemMessage tsm in IsPreRemoveMsg)
                                {
                                    MsgPo msgPo = ObjectConvert.SystemMsgToShowMsg(tsm, this);
                                    msgFloatView.OnMsg(msgPo);
                                }
                            }
                        }
                    }
                });
            }
        }

        private void SetPopShow(bool status, List<TTbookSystemMessage> sysMsg)
        {
            this.Dispatcher.Invoke(() =>
            {
                bool isShow = status && btnMsg.Visibility == Visibility.Visible;// && (Application.Current.MainWindow.WindowState == WindowState.Normal || Application.Current.MainWindow.WindowState == WindowState.Maximized);
                if (isShow)
                {
                    bool ifLeft = SystemParameters.MenuDropAlignment;
                    if (!ifLeft)
                    {
                        popMsg.HorizontalOffset = (popMsg.Width - btnMsg.Width) * (-1);
                    }

                    popMsg.Visibility = Visibility.Visible;
                    popMsg.IsOpen = isShow;
                    popMsg.StaysOpen = true;

                    ProcessPop(sysMsg);

                    if (!Application.Current.MainWindow.IsActive)
                    {
                        Window window = CommonUtils.GetParentObject<Window>(this, "winMsg");
                        if (status && (window != null && !Application.Current.MainWindow.IsActive && !window.IsActive))// && !Application.Current.MainWindow.IsActive)
                            WindowIconHelper.Instance.Start();
                    }
                }
                else
                {
                    popMsg.StaysOpen = false;
                    popMsg.IsOpen = false;
                    popMsg.Visibility = Visibility.Collapsed;
                    //firtMsg.FloatAction(false);
                    //secondMsg.FloatAction(false);
                    //threeMsg.FloatAction(false);

                    Window window = CommonUtils.GetParentObject<Window>(this, "winMsg");
                    if (status && (window != null && !Application.Current.MainWindow.IsActive && !window.IsActive))// && !Application.Current.MainWindow.IsActive)
                        WindowIconHelper.Instance.Start();
                }

                if (btnMsg.Visibility == Visibility.Visible)
                    borderMain.Visibility = Visibility.Collapsed;


            });
        }

        private void SetTabPageIndex()
        {
            string s = tabMsgNotice.Header.ToString();
            if (s.Contains("["))
            {
                string rec = s.Split('[')[1].TrimEnd(']');
                if (!string.IsNullOrEmpty(rec))
                {
                    if (rec == btnMsg.Content.ToString())
                    {
                        this.Dispatcher.Invoke(() =>
                        {
                            tcPlugin.SelectedIndex = 1;

                            tabMsgNotice.IsSelected = true;
                        });
                    }
                }
            }
        }


        private void ProcessPop(List<TTbookSystemMessage> sysMsg)
        {
            if (sysMsg == null || sysMsg.Count <= 0)
                return;

            List<MsgPo> temDatas = new List<MsgPo>();
            foreach (TTbookSystemMessage msg in sysMsg)
            {
                MsgPo msgPo = ObjectConvert.SystemMsgToShowMsg(msg, this);
                if (msgPo != null)
                {
                    temDatas.Add(msgPo);
                }
            }

            int index = 0;
            double popHeigh = 0;
            foreach (MsgPo msgPo in temDatas)
            {
                //if (index > 2)
                //    break;

                msgPo.IsPopShow = true;
                MsgPo copyMsgPo = ObjectConvert.CopyMsgPo(msgPo);
                ObjectConvert.ChangeStyle(copyMsgPo, this);

                msgFloatView.OnMsg(copyMsgPo);
                //if (index == 0)
                //{
                //    firtMsg.Visibility = Visibility.Visible;
                //    firtMsg.Init(copyMsgPo);

                //    popHeigh += firtMsg.Height;
                //}
                //else if (index == 1)
                //{
                //    secondMsg.Visibility = Visibility.Visible;
                //    secondMsg.Init(copyMsgPo);

                //    popHeigh += secondMsg.Height;
                //}
                //else if (index == 2)
                //{
                //    threeMsg.Visibility = Visibility.Visible;
                //    threeMsg.Init(copyMsgPo);

                //    popHeigh += threeMsg.Height;
                //}


                index++;
            }
            //popMsg.Height = popHeigh + index * 2;
        }


        public void OnAdminMsg(List<TTbookSystemMessage> sysMsg)
        {
            msgTask.Enqueue(sysMsg);
            //Process(sysMsg);
        }



        private void SetShowLevel()
        {
            this.Dispatcher.Invoke(() =>
            {
                Style style = (Style)FindResource("NoticeNormalButton");

                int msgShowCount = msgUI.GetShowMsgCount(true, string.Empty);
                int dealMsgShowCount = dealMsgUI.GetShowMsgCount(true, string.Empty);
                int total = msgShowCount + dealMsgShowCount;

                if (total > 99)
                    btnMsg.Content = "99+";
                else
                {
                    if (total > 0)
                        btnMsg.Content = total;
                    else
                        btnMsg.Content = string.Empty;
                }

                if (msgShowCount > 0)
                    tabMsgNotice.Header = string.Format("消息提醒[{0}]", msgShowCount > 99 ? "99+" : msgShowCount.ToString());
                else
                    tabMsgNotice.Header = "消息提醒";

                if (dealMsgShowCount > 0)
                    tabDo.Header = string.Format("待办事项[{0}]", dealMsgShowCount > 99 ? "99+" : dealMsgShowCount.ToString());
                else
                    tabDo.Header = "待办事项";

                if (msgShowCount == 0 && dealMsgShowCount == 0)
                    btnMsg.Style = style;
                else
                {
                    //重要消息
                    int warnmsgShowCount = msgUI.GetShowMsgCount(true, "2") + msgUI.GetShowMsgCount(true, "1");
                    //重要待办
                    int warndealMsgShowCount = dealMsgUI.GetShowMsgCount(true, "2");

                    if (warnmsgShowCount == 0 && warndealMsgShowCount == 0)
                    {
                        btnMsg.Style = (Style)FindResource("NoticeWarntButton");  //绿色
                    }
                    else
                    {
                        btnMsg.Style = (Style)FindResource("NoticeUrgebtButton"); //红色
                    }
                }

            });
        }

        private void btnMsg_MouseUp(object sender, MouseButtonEventArgs e)
        {

            isGridChange = true;
            btnMsg.Visibility = Visibility.Collapsed;
            this.Width = 360;
            this.Height = 478;

            gMsg.Visibility = Visibility.Visible;
            borderMain.Visibility = Visibility.Visible;
            //SetPopShow(true);

            OnSiseChange?.Invoke(this.Width, this.Height);

            gMsg.Background = tcPlugin.Background = (SolidColorBrush)this.FindResource("ColorBrush.Gray18");

        }

        private void btnMsg_Click(object sender, RoutedEventArgs e)
        {
            isGridChange = true;
            btnMsg.Visibility = Visibility.Collapsed;
            this.Width = 360;
            this.Height = 478;

            gMsg.Visibility = Visibility.Visible;
            borderMain.Visibility = Visibility.Visible;

            //SetPopShow(true);

            OnSiseChange?.Invoke(this.Width, this.Height);
        }


        public void SetAlignment()
        {
            //获取系统是以Left-handed（true）还是Right-handed（false）
            var ifLeft = SystemParameters.MenuDropAlignment;

            if (!ifLeft)
            {
                // change to false
                var t = typeof(SystemParameters);
                var field = t.GetField("_menuDropAlignment", BindingFlags.NonPublic | BindingFlags.Static);
                field.SetValue(null, false);

                ifLeft = SystemParameters.MenuDropAlignment;
            }
        }

        public event Action<double, double> OnSiseChange;

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            isGridChange = false;
            btnMsg.Visibility = Visibility.Visible;
            this.Width = 65;
            this.Height = 65;

            borderMain.Visibility = gMsg.Visibility = Visibility.Collapsed;

            OnSiseChange?.Invoke(this.Width, this.Height);
        }

        private void UserControl_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
        }

        private void btnPopDeal_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnPopKnow_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Storyboard_Completed(object sender, EventArgs e)
        {
            this.SetPopShow(false, null);
        }

        private void OnStoryboardComplete(bool isOk)
        {
            if (isOk)
            {
                SetPopShow(false, null);
            }
        }

        private void popMsg_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //firtMsg.FloatAction(false);
            //secondMsg.FloatAction(false);
            //threeMsg.FloatAction(false);
        }

        private void popMsg_MouseLeave(object sender, MouseEventArgs e)
        {
            //Point p = e.GetPosition((IInputElement)sender);
            //if (p.X > popMsg.Width || p.X < -250 || p.Y > popMsg.Height || p.Y < 0)
            //{
            //    firtMsg.FloatAction(true);
            //    secondMsg.FloatAction(true);
            //    threeMsg.FloatAction(true);
            //}

        }

        /// <summary>
        /// 更新消息详情浮动框位置
        /// </summary>
        public void RefreshPopupPostion()
        {
            try
            {
                var mi = typeof(Popup).GetMethod("UpdatePosition", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                mi.Invoke(popMsg, null);
            }
            catch (Exception e)
            { }
        }

        /// <summary>
        /// 接收应用程序消息
        /// </summary>
        /// <param name="msg"></param>
        private void OnAppMsg(object msg)
        {
            PResult obj = (PResult)msg;

            if (obj.Code == "MsgSetting")
            {
                // 应用程序模拟的系统消息
                if (JsonSplit.IsJson(obj.Msg))
                {
                    List<TTbookSystemMessage> lstmsg = JsonConvert.DeserializeObject<List<TTbookSystemMessage>>(obj.Msg);
                    if (lstmsg != null)
                    {
                        OnAdminMsg(lstmsg);
                    }
                }
            }
            else if (obj.Code == "MsgPoReadFlag")
            {
                //定向回价 价格匹配，点击 待办事项，处理按钮 
                if (obj.Info1 == "MsgPo")
                {
                    MsgPo msgPo = Newtonsoft.Json.JsonConvert.DeserializeObject<MsgPo>(obj.Msg);
                    msgUI.ProcSetReadflag(msgPo);   //消息提醒
                    dealMsgUI.ProcSetReadflag(msgPo); //待办事项
                    isGridChange = true;
                }

                ////定向回价 价格匹配，点击 待办事项，处理按钮 
                //if (obj.Info1 == "MsgUIRefreshCount")
                //{
                //    isGridChange = true;
                //}
            }
        }

        /// <summary>
        /// 设为已读
        /// </summary>
        private void SetAllReaded()
        {
            msgUI.SetAllReaded();
            dealMsgUI.SetAllReaded();
            isGridChange = true;
        }

        private async void btnCloseAllMsgSetting_Click(object sender, RoutedEventArgs e)
        {
            await MsgSettingHandler.Instance.DeleteAllDatas(true);
        }
    }
}
