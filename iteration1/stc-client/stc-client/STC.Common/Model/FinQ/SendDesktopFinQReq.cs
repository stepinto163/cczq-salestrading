using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.FinQ
{
    public class SendDesktopFinQReq
    {
        public string sender;
        public string receiver;
        public bool send_direct;
        public bool clear_edit;
        public string channel;
        public string message;
        public string Username;
    }
}
