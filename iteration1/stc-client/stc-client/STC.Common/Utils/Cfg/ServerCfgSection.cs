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
    public class ServerCfgSection : ConfigurationSection
    {
        [ConfigurationProperty("ServerGroup", IsRequired = true)]
        [ConfigurationCollection(typeof(ServerSection), CollectionType = ConfigurationElementCollectionType.AddRemoveClearMap, AddItemName = "Server")]
        public ServerCfgContainer ServerGroup
        {
            get { return base["ServerGroup"] as ServerCfgContainer; }
            set { base["ServerGroup"] = value; }

        }
    }
}
