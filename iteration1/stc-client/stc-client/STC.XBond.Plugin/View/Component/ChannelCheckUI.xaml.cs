using STC.Common.Handler.Admin;
using STC.Common.Handler.SystemParam;
using STC.Common.Model;
using STC.Common.Model.Base;
using STC.Common.Model.TBook;
using STC.Common.Utils;
using STC.XBond.Plugin.Utils;
using STC.XBond.Plugin.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace STC.XBond.Plugin.View.Component
{
    /// <summary>
    /// ChannelCheckUI.xaml 的交互逻辑
    /// </summary>
    public partial class ChannelCheckUI : UserControl
    {
        private ChannelCheckModel _channelCheckModel = null;

        public ChannelCheckUI()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="securityType">
        /// 利率:BNC,信用:BCO
        /// </param>
        /// <param name="isFirm">
        /// 0:firm, 1:nonFirm, 2/null:没有firm/nonfirm，默认设置权限
        /// </param>
        /// <param name="bgColor"></param>
        public void Init(string securityType, Brush bgColor, string isFirm)
        {
            _channelCheckModel = new ChannelCheckModel();
            this.DataContext = _channelCheckModel;

            _channelCheckModel.Init(this, securityType, bgColor, isFirm);
        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            TextBox tb = sender as TextBox;
            Regex regex = new Regex("^[0-9]+$");
            e.Handled = !regex.IsMatch(e.Text);

            if (e.Handled)
                return;
            
            int max = _channelCheckModel.RandomMax;

            int min = 0;
            string text = tb.Text;

            if ((text.Contains(".") && e.Text.Equals("."))
                || (text.Contains("-") && e.Text.Equals("-")))
            {
                e.Handled = true;
                return;
            }

            string selectTxt = tb.SelectedText;
            decimal d = 0;
            if (selectTxt.Length == text.Length)
            {
                if (decimal.TryParse(e.Text, out d) && (d > max || d < min))
                {
                    e.Handled = true;
                    return;
                }

                return;
            }

            int inputIndex = tb.SelectionStart;
            text = text.Insert(inputIndex, e.Text);

            
            if (decimal.TryParse(text, out d) && (d > max || d < min))
            {
                e.Handled = true;
                return;
            }

            string[] arr = text.Split(new string[] { "." }, StringSplitOptions.RemoveEmptyEntries);
            if (arr != null && arr.Length > 1)
            {
                if (decimal.TryParse(arr[1], out d) && arr[1].Length > 8)
                {
                    e.Handled = true;
                    return;
                }
            }
        }


        // 过滤属性
        public static readonly DependencyProperty CheckValueProperty = DependencyProperty.Register("CheckValue", typeof(ChannelCheckParam),
            typeof(ChannelCheckUI), new PropertyMetadata(new ChannelCheckParam()));

        public ChannelCheckParam CheckValue
        {
            get { return (ChannelCheckParam)GetValue(CheckValueProperty); }
            set
            {
                SetValue(CheckValueProperty, value);


                RaiseEvent(new RoutedEventArgs { RoutedEvent = CheckValueChangedEvent });
            }
        }

        public static readonly RoutedEvent CheckValueChangedEvent = EventManager.RegisterRoutedEvent("CheckValueChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ChannelCheckUI));

        public event RoutedEventHandler CheckValueChanged
        {
            add { AddHandler(CheckValueChangedEvent, value); }
            remove { RemoveHandler(CheckValueChangedEvent, value); }
        }

        private void lbDirect_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CheckValue = _channelCheckModel.GetCheckValue();
        }
        
    }

    public class ChannelCheckParam
    {
        /// <summary>
        /// 1:所有
        /// 2:仅内部
        /// 3:xbond
        /// </summary>
        public string CrossVenues { get; set; }

        /// <summary>
        /// 1:不发送
        /// 2:默认
        /// 3:指定
        /// </summary>
        public string VenuesSelectF { get; set; }

        /// <summary>
        /// 前置条件：VenuesSelectF = 2
        /// 随机通道数
        /// </summary>
        public string VenuesRdmNum { get; set; }

        /// <summary>
        /// 前置条件：VenuesSelectF = 3
        /// 选择的通道
        /// 格式：{0}-{1},{0}-{1}
        /// </summary>
        public string SelfSelectVenues { get; set; }

        public string ToString()
        {
            return "CrossVenues:" + CrossVenues + ", VenuesSelectF:" + VenuesSelectF + ", VenuesRdmNum:" + VenuesRdmNum + ", SelfSelectVenues:" + SelfSelectVenues;
        }
    }

    public class ChannelCheckModel : BaseViewModel
    {
        private string _isFirm = string.Empty;
        private ChannelCheckUI _ownControl;
        private bool isInit = true;
        private const string RANDOMTXT = "家渠道 (共{0}家)";
        private readonly string XBOND = "XBOND";

        #region 界面属性

        private Visibility _allowanceChannelVisibility = Visibility.Visible;
        public Visibility AllowanceChannelVisibility
        {
            get
            {
                return _allowanceChannelVisibility;
            }

            set
            {
                _allowanceChannelVisibility = value;

                this.OnPropertyChanged();
            }
        }


        private Visibility _innerDefaultVisibility = Visibility.Visible;
        public Visibility InnerDefaultVisibility
        {
            get
            {
                return _innerDefaultVisibility;
            }

            set
            {
                _innerDefaultVisibility = value;

                this.OnPropertyChanged();
            }
        }

        private Visibility _checkAllVisibility = Visibility.Visible;
        public Visibility CheckAllVisibility
        {
            get
            {
                return _checkAllVisibility;
            }

            set
            {
                _checkAllVisibility = value;

                this.OnPropertyChanged();
            }
        }

        private Visibility _checkInnerVisibility = Visibility.Visible;
        public Visibility CheckInnerVisibility
        {
            get
            {
                return _checkInnerVisibility;
            }

            set
            {
                _checkInnerVisibility = value;

                this.OnPropertyChanged();
            }
        }

        private Visibility _checkXBondVisibility = Visibility.Visible;
        public Visibility CheckXBondVisibility
        {
            get
            {
                return _checkXBondVisibility;
            }

            set
            {
                _checkXBondVisibility = value;

                this.OnPropertyChanged();
            }
        }

        private bool _checkAll = false;
        public bool CheckAll
        {
            get
            {
                return _checkAll;
            }

            set
            {
                _checkAll = value;

                if (value)
                {
                    CheckInner = false;
                    CheckXBond = false;
                    CheckXBondVisibility = Visibility.Collapsed;
                }

                if (!isInit)
                    _ownControl.CheckValue = GetCheckValue();

                this.OnPropertyChanged();
            }
        }

        private bool _checkInner = false;
        public bool CheckInner
        {
            get
            {
                return _checkInner;
            }

            set
            {
                _checkInner = value;

                if (value)
                {
                    CheckAll = false;
                }

                ChangeUI();

                if (!isInit)
                    _ownControl.CheckValue = GetCheckValue();

                this.OnPropertyChanged();
            }
        }

        private bool _checkXBond = false;
        public bool CheckXBond
        {
            get
            {
                return _checkXBond;
            }

            set
            {
                _checkXBond = value;

                if (InnerDefaultVisibility == Visibility.Collapsed && CheckXBondVisibility == Visibility.Visible)
                {
                    if (!_checkXBond)
                        DirectionCheck = false;
                    else
                    {
                        DirectionCheck = true;

                        if (DirectionItems != null)
                        {
                            foreach (ListBoxItemData itemData in DirectionItems)
                            {
                                if (!itemData.Name.ToUpper().Equals(XBOND))
                                {
                                    itemData.Checked = false;
                                    itemData.ItemVisibility = Visibility.Collapsed;
                                }
                                else
                                {
                                    itemData.Checked = true;
                                }
                            }
                        }
                    }
                }

                if (!isInit)
                    _ownControl.CheckValue = GetCheckValue();

                this.OnPropertyChanged();
            }
        }

        private bool _checkNotOut = false;
        public bool CheckNotOut
        {
            get
            {
                return _checkNotOut;
            }

            set
            {
                _checkNotOut = value;

                if (value)
                {
                    DefaultCheck = DirectionCheck = false;
                }

                if (!isInit)
                    _ownControl.CheckValue = GetCheckValue();

                this.OnPropertyChanged();
            }
        }

        private bool defaultCheck = false;
        public bool DefaultCheck
        {
            get
            {
                return defaultCheck;
            }

            set
            {
                defaultCheck = value;

                if (!isInit)
                {
                    ResetListBoxSelected();

                    if (defaultCheck)
                    {
                        RandomEnable = true;
                        CheckNotOut = DirectionCheck = false;

                        DefaultItemEnable = true;

                        DirectionItemEnable = false;

                    }
                    else
                    {
                        RandomEnable = false;
                        DefaultItemEnable = false;

                        ChangeBoxItemBG(DefaultFirstItems);
                        ChangeBoxItemBG(DefaultSecondItems);
                        ChangeBoxItemCheck(DefaultFirstItems, false);
                        ChangeBoxItemCheck(DefaultSecondItems, false);
                    }
                    if (!isInit)
                        _ownControl.CheckValue = GetCheckValue();
                }

                this.OnPropertyChanged();
            }
        }

        private bool directionItemEnable = false;
        public bool DirectionItemEnable
        {
            get { return directionItemEnable; }
            set
            {
                directionItemEnable = value;
                this.OnPropertyChanged();
            }
        }

        private bool defaultItemEnable = false;
        public bool DefaultItemEnable
        {
            get { return defaultItemEnable; }
            set
            {
                defaultItemEnable = value;
                this.OnPropertyChanged();
            }
        }

        private bool randomEnable = false;
        public bool RandomEnable
        {
            get
            {
                return randomEnable;
            }

            set
            {
                randomEnable = value;

                this.OnPropertyChanged();
            }
        }


        private bool directionCheck = false;
        public bool DirectionCheck
        {
            get
            {
                return directionCheck;
            }

            set
            {
                directionCheck = value;

                if (!isInit)
                {
                    ResetListBoxSelected();
                    if (directionCheck)
                    {
                        CheckNotOut = DefaultCheck = false;

                        DirectionItemEnable = true;
                    }
                    else
                    {
                        DirectionItemEnable = false;

                        ChangeBoxItemBG(DirectionItems);
                        ChangeBoxItemCheck(DirectionItems, false);
                    }
                    if (!isInit)
                        _ownControl.CheckValue = GetCheckValue();
                }

                this.OnPropertyChanged();
            }
        }

        private string randomNum;
        public string RandomNum
        {
            get { return randomNum; }
            set
            {
                if (value != null)
                {
                    randomNum = value;
                    if (!isInit)
                        _ownControl.CheckValue = GetCheckValue();
                    this.OnPropertyChanged();
                }
            }
        }

        private string _randomTxt;
        public string RandomTxt
        {
            get
            {
                return _randomTxt;
            }

            set
            {
                _randomTxt = value;
                this.OnPropertyChanged();
            }
        }

        private int _randomMax = 0;

        public int RandomMax
        {
            get { return _randomMax; }
            set
            {
                _randomMax = value;
            }
        }

        #endregion

        #region listbox 动态数据源
        private List<ListBoxItemData> defaultFirstItems;
        public List<ListBoxItemData> DefaultFirstItems
        {
            get
            {
                return defaultFirstItems;
            }

            set
            {
                defaultFirstItems = value;
                this.OnPropertyChanged();
            }
        }


        private List<ListBoxItemData> defaultSecondItems;
        public List<ListBoxItemData> DefaultSecondItems
        {
            get
            {
                return defaultSecondItems;
            }

            set
            {
                defaultSecondItems = value;
                this.OnPropertyChanged();
            }
        }
        private List<ListBoxItemData> directionItems;
        public List<ListBoxItemData> DirectionItems
        {
            get
            {
                return directionItems;
            }

            set
            {
                directionItems = value;
                this.OnPropertyChanged();
            }
        }
        #endregion

        public ChannelCheckParam GetCheckValue()
        {
            ChannelCheckParam result = new ChannelCheckParam();

            result.CrossVenues = CheckAll ? "1" : CheckInner ? "2" : string.Empty;
            if (CheckXBond && CheckXBondVisibility == Visibility.Visible)
                result.CrossVenues = !CheckAll && !CheckInner ? "4":"3";

            result.VenuesRdmNum = RandomEnable ? RandomNum : "0";

            result.VenuesSelectF = CheckNotOut ? "1" : DefaultCheck ? "2" : DirectionCheck ? "3" : string.Empty;

            if (DefaultCheck)
            {
                result.SelfSelectVenues = string.Empty;
            }
            else if (DirectionCheck)
            {
                result.SelfSelectVenues = GetSelectItem(_ownControl.FindName("lbDirect") as ListBox);
            }

            return result;
        }


        public string GetSelectItem(ListBox listBox)
        {
            string result = string.Empty;

            foreach (ListBoxItemData itemData in listBox.SelectedItems)
            {
                if (itemData != null)
                {
                    result += string.Format("{0}-{1},", itemData.Venuetypegw, itemData.Code);
                }
            }

            return string.IsNullOrEmpty(result) ? result : result.Substring(0, result.Length - 1);
        }

        private void ResetListBoxSelected()
        {
            ListBox listBox = _ownControl.FindName("lbSecondLevel") as ListBox;
            listBox.SelectedIndex = -1;

            listBox = _ownControl.FindName("lbFirstLevel") as ListBox;
            listBox.SelectedIndex = -1;

            listBox = _ownControl.FindName("lbDirect") as ListBox;
            listBox.SelectedIndex = -1;
        }

        private void ChangeBoxItemBG(List<ListBoxItemData> itemDatas)
        {
            if (itemDatas != null && itemDatas.Count > 0)
            {
                foreach (ListBoxItemData itemData in itemDatas)
                {
                    itemData.BgColor = _currentBgColor;
                }
            }
        }

        private void ChangeBoxItemCheck(List<ListBoxItemData> itemDatas, bool checkStatus)
        {
            if (itemDatas != null && itemDatas.Count > 0)
            {
                foreach (ListBoxItemData itemData in itemDatas)
                {
                    itemData.Checked = checkStatus;
                }
            }
        }

        #region 初始化

        private Brush _currentBgColor;

        private void InitUI(string goodsType)
        {
            CheckAll = true;
            List<UserVenue> userVenues = SystemParamHandler.Instance.GetUserVenueByCondition(goodsType, "1");
            
            DefaultFirstItems = VenueToListBoxItemData(SystemParamHandler.Instance.GetListVenueByCondition(goodsType, "1"));
            DefaultSecondItems = VenueToListBoxItemData(SystemParamHandler.Instance.GetListVenueByCondition(goodsType, "2"));
            DirectionItems = VenueToListBoxItemData(SystemParamHandler.Instance.GetListVenueByCondition(goodsType, string.Empty));
            DirectionItems.RemoveAll(p => p.Code == "QDMESP");
            SetItemParam(DirectionItems, goodsType, "2");

            if (DefaultFirstItems != null)
            {
                foreach (ListBoxItemData itemData in DefaultFirstItems)
                    itemData.ItemEnable = false;
            }

            if (DefaultSecondItems != null)
            {
                foreach (ListBoxItemData itemData in DefaultSecondItems)
                    itemData.ItemEnable = false;
            }

            int count = 0;

            if (DefaultSecondItems != null)
                count += DefaultSecondItems.Count;

            RandomMax = count;
            RandomTxt = string.Format(RANDOMTXT, count);
        }

        public void Init(ChannelCheckUI ownControl, string security, Brush bgColor, string isFirm)
        {
            _ownControl = ownControl;
            _isFirm = isFirm;
            _currentBgColor = bgColor;
            
            InitUI(security);

            SetPermission();

            ChangeUI();

            isInit = false;

            _ownControl.CheckValue = GetCheckValue();
            
        }

        private void SetPermission()
        {
            bool isAllow = AdminHandler.Instance.CheckPermission(PermissionConsts.MODULEID, PermissionConsts.CHECK_ALL_ACTIONID);
            if (isAllow)
            {
                _checkInnerVisibility = Visibility.Visible;
                this.OnPropertyChanged("CheckInnerVisibility");

                _checkXBondVisibility = Visibility.Visible;
                this.OnPropertyChanged("CheckXBondVisibility");

                _checkAllVisibility = Visibility.Visible;
                this.OnPropertyChanged("CheckAllVisibility");

                _checkAll = true;
                this.OnPropertyChanged("CheckAll");

                _checkAll = true;
                _checkInner = false;
                _checkXBond = false;

                this.OnPropertyChanged("CheckAll");
                this.OnPropertyChanged("CheckInner");
                this.OnPropertyChanged("CheckXBond");

            }
            else
            {
                _checkAll = false;
                this.OnPropertyChanged("CheckAll");

                _checkAllVisibility = Visibility.Collapsed;
                this.OnPropertyChanged("CheckAllVisibility");
                

                isAllow = AdminHandler.Instance.CheckPermission(PermissionConsts.MODULEID, PermissionConsts.CHECK_INNER_ACTIONID);
                if (isAllow)
                {
                    _checkInnerVisibility = Visibility.Visible;
                    this.OnPropertyChanged("CheckInnerVisibility");

                    _checkInner = true;
                    this.OnPropertyChanged("CheckInner");

                    _checkXBondVisibility = Visibility.Collapsed;
                    this.OnPropertyChanged("CheckXBondVisibility");
                }
                else
                {
                    isAllow = AdminHandler.Instance.CheckPermission(PermissionConsts.MODULEID, PermissionConsts.CHECK_INNER_XBOND_ACTIONID);

                    _checkXBondVisibility = isAllow ? Visibility.Visible : Visibility.Collapsed;
                    this.OnPropertyChanged("CheckXBondVisibility");

                    _checkInnerVisibility = isAllow ? Visibility.Visible : Visibility.Collapsed;
                    this.OnPropertyChanged("CheckInnerVisibility");

                    if (isAllow)
                    {
                        _checkInner = true;
                        this.OnPropertyChanged("CheckInner");

                        CheckXBond = true;
                    }
                }
            }
        }

        private bool CheckVenuePermission(string venue)
        {
            bool result = false;

            if (!string.IsNullOrEmpty(venue) && DirectionItems != null && DirectionItems.Count > 0)
            {
                foreach (ListBoxItemData listBoxItemData in DirectionItems)
                {
                    if (listBoxItemData.Name.ToUpper() == (venue.ToUpper()))
                    {
                        result = true;
                        break;
                    }
                }
            }
            else
                result = true;

            return result;
        }

        private bool HasXBondPermission()
        {
            bool isAllow = AdminHandler.Instance.CheckPermission(PermissionConsts.MODULEID, PermissionConsts.CHECK_ALL_ACTIONID)
                || AdminHandler.Instance.CheckPermission(PermissionConsts.MODULEID, PermissionConsts.CHECK_INNER_XBOND_ACTIONID);

            isAllow = CheckVenuePermission(XBOND);

            return isAllow;
        }

        private bool HasInnerPermission()
        {
            bool isAllow = AdminHandler.Instance.CheckPermission(PermissionConsts.MODULEID, PermissionConsts.CHECK_ALL_ACTIONID)
                || AdminHandler.Instance.CheckPermission(PermissionConsts.MODULEID, PermissionConsts.CHECK_INNER_XBOND_ACTIONID)
                || AdminHandler.Instance.CheckPermission(PermissionConsts.MODULEID, PermissionConsts.CHECK_INNER_ACTIONID);

            return isAllow;
        }

        private bool OnlyInnerPermission()
        {
            bool isAllow = AdminHandler.Instance.CheckPermission(PermissionConsts.MODULEID, PermissionConsts.CHECK_INNER_ACTIONID);

            return isAllow;
        }

        private bool HasAllPermission()
        {
            bool isAllow = AdminHandler.Instance.CheckPermission(PermissionConsts.MODULEID, PermissionConsts.CHECK_ALL_ACTIONID);

            return isAllow;
        }

        private void ChangeUI()
        {
            if (_isFirm.Equals("0"))//Firm
            {
                if (HasXBondPermission() && !CheckAll)
                {
                    _checkXBondVisibility = Visibility.Visible;
                    _checkXBond = true;
                }
                else
                {
                    _checkXBondVisibility = Visibility.Collapsed;
                    _checkXBond = false;
                }

                this.OnPropertyChanged("CheckXBondVisibility");
                this.OnPropertyChanged("CheckXBond");
            }
            else if (_isFirm.Equals("1"))//noFirm
            {
                _checkXBondVisibility = Visibility.Collapsed;
                _checkXBond = false;

                this.OnPropertyChanged("CheckXBond");
                this.OnPropertyChanged("CheckXBondVisibility");
            }
            else//null
            {
                if (HasXBondPermission())
                {
                    if (CheckAll)
                    {
                        _checkXBondVisibility = Visibility.Collapsed;
                        _checkXBond = false;
                    }
                    else
                    {
                        _checkXBondVisibility = Visibility.Visible;
                        _checkXBond = true;
                    }
                }
                else
                {
                    _checkXBondVisibility = Visibility.Collapsed;
                    _checkXBond = false;
                }
                this.OnPropertyChanged("CheckXBond");
                this.OnPropertyChanged("CheckXBondVisibility");
            
            }
            
            if (_checkInner)
            {
                AllowanceChannelVisibility = Visibility.Collapsed;
                
                CheckAll = false;

                if (CheckXBondVisibility == Visibility.Visible)
                {
                    CheckXBond = true;

                    OnlyShowXBond();
                }
            }
            else
            {
                AllowanceChannelVisibility = Visibility.Visible;

                _innerDefaultVisibility = Visibility.Visible;
                this.OnPropertyChanged("InnerDefaultVisibility");

                if (DirectionItems != null)
                {
                    foreach (ListBoxItemData itemData in DirectionItems)
                    {
                        itemData.ItemVisibility = Visibility.Visible;
                        itemData.ItemEnable = true;
                    }
                }
            }
        }

        private void OnlyShowXBond()
        {
            _innerDefaultVisibility = Visibility.Collapsed;
            this.OnPropertyChanged("InnerDefaultVisibility");
           
            AllowanceChannelVisibility = Visibility.Visible;
            
            if (CheckInner && CheckInnerVisibility == Visibility.Visible && CheckXBond && CheckXBondVisibility == Visibility.Visible)
            {
                _checkNotOut = false;
                this.OnPropertyChanged("CheckNotOut");
                defaultCheck = false;
                this.OnPropertyChanged("DefaultCheck");

                directionCheck = true;
                this.OnPropertyChanged("DirectionCheck");
            }
            

            if (DirectionItems != null)
            {
                foreach (ListBoxItemData itemData in DirectionItems)
                {
                    if (!itemData.Name.ToUpper().Equals(XBOND))
                        itemData.ItemVisibility = Visibility.Collapsed;
                    else
                    {
                        itemData.Checked = true;
                    }
                }
            }
        }

        private void SetItemParam(List<ListBoxItemData> itemDatas, string goodsType, string operationType)
        {
            if (itemDatas != null && itemDatas.Count > 0)
            {
                foreach (ListBoxItemData item in itemDatas)
                {
                    item.BgColor = _currentBgColor;
                    bool isContain = false;
                    List<UserVenue> userItemDatas = SystemParamHandler.Instance.GetUserVenueByCondition(goodsType, operationType);
                    if (userItemDatas != null && userItemDatas.Count > 0)
                    {
                        foreach (UserVenue uv in userItemDatas)
                        {
                            if (!string.IsNullOrEmpty(uv.selfselectvenues) && uv.selfselectvenues.Contains(item.Code))
                            {
                                item.Checked = true;
                                isContain = true;
                                if (operationType.Equals("2"))
                                    DirectionCheck = true;
                                else
                                    DefaultCheck = true;

                                break;
                            }

                            RandomNum = uv.venuesrdmnum;
                        }
                    }
                    if (!isContain)
                        item.Checked = false;
                }
            }
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
                    itemData.Venuetypegw = uv.venuetypegw;

                    result.Add(itemData);

                }
                result.Sort(new ListBoxItemSort());
            }

            return result;
        }

        private List<ListBoxItemData> UserVenueToListBoxItemData(List<UserVenue> userVenues, string level)
        {
            List<ListBoxItemData> result = null;

            if (userVenues != null && userVenues.Count > 0)
            {
                result = new List<ListBoxItemData>();
                foreach (UserVenue uv in userVenues)
                {
                    string[] arr = uv.selfselectvenues.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string str in arr)
                    {
                        ListBoxItemData itemData = new ListBoxItemData();
                        itemData.Code = str;
                        Venue venue = SystemParamHandler.Instance.GetVenueByCondition(uv.type, str);
                        if (venue != null)
                        {
                            itemData.Name = venue.name;
                            itemData.Seq = venue.sn;
                            itemData.Level = venue.level;
                            itemData.Venuetypegw = venue.venuetypegw;

                            if (string.IsNullOrEmpty(level) || (!string.IsNullOrEmpty(level) && itemData.Level.ToString().Equals(level)))
                                result.Add(itemData);
                        }
                    }
                }
                result.Sort(new ListBoxItemSort());
            }

            return result;
        }

        #endregion
    }
}
