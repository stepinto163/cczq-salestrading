using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows;
using System.Threading;
using STC.XBond.Plugin.View;
using System.Windows.Input;
using STC.XBond.Plugin.Utils;
using STC.Common.Handler.TBook;
using STC.Common.Model.Fix.Message;
using STC.Common.Utils;
using STC.Common.Handler.Inner;
using STC.Common.Model.TBook;
using STC.Common.Handler.Base;
using STC.Common.Model.Base;
using STC.Common.Model.Inner;
using STC.Common.Handler.Admin;
using STC.XBond.Plugin.View.Component;
using STC.Common.Model.Admin;
using System.Collections.ObjectModel;
using STC.Common.Model;
using com.gateway.log;

namespace STC.XBond.Plugin.ViewModel
{
    /// <summary>
    /// 渠道执行界面VenuesExecutionView的数据内容
    /// </summary>
    public class VenuesExecutionViewModel : ViewModelBase
    {
        /// <summary>
        /// "处理"按钮权限
        /// </summary>
        private readonly string MODULEID = "2002";
        private readonly string TP_ACTIONID = "200201";//国利
        private readonly string GJ_ACTIONID = "200202";//国际
        private readonly string PINAN_ACTIONID = "200203";//平安
        private readonly string XT_ACTIONID = "200204";//信唐
        private readonly string ZC_ACTIONID = "200205";//中诚

        private UserControl _myOwn = null;

        #region 渠道执行 字段
        public const string ID = "Id";//DataTable key
        public const string ClORDID = "ClOrdID";//客户端生成单号
        public const string ORDER_ID = "OrderId";//单号
        public const string NO_FIELD = "No"; //序号
        public const string STATUS_FIELD = "Status";//状态
        public const string VENUES_FIELD = "Venues";//渠道
        public const string FIRMFLAG_FIELD = "FirmFlag";//类型
        public const string ISOCO_FIELD = "IsOCO";//是否是OCO
        public const string SIDE_FIELD = "Side";//方向
        public const string SECURITYID_FIELD = "SecurityID";//代码
        public const string SYMBOL_FIELD = "Symbol";//简称
        public const string PX_FIELD = "Px";//本方报价（%）
        public const string QTY_FIELD = "Qty";//报量（万元）
        public const string QTY__PARTY_FIELD = "LeavesQty";//剩余成交量（万元）
        public const string LASTQTY__PARTY_FIELD = "LastQty";//部分成交量（万元）
        public const string SETTLTYPE_FIELD = "SettlType";//清算速度
        public const string PARTYSUBID_FIELD = "PartySubID";// 本方交易员
        public const string STCCODE_FIELD = "STCCode";//对应订单号
        public const string QUOTETIME_FIELD = "QuoteTime";//报价时间
        public const string LASTOPERATOR_FIELD = "LastOperator";//最后操作人
        public const string OPERATIONTIME_FIELD = "OperationTime"; //操作时间
        public const string BUTTON_STATUS = "ButtonStatus";//操作按钮显示
        public const string BUTTON_STATUS_POINT = "PointButtonStatus";//点价单操作按钮显示
        public const string FAIL_REASON = "FailReason";//失败原因

        public const string DEAL_STATUS_SORT = "DealStatusSort";//待处理状态置顶排序, 0:待处理，1:其他状态

        public const string COUNTERPARTYNAME = "CounterPartyName";//对手方
        public const string COUNTERPARTYSUBNAME = "CounterPartySubName";//对手方交易员
        public const string LAST_PX = "LastPx";//成交价

        public const string REMAINING_TIME_FIELD = "RemainingTime";//倒计时时间
        public const string NONFIRM_TIME_FIELD = "NonfirmTime";
        public const string TIME_IN_FORCE = "TimeInForce";
        public const string REMIAN_TIME_VISIBILITY = "RemainTimeVisibility";
        public const string IOC_TIME_OUT = "IocTimeOut";
        public const string INFO3 = "Info3";
        public const string BOND_INFO = "BondInfo";
        public const string SIDE_BRUSH = "SideBrush";

        public const string ORDER_PREFIX = "OrderPrefix";
        public const string ORDER_SUFFIX = "OrderSuffix";

        public const string SETTLEDATE_FIELD = "SettlDate";

        #endregion

        private ObservableCollection<TDealer> _userDatas;
        public ObservableCollection<TDealer> UserDatas
        {
            get
            {
                if (_userDatas == null)
                {
                    _userDatas = new ObservableCollection<TDealer>();//AdminHandler.Instance.GetStcTbookUsers();

                    TDealer emptyUser = new TDealer();
                    emptyUser.Name = string.Empty;
                    _userDatas.Add(emptyUser);
                }


                return _userDatas;
            }
        }
        
        public void OnCheckChange()
        {
            IEnumerable<TDealer> bookExs = _userDatas.Where(b => b.IsCheck == true);

            StringBuilder builder = new StringBuilder();

            foreach (TDealer item in bookExs)
            {
                builder.Append(item.Name + ",");
            }

            string checkNames = builder == null ? string.Empty : builder.ToString();
            SelectedUser = checkNames.Length > 0 ? checkNames.Substring(0, checkNames.Length - 1) : checkNames;
        }

        private string _selectedUser;
        public string SelectedUser
        {
            get { return _selectedUser; }
            set
            {
                _selectedUser = value;
                this.OnPropertyChanged();
                FilterOrderData();

                if (string.IsNullOrEmpty(value))
                {
                    foreach (TDealer user in _userDatas)
                        user.IsCheck = false;
                }
                else
                {
                    string[] arr = value.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (TDealer user in _userDatas)
                    {
                        if (!arr.Contains(user.Name))
                            user.IsCheck = false;
                        else
                            user.IsCheck = true;
                    }
                }
            }
        }

        private decimal _totalOrder;
        public decimal TotalOrder { get { return _totalOrder; } set { _totalOrder = value; this.OnPropertyChanged(); } }

