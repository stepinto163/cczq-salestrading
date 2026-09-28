using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Position.Plugin.Model
{
    /// <summary>
    /// 用户持仓组合名称表
    /// </summary>
    public class PostionBookData : ViewModelBase
    {
        /// <summary>
        /// 组合ID
        /// </summary>
        public int id { get; set; }
        /// <summary>
        /// 类型
        /// </summary>
        private bool _ischecked = false;
        public bool IsChecked
        {
            get 
            { 
                return _ischecked; 
            }
            set
            {
                _ischecked = value;
                this.OnPropertyChanged(nameof(IsChecked));
            }
        }
        /// <summary>
        /// 组合名称
        /// </summary>
        public string bookname { get; set; }
        public string closeby { get; set; }
        public string userid { get; set; }
        public string username { get; set; }
        public string createtime { get; set; }
        public string updatetime { get; set; }
        public string remark { get; set; }
        public string inf1 { get; set; }
        public string inf2 { get; set; }
        public string inf3 { get; set; }
        public string inf4 { get; set; }
        public string inf5 { get; set; }
    }
}
