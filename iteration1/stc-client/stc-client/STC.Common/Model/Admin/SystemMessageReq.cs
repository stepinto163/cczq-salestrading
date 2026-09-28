using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.Admin
{
    /// <summary>
    /// 代办事项/消息提醒查询请求参数
    /// </summary>
    public class SystemMessageReq
    {
        /// <summary>
        /// N:normal
        /// A:Warn
        /// C:Critical
        /// ER:ExecutionReport
        /// </summary>
        public string contenttype;
        public string content;
        /// <summary>
        /// type=2代办事项，type=1 消息通知
        /// </summary>
        public string type;
        public string closeby;
        public string status;
        public string begintime;
        public string endtime;
        public string id;
        public string readflag;
        public string createtime;
        public string test;
        public string username;
        public string delFlag;
        public string venue;
        public int pageSize;
        public int pageNo;
        public string contentP1;
        public string contentP2;
        //不传时默认升序 传了就降序
        public string sort;
        public string displayAll;
    }

    public class SystemMessageDetail
    {
        public string EventName { get; set; }
        public object Data { get; set; }
    }
}
