using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.BookManagement
{
    public class BlockQueryReq
    {
        public String parentflag = "1"; // 订单类型
        public String blocknum; //包编号
        public String securityid;// 债券代码
        public String exectype; //包状态
        public String terminal; //成交方式
        public String execid;// 成交单号
        public String side; //方向
       
        public String partysubname; //交易员
        public String dealTimeBegin; //开始时间 YYYYMMDD
        public String dealTimeEnd; //结束时间 YYYYMMDD
        public int pageNum;// 从0开始
        public int pageSize;// = 5000
        public String userid;
        public String username;
    }

}
