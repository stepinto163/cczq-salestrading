using Ats.Foundation.Message;
using com.gateway.log;
using STC.Common.Handler.Admin;
using STC.Common.Handler.Inner;
using STC.Common.Handler.SystemParam;
using STC.Common.Model.Admin;
using STC.Common.Model.Base;
using STC.Common.Model.Drp;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.TBook;
using STC.Common.Utils;
using STC.Common.View.Drp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using static STC.Common.Model.Fix.MessageDictionary.FieldsEnum;

namespace STC.Common.ViewModel.Drp.Utils
{
    public static class DRPTools
    {
        #region Constants

        const UInt32 SWP_NOSIZE = 0x0001;
        const UInt32 SWP_NOMOVE = 0x0002;
        const UInt32 SWP_SHOWWINDOW = 0x0040;

        /// <summary>
        /// 渠道权限
        /// </summary>
        private static readonly string MODULEID = "1002";
        /// <summary>
        /// 所有权限
        /// </summary>
        private static readonly string CHECK_ALL_ACTIONID = "100203";
        /// <summary>
        /// 仅内部
        /// </summary>
        private static readonly string CHECK_INNER_ACTIONID = "100201";
        /// <summary>
        /// 内部，X-Bond
        /// </summary>
        private static readonly string CHECK_INNER_XBOND_ACTIONID = "100202";

        #endregion

        #region API
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, IntPtr ProcessId);

        [DllImport("user32.dll")]
        private static extern bool AttachThreadInput(uint idAttach, uint idAttachTo, bool fAttach);

