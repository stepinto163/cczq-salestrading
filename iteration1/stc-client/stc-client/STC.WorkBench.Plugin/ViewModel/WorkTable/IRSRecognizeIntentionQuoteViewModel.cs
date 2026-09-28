using Ats.Foundation.Message;
using com.gateway.log;
using netapi.Api;
using Newtonsoft.Json;
using STC.Common.Data;
using STC.Common.Handler.Admin;
using STC.Common.Handler.Base;
using STC.Common.Handler.SystemParam;
using STC.Common.Handler.Table;
using STC.Common.Model;
using STC.Common.Model.Admin;
using STC.Common.Model.CRM;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.Table;
using STC.Common.Model.TBook;
using STC.Common.Utils;
using STC.Common.View.Component;
using STC.Common.View.Order;
using STC.Common.View.Order.InnerView;
using STC.Common.ViewModel;
using STC.Common.ViewModel.Data;
using STC.Common.ViewModel.Order.InnerPanel;
using STC.Common.ViewModel.Order.InnerView;
using STC.WorkBench.Plugin.View;
using STC.WorkBench.Plugin.View.WorkTable;
using STC.WorkBench.Plugin.ViewModel.Data;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using static STC.WorkBench.Plugin.View.Component.AutoCompleteComboBox;
using WtUtils = STC.WorkBench.Plugin.Utils.CommonUtils;

namespace STC.WorkBench.Plugin.ViewModel.WorkTable
{
    public class IRSRecognizeIntentionQuoteViewModel : ViewModelBase
    {
        #region 构造

        public IRSRecognizeIntentionQuoteWin myWindow;

        private static string COLUMNNAME_INSTITUTION = "客户机构";
        private static string COLUMNNAME_TRADER = "客户名";
        private static string COLUMNNAME_SIDE = "方向";
        private static string COLUMNNAME_PRICE = "报价";
        private static string COLUMNNAME_QTY = "报量";
        private static string COLUMNNAME_SETTLETYPE = "清算速度";

        private bool isSysAuto_Institution = false;

        private CrmInst _selectedInstitution_Edit;

        public CrmInst SelectedInstintuion_Edit
        {
            get { return _selectedInstitution_Edit; }
            set
            {
                if (_selectedInstitution_Edit != value)
                {
                    _selectedInstitution_Edit = value;

                    if (value != null)
                    {
                        List<CrmInst> result = GetCrmInst();
                        if (this._selectedInstitution_Edit != null && !string.IsNullOrEmpty(this._selectedInstitution_Edit.insttypevalue))
                        {
                            result = result.Where(item => item.shortnamecn == this._selectedInstitution_Edit.insttypevalue).ToList();
                            if (result != null && result.Count > 0)
                            {
                                this.SelectClientTypeItem = result.First();
                            }

                        }
                    }


                    this.OnPropertyChanged();
                }
            }
        }

        private bool isSysAuto_Trader = false;

        private string _institutionName_Edit;

        public string InstitutionName_Edit
        {
            get { return _institutionName_Edit; }
            set
            {
                if (!(string.IsNullOrEmpty(_institutionName_Edit) && string.IsNullOrEmpty(value)) && _institutionName_Edit != value)
                {
                    //if (isSysAuto_Institution == false
                    //&& _institutionName_Edit != value)
                    //{
                    //    isSysAuto_Trader = true;
                    //    SelectedTrader_Edit = null;
                    //    TraderName_Edit = string.Empty;
                    //}
                    //isSysAuto_Institution = false;

                    _institutionName_Edit = value;
                    this.OnPropertyChanged();
                }
            }
        }


        private CrmContact _selectedTrader_Edit;

