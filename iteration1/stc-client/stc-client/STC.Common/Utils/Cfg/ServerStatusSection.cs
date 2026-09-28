using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Utils.Cfg
{
    /// <summary>
    /// 配置文件管理类(服务连接状态)
    /// </summary>
    public class ServerStatusSection : ConfigurationElement
    {
        [ConfigurationProperty("Id", IsRequired = true)]
        public string Id
        {
            get { return (base["Id"].ToString()); }
            set { base["Id"] = value; }
        }

        [ConfigurationProperty("VenueTypeGW", IsRequired = true)]
        public string VenueTypeGW
        {
            get { return (base["VenueTypeGW"].ToString()); }
            set { base["VenueTypeGW"] = value; }
        }

        [ConfigurationProperty("Name", IsRequired = true)]
        public string Name
        {
            get { return (base["Name"].ToString()); }
            set { base["Name"] = value; }
        }

        [ConfigurationProperty("TargetSessionId", IsRequired = true)]
        public string TargetSessionId
        {
            get { return (base["TargetSessionId"].ToString()); }
            set { base["TargetSessionId"] = value; }
        }
    }
}
