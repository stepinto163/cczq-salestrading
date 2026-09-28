using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.Table
{
    public class QueryQuoteTagReplyPo : BaseReplyPo
    {
        public List<QuoteTag> Datas;
    }

    public class QuoteTag
    {
        public string tagType { get; set; } // 标签类型
        public string tagTypeName { get; set; }// 标签类型名称
        public int tagTypeStartIndex { get; set; }// 标签类型占二进制字符的长度
        public int tagTypeLength { get; set; }// 标签类型在二进制字符中的起点
        public string signField { get; set; }//sign , sign1, sign2
        public List<ChildTag> childTags { get; set; }
    }

    public class ChildTag
    {
        public string childTag { get; set; }//子标签
        public string childTagName { get; set; }//子标签名称
        public string childTagValue { get; set; }//子标签
        public string firstletter { get; set; }//首字母
        public string pinyin { get; set; }//拼音
        public List<ChildTag> childTags { get; set; }
    }
}
