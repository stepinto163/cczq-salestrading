using System;
using System.Data;
using System.IO;
using System.Windows.Forms;

namespace MDT.Tools.Lua.Plugin.Utils
{
    internal class FilePathHelper
    {
        private FilePathHelper()
        { }
       
        public static readonly string LuaScriptPath = Application.StartupPath + "\\script\\";
    }
}
