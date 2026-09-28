using STC.Common.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Repo.Plugin.Model
{
    public class TotalBondItem : ViewModelBase
    {
        private string _securityID;

        public string SecurityID
        {
            get { return _securityID; }
            set { 
                _securityID = value;
                this.OnPropertyChanged();
            }
        }

        private string _symbol;

        public string Symbol
        {
            get { return _symbol; }
            set { 
                _symbol = value;
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



        private decimal _ct0;

        public decimal CT0
        {
            get { return _ct0; }
            set { _ct0 = value;
                this.OnPropertyChanged();
            }
        }

        private decimal _ct1;

        public decimal CT1
        {
            get { return _ct1; }
            set { _ct1 = value;
                this.OnPropertyChanged();
            }
        }

        private decimal _m0;

        public decimal M0
        {
            get { return _m0; }
            set { _m0 = value;
                this.OnPropertyChanged();
            }
        }

        private decimal _st0;

        public decimal ST0
        {
            get { return _st0; }
            set { _st0 = value;
                this.OnPropertyChanged();
            }
        }

        private decimal _st1;

        public decimal ST1
        {
            get { return _st1; }
            set { _st1 = value;
                this.OnPropertyChanged();
            }
        }

        private string _institutionCode;

        public string InstitutionCode
        {
            get { return _institutionCode; }
            set { _institutionCode = value;
                this.OnPropertyChanged();
            }
        }

        private string _institutionName;

        public string InstitutionName
        {
            get { return _institutionName; }
            set
            {
                _institutionName = value;
                this.OnPropertyChanged();
            }
        }

        private decimal? _lastQty;

        public decimal? LastQty
        {
            get { return _lastQty; }
            set
            {
                _lastQty = value;
                this.OnPropertyChanged();
            }
        }

        private string _qtpAccout;

        public string QtpAccout
        {
            get { return _qtpAccout; }
            set { _qtpAccout = value;
                this.OnPropertyChanged();
            }
        }

        private string _dueDate;

        public string DueDate
        {
            get { return _dueDate; }
            set { 
                _dueDate = value;
                this.OnPropertyChanged(); }
        }

    }
}
