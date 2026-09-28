using STC.Common.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.Hedge
{
    /// <summary>
    /// 持仓模拟交易存储表
    /// </summary>
    public class SimPositionExecordersDataModel : ViewModelBase
    {

        private string _key = string.Empty;
        /// <summary>
        /// key
        /// </summary>
        public string key
        {
            get
            {
                return _key;
            }
            set
            {
                if (_key != value)
                {
                    _key = value;
                    this.OnPropertyChanged(nameof(key));
                }
            }
        }

        private string _ischecked = "0";
        /// <summary>
        /// _ischecked
        /// </summary>
        public string ischecked
        {
            get
            {
                return _ischecked;
            }
            set
            {
                if (_ischecked != value)
                {
                    _ischecked = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private string _id = string.Empty;
        /// <summary>
        /// id
        /// </summary>
        public string id
        {
            get
            {
                return _id;
            }
            set
            {
                if (_id != value)
                {
                    _id = value;
                    this.OnPropertyChanged(nameof(id));
                }
            }
        }

        private string _bookid = string.Empty;
        /// <summary>
        /// 组合ID
        /// </summary>
        public string bookId
        {
            get
            {
                return _bookid;
            }
            set
            {
                if (_bookid != value)
                {
                    _bookid = value;
                    this.OnPropertyChanged(nameof(bookId));
                }
            }
        }

        private string _bookname = string.Empty;

        /// <summary>
        /// 组合名称
        /// </summary>
        public string bookName
        {
            get
            {
                return _bookname;
            }
            set
            {
                if (_bookname != value)
                {
                    _bookname = value;
                    this.OnPropertyChanged(nameof(bookName));
                }
            }
        }


        private string _securityid = string.Empty;
        /// <summary>
        ///  代码,产品唯一编号
        /// </summary>
        public string securityid
        {
            get
            {
                return _securityid;
            }
            set
            {
                if (_securityid != value)
                {
                    _securityid = value;
                    this.OnPropertyChanged(nameof(securityid));
                }
            }
        }

        private string _symbol = string.Empty;
        /// <summary>
        /// 简称,产品名称
        /// </summary>
        public string symbol
        {
            get
            {
                return _symbol;
            }
            set
            {
                if (_symbol != value)
                {
                    _symbol = value;
                    this.OnPropertyChanged(nameof(symbol));
                }
            }
        }


        /// <summary>
        /// 剩余时间
        /// 前端计算得出，用于债券剩余期限的展示使用。
        /// 后台订单中使用的剩余期限由后台自己维护。
        /// </summary>
        public string _remaingingtime;
        public string RemaingingTime
        {
            get
            {
                return _remaingingtime;
            }
            set
            {
                if (_remaingingtime != value)
                {
                    _remaingingtime = value;
                    this.OnPropertyChanged(nameof(RemaingingTime));
                }
            }
        }

        private string _side = string.Empty;
        /// <summary>
        /// 方向 >0多 <0空
        /// </summary>
        public string side
        {
            get
            {
                return _side;
            }
            set
            {
                if (_side != value)
                {
                    _side = value;
                    this.OnPropertyChanged(nameof(side));
                }
            }
        }


        private decimal _qty = 0;
        /// <summary>
        /// 面额（万）
        /// </summary>
        public decimal qty
        {
            get
            {
                return _qty;
            }
            set
            {
                if (_qty != value)
                {
                    _qty = value;
                    this.OnPropertyChanged(nameof(qty));
                }
            }
        }



        private string _closeby = string.Empty;
        /// <summary>
        /// 最后更新人
        /// </summary>
        public string closeBy
        {
            get
            {
                return _closeby;
            }
            set
            {
                if (_closeby != value)
                {
                    _closeby = value;
                    this.OnPropertyChanged(nameof(closeBy));
                }
            }
        }

        private string _fullname = string.Empty;
        /// <summary>
        /// 更新人姓名
        /// </summary>
        public string FullName
        {
            get
            {
                return _fullname;
            }
            set
            {
                if (_fullname != value)
                {
                    _fullname = value;
                    this.OnPropertyChanged(nameof(FullName));
                }
            }
        }
        


        private string _updateTime = string.Empty;
        /// <summary>
        /// 最后更新时间
        /// </summary>
        public string updateTime
        {
            get
            {
                return _updateTime;
            }
            set
            {
                if (_updateTime != value)
                {
                    _updateTime = value;
                    this.OnPropertyChanged(nameof(updateTime));
                }
            }
        }

        private bool _editbtnvisible = false;
        /// <summary>
        /// EditBtnVisible
        /// </summary>
        public bool EditBtnVisible
        {
            get
            {
                return _editbtnvisible;
            }
            set
            {
                if (_editbtnvisible != value)
                {
                    _editbtnvisible = value;
                    this.OnPropertyChanged(nameof(EditBtnVisible));
                }
            }
        }

        private bool _savebtnvisible = true;
        /// <summary>
        /// SaveBtnVisible
        /// </summary>
        public bool SaveBtnVisible
        {
            get
            {
                return _savebtnvisible;
            }
            set
            {
                if (_savebtnvisible != value)
                {
                    _savebtnvisible = value;
                    this.OnPropertyChanged(nameof(SaveBtnVisible));
                }
            }
        }

        /// <summary>
        /// 克隆
        /// </summary>
        public SimPositionExecordersDataModel Clone()
        {
            SimPositionExecordersDataModel newItem = new SimPositionExecordersDataModel();
            newItem.key = this.key;
            newItem.ischecked = this.ischecked;//>>>
            newItem.id = this.id;//>>>
            newItem.bookId = this.bookId;
            newItem.bookName = this.bookName;
            newItem.securityid = this.securityid;
            newItem.symbol = this.symbol;
            newItem.RemaingingTime = this.RemaingingTime;
            newItem.side = this.side;
            newItem.qty = this.qty;
            newItem.closeBy = this.closeBy;//>>>
            newItem.FullName = this.FullName;//>>>
            newItem.updateTime = this.updateTime;//>>>
            newItem.EditBtnVisible = this.EditBtnVisible;//>>>
            newItem.SaveBtnVisible = this.SaveBtnVisible;//>>>
            return newItem;
        }
    }
}
