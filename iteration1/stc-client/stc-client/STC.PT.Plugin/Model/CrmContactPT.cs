using STC.Common.Model.CRM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.PT.Plugin.Model
{
    public class CrmContactPT:CrmContact
    {
        public string DisplayName => $"{namecn} ({id})";
    }
}
