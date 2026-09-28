using STC.Common.Model.Fix.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.PT
{
  public  class PtExecordersReplyPo: BaseReplyPo
    {
        public int totalSize;
        public List<TbookPtExecorders> Datas;
    }
}
