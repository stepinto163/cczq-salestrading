using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.Cfg
{
    public class RemoteServerContainer : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new RemoteServer();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return (element as RemoteServer).Id;
        }
    }
}
