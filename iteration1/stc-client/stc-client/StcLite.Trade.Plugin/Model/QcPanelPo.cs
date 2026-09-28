using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BDB.Trade.Plugin.Model
{
    public class QcPanelPo
    {
        public int Row { get; set; }
        public int Col { get; set; }
        public string Id { get; set; }
        public string BondCode { get; set; }
        public string BrokerId { get; set; }
        public string BestType { get; set; }

        public QcPanelPo()
        {
            BrokerId = "";
            BestType = "3";
        }
    }
}