        [DllImport("user32.dll")]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);
    
        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        private const int SW_SHOWNORMAL = 1;
        #endregion
        /// <summary>
        /// Activate a window from anywhere by attaching to the foreground window
        /// </summary>
        public static void GlobalActivate(Window w)
        {
            //Get the process ID for this window's thread
            var interopHelper = new WindowInteropHelper(w);
            var thisWindowThreadId = GetWindowThreadProcessId(interopHelper.Handle, IntPtr.Zero);

            //Get the process ID for the foreground window's thread
            var currentForegroundWindow = GetForegroundWindow();
            var currentForegroundWindowThreadId = GetWindowThreadProcessId(currentForegroundWindow, IntPtr.Zero);

            //Attach this window's thread to the current window's thread
            AttachThreadInput(currentForegroundWindowThreadId, thisWindowThreadId, true);

            //Set the window position
            //SetWindowPos(interopHelper.Handle, new IntPtr(0), 0, 0, 0, 0, SWP_NOSIZE | SWP_NOMOVE | SWP_SHOWWINDOW);

            //Detach this window's thread from the current window's thread
            AttachThreadInput(currentForegroundWindowThreadId, thisWindowThreadId, false);

            //Show and activate the window
            //if (w.WindowState == WindowState.Minimized) w.WindowState = WindowState.Normal;
            w.Show();
            w.Activate();
        }

        /// <summary>
        /// Restore window where Minimized
        /// </summary>
        /// <param name="mainWindow"></param>
        public static void RestoreWindowWhenMinimized(Window mainWindow)
        {
            if (mainWindow != null)
            {
                var handle = new System.Windows.Interop.WindowInteropHelper(mainWindow).Handle;

                if (System.Windows.Application.Current.MainWindow.WindowState == System.Windows.WindowState.Minimized)
                {
                    ShowWindow(handle, SW_SHOWNORMAL); // 还原窗口
                    SetForegroundWindow(handle); // 让窗口到前台
                }
            }
        }

        /// <summary>
        /// DRP 回价总窗口数据
        /// </summary>
        /// <param name="pv"></param>
        /// <returns></returns>
        public static DRPListModel PriceValueToGridData(DRPValue pv)
        {
            DRPListModel result = new DRPListModel();
            result.FirmType = pv.firmFlag == "0" ? "firm" : "nonfirm";
            result.FirmFlag = pv.firmFlag;
            result.Id = pv.orderId;
            result.QuoteID = pv.quoteId;
            result.OrderId = pv.orderId;

            result.Source = GenerateSourceByVenues(pv.venue, pv.partysubname);
            result.NetPrice = pv.netPrice.ToString("N4");

            string showName = ComboSourceUtil.Instance.FindEnum(pv.venue, "venues");

            if (!string.IsNullOrEmpty(showName))
                result.Source = showName;

            result.SettlType = pv.settlType;
            if (pv.settlType != null)
                result.SettlType_yuan = pv.settlType.Contains("+") ? pv.settlType : "";

            result.Settle = ParseSettle(pv.settlType);
            result.DealSettle = ParseSettle(pv.settlType);
            result.OrderStatus = pv.orderStatus;
            result.QuoteType = pv.quotetype;
            result.SecurityType = pv.symbolType;
            result.MarketIndicator = pv.marketIndicator;
            result.Symbol = pv.securityid;
            result.SymbolType = pv.symbolType;
            result.IsIRS = pv.marketIndicator.ToString().Trim() == "2" ? true : false;
            //1 BID BUY   2 Ofr SELL
            result.Side = (pv.side.ToUpper() == "BUY" || pv.side.ToUpper() == "1") ? "1" : "2";
            result.PriceBrush = (pv.side.ToUpper() == "BUY" || pv.side.ToUpper() == "1") ? new SolidColorBrush(Color.FromRgb(0xFB, 0x94, 0x3C)) : new SolidColorBrush(Color.FromRgb(0x00, 0x9A, 0xFF));

            result.TimeInForce = pv.timeInForce.ToUpper() == "DAY" ? "1" : "3";

            result.WholeFlag = pv.wholeflag;
            result.VenueTypeGW = pv.venueGW;
            result.Venue = pv.venue;

            // -1 , 999 代表无价
            if (pv.price.ToString() == "-1" || pv.price.ToString() == "999")
            {
                result.Price = "--";
            }
            else
            {
                result.Price = CommonUtils.ConvertToDecimal(pv.price.ToString()).ToString("F4");
            }

            result.Amount = ((int)pv.qty).ToString();
            if (pv.dqOrderInfo2 != null)
            {
                //1 - 议价，2 - 成交，3 - 拒绝  4-撤销  5 - 报价方撤单
                result.PointPrice = pv.dqOrderInfo2.bargainYield;
                result.MsgCode = pv.dqOrderInfo2.msgCode;
            }
            else
            {
                result.PointPrice = result.Price;
            }
            if (pv.orderStatus == "Partially_Filled")
            {
                result.PointAmount = pv.cumQty.ToString("0");
            }
            else
            {
                if (pv.dqOrderInfo2 != null)
                {
                    if ((pv.dqOrderInfo2.actionType == "1") || (pv.dqOrderInfo2.actionType == "3"))
                    {
                        result.PointAmount = pv.dqOrderInfo2.bargainQty;
                    }
                    else
                    {
                        result.PointAmount = result.Amount;
                    }
                }
                else
                {
                    result.PointAmount = result.Amount;
                }
            }

            switch (result.MsgCode)
            {
                case "11016": //回价成交
                    result.CMsgCode = pv.orderStatus == "Partially_Filled" ? "部分成交" : "全部成交";
                    break;
                case "11011": //议价拒绝
                    string price = result.PointPrice.ToString() == "--" ? "--" : CommonUtils.FormatNum(result.PointPrice.ToString());
                    int tmpamount = CommonUtils.ConvertToInt(result.PointAmount);
                    string amount = $"{(tmpamount / 1000).ToString()}K";
                    //result.CMsgCode = $"价格讨论({price} {amount})";
                    result.CMsgCode = $"价格讨论";
                    break;
                case "10010":
                case "11012":
                case "11013":
                case "11015":
                    result.CMsgCode = ComboSourceUtil.Instance.FindEnum(result.MsgCode, "dqmsgcode");
                    break;
                default:
                    result.CMsgCode = "";
                    break;
            }

            result.ReplyTooltip = "快速成交";
            result.TransactTime = pv.transactTime;



            result.IocTimeOut = pv.iocTimeOut;

            ////if (!string.IsNullOrEmpty(pv.iocTimeOut))
            ////{
            ////    int icotimeout = 0;
            ////    int.TryParse(pv.iocTimeOut.ToString(), out icotimeout);
            ////    if (string.IsNullOrEmpty(result.ValidUntilTime))
            ////        result.ValidUntilTime = DateTimeEx.GetNow().AddSeconds(icotimeout).ToString("yyyy-MM-dd HH:mm:ss.fff");
            ////}
            ///
            result.ValidUntilTime = pv.ValidUntilTime;
            result.ClOrdID = pv.clOrderId;
            result.SecurityID = pv.securityid;

            result.QuoteType = pv.quotetype;
            result.UserName = pv.userName;
            result.UserID = pv.userID;
            result.Name = pv.name;

            if (pv.orderStatus == "New")
            {
                result.EnabledBtn = true;
                result.RemainSeconds = "-1";
            }
            else
            {
                result.EnabledBtn = false;
                result.IocTimeOut = null;
                result.RemainSeconds = "--";
                result.IsChecked = false;
            }

            //历史数据有问题，报价变成了价格讨论
            //if (!string.IsNullOrEmpty(result.MsgCode))
            //{
            //    if (result.MsgCode == "11011")
            //    {
            //        result.EnabledBtn = true;
            //        result.RemainSeconds = "-1";
            //    }
            //}

            if (!string.IsNullOrEmpty(pv.wholeflag) && pv.wholeflag.Equals("1"))
            {
                result.WholeVisibility = Visibility.Visible;
                result.EnabledEditPointAmountBtn = false;
            }
            else
            {
                result.EnabledEditPointAmountBtn = result.EnabledBtn ? true : false;
            }

            return result;
        }

        /// <summary>
        /// content 转 ViewModel
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public static DRPNotificationViewModel DrpContentToViewModel(NotificationContent content)
        {
            DRPNotificationViewModel result = new DRPNotificationViewModel();
            result.Title = content.Title;
            result.Message = content.Message;
            result.Id = content.Id;
            result.IsShow = content.IsShow;
            result.ContentType = content.ContentType;
            result.DelFlag = content.DelFlag;
            result.Detail = content.Detail;

            if (content.Detail != null)
            {
                result.DrpDetail = Newtonsoft.Json.JsonConvert.DeserializeObject<DRPDetail>(content.Detail);
                result.SecurityID = result.DrpDetail.order.symbol;
                result.OrderId = result.DrpDetail.iocOrder.orderId;
                result.QuoteID = result.DrpDetail.order.orderId;
                if (result.DrpDetail.order.symbolType.ToUpper() == "BCO")
                {
                    result.Symbol = result.DrpDetail.order.securityid;
                }
                else
                {
                    result.Symbol = result.DrpDetail.order.symbol;
                }

                bool isirs = CommonUtils.IsIRS(result.DrpDetail.order.symbol);
                bool isshe = CommonUtils.IsSheBond(result.DrpDetail.order.symbol);
                result.IsShowIRS = isirs ? Visibility.Collapsed : Visibility.Visible;
                result.IsShowSHE = isshe;
                result.Side = result.DrpDetail.order.side;
                if (result.DrpDetail.order.price == -1 || result.DrpDetail.order.price == 999)
                {
                    result.Price = "--";
                }
                else
                {
                    result.Price = CommonUtils.ConvertToDecimal(result.DrpDetail.order.price.ToString()).ToString("F4");
                }
                result.Amount = result.DrpDetail.order.qty.ToString() == "0" ? "--" : result.DrpDetail.order.qty.ToString();

                result.LeavesQtyTotal = result.DrpDetail.order.qty - result.DrpDetail.order.cumQty;
                result.ShowLeavesQtyTotal = result.LeavesQtyTotal.ToString("0") == "0" ? "--" : result.LeavesQtyTotal.ToString("0");

                result.Settle = result.DrpDetail.order.settlType;

                
                Dictionary<string, string> DictInSettle = new Dictionary<string, string>();
                if (result.Settle != null)
                {
                    List<string> listSettle = result.Settle.Split(':').ToList();
                    foreach (string x in listSettle)
                    {
                        if (!DictInSettle.ContainsKey(x))
                        {
                            DictInSettle.Add(x, x);
                        }
                    }
                    if (DictInSettle.Count > 0)
                    {
                        //判断是否有+1
                        if (DictInSettle.ContainsKey("2") || DictInSettle.ContainsKey("3") || DictInSettle.ContainsKey("2,3"))
                        {
                            result.ShowSettle = "+1";
                        }

                        if (DictInSettle.ContainsKey("1") && listSettle.Count == 1)
                        {
                            result.ShowSettle = "+0";
                        }

                        if (result.Settle.Contains("+"))
                        {
                            result.ShowSettle = "远";
                        }
                    }
                }
               
                //result.ShowSettle = ComboSourceUtil.Instance.FindEnum(result.Settle, "settletype_drp");

                result.ShowSourceSettle = result.ShowSettle;
                

                result.SettlDate = result.DrpDetail.order.settlDate;
                result.WholeFlag =  result.DrpDetail.order.wholeflag ?? "0";


                //1 BID BUY   2 Ofr SELL
                result.ShowSide = (result.DrpDetail.order.side.ToUpper().Equals("BUY") || result.DrpDetail.order.side.Equals("1")) ? "1" : "2";
                result.ShowPrice = result.Price.ToString() == "--"  ? "--" : CommonUtils.FormatNum(result.Price.ToString());

                if (result.DrpDetail.iocOrder.dqOrderInfo2 != null)
                {
                    result.ActionType = result.DrpDetail.iocOrder.dqOrderInfo2.actionType;
                    //result.OrderId = result.DrpDetail.iocOrder.dqOrderInfo2.orderId;
                }

                // 设置排序, IRS是反向的
                if (!isirs)
                    result.SetSortDescriptions(result.ShowSide);
                else
                    result.SetSortDescriptions(result.ShowSide == "1" ? "2" : "1");

                // 计算活着的笔数
                List<DRPValue> Tmp = new List<DRPValue>(result.DrpDetail.iocOrderDqList.Where(p => p.orderStatus == "New"));
                result.IocCount = Tmp.Count.ToString();

                foreach (DRPValue pv in result.DrpDetail.iocOrderDqList)
                {
                    DRPListModel dg = DRPTools.PriceValueToGridData(pv);
                    dg.IsIRS = isirs;
                    dg.TransactTime = pv.transactTime;
                    dg.Symbol = pv.symbol;

                    if (dg.OrderStatus == "New" || dg.OrderStatus == "Partially_Filled" || dg.OrderStatus == "Filled")
                    {
                        dg.ForegroundBrush = new SolidColorBrush(Color.FromRgb(0xD7, 0xD7, 0xD7)); ;  //#D7D7D7
                    }
                    else
                    {
                        dg.ForegroundBrush = new SolidColorBrush(Color.FromRgb(0xA0, 0xA0, 0xA0)); ;  //#A0A0A0
                    }

                    dg.OriAmount = (result.DrpDetail.order.qty - result.DrpDetail.order.cumQty).ToString();
                    dg.OriPrice = CommonUtils.ConvertToDecimal(result.DrpDetail.order.price.ToString()).ToString("F4");
                    dg.OriSettle = result.DrpDetail.order.settlType;
                    dg.OriSide = result.ShowSide;


                    DRPListModel rf = result.GridDatas.FirstOrDefault(o => o.Id == dg.Id);
                    if (rf != null)
                    {
                        rf.Amount = dg.Amount;
                        rf.FirmType = dg.FirmType;
                        rf.Id = dg.Id;
                        rf.Price = dg.Price;
                        rf.Source = dg.Source;
                        rf.PriceBrush = dg.PriceBrush;
                        rf.EnabledBtn = dg.EnabledBtn;
                        rf.TransactTime = dg.TransactTime;
                        rf.PointPriceBrush = dg.PointPriceBrush;
                        rf.OrderStatus = dg.OrderStatus;
                        //因为是全量推送，如果已经记录，则不更新倒计时
                        //rf.RemainSeconds = dg.RemainSeconds;
                        rf.ValidUntilTime = dg.ValidUntilTime;
                        rf.PointAmount = dg.PointAmount;
                        rf.PointPrice = dg.PointPrice;
                        rf.Side = dg.Side;
                        rf.Settle = dg.Settle;
                        rf.MsgCode = dg.MsgCode;
                        rf.CMsgCode = dg.CMsgCode;
                        rf.OriAmount = (result.DrpDetail.order.qty - result.DrpDetail.order.cumQty).ToString();
                        rf.OriPrice = CommonUtils.ConvertToDecimal(result.DrpDetail.order.price.ToString()).ToString("F4");
                        rf.OriSettle = result.DrpDetail.order.settlType;
                        rf.OriSide = result.ShowSide;
                        rf.IsIRS = isirs;
                    }
                    else
                    {
                        result.GridDatas.Add(dg);
                    }
                }
            }

            return result;
        }

        public static void UpdateGridData(ObservableCollection<DRPListModel> gridDatas, DRPListModel dg)
        {
            DRPListModel rf = gridDatas.FirstOrDefault(o => o.Id == dg.Id);
            if (rf != null)
            {
                rf.Amount = dg.Amount;
                rf.FirmType = dg.FirmType;
                rf.Id = dg.Id;
                rf.Price = dg.Price;
                rf.Source = dg.Source;
                rf.PriceBrush = dg.PriceBrush;
                rf.EnabledBtn = dg.EnabledBtn;
                rf.TransactTime = dg.TransactTime;
                rf.PointPriceBrush = dg.PointPriceBrush;
                rf.OrderStatus = dg.OrderStatus;
                //因为是全量推送，如果已经记录，则不更新倒计时
                //rf.RemainSeconds = dg.RemainSeconds;
                rf.ValidUntilTime = dg.ValidUntilTime;
                rf.PointAmount = dg.PointAmount;
                rf.PointPrice = dg.PointPrice;
                rf.Side = dg.Side;
                rf.MsgCode = dg.MsgCode;
                rf.CMsgCode = dg.CMsgCode;
                rf.OriAmount = dg.OriAmount;
                rf.OriPrice = dg.OriPrice;
                rf.OriSettle = dg.OriSettle;
                rf.IsIRS = dg.IsIRS;
            }
            else
            {
                gridDatas.Add(dg);
            }
        }

        /// <summary>
        /// 定向议价
        /// </summary>
        /// <param name="gridData"></param>
        /// <returns></returns>
        public static Model.Fix.Message.OrderCancelRequest GetCancelOrder(DRPListModel gridData)
        {
            Model.Fix.Message.OrderCancelRequest newOrder = new Model.Fix.Message.OrderCancelRequest();
            // Add TradType
            string tradeType = "1";
            if (!string.IsNullOrEmpty(LocalEnv.ConfInfo.TradeType))
                tradeType = LocalEnv.ConfInfo.TradeType;

            newOrder.AlgoName = "DQ";//算法名称（DQ-定向回价、STP-直通、CROSS-撮合）
            newOrder.VenueTypeGW = gridData.VenueTypeGW;
            newOrder.Venues = gridData.Venue;
            newOrder.QuoteID = gridData.QuoteID;
            newOrder.Symbol = gridData.Symbol;
            newOrder.OrderID = gridData.OrderId;
            newOrder.Side = gridData.Side;
            newOrder.Price = gridData.PointPrice;
            newOrder.NetPrice = gridData.NetPrice;

            newOrder.OrderQty = gridData.PointAmount;
            newOrder.MarketIndicator = gridData.MarketIndicator;
            newOrder.SecurityID = gridData.SecurityID;
            newOrder.QuoteType = null;
            newOrder.UserName = LocalEnv.UserInfo.UserName;
            newOrder.DqUpdate = gridData.ChangeOrderPrice ? "1" : "0";
            return newOrder;
        }

        /// <summary>
        /// 拒绝
        /// </summary>
        /// <param name="gridData"></param>
        /// <returns></returns>
        public static Model.Fix.Message.OrderCancelRequest GetRejectOrder(DRPListModel gridData)
        {
            Model.Fix.Message.OrderCancelRequest newOrder = new Model.Fix.Message.OrderCancelRequest();
            // Add TradType
            string tradeType = "1";
            if (!string.IsNullOrEmpty(LocalEnv.ConfInfo.TradeType))
                tradeType = LocalEnv.ConfInfo.TradeType;

            newOrder.AlgoName = "DQ";//算法名称（DQ-定向回价、STP-直通、CROSS-撮合）
            newOrder.VenueTypeGW = gridData.VenueTypeGW;
            newOrder.Venues = gridData.Venue;
            newOrder.QuoteID = gridData.QuoteID;
            newOrder.Symbol = gridData.Symbol;
            newOrder.OrderID = gridData.OrderId;
            newOrder.Side = gridData.Side;
            newOrder.Price = gridData.Price;
            newOrder.NetPrice = gridData.NetPrice;
            newOrder.OrderQty = gridData.Amount;
            newOrder.MarketIndicator = gridData.MarketIndicator;
            newOrder.SecurityID = gridData.SecurityID;
            newOrder.QuoteType = null;
            newOrder.UserName = LocalEnv.UserInfo.UserName;
            newOrder.DqUpdate = "0";
            return newOrder;
        }

        /// <summary>
        /// 定向成交
        /// </summary>
        /// <param name="gridData"></param>
        /// <returns></returns>
        public static NewOrderSingle GetNewOrderSingle(DRPListModel gridData)
        {
            NewOrderSingle newOrder = new NewOrderSingle();
            // Add TradType
            string tradeType = "1";
            if (!string.IsNullOrEmpty(LocalEnv.ConfInfo.TradeType))
                tradeType = LocalEnv.ConfInfo.TradeType;

            newOrder.AlgoName = "STP";//算法名称（DQ-定向回价、STP-直通、CROSS-撮合）
            newOrder.ClOrdID = DataTimeSequence.Instance.GetId();
            newOrder.VenueTypeGW = gridData.VenueTypeGW;
            newOrder.Venues = gridData.Venue;

            newOrder.SecurityID = gridData.SecurityID;
            newOrder.Symbol = gridData.Symbol;
            newOrder.SecurityType = gridData.SecurityType;

            newOrder.QuoteID = gridData.OrderId;
            newOrder.Side = gridData.Side == "1" ? "2" : "1";
            newOrder.Price = gridData.PointPrice;
            newOrder.PriceType = "3"; //收益率
            newOrder.Yield = gridData.PointPrice;
            newOrder.NetPrice = gridData.NetPrice;
            newOrder.OrderQty = gridData.PointAmount;
            newOrder.MarketIndicator = gridData.MarketIndicator;

            newOrder.QuoteType = gridData.QuoteType;
            newOrder.UserName = LocalEnv.UserInfo.UserName;
            newOrder.Terminal = string.Format("STClient_v{0}", LocalEnv.ServerInfo.Version);
            newOrder.FirmFlag = gridData.FirmFlag;
            newOrder.WholeFlag = gridData.WholeFlag;
            newOrder.IocTimeOut = gridData.IocTimeOut;

            newOrder.TimeInForce = gridData.TimeInForce;
            newOrder.SettlType = ComboSourceUtil.Instance.FindEnum(gridData.Settle, "settletype_revert");

            newOrder.PendingCancelTime = "15";
            newOrder.MacID = EngineHelper.GetMacByNetworkInterface();
            newOrder.InsideIP = EngineHelper.GetIp();
            newOrder.HardDiskID = EngineHelper.Getharddiskid2();


            newOrder.DeliveryType = gridData.DeliveryType;
            newOrder.ClearingMethod = gridData.ClearingMethod;
            newOrder.OrdType = "2";

            return newOrder;
        }


        public static Model.Fix.Message.OrderCancelRequest GetCancelOrder(DataRowView drv)
        {
            Model.Fix.Message.OrderCancelRequest newOrder = new Model.Fix.Message.OrderCancelRequest();
            // Add TradType
            string tradeType = "1";
            if (!string.IsNullOrEmpty(LocalEnv.ConfInfo.TradeType))
                tradeType = LocalEnv.ConfInfo.TradeType;

            newOrder.AlgoName = "DQ";//算法名称（DQ-定向回价、STP-直通、CROSS-撮合）
            newOrder.VenueTypeGW = drv["VenueTypeGW"].ToString();
            newOrder.Venues = drv["Veneues"].ToString();
            newOrder.QuoteID = drv["QuoteID"].ToString();
            newOrder.Symbol = drv["Symbol"].ToString();
            newOrder.OrderID = drv["STCCode"].ToString();
            newOrder.Side = drv["Side"].ToString();
            newOrder.Price = drv["Price"].ToString();
            newOrder.NetPrice = drv["NetPrice"].ToString();

            newOrder.OrderQty = drv["Qty"].ToString();
            newOrder.MarketIndicator = drv["MarketIndicator"].ToString();
            newOrder.SecurityID = drv["SecurityID"].ToString();
            newOrder.QuoteType = drv["OrdType"].ToString();
            newOrder.UserName = LocalEnv.UserInfo.UserName;

            return newOrder;
        }

        public static async Task<STC.Common.Model.Fix.Message.PResult> CancelOrder(Model.Fix.Message.OrderCancelRequest cancelPo)
        {
            STC.Common.Model.Fix.Message.PResult reply = null;
            await Task.Run(() =>
            {
                reply = InnerHandler.Instance.DRPCancelOrder(cancelPo);
            });
            return reply;
        }

        /// <summary>
        /// content 转 被点价ViewModel
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public static NofirmPointPriceViewModel NotificationToNofirmPointPrice(NotificationContent content)
        {
            NofirmPointPriceViewModel result = new NofirmPointPriceViewModel();
            PointPriceDetailModel ppdm = null;

            result.Id = content.Id;
            result.ContentType = content.ContentType;
            result.DelFlag = content.DelFlag;
            result.Detail = content.Detail;
            result.Message = content.Message;

            if (content.Detail != null)
            {
                ppdm = Newtonsoft.Json.JsonConvert.DeserializeObject<PointPriceDetailModel>(content.Detail);
                result.DrpDetail = Newtonsoft.Json.JsonConvert.DeserializeObject<PointPriceDetailModel>(content.Detail);

                result.SecurityID = result.DrpDetail.order.securityid;
                result.Symbol = result.DrpDetail.order.symbol;
                if (result.DrpDetail.order.symbolType.ToUpper() == "BCO")
                {
                    result.Symbol = result.DrpDetail.order.securityid;
                }

                bool isirs = CommonUtils.IsIRS(result.DrpDetail.order.symbol);
                bool isshe = CommonUtils.IsSheBond(result.DrpDetail.order.symbol);
                result.IsShowIRS = isirs;
                result.IsShowSHE = isshe;
                result.UserName = result.DrpDetail.iocOrder.name;
                result.Settle = result.DrpDetail.order.settlType;
                result.ShowSettle = ComboSourceUtil.Instance.FindEnum(result.DrpDetail.order.settlType, "settletype_myorder");
                result.WholeFlag = result.DrpDetail.order.wholeflag == null ? "0" : result.DrpDetail.order.wholeflag;
                result.Side = (result.DrpDetail.iocOrder.side.ToUpper().Equals("BUY") || result.DrpDetail.iocOrder.side.Equals("1")) ? "1" : "2";
                result.IocTimeOut = result.DrpDetail.iocOrder.iocTimeOut;
                result.Price = CommonUtils.ConvertToDecimal(result.DrpDetail.order.price.ToString()).ToString("F4");
                

                result.SourcePointAmount = result.DrpDetail.iocOrder.qty;
                if (isirs)
                {
                    result.PointAmount = result.DrpDetail.iocOrder.qty.ToString() == "0" ? "--" : (result.DrpDetail.iocOrder.qty / 100).ToString();
                }
                else
                {
                    if (!isshe)
                        result.PointAmount = result.DrpDetail.iocOrder.qty.ToString() == "0" ? "--" : (result.DrpDetail.iocOrder.qty / 1000).ToString();
                    else
                        result.PointAmount = result.DrpDetail.iocOrder.qty.ToString() == "0" ? "--" : (result.DrpDetail.iocOrder.qty / 1).ToString();
                }

                result.Amount = result.DrpDetail.order.qty.ToString() == "0" ? "--" : result.DrpDetail.order.qty.ToString();

                result.LeavesQtyTotal = result.DrpDetail.order.qty - result.DrpDetail.order.cumQty;
                result.ShowLeavesQtyTotal = result.LeavesQtyTotal.ToString() == "0" ? "--" : result.LeavesQtyTotal.ToString();

                result.OrderId = result.DrpDetail.iocOrder.orderId;
                //1 BID BUY   2 Ofr SELL
                result.ShowSide = (result.DrpDetail.order.side.ToUpper().Equals("BUY") || result.DrpDetail.order.side.Equals("1")) ? "1" : "2";
                result.ShowPrice = CommonUtils.FormatNum(result.DrpDetail.order.price.ToString());

                result.PointPrice = CommonUtils.FormatNum(result.DrpDetail.iocOrder.price.ToString());

                result.TransactTime = result.DrpDetail.iocOrder.transactTime;
                //改为当收到回应消息后，以本地时间计算
                result.OrderValidUntilTime = result.DrpDetail.iocOrder.validuntitime;
            }

            return result;
        }



        /// <summary>
        /// 消息转换
        /// </summary>
        /// <param name="sysMsg"></param>
        /// <param name="uc"></param>
        /// <returns></returns>
        public static MsgPo SystemMsgToShowMsg(TTbookSystemMessage sysMsg, FrameworkElement uc)
        {
            MsgPo result = null;
            try
            {
                if (string.IsNullOrEmpty(sysMsg.content))
                {
                    return result;
                }

                if (!JsonSplit.IsJson(sysMsg.content))
                {
                    return result;
                }

                TMessageContent detail = MsgHelper.Deserialize<TMessageContent>(sysMsg.content, Ats.Foundation.Message.Serializer.MsgSerializerType.Json);

                if (sysMsg.type.Equals("2"))
                {
                    try
                    {
                        if (!string.IsNullOrEmpty(detail.detail))
                        {
                            ////PriceMatchDetail priceMatchDetail = MsgHelper.Deserialize<PriceMatchDetail>(detail.detail);
                            ////if ((priceMatchDetail == null || priceMatchDetail.Prices == null) && detail.contentType.Equals(AdminConsts.PriceMatch))
                            ////    return result;
                            ////else if (priceMatchDetail != null && priceMatchDetail.Prices != null
                            ////    && (priceMatchDetail.Prices == null || priceMatchDetail.Prices.Count == 0)
                            ////    && (!string.IsNullOrEmpty(detail.contentType) && detail.contentType.Equals(AdminConsts.PriceMatch)))
                            ////{
                            ////    sysMsg.delflag = "1";//前台主动过滤匹配0条报价的消息
                            ////}
                        }
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Error(ex);
                    }
                }

                result = new MsgPo();
                result.username = sysMsg.username;
                result.MessageContent = detail;
                result.Level = detail.type;
                result.Id = sysMsg.id;//sysMsg.id;
                result.HeaderTitle = !string.IsNullOrEmpty(detail.title) ? string.Format("[{0}]", detail.title) : string.Empty;
                result.ContentType = detail.contentType;
                result.counterpartyid = detail.counterpartyid;
                result.counterpartyname = detail.counterpartyname;
                result.counterpartysubid = detail.counterpartysubid;
                result.counterpartysubname = detail.counterpartysubname;
                result.readFlag = sysMsg.readflag;

                result.Venue = detail.venue;
                result.Content = detail.content;
                result.IsShow = detail.show ?? true;

                //如果消息提醒不是RFQ则走原来的流程，修改CloseBy   result.ContentType.Equals("DQB") || result.ContentType.Equals("DQ")
                if (sysMsg.closeby != "RFQSvr" && sysMsg.closeby != "RFQMSvr")
                {
                    if (!string.IsNullOrEmpty(detail.counterpartyname) && !string.IsNullOrEmpty(detail.counterpartysubname))
                        result.CloseBy = string.Format("{0}({1})", detail.counterpartyname, detail.counterpartysubname);
                    else if (string.IsNullOrEmpty(detail.counterpartyname) && string.IsNullOrEmpty(detail.counterpartysubname))
                        result.CloseBy = string.Empty;
                    else
                        result.CloseBy = string.IsNullOrEmpty(detail.counterpartyname) ? detail.counterpartysubname : detail.counterpartyname;
                }

                result.ValueTime = detail.validtime;//"有效时间 3分20秒";
                if (!string.IsNullOrEmpty(sysMsg.createtime))
                    result.UpdateTime = sysMsg.createtime.Substring(11, 8); //"10:32:21";
                result.createtime = sysMsg.createtime;

                if (result.Level != null)
                {
                    if (result.Level.Equals(AdminConsts.Admin_Critical))
                    {
                        result.Level = "1";

                        {
                            result.ShowBG = (Brush)uc.FindResource("ColorBrush.Red8");
                            result.ValueTimeVisibility = Visibility.Visible;
                            result.ShowHeight = 96;
                            result.StatusWidth = 40;
                            result.ShowBorderBrush = (Brush)uc.FindResource("ColorBrush.Red7");
                        }
                    }
                    else if (result.Level.Equals(AdminConsts.Admin_Normal))
                    {
                        result.Level = "3";
                        result.ShowBG = (Brush)uc.FindResource("ColorBrush.Gray13");

                        result.ShowHeight = 78;
                        result.StatusWidth = 40;

                        result.ShowBorderBrush = (Brush)uc.FindResource("ColorBrush.Gray13");
                    }
                    else if (result.Level.Equals(AdminConsts.Admin_Warn))
                    {
                        result.Level = "2";
                        result.ShowBG = (Brush)uc.FindResource("ColorBrush.Gray13");

                        if (sysMsg.type.Equals("1"))
                            result.DealVisibility = Visibility.Collapsed;

                        result.ShowHeight = 96;
                        result.StatusWidth = 42;
                        result.ShowBorderBrush = (Brush)uc.FindResource("ColorBrush.Orange5");
                    }


                    if (string.IsNullOrEmpty(result.ValueTime))
                    {
                        result.ValueTimeVisibility = Visibility.Collapsed;
                    }
                    else
                    {
                        result.ValueTimeVisibility = Visibility.Visible;

                    }

                    if (!string.IsNullOrEmpty(sysMsg.readflag) && sysMsg.readflag.Equals("1"))
                    {
                        result.KnowVisibility = Visibility.Collapsed;
                    }

                    result.CloseByHeight = 0;
                    result.CloseByVisibility = Visibility.Collapsed;
                    result.ShowHeight = 78;

                    if (!string.IsNullOrEmpty(sysMsg.type) && sysMsg.type.Equals("1"))
                    {
                        result.DealVisibility = Visibility.Collapsed;
                        result.BtnShowTxt = "清除";
                    }
                    else if (!string.IsNullOrEmpty(sysMsg.type) && sysMsg.type.Equals("2"))
                    {
                        result.BtnDealShowTxt = "处理";
                        result.BtnShowTxt = "忽略";
                        result.KnowVisibility = Visibility.Visible;
                        result.DealVisibility = Visibility.Visible;
                        if (result.ContentType.Equals(AdminConsts.Admin_ER) || result.ContentType.Equals(AdminConsts.Admin_EV))
                        {
                            if ((!string.IsNullOrEmpty(detail.venue) && AdminHandler.Instance.CheckPermission("1001", string.Empty) && result.Level.Equals("2"))
                                || (result.ContentType.Equals(AdminConsts.Admin_EV)))
                                result.BtnDealShowTxt = "处理";
                            else
                                result.BtnDealShowTxt = "查看";


                        }
                    }
                    if (!string.IsNullOrEmpty(sysMsg.type) && sysMsg.type.Equals("2") && !string.IsNullOrEmpty(detail.detail))
                    {
                        result.Status = "";
                        result.StatusWidth = 50;
                        result.ShowStatusBG = (Brush)uc.FindResource("ColorBrush.Orange5");
                        result.StatusVisibility = Visibility.Visible;

                        if (result.ContentType.Equals(AdminConsts.PriceMatch))
                            result.Status = "请确认";

                    }
                    else if (sysMsg.type.Equals("1"))
                    {
                        if (result.Level.Equals("1"))
                        {
                            result.Status = "";
                            result.StatusWidth = 50;
                            result.ShowStatusBG = (Brush)uc.FindResource("ColorBrush.Red7");
                            result.StatusVisibility = Visibility.Visible;
                        }
                        else if (result.Level.Equals("2"))
                        {
                            result.Status = "";
                            result.StatusWidth = 80;
                            result.ShowStatusBG = (Brush)uc.FindResource("ColorBrush.Orange5");
                            result.StatusVisibility = Visibility.Visible;
                        }
                        else if (result.Level.Equals("3"))
                        {
                            result.StatusVisibility = Visibility.Collapsed;
                        }

                        if (detail.title.Equals("连接中断"))
                            result.Status = "";
                        else if (detail.title.Equals("下单失败") || detail.title.Equals("点价单失败"))
                            result.Status = "请知悉";
                        else if (detail.title.Equals("撤单失败"))
                            result.Status = "";
                        else if (detail.title.Equals("订单超时"))
                            result.Status = "";

                    }

                    if (!string.IsNullOrEmpty(sysMsg.type) && sysMsg.type.Equals("2") && result.Level.Equals("3"))
                    {
                        result.StatusVisibility = Visibility.Collapsed;
                    }

                    result.MsgType = sysMsg.type;

                    result.Detail = detail.detail;
                    result.DelFlag = sysMsg.delflag;

                    result.SignalDetail = sysMsg.detail;

                    if (result.ContentType != null)
                    {
                        if (result.ContentType.Equals(AdminConsts.Admin_IT))
                        {
                            result.TimeOutTxtVisibility = Visibility.Visible;
                            result.UpdateTimeVisibility = Visibility.Collapsed;

                            result.PopDealVisibility = Visibility.Visible;
                            result.PopRefuseVisibility = Visibility.Visible;
                            result.KnowVisibility = Visibility.Collapsed;

                            result.ShowBorderBrush = (Brush)uc.FindResource("ColorBrush.Red16");//ColorBrush.Red7");

                        }
                        else if (result.ContentType.Equals(AdminConsts.Admin_ST))
                        {
                            //如果是RFQ撤单，则隐藏去处理按钮
                            if (sysMsg.closeby == "RFQSvr" || sysMsg.closeby == "RFQMSvr")
                            {
                                result.PopDealVisibility = Visibility.Collapsed;
                                result.DealVisibility = Visibility.Collapsed;
                            }
                        }
                        else if (result.ContentType.Equals("DQB") || result.ContentType.Equals("DQ"))
                        {
                            result.HeaderTitle = !string.IsNullOrEmpty(detail.title) ? string.Format("{0}", detail.title) : string.Empty;
                            //定向回价
                            result.KnowVisibility = Visibility.Collapsed;
                            result.LookVisibility = Visibility.Collapsed;
                            result.PopQuickDealVisibility = Visibility.Collapsed;
                            result.PopDealVisibility = Visibility.Collapsed;
                            result.DealVisibility = Visibility.Visible;
                            result.BtnDealShowTxt = "回价";
                        }
                    }
                }

            }
            catch (Exception e)
            {
                LogHelper.Error(e);
            }
            return result;
        }

        /// <summary>
        /// 是否有内部交易权限和渠道权限
        /// </summary>
        /// <param name="security"></param>
        /// <returns></returns>
        public static bool CheckInnerPermission(string security)
        {
            string goodsType = CommonUtils.SetGoodsShortname(security);
            // 是否有内部交易权限
            bool IsHaveVenueInnerPermission = GetIsHaveInner(goodsType);
            // 渠道权限 内部
            //bool IsHaveInnerPermission = AdminHandler.Instance.CheckPermission(MODULEID, CHECK_INNER_ACTIONID);

            return IsHaveVenueInnerPermission;
        }

        /// <summary>
        /// 是否有内部交易权限
        /// </summary>
        /// <param name="goodsType"></param>
        /// <returns></returns>
        private static bool GetIsHaveInner(string goodsType)
        {
            List<Venue> LstHaveInner = SystemParamHandler.Instance.GetListVenueByFilter(goodsType, "1");
            if (LstHaveInner != null)
            {
                var result = LstHaveInner.Where(delegate (Venue uv)
                {
                    if (uv.venuetypegw == "INNER")
                        return true;
                    else
                        return false;
                }).ToList();

                return result.Count > 0 ? true : false;
            }
            else
                return false;
        }

        #region Functions
        private static string GenerateSourceByVenues(string venues, string partSubName)
        {
            if (string.IsNullOrEmpty(venues) == false)
            {
                if (venues.ToUpper() == "IDB")
                {
                    return $"销售-{partSubName}";
                }
                else if (venues.ToUpper() == "INNER"
                    || venues.ToUpper() == "QUANT"
                    || venues.ToUpper() == "MANUAL")
                {
                    return $"交易员-{partSubName}";
                }
            }
            return venues;
        }

        private static string ParseSettle(string settle)
        {
            string result = string.Empty;

            if (!string.IsNullOrEmpty(settle))
            {
                string[] arr = settle.Split(new char[] { ',' });
                StringBuilder sb = new StringBuilder();
                if (arr != null && arr.Length > 0)
                {
                    foreach (string str in arr)
                    {
                        string tem = CommonMapUtil.FindEnum(str, "settletype");
                        sb.Append(tem + ",");
                    }
                }

                string parseStr = sb.ToString();

                result = parseStr.Length > 0 ? parseStr.Substring(0, parseStr.Length - 1) : parseStr;

            }

            return result;
        }

        /// <summary>
        /// 格式化时间为阅读方式
        /// </summary>
        /// <param name="diffTime"></param>
        /// <returns></returns>
        public static string FormatTime(int diffTime)
        {
            string result = "";
            int hh = int.Parse(Math.Floor(diffTime / 3600.0).ToString());
            int rel = diffTime % 3600;
            int mm = int.Parse(Math.Floor(rel / 60.0).ToString());
            int relsec = diffTime - hh * 3600 - mm * 60;
            if (hh > 0)
            {
                result = $"{hh}小时{mm}分钟{relsec}秒";
            }

            if (hh == 0 && mm > 0)
            {
                result = $"{mm}分钟{relsec}秒";
            }

            if (hh == 0 && mm == 0)
            {
                result = $"{relsec}秒";
            }

            if (hh == 0 && mm == 0 && relsec == 0)
            {
                result = $"";
            }

            //result = result.Replace("钟0秒", "钟").Replace("时0分钟", "时");
            return result;
        }

        /// <summary>
        /// 格式化时间为短的形式
        /// </summary>
        /// <param name="diffTime"></param>
        /// <returns></returns>
        public static string FormatDrpTime(int diffTime)
        {
            string result = "";
            int hh = int.Parse(Math.Floor(diffTime / 3600.0).ToString());
            int rel = diffTime % 3600;
            int mm = int.Parse(Math.Floor(rel / 60.0).ToString());
            int relsec = diffTime - hh * 3600 - mm * 60;

            string smm = ("00" + mm.ToString()).Substring(("00" + mm.ToString()).Length - 2, 2);
            string sss = ("00" + relsec.ToString()).Substring(("00" + relsec.ToString()).Length - 2);

            if (hh > 0)
            {
                result = $"{hh}小时{smm}分钟{sss}秒";
            }

            if (hh == 0 && mm > 0)
            {
                result = $"{smm}分钟{sss}秒";
            }

            if (hh == 0 && mm == 0)
            {
                result = $"{sss}秒";
            }

            if (hh == 0 && mm == 0 && relsec == 0)
            {
                result = $"";
            }

            result = result.Replace("分钟", ":").Replace("小时", ":").Replace("秒", "");
            if (hh == 0 && mm == 0)
            {
                result = $"{result}s";
            }

            return result;
        }


        private static string[] exSecurityArr = new string[] { "SH", "SZ", "sh", "sz" };

        /// <summary>
        /// 是否是银行间
        /// </summary>
        /// <param name="securityID"></param>
        /// <returns></returns>
        public static bool IsBank(string securityID)
        {
            if (string.IsNullOrEmpty(securityID))
                return false;

            bool isBank = false;
            foreach (string str in exSecurityArr)
            {
                if (securityID.ToUpper().EndsWith(str))
                {
                    isBank = true;
                    break;
                }
            }

            return isBank;
        }
        #endregion

        /// <summary>
        /// content 转 双边断报消息ViewModel
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public static MMBreakAlarmReminderViewModel setBeakAlarmReminder(NotificationContent content)
        {
            MMBreakAlarmReminderViewModel result = new MMBreakAlarmReminderViewModel();
            MMBreakAlarmModel ppdm = null;
            result.DataDV.Table.Rows.Clear();
            result.Id = content.Id;
            result.Detail = content.Detail;
            if (content.Detail != null)
            {
                ppdm = Newtonsoft.Json.JsonConvert.DeserializeObject<MMBreakAlarmModel>(content.Detail);
                result.UpdateTime = TimeUtils.Instance.ToTime(ppdm.creatTime);                
                if (ppdm.passiveCount > 0)
                {
                    result.DataDV.Table.Rows.Add("被动关闭债券：", ppdm.passiveCount.ToString()+"只");
                }
                if (ppdm.dangerCount > 0)
                {
                    result.DataDV.Table.Rows.Add("断报券危险：", ppdm.dangerCount.ToString() + "只");
                }
                if ("2".Equals(ppdm.status)) 
                {
                    result.DataDV.Table.Rows.Add("做市状态：", "需关注");
                }                
            }

            return result;
        }
    }
}
