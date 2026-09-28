using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.Drp
{
    //双边断报消息
    public class MMBreakAlarmModel
    {
        public string creatTime;
        public int dangerCount;
        public int passiveCount;
        public List<BreakAlarmModel> infos = new List<BreakAlarmModel>();
        public string status;
        public MMBreakAlarmModel Copy() 
        {
            MMBreakAlarmModel retReq = new MMBreakAlarmModel();
            retReq.creatTime = this.creatTime;
            retReq.dangerCount = this.dangerCount;
            retReq.passiveCount = this.passiveCount;
            retReq.infos = this.infos;
            retReq.status = this.status;
            return retReq;
        }
    }
    public class BreakAlarmModel 
    {
        public string creatTime;
        public string groupId;
        public string groupName;
        public string groupType;
        public string monitorStatus;
        public string reason;
        public string securityId;
        public string symbol;
    }
}
