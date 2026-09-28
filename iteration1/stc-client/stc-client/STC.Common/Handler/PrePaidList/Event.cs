using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Handler.PrePaidList
{
    public class Event
    {
        public Event()
        {
            value = new Dictionary<string, object>();
        }
        public IDictionary<string, object> value { get; set; }
        public string name { get; set; }//信号名字

    }
}
