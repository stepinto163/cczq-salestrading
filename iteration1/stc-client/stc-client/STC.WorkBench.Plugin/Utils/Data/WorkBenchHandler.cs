using com.gateway.log;
using STC.Common.Data;
using STC.Common.Utils;
using STC.WorkBench.Plugin.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace STC.WorkBench.Plugin.Utils.Data
{
    public class WorkBenchHandler
    {
        private static WorkBenchHandler _worktablehandler = new WorkBenchHandler();
        public static WorkBenchHandler Instance { get { return _worktablehandler; } }
        public WorkBenchHandler()
        {

        }

        public void RegSystemReadyEvent()
        {
            DesignHandler.Instance.OnSystemReady += Instance_OnSystemReady;
        }

        private void Instance_OnSystemReady(object obj)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                  //接收数据
                try
                {
                    IntentionOldHandler.Instance.init();
                }
                catch (Exception ex)
                {
                    LogHelper.Error(ex);
                }

                PopupWindowCommand popupWindowCommand = new PopupWindowCommand();
                //加载打开的窗口
                string lstkeysw = IniFileHelper.ReadValue($"STC.WorkBench.Plugin-{LocalEnv.UserInfo.UserName}", "Keys");
                string[] keyListw = lstkeysw.Split(',');
                for (int i = 0; i < keyListw.Length; i++)
                {
                    string winid = keyListw[i];
                    if (!string.IsNullOrEmpty(winid))
                    {
                        string para = IniFileHelper.ReadValue($"STC.WorkBench.Plugin_{LocalEnv.UserInfo.UserName}_{winid}", "CommandParameter");
                        if (!string.IsNullOrEmpty(para))
                        {
                            if (para.Contains(","))
                            {
                                string[] paralst = para.Split(',');
                                PopupTypeEnum pye = (PopupTypeEnum)Enum.Parse(typeof(PopupTypeEnum), paralst[0]);
                                List<object> pobject = new List<object>();
                                pobject.Add(pye);
                                pobject.Add(paralst[1]);
                                popupWindowCommand.Execute(pobject.ToArray());
                            }
                            else
                            {
                                PopupTypeEnum pye = (PopupTypeEnum)Enum.Parse(typeof(PopupTypeEnum), para);
                                popupWindowCommand.Execute(pye);
                            }
                        }
                    }
                }

                DesignHandler.Instance.OnSystemReady -= Instance_OnSystemReady;
            }));
        }
    }
}
