using STC.InnerBoard.Plugin.ViewModel.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.InnerBoard.Plugin.ViewModel.Comparer
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
                        result = x.CreateTime.CompareTo(y.CreateTime);
                    }
                    else
                    {
                        result = y.FirmFlag.CompareTo(x.FirmFlag);
                    }
                }
                else
                {
                    int flag = x.Price.Value.CompareTo(y.Price.Value);
                    result = _side == "0" ? flag : -flag;
                }
            }
            else
            {
                if (!x.Price.HasValue && !y.Price.HasValue)
                {
                    if (x.FirmFlag == y.FirmFlag)
                    {
                        result = x.CreateTime.CompareTo(y.CreateTime);
                    }
                    else
                    {
                        result = y.FirmFlag.CompareTo(x.FirmFlag);
                    }
                }
                else if (!x.Price.HasValue && y.Price.HasValue)
                {
                    result = 1;
                }
                else if (x.Price.HasValue && !y.Price.HasValue)
                {
                    result = -1;
                }
            }

            if (result == 0)
            {
                result = x.OrderID.CompareTo(y.OrderID);
            }

            return result;
        }
    }
}
