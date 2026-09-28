using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.Ans
{
    public class QueryANSOutlineReplyPo : BaseReplyPo
    {
        public int count; //总条数
        public int pageNum;//当前页
        public int pageSize;//每页条数
        public List<ANSOutline> Datas;
    }

    public class ANSOutline
    {
        public string date;// 日期 MM-dd
        public int bncSecurityCount;// 利率债有效报价
        public int bncSecurityTypeCount;//利率债品种分布
        public int bncTimetomaturityCount;// 利率债剩余期限
        public int bncNDCount;// 利率债国债数量
        public int bncNewNDCount;// 利率债新发国债期限
        public int bcoSecurityCount;// 信用债有效报价
        public int bcoSecurityTypeCount;// 信用债品种分布
        public int bcoIssuerCount;// 信用债发行人覆盖

    }
}
