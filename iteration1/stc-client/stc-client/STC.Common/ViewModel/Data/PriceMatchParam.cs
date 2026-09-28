using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Data
{
    /// <summary>
    /// 消息匹配参数数据结构
    /// </summary>
    public class PriceMatchParam
    {
        public string WholeFlag { get; set; }
        public string Symbol { get; set; }
        public string Side { get; set; }
        public string SymbolName { get; set; }
        public string OrderId { get; set; }
        public string RandomNum { get; set; }
        public bool IsFirm { get; set; }
        public string CustomFirmAmount { get; set; }
        public string NetPrice { get; set; }
        public string Price { get; set; }

        public string PriceType { get; set; }

        public string SettlType { get; set; }

        public string NonfirmTime { get; set; }

        public string Terminal { get; set; }

        public string Info3 { get; set; }
        public string Text { get; set; }
        /// <summary>
        /// true: 先撤单再下单； false：直接下单
        /// </summary>
        public bool UpdataPrice = true;

        /// <summary>
        /// 1:所有
        /// 2:仅内部
        /// 3:xbond
        /// </summary>
        public string CrossVenues { get; set; }
        public string VenuesSelectF { get; set; }//渠道类型  不填写1,默认2，指定3 
        public string VenuesRdmNum { get; set; }//渠道类型默认，填写随机几家渠道
        public string SelfSelectVenues { get; set; }// 指定渠道参考3.1章节，多家用逗号隔开SOR_CTTJ,SOR_TPSC

        public string InstitutionID{ get; set; }//客盘机构id
        public string InstitutionName { get; set; }//客盘机构名字
        public string TraderID { get; set; }//客盘交易员id
        public string TraderName { get; set; }//客盘交易员名字

        public Common.Model.Fix.Message.NewOrderSingle OldOrder { get; set; }

        /// <summary>
        /// 发起方式
        /// 1:join
        /// 2:价格匹配nonfirm转firm
        /// 3:Ctrl+双击发起
        /// </summary>
        public string OperationType;

        public string Id;
        public string TrsactTime;
        public string Source;


        public string DeliveryType;
        public string ClearingMethod;
        public string QuoteType;

        public string Info1;
        public string Info2;
        public string Info4;
        public string Info5;
        public string Info6;
        public string ClOrdId;

        public string InValidFlag;
        public string TrigEvent;
        public string CancEvent;
        public string EREventName;
        public string CancEventName;

        public string VenueTypeGW;

        /// <summary>
        /// 直通/OCO
        /// </summary>
        public string SendOrderType;

        public string AccountID;

        public string PriceCondition;


        public string IntentionID;

        public string Trader { get; set; }//代替交易员id

        public string Venues;

        //IRS 债券ID   == Symbol
        public string BondID;
        //IRS 的期限
        public string TimeToMaturity;

        public string UserName;

        public string Name;

        /// <summary>
        /// 是否是需求单
        /// </summary>
        public bool IsIntention;

        /// <summary>
        /// 来源
        /// </summary>
        public string channel;

        /// <summary>
        /// 需求工作台订单状态
        /// </summary>
        public string status;

        /// <summary>
        /// 更新时间
        /// </summary>
        public string updateTime;

        //public string counterPartySubID; //对手方ID（STC userName)

        //public string counterPartySubName; //对手方名称 (中文名称)

        #region 买方机构字段
        public string roletype;// 买方机构，角色类型

        public string managerId; // 投资经理
        public string managerName; // 投资经理名
        public string buyerTraderId; // 交易员
        public string buyerTraderName;// 交易员名

        public string relatedUsers;//对谁可见

        public long? MatchedQty;//匹配成交量
        #endregion
    }
}
