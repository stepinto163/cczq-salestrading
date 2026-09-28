using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.Drp
{
    public class DRPQueryOrderLogResult
    {
        public string Code;
        public List<DRPOrderLogModel> Datas;
        public string Msg;
        public int totalSize;
    }
}
