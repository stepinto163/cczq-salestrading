using Ats.Foundation.Message;
using com.gateway.log;
using Newtonsoft.Json;
using STC.Common.Data;
using STC.Common.Handler.Admin;
using STC.Common.Handler.Cal;
using STC.Common.Handler.PP;
using STC.Common.Handler.PT;
using STC.Common.Handler.SystemParam;
using STC.Common.Handler.Table;
using STC.Common.Model;
using STC.Common.Model.Base;
using STC.Common.Model.Cal;
using STC.Common.Model.CRM;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.PP;
using STC.Common.Model.PT;
using STC.Common.Model.Table;
using STC.Common.Model.TBook;
using STC.Common.Utils;
using STC.Common.View.Component;
using STC.Common.View.Order;
using STC.Common.View.UI;
using STC.Common.ViewModel.Data;
using STC.PT.Plugin.Handler.Process;
using STC.PT.Plugin.View.Component;
using STC.PT.Plugin.ViewModel.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using static STC.Common.View.Component.AutoCompleteComboBox;
using WtUtils = STC.PT.Plugin.Utils.CommonUtils;

namespace STC.PT.Plugin.ViewModel
{
    public class BatchRecognizeViewModel : ViewModelBase
    {
        #region 字段定义
        private Dictionary<string, string> _sideDic = new Dictionary<string, string>();
        public Dictionary<string, string> SideDic
        {
            get
            {
                if (_sideDic.Count == 0)
                {
                    _sideDic["1"] = "买入";
                    _sideDic["2"] = "卖出";
                }
                return _sideDic;
            }
        }

        private Dictionary<string, string> _TrdSubTypeItems = new Dictionary<string, string>();
        public Dictionary<string, string> TrdSubTypeItems
        {
            get
            {
                if (_TrdSubTypeItems.Count == 0)
                {
                    _TrdSubTypeItems["1"] = "我方发起";
                    _TrdSubTypeItems["2"] = "对方发起";
                }
                return _TrdSubTypeItems;
            }
        }


        private Dictionary<string, string> _DealWayItems = new Dictionary<string, string>();
        public Dictionary<string, string> DealWayItems
        {
            get
            {
                if (_DealWayItems.Count == 0)
                {
                    _DealWayItems["1"] = "对话";
                    _DealWayItems["2"] = "请求";
                    _DealWayItems["6"] = "固收";
                    _DealWayItems["7"] = "竞价";
                }
                return _DealWayItems;
            }
        }


        private Dictionary<string, string> _SettlTypeDic = new Dictionary<string, string>();
        public Dictionary<string, string> SettlTypeDic
        {
            get
            {
                if (_SettlTypeDic.Count == 0)
                {
                    _SettlTypeDic["1"] = "T+0";
                    _SettlTypeDic["2"] = "T+1";
                }
                return _SettlTypeDic;
            }
        }

        private bool _isVip = false;

        public bool IsVip
        {
            get
            {
                return _isVip;
            }
            set
            {
                if (_isVip != value)
                {
                    _isVip = value;
                    this.OnPropertyChanged();
                }
            }
        }

        #region 有效时长

        private int validSelectIndex = 0;

        public int ValidSelectIndex
        {
            get
            {
                return validSelectIndex;
            }

            set
            {
                validSelectIndex = value;
                if (validSelectIndex == 0)
                {
                    DefaultValidTimeQuotationCheck = true;
                }
                else if (validSelectIndex == 1)
                {
                    HourValidTimeQuotationCheck = true;
                }
                else if (validSelectIndex == 2)
                {
                    TimeValidTimeQuotationCheck = true;
                }
                else if (validSelectIndex == 3)
                {
                    NoLimitValidTimeQuotationCheck = true;
                }

                this.OnPropertyChanged();
            }
        }

        private int defaultValidUntilTime = 30;
        public int DefaultValidUntilTime
        {
            get
            {
                return defaultValidUntilTime;
            }

            set
            {
                defaultValidUntilTime = value;
                this.OnPropertyChanged();
            }
        }



        private bool defaultValidTimeQuotationCheck = true;
        /// <summary>
        /// 默认
        /// </summary>
        public bool DefaultValidTimeQuotationCheck
        {
            get
            {
                return defaultValidTimeQuotationCheck;
            }

            set
            {
                if (!NoLimitValidTimeQuotationCheck)
                {
                    if (!(HourValidTimeQuotationCheck ^ TimeValidTimeQuotationCheck))
                    {
                        return;
                    }
                }

                defaultValidTimeQuotationCheck = value;
                if (defaultValidTimeQuotationCheck)
                {
                    HourValidTimeQuotationCheck = TimeValidTimeQuotationCheck = NoLimitValidTimeQuotationCheck = false;
                    ValidMinute = ValidSecond = "";
                    ValidTimeHour = ValidTimeMinute = ValidTimeSecond = "";
                }

                this.OnPropertyChanged();
            }
        }

