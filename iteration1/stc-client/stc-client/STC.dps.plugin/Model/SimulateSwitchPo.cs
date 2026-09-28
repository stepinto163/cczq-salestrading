using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.dps.plugin.Model
{
    public class SimulateSwitchPo
    {
        /// <summary>
        /// STCExcel开头的发送
        /// </summary>
        public string terminal;
        /// <summary>
        /// 0: 不发送
        /// 1: 发送
        /// </summary>
        public string confirmflag;
    }
}
