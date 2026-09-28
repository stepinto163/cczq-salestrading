
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BDB.Trade.Plugin.Handler.DataProcesser
{
    public class ExecOrderCache
    {
        private static ExecOrderCache _execOrderCache = new ExecOrderCache();

        public static ExecOrderCache Instance
        {
            get
            {
                return _execOrderCache;
            }
        }

        private ExecOrderCache()
        { }

        
    }
}
