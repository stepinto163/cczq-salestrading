using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDT.Tools.Wpf.AutoUpdater.Utils
{
    public class HostHelper
    {
        public static string ReplaceHost { get; set; }
        public static int ReplacePort { get; set; }



        public static string ReplaceIpAndPort(string originalUrl)
        {
            string updatedUrl = originalUrl;
            if (!string.IsNullOrEmpty(originalUrl) && !string.IsNullOrEmpty(ReplaceHost))
            {
                try
                {
                    // 使用Uri类来解析URL
                    UriBuilder uriBuilder = new UriBuilder(originalUrl);

                    // 替换IP和端口
                    uriBuilder.Host = ReplaceHost;
                    //uriBuilder.Port = ReplacePort;

                    // 获取更新后的URL
                    updatedUrl = uriBuilder.Uri.ToString();

                    LogHelper.Info(string.Format("originalUrl:{0}, newUrl:{1}", originalUrl, updatedUrl));
                }
                catch (Exception e)
                {

                }
            }

            LogHelper.Info("ReplaceIpAndPort Url:" + updatedUrl);
            return updatedUrl;
        }

    }
}
