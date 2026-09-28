using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.Base
{
    public class BrokerAccount
    {
        public string orgId { get; set; }
        public string astChildTp { get; set; }
        public string astParentTp { get; set; }
        public string brokerContactId { get; set; }
        public string qm { get; set; }
        public string qq { get; set; }
        public string rm { get; set; }
        public string userId { get; set; }
        public string userNm { get; set; }
    }
}
