using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Frame.Utils.Cfg
{
    public class IsDcsBrokerCfgContainer : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new IsDcsBrokerSection();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return (element as IsDcsBrokerSection).Id;
        }
    }
}
