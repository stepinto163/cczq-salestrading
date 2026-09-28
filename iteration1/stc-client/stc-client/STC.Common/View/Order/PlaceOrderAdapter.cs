using Ats.Foundation.Message;
using com.gateway.log;
using netapi.Api;
using Newtonsoft.Json;
using STC.Common.Data;
using STC.Common.Handler.SystemParam;
using STC.Common.Model;
using STC.Common.Model.Base;
using STC.Common.Model.Cal;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.TBook;
using STC.Common.Utils;
using STC.Common.View.UI;
using STC.Common.ViewModel.Order;
using STC.Common.ViewModel.Order.InnerPanel;
using STC.Common.ViewModel.Order.InnerView;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace STC.Common.View.Order
{
    public class PlaceOrderAdapter
    {
        private static PlaceOrderAdapter placeOrderHelper = new PlaceOrderAdapter();

        public static PlaceOrderAdapter Instance { get { return placeOrderHelper; } }

        private PlaceOrderAdapter()
        { }

        private SendPriceViewModel sendPriceViewModel;
        private Window myWindow;

        public void PlaceOrder(SendPriceViewModel sendPriceViewModel, Window myWindow)
        {
            this.sendPriceViewModel = sendPriceViewModel;
            this.myWindow = myWindow;

            if (ApiManager.Instance.GetDefaultApi().EnabledMsgLog == "1")
            {
                LogHandler.Info("点击报价按钮");
            }
            sendPriceViewModel.SendPriceEnable = false;

            if (CheckParam(sendPriceViewModel))
            {
                if (sendPriceViewModel.OnCustomConfirmAction != null)
                {
                    sendPriceViewModel.OnCustomConfirmAction(GetNewOrderSingle());
                    return;
                }

                PriceMatchParam priceMatchParam = sendPriceViewModel.GetPriceMatchParam();
                if (priceMatchParam != null && priceMatchParam.UpdataPrice)
                    UpdatePrice();
                else
                    PlaceOrder();
            }
            else
            {
                sendPriceViewModel.SendPriceEnable = true;
                SendPriceWarnView warnView = new SendPriceWarnView(GetErrorTxt(), GetNewOrderSingle());
                warnView.Owner = myWindow;
                warnView.Show();
            }
        }

        /// <summary>
        /// 报价错误信息
        /// </summary>
        /// <returns></returns>
        private BaseReplyPo GetErrorTxt()
        {
            BaseReplyPo reply = new BaseReplyPo();
            reply.Code = "10001";

            List<TPlaceOrderResult> result = new List<TPlaceOrderResult>();

            TPlaceOrderResult por = new TPlaceOrderResult();
            por.Code = "10003";
            por.DoubleWarn = false;
            decimal temD = 0;

            if (sendPriceViewModel.GetDetailConditionViewModel().HourValidTimeQuotationCheck)
            {
                // 时长
                // ValidMinute  ValidSecond
                if (string.IsNullOrEmpty(sendPriceViewModel.GetDetailConditionViewModel().ValidMinute) && string.IsNullOrEmpty(sendPriceViewModel.GetDetailConditionViewModel().ValidSecond))
                {
                    por.Reason = "有效时长需大于0秒";
                }

                int totalsecond = 0;
                if (!string.IsNullOrEmpty(sendPriceViewModel.GetDetailConditionViewModel().ValidMinute))
                {
                    totalsecond = int.Parse(sendPriceViewModel.GetDetailConditionViewModel().ValidMinute) * 60;
                }

                if (!string.IsNullOrEmpty(sendPriceViewModel.GetDetailConditionViewModel().ValidSecond))
                {
                    totalsecond += int.Parse(sendPriceViewModel.GetDetailConditionViewModel().ValidSecond);
                }

                if (totalsecond == 0)
                {
                    por.Reason = "有效时长需大于0秒";
                }
            }
            else if (sendPriceViewModel.GetDetailConditionViewModel().TimeValidTimeQuotationCheck)
            {
                // 时点
                // ValidTimeHour ValidTimeMinute ValidTimeSecond
                if (string.IsNullOrEmpty(sendPriceViewModel.GetDetailConditionViewModel().ValidTimeHour))
                {
                    por.Reason = "必须输入时";
                }

                int hour = 0;
                int min = 0;
                int sec = 0;

                if (!string.IsNullOrEmpty(sendPriceViewModel.GetDetailConditionViewModel().ValidTimeHour))
                {
                    hour = int.Parse(sendPriceViewModel.GetDetailConditionViewModel().ValidTimeHour);
                }

                if (!string.IsNullOrEmpty(sendPriceViewModel.GetDetailConditionViewModel().ValidTimeMinute))
                {
                    min = int.Parse(sendPriceViewModel.GetDetailConditionViewModel().ValidTimeMinute);
                }

                if (!string.IsNullOrEmpty(sendPriceViewModel.GetDetailConditionViewModel().ValidTimeSecond))
                {
                    sec = int.Parse(sendPriceViewModel.GetDetailConditionViewModel().ValidTimeSecond);
                }

                DateTime tmpdt = DateTime.Parse(DateTimeEx.ServerTimeNow.ToString("yyyy-MM-dd").ToString());
                tmpdt = tmpdt.AddHours(hour).AddMinutes(min).AddSeconds(sec);
                if (DateTimeEx.ServerTimeNow >= tmpdt)
                {
                    por.Reason = "订单撤单时点不能在报单时间之前";
                }
            }

            OCOViewModel ocoViewModel = sendPriceViewModel.getCrossViewModel().GetOCOViewModel();
            CrossViewModel crossViewModel = sendPriceViewModel.getCrossViewModel();

            if (crossViewModel.CrossVisibility == Visibility.Collapsed && crossViewModel.OCOVisibility == Visibility.Collapsed && crossViewModel.SniperVisibility == Visibility.Collapsed)
            {
                por.Reason = "没有执行权限";
            }
            else if (ocoViewModel.CheckAllVisibility == Visibility.Collapsed && ocoViewModel.CheckInnerVisibility == Visibility.Collapsed && ocoViewModel.CheckXBondVisibility == Visibility.Collapsed)
                por.Reason = "没有撮合渠道权限";
            else if (string.IsNullOrEmpty(sendPriceViewModel.SendPriceItem.Security))
                por.Reason = "未选择债券";
            else if (!decimal.TryParse(sendPriceViewModel.CustomPrice, out temD) || temD == 0)// string.IsNullOrEmpty(CustomPrice))
            {
                if (!(ocoViewModel.CheckInnerVisibility == Visibility.Visible && ocoViewModel.CheckInner && sendPriceViewModel.FirmChecked))
                {
                    por.Reason = "未填写价格或价格为0";
                }
            }
            else if (sendPriceViewModel.GetQtyViewModel().GetAmount() == 0)// string.IsNullOrEmpty(sendPriceItem.Amount))
            {
                if (!(ocoViewModel.CheckInnerVisibility == Visibility.Visible && ocoViewModel.CheckInner && sendPriceViewModel.FirmChecked))
                {
                    por.Reason = "未填写量或量为0";
                }
            }
            else if (string.IsNullOrEmpty(sendPriceViewModel.NetPrice))
            {
                if (sendPriceViewModel.GetCalBondErrMsg().Length > 0)
                {
                    por.Reason = $"{sendPriceViewModel.GetCalBondErrMsg()}, 调用计算器服务失败,请重新打开窗口或修改报价参数";
                }
                else
                {
                    por.Reason = $"{sendPriceViewModel.GetLastCalResult().Msg}";
                }
            }
            else if (crossViewModel.SelectTabIndex == 1 && !ocoViewModel.CheckAll && !ocoViewModel.CheckInner && !ocoViewModel.CheckXBond)
                por.Reason = "必须选择一个撮合渠道";
            //else if (!ocoViewModel.CheckAll && !ocoViewModel.CheckInner && ocoViewModel.CheckXBond)
            //    por.Reason = "撮合渠道不能单选X-bond";
            //else if (ocoViewModel.CheckAll)
            //{
            //    string str = sendPriceViewModel.GetSendPriceModelAdapter().GetSelectItem(myWindow.FindName("lbDirect") as ListBox);
            //    if (string.IsNullOrEmpty(str))
            //        por.Reason = "余量发送渠道-指定，必须选择一个渠道";
            //}
            else if (crossViewModel.SelectTabIndex == 1 && ocoViewModel.CheckAll && !ocoViewModel.CheckNotOut)
                por.Reason = "余量发送渠道必须选择一个";


            if (crossViewModel.SelectTabIndex == 1)
            {
                // 检查个别情况的验证
                string venuesSelectF = string.Empty;
                if (!(ocoViewModel.CheckInner && ocoViewModel.CheckXBondVisibility == Visibility.Collapsed))
                    venuesSelectF = ocoViewModel.CheckNotOut ? "1" : "3";
                if (venuesSelectF == "3")
                {
                    string selfSelectVenues = sendPriceViewModel.getCrossViewModel().GetSelectItem();
                    if (string.IsNullOrEmpty(selfSelectVenues))
                    {
                        por.Reason = "余量发送渠道必须选择一个";
                    }
                }
            }

            if (crossViewModel.SelectTabIndex == 0)
            {
                string selectItem = crossViewModel.GetSelectItem();
                bool isInner = false;
                if (sendPriceViewModel.FirmChecked || sendPriceViewModel.WholeFlagChecked)
                {
                    string[] arr = selectItem.Split(new string[] { "-" }, StringSplitOptions.RemoveEmptyEntries);
                    if (arr != null && arr.Length > 0)
                    {
                        string selectVenueGwType = arr[1];
                        string venue = arr[0];

                        if (!string.IsNullOrEmpty(venue) && venue == "INNER")
                        {
                            isInner = true;
                        }
                    }
                }


                if (sendPriceViewModel.FirmChecked && !isInner)
                    por.Reason = "直通单不支持Nonfirm";
                else if (sendPriceViewModel.WholeFlagChecked && !isInner)
                    por.Reason = "直通单不支持整量";
                else
                {
                    
                    if (string.IsNullOrEmpty(selectItem))
                    {
                        por.Reason = "直通单必须选择一个渠道";
                    }

                    string settle = GetSetttle();
                    if (selectItem.ToUpper().Contains("ODM") && !(settle.Equals("1") || settle.Equals("2")))
                    {
                        por.Reason = "XBOND渠道只支持单选+0、+1或今+1";
                    }

                    if (selectItem.ToUpper().Contains("ODM"))
                    {
                        BondsInfoPo bondsInfoPo = CommonUtils.GetBondsInfoPoBySecurityID(sendPriceViewModel.SendPriceItem.Security);
                        if (bondsInfoPo.flag != "Y")
                            por.Reason = "非ODM可用债券";
                    }
                }
            }
            else if (crossViewModel.SelectTabIndex == 1)
            {
                string selectItem = crossViewModel.GetSelectItem();
                string settle = GetSetttle();
                if (selectItem.ToUpper().Contains("ODM") && !(settle.Equals("1") || settle.Equals("2")))
                {
                    por.Reason = "XBOND渠道只支持单选+0、+1或今+1";
                }
            }

            if (sendPriceViewModel.GetLastCalResult() != null && sendPriceViewModel.GetLastCalResult().Data != null 
                && !string.IsNullOrEmpty(sendPriceViewModel.GetLastCalResult().ReqData) && sendPriceViewModel.GetLastCalResult().IsSuccess()
                && !string.IsNullOrEmpty(sendPriceViewModel.GetLastReqData()))
            {
                if (sendPriceViewModel.GetLastCalResult().ReqData.ToString() != sendPriceViewModel.GetLastReqData().ToString())
                {
                    por.Reason = $"{sendPriceViewModel.GetLastCalResult().Msg}";
                }
            }

            DetailConditionViewModel detailConditionViewModel = sendPriceViewModel.GetDetailConditionViewModel();

            PriceCondition priceCondition = detailConditionViewModel.GetPriceCondition();
            if (priceCondition != null)
            {
                if (priceCondition.CounterpartyBestOffer == string.Empty)
                {
                    por.Reason = "条件撤销已勾选，必须输入bp";
                }
            }

            result.Add(por);

            reply.Msg = MsgHelper.Serializer<List<TPlaceOrderResult>>(result);

            return reply;
        }

        /// <summary>
        /// 下单
        /// </summary>
        private void PlaceOrder()
        {
            NewOrderSingle sendPricePo = GetNewOrderSingle();
            sendPriceViewModel.SendPriceEnable = false;

            ThreadPool.QueueUserWorkItem(o =>
            {
                try
                {
                    PResult result = OrdersHandler.Instance.PlaceOrder(sendPricePo);


                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        try
                        {

                            if (result != null)
                            {
                                if (result.Code == "10000" || result.IsSuccess())
                                {
                                    SendPriceWarnView warnView = new SendPriceWarnView(result, sendPricePo);
                                    warnView.OnAction += WarnView_OnAction;
                                    if (!result.IsSuccess())
                                    {
                                        warnView.ShowDialog();
                                    }
                                    else
                                    {
                                        warnView.Owner = Application.Current.MainWindow;
                                        warnView.Show();
                                    }
                                }
                                else
                                {
                                    MessageBoxGUI msgBox = new MessageBoxGUI();
                                    msgBox.setWarnMsg(result.Msg);
                                    msgBox.Owner = myWindow;
                                    msgBox.ShowDialog();

                                    OnCancelClick();
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            LogHelper.Error(e);

                            OnCancelClick();
                        }
                    }));
                }
                catch (Exception e)
                {
                    LogHelper.Error(e);
                }
            });
        }

        /// <summary>
        /// 处理报价警告窗口的用户操作
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="isCancel"></param>
        /// <param name="isDoubleCheck"></param>
        private void WarnView_OnAction(object sender, bool isCancel, bool isDoubleCheck)
        {
            SendPriceWarnView warnView = sender as SendPriceWarnView;
            warnView.OnAction -= WarnView_OnAction;
            if (!isCancel)
                OnCancelClick();
            else
                sendPriceViewModel.SendPriceEnable = true;
        }

        private void OnCancelClick()
        {
            if (sendPriceViewModel.GetSendPriceModelAdapter() != null)
                sendPriceViewModel.GetSendPriceModelAdapter().Clear();

            if (myWindow != null)
                myWindow.Close();
        }


        /// <summary>
        /// 生成订单报文
        /// </summary>
        /// <returns></returns>
        private NewOrderSingle GetNewOrderSingle()
        {
            NewOrderSingle sendPricePo = new NewOrderSingle();

            string selectVenueGwType = string.Empty;

            string selectItem = sendPriceViewModel.getCrossViewModel().GetSingleSelectItem();
            string[] arr = selectItem.Split(new string[] { "-" }, StringSplitOptions.RemoveEmptyEntries);
            if (arr != null && arr.Length > 0)
            {
                selectVenueGwType = arr[1];
            }

            if (sendPriceViewModel.getCrossViewModel().SelectTabIndex == 0)
            {
                sendPricePo.AlgoName = "STP";

                if (arr != null && arr.Length > 0) 
                {
                    string venue = arr[0];

                    if (!string.IsNullOrEmpty(selectVenueGwType) && selectVenueGwType == "CDH")
                        sendPricePo.VenueTypeGW = "SOR";
                    else
                        sendPricePo.VenueTypeGW = selectVenueGwType;

                    sendPricePo.Venues = venue;

                    if (!string.IsNullOrEmpty(selectVenueGwType) && selectVenueGwType == "INNER")
                    {
                        sendPricePo.AlgoName = LocalEnv.ConfInfo.STPINNERAlgoName;
                        sendPricePo.VenueTypeGW = "INNER";
                        sendPricePo.Venues = "MANUAL";
                    }
                }
            }
            else if (sendPriceViewModel.getCrossViewModel().SelectTabIndex == 1 || sendPriceViewModel.getCrossViewModel().SelectTabIndex == 2)
            {
                sendPricePo.AlgoName = "CROSS";
                sendPricePo.VenueTypeGW = "INNER";
                sendPricePo.Venues = "MANUAL";
            }

            
            sendPricePo.ClOrdID = DataTimeSequence.Instance.GetId();
            PriceMatchParam priceMatchParam = sendPriceViewModel.GetPriceMatchParam();
            if (priceMatchParam != null)
            {
                sendPricePo.Info1 = priceMatchParam.Info1;
                sendPricePo.Info2 = priceMatchParam.Info2;
                sendPricePo.Info3 = priceMatchParam.Info3;
                sendPricePo.Info4 = priceMatchParam.Info4;
                sendPricePo.Info5 = priceMatchParam.Info5;
            }

            //sendPricePo.MarketIndicator = "4";
            BondsInfoPo bondsInfoPo = sendPriceViewModel.GetBondsInfoPo();
            if (bondsInfoPo != null)
                sendPricePo.MarketIndicator = bondsInfoPo.MarketIndicator;
            else
                sendPricePo.MarketIndicator = "4";


            sendPricePo.OrdType = "2";

            if (sendPriceViewModel.GetQtyViewModel() != null && sendPriceViewModel.GetQtyViewModel().GetAmount() != 0)
                sendPricePo.OrderQty = sendPriceViewModel.GetQtyViewModel().TrueAmount?.ToString();

            sendPricePo.Price = sendPriceViewModel.CustomPrice;

            if (sendPricePo.MarketIndicator == "4")
                sendPricePo.NetPrice = sendPriceViewModel.NetPrice;
            else
                sendPricePo.NetPrice = "100";
            sendPricePo.PriceType = "3"; //收益率
            sendPricePo.SecurityID = sendPriceViewModel.SendPriceItem.Security;

            sendPricePo.RiskManualCk = string.Empty;//默认空值（检查）, 1：只检查风控禁止，2：风控不检查

            if (sendPriceViewModel.SettleBoxIndex == 1)
            {
                sendPricePo.SettlType = sendPriceViewModel.SendPriceItem.SettleType;
            }
            else
                sendPricePo.SettlType = GetSetttle();

            sendPricePo.Side = sendPriceViewModel.IsBid ? "1" : "2";

            sendPricePo.Terminal = string.Format("STClient_{0}", LocalEnv.ServerInfo.Version);
            sendPricePo.TimeInForce = "0";
            sendPricePo.UserName = LocalEnv.UserInfo.UserName;

            //选中：0(nonfirm)，未选中：1(firm)
            sendPricePo.FirmFlag = sendPriceViewModel.FirmChecked ? "0" : "1";

            sendPricePo.SelfSelectVenues = sendPriceViewModel.getCrossViewModel().GetSelectItem();

            if (sendPriceViewModel.getCrossViewModel().SelectTabIndex == 1)
            {
                OCOViewModel ocoViewModel = sendPriceViewModel.getCrossViewModel().GetOCOViewModel();
                sendPricePo.CrossVenues = (ocoViewModel.CheckAll && ocoViewModel.ChanelAllVisibility==Visibility.Visible) ? "1" : ocoViewModel.CheckInner ? "2" : string.Empty;

                if (ocoViewModel.CheckXBond && ocoViewModel.CheckXBondVisibility == Visibility.Visible)
                {
                    sendPricePo.CrossVenues = "3";
                    //sendPricePo.Venues = "ODM"; //ODM=XB  QDMESP=双边
                }

                if (!(ocoViewModel.CheckInner && ocoViewModel.CheckXBondVisibility == Visibility.Collapsed))
                {
                    sendPricePo.VenuesSelectF = ocoViewModel.CheckNotOut ? "1" : "3";

                    if (!string.IsNullOrEmpty(selectVenueGwType) && selectVenueGwType == "INNER" && !ocoViewModel.CheckNotOut)
                    {
                        sendPricePo.VenuesSelectF = "1";
                        sendPricePo.SelfSelectVenues = string.Empty;
                    }
                }
            }
            else if (sendPriceViewModel.getCrossViewModel().SelectTabIndex == 2)
            {
                sendPricePo.CrossVenues = "1";
                sendPricePo.VenuesSelectF = "1";
            }


            if (sendPriceViewModel.getCrossViewModel().SelectTabIndex == 0)
            {
                if (!string.IsNullOrEmpty(selectVenueGwType) && selectVenueGwType == "INNER")
                {
                    sendPricePo.CrossVenues = "2";
                    sendPricePo.VenuesSelectF = "1";
                }
                else
                {
                    sendPricePo.CrossVenues = string.Empty;
                }
            }

            if (!sendPriceViewModel.WholeFlagChecked)
                sendPricePo.WholeFlag = "0";
            else
                sendPricePo.WholeFlag = "1";

            sendPricePo.HandlInst = sendPriceViewModel.DarkDiskFlagChecked ? "2" : "1";

            sendPricePo.QuantID = "";
            decimal d = 0;
            
            

            

            sendPricePo.MacID = EngineHelper.GetMacByNetworkInterface();
            sendPricePo.InsideIP = EngineHelper.GetIp();
            sendPricePo.HardDiskID = EngineHelper.Getharddiskid2();

            DetailConditionViewModel detailConditionViewModel = sendPriceViewModel.GetDetailConditionViewModel();

            // 默认时间
            if (detailConditionViewModel.DefaultValidTimeQuotationCheck)
            {
                int hour = DateTimeEx.ServerTimeNow.Hour;
                int min = DateTimeEx.ServerTimeNow.Minute;
                if (hour > 0 && hour < 6)
                {
                    sendPricePo.ValidUntilTime = (DateTime.Parse(DateTimeEx.ServerTimeNow.ToString("yyyy-MM-dd") + " 06:00:00")).ToString("yyyy-MM-dd HH:mm:ss");
                }
                else if (hour >= 6 && hour < 12)
                {
                    sendPricePo.ValidUntilTime = (DateTime.Parse(DateTimeEx.ServerTimeNow.ToString("yyyy-MM-dd") + " 12:00:00")).ToString("yyyy-MM-dd HH:mm:ss");
                }
                else
                {
                    sendPricePo.ValidUntilTime = (DateTime.Parse(DateTimeEx.ServerTimeNow.ToString("yyyy-MM-dd") + " 16:30:00")).ToString("yyyy-MM-dd HH:mm:ss");
                }
            }

            // 时长
            if (detailConditionViewModel.HourValidTimeQuotationCheck)
            {
                // ValidMinute  ValidSecond
                int totalsecond = 0;
                if (!string.IsNullOrEmpty(detailConditionViewModel.ValidMinute))
                {
                    totalsecond = int.Parse(detailConditionViewModel.ValidMinute) * 60;
                }

                if (!string.IsNullOrEmpty(detailConditionViewModel.ValidSecond))
                {
                    totalsecond += int.Parse(detailConditionViewModel.ValidSecond);
                }

                if (totalsecond == 0)
                {
                    sendPricePo.ValidUntilTime = DateTimeEx.ServerTimeNow.AddSeconds(detailConditionViewModel.DefaultValidUntilTime).ToString("yyyy-MM-dd HH:mm:ss");
                }
                else
                {
                    sendPricePo.ValidUntilTime = DateTimeEx.ServerTimeNow.AddSeconds(totalsecond).ToString("yyyy-MM-dd HH:mm:ss");
                }
            }

            // 时点
            if (detailConditionViewModel.TimeValidTimeQuotationCheck)
            {
                // ValidTimeHour ValidTimeMinute ValidTimeSecond
                int hour = 0;
                int min = 0;
                int sec = 0;

                if (!string.IsNullOrEmpty(detailConditionViewModel.ValidTimeHour))
                {
                    hour = int.Parse(detailConditionViewModel.ValidTimeHour);
                }

                if (!string.IsNullOrEmpty(detailConditionViewModel.ValidTimeMinute))
                {
                    min = int.Parse(detailConditionViewModel.ValidTimeMinute);
                }

                if (!string.IsNullOrEmpty(detailConditionViewModel.ValidTimeSecond))
                {
                    sec = int.Parse(detailConditionViewModel.ValidTimeSecond);
                }

                DateTime tmpdt = DateTime.Parse(DateTimeEx.ServerTimeNow.ToString("yyyy-MM-dd").ToString());
                tmpdt = tmpdt.AddHours(hour).AddMinutes(min).AddSeconds(sec);
                sendPricePo.ValidUntilTime = tmpdt.ToString("yyyy-MM-dd HH:mm:ss");
            }

            // 无
            if (detailConditionViewModel.NoLimitValidTimeQuotationCheck)
            {
                // 不填写
            }
            
            //订阅标签
            TagData tagData = detailConditionViewModel.GetTagData();

            //条件撤单
            PriceCondition priceCondition = detailConditionViewModel.GetPriceCondition();
            if (priceCondition != null)
            {
                sendPricePo.PriceCondition = JsonConvert.SerializeObject(priceCondition);
            }

            // Modi:cyc Date:2021-04-13 Func:增加用户选择的 需求有效时长 选项记忆功能
            string validTimeType = detailConditionViewModel.DefaultValidTimeQuotationCheck ? "0" : detailConditionViewModel.HourValidTimeQuotationCheck ? "1" : detailConditionViewModel.TimeValidTimeQuotationCheck ? "2" : detailConditionViewModel.NoLimitValidTimeQuotationCheck ? "3" : "0";
            // 默认
            string info1 = string.Empty;
            if (detailConditionViewModel.DefaultValidTimeQuotationCheck)
            {
                info1 = $"{validTimeType}|";
                OrderTimeModel otm = new OrderTimeModel() { ValidTimeType = info1, ValidUntilTime = sendPricePo.ValidUntilTime, IF_SUB_BOND = tagData.IF_SUB_BOND ?? string.Empty, VIP= tagData.VIP ?? string.Empty, ORG_TYPE = tagData.ORG_TYPE?? string.Empty, MorethanMarketBPCheck = priceCondition.MorethanMarketBPCheck ?? string.Empty, MorethanMarketBP = priceCondition.MorethanMarketBP ?? string.Empty };
                sendPricePo.Info1 = JsonConvert.SerializeObject(otm);
            }

            // 时长
            if (detailConditionViewModel.HourValidTimeQuotationCheck)
            {
                // ValidMinute,ValidSecond
                string min = string.IsNullOrEmpty(detailConditionViewModel.ValidMinute) ? "" : detailConditionViewModel.ValidMinute;
                string sec = string.IsNullOrEmpty(detailConditionViewModel.ValidSecond) ? "" : detailConditionViewModel.ValidSecond;
                info1 = $"{validTimeType}|{min}:{sec}";
                OrderTimeModel otm = new OrderTimeModel() { ValidTimeType = info1, ValidUntilTime = sendPricePo.ValidUntilTime, IF_SUB_BOND = tagData.IF_SUB_BOND ?? string.Empty, VIP = tagData.VIP ?? string.Empty, ORG_TYPE = tagData.ORG_TYPE ?? string.Empty, MorethanMarketBPCheck = priceCondition.MorethanMarketBPCheck ?? string.Empty, MorethanMarketBP = priceCondition.MorethanMarketBP ?? string.Empty };
                sendPricePo.Info1 = JsonConvert.SerializeObject(otm);
            }
            // 时点
            if (detailConditionViewModel.TimeValidTimeQuotationCheck)
            {
                // ValidTimeHour ValidTimeMinute ValidTimeSecond
                string hour = string.IsNullOrEmpty(detailConditionViewModel.ValidTimeHour) ? "" : detailConditionViewModel.ValidTimeHour;
                string min = string.IsNullOrEmpty(detailConditionViewModel.ValidTimeMinute) ? "" : detailConditionViewModel.ValidTimeMinute;
                string sec = string.IsNullOrEmpty(detailConditionViewModel.ValidTimeSecond) ? "" : detailConditionViewModel.ValidTimeSecond;
                info1 = $"{validTimeType}|{hour}:{min}:{sec}";
                OrderTimeModel otm = new OrderTimeModel() { ValidTimeType = info1, ValidUntilTime = sendPricePo.ValidUntilTime, IF_SUB_BOND = tagData.IF_SUB_BOND ?? string.Empty, VIP = tagData.VIP ?? string.Empty, ORG_TYPE = tagData.ORG_TYPE ?? string.Empty, MorethanMarketBPCheck = priceCondition.MorethanMarketBPCheck ?? string.Empty, MorethanMarketBP = priceCondition.MorethanMarketBP ?? string.Empty };
                sendPricePo.Info1 = JsonConvert.SerializeObject(otm);
            }

            // 无
            if (detailConditionViewModel.NoLimitValidTimeQuotationCheck)
            {
                info1 = $"{validTimeType}|";
                OrderTimeModel otm = new OrderTimeModel() { ValidTimeType = info1, ValidUntilTime = string.Empty, IF_SUB_BOND = tagData.IF_SUB_BOND ?? string.Empty, VIP = tagData.VIP ?? string.Empty, ORG_TYPE = tagData.ORG_TYPE ?? string.Empty, MorethanMarketBPCheck = priceCondition.MorethanMarketBPCheck ?? string.Empty, MorethanMarketBP = priceCondition.MorethanMarketBP ?? string.Empty };
                sendPricePo.Info1 = JsonConvert.SerializeObject(otm);
            }

            // 用户设置的到期时间,需要大于用户自定义的时间
            string selfUntilTime = sendPricePo.ValidUntilTime;
            UserSetting userSetting = SystemParamHandler.Instance.CopyUserSetting();
            if (userSetting != null)
            {
                // 个人设置时间
                string validUntiTimeSetting = userSetting.validUntiTimeSetting;

                // 如果撤单时间和个人设置时间不为空
                if (!string.IsNullOrEmpty(selfUntilTime) && (!string.IsNullOrEmpty(validUntiTimeSetting)))
                {
                    validUntiTimeSetting = string.Format("{0} {1}", DateTimeEx.ServerTimeNow.ToString("yyyy-MM-dd").ToString(), validUntiTimeSetting);
                    // 默认，时长，时点
                    if (!detailConditionViewModel.NoLimitValidTimeQuotationCheck)
                    {
                        sendPricePo.ValidUntilTime = GetMinDate(selfUntilTime, validUntiTimeSetting, detailConditionViewModel.DefaultValidTimeQuotationCheck);
                    }
                    else
                    {
                        //选择了无
                        if (DateTimeEx.ServerTimeNow.CompareTo(DateTime.Parse(validUntiTimeSetting)) <= 0)
                        {
                            sendPricePo.ValidUntilTime = validUntiTimeSetting;
                        }
                        else
                        {
                            sendPricePo.ValidUntilTime = string.Empty;
                        }
                    }
                }
                else
                {
                    // 默认，时长，时点
                    if (!detailConditionViewModel.NoLimitValidTimeQuotationCheck)
                    {
                        // 默认
                        if (detailConditionViewModel.DefaultValidTimeQuotationCheck)
                        {
                            sendPricePo.ValidUntilTime = GetMinDate(selfUntilTime, "", true);
                        }
                    }
                    else
                    {
                        sendPricePo.ValidUntilTime = string.Empty;
                    }
                }
            }

            if (priceMatchParam != null)
            {
                sendPricePo.Info3 = priceMatchParam.Info3;
                sendPricePo.Info4 = priceMatchParam.Info4;
            }

            if (priceMatchParam != null && priceMatchParam.OldOrder != null)
            {
                //sendPricePo.AlgoName = priceMatchParam.OldOrder.AlgoName;
                //sendPricePo.VenueTypeGW = priceMatchParam.OldOrder.VenueTypeGW;

                //if (!(!FirmChecked && (!string.IsNullOrEmpty(_priceMatchParam.OldOrder.Venues) && _priceMatchParam.OldOrder.Venues.ToUpper().Equals("IDB"))))
                //    sendPricePo.Venues = _priceMatchParam.OldOrder.Venues;

                sendPricePo.UserID = priceMatchParam.OldOrder.UserID;
                sendPricePo.UserName = priceMatchParam.OldOrder.UserName;
                sendPricePo.BizType = priceMatchParam.OldOrder.BizType;
                sendPricePo.BizTypeID = priceMatchParam.OldOrder.BizTypeID;
                sendPricePo.RoleID = priceMatchParam.OldOrder.RoleID;
                sendPricePo.RoleName = priceMatchParam.OldOrder.RoleName;
                sendPricePo.QuantID = priceMatchParam.OldOrder.QuantID;
                //sendPricePo.QuoteID = _priceMatchParam.OldOrder.QuoteID;
                sendPricePo.StrategyOrderId = priceMatchParam.OldOrder.StrategyOrderId;
                sendPricePo.InstitutionID = priceMatchParam.OldOrder.InstitutionID;
                sendPricePo.InstitutionName = priceMatchParam.OldOrder.InstitutionName;
                sendPricePo.TraderID = priceMatchParam.OldOrder.TraderID;
                sendPricePo.TraderName = priceMatchParam.OldOrder.TraderName;
            }

            if (priceMatchParam != null)
            {
                sendPricePo.InValidFlag = priceMatchParam.InValidFlag;
                sendPricePo.TrigEvent = priceMatchParam.TrigEvent;
                sendPricePo.CancEvent = priceMatchParam.CancEvent;
                sendPricePo.EREventName = priceMatchParam.EREventName;
                sendPricePo.CancEventName = priceMatchParam.CancEventName;
            }

            if (LocalEnv.ConfInfo.IsAccountUser == "1")
            {
                sendPricePo.AccountID = sendPriceViewModel.SelectedItem;
                sendPricePo.AccountName = sendPriceViewModel.SelectedText;
            }

            //订阅标签
            //TagData tagData = detailConditionViewModel.GetTagData();
            //sendPricePo.OrgType = tagData.ClientType;
            //sendPricePo.VipCode = tagData.VipCode;
            sendPricePo.IfSubBond = tagData.IF_SUB_BOND;
            Dictionary<string, object> dic = new Dictionary<string, object>();
            if (!string.IsNullOrEmpty(tagData.IF_SUB_BOND))
                dic.Add("IF_SUB_BOND", tagData.IF_SUB_BOND);
            if (!string.IsNullOrEmpty(tagData.VIP))
                dic.Add("VIP", tagData.VIP);
            if (!string.IsNullOrEmpty(tagData.ORG_TYPE))
                dic.Add("ORG_TYPE", tagData.ORG_TYPE);

            string jsonInfo2 = JsonConvert.SerializeObject(dic);
            sendPricePo.Info2 = jsonInfo2;

            return sendPricePo;
        }

        /// <summary>
        /// 取2个日期的最小值,当为默认时
        /// </summary>
        /// <param name="selfUntilTime">撤单时间</param>
        /// <param name="validUntiTimeSetting">个人设置</param>
        /// <returns></returns>
        private string GetMinDate(string selfUntilTime, string validUntiTimeSetting = "", bool isDefault = true)
        {
            List<DateTime> LstTime = new List<DateTime>();
            DateTime cudatetime = DateTimeEx.ServerTimeNow;
            LogHandler.Info($"GetMinDate:selfUntilTime:{selfUntilTime} validUntiTimeSetting:{validUntiTimeSetting} isDefault:{isDefault.ToString()}" + " ServerTime:" + cudatetime.ToString("yyyy-MM-dd HH:mm:ss"));
            // 如果是时点, 失效时间<当前时间，则直接失效
            if (!isDefault)
            {
                if (cudatetime.CompareTo(DateTime.Parse(selfUntilTime)) >= 0)
                {
                    return selfUntilTime;
                }
            }

            // DateTime cudatetime = DateTime.Parse(DateTimeEx.ServerTime.ToString("yyyy-MM-dd") + " 08:35:00");
            if (isDefault)
            {
                LstTime.Add(DateTime.Parse(DateTimeEx.ServerTimeNow.ToString("yyyy-MM-dd") + " 06:00:00"));
                LstTime.Add(DateTime.Parse(DateTimeEx.ServerTimeNow.ToString("yyyy-MM-dd") + " 12:00:00"));
                LstTime.Add(DateTime.Parse(DateTimeEx.ServerTimeNow.ToString("yyyy-MM-dd") + " 16:30:00"));
            }

            LstTime.Add(cudatetime);
            LstTime.Add(DateTime.Parse(selfUntilTime));
            if (!string.IsNullOrEmpty(validUntiTimeSetting))
            {
                LstTime.Add(DateTime.Parse(validUntiTimeSetting));
            }

            List<DateTime> NewLstTime = LstTime.Distinct().ToList();
            NewLstTime.Sort();
            int i = NewLstTime.IndexOf(cudatetime);
            NewLstTime.RemoveRange(0, i);

            if (NewLstTime.Count == 1)
            {
                return string.Empty;
            }
            else
            {
                return NewLstTime[1].ToString("yyyy-MM-dd HH:mm:ss");
            }
        }

        /// <summary>
        /// 获得清算速度
        /// </summary>
        /// <returns></returns>
        private string GetSetttle(bool isfilter = false)
        {
            string settle = string.Empty;

            if (sendPriceViewModel.AddOneOpenedTag == "1")
            {
                if (sendPriceViewModel.SettleBoxIndex2 == 0)
                {
                    OCOViewModel ocoViewModel = sendPriceViewModel.getCrossViewModel().GetOCOViewModel();
                    CrossViewModel crossViewModel = sendPriceViewModel.getCrossViewModel();
                    string selectItem = crossViewModel.GetSelectItem();

                    if (selectItem.ToUpper().Contains("ODM"))
                    {
                        settle = "2";
                    }
                    else
                    {
                        settle = "2,3";
                    }
                }
                else if (sendPriceViewModel.SettleBoxIndex2 == 1)
                {
                    settle = "2";
                }
                else if (sendPriceViewModel.SettleBoxIndex2 == 2)
                {
                    settle = "3";
                }
            }
            else
            {
                if (sendPriceViewModel.SettleBoxIndex == 0)
                {
                    settle = "1";
                }
                else
                {
                    // 如果不是获取最优价
                    if (!isfilter)
                    {
                        if (sendPriceViewModel.FwdSelectDate != null && sendPriceViewModel.FwdSelectDate.HasValue)
                        {
                            if (sendPriceViewModel.GetSendPriceModelAdapter().IsSpotSettle(sendPriceViewModel.FwdSelectDate, sendPriceViewModel.FwdComboBoxSelectIndex))
                                settle = sendPriceViewModel.GetSendPriceModelAdapter().IsTomorrow(sendPriceViewModel.FwdSelectDate, sendPriceViewModel.FwdComboBoxSelectIndex) ? "2" : "1";//2:明天+0转T+1
                            else
                                settle = sendPriceViewModel.FwdSelectDate.Value.ToString("MM.dd") + (sendPriceViewModel.FwdComboBoxSelectIndex == 0 ? "+0" : "+1");
                        }
                    }
                    else
                    {
                        settle = "4"; //远期
                    }
                }
            }
            if (string.IsNullOrEmpty(settle))
                settle = "2,3";//默认+1

            return settle;
        }

        /// <summary>
        /// 更新订单（即先撤单后重下）
        /// </summary>
        private void UpdatePrice()
        {
            NewOrderSingle newOrder = GetNewOrderSingle();
            newOrder.Info5 = "转firm订单";
            OrdersHandler.Instance.ModifyOrders(newOrder, sendPriceViewModel.GetPriceMatchParam().OrderId, UpdatePriceCallBack);
        }

        /// <summary>
        /// 更新订单后的结果回调
        /// </summary>
        /// <param name="reply"></param>
        private void UpdatePriceCallBack(ModifyOrdersReply reply)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {

                if (reply.Code == "10000" || reply.IsSuccess())
                {
                    SendPriceWarnView warnView = new SendPriceWarnView(reply, GetNewOrderSingle());
                    warnView.Owner = Application.Current.MainWindow;
                    //warnView.OnAction += WarnView_OnAction;
                    //if (!reply.IsSuccess())
                    //{
                    //    //warnView.OnAction += WarnView_OnAction;
                    //    //warnView.Owner = myWindow;
                    //    warnView.ShowDialog();
                    //}
                    //else
                    warnView.Show();
                }
                else
                {
                    MessageBoxGUI msgBox = new MessageBoxGUI();
                    if (this.myWindow.IsActive)
                    {
                        msgBox.Owner = this.myWindow;
                    }
                    msgBox.setWarnMsg(reply.Msg);
                    msgBox.ShowDialog();

                }

                OnCancelClick();
            }));
        }



        /// <summary>
        /// 报价前检查参数设置
        /// </summary>
        /// <returns></returns>
        private bool CheckParam(SendPriceViewModel sendPriceViewModel)
        {
            bool result = true;
            decimal temD = 0;

            DetailConditionViewModel detailConditionViewModel = sendPriceViewModel.GetDetailConditionViewModel();

            if (detailConditionViewModel.HourValidTimeQuotationCheck)
            {
                // 时长
                // ValidMinute  ValidSecond
                if (string.IsNullOrEmpty(detailConditionViewModel.ValidMinute) && string.IsNullOrEmpty(detailConditionViewModel.ValidSecond))
                {
                    result = false;
                }

                int totalsecond = 0;
                if (!string.IsNullOrEmpty(detailConditionViewModel.ValidMinute))
                {
                    totalsecond = int.Parse(detailConditionViewModel.ValidMinute) * 60;
                }

                if (!string.IsNullOrEmpty(detailConditionViewModel.ValidSecond))
                {
                    totalsecond += int.Parse(detailConditionViewModel.ValidSecond);
                }

                if (totalsecond == 0)
                {
                    result = false;
                }
            }
            else if (detailConditionViewModel.TimeValidTimeQuotationCheck)
            {
                // 时点
                // ValidTimeHour ValidTimeMinute ValidTimeSecond
                int hour = 0;
                int min = 0;
                int sec = 0;

                if (!string.IsNullOrEmpty(detailConditionViewModel.ValidTimeHour))
                {
                    hour = int.Parse(detailConditionViewModel.ValidTimeHour);
                }

                if (!string.IsNullOrEmpty(detailConditionViewModel.ValidTimeMinute))
                {
                    min = int.Parse(detailConditionViewModel.ValidTimeMinute);
                }

                if (!string.IsNullOrEmpty(detailConditionViewModel.ValidTimeSecond))
                {
                    sec = int.Parse(detailConditionViewModel.ValidTimeSecond);
                }

                DateTime tmpdt = DateTime.Parse(DateTimeEx.ServerTimeNow.ToString("yyyy-MM-dd").ToString());
                tmpdt = tmpdt.AddHours(hour).AddMinutes(min).AddSeconds(sec);
                if (DateTimeEx.ServerTimeNow >= tmpdt)
                {
                    return false;
                }
            }
            OCOViewModel ocoViewModel = sendPriceViewModel.getCrossViewModel().GetOCOViewModel();
            CrossViewModel crossViewModel = sendPriceViewModel.getCrossViewModel();

            if (crossViewModel.CrossVisibility == Visibility.Collapsed && crossViewModel.OCOVisibility == Visibility.Collapsed && crossViewModel.SniperVisibility == Visibility.Collapsed)
            {
                result = false;
            }
            else if (ocoViewModel.CheckAllVisibility == Visibility.Collapsed && ocoViewModel.CheckInnerVisibility == Visibility.Collapsed && ocoViewModel.CheckXBondVisibility == Visibility.Collapsed)
                result = false;
            else if (string.IsNullOrEmpty(sendPriceViewModel.SendPriceItem.Security))
                result = false;
            else if ((!decimal.TryParse(sendPriceViewModel.CustomPrice, out temD) || temD == 0) || (sendPriceViewModel.CheckBondType() && string.IsNullOrEmpty(sendPriceViewModel.NetPrice))
                || (sendPriceViewModel.GetQtyViewModel().GetAmount() == 0))
            {
                if (!(ocoViewModel.CheckInnerVisibility == Visibility.Visible && ocoViewModel.CheckInner && sendPriceViewModel.FirmChecked))
                {
                    result = false;
                }
                else
                {
                    //Nonfirm支持OCO 仅内部时，发送无价、无量的报价
                }
            }
            //else if (sendPriceViewModel.CheckBondType() && string.IsNullOrEmpty(sendPriceViewModel.NetPrice))
            //    result = false;
            //else if (sendPriceViewModel.GetQtyViewModel().GetAmount() == 0)//(string.IsNullOrEmpty(sendPriceItem.Amount))
            //    result = false;
            else if (crossViewModel.SelectTabIndex == 1 && !ocoViewModel.CheckAll && !ocoViewModel.CheckInner && !ocoViewModel.CheckXBond)
                result = false;
            //else if (!CheckAll && !CheckInner && CheckXBond)
            //    result = false;

            else if (crossViewModel.SelectTabIndex == 1 && ocoViewModel.CheckAll)
            {
                if (!ocoViewModel.CheckNotOut)
                {
                    string str = sendPriceViewModel.getCrossViewModel().GetSelectItem();
                    if (string.IsNullOrEmpty(str))
                        result = false;
                }
            }
            else if (crossViewModel.SelectTabIndex == 1 && ocoViewModel.CheckAll && !ocoViewModel.CheckNotOut)
                result = false;







            if (crossViewModel.SelectTabIndex == 0)
            {
                string selectItem = crossViewModel.GetSelectItem();

                if (sendPriceViewModel.FirmChecked || sendPriceViewModel.WholeFlagChecked)
                {
                    string[] arr = selectItem.Split(new string[] { "-" }, StringSplitOptions.RemoveEmptyEntries);
                    if (arr != null && arr.Length > 0)
                    {
                        string selectVenueGwType = arr[1];
                        string venue = arr[0];

                        if (!(!string.IsNullOrEmpty(venue) && venue == "INNER"))
                        {
                            result = false;
                        }
                    }
                }

                if (string.IsNullOrEmpty(selectItem))
                    result = false;
                string settle = GetSetttle();
                if (selectItem.ToUpper().Contains("ODM") && !(settle.Equals("1") || settle.Equals("2")))
                {
                    result = false;
                }

                if (selectItem.ToUpper().Contains("ODM"))
                {
                    BondsInfoPo bondsInfoPo = CommonUtils.GetBondsInfoPoBySecurityID(sendPriceViewModel.SendPriceItem.Security);
                    if (bondsInfoPo.flag != "Y")
                        result = false;//非ODM债券
                }
            }
            else if (crossViewModel.SelectTabIndex == 1)
            {
                string selectItem = crossViewModel.GetSelectItem();
                string settle = GetSetttle();
                if (selectItem.ToUpper().Contains("ODM") && !(settle.Equals("1") || settle.Equals("2")))
                {
                    result = false;
                }
            }

            CalResult _lastCalResult = sendPriceViewModel.GetLastCalResult();
            string _lastReqData = sendPriceViewModel.GetLastReqData();
            if (_lastCalResult != null && _lastCalResult.Data != null && !string.IsNullOrEmpty(_lastCalResult.ReqData) && _lastCalResult.IsSuccess() && !string.IsNullOrEmpty(_lastReqData))
            {
                if (!_lastCalResult.ReqData.Equals(_lastReqData))
                {
                    result = false;
                }
            }


            if (crossViewModel.SelectTabIndex == 1)
            {
                // 检查个别情况的验证
                string venuesSelectF = string.Empty;
                if (!(ocoViewModel.CheckInner && ocoViewModel.CheckXBondVisibility == Visibility.Collapsed))
                    venuesSelectF = ocoViewModel.CheckNotOut ? "1" : "3";
                if (venuesSelectF == "3")
                {
                    string selfSelectVenues = sendPriceViewModel.getCrossViewModel().GetSelectItem();
                    if (string.IsNullOrEmpty(selfSelectVenues))
                    {
                        result = false;
                    }
                }
            }


            PriceCondition priceCondition = sendPriceViewModel.GetPriceCondition();
            if (priceCondition != null)
            {
                if (priceCondition.CounterpartyBestOffer == string.Empty)
                {
                    result = false;
                }
            }

            return result;
        }



        public bool CheckNonFirmOrder(List<NewOrderSingle> orderList)
        {
            bool result = true;

            foreach (NewOrderSingle newOrderSingle in orderList)
            {
                if (newOrderSingle.AlgoName == "STP")
                {
                    result = false;

                    break;
                }
            }

            if (!result)
            {
                BaseReplyPo reply = new BaseReplyPo();

                reply.Code = "10001";

                List<TPlaceOrderResult> resultList = new List<TPlaceOrderResult>();

                TPlaceOrderResult por = new TPlaceOrderResult();
                por.Code = "10003";
                por.DoubleWarn = false;

                por.Reason = "批量操作包含直通单，直通单不支持Nonfirm";
                
                resultList.Add(por);

                reply.Msg = MsgHelper.Serializer<List<TPlaceOrderResult>>(resultList);

                SendPriceWarnView warnView = new SendPriceWarnView(reply, GetNewOrderSingle());
                warnView.Owner = Application.Current.MainWindow;
                warnView.Show();
            }


            return result;
        }
    }
}
