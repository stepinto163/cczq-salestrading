using STC.Broker.Plugin.Model;
using STC.Common.Model.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Data.Plugin.Model.Base
{
    public class BondsSearchReplyPo:BaseReplyPo
    {
        public List<BondsInfoPo> datas;
        public int Total;
    }
}
