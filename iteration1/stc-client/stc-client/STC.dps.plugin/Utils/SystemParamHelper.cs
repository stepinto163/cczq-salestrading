using com.gateway.log;
using Newtonsoft.Json;
using STC.Common.Handler.SystemParam;
using STC.dps.plugin.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.dps.plugin.Utils
{
    public class SystemParamHelper
    {
        public const string STCExcel = "STCExcel";

        public static bool isSimulate(string terminal)
        {
            bool result = false;
            
            string info1 = SystemParamHandler.Instance.GetUserSetting().inf1;
            LogHelper.Info(string.Format("Check terminal:{0}, info1:{1}", terminal, info1));
            if (!string.IsNullOrEmpty(info1) && !string.IsNullOrEmpty(terminal))
            {
                List<SimulateSwitchPo> simulateSwitchList = JsonConvert.DeserializeObject<List<SimulateSwitchPo>>(info1);
                if (simulateSwitchList != null && simulateSwitchList.Count > 0)
                {
                    foreach (SimulateSwitchPo ssp in simulateSwitchList)
                    {
                        if (!string.IsNullOrEmpty(ssp.terminal) && terminal.Contains(ssp.terminal) && ssp.confirmflag == "1")
                        {
                            result = true;
                            break;
                        }
                    }
                }
                
            }

            return result;
        }
    }
}
