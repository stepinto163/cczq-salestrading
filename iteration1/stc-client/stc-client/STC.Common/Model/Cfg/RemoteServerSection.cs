using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.Cfg
{
    public class RemoteServerSection : ConfigurationSection
    {
        [ConfigurationProperty("RemoteServerGroup", IsRequired = true)]
        [ConfigurationCollection(typeof(RemoteServer), CollectionType = ConfigurationElementCollectionType.AddRemoveClearMap, AddItemName = "RemoteServer")]
        public RemoteServerContainer RemoteServerGroup
        {
            get { return base["RemoteServerGroup"] as RemoteServerContainer; }
            set { base["RemoteServerGroup"] = value; }

        }
    }
}
