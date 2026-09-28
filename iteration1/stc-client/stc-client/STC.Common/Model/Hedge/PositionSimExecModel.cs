using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.Hedge
{
    public class PositionSimExecModel
    {
        public string id { get; set; }

        public string securityId { get; set; }

        public string symbol { get; set; }

        public string bookId { get; set; }

        public string bookName { get; set; }

        public string side { get; set; }

        public decimal qty { get; set; }

        public string closeBy { get; set; }

        public string FullName { get; set; }

        public string updateTime { get; set; }
    }
}
