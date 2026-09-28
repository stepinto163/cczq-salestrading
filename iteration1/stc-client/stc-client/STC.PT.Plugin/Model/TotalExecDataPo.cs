using STC.Common.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.PT.Plugin.Model
{
    public class TotalExecDataPo : BaseViewModel
    {
        private string _SecurityID;
        public string SecurityID 
        {
            get { return _SecurityID; }
            set 
            {  
                _SecurityID = value;
                this.OnPropertyChanged();
            }
        }
        private string _Symbol;
        public string Symbol
        {
            get { return _Symbol; }
            set
            {
                _Symbol = value;
                this.OnPropertyChanged();
            }
        }
        private decimal _BidLastQty;
        public decimal BidLastQty
        {
            get { return _BidLastQty; }
            set
            {
                _BidLastQty = value;
                this.OnPropertyChanged();
            }
        }
        private decimal _OfrLastQty;
        public decimal OfrLastQty
        {
            get { return _OfrLastQty; }
            set
            {
                _OfrLastQty = value;
                this.OnPropertyChanged();
            }
        }
        private decimal _NettingLastQty;
        public decimal NettingLastQty
        {
            get { return _NettingLastQty; }
            set
            {
                _NettingLastQty = value;
                this.OnPropertyChanged();
            }
        }
    }
}
