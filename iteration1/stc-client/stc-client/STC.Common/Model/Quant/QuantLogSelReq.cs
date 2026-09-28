using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.Quant
{
    /// <summary>
    /// 策略日志查询
    /// </summary>
    public class QuantLogSelReq
    {
        // 开始日期
        public string beginDate;
        // 结束日期
        public string endDate;
        //策略quantid
        public string quantid;
        //策略id
        public string id;
        //第几页
        public int pageNum;
        //每页条数
        public int pageSize = 5000;
    }
}
