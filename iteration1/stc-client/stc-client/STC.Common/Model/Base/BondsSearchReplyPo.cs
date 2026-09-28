using STC.Common.Model.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.Base
{
    /// <summary>
    /// 债券分页查询接口请求结果
    /// </summary>
    public class BondsSearchReplyPo:BaseReplyPo
    {
        public List<BondsInfoPo> datas;
        public int Total;
    }
}
