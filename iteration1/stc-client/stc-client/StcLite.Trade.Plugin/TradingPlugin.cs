using com.gateway.log;
using STC.Common.Utils;
using BDB.Trade.Plugin.Cache;
using BDB.Trade.Plugin.Handler;
using BDB.Trade.Plugin.Handler.DataProcesser;
using BDB.Trade.Plugin.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BDB.Price.Plugin.Handler;
using BDB.Price.Plugin.Chart.Deal;
using STC.Frame.Plugin;
using STC.Frame.Common;
using STC.Common.Data;
using ExecutionsHandler = BDB.Trade.Plugin.Handler.ExecutionsHandler;
using OrdersHandler = BDB.Trade.Plugin.Handler.OrdersHandler;
using STC.Common.View.UI;
using System.Windows.Controls;
using BDB.Trade.Plugin.View.UserSetting;
using BDB.Trade.Plugin.View.Trade.Card;
using BDB.Price.Plugin.Utils;
using STC.Common.Model.Admin;
using STC.Common.Handler.Admin;
using STC.Common.Model.Base;
using System.Windows;

namespace BDB.Trade.Plugin
{
    public class TradingPlugin : AbstractPlugin
    {
        /// <summary>
        /// 1： 【最新成交-中债估值】 0：【最新成交-昨收】
        /// </summary>
        public static string BP_WITH_CCDC = "0";

        #region 插件信息

        private int _tag = 0;

        public override int Tag
        {
            get { return _tag; }
            set { _tag = value; }
        }

        public override int PluginKey
        {
            get { return (int)PluginKeyEnum.TradingPlugin; }
        }

        protected new bool isMain = false;
        public override bool IsMain { get; set; }

        protected new bool isAlone = true;
        public override bool IsAlone { get; set; }

        protected new bool enabled = true;
        public override bool Enabled
        {
            get { return enabled; }
            set { enabled = value; }
        }

        private bool isFullScreen = false;
        public override bool IsFullScreen
        {
            get
            {
                return isFullScreen;
            }

            set
            {
                isFullScreen = value;
            }
        }

        public override string PluginName
        {
            get { return "行情看板"; }
        }

        public override string Description
        {
            get { return "行情看板"; }
        }

        public override string Author
        {
            get { return "jiaqi.rao"; }
        }

        #endregion

        private bool _isFirst = true;

