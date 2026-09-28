using Ats.Foundation.Message;
using com.gateway.log;
using STC.Common.Data;
using STC.Common.Data.DataModel;
using STC.Common.DataProcesser;
using STC.Common.Handler.Base;
using STC.Common.Handler.Cal;
using STC.Common.Handler.Table;
using STC.Common.Model.Base;
using STC.Common.Model.Cal;
using STC.Common.Model.Table;
using STC.Common.Model.TBook;
using STC.Common.Utils;
using STC.Common.View;
using STC.Common.ViewModel;
using STC.Common.ViewModel.Order;
using STC.WorkTable.Plugin.View;
using STC.WorkTable.Plugin.View.IntentionPanel;
using STC.WorkTable.Plugin.ViewModel.Data;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using WtUtils = STC.WorkTable.Plugin.Utils.CommonUtils;

namespace STC.WorkTable.Plugin.ViewModel.IntentionPanel
{
    /// <summary>
    /// 批量操作类型
    /// </summary>
    public enum BatchOperateTypeEnum
    {
        /// <summary>
        /// 无操作
        /// </summary>
        None,

        /// <summary>
        /// 转firm
        /// </summary>
        ToFirm,

        /// <summary>
        /// 转Nonfirm
        /// </summary>
        ToNonFirm,

        /// <summary>
        /// 编辑价格
        /// </summary>
        EditPrice,

        /// <summary>
        /// 编辑数量
        /// </summary>
        EditQty,

        /// <summary>
        /// 编辑渠道
        /// </summary>
        EditVenues,
    }


    public class IntentionPanelViewModel : ViewModelBase
    {
        private string _IntentionId;

        public string IntentionID
        {
            get
            {
                return _IntentionId;
            }
            set
            {
                _IntentionId = value;
                this.OnPropertyChanged();

                IntentionQuoteInfo cacheDataInfo = IntentionHandler.Instance.GetIntentionByID(value);

                UpateData(cacheDataInfo);

                this.SearchOrdersByIntentionID(cacheDataInfo.intentionId, cacheDataInfo.userName);

                this.SearchExecsByIntentionID(cacheDataInfo.intentionId, cacheDataInfo.userName);

                this.IntentionOrderViewSource.View.Filter = x =>
                {
                    IntentionOrderItem item = (IntentionOrderItem)x;
                    if (string.IsNullOrEmpty(this.IntentionID))
                        return false;
                    else
                        return this.IntentionID == item.IntentionId;
                };

                this.IntentionExecViewSource.View.Filter = x =>
                {
                    IntentionExecItem item = (IntentionExecItem)x;
                    if (string.IsNullOrEmpty(this.IntentionID))
                        return false;
                    else
                        return this.IntentionID == item.IntentionId;
                };

                if (_totalExecCountDic.ContainsKey(value))
                {
                    this.TotalExecCount = _totalExecCountDic[value];
                }
                else
                {
                    this.TotalExecCount = 0;
                }
               
                if (_totalExecQtyDic.ContainsKey(value))
                {
                    this.TotalExecQty = _totalExecQtyDic[value] / 1000;
                }
                else
                {
                    this.TotalExecQty = 0;
                }
            }
        }


        private TradeRequestData _data;

        public TradeRequestData Data
        {
            get { return _data; }
            set
            {
                _data = value;
                this.OnPropertyChanged();
            }
        }

        private string _baseInfo;

        public string BaseInfo
        {
            get
            {
                return _baseInfo;
            }
            set
            {
                _baseInfo = value;
                this.OnPropertyChanged();
            }
        }

        private string _BtnVisibility = "2";

        public string BtnVisibility
        {
            get
            {
                return _BtnVisibility;
            }
            set
            {
                _BtnVisibility = value;
                this.OnPropertyChanged();
            }
        }

        public ICommand DofirmOrderCommand { get; set; }

        public ICommand EditPriceCommand { get; set; }

        public ICommand EditQtyCommand { get; set; }

        public ICommand AddExecCommand { get; set; }

        private IntentionOrderItem _SelectIntentionOrder;

        public IntentionOrderItem SelectIntentionOrder
        {
            get 
            { 
                return _SelectIntentionOrder;
            }
            set 
            { 
                _SelectIntentionOrder = value;
                if (_SelectIntentionOrder != null && _SelectIntentionOrder.FirmFlag == "1")
                {
                    BtnVisibility = "0";
                }
                else
                {
                    BtnVisibility = "2";
                }
                this.OnPropertyChanged();
            }
        }

        private object _IntentionOrderLock = new object();
        private CustomCollection<IntentionOrderItem> _IntentionOrders = new CustomCollection<IntentionOrderItem>();
        public CustomCollection<IntentionOrderItem> IntentionOrders
        {
            get
            {
                return _IntentionOrders;
            }
        }

        private CollectionViewSource _IntentionOrderViewSource = new CollectionViewSource();
        public CollectionViewSource IntentionOrderViewSource
        {
            get { return _IntentionOrderViewSource; }
            set { _IntentionOrderViewSource = value; }
        }

        private ConcurrentDictionary<string, int> _totalExecCountDic = new ConcurrentDictionary<string, int>();
        private int _TotalExecCount = 0;
        public int TotalExecCount
        {
            get
            {
                return _TotalExecCount;
            }
            set
            {
                _TotalExecCount = value;
                this.OnPropertyChanged();
            }
        }

        private ConcurrentDictionary<string, int> _totalExecQtyDic = new ConcurrentDictionary<string, int>();
        private int _TotalExecQty = 0;
        public int TotalExecQty
        {
            get
            {
                return _TotalExecQty;
            }
            set
            {
                _TotalExecQty = value;
                this.OnPropertyChanged();
            }
        }

        private IntentionOrderItem _SelectIntentionExec;

        public IntentionOrderItem SelectIntentionExec
        {
            get
            {
                return _SelectIntentionExec;
            }
            set
            {
                _SelectIntentionExec = value;
                this.OnPropertyChanged();
            }
        }

        private object _IntentionExecLock = new object();
        private CustomCollection<IntentionExecItem> _IntentionExecs = new CustomCollection<IntentionExecItem>();
        private CustomCollection<IntentionExecItem> IntentionExecs
        {
            get
            {
                return _IntentionExecs;
            }
        }
        private CollectionViewSource _IntentionExecViewSource = new CollectionViewSource();
        public CollectionViewSource IntentionExecViewSource
        {
            get { return _IntentionExecViewSource; }
            set { _IntentionExecViewSource = value; }
        }

