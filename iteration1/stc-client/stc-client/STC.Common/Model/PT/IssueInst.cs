using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.PT
{
    /// <summary>
    /// 机构信息
    /// </summary>
    public class IssueInst
    {
        public long id;
        //分组id
        public string groupId;
        //机构id
        public string instId;
        //机构简称
        public string instShortName;
        //机构全称
        public string instFullName;
        //机构交易员信息
        public List<IssueInstTrader> instTraders;
        //备注
        public string remark;
        //创建人
        public string createby;
        public string createtime;//创建时间
        public string updateby;//更新人
        public string updatetime;//更新时间
        /**
         * 操作类型：0-查询；1-新增；2-修改；3-删除
         */
        public string action;

        /// <summary>
        /// “默认请求”勾选项：1-是；2-否; operateType为3必填
        /// </summary>
        public string defaultRequest { get; set; }
    }
}
