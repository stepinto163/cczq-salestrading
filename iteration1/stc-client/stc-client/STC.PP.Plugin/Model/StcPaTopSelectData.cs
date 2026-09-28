using STC.Common.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.PP.Plugin.Model
{
    public class StcPaTopSelectData : BaseViewModel
    {
        private string _bookitem;
        /// <summary>
        /// 选中的组合代码
        /// </summary>
        public string BookItem
        {
            get { return _bookitem; }
            set { if (_bookitem != value) { _bookitem = value; OnPropertyChanged(); } }
        }


        private string _bookitemText;
        /// <summary>
        /// 选中的组合代码Text
        /// </summary>
        public string BookItemText
        {
            get { return _bookitemText; }
            set { if (_bookitemText != value) { _bookitemText = value; OnPropertyChanged(); } }
        }

        private string _fundaccountitem;
        /// <summary>
        /// 选中的投资经理
        /// </summary>
        public string FundAccountItem
        {
            get { return _fundaccountitem; }
            set { if (_fundaccountitem != value) { _fundaccountitem = value; OnPropertyChanged(); } }
        }


        private string _tradeaccountitem;
        /// <summary>
        /// 选择的交易账户
        /// </summary>
        public string TradeAccountItem
        {
            get { return _tradeaccountitem; }
            set { if (_tradeaccountitem != value) { _tradeaccountitem = value; OnPropertyChanged(); } }
        }

        private string _startexcloseitem;
        /// <summary>
        /// 交易所收盘价-估值开始
        /// </summary>
        public string StartExCloseItem
        {
            get { return _startexcloseitem; }
            set { if (_startexcloseitem != value) { _startexcloseitem = value; OnPropertyChanged(); } }
        }

        private string _endExCloseItem;
        /// <summary>
        /// 交易所收盘价-估值 结束
        /// </summary>
        public string EndExCloseItem
        {
            get { return _endExCloseItem; }
            set
            {
                _endExCloseItem = value;
                this.OnPropertyChanged();
            }
        }

        private string _mboltype = "0";
        /// <summary>
        /// 债券类型 BCO,BNC,ABS,NCD  转换关系：0-全 1-信用债 BCO 2-利率债 BNC 11-ABS 12-NCD   BizTypeConvert
        /// </summary>
        public string SymbolType
        {
            get { return _mboltype; }
            set
            {
                _mboltype = value;
                this.OnPropertyChanged();
            }
        }

        private string _startTermDY = string.Empty;
        /// <summary>
        /// 期限 开始
        /// </summary>
        public string StartTermDYItem
        {
            get { return _startTermDY; }
            set
            {
                if (_startTermDY != value)
                {
                    _startTermDY = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private string _endTermDY = string.Empty;
        /// <summary>
        /// 期限 结束
        /// </summary>
        public string EndTermDYItem
        {
            get { return _endTermDY; }
            set
            {
                if (_endTermDY != value)
                {
                    _endTermDY = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private bool _IsAppTerm;

        /// <summary>
        /// 是否是自定义期限，已经选择了自定义期限后，其他的期限选择会被清空，该定义已经废弃，为了兼容，故保留
        /// </summary>
        public bool IsAppTermItem
        {
            get { return _IsAppTerm; }
            set
            {
                _IsAppTerm = value;
            }
        }



        private string _startCoupon;
        /// <summary>
        /// 票面 开始
        /// </summary>
        public string StartCouponItem
        {
            get { return _startCoupon; }
            set
            {
                _startCoupon = value;
                this.OnPropertyChanged();
            }
        }

        private string _endCoupon;
        /// <summary>
        /// 票面 结束
        /// </summary>
        public string EndCouponItem
        {
            get { return _endCoupon; }
            set
            {
                _endCoupon = value;
                this.OnPropertyChanged();
            }
        }

        private bool _isCoupon;
        /// <summary>
        /// 不用选择票面
        /// </summary>
        public bool IsCouponItem
        {
            get { return _isCoupon; }
            set
            {
                _isCoupon = value;
                this.OnPropertyChanged();
            }
        }

        private string _startPrice;
        /// <summary>
        /// 报价（收益率） 开始
        /// </summary>
        public string StartPriceItem
        {
            get { return _startPrice; }
            set
            {
                _startPrice = value;
                this.OnPropertyChanged();
            }
        }

        private string _endPrice;
        /// <summary>
        /// 报价（收益率） 结束
        /// </summary>
        public string EndPriceItem
        {
            get { return _endPrice; }
            set
            {
                _endPrice = value;
                this.OnPropertyChanged();
            }
        }

        private string _startChinaBondValue;
        /// <summary>
        /// 中债估值 开始
        /// </summary>
        public string StartChinaBondItem
        {
            get { return _startChinaBondValue; }
            set
            {
                _startChinaBondValue = value;
                this.OnPropertyChanged();
            }
        }

        private string _endChinaBondValue;
        /// <summary>
        /// 中债估值 结束
        /// </summary>
        public string EndChinaBondItem
        {
            get { return _endChinaBondValue; }
            set
            {
                _endChinaBondValue = value;
                this.OnPropertyChanged();
            }
        }

        private string _StartBidBp;
        /// <summary>
        /// bid-中债 开始
        /// </summary>
        public string StartBidBpItem
        {
            get { return _StartBidBp; }
            set
            {
                _StartBidBp = value;
                this.OnPropertyChanged();
            }
        }


        private string _EndBidBp;
        /// <summary>
        /// bid-中债 结束
        /// </summary>
        public string EndBidBpItem
        {
            get { return _EndBidBp; }
            set
            {
                _EndBidBp = value;
                this.OnPropertyChanged();
            }
        }

        private string _StartOfrBp;
        /// <summary>
        /// 中债-ofr 开始
        /// </summary>
        public string StartOfrBpItem
        {
            get { return _StartOfrBp; }
            set
            {
                _StartOfrBp = value;
                this.OnPropertyChanged();
            }
        }

        private string _EndOfrBp;
        /// <summary>
        /// 中债-ofr 结束
        /// </summary>
        public string EndOfrBpItem
        {
            get { return _EndOfrBp; }
            set
            {
                _EndOfrBp = value;
                this.OnPropertyChanged();
            }
        }

    }
}
