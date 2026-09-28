using STC.Common.Model.CRM;
using STC.WorkBench.Plugin.Utils.Data.CRM.Model.CRM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.WorkBench.Plugin.Utils.Comparer
{
    public class TraderComparer : IComparer<CrmContact>
    {
        private string currentSearchText;

        public TraderComparer(string searchText)
        {
            currentSearchText = searchText;
        }

        public int Compare(CrmContact x, CrmContact y)
        {
            if (x.namecn.Length == y.namecn.Length)
                return x.namecn.IndexOf(currentSearchText).CompareTo(y.namecn.IndexOf(currentSearchText));
            else
                return x.namecn.Length.CompareTo(y.namecn.Length);
        }
    }
}
