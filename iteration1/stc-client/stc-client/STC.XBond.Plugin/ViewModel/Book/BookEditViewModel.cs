using com.gateway.log;
using STC.Common.Handler.BookManagement;
using STC.Common.Model.Admin;
using STC.Common.Model.BookManagement;
using STC.Common.Model.Fix.Message;
using STC.Common.Utils;
using STC.XBond.Plugin.Utils;
using STC.XBond.Plugin.View.Book;
using STC.XBond.Plugin.View.Component;
using STC.XBond.Plugin.ViewModel.Data;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Reflection;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
namespace STC.XBond.Plugin.ViewModel
{
    /// <summary>
    /// BookEditView的数据内容
    /// </summary>
    public class BookEditViewModel : ViewModelBase
    {

        // 买入边框色  #ED6D00
        private SolidColorBrush _sellBorderBrush = new SolidColorBrush(Color.FromArgb(255, 237, 109, 0));

        // 买入标题颜色 0:#ED6D00 1:#141414
        private LinearGradientBrush _sellBGBrush = new LinearGradientBrush(Color.FromArgb(255, 237, 109, 0),
                Color.FromArgb(255, 20, 20, 20),
                new Point(0.5, 0),
                new Point(0.5, 1));
        // 买入内边框线 #ED6D00
        private SolidColorBrush _sellInnerBorderBrush = new SolidColorBrush(Color.FromArgb(255, 237, 109, 0));

        // 卖出内边框色 #009AFF
        private SolidColorBrush _buyBorderBrush = new SolidColorBrush(Color.FromArgb(255, 0, 154, 255));

        // 卖出边框颜色 0:#095789 1:#141414
        private LinearGradientBrush _buyBGBrush = new LinearGradientBrush(Color.FromArgb(255, 9, 87, 137),
                Color.FromArgb(255, 20, 20, 20),
                new Point(0.5, 0),
                new Point(0.5, 1));

        // 卖出内边框线 #89C9FF 
        private SolidColorBrush _buyInnerBorderBrush = new SolidColorBrush(Color.FromArgb(255, 137, 201, 255));

        // 卖出价格颜色 #58B3FF
        private SolidColorBrush _buyPriceBrush = new SolidColorBrush(Color.FromArgb(255, 88, 179, 255));

        // 补充数据不符合颜色 #E63034
        private SolidColorBrush _errorQtyBrush = new SolidColorBrush(Color.FromArgb(255, 230, 48, 52));

        // 补充数量符合颜色 #515151
        private SolidColorBrush _nornalQtyBrush = new SolidColorBrush(Color.FromArgb(255, 81, 81, 81));



        /// <summary>
        /// 包号
        /// </summary>
        private string _blockNum = string.Empty;

