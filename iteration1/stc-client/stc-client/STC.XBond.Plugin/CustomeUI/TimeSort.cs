using STC.XBond.Plugin.CustomUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.XBond.Plugin.CustomeUI
{
    /// <summary>
    /// 代办事项，消息提醒数据排序类
    /// </summary>
    public class TimeSort : IComparer<MsgPo>
    {
        public int Compare(MsgPo x, MsgPo y)
        {
            return y.UpdateTime.CompareTo(x.UpdateTime);
        }
    }
}
