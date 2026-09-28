using com.gateway.log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using STC.Frame.Plugin;
using STC.Frame.Common;
using STC.Common.Handler.SystemParam;
using STC.Common.Data;
using STC.Common.Model.Admin;
using STC.Common.Utils;
using STC.Common.Handler.Admin;
using STC.Common.Model.MM;
using STC.Common.ViewModel.Drp.Utils;
using BDB.Trade.Plugin.CustomeUI;
using STC.Common.Handler.Base;

namespace BDB.Trade.Plugin
{
    public class MsgPlugin : AbstractPlugin
    {
        #region 插件信息

        private int _tag = 5;

        public override int Tag
        {
            get { return _tag; }
            set { _tag = value; }
        }

        public override int PluginKey
        {
            get { return (int)PluginKeyEnum.MsgPlugin; }
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
            get { return "消息铃铛"; }
        }

        public override string Description
        {
            get { return "消息铃铛"; }
        }

        public override string Author
        {
            get { return "饶嘉琪"; }
        }

        #endregion

        public const string PARAM_KEY_MSGBELLPOPUP = "STC_Msg_Notice_Bell";

        // <summary>
        /// 消息铃铛弹出样式 1 浮动 0 固定
        /// </summary>
        private string MessageBellPopupStyle = "1";


        private bool isInitSmallbell = false;

        protected override void load()
        {
            //bool result = PermisionHandler.Instance.GetIdcTraderPermission();

            ThreadPool.QueueUserWorkItem(o =>
            {

                try
                {
                    BaseHandler.Instance.InitSubscribe();
                    SystemParamHandler.Instance.Init();

                    if (!isInitSmallbell)
                    {
                        DesignHandler.Instance.OnSmallbellChange += Instance_OnSmallbellChange;
                        DesignHandler.Instance.OnGlobalActivate += Instance_OnGlobalActivate;
                        DesignHandler.Instance.OnNotifyWindowState += Instance_OnNotifyWindowState;
                        Instance_OnSystemReady(null);
                        isInitSmallbell = true;
                    }

                    //if (!result)
                    //{
                    //    LogHelper.Info(string.Format("No IdcTraderPermission(0)", LocalEnv.UserInfo.IdcTradePermissions));
                    //    return;
                    //}

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
                }
                catch (Exception e)
                {
                    LogHelper.Error(e);
                }
            });
            
        }

        private void Instance_OnSmallbellChange()
        {
            //msgView.ClearUI();
            //msgView.Close();
            //msgView = null;

            //msgView = new CustomMsgView();
            //msgView.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;


            msgView.Top = System.Windows.Application.Current.MainWindow.Top + 50;
            msgView.Left = System.Windows.Application.Current.MainWindow.Left + System.Windows.Application.Current.MainWindow.Width - 68;
            //msgView.Show();
            msgView.DealDoubleClick();
        }

        /// <summary>
        /// 接收事件
        /// </summary>
        /// <param name="obj"></param>
        private void Instance_OnGlobalActivate(object obj)
        {
            if (obj.GetType() == typeof(bool))
            {
                SetMsgTopMost((bool)obj);
            }
            else
            {
                SetMsgTopMost();
            }
        }

        private CustomMsgView msgView = null;

        private void ShowCustomMsg()
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                msgView = new CustomMsgView();

                msgView.Top = System.Windows.Application.Current.MainWindow.Top + 60;
                msgView.Left = System.Windows.Application.Current.MainWindow.Left + System.Windows.Application.Current.MainWindow.Width - 130;


