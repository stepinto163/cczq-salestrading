using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Sys.Plugin.Utils
{
    /// <summary>
    /// 日志清理处理器
    /// </summary>
    public class LogClearHelper
    {
        public static int holdDays = 7;
        public static string dirPath = "logs";
        public static void Clear()
        {

            try
            {
                string[] files = Directory.GetFiles(dirPath);
                var lt = getHoldDays();

                foreach (string file in files)
                {
                    try
                    {
                        string[] fileName = file.Split(new string[] { "\\" }, StringSplitOptions.RemoveEmptyEntries);
                        string simplefileName = fileName[fileName.Length - 1];
                        bool flag = false;
                        foreach (string holdDate in lt)
                        {
                            if (simplefileName.Contains(holdDate))
                            {
                                flag = true;
                                break;
                               
                            }
                        }
                        if (!flag) {
                            File.Delete(file);
                        }
                    }
                    catch (Exception e) { }

                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

        }
        public static List<string> getHoldDays()
        {
            List<string> lt = new List<string>();
            var now = DateTime.Now;
            for (int i = holdDays; i > 0; i--)
            {
                lt.Add(now.ToString("yyyyMMdd"));
                now = now.AddDays(-1);

            }
            return lt;
        }
    }
}
