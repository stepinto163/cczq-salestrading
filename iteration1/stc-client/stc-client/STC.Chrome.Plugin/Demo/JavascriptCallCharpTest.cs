using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Chrome.Plugin.Demo
{
    /// <summary>
    /// js异步调用C#后台方法
    /// </summary>
    public class JavascriptCallCharpTest
    {
        private ChromeUserControl ChromeCtrl;

        /// <summary>
        /// js直接调用的c#后台演示类
        /// </summary>
        /// <param name="browse"></param>
        public JavascriptCallCharpTest(ChromeUserControl browse)
        {
            this.ChromeCtrl = browse;
        }

        /// <summary>
        /// JavaScript异步调用的函数，骆驼命名法
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public string doTest(string username, string password)
        {
            string res = string.Empty;
            var t = Task.Run(async () =>
            {
                res = await doTestTask(username, password);
            });

            Task.WaitAll(t);
            return res;
        }

        /// <summary>
        /// 通用异步函数，骆驼命名法
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public async Task<string> doTestTask(string username, string password)
        {
            string reply = null;
            await Task.Run(() =>
            {
                //堵塞线程 3秒，测试前台异步
                //Thread.Sleep(3000);
                reply = $"当前时间：{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}\r\nUserName：{username}\r\nSessionID:{password}";
            });
            return reply;
        }

    }
}
