using CefSharp;
using CefSharp.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace STC.Chrome.Plugin
{
    class LifeSpanPageHandler : ILifeSpanHandler
    {
        bool ILifeSpanHandler.DoClose(IWebBrowser chromiumWebBrowser, IBrowser browser)
        {
            return false;
        }

        void ILifeSpanHandler.OnAfterCreated(IWebBrowser chromiumWebBrowser, IBrowser browser)
        {
           
        }

        void ILifeSpanHandler.OnBeforeClose(IWebBrowser chromiumWebBrowser, IBrowser browser)
        {
            
        }

        public bool OnBeforePopup(IWebBrowser browserControl, IBrowser browser, IFrame frame, string targetUrl,string targetFrameName, WindowOpenDisposition targetDisposition, bool userGesture, IPopupFeatures popupFeatures,IWindowInfo windowInfo, IBrowserSettings browserSettings, ref bool noJavascriptAccess, out IWebBrowser newBrowser)
        {
            newBrowser = null;

            Application.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, new Action(() =>
            {
                //var chromiumWebBrowser = (ChromiumWebBrowser)browserControl;
                //chromiumWebBrowser.Load(targetUrl);
                //return true; //Return true to cancel the popup creation copyright by codebye.com.
                Common.Model.Chrome.ChromeWinModel cw = new Common.Model.Chrome.ChromeWinModel() { IsOpenInMainTabPage = true, Url = targetUrl, IsShowAddressBar = false, IsShowStatusBar = false };
                cw.UseChromePluginObject = true;
                cw.IsShowMenuHandler = false;
                cw.IsShowAddressViewSourceDevTools = false;
                cw.ForceTargetSelf = false;
                cw.ForceTargetOtherPage = true;
                cw.Key = "";
                cw.Title = "";
                Common.Utils.AppUtils.Application.PluginManager.Application.BroadCast("OpenChrome", cw);
            }));

            return true;
        }
    }
}
