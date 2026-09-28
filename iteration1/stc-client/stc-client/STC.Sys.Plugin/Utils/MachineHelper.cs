using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Management;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Drawing.Text;
using System.Drawing;
using Microsoft.Win32;
using System.Collections;
using com.gateway.log;

namespace STC.Sys.Plugin.Utils
{
    /// <summary>
    /// 机器信息处理器
    /// </summary>
    public class MachineHelper
    {
        private static Mutex mutex = null;

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

        public static bool Is64BitProcess()
        {

            return IntPtr.Size == 8;

        }

        public static int GetOSBit()
        {

            try
            {
                string addressWidth = String.Empty;
                ConnectionOptions mConnOption = new ConnectionOptions();
                ManagementScope mMs = new ManagementScope(@"\\localhost", mConnOption);
                ObjectQuery mQuery = new ObjectQuery("select AddressWidth from Win32_Processor");
                ManagementObjectSearcher mSearcher = new ManagementObjectSearcher(mMs, mQuery);
                ManagementObjectCollection mObjectCollection = mSearcher.Get();
                foreach (ManagementObject mObject in mObjectCollection)
                {
                    addressWidth = mObject["AddressWidth"].ToString();
                }
                return Int32.Parse(addressWidth);
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
                return 32;
            }
        }

