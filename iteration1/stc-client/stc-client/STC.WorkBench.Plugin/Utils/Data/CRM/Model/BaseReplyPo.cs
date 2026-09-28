using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.WorkBench.Plugin.Utils.Data.CRM.Model
{
   public class BaseReplyPo
    {
        public string Code;
        public string Msg;
        public int? IsAll;

        public bool IsSuccess()
        {
            return Code.Equals("0000");
        }
    }
}
