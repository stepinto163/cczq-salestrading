using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace STC.Common.Utils
{
    /// <summary>
    /// 线程池公共类
    /// </summary>
    public class ThreadPoolHelper
    {
        public static void InvokeThreadPool(WaitCallback action)
        {
            ThreadPool.QueueUserWorkItem(action);
        }

        public static void InvokeThreadPool(WaitCallback action, object state)
        {
            ThreadPool.QueueUserWorkItem(action, state);
        }
    }
}
