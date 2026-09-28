using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.Ans
{
    public class QueryANSDetailReplyPo : BaseReplyPo
    {
        public int count; //总条数
        public int pageNum;//当前页
        public int pageSize;//每页条数
        public List<ANS> Datas;
    }

    public class ANS
    {
        public string date;// 日期 MM-dd
        public string securityId;// 债券代码
        public string symbol;// 债券简称
        public string symbolType;//债券种类
        public string username;// 报价人
        public string securitySuperType;// 债券类型
        public string securityTypeId; //债券分类
        public string timetomaturity;// 剩余期限
        public string timetomaturityGroup;// 剩余期限分组
        public long quoteTimeLength;//  报价时长
        public int executionCount;// 成交笔数
        public decimal executionQty;// 成交金额
        public decimal avgBP;// 平均点差
        public string quoteBeginTime;// 报价开始时间
        public string quoteEndTime;// 报价结束时间

    }
}
