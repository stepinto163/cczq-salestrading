using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.Cfg
{
    public class RemoteServer : ConfigurationElement
    {
        [ConfigurationProperty("Id", IsRequired = true)]
        public string Id
        {
            get { return (base["Id"].ToString()); }
            set { base["Id"] = value; }
        }

        [ConfigurationProperty("Name", IsRequired = true)]
        public string Name
        {
            get { return (base["Name"].ToString()); }
            set { base["Name"] = value; }
        }

        [ConfigurationProperty("Host", IsRequired = true)]
        public string Host
        {
            get { return (base["Host"].ToString()); }
            set { base["Host"] = value; }
        }

        [ConfigurationProperty("Port", IsRequired = true)]
        public string Port
        {
            get { return (base["Port"].ToString()); }
            set { base["Port"] = value; }
        }

        [ConfigurationProperty("Protocol", IsRequired = true)]
        public string Protocol
        {
            get { return (base["Protocol"].ToString()); }
            set { base["Protocol"] = value; }
        }
    }
}
