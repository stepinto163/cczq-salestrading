using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDT.Tools.Wpf.AutoUpdater.Proxy
{
    public class ProxyInfo
    {
        public ProxyInfo()
        {
        }

        public ProxyInfo(ProxyType proxyType, string proxyHost, int proxyPort)
        {
            this.ProxyType = proxyType;
            this.ProxyHost = proxyHost;
            this.ProxyPort = proxyPort;
        }
        public ProxyInfo(ProxyType proxyType, string proxyHost, int proxyPort, string proxyUser, string proxyPassword)
        {
            this.ProxyType = proxyType;
            this.ProxyHost = proxyHost;
            this.ProxyPort = proxyPort;
            this.ProxyUser = proxyUser;
            this.ProxyPassword = proxyPassword;
        }

        public ProxyType ProxyType { get; set; }
        public string ProxyHost { get; set; }
        public int ProxyPort { get; set; }
        public string ProxyUser { get; set; }
        public string ProxyPassword { get; set; }
    }
}
