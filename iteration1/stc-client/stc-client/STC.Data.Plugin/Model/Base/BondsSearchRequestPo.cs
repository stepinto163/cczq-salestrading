using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Data.Plugin.Model.Base
{
    public class BondsSearchRequestPo
    {
        public int PageNum;
        public int PageSize;
        public string BondModifyDate;
        public string BondListInfoModifyDate;
        public string[] BondSubtypes;

    }
}