        public string BlockNum
        {
            get
            {
                return _blockNum;
            }
            set
            {
                if (_blockNum != value)
                {
                    _blockNum = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 线下补录， 选择买入
        /// </summary>
        private bool _buyIsCheck = true;

        public bool BuyIsCheck
        {
            get
            {
                return _buyIsCheck;
            }
            set
            {
                if (_buyIsCheck != value)
                {
                    _buyIsCheck = value;
                    if (_buyIsCheck)
                    {
                        this.Side = "1";
                    }
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 线下补录， 选择买入
        /// </summary>
        private bool _sellIsCheck = false;

        public bool SellIsCheck
        {
            get
            {
                return _sellIsCheck;
            }
            set
            {
                if (_sellIsCheck != value)
                {
                    _sellIsCheck = value;
                    if (_sellIsCheck)
                    {
                        this.Side = "2";
                    }
                }
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// 买卖方向
        /// </summary>
        private string _side = string.Empty;

        public string Side
        {
            get
            {
                return _side;
            }
            set
            {
                if (_side != value)
                {
                    _side = value;
                    if (_side == "2")
                    {
                        // 卖出
                        this.SideDisplay = "卖出";
                        this.BorderBrush = this._sellBorderBrush;
                        this.BGBrush = this._sellBGBrush;
                        this.InnerBorderBrush = this._sellInnerBorderBrush;
                    }
                    else
                    {
                        // 买入
                        this.SideDisplay = "买入";
                        this.BorderBrush = this._buyBorderBrush;
                        this.BGBrush = this._buyBGBrush;
                        this.InnerBorderBrush = this._buyInnerBorderBrush;
                    }
                }
            }
        }

        /// <summary>
        /// 买卖显示文字
        /// </summary>
        private string _sideDisplay = string.Empty;

        public string SideDisplay
        {
            get
            {
                return _sideDisplay;
            }
            set
            {
                if (_sideDisplay != value)
                {
                    _sideDisplay = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 边框颜色
        /// </summary>
        private SolidColorBrush _borderBrush;

        public SolidColorBrush BorderBrush
        {
            get
            {
                return _borderBrush;
            }
            set
            {
                if (_borderBrush != value)
                {
                    this._borderBrush = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 标题背景颜色
        /// </summary>
        private LinearGradientBrush _bgBrush;

        public LinearGradientBrush BGBrush
        {
            get
            {
                return _bgBrush;
            }
            set
            {
                if (_bgBrush != value)
                {
                    this._bgBrush = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 内边框色 
        /// </summary>
        private SolidColorBrush _innerBorderBrush;

        public SolidColorBrush InnerBorderBrush
        {
            get
            {
                return _innerBorderBrush;
            }
            set
            {
                if (_innerBorderBrush != value)
                {
                    this._innerBorderBrush = value;
                    this.OnPropertyChanged();
                }
            }
        }


        /// <summary>
        /// 债券代码
        /// </summary>
        private string _securityID = string.Empty;

        public string SecurityID
        {
            get
            {
                return _securityID;
            }
            set
            {
                if (_securityID != value)
                {
                    this._securityID = value;
                    _updateSecurityID?.Invoke(_securityID);
                    this.OnPropertyChanged();
                }
            }
        }


        /// <summary>
        /// 债券名称
        /// </summary>
        private string _symbol = string.Empty;

        public string Symbol
        {
            get
            {
                return _symbol;
            }
            set
            {
                if (_symbol != value)
                {
                    _symbol = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 价格
        /// </summary>
        private string _yield;

        public string Yield
        {
            get
            {
                return _yield;
            }
            set
            {
                if (_yield != value)
                {
                    _yield = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 价格
        /// </summary>
        private string _price;

        public string Price
        {
            get
            {
                return _price;
            }
            set
            {
                if (_price != value)
                {
                    _price = value;
                    this.OnPropertyChanged();
                }
            }
        }
        /// <summary>
        /// 数量
        /// </summary>
        private string _qty = string.Empty;

        public string Qty
        {
            get
            {
                return _qty;
            }
            set
            {
                if (_qty != value)
                {
                    _qty = value;


                    this.OnPropertyChanged();

                    this.QtyLong = CommonUtils.ConvertToLong(_qty);

                    if (IsUnderline && SelfList != null && SelfList.Count > 0)
                    {
                        BookEditItemViewModel bookEditItemViewModel = SelfList[0];
                        if (!string.IsNullOrEmpty(bookEditItemViewModel.PanelType) && bookEditItemViewModel.PanelType.Contains(Constants.SELF_PART))
                            this.SelfQty = QtyLong;

                    }
                }
            }
        }

        private long _Qty_Long = 0;
        public long QtyLong
        {
            get
            {
                return _Qty_Long;
            }
            set
            {
                if (_Qty_Long != value)
                {
                    _Qty_Long = value;
                    if (_Qty_Long == this.CounterPartQty)
                    {
                        this.CounterPartyQtyBrush = _nornalQtyBrush;
                    }
                    else
                    {
                        this.CounterPartyQtyBrush = _errorQtyBrush;
                    }

                    if (_Qty_Long == this.SelfQty)
                    {
                        this.SelfQtyBrush = _nornalQtyBrush;
                    }
                    else
                    {
                        this.SelfQtyBrush = _errorQtyBrush;
                    }
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 成交时间
        /// </summary>
        private string _tradedate;

        public string TradeDate
        {
            get
            {
                return _tradedate;
            }
            set
            {
                if (_tradedate != value)
                {
                    _tradedate = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 交割日期
        /// </summary>
        private string _settldate;

        public string SettleDate
        {
            get
            {
                return _settldate;
            }
            set
            {
                if (_settldate != value)
                {
                    _settldate = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 清算速度
        /// </summary>
        private string _settlType = string.Empty;

        public string SettlType
        {
            get
            {
                return _settlType;
            }
            set
            {
                if (_settlType != value)
                {
                    _settlType = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        ///  清算速度 日期部分
        /// </summary>
        private DateTime _settltypeDateTime = DateTimeEx.ServerTime;

        public DateTime SettlTypeDateTime
        {
            get
            {
                return _settltypeDateTime;
            }
            set
            {
                if (_settltypeDateTime != value)
                {
                    _settltypeDateTime = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 清算速度 +0，+1部分
        /// </summary>
        private int _comboBoxSelectIndex = 0;

        public int ComboBoxSelectIndex
        {
            get
            {
                return _comboBoxSelectIndex;
            }
            set
            {
                if (_comboBoxSelectIndex != value)
                {
                    _comboBoxSelectIndex = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        ///  备注
        /// </summary>
        private string _Remark;

        public string Remark
        {
            get
            {
                return _Remark;
            }
            set
            {
                if (_Remark != value)
                {
                    _Remark = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 最后发送
        /// </summary>
        private string _lstSendTime = string.Empty;

        public string LstSendTime
        {
            get
            {
                return _lstSendTime;
            }
            set
            {
                if (_lstSendTime != value)
                {
                    _lstSendTime = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 最后更新
        /// </summary>
        private string _updateTime = string.Empty;

        public string UpdateTime
        {
            get
            {
                return _updateTime;
            }
            set
            {
                if (_updateTime != value)
                {
                    _updateTime = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 本方数量
        /// </summary>
        private decimal _selfQty = 0;

        public decimal SelfQty
        {
            get
            {
                return _selfQty;
            }
            set
            {
                if (_selfQty != value)
                {
                    _selfQty = value;
                    if (UnderlineVisibility == Visibility.Collapsed && this.SelfList.Count > 0)
                    {
                        if (this.SelfList[0].PanelType.Contains(Constants.SELF_PART))
                        {
                            this.SelfList[0].Qty = value.ToString();
                        }
                    }
                    if (_selfQty == this.QtyLong)
                    {
                        this.SelfQtyBrush = _nornalQtyBrush;
                    }
                    else
                    {
                        this.SelfQtyBrush = _errorQtyBrush;
                    }

                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 对手方数量
        /// </summary>
        private decimal _CounterPartyQty = 0;

        public decimal CounterPartQty
        {
            get
            {
                return _CounterPartyQty;
            }
            set
            {
                if (_CounterPartyQty != value)
                {
                    _CounterPartyQty = value;

                    if (_CounterPartyQty == this.QtyLong)
                    {
                        this.CounterPartyQtyBrush = _nornalQtyBrush;
                    }
                    else
                    {
                        this.CounterPartyQtyBrush = _errorQtyBrush;
                    }

                    if (this.SelfList != null && this.SelfList.Count > 0)
                    {
                        if (this.SelfList[0].PanelType.Contains(Constants.SELF_PART))
                        {
                            //this.SelfQty = value;
                        }
                    }
                    this.OnPropertyChanged();
                }
            }
        }

        private Visibility isTipVis = Visibility.Collapsed;

        public Visibility IsTipVis
        {
            get
            {
                return isTipVis;
            }

            set
            {
                isTipVis = value;
                this.OnPropertyChanged();
            }
        }
        private string info7 = "";


        public string Info7
        {
            get
            {
                return info7;
            }

            set
            {
                info7 = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// 本方数量颜色
        /// </summary>
        private SolidColorBrush _selfQtyBrush;

        public SolidColorBrush SelfQtyBrush
        {
            get
            {
                return _selfQtyBrush;
            }
            set
            {
                if (_selfQtyBrush != value)
                {
                    _selfQtyBrush = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 对手方数量颜色
        /// </summary>
        private SolidColorBrush _counterPartyQtyBrush;

        public SolidColorBrush CounterPartyQtyBrush
        {
            get
            {
                return _counterPartyQtyBrush;
            }
            set
            {
                if (_counterPartyQtyBrush != value)
                {
                    _counterPartyQtyBrush = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        ///  本方列表
        /// </summary>
        private ObservableCollection<BookEditItemViewModel> _SelfList = new ObservableCollection<BookEditItemViewModel>();

        public ObservableCollection<BookEditItemViewModel> SelfList
        {
            get
            {
                return _SelfList;
            }
            set
            {
                if (_SelfList != value)
                {
                    _SelfList = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 对手方列表
        /// </summary>
        private ObservableCollection<BookEditItemViewModel> _CounterPartList = new ObservableCollection<BookEditItemViewModel>();

        public ObservableCollection<BookEditItemViewModel> CounterPartList
        {
            get
            {
                return _CounterPartList;
            }
            set
            {
                if (_CounterPartList != value)
                {
                    _CounterPartList = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 是否显示线上部分
        /// </summary>
        private Visibility _OnlineVisibility = Visibility.Collapsed;
        public Visibility OnlineVisibility
        {
            get
            {
                return _OnlineVisibility;
            }
            set
            {
                if (_OnlineVisibility != value)
                {
                    _OnlineVisibility = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        ///  是否显示线下部分
        /// </summary>
        private Visibility _UnderlineVisiblity = Visibility.Collapsed;

        public Visibility UnderlineVisibility
        {
            get
            {
                return _UnderlineVisiblity;
            }
            set
            {
                if (_UnderlineVisiblity != value)
                {
                    _UnderlineVisiblity = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 添加对手方按钮是否显示
        /// </summary>
        private Visibility _CounterPartyAddVisibility = Visibility.Collapsed;

        public Visibility CounterPartyAddVisibility
        {
            get
            {
                return _CounterPartyAddVisibility;
            }
            set
            {
                if (_CounterPartyAddVisibility != value)
                {
                    _CounterPartyAddVisibility = value;
                    this.OnPropertyChanged();
                }
            }
        }


        private BookEditItemViewModel _selfSelectedItem;

        public BookEditItemViewModel SelfSelectedItem
        {
            get
            {
                return _selfSelectedItem;
            }
            set
            {
                if (_selfSelectedItem != value)
                {
                    _selfSelectedItem = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private BookEditItemViewModel _counterPartySelectedItem;

        public BookEditItemViewModel CounterPartySelectedItem
        {
            get
            {
                return _counterPartySelectedItem;
            }
            set
            {
                if (_counterPartySelectedItem != value)
                {
                    _counterPartySelectedItem = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 添加对手方
        /// </summary>
        public ICommand AddCounterPartCommand { get; set; }

        /// <summary>
        /// 删除对手方
        /// </summary>
        public ICommand DelCounterPartCommand { get; set; }

        /// <summary>
        /// 切换面板类型
        /// </summary>
        public ICommand ChangePanelTypeCommand { get; set; }

        /// <summary>
        /// 切换经纪商
        /// </summary>
        public ICommand BrokerChangeCommand { get; set; }


        //private ConcurrentDictionary<string, String> _CacheBlocks = new ConcurrentDictionary<string, String>();
        private static readonly Object updateEventLock = new Object();
        private static Action<String> _updateSecurityID;
        public static event Action<string> UpdateSecurityID
        {
            add
            {
                lock (updateEventLock)
                {
                    _updateSecurityID += value;
                }
                //System.Threading.ThreadPool.QueueUserWorkItem(o =>
                //{
                //    foreach (var obj in _CacheBlocks)
                //    {
                //        _updateSecurity?.Invoke(obj.Value);
                //    }
                //});
            }
            remove
            {
                lock (updateEventLock)
                {
                    _updateSecurityID -= value;
                }
            }
        }

        private bool IsUnderline = false;
        private string terminal = "2";
        private bool isEdit = false;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="blocknum"></param>
        /// <param name="isUnderline"></param>
        /// <param name="order"></param>
        public BookEditViewModel(string blocknum, bool isUnderline = false, ExecutionReport order = null)
        {
            Side = "1";
            this.BlockNum = blocknum;
            this.IsUnderline = isUnderline;

            ChangePanelTypeCommand = new RelayCommand<ComboBox>(ChangePanelType);

            if (IsUnderline)
            {
                AddCounterPartCommand = new RelayCommand(AddCounterPart);
                DelCounterPartCommand = new RelayCommand<Button>(DelCounterPart);

                BrokerChangeCommand = new RelayCommand<ComboBox>(BrokerChangePart);

                CounterPartyAddVisibility = Visibility.Visible;
                if (string.IsNullOrEmpty(this.BlockNum))
                {
                    OnlineVisibility = Visibility.Collapsed;
                    UnderlineVisibility = Visibility.Visible;
                    terminal = "2";
                    InitUnderline(order);

                    if (order != null)
                        CounterPartyAddVisibility = Visibility.Collapsed;

                }
                else
                {
                    OnlineVisibility = Visibility.Visible;
                    UnderlineVisibility = Visibility.Collapsed;
                    isEdit = true;
                    LoadingDatas();
                }
            }
            else
            {
                CounterPartyAddVisibility = Visibility.Collapsed;
                OnlineVisibility = Visibility.Visible;
                UnderlineVisibility = Visibility.Collapsed;
                isEdit = true;
                LoadingDatas();
            }
        }

        /// <summary>
        /// 切换5家经纪商
        /// </summary>
        /// <param name="obj"></param>
        private void BrokerChangePart(ComboBox obj)
        {
            if (obj != null && obj.DataContext != null && obj.DataContext is BookEditItemViewModel)
            {
                BookEditItemViewModel itemVM = obj.DataContext as BookEditItemViewModel;
                if (itemVM.IsBrokerChangePanelType == true)
                {
                    itemVM.IsBrokerChangePanelType = false;
                    return;
                }
                BookEditView bookEditView = CommonUtils.GetParentObject<BookEditView>(obj) as BookEditView;
                if (Common.View.Component.CustomMessageBox.Question("是否要切换当前经纪商?", "切换", bookEditView) == false)
                {
                    itemVM.IsBrokerChangePanelType = true;
                    itemVM.Broker = itemVM.LastBroker;
                    itemVM.BrokerItem = itemVM.LastBrokerItem;
                    return;
                }

                ObservableCollection<BookEditItemViewModel> List;
                if (itemVM.IsSelf)
                {
                    List = this.SelfList;
                }
                else
                {
                    List = this.CounterPartList;
                }
                if (List != null && string.IsNullOrEmpty(itemVM.LastBroker) == false && itemVM.PanelType.Contains("underline"))
                {
                    int index = List.IndexOf(itemVM);
                    if (index != -1)
                    {
                        itemVM.RemoveDatas(this);
                    }
                }
            }

        }

        /// <summary>
        /// 加载交易包数据,  显示加以包编辑框
        /// </summary>
        private async void LoadingDatas()
        {
            if (string.IsNullOrEmpty(this.BlockNum) == false)
            {
                await Task.Run(() =>
                {
                    try
                    {
                        #region 模拟数据 

                        //List<ExecutionReport> datas = this._originalDatas;
                        //if (datas == null)
                        //{
                        //    datas = GetTestList("");
                        //}
                        #endregion

                        BlockNumQueryReq request = new BlockNumQueryReq();
                        request.blocknum = this.BlockNum;
                        request.pageSize = 5000;
                        request.pageNum = 0;
                        if (isEdit)
                        {
                            request.flag = "2";
                        }
                        else
                        {
                            request.flag = "1";
                        }
                        isEdit = false;
                        BlockNumQueryPo blockNumQueryPo = BookMgtHandler.Instance.BlockNumQuery(request);
                        //blockNumQueryPo = Ats.Foundation.Message.MsgHelper.Deserialize<BlockNumQueryPo>("{\"datas\":[{\"ActionFlag\":\"A\",\"BizTypeID\":\"25\",\"BlockNum\":\"RB771312093734387712\",\"BrokerExecID\":\"\",\"CounterPartyID\":\"\",\"CounterPartyName\":\"博时ddf\",\"CounterPartyOrdID\":\"\",\"CounterPartySubID\":\"\",\"CounterPartySubName\":\"杨柳迪\",\"CounterPartyVenues\":\"TPSC\",\"CounterpartySalesAgentFlag\":\"0\",\"DcsexecID\":\"DCS770675148524109824\",\"DealType\":\"2\",\"ExecID\":\"R771312093738582020\",\"ExecType\":\"22\",\"LastQty\":\"1000.00000000\",\"OrderID\":\"R771312093738582021\",\"ParentFlag\":\"\",\"ParentFlagQty\":\"2000\",\"PartyID\":\"25\",\"PartyName\":\"杨柳迪\",\"PartySubID\":\"\",\"PartySubName\":\"黄义峰\",\"Price\":\"3.25600000\",\"RecordNum\":\"3\",\"SalesAgentFlag\":\"0\",\"SecurityID\":\"170202.IB\",\"SendingTime\":\"2020-10-29 09:58:29.273\",\"SettlDate\":\"20201119\",\"SettlType\":\"2\",\"Side\":\"1\",\"StandardHeader\":{\"MsgType\":\"8\"},\"StandardTrailer\":{},\"Symbol\":\"17国开02\",\"SynchFlag\":\"22\",\"Terminal\":\"2\",\"TradeDate\":\"20201118\",\"TransType\":\"A\",\"TransactTime\":\"2020-10-29 09:58:53\",\"UpdateTime\":\"2020-10-29 09:58:29.273\",\"UpdateUser\":\"ling.j\",\"UserID\":\"36\",\"UserName\":\"hyf\",\"VenueTypeGW\":\"OFFLINE\",\"Venues\":\"TBOOK\",\"Yield\":\"3.2560\"},{\"ActionFlag\":\"A\",\"BizTypeID\":\"25\",\"BlockNum\":\"RB771312093734387712\",\"CounterPartyID\":\"\",\"CounterPartyName\":\"\",\"CounterPartyOrdID\":\"\",\"CounterPartySubID\":\"\",\"CounterPartySubName\":\"\",\"CounterPartyVenues\":\"TBook\",\"CounterpartySalesAgentFlag\":\"1\",\"DealType\":\"2\",\"ExecID\":\"R771312093738582018\",\"ExecType\":\"21\",\"LastQty\":\"1000.00000000\",\"OrderID\":\"R771312093738582019\",\"ParentFlag\":\"0\",\"ParentFlagQty\":\"2000\",\"PartyID\":\"25\",\"PartySubID\":\"94\",\"PartySubName\":\"霸气校长\",\"Price\":\"2.33300000\",\"RecordNum\":\"3\",\"SalesAgentFlag\":\"1\",\"SecurityID\":\"170202.IB\",\"SendingTime\":\"2020-10-29 09:58:29.270\",\"SettlDate\":\"20201029\",\"SettlType\":\"1\",\"Side\":\"1\",\"StandardHeader\":{\"MsgType\":\"8\"},\"StandardTrailer\":{},\"Symbol\":\"17国开02\",\"SynchFlag\":\"22\",\"Terminal\":\"2\",\"TradeDate\":\"20201029\",\"TransType\":\"A\",\"TransactTime\":\"2020-10-29 09:58:53\",\"UpdateTime\":\"2020-10-29 09:58:29.270\",\"UpdateUser\":\"ling.j\",\"UserID\":\"94\",\"UserName\":\"164646\",\"VenueTypeGW\":\"OFFLINE\",\"Venues\":\"TBook\",\"Yield\":\"2.333\"},{\"ActionFlag\":\"A\",\"BizTypeID\":\"25\",\"BlockNum\":\"RB771312093734387712\",\"CounterPartyID\":\"25\",\"CounterPartyName\":\"山西证券\",\"CounterPartyOrdID\":\"25\",\"CounterPartySubID\":\"94\",\"CounterPartySubName\":\"霸气校长\",\"CounterPartyVenues\":\"TBOOK\",\"CounterpartySalesAgentFlag\":\"1\",\"DealType\":\"1\",\"ExecID\":\"R771312093738582016\",\"ExecType\":\"21\",\"LastQty\":\"1000.00000000\",\"OrderID\":\"R771312093738582017\",\"ParentFlag\":\"1\",\"ParentFlagQty\":\"2000\",\"PartyID\":\"25\",\"PartyName\":\"山西证券\",\"PartySubID\":\"36\",\"PartySubName\":\"黄义峰\",\"RecordNum\":\"3\",\"SalesAgentFlag\":\"0\",\"SecurityID\":\"170202.IB\",\"SendingTime\":\"2020-10-29 09:58:29.272\",\"SettlDate\":\"20201029\",\"SettlType\":\"1\",\"Side\":\"1\",\"StandardHeader\":{\"MsgType\":\"8\"},\"StandardTrailer\":{},\"Symbol\":\"17国开02\",\"SynchFlag\":\"22\",\"Terminal\":\"2\",\"TradeDate\":\"20201029\",\"TransType\":\"A\",\"TransactTime\":\"2020-10-29 09:58:53\",\"UpdateTime\":\"2020-10-29 09:58:29.272\",\"UpdateUser\":\"ling.j\",\"UserID\":\"36\",\"UserName\":\"hyf\",\"VenueTypeGW\":\"OFFLINE\",\"Venues\":\"TBOOK\"}],\"totalSize\":3}"); 
                        //blockNumQueryPo = Ats.Foundation.Message.MsgHelper.Deserialize<BlockNumQueryPo>("{\"datas\":[{\"ActionFlag\":\"A\",\"BlockNum\":\"IN2722133\",\"CounterPartyID\":\"\",\"CounterPartyName\":\"销售公账\",\"CounterPartyOrdID\":\"\",\"CounterPartySubID\":\"246\",\"CounterPartySubName\":\"销售公账\",\"CounterPartyVenues\":\"TBOOK\",\"CounterpartySalesAgentFlag\":\"1\",\"DealType\":\"1\",\"ExecID\":\"R855091849625370624\",\"ExecType\":\"22\",\"Info7\":\"\",\"LastQty\":\"1000.00000000\",\"OrderID\":\"R855091849625370625\",\"ParentFlag\":\"0\",\"ParentFlagQty\":\"2000\",\"PartyName\":\"鄞州银行\",\"RecordNum\":\"3\",\"SalesAgentFlag\":\"1\",\"SecurityID\":\"110244.IB\",\"SendingTime\":\"2021 - 06 - 17 14:29:17.673\",\"SettlDate\":\"20210618\",\"SettlType\":\"2\",\"Side\":\"1\",\"StandardHeader\":{\"MsgType\":\"8\"},\"StandardTrailer\":{},\"Symbol\":\"11国开44\",\"SynchFlag\":\"22\",\"Terminal\":\"0\",\"TradeDate\":\"20210617\",\"TradeMethod\":\"1\",\"TransType\":\"A\",\"TransactTime\":\"2021 - 06 - 17 14:29:43\",\"UpdateTime\":\"2021 - 06 - 17 14:29:17.673\",\"UpdateUser\":\"ling.j\",\"UserID\":\"901\",\"UserName\":\"ling.j\",\"VenueTypeGW\":\"OFFLINE\",\"Venues\":\"TBOOK\",\"Yield\":\"5.0000\"},{\"ActionFlag\":\"A\",\"BlockNum\":\"IN2722133\",\"CounterPartyID\":\"105881\",\"CounterPartyName\":\"安邦基金\",\"CounterPartyOrdID\":\"105881\",\"CounterPartySubID\":\"105881\",\"CounterPartySubName\":\"安邦基金\",\"CounterPartyVenues\":\"IDB\",\"CounterpartySalesAgentFlag\":\"1\",\"DealType\":\"2\",\"ExecID\":\"R855091675310096384\",\"ExecType\":\"22\",\"Info7\":\"\",\"LastQty\":\"1000.00000000\",\"OrderID\":\"R855091675310096385\",\"ParentFlag\":\"0\",\"ParentFlagQty\":\"2000\",\"PartyName\":\"鄞州银行\",\"PartySubID\":\"901\",\"PartySubName\":\"ling.j\",\"Price\":\"4.50000000\",\"RecordNum\":\"3\",\"SalesAgentFlag\":\"1\",\"SecurityID\":\"110244.IB\",\"SendingTime\":\"2021 - 06 - 17 14:29:17.671\",\"SettlDate\":\"20210618\",\"SettlType\":\"2\",\"Side\":\"1\",\"StandardHeader\":{\"MsgType\":\"8\"},\"StandardTrailer\":{},\"Symbol\":\"11国开44\",\"SynchFlag\":\"22\",\"Terminal\":\"0\",\"TradeDate\":\"20210617\",\"TradeMethod\":\"60\",\"TransType\":\"A\",\"TransactTime\":\"2021 - 06 - 17 14:29:02\",\"UpdateTime\":\"2021 - 06 - 17 14:29:17.671\",\"UpdateUser\":\"ling.j\",\"UserID\":\"901\",\"UserName\":\"ling.j\",\"VenueTypeGW\":\"OFFLINE\",\"Venues\":\"IDB\",\"Yield\":\"4.5000\"},{\"ActionFlag\":\"A\",\"BizType\":\"9\",\"BizTypeID\":\"9\",\"BlockNum\":\"IN2722133\",\"ClOrdID\":\"855091450046672896\",\"CounterPartyName\":\"鄞州银行\",\"CounterPartySubID\":\"902\",\"CounterPartySubName\":\"yzrbdealer\",\"CounterPartyVenues\":\"MANUAL\",\"CounterpartySalesAgentFlag\":\"\",\"DealType\":\"1\",\"DirtyPrice\":\"104.14459315\",\"ExecID\":\"I855091450050781186\",\"ExecType\":\"22\",\"FirmFlag\":\"1\",\"Info3\":\"\",\"Info4\":\"G0001242011FINPBB44CIB\",\"Info6\":\"2\",\"Info7\":\"\",\"LastQty\":\"2000.00000000\",\"MarketIndicator\":\"4\",\"NetPrice\":\"99.95610000\",\"OrdStatus\":\"2\",\"OrdType\":\"2\",\"OrderID\":\"e855091450055086081\",\"OrigOrdPrice\":\"5\",\"ParentFlag\":\"1\",\"ParentFlagQty\":\"2000\",\"PartyName\":\"鄞州银行\",\"PartySubID\":\"901\",\"PartySubName\":\"ling.j\",\"Price\":\"5.00000000\",\"RecordNum\":\"3\",\"RoleID\":\"733993404672864256\",\"RoleName\":\"MyTest01\",\"SalesAgentFlag\":\"1\",\"SecurityID\":\"110244.IB\",\"SendingTime\":\"2021 - 06 - 17 14:29:17.673\",\"SettlDate\":\"20210618\",\"SettlType\":\"2\",\"Side\":\"2\",\"StandardHeader\":{\"MsgType\":\"8\"},\"StandardTrailer\":{},\"StrategyOrderId\":\"s855091450038284288\",\"Symbol\":\"11国开44\",\"SynchFlag\":\"22\",\"Terminal\":\"0\",\"TimeInForce\":\"3\",\"TradeDate\":\"20210617\",\"TransType\":\"A\",\"TransactTime\":\"2021 - 06 - 17 14:28:08.523\",\"UpdateTime\":\"2021 - 06 - 17 14:29:17.673\",\"UpdateUser\":\"ling.j\",\"UserID\":\"901\",\"UserName\":\"ling.j\",\"VenueTypeGW\":\"INNER\",\"Venues\":\"MANUAL\",\"Yield\":\"5\"}],\"totalSize\":3} ");

                        if (blockNumQueryPo != null && blockNumQueryPo.datas != null && blockNumQueryPo.datas.Count > 0)
                        {
                            _originalDatas = blockNumQueryPo.datas;
                            foreach (ExecutionReport obj in blockNumQueryPo.datas)
                            {
                                terminal = obj.Terminal;
                                break;
                            }
                            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                            {
                                string selfPartySubID = string.Empty;
                                List<string> usedList = new List<string>();
                                foreach (ExecutionReport report in _originalDatas)
                                {
                                    if (report.ParentFlag == "1")
                                    {
                                        selfPartySubID = report.PartySubID;

                                        // 交易包要素
                                        this.Side = report.Side;
                                        this.SecurityID = report.SecurityID;
                                        this.Symbol = report.Symbol;
                                        // 对手方成交价， 或者内部划转价格
                                        //if (report.SalesAgentFlag == "1" && report.CounterpartySalesAgentFlag == "1")
                                        //{
                                        //    this.Yield = (CommonUtils.ConvertToDecimal(report.Price) + CommonUtils.ConvertToDecimal(report.Yield)) / 2;
                                        //}
                                        //else
                                        //{
                                        //    this.Yield = CommonUtils.ConvertToDecimal(report.Price);
                                        //}

                                        this.Yield = CommonUtils.ConvertToDecimal(report.Yield).ToString("N4");

                                        this.Price = CommonUtils.ConvertToDecimal(report.Price).ToString("N4");
                                        this.TradeDate = report.TradeDate;
                                        this.SettleDate = report.SettlDate;
                                        this.SettlType = report.SettlType;

                                        if (IsUnderline)
                                        {
                                            this.Yield = "(参见对手方)";
                                            this.SettlType = "(参见对手方)";

                                        }

                                        if (UnderlineVisibility == Visibility.Visible)
                                        {
                                            DateTime settlDateTime;
                                            int selectIndex;
                                            CommonUtils.SetSettleType(report.SettlType, report.TradeDate, out settlDateTime, out selectIndex);
                                            this.SettlTypeDateTime = settlDateTime;
                                            this.ComboBoxSelectIndex = selectIndex;
                                        }

                                        this.LstSendTime = report.SendingTime;
                                        this.UpdateTime = report.UpdateTime;

                                        // 生成本方
                                        BookEditItemViewModel item = new BookEditItemViewModel();
                                        item.VenuesTypeGW = report.VenueTypeGW;
                                        item.Venues = report.Venues;
                                        item.ExecID = report.ExecID;
                                        item.IsSelf = true;
                                        item.IsUnderline = IsUnderline;
                                        item.PanelTypeDic = item.SelfPanelTypeDic;
                                        if (UnderlineVisibility == Visibility.Visible)
                                        {
                                            if (report.SalesAgentFlag == "1")
                                            {
                                                item.PanelType = Constants.IDB_UNDERLINE_PART;
                                            }
                                            else
                                            {
                                                item.PanelType = Constants.SELF_UNDERLINE_PART;
                                            }
                                        }
                                        else
                                        {
                                            if (report.SalesAgentFlag == "1")
                                            {
                                                item.PanelType = Constants.IDB_PART;
                                            }
                                            else
                                            {
                                                item.PanelType = Constants.SELF_PART;
                                            }
                                        }

                                        //item.PanelType = GetPanelType(report.SalesAgentFlag, report.Venues);
                                        if (item.PanelType.Contains(Constants.SELF_PART))
                                        {
                                            IEnumerable<StcTbookUsers> findList = BookHandler.SelfList.Where(entry => entry.Name == report.PartySubName);
                                            if (findList != null && findList.Count() > 0)
                                            {
                                                StcTbookUsers stcTBookUser = findList.First();
                                                item.SaleItem = stcTBookUser;
                                            }
                                            else
                                            {
                                                // 可能角色改变，模拟一条历史用户信息
                                                if (!string.IsNullOrEmpty(report.BizTypeID))
                                                {
                                                    StcTbookUsers stcTBookUser = new StcTbookUsers();
                                                    stcTBookUser.Biztypeid = report.BizTypeID;
                                                    stcTBookUser.Name = report.PartySubName;
                                                    stcTBookUser.Userid = report.PartySubID;
                                                    item.SaleItem = stcTBookUser;
                                                    stcTBookUser.IsHistory = true;
                                                    BookHandler.SalesList.Add(stcTBookUser);
                                                }
                                            }
                                        }
                                        else if (item.PanelType.Contains(Constants.IDB_PART))
                                        {
                                            IEnumerable<StcTbookUsers> findList = BookHandler.SalesList.Where(entry => entry.Name == report.PartySubName);
                                            if (findList != null && findList.Count() > 0)
                                            {
                                                StcTbookUsers stcTBookUser = findList.First();
                                                item.SaleItem = stcTBookUser;
                                            }
                                            else
                                            {
                                                // 可能角色改变，模拟一条历史用户信息
                                                if (!string.IsNullOrEmpty(report.BizTypeID))
                                                {
                                                    StcTbookUsers stcTBookUser = new StcTbookUsers();
                                                    stcTBookUser.Biztypeid = report.BizTypeID;
                                                    stcTBookUser.Name = report.PartySubName;
                                                    stcTBookUser.Userid = report.PartySubID;
                                                    item.SaleItem = stcTBookUser;
                                                    stcTBookUser.IsHistory = true;
                                                    BookHandler.SalesList.Add(stcTBookUser);
                                                }
                                            }
                                        }

                                        item.InstitutionID = report.PartyID;
                                        item.Name = report.PartyName;

                                        item.SubID = report.PartySubID;
                                        item.SubName = report.PartySubName;
                                        if (!string.IsNullOrEmpty(report.TradeMethod))
                                        {
                                            if (SecurityID.EndsWith("IB"))
                                            {
                                                item.TradeMethod = ComboSourceUtil.Instance.FindEnum(report.TradeMethod, "trademethod");
                                                item.SendPriceTypeVisibility = "0";
                                                item.PositionVisibility = "2";
                                            }
                                            else
                                            {
                                                item.TradeMethod = report.TradeMethod;
                                            }
                                        }
                                        else
                                        {
                                            if (SecurityID.EndsWith("IB"))
                                            {
                                                item.SendPriceTypeVisibility = "0";
                                                item.PositionVisibility = "2";
                                            }
                                            else
                                            {
                                                item.SendPriceTypeVisibility = "2";
                                                item.PositionVisibility = "0";
                                            }
                                        }

                                        item.Yield = string.IsNullOrEmpty(report.OrigOrdPrice) ? (string.IsNullOrEmpty(report.Yield) ? "--" : report.Yield) : CommonUtils.ConvertToDecimal(report.OrigOrdPrice).ToString("N4"); // 本方报价
                                        item.Qty = (string.IsNullOrEmpty(report.LastQty) || report.LastQty == "0") ? "--" : CommonUtils.ConvertToLong(report.LastQty).ToString();
                                        item.SettlType = report.SettlType;
                                        item.TradeDate = report.TradeDate;
                                        item.SettlDate = report.SettlDate;
                                        item.CreateTime = report.TransactTime;
                                        this.Info7 = report.Info7;
                                        if (!string.IsNullOrEmpty(report.Info7))
                                        {
                                            this.IsTipVis = Visibility.Visible;
                                        }
                                        else
                                        {
                                            this.IsTipVis = Visibility.Collapsed;
                                        }
                                        item.LastPx = string.IsNullOrEmpty(report.Price) ? string.Empty : CommonUtils.ConvertToDecimal(report.Price).ToString("N4");
                                        if (report.SalesAgentFlag == "1")
                                        {
                                            //foreach (ExecutionReport subReport in _originalDatas)
                                            for (int i = _originalDatas.Count - 1; i >= 0; i--)
                                            {

                                                ExecutionReport subReport = _originalDatas[i];

                                                if (usedList.Contains(subReport.ExecID) == false
                                                    && (subReport.PartySubID == report.PartySubID || (!IsUnderline && subReport.PartySubID == subReport.UserID))
                                                    && subReport.Side != report.Side
                                                    && subReport.SalesAgentFlag == "1"
                                                    && subReport.ParentFlag != "1")
                                                {
                                                    usedList.Add(subReport.ExecID);
                                                    DataRow dr = item.Datas.NewRow();
                                                    dr[BookEditItemViewModel.EXECID_FIELD] = subReport.ExecID;
                                                    dr[BookEditItemViewModel.SOURCETRADER_FIELD] = subReport.CounterPartyVenues;
                                                    dr[BookEditItemViewModel.COUNTER_PARTY_ID_FIELD] = subReport.CounterPartyID;
                                                    dr[BookEditItemViewModel.COUNTER_PARTY_NAME_FIELD] = subReport.CounterPartyName;
                                                    dr[BookEditItemViewModel.COUNTER_PARTY_SUB_ID_FIELD] = subReport.CounterPartySubID;
                                                    dr[BookEditItemViewModel.COUNTER_PARTY_SUB_NAME_FIELD] = subReport.CounterPartySubName;
                                                    dr[BookEditItemViewModel.YIELD_FIELD] = CommonUtils.ConvertToDecimal(subReport.Yield).ToString("N4");
                                                    decimal idbQty = CommonUtils.ConvertToDecimal(subReport.LastQty);
                                                    this.SelfQty += idbQty;
                                                    dr[BookEditItemViewModel.QTY_FIELD] = idbQty;
                                                    dr[BookEditItemViewModel.SETTLETYPE_FIELD] = subReport.SettlType;
                                                    dr[BookEditItemViewModel.TRADEDATE_FIELD] = subReport.TradeDate;
                                                    dr[BookEditItemViewModel.SETTLDATE_FIELD] = subReport.SettlDate;
                                                    dr[BookEditItemViewModel.CREATEDATE_FIELD] = subReport.TransactTime;
                                                    string createdate = CommonUtils.ConvertToDateTime(CommonUtils.ConvertToString(dr[BookEditItemViewModel.CREATEDATE_FIELD])).ToString("yyyy-MM-dd");
                                                    string currentdate = DateTime.Now.ToString("yyyy-MM-dd");
                                                    if (createdate.Equals(currentdate))
                                                    {
                                                        dr[BookEditItemViewModel.CREATEDATEVIS_FIELD] = "0";
                                                    }
                                                    else
                                                    {
                                                        dr[BookEditItemViewModel.CREATEDATEVIS_FIELD] = "1";
                                                    }

                                                    string yield = CommonUtils.ConvertToString(dr[BookEditItemViewModel.YIELD_FIELD]);
                                                    if (!this.IsUnderline && !string.IsNullOrEmpty(yield) && !Yield.Equals(yield))
                                                    {
                                                        dr[BookEditItemViewModel.PRICE_WARN_BG] = "1";
                                                    }
                                                    else
                                                        dr[BookEditItemViewModel.PRICE_WARN_BG] = string.Empty;

                                                    string temSettle = CommonUtils.ConvertToString(dr[BookEditItemViewModel.SETTLETYPE_FIELD]);
                                                    if (!this.IsUnderline && !string.IsNullOrEmpty(temSettle) && !SettlType.Equals(temSettle))
                                                    {
                                                        dr[BookEditItemViewModel.SETTLE_WARN_BG] = "1";
                                                    }
                                                    else
                                                        dr[BookEditItemViewModel.SETTLE_WARN_BG] = string.Empty;
                                                    if (!string.IsNullOrEmpty(subReport.TradeMethod))
                                                    {
                                                        if (!string.IsNullOrEmpty(SecurityID))
                                                        {
                                                            if (SecurityID.EndsWith("IB"))//银行间债券
                                                            {
                                                                dr[BookEditItemViewModel.TRADEMETHOD_FIELD] = ComboSourceUtil.Instance.FindEnum(subReport.TradeMethod, "trademethod");
                                                            }
                                                            else//交易所债券SH,SZ
                                                            {
                                                                dr[BookEditItemViewModel.TRADEMETHOD_FIELD] = subReport.TradeMethod;
                                                            }
                                                        }
                                                    }
                                                    dr[BookEditItemViewModel.PARTY_SUB_ID_FIELD] = subReport.PartySubID;
                                                    dr[BookEditItemViewModel.PARTY_SUB_NAME_FIELD] = subReport.PartySubName;
                                                    item.Datas.Rows.Add(dr);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            this.SelfQty += CommonUtils.ConvertToLong(report.ParentFlagQty);// item.Qty);
                                        }
                                        this.Qty = CommonUtils.ConvertToLong(report.ParentFlagQty).ToString();
                                        this.SelfList.Add(item);

                                        break;
                                    }
                                }

                                // 生成对手方
                                //foreach (ExecutionReport report in _originalDatas)
                                for (int i = _originalDatas.Count - 1; i >= 0; i--)
                                {
                                    ExecutionReport report = _originalDatas[i];
                                    if (usedList.Contains(report.ExecID))
                                    {
                                        continue;
                                    }
                                    if (report.PartySubID != selfPartySubID
                                        && !(string.IsNullOrEmpty(report.PartySubID) &&
                                        (report.CounterPartyVenues == "CNEX"
                                                 || report.CounterPartyVenues == "TPSC"
                                                 || report.CounterPartyVenues == "CCTB"
                                                 || report.CounterPartyVenues == "CTTJ"
                                                 || report.CounterPartyVenues == "UEDA"
                                                 || report.CounterPartyVenues == "PTCN"))) //TODO:目前关联DCS单子拿不到交易员对应的QB账号信息，暂时适配Broker的DCS单子判断
                                    {
                                        continue;
                                    }
                                    if (report.ParentFlag.Equals("0"))
                                    {
                                        continue;
                                    }
                                    if (this.CounterPartList.Any(
                                         item =>
                                         {

                                             if (report.CounterPartyVenues == "CNEX"
                                                 || report.CounterPartyVenues == "TPSC"
                                                 || report.CounterPartyVenues == "CCTB"
                                                 || report.CounterPartyVenues == "CTTJ"
                                                 || report.CounterPartyVenues == "UEDA"
                                                 || report.CounterPartyVenues == "PTCN")
                                             {
                                                 return item.PanelType == GetPanelType(report.CounterpartySalesAgentFlag, report.CounterPartyVenues) && item.Broker == report.CounterPartyVenues;
                                             }
                                             else
                                             {
                                                 return item.PanelType == GetPanelType(report.CounterpartySalesAgentFlag, report.CounterPartyVenues) && item.SubName == report.CounterPartySubName;
                                             }

                                         }) == false)
                                    {
                                        usedList.Add(report.ExecID);
                                        BookEditItemViewModel counterPartyItem = new BookEditItemViewModel();
                                        counterPartyItem.IsUnderline = IsUnderline;
                                        counterPartyItem.VenuesTypeGW = report.VenueTypeGW;
                                        counterPartyItem.Venues = report.Venues;
                                        if (IsUnderline)
                                        {
                                            counterPartyItem.DelBtnVisibility = Visibility.Visible;
                                            counterPartyItem.PanelTypeDic = counterPartyItem.CounterPanelTypeDic;
                                        }
                                        counterPartyItem.ExecID = report.ExecID;
                                        counterPartyItem.PanelType = GetPanelType(report.CounterpartySalesAgentFlag, report.CounterPartyVenues);
                                        counterPartyItem.Broker = report.CounterPartyVenues;
                                        if (report.CounterPartyVenues == "CNEX"
                                                || report.CounterPartyVenues == "TPSC"
                                                || report.CounterPartyVenues == "CCTB"
                                                || report.CounterPartyVenues == "CTTJ"
                                                || report.CounterPartyVenues == "UEDA"
                                                || report.CounterPartyVenues == "PTCN")
                                        {
                                            IEnumerable<KeyValuePair<string, string>> arr = counterPartyItem.VenuesSource.Where(pair => pair.Key == report.CounterPartyVenues);
                                            if (arr != null && arr.Count() > 0)
                                            {
                                                counterPartyItem.BrokerItem = arr.First();
                                            }
                                            if (report.Terminal == "1")
                                            {
                                                counterPartyItem.PriceVisibility = Visibility.Collapsed;
                                            }
                                        }

                                        counterPartyItem.InstitutionID = report.CounterPartyID;
                                        counterPartyItem.Name = report.CounterPartyName;
                                        if (counterPartyItem.PanelType.Contains(Constants.SELF_PART))
                                        {
                                            IEnumerable<StcTbookUsers> findList = BookHandler.SelfList.Where(entry => entry.Userid == report.CounterPartySubID);
                                            if (findList != null && findList.Count() > 0)
                                            {
                                                StcTbookUsers stcTBookUser = findList.First();
                                                counterPartyItem.SaleItem = stcTBookUser;
                                            }
                                            else
                                            {
                                                // 可能角色改变，模拟一条历史用户信息
                                                if (!string.IsNullOrEmpty(report.BizTypeID))
                                                {
                                                    StcTbookUsers stcTBookUser = new StcTbookUsers();
                                                    stcTBookUser.Biztypeid = report.BizTypeID;
                                                    stcTBookUser.Name = report.CounterPartySubName;//PartySubName;
                                                    stcTBookUser.Userid = report.CounterPartySubID;// PartySubID;
                                                    stcTBookUser.IsHistory = true;
                                                    BookHandler.SelfList.Add(stcTBookUser);
                                                    //BookHandler.SalesList.Add(stcTBookUser);
                                                    counterPartyItem.SaleItem = stcTBookUser;
                                                }
                                            }
                                        }
                                        else if (counterPartyItem.PanelType.Contains(Constants.IDB_PART))
                                        {
                                            IEnumerable<StcTbookUsers> findList = BookHandler.SalesList.Where(entry => entry.Userid == report.CounterPartySubID);
                                            if (findList != null && findList.Count() > 0)
                                            {
                                                StcTbookUsers stcTBookUser = findList.First();
                                                counterPartyItem.SaleItem = stcTBookUser;
                                            }
                                            else
                                            {
                                                // 可能角色改变，模拟一条历史用户信息
                                                if (!string.IsNullOrEmpty(report.BizTypeID))
                                                {
                                                    StcTbookUsers stcTBookUser = new StcTbookUsers();
                                                    stcTBookUser.Biztypeid = string.IsNullOrEmpty(report.BizTypeID) ? string.Empty : report.BizTypeID;
                                                    stcTBookUser.Name = report.CounterPartySubName;//.PartySubName;
                                                    stcTBookUser.Userid = report.CounterPartySubID;
                                                    stcTBookUser.IsHistory = true;
                                                    BookHandler.SalesList.Add(stcTBookUser);
                                                    counterPartyItem.SaleItem = stcTBookUser;
                                                }
                                            }
                                        }
                                        counterPartyItem.Remark = report.Info3;
                                        counterPartyItem.SubID = report.CounterPartySubID;
                                        counterPartyItem.SubName = report.CounterPartySubName;
                                        if (!string.IsNullOrEmpty(report.TradeMethod))
                                        {
                                            if (SecurityID.EndsWith("IB"))
                                            {
                                                counterPartyItem.TradeMethod = ComboSourceUtil.Instance.FindEnum(report.TradeMethod, "trademethod");
                                                counterPartyItem.PositionVisibility = "2";
                                                counterPartyItem.SendPriceTypeVisibility = "0";
                                                if (report.TradeMethod.Equals("4") || report.TradeMethod.Equals("50"))
                                                {
                                                    counterPartyItem.SubNameVisibility = false;
                                                }
                                                else
                                                {
                                                    counterPartyItem.SubNameVisibility = true;
                                                }
                                            }
                                            else
                                            {
                                                counterPartyItem.TradeMethod = report.TradeMethod;
                                                counterPartyItem.PositionVisibility = "0";
                                                counterPartyItem.SendPriceTypeVisibility = "2";
                                            }
                                        }
                                        else
                                        {
                                            if (SecurityID.EndsWith("IB"))
                                            {
                                                counterPartyItem.SendPriceTypeVisibility = "0";
                                                counterPartyItem.PositionVisibility = "2";
                                            }
                                            else
                                            {
                                                counterPartyItem.SendPriceTypeVisibility = "2";
                                                counterPartyItem.PositionVisibility = "0";
                                            }
                                        }

                                        counterPartyItem.Yield = string.IsNullOrEmpty(Yield) ? string.Empty : CommonUtils.ConvertToDecimal(Yield).ToString("N4"); // 对手方报价
                                        counterPartyItem.LastPx = string.IsNullOrEmpty(report.Price) ? string.Empty : CommonUtils.ConvertToDecimal(report.Price).ToString("N4");
                                        counterPartyItem.Qty = (string.IsNullOrEmpty(report.LastQty) || report.LastQty == "0") ? string.Empty : CommonUtils.ConvertToLong(report.LastQty).ToString();
                                        counterPartyItem.SettlType = report.SettlType;
                                        counterPartyItem.TradeDate = report.TradeDate;
                                        counterPartyItem.SettlDate = report.SettlDate;
                                        counterPartyItem.CreateTime = report.TransactTime;
                                        if (IsUnderline)
                                        {
                                            counterPartyItem.Yield = string.IsNullOrEmpty(report.Yield) ? string.Empty : CommonUtils.ConvertToDecimal(report.Yield).ToString("N4"); // 对手方报价
                                            DateTime settlDateTime;
                                            int selectIndex;
                                            CommonUtils.SetSettleType(report.SettlType, report.TradeDate, out settlDateTime, out selectIndex);
                                            counterPartyItem.SelectDate = settlDateTime;
                                            counterPartyItem.ComboBoxSelectIndex = selectIndex;
                                        }
                                        if (counterPartyItem.PanelType.Contains(Constants.SELF_PART)
                                            || counterPartyItem.PanelType.Contains(Constants.MARKET_PART))
                                        {
                                            this.CounterPartQty += CommonUtils.ConvertToLong(counterPartyItem.Qty);
                                        }
                                        if (report.ParentFlag == "1")
                                        {
                                            this.CounterPartList.Insert(0, counterPartyItem);
                                        }
                                        else
                                        {
                                            this.CounterPartList.Add(counterPartyItem);
                                        }
                                        if (report.CounterPartyVenues == "CNEX"
                                               || report.CounterPartyVenues == "TPSC"
                                               || report.CounterPartyVenues == "CCTB"
                                               || report.CounterPartyVenues == "CTTJ"
                                               || report.CounterPartyVenues == "UEDA"
                                               || report.CounterPartyVenues == "PTCN")
                                        {
                                            DataRow dr = counterPartyItem.Datas.NewRow();
                                            dr[BookEditItemViewModel.EXECID_FIELD] = report.ExecID;
                                            dr[BookEditItemViewModel.SOURCETRADER_FIELD] = report.CounterPartyVenues;
                                            dr[BookEditItemViewModel.COUNTER_PARTY_ID_FIELD] = report.CounterPartyID;
                                            dr[BookEditItemViewModel.COUNTER_PARTY_SUB_ID_FIELD] = report.CounterPartySubID;
                                            dr[BookEditItemViewModel.COUNTER_PARTY_NAME_FIELD] = report.CounterPartyName;
                                            dr[BookEditItemViewModel.COUNTER_PARTY_SUB_NAME_FIELD] = report.CounterPartySubName;
                                            dr[BookEditItemViewModel.YIELD_FIELD] = CommonUtils.ConvertToDecimal(report.Yield).ToString("N4");
                                            decimal qty = CommonUtils.ConvertToDecimal(report.LastQty);
                                            dr[BookEditItemViewModel.QTY_FIELD] = qty;
                                            dr[BookEditItemViewModel.EDIT_QTY_FIELD] = qty;
                                            this.CounterPartQty += qty;
                                            dr[BookEditItemViewModel.SETTLETYPE_FIELD] = report.SettlType;
                                            dr[BookEditItemViewModel.TRADEDATE_FIELD] = report.TradeDate;
                                            dr[BookEditItemViewModel.SETTLDATE_FIELD] = report.SettlDate;
                                            dr[BookEditItemViewModel.CREATEDATE_FIELD] = report.TransactTime;
                                            string createdate = CommonUtils.ConvertToDateTime(CommonUtils.ConvertToString(dr[BookEditItemViewModel.CREATEDATE_FIELD])).ToString("yyyy-MM-dd");
                                            string currentdate = DateTime.Now.ToString("yyyy-MM-dd");
                                            if (createdate.Equals(currentdate))
                                            {
                                                dr[BookEditItemViewModel.CREATEDATEVIS_FIELD] = "0";
                                            }
                                            else
                                            {
                                                dr[BookEditItemViewModel.CREATEDATEVIS_FIELD] = "1";
                                            }
                                            dr[BookEditItemViewModel.DCSEXEID_FIELD] = report.DcsexecID;
                                            dr[BookEditItemViewModel.BROKEREXECID_FIELD] = report.BrokerExecID;


                                            string yield = CommonUtils.ConvertToString(dr[BookEditItemViewModel.YIELD_FIELD]);
                                            if (!this.IsUnderline && !string.IsNullOrEmpty(yield) && !Yield.Equals(yield))
                                            {
                                                dr[BookEditItemViewModel.PRICE_WARN_BG] = "1";
                                            }
                                            else
                                                dr[BookEditItemViewModel.PRICE_WARN_BG] = string.Empty;

                                            string temSettle = CommonUtils.ConvertToString(dr[BookEditItemViewModel.SETTLETYPE_FIELD]);
                                            if (!this.IsUnderline && !string.IsNullOrEmpty(temSettle) && !SettlType.Equals(temSettle))
                                            {
                                                dr[BookEditItemViewModel.SETTLE_WARN_BG] = "1";
                                            }
                                            else
                                                dr[BookEditItemViewModel.SETTLE_WARN_BG] = string.Empty;
                                            if (!LocalEnv.ConfInfo.IsDcsBroker.ContainsKey(report.CounterPartyVenues))
                                            {
                                                dr[BookEditItemViewModel.BROKER_REMARK_FIELD] = report.Info3;

                                            }
                                            if (!string.IsNullOrEmpty(report.TradeMethod))
                                            {
                                                if (!string.IsNullOrEmpty(SecurityID))
                                                {
                                                    if (SecurityID.EndsWith("IB"))//银行间债券
                                                    {
                                                        dr[BookEditItemViewModel.TRADEMETHOD_FIELD] = ComboSourceUtil.Instance.FindEnum(report.TradeMethod, "trademethod");
                                                    }
                                                    else//交易所债券SH,SZ
                                                    {
                                                        dr[BookEditItemViewModel.TRADEMETHOD_FIELD] = report.TradeMethod;
                                                    }
                                                }
                                            }

                                            counterPartyItem.Datas.Rows.Add(dr);
                                            counterPartyItem.Info7 = this.Info7;
                                            counterPartyItem.IsTipVis = this.IsTipVis;
                                        }
                                    }
                                }

                                //foreach (ExecutionReport report in _originalDatas)
                                for (int i = _originalDatas.Count - 1; i >= 0; i--)
                                {
                                    ExecutionReport report = _originalDatas[i];
                                    if (usedList.Contains(report.ExecID))
                                    {
                                        continue;
                                    }
                                    if (report.CounterPartyVenues == "CNEX"
                                       || report.CounterPartyVenues == "TPSC"
                                       || report.CounterPartyVenues == "CCTB"
                                       || report.CounterPartyVenues == "CTTJ"
                                       || report.CounterPartyVenues == "PTCN"
                                       || report.CounterPartyVenues == "UEDA"
                                       || (report.SalesAgentFlag == "1"))
                                    {
                                        IEnumerable<BookEditItemViewModel> findList = this.CounterPartList.Where(
                                            item =>
                                            {
                                                if (report.CounterPartyVenues == "CNEX"
                                                   || report.CounterPartyVenues == "TPSC"
                                                   || report.CounterPartyVenues == "CCTB"
                                                   || report.CounterPartyVenues == "CTTJ"
                                                   || report.CounterPartyVenues == "UEDA"
                                                   || report.CounterPartyVenues == "PTCN")
                                                {
                                                    return item.PanelType == GetPanelType(report.CounterpartySalesAgentFlag, report.CounterPartyVenues) && item.Broker == report.CounterPartyVenues;
                                                }
                                                else
                                                {
                                                    //if (report.SalesAgentFlag == "1" && report.TimeInForce != "3")
                                                    //{
                                                    //    return item.PanelType == GetPanelType(report.SalesAgentFlag, report.Venues);
                                                    //}
                                                    //else
                                                    //{
                                                    return item.PanelType == GetPanelType(report.SalesAgentFlag, report.Venues) && item.SubName == report.PartySubName;
                                                    //}
                                                }
                                            });
                                        if (findList != null && findList.Count() > 0)
                                        {
                                            BookEditItemViewModel editItem = findList.First();
                                            DataRow dr = editItem.Datas.NewRow();
                                            dr[BookEditItemViewModel.EXECID_FIELD] = report.ExecID;
                                            dr[BookEditItemViewModel.SOURCETRADER_FIELD] = report.CounterPartyVenues;
                                            dr[BookEditItemViewModel.COUNTER_PARTY_ID_FIELD] = report.CounterPartyID;
                                            dr[BookEditItemViewModel.COUNTER_PARTY_NAME_FIELD] = report.CounterPartyName;
                                            dr[BookEditItemViewModel.COUNTER_PARTY_SUB_ID_FIELD] = report.CounterPartySubID;
                                            dr[BookEditItemViewModel.COUNTER_PARTY_SUB_NAME_FIELD] = report.CounterPartySubName;
                                            dr[BookEditItemViewModel.YIELD_FIELD] = CommonUtils.ConvertToDecimal(report.Yield).ToString("N4");
                                            decimal qty = CommonUtils.ConvertToDecimal(report.LastQty);
                                            dr[BookEditItemViewModel.QTY_FIELD] = qty;
                                            dr[BookEditItemViewModel.EDIT_QTY_FIELD] = qty;
                                            this.CounterPartQty += qty;
                                            dr[BookEditItemViewModel.SETTLETYPE_FIELD] = report.SettlType;
                                            dr[BookEditItemViewModel.TRADEDATE_FIELD] = report.TradeDate;
                                            dr[BookEditItemViewModel.SETTLDATE_FIELD] = report.SettlDate;
                                            dr[BookEditItemViewModel.CREATEDATE_FIELD] = report.TransactTime;
                                            string createdate = CommonUtils.ConvertToDateTime(CommonUtils.ConvertToString(dr[BookEditItemViewModel.CREATEDATE_FIELD])).ToString("yyyy-MM-dd");
                                            string currentdate = DateTime.Now.ToString("yyyy-MM-dd");
                                            if (createdate.Equals(currentdate))
                                            {
                                                dr[BookEditItemViewModel.CREATEDATEVIS_FIELD] = "0";
                                            }
                                            else
                                            {
                                                dr[BookEditItemViewModel.CREATEDATEVIS_FIELD] = "1";
                                            }
                                            dr[BookEditItemViewModel.DCSEXEID_FIELD] = report.DcsexecID;
                                            dr[BookEditItemViewModel.BROKEREXECID_FIELD] = report.BrokerExecID;


                                            string yield = CommonUtils.ConvertToString(dr[BookEditItemViewModel.YIELD_FIELD]);
                                            if (!this.IsUnderline && !string.IsNullOrEmpty(yield) && !Yield.Equals(yield))
                                            {
                                                dr[BookEditItemViewModel.PRICE_WARN_BG] = "1";
                                            }
                                            else
                                                dr[BookEditItemViewModel.PRICE_WARN_BG] = string.Empty;


                                            string temSettle = CommonUtils.ConvertToString(dr[BookEditItemViewModel.SETTLETYPE_FIELD]);
                                            if (!this.IsUnderline && !string.IsNullOrEmpty(temSettle) && !SettlType.Equals(temSettle))
                                            {
                                                dr[BookEditItemViewModel.SETTLE_WARN_BG] = "1";
                                            }
                                            else
                                                dr[BookEditItemViewModel.SETTLE_WARN_BG] = string.Empty;
                                            if (!LocalEnv.ConfInfo.IsDcsBroker.ContainsKey(report.CounterPartyVenues))
                                            {
                                                dr[BookEditItemViewModel.BROKER_REMARK_FIELD] = report.Info3;

                                            }
                                            if (!string.IsNullOrEmpty(report.TradeMethod))
                                            {
                                                if (!string.IsNullOrEmpty(SecurityID))
                                                {
                                                    if (SecurityID.EndsWith("IB"))//银行间债券
                                                    {
                                                        dr[BookEditItemViewModel.TRADEMETHOD_FIELD] = ComboSourceUtil.Instance.FindEnum(report.TradeMethod, "trademethod");
                                                    }
                                                    else//交易所债券SH,SZ
                                                    {
                                                        dr[BookEditItemViewModel.TRADEMETHOD_FIELD] = report.TradeMethod;
                                                    }
                                                }
                                            }
                                            editItem.Info7 = this.Info7;
                                            editItem.IsTipVis = this.IsTipVis;
                                            if (!string.IsNullOrEmpty(report.OrigOrdPrice))
                                                editItem.Yield = CommonUtils.ConvertToDecimal(report.OrigOrdPrice).ToString("N4");

                                            //editItem.Yield = string.IsNullOrEmpty(report.OrigOrdPrice) ? Price : CommonUtils.ConvertToDecimal(report.OrigOrdPrice).ToString("N4");
                                            editItem.Datas.Rows.Add(dr);
                                        }
                                    }
                                }
                            }));
                        }
                    }
                    catch (Exception e)
                    {
                        LogHelper.Error("LoadingDatas", e);
                    }
                });
            }
        }


        /// <summary>
        /// 获取面板类型
        /// </summary>
        /// <param name="salesAgentFlag"></param>
        /// <param name="venues"></param>
        /// <returns></returns>
        private string GetPanelType(string salesAgentFlag, string venues)
        {
            if (salesAgentFlag == "1")
            {
                if (IsUnderline)
                {
                    return Constants.IDB_UNDERLINE_PART;
                }
                else
                {
                    return Constants.IDB_PART;
                }
            }
            else
            {
                if (venues == "ODM"
                    || venues == "MARKET")
                {
                    if (IsUnderline)
                    {
                        return Constants.MARKET_UNDERLINE_PART;
                    }
                    else
                    {
                        return Constants.MARKET_PART;
                    }
                }
                else if (venues == "TPSC"
                    || venues == "CTTJ"
                    || venues == "CNEX"
                    || venues == "CCTB"
                    || venues == "UEDA"
                    || venues == "PTCN")
                {
                    if (IsUnderline)
                    {
                        return Constants.BROKER_UNDERLINE_PART;
                    }
                    else
                    {
                        return Constants.BROKER_PART;
                    }
                }
                else if (venues == "TBOOK"
                    || venues == "MANUAL"
                    || venues == "QUANT")
                {
                    if (IsUnderline)
                    {
                        return Constants.SELF_UNDERLINE_PART;
                    }
                    else
                    {
                        return Constants.SELF_PART;
                    }
                }

            }
            return string.Empty;
        }

        /// <summary>
        /// 初始化线下补入界面
        /// </summary>
        private void InitUnderline(ExecutionReport subReport = null)
        {
            // 本方只有1家， 【销售】或者【自营】
            BookEditItemViewModel item = new BookEditItemViewModel();
            item.IsUnderline = IsUnderline;
            item.VenuesTypeGW = "OFFLINE";
            item.Venues = "TBOOK";
            item.IsSelf = true;
            item.PanelType = (subReport == null || string.IsNullOrEmpty(subReport.SalesAgentFlag)) ? Constants.IDB_UNDERLINE_PART : subReport.SalesAgentFlag;
            item.PanelTypeDic = item.SelfPanelTypeDic;

            if (subReport != null)
            {

                StcTbookUsers user = null;
                if (!string.IsNullOrEmpty(subReport.PartySubName))
                {
                    if (subReport.SalesAgentFlag.Equals(Constants.IDB_UNDERLINE_PART))
                    {
                        user = BookHandler.GetUserByName(subReport.PartySubName, true);
                    }
                    else
                    {
                        user = BookHandler.GetUserByName(subReport.PartySubName, false);

                    }

                    item.SaleItem = user;
                    if (user != null && subReport.SalesAgentFlag == Constants.SELF_PART)
                        item.SubName = user.Name;
                }

                item.PanelType = subReport.SalesAgentFlag;

                if (subReport.SalesAgentFlag != Constants.SELF_PART)
                {
                    DataRow dr = item.Datas.NewRow();
                    dr[BookEditItemViewModel.EXECID_FIELD] = subReport.ExecID;
                    dr[BookEditItemViewModel.SOURCETRADER_FIELD] = "TBOOK";//subReport.CounterPartyVenues;
                    dr[BookEditItemViewModel.COUNTER_PARTY_ID_FIELD] = subReport.CounterPartyID;

                    if (string.IsNullOrEmpty(subReport.Venues) || !subReport.Venues.Equals("IDB", StringComparison.OrdinalIgnoreCase))
                    {
                        dr[BookEditItemViewModel.COUNTER_PARTY_NAME_FIELD] = subReport.InstitutionName;
                        //dr[BookEditItemViewModel.COUNTER_PARTY_SUB_NAME_FIELD] = subReport.PartySubName;
                    }

                    //dr[BookEditItemViewModel.COUNTER_PARTY_NAME_FIELD] = subReport.PartySubName;// CounterPartyName;
                    dr[BookEditItemViewModel.COUNTER_PARTY_SUB_ID_FIELD] = subReport.CounterPartySubID;
                    //dr[BookEditItemViewModel.COUNTER_PARTY_SUB_NAME_FIELD] = subReport.InstitutionName;// CounterPartySubName;
                    dr[BookEditItemViewModel.YIELD_FIELD] = CommonUtils.ConvertToDecimal(subReport.Yield).ToString("N4");
                    decimal idbQty = CommonUtils.ConvertToDecimal(subReport.LastQty);
                    this.SelfQty = idbQty;
                    dr[BookEditItemViewModel.QTY_FIELD] = idbQty;
                    dr[BookEditItemViewModel.SETTLETYPE_FIELD] = subReport.SettlType;

                    string tradeDate = CommonUtils.CalTradeDate(subReport.SettlType);
                    dr[BookEditItemViewModel.EDIT_SETTLETYPE_DATE_FIELD] = CommonUtils.ConvertToDateTime2(tradeDate);

                    DateTime settlDateTime;
                    int selectIndex;
                    CommonUtils.SetSettleType(subReport.SettlType, tradeDate, out settlDateTime, out selectIndex);
                    dr[BookEditItemViewModel.EDIT_SETTLETYPE_FIELD] = selectIndex;


                    dr[BookEditItemViewModel.TRADEDATE_FIELD] = tradeDate;//subReport.TradeDate;
                    dr[BookEditItemViewModel.SETTLDATE_FIELD] = subReport.SettlDate;
                    dr[BookEditItemViewModel.CREATEDATE_FIELD] = subReport.TransactTime;
                    string createdate = CommonUtils.ConvertToDateTime(CommonUtils.ConvertToString(dr[BookEditItemViewModel.CREATEDATE_FIELD])).ToString("yyyy-MM-dd");
                    string currentdate = DateTime.Now.ToString("yyyy-MM-dd");
                    if (createdate.Equals(currentdate))
                    {
                        dr[BookEditItemViewModel.CREATEDATEVIS_FIELD] = "0";
                    }
                    else
                    {
                        dr[BookEditItemViewModel.CREATEDATEVIS_FIELD] = "1";
                    }

                    string yield = CommonUtils.ConvertToString(dr[BookEditItemViewModel.YIELD_FIELD]);
                    if (!this.IsUnderline && !string.IsNullOrEmpty(yield) && !Yield.Equals(yield))
                    {
                        dr[BookEditItemViewModel.PRICE_WARN_BG] = "1";
                    }
                    else
                        dr[BookEditItemViewModel.PRICE_WARN_BG] = string.Empty;

                    string temSettle = CommonUtils.ConvertToString(dr[BookEditItemViewModel.SETTLETYPE_FIELD]);
                    if (!this.IsUnderline && !string.IsNullOrEmpty(temSettle) && !SettlType.Equals(temSettle))
                    {
                        dr[BookEditItemViewModel.SETTLE_WARN_BG] = "1";
                    }
                    else
                        dr[BookEditItemViewModel.SETTLE_WARN_BG] = string.Empty;
                    if (!string.IsNullOrEmpty(subReport.TradeMethod))
                    {
                        if (!string.IsNullOrEmpty(SecurityID))
                        {
                            if (SecurityID.EndsWith("IB"))//银行间债券
                            {
                                dr[BookEditItemViewModel.TRADEMETHOD_FIELD] = ComboSourceUtil.Instance.FindEnum(subReport.TradeMethod, "trademethod");
                            }
                            else//交易所债券SH,SZ
                            {
                                dr[BookEditItemViewModel.TRADEMETHOD_FIELD] = subReport.TradeMethod;
                            }
                        }
                    }
                    dr[BookEditItemViewModel.PARTY_SUB_ID_FIELD] = subReport.PartySubID;
                    dr[BookEditItemViewModel.PARTY_SUB_NAME_FIELD] = subReport.PartySubName;
                    item.Datas.Rows.Add(dr);
                }
            }





            //item.Name = "山西证券";
            //item.SubName = "张三";
            //item.Yield = 3.24m;
            //DataRow dr = item.Datas.NewRow();
            //dr[BookEditItemViewModel.SALES_AGENT_FLAG_FIELD] = "1";
            //dr[BookEditItemViewModel.COUNTER_PARTY_NAME_FIELD] = "中国银行";
            //dr[BookEditItemViewModel.COUNTER_PARTY_SUB_NAME_FIELD] = "李丹丹";
            //dr[BookEditItemViewModel.YIELD_FIELD] = 3.34m;
            //dr[BookEditItemViewModel.QTY_FIELD] = 2000;
            //dr[BookEditItemViewModel.SETTLETYPE_FIELD] = "08-20 +0";
            //item.Datas.Rows.Add(dr);
            this.SelfList.Add(item);

            // 对手方默认放【销售】
            //this.AddCounterPart();
            if (subReport != null)//从tradebook点价发起
                InitCounterPart(subReport);
            else
                this.AddCounterPart();// 对手方默认放【销售】

        }

        /// <summary>
        /// 初始化对手方数据
        /// </summary>
        /// <param name="subReport"></param>
        private void InitCounterPart(ExecutionReport subReport)
        {
            BookEditItemViewModel item = new BookEditItemViewModel();
            item.IsUnderline = IsUnderline;
            item.VenuesTypeGW = "OFFLINE";
            item.Venues = "TBOOK";
            item.PanelType = (subReport == null || string.IsNullOrEmpty(subReport.SalesAgentFlag)) ? Constants.IDB_UNDERLINE_PART : subReport.SalesAgentFlag;
            item.DelBtnVisibility = Visibility.Visible;
            item.PanelTypeDic = item.CounterPanelTypeDic;


            if (subReport != null)
            {
                StcTbookUsers user = null;
                if (!string.IsNullOrEmpty(subReport.PartySubName))
                {
                    if (subReport.SalesAgentFlag.Equals(Constants.IDB_UNDERLINE_PART))
                    {
                        user = BookHandler.GetUserByName(subReport.PartySubName, true);
                    }
                    else
                    {
                        user = BookHandler.GetUserByName(subReport.PartySubName, false);

                    }

                    item.SaleItem = user;
                }


                DateTime settlDateTime;
                int selectIndex;
                CommonUtils.SetSettleType(subReport.SettlType, subReport.TradeDate, out settlDateTime, out selectIndex);
                string selectdate = CommonUtils.CalTradeDate(subReport.SettlType);//CommonUtils.CalSettlDate(string.Empty, subReport.SettlType, out newSettltype);
                item.SelectDate = CommonUtils.ConvertToDateTime2(selectdate);//settlDateTime;
                item.ComboBoxSelectIndex = selectIndex;

                DataRow dr = item.Datas.NewRow();
                dr[BookEditItemViewModel.EXECID_FIELD] = subReport.ExecID;

                dr[BookEditItemViewModel.COUNTER_PARTY_ID_FIELD] = subReport.CounterPartyID;
                dr[BookEditItemViewModel.COUNTER_PARTY_NAME_FIELD] = subReport.InstitutionName;// ;// CounterPartyName;
                dr[BookEditItemViewModel.COUNTER_PARTY_SUB_ID_FIELD] = subReport.CounterPartySubID;

                decimal idbQty = CommonUtils.ConvertToDecimal(subReport.LastQty);
                this.CounterPartQty = idbQty;

                dr[BookEditItemViewModel.TRADEDATE_FIELD] = string.IsNullOrEmpty(subReport.TradeDate) ? selectdate : string.Empty;
                dr[BookEditItemViewModel.SOURCETRADER_FIELD] = GetVenues(item.PanelType);
                if (!string.IsNullOrEmpty(subReport.Venues) && subReport.Venues.Equals("IDB", StringComparison.OrdinalIgnoreCase))
                {
                    item.PanelType = Constants.IDB_UNDERLINE_PART;
                    dr[BookEditItemViewModel.SOURCETRADER_FIELD] = Constants.IDB;
                    dr[BookEditItemViewModel.COUNTER_PARTY_SUB_NAME_FIELD] = subReport.TraderName;

                    //dr[BookEditItemViewModel.TRADEDATE_FIELD] = string.IsNullOrEmpty(subReport.TradeDate) ? selectdate : string.Empty;
                    
                }
                else if (subReport.SalesAgentFlag == Constants.SELF_PART)
                {
                    item.PanelType = Constants.SELF_UNDERLINE_PART;
                    item.Qty = idbQty.ToString();
                    
                }
                dr[BookEditItemViewModel.YIELD_FIELD] = CommonUtils.ConvertToDecimal(subReport.Yield).ToString("N4");
                
                dr[BookEditItemViewModel.QTY_FIELD] = idbQty;
                dr[BookEditItemViewModel.SETTLETYPE_FIELD] = subReport.SettlType;
                
                dr[BookEditItemViewModel.SETTLDATE_FIELD] = subReport.SettlDate;
                dr[BookEditItemViewModel.CREATEDATE_FIELD] = subReport.TransactTime;
                string createdate = CommonUtils.ConvertToDateTime(CommonUtils.ConvertToString(dr[BookEditItemViewModel.CREATEDATE_FIELD])).ToString("yyyy-MM-dd");
                string currentdate = DateTime.Now.ToString("yyyy-MM-dd");
                if (createdate.Equals(currentdate))
                {
                    dr[BookEditItemViewModel.CREATEDATEVIS_FIELD] = "0";
                }
                else
                {
                    dr[BookEditItemViewModel.CREATEDATEVIS_FIELD] = "1";
                }

                string yield = CommonUtils.ConvertToString(dr[BookEditItemViewModel.YIELD_FIELD]);
                if (!this.IsUnderline && !string.IsNullOrEmpty(yield) && !Yield.Equals(yield))
                {
                    dr[BookEditItemViewModel.PRICE_WARN_BG] = "1";
                }
                else
                    dr[BookEditItemViewModel.PRICE_WARN_BG] = string.Empty;

                string temSettle = CommonUtils.ConvertToString(dr[BookEditItemViewModel.SETTLETYPE_FIELD]);
                if (!this.IsUnderline && !string.IsNullOrEmpty(temSettle) && !SettlType.Equals(temSettle))
                {
                    dr[BookEditItemViewModel.SETTLE_WARN_BG] = "1";
                }
                else
                    dr[BookEditItemViewModel.SETTLE_WARN_BG] = string.Empty;
                if (!string.IsNullOrEmpty(subReport.TradeMethod))
                {
                    if (!string.IsNullOrEmpty(SecurityID))
                    {
                        if (SecurityID.EndsWith("IB"))//银行间债券
                        {
                            dr[BookEditItemViewModel.TRADEMETHOD_FIELD] = ComboSourceUtil.Instance.FindEnum(subReport.TradeMethod, "trademethod");
                        }
                        else//交易所债券SH,SZ
                        {
                            dr[BookEditItemViewModel.TRADEMETHOD_FIELD] = subReport.TradeMethod;
                        }
                    }
                }
                dr[BookEditItemViewModel.PARTY_SUB_ID_FIELD] = subReport.PartySubID;
                dr[BookEditItemViewModel.PARTY_SUB_NAME_FIELD] = subReport.PartySubName;

                item.Yield = subReport.Yield;

                item.Datas.Rows.Add(dr);


            }

            this.CounterPartList.Add(item);

        }

        /// <summary>
        ///  添加对手方， 最多10条
        /// </summary>
        private void AddCounterPart()
        {
            if (this.CounterPartList.Count <= 10)
            {
                BookEditItemViewModel item = new BookEditItemViewModel();
                item.IsUnderline = IsUnderline;
                item.VenuesTypeGW = "OFFLINE";
                item.Venues = "TBOOK";
                item.PanelType = Constants.IDB_UNDERLINE_PART;
                item.DelBtnVisibility = Visibility.Visible;
                item.PanelTypeDic = item.CounterPanelTypeDic;

                this.CounterPartList.Add(item);
            }
        }

        /// <summary>
        /// 删除对手方
        /// </summary>
        private void DelCounterPart(Button delBtn)
        {
            if (delBtn != null)
            {
                BookEditView bookEditView = CommonUtils.GetParentObject<BookEditView>(delBtn) as BookEditView;
                if (Common.View.Component.CustomMessageBox.Question("是否要删除当前记录?", "删除", bookEditView))
                {
                    BookEditItemViewModel bookEditItemViewModel = delBtn.DataContext as BookEditItemViewModel;

                    if (bookEditItemViewModel.PanelType.Contains(Constants.IDB_PART)
                        || bookEditItemViewModel.PanelType.Contains(Constants.BROKER_PART))
                    {
                        foreach (DataRow dr in bookEditItemViewModel.Datas.Rows)
                        {
                            this.CounterPartQty -= CommonUtils.ConvertToLong(dr[BookEditItemViewModel.QTY_FIELD] == null ? "0" : dr[BookEditItemViewModel.QTY_FIELD].ToString());
                        }
                    }
                    else
                    {
                        this.CounterPartQty -= CommonUtils.ConvertToLong(bookEditItemViewModel.Qty);
                    }

                    this.CounterPartList.Remove(bookEditItemViewModel);
                }
            }
        }


        private void ChangePanelType(ComboBox panelTypeCbc)
        {
            if (panelTypeCbc != null && panelTypeCbc.DataContext != null && panelTypeCbc.DataContext is BookEditItemViewModel)
            {
                BookEditItemViewModel itemVM = panelTypeCbc.DataContext as BookEditItemViewModel;
                if (itemVM.IsCancelChangePanelType == true)
                {
                    itemVM.IsCancelChangePanelType = false;
                    return;
                }
                BookEditView bookEditView = CommonUtils.GetParentObject<BookEditView>(panelTypeCbc) as BookEditView;
                if (Common.View.Component.CustomMessageBox.Question("是否要切换当前类型?", "切换", bookEditView) == false)
                {
                    itemVM.IsCancelChangePanelType = true;
                    itemVM.PanelType = itemVM.LastPanelType;
                    return;
                }

                ObservableCollection<BookEditItemViewModel> List;
                if (itemVM.IsSelf)
                {
                    List = this.SelfList;
                }
                else
                {
                    List = this.CounterPartList;
                }
                if (List != null && string.IsNullOrEmpty(itemVM.LastPanelType) == false && itemVM.PanelType.Contains("underline"))
                {
                    int index = List.IndexOf(itemVM);
                    if (index != -1)
                    {
                        // 就算数量
                        if (itemVM.IsSelf)
                        {
                            if (itemVM.PanelType.Contains(Constants.IDB_PART))
                            {
                                this.SelfQty = 0;
                            }
                            else if (itemVM.PanelType.Contains(Constants.SELF_PART))
                            {
                                this.SelfQty = this.QtyLong;//this.CounterPartQty;
                            }
                        }
                        else
                        {
                            // 就算数量
                            if (itemVM.LastPanelType.Contains(Constants.IDB_PART)
                                || itemVM.LastPanelType.Contains(Constants.BROKER_PART))
                            {
                                foreach (DataRow dr in itemVM.Datas.Rows)
                                {
                                    this.CounterPartQty -= CommonUtils.ConvertToLong(dr[BookEditItemViewModel.QTY_FIELD] == null ? "0" : dr[BookEditItemViewModel.QTY_FIELD].ToString());
                                }
                            }
                            else
                            {
                                this.CounterPartQty -= CommonUtils.ConvertToLong(itemVM.Qty);
                            }
                        }


                        // 切换
                        BookEditItemViewModel newItem = new BookEditItemViewModel();
                        newItem.IsUnderline = IsUnderline;
                        newItem.VenuesTypeGW = "OFFLINE";
                        newItem.Venues = GetVenues(itemVM.PanelType);
                        newItem.IsSelf = itemVM.IsSelf;
                        newItem.PanelType = itemVM.PanelType;
                        newItem.PanelTypeDic = itemVM.PanelTypeDic;
                        newItem.DelBtnVisibility = itemVM.DelBtnVisibility;
                        if (SecurityID.EndsWith("IB"))
                        {
                            newItem.PositionVisibility = "2";
                            newItem.SendPriceTypeVisibility = "0";
                            newItem.TradeMethod = "待定";
                        }
                        else
                        {
                            newItem.PositionVisibility = "0";
                            newItem.SendPriceTypeVisibility = "2";
                        }
                        List[index] = newItem;
                    }
                }
            }
        }

        private string CheckData(ObservableCollection<BookEditItemViewModel> datas)
        {
            string result = string.Empty;

            foreach (BookEditItemViewModel model in datas)
            {
                foreach (DataRow dr in model.Datas.Rows)
                {
                    string newYield = CommonUtils.ConvertToString(dr[BookEditItemViewModel.YIELD_FIELD]);
                    string newQty = dr[BookEditItemViewModel.QTY_FIELD].ToString();

                    if (CommonUtils.ConvertToString(dr[BookEditItemViewModel.COUNTER_PARTY_NAME_FIELD]).Contains(Constants.PUBLIC_ACCOUNT) == false && !model.PanelType.Contains(Constants.BROKER_PART))
                    {
                        if (string.IsNullOrEmpty(newYield) || string.IsNullOrEmpty(newQty))
                        {
                            return "收益率或成交量没有填写";
                        }
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(newQty))
                        {
                            return "收益率或成交量没有填写";
                        }
                    }
                }
            }

            return result;
        }

        List<ExecutionReport> _originalDatas;
        List<ExecutionReport> _editDatas;

        /// <summary>
        /// 提交按钮处理逻辑
        /// </summary>
        bool status = false;
        public string Submit()
        {
            string checkResult = this.CheckData(this.SelfList);

            if (!string.IsNullOrEmpty(checkResult))
                return checkResult;

            checkResult = this.CheckData(this.CounterPartList);

            if (!string.IsNullOrEmpty(checkResult))
                return checkResult;


            if (string.IsNullOrEmpty(this.Symbol))
            {
                return "债券代码不可为空";
            }

            if (this.QtyLong <= 0)
            {
                return "总量不符";
            }
            if (SelfQty != this.QtyLong)
            {
                return "本方总量不符";
            }
            if (CounterPartQty != this.QtyLong)
            {
                return "对手方总量不符";
            }

            //if (IsUnderline)
            //{
            if (this.SelfList[0].SaleItem == null)
            {
                return "请选择本方交易员";
            }
            string selfTraderID = this.SelfList[0].SubID == "--" ? null : this.SelfList[0].SubID;
            if (UnderlineVisibility == Visibility.Visible)
            {
                selfTraderID = this.SelfList[0].SaleItem == null ? string.Empty : this.SelfList[0].SaleItem.Userid;
            }
            bool isSelf = false;
            foreach (BookEditItemViewModel counterItem in this.SelfList)
            {
                if (counterItem.PanelType.Contains(Constants.IDB_PART))
                {
                    foreach (DataRow dr in counterItem.Datas.Rows)
                    {
                        string counterPartyName = dr[BookEditItemViewModel.COUNTER_PARTY_NAME_FIELD] as string;
                        if (string.IsNullOrEmpty(counterPartyName))
                        {
                            return "本方销售未填机构名";
                        }
                    }
                }
                if (counterItem.PanelType.Contains(Constants.SELF_PART))
                {
                    isSelf = true;
                }
            }

            List<string> counterTraderList = new List<string>();
            List<string> marketList = new List<string>();
            List<string> brokerList = new List<string>();
            status = false;
            foreach (BookEditItemViewModel counterItem in this.CounterPartList)
            {
                if (counterItem.PanelType.Contains(Constants.SELF_PART))
                {
                    isSelf = true;
                    break;
                }
            }
            if (isSelf && this.CounterPartList.Count() > 1)
            {
                status = BookMessageBox.Question1("自营交易包中，若本方 / 对手方机构数 > 1，将触发下游系统报错，建议拆分后提交。", "提示", "继续提交", "取消");
            }
            if (status || (!status && this.CounterPartList.Count() == 1) || !isSelf)
            {
                foreach (BookEditItemViewModel counterItem in this.CounterPartList)
                {
                    if (counterItem.PanelType.Contains(Constants.IDB_PART)
                        || counterItem.PanelType.Contains(Constants.SELF_PART))
                    {
                        if (counterItem.SaleItem == null)
                        {
                            return "请选择对手方交易员";
                        }
                        string counterTraderID = counterItem.SaleItem.Userid;
                        //if (selfTraderID == counterTraderID)
                        //{
                        //    return "本方和对手方交易员不可相同";
                        //}
                        //else
                        if (counterTraderList.Contains(counterTraderID))
                        {
                            return "对手方销售或自营中存在重复交易员";
                        }
                        else
                        {
                            if (counterItem.PanelType.Contains(Constants.IDB_PART))
                            {
                                if (string.IsNullOrEmpty(counterItem.Yield))
                                {
                                    return "对手方销售未填成交价";
                                }
                                if (counterItem.Datas.Rows.Count == 0)
                                {
                                    return "对手方销售未填机构名、成交价、成交量、交易日期";
                                }
                                if (!BookHandler.Instance.CheckUserIsExit(counterItem.SaleItem, true))
                                    return string.Format("对手方销售不存在交易员({0})", counterItem.SaleItem.Name);
                                foreach (DataRow dr in counterItem.Datas.Rows)
                                {
                                    string counterPartyName = dr[BookEditItemViewModel.COUNTER_PARTY_NAME_FIELD] as string;
                                    if (string.IsNullOrEmpty(counterPartyName))
                                    {
                                        return "对手方销售未填机构名";
                                    }
                                }
                            }
                            else if (counterItem.PanelType.Contains(Constants.SELF_PART))
                            {
                                if (string.IsNullOrEmpty(counterItem.Yield))
                                {
                                    return "对手方自营未填成交价";
                                }
                                if (string.IsNullOrEmpty(counterItem.Qty))
                                {
                                    return "对手方自营未填成交量";
                                }
                                if (!BookHandler.Instance.CheckUserIsExit(counterItem.SaleItem, false))
                                    return string.Format("对手方自营中不存在交易员({0})", counterItem.SaleItem.Name);
                            }
                        }
                        counterTraderList.Add(counterTraderID);
                    }
                    else if (counterItem.PanelType.Contains(Constants.MARKET_PART))
                    {
                        if (string.IsNullOrEmpty(counterItem.Name))
                        {
                            return "对手方市场未填机构名";
                        }
                        if (marketList.Contains(counterItem.SubName))
                        {
                            return "对手方市场中存在重复交易员";
                        }
                        if (string.IsNullOrEmpty(counterItem.Yield))
                        {
                            return "对手方市场未填成交价";
                        }
                        if (string.IsNullOrEmpty(counterItem.Qty))
                        {
                            return "对手方市场未填成交量";
                        }
                        marketList.Add(counterItem.SubName);
                    }
                    else if (counterItem.PanelType.Contains(Constants.BROKER_PART))
                    {
                        if (brokerList.Contains(counterItem.Broker))
                        {
                            return "对手方存在重复经纪商类型";
                        }
                        if (counterItem.Datas.Rows.Count == 0)
                        {
                            return "对手方经纪商未填报价机构、成交价、成交量、交易日期";
                        }
                        foreach (DataRow dr in counterItem.Datas.Rows)
                        {
                            string counterPartyName = dr[BookEditItemViewModel.COUNTER_PARTY_NAME_FIELD] as string;
                            if (string.IsNullOrEmpty(counterPartyName))
                            {
                                return "对手方经纪商未填机构名";
                            }
                            long qty = CommonUtils.ConvertToLong(dr[BookEditItemViewModel.QTY_FIELD].ToString());
                            if (qty <= 0)
                            {
                                return "对手方经纪商未填成交量";
                            }
                        }
                        brokerList.Add(counterItem.Broker);
                    }
                }
                //}

                Submit_Underline();

                if (IsUnderline == false)
                {
                    Submit_Online();
                }

                // 发送
                //BlockUpdateReq updateReq = new BlockUpdateReq();
                //updateReq.blockdatalist = _editDatas;

                int recordNum = _editDatas.Count;
                foreach (ExecutionReport er in _editDatas)
                {
                    er.RecordNum = recordNum.ToString();
                }

                PResult result = BookMgtHandler.Instance.BlockUpdate(_editDatas);//ClientApi.Instance.BlockUpdate(_editDatas);

                if (result == null)
                {
                    return "发送失败";
                }
                else
                {
                    if (result.Code == "0000")
                    {
                        return string.Empty;
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(result.Msg))
                        {
                            return $"{result.Code}错误";
                        }
                        else
                        {
                            return result.Msg;
                        }
                    }
                }
            }
            else
            {
                return "重新填写";
            }
        }

        /// <summary>
        /// 提交， 适用于线下补录 新建
        /// </summary>
        /// <returns>error message</returns>
        private string Submit_Underline()
        {
            List<ExecutionReport> datas = new List<ExecutionReport>();

            string blockNum = this.BlockNum;

            string cumQty = string.Empty;

            // 本方只有一条
            string partyID = string.Empty;
            string partyName = "山西证券";
            string salesAgentFlag = string.Empty;
            string partySubID = string.Empty;
            string partySubName = string.Empty;
            string venues = string.Empty;

            foreach (BookEditItemViewModel book in this.SelfList)
            {
                salesAgentFlag = GetSalesAgentFlag(book.PanelType);

                venues = GetVenues(book.PanelType);
                //partyID = book.SaleItem == null ? string.Empty : book.SaleItem.Biztypeid;
                //partySubID = book.SaleItem == null ? string.Empty : book.SaleItem.Userid;
                //partySubName = book.SaleItem == null ? string.Empty : book.SaleItem.Name;
                if (UnderlineVisibility == Visibility.Visible)
                {
                    partyID = book.SaleItem == null ? string.Empty : book.SaleItem.Biztypeid;
                    partySubID = book.SaleItem == null ? string.Empty : book.SaleItem.Userid;
                    partySubName = book.SaleItem == null ? string.Empty : book.SaleItem.Name;
                }
                else
                {
                    partyID = book.InstitutionID;
                    partyName = book.Name;
                    partySubID = book.SubID == "--" ? null : book.SubID;
                    partySubName = book.SubName == "--" ? null : book.SubName;
                }

                #region 本方销售的对手方
                foreach (DataRow dr in book.Datas.Rows)
                {
                    if (CommonUtils.ConvertToString(dr[BookEditItemViewModel.ISEDIT_FIELD]) == "0"
                        && CommonUtils.ConvertToLong(dr[BookEditItemViewModel.QTY_FIELD] == null ? "0" : dr[BookEditItemViewModel.QTY_FIELD].ToString()) == 0)
                    {
                        continue;
                    }
                    ExecutionReport obj1 = new ExecutionReport();
                    obj1.ParentFlagQty = this.Qty;
                    obj1.ParentFlag = "0";
                    obj1.Terminal = terminal;
                    //obj1.ExecID = $"execid-{++index}"; // 先自己生成一下
                    obj1.ExecID = CommonUtils.ConvertToString(dr[BookEditItemViewModel.EXECID_FIELD]);
                    obj1.SalesAgentFlag = "1";
                    obj1.BlockNum = blockNum;// 先自己瞎遍一个
                    obj1.Venues = CommonUtils.ConvertToString(dr[BookEditItemViewModel.SOURCETRADER_FIELD]);

                    obj1.SecurityID = this.SecurityID;
                    obj1.Symbol = this.Symbol;
                    string yield = CommonUtils.ConvertToString(dr[BookEditItemViewModel.YIELD_FIELD]);
                    obj1.Yield = obj1.Price = yield == "--" ? null : yield.Replace(",", "");// 成交价
                    obj1.LastQty = CommonUtils.ConvertToString(dr[BookEditItemViewModel.QTY_FIELD]);
                    string settltype = CommonUtils.ConvertToString(dr[BookEditItemViewModel.SETTLETYPE_FIELD]);
                    obj1.TradeDate = CommonUtils.ConvertToString(dr[BookEditItemViewModel.TRADEDATE_FIELD]);
                    DateTime createTime = CommonUtils.ConvertToDateTime(CommonUtils.ConvertToString(dr[BookEditItemViewModel.CREATEDATE_FIELD]));
                    //todo计算交割日
                    obj1.SettlDate = CommonUtils.CalSettlDate(createTime.ToString("yyyyMMdd"), settltype, out settltype);
                    obj1.SettlType = settltype == "--" ? null : settltype;
                    obj1.Side = GetReserveSide(this.Side);

                    obj1.PartyID = partyID;
                    obj1.PartyName = partyName;
                    obj1.CounterPartyID = CommonUtils.ConvertToString(dr[BookEditItemViewModel.COUNTER_PARTY_ID_FIELD]);
                    obj1.CounterpartySalesAgentFlag = "1";
                    string counterpartysubid = CommonUtils.ConvertToString(dr[BookEditItemViewModel.COUNTER_PARTY_SUB_ID_FIELD]);
                    obj1.CounterPartySubID = counterpartysubid == "--" ? null : counterpartysubid;
                    obj1.CounterPartyName = CommonUtils.ConvertToString(dr[BookEditItemViewModel.COUNTER_PARTY_NAME_FIELD]);
                    string counterpartysubname = CommonUtils.ConvertToString(dr[BookEditItemViewModel.COUNTER_PARTY_SUB_NAME_FIELD]);
                    obj1.CounterPartySubName = counterpartysubname == "--" ? null : counterpartysubname;
                    obj1.CounterPartyVenues = CommonUtils.ConvertToString(dr[BookEditItemViewModel.SOURCETRADER_FIELD]);
                    obj1.UpdateUser = LocalEnv.UserInfo.UserName;
                    obj1.SendingTime = DateTimeEx.ServerTime.ToString(Constants.YYYYMMDDHHMMSSFFF_DATEFORMAT);
                    obj1.UpdateTime = DateTimeEx.ServerTime.ToString(Constants.YYYYMMDDHHMMSSFFF_DATEFORMAT);
                    obj1.VenueTypeGW = book.VenuesTypeGW;
                    obj1.UserID = book.SaleItem.Userid;//LocalEnv.UserInfo.UserId;//partySubID;
                    obj1.UserName = book.SaleItem.Username;// LocalEnv.UserInfo.UserName;//partySubName;
                    obj1.BizTypeID = partyID;
                    if (obj1.CounterPartyName.Contains("公账"))
                    {
                        obj1.DealType = "1";
                        //obj1.CounterPartyID = BookHandler.publicUser == null ? string.Empty : BookHandler.publicUser.Userid;
                        obj1.CounterPartySubName = obj1.CounterPartyName;
                        obj1.CounterPartySubID = BookHandler.publicUser == null ? string.Empty : BookHandler.publicUser.Userid;

                        obj1.CounterPartyName = obj1.CounterPartyName;//string.Empty;
                        obj1.CounterPartyID = string.Empty;

                        if (IsUnderline)
                        {
                            obj1.TradeDate = this.CounterPartList[0].SelectDate.ToString("yyyyMMdd");
                            createTime = this.CounterPartList[0].CreateTime == null ? DateTimeEx.ServerTime : CommonUtils.ConvertToDateTime(this.CounterPartList[0].CreateTime);
                            settltype = CommonUtils.IsSpotSettle(createTime, this.CounterPartList[0].SelectDate, this.CounterPartList[0].ComboBoxSelectIndex);
                            if (string.IsNullOrEmpty(settltype))
                            {
                                settltype = this.CounterPartList[0].SelectDate.ToString("MM.dd") + (this.CounterPartList[0].ComboBoxSelectIndex == 0 ? "+0" : "+1");
                            }
                            //obj1.SettlDate = book.SettlDate; //todo计算交割日
                            obj1.SettlDate = CommonUtils.CalSettlDate(createTime.ToString("yyyyMMdd"), settltype, out settltype);
                            obj1.SettlType = settltype;
                            obj1.Yield = this.CounterPartList[0].Yield == "--" ? null : this.CounterPartList[0].Yield.Replace(",", "");
                            obj1.Price = this.CounterPartList[0].LastPx == "--" ? null : this.CounterPartList[0].LastPx.Replace(",", "");
                            obj1.PartySubID = book.SaleItem == null ? string.Empty : book.SaleItem.Userid;
                            obj1.PartySubName = book.SaleItem == null ? string.Empty : book.SaleItem.Name;
                        }
                        else
                        {
                            obj1.TradeDate = this.TradeDate;
                            obj1.SettlDate = this.SettleDate;
                            obj1.SettlType = this.SettlType;
                            obj1.Yield = this.Yield.Replace(",", "");

                            //线上场景：销售对Broker/Market
                            bool isBroker = false;
                            for (int i = 0; i < this.CounterPartList.Count; i++)
                            {
                                BookEditItemViewModel counterBook = this.CounterPartList[i];

                                if (counterBook.PanelType.Contains(Constants.BROKER_PART) || counterBook.PanelType.Contains(Constants.MARKET_PART) || counterBook.PanelType.Contains(Constants.IDB_PART))
                                {
                                    isBroker = true;
                                    break;
                                }
                            }
                            if (isBroker)
                            {
                                obj1.PartySubID = book.SaleItem == null ? string.Empty : book.SaleItem.Userid;
                                obj1.PartySubName = book.SaleItem == null ? string.Empty : book.SaleItem.Name;
                            }
                        }
                    }
                    else
                    {
                        if (IsUnderline)
                        {
                            obj1.DealType = "2";// 成交
                            obj1.PartySubID = book.SaleItem == null ? string.Empty : book.SaleItem.Userid;
                            obj1.PartySubName = book.SaleItem == null ? string.Empty : book.SaleItem.Name;
                        }
                        else
                        {
                            obj1.DealType = "2";
                            if (string.IsNullOrEmpty(CommonUtils.ConvertToString(dr[BookEditItemViewModel.EXECID_FIELD])))
                            {
                                obj1.PartySubID = book.SaleItem == null ? string.Empty : book.SaleItem.Userid;
                                obj1.PartySubName = book.SaleItem == null ? string.Empty : book.SaleItem.Name;
                            }
                            else
                            {
                                obj1.PartySubID = CommonUtils.ConvertToString(dr[BookEditItemViewModel.PARTY_SUB_ID_FIELD]);
                                obj1.PartySubName = CommonUtils.ConvertToString(dr[BookEditItemViewModel.PARTY_SUB_NAME_FIELD]);
                            }
                        }
                    }
                    string tradeMethod = CommonUtils.ConvertToString(dr[BookEditItemViewModel.TRADEMETHOD_FIELD]);
                    if ("0".Equals(CommonUtils.ConvertToString(book.SendPriceTypeVisibility)))
                    {
                        obj1.TradeMethod = ComboSourceUtil.Instance.FindEnum(tradeMethod, "trademethod1");
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(SecurityID))
                        {
                            if (SecurityID.EndsWith("IB"))//银行间债券
                            {
                                if (!string.IsNullOrEmpty(tradeMethod))
                                {
                                    obj1.TradeMethod = ComboSourceUtil.Instance.FindEnum(tradeMethod, "trademethod1");
                                }
                            }
                            else//交易所债券SH,SZ
                            {
                                obj1.TradeMethod = tradeMethod;
                            }
                        }
                    }
                    datas.Add(obj1);
                }
                #endregion
            }

            for (int i = 0; i < this.CounterPartList.Count; i++)
            {
                BookEditItemViewModel book = this.CounterPartList[i];

                #region 对手方是broker
                if (book.PanelType.Contains(Constants.BROKER_PART))
                {
                    for (int j = 0; j < book.Datas.Rows.Count; j++)
                    {
                        DataRow dr = book.Datas.Rows[j];
                        ExecutionReport obj1 = new ExecutionReport();
                        obj1.ParentFlagQty = this.Qty;
                        obj1.Terminal = terminal;
                        obj1.ExecID = CommonUtils.ConvertToString(dr[BookEditItemViewModel.EXECID_FIELD]); // 先自己生成一下
                        obj1.ParentFlag = (i == 0 && j == 0) ? "1" : string.Empty;
                        obj1.SalesAgentFlag = salesAgentFlag;
                        obj1.BlockNum = blockNum;// 先自己瞎遍一个
                        obj1.Venues = venues;
                        obj1.DcsexecID = CommonUtils.ConvertToString(dr[BookEditItemViewModel.DCSEXEID_FIELD]);

                        obj1.SecurityID = this.SecurityID;
                        obj1.Symbol = this.Symbol;
                        string yield = CommonUtils.ConvertToString(dr[BookEditItemViewModel.YIELD_FIELD]);
                        if (IsUnderline)
                        {
                            obj1.DealType = "2";//成交
                            obj1.Yield = obj1.Price = yield == "--" ? null : yield.Replace(",", "");//报价 （线上和线下??取值不同）
                        }
                        else
                        {
                            obj1.Yield = obj1.LastPx = obj1.Price = yield == "--" ? null : yield.Replace(",", ""); //成交价
                            obj1.Price = this.Price;
                        }
                        obj1.LastQty = CommonUtils.ConvertToString(dr[BookEditItemViewModel.QTY_FIELD]);
                        string settltype = CommonUtils.ConvertToString(dr[BookEditItemViewModel.SETTLETYPE_FIELD]);
                        obj1.TradeDate = CommonUtils.ConvertToString(dr[BookEditItemViewModel.TRADEDATE_FIELD]);

                        DateTime createTime = CommonUtils.ConvertToDateTime(CommonUtils.ConvertToString(dr[BookEditItemViewModel.CREATEDATE_FIELD]));
                        //todo计算交割日
                        //if (book.Broker.Equals("TPSC"))
                        //{
                        //    obj1.SettlDate = CommonUtils.CalSettlDate(obj1.TradeDate, settltype, out settltype);
                        //}
                        //else
                        //{
                        obj1.SettlDate = CommonUtils.CalSettlDate(createTime.ToString("yyyyMMdd"), settltype, out settltype);
                        //}
                        obj1.SettlType = settltype == "--" ? null : settltype;
                        obj1.Side = this.Side;

                        obj1.PartyID = partyID;

                        string brokerPartySubId = CommonUtils.ConvertToString(dr[BookEditItemViewModel.COUNTER_PARTY_SUB_ID_FIELD]);
                        string brokerPartySubName = CommonUtils.ConvertToString(dr[BookEditItemViewModel.COUNTER_PARTY_SUB_NAME_FIELD]);
                        obj1.PartySubID = UnderlineVisibility == Visibility.Visible ? brokerPartySubId : partySubID;
                        obj1.PartyName = UnderlineVisibility == Visibility.Visible ? brokerPartySubName : partyName;
                        obj1.PartySubName = partySubName;
                        if (obj1.ParentFlag == "1" && string.IsNullOrEmpty(obj1.PartySubID) && UnderlineVisibility == Visibility.Visible)
                            obj1.PartySubID = partySubID;

                        obj1.CounterPartyID = CommonUtils.ConvertToString(dr[BookEditItemViewModel.COUNTER_PARTY_ID_FIELD]);
                        string counterpartysubid = CommonUtils.ConvertToString(dr[BookEditItemViewModel.COUNTER_PARTY_SUB_ID_FIELD]);
                        obj1.CounterPartySubID = counterpartysubid == "--" ? null : counterpartysubid;
                        obj1.CounterPartyName = CommonUtils.ConvertToString(dr[BookEditItemViewModel.COUNTER_PARTY_NAME_FIELD]);
                        string counterpartysubname = CommonUtils.ConvertToString(dr[BookEditItemViewModel.COUNTER_PARTY_SUB_NAME_FIELD]);
                        obj1.CounterPartySubName = counterpartysubname == "--" ? null : counterpartysubname;
                        obj1.CounterpartySalesAgentFlag = "0";
                        obj1.CounterPartyVenues = CommonUtils.ConvertToString(dr[BookEditItemViewModel.SOURCETRADER_FIELD]); ;
                        if (obj1.ParentFlag == "1")
                        {
                            obj1.Info7 = !string.IsNullOrEmpty(Info7) ? Info7.Replace("量不平；", "").Replace("自营机构超过1家，建议拆分；", "") : "";

                            if (status)
                            {
                                obj1.Info7 = !string.IsNullOrEmpty(obj1.Info7) ? string.Format("{0}；自营机构超过1家，建议拆分；", obj1.Info7) : "自营机构超过1家，建议拆分；";
                            }
                            datas.Insert(0, obj1);
                        }
                        else
                        {
                            datas.Add(obj1);
                        }
                        obj1.UpdateUser = LocalEnv.UserInfo.UserName;
                        obj1.SendingTime = DateTimeEx.ServerTime.ToString(Constants.YYYYMMDDHHMMSSFFF_DATEFORMAT);
                        obj1.UpdateTime = DateTimeEx.ServerTime.ToString(Constants.YYYYMMDDHHMMSSFFF_DATEFORMAT);
                        obj1.VenueTypeGW = book.VenuesTypeGW;
                        obj1.UserID = SelfList.Count > 0 ? SelfList[0].SaleItem.Userid : string.Empty;//LocalEnv.UserInfo.UserId;//partySubID;
                        obj1.UserName = SelfList.Count > 0 ? SelfList[0].SaleItem.Username : string.Empty;// LocalEnv.UserInfo.UserName;// partySubName;
                        obj1.BizTypeID = partyID;
                        obj1.DcsexecID = CommonUtils.ConvertToString(dr[BookEditItemViewModel.DCSEXEID_FIELD]);
                        obj1.BrokerExecID = CommonUtils.ConvertToString(dr[BookEditItemViewModel.BROKEREXECID_FIELD]);
                        obj1.Info3 = CommonUtils.ConvertToString(dr[BookEditItemViewModel.BROKER_REMARK_FIELD]);
                        string tradeMethod = CommonUtils.ConvertToString(dr[BookEditItemViewModel.TRADEMETHOD_FIELD]);
                        if ("0".Equals(CommonUtils.ConvertToString(book.SendPriceTypeVisibility)))
                        {
                            obj1.TradeMethod = ComboSourceUtil.Instance.FindEnum(tradeMethod, "trademethod1");
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(SecurityID))
                            {
                                if (SecurityID.EndsWith("IB"))//银行间债券
                                {
                                    if (!string.IsNullOrEmpty(tradeMethod))
                                    {
                                        obj1.TradeMethod = ComboSourceUtil.Instance.FindEnum(tradeMethod, "trademethod1");
                                    }
                                }
                                else//交易所债券SH,SZ
                                {
                                    obj1.TradeMethod = tradeMethod;
                                }
                            }
                        }
                    }
                }
                #endregion
                #region 对手方不是broker
                else
                {
                    #region idb 对手方子单
                    long counterpartyIDBTotalQty = 0;
                    if (book.PanelType.Contains(Constants.IDB_PART))
                    {
                        foreach (DataRow dr in book.Datas.Rows)
                        {
                            if (CommonUtils.ConvertToString(dr[BookEditItemViewModel.ISEDIT_FIELD]) == "0"
                                && CommonUtils.ConvertToLong(dr[BookEditItemViewModel.QTY_FIELD] == null ? "0" : dr[BookEditItemViewModel.QTY_FIELD].ToString()) == 0)
                            {
                                continue;
                            }
                            ExecutionReport idbObj = new ExecutionReport();
                            idbObj.ParentFlagQty = this.Qty;
                            idbObj.Terminal = terminal;
                            idbObj.ExecID = CommonUtils.ConvertToString(dr[BookEditItemViewModel.EXECID_FIELD]); // 先自己生成一下
                            idbObj.ParentFlag = "0";
                            idbObj.BlockNum = blockNum;// 先自己瞎遍一个


                            idbObj.SecurityID = this.SecurityID;
                            idbObj.Symbol = this.Symbol;
                            string yield = CommonUtils.ConvertToString(dr[BookEditItemViewModel.YIELD_FIELD]);
                            idbObj.Yield = idbObj.Price = yield == "--" ? null : yield.Replace(",", "");//成交价
                            long qty = CommonUtils.ConvertToLong(dr[BookEditItemViewModel.QTY_FIELD] == null ? "0" : dr[BookEditItemViewModel.QTY_FIELD].ToString());
                            counterpartyIDBTotalQty += qty;
                            idbObj.LastQty = CommonUtils.ConvertToString(dr[BookEditItemViewModel.QTY_FIELD]);
                            string settlType = CommonUtils.ConvertToString(dr[BookEditItemViewModel.SETTLETYPE_FIELD]);
                            idbObj.TradeDate = CommonUtils.ConvertToString(dr[BookEditItemViewModel.TRADEDATE_FIELD]);
                            
                            DateTime createTime = CommonUtils.ConvertToDateTime(CommonUtils.ConvertToString(dr[BookEditItemViewModel.CREATEDATE_FIELD]));
                            //todo计算交割日
                            idbObj.SettlDate = CommonUtils.CalSettlDate(createTime.ToString("yyyyMMdd"), settlType, out settlType);
                            idbObj.SettlType = settlType == "--" ? null : settlType;
                            idbObj.Side = this.Side;

                            if (IsUnderline)
                            {
                                idbObj.PartyID = book.SaleItem == null ? string.Empty : book.SaleItem.Biztypeid;
                                idbObj.PartySubName = "山西证券";
                                idbObj.PartySubID = book.SaleItem == null ? string.Empty : book.SaleItem.Userid;
                                idbObj.PartySubName = book.SaleItem == null ? string.Empty : book.SaleItem.Name;
                            }
                            else
                            {
                                idbObj.PartyID = book.InstitutionID;
                                idbObj.PartyName = book.Name;
                                idbObj.PartySubID = book.SubID;
                                idbObj.PartySubName = book.SubName;
                            }

                            idbObj.SalesAgentFlag = "1";
                            idbObj.Venues = CommonUtils.ConvertToString(dr[BookEditItemViewModel.SOURCETRADER_FIELD]);//"IDB"

                            idbObj.CounterPartyID = CommonUtils.ConvertToString(dr[BookEditItemViewModel.COUNTER_PARTY_ID_FIELD]);
                            idbObj.CounterpartySalesAgentFlag = "1";
                            string counterpartysubid = CommonUtils.ConvertToString(dr[BookEditItemViewModel.COUNTER_PARTY_SUB_ID_FIELD]);
                            idbObj.CounterPartySubID = counterpartysubid == "--" ? null : counterpartysubid;
                            idbObj.CounterPartyName = CommonUtils.ConvertToString(dr[BookEditItemViewModel.COUNTER_PARTY_NAME_FIELD]);
                            string counterpartysubname = CommonUtils.ConvertToString(dr[BookEditItemViewModel.COUNTER_PARTY_SUB_NAME_FIELD]);
                            idbObj.CounterPartySubName = counterpartysubname == "--" ? null : counterpartysubname;
                            idbObj.CounterPartyVenues = CommonUtils.ConvertToString(dr[BookEditItemViewModel.SOURCETRADER_FIELD]);


                            idbObj.UpdateUser = LocalEnv.UserInfo.UserName;
                            idbObj.SendingTime = DateTimeEx.ServerTime.ToString(Constants.YYYYMMDDHHMMSSFFF_DATEFORMAT);
                            idbObj.UpdateTime = DateTimeEx.ServerTime.ToString(Constants.YYYYMMDDHHMMSSFFF_DATEFORMAT);
                            idbObj.VenueTypeGW = book.VenuesTypeGW;
                            idbObj.UserID = book.SaleItem.Userid;//LocalEnv.UserInfo.UserId;//partySubID;
                            idbObj.UserName = book.SaleItem.Username;//LocalEnv.UserInfo.UserName;//partySubName;
                            idbObj.BizTypeID = partyID;
                            if (idbObj.CounterPartyName.Contains("公账"))
                            {
                                idbObj.DealType = "1";
                                //idbObj.CounterPartyID = BookHandler.publicUser == null ? string.Empty : BookHandler.publicUser.Userid;

                                idbObj.CounterPartySubName = idbObj.CounterPartyName;
                                idbObj.CounterPartySubID = BookHandler.publicUser == null ? string.Empty : BookHandler.publicUser.Userid;

                                idbObj.CounterPartyName = idbObj.CounterPartyName;//string.Empty;
                                idbObj.CounterPartyID = string.Empty;

                                if (IsUnderline)
                                {
                                    idbObj.TradeDate = book.SelectDate.ToString("yyyyMMdd");
                                    createTime = book.CreateTime == null ? DateTimeEx.ServerTime : CommonUtils.ConvertToDateTime(book.CreateTime);
                                    string settltype = CommonUtils.IsSpotSettle(createTime, book.SelectDate, book.ComboBoxSelectIndex);
                                    if (string.IsNullOrEmpty(settltype))
                                    {
                                        settltype = book.SelectDate.ToString("MM.dd") + (book.ComboBoxSelectIndex == 0 ? "+0" : "+1");
                                    }
                                    //obj1.SettlDate = book.SettlDate; //todo计算交割日
                                    idbObj.SettlDate = CommonUtils.CalSettlDate(createTime.ToString("yyyyMMdd"), settltype, out settltype);
                                    idbObj.SettlType = settltype;
                                    idbObj.Yield = book.Yield.Replace(",", "");
                                }
                                else
                                {
                                    idbObj.TradeDate = this.TradeDate;
                                    idbObj.SettlDate = this.SettleDate;
                                    idbObj.SettlType = this.SettlType;
                                    idbObj.Yield = this.Yield.Replace(",", "");
                                }
                            }
                            else
                            {
                                //if (IsUnderline)
                                //{
                                idbObj.DealType = "2"; // 成交
                                //}
                            }
                            string tradeMethod = CommonUtils.ConvertToString(dr[BookEditItemViewModel.TRADEMETHOD_FIELD]);

                            if ("0".Equals(CommonUtils.ConvertToString(book.SendPriceTypeVisibility)))
                            {
                                idbObj.TradeMethod = ComboSourceUtil.Instance.FindEnum(tradeMethod, "trademethod1");
                            }
                            else
                            {
                                if (!string.IsNullOrEmpty(SecurityID))
                                {
                                    if (SecurityID.EndsWith("IB"))//银行间债券
                                    {
                                        if (!string.IsNullOrEmpty(tradeMethod))
                                        {
                                            idbObj.TradeMethod = ComboSourceUtil.Instance.FindEnum(tradeMethod, "trademethod1");
                                        }
                                    }
                                    else//交易所债券SH,SZ
                                    {
                                        idbObj.TradeMethod = tradeMethod;
                                    }
                                }
                            }
                            if (idbObj.ParentFlag == "1")
                            {
                                idbObj.Info7 = !string.IsNullOrEmpty(Info7) ? Info7.Replace("量不平；", "").Replace("自营机构超过1家，建议拆分；", "") : "";
                                if (status)
                                {
                                    idbObj.Info7 = !string.IsNullOrEmpty(idbObj.Info7) ? string.Format("{0}；自营机构超过1家，建议拆分；", idbObj.Info7) : "自营机构超过1家，建议拆分；";
                                }
                            }
                            
                            datas.Add(idbObj);
                        }
                    }
                    #endregion
                    // 母单
                    ExecutionReport obj1 = new ExecutionReport();
                    obj1.ParentFlagQty = this.Qty;
                    obj1.Terminal = terminal;
                    obj1.ExecID = CommonUtils.ConvertToString(book.ExecID);// 先自己生成一下
                    obj1.ParentFlag = i == 0 ? "1" : string.Empty;
                    obj1.BlockNum = blockNum;// 先自己瞎遍一个

                    obj1.SecurityID = this.SecurityID;
                    obj1.Symbol = this.Symbol;
                    if (IsUnderline)
                    {
                        obj1.Yield = obj1.Price = book.Yield == "--" ? null : book.Yield.Replace(",", ""); //报价 （线上和线下??取值不同）
                    }
                    else
                    {
                        obj1.Yield = obj1.LastPx = obj1.Price = this.Yield.Replace(",", ""); //成交价
                        obj1.Price = this.Price;
                    }

                    //调试,对手方是idb计算总数， 其他直接取
                    if (book.PanelType.Contains(Constants.IDB_PART))
                    {
                        obj1.LastQty = counterpartyIDBTotalQty.ToString();
                    }
                    else
                    {
                        obj1.LastQty = book.Qty;
                    }
                    obj1.TradeDate = book.TradeDate;
                    obj1.SettlType = book.SettlType;//调试，对手方idb无，
                    obj1.SettlDate = book.SettlDate;

                    if (IsUnderline)
                    {
                        obj1.TradeDate = book.SelectDate.ToString("yyyyMMdd");
                        DateTime createTime = book.CreateTime == null ? DateTimeEx.ServerTime : CommonUtils.ConvertToDateTime(book.CreateTime);
                        string settltype = CommonUtils.IsSpotSettle(createTime, book.SelectDate, book.ComboBoxSelectIndex);
                        if (string.IsNullOrEmpty(settltype))
                        {
                            settltype = book.SelectDate.ToString("MM.dd") + (book.ComboBoxSelectIndex == 0 ? "+0" : "+1");
                        }
                        //obj1.SettlDate = book.SettlDate; //todo计算交割日
                        obj1.SettlDate = CommonUtils.CalSettlDate(createTime.ToString("yyyyMMdd"), settltype, out settltype);
                        obj1.SettlType = settltype;

                    }



                    obj1.Side = this.Side;

                    obj1.SalesAgentFlag = salesAgentFlag;
                    obj1.Venues = venues;
                    obj1.PartyID = partyID;
                    obj1.PartyName = partyName;
                    obj1.PartySubID = partySubID;
                    obj1.PartySubName = partySubName;



                    if (book.PanelType.Contains(Constants.IDB_PART)
                        || book.PanelType.Contains(Constants.SELF_PART))
                    {
                        if (IsUnderline)
                        {
                            obj1.DealType = "1";
                            obj1.CounterPartyID = book.SaleItem == null ? string.Empty : book.SaleItem.Biztypeid;
                            obj1.CounterPartyName = partyName;
                            obj1.CounterPartySubID = book.SaleItem == null ? string.Empty : book.SaleItem.Userid;
                            obj1.CounterPartySubName = book.SaleItem == null ? string.Empty : book.SaleItem.Name;
                        }
                        else
                        {
                            obj1.CounterPartyID = book.InstitutionID;
                            obj1.CounterPartyName = book.Name;
                            obj1.CounterPartySubID = book.SubID;
                            obj1.CounterPartySubName = book.SubName;
                        }
                    }
                    else if (book.PanelType.Contains(Constants.MARKET_PART))
                    {
                        obj1.DealType = "2";
                        obj1.CounterPartyID = book.InstitutionID;
                        obj1.CounterPartyName = book.Name;
                        obj1.CounterPartySubName = book.SubName;
                        obj1.Info3 = book.Remark;
                        if (SecurityID.EndsWith("IB"))
                        {
                            obj1.TradeMethod = ComboSourceUtil.Instance.FindEnum(book.TradeMethod, "trademethod1");
                        }
                        else
                        {
                            obj1.TradeMethod = book.TradeMethod;
                        }
                    }

                    obj1.CounterpartySalesAgentFlag = GetSalesAgentFlag(book.PanelType);
                    obj1.CounterPartyVenues = GetVenues(book.PanelType);

                    if (obj1.ParentFlag == "1")
                    {
                        obj1.Info7 = !string.IsNullOrEmpty(Info7) ? Info7.Replace("量不平；", "").Replace("自营机构超过1家，建议拆分；", "") : "";
                        if (status)
                        {
                            obj1.Info7 = !string.IsNullOrEmpty(obj1.Info7) ? string.Format("{0}；自营机构超过1家，建议拆分；", obj1.Info7) : "自营机构超过1家，建议拆分；";
                        }
                        datas.Insert(0, obj1);
                    }
                    else
                    {
                        datas.Add(obj1);
                    }
                    obj1.UpdateUser = LocalEnv.UserInfo.UserName;
                    obj1.SendingTime = DateTimeEx.ServerTime.ToString(Constants.YYYYMMDDHHMMSSFFF_DATEFORMAT);
                    obj1.UpdateTime = DateTimeEx.ServerTime.ToString(Constants.YYYYMMDDHHMMSSFFF_DATEFORMAT);
                    obj1.VenueTypeGW = book.VenuesTypeGW;
                    obj1.UserID = SelfList.Count > 0 ? SelfList[0].SaleItem.Userid : string.Empty;////partySubID;
                    obj1.UserName = SelfList.Count > 0 ? SelfList[0].SaleItem.Username : string.Empty; //partySubName;
                    obj1.BizTypeID = partyID;
                }
                #endregion
            }

            this._editDatas = datas;

            return string.Empty;
        }

        /// <summary>
        /// 提交线上修改数据
        /// </summary>
        private void Submit_Online()
        {
            if (this._editDatas != null)
            {
                foreach (ExecutionReport er in this._editDatas)
                {
                    string execID = er.ExecID;

                    IEnumerable<ExecutionReport> findList = _originalDatas.Where(item => item.ExecID == er.ExecID);
                    if (findList != null && findList.Count() > 0)
                    {
                        ExecutionReport originalEr = findList.First();
                        CopyExecutionReport(er, originalEr);

                        if (string.IsNullOrEmpty(er.DealType))
                        {
                            if (!string.IsNullOrEmpty(er.ParentFlag) && er.ParentFlag == "1")
                                er.DealType = "1";//1:内部划转, 1:成交
                            else if (!string.IsNullOrEmpty(er.PartyName))
                                er.DealType = er.PartyName.Contains("公账") ? "1" : "2";
                        }
                        else if (er.DealType == "2" && !string.IsNullOrEmpty(er.ParentFlag) && er.ParentFlag == "1")
                        {
                            if (!(er.CounterPartyVenues == "CNEX" || er.CounterPartyVenues == "TPSC"
                                                 || er.CounterPartyVenues == "CCTB"
                                                 || er.CounterPartyVenues == "CTTJ"
                                                 || er.CounterPartyVenues == "PTCN"
                                                 || er.CounterPartyVenues == "UEDA"
                                                 || er.CounterPartyVenues == "ODM"
                                                 || er.CounterPartyVenues == "MARKET"))//对手方非Broker/市场
                            {
                                er.DealType = "1";
                            }
                            er.Info7 = !string.IsNullOrEmpty(er.Info7)?er.Info7.Replace("量不平；", ""):"";
                        }
                        else if (er.DealType == "2" && !string.IsNullOrEmpty(er.ParentFlag) && er.ParentFlag == "0" && !string.IsNullOrEmpty(er.CounterPartyName))
                        {
                            er.DealType = er.CounterPartyName.Contains("公账") ? "1" : "2";
                        }
                        else if (er.DealType == "1" && !string.IsNullOrEmpty(er.ParentFlag) && er.ParentFlag == "0")
                        {
                            er.DealType = er.CounterPartyName.Contains("公账") ? "1" : "2";
                        }

                    }
                    else
                    {
                        if (string.IsNullOrEmpty(er.DealType))
                        {
                            if (!string.IsNullOrEmpty(er.PartyName))
                                er.DealType = er.PartyName.Contains("公账") ? "1" : "2";
                            else
                                er.DealType = "2";

                            er.ExecID = string.Empty;
                        }

                        //er.UserID = er.PartySubID;//LocalEnv.UserInfo.UserId;
                        //er.UserName = er.PartySubName;//LocalEnv.UserInfo.UserName;
                    }
                }
            }
        }

        // todo少备注字段, todo计算交割日
        public string[] IgnoreArr = new string[] { "Comparer","Count","Keys","Values","Item",
            "Venues", "Terminal", "SecurityID" , "Symbol", "Side", "Price", "Yield","LastQty", "ExecID","PartyID", "PartyName", "PartySubID", "PartySubName","CounterPartyID",
        "CounterPartyName","CounterPartySubID","CounterPartySubName", "UpdateTime","UpdateUser","SalesAgentFlag","CounterpartySalesAgentFlag","RecordNum", "SendingTime",
            "CounterPartyVenues","ParentFlag","SettlType","Info3","VenueTypeGW","BrokerExecID","TradeDate","SettlDate","ParentFlagQty","DcsexecID","UserID","UserName","BizTypeID"
        ,"TradeMethod"};
        private void CopyExecutionReport(ExecutionReport newEr, ExecutionReport originalEr)
        {
            try
            {
                PropertyInfo[] props = typeof(ExecutionReport).GetProperties(BindingFlags.Public | BindingFlags.Instance);
                foreach (PropertyInfo pi in props)
                {
                    //Console.WriteLine(pi.Name);
                    if (IgnoreArr.Contains(pi.Name) == false)
                    {
                        try
                        {
                            if (IsUnderline && pi.Name == "DealType")
                            {
                                continue;
                            }
                            pi.SetValue(newEr, pi.GetValue(originalEr));
                        }
                        catch (Exception e)
                        {
                            LogHandler.Error(e);
                            LogHandler.Info(pi.Name);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                LogHelper.Error(e);
            }
        }

        /// <summary>
        /// 判断是否销售
        /// </summary>
        /// <param name="panelType"></param>
        /// <returns></returns>
        private string GetSalesAgentFlag(string panelType)
        {
            if (panelType == Constants.IDB_PART
                || panelType == Constants.IDB_UNDERLINE_PART)
            {
                return "1";
            }
            else
            {
                return "0";
            }
        }

        /// <summary>
        ///  如果线上的IDB或者Manual，quant经过包编辑，都会被改成TBook
        /// </summary>
        /// <param name="panelType"></param>
        /// <returns></returns>
        private string GetVenues(string panelType)
        {
            if (panelType.Contains(Constants.IDB_PART))
            {
                return "TBOOK";
            }
            else if (panelType.Contains(Constants.SELF_PART))
            {
                return "TBOOK";
            }
            else if (panelType.Contains(Constants.MARKET_PART))
            {
                return "MARKET";
            }
            return string.Empty;
        }

        /// <summary>
        ///  获得反方向
        /// </summary>
        /// <param name="side"></param>
        /// <returns></returns>
        private string GetReserveSide(string side)
        {
            return side == "1" ? "2" : "1";
        }

        /// <summary>
        /// 测试数据
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private List<ExecutionReport> GetTestList(string type)
        {
            List<ExecutionReport> datas = new List<ExecutionReport>();

            #region 1. 销售对销售例子
            ExecutionReport obj1 = new ExecutionReport();
            obj1.ExecID = "execid-1";
            obj1.ParentFlag = "1";
            obj1.SalesAgentFlag = "1";
            obj1.BlockNum = this.BlockNum;
            obj1.Venues = "IDB";
            obj1.Side = "2";
            obj1.PartyID = "sx-1";
            obj1.PartyName = "山西证券";
            obj1.PartySubName = "张三";
            obj1.SecurityID = "180210.IB";
            obj1.Symbol = "18国债28";
            obj1.Yield = "2.34";
            obj1.CumQty = "4000";
            obj1.SettlType = "1";
            obj1.CounterPartyID = "sx-2";
            obj1.CounterPartyName = "山西证券";
            obj1.CounterPartySubName = "李四";
            obj1.CounterpartySalesAgentFlag = "1";
            obj1.CounterPartyVenues = "IDB";
            obj1.SendingTime = "09-01 13:00";
            obj1.UpdateTime = "09-22 09:00";
            datas.Add(obj1);

            ExecutionReport obj2 = new ExecutionReport();
            obj2.ExecID = "execid-2";
            obj2.TimeInForce = "";
            obj2.SalesAgentFlag = "1";
            obj2.BlockNum = this.BlockNum;
            obj2.Venues = "IDB";
            obj2.Side = "1"; // 注意方向
            obj2.PartyID = "sx-1";
            obj2.PartyName = "山西证券";
            obj2.PartySubName = "张三";
            obj2.SecurityID = "180210.IB";
            obj2.Symbol = "18国债28";
            obj2.Yield = "2.34";
            obj2.CumQty = "2000"; // 注意数量
            obj2.SettlType = "1";
            obj2.CounterPartyID = "zh-1";
            obj2.CounterPartyName = "中国银行";
            obj2.CounterPartySubName = "李丹丹";
            obj2.CounterpartySalesAgentFlag = "1";
            obj2.CounterPartyVenues = "IDB";
            datas.Add(obj2);

            ExecutionReport obj3 = new ExecutionReport();
            obj3.ExecID = "execid-3";
            obj3.TimeInForce = "";
            obj3.SalesAgentFlag = "1";
            obj3.BlockNum = this.BlockNum;
            obj3.Venues = "IDB";
            obj3.Side = "1";
            obj3.PartyID = "sx-1";
            obj3.PartyName = "山西证券";
            obj3.PartySubName = "张三";
            obj3.SecurityID = "180210.IB";
            obj3.Symbol = "18国债28";
            obj3.Yield = "2.34";
            obj3.CumQty = "2000";
            obj3.SettlType = "1";
            obj3.CounterPartyID = "gf-1";
            obj3.CounterPartyName = "广发银行";
            obj3.CounterPartySubName = "张顺";
            obj3.CounterpartySalesAgentFlag = "0";
            obj3.CounterPartyVenues = "MANUAL";
            datas.Add(obj3);



            ExecutionReport obj4 = new ExecutionReport();
            obj4.ExecID = "execid-4";
            obj4.TimeInForce = "";
            obj4.SalesAgentFlag = "1";
            obj4.BlockNum = this.BlockNum;
            obj4.Venues = "IDB";
            obj4.Side = "2"; //注意方向
            obj4.PartyID = "sx-2";
            obj4.PartyName = "山西证券";// 注意对手方
            obj4.PartySubName = "李四";// 注意对手方
            obj4.SecurityID = "180210.IB";
            obj4.Symbol = "18国债28";
            obj4.Yield = "2.34";
            obj4.CumQty = "2000";// 注意数量
            obj4.SettlType = "1";
            obj4.CounterPartyID = "zs-1";
            obj4.CounterPartyName = "招商银行";
            obj4.CounterPartySubName = "张星";
            obj4.CounterpartySalesAgentFlag = "1";
            obj4.CounterPartyVenues = "IDB";
            datas.Add(obj4);

            ExecutionReport obj5 = new ExecutionReport();
            obj5.ExecID = "execid-5";
            obj5.TimeInForce = "";
            obj5.SalesAgentFlag = "1";
            obj5.BlockNum = this.BlockNum;
            obj5.Venues = "IDB";
            obj5.Side = "2";
            obj5.PartyID = "sx-2";
            obj5.PartyName = "山西证券";
            obj5.PartySubName = "李四";
            obj5.SecurityID = "180210.IB";
            obj5.Symbol = "18国债28";
            obj5.Yield = "2.34";
            obj5.CumQty = "2000";
            obj5.SettlType = "1";
            obj5.CounterPartyID = "df-1";
            obj5.CounterPartyName = "东方证券";
            obj5.CounterPartySubName = "东方";
            obj5.CounterpartySalesAgentFlag = "0";
            obj5.CounterPartyVenues = "MANUAL";
            datas.Add(obj5);
            #endregion
            #region 2. 销售对自营例子
            //ExecutionReport obj1 = new ExecutionReport();
            //obj1.ExecID = "execid-1";
            //obj1.TimeInForce = "3";
            //obj1.SalesAgentFlag = "1";
            //obj1.BlockNum = this.BlockNum;
            //obj1.Venues = "IDB";
            //obj1.Side = "2";
            //obj1.PartyID = "sx-1";
            //obj1.PartyName = "山西证券";
            //obj1.PartySubName = "张三";
            //obj1.SecurityID = "180210.IB";
            //obj1.Symbol = "18国债28";
            //obj1.Yield = "2.34";
            //obj1.CumQty = "4000";
            //obj1.SettlType = "1";
            //obj1.CounterPartyID = "sx-3";
            //obj1.CounterPartyName = "山西证券";
            //obj1.CounterPartySubName = "王五";
            //obj1.CounterpartySalesAgentFlag = "0";
            //obj1.CounterPartyVenues = "MANUAL";
            //obj1.SendingTime = "09-01 13:00";
            //obj1.UpdateTime = "09-22 09:00";
            //datas.Add(obj1);

            //ExecutionReport obj2 = new ExecutionReport();
            //obj2.ExecID = "execid-2";
            //obj2.TimeInForce = "";
            //obj2.SalesAgentFlag = "1";
            //obj2.BlockNum = this.BlockNum;
            //obj2.Venues = "IDB";
            //obj2.Side = "1"; // 注意方向
            //obj2.PartyID = "sx-1";
            //obj2.PartyName = "山西证券";
            //obj2.PartySubName = "张三";
            //obj2.SecurityID = "180210.IB";
            //obj2.Symbol = "18国债28";
            //obj2.Yield = "2.34";
            //obj2.CumQty = "2000"; // 注意数量
            //obj2.SettlType = "1";
            //obj2.CounterPartyID = "zh-1";
            //obj2.CounterPartyName = "中国银行";
            //obj2.CounterPartySubName = "李丹丹";
            //obj2.CounterpartySalesAgentFlag = "1";
            //obj2.CounterPartyVenues = "IDB";
            //datas.Add(obj2);

            //ExecutionReport obj3 = new ExecutionReport();
            //obj3.ExecID = "execid-3";
            //obj3.TimeInForce = "";
            //obj3.SalesAgentFlag = "0";
            //obj3.BlockNum = this.BlockNum;
            //obj3.Venues = "IDB";
            //obj3.Side = "1";
            //obj3.PartyID = "sx-1";
            //obj3.PartyName = "山西证券";
            //obj3.PartySubName = "张三";
            //obj3.SecurityID = "180210.IB";
            //obj3.Symbol = "18国债28";
            //obj3.Yield = "2.34";
            //obj3.CumQty = "2000";
            //obj3.SettlType = "1";
            //obj3.CounterPartyID = "gf-1";
            //obj3.CounterPartyName = "广发银行";
            //obj3.CounterPartySubName = "张顺";
            //obj3.CounterpartySalesAgentFlag = "0";
            //obj3.CounterPartyVenues = "IDB";
            //datas.Add(obj3);

            #endregion

            #region 3. 销售对broker例子
            //ExecutionReport obj1 = new ExecutionReport();
            //obj1.ExecID = "execid-1";
            //obj1.TimeInForce = "3";
            //obj1.SalesAgentFlag = "1";
            //obj1.BlockNum = this.BlockNum;
            //obj1.Venues = "IDB";
            //obj1.Side = "2";
            //obj1.PartyID = "sx-1";
            //obj1.PartyName = "山西证券";
            //obj1.PartySubName = "张三";
            //obj1.SecurityID = "180210.IB";
            //obj1.Symbol = "18国债28";
            //obj1.Yield = "2.34";
            //obj1.CumQty = "2000";
            //obj1.SettlType = "1";
            //obj1.CounterPartyID = "sx-2";
            //obj1.CounterPartyName = "山西证券";
            //obj1.CounterPartySubName = "李四";
            //obj1.CounterpartySalesAgentFlag = "0";
            //obj1.CounterPartyVenues = "PTCN";
            //obj1.SendingTime = "09-01 13:00";
            //obj1.UpdateTime = "09-22 09:00";
            //datas.Add(obj1);

            //ExecutionReport obj2 = new ExecutionReport();
            //obj2.ExecID = "execid-2";
            //obj2.TimeInForce = "";
            //obj2.SalesAgentFlag = "1";
            //obj2.BlockNum = this.BlockNum;
            //obj2.Venues = "IDB";
            //obj2.Side = "1"; // 注意方向
            //obj2.PartyID = "sx-1";
            //obj2.PartyName = "山西证券";
            //obj2.PartySubName = "张三";
            //obj2.SecurityID = "180210.IB";
            //obj2.Symbol = "18国债28";
            //obj2.Yield = "2.34";
            //obj2.CumQty = "2000"; // 注意数量
            //obj2.SettlType = "1";
            //obj2.CounterPartyID = "zh-1";
            //obj2.CounterPartyName = "中国银行";
            //obj2.CounterPartySubName = "李丹丹";
            //obj2.CounterpartySalesAgentFlag = "1";
            //obj2.CounterPartyVenues = "IDB";
            //datas.Add(obj2);

            //ExecutionReport obj3 = new ExecutionReport();
            //obj3.ExecID = "execid-3";
            //obj3.TimeInForce = "";
            //obj3.SalesAgentFlag = "1";
            //obj3.BlockNum = this.BlockNum;
            //obj3.Venues = "IDB";
            //obj3.Side = "1";
            //obj3.PartyID = "sx-1";
            //obj3.PartyName = "山西证券";
            //obj3.PartySubName = "张三";
            //obj3.SecurityID = "180210.IB";
            //obj3.Symbol = "18国债28";
            //obj3.Yield = "2.34";
            //obj3.CumQty = "2000";
            //obj3.SettlType = "1";
            //obj3.CounterPartyID = "gf-1";
            //obj3.CounterPartyName = "广发银行";
            //obj3.CounterPartySubName = "张顺";
            //obj3.CounterpartySalesAgentFlag = "0";
            //obj3.CounterPartyVenues = "MANUAL";
            //datas.Add(obj3);

            //ExecutionReport obj4 = new ExecutionReport();
            //obj4.ExecID = "execid-4";
            //obj4.TimeInForce = "";
            //obj4.SalesAgentFlag = "1";
            //obj4.BlockNum = this.BlockNum;
            //obj4.Venues = "IDB";
            //obj4.Side = "2"; //注意方向
            //obj4.PartyID = "sx-1";
            //obj4.PartyName = "山西证券";// 注意对手方
            //obj4.PartySubName = "张三";// 注意对手方
            //obj4.SecurityID = "180210.IB";
            //obj4.Symbol = "18国债28";
            //obj4.Yield = "2.34";
            //obj4.CumQty = "2000";// 注意数量
            //obj4.SettlType = "1";
            //obj4.CounterPartyID = "zs-1";
            //obj4.CounterPartyName = "招商银行";
            //obj4.CounterPartySubName = "张星";
            //obj4.CounterpartySalesAgentFlag = "0";
            //obj4.CounterPartyVenues = "PTCN";
            //datas.Add(obj4);

            #endregion
            #region 4. 销售对市场例子
            //ExecutionReport obj1 = new ExecutionReport();
            //obj1.ExecID = "execid-1";
            //obj1.TimeInForce = "3";
            //obj1.SalesAgentFlag = "1";
            //obj1.BlockNum = this.BlockNum;
            //obj1.Venues = "IDB";
            //obj1.Side = "2";
            //obj1.PartyID = "sx-1";
            //obj1.PartyName = "山西证券";
            //obj1.PartySubName = "张三";
            //obj1.SecurityID = "180210.IB";
            //obj1.Symbol = "18国债28";
            //obj1.Yield = "2.34";
            //obj1.CumQty = "4000";
            //obj1.SettlType = "1";
            //obj1.CounterPartyID = "sx-3";
            //obj1.CounterPartyName = "山西证券";
            //obj1.CounterPartySubName = "王五";
            //obj1.CounterpartySalesAgentFlag = "0";
            //obj1.CounterPartyVenues = "ODM";
            //obj1.SendingTime = "09-01 13:00";
            //obj1.UpdateTime = "09-22 09:00";
            //datas.Add(obj1);

            //ExecutionReport obj2 = new ExecutionReport();
            //obj2.ExecID = "execid-2";
            //obj2.TimeInForce = "";
            //obj2.SalesAgentFlag = "1";
            //obj2.BlockNum = this.BlockNum;
            //obj2.Venues = "IDB";
            //obj2.Side = "1"; // 注意方向
            //obj2.PartyID = "sx-1";
            //obj2.PartyName = "山西证券";
            //obj2.PartySubName = "张三";
            //obj2.SecurityID = "180210.IB";
            //obj2.Symbol = "18国债28";
            //obj2.Yield = "2.34";
            //obj2.CumQty = "2000"; // 注意数量
            //obj2.SettlType = "1";
            //obj2.CounterPartyID = "zh-1";
            //obj2.CounterPartyName = "中国银行";
            //obj2.CounterPartySubName = "李丹丹";
            //obj2.CounterpartySalesAgentFlag = "1";
            //obj2.CounterPartyVenues = "IDB";
            //datas.Add(obj2);

            //ExecutionReport obj3 = new ExecutionReport();
            //obj3.ExecID = "execid-3";
            //obj3.TimeInForce = "";
            //obj3.SalesAgentFlag = "0";
            //obj3.BlockNum = this.BlockNum;
            //obj3.Venues = "IDB";
            //obj3.Side = "1";
            //obj3.PartyID = "sx-1";
            //obj3.PartyName = "山西证券";
            //obj3.PartySubName = "张三";
            //obj3.SecurityID = "180210.IB";
            //obj3.Symbol = "18国债28";
            //obj3.Yield = "2.34";
            //obj3.CumQty = "2000";
            //obj3.SettlType = "1";
            //obj3.CounterPartyID = "gf-1";
            //obj3.CounterPartyName = "广发银行";
            //obj3.CounterPartySubName = "张顺";
            //obj3.CounterpartySalesAgentFlag = "0";
            //obj3.CounterPartyVenues = "IDB";
            //datas.Add(obj3);
            #endregion
            // ---------------------------------

            #region 5. 自营对销售例子
            //ExecutionReport obj1 = new ExecutionReport();
            //obj1.ExecID = "execid-1";
            //obj1.TimeInForce = "3";
            //obj1.SalesAgentFlag = "0";
            //obj1.BlockNum = this.BlockNum;
            //obj1.Venues = "MANUAL";
            //obj1.Side = "2";
            //obj1.PartyID = "sx-1";
            //obj1.PartyName = "山西证券";
            //obj1.PartySubName = "张三";
            //obj1.SecurityID = "180210.IB";
            //obj1.Symbol = "18国债28";
            //obj1.Yield = "2.34";
            //obj1.CumQty = "4000";
            //obj1.SettlType = "1";
            //obj1.CounterPartyID = "sx-2";
            //obj1.CounterPartyName = "山西证券";
            //obj1.CounterPartySubName = "李四";
            //obj1.CounterpartySalesAgentFlag = "1";
            //obj1.CounterPartyVenues = "IDB";
            //obj1.SendingTime = "09-01 13:00";
            //obj1.UpdateTime = "09-22 09:00";
            //datas.Add(obj1);

            //ExecutionReport obj4 = new ExecutionReport();
            //obj4.ExecID = "execid-4";
            //obj4.TimeInForce = "";
            //obj4.SalesAgentFlag = "1";
            //obj4.BlockNum = this.BlockNum;
            //obj4.Venues = "IDB";
            //obj4.Side = "2"; //注意方向
            //obj4.PartyID = "sx-2";
            //obj4.PartyName = "山西证券";// 注意对手方
            //obj4.PartySubName = "李四";// 注意对手方
            //obj4.SecurityID = "180210.IB";
            //obj4.Symbol = "18国债28";
            //obj4.Yield = "2.34";
            //obj4.CumQty = "2000";// 注意数量
            //obj4.SettlType = "1";
            //obj4.CounterPartyID = "zs-1";
            //obj4.CounterPartyName = "招商银行";
            //obj4.CounterPartySubName = "张星";
            //obj4.CounterpartySalesAgentFlag = "1";
            //obj4.CounterPartyVenues = "IDB";
            //datas.Add(obj4);

            //ExecutionReport obj5 = new ExecutionReport();
            //obj5.ExecID = "execid-3";
            //obj5.TimeInForce = "";
            //obj5.SalesAgentFlag = "0";
            //obj5.BlockNum = this.BlockNum;
            //obj5.Venues = "IDB";
            //obj5.Side = "2";
            //obj5.PartyID = "sx-2";
            //obj5.PartyName = "山西证券";
            //obj5.PartySubName = "李四";
            //obj5.SecurityID = "180210.IB";
            //obj5.Symbol = "18国债28";
            //obj5.Yield = "2.34";
            //obj5.CumQty = "2000";
            //obj5.SettlType = "1";
            //obj5.CounterPartyID = "df-1";
            //obj5.CounterPartyName = "东方证券";
            //obj5.CounterPartySubName = "东方";
            //obj5.CounterpartySalesAgentFlag = "0";
            //obj5.CounterPartyVenues = "IDB";
            //datas.Add(obj5);
            #endregion
            #region 6. 自营对自营例子
            //ExecutionReport obj1 = new ExecutionReport();
            //obj1.ExecID = "execid-1";
            //obj1.TimeInForce = "3";
            //obj1.SalesAgentFlag = "0";
            //obj1.BlockNum = this.BlockNum;
            //obj1.Venues = "MANUAL";
            //obj1.Side = "2";
            //obj1.PartyID = "sx-1";
            //obj1.PartyName = "山西证券";
            //obj1.PartySubName = "张三";
            //obj1.SecurityID = "180210.IB";
            //obj1.Symbol = "18国债28";
            //obj1.Yield = "2.34";
            //obj1.CumQty = "4000";
            //obj1.SettlType = "1";
            //obj1.CounterPartyID = "sx-2";
            //obj1.CounterPartyName = "山西证券";
            //obj1.CounterPartySubName = "李四";
            //obj1.CounterpartySalesAgentFlag = "0";
            //obj1.CounterPartyVenues = "MANUAL";
            //obj1.SendingTime = "09-01 13:00";
            //obj1.UpdateTime = "09-22 09:00";
            //datas.Add(obj1);
            #endregion
            #region 7.自营对broker例子
            //ExecutionReport obj1 = new ExecutionReport();
            //obj1.ExecID = "execid-1";
            //obj1.TimeInForce = "3";
            //obj1.SalesAgentFlag = "0";
            //obj1.BlockNum = this.BlockNum;
            //obj1.Venues = "MANUAL";
            //obj1.Side = "2";
            //obj1.PartyID = "sx-2";
            //obj1.PartyName = "山西证券";
            //obj1.PartySubName = "张三";
            //obj1.SecurityID = "180210.IB";
            //obj1.Symbol = "18国债28";
            //obj1.Yield = "2.34";
            //obj1.CumQty = "4000";
            //obj1.SettlType = "1";
            //obj1.CounterPartyID = "sx-2";
            //obj1.CounterPartyName = "山西证券";
            //obj1.CounterPartySubName = "李四";
            //obj1.CounterpartySalesAgentFlag = "0";
            //obj1.CounterPartyVenues = "PTCN";
            //obj1.SendingTime = "09-01 13:00";
            //obj1.UpdateTime = "09-22 09:00";
            //datas.Add(obj1);

            //ExecutionReport obj4 = new ExecutionReport();
            //obj4.ExecID = "execid-4";
            //obj4.TimeInForce = "";
            //obj4.SalesAgentFlag = "0";
            //obj4.BlockNum = this.BlockNum;
            //obj4.Venues = "MANUAL";
            //obj4.Side = "2"; //注意方向
            //obj4.PartyID = "sx-2";
            //obj4.PartyName = "山西证券";// 注意对手方
            //obj4.PartySubName = "李四";// 注意对手方
            //obj4.SecurityID = "180210.IB";
            //obj4.Symbol = "18国债28";
            //obj4.Yield = "2.34";
            //obj4.CumQty = "2000";// 注意数量
            //obj4.SettlType = "1";
            //obj4.CounterPartyID = "zs-1";
            //obj4.CounterPartyName = "招商银行";
            //obj4.CounterPartySubName = "张星";
            //obj4.CounterpartySalesAgentFlag = "0";
            //obj4.CounterPartyVenues = "PTCN";
            //datas.Add(obj4);

            //ExecutionReport obj5 = new ExecutionReport();
            //obj5.ExecID = "execid-3";
            //obj5.TimeInForce = "";
            //obj5.SalesAgentFlag = "0";
            //obj5.BlockNum = this.BlockNum;
            //obj5.Venues = "MANUAL";
            //obj5.Side = "2";
            //obj5.PartyID = "sx-2";
            //obj5.PartyName = "山西证券";
            //obj5.PartySubName = "李四";
            //obj5.SecurityID = "180210.IB";
            //obj5.Symbol = "18国债28";
            //obj5.Yield = "2.34";
            //obj5.CumQty = "2000";
            //obj5.SettlType = "1";
            //obj5.CounterPartyID = "df-1";
            //obj5.CounterPartyName = "东方证券";
            //obj5.CounterPartySubName = "东方";
            //obj5.CounterpartySalesAgentFlag = "0";
            //obj5.CounterPartyVenues = "CTTJ";
            //datas.Add(obj5);
            #endregion
            #region 8 自营对市场例子
            //ExecutionReport obj1 = new ExecutionReport();
            //obj1.ExecID = "execid-1";
            //obj1.TimeInForce = "3";
            //obj1.SalesAgentFlag = "0";
            //obj1.BlockNum = this.BlockNum;
            //obj1.Venues = "MANUAL";
            //obj1.Side = "2";
            //obj1.PartyID = "sx-1";
            //obj1.PartyName = "山西证券";
            //obj1.PartySubName = "张三";
            //obj1.SecurityID = "180210.IB";
            //obj1.Symbol = "18国债28";
            //obj1.Yield = "2.34";
            //obj1.CumQty = "4000";
            //obj1.SettlType = "1";
            //obj1.CounterPartyID = "sx-2";
            //obj1.CounterPartyName = "山西证券";
            //obj1.CounterPartySubName = "李四";
            //obj1.CounterpartySalesAgentFlag = "0";
            //obj1.CounterPartyVenues = "ODM";
            //obj1.SendingTime = "09-01 13:00";
            //obj1.UpdateTime = "09-22 09:00";
            //datas.Add(obj1);
            #endregion
            return datas;
        }

        /// <summary>
        /// 测试数据
        /// </summary>
        private void TestData()
        {
            // 测试
            this.BlockNum = "123456";
            this.Side = "2";
            this.SecurityID = "180210";
            this.Symbol = "18国债28";
            this.Yield = "2.34";
            this.Qty = "5000";
            this.SettlType = "08.20 +1";
            this.LstSendTime = "08-11  15:00";

            BookEditItemViewModel item = new BookEditItemViewModel();
            item.IsSelf = true;
            item.PanelType = "idb";
            item.Name = "山西证券";
            item.SubName = "张三";
            item.Yield = "3.24";
            DataRow dr = item.Datas.NewRow();
            dr[BookEditItemViewModel.SOURCETRADER_FIELD] = "IDB";
            dr[BookEditItemViewModel.COUNTER_PARTY_NAME_FIELD] = "中国银行";
            dr[BookEditItemViewModel.COUNTER_PARTY_SUB_NAME_FIELD] = "李丹丹";
            dr[BookEditItemViewModel.YIELD_FIELD] = 3.34m;
            dr[BookEditItemViewModel.QTY_FIELD] = 2000;
            dr[BookEditItemViewModel.SETTLETYPE_FIELD] = "08-20 +0";
            item.Datas.Rows.Add(dr);

            DataRow dr1 = item.Datas.NewRow();
            dr1[BookEditItemViewModel.SOURCETRADER_FIELD] = "MANUAL";
            dr1[BookEditItemViewModel.COUNTER_PARTY_NAME_FIELD] = "广发银行";
            dr1[BookEditItemViewModel.COUNTER_PARTY_SUB_NAME_FIELD] = "张顺";
            dr1[BookEditItemViewModel.YIELD_FIELD] = 3.35m;
            dr1[BookEditItemViewModel.QTY_FIELD] = 3000;
            dr1[BookEditItemViewModel.SETTLETYPE_FIELD] = "08-21 +1";
            item.Datas.Rows.Add(dr1);

            this.SelfList.Add(item);
            //-----------------------------------

            //BookEditItemViewModel item1 = new BookEditItemViewModel();
            //item1.PanelType = "broker";
            //this.SelfList.Add(item1);
            //BookEditItemViewModel item2 = new BookEditItemViewModel();
            //item2.PanelType = "market";
            //this.SelfList.Add(item2);
            BookEditItemViewModel item3 = new BookEditItemViewModel();
            item3.IsSelf = true;
            item3.PanelType = "self";
            this.SelfList.Add(item3);

            // -------------------------------------
            BookEditItemViewModel counteritem = new BookEditItemViewModel();
            counteritem.PanelType = "idb";
            this.CounterPartList.Add(counteritem);
            BookEditItemViewModel counteritem1 = new BookEditItemViewModel();
            counteritem1.PanelType = "broker";
            counteritem1.Broker = "CNEX";
            counteritem1.Yield = "3.24";
            DataRow dr_broker = counteritem1.Datas.NewRow();
            dr_broker[BookEditItemViewModel.SOURCETRADER_FIELD] = "CNEX";
            dr_broker[BookEditItemViewModel.COUNTER_PARTY_NAME_FIELD] = "昆山农商行";
            dr_broker[BookEditItemViewModel.COUNTER_PARTY_SUB_NAME_FIELD] = "李思";
            dr_broker[BookEditItemViewModel.YIELD_FIELD] = 3.34m;
            dr_broker[BookEditItemViewModel.QTY_FIELD] = 2000;
            dr_broker[BookEditItemViewModel.SETTLETYPE_FIELD] = "08-20 +0";
            counteritem1.Datas.Rows.Add(dr_broker);
            this.CounterPartList.Add(counteritem1);

            BookEditItemViewModel counteritem2 = new BookEditItemViewModel();
            counteritem2.PanelType = "market";
            this.CounterPartList.Add(counteritem2);
            BookEditItemViewModel counteritem3 = new BookEditItemViewModel();
            counteritem3.PanelType = "self";
            this.CounterPartList.Add(counteritem3);
        }
    }
}
