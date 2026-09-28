using com.gateway.log;
using Newtonsoft.Json;
using STC.Common.Handler.Admin;
using STC.Common.Handler.SystemParam;
using STC.Common.Model;
using STC.Common.Model.Admin;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.TBook;
using STC.Common.Utils;
using STC.Common.View.Component;
using STC.Common.View.Order.InnerView;
using STC.Common.View.UI;
using STC.Common.ViewModel.Data;
using STC.XBond.Plugin.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace STC.XBond.Plugin.ViewModel.Setting
{
    public class LikeViewModel : BaseViewModel
    {
        private UserControl _myWindow;

        private readonly string BNC = "BNC";
        private readonly string BCO = "BCO";
        private bool isInit = true;
        private string ChannelTxt = "家渠道 (共{0}家)";


        private Visibility priceConditionVisibility = Visibility.Collapsed;
        public Visibility PriceConditionVisibility
        {
            get { return priceConditionVisibility; }
            set
            {
                priceConditionVisibility = value;

                this.OnPropertyChanged();
            }
        }


        #region BCO参数

        private string _allSecondChannelBCO = string.Empty;
        public string AllSecondChannelBCO
        {
            get
            {
                return _allSecondChannelBCO;
            }

            set
            {
                _allSecondChannelBCO = value;
                this.OnPropertyChanged();
            }
        }

        private string _randomNumBCO = string.Empty;
        public string RandomNumBCO
        {
            get
            {
                return _randomNumBCO;
            }

            set
            {
                string old = _randomNumBCO;
                _randomNumBCO = value;
                this.OnPropertyChanged();

                //if (!isInit && old != null && !old.Equals(_randomNumBCO) && DefaultCheckedBCO)
                //    this.UpdateData();
            }
        }


        private bool _isSendOutBCO;

        public bool IsSendOutBCO
        {
            get
            {
                return _isSendOutBCO;
            }

            set
            {
                bool old = _isSendOutBCO;
                _isSendOutBCO = value;
                this.OnPropertyChanged();

                if (value)
                {
                    DefaultCheckedBCO = DirectionCheckedBCO = false;
                    ListBox listBox = _myWindow.FindName("lbBCODirection") as ListBox;
                    listBox.SelectedIndex = -1;

                    SetListBoxItemEnable(listBox, false);
                    listBox = _myWindow.FindName("lbFirstLevelBCO") as ListBox;
                    SetListBoxItemEnable(listBox, false);
                    listBox.SelectedIndex = -1;

                    listBox = _myWindow.FindName("lbSecondLevelBCO") as ListBox;
                    SetListBoxItemEnable(listBox, false);
                    listBox.SelectedIndex = -1;
                }

                //if (old != value && value)
                //    UpdateData();
            }
        }

        private bool _defaultCheckedBCO;
        public bool DefaultCheckedBCO
        {
            get
            {
                return _defaultCheckedBCO;
            }

            set
            {
                bool old = _defaultCheckedBCO;
                _defaultCheckedBCO = value;
                this.OnPropertyChanged();

                if (value)
                {
                    IsSendOutBCO = DirectionCheckedBCO = false;
                    ListBox listBox = _myWindow.FindName("lbBCODirection") as ListBox;
                    listBox.SelectedIndex = -1;


                    SetListBoxItemEnable(listBox, false);
                    listBox = _myWindow.FindName("lbFirstLevelBCO") as ListBox;
                    //SetListBoxItemEnable(listBox, true);

                    listBox = _myWindow.FindName("lbSecondLevelBCO") as ListBox;
                    //SetListBoxItemEnable(listBox, true);

                    TextBox tb = _myWindow.FindName("tbRandomNumBco") as TextBox;
                    tb.IsEnabled = true;
                }
                else
                {
                    ListBox listBox = _myWindow.FindName("lbFirstLevelBCO") as ListBox;
                    SetListBoxItemEnable(listBox, false);
                    listBox.SelectedIndex = -1;

                    listBox = _myWindow.FindName("lbSecondLevelBCO") as ListBox;
                    SetListBoxItemEnable(listBox, false);
                    listBox.SelectedIndex = -1;

                    TextBox tb = _myWindow.FindName("tbRandomNumBco") as TextBox;
                    tb.IsEnabled = false;
                }
                //if (old != value && value)
                //    UpdateData();
            }
        }

        private bool _directionCheckedBCO;
        public bool DirectionCheckedBCO
        {
            get
            {
                return _directionCheckedBCO;
            }

            set
            {
                bool old = _directionCheckedBCO;
                _directionCheckedBCO = value;
                this.OnPropertyChanged();

                if (value)
                {
                    IsSendOutBCO = DefaultCheckedBCO = false;
                    ListBox listBox = _myWindow.FindName("lbBCODirection") as ListBox;
                    //listBox.SelectedIndex = 0;

                    SetListBoxItemEnable(listBox, true);
                    listBox = _myWindow.FindName("lbFirstLevelBCO") as ListBox;
                    SetListBoxItemEnable(listBox, false);
                    listBox.SelectedIndex = -1;

                    listBox = _myWindow.FindName("lbSecondLevelBCO") as ListBox;
                    SetListBoxItemEnable(listBox, false);
                    listBox.SelectedIndex = -1;
                }
                else
                {
                    ListBox listBox = _myWindow.FindName("lbBCODirection") as ListBox;
                    SetListBoxItemEnable(listBox, false);
                    listBox.SelectedIndex = -1;
                }

                //if (old != value && value)
                //    UpdateData();
            }
        }

        private List<ListBoxItemData> _defaultFirstItemDataBCO;
        public List<ListBoxItemData> DefaultFirstItemDataBCO
        {
            get
            {
                return _defaultFirstItemDataBCO;
            }

            set
            {
                _defaultFirstItemDataBCO = value;
                if (value != null)
                    this.OnPropertyChanged();
            }
        }

        private List<ListBoxItemData> _defaultSecondItemDataBCO;
        public List<ListBoxItemData> DefaultSecondItemDataBCO
        {
            get
            {
                return _defaultSecondItemDataBCO;
            }

            set
            {
                _defaultSecondItemDataBCO = value;
                if (value != null)
                    this.OnPropertyChanged();
            }
        }

        private List<ListBoxItemData> _directionItemDataBCO;
        public List<ListBoxItemData> DirectionItemDataBCO
        {
            get
            {
                return _directionItemDataBCO;
            }

            set
            {
                _directionItemDataBCO = value;

                if (value != null)
                    this.OnPropertyChanged();
            }
        }

        #endregion

        #region BNC参数

        private string _allSecondChannel = string.Empty;
        public string AllSecondChannel
        {
            get
            {
                return _allSecondChannel;
            }

            set
            {
                _allSecondChannel = value;
                this.OnPropertyChanged();
            }
        }

        private string _randomNum = string.Empty;
        public string RandomNum
        {
            get
            {
                return _randomNum;
            }

            set
            {
                string old = _randomNum;
                _randomNum = value;
                this.OnPropertyChanged();

                //if (!isInit && old != null && !old.Equals(_randomNum) && DefaultChecked)
                //    this.UpdateData();
            }
        }


        private bool _isSendOut;

        public bool IsSendOut
        {
            get
            {
                return _isSendOut;
            }

            set
            {
                bool old = _isSendOut;
                _isSendOut = value;
                this.OnPropertyChanged();

                if (value)
                {
                    DefaultChecked = DirectionChecked = false;
                    ListBox listBox = _myWindow.FindName("lbDirect") as ListBox;
                    listBox.SelectedIndex = -1;

                    SetListBoxItemEnable(listBox, false);
                    listBox = _myWindow.FindName("lbFirstLevel") as ListBox;
                    SetListBoxItemEnable(listBox, false);
                    listBox.SelectedIndex = -1;

                    listBox = _myWindow.FindName("lbSecondLevel") as ListBox;
                    SetListBoxItemEnable(listBox, false);
                    listBox.SelectedIndex = -1;
                }

                //if (old != value && value)
                //    UpdateData();

            }
        }

        private bool _defaultChecked;
        public bool DefaultChecked
        {
            get
            {
                return _defaultChecked;
            }

            set
            {
                bool old = _defaultChecked;
                _defaultChecked = value;
                this.OnPropertyChanged();

                if (value)
                {
                    IsSendOut = DirectionChecked = false;
                    ListBox listBox = _myWindow.FindName("lbDirect") as ListBox;
                    listBox.SelectedIndex = -1;


                    SetListBoxItemEnable(listBox, false);
                    listBox = _myWindow.FindName("lbFirstLevel") as ListBox;
                    //SetListBoxItemEnable(listBox, true);

                    listBox = _myWindow.FindName("lbSecondLevel") as ListBox;
                    //SetListBoxItemEnable(listBox, true);

                    TextBox tb = _myWindow.FindName("tbRandomNum") as TextBox;
                    tb.IsEnabled = true;

                }
                else
                {
                    ListBox listBox = _myWindow.FindName("lbFirstLevel") as ListBox;
                    SetListBoxItemEnable(listBox, false);
                    listBox.SelectedIndex = -1;

                    listBox = _myWindow.FindName("lbSecondLevel") as ListBox;
                    SetListBoxItemEnable(listBox, false);
                    listBox.SelectedIndex = -1;

                    TextBox tb = _myWindow.FindName("tbRandomNum") as TextBox;
                    tb.IsEnabled = false;
                }

                //if (old != value && value)
                //    UpdateData();
            }
        }

        private bool _directionChecked;
        public bool DirectionChecked
        {
            get
            {
                return _directionChecked;
            }

            set
            {
                bool old = _directionChecked;
                _directionChecked = value;
                this.OnPropertyChanged();

                if (value)
                {
                    IsSendOut = DefaultChecked = false;
                    ListBox listBox = _myWindow.FindName("lbDirect") as ListBox;
                    //listBox.SelectedIndex = 0;


                    SetListBoxItemEnable(listBox, true);
                    listBox = _myWindow.FindName("lbFirstLevel") as ListBox;
                    SetListBoxItemEnable(listBox, false);
                    listBox.SelectedIndex = -1;

                    listBox = _myWindow.FindName("lbSecondLevel") as ListBox;
                    SetListBoxItemEnable(listBox, false);
                    listBox.SelectedIndex = -1;
                }
                else
                {
                    ListBox listBox = _myWindow.FindName("lbDirect") as ListBox;
                    listBox.SelectedIndex = -1;
                    SetListBoxItemEnable(listBox, false);
                }

                //if (old != value && value)
                //    UpdateData();
            }
        }

        private List<ListBoxItemData> _defaultFirstItemData;
        public List<ListBoxItemData> DefaultFirstItemData
        {
            get
            {
                return _defaultFirstItemData;
            }

            set
            {
                _defaultFirstItemData = value;

                if (value != null)
                    this.OnPropertyChanged();
            }
        }

        private List<ListBoxItemData> _defaultSecondItemData;
        public List<ListBoxItemData> DefaultSecondItemData
        {
            get
            {
                return _defaultSecondItemData;
            }

            set
            {
                _defaultSecondItemData = value;

                if (value != null)
                    this.OnPropertyChanged();
            }
        }

        private List<ListBoxItemData> _directionItemData;
        public List<ListBoxItemData> DirectionItemData
        {
            get
            {
                return _directionItemData;
            }

            set
            {
                _directionItemData = value;

                if (value != null)
                    this.OnPropertyChanged();
            }
        }


        #endregion

        private bool _sendPriceConsistencyCheck = true;

        public bool SendPriceConsistencyCheck
        {
            get
            {
                return _sendPriceConsistencyCheck;
            }

            set
            {
                _sendPriceConsistencyCheck = value;
                this.OnPropertyChanged();
            }
        }

        private bool _pointPriceConsistencyCheck = true;

        public bool PointPriceConsistencyCheck
        {
            get
            {
                return _pointPriceConsistencyCheck;
            }

            set
            {
                _pointPriceConsistencyCheck = value;
                this.OnPropertyChanged();
            }
        }

        private bool _PriceMatchCheck = true;

        public bool PriceMatchCheck
        {
            get
            {
                return _PriceMatchCheck;
            }
            set
            {
                _PriceMatchCheck = value;
                this.OnPropertyChanged();
            }
        }

        private bool _TradeBookHeaderCheck = false;

        public bool TradeBookHeaderCheck
        {
            get
            {
                return _TradeBookHeaderCheck;
            }
            set
            {
                _TradeBookHeaderCheck = value;
                this.OnPropertyChanged();
            }
        }

        private bool _saveIsEnable = true;

        public bool SaveIsEnable
        {
            get
            {
                return _saveIsEnable;
            }

            set
            {
                _saveIsEnable = value;
            }
        }


        private bool _cancelTimeEnable = true;

        public bool CancelTimeEnable
        {
            get
            {
                return _cancelTimeEnable;
            }

            set
            {
                _cancelTimeEnable = value;

                if (!value)
                {
                    HourSetting = MinuteSetting = SecondSetting = string.Empty;
                }
                this.OnPropertyChanged();
            }
        }

        private int firmselectindex = 1;
        /// <summary>
        /// 默认NF
        /// </summary>
        public int FirmSelectIndex
        {
            get
            {
                return firmselectindex;
            }

            set
            {
                firmselectindex = value;
                FirmTxt = value == 1 ? NONFIRM : FIRM;
                this.OnPropertyChanged(nameof(FirmSelectIndex));
            }
        }

        private const string FIRM = "firm";
        private const string NONFIRM = "nonfirm";

        private string firmTxt = NONFIRM;
        public string FirmTxt
        {
            get
            {
                return firmTxt;
            }

            set
            {
                firmTxt = value;
                this.OnPropertyChanged(nameof(FirmTxt));
            }
        }

        #region 树状分组数据源

        private ObservableCollection<TreeNodeModel> _MultiTreeComboxItemsSource = new ObservableCollection<TreeNodeModel>();
        public ObservableCollection<TreeNodeModel> MultiTreeComboxItemsSource
        {
            get { return _MultiTreeComboxItemsSource; }
            set
            {
                if (_MultiTreeComboxItemsSource != value)
                {
                    _MultiTreeComboxItemsSource = value;
                    this.OnPropertyChanged();
                }
            }
        }


        private string _SelectedItem = null;
        public string SelectedItem
        {
            get { return _SelectedItem; }
            set
            {
                _SelectedItem = value;
                this.OnPropertyChanged();
            }
        }


        private string _SelectedText = null;
        public string SelectedText
        {
            get { return _SelectedText; }
            set
            {
                _SelectedText = value;
                this.OnPropertyChanged();
            }
        }
        #endregion

        #region  报价有效时长属性
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

                this.OnPropertyChanged(nameof(ValidSelectIndex));
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
                this.OnPropertyChanged(nameof(DefaultValidUntilTime));
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

                this.OnPropertyChanged(nameof(DefaultValidTimeQuotationCheck));
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
                this.OnPropertyChanged(nameof(HourValidTimeQuotationCheck));
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
                    this.OnPropertyChanged(nameof(ValidMinute));
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
                    this.OnPropertyChanged(nameof(ValidSecond));
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
                    this.OnPropertyChanged(nameof(ValidTimeHour));
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
                    this.OnPropertyChanged(nameof(ValidTimeMinute));
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
                    this.OnPropertyChanged(nameof(ValidTimeSecond));
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
                this.OnPropertyChanged(nameof(TimeValidTimeQuotationCheck));
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
                this.OnPropertyChanged(nameof(NoLimitValidTimeQuotationCheck));
            }
        }


        private int validSelectIndexRfq = 0;
        /// <summary>
        /// Rfq
        /// </summary>
        public int ValidSelectIndexRfq
        {
            get
            {
                return validSelectIndexRfq;
            }

            set
            {
                validSelectIndexRfq = value;
                this.OnPropertyChanged(nameof(ValidSelectIndexRfq));
            }
        }

        /// <summary>
        /// 时长 - 分钟
        /// </summary>
        private string validMinuteRfq = string.Empty;
        public string ValidMinuteRfq
        {
            get { return validMinuteRfq; }
            set
            {
                if (value != null)
                {
                    validMinuteRfq = value;
                    this.OnPropertyChanged(nameof(ValidMinuteRfq));
                }
            }
        }


        private Visibility rfqSettingVisible = Visibility.Collapsed;
        public Visibility RfqSettingVisible
        {
            get { return rfqSettingVisible; }
            set
            {
                if (value != rfqSettingVisible)
                {
                    rfqSettingVisible = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        ///  时长 - 秒钟
        /// </summary>
        private string validSecondRfq = string.Empty;
        public string ValidSecondRfq
        {
            get { return validSecondRfq; }
            set
            {
                if (value != null)
                {
                    validSecondRfq = value;
                    this.OnPropertyChanged(nameof(ValidSecondRfq));
                }
            }
        }


        #region 倒挂撤单

        private string morethanMarketBP;
        /// <summary>
        /// 与市场最优倒挂 bp
        /// </summary>
        public string MorethanMarketBP
        {
            get 
            { 
                return morethanMarketBP; 
            }
            set
            {
                morethanMarketBP = value;
                this.OnPropertyChanged(nameof(MorethanMarketBP));
            }
        }

        private bool morethanMarketBPCheck;
        /// <summary>
        /// 与市场最优倒挂是否选择
        /// </summary>
        public bool MorethanMarketBPCheck
        {
            get { return morethanMarketBPCheck; }
            set
            {
                morethanMarketBPCheck = value;
                this.OnPropertyChanged(nameof(MorethanMarketBPCheck));

            }
        }
        #endregion

        #region 订阅
        private bool checkSyntropy;
        /// <summary>
        /// 该券同向
        /// </summary>
        public bool CheckSyntropy
        {
            get { return checkSyntropy; }
            set { checkSyntropy = value; this.OnPropertyChanged(); }
        }

        
        private bool checkReverse;
        /// <summary>
        /// 该券反向
        /// </summary>
        public bool CheckReverse
        {
            get { return checkReverse; }
            set { checkReverse = value; this.OnPropertyChanged(); }
        }
        #endregion

        #endregion
        public ICommand OkCommand { get { return new RelayCommand(OnOKClick); } }


        private bool _isShowWarn = false;
        private MessageBoxGUI _warnBox = null;
        public void OnOKClick()
        {
            string result = CheckParam();
            if (!string.IsNullOrEmpty(result))
            {
                if (!_isShowWarn)
                {
                    if (_warnBox == null)
                        _warnBox = new MessageBoxGUI();
                    else
                        _warnBox.Activate();

                    _warnBox.setWarnMsg(result);
                    _warnBox.Closed += WarnBox_Closed;
                    _isShowWarn = true;
                    _warnBox.ShowDialog();
                    
                }
                return;
            }

            SaveIsEnable = false;
            UpdateData();
        }

        private void WarnBox_Closed(object sender, EventArgs e)
        {
            MessageBoxGUI win = sender as MessageBoxGUI;
            win.Closed -= WarnBox_Closed;
            _warnBox = null;
            _isShowWarn = false;
        }

        private string CheckParam()
        {
            string result = string.Empty;

            decimal d = 0;
            if (decimal.TryParse(DefaultAmount, out d) && (d % 100) != 0)
                result = string.Format("缺省报量必须是100的整数倍，当前输入值：{0}", DefaultAmount);

            if (string.IsNullOrEmpty(result))
            {
                if (CancelTimeEnable && (string.IsNullOrEmpty(HourSetting) || string.IsNullOrEmpty(MinuteSetting) || string.IsNullOrEmpty(SecondSetting)))
                    result = string.Format("定时撤单已勾选，时分秒必须输入值");
            }

            if (LocTimeEnable)
            {
                if (string.IsNullOrEmpty(LocTimeOut))
                {
                    result = string.Format("点价时长需超过10s");
                }
                else
                {
                    if (int.Parse(LocTimeOut) < 10)
                    {
                        result = string.Format("点价时长需超过10s");
                    }
                }
            }

            return result;
        }

        private void SetListBoxItemEnable(ListBox listBox, bool isEnable)
        {
            foreach (ListBoxItemData itemData in listBox.Items)
            {
                if (itemData != null)
                {
                    itemData.ItemEnable = isEnable;
                }
            }

        }

        #region 偏好设置

        private string _nonfirmTime;
        public string NonfirmTime
        {
            get
            {
                return _nonfirmTime;
            }

            set
            {
                string old = _nonfirmTime;
                _nonfirmTime = value;

                //if (_nonfirmTime != null && !old.Equals(_nonfirmTime))
                //    UpdateData();

                this.OnPropertyChanged();
            }
        }
        

        private string _defaultAmount = "3000";
        public string DefaultAmount
        {
            get
            {
                return _defaultAmount;
            }

            set
            {
                string old = _defaultAmount == null ? string.Empty : _defaultAmount;
                _defaultAmount = value;

                //if (_defaultAmount != null && !old.Equals(_defaultAmount))
                //    UpdateData();

                this.OnPropertyChanged();
            }
        }

        private string _defaultPointAmount = "3000";
        public string DefaultPointAmount
        {
            get
            {
                return _defaultPointAmount;
            }

            set
            {
                string old = _defaultPointAmount == null ? string.Empty : _defaultPointAmount;
                _defaultPointAmount = value;
                
                this.OnPropertyChanged();
            }
        }

        private bool _nonFirmCheck = false;

        public bool NonFirmCheck
        {
            get
            {
                return _nonFirmCheck;
            }

            set
            {
                _nonFirmCheck = value;

                NonfirmTimeEnable = value;

                this.OnPropertyChanged();
            }
        }

        private bool _nonfirmTimeEnable;

        public bool NonfirmTimeEnable
        {
            get
            {
                return _nonfirmTimeEnable;
            }

            set
            {
                _nonfirmTimeEnable = value;

                this.OnPropertyChanged();
            }
        }

        private string _locTimeOut;

        public string LocTimeOut
        {
            get
            {
                return _locTimeOut;
            }

            set
            {
                _locTimeOut = value;

                this.OnPropertyChanged();
            }
        }

        private bool _locTimeEnable;

        public bool LocTimeEnable
        {
            get
            {
                return _locTimeEnable;
            }

            set
            {
                _locTimeEnable = value;
                if (LocTimeEnable)
                {
                    if (string.IsNullOrEmpty(LocTimeOut))
                    {
                        LocTimeOut = "30";
                    }
                    else
                    {
                        if (int.Parse(LocTimeOut) < 10)
                        {
                            LocTimeOut = "10";
                        }
                    }
                }

                this.OnPropertyChanged();
            }
        }

        private string _validUntiTimeSetting;
        public string ValidUntiTimeSetting
        {
            get
            {
                return _validUntiTimeSetting;
            }

            set
            {
                _validUntiTimeSetting = value;

                this.OnPropertyChanged();
            }
        }

        private string _hourSetting;
        public string HourSetting
        {
            get
            {
                return _hourSetting;
            }

            set
            {
                _hourSetting = value;

                this.OnPropertyChanged();
            }
        }

        private string _minuteSetting;
        public string MinuteSetting
        {
            get
            {
                return _minuteSetting;
            }

            set
            {
                _minuteSetting = value;

                this.OnPropertyChanged();
            }
        }

        private string _secondSetting;
        public string SecondSetting
        {
            get
            {
                return _secondSetting;
            }

            set
            {
                _secondSetting = value;

                this.OnPropertyChanged();
            }
        }

        #endregion



        #region  按钮命令

        public ICommand AddCommand { get { return new RelayCommand(OnAddClick); } }
        public ICommand DownCommand { get { return new RelayCommand(OnDownClick); } }

        public ICommand PointAddCommand { get { return new RelayCommand(OnPointAddClick); } }
        public ICommand PointDownCommand { get { return new RelayCommand(OnPointDownClick); } }

        private void OnPointAddClick()
        {
            decimal d = 0;
            if (string.IsNullOrEmpty(_defaultPointAmount))
                DefaultPointAmount = "1000";
            else if (decimal.TryParse(_defaultPointAmount, out d))
            {
                DefaultPointAmount = (d + 1000).ToString();
            }
        }

        private void OnPointDownClick()
        {
            decimal d = 0;
            if (decimal.TryParse(_defaultPointAmount, out d) && (d - 1000) > 0)
            {
                DefaultPointAmount = (d - 1000).ToString();
            }
        }

        private void OnAddClick()
        {
            decimal d = 0;
            if (string.IsNullOrEmpty(_defaultAmount))
                DefaultAmount = "1000";
            else if (decimal.TryParse(_defaultAmount, out d))
            {
                DefaultAmount = (d + 1000).ToString();
            }
        }

        private void OnDownClick()
        {
            decimal d = 0;
            if (decimal.TryParse(_defaultAmount, out d) && (d - 1000) > 0)
            {
                DefaultAmount = (d - 1000).ToString();
            }
        }

        #endregion

        private void GetCommonData(out string nonfirmTime, out string defaultAmount, out string valueTime)
        {
            defaultAmount = string.Empty;
            valueTime = string.Empty;
            nonfirmTime = SystemParamHandler.Instance.GetFirmtonontime();
            //List<UserVenue> userVenues = SystemParamHandler.Instance.GetUserVenueByCondition("BNC", "1");
            //if (userVenues != null && userVenues.Count > 0)
            //{
            //    nonfirmTime = userVenues[0].firmtonontime;
            //}
            //else
            //{
            //    userVenues = SystemParamHandler.Instance.GetUserVenueByCondition("BNC", "2");
            //    if (userVenues != null && userVenues.Count > 0)
            //    {
            //        nonfirmTime = userVenues[0].firmtonontime;
            //    }
            //    else
            //    {
            //        userVenues = SystemParamHandler.Instance.GetUserVenueByCondition("BNC", "3");
            //        if (userVenues != null && userVenues.Count > 0)
            //        {
            //            nonfirmTime = userVenues[0].firmtonontime;
            //        }
            //        else
            //        {
            //            userVenues = SystemParamHandler.Instance.GetUserVenueByCondition("BCO", "1");
            //            if (userVenues != null && userVenues.Count > 0)
            //            {
            //                nonfirmTime = userVenues[0].firmtonontime;
            //            }
            //            else
            //            {
            //                userVenues = SystemParamHandler.Instance.GetUserVenueByCondition("BCO", "2");
            //                if (userVenues != null && userVenues.Count > 0)
            //                {
            //                    nonfirmTime = userVenues[0].firmtonontime;
            //                }
            //                else
            //                {
            //                    userVenues = SystemParamHandler.Instance.GetUserVenueByCondition("BCO", "3");
            //                    if (userVenues != null && userVenues.Count > 0)
            //                    {
            //                        nonfirmTime = userVenues[0].firmtonontime;
            //                    }
            //                    else
            //                        nonfirmTime = string.Empty;
            //                }
            //            }
            //        }
            //    }
            //}

            if (SystemParamHandler.Instance.GetUserSetting() != null)
            {
                defaultAmount = SystemParamHandler.Instance.GetUserSetting().userQty;
                valueTime = SystemParamHandler.Instance.GetUserSetting().validUntiTimeSetting;
                //if (!string.IsNullOrEmpty(defaultAmount))
                //    defaultAmount = string.Empty;
            }
        }

        private void ParseValidTime(string validUntiTimeSetting)
        {
            if (!string.IsNullOrEmpty(validUntiTimeSetting))
            {
                string[] arr = validUntiTimeSetting.Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries);
                if (arr != null && arr.Length > 2)
                {
                    HourSetting = arr[0];
                    MinuteSetting = arr[1];
                    SecondSetting = arr[2];
                }
                else
                    CancelTimeEnable = false;
            }
            else
                CancelTimeEnable = false;
        }

        public void Init(UserControl parentUI)
        {
            try
            {
                _myWindow = parentUI;

                //InitAccountUsers();

                SetRfqVisible();

                GetCommonData(out _nonfirmTime, out _defaultAmount, out _validUntiTimeSetting);

                ParseValidTime(_validUntiTimeSetting);

                NonfirmTime = _nonfirmTime;
                if (!string.IsNullOrEmpty(NonfirmTime))
                    NonFirmCheck = true;
                else
                {
                    //设置默认显示20分钟
                    NonFirmCheck = true;
                    NonfirmTime = "20";
                }

                if (SystemParamHandler.Instance.GetUserSetting() != null && !string.IsNullOrEmpty(SystemParamHandler.Instance.GetUserSetting().userNofirmIOCTimeOut))
                {
                    LocTimeOut = SystemParamHandler.Instance.GetUserSetting().userNofirmIOCTimeOut;
                    LocTimeEnable = true;
                }
                else
                {
                    LocTimeOut = "30";//string.Empty;ConsistencyCheckPo
                    //LocTimeEnable = true;//false;
                }

                if (SystemParamHandler.Instance.GetUserSetting() != null && !string.IsNullOrEmpty(SystemParamHandler.Instance.GetUserSetting().inf2))
                {
                    ConsistencyCheckPo consistencyCheckPo = JsonConvert.DeserializeObject<ConsistencyCheckPo>(SystemParamHandler.Instance.GetUserSetting().inf2);
                    if (consistencyCheckPo != null)
                    {
                        SendPriceConsistencyCheck = consistencyCheckPo.SendPriceConsistencyCheck == "1";
                        PointPriceConsistencyCheck = consistencyCheckPo.PointPriceConsistencyCheck == "1";

                        DefaultPointAmount = consistencyCheckPo.PointPriceQty;

                        //InitDefaultAccount(consistencyCheckPo.AccountId);
                    }
                    else
                    {
                        SendPriceConsistencyCheck = PointPriceConsistencyCheck = false;
                    }

                    //解析默认有效时长
                    ParseValidTimeFormat(consistencyCheckPo.DefaultValidTime);

                    //解析Rfq有效时长
                    ParseRfqValidTimeFormat(consistencyCheckPo.RfqDefaultValidTime);

                    //解析默认类型 0(nonfirm)，未选中：1(firm)
                    ParseDefaultFirmType(consistencyCheckPo.DefaultFirmType);

                    //解析订阅
                    if (string.IsNullOrEmpty(consistencyCheckPo.SubscribeReverse) && string.IsNullOrEmpty(consistencyCheckPo.SubscribeSyntropy))
                    {
                        this.CheckReverse = true;
                        this.CheckSyntropy = false;
                    }
                    else
                    {
                        this.CheckReverse = ParseCheckedType(consistencyCheckPo.SubscribeReverse);
                        this.CheckSyntropy = ParseCheckedType(consistencyCheckPo.SubscribeSyntropy);
                    }

                    //接续手工单默认组合
                    this.SelectedItem = consistencyCheckPo.BookId ?? null;
                }
                else
                {
                    SendPriceConsistencyCheck = PointPriceConsistencyCheck = false;
                }

                if (SystemParamHandler.Instance.GetUserSetting() != null && !string.IsNullOrEmpty(SystemParamHandler.Instance.GetUserSetting().inf3))
                {
                    PriceMatchCheck = SystemParamHandler.Instance.GetUserSetting().inf3 == "0" ? false : true;
                }

                //解析撤单条件
                if (SystemParamHandler.Instance.GetUserSetting() != null && !string.IsNullOrEmpty(SystemParamHandler.Instance.GetUserSetting().priceCondition))
                {
                    PriceCondition priceCondition = JsonConvert.DeserializeObject<PriceCondition>(SystemParamHandler.Instance.GetUserSetting().priceCondition);
                    this.MorethanMarketBPCheck = string.IsNullOrEmpty(priceCondition.MorethanMarketBPCheck) ? false : priceCondition.MorethanMarketBPCheck == "1" ? true : false;
                    this.MorethanMarketBP = CommonUtils.IsNumeric(priceCondition.MorethanMarketBP) ? priceCondition.MorethanMarketBP : "";
                }

                DefaultAmount = _defaultAmount;

                //if (!string.IsNullOrEmpty(_defaultAmount))
                //{
                //    NonFirmCheck = true;
                //}

                DefaultFirstItemData = VenueToListBoxItemData(SystemParamHandler.Instance.GetListVenueByCondition("BNC", "1"));
                SetItemCheckColor(DefaultFirstItemData);

                DefaultSecondItemData = VenueToListBoxItemData(SystemParamHandler.Instance.GetListVenueByCondition("BNC", "2"));
                SetItemCheckColor(DefaultSecondItemData);

                DirectionItemData = VenueToListBoxItemData(SystemParamHandler.Instance.GetListVenueByCondition("BNC", string.Empty));
                SetItemParam(DirectionItemData, "BNC", "3");
                SetItemParam(DefaultSecondItemData, "BNC", "2");
                SetItemCheckColor(DirectionItemData);

                DefaultFirstItemDataBCO = VenueToListBoxItemData(SystemParamHandler.Instance.GetListVenueByCondition("BCO", "1"));
                SetItemCheckColor(DefaultFirstItemDataBCO);

                DefaultSecondItemDataBCO = VenueToListBoxItemData(SystemParamHandler.Instance.GetListVenueByCondition("BCO", "2"));
                SetItemCheckColor(DefaultSecondItemDataBCO);

                DirectionItemDataBCO = VenueToListBoxItemData(SystemParamHandler.Instance.GetListVenueByCondition("BCO", string.Empty));
                SetItemParam(DirectionItemDataBCO, "BCO", "3");
                SetItemParam(DefaultSecondItemDataBCO, "BCO", "2");
                SetItemCheckColor(DirectionItemDataBCO);

                if (!DefaultChecked && !DirectionChecked)
                {
                    List<UserVenue> userVenues = SystemParamHandler.Instance.GetUserVenueByCondition("BNC", "1");
                    IsSendOut = GetUserVenue(userVenues, "1");
                }

                if (!DefaultCheckedBCO && !DirectionCheckedBCO)
                {
                    List<UserVenue> userVenues = SystemParamHandler.Instance.GetUserVenueByCondition("BCO", "1");
                    IsSendOutBCO = GetUserVenue(userVenues, "1");
                }

                //IsSendOut = !DefaultChecked && !DirectionChecked;
                //IsSendOutBCO = !DefaultCheckedBCO && !DirectionCheckedBCO;

                if (DefaultSecondItemData != null)
                    AllSecondChannel = string.Format(ChannelTxt, DefaultSecondItemData.Count);

                if (DefaultSecondItemDataBCO != null)
                    AllSecondChannelBCO = string.Format(ChannelTxt, DefaultSecondItemDataBCO.Count);

                SetItemDataStatus(DefaultFirstItemData, false);
                SetItemDataStatus(DefaultSecondItemData, false);

                SetItemDataStatus(DefaultFirstItemDataBCO, false);
                SetItemDataStatus(DefaultSecondItemDataBCO, false);

                //设置树数据源
                MultiTreeComboxItemsSource = STC.Common.Handler.PP.PPHandler.Instance.GetFundAccountTBookGroup();

                SetTradeBookHeader();

                isInit = false;
            }
            catch (Exception e)
            {
                LogHelper.Error(e);
            }
            finally
            {
                //isInit = false;
            }
        }


        #region 报价选择类型解析
        /// <summary>
        /// 报价选择类型解析  默认 "0|" : 时长 "1|{min}:{sec}" : 时点  "2|{hour}:{min}:{sec}" 无 "3|"
        /// </summary>
        /// <param name="info2"></param>
        private void ParseValidTimeFormat(string info2)
        {
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

        private string GetDefaultValidTimeFormat()
        {
            // Modi:cyc Date:2021-04-13 Func:增加用户选择的 需求有效时长 选项记忆功能
            string validTimeType = DefaultValidTimeQuotationCheck ? "0" : HourValidTimeQuotationCheck ? "1" : TimeValidTimeQuotationCheck ? "2" : NoLimitValidTimeQuotationCheck ? "3" : "0";
            // 默认
            string info1 = string.Empty;
            if (DefaultValidTimeQuotationCheck)
            {
                info1 = $"{validTimeType}|";
            }

            // 时长
            if (HourValidTimeQuotationCheck)
            {
                // ValidMinute,ValidSecond
                string min = string.IsNullOrEmpty(ValidMinute) ? "" : ValidMinute;
                string sec = string.IsNullOrEmpty(ValidSecond) ? "" : ValidSecond;
                info1 = $"{validTimeType}|{min}:{sec}";
            }

            // 时点
            if (TimeValidTimeQuotationCheck)
            {
                // ValidTimeHour ValidTimeMinute ValidTimeSecond
                string hour = string.IsNullOrEmpty(ValidTimeHour) ? "" : ValidTimeHour;
                string min = string.IsNullOrEmpty(ValidTimeMinute) ? "" : ValidTimeMinute;
                string sec = string.IsNullOrEmpty(ValidTimeSecond) ? "" : ValidTimeSecond;
                info1 = $"{validTimeType}|{hour}:{min}:{sec}";
            }

            // 无
            if (NoLimitValidTimeQuotationCheck)
            {
                info1 = $"{validTimeType}|";
            }

            return info1;
        }


        /// <summary>
        /// Rfq选择类型解析  时长 "1|{min}:{sec}"
        /// </summary>
        /// <param name="info2"></param>
        private void ParseRfqValidTimeFormat(string info2)
        {
            if (!string.IsNullOrEmpty(info2))
            {
                if (info2.StartsWith("1|"))
                {
                    ValidSelectIndexRfq = 1;
                    // ValidMinute,ValidSecond
                    string s = info2.Split('|')[1];
                    if (s.Contains(":"))
                    {
                        ValidMinuteRfq = s.Split(':')[0];
                        ValidSecondRfq = s.Split(':')[1];
                    }
                }
            }
        }

        /// <summary>
        /// Rfq
        /// </summary>
        /// <returns></returns>
        private string GetRfqDefaultValidTimeFormat()
        {
            // Modi:cyc Date:2022-12-16 Func:增加用户选择的 Rfq 需求有效时长 选项记忆功能  "1|{min}:{sec}
            string validTimeType = "1";
            // 默认
            string info1 = string.Empty;
            
            // 时长
            {
                // ValidMinute,ValidSecond
                string min = string.IsNullOrEmpty(ValidMinuteRfq) ? "" : ValidMinuteRfq;
                string sec = string.IsNullOrEmpty(ValidSecondRfq) ? "" : ValidSecondRfq;
                info1 = $"{validTimeType}|{min}:{sec}";
            }

            return info1;
        }


        /// <summary>
        /// 解析 默认类型 0(nonfirm)，未选中：1(firm) 到 FirmSelectIndex 0 firm 1 nonfirm
        /// </summary>
        /// <param name="info2"></param>
        private int ParseDefaultFirmType(string info2)
        {
            if (string.IsNullOrEmpty(info2))
            {
                FirmSelectIndex = 1;
            }
            else
            {
                FirmSelectIndex = info2 == "0" ? 1 : 0;
            }

            return FirmSelectIndex;
        }

        /// <summary>
        /// 获取用户默认的类型
        /// </summary>
        /// <returns></returns>
        private string GetDefaultFirmType()
        { 
            return FirmSelectIndex == 0 ? "1" : "0";
        }

        /// <summary>
        /// 解析订阅类型或 bool解析
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        private bool ParseCheckedType(string info)
        {
            if (string.IsNullOrEmpty(info))
            {
                return false;
            }
            else
            {
                return info == "0" ? false : true;
            }
        }

        #endregion


        private bool GetUserVenue(List<UserVenue> userVenues, string selfselectvenues)
        {
            bool result = false;

            if (userVenues != null && userVenues.Count > 0)
            {
                foreach (UserVenue uv in userVenues)
                {
                    if (!string.IsNullOrEmpty(uv.venuesselectf) && uv.venuesselectf.Equals(selfselectvenues))
                    {
                        result = true;
                        break;
                    }
                }
            }

            return result;
        }

        private void SetItemDataStatus(List<ListBoxItemData> itemDatas, bool status)
        {
            if (itemDatas != null)
            {
                foreach (ListBoxItemData item in itemDatas)
                {
                    item.ItemEnable = status;
                }
            }
        }

        private void SetItemCheckColor(List<ListBoxItemData> itemDatas)
        {
            if (itemDatas != null && itemDatas.Count > 0)
            {
                foreach (ListBoxItemData item in itemDatas)
                {
                    item.BgColor = (SolidColorBrush)_myWindow.FindResource("ColorBrush.Red7");
                }
            }
        }

        private void SetItemParam(List<ListBoxItemData> itemDatas, string goodsType, string operationType)
        {
            if (itemDatas != null && itemDatas.Count > 0)
            {
                List<UserVenue> userItemDatas = SystemParamHandler.Instance.GetUserVenueByCondition(goodsType, operationType);

                if (operationType.Equals("3") && userItemDatas != null && userItemDatas.Count > 0)
                {
                    if (goodsType.Equals("BNC"))
                        DirectionChecked = true;
                    else
                        DirectionCheckedBCO = true;
                }

                foreach (ListBoxItemData item in itemDatas)
                {
                    if (userItemDatas != null && userItemDatas.Count > 0)
                    {
                        foreach (UserVenue uv in userItemDatas)
                        {
                            if (!string.IsNullOrEmpty(uv.selfselectvenues) && uv.selfselectvenues.Contains(item.Code))
                            {
                                item.Checked = true;

                                if (goodsType.Equals(BNC))
                                    DirectionChecked = true;
                                else if (goodsType.Equals(BCO))
                                    DirectionCheckedBCO = true;


                                break;
                            }

                            if (goodsType.Equals(BNC) && (!string.IsNullOrEmpty(uv.venuesselectf) && uv.venuesselectf.Equals("2")))
                            {
                                DefaultChecked = true;
                                RandomNum = uv.venuesrdmnum;
                            }
                            else if (goodsType.Equals(BCO) && (!string.IsNullOrEmpty(uv.venuesselectf) && uv.venuesselectf.Equals("2")))
                            {
                                DefaultCheckedBCO = true;
                                RandomNumBCO = uv.venuesrdmnum;
                            }
                        }
                    }
                }

                if (DirectionChecked)
                {
                    foreach (ListBoxItemData item in itemDatas)
                    {
                        item.ItemEnable = true;
                    }
                }

                if (DirectionCheckedBCO)
                {
                    foreach (ListBoxItemData item in itemDatas)
                    {
                        item.ItemEnable = true;
                    }
                }
            }
        }

        private List<ListBoxItemData> GetVenueByCondition(string goodsType, string level)
        {
            List<ListBoxItemData> result = null;

            return result;
        }

        private List<ListBoxItemData> VenueToListBoxItemData(List<Venue> venues)
        {
            List<ListBoxItemData> result = null;

            if (venues != null && venues.Count > 0)
            {
                result = new List<ListBoxItemData>();
                foreach (Venue uv in venues)
                {
                    ListBoxItemData itemData = new ListBoxItemData();
                    itemData.Code = uv.venue;
                    itemData.Name = uv.name;
                    itemData.Seq = uv.sn;
                    itemData.Level = uv.level;

                    result.Add(itemData);

                }
                result.Sort(new ListBoxItemSort());
            }

            return result;
        }

        private bool isSendUpdate = false;
        private void UpdateData()
        {
            //if (isInit)
            //{
            //    SaveIsEnable = true;
            //    return;
            //}
            //else
            {
                UserSetting userSetting = SystemParamHandler.Instance.CopyUserSetting();
                if (userSetting != null)
                {
                    userSetting.systemVenue = null;

                    userSetting.userVenue.Clear();

                    UserVenue bncUserVenue = GetBNCUserVenue();
                    UserVenue bcoUserVenue = GetBCOUserVenue();

                    if (CancelTimeEnable)
                    {
                        if (!string.IsNullOrEmpty(HourSetting) && !string.IsNullOrEmpty(MinuteSetting) && !string.IsNullOrEmpty(SecondSetting))
                            userSetting.validUntiTimeSetting = string.Format("{0}:{1}:{2}", HourSetting.Length == 1 ? ("0" + HourSetting) : HourSetting,
                                MinuteSetting.Length == 1 ? ("0" + MinuteSetting) : MinuteSetting, SecondSetting.Length == 1 ? ("0" + SecondSetting) : SecondSetting);
                    }
                    else
                        userSetting.validUntiTimeSetting = string.Empty;

                    userSetting.userQty = DefaultAmount;
                    userSetting.userNofirmIOCTimeOut = LocTimeEnable ? LocTimeOut : string.Empty;
                    userSetting.userVenue.Add(bncUserVenue);
                    userSetting.userVenue.Add(bcoUserVenue);

                    ConsistencyCheckPo consistencyCheckPo = new ConsistencyCheckPo();
                    if (SendPriceConsistencyCheck)
                        consistencyCheckPo.SendPriceConsistencyCheck = "1";
                    if (PointPriceConsistencyCheck)
                        consistencyCheckPo.PointPriceConsistencyCheck = "1";

                    consistencyCheckPo.PointPriceQty = DefaultPointAmount;

                    string info1 = GetDefaultValidTimeFormat();
                    consistencyCheckPo.DefaultValidTime = info1;
                    string rfqinfo = GetRfqDefaultValidTimeFormat();
                    consistencyCheckPo.RfqDefaultValidTime = rfqinfo;
                    string defaultfirmtype = GetDefaultFirmType();
                    consistencyCheckPo.DefaultFirmType = defaultfirmtype;

                    consistencyCheckPo.SubscribeReverse = CheckReverse ? "1" : "0";
                    consistencyCheckPo.SubscribeSyntropy = CheckSyntropy ? "1" : "0";

                    //if (AccountUserItem != null)
                    //{
                    //    consistencyCheckPo.AccountId = AccountUserItem.Id;
                    //}

                    //手工单默认组合
                    if (!string.IsNullOrEmpty(SelectedItem))
                    {
                        consistencyCheckPo.BookId = SelectedItem;
                        consistencyCheckPo.BookName = SelectedText;
                    }

                    userSetting.inf2 = JsonConvert.SerializeObject(consistencyCheckPo);

                    userSetting.inf3 = PriceMatchCheck == false ? "0" : "1";

                    PriceCondition priceCondition = new PriceCondition();
                    priceCondition.MorethanMarketBPCheck = MorethanMarketBPCheck ? "1" : "0";
                    priceCondition.MorethanMarketBP = string.IsNullOrEmpty(MorethanMarketBP) ? "" : MorethanMarketBP;
                    userSetting.priceCondition = JsonConvert.SerializeObject(priceCondition);

                    isSendUpdate = true;
                    ThreadPoolHelper.InvokeThreadPool(GetSettingParam, userSetting);
                }
                else
                    SaveIsEnable = true;
            }
        }

        private UserVenue GetBNCUserVenue()
        {
            UserVenue userVenue = new UserVenue();
            userVenue.firmtonontime = NonFirmCheck ? NonfirmTime : string.Empty;
            userVenue.type = BNC;

            //DefaultAmount
            if (IsSendOut)
            {
                userVenue.venuesselectf = "1";
            }
            else if (DefaultChecked)
            {
                if (DefaultChecked)
                {
                    userVenue.venuesrdmnum = RandomNum;
                    userVenue.venuesselectf = "2";
                }


            }
            else if (DirectionChecked)
            {
                userVenue.venuesselectf = "3";
                StringBuilder sb = new StringBuilder();
                foreach (ListBoxItemData item in DirectionItemData)
                {
                    if (item.Checked)
                    {
                        sb.Append(item.Code);
                        sb.Append(",");
                    }
                }
                string selectVenues = sb.ToString();
                userVenue.selfselectvenues = !string.IsNullOrEmpty(selectVenues) ? selectVenues.Substring(0, selectVenues.Length - 1) : string.Empty;
            }

            return userVenue;
        }

        private UserVenue GetBCOUserVenue()
        {
            UserVenue userVenue = new UserVenue();
            userVenue.firmtonontime = NonFirmCheck ? NonfirmTime : string.Empty;
            userVenue.type = BCO;

            //DefaultAmount
            if (IsSendOutBCO)
            {
                userVenue.venuesselectf = "1";
            }
            else if (DefaultCheckedBCO)
            {
                if (DefaultCheckedBCO)
                {
                    userVenue.venuesrdmnum = RandomNumBCO;
                    userVenue.venuesselectf = "2";
                }


            }
            else if (DirectionCheckedBCO)
            {
                userVenue.venuesselectf = "3";
                StringBuilder sb = new StringBuilder();
                foreach (ListBoxItemData item in DirectionItemDataBCO)
                {
                    if (item.Checked)
                    {
                        sb.Append(item.Code);
                        sb.Append(",");
                    }
                }
                string selectVenues = sb.ToString();
                if (!string.IsNullOrEmpty(selectVenues))
                    userVenue.selfselectvenues = selectVenues.Substring(0, selectVenues.Length - 1);
            }

            return userVenue;
        }


        private void GetSettingParam(object obj)
        {
            try
            {
                bool isSuccess = SaveTradeBookHeader();
                UserSetting us = obj as UserSetting;
                PResult result = SystemParamHandler.Instance.UpdatesystemParas(us);
                if (result.IsSuccess())
                {
                    SystemParamHandler.Instance.CopyParam(us);

                    if (isSuccess)
                    {
                        Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                        {
                            MessageBoxGUI msgBox = new MessageBoxGUI();
                            msgBox.setWarnMsg("保存成功", true);
                            msgBox.ShowDialog();
                        }));
                    }
                }
            }
            catch (Exception e)
            {
                LogHelper.Error(e);
            }
            finally
            {
                isSendUpdate = false;

                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    SaveIsEnable = true;
                }));
            }
        }

        //#region 账户组合

        //private Visibility _accountUserVisibility = Visibility.Collapsed;

        //public Visibility AccountUserVisibility
        //{
        //    get { return _accountUserVisibility; }
        //    set
        //    {
        //        _accountUserVisibility = value;
        //        this.OnPropertyChanged();
        //    }
        //}

        //private AccountUserItem _accountUserItem = null;

        //public AccountUserItem AccountUserItem

        //{
        //    get { return _accountUserItem; }
        //    set
        //    {
        //        _accountUserItem = value;
        //        this.OnPropertyChanged();
        //    }
        //}

        //private ObservableCollection<AccountUserItem> _accountUserSource = new ObservableCollection<AccountUserItem>();

        //public ObservableCollection<AccountUserItem> AccountUserSource
        //{
        //    get
        //    {
        //        return _accountUserSource;
        //    }
        //}


        //private void InitAccountUsers()
        //{
        //    if (LocalEnv.ConfInfo.IsAccountUser != "1")
        //        return;

        //    AccountUserVisibility = Visibility.Visible;

        //    _accountUserSource.Add(new AccountUserItem());
        //    AccountAndUsersQueryReplyPo result = AdminHandler.Instance.getAccountUser();
        //    if (result != null && result.Datas != null)
        //    {
        //        foreach (TbookAccountUsers accountUser in result.Datas)
        //        {
        //            _accountUserSource.Add(new AccountUserItem() { Id = accountUser.accountid, AccountName = accountUser.accountname });
        //        }
        //    }
        //}

        //private void InitDefaultAccount(string defaultAccountId)
        //{
        //    if (_accountUserSource != null)
        //    {
        //        foreach(AccountUserItem item in _accountUserSource)
        //        {
        //            if (item.Id == defaultAccountId)
        //            {
        //                this.AccountUserItem = item;
        //            }
        //        }
        //    }
        //}

        //#endregion


        private void SetRfqVisible()
        {
            bool isExistPlugin = false;
            //判断是否有RFQ权限
            bool isPermission = AdminHandler.Instance.CheckPermisionById("4000", "4016");
            if (isPermission)
            {
                if (Directory.Exists(System.IO.Path.Combine(System.Environment.CurrentDirectory, "plugin")))
                {
                    FileInfo[] files = new DirectoryInfo(System.IO.Path.Combine(System.Environment.CurrentDirectory, "plugin")).GetFiles("*.dll");
                    foreach (FileInfo info2 in files)
                    {
                        if (info2.Name.Equals("STC.Rfq.Plugin.dll"))
                        {
                            isExistPlugin = true;
                            break;
                        }
                    }
                }
            }

            RfqSettingVisible = isExistPlugin ? Visibility.Visible : Visibility.Collapsed;
        }

        #region TradeBook页签标题设置 

        private void SetTradeBookHeader()
        {
            SystemParameterReq req = new SystemParameterReq();
            req.ParamKey = CommonUtils.PARAM_KEY_TRADEBOOKHEADER;
            req.Username = LocalEnv.UserInfo.UserName;
            TbookSystemParameterReplyPo reply = AdminHandler.Instance.querySystemParameter(req);
            if (reply != null && reply.IsSuccess() && reply.Datas != null && reply.Datas.tbookSystemParameter != null && !string.IsNullOrEmpty(reply.Datas.tbookSystemParameter.param_value))
            {
                CommonUtils.TradeBookHeader = reply.Datas.tbookSystemParameter.param_value;
                if (CommonUtils.TradeBookHeader == "1")
                {
                    TradeBookHeaderCheck = true;
                }
            }
        }

        private bool SaveTradeBookHeader()
        {
            SystemGlobalParamReq req = new SystemGlobalParamReq();
            req.action = "2";
            req.list = new List<TTbookSystemParameter>();
            TTbookSystemParameter param = new TTbookSystemParameter();
            param.param_key = CommonUtils.PARAM_KEY_TRADEBOOKHEADER;
            param.username = LocalEnv.UserInfo.UserName;
            param.param_value = TradeBookHeaderCheck ? "1" : "0";
            param.param_desc = "TradeBook页签标题设置";
            req.list.Add(param);

            PResult reply = AdminHandler.Instance.updateSystemGlobalPara(req);

            bool isSuccess = reply != null || reply.IsSuccess();
            if (isSuccess)
            {
                CommonUtils.TradeBookHeader = param.param_value;
                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    TabPageHelper.SwitchTradeBookHeader();
                }));
                
            }
            return isSuccess;
        }

        #endregion
    }

    public class CustomChannel : ViewModelBase
    {
        #region 发送渠道

        private string _randomNum;
        public string RandomNum
        {
            get
            {
                return _randomNum;
            }

            set
            {
                _randomNum = value;
                this.OnPropertyChanged();
            }
        }


        private bool _isSendOut;

        public bool IsSendOut
        {
            get
            {
                return _isSendOut;
            }

            set
            {
                _isSendOut = value;
                this.OnPropertyChanged();
            }
        }

        private bool _defaultChecked;
        public bool DefaultChecked
        {
            get
            {
                return _defaultChecked;
            }

            set
            {
                _defaultChecked = value;
                this.OnPropertyChanged();
            }
        }

        private bool _directionChecked;
        public bool DirectionChecked
        {
            get
            {
                return _directionChecked;
            }

            set
            {
                _directionChecked = value;
                this.OnPropertyChanged();
            }
        }

        private List<ListBoxItemData> _defaultFirstItemData;
        public List<ListBoxItemData> DefaultFirstItemData
        {
            get
            {
                return _defaultFirstItemData;
            }

            set
            {
                _defaultFirstItemData = value;
            }
        }

        private List<ListBoxItemData> _defaultSecondItemData;
        public List<ListBoxItemData> DefaultSecondItemData
        {
            get
            {
                return _defaultSecondItemData;
            }

            set
            {
                _defaultSecondItemData = value;
            }
        }

        private List<ListBoxItemData> _directionItemData;
        public List<ListBoxItemData> DirectionItemData
        {
            get
            {
                return _directionItemData;
            }

            set
            {
                _directionItemData = value;
            }
        }


        #endregion
    }
}