        public IntentionPanelViewModel()
        {
            DofirmOrderCommand = new ComRelayCommand<string>(DofirmOrder);
            EditPriceCommand = new ComRelayCommand<string>(EditPrice);
            EditQtyCommand = new ComRelayCommand<string>(EditQty);
            AddExecCommand = new ComRelayCommand(DoAddExecCommand);

            BaseHandler.Instance.SubscribeLoadDataStatus(onloadData);
            BaseHandler.Instance.Init(Instance_OnClear);
            IntentionHandler.Instance.OnIntentionQuotes += Instance_OnIntentionQuotes;
            IntentionHandler.Instance.OnIntentionOrders += Instance_OnIntentionOrders;
            IntentionHandler.Instance.OnIntentionExecs += Instance_OnIntentionExecs;

            this.IntentionOrderViewSource.IsLiveFilteringRequested = true;
            this.IntentionOrderViewSource.IsLiveSortingRequested = true;
            this.IntentionOrderViewSource.Source = IntentionOrders;
            this.IntentionOrderViewSource.View.Filter = x =>
            {
                IntentionOrderItem item = (IntentionOrderItem)x;
                if (string.IsNullOrEmpty(this.IntentionID))
                    return false;
                else
                    return this.IntentionID == item.IntentionId;
            };
            BindingOperations.EnableCollectionSynchronization(this.IntentionOrderViewSource.View, _IntentionOrderLock);

            this.IntentionExecViewSource.IsLiveFilteringRequested = true;
            this.IntentionExecViewSource.IsLiveSortingRequested = true;
            this.IntentionExecViewSource.Source = IntentionExecs;
            this.IntentionExecViewSource.View.Filter = x =>
            {
                IntentionExecItem item = (IntentionExecItem)x;
                if (string.IsNullOrEmpty(this.IntentionID))
                    return false;
                else
                    return this.IntentionID == item.IntentionId;
            };
            BindingOperations.EnableCollectionSynchronization(this.IntentionExecViewSource.View, _IntentionExecLock);
        }

        private ConcurrentDictionary<string, string> _SearchIntentionOrdersDic = new ConcurrentDictionary<string, string>();

        private ConcurrentDictionary<string, string> _SearchIntentionExecsDic = new ConcurrentDictionary<string, string>();

        private void SearchOrdersByIntentionID(string intentionID, string userName)
        {
            if (_SearchIntentionOrdersDic.ContainsKey(intentionID))
                return;

            _SearchIntentionOrdersDic[intentionID] = intentionID;
            System.Threading.ThreadPool.QueueUserWorkItem(o =>
            {
                try
                {
                    IntentionOrderRequest request = new IntentionOrderRequest();
                    request.intentionId = intentionID;
                    request.userName = userName;
                   
                    ///获取订单 
                    var reply = TableHandler.Instance.QueryIntentionOrder(request);


                    if (reply != null && reply.Datas != null)
                    {
                        foreach (IntentionOrderInfo obj in reply.Datas)
                        {
                            Instance_OnIntentionOrders(obj);
                        }
                    }
                }
                catch (Exception e)
                {
                    LogHelper.Error("RequestOrders", e);
                }
            });
        }

        private void SearchExecsByIntentionID(string intentionID, string userName)
        {
            if (_SearchIntentionExecsDic.ContainsKey(intentionID))
                return;

            _SearchIntentionExecsDic[intentionID] = intentionID;
            System.Threading.ThreadPool.QueueUserWorkItem(o =>
            {
                try
                {
                    IntentionExecRequest request = new IntentionExecRequest();
                    request.intentionId = intentionID;
                    request.userName = userName;
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

        private void DoAddExecCommand()
        {
            if (!WindowManager.Instance.IsExist(Utils.WindowKeyHelper.ADDEXECUTIONWIN))
            {
                AddExecutionWin win = new AddExecutionWin();
                if (win.DataContext != null && win.DataContext is AddExecutionViewModel)
                {
                    AddExecutionViewModel addExecVM = win.DataContext as AddExecutionViewModel;
                    addExecVM.Data = this.Data;
                    addExecVM.IntentionExecViewSource = this.IntentionExecViewSource;
                    addExecVM.init();
                }
                WindowManager.Instance.OpenOnlyOneWindow(Utils.WindowKeyHelper.ADDEXECUTIONWIN, win);
            }
        }

        private void Instance_OnIntentionExecs(IntentionExecInfo obj)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                UpdateIntentionExecs(obj);
            }));
        }

