using STC.Mmshe.Plugin.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace STC.Mmshe.Plugin.ViewModel
{
    public class MyQuotePopupViewModel : ViewModelBase
    {
        private MyQuotePopupPo _Data;
        public MyQuotePopupPo Data
        {
            get { return this._Data; }

            set
            {
                this._Data = value;
                this.OnPropertyChanged();
            }
        }

        public Brush _ForegroundBrush = new SolidColorBrush((Color) ColorConverter.ConvertFromString("#FFFFFF"));
        public Brush ForegroundBrush
        {
            get { return this._ForegroundBrush; }

            set
            {
                this._ForegroundBrush = value;
                this.OnPropertyChanged();
            }
        }

        public string _Sign;
        public string Sign
        {
            get { return this._Sign; }

            set
            {
                this._Sign = value;
                this.OnPropertyChanged();
            }
        }
    }
    public class MyQuotePopupPo : ViewModelBase
    {
        public string _QuoteModel;
        public string QuoteModel
        {
            get { return _QuoteModel; }
            set
            {
                _QuoteModel = value;
                OnPropertyChanged();
            }
        }
        public string _BasePrice;
        public string BasePrice
        {
            get { return _BasePrice; }
            set
            {
                _BasePrice = value;
                OnPropertyChanged();
            }
        }
        public string _DefenReferMarketType;
        public string DefenReferMarketType
        {
            get { return _DefenReferMarketType; }
            set
            {
                _DefenReferMarketType = value;
                OnPropertyChanged();
            }
        }
        public string _DefenReferLevel;
        public string DefenReferLevel
        {
            get { return _DefenReferLevel; }
            set
            {
                _DefenReferLevel = value;
                OnPropertyChanged();
            }
        }
        public string _CalDetail;
        public string CalDetail
        {
            get { return _CalDetail; }
            set
            {
                _CalDetail = value;
                OnPropertyChanged();
            }
        }
        public string _AlgoDefenValue;
        public string AlgoDefenValue
        {
            get { return _AlgoDefenValue; }
            set
            {
                _AlgoDefenValue = value;
                OnPropertyChanged();
            }
        }
        public string _globalWall;
        public string globalWall
        {
            get { return _globalWall; }
            set
            {
                _globalWall = value;
                OnPropertyChanged();
            }
        }
        public string _AlgoTrigValue;
        public string AlgoTrigValue
        {
            get { return _AlgoTrigValue; }
            set
            {
                _AlgoTrigValue = value;
                OnPropertyChanged();
            }
        }

        public string _defenCDCDetail;
        public string defenCDCDetail
        {
            get { return _defenCDCDetail; }
            set
            {
                _defenCDCDetail = value;
                OnPropertyChanged();
            }
        }

        public string _DefenCDCValue;
        public string DefenCDCValue
        {
            get { return _DefenCDCValue; }
            set
            {
                _DefenCDCValue = value;
                OnPropertyChanged();
            }
        }

        public string _VisibileDefendValue;
        public string VisibileDefendValue
        {

            get { return _VisibileDefendValue; }

            set {
                _VisibileDefendValue = value;
                OnPropertyChanged();
            }
        }

        public string _VisibileAlgorithmValue;
        public string VisibileAlgorithmValue
        {

            get { return _VisibileAlgorithmValue; }

            set
            {
                _VisibileAlgorithmValue = value;
                OnPropertyChanged();
            }
        }

        public string _AlgoBaseSecurityid;

        public string AlgoBaseSecurityid
        {
            get { return _AlgoBaseSecurityid; }
            set
            {
                _AlgoBaseSecurityid = value;
                OnPropertyChanged();
            }
        }

        public string _AlgoMidType;//算法参考价类型
        public string AlgoMidType
        {
            get { return _AlgoMidType; }
            set
            {
                _AlgoMidType = value;
                OnPropertyChanged();
            }
        }


        public string _AlgoMidValue;//算法参考价值
        public string AlgoMidValue
        {
            get { return _AlgoMidValue; }
            set
            {
                _AlgoMidValue = value;
                OnPropertyChanged();
            }
        }

        public string _AlgoMidOffsetValue;//算法偏离
        public string AlgoMidOffsetValue
        {
            get { return _AlgoMidOffsetValue; }
            set
            {
                _AlgoMidOffsetValue = value;
                OnPropertyChanged();
            }
        }

        public string _AlgoValue;//算法点差
        public string AlgoValue
        {
            get { return _AlgoValue; }
            set
            {
                _AlgoValue = value;
                OnPropertyChanged();
            }
        }


    }
}
