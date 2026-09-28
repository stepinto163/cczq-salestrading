using STC.Common.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Broker.Plugin.Model.Card
{
    /// <summary>
    /// 组债券列表
    /// </summary>
    public class QuotePageModel : BaseViewModel
    {
        private string _userName;
        /// <summary>
        /// 用户名
        /// </summary>
        public string UserName
        {
            get { return _userName; }
            set { _userName = value; this.OnPropertyChanged(); }
        }

       
        private string _groupId;
        /// <summary>
        /// 组Id
        /// </summary>
        public string GroupId
        {
            get { return _groupId; }
            set { _groupId = value; this.OnPropertyChanged(); }
        }

        private string _groupname;
        /// <summary>
        /// 组名
        /// </summary>
        public string GroupName
        {
            get { return _groupname; }
            set { _groupname = value; this.OnPropertyChanged(); }
        }


        private List<QuoteGroupConfigModel> _panels;
        /// <summary>
        /// 组Id
        /// </summary>
        public List<QuoteGroupConfigModel> Panels
        {
            get { return _panels; }
            set { _panels = value; this.OnPropertyChanged(); }
        }

        public QuotePageModel()
        {
            Panels = new List<QuoteGroupConfigModel>();
        }
    }
}
