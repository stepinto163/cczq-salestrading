using Ats.Foundation.Message;
using com.gateway.log;
using Newtonsoft.Json;
using STC.Common.Data;
using STC.Common.Handler.Admin;
using STC.Common.Handler.SystemParam;
using STC.Common.Handler.Table;
using STC.Common.Model;
using STC.Common.Model.Base;
using STC.Common.Model.CRM;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.Table;
using STC.Common.Model.TBook;
using STC.Common.Utils;
using STC.Common.View.Component;
using STC.Common.View.Order;
using STC.PT.Plugin.Handler.Process;
using STC.PT.Plugin.View.Component;
using STC.PT.Plugin.ViewModel.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using static STC.Common.View.Component.AutoCompleteComboBox;
using WtUtils = STC.PT.Plugin.Utils.CommonUtils;

namespace STC.PT.Plugin.ViewModel
{
    public class RecognizeDealWinViewModel : ViewModelBase
    {
        private Dictionary<string, string> _sideDic = new Dictionary<string, string>();
        public Dictionary<string, string> SideDic
        {
            get
            {
                if (_sideDic.Count == 0)
                {
                    _sideDic["1"] = "Bid";
                    _sideDic["2"] = "Ofr";
                }
                return _sideDic;
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

        internal void UnregisterEvent()
        {
            IntentionOldHandler.Instance.OnRecognition -= Instance_OnRecoginition;
        }

        ConsistencyCheckPo consistency;

        public RecognizeDealWinViewModel()
        {
            if (LocalEnv.ConfInfo.IsParserImage == "1")
            {
                ParserImageVisibility = Visibility.Visible;
            }

            DelOrderCommand = new ComRelayCommand(DeleteOrder);
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
            if ("1".Equals(item.Side)) 
            {
                item.DealSide = "1";
            }
            else
            {
                item.DealSide = "2";
            }
            item.DealWayVisible = STC.PT.Plugin.Utils.CommonUtils.IsBank(obj.securityId)?"false":"true";
            item.PartyName = PTExecOrderProcesser.Instance.partyName;
            if (item.DealWayVisible.Equals("true"))
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
                list.Add(model);
                list.Add(model2);
                item.DealWayItems = list;
                item.DealWaySelect = model;
            }
            item.PropertyChanged += Item_PropertyChanged;

            this.IntentionQuoteInfo.Add(item);

            return item;
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
                    STC.Common.Model.Table.WorkBrench.RecognizeIntentionQuoteRequest request = new STC.Common.Model.Table.WorkBrench.RecognizeIntentionQuoteRequest();
                    request.userName = LocalEnv.UserInfo.UserName;
                    request.content = txtcont;
                    ///获取订单 
                    // STC.Common.Model.Table.WorkBrench.RecognizeIntentionQuoteReplyPo reply = TableOldHandler.Instance.RecognizeIntentionQuote(request);

                    STC.Common.Model.Table.WorkBrench.RecognizeIntentionQuoteReplyPo reply = ParserHandler.Instance.RequestParserBond(request);

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
            //string tradeDate = CommonUtils.CalTradeDate(item.SettlType);
            //item.edit_settltype_date = CommonUtils.ConvertToDateTime2(tradeDate);
            //DateTime settlDateTime;
            //int selectIndex;
            //CommonUtils.SetSettleType(item.SettlType, tradeDate, out settlDateTime, out selectIndex);
            //item.edit_settletype = selectIndex;
        }

        private static string COLUMNNAME_INSTITUTION = "客户机构";
        private static string COLUMNNAME_TRADER = "客户名";
        private static string COLUMNNAME_SIDE = "方向";
        private static string COLUMNNAME_PRICE = "成交价";
        private static string COLUMNNAME_QTY = "成交量";
        private static string COLUMNNAME_SETTLETYPE = "清算速度";

        internal void SaveRow(IntentionQuoteItem item, string columnHeader = null)
        {
            item.IsEdit = "2";
            // 价格
            decimal? price = null;
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

            // 客户机构
            if (columnHeader == null || columnHeader == COLUMNNAME_INSTITUTION)
            {
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
                if (settlType == item.SettlType)
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
            // 客户机构
            if (columnHeader == null || columnHeader == COLUMNNAME_INSTITUTION)
            {
                item.InstitutionName = institutionName;
                item.InstitutionId = institutionId;
            }
            // 客户名
            if (columnHeader == null || columnHeader == COLUMNNAME_TRADER)
            {
                item.TraderName = traderName;
                item.TraderId = traderId;
            }
            // 方向
            if (columnHeader == null || columnHeader == COLUMNNAME_SIDE)
                item.Side = side;
            // 价格
            if (columnHeader == null || columnHeader == COLUMNNAME_PRICE)
                item.Price = price;
            // 数量
            if (columnHeader == null || columnHeader == COLUMNNAME_QTY)
            {
                item.Qty = qty;
                item.WholeFlag = wholeFlag;
            }
            // 清算速度
            if (columnHeader == null || columnHeader == COLUMNNAME_SETTLETYPE)
                item.SettlType = settlType;

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
                List<CrmInst> myInstList = Common.Utils.CommonUtils.GetMyInstList();
                if ((instIdList == null || instIdList.Count == 0) && myInstList.Count > 0)
                {
                    result.AddRange(myInstList);
                }
                else
                {
                    result.AddRange(WtUtils.GetInstitutionsInfoPo(instIdList));
                }
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
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (this.ViewSource.View != null)
                {
                    string CopyTxt = getCopyTxt(columnOrder, this.ViewSource.View);
                    ClipboardHelper.CopyToClipboard(CopyTxt.Replace(" ", PTInnerSettingViewModel.Instance.TabStr));
                }
            });
        }
        private string getCopyTxt(List<DataGridColumn> columnOrder, ICollectionView list) 
        {
            List<object[]> exportedData = new List<object[]>();
            foreach (IntentionQuoteItem item in list)
            {
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
                //发单方向
                string dealSideAndWay = "";
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
                }
                

                exportedData.Add(rowData);
            }
            string CopyTxt = "";
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
    }
}
