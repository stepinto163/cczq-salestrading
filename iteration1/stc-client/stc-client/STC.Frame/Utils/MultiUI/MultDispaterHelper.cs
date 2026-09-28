using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace STC.Frame.Utils.MultiUI
{
    public static class MultDispaterHelper
    {
        private static Dictionary<string, Dispatcher> dispatcherDic = new Dictionary<string, Dispatcher>();

        /// <summary>
        /// 根据模块名称找到对应的UI线程执行action
        /// </summary>
        /// <param name="modelName"></param>
        /// <returns></returns>
        public static async Task InvokeByModelName(string modelName, Action action)
        {
            Dispatcher dispatcher;
            if (!dispatcherDic.TryGetValue(modelName, out dispatcher))
            {
                dispatcher = await UIDispatcher.RunNewAsync();
                dispatcherDic[modelName] = dispatcher;
            }
            await dispatcher.InvokeAsync(action);
        }

        public static async Task<T> InvokeByModelName<T>(string modelName, Func<T> action)
        {
            Dispatcher dispatcher;
            if (!dispatcherDic.TryGetValue(modelName, out dispatcher))
            {
                dispatcher = await UIDispatcher.RunNewAsync();
                dispatcherDic[modelName] = dispatcher;
            }
            return await dispatcher.InvokeAsync(action);
        }
    }
}
