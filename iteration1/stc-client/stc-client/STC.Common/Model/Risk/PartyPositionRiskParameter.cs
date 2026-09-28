using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.Risk
{
    /// <summary>
    /// 交易员头寸
    /// </summary>
    public class PartyPositionRiskParameter
    {
        public String stcAccount = "";//stc账户
        public String userName = "";//用户名
        public String dept;//部门
        public String interestRateLongExtremeValue = "";//利率多头限额
        public String interestRateShortExtremeValue = "";//利率空头限额
        public String creditExtremeValue;//信用限额
        public String update;//修改时间
    }
}
