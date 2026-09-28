using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.PP
{
    public class StcTradingAccountModel
    {
        /// <summary>
        /// 交易账户名称
        /// </summary>
        public string accountName { get; set; }

        /// <summary>
        /// 交易账户代码
        /// </summary>
        public string accountNo { get; set; }

        /// <summary>
        /// 资产投向  assetAllocation=配置， cash=现金， equity=权益， fixedIncome=固收， fund=基金， futurs=期货， hkStockConnect=港股通， other=其他
        /// </summary>
        public string assetInvestment { get; set; }

        /// <summary>
        /// 资金帐号
        /// </summary>
        public string capitalAccountNo { get; set; }

        /// <summary>
        /// 场内/场外标志  true:场外  false:场内
        /// </summary>
        public bool isOtc { get; set; }


        public bool otc { get; set; }

        /// <summary>
        /// 组合代码
        /// </summary>
        public string portfolioNo { get; set; }

        /// <summary>
        /// 交易账户状态
        /// </summary>
        public int status { get; set; }
    }
}
