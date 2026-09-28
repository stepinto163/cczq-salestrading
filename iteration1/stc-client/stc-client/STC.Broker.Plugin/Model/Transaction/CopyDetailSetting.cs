using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Broker.Plugin.Model.Transaction
{
    public class CopyDetailSetting
    {
        /// <summary>
        /// 其他人成交单是否按照机构交易员拆分
        /// </summary>
        public string splitshowotherorders;

        /// <summary>
        /// 明细含公司名称展示
        /// </summary>
        public string WithCompanyName;

        /// <summary>
        /// 格式化机构设置 机构合并展示
        /// 示例:[{"name":"工商银行","userName":"2a4622f4346a647001346a7c172a00cb(contact表中的instCode"}
        /// </summary>
        public List<AccountInfo> FormatInst;
    }
}
