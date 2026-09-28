using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Frame.Utils.Cfg
{
    public class IsDcsBrokerSection : ConfigurationElement
    {
        [ConfigurationProperty("Id", IsRequired = true)]
        public string Id
        {
            get { return (base["Id"].ToString()); }
            set { base["Id"] = value; }
        }

        [ConfigurationProperty("Broker", IsRequired = true)]
        public string Broker
        {
            get { return (base["Broker"].ToString()); }
            set { base["Broker"] = value; }
        }

        [ConfigurationProperty("Name", IsRequired = true)]
        public string Name
        {
            get { return (base["Name"].ToString()); }
            set { base["Name"] = value; }
        }

        [ConfigurationProperty("Status", IsRequired = true)]
        public string Status
        {
            get { return (base["Status"].ToString()); }
            set { base["Status"] = value; }
        }
    }
}
