using com.gateway.log;
using STC.Common.Api;
using STC.Common.Data;
using STC.Common.Handler.Admin;
using STC.Common.Handler.Cal;
using STC.Common.Handler.SystemParam;
using STC.Common.Model.Base;
using STC.Common.Model.Cal;
using STC.Common.Model.Fix.Message;
using STC.Common.Utils;
using STC.Common.View;
using STC.Common.View.Order;
using STC.Common.View.UI;
using STC.Common.ViewModel;
using STC.Mm.Plugin.Utils;
using STC.Mm.Plugin.View;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace STC.Mm.Plugin.ViewModel.TradeBookHandler
{
    public class QuickOrderHandler
    {
        private static QuickOrderHandler _quickOrderHandler = new QuickOrderHandler();
        public static QuickOrderHandler Instance { get { return _quickOrderHandler; } }

        /// <summary>
        /// 构造函数
        /// </summary>
        private QuickOrderHandler()
        {

        }

        public void QuickPrice(DataRowView dataGridRow, bool isBid, string securityId)
        {
            string side = isBid ? WindowKeyHelper.BID : WindowKeyHelper.OFR;
            SendPriceView sendPriceView = null;
            if (dataGridRow != null)
            {
                // todo 保价逻辑， 数据在drv中
                string securityID = dataGridRow[CTPriceControler.SECURITY_ID_FIELD].ToString();
                string temSide = dataGridRow[CTPriceControler.SIDE_FIELD].ToString();
                string price = dataGridRow[CTPriceControler.ENTRY_PX_FIELD].ToString();
                // 报量为个人偏好值
                string qty = SystemParamHandler.Instance.GetUserSetting().userQty;

                PriceMatchParam priceMatchParam = new PriceMatchParam();
                priceMatchParam.Side = temSide == "1" ? "SELL" : "BUY";
                priceMatchParam.Price = price;
                priceMatchParam.UpdataPrice = false;
                priceMatchParam.CustomFirmAmount = qty;
                priceMatchParam.IsFirm = true;
                priceMatchParam.Symbol = securityID;
                priceMatchParam.NonfirmTime = string.Empty;
                priceMatchParam.OperationType = "1";
                //设置默认结算方式
                priceMatchParam.SettlType = GetPriceSettle(dataGridRow);
                priceMatchParam.NetPrice = dataGridRow[CTPriceControler.NET_PRICE_FIELD].ToString();
                sendPriceView = new SendPriceView(priceMatchParam);
            }
            else
            {
                sendPriceView = new SendPriceView(securityId, isBid);
            }

            if (!WindowManager.Instance.IsExist(WindowKeyHelper.GetSendPriceWinKey(side, securityId)))
            {
                WindowManager.Instance.OpenOnlyOneWindow(WindowKeyHelper.GetSendPriceWinKey(side, securityId), sendPriceView);
            }
        }

        private string GetPriceSettle(DataRowView drv)
        {
            string result = string.Empty;
            if (drv != null)
            {
                // 结算方式
                result = drv[CTPriceControler.SETTL_TYPE_FIELD].ToString();
                if (string.IsNullOrEmpty(result))
                    result = drv[CTPriceControler.SETTL_TYPE_YUAN_FIELD].ToString();
            }
            return result;
        }


        public void BatchQuickOrder(List<PriceMatchParam> priceMatchParams)
        {
            if (priceMatchParams != null && priceMatchParams.Count > 0)
            {
                foreach (PriceMatchParam priceMatchParam in priceMatchParams)
                {
                    this.QuickOrder(null, priceMatchParam.Side == "1", priceMatchParam, true);
                }
            }
        }


        public bool QuickOrder(DataRowView dg, bool isBid, PriceMatchParam priceMatchParam, bool isQuick)
        {
            bool result = false;
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl) || isQuick)
            {
                result = true;

                NewOrderSingle newOrderSingle = null;
                if (dg != null)
                    newOrderSingle = GetNewOrderSingle(dg, isBid);
                else if (priceMatchParam != null)
                    newOrderSingle = GetNewOrderSingle(priceMatchParam);
                
                ThreadPool.QueueUserWorkItem(o =>
                {
                    try
                    {
                        CalReq calReq = GetCalReq("4", newOrderSingle.SettlType, newOrderSingle.Price, newOrderSingle.SecurityID);

                        CalResult calResult = CalHandler.Instance.CalBond(calReq);
                        if (calResult != null && calResult.Data != null && calResult.IsSuccess())
                        {
                            if (string.IsNullOrEmpty(newOrderSingle.NetPrice))
                                newOrderSingle.NetPrice = calResult.Data.CleanPrice.ToString("F4");

                            SendPointPrice(newOrderSingle);
                        }

                    }
                    catch (Exception e)
                    {
                        LogHelper.Error(e);
                    }
                });
            }
            return result;
        }

        private bool CheckWarnSettle(string settle)
        {
            bool result = false;
            string parseSettle = GetSettle(settle);
            if (!string.IsNullOrEmpty(settle) && ((settle == "+0" || settle == "T+0") || (parseSettle != "1" && parseSettle != "2" && parseSettle != "3" && parseSettle != "4" && parseSettle != "2,3")))
            {
                result = true;
            }

            return result;
        }

        private void SendPointPrice(object newOrder)
        {
            PResult result = OrdersHandler.Instance.PlaceOrder(newOrder as NewOrderSingle);
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                try
                {
                    if (!result.IsSuccess())
                    {
                        if (result.Code == "10000")
                        {
                            SendPriceWarnView warnView = new SendPriceWarnView(result, newOrder as NewOrderSingle);
                            warnView.Owner = Application.Current.MainWindow;
                            warnView.Show();
                        }
                        else
                        {
                            MessageBoxGUI msgBox = new MessageBoxGUI();
                            msgBox.setWarnMsg(result.Msg);
                            msgBox.ShowDialog();
                        }
                    }
                }
                catch (Exception e)
                {
                    LogHelper.Error(e);
                }
            }));
        }

        private string NumChange(string price)
        {
            decimal d = 0;
            if (decimal.TryParse(price, out d))
            {
                d = d / 100;
                price = d.ToString();
            }

            return price;
        }

        private CalReq GetCalReq(string calType, string settleType, string val, string securityid)
        {
            CalReq requst = new CalReq();
            requst.SecurityID = securityid;
            requst.CalType = calType;//"4";
            requst.SettlType = settleType;// "1";
            requst.Value = NumChange(val);// customPrice;
            if (!string.IsNullOrEmpty(LocalEnv.ConfInfo.CalType))
                requst.CalPath = LocalEnv.ConfInfo.CalType;

            return requst;
        }

        private NewOrderSingle GetNewOrderSingle(DataRowView dataGridRow, bool isBid)
        {
            NewOrderSingle newOrder = new NewOrderSingle();

            string source = dataGridRow[CTPriceControler.SOURCE_FIELD].ToString();
            string id = CommonUtils.GetOrderId(dataGridRow[CTPriceControler.KEY_FIELD].ToString());
            string transacTime = dataGridRow[CTPriceControler.TRANSACT_TIME_FILED].ToString();
            string deliveryType = dataGridRow[CTPriceControler.DELIVERY_TYPE_FILED].ToString();
            string clearingMethod = dataGridRow[CTPriceControler.CLEARING_METHOD_FILED].ToString();
            string quoteType = dataGridRow[CTPriceControler.QUOTE_TYPE_FILED].ToString();
            string venueGwType = dataGridRow[CTPriceControler.VENUE_TYPE_GW_FIELD].ToString();
            
            string security = dataGridRow[CTPriceControler.SECURITY_ID_FIELD].ToString();

            List<SimpleBondsInfoPo> bondsInfoPos = CommonUtils.GetBondsInfoPo(security, 1);
            string goodsType = string.Empty;
            SimpleBondsInfoPo bondsInfoPo = null;
            if (bondsInfoPos != null && bondsInfoPos.Count > 0)
            {
                goodsType = bondsInfoPos[0].symbolType;

                bondsInfoPo = bondsInfoPos[0];
            }

            string tradeType = "1";
            if (!string.IsNullOrEmpty(LocalEnv.ConfInfo.TradeType))
                tradeType = LocalEnv.ConfInfo.TradeType;

            if (tradeType == "1")
            {
                if (!string.IsNullOrEmpty(source) && source == "QDMESP")
                {
                    newOrder.AlgoName = "STP";
                    newOrder.VenueTypeGW = "QDMESP";
                    newOrder.Venues = "QDMESP";
                    if (!string.IsNullOrEmpty(id))
                        newOrder.QuoteID = id;//_currentId.Substring(0, _currentId.Length - 1);

                    if (!string.IsNullOrEmpty(transacTime))
                        newOrder.Info2 = transacTime;

                    if (!string.IsNullOrEmpty(deliveryType))
                        newOrder.DeliveryType = deliveryType;

                    if (!string.IsNullOrEmpty(clearingMethod))
                        newOrder.ClearingMethod = clearingMethod;

                    if (!string.IsNullOrEmpty(quoteType))
                        newOrder.QuoteType = quoteType;
                }
                else
                {
                    newOrder.AlgoName = "CROSS";
                    newOrder.VenueTypeGW = "INNER";
                    newOrder.Venues = "MANUAL";
                }
            }
            else if (tradeType == "2")
            {
                newOrder.AlgoName = "STP";
                if (!string.IsNullOrEmpty(venueGwType) && venueGwType == "CDH")
                    newOrder.VenueTypeGW = "SOR";
                else
                    newOrder.VenueTypeGW = venueGwType;

                newOrder.Venues = source;

                if (!string.IsNullOrEmpty(source) && source == "QDMESP")
                {
                    //newOrder.AlgoName = "STP";
                    //newOrder.VenueTypeGW = "QDMESP";
                    //newOrder.Venues = "QDMESP";
                    if (!string.IsNullOrEmpty(id))
                        newOrder.QuoteID = id;//_currentId.Substring(0, _currentId.Length - 1);

                    if (!string.IsNullOrEmpty(transacTime))
                        newOrder.Info2 = transacTime;

                    if (!string.IsNullOrEmpty(deliveryType))
                        newOrder.DeliveryType = deliveryType;

                    if (!string.IsNullOrEmpty(clearingMethod))
                        newOrder.ClearingMethod = clearingMethod;

                    if (!string.IsNullOrEmpty(quoteType))
                        newOrder.QuoteType = quoteType;
                }
                else
                {
                    if (!string.IsNullOrEmpty(id))
                        newOrder.QuoteID = id;
                }
            }



            if (bondsInfoPo != null)
                newOrder.MarketIndicator = bondsInfoPo.MarketIndicator;
            else
                newOrder.MarketIndicator = "4";

            newOrder.UserName = LocalEnv.UserInfo.UserName;
            if (CheckBondType(bondsInfoPo))
            {
                //newOrder.NetPrice = netPrice;
            }
            else
                newOrder.NetPrice = "100";

            newOrder.CrossVenues = GetPermission();//_checkAll ? "1" : _checkInner ? "2" : string.Empty;
            newOrder.ClOrdID = DataTimeSequence.Instance.GetId();//"c" + DateTime.Now.Ticks;
            newOrder.SecurityID = security;
            //newOrder.ValidUntilTime = DateTime.Now.AddHours(3).ToString("yyyy-MM-dd HH:mm:ss");
            newOrder.Side = !isBid ? "1" : "2";
            newOrder.Price = dataGridRow[CTPriceControler.ENTRY_PX_FIELD].ToString(); //CustomPrice;//"4.2";
            newOrder.OrdType = "2";
            newOrder.OrderQty = dataGridRow[CTPriceControler.QTY_FIELD].ToString();// sendPriceItem.Amount;
            newOrder.TimeInForce = "3";
            newOrder.SettlType = GetSettle(dataGridRow);
            newOrder.Terminal = string.Format("STClient_v{0}", LocalEnv.ServerInfo.Version);
            //选中：0(nonfirm)，未选中：1(firm)
            newOrder.FirmFlag = "1";// dataGridRow[CTPriceControler.FIRM_TYPE_FIELD].ToString();// firmChecked ? "0" : "1";

            newOrder.MacID = EngineHelper.GetMacByNetworkInterface();
            newOrder.InsideIP = EngineHelper.GetIp();
            newOrder.HardDiskID = EngineHelper.Getharddiskid2();

            string IocOutTime = string.Empty;
            if (SystemParamHandler.Instance.GetUserSetting() != null
                    && !string.IsNullOrEmpty(SystemParamHandler.Instance.GetUserSetting().userNofirmIOCTimeOut))
            {
                IocOutTime = SystemParamHandler.Instance.GetUserSetting().userNofirmIOCTimeOut;
            }
            else
            {
                IocOutTime = "30";
            }


            newOrder.IocTimeOut = IocOutTime;
            

            return newOrder;
        }


        private NewOrderSingle GetNewOrderSingle(PriceMatchParam priceMatchParam)
        {
            NewOrderSingle newOrder = new NewOrderSingle();

            string tradeType = "1";
            if (!string.IsNullOrEmpty(LocalEnv.ConfInfo.TradeType))
                tradeType = LocalEnv.ConfInfo.TradeType;

            string source = priceMatchParam.Source;
            string id = priceMatchParam.Id;
            string transacTime = priceMatchParam.TrsactTime;
            string deliveryType = priceMatchParam.DeliveryType;
            string clearingMethod = priceMatchParam.ClearingMethod;
            string quoteType = priceMatchParam.QuoteType;

            string security = priceMatchParam.Symbol;

            List<SimpleBondsInfoPo> bondsInfoPos = CommonUtils.GetBondsInfoPo(security, 1);
            string goodsType = string.Empty;
            SimpleBondsInfoPo bondsInfoPo = null;
            if (bondsInfoPos != null && bondsInfoPos.Count > 0)
            {
                goodsType = bondsInfoPos[0].symbolType;

                bondsInfoPo = bondsInfoPos[0];
            }

            if (tradeType == "1")
            {
                if (!string.IsNullOrEmpty(source) && source == "QDMESP")
                {
                    newOrder.AlgoName = "STP";
                    newOrder.VenueTypeGW = "QDMESP";
                    newOrder.Venues = "QDMESP";
                    if (!string.IsNullOrEmpty(id))
                        newOrder.QuoteID = id;//_currentId.Substring(0, _currentId.Length - 1);

                    if (!string.IsNullOrEmpty(transacTime))
                        newOrder.Info2 = transacTime;

                    if (!string.IsNullOrEmpty(deliveryType))
                        newOrder.DeliveryType = deliveryType;

                    if (!string.IsNullOrEmpty(clearingMethod))
                        newOrder.ClearingMethod = clearingMethod;

                    if (!string.IsNullOrEmpty(quoteType))
                        newOrder.QuoteType = quoteType;
                }
                else
                {
                    newOrder.AlgoName = "CROSS";
                    newOrder.VenueTypeGW = "INNER";
                    newOrder.Venues = "MANUAL";
                }
            }
            else if(tradeType == "2")
            {
                newOrder.AlgoName = "STP";
                if (!string.IsNullOrEmpty(priceMatchParam.VenueTypeGW) && priceMatchParam.VenueTypeGW == "CDH")
                    newOrder.VenueTypeGW = "SOR";
                else
                    newOrder.VenueTypeGW = priceMatchParam.VenueTypeGW;

                newOrder.Venues = source;

                if (!string.IsNullOrEmpty(priceMatchParam.Source) && priceMatchParam.Source == "QDMESP")
                {
                    //newOrder.AlgoName = "STP";
                    //newOrder.VenueTypeGW = "QDMESP";
                    //newOrder.Venues = "QDMESP";
                    if (!string.IsNullOrEmpty(id))
                        newOrder.QuoteID = id;//_currentId.Substring(0, _currentId.Length - 1);

                    if (!string.IsNullOrEmpty(priceMatchParam.TrsactTime))
                        newOrder.Info2 = priceMatchParam.TrsactTime;

                    if (!string.IsNullOrEmpty(priceMatchParam.DeliveryType))
                        newOrder.DeliveryType = priceMatchParam.DeliveryType;

                    if (!string.IsNullOrEmpty(priceMatchParam.ClearingMethod))
                        newOrder.ClearingMethod = priceMatchParam.ClearingMethod;

                    if (!string.IsNullOrEmpty(priceMatchParam.QuoteType))
                        newOrder.QuoteType = priceMatchParam.QuoteType;
                }
                else
                {
                    if (!string.IsNullOrEmpty(id))
                        newOrder.QuoteID = id;
                }
            }



            if (bondsInfoPo != null)
                newOrder.MarketIndicator = bondsInfoPo.MarketIndicator;
            else
                newOrder.MarketIndicator = "4";

            newOrder.UserName = LocalEnv.UserInfo.UserName;
            if (CheckBondType(bondsInfoPo))
            {
                //newOrder.NetPrice = netPrice;
            }
            else
                newOrder.NetPrice = "100";

            newOrder.CrossVenues = GetPermission();//_checkAll ? "1" : _checkInner ? "2" : string.Empty;
            newOrder.ClOrdID = DataTimeSequence.Instance.GetId();//"c" + DateTime.Now.Ticks;
            newOrder.SecurityID = security;
            //newOrder.ValidUntilTime = DateTime.Now.AddHours(3).ToString("yyyy-MM-dd HH:mm:ss");
            newOrder.Side = priceMatchParam.Side.Equals("1") ? "2" : "1" ;
            newOrder.Price = priceMatchParam.Price;//CustomPrice;//"4.2";
            newOrder.OrdType = "2";
            newOrder.OrderQty = priceMatchParam.CustomFirmAmount;// sendPriceItem.Amount;
            newOrder.TimeInForce = "3";
            newOrder.SettlType = GetSettle(priceMatchParam.SettlType);
            newOrder.Terminal = string.Format("STClient_v{0}", LocalEnv.ServerInfo.Version);
            //选中：0(nonfirm)，未选中：1(firm)
            //newOrder.FirmFlag = _isFirm ? "1" : "0";
            newOrder.FirmFlag = "1";// priceMatchParam.IsFirm ? "0" : "1";

            newOrder.MacID = EngineHelper.GetMacByNetworkInterface();
            newOrder.InsideIP = EngineHelper.GetIp();
            newOrder.HardDiskID = EngineHelper.Getharddiskid2();

            string IocOutTime = string.Empty;
            if (SystemParamHandler.Instance.GetUserSetting() != null
                    && !string.IsNullOrEmpty(SystemParamHandler.Instance.GetUserSetting().userNofirmIOCTimeOut))
            {
                IocOutTime = SystemParamHandler.Instance.GetUserSetting().userNofirmIOCTimeOut;
            }
            else
            {
                IocOutTime = "30";
            }


            newOrder.IocTimeOut = IocOutTime;


            return newOrder;
        }


        private string GetSettle(string settle)
        {
            string result = string.Empty;
            if (!string.IsNullOrEmpty(settle))
            {
                if (settle.Equals("+1") || settle.Equals("2") || settle.Equals("T+1") || settle.Equals("T+1(不限)")
                        || settle.Equals("2,3") || settle.Equals("3") || settle.Equals("T+1(明+0)") || settle.Equals("T+1(今+1)"))
                {
                    result = "2";

                }
                else
                {
                    if (settle.Equals("1") || settle.Equals("T+0"))//T+0
                    {
                        result = "1";
                    }
                    else if (settle.Equals("明天+1") || settle.Equals("4"))
                    {
                        result = "4";
                    }
                    else
                    {
                        result = settle;
                    }
                }
            }

            return result;
        }

        private bool CheckBondType(SimpleBondsInfoPo bondsInfoPo)
        {
            return bondsInfoPo != null && !string.IsNullOrEmpty(bondsInfoPo.MarketIndicator) && bondsInfoPo.MarketIndicator.Equals("4");
        }

        private string GetSettle(DataRowView drv)
        {
            string result = string.Empty;
            if (drv != null)
            {
                // 结算方式
                result = drv[CTPriceControler.SETTL_TYPE_FIELD].ToString();
                if (string.IsNullOrEmpty(result))
                    result = drv[CTPriceControler.SETTL_TYPE_YUAN_FIELD].ToString();
            }
            return result;
        }

        private string GetPermission()
        {
            string result = string.Empty;
            bool isAllow = AdminHandler.Instance.CheckPermission(ApiConsts.MODULEID, ApiConsts.CHECK_ALL_ACTIONID);
            if (isAllow)
            {
                result = "1";
            }
            else
            {
                isAllow = AdminHandler.Instance.CheckPermission(ApiConsts.MODULEID, ApiConsts.CHECK_INNER_ACTIONID);
                if (isAllow)
                {
                    result = "2";
                }
                else
                {
                    isAllow = AdminHandler.Instance.CheckPermission(ApiConsts.MODULEID, ApiConsts.CHECK_INNER_XBOND_ACTIONID);
                    result = "3";
                }
            }
            return result;
        }
    }
}
