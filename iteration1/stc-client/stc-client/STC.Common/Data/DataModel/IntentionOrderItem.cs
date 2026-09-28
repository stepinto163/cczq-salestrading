using STC.Common.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Data.DataModel
{
    public class IntentionOrderItem : ViewModelBase
    {
        private string _IntentionId;

        public string IntentionId
        {
            get { return _IntentionId; }
            set
            {
                if (_IntentionId != value)
                {
                    _IntentionId = value;
                    this.OnPropertyChanged();
                }

            }
        }

        private string _OrderId;

        public string OrderId
        {
            get { return _OrderId; }
            set
            {
                if (_OrderId != value)
                {
                    _OrderId = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private string _SecurityId;

        public string SecurityId
        {
            get { return _SecurityId; }
            set
            {
                if (_SecurityId != value)
                {
                    _SecurityId = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private string _Symbol;

        public string Symbol
        {
            get { return _Symbol; }
            set
            {
                if (_Symbol != value)
                {
                    _Symbol = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private decimal _Price;

        public decimal Price
        {
            get { return _Price; }
            set
            {
                if (_Price != value)
                {
                    _Price = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private int _Qty;

        public int Qty
        {
            get { return _Qty; }
            set
            {
                if (_Qty != value)
                {
                    _Qty = value;
                    this.OnPropertyChanged();
                }
            }
        }


        private string _side;

        public string Side
        {
            get { return _side; }
            set
            {
                if (_side != value)
                {
                    _side = value;
                    this.OnPropertyChanged();
                }

            }
        }

        private string _VenueTypeGw;

        public string VenueTypeGw
        {
            get { return _VenueTypeGw; }
            set
            {
                if (_VenueTypeGw != value)
                {
                    _VenueTypeGw = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private string _Venues;

        public string Venues
        {
            get { return _Venues; }
            set
            {
                if (_Venues != value)
                {
                    _Venues = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private string _VenuesSelectF;

        public string VenuesSelectF
        {
            get { return _VenuesSelectF; }
            set
            {
                if (_VenuesSelectF != value)
                {
                    _VenuesSelectF = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private string _SelfSelectVenues;

        public string SelfSelectVenues
        {
            get { return _SelfSelectVenues; }
            set
            {
                if (_SelfSelectVenues != value)
                {
                    _SelfSelectVenues = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private string _DisplayVenues;

        public string DisplayVenues
        {
            get { return _DisplayVenues; }
            set
            {
                if (_DisplayVenues != value)
                {
                    _DisplayVenues = value;
                    this.OnPropertyChanged();
                }
            }
        }
        private string _FirmFlag;

        public string FirmFlag
        {
            get { return _FirmFlag; }
            set
            {
                if (_FirmFlag != value)
                {
                    _FirmFlag = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private string _UpdateTime;

        public string UpdateTime
        {
            get { return _UpdateTime; }
            set { _UpdateTime = value; }
        }

    }
}
