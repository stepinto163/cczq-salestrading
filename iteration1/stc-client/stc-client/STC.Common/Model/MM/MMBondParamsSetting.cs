using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.MM
{
    public class MMBondParamsSetting: MMOrderParamReq
    {
        public string paramKey;//default-单券默认设置；CreditDefault-信用债默认；InterestDefault-利率债默认；InterestActive-活跃债；InterestBid-买入激进；InterestOfr:卖出激进
    }
}
