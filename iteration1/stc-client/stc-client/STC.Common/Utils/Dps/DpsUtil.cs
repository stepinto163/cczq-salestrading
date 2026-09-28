using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace STC.Common.Utils.Dps
{
    public class DpsUtil
    {
        private static DpsUtil server = new DpsUtil();
        public static DpsUtil Instance { get { return server; } }

        ProcessWatcher pw = new ProcessWatcher();

        public void StartDps()
        {
            string startDps = LocalEnv.ConfInfo.StartDps;
            if (!string.IsNullOrEmpty(startDps) && startDps.Equals("1"))
            {
                LogHandler.Info("Start dps");
                string dps = "dps.exe";
                if (!string.IsNullOrEmpty(LocalEnv.ConfInfo.LoginEnv))
                {
                    dps = GetDpsFullName();
                }
                pw.WatcherProName = dps;

                switch (LocalEnv.ServerInfo.AppName.ToLower())
                {
                    case "bdb":
                    case "idcclient":
                        ProxyInfo proxyInfoQb = ProxyUtils.ReadQBProxyInfo();
                        SetDpsProxyInfo(proxyInfoQb);
                        break;
                    default:
                        ProxyInfo proxyInfo = ProxyUtils.ReadProxyInfo();
                        SetDpsProxyInfo(proxyInfo);
                        break;

                }

                try
                {
                    pw.start();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.StackTrace);
                }
            }
        }

        public string GetDpsFullName()
        {
            string dpsname = string.Empty;
            string startDps = LocalEnv.ConfInfo.StartDps;
            if (!string.IsNullOrEmpty(startDps) && startDps.Equals("1"))
            {
                string env = string.Empty;
                if (LocalEnv.ConfInfo.LoginEnv.Contains("."))
                {
                    env = LocalEnv.ConfInfo.LoginEnv.Split('.')[0];
                }
                else
                {
                    env = LocalEnv.ConfInfo.LoginEnv;
                }

                switch (LocalEnv.ServerInfo.AppName.ToLower())
                {
                    case "bdb":
                    case "idcclient":
                        dpsname = "dps-bdb.exe";
                        break;
                    default:

                        if (env == "PRD" || "PRO".Equals(env, StringComparison.OrdinalIgnoreCase))
                        {
                            dpsname = "dps.exe";
                        }
                        else
                        {
                            dpsname = string.Format("{0}-{1}.exe", "dps", LocalEnv.ConfInfo.LoginEnv);
                        }
                        break;

                }
            }
            return dpsname;
        }

        public com.gateway.connector.tcp.ProxyInfo ToTcpProxyInfo(ProxyInfo proxyInfoCfg)
        {
            //AccessType:
            //1：不使用代理
            //0: 使用与Internet Explorer相同的代理
            //3: socks4 （同时ProxyType=1）
            //3: socks5 （同时ProxyType=2）
            //3: https  （同时ProxyType=3）
            if (!string.IsNullOrEmpty(proxyInfoCfg.AccessType) && proxyInfoCfg.AccessType != "1")
            {
                com.gateway.connector.tcp.ProxyType proxyType = com.gateway.connector.tcp.ProxyType.http;
                if (!string.IsNullOrEmpty(proxyInfoCfg.ProxyType))
                {
                    if (proxyInfoCfg.AccessType == "0")
                    {
                        // 获取IE代理设置
                        WebRequest.DefaultWebProxy = WebRequest.GetSystemWebProxy();
                        WebProxy webProxy = WebRequest.DefaultWebProxy as WebProxy;
                        if (webProxy == null)
                        {
                            webProxy = WebProxy.GetDefaultProxy();
                        }
                        if (webProxy != null && webProxy.Address != null)
                        {
                            proxyInfoCfg.ProxyAddress = webProxy.Address.Host;
                            proxyInfoCfg.ProxyPort = webProxy.Address.Port.ToString();

                            proxyType = com.gateway.connector.tcp.ProxyType.http;
                        }
                        else
                        {
                            proxyInfoCfg.ProxyAddress = string.Empty;
                            proxyInfoCfg.ProxyPort = string.Empty;
                        }
                    }
                    else if (proxyInfoCfg.AccessType == "3")
                    {
                        if (proxyInfoCfg.ProxyType == "1")
                        {
                            proxyType = com.gateway.connector.tcp.ProxyType.socks4;
                        }
                        else if (proxyInfoCfg.ProxyType == "2")
                        {
                            proxyType = com.gateway.connector.tcp.ProxyType.socks5;
                        }
                        else if (proxyInfoCfg.ProxyType == "3")
                        {
                            proxyType = com.gateway.connector.tcp.ProxyType.http;
                        }
                    }
                }
                else
                {
                    proxyInfoCfg.ProxyAddress = string.Empty;
                    proxyInfoCfg.ProxyPort = string.Empty;
                }

                int proxyPort = 0;
                int.TryParse(proxyInfoCfg.ProxyPort, out proxyPort);

                if (!string.IsNullOrEmpty(proxyInfoCfg.ProxyAddress) && !string.IsNullOrEmpty(proxyInfoCfg.ProxyPort))
                {
                    com.gateway.connector.tcp.ProxyInfo proxy = new com.gateway.connector.tcp.ProxyInfo(proxyType,
                    proxyInfoCfg.ProxyAddress, proxyPort, proxyInfoCfg.ProxyUser, proxyInfoCfg.ProxyPasswd);

                    return proxy;
                } 
            }
            return null;
        }

        public void SetDpsProxyInfo(ProxyInfo proxyInfo)
        {
            pw.Proxy = ToTcpProxyInfo(proxyInfo);
        }

        public void StopDps()
        {
            try
            {
                pw.stop();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
            }
        }

        //BDB Add
        private const string LoginData = "Login";
        private const string ServerInfo = "ServerInfo";

        public void WriteServerInfo(ServerInfo serverInfo)
        {
            string json = serverInfo == null ? string.Empty : CommonUtils.SerializeObject(serverInfo);
            com.gateway.log.LogHelper.Info("WriteServerInfo:" + json);
            IniFileHelper.WriteValue(LoginData, ServerInfo, json);
        }

        public bool IsValidDomain(string domain)
        {
            string pattern = @"^(?=.{1,253}$)((?!-)[A-Za-z0-9-]{1,63}(?<!-)\.)+[A-Za-z]{2,}$";
            Regex regex = new Regex(pattern);
            return regex.IsMatch(domain);
        }

        public bool IsValidIPv4(string ip)
        {
            string pattern = @"^(\d{1,3}\.){3}\d{1,3}$";
            Regex regex = new Regex(pattern);
            if (regex.IsMatch(ip))
            {
                string[] parts = ip.Split('.');
                foreach (string part in parts)
                {
                    if (int.Parse(part) > 255 || int.Parse(part) < 0)
                    {
                        return false;
                    }
                }
                return true;
            }
            return false;
        }

        public bool IsValidIPv6(string ip)
        {
            string pattern = @"^([\da-fA-F]{1,4}:){7}[\da-fA-F]{1,4}$|^([\da-fA-F]{1,4}:){1,7}:$|^([\da-fA-F]{1,4}:){1,6}:[\da-fA-F]{1,4}$|^([\da-fA-F]{1,4}:){1,5}(:[\da-fA-F]{1,4}){1,2}$|^([\da-fA-F]{1,4}:){1,4}(:[\da-fA-F]{1,4}){1,3}$|^([\da-fA-F]{1,4}:){1,3}(:[\da-fA-F]{1,4}){1,4}$|^([\da-fA-F]{1,4}:){1,2}(:[\da-fA-F]{1,4}){1,5}$|^[\da-fA-F]{1,4}:(:[\da-fA-F]{1,4}){1,6}$|^:((:[\da-fA-F]{1,4}){1,7}|:)$|^fe80:(:[\da-fA-F]{0,4}){0,4}%[0-9a-zA-Z]{1,}$|^::(ffff(:0{1,4}){0,1}:){0,1}((25[0-5]|(2[0-4]|1{0,1}[0-9]|[1-9]?)?[0-9])\.){3,3}(25[0-5]|(2[0-4]|1{0,1}[0-9]|[1-9]?)?[0-9])$|^([\da-fA-F]{1,4}:){1,4}:((25[0-5]|(2[0-4]|1{0,1}[0-9]|[1-9]?)?[0-9])\.){3,3}(25[0-5]|(2[0-4]|1{0,1}[0-9]|[1-9]?)?[0-9])$";
            Regex regex = new Regex(pattern);
            return regex.IsMatch(ip);
        }

        private ServerInfo GetDefaultServerInfo(ServerInfo serverInfo)
        {
            ServerInfo result = new ServerInfo();

            result.Host = serverInfo.Host;
            result.Port = serverInfo.Port;
            result.Id = serverInfo.Id;
            result.Protocol = serverInfo.Protocol;
            result.Name = serverInfo.Name;

            return result;
        }
        public ServerInfo GetRemoteServerInfo(string readType, List<string> filterHost)
        {
            ServerInfo result = null;

            string usedServerInfo = string.Empty;
            if (!string.IsNullOrEmpty(readType) && readType == "1" && LocalEnv.ConfInfo.RemoteServerInfos != null && LocalEnv.ConfInfo.RemoteServerInfos.Count > 0)
            {
                usedServerInfo = IniFileHelper.ReadValue(LoginData, ServerInfo);

                com.gateway.log.LogHelper.Info("GetRemoteServerInfo from history:" + usedServerInfo);
            }


            if (string.IsNullOrEmpty(usedServerInfo))
            {
                foreach (ServerInfo serverInfo in LocalEnv.ConfInfo.RemoteServerInfos)
                {
                    try
                    {
                        if (filterHost != null && filterHost.Contains(serverInfo.Host))
                            continue;


                        IPHostEntry hostEntry = Dns.GetHostEntry(serverInfo.Host);

                        string ipAddress = hostEntry.AddressList[0].ToString();

                        result = GetDefaultServerInfo(serverInfo);

                        break;
                    }
                    catch (Exception ex)
                    {
                        com.gateway.log.LogHelper.Error("Host:" + serverInfo.Host + " parse fail:", ex);
                        result = GetDefaultServerInfo(serverInfo);
                        break;
                    }
                }
            }
            else
            {
                result = Newtonsoft.Json.JsonConvert.DeserializeObject<ServerInfo>(usedServerInfo);
            }

            string json = Newtonsoft.Json.JsonConvert.SerializeObject(result);
            com.gateway.log.LogHelper.Info("GetRemoteServerInfo:" + json);


            return result;
        }
    }
}
