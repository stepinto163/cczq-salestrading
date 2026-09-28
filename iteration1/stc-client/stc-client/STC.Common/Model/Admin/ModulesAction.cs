using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.Admin
{
    /// <summary>
    /// 模块动作
    /// </summary>
    public class ModulesAction
    {
        public string actionname { get; set; }
        public string id;
        public string moduleid;
        public string remark { get; set; }
        public string actionStatus;//0删除，1增加

        public string closeby;
        public string createtime;
        public string ip;
        public string terminal;
        public string updatetime;

        public string inf1;
        public string inf2;
        public string inf3;
        public string inf4;
        public string inf5;
        #region 用于ui界面展示
        public bool IsSelected { get; set; }
        public bool IsChange
        {
            get
            {
                if (initSelect == IsSelected)
                    return false;
                else
                    return true;
            }
        }
        public bool initSelect = false;
        #endregion
    }
}
