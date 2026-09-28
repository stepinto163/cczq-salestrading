using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.Position
{
    public class QureyHistoryDetailResult : BaseReplyPo
    {
        public int totalSize;
        public List<PositionDetailModel> Datas { get; set; }
    }
}
