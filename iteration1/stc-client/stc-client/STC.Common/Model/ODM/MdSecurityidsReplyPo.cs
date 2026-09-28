using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.ODM
{
    /// <summary>
    /// ODM债券订阅列表
    /// </summary>
    public class MdSecurityidsReplyPo : BaseReplyPo
    {
        public List<StcTbookMdSecurityids> datas;
        public int totalSize;
    }
}
