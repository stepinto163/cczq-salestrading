using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.dps.plugin.PrePaidList.Event
{/// <summary>
/// 信号接口
/// </summary>
    public interface IEventListener
    {
        string getOrderId();
        void onEvent(STC.Common.Handler.PrePaidList.Event ev);
    }
}
