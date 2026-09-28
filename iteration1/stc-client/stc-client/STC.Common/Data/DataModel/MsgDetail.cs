using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Data.DataModel
{
    public class DetailOrder
    {
        public string securityid;
        public string symbol;
        public string side;
        public string settltype;
        public decimal? price;
        public long? orderqty;
        public string username;
    }
}
