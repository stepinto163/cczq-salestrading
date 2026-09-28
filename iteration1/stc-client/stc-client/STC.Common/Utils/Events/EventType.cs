using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Utils.Events
{
    /// <summary>
    /// 订阅发布事件基类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class EventType<T>
    {
        private static Dictionary<object, List<Action<T>>> eventActions = new Dictionary<object, List<Action<T>>>();

        string TypeName
        {
            get
            {
                return this.GetType().Name;
            }
        }

        /// <summary>
        /// 订阅某一类型事件
        /// </summary>
        /// <param name="action"></param>
        public void Subscribe(Action<T> action)
        {
            if (action == null) return;
            if (eventActions.ContainsKey(TypeName))
            {
                eventActions[TypeName].Add(action);
            }
            else
            {
                eventActions.Add(TypeName, new List<Action<T>>() { action });
            }
        }

        /// <summary>
        /// 取消订阅某一类事件
        /// </summary>
        /// <param name="action"></param>
        public void UnSubscribe(Action<T> action)
        {
            if (action == null) return;
            if (eventActions.ContainsKey(TypeName))
            {
                eventActions[TypeName].Remove(action);
            }
        }

        /// <summary>
        /// 发布事件
        /// </summary>
        /// <param name="obj"></param>
        public void Publish(T obj)
        {
            if (eventActions.ContainsKey(TypeName))
            {
                foreach(var item in eventActions[TypeName])
                {
                    item?.Invoke(obj);
                }
            }
        }
    }

    public class NotifyCloseTabItem: EventType<int> { }

}