        private bool hourValidTimeQuotationCheck = false;
        /// <summary>
        /// 时长
        /// </summary>
        public bool HourValidTimeQuotationCheck
        {
            get
            {
                return hourValidTimeQuotationCheck;
            }

            set
            {
                if (!NoLimitValidTimeQuotationCheck)
                {
                    if (!(DefaultValidTimeQuotationCheck ^ TimeValidTimeQuotationCheck))
                    {
                        return;
                    }
                }
                hourValidTimeQuotationCheck = value;
                if (hourValidTimeQuotationCheck)
                {
                    DefaultValidTimeQuotationCheck = TimeValidTimeQuotationCheck = NoLimitValidTimeQuotationCheck = false;
                    ValidMinute = ValidSecond = "";
                    ValidTimeHour = ValidTimeMinute = ValidTimeSecond = "";
                }
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// 时长 - 分钟
        /// </summary>
        private string validMinute = string.Empty;
        public string ValidMinute
        {
            get { return validMinute; }
            set
            {
                if (value != null)
                {
                    validMinute = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        ///  时长 - 秒钟
        /// </summary>
        private string validSecond = string.Empty;
        public string ValidSecond
        {
            get { return validSecond; }
            set
            {
                if (value != null)
                {
                    validSecond = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 时点 - 小时
        /// </summary>
        private string validTimeHour = string.Empty;
        public string ValidTimeHour
        {
            get { return validTimeHour; }
            set
            {
                if (value != null)
                {
                    validTimeHour = value;
                    this.OnPropertyChanged();
                }
            }
        }
        /// <summary>
        /// 时点 - 分钟
        /// </summary>
        private string validTimeMinute = string.Empty;
        public string ValidTimeMinute
        {
            get { return validTimeMinute; }
            set
            {
                if (value != null)
                {
                    validTimeMinute = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 时点 - 秒钟
        /// </summary>
        private string validTimeSecond = string.Empty;
        public string ValidTimeSecond
        {
            get { return validTimeSecond; }
            set
            {
                if (value != null)
                {
                    validTimeSecond = value;
                    this.OnPropertyChanged();
                }
            }
        }


        private bool timeValidTimeQuotationCheck = false;
        /// <summary>
        /// 时点
        /// </summary>
        public bool TimeValidTimeQuotationCheck
        {
            get
            {
                return timeValidTimeQuotationCheck;
            }

            set
            {
                if (!NoLimitValidTimeQuotationCheck)
                {
                    if (!(DefaultValidTimeQuotationCheck ^ HourValidTimeQuotationCheck))
                    {
                        return;
                    }
                }
                timeValidTimeQuotationCheck = value;
                if (timeValidTimeQuotationCheck)
                {
                    DefaultValidTimeQuotationCheck = HourValidTimeQuotationCheck = NoLimitValidTimeQuotationCheck = false;
                    ValidMinute = ValidSecond = "";
                    ValidTimeHour = ValidTimeMinute = ValidTimeSecond = "";
                }
                this.OnPropertyChanged();
            }
        }

        private bool noLimitValidTimeQuotationCheck = false;
        /// <summary>
        /// 无
        /// </summary>
        public bool NoLimitValidTimeQuotationCheck
        {
            get
            {
                return noLimitValidTimeQuotationCheck;
            }

            set
            {
                noLimitValidTimeQuotationCheck = value;
                if (noLimitValidTimeQuotationCheck)
                {
                    DefaultValidTimeQuotationCheck = HourValidTimeQuotationCheck = TimeValidTimeQuotationCheck = false;
                    ValidMinute = ValidSecond = "";
                    ValidTimeHour = ValidTimeMinute = ValidTimeSecond = "";
                }
                this.OnPropertyChanged();
            }
        }

        #endregion



        private bool _IsAllChecked_Same = false;
        public bool IsAllChecked_Same
        {
            get { return _IsAllChecked_Same; }
            set
            {
                _IsAllChecked_Same = value;
                this.OnPropertyChanged();
            }
        }

        private bool _IsAllChecked_Opposite = false;

        public bool IsAllChecked_Opposite
        {
            get { return _IsAllChecked_Opposite; }
            set
            {
                _IsAllChecked_Opposite = value;
                this.OnPropertyChanged();
            }
        }

        private object _LockObject = new object();
        private CollectionViewSource viewSource = new CollectionViewSource();
        public CollectionViewSource ViewSource
        {
            get { return viewSource; }
            set { viewSource = value; }
        }

        private CustomCollection<IntentionQuoteItem> intentionQuoteInfo = new CustomCollection<IntentionQuoteItem>();

        public CustomCollection<IntentionQuoteItem> IntentionQuoteInfo
        {
            get { return intentionQuoteInfo; }
            set { intentionQuoteInfo = value; }
        }

        private IntentionQuoteItem _selectedItem_Recognize;
        public IntentionQuoteItem SelectedItem_Recognize
        {
            get
            {
                return this._selectedItem_Recognize;
            }

            set
            {
                this._selectedItem_Recognize = value;
                this.OnPropertyChanged();
            }
        }

        public ICommand DelOrderCommand { get; set; }

        private bool _isPublishBtnEnable;

        public bool IsPublishBtnEnable
        {
            get { return _isPublishBtnEnable; }
            set
            {
                _isPublishBtnEnable = value;
                this.OnPropertyChanged();
            }
        }

        private bool _isFirmPublishBtnEnable;

        public bool IsFirmPublishBtnEnable
        {
            get { return _isFirmPublishBtnEnable; }
            set
            {
                _isFirmPublishBtnEnable = value;
                this.OnPropertyChanged();
            }
        }
        private bool _isPublishBtnEnable_PA;

        public bool IsPublishBtnEnable_PA
        {
            get { return _isPublishBtnEnable_PA; }
            set
            {
                _isPublishBtnEnable_PA = value;
                this.OnPropertyChanged();
            }
        }
        private Visibility _ParserImageVisibility = Visibility.Collapsed;

        public Visibility ParserImageVisibility
        {
            get { return _ParserImageVisibility; }
            set
            {
                _ParserImageVisibility = value;
                this.OnPropertyChanged();
            }
        }
        public ICommand SaveCommand { get; set; }
        public ICommand RiskCheckCommand { get; set; }
        public event Action RequestClose;

        // 触发关闭窗口的方法
        public void CloseWindow()
        {
            RequestClose?.Invoke();
        }

        #endregion
        internal void UnregisterEvent()
        {
            IntentionOldHandler.Instance.OnRecognition -= Instance_OnRecoginition;
        }

        ConsistencyCheckPo consistency;

        public BatchRecognizeViewModel()
        {
            /*if (LocalEnv.ConfInfo.IsParserImage == "1")
            {
                ParserImageVisibility = Visibility.Visible;
            }*/
            ParserImageVisibility = Visibility.Collapsed;

            DelOrderCommand = new ComRelayCommand(DeleteOrder);
            SaveCommand = new ComRelayCommand(SaveOrderClick);
            RiskCheckCommand = new ComRelayCommand(RiskCheckClick);
            IntentionQuoteInfo.CollectionChanged += IntentionQuoteInfo_CollectionChanged;
            IntentionOldHandler.Instance.OnRecognition += Instance_OnRecoginition;
            this.ViewSource.IsLiveFilteringRequested = true;
            this.ViewSource.IsLiveSortingRequested = true;
            this.ViewSource.LiveFilteringProperties.Add("Status");
            this.ViewSource.Source = IntentionQuoteInfo;
            BindingOperations.EnableCollectionSynchronization(this.ViewSource.View, _LockObject);
            ParseValidTimeFormat();
            consistency = UserSettingAdapter.Instance.GetConsistencyCheckPo();
        }

        private void RiskCheckClick()
        {
            if (this.ViewSource.View != null)
            {
                List<NewOrderSingle> reqList = new List<NewOrderSingle>();
                foreach (IntentionQuoteItem item in this.ViewSource.View)
                {
                    NewOrderSingle req = new NewOrderSingle();
                    req.SecurityID = item.SecurityId;
                    req.Venues = "MANUAL";
                    req.VenueTypeGW = "INNER";
                    req.StrategyOrderId = item.ClOrdID;
                    req.ClOrdID = item.ClOrdID;
                    req.MarketIndicator = "4";
                    req.ActionFlag = "4";
                    req.Side = item.Side;
                    req.OrdStatus = "0";
                    //req.netprice = 
                    req.OrderQty = item.Qty.ToString();
                    req.Info1 = DateTime.Today.ToString("yyyyMMdd");
                    //req.RiskReTryFlag = "0";
                    req.AccountID = LocalEnv.UserInfo.UserName;
                    req.UserName = LocalEnv.UserInfo.UserName;
                    req.CounterPartyName = item.SelectedInstintuion_Edit != null ? item.SelectedInstintuion_Edit.fullnamecn : "";
                    req.Price = item.Price + "";
                    req.Info2 = item.SelectedInstintuion_Edit != null ? item.SelectedInstintuion_Edit.shortnamecn : "";
                    req.SettlType = item.SettlType;
                    req.Symbol = item.Symbol;
                    reqList.Add(req);
                }
                if (reqList.Count > 0)
                {
                    ThreadPool.QueueUserWorkItem(o =>
                    {
                        try
                        {
                            BatchCheckRiskReplyPo po = PPHandler.Instance.BatchCheckRisk(reqList);
                            if (po != null && po.Code == "10000" && po.Msg != null && po.Msg.Count > 0)
                            {
                                Application.Current.Dispatcher.Invoke(() =>
                                {
                                    bool isProhibit = false;
                                    bool isWarn = false;
                                    foreach (IntentionQuoteItem item in this.IntentionQuoteInfo)
                                    {
                                        string clordid = item.ClOrdID;
                                        if (po.Msg.ContainsKey(clordid))
                                        {
                                            RiskResult riskResult = po.Msg[clordid];
                                            if (riskResult != null)
                                            {
                                                StringBuilder sb = new StringBuilder();
                                                if (riskResult.FailMsg != null && riskResult.FailMsg.Count > 0)
                                                {
                                                    foreach (TPlaceOrderResult failMsg in riskResult.FailMsg)
                                                    {
                                                        if (failMsg.Code == "10001")
                                                        {
                                                            sb.AppendLine(failMsg.Reason);
                                                            isWarn = true;
                                                        }

                                                        if (failMsg.Code == "10002")
                                                        {
                                                            sb.AppendLine(failMsg.Reason);
                                                            isProhibit = true;
                                                        }
                                                    }
                                                }
                                                item.RiskNote = sb.ToString();// 风控检查备注
                                            }
                                        }
                                        if (isProhibit)
                                        {
                                            IsPublishBtnEnable_PA = false;
                                        }
                                        else if (isWarn)
                                        {
                                            IsPublishBtnEnable_PA = IsPublishBtnEnable;
                                        }
                                        else
                                        {
                                            IsPublishBtnEnable_PA = IsPublishBtnEnable;
                                        }
                                    }
                                });
                            }
                        }
                        catch (Exception e) 
                        { 

                        }
                    });
                }
            }
        }
        public bool check(IntentionQuoteItem item)
        {
            bool ischeck = false;
            if (string.IsNullOrWhiteSpace(item.SecurityId))
            {
                CustomMessageBox.Info(string.Format("第{0}行缺少债券!", item.No), "录入");
                return ischeck;
            }
            if (string.IsNullOrWhiteSpace(item.Side))
            {
                CustomMessageBox.Info(string.Format("第{0}行缺少方向!", item.No), "录入");
                return ischeck;
            }
            if (item.Price == null)
            {
                CustomMessageBox.Info(string.Format("第{0}行缺少成交价!", item.No), "录入");
                return ischeck;
            }
            if (item.Qty == null)
            {
                CustomMessageBox.Info(string.Format("第{0}行缺少量!", item.No),"录入");
                return ischeck;
            }
            if (string.IsNullOrWhiteSpace(item.SettlType))
            {
                CustomMessageBox.Info(string.Format("第{0}行缺少清算速度!", item.No), "录入");
                return ischeck;
            }
            if (item.TrdSubType == null)
            {
                CustomMessageBox.Info(string.Format("第{0}行缺少发单方向!", item.No), "录入");
                return ischeck;
            }
            if (item.DealWay == null)
            {
                CustomMessageBox.Info(string.Format("第{0}行缺少发单方式!", item.No), "录入");
                return ischeck;
            }
            /*if (string.IsNullOrWhiteSpace(item.CounterPartyName_Edit))
            {
                CustomMessageBox.Info(string.Format("第{0}行缺少对方机构!", item.No), "录入");
                return ischeck;
            }*/
            ischeck = true;
            return ischeck;
        }
        private void SaveOrderClick()
        {
            ThreadPool.QueueUserWorkItem(o =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (this.ViewSource.View != null)
                    {
                        List<PtExecOrderSaveReq> reqList = new List<PtExecOrderSaveReq>();
                        foreach (IntentionQuoteItem item in this.ViewSource.View)
                        {
                            bool ischeck = check(item);
                            if (!ischeck) 
                            { 
                                break;
                            }
                            PtExecOrderSaveReq req = new PtExecOrderSaveReq();
                            req.securityid = item.SecurityId;
                            req.symbol = item.Symbol;
                            req.venues = "MANUAL";
                            req.side = item.Side;
                            req.yield = CommonUtils.ConvertToString(item.Price);
                            req.price = CommonUtils.ConvertToString(item.Price);
                            if (item.SecurityId.EndsWith(".SH"))
                            {
                                req.netprice = item.CleanPrice.HasValue? item.CleanPrice.Value.ToString("F3"):"";
                            }
                            else 
                            {
                                req.netprice = CommonUtils.ConvertToString(item.CleanPrice);
                            }
                            if (item.Yieldtoexecution.HasValue)
                            {
                                req.exePrice = CommonUtils.ConvertToString(item.Yieldtoexecution);
                            }
                            req.fullPrice = CommonUtils.ConvertToString(item.FullPrice);
                            req.qty = item.Qty.ToString();
                            req.settltype = "1".Equals(item.SettlType)?"+0":"+1";
                            req.trdSubType = item.TrdSubType;
                            req.trdType = item.DealWay;
                            req.bridgePaymentFixedRate = item.PaymentRate;
                            req.counterpartyid = item.SelectedInstintuion_Edit != null ?item.SelectedInstintuion_Edit.idbkey:"";
                            req.counterpartyname = item.CounterPartyName_Edit;
                            req.counterpartyfullname = item.SelectedInstintuion_Edit != null ? item.SelectedInstintuion_Edit.fullnamecn : "";
                            req.realcounterpartyname = string.IsNullOrEmpty(item.RealCounterPartyName)? item.CounterPartyName_Edit: item.RealCounterPartyName;
                            req.name = LocalEnv.UserInfo.FullName;
                            req.username = LocalEnv.UserInfo.UserName;
                            req.partyid = PTExecOrderProcesser.Instance.partyId;
                            req.partyname = PTExecOrderProcesser.Instance.partyName;
                            req.partysubname = LocalEnv.UserInfo.FullName;
                            req.partysubid = LocalEnv.UserInfo.UserId;
                            req.tradedate = string.IsNullOrEmpty(item.TradeDate)? DateTime.Now.ToString("yyyyMMdd"):item.TradeDate;
                            req.traderid = LocalEnv.UserInfo.UserName;
                            req.tradername = LocalEnv.UserInfo.FullName;
                            req.allocText = item.AllocText;
                            req.yield2 = item.Yield2.HasValue ? CommonUtils.ConvertToString(item.Yield2.Value) : "";
                            req.cleanPrice2 = item.NetPrice2.HasValue ? CommonUtils.ConvertToString(item.NetPrice2.Value) : "";
                            req.fullPrice2 = item.FullPrice2.HasValue ? CommonUtils.ConvertToString(item.FullPrice2.Value) : "";
                            req.exePrice2 = item.Yieldtoexecution2.HasValue ? CommonUtils.ConvertToString(item.Yieldtoexecution2.Value) : "";
                            req.settlAmount = item.SettlAmount.HasValue ? CommonUtils.ConvertToString(item.SettlAmount.Value) : "";
                            req.trdMatchId = item.TrdMatchId;
                            req.counterpartysubname = item.CounterPartySubName;
                            req.sseinstitutioncode = item.Sseinstitutioncode;
                            req.szedealercode = item.Szedealercode;
                            req.szetradingentitycode = item.Szetradingentitycode;
                            req.szetradercode = item.Szetradercode;
                            req.RiskManualCk = "2";
                            reqList.Add(req);
                        }
                        if (reqList.Count > 0)
                        {
                            PResult result = PTHandler.Instance.ExecOrderBatchSave(reqList);
                            if (result != null && "0000".Equals(result.Code))
                            {
                                CloseWindow();
                            }
                            else
                            {
                                showMessage(string.Format("{0}失败{1}", "新增", result != null ? result.Msg : ""), false);
                            }
                        }
                    }
                });
            });
        }
        /// <summary>
        /// 提示消息弹框
        /// </summary>
        /// <param name="message"></param>
        private void showMessage(string message, bool autoClose)
        {
            MessageBoxGUI me = new MessageBoxGUI();
            me.HideCancelButton();
            me.setWarnMsg(message, autoClose);
            me.ShowDialog();
        }
        private void IntentionQuoteInfo_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            IsFirmPublishBtnEnable = IsPublishBtnEnable = this.intentionQuoteInfo.Count > 0 ? true : false;

            if (IsFirmPublishBtnEnable)
            {
                foreach (IntentionQuoteItem data in this.IntentionQuoteInfo)
                {
                    if (!data.Price.HasValue || !data.Qty.HasValue)
                    {
                        IsFirmPublishBtnEnable = false;
                        break;
                    }
                }
            }
            SetOppositeCheckAll();
            SetSameCheckAll();
        }

        internal void ClickOppositeCheckAll(bool flag)
        {
            foreach (IntentionQuoteItem item in this.intentionQuoteInfo)
            {
                item.IsOpposite = flag;
            }
        }

        internal void ClickSameCheckAll(bool flag)
        {
            foreach (IntentionQuoteItem item in this.intentionQuoteInfo)
            {
                item.IsSame = flag;
            }
        }

        internal void SetOppositeCheckAll()
        {
            bool flag = true;
            foreach (IntentionQuoteItem item in this.intentionQuoteInfo)
            {
                flag = flag && item.IsOpposite;
            }
            this.IsAllChecked_Opposite = this.intentionQuoteInfo.Count > 0 && flag;
        }

        internal void SetSameCheckAll()
        {
            bool flag = true;
            foreach (IntentionQuoteItem item in this.intentionQuoteInfo)
            {
                flag = flag && item.IsSame;
            }
            this.IsAllChecked_Same = this.intentionQuoteInfo.Count > 0 && flag;
        }

        private void Instance_OnRecoginition(STC.Common.Model.Table.WorkBrench.IntentionQuoteInfo obj)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() => {
                AddRecognize(obj);
                //IntentionQuoteItem item = null;
                //var findList = IntentionQuoteInfo.Where(x => x.IntentionId == obj.intentionId);
                //if (findList != null && findList.Count() > 0)
                //{
                //    item = findList.First();
                //    if (!string.IsNullOrEmpty(obj.userName) && obj.userName != LocalEnv.UserInfo.UserName
                //        && obj.status != "0")
                //    {
                //        this.IntentionQuoteInfo.Remove(item);
                //        return;
                //    }

                //    item.Status = obj.status;

                //    item.InstitutionId = obj.institutionId;
                //    item.InstitutionName = obj.institutionName;
                //    item.TraderId = obj.traderId;
                //    item.TraderName = obj.traderName;
                //    item.Side = obj.side;
                //    item.Price = obj.price;
                //    item.Qty = obj.qty;
                //    item.SettlType = obj.settlType;
                //    item.UpdateTime = obj.updateTime;
                //    item.UserName = obj.userName;
                //    item.CloseBy = obj.closeBy;
                //    item.OrderID = obj.orderId;
                //    item.QqNumber = obj.qqNumber;

                //}

            }));
        }

        internal void ClearList()
        {
            if (this.intentionQuoteInfo != null)
            {
                this.IntentionQuoteInfo.Clear();
            }
        }

        private bool isStartRecog = false;
        private IntentionQuoteItem AddRecognize(STC.Common.Model.Table.WorkBrench.IntentionQuoteInfo obj)
        {
            if (this.IntentionQuoteInfo.Any(entry => entry.ClOrdID == obj.clordid))
            {
                return this.IntentionQuoteInfo.First(entry => entry.ClOrdID == obj.clordid);
            }

            if (isStartRecog)
            {
            }


            IntentionQuoteItem item = new IntentionQuoteItem();
            if (consistency != null)
            {
                item.IsOpposite = UserSettingAdapter.Instance.ParseCheckedType(consistency.SubscribeReverse);
                item.IsSame = UserSettingAdapter.Instance.ParseCheckedType(consistency.SubscribeSyntropy);
            }
            item.IntentionId = obj.intentionId;
            item.InstitutionId = obj.institutionId;
            item.InstitutionName = obj.institutionName;
            item.TraderId = obj.traderId;
            item.TraderName = obj.traderName;
            item.QqNumber = obj.qqNumber;
            item.Side = obj.side;
            item.SecurityId = obj.securityId;
            item.Symbol = obj.symbol;
            item.CleanPrice = obj.cleanPrice;
            item.FullPrice = obj.fullPrice;
            item.Price = obj.price;
            //item.ClOrdID = obj.clordid;
            decimal qty = obj.qty.ConvertToDecimal() / 100;
            if (qty >= 1)
            {
                item.Qty = qty * 100;
            }
            //item.Qty = obj.qty;

            item.SettlType = obj.settlType;
            //string tradeDate = CommonUtils.CalTradeDate(obj.settlType);
            //item.edit_settltype_date = CommonUtils.ConvertToDateTime2(tradeDate);
            //DateTime settlDateTime;
            //int selectIndex;
            //CommonUtils.SetSettleType(obj.settlType, tradeDate, out settlDateTime, out selectIndex);
            //item.edit_settletype = selectIndex;

            //item.BrokerId = obj.brokerId;
            //item.BrokerName = obj.brokerName;
            item.Text = obj.text;
            item.CreateTime = obj.createTime;
            item.UpdateTime = obj.updateTime;
            item.UserName = obj.userName;
            item.CloseBy = obj.closeBy;
            item.CloseByName = obj.closeByName;
            item.Status = obj.status;
            item.OrderID = obj.orderId;
            item.WholeFlag = obj.wholeFlag;
            item.ClOrdID = obj.clordid;
            item.TimeToMaturity = obj.timeToMaturity;
            item.TimeToMaturityToWorkDay = obj.timeToMaturityToWorkDay;
            item.TimeToMaturity_Convert = Utils.CommonUtils.ConvertRemainingTime(obj.timeToMaturity);
            item.TrdSubTypeItem_Edit = "1";
            item.TrdSubType = "1";
            item.DealWayItem_Edit = SetDefultDealWay(item.SecurityId, item.CounterPartySubName);
            item.DealWay = SetDefultDealWay(item.SecurityId, item.CounterPartySubName);

            try
            {
                //if (!_symbolCountDic.ContainsKey(er.securityid))
                //{
                //    _symbolCountDic[er.securityid] = null;
                //    LogHandler.Info($"InnerBoardViewModel UpdateInnerOrder RegisterCTPrice :{er.securityid}.");
                //    PriceHandler.Instance.RegisterCTPrice(er.securityid, UpdateCtPrice);
                //}

                CCDCValuaModel ccdcValueModel = CommonUtils.GetCCDCValua(obj.securityId);
                if (ccdcValueModel != null && !string.IsNullOrEmpty(ccdcValueModel.CCDCValuation))
                {
                    item.CCDCValuation = ccdcValueModel.CCDCValuation.ConvertToDecimal();
                    item.CCDCValuation_Date = ccdcValueModel.CCDCValuaDate;
                    item.ShowLastWorkDay = ccdcValueModel.ShowLastWorkDay;
                }
                else
                {
                    PriceHandler.Instance.RegisterCTPrice(obj.securityId, UpdateCtPrice);
                }
            }
            catch (Exception e)
            {
                LogHandler.Error(e);
            }
            item.PriceType = obj.priceType;
            /*if ("1".Equals(item.Side)) 
            {
                item.DealSide = "1";
            }
            else
            {
                item.DealSide = "2";
            }*/
            /*List<ComboxBaseModel> sideList = new List<ComboxBaseModel>();
            ComboxBaseModel MeSide = new ComboxBaseModel()
            {
                DisplayMemberPath = "我方发起",
                ValueMemberPath = "1",
                KeyMemberPath = "MeSide"

            };
            ComboxBaseModel CounterSide = new ComboxBaseModel()
            {
                DisplayMemberPath = "对方发起",
                ValueMemberPath = "2",
                KeyMemberPath = "CounterSide"
            };
            sideList.Add(MeSide);
            sideList.Add(CounterSide);
            item.TrdSubTypeItems = sideList;
            item.TrdSubTypeItem = MeSide;*/
            item.DealWayVisible = STC.PT.Plugin.Utils.CommonUtils.IsBank(obj.securityId)?"false":"true";
            item.PartyName = PTExecOrderProcesser.Instance.partyName;
            /*if (item.DealWayVisible.Equals("true"))
            {
                List<ComboxBaseModel> list = new List<ComboxBaseModel>();
                ComboxBaseModel model = new ComboxBaseModel()
                {
                    DisplayMemberPath = "对话",
                    ValueMemberPath = "1",
                    KeyMemberPath = "Dialogue"

                };
                ComboxBaseModel model2 = new ComboxBaseModel()
                {
                    DisplayMemberPath = "请求",
                    ValueMemberPath = "2",
                    KeyMemberPath = "Request"
                };
                ComboxBaseModel model3 = new ComboxBaseModel()
                {
                    DisplayMemberPath = "固收",
                    ValueMemberPath = "6",
                    KeyMemberPath = "Fixed"
                };
                ComboxBaseModel model4 = new ComboxBaseModel()
                {
                    DisplayMemberPath = "竞价",
                    ValueMemberPath = "7",
                    KeyMemberPath = "Bidding"
                };
                list.Add(model);
                list.Add(model2);
                list.Add(model3);
                list.Add(model4);
                item.DealWayItems = list;
                item.DealWaySelect = model;
            }*/
            item.PropertyChanged += Item_PropertyChanged;

            this.IntentionQuoteInfo.Add(item);

            return item;
        }
        public string SetDefultDealWay(string securityid,string CounterPartySubNameText)
        {
            if (securityid.ToUpper().EndsWith(".IB"))
            {
                if (string.IsNullOrEmpty(CounterPartySubNameText))
                {
                    return "2";//对话
                }
                else
                {
                    return "1";//请求
                }
            }
            else if (securityid.ToUpper().EndsWith(".SH") || securityid.ToUpper().EndsWith(".SZ"))
            {
                return "6";//固收
            }
            else { 
                return "";
            }
        }
        private void UpdateCtPrice(MarketDataSnapshotFullRefresh price)
        {
            if (System.Windows.Application.Current != null)
            {
                System.Windows.Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    CCDCValuaModel srcccdc = null;

                    try
                    {
                        srcccdc = CommonUtils.GetCCDCValua(price.SecurityID);
                        if (srcccdc == null)
                        {
                            srcccdc = CommonUtils.GenerateCCDCValue(price);

                            CommonUtils.SetCCDCValua(price.SecurityID, srcccdc);
                        }
                    }
                    catch (Exception e)
                    {
                        LogHandler.Error(e);
                    }

                    lock (_LockObject)
                    {
                        IEnumerable<IntentionQuoteItem> findDatas_best = IntentionQuoteInfo.Where(item => item.SecurityId == price.SecurityID);
                        foreach (IntentionQuoteItem findData in findDatas_best)
                        {
                            findData.CCDCValuation = srcccdc.CCDCValuation.ConvertToDecimal();
                            findData.CCDCValuation_Date = srcccdc.CCDCValuaDate;
                            findData.ShowLastWorkDay = srcccdc.ShowLastWorkDay;
                        }
                    }

                }));
            }
            PriceHandler.Instance.UnregisterCTPrice(price.SecurityID, UpdateCtPrice);
        }

        private void Item_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Qty"
                || e.PropertyName == "Price")
            {
                IsFirmPublishBtnEnable = IsPublishBtnEnable = this.intentionQuoteInfo.Count > 0 ? true : false;

                if (IsFirmPublishBtnEnable)
                {
                    foreach (IntentionQuoteItem data in this.IntentionQuoteInfo)
                    {
                        if (!data.Price.HasValue || !data.Qty.HasValue)
                        {
                            IsFirmPublishBtnEnable = false;
                            break;
                        }
                    }
                }
            }
        }

