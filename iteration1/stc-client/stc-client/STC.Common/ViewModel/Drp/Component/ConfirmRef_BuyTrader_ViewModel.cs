using Ats.Foundation.Message;
using com.gateway.log;
using STC.Common.Command;
using STC.Common.Data;
using STC.Common.Model.TBook;
using STC.Common.Utils;
using STC.Common.View.Drp.Component;
using STC.Common.View.MyTrading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace STC.Common.ViewModel.Drp.Component
{
    public class ConfirmRef_BuyTrader_ViewModel : ViewModelBase
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


        public ConfirmRef_BuyTrader ParentWindow;

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

        public ICommand OkCommand { get; set; }

        public ICommand CancelCommand { get; set; }

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

        ///// <summary>
        ///// 销交
        ///// </summary>
        //private string _Dealer;

        //public string Dealer
        //{
        //    get { return _Dealer; }
        //    set
        //    {
        //        _Dealer = value;
        //        this.OnPropertyChanged();
        //    }
        //}

        ///// <summary>
        ///// 创建时间
        ///// </summary>
        //private string _CreateTime;

        //public string CreateTime
        //{
        //    get { return _CreateTime; }
        //    set
        //    {
        //        _CreateTime = value;
        //        this.OnPropertyChanged();
        //    }
        //}

        ///// <summary>
        ///// 债券代码
        ///// </summary>
        //private string _SecurityID;

        //public string SecurityID
        //{
        //    get { return _SecurityID; }
        //    set
        //    {
        //        _SecurityID = value;
        //        this.OnPropertyChanged();
        //    }
        //}

        ///// <summary>
        ///// 方向
        ///// </summary>
        //private string _Side;

        //public string Side
        //{
        //    get { return _Side; }
        //    set
        //    {
        //        _Side = value;
        //        this.OnPropertyChanged();
        //    }
        //}

        ///// <summary>
        ///// 客户报价
        ///// </summary>
        //private string _CustomerPrice;

        //public string CustomerPrice
        //{
        //    get { return _CustomerPrice; }
        //    set
        //    {
        //        _CustomerPrice = value;
        //        this.OnPropertyChanged();
        //    }
        //}

        ///// <summary>
        ///// 定价
        ///// </summary>
        //private string _Price;

        //public string Price
        //{
        //    get { return _Price; }
        //    set
        //    {
        //        if (value == null || value.ConvertToDecimal() < 0)
        //        {
        //            _Price = CustomerPrice;
        //        }
        //        else
        //        {
        //            _Price = value;
        //        }


        //        this.OnPropertyChanged();
        //    }
        //}

        ///// <summary>
        ///// 数量
        ///// </summary>
        //private string _Qty;

        //public string Qty
        //{
        //    get { return _Qty; }
        //    set
        //    {
        //        _Qty = value;
        //        this.OnPropertyChanged();
        //    }
        //}

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

        private string _investorName;

        public string InvestorName
        {
            get { return _investorName; }
            set 
            {
                _investorName = value;
                this.OnPropertyChanged();
            }
        }

        private string _refText;

        public string RefText
        {
            get { return _refText; }
            set
            {
                _refText = value;
                this.OnPropertyChanged();
            }
        }

        private STC.Common.Model.Table.WorkBrench.IntentionQuoteInfo _currentInfo;

        public ConfirmRef_BuyTrader_ViewModel()
        {
            this.OkCommand = new DelegateCommand(OnOkCommand);
            this.CancelCommand = new DelegateCommand(OnCancelCommand);
        }

        /// <summary>
        /// 确认
        /// </summary>
        /// <param name="inparam"></param>
        internal void OnOkCommand(object inparam)
        {
            if (this._currentInfo != null)
            {
                string exeStatus = IntentionOldHandler.Instance.ExtractByKey(this._currentInfo.subStatus, "MATCH_STATUS");
                if (exeStatus != "5")
                {
                    Common.View.Component.CustomMessageBox.Info("该个需已经匹配成交，不允许撤销", "同意");
                    return;
                }

                DesignHandler.Instance.AgreeRef(_currentInfo);
                //DoConfirmOne("82");
              
            }
        }
         
        /// <summary>
        /// 取消
        /// </summary>
        /// <param name="inparam"></param>
        private void OnCancelCommand(object inparam)
        {
            if (this._currentInfo != null)
            {
                DoConfirmOne("87");
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
            return item;
        }

        private Common.Model.Fix.Message.NewOrderSingle GetNewOrderSingle(MyTrading.IntentionQuoteItem item, string firmflag)
        {
            Common.Model.Fix.Message.NewOrderSingle request = new Common.Model.Fix.Message.NewOrderSingle();
            request.FirmFlag = firmflag;
            request.OrgType = item.orgType;
            request.VipCode = item.vipCode;
            request.IfSubBond = item.ifSubBond;
            request.AlgoName = "CROSS";
            request.VenueTypeGW = "INNER";
            request.Venues = "MANUAL";
            request.ClOrdID = item.ClOrdID;
            request.IntentionID = item.IntentionId;
            request.MarketIndicator = "4";
            request.OrdType = "2";
            request.OrderQty = item.Qty.ToString();
            request.Price = item.Price.ToString();
            request.PriceType = item.PriceType; //收益率
            //request.NetPrice = ;
            request.SecurityID = item.SecurityId;
            request.Symbol = item.Symbol;
            //request.OrdStatus = ;
            request.RiskManualCk = "";
            request.SettlType = item.SettlType;
            request.Side = item.Side;
            request.Terminal = string.Format("STClient_v{0}", LocalEnv.ServerInfo.Version); ;
            request.TimeInForce = "0";
            request.Name = request.UserName = LocalEnv.UserInfo.UserName;
            request.WholeFlag = item.WholeFlag;
            request.HandlInst = "1";
            //request.QuantID = ;
            request.MacID = EngineHelper.GetMacByNetworkInterface();
            request.InsideIP = EngineHelper.GetIp();
            request.HardDiskID = EngineHelper.Getharddiskid2();
            // 默认时间
            request.ValidUntilTime = item.validUntilTime;
            request.Info1 = item.Info1;
            request.InstitutionID = item.InstitutionId;
            request.InstitutionName = item.InstitutionName;
            request.TraderID = item.TraderId;
            request.TraderName = item.TraderName;
            request.UserName = item.UserName;
            request.AccountID = item.AccountID;
            request.AccountName = item.AccountName;
            request.Text = item.Text;
            request.Info2 = item.Info2;
            return request;
        }

        //private System.Threading.Tasks.Task<bool> UpdateIntention(MyTrading.IntentionQuoteItem item)
        //{
            //STC.Common.Model.Table.WorkBrench.UpdateIntentionQuoteRequest updateRequest = new STC.Common.Model.Table.WorkBrench.UpdateIntentionQuoteRequest();
            //updateRequest.intentionId = item.IntentionId;

            //if (CommonUtils.IsNumeric(this.Price))
            //{
            //    updateRequest.price = this.Price.ConvertToDecimal();
            //}

            //if (CommonUtils.IsNumeric(this.CustomerPrice))
            //{
            //    updateRequest.customerPrice = this.CustomerPrice.ConvertToDecimal();
            //}

            //if (CommonUtils.IsNumeric(this.Qty))
            //{
            //    updateRequest.qty = this.Qty.ConvertToDecimal();
            //}


            //updateRequest.updateTime = DateTimeEx.ServerTime.ToString("yyyy-MM-dd HH:mm:ss.fff");

            //updateRequest.info2 = item.Info2;

            //return IntentionOldHandler.Instance.UpdateIntentionQuote(updateRequest);
        //}


        internal void DoConfirmOne(string actiontype)
        {
            STC.Common.Model.Table.WorkBrench.ConfirmIntentionQuoteRequest request = new STC.Common.Model.Table.WorkBrench.ConfirmIntentionQuoteRequest();
            request.intentionId = _currentInfo.intentionId;
            request.actionType = actiontype;
            request.userName = _currentInfo.userName;
            request.closeBy = LocalEnv.UserInfo.UserName;
            request.closeByName = LocalEnv.UserInfo.FullName;
            if (LocalEnv.UserInfo.RoleType == RoleType.BUYER_ROLE)
            {
                request.quoteType = "BUYER_INTENTION";
            }
            else
            {
                request.quoteType = "INTENTION";
            }
            MyTrading.IntentionQuoteItem item = GetIntentionQuoteItem(_currentInfo);

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
                    orderDic[request.intentionId] = item;

                    GetErrorInfo(request,
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

                    InvestorName = $"{info.partyManagerName}申请:";

                    string side = ComboSourceUtil.Instance.FindEnum(info.side, "side");
                    string price = info.price.HasValue ? $" {info.price?.ToString("0.00##")}%" : string.Empty;
                    string qty = info.qty.HasValue ? $" {info.qty?.ToString("0")}" : string.Empty;

                    RefText = $"REF! {info.securityId} {info.symbol} {side}{price}{qty}";
                    
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
                    InvestorName = $"{info.partyManagerName}申请:";

                    string side = ComboSourceUtil.Instance.FindEnum(info.side, "side");
                    string price = info.price.HasValue ? $" {info.price?.ToString("0.00##")}%" : string.Empty;
                    string qty = info.qty.HasValue ? $" {info.qty?.ToString("0")}" : string.Empty;

                    RefText = $"REF! {info.securityId} {info.symbol} {side}{price}{qty}";

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

                    _currentInfo = info;
                });
            }));
        }
    }
}