        private void UpdateIntentionExecs(IntentionExecInfo obj)
        {
            try
            {
                if (string.IsNullOrEmpty(obj.intentionId))
                    return;

                string execid = obj.execid;
                lock (_IntentionExecs)
                {
                    if (_IntentionExecs.Any(item => item.ExecId == execid && item.IntentionId == obj.intentionId))
                    {
                        IntentionExecItem findItem = _IntentionExecs.First(item => item.ExecId == execid && item.IntentionId == obj.intentionId);
                        //if (obj.updatetime != null && obj.updatetime.CompareTo(findItem.Info.updatetime) < 0)
                        //{
                        //    return;
                        //}

                        // 绑定删除
                        if(obj.operation == "2")
                        {
                            _IntentionExecs.Remove(findItem);
                        }

                        if(!string.IsNullOrEmpty(obj.intentionId))
                        {
                            if (!_totalExecQtyDic.ContainsKey(obj.intentionId))
                            {
                                _totalExecQtyDic[obj.intentionId] = 0;
                            }
                            _totalExecQtyDic[obj.intentionId] -= findItem.BookQty == null ? 0 : (int)findItem.BookQty.Value;
                        }

                        findItem.Price = obj.price;
                        findItem.BookQty = obj.qty;
                        findItem.TransactTime = obj.transacttime;
                        findItem.Venues = obj.venues;
                        findItem.PNL = obj.profitLoss;
                        findItem.Info = obj;
                        findItem.RelationFlag = obj.relationFlag;

                        if (!string.IsNullOrEmpty(obj.intentionId))
                        {
                            _totalExecQtyDic[obj.intentionId] += findItem.BookQty == null ? 0 : (int)findItem.BookQty.Value;
                        }
                    }
                    else
                    {
                        if (obj.operation == "2")
                        {
                            return;
                        }

                        IntentionExecItem newItem = new IntentionExecItem();
                        newItem.ExecId = obj.execid;
                        newItem.IntentionId = obj.intentionId;
                        newItem.SecurityId = obj.securityId;
                        newItem.Symbol = obj.symbol;
                        newItem.Side = obj.side;
                        newItem.Price = obj.price;
                        newItem.BookQty = obj.qty;
                        newItem.PNL = obj.profitLoss;
                        newItem.Venues = obj.venues;
                        newItem.TransactTime = obj.transacttime;
                        newItem.Info = obj;
                        newItem.RelationFlag = obj.relationFlag;
                        _IntentionExecs.Add(newItem);

                        if (!string.IsNullOrEmpty(obj.intentionId))
                        {
                            if (!_totalExecCountDic.ContainsKey(obj.intentionId))
                            {
                                _totalExecCountDic[obj.intentionId] = 0;
                            }
                            _totalExecCountDic[obj.intentionId]++;

                            if (!_totalExecQtyDic.ContainsKey(obj.intentionId))
                            {
                                _totalExecQtyDic[obj.intentionId] = 0;
                            }
                            _totalExecQtyDic[obj.intentionId] += newItem.BookQty == null ? 0 : (int)newItem.BookQty.Value;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                LogHelper.Error(e);
            }

        }

        private void DofirmOrder(string obj)
        {
            ConfirmIntentionQuoteRequest request = GetFirmOrderRequest();
            bool canComfirm = false;
            if (obj == "全部")
            {
                if (this.Data.Qty > 0)
                {
                    request.qty = this.Data.Qty;
                    canComfirm = true;
                }
            }
            else if (obj == "1000")
            {
                if (this.Data.LeftQty > 1000)
                {
                    request.qty = 1000;
                    canComfirm = true;
                }
                else
                {
                    WtUtils.Prompt2Second("警告", "剩余量不足");
                }
            }
            else if (obj == "3000")
            {
                if (this.Data.LeftQty > 3000)
                {
                    request.qty = 3000;
                    canComfirm = true;
                }
                else
                {
                    WtUtils.Prompt2Second("警告", "剩余量不足");
                }
            }
            else if (obj == "5000")
            {
                if (this.Data.LeftQty > 5000)
                {
                    request.qty = 5000;
                    canComfirm = true;
                }
                else
                {
                    WtUtils.Prompt2Second("警告", "剩余量不足");
                }
            }
            else if (obj == "报价")
            {
                bool isBid = Data.Side == "1" ? true : false;
                SendPriceViewLite sendPriceViewlite = new SendPriceViewLite(Data.SecurityId, isBid);

                if (sendPriceViewlite.DataContext != null && sendPriceViewlite.DataContext is SendPriceViewModelLite)
                {
                    SendPriceViewModelLite sendPriceViewModel = sendPriceViewlite.DataContext as SendPriceViewModelLite;

                    sendPriceViewModel.OnCustomConfirmAction = SendOrder;
                }

                if (!WindowManager.Instance.IsExist(Utils.WindowKeyHelper.GetSendPriceWinKey(Data.Side, Data.SecurityId)))
                {
                    WindowManager.Instance.OpenOnlyOneWindow(Utils.WindowKeyHelper.GetSendPriceWinKey(Data.Side, Data.SecurityId), sendPriceViewlite);
                }
            }
           
            if (canComfirm)
            {
                ConfirmIntention(request);
            }
        }

        private void SendOrder(Common.Model.Fix.Message.NewOrderSingle order)
        {
            ConfirmIntentionQuoteRequest request = GetFirmOrderRequest();
            decimal orderQty = order.OrderQty.ConvertToDecimal();
            if (this.Data.LeftQty > orderQty)
            {
                request.qty = order.OrderQty.ConvertToDecimal();
            }
            else
            {
                WtUtils.Prompt2Second("警告", "剩余量不足");
            }
            request.firmFlag = order.FirmFlag;
            request.venueTypeGw = order.VenueTypeGW;
            request.venues = order.Venues;
            request.settlType = order.SettlType;
            request.wholeFlag = order.WholeFlag;
            request.riskManualCk = order.RiskManualCk;
            request.handlInst = order.HandlInst;
            request.crossVenues = order.CrossVenues;
            request.venuesSelectF = order.VenuesSelectF;
            request.selfSelectVenues = order.SelfSelectVenues;
            request.venuesRdmNum = order.VenuesRdmNum;
            ConfirmIntention(request);
        }

        private void Instance_OnIntentionOrders(IntentionOrderInfo obj)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                UpdateIntentionOrders(obj);
            }));
        }


        private void UpdateIntentionOrders(IntentionOrderInfo obj)
        {
            try
            {
                string orderid = obj.orderid;
                lock (_IntentionOrders)
                {
                    if (_IntentionOrders.Any(item => item.OrderId == orderid))
                    {
                        IntentionOrderItem findItem = _IntentionOrders.First(item => item.OrderId == orderid);
                        if (findItem.UpdateTime != null && obj.updatetime.CompareTo(findItem.UpdateTime) < 0)
                        {
                            return;
                        }
                        if (!(obj.orderstatus == "N" || obj.orderstatus == "0" || obj.orderstatus == "1" || obj.orderstatus == "S" || obj.orderstatus == "6"))
                        {
                            _IntentionOrders.Remove(findItem);
                        }
                        else
                        {
                            findItem.Price = obj.price.Value;
                            findItem.Qty = (int)obj.orderqty.Value;
                            findItem.SelfSelectVenues = obj.selfselectvenues;
                            findItem.VenuesSelectF = obj.venuesselectf;
                            string venuesSelectDisplay = "内部";
                            if (obj.venuesselectf == "3")
                            {
                                StringBuilder displaySB = new StringBuilder();
                                string[] arr = obj.selfselectvenues.Split(',');
                                foreach (string v in arr)
                                {
                                    if (string.IsNullOrEmpty(v) == false)
                                    {
                                        string v_str = ComboSourceUtil.Instance.FindEnum(v.Substring(v.IndexOf('-') + 1), "venues");
                                        displaySB.Append($"<{v_str}>");
                                    }
                                }
                                venuesSelectDisplay = $"指定{displaySB}";
                            }
                            findItem.DisplayVenues = venuesSelectDisplay;
                            findItem.Venues = obj.venues;
                            findItem.VenueTypeGw = obj.venuetypegw;
                            findItem.UpdateTime = obj.updatetime;
                        }
                    }
                    else
                    {
                        if (!(obj.orderstatus == "N" || obj.orderstatus == "0" || obj.orderstatus == "1" || obj.orderstatus == "S" || obj.orderstatus == "6"))
                        {
                            return;
                        }

                        IntentionOrderItem newItem = new IntentionOrderItem();
                        newItem.OrderId = obj.orderid;
                        newItem.IntentionId = obj.intentionId;
                        newItem.SecurityId = obj.securityId;
                        newItem.Symbol = obj.symbol;
                        newItem.Side = obj.side;
                        newItem.Price = obj.price.Value;
                        newItem.Qty = (int)obj.orderqty.Value;
                        newItem.SelfSelectVenues = obj.selfselectvenues;
                        newItem.VenuesSelectF = obj.venuesselectf;
                        string venuesSelectDisplay = "内部";
                        if (obj.venuesselectf == "3")
                        {
                            StringBuilder displaySB = new StringBuilder();
                            string[] arr = obj.selfselectvenues.Split(',');
                            foreach (string v in arr)
                            {
                                if (string.IsNullOrEmpty(v) == false)
                                {
                                    string v_str = ComboSourceUtil.Instance.FindEnum(v.Substring(v.IndexOf('-') + 1), "venues");
                                    displaySB.Append($"<{v_str}>");
                                }
                            }
                            venuesSelectDisplay = $"指定{displaySB}";
                        }
                        newItem.DisplayVenues = venuesSelectDisplay;
                        newItem.Venues = obj.venues;
                        newItem.VenueTypeGw = obj.venuetypegw;
                        newItem.UpdateTime = obj.updatetime;
                        newItem.FirmFlag = obj.firmflag;
                        _IntentionOrders.Add(newItem);
                    }
                }
            }
            catch (Exception e)
            {
                LogHelper.Error(e);
            }


        }

