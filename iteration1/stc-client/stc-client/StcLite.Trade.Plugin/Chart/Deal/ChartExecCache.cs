using com.gateway.log;
using OxyPlot.Axes;
using STC.Common.Model.Fix.Message;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace BDB.Price.Plugin.Chart.Deal
{
    public class ChartExecCache
    {
        private Thread timeThread;

        private List<ExecutionReport> execList = new List<ExecutionReport>();

        private object _actionLock = new object();
        private Action _BatchUpdateChatDataAction;
        public event Action BatchUpdateChartDataAction
        {
            add
            {
                lock (_actionLock)
                {
                    _BatchUpdateChatDataAction += value;
                }
            }
            remove
            {
                lock (_actionLock)
                {
                    _BatchUpdateChatDataAction -= value;
                }
            }
        }


        private ChartExecCache() {

            timeThread = new Thread(CalTime);
            timeThread.IsBackground = true;
            timeThread.Start();
        }

        private void CalTime()
        {
            try
            {
                while(true)
                {
                    _BatchUpdateChatDataAction?.Invoke();
                    Thread.Sleep(250);
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
            }
        }

        public void Init()
        {
            //#region 收MarketExec消息
            //ExecutionsHandler.Instance.OnMarketExecutions += Instance_OnMarketExecutions;
            //#endregion
        }

        private static ChartExecCache server = new ChartExecCache();

        public static ChartExecCache Instance { get { return server; } }
    }
}
