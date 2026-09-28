using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.Table
{
    public class QueryTagsLoadRequest
    {
        public string typeCode;//标签类型
        public int pageSize;//一页条数
        public int pageNum;//页数，从1开始
        public string maxTagValue;// 最大值
    }
}
