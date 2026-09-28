using Newtonsoft.Json;
using STC.Common.Data;
using STC.Common.Model.Base;
using STC.Common.Utils;
using STC.Common.View.Card;
using STC.Common.View.UI;
using STC.Common.ViewModel;
using STC.Frame.Common;
using STC.XBond.Plugin.Command;
using STC.XBond.Plugin.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace STC.XBond.Plugin.ViewModel.TradeBookHandler
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
            DesignHandler.Instance.OnTabChange += Instance_OnTabChange;
        }

        public void Init()
        {
            DesignHandler.Instance.OnSystemReady += Instance_OnSystemReady;
            Common.Handler.OpenWin.OpenWinHandler.Instance.SubscribePriceWin(Instance_PriceWinMsg);
            Common.Handler.OpenWin.OpenWinHandler.Instance.SubscribeClipboard(Instance_Clipboard);
            //订阅App消息
            Common.Handler.Base.BaseHandler.Instance.SubscribeAppMsg(OnAppMsg);

        }
        /// <summary>
        /// 接收应用程序消息
        /// </summary>
        /// <param name="msg"></param>
        private void OnAppMsg(object msg)
        {
            Common.Model.Fix.Message.PResult obj = (Common.Model.Fix.Message.PResult)msg;

            if (obj.Code == "PopupSTCQuotesMiniWindows" && !string.IsNullOrEmpty(obj.Info1))
            {
                try
                {
                    Instance_PriceWinMsg(obj.Info1.ToString());
                }
                catch { }
            }
        }

        private void Instance_OnSystemReady(object obj)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                //加载行情小窗
                string lstkeys = IniFileHelper.ReadValue($"QuotesMiniWindows-{LocalEnv.UserInfo.UserName}", "Keys");
                if (!string.IsNullOrEmpty(lstkeys))
                {
                    string[] keyList = lstkeys.Split(',');
                    for (int i = 0; i < keyList.Length; i++)
                    {
                        string securityID = keyList[i];
                        if (!string.IsNullOrEmpty(securityID))
                        {
                            SubThreadOpenQuotesMiniWindows(securityID);
                        }
                    }
                }
                

                //加载打开的窗口
                string lstkeysw= IniFileHelper.ReadValue($"STC.XBond.Plugin-{LocalEnv.UserInfo.UserName}", "Keys");
                string[] keyListw = lstkeysw.Split(',');
                for (int i = 0; i < keyListw.Length; i++)
                {
                    
                    string winid = keyListw[i];
                    if (!string.IsNullOrEmpty(winid))
                    {
                        string para = IniFileHelper.ReadValue($"STC.XBond.Plugin_{LocalEnv.UserInfo.UserName}_{winid}", "CommandParameter");
                        if (!string.IsNullOrEmpty(para))
                        {
                            if (JsonSplit.IsJson(para))
                            {
                                string[] o = JsonConvert.DeserializeObject<string[]>(para);
                                List<object> pobject = new List<object>();
                                for (int j = 0; j < o.Length; j++)
                                {
                                    if (j == 0)
                                    {
                                        PopupTypeEnum pye = (PopupTypeEnum)Enum.Parse(typeof(PopupTypeEnum), o[j]);
                                        pobject.Add(pye);
                                    }
                                    else
                                    {
                                        pobject.Add(o[j]);
                                    }
                                }

                                Utils.WindowKeyHelper.popupWindowCommand.Execute(pobject.ToArray());
                            }
                            else
                            {
                                PopupTypeEnum pye = (PopupTypeEnum)Enum.Parse(typeof(PopupTypeEnum), para);
                                Utils.WindowKeyHelper.popupWindowCommand.Execute(pye);
                            }
                        }
                    }
                }

            }));
        }


        // 收到ExcelPlugIn 消息
        private void Instance_PriceWinMsg(object obj)
        {
            if (obj != null)
            {
                string instr = obj.ToString();
                List<string> lstsecurityID = instr.Split(',').ToList();
                List<BondsInfoPo> LstBondInfoPo = new List<BondsInfoPo>();
                foreach (string securityID in lstsecurityID)
                {
                    BondsInfoPo bondsInfoPo = CommonUtils.GetBondsInfoPoBySecurityID(securityID);
                    if (bondsInfoPo != null)
                    {
                        LstBondInfoPo.Add(bondsInfoPo);
                        //OpenQuotesMiniWindows(securityID);
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
            }
        }

        /// <summary>
        /// 默认打开从记忆中查找
        /// </summary>
        /// <param name="securityID"></param>
        /// <param name="isAlwaysTopmost"></param>
        private void ArrangeOpenQuotesMiniWindows(List<BondsInfoPo> lstBondInfoPo)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                string securityID = string.Empty;
                bool isAlwaysTopmost = true;
                int count = lstBondInfoPo.Count;
                Point lastWindowPosition; // 记录上一个窗口的位置
                // 错位间距
                double offset = 40;
                // 获取屏幕尺寸
                double screenWidth = SystemParameters.PrimaryScreenWidth;
                double screenHeight = SystemParameters.PrimaryScreenHeight;

                // 计算窗口默认大小（假设窗口大小为 400x300）
                double windowWidth = 340;
                double windowHeight = 485;

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
                    string winkey = $"QuotesMiniWindows-{securityID}";
                    if (WindowManager.Instance.IsExist(winkey) == false)
                    {
                        NoBorderWin myTradingPopupWin = new NoBorderWin(securityID, isAlwaysTopmost, false);
                        myTradingPopupWin.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0A0F0E"));
                        myTradingPopupWin.Title = securityID;
                        PriceAndOrderView tradeBookView = new PriceAndOrderView(myTradingPopupWin);
                        myTradingPopupWin.SetMainControl(tradeBookView);
                        myTradingPopupWin.ResizeMode = ResizeMode.CanResize;
                        myTradingPopupWin.WindowStyle = WindowStyle.None;
                        myTradingPopupWin.WindowState = WindowState.Normal;
                        myTradingPopupWin.WindowStartupLocation = WindowStartupLocation.Manual;
                        myTradingPopupWin.Owner = null;
                        myTradingPopupWin.Width = 340;
                        myTradingPopupWin.Height = 485;

                        myTradingPopupWin.Left = lastWindowPosition.X;
                        myTradingPopupWin.Top = lastWindowPosition.Y;

                        // 计算下一个窗口的位置
                        lastWindowPosition.X += offset;
                        lastWindowPosition.Y += offset;

                        if (isAlwaysTopmost)
                            myTradingPopupWin.Topmost = true;

                        tradeBookView.Init(securityID, myTradingPopupWin.myWindow, true);
                        WindowManager.Instance.OpenWindow(winkey, myTradingPopupWin);
                    }
                }
            }));
        }


        private void Instance_Clipboard(object obj)
        {
            try
            {
                ClipboardHelper.CopyToClipboard(obj);
            }
            catch (Exception ex)
            {
            }
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
                myTradingPopupWin.Width = 340;
                myTradingPopupWin.Height = 485;
                if (isAlwaysTopmost)
                    myTradingPopupWin.Topmost = true;
                tradeBookView.Init(securityID, myTradingPopupWin.myWindow,true);
                WindowManager.Instance.OpenWindow(winkey, myTradingPopupWin);
            }
        }

        private void Instance_OnTabChange(object obj)
        {
            if (obj != null)
            {
                TabItem tabItem = obj as TabItem;
                if (tabItem != null)
                {
                    int pluginKey = (int)tabItem.Tag;
                    if (pluginKey == (int)PluginKeyEnum.TradeBookPlugin)
                    {
                        string securityId = tabItem.ToolTip.ConvertToString();

                        GetTradeBookView(securityId, true);
                    }
                }
            }
            // 系统备妥
            // DesignHandler.Instance.SystemIsReady(obj);
        }

        private TradeBookView view = new TradeBookView();
        private QuotationListViewModel viewModel = new QuotationListViewModel();


        public Task<TradeBookView> GetTradeBookView(object pluginData, bool needInit)
        {
            return Task.Run(() =>
            {
                if (needInit)
                {
                    Application.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, new Action(() =>
                    {
                        if (pluginData != null)
                        {
                            viewModel.SecurityID = pluginData as string;
                            //view.UpdateeLastExectu(viewModel.SecurityID);
                            view.ChangeVisibility();
                            BondsInfoPo bondsInfoPo = CommonUtils.GetBondsInfoPoBySecurityID(viewModel.SecurityID);
                            if (bondsInfoPo != null)
                            {
                                //viewModel.SecurityID = bondsInfoPo.Bond_ID;
                                viewModel.Symbol = bondsInfoPo.Short_Name;
                                viewModel.FullName = bondsInfoPo.Short_Name;
                                viewModel.MarketIndicator = bondsInfoPo.MarketIndicator;

                                view.QList.SetQtyHeaderText(bondsInfoPo.MarketIndicator);
                                view.LastExecutions.UpdatePriceType(viewModel.PriceTypeIndex);
                                //view.LastExecutions.SecurtityID = string.IsNullOrEmpty(bondsInfoPo.Bond_ID) ? "xxxxx" : bondsInfoPo.Bond_ID;
                                view.LastExecutions.SetSecurtityID(bondsInfoPo.Bond_ID);

                                if (!string.IsNullOrEmpty(bondsInfoPo.Option_Type) &&
                                    ((!bondsInfoPo.Option_Type.Equals("NON") && bondsInfoPo.Option_Date != null) ||
                                    (bondsInfoPo.Option_Type.Equals("ETS") && bondsInfoPo.Option_Date == null)))
                                {
                                    viewModel.ShowExeRadioVisible = Visibility.Visible;
                                }
                                else
                                {
                                    viewModel.ShowExeRadioVisible = Visibility.Collapsed;
                                }
                            }
                            else
                            {
                                view.LastExecutions.SetSecurtityID(viewModel.SecurityID);
                                viewModel.ShowExeRadioVisible = Visibility.Collapsed;
                            }
                        }
                        else
                        {
                            view.UpdateeLastExectu(string.Empty);
                        }

                        if (view.DataContext == null)
                            view.DataContext = viewModel;
                    }));
                }

                //国债期货量单位为手
                if (CommonUtils.IsFutureByMarketIndicator(viewModel.MarketIndicator))
                {
                    viewModel.HeaderAmount = "量(手)";
                    viewModel.ShowSum = Visibility.Collapsed;
                }
                else
                {
                    //默认为 万
                    viewModel.HeaderAmount = "量(万)";
                    viewModel.ShowSum = Visibility.Visible;
                }

                return view;
            });


        }

    }
}
