using BDB.Trade.Plugin.Command;
using com.gateway.log;
using STC.Common.Data;
using STC.Common.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace BDB.Price.Plugin.Utils
{
    public class BDBWinHandler
    {
        private static BDBWinHandler _bdbWinHandler = new BDBWinHandler();

        public static BDBWinHandler Instance { get { return _bdbWinHandler; } }

        public void RegSystemReadyEvent()
        {
            DesignHandler.Instance.OnSystemReady += Instance_OnSystemReady;
        }

        private void Instance_OnSystemReady(object obj)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                PopupWindowCommand popupWindowCommand = new PopupWindowCommand();
                //加载打开的窗口
                string lstkeysw = IniFileHelper.ReadValue($"BDB.Price.Plugin-{LocalEnv.UserInfo.UserName}", "Keys");
                string[] keyListw = lstkeysw.Split(',');
                for (int i = 0; i < keyListw.Length; i++)
                {
                    string winid = keyListw[i];
                    if (!string.IsNullOrEmpty(winid))
                    {
                        string para = IniFileHelper.ReadValue($"BDB.Price.Plugin_{LocalEnv.UserInfo.UserName}_{winid}", "CommandParameter");
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