        private decimal _waitDealOrder;
        public decimal WaitDealOrder { get { return _waitDealOrder; } set { _waitDealOrder = value; this.OnPropertyChanged(); } }

        private decimal _dealingOrder;
        public decimal DealingOrder { get { return _dealingOrder; } set { _dealingOrder = value; this.OnPropertyChanged(); } }

        private decimal _waitCancelOrder;
        public decimal WaitCancelOrder { get { return _waitCancelOrder; } set { _waitCancelOrder = value; this.OnPropertyChanged(); } }

        // 搜索
        private string _searchText = string.Empty;
        public string SearchText
        {
            get
            {
                return _searchText;
            }

            set
            {
                if (_searchText != value)
                {
                    _searchText = value;
                    this.OnPropertyChanged();

                    FilterOrderData();

                }
            }
        }

        private object lockObject = new object();

        /// <summary>
        /// 渠道执行数据
        /// </summary>
        private DataTable _VenuesExecutionDT;

        public DataTable VenuesExecutionDT
        {
            get
            {
                lock (lockObject)
                {
                    if (_VenuesExecutionDT == null)
                    {
                        _VenuesExecutionDT = new DataTable();
                        _VenuesExecutionDT.Columns.Add(ID, typeof(string));
                        _VenuesExecutionDT.Columns.Add(ORDER_ID, typeof(string));
                        _VenuesExecutionDT.Columns.Add(NO_FIELD, typeof(int));
                        _VenuesExecutionDT.Columns.Add(STATUS_FIELD, typeof(string));
                        _VenuesExecutionDT.Columns.Add(VENUES_FIELD, typeof(string));
                        _VenuesExecutionDT.Columns.Add(FIRMFLAG_FIELD, typeof(string));
                        _VenuesExecutionDT.Columns.Add(ISOCO_FIELD, typeof(string));
                        _VenuesExecutionDT.Columns.Add(SIDE_FIELD, typeof(string));
                        _VenuesExecutionDT.Columns.Add(SECURITYID_FIELD, typeof(string));
                        _VenuesExecutionDT.Columns.Add(SYMBOL_FIELD, typeof(string));
                        _VenuesExecutionDT.Columns.Add(PX_FIELD, typeof(decimal)); 
                        _VenuesExecutionDT.Columns.Add(QTY_FIELD, typeof(long));
                        _VenuesExecutionDT.Columns.Add(QTY__PARTY_FIELD, typeof(long));
                        _VenuesExecutionDT.Columns.Add(LASTQTY__PARTY_FIELD, typeof(long));
                        _VenuesExecutionDT.Columns.Add(SETTLTYPE_FIELD, typeof(string));
                        _VenuesExecutionDT.Columns.Add(PARTYSUBID_FIELD, typeof(string));
                        _VenuesExecutionDT.Columns.Add(STCCODE_FIELD, typeof(string));
                        _VenuesExecutionDT.Columns.Add(QUOTETIME_FIELD, typeof(string));
                        _VenuesExecutionDT.Columns.Add(LASTOPERATOR_FIELD, typeof(string));
                        _VenuesExecutionDT.Columns.Add(OPERATIONTIME_FIELD, typeof(string));
                        _VenuesExecutionDT.Columns.Add(ClORDID, typeof(string)); 
                        _VenuesExecutionDT.Columns.Add(BUTTON_STATUS, typeof(string));
                        _VenuesExecutionDT.Columns.Add(BUTTON_STATUS_POINT, typeof(string));
                        _VenuesExecutionDT.Columns.Add(FAIL_REASON, typeof(string)); 
                        _VenuesExecutionDT.Columns.Add(COUNTERPARTYSUBNAME, typeof(string)); 
                        _VenuesExecutionDT.Columns.Add(COUNTERPARTYNAME, typeof(string));
                        _VenuesExecutionDT.Columns.Add(DEAL_STATUS_SORT, typeof(string)); 
                        _VenuesExecutionDT.Columns.Add(LAST_PX, typeof(string));
                        _VenuesExecutionDT.Columns.Add(NONFIRM_TIME_FIELD, typeof(string));
                        _VenuesExecutionDT.Columns.Add(REMAINING_TIME_FIELD, typeof(string));

                        _VenuesExecutionDT.Columns.Add(TIME_IN_FORCE, typeof(string));
                        _VenuesExecutionDT.Columns.Add(REMIAN_TIME_VISIBILITY, typeof(string)); 
                        _VenuesExecutionDT.Columns.Add(IOC_TIME_OUT, typeof(string)); 
                        _VenuesExecutionDT.Columns.Add(INFO3, typeof(string));
                        _VenuesExecutionDT.Columns.Add(BOND_INFO, typeof(string));

                        _VenuesExecutionDT.Columns.Add(ORDER_PREFIX, typeof(string)); 
                        _VenuesExecutionDT.Columns.Add(ORDER_SUFFIX, typeof(string));
                        _VenuesExecutionDT.Columns.Add(SETTLEDATE_FIELD, typeof(string));

                        _VenuesExecutionDT.Columns.Add(SIDE_BRUSH, typeof(System.Windows.Media.SolidColorBrush));

                        _VenuesExecutionDT.PrimaryKey = new DataColumn[] { _VenuesExecutionDT.Columns[ID] };
                    }

                }
                return _VenuesExecutionDT;
            }
        }

        private DataView _VenuesExecutionDV;

        public DataView VenuesExecutionDV
        {
            get
            {
                lock (lockObject)
                {
                    if (_VenuesExecutionDV == null)
                    {
                        _VenuesExecutionDV = new DataView(this.VenuesExecutionDT);
                        _VenuesExecutionDV.Sort = string.Format("{0}", $"{NO_FIELD} ASC");
                        //_VenuesExecutionDV.Sort = string.Format("{0}, {1}", $"{DEAL_STATUS_SORT} ASC", $"{QUOTETIME_FIELD} ASC"); //$"{QUOTETIME_FIELD}";
                        this.OnPropertyChanged();
                    }
                    return _VenuesExecutionDV;
                }
            }
        }