        private void Instance_OnClear(ClearPo obj)
        {
            LogHandler.Info("IntentionPanelViewModel Clear");
            if (obj != null && "Y".Equals(obj.IsClear))
            {
                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    this.ClearData(false);

                    //this.Init();
                    lock (_IntentionOrders)
                    {
                        _IntentionOrders.Clear();
                    }

                    lock (_IntentionExecs)
                    {
                        _IntentionExecs.Clear();
                    }
                }));
            }
        }

        public void ClearData(bool clearSymbol = false)
        {
            //LogHelper.Info("IntentionPanelViewModel ClearData :" + this._SecurityID);
            if (clearSymbol)
            {
               
            }
        }

        private void onloadData(bool obj)
        {
            if (Data == null)
            {
                return;
            }
            BondsInfoPo bondsInfoPo = CommonUtils.GetBondsInfoPoBySecurityID(Data.SecurityId);
            if (bondsInfoPo != null)
            {
                //Symbol = bondsInfoPo.Short_Name;
                //Security = bondsInfoPo.Bond_ID;
                //SymbolName = bondsInfoPo.Short_Name;
                this.Data.RemainingTime = bondsInfoPo.RemaingingTime;
                this.Data.SecurityType = bondsInfoPo.SECURITY_TYPE;
            }
        }
        private void GetSymbolInfo()
        {
            if (!string.IsNullOrEmpty(Data.SecurityId))
            {
                BondsInfoPo bondsInfoPo = CommonUtils.GetBondsInfoPoBySecurityID(Data.SecurityId);
                if (bondsInfoPo != null)
                {
                    this.Data.RemainingTime = bondsInfoPo.RemaingingTime;
                    this.Data.SecurityType = bondsInfoPo.SECURITY_TYPE;
                }
            }
        }

        private void Instance_OnIntentionQuotes(Common.Model.Table.IntentionQuoteInfo obj)
        {
            if (obj.intentionId == this.IntentionID)
            {
                UpateData(obj);
            }
        }

        private void UpateData(IntentionQuoteInfo obj)
        {
            if (this.Data == null)
                this.Data = new TradeRequestData();

            this.Data.IntentionId = obj.intentionId;
            this.Data.SecurityId = obj.securityId;
            this.Data.Symbol = obj.symbol;
            this.Data.Side = obj.side;
            this.Data.NetPrice = obj.netPrice;
            this.Data.Price = obj.price;
            this.Data.BookQty = obj.bookQty;
            this.Data.LeftQty = obj.leftQty;
            this.Data.Qty = obj.qty;
            this.Data.InstitutionId = obj.institutionId;
            this.Data.InstitutionName = obj.institutionName;
            this.Data.TraderId = obj.traderId;
            this.Data.TraderName = obj.traderName;
            this.Data.QqNumber = obj.qqNumber;
            this.Data.Text = obj.text;
            this.Data.Status = obj.status;
            this.Data.SettlType = obj.settlType;
            this.Data.CreateTime = obj.createTime;
            this.Data.UpdateTime = obj.updateTime;
            this.Data.NikeName = obj.nickName;
            this.Data.UserName = obj.userName;
            double completeRate = 0;
            double.TryParse(obj.completeRate == null ? "0" : obj.completeRate.ToString(), out completeRate);
            this.Data.CompleteRate = completeRate;
            //?this.Data.ExecutionPrice = obj.ExecutionPrice;
            this.Data.PNL = obj.profitLoss;
            this.Data.RunningTime = obj.runningTime;
            GetSymbolInfo();
            string displayside = ComboSourceUtil.Instance.FindEnum(obj.side, "side");
            string displayPrice = obj.price?.ToString("0.00##");
            string displayQty = (obj.qty / 1000)?.ToString("0") + "kw";
            string displaySettletype = ComboSourceUtil.Instance.FindEnum(obj.settlType, "settletype");
            this.BaseInfo = $"{obj.securityId}  {displayside} {displayPrice}  {displayQty} {displaySettletype}";
        }


        internal ConfirmIntentionQuoteRequest GetFirmOrderRequest()
        {
            ConfirmIntentionQuoteRequest request = new ConfirmIntentionQuoteRequest();
            request.intentionId = this.IntentionID;
            request.userName = LocalEnv.UserInfo.UserName;
            request.clOrdId = DataTimeSequence.Instance.GetId();
            request.actionType = "2";//闪电确认
            request.venueTypeGw = "INNER";
            request.venues = "MANUAL";
            request.orderType = "2"; //1:市价单; 2:限价单
            request.timeInForce = "0";// 0:DAY; 1:GTC; 3:IOC; 4:FOK; A:GFD
            request.firmFlag = "1";
            return request;
        }

        internal void ConfirmIntention(ConfirmIntentionQuoteRequest request)
        {
            IntentionOrder io = new IntentionOrder();
            io.request = request;
            io.detailIntentionItem = new TradeRequestData();
            io.detailIntentionItem.SecurityId = this.Data.SecurityId;
            io.detailIntentionItem.Side = this.Data.Side;
            io.detailIntentionItem.Price = request.price == null ? this.Data.Price : request.price.Value;
            io.detailIntentionItem.Qty = request.qty == null ? this.Data.Qty : request.qty;

            System.Threading.ThreadPool.QueueUserWorkItem(o =>
            {
                try
                {
                    ConfirmIntentionQuoteReplyPo replyPo = IntentionHandler.Instance.ConfirmIntentionQuote(request);
                    if (replyPo == null)
                    {
                        WtUtils.Prompt2Second("警告", $"意向id:[{this.IntentionID}]普通确认失败");
                    }
                    else
                    {
                        if (!replyPo.IsSuccess())
                        {
                            if (replyPo.Code == "10000")
                            {
                                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                                {
                                    SendPriceIntentionWarnView warnView = new SendPriceIntentionWarnView(replyPo, io);
                                    warnView.ShowDialog();
                                }));

                            }
                            else
                            {
                                WtUtils.Prompt2Second("警告", replyPo.Msg);
                            }
                        }
                        else
                        {
                            //replyUpdateThread.Enqueue(replyPo);
                        }
                    }
                }
                catch (Exception e)
                {
                    LogHelper.Error("ConfirmIntention", e);
                }
            });
        }

        #region 改单
        Dictionary<string, Common.Model.Fix.Message.NewOrderSingle> newOrderSingleDic = new Dictionary<string, Common.Model.Fix.Message.NewOrderSingle>();
        Dictionary<string, List<TPlaceOrderResult>> warnReplyDic = new Dictionary<string, List<TPlaceOrderResult>>();
        Dictionary<string, List<TPlaceOrderResult>> prohibitReplyDic = new Dictionary<string, List<TPlaceOrderResult>>();
        /// <summary>
        /// 批量操作按钮是否可用
        /// </summary>
        private bool _BatchOperateBtnEnanble = true;

        public bool BatchOperateBtnEnable
        {
            get
            {
                return _BatchOperateBtnEnanble;
            }
            set
            {
                if (_BatchOperateBtnEnanble != value)
                {
                    _BatchOperateBtnEnanble = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private long opsCount = 0;
        private volatile int failCount = 0;
        private BatchOperateTypeEnum batchOpsTypeEnum;
        private void EditPrice(string parameter)
        {
            BatchOperateBtnEnable = false;
            newOrderSingleDic.Clear();
            warnReplyDic.Clear();
            prohibitReplyDic.Clear();
            batchOpsTypeEnum = BatchOperateTypeEnum.EditPrice;

            lock (_IntentionOrderLock)
            {
                //foreach (IntentionOrderItem drv in this._IntentionOrderViewSource.View)
                //{
                //    if (!(drv[MyOrderProcesser.ISCHECKED_ORDER_FIELD] is System.DBNull)
                //        && (bool)drv[MyOrderProcesser.ISCHECKED_ORDER_FIELD] == true)
                //    {

                       Common.Model.Fix.Message.NewOrderSingle newOrderSingle = GetNewOrderSingle(MyOrderProcesser.Processer.GetOrderById(this.SelectIntentionOrder.OrderId));
                        modifyPrice(newOrderSingle, parameter);

                        if (newOrderSingle.Venues == "IDB")
                        {
                            newOrderSingle.Venues = "MANUAL";
                        }
                        newOrderSingleDic[newOrderSingle.StrategyOrderId] = newOrderSingle;
                //    }
                //}
            }

            if (newOrderSingleDic.Count == 0)
            {
                BatchOperateBtnEnable = true;
                return;
            }

            Task.Run(() =>
            {
                try
                {
                    InitStatistic();

                    foreach (STC.Common.Model.Fix.Message.NewOrderSingle sendPricePo in newOrderSingleDic.Values)
                    {
                        ThreadPool.QueueUserWorkItem(o => {
                            string securityID = sendPricePo.SecurityID;
                            string settlType = sendPricePo.SettlType;
                            string price = sendPricePo.Price;
                            decimal netPrice = CalBond(securityID, "4", settlType, NumChange(price));
                            sendPricePo.NetPrice = netPrice.ToString();
                            OrdersHandler.Instance.ModifyOrders(sendPricePo, sendPricePo.StrategyOrderId, UpdatePriceCallBack);
                        });
                    }
                }
                catch (Exception e)
                {
                    LogHelper.Error(e);
                }
            });
        }

        /// <summary>
        /// 初始化统计值
        /// </summary>
        private void InitStatistic()
        {
            Interlocked.Exchange(ref opsCount, newOrderSingleDic.Count);
            failCount = 0;
        }

        private void UpdatePriceCallBack(ModifyOrdersReply reply)
        {
            Interlocked.Decrement(ref opsCount);
            if (newOrderSingleDic != null)
            {
                if (reply.IsSuccess() == false)
                {
                    failCount++;
                    if (reply.Code == "10000")
                    {


                        try
                        {
                            bool isProhibitory = false;
                            List<TPlaceOrderResult> orderRisks = MsgHelper.Deserialize<List<TPlaceOrderResult>>(reply.Msg);
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
                                prohibitReplyDic[reply.OrderID] = orderRisks;
                            }
                            else
                            {
                                warnReplyDic[reply.OrderID] = orderRisks;
                            }
                        }
                        catch (Exception ex)
                        {
                            LogHelper.Error(ex);
                        }
                    }
                }
            }

            if (Interlocked.Read(ref opsCount) == 0)
            {
                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    foreach (IntentionOrderItem order in this.IntentionExecViewSource.View)
                    {
                        if (order.OrderId == reply.OrderID)
                        {
                            this.SelectIntentionOrder = order;
                        }
                    }

                    BatchOperateBtnEnable = true;
                    int totalCount = newOrderSingleDic.Count;

                    if (newOrderSingleDic.Count > 0 && (warnReplyDic.Count > 0 || prohibitReplyDic.Count > 0))
                    {
                        BatchResultWindow batchResultWindow = new BatchResultWindow(newOrderSingleDic,
                            warnReplyDic, prohibitReplyDic, failCount);
                        batchResultWindow.Owner = Application.Current.MainWindow;
                        batchResultWindow.ShowDialog();
                    }
                    else
                    {
                        string text = $"共执行 {totalCount} 条, 其中 {totalCount - failCount} 条成功";

                        if (batchOpsTypeEnum == BatchOperateTypeEnum.EditQty)
                        {
                            if (failCount == 0)
                            {
                                WtUtils.Prompt2Second("订单已更新", "订单已更新");
                            }
                            else
                            {
                                WtUtils.Prompt2Second("批量编辑报量", text);
                            }

                        }
                        else if (batchOpsTypeEnum == BatchOperateTypeEnum.EditPrice)
                        {
                            if (failCount == 0)
                            {
                                WtUtils.Prompt2Second("订单已更新", "订单已更新");
                            }
                            else
                            {
                                WtUtils.Prompt2Second("批量编辑报价", text);
                            }
                        }
                        else if (batchOpsTypeEnum == BatchOperateTypeEnum.EditVenues)
                        {
                            if (failCount == 0)
                            {
                                WtUtils.Prompt2Second("渠道已更新", "渠道已更新");
                            }
                            else
                            {
                                WtUtils.Prompt2Second("批量编辑渠道", text);
                            }
                        }
                    }
                }));
            }
        }

        private string NumChange(string price)
        {
            decimal d = 0;
            if (decimal.TryParse(price, out d))
            {
                d = d / 100;
                price = d.ToString();
            }

            return price;
        }


        public decimal CalBond(string securityID, string calType, string settleType, string val)
        {
            try
            {
                CalReq requst = new CalReq();
                requst.SecurityID = securityID;
                requst.CalType = calType;//"4";
                requst.SettlType = settleType;// "1";
                requst.Value = val;// customPrice;

                CalResult result = CalHandler.Instance.CalBond(requst);
                if (requst != null && result.Data != null)
                {
                    if (calType.Equals("4"))
                    {
                        //NetPrice = result.Data.CleanPrice.ToString("N4");
                        return result.Data.CleanPrice;
                    }
                    else if (calType.Equals("2"))
                    {
                        //CustomPrice = (result.Data.Yield * 100).ToString("N4");
                        return result.Data.Yield;
                    }
                }
            }
            catch (Exception e)
            {
                LogHelper.Error(e);
            }
            return decimal.Zero;
        }


        private void modifyPrice(Common.Model.Fix.Message.NewOrderSingle newOrderSingle, string parameter)
        {
            decimal price = CommonUtils.ConvertToDecimal(newOrderSingle.Price);
            if (parameter == "+5")
            {
                price += 0.05m;
            }
            else if (parameter == "+1")
            {
                price += 0.01m;
            }
            else if (parameter == "+0.5")
            {
                price += 0.005m;
            }
            else if (parameter == "+0.25")
            {
                price += 0.0025m;
            }
            else if (parameter == "-0.25")
            {
                price -= 0.0025m;
            }
            else if (parameter == "-0.5")
            {
                price -= 0.005m;
            }
            else if (parameter == "-1")
            {
                price -= 0.01m;
            }
            else if (parameter == "-5")
            {
                price -= 0.05m;
            }
            newOrderSingle.Price = price.ToString();
        }

        private void EditQty(string parameter)
        {
            BatchOperateBtnEnable = false;
            newOrderSingleDic.Clear();
            warnReplyDic.Clear();
            prohibitReplyDic.Clear();
            batchOpsTypeEnum = BatchOperateTypeEnum.EditQty;

            lock (_IntentionOrderLock)
            {
                //foreach (DataRowView drv in this.MyOrdersDV)
                //{
                //    if (!(drv[MyOrderProcesser.ISCHECKED_ORDER_FIELD] is System.DBNull)
                //        && (bool)drv[MyOrderProcesser.ISCHECKED_ORDER_FIELD] == true)
                //    {
                        Common.Model.Fix.Message.NewOrderSingle newOrderSingle = GetNewOrderSingle(MyOrderProcesser.Processer.GetOrderById(this.SelectIntentionOrder.OrderId));
                        modifyQty(newOrderSingle, parameter);
                        int orderQty = CommonUtils.ConvertToInt(newOrderSingle.OrderQty);
                        if (orderQty > 1000000)
                        {
                            BatchOperateBtnEnable = true;

                            WtUtils.Prompt2Second("修改订单数量", "不支持大于1亿的报单");
                            return;
                        }
                        if (newOrderSingle.Venues == "IDB")
                        {
                            newOrderSingle.Venues = "MANUAL";
                        }
                        newOrderSingleDic[newOrderSingle.StrategyOrderId] = newOrderSingle;
                //    }
                //}
            }

            if (newOrderSingleDic.Count == 0)
            {
                BatchOperateBtnEnable = true;
                return;
            }

            Task.Run(() =>
            {
                try
                {
                    InitStatistic();

                    foreach (STC.Common.Model.Fix.Message.NewOrderSingle sendPricePo in newOrderSingleDic.Values)
                    {
                        int orderQty = CommonUtils.ConvertToInt(sendPricePo.OrderQty);
                        if (orderQty <= 0)
                        {
                            OrderCancelRequest cancelPo = new OrderCancelRequest();
                            cancelPo.AlgoName = sendPricePo.AlgoName;
                            cancelPo.ClOrdID = DataTimeSequence.Instance.GetId();
                            cancelPo.OrderID = sendPricePo.StrategyOrderId;
                            TBookCancelReplyPo result = OrdersHandler.Instance.CancelOrder(cancelPo);

                            ModifyOrdersReply reply = new ModifyOrdersReply();
                            if (result.IsSuccess())
                            {
                                reply.Code = "0000";
                                reply.Msg = "撤单成功";
                            }
                            else
                            {
                                reply.Code = "0001";
                                reply.Msg = "撤单失败";
                            }

                            reply.OrderID = sendPricePo.StrategyOrderId;
                            UpdatePriceCallBack(reply);
                        }
                        else if (orderQty > 1000000)
                        {
                            ModifyOrdersReply reply = new ModifyOrdersReply();
                            reply.Code = "0001";
                            reply.Msg = "报量超出最大上限1000000万";
                            reply.OrderID = sendPricePo.StrategyOrderId;
                            UpdatePriceCallBack(reply);
                        }
                        else
                        {
                            OrdersHandler.Instance.ModifyOrders(sendPricePo, sendPricePo.StrategyOrderId, UpdatePriceCallBack);
                        }

                    }
                }
                catch (Exception e)
                {
                    LogHelper.Error(e);
                }
            });

        }

        private void modifyQty(Common.Model.Fix.Message.NewOrderSingle newOrderSingle, string parameter)
        {
            int qty = CommonUtils.ConvertToInt(newOrderSingle.OrderQty);
            if (parameter == "+5kw")
            {
                qty += 5000;
            }
            if (parameter == "+3kw")
            {
                qty += 3000;
            }
            else if (parameter == "+1kw")
            {
                qty += 1000;
            }
            else if (parameter == "-5kw")
            {
                qty -= 5000;
            }
            else if (parameter == "-3kw")
            {
                qty -= 3000;
            }
            else if (parameter == "-1000")
            {
                qty -= 1000;
            }

            newOrderSingle.OrderQty = qty.ToString();
        }

        internal void EditVenues(string parameter)
        {
            BatchOperateBtnEnable = false;
            newOrderSingleDic.Clear();
            warnReplyDic.Clear();
            prohibitReplyDic.Clear();
            batchOpsTypeEnum = BatchOperateTypeEnum.EditQty;

            lock (MyOrderProcesser.Processer.LockObj_MyOrders)
            {
                //foreach (DataRowView drv in this.MyOrdersDV)
                //{
                //    if (!(drv[MyOrderProcesser.ISCHECKED_ORDER_FIELD] is System.DBNull)
                //        && (bool)drv[MyOrderProcesser.ISCHECKED_ORDER_FIELD] == true)
                //    {
                        Common.Model.Fix.Message.NewOrderSingle newOrderSingle = GetNewOrderSingle(MyOrderProcesser.Processer.GetOrderById(this.SelectIntentionOrder.OrderId));

                        modifyVenues(newOrderSingle, parameter);


                        if (newOrderSingle.Venues == "IDB")
                        {
                            newOrderSingle.Venues = "MANUAL";
                        }
                        newOrderSingleDic[newOrderSingle.StrategyOrderId] = newOrderSingle;
                //    }
                //}
            }

            if (newOrderSingleDic.Count == 0)
            {
                BatchOperateBtnEnable = true;
                return;
            }

            Task.Run(() =>
            {
                try
                {
                    InitStatistic();

                    foreach (STC.Common.Model.Fix.Message.NewOrderSingle sendPricePo in newOrderSingleDic.Values)
                    {
                        OrdersHandler.Instance.ModifyOrders(sendPricePo, sendPricePo.StrategyOrderId, UpdatePriceCallBack);
                    }
                }
                catch (Exception e)
                {
                    LogHelper.Error(e);
                }
            });
        }

        private void modifyVenues(Common.Model.Fix.Message.NewOrderSingle newOrderSingle, string parameter)
        {
            if (parameter == "仅内部")
            {
                newOrderSingle.CrossVenues = "2";
                newOrderSingle.VenuesSelectF = string.Empty;
                newOrderSingle.SelfSelectVenues = null;
                newOrderSingle.VenuesRdmNum = null;
            }
            if (parameter == "XBOND")
            {
                newOrderSingle.CrossVenues = "3";
                newOrderSingle.VenuesSelectF = "3";
                newOrderSingle.SelfSelectVenues = "ODM-ODM";
                newOrderSingle.VenuesRdmNum = null;
            }
            else if (parameter == "默认")
            {
                newOrderSingle.CrossVenues = "1";
                newOrderSingle.VenuesSelectF = "2";
                newOrderSingle.SelfSelectVenues = string.Empty;
                newOrderSingle.VenuesRdmNum = null;
            }
            else if (parameter.Contains("默认+"))
            {

                newOrderSingle.CrossVenues = "1";
                newOrderSingle.VenuesSelectF = "2";
                newOrderSingle.SelfSelectVenues = string.Empty;
                newOrderSingle.VenuesRdmNum = parameter.Substring(3);
            }
        }


        internal void AssignVenues(string SelfSelectVenues)
        {
            BatchOperateBtnEnable = false;
            newOrderSingleDic.Clear();
            warnReplyDic.Clear();
            prohibitReplyDic.Clear();
            batchOpsTypeEnum = BatchOperateTypeEnum.EditVenues;

            bool hasBNC = false;
            bool hasBCO = false;
            bool isfirm = true;
            int i = 0;
            lock (_IntentionOrderLock)
            {
                //foreach (DataRowView drv in this.MyOrdersDV)
                //{
                //    if (!(drv[MyOrderProcesser.ISCHECKED_ORDER_FIELD] is System.DBNull)
                //        && (bool)drv[MyOrderProcesser.ISCHECKED_ORDER_FIELD] == true)
                //    {
                Common.Model.Fix.Message.NewOrderSingle newOrderSingle = GetNewOrderSingle(MyOrderProcesser.Processer.GetOrderById(this.SelectIntentionOrder.OrderId));

                if (CommonUtils.IsRate(newOrderSingle.SecurityType))
                        {
                            hasBNC = true;
                        }
                        else
                        {
                            hasBCO = true;
                        }

                        if (newOrderSingle.Venues == "IDB")
                        {
                            newOrderSingle.Venues = "MANUAL";
                        }
                        if (newOrderSingle.FirmFlag == "0" && i == 0)
                        {
                            isfirm = false;
                            i++;
                        }
                        newOrderSingleDic[newOrderSingle.StrategyOrderId] = newOrderSingle;
                //    }
                //}
            }

            if (newOrderSingleDic.Count == 0)
            {
                BatchOperateBtnEnable = true;
                return;
            }

            //AssignVenuesWindow win = new AssignVenuesWindow(isfirm);
            //AssignVenuesWindowViewModel viewModel = win.DataContext as AssignVenuesWindowViewModel;
            //if (hasBNC) viewModel.IsShowInterestRateDebt = Visibility.Visible;
            //if (hasBCO) viewModel.IsShowCreditDebt = Visibility.Visible;
            //bool? result = win.ShowDialog();

            //string bncSelfSelectVenues = win.InterestRateDebt.SelfSelectVenues;
            //string bcoSelfSelectsVenues = win.CreditDebt.SelfSelectVenues;

            //if (result == false)
            //{
            //    BatchOperateBtnEnable = true;
            //    return;
            //}

            Task.Run(() =>
            {
                try
                {
                    InitStatistic();

                    foreach (STC.Common.Model.Fix.Message.NewOrderSingle sendPricePo in newOrderSingleDic.Values)
                    {
                        sendPricePo.CrossVenues = "1";
                        sendPricePo.VenuesSelectF = "3";
                        sendPricePo.VenuesRdmNum = null;
                        sendPricePo.SelfSelectVenues = SelfSelectVenues;
                        //if (CommonUtils.IsRate(sendPricePo.SecurityType))
                        //{
                        //    sendPricePo.SelfSelectVenues = bncSelfSelectVenues;
                        //}
                        //else
                        //{
                        //    sendPricePo.SelfSelectVenues = bcoSelfSelectsVenues;
                        //}

                        OrdersHandler.Instance.ModifyOrders(sendPricePo, sendPricePo.StrategyOrderId, UpdatePriceCallBack);
                    }
                }
                catch (Exception e)
                {
                    LogHelper.Error(e);
                }
            });


        }

        private Common.Model.Fix.Message.NewOrderSingle GetNewOrderSingle(DataRow drv)
        {
            string strategyCodeID = CommonUtils.ConvertToString(drv[MyOrderProcesser.STCCODE_ORDER_FIELD]);
            string securityID = CommonUtils.ConvertToString(drv[MyOrderProcesser.SECURITYID_ORDER_FIELD]);
            string qty = CommonUtils.ConvertToString(drv[MyOrderProcesser.QTY_ORDER_FIELD]);
            string exeqty = CommonUtils.ConvertToString(drv[MyOrderProcesser.EXECQTY_ORDER_FIELD]);
            string price = CommonUtils.ConvertToString(drv[MyOrderProcesser.PRICE_ORDER_FIELD]);
            string netPrice = CommonUtils.ConvertToString(drv[MyOrderProcesser.NETPRICE_ORDER_FIELD]);
            string settlType = CommonUtils.ConvertToString(drv[MyOrderProcesser.SETTLETYPE_ORDER_FIELD]);
            string side = CommonUtils.ConvertToString(drv[MyOrderProcesser.SIDE_ORDER_FIELD]);
            string wholeFlag = CommonUtils.ConvertToString(drv[MyOrderProcesser.WHOLE_FLAG_FIELD]);
            string firmFlag = CommonUtils.ConvertToString(drv[MyOrderProcesser.FIRMFLAG_ORDER_FIELD]);
            string crossVenues = CommonUtils.ConvertToString(drv[MyOrderProcesser.CROSS_VENUES_FIELD]);
            string firmToNonTime = CommonUtils.ConvertToString(drv[MyOrderProcesser.NONFIRM_TIME_FIELD]);
            string randomNum = CommonUtils.ConvertToString(drv[MyOrderProcesser.VENUES_RDM_NUM_FIELD]);
            string venuesSelectF = CommonUtils.ConvertToString(drv[MyOrderProcesser.VENUES_SELECT_F_FIELD]);
            string selfSelectVenues = CommonUtils.ConvertToString(drv[MyOrderProcesser.SELF_SELECT_VENUES_FIELD]);


            string venues = CommonUtils.ConvertToString(drv[MyOrderProcesser.VENEUES_ORDER_FIELD]);
            string venueTypeGw = CommonUtils.ConvertToString(drv[MyOrderProcesser.VENUETYPEGW_FIELD]);
            string algoName = CommonUtils.ConvertToString(drv[MyOrderProcesser.ALGONAME_ORDER_FIELD]);
            string quantID = CommonUtils.ConvertToString(drv[MyOrderProcesser.QUANTID_FIELD]);
            string timeInForce = CommonUtils.ConvertToString(drv[MyOrderProcesser.TIMEINFORCE_FIELD]);
            string ordStatus = CommonUtils.ConvertToString(drv[MyOrderProcesser.ORDERSTATUS_ORDER_FIELD]);
            string bizType = CommonUtils.ConvertToString(drv[MyOrderProcesser.BIZTYPE_FIELD]);
            string bizTypeID = CommonUtils.ConvertToString(drv[MyOrderProcesser.BIZTYPEID_FIELD]);
            string quoteID = CommonUtils.ConvertToString(drv[MyOrderProcesser.QUOTEID_FIELD]);
            string roleID = CommonUtils.ConvertToString(drv[MyOrderProcesser.ROLEID_FIELD]);
            string roleName = CommonUtils.ConvertToString(drv[MyOrderProcesser.ROLENAME_FIELD]);
            string symbol = CommonUtils.ConvertToString(drv[MyOrderProcesser.SYMBOL_ORDER_FIELD]);
            string userId = CommonUtils.ConvertToString(drv[MyOrderProcesser.USERID_FIELD]);
            string institutionID = CommonUtils.ConvertToString(drv[MyOrderProcesser.INSTITUTIONID_FIELD]);
            string institutionName = CommonUtils.ConvertToString(drv[MyOrderProcesser.INSTITUTIONNAME_FIELD]);
            string traderID = CommonUtils.ConvertToString(drv[MyOrderProcesser.TRADERID_FIELD]);
            string traderName = CommonUtils.ConvertToString(drv[MyOrderProcesser.TRADERNAME_FIELD]);
            string showTrader = CommonUtils.ConvertToString(drv[MyOrderProcesser.SHOW_TRADER_FIELD]);
            string ternimal = CommonUtils.ConvertToString(drv[MyOrderProcesser.TERMINAL_FIELD]);
            string info3 = CommonUtils.ConvertToString(drv[MyOrderProcesser.INFO3_FIELD]);
            string intentionId = CommonUtils.ConvertToString(drv[MyOrderProcesser.INTENTIONID_FIELD]);

            Common.Model.Fix.Message.NewOrderSingle sendPricePo = new Common.Model.Fix.Message.NewOrderSingle();

            BondsInfoPo bondsInfoPo = CommonUtils.GetBondsInfoPoBySecurityID(securityID);
            sendPricePo.SecurityType = bondsInfoPo.SECURITY_TYPE;
            sendPricePo.BizType = bizType;
            sendPricePo.BizTypeID = bizTypeID;
            sendPricePo.QuoteID = quoteID;
            sendPricePo.RoleID = roleID;
            sendPricePo.RoleName = roleName;
            sendPricePo.Symbol = symbol;
            sendPricePo.UserID = userId;
            sendPricePo.OrdStatus = ordStatus;
            sendPricePo.StrategyOrderId = strategyCodeID;

            sendPricePo.AlgoName = algoName;
            sendPricePo.VenueTypeGW = venueTypeGw;
            sendPricePo.Venues = venues;
            sendPricePo.ClOrdID = DataTimeSequence.Instance.GetId();
            sendPricePo.MarketIndicator = "4";
            sendPricePo.OrdType = "2";
            sendPricePo.OrderQty = qty;//(CommonUtils.ConvertToDecimal(qty) - CommonUtils.ConvertToDecimal(exeqty)).ToString();
            sendPricePo.Price = price;
            sendPricePo.NetPrice = netPrice;
            sendPricePo.SecurityID = securityID;

            sendPricePo.RiskManualCk = string.Empty;//默认空值（检查）, 1：只检查风控禁止，2：风控不检查
            sendPricePo.SettlType = settlType;

            sendPricePo.Side = side;

            sendPricePo.Terminal = string.Format("STClient_v{0}", LocalEnv.ServerInfo.Version);
            sendPricePo.TimeInForce = timeInForce;
            sendPricePo.UserName = LocalEnv.UserInfo.UserName;

            //选中：0(nonfirm)，未选中：1(firm)
            sendPricePo.FirmFlag = firmFlag;

            sendPricePo.CrossVenues = crossVenues;

            sendPricePo.WholeFlag = wholeFlag;

            sendPricePo.QuantID = quantID;

            sendPricePo.FirmToNonTime = firmToNonTime;

            sendPricePo.VenuesRdmNum = randomNum;

            sendPricePo.VenuesSelectF = venuesSelectF;

            sendPricePo.SelfSelectVenues = selfSelectVenues;

            sendPricePo.InstitutionID = institutionID; //客盘机构id
            sendPricePo.InstitutionName = institutionName;//客盘机构名字
            sendPricePo.TraderID = traderID; // 客盘交易员id
            sendPricePo.TraderName = traderName;//客盘交易员名字
            sendPricePo.ShowTrader = showTrader;// 是否显示经济商

            //sendPricePo.ValidUntilTime = DateTime.Now.AddDays(10).ToString("yyyy-MM-dd HH:mm:ss");
            sendPricePo.Terminal = ternimal;
            sendPricePo.Info3 = info3;
            sendPricePo.Info4 = intentionId;

            return sendPricePo;
        }
        #endregion
    }
}
