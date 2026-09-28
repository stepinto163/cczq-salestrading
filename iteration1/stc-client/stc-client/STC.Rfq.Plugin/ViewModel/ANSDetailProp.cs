using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Rfq.Plugin.ViewModel
{
    //public class ANSDetailProp : ViewModelBase
    //{
    //    private int _count;
    //    public int count
    //    {
    //        get { return _count; }
    //        set
    //        {
    //            _count = value;
    //            OnPropertyChanged();
    //        }
    //    }

    //    private int _pageNum;
    //    public int pageNum
    //    {
    //        get { return _pageNum; }
    //        set
    //        {
    //            _pageNum = value;
    //            OnPropertyChanged();
    //        }
    //    }

    //    private int _pageSize;
    //    public int pageSize
    //    {
    //        get { return _pageSize; }
    //        set
    //        {
    //            _pageSize = value;
    //            OnPropertyChanged();
    //        }
    //    }

    //    private List<ANSDetailListProp> _list = new List<ANSDetailListProp>();
    //    public List<ANSDetailListProp> list
    //    {
    //        get { return _list; }
    //        set
    //        {
    //            _list = value;
    //            OnPropertyChanged();
    //        }
    //    }
    //}

    public class ANSDetailProp : ViewModelBase
    {
        // 日期 MM-dd
        private string _date;
        public string date
        {
            get { return _date; }
            set
            {
                _date = value;
                OnPropertyChanged();
            }
        }

        // 债券代码
        private string _securityId;
        public string securityId
        {
            get { return _securityId; }
            set
            {
                _securityId = value;
                OnPropertyChanged();
            }
        }

        // 债券简称
        private string _symbol;
        public string symbol
        {
            get { return _symbol; }
            set
            {
                _symbol = value;
                OnPropertyChanged();
            }
        }

        //债券种类
        private string _symbolType;
        public string symbolType
        {
            get { return _symbolType; }
            set
            {
                _symbolType = value;
                OnPropertyChanged();
            }
        }

        // 报价人
        private string _username;
        public string username
        {
            get { return _username; }
            set
            {
                _username = value;
                OnPropertyChanged();
            }
        }

        // 债券类型
        private string _securitySuperType;
        public string securitySuperType
        {
            get { return _securitySuperType; }
            set
            {
                _securitySuperType = value;
                OnPropertyChanged();
            }
        }

        // 剩余期限
        private string _timetomaturity;
        public string timetomaturity
        {
            get { return _timetomaturity; }
            set
            {
                _timetomaturity = value;
                OnPropertyChanged();
            }
        }

        // 剩余期限分组
        private string _timetomaturityGroup;
        public string timetomaturityGroup
        {
            get { return _timetomaturityGroup; }
            set
            {
                _timetomaturityGroup = value;
                OnPropertyChanged();
            }
        }

        //  报价时长
        private long _quoteTimeLength;
        public long quoteTimeLength
        {
            get { return _quoteTimeLength; }
            set
            {
                _quoteTimeLength = value;
                OnPropertyChanged();
            }
        }

        // 成交笔数
        private int _executionCount;
        public int executionCount
        {
            get { return _executionCount; }
            set
            {
                _executionCount = value;
                OnPropertyChanged();
            }
        }

        // 成交金额
        private decimal _executionQty;
        public decimal executionQty
        {
            get { return _executionQty; }
            set
            {
                _executionQty = value;
                OnPropertyChanged();
            }
        }

        // 平均点差
        private decimal _avgBP;
        public decimal avgBP
        {
            get { return _avgBP; }
            set
            {
                _avgBP = value;
                OnPropertyChanged();
            }
        }

        // 报价开始时间
        private string _quoteBeginTime;
        public string quoteBeginTime
        {
            get { return _quoteBeginTime; }
            set
            {
                _quoteBeginTime = value;
                OnPropertyChanged();
            }
        }

        // 报价结束时间
        private string _quoteEndTime;
        public string quoteEndTime
        {
            get { return _quoteEndTime; }
            set
            {
                _quoteEndTime = value;
                OnPropertyChanged();
            }
        }
    }
}
