using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.Table.WorkBrench
{
    public class TableIntentionExecution
    {
        public string id;

        public string intentionId;

        public string executionId;

        public string securityId;

        public string side;

        /**
         * 关联量
         */
        public decimal quantity;

        //交易员名称
        public string partyTrader;

        //投资经理
        public string partyManager;

        //交易员名称(中文名)
        public string partyTraderName;

        //投资经理(中文名)
        public string partyManagerName;

        //分组id
        public string bookId;

        //分组名称
        public string bookName;

        public string accountID;

        public string accountName;

        //扩展字段
        public string info1;

        public string info2;

        public string info3;

        public string info4;

        public string info5;

        public string createTime;

        public string updateTime;

        public string intentionStatus; //意向状态

        public string executionStatus; //成交状态

        public string delflag; //删除标记

        //操作类型，新增:0、修改:1、删除:2
        public string action;
    }
}
