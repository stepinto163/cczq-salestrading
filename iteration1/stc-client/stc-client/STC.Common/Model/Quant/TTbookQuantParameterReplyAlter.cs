using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.Quant
{
   public class TTbookQuantParameterReplyAlter : BaseReplyPo
    {
        public ErrorMsg Datas;
    }

    public class ErrorMsg
    {
        public string username;
        public string msg;
    }
}
