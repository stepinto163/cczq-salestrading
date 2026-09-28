using com.gateway.log;
using STC.Common.Handler.Admin;
using STC.Common.Model;
using STC.Common.Model.Admin;
using STC.Common.Model.Fix.Message;
using STC.Common.Utils;
using STC.Common.View.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace STC.Frame.UI
{
    class UpdateUserPwdViewModel : BaseViewModel
    {
        private Window _ownUI;

        #region

        private bool _confirmEnable = true;
        public bool ConfirmEnable
        {
            get { return _confirmEnable; }
            set { _confirmEnable = value; this.OnPropertyChanged(); }
        }

        private Brush _oldPwdBorderBrush;
        public Brush OldPwdBorderBrush
        {
            get { return _oldPwdBorderBrush; }
            set { _oldPwdBorderBrush = value; this.OnPropertyChanged(); }
        }

        private Brush _newPwdBorderBrush;
        public Brush NewPwdBorderBrush
        {
            get { return _newPwdBorderBrush; }
            set { _newPwdBorderBrush = value; this.OnPropertyChanged(); }
        }

        private Brush _newCopyPwdBorderBrush;
        public Brush NewCopyPwdBorderBrush
        {
            get { return _newCopyPwdBorderBrush; }
            set { _newCopyPwdBorderBrush = value; this.OnPropertyChanged(); }
        }


        private Visibility _oldPwdTxtVisibillity = Visibility.Collapsed;
        public Visibility OldPwdTxtVisibillity
        {
            get { return _oldPwdTxtVisibillity; }
            set { _oldPwdTxtVisibillity = value; this.OnPropertyChanged(); }
        }

        private Visibility _newPwdTxtVisibillity = Visibility.Collapsed;
        public Visibility NewPwdTxtVisibillity
        {
            get { return _newPwdTxtVisibillity; }
            set { _newPwdTxtVisibillity = value; this.OnPropertyChanged(); }
        }

        private Visibility _newCopyPwdTxtVisibillity = Visibility.Collapsed;
        public Visibility NewCopyPwdTxtVisibillity
        {
            get { return _newCopyPwdTxtVisibillity; }
            set { _newCopyPwdTxtVisibillity = value; this.OnPropertyChanged(); }
        }

        private string _updatePwdDay;
        public string UpdatePwdDay
        {
            get { return _updatePwdDay; }
            set { _updatePwdDay = value; this.OnPropertyChanged(); }
        }

        private string _oldPwd;
        public string OldPwd
        {
            get { return _oldPwd; }
            set { _oldPwd = value; this.OnPropertyChanged(); }
        }

        private string _newPwd;
        public string NewPwd
        {
            get { return _newPwd; }
            set { _newPwd = value; this.OnPropertyChanged(); }
        }

        private string _newCopyPwd;
        public string NewCopyPwd
        {
            get { return _newCopyPwd; }
            set { _newCopyPwd = value; this.OnPropertyChanged(); }
        }


        private bool _ispowerPwd;
        public bool IspowerPwd
        {
            get { return _ispowerPwd; }
            set { _ispowerPwd = value; this.OnPropertyChanged(); }
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
        #endregion

        public void Init(Window ownUI)
        {
            _ownUI = ownUI;

            NewCopyPwdBorderBrush = NewPwdBorderBrush = OldPwdBorderBrush = (Brush)_ownUI.FindResource("ColorBrush.Gray15");
        }

        public bool Update(string oldPwd, string newPwd, string newCopyPwd)
        {
            bool status = false;
            ConfirmEnable = false;

            NewCopyPwdBorderBrush = NewPwdBorderBrush = OldPwdBorderBrush = (Brush)_ownUI.FindResource("ColorBrush.Gray15");

            NewCopyPwdTxtVisibillity = NewPwdTxtVisibillity = OldPwdTxtVisibillity = Visibility.Collapsed;

            bool checkResult = Check(oldPwd, newPwd, newCopyPwd);

            if (checkResult)
            {
                TUserRequest po = new TUserRequest();
                po.username = LocalEnv.UserInfo.UserName;
                po.password = oldPwd;
                po.newpass = newPwd;
                try
                {
                    PResult result = AdminHandler.Instance.userUpdate(po);
                    if (result != null)
                    {
                        if (result.Code.Equals("0000"))
                        {
                            status = true;
                            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                            {
                                //_ownUI.Close(); 

                                MessageBoxGUI msgBox = new MessageBoxGUI();
                                msgBox.setWarnMsg("保存成功", true);
                                msgBox.ShowDialog();
                            }));
                        }
                    }
                    else if (result == null || !result.IsSuccess())
                    {
                        status = false;
                        Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                        {
                            DeleteBoxView deleteBoxView = new DeleteBoxView();
                            deleteBoxView.Title = "提示";
                            deleteBoxView.setWarnMsg("系统异常，修改失败");
                            deleteBoxView.Show();
                        }));
                    }
                }
                catch (Exception e)
                {
                    LogHelper.Error(e);
                }
                finally
                {
                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        ConfirmEnable = true;
                    }));
                }
            }
            else
            {
                status = false;
                ConfirmEnable = true;
            }
            return status;
        }

        private bool Check(string oldPwd, string newPwd, string newCopyPwd)
        {
            bool result = true;

            if (LocalEnv.UserInfo.Password != oldPwd)
            {
                result = false;

                OldPwdBorderBrush = (Brush)_ownUI.FindResource("ColorBrush.Red11");

                OldPwdTxtVisibillity = Visibility.Visible;
            }

            if (IspowerPwd)
            {
                if (newPwd.Length < 8 || doExcute(newPwd) < 3)
                {
                    result = false;

                    NewPwdTxtVisibillity = Visibility.Visible;

                    PassErrorText = "不低于8个字符，取大小写字母、数字和符号构成";

                    NewPwdBorderBrush = (Brush)_ownUI.FindResource("ColorBrush.Red11");
                }
            }
            else
            {
                if (newPwd.Length < 6)
                {
                    result = false;

                    NewPwdTxtVisibillity = Visibility.Visible;
                    PassErrorText = "密码长度不能小于6位";

                    NewPwdBorderBrush = (Brush)_ownUI.FindResource("ColorBrush.Red11");
                }
                if (newPwd.Length > 16)
                {
                    result = false;

                    NewPwdTxtVisibillity = Visibility.Visible;
                    PassErrorText = "密码长度不能大于16位";

                    NewPwdBorderBrush = (Brush)_ownUI.FindResource("ColorBrush.Red11");
                }
            }

            if (newPwd != newCopyPwd)
            {
                result = false;

                NewCopyPwdTxtVisibillity = Visibility.Visible;

                NewCopyPwdBorderBrush = (Brush)_ownUI.FindResource("ColorBrush.Red11");
            }

            return result;
        }

        static string digital = "[0-9]";
        static string capital = "[A-Z]";
        static string lowercase = "[a-z]";
        static string spec = "[-~!@#$%^&*()_+{}|:<>?;',.]";
        Regex digitalregex = new Regex(digital);
        Regex capitalregex = new Regex(capital);
        Regex lowercaseregex = new Regex(lowercase);
        Regex specregex = new Regex(spec);
        public int doExcute(string password)
        {
            int kindOfCharacter = 0;

            /** 有大写字母 */
            for (int i = 0; i < password.Length; i++)
            {
                if (capitalregex.IsMatch(password.Substring(i, 1)))
                {
                    kindOfCharacter++;
                    break;
                }
            }
            /** 有小写字母*/
            for (int i = 0; i < password.Length; i++)
            {
                if (lowercaseregex.IsMatch(password.Substring(i, 1)))
                {
                    kindOfCharacter++;
                    break;
                }
            }
            /** 有数字 */
            for (int i = 0; i < password.Length; i++)
            {
                if (digitalregex.IsMatch(password.Substring(i, 1)))
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
