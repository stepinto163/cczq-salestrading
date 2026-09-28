using STC.Common.Model.Admin;
using STC.Common.Utils;
using STC.XBond.Plugin.Utils;
using STC.XBond.Plugin.View.Book;
using STC.XBond.Plugin.View.Component;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Serialization;

namespace STC.XBond.Plugin.ViewModel
{
    public class BookEditItemViewModel : ViewModelBase
    {
        // public ObservableCollection<BookEditItemViewModel> List;

        private string _ExecID = string.Empty;

        public string ExecID
        {
            get
            {
                return _ExecID;
            }
            set
            {
                _ExecID = value;
            }
        }

        private string _LastPanelType = string.Empty;

        public string LastPanelType
        {
            get
            {
                return _LastPanelType;
            }
        }

        public bool IsCancelChangePanelType = false;

        private string _LastBroker = string.Empty;

        public string LastBroker
        {
            get
            {
                return _LastBroker;
            }
        }

        public bool IsBrokerChangePanelType = false;

        /// <summary>
        /// 面板类型
        /// </summary>
        private string _PanelType = string.Empty;

        public string PanelType
        {
            get
            {
                return _PanelType;
            }
            set
            {
                if (_PanelType != value)
                {
                    _LastPanelType = _PanelType;
                    _PanelType = value;

                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 本方price是否显示
        /// </summary>
        private Visibility _VisibilitySelfIDBPrice = Visibility.Visible;

        public Visibility VisibilitySelfIDBPrice
        {
            get
            {
                return _VisibilitySelfIDBPrice;
            }

            set
            {
                _VisibilitySelfIDBPrice = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// 是否是本方
        /// </summary>
        private bool _isSelf = false;

        public bool IsSelf
        {
            get
            {
                return _isSelf;
            }
            set
            {
                if (_isSelf != value)
                {
                    if(value == true && IsUnderline == true)
                    {
                        VisibilitySelfIDBPrice = Visibility.Collapsed;
                    }
                    _isSelf = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 是否是线下补录
        /// </summary>
        private bool _IsUnderline = false;


        public bool IsUnderline
        {
            get
            {
                return _IsUnderline;
            }

            set
            {
                _IsUnderline = value;
                if (IsSelf == true && value == true)
                {
                    VisibilitySelfIDBPrice = Visibility.Collapsed;
                }
            }
        }

        /// <summary>
        /// 机构ID
        /// </summary>
        private string _institutionID = string.Empty;

        public string InstitutionID
        {
            get
            {
                return _institutionID;
            }
            set
            {
                if (_institutionID != value)
                {
                    _institutionID = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 机构名
        /// </summary>
        private string _institutionName = string.Empty;
        
        public string Name
        {
            get
            {
                return _institutionName;
            }
            set
            {
                if (_institutionName != value)
                {
                    _institutionName = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 交易员id
        /// </summary>
        private string _subID = string.Empty;

        public string SubID
        {
            get
            {
                return _subID;
            }
            set
            {
                if (_subID != value)
                {
                    _subID = value;
                }
            }
        }

        private Visibility _selfInfoVisibility = Visibility.Visible;
        public Visibility SelfInfoVisibility
        {
            get
            {
                return IsUnderline ? Visibility.Collapsed : Visibility.Visible;
            }
        }


        /// <summary>
        /// 交易员名
        /// </summary>
        private string _subName = string.Empty;

        public string SubName
        {
            get
            {
                return _subName;
            }
            
            set
            {
                if(_subName != value)
                {
                    _subName = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 报价
        /// </summary>
        private string _yield = string.Empty;

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
        /// 报价
        /// </summary>
        private string _lastPx = string.Empty;

        public string LastPx
        {
            get
            {
                return _lastPx;
            }
            set
            {
                if (_lastPx != value)
                {
                    _lastPx = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 报量
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
                    //this.CounterPartQty += (value - _qty);
                    _qty = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 交易日期
        /// </summary>
        private string _TradeDate;
        public string TradeDate
        {
            get
            {
                return _TradeDate;
            }
            set
            {
                _TradeDate = value;
            }
        }

        /// <summary>
        /// 交割日期
        /// </summary>
        private string _SettlDate;
        public string SettlDate
        {
            get
            {
                return _SettlDate;
            }
            set
            {
                _SettlDate = value;
            }
        }


        /// <summary>
        /// 清算速度
        /// </summary>
        private string _settlType;

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
        /// 清算速度DataPicker值
        /// </summary>
        private DateTime _selectDate = DateTimeEx.ServerTime;

        public DateTime SelectDate
        {
            get
            {
                return _selectDate;
            }
            set
            {
                if (_selectDate != value)
                {
                    _selectDate = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 创建日期
        /// </summary>
        private string _createTime;

        public string CreateTime
        {
            get
            {
                return _createTime;
            }
            set
            {
                if (_createTime != value)
                {
                    _createTime = value;
                }
            }
        }


        /// <summary>
        ///  清算速度combobox的值
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
        /// 市场备注
        /// </summary>
        private string _Remark = string.Empty;

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
        /// 成交场所
        /// </summary>
        private string _VenuesTypeGW = string.Empty;

        public string VenuesTypeGW
        {
            get
            {
                return _VenuesTypeGW;
            }
            set
            {
                if (_VenuesTypeGW != value)
                {
                    _VenuesTypeGW = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 线上的交易方式
        /// </summary>
        private string _Venues = string.Empty;
        public string Venues
        {
            get
            {
                return _Venues;
            }
            set
            {
                if (_Venues != value)
                {
                    _Venues = value;
                    this.OnPropertyChanged();
                }
            }
        }

        // 是否显示deletebtn
        private Visibility _delBtnVisibility = Visibility.Collapsed;

        public Visibility DelBtnVisibility
        {
            get
            {
                return _delBtnVisibility;
            }
            set
            {
                if (_delBtnVisibility != value)
                {
                    _delBtnVisibility = value;
                    this.OnPropertyChanged();
                }
            }
        }


        /// <summary>
        /// 渠道执行时不显示经纪商报价
        /// </summary>
        private Visibility _PriceVisibility = Visibility.Visible;

        public Visibility PriceVisibility
        {
            get
            {
                return _PriceVisibility;
            }
            set
            {
                if (_PriceVisibility != value)
                {
                    _PriceVisibility = value;
                }
            }
        }

        /// <summary>
        /// 席位号显示
        /// </summary>
        private string positionVisibility;
        public string PositionVisibility
        {
            get
            {
                return positionVisibility;
            }

            set
            {
                positionVisibility = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// 报价方式显示
        /// </summary>
        private string sendPriceTypeVisibility;
        public string SendPriceTypeVisibility
        {
            get
            {
                return sendPriceTypeVisibility;
            }

            set
            {
                sendPriceTypeVisibility = value;
                this.OnPropertyChanged();
            }
        }
        /// <summary>
        /// 报价方式显示
        /// </summary>
        private bool subNameVisibility = true;
        public bool SubNameVisibility
        {
            get
            {
                return subNameVisibility;
            }

            set
            {
                subNameVisibility = value;
                this.OnPropertyChanged();
            }
        }

        ///// <summary>
        ///// 价格颜色
        ///// </summary>
        //private SolidColorBrush _PriceColor = new SolidColorBrush(Color.FromArgb(255, 88, 179, 255));

        //public SolidColorBrush PriceColor
        //{
        //    get
        //    {
        //        return _PriceColor;
        //    }
        //    set
        //    {
        //        if (_PriceColor != value)
        //        {
        //            _PriceColor = value;
        //            this.OnPropertyChanged();
        //        }
        //    }
        //}

        public Dictionary<string, string> PanelTypeDic { get; set; }

        /// <summary>
        /// 本方面板类型
        /// </summary>
        Dictionary<string, string> _selfPanelTypeDic;

        public  Dictionary<string, string> SelfPanelTypeDic
        {
            get
            {
                if (_selfPanelTypeDic == null)
                {
                    _selfPanelTypeDic = new Dictionary<string, string>();
                    _selfPanelTypeDic[Constants.IDB_UNDERLINE_PART] = "销售";
                    _selfPanelTypeDic[Constants.SELF_UNDERLINE_PART] = "自营";
                }
                
                return _selfPanelTypeDic;
            }
        }

        /// <summary>
        /// 对手方面板类型
        /// </summary>
        Dictionary<string, string> _counterPanelTypeDic;

        public Dictionary<string, string> CounterPanelTypeDic
        {
            get
            {
                if (_counterPanelTypeDic == null)
                {
                    _counterPanelTypeDic = new Dictionary<string, string>();
                    _counterPanelTypeDic[Constants.IDB_UNDERLINE_PART] = "销售";
                    _counterPanelTypeDic[Constants.BROKER_UNDERLINE_PART] = "经纪商";
                    _counterPanelTypeDic[Constants.SELF_UNDERLINE_PART] = "自营";
                    _counterPanelTypeDic[Constants.MARKET_UNDERLINE_PART] = "市场";
                }
                return _counterPanelTypeDic;
            }
        }

        /// <summary>
        /// 销售数据源
        /// </summary>
        public List<StcTbookUsers> SalesSource
        {
            get
            {
                if (SaleItem == null && BookHandler.SalesList.Count > 0)
                {
                    SaleItem = BookHandler.SalesList[0];
                }
                
                return BookHandler.SalesList;
            }
        }

        /// <summary>
        /// 所选销售
        /// </summary>
        private StcTbookUsers _SaleItem;
        public StcTbookUsers SaleItem
        {
            get
            {
                return _SaleItem;
            }
            set
            {
                if (_SaleItem != value)
                {
                    _SaleItem = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 自营数据源
        /// </summary>
        public List<StcTbookUsers> SelfSource
        {
            get
            {
                if (SaleItem == null && BookHandler.SelfList.Count > 0)
                {
                    SaleItem = BookHandler.SelfList[0];
                }
                return BookHandler.SelfList;
            }
        }

        ///// <summary>
        ///// 所选自营
        ///// </summary>
        //private StcTbookUsers _SelfItem;
        //public StcTbookUsers SelfItem
        //{
        //    get
        //    {
        //        return _SelfItem;
        //    }
        //    set
        //    {
        //        if (_SelfItem != value)
        //        {
        //            _SelfItem = value;
        //            this.OnPropertyChanged();
        //        }
        //    }
        //}

        /// <summary>
        /// broker数据源
        /// </summary>
        private Dictionary<string, string> _venuesSource = new Dictionary<string, string>()
        {
            { "TPSC", "国利" },
            { "CTTJ", "信唐"},
            { "CNEX", "国际"},
            { "CCTB", "中诚"},
            { "PTCN", "平安"},
            { "UEDA", "上田"}
        };

        public Dictionary<string, string> VenuesSource
        {
            get
            {
                if (BrokerItem == null)
                {
                    BrokerItem = new KeyValuePair<string, string>("TPSC", "国利");
                }
                return _venuesSource;
            }
        }

        private KeyValuePair<string, string>? _LastBrokerItem = new KeyValuePair<string, string>();

        public KeyValuePair<string, string>? LastBrokerItem
        {
            get
            {
                return _LastBrokerItem;
            }
        }
        /// <summary>
        /// 所选Broker
        /// </summary>
        private KeyValuePair<string, string>? _BrokerItem;

        public KeyValuePair<string, string>? BrokerItem
        {
            get
            {
                return _BrokerItem;
            }
            set
            {
                if (_BrokerItem == null || !_BrokerItem.Value.Equals(value.Value))
                {
                    _LastBrokerItem = _BrokerItem;
                    _BrokerItem = value;
                    this.Broker = value.Value.Key;
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

        ///// <summary>
        ///// 市场对手方机构名
        ///// </summary>
        //private string _CounterPartyName;

        //public string CounterPartyName
        //{
        //    get
        //    {
        //        return _CounterPartyName;
        //    }
        //    set
        //    {
        //        if (_CounterPartyName != value)
        //        {
        //            _CounterPartyName = value;
        //            this.OnPropertyChanged();
        //        }
        //    }
        //}

        ///// <summary>
        ///// 市场对手方交易员名
        ///// </summary>
        //private string _CounterPartySubName;

        //public string CounterPartySubName
        //{
        //    get
        //    {
        //        return _CounterPartySubName;
        //    }
        //    set
        //    {
        //        if (_CounterPartySubName != value)
        //        {
        //            _CounterPartySubName = value;
        //            this.OnPropertyChanged();
        //        }
        //    }
        //}
        public void RemoveDatas(BookEditViewModel bookEditViewModel)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                lock (Datas)
                {
                    int count = Datas.Rows.Count;
                    for (int i = count - 1; i >= 0; i--)
                    {
                        long qty = CommonUtils.ConvertToLong(Datas.Rows[i][QTY_FIELD] == null ? "0" : Datas.Rows[i][QTY_FIELD].ToString());
                        if (IsSelf)
                        {
                            bookEditViewModel.SelfQty -= qty;
                        }
                        else
                        {
                            bookEditViewModel.CounterPartQty -= qty;
                        }
                        this.Datas.Rows.RemoveAt(i);
                    }
                }
            }));
        }
        // 确认数据
        public const string EXECID_FIELD = "execID";
        public const string ISEDIT_FIELD = "isEdit";
        public const string ISEDIT_PUBLICACCOUNT_FIELD = "isEdit_publicaccount";
        public const string SOURCETRADER_FIELD = "SourceTrader";
        public const string COUNTER_PARTY_ID_FIELD = "counterpartyid";
        public const string COUNTER_PARTY_NAME_FIELD = "counterpartyname";
        public const string COUNTER_PARTY_SUB_ID_FIELD = "counterpartysubid";
        public const string COUNTER_PARTY_SUB_NAME_FIELD = "counterpartysubname";
        public const string YIELD_FIELD = "yield";
        public const string QTY_FIELD = "qty";
        public const string SETTLETYPE_FIELD = "settltype";
        public const string TRADEDATE_FIELD = "tradedate";
        public const string SETTLDATE_FIELD = "settldate";
        public const string CREATEDATE_FIELD = "createdate";
        public const string CREATEDATEVIS_FIELD = "createdateVis";
        public const string DCSEXEID_FIELD = "dcsexeid";
        public const string JSONINFO_FIELD = "jsonInfo";
        public const string BROKEREXECID_FIELD = "BrokerExecId";
        // 未确认数据
        public const string EDIT_COUNTER_PARTY_NAME_FIELD = "edit_counterpartyname";
        public const string EDIT_COUNTER_PARTY_SUB_NAME_FIELD = "edit_counterpartysubname";
        public const string EDIT_YIELD_FIELD = "edit_yield";
        public const string EDIT_QTY_FIELD = "edit_qty";
        public const string EDIT_SETTLETYPE_DATE_FIELD = "edit_settltype_date";
        public const string EDIT_SETTLETYPE_FIELD = "edit_settletype";

        public const string PRICE_WARN_BG = "PriceWarnBg";
        public const string SETTLE_WARN_BG = "SettleWarnBg";

        public const string NOTDCS_FIELD = "EditDcsBrokerVis";
        public const string BROKER_REMARK_FIELD = "BrokerRemark";
        public const string EDIT_BROKER_REMARK_FIELD = "EditBrokerRemark";
        public const string TRADEMETHOD_FIELD = "TradeMethod";
        public const string EDIT_TRADEMETHOD_FIELD = "edit_TradeMethod";
        public const string TRADEMETHODITEM_FIELD = "TradeMethodItem";

        public const string ISEDIT_TRADEMETHOD_FIELD = "isEdit_TradeMethod";
        public const string PARTY_SUB_ID_FIELD = "PartySubID";
        public const string PARTY_SUB_NAME_FIELD = "PartySubName";

        /// <summary>
        /// 表格绑定数据
        /// </summary>
        private DataTable _datas;

        public DataTable Datas
        {
            get
            {
                if (_datas == null)
                {
                    _datas = new DataTable();
                    _datas.Columns.Add(EXECID_FIELD, typeof(string)); 
                    _datas.Columns.Add(ISEDIT_FIELD, typeof(string)); // 0:显示 2：不显示
                    _datas.Columns.Add(ISEDIT_PUBLICACCOUNT_FIELD, typeof(string)); // 0:显示， 2：不显示
                    _datas.Columns.Add(SOURCETRADER_FIELD, typeof(string));
                    _datas.Columns.Add(COUNTER_PARTY_ID_FIELD, typeof(string));
                    _datas.Columns.Add(COUNTER_PARTY_NAME_FIELD, typeof(string));
                    _datas.Columns.Add(COUNTER_PARTY_SUB_ID_FIELD, typeof(string));
                    _datas.Columns.Add(COUNTER_PARTY_SUB_NAME_FIELD,typeof(string));
                    _datas.Columns.Add(YIELD_FIELD, typeof(string));
                    _datas.Columns.Add(QTY_FIELD, typeof(long));
                    _datas.Columns.Add(SETTLETYPE_FIELD, typeof(string));
                    _datas.Columns.Add(TRADEDATE_FIELD, typeof(string)); // 交易日期
                    _datas.Columns.Add(SETTLDATE_FIELD, typeof(string)); //交割日期
                    _datas.Columns.Add(CREATEDATE_FIELD, typeof(string)); //创建日期
                    _datas.Columns.Add(CREATEDATEVIS_FIELD, typeof(string)); //创建日期
                    _datas.Columns.Add(DCSEXEID_FIELD, typeof(string)); //内码
                    _datas.Columns.Add(JSONINFO_FIELD, typeof(string));//idb,brokers添加时使用
                    _datas.Columns.Add(BROKEREXECID_FIELD, typeof(string));//brokerexecid

                    _datas.Columns.Add(EDIT_COUNTER_PARTY_NAME_FIELD, typeof(string));
                    _datas.Columns.Add(EDIT_COUNTER_PARTY_SUB_NAME_FIELD, typeof(string));
                    _datas.Columns.Add(EDIT_YIELD_FIELD, typeof(string));
                    _datas.Columns.Add(EDIT_QTY_FIELD, typeof(string));
                    _datas.Columns.Add(EDIT_SETTLETYPE_DATE_FIELD, typeof(DateTime));
                    _datas.Columns.Add(EDIT_SETTLETYPE_FIELD, typeof(int));
                    _datas.Columns.Add(PRICE_WARN_BG, typeof(string));
                    _datas.Columns.Add(SETTLE_WARN_BG, typeof(string));
                    _datas.Columns.Add(NOTDCS_FIELD, typeof(Visibility));
                    _datas.Columns.Add(BROKER_REMARK_FIELD, typeof(string));
                    _datas.Columns.Add(EDIT_BROKER_REMARK_FIELD, typeof(string));
                    _datas.Columns.Add(TRADEMETHOD_FIELD, typeof(string));
                    _datas.Columns.Add(EDIT_TRADEMETHOD_FIELD, typeof(string));
                    _datas.Columns.Add(TRADEMETHODITEM_FIELD, typeof(ComboBoxItem));
                    _datas.Columns.Add(ISEDIT_TRADEMETHOD_FIELD, typeof(string));
                    _datas.Columns.Add(PARTY_SUB_ID_FIELD, typeof(string));
                    _datas.Columns.Add(PARTY_SUB_NAME_FIELD, typeof(string));
                }
                return _datas;
            }
        }

        /// <summary>
        /// 经纪商
        /// </summary>
        private string _broker;

        public string Broker
        {
            get
            {
                return _broker;
            }
            set
            {
                if (_broker != value)
                {
                    _LastBroker = _broker;

                    _broker = value;
                    if (LocalEnv.ConfInfo.IsDcsBroker.ContainsKey(this.Broker))
                    {
                        DcsBrokerVis = Visibility.Visible;
                        NotDcsBrokerVis = Visibility.Hidden;
                    }
                    else
                    {
                        DcsBrokerVis = Visibility.Hidden;
                        NotDcsBrokerVis = Visibility.Visible;
                    }
                    this.OnPropertyChanged();
                }
            }
        }

        private DataRowView _selectedRow;

        /// <summary>
        /// 选中行记录
        /// </summary>
        public DataRowView SelectedRow
        {
            get
            {
                return _selectedRow;
            }

            set
            {
                _selectedRow = value;
            }
        }

        /// <summary>
        /// 删除表格行记录
        /// </summary>
        public ICommand DelRowCommand { get; set; }

        /// <summary>
        /// 添加Broker行
        /// </summary>
        public ICommand AddBrokerRowCommand { get; set; }



        /// <summary>
        /// 添加IDB行
        /// </summary>
        public ICommand AddIDBRowCommand { get; set; }

        /// <summary>
        /// 添加手工行
        /// </summary>
        public ICommand AddManualRowCommand { get; set; }

        /// <summary>
        /// 编辑行
        /// </summary>
        public ICommand EditRowCommand { get; set; }

        /// <summary>
        /// 确认行
        /// </summary>
        public ICommand SaveRowCommand { get; set; }

        /// <summary>
        // 取消行
        /// </summary>
        public ICommand CancelRowCommand { get; set; }

        public ICommand CPNTextChangeCommand { get; set; }
        public ICommand TradeMethodChangeCommand { get; set; }
        public ICommand MarketTradeMethodChangeCommand { get; set; }

        /// <summary>
        /// 添加非DCS的
        /// </summary>
        public ICommand AddNonDcsRowCommand { get; set; }

        private Visibility dcsBrokerVis;
        private Visibility notDcsBrokerVis;

        public Visibility DcsBrokerVis
        {
            get
            {
                return dcsBrokerVis;
            }

            set
            {
                dcsBrokerVis = value;
                this.OnPropertyChanged();
            }
        }

        public Visibility NotDcsBrokerVis
        {
            get
            {
                return notDcsBrokerVis;
            }

            set
            {
                notDcsBrokerVis = value;
                this.OnPropertyChanged();
            }
        }

        private string tradeMethod;
        public string TradeMethod
        {
            get
            {
                return tradeMethod;
            }

            set
            {
                if (tradeMethod != value)
                {
                    tradeMethod = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private string token = string.Empty;

        /// <summary>
        /// 构造函数
        /// </summary>
        public BookEditItemViewModel()
        {
            token = Guid.NewGuid().ToString();
            // 初始化Command
            DelRowCommand = new RelayCommand<Button>(DelRow);
            AddBrokerRowCommand = new RelayCommand<Button>(AddBrokerRow);
            AddIDBRowCommand = new RelayCommand<Button>(AddIDBRow);
            AddManualRowCommand = new RelayCommand<Button>(AddManualRow);

            EditRowCommand = new RelayCommand<Button>(EditRow);
            SaveRowCommand = new RelayCommand<Button>(SaveRow);
            CancelRowCommand = new RelayCommand(CancelRow);
            AddNonDcsRowCommand = new RelayCommand<Button>(AddNonDcsRow);

            CPNTextChangeCommand = new RelayCommand(CPNTextChange);
            TradeMethodChangeCommand = new RelayCommand(TradeMethodChange);
            MarketTradeMethodChangeCommand = new RelayCommand<ComboBox>(MarketTradeMethodChange);
            BookEditViewModel.UpdateSecurityID += BookEditViewModel_UpdateSecurityID;
        }

        private void BookEditViewModel_UpdateSecurityID(string obj)
        {
            securityID = obj;
            if (!string.IsNullOrEmpty(obj) && obj.EndsWith("IB"))
            {
                PositionVisibility = "2";
                SendPriceTypeVisibility = "0";
            }
            else
            {
                PositionVisibility = "0";
                SendPriceTypeVisibility = "2";
            }
        }

        /// <summary>
        /// 线下录入【市场】：切换市场交易模式
        /// </summary>
        /// <param name="obj"></param>
        private void MarketTradeMethodChange(ComboBox obj)
        {
            if (obj != null && obj.SelectedIndex >= 0)
            {
                ComboBoxItem cbi = (ComboBoxItem)obj.SelectedItem;
                if (cbi != null && ("4".Equals(cbi.Tag) || "50".Equals(cbi.Tag)))
                {
                    SubNameVisibility = false;
                    SubName = "--";
                }
                else
                {
                    SubNameVisibility = true;
                }
            }
        }

        /// <summary>
        /// 添加经济商手工数据
        /// </summary>
        /// <param name="addNonDcsBtn"></param>
        private void AddNonDcsRow(Button addNonDcsBtn)
        {
            if (addNonDcsBtn != null)
            {
                BookEditView bookEditView = CommonUtils.GetParentObject<BookEditView>(addNonDcsBtn) as BookEditView;
                BookEditViewModel bookEditViewModel = (bookEditView.DataContext as BookEditViewModel);
                if (bookEditViewModel != null)
                {
                    if (!string.IsNullOrEmpty(bookEditViewModel.SecurityID))
                    {
                        securityID = bookEditViewModel.SecurityID;
                        DataRow dr = this.Datas.NewRow();
                        dr[ISEDIT_FIELD] = "0";
                        if (LocalEnv.ConfInfo.IsDcsBroker.ContainsKey(this.Broker))
                        {
                            dr[NOTDCS_FIELD] = Visibility.Collapsed;
                        }
                        else
                        {
                            dr[NOTDCS_FIELD] = Visibility.Visible;
                        }
                        SetIsEdit4PublicAccount(dr);
                        dr[SOURCETRADER_FIELD] = this.Broker;
                        dr[EDIT_SETTLETYPE_DATE_FIELD] = DateTimeEx.ServerTime;
                        dr[EDIT_SETTLETYPE_FIELD] = 0;

                        if (securityID.EndsWith("IB"))
                        {
                            SendPriceTypeVisibility = "0";
                            PositionVisibility = "2";
                            dr[EDIT_TRADEMETHOD_FIELD] = "待定";
                        }
                        else
                        {
                            SendPriceTypeVisibility = "2";
                            PositionVisibility = "0";
                        }
                        this.Datas.Rows.Add(dr);
                    }
                    else
                    {
                        Common.View.Component.CustomMessageBox.Error("债券代码不可为空", null, bookEditView);
                    }
                }
            }
        }

        /// <summary>
        /// 修改机构号逻辑
        /// </summary>
        private void CPNTextChange()
        {
            if (this.SelectedRow != null && this.SelectedRow.Row.RowState != DataRowState.Detached)
            {
                SetIsEdit4PublicAccount(this.SelectedRow.Row);
            }
        }

        private void SetIsEdit4PublicAccount(DataRow dr)
        {
           string text = CommonUtils.ConvertToString(dr[EDIT_COUNTER_PARTY_NAME_FIELD]);
            if (text.Trim().Contains(Constants.PUBLIC_ACCOUNT) && !this.PanelType.Contains(Constants.BROKER_PART))
            {
                dr[ISEDIT_PUBLICACCOUNT_FIELD] = "2";
                dr[COUNTER_PARTY_SUB_ID_FIELD] = "--";
                dr[COUNTER_PARTY_SUB_NAME_FIELD] = "--";
                dr[YIELD_FIELD] = "--";
                dr[SETTLETYPE_FIELD] = "--";
                dr[ISEDIT_TRADEMETHOD_FIELD] = "2";
            }
            else
            {
                string isedit = dr[ISEDIT_FIELD] as string;
                dr[ISEDIT_PUBLICACCOUNT_FIELD] = dr[ISEDIT_FIELD];
                if (!string.IsNullOrEmpty(securityID))
                {
                    if (securityID.EndsWith("IB"))//银行间债券
                    {
                        if (dr[TRADEMETHODITEM_FIELD] != DBNull.Value)
                        {
                            ComboBoxItem cbi = (ComboBoxItem)dr[TRADEMETHODITEM_FIELD];
                            if (cbi != null && ("4".Equals(cbi.Tag) || "50".Equals(cbi.Tag)))
                            {
                                dr[ISEDIT_PUBLICACCOUNT_FIELD] = "2";
                                dr[COUNTER_PARTY_SUB_NAME_FIELD] = "--";
                                if (text.Trim().Contains(Constants.PUBLIC_ACCOUNT) && !this.PanelType.Contains(Constants.BROKER_PART))
                                {
                                    dr[ISEDIT_TRADEMETHOD_FIELD] = "2";
                                }
                                else
                                {
                                    dr[ISEDIT_TRADEMETHOD_FIELD] = dr[ISEDIT_FIELD];
                                }
                            }
                            else
                            {
                                if ("2".Equals(isedit))
                                {
                                    dr[ISEDIT_TRADEMETHOD_FIELD] = "2";
                                }
                                else
                                {
                                    dr[ISEDIT_TRADEMETHOD_FIELD] = "0";
                                }
                            }
                        }
                        else
                        {
                            if ("2".Equals(isedit))
                            {
                                dr[ISEDIT_TRADEMETHOD_FIELD] = "2";
                            }
                            else
                            {
                                dr[ISEDIT_TRADEMETHOD_FIELD] = "0";
                            }
                        }
                    }
                    else//交易所债券SH,SZ
                    {
                        if ("2".Equals(isedit))
                        {
                            dr[ISEDIT_TRADEMETHOD_FIELD] = "2";
                        }
                        else
                        {
                            dr[ISEDIT_TRADEMETHOD_FIELD] = "0";
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 【经济商】【销售】：切换市场交易模式
        /// </summary>
        private void TradeMethodChange()
        {
            if (this.SelectedRow != null && this.SelectedRow.Row.RowState != DataRowState.Detached)
            {
                TradeMethodPublicAccount(this.SelectedRow.Row);
            }
        }

        /// <summary>
        ///  【经济商】【销售】：切换市场交易模式
        /// </summary>
        /// <param name="dr"></param>
        private void TradeMethodPublicAccount(DataRow dr)
        {
            string text = CommonUtils.ConvertToString(dr[EDIT_COUNTER_PARTY_NAME_FIELD]);
            ComboBoxItem cbi = (ComboBoxItem)dr[TRADEMETHODITEM_FIELD];
            if (cbi != null && ("4".Equals(cbi.Tag) || "50".Equals(cbi.Tag)))
            {
                dr[COUNTER_PARTY_SUB_NAME_FIELD] = "--";
                dr[ISEDIT_PUBLICACCOUNT_FIELD] = "2";
                if (text.Trim().Contains(Constants.PUBLIC_ACCOUNT) && !this.PanelType.Contains(Constants.BROKER_PART))
                {
                    dr[ISEDIT_TRADEMETHOD_FIELD] = "2";
                }
                else
                {
                    dr[ISEDIT_TRADEMETHOD_FIELD] = "0";
                }
            }
            else
            {
                if (text.Trim().Contains(Constants.PUBLIC_ACCOUNT) && !this.PanelType.Contains(Constants.BROKER_PART))
                {
                    dr[ISEDIT_PUBLICACCOUNT_FIELD] = "2";
                }
                else
                {
                    dr[ISEDIT_PUBLICACCOUNT_FIELD] = dr[ISEDIT_FIELD];
                }
            }
        }
        private string securityID = "";

        /// <summary>
        /// 编辑记录
        /// </summary>
        /// <param name="editBtn"></param>
        private void EditRow(Button editBtn)
        {
            if (this.SelectedRow != null && this.SelectedRow.Row.RowState != DataRowState.Detached)
            {
                BookEditView bookEditView = CommonUtils.GetParentObject<BookEditView>(editBtn) as BookEditView;
                BookEditViewModel bookEditViewModel = (bookEditView.DataContext as BookEditViewModel);
                if (bookEditViewModel != null)
                {
                    securityID = bookEditViewModel.SecurityID;
                }
                this.SelectedRow[ISEDIT_FIELD] = "0";
                CPNTextChange();
                this.SelectedRow[EDIT_COUNTER_PARTY_NAME_FIELD] = this.SelectedRow[COUNTER_PARTY_NAME_FIELD] as string;
                this.SelectedRow[EDIT_COUNTER_PARTY_SUB_NAME_FIELD] = this.SelectedRow[COUNTER_PARTY_SUB_NAME_FIELD] as string;
                this.SelectedRow[EDIT_YIELD_FIELD] = this.SelectedRow[YIELD_FIELD];
                this.SelectedRow[EDIT_QTY_FIELD] = this.SelectedRow[QTY_FIELD];

                string settlType = CommonUtils.ConvertToString(this.SelectedRow[SETTLETYPE_FIELD]);
                string tradeDate = CommonUtils.ConvertToString(this.SelectedRow[TRADEDATE_FIELD]);

                DateTime settlDateTime;
                int selectIndex;
                CommonUtils.SetSettleType(settlType, tradeDate, out settlDateTime, out selectIndex);
                this.SelectedRow[EDIT_SETTLETYPE_DATE_FIELD] = settlDateTime;
                this.SelectedRow[EDIT_SETTLETYPE_FIELD] = selectIndex;
                if (securityID.EndsWith("IB"))
                {
                    SendPriceTypeVisibility = "0";
                    PositionVisibility = "2";
                    if (!String.IsNullOrEmpty(CommonUtils.ConvertToString(this.SelectedRow[TRADEMETHOD_FIELD])))
                    {
                        this.SelectedRow[EDIT_TRADEMETHOD_FIELD] = this.SelectedRow[TRADEMETHOD_FIELD];
                    }
                    else
                    {
                        this.SelectedRow[EDIT_TRADEMETHOD_FIELD] = "待定";
                    }
                }
                else
                {
                    SendPriceTypeVisibility = "2";
                    PositionVisibility = "0";
                    this.SelectedRow[EDIT_TRADEMETHOD_FIELD] = this.SelectedRow[TRADEMETHOD_FIELD];
                }
                this.SelectedRow[EDIT_BROKER_REMARK_FIELD] = this.SelectedRow[BROKER_REMARK_FIELD];                
            }
        }

        /// <summary>
        /// 保存记录修改
        /// </summary>
        /// <param name="saveBtn"></param>
        private void SaveRow(Button saveBtn)
        {
            if (saveBtn != null)
            {
                BookEditView bookEditView = CommonUtils.GetParentObject<BookEditView>(saveBtn) as BookEditView;
                BookEditViewModel bookEditViewModel = (bookEditView.DataContext as BookEditViewModel);

                if (this.SelectedRow != null && this.SelectedRow.Row.RowState != DataRowState.Detached)
                {
                    string newYield = CommonUtils.ConvertToString(this.SelectedRow[EDIT_YIELD_FIELD]);
                    string newQty = this.SelectedRow.Row[EDIT_QTY_FIELD].ToString();

                    if (CommonUtils.ConvertToString(SelectedRow[EDIT_COUNTER_PARTY_NAME_FIELD]).Contains(Constants.PUBLIC_ACCOUNT) == false)
                    {
                        if (string.IsNullOrEmpty(newYield) || string.IsNullOrEmpty(newQty))
                        {
                            Common.View.Component.CustomMessageBox.Error("收益率或成交量没有填写");
                            return;
                        }
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(newQty))
                        {
                            Common.View.Component.CustomMessageBox.Error("成交量没有填写");
                            return;
                        }
                    }


                    this.SelectedRow[ISEDIT_FIELD] = "2";
                    CPNTextChange();
                    if (CommonUtils.ConvertToString(SelectedRow[EDIT_COUNTER_PARTY_NAME_FIELD]) .Contains(Constants.PUBLIC_ACCOUNT) == false || this.PanelType.Contains(Constants.BROKER_PART))
                    {
                        string newCounterPartyName = CommonUtils.ConvertToString(this.SelectedRow[EDIT_COUNTER_PARTY_NAME_FIELD]);
                        string oldCounterPartyName = CommonUtils.ConvertToString(this.SelectedRow[COUNTER_PARTY_NAME_FIELD]);

                        if (newCounterPartyName != oldCounterPartyName)
                        {
                            this.SelectedRow[COUNTER_PARTY_ID_FIELD] = string.Empty;
                            this.SelectedRow[COUNTER_PARTY_NAME_FIELD] = newCounterPartyName;
                        }

                        string newCounterPartySubName = CommonUtils.ConvertToString(this.SelectedRow[EDIT_COUNTER_PARTY_SUB_NAME_FIELD]);
                        string oldCounterPartySubName = CommonUtils.ConvertToString(this.SelectedRow[COUNTER_PARTY_SUB_NAME_FIELD]);

                        if (newCounterPartySubName != oldCounterPartySubName)
                        {
                            this.SelectedRow[COUNTER_PARTY_SUB_ID_FIELD] = string.Empty;
                            this.SelectedRow[COUNTER_PARTY_SUB_NAME_FIELD] = newCounterPartySubName;
                        }

                        this.SelectedRow[YIELD_FIELD] = CommonUtils.ConvertToString(this.SelectedRow[EDIT_YIELD_FIELD]);
                        DateTime tradeDate = (DateTime)this.SelectedRow[EDIT_SETTLETYPE_DATE_FIELD];
                        int ComboBoxSelectIndex = (int)this.SelectedRow[EDIT_SETTLETYPE_FIELD];
                        DateTime createtime = CommonUtils.ConvertToDateTime(CommonUtils.ConvertToString(this.SelectedRow[CREATEDATE_FIELD]));
                        string settle = CommonUtils.IsSpotSettle(createtime, tradeDate, ComboBoxSelectIndex);
                        if(string.IsNullOrEmpty(settle))
                        {
                            settle = tradeDate.ToString("MM.dd") + (ComboBoxSelectIndex == 0 ? "+0" : "+1");
                        }

                        this.SelectedRow[TRADEDATE_FIELD] = tradeDate.ToString("yyyyMMdd");
                       
                        // 计算交割日期
                        //this.SelectedRow[SETTLDATE_FIELD] = CommonUtils.CalSettlDate(createtime.ToString("yyyyMMdd"), settle, out settle);
                        this.SelectedRow[SETTLETYPE_FIELD] = settle;
                    }
                    else
                    {
                        this.SelectedRow[COUNTER_PARTY_ID_FIELD] = BookHandler.publicUser == null ? string.Empty : BookHandler.publicUser.Userid;
                        this.SelectedRow[COUNTER_PARTY_NAME_FIELD] = BookHandler.publicUser == null ? "山西证券" : BookHandler.publicUser.Name;
                        this.SelectedRow[COUNTER_PARTY_SUB_ID_FIELD] = string.Empty;
                        this.SelectedRow[COUNTER_PARTY_SUB_NAME_FIELD] = string.Empty;

                    }
                   

                    long oldqty = CommonUtils.ConvertToLong(this.SelectedRow.Row[QTY_FIELD] == null ? "0" : this.SelectedRow.Row[QTY_FIELD].ToString());
                    long newqty = CommonUtils.ConvertToLong(this.SelectedRow.Row[EDIT_QTY_FIELD] == null ? "0" : this.SelectedRow.Row[EDIT_QTY_FIELD].ToString());
                    if (IsSelf)
                    {
                        bookEditViewModel.SelfQty += (newqty - oldqty);
                    }
                    else
                    {
                        bookEditViewModel.CounterPartQty += (newqty - oldqty);
                    }
                    this.SelectedRow[QTY_FIELD] = newqty;


                    string yield = CommonUtils.ConvertToString(this.SelectedRow.Row[YIELD_FIELD]);
                    if (!this.IsUnderline && !string.IsNullOrEmpty(yield) && !string.IsNullOrEmpty(bookEditViewModel.Yield) && !bookEditViewModel.Yield.Equals(yield))
                    {
                        this.SelectedRow[PRICE_WARN_BG] = "1";
                    }
                    else
                        this.SelectedRow[PRICE_WARN_BG] = string.Empty;

                    string temSettle = CommonUtils.ConvertToString(this.SelectedRow.Row[SETTLETYPE_FIELD]);
                    if (!this.IsUnderline && !string.IsNullOrEmpty(temSettle) && !string.IsNullOrEmpty(bookEditViewModel.SettlType) && !bookEditViewModel.SettlType.Equals(temSettle))
                    {
                        this.SelectedRow[SETTLE_WARN_BG] = "1";
                    }
                    else
                        this.SelectedRow[SETTLE_WARN_BG] = string.Empty;
                    string tradeMethod = CommonUtils.ConvertToString(this.SelectedRow[EDIT_TRADEMETHOD_FIELD]);
                    string oldtradeMethod = CommonUtils.ConvertToString(this.SelectedRow[TRADEMETHOD_FIELD]);

                    if (tradeMethod != oldtradeMethod)
                    {
                        this.SelectedRow[TRADEMETHOD_FIELD] = tradeMethod;
                    }
                    
                    if ("收请求".Equals(this.SelectedRow[TRADEMETHOD_FIELD]) || "发请求".Equals(this.SelectedRow[TRADEMETHOD_FIELD]))
                    {
                        this.SelectedRow[COUNTER_PARTY_SUB_ID_FIELD] = string.Empty;
                        this.SelectedRow[COUNTER_PARTY_SUB_NAME_FIELD] = "--";
                    }
                    string remark = CommonUtils.ConvertToString(this.SelectedRow[EDIT_BROKER_REMARK_FIELD]);
                    string oldRemark = CommonUtils.ConvertToString(this.SelectedRow[BROKER_REMARK_FIELD]);

                    if (remark != oldRemark)
                    {
                        this.SelectedRow[BROKER_REMARK_FIELD] = remark;
                    }
                }
            }
            
        }

        /// <summary>
        /// 取消编辑修改
        /// </summary>
        private void CancelRow()
        {
            if (this.SelectedRow != null && this.SelectedRow.Row.RowState != DataRowState.Detached)
            {
                this.SelectedRow[ISEDIT_FIELD] = "2";
                CPNTextChange();
            }
        }

        /// <summary>
        /// 删除数据
        /// </summary>
        /// <param name="deleteBtn"></param>
        private void DelRow(Button deleteBtn)
        {
            if (deleteBtn != null)
            {
                BookEditView bookEditView = CommonUtils.GetParentObject<BookEditView>(deleteBtn) as BookEditView;
                BookEditViewModel bookEditViewModel = (bookEditView.DataContext as BookEditViewModel);
                if (Common.View.Component.CustomMessageBox.Question("是否要删除当前记录?", "删除", bookEditView))
                {
                    if (SelectedRow != null)
                    {
                        long qty = CommonUtils.ConvertToLong(this.SelectedRow.Row[QTY_FIELD] == null ? "0" : this.SelectedRow.Row[QTY_FIELD].ToString());
                        if (IsSelf)
                        {
                            bookEditViewModel.SelfQty -= qty;
                        }
                        else
                        {
                            bookEditViewModel.CounterPartQty -= qty;
                        }
                        this.Datas.Rows.Remove(this.SelectedRow.Row);
                    }
                }
            }
        }

        /// <summary>
        /// 添加销售手工数据
        /// </summary>
        /// <param name="addBtn"></param>
        private void AddManualRow(Button addBtn)
        {
            if (addBtn != null)
            {
                BookEditView bookEditView = CommonUtils.GetParentObject<BookEditView>(addBtn) as BookEditView;
                BookEditViewModel bookEditViewModel = (bookEditView.DataContext as BookEditViewModel);
                if (bookEditViewModel != null)
                {
                    if (!string.IsNullOrEmpty(bookEditViewModel.SecurityID))
                    {
                        securityID = bookEditViewModel.SecurityID;
                        DataRow dr_manual = this.Datas.NewRow();
                        dr_manual[ISEDIT_FIELD] = "0";
                        SetIsEdit4PublicAccount(dr_manual);
                        dr_manual[SOURCETRADER_FIELD] = "TBOOK";
                        dr_manual[EDIT_SETTLETYPE_DATE_FIELD] = DateTimeEx.ServerTime;
                        dr_manual[EDIT_SETTLETYPE_FIELD] = 0;
                        if (securityID.EndsWith("IB"))
                        {
                            SendPriceTypeVisibility = "0";
                            PositionVisibility = "2";
                            dr_manual[EDIT_TRADEMETHOD_FIELD] = "待定";
                        }
                        else
                        {
                            SendPriceTypeVisibility = "2";
                            PositionVisibility = "0";
                        }
                        this.Datas.Rows.Add(dr_manual);
                    }
                    else
                    {
                        Common.View.Component.CustomMessageBox.Error("债券代码不可为空", null, bookEditView);
                    }
                }
            }
        }

        /// <summary>
        /// 添加经济商数据
        /// </summary>
        /// <param name="AddBrokerBtn"></param>
        private void AddBrokerRow(Button AddBrokerBtn)
        {
            if (AddBrokerBtn != null)
            {
                BookEditView bookEditView = CommonUtils.GetParentObject<BookEditView>(AddBrokerBtn) as BookEditView;
                BookEditViewModel bookEditViewModel = (bookEditView.DataContext as BookEditViewModel);
                // 显示“经纪商成交单”
                // 一条记录只能打开一个窗口
                if (WindowManager.Instance.IsExist(token))
                {
                    return;
                }

                if (string.IsNullOrEmpty(bookEditViewModel.SecurityID))
                {
                    Common.View.Component.CustomMessageBox.Info("请输入债券代码！","提示", bookEditView);
                    return;
                }

                BrokerDetailListView brokerDetailListView = new BrokerDetailListView(this.Datas, bookEditViewModel, this.Broker);
                brokerDetailListView.Owner = bookEditView;
                brokerDetailListView.Show();

                WindowManager.Instance.OpenOnlyOneWindow(token, brokerDetailListView);
            }
        }

        /// <summary>
        /// 添加IDB数据
        /// </summary>
        /// <param name="AddIDBBtn"></param>
        private void AddIDBRow(Button AddIDBBtn)
        {
            if (AddIDBBtn != null)
            {
                BookEditView bookEditView = CommonUtils.GetParentObject<BookEditView>(AddIDBBtn) as BookEditView;
                BookEditViewModel bookEditViewModel = (bookEditView.DataContext as BookEditViewModel);
                //  显示“代客IDB订单”
                // 一条记录只能打开一个窗口
                if (WindowManager.Instance.IsExist(token))
                {
                    return;
                }
                if (string.IsNullOrEmpty(bookEditViewModel.SecurityID))
                {
                    Common.View.Component.CustomMessageBox.Info("请输入债券代码！", "提示", bookEditView);
                    return;
                }

                IDBDetailListView idbDetailListView = new IDBDetailListView(this.Datas, bookEditViewModel, IsSelf, SaleItem);
                idbDetailListView.Owner = bookEditView;
                idbDetailListView.Show();

                WindowManager.Instance.OpenOnlyOneWindow(token, idbDetailListView);
            }
        }
    }
}
