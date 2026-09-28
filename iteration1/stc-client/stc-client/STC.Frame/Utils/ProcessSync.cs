using com.gateway.log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace STC.Frame.Utils
{
    public class ProcessSync
    {
        private string syncName = "dps";
        EventWaitHandle mt;
        public int TimeOut = 10 * 1000;

        public ProcessSync(string syncName)
        {
            this.syncName = syncName;
            mt = new EventWaitHandle(false, EventResetMode.AutoReset, syncName);
        }
        public bool WaitOne()
        {
            return WaitOne(TimeOut);
        }
        public bool WaitOne(int timeOut)
        {
            LogHelper.Info("WaitOne Begin");
            bool flag = mt.WaitOne(timeOut);
            LogHelper.Info("WaitOne End:" + flag);
            return flag;
        }
        public void Notify()
        {
            mt.Set();
            LogHelper.Info("Notify");
        }
    }
}
