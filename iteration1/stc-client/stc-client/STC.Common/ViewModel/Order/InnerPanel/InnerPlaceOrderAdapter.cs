using Ats.Foundation.Message;
using com.gateway.log;
using netapi.Api;
using Newtonsoft.Json;
using STC.Common.Data;
using STC.Common.Handler.SystemParam;
using STC.Common.Handler.Table;
using STC.Common.Model;
using STC.Common.Model.Base;
using STC.Common.Model.Cal;
using STC.Common.Model.Common;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.Table;
using STC.Common.Model.TBook;
using STC.Common.Utils;
using STC.Common.View;
using STC.Common.View.Component;
using STC.Common.View.UI;
using STC.Common.ViewModel.Order.InnerView;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace STC.Common.ViewModel.Order.InnerPanel
{
    public class InnerPlaceOrderAdapter
    {

        private static InnerPlaceOrderAdapter placeOrderHelper = new InnerPlaceOrderAdapter();

        public static InnerPlaceOrderAdapter Instance { get { return placeOrderHelper; } }

        private InnerPlaceOrderAdapter()
        { }

        private SendRequireViewModel sendPriceViewModel;
        private Window myWindow;


        public void PlaceOrder(SendRequireViewModel sendPriceViewModel, Window myWindow)
        {
            RequireParseViewModel requireParseViewModel = sendPriceViewModel.GetRequireParseViewModel();
            if (LocalEnv.ConfInfo.RequiredIns == "1" && LocalEnv.UserInfo.RoleType != RoleType.BUYER_ROLE && requireParseViewModel != null && requireParseViewModel.SelectedInstintuion_Edit == null)
            {
                MessageBoxGUI msgBox = new MessageBoxGUI();
                msgBox.setWarnMsg("请填写来源方机构");
                msgBox.Owner = myWindow;
                msgBox.ShowDialog();
                return;
            }


            this.sendPriceViewModel = sendPriceViewModel;
            this.myWindow = myWindow;

            if (ApiManager.Instance.GetDefaultApi().EnabledMsgLog == "1")
            {
                LogHandler.Info("点击发布需求报价按钮");
            }
            sendPriceViewModel.SendPriceEnable = false;
            PriceMatchParam priceMatchParam = sendPriceViewModel.GetPriceMatchParam();
            if (CheckParam(sendPriceViewModel))
            {
                if (priceMatchParam != null && priceMatchParam.IsIntention && priceMatchParam.status != "1")
                {
                    if (LocalEnv.UserInfo.RoleType == RoleType.BUYER_ROLE && LocalEnv.ConfInfo.IsPAYL)
                    {
                        
                        if (requireParseViewModel.SelectTraderItem == null)
                        {
                            BaseReplyPo reply = new BaseReplyPo();
                            reply.Code = "10001";
                            List<TPlaceOrderResult> result = new List<TPlaceOrderResult>();
                            TPlaceOrderResult por = new TPlaceOrderResult();
                            por.Code = "10004";
                            por.DoubleWarn = false;
                            por.Reason = "交易员不能为空";
                            result.Add(por);
                            reply.Msg = MsgHelper.Serializer<List<TPlaceOrderResult>>(result);

                            sendPriceViewModel.SendPriceEnable = true;
                            SendPriceWarnView warnView = new SendPriceWarnView(reply, GetNewOrderSingle());
                            warnView.Owner = myWindow;
                            warnView.Show();

                            return;
                        }
                    }

                    // 处理意向单
                    // 新建
                    if (string.IsNullOrEmpty(priceMatchParam.IntentionID))
                    {
                        PlaceInstertQuoteRequest();
                    }
                    // 更新
                    else
                    {
                        UpdateIntentionOrder();
                    }
                }
                else
                {
                    if (priceMatchParam != null && priceMatchParam.UpdataPrice == false && priceMatchParam.IntentionID != null)
                    {
                        // 恢复报价
                        ConfirmIntentionOrder();
                    }
                    else
                    {
                        //PriceMatchParam priceMatchParam = sendPriceViewModel.GetPriceMatchParam();
                        //if (priceMatchParam != null && priceMatchParam.UpdataPrice)
                        //    UpdatePrice();
                        //else
                        PlaceOrder();
                    }
                }
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

            TagConditionViewModel detailConditionViewModel = sendPriceViewModel.GetDetailConditionViewModel();

            if (detailConditionViewModel.HourValidTimeQuotationCheck)
            {
                // 时长
                // ValidMinute  ValidSecond
                if (string.IsNullOrEmpty(detailConditionViewModel.ValidMinute) && string.IsNullOrEmpty(detailConditionViewModel.ValidSecond))
                {
                    por.Reason = "有效时长需大于0秒";
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
                    por.Reason = "有效时长需大于0秒";
                }
            }
            else if (detailConditionViewModel.TimeValidTimeQuotationCheck)
            {
                // 时点
                // ValidTimeHour ValidTimeMinute ValidTimeSecond
                if (string.IsNullOrEmpty(detailConditionViewModel.ValidTimeHour))
                {
                    por.Reason = "必须输入时";
                }

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
                    por.Reason = "订单撤单时点不能在报单时间之前";
                }
            }



            if (string.IsNullOrEmpty(sendPriceViewModel.SendPriceItem.Security) || sendPriceViewModel.SendPriceItem.Security == "Tradebook")
                por.Reason = "未选择债券";

            else if (string.IsNullOrEmpty(sendPriceViewModel.NetPrice))
            {
                PriceMatchParam priceMatchParam = sendPriceViewModel.GetPriceMatchParam();
                if (priceMatchParam != null && priceMatchParam.IsIntention && priceMatchParam.status != "1")
                {
                }
                else
                {
                    if (priceMatchParam != null && priceMatchParam.UpdataPrice == false && priceMatchParam.IntentionID != null)
                    {

                    }
                    else
                    {
                        if (sendPriceViewModel.GetCalBondErrMsg().Length > 0)
                        {
                            por.Reason = $"{sendPriceViewModel.GetCalBondErrMsg()}, 调用计算器服务失败,请重新打开窗口或修改报价参数";
                        }
                        else
                        {
                            if (sendPriceViewModel.GetLastCalResult() != null)
                            {
                                por.Reason = $"{sendPriceViewModel.GetLastCalResult().Msg}";
                            }
                            else
                            {
                                por.Reason = $"净价为空";
                            }
                        }
                    }
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

            PriceCondition priceCondition = detailConditionViewModel.GetPriceCondition();
            if (priceCondition != null)
            {
                if (priceCondition.CounterpartyBestOffer == string.Empty)
                {
                    por.Reason = "条件撤销已勾选，必须输入bp";
                }
            }

            if (LocalEnv.UserInfo.RoleType == RoleType.BUYER_ROLE && LocalEnv.ConfInfo.IsPAYL)
            {
                PriceMatchParam priceMatchParam = sendPriceViewModel.GetPriceMatchParam();
                long qty = 0;
                if (sendPriceViewModel.GetQtyViewModel() != null && sendPriceViewModel.GetQtyViewModel().GetAmount() != 0)
                    qty = sendPriceViewModel.GetQtyViewModel().TrueAmount.ConvertToLong();
                if (priceMatchParam != null && priceMatchParam.MatchedQty.HasValue && priceMatchParam.MatchedQty.Value > qty)
                {
                    por.Reason = "个需的量不能修改为小于已匹配成交量";
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

            try
            {
                sendPricePo.HandlInst = "2";
                RecognizeIntentionQuoteReplyPo result = IntentionOldHandler.Instance.PublishIntention(sendPricePo);

                try
                {

                    if (result != null)
                    {
                        if (result.Code == "10000" || result.IsSuccess())
                        {
                            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                            {
                                SendPriceWarnView warnView = new SendPriceWarnView(result, sendPricePo);
                                warnView.OnAction += WarnView_OnAction;
                                if (!result.IsSuccess())
                                {
                                    warnView.ShowDialog();
                                }
                                else
                                {
                                    RemenberInstAndTrade(sendPricePo.TraderID, sendPricePo.InstitutionID);
                                    warnView.Owner = Application.Current.MainWindow;
                                    warnView.ShowInTaskbar = false;
                                    warnView.Show();
                                }
                            }));

                            if (result.IsSuccess())
                            {
                                if (!string.IsNullOrEmpty(sendPricePo.IntentionID))
                                {
                                    //作废区恢复历史记录成功后发送消息，删除作废区的订单
                                    STC.Common.Handler.Base.BaseHandler.Instance.PublishAppMsg(new STC.Common.Model.Fix.Message.PResult() { Code = "ResumeInnerHistoryRequire", RequestID = $"{sendPricePo.IntentionID}" });
                                }
                            }
                        }
                        else
                        {
                            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                            {
                                MessageBoxGUI msgBox = new MessageBoxGUI();
                                msgBox.setWarnMsg(result.Msg);
                                msgBox.Owner = myWindow;
                                msgBox.ShowDialog();
                                OnCancelClick();
                            }));

                        }
                    }
                }
                catch (Exception e)
                {
                    LogHelper.Error(e);

                    OnCancelClick();
                }

            }
            catch (Exception e)
            {
                LogHelper.Error(e);
            }

        }

        private void RemenberInstAndTrade(string traderId, string instituionId)
        {
            if (!string.IsNullOrEmpty(traderId))
            {
                Common.Utils.CommonUtils.AddContract(traderId);
                Common.Utils.CommonUtils.SaveContract();
            }
            if (!string.IsNullOrEmpty(instituionId))
            {
                Common.Utils.CommonUtils.AddInst(instituionId);
                Common.Utils.CommonUtils.SaveInst();
            }
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
            
            sendPricePo.AlgoName = "CROSS";
            sendPricePo.VenueTypeGW = "INNER";
            sendPricePo.Venues = "MANUAL";

            sendPricePo.ClOrdID = DataTimeSequence.Instance.GetId();
            PriceMatchParam priceMatchParam = sendPriceViewModel.GetPriceMatchParam();
            if (priceMatchParam != null)
            {
                sendPricePo.Info1 = priceMatchParam.Info1;
                sendPricePo.Info2 = priceMatchParam.Info2;
                sendPricePo.Info3 = priceMatchParam.Info3;
                sendPricePo.Info4 = priceMatchParam.Info4;
                sendPricePo.Info5 = priceMatchParam.Info5;
                sendPricePo.IntentionID = priceMatchParam.IntentionID;
            }

            //sendPricePo.MarketIndicator = "4";
            BondsInfoPo bondsInfoPo = sendPriceViewModel.GetBondsInfoPo();
            if (bondsInfoPo != null)
                sendPricePo.MarketIndicator = bondsInfoPo.MarketIndicator;
            else
                sendPricePo.MarketIndicator = "4";


            sendPricePo.OrdType = "2";

            if (sendPriceViewModel.GetQtyViewModel() != null && sendPriceViewModel.GetQtyViewModel().GetAmount() != 0)
                sendPricePo.OrderQty = sendPriceViewModel.GetQtyViewModel().TrueAmount.ToString();

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
            //选中：0(nonfirm)，未选中：1(firm)
            sendPricePo.FirmFlag = sendPriceViewModel.FirmChecked ? "0" : "1";


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

            TagConditionViewModel tagconditionviewmodel = sendPriceViewModel.GetDetailConditionViewModel();

            //识别
            RequireParseViewModel requireParseViewModel = sendPriceViewModel.GetRequireParseViewModel();
            //if (IntentionOldHandler.Instance.isSALER())
            {
                sendPricePo.InstitutionID = requireParseViewModel.SelectedInstintuion_Edit == null ? string.Empty : requireParseViewModel.SelectedInstintuion_Edit.idbkey;
                //sendPricePo.InstitutionName = requireParseViewModel.SelectedInstintuion_Edit == null ? string.Empty : requireParseViewModel.SelectedInstintuion_Edit.shortnamecn;
                sendPricePo.InstitutionName = requireParseViewModel.InstitutionName_Edit;
                sendPricePo.TraderID = requireParseViewModel.SelectedTrader_Edit == null ? string.Empty : requireParseViewModel.SelectedTrader_Edit.idbkey;
                //sendPricePo.TraderName = requireParseViewModel.SelectedTrader_Edit == null ? string.Empty : requireParseViewModel.SelectedTrader_Edit.namecn;
                sendPricePo.TraderName = requireParseViewModel.TraderName_Edit;
            }

            sendPricePo.Text = requireParseViewModel.BondTxt;

            if (!string.IsNullOrEmpty(sendPriceViewModel.PriceType))
            {
                sendPricePo.PriceType = sendPriceViewModel.PriceType;
            }
            else
            {
                sendPricePo.PriceType = requireParseViewModel.PriceType;
            }
            if (string.IsNullOrEmpty(sendPricePo.PriceType))
            {
                sendPricePo.PriceType = "3"; //收益率
            }
            //if (sendPricePo.MarketIndicator == "4")
            //    sendPricePo.NetPrice = sendPriceViewModel.NetPrice;
            //else
            //    sendPricePo.NetPrice = "100";
            sendPricePo.NetPrice = sendPriceViewModel.NetPrice;
            sendPricePo.Price = sendPriceViewModel.CustomPrice;

            //标签
            TagData tagData = tagconditionviewmodel.GetTagData();
            sendPricePo.IfSubBond = tagData.IF_SUB_BOND;

            TagData clientTagData = requireParseViewModel.GetTagData();

            sendPricePo.OrgType = clientTagData.ORG_TYPE;
            sendPricePo.VipCode = clientTagData.VIP;

            sendPricePo.UserName = string.IsNullOrEmpty(requireParseViewModel.GetBrokerName()) ? LocalEnv.UserInfo.UserName : requireParseViewModel.GetBrokerName(); //被代操作员
            sendPricePo.Name = LocalEnv.UserInfo.UserName; //放当前操作员

            //撤单时间
            string info1 = tagconditionviewmodel.GetValidTimeType();

            //撤单条件
            PriceCondition priceCondition = tagconditionviewmodel.GetPriceCondition();
            if (priceCondition != null)
            {
                sendPricePo.PriceCondition = JsonConvert.SerializeObject(priceCondition);
            }

            //默认 "0|" : 时长 "1|{min}:{sec}" : 时点  "2|{hour}:{min}:{sec}" 无 "3|"
            string validtimetype = tagconditionviewmodel.GetValidTimeType();
            //yyyy-MM-dd HH:mm:ss
            string validuntiltime = tagconditionviewmodel.GetValidUntilTime();

            string quotetype = "require";
            if (priceMatchParam != null)
            {
                if (priceMatchParam.IntentionID != null)
                {
                    sendPricePo.IntentionID = priceMatchParam.IntentionID;
                    sendPricePo.Info4 = priceMatchParam.IntentionID;
                    //if (!sendPriceViewModel._isUpdatePrice)
                    //{
                    //    if (!string.IsNullOrEmpty(priceMatchParam.IntentionID))
                    //    {
                    //        //如果是恢复订单，需要传类型,修改时不要传
                    //        quotetype = "unrefrequire";
                    //        sendPricePo.QuoteType = quotetype;
                    //    }
                    //}
                }
            }

            OrderTimeModel otm = new OrderTimeModel() { ValidTimeType = info1, ValidUntilTime = validuntiltime, IF_SUB_BOND = sendPricePo.IfSubBond ?? string.Empty, VIP = sendPricePo.VipCode ?? string.Empty, ORG_TYPE = sendPricePo.OrgType ?? string.Empty, MorethanMarketBPCheck = priceCondition.MorethanMarketBPCheck ?? string.Empty, MorethanMarketBP = priceCondition.MorethanMarketBP ?? string.Empty, Text = sendPricePo.Text, QuoteType = quotetype, UserName = sendPricePo.UserName };

            sendPricePo.Info1 = JsonConvert.SerializeObject(otm);
            sendPricePo.ValidUntilTime = validuntiltime;

            sendPricePo.OCOFlag = tagconditionviewmodel.OcoFlag;

            TagData info2tag = null;
            if (priceMatchParam != null && JsonSplit.IsJson(priceMatchParam.Info2))
            {
                info2tag = JsonConvert.DeserializeObject<TagData>(priceMatchParam.Info2);
            }
            else
            {
                info2tag = new TagData();
            }
            info2tag.IF_SUB_BOND = otm.IF_SUB_BOND;
            info2tag.ORG_TYPE = otm.ORG_TYPE;
            info2tag.VIP = otm.VIP;
            info2tag.HURRY_FLAG = tagconditionviewmodel.HurryFlag;
            info2tag.SUGGEST_FLAG = tagconditionviewmodel.SuggestFlag;
            info2tag.FIRE_FLAG = tagconditionviewmodel.FireFlag;
            info2tag.FLOOR_PRICE = tagconditionviewmodel.FloorPrice;
            if (sendPricePo.PriceType != "1" && sendPricePo.PriceType != "2")
            {
                info2tag.CUSTOMER_PRICE = sendPriceViewModel.CustomerPrice.HasValue ? sendPriceViewModel.CustomerPrice.ToString() : string.Empty;
                info2tag.CUSTOMER_SPREAD = sendPriceViewModel.BP.HasValue ? sendPriceViewModel.BP.ToString() : string.Empty;
            }
            else
            {
                info2tag.CUSTOMER_PRICE = null;
                info2tag.CUSTOMER_SPREAD = null;
            }
            info2tag.PUBLIC_REMARKS = tagconditionviewmodel.PublicRemarks;
            info2tag.PERSONAL_REMARKS = tagconditionviewmodel.PersonalRemarks;
            info2tag.OCO_FLAG = tagconditionviewmodel.OcoFlag;

            sendPricePo.Info2 = JsonConvert.SerializeObject(info2tag);

            if (priceMatchParam != null)
            {
                sendPricePo.Info3 = priceMatchParam.Info3;
                sendPricePo.Info4 = priceMatchParam.Info4;
            }

            if (sendPriceViewModel.GetPriceMatchParam() != null && !string.IsNullOrEmpty(sendPriceViewModel.GetPriceMatchParam().IntentionID))
                sendPricePo.StrategyOrderId = sendPriceViewModel.GetPriceMatchParam().OrderId;//修改发布需求

            sendPricePo.Symbol = sendPriceViewModel.GetPriceMatchParam()==null?string.Empty: sendPriceViewModel.GetPriceMatchParam().SymbolName;

            if (string.IsNullOrEmpty(sendPricePo.Symbol) && !string.IsNullOrEmpty(sendPriceViewModel.ShowText))
            {
                string[] arr = sendPriceViewModel.ShowText.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                if (arr != null && arr.Length > 1)
                {
                    sendPricePo.Symbol = arr[1];
                }
            }

            if (LocalEnv.ConfInfo.IsAccountUser == "1")
            {
                sendPricePo.AccountID = sendPriceViewModel.SelectedItem;
                sendPricePo.AccountName = sendPriceViewModel.SelectedText;
            }

            sendPricePo.Channel = (priceMatchParam == null || string.IsNullOrEmpty(priceMatchParam.channel)) ? "CLIENT" : priceMatchParam.channel;
            if (priceMatchParam != null && !string.IsNullOrEmpty(priceMatchParam.QuoteType))
            {
                sendPricePo.QuoteType = priceMatchParam.QuoteType;
            }
           
            //sendPricePo.HurryFlag = tagconditionviewmodel.HurryFlag;
            //sendPricePo.HurryFlag = tagconditionviewmodel.HurryFlag;
            //sendPricePo.SuggestFlag = tagconditionviewmodel.SuggestFlag;
            //sendPricePo.FireFlag = tagconditionviewmodel.FireFlag;
            //sendPricePo.FloorPrice = tagconditionviewmodel.FloorPrice;
            //sendPricePo.OCOFlag = tagconditionviewmodel.OcoFlag;
            //sendPricePo.PublicRemarks = tagconditionviewmodel.PublicRemarks;
            //sendPricePo.PersonalRemarks = tagconditionviewmodel.PersonalRemarks;
            //request.customerPrice = info.customerPrice.Value;
            //request.customerSpread = info.customerSpread.Value;

            //string s = JsonConvert.SerializeObject(sendPricePo);
            return sendPricePo;
        }

        /// <summary>
        /// 取2个日期的最小值,当为默认时
        /// </summary>
        /// <param name="selfUntilTime">撤单时间</param>
        /// <param name="validUntiTimeSetting">个人设置</param>
        /// <returns></returns>
        public string GetMinDate(string selfUntilTime, string validUntiTimeSetting = "", bool isDefault = true)
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

            // DateTime cudatetime = DateTime.Parse(DateTimeEx.ServerTimeNow.ToString("yyyy-MM-dd") + " 08:35:00");
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
                if (sendPriceViewModel.SettleBoxIndex2 == 1)
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
        private bool CheckParam(SendRequireViewModel sendPriceViewModel)
        {
            bool result = true;
            decimal temD = 0;

            TagConditionViewModel detailConditionViewModel = sendPriceViewModel.GetDetailConditionViewModel();

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

            if (string.IsNullOrEmpty(sendPriceViewModel.SendPriceItem.Security) || sendPriceViewModel.SendPriceItem.Security == "Tradebook")
                result = false;
            else if (!decimal.TryParse(sendPriceViewModel.CustomPrice, out temD) || temD == 0)//(string.IsNullOrEmpty(CustomPrice))
            {
                if (!sendPriceViewModel.FirmChecked)
                    result = false;
            }
            else if (sendPriceViewModel.CheckBondType() && string.IsNullOrEmpty(sendPriceViewModel.NetPrice))
            {
                PriceMatchParam priceMatchParam = sendPriceViewModel.GetPriceMatchParam();
                if (priceMatchParam != null && priceMatchParam.IsIntention && priceMatchParam.status != "1")
                {
                }
                else
                {
                    if (priceMatchParam != null && priceMatchParam.UpdataPrice == false && priceMatchParam.IntentionID != null)
                    {

                    }
                    else
                    {
                        result = false;
                    }
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

            PriceCondition priceCondition = detailConditionViewModel.GetPriceCondition();
            if (priceCondition != null)
            {
                if (priceCondition.CounterpartyBestOffer == string.Empty)
                {
                    result = false;
                }
            }

            if (LocalEnv.UserInfo.RoleType == RoleType.BUYER_ROLE && LocalEnv.ConfInfo.IsPAYL)
            {
                PriceMatchParam priceMatchParam = sendPriceViewModel.GetPriceMatchParam();
                long qty = 0;
                if (sendPriceViewModel.GetQtyViewModel() != null && sendPriceViewModel.GetQtyViewModel().GetAmount() != 0)
                    qty = sendPriceViewModel.GetQtyViewModel().TrueAmount.ConvertToLong();
                if (priceMatchParam != null && priceMatchParam.MatchedQty.HasValue && priceMatchParam.MatchedQty.Value > qty)
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

        #region 保存需求单
        /// <summary>
        /// 生成订单报文, 这里应该拿改后的值
        /// ， 界面上控制哪些可以修改， 哪些不能修改
        /// 撤单条件，订阅 是否传值， 
        /// </summary>
        /// <returns></returns>
        private STC.Common.Model.Table.WorkBrench.UpdateIntentionQuoteRequest GetIntentionOrder()
        {
            STC.Common.Model.Table.WorkBrench.UpdateIntentionQuoteRequest request = new STC.Common.Model.Table.WorkBrench.UpdateIntentionQuoteRequest();
            PriceMatchParam priceMatchParam = sendPriceViewModel.GetPriceMatchParam();
            request.intentionId = priceMatchParam.IntentionID;
            RequireParseViewModel requireParseViewModel = sendPriceViewModel.GetRequireParseViewModel();
            TagConditionViewModel tagconditionviewmodel = sendPriceViewModel.GetDetailConditionViewModel();
            request.securityid = sendPriceViewModel.SendPriceItem.Security;
            if (!string.IsNullOrEmpty(sendPriceViewModel.PriceType))
            {
                request.priceType = sendPriceViewModel.PriceType;
            }
           
            if (LocalEnv.ConfInfo.IsPAYL)
            {
                request.relatedUsers = sendPriceViewModel.RelatedUsers;
            }
            if (LocalEnv.UserInfo.RoleType == RoleType.BUYER_ROLE)
            {
                request.counterPartyID = request.institutionId = LocalEnv.UserInfo.Termids;
                request.counterPartyName = request.institutionName = LocalEnv.UserInfo.biztype;

                string managerid = null;
                string managerName = null;
                string traderid = null;
                string traderName = null;

                if (requireParseViewModel.SelectTraderItem != null)
                {
                    traderid = requireParseViewModel.SelectTraderItem.Username;
                    traderName = requireParseViewModel.SelectTraderItem.Name;
                }
                //else
                //{
                //    traderid = requireParseViewModel.SelectTraderText;
                //    traderName = requireParseViewModel.SelectTraderText;
                //}

                if (requireParseViewModel.SelectInvestorItem != null)
                {
                    managerid = requireParseViewModel.SelectInvestorItem.Username;
                    managerName = requireParseViewModel.SelectInvestorItem.Name;
                }
                //else
                //{
                //    managerid = requireParseViewModel.SelectInvestorText;
                //    managerName = requireParseViewModel.SelectInvestorText;
                //}

                if (CommonUtils.IsInvestor())
                {
                    if (string.IsNullOrEmpty(managerid))
                    {
                        managerid = LocalEnv.UserInfo.UserName;
                        managerName = LocalEnv.UserInfo.FullName;
                    }
                }
                else
                {
                    if (string.IsNullOrEmpty(traderid))
                    {
                        traderid = LocalEnv.UserInfo.UserName;
                        traderName = LocalEnv.UserInfo.FullName;
                    }
                }

                if (priceMatchParam.roletype == "0011")
                {
                    request.counterPartySubID = traderid;
                    request.counterPartySubName = traderName;
                }
                else
                {
                    request.counterPartySubID = managerid;
                    request.counterPartySubName = managerName;
                }

                request.partyManager = managerid;
                request.partyManagerName = managerName;
                request.partyTrader = traderid;
                request.partyTraderName = traderName;

                NoteData info5tag = null;
                if (priceMatchParam != null && JsonSplit.IsJson(priceMatchParam.Info5))
                {
                    info5tag = JsonConvert.DeserializeObject<NoteData>(priceMatchParam.Info5);
                }
                else
                {
                    info5tag = new NoteData();
                }

                info5tag.PUBLIC_REMARKS = tagconditionviewmodel.PublicRemarks;
                if (info5tag.PERSONAL_REMARKS == null)
                {
                    info5tag.PERSONAL_REMARKS = new Dictionary<string, string>();
                }
                info5tag.PERSONAL_REMARKS[LocalEnv.UserInfo.UserName] = tagconditionviewmodel.PersonalRemarks;
                info5tag.COMMAND_NOTES = tagconditionviewmodel.CommandNotes;

                request.info5 = JsonConvert.SerializeObject(info5tag);

            }
            else
            {
                request.institutionId = requireParseViewModel.SelectedInstintuion_Edit == null ? string.Empty : requireParseViewModel.SelectedInstintuion_Edit.idbkey;
                request.institutionName = requireParseViewModel.InstitutionName_Edit;
                request.traderId = requireParseViewModel.SelectedTrader_Edit == null ? string.Empty : requireParseViewModel.SelectedTrader_Edit.idbkey;
                request.traderName = requireParseViewModel.TraderName_Edit;
            }
            request.side = sendPriceViewModel.IsBid ? "1" : "2";

            if (!string.IsNullOrEmpty(sendPriceViewModel.CustomPrice))
                request.price = sendPriceViewModel.CustomPrice.ConvertToDecimal();

            if (!string.IsNullOrEmpty(sendPriceViewModel.NetPrice))
                request.cleanPrice = sendPriceViewModel.NetPrice.ConvertToDecimal();

            if (sendPriceViewModel.GetQtyViewModel() != null && sendPriceViewModel.GetQtyViewModel().GetAmount() != 0)
                request.qty = sendPriceViewModel.GetQtyViewModel().TrueAmount;

            if (sendPriceViewModel.SettleBoxIndex == 1)
            {
                request.settlType = sendPriceViewModel.SendPriceItem.SettleType;
            }
            else
                request.settlType = GetSetttle();

            if (!sendPriceViewModel.WholeFlagChecked)
                request.wholeFlag = "0";
            else
                request.wholeFlag = "1";
            
            request.closeBy = LocalEnv.UserInfo.UserName;
            if (LocalEnv.UserInfo.RoleType == RoleType.BUYER_ROLE)
            {
                request.userName = priceMatchParam.UserName;
            }
            else
            {
                request.userName = string.IsNullOrEmpty(requireParseViewModel.GetBrokerName()) ? LocalEnv.UserInfo.UserName : requireParseViewModel.GetBrokerName();
            }
            
            //record.qqNumber = item.QqNumber;
            //record.createTime = item.CreateTime;
            //record.updateTime = item.UpdateTime;
            //record.fullPrice = item.FullPrice;

            //// 不支持投组账户
            //if (LocalEnv.ConfInfo.IsAccountUser == "1")
            //{
            //    if (sendPriceViewModel.AccountUserItem != null)
            //    {
            //        request.accountID = sendPriceViewModel.AccountUserItem.Id;
            //        request.accountName = sendPriceViewModel.AccountUserItem.AccountName;
            //    }
            //}

            //标签
            
            TagData tagData = tagconditionviewmodel.GetTagData();
            TagData clientTagData = requireParseViewModel.GetTagData();

            request.closeBy = LocalEnv.UserInfo.UserName;
            request.closeByName = LocalEnv.UserInfo.FullName;

            //撤单时间
            string info1 = tagconditionviewmodel.GetValidTimeType();

            ////撤单条件，置灰
            PriceCondition priceCondition = tagconditionviewmodel.GetPriceCondition();
            //if (priceCondition != null)
            //{
            //    record.PriceCondition = JsonConvert.SerializeObject(priceCondition);
            //}

            //默认 "0|" : 时长 "1|{min}:{sec}" : 时点  "2|{hour}:{min}:{sec}" 无 "3|"
            string validtimetype = tagconditionviewmodel.GetValidTimeType();
            //yyyy-MM-dd HH:mm:ss
            string validuntiltime = tagconditionviewmodel.GetValidUntilTime();

            // 不支持条件单，没有订阅
            string quotetype = "require";
            OrderTimeModel otm = null;
            if (priceMatchParam != null && JsonSplit.IsJson(priceMatchParam.Info1))
            {
                otm = JsonConvert.DeserializeObject<OrderTimeModel>(priceMatchParam.Info1);
                otm.IF_SUB_BOND = tagData.IF_SUB_BOND ?? string.Empty;
            }
            else
            {
                otm = new OrderTimeModel() { ValidTimeType = info1, ValidUntilTime = validuntiltime, IF_SUB_BOND = tagData.IF_SUB_BOND ?? string.Empty, VIP = clientTagData.VIP ?? string.Empty, ORG_TYPE = clientTagData.ORG_TYPE ?? string.Empty, MorethanMarketBPCheck = priceCondition.MorethanMarketBPCheck ?? string.Empty, MorethanMarketBP = priceCondition.MorethanMarketBP ?? string.Empty, Text = requireParseViewModel.BondTxt, QuoteType = quotetype, UserName = request.userName };
            }
           
            //request.validuntitime = validuntiltime;

            request.hurryFlag = tagconditionviewmodel.HurryFlag;
            request.suggestFlag = tagconditionviewmodel.SuggestFlag;
            request.fireFlag = tagconditionviewmodel.FireFlag;
            request.floorPrice = tagconditionviewmodel.FloorPrice;
            request.OcoFlag = tagconditionviewmodel.OcoFlag;
            request.publicRemarks = tagconditionviewmodel.PublicRemarks;
            request.personalRemarks = tagconditionviewmodel.PersonalRemarks;
            if (request.priceType != "1" && request.priceType != "2")
            {
                request.customerPrice = sendPriceViewModel.CustomerPrice;
                request.customerSpread = sendPriceViewModel.BP;
            }
            else
            {
                request.customerPrice = null;
                request.customerSpread = null;
            }

            TagData info2tag = null;
            if (priceMatchParam != null && JsonSplit.IsJson(priceMatchParam.Info2))
            {
                info2tag = JsonConvert.DeserializeObject<TagData>(priceMatchParam.Info2);
            }
            else
            {
                info2tag = new TagData();
            }

            info2tag.IF_SUB_BOND = tagData.IF_SUB_BOND ?? string.Empty;
            info2tag.ORG_TYPE = clientTagData.ORG_TYPE ?? string.Empty;
            info2tag.VIP = clientTagData.VIP ?? string.Empty;
            info2tag.HURRY_FLAG = tagconditionviewmodel.HurryFlag;
            info2tag.SUGGEST_FLAG = tagconditionviewmodel.SuggestFlag;
            info2tag.FIRE_FLAG = tagconditionviewmodel.FireFlag;
            info2tag.FLOOR_PRICE = tagconditionviewmodel.FloorPrice;
            if (request.priceType != "1" && request.priceType != "2")
            {
                info2tag.CUSTOMER_PRICE = sendPriceViewModel.CustomerPrice.HasValue ? sendPriceViewModel.CustomerPrice.ToString() : string.Empty;
                info2tag.CUSTOMER_SPREAD = sendPriceViewModel.BP.HasValue ? sendPriceViewModel.BP.ToString() : string.Empty;
            }
            else
            {
                info2tag.CUSTOMER_PRICE = null;
                info2tag.CUSTOMER_SPREAD = null;
            }
            info2tag.PUBLIC_REMARKS = tagconditionviewmodel.PublicRemarks;
            info2tag.PERSONAL_REMARKS = tagconditionviewmodel.PersonalRemarks;
            info2tag.OCO_FLAG = tagconditionviewmodel.OcoFlag;

            request.info2 = JsonConvert.SerializeObject(info2tag);
            request.VipCode = info2tag.VIP;
            request.OrgType = info2tag.ORG_TYPE;
            request.IfSubBond = info2tag.IF_SUB_BOND;
            request.channel = (priceMatchParam == null || string.IsNullOrEmpty(priceMatchParam.channel)) ? "CLIENT" : priceMatchParam.channel;
            if (priceMatchParam != null && !string.IsNullOrEmpty(priceMatchParam.QuoteType))
            {
                request.QuoteType = priceMatchParam.QuoteType;
            }
            if (LocalEnv.ConfInfo.IsPAYL)
            {
                // 持仓组合id
                string bookid = string.Empty;
                if (sendPriceViewModel.SelectedGroupItem != null && sendPriceViewModel.SelectedGroupItem.Count > 0)
                {
                    bookid = sendPriceViewModel.SelectedGroupItem[0];
                }
                request.bookId = bookid;
                //持仓组合名称
                string bookName = string.Empty;
                if (sendPriceViewModel.SelectedGroupText != null && sendPriceViewModel.SelectedGroupText.Count > 0)
                {
                    bookName = sendPriceViewModel.SelectedGroupText[0];
                }
                request.bookName = bookName;
                // 持仓组合账号id
                string accountid = string.Empty;
                if (!string.IsNullOrEmpty(sendPriceViewModel.SelectedTradeAccountCode))
                {
                    accountid = sendPriceViewModel.SelectedTradeAccountCode;
                }
                request.accountID = accountid;
                // 持仓组合账号名称
                string accountName = string.Empty;
                if (!string.IsNullOrEmpty(sendPriceViewModel.SelectedTradeAccountName))
                {
                    accountName = sendPriceViewModel.SelectedTradeAccountName;
                }
                request.accountName = accountName;
                // 恒生基金编号
                string hsProductId = string.Empty;
                if (!string.IsNullOrEmpty(sendPriceViewModel.SelectedProductId))
                {
                    hsProductId = sendPriceViewModel.SelectedProductId;
                }
                otm.HsProductId = hsProductId;
                request.inf1 = JsonConvert.SerializeObject(otm);

                if (CommonUtils.IsInvestor())
                {
                    request.actionFlag = "0";
                }
                else
                {
                    request.actionFlag = "2";
                }

                request.riskManualCk = "1";
            }
            return request;
        }


        /// <summary>
        /// 修改需求单
        /// </summary>
        private void UpdateIntentionOrder()
        {
            STC.Common.Model.Table.WorkBrench.UpdateIntentionQuoteRequest request = GetIntentionOrder();
            sendPriceViewModel.SendPriceEnable = false;

            if (LocalEnv.UserInfo.RoleType == RoleType.BUYER_ROLE)
            {
                STC.Common.Model.Table.WorkBrench.IntentionQuoteInfo sameData = IntentionOldHandler.Instance.GetSameIntentionInfo(request.partyManager, request.partyTrader, request.securityid, request.side, request.accountID);
                if (sameData != null && sameData.intentionId != request.intentionId)
                {
                    string side = ComboSourceUtil.Instance.FindEnum(request.side, "side");
                    string manager = string.IsNullOrEmpty(request.partyManagerName) ? string.Empty : $"{request.partyManagerName}(投资经理)";
                    string managerComma = string.IsNullOrEmpty(manager) ? string.Empty : "、";
                    string trader = string.IsNullOrEmpty(request.partyTraderName) ? string.Empty : $"{request.partyTraderName}(交易员),";
                    string account = string.Empty;
                    if (LocalEnv.ConfInfo.IsPAYL)
                    {
                        account = string.IsNullOrEmpty(request.accountName) ? string.Empty : $"在{request.accountName}(交易账户)下,\r\n";
                    }

                    PriceMatchParam priceMatchParam = sendPriceViewModel.GetPriceMatchParam();
                    if (priceMatchParam != null && priceMatchParam.MatchedQty.HasValue && priceMatchParam.MatchedQty > 0)
                    {
                        string message = $"已有{manager}{managerComma}{trader}\r\n{account}针对{request.securityid}债券的{side}需求，\r\n请先解除匹配的成交再修改。";
                        CustomMessageBox.Info(message, "更新提醒");
                        return;
                    }
                    else
                    {
                        string message = $"已有{manager}{managerComma}{trader}\r\n{account}针对{request.securityid}债券的{side}需求，\r\n是否用当前要素更新该需求?";
                        if (CustomMessageBox.Question(message, "更新提醒", null, 300, 300) == false)
                        {
                            sendPriceViewModel.SendPriceEnable = true;
                            return;
                        }
                    }
                }
            }
            

            try
            {
                PResult result = TableOldHandler.Instance.UpdateIntentionQuote(request);
                if (result != null && result.Code == "10000")
                {
                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        BaseReplyPo reply = new BaseReplyPo();
                        reply.Code = result.Code;
                        //List<TPlaceOrderResult> resultList = new List<TPlaceOrderResult>();
                        //TPlaceOrderResult por = new TPlaceOrderResult();
                        //if (CommonUtils.IsInvestor())
                        //{
                        //    por.Code = "10001";
                        //}
                        //else
                        //{
                        //    if (result.Msg.Contains("强风控"))
                        //    {
                        //        por.Code = "10002";
                        //    }
                        //    else if (result.Msg.Contains("弱风控"))
                        //    {
                        //        por.Code = "10001";
                        //    }
                        //}
                        //por.DoubleWarn = false;
                        //por.Reason = result.Msg;
                        //resultList.Add(por);
                        //reply.Msg = MsgHelper.Serializer<List<TPlaceOrderResult>>(resultList);
                        reply.Msg = result.Msg;

                        SendPriceWarnView warnView = new SendPriceWarnView(reply, GetNewOrderSingle(), request);
                        warnView.OnAction += WarnView_OnAction;
                        warnView.ShowDialog();
                    }));
                }
                else
                {
                    string errMsg = result == null ? $"意向id:[{request.intentionId}]修改失败" : (!result.IsSuccess() ? result.Msg : string.Empty);
                    if (!string.IsNullOrEmpty(errMsg))
                    {
                        Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                        {
                            MessageBoxGUI msgBox = new MessageBoxGUI();
                            msgBox.setWarnMsg(errMsg);
                            msgBox.Owner = myWindow;
                            msgBox.ShowDialog();

                            sendPriceViewModel.SendPriceEnable = true;
                        }));
                    }
                    else
                    {
                        RemenberInstAndTrade(request.traderId, request.institutionId);
                        OnCancelClick();
                    }
                }
            }
            catch (Exception e)
            {
                LogHelper.Error(e);

                OnCancelClick();
            }
        }

        /// <summary>
        /// 新建意向单
        /// </summary>
        /// <returns></returns>
        private STC.Common.Model.Table.WorkBrench.InsertIntentionQuoteReq GetInsertIntentionQuoteRequest()
        {
            PriceMatchParam priceMatchParam = sendPriceViewModel.GetPriceMatchParam();
            //建立意向单
            STC.Common.Model.Table.WorkBrench.InsertIntentionQuoteReq request = new STC.Common.Model.Table.WorkBrench.InsertIntentionQuoteReq();
            request.userName = LocalEnv.UserInfo.UserName;
            string quotetype = "INTENTION";
            if (LocalEnv.UserInfo.RoleType == RoleType.BUYER_ROLE)
            {
                quotetype = "BUYER_INTENTION";
            }
            
            request.quoteType = quotetype;
            STC.Common.Model.Table.WorkBrench.IntentionQuoteInfo record = new STC.Common.Model.Table.WorkBrench.IntentionQuoteInfo();

            if (LocalEnv.ConfInfo.IsPAYL)
            {
                request.relatedUsers = sendPriceViewModel.RelatedUsers;
                record.relatedUsers = sendPriceViewModel.RelatedUsers;
            }
            record.intentionId = priceMatchParam.IntentionID;
            RequireParseViewModel requireParseViewModel = sendPriceViewModel.GetRequireParseViewModel();
            TagConditionViewModel tagconditionviewmodel = sendPriceViewModel.GetDetailConditionViewModel();
            if (LocalEnv.UserInfo.RoleType == RoleType.BUYER_ROLE)
            {
                record.counterPartyID = record.institutionId = LocalEnv.UserInfo.Termids;
                record.counterPartyName = record.institutionName = LocalEnv.UserInfo.biztype;
                record.traderId = LocalEnv.UserInfo.UserName;
                record.traderName = LocalEnv.UserInfo.FullName;

                string managerid = null;
                string managerName = null;
                string traderid = null;
                string traderName = null;

                if (requireParseViewModel.SelectTraderItem != null)
                {
                    traderid = requireParseViewModel.SelectTraderItem.Username;
                    traderName = requireParseViewModel.SelectTraderItem.Name;
                }
                //else
                //{
                //    traderid = requireParseViewModel.SelectTraderText;
                //    traderName = requireParseViewModel.SelectTraderText;
                //}

                if (requireParseViewModel.SelectInvestorItem != null)
                {
                    managerid = requireParseViewModel.SelectInvestorItem.Username;
                    managerName = requireParseViewModel.SelectInvestorItem.Name;
                }
                //else
                //{
                //    managerid = requireParseViewModel.SelectInvestorText;
                //    managerName = requireParseViewModel.SelectInvestorText;
                //}

                if (CommonUtils.IsInvestor())
                {
                    if (string.IsNullOrEmpty(managerid))
                    {
                        managerid = LocalEnv.UserInfo.UserName;
                        managerName = LocalEnv.UserInfo.FullName;
                    }
                    record.roletype = "0011";//投机经理

                    record.counterPartySubID = traderid;
                    record.counterPartySubName = traderName;
                }
                else
                {
                    if (string.IsNullOrEmpty(traderid))
                    {
                        traderid = LocalEnv.UserInfo.UserName;
                        traderName = LocalEnv.UserInfo.FullName;
                    }
                    record.roletype = "0100";//交易员

                    record.counterPartySubID = managerid;
                    record.counterPartySubName = managerName;
                }

                record.partyManager = managerid;
                record.partyManagerName = managerName;
                record.partyTrader = traderid;
                record.partyTraderName = traderName;

                NoteData info5tag = null;
                if (priceMatchParam != null && JsonSplit.IsJson(priceMatchParam.Info5))
                {
                    info5tag = JsonConvert.DeserializeObject<NoteData>(priceMatchParam.Info5);
                }
                else
                {
                    info5tag = new NoteData();
                }

                info5tag.PUBLIC_REMARKS = tagconditionviewmodel.PublicRemarks;
                if (info5tag.PERSONAL_REMARKS == null)
                {
                    info5tag.PERSONAL_REMARKS = new Dictionary<string, string>();
                }
                info5tag.PERSONAL_REMARKS[LocalEnv.UserInfo.UserName] = tagconditionviewmodel.PersonalRemarks;
                info5tag.COMMAND_NOTES = tagconditionviewmodel.CommandNotes;

                record.info5 = JsonConvert.SerializeObject(info5tag);

            }
            else
            {
                record.institutionId = requireParseViewModel.SelectedInstintuion_Edit == null ? string.Empty : requireParseViewModel.SelectedInstintuion_Edit.idbkey;
                record.institutionName = requireParseViewModel.InstitutionName_Edit;
                record.traderId = requireParseViewModel.SelectedTrader_Edit == null ? string.Empty : requireParseViewModel.SelectedTrader_Edit.idbkey;
                record.traderName = requireParseViewModel.TraderName_Edit;
            }

            record.side = sendPriceViewModel.IsBid ? "1" : "2";
            record.securityId = sendPriceViewModel.SendPriceItem.Security;
            record.symbol = sendPriceViewModel.GetPriceMatchParam() == null ? string.Empty : sendPriceViewModel.GetPriceMatchParam().SymbolName;

            if (string.IsNullOrEmpty(record.symbol) && !string.IsNullOrEmpty(sendPriceViewModel.ShowText))
            {
                string[] arr = sendPriceViewModel.ShowText.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                if (arr != null && arr.Length > 1)
                {
                    record.symbol = arr[1];
                }
            }

            if (!string.IsNullOrEmpty(sendPriceViewModel.CustomPrice))
                record.price = sendPriceViewModel.CustomPrice.ConvertToDecimal();
            if (!string.IsNullOrEmpty(sendPriceViewModel.PriceType))
            {
                record.priceType = sendPriceViewModel.PriceType;
            }
            else
            {
                record.priceType = requireParseViewModel.PriceType; //价格类型，收益率
            }

            if (!string.IsNullOrEmpty(sendPriceViewModel.NetPrice))
                record.cleanPrice = sendPriceViewModel.NetPrice.ConvertToDecimal();

            if (sendPriceViewModel.GetQtyViewModel() != null && sendPriceViewModel.GetQtyViewModel().GetAmount() != 0)
                record.qty = sendPriceViewModel.GetQtyViewModel().TrueAmount;

            if (sendPriceViewModel.SettleBoxIndex == 1)
            {
                record.settlType = sendPriceViewModel.SendPriceItem.SettleType;
            }
            else
                record.settlType = GetSetttle();

            if (!sendPriceViewModel.WholeFlagChecked)
                record.wholeFlag = "0";
            else
                record.wholeFlag = "1";

            record.text = requireParseViewModel.BondTxt;

            record.userName = record.brokerId = record.brokerName = string.IsNullOrEmpty(requireParseViewModel.GetBrokerName()) ? LocalEnv.UserInfo.UserName : requireParseViewModel.GetBrokerName();
            //record.qqNumber = item.QqNumber;
            //record.createTime = item.CreateTime;
            //record.updateTime = item.UpdateTime;
            //record.fullPrice = item.FullPrice;


            if (LocalEnv.ConfInfo.IsAccountUser == "1")
            {
                record.accountID = sendPriceViewModel.SelectedItem;
                record.accountName = sendPriceViewModel.SelectedText;
            }

            //标签
            TagData tagData = tagconditionviewmodel.GetTagData();
            record.ifSubBond = tagData.IF_SUB_BOND;
            TagData clientTagData = requireParseViewModel.GetTagData();
            record.orgType = clientTagData.ORG_TYPE;
            record.vipCode = clientTagData.VIP;


            record.closeBy = LocalEnv.UserInfo.UserName;
            record.closeByName = LocalEnv.UserInfo.FullName;
            record.status = "0";



            //撤单时间
            string info1 = tagconditionviewmodel.GetValidTimeType();

            ////撤单条件，置灰
            PriceCondition priceCondition = tagconditionviewmodel.GetPriceCondition();
            //if (priceCondition != null)
            //{
            //    record.PriceCondition = JsonConvert.SerializeObject(priceCondition);
            //}

            //默认 "0|" : 时长 "1|{min}:{sec}" : 时点  "2|{hour}:{min}:{sec}" 无 "3|"
            string validtimetype = tagconditionviewmodel.GetValidTimeType();
            //yyyy-MM-dd HH:mm:ss
            string validuntiltime = tagconditionviewmodel.GetValidUntilTime();

            request.quoteType = quotetype;
            OrderTimeModel otm = new OrderTimeModel() { ValidTimeType = info1, ValidUntilTime = validuntiltime, IF_SUB_BOND = record.ifSubBond ?? string.Empty, VIP = record.vipCode ?? string.Empty, ORG_TYPE = record.orgType ?? string.Empty, MorethanMarketBPCheck = priceCondition.MorethanMarketBPCheck ?? string.Empty, MorethanMarketBP = priceCondition.MorethanMarketBP ?? string.Empty, Text = record.text, QuoteType = quotetype, UserName = record.userName };

            record.validuntitime = validuntiltime;

            record.hurryFlag = tagconditionviewmodel.HurryFlag;
            record.suggestFlag = tagconditionviewmodel.SuggestFlag;
            record.fireFlag = tagconditionviewmodel.FireFlag;
            record.floorPrice = tagconditionviewmodel.FloorPrice;
            record.OcoFlag = tagconditionviewmodel.OcoFlag;
            record.publicRemarks = tagconditionviewmodel.PublicRemarks;
            record.personalRemarks = tagconditionviewmodel.PersonalRemarks;
            if (record.priceType != "1" && record.priceType != "2")
            {
                record.customerPrice = sendPriceViewModel.CustomerPrice;
                record.customerSpread = sendPriceViewModel.BP;
            }
            else
            {
                record.customerPrice = null;
                record.customerSpread = null;
            }

            TagData info2tag = null;
            if (priceMatchParam != null && JsonSplit.IsJson(priceMatchParam.Info2))
            {
                info2tag = JsonConvert.DeserializeObject<TagData>(priceMatchParam.Info2);
            }
            else
            {
                info2tag = new TagData();
            }

            info2tag.IF_SUB_BOND = otm.IF_SUB_BOND;
            info2tag.ORG_TYPE = otm.ORG_TYPE;
            info2tag.VIP = otm.VIP;
            info2tag.HURRY_FLAG = tagconditionviewmodel.HurryFlag;
            info2tag.SUGGEST_FLAG = tagconditionviewmodel.SuggestFlag;
            info2tag.FIRE_FLAG = tagconditionviewmodel.FireFlag;
            info2tag.FLOOR_PRICE = tagconditionviewmodel.FloorPrice;
            if (record.priceType != "1" && record.priceType != "2")
            {
                info2tag.CUSTOMER_PRICE = sendPriceViewModel.CustomerPrice.HasValue ? sendPriceViewModel.CustomerPrice.ToString() : string.Empty;
                info2tag.CUSTOMER_SPREAD = sendPriceViewModel.BP.HasValue ? sendPriceViewModel.BP.ToString() : string.Empty;
            }
            else
            {
                info2tag.CUSTOMER_PRICE = null;
                info2tag.CUSTOMER_SPREAD = null;
            }
            info2tag.PUBLIC_REMARKS = tagconditionviewmodel.PublicRemarks;
            info2tag.PERSONAL_REMARKS = tagconditionviewmodel.PersonalRemarks;
            info2tag.OCO_FLAG = tagconditionviewmodel.OcoFlag;

            record.info2 = JsonConvert.SerializeObject(info2tag);
            record.channel = (priceMatchParam == null || string.IsNullOrEmpty(priceMatchParam.channel)) ? "CLIENT" : priceMatchParam.channel;
            record.handlinst = sendPriceViewModel.DarkDiskFlagChecked ? "2" : "1";
            record.clordId = DataTimeSequence.Instance.GetId();
            request.quoteList = new List<Model.Table.WorkBrench.IntentionQuoteInfo>();
            if (LocalEnv.ConfInfo.IsPAYL)
            {
                // 持仓组合id
                string bookid = string.Empty;
                if (sendPriceViewModel.SelectedGroupItem != null && sendPriceViewModel.SelectedGroupItem.Count > 0)
                {
                    bookid = sendPriceViewModel.SelectedGroupItem[0];
                }
                record.bookId = bookid;
                //持仓组合名称
                string bookName = string.Empty;
                if (sendPriceViewModel.SelectedGroupText != null && sendPriceViewModel.SelectedGroupText.Count > 0)
                {
                    bookName = sendPriceViewModel.SelectedGroupText[0];
                }
                record.bookName = bookName;
                // 持仓组合账号id
                string accountid = string.Empty;
                if (!string.IsNullOrEmpty(sendPriceViewModel.SelectedTradeAccountCode))
                {
                    accountid = sendPriceViewModel.SelectedTradeAccountCode;
                }
                record.accountID = accountid;
                // 持仓组合账号名称
                string accountName = string.Empty;
                if (!string.IsNullOrEmpty(sendPriceViewModel.SelectedTradeAccountName))
                {
                    accountName = sendPriceViewModel.SelectedTradeAccountName;
                }
                record.accountName = accountName;
                // 恒生基金编号
                string hsProductId = string.Empty;
                if (!string.IsNullOrEmpty(sendPriceViewModel.SelectedProductId))
                {
                    hsProductId = sendPriceViewModel.SelectedProductId;
                }
                otm.HsProductId = hsProductId;
                record.inf1 = JsonConvert.SerializeObject(otm);

                if (CommonUtils.IsInvestor())
                {
                    request.actionFlag = "0";
                }
                else
                {
                    request.actionFlag = "2";
                }
                
                request.riskManualCk = "1";
            }
            request.quoteList.Add(record);
            return request;
        }

        /// <summary>
        /// 保存需求单
        /// </summary>
        private void PlaceInstertQuoteRequest()
        {
            STC.Common.Model.Table.WorkBrench.InsertIntentionQuoteReq request = GetInsertIntentionQuoteRequest();
            
            sendPriceViewModel.SendPriceEnable = false;

            
            if (LocalEnv.UserInfo.RoleType == RoleType.BUYER_ROLE)
            {
                //if (!string.IsNullOrEmpty(request.quoteList[0].partyManager) && !string.IsNullOrEmpty(request.quoteList[0].partyTrader))
                {
                    STC.Common.Model.Table.WorkBrench.IntentionQuoteInfo sameData = IntentionOldHandler.Instance.GetSameIntentionInfo(request.quoteList[0].partyManager, request.quoteList[0].partyTrader, request.quoteList[0].securityId, request.quoteList[0].side, request.quoteList[0].accountID);


                    if (sameData != null)
                    {
                        // 如果是撤销中， 直接拒绝
                        if (sameData.status == "82")
                        {
                            string side = ComboSourceUtil.Instance.FindEnum(request.quoteList[0].side, "side");
                            string manager = string.IsNullOrEmpty(request.quoteList[0].partyManagerName) ? string.Empty : $"{request.quoteList[0].partyManagerName}(投资经理)";
                            string managerComma = string.IsNullOrEmpty(manager) ? string.Empty : "、";
                            string trader = string.IsNullOrEmpty(request.quoteList[0].partyTraderName) ? string.Empty : $"{request.quoteList[0].partyTraderName}(交易员),";
                            string account = string.Empty;
                            if (LocalEnv.ConfInfo.IsPAYL)
                            {
                                account = string.IsNullOrEmpty(request.quoteList[0].accountName) ? string.Empty : $"在{request.quoteList[0].accountName}(交易账户)下,\r\n";
                            }
                            string message = $"已有{manager}{managerComma}{trader}\r\n{account}针对{request.quoteList[0].securityId}债券的{side}需求正在撤销中，\r\n将无法新增需求";
                           

                            BaseReplyPo reply = new BaseReplyPo();
                            reply.Code = "10001";
                            List<TPlaceOrderResult> result = new List<TPlaceOrderResult>();
                            TPlaceOrderResult por = new TPlaceOrderResult();
                            por.Code = "10004";
                            por.DoubleWarn = false;
                            por.Reason = message;
                            result.Add(por);
                            reply.Msg = MsgHelper.Serializer<List<TPlaceOrderResult>>(result);
                           
                            sendPriceViewModel.SendPriceEnable = true;
                            SendPriceWarnView warnView = new SendPriceWarnView(reply, GetNewOrderSingle());
                            warnView.Owner = myWindow;
                            warnView.Show();

                            return;
                        }

                        // 如果是未处理和处理中，弹询框
                        if (sameData.status == "80" || sameData.status == "81")
                        {
                            string side = ComboSourceUtil.Instance.FindEnum(request.quoteList[0].side, "side");
                            string manager = string.IsNullOrEmpty(request.quoteList[0].partyManagerName) ? string.Empty : $"{request.quoteList[0].partyManagerName}(投资经理)";
                            string managerComma = string.IsNullOrEmpty(manager) ? string.Empty : "、";
                            string trader = string.IsNullOrEmpty(request.quoteList[0].partyTraderName) ? string.Empty : $"{request.quoteList[0].partyTraderName}(交易员),";
                            string account = string.Empty;
                            if (LocalEnv.ConfInfo.IsPAYL)
                            {
                                account = string.IsNullOrEmpty(request.quoteList[0].accountName) ? string.Empty : $"在{request.quoteList[0].accountName}(交易账户)下,\r\n";
                            }
                            string message = $"已有{manager}{managerComma}{trader}\r\n{account}针对{request.quoteList[0].securityId}债券的{side}需求，\r\n是否用当前要素更新该需求?";
                            if (CustomMessageBox.Question(message, "更新提醒", null, 300, 300) == false)
                            {
                                sendPriceViewModel.SendPriceEnable = true;
                                return;
                            }
                        }
                    }
                }
            }

            try
            {
                string traderId = string.Empty;
                string institutionId = string.Empty;
                foreach (STC.Common.Model.Table.WorkBrench.IntentionQuoteInfo intentionQuoteInfo in request.quoteList)
                {
                    traderId = intentionQuoteInfo.traderId;
                    institutionId = intentionQuoteInfo.institutionId;
                }

                Common.Model.Table.WorkBrench.InsertIntentionQuoteReplyPo result = IntentionOldHandler.Instance.InsertIntentionQuote_Common(request);

                if (result != null && result.Code == "10000")
                {
                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        string riskCheckResult = string.Empty;
                        string riskCheckDetail = string.Empty;
                        if (result != null && result.Datas != null && result.Datas.Count > 0)
                        {
                           foreach (Common.Model.Table.WorkBrench.IntentionQuoteInfo obj in result.Datas)
                            {
                                if (JsonSplit.IsJson(obj.info5))
                                {
                                    NoteData info5tag = JsonConvert.DeserializeObject<NoteData>(obj.info5);

                                    riskCheckResult = info5tag.RISK_CHECK_RESULT;
                                    riskCheckDetail = info5tag.RISK_CHECK_DETAIL;
                                }
                            }
                        }

                        BaseReplyPo reply = new BaseReplyPo();
                        reply.Code = result.Code;
                        reply.Msg = riskCheckDetail;

                        SendPriceWarnView warnView = new SendPriceWarnView(reply, GetNewOrderSingle(), request);
                        warnView.OnAction += WarnView_OnAction;
                        warnView.ShowDialog();
                    }));
                }
                else
                {
                    string errMsg = result == null ? $"新增需求失败" : (!result.IsSuccess() ? result.Msg : string.Empty);
                    if (!string.IsNullOrEmpty(errMsg))
                    {
                        Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                        {
                            MessageBoxGUI msgBox = new MessageBoxGUI();
                            msgBox.setWarnMsg(errMsg);
                            msgBox.Owner = myWindow;
                            msgBox.ShowDialog();

                            sendPriceViewModel.SendPriceEnable = true;
                        }));
                    }
                    else
                    {
                        RemenberInstAndTrade(traderId, institutionId);
                        OnCancelClick();
                    }
                }
            }
            catch (Exception e)
            {
                LogHelper.Error(e);

                OnCancelClick();
            }
        }
        #endregion

        #region 恢复
        /// <summary>
        /// 恢复内部订单
        /// </summary>
        private void ConfirmIntentionOrder()
        {
            PriceMatchParam priceMatchParam = sendPriceViewModel.GetPriceMatchParam();
            NewOrderSingle tempOrder = GetNewOrderSingle();
            STC.Common.Model.Table.WorkBrench.ConfirmIntentionQuoteRequest request = new STC.Common.Model.Table.WorkBrench.ConfirmIntentionQuoteRequest();
            request.intentionId = tempOrder.IntentionID;
            request.actionType = "5";// 恢复
            request.userName = tempOrder.UserName;
            request.closeBy = LocalEnv.UserInfo.UserName;
            request.closeByName = LocalEnv.UserInfo.FullName;
            request.clOrdId = DataTimeSequence.Instance.GetId();
            request.ifSubBond = tempOrder.IfSubBond;
            request.vipCode = tempOrder.VipCode;
            request.orgType = tempOrder.OrgType;
            request.validUntilTime = tempOrder.ValidUntilTime;
            request.inf1 = tempOrder.Info1;
            request.info2 = tempOrder.Info2;
            request.channel = tempOrder.Channel;
            request.priceType = tempOrder.PriceType;
            request.updateTime = priceMatchParam.updateTime;
            if (LocalEnv.UserInfo.RoleType == RoleType.BUYER_ROLE)
            {
                request.quoteType = "BUYER_INTENTION";
            }
            else
            {
                request.quoteType = "INTENTION";
            }
            sendPriceViewModel.SendPriceEnable = false;

            try
            {
                STC.Common.Model.Table.WorkBrench.ConfirmIntentionQuoteReplyPo result = TableOldHandler.Instance.ConfirmIntentionQuote(request);
                string errMsg = result == null ? $"意向id:[{request.intentionId}]修改失败" : (!result.IsSuccess() ? result.Msg : string.Empty);
                if (!string.IsNullOrEmpty(errMsg))
                {
                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        MessageBoxGUI msgBox = new MessageBoxGUI();
                        msgBox.setWarnMsg(errMsg);
                        msgBox.Owner = myWindow;
                        msgBox.ShowDialog();

                        sendPriceViewModel.SendPriceEnable = true;
                    }));
                }
                else
                {
                    RemenberInstAndTrade(tempOrder.TraderID, tempOrder.InstitutionID);
                    OnCancelClick();
                    //作废区恢复历史记录成功后发送消息，删除作废区的订单
                    STC.Common.Handler.Base.BaseHandler.Instance.PublishAppMsg(new STC.Common.Model.Fix.Message.PResult() { Code = "ResumeInnerHistoryRequire", RequestID = $"{request.intentionId}" });
                }
            }
            catch (Exception e)
            {
                LogHelper.Error(e);

                OnCancelClick();
            }
        }
        #endregion
    }
}
