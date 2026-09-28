using com.gateway.log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Utils
{
    public class EngineHelper
    {
        private static string _MAC = string.Empty;
        private static string _HARDDISKID = string.Empty;

        /// <summary>
        /// 获取本机MAC地址
        /// </summary>
        /// <returns></returns>
        public static string GetMacByNetworkInterface()
        {
            if (!string.IsNullOrEmpty(_MAC))
                return _MAC;

            string mac = string.Empty;
            try
            {
                List<string> macs = new List<string>();
                NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();
                foreach (NetworkInterface ni in interfaces)
                {
                    if (ni.OperationalStatus == OperationalStatus.Up && !string.IsNullOrEmpty(ni.GetPhysicalAddress().ToString()))
                        macs.Add(ni.GetPhysicalAddress().ToString());
                }

                mac = macs.Count > 0 ? macs[0] : string.Empty;
                if (!string.IsNullOrEmpty(mac) && mac.Length > 11)
                {
                    mac = mac.Insert(2, "-").Insert(5, "-").Insert(8, "-").Insert(11, "-").Insert(14, "-");
                }

                _MAC = mac;
            }
            catch (Exception e)
            {
                LogHelper.Error("GetMacByNetworkInterface exception:", e);
            }

            return mac;
        }

        //获取磁盘序列号

        public static string Getharddiskid2()
        {
            if (!string.IsNullOrEmpty(_HARDDISKID))
                return _HARDDISKID;

            string strDiskDrive;
            ManagementObjectSearcher opSearch;
            ManagementObjectCollection mocSystem;
            strDiskDrive = "";
            try
            {
                opSearch = new ManagementObjectSearcher("SELECT * FROM Win32_DiskDrive");
                mocSystem = opSearch.Get();
                foreach (ManagementObject opInfo in mocSystem)
                {
                    strDiskDrive = opInfo["Model"].ToString().Trim();
                    
                    break;
                }
            }
            catch
            {
                //TO DO：添加异常处理                    
            }

            _HARDDISKID = strDiskDrive;

            return strDiskDrive;
        }

    /// <summary>
    /// 取得设备硬盘的物理序列号   
    /// </summary>
    /// <returns></returns>

    public static string GetSerialNumber()
        {
            try
            {
                ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PhysicalMedia");
                string sHardDiskSerialNumber = "";
                foreach (ManagementObject mo in searcher.Get())
                {
                    sHardDiskSerialNumber = mo["SerialNumber"].ToString().Trim();//去第一块目前
                    break;
                }
                return sHardDiskSerialNumber;
            }
            catch
            {
                return "";
            }

        }

        public static string GetIp1()
        {
            string ip = string.Empty;

            List<string> ipList = GetLocalIpAddress("InterNetwork");

            if (ipList != null && ipList.Count > 0)
            {
                ip = ipList[0];
            }
            else
            {
                ipList = GetLocalIpAddress("InterNetworkV6");
                if (ipList != null && ipList.Count > 0)
                {
                    ip = ipList[0];
                }
            }

            return ip;
        }

        /// <summary>
        /// 获取本机所有ip地址
        /// </summary>
        /// <param name="netType">"InterNetwork":ipv4地址，"InterNetworkV6":ipv6地址</param>
        /// <returns>ip地址集合</returns>
        public static List<string> GetLocalIpAddress(string netType)
        {
            List<string> IPList = new List<string>();
            try
            {
                string hostName = Dns.GetHostName();                    //获取主机名称  
                IPAddress[] addresses = Dns.GetHostAddresses(hostName); //解析主机IP地址  

                if (netType == string.Empty)
                {
                    for (int i = 0; i < addresses.Length; i++)
                    {
                        IPList.Add(addresses[i].ToString());
                    }
                }
                else
                {
                    //AddressFamily.InterNetwork表示此IP为IPv4,
                    //AddressFamily.InterNetworkV6表示此地址为IPv6类型
                    for (int i = 0; i < addresses.Length; i++)
                    {
                        if (addresses[i].AddressFamily.ToString() == netType)
                        {
                            IPList.Add(addresses[i].ToString());
                        }
                    }
                }
            }
            catch (Exception e)
            {
                LogHelper.Error("GetLocalIpAddress exception:", e);
            }
            return IPList;
        }

        public static string GetIp()
        {
            List<string> localIpList = GetLocalIPList();
            if (localIpList != null && localIpList.Count > 0)
            {
                return localIpList[0];
            }
            return string.Empty;
        }

        public static List<string> GetLocalIPList()
        {
            List<string> IpList = new List<string>();
            List<string> AllIpList = new List<string>();
            try
            {
                foreach (var item in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
                {
                    AllIpList.Add(item.ToString());
                }
                if (AllIpList.Count() == 1)
                {
                    IpList.Add(AllIpList[0].ToString());
                    return IpList;
                }
                else if (AllIpList.Count() == 0)
                {
                    return IpList;
                }
                //当存在多个网卡时,判断哪个ip是物理网卡的ip
                else
                {
                    //虚拟网卡mac地址的前6位
                    string[] vmwareList = new string[] { "000569", "000C29", "005056", "001c14", "001C42", "0003FF", "000F4B", "00163E", "080027" };
                    ManagementClass mc = new ManagementClass("Win32_NetworkAdapterConfiguration");
                    ManagementObjectCollection nics = mc.GetInstances();
                    foreach (ManagementObject nic in nics)
                    {
                        if (Convert.ToBoolean(nic["ipEnabled"]) == true)
                        {
                            foreach (var item in (nic["IPAddress"] as String[]))
                            {
                                if (AllIpList.Contains(item))
                                {
                                    string[] gw = nic["DefaultIPGateway"] as String[];
                                    if (gw != null && gw.Length > 0)//判断当前网卡是否存在默认网关
                                    {
                                        string mac = nic["MacAddress"].ToString().Replace(":", "").Replace("-", "");
                                        if (!string.IsNullOrEmpty(mac) && mac.Length > 6)
                                        {
                                            mac = mac.Substring(0, 6);
                                            if (!vmwareList.Contains(mac))//判断当前网卡的mac是否为虚拟网卡的mac
                                            {
                                                IpList.Add(item);
                                                return IpList;
                                            }
                                        }
                                    }
                                }
                            }

                        }
                    }
                }
            }
            catch
            {
                return IpList;
            }

            return IpList;
        }
    }
}
