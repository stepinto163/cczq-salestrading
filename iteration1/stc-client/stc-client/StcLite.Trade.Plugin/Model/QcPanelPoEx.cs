using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BDB.Trade.Plugin.Model
{
    public class QcPanelPoEx : QcPanelPo
    {

        public int PageIndex { get; set; }
        public string GroupId { get; set; }

    }
}
