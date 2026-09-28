using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace STC.Chrome.Plugin.Model
{
    public class UserInfoClassModel
    {
        public Common.Utils.UserInfo UserInfo { get; set; }

        private string code = "0000";
        public string Code 
        {
            get
            {
                return code;
            }
            set
            {
                code = value;
            }
        }

        private string msg = "Success";
        public string Msg 
        {
            get
            {
                return msg;
            }
            set
            {
                msg = value;
            }
        }
        
    }
}
