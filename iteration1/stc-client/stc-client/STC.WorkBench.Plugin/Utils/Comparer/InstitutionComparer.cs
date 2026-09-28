using STC.Common.Model.CRM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.WorkBench.Plugin.Utils.Comparer
{
    public class InstitutionComparer : IComparer<CrmInst>
    {
        private string currentSearchText;

        public InstitutionComparer(string searchText)
        {
            currentSearchText = searchText;
        }

        public int Compare(CrmInst x, CrmInst y)
        {
            if (x.shortnamecn.Length == y.shortnamecn.Length)
                return x.shortnamecn.IndexOf(currentSearchText).CompareTo(y.shortnamecn.IndexOf(currentSearchText));
            else
                return x.shortnamecn.Length.CompareTo(y.shortnamecn.Length);
        }
    }
}
