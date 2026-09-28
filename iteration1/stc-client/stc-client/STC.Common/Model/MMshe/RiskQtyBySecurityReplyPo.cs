using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.MMshe
{
    public class RiskQtyBySecurityReplyPo : BaseReplyPo
    {
        public RiskQtyBySecurity Data;
    }

    public class RiskQtyBySecurity {
        public string riskMinQtySize;
        public string riskMaxQtyRatio;
    }
}
