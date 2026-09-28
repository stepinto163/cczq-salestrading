using com.gateway.log;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using STC.Common.Data;
using STC.Common.Handler.Admin;
using STC.Common.Handler.Base;
using STC.Common.Handler.PP;
using STC.Common.Handler.PT;
using STC.Common.Handler.SystemParam;
using STC.Common.Handler.Table;
using STC.Common.Model;
using STC.Common.Model.Admin;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.PP;
using STC.Common.Model.PT;
using STC.Common.Model.Table.WorkBrench;
using STC.Common.Model.TBook;
using STC.Common.Utils;
using STC.Common.View;
using STC.Common.View.Component;
using STC.Common.View.UI;
using STC.Common.ViewModel;
using STC.Common.ViewModel.Order.InnerPanel;
using STC.PT.Plugin.Model;
using STC.PT.Plugin.Utils;
using STC.PT.Plugin.View.ExecManage;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;

namespace STC.PT.Plugin.ViewModel.ExecManage
{
    public class MatchIntentionViewModel : ViewModelBase
    {
        public MatchIntentionView matchIntentionView;
        private bool _DataGridIsEnabled = true;
        public bool DataGridIsEnabled
        {
            get { return _DataGridIsEnabled; }
            set
            {
                _DataGridIsEnabled = value;
                this.OnPropertyChanged();
            }
        }
        private PtExecordersData _currentItem;

        public PtExecordersData CurrentItem
        {
            get { return _currentItem; }
            set
            {
                _currentItem = value;
                if (value != null)
                {
                    if (!IsInvestor)
                    {
                        if (!string.IsNullOrEmpty(_currentItem.MatchQty))
                        {
                            RelatedQty = _currentItem.MatchQty;
                        }
                        LastQty = _currentItem.LastQty;
                    }
                    else
                    {
                        RelatedQty = "--";
                        LastQty = "--";
                    }
                    if ("1".Equals(CurrentItem.Delflag) || "1".Equals(CurrentItem.Sendstatus))
                    {
                        DataGridIsEnabled = false;
                    }
                }
                this.OnPropertyChanged();
            }
        }
        private bool _IsInvestor = false;
        public bool IsInvestor

        {
            get { return _IsInvestor; }
            set
            {
                _IsInvestor = value;
                this.OnPropertyChanged();
            }
        }

        private bool _IsAdd = false;
        public bool IsAdd

