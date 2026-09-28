using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.dps.plugin.Common
{
    public class DpsLocalEnv
    {
        public static ConfInfo ConfInfo = new ConfInfo();
        public static void Load()
        {
            ConfInfo.IsOpenQM = System.Configuration.ConfigurationManager.AppSettings["IsOpenQM"] + "";
        }
    }
    public class ConfInfo
    {
        //是否开启qm
        public static string IsOpenQM;

    }
}