                msgView.Show();
            });
        }

        /// <summary>
        /// 设置小玲铛的位置
        /// </summary>
        /// <param name="isForceReset"></param>
        private void SetMsgTopMost(bool isForceReset = false)
        {
            try
            {
                if (msgView != null && System.Windows.Application.Current != null && System.Windows.Application.Current.MainWindow != null)
                {
                    if (isForceReset)
                    {
                        msgView.Top = System.Windows.Application.Current.MainWindow.Top + 50;
                        msgView.Left = System.Windows.Application.Current.MainWindow.Left + System.Windows.Application.Current.MainWindow.Width - 362;
                        DRPTools.GlobalActivate(msgView);
                    }
                    else
                    {
                        if (msgView != null)
                        {
                            if (MessageBellPopupStyle == "0")
                            {
                                //固定
                                CustomUI.MsgUI msgui = (CustomUI.MsgUI)msgView.FindName("msgUI");

                                if (msgui != null)
                                {
                                    msgui.Dispatcher.Invoke(new Action(() =>
                                    {
                                        if (msgView.Visibility == System.Windows.Visibility.Hidden)
                                        {
                                            msgView.Visibility = System.Windows.Visibility.Visible;
                                        }

                                        if (msgui != null)
                                        {
                                            if ((msgui.FindName("btnMsg") as System.Windows.Controls.Button).Visibility == System.Windows.Visibility.Visible)
                                            {
                                                msgView.Top = System.Windows.Application.Current.MainWindow.Top + 50;
                                                msgView.Left = System.Windows.Application.Current.MainWindow.Left + System.Windows.Application.Current.MainWindow.Width - 68;
                                            }
                                            else
                                            {
                                                msgView.Top = System.Windows.Application.Current.MainWindow.Top + 50;
                                                msgView.Left = System.Windows.Application.Current.MainWindow.Left + System.Windows.Application.Current.MainWindow.Width - 362;
                                                DRPTools.GlobalActivate(msgView);
                                            }
                                        }
                                    }));
                                }
                            }
                            else
                            {
                                //浮动
                                DRPTools.GlobalActivate(msgView);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogHandler.Error(ex);
            }
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
            req.ParamKey = PARAM_KEY_MSGBELLPOPUP;
            req.Username = LocalEnv.UserInfo.UserName;
            
            TbookSystemParameterReplyPo reply = await GetReply(req);
            if (reply != null && reply.IsSuccess())
            {
                if (reply.Datas != null && reply.Datas.tbookSystemParameter != null && !string.IsNullOrEmpty(reply.Datas.tbookSystemParameter.param_value))
                {
                    MessageBellPopupStyle = reply.Datas.tbookSystemParameter.param_value ?? "1";
                }
            }

            DesignHandler.Instance.OnSettingChange += Instance_OnSettingChange;
            SetMsgTopMost();
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

        /// <summary>
        /// 接收个人设置 弹窗设置 改变事件
        /// </summary>
        /// <param name="obj"></param>
        private void Instance_OnSettingChange(object obj)
        {
            if (obj is NewSettingMsg)
            {
                NewSettingMsg msg = (NewSettingMsg)obj;
                if (msg.key == "MessageBellPopupStyle")
                {
                    MessageBellPopupStyle = msg.value;
                    SetMsgTopMost();
                }
            }
        }

        /// <summary>
        /// 窗口最小化通知事件
        /// </summary>
        /// <param name="obj"></param>
        private void Instance_OnNotifyWindowState(object obj)
        {
            if (msgView != null)
            {
                if (MessageBellPopupStyle == "0")
                {
                    CustomUI.MsgUI msgui = (CustomUI.MsgUI)msgView.FindName("msgUI");

                    msgui.Dispatcher.Invoke(new Action(() =>
                    {
                        //固定
                        if (obj.GetType() == typeof(System.Windows.WindowState))
                        {
                            if ((System.Windows.WindowState)obj == System.Windows.WindowState.Minimized)
                            {
                                msgView.Visibility = System.Windows.Visibility.Hidden;
                            }
                            else
                            {
                                if (msgView.Visibility != System.Windows.Visibility.Visible)
                                    msgView.Visibility = System.Windows.Visibility.Visible;
                            }
                        }

                    }));
                }
            }
        }
    }
}
