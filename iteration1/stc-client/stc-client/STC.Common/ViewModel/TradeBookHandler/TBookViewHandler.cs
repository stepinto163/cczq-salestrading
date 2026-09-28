using Newtonsoft.Json;
using STC.Common.Command;
using STC.Common.Data;
using STC.Common.Model.Base;
using STC.Common.Model.Fix.Message;
using STC.Common.Utils;
using STC.Common.View.Card;
using STC.Common.View.UI;
using STC.Common.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace STC.Common.ViewModel.TradeBookHandler
{
    public class TBookViewHandler
    {
        private static TBookViewHandler _tbookViewHandler = new TBookViewHandler();
        public static TBookViewHandler Instance { get { return _tbookViewHandler; } }

        /// <summary>
        /// 构造函数
        /// </summary>
        private TBookViewHandler()
        {
            
        }


        /// <summary>
        /// 直接点击打开小窗，则自动置顶，如果是接受消息打开也自动置顶
        /// </summary>
        /// <param name="securityID"></param>
        public void OpenQuotesMiniWindows(string securityID)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                SubThreadOpenQuotesMiniWindows(securityID, true);
            }));
        }

        /// <summary>
        /// 默认打开从记忆中查找
        /// </summary>
        /// <param name="securityID"></param>
        /// <param name="isAlwaysTopmost"></param>
        private void SubThreadOpenQuotesMiniWindows(string securityID, bool isAlwaysTopmost = false)
        {
            if (STC.Common.Utils.CommonUtils.IsBDBAppName())
            {
                Common.Handler.Base.BaseHandler.Instance.PublishAppMsg(new PResult() { Code = "PopupBDBQuotesMiniWindows", Info1 = securityID });
            }
            else
            {
                string winkey = $"QuotesMiniWindows-{securityID}";
                if (WindowManager.Instance.IsExist(winkey) == false)
                {
                    NoBorderWin myTradingPopupWin = new NoBorderWin(securityID, isAlwaysTopmost);
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
                    if (isAlwaysTopmost)
                        myTradingPopupWin.Topmost = true;
                    tradeBookView.Init(securityID, myTradingPopupWin.myWindow, true);
                    WindowManager.Instance.OpenWindow(winkey, myTradingPopupWin);
                }
            }
        }
    }
}
