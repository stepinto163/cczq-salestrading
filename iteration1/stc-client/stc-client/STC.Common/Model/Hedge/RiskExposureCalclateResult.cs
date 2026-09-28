using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.Hedge
{
    public class RiskExposureCalclateResult : BaseReplyPo
    {
        public List<Sensitive> Datas;
    }

    public class Sensitive
    {
        public string combineName { get; set; }
        public string dataTime { get; set; }
        public double quantity { get; set; }
        public Dictionary<string, string> rateKrdMap { get; set; }
        public string securityId { get; set; }
    }
}
