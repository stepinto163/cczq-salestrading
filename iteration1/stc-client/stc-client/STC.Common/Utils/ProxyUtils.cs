using com.gateway.log;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Utils
{
    public class ProxyUtils
    {
        public static ProxyInfo ProxyInfo = new ProxyInfo();

        /// <summary>
        /// Chrome浏览器代理信息
        /// </summary>
        public static string ChromeProxyinfo = "";

        public static ProxyInfo ReadProxyInfo()
        {
            ProxyInfo proxyInfo = new ProxyInfo();

            string proxyCfgPath = AppDomain.CurrentDomain.BaseDirectory + @"data\Proxy.ini";
            if (File.Exists(proxyCfgPath))
            {
                LogHelper.Info("proxyCfgPath:" + proxyCfgPath);
                Dictionary<string, Dictionary<string, string>> dic = IniFileHelper.ReadIniFile(proxyCfgPath);
                LogHelper.Info("proxyCfg:" + JsonConvert.SerializeObject(dic));
                if (dic.ContainsKey("Proxy"))
                {
                    var proxySection = dic["Proxy"];
                    if (proxySection.ContainsKey("AccessType"))
                    {
                        string accessType = proxySection["AccessType"];
                        proxyInfo.AccessType = accessType;
                        // 处理 accessType 值
                    }
                    if (proxySection.ContainsKey("ProxyPort"))
                    {
                        string proxyPort = proxySection["ProxyPort"];
                        proxyInfo.ProxyPort = proxyPort;
                        // 处理 accessType 值
                    }
                    if (proxySection.ContainsKey("ProxyType"))
                    {
                        string proxyType = proxySection["ProxyType"];
                        proxyInfo.ProxyType = proxyType;
                        // 处理 accessType 值
                    }
                    if (proxySection.ContainsKey("ProxyAddress"))
                    {
                        string proxyAddress = proxySection["ProxyAddress"];
                        proxyInfo.ProxyAddress = proxyAddress;
                        // 处理 accessType 值
                    }
                    if (proxySection.ContainsKey("ProxyPasswd"))
                    {
                        string proxyPasswd = proxySection["ProxyPasswd"];
                        proxyInfo.ProxyPasswd = proxyPasswd;
                        // 处理 accessType 值
                    }
                    if (proxySection.ContainsKey("ProxyUser"))
                    {
                        string proxyUser = proxySection["ProxyUser"];
                        proxyInfo.ProxyUser = proxyUser;
                        // 处理 accessType 值
                    }
                    // 你可以继续检查和处理 Proxy 部分中的其他键值
                }
                LogHelper.Info("ProxyInfo:" + JsonConvert.SerializeObject(proxyInfo));
            }


            ProxyInfo = proxyInfo;

            return proxyInfo;
        }

        public static ProxyInfo ReadQBProxyInfo()
        {
            ProxyInfo proxyInfo = new ProxyInfo();

            string baseFilePath = AppDomain.CurrentDomain.BaseDirectory;
            string substringToExclude = "IDCClient\\";

            //substringToExclude = "client\\";

            int index = baseFilePath.IndexOf(substringToExclude);
            LogHelper.Info("baseFilePath:" + baseFilePath);
            if (index >= 0)
            {
                string proxyCfgPath = baseFilePath.Substring(0, index);
                // result 现在包含 "E:\\QB-prd\\Qeubee" 部分

                //proxyCfgPath = "D:\\code\\git\\stc-lite\\stc-client\\"; 

                proxyCfgPath = proxyCfgPath + "QuoteServer.ini";
                LogHelper.Info("proxyCfgPath:" + proxyCfgPath);
                Dictionary<string, Dictionary<string, string>> dic = IniFileHelper.ReadIniFile(proxyCfgPath);
                LogHelper.Info("proxyCfg:" + JsonConvert.SerializeObject(dic));
                if (dic.ContainsKey("Proxy"))
                {
                    var proxySection = dic["Proxy"];
                    if (proxySection.ContainsKey("AccessType"))
                    {
                        string accessType = proxySection["AccessType"];
                        proxyInfo.AccessType = accessType;
                        // 处理 accessType 值
                    }
                    if (proxySection.ContainsKey("ProxyPort"))
                    {
                        string proxyPort = proxySection["ProxyPort"];
                        proxyInfo.ProxyPort = proxyPort;
                        // 处理 accessType 值
                    }
                    if (proxySection.ContainsKey("ProxyType"))
                    {
                        string proxyType = proxySection["ProxyType"];
                        proxyInfo.ProxyType = proxyType;
                        // 处理 accessType 值
                    }
                    if (proxySection.ContainsKey("ProxyAddress"))
                    {
                        string proxyAddress = proxySection["ProxyAddress"];
                        proxyInfo.ProxyAddress = proxyAddress;
                        // 处理 accessType 值
                    }
                    if (proxySection.ContainsKey("ProxyPasswd"))
                    {
                        string proxyPasswd = proxySection["ProxyPasswd"];
                        proxyInfo.ProxyPasswd = proxyPasswd;
                        // 处理 accessType 值
                    }
                    if (proxySection.ContainsKey("ProxyUser"))
                    {
                        string proxyUser = proxySection["ProxyUser"];
                        proxyInfo.ProxyUser = proxyUser;
                        // 处理 accessType 值
                    }
                    // 你可以继续检查和处理 Proxy 部分中的其他键值
                }
                LogHelper.Info("ProxyInfo:" + JsonConvert.SerializeObject(proxyInfo));
            }

            ProxyInfo = proxyInfo;

            return proxyInfo;
        }

        public static void SaveProxyInfo(ProxyInfo info)
        {
            string proxyCfgPath = AppDomain.CurrentDomain.BaseDirectory + @"data\Proxy.ini";
            IniFileHelper.WriteIniFile(info, proxyCfgPath);
            ProxyUtils.ProxyInfo = info;
        }

        public static string ConvertToUpdate()
        {
            //AccessType:
            //1：不使用代理
            //0: 使用与Internet Explorer相同的代理
            //3: socks4 （同时ProxyType=1）
            //3: socks5 （同时ProxyType=2）
            //3: https  （同时ProxyType=3）
            if (!string.IsNullOrEmpty(ProxyUtils.ProxyInfo.AccessType) && ProxyUtils.ProxyInfo.AccessType != "1")
            {
                if (!string.IsNullOrEmpty(ProxyUtils.ProxyInfo.AccessType))
                {
                    if (ProxyUtils.ProxyInfo.AccessType == "0")
                    {
                        return "4";
                    }
                    else if (ProxyUtils.ProxyInfo.AccessType == "3")
                    {
                        if (ProxyUtils.ProxyInfo.ProxyType == "1")
                        {
                            return "2";
                        }
                        else if (ProxyUtils.ProxyInfo.ProxyType == "2")
                        {
                            return "3";
                        }
                        else if (ProxyUtils.ProxyInfo.ProxyType == "3")
                        {
                            return "1";
                        }
                    }
                }
            }
            return "0";
        }

        public static string getArgs(com.gateway.connector.tcp.ProxyInfo proxy)
        {
            StringBuilder args = new StringBuilder();
            if (proxy != null && !string.IsNullOrEmpty(proxy.ProxyHost) && proxy.ProxyPort > 0)
            {

                args.Append("-t ").Append(proxy.ProxyType).Append(" ");

                if (!string.IsNullOrEmpty(proxy.ProxyHost))
                {
                    args.Append("-h ").Append(proxy.ProxyHost).Append(" ");
                }

                args.Append("-p ").Append(proxy.ProxyPort).Append(" ");

                if (!string.IsNullOrEmpty(proxy.ProxyUser))
                {
                    args.Append("-u ").Append(proxy.ProxyUser).Append(" ");
                }
                if (!string.IsNullOrEmpty(proxy.ProxyPassword))
                {
                    args.Append("-d ").Append(proxy.ProxyPassword).Append(" ");
                }
            }
            args.Append($"-gh {LocalEnv.ServerInfo.DpsHost} -gp {LocalEnv.ServerInfo.DpsPort} -gt {LocalEnv.ServerInfo.DpsProtocol}");
            LogHelper.Info("getArgs:{0}", args);
            return args.ToString();
        }


        //http代理
        //SetProxyNew(Browser, "127.0.0.1:8118");
        //sock5代理
        //SetProxyNew(Browser, "socks5://127.0.0.1:1080");
        //是否开代理
        //AccessType:
        //1：不使用代理
        //0: 使用与Internet Explorer相同的代理
        //3: socks4 （同时ProxyType=1）
        //3: socks5 （同时ProxyType=2）
        //3: https  （同时ProxyType=3）
        public static string GetChromeProxyInfo(ProxyInfo proxyInfo)
        {
            if (proxyInfo == null || string.IsNullOrWhiteSpace(proxyInfo.AccessType))
            {
                return "";
            }
            if (proxyInfo.AccessType == "1")
            {
                //不使用代理
                return "";
            }
            else if (proxyInfo.AccessType == "0")
            {
                //使用与Internet Explorer相同的代理
                return "";
            }
            else if (proxyInfo.AccessType == "3")
            {
                //socks4/socks5/https代理
                if (proxyInfo.ProxyType == "1" || proxyInfo.ProxyType == "2")
                {
                    return "socks5://" + proxyInfo.ProxyAddress + ":" + proxyInfo.ProxyPort;
                }
                else if (proxyInfo.ProxyType == "3")
                {
                    return proxyInfo.ProxyAddress + ":" + proxyInfo.ProxyPort;
                }
            }

            return "";
        }

    }
}
