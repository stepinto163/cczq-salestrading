using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Sys.Plugin.Utils
{
    /// <summary>
    /// 临时数据清理处理器
    /// </summary>
    public class RockDbClearHelper
    {
        public static int holdDays = 7;
        public static string dirPath = System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "data");
        public static void Clear()
        {
            try
            {
                string[] files = Directory.GetDirectories(dirPath);
                var lt = getHoldDays();

                string drpath = string.Empty;
                string drfullpath = string.Empty;
                foreach (string s in files)
                {
                    drpath = s.Replace(dirPath, "").Replace("\\","");
                    if (!lt.Contains(drpath.Split('.')[0]))
                    {
                        drfullpath = Path.Combine(dirPath, drpath);
                        if (Directory.Exists(drfullpath))
                        {
                            try
                            {
                                com.gateway.log.LogHelper.Info($"Delete RockDb's temporary directory: {drfullpath}");
                                STC.Common.Utils.FileHelper.DeleteDirectory(drfullpath);
                            }
                            catch (Exception ex)
                            {
                                com.gateway.log.LogHelper.Error($"Delete RockDb's temporary directory {drfullpath} exception: {ex.Message}");
                            }
                        }
                    }
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
