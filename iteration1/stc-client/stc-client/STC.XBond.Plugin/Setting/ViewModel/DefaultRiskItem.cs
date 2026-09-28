using STC.XBond.Plugin.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.XBond.Plugin.Setting.ViewModel
{
    public class DefaultRiskItem : ViewModelBase
    {
        private decimal? cQtyMin;
        private decimal? cQtyMax;
        private decimal cBid;
        private decimal cOfr;
        private decimal cDropAwayFromBid;
        private decimal cDropAwayFromOfr;
        private decimal cMarketBid;
        private decimal cMarketOfr;
        private bool cBidDefautlCheck;
        private bool cOfrDefautlCheck;

        private decimal? iQtyMin;
        private decimal? iQtyMax;
        private decimal iBid;
        private decimal iOfr;
        private decimal iDropAwayFromBid;
        private decimal iDropAwayFromOfr;
        private decimal iMarketBid;
        private decimal iMarketOfr;
        private bool iBidDefautlCheck;
        private bool iOfrDefautlCheck;

        public decimal? CQtyMin
        {
            get
            {
                return cQtyMin;
            }

            set
            {   cQtyMin = value;
                this.OnPropertyChanged();
            }
        }

        public decimal? CQtyMax
        {
            get
            {
                return cQtyMax;
            }

            set
            {
                cQtyMax = value;
                this.OnPropertyChanged();
            }
        }

        public decimal CBid
        {
            get
            {
                return cBid;
            }

            set
            {
                cBid = value;
                this.OnPropertyChanged();
            }
        }

        public decimal COfr
        {
            get
            {
                return cOfr;
            }

            set
            {
                cOfr = value;
                this.OnPropertyChanged();
            }
        }

        public decimal CDropAwayFromBid
        {
            get
            {
                return cDropAwayFromBid;
            }

            set
            {
                cDropAwayFromBid = value;
                this.OnPropertyChanged();
            }
        }

        public decimal CDropAwayFromOfr
        {
            get
            {
                return cDropAwayFromOfr;
            }

            set
            {
                cDropAwayFromOfr = value;
                this.OnPropertyChanged();
            }
        }

        public decimal CMarketBid
        {
            get
            {
                return cMarketBid;
            }

            set
            {
                cMarketBid = value;
                this.OnPropertyChanged();
            }
        }

        public decimal CMarketOfr
        {
            get
            {
                return cMarketOfr;
            }

            set
            {
                cMarketOfr = value;
                this.OnPropertyChanged();
            }
        }

        public decimal? IQtyMin
        {
            get
            {
                return iQtyMin;
            }

            set
            {
                iQtyMin = value;
                this.OnPropertyChanged();
            }
        }

        public decimal? IQtyMax
        {
            get
            {
                return iQtyMax;
            }

            set
            {
                iQtyMax = value;
                this.OnPropertyChanged();
            }
        }

        public decimal IBid
        {
            get
            {
                return iBid;
            }

            set
            {
                iBid = value;
                this.OnPropertyChanged();
            }
        }

        public decimal IOfr
        {
            get
            {
                return iOfr;
            }

            set
            {
                iOfr = value;
                this.OnPropertyChanged();
            }
        }

        public decimal IDropAwayFromBid
        {
            get
            {
                return iDropAwayFromBid;
            }

            set
            {
                iDropAwayFromBid = value;
                this.OnPropertyChanged();
            }
        }

        public decimal IDropAwayFromOfr
        {
            get
            {
                return iDropAwayFromOfr;
            }

            set
            {
                iDropAwayFromOfr = value;
                this.OnPropertyChanged();
            }
        }

        public decimal IMarketBid
        {
            get
            {
                return iMarketBid;
            }

            set
            {
                iMarketBid = value;
                this.OnPropertyChanged();
            }
        }

        public decimal IMarketOfr
        {
            get
            {
                return iMarketOfr;
            }

            set
            {
                iMarketOfr = value;
                this.OnPropertyChanged();
            }
        }

        public bool CBidDefautlCheck
        {
            get
            {
                return cBidDefautlCheck;
            }

            set
            {
                cBidDefautlCheck = value;
                this.OnPropertyChanged();
            }
        }

        public bool COfrDefautlCheck
        {
            get
            {
                return cOfrDefautlCheck;
            }

            set
            {
                cOfrDefautlCheck = value;
                this.OnPropertyChanged();
            }
        }

        public bool IBidDefautlCheck
        {
            get
            {
                return iBidDefautlCheck;
            }

            set
            {
                iBidDefautlCheck = value;
                this.OnPropertyChanged();
            }
        }

        public bool IOfrDefautlCheck
        {
            get
            {
                return iOfrDefautlCheck;
            }

            set
            {
                iOfrDefautlCheck = value;
                this.OnPropertyChanged();
            }
        }
    }
}
