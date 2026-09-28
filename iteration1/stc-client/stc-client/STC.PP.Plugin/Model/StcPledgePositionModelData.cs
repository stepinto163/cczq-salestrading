using STC.Common.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.PP.Plugin.Model
{
    /// <summary>
    /// 质押券持仓
    /// </summary>
    public class StcPledgePositionModelData : BaseViewModel
    {
        private bool _isChecked = false;
        public bool IsChecked
        {
            get { return _isChecked; }
            set
            {
                if (_isChecked != value)
                {
                    _isChecked = value; OnPropertyChanged();
                }
            }
        }


        private string _portfolioNo;
        /// <summary>
        /// 组合代码
        /// </summary>
        public string PortfolioNo
        {
            get { return _portfolioNo; }
            set { if (_portfolioNo != value) { _portfolioNo = value; OnPropertyChanged(); } }
        }


        private string _portfolioName;
        /// <summary>
        /// 组合名称
        /// </summary>
        public string PortfolioName
        {
            get { return _portfolioName; }
            set { if (_portfolioName != value) { _portfolioName = value; OnPropertyChanged(); } }
        }


        private string _tradeAccount;
        /// <summary>
        /// 交易账户代码
        /// </summary>
        public string TradeAccount
        {
            get { return _tradeAccount; }
            set { if (_tradeAccount != value) { _tradeAccount = value; OnPropertyChanged(); } }
        }

        private string _tradeName;

        /// <summary>
        /// 交易账户名称
        /// </summary>
        public string TradeName
        {
            get { return _tradeName; }
            set { if (_tradeName != value) { _tradeName = value; OnPropertyChanged(); } }
        }


        private string _securityId;
        /// <summary>
        /// 债券代码
        /// </summary>
        public string SecurityId
        {
            get { return _securityId; }
            set { if (_securityId != value) { _securityId = value; OnPropertyChanged(); } }
        }


        private string _symbolName;
        /// <summary>
        /// 债券名称
        /// </summary>
        public string SymbolName
        {
            get { return _symbolName; }
            set { if (_symbolName != value) { _symbolName = value; OnPropertyChanged(); } }
        }

        private string _tradeType;
        /// <summary>
        /// 类型：1:质押, 2:STC在途意向,3:STC在途成交
        /// </summary>
        public string TradeType
        {
            get { return _tradeType; }
            set { if (_tradeType != value) { _tradeType = value; OnPropertyChanged(); } }
        }

        private string _side;

        /// <summary>
        /// 方向
        /// </summary>
        public string Side
        {
            get { return _side; }
            set { if (_side != value) { _side = value; OnPropertyChanged(); } }
        }


        private decimal _qty;
        /// <summary>
        /// 量
        /// </summary>
        public decimal Qty
        {
            get { return _qty; }
            set { if (_qty != value) { _qty = value; OnPropertyChanged(); } }
        }

        private string _maturityDate;

        /// <summary>
        /// 到期日
        /// </summary>
        public string MaturityDate
        {
            get { return _maturityDate; }
            set { if (_maturityDate != value) { _maturityDate = value; OnPropertyChanged(); } }
        }


        /// <summary>
        /// 是否是当天
        /// </summary>
        public string IsCurrentDay
        {
            get 
            {
                string currentday = DateTime.Now.ToString("yyyyMMdd");
                return currentday == MaturityDate ? "1" : "0" ;
            }
        }

        private string _counterParty;
        /// <summary>
        /// 对手方
        /// </summary>
        public string CounterParty
        {
            get { return _counterParty; }
            set { if (_counterParty != value) { _counterParty = value; OnPropertyChanged(); } }
        }

        public StcPledgePositionModelData()
        {
        }
    }
}
