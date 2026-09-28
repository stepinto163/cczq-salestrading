using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.Common
{
    public class PriceType
    {
        public static readonly string Intention = "0";//意向
        public static readonly string NetPrice = "1";//净价
        public static readonly string FullPrice = "2";//全价
        public static readonly string Yield = "3";//收益率
        public static readonly string InterestMargin = "4";//利差
        public static readonly string BrokerPoint = "5";//经纪商指定
    }
}
