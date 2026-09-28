using STC.Common.Model;
using STC.Common.Utils;
using STC.Common.View.UI;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Configuration;
using System.Globalization;
using Microsoft.Win32;
using STC.Excel.Plugin.Utils;
using STC.Common.Data;
using STC.Common.Model.MM;

namespace STC.Excel.Plugin.ViewModel.Setting
{
    public class ExcelViewModel : BaseViewModel
    {
        [DllImport("kernel32.dll", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool IsWow64Process([In] IntPtr process, [Out] out bool wow64Process);
        private string inipath = AppDomain.CurrentDomain.BaseDirectory + @"excelAddin";
        private string iniConfigpath = AppDomain.CurrentDomain.BaseDirectory + @"excelAddin";
        private string inInstallPath= AppDomain.CurrentDomain.BaseDirectory + @"excelInstall";
        private string xllbit = string.Empty;
        public ICommand InstallExcelPluginCommand { get { return new ComRelayCommand(OnInstallExcelPluginClick); } }
        public ICommand UnInstallExcelPluginCommand { get { return new ComRelayCommand(OnUnInstallExcelPluginClick); } }

        private string _excelpluginversion = "";
        private string _excelpluginButtonText = "安装";
        public string ExcelPlugInVersion
        {
            get
            {
                return _excelpluginversion;
            }

            set
            {
                _excelpluginversion = value;
                this.OnPropertyChanged(nameof(ExcelPlugInVersion));
            }
        }


        public string ExcelPlugInButtonText
        {
            get
            {
                return _excelpluginButtonText;
            }

            set
            {
                _excelpluginButtonText = value;
                this.OnPropertyChanged(nameof(ExcelPlugInButtonText));
            }
        }


        private bool isUninstallButtonEnable = false;
        public bool IsUninstallButtonEnable
        {
            get
            {
                return isUninstallButtonEnable;
            }

            set
            {
                isUninstallButtonEnable = value;
                this.OnPropertyChanged(nameof(IsUninstallButtonEnable));
            }
        }

        /// <summary>
        /// 是否有升级版本，如果有设为true，则自动在升级按钮后显示 小红点，同时可升级和提醒图标显示出来
        /// </summary>
        private bool isUpgradeRedVisible = false;
        public bool IsUpgradeRedVisible
        {
            get
            {
                return isUpgradeRedVisible;
            }

            set
            {
                isUpgradeRedVisible = value;
                this.OnPropertyChanged(nameof(IsUpgradeRedVisible));
            }
        }

        public void Init()
        {

            
            InitVersion();
            /*string loginEnv = ConfigurationManager.AppSettings["LoginEnv"].ToString();

            if (loginEnv.Equals("DEV")) {
                dstFileName = "STCExcel-Dev.xll";
            }*/

            if (!Directory.Exists(inInstallPath))
            {
                Directory.CreateDirectory(inInstallPath);

            }

        }

        /// <summary>
        /// 获取当前插件的版本
        /// </summary>
        /// <returns></returns>
        private string GetExcelPlugInVer()
        {
            // 获取ExcelPlugIn 版本
            string ver = "";
            IsUninstallButtonEnable = checkRegisterAddin() ? true : false;
            if (IsUninstallButtonEnable)
            {
                ver = GetExcelPlugInVersion();
            }

            return ver;
        }

        /// <summary>
        /// 是否需要更新插件，用于判断是否需要显示小红点
        /// </summary>
        /// <param name="ver"></param>
        /// <returns></returns>
        public bool IsNeedUpgrade(string ver = "")
        {
            if (string.IsNullOrEmpty(ver))
                ver= GetExcelPlugInVer();

            if (!string.IsNullOrEmpty(ver) && ver.Contains("可更新"))
            {
                ///设置升级按钮上显示小红点
                IsUpgradeRedVisible = true;
            }
            else
            {
                ///取消升级按钮上显示小红点
                IsUpgradeRedVisible = false;
            }

            return IsUpgradeRedVisible;
        }

        private void InitVersion()
        {
            // 获取ExcelPlugIn 版本
            string ver = GetExcelPlugInVer();
            //IsUninstallButtonEnable = checkRegisterAddin() ? true : false;
            //if (IsUninstallButtonEnable) 
            //{
            //  ver = GetExcelPlugInVersion();
            //}

            ExcelPlugInVersion = string.IsNullOrEmpty(ver) ? "暂未安装" : $"已安装      {ver}";
            if (IsNeedUpgrade(ver))
            {
                DesignHandler.Instance.SettingChange(new NewSettingMsg() { key = "ExcelPlugInShowRedSign", value = "1" });
                ExcelPlugInButtonText = "升级";
            }
            else 
            {
                DesignHandler.Instance.SettingChange(new NewSettingMsg() { key = "ExcelPlugInShowRedSign", value = "0" });
                ExcelPlugInButtonText = "安装";
            }
        }

        /// <summary>
        /// 获取Excel PlugIn 版本，如果有版本返回版本号 v1.6_1.0.0_20220222，无版本返回 "";
        /// </summary>
        /// <returns></returns>
        private string GetExcelPlugInVersion()
        {
            string newVersion = string.Empty;
            string oldVersion = string.Empty;
            //string newConfigPatch=string.Empty;
            string oldConfigPatch = string.Empty;
            string newXllPatch = string.Empty;
            string oldXllPatch = string.Empty;
            if ("STCExcel-AddIn-packed.xll".Equals(xllbit)) {
                //newConfigPatch = Path.Combine(inipath, "STCExcel-AddIn-packed.xll.config");
                oldConfigPatch = Path.Combine(inInstallPath, "STCExcel-AddIn-packed.xll.config");
                newXllPatch = Path.Combine(inipath, "STCExcel-AddIn-packed.xll");
                oldXllPatch = Path.Combine(inInstallPath, "STCExcel-AddIn-packed.xll");

            }
            if ("STCExcel-AddIn64-packed.xll".Equals(xllbit))
            {
                //newConfigPatch = Path.Combine(inipath, "STCExcel-AddIn64-packed.xll.config");
                oldConfigPatch = Path.Combine(inInstallPath, "STCExcel-AddIn64-packed.xll.config");
                newXllPatch = Path.Combine(inipath, "STCExcel-AddIn64-packed.xll");
                oldXllPatch = Path.Combine(inInstallPath, "STCExcel-AddIn64-packed.xll");
            }

            string oldMD5 = string.Empty;
            string newMD5 = string.Empty;
            if (File.Exists(oldXllPatch))
            {
                oldMD5 = MD5Utils.GetMD5HashFromFile(oldXllPatch);
            }

            if (File.Exists(newXllPatch))
            {
                newMD5 = MD5Utils.GetMD5HashFromFile(newXllPatch);
            }

            if (string.IsNullOrEmpty(oldMD5))
            {
                return string.Empty;
            }

            if (!string.IsNullOrEmpty(oldConfigPatch)) {
                //newVersion = getVersion("Version", newConfigPatch);
                oldVersion = getVersion("Version", oldConfigPatch);
            }

            if (newMD5.Equals(oldMD5))
            {
                return oldVersion;
            }
            else 
            {
                // 获取版本
                string ver = string.Format("当前版本:{0}  可更新!", oldVersion);
                return ver;
            }

        }



        public string getVersion(string key, string configName)
        {
            string versin = "";
            ExeConfigurationFileMap ecf = new ExeConfigurationFileMap();
            ecf.ExeConfigFilename = configName; ;
            Configuration config = ConfigurationManager.OpenMappedExeConfiguration(ecf, ConfigurationUserLevel.None);
            //AppSettingsSection appSettings = (AppSettingsSection)config.GetSection("appSettings");
            if (config != null) {
                if (config.AppSettings.Settings[key] != null)
                {
                    versin = config.AppSettings.Settings[key].Value;
                }
            }

            return versin;
        }





        /// <summary>
        /// 安装插件
        /// </summary>
        private void OnInstallExcelPluginClick()
        {
            System.Threading.Tasks.Task.Factory.StartNew(() =>
            {
                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    //string gatewayPath = Path.Combine(inipathGw, "gateway-api.dll");
                    //File.Copy(gatewayPath, Path.Combine(inipath, "gateway-api.dll"), true);

                    string configPatch1 = Path.Combine(inipath, "STCExcel-AddIn-packed.xll.config");
                    string xllPatch1 = Path.Combine(inipath, "STCExcel-AddIn-packed.xll");

                    string configPatch2 = Path.Combine(inipath, "STCExcel-AddIn64-packed.xll.config");
                    string xllPatch2 = Path.Combine(inipath, "STCExcel-AddIn64-packed.xll");

                    if (File.Exists(configPatch1) && File.Exists(xllPatch1))
                    { 
                        File.Copy(configPatch1, Path.Combine(inInstallPath, "STCExcel-AddIn-packed.xll" + ".config"), true);
                        File.Copy(xllPatch1, Path.Combine(inInstallPath, "STCExcel-AddIn-packed.xll"), true);
                    }

                    if (File.Exists(configPatch2) && File.Exists(xllPatch2))
                    {
                        File.Copy(configPatch2, Path.Combine(inInstallPath, "STCExcel-AddIn64-packed.xll" + ".config"), true);
                        File.Copy(xllPatch2, Path.Combine(inInstallPath, "STCExcel-AddIn64-packed.xll"), true);
                    }

                    InstallExcelUdfAddins();

                    InitVersion();
                    LogHandler.Info($"安装 Excel 插件成功!");
                }));
            });
        }

