using STC.Common.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Utils
{
    public class AppUtils
    {
        public static IForm Application = null;

        public static bool IsInnerInput = false;

        public static bool AppClosing = false;

        public static string SaveNodeName = "ShowInfo";

        public static string GetCacheKey()
        {
            return string.Format("{0}-{1}", AppUtils.SaveNodeName, LocalEnv.UserInfo.UserName);
        }

        public static string GetCacheKey(string winName)
        {
            return string.Format("{0}-{1}-{2}", AppUtils.SaveNodeName, LocalEnv.UserInfo.UserName, winName);
        }
    }
}
