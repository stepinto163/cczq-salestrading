using com.gateway.log;
using STC.Common.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace STC.WorkBench.Plugin.Utils.Data.CRM.Handler
{
    public class SyncDataHandler
    {
        private Random random = new Random();
        private System.Threading.Timer timer = null;
        private static SyncDataHandler syncDataHelper = new SyncDataHandler();
        public static SyncDataHandler Instance { get { return syncDataHelper; } }

        private SyncDataHandler()
        {
        }

        public void Start()
        {
            //第一次开始的时间
            int hour = 0;
            int minute = 0;
            int second = 0;
            bool needAddDay = true;
            if (!string.IsNullOrEmpty(LocalEnv.ConfInfo.ResetDataTime))
            {
                string[] arr = LocalEnv.ConfInfo.ResetDataTime.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                if (arr != null && arr.Length > 2)
                {
                    int.TryParse(arr[0], out hour);
                    int.TryParse(arr[1], out minute);
                    int.TryParse(arr[2], out second);

                    needAddDay = false;
                }
                else
                {
                    hour = random.Next(2, 6);
                    minute = random.Next(10, 59);
                    second = random.Next(10, 59);
                }
            }
            else
            {
                hour = random.Next(2, 6);
                minute = random.Next(10, 59);
                second = random.Next(10, 59);
            }

            DateTime startTime = new DateTime(
                DateTime.Now.Year,
                DateTime.Now.Month,
                DateTime.Now.Day, hour
                , minute, second);

            if (needAddDay)
                startTime.AddDays(1);

            if (startTime < DateTime.Now)
                startTime = startTime.AddDays(1.0);

            TimeSpan delayTime = (startTime - DateTime.Now);

            TimeSpan intervalTime = new TimeSpan(1, 0, 0, 0); // 1 天

            timer = new System.Threading.Timer(new TimerCallback(SyncData), null, delayTime, intervalTime);

            LogHelper.Debug(string.Format("Sync data time {0}:{1}:{2}", startTime.Hour, startTime.Minute, startTime.Second));
        }
        private void SyncData(object obj)
        {
            ThreadPool.QueueUserWorkItem(o =>
            {
                try
                {
                    LogHelper.Debug("Start sync insts.");
                    DataHandler.Instance.SyncInsts();
                }
                catch (Exception ex)
                {
                    LogHelper.Debug("Sync insts exception.");
                    LogHelper.Error(ex);
                }
                //try
                //{
                //    LogHelper.Debug("Start sync contact.");
                //    DataHandler.Instance.SyncContact();
                //}
                //catch (Exception ex)
                //{
                //    LogHelper.Debug("Sync contact exception.");
                //    LogHelper.Error(ex);
                //}
                
            });
        }
    }
}