        /// <summary>
        /// 卸载插件
        /// </summary>
        private void OnUnInstallExcelPluginClick()
        {
            System.Threading.Tasks.Task.Factory.StartNew(() =>
            {
                try
                {
                    dynamic excelComObj = Marshal.GetActiveObject("Excel.Application");
                    showMessage("请关闭Excel程序并保存内容后再运行，防止数据丢失造成损失！！");
                }
                catch
                {
                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        bool flag = DelectDir();

                        if (flag)
                        {
                            showMessage("卸载 Excel 插件成功!");
                            xllbit = string.Empty;
                        }
                        else
                        {
                            showMessage("卸载 Excel 插件失败!");
                        }
                        KillExcel();
                        InitVersion();
                    }));
                }
            });

        }

        /// <summary>
        /// 提示消息弹框
        /// </summary>
        /// <param name="message"></param>
        private void showMessage(string message)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                DeleteBoxView me = new DeleteBoxView();
                me.tbTitle.Text = "提示";
                me.setWarnMsg(message);
                me.Owner = Application.Current.MainWindow;
                me.Show();
            }));
        }


        private void InstallExcelUdfAddins()
        {
            LogHandler.Info($"Excel 插件安装开始");
            dynamic excelComObj;
            
            try
            {
                excelComObj = Marshal.GetActiveObject("Excel.Application");
                showMessage("检测到当前有已打开的Excel程序\r\n请关闭Excel程序并保存内容后再运行，防止数据丢失造成损失！！");
                return;
            }
            catch
            {
            }

            bool flag = CaRegisterAddIn("STCExcel-AddIn-packed.xll", "STCExcel-AddIn64-packed.xll");
            if (flag)
            {
                showMessage("安装 Excel 插件成功!");
            }
            else
            {
                showMessage("安装 Excel 插件失败!");
            }
        }

        private bool DelectDir()
        {
            bool flag = true;
            try
            {
                //强制退出Excel，后进行卸载
                KillExcel();
                flag= CaUnRegisterAddIn("STCExcel-AddIn-packed.xll", "STCExcel-AddIn64-packed.xll");
            }
            catch (Exception e)
            {
                LogHandler.Error($"卸载 STCExcel插件失败! {e}");
                flag = false;
            }
            return flag;
        }


        private void KillExcel()
        {
            try
            {
                //强制退出Excel，后进行卸载
                foreach (var item in Process.GetProcesses())
                {
                    if (item.ProcessName.ToLower() == "excel")
                    {
                        item.Kill();
                    }
                }
            }
            catch { }
        }




        #region CaRegisterAddIn
        public bool CaRegisterAddIn(string szXll32Bit, string szXll64Bit)
        {
            string szOfficeRegKeyVersions = string.Empty;
            string szBaseAddInKey = @"Software\Microsoft\Office\";
            string szXllToRegister = string.Empty;
            string szFolder = string.Empty;
            int nOpenVersion;
            double nVersion;
            bool bFoundOffice = true;
            List<string> lstVersions;

            try
            {
                LogHandler.Info($"Enter try block of CaRegisterAddIn");

                szOfficeRegKeyVersions = "11.0,12.0,14.0,15.0,16.0";
                szXll32Bit = Path.Combine(inInstallPath, szXll32Bit.ToString()); //szFolder.ToString() + szXll32Bit.ToString();
                szXll64Bit = Path.Combine(inInstallPath, szXll64Bit.ToString()); //szFolder.ToString() + szXll64Bit.ToString();

                if (szOfficeRegKeyVersions.Length > 0)
                {
                    lstVersions = szOfficeRegKeyVersions.Split(',').ToList();

                    foreach (string szOfficeVersionKey in lstVersions)
                    {
                        nVersion = double.Parse(szOfficeVersionKey, NumberStyles.Any, CultureInfo.InvariantCulture);

                        LogHandler.Info($"Retrieving Registry Information for : " + szBaseAddInKey + szOfficeVersionKey);

                        // get the OPEN keys from the Software\Microsoft\Office\[Version]\Excel\Options key, skip if office version not found.
                        if (Registry.CurrentUser.OpenSubKey(szBaseAddInKey + szOfficeVersionKey, false) != null)
                        {
                            string szKeyName = szBaseAddInKey + szOfficeVersionKey + @"\Excel\Options";


                            szXllToRegister = GetAddInName(szXll32Bit, szXll64Bit, szOfficeVersionKey, nVersion);

                            RegistryKey rkExcelXll = Registry.CurrentUser.OpenSubKey(szKeyName, true);

                            if (szXllToRegister != string.Empty && rkExcelXll != null)
                            {
                                
                                string[] szValueNames = rkExcelXll.GetValueNames();
                                bool bIsOpen = false;
                                int nMaxOpen = -1;
                                string valueName = "OPEN";

                                foreach (string szValueName in szValueNames)
                                {
                                    
                                    if (szValueName.StartsWith("OPEN"))
                                    {
                                        nOpenVersion = int.TryParse(szValueName.Substring(4), NumberStyles.Any, CultureInfo.InvariantCulture, out nOpenVersion) ? nOpenVersion : 0;
                                        int nNewOpen = szValueName == "OPEN" ? 0 : nOpenVersion;
                                        if (nNewOpen > nMaxOpen)
                                        {
                                            nMaxOpen = nNewOpen;
                                        }

                                        
                                        if (rkExcelXll.GetValue(szValueName).ToString().Contains("STCExcel"))
                                        {
                                            valueName = szValueName;
                                            bIsOpen = true;
                                        }
                                    }
                                }


                                if (!bIsOpen)
                                {
                                    if (nMaxOpen == -1)
                                    {
                                        rkExcelXll.SetValue("OPEN", "/R \"" + szXllToRegister + "\"");
                                    }
                                    else
                                    {
                                        rkExcelXll.SetValue("OPEN" + (nMaxOpen + 1).ToString(), "/R \"" + szXllToRegister + "\"");
                                    }
                                    rkExcelXll.Close();
                                    LogHandler.Info($" OfficeVersion:{szOfficeVersionKey} ExcelXll register success ");
                                }
                                else {
                                    rkExcelXll.SetValue(valueName, "/R \"" + szXllToRegister + "\"");
                                }
                                bFoundOffice = true;
                            }
                            else
                            {
                                LogHandler.Info($"Unable to retrieve key for : " + szKeyName);
                            }
                        }
                        else
                        {
                            LogHandler.Info($"Unable to retrieve registry Information for : " + szBaseAddInKey + szOfficeVersionKey);
                        }
                    }
                }

                LogHandler.Info($"End CaRegisterAddIn");
            }
            catch (System.Security.SecurityException ex)
            {
                LogHandler.Error($"CaRegisterAddIn SecurityException:{ex}");
                bFoundOffice = false;
            }
            catch (System.UnauthorizedAccessException ex)
            {
                LogHandler.Error($"CaRegisterAddIn UnauthorizedAccessException:{ex}");
                bFoundOffice = false;
            }
            catch (Exception ex)
            {
                LogHandler.Error($"CaRegisterAddIn Exception:{ex}");
                bFoundOffice = false;
            }
            return bFoundOffice;


        }
        #endregion


        private bool checkRegisterAddin() {
           string szOfficeRegKeyVersions = "11.0,12.0,14.0,15.0,16.0";
            List<string> lstVersions;
            string szBaseAddInKey = @"Software\Microsoft\Office\";
            double nVersion;
            bool bFoundOffice = true;
            
            if (szOfficeRegKeyVersions.Length > 0)
            {
                lstVersions = szOfficeRegKeyVersions.Split(',').ToList();

                foreach (string szOfficeVersionKey in lstVersions)
                {
                    nVersion = double.Parse(szOfficeVersionKey, NumberStyles.Any, CultureInfo.InvariantCulture);
                    if (Registry.CurrentUser.OpenSubKey(szBaseAddInKey + szOfficeVersionKey, false) != null)
                    {
                        string szKeyName = szBaseAddInKey + szOfficeVersionKey + @"\Excel\Options";
                        RegistryKey rkExcelXll = Registry.CurrentUser.OpenSubKey(szKeyName, true);

                        xllbit = GetAddInName("STCExcel-AddIn-packed.xll", "STCExcel-AddIn64-packed.xll", szOfficeVersionKey, nVersion);
                        if ( rkExcelXll != null)
                        {
                            string[] szValueNames = rkExcelXll.GetValueNames();
                            bool bIsOpen = true;
                            bool flag = false;
                            foreach (string szValueName in szValueNames)
                            {

                                if (szValueName.StartsWith("OPEN"))
                                {
                                    flag = true;
                                    if (!rkExcelXll.GetValue(szValueName).ToString().Contains("STCExcel"))
                                    {
                                        bIsOpen = false;
                                    }
                                   
                                }
                                
                            }

                            if (!bIsOpen|| !flag)
                            {
                                bFoundOffice = false;
                                //xllbit = string.Empty;
                                break;
                            }
                           
                        } 
                    }
                    
                }
            }

            return bFoundOffice;
        }

        #region CaUnRegisterAddIn
        public bool CaUnRegisterAddIn(string szXll32Bit, string szXll64Bit)
        {
            string szOfficeRegKeyVersions = string.Empty;
            string szBaseAddInKey = @"Software\Microsoft\Office\";
            string szFolder = string.Empty;
            bool bFoundOffice = true;
            List<string> lstVersions;

            try
            {
                LogHandler.Info($"Begin CaUnRegisterAddIn");

                szOfficeRegKeyVersions = " 11.0,12.0,14.0,15.0,16.0";

                szXll32Bit = Path.Combine(inInstallPath, szXll32Bit.ToString());
                szXll64Bit = Path.Combine(inInstallPath, szXll64Bit.ToString());



                if (szOfficeRegKeyVersions.Length > 0)
                {
                    lstVersions = szOfficeRegKeyVersions.Split(',').ToList();

                    foreach (string szOfficeVersionKey in lstVersions)
                    {
                        
                        if (Registry.CurrentUser.OpenSubKey(szBaseAddInKey + szOfficeVersionKey, false) != null)
                        {
                            bFoundOffice = true;

                            string szKeyName = szBaseAddInKey + szOfficeVersionKey + @"\Excel\Options";

                            var rkAddInKey = Registry.CurrentUser.OpenSubKey(szKeyName, true);
                            if (rkAddInKey == null) continue;

                            var szValueNames = rkAddInKey.GetValueNames();
                            var allOpenKeyValues = new List<string>();
                            foreach (string szValueName in szValueNames)
                            {
                               
                                if (szValueName.StartsWith("OPEN") && (rkAddInKey.GetValue(szValueName).ToString().Contains(szXll32Bit) || rkAddInKey.GetValue(szValueName).ToString().Contains(szXll64Bit)))
                                {
                                   
                                    rkAddInKey.DeleteValue(szValueName);
                                }
                                else if (szValueName.StartsWith("OPEN"))
                                {
                                    allOpenKeyValues.Add(rkAddInKey.GetValue(szValueName).ToString());
                                    rkAddInKey.DeleteValue(szValueName);
                                }
                            }

                            var i = 0;
                            foreach (var szValueName in allOpenKeyValues)
                            {
                                if (i > 0)
                                    rkAddInKey.SetValue("OPEN" + i, szValueName);
                                else
                                    rkAddInKey.SetValue("OPEN", szValueName);

                                ++i;
                            }
                        }
                    }
                }

                LogHandler.Info($"卸载 Excel 插件成功");
            }
            catch (Exception ex)
            {
                LogHandler.Error($"Excel 插件卸载失败！：{ex}");
                bFoundOffice = false;
            }

            return bFoundOffice;
        }
        #endregion

        public static string GetAddInName(string szXll32Name, string szXll64Name, string szOfficeVersionKey, double nVersion)
        {
            string szXllToRegister = string.Empty;

            if (nVersion >= 14)
            {
                // determine if office is 32-bit or 64-bit
                RegistryKey localMachineRegistry = // 64bit machines need to determine correct hive.
                    RegistryKey.OpenBaseKey(RegistryHive.LocalMachine,
                        Environment.Is64BitOperatingSystem ? RegistryView.Registry64 : RegistryView.Registry32);
                RegistryKey rkBitness = localMachineRegistry.OpenSubKey(@"Software\Microsoft\Office\" + szOfficeVersionKey + @"\Outlook", false);
                if (rkBitness != null)
                {
                    object oBitValue = rkBitness.GetValue("Bitness");
                    if (oBitValue != null)
                    {
                        if (oBitValue.ToString() == "x64")
                        {
                            szXllToRegister = szXll64Name;
                        }
                        else
                        {
                            szXllToRegister = szXll32Name;
                        }
                    }
                    else
                    {
                        szXllToRegister = szXll32Name;
                    }
                }
                else
                {
                    if (Environment.Is64BitOperatingSystem)
                    {
                        localMachineRegistry = //64bit machines need to check 32bit registry too!
                            RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);
                        rkBitness =
                            localMachineRegistry.OpenSubKey(
                                @"Software\Microsoft\Office\" + szOfficeVersionKey + @"\Outlook", false);
                        if (rkBitness != null)
                        {
                            var oBitValue = rkBitness.GetValue("Bitness");
                            if (oBitValue != null)
                            {
                                if (oBitValue.ToString() == "x64")
                                {
                                    szXllToRegister = szXll64Name;
                                }
                                else
                                {
                                    szXllToRegister = szXll32Name;
                                }
                            }
                            else
                            {
                                szXllToRegister = szXll32Name;
                            }
                        }
                        else
                        {
                            szXllToRegister = szXll64Name;
                        }
                    }
                    else
                        szXllToRegister = szXll32Name;
                }
            }
            else
            {
                szXllToRegister = szXll32Name;
            }
            LogHandler.Info($"GetAddInName szXllToRegister:{szXllToRegister}");
            return szXllToRegister;
        }
    }
}
