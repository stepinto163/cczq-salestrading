using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.Table.WorkBrench
{
    public class IntentionExecutionAssociateReq
    {
        public List<TableIntentionExecution> requestList;

        public List<string> intentionIds;

        public List<string> executionIds;

        //操作类型 0:查询 1:关联
        public string action;
    }
}