        {
            get { return _IsAdd; }
            set
            {
                _IsAdd = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// 资源锁
        /// </summary>
        private object _lockObj = new object();
        public object LockObj
        {
            get
            {
                return _lockObj;
            }
        }
        /// <summary>
        /// 选中的行数据
        /// </summary>
        private IntentionQuoteItem _selectedItem;
        public IntentionQuoteItem SelectedItem
        {
            get
            {
                return _selectedItem;
            }
            set
            {
                _selectedItem = value;
                this.OnPropertyChanged();
            }
        }
        private ObservableCollection<IntentionQuoteItem> _IntentionOrders = new ObservableCollection<IntentionQuoteItem>();

        public ObservableCollection<IntentionQuoteItem> IntentionOrders
        {
            get
            {
                if (_IntentionOrders == null)
                    _IntentionOrders = new ObservableCollection<IntentionQuoteItem>();

                return _IntentionOrders;
            }

            set
            {
                _IntentionOrders = value;
                this.OnPropertyChanged(nameof(IntentionOrders));
            }
        }

        private CollectionViewSource mv = new CollectionViewSource();
        public CollectionViewSource Mv
        {
            get { return mv; }
            set
            {
                mv = value;
                this.OnPropertyChanged(nameof(Mv));
            }
        }

        private string _relatedQty;

        public string RelatedQty
        {
            get { return _relatedQty; }
            set
            {
                _relatedQty = value;
                this.OnPropertyChanged();
            }
        }

        private string _LastQty;

        public string LastQty
        {
            get { return _LastQty; }
            set
            {
                _LastQty = value;
                this.OnPropertyChanged();
            }
        }
        private bool _ConfirmEnable = true;
        public bool ConfirmEnable
        {
            get { return _ConfirmEnable; }
            set
            {
                _ConfirmEnable = value;
                this.OnPropertyChanged();
            }
        }

        private int _deciamlLength;

        public int DeciamlLength
        {
            get { return _deciamlLength; }
            set
            {
                _deciamlLength = value;
                this.OnPropertyChanged();
            }
        }

        private decimal _rangeValue;
        public decimal RangeValue
        {
            get { return _rangeValue; }
            set
            {
                _rangeValue = value;
                this.OnPropertyChanged();
            }
        }

        private static ObservableCollection<KeyValuePair<string, string>> _priceType_empty_Dic = new ObservableCollection<KeyValuePair<string, string>>();
        public static ObservableCollection<KeyValuePair<string, string>> PriceTypeEmptyDic
        {
            get
            {
                if (_priceType_empty_Dic.Count == 0)
                {
                    _priceType_empty_Dic.Add(new KeyValuePair<string, string>("1", "净价"));//["1"] = "净价";
                    //_priceType_empty_Dic.Add(new KeyValuePair<string, string>("2", "全价"));//["2"] = "全价";
                    _priceType_empty_Dic.Add(new KeyValuePair<string, string>("3", "到期"));//["3"] = "到期收益率";
                    _priceType_empty_Dic.Add(new KeyValuePair<string, string>("7", "行权"));//["7"] = "行权收益率";
                }
                return _priceType_empty_Dic;
            }
        }

        private static ObservableCollection<TreeNodeModel> _MultiTreeComboxItemsSource = new ObservableCollection<TreeNodeModel>();

        public static ObservableCollection<TreeNodeModel> MultiTreeComboxItemsSource
        {
            get
            {
                return _MultiTreeComboxItemsSource;
            }
        }


        public ICommand RefreshCommand { get { return new RelayCommand(RefreshClick); } }
        public ICommand ConfirmCommand { get { return new RelayCommand(ConfirmClick); } }
        public ICommand AddCommand { get { return new RelayCommand(AddClick); } }

        public ICommand DelRecordCommand { get { return new ComRelayCommand(OnDelCommandClick); } }
        public ICommand SaveRecordCommand { get { return new ComRelayCommand(OnSaveCommandClick); } }

        public event Action RequestClose;

        private IntentionQuoteRequest intentionReq = new IntentionQuoteRequest();

        private List<string> matchedIntentionList = new List<string>();

        private List<IntentionQuoteInfo> IntentionQuoteList = new List<IntentionQuoteInfo>();

        private List<string> addIntentionList = new List<string>();

        private Dictionary<string, IntentionQuoteItem> dicBindQtyEdited = new Dictionary<string, IntentionQuoteItem>();

        private string PriceType;

        public MatchIntentionViewModel()
        {
            SendPriceWarnViewModel.OnConfirmChanged += SendPriceWarnViewModel_OnConfirmChanged;
            BaseHandler.Instance.SubscribeLoadDataStatus(onloadData);
            InitBookUser();

        }



        private void InitBookUser()
        {
            _MultiTreeComboxItemsSource = !STC.Common.Utils.CommonUtils.IsInvestor() ? STC.Common.Handler.PP.PPHandler.Instance.GetTradeTBookGroup() : STC.Common.Handler.PP.PPHandler.Instance.GetFundAccountTBookGroup();
        }

        public bool isLoad = false;
        private void onloadData(bool obj)
        { 
            if (obj)
            {
                if (!isLoad)
                {
                    isLoad = true;
                    ThreadPoolHelper.InvokeThreadPool((o) =>
                    {
                        Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background, (Action)(() =>
                        {
                            Init();
                        }));
                    });
                }
            }
        }

        private bool isInit = false;
        private void Init()
        {
            isInit = true;
            try
            {
                this.Mv.Source = IntentionOrders;
                this.Mv.IsLiveFilteringRequested = true;
                this.Mv.IsLiveSortingRequested = true;

                //this.Mv.SortDescriptions.Add(new SortDescription("IntentionId", ListSortDirection.Descending));
                this.Mv.SortDescriptions.Add(new SortDescription("IsOldChecked", ListSortDirection.Descending));
                matchedIntentionList.Clear();
                Task.Run(() =>
                {
                    InitBond(CurrentItem.SecurityID);
                    QueryAssociateByExecID();
                    QueryIntention();
                    QueryAssociate();
                    if (IsAdd)
                        AddClick();

                });
            }
            catch (Exception e)
            {
                LogHelper.Error("ExecuterOrderHandler init:", e);
            }
        }
        private void QueryIntention()
        {
            intentionReq.securityId = CurrentItem.SecurityID;
            intentionReq.side = CurrentItem.Side;
            if (LocalEnv.UserInfo.RoleType == RoleType.BUYER_ROLE)
            {
                intentionReq.quoteType = "BUYER_INTENTION";
            }
            else
            {
                intentionReq.quoteType = "INTENTION";
            }
            intentionReq.userName = LocalEnv.UserInfo.UserName;
            if (!string.IsNullOrEmpty(LocalEnv.UserInfo.Termids))
            {
                intentionReq.biztypeid = LocalEnv.UserInfo.Termids;
            }
            var reply = TableOldHandler.Instance.QueryIntentionQuote(intentionReq);

            if (reply != null && reply.Datas != null)
            {
                Application.Current.Dispatcher.Invoke((Action)(() =>
                {
                    IntentionOrders.Clear();
                    IntentionQuoteList.Clear();
                    foreach (IntentionQuoteInfo obj in reply.Datas)
                    {
                        string execStatus = IntentionOldHandler.Instance.ExtractByKey(obj.subStatus, "EXEC_STATUS");

                        if (("80".Equals(obj.status) ||
                        "81".Equals(obj.status) ||
                        ("84".Equals(obj.status) && ("9".Equals(execStatus) || "10".Equals(execStatus))) ||
                        matchedIntentionList.Contains(obj.intentionId)) &&
                        (!IsInvestor ||
                        (IsInvestor && LocalEnv.UserInfo.UserName.Equals(obj.partyManager))))
                        {
                            IntentionQuoteList.Add(obj);
                            OnIntentionOrder(obj);
                        }
                    }
                }));
            }
        }
        private IntentionQuoteItem OnIntentionOrder(IntentionQuoteInfo order)
        {
            IntentionQuoteItem dataRow = null;
            try
            {
                lock (_lockObj)
                {
                    dataRow = this.IntentionOrders.FirstOrDefault(o => o.IntentionId == order.intentionId);
                }
                bool isold = false;
                if (dataRow != null)
                {
                    isold = true;
                }
                else
                {
                    dataRow = new IntentionQuoteItem();
                    dataRow.IntentionId = order.intentionId;
                }
                dataRow.InvestorName = order.partyManagerName;
                dataRow.Investor = order.partyManager;
                dataRow.Trader_Buyer = order.partyTrader;
                dataRow.TraderName_Buyer = order.partyTraderName;
                // 原始数据
                dataRow.BookId = order.bookId;
                dataRow.BookName = order.bookName;
                dataRow.AccountID = order.accountID;
                dataRow.AccountName = order.accountName;
                dataRow.HsProductId = order.hsProductId;

                if (!string.IsNullOrEmpty(order.accountID))
                {
                    dataRow.SelectedTradeAccountCode = order.accountID;
                }
                else if (!string.IsNullOrWhiteSpace(order.bookId))
                {
                    dataRow.SelectedTradeAccountCode = order.bookId;
                }
                dataRow.Side = order.side;
                dataRow.Price = order.price;
                dataRow.Price_Edit = order.price;
                dataRow.PriceType = order.priceType;
                if (order.priceType == "1")
                {
                    dataRow.Price_Display = order.cleanPrice;
                }
                else if (order.priceType == "2")
                {
                    dataRow.Price_Display = order.fullPrice;
                }
                else
                {
                    dataRow.Price_Display = order.price;
                }
                dataRow.WholeFlag = order.wholeFlag;
                dataRow.Status = order.status;
                dataRow.DelFlag = order.delflag;
                dataRow.SettlType = order.settlType;
                dataRow.SettlType_Edit = dataRow.SettlType;
                if (!string.IsNullOrEmpty(order.inf1) && JsonSplit.IsJson(order.inf1))
                {
                    JObject jsonObj = JObject.Parse(order.inf1);
                    if (jsonObj.ContainsKey("HsProductId"))
                    {
                        dataRow.HsProductId = jsonObj["HsProductId"] == null ? string.Empty : jsonObj["HsProductId"].ToString();
                    }
                }
                if (JsonSplit.IsJson(order.info5))
                {
                    NoteData info5tag = JsonConvert.DeserializeObject<NoteData>(order.info5);
                    dataRow.RiskNote = info5tag.RISK_CHECK_RESULT;
                }
                //dataRow.RiskNote = order
                dataRow.BindQty = 0;
                dataRow.BindQty_Edit = 0;//order.qty.HasValue ? (long)order.qty.Value : 0;
                //dataRow.MatchedQty = order.ma
                dataRow.EditEnable = "False";
                dataRow.EditBtnVisible = "0";
                dataRow.SaveBtnVisible = "1";
                dataRow.Qty = order.qty;
                if (addIntentionList.Contains(order.intentionId))
                    dataRow.DelBtnVisible = "0";
                if (!isold)
                    IntentionOrders.Add(dataRow);
            }
            catch (Exception e)
            {
                LogHelper.Error("OnIntentionOrder error:", e);
            }
            return dataRow;
        }

        private void QueryAssociateByExecID()
        {
            IntentionExecutionAssociateReq req = new IntentionExecutionAssociateReq();
            req.executionIds = new List<string>();
            req.executionIds.Add(this.CurrentItem.ExecID);
            req.action = "0";
            IntentionExecutionAssociateReplyPo reply = TableOldHandler.Instance.ProcessIntentionExecutionAssociate(req);

            if (reply != null && reply.IsSuccess() && reply.data != null && reply.data.Count > 0)
            {
                Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background, (Action)(() =>
                {
                    foreach (TableIntentionExecution d in reply.data)
                    {
                        if ("1".Equals(d.delflag))
                        {
                            continue;
                        }
                        matchedIntentionList.Add(d.intentionId);
                    }
                }));
            }
        }

