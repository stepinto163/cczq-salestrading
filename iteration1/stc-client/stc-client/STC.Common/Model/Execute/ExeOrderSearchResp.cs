using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.Execute
{
    public class ExeOrderSearchResp
    {
        public string Code { get; set; }
        public string Msg { get; set; }
        public int TotalCount { get; set; }
        public int PageNum { get; set; }
        public int PageSize { get; set; }
        public int HasMoreData { get; set; }

        public List<ExecuteOrder> Datas { get; set; }
    }
}
