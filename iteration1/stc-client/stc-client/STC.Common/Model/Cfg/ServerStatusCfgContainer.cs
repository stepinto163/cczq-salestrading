using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.Cfg
{
    /// <summary>
    /// 配置文件管理类(服务连接状态)
    /// </summary>
    public class ServerStatusCfgContainer : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new ServerStatusSection();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return (element as ServerStatusSection).Id;
        }
    }
}
