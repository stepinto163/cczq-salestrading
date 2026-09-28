using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.XBond.Plugin.ViewModel.Data
{
    public class IntentionQuoteItem : ViewModelBase
    {
        private string _clordid;

        /// <summary>
        /// 客户订单编号
        /// </summary>
        public string ClOrdID
        {
            get
            {
                return _clordid;
            }
            set
            {
                _clordid = value;
                this.OnPropertyChanged();
            }
        }


        private bool _isChecked = true;

        public bool IsChecked
        {
            get { return _isChecked; }
            set
            {
                _isChecked = value;
                this.OnPropertyChanged();
            }
        }

        private bool _isShow = false;

        public bool IsShow
        {
            get { return _isShow; }
            set
            {
                _isShow = value;
                this.OnPropertyChanged();
            }
        }

        private string _intentionId;

        public string IntentionId
        {
            get { return _intentionId; }
            set
            {
                _intentionId = value;
                this.OnPropertyChanged();
            }
        }

        private string _side_Edit;

        public string Side_Edit
        {
            get { return _side_Edit; }
            set
            {
                _side_Edit = value;
                this.OnPropertyChanged();
            }
        }

        private string _side;

        public string Side
        {
            get { return _side; }
            set
            {
                _side = value;
                this.OnPropertyChanged();
            }
        }

        private string _securityId;

        public string SecurityId
        {
            get { return _securityId; }
            set
            {
                _securityId = value;
                this.OnPropertyChanged();
            }
        }

        private string _symbol;

        public string Symbol
        {
            get { return _symbol; }
            set
            {
                _symbol = value;
                this.OnPropertyChanged();
            }
        }

        private decimal? _cleanPrice;

        public decimal? CleanPrice
        {
            get { return _cleanPrice; }
            set
            {
                _cleanPrice = value;
                this.OnPropertyChanged();
            }
        }

        private decimal? _fullPrice;

        public decimal? FullPrice
        {
            get { return _fullPrice; }
            set
            {
                _fullPrice = value;
                this.OnPropertyChanged();
            }
        }

        private decimal? _price_Edit;

        public decimal? Price_Edit
        {
            get { return _price_Edit; }
            set
            {
                _price_Edit = value;
                this.OnPropertyChanged();
            }
        }

        private decimal? _price;

        public decimal? Price
        {
            get { return _price; }
            set
            {
                _price = value;
                this.OnPropertyChanged();
            }
        }

        private decimal? _qty_Edit;

        public decimal? Qty_Edit
        {
            get { return _qty_Edit; }
            set
            {
                _qty_Edit = value;
                this.OnPropertyChanged();
            }
        }

        private decimal? _qty;

        public decimal? Qty
        {
            get { return _qty; }
            set
            {
                _qty = value;
                this.OnPropertyChanged();
            }
        }


        private decimal? _executionqty;

        public decimal? ExecutionQty
        {
            get { return _executionqty; }
            set
            {
                _executionqty = value;
                this.OnPropertyChanged();
            }
        }

        private string _settlType;

        public string SettlType
        {
            get { return _settlType; }
            set
            {
                _settlType = value;
                this.OnPropertyChanged();
            }
        }

        private string _settlType_edit;

        public string SettlType_Edit
        {
            get { return _settlType_edit; }
            set
            {
                _settlType_edit = value;
                this.OnPropertyChanged();
            }
        }

        private DateTime _edit_settltype_date;

        public DateTime edit_settltype_date
        {
            get { return _edit_settltype_date; }
            set
            {
                _edit_settltype_date = value;
                OnPropertyChanged();
            }
        }

        private int _edit_settletype = int.MinValue;

        public int edit_settletype
        {
            get { return _edit_settletype; }
            set
            {
                _edit_settletype = value;
                this.OnPropertyChanged();
            }
        }

        private string _text;

        public string Text
        {
            get { return _text; }
            set
            {
                _text = value;
                this.OnPropertyChanged();
            }
        }

        private string _status;

        public string Status
        {
            get { return _status; }
            set
            {
                _status = value;
                this.OnPropertyChanged();
            }
        }

        private string _type;

        public string Type
        {
            get { return _type; }
            set
            {
                _type = value;
                this.OnPropertyChanged();
            }
        }

        private string _createTime;

        public string CreateTime
        {
            get { return _createTime; }
            set
            {
                _createTime = value;
                this.OnPropertyChanged();
            }
        }

        private string _updateTime;

        public string UpdateTime
        {
            get { return _updateTime; }
            set
            {
                _updateTime = value;
                this.OnPropertyChanged();
            }
        }

        private string _closeBy;

        public string CloseBy
        {
            get { return _closeBy; }
            set
            {
                _closeBy = value;
                this.OnPropertyChanged();
            }
        }

        private string _closeByName;

        public string CloseByName
        {
            get { return _closeByName; }
            set
            {
                _closeByName = value;
                this.OnPropertyChanged();
            }
        }

        private string _orderID;

        public string OrderID
        {
            get
            {
                return _orderID;
            }
            set
            {
                _orderID = value;
                this.OnPropertyChanged();
            }
        }


        private string _isEdit = "2"; // 0:显示 2：不显示

        public string IsEdit
        {
            get
            {
                return _isEdit;
            }
            set
            {
                _isEdit = value;
                this.OnPropertyChanged();
            }
        }

        // 序号
        private int? _no;

        public int? No
        {
            get
            {
                return _no;
            }
            set
            {
                _no = value;
                this.OnPropertyChanged();
            }
        }

        private string _userName;

        public string UserName
        {
            get
            {
                return _userName;
            }
            set
            {
                _userName = value;
                this.OnPropertyChanged();
            }
        }

        private bool _isConfirmed;
        public bool IsConfirmed
        {
            get
            {
                return _isConfirmed;
            }
            set
            {
                _isConfirmed = value;
                this.OnPropertyChanged();
            }
        }

        private string _wholeFlag_Edit;

        public string WholeFlag_Edit
        {
            get
            {
                return _wholeFlag_Edit;
            }
            set
            {
                _wholeFlag_Edit = value;
                this.OnPropertyChanged();
            }
        }

        private string _wholeFlag;

        public string WholeFlag
        {
            get
            {
                return _wholeFlag;
            }
            set
            {
                _wholeFlag = value;
                this.OnPropertyChanged();
            }
        }

        private string _remainingTime;
        public string RemainingTime
        {
            get { return _remainingTime; }
            set { _remainingTime = value; this.OnPropertyChanged(); }
        }

        private decimal _remaingingTime_convert;
        public decimal RemaingingTime_convert
        {
            get { return _remaingingTime_convert; }
            set { _remaingingTime_convert = value; this.OnPropertyChanged(); }
        }

        /// <summary>
        /// 中债
        /// </summary>
        private decimal? _ccdcvaluation;

        public decimal? CCDCValuation
        {
            get
            {
                return _ccdcvaluation;
            }
            set
            {
                if (_ccdcvaluation != value)
                {
                    _ccdcvaluation = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 中债估值偏离
        /// </summary>
        private decimal? _ccdcvaluation_bp;

        public decimal? CCDCValuation_BP
        {
            get
            {
                return _ccdcvaluation_bp;
            }
            set
            {
                if (_ccdcvaluation_bp != value)
                {
                    _ccdcvaluation_bp = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 中债日期
        /// </summary>
        private string _ccdcvaluation_date;

        public string CCDCValuation_Date
        {
            get
            {
                return _ccdcvaluation_date;
            }
            set
            {
                if (_ccdcvaluation_date != value)
                {
                    _ccdcvaluation_date = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private string _showlastworkday;
        /// <summary>
        /// 中债估值日期警告标记 !
        /// </summary>
        public string ShowLastWorkDay
        {
            get { return _showlastworkday; }
            set
            {
                _showlastworkday = value;
                this.OnPropertyChanged();
            }
        }

        //到期时间
        private string _timeToMaturity;

        public string TimeToMaturity
        {
            get { return _timeToMaturity; }
            set
            {
                _timeToMaturity = value;
                this.OnPropertyChanged();
            }
        }

        //到期时间_转换
        private decimal _timeToMaturity_Convert;

        public decimal TimeToMaturity_Convert
        {
            get { return _timeToMaturity_Convert; }
            set
            {
                _timeToMaturity_Convert = value;
                this.OnPropertyChanged();
            }
        }

        //到期日距离下个工作日天数
        private string _timeToMaturityToWorkDay;

        public string TimeToMaturityToWorkDay
        {
            get { return _timeToMaturityToWorkDay; }
            set
            {
                _timeToMaturityToWorkDay = value;
                this.OnPropertyChanged();
            }
        }

        private string _orderCount = string.Empty;

        public string OrderCount
        {
            get
            {
                return _orderCount;
            }

            set
            {
                _orderCount = value;
                this.OnPropertyChanged();

            }
        }
    }
}
