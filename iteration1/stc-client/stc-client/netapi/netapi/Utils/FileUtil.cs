using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace common.netapi.Utils
{
    public class FileUtil
    {
        public static string BaseDir { get; set; }

        public static string ReadFile(string name)
        {
            if (File.Exists(name))
            {
                StreamReader reader = new StreamReader(name);
                return reader.ReadToEnd();
            }
            return "";
        }

        public static void CreateFolderByFile(string file)
        {
            int index1 = file.LastIndexOfAny(new char[] { '/', '\\' });
            if (index1 > 0)
            {
                string folder = file.Substring(0, index1);
                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }
            }
        }
    }
}