        public void RecognizeIntentionQuote(string content)
        {
            isStartRecog = true;
            System.Threading.ThreadPool.QueueUserWorkItem(o =>
            {
                try
                {
                    string pattern = @"(\r\n)+";
                    string txtcont = System.Text.RegularExpressions.Regex.Replace(content, pattern, "\r\n");
                    /*STC.Common.Model.Table.WorkBrench.RecognizeIntentionQuoteRequest request = new STC.Common.Model.Table.WorkBrench.RecognizeIntentionQuoteRequest();
                    request.userName = LocalEnv.UserInfo.UserName;
                    request.content = txtcont;*/
                    RecognizeDealReq req = new RecognizeDealReq();
                    req.user = LocalEnv.UserInfo.UserName;
                    req.my_side_company_name = PTExecOrderProcesser.Instance.partyName;
                    req.prompt = txtcont;
                    ///获取订单 
                    // STC.Common.Model.Table.WorkBrench.RecognizeIntentionQuoteReplyPo reply = TableOldHandler.Instance.RecognizeIntentionQuote(request);

                    RecognizeDealReply reply = ParserHandler.Instance.RequestParserDeal(req);

                    if (reply == null)
                    {
                        CommonUtils.Prompt2Second("警告", $"报价识别失败");
                    }
                    else
                    {
                        if (!reply.IsSuccess())
                        {
                            CommonUtils.Prompt2Second("警告", reply.Msg);
                        }
                        else
                        {
                            if (reply.Datas != null)
                            {
                                Application.Current.Dispatcher.BeginInvoke((Action)(async () =>
                                {
                                    int index = 0;
                                    foreach (TradeInfo obj in reply.Datas)
                                    {
                                        IntentionQuoteItem item = AddRecognizeDeal(obj);
                                        item.No = ++index;
                                        await System.Windows.Threading.Dispatcher.Yield();
                                    }
                                    /*foreach (STC.Common.Model.Table.WorkBrench.IntentionQuoteInfo obj in reply.Datas)
                                    {
                                        IntentionQuoteItem item = AddRecognize(obj);
                                        item.No = ++index;
                                        await System.Windows.Threading.Dispatcher.Yield();
                                    }*/
                                    RiskCheckClick();
                                }));
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    LogHelper.Error("RequestOrders", e);
                }
            });
        }


        public void RecognizeIntentionQuoteImage(string content)
        {
            isStartRecog = true;
            System.Threading.ThreadPool.QueueUserWorkItem(o =>
            {
                try
                {
                    STC.Common.Model.Table.WorkBrench.RecognizeIntentionQuoteRequest request = new STC.Common.Model.Table.WorkBrench.RecognizeIntentionQuoteRequest();
                    request.userName = LocalEnv.UserInfo.UserName;
                    request.content = content;
                    ///获取订单 
                    // STC.Common.Model.Table.WorkBrench.RecognizeIntentionQuoteReplyPo reply = TableOldHandler.Instance.RecognizeIntentionQuote(request);

                    STC.Common.Model.Table.WorkBrench.RecognizeIntentionQuoteReplyPo reply = ParserHandler.Instance.RequestParserImage(request);

                    if (reply == null)
                    {
                        CommonUtils.Prompt2Second("警告", $"报价识别失败");
                    }
                    else
                    {
                        if (!reply.IsSuccess())
                        {
                            CommonUtils.Prompt2Second("警告", reply.Msg);
                        }
                        else
                        {
                            if (reply.Datas != null)
                            {
                                Application.Current.Dispatcher.BeginInvoke((Action)(async () =>
                                {
                                    int index = 0;
                                    foreach (STC.Common.Model.Table.WorkBrench.IntentionQuoteInfo obj in reply.Datas)
                                    {
                                        IntentionQuoteItem item = AddRecognize(obj);

                                        item.No = ++index;
                                        await System.Windows.Threading.Dispatcher.Yield();
                                    }

                                    RiskCheckClick();
                                }));
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    LogHelper.Error("RequestOrders", e);
                }
            });
        }

        /// <summary>
        /// 编辑记录
        /// </summary>
        /// <param name="editBtn"></param>
        private void EditRow(Button editBtn)
        {
            if (editBtn != null && editBtn.DataContext is IntentionQuoteItem)
            {
                IntentionQuoteItem item = editBtn.DataContext as IntentionQuoteItem;
                EditRow(item);
            }
        }

        internal void EditRow(IntentionQuoteItem item)
        {
            item.IsEdit = "0";
            item.InstitutionName_Edit = item.InstitutionName;
            //item.InstitutionName_Text = item.InstitutionName;
            item.TraderName_Edit = item.TraderName;
            //item.TraderName_Text = item.TraderName;
            item.Side_Edit = item.Side;
            item.Price_Edit = item.Price;
            item.Qty_Edit = item.Qty;
            item.SettlType_Edit = item.SettlType;
            item.WholeFlag_Edit = item.WholeFlag;
            item.Yieldtoexecution_Edit = item.Yieldtoexecution;
            item.RealCounterPartyName_Edit = item.RealCounterPartyName;
            item.CounterPartySubName_Edit = item.CounterPartySubName;
            item.CleanPrice_Edit =CommonUtils.ConvertToString(item.CleanPrice);
            item.FullPrice_Edit = CommonUtils.ConvertToString(item.FullPrice);
            item.PaymentRate_Edit = item.PaymentRate;
            //string tradeDate = CommonUtils.CalTradeDate(item.SettlType);
            //item.edit_settltype_date = CommonUtils.ConvertToDateTime2(tradeDate);
            //DateTime settlDateTime;
            //int selectIndex;
            //CommonUtils.SetSettleType(item.SettlType, tradeDate, out settlDateTime, out selectIndex);
            //item.edit_settletype = selectIndex;
        }

        public static string COLUMNNAME_INSTITUTION = "发单机构";
        public static string COLUMNNAME_TRADER = "客户名";
        public static string COLUMNNAME_SIDE = "我方方向";
        public static string COLUMNNAME_PRICE = "成交价(%)";
        public static string COLUMNNAME_Yieldtoexecution = "成交价(权%)";
        public static string COLUMNNAME_QTY = "成交量";
        public static string COLUMNNAME_SETTLETYPE = "清算速度";
        public static string COLUMNNAME_PaymentRate = "代付费率";
        public static string COLUMNNAME_TrdSubType= "发单方向";
        public static string COLUMNNAME_DealWaySelect = "发单方式";
        public static string COLUMNNAME_Realcounterpartyname = "终端对手方";
        public static string COLUMNNAME_CounterPartySubName = "对手方交易员";
        public static string COLUMNNAME_CleanPrice = "净价";
        public static string COLUMNNAME_FullPrice = "全价";

        internal void SaveRow(IntentionQuoteItem item, string columnHeader = null)
        {
            item.IsEdit = "2";
            // 价格
            decimal? price = null;
            // 行权价格
            decimal? yieldToExe = null;
            // 净价
            decimal? cleanPrice = null;
            // 全价
            decimal? fullPrice = null;
            // 报量
            decimal? qty = null;
            // 整量
            string wholeFlag = null;
            // 清算速度
            string settlType = null;
            // 交易员编号
            string traderId = null;
            // 交易员名称
            string traderName = null;
            // 经纪商编号
            string brokerId = null;
            // 经纪商名称
            string brokerName = null;
            // 机构编号
            string institutionId = null;
            // 机构名称
            string institutionName = null;
            // 方向
            string side = null;
            // 发单方向
            string trdSubType = null;
            // 发单方式
            string dealWay = null;
            // 代付费率
            string paymentrate = null;
            //终端对手方
            string realcounterpartyname = null;
            //对手方交易员
            string counterPartySubName = null;
            // 发单机构
            if (columnHeader == null || columnHeader == COLUMNNAME_INSTITUTION)
            {
                if (item.SelectedInstintuion_Edit != null)
                {
                    institutionId = item.SelectedInstintuion_Edit.idbkey;
                    institutionName = item.SelectedInstintuion_Edit.shortnamecn;
                    if (institutionId == item.CounterPartyId)
                        return;
                }
            }
            //对手方交易员
            if (columnHeader == null || columnHeader == COLUMNNAME_CounterPartySubName)
            {
                if (item.CounterPartySubName != null)
                {
                    counterPartySubName = item.CounterPartySubName_Edit;
                    if (counterPartySubName == item.CounterPartySubName)
                        return;
                }
            }
            //终端对手方
            if (columnHeader == null || columnHeader == COLUMNNAME_Realcounterpartyname)
            {
                if (item.RealCounterPartyName != null)
                {
                    realcounterpartyname = item.RealcounterpartynameItem_Edit.shortnamecn;
                    if (realcounterpartyname == item.RealCounterPartyName)
                        return;
                }
            }

            // 客户名
            if (columnHeader == null || columnHeader == COLUMNNAME_TRADER)
            {
            }

            // 方向
            if (columnHeader == null || columnHeader == COLUMNNAME_SIDE)
            {
                side = item.Side_Edit;
                if (side == item.Side)
                    return;
            }
            // 价格
            if (columnHeader == null || columnHeader == COLUMNNAME_PRICE)
            {
                price = item.Price_Edit;
                if (price == item.Price)
                    return;
            }

            // 净价
            if (columnHeader == null || columnHeader == COLUMNNAME_CleanPrice)
            {
                cleanPrice = CommonUtils.ConvertToDecimalOrNull(item.CleanPrice_Edit);
                if (cleanPrice == item.CleanPrice)
                    return;
            }
            // 全价
            if (columnHeader == null || columnHeader == COLUMNNAME_FullPrice)
            {
                fullPrice = CommonUtils.ConvertToDecimalOrNull(item.FullPrice_Edit);
                if (fullPrice == item.FullPrice)
                    return;
            }
            // 价格
            if (columnHeader == null || columnHeader == COLUMNNAME_Yieldtoexecution)
            {
                yieldToExe = item.Yieldtoexecution_Edit;
                if (yieldToExe == item.Yieldtoexecution)
                    return;
            }
            // 数量
            if (columnHeader == null || columnHeader == COLUMNNAME_QTY)
            {
                qty = item.Qty_Edit;
                wholeFlag = item.WholeFlag_Edit;
                if (qty == item.Qty && wholeFlag == item.WholeFlag)
                    return;
            }
            // 清算速度
            if (columnHeader == null || columnHeader == COLUMNNAME_SETTLETYPE)
            {
                //DateTime createtime = CommonUtils.ConvertToDateTime(item.CreateTime);
                //settlType = CommonUtils.IsSpotSettle(createtime, item.edit_settltype_date, item.edit_settletype);
                //if (string.IsNullOrEmpty(settlType))
                //{
                //    settlType = item.edit_settltype_date.ToString("MM.dd") + (item.edit_settletype == 0 ? "+0" : "+1");
                //}
                settlType = item.SettlType_Edit;
                if (settlType == item.SettlTypeDisplay)
                    return;
            }

            // 发单方向
            if (columnHeader == null || columnHeader == COLUMNNAME_TrdSubType)
            {
                trdSubType = item.TrdSubTypeItem_Edit;
                if (trdSubType == item.TrdSubType)
                    return;
            }

            // 发单方式
            if (columnHeader == null || columnHeader == COLUMNNAME_DealWaySelect)
            {
                dealWay = item.DealWayItem_Edit;
                if (dealWay == item.DealWay)
                    return;
            }
            // 代付费率
            if (columnHeader == null || columnHeader == COLUMNNAME_PaymentRate)
            {
                paymentrate = item.PaymentRate_Edit;
                if (paymentrate == item.PaymentRate)
                    return;
            }

            // 新更新
            //UpdateIntentionQuoteRequest request = new UpdateIntentionQuoteRequest();
            //request.intentionId = item.IntentionId;
            //request.institutionId = institutionId;
            //request.institutionName = institutionName;
            //request.traderId = traderId;
            //request.traderName = traderName;
            //request.side = side;
            //request.price = price;
            //request.qty = qty;
            //request.settlType = settlType;
            //bool isSuccess = await IntentionOldHandler.Instance.UpdateIntentionQuote(request);
            //if (isSuccess)
            //{
            // 发单机构
            if (columnHeader == null || columnHeader == COLUMNNAME_INSTITUTION)
            {
                item.CounterPartyName = institutionName;
                item.CounterPartyId = institutionId;
                RefeshInstMsg(item);
            }
            //终端对手方
            if (columnHeader == null || columnHeader == COLUMNNAME_Realcounterpartyname)
            {
                item.RealCounterPartyName = realcounterpartyname;
            }
            //对手方交易员
            if (columnHeader == null || columnHeader == COLUMNNAME_CounterPartySubName)
            {
                item.CounterPartySubName = counterPartySubName;
            }
            // 客户名
            /*if (columnHeader == null || columnHeader == COLUMNNAME_TRADER)
            {
                item.TraderName = traderName;
                item.TraderId = traderId;
            }*/
            // 方向
            if (columnHeader == null || columnHeader == COLUMNNAME_SIDE)
                item.Side = side;
            // 价格
            if (columnHeader == null || columnHeader == COLUMNNAME_PRICE)
            {
                item.Price = price;
                if (item.Price != null && !string.IsNullOrEmpty(item.SettlType))
                {
                    //CalBond("4", item.SettlType, NumChange(item.Price), item);
                    CalBond("4", item.SettlType,item.Price.ToString(), false, item);
                }
            }
            // 净价
            if (columnHeader == null || columnHeader == COLUMNNAME_CleanPrice)
            {
                item.CleanPrice = cleanPrice;
                if (item.CleanPrice != null && !string.IsNullOrEmpty(item.SettlType))
                {
                    CalBond("2", item.SettlType, item.CleanPrice.ToString(), false, item);
                }
            }
            // 全价
            if (columnHeader == null || columnHeader == COLUMNNAME_FullPrice)
            {
                item.FullPrice = fullPrice;
                if (item.FullPrice != null && !string.IsNullOrEmpty(item.SettlType))
                {
                    //CalBond("4", item.SettlType, NumChange(item.Price), item);
                    CalBond("3", item.SettlType, item.FullPrice.ToString(), false, item);
                }
            }
            // 行权价格
            if (columnHeader == null || columnHeader == COLUMNNAME_Yieldtoexecution)
            {
                item.Yieldtoexecution = yieldToExe;
                if (item.Yieldtoexecution != null && !string.IsNullOrEmpty(item.SettlType))
                {
                    CalBond("7", item.SettlType, item.Yieldtoexecution.ToString(), false, item);
                }
            }
            // 数量
            if (columnHeader == null || columnHeader == COLUMNNAME_QTY)
            {
                item.Qty = qty;
                item.WholeFlag = wholeFlag;
            }
            // 清算速度
            if (columnHeader == null || columnHeader == COLUMNNAME_SETTLETYPE)
            {
                
                item.SettlTypeDisplay = settlType;
                int beginIndex = settlType.IndexOf('+');
                if (beginIndex != -1)
                {
                    string settlTypeDisplay = settlType.Substring(beginIndex);
                    if ("+1".Equals(settlTypeDisplay))
                    {
                        item.SettlType = "2";
                    }
                    else
                    {
                        item.SettlType = "1";
                    }
                }
                else 
                {
                    item.SettlType = settlType;
                }
                item.SettlTypeDisplay = settlType;
                if (item.Price != null && !string.IsNullOrEmpty(item.SettlType))
                {
                    CalBond("4", item.SettlType, item.Price.ToString(), false, item);
                    //CalBond("4", item.SettlType, NumChange(item.Price), item);
                }
            }
            

            if (columnHeader == null || columnHeader == COLUMNNAME_TrdSubType)
                item.TrdSubType = trdSubType;

            if (columnHeader == null || columnHeader == COLUMNNAME_DealWaySelect)
                item.DealWay = dealWay;
            if (columnHeader == null || columnHeader == COLUMNNAME_PaymentRate)
            {
                item.PaymentRate = paymentrate; 
                setPaymentNetPrice(item);
            }

            //if (string.IsNullOrEmpty(item.OrderID) == false)
            //{
            //    // 更新我的订单/我的撤单
            //    MyOrderProcesser.Processer.UpdateCustomInfo(item.IntentionId,
            //       item.InstitutionId, item.InstitutionName,
            //       item.TraderId, item.TraderName
            //       );
            //    // 更新我的成交
            //    MyExecutionProcesser.Processer.UpdateCustomInfo(item.IntentionId,
            //        item.InstitutionId, item.InstitutionName,
            //        item.TraderId, item.TraderName
            //        );
            //}
            //}
        }
        private string NumChange(decimal? price)
        {
            decimal d = (decimal)(price / 100);
            return d.ToString();
        }
        /// <summary>
        /// 计算收益率或者净价
        /// </summary>
        /// <param name="calType"></param>
        /// <param name="settleType"></param>
        /// <param name="val"></param>
        private void CalBond(string calType, string settleType, string val,  IntentionQuoteItem item)
        {
            decimal d = 0;
            if (string.IsNullOrEmpty(val) || string.IsNullOrEmpty(settleType) || !decimal.TryParse(val, out d) )
                return;
            ThreadPool.QueueUserWorkItem(o =>
            {
                try
                {
                    CalReq requst = new CalReq();
                    requst.SecurityID = item.SecurityId; //sendPriceItem.Security;
                    requst.CalType = calType;//"4";
                    requst.SettlType = settleType;// "1";
                    requst.Value = val;// customPrice;
                    if (!string.IsNullOrEmpty(LocalEnv.ConfInfo.CalType))
                        requst.CalPath = LocalEnv.ConfInfo.CalType;

                    CalResult result = CalHandler.Instance.CalBond(requst);
                    if (requst != null)
                    {
                        if (result != null && result.Data != null && result.IsSuccess())
                        {
                            result.ReqData = string.Format("{0}-{1}-{2}", requst.CalType, requst.SettlType, requst.Value);
                            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                            {
                                if (calType.Equals("4"))
                                {
                                    item.CleanPrice = result.Data.CleanPrice;
                                    item.FullPrice = result.Data.FullPrice;
                                }
                                else if (calType.Equals("2"))//净价
                                {
                                    if (result.IsSuccess())
                                    {
                                        item.Price = (result.Data.Yield * 100);
                                        item.FullPrice = result.Data.FullPrice;
                                    }
                                }
                                else if (calType.Equals("7"))//行权收益率
                                {
                                    if (result.IsSuccess())
                                    {
                                        item.Price = (result.Data.Yield * 100);
                                        item.FullPrice = result.Data.FullPrice;
                                        item.CleanPrice = result.Data.CleanPrice;
                                    }
                                }
                            }));
                        }
                    }
                }
                catch (Exception e)
                {
                    LogHelper.Error(e);
                }
                finally
                {
                }
            });
        }
        public void DeleteOrder()
        {
            if (this.SelectedItem_Recognize != null)
            {
                this.intentionQuoteInfo.Remove(this.SelectedItem_Recognize);
            }
        }
        private Task<List<string>> Sub_ExportExcel(string filePath, bool isapprovalhis, List<DataGridColumn> columnOrder)
        {
            return Task.Run(() =>
            {
                List<string> errorlist = new List<string>();
                STC.Frame.Utils.ExcelHelper.ExcelHelper helper = null;
                try
                {
                    // 1 检查文件是否被占用
                    if (File.Exists(filePath))
                    {
                        bool isFileLock = STC.Frame.Utils.ExcelHelper.ExcelHelper.CheckFileIsLock(filePath);
                        if (isFileLock)
                        {
                            errorlist.Add($"{filePath} 被占用，请关闭后再试。");
                            return errorlist;
                        }

                        File.Delete(filePath);
                    }


                    helper = new STC.Frame.Utils.ExcelHelper.ExcelHelper(filePath);
                    Dictionary<string, DataTable> dataDic = new Dictionary<string, DataTable>();
                    DataTable dt = GenerateExcelDataTable(isapprovalhis, columnOrder);
                    string name = "";
                    if (isapprovalhis)
                    {
                        name = "审批历史";
                    }
                    else
                    {
                        name = "交易记录";
                    }
                    dataDic[name] = dt;
                    Dictionary<string, int> result = helper.DataTableToExcel(dataDic, true);
                    if (result == null)
                        errorlist.Add("无数据导出");

                }
                catch (Exception e)
                {
                    errorlist.Add(e.Message);
                    LogHelper.Error(e);
                }
                finally
                {
                    if (helper != null)
                        helper.Dispose();
                }
                return errorlist;
            });
        }
        private DataTable GenerateExcelDataTable(bool isapprovalhis, List<DataGridColumn> columnOrder)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("债券代码");
            dt.Columns.Add("债券简称");
            dt.Columns.Add("方向");
            dt.Columns.Add("成交价");
            dt.Columns.Add("成交量(万)");
            dt.Columns.Add("清算速度");
            dt.Columns.Add("剩余期限");
            dt.Columns.Add("我方");
            dt.Columns.Add("对手方机构");
            dt.Columns.Add("发单方向");
            dt.Columns.Add("成交方式");
            //dt.Columns.Add("中债估值");
            dt.Columns.Add("原文");
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (this.ViewSource.View != null)
                {
                    foreach (IntentionQuoteItem item in this.ViewSource.View)
                    {
                        DataRow newDR = dt.NewRow();
                        newDR["债券代码"] = item.SecurityId;
                        newDR["债券简称"] = item.Symbol; 
                        newDR["方向"] = ComboSourceUtil.Instance.FindEnum(item.Side, "side");
                        newDR["成交价"] = item.Price;
                        newDR["成交量(万)"] = item.Qty;
                        newDR["清算速度"] = ComboSourceUtil.Instance.FindEnum(item.SettlType, "settletype"); 
                        newDR["剩余期限"] = item.TimeToMaturity;
                        newDR["我方"] = item.PartyName;
                        newDR["对手方机构"] = item.CounterPartyName_Edit;
                        newDR["发单方向"] = string.Format("OFR{0}BID",item.DealSideDispaly);
                        newDR["成交方式"] = item.DealWaySelect != null?item.DealWaySelect.DisplayMemberPath:"";
                        //newDR["中债估值"] = item.CCDCValuation;
                        /*ObservableCollection<IntentionQuoteItem> data = new ObservableCollection<IntentionQuoteItem>();
                        data.Add(item);*/
                        newDR["原文"] = item.Text;
                        dt.Rows.Add(newDR);
                    }
                }
            });
            return dt;
        }
        public async void exportDeal(Window win,List<DataGridColumn> columnOrder)
        {
            try
            {
                //创建一个保存文件式的对话框
                System.Windows.Forms.SaveFileDialog sfd = new System.Windows.Forms.SaveFileDialog();                              //设置这个对话框的起始保存路径
                sfd.InitialDirectory = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                sfd.RestoreDirectory = true;
                sfd.FileName = $"成交识别导出_{DateTime.Now.ToString("yyyy_MM_dd")}";
                //设置保存的文件的类型，注意过滤器的语法
                sfd.Filter = "Excel Documents|*.xls;*.xlsx;";
                sfd.DefaultExt = ".xlsx";
                //调用ShowDialog()方法显示该对话框，该方法的返回值代表用户是否点击了确定按钮
                string promptTxt = string.Empty;
                if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    if (!sfd.FileName.EndsWith(".xls", true, System.Globalization.CultureInfo.CurrentCulture)
                        && !sfd.FileName.EndsWith(".xlsx", true, System.Globalization.CultureInfo.CurrentCulture))
                    {
                        Prompt2Seconds prompt = new Prompt2Seconds();
                        prompt.Owner = Application.Current.MainWindow;
                        prompt.PromptTitle = "成交识别导出";
                        prompt.PromptContext = "当前只支持xls文件和xlsx文件";
                        prompt.Show();
                        return;
                    }

                    List<string> errorList = await Sub_ExportExcel(sfd.FileName, false, columnOrder);

                    if (errorList != null && errorList.Count > 0)
                    {
                        StringBuilder errors = new StringBuilder();
                        foreach (string error in errorList)
                        {
                            if (errors.Length > 0)
                            {
                                errors.AppendLine();
                            }
                            errors.Append(error);
                        }
                        promptTxt = errors.ToString();
                    }
                    else
                    {
                        promptTxt = "导出成功";
                    }
                }
                else
                {
                    promptTxt = "取消导出";
                }

                if (string.IsNullOrEmpty(promptTxt) == false)
                {
                    Prompt2Seconds prompt = new Prompt2Seconds();
                    prompt.Owner = Application.Current.MainWindow;
                    prompt.PromptTitle = "成交识别导出";
                    prompt.PromptContext = promptTxt;
                    prompt.Show();
                    //win.Close();
                }
            }
            catch (Exception e)
            {
                Prompt2Seconds prompt = new Prompt2Seconds();
                prompt.Owner = Application.Current.MainWindow;
                prompt.PromptTitle = "成交识别导出";
                prompt.PromptContext = e.Message;
                prompt.Show();

                LogHelper.Error(e);
            }
        }

       
        #region 账户组合

