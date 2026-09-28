using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace netapi.Api
{
    public class NetMessage
    {
        public string Topic { get; set; }
        public string Data { get; set; }

        public string SendTime { get; set; }
    }
}
