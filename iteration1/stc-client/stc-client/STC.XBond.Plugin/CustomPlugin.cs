using Newtonsoft.Json;
using STC.Common.Data;
using STC.Common.Handler.Admin;
using STC.Common.Handler.SystemParam;
using STC.Common.Model.TBook;
using STC.Common.Utils;
using STC.Common.View.UI;
using STC.Frame.Common;
using STC.Frame.Plugin;
using STC.XBond.Plugin.CustomeUI;
using STC.XBond.Plugin.Utils;
using STC.XBond.Plugin.View.Setting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace STC.XBond.Plugin
{
    /// <summary>
    /// 消息提醒插件类
    /// </summary>
    public class CustomPlugin : AbstractPlugin
    {
        public override string Author
        {
            get
            {
                return "饶嘉琪";
            }
        }

        public override string Description
        {
            get
            {
                return "消息插件";
            }
        }

        private bool enabled = true;
        public override bool Enabled
        {
            get
            {
                return enabled;
            }

            set
            {
                enabled = value;
            }
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

        protected new bool isMain = false;
        public override bool IsMain { get; set; }

        protected new bool isAlone = true;
        public override bool IsAlone { get; set; }

        public override int PluginKey
        {
            get
            {
                return (int)PluginKeyEnum.CustomPlugin;
            }
        }

        public override string PluginName
        {
            get
            {
                return "消息插件";
            }
        }

        private int _tag = 0;
        public override int Tag
        {
            get { return _tag; }
            set { _tag = value; }
        }

        // <summary>
        /// 消息铃铛弹出样式 1 浮动 0 固定
        /// </summary>
        public static string MessageBellPopupStyle = "1";
        /// <summary>
        /// 跳数字且弹框(1)
        /// 只跳数字不弹框(0)
        /// </summary>
        public static string PopMsgMode = "1";

        private readonly string _ModelId = "4000";
        private readonly string _Code = "4002";


        private void InitSetting()
        {

            DesignHandler.Instance.OnShowSetting += Instance_ShowDesign;
        }

        private void Instance_ShowDesign(object obj)
        {
            if (obj != null)
            {
                SettingView settingView = obj as SettingView;
                if (settingView != null)
                {
                    bool isPermission_tbook = AdminHandler.Instance.CheckPermisionById(_ModelId, _Code);

                    if (isPermission_tbook)
                    {
                        //如果不是BDB则加载个人资料
                        if (!STC.Common.Utils.CommonUtils.IsBDBAppName())
                        {
                            TabItem userTabItem = new TabItem();
                            UserInfoView userInfoView = new UserInfoView();
                            userTabItem.Content = userInfoView;
                            userTabItem.Header = "个人资料";
                            settingView.AddTabPage(userTabItem);
                        }

                        if (LocalEnv.ConfInfo.IsShowPriceCondition != "1")
                        {
                            settingView.Height -= 100;
                        }

                        bool isAllow = AdminHandler.Instance.CheckPermission("1003", null);
                        if (isAllow)
                        {
                            bool isHaveInner = true;

                            //如果是BDB需要判断QB的权限
                            if (CommonUtils.IsBDBAppName())
                            {
                                bool isPermission = AdminHandler.Instance.CheckPermisionById("4000", "4013");
                                if (!isPermission || (LocalEnv.UserInfo.BdbInnerPermissions ?? "0") != "1")
                                {
                                    isHaveInner = false;
                                }
                            }

                            if (isHaveInner)
                            {
                                TabItem likeTabItem = new TabItem();
                                LikeView likeView = new LikeView();
                                likeTabItem.Content = likeView;
                                likeTabItem.Header = "偏好设置";
                                settingView.AddTabPage(likeTabItem);
                            }
                        }
                    }

                    TabItem msgTabItem = new TabItem();
                    MsgAlertSettingView msgSettingView = new MsgAlertSettingView();
                    msgTabItem.Content = msgSettingView;
                    msgTabItem.Header = "提醒设置";
                    settingView.AddTabPage(msgTabItem);
                }
            }
        }

        private bool isInitSmallbell = false;
        protected override void load()
        {
            //bool isPermission = AdminHandler.Instance.CheckPermisionById(_ModelId, _Code);
            //if (!isPermission)
            //    return;

            //初始化强提醒显示区
            STC.Common.View.StrongNotify.StrongNotifyHandler.Instance.InitScreen();

            SystemCheckHelper.Instance.Init();

            InitSetting();

            if (!isInitSmallbell)
            {
                DesignHandler.Instance.OnSmallbellChange += Instance_OnSmallbellChange;// 界面上小铃铛
                //DesignHandler.Instance.OnGlobalActivate += Instance_OnGlobalActivate;//恢复原位 去掉
                DesignHandler.Instance.OnSystemReady += Instance_OnSystemReady;//系统备妥
                //DesignHandler.Instance.OnNotifyWindowState += Instance_OnNotifyWindowState;// 主界面WindowState变化
                isInitSmallbell = true;
            }

            this.broadcast("loadCRM", string.Empty);
            
            base.load();
        }

        private CustomMsgView msgView = null;

        private void ShowCustomMsg()
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                try
                {
                    DesignHandler.Instance.OnSettingChange += Instance_OnSettingChange;
                    
                    msgView = new CustomMsgView();
                    msgView.Show();

                    SetBellPopupStyle();
                }
                catch (Exception ex)
                {
                    LogHandler.Info(ex);
                }
            });
        }

        private void MainWindow_StateChanged(object sender, EventArgs e)
        {
            Grid fixMsgPart = (Grid)System.Windows.Application.Current.MainWindow.FindName("FixMsgPart");
            if (fixMsgPart != null)
            {
                msgView.Top = System.Windows.Application.Current.MainWindow.Top + System.Windows.Application.Current.MainWindow.Height - fixMsgPart.ActualHeight;
                msgView.Left = System.Windows.Application.Current.MainWindow.Left + fixMsgPart.ActualWidth - 360;
                msgView.Height = msgView.msgUI.Height = fixMsgPart.ActualHeight - 1;
            }

            if (System.Windows.Application.Current.MainWindow.WindowState == System.Windows.WindowState.Minimized)
            {
                msgView.Visibility = System.Windows.Visibility.Hidden;
            }
            else
            {
                msgView.Visibility = System.Windows.Visibility.Visible;
            }
        }

        private void MainWindow_LocationChanged(object sender, EventArgs e)
        {
            Grid fixMsgPart = (Grid)System.Windows.Application.Current.MainWindow.FindName("FixMsgPart");
            if (fixMsgPart != null)
            {
                msgView.Top = System.Windows.Application.Current.MainWindow.Top + System.Windows.Application.Current.MainWindow.Height - fixMsgPart.ActualHeight;
                msgView.Left = System.Windows.Application.Current.MainWindow.Left + fixMsgPart.ActualWidth - 360;
                msgView.Height = msgView.msgUI.Height = fixMsgPart.ActualHeight - 1;
            }
        }

        private void MainWindow_SizeChanged(object sender, System.Windows.SizeChangedEventArgs e)
        {
            Grid fixMsgPart = (Grid)System.Windows.Application.Current.MainWindow.FindName("FixMsgPart");
            if (fixMsgPart != null)
            {
                msgView.Top = System.Windows.Application.Current.MainWindow.Top + System.Windows.Application.Current.MainWindow.Height - fixMsgPart.ActualHeight;
                msgView.Left = System.Windows.Application.Current.MainWindow.Left + fixMsgPart.ActualWidth - 360;
                msgView.Height = msgView.msgUI.Height = fixMsgPart.ActualHeight - 1;
            }
        }

        private void Instance_OnSmallbellChange()
        {
            if (MessageBellPopupStyle == "1")
            {
                msgView.Top = System.Windows.Application.Current.MainWindow.Top + 50;
                msgView.Left = System.Windows.Application.Current.MainWindow.Left + System.Windows.Application.Current.MainWindow.Width - 158;

                msgView.DealDoubleClick();
            }
            else
            {
                Grid fixMsgPart = (Grid)System.Windows.Application.Current.MainWindow.FindName("FixMsgPart");
                if (fixMsgPart != null)
                {
                    if (msgView.msgUI.borderMain.Visibility == System.Windows.Visibility.Collapsed)
                    {
                        msgView.Show();
                        msgView.DealDoubleClick();
                        msgView.Top = System.Windows.Application.Current.MainWindow.Top + System.Windows.Application.Current.MainWindow.Height - fixMsgPart.ActualHeight;
                        msgView.Left = System.Windows.Application.Current.MainWindow.Left + fixMsgPart.ActualWidth - 360;
                        msgView.Height = msgView.msgUI.Height = fixMsgPart.ActualHeight - 1;
                    }
                    else
                    {
                        msgView.DealClose();
                    }
                }
            }
        }

        public override void BeforeTerminating()
        {
        }

        public override void onNotify(string name, object o)
        {
        }

        private void SetPopMsg()
        {
            if (MessageBellPopupStyle == "1" && PopMsgMode != "1")
            {
                //固定
                STC.XBond.Plugin.CustomUI.MsgUI msgui = (STC.XBond.Plugin.CustomUI.MsgUI)msgView.FindName("msgUI");

                if (msgui != null)
                {
                    msgui.ClearPopMsg();
                }
            }
        }

        /// <summary>
        /// 当系统准备好后，读取个人设置内容，设置小玲铛
        /// </summary>
        /// <param name="obj"></param>
        private void Instance_OnSystemReady(object obj)
        {

            Thread t = new Thread(() =>
            {
                SystemParamHandler.Instance.Init();

                // 1. 读配置
                UserSetting us = SystemParamHandler.Instance.GetUserSetting();

                int index = 0;
                while (index < 3 && us == null)
                {
                    index++;
                        Thread.Sleep(500);
                        us = SystemParamHandler.Instance.GetUserSetting();
                }

                if (us != null)
                {
                    if (us.inf2 != null)
                    {
                        ConsistencyCheckPo consistencyCheckPo = JsonConvert.DeserializeObject<ConsistencyCheckPo>(us.inf2);
                        if (consistencyCheckPo != null)
                        {
                            MessageBellPopupStyle = consistencyCheckPo.MessageBellPopupStyle ?? "1";
                            PopMsgMode = consistencyCheckPo.PopMsgMode ?? "1";
                        }
                    }
                }


                // 2.打开铃铛
                while (true)
                {
                    if (DesignHandler.IsShow)
                    {
                        ShowCustomMsg();

                        break;
                    }
                    else
                    {
                        try
                        {
                            Thread.Sleep(1000);
                        }
                        catch (Exception e)
                        { }
                    }

                }
            }, (int)ThreadPriority.Highest);
            t.IsBackground = true;
            t.Start();
        }

        /// <summary>
        /// 接收个人设置 弹窗设置 改变事件
        /// </summary>
        /// <param name="obj"></param>
        private void Instance_OnSettingChange(object obj)
        {
            if (obj is Common.Model.MM.NewSettingMsg)
            {
                Common.Model.MM.NewSettingMsg msg = (Common.Model.MM.NewSettingMsg)obj;
                if (msg.key == "MessageBellPopupStyle")
                {
                    MessageBellPopupStyle = msg.value;
                    SetBellPopupStyle();
                }
                else if (msg.key == "PopMsgMode")
                {
                    PopMsgMode = msg.value;
                    SetPopMsg();
                }
            }
        }

        private void SetBellPopupStyle()
        {
            if (MessageBellPopupStyle == "0")
            {
                Grid fixMsgPart = (Grid)System.Windows.Application.Current.MainWindow.FindName("FixMsgPart");
                if (fixMsgPart != null)
                {
                    System.Windows.Application.Current.MainWindow.SizeChanged += MainWindow_SizeChanged;
                    System.Windows.Application.Current.MainWindow.LocationChanged += MainWindow_LocationChanged;
                    System.Windows.Application.Current.MainWindow.StateChanged += MainWindow_StateChanged;

                    msgView.DealDoubleClick();
                    msgView.DealClose();
                    msgView.Top = System.Windows.Application.Current.MainWindow.Top + System.Windows.Application.Current.MainWindow.Height - fixMsgPart.ActualHeight;
                    msgView.Left = System.Windows.Application.Current.MainWindow.Left + fixMsgPart.ActualWidth - 360;
                    msgView.Height = msgView.msgUI.Height = fixMsgPart.ActualHeight - 1;
                }
            }
            else
            {
                System.Windows.Application.Current.MainWindow.SizeChanged -= MainWindow_SizeChanged;
                System.Windows.Application.Current.MainWindow.LocationChanged -= MainWindow_LocationChanged;
                System.Windows.Application.Current.MainWindow.StateChanged -= MainWindow_StateChanged;

                msgView.DealClose();
                msgView.Top = System.Windows.Application.Current.MainWindow.Top + 60;
                msgView.Left = System.Windows.Application.Current.MainWindow.Left + System.Windows.Application.Current.MainWindow.Width - 130;

                msgView.Visibility = System.Windows.Visibility.Visible;
            }

            SetPopMsg();
        }
    }
}
