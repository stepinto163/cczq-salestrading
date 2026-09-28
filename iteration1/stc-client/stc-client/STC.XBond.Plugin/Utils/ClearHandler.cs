using STC.Common.Model.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.XBond.Plugin.Utils
{
    /// <summary>
    /// 日末清零回调处理
    /// </summary>
    public class ClearHandler
    {
        private static ClearHandler handler = new ClearHandler();
        public static ClearHandler Instance { get { return handler; } }

        public event Action<ClearPo> OnClear;

        private ClearHandler() { }

        public void NotifyClearMsg(ClearPo clearPo)
        {
            OnClear?.Invoke(clearPo);
        }
    }
}