        private Visibility _accountUserVisibility = Visibility.Collapsed;

        public Visibility AccountUserVisibility
        {
            get { return _accountUserVisibility; }
            set
            {
                _accountUserVisibility = value;
                this.OnPropertyChanged();
            }
        }

        
       
        #endregion

        #region firm发布 、NF发布
        
        
        private void GetErrorInfo(Common.Model.Fix.Message.NewOrderSingle request,
                ref int failCount,
                ref int totalCount,
                Dictionary<string, List<TPlaceOrderResult>> warnReplyDic,
                Dictionary<string, List<TPlaceOrderResult>> prohibitReplyDic,
                Dictionary<string, List<TPlaceOrderResult>> exceptionReplyDic)
        {

            try
            {
                request.HandlInst = "2";
                RecognizeIntentionQuoteReplyPo replyPo = TableHandler.Instance.PublishIntention(request);
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
                                prohibitReplyDic[request.ClOrdID] = orderRisks;
                            }
                            else
                            {
                                warnReplyDic[request.ClOrdID] = orderRisks;
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
                            exceptionReplyDic[request.ClOrdID] = exceptionResult;
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

        //private void SetDoAllBtnEnable(bool isEnable)
        //{
        //    this.ConfirmEnable = isEnable;
        //    this.QuickConfirmEnable = isEnable;
        //    this.TGEnable = isEnable;
        //}

        

        private Common.Model.Fix.Message.NewOrderSingle GetNewOrderSingle(IntentionQuoteItem item, string firmflag)
        {
            Common.Model.Fix.Message.NewOrderSingle request = new Common.Model.Fix.Message.NewOrderSingle();
            request.FirmFlag = firmflag;
            request.VipCode = IsVip ? "0001" : "0010";
            if (item.IsOpposite && item.IsSame)
            {
                request.IfSubBond = "ALL";
            }
            else if (item.IsOpposite)
            {
                request.IfSubBond = item.Side == "1" ? "OFR" : "BID";
            }
            else if (item.IsSame)
            {
                request.IfSubBond = item.Side == "1" ? "BID" : "OFR";
            }
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
            if (this.DefaultValidTimeQuotationCheck)
            {
                int hour = DateTimeEx.ServerTimeNow.Hour;
                int min = DateTimeEx.ServerTimeNow.Minute;
                if (hour > 0 && hour < 6)
                {
                    request.ValidUntilTime = (DateTime.Parse(DateTimeEx.ServerTimeNow.ToString("yyyy-MM-dd") + " 06:00:00")).ToString("yyyy-MM-dd HH:mm:ss");
                }
                else if (hour >= 6 && hour < 12)
                {
                    request.ValidUntilTime = (DateTime.Parse(DateTimeEx.ServerTimeNow.ToString("yyyy-MM-dd") + " 12:00:00")).ToString("yyyy-MM-dd HH:mm:ss");
                }
                else
                {
                    request.ValidUntilTime = (DateTime.Parse(DateTimeEx.ServerTimeNow.ToString("yyyy-MM-dd") + " 16:30:00")).ToString("yyyy-MM-dd HH:mm:ss");
                }
            }

            // 时长
            if (this.HourValidTimeQuotationCheck)
            {
                // ValidMinute  ValidSecond
                int totalsecond = 0;
                if (!string.IsNullOrEmpty(this.ValidMinute))
                {
                    totalsecond = int.Parse(this.ValidMinute) * 60;
                }

                if (!string.IsNullOrEmpty(this.ValidSecond))
                {
                    totalsecond += int.Parse(this.ValidSecond);
                }

                if (totalsecond == 0)
                {
                    request.ValidUntilTime = DateTimeEx.ServerTimeNow.AddSeconds(this.DefaultValidUntilTime).ToString("yyyy-MM-dd HH:mm:ss");
                }
                else
                {
                    request.ValidUntilTime = DateTimeEx.ServerTimeNow.AddSeconds(totalsecond).ToString("yyyy-MM-dd HH:mm:ss");
                }
            }

            // 时点
            if (this.TimeValidTimeQuotationCheck)
            {
                // ValidTimeHour ValidTimeMinute ValidTimeSecond
                int hour = 0;
                int min = 0;
                int sec = 0;

                if (!string.IsNullOrEmpty(this.ValidTimeHour))
                {
                    hour = int.Parse(this.ValidTimeHour);
                }

                if (!string.IsNullOrEmpty(this.ValidTimeMinute))
                {
                    min = int.Parse(this.ValidTimeMinute);
                }

                if (!string.IsNullOrEmpty(this.ValidTimeSecond))
                {
                    sec = int.Parse(this.ValidTimeSecond);
                }

                DateTime tmpdt = DateTime.Parse(DateTimeEx.ServerTimeNow.ToString("yyyy-MM-dd").ToString());
                tmpdt = tmpdt.AddHours(hour).AddMinutes(min).AddSeconds(sec);
                request.ValidUntilTime = tmpdt.ToString("yyyy-MM-dd HH:mm:ss");
            }

            // 无
            if (this.NoLimitValidTimeQuotationCheck)
            {
                // 不填写
            }
            // Modi:cyc Date:2021-04-13 Func:增加用户选择的 需求有效时长 选项记忆功能
            string validTimeType = this.DefaultValidTimeQuotationCheck ? "0" : this.HourValidTimeQuotationCheck ? "1" : this.TimeValidTimeQuotationCheck ? "2" : this.NoLimitValidTimeQuotationCheck ? "3" : "0";
            // 默认
            string info1 = string.Empty;
            if (this.DefaultValidTimeQuotationCheck)
            {
                info1 = $"{validTimeType}|";
                OrderTimeModel otm = new OrderTimeModel() { ValidTimeType = info1, ValidUntilTime = request.ValidUntilTime, IF_SUB_BOND = request.IfSubBond ?? string.Empty, VIP = request.VipCode ?? string.Empty, ORG_TYPE = request.OrgType ?? string.Empty };
                request.Info1 = JsonConvert.SerializeObject(otm);
            }

            // 时长
            if (this.HourValidTimeQuotationCheck)
            {
                // ValidMinute,ValidSecond
                string min = string.IsNullOrEmpty(this.ValidMinute) ? "" : this.ValidMinute;
                string sec = string.IsNullOrEmpty(this.ValidSecond) ? "" : this.ValidSecond;
                info1 = $"{validTimeType}|{min}:{sec}";
                OrderTimeModel otm = new OrderTimeModel() { ValidTimeType = info1, ValidUntilTime = request.ValidUntilTime, IF_SUB_BOND = request.IfSubBond ?? string.Empty, VIP = request.VipCode ?? string.Empty, ORG_TYPE = request.OrgType ?? string.Empty };
                request.Info1 = JsonConvert.SerializeObject(otm);
            }
            // 时点
            if (this.TimeValidTimeQuotationCheck)
            {
                // ValidTimeHour ValidTimeMinute ValidTimeSecond
                string hour = string.IsNullOrEmpty(this.ValidTimeHour) ? "" : this.ValidTimeHour;
                string min = string.IsNullOrEmpty(this.ValidTimeMinute) ? "" : this.ValidTimeMinute;
                string sec = string.IsNullOrEmpty(this.ValidTimeSecond) ? "" : this.ValidTimeSecond;
                info1 = $"{validTimeType}|{hour}:{min}:{sec}";
                OrderTimeModel otm = new OrderTimeModel() { ValidTimeType = info1, ValidUntilTime = request.ValidUntilTime, IF_SUB_BOND = request.IfSubBond ?? string.Empty, VIP = request.VipCode ?? string.Empty, ORG_TYPE = request.OrgType ?? string.Empty };
                request.Info1 = JsonConvert.SerializeObject(otm);
            }

            // 无
            if (this.NoLimitValidTimeQuotationCheck)
            {
                info1 = $"{validTimeType}|";
                OrderTimeModel otm = new OrderTimeModel() { ValidTimeType = info1, ValidUntilTime = string.Empty, IF_SUB_BOND = request.IfSubBond ?? string.Empty, VIP = request.VipCode ?? string.Empty, ORG_TYPE = request.OrgType ?? string.Empty };
                request.Info1 = JsonConvert.SerializeObject(otm);
            }

            // 用户设置的到期时间,需要大于用户自定义的时间
            string selfUntilTime = request.ValidUntilTime;
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
                    if (!this.NoLimitValidTimeQuotationCheck)
                    {
                        request.ValidUntilTime = GetMinDate(selfUntilTime, validUntiTimeSetting, this.DefaultValidTimeQuotationCheck);
                    }
                    else
                    {
                        //选择了无
                        if (DateTimeEx.ServerTimeNow.CompareTo(DateTime.Parse(validUntiTimeSetting)) <= 0)
                        {
                            request.ValidUntilTime = validUntiTimeSetting;
                        }
                        else
                        {
                            request.ValidUntilTime = string.Empty;
                        }
                    }
                }
                else
                {
                    // 默认，时长，时点
                    if (!this.NoLimitValidTimeQuotationCheck)
                    {
                        // 默认
                        if (this.DefaultValidTimeQuotationCheck)
                        {
                            request.ValidUntilTime = GetMinDate(selfUntilTime, "", true);
                        }
                    }
                    else
                    {
                        request.ValidUntilTime = string.Empty;
                    }
                }
            }
            //request.PriceCondition = ;
            if (IntentionOldHandler.Instance.isSALER())
            {
                // 机构编号
                string institutionId = null;
                // 机构名称
                string institutionName = null;
                
                request.InstitutionID = institutionId;
                request.InstitutionName = institutionName;
                // 交易员编号
                string traderId = null;
                // 交易员名称
                string traderName = null;
               
                request.TraderID = traderId;
                request.TraderName = traderName;
            }

           
            request.Text = item.Text;
            return request;
        }



