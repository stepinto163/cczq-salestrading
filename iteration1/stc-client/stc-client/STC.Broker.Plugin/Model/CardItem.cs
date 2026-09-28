using STC.Broker.Plugin.View.Price;
using STC.Broker.Plugin.ViewModel.UI;
using STC.Common.Model;
using STC.Common.Model.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Broker.Plugin.Model
{
    public class CardItem : BaseViewModel
    {
        private string _id;
        /// <summary>
        /// 唯一 guid
        /// </summary>
        public string Id 
        { 
            get 
            { 
                return _id; 
            } 
            set 
            { 
                _id = value; 
                this.OnPropertyChanged(); 
            } 
        }

        private string _groupId;
        /// <summary>
        /// 组Id
        /// </summary>
        public string GroupId
        {
            get 
            { 
                return _groupId; 
            }
            set
            {
                _groupId = value;
                this.OnPropertyChanged();
            }
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

        private decimal _customSort;
        public decimal CustomSort { get { return _customSort; } set { _customSort = value; this.OnPropertyChanged(); } }

        private string _bondType;
        public string BondType { get { return _bondType; } set { _bondType = value; this.OnPropertyChanged(); } }

        private string _bondCategory;
        public string BondCategory { get { return _bondCategory; } set { _bondCategory = value; this.OnPropertyChanged(); } }


        private string _remaingingTime;
        public string RemaingingTime { get { return _remaingingTime; } set { _remaingingTime = value; this.OnPropertyChanged(); } }
                

        private int _remaingingTimeConvert;
        public int RemaingingTimeConvert { get { return _remaingingTimeConvert; } set { _remaingingTimeConvert = value; this.OnPropertyChanged(); } }

        private int _page;
        public int Page { get { return _page; } set { _page = value; this.OnPropertyChanged(); } }


        private string _settleindex = "1";
        /// <summary>
        /// 清算速度 索引 默认选择 1,当多选时  0,1,2,3 全,+1,+0,远
        /// </summary>
        public string SettleIndex
        {
            get
            {
                return _settleindex;
            }
            set
            {
                _settleindex = value;
                this.OnPropertyChanged();
            }
        }

        private CardPriceView cardPriceView;

        public CardPriceView CurCardPriceView
        {
            get
            {
                return cardPriceView;
            }
            set
            {
                cardPriceView = value;
                this.OnPropertyChanged();
            }
        }
    }
}
