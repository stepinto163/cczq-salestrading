using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.PT
{
    public class SystemPowerReplay : BaseReplyPo
    {
        public Dictionary<string, List<SystemPowerModel>> Datas { get; set; }


    }


    public class SystemPowerModel
    {

        public string userId { get; set; }
        public string userName { get; set; }
        public string userDisplayName { get; set; }
    }

}
