using MDT.Tools.Wpf.AutoUpdater.Proxy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace MDT.Tools.Wpf.AutoUpdater
{
    public class HttpWebClient : WebClient
    {
        private int timeOut = 30;
        public int TimeOut
        {
            get { return timeOut; }
            set { timeOut = value; }
        }

        protected override WebRequest GetWebRequest(Uri address)
        {
            HttpWebRequest request = (HttpWebRequest)base.GetWebRequest(address);
            request.Proxy = null;
            request.Timeout = 1000 * TimeOut;
            request.ReadWriteTimeout = 1000 * TimeOut;
            request.KeepAlive = false;
            request.ProtocolVersion = HttpVersion.Version10;

            if (ProxyUtils.ProxyInfo != null)
            {
                LogHelper.Info("HttpWebClient proxyType:" + ProxyUtils.ProxyInfo.ProxyType
                    + ", host:" + ProxyUtils.ProxyInfo.ProxyHost + ", port:" + ProxyUtils.ProxyInfo.ProxyPort
                    + ", user:" + ProxyUtils.ProxyInfo.ProxyUser + ", pwd:" + ProxyUtils.ProxyInfo.ProxyPassword);

                switch (ProxyUtils.ProxyInfo.ProxyType)
                {
                    case ProxyType.http:
                        LogHelper.Info("http");
                        if (!string.IsNullOrEmpty(ProxyUtils.ProxyInfo.ProxyHost) && ProxyUtils.ProxyInfo.ProxyPort > 0)
                        {
                            request.Proxy = this.Proxy = new WebProxy(ProxyUtils.ProxyInfo.ProxyHost, ProxyUtils.ProxyInfo.ProxyPort);

                            if (!string.IsNullOrEmpty(ProxyUtils.ProxyInfo.ProxyUser) && !string.IsNullOrEmpty(ProxyUtils.ProxyInfo.ProxyPassword))
                            {
                                if (request.Proxy != null)
                                {
                                    request.Proxy.Credentials = new NetworkCredential(ProxyUtils.ProxyInfo.ProxyUser, ProxyUtils.ProxyInfo.ProxyPassword);
                                }
                            }
                            else
                            {
                                if (request.Proxy != null)
                                {
                                    request.Proxy.Credentials = CredentialCache.DefaultCredentials;
                                }

                                if (WebRequest.DefaultWebProxy != null)
                                {
                                    WebRequest.DefaultWebProxy.Credentials = CredentialCache.DefaultCredentials;
                                }
                            }
                        }
                        break;

                    case ProxyType.IE:
                        LogHelper.Info("ie");
                        request.Proxy = this.Proxy = WebRequest.GetSystemWebProxy();
                        if (!string.IsNullOrEmpty(ProxyUtils.ProxyInfo.ProxyUser) && !string.IsNullOrEmpty(ProxyUtils.ProxyInfo.ProxyPassword))
                        {
                            if (request.Proxy != null)
                            {
                                request.Proxy.Credentials = new NetworkCredential(ProxyUtils.ProxyInfo.ProxyUser, ProxyUtils.ProxyInfo.ProxyPassword);
                            }
                        }
                        else
                        {
                            if (WebRequest.DefaultWebProxy != null)
                            {
                                WebRequest.DefaultWebProxy.Credentials = CredentialCache.DefaultCredentials;
                            }
                            if (request.Proxy != null)
                            {
                                request.Proxy.Credentials = CredentialCache.DefaultCredentials;
                            }
                        }
                        break;

                    case ProxyType.None:
                        LogHelper.Info("none");
                        request.Proxy = this.Proxy = WebRequest.GetSystemWebProxy();
                        LogHelper.Info("none1");
                        if (!string.IsNullOrEmpty(ProxyUtils.ProxyInfo.ProxyUser) && !string.IsNullOrEmpty(ProxyUtils.ProxyInfo.ProxyPassword))
                        {
                            LogHelper.Info("none2");
                            if (request.Proxy != null)
                            {
                                request.Proxy.Credentials = new NetworkCredential(ProxyUtils.ProxyInfo.ProxyUser, ProxyUtils.ProxyInfo.ProxyPassword);
                            }
                        }
                        else
                        {
                            LogHelper.Info("none3");
                            if (request.Proxy != null)
                            {
                                request.Proxy.Credentials = CredentialCache.DefaultCredentials;
                            }
                            if (WebRequest.DefaultWebProxy != null)
                            {
                                WebRequest.DefaultWebProxy.Credentials = CredentialCache.DefaultCredentials;
                            }
                        }
                        LogHelper.Info("none4");
                        break;

                    default:
                        request.Proxy = null;
                        break;
                }
            }

            return request;
        }
    }

}