        private void QueryAssociate()
        {
            IntentionExecutionAssociateReq req = new IntentionExecutionAssociateReq();
            req.executionIds = new List<string>();
            req.executionIds.Add(this.CurrentItem.ExecID);
            req.intentionIds = new List<string>();
            foreach (IntentionQuoteItem item in IntentionOrders)
            {
                req.intentionIds.Add(item.IntentionId);
            }
            req.action = "0";
            IntentionExecutionAssociateReplyPo reply = TableOldHandler.Instance.ProcessIntentionExecutionAssociate(req);

            if (reply != null && reply.IsSuccess() && reply.data != null && reply.data.Count > 0)
            {
                Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background, (Action)(() =>
                {
                    try
                    {
                        //foreach (IntentionQuoteItem item in this.IntentionOrders)
                        //{
                        //    item.IsChecked = false;
                        //    item.IsOldChecked = false;
                        //    item.MatchedQty = null;
                        //    item.BindQty = 0;
                        //    item.BindQty_Edit = 0;
                        //    //if (!"80".Equals(item.Status) && !"81".Equals(item.Status))
                        //    //{
                        //    //    item.IsCheckEnable = false;
                        //    //}
                        //}
                        bool isMatched = false;
                        foreach (TableIntentionExecution d in reply.data)
                        {
                            if ("1".Equals(d.delflag))
                            {
                                continue;
                            }
                            IntentionQuoteItem execData = this.IntentionOrders.FirstOrDefault<IntentionQuoteItem>(x => x.IntentionId == d.intentionId);
                            if (execData != null)
                            {
                                isMatched = true;
                                if (!d.executionId.Equals(CurrentItem.ExecID))
                                {
                                    if (execData.MatchedQty == null)
                                    {
                                        execData.MatchedQty = Common.Utils.CommonUtils.ConvertToLong(Common.Utils.CommonUtils.ConvertToString(d.quantity));
                                    }
                                    else
                                    {
                                        execData.MatchedQty = execData.MatchedQty + Common.Utils.CommonUtils.ConvertToLong(Common.Utils.CommonUtils.ConvertToString(d.quantity));
                                    }
                                }
                                if (d.executionId.Equals(CurrentItem.ExecID))
                                {
                                    execData.ID = d.id;
                                    execData.BindQty = Common.Utils.CommonUtils.ConvertToLong(Common.Utils.CommonUtils.ConvertToString(d.quantity));
                                    execData.BindQty_Edit = Common.Utils.CommonUtils.ConvertToLong(Common.Utils.CommonUtils.ConvertToString(d.quantity));
                                    execData.IsChecked = true;
                                    execData.IsOldChecked = true;

                                    if (!"80".Equals(execData.Status) && !"81".Equals(execData.Status))
                                    {
                                        execData.IsCheckEnable = false;
                                    }
                                }
                            }
                        }
                        if (!isMatched)
                        {
                            setDefaultMatch();
                        }
                    }
                    catch (Exception e)
                    {
                        LogHelper.Error(e);
                    }
                    finally {
                        isInit = false;
                    }
                }));
            }
            if (addIntentionList.Count > 0)
            {
                foreach (string d in addIntentionList)
                {
                    IntentionQuoteItem execData = this.IntentionOrders.FirstOrDefault<IntentionQuoteItem>(x => x.IntentionId == d);
                    if (execData != null)
                    {
                        execData.IsChecked = true;
                    }
                }
            }
            //else
            //{
            //    setDefaultMatch();
            //}
        }

        private void setDefaultMatch()
        {
            if (this.IntentionOrders.Count == 0 || CurrentItem == null)
            {
                return;
            }

            IntentionQuoteItem intentionQuoteItem = IntentionOrders[0];
            if ((intentionQuoteItem.MatchedQty == null || intentionQuoteItem.MatchedQty <= 0) && intentionQuoteItem.Qty.HasValue && decimal.Compare(intentionQuoteItem.Qty.Value, Common.Utils.CommonUtils.ConvertToDecimal(CurrentItem.LastQty)) == 0)
            {
                intentionQuoteItem.IsChecked = true;
                intentionQuoteItem.BindQty_Edit = intentionQuoteItem.Qty.HasValue ? Common.Utils.CommonUtils.ConvertToLong(Common.Utils.CommonUtils.ConvertToString(intentionQuoteItem.Qty.Value)) : 0;
            }
        }

        public void RefreshClick()
        {
            try
            {
                IsAdd = false;
                addIntentionList.Clear();
                dicBindQtyEdited.Clear();
                ThreadPoolHelper.InvokeThreadPool((o) =>
                {
                    Task.Run(() =>
                    {
                        QueryIntention();
                        QueryAssociate();
                    });
                });
            }
            catch (Exception e)
            {
                LogHelper.Error("ExecuterOrderHandler init:", e);
            }
        }

        public async Task<bool> RefreshAsync()
        {
            try
            {
                await Task.WhenAll(
                    Task.Run(() => QueryIntention()),
                    Task.Run(() => QueryAssociate())
                );
                isInit = false;
                return isInit;
            }
            catch (Exception ex)
            {
                LogHelper.Error("刷新失败", ex);
                isInit = true;
                throw; // 或返回false
            }
        }


