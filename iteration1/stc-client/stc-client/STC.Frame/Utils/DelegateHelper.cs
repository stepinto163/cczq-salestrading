using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.XBond.Plugin.Utils
{
    public delegate void VoidDelegate();

    public delegate void UpdateErDelegate(Common.Model.Fix.Message.ExecutionReport obj);

    public delegate void UpdateDataRowDelegate(DataRow dr);

    public delegate void UpdateDataTableDelegate(DataTable dt);
}
