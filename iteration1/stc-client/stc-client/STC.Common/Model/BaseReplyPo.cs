using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model
{
   public class BaseReplyPo
    {
        public string Code;
        public string Msg;
        public int? IsAll;

        public bool IsSuccess()
        {
            return "0000".Equals(Code);
        }
    }
}