        protected override void load()
        {
            //第一次菜单加载
            if (_isFirst)
            {
                InitSetting();

                if (LocalEnv.DictSkin.Contains(LocalEnv.ConfInfo.DefaultSkinStyle))
                {
                    AppUtils.Application.MenuPanel.AddMenu($"pack://application:,,,/STC.Common;component/Images/{LocalEnv.ConfInfo.DefaultSkinStyle}/stc_image_menulist_neibukanban.png", Description, PluginKey, "3");
                }
                else
                {
                    AppUtils.Application.MenuPanel.AddMenu("pack://application:,,,/STC.Common;component/Images/DefaultSkin/stc_image_menulist_neibukanban.png", Description, PluginKey, "3");
                }

                _isFirst = false;
                enabled = false;

                
                if (LocalEnv.UserInfo.BdbBboPermissions == "1")
                {
                    //Add BestMarket Modle
                    STC.Common.Model.Admin.Module m = new STC.Common.Model.Admin.Module() { modulename = "市场最优", closeby = "test", createtime = "2025-03-20 16:00:00", id = "4055", inf1 = "web", inf2 = @"http://172.16.137.84:2013/cust", inf3 = "", inf4 = "0", inf5 = "" };
                    if (LocalEnv.DictSkin.Contains(LocalEnv.ConfInfo.DefaultSkinStyle))
                    {
                        AppUtils.Application.MenuPanel.AddMenu($"pack://application:,,,/STC.Common;component/Images/{LocalEnv.ConfInfo.DefaultSkinStyle}/stc_image_menulist_xiazaizhongxin.png", m.modulename, int.Parse(m.id), "7");
                    }
                    else
                    {
                        AppUtils.Application.MenuPanel.AddMenu("pack://application:,,,/STC.Common;component/Images/DefaultSkin/stc_image_menulist_xiazaizhongxin.png", m.modulename, int.Parse(m.id), "7");
                    }
                }

                if (LocalEnv.UserInfo.BdbStaratPermissions == "1")
                {
                    STC.Common.Model.Admin.Module m = new STC.Common.Model.Admin.Module() { modulename = "策略定价", closeby = "test", createtime = "2025-07-02 16:00:00", id = "4056", inf1 = "web", inf2 = @"http://172.16.137.84:2011/market-data-info", inf3 = "", inf4 = "0", inf5 = "" };
                    if (LocalEnv.DictSkin.Contains(LocalEnv.ConfInfo.DefaultSkinStyle))
                    {
                        AppUtils.Application.MenuPanel.AddMenu($"pack://application:,,,/STC.Common;component/Images/{LocalEnv.ConfInfo.DefaultSkinStyle}/stc_image_menulist_xiazaizhongxin.png", m.modulename, int.Parse(m.id), "7");
                    }
                    else
                    {
                        AppUtils.Application.MenuPanel.AddMenu("pack://application:,,,/STC.Common;component/Images/DefaultSkin/stc_image_menulist_xiazaizhongxin.png", m.modulename, int.Parse(m.id), "7");
                    }
                }

                //是否在菜单显示演示程序菜单入口
                string isShowPlotDemoMenu = System.Configuration.ConfigurationManager.AppSettings["IsShowPlotDemoMenu"];
                isShowPlotDemoMenu = string.IsNullOrEmpty(isShowPlotDemoMenu) ? "0" : isShowPlotDemoMenu;
                if (isShowPlotDemoMenu == "1")
                { 
                    //Add Demo Modle
                    string urifilename = AppDomain.CurrentDomain.BaseDirectory + $@"bin\HTML\index.html";
                    if (System.IO.File.Exists(urifilename))
                    {
                        STC.Common.Model.Admin.Module demoweb = new STC.Common.Model.Admin.Module() { modulename = "C# Js Call", closeby = "test", createtime = "2025-03-20 16:00:00", id = "9898", inf1 = "web", inf2 = @"bin\HTML\index.html", inf3 = "", inf4 = "0", inf5 = "" };
                        if (LocalEnv.DictSkin.Contains(LocalEnv.ConfInfo.DefaultSkinStyle))
                        {
                            AppUtils.Application.MenuPanel.AddMenu($"pack://application:,,,/STC.Common;component/Images/{LocalEnv.ConfInfo.DefaultSkinStyle}/stc_image_menulist_xiazaizhongxin.png", demoweb.modulename, int.Parse(demoweb.id), "7");
                        }
                        else
                        {
                            AppUtils.Application.MenuPanel.AddMenu("pack://application:,,,/STC.Common;component/Images/DefaultSkin/stc_image_menulist_xiazaizhongxin.png", demoweb.modulename, int.Parse(demoweb.id), "7");
                        }
                    }
                }

                BDBWinHandler.Instance.RegSystemReadyEvent();

                // 读取bp模式
                Instance_OnSystemReady(null);
                //return;
            }

            MsgSettingHandler.Instance.Init();

            //SqliteManager.Initialize();
            LayoutSetttingCache.Instance.InitSetting();

            //MyExecutionProcesser.Processer.Init();//todo_lily:
            //MyOrderProcesser.Processer.Init();//todo_lily:
            //EmsOrderProcesser.Processer.Init();//todo_lily:
            LastExecutionProcesser.Processer.Init();
            //MarketExecutionProcesser.Processer.Init();//todo_lily:
            ChartExecCache.Instance.Init();


            ThreadPool.QueueUserWorkItem(o =>
            {
                try
                {
                    ExecutionsHandler.Instance.init();
                    OrdersHandler.Instance.init();
                    //EmsOrdersHandler.Instance.init();

                    //OpenWinHandler.Instance.SubscribeSendQQMsg(Instance_SendQQMsg);
                    //SimularDataHandler.Instance.StartSimular();
                    //SimularDataHandler.Instance.StartSimularExec();

                    //LoadQQ();

                    OpenWinHandler.Instance.SubscribePriceWin(Instance_PriceWinMsg);
                }
                catch (Exception e)
                {
                    LogHelper.Error("RequestOrders", e);
                }
            });

            //订阅App消息
            STC.Common.Handler.Base.BaseHandler.Instance.SubscribeAppMsg(OnAppMsg);

            TabPageHelper.OpenTabPage(this.Description, new TradeMainView(), PluginKey);
        }

