using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.PT
{
    /// <summary>
    /// 查询订单修改结算结果
    /// </summary>
    public class SettlResult:BaseReplyPo
    {
        public string Datas { get; set; }

        public SettlModel SettlData
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(Datas))
                {
                    return JsonConvert.DeserializeObject<SettlModel>(Datas);
                }
                else
                {
                    return null;
                }

            }
        }
    }

    public class SettlModel
    {
        /// <summary>
        /// 代付方机构id
        /// </summary>
        public string UpdateOfrSettltype { get; set; }
        /// <summary>
        /// 代付费用
        /// </summary>
        public string UpdateBidSettltype { get; set; }
        /// <summary>
        /// 订单id
        /// </summary>
        public List<string> SameBatchExecids { get; set; }

        /// <summary>
        /// 交易员A是否首次修改
        /// </summary>
        public string UpdateTraderA { get; set; }
    }
}
