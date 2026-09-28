using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.Monitor
{
    public class MonitorInfoModel
    {
        public int id { get; set; }
        public string serviceName { get; set; }
        public string host { get; set; }
        public string port { get; set; }
        public string type { get; set; }
        public string cnServiceName { get; set; }
        public string loc { get; set; }
        public string fill { get; set; }
        public string color { get; set; }
        public string size { get; set; }
        public int thickness { get; set; }
        public string figure { get; set; }
        public string svrGroup { get; set; }
        public int visible { get; set; }
        public string action { get; set; }  // action = 1查询 2修改 3 插入 4查询最新的zookeeper
        public int isGroup { get; set; }
        /// <summary>
        /// 服务资源信息
        /// </summary>
        public string desc { get; set; }

        public string connectSvr { get; set; }
        public string updatetime { get; set; }

    }
}
