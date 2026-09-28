using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace STC.Common.Model
{
    /// <summary>
    /// 帮助类
    /// </summary>
    internal class RunTimeConfig
    {
        public string FramePluginKey { get; set; } = "";
        public string FunctionPluginKey { get; set; } = "";

        public List<int> GetFramePluginKeyList
        {
            get
            {
                List<int> lt = new List<int>();
                string[] strs = FramePluginKey.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                int temp = 0;
                foreach (string str in strs)
                {
                    if (int.TryParse(str, out temp))
                    {
                        lt.Add(temp);
                    }
                }
                return lt;
            }
        }


        public List<int> GetFunctionPluginKeyList
        {
            get
            {
                List<int> lt = new List<int>();
                string[] strs = FunctionPluginKey.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                int temp = 0;
                foreach (string str in strs)
                {
                    if (int.TryParse(str, out temp))
                    {
                        lt.Add(temp);
                    }
                }
                return lt;
            }
        }
    }
}
