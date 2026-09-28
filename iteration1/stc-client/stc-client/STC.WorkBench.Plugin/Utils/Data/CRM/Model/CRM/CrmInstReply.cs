
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.WorkBench.Plugin.Utils.Data.CRM.Model.CRM
{
    public class CrmInstReply : BaseReplyPo
    {
        public int totalSize;
        public List<CrmInst> Datas { get; set; }
    }
}
