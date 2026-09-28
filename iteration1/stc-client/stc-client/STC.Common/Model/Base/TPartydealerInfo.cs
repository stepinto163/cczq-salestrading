using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.Base
{
    /// <summary>
    /// 全量交易员通讯录信息接口请求结果
    /// </summary>
    public class TPartydealerInfo
    {
        public string Code { get; set; }
        public List<DealerInfo> Datas { get; set; }
    }

    public class DealerInfo
    {
        public string ADDRESS { get; set; }
        public string DEALER_ID { get; set; }
        public string DEALER_NAME { get; set; }
        public string EMAIL { get; set; }
        public string FAX { get; set; }
        public int ID { get; set; }
        public string PARTY_CODE6 { get; set; }
        public string PARTY_NAME { get; set; }
        public string TELE { get; set; }
        public string UPDATE_TIME { get; set; }

    }
}
