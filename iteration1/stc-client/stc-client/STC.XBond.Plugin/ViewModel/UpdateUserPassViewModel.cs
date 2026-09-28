using Ats.Foundation.Message;
using STC.Common.Handler.Admin;
using STC.Common.Model.Admin;
using STC.Common.Model.Fix.Message;
using STC.Common.Utils;
using STC.Common.View.UI;
using STC.XBond.Plugin.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace STC.XBond.Plugin.ViewModel
{
    public class UpdateUserPassViewModel : ViewModelBase
    {
        private Window _myWindow;
        public UpdateUserPassViewModel()
        {
            PassErrorVisible = Visibility.Hidden;
            ConfirmErrorVisible = Visibility.Hidden;
            CommonVisible = Visibility.Visible;
            SuccessVisible = Visibility.Collapsed;
        }
        private Visibility _PassErrorVisible = Visibility.Collapsed;

        public Visibility PassErrorVisible
        {
            get
            {
                return _PassErrorVisible;
            }

            set
            {
                if (this._PassErrorVisible != value)
                {
                    this._PassErrorVisible = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private Visibility _ConfirmErrorVisible = Visibility.Collapsed;

        public Visibility ConfirmErrorVisible
        {
            get
            {
                return _ConfirmErrorVisible;
            }

            set
            {
                if (this._ConfirmErrorVisible != value)
                {
                    this._ConfirmErrorVisible = value;
                    this.OnPropertyChanged();
                }
            }
        }


        private String _PassErrorText;

        public string PassErrorText
        {
            get
            {
                return _PassErrorText;
            }

            set
            {
                if (this._PassErrorText != value)
                {
                    this._PassErrorText = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private Visibility _SuccessVisible = Visibility.Collapsed;

        public Visibility SuccessVisible
        {
            get
            {
                return _SuccessVisible;
            }

            set
            {
                if (this._SuccessVisible != value)
                {
                    this._SuccessVisible = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private Visibility _CommonVisible = Visibility.Collapsed;

        public Visibility CommonVisible
        {
            get
            {
                return _CommonVisible;
            }

            set
            {
                if (this._CommonVisible != value)
                {
                    this._CommonVisible = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private Visibility _ErrorPassVisible = Visibility.Collapsed;
        public Visibility ErrorPassVisible
        {
            get
            {
                return _ErrorPassVisible;
            }

            set
            {
                if (this._ErrorPassVisible != value)
                {
                    this._ErrorPassVisible = value;
                    this.OnPropertyChanged();
                }
            }
        }
        public ICommand CloseCommand { get { return new RelayCommand(OnCloseClick); } }

        private void OnCloseClick()
        {
            _myWindow.Close();
        }

        public void init(Window parentUI) {
            _myWindow = parentUI;
        }

        public Boolean btOK(String password,String newPass,String confirmPass)
        {
            bool status = false;
            //string encryPass = MsgHelper.Serializer(EncrypterHelper.getEncryptDic(password));
            // type == 1 不带验证的密码
            // type == 2 非纯数字的密码
            CommonVisible = Visibility.Visible;
            SuccessVisible = Visibility.Collapsed;
            ErrorPassVisible = Visibility.Collapsed;
            if (newPass.Length < 6)
            {
                PassErrorText = "密码长度不能小于6位";
                PassErrorVisible = Visibility.Visible;
                ConfirmErrorVisible = Visibility.Hidden;
                status = false;
            }
            else if (newPass.Length > 16)
            {
                PassErrorText = "密码长度不能大于16位";
                PassErrorVisible = Visibility.Visible;
                ConfirmErrorVisible = Visibility.Hidden;
                status = false;
            }
            else
            {
                if (!"".Equals(newPass) && !"".Equals(confirmPass))
                {
                    // && 
                    if (newPass.Equals(confirmPass))
                    {
                        if (doExcute(newPass) >= 1)
                        {
                            PassErrorText = "";
                            PassErrorVisible = Visibility.Hidden;
                            ConfirmErrorVisible = Visibility.Hidden;
                            TUserRequest po = new TUserRequest();
                            po.username = LocalEnv.UserInfo.UserName;
                            po.password = password;
                            po.newpass = newPass;
                            PResult result = AdminHandler.Instance.userUpdate(po);
                            if (result != null)
                            {
                                if (result.Code.Equals("0000"))
                                {
                                    CommonVisible = Visibility.Collapsed;
                                    SuccessVisible = Visibility.Visible;
                                    status = true;
                                }
                                else if (result.Code.Equals("5000"))
                                {
                                    ErrorPassVisible = Visibility.Visible;
                                    status = false;
                                }
                            }
                            else
                            {
                                DeleteBoxView deleteBoxView = new DeleteBoxView();
                                deleteBoxView.Title = "提示";
                                deleteBoxView.setWarnMsg("系统异常，修改失败");
                                deleteBoxView.Show();
                                status = false;
                            }
                        }
                        else
                        {
                            PassErrorText = "6-16个字符，区分大小写";
                            PassErrorVisible = Visibility.Visible;
                            ConfirmErrorVisible = Visibility.Hidden;
                            status = false;
                        }
                    }
                    else
                    {
                        PassErrorText = "";
                        PassErrorVisible = Visibility.Hidden;
                        ConfirmErrorVisible = Visibility.Visible;
                        status = false;
                    }
                }
                else
                {
                    PassErrorVisible = Visibility.Hidden;
                    ConfirmErrorVisible = Visibility.Visible;
                    status = false;
                }
            }
            return status;
        }
        static String digital = "[0-9]";
        static String capital = "[A-Z]";
        static String lowercase = "[a-z]";
        static String spec = "[-~!@#$%^&*()_+{}|:<>?;',.]";
        Regex digitalregex = new Regex(digital);
        Regex capitalregex = new Regex(capital);
        Regex lowercaseregex = new Regex(lowercase);
        Regex specregex = new Regex(spec);
        public int doExcute(String password)
        {
            int kindOfCharacter = 0;

            /** 有大写字母或小写字母，即有字母 */
            for (int i = 0; i < password.Length; i++)
            {
                if (capitalregex.IsMatch(password.Substring(i, 1))
                        || lowercaseregex.IsMatch(password.Substring(i,1)))
                {
                    kindOfCharacter++;
                    break;
                }
            }

            /** 有数字 */
            for (int i = 0; i < password.Length; i++)
            {
                if (digitalregex.IsMatch(password.Substring(i,1)))
                {
                    kindOfCharacter++;
                    break;
                }
            }

            /** 有符号 */
            for (int i = 0; i < password.Length; i++)
            {
                if (specregex.IsMatch(password.Substring(i, 1)))
                {
                    kindOfCharacter++;
                    break;
                }
            }

            if (password.Length < 6)
            {
                kindOfCharacter = 0;
            }

            return kindOfCharacter;
        }
    }
}
