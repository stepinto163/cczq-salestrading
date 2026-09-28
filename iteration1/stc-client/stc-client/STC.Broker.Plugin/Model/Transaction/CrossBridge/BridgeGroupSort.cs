using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Broker.Plugin.Model.Transaction.CrossBridge
{
    /// <summary>
    /// 桥分组比较
    /// </summary>
    public class BridgeGroupSort : IComparer<BridgeOrderDetail>
    {
        public int Compare(BridgeOrderDetail x, BridgeOrderDetail y)
        {
            if (string.IsNullOrEmpty(x.SecurityID))
                return 1;
            else if (x.SecurityID.CompareTo(y.SecurityID) != 0)
                return x.SecurityID.CompareTo(y.SecurityID);
            else if (x.price == null)
                return 1;
            else if (x.price.Value.CompareTo(y.price.Value) != 0)
                return x.price.Value.CompareTo(y.price.Value);
            else if (string.IsNullOrEmpty(x.OfrInstNamePY))
                return 1;
            else if (x.OfrInstNamePY.CompareTo(y.OfrInstNamePY) != 0)
                return x.OfrInstNamePY.CompareTo(y.OfrInstNamePY);
            else if (string.IsNullOrEmpty(x.BidInstNamePY))
                return 1;
            else if (x.BidInstNamePY.CompareTo(y.BidInstNamePY) != 0)
                return x.BidInstNamePY.CompareTo(y.BidInstNamePY);
            else if (string.IsNullOrEmpty(x.DealDate2) || string.IsNullOrEmpty(y.DealDate2))
                return 1;
            else if (x.DealDate2.CompareTo(y.DealDate2) != 0)
                return x.DealDate2.CompareTo(y.DealDate2);
            return x.createTime.CompareTo(y.createTime);
        }
    }
}
