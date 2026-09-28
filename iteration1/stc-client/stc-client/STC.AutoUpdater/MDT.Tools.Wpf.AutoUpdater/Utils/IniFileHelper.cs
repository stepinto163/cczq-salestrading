using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MDT.Tools.Wpf.Utils
{
  public  class IniFileHelper
    {        
        
        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);

        private static string inipath = AppDomain.CurrentDomain.BaseDirectory + @"data\history.data";
        public const string LoginData = "Login";

        private static bool ExistINIFile()
        {
            return File.Exists(inipath);
        }

        public static void WriteValue(string Section, string Key, string Value)
        {
            if (!ExistINIFile())
            {
                FileHelper.CreateDirectory(inipath);
            }
            WritePrivateProfileString(Section, Key, Value, inipath);
        }
      
        public static string ReadValue(string Section, string Key)
        {
            if (ExistINIFile())
            {
                StringBuilder temp = new StringBuilder(500);
                int i = GetPrivateProfileString(Section, Key, "", temp, 500, inipath);
                return temp.ToString();
            }
            else
            {
                return null;
            }
            
        }      
        
    }
}
