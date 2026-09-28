using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Management;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Drawing.Text;
using Microsoft.Win32;
using System.Collections;
using com.gateway.log;
using System.Linq;
using System.Runtime.InteropServices;
using System.Drawing;

namespace STC.Common.Utils
{
    /// <summary>
    /// 机器信息处理器
    /// </summary>
    public class MachineHelper
    {
        public static void sysInfo()
        {
            StringBuilder sb = new StringBuilder();
            try
            {
                //Win32Helper.MEMORY_INFO memInfo = new Win32Helper.MEMORY_INFO();
                //Win32Helper.GlobalMemoryStatus(ref memInfo);
                sb.Append(string.Format("FrameWork版本号:{0}[{1}]", Environment.Version, MachineHelper.GetNetFramework()));
                sb.Append(string.Format(",操作系统版本号:{0},{1}", MachineHelper.GetOperatingSystem(), MachineHelper.GetSystemType()));
                sb.Append(string.Format(",Cpu个数:{0}个,主频:{1}", Environment.ProcessorCount, MachineHelper.GetCpuSpeed()));
                sb.Append(string.Format(",是否是64位处理器:{0},操作系统:{1}位", MachineHelper.Is64BitProcess(), MachineHelper.GetOSBit()));
                sb.Append(string.Format(",是否是管理员权限:{0}", MachineHelper.IsAdministrator()));
                sb.Append(string.Format(",物理内存:{0}GB", MachineHelper.GetTotalPhysicalMemory()));
                //sb.Append(string.Format(",交换文件总大小:{0}GB", Win32Helper.ConvertBytes(memInfo.dwTotalPageFile.ToString(), 3)));
                sb.Append(string.Format(",分辨率:{0}", MachineHelper.GetResolution()));
                sb.Append(string.Format(",Dpi:{0}", MachineHelper.GetDpi()));

                sb.Append(string.Format(",微软雅黑:{0}", MachineHelper.GetFont()));
                sb.AppendLine(string.Format("\t计算机名称:{0}", Environment.MachineName));
                sb.AppendLine(string.Format("\t当前登录用户名:{0}", Environment.UserName));
                sb.AppendLine(string.Format("\t当前服务器时间:{0}", Common.Utils.DateTimeEx.ServerTime.ToString("yyyy-MM-dd HH:mm:ss.fff")));


            }
            catch (Exception e)
            {
                sb.AppendLine("\t" + e.StackTrace);
            }
            LogHelper.Info(sb.ToString());
        }

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

        #region HightDpi

