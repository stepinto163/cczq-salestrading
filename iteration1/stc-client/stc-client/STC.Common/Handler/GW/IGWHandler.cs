using STC.Common.Model.Fix.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Handler.GW
{
    public interface IGWHandler
    {
        PResult SendForGW(string topic, string content, bool isLocal = false);

        void SubscribeGW(string topic, Action<object> action);

        PResult PubEvent(string userName, string eventName, string content, bool isLocal);
    }
}
