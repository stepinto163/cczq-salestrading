using STC.IRS.Plugin.ViewModel.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.IRS.Plugin.ViewModel.Comparer
{
    public class PriceComparer : IComparer<InnerOrderData>
    {
        private string _side;
        public PriceComparer(string side)
        {
            _side = side;
        }


        public int Compare(InnerOrderData x, InnerOrderData y)
        {
            int result = 0;
            if (x.Price.HasValue && y.Price.HasValue)
            {
                if (x.Price == y.Price)
                {
                    if (x.FirmFlag == y.FirmFlag)
                    {
                        return x.CreateTime.CompareTo(y.CreateTime);
                    }
                    else
                    {
                        return y.FirmFlag.CompareTo(x.FirmFlag);
                    }
                }
                else
                {
                    result = x.Price.Value.CompareTo(y.Price.Value);
                    return _side == "0" ? result : -result;
                }
            }
            else
            {
                if (!x.Price.HasValue && !y.Price.HasValue)
                {
                    if (x.FirmFlag == y.FirmFlag)
                    {
                        return x.CreateTime.CompareTo(y.CreateTime);
                    }
                    else
                    {
                        return y.FirmFlag.CompareTo(x.FirmFlag);
                    }
                }
                if (!x.Price.HasValue && y.Price.HasValue)
                {
                    return 1;
                }
                if (x.Price.HasValue && !y.Price.HasValue)
                {
                    return -1;
                }
            }
            return result;
        }
    }
}
