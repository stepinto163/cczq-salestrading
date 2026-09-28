using com.gateway.log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Frame.Update
{
    public class HostHelper
    {
        public static string ReplaceHost { get; set; }
        public static int ReplacePort { get; set; }
        public static string ReplaceProtocol { get; set; }


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

                    if (ReplacePort != 0)
                        uriBuilder.Port = ReplacePort;

                    if (!string.IsNullOrEmpty(ReplaceProtocol))
                        uriBuilder.Scheme = ReplaceProtocol;

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

        /// <summary>
        /// 替换IP为登录IP
        /// </summary>
        /// <param name="originalUrl"></param>
        /// <returns></returns>
        public static string ReplaceIp(string originalUrl)
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
                    // 获取更新后的URL
                    updatedUrl = uriBuilder.Uri.ToString();
 
                    //LogHelper.Info(string.Format("originalUrl:{0}, newUrl:{1}", originalUrl, updatedUrl));
                }
                catch (Exception e)
                {

                }
            }

            //LogHelper.Info("ReplaceIp Url:" + updatedUrl);

            return updatedUrl;
        }
    }
}
