using BDB.Trade.Plugin.CustomUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BDB.Trade.Plugin.CustomeUI
{
    public class TimeSort : IComparer<MsgPo>
    {
        public int Compare(MsgPo x, MsgPo y)
        {
            return y.UpdateTime.CompareTo(x.UpdateTime);
        }
    }
}
