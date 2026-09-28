using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.Cal
{
    public class CalWeekDateResult : BaseReplyPo
    {
        public WeekInfo Data;
    }


    public class WeekInfo
    {
        public string settlementType;
        public string baseDate;
        /// <summary>
        /// 计算出的正确的清算速度
        /// </summary>
        public string settlementType2;
        public string dealDate;
        public string settlementDate;
       
    }
}
