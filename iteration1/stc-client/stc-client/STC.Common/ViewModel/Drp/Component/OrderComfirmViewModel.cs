using Ats.Foundation.Message;
using com.gateway.log;
using Newtonsoft.Json;
using STC.Common.Command;
using STC.Common.Data;
using STC.Common.Handler.Table;
using STC.Common.Model;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.Table;
using STC.Common.Model.TBook;
using STC.Common.Utils;
using STC.Common.View;
using STC.Common.View.Component;
using STC.Common.View.Drp.Component;
using STC.Common.View.MyTrading;
using STC.Common.ViewModel.Order.InnerPanel;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace STC.Common.ViewModel.Drp.Component
{
    public class OrderComfirmViewModel : ViewModelBase
    {

        private Brush borderBrush = new SolidColorBrush(Color.FromRgb(0x3C, 0x3C, 0x3C));
        /// <summary>
        /// 边框颜色，激活和失去焦点后变化
        /// </summary>
        public Brush BorderBrush
        {
            get { return borderBrush; }
            set
            {
                if (value != null)
                {
                    borderBrush = value;
                    this.OnPropertyChanged(nameof(BorderBrush));
                }
            }
        }

        private string _MsgId;
        public string MsgId
        {
            get
            {
                return _MsgId;
            }
            set
            {
                _MsgId = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// 标题
        /// </summary>
        private string _Title;

        public string Title
        {
            get { return _Title; }
            set
            {
                _Title = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// 销交
        /// </summary>
        private string _Dealer;

        public string Dealer
        {
            get { return _Dealer; }
            set
            {
                _Dealer = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// 创建时间
        /// </summary>
        private string _CreateTime;

        public string CreateTime
        {
            get { return _CreateTime; }
            set
            {
                _CreateTime = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// 债券代码
        /// </summary>
        private string _SecurityID;

        public string SecurityID
        {
            get { return _SecurityID; }
            set
            {
                _SecurityID = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// 方向
        /// </summary>
        private string _Side;

        public string Side
        {
            get { return _Side; }
            set
            {
                _Side = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// 客户报价
        /// </summary>
        private string _CustomerPrice;

        public string CustomerPrice
        {
            get { return _CustomerPrice; }
            set
            {
                _CustomerPrice = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// 定价
        /// </summary>
        private string _Price;

        public string Price
        {
            get { return _Price; }
            set
            {
                if (value == null || value.ConvertToDecimal() < 0)
                {
                    _Price = CustomerPrice;
                }
                else
                {
                    _Price = value;
                }
                
                
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// 数量
        /// </summary>
        private string _Qty;

        public string Qty
        {
            get { return _Qty; }
            set
            {
                _Qty = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// 原文
        /// </summary>
        private string _Text;

        public string Text
        {
            get { return _Text; }
            set
            {
                _Text = value;
                this.OnPropertyChanged();
            }
        }


        /// <summary>
        /// 是否微信
        /// </summary>
        private string _IsWX;

        public string IsWX
        {
            get { return _IsWX; }
            set
            {
                _IsWX = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// 点差
        /// </summary>
        private decimal? _bp;

        public decimal? BP
        {
            get
            {
                return _bp;
            }
            set
            {
                if (_bp != value)
                {
                    _bp = value;

                    if (BP.HasValue && CommonUtils.IsNumeric(CustomerPrice))
                    {
                        if (Side == "1")
                        {
                            Price = (CustomerPrice.ConvertToDecimal() + BP / 100)?.ToString("0.####");
                        }
                        else
                        {
                            Price = (CustomerPrice.ConvertToDecimal() - BP / 100)?.ToString("0.####");
                        }
                    }
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 急
        /// </summary>
        private string _hurryFlag;

        public string HurryFlag
        {
            get
            {
                return _hurryFlag;
            }
            set
            {
                _hurryFlag = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// 荐
        /// </summary>
        private string _suggestFlag;

        public string SuggestFlag
        {
            get
            {
                return _suggestFlag;
            }
            set
            {
                _suggestFlag = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// 火
        /// </summary>
        private string _fireFlag;

        public string FireFlag
        {
            get
            {
                return _fireFlag;
            }
            set
            {
                _fireFlag = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// 底价
        /// </summary>
        private string _floorPrice;

        public string FloorPrice
        {
            get
            {
                return _floorPrice;
            }
            set
            {
                _floorPrice = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// OCO 打包 1:oco 2:打包
        /// </summary>
        private string _ocoFlag;

        public string OcoFlag
        {
            get
            {
                return _ocoFlag;
            }
            set
            {
                _ocoFlag = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// 公开备注
        /// </summary>
        private string _publicRemarks;

        public string PublicRemarks
        {
            get { return _publicRemarks; }
            set
            {
                _publicRemarks = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// 个人备注
        /// </summary>
        private string _personalRemarks;

        public string PersonalRemarks
        {
            get
            {
                return _personalRemarks;
            }
            set
            {
                _personalRemarks = value;
                this.OnPropertyChanged();
            }
        }

        private STC.Common.Model.Table.WorkBrench.IntentionQuoteInfo _currentInfo;

        public OrderComfirmViewModel()
        {
        }


        internal void OnOkCommand()
        {
            if (this._currentInfo != null)
            {
                DoConfirmOne(GetIntentionQuoteItem(_currentInfo));
            }
        }

        private MyTrading.IntentionQuoteItem GetIntentionQuoteItem(STC.Common.Model.Table.WorkBrench.IntentionQuoteInfo obj)
        {
            MyTrading.IntentionQuoteItem item = new MyTrading.IntentionQuoteItem();
            item.ClOrdID = obj.clordid;
            item.IntentionId = obj.intentionId;
            item.InstitutionId = obj.institutionId;
            item.InstitutionName = obj.institutionName;
            item.TraderId = obj.traderId;
            item.TraderName = obj.traderName;
            item.BrokerId = obj.userName;
            item.BrokerName = obj.name;
            item.BrokerName_Edit = obj.name;
            item.QqNumber = obj.qqNumber;
            item.Side = obj.side;
            item.SecurityId = obj.securityId;
            item.Symbol = obj.symbol;
            item.CleanPrice = obj.cleanPrice;
            item.FullPrice = obj.fullPrice;
            item.CustomerPrice = obj.price;
            item.BP = this.BP;

            item.Qty = obj.qty;
            item.ExecutionQty = obj.executionqty;
            item.SettlType = obj.settlType;
            item.Text = obj.text;
            item.CreateTime = obj.createTime;
            item.UpdateTime = obj.updateTime;
            item.UserName = obj.userName;
            item.CloseBy = obj.closeBy;
            item.CloseByName = obj.closeByName;
            item.Type = obj.type;
            item.Status = obj.status;
            item.WholeFlag = obj.wholeFlag;
            item.AccountID = obj.accountID;
            item.AccountName = obj.accountName;
            item.ifSubBond = obj.ifSubBond;
            item.vipCode = obj.vipCode;
            item.validUntilTime = obj.validuntitime;
            item.Info1 = obj.inf1;
            item.Info2 = obj.info2;
            item.orgType = obj.orgType;
            item.OrderID = obj.orderId;
            item.PriceType = obj.priceType;
            item.FirmFlag = obj.firmFlag;

            item.Investor = obj.partyManager;
            item.InvestorName = obj.partyManagerName;
            item.TraderName_Buyer = obj.partyTraderName;
            item.Trader_Buyer = obj.partyTrader;
            item.BookId = obj.bookId;
            item.BookName = obj.bookName;
            item.HsProductId = obj.hsProductId;
            item.AccountID = obj.accountID;
            item.AccountName = obj.accountName;
            return item;
        }

        private Common.Model.Fix.Message.NewOrderSingle GetNewOrderSingle(STC.Common.Model.Table.WorkBrench.IntentionQuoteInfo item, string firmflag)
        {
            Common.Model.Fix.Message.NewOrderSingle request = new Common.Model.Fix.Message.NewOrderSingle();
            request.FirmFlag = firmflag;
            request.OrgType = item.orgType;
            request.VipCode = item.vipCode;
            request.IfSubBond = item.ifSubBond;
            request.AlgoName = "CROSS";
            request.VenueTypeGW = "INNER";
            request.Venues = "MANUAL";
            request.ClOrdID = item.clordId;
            request.IntentionID = item.intentionId;
            request.MarketIndicator = "4";
            request.OrdType = "2";
            request.OrderQty = item.qty.ToString();
            request.Price = item.price.ToString();
            request.PriceType = item.priceType; //收益率
            //request.NetPrice = ;
            request.SecurityID = item.securityId;
            request.Symbol = item.symbol;
            //request.OrdStatus = ;
            request.RiskManualCk = "";
            request.SettlType = item.settlType;
            request.Side = item.side;
            request.Terminal = string.Format("STClient_v{0}", LocalEnv.ServerInfo.Version); ;
            request.TimeInForce = "0";
            request.Name = request.UserName = LocalEnv.UserInfo.UserName;
            request.WholeFlag = item.wholeFlag;
            request.HandlInst = "1";
            //request.QuantID = ;
            request.MacID = EngineHelper.GetMacByNetworkInterface();
            request.InsideIP = EngineHelper.GetIp();
            request.HardDiskID = EngineHelper.Getharddiskid2();
            // 默认时间
            request.ValidUntilTime = item.validuntitime;
            request.Info1 = item.inf1;
            request.InstitutionID = item.institutionId;
            request.InstitutionName = item.institutionName;
            request.TraderID = item.traderId;
            request.TraderName = item.traderName;
            request.UserName = item.userName;
            request.AccountID = item.accountID;
            request.AccountName = item.accountName;
            request.Text = item.text;

            request.OCOFlag = this.OcoFlag;
            TagData info2tag = new TagData()
            {
                IF_SUB_BOND = request.IfSubBond,
                ORG_TYPE = request.OrgType,
                VIP = request.VipCode,
                HURRY_FLAG = this.HurryFlag,
                SUGGEST_FLAG = this.SuggestFlag,
                FIRE_FLAG = this.FireFlag,
                FLOOR_PRICE = this.FloorPrice,
                CUSTOMER_PRICE = item.customerPrice.HasValue ? item.customerPrice.ToString() : string.Empty,
                CUSTOMER_SPREAD = this.BP?.ToString(),
                PUBLIC_REMARKS = this.PublicRemarks,
                PERSONAL_REMARKS = this.PersonalRemarks,
                OCO_FLAG = this.OcoFlag
            };
            request.Info2 = JsonConvert.SerializeObject(info2tag);
            return request;
        }

        private System.Threading.Tasks.Task<bool> UpdateIntention(MyTrading.IntentionQuoteItem item)
        {
            STC.Common.Model.Table.WorkBrench.UpdateIntentionQuoteRequest updateRequest = new STC.Common.Model.Table.WorkBrench.UpdateIntentionQuoteRequest();
            updateRequest.intentionId = item.IntentionId;

            if (CommonUtils.IsNumeric(this.Price))
            {
                updateRequest.price = this.Price.ConvertToDecimal();
            }

            if (CommonUtils.IsNumeric(this.CustomerPrice))
            {
                updateRequest.customerPrice = this.CustomerPrice.ConvertToDecimal();
            }

            if (CommonUtils.IsNumeric(this.Qty))
            {
                updateRequest.qty = this.Qty.ConvertToDecimal();
            }

            updateRequest.customerSpread = this.BP;
            updateRequest.hurryFlag = this.HurryFlag;
            updateRequest.suggestFlag = this.SuggestFlag;
            updateRequest.fireFlag = this.FireFlag;
            updateRequest.floorPrice = this.FloorPrice;
            updateRequest.OcoFlag = this.OcoFlag;
            //updateRequest.publicRemarks = this.PublicRemarks;
            //updateRequest.personalRemarks = this.PersonalRemarks;
            updateRequest.updateTime = DateTimeEx.ServerTime.ToString("yyyy-MM-dd HH:mm:ss.fff");
            Common.ViewModel.Order.InnerPanel.TagData info2tag = new Common.ViewModel.Order.InnerPanel.TagData()
            {
                HURRY_FLAG = this.HurryFlag,
                SUGGEST_FLAG = this.SuggestFlag,
                FIRE_FLAG = this.FireFlag,
                FLOOR_PRICE = this.FloorPrice,
                CUSTOMER_PRICE = this.CustomerPrice,
                CUSTOMER_SPREAD = this.BP.HasValue ? this.BP.ToString() : string.Empty,
                //PUBLIC_REMARKS = item.PublicRemarks_Edit,
                //PERSONAL_REMARKS = item.PersonalRemarks_Edit,
                OCO_FLAG = this.OcoFlag
            };
            updateRequest.info2 = Newtonsoft.Json.JsonConvert.SerializeObject(info2tag);
            if (LocalEnv.UserInfo.RoleType == RoleType.BUYER_ROLE)
            {
                updateRequest.QuoteType = "BUYER_INTENTION";
            }
            else
            {
                updateRequest.QuoteType = "INTENTION";
            }

            return UpdateIntentionQuote(updateRequest);
        }

        public Task<bool> UpdateIntentionQuote(STC.Common.Model.Table.WorkBrench.UpdateIntentionQuoteRequest request)
        {
            return Task.Run(() =>
            {
                bool isSuccess = false;
                if (string.IsNullOrEmpty(request.intentionId))
                    return isSuccess;

                PResult result = TableOldHandler.Instance.UpdateIntentionQuote(request);
                if (result == null)
                {
                    CommonUtils.Prompt2Second("警告", $"意向id:[{request.intentionId}]修改失败");
                }
                else
                {
                    if (!result.IsSuccess())
                    {
                        if (result.Code == "10000")
                        {
                            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                            {
                                BaseReplyPo reply = new BaseReplyPo();
                                reply.Code = result.Code;
                                reply.Msg = result.Msg;

                                SendPriceWarnView warnView = new SendPriceWarnView(reply, GetNewOrderSingle(_currentInfo, "0"), request);
                                warnView.ShowDialog();
                            }));
                        }
                        else
                        {
                            CommonUtils.Prompt2Second("警告", result.Msg);
                        }
                    }
                    else
                    {
                        isSuccess = true;
                    }
                }
                return isSuccess;
            });
        }


        private async void DoConfirmOne(MyTrading.IntentionQuoteItem item)
        {

            if (LocalEnv.UserInfo.RoleType == RoleType.BUYER_ROLE)
            {
                STC.Common.Model.Table.WorkBrench.IntentionQuoteInfo sameData = IntentionOldHandler.Instance.GetSameIntentionInfo(item.Investor, item.Trader_Buyer, item.SecurityId, item.Side, item.AccountID);
                if (sameData != null && sameData.intentionId != item.IntentionId)
                {
                    string side = ComboSourceUtil.Instance.FindEnum(item.Side, "side");
                    string manager = string.IsNullOrEmpty(item.InvestorName) ? string.Empty : $"{item.InvestorName}(投资经理)";
                    string managerComma = string.IsNullOrEmpty(manager) ? string.Empty : "、";
                    string trader = string.IsNullOrEmpty(item.TraderName_Buyer) ? string.Empty : $"{item.TraderName_Buyer}(交易员),";
                    string account = string.Empty;
                    if (LocalEnv.ConfInfo.IsPAYL)
                    {
                        account = string.IsNullOrEmpty(item.AccountName) ? string.Empty : $"在{item.AccountName}(交易账户)下,\r\n";
                    }

                    string message = $"已有{manager}{managerComma}{trader}\r\n{account}针对{item.SecurityId}债券的{side}需求，\r\n是否用当前要素更新该需求?";
                    if (CustomMessageBox.Question(message, "提醒", null, 300, 300) == false)
                    {
                        return;
                    }
                }
            }


            bool isSuccess = await UpdateIntention(item);

            if (!isSuccess)
            {
                return;
            }

            item.IsConfirmed = true;
            string intentionId = item.IntentionId;
            STC.Common.Model.Table.WorkBrench.ConfirmIntentionQuoteRequest request = new STC.Common.Model.Table.WorkBrench.ConfirmIntentionQuoteRequest();
            request.intentionId = intentionId;
            request.corssVenues = "2";
            if (LocalEnv.UserInfo.RoleType == RoleType.BUYER_ROLE)
            {
                request.actionType = "81";// NF发布确认
            }
            else
            {
                request.actionType = "1";// NF发布确认
            }
            
            request.userName = item.BrokerId;
            request.closeBy = LocalEnv.UserInfo.UserName;
            request.closeByName = LocalEnv.UserInfo.FullName;
            request.clOrdId = DataTimeSequence.Instance.GetId();
            request.accountID = item.AccountID;
            request.accountName = item.AccountName;
            request.ifSubBond = item.ifSubBond;
            request.vipCode = item.vipCode;
            request.orgType = item.orgType;
            request.validUntilTime = item.validUntilTime;
            request.inf1 = item.Info1;
            request.priceType = item.PriceType;
            request.updateTime = item.UpdateTime;
            if (LocalEnv.UserInfo.RoleType == RoleType.BUYER_ROLE)
            {
                request.quoteType = "BUYER_INTENTION";
            }
            else
            {
                request.quoteType = "INTENTION";
            }
            //request.hurryFlag = this.HurryFlag;
            //request.suggestFlag = this.SuggestFlag;
            //request.fireFlag = this.FireFlag;
            //request.floorPrice = this.FloorPrice;
            //request.OcoFlag = this.OcoFlag;
            //request.publicRemarks = info.publicRemarks;
            //request.personalRemarks = info.personalRemarks;
            //request.customerPrice = info.customerPrice.Value;
            //request.customerSpread = this.BP.Value;
            

            item.Request = request;

            System.Threading.ThreadPool.QueueUserWorkItem(o =>
            {
                try
                {
                    int totalCount = 0;
                    int failCount = 0;
                    Dictionary<string, MyTrading.IntentionQuoteItem> orderDic = new Dictionary<string, MyTrading.IntentionQuoteItem>();
                    Dictionary<string, List<TPlaceOrderResult>> warnReplyDic = new Dictionary<string, List<TPlaceOrderResult>>();
                    Dictionary<string, List<TPlaceOrderResult>> prohibitReplyDic = new Dictionary<string, List<TPlaceOrderResult>>();
                    Dictionary<string, List<TPlaceOrderResult>> exceptionReplyDic = new Dictionary<string, List<TPlaceOrderResult>>();
                    orderDic[item.IntentionId] = item;

                    GetErrorInfo(item.Request,
                            ref failCount,
                            ref totalCount,
                            warnReplyDic,
                            prohibitReplyDic,
                            exceptionReplyDic);

                    while (totalCount != 1)
                    {
                        Thread.Sleep(100);
                    }

                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        if (failCount > 0)
                        {
                            if (warnReplyDic.Count > 0 || prohibitReplyDic.Count > 0 || exceptionReplyDic.Count > 0)
                            {
                                BatchResultWindow batchResultWindow = new BatchResultWindow(orderDic,
                                    warnReplyDic, prohibitReplyDic, failCount, exceptionReplyDic);
                                batchResultWindow.Owner = Application.Current.MainWindow;
                                batchResultWindow.ShowDialog();
                            }
                            //else
                            //{
                            //    CustomMessageBox.Info($"共普通确认{totalCount}条, 其中失败{failCount}条", "普通确认结果");
                            //}
                        }
                        //else
                        //{
                        //    CustomMessageBox.Info($"共普通确认{totalCount}条, 全部成功", "普通确认结果");
                        //}
                    }));
                }
                catch (Exception e)
                {
                    LogHelper.Error("DoConfirmCommand", e);
                }
                finally
                {
                    item.IsConfirmed = false;
                }
            });
        }

        private void GetErrorInfo(STC.Common.Model.Table.WorkBrench.ConfirmIntentionQuoteRequest request,
            ref int failCount,
            ref int totalCount,
            Dictionary<string, List<TPlaceOrderResult>> warnReplyDic,
            Dictionary<string, List<TPlaceOrderResult>> prohibitReplyDic,
            Dictionary<string, List<TPlaceOrderResult>> exceptionReplyDic)
        {

            try
            {
                request.channel = "CLIENT";
                STC.Common.Model.Table.WorkBrench.ConfirmIntentionQuoteReplyPo replyPo = IntentionOldHandler.Instance.ConfirmIntentionQuote(request);
                if (replyPo == null || (replyPo != null && !replyPo.IsSuccess()))
                {
                    failCount++;

                    if (replyPo != null)
                    {
                        if (replyPo.Code == "10000")
                        {
                            bool isProhibitory = false;
                            List<TPlaceOrderResult> orderRisks = MsgHelper.Deserialize<List<TPlaceOrderResult>>(replyPo.Msg);
                            foreach (TPlaceOrderResult placeOrderResult in orderRisks)
                            {
                                if (placeOrderResult.Code == "10002")
                                {
                                    isProhibitory = true;
                                    break;
                                }
                            }

                            if (isProhibitory)
                            {
                                prohibitReplyDic[request.intentionId] = orderRisks;
                            }
                            else
                            {
                                warnReplyDic[request.intentionId] = orderRisks;
                            }
                        }
                        else
                        {
                            List<TPlaceOrderResult> exceptionResult = new List<TPlaceOrderResult>();
                            TPlaceOrderResult result = new TPlaceOrderResult();
                            result.Code = "10003";
                            result.DoubleWarn = false;
                            result.Reason = replyPo.Msg;
                            exceptionResult.Add(result);
                            exceptionReplyDic[request.intentionId] = exceptionResult;
                        }
                    }
                }
                else
                {
                    //replyUpdateThread.Enqueue(replyPo);
                }
            }
            catch (Exception e)
            {
                LogHelper.Error("批量执行操作", e);
            }
            finally
            {
                totalCount++;
            }
        }

        private bool IsInitData = false;

        public void InitData(STC.Common.Model.Table.WorkBrench.IntentionQuoteInfo info)
        {
            ThreadPool.QueueUserWorkItem(o => Application.Current.Dispatcher.Invoke(delegate
            {

                if (!IsInitData)
                {
                    IsInitData = true;

                    if (!string.IsNullOrEmpty(info.institutionName) && !string.IsNullOrEmpty(info.traderName))
                    {
                        Dealer = info.institutionName + "-" + info.traderName + "    ";
                    }
                    else if (!string.IsNullOrEmpty(info.institutionName))
                    {
                        Dealer = info.institutionName + "    ";
                    }
                    else if (!string.IsNullOrEmpty(info.traderName))
                    {
                        Dealer = info.traderName + "    ";
                    }
                    else
                    {
                        Dealer = string.Empty;
                    }
                    CreateTime = info.createTime;

                    SecurityID = info.securityId;
                    Side = info.side;
                    CustomerPrice = info.customerPrice?.ToString("0.####");
                    BP = info.customerSpread;
                    Price = info.price?.ToString("0.####");

                    if (CommonUtils.SaleDic != null)
                    {
                        if (info.channel == "FINQ")
                        {
                            if (LocalEnv.UserInfo.RoleType == RoleType.BUYER_ROLE)
                            {
                                if (CommonUtils.SaleDic.ContainsKey("IsChecked_Broker_Spread") && CommonUtils.SaleDic["IsChecked_Broker_Spread"] == "1"
                                    && CommonUtils.SaleDic.ContainsKey("Broker_Spread") && CommonUtils.IsNumeric(CommonUtils.SaleDic["Broker_Spread"]))
                                {
                                    BP = CommonUtils.SaleDic["Broker_Spread"].ConvertToDecimal();
                                }
                            }
                            else
                            {
                                if (CommonUtils.SaleDic.ContainsKey("IsBp_FQ") && CommonUtils.SaleDic["IsBp_FQ"] == "1"
                                && CommonUtils.SaleDic.ContainsKey("Bp_FQ") && CommonUtils.IsNumeric(CommonUtils.SaleDic["Bp_FQ"]))
                                {
                                    BP = CommonUtils.SaleDic["Bp_FQ"].ConvertToDecimal();
                                }
                            }
                        }
                        else if (info.channel == "WECHAT")
                        {
                            if (CommonUtils.SaleDic.ContainsKey("IsBp_WX") && CommonUtils.SaleDic["IsBp_WX"] == "1"
                                && CommonUtils.SaleDic.ContainsKey("Bp_WX") && CommonUtils.IsNumeric(CommonUtils.SaleDic["Bp_WX"]))
                            {
                                BP = CommonUtils.SaleDic["Bp_WX"].ConvertToDecimal();
                            }
                        }
                        else if (info.channel == "CLIENT")
                        {
                            if (CommonUtils.SaleDic.ContainsKey("IsBp") && CommonUtils.SaleDic["IsBp"] == "1"
                                && CommonUtils.SaleDic.ContainsKey("Bp") && CommonUtils.IsNumeric(CommonUtils.SaleDic["Bp"]))
                            {
                                BP = CommonUtils.SaleDic["Bp"].ConvertToDecimal();
                            }
                        }
                    }

                    Qty = info.qty.ToString();
                    Text = info.text;
                    if (info.channel == "WECHAT")
                    {
                        IsWX = "1";
                        Title = "收到来自企业微信的新报价";
                    }
                    else
                    {
                        IsWX = "0";
                        Title = "发来新报价";
                    }

                    HurryFlag = info.hurryFlag;
                    SuggestFlag = info.suggestFlag;
                    FireFlag = info.fireFlag;
                    FloorPrice = info.floorPrice;
                    OcoFlag = info.OcoFlag;

                    _currentInfo = info;
                }
            }));

        }


        public void UpdateData(STC.Common.Model.Table.WorkBrench.IntentionQuoteInfo info)
        {
            ThreadPool.QueueUserWorkItem(o => Application.Current.Dispatcher.Invoke(delegate
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (!string.IsNullOrEmpty(info.institutionName) && !string.IsNullOrEmpty(info.traderName))
                    {
                        Dealer = info.institutionName + "-" + info.traderName + "    ";
                    }
                    else if (!string.IsNullOrEmpty(info.institutionName))
                    {
                        Dealer = info.institutionName + "    ";
                    }
                    else if (!string.IsNullOrEmpty(info.traderName))
                    {
                        Dealer = info.traderName + "    ";
                    }
                    else
                    {
                        Dealer = string.Empty;
                    }
                    CreateTime = info.createTime;

                    SecurityID = info.securityId;
                    Side = info.side;
                    CustomerPrice = info.customerPrice?.ToString("0.####");
                    BP = info.customerSpread;
                    Price = info.price?.ToString("0.####");

                    if (CommonUtils.SaleDic != null)
                    {
                        if (info.channel == "FINQ")
                        {
                            if (LocalEnv.UserInfo.RoleType == RoleType.BUYER_ROLE)
                            {
                                if (CommonUtils.SaleDic.ContainsKey("IsChecked_Broker_Spread") && CommonUtils.SaleDic["IsChecked_Broker_Spread"] == "1"
                                    && CommonUtils.SaleDic.ContainsKey("Broker_Spread") && CommonUtils.IsNumeric(CommonUtils.SaleDic["Broker_Spread"]))
                                {
                                    BP = CommonUtils.SaleDic["Broker_Spread"].ConvertToDecimal();
                                }
                            }
                            else
                            {
                                if (CommonUtils.SaleDic.ContainsKey("IsBp_FQ") && CommonUtils.SaleDic["IsBp_FQ"] == "1"
                                && CommonUtils.SaleDic.ContainsKey("Bp_FQ") && CommonUtils.IsNumeric(CommonUtils.SaleDic["Bp_FQ"]))
                                {
                                    BP = CommonUtils.SaleDic["Bp_FQ"].ConvertToDecimal();
                                }
                            }
                        }
                        else if (info.channel == "WECHAT")
                        {
                            if (CommonUtils.SaleDic.ContainsKey("IsBp_WX") && CommonUtils.SaleDic["IsBp_WX"] == "1"
                                && CommonUtils.SaleDic.ContainsKey("Bp_WX") && CommonUtils.IsNumeric(CommonUtils.SaleDic["Bp_WX"]))
                            {
                                BP = CommonUtils.SaleDic["Bp_WX"].ConvertToDecimal();
                            }
                        }
                        else if (info.channel == "CLIENT")
                        {
                            if (CommonUtils.SaleDic.ContainsKey("IsBp") && CommonUtils.SaleDic["IsBp"] == "1"
                                && CommonUtils.SaleDic.ContainsKey("Bp") && CommonUtils.IsNumeric(CommonUtils.SaleDic["Bp"]))
                            {
                                BP = CommonUtils.SaleDic["Bp"].ConvertToDecimal();
                            }
                        }
                    }

                    

                    Qty = info.qty.ToString();
                    Text = info.text;
                    if (info.channel == "WECHAT")
                    {
                        IsWX = "1";
                        Title = "收到来自企业微信的新报价";
                    }
                    else
                    {
                        IsWX = "0";
                        Title = "发来新报价";
                    }


                    HurryFlag = info.hurryFlag;
                    SuggestFlag = info.suggestFlag;
                    FireFlag = info.fireFlag;
                    FloorPrice = info.floorPrice;
                    OcoFlag = info.OcoFlag;

                    _currentInfo = info;
                });
            }));
        }
    }
}
