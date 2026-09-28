using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace STC.Frame.Utils
{
    /// <summary>
    /// 错误代码处理类
    /// </summary>
    public partial class ConstCode:ResourceDictionary
    {
        private ConstCode()
        {
            InitializeComponent();
        }

        private static ConstCode constCode = new ConstCode();
        public static ConstCode Instance { get { return constCode; } }

        public string getMsg(string code)
        {
            if (!string.IsNullOrWhiteSpace(code) && this.Contains(code))
            {
                return this[code] + "";
            }
            return string.Format("code:{0},Unknown", code);
        }
        
        public bool isContainCode(string code)
        {
            return this.Contains(code);
        }

    }
}
