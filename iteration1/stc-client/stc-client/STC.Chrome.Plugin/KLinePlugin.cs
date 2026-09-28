using CefSharp;
using CefSharp.Wpf;
using com.gateway.log;
using STC.Chrome.Plugin.View.KLineChart;
using STC.Common.Handler.Admin;
using STC.Common.Model.Chrome;
using STC.Common.Utils;
using STC.Common.Utils.Events;
using STC.Common.View.UI;
using STC.Frame.Common;
using STC.Frame.Plugin;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace STC.Chrome.Plugin
{
    public class KLinePlugin : AbstractPlugin
    {
        #region 插件信息
        NotifyCloseTabItem notifyCloseTabItem;

        private int _tag = 30;

        public override int Tag
        {
            get { return _tag; }
            set { _tag = value; }
        }

        public override int PluginKey
        {
            get { return (int)PluginKeyEnum.KLinePlugin; }
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
            get { return "K线分析"; }
        }

        public override string Description
        {
            get { return "K线分析"; }
        }

        public override string Author
        {
            get { return "饶嘉琪"; }
        }

        #endregion


        private string OpenKLine = "OpenKLine";

        private readonly string _ModelId = "4000";
        private readonly string _Code = "4014";
        private bool _isFirst = true;


        protected override void load()
        {
            bool isPermission = AdminHandler.Instance.CheckPermisionById(_ModelId, _Code);
            if (!isPermission)
                return;

            //第一次菜单加载
            if (_isFirst)
            {
                AppUtils.Application.PluginManager.Application.Subscribe(OpenKLine, this);
                if (LocalEnv.DictSkin.Contains(LocalEnv.ConfInfo.DefaultSkinStyle))
                {
                    AppUtils.Application.MenuPanel.AddMenu($"pack://application:,,,/STC.Common;component/Images/{LocalEnv.ConfInfo.DefaultSkinStyle}/stc_image_menulist_kline.png", Description, PluginKey, "10");
                }
                else
                {
                    AppUtils.Application.MenuPanel.AddMenu("pack://application:,,,/STC.Common;component/Images/DefaultSkin/stc_image_menulist_kline.png", Description, PluginKey, "10");
                }

                _isFirst = false;

                enabled = false;

                string zipFilePath = AppDomain.CurrentDomain.BaseDirectory + $"bin/Html.zip";
                string unzipFilePath = AppDomain.CurrentDomain.BaseDirectory + $"bin/";

                SharpZip.UnpackFiles(zipFilePath, unzipFilePath);

                return;
            }



            

            if (notifyCloseTabItem == null)
            {
                notifyCloseTabItem = EventHub.GetEvent<NotifyCloseTabItem>();
                notifyCloseTabItem.Subscribe(NotifyCloseTabItem);
            }

            OnMenuClick();

            base.load();
        }

        private void OnMenuClick()
        {
            string url = AppDomain.CurrentDomain.BaseDirectory + $"bin/HTML/KLine/KLineView.html?timestamp={new Random(10000).NextDouble()}";
            //string url = $"/STC.Chrome.Plugin;component/Html/KLine/KLineView.html?timestamp={new Random(10000).NextDouble()}";
            Common.Model.Chrome.ChromeWinModel cw = new Common.Model.Chrome.ChromeWinModel() { IsOpenInMainTabPage = true, Url = url, IsShowAddressBar = true, IsShowStatusBar = false };
            cw.Key = "JsC#KLineDemo";
            cw.Title = "K线分析";
            cw.UseChromePluginObject = true;
            cw.SecurityId = "220210.IB";
            cw.CallBackObjectInstance.Add("cefJS", "KLineViewModel");
            //下面采用异步方式，前端Js也是异步调用的，因而在key后面加上| 告诉插件要异步注册到chrome中
            //cw.CallBackObjectInstance.Add("csharpTest|", "JavascriptCallCharpTest");
            AppUtils.Application.PluginManager.Application.BroadCast("OpenKLine", cw);
        }

        protected override void unload()
        {
            AppUtils.Application.PluginManager.Application.Unsubscribe(OpenKLine, this);
        }

        /// <summary>
        /// ChromeUserControl字典
        /// </summary>
        private Dictionary<string, KLineView> DicChromeUserControl = new Dictionary<string, KLineView>();

        /// <summary>
        /// 释放消息处理
        /// </summary>
        /// <param name="tabItemHashCode"></param>
        private void NotifyCloseTabItem(int tabItemHashCode)
        {
            foreach (System.Windows.Controls.TabItem tabItem in Application.Panel.Items)
            {
                if (tabItem.GetHashCode() == tabItemHashCode)
                {
                    if (tabItem != null && tabItem.Content != null && tabItem.Content is KLineView)
                    {
                        LogHelper.Info("KLineView:" + tabItem.Header + " UserControl_Unloaded ");
                        KLineView mainView = tabItem.Content as KLineView;
                        //if (DicChromeUserControl.ContainsKey(mainView.Key.ToString()))
                        //{
                        //    DicChromeUserControl.Remove(mainView.Key.ToString());
                        //    //移除注册便于扩展方式调用
                        //    remove($"ChromeUserControl_{mainView.Key.ToString()}");
                        //}
                        //mainView.Dispose();
                    }
                }
            }
        }

        /// <summary>
        /// 接受的消息
        /// </summary>
        /// <param name="name"></param>
        /// <param name="o"></param>
        public override void onNotify(string name, object o)
        {
            base.onNotify(name, o);
            if (!string.IsNullOrEmpty(name))
            {
                if (name.Equals(OpenKLine))
                {
                    OpenChrome((ChromeWinModel)o);
                }

                if (!string.IsNullOrEmpty(name))
                {
                    // 接收到清空复位消息
                    if (name.Equals(Consts.Topic_Clear))
                    {

                    }
                }
            }
        }

        public void OpenChrome(ChromeWinModel cw)
        {
            //处理原始参数，如果key为空则取标题
            if (string.IsNullOrEmpty(cw.Key))
            {
                if (!string.IsNullOrEmpty(cw.Title))
                {
                    cw.Key = cw.Title;
                }
                else
                {
                    cw.Key = "KLineDemo";
                }
            }

            if (LocalEnv.ConfInfo.ChromeShowToolBar == "0")
            {
                cw.IsShowAddressBar = false;
                cw.IsShowStatusBar = false;
            }

            if (cw.IsOpenInMainTabPage)
            {
                OpenChromeInTabpage(cw);
            }
            else
            {
                OpenChromeInWindow(cw);
            }
        }

        /// <summary>
        /// 打开Tabpage添加到首页
        /// </summary>
        /// <param name="cw"></param>
        public void OpenChromeInTabpage(ChromeWinModel cw)
        {
            string tagHeader = cw.Title;
            if (TabPageHelper.IsExistTabItem(tagHeader))
            {
                TabItem currentSelectedItem = (TabItem)AppUtils.Application.Panel.SelectedItem;
                KLineView kLineView = currentSelectedItem.Content as KLineView;
                kLineView.ResetDataBySecurityId(cw.SecurityId);

                return;
            }

            try
            {

                KLineView chrom = new KLineView(cw);
                //chrom.SetTopBarVisible(cw.IsShowAddressBar);
                //chrom.SetStatusBarVisible(cw.IsShowStatusBar);

                if (!DicChromeUserControl.ContainsKey(cw.Key))
                {
                    DicChromeUserControl.Add(cw.Key, chrom);
                    //注册便于扩展方式调用
                    registerObject($"KLineView_{cw.Key}", chrom);
                }

                TabPageHelper.OpenTabPage(tagHeader, chrom, PluginKey);
                chrom.SetAddress(cw.Url);


            }
            catch (Exception ex)
            {
                LogHandler.Error(ex.Message);
            }
        }

        /// <summary>
        /// 打开独立窗口
        /// </summary>
        /// <param name="cw"></param>
        public void OpenChromeInWindow(ChromeWinModel cw)
        {
            BorderWin myTradingPopupWin = new BorderWin();
            myTradingPopupWin.WindowState = cw.WindowState;
            myTradingPopupWin.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
            //myTradingPopupWin.Closed += MyTradingPopupWin_Closed;
            try
            {
                KLineView chrom = new KLineView(cw);

                //chrom.SetTopBarVisible(cw.IsShowAddressBar);
                //chrom.SetStatusBarVisible(cw.IsShowStatusBar);
                myTradingPopupWin.SetMainControl(chrom);
                myTradingPopupWin.SetTitleAndKey(cw.Key, cw.Title);
                myTradingPopupWin.Title = cw.Title;
                chrom.SetParentWindow(myTradingPopupWin);
                chrom.SetAddress(cw.Url);

                if (!DicChromeUserControl.ContainsKey(cw.Key))
                {
                    DicChromeUserControl.Add(cw.Key, chrom);
                    //注册便于扩展方式调用
                    registerObject($"KLineView_{cw.Key}", chrom);
                }

                myTradingPopupWin.Width = cw.Width;
                myTradingPopupWin.Height = cw.Height;

                if (cw.WindowState != System.Windows.WindowState.Maximized)
                {
                    if ((cw.MaxHeight != 0) && (cw.MaxWidth != 0))
                    {
                        myTradingPopupWin.MaxHeight = cw.MaxHeight;
                        myTradingPopupWin.MaxWidth = cw.MaxWidth;
                    }
                }

                myTradingPopupWin.Owner = null;
                if ((cw.PositionX != 0) && (cw.PositionY != 0))
                {
                    myTradingPopupWin.Top = cw.PositionX;
                    myTradingPopupWin.Left = cw.PositionY;
                }

                WindowManager.Instance.OpenOnlyOneWindow(cw.Key, myTradingPopupWin, null);
            }
            catch (Exception ex)
            {
                LogHandler.Error(ex.Message);
            }
        }

    }
}