        public CrmContact SelectedTrader_Edit
        {
            get { return _selectedTrader_Edit; }
            set
            {

                if (_selectedTrader_Edit != value)
                {
                    // 只有crmContact中才有机构信息，可以联动显示
                    if (value != null)
                    {
                        if (SelectedInstintuion_Edit == null || SelectedInstintuion_Edit.idbkey != value.instid)
                        {
                            List<string> instList = new List<string>();
                            instList.Add(value.instid);
                            List<CrmInst> results = WtUtils.GetInstitutionsInfoPo(instList);
                            if (results != null && results.Count > 0)
                            {
                                isSysAuto_Institution = true;
                                isSysAuto_Trader = true;
                                SelectedInstintuion_Edit = results[0];
                                InstitutionName_Edit = results[0].shortnamecn;
                            }
                        }

                        if (SelectedInstintuion_Edit != null && SelectedInstintuion_Edit.idbkey == value.instid)
                        {
                            isSysAuto_Institution = false;
                            isSysAuto_Trader = true;
                            SelectedInstintuion_Edit = SelectedInstintuion_Edit;
                            InstitutionName_Edit = InstitutionName_Edit;
                        }
                    }

                    _selectedTrader_Edit = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private string _traderName_Edit;

        public string TraderName_Edit
        {
            get { return _traderName_Edit; }
            set
            {
                if (!(string.IsNullOrEmpty(_traderName_Edit) && string.IsNullOrEmpty(value)) && _traderName_Edit != value)
                {
                    //if (isSysAuto_Trader == false
                    //    && _traderName_Edit != value)
                    //{
                    //    isSysAuto_Institution = true;
                    //    SelectedInstintuion_Edit = null;
                    //    InstitutionName_Edit = string.Empty;
                    //}
                    isSysAuto_Trader = false;

                    _traderName_Edit = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private StcTbookUsers _selectedBroker_Edit;

        public StcTbookUsers SelectedBroker_Edit
        {
            get { return _selectedBroker_Edit; }
            set
            {
                if (_selectedBroker_Edit != value)
                {
                    _selectedBroker_Edit = value;
                    this.OnPropertyChanged();
                }
            }
        }


        private CrmInst selectClientTypeItem;

        public CrmInst SelectClientTypeItem
        {
            get { return selectClientTypeItem; }
            set
            {
                if (selectClientTypeItem != value)
                {
                    selectClientTypeItem = value;
                    if (selectClientTypeItem != null)
                        ClientTypeEnum = selectClientTypeItem.shortnamecn;
                    this.OnPropertyChanged();
                }
            }
        }

        private string clientTypeEnum;
        public string ClientTypeEnum
        {
            get { return clientTypeEnum; }
            set { clientTypeEnum = value; this.OnPropertyChanged(); }
        }

        private string _brokerName_Edit;

        public string BrokerName_Edit
        {
            get { return _brokerName_Edit; }
            set
            {
                _brokerName_Edit = value;
                this.OnPropertyChanged();
            }
        }


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

        private bool _isPublishBtnEnable;

        public bool IsPublishBtnEnable
        {
            get { return _isPublishBtnEnable; }
            set {
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

        ConsistencyCheckPo consistency;


        private string contentTxt;
        /// <summary>
        /// 识别文本
        /// </summary>
        public string ContentTxt
        {
            get { return contentTxt; }
            set
            {
                contentTxt = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// 获取时间控制
        /// </summary>
        /// <returns></returns>
        public CancelOrderTimeConditionViewModel GetDetailConditionViewModel()
        {
            CancelOrderTimeConditionView detailConditionView = myWindow.FindName("detailConditionView") as CancelOrderTimeConditionView;
            return detailConditionView.DataContext as CancelOrderTimeConditionViewModel;
        }
        #endregion

        #region Command

        public ICommand DelOrderCommand { get; set; }

        public ICommand CloseCommand { get; set; }

        public ICommand ClearContentTxtCommand { get; set; }
        //public ICommand RecognizeCommand { get { return new ComRelayCommand(OnRecognizeCommand); } }
        public ICommand ClearCommand { get; set; }

        public ICommand NFOkCommand { get; set; }

        public ICommand FirmOkCommand { get; set; }

        public ICommand RecognizeCommand { get; set; }

        #endregion

        public IRSRecognizeIntentionQuoteViewModel()
        {
            if (AdminHandler.Instance.GetCachestcUserQuery() != null && AdminHandler.Instance.GetCachestcUserQuery().Datas != null)
            {
                if (AdminHandler.Instance.GetCachestcUserQuery().Datas.Any(item => item.Username == LocalEnv.UserInfo.UserName))
                {
                    StcTbookUsers currentUser = AdminHandler.Instance.GetCachestcUserQuery().Datas.First(item => item.Username == LocalEnv.UserInfo.UserName);
                    this.SelectedBroker_Edit = currentUser;
                    this.BrokerName_Edit = currentUser.Name;
                }
            }


            DelOrderCommand = new ComRelayCommand(DeleteOrderCommand);
            CloseCommand = new ComRelayCommand(OnCloseClick);
            ClearCommand = new ComRelayCommand(OnClearCommand);
            NFOkCommand = new ComRelayCommand(OnNFOkCommand);
            FirmOkCommand = new ComRelayCommand(OnFirmOkCommand);
            ClearContentTxtCommand = new ComRelayCommand(OnClearContentTxtCommand);
            RecognizeCommand = new ComRelayCommand(OnRecognizeCommand);

            InitAccountUsers();

            IntentionQuoteInfo.CollectionChanged += IntentionQuoteInfo_CollectionChanged;
            this.ViewSource.IsLiveFilteringRequested = true;
            this.ViewSource.IsLiveSortingRequested = true;
            this.ViewSource.LiveFilteringProperties.Add("Status");
            this.ViewSource.Source = IntentionQuoteInfo;
            BindingOperations.EnableCollectionSynchronization(this.ViewSource.View, _LockObject);
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

        private IntentionQuoteItem AddRecognize(STC.Common.Model.Table.WorkBrench.IntentionQuoteInfo obj)
        {
            IntentionQuoteItem item = new IntentionQuoteItem();
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
            if(CommonUtils.IsSpreadOrBasic(obj.securityId))
            {
                item.Price = obj.price.HasValue ? obj.price / 100 : obj.price;
            }
            else
            {
                item.Price = obj.price;
            }
            
            if (obj.qty.ConvertToDecimal() > 0)
            {
                item.Qty = obj.qty.ConvertToDecimal();
            }

            item.SettlType = obj.settlType;
            item.Text = obj.text;
            item.CreateTime = obj.createTime;
            item.UpdateTime = obj.updateTime;
            item.UserName = obj.userName;
            item.CloseBy = obj.closeBy;
            item.CloseByName = obj.closeByName;
            item.Status = obj.status;
            item.OrderID = obj.orderId;
            item.WholeFlag = obj.wholeFlag;
            item.ParseType = obj.parseType;
            item.ClOrdID = DataTimeSequence.Instance.GetId();
            item.PriceType = obj.priceType;
            item.PropertyChanged += Item_PropertyChanged;
            this.IntentionQuoteInfo.Add(item);

            return item;
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


        #region 编辑保存
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
                if (this.SelectedInstintuion_Edit != null)
                {
                    institutionName = this.SelectedInstintuion_Edit.shortnamecn;
                    institutionId = this.SelectedInstintuion_Edit.idbkey;
                }
                else
                {
                    institutionId = this.InstitutionName_Edit == item.InstitutionName ? item.InstitutionId : string.Empty;
                    institutionName = this.InstitutionName_Edit;
                }
                if (institutionId == item.InstitutionId && institutionName == item.InstitutionName)
                {
                    return;
                }

                //// 联动检查客户名
                //if (!string.IsNullOrEmpty(institutionId) 
                //        && !string.IsNullOrEmpty(item.TraderId))
                //{
                //    List<CrmContact> CrmContactList = CommonUtils.GetContactidInfoPo(item.InstitutionId);
                //    if (!CrmContactList.Any(m=>m.ContactId == item.TraderId))
                //    {
                //        traderName = string.Empty;
                //        traderId = string.Empty;
                //    }
                //}
            }

            // 客户名
            if (columnHeader == null || columnHeader == COLUMNNAME_TRADER)
            {
                if (this.SelectedTrader_Edit != null)
                {
                    traderName = this.SelectedTrader_Edit.namecn;
                    traderId = this.SelectedTrader_Edit.idbkey;
                    //item.QqNumber = item.SelectedTrader_Edit.QQ;
                }
                else
                {
                    traderName = this.TraderName_Edit;
                    traderId = this.TraderName_Edit == item.TraderName ? item.TraderId : string.Empty;
                }
                if (traderId == item.TraderId && traderName == item.TraderName)
                {
                    return;
                }
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


        #endregion



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

        private AccountUserItem _accountUserItem = null;

        public AccountUserItem AccountUserItem

        {
            get { return _accountUserItem; }
            set
            {
                _accountUserItem = value;
                this.OnPropertyChanged();
            }
        }

        private ObservableCollection<AccountUserItem> _accountUserSource = new ObservableCollection<AccountUserItem>();

        public ObservableCollection<AccountUserItem> AccountUserSource
        {
            get
            {
                return _accountUserSource;
            }
        }


        private void InitAccountUsers()
        {
            if (LocalEnv.ConfInfo.IsAccountUser != "1")
                return;

            string defaultAccountID = string.Empty;
            AccountUserVisibility = Visibility.Visible;
            UserSetting Us = SystemParamHandler.Instance.GetUserSetting();

            if (Us != null && !string.IsNullOrEmpty(Us.inf2))
            {
                if (JsonSplit.IsJson(Us.inf2))
                {
                    ConsistencyCheckPo ck = Newtonsoft.Json.JsonConvert.DeserializeObject<ConsistencyCheckPo>(Us.inf2);
                    defaultAccountID = ck.AccountId;
                }
            }
            _accountUserSource.Add(new AccountUserItem());
            AccountAndUsersQueryReplyPo result = AdminHandler.Instance.getAccountUser();
            if (result != null && result.Datas != null)
            {
                foreach (TbookAccountUsers accountUser in result.Datas)
                {
                    AccountUserItem item = new AccountUserItem() { Id = accountUser.accountid, AccountName = accountUser.accountname };
                    _accountUserSource.Add(item);
                    if (!string.IsNullOrEmpty(defaultAccountID) && defaultAccountID == accountUser.accountid)
                    {
                        AccountUserItem = item;
                    }
                }
            }

        }

        #endregion

        #region NewCommand

        private void OnClearContentTxtCommand()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                this.ContentTxt = string.Empty;
                (myWindow.FindName("contentTxt") as WatermarkTextBox).Focus();
            });
        }


        private void DeleteOrderCommand()
        {
            if (this.SelectedItem_Recognize != null)
            {
                this.intentionQuoteInfo.Remove(this.SelectedItem_Recognize);
            }
        }

        private void OnCloseClick()
        {
            RemenberInstAndTrade();
            myWindow.Close();
        }


        private void OnClearCommand()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (this.intentionQuoteInfo != null)
                {
                    this.IntentionQuoteInfo.Clear();
                }
            });
        }

        private char[] enterSplitChar = new char[] { '\n','\r' };

        /// <summary>
        /// 识别Command
        /// </summary>
        private void OnRecognizeCommand()
        {
            OnClearCommand();

            System.Threading.ThreadPool.QueueUserWorkItem(o =>
            {
                try
                {

                    string pattern = @"(\r\n)+";
                    string content = Regex.Replace(ContentTxt.Trim(), pattern, "\r\n");
                    if (string.IsNullOrEmpty(content))
                    {
                        CommonUtils.Prompt2Second("警告", $"请输入识别文本！");
                        return;
                    }


                    STC.Common.Model.Table.WorkBrench.RecognizeIntentionQuoteRequest request = new STC.Common.Model.Table.WorkBrench.RecognizeIntentionQuoteRequest();
                    request.userName = LocalEnv.UserInfo.UserName;
                    ///获取订单 
                    STC.Common.Model.Table.WorkBrench.RecognizeIntentionQuoteReplyPo reply = null;

                    string[] lsstring = content.Split(enterSplitChar, StringSplitOptions.RemoveEmptyEntries);
                    List<string> ls = new List<string>();
                    foreach(string str in lsstring)
                    {
                        ls.Add(str);
                    }
                    request.contentList = ls;

                    reply = ParserHandler.Instance.RequestParserIRS(request);

                    if (reply == null)
                    {
                        CommonUtils.Prompt2Second("警告", $"报价识别失败！");
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
                                        obj.qty = obj.quantity == null ? null : obj.quantity;
                                        if (!string.IsNullOrEmpty(obj.BondID))
                                        {
                                            obj.securityId = obj.BondID;
                                            obj.symbol = obj.BondID;
                                        }

                                        if (!string.IsNullOrEmpty(obj.side))
                                        {
                                            if (obj.side.ToUpper() == "BID")
                                                obj.side = "1";
                                            if (obj.side.ToUpper() == "OFR")
                                                obj.side = "2";
                                        }
                                        obj.text = obj.original;
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
        /// Nonfirm发布
        /// </summary>
        private void OnNFOkCommand()
        {
            ComfireCommand("0", "NonFirm");
        }

        /// <summary>
        /// Firm发布
        /// </summary>
        private void OnFirmOkCommand()
        {
            ComfireCommand("1"," Firm");
        }

        private async void ComfireCommand(string isfirm = "0", string firmtext = "")
        {
            string errMsg = GetErrorTxt();

            if (!string.IsNullOrEmpty(errMsg))
            {
                CommonUtils.Prompt2Second("确认", $"校验失败：{errMsg}", myWindow);
                return;
            }

            int totalCount_local = 0;
            int failCount_local = 0;
            Dictionary<string, List<TPlaceOrderResult>> exceptionReplyDic = new Dictionary<string, List<TPlaceOrderResult>>();

            foreach (IntentionQuoteItem item in this.IntentionQuoteInfo)
            {
                GetLocalCheckErrorInfo(item, ref failCount_local, ref totalCount_local, exceptionReplyDic);
            }

            StringBuilder error0 = new StringBuilder();
            string alertmsg = $"是否执行{firmtext}发布操作？";
            if (failCount_local > 0)
            {
                if (exceptionReplyDic.Count > 0)
                {
                    foreach (var rs in exceptionReplyDic)
                    {
                        error0.AppendLine($"客户订单编号 {rs.Key}: {rs.Value[0].Reason}");
                    }

                    alertmsg = $"{alertmsg}({totalCount_local - failCount_local}笔)\r\n校验失败 {failCount_local} 笔, 原因：\r\n{error0.ToString()}";
                }
            }
            else
            {
                alertmsg = $"{alertmsg}{totalCount_local}笔）";
            }


            if (!Common.View.Component.CustomMessageBox.Question(alertmsg, $"{firmtext}发布"))
            {
                return;
            }

            List<NewOrderSingle> reqlst = new List<NewOrderSingle>();

            foreach (IntentionQuoteItem item in this.IntentionQuoteInfo)
            {
                Common.Model.BaseReplyPo replyPo = CheckParam(item);
                if (!(replyPo == null || (replyPo != null && !replyPo.IsSuccess())))
                {
                    NewOrderSingle request = GetNewOrderSingle(item, isfirm);
                    reqlst.Add(request);
                }
            }

            if (reqlst.Count == 0)
            {
                CommonUtils.Prompt2Second("确认", $"{ firmtext}发布需求失败, 原因:\r\n校验失败 {failCount_local} 笔", myWindow);
                return;
            }

            if (ApiManager.Instance.GetDefaultApi().EnabledMsgLog == "1")
            {
                LogHandler.Info($"单击{firmtext}发布按钮");
            }


            StringBuilder error = null;
            error = await SubPublishOrders(reqlst);
            if (error != null && error.Length > 0)
            {
                CustomMessageBox.Error($"{firmtext}发布需求失败, 原因:\r\n{error.ToString()}");
            }
            else
            {
                OnCloseClick();
                CustomMessageBox.Info($"{firmtext}发布需求成功！");
            }
        }

        /// <summary>
        /// 发布
        /// </summary>
        /// <param name="tmp"></param>
        /// <returns></returns>
        private async Task<StringBuilder> SubPublishOrders(List<NewOrderSingle> reqlst)
        {
            StringBuilder error = new StringBuilder();

            await Task.Run(() =>
            {
                if (reqlst != null)
                {
                    int count = reqlst.Count;
                    Task[] lstTask = new Task[count];
                    ConcurrentDictionary<string, PResult> dic = new ConcurrentDictionary<string, PResult>();
                    for (int i = 0; i < count; i++)
                    {
                        NewOrderSingle sendPricePo = reqlst[i];
                        lstTask[i] = new Task(() =>
                        {
                            sendPricePo.HandlInst = "2";
                            RecognizeIntentionQuoteReplyPo res2 = IntentionOldHandler.Instance.PublishIntention(sendPricePo);
                            if (res2.Code == "10000" || res2.IsSuccess())
                            {
                                PResult result = new PResult();
                                result.Code = "0000";
                                result.Msg = res2.Msg;
                                dic[sendPricePo.ClOrdID] = result;
                            }
                            else
                            {
                                PResult res = new PResult();
                                res.Code = "-1";
                                res.Msg = "服务器接收数据失败！";
                                dic[sendPricePo.ClOrdID] = res;
                            }
                        });

                        lstTask[i].Start();
                    }

                    Task.WaitAll(lstTask);

                    foreach (KeyValuePair<string, PResult> pair in dic)
                    {
                        string orderID = pair.Key;
                        PResult reply = pair.Value;
                        if (reply != null)
                        {
                            if (reply.IsSuccess() == false)
                            {
                                error.AppendLine($"客户订单编号 {orderID}:{reply.Msg}");
                            }
                        }
                    }
                }
            });

            return error;
        }

        #endregion

        #region firm发布 、NF发布 条件检查
        /// <summary>
        /// 报价前检查参数设置
        /// </summary>
        /// <returns></returns>
        internal string GetErrorTxt()
        {
            string errMsg = string.Empty;
            //撤单时间
            CancelOrderTimeConditionViewModel tagconditionviewmodel = GetDetailConditionViewModel();
            //yyyy-MM-dd HH:mm:ss
            string validuntiltime = tagconditionviewmodel.GetValidUntilTime();
            if (!string.IsNullOrEmpty(validuntiltime))
            {
                DateTime tmpdt = DateTime.Parse(validuntiltime);
                if (DateTimeEx.ServerTimeNow >= tmpdt)
                {
                    errMsg = "订单撤单时点不能在报单时间之前";
                }
            }

            if (this.SelectedBroker_Edit == null)
            {
                errMsg = "Trader未选择";
            }
            return errMsg;
        }

        private ConfirmIntentionQuoteReplyPo CheckParam(IntentionQuoteItem item)
        {
            ConfirmIntentionQuoteReplyPo reply = new ConfirmIntentionQuoteReplyPo();
            reply.intentionId = item.IntentionId;
            reply.orderId = item.OrderID;
            reply.securityId = item.SecurityId;
            reply.status = item.Status;
            reply.Code = "0000";
            decimal d = 0;
            string errorMsg = string.Empty;
            
            if (string.IsNullOrEmpty(item.Side))
                errorMsg = "方向未填写";
            else if (item.Qty != null && item.Qty > 0 && decimal.TryParse(item.Qty.ToString(), out d) && (d % 1) > 0)
                errorMsg = string.Format("量必须是1 的整数倍，当前输入值：{0}", item.Qty);

            if (!string.IsNullOrEmpty(errorMsg))
            {
                reply.Code = "10001";

                List<TPlaceOrderResult> result = new List<TPlaceOrderResult>();
                TPlaceOrderResult por = new TPlaceOrderResult();
                por.Code = "10003";
                por.DoubleWarn = false;
                por.Reason = errorMsg;
                result.Add(por);

                reply.Msg = MsgHelper.Serializer<List<TPlaceOrderResult>>(result);
            }
            return reply;
        }

        private void GetLocalCheckErrorInfo(IntentionQuoteItem item, ref int failCount,ref int totalCount,Dictionary<string, List<TPlaceOrderResult>> exceptionReplyDic)
        {
            try
            {
                Common.Model.BaseReplyPo replyPo = CheckParam(item);
                if (replyPo == null || (replyPo != null && !replyPo.IsSuccess()))
                {
                    failCount++;

                    if (replyPo != null)
                    {
                        if (replyPo.Code == "10001")
                        {
                            exceptionReplyDic[item.ClOrdID.ToString()] = MsgHelper.Deserialize<List<TPlaceOrderResult>>(replyPo.Msg);
                        }
                    }
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

        /// <summary>
        /// 生成订单
        /// </summary>
        /// <param name="item"></param>
        /// <param name="firmflag"></param>
        /// <returns></returns>
        private Common.Model.Fix.Message.NewOrderSingle GetNewOrderSingle(IntentionQuoteItem item, string firmflag)
        {
            Common.Model.Fix.Message.NewOrderSingle request = new Common.Model.Fix.Message.NewOrderSingle();
            request.FirmFlag = firmflag;
            request.OrgType = SelectClientTypeItem != null ? selectClientTypeItem.instcode : string.Empty;
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
            //request.ClOrdID = DataTimeSequence.Instance.GetId();
            request.ClOrdID = item.ClOrdID;
            request.MarketIndicator = "2";
            request.OrdType = "2";
            decimal d = 0;
            if (item.Qty != null && item.Qty > 0 && decimal.TryParse(item.Qty.ToString(), out d))
            {
                if (d > 0)
                {
                    request.OrderQty = (d * 100).ToString();
                }
            }

            
            request.Price = item.Price.ToString();
            request.PriceType = item.PriceType; //收益率
            //request.NetPrice = ;
            request.SecurityID = item.SecurityId;
            request.Symbol = item.Symbol;
            request.TimeInForce = "0";
            request.Side = item.Side;
            request.RiskManualCk = "2";//默认空值（检查）, 1：只检查风控禁止，2：风控不检查
            request.Text = item.Text;
            //撤单时间
            CancelOrderTimeConditionViewModel tagconditionviewmodel = GetDetailConditionViewModel();
            //默认 "0|" : 时长 "1|{min}:{sec}" : 时点  "2|{hour}:{min}:{sec}" 无 "3|"
            string validtimetype = tagconditionviewmodel.GetValidTimeType();
            //yyyy-MM-dd HH:mm:ss
            string validuntiltime = tagconditionviewmodel.GetValidUntilTime();
            OrderTimeModel otm = new OrderTimeModel() { ValidTimeType = validtimetype, ValidUntilTime = validuntiltime, IF_SUB_BOND = request.IfSubBond ?? string.Empty, VIP = request.VipCode ?? string.Empty, ORG_TYPE = request.OrgType ?? string.Empty, Text = item.Text, QuoteType = "irs", IsFromTableSvrOrTradeBook = "1"};
            request.Info1 = JsonConvert.SerializeObject(otm);
            request.ValidUntilTime = validuntiltime;
           

            if (IntentionOldHandler.Instance.isSALER())
            {
                // 机构编号
                string institutionId = null;
                // 机构名称
                string institutionName = null;
                if (this.SelectedInstintuion_Edit != null)
                {
                    institutionName = this.SelectedInstintuion_Edit.shortnamecn;
                    institutionId = this.SelectedInstintuion_Edit.idbkey;
                }
                else
                {
                    institutionId = this.InstitutionName_Edit == item.InstitutionName ? item.InstitutionId : string.Empty;
                    institutionName = this.InstitutionName_Edit;
                }
                request.InstitutionID = institutionId;
                request.InstitutionName = institutionName;
                // 交易员编号
                string traderId = null;
                // 交易员名称
                string traderName = null;
                if (this.SelectedTrader_Edit != null)
                {
                    traderName = this.SelectedTrader_Edit.namecn;
                    traderId = this.SelectedTrader_Edit.idbkey;
                }
                else
                {
                    traderName = this.TraderName_Edit;
                    traderId = this.TraderName_Edit == item.TraderName ? item.TraderId : string.Empty;
                }
                request.TraderID = traderId;
                request.TraderName = traderName;
            }

            TagData info2tag = new TagData() { IF_SUB_BOND = otm.IF_SUB_BOND, ORG_TYPE = otm.ORG_TYPE, VIP = otm.VIP };
            request.Info2 = JsonConvert.SerializeObject(info2tag);

            request.Terminal = string.Format("STClient_{0}", LocalEnv.ServerInfo.Version);

            if (this.SelectedBroker_Edit != null)
            {
                request.UserName = this.SelectedBroker_Edit.Username;
            }

            if (LocalEnv.ConfInfo.IsAccountUser == "1")
            {
                if (this.AccountUserItem != null)
                {
                    request.AccountID = this.AccountUserItem.Id;
                    request.AccountName = this.AccountUserItem.AccountName;
                }
            }

            request.Name = LocalEnv.UserInfo.UserName;
            request.QuoteType = "irs";
            request.MacID = EngineHelper.GetMacByNetworkInterface();
            request.InsideIP = EngineHelper.GetIp();
            request.HardDiskID = EngineHelper.Getharddiskid2();

            request.SettlType = null;
            request.NetPrice = null;
            request.WholeFlag = null;
            request.Channel = "CLIENT";
            return request;
        }


        #endregion

        #region 保存设置
        private void RemenberInstAndTrade()
        {
            if (this.SelectedTrader_Edit != null && !string.IsNullOrEmpty(this.SelectedTrader_Edit.idbkey))
            {
                Common.Utils.CommonUtils.AddContract(this.SelectedTrader_Edit.idbkey);
                Common.Utils.CommonUtils.SaveContract();
            }
            if (this.SelectedInstintuion_Edit != null && !string.IsNullOrEmpty(SelectedInstintuion_Edit.idbkey))
            {
                Common.Utils.CommonUtils.AddInst(SelectedInstintuion_Edit.idbkey);
                Common.Utils.CommonUtils.SaveInst();
            }
        }

        #endregion

        #region 其它客户处理

        internal void FilterInstitution(AutoCompleteArgs args)
        {

            //BankListViewSource.View.Filter = x =>
            //{
            //    if (string.IsNullOrEmpty(args.Pattern))
            //        return true;
            //    else
            //        return ((KeyValueData)x).Key.Contains(args.Pattern) || ((KeyValueData)x).Value.Contains(args.Pattern);
            //};
            //args.DataSource = BankListViewSource.View;
            string currentInstId = this.SelectedTrader_Edit == null ? string.Empty : this.SelectedTrader_Edit.instid;

            List<string> instIdList = null;
            if (!string.IsNullOrEmpty(currentInstId))
            {
                if (instIdList == null)
                    instIdList = new List<string>();
                if (!instIdList.Contains(currentInstId))
                {
                    instIdList.Add(currentInstId);
                }
            }

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

        internal void FilterTrader(AutoCompleteArgs args)
        {
            string currentInstID = this._selectedInstitution_Edit == null ? null : this._selectedInstitution_Edit.idbkey;

            List<CrmContact> result = new List<CrmContact>();
            if (!string.IsNullOrEmpty(args.Pattern))
            {

                string txt = args.Pattern.Replace("'", "");
                if (txt.Contains(" "))
                {
                    string[] arr = txt.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);

                    foreach (string str in arr)
                    {
                        List<CrmContact> selectDatas = WtUtils.GetDBContactInfoPo(str, currentInstID);
                        result.AddRange(selectDatas);
                    }

                }
                else
                    result = WtUtils.GetDBContactInfoPo(args.Pattern.Trim(), currentInstID);
            }
            else
            {
                List<CrmContact> myTraderList = Common.Utils.CommonUtils.GetMyContract();
                if (string.IsNullOrEmpty(currentInstID) && myTraderList.Count > 0)
                {
                    result = myTraderList;
                }
                else
                {
                    result = WtUtils.GetDBContactInfoPo(null, currentInstID);
                }
            }

            if (!string.IsNullOrEmpty(args.Pattern))
            {
                // 排序
                if (result != null)
                {
                    result = result.OrderBy(item => item, new TraderComparer(args.Pattern)).ToList();
                }
            }

            args.DataSource = result;
        }

        internal void FilterTraderLocal(AutoCompleteArgs args)
        {
            string currentInstID = this._selectedInstitution_Edit == null ? string.Empty : this._selectedInstitution_Edit.idbkey;

            List<CrmContact> result = new List<CrmContact>();
            if (!string.IsNullOrEmpty(args.Pattern))
            {

                string txt = args.Pattern.Replace("'", "");
                if (txt.Contains(" "))
                {
                    string[] arr = txt.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);

                    foreach (string str in arr)
                    {
                        List<CrmContact> selectDatas = WtUtils.GetContactInfoPo(str, currentInstID);
                        result.AddRange(selectDatas);
                    }

                }
                else
                    result = WtUtils.GetContactInfoPo(args.Pattern.Trim(), currentInstID);
            }
            else
            {
                List<CrmContact> myTraderList = Common.Utils.CommonUtils.GetMyContract();
                if (string.IsNullOrEmpty(currentInstID) && myTraderList.Count > 0)
                {
                    result = myTraderList;
                }
                else
                {
                    result.AddRange(WtUtils.GetContactInfoPo(null, currentInstID));
                }
            }

            if (!string.IsNullOrEmpty(args.Pattern))
            {
                // 排序
                if (result != null)
                {
                    result = result.OrderBy(item => item, new TraderComparer(args.Pattern)).ToList();
                }
            }

            args.DataSource = result;
        }


        internal void FilterClientType(AutoCompleteArgs args)
        {
            List<CrmInst> result = GetCrmInst(); ;
            if (this._selectedInstitution_Edit != null && !string.IsNullOrEmpty(this._selectedInstitution_Edit.insttypevalue))
            {
                result = result.Where(item => item.shortnamecn == this._selectedInstitution_Edit.insttypevalue).ToList();
            }
            if (!string.IsNullOrEmpty(args.Pattern))
            {
                result = result.Where(item => item.shortnamecn.Contains(args.Pattern)).ToList();
            }

            args.DataSource = result;
        }
        private List<CrmInst> GetCrmInst()
        {
            List<CrmInst> result = new List<CrmInst>();
            Dictionary<string, QuoteTag> dic = BaseHandler.Instance.GetQuoteTagDic();

            if (dic != null && dic.ContainsKey("ORG_TYPE") && dic["ORG_TYPE"] != null
                && dic["ORG_TYPE"].childTags != null && dic["ORG_TYPE"].childTags.Count > 0)
            {
                foreach (ChildTag ct in dic["ORG_TYPE"].childTags)
                {
                    CrmInst bankCrm = new CrmInst();
                    bankCrm.shortnamecn = ct.childTagName;
                    bankCrm.instcode = ct.childTagValue;
                    result.Add(bankCrm);
                }
            }
            return result;
        }

        internal void FilterBrokerType(AutoCompleteArgs args)
        {
            List<StcTbookUsers> list = new List<StcTbookUsers>();
            STCUserReplyPo result = AdminHandler.Instance.GetCachestcUserQuery();

            if (!string.IsNullOrEmpty(args.Pattern))
            {
                list = result.Datas.Where(d => d.ActionIds.Contains("600102") && ((d.Username != null && d.Username.Contains(args.Pattern))
                || (d.Name != null && d.Name.Contains(args.Pattern)))).ToList();//多条件筛选，新功能增加
            }
            else
            {
                list = result.Datas.Where(d => d.ActionIds.Contains("600102")).ToList();
            }

            args.DataSource = list;
        }

        #endregion
    }
}
