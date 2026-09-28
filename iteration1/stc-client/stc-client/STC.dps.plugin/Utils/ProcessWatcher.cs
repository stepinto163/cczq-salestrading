using System.Diagnostics;

namespace STC.dps.plugin.Utils
{
    public class ProcessWatcher
    {
        public static string WatcherProName { get; set; } = "qmessages";

        public static Process QmProcess;
        public static Process IsExitProess()
        {
            string address = System.AppDomain.CurrentDomain.BaseDirectory + "/" + WatcherProName;
            Process[] arrayProcess = Process.GetProcesses();
            foreach (Process p in arrayProcess)
            {
                if (p.ProcessName.Equals(WatcherProName))
                {
                    try
                    {
                        QmProcess = p;
                        return p;
                    }
                    catch
                    {

                    }
                }
            }
            return null;
        }

      




    }

}
