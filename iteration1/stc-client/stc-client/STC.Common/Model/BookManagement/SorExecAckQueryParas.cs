using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.BookManagement
{
    public class SorExecAckQueryParas
    {
        public String securityid;// 产品唯一编号
        public String venues;
        public String side; //方向
        public String partysubname; //交易员
        public String dealTimeBegin;//开始时间 YYYYMMDD
        public String dealTimeEnd;//结束时间 YYYYMMDD
        public int pageNum;
        public int pageSize;
        public String brokerExecID;
    }
}
