using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BDB.Trade.Plugin.Utils
{
    public class IdcHelper
    {
        private static IdcHelper _idcHelper = new IdcHelper();
        public static IdcHelper Instance { get { return _idcHelper; } }

        private IdcHelper()
        { }

        public void IdcDataTransfer()
        {
        }
    }
}
