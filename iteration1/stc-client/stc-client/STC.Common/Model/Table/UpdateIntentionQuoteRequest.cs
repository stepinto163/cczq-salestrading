using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.Table
{
    public class UpdateIntentionQuoteRequest
    {
        // 意向报价唯一编号
        public string intentionId;
        // 替换报价编号
        public string replaceId;
        // 价格
        public decimal? price;
        // 报量
        public decimal? qty;
        // 清算类型
        public string settlType;
        // 交易员编号
        public string traderId;
        // 交易员名称
        public string traderName;
        // 经纪商编号
        public string brokerId;
        // 经纪商名称
        public string brokerName;
        // 机构编号
        public string institutionId;
        // 机构名称
        public string institutionName;
        // 方向
        public string side;
        // 债券代码
        public string securityId;
        // 债券简称
        public string symbol;
        // 用户名称
        public string userName;
        // 用户昵称
        public string nickName;
        // 价格区间-起始价
        public decimal? startPrice;
        // 价格区间-结尾价
        public decimal? endPrice;
        // 意向报价标签，逗号分割
        public string tagList;
        // 状态：
        // 0=待处理
        // 1=已确认
        // 2=报价中
        // 3=已忽略
        // 4=已暂停
        // 5=已完成
        public string status;
    }
}
