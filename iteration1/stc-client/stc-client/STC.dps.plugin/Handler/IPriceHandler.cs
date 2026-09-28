using STC.Common.Model.Fix.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.dps.plugin.Handler
{
    public interface IPriceHandler
    {
        void Subscribe(string securityId, string settle);
    }
}
