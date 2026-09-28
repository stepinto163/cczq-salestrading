using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.Cal
{
    /// <summary>
    /// 请求计算净价、收益率返回结果
    /// </summary>
    public class CalResult : BaseReplyPo
    {
        public CalData Data;

        public string ReqData;
    }

    public class CalData
    {
        public decimal Yield { get; set; }
        public decimal CleanPrice { get; set; }
        public decimal YieldToExecution { get; set; }
        public decimal FullPrice { get; set; }
    }
}
