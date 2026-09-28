using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.Cfg
{
    public class ServerSection : ConfigurationElement
    {
        [ConfigurationProperty("Id", IsRequired = true)]
        public string Id
        {
            get { return base["Id"].ToString(); }
            set { base["Id"] = value; }
        }

        [ConfigurationProperty("Name", IsRequired = true)]
        public string Name
        {
            get { return base["Name"].ToString(); }
            set { base["Name"] = value; }
        }

        [ConfigurationProperty("Host", IsRequired = true)]
        public string Host
        {
            get { return base["Host"].ToString(); }
            set { base["Host"] = value; }
        }

        [ConfigurationProperty("Port", IsRequired = true)]
        public string Port
        {
            get { return base["Port"].ToString(); }
            set { base["Port"] = value; }
        }

        [ConfigurationProperty("Protocol", IsRequired = true)]
        public string Protocol
        {
            get { return base["Protocol"].ToString(); }
            set { base["Protocol"] = value; }
        }

        [ConfigurationProperty("DpsHost", IsRequired = false)]
        public string DpsHost
        {
            get { return base["DpsHost"].ToString(); }
            set { base["DpsHost"] = value; }
        }

        [ConfigurationProperty("DpsPort", IsRequired = false)]
        public string DpsPort
        {
            get { return base["DpsPort"].ToString(); }
            set { base["DpsPort"] = value; }
        }

        [ConfigurationProperty("DpsProtocol", IsRequired = false)]
        public string DpsProtocol
        {
            get { return base["DpsProtocol"].ToString(); }
            set { base["DpsProtocol"] = value; }
        }

        [ConfigurationProperty("UpdateHost", IsRequired = false)]
        public string UpdateHost
        {
            get { return base["UpdateHost"].ToString(); }
            set { base["UpdateHost"] = value; }
        }

        [ConfigurationProperty("UpdatePort", IsRequired = false)]
        public string UpdatePort
        {
            get { return base["UpdatePort"].ToString(); }
            set { base["UpdatePort"] = value; }
        }

        [ConfigurationProperty("UpdateProtocol", IsRequired = false)]
        public string UpdateProtocol
        {
            get { return base["UpdateProtocol"].ToString(); }
            set { base["UpdateProtocol"] = value; }
        }
    }
}
