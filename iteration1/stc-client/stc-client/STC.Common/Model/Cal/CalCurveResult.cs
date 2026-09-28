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
    public class CalCurveResult : BaseReplyPo
    {
        public List<string[]> Datas { get; set; }
    }
}
