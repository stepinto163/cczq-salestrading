using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.PT
{
    public class GrExecOrderQueryReq
    {
        public string execid;
        public string userName;
        public string dealTimeBegin;
        public string dealTimeEnd;
        public string tradedateBegin { get; set; }
        public string createTimeEnd { get; set; }

        /// <summary>
        /// "Y"：只看自己
        /// </summary>
        public string ownOrder { get; set; }
        /// <summary>
        /// 债券代码
        /// </summary>
        public string bondCode { get; set; }
        /// <summary>
        /// 机构Id
        /// </summary>
        public string bankId { get; set; }
        /// <summary>
        /// 交易员Id
        /// </summary>
        public string bankAgentId { get; set; }
        /// <summary>
        /// 排序方向
        /// A Ascending 剩序
        /// D Descending 降序
        /// </summary>
        public string sortDirection { get; set; }
        /// <summary>
        /// 排序字段
        /// </summary>
        public string sortField { get; set; }
        /// <summary>
        /// 剩余期限排序顺序 (A [ASC] - 从短到长正序, D [DESC] - 从长到短逆序)
        /// </summary>
        public string maturityOrder { get; set; }

        public int pageNum = 0;
        public int pageSize = 5000;


        /// <summary>
        /// 机构bID
        /// </summary>
        public string counterpartyInstitutionid { get; set; }
        /// <summary>
        /// 交易员B Id
        /// </summary>
        public string counterpartyTraderid { get; set; }

        /// <summary>
        /// 机构bID
        /// </summary>
        public string createTimeBegin { get; set; }

        /// <summary>
        /// 机构A  ID
        /// </summary>
        public string institutionid { get; set; }

        /// <summary>
        /// 债券代码
        /// </summary>
        public string securityid { get; set; }

        /// <summary>
        /// 交易员Id
        /// </summary>
        public string traderid { get; set; }

        /// <summary>
        /// 操作类型:交易处理后状态
        /// </summary>
        public List<int> commandTypes;

        /// <summary>
        /// 交易员Id
        /// </summary>
        public string queryHistType { get; set; }

        public string updateTimeEnd { get; set; }


        public string updateTimeBegin { get; set; }

    }
}
