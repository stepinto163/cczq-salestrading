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
    public class ServerStatusCfgSection : ConfigurationSection
    {
        [ConfigurationProperty("ServerStatusGroup", IsRequired = true)]
        [ConfigurationCollection(typeof(ServerStatusSection), CollectionType = ConfigurationElementCollectionType.AddRemoveClearMap, AddItemName = "ServerStatus")]
        public ServerStatusCfgContainer ServerGroup
        {
            get { return base["ServerStatusGroup"] as ServerStatusCfgContainer; }
            set { base["ServerStatusGroup"] = value; }

        }
    }
}
