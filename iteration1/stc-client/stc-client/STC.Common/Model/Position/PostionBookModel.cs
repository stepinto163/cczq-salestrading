using STC.Common.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.Position
{
    public class PostionBookModel: ViewModelBase
    {
        /// <summary>
        /// 组合ID
        /// </summary>
        public string id { get; set; }

        /// <summary>
        /// 组合名称
        /// </summary>
        public string bookname { get; set; }
        public bool IsChecked { get; set; }
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
