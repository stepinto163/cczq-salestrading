using STC.Common.Model.Common;
using com.gateway.log;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Utils.Proxy
{
    public class ProxyUtils
    {
        public static ProxyInfo ProxyInfo = new ProxyInfo();

        public static ProxyInfo ReadProxyInfo()
        {
            ProxyInfo proxyInfo = new ProxyInfo();

            string baseFilePath = AppDomain.CurrentDomain.BaseDirectory;
            string substringToExclude = "IDCClient\\";

            int index = baseFilePath.IndexOf(substringToExclude);
            LogHelper.Info("baseFilePath:" + baseFilePath);
            if (index >= 0)
            {
                string proxyCfgPath = baseFilePath.Substring(0, index);
                // result 现在包含 "E:\\QB-prd\\Qeubee" 部分

                proxyCfgPath = proxyCfgPath + "QuoteServer.ini";
                LogHelper.Info("proxyCfgPath:" + proxyCfgPath);
                Dictionary<string, Dictionary<string, string>> dic = IniFileReader.ReadIniFile(proxyCfgPath);
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
    }
}
