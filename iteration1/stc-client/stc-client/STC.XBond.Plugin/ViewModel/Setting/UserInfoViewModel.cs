using Newtonsoft.Json;
using STC.Common.Handler.Admin;
using STC.Common.Model;
using STC.Common.Model.Admin;
using STC.Common.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace STC.XBond.Plugin.ViewModel.Setting
{
    public class UserInfoViewModel : BaseViewModel
    {
        private bool isLoad = false;
        private bool _isSafeModel = false;

        private string _updatePwdDay;
        public string UpdatePwdDay
        {
            get { return _updatePwdDay; }
            set { _updatePwdDay = value; }
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
                }
                return _userInfos;
            }

            set
            {
                _userInfos = value;
                this.OnPropertyChanged(nameof(UserInfos));
            }
        }

        public bool IsSafeModel { get { return _isSafeModel; } }

        private bool _ispowerPwd = false;
        public bool IspowerPwd { get { return _ispowerPwd; } }
        public void Init()
        {
            UserInfos.Clear();
            GetUserInfo();
        }

        private void SetSafeModel(TSafeManger safeManger)
        {
            if (safeManger != null)
            {
                if (safeManger.Code.Equals("0000") && safeManger.Datas != null && safeManger.Datas.tbookSystemParameterList != null && safeManger.Datas.tbookSystemParameterList.Count > 0)
                {
                    TTbookSystemParameter tbookSystemMessage = safeManger.Datas.tbookSystemParameterList[0];
                    if (tbookSystemMessage.param_value != null)
                    {
                        TSafeMangerReq safeMangerReq = JsonConvert.DeserializeObject<TSafeMangerReq>(tbookSystemMessage.param_value);

                        if (safeMangerReq.powerPwd == "1" || !string.IsNullOrEmpty(safeMangerReq.updatePwdTime) || !string.IsNullOrEmpty(safeMangerReq.stopPwdTimes))
                        {
                            _ispowerPwd = "1".Equals(safeMangerReq.powerPwd) ? true : false;
                            _isSafeModel = true;
                            _updatePwdDay = safeMangerReq.updatePwdTime;
                        }
                    }
                }
            }
        }


        private void GetUserInfo()
        {
            ThreadPoolHelper.InvokeThreadPool((o) =>
            {
                TUsersQuery result = AdminHandler.Instance.usersQuery();

                TSafeMangerReq safeMangerReq = new TSafeMangerReq();
                safeMangerReq.action = "1";

                TSafeManger safeManger = AdminHandler.Instance.safeMangerService(safeMangerReq);

                SetSafeModel(safeManger);

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
                    //this.OnPropertyChanged("UserInfos");

                });
            });
        }

        #endregion

        private void OnUserInfoClick()
        {
            if (!isLoad)
            {
                UserInfos.Clear();
                this.OnPropertyChanged("UserInfos");

                GetUserInfo();

                isLoad = true;
            }
        }
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
