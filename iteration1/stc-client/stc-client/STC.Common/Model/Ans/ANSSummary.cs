using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.Ans
{
    public class ANSSummary
    {
        public string status;// 状态，1：正常，2：需关注，3：已达标，4：失败
        public string warnStatus;// 最低做市义务警告，1：需警告，0：无需警告
        public string checkTime;// 检查时间 HH:mm
        public string remainTime;// 剩余可报时间 HH:mm
        public string bncSecurityCount;// 利率债有效报价
        public string bncSecurityTypeCount;// 利率债品种分布
        public string bncTimetomaturityCount;// 利率债剩余期限
        public string bncNDCount;// 利率债国债数量
        public string bncNewNDCount;// 利率债新发国债期限
        public string bncCfetsSecurityCount;// 利率债cfets有效报价
        public string bncCfetsSecurityTypeCount;// 利率债cfets品种分布
        public string bncCfetsTimetomaturityCount;// 利率债cfets剩余期限
        public string bncCfetsNDCount;// 利率债cfets国债数量
        public string bncCfetsNewNDCount;// 利率债cfets新发国债期限
        public string bcoSecurityCount;// 信用债有效报价
        public string bcoSecurityTypeCount;// 信用债品种分布
        public string bcoIssuerCount;// 信用债发行人覆盖
        public string bcoCfetsSecurityCount;// 信用债cfets有效报价
        public string bcoCfetsSecurityTypeCount;// 信用债cfets品种分布
        public string bcoCfetsIssuerCount;// 信用债cfets发行人覆盖
        public List<SecurityTerm> securityTermLt;  // 利率债期限统计
        public List<SecurityType> securityTypeLt;//信用债分类统计
    }

    public class SecurityTerm
    {
        public string securityDec;//债券分类名称
        public string newBondSign;//新发国债标识 0-不是新发国债，1-是新发国债
        public int countSafe; // 安全只数
        public List<int> termCountList;//    期限集合，例如：[0，1，0，2，4，0]，分别对应0-1Y，1-3Y，3-5Y，5-7Y，7-10Y，>10Y的个数，注意：当securityDec为[新发国债] 时，对应原始期限的个数

    }

    public class SecurityType
    {
        public string securityDec;// 债券分类名称
        public int countSafe;// 安全只数

    }
}