        private void ConfirmClick()
        {
            Task.Run(() =>
            {
                try
                {
                    ConfirmEnable = false;
                    decimal newMatchQty = 0;
                    bool isMatchUpIntention = false;
                    IntentionExecutionAssociateReq req = new IntentionExecutionAssociateReq();
                    req.requestList = new List<TableIntentionExecution>();
                    req.action = "1";
                    StringBuilder accountStr = new StringBuilder();
                    StringBuilder accountnameStr = new StringBuilder();
                    StringBuilder partymanageridStr = new StringBuilder();
                    StringBuilder partymanagernameStr = new StringBuilder();
                    foreach (IntentionQuoteItem row in this.IntentionOrders)
                    {
                        TableIntentionExecution subReq = new TableIntentionExecution();
                        subReq.id = row.ID;
                        subReq.intentionId = row.IntentionId;
                        subReq.executionId = this.CurrentItem.ExecID;
                        subReq.securityId = this.CurrentItem.SecurityID;
                        subReq.side = this.CurrentItem.Side;
                        subReq.quantity = row.BindQty_Edit.HasValue ? row.BindQty_Edit.Value : decimal.Zero;
                        subReq.partyTrader = row.Trader_Buyer;
                        subReq.partyTraderName = row.TraderName_Buyer;
                        subReq.partyManager = row.Investor;
                        subReq.partyManagerName = row.InvestorName;
                        subReq.bookId = row.BookId;
                        subReq.bookName = row.BookName;
                        subReq.accountID = row.AccountID;
                        subReq.accountName = row.AccountName;
                        subReq.intentionStatus = row.Status;
                        subReq.executionStatus = this.CurrentItem.Sendstatus;
                        string action = string.Empty;
                        if (row.IsChecked)
                        {
                            if (subReq.quantity == 0)
                            {
                                showMessage("匹配量不能为0！", false);
                                return;
                            }
                            newMatchQty += subReq.quantity;
                            if (string.IsNullOrEmpty(row.ID))
                            {
                                action = "0";
                                if (((row.MatchedQty.HasValue ? row.MatchedQty.Value : decimal.Zero)
                                + (row.BindQty_Edit.HasValue ? row.BindQty_Edit.Value : decimal.Zero))
                                > (row.Qty.HasValue ? row.Qty.Value : decimal.Zero))
                                {
                                    isMatchUpIntention = true;
                                }
                            }
                            else
                            {
                                action = "1";
                                if (((row.MatchedQty.HasValue ? row.MatchedQty.Value : decimal.Zero)
                                + (row.BindQty_Edit.HasValue ? row.BindQty_Edit.Value : decimal.Zero))
                                > (row.Qty.HasValue ? row.Qty.Value : decimal.Zero))
                                {
                                    isMatchUpIntention = true;
                                }
                            }
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(row.ID))
                            {
                                action = "2";
                                newMatchQty -= row.MatchedQty.HasValue ? row.MatchedQty.Value : decimal.Zero;
                                subReq.quantity = 0;
                            }
                        }
                        subReq.action = action;


                        if (!string.IsNullOrEmpty(action))
                        {
                            if (!"2".Equals(action))
                            {
                                if (!string.IsNullOrEmpty(subReq.accountID))
                                {
                                    accountStr.Append(subReq.accountID).Append("|");
                                    accountnameStr.Append(subReq.accountName).Append("|");
                                }
                                if (!string.IsNullOrEmpty(subReq.partyManager))
                                {
                                    partymanageridStr.Append(subReq.partyManager).Append("|");
                                    partymanagernameStr.Append(subReq.partyManagerName).Append("|");
                                }
                            }
                            req.requestList.Add(subReq);
                        }
                    }

                    decimal lastQty = Common.Utils.CommonUtils.ConvertToDecimal(this.CurrentItem.LastQty);
                    if (newMatchQty > lastQty)
                    {
                        showMessage("总匹配量大于成交量，请修改！", false);
                        return;
                    }

                    bool result = true;
                    if (isMatchUpIntention)
                    {
                        result = Common.View.Component.CustomMessageBox.Question1("存在匹配量大于个需可匹配量的记录，是否继续保存", "匹配个需", matchIntentionView, "取消", "保存");
                    }
                    if (result == true)
                    {
                        bool checkRiskStatus = checkRisk();
                        if (checkRiskStatus == false)
                        {
                            result = Common.View.Component.CustomMessageBox.Question1("存在匹配触发风控，是否继续保存", "匹配个需", matchIntentionView, "取消", "保存");
                        }
                        if (result == true)
                        {
                            IntentionExecutionAssociateReplyPo reply = TableOldHandler.Instance.ProcessIntentionExecutionAssociate(req);

                            if (reply != null && reply.IsSuccess())
                            {
                                dicBindQtyEdited.Clear();
                                Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background, (Action)(() =>
                                {
                                    RequestClose.Invoke();
                                }));
                            }
                            else
                            {
                                string errMsg = reply == null ? "保存关联关系失败" : reply.Msg;
                                showMessage(errMsg, false);
                            }
                        }
                    }
                }
                finally
                {
                    ConfirmEnable = true;
                }
            });
        }
        /// <summary>
        /// 提示消息弹框
        /// </summary>
        /// <param name="message"></param>
        private void showMessage(string message, bool autoClose)
        {
            Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background, (Action)(() =>
            {
                MessageBoxGUI me = new MessageBoxGUI();
                me.HideCancelButton();
                me.setWarnMsg(message, autoClose);
                me.ShowDialog();

            }));
        }
        #region 勾选
        private bool _hasSelected = false;
        // 是否选中全选多选框
        private bool _IsAllChecked = false;
        public bool IsAllChecked
        {
            get
            {
                return _IsAllChecked;
            }

            set
            {
                if (_IsAllChecked != value)
                {
                    _IsAllChecked = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private volatile bool isAdjustAllChkStatus = false;
        public void CheckBoxAllValueChange(bool isAllChecked)
        {
            int workingDataCount = 0;
            lock (_lockObj)
            {
                foreach (IntentionQuoteItem item in this.IntentionOrders)
                {
                    item.IsChecked = isAllChecked;
                    workingDataCount++;
                }
            }


            if (workingDataCount == 0)
            {
                IsAllChecked = false;
                _hasSelected = false;
            }
            else
            {
                IsAllChecked = isAllChecked;
                _hasSelected = isAllChecked;
            }
        }

        public void AdjustAllChkStatus()
        {
            bool isCheckAll = true;

            int workingDataCount = 0;
            lock (_lockObj)
            {
                this._hasSelected = false;
                foreach (IntentionQuoteItem item in this.IntentionOrders)
                {
                    workingDataCount++;
                    if (item.IsChecked == true)
                    {
                        this._hasSelected = true;
                    }
                    if (item.IsChecked == false)
                    {
                        isCheckAll = false;
                    }
                    if (this._hasSelected == true && isCheckAll == false)
                    {
                        break;
                    }
                }
                IsAllChecked = isCheckAll && workingDataCount != 0;
            }

        }

        public void CheckBoxSingleClick()
        {
            bool flag = true;
            int i = 0;
            lock (_lockObj)
            {
                foreach (IntentionQuoteItem item in this.IntentionOrders)
                {
                    flag = flag && item.IsChecked;
                    if (item.IsChecked)
                    {
                        i++;
                    }
                }
                _IsAllChecked = this.IntentionOrders.Count > 0 && flag;
            }
            this.OnPropertyChanged("IsAllChecked");
        }
        #endregion

        #region 风控检查
        Dictionary<string, PtExecOrderSaveReq> execDic = new Dictionary<string, PtExecOrderSaveReq>();
        Dictionary<string, List<TPlaceOrderResult>> warnReplyDic = new Dictionary<string, List<TPlaceOrderResult>>();

        public bool checkRisk()
        {
            //if (SelectedItem != null)
            //{
            //    if (!SelectedItem.BindQty_Edit.HasValue || SelectedItem.BindQty_Edit.Value.CompareTo(0) <= 0 || !SelectedItem.IsChecked)
            //    {
            //        SelectedItem.RiskNote = "";
            //        return true;
            //    }
            if (isInit)
                return true;
            List<NewOrderSingle> reqList = new List<NewOrderSingle>();
            bool flag = true;
            foreach (IntentionQuoteItem row in this.IntentionOrders)
            {
                if (!row.BindQty_Edit.HasValue || row.BindQty_Edit.Value.CompareTo(0) <= 0 || !row.IsChecked || "1".Equals(row.Side) ||
                    string.IsNullOrEmpty(row.AccountID))
                {
                    row.RiskNote = "";
                    continue;
                }
                IntentionQuoteItem item = row;
                NewOrderSingle req = new NewOrderSingle();
                req.SecurityID = CurrentItem.SecurityID;
                req.Venues = CurrentItem.Venues;//成交渠道
                req.VenueTypeGW = CurrentItem.Venuetypegw; //成交渠道
                req.StrategyOrderId = item.IntentionId;
                req.ClOrdID = CurrentItem.ExecID;
                req.MarketIndicator = "4";
                req.ActionFlag = "8";
                req.Side = CurrentItem.Side;
                req.OrdStatus = "0";
                req.OrderQty = item.BindQty_Edit.ToString();//关联量输入量
                req.Info1 = CurrentItem.TradeDate;
                req.AccountID = item.AccountID;//?个需的
                req.UserName = CurrentItem.Username;
                req.CounterPartyName = CurrentItem.Counterpartyfullname;
                req.Price = CurrentItem.LastPx;
                req.Info2 = CurrentItem.CounterPartyName;
                req.SettlType = "+1".Equals(CurrentItem.SettlType) ? "2" : "1";
                req.Symbol = CurrentItem.Symbol;
                string bookid = string.Empty;
                if (!string.IsNullOrEmpty(item.BookId))
                {
                    bookid = item.BookId;
                }
                req.BookID = bookid;
                reqList.Add(req);
                if (reqList.Count > 0)
                {
                    BatchCheckRiskReplyPo po = PPHandler.Instance.BatchCheckRisk(reqList);
                    if (po != null && po.Code == "10000" && po.Msg != null && po.Msg.Count > 0)
                    {
                        string clordid = CurrentItem.ExecID;
                        if (po.Msg.ContainsKey(clordid))
                        {
                            RiskResult riskResult = po.Msg[clordid];
                            if (riskResult != null)
                            {
                                StringBuilder sb = new StringBuilder();
                                if (riskResult.FailMsg != null && riskResult.FailMsg.Count > 0)
                                {
                                    flag = false;
                                    List<TPlaceOrderResult> orderRisks = riskResult.FailMsg;
                                    foreach (TPlaceOrderResult placeOrderResult in orderRisks)
                                    {
                                        item.RiskNote = placeOrderResult.Reason;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            //}
            return flag;
        }
        #endregion


        private void AddClick()
        {
            IsAdd = true;
            ConfirmEnable = false;
            Application.Current.Dispatcher.Invoke((Action)(() =>
            {
                try
                {
                    InsertIntentionQuoteReq req = convert(CurrentItem);
                    IntentionQuoteInfo order = new IntentionQuoteInfo();
                    if (req != null && req.quoteList != null && req.quoteList.Count > 0)
                        order = req.quoteList[0];
                    IntentionQuoteItem dataRow;
                    lock (_lockObj)
                    {
                        dataRow = this.IntentionOrders.FirstOrDefault(o => o.IntentionId == order.intentionId);
                    }
                    bool isold = false;
                    if (dataRow != null)
                    {
                        isold = true;
                    }
                    else
                    {
                        dataRow = new IntentionQuoteItem();
                        dataRow.IntentionId = order.intentionId;
                    }
                    if (STC.Common.Utils.CommonUtils.IsInvestor())
                    {
                        //dataRow.IsChecked = true;
                        //dataRow.IsOldChecked = false;
                        dataRow.InvestorEnable = "False";
                        dataRow.TraderEnable = "False";
                        dataRow.Investor = LocalEnv.UserInfo.UserName;
                        dataRow.InvestorName = LocalEnv.UserInfo.FullName;
                        List<StcTbookUsers> investors = Common.Utils.CommonUtils.GetInvestors();
                        List<StcTbookUsers> li = investors.Where(d => d.Username.Equals(LocalEnv.UserInfo.UserName)).ToList();
                        dataRow.SelectedInvestor_Edit = li[0];
                        List<StcTbookUsers> traders = Common.Utils.CommonUtils.GetTraders();
                        List<string> traderList = AdminHandler.Instance.GetTraderList(LocalEnv.UserInfo.UserId);
                        if (traderList != null)
                        {
                            List<StcTbookUsers> list = traders.Where(d => traderList.Contains(d.Userid)).ToList();//多条件筛选，新功能增加
                            if (list != null && list.Count > 0)
                            {
                                StcTbookUsers stcTbookUsers = list[0];
                                dataRow.Trader_Buyer = stcTbookUsers.Username;
                                dataRow.TraderName_Buyer = stcTbookUsers.Name;
                                dataRow.SelectedTrader_Buy_Edit = list[0];
                            }
                        }
                        dataRow.Status = "80";
                    }
                    else
                    {
                        dataRow.InvestorEnable = "True";
                        dataRow.TraderEnable = "False";
                        List<StcTbookUsers> traders = Common.Utils.CommonUtils.GetTraders();
                        dataRow.Trader_Buyer = LocalEnv.UserInfo.UserName;
                        dataRow.TraderName_Buyer = LocalEnv.UserInfo.FullName;
                        List<StcTbookUsers> list = traders.Where(d => d.Username.Equals(LocalEnv.UserInfo.UserName)).ToList();
                        dataRow.SelectedTrader_Buy_Edit = list[0];
                        dataRow.Status = "81";
                    }
                    dataRow.BookName = order.bookName;
                    dataRow.BookId = order.bookId;
                    dataRow.AccountName = order.accountName;
                    dataRow.AccountID = order.accountID;
                    dataRow.Side = order.side;

                    dataRow.PriceType = PriceType;
                    if (dataRow.PriceType == "1")
                    {
                        dataRow.Price = order.cleanPrice;
                        dataRow.Price_Edit = order.cleanPrice;
                    }
                    else if (dataRow.PriceType == "7")
                    {
                        dataRow.Price = order.Yieldtoexecution;
                        dataRow.Price_Edit = order.Yieldtoexecution;
                    }
                    else
                    {
                        dataRow.Price = order.price;
                        dataRow.Price_Edit = order.price;
                    }
                    dataRow.Qty = order.qty;
                    dataRow.WholeFlag = order.wholeFlag;
                    dataRow.DelFlag = order.delflag;
                    dataRow.SettlType_Edit = dataRow.SettlType = "3";//默认 明天+0
                    if (!string.IsNullOrEmpty(order.inf1) && JsonSplit.IsJson(order.inf1))
                    {
                        JObject jsonObj = JObject.Parse(order.inf1);
                        if (jsonObj.ContainsKey("HsProductId"))
                        {
                            dataRow.HsProductId = jsonObj["HsProductId"] == null ? string.Empty : jsonObj["HsProductId"].ToString();
                        }
                    }//decimalValue.HasValue ? (long)decimalValue.Value : 0;
                    dataRow.BindQty = dataRow.Qty.HasValue ? (long)dataRow.Qty.Value : 0;
                    dataRow.BindQty_Edit = dataRow.Qty.HasValue ? (long)dataRow.Qty.Value : 0;
                    dataRow.EditEnable = "True";
                    dataRow.EditBtnVisible = "1";
                    dataRow.SaveBtnVisible = "0";
                    dataRow.DelBtnVisible = "0";
                    dataRow.ClOrdID = order.clordId;
                    //dataRow.MatchedQty = order.ma
                    if (!isold)
                        IntentionOrders.Add(dataRow);
                }
                catch (Exception e)
                {
                    LogHelper.Error("OnIntentionOrder error:", e);
                }
            }));
        }

        private STC.Common.Model.Table.WorkBrench.InsertIntentionQuoteReq convert(PtExecordersData req)
        {

            STC.Common.Model.Table.WorkBrench.InsertIntentionQuoteReq request = new STC.Common.Model.Table.WorkBrench.InsertIntentionQuoteReq();
            request.userName = LocalEnv.UserInfo.UserName;
            string quotetype = "INTENTION";
            if (LocalEnv.UserInfo.RoleType == RoleType.BUYER_ROLE)
            {
                quotetype = "BUYER_INTENTION";
            }
            request.quoteType = quotetype;
            request.quoteList = new List<IntentionQuoteInfo>();
            IntentionQuoteInfo record = new IntentionQuoteInfo();
            record.counterPartyID = req.CounterPartyId;//.counterpartyid;
            record.counterPartyName = req.CounterPartyName;//.counterpartyname;
            record.counterPartySubID = req.CounterPartySubId;//.counterpartysubid;
            record.counterPartySubName = req.CounterPartySubName;//.counterpartysubname;
            record.traderId = req.TraderID;//.traderid;
            record.traderName = req.TraderName;//.tradername;
            record.side = req.Side;//.side;
            record.securityId = req.SecurityID;//.securityid;
            record.symbol = req.Symbol;//.symbol;
            record.price = STC.Common.Utils.CommonUtils.ConvertToDecimalOrNull(req.LastPx);//price);
            record.priceType = PriceType;
            record.cleanPrice = STC.Common.Utils.CommonUtils.ConvertToDecimalOrNull(req.NetPrice);
            record.qty = STC.Common.Utils.CommonUtils.ConvertToDecimalOrNull(req.LastQty);
            record.Yieldtoexecution = STC.Common.Utils.CommonUtils.ConvertToDecimalOrNull(req.Yieldtoexecution);
            //record.settlType = "+0".Equals(settleType) ? "1" : "2";//"+1".Equals(ExecordersData.SettlType) ? 0 : 1;
            record.userName = req.Username;
            record.closeBy = LocalEnv.UserInfo.UserName;
            record.closeByName = LocalEnv.UserInfo.FullName;
            record.status = "0";
            record.channel = "CLIENT";
            record.clordId = DataTimeSequence.Instance.GetId();
            record.settlType = req.SettlType;
            if (!string.IsNullOrEmpty(req.ExecID))
            {
                /*string execidjson = "{{\"execid\":\"{0}\"}}";
                record.info2 = string.Format(execidjson,req.execid);*/
            }
            record.institutionId = LocalEnv.UserInfo.Termids;
            record.institutionName = LocalEnv.UserInfo.biztype;
            record.userName = LocalEnv.UserInfo.UserName;
            request.quoteList.Add(record);
            return request;
        }




        #region 保存检查
        IntentionQuoteInfo sameData = null;
        private void OnSaveCommandClick()
        {
            IntentionQuoteItem dataRow = SelectedItem;

            InsertIntentionQuoteReq request = new InsertIntentionQuoteReq();

            request.userName = LocalEnv.UserInfo.UserName;
            string quotetype = "INTENTION";
            if (LocalEnv.UserInfo.RoleType == RoleType.BUYER_ROLE)
            {
                quotetype = "BUYER_INTENTION";
            }
            request.quoteType = quotetype;
            request.quoteList = new List<IntentionQuoteInfo>();
            IntentionQuoteInfo order = new IntentionQuoteInfo();
            order.counterPartyID = CurrentItem.CounterPartyId;
            order.counterPartyName = CurrentItem.CounterPartyName;
            order.counterPartySubID = CurrentItem.CounterPartySubId;
            order.counterPartySubName = CurrentItem.CounterPartySubName;
            //order.traderId = CurrentItem.TraderID;
            //order.traderName = CurrentItem.TraderName;
            order.side = dataRow.Side;
            order.securityId = CurrentItem.SecurityID;
            order.symbol = CurrentItem.Symbol;
            order.price = dataRow.Price_Edit;
            order.priceType = dataRow.PriceType;
            if (dataRow.PriceType == "1")
            {
                order.cleanPrice = dataRow.Price_Edit;

            }
            else if (dataRow.PriceType == "2")
            {
                order.fullPrice = dataRow.Price_Edit;
            }
            else
            {
                order.price = dataRow.Price_Edit;
            }
            order.intentionId = dataRow.IntentionId;

            order.partyManagerName = dataRow.InvestorName;
            order.partyManager = dataRow.SelectedInvestor_Edit != null ? dataRow.SelectedInvestor_Edit.Username : ""; //dataRow.Investor;
            order.partyTrader = dataRow.SelectedTrader_Buy_Edit != null ? dataRow.SelectedTrader_Buy_Edit.Username : "";//dataRow.Trader_Buyer;
            order.partyTraderName = dataRow.TraderName_Buyer;
            order.traderId = order.partyTrader;
            order.traderName = order.partyTraderName;
            // 持仓组合id
            string bookid = string.Empty;
            if (dataRow.SelectedGroupItem != null && dataRow.SelectedGroupItem.Count > 0)
            {
                order.bookId = dataRow.SelectedGroupItem[0];
            }
            //持仓组合名称
            string bookName = string.Empty;
            if (dataRow.SelectedGroupText != null && dataRow.SelectedGroupText.Count > 0)
            {
                order.bookName = dataRow.SelectedGroupText[0];
            }
            // 持仓组合账号id
            string accountid = string.Empty;
            if (!string.IsNullOrEmpty(dataRow.SelectedTradeAccountId))
            {
                order.accountID = dataRow.SelectedTradeAccountId;
            }
            // 持仓组合账号名称
            string accountName = string.Empty;
            if (!string.IsNullOrEmpty(dataRow.SelectedTradeAccountName))
            {
                order.accountName = dataRow.SelectedTradeAccountName;
            }
            OrderTimeModel otm = new OrderTimeModel();
            otm.HsProductId = dataRow.HsProductId;
            order.inf1 = JsonConvert.SerializeObject(otm);

            order.qty = dataRow.Qty;
            order.wholeFlag = dataRow.WholeFlag;
            order.status = dataRow.Status;
            order.delflag = dataRow.DelFlag;
            order.settlType = dataRow.SettlType_Edit;
            order.closeBy = LocalEnv.UserInfo.UserName;
            order.closeByName = LocalEnv.UserInfo.FullName;
            order.status = "0";
            order.channel = "CLIENT";
            order.clordId = DataTimeSequence.Instance.GetId();

            order.institutionId = LocalEnv.UserInfo.Termids;
            order.institutionName = LocalEnv.UserInfo.biztype;
            order.userName = LocalEnv.UserInfo.UserName;
            request.quoteList.Add(order);

            //dataRow.BindQty = 0;
            //dataRow.BindQty_Edit = 0;
            //dataRow.EditEnable = "True";
            //dataRow.EditBtnVisible = "1";
            //dataRow.SaveBtnVisible = "0";
            sameData = null;
            if (LocalEnv.UserInfo.RoleType == RoleType.BUYER_ROLE)
            {
                if (!string.IsNullOrEmpty(request.quoteList[0].partyManager) || !string.IsNullOrEmpty(request.quoteList[0].partyTrader))
                {
                    sameData = GetSameIntentionInfo(request.quoteList[0].partyManager, request.quoteList[0].partyTrader, request.quoteList[0].securityId, request.quoteList[0].side, request.quoteList[0].accountID);

                    if (sameData != null)
                    {
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
                                //sendPriceViewModel.SendPriceEnable = true;
                                return;
                            }
                        }
                    }
                }
            }
            Common.Model.Table.WorkBrench.InsertIntentionQuoteReplyPo intentionResult = IntentionOldHandler.Instance.InsertIntentionQuote_Common_New(request);
            if (intentionResult != null)
            {
                if (intentionResult.Code == "10000")
                {
                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        string riskCheckResult = string.Empty;
                        string riskCheckDetail = string.Empty;
                        if (intentionResult != null && intentionResult.Datas != null && intentionResult.Datas.Count > 0)
                        {
                            foreach (Common.Model.Table.WorkBrench.IntentionQuoteInfo obj in intentionResult.Datas)
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
                        reply.Code = intentionResult.Code;
                        reply.Msg = riskCheckDetail;

                        NewOrderSingle nos = new NewOrderSingle();
                        nos.SecurityID = request.quoteList[0].securityId;//req.securityid;
                        nos.OrderQty = request.quoteList[0].qty + "";//req.qty;
                        nos.Side = request.quoteList[0].side;//req.side;
                        SendPriceWarnView warnView = new SendPriceWarnView(reply, nos, request, this.matchIntentionView);
                        warnView.Owner = matchIntentionView;
                        warnView.ShowDialog();
                    }));
                }
                else if (intentionResult.Code == "0000")
                {
                    Application.Current.Dispatcher.Invoke((Action)(() =>
                    {
                        SendPriceWarnViewModel_OnConfirmChanged(intentionResult);
                    }));
                }
                else
                {
                    string errMsg = intentionResult == null ? $"创建个需失败" : "创建个需失败：" + (!intentionResult.IsSuccess() ? intentionResult.Msg : string.Empty);
                    if (!string.IsNullOrEmpty(errMsg))
                    {
                        showMessage(errMsg, false);
                    }
                    // CloseWindow();
                }
            }
            else
            {
                string errMsg = intentionResult == null ? $"创建个需失败" : "创建个需失败：" + (!intentionResult.IsSuccess() ? intentionResult.Msg : string.Empty);
                if (!string.IsNullOrEmpty(errMsg))
                {
                    showMessage(errMsg, false);
                }
                // CloseWindow();
            }
        }

        private void SendPriceWarnViewModel_OnConfirmChanged(InsertIntentionQuoteReplyPo intentionResult)
        {
            if (intentionResult == null || intentionResult.Datas == null || intentionResult.Datas.Count == 0)
                return;
            addIntentionList.Add(intentionResult.Datas[0].intentionId);
            Requery(intentionResult);
        }

        private async void Requery(InsertIntentionQuoteReplyPo intentionResult = null)
        {
            await RequeryAsync();
            Application.Current.Dispatcher.Invoke((Action)(() =>
            {
                foreach (string s in dicBindQtyEdited.Keys)
                {
                    IntentionQuoteItem editedItem = this.IntentionOrders.FirstOrDefault(a => a.IntentionId.Equals(s));
                    if (editedItem != null)
                    {
                        editedItem.BindQty_Edit = dicBindQtyEdited[s].BindQty_Edit.HasValue ? dicBindQtyEdited[s].BindQty_Edit.Value : 0;
                        editedItem.IsOldChecked = true;
                        editedItem.IsChecked = true;
                    }
                }
                if (intentionResult != null)
                {
                    IntentionQuoteItem addedItem = this.IntentionOrders.FirstOrDefault(a => a.IntentionId.Equals(intentionResult.Datas[0].intentionId));
                    if (addedItem != null)
                    {
                        addedItem.BindQty_Edit = intentionResult.Datas[0].qty.HasValue ? (long)intentionResult.Datas[0].qty.Value : 0;
                        addedItem.IsOldChecked = true;
                        addedItem.IsChecked = true;
                        CacheBindQTY(addedItem);
                    }
                    this.Mv.View.Refresh();
                }
                IntentionQuoteItem row = this.IntentionOrders.FirstOrDefault(a => a.SaveBtnVisible.Equals("0"));//有新增未入库的个需
                if (row == null)
                {
                    ConfirmEnable = true;
                }
            }));
        }

        private async Task RequeryAsync()
        {
            await Task.Run(() =>
            {
                QueryIntention();
                QueryAssociate();
            });
        }
        #endregion

        public void CacheBindQTY(IntentionQuoteItem row)
        {
            if (!string.IsNullOrEmpty(row.IntentionId) && !dicBindQtyEdited.ContainsKey(row.IntentionId))
                dicBindQtyEdited.Add(row.IntentionId, row);
        }


        public IntentionQuoteInfo GetSameIntentionInfo(string partymanagerid, string partyTraderid, string securityid, string side, string accountid)
        {
            if (IntentionOrders != null)
            {
                return IntentionQuoteList.FirstOrDefault(e => e.quotetype == "BUYER_INTENTION" && (e.status == "80" || e.status == "81" || e.status == "82") && e.securityId == securityid && e.side == side
                && ((string.IsNullOrEmpty(partymanagerid) && string.IsNullOrEmpty(e.partyManager)) || e.partyManager == partymanagerid)
                && ((string.IsNullOrEmpty(partyTraderid) && string.IsNullOrEmpty(e.partyTrader)) || e.partyTrader == partyTraderid)
                && ((string.IsNullOrEmpty(accountid) && string.IsNullOrEmpty(e.accountID)) || e.accountID == accountid));
            }
            return null;
        }

        private string INTENTION_GROUP = "INTENTION_GROUP";
        #region 删除或撤销
        private void OnDelCommandClick()
        {
            if (SelectedItem == null)
            {
                //Common.View.Component.CustomMessageBox.Info("请先选择操作记录", "批量REF");
                return;
            }
            if (!STC.Common.Utils.CommonUtils.IsInvestor())
            {
                if (!string.IsNullOrEmpty(SelectedItem.ID))
                {
                    Common.View.Component.CustomMessageBox.Info("请先解除匹配的成交再撤销。", "撤销");
                    return;
                }
            }
            lock (_lockObj)
            {
                IntentionQuoteItem item = this.SelectedItem;
                if (string.IsNullOrEmpty(item.IntentionId))
                {
                    IntentionOrders.Remove(item);
                }
                else
                {
                    STC.Common.Model.Table.WorkBrench.ConfirmIntentionQuoteRequest request = new STC.Common.Model.Table.WorkBrench.ConfirmIntentionQuoteRequest();
                    request.intentionId = item.IntentionId;
                    request.corssVenues = "2";
                    request.actionType = "82";// 闪电下单
                    request.userName = item.BrokerId;
                    request.closeBy = LocalEnv.UserInfo.UserName;
                    request.closeByName = LocalEnv.UserInfo.FullName;
                    request.clOrdId = DataTimeSequence.Instance.GetId();
                    request.accountID = item.AccountID;
                    request.accountName = item.AccountName;
                    request.ifSubBond = item.ifSubBond;
                    request.vipCode = item.vipCode;
                    request.orgType = item.orgType;
                    request.validUntilTime = GetValidUntilTime(item.Info1);
                    request.inf1 = item.Info1;
                    request.info2 = item.Info2;
                    request.channel = item.Channel;
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
                    STC.Common.Model.Table.WorkBrench.ConfirmIntentionQuoteReplyPo replyPo = IntentionOldHandler.Instance.ConfirmIntentionQuote(request);
                   
                    if (replyPo != null)
                    {
                        if ("0000".Equals(replyPo.Code))
                        {
                            IntentionOrders.Remove(item);
                        }
                        else
                        {
                            showMessage(string.Format("{0}失败{1}", "删除", replyPo != null ? replyPo.Msg : ""), false);
                        }
                    }
                }
                IntentionQuoteItem row = this.IntentionOrders.FirstOrDefault(a => a.SaveBtnVisible.Equals("0"));//有新增未入库的个需
                if (row == null)
                {
                    ConfirmEnable = true;
                }
            }
        }
    

        private string GetValidUntilTime(string info1)
        {
            OrderTimeModel otm = null;
            if (JsonSplit.IsJson(info1))
            {
                otm = Newtonsoft.Json.JsonConvert.DeserializeObject<OrderTimeModel>(info1);
            }
            string ValidUntilTime = string.Empty;
            if (otm != null && otm.ValidTimeType == "0|")
            {
                // 默认
                int hour = DateTimeEx.ServerTimeNow.Hour;
                int min = DateTimeEx.ServerTimeNow.Minute;
                if (hour > 0 && hour < 6)
                {
                    ValidUntilTime = DateTime.Parse(DateTimeEx.ServerTimeNow.ToString("yyyy-MM-dd") + " 06:00:00").ToString("yyyy-MM-dd HH:mm:ss");
                }
                else if (hour >= 6 && hour < 12)
                {
                    ValidUntilTime = DateTime.Parse(DateTimeEx.ServerTimeNow.ToString("yyyy-MM-dd") + " 12:00:00").ToString("yyyy-MM-dd HH:mm:ss");
                }
                else
                {
                    ValidUntilTime = DateTime.Parse(DateTimeEx.ServerTimeNow.ToString("yyyy-MM-dd") + " 16:30:00").ToString("yyyy-MM-dd HH:mm:ss");
                }
            }
            else if (otm != null && !string.IsNullOrEmpty(otm.ValidTimeType) && otm.ValidTimeType.StartsWith("1|"))
            {
                //时长
                int totalsecond = 0;
                string temp = otm.ValidTimeType.Replace("1|", string.Empty);
                string[] tempArr = temp.Split(CommonConstants.COLON_CHAR);
                if (tempArr != null && tempArr.Length == 2)
                {
                    string ValidMinute = tempArr[0];
                    string ValidSecond = tempArr[1];
                    if (!string.IsNullOrEmpty(ValidMinute))
                    {
                        totalsecond = int.Parse(ValidMinute) * 60;
                    }

                    if (!string.IsNullOrEmpty(ValidSecond))
                    {
                        totalsecond += int.Parse(ValidSecond);
                    }
                }


                if (totalsecond == 0)
                {
                    ValidUntilTime = DateTimeEx.ServerTimeNow.AddSeconds(30).ToString("yyyy-MM-dd HH:mm:ss");
                }
                else
                {
                    ValidUntilTime = DateTimeEx.ServerTimeNow.AddSeconds(totalsecond).ToString("yyyy-MM-dd HH:mm:ss");
                }
            }


            // 用户设置的到期时间,需要大于用户自定义的时间
            string selfUntilTime = ValidUntilTime;
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
                    if (!(otm != null && otm.ValidTimeType == "3|"))
                    {
                        ValidUntilTime = GetMinDate(selfUntilTime, validUntiTimeSetting, otm != null && otm.ValidTimeType == "0|");
                    }
                    else
                    {
                        //选择了无
                        if (DateTimeEx.ServerTimeNow.CompareTo(DateTime.Parse(validUntiTimeSetting)) <= 0)
                        {
                            ValidUntilTime = validUntiTimeSetting;
                        }
                        else
                        {
                            ValidUntilTime = string.Empty;
                        }
                    }
                }
                else
                {
                    // 默认，时长，时点
                    if (!(otm != null && otm.ValidTimeType == "3|"))
                    {
                        // 默认
                        if (otm != null && otm.ValidTimeType == "0|")
                        {
                            ValidUntilTime = GetMinDate(selfUntilTime, "", true);
                        }
                    }
                    else
                    {
                        ValidUntilTime = string.Empty;
                    }
                }
            }
            return ValidUntilTime;
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

        #endregion


        private void InitBond(string securityId)
        {
            STC.Common.Model.Base.BondsInfoPo bondsInfoPo = Common.Utils.CommonUtils.GetBondsInfoPoBySecurityID(securityId);

            if (bondsInfoPo != null)
            {
                // 含权
                if (!string.IsNullOrEmpty(bondsInfoPo.Option_Type) &&
                ((!bondsInfoPo.Option_Type.Equals("NON") && bondsInfoPo.Option_Date != null) ||
                (bondsInfoPo.Option_Type.Equals("ETS") && bondsInfoPo.Option_Date == null)))
                {

                    if (PriceTypeEmptyDic.Count == 2)
                    {
                        Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            PriceTypeEmptyDic.Add(new KeyValuePair<string, string>("7", "行权"));
                        }));
                        
                    }
                    this.PriceType = "7";
                }
                else
                {
                    this.PriceType = "3";
                    if (PriceTypeEmptyDic.Count == 3)
                    {
                        Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            PriceTypeEmptyDic.RemoveAt(2);
                        }));
                    }
                }
                if (bondsInfoPo.SECURITY_TYPE == "BNC")
                {
                    if (STC.Common.Utils.CommonUtils.RateDP == "1")
                    {
                        DeciamlLength = 2;
                        RangeValue = 0.01m;
                    }
                    else
                    {
                        DeciamlLength = 4;
                        RangeValue = 0.0025m;
                    }
                }
                else
                {
                    if (STC.Common.Utils.CommonUtils.CreditDP == "2")
                    {
                        DeciamlLength = 4;
                        RangeValue = 0.0025m;
                    }
                    else
                    {
                        DeciamlLength = 2;
                        RangeValue = 0.01m;
                    }
                }
            }
            else
            {
                DeciamlLength = 4;
                RangeValue = 0.0025m;
            }
        }

        public void Clear()
        {
            PriceTypeEmptyDic.Clear();
        }

    }
}