        public VenuesExecutionViewModel()
        {
            // 我的订单
            BindingOperations.EnableCollectionSynchronization(VenuesExecutionDV, lockObject);
            //GenterateData();
        }
		
		~VenuesExecutionViewModel()
        {
            if (Application.Current != null)
            {
                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    this.Clear();
                }));
            }
        }


        StringBuilder filter_VenuesExe = new StringBuilder();
        /// <summary>
        /// 处理过滤条件
        /// </summary>
        /// <param name="filterDic"></param>
        public void ProcessMyOrdersFilterCondition(Dictionary<string, IList> filterDic)
        {
            filter_VenuesExe.Remove(0, filter_VenuesExe.Length);
            // Venues
            IList venuesList = filterDic["Venues"];
            StringBuilder inFilterSB_venues = new StringBuilder();
            if (venuesList.Count > 0)
            {
                foreach (ListBoxItem item in venuesList)
                {
                    if (item.Content.ToString() == CommonConstants.AllBroker)
                    {
                        continue;
                    }

                    if (inFilterSB_venues.Length > 0)
                    {
                        inFilterSB_venues.Append(" or ");
                    }

                    if (item.Content.ToString() == CommonConstants.CCTB)
                    {
                        inFilterSB_venues.Append($"{VenuesExecutionViewModel.VENUES_FIELD} = 'CCTB'");
                    }
                    else if (item.Content.ToString() == CommonConstants.CNEX)
                    {
                        inFilterSB_venues.Append($"{VenuesExecutionViewModel.VENUES_FIELD} = 'CNEX'");
                    }
                    else if (item.Content.ToString() == CommonConstants.CTTJ)
                    {
                        inFilterSB_venues.Append($"{VenuesExecutionViewModel.VENUES_FIELD} = 'CTTJ'");
                    }
                    else if (item.Content.ToString() == CommonConstants.TPSC)
                    {
                        inFilterSB_venues.Append($"{VenuesExecutionViewModel.VENUES_FIELD} = 'TPSC'");
                    }
                    else if (item.Content.ToString() == CommonConstants.PTCN)
                    {
                        inFilterSB_venues.Append($"{VenuesExecutionViewModel.VENUES_FIELD} = 'PTCN'");
                    }
                    else if (item.Content.ToString() == CommonConstants.UEDA)
                    {
                        inFilterSB_venues.Append($"{VenuesExecutionViewModel.VENUES_FIELD} = 'UEDA'");
                    }
                }
            }

            if (inFilterSB_venues.Length > 0)
            {
                filter_VenuesExe.Append($"({inFilterSB_venues})");
            }

            // Status
            IList statusList = filterDic["Status"];
            StringBuilder inFilterSB_Status = new StringBuilder();
            if (statusList.Count > 0)
            {
                foreach (ListBoxItem item in statusList)
                {
                    if (item.Content.ToString() != CommonConstants.ALL)
                    {
                        if (inFilterSB_Status.Length > 0)
                        {
                            inFilterSB_Status.Append(" or ");
                        }

                        inFilterSB_Status.Append($"{VenuesExecutionViewModel.STATUS_FIELD} = '{item.Content.ToString()}'");
                    }
                }
            }

            if (inFilterSB_Status.Length > 0)
            {
                if (filter_VenuesExe.Length > 0)
                {
                    filter_VenuesExe.Append(" and ");
                }

                filter_VenuesExe.Append($"({inFilterSB_Status})");
            }

            //IsOCO
            if (filterDic.ContainsKey("IsOCO"))
            {
                IList isOCOList = filterDic["IsOCO"];
                StringBuilder inFilterSB_isOCO = new StringBuilder();
                if (isOCOList.Count > 0)
                {
                    foreach (ListBoxItem item in isOCOList)
                    {
                        if (item.Content.ToString() != CommonConstants.ALL)
                        {

                            if (inFilterSB_isOCO.Length > 0)
                            {
                                inFilterSB_isOCO.Append(" or ");
                            }

                            if (item.Content.ToString() == Constants.ISOCO)
                            {
                                inFilterSB_isOCO.Append($"{VenuesExecutionViewModel.ISOCO_FIELD} = '1'");
                            }
                            else if (item.Content.ToString() == Constants.NOTOCO)
                            {
                                inFilterSB_isOCO.Append($"{VenuesExecutionViewModel.ISOCO_FIELD} = '0'");
                            }
                        }
                    }
                }

                if (inFilterSB_isOCO.Length > 0)
                {
                    if (filter_VenuesExe.Length > 0)
                    {
                        filter_VenuesExe.Append(" and ");
                    }

                    filter_VenuesExe.Append($"({inFilterSB_isOCO})");
                }
            }


            // MyOperation
            IList myOperationList = filterDic["MyOperation"];
            StringBuilder inFilterSB_MyOperation = new StringBuilder();
            if (myOperationList.Count > 0)
            {
                foreach (ListBoxItem item in myOperationList)
                {
                    if (item.Content.ToString() != CommonConstants.ALL)
                    {
                        if (inFilterSB_MyOperation.Length > 0)
                        {
                            inFilterSB_MyOperation.Append(" or ");
                        }

                        if (item.Content.ToString() == Constants.MYOPERATION)
                            inFilterSB_MyOperation.Append($"{VenuesExecutionViewModel.LASTOPERATOR_FIELD} = '{LocalEnv.UserInfo.FullName}'");
                        else if (item.Content.ToString() == Constants.NOOPERATION)
                            inFilterSB_MyOperation.Append($"{VenuesExecutionViewModel.LASTOPERATOR_FIELD} = '{string.Empty}'");
                    }
                    else
                    {
                        inFilterSB_MyOperation.Clear();
                        break;
                    }
                }
            }

            if (inFilterSB_MyOperation.Length > 0)
            {
                if (filter_VenuesExe.Length > 0)
                {
                    filter_VenuesExe.Append(" and ");
                }

                filter_VenuesExe.Append($"({inFilterSB_MyOperation})");
            }

            FilterOrderData();
        }

        private void ComputeOrderByStatus()
        {
            int totalOrderCount = 0;
            int waitDealOrderCount = 0;
            int dealOrderCount = 0;
            int waitCancelOrderCount = 0;
            foreach (DataRowView item in VenuesExecutionDV)
            {
                totalOrderCount++;
                string status = item[STATUS_FIELD] as string;
                if (status.Equals("待处理") || status.Equals("0"))
                    waitDealOrderCount++;
                else if (status.Equals("处理中") || status.Equals("3"))
                    dealOrderCount++;
                else if (status.Equals("待撤销") || status.Equals("6"))
                    waitCancelOrderCount++;
            }

            TotalOrder = totalOrderCount;
            WaitDealOrder = waitDealOrderCount;
            DealingOrder = dealOrderCount;
            WaitCancelOrder = waitCancelOrderCount;
        }

        /// <summary>
        /// 过滤数据
        /// </summary>
        private void FilterOrderData()
        {
            StringBuilder tempFilter = new StringBuilder();
            if (this.SearchText.Length > 0)
            {
                tempFilter.Append($"{VenuesExecutionViewModel.SECURITYID_FIELD} like '%{this.SearchText}%'");
            }

            if (filter_VenuesExe.Length > 0)
            {
                if (tempFilter.Length > 0)
                {
                    tempFilter.Append(" and ");
                }

                tempFilter.Append(filter_VenuesExe);
            }

            if (_selectedUser != null && !string.IsNullOrEmpty(_selectedUser))
            {
                if (tempFilter.Length > 0)
                {
                    tempFilter.Append(" and ");
                }

                string[] arrName = _selectedUser.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);

                tempFilter.Append(" (");
                StringBuilder sb = new StringBuilder();
                foreach (string str in arrName)
                {
                    sb.Append($"{PARTYSUBID_FIELD} = '{str}' or ");
                }
                string tem = sb.ToString();
                tempFilter.Append(tem.Substring(0, tem.Length - 3) + ")");
            }

            this.VenuesExecutionDV.RowFilter = tempFilter.ToString();


            ComputeOrderByStatus();
        }

        private CollectionViewSource filteredMarketExecutions = new CollectionViewSource();
        public CollectionViewSource FilteredMarketExecutions
        {
            get { return filteredMarketExecutions; }
            set { filteredMarketExecutions = value; }
        }


        private void GenterateData()
        {
            //System.Threading.ThreadPool.QueueUserWorkItem(o =>
            //{
            for (int i = 0; i < 4000; i++)
            {
                DataRow dr = this.VenuesExecutionDT.NewRow();
                dr[ID] = "ID:" + i;
                dr[NO_FIELD] = i;
                dr[STATUS_FIELD] = "已成交";
                dr[VENUES_FIELD] = "CNEX";
                dr[FIRMFLAG_FIELD] = "0";
                dr[ISOCO_FIELD] = "1";
                dr[SIDE_FIELD] = "1";
                dr[SECURITYID_FIELD] = "180024.IB";
                dr[SYMBOL_FIELD] = "18国债24";
                dr[PX_FIELD] = 3.1900;
                dr[QTY_FIELD] = 4000;
                dr[SETTLTYPE_FIELD] = "1";
                dr[PARTYSUBID_FIELD] = "yxl";
                dr[STCCODE_FIELD] = "STC040201";
                dr[LASTOPERATOR_FIELD] = "09:50:00";
                dr[QUOTETIME_FIELD] = "";
                dr[OPERATIONTIME_FIELD] = "";

                dr[DEAL_STATUS_SORT] = "1";

                this.VenuesExecutionDT.Rows.Add(dr);
            }



            //dr = this.VenuesExecutionDT.NewRow();
            //dr[NO_FIELD] = 2;
            //dr[STATUS_FIELD] = "已撤销";
            //dr[VENUES_FIELD] = "CNEX";
            //dr[FIRMFLAG_FIELD] = "0";
            //dr[ISOCO_FIELD] = "1";
            //dr[SIDE_FIELD] = "1";
            //dr[SECURITYID_FIELD] = "180024.IB";
            //dr[SYMBOL_FIELD] = "18国债24";
            //dr[PX_FIELD] = 3.1900;
            //dr[QTY_FIELD] = 4000;
            //dr[SETTLTYPE_FIELD] = "1";
            //dr[PARTYSUBID_FIELD] = "yxl";
            //dr[STCCODE_FIELD] = "STC040201";
            //dr[LASTOPERATOR_FIELD] = "09:50:00";
            //dr[QUOTETIME_FIELD] = "";
            //dr[OPERATIONTIME_FIELD] = "";
            //this.VenuesExecutionDT.Rows.Add(dr);
            //this.VenuesExecutionDT.AcceptChanges();
            //});
        }

        #region 自定义业务逻辑

        private object lockObj_MyExecutions = new object();
        public object LockObj_MyExecutions
        {
            get
            {
                return lockObj_MyExecutions;
            }
        }

        private AsyncThreadQueue<ExecutionReport> _orderTask = null;

        public void Init(UserControl owe)
        {
            _myOwn = owe;

            BaseHandler.Instance.Init(Instance_OnClear);

            if (_orderTask == null)
                _orderTask = new AsyncThreadQueue<ExecutionReport>(UpdateVenueOrder);
            else
                _orderTask.Clear();

            InitData();
        }

        #region 日末重置
        private void Instance_OnClear(ClearPo obj)
        {
            LogHandler.Info("VenuesExecutionViewModel Clear");
            if (obj != null && "Y".Equals(obj.IsClear))
            {
                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    this.Clear();
                    this.Init(_myOwn);
                }));
            }
        }

        public void Clear()
        {
            InnerHandler.Instance.UnSubscribeOrder(OnOrder);

            lock (this.LockObj_MyExecutions)
            {
                if (_VenuesExecutionDT != null)
                {
                    _VenuesExecutionDT.Clear();
                }
            }

            if (_orderTask != null)
            {
                _orderTask.Dispose();
                _orderTask = null;
            }

            if (UserDatas != null)
            {
                List<TDealer> removeList = new List<TDealer>();
                foreach (TDealer dealer in UserDatas)
                {
                    if (!string.IsNullOrEmpty(dealer.Name))
                    {
                        removeList.Add(dealer);
                    }
                }

                if (removeList.Count > 0)
                {
                    foreach (TDealer dealer in removeList)
                        UserDatas.Remove(dealer);
                }
            }

            SelectedUser = String.Empty;

        }
        #endregion


        private void InitData()
        {
            ThreadPool.QueueUserWorkItem(o =>
            {
                try
                {
                    QueryOrderReq request = new QueryOrderReq();
                    string today = DateTimeEx.ServerTime.ToString("yyyy-MM-dd");
                    request.dealTimeBegin = today;
                    request.dealTimeEnd = DateTimeEx.ServerTime.AddDays(1).ToString("yyyy-MM-dd"); ;
                    //request.userName = LocalEnv.UserInfo.UserName;
                    //N:已申请,0:处理中，1,2:已确认(其中1为部分确认,2完全确认)，4:已撤销，8:失败
                    //E(Broker B确认),F(Broker B撤销),G（Broker A确认），H（Broker A撤销）
                    //request.orderStatus = "N,0,1";
                    int index = 0;
                    request.pageSize = 2000;

                    bool flag = true;
                    while (flag)
                    {
                        request.pageNum = index;
                        ///获取订单 
                        var reply = InnerHandler.Instance.QueryInnerOrderForInner(request);

                        if (reply == null)
                        {
                            flag = false;
                            break;
                        }

                        if (reply != null)
                        {
                            InitTableData(reply);

                            if (reply.Count < request.pageSize)
                            {
                                flag = false;
                            }
                            else
                            {
                                index++;
                            }
                        }
                        else
                            break;
                    }
                }
                catch (Exception e)
                {
                    LogHelper.Error("RequestOrders", e);
                }
                finally
                {
                    InnerHandler.Instance.Init(OnOrder);
                }
            });
        }

        private void OnOrder(ExecutionReport order)
        {
            _orderTask.Enqueue(order);
        }

        private void InitTableData(List<ExecutionReport> orders)
        {
            foreach (ExecutionReport order in orders)
            {
                DataRow myExecution = this.VenuesExecutionDT.NewRow();

                myExecution[ID] = GetKey(order);
                myExecution[ClORDID] = order.ClOrdID;
                myExecution[NO_FIELD] = order.AllocID;
                myExecution[ORDER_ID] = order.OrderID; // 成交编号
                                                       //myExecution[NO_FIELD] = orde; // 序号

                myExecution[STATUS_FIELD] = CommonMapUtil.FindEnum(order.ExecType, "venueOrdStatus"); // 订单状态
                myExecution[VENUES_FIELD] = order.Venues; // 渠道
                myExecution[FIRMFLAG_FIELD] = order.FirmFlag; // 类型:firm/nonfirm
                                                              //myExecution[ISOCO_FIELD] = order.oco
                myExecution[SIDE_FIELD] = order.Side;//方向
                myExecution[SECURITYID_FIELD] = order.SecurityID;//代码
                myExecution[SYMBOL_FIELD] = order.Symbol; // 简称
                myExecution[PX_FIELD] = CommonUtils.ConvertToDecimal(order.Price); // 本方报价
                myExecution[QTY_FIELD] = CommonUtils.ConvertToDecimal(order.OrderQty); //报量（万元）
                myExecution[QTY__PARTY_FIELD] = CommonUtils.ConvertToDecimal(order.LeavesQty); //剩余成交量（万元）
                myExecution[LASTQTY__PARTY_FIELD] = CommonUtils.ConvertToDecimal(order.LastQty); //部分成交量（万元）
                myExecution[SETTLTYPE_FIELD] = order.SettlType;//结算方式
                myExecution[PARTYSUBID_FIELD] = order.PartySubName;//本方交易员
                myExecution[STCCODE_FIELD] = order.OrderID;// 对应订单号
                myExecution[QUOTETIME_FIELD] = CommonUtils.ConvertTimeFormat(order.TransactTime);//报价时间

                myExecution[LASTOPERATOR_FIELD] = string.IsNullOrEmpty(order.UpdateUser)?string.Empty:order.UpdateUser;// 最后操作人
                myExecution[OPERATIONTIME_FIELD] = order.UpdateTime;//操作时间

                myExecution[FAIL_REASON] = order.RejectText;//拒绝原因

                myExecution[COUNTERPARTYNAME] = order.CounterPartyName;//对手方
                myExecution[COUNTERPARTYSUBNAME] = order.CounterPartySubName;//对手方交易员
                myExecution[LAST_PX] = order.LastPx;//成交价

                string firmToNonTime = order.FirmToNonTime == null ? string.Empty : order.FirmToNonTime;
                myExecution[NONFIRM_TIME_FIELD] = firmToNonTime; // nofirmtime

                myExecution[TIME_IN_FORCE] = order.TimeInForce.Equals("0")?"报价":"点价"; // TimeInForce

                myExecution[DEAL_STATUS_SORT] = order.ExecType.Equals("0") || order.ExecType.Equals("6") ? "0" : "1";

                if ((string.IsNullOrEmpty(order.OCOFlag) || order.OCOFlag.Equals("0"))
                    && (!string.IsNullOrEmpty(order.FirmFlag) && order.FirmFlag.Equals("1")))
                    myExecution[ISOCO_FIELD] = "1";//是否OCO
                else
                    myExecution[ISOCO_FIELD] = "0";

                myExecution[REMIAN_TIME_VISIBILITY] = order.TimeInForce;//.Equals("3") ? Visibility.Visible : Visibility.Collapsed;

                myExecution[IOC_TIME_OUT] = order.IocTimeOut;//
                myExecution[INFO3] = order.Info3;

                myExecution[ORDER_PREFIX] = order.OrderID.Substring(0, order.OrderID.Length - 4);
                myExecution[ORDER_SUFFIX] = order.OrderID.Substring(order.OrderID.Length - 4, 4);

                myExecution[SETTLEDATE_FIELD] = order.SettlDate;
                AddOrUpdateDealer(order.PartySubName);

                this.VenuesExecutionDT.Rows.Add(myExecution);

                SetBtnVisibility(myExecution, order.ExecType, order.Venues, order.TimeInForce);
            }

            this.VenuesExecutionDT.AcceptChanges();

            ComputeOrderByStatus();
        }

        private void AddOrUpdateDealer(string name)
        {
            if (!string.IsNullOrEmpty(name))
            {
                if (Application.Current != null)
                {
                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        List<TDealer> selectUsers = UserDatas.Where(delegate (TDealer user)
                        {
                            return !string.IsNullOrEmpty(user.Name) && user.Name.ToUpper().Equals(name.ToUpper());
                        }).ToList();

                        if (selectUsers == null || selectUsers.Count <= 0)
                        {
                            TDealer newUser = new TDealer();
                            newUser.Name = name;


                            UserDatas.Add(newUser);
                        }
                    }));
                }
            }
        }

        private string GetKey(ExecutionReport order)
        {
            return string.Format("{0}{1}", order.OrderID, order.ClOrdID);
        }

        private void UpdateVenueOrder(ExecutionReport order)
        {
            try
            {
                lock (this.LockObj_MyExecutions)
                {
                    DataRow myExecution = this.VenuesExecutionDT.Rows.Find(GetKey(order));
                    if (myExecution != null)
                    {
                        if (CommonUtils.ConvertToString(myExecution[OPERATIONTIME_FIELD]).CompareTo(order.UpdateTime) > 0)
                        {
                            // 如果时间早， 就不更新了。
                            return;
                        }
                    }
                    else
                    {
                        myExecution = this.VenuesExecutionDT.NewRow();
                        myExecution[ORDER_ID] = order.OrderID; // 成交编号
                        myExecution[ClORDID] = order.ClOrdID;
                        myExecution[ID] = GetKey(order);
                        //myExecution[NO_FIELD] = orde; // 序号
                        this.VenuesExecutionDT.Rows.Add(myExecution);
                    }
                    myExecution[INFO3] = order.Info3;
                    myExecution[IOC_TIME_OUT] = order.IocTimeOut;//
                    myExecution[REMIAN_TIME_VISIBILITY] = order.TimeInForce;//.Equals("3") ? Visibility.Visible : Visibility.Collapsed;
                    myExecution[TIME_IN_FORCE] = order.TimeInForce.Equals("0") ? "报价" : "点价"; // TimeInForce
                    myExecution[NO_FIELD] = string.IsNullOrEmpty(order.AllocID)?string.Empty:order.AllocID;
                    myExecution[STATUS_FIELD] = CommonMapUtil.FindEnum(order.ExecType, "venueOrdStatus"); // 订单状态
                    myExecution[VENUES_FIELD] = order.Venues; // 渠道
                    myExecution[FIRMFLAG_FIELD] = order.FirmFlag; // 类型:firm/nonfirm
                    //myExecution[ISOCO_FIELD] = order.oco
                    myExecution[SIDE_FIELD] = order.Side;//方向
                    myExecution[SECURITYID_FIELD] = order.SecurityID;//代码
                    myExecution[SYMBOL_FIELD] = order.Symbol; // 简称
                    myExecution[PX_FIELD] = CommonUtils.ConvertToDecimal(order.Price); // 本方报价
                    myExecution[QTY_FIELD] = CommonUtils.ConvertToDecimal(order.OrderQty); //报量（万元）
                    myExecution[QTY__PARTY_FIELD] = CommonUtils.ConvertToDecimal(order.LeavesQty); //剩余成交量（万元）
                    myExecution[LASTQTY__PARTY_FIELD] = CommonUtils.ConvertToDecimal(order.LastQty); //部分成交量（万元）
                    myExecution[SETTLTYPE_FIELD] = order.SettlType;//结算方式
                    myExecution[PARTYSUBID_FIELD] = order.PartySubName;//本方交易员
                    myExecution[STCCODE_FIELD] = order.OrderID;// 对应订单号
                    myExecution[QUOTETIME_FIELD] = CommonUtils.ConvertTimeFormat(order.TransactTime);//报价时间

                    myExecution[LASTOPERATOR_FIELD] = string.IsNullOrEmpty(order.UpdateUser) ? string.Empty : order.UpdateUser;// 最后操作人
                    myExecution[OPERATIONTIME_FIELD] = order.UpdateTime;//操作时间

                    myExecution[FAIL_REASON] = order.RejectText;//拒绝原因

                    myExecution[COUNTERPARTYNAME] = order.CounterPartyName;//对手方
                    myExecution[COUNTERPARTYSUBNAME] = order.CounterPartySubName;//对手方交易员

                    myExecution[LAST_PX] = order.LastPx;//成交价

                    string firmToNonTime = order.FirmToNonTime == null ? string.Empty : order.FirmToNonTime;
                    myExecution[NONFIRM_TIME_FIELD] = firmToNonTime; // nofirmtime

                    if ((string.IsNullOrEmpty(order.OCOFlag) || order.OCOFlag.Equals("0"))
                    && (!string.IsNullOrEmpty(order.FirmFlag) && order.FirmFlag.Equals("1")))
                        myExecution[ISOCO_FIELD] = "1"; //是否OCO
                    else
                        myExecution[ISOCO_FIELD] = "0";

                    myExecution[DEAL_STATUS_SORT] = order.ExecType.Equals("0") || order.ExecType.Equals("6") ? "0" : "1";

                    SetBtnVisibility(myExecution, order.ExecType, order.Venues, order.TimeInForce);

                    myExecution[ORDER_PREFIX] = order.OrderID.Substring(0, order.OrderID.Length - 4);
                    myExecution[ORDER_SUFFIX] = order.OrderID.Substring(order.OrderID.Length - 4, 4);

                    myExecution[SETTLEDATE_FIELD] = order.SettlDate;

                    this.VenuesExecutionDT.AcceptChanges();

                    AddOrUpdateDealer(order.PartySubName);

                    ComputeOrderByStatus();
                }
            }
            catch (Exception e)
            {
                LogHelper.Error("UpdateVenueOrder Method Exception", e);
            }
        }

        private bool IsPointOrder(string timeInForce)
        {
            return !string.IsNullOrEmpty(timeInForce) && timeInForce.Equals("3");
        }

        private void SetBtnVisibility(DataRow myExecution, string status, string venues, string timeInForce)
        {
            string actionId = CommonMapUtil.FindEnum(venues, "dealpermission");

            bool isAllow = AdminHandler.Instance.CheckPermission(MODULEID, actionId);
            if (!isAllow)
            {
                myExecution[BUTTON_STATUS] = "00000";

                return;
            }

            switch (status)
            {
                case "0"://待处理

                    myExecution[BUTTON_STATUS] = "010000";
                    myExecution[BUTTON_STATUS_POINT] = IsPointOrder(timeInForce) ? "10" : "00";

                    break;

                case "3"://处理中

                    myExecution[BUTTON_STATUS] = "001100";

                    myExecution[BUTTON_STATUS_POINT] = "01";//IsPointOrder(timeInForce) ? "01" : "00";

                    break;

                case "2"://已成交

                    myExecution[BUTTON_STATUS] = "000010";

                    myExecution[BUTTON_STATUS_POINT] = "00";

                    break;

                case "7"://撤销中

                    myExecution[BUTTON_STATUS] = "000001";
                    myExecution[BUTTON_STATUS_POINT] = "00";

                    break;

                case "6"://待撤销

                    myExecution[BUTTON_STATUS] = "100000";
                    myExecution[BUTTON_STATUS_POINT] = "00";

                    break;

                case "4"://已撤销

                    myExecution[BUTTON_STATUS] = "000010";
                    myExecution[BUTTON_STATUS_POINT] = "00";

                    break;

                case "1"://部分成交

                    myExecution[BUTTON_STATUS] = "000010";
                    myExecution[BUTTON_STATUS_POINT] = "00";

                    break;

                case "8"://撤销失败

                    myExecution[BUTTON_STATUS] = "000010";
                    myExecution[BUTTON_STATUS_POINT] = "00";

                    break;

            }
        }

        #region 界面绑定字段
        private DataRowView _currentSelectItem;
        public DataRowView CurrentSelectItem
        {
            get { return _currentSelectItem; }
            set
            {
                _currentSelectItem = value;
                
            }
        }
        #endregion
        
        #region 命令事件

        public ICommand ExecuteCommand { get { return new RelayCommand(OnExecuteClick); } }
        public ICommand DealCommand { get { return new RelayCommand(OnDealClick); } }
        public ICommand LookCommand { get { return new RelayCommand(OnLookClick); } }
        public ICommand CancelCommand { get { return new RelayCommand(OnCancelClick); } }
        public ICommand IgnoreCommand { get { return new RelayCommand(IgnoreClick); } }
        public ICommand FailCommand { get { return new RelayCommand(FailClick); } }
        public ICommand RefuseCommand { get { return new RelayCommand(RefuseClick); } }

        private void RefuseClick()
        {
            InnerOrderReq orderReq = GetInnerOrder(_currentSelectItem, "8");//8：拒单
            orderReq.Rejecterreason = CommonMapUtil.FindEnum("8", "rejecterreason");
            ThreadPoolHelper.InvokeThreadPool(IgnoreOrder, orderReq);//失败
        }
        

        private void IgnoreClick()
        {
            ThreadPoolHelper.InvokeThreadPool(IgnoreOrder, GetInnerOrder(_currentSelectItem, "4"));//已撤销
        }

        private void FailClick()
        {
            InnerOrderReq orderReq = GetInnerOrder(_currentSelectItem, "4");
            orderReq.Rejecterreason = CommonMapUtil.FindEnum("7", "rejecterreason");
            ThreadPoolHelper.InvokeThreadPool(IgnoreOrder, orderReq);//失败
        }

        private void IgnoreOrder(object orderReq)
        {
            try
            {
                PResult result = InnerHandler.Instance.CancelOrder(orderReq as InnerOrderReq);

                DealReply(result);
            }
            catch (Exception e)
            {
                LogHelper.Error(e);
            }
        }

        private void OnExecuteClick()
        {
            if (_currentSelectItem != null)
            {
                VenuesExecutionEntryView view = new VenuesExecutionEntryView(DataRowToExecutionReport(_currentSelectItem), false, false);
                view.Show();
            }
        }
        
        private InnerOrderReq GetInnerOrder(DataRowView dr, string status)
        {
            InnerOrderReq orderReq = null;

            if (_currentSelectItem != null)
            {
                orderReq = new InnerOrderReq();
                orderReq.ClOrdID = dr[ClORDID] as string;
                orderReq.ExecType = status;//CommonMapUtil.FindEnum(dr[STATUS_FIELD] as string, "venuestatus");
                orderReq.OrderID = dr[ORDER_ID] as string;
                orderReq.Lastpartyname = LocalEnv.UserInfo.FullName;//dr[PARTYSUBID_FIELD] as string;
                
                orderReq.Rejecterreason = !string.IsNullOrEmpty(status) ? CommonMapUtil.FindEnum(status.Equals("4")?"5":status, "rejecterreason") : status;
            }

            return orderReq;
        }

        private void DealReply(PResult result)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                if (!result.IsSuccess())
                {
                    Common.View.Component.CustomMessageBox.Warning(result.Msg);
                }
            }));
        }

        private void OnDealClick()
        {
            if (_currentSelectItem != null)
            {
                string exeStatus = _currentSelectItem[STATUS_FIELD] as string;
                exeStatus = CommonMapUtil.FindEnum(exeStatus, "venuestatus");
                if (exeStatus.Equals("6"))//待撤销到撤销中
                    ThreadPoolHelper.InvokeThreadPool(CancelOrder, GetInnerOrder(_currentSelectItem, "6"));
                else//待处理到处理中
                    ThreadPoolHelper.InvokeThreadPool(DealOrder, GetInnerOrder(_currentSelectItem, "0"));

                if (_myOwn != null)
                {
                    CustomDataGrid dgData = _myOwn.FindName("dg_VenuesExecution") as CustomDataGrid;
                    if (dgData != null)
                    {
                        dgData.CopyOneData(dgData);
                    }
                }
            }
        }

        private void DealOrder(object orderReq)
        {
            try
            {
                PResult result = InnerHandler.Instance.TransactionOrder(orderReq as InnerOrderReq);

                DealReply(result);
            }
            catch (Exception e)
            {
                LogHelper.Error(e);
            }
        }

        private void CancelOrder(object orderReq)
        {
            try
            {
                PResult result = InnerHandler.Instance.CancelOrder(orderReq as InnerOrderReq);

                DealReply(result);
            }
            catch (Exception e)
            {
                LogHelper.Error(e);
            }
        }


        private void OnLookClick()
        {
            VenuesLookView lookView = new VenuesLookView(DataRowToExecutionReport(_currentSelectItem));
            lookView.Show();
        }

        

        private void OnCancelClick()
        {
            if (_currentSelectItem != null)
            {
                if (_myOwn != null)
                {
                    CustomDataGrid dgData = _myOwn.FindName("dg_VenuesExecution") as CustomDataGrid;
                    if (dgData != null)
                    {
                        dgData.CopyOneData(dgData, true);
                    }
                }

                VenuesExecutionEntryView view = new VenuesExecutionEntryView(DataRowToExecutionReport(_currentSelectItem), true, false);
                
                view.Show();
            }
        }

        #endregion

        #endregion

        private ExecutionReport DataRowToExecutionReport(DataRowView dr)
        {
            ExecutionReport result = new ExecutionReport();

            result.AllocID = dr[NO_FIELD] as string;
            result.OrderID = dr[ORDER_ID] as string; // 成交编号
                                                     //myExecution[NO_FIELD] = orde; // 序号
            result.ClOrdID = dr[ClORDID] as string;
            result.ExecType = dr[STATUS_FIELD] as string; // 订单状态
            //result.OrdStatus = dr[STATUS_FIELD] as string; // 订单状态
            result.Venues = dr[VENUES_FIELD] as string; // 渠道
            result.FirmFlag = dr[FIRMFLAG_FIELD] as string; // 类型:firm/nonfirm
                                                            //myExecution[ISOCO_FIELD] = order.oco
            result.Side = dr[SIDE_FIELD] as string;//方向
            result.SecurityID = dr[SECURITYID_FIELD] as string;//代码
            result.Symbol = dr[SYMBOL_FIELD] as string; // 简称
            
            if (dr[PX_FIELD] != null)
                result.Price = string.Format("{0:N4}", dr[PX_FIELD]); // 本方报价

            if (dr[QTY_FIELD] != null)
            {
                string orderStatus = CommonMapUtil.FindEnum(result.ExecType, "venuestatus");
                if (orderStatus.Equals("1") || orderStatus.Equals("7")) //1:部分成交, 7:待撤销
                {
                    result.OrderQty = string.Format("{0:N2}", (long)dr[LASTQTY__PARTY_FIELD] == 0 ? dr[QTY_FIELD] : dr[LASTQTY__PARTY_FIELD]); //部分成交量（万元）
                }
                else
                    result.OrderQty = string.Format("{0:N2}", dr[QTY_FIELD]); //报量（万元）
            }
            result.SettlType = dr[SETTLTYPE_FIELD] as string;//结算方式
            result.UserName = dr[PARTYSUBID_FIELD] as string;//本方交易员
            result.OrderID = dr[STCCODE_FIELD] as string;// 对应订单号
            result.TransactTime = CommonUtils.ConvertTimeFormat(dr[QUOTETIME_FIELD] as string);//报价时间

            result.UpdateUser = dr[LASTOPERATOR_FIELD] as string;// 最后操作人
            result.UpdateTime = dr[OPERATIONTIME_FIELD] as string;//操作时间
            result.RejectText = dr[FAIL_REASON] as string;//拒绝原因
            result.CounterPartySubName = dr[COUNTERPARTYSUBNAME] as string;//对手方交易员
            result.CounterPartyName = dr[COUNTERPARTYNAME] as string;//对手方

            result.LastPx = dr[LAST_PX] as string;//成交价

            result.SettlType = dr[SETTLTYPE_FIELD] as string;
            result.SettlDate = dr[SETTLEDATE_FIELD] as string;

            return result;
        }
    }

    public class TDealer : BaseViewModel
    {
        private string _name;
        public string Name { get { return _name; } set { _name = value; this.OnPropertyChanged(); } }

        private bool _isCheck;
        public bool IsCheck { get { return _isCheck; } set { _isCheck = value; this.OnPropertyChanged(); } }
    }
}
