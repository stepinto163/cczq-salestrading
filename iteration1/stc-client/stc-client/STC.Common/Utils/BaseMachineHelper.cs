using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace STC.Common.Utils
{
    public class BaseMachineHelper
    {
        private static Mutex mutex = null;

        public static EventWaitHandle _appEvent = null;


        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern uint SetThreadExecutionState(uint esFlags);

        private const uint ES_CONTINUOUS = 0x80000000;   // 持续应用
        private const uint ES_SYSTEM_REQUIRED = 0x00000001; // 防止系统休眠
        private const uint ES_DISPLAY_REQUIRED = 0x00000002; // 防止显示器关闭
        private const uint ES_AWAYMODE_REQUIRED = 0x00000040; // 防止系统进入睡眠模式

        /// <summary>
        /// 检查是否启动了多个进程
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static bool CheckProcessIsMultiple(string name)
        {
            bool newMutexCreated = false;
            try
            {
                string mutexName = "Global\\" + name;
                mutex = new Mutex(false, mutexName, out newMutexCreated);
            }
            catch
            {

            }
            return !newMutexCreated;
        }

        /// <summary>
        /// 防止系统进入睡眠模式
        /// </summary>
        public static void PreventSleep()
        {
            SetThreadExecutionState(ES_CONTINUOUS | ES_SYSTEM_REQUIRED | ES_DISPLAY_REQUIRED);
        }

        /// <summary>
        /// 允许系统进入正常的睡眠模式（恢复默认）
        /// </summary>
        public static void AllowSleep()
        {
            SetThreadExecutionState(ES_CONTINUOUS);
        }


        #region 虚拟机检测
        /// <summary>
        /// 检测当前系统是否运行在虚拟机中
        /// </summary>
        /// <returns>如果是虚拟机返回true，否则返回false</returns>
        public static bool IsVirtualMachine()
        {
            try
            {
                // 只检测最关键的WMI信息，这是最可靠的方法
                return CheckWMI();
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 通过WMI检测虚拟机（最核心的检测方法）
        /// </summary>
        private static bool CheckWMI()
        {
            try
            {
                // 检查计算机系统制造商和型号 - 这是最有效的单一检测方法
                using (var searcher = new ManagementObjectSearcher("SELECT Manufacturer, Model FROM Win32_ComputerSystem"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        string manufacturer = GetStringValue(obj["Manufacturer"]).ToLower();
                        string model = GetStringValue(obj["Model"]).ToLower();

                        // 检查常见虚拟机厂商标识
                        if (manufacturer.Contains("vmware") || manufacturer.Contains("oracle") ||
                            manufacturer.Contains("virtualbox") || manufacturer.Contains("parallels") ||
                            manufacturer.Contains("microsoft corporation") ||
                            model.Contains("virtualbox") || model.Contains("vmware") ||
                            model.Contains("virtual"))
                        {
                            return true;
                        }
                    }
                }

                // 辅助检查：BIOS信息
                using (var searcher = new ManagementObjectSearcher("SELECT Version, SerialNumber FROM Win32_BIOS"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        string version = GetStringValue(obj["Version"]).ToLower();
                        string serialNumber = GetStringValue(obj["SerialNumber"]).ToLower();

                        if (version.Contains("vbox") || version.Contains("vmware") ||
                            serialNumber.Contains("vmware") || serialNumber.Contains("vbox") ||
                            serialNumber.Contains("0"))  // 很多虚拟机BIOS序列号为0
                        {
                            return true;
                        }
                    }
                }
            }
            catch
            {
                // WMI查询失败
            }

            return false;
        }

        /// <summary>
        /// 安全获取字符串值的辅助方法
        /// </summary>
        private static string GetStringValue(object value)
        {
            return value != null ? value.ToString() : string.Empty;

        }
        #endregion

    }
}
