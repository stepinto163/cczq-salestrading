using STC.Common.Model.Fix.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.BookManagement
{
    public class BlockUpdateReq
    {
        public List<ExecutionReport> blockdatalist = new List<ExecutionReport>();
    }
}
