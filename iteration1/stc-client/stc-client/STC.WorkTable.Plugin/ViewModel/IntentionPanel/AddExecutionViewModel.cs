using com.gateway.log;
using STC.Common.Api;
using STC.Common.Data;
using STC.Common.Data.DataModel;
using STC.Common.Handler.MarketExec;
using STC.Common.Handler.Table;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.Table;
using STC.Common.Model.TBook;
using STC.Common.Utils;
using STC.Common.ViewModel;
using STC.WorkTable.Plugin.Validation;
using STC.WorkTable.Plugin.ViewModel.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace STC.WorkTable.Plugin.ViewModel.IntentionPanel
{
    public class AddExecutionViewModel : ViewModelBase, IValidationErrorHandler
    {
        #region 当前意向信息
        private TradeRequestData _data;

        public TradeRequestData Data
        {
            get { return _data; }
            set {
                _data = value;
                this.OnPropertyChanged();
            }
        }

        private List<string> originExecIDList = new List<string>();

        private CollectionViewSource _IntentionExecViewSource;
        public CollectionViewSource IntentionExecViewSource
        {
            get { return _IntentionExecViewSource; }
            set {
                _IntentionExecViewSource = value;
                this.OnPropertyChanged();

                foreach (IntentionExecItem item in value.View)
                {
                    if (item.ExecId.StartsWith("t"))
                    {
                        IntentionExecItem newCustomItem = item.Clone();
                        newCustomItem.ExecId = item.Info.execid;
                        newCustomItem.RelationFlag = item.Info.relationFlag;
                        newCustomItem.CounterPartyName = item.Info.counterpartyname;
                        newCustomItem.CounterPartySubName = item.Info.counterpartysubname;
                        newCustomItem.Price = item.Info.price.Value;
                        newCustomItem.BookQty = item.Info.qty;
                        newCustomItem.TradeDate = item.Info.tradedate;
                        newCustomItem.SettlType = item.Info.settlType;
                        newCustomItem.Remark = item.Info.remark;

                        this.CustomExecs.Add(newCustomItem);
                    }

                    if (item.RelationFlag == "0" && !item.ExecId.StartsWith("t"))
                    {
                        try
                        {
                            IntentionExecItem findItem = this.STCExecs.First(x => x.ExecId == item.ExecId);
                            if (findItem != null)
                            {
                                if (findItem.IsChecked == false)
                                {
                                    findItem.IsChecked = true;
                                    SelectedRowCount++;
                                }
                            }
                        }
                        catch(Exception e)
                        {

                        }
                        
                    }
                }

                CheckExeQty();
            }
        }
        #endregion

        #region 属性
        private int _ErrorVisibility = 0;

        public int ErrorVisibility
        {
            get { return _ErrorVisibility; }
            set { 
                _ErrorVisibility = value;
                this.OnPropertyChanged();
            }
        }

        private bool _IsOkBtnEnable = true;

        public bool IsOkBtnEnable
        {
            get { return _IsOkBtnEnable; }
            set
            {
                _IsOkBtnEnable = value;
                this.OnPropertyChanged();
            }
        }

        private string _Error;

        public string Error
        {
            get { return _Error; }
            set {
                _Error = value;
                this.OnPropertyChanged();
            }
        }

        #endregion


        private object lockObj_Executions = new object();
        public object LockObj_Executions
        {
            get
            {
                return lockObj_Executions;
            }
        }

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

        private int _selectedRowCount = 0;

        public int SelectedRowCount
        {
            get
            {
                return _selectedRowCount;
            }
            set
            {
                _selectedRowCount = value;
            }
        }

        public void CtrlCheckBox(bool flag)
        {
            lock(LockObj_Executions)
            {
                foreach(IntentionExecItem item in this.STCExecs)
                {
                    item.IsChecked = flag;
                }
            }
            if (flag)
                SelectedRowCount = this.STCExecs.Count;
            else
                SelectedRowCount = 0;
        }

        public bool isCheckAll()
        {
            return SelectedRowCount == this.STCExecs.Count && this.STCExecs.Count != 0;
        }

        private CustomCollection<IntentionExecItem> _STCExecs = new CustomCollection<IntentionExecItem>();
        private CustomCollection<IntentionExecItem> STCExecs
        {
            get
            {
                return _STCExecs;
            }
        }

        private CollectionViewSource _STCExecsViewSource = new CollectionViewSource();
        public CollectionViewSource STCExecsViewSource
        {
            get { return _STCExecsViewSource; }
            set { _STCExecsViewSource = value; }
        }


        private IntentionExecItem _SelectedRow_Custom;

        public IntentionExecItem SelectedRow_Custom
        {
            get { return _SelectedRow_Custom; }
            set
            {
                _SelectedRow_Custom = value;
                this.OnPropertyChanged();
            }
        }

        private object lockObj_Custom = new object();
        public object LockObj_Custom
        {
            get
            {
                return lockObj_Custom;
            }
        }

        private CustomCollection<IntentionExecItem> _CustomExecs = new CustomCollection<IntentionExecItem>();
        private CustomCollection<IntentionExecItem> CustomExecs
        {
            get
            {
                return _CustomExecs;
            }
        }

        private CollectionViewSource _CustomExecsViewSource = new CollectionViewSource();
        public CollectionViewSource CustomExecsViewSource
        {
            get { return _CustomExecsViewSource; }
            set { _CustomExecsViewSource = value; }
        }


        public AddExecutionViewModel()
        {
            AddCustomCommand = new ComRelayCommand(AddCustomExec);
            DeleteCustomCommand = new ComRelayCommand(DeleteCustomExec);
            if (this.CancelBtnCommand == null)
                this.CancelBtnCommand = new ComRelayCommand<Window>(CloseWindow);

            if (this.ConfirmBtnCommand == null)
                this.ConfirmBtnCommand = new ComRelayCommand<Window>(Confirm);

            this.STCExecsViewSource.IsLiveFilteringRequested = true;
            this.STCExecsViewSource.IsLiveSortingRequested = true;
            this.STCExecsViewSource.Source = STCExecs;
            BindingOperations.EnableCollectionSynchronization(this.STCExecsViewSource.View, lockObj_Executions);

            this.CustomExecsViewSource.IsLiveFilteringRequested = true;
            this.CustomExecsViewSource.IsLiveSortingRequested = true;
            this.CustomExecsViewSource.Source = CustomExecs;
            BindingOperations.EnableCollectionSynchronization(this.CustomExecsViewSource.View, LockObj_Custom);
        }

        internal void init()
        {
            IntentionHandler.Instance.OnIntentionHistoryExecs += Instance_OnIntentionExecs;

            this.RequestTodayExes(this.Data.IntentionId);
        }

        private void Clear()
        {
            if (this.STCExecs != null)
                this.STCExecs.Clear();

            if (this.CustomExecs != null)
                this.CustomExecs.Clear();
        }

        private void RequestTodayExes(string intenionid)
        {
            System.Threading.ThreadPool.QueueUserWorkItem(o =>
            {
                try
                {
                    IntentionExecRequest request = new IntentionExecRequest();
                    request.intentionId = intenionid;
                    request.userName = LocalEnv.UserInfo.UserName;
                    request.date = DateTimeEx.ServerTime.ToString("yyyy-MM-dd");
                    int index = 0;
                    request.pageSize = 2000;

                    bool flag = true;
                    while (flag)
                    {
                        request.pageNum = index;
                        ///获取订单 
                        var reply = TableHandler.Instance.QueryIntentionExec(request);

                        if (reply == null)
                        {
                            flag = false;
                            break;
                        }

                        if (reply != null && reply.Datas != null)
                        {
                            foreach (IntentionExecInfo obj in reply.Datas)
                            {
                                Instance_OnIntentionExecs(obj);
                            }
                            if (reply.Datas.Count < request.pageSize)
                            {
                                flag = false;
                            }
                            else
                            {
                                index++;
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

        private void Instance_OnIntentionExecs(IntentionExecInfo obj)
        {
            if (!obj.execid.StartsWith("t")
                && obj.venuetypegw != null && !obj.venuetypegw.ToLower().Equals(ApiConsts.CDH.ToLower())
                                    && obj.relationFlag != "1"
                                    && obj.securityId == this.Data.SecurityId && obj.side == this.Data.Side)
            {
                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    UpdateExecutions(obj);
                }));
            }
        }

        private void UpdateExecutions(IntentionExecInfo obj)
        {
            try
            {
                lock (this.LockObj_Executions)
                {
                    long start = DateTime.Now.Ticks;
                    IntentionExecItem item = null;
                    if (this.STCExecs.Any(x => x.ExecId == obj.execid))
                        item = this.STCExecs.First(x => x.ExecId == obj.execid);
                    if (item != null)
                    {
                        if (obj.operation == "2")
                        {
                            this.STCExecs.Remove(item);
                        }
                    }
                    else
                    {
                        if (obj.operation == "2")
                        {
                            return;
                        }

                        item = new IntentionExecItem();
                        item.ExecId = obj.execid; // 成交编号
                        this.STCExecs.Add(item);
                    }

                    #region 判断是否已关联
                    var findList = this.IntentionExecViewSource.View.Cast<IntentionExecItem>().Where(x => x.ExecId == obj.execid);
                    if (findList.Count() > 0)
                    {
                        if (item.IsChecked == false)
                        {
                            item.IsChecked = true;
                            SelectedRowCount++;
                        }
                    }
                    #endregion
                    item.Side = obj.side; // 方向
                    item.SecurityId = obj.securityId; // 代码
                    item.Symbol = obj.symbol; // 简称，可能要从基本数据里取
                    item.Price = obj.price;//CommonUtils.ConvertToDecimal(obj.Yield); // 成交价；
                    //dr[NETPRICE_FIELD] = obj.netprice == null ? "--" : string.Format("{0:N4}", obj.netprice); // 净价
                                                                                                              // dr[NETPRICE1_FIELD] = CommonUtils.ConvertToDecimal(obj.NetPrice);
                    //string dirtyPrice = obj.dirtyprice == null ? "--" : string.Format("{0:00##}", obj.dirtyprice.Value);
                    //dr[DIRTYPRICE_FIELD] = dirtyPrice;// 全价
                    item.SettlType = obj.settlType; // 清算速度
                    //dr[REMAINQTY_FIELD] = obj.remainQty == null ? "--" : string.Format("{0:0}", obj.remainQty.Value); ;//剩余量
                    item.RemainQty = obj.remainQty;
                    item.BookQty = obj.qty; // 成交量
                    //dr[DELIVERYAMOUNT_FIELD] = (obj.dirtyprice == null || obj.qty == null) ? "--" : (obj.dirtyprice.Value * obj.qty.Value).ToString("0:00##");//结算金额,结算金额=全价 *成交量
                    item.CounterPartyName = obj.counterpartyname;//对手方
                    item.CounterPartySubName = obj.counterpartysubname;// 对手交易员
                    item.Venues = obj.venues;// 交易场所
                    //dr[PARTYSUBNAME_FIELD] = obj.partysubname;//本方交易员

                    if (!string.IsNullOrEmpty(obj.tradedate) && obj.tradedate.Length > 7)
                        item.TradeDate = obj.tradedate.Insert(4, "-").Insert(7, "-");
                    else
                        item.TradeDate = obj.tradedate;//obj.TransactTime;// 成交时间

                    item.TransactTime = obj.transacttime;// 成交时间

                    //dr[CREATE_DATE_FIELD] = obj.transacttime;// 成交时间
                    //dr[ORDERID_FIELD] = obj.orderid;//我的订单号 
                    //dr[REFORDERID_FIELD] = obj.CounterPartyOrdID;//对手订单号
                    //dr[SYNC_STATUS_FIELD] = obj.synchflag; // TODO: 同步状态
                    //dr[CLORDID_FIELD] = obj.execid; //clOrdID
                    //dr[STRATEGY_ORDER_ID_FIELD] = obj.strategyorderid;
                    item.TraderId = obj.traderId;
                    item.TraderName = obj.traderName;
                    item.InstitutionId = obj.institutionId;
                    item.InstitutionName = obj.institutionName;
                    //// 渠道显示
                    //string venuesSelectDisplay = "TradeBook";
                    //if (obj.VenuesSelectF == "2")
                    //{
                    //    if (string.IsNullOrEmpty(obj.VenuesRdmNum) == false)
                    //    {
                    //        venuesSelectDisplay = $"默认+{obj.VenuesRdmNum}家";
                    //    }
                    //    else
                    //    {
                    //        venuesSelectDisplay = "默认";
                    //    }
                    //}
                    //else if (obj.VenuesSelectF == "3")
                    //{
                    //    StringBuilder displaySB = new StringBuilder();
                    //    string[] arr = obj.SelfSelectVenues.Split(',');
                    //    foreach (string v in arr)
                    //    {
                    //        if (string.IsNullOrEmpty(v) == false)
                    //        {
                    //            string v_str = ComboSourceUtil.Instance.FindEnum(v.Substring(v.IndexOf('-') + 1), "venues");
                    //            displaySB.Append($"<{v_str}>");
                    //        }
                    //    }
                    //    venuesSelectDisplay = $"指定{displaySB}";
                    //}
                    //dr[VENUES_SELECT_DISPLAY_FIELD] = venuesSelectDisplay;
                    string remark = "";
                    if (obj.venuetypegw.Equals("INNER") && !obj.venues.Equals("MANUAL") && !obj.venues.Equals("IDB") && !obj.venues.Equals("QUANT"))
                    {
                        remark = "渠道执行";
                    }
                    item.Remark = remark;
                    //dr[QUANTID_FIELD] = obj.QuantID;
                    item.IntentionId = obj.intentionId;
                    item.Info = obj;
                    long end = DateTime.Now.Ticks;
                    Console.WriteLine("UpdateExecutions time :" + (end - start) / 10000000.0);
                }
            }
            catch (Exception e)
            {
                LogHelper.Error("UpdateExecutions Method", e);
            }
        }

        #region 自定义部分

        public ICommand AddCustomCommand { get; set; }

        public ICommand DeleteCustomCommand { get; set; }

        public ICommand CancelBtnCommand { get; set; }

        public ICommand ConfirmBtnCommand { get; set; }

        private bool _isValid_TotalExeQty = false;

        public bool IsValid_TotalExeQty
        {
            get { return _isValid_TotalExeQty; }
            set
            {
                _isValid_TotalExeQty = value;
                IsOkBtnEnable = string.IsNullOrEmpty(this.Error) && _isValid && _isValid_TotalExeQty;
            }
        }

        private bool _isValid = true;
        public bool IsValid
        {
            get { return _isValid; }
            set
            {
                _isValid = value;
                IsOkBtnEnable = string.IsNullOrEmpty(this.Error) && _isValid && _isValid_TotalExeQty;
            }
        }
        public Dictionary<UIElement, int> ErrorList { get; set; }

        private void AddCustomExec()
        {
            IntentionExecItem newItem = new IntentionExecItem();
            newItem.IntentionId = this.Data.IntentionId;
            newItem.RelationFlag = "0";
            newItem.InstitutionId = this.Data.InstitutionId;
            newItem.InstitutionName = this.Data.InstitutionName;
            newItem.TraderId = this.Data.TraderId;
            newItem.TraderName = this.Data.TraderName;
            newItem.UserName = LocalEnv.UserInfo.UserName;
            newItem.NikeName = LocalEnv.UserInfo.FullName;
            this.CustomExecs.Add(newItem);
        }

        private void DeleteCustomExec()
        {
            this.SelectedRow_Custom.CounterPartyName = "1";
            this.SelectedRow_Custom.CounterPartySubName = "1";
            this.SelectedRow_Custom.Price = 1;
            this.SelectedRow_Custom.SettlType = "1";
            this.SelectedRow_Custom.BookQty = 1;
            this.CustomExecs.Remove(this.SelectedRow_Custom);
        }

        private async void Confirm(Window win)
        {
            RelateIntentionReq request = new RelateIntentionReq();
            foreach (IntentionExecItem stcExec in this.STCExecs)
            {
                if (stcExec.IsChecked == false)
                {
                    continue;
                }
                RelateIntentionInfo item = new RelateIntentionInfo();
                item.intentionId = this.Data.IntentionId;
                item.relationFlag = "0";
                item.execid = stcExec.ExecId;
                item.orderstatus = "2";
                item.counterpartyname = stcExec.CounterPartyName;
                item.counterpartysubname = stcExec.CounterPartySubName;
                item.settltype = stcExec.SettlType;
                item.price = item.origordprice = stcExec.Price;
                item.qty = stcExec.BookQty;
                item.tradedate = stcExec.TradeDate;
                item.remark = stcExec.Remark;
                item.username = LocalEnv.UserInfo.UserName;
                item.venuetypegw = stcExec.Venues;
                item.venues = stcExec.Venues;
                item.operation = "0";
                if (request.execList == null)
                {
                    request.execList = new List<RelateIntentionInfo>();
                }
                request.execList.Add(item);
            }

            foreach (IntentionExecItem drv in this.CustomExecs)
            {
                RelateIntentionInfo item = new RelateIntentionInfo();
                item.intentionId = this.Data.IntentionId;
                item.relationFlag = "0";
                item.orderstatus = "2";
                item.counterpartyname = drv.CounterPartyName;
                item.counterpartysubname = drv.CounterPartySubName;
                item.settltype = drv.SettlType;
                item.price = item.origordprice = drv.Price;
                item.qty = drv.BookQty;
                item.tradedate = drv.TradeDate;
                item.remark = drv.Remark;
                item.username = LocalEnv.UserInfo.UserName;
                item.venuetypegw = "INNER";
                item.venues = "MANUAL";
                item.securityid = this.Data.SecurityId;
                item.symbol = this.Data.Symbol;
                item.side = this.Data.Side;
                item.institutionname = this.Data.InstitutionName;
                item.tradername = this.Data.TraderName;
                item.execid = drv.ExecId;
                item.operation = "0";
                if (request.execList == null)
                {
                    request.execList = new List<RelateIntentionInfo>();
                }
                request.execList.Add(item);
            }

            CheckDeleteFlag(request);

            PResult result = await IntentionHandler.Instance.RelateIntention(request);

            this.Error = result == null ? "意向关联失败" : result.Msg;

            if (result.IsSuccess())
            {
                ErrorVisibility = 2;
                IntentionHandler.Instance.OnIntentionHistoryExecs -= Instance_OnIntentionExecs;
                CloseWindow(win);
            }
            else
            {
                ErrorVisibility = 0;
            }
        }

        private void CheckDeleteFlag(RelateIntentionReq request)
        {
            foreach (IntentionExecItem item in IntentionExecViewSource.View)
            {
                if (item.ExecId.StartsWith("t")  
                    || (item.RelationFlag == "0" && !item.ExecId.StartsWith("t")))
                {
                    if (request.execList == null || !request.execList.Exists(x => x.execid == item.ExecId))
                    {
                        // 删除数据
                        RelateIntentionInfo deleteInfo = new RelateIntentionInfo();
                        deleteInfo.operation = "2";
                        deleteInfo.execid = item.ExecId;
                        deleteInfo.intentionId = item.IntentionId;
                        deleteInfo.username = LocalEnv.UserInfo.UserName;
                        if (request.execList == null)
                        {
                            request.execList = new List<RelateIntentionInfo>();
                        }
                        request.execList.Add(deleteInfo);
                    }
                    else
                    {
                        RelateIntentionInfo updateInfo = request.execList.Find(x => x.execid == item.ExecId);
                        updateInfo.operation = "1";
                    }
                }
            }    
        }

        private void CloseWindow(Window win)
        {
            if (win != null)
            {
                win.Close();
            }

            Clear();
        }
        #endregion

        internal void CheckExeQty()
        {
            decimal currentExeQty = 0;
            foreach (IntentionExecItem stcExec in this.STCExecs)
            {
                if (stcExec.IsChecked == false)
                {
                    continue;
                }
                if (stcExec.BookQty.HasValue)
                    currentExeQty += stcExec.BookQty.Value;
            }

            foreach (IntentionExecItem drv in this.CustomExecs)
            {
                if (drv.BookQty.HasValue)
                    currentExeQty += drv.BookQty.Value;
            }

            if (currentExeQty > this.Data.Qty)
            {
                this.Error = "成交量超出报量";
                this.ErrorVisibility = 0;
                IsValid_TotalExeQty = false;
            }
            else
            {
                this.Error = string.Empty;
                this.ErrorVisibility = 2;
                IsValid_TotalExeQty = true;
            }
        }
    }
}
