using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.Table
{
    public class IntentionQuoteRequest
    {
        // stc用户名
        public string userName;
        // 订单状态 ,0=待处理,1=已处理,2=已成交, 3=已忽略
        public string status;
        // 日期，格式yyyy-MM-dd，不传代表所有
        public string date;
        // 债券编号
        public string securityId;
        // 方向
        public string side;
        // 机构名称
        public string institutionName;
        // 交易员名称
        public string traderName;
        // 开始日期，格式yyyy-MM-dd
        public string startDate;
        // 结束日期，格式yyyy-MM-dd
        public string endDate;
        // 分页号码
        public int pageNum;
        // 分页大小
        public int pageSize;
        // 债券到期日
        public string maturityDate;
        // 债券类型编号
        public string securityTypeId;
        // 债券类型
        public string securityType;
        // 债券发行人
        public string isssuer;
        // 债券发行人评级
        public string issuerRating;
        // 清算速度
        public string settlType;
    }
}
