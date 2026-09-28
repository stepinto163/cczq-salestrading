using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.Position
{
    public class QueryPositionBookResult : BaseReplyPo
    {
        public List<PostionBookModel> Datas { get; set; }
    }
}
