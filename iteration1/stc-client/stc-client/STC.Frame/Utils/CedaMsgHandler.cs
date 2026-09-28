using com.adaptiveMQ2.event_c;
using com.adaptiveMQ2.message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDC.Frame.Utils
{
    public class CedaMsgHandler : IMessageListener
    {
        public event Action<string,string, Message> ProcessMsg;
        public void onMessage(Message msg)
        {
            string topic = msg.Destination.getName();
            string svrId = msg.SvrID;
            if (ProcessMsg != null)
            {
                ProcessMsg(svrId, topic, msg);
            }
        }
    }
}
