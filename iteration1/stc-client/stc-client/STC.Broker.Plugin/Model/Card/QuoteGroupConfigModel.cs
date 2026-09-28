using STC.Common.Model;
using STC.Common.Model.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Broker.Plugin.Model.Card
{
    public class QuoteGroupConfigModel : BaseViewModel
    {
        private string _id;
        /// <summary>
        /// 唯一 Guid
        /// </summary>
        public string Id
        {
            get { return _id; }
            set { _id = value; this.OnPropertyChanged(); }
        }


        private string _ParentId = string.Empty;
        /// <summary>
        /// 服务器保存的债券信息 唯一 ID
        /// </summary>
        public string ParentId
        {
            get { return _ParentId; }
            set { _ParentId = value; this.OnPropertyChanged(); }
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


        private int _page;
        /// <summary>
        /// 页码
        /// </summary>
        public int Page
        {
            get { return _page; }
            set { _page = value; this.OnPropertyChanged(); }
        }

        private string _securityID;
        /// <summary>
        /// 债券编码
        /// </summary>
        public string SecurityID
        {
            get { return _securityID; }
            set { _securityID = value; this.OnPropertyChanged(); }
        }


        private string _symbolName;
        /// <summary>
        /// 名称
        /// </summary>
        public string SymbolName
        {
            get { return _symbolName; }
            set { _symbolName = value; this.OnPropertyChanged(); }
        }

        private string _remaingingtime = string.Empty;
        /// <summary>
        /// 剩余期限
        /// </summary>
        public string RemaingingTime
        {
            get { return _remaingingtime; }
            set { _remaingingtime = value; this.OnPropertyChanged(); }
        }

        private int _remaingingtimeconvert;
        public int RemaingingTimeConvert
        {
            get { return _remaingingtimeconvert; }
            set { _remaingingtimeconvert = value; this.OnPropertyChanged(); }
        }

        private string _bondcategory;
        /// <summary>
        /// 自定义债券归类
        /// </summary>
        public string BondCategory
        {
            get { return _bondcategory; }
            set { _bondcategory = value; this.OnPropertyChanged(); }
        }

        private string _bondType;
        public string BondType 
        { 
            get { return _bondType; } 
            set 
            {
                _bondType = value; 
                this.OnPropertyChanged();
            }
        }

        private string _marketIndicator = "4";
        /// <summary>
        /// 4 债券 5期货
        /// </summary>
        public string MarketIndicator
        {
            get { return _marketIndicator; }
            set { _marketIndicator = value; this.OnPropertyChanged(); }
        }

        /// <summary>
        /// 清算速度 索引 默认选择 1,当多选时  0,1,2,3 全,+1,+0,远
        /// </summary>
        public string SettleIndex { get; set; }

        /// <summary>
        /// 排序
        /// </summary>
        public int CustomSort { get; set; }

        public QuoteGroupConfigModel()
        {
            SecurityID = "";
            SymbolName = "";
            _remaingingtime = "";
            RemaingingTimeConvert = 0;
            BondType = "";
            BondCategory = "";
            SettleIndex = "1";
            GroupId = "";
            CustomSort = 999;
            Id = Guid.NewGuid().ToString();
        }
    }
}
