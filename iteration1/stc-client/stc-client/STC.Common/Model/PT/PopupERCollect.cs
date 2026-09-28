using STC.Common.Model.Fix.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.PT
{
    public class PopupERCollect
    {
        public List<ExecutionReport> list = new List<ExecutionReport>();
        public bool isUpdate;
        /// <summary>
        /// 来自Tbook订单
        /// </summary>
        public bool isFromTbook = false;
        /// <summary>
        /// 从 交易处理 中点开
        /// </summary>
        public bool isFromHistory = false;
    }
}
