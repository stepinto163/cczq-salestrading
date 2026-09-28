using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace STC.Common.Model.PP
{
    /// <summary>
    /// 查询债券持仓
    /// </summary>
    public class QueryBondPositionReq
    {
        /// <summary>
        /// 页码
        /// </summary>
        public int pageNum = 0;

        /// <summary>
        /// 条数
        /// </summary>

        public int pageSize = 1000;

        /// <summary>
        /// 用户名
        /// </summary>
        public string userName = null;

        /// <summary>
        /// 搜索框文本
        /// </summary>
        public string securityId;

        /// <summary>
        /// 投资经理
        /// </summary>
        public string tradeManager;

        /// <summary>
        /// 交易账户
        /// </summary>
        public string tradeAccount;
        public string tradeAccount1;
        public string tradeAccount2;
        public string tradeAccount3;
        public string tradeAccount4;
        public string tradeAccount5;
        public string tradeAccount6;
        public string tradeAccount7;
        public string tradeAccount8;
        public string tradeAccount9;
        public string tradeAccount10;

        /// <summary>
        /// value1,value2，中债估值
        /// </summary>
        public string chinaBondValue;

        /// <summary>
        /// value1,value2，交易所收盘价
        /// </summary>
        public string exClose2;

        /// <summary>
        /// 债券类型 1-信用债 BCO 2-利率债 BNC 11-ABS 12-NCD   BizTypeConvert
        /// </summary>
        public string symbolType;

        // 标签
        //new Dictionary<string, List<string>>();
        public Dictionary<string, List<string>> reqTags = null;

        // 扩展参数  "TermDaysRange", "CouponRateCurrentRange", "PriceRange", "YieldRange", "ChinaBondValuationRange"
        public Dictionary<string, string> extParams = null;
    }
}
