using com.gateway.log;
using STC.Sys.Plugin.Utils;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace STC.Sys.Plugin
{
    /// <summary>
    /// 机器系统信息
    /// </summary>
    public class SysInfo
    {
        private static PerformanceCounter pcCpuLoad;
        static SysInfo()
        {
            try
            {
                pcCpuLoad = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            }
            catch
            {


            }

        }

        /// <summary>
        /// cpu、内存信息
        /// </summary>
        public static void sysRunInfo()
        {
            StringBuilder sb = new StringBuilder();
            try
            {
                //Win32Helper.MEMORY_INFO memInfo = new Win32Helper.MEMORY_INFO();
                //Win32Helper.GlobalMemoryStatus(ref memInfo);
                float cpu = 0;
                if (pcCpuLoad != null)
                {
                    cpu = pcCpuLoad.NextValue();
                }
                sb.Append("runinfo:");
                sb.Append(string.Format("当前处理器使用率:{0}%", cpu.ToString("N0")));
                //sb.Append(string.Format(",当前内存使用率:{0}%", memInfo.dwMemoryLoad));
                Process currentProcess = Process.GetCurrentProcess();

                string memory = (currentProcess.WorkingSet64 / 1024 / 1024).ToString() + "M (" + (currentProcess.WorkingSet64 / 1024).ToString() + "KB)";
                string threadCount = currentProcess.Threads.Count.ToString();

                sb.Append(string.Format(",总的处理器时间:{0}分钟", currentProcess.TotalProcessorTime.TotalMinutes.ToString("N2")));
                sb.Append(string.Format(",用户处理器时间:{0}分钟", currentProcess.UserProcessorTime.TotalMinutes.ToString("N2")));
                sb.Append(string.Format(",占用内存数:{0},开启线程数:{1},用户对象:{2}", memory, threadCount, currentProcess.HandleCount));
                LogHelper.Info(sb.ToString());
            }
            catch (Exception e) { }
        }     

        /// <summary>
        /// 操作系统信息
        /// </summary>
        public static void sysInfo()
        {
            StringBuilder sb = new StringBuilder();
            try
            {


                //Win32Helper.MEMORY_INFO memInfo = new Win32Helper.MEMORY_INFO();
                //Win32Helper.GlobalMemoryStatus(ref memInfo);
                sb.Append(string.Format("FrameWork版本号:{0}[{1}]", Environment.Version, MachineHelper.GetNetFramework()));
                sb.Append(string.Format(",操作系统版本号:{0},{1}", MachineHelper.GetOperatingSystem(), MachineHelper.GetSystemType()));
                sb.Append(string.Format(",Cpu个数:{0}个,主频:{1}", Environment.ProcessorCount, MachineHelper.GetCpuSpeed()));
                sb.Append(string.Format(",是否是64位处理器:{0},操作系统:{1}位", MachineHelper.Is64BitProcess(), MachineHelper.GetOSBit()));
                sb.Append(string.Format(",是否是管理员权限:{0}", MachineHelper.IsAdministrator()));
                sb.Append(string.Format(",物理内存:{0}GB", MachineHelper.GetTotalPhysicalMemory()));
                //sb.Append(string.Format(",交换文件总大小:{0}GB", Win32Helper.ConvertBytes(memInfo.dwTotalPageFile.ToString(), 3)));
                sb.Append(string.Format(",分辨率:{0}", MachineHelper.GetResolution()));
                sb.Append(string.Format(",Dpi:{0}", MachineHelper.GetDpi()));

                sb.Append(string.Format(",微软雅黑:{0}", MachineHelper.GetFont()));
                sb.AppendLine(string.Format("\t计算机名称:{0}", Environment.MachineName));
                sb.AppendLine(string.Format("\t当前登录用户名:{0}", Environment.UserName));
                sb.AppendLine(string.Format("\t当前服务器时间:{0}", Common.Utils.DateTimeEx.ServerTime.ToString("yyyy-MM-dd HH:mm:ss.fff")));


            }
            catch (Exception e)
            {
                sb.AppendLine("\t" + e.StackTrace);
            }
            LogHelper.Info(sb.ToString());
        }
        public static int PingCount = 20;
        private static void pingoTest(string host, StringBuilder sb)
        {
            try
            {
                sb.AppendLine("网络信息:");
                for (int i = 0; i < PingCount; i++)
                {
                    var p = new Ping();
                    PingReply reply = p.Send(host);
                    if (reply != null)
                    {
                        if (reply.Status == IPStatus.Success)
                        {
                            string msg = string.Format("\t来自 {0} 的回复: 字节={1} 时间={2}ms TTL={3}", reply.Address,
                                                       reply.Buffer.Length, reply.RoundtripTime, reply.Options.Ttl);
                            sb.AppendLine(msg);
                        }
                        else
                        {
                            string msg = string.Format("\tIPStatus:{0}", reply.Status.ToString());
                            sb.AppendLine(msg);
                        }
                    }
                }

            }
            catch (Exception e)
            {
                sb.AppendLine("\t" + e.StackTrace);
            }

        }
    }
}
