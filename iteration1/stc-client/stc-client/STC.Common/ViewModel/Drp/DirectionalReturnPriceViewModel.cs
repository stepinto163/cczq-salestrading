using Ats.Foundation.Message;
using com.gateway.log;
using STC.Common.Command;
using STC.Common.Data;
using STC.Common.Handler.Admin;
using STC.Common.Handler.Inner;
using STC.Common.Handler.SystemParam;
using STC.Common.Handler.TBook;
using STC.Common.Model.Admin;
using STC.Common.Model.Base;
using STC.Common.Model.Drp;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.TBook;
using STC.Common.Utils;
using STC.Common.View.Component;
using STC.Common.View.Drp.Component;
using STC.Common.ViewModel.Drp.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace STC.Common.ViewModel.Drp
{
    /// <summary>
    /// PriceMatchView的数据内容
    /// </summary>
    public class DirectionalReturnPriceViewModel : ViewModelBase
    {
        private MsgPo _currentMsg = null;
        private AsyncThreadQueue<List<TTbookSystemMessage>> _msgTask = null;
        private string currentMsgId = string.Empty;
        private object _lockObj = new object();
        private Window _myWindow;
        private DRPDetail drd = null;

        #region 界面属性
        public string CurrentMsgId
        {
            get { return currentMsgId; }
            set
            {
                if (value != null)
                {
                    currentMsgId = value;
                    this.OnPropertyChanged(nameof(currentMsgId));
                }
            }
        }

        private string _orderid = string.Empty;
        public string OrderId
        {
            get { return _orderid; }
            set
            {
                if (value != null)
                {
                    _orderid = value;
                    this.OnPropertyChanged(nameof(OrderId));
                }
            }
        }

        private Brush borderBrush = new SolidColorBrush(Color.FromRgb(0x28, 0x28, 0x28));
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


        private bool isAllChecked;
        public bool IsAllChecked 
        {
            get 
            { 
                return isAllChecked; 
            } 
            set 
            {
                isAllChecked = value; 
                this.OnPropertyChanged(); 
                this.OnCheckAll(); 
            } 
        }

        private ObservableCollection<DRPListModel> _gridDatas;

        /// <summary>
        /// 回价列表
        /// </summary>
        public ObservableCollection<DRPListModel> GridDatas
        {
            get
            {
                if (_gridDatas == null)
                    _gridDatas = new ObservableCollection<DRPListModel>();

                return _gridDatas;
            }

            set
            {
                if (value != null)
                {
                    _gridDatas = value;
                    this.OnPropertyChanged(nameof(GridDatas));
                }
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

        private DRPListModel selectDrp;

        public DRPListModel SelectDrp
        {
            get { return selectDrp; }
            set
            {
                if (value != null)
                {
                    selectDrp = value;
                    this.OnPropertyChanged(nameof(SelectDrp));
                }
            }
        }

        private string _contentStart;
        /// <summary>
        /// 债券代码
        /// </summary>
        public string ContentStart
        {
            get
            {
                return _contentStart;
            }

            set
            {
                _contentStart = value;

                this.OnPropertyChanged();
            }
        }

        private string _firmType;
        /// <summary>
        /// 显示用Firm类型
        /// </summary>
        public string FirmType
        {
            get
            {
                return _firmType;
            }

            set
            {
                _firmType = value;

                this.OnPropertyChanged();
            }
        }


        private string _showside;
        public string ShowSide
        {
            get
            {
                return _showside;
            }

            set
            {
                _showside = value;

                this.OnPropertyChanged();
            }
        }

        private string _ShowPrice;
        public string ShowPrice
        {
            get
            {
                return _ShowPrice;
            }

            set
            {
                _ShowPrice = value;
                this.OnPropertyChanged();
            }
        }


 
        private Visibility _errorVisibility = Visibility.Collapsed;
        /// <summary>
        /// 选择汇总区错误信息是否可见
        /// </summary>
        public Visibility ErrorVisibility
        {
            get
            {
                return _errorVisibility;
            }

            set
            {
                _errorVisibility = value;
                this.OnPropertyChanged();
            }
        }

        private string _errorTxt;
        public string ErrorTxt
        {
            get
            {
                return _errorTxt;
            }

            set
            {
                _errorTxt = value;

                this.OnPropertyChanged();
            }
        }

        private Visibility _checkAmountErrorVisibility = Visibility.Collapsed;
        /// <summary>
        /// 选择汇总区信息是否可见
        /// </summary>
        public Visibility CheckAmountErrorVisibility
        {
            get
            {
                return _checkAmountErrorVisibility;
            }

            set
            {
                _checkAmountErrorVisibility = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// 当点价总量>报价总量时为red， 否则为white
        /// </summary>
        private string _warningColor = "red";

        public string WarningColor
        {
            get
            {
                return _warningColor;
            }
            set
            {
                _warningColor = value;
                this.OnPropertyChanged();
            }
        }

        private string _customAmount;
        /// <summary>
        /// 已确认量
        /// </summary>
        public string CustomAmount
        {
            get
            {
                return _customAmount;
            }

            set
            {
                _customAmount = value;

                this.OnPropertyChanged();
            }
        }


        private string _ShowTotal = "--";
        /// <summary>
        /// 报价总量
        /// </summary>
        public string ShowTotal
        {
            get
            {
                return _ShowTotal;
            }

            set
            {
                _ShowTotal = value;
                this.OnPropertyChanged(nameof(ShowTotal));
            }
        }



        private decimal _matchTotal = decimal.Zero;
        /// <summary>
        /// 报价总量
        /// </summary>
        public decimal TotalAmout
        {
            get
            {
                return _matchTotal;
            }

            set
            {
                _matchTotal = value;
                this.OnPropertyChanged(nameof(TotalAmout));
            }
        }

        private decimal _pointTotal = decimal.Zero;
        /// <summary>
        /// 点价总量
        /// </summary>
        public decimal PointTotal
        {
            get
            {
                return _pointTotal;
            }
            set
            {
                _pointTotal = value;
                this.OnPropertyChanged();
            }
        }

        private decimal _leavesqtytotal = decimal.Zero;
        /// <summary>
        /// 待确认量
        /// </summary>
        public decimal LeavesQtyTotal
        {
            get
            {
                return _leavesqtytotal;
            }
            set
            {
                _leavesqtytotal = value;
                this.OnPropertyChanged();
            }
        }

        private string _showleavesqtytotal = "--";
        /// <summary>
        /// 待确认量
        /// </summary>
        public string ShowLeavesQtyTotal
        {
            get
            {
                return _showleavesqtytotal;
            }
            set
            {
                _showleavesqtytotal = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// 原始输入整量标记
        /// </summary>
        private string _wholeflag = string.Empty;
        public string Wholeflag
        {
            get { return _wholeflag; }
            set
            {
                _wholeflag = value;
                this.OnPropertyChanged(nameof(Wholeflag));
            }
        }

        private string ioccount;
        /// <summary>
        /// 有效回价个数
        /// </summary>
        public string IocCount { get { return ioccount; } set { ioccount = value; this.OnPropertyChanged(); } }

        private string iocrecccount;
        /// <summary>
        /// 回价个数
        /// </summary>
        public string IocReccCount { get { return iocrecccount; } set { iocrecccount = value; this.OnPropertyChanged(); } }

        private List<PriceMatchParam> _priceMatchParams;
        private List<DRPValue> _priceValues;
        private bool IsSetCheckedFromRefreshData = false;
        private bool IsSetFromAutoTotal = false;

        private string orderremainseconds;
        /// <summary>
        /// 母单的倒计时
        /// </summary>
        public string OrderRemainSeconds { get { return orderremainseconds; } set { orderremainseconds = value; this.OnPropertyChanged(); } }

                    
        private string ordervaliduntitime;
        /// <summary>
        /// 母单撤单时间
        /// </summary>
        public string OrderValidUntiTime { get { return ordervaliduntitime; } set { ordervaliduntitime = value; this.OnPropertyChanged(); } }

        public ICommand CloseCommand { get; set; }
        public ICommand AddAmountCommand { get; set; }
        public ICommand DownAmountCommand { get; set; }
        public ICommand ReplyCommand { get; set; }
        public ICommand RefuseCommand { get; set; }
        public ICommand CancelCommand { get; set; }
        public ICommand BatchRefusedCommand { get; set; }
        public ICommand BatchConfirmCommand { get; set; }

        public ICommand CopyOrderCommand { get; set; }

        public ICommand ExpandCommand { get; set; }
        //Modi:cyc Date:2023-03-24 Func:增加倒计时
        System.Threading.Timer descTimer;

        private object Updatelock = new object();

        #region IRS
        /// <summary>
        /// 是否显示 IRS模式, IRS时不可见 现券时可见
        /// </summary>
        private Visibility isShowIRS = Visibility.Visible;
        public Visibility IsShowIRS
        {
            get { return isShowIRS; }
            set
            {
                isShowIRS = value;
                this.OnPropertyChanged(nameof(IsShowIRS));
            }
        }
        #endregion

        #endregion

        #region 构造函数
        public DirectionalReturnPriceViewModel()
        {
            ReplyCommand = new DelegateCommand(new Action<object>(OnReplyCommand));
            RefuseCommand = new DelegateCommand(new Action<object>(OnRefuseCommand));
            CancelCommand = new ComRelayCommand(CancelClick);
            BatchRefusedCommand = new ComRelayCommand(OnBatchRefusedCommand);
            BatchConfirmCommand = new ComRelayCommand(OnBatchReplyCommand);
            CloseCommand = new ComRelayCommand(CloseClick);
            AddAmountCommand = new DelegateCommand(new Action<object>(OnAddAmountCommand));
            DownAmountCommand = new DelegateCommand(new Action<object>(OnDownAmountCommand));
            CopyOrderCommand = new DelegateCommand(new Action<object>(OnCopyOrderCommand));
            ExpandCommand = new ComRelayCommand(OnExpandCommand);
            this.Mv.Source = GridDatas;
            this.Mv.IsLiveFilteringRequested = true;
            this.Mv.IsLiveSortingRequested = true;
        }


        /// <summary>
        /// 初始化界面 订阅系统参数消息数据
        /// </summary>
        /// <param name="ownWindow"></param>
        /// <param name="msg"></param>
        public void Init(Window ownWindow, MsgPo msg)
        {
            if (_msgTask == null)
                _msgTask = new AsyncThreadQueue<List<TTbookSystemMessage>>(Process);
            else
                _msgTask.Clear();

            _myWindow = ownWindow;

            InitUI(msg, false);

            StartTimeTick();

            AdminHandler.Instance.Init(OnAdminMsg);
        }

        #region 参数消息数据处理
        private void InitUI(MsgPo msg, bool isRealtime)
        {
            lock (Updatelock)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    _currentMsg = msg;
                    List<DRPValue> priceValues = null;

                    if (msg != null)
                    {
                        if (!string.IsNullOrEmpty(msg.Detail))
                        {
                            drd = Newtonsoft.Json.JsonConvert.DeserializeObject<DRPDetail>(msg.Detail);
                            priceValues = drd.iocOrderDqList;
                            OrderId = drd.order.orderId;

                            DRPDetail sourceDRPDetail = STC.Common.Handler.Drp.DrpHandler.Instance.GetDRPDetail(OrderId);

                            foreach (DRPValue pv in priceValues)
                            {
                                DRPListModel dg = DRPTools.PriceValueToGridData(pv);
                                if (sourceDRPDetail != null)
                                {
                                    DRPValue dv = sourceDRPDetail.iocOrderDqList.FirstOrDefault(o => o.orderId == dg.OrderId);
                                    if (dv != null)
                                    {
                                        dg.ValidUntilTime = dv.ValidUntilTime;
                                    }
                                    else
                                    {
                                        if (!string.IsNullOrEmpty(dg.IocTimeOut))
                                        {
                                            int icotimeout = 0;
                                            int.TryParse(dg.IocTimeOut.ToString(), out icotimeout);
                                            if (string.IsNullOrEmpty(dg.ValidUntilTime))
                                                dg.ValidUntilTime = DateTimeEx.GetNow().AddSeconds(icotimeout).ToString("yyyy-MM-dd HH:mm:ss.fff");
                                        }
                                    }
                                }

                                dg.TransactTime = pv.transactTime;
                                dg.Symbol = drd.order.symbol;
                                dg.OriAmount = (drd.order.qty - drd.order.cumQty).ToString();
                                dg.OriPrice = CommonUtils.ConvertToDecimal(drd.order.price.ToString()).ToString("F4");
                                dg.OriSettle = drd.order.settlType;


                                DRPListModel rf = this.GridDatas.FirstOrDefault(o => o.Id == dg.Id);
                                if (rf != null)
                                {
                                    rf.Amount = dg.Amount;
                                    rf.FirmType = dg.FirmType;
                                    rf.Id = dg.Id;
                                    rf.Price = dg.Price;
                                    rf.Source = dg.Source;
                                    rf.PriceBrush = dg.PriceBrush;
                                    rf.EnabledBtn = dg.EnabledBtn;
                                    rf.TransactTime = dg.TransactTime;
                                    rf.PointPriceBrush = dg.PointPriceBrush;
                                    rf.PointPriceBackGround = dg.PointPriceBackGround;
                                    rf.OrderStatus = dg.OrderStatus;
                                    //rf.RemainSeconds = dg.RemainSeconds;
                                    rf.ValidUntilTime = dg.ValidUntilTime;
                                    rf.Side = dg.Side;
                                    rf.PointAmount = dg.PointAmount;
                                    rf.PointPrice = dg.PointPrice;
                                    rf.Settle = dg.Settle;
                                    if (dg.OrderStatus != "New")
                                        rf.IsChecked = dg.IsChecked;

                                    rf.OriAmount = (drd.order.qty - drd.order.cumQty).ToString();
                                    rf.OriPrice = CommonUtils.ConvertToDecimal(drd.order.price.ToString()).ToString("F4");
                                    rf.OriSettle = drd.order.settlType;
                                    rf.EnabledEditPointAmountBtn = dg.EnabledEditPointAmountBtn;
                                    rf.MsgCode = dg.MsgCode;
                                    rf.CMsgCode = dg.CMsgCode;
                                    rf.IsIRS = dg.IsIRS;
                                }
                                else
                                {
                                    this.GridDatas.Add(dg);
                                }
                            }

                            LeavesQtyTotal = drd.order.qty - drd.order.cumQty;
                            ShowLeavesQtyTotal = LeavesQtyTotal.ToString() == "0" ? "--" : $"{LeavesQtyTotal.ToString()}";
                            Wholeflag =  drd.order.wholeflag ?? "0";

                            SetFirstBestPriceSelected();

                            ComputeAmount();

                        // 计算活着的笔数
                        List<DRPValue> Tmp = new List<DRPValue>(drd.iocOrderDqList.Where(p => p.orderStatus == "New"));
                            IocCount = $"收到回价({Tmp.Count}笔)";
                            IocReccCount = $"已收到定向回价({drd.iocOrderDqList.Count.ToString()}笔), 请确认: ";
                        }

                        // 修改标题显示内容
                        CurrentMsgId = msg.Id;
                        
                        if (msg.Content != null)
                        {
                            ContentStart = drd.order.symbol;

                            if (drd.order.symbolType.ToUpper() == "BCO")
                            {
                                ContentStart = drd.order.securityid;
                            }


                            FirmType = drd.order.firmFlag == "1" ? $"(Firm)" : $"(NonFirm)";
                            //1 BID BUY   2 Ofr SELL
                            ShowSide = (drd.order.side.ToUpper().Equals("BUY") || drd.order.side.Equals("1")) ? "1" : "2";

                            string tmprice = "";
                            if (drd.order.price.ToString() == "-1" || drd.order.price.ToString() == "999")
                            {
                                tmprice = "--";
                            }
                            else
                            {
                                tmprice = CommonUtils.ConvertToDecimal(drd.order.price.ToString()).ToString("F4");
                            }

                            IsShowIRS = drd.order.marketIndicator.ToString().Trim()=="2" ? Visibility.Collapsed : Visibility.Visible;

                            ShowPrice = tmprice == "--" ? "--" : CommonUtils.FormatNum(tmprice);
                            // 母单撤单时间
                            OrderValidUntiTime = drd.order.validuntitime;
                            // 设置排序 IRS的排序是反向的
                            if (drd.order.marketIndicator.ToString().Trim() != "2")
                                SetSortDescriptions(ShowSide);
                            else
                                SetSortDescriptions(ShowSide == "1" ? "2" : "1");
                        }
                    }
                });

            }
        }

        /// <summary>
        /// 接收系统参数消息数据
        /// </summary>
        /// <param name="sysMsg"></param>
        private void OnAdminMsg(List<TTbookSystemMessage> sysMsg)
        {
            _msgTask.Enqueue(sysMsg);
        }

        /// <summary>
        /// 处理系统参数消息数据
        /// </summary>
        /// <param name="sysMsg"></param>
        private void Process(List<TTbookSystemMessage> sysMsg)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                lock (_lockObj)
                {
                    if (sysMsg != null && sysMsg.Count > 0 && _currentMsg != null && CurrentMsgId.Equals(sysMsg[0].id))
                    {
                        IsSetCheckedFromRefreshData = true;
                        IsAllChecked = false;
                        foreach (TTbookSystemMessage msg in sysMsg)
                        {
                            if (msg != null)
                            {
                                // 加入删除逻辑
                                string isdel = msg.delflag ?? "0";
                                if (isdel == "1")
                                {
                                    if (msg.id == CurrentMsgId)
                                    {
                                        CloseClick();
                                        break;
                                    }
                                }
                            }

                            if (msg.content != null)
                            {
                                TMessageContent detail = MsgHelper.Deserialize<TMessageContent>(msg.content, Ats.Foundation.Message.Serializer.MsgSerializerType.Json);
                                if (detail.contentType.Equals("DQ") || detail.contentType.Equals("DQB"))
                                {
                                    InitUI(DRPTools.SystemMsgToShowMsg(msg, _myWindow), true);
                                }
                            }
                        }
                    }

                    //成交回报,收到成交回报更新剩余量
                    if (sysMsg != null && sysMsg.Count > 0)
                    {
                        foreach (TTbookSystemMessage msg in sysMsg)
                        {
                            if (msg.content != null)
                            {
                                TMessageContent detail = MsgHelper.Deserialize<TMessageContent>(msg.content, Ats.Foundation.Message.Serializer.MsgSerializerType.Json);
                                if (detail.contentType.Equals(AdminConsts.Admin_ER))
                                {
                                    ExecutionReport er = Newtonsoft.Json.JsonConvert.DeserializeObject<ExecutionReport>(detail.detail);
                                    if (CurrentMsgId.Equals($"DQLimit_{er.QuoteID}"))
                                    {
                                        Application.Current.Dispatcher.Invoke(() =>
                                        {
                                            //只更新界面的成交量
                                            decimal _LeavesQty = decimal.Parse(er.LeavesQty);
                                            string _ShowLeavesQtyTotal = _LeavesQty.ToString() == "0" ? "--" : _LeavesQty.ToString();
                                            LeavesQtyTotal = _LeavesQty;
                                            ShowLeavesQtyTotal = _ShowLeavesQtyTotal;
                                        });
                                    }
                                }
                            }
                        }
                    }    
                }
            }));
        }

        public void SetSortDescriptions(string side)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                //排序 ShowSide 1 Bid Buy asc  2 Ofr  desc
                this.Mv.SortDescriptions.Add(new SortDescription("EnabledBtn", ListSortDirection.Descending));
                this.Mv.SortDescriptions.Add(new SortDescription("Price", side == "1" ? ListSortDirection.Descending : ListSortDirection.Ascending));
                this.Mv.SortDescriptions.Add(new SortDescription("TransactTime", ListSortDirection.Descending));
                this.Mv.View.Refresh();
            });
        }

       /// <summary>
        /// 自动选择最优价
        /// </summary>
        /// <param name="dr"></param>
        public void SetFirstBestPriceSelected()
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(async () =>
            {
                foreach (DRPListModel dg in this.Mv.View)
                {
                    if (dg.EnabledBtn)
                    {
                        this.SelectDrp = dg;
                        break;
                    }
                }

                await System.Windows.Threading.Dispatcher.Yield(System.Windows.Threading.DispatcherPriority.Render);
            }));
        }

        #endregion


        #region UI
        private void OnCheckAll()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (!IsSetCheckedFromRefreshData && !IsSetFromAutoTotal)
                {
                    foreach (DRPListModel gridData in GridDatas)
                    {
                        if (gridData.EnabledBtn)
                        {
                            gridData.IsChecked = IsAllChecked;
                        }
                    }
                }
                else
                {
                    IsSetCheckedFromRefreshData = false;
                }

                if (!IsSetFromAutoTotal)
                {
                    ComputeAmount();
                }
                else
                {
                    IsSetFromAutoTotal = false;
                }
            });
        }


        /// <summary>
        /// 计算汇总值
        /// </summary>
        public void ComputeAmount()
        {
            string tipTxt = string.Empty;
            decimal oldTotal = 0;
            decimal pointTotal = 0;
            int index = 0;

            foreach (DRPListModel gridData in GridDatas)
            {
                if (gridData.EnabledBtn && gridData.IsChecked)
                {
                    index++;
                    decimal d = 0;
                    if (decimal.TryParse(gridData.Amount, out d))
                        oldTotal += d;
                    if (decimal.TryParse(gridData.PointAmount, out d))
                        pointTotal += d;
                }
            }

            if (drd != null && drd.order != null)
                oldTotal = drd.order.qty;

            if (pointTotal > oldTotal)
            {
                this.WarningColor = "red";
            }
            else
            {
                this.WarningColor = "white";
            }

            TotalAmout = oldTotal;
            PointTotal = pointTotal;
            ShowTotal = TotalAmout.ToString() == "0" ? "--" : $"{TotalAmout.ToString()}万";

            CheckAmountErrorVisibility = Visibility.Visible;
            ErrorVisibility = Visibility.Collapsed;

            //计算选中的个数是否和有效的个数相同，如果不同则设置 全选功能
            int IsCheckedCount = GridDatas.Count(p => p.IsChecked);
            int IsEnableCount = GridDatas.Count(p => p.EnabledBtn);
            if (IsCheckedCount == IsEnableCount && IsCheckedCount>0)
            {
                IsSetFromAutoTotal = true;
                this.IsAllChecked = true;
            }
            else
            {
                IsSetFromAutoTotal = true;
                this.IsAllChecked = false;
            }
        }

        public void Clear()
        {
            AdminHandler.Instance.UnSubscribeOrder(OnAdminMsg);
            _msgTask.Clear();
            _msgTask.Dispose();
        }

        /// <summary>
        /// 取消
        /// </summary>
        private void CancelClick()
        {
            AdminHandler.Instance.UnSubscribeOrder(OnAdminMsg);
            _msgTask.Clear();
            _msgTask.Dispose();
            _myWindow.Close();
        }

       
        #endregion


        #region Command Proceduce

        /// <summary>
        /// 取消并关闭窗口
        /// </summary>
        private void CloseClick()
        {
            AdminHandler.Instance.UnSubscribeOrder(OnAdminMsg);
            _msgTask.Clear();
            _msgTask.Dispose();
            _myWindow.Close();

        }


        /// <summary>
        /// 增加报量
        /// </summary>
        public void OnAddAmountCommand(object inparam)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(async () =>
            {
                DRPListModel rf = GridDatas.FirstOrDefault(o => o.Id == (string)inparam);
                if (rf != null)
                {
                    decimal d = 0;
                    if (decimal.TryParse(rf.PointAmount, out d))
                    {
                        if ((d + 1000) < 10000000)
                            rf.PointAmount = (d + 1000).ToString();
                        else
                            rf.PointAmount = "10000000";

                        ComputeAmount();
                    }
                    else
                    {
                        rf.PointAmount = "1000";
                    }

                    ComputeAmount();
                }

                await System.Windows.Threading.Dispatcher.Yield(System.Windows.Threading.DispatcherPriority.Render);
            }));
        }

        /// <summary>
        /// 减少报量
        /// </summary>
        public void OnDownAmountCommand(object inparam)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(async () =>
            {
                DRPListModel rf = GridDatas.FirstOrDefault(o => o.Id == (string)inparam);
                if (rf != null)
                {
                    decimal d = 0;
                    if (decimal.TryParse(rf.PointAmount, out d))
                    {
                        if ((d - 1000) >= 0)
                        {
                            rf.PointAmount = (d - 1000).ToString();
                        }
                    }
                    else
                    {
                        rf.PointAmount = "1000";
                    }

                    ComputeAmount();
                }

                await System.Windows.Threading.Dispatcher.Yield(System.Windows.Threading.DispatcherPriority.Render);
            }));
        }

        /// <summary>
        /// 回应(成交或议价)
        /// </summary>
        /// <param name="inparam"></param>
        private void OnReplyCommand(object inparam)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(async () =>
            {
                DRPListModel rf = GridDatas.FirstOrDefault(o => o.Id == (string)inparam);
                if (rf != null)
                {
                    string errorMsg = string.Empty;
                    bool NeedPrompt = false;
                    string info1 = "1";

                    decimal d = 0;
                    if (IsShowIRS == Visibility.Collapsed)
                    {
                        // IRS
                        if (decimal.TryParse(rf.PointAmount, out d) && d % 100 != 0)
                        {
                            CustomMessageBox.Error(string.Format("确认量必须是100的整数倍，当前输入值：{0}", rf.PointAmount));
                            return;
                        }
                    }
                    else if (decimal.TryParse(rf.PointAmount, out d) && d % 100 != 0)
                    {
                        ErrorTxt = "确认量必须是100的整数倍";
                        CustomMessageBox.Warning($"{ErrorTxt}", "定向回价");
                        return;
                    }

                    //if (TotalAmout > 0)
                    //{
                    if (decimal.TryParse(rf.PointAmount, out d))
                    {
                        if (d > decimal.Parse(rf.Amount))
                        {
                            string ErrorTxt = "确认量需要小于等于回价总量";
                            CustomMessageBox.Warning($"{ErrorTxt}", "定向回价");
                            return;
                        }
                        if (TotalAmout > 0)
                        {
                            if (d > LeavesQtyTotal)
                            {
                                string ErrorTxt = "确认量需要小于等于报价总量";
                                CustomMessageBox.Warning($"{ErrorTxt}", "定向回价");
                                return;
                            }
                        }
                    }
                    //}

                    if (d > 10000000 || d < 1)
                    {
                        string ErrorTxt = "确认量输入范围 [1，10000000] ";
                        CustomMessageBox.Warning($"{ErrorTxt}", "定向回价");
                        return;
                    }
                    if (decimal.TryParse(rf.PointPrice, out d))
                    {
                        if (d > 9999 || d < -100)
                        {
                            string ErrorTxt = "价格输入范围 [-100，9999]";
                            CustomMessageBox.Warning($"{ErrorTxt}", "定向回价");
                            return;
                        }
                    }

                    if (CommonUtils.ConvertToDecimal(rf.Price) == CommonUtils.ConvertToDecimal(rf.PointPrice))
                    {
                        info1 = "2";
                    }
                    else
                    {
                        info1 = "1";
                    }

                    string dm = info1 == "2" ? "成交意向发送" : "议价意向发送";

                    ThreadPoolHelper.InvokeThreadPool((o) =>
                    {
                        PResult result = null;
                        if (info1 == "1")
                        {
                            //1-议价
                            Common.Model.Fix.Message.OrderCancelRequest rs = DRPTools.GetCancelOrder(rf);
                            rs.Info1 = info1;
                            result = InnerHandler.Instance.DRPCancelOrder(rs);
                        }
                        else
                        {
                            //2-成交
                            NewOrderSingle rs = DRPTools.GetNewOrderSingle(rf); //报价类型（1-议价，2-成交，3-拒绝）
                            rs.Info1 = info1;
                            result = InnerHandler.Instance.DRPOrder(rs);
                        }

                        if (result != null && result.Code == "0000")
                        {
                            NeedPrompt = false;
                        }
                        else
                        {
                            NeedPrompt = true;
                            if (result != null && !string.IsNullOrEmpty(result.Msg))
                            {
                                errorMsg = result.Msg;
                            }
                            else
                            {
                                errorMsg = "服务器接收数据失败！";
                            }
                        }

                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            if (NeedPrompt)
                            {
                                CustomMessageBox.Warning($"定向回价失败！\r\n原因:{errorMsg}", "定向回价");
                                if (result == null)
                                {
                                    return;
                                }

                                if (result.Code != "0000")
                                {
                                    return;
                                }
                            }

                            //CommonUtils.Prompt2Second("定向回价", $"{dm}成功.");

                        });
                    });
                }

                await System.Windows.Threading.Dispatcher.Yield(System.Windows.Threading.DispatcherPriority.Render);
            }));
        }

        /// <summary>
        /// 拒绝
        /// </summary>
        /// <param name="inparam"></param>
        private void OnRefuseCommand(object inparam)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(async () =>
            {
                string errorMsg = string.Empty;
                bool NeedPrompt = false;
                DRPListModel  rf = GridDatas.FirstOrDefault(o => o.Id == (string)inparam);
                if (rf != null)
                {
                    ThreadPoolHelper.InvokeThreadPool((o) =>
                    {
                        Common.Model.Fix.Message.OrderCancelRequest rs = DRPTools.GetRejectOrder(rf);
                        rs.Info1 = "3"; //报价类型（1-议价，2-成交，3-拒绝）
                        PResult result = InnerHandler.Instance.DRPCancelOrder(rs);
                        if (result != null && result.Code == "0000")
                        {
                            NeedPrompt = false;
                        }
                        else
                        {
                            NeedPrompt = true;
                            if (result != null && !string.IsNullOrEmpty(result.Msg))
                            {
                                errorMsg = result.Msg;
                            }
                            else
                            {
                                errorMsg = "服务器接收数据失败！";
                            }
                        }

                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            if (NeedPrompt)
                            {
                                CustomMessageBox.Error( $"定向回价失败！\r\n原因:{errorMsg}", "定向回价");

                                if (result == null)
                                {
                                    return;
                                }

                                if (result.Code != "0000")
                                {
                                    return;
                                }
                            }

                            //CommonUtils.Prompt2Second("定向回价", $"拒绝意向发送成功.", _myWindow);
                        });
                    });

                }
                await System.Windows.Threading.Dispatcher.Yield(System.Windows.Threading.DispatcherPriority.Render);
            }));
        }

        /// <summary>
        /// 批量拒绝
        /// </summary>
        private async void OnBatchRefusedCommand()
        {
            List<DRPListModel> tmp = new List<DRPListModel>(GridDatas.Where(p => p.IsChecked));
            if (tmp.Count > 0)
            {
                bool result = result = CustomMessageBox.Question("是否批量忽略定向回价订单？", "定向回价");
                if (result == true)
                {
                    StringBuilder error = null;
                    error = await SuRefusedAllOrders(tmp);
                    if (error != null && error.Length > 0)
                    {
                        CustomMessageBox.Error($"批量忽略订单失败, 原因:\r\n{error.ToString()}");
                    }

                    CloseClick();
                }
            }
            else
            {
                if (PointTotal == 0)
                {
                    CustomMessageBox.Warning("未选择订单或确认量为0", "定向回价");
                    return;
                }
            }
        }

        private async Task<StringBuilder> SuRefusedAllOrders(List<DRPListModel> tmp)
        {
            StringBuilder error = new StringBuilder();

            await Task.Run(() =>
            {
                List<Model.Fix.Message.OrderCancelRequest> cancelOrdersList = new List<Model.Fix.Message.OrderCancelRequest>();
                foreach (DRPListModel rf in tmp)
                {
                    Model.Fix.Message.OrderCancelRequest rs = DRPTools.GetRejectOrder(rf);
                    rs.Info1 = "3"; //报价类型（1-议价，2-成交，3-拒绝）
                    cancelOrdersList.Add(rs);
                }

                if (cancelOrdersList != null)
                {
                    int count = cancelOrdersList.Count;
                    Task[] lstTask = new Task[count];

                    ConcurrentDictionary<string, PResult> dic = new ConcurrentDictionary<string, PResult>();
                    for (int i = 0; i < count; i++)
                    {
                        Model.Fix.Message.OrderCancelRequest cancelPo = cancelOrdersList[i];
                        lstTask[i] = new Task(() =>
                        {
                            PResult result = InnerHandler.Instance.DRPCancelOrder(cancelPo);
                            if (result != null && !string.IsNullOrEmpty(result.Msg))
                            {
                                dic[cancelPo.OrderID] = result;
                            }
                            else
                            {
                                PResult res = new PResult();
                                res.Code = "-1";
                                res.Msg = "服务器接收数据失败！";
                                dic[cancelPo.OrderID] = res;
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
                                error.AppendLine($"订单号 {orderID}:{reply.Msg}");
                            }
                        }
                    }

                }
            });

            return error;
        }

        /// <summary>
        /// 批量议价或成交
        /// </summary>
        private async void OnBatchReplyCommand()
        {
            ErrorVisibility = Visibility.Collapsed;
            CheckAmountErrorVisibility = Visibility.Visible;
            ErrorTxt = string.Empty;

            if (!CheckParam())
            {
                ErrorVisibility = Visibility.Visible;
                CheckAmountErrorVisibility = Visibility.Collapsed;
                ErrorTxt = "确认量必须是100的整数倍";
                CustomMessageBox.Warning($"{ErrorTxt}", "定向回价");
                return;
            }

            bool isAmountError = false;
            foreach (DRPListModel gridData in GridDatas)
            {
                if (gridData.EnabledBtn && gridData.IsChecked)
                {
                    if (!string.IsNullOrEmpty(gridData.PointAmount))
                    {
                        decimal d = 0;
                        if (decimal.TryParse(gridData.PointAmount, out d))
                        {
                            if (d > decimal.Parse(gridData.Amount))
                            {
                                ErrorVisibility = Visibility.Visible;
                                CheckAmountErrorVisibility = Visibility.Collapsed;
                                ErrorTxt = $"{gridData.UserName} 确认量需要小于等于回价总量";
                                CustomMessageBox.Warning($"{ErrorTxt}", "定向回价");
                                isAmountError = true;
                                break;
                            }
                        }
                    }
                }
            }

            if (isAmountError)
            {
                return;
            }

            if (PointTotal == 0)
            {
                string ErrorTxt = "未选择订单或确认量为0";
                CustomMessageBox.Warning($"{ErrorTxt}", "定向回价");
                return;
            }

            if (TotalAmout > 0)
            {
                if (PointTotal > LeavesQtyTotal)
                {
                    CustomMessageBox.Warning($"点价总量 {PointTotal} > 待确认量{LeavesQtyTotal} !");
                    return;
                }
            }

            List<DRPListModel> tmp = new List<DRPListModel>(GridDatas.Where(p => p.IsChecked));
            if (tmp.Count > 0)
            {
                bool result = result = CustomMessageBox.Question("是否批量确认定向回价订单？", "定向回价");
                if (result == true)
                {
                    StringBuilder error = null;
                    error = await SubBatchReplyOrders(tmp);
                    if (error != null && error.Length > 0)
                    {
                        CustomMessageBox.Error($"批量确认订单失败, 原因:\r\n{error.ToString()}");
                    }

                    CloseClick();
                }
            }
            else
            {
                if (PointTotal == 0)
                {
                    CustomMessageBox.Warning("没有选择订单", "定向回价");
                    return;
                }
            }


        }


        /// <summary>
        /// 复制报价
        /// </summary>
        /// <param name="inparam"></param>
    
        
        
        public void OnCopyOrderCommand(object inparam)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(async () =>
            {
                DRPListModel rf = GridDatas.FirstOrDefault(o => o.Id == (string)inparam);
                if (rf != null)
                {
                    string side = rf.Side == "1" ? "TKN" : "GVN";
                    string txt = string.Empty;
                    if (!string.IsNullOrEmpty(rf.RemainSeconds) && rf.RemainSeconds != "--" && rf.RemainSeconds != "0")
                            txt = $"{this.ContentStart} {side} {rf.Price} {rf.Amount}万 是否OK？({rf.RemainSeconds}s 内回复有效)";
                    else
                        txt = $"{this.ContentStart} {side} {rf.Price} {rf.Amount}万 是否OK？";

                    ClipboardHelper.CopyToClipboard(txt);
                    CommonUtils.Prompt2Second("提示", "已复制");
                }

                await System.Windows.Threading.Dispatcher.Yield(System.Windows.Threading.DispatcherPriority.Render);
            }));
        }


        private async Task<StringBuilder> SubBatchReplyOrders(List<DRPListModel> tmp)
        {
            StringBuilder error = new StringBuilder();

            await Task.Run(() =>
            {
                List<object> replyOrdersList = new List<object>();
                string info1 = "1";
                foreach (DRPListModel rf in tmp)
                {
                    if (CommonUtils.ConvertToDecimal(rf.Price) == CommonUtils.ConvertToDecimal(rf.PointPrice))
                    {
                        info1 = "2";
                    }
                    else
                    {
                        info1 = "1";
                    }

                    if (info1 == "1")
                    {
                        //1-议价
                        Model.Fix.Message.OrderCancelRequest rs = DRPTools.GetCancelOrder(rf);
                        rs.Info1 = info1;
                        replyOrdersList.Add(rs);
                    }
                    else
                    {
                        //2-成交
                        NewOrderSingle rs2 = DRPTools.GetNewOrderSingle(rf); //报价类型（1-议价，2-成交，3-拒绝）
                        rs2.Info1 = info1;
                        replyOrdersList.Add(rs2);
                    }
                }

                if (replyOrdersList != null)
                {
                    int count = replyOrdersList.Count;
                    Task[] lstTask = new Task[count];

                    ConcurrentDictionary<string, PResult> dic = new ConcurrentDictionary<string, PResult>();
                    for (int i = 0; i < count; i++)
                    {
                        //1-议价，2-成交，3-拒绝
                        //1. result = InnerHandler.Instance.DRPCancelOrder(rs);    //Model.Fix.Message.OrderCancelRequest
                        //2. result = InnerHandler.Instance.DRPOrder(rs);     //NewOrderSingle
                        object srcobject = replyOrdersList[i];
                        lstTask[i] = new Task(() =>
                        {
                            if (srcobject.GetType() == typeof(Model.Fix.Message.OrderCancelRequest))
                            {
                                PResult result = InnerHandler.Instance.DRPCancelOrder((Model.Fix.Message.OrderCancelRequest)srcobject);
                                if (result != null && !string.IsNullOrEmpty(result.Msg))
                                {
                                    dic[((Model.Fix.Message.OrderCancelRequest)srcobject).OrderID] = result;
                                }
                                else
                                {
                                    PResult res = new PResult();
                                    res.Code = "-1";
                                    res.Msg = "服务器接收数据失败！";
                                    dic[((Model.Fix.Message.OrderCancelRequest)srcobject).OrderID] = res;
                                }
                            }
                            else if (srcobject.GetType() == typeof(NewOrderSingle))
                            {
                                PResult result = InnerHandler.Instance.DRPOrder((NewOrderSingle)srcobject);
                                if (result != null && !string.IsNullOrEmpty(result.Msg))
                                {
                                    dic[((NewOrderSingle)srcobject).QuoteID] = result;
                                }
                                else
                                {
                                    PResult res = new PResult();
                                    res.Code = "-1";
                                    res.Msg = "服务器接收数据失败！";
                                    dic[((NewOrderSingle)srcobject).QuoteID] = res;
                                }
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
                                error.AppendLine($"订单号 {orderID}:{reply.Msg}");
                            }
                        }
                    }

                }
            });

            return error;
        }


        #endregion

        #endregion

        #region  Proc
        private bool CheckParam()
        {
            bool result = true;
            foreach (DRPListModel gridData in GridDatas)
            {
                if (gridData.EnabledBtn && gridData.IsChecked)
                {
                    if (!string.IsNullOrEmpty(gridData.PointAmount))
                    {
                        decimal d = 0;
                        if (decimal.TryParse(gridData.PointAmount, out d) && d % 100 != 0)
                        {
                            result = false;
                            break;
                        }
                    }
                }
            }
            return result;
        }

        private bool CheckVenuePermission(string venue, string security)
        {
            //if (!string.IsNullOrEmpty(venue) && ((venue.ToUpper() == "MANUAL") || (venue.ToUpper() == "人工") || (venue.ToUpper() == "内部")))
            //    return true;

            string goodsType = CommonUtils.SetGoodsShortname(security);

            List<Venue> venues = SystemParamHandler.Instance.GetListVenueByFilter(goodsType, string.Empty);

            bool result = false;
            foreach (Venue ve in venues)
            {
                if (ve.venue.ToUpper() == venue.ToUpper())
                {
                    result = true;

                    break;
                }
            }

            return result;
        }

        #endregion

        #region 定向报价倒计时
        private void StartTimeTick()
        {
            this.descTimer = new System.Threading.Timer(new TimerCallback(DescTimer_Tick), null, 1000, 1000);
        }

        /// <summary>
        /// 更新剩余时间
        /// </summary>

        private void DescTimer_Tick(object state)
        {
            try
            {
                UpdatMyOrdersTimeRemains();
            }
            catch (Exception ex)
            {

            }
        }


        public void UpdatMyOrdersTimeRemains()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                lock (Updatelock)
                {
                    int res = 0;

                    foreach (DRPListModel myOrder in this.GridDatas)
                    {
                        if (!string.IsNullOrEmpty(myOrder.IocTimeOut) && myOrder.EnabledBtn)
                        {
                            DateTime transactTime = CommonUtils.ConvertToDateTime(myOrder.ValidUntilTime.ToString());

                            TimeSpan diffTime = transactTime - DateTimeEx.ServerTimeNow;
                            res = Convert.ToInt32(diffTime.TotalSeconds);

                            myOrder.RemainSeconds = res < 0 ? "0" : res.ToString();
                        }
                        else
                        {
                            myOrder.RemainSeconds = "--";
                            myOrder.EnabledBtn = false;
                        }
                    }

                    //DateTime ordertransactTime = CommonUtils.ConvertToDateTime(OrderValidUntiTime.ToString());
                    //TimeSpan orderdiffTime = ordertransactTime - DateTimeEx.ServerTime;
                    //res = Convert.ToInt32(orderdiffTime.TotalSeconds);
                    //this.OrderRemainSeconds = res < 0 ? "--" : $"{DRPTools.FormatDrpTime(res)}";
                }
            });
        }


        /// <summary>
        /// 打开或关闭回价跟踪窗口 (暂时不用)
        /// </summary>
        private void OnExpandCommand()
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                if (_myWindow != null)
                {
                    Button btn_Expand = (Button)_myWindow.FindName("btn_Expand");
                    Border Border_Left1 = (Border)_myWindow.FindName("Border_Left1");
                    Border Border_Left2 = (Border)_myWindow.FindName("Border_Left2");
                    Border Border_Left3 = (Border)_myWindow.FindName("Border_Left3");
                    Border Border_Left4 = (Border)_myWindow.FindName("Border_Left4");
                    Border Border_Left5 = (Border)_myWindow.FindName("Border_Left5");
                    Border Border_Left6 = (Border)_myWindow.FindName("Border_Left6");
                    Border Border_Right1 = (Border)_myWindow.FindName("Border_Right1");

                    DRPOrderLogView drpOrderLogView = (DRPOrderLogView)_myWindow.FindName("drpOrderLogView");
                    System.Windows.Shapes.Line LineSplit = (System.Windows.Shapes.Line)_myWindow.FindName("LineSplit");

                    if (btn_Expand.Tag.ToString() == "Collapsed")
                    {
                        btn_Expand.Content = "<<报价追踪";
                        btn_Expand.Tag = "Visible";
                        _myWindow.Width = 940;
                        Border_Left1.BorderThickness = new Thickness(2, 2, 1, 0);
                        Border_Left2.BorderThickness = new Thickness(2, 0, 1, 0);
                        Border_Left3.BorderThickness = new Thickness(2, 0, 1, 0);
                        Border_Left4.BorderThickness = new Thickness(2, 0, 1, 0);
                        Border_Left5.BorderThickness = new Thickness(2, 0, 1, 0);
                        Border_Left6.BorderThickness = new Thickness(2, 0, 1, 2);
                        Border_Right1.BorderThickness = new Thickness(0, 2, 2, 2);
                        Border_Left6.CornerRadius = new CornerRadius(0, 0, 2, 2);
                        if (drpOrderLogView != null)
                        {
                            drpOrderLogView.QueryLog(OrderId.Replace("DQLimit_",""), "1");
                        }

                    }
                    else
                    {
                        btn_Expand.Content = ">>报价追踪";
                        btn_Expand.Tag = "Collapsed";
                        _myWindow.Width = 640;

                        Border_Left1.BorderThickness = new Thickness(2, 2, 2, 0);
                        Border_Left2.BorderThickness = new Thickness(2, 0, 2, 0);
                        Border_Left3.BorderThickness = new Thickness(2, 0, 2, 0);
                        Border_Left4.BorderThickness = new Thickness(2, 0, 2, 0);
                        Border_Left5.BorderThickness = new Thickness(2, 0, 2, 0);
                        Border_Left6.BorderThickness = new Thickness(2, 0, 2, 2);
                        Border_Left6.CornerRadius = new CornerRadius(0, 0, 4, 4);
                        Border_Right1.BorderThickness = new Thickness(2, 0, 2, 2);
                    }
                }
            }));
        }

        #endregion

        /// <summary>
        /// 报价或议价
        /// </summary>
        /// <param name="inparam"></param>
        public void SetClipboard(object inparam)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                DRPListModel rf = GridDatas.FirstOrDefault(o => o.Id == (string)inparam);
                if (rf != null)
                {
                    string side = rf.Side == "1" ? "TKN" : "GVN";
                    string txt = string.Empty;

                    if (!string.IsNullOrEmpty(rf.IocTimeOut))
                        txt = $"{this.ContentStart} {side} {rf.PointPrice} {rf.PointAmount}万 是否OK？({rf.IocTimeOut}s 内回复有效)";
                    else
                        txt = $"{this.ContentStart} {side} {rf.PointPrice} {rf.PointAmount}万 是否OK？";

                    ClipboardHelper.CopyToClipboard(txt);
                    CommonUtils.Prompt2Second("提示", "已复制");
                }
            });
        }
    }
}
