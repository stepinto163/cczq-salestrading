using STC.Common.Model.Fix.Message;
using STC.Common.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.XBond.Plugin.ViewModel.Comparer
{
    /// <summary>
    /// 价格排序器
    /// </summary>
    public class PriceSortComparer : IComparer<GNoMDEntries>
    {
        int _SortDirection = 1;
        bool _IsShowNetPrice = false;

        public PriceSortComparer(int sortDirection, bool isShowNetPrice)
        {
            this._SortDirection = sortDirection;
            this._IsShowNetPrice = isShowNetPrice;
        }

        public int Compare(GNoMDEntries a, GNoMDEntries b)
        {
            int sortInx = 0;
            if (_IsShowNetPrice)
            {
                decimal a_decimal = CommonUtils.ConvertToDecimal(a.NetPrice);
                decimal b_decimal = CommonUtils.ConvertToDecimal(b.NetPrice);

                sortInx = a_decimal.CompareTo(b_decimal) * _SortDirection;
            }
            else
            {
                decimal a_decimal = CommonUtils.ConvertToDecimal(a.MDEntryPx);
                decimal b_decimal = CommonUtils.ConvertToDecimal(b.MDEntryPx);

                sortInx = a_decimal.CompareTo(b_decimal) * _SortDirection;
            }
            if (sortInx == 0)
            {
                return a.Venues.CompareTo(b.Venues);
            }
            return sortInx;
        }
    }
}
