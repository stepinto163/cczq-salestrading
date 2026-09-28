using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.Table
{
    public class FQInfo
    {
        public string fromCorpName;
        public string fromName;
        public string fromQQ;
        public string source;
        public string toCorpName;
        public string toGroupOpenID;
        public string toName;
        public string toQQ;
        public string toGroupName;
        public string toQQGroup;

        public string ADD_WHITE_LIST;//是否显示加号
        public string inquiryStatus;// 是否有询价 0：没有 1： 有

        public string MANAGER_ASSISTANTS;
        public string MATCHED_QUANTITY;
        public string INTENTION_GROUP_KEY;
        public string SUB_INTENTIONIDS;
        public string StrategyOrderId;
    }
}
