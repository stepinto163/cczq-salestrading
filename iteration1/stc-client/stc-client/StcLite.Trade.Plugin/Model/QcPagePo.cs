using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BDB.Trade.Plugin.Model
{
    public class QcPagePo
    {
        public int Index { get; set; }
        public bool IsSeleted { get; set; }
        public string GroupId { get; set; }
        public List<QcPanelPo> Panels { get; set; }

        public QcPagePo()
        {
            Panels = new List<QcPanelPo>();
        }
    }
}
