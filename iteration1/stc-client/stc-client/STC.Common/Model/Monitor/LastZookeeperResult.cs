using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.Monitor
{
    public class LastZookeeperResult
    {
        public string Code { get; set; }
        public List<monitormodel> Datas { get; set; }
        public string Msg { get; set; }
    }
}
