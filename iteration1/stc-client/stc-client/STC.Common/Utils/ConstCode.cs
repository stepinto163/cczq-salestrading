using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace STC.Common.Utils
{
    public partial class ConstCode:ResourceDictionary
    {
        /*private ConstCode()
        {
            InitializeComponent();
        }*/

        private static ConstCode constCode = new ConstCode();
        public static ConstCode Instance { get { return constCode; } }
        /// <summary>
        /// 根据 LoginResult中的Code返回错误信息
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public string getMsg(string code)
        {
            if (!string.IsNullOrWhiteSpace(code) && this.Contains(code))
            {
                return this[code] + "";
            }
            return string.Format("code:{0},Unknown", code);
        }
        public string getMsg(string code,string rejectText)
        {
            if (!string.IsNullOrWhiteSpace(code) && this.Contains(code))
            {
                return this[code] + "";
            }
            else 
            {
                return rejectText ;
            }
            return string.Format("code:{0},Unknown", code);
        }
    }
}
