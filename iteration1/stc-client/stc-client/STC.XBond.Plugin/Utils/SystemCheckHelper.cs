using com.gateway.log;
using Newtonsoft.Json;
using STC.Common.Data;
using STC.Common.Handler.Admin;
using STC.Common.Model.Admin;
using STC.Common.Utils;
using STC.XBond.Plugin.View.Setting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace STC.XBond.Plugin.Utils
{
    public class SystemCheckHelper
    {
        private Thread _thread = null;
        private string _clientAutoExitTime = "";

        private static SystemCheckHelper designHandler = new SystemCheckHelper();

        public static SystemCheckHelper Instance { get { return designHandler; } }


        private SystemCheckHelper()
        {
            
        }

        public void Init()
        {
            DesignHandler.Instance.OnSystemReady += Instance_OnSystemReady;
        }

        private void Instance_OnSystemReady(object obj)
        {
            //StartCheckSystemStatus();
        }

        public void StartCheckSystemStatus()
        {
            TSafeMangerReq req = new TSafeMangerReq();
            req.action = "3";

            ThreadPoolHelper.InvokeThreadPool((o) =>
            {

                TSafeManger safeManger = AdminHandler.Instance.safeMangerService(req);
                if (safeManger != null && safeManger.Datas != null && safeManger.Datas.tbookSystemParameterList != null
                    && safeManger.Datas.tbookSystemParameterList.Count > 0 && safeManger.Datas.overPwdValidTime == "1")
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        UpdateUserPwdView _updateUserPwdView = new UpdateUserPwdView();

                        TTbookSystemParameter tbookSystemMessage = safeManger.Datas.tbookSystemParameterList[0];
                        if (tbookSystemMessage.param_value != null)
                        {
                            TSafeMangerReq safeMangerReq = JsonConvert.DeserializeObject<TSafeMangerReq>(tbookSystemMessage.param_value);

                            if (safeMangerReq.powerPwd == "1" || !string.IsNullOrEmpty(safeMangerReq.updatePwdTime) || !string.IsNullOrEmpty(safeMangerReq.stopPwdTimes))
                            {
                                _updateUserPwdView.SetDefaultUpdateDay(safeMangerReq.updatePwdTime, safeMangerReq.powerPwd == "1"?true:false,true);
                            }
                        }

                        _updateUserPwdView.ShowDialog();
                    });
                }

                if (safeManger != null && safeManger.Datas != null)
                {
                    _clientAutoExitTime = safeManger.Datas.clientAutoExitTime;

                    this.StartThread();
                }

            });
        }

        private void StartThread()
        {
            _thread = new Thread(CheckClientStatus);
            _thread.IsBackground = true;
            _thread.Start();
        }

        private void CheckClientStatus()
        {
            while (true)
            {
                try
                {
                    string currentTime = DateTime.Now.ToString("HH:mm:ss");
                    //_clientAutoExitTime = currentTime;
                    if (!string.IsNullOrEmpty(_clientAutoExitTime) && currentTime == _clientAutoExitTime)
                    {
                        DesignHandler.Instance.SystemExit(true);

                        break;
                    }

                    Thread.Sleep(2000);
                }
                catch (Exception e)
                { }
            }
        }
        
    }
}
