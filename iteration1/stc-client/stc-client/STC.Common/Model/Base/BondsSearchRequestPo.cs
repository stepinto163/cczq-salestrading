using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.Base
{
    /// <summary>
    /// 债券分页接口请求参数
    /// </summary>
    public class BondsSearchRequestPo
    {
        public int PageNum;
        public int PageSize;
        public string BondModifyDate;
        public string BondListInfoModifyDate;
        public string[] BondSubtypes;
        public string BondMasterModifyDate;
    }
}