        /// <summary>
        /// 报价前检查参数设置
        /// </summary>
        /// <returns></returns>
        internal string GetErrorTxt()
        {
            string errMsg = string.Empty;
            if (this.HourValidTimeQuotationCheck)
            {
                // 时长
                // ValidMinute  ValidSecond
                if (string.IsNullOrEmpty(this.ValidMinute) && string.IsNullOrEmpty(this.ValidSecond))
                {
                    errMsg = "有效时长需大于0秒";
                }

                int totalsecond = 0;
                if (!string.IsNullOrEmpty(this.ValidMinute))
                {
                    totalsecond = int.Parse(this.ValidMinute) * 60;
                }

                if (!string.IsNullOrEmpty(this.ValidSecond))
                {
                    totalsecond += int.Parse(this.ValidSecond);
                }

                if (totalsecond == 0)
                {
                    errMsg = "有效时长需大于0秒";
                }
            }
            else if (this.TimeValidTimeQuotationCheck)
            {
                // 时点
                // ValidTimeHour ValidTimeMinute ValidTimeSecond
                if (string.IsNullOrEmpty(this.ValidTimeHour))
                {
                    errMsg = "必须输入时";
                }

                int hour = 0;
                int min = 0;
                int sec = 0;

                if (!string.IsNullOrEmpty(this.ValidTimeHour))
                {
                    hour = int.Parse(this.ValidTimeHour);
                }

                if (!string.IsNullOrEmpty(this.ValidTimeMinute))
                {
                    min = int.Parse(this.ValidTimeMinute);
                }

                if (!string.IsNullOrEmpty(this.ValidTimeSecond))
                {
                    sec = int.Parse(this.ValidTimeSecond);
                }

                DateTime tmpdt = DateTime.Parse(DateTimeEx.ServerTimeNow.ToString("yyyy-MM-dd").ToString());
                tmpdt = tmpdt.AddHours(hour).AddMinutes(min).AddSeconds(sec);
                if (DateTimeEx.ServerTimeNow >= tmpdt)
                {
                    errMsg = "订单撤单时点不能在报单时间之前";
                }
            }
            return errMsg;
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
        /// 报价选择类型解析  默认 "0|" : 时长 "1|{min}:{sec}" : 时点  "2|{hour}:{min}:{sec}" 无 "3|"
        /// </summary>
        /// <param name="info2"></param>
        public void ParseValidTimeFormat()
        {
            string info2 = string.Empty;
            if (SystemParamHandler.Instance.GetUserSetting() != null)
            {
                if (!string.IsNullOrEmpty(SystemParamHandler.Instance.GetUserSetting().inf2))
                {
                    ConsistencyCheckPo consistencyCheckPo = JsonConvert.DeserializeObject<ConsistencyCheckPo>(SystemParamHandler.Instance.GetUserSetting().inf2);
                    if (consistencyCheckPo != null)
                    {
                        info2 = consistencyCheckPo.DefaultValidTime;
                    }
                }
            }

            if (!string.IsNullOrEmpty(info2))
            {
                //报价选择过期类型  默认 "0|" : 时长 "1|{min}:{sec}" : 时点  "2|{hour}:{min}:{sec}"
                if (info2.StartsWith("0|"))
                {
                    ValidSelectIndex = 0;
                }
                else if (info2.StartsWith("1|"))
                {
                    ValidSelectIndex = 1;
                    // ValidMinute,ValidSecond
                    string s = info2.Split('|')[1];
                    if (s.Contains(":"))
                    {
                        ValidMinute = s.Split(':')[0];
                        ValidSecond = s.Split(':')[1];
                    }
                }
                else if (info2.StartsWith("2|"))
                {
                    ValidSelectIndex = 2;

                    // ValidTimeHour ValidTimeMinute ValidTimeSecond
                    string s = info2.Split('|')[1];
                    if (s.Contains(":"))
                    {
                        ValidTimeHour = s.Split(':')[0];
                        ValidTimeMinute = s.Split(':')[1];
                        ValidTimeSecond = s.Split(':')[2];
                    }
                }
                else if (info2.StartsWith("3|"))
                {
                    ValidSelectIndex = 3;
                    // 无
                }
            }
        }
        #endregion

        private void RemenberInstAndTrade()
        {
           
        }



      
        private string clientTypeEnum;
        public string ClientTypeEnum
        {
            get { return clientTypeEnum; }
            set { clientTypeEnum = value; this.OnPropertyChanged(); }
        }


        internal void FilterClientType(AutoCompleteArgs args)
        {
            
        }
        internal void FilterInstitution(STC.PT.Plugin.View.Component.AutoCompleteComboBox.AutoCompleteArgs args)
        {
            //string currentInstId = this.SelectedItem_Recognize.SelectedInstintuion_Edit == null ? string.Empty : this.SelectedItem_Recognize.SelectedInstintuion_Edit.in;

            List<string> instIdList = null;
            //if (!string.IsNullOrEmpty(currentInstId))
            //{
            //    if (instIdList == null)
            //        instIdList = new List<string>();
            //    if (!instIdList.Contains(currentInstId))
            //    {
            //        instIdList.Add(currentInstId);
            //    }
            //}

            List<CrmInst> result = new List<CrmInst>();
            if (!string.IsNullOrEmpty(args.Pattern))
            {

                string txt = args.Pattern.Replace("'", "");
                if (txt.Contains(" "))
                {
                    string[] arr = txt.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);

                    foreach (string str in arr)
                    {

                        List<CrmInst> selectDatas = WtUtils.GetInstitutionsInfoPo(str, 2000, instIdList);
                        result.AddRange(selectDatas);
                    }
                }
                else
                {
                    result = WtUtils.GetInstitutionsInfoPo(args.Pattern.Trim(), 2000, instIdList);
                }
            }
            else
            {
                /*List<CrmInst> myInstList = Common.Utils.CommonUtils.GetMyInstList();
                if ((instIdList == null || instIdList.Count == 0) && myInstList.Count > 0)
                {
                    result.AddRange(myInstList);
                }
                else
                {
                    result.AddRange(WtUtils.GetInstitutionsInfoPo(instIdList));
                }*/
                result.AddRange(WtUtils.GetInstitutionsInfoPo(instIdList));
            }

            if (!string.IsNullOrEmpty(args.Pattern))
            {
                // 排序
                if (result != null)
                {
                    result = result.OrderBy(item => item, new InstitutionComparer(args.Pattern)).ToList();
                }
            }

            args.DataSource = result;
        }


