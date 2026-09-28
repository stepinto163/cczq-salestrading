using STC.XBond.Plugin.Utils;
using STC.Common.Handler.Admin;
using STC.Common.Handler.SystemParam;
using STC.Common.Model.Admin;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.TBook;
using STC.Common.Utils;
using STC.Common.View.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Collections.ObjectModel;
using com.gateway.log;
using System.Windows.Media;

namespace STC.XBond.Plugin.ViewModel
{
    /// <summary>
    /// 用户设置UserSettingView的数据内容
    /// </summary>
    public class UserSettingModel : ViewModelBase
    {
        private Window _myWindow;
        private readonly string BNC = "BNC";
        private readonly string BCO = "BCO";
        private bool isInit = true;
        private string ChannelTxt = "家渠道 (共{0}家)";

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

        #region 按钮样式

        private Brush _individualForgound;
        public Brush IndividualForgound
        {
            get { return _individualForgound; }
            set
            {
                _individualForgound = value;

                this.OnPropertyChanged();
            }
        }

        private Brush _individualBackgound;
        public Brush IndividualBackgound
        {
            get { return _individualBackgound; }
            set
            {
                _individualBackgound = value;

                this.OnPropertyChanged();
            }
        }

        private Brush _favoriteForgound;
        public Brush FavoriteForgound
        {
            get { return _favoriteForgound; }
            set
            {
                _favoriteForgound = value;

                this.OnPropertyChanged();
            }
        }

        private Brush _favoriteBackgound;
        public Brush FavoriteBackgound
        {
            get { return _favoriteBackgound; }
            set
            {
                _favoriteBackgound = value;

                this.OnPropertyChanged();
            }
        }


        #endregion

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

        public ICommand OkCommand { get { return new RelayCommand(OnOKClick); } }