        public static bool IsAdministrator()
        {
            try
            {
                WindowsIdentity identity = WindowsIdentity.GetCurrent();
                WindowsPrincipal principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
            catch (Exception e)
            {
            }
            return false;
        }

        public static string GetCpuSpeed()
        {
            string str = "";
            try
            {
                ManagementObjectSearcher MySearcher = new ManagementObjectSearcher("SELECT * FROM Win32_Processor");
                foreach (ManagementObject MyObject in MySearcher.Get())
                {
                    str = MyObject.Properties["CurrentClockSpeed"].Value.ToString();
                }
            }
            catch (Exception e)
            {
            }
            return str+ "MHz";
        }


        /// <summary>
        /// 获取安装目录空间大小
        /// </summary>
        /// <returns>double</returns>
        public static double GetInstallationDirectory(String local)
        {
            try
            {
                ManagementClass diskClass = new ManagementClass("Win32_LogicalDisk");
                ManagementObjectCollection disks = diskClass.GetInstances();
                ulong space = 0;
                foreach (ManagementObject disk in disks)
                {
                    if (disk["Name"].ToString() == local)
                    {
                        space = (ulong)disk["FreeSpace"];
                    }
                }
                return (space / 1024 / 1024 / 1024);
            }
            catch (Exception e) { }
            return 0;
        }
       public static string GetSystemType()
        {
            try
            {
                string st = "";
                ManagementClass mc = new ManagementClass("Win32_ComputerSystem");
                ManagementObjectCollection moc = mc.GetInstances();
                foreach (ManagementObject mo in moc)
                {
                    st = mo["SystemType"].ToString();
                }
                moc = null;
                mc = null;
                return st;
            }
            catch
            {
                return "unknow";
            }

        }
      public static  string GetTotalPhysicalMemory()
        {
            try
            {

                string st = "";
                ManagementClass mc = new ManagementClass("Win32_ComputerSystem");
                ManagementObjectCollection moc = mc.GetInstances();
                foreach (ManagementObject mo in moc)
                {
                    st = mo["TotalPhysicalMemory"].ToString();
                    decimal d =decimal.Parse(st)/1000/ 1000 / 1000;
                    st = d.ToString("N0");
                }
                moc = null;
                mc = null;
                return st;
            }
            catch
            {
                return "unknow";
            }
        }
 
        /// <summary>
        /// 获取分辨率
        /// </summary>
        /// <returns>string</returns>
        public static string GetResolution()
        {
            Rectangle rect = new Rectangle();
            rect = System.Windows.Forms.Screen.PrimaryScreen.Bounds;
            //rect.Width;//屏幕宽
            //rect.Height;//屏幕高
            return rect.Width + "*" + rect.Height;

        }

        /// <summary>
        /// 获取系统颜色
        /// </summary>
        /// <returns>string</returns>
        public static string GetSystemColor()
        {
            return null;
        }

        /// <summary>
        /// 获取操作系统
        /// </summary>
        /// <returns>string</returns>
        public static string GetOperatingSystem()
        {
            //return Environment.OSVersion.ToString();
            string s = Environment.OSVersion.ToString();
            try
            {
                RegistryKey rk;
                rk = Registry.LocalMachine.OpenSubKey("Software\\Microsoft\\Windows NT\\CurrentVersion");
                s = rk.GetValue("ProductName").ToString();
            }
            catch (Exception ex)
            {

                LogHelper.Error(ex);
            }

            return s;

        }



        /// <summary>
        /// 获取浏览器（IE6以上）
        /// </summary>
        /// <returns>string</returns>
        public static string GetBrowser()
        {
            RegistryKey rk = Registry.LocalMachine.OpenSubKey("Software\\Microsoft\\Internet Explorer");
            return rk == null ? "" : rk.GetValue("svcVersion") + "";
        }

        /// <summary>
        /// 获取字体大小
        /// </summary>
        /// <returns>string</returns>
        public static string GetFontSize()
        {
            return null;
        }

        /// <summary>
        /// 获取http1.1
        /// </summary>
        /// <returns>string</returns>
        public static string GetHttp()
        {
            return null;
        }

        /// <summary>
        /// 获取语言（中文）
        /// </summary>
        /// <returns>bool</returns>
        public static bool GetLanguage()
        {
            return System.Threading.Thread.CurrentThread.CurrentCulture.Name == "zh-CN";
        }

        /// <summary>
        /// 获取字体（微软雅黑）
        /// </summary>
        /// <returns>bool</returns>
        public static bool GetFont()
        {
            InstalledFontCollection MyFont = new InstalledFontCollection();
            FontFamily[] MyFontFamilies = MyFont.Families;
            ArrayList list = new ArrayList();
            int Count = MyFontFamilies.Length;
            for (int i = 0; i < Count; i++)
            {
                string FontName = MyFontFamilies[i].Name;
                list.Add(FontName);
            }
            return list.Contains("微软雅黑");
        }

        public static bool text()
        {
            return false;
        }

        public static Point GetDpi()
        {
            Point p = new Point();
            try
            {
                using (ManagementClass mc = new ManagementClass("Win32_DesktopMonitor"))
                {
                    using (ManagementObjectCollection moc = mc.GetInstances())
                    {

                        int PixelsPerXLogicalInch = 0; // dpi for x
                        int PixelsPerYLogicalInch = 0; // dpi for y

                        foreach (ManagementObject each in moc)
                        {
                            PixelsPerXLogicalInch = int.Parse((each.Properties["PixelsPerXLogicalInch"].Value.ToString()));
                            PixelsPerYLogicalInch = int.Parse((each.Properties["PixelsPerYLogicalInch"].Value.ToString()));
                        }
                        p.X = PixelsPerXLogicalInch;
                        p.Y = PixelsPerYLogicalInch;
                    }
                }
            }
            catch (Exception e)
            {
            }
            return p;
        }

        public static double UsingPerfmon(string pname)
        {
            double d;


            using (var p1 = new PerformanceCounter("Process", "% Processor Time", pname))
            {
                d = p1.NextValue() / Environment.ProcessorCount;
            }
            return d;

        }
        /// <summary>
        /// 获取本机最新.netframework版本
        /// </summary>
        /// <returns>string</returns>
        public static string GetNetFramework()
        {

            int i = 0;
            StringBuilder sb = new StringBuilder();
            try
            {
                using (RegistryKey ndpKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\NET Framework Setup\NDP\"))
                {
                    foreach (string versionKeyName in ndpKey.GetSubKeyNames())
                    {
                        if (versionKeyName.StartsWith("v"))
                        {

                            RegistryKey versionKey = ndpKey.OpenSubKey(versionKeyName);
                            string name = (string)versionKey.GetValue("Version", "");
                            string sp = versionKey.GetValue("SP", "").ToString();
                            string install = versionKey.GetValue("Install", "").ToString();
                            if (install == "") //no install info, must be later.
                                Console.WriteLine(versionKeyName + "  " + name);
                            else
                            {
                                if (sp != "" && install == "1")
                                {
                                    sb.Append(versionKeyName + "  " + name + "  SP" + sp + " ; ");
                                    if ((versionKeyName.Contains("v2.0") && sp != "2") || (versionKeyName.Contains("v3.0") && sp != "2") || (versionKeyName.Contains("v3.5") && sp != "1"))
                                    {
                                        i = -1;
                                    }
                                    else if ((versionKeyName.Contains("v2.0") && sp == "2") || (versionKeyName.Contains("v3.0") && sp == "2") || (versionKeyName.Contains("v3.5") && sp == "1"))
                                    {
                                        if (i != -1)
                                        {
                                            i++;
                                        }
                                    }
                                }

                            }
                            if (name != "")
                            {
                                continue;
                            }
                        }
                    }
                }

                try
                {
                    using (
                        RegistryKey ndpKey =
                            Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full\"))
                    {
                        int releaseKey = (int)ndpKey.GetValue("Release");
                        {
                            if (releaseKey == 378389)

                                sb.Append("v4.5 " + " ; ");

                            if (releaseKey == 378758 || releaseKey == 378675)

                                sb.Append("v4.5.1" + " ; ");

                            if (releaseKey == 379893)

                                sb.Append("v4.5.2" + " ; ");

                            if (releaseKey == 393295 || releaseKey == 393297)

                                sb.Append("v4.6" + " ; ");

                            if (releaseKey == 394254 || releaseKey == 394271)

                                sb.Append("v4.6.1" + " ; ");

                            if (releaseKey == 394802 || releaseKey == 394806)

                                sb.Append("v4.6.2" + " ; ");

                            if (releaseKey == 460798 || releaseKey == 460805)

                                sb.Append("v4.7" + " ; ");

                            if (releaseKey == 461308 || releaseKey == 461310)

                                sb.Append("v4.7.1" + " ; ");

                            if (releaseKey == 461808 || releaseKey == 461814)

                                sb.Append("v4.7.2" + " ; ");

                            if (releaseKey >= 528040)

                                sb.Append("4.8 or later" + " ; ");

                        }
                    }
                }
                catch (Exception e)
                {
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex.Message);
            }

            return sb.ToString();
        }
    }
}
