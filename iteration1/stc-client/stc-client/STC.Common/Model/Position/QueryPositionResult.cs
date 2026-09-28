using System.Collections.Generic;

namespace STC.Common.Model.Position
{
    public class QueryPositionResult : BaseReplyPo
    {
        public int totalSize;
        public List<PositionDetailModel> Datas { get; set; }
    }
}
