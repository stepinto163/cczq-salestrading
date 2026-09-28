using STC.Common.Model.Fix.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.Admin
{
    public class TbookSystemParameterReplyPo : PResult
    {
        public TSystemGlobalParam Datas;
    }
    public class TSystemGlobalParam
    {
        public TTbookSystemParameter tbookSystemParameter;
    }
}
