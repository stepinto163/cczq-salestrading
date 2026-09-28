using STC.Common.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace STC.XBond.Plugin.ViewModel.Order
{
    public class BatchPointPriceDomain : BaseViewModel
    {
        private Visibility _warnImageVisibility = Visibility.Collapsed;
        public Visibility WarnImageVisibility
        {
            get { return _warnImageVisibility; }
            set
            {
                _warnImageVisibility = value;
                this.OnPropertyChanged();
            }
        }

        private Visibility _oneSettleVisibility = Visibility.Collapsed;
        public Visibility OneSettleVisibility
        {
            get { return _oneSettleVisibility; }
            set
            {
                _oneSettleVisibility = value;
                this.OnPropertyChanged();
            }
        }

        private string _warnImageTooltip = string.Empty;
        public string WarnImageTooltip
        {
            get { return _warnImageTooltip; }
            set
            {
                _warnImageTooltip = value;
                this.OnPropertyChanged();
            }
        }

        private Brush _borderBrush;
        public Brush BorderBrush
        {
            get { return _borderBrush; }
            set
            {
                _borderBrush = value;
                this.OnPropertyChanged();
            }
        }

        private Brush _bgBrush;
        public Brush BgBrush
        {
            get { return _bgBrush; }
            set
            {
                _bgBrush = value;
                this.OnPropertyChanged();
            }
        }

        private Brush _boxBgBrush;
        public Brush BoxBgBrush
        {
            get { return _boxBgBrush; }
            set
            {
                _boxBgBrush = value;
                this.OnPropertyChanged();
            }
        }

        private Brush _boxBorderBrush;
        public Brush BoxBorderBrush
        {
            get { return _boxBorderBrush; }
            set
            {
                _boxBorderBrush = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// 交易方向
        /// </summary>
        private string _side = string.Empty;
        public string Side
        {
            get { return _side; }
            set
            {
                _side = value;
                this.OnPropertyChanged();
            }
        }

        private string _sideTxt = string.Empty;
        public string SideTxt
        {
            get { return _sideTxt; }
            set
            {
                _sideTxt = value;
                this.OnPropertyChanged();
            }
        }

        private string _securityId = string.Empty;
        public string SecurityId
        {
            get { return _securityId; }
            set
            {
                _securityId = value;
                this.OnPropertyChanged();
            }
        }

        private string _symbolName = string.Empty;
        public string SymbolName
        {
            get { return _symbolName; }
            set
            {
                _symbolName = value;
                this.OnPropertyChanged();
            }
        }

        private Brush _sideBrush;
        public Brush SideBrush
        {
            get { return _sideBrush; }
            set
            {
                _sideBrush = value;
                this.OnPropertyChanged();
            }
        }

        private string _bestBid;
        public string BestBid
        {
            get { return _bestBid; }
            set
            {
                _bestBid = value;
                this.OnPropertyChanged();
            }
        }

        private string _bestOfr;
        public string BestOfr
        {
            get { return _bestOfr; }
            set
            {
                _bestOfr = value;
                this.OnPropertyChanged();
            }
        }

        private string _price;
        public string Price
        {
            get { return _price; }
            set
            {
                _price = value;
                this.OnPropertyChanged();
            }
        }

        private bool _priceEnable = false;
        public bool PriceEnable
        {
            get { return _priceEnable; }
            set
            {
                _priceEnable = value;
                this.OnPropertyChanged();
            }
        }

        private string _qty;
        public string Qty
        {
            get { return _qty; }
            set
            {
                _qty = value;
                this.OnPropertyChanged();
            }
        }

        private string _clickQty;
        public string ClickQty
        {
            get { return _clickQty; }
            set
            {
                _clickQty = value;
                this.OnPropertyChanged();
            }
        }

        private string _matchTotalQty;
        public string MatchTotalQty
        {
            get { return _matchTotalQty; }
            set
            {
                _matchTotalQty = value;
                this.OnPropertyChanged();
            }
        }

        private string _sendPriceTxt;
        public string SendPriceTxt
        {
            get { return _sendPriceTxt; }
            set
            {
                _sendPriceTxt = value;
                this.OnPropertyChanged();
            }
        }

        private string _settleTxt;
        public string SettleTxt
        {
            get { return _settleTxt; }
            set
            {
                _settleTxt = value;
                this.OnPropertyChanged();
            }
        }
        

        private int _settleSelectIndex;
        public int SettleSelectIndex
        {
            get { return _settleSelectIndex; }
            set
            {
                _settleSelectIndex = value;
                this.OnPropertyChanged();
            }
        }

        private List<ListBoxItemData> _settleSource = new List<ListBoxItemData>();
        public List<ListBoxItemData> SettleSource
        {
            get
            {
                return _settleSource;
            }
        }

        private int _listBoxSelectIndex;
        public int ListBoxSelectIndex
        {
            get { return _listBoxSelectIndex; }
            set
            {
                _listBoxSelectIndex = value;
                this.OnPropertyChanged();
            }
        }


        private string _iocOutTime;
        public string IocOutTime
        {
            get { return _iocOutTime; }
            set
            {
                _iocOutTime = value;
                this.OnPropertyChanged();
            }
        }
        
    }
}