        public void CopyDeal(List<DataGridColumn> columnOrder)
        {
            Task.Run(() =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (this.ViewSource.View != null)
                    {
                        string CopyTxt = getCopyTxt(columnOrder, this.ViewSource.View);
                        CopyTxt = CopyTxt.Replace(" ", PTInnerSettingViewModel.Instance.TabStr);
                        //Clipboard.SetDataObject(CopyTxt, true);
                        ClipboardHelper.SetTextWithRetries(CopyTxt, maxRetries: 2, delayMs: 150);
                    }
                });
            });
        }
        private string getCopyTxt(List<DataGridColumn> columnOrder, ICollectionView list)
        {
            string CopyTxt = "";
            List<object[]> exportedData = new List<object[]>();
            foreach (IntentionQuoteItem item in list)
            {
                if (!check(item)) 
                {
                    CopyTxt = "";
                    break;
                }
                object[] rowData = new object[columnOrder.Count-1];
                int i = 0;
                foreach (var column in columnOrder)
                {
                    if ("代码".Equals(column.Header))
                    {
                        rowData[i] = item.SecurityId;
                    }
                    else if ("简称".Equals(column.Header))
                    {
                        rowData[i] = item.Symbol;
                    }
                    else if ("成交价".Equals(column.Header))
                    {
                        if ("1".Equals(item.PriceType))
                        {
                            rowData[i] = CommonUtils.ConvertToString(item.Price);
                        }
                        else
                        {
                            rowData[i] = string.Format("{0}%", CommonUtils.ConvertToString(item.Price));
                        }
                    }
                    else if ("成交量".Equals(column.Header))
                    {
                        rowData[i] = string.Format("{0}kw", decimal.Divide(CommonUtils.ConvertToDecimal(CommonUtils.ConvertToString(item.Qty)), 1000));
                    }
                    else if ("清算速度".Equals(column.Header))
                    {
                        rowData[i] = ComboSourceUtil.Instance.FindEnum(item.SettlType, "settletype_drp");
                    }
                    else if ("剩余期限".Equals(column.Header))
                    {
                        rowData[i] = item.TimeToMaturity;
                    }
                    else if ("我方机构".Equals(column.Header))
                    {
                        continue;
                    }
                    else if ("发单方向".Equals(column.Header))
                    {
                        continue;
                    }
                    i++;
                }
                string trdSubType = "1".Equals(item.TrdSubType)?"我方发":"对方发";
                string dealwaymsg = "";
                if (!string.IsNullOrWhiteSpace(item.DealWay) && DealWayItems.TryGetValue(item.DealWay, out dealwaymsg)) ;

                rowData[i-1] = string.Format(" to {0} ,{1}  [{2}]", item.CounterPartyName_Edit != null ? item.CounterPartyName_Edit.Trim() : "", trdSubType, dealwaymsg);

                //发单方向
                /*string dealSideAndWay = "";
                if (!"false".Equals(item.DealWayVisible))
                {
                    string dealway = item.DealWaySelect != null ? item.DealWaySelect.DisplayMemberPath : "";
                    if ("1".Equals(item.DealSide))
                    {
                        dealSideAndWay = string.Format("【{0}】发{1}", dealway, item.CounterPartyName_Edit != null ? item.CounterPartyName_Edit.Trim() : "");
                    }
                    else
                    {
                        dealSideAndWay = string.Format("【{0}】发{1}", dealway, item.PartyName.Trim());
                    }
                }
                //我方机构
                if ("1".Equals(item.DealSide))
                {
                    if (string.IsNullOrEmpty(dealSideAndWay))
                    {
                        rowData[i] = string.Format("{0} to {1}", item.PartyName.Trim(), item.CounterPartyName_Edit != null ? item.CounterPartyName_Edit.Trim() : "");
                    }
                    else 
                    {
                        rowData[i] = string.Format("{0} to {1},{2}", item.PartyName.Trim(), item.CounterPartyName_Edit != null ? item.CounterPartyName_Edit.Trim() : "",dealSideAndWay);
                    }
                }
                else
                {
                    if (string.IsNullOrEmpty(dealSideAndWay))
                    {
                        rowData[i] = string.Format("{0} to {1}", item.CounterPartyName_Edit != null ? item.CounterPartyName_Edit.Trim() : "", item.PartyName.Trim());
                    }
                    else 
                    {
                        rowData[i] = string.Format("{0} to {1},{2}", item.CounterPartyName_Edit != null ? item.CounterPartyName_Edit.Trim() : "", item.PartyName.Trim(),dealSideAndWay);
                    }
                }*/


                exportedData.Add(rowData);
            }
            string tabStr = " ";
            foreach (object[] row in exportedData)
            {
                if (!string.IsNullOrEmpty(CopyTxt))
                {
                    CopyTxt = CopyTxt + "  \n";
                }
                string rowDetail = "";
                foreach (object col in row)
                {
                    string detail = "";
                    if (col != null)
                    {
                        detail = (string)col;
                        if (string.IsNullOrEmpty(detail))
                        {
                            continue;
                        }
                    }
                    if (string.IsNullOrEmpty(rowDetail))
                    {
                        rowDetail = detail;
                    }
                    else
                    {
                        rowDetail = rowDetail + tabStr + detail;
                    }
                }
                CopyTxt = CopyTxt + rowDetail;
            }
            return CopyTxt;
        }

        private IntentionQuoteItem AddRecognizeDeal(TradeInfo obj)
        {
            if (this.IntentionQuoteInfo.Any(entry => entry.ClOrdID == obj.msgTime))
            {
                return this.IntentionQuoteInfo.First(entry => entry.ClOrdID == obj.msgTime);
            }
            IntentionQuoteItem item = new IntentionQuoteItem();
            if (consistency != null)
            {
                item.IsOpposite = UserSettingAdapter.Instance.ParseCheckedType(consistency.SubscribeReverse);
                item.IsSame = UserSettingAdapter.Instance.ParseCheckedType(consistency.SubscribeSyntropy);
            }
            
            item.ClOrdID = obj.msgTime;
            item.Side = obj.side;
            item.SecurityId = obj.securityID;
            item.Symbol = obj.symbol;
            item.TradeDate = obj.tradeDate;
            item.SettlDate = obj.settlDate;

            item.PriceType = obj.priceType;
            string settlType = obj.settlType;
            if (!string.IsNullOrEmpty(settlType))
            {
                
                if ("1".Equals(settlType))
                {
                    item.SettlType = "1";
                }
                else if ("2".Equals(settlType))
                {
                    item.SettlType = "2";
                }
                else if ("3".Equals(settlType))
                {
                    item.SettlType = "1";
                }
                else
                {
                    int beginIndex = settlType.IndexOf('+');
                    settlType = settlType.Substring(beginIndex);
                    if ("+1".Equals(settlType))
                    {
                        item.SettlType = "2";
                    }
                    else
                    {
                        item.SettlType = "1";
                    }
                }
            }
            else
            {
                item.SettlType = "1";
            }
            if (string.IsNullOrEmpty(item.TradeDate))
            {
                string tradeDate = "";
                if (string.IsNullOrEmpty(obj.settlType))
                {
                    tradeDate = GetSettleTypeFromServer("3");
                }
                else
                { 
                    tradeDate = GetSettleTypeFromServer(item.SettlType);
                }
                item.TradeDate = tradeDate;
            }
            DateTime today = DateTime.Now;
            DateTime tradeTime = CommonUtils.ConvertToDateTime2(item.TradeDate);
            string settlTypeDisplay = CommonUtils.IsSpotSettle(today, tradeTime, "2".Equals(item.SettlType)?1:0);
            item.SettlTypeDisplay = string.IsNullOrEmpty(settlTypeDisplay)?obj.settlType:settlTypeDisplay;
            if (string.IsNullOrEmpty(obj.priceType))
            {
                item.Price = CommonUtils.ConvertToDecimalOrNull(obj.price);
                CalBond("4", item.SettlType, obj.price, false, item);
            }
            else 
            {
                if ("1".Equals(obj.priceType))
                {
                    item.Price = CommonUtils.ConvertToDecimalOrNull(obj.price);
                    CalBond("4", item.SettlType, obj.price, false, item);
                }
                else if ("2".Equals(obj.priceType))
                {
                    item.Yieldtoexecution = CommonUtils.ConvertToDecimalOrNull(obj.price);
                    CalBond("7", item.SettlType, obj.price, false, item);

                }
                else if ("3".Equals(obj.priceType))
                {
                    item.CleanPrice = string.IsNullOrEmpty(obj.price) ? CommonUtils.ConvertToDecimalOrNull(obj.cleanPrice) : CommonUtils.ConvertToDecimalOrNull(obj.price);

                    string cleanPrice = "";
                    if (item.SecurityId.EndsWith(".SH"))
                    {
                        cleanPrice = item.CleanPrice.Value.ToString("F3");
                    }
                    else
                    {
                        cleanPrice = CommonUtils.ConvertToString(item.CleanPrice);
                    }
                    CalBond("2", item.SettlType, cleanPrice, false, item);
                }
                else if ("4".Equals(obj.priceType))
                {
                    item.FullPrice = CommonUtils.ConvertToDecimalOrNull(obj.price);
                    CalBond("3", item.SettlType, obj.price, false, item);
                }
                else 
                {
                    item.Price = CommonUtils.ConvertToDecimalOrNull(obj.price);
                    CalBond("4", item.SettlType, obj.price, false, item);
                }
            }
            decimal qty = string.IsNullOrEmpty(obj.allocQty)?decimal.Parse("0") :(decimal)(CommonUtils.ConvertToDecimalOrNull(obj.allocQty) / 100);
            if (qty >= 1)
            {
                item.Qty = qty * 100;
            }
            item.TrdSubTypeItem_Edit = string.IsNullOrEmpty(obj.trdSubType)?"1":obj.trdSubType;
            item.TrdSubType = string.IsNullOrEmpty(obj.trdSubType) ? "1" : obj.trdSubType;
            item.DealWayItem_Edit = string.IsNullOrEmpty(obj.trdType)?SetDefultDealWay(item.SecurityId, item.CounterPartySubName):obj.trdType;
            item.DealWay = string.IsNullOrEmpty(obj.trdType) ? SetDefultDealWay(item.SecurityId, item.CounterPartySubName) : obj.trdType;

            try
            {
                CCDCValuaModel ccdcValueModel = CommonUtils.GetCCDCValua(obj.securityID);
                if (ccdcValueModel != null && !string.IsNullOrEmpty(ccdcValueModel.CCDCValuation))
                {
                    item.CCDCValuation = ccdcValueModel.CCDCValuation.ConvertToDecimal();
                    item.CCDCValuation_Date = ccdcValueModel.CCDCValuaDate;
                    item.ShowLastWorkDay = ccdcValueModel.ShowLastWorkDay;
                }
                else
                {
                    PriceHandler.Instance.RegisterCTPrice(obj.securityID, UpdateCtPrice);
                }
                item.IsExeCdcType = "1";
                var bondsInfoPo = CommonUtils.GetBondsInfoPoBySecurityID(item.SecurityId);
                if (bondsInfoPo != null && !string.IsNullOrEmpty(bondsInfoPo.Option_Type) &&
                            ((!bondsInfoPo.Option_Type.Equals("NON") && bondsInfoPo.Option_Date != null) ||
                            (bondsInfoPo.Option_Type.Equals("ETS") && bondsInfoPo.Option_Date == null)))
                {
                    item.IsExeCdcType = "0";
                }
                if (bondsInfoPo != null)
                {
                    if (string.IsNullOrEmpty(bondsInfoPo.RemaingingTime) && "4".Equals(bondsInfoPo.MarketIndicator))
                    {
                        item.RemainingTime = "已到期";
                    }
                    else
                    {
                        item.RemainingTime = bondsInfoPo.RemaingingTime;
                        item.RemaingingTime_convert = CommonUtils.ConvertRemainingTime(bondsInfoPo.RemaingingTime);
                    }
                }
            }
            catch (Exception e)
            {
                LogHandler.Error(e);
            }
            item.DealWayVisible = STC.PT.Plugin.Utils.CommonUtils.IsBank(obj.securityID) ? "false" : "true";
            item.PartyName = obj.partyName;
            item.CounterPartyName_Edit = obj.counterPartyName;
            item.CounterPartyName = obj.counterPartyName;
            item.CounterPartySubName = obj.counterPartySubName;
            item.RealCounterPartyName = obj.realCounterPartyName;
            item.RealCounterPartyName_Edit = obj.realCounterPartyName;
            item.PaymentRate = obj.bridgePaymentFixedRate;
            item.AllocText = obj.allocText; 
            setPaymentNetPrice(item);
            RefeshInstMsg(item);
            item.PropertyChanged += Item_PropertyChanged;

            this.IntentionQuoteInfo.Add(item);

            return item;
        }
        /// <summary>
        /// 计算收益率或者净价
        /// </summary>
        /// <param name="calType"></param>
        /// <param name="settleType"></param>
        /// <param name="val"></param>
        private void CalBond(string calType, string settleType, string val, bool isPayment, IntentionQuoteItem item)
        {
            decimal d = 0;
            if (string.IsNullOrEmpty(val) || string.IsNullOrEmpty(settleType) || !decimal.TryParse(val, out d))
                return;
            try
            {
                CalReq requst = new CalReq();
                requst.SecurityID = item.SecurityId;
                requst.CalType = calType;
                requst.SettlType = settleType;
                if ("4".Equals(calType) || "7".Equals(calType)) 
                {
                    val = decimal.Divide(CommonUtils.ConvertToDecimal(val),decimal.Parse( "100")).ToString();
                }
                requst.Value = val;
                requst.Tradedate = item.TradeDate;
                if (!string.IsNullOrEmpty(LocalEnv.ConfInfo.CalType))
                    requst.CalPath = LocalEnv.ConfInfo.CalType;

                CalResult result = CalHandler.Instance.CalBond(requst);
                if (requst != null)
                {
                    if (result != null && result.Data != null && result.IsSuccess())
                    {
                        result.ReqData = string.Format("{0}-{1}-{2}", requst.CalType, requst.SettlType, requst.Value);
                        Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                        {
                            if (isPayment)
                            {
                                if (calType.Equals("4"))
                                {
                                    item.NetPrice2 = result.Data.CleanPrice;
                                    this.OnPropertyChanged("PaymentNetPrice");
                                    item.FullPrice2 = result.Data.FullPrice;
                                    this.OnPropertyChanged("PaymentFullPrice");
                                    if ("0".Equals(item.IsExeCdcType))
                                    {
                                        item.Yieldtoexecution2 = (result.Data.YieldToExecution * 100);
                                        this.OnPropertyChanged("PaymentExePrice");
                                    }
                                    setSettlAmount(item);
                                }
                                else if (calType.Equals("2"))//净价
                                {
                                    if (result.IsSuccess())
                                    {
                                        item.Yield2 = (result.Data.Yield * 100);
                                        item.FullPrice2 = result.Data.FullPrice;
                                        this.OnPropertyChanged("PaymentYield");
                                        this.OnPropertyChanged("PaymentFullPrice");
                                        if ("0".Equals(item.IsExeCdcType))
                                        {
                                            item.Yieldtoexecution2 = (result.Data.YieldToExecution * 100);
                                            this.OnPropertyChanged("PaymentExePrice");
                                        }
                                        setSettlAmount(item);
                                    }
                                }
                                setTrdMatchId(item);
                            }
                            else
                            {
                                if (calType.Equals("4"))
                                {
                                    item.CleanPrice = result.Data.CleanPrice;
                                    item.FullPrice = result.Data.FullPrice;
                                    if ("0".Equals(item.IsExeCdcType))
                                    {
                                        item.Yieldtoexecution = (result.Data.YieldToExecution * 100);
                                    }
                                    this.OnPropertyChanged("NetPrice");
                                    this.OnPropertyChanged("FullPrice");
                                    this.OnPropertyChanged("ExePrice");
                                }
                                else if (calType.Equals("2"))//净价
                                {
                                    if (result.IsSuccess())
                                    {
                                        item.Price = (result.Data.Yield * 100);
                                        item.FullPrice = result.Data.FullPrice;
                                        if ("0".Equals(item.IsExeCdcType))
                                        {
                                            item.Yieldtoexecution = (result.Data.YieldToExecution * 100);
                                        }
                                        this.OnPropertyChanged("Price");
                                        this.OnPropertyChanged("FullPrice");
                                        this.OnPropertyChanged("ExePrice");
                                    }
                                }
                                else if (calType.Equals("3"))//全价
                                {
                                    if (result.IsSuccess())
                                    {
                                        item.Price = (result.Data.Yield * 100);
                                        item.CleanPrice = result.Data.CleanPrice;
                                        if ("0".Equals(item.IsExeCdcType))
                                        {
                                            item.Yieldtoexecution = (result.Data.YieldToExecution * 100);
                                        }
                                        this.OnPropertyChanged("Price");
                                        this.OnPropertyChanged("FullPrice");
                                        this.OnPropertyChanged("ExePrice");
                                    }
                                }
                                else if (calType.Equals("7"))//行权收益率
                                {
                                    if (result.IsSuccess())
                                    {
                                        item.Price = (result.Data.Yield * 100);
                                        item.FullPrice = result.Data.FullPrice;
                                        item.CleanPrice = result.Data.CleanPrice;
                                        this.OnPropertyChanged("Price");
                                        this.OnPropertyChanged("FullPrice");
                                        this.OnPropertyChanged("NetPrice");
                                    }
                                }
                                setPaymentNetPrice(item);
                            }                            
                        }));
                    }
                }
            }
            catch (Exception e)
            {
                LogHelper.Error(e);
            }
        }
        /// <summary>
        /// 计算正确的清算速度
        /// </summary>
        /// <param name="settleType">明天+0 ，6.15+0 </param>
        private string GetSettleTypeFromServer(string settleType)
        {
            try
            {
                CalWeekDateReq requst = new CalWeekDateReq();
                requst.settlementType = settleType;// "1";

                CalWeekDateResult result = CalHandler.Instance.calSettlementDate(requst);
                if (result != null && result.Data != null)
                {
                    string tradedate = result.Data.settlementDate;
                    if (!string.IsNullOrEmpty(tradedate))
                    {
                        return tradedate;
                    }
                    else
                    {
                        tradedate = DateTime.Now.AddDays(1).ToString("yyyyMMdd");
                        return tradedate;
                    }
                }
            }
            catch (Exception e)
            {
                LogHelper.Error(e);
            }
            return "";
        }

        /// <summary>
        /// 计算费后净价
        /// </summary>
        /// <param name="item"></param>
        private void setPaymentNetPrice(IntentionQuoteItem item)
        {
            if (item.CleanPrice.HasValue)
            {
                decimal paymentRate = string.IsNullOrEmpty(item.PaymentRate) ? 0 : decimal.Parse(item.PaymentRate);
                //decimal coufee = string.IsNullOrEmpty(Couponrate) ? 0 : decimal.Parse(Couponrate);//其他费率
                string cleanPrice = "";
                if (item.SecurityId.EndsWith(".SH"))
                {
                    cleanPrice = item.CleanPrice.Value.ToString("F3");
                }
                else
                {
                    cleanPrice = CommonUtils.ConvertToString(item.CleanPrice);
                }
                decimal netprice = CommonUtils.ConvertToDecimal(cleanPrice);
                if ("2".Equals(item.Side))//Ofr成交净价-代付费率-其他费率
                {
                    item.NetPrice2 = (netprice - paymentRate / 1000 );
                }
                else //Bid成交净价+代付费率+其他费率
                {
                    item.NetPrice2 = (netprice + paymentRate / 1000 );
                }
                //计算费后收益率和费后全价
                CalBond("2", item.SettlType, item.NetPrice2.ToString(), true,item);
            }
        }
        private void setSettlAmount(IntentionQuoteItem item)
        {
            if (item.FullPrice2.HasValue&& item.Qty.HasValue)
            {
                item.SettlAmount = (item.FullPrice2.Value * item.Qty.Value * 10000);
            }
        }
        private void setTrdMatchId(IntentionQuoteItem item)
        {
            if (!item.NetPrice2.HasValue)
            {
                item.TrdMatchId = "";
            }
            else
            {
                string NetPrice = item.NetPrice2.Value.ToString("F4");
                int dotIndex = NetPrice.IndexOf('.');
                string fractionalPart;

                if (dotIndex >= 0)
                {
                    fractionalPart = NetPrice.Substring(dotIndex + 1);
                }
                else
                {
                    fractionalPart = "0";
                }
                if (fractionalPart.Length < 3)
                    fractionalPart = fractionalPart.PadRight(3, '0');
                else
                {
                    fractionalPart = fractionalPart.Substring(fractionalPart.Length - 3, 3);
                }

                if (fractionalPart[0] == '0')
                {
                    fractionalPart = '1' + fractionalPart.Substring(1);
                }
                item.TrdMatchId = fractionalPart;
            }
        }
        private void RefeshInstMsg(IntentionQuoteItem item)
        {
            if (item.CounterPartyName != null && !string.IsNullOrEmpty(item.SecurityId))
            {
                if (item.SelectedInstintuion_Edit == null)
                {
                    List<CrmInst> results = STC.PT.Plugin.Utils.CommonUtils.GetInstitutionsInfoPo(item.CounterPartyName, 2, null);
                    if (results != null && results.Count == 1)
                    {
                        item.SelectedInstintuion_Edit = results[0];
                    }
                }
                if (item.SelectedInstintuion_Edit == null) 
                {
                    return;
                }
                if (item.SecurityId.ToUpper().EndsWith(".SH"))
                {
                    item.Sseinstitutioncode = item.SelectedInstintuion_Edit.sseinstitutioncode;
                    item.Szedealercode = "";
                    item.Szetradingentitycode = "";
                    item.Szetradercode = "";
                }
                else if (item.SecurityId.ToUpper().EndsWith(".SZ"))
                {

                    item.Sseinstitutioncode = "";
                    item.Szedealercode = item.SelectedInstintuion_Edit.szedealercode;
                    item.Szetradingentitycode = item.SelectedInstintuion_Edit.ssetradingentitycode;
                    string instname = item.SelectedInstintuion_Edit.shortnamecn;
                    List<CrmContact> list = STC.PT.Plugin.Utils.CommonUtils.GetContactInfoPoByInstname(instname, true);
                    if (list.Count > 0 && list.Count == 1)
                    {
                        CrmContact crmContact = list[0];
                        Model.CrmContactPT crmContactPT = new Model.CrmContactPT
                        {
                            id = crmContact.id,
                            namecn = crmContact.namecn,
                            instname = crmContact.instname
                        };
                        item.Szetradercode = crmContactPT.DisplayName;
                    }
                    else
                    {
                        item.Szetradercode = "";
                    }
                }
                else
                {
                    item.Sseinstitutioncode = "";
                    item.Szedealercode = "";
                    item.Szetradingentitycode = "";
                    item.Szetradercode = "";
                }
            }
            else
            {
                item.Sseinstitutioncode = "";
                item.Szedealercode = "";
                item.Szetradingentitycode = "";
                item.Szetradercode = "";
            }
        }
    }
}
