using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.Admin
{
    public class TUserRequest
    {
        private string _userid;
        private string _username = "";//登录名
        private string _password;//密码
        private string _newpass;//密码

        public string userid
        {
            get
            {
                return _userid;
            }

            set
            {
                _userid = value;
            }
        }

        public string username
        {
            get
            {
                return _username;
            }

            set
            {
                _username = value;
            }
        }

        public string password
        {
            get
            {
                return _password;
            }

            set
            {
                _password = value;
            }
        }

        public string newpass
        {
            get
            {
                return _newpass;
            }

            set
            {
                _newpass = value;
            }
        }
    }
}
