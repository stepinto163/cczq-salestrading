using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Mm.Plugin.ViewModel
{
    public class ANSOutlineProp : ViewModelBase
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

        // 利率债有效报价
        private int _bncSecurityCount;
        public int bncSecurityCount
        {
            get { return _bncSecurityCount; }
            set
            {
                _bncSecurityCount = value;
                OnPropertyChanged();
            }
        }

        //利率债品种分布
        private int _bncSecurityTypeCount;
        public int bncSecurityTypeCount
        {
            get { return _bncSecurityTypeCount; }
            set
            {
                _bncSecurityTypeCount = value;
                OnPropertyChanged();
            }
        }

        // 利率债剩余期限
        private int _bncTimetomaturityCount;
        public int bncTimetomaturityCount
        {
            get { return _bncTimetomaturityCount; }
            set
            {
                _bncTimetomaturityCount = value;
                OnPropertyChanged();
            }
        }

        // 利率债国债数量
        private int _bncNDCount;
        public int bncNDCount
        {
            get { return _bncNDCount; }
            set
            {
                _bncNDCount = value;
                OnPropertyChanged();
            }
        }

        //利率债新发国债期限
        private int _bncNewNDCount;
        public int bncNewNDCount
        {
            get { return _bncNewNDCount; }
            set
            {
                _bncNewNDCount = value;
                OnPropertyChanged();
            }
        }

        //信用债有效报价
        private int _bcoSecurityCount;
        public int bcoSecurityCount
        {
            get { return _bcoSecurityCount; }
            set
            {
                _bcoSecurityCount = value;
                OnPropertyChanged();
            }
        }

        //信用债品种分布
        private int _bcoSecurityTypeCount;
        public int bcoSecurityTypeCount
        {
            get { return _bcoSecurityTypeCount; }
            set
            {
                _bcoSecurityTypeCount = value;
                OnPropertyChanged();
            }
        }
    }

    //public class ANSOutlineProp : ViewModelBase
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

    //    private List<ANSOutlineListProp> _Datas = new List<ANSOutlineListProp>();
    //    public List<ANSOutlineListProp> Datas
    //    {
    //        get { return _Datas; }
    //        set
    //        {
    //            _Datas = value;
    //            OnPropertyChanged();
    //        }
    //    }

    //}
}
