using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace STC.Frame.Utils
{
    public class BaseMachineHelper
    {
        private static Mutex mutex = null;

        public static EventWaitHandle _appEvent = null;

        /// <summary>
        /// 检查是否启动了多个进程
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static bool CheckProcessIsMultiple(string name)
        {
            bool newMutexCreated = false;
            try
            {
                string mutexName = "Global\\" + name;
                mutex = new Mutex(false, mutexName, out newMutexCreated);
            }
            catch
            {

            }
            return !newMutexCreated;
        }
    }
}
