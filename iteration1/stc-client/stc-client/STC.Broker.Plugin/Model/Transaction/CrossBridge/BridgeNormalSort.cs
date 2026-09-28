using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Broker.Plugin.Model.Transaction.CrossBridge
{
    /// <summary>
    /// 桥一般比较
    /// </summary>
    public class BridgeNormalSort : IComparer<BridgeOrderDetail>
    {
        public int Compare(BridgeOrderDetail x, BridgeOrderDetail y)
        {
            string createtimeX = string.Empty;
            string createtimeY = string.Empty;

            if (x.BridgeType.Equals("B,O"))
                createtimeX = x.BridgeCreateTime == null ? string.Empty : x.BridgeCreateTime;
            else
            {
                if (x.isBidBridge.Equals("1"))
                    createtimeX = x.bridgeOrder2?.createTime + "";
                else if (x.isBidBridge.Equals("2"))
                    createtimeX = x.bridgeOrder?.createTime + "";
            }

            if (y.BridgeType.Equals("B,O"))
                createtimeY = y.BridgeCreateTime == null ? string.Empty : y.BridgeCreateTime;
            else
            {
                if (y.isBidBridge.Equals("1"))
                    createtimeY = y.bridgeOrder2?.createTime + "";
                else if (y.isBidBridge.Equals("2"))
                    createtimeY = y.bridgeOrder?.createTime + "";
            }

            if (createtimeX.CompareTo(createtimeY) != 0)
                return createtimeX.CompareTo(createtimeY);
            else
                return Convert.ToDecimal(x.seq).CompareTo(y.seq);
        }
    }
}
