using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.Position
{
    public class QueryProcessingRecordResult
    {
        public int totalSize { get; set; }

        public List<QueryProcessingRecord> Datas { get; set; }
    }
}
