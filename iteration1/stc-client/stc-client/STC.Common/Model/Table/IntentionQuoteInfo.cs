using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.Table
{

    public class IntentionQuoteInfo
    {
        // 意向报价唯一编号
        public string intentionId;
        // 机构编号
        public string institutionId;
        // 机构名称
        public string institutionName;
        // 交易员编号
        public string traderId;
        // 交易员名称
        public string traderName;
        // QQ号码
        public string qqNumber;
        // 债券编号
        public string securityId;
        // 债券简称
        public string symbol;
        // 报价方向 1=买、2=卖
        public string side;
        // 价格
        public decimal? price;
        // 净价
        public decimal? netPrice;
        // 全价(弃用)
        public decimal? fullPrice;
        // 报量
        public decimal? qty;
        //加权成交价
        public decimal? lastPrice;
        ////成交量
        //public decimal? lastQty;
        // 清算类型 1=T+0, 2=T+1, 3=Tm+0, 4=Tm+1, 99=其他
        public string settlType;
        // 原始文本 
        public string text;
        // 订单状态 ,0=待处理,1=已处理,2=报价中, 3=已忽略,4=已暂停,5=已完成,6已删除
        public string status;
        // 创建时间
        public string createTime;
        // 更新时间
        public string updateTime;
        // 我的订单号(弃用)
        public string orderId;
        // 用户名
        public string userName;
        // 用户昵称
        public string nickName;
        // 价格区间-起始价
        public decimal? startPrice;
        // 价格区间-结尾价
        public decimal? endPrice;
        // 簿记量
        public decimal? bookQty;
        // 剩余量
        public decimal? leftQty;
        // 盈亏值
        public decimal? profitLoss;
        // 报量完成度
        public string completeRate;
        // 运行时长
        public long runningTime;
        // 意向报价标签，逗号分割
        public string tagList;
        // 操作人（弃用）
        public string closeBy;
    }
}
