using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.MDHist
{
    public class TKLineReply : BaseReplyPo
    {
        public List<kLineData> PriceDatas = new List<kLineData>();

        public string Data { get; set; }

    }
}
