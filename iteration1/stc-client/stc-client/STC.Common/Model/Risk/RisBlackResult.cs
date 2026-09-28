using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.Risk
{
    /// <summary>
    /// 下载查询所有黑名单返回结果
    /// </summary>
    public class RisBlackResult : BaseReplyPo
    {
        public List<RisBlackModel> Datas { get; set; }
    }
}
