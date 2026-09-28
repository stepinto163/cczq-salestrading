using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.Ans
{
    public class ANSMonitorInfo
    {
        public string venue;// 渠道
        public string securityId;// 债券代码
        public string settleType;//清算类型
        public long quoteTimeLength;//报价时长（分钟）
        public string pauseStatus;// 暂停状态 0-已暂停 1-正在报价或为终态（已达标、失败）
        public string monitorStatus;//监控状态:1危险，2可达标，3已达标，4失败 。 注意：3、4 为终态 ， 1、2为非终态
        public string updateTime;// 更新时间
        public string newBondSign;// 新发国债标识 0-不是新发国债，1-是新发国债
    }
}