        /// <summary>
        /// 当系统准备好后，读取个人设置内容，设置小玲铛
        /// </summary>
        /// <param name="obj"></param>
        private async void Instance_OnSystemReady(object obj)
        {
            //UserSetting us = SystemParamHandler.Instance.GetUserSetting();
            //if (us != null)
            //{
            //    ConsistencyCheckPo consistencyCheckPo = JsonConvert.DeserializeObject<ConsistencyCheckPo>(us.inf2);
            //    if (consistencyCheckPo != null)
            //    {
            //        MessageBellPopupStyle = consistencyCheckPo.MessageBellPopupStyle ?? "1";
            //    }
            //}
            SystemParameterReq req = new SystemParameterReq();
            req.ParamKey = ViewModel.UserSetting.NewUserSettingViewModel.PARAM_KEY_BpWithCCDC;
            req.Username = LocalEnv.UserInfo.UserName;

            TbookSystemParameterReplyPo reply = await GetReply(req);
            if (reply != null && reply.IsSuccess())
            {
                if (reply.Datas != null && reply.Datas.tbookSystemParameter != null && !string.IsNullOrEmpty(reply.Datas.tbookSystemParameter.param_value))
                {
                    BP_WITH_CCDC = reply.Datas.tbookSystemParameter.param_value == "1" ? "1" : "0";
                }
            }
        }

        public async Task<TbookSystemParameterReplyPo> GetReply(SystemParameterReq req)
        {
            TbookSystemParameterReplyPo reply = null;
            await Task.Run(() =>
            {
                reply = AdminHandler.Instance.querySystemParameter(req);
            });

            return reply;
        }

        //private async void LoadQQ()
        //{
        //    await LoginHandler.Instance.GetQQ(string.Empty, string.Empty);
        //}

        private void InitSetting()
        {
            DesignHandler.Instance.OnShowSetting += Instance_ShowDesign;
            DesignHandler.Instance.OnPriceAndOrderView_BDB = Instance_OnPriceAndOrderView_BDB;
        }

        private STC.Common.Interface.IPriceAndOrderViewInterface Instance_OnPriceAndOrderView_BDB()
        {
            PriceAndOrderView paov = new PriceAndOrderView("Tradebook");
            paov.CardMode = Price.Plugin.Model.CardShowType.AloneWindow;
            return paov;
        }

        private void Instance_ShowDesign(object obj)
        {
            if (obj != null)
            {
                SettingView settingView = obj as SettingView;
                if (settingView != null)
                {
                    TabItem saleTabItem = new TabItem();
                    NewUserSettingView usersettingview = new NewUserSettingView();
                    saleTabItem.Content = usersettingview;
                    saleTabItem.Header = "布局设置";
                    settingView.AddTabPage(saleTabItem);
                }
            }
        }


        #region 收到发送渠道QQ消息
        // 收到ExcelPlugIn 消息
        private void Instance_SendQQMsg(object obj)
        {
            if (obj != null)
            {
                string objmsg = obj.ToString();

                Task.Factory.StartNew(async () =>
                {
                    if (objmsg.Contains(":"))
                    {
                        string broker = objmsg.Split(':')[0];
                        string msg = objmsg.Split(':')[1];
                        string qq = ResulfApiHelper.GetQQByBrokerCN(broker);
                        await SendMessageToQQ(qq, msg);
                    }
                });
            }
        }

        /// <summary>
        /// 直接点击打开小窗，则自动置顶，如果是接受消息打开也自动置顶
        /// </summary>
        /// <param name="securityID"></param>
        public static async Task SendMessageToQQ(string qq, string msg)
        {
            await ResulfApiHelper.ManualSendMessageToQQRequest(qq, msg);
        }
        #endregion


        // 收到ExcelPlugIn 消息
        private void Instance_PriceWinMsg(object obj)
        {
            if (obj != null)
            {
                string securityID = obj.ToString();
                if (securityID != null)
                {
                    STC.Common.Handler.Base.BaseHandler.Instance.PublishAppMsg(new STC.Common.Model.Fix.Message.PResult() { Code = "PopupBDBQuotesMiniWindows", Info1 = securityID });
                }
            }
        }

