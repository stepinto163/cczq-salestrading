using STC.Common.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.Hedge
{
    public class KrdPositionDataModel : ViewModelBase
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

        private string _sortkey = "0";
        /// <summary>
        /// sortkey 3实况，2实况+模拟，1差值, 0数据区 - 模拟数据
        /// </summary>
        public string sortkey
        {
            get
            {
                return _sortkey;
            }
            set
            {
                if (_sortkey != value)
                {
                    _sortkey = value;
                    this.OnPropertyChanged(nameof(sortkey));
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

        private string _securityid = string.Empty;
        /// <summary>
        ///  债券编号
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
        /// 简称
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


        private string _combineName = string.Empty;
        /// <summary>
        ///  组合名称
        /// </summary>
        public string combineName
        {
            get
            {
                return _combineName;
            }
            set
            {
                if (_combineName != value)
                {
                    _combineName = value;
                    this.OnPropertyChanged(nameof(combineName));
                }
            }
        }

        private decimal _quantity = 0;
        /// <summary>
        /// 量
        /// </summary>
        public decimal quantity
        {
            get
            {
                return _quantity;
            }
            set
            {
                if (_quantity != value)
                {
                    _quantity = value;
                    this.OnPropertyChanged(nameof(quantity));
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


        private string _DV01 = string.Empty;
        /// <summary>
        /// DV01  实况,实况+模拟,差值
        /// </summary>
        public string DV01
        {
            get
            {
                return _DV01;
            }
            set
            {
                if (_DV01 != value)
                {
                    _DV01 = value;
                    this.OnPropertyChanged(nameof(DV01));
                }
            }
        }

        private decimal _totfyDv01 = 0;
        /// <summary>
        /// 总计
        /// </summary>
        public decimal TotfyDV01
        {
            get
            {
                return _totfyDv01;
            }
            set
            {
                if (_totfyDv01 != value)
                {
                    _totfyDv01 = value;
                    this.OnPropertyChanged(nameof(TotfyDV01));
                }
            }
        }


        private decimal _F6M = 0;
        /// <summary>
        /// 关键期限0.5年
        /// </summary>
        public decimal F6M
        {
            get
            {
                return _F6M;
            }
            set
            {
                if (_F6M != value)
                {
                    _F6M = value;
                    this.OnPropertyChanged(nameof(F6M));
                }
            }
        }

        private decimal _F1Y = 0;
        /// <summary>
        /// 关键期限1年
        /// </summary>
        public decimal F1Y
        {
            get
            {
                return _F1Y;
            }
            set
            {
                if (_F1Y != value)
                {
                    _F1Y = value;
                    this.OnPropertyChanged(nameof(F1Y));
                }
            }
        }

        private decimal _F2Y = 0;
        /// <summary>
        /// 关键期限2年
        /// </summary>
        public decimal F2Y
        {
            get
            {
                return _F2Y;
            }
            set
            {
                if (_F2Y != value)
                {
                    _F2Y = value;
                    this.OnPropertyChanged(nameof(F2Y));
                }
            }
        }

        private decimal _F3Y = 0;
        /// <summary>
        /// 关键期限3年
        /// </summary>
        public decimal F3Y
        {
            get
            {
                return _F3Y;
            }
            set
            {
                if (_F3Y != value)
                {
                    _F3Y = value;
                    this.OnPropertyChanged(nameof(F3Y));
                }
            }
        }

        private decimal _F5Y = 0;
        /// <summary>
        /// 关键期限5年
        /// </summary>
        public decimal F5Y
        {
            get
            {
                return _F5Y;
            }
            set
            {
                if (_F5Y != value)
                {
                    _F5Y = value;
                    this.OnPropertyChanged(nameof(F5Y));
                }
            }
        }

        private decimal _F7Y = 0;
        /// <summary>
        /// 关键期限7年
        /// </summary>
        public decimal F7Y
        {
            get
            {
                return _F7Y;
            }
            set
            {
                if (_F7Y != value)
                {
                    _F7Y = value;
                    this.OnPropertyChanged(nameof(F7Y));
                }
            }
        }


        private decimal _F10Y = 0;
        /// <summary>
        /// 关键期限10年
        /// </summary>
        public decimal F10Y
        {
            get
            {
                return _F10Y;
            }
            set
            {
                if (_F10Y != value)
                {
                    _F10Y = value;
                    this.OnPropertyChanged(nameof(F10Y));
                }
            }
        }

        private decimal _F15Y = 0;
        /// <summary>
        /// 关键期限15年
        /// </summary>
        public decimal F15Y
        {
            get
            {
                return _F15Y;
            }
            set
            {
                if (_F15Y != value)
                {
                    _F15Y = value;
                    this.OnPropertyChanged(nameof(F15Y));
                }
            }
        }


        private decimal _F20Y = 0;
        /// <summary>
        /// 关键期限20年
        /// </summary>
        public decimal F20Y
        {
            get
            {
                return _F20Y;
            }
            set
            {
                if (_F20Y != value)
                {
                    _F20Y = value;
                    this.OnPropertyChanged(nameof(F20Y));
                }
            }
        }


        private decimal _F30Y = 0;
        /// <summary>
        /// 关键期限30年
        /// </summary>
        public decimal F30Y
        {
            get
            {
                return _F30Y;
            }
            set
            {
                if (_F30Y != value)
                {
                    _F30Y = value;
                    this.OnPropertyChanged(nameof(F30Y));
                }
            }
        }


        private decimal _F40Y = 0;
        /// <summary>
        /// 关键期限40年
        /// </summary>
        public decimal F40Y
        {
            get
            {
                return _F40Y;
            }
            set
            {
                if (_F40Y != value)
                {
                    _F40Y = value;
                    this.OnPropertyChanged(nameof(F40Y));
                }
            }
        }


        private decimal _F50Y = 0;
        /// <summary>
        /// 关键期限50年
        /// </summary>
        public decimal F50Y
        {
            get
            {
                return _F50Y;
            }
            set
            {
                if (_F50Y != value)
                {
                    _F50Y = value;
                    this.OnPropertyChanged(nameof(F50Y));
                }
            }
        }


        private string _updatetime = string.Empty;
        /// <summary>
        /// 计算时间
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

        /// <summary>
        /// 删除标记  "1" 删除
        /// </summary>

        private string _delflag = string.Empty;
        /// <summary>
        /// 清算速度
        /// </summary>
        public string delflag
        {
            get
            {
                return _delflag;
            }
            set
            {
                if (_delflag != value)
                {
                    _delflag = value;
                    this.OnPropertyChanged();
                }
            }
        }
    }
}

