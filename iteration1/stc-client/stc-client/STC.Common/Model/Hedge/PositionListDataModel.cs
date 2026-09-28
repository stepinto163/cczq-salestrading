using STC.Common.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.Hedge
{
    public class PositionListDataModel : ViewModelBase
    {

        private string _key = string.Empty;
        /// <summary>
        /// key   BookId + "#" + Securityid
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

        private string _bookid = string.Empty;
        /// <summary>
        /// 组合ID
        /// </summary>
        public string bookid
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
                    this.OnPropertyChanged(nameof(bookid));
                }
            }
        }

        private string _bookname = string.Empty;

        /// <summary>
        /// 组合名称
        /// </summary>
        public string bookname
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
                    this.OnPropertyChanged(nameof(bookname));
                }
            }
        }

        private string _settlDate;
        /// <summary>
        /// 起息日 = 交割日
        /// </summary>
        public string settlDate
        {
            get
            {
                return _settlDate;
            }
            set
            {
                if (_settlDate != value)
                {
                    _settlDate = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private string _securityid = string.Empty;
        /// <summary>
        /// 代码
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

        private decimal _netposition = 0;
        /// <summary>
        /// 持仓
        /// </summary>
        public decimal netposition
        {
            get
            {
                return _netposition;
            }
            set
            {
                if (_netposition != value)
                {
                    _netposition = value;
                    this.OnPropertyChanged(nameof(netposition));
                }
            }
        }

        private string _updatetime = string.Empty;
        /// <summary>
        /// 更新时间
        /// </summary>
        public string updatetime
        {
            get
            {
                return _updatetime;
            }
            set
            {
                if (_updatetime != value)
                {
                    _updatetime = value;
                    this.OnPropertyChanged(nameof(updatetime));
                }
            }
        }
    }
}
