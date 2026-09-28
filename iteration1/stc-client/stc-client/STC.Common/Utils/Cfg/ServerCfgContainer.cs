using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Utils.Cfg
{
    /// <summary>
    /// 配置文件管理类(连接服务器相关信息)
    /// </summary>
    public class ServerCfgContainer : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new ServerSection();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return (element as ServerSection).Id;
        }
    }
}
