using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Utils.Events
{
    /// <summary>
    /// 事件接收分发工具类
    /// </summary>
    public class EventHub
    {
        public static T GetEvent<T>() where T : class, new()
        {
            return Activator.CreateInstance(typeof(T)) as T;
        }
    }
}
