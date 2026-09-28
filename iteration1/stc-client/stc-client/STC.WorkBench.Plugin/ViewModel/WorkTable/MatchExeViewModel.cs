using com.gateway.log;
using netapi.Api;
using Newtonsoft.Json;
using STC.Common.Data;
using STC.Common.Handler.Admin;
using STC.Common.Handler.Base;
using STC.Common.Handler.PT;
using STC.Common.Handler.Table;
using STC.Common.Model.Admin;
using STC.Common.Model.Base;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.PT;
using STC.Common.Model.Table;
using STC.Common.Model.Table.WorkBrench;
using STC.Common.Model.TBook;
using STC.Common.Utils;
using STC.Common.View.MyTrading;
using STC.Common.ViewModel;
using STC.Common.ViewModel.Order.InnerPanel;
using STC.WorkBench.Plugin.View.WorkTable;
using STC.WorkBench.Plugin.ViewModel.Data;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace STC.WorkBench.Plugin.ViewModel.WorkTable
{
    public class MatchExeViewModel : ViewModelBase
    {
        public Action<IntentionQuoteItem> FinishIntentionItemAction;

        private int PageSize = 2000;

        private IntentionQuoteItem _currentItem;

        public IntentionQuoteItem CurrentItem
        {
            get { return _currentItem; }
            set
            {
                _currentItem = value;
                this.OnPropertyChanged();

                if (!isLoad && this.CurrentItem != null)
                {
                    isLoad = true;
                    // 查询持仓组合
                    Init();
                }
            }
        }

        public const string _ModelId = "4019";
        public const string _INNERTypeModelId = "6001";
        public bool ManageYourselfViewAll = false;
        public string user = ApiManager.Instance.GetDefaultApi().UserName;
        public bool isSales = false;

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


        /// <summary>
        /// 我的订单资源锁
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
        private PtExecordersData _selectedItem;
        public PtExecordersData SelectedItem
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

        private ObservableCollection<PtExecordersData> _ptOrders = new ObservableCollection<PtExecordersData>();

        public ObservableCollection<PtExecordersData> PTOrders
        {
            get
            {
                if (_ptOrders == null)
                    _ptOrders = new ObservableCollection<PtExecordersData>();

                return _ptOrders;
            }

            set
            {
                _ptOrders = value;
                this.OnPropertyChanged(nameof(PTOrders));
                this.Mv.View.Refresh();
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

        private decimal _relatedQty;

        public decimal RelatedQty
        {
            get { return _relatedQty; }
            set
            {
                _relatedQty = value;
                this.OnPropertyChanged();
            }
        }

        private long _ExecQty;

        public long ExecQty
        {
            get { return _ExecQty; }
            set
            {
                _ExecQty = value;
                this.OnPropertyChanged();
            }
        }

        private List<string> execIdList = new List<string>();

        /// <summary>
        /// 成交匹配关系
        /// key : execId
        /// value : 所有相关联intentionid
        /// </summary>
        private ConcurrentDictionary<string, List<string>> exeAndIntentionsDic = new ConcurrentDictionary<string, List<string>>();
        /// <summary>
        /// 非本execid绑定的成交数量
        /// key ： execID
        /// value：非本意向匹配的成交量
        /// </summary>
        Dictionary<string, decimal> otherUsedQtyDic = new Dictionary<string, decimal>();
        /// <summary>
        /// 成交和个需关联关系数据
        /// key:execID
        /// value: TableIntentionExecution
        /// </summary>
        Dictionary<string, TableIntentionExecution> currentBindQtyDic = new Dictionary<string, TableIntentionExecution>();

        internal void load()
        {
            this.Mv.Source = PTOrders;
            //this.mv.View.CollectionChanged += View_CollectionChanged;
            this.Mv.IsLiveFilteringRequested = true;
            this.Mv.IsLiveSortingRequested = true;

            this.Mv.SortDescriptions.Add(new SortDescription("IsChecked_Edit", ListSortDirection.Descending));
            this.Mv.SortDescriptions.Add(new SortDescription("Createtime", ListSortDirection.Descending));

            SetMvFilter();

        }

        public MatchExeViewModel()
        {
            BaseHandler.Instance.SubscribeLoadDataStatus(onloadData);
            isInit = false;
           // setDefaultParam();
            isInit = true;
        }

        public bool isLoad = false;
        private void onloadData(bool obj)
        {
            if (obj)
            {
                if (!isLoad && this.CurrentItem != null)
                {
                    isLoad = true;
                    // 查询持仓组合
                    Init();
                }
            }
        }

        public void Init()
        {
            try
            {
                Task.Run(() =>
                {
                    List<ModulesAction> Action = AdminHandler.Instance.GetModulesActionById(_ModelId);
                    if (Action != null && Action.Count > 0)
                    {
                        foreach (ModulesAction m in Action)
                        {
                            if ("401902".Equals(m.id))//管理自己&查看全部
                            {
                                user = "*";
                                ManageYourselfViewAll = true;
                            }
                            else if ("401903".Equals(m.id))//管理全部
                            {
                                user = "*";
                            }
                            else
                            {//401901管理自己
                            }
                            break;
                        }
                        PTHandler.Instance.SubscribeExec(Instance_OnExec, user);
                    }
                    List<ModulesAction> ActionINNER = AdminHandler.Instance.GetModulesActionById(_INNERTypeModelId);
                    if (ActionINNER != null && ActionINNER.Count > 0)
                    {
                        foreach (ModulesAction m in ActionINNER)
                        {
                            if ("600102".Equals(m.id))//代客销售角色
                            {
                                isSales = true;
                            }
                            else
                            {
                                isSales = false;
                            }
                            break;
                        }
                    }
                    RefreshDatas();
                });
            }
            catch (Exception e)
            {
                LogHelper.Error("ExecuterOrderHandler init:", e);
            }
        }

        public bool isInit = false;
        public void ExecOrderQuery(PtExecOrderQueryReq req)
        {
            try
            {
                //execIdList.Clear();
                PtExecordersReplyPo reply = PTHandler.Instance.ExecOrderQuery(req);
                int maxCount = reply.totalSize;
                int pageNum = 0;
                if (maxCount > 0)
                {
                    pageNum = maxCount % PageSize == 0 ? (maxCount / PageSize) : (maxCount / PageSize + 1);
                }
                else
                    pageNum = 0;

                if (reply != null && reply.Datas != null)
                {
                    foreach (TbookPtExecorders eo in reply.Datas)
                    {
                        execIdList.Add(eo.Execid);
                    }

                    Application.Current.Dispatcher.Invoke((Action)(() =>
                    {
                        foreach (TbookPtExecorders eo in reply.Datas)
                        {
                            OnOrder(eo);
                        }
                    }));
                }

                for (int i = 1; i < pageNum; i++)
                {
                    req.pageNum = i;
                    reply = PTHandler.Instance.ExecOrderQuery(req);
                    if (reply != null && reply.Datas != null)
                    {
                        foreach (TbookPtExecorders eo in reply.Datas)
                        {
                            execIdList.Add(eo.Execid);
                        }

                        Application.Current.Dispatcher.Invoke((Action)(() =>
                        {
                            foreach (TbookPtExecorders eo in reply.Datas)
                            {
                                OnOrder(eo);
                            }
                        }));
                    }
                }
            }
            catch (Exception e)
            {
                LogHelper.Error("ExecOrderQuery error:", e);
            }
        }

        public PtExecOrderQueryReq request;
        private void Instance_OnExec(List<TbookPtExecorders> list)
        {

            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                foreach (TbookPtExecorders order in list)
                {
                    if (request != null)
                    {
                        if (!string.IsNullOrEmpty(request.bondType))
                        {
                            if ((request.bondType.Equals("bond") && !"4".Equals(order.Marketindicator)) || (request.bondType.Equals("irs") && !"2".Equals(order.Marketindicator)))
                            {
                                checkIsExistOrder(order);
                                return;
                            }
                        }
                        if (!string.IsNullOrEmpty(request.approvalstatus) && request.approvalstatus != order.Approvalstatus)
                        {
                            checkIsExistOrder(order);
                            return;
                        }
                        if (!string.IsNullOrEmpty(request.institutionid) && request.institutionid != order.Institutionid)
                        {
                            checkIsExistOrder(order);
                            return;
                        }
                        if (!string.IsNullOrEmpty(request.traderid) && request.securityid != order.Traderid)
                        {
                            checkIsExistOrder(order);
                            return;
                        }
                        if (!string.IsNullOrEmpty(request.venueexecid) && request.venueexecid != order.Venueexecid)
                        {
                            checkIsExistOrder(order);
                            return;
                        }
                        if (!request.send && "1".Equals(order.Sendstatus))
                        {
                            checkIsExistOrder(order);
                            return;
                        }

                        if (!string.IsNullOrEmpty(request.securityid) && request.securityid != order.Securityid)
                        {
                            checkIsExistOrder(order);
                            return;
                        }
                        if (!string.IsNullOrEmpty(request.settltype))
                        {
                            if ((request.settltype.Equals("F") && !order.Settltype.Contains("+")) || (!request.settltype.Equals("F") && !request.settltype.Equals(order.Settltype)))
                            {
                                checkIsExistOrder(order);
                                return;
                            }
                        }
                        if (!string.IsNullOrEmpty(request.userId) && LocalEnv.UserInfo.UserId != order.Userid)
                        {
                            checkIsExistOrder(order);
                            return;
                        }
                    }
                    //TotalCount++;
                    OnOrder(order);
                    //refreshTotalBySecurityID(order.Securityid);
                    this.Mv.View?.Refresh();
                }
            }));

        }


        private void checkIsExistOrder(TbookPtExecorders order)
        {
            PtExecordersData dataRow;
            lock (_lockObj)
            {
                dataRow = this.PTOrders.FirstOrDefault(o => o.ExecID == order.Execid);
                if (dataRow != null)
                {
                    this.PTOrders.Remove(dataRow);
                }
            }
        }


        public string IsCheckOwn(string Username, string entrytype)
        {
            //如果权限是：管理自己&查看全部，如果不是自己的策略，就不能操作
            string isOwn = "0";
            if ("0".Equals(entrytype))
            {
                isOwn = "1";
            }
            if (ManageYourselfViewAll && (Username == null || !Username.Equals(ApiManager.Instance.GetDefaultApi().UserName)))
            {
                isOwn = "1";
            }
            return isOwn;
        }

        public void OnOrder(TbookPtExecorders order)
        {
            try
            {



                PtExecordersData dataRow;
                lock (_lockObj)
                {
                    dataRow = this.PTOrders.FirstOrDefault(o => o.ExecID == order.Execid);
                }
                bool isold = false;
                if (dataRow != null)
                {
                    /*if ("1".Equals(order.Delflag))
                    {   //删除
                        lock (_lockObj)
                        {
                            PTOrders.Remove(dataRow);
                        }
                        return;
                    }*/
                    isold = true;
                }
                else
                {
                    dataRow = new PtExecordersData();
                    dataRow.ExecID = order.Execid;
                    dataRow.IsUpdated = false;
                }

                dataRow.ClOrdID = order.Clordid;
                dataRow.Matchstatus = order.Matchstatus;
                dataRow.Relatestatus = order.Relatestatus;
                dataRow.Sendstatus = order.Sendstatus;
                dataRow.SecurityID = order.Securityid;
                dataRow.Symboltype = "BNC".Equals(order.Symboltype) ? "利率债" : "信用债";
                dataRow.Side = order.Side;
                dataRow.LastPx = string.IsNullOrEmpty(order.Yield) ? "0" : order.Yield;
                dataRow.LastPx_Display = Common.Utils.CommonUtils.FormatPrice(order.Yield + "");// 用于显示的成交价
                dataRow.NetPrice = Common.Utils.CommonUtils.ConvertToString(order.Netprice);
                dataRow.LastQty = Common.Utils.CommonUtils.ConvertToString(order.Qty);
                dataRow.TradeDate = order.Tradedate;//order.Settldate;
                dataRow.ExecutionTime = order.Transacttime;
                dataRow.SettlType = string.IsNullOrEmpty(order.Settltype) ? "" : order.Settltype;
                dataRow.StrategyOrderId = order.Strategyorderid;
                dataRow.Venueexecid = order.Venueexecid;
                dataRow.Venueid = order.Venueid;
                dataRow.Symbol = order.Symbol;
                dataRow.Symboltype = order.Symboltype;
                dataRow.Tag1 = order.Tag1;
                dataRow.InstitutionName = order.Institutionname;
                dataRow.InstitutionID = order.Institutionid;
                dataRow.TraderName = order.Tradername;
                dataRow.TraderID = order.Traderid;
                dataRow.Venuetypegw = order.Venuetypegw;
                dataRow.Venues = order.Venues;
                dataRow.Quotemethod = order.Quotemethod;
                dataRow.CounterPartyId = order.Counterpartyid;
                dataRow.CounterPartyName = order.Counterpartyname;
                dataRow.CounterPartySubId = order.Counterpartysubid;
                dataRow.CounterPartySubName = order.Counterpartysubname;
                dataRow.PartySubName = order.Partysubname;
                dataRow.AccountId = order.Accountid;
                dataRow.AccountName = order.Accountname;
                dataRow.Remark = order.Remark;
                dataRow.UpdateTime = order.Updatetime;
                //dataRow.ShowDeleteBtn = "0".Equals(IsCheckOwn(order.Username, order.Entrytype)) ? "1".Equals(order.Delflag) ? "1" : "0" : "1";
                //dataRow.ShowSendBtn = "0".Equals("0".Equals(IsCheckOwn(order.Username, order.Entrytype)) ?
                //    (isCurrentBeforeDate(order.Tradedate, true) ?
                //    "1" : ("1".Equals(order.Sendstatus) ?
                //    ("3".Equals(order.Approvalstatus) ? "0" : "1") : "0")) : "1") ? "1".Equals(order.Delflag) ? "1" : "0" : "1";
                //dataRow.ShowSendBtn_FQ = "0".Equals("0".Equals(IsCheckOwn(order.Username, order.Entrytype)) ? ((dataRow.ShowSendBtn == "0" && LocalEnv.ConfInfo.IsFQ) ? "0" : "1") : "1") ? "1".Equals(order.Delflag) ? "1" : "0" : "1";
                //dataRow.ShowEditBtn = "0".Equals("0".Equals(IsCheckOwn(order.Username, order.Entrytype)) ? (isCurrentBeforeDate(order.Tradedate, false) ? "1" : ("1".Equals(order.Sendstatus) ? "1" : "0")) : "1") ? "1".Equals(order.Delflag) ? "1" : "0" : "1";
                dataRow.Seatnumber = order.Seatnumber;
                dataRow.Approvalstatus = order.Approvalstatus;
                dataRow.Approvalorderid = order.Approvalorderid;
                dataRow.Approvaltime = order.Approvaltime;
                dataRow.Approvalterminal = order.Approvalterminal;
                dataRow.IsMe = order.Userid == LocalEnv.UserInfo.UserId;
                dataRow.Username = order.Username;
                dataRow.Settldate = order.Settldate;
                if (!string.IsNullOrEmpty(order.Inf1))
                {
                    if (JsonSplit.IsJson(order.Inf1))
                    {
                        ParserInf inf1po = CommonUtils.DeserializeObject<ParserInf>(order.Inf1);
                        if (inf1po != null)
                        {
                            dataRow.Sign = inf1po.Sign;
                            dataRow.Sign1 = inf1po.Sign1;
                            dataRow.Sign2 = inf1po.Sign2;
                            dataRow.Sign3 = inf1po.Sign3;
                        }
                    }
                }
                dataRow.MaturityDays = order.MaturityDays;
                dataRow.IssuerCode = order.IssuerCode;
                dataRow.WarranterCode = order.WarranterCode;
                dataRow.DayOffStr = order.dayOffStr;
                dataRow.Blocknum = order.Blocknum;
                dataRow.Createtime = order.Createtime;
                dataRow.BusinessType = order.BusinessType;
                dataRow.Bookid = order.Bookid;
                dataRow.BookName = order.BookName;
                dataRow.MarketIndicator = order.Marketindicator;
                dataRow.Offsetflag = order.Offsetflag;
                dataRow.Yieldtoexecution = Common.Utils.CommonUtils.ConvertToString(order.Yieldtoexecution);
                dataRow.FullPrice = Common.Utils.CommonUtils.ConvertToString(order.Dirtyprice);
                dataRow.Realcounterpartyname = order.Realcounterpartyname;
                dataRow.TrdSubType = order.TrdSubType;
                dataRow.TrdType = order.TrdType;
                dataRow.SettlAmount = Common.Utils.CommonUtils.ConvertToString(order.SettlAmount);
                dataRow.BridgePaymentFixedRate = order.BridgePaymentFixedRate;
                dataRow.Yield2 = Common.Utils.CommonUtils.ConvertToString(order.Yield2);
                dataRow.CleanPrice2 = Common.Utils.CommonUtils.ConvertToString(order.CleanPrice2);
                dataRow.FullPrice2 = Common.Utils.CommonUtils.ConvertToString(order.FullPrice2);
                dataRow.TrdMatchId = order.TrdMatchId;
                dataRow.AllocText = order.AllocText;
                //if (string.IsNullOrEmpty(order.BookName) && !string.IsNullOrEmpty(order.Bookid) && PTExecOrderProcesser.Instance.BookAllDict.ContainsKey(order.Bookid))
                //{
                //    string bookName = "";
                //    if (PTExecOrderProcesser.Instance.BookAllDict.TryGetValue(order.Bookid, out bookName))
                //    {
                //        dataRow.BookName = bookName;
                //    }
                //}
                BondsInfoPo bondsInfoPo = Common.Utils.CommonUtils.GetBondsInfoPoBySecurityID(order.Securityid);
                if (bondsInfoPo != null)
                {
                    if (string.IsNullOrEmpty(bondsInfoPo.RemaingingTime) && "4".Equals(bondsInfoPo.MarketIndicator))
                    {
                        dataRow.RemainingTime = "已到期";
                    }
                    else
                    {
                        dataRow.RemainingTime = bondsInfoPo.RemaingingTime;
                    }
                }
                dataRow.Entrytype = "0".Equals(order.Entrytype) ? "接口" : "手工";
                dataRow.Delflag = order.Delflag;
               // dataRow.DateFilter = getDateFilter(dataRow);
                dataRow.Riskflag = order.Riskflag;
                dataRow.RiskRemark = order.Riskremark;
                dataRow.Partymanagername = order.Partymanagername;
                dataRow.Partymanagerid = order.Partymanagerid;
                dataRow.BindQty = 0;
                dataRow.BindQty_Edit = 0;
                if (!isold)
                {
                    lock (_lockObj)
                    {
                        dataRow.PropertyChanged += DataRow_PropertyChanged;
                        PTOrders.Insert(0, dataRow);
                    }
                }
                else
                {
                    dataRow.IsUpdated = true;
                }
                if (currentBindQtyDic.ContainsKey(dataRow.ExecID))
                {
                    TableIntentionExecution execData = currentBindQtyDic[dataRow.ExecID];
                    if (execData != null)
                    {
                        dataRow.ID = execData.id;
                        dataRow.BindQty = execData.quantity;
                        dataRow.BindQty_Edit = execData.quantity;
                        if (execData.quantity > 0)
                        {
                            dataRow.IsChecked = true;
                            dataRow.IsChecked_Edit = true;
                        }
                        //RelatedQty += execData.quantity;
                    }
                }
                if (otherUsedQtyDic.ContainsKey(dataRow.ExecID))
                {
                    dataRow.OtherUsedQty = otherUsedQtyDic[dataRow.ExecID];
                }
                else
                {
                    dataRow.OtherUsedQty = decimal.Zero;
                }
                //}
            }
            catch (Exception e)
            {
                LogHelper.Error("OnOrder", e);
            }
        }

        private void DataRow_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "BindQty_Edit" || e.PropertyName == "IsChecked")
            {
                PtExecordersData currentData = sender as PtExecordersData;
                if (currentData != null)
                {
                    RelatedQty = decimal.Zero;
                    foreach (PtExecordersData item in this.PTOrders)
                    {
                        if (item.IsChecked)
                        {
                            RelatedQty += item.BindQty_Edit.HasValue ? item.BindQty_Edit.Value : decimal.Zero;
                        }
                    }
                    AdjustAllChkStatus();
                }
            }
        }

        internal void RefreshDatas()
        {
            if (this.CurrentItem == null)
            {
                return;
            }

            Task.Run(() =>
            {
                this.execIdList.Clear();

                QueryAssociate();

                PtExecOrderQueryReq req = new PtExecOrderQueryReq();
                req.pageNum = 0;
                req.pageSize = PageSize;
                req.createTimeBegin = DateTimeEx.ServerTime.ToString("yyyy-MM-dd");
                req.createTimeEnd = DateTimeEx.ServerTime.ToString("yyyy-MM-dd");
                if (IsInvestor)
                {
                    req.partysubid = LocalEnv.UserInfo.UserId;
                }
                //req.userId = this.IsMe ? LocalEnv.UserInfo.UserId : "";
                if (!string.IsNullOrEmpty(user) && !"*".Equals(user))
                    req.userName = user;
                request = req;
                request.securityid = this.CurrentItem.SecurityId;
                request.side = this.CurrentItem.Side;
                request.tradedateBegin = DateTimeEx.ServerTime.ToString("yyyyMMdd");
                ExecOrderQuery(req);

                if(execIdList != null && execIdList.Count > 0)
                {
                    QueryAssociate(this.execIdList);
                    ExecOrderQuery(new PtExecOrderQueryReq() { execIdList = this.execIdList });
                }
            });
        }

        internal void QueryAssociate(List<string> execIdList = null)
        {
           
            exeAndIntentionsDic.Clear();
            currentBindQtyDic.Clear();
            otherUsedQtyDic.Clear();
            //RelatedQty = 0;
            IntentionExecutionAssociateReq req = new IntentionExecutionAssociateReq();
            req.action = "0";
            req.intentionIds = new List<string>();
            req.intentionIds.Add(this.CurrentItem.IntentionId);
            req.executionIds = execIdList;
            IntentionExecutionAssociateReplyPo reply = TableOldHandler.Instance.ProcessIntentionExecutionAssociate(req);

            if (reply != null && reply.IsSuccess() && reply.data != null && reply.data.Count > 0)
            {
                foreach (TableIntentionExecution d in reply.data)
                {
                    if (d.delflag == "1")
                    {
                        continue;
                    }
                    if (!this.execIdList.Contains(d.executionId))
                    {
                        this.execIdList.Add(d.executionId);
                    }
                    
                    if (!exeAndIntentionsDic.ContainsKey(d.executionId))
                    {
                        exeAndIntentionsDic[d.executionId] = new List<string>();
                    }
                    if (!exeAndIntentionsDic[d.executionId].Contains(d.intentionId))
                    {
                        exeAndIntentionsDic[d.executionId].Add(d.intentionId);
                    }

                    if (d.intentionId == this.CurrentItem.IntentionId)
                    {
                        currentBindQtyDic[d.executionId] = d;

                    }
                    else
                    {
                        if (!otherUsedQtyDic.ContainsKey(d.executionId))
                        {
                            otherUsedQtyDic[d.executionId] = d.quantity;
                        }
                        else
                        {
                            otherUsedQtyDic[d.executionId] += d.quantity;
                        }
                    }
                }
            }
        }

        private bool _CanSave = true;

        public bool CanSave
        {
            get { return _CanSave; }
            set
            {
                _CanSave = value;
                this.OnPropertyChanged();
            }
        }

        internal void SaveAssociate(Window win, bool isFinish = false)
        {
            CanSave = false;
            //if (!this.PTOrders.Any(x => x.IsChecked))
            //{
            //    CanSave = true;
            //    CommonUtils.Prompt2Second("提示", "请先勾选关联成交数据！", win);
            //    return;
            //}
            
            if (RelatedQty > this.CurrentItem.Qty)
            {
                CanSave = true;
                CommonUtils.Prompt2Second("提示", "匹配成交量大于报量！", win);
                return;
            }

            if (this.PTOrders.Any(x => x.IsChecked && x.OtherUsedQty + (x.BindQty_Edit.HasValue ? x.BindQty_Edit.Value : decimal.Zero) > x.LastQty.ConvertToDecimal()))
            {
                CanSave = true;
                CommonUtils.Prompt2Second("提示", "单笔匹配成交量大于报量！", win);
                return;
            }

            Task.Run(() =>
            {
                decimal newMatchQty = 0;
                IntentionExecutionAssociateReq req = new IntentionExecutionAssociateReq();
                req.action = "1";
                req.requestList = new List<TableIntentionExecution>();

                List<PtExecOrderSaveReq> reqList = new List<PtExecOrderSaveReq>();
                
                foreach (PtExecordersData row in this.PTOrders)
                {
                    // 关联关系
                    TableIntentionExecution subReq = new TableIntentionExecution();
                    subReq.id = row.ID;
                    subReq.intentionId = this.CurrentItem.IntentionId;
                    subReq.executionId = row.ExecID;
                    subReq.securityId = this.CurrentItem.SecurityId;
                    subReq.side = this.CurrentItem.Side;
                    subReq.quantity = row.BindQty_Edit.HasValue ? row.BindQty_Edit.Value : decimal.Zero;
                    subReq.partyTrader = this.CurrentItem.Trader_Buyer;
                    subReq.partyTraderName = this.CurrentItem.TraderName_Buyer;
                    subReq.partyManager = this.CurrentItem.Investor;
                    subReq.partyManagerName = this.CurrentItem.InvestorName;
                    subReq.bookId = this.CurrentItem.BookId;
                    subReq.bookName = this.CurrentItem.BookName;
                    subReq.accountID = this.CurrentItem.AccountID;
                    subReq.accountName = this.CurrentItem.AccountName;
                    subReq.intentionStatus = this.CurrentItem.Status;
                    subReq.executionStatus = row.Sendstatus;
                    string action = string.Empty;
                    if (row.IsChecked)
                    {
                        newMatchQty += subReq.quantity;
                        if (string.IsNullOrEmpty(row.ID))
                        {
                            action = "0";
                        }
                        else
                        {
                            action = "1";
                        }
                    }
                    else
                    {
                        newMatchQty += row.MatchQty.HasValue ? row.MatchQty.Value : decimal.Zero;
                        if (!string.IsNullOrEmpty(row.ID))
                        {
                            action = "2";
                        }
                        else
                        {
                            continue;
                        }
                    }
                    subReq.action = action;
                    req.requestList.Add(subReq);

                    //更新成交
                    PtExecOrderSaveReq ptUpdateReq = new PtExecOrderSaveReq();
                    ptUpdateReq.execid = row.ExecID;
                    decimal lastQty = row.LastQty.ConvertToDecimal();
                    decimal otherUsedQty = row.OtherUsedQty;
                    if (subReq.quantity + otherUsedQty == lastQty)
                    {
                        ptUpdateReq.matchstatus = "7";
                    }
                    else if (newMatchQty + otherUsedQty == 0)
                    {
                        ptUpdateReq.matchstatus = "5";
                    }
                    else if (subReq.quantity + otherUsedQty < lastQty)
                    {
                        ptUpdateReq.matchstatus = "6";
                    }
                    else
                    {
                        ptUpdateReq.matchstatus = "8";
                    }
                    ptUpdateReq.matchqty = Common.Utils.CommonUtils.ConvertToString(subReq.quantity + otherUsedQty);

                    if (exeAndIntentionsDic.ContainsKey(row.ExecID))
                    {
                        StringBuilder bookids = new StringBuilder();
                        StringBuilder bookNames = new StringBuilder();
                        StringBuilder partymanagerids = new StringBuilder();
                        StringBuilder partymanagernames = new StringBuilder();
                        StringBuilder accountids = new StringBuilder();
                        StringBuilder accountnames = new StringBuilder();
                        List<string> intentionidList = exeAndIntentionsDic[row.ExecID];
                        if (intentionidList != null && intentionidList.Count > 0)
                        {
                            foreach (string intentionId in intentionidList)
                            {
                                STC.Common.Model.Table.WorkBrench.IntentionQuoteInfo intentionInfo = IntentionOldHandler.Instance.GetIntentionByID(intentionId);
                                if (intentionInfo != null && !string.IsNullOrEmpty(intentionInfo.bookId) && !bookids.ToString().Contains($"{intentionInfo.bookId}{CommonConstants.BAR_CHAR}"))
                                {
                                    bookids.Append(intentionInfo.bookId);
                                    bookids.Append(CommonConstants.BAR_CHAR);
                                }
                                if (intentionInfo != null && !string.IsNullOrEmpty(intentionInfo.bookName) && !bookNames.ToString().Contains($"{intentionInfo.bookName}{CommonConstants.BAR_CHAR}"))
                                {
                                    bookNames.Append(intentionInfo.bookName);
                                    bookNames.Append(CommonConstants.BAR_CHAR);
                                }
                                if (intentionInfo != null && !string.IsNullOrEmpty(intentionInfo.accountID) && !accountids.ToString().Contains($"{intentionInfo.accountID}{CommonConstants.BAR_CHAR}"))
                                {
                                    accountids.Append(intentionInfo.accountID);
                                    accountids.Append(CommonConstants.BAR_CHAR);
                                }
                                if (intentionInfo != null && !string.IsNullOrEmpty(intentionInfo.accountName) && !accountnames.ToString().Contains($"{intentionInfo.accountName}{CommonConstants.BAR_CHAR}"))
                                {
                                    accountnames.Append(intentionInfo.accountName);
                                    accountnames.Append(CommonConstants.BAR_CHAR);
                                }
                                if (intentionInfo != null && !string.IsNullOrEmpty(intentionInfo.partyManager) && !partymanagerids.ToString().Contains($"{intentionInfo.partyManager}{CommonConstants.BAR_CHAR}"))
                                {
                                    partymanagerids.Append(intentionInfo.partyManager);
                                    partymanagerids.Append(CommonConstants.BAR_CHAR);
                                }
                                if (intentionInfo != null && !string.IsNullOrEmpty(intentionInfo.partyManagerName) && !partymanagernames.ToString().Contains($"{intentionInfo.partyManagerName}{CommonConstants.BAR_CHAR}"))
                                {
                                    partymanagernames.Append(intentionInfo.partyManagerName);
                                    partymanagernames.Append(CommonConstants.BAR_CHAR);
                                }
                            }
                        }
                        ptUpdateReq.bookid = bookids.ToString();
                        ptUpdateReq.bookname = bookNames.ToString(); 
                        ptUpdateReq.accountid = accountids.ToString();
                        ptUpdateReq.accountname = accountnames.ToString();
                        ptUpdateReq.partymanagerid = partymanagerids.ToString();
                        ptUpdateReq.partymanagername = partymanagernames.ToString();
                    }

                    reqList.Add(ptUpdateReq);
                }

                
                if (newMatchQty > this.CurrentItem.Qty)
                {
                    CanSave = true;
                    CommonUtils.Prompt2Second("提示", "匹配成交量大于报量！", win);
                    return;
                }

                IntentionExecutionAssociateReplyPo reply = TableOldHandler.Instance.ProcessIntentionExecutionAssociate(req);

                if (reply != null && reply.IsSuccess())
                {
                    
                    //PtExecUpdateReplyPo result = PTHandler.Instance.ExecOrderBatchUpdate(reqList);
                    //if (result == null || !result.IsSuccess())
                    //{
                    //    CanSave = true;
                    //    string errMsg = result == null ? "修改成交匹配状态失败" : result.Msg;
                    //    CommonUtils.Prompt2Second("提示", errMsg, win);
                    //    return;
                    //}

                    Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background, (Action)(() =>
                    {
                        CanSave = true;

                        if (isFinish)
                        {
                            FinishIntentionItemAction?.Invoke(this.CurrentItem);
                        }
                        
                        win.Close();
                    }));
                }
                else
                {
                    CanSave = true;
                    string errMsg = reply == null ? "保存失败" : reply.Msg;
                    CommonUtils.Prompt2Second("提示", errMsg, win);
                }
            });
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
                foreach (PtExecordersData item in this.PTOrders)
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
            var checkCount = this.PTOrders.AsParallel().Count(x => x.IsChecked);
            int workingDataCount = this.PTOrders.AsParallel().Count();
            this._hasSelected = checkCount > 0;
            IsAllChecked = checkCount == workingDataCount && workingDataCount != 0;
        }

        public void CheckBoxSingleClick()
        {
            bool flag = true;
            int i = 0;
            lock (_lockObj)
            {
                foreach (PtExecordersData item in this.PTOrders)
                {
                    flag = flag && item.IsChecked;
                    if (item.IsChecked)
                    {
                        i++;
                    }
                }
                _IsAllChecked = this.PTOrders.Count > 0 && flag;
            }
            this.OnPropertyChanged("IsAllChecked");
        }
        #endregion


        private void SetMvFilter()
        {
            this.Mv.View.Filter = x =>
            {
                PtExecordersData item = (PtExecordersData)x;

                if (_venuesFilterList != null && _venuesFilterList.Count > 0 && !_venuesFilterList.Contains(item.Venues))
                {
                    return false;
                }

                if (_matchStatusList != null && _matchStatusList.Count > 0 && !_matchStatusList.Contains(item.Matchstatus))
                {
                    return false;
                }

                return true;
            };
        }

        private List<string> _venuesFilterList = new List<string>();
        internal void ProcessVenuesFilter(string[] arr)
        {
            _venuesFilterList.Clear();
            if (arr != null)
            {
                _venuesFilterList.AddRange(arr);
            }
            this.mv?.View.Refresh();
        }

        private List<string> _matchStatusList = new List<string>();
        internal void ProcessMatchStatus(List<string> TagsList)
        {
            _matchStatusList.Clear();
            if (TagsList != null)
            {
                _matchStatusList.AddRange(TagsList);
            }
            this.mv?.View.Refresh();
        }
    }
}