        public static bool IsOpenHeightDpi()
        {
            string processName = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
            bool isOpenHeightDpi = false;
            try
            {
                RegistryKey registry = Registry.CurrentUser;
                RegistryKey softwareKey = registry.OpenSubKey(@"SOFTWARE");
                if (softwareKey != null)
                {
                    RegistryKey microsoftKey = softwareKey.OpenSubKey(@"Microsoft");
                    if (microsoftKey != null)
                    {
                        RegistryKey windowsNtKey = microsoftKey.OpenSubKey(@"Windows NT");
                        if (windowsNtKey != null)
                        {
                            RegistryKey urrentVersionNtKey = windowsNtKey.OpenSubKey(@"CurrentVersion");
                            if (urrentVersionNtKey != null)
                            {
                                RegistryKey appCompatFlagsNtKey = urrentVersionNtKey.OpenSubKey(@"AppCompatFlags");
                                if (appCompatFlagsNtKey != null)
                                {
                                    RegistryKey layersKey = appCompatFlagsNtKey.OpenSubKey(@"Layers");

                                    if (layersKey != null)
                                    {
                                        foreach (string item in layersKey.GetValueNames())
                                        {
                                            if (processName == item)
                                            {
                                                isOpenHeightDpi = true;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch
            {
                isOpenHeightDpi = false;
            }
            return isOpenHeightDpi;
        }


        public static bool SetHeightDpi()
        {
            string processName = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
            bool isOpenHeightDpi = false;
            try
            {
                RegistryKey registry = Registry.CurrentUser;
                RegistryKey softwareKey = registry.OpenSubKey(@"SOFTWARE", true);
                if (softwareKey != null)
                {
                    RegistryKey microsoftKey = softwareKey.OpenSubKey(@"Microsoft", true);
                    if (microsoftKey != null)
                    {
                        RegistryKey windowsNtKey = microsoftKey.OpenSubKey(@"Windows NT", true);
                        if (windowsNtKey != null)
                        {
                            RegistryKey urrentVersionNtKey = windowsNtKey.OpenSubKey(@"CurrentVersion", true);
                            if (urrentVersionNtKey != null)
                            {
                                RegistryKey appCompatFlagsNtKey = urrentVersionNtKey.OpenSubKey(@"AppCompatFlags", true);
                                if (appCompatFlagsNtKey != null)
                                {
                                    RegistryKey layersKey = appCompatFlagsNtKey.OpenSubKey(@"Layers", true);

                                    if (layersKey != null)
                                    {
                                        layersKey.SetValue(processName, @"~ PERPROCESSSYSTEMDPIFORCEOFF HIGHDPIAWARE");
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                isOpenHeightDpi = false;
            }
            return isOpenHeightDpi;
        }


        /// <summary>
        /// 删除就的DPI设置
        /// </summary>
        /// <returns></returns>
        public static bool ReMoveHeightDpiOldSet()
        {
            string processName = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
            string rootkey = @"SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\AppCompatFlags\\Layers";
            try
            {
                return FindAndRemoveSubKey(Registry.CurrentUser, rootkey, processName);
            }
            catch (Exception ex)
            {
                LogHelper.Error($"操作注册表时发生错误：{ex.Message}");
                return false;
            }

            return false;
        }


        /// <summary>
        /// 检查指定的注册表 Key 的子键中是否包含指定的子键名，如果包含则删除。
        /// </summary>
        /// <param name="hive">注册表根键 (例如：Registry.LocalMachine, Registry.CurrentUser)。</param>
        /// <param name="parentKeyPath">父键的路径 (例如：@"SOFTWARE\MyApplication")。</param>
        /// <param name="subKeyNameToFindAndRemove">要查找并删除的子键名。</param>
        /// <param name="deleteSubkeys">是否递归删除子键。如果为 true，则删除 Key 及其所有子 Key；如果为 false，则只删除 Key 本身，如果 Key 有子 Key 则抛出异常。</param>
        /// <returns>如果找到并删除子键则返回 true，如果未找到则返回 false，发生错误也返回 false。</returns>
        public static bool FindAndRemoveSubKey(RegistryKey hive, string parentKeyPath, string subKeyNameToFindAndRemove)
        {
            try
            {
                using (RegistryKey parentKey = hive.OpenSubKey(parentKeyPath, true)) // 以可写方式打开父键
                {
                    if (parentKey == null)
                    {
                        return false;
                    }

                    string[] subKeyNames = parentKey.GetValueNames();
                    if (subKeyNames != null)
                    {
                        if (subKeyNames.Contains(subKeyNameToFindAndRemove)) // 检查值是否存在
                        {
                            parentKey.DeleteValue(subKeyNameToFindAndRemove);
                            LogHelper.Info($"删除注册表值: {parentKeyPath} 成功！");
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error($"操作注册表时发生错误：{ex.Message}");
                return false;
            }

            return false;
        }


        #endregion

        #region DPI Helper

        [DllImport("user32.dll")]
        private static extern IntPtr GetDC(IntPtr hWnd);

        [DllImport("gdi32.dll")]
        private static extern int GetDeviceCaps(IntPtr hdc, int nIndex);

        [DllImport("user32.dll")]
        private static extern int ReleaseDC(IntPtr hWnd, IntPtr hdc);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("kernel32.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);

        private const int LOGPIXELSX = 88;
        private const int LOGPIXELSY = 90;
        private const double DEFAULT_DPI = 96.0;

        // .NET 4.0 不能直接调用 GetDpiForWindow，需要通过委托调用
        private delegate int GetDpiForWindowDelegate(IntPtr hWnd);
        private static GetDpiForWindowDelegate _getDpiForWindow;
        
        public static System.Windows.Media.Matrix GetDpiMatrix(string scrname)
        {
            IntPtr user32 = GetModuleHandle("user32.dll");
            IntPtr procAddress = GetProcAddress(user32, "GetDpiForWindow");

            if (procAddress != IntPtr.Zero)
            {
                _getDpiForWindow = (GetDpiForWindowDelegate)Marshal.GetDelegateForFunctionPointer(procAddress, typeof(GetDpiForWindowDelegate));
            }

            if (string.IsNullOrEmpty(scrname)) return System.Windows.Media.Matrix.Identity;

            IntPtr hwnd = new System.Windows.Interop.WindowInteropHelper(System.Windows.Application.Current.MainWindow).Handle;
            System.Windows.Window tempWindow = null;
            foreach (System.Windows.Forms.Screen sc in System.Windows.Forms.Screen.AllScreens)
            {
                if (scrname == sc.DeviceName.Replace("\\", "").Replace(".", ""))
                {
                    // 获取当前屏幕的工作区域和 DPI
                    System.Drawing.Rectangle screenBounds = sc.WorkingArea;
                    // 创建一个临时的 WPF 窗口（不需要显示）
                    tempWindow = new System.Windows.Window
                    {
                        WindowStartupLocation = System.Windows.WindowStartupLocation.Manual,
                        Left = sc.WorkingArea.Left, // 放置在目标屏幕上
                        Top = sc.WorkingArea.Top,
                        Width = 1, // 尽量小，避免影响用户体验
                        Height = 1,
                        ShowInTaskbar = false, // 不在任务栏显示
                        WindowStyle = System.Windows.WindowStyle.None // 没有边框
                    };

                    tempWindow.Show(); // 必须 Show 才能获取 PresentationSource
                    hwnd = new System.Windows.Interop.WindowInteropHelper(tempWindow).Handle;
                    break;
                }
            }
            
            if (_getDpiForWindow != null) // 如果 GetDpiForWindow 可用，直接使用
            {
                int dpiX = _getDpiForWindow(hwnd);
                double scaleX = dpiX / DEFAULT_DPI;
                tempWindow.Close();
                return new System.Windows.Media.Matrix(scaleX, 0, 0, scaleX, 0, 0);
            }
            else // 兼容 Windows 7/8
            {
                IntPtr hdc = GetDC(IntPtr.Zero);
                if (hdc != IntPtr.Zero)
                {
                    int dpiX = GetDeviceCaps(hdc, LOGPIXELSX);
                    int dpiY = GetDeviceCaps(hdc, LOGPIXELSY);
                    ReleaseDC(IntPtr.Zero, hdc);

                    double scaleX = dpiX / DEFAULT_DPI;
                    double scaleY = dpiY / DEFAULT_DPI;

                    return new System.Windows.Media.Matrix(scaleX, 0, 0, scaleY, 0, 0);
                }
            }

            return System.Windows.Media.Matrix.Identity;
        }

        #endregion

    }
}
