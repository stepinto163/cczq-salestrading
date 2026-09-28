using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.Table.WorkBrench
{
    public class RecognizeIntentionQuoteRequest
    {
        // stc用户名
        public string userName;
        // 意向报价原始文本
        public string content;

        public List<string> contentList;
    }
}