        private bool _isShowWarn = false;
        private MessageBoxGUI _warnBox = null;
        private void OnOKClick()
        {
            if (!CheckParam())
            {
                if (!_isShowWarn)
                {
                    if (_warnBox == null)
                        _warnBox = new MessageBoxGUI();
                    else
                        _warnBox.Activate();

                    _warnBox.setWarnMsg(string.Format("缺省报量必须是100的整数倍，当前输入值：{0}", DefaultAmount));
                    _warnBox.Closed += WarnBox_Closed;
                    _warnBox.ShowDialog();
                    _isShowWarn = true;
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

        private bool CheckParam()
        {
            bool result = true;
            
            decimal d = 0;
            if (decimal.TryParse(DefaultAmount, out d) && (d % 100) > 0)
                result = false;

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

        #region 个人设置
        private ObservableCollection<UserInfo> _userInfos;

        public ObservableCollection<UserInfo> UserInfos
        {
            get
            {
                if (_userInfos == null)
                {
                    _userInfos = new ObservableCollection<UserInfo>();

                    this.OnPropertyChanged();
                }

                return _userInfos;
            }

        }


        private void GetUserInfo()
        {
            ThreadPoolHelper.InvokeThreadPool((o) =>
            {
                TUsersQuery result = AdminHandler.Instance.usersQuery();
                if (result == null)
                    return;

                Application.Current.Dispatcher.Invoke(() =>
                {
                    UserInfo userInfo = new UserInfo();
                    
                    userInfo.Role = result.Datas.rolename;

                    userInfo.Name = LocalEnv.UserInfo.FullName;
                    userInfo.StcAccount = LocalEnv.UserInfo.UserName;
                    userInfo.Pwd = LocalEnv.UserInfo.Password;
                    userInfo.Status = "有效";

                    if (result.Datas.tbookVenueUsers != null && result.Datas.tbookVenueUsers.Count > 0)
                    {
                        foreach (TbookVenueUsers vu in result.Datas.tbookVenueUsers)
                        {
                            if (vu.venuetypegw.ToUpper().Equals("ODM"))
                            {
                                userInfo.CfetsAccount = vu.partysubid;
                                userInfo.CfetsName = vu.partysubname;

                                //UserInfos.Add(userInfo);

                            }

                            if (vu.venuetypegw.ToUpper().Equals("SOR"))
                            {
                                userInfo.QbAccount = vu.partysubid;
                            }
                        }
                    }
                    UserInfos.Add(userInfo);

                    //LogHelper.Info("Refresh datagrid, data:" + userInfo.ToString());
                    this.OnPropertyChanged("UserInfos");
                    
                });
            });
        }

        #endregion

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

                this.OnPropertyChanged();
            }
        }
        #endregion

        #region
        private Visibility _userGridVisibility = Visibility.Visible;
        public Visibility UserGridVisibility
        {
            get
            {
                return _userGridVisibility;
            }

            set
            {
                _userGridVisibility = value;
                this.OnPropertyChanged();
            }
        }

        private Visibility _favoriteGridVisibility = Visibility.Collapsed;
        public Visibility FavoriteGridVisibility
        {
            get
            {
                return _favoriteGridVisibility;
            }

            set
            {
                _favoriteGridVisibility = value;
                this.OnPropertyChanged();
            }
        }


        #endregion


        #region  按钮命令

        public ICommand UserInfoCommand { get { return new RelayCommand(OnUserInfoClick); } }
        public ICommand FavoriteCommand { get { return new RelayCommand(OnFavoriteClick); } }
        public ICommand CloseCommand { get { return new RelayCommand(OnCloseClick); } }
        public ICommand AddCommand { get { return new RelayCommand(OnAddClick); } }
        public ICommand DownCommand { get { return new RelayCommand(OnDownClick); } }

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

        private bool isLoad = false;
        private void OnUserInfoClick()
        {
            IndividualBackgound = (Brush)_myWindow.FindResource("ColorBrush.Black8");
            IndividualForgound = (Brush)_myWindow.FindResource("ColorBrush.Green");

            FavoriteForgound = (Brush)_myWindow.FindResource("ColorBrush.While");
            FavoriteBackgound = (Brush)_myWindow.FindResource("ColorBrush.Gray13");

            UserGridVisibility = Visibility.Visible;
            FavoriteGridVisibility = Visibility.Collapsed;

            //UserInfo userInfo = new UserInfo();
            //userInfo.Name = LocalEnv.UserInfo.UserName;
            //userInfo.StcAccount = LocalEnv.UserInfo.UserId;
            //userInfo.Pwd = LocalEnv.UserInfo.Password;
            if (!isLoad)
            {
                UserInfos.Clear();
                this.OnPropertyChanged("UserInfos");

                GetUserInfo();

                isLoad = true;
            }
        }

        private void OnFavoriteClick()
        {
            FavoriteBackgound = (Brush)_myWindow.FindResource("ColorBrush.Black8");
            FavoriteForgound = (Brush)_myWindow.FindResource("ColorBrush.Green");

            IndividualForgound = (Brush)_myWindow.FindResource("ColorBrush.While");
            IndividualBackgound = (Brush)_myWindow.FindResource("ColorBrush.Gray13");

            UserGridVisibility = Visibility.Collapsed;
            FavoriteGridVisibility = Visibility.Visible;

            isInit = false;
        }

        private void OnCloseClick()
        {
            _myWindow.Close();
        }
        #endregion

        private void GetCommonData(out string nonfirmTime, out string defaultAmount)
        {
            defaultAmount = string.Empty;

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
                //if (!string.IsNullOrEmpty(defaultAmount))
                //    defaultAmount = string.Empty;
            }
        }

        public void Init(Window parentUI)
        {
            try
            {
                _myWindow = parentUI;

                OnUserInfoClick();

                GetCommonData(out _nonfirmTime, out _defaultAmount);

                NonfirmTime = _nonfirmTime;
                if (!string.IsNullOrEmpty(NonfirmTime))
                    NonFirmCheck = true;
                else
                {
                    //设置默认显示20分钟
                    NonFirmCheck = true;
                    NonfirmTime = "20";
                }

                if (!string.IsNullOrEmpty(SystemParamHandler.Instance.GetUserSetting().userNofirmIOCTimeOut))
                {
                    LocTimeOut = SystemParamHandler.Instance.GetUserSetting().userNofirmIOCTimeOut;
                    LocTimeEnable = true;
                }
                else
                {
                    LocTimeOut = "30";//string.Empty;
                    //LocTimeEnable = true;//false;
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
            foreach (ListBoxItemData item in itemDatas)
            {
                item.ItemEnable = status;
            }
        }

        private void SetItemCheckColor(List<ListBoxItemData> itemDatas)
        {
            if (itemDatas != null && itemDatas.Count > 0)
            {
                foreach (ListBoxItemData item in itemDatas)
                {
                    item.BgColor = (Brush)_myWindow.FindResource("ColorBrush.Red7");
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
        public void UpdateData()
        {
            if (isInit)
            {
                SaveIsEnable = true;
                return;
            }
            else
            {
                UserSetting userSetting = SystemParamHandler.Instance.CopyUserSetting();
                if (userSetting != null)
                {
                    userSetting.systemVenue = null;

                    userSetting.userVenue.Clear();

                    UserVenue bncUserVenue = GetBNCUserVenue();
                    UserVenue bcoUserVenue = GetBCOUserVenue();

                    userSetting.userQty = DefaultAmount;
                    userSetting.userNofirmIOCTimeOut = LocTimeEnable ? LocTimeOut : string.Empty;
                    userSetting.userVenue.Add(bncUserVenue);
                    userSetting.userVenue.Add(bcoUserVenue);
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
            userVenue.firmtonontime = NonFirmCheck?NonfirmTime:string.Empty;
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
                UserSetting us = obj as UserSetting;
                PResult result = SystemParamHandler.Instance.UpdatesystemParas(us);
                if (result.IsSuccess())
                {
                    SystemParamHandler.Instance.CopyParam(us);

                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        MessageBoxGUI msgBox = new MessageBoxGUI();
                        msgBox.setWarnMsg("保存成功", true);
                        msgBox.ShowDialog();
                    }));
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

    public class UserInfo : ViewModelBase
    {
        #region 个人资料

        private string _stcAccount;

        public string StcAccount
        {
            get
            {
                return _stcAccount;
            }

            set
            {
                _stcAccount = value;

                this.OnPropertyChanged();
            }
        }

        private string _name;
        public string Name
        {
            get
            {
                return _name;
            }

            set
            {
                _name = value;

                this.OnPropertyChanged();
            }
        }

        private string _pwd;
        public string Pwd
        {
            get
            {
                return _pwd;
            }

            set
            {
                _pwd = value;

                this.OnPropertyChanged();
            }
        }

        private string _qbAccount;
        public string QbAccount
        {
            get
            {
                return _qbAccount;
            }

            set
            {
                _qbAccount = value;

                this.OnPropertyChanged();
            }
        }

        private string _cfetsAccount;
        public string CfetsAccount
        {
            get
            {
                return _cfetsAccount;
            }

            set
            {
                _cfetsAccount = value;

                this.OnPropertyChanged();
            }
        }

        private string _cfetsName;
        public string CfetsName
        {
            get
            {
                return _cfetsName;
            }

            set
            {
                _cfetsName = value;

                this.OnPropertyChanged();
            }
        }

        private string _role;
        public string Role
        {
            get
            {
                return _role;
            }

            set
            {
                _role = value;

                this.OnPropertyChanged();
            }
        }

        private string _status;
        public string Status
        {
            get
            {
                return _status;
            }

            set
            {
                _status = value;

                this.OnPropertyChanged();
            }
        }

        public override string ToString()
        {
            return "Status:" + Status + ", role:" + Role + ", CfetsName:" + CfetsName + ", CfetsAccount:" + CfetsAccount + ", Name:" + Name + ", Account:" + StcAccount;
        }

        #endregion

    }
}
