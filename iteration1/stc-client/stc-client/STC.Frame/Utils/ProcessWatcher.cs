using com.gateway.log;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace STC.Frame.Utils
{
    public class ProcessWatcher
    {
        public string WatcherProName { get; set; } = "dps";


        public void start()
        {
            scanProcessList(WatcherProName);

        }
        public void stop()
        {
            Process[] arrayProcess = Process.GetProcesses();
            foreach (Process p in arrayProcess)
            {
                if (p.ProcessName.Equals(WatcherProName))
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
        private void scanProcessList(string address)
        {
            address = System.AppDomain.CurrentDomain.BaseDirectory + "/" + address;
            Process[] arrayProcess = Process.GetProcesses();
            foreach (Process p in arrayProcess)
            {
                if (p.ProcessName.Equals(WatcherProName))
                {
                    try
                    {
                        this.WatchProcess(p, address);
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

                process.StartInfo.FileName = address;
                process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                process.Start();
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(address + ":" + ex.Message);
            }
            this.WatchProcess(process, address);

        }

        private void WatchProcess(Process process, string address)
        {
            ProcessRestart objProcessRestart = new ProcessRestart(process, address);
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


        public ProcessRestart()
        { }



        public ProcessRestart(Process process, string address)
        {
            this._process = process;
            this._address = address;
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
                    _process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
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
