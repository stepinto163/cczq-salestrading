using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Utils
{
    /// <summary>
    /// ini文件处理类
    /// </summary>
  public  class IniFileHelper
    {        
        
        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);

        // 使用SHChangeNotify通知系统刷新INI文件
        [DllImport("shell32.dll")]
        private static extern void SHChangeNotify(int wEventId, int uFlags, IntPtr dwItem1, IntPtr dwItem2);


        private static string historyDataName = @"data\history.data";
        private static string inipath = AppDomain.CurrentDomain.BaseDirectory + @"data\history.data";
        public const string LoginData = "Login";

        private static void InitDataPath()
        {
            if (!string.IsNullOrEmpty(LocalEnv.ConfInfo.DataPath))
                inipath = Path.Combine(LocalEnv.ConfInfo.DataPath,  historyDataName);
        }

        private static bool ExistINIFile()
        {
            return File.Exists(inipath);
        }

        public static void WriteValue(string Section, string Key, string Value)
        {
            InitDataPath();

            if (!ExistINIFile())
            {
                FileHelper.CreateDirectory(inipath);
            }
            WritePrivateProfileString(Section, Key, Value, inipath);
        }
      
        public static string ReadValue(string Section, string Key, int size = 20000)
        {
            InitDataPath();

            if (ExistINIFile())
            {
                StringBuilder temp = new StringBuilder(size);
                int i = GetPrivateProfileString(Section, Key, "", temp, size, inipath);
                return temp.ToString();
            }
            else
            {
                return null;
            }
            
        }

        /// <summary>
        /// 通知系统刷新 INI 文件
        /// </summary>
        public static void RefreshIni()
        {
            // 通知系统刷新 INI 文件
            SHChangeNotify(0x8000000, 0x1000, IntPtr.Zero, IntPtr.Zero);
        }

        public static void WriteIniFile(ProxyInfo info, string filePath)
        {
            if (!File.Exists(filePath))
            {
                FileHelper.CreateDirectory(filePath);
            }

            WritePrivateProfileString("Proxy", "AccessType", info.AccessType, filePath);
            WritePrivateProfileString("Proxy", "ProxyPort", info.ProxyPort, filePath);
            WritePrivateProfileString("Proxy", "ProxyType", info.ProxyType, filePath);
            WritePrivateProfileString("Proxy", "ProxyAddress", info.ProxyAddress, filePath);
            WritePrivateProfileString("Proxy", "ProxyUser", info.ProxyUser, filePath);
            WritePrivateProfileString("Proxy", "ProxyPasswd", info.ProxyPasswd, filePath);
        }

        public static Dictionary<string, Dictionary<string, string>> ReadIniFile(string filePath)
        {
            var result = new Dictionary<string, Dictionary<string, string>>();
            string currentSection = null;

            if (File.Exists(filePath))
            {
                var lines = File.ReadAllLines(filePath);

                foreach (var line in lines)
                {
                    string trimmedLine = line.Trim();

                    if (trimmedLine.StartsWith("[") && trimmedLine.EndsWith("]"))
                    {
                        currentSection = trimmedLine.Substring(1, trimmedLine.Length - 2);
                        result[currentSection] = new Dictionary<string, string>();
                    }
                    else if (currentSection != null)
                    {
                        var parts = trimmedLine.Split('=');
                        if (parts.Length == 2)
                        {
                            var key = parts[0].Trim();
                            var value = parts[1].Trim();
                            result[currentSection][key] = value;
                        }
                    }
                }
            }

            return result;
        }
    }
}
