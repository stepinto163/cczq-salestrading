using STC.Common.Model;
using STC.Common.Utils;
using STC.Frame.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace STC.Broker.Plugin.Model.Transaction
{
    /// <summary>
    /// 多最优价报价
    /// </summary>
    public class MultiBestQuoteModel : BaseViewModel
    {

        private string _Side = "";
        /// <summary>
        /// 方向 (side == "1") ofr
        /// </summary>
        public string Side 
        { 
            get 
            { 
                return _Side; 
            }
            set
            {
                _Side = value;
                this.OnPropertyChanged();
            }
        }



        private string _settlType;
        /// <summary>
        /// 清算速度 全 +1 +0 远
        /// </summary>
        public string SettlType 
        {
            get
            { 
                return _settlType; 
            }
            set
            {
                _settlType = value; 
                this.OnPropertyChanged();
            } 
        }


        private string _settlementType;
        /// <summary>
        /// 清算速度 列表  全   +1 2,3  +0 1 远 +
        /// </summary>
        public string SettlementType
        {
            get
            {
                return _settlementType;
            }
            set
            {
                _settlementType = value;
                this.OnPropertyChanged();
            }
        }


        #region 最优

        private decimal? _BestPrice;

        /// <summary>
        /// 价格  -1 999 = --
        /// </summary>
        public decimal? BestPrice
        {
            get
            {
                return _BestPrice;
            }
            set
            {
                _BestPrice = value;

                this.OnPropertyChanged();
            }
        }

        private string _Bestshowprice;

        /// <summary>
        /// 价格
        /// </summary>
        public string BestShowprice
        {
            get
            {
                return _Bestshowprice;
            }

            set
            {
                _Bestshowprice = value;
                this.OnPropertyChanged(nameof(BestShowprice));

                if (BestPrice.HasValue)
                    BestPriceFormat = CommonUtils.FormatNum(BestPrice.Value.ToString());
                else
                    BestPriceFormat = "-.-";

                BestPriceInt = BestPriceFormat.Substring(0, BestPriceFormat.IndexOf('.') + 1);
                BestPriceDecimal = BestPriceFormat.Substring(BestPriceFormat.IndexOf('.') + 1);
            }
        }

        private string _BestPriceFormat;
        public string BestPriceFormat
        {
            get
            {
                return _BestPriceFormat;
            }
            set
            {
                _BestPriceFormat = value;
                this.OnPropertyChanged();
            }

        }

        private string _BestPriceInt;
        public string BestPriceInt
        {
            get
            {
                return _BestPriceInt;
            }
            set
            {
                _BestPriceInt = value;
                this.OnPropertyChanged();
            }
        }

        private string _BestPriceDecimal;
        public string BestPriceDecimal
        {
            get
            {
                return _BestPriceDecimal;
            }
            set
            {
                _BestPriceDecimal = value;
                this.OnPropertyChanged();
            }
        }

        private decimal _BestSumQty;

        /// <summary>
        ///  最优 Bid Qty
        /// </summary>
        public decimal BestSumQty
        {
            get
            {
                return _BestSumQty;
            }
            set
            {
                _BestSumQty = value;

                if (value > 0)
                {
                    BestShowQty = (value / 1000).ToString("0.#");
                }
                else
                {
                    BestShowQty = "--";
                }

                this.OnPropertyChanged();
            }
        }


        private string _BestShowQty;

        /// <summary>
        /// 显示订单量
        /// </summary>
        public string BestShowQty
        {
            get
            {
                return _BestShowQty;
            }
            set
            {
                _BestShowQty = value;
                this.OnPropertyChanged();
            }
        }

        #endregion


        //priceType '0-意向;1-净价;2- 全价;3-收益率;4-利差;8-经纪商未指定',  当 priceType == 1   price=MDEntryPx 保存的是净价 yield保存是收益率 netPrice净价
        private string _PriceType;
        public string PriceType { get { return _PriceType; } set { _PriceType = value; this.OnPropertyChanged(); } }


        #region ID
        private string _id;
        /// <summary>
        /// 唯一标识
        /// </summary>
        public string id
        {
            get
            {
                return _id;
            }
            set
            {
                _id = value;
                this.OnPropertyChanged();
            }
        }

        private decimal _qty;

        /// <summary>
        /// 数量
        /// </summary>
        public decimal qty
        {
            get
            {
                return _qty;
            }
            set
            {
                _qty = value;
                BestSumQty = value;
                this.OnPropertyChanged();
            }
        }

        private int _MDPriceLevel = 0;
        /// <summary>
        /// 优先顺序
        /// </summary>
        public int MDPriceLevel
        {
            get
            {
                return _MDPriceLevel;
            }
            set
            {
                _MDPriceLevel = value;
                this.OnPropertyChanged();
            }
        }

        private ObservableCollection<GoodsVolumeVoModel> _ObsQuoteModel = new ObservableCollection<GoodsVolumeVoModel>();
        /// <summary>
        /// 最优价 ObservableCollection 逐笔报价列表，用于点价更新
        /// </summary>
        public ObservableCollection<GoodsVolumeVoModel> ObsQuoteModel
        {
            get
            {
                return _ObsQuoteModel;
            }
            set
            {
                _ObsQuoteModel = value;
                this.OnPropertyChanged(nameof(ObsQuoteModel));
            }
        }


        private bool isVisible = true;
        /// <summary>
        /// 是否显示
        /// </summary>
        public bool IsVisible
        {
            get
            {
                return isVisible;
            }
            set
            {
                isVisible = value;
                this.OnPropertyChanged();
            }
        }
        #endregion
    }
}

