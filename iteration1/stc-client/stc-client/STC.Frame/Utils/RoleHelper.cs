using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Frame.Utils
{
    /// <summary>
    /// 控制插件类
    /// </summary>
    public class RoleHelper
    {

        static Dictionary<string, string[]> rolePluginFile = new Dictionary<string, string[]>();
        static List<string> sysPluginFile = new List<string>();
        static RoleHelper()
        {
            sysPluginFile.Add("STC.Sys.Plugin.dll");
            sysPluginFile.Add("IDC.Lua.Plugin.dll");
            rolePluginFile.Add("BOND", new string[] { "IDC.Data.Plugin.dll", "IDC.Broker.Plugin.dll" });//利率/信用/NCD/ABS 债券 
            rolePluginFile.Add("bill", new string[] { "IDC.Data.Plugin.dll", "IDC.Bill.Plugin.dll", "IDC.Bill.Plugin.Themes.dll" });//票据
            rolePluginFile.Add("MP", new string[] { "IDC.Data.Plugin.dll", "IDC.Matching.Plugin.dll" });//IRS 
            rolePluginFile.Add("STC", new string[] { 
                "IDC.Data.Plugin.dll",
                "STC.Bond.Plugin.dll",
                "STC.Mm.Plugin.dll",
                "STC.Mmshe.Plugin.dll",
                "STC.WorkTable.Plugin.dll",
                "STC.Position.Plugin.dll",
                "STC.WorkTable.Plugin.dll",
                "STC.InnerBoard.Plugin.dll",
                "STC.Rfq.Plugin.dll",
                "STC.Python.Plugin.dll",
                "STC.DownloadCenter.Plugin.dll",
                "STC.PT.Plugin.dll",
                "STC.IRS.Plugin.dll",
                "STC.Broker.Plugin",
                "STC.Repo.Plugin"
                });//bond 
            rolePluginFile.Add("BDB", new string[] { "BDB.Price.Plugin.dll", "STC.InnerBoard.Plugin.dll"});//BDB
            rolePluginFile.Add("IDCCLIENT", new string[] { "BDB.Price.Plugin.dll", "STC.InnerBoard.Plugin.dll"});//IDCCLIENT
        }

        /// <summary>
        /// 加载的插件
        /// </summary>
        /// <param name="prods"></param>
        /// <returns></returns>
        public static List<string> GetPluginFile(string prods)
        {
            if (netapi.Api.LocalIntegrationProfile.Current.Enabled)
            {
                // Local mode loads only the sales-trading business plugin.
                return new List<string> { "STC.SalesTrading.Plugin.dll" };
            }
            List<string> lt = new List<string>();
            if ((prods ?? string.Empty).Split(',').Contains("FICC"))
            {
                lt.Add("STC.SalesTrading.Plugin.dll");
            }
            switch (STC.Common.Utils.LocalEnv.ServerInfo.AppName.ToLower())
            {
                case "bdb":
                case "idcclient":
                    lt.AddRange(sysPluginFile);
                    lt.Add("BDB.Price.Plugin.dll");
                    //开发环境测试
                    lt.Add("STC.Data.Plugin.dll");
                    lt.Add("STC.Excel.Plugin.dll");
                    lt.Add("STC.Chrome.Plugin.dll");
                    lt.Add("STC.InnerBoard.Plugin.dll");
                    lt.Add("STC.Sys.Plugin.dll");
                    lt.Add("STC.XBond.Plugin.dll");
                    lt.Add("STC.WorkBench.Plugin.dll");
                    lt.Add("STC.quant.plugin.dll");
                    //生产环境用，生产环境需要Jenkins的脚本中修改文件名
                    lt.Add("BDB.Data.Plugin.dll");
                    lt.Add("BDB.Excel.Plugin.dll");
                    lt.Add("BDB.Chrome.Plugin.dll");
                    lt.Add("BDB.InnerBoard.Plugin.dll");
                    lt.Add("BDB.Sys.Plugin.dll");
                    lt.Add("BDB.XBond.Plugin.dll");
                    lt.Add("BDB.WorkBench.Plugin.dll");
                    lt.Add("BDB.quant.plugin.dll");
                    break;
                default:
                    lt.AddRange(sysPluginFile);
                    lt.Add("STC.XBond.Plugin.dll");
                    lt.Add("STC.Data.Plugin.dll");
                    lt.Add("STC.Mm.Plugin.dll");
                    lt.Add("STC.Execute.Plugin.dll");
                    lt.Add("STC.BilateralMarket.Plugin.dll");
                    lt.Add("STC.quant.plugin.dll");
                    lt.Add("STC.WorkTable.Plugin.dll");
                    lt.Add("STC.Excel.Plugin.dll");
                    lt.Add("STC.Chrome.Plugin.dll");
                    lt.Add("STC.Mmshe.Plugin.dll");
                    lt.Add("STC.Position.Plugin.dll");
                    lt.Add("STC.WorkBench.Plugin.dll");
                    lt.Add("STC.InnerBoard.Plugin.dll");
                    lt.Add("STC.Rfq.Plugin.dll");
                    lt.Add("STC.Python.Plugin.dll");
                    lt.Add("STC.DownloadCenter.Plugin.dll");
                    lt.Add("STC.PT.Plugin.dll");
                    lt.Add("STC.IRS.Plugin.dll");
                    lt.Add("STC.Broker.Plugin.dll");
                    lt.Add("STC.PP.Plugin.dll");
                    lt.Add("STC.SalesTrading.Plugin.dll");
                    break;
            }


            if (!string.IsNullOrEmpty(prods))
            {
                string[] strs = prods.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var str in strs)
                {
                    if (rolePluginFile.ContainsKey(str))
                    {
                        var ps = rolePluginFile[str];
                        foreach (string s in ps)
                        {
                            if (!lt.Contains(s))
                            {
                                lt.Add(s);
                            }
                        }
                    }
                }
            }

            return lt;
        }

        public static List<string> getPluginFile(string termids)
        {
            List<string> lt = new List<string>();
            lt.AddRange(sysPluginFile);
            if (!string.IsNullOrEmpty(termids))
            {
                string[] strs = termids.Split(new string[] { ","},StringSplitOptions.RemoveEmptyEntries);
                foreach (var str in strs)
                {
                    if (rolePluginFile.ContainsKey(str))
                    {
                        var ps = rolePluginFile[str];
                        foreach(string s in ps) { 
                        if (!lt.Contains(s))
                        {
                            lt.Add(s);
                        }
                        }
                    }
                }
            }
        
            return lt;
        }
    }
}
