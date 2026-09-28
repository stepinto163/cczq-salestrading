using com.gateway.log;
using STC.Common.Utils.Dps;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace STC.Common.Utils
{
    public class ProcessWatcher
    {
        public string WatcherProName { get; set; } = "dps";

        private com.gateway.connector.tcp.ProxyInfo _proxy;

        public com.gateway.connector.tcp.ProxyInfo Proxy
        {
            get { return _proxy; }
            set 
            { 
                _proxy = value;
                if (objProcessRestart != null)
                {
                    objProcessRestart.ProxyInfo = value;
                }
            }
        }

        private ProcessRestart objProcessRestart;

        public void start()
        {
            LogHelper.Info("start:{0},{1}", WatcherProName, Proxy);
            scanProcessList(WatcherProName, Proxy);

        }
        public void stop()
        {
            Process[] arrayProcess = Process.GetProcesses();
            string pName = WatcherProName;
            if (!string.IsNullOrEmpty(WatcherProName)
                && WatcherProName.ToLower().EndsWith(".exe"))
            {
                pName = WatcherProName.Substring(0, WatcherProName.Length - 4);
            }
            foreach (Process p in arrayProcess)
            {
                if (p.ProcessName.Equals(pName))
                {
                    try
                    {
                        p.Kill();
                        return;
                    }
                    catch
                    {

                    }
                }
            }
        }
        private void processStart(Process process, string address)
        {
            process.StartInfo.FileName = address;
            process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            process.StartInfo.Arguments = ProxyUtils.getArgs(Proxy);
            process.Start();
        }
        private void scanProcessList(string address, com.gateway.connector.tcp.ProxyInfo proxy)
        {
            address = System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory ,address);
            Process[] arrayProcess = Process.GetProcesses();
            foreach (Process p in arrayProcess)
            {
                if (p.ProcessName.Equals(WatcherProName))
                {
                    try
                    {
                        this.WatchProcess(p, address, proxy);
                        return;
                    }
                    catch
                    {

                    }
                }
            }

            Process process = new Process();
            try
            {
                //process.StartInfo.FileName = address;
                //process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                //process.StartInfo.Arguments = $"-gh {LocalEnv.ServerInfo.DpsHost} -gp {LocalEnv.ServerInfo.DpsPort} -gt {LocalEnv.ServerInfo.DpsProtocal}";
                //process.Start();
				 processStart(process, address);
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(address + ":" + ex.Message);
            }
            this.WatchProcess(process, address, proxy);

        }

        private void WatchProcess(Process process, string address, com.gateway.connector.tcp.ProxyInfo proxy)
        {
            if (objProcessRestart == null)
            {
                objProcessRestart = new ProcessRestart(process, address, proxy);
            }
            Thread thread = new Thread(new ThreadStart(objProcessRestart.RestartProcess));
            thread.Start();
            ProcessSync ps = new ProcessSync(WatcherProName);
            //ps.WaitOne(5000);
            ps.WaitOne(5 * 1000);

        }
        private string FormatPath(string path)
        {
            return path.ToLower().Trim().TrimEnd('\\');
        }
    }
    internal class ProcessRestart
    {
        private Process _process;
        private string _address;
        private com.gateway.connector.tcp.ProxyInfo _proxy;

        public com.gateway.connector.tcp.ProxyInfo ProxyInfo
        {
            get { return _proxy; }
            set { _proxy = value; }
        }

        public ProcessRestart()
        { }

        public ProcessRestart(Process process, string address, com.gateway.connector.tcp.ProxyInfo proxy)
        {
            this._process = process;
            this._address = address;
            this._proxy = proxy;
        }


        public void RestartProcess()
        {
            try
            {
                while (true)
                {

                    _process.WaitForExit();
                    _process.Close();    //释放已退出进程的句柄
                    _process.StartInfo.FileName = this._address;
                    //_process.StartInfo.Arguments = $"-gh {LocalEnv.ServerInfo.DpsHost} -gp {LocalEnv.ServerInfo.DpsPort} -gt {LocalEnv.ServerInfo.DpsProtocal}";
                    _process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    _process.StartInfo.Arguments = ProxyUtils.getArgs(_proxy);
                    _process.Start();

                    Thread.Sleep(1000);
                }
            }
            catch
            {

            }
        }
    }
}
