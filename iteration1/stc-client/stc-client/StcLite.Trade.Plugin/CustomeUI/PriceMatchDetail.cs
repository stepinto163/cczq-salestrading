using STC.Common.Model.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BDB.Trade.Plugin.CustomeUI
{
    public class PriceMatchDetail
    {
        public Order order;
        public List<PriceValue> Prices = new List<PriceValue>();
        public Order iocOrder;
    }
}
