using Quartz;
using STC.Common.Utils;
using STC.Speech.aliyun;
using STC.Speech.utils;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Speech.job
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
            await Task.Run(() =>
            {
                SpeechConsts.token = RefulshToken.getToken(SpeechConsts.accessKeyID, SpeechConsts.accessKeySecret);
                LogHandler.Info("ScheduleToken :" + SpeechConsts.token);
            });
        }
    }
}
