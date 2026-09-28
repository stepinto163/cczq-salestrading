using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.Cfg
{
    public class IsDcsBrokerCfgSection : ConfigurationSection
    {
        [ConfigurationProperty("IsDcsBrokerGroup", IsRequired = true)]
        [ConfigurationCollection(typeof(IsDcsBrokerSection), CollectionType = ConfigurationElementCollectionType.AddRemoveClearMap, AddItemName = "IsDcsBroker")]
        public IsDcsBrokerCfgContainer DcsBroker
        {
            get { return base["IsDcsBrokerGroup"] as IsDcsBrokerCfgContainer; }
            set { base["IsDcsBrokerGroup"] = value; }

        }
    }
}