        /// <summary>
        /// 接收应用程序消息
        /// </summary>
        /// <param name="msg"></param>
        private void OnAppMsg(object msg)
        {
            STC.Common.Model.Fix.Message.PResult obj = (STC.Common.Model.Fix.Message.PResult)msg;
            if (obj.Code == "MsgSettingHandler")
            {
                //关闭所有条件
                if (obj.Info1 == "DeleteAllDatas")
                {
                    SubDeleteAllDatas();
                }
            }
            else if (obj.Code == "PopupBDBQuotesMiniWindows")
            {
                System.Windows.Application.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, new Action(() =>
                {
                    //打开BDB行情小窗
                    string instr = obj.Info1.ToString();
                    List<string> lstsecurityID = instr.Split(',').ToList();
                    List<BondsInfoPo> LstBondInfoPo = new List<BondsInfoPo>();
                    foreach (string securityID in lstsecurityID)
                    {
                        BondsInfoPo bondsInfoPo = CommonUtils.GetBondsInfoPoBySecurityID(securityID);
                        if (bondsInfoPo != null)
                        {
                            LstBondInfoPo.Add(bondsInfoPo);
                        }
                    }

                    if (LstBondInfoPo.Count > 0)
                    {
                        ArrangeOpenQuotesMiniWindows(LstBondInfoPo);
                    }
                    else
                    {
                        string alertmsg = "基础数据加载中或未查到该债券基础信息！ 请稍后重试...";
                        STC.Common.Handler.Base.BaseHandler.Instance.PublishAppMsg(new STC.Common.Model.Fix.Message.PResult() { Code = "getFQ", Info1 = "WebFQ", Info2 = "ShowFQMessage", Info3 = alertmsg, Info4 = "提醒:" });
                    }
                }));
            }

        }

        /// <summary>
        /// 打开BDB行情小窗
        /// </summary>
        /// <param name="securityID"></param>
        /// <param name="isAlwaysTopmost"></param>
        private void ArrangeOpenQuotesMiniWindows(List<BondsInfoPo> lstBondInfoPo)
        {
            //打开BDB行情小窗
            System.Windows.Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                string securityID = string.Empty;
                int count = lstBondInfoPo.Count;
                Point lastWindowPosition; // 记录上一个窗口的位置
                // 错位间距
                double offset = 40;
                // 获取屏幕尺寸
                double screenWidth = SystemParameters.PrimaryScreenWidth;
                double screenHeight = SystemParameters.PrimaryScreenHeight;

                // 计算窗口默认大小（假设窗口大小为 400x300）
                double windowWidth = 325;
                double windowHeight = 475;

                // 计算整个窗口群的初始起点，使其尽可能居中
                double totalOffsetX = (count - 1) * offset;
                double totalOffsetY = (count - 1) * offset;
                double startX = (screenWidth - windowWidth - totalOffsetX) / 2;
                double startY = (screenHeight - windowHeight - totalOffsetY) / 2;

                // 设置第一个窗口的位置
                lastWindowPosition = new Point(startX, startY);

                for (int i = 0; i < lstBondInfoPo.Count; i++)
                {
                    securityID = lstBondInfoPo[i].Bond_ID;

                    object cardobj = Price.Plugin.Cache.CardWinCache.Instance.GetBySecurity(securityID);

                    if (cardobj == null)
                    {
                        Price.Plugin.View.Trade.Card.CardWindow cardWindow = new Price.Plugin.View.Trade.Card.CardWindow(false);
                        cardWindow.SecurityId = securityID;
                        cardWindow.ResizeMode = ResizeMode.CanResize;
                        cardWindow.WindowStartupLocation = WindowStartupLocation.Manual;
                        cardWindow.Left = lastWindowPosition.X;
                        cardWindow.Top = lastWindowPosition.Y;
                        cardWindow.Height = windowHeight;
                        cardWindow.Width = windowWidth;
                        // 计算下一个窗口的位置
                        lastWindowPosition.X += offset;
                        lastWindowPosition.Y += offset;
                        cardWindow.Show();

                        Price.Plugin.Cache.CardWinCache.Instance.AddOrUpdate(securityID, cardWindow);
                    }
                    else
                    {
                        Price.Plugin.View.Trade.Card.CardWindow cardWindow = cardobj as Price.Plugin.View.Trade.Card.CardWindow;
                        cardWindow.Activate();
                    }
                }
            }));
        }


        /// <summary>
        /// 关闭所有条件
        /// </summary>
        private async void SubDeleteAllDatas()
        {
            await MsgSettingHandler.Instance.DeleteAllDatas(true);
        }
    }
}
