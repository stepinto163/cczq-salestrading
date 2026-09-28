using CefSharp;
using com.gateway.log;
using Newtonsoft.Json;
using STC.Chrome.Plugin.Model;
using STC.Common.Model.Base;
using STC.Common.Model.Chrome;
using STC.Common.Utils;
using STC.Common.View.Card;
using STC.Common.View.UI;
using STC.Common.ViewModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace STC.Chrome.Plugin.Demo
{
    /// <summary>
    /// js直接调用的c#后台方法，暴漏方法名采用 骆驼命名法
    /// </summary>
    public class CefJsEventDemo
    {
        private ChromeUserControl ChromeCtrl;
        private object updlock = new object();


        /// <summary>
        ///  深度行情ViewModel
        /// </summary>
        public QuotationListViewModel viewModel;

        /// <summary>
        /// 债券代码
        /// </summary>
        private string _SecurityID = string.Empty;

        /// <summary>
        /// 债券代码
        /// </summary>
        public string SecurityID
        {
            get
            {
                return this._SecurityID;
            }

            set
            {
                if ((this._SecurityID != value))
                {
                    // 切换债券
                    this._SecurityID = value;

                    if (!string.IsNullOrEmpty(_SecurityID))
                    {
                        if (viewModel == null)
                        {
                            viewModel = new QuotationListViewModel();
                            viewModel.SecurityID = value;
                            viewModel.OnListChangeEvent += ViewModel_OnListChangeEvent;
                        }
                        else
                        {
                            viewModel.SecurityID = value;
                        }
                    }
                }
            }
        }


        /// <summary>
        /// js直接调用的c#后台演示类
        /// </summary>
        /// <param name="browse"></param>
        public CefJsEventDemo(ChromeUserControl browse)
        {
            this.ChromeCtrl = browse;
        }

        /// <summary>
        /// 设置URL，骆驼命名法
        /// </summary>
        /// <param name="url"></param>
        public void setUrl(string code, string url)
        {
            Random rd = new Random(10000);
            if (!string.IsNullOrEmpty(code))
            {
                url = AppDomain.CurrentDomain.BaseDirectory + $"bin/HTML/{url}?username={code}&timestamp={rd.NextDouble()}";
            }
            else
            {
                url = AppDomain.CurrentDomain.BaseDirectory + $"bin/HTML/{url}?timestamp={rd.NextDouble()}";
            }

            ChromeCtrl.SetAddress(url);
        }

        /// <summary>
        /// 获取用户登录名和密码，骆驼命名法
        /// </summary>
        /// <param name="_data"></param>
        /// <returns></returns>
        public string getUserNameSID(string _data)
        {
            UserInfoClassModel ui = new UserInfoClassModel() { UserInfo = LocalEnv.UserInfo, Msg = "Success" };
            return JsonConvert.SerializeObject(ui);
        }

        /// <summary>
        /// 搜索债券，骆驼命名法
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public string getSymbol(string code)
        {
            string res = string.Empty;
            string txt = code.Replace("'", "");
            List<SimpleBondsInfoPo> result = new List<SimpleBondsInfoPo>();
            if (txt.Contains(" "))
            {
                string[] arr = txt.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);

                foreach (string str in arr)
                {
                    List<SimpleBondsInfoPo> selectDatas = CommonUtils.GetBondsInfoPo(str, 20);
                    result.AddRange(selectDatas);
                }
            }
            else
                result = CommonUtils.GetBondsInfoPo(txt.Trim(), 20);

            return JsonConvert.SerializeObject(result);
        }

        /// <summary>
        /// 获取债券的成交列表，骆驼命名法
        /// </summary>
        /// <param name="securityId"></param>
        /// <returns></returns>
        public void setSecurityID(string securityId)
        {
            // 初始化实例
            this.SecurityID = securityId;
        }


        /// <summary>
        /// 打开行情小窗
        /// </summary>
        /// <param name="securityid"></param>
        public void openPirceAndOrderView(string securityID)
        {
            string winkey = $"QuotesMiniWindows-{securityID}";
            Application.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, new Action(() =>
            {
                if (WindowManager.Instance.IsExist(winkey) == false)
                {
                    NoBorderWin myTradingPopupWin = new NoBorderWin(securityID, true);
                    myTradingPopupWin.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0A0F0E"));
                    myTradingPopupWin.Title = securityID;
                    PriceAndOrderView tradeBookView = new PriceAndOrderView(myTradingPopupWin);
                    myTradingPopupWin.SetMainControl(tradeBookView);
                    myTradingPopupWin.ResizeMode = ResizeMode.CanResize;
                    myTradingPopupWin.WindowStyle = WindowStyle.None;
                    myTradingPopupWin.WindowState = WindowState.Normal;
                    myTradingPopupWin.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                    myTradingPopupWin.Owner = null;
                    myTradingPopupWin.Width = 290;
                    myTradingPopupWin.Height = 485;
                    myTradingPopupWin.Topmost = true;
                    tradeBookView.Init(securityID, myTradingPopupWin.myWindow);
                    WindowManager.Instance.OpenWindow(winkey, myTradingPopupWin);
                }
            }));
        }

        /// <summary>
        /// 深度行情ViewModel 事件通知
        /// </summary>
        /// <param name="obj"></param>
        private void ViewModel_OnListChangeEvent(QuotationEventArgs obj)
        {
            string res = JsonConvert.SerializeObject(obj);
            var t = Task.Run(async () =>
            {
                await CallJsAsync(res);
            });
        }

        /// <summary>
        /// c# 回调 Js 方法 MG_CallBack
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private async Task CallJsAsync(string obj)
        {
            try
            {
                if (ChromeCtrl.Browser != null)
                {
                    JavascriptResponse x = await ChromeCtrl.Browser.GetBrowser().MainFrame.EvaluateScriptAsync($"MG_CallBack('{obj}')");
                    //是否执行错误
                    //string res = x.Result.ToString();
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex.Message);
            }
        }
    }
}
