using STC.Common.Model;
using BDB.Trade.Plugin.ViewModel.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace BDB.Trade.Plugin.ViewModel.Trade
{
    public class InnerPriceViewModel : BaseViewModel
    {
        private FrameworkElement _ownUI = null;

        private InnerPriceItem _showPriceItem = new InnerPriceItem();

        public InnerPriceItem ShowPriceItem { get { return _showPriceItem; } }


        public void Init(FrameworkElement ownUI)
        {
            this._ownUI = ownUI;
        }

        private object FindUIByName(string name)
        {
            object result = _ownUI.FindName(name);

            return result;
        }



        #region 单Broker显示属性

        private Visibility _borderVisibility = Visibility.Collapsed;
        public Visibility BorderVisibility
        {
            get { return _borderVisibility; }
            set { _borderVisibility = value; this.OnPropertyChanged(); }
        }

        private string _bidPrice;
        public string BidPrice { get { return _bidPrice; } set { _bidPrice = value; this.OnPropertyChanged(); } }


        private string _bidTxt;
        public string BidTxt { get { return _bidTxt; } set { _bidTxt = value; this.OnPropertyChanged(); } }

        private string _ofrPrice;
        public string OfrPrice { get { return _ofrPrice; } set { _ofrPrice = value; this.OnPropertyChanged(); } }


        private string _ofrTxt;
        public string OfrTxt { get { return _ofrTxt; } set { _ofrTxt = value; this.OnPropertyChanged(); } }

        #endregion

        private GridLength _frameOneColumnWidth;
        public GridLength FrameOneColumnWidth { get { return _frameOneColumnWidth; } set { _frameOneColumnWidth = value; this.OnPropertyChanged(); } }

        private GridLength _innerColumnWidth;
        public GridLength InnerColumnWidth { get { return _innerColumnWidth; } set { _innerColumnWidth = value; this.OnPropertyChanged(); } }

        private Visibility _headVisibility = Visibility.Visible;
        public Visibility headVisibility
        {
            get { return _headVisibility; }
            set { _headVisibility = value; this.OnPropertyChanged(); }
        }


        #region 行显示

        private string _bidOne;
        public string BidOne { get { return _bidOne; } set { _bidOne = value; this.OnPropertyChanged(); } }

        private string _bidOneAmount;
        public string BidOneAmount { get { return _bidOneAmount; } set { _bidOneAmount = value; this.OnPropertyChanged(); } }

        private string _ofrOne;
        public string OfrOne { get { return _ofrOne; } set { _ofrOne = value; this.OnPropertyChanged(); } }

        private string _ofrOneAmount;
        public string OfrOneAmount { get { return _ofrOneAmount; } set { _ofrOneAmount = value; this.OnPropertyChanged(); } }

        private Visibility _cornerOneBidVisibility = Visibility.Collapsed;
        public Visibility CornerOneBidVisibility
        {
            get { return _cornerOneBidVisibility; }
            set { _cornerOneBidVisibility = value; this.OnPropertyChanged(); }
        }

        private Visibility _cornerOneOfrVisibility = Visibility.Collapsed;
        public Visibility CornerOneOfrVisibility
        {
            get { return _cornerOneOfrVisibility; }
            set { _cornerOneOfrVisibility = value; this.OnPropertyChanged(); }
        }


        private string _bidTwo;
        public string BidTwo { get { return _bidTwo; } set { _bidTwo = value; this.OnPropertyChanged(); } }

        private string _bidTwoAmount;
        public string BidTwoAmount { get { return _bidTwoAmount; } set { _bidTwoAmount = value; this.OnPropertyChanged(); } }

        private string _ofrTwo;
        public string OfrTwo { get { return _ofrTwo; } set { _ofrTwo = value; this.OnPropertyChanged(); } }

        private string _ofrTwoAmount;
        public string OfrTwoAmount { get { return _ofrTwoAmount; } set { _ofrTwoAmount = value; this.OnPropertyChanged(); } }

        private Visibility _cornerTwoBidVisibility = Visibility.Collapsed;
        public Visibility CornerTwoBidVisibility
        {
            get { return _cornerTwoBidVisibility; }
            set { _cornerTwoBidVisibility = value; this.OnPropertyChanged(); }
        }

        private Visibility _cornerTwoOfrVisibility = Visibility.Collapsed;
        public Visibility CornerTwoOfrVisibility
        {
            get { return _cornerTwoOfrVisibility; }
            set { _cornerTwoOfrVisibility = value; this.OnPropertyChanged(); }
        }




        private string _bidThree;
        public string BidThree { get { return _bidThree; } set { _bidThree = value; this.OnPropertyChanged(); } }

        private string _bidThreeAmount;
        public string BidThreeAmount { get { return _bidThreeAmount; } set { _bidThreeAmount = value; this.OnPropertyChanged(); } }

        private string _ofrThree;
        public string OfrThree { get { return _ofrThree; } set { _ofrThree = value; this.OnPropertyChanged(); } }

        private string _ofrThreeAmount;
        public string OfrThreeAmount { get { return _ofrThreeAmount; } set { _ofrThreeAmount = value; this.OnPropertyChanged(); } }

        private Visibility _cornerThreeBidVisibility = Visibility.Collapsed;
        public Visibility CornerThreeBidVisibility
        {
            get { return _cornerThreeBidVisibility; }
            set { _cornerThreeBidVisibility = value; this.OnPropertyChanged(); }
        }

        private Visibility _cornerThreeOfrVisibility = Visibility.Collapsed;
        public Visibility CornerThreeOfrVisibility
        {
            get { return _cornerThreeOfrVisibility; }
            set { _cornerThreeOfrVisibility = value; this.OnPropertyChanged(); }
        }




        private string _bidFour;
        public string BidFour { get { return _bidFour; } set { _bidFour = value; this.OnPropertyChanged(); } }

        private string _bidFourAmount;
        public string BidFourAmount { get { return _bidFourAmount; } set { _bidFourAmount = value; this.OnPropertyChanged(); } }

        private string _ofrFour;
        public string OfrFour { get { return _ofrFour; } set { _ofrFour = value; this.OnPropertyChanged(); } }

        private string _ofrFourAmount;
        public string OfrFourAmount { get { return _ofrFourAmount; } set { _ofrFourAmount = value; this.OnPropertyChanged(); } }

        private Visibility _cornerFourBidVisibility = Visibility.Collapsed;
        public Visibility CornerFourBidVisibility
        {
            get { return _cornerFourBidVisibility; }
            set { _cornerFourBidVisibility = value; this.OnPropertyChanged(); }
        }

        private Visibility _cornerFourOfrVisibility = Visibility.Collapsed;
        public Visibility CornerFourOfrVisibility
        {
            get { return _cornerFourOfrVisibility; }
            set { _cornerFourOfrVisibility = value; this.OnPropertyChanged(); }
        }



        private string _bidFive;
        public string BidFive { get { return _bidFive; } set { _bidFive = value; this.OnPropertyChanged(); } }

        private string _bidFiveAmount;
        public string BidFiveAmount { get { return _bidFiveAmount; } set { _bidFiveAmount = value; this.OnPropertyChanged(); } }

        private string _ofrFive;
        public string OfrFive { get { return _ofrFive; } set { _ofrFive = value; this.OnPropertyChanged(); } }

        private string _ofrFiveAmount;
        public string OfrFiveAmount { get { return _ofrFiveAmount; } set { _ofrFiveAmount = value; this.OnPropertyChanged(); } }

        private Visibility _cornerFiveBidVisibility = Visibility.Collapsed;
        public Visibility CornerFiveBidVisibility
        {
            get { return _cornerFiveBidVisibility; }
            set { _cornerFiveBidVisibility = value; this.OnPropertyChanged(); }
        }

        private Visibility _cornerFiveOfrVisibility = Visibility.Collapsed;
        public Visibility CornerFiveOfrVisibility
        {
            get { return _cornerFiveOfrVisibility; }
            set { _cornerFiveOfrVisibility = value; this.OnPropertyChanged(); }
        }



        private Visibility _oneVisibility = Visibility.Collapsed;
        public Visibility OneVisibility
        {
            get { return _oneVisibility; }
            set { _oneVisibility = value; this.OnPropertyChanged(); }
        }

        private Visibility _twoVisibility = Visibility.Collapsed;
        public Visibility TwoVisibility
        {
            get { return _twoVisibility; }
            set { _twoVisibility = value; this.OnPropertyChanged(); }
        }

        private Visibility _threeVisibility = Visibility.Collapsed;
        public Visibility ThreeVisibility
        {
            get { return _threeVisibility; }
            set { _threeVisibility = value; this.OnPropertyChanged(); }
        }

        private Visibility _fourVisibility = Visibility.Collapsed;
        public Visibility FourVisibility
        {
            get { return _fourVisibility; }
            set { _fourVisibility = value; this.OnPropertyChanged(); }
        }

        private Visibility _fiveVisibility = Visibility.Collapsed;
        public Visibility FiveVisibility
        {
            get { return _fiveVisibility; }
            set { _fiveVisibility = value; this.OnPropertyChanged(); }
        }

        #endregion

    }
}
