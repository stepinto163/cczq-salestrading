using Quartz;
using STC.Frame.Utils;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Speech.Plugin
{
    /// <summary>
    /// 定时每天去获取token
    /// </summary>
    class ScheduleToken : IJob
    {
        
        /// <summary>
        /// 创建要执行的作业
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task Execute(IJobExecutionContext context)
        {
            await Task.Run(() => {
                string accessKeyID = ConfigurationManager.AppSettings["AccessKeyID"].ToString();
                LogHandler.Info("AccessKeyID:" + accessKeyID);
                string accessKeySecret = ConfigurationManager.AppSettings["AccessKeySecret"].ToString();
                LogHandler.Info("AccessKeySecret:" + accessKeySecret);
                MainWindow.token = RefulshToken.getToken(accessKeyID,accessKeySecret);
            });
        }
    }
}
