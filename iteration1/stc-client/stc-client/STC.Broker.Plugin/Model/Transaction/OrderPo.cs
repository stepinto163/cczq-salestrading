
using STC.Common.Utils;
using STC.Frame.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Broker.Plugin.Model.Transaction
{
    /// <summary>
    /// 意向单
    /// </summary>
    public class OrderPo
    {
        /// <summary>
        /// broker唯一标
        /// </summary>
        public string brokerid { get; set; }
        /// <summary>
        /// broker名字
        /// </summary>
        public string brokername { get; set; }
        /// <summary>
        /// 交易员QB唯一标识
        /// </summary>
        public string bankAgentQbId { get; set; }
        /// <summary>
        /// 交易员QB代码
        /// </summary>
        public string bankQbCode { get; set; }
        /// <summary>
        /// 机构QBID
        /// </summary>
        public string bankQbId { get; set; }
        /// <summary>
        /// 取消数量
        /// </summary>
        public decimal? cancelledQty { get; set; }
        /// <summary>
        /// 客户端ID
        /// </summary>
        public string cid { get; set; }
        /// <summary>
        /// 客户端下单ID
        /// </summary>
        public string clientOrderId { get; set; }
        /// <summary>
        /// 来源
        /// 公司标识
        /// </summary>
        public string companyId { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public decimal? createTime { get; set; }
        /// <summary>
        /// 匹配数量
        /// </summary>
        public decimal? matchQty { get; set; }
        /// <summary>
        /// 成交量
        /// </summary>
        public decimal? executionQty { get; set; }
        /// <summary>
        /// 债券代码
        /// </summary>
        public string goodsCode { get; set; }
        /// <summary>
        /// 债券名称
        /// </summary>
        public string goodsName { get; set; }
        /// <summary>
        /// ID
        /// </summary>
        public string id { get; set; }
        /// <summary>
        /// 订单单状态
        /// O 新意向 New；
        /// C 过期 Expired
        /// 1 部分成交 Partially_Filled； 
        /// 2 全部成交 Filled； 
        /// 4 撤销 Cancelled； 
        /// 6 挂起 Pending_Cancel ；  
        /// 8 拒绝 Rejected；
        /// </summary>
        public string orderStatus { get; set; }
        /// <summary>
        /// 订单类型
        /// </summary>
        public string orderType { get; set; }
        /// <summary>
        /// 价格
        /// </summary>
        public decimal? price { get; set; }
        /// <summary>
        /// 数量
        /// </summary>
        public decimal? qty { get; set; }
        /// <summary>
        /// 结算类型
        /// </summary>
        public string settlementType { get; set; }
        /// <summary>
        /// 方向
        /// </summary>
        public string side { get; set; }
        /// <summary>
        /// 客户端提交量
        /// </summary>
        public string submitClientQty { get; set; }
        /// <summary>
        /// 目的
        /// 操作终端
        /// </summary>
        public string terminal { get; set; }
        /// <summary>
        /// 固定标识
        /// 3-匹配不上不会立刻撤销，改挂单
        /// A-FAK订单
        /// </summary>
        public string tif { get; set; }
        /// <summary>
        /// 时间
        /// </summary>
        public decimal? time { get; set; }
        /// <summary>
        /// 更新时间
        /// </summary>
        public decimal? updateTime { get; set; }
        /// <summary>
        /// 用户名
        /// </summary>
        public string userName { get; set; }
        /// <summary>
        /// 线上/线下
        /// 1-线下点价
        /// 2-线上
        /// 3-线下录入
        /// </summary>
        public string online { get; set; }
        /// <summary>
        /// 发送消息标识
        /// </summary>
        public string sendmsgflag { get; set; }
        /// <summary>
        /// 拒绝者
        /// </summary>
        public string rejecter { get; set; }
        /// <summary>
        /// 拒绝原因
        /// </summary>
        public string rejecterReason { get; set; }
        /// <summary>
        /// 名字
        /// </summary>
        public string name { get; set; }
        /// <summary>
        /// 机构代码
        /// </summary>
        public string bankCode { get; set; }
        /// <summary>
        /// 交易员代码
        /// </summary>
        public string bankAgentCode { get; set; }
        /// <summary>
        /// 成交单列表
        /// </summary>
        public List<OrderDetailPo> execOrders { get; set; }
        /// <summary>
        /// 预留扩展字段1
        /// </summary>
        public string inf1 { get; set; }
        /// <summary>
        /// 预留扩展字段2
        /// </summary>
        public string inf2 { get; set; }
        /// <summary>
        /// 是否免佣
        /// 1：免佣 0：不免用 
        /// </summary>
        public string brokeragea { get; set; }
        /// <summary>
        /// 是否免佣
        /// 1：免佣 0：不免用 
        /// </summary>
        public string brokerageb { get; set; }

        //public Dictionary<string, string> ExpectedStateB = new Dictionary<string, string>();
        //public Dictionary<string, string> ExpectedStateA = new Dictionary<string, string>();
        /// <summary>
        /// 显示中文结算方式
        /// </summary>
        public string SettlementTypeDisplay
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(settlementType))
                {
                    if (settlementType.Equals("all"))
                        return settlementType.Replace("all", "不限");
                    else if (settlementType.Equals("allSpot"))
                        return settlementType.Replace("allSpot", "现券不限");
                    else if (settlementType.Equals("allFwd"))
                        return settlementType.Replace("allFwd", "远期不限");

                }
                return settlementType;
            }
        }

        public string UpdateTimeStr
        {
            get
            {
                if (execOrders != null && execOrders.Any())
                {
                    return execOrders.Max(o => o.updateTime);
                }
                return DateTimeEx.GetDatetime((long)(updateTime.HasValue ? updateTime.Value : 0)).ToString("yyyy-MM-dd HH:mm:ss");
            }
        }
        /// <summary>
        /// brokerA需要发送消息标签
        /// 如果是TraderA线上发起的意向，BrokerA确认或拒绝后，会改变为“发送”按钮；
        /// 如果是BrokerA线下发起的意向，确认或拒绝后成交单即完成。
        /// 如果是一笔意向拆成了多个单子，则必须每个单子都确认过以后才能点发送按钮，否则弹出提示“请全部确认后再发送！”。
        /// </summary>
        public bool NeedSendMsg
        {
            get
            {
                return "2".Equals(online) 
                    && ("1".Equals(orderStatus) || "2".Equals(orderStatus) || "4".Equals(orderStatus)) 
                    && !"1".Equals(sendmsgflag) && LocalEnv.UserInfo.IsMyself(brokerid);
            }
        }
        /// <summary>
        /// brokerB需要点击发送按钮
        /// </summary>
        public bool NeeSendMsgForB
        {
            get
            {
                return "2".Equals(online) && ("0".Equals(orderStatus) || "1".Equals(orderStatus) || "2".Equals(orderStatus) || "4".Equals(orderStatus)) && !"1".Equals(sendmsgflag);
            }
        }
        /// <summary>
        /// 是否GVN
        /// </summary>
        public bool IsGVN { get { return "1".Equals(side); } }

        public string PriceFormat
        {
            get
            {
                return Common.Utils.MathEx.PriceFormat(price.HasValue ? price.Value : 0);
            }
        }

        /****************************************
         * 20200831 V2.6版本添加 FAK订单 
         ****************************************/
        /// <summary>
        /// 起始时间
        /// </summary>
        public string fakStartTime { get; set; }
        /// <summary>
        /// 计时时间
        /// </summary>
        public string exposureDuration { get; set; }
        /// <summary>
        /// 剩余时间
        /// </summary>
        public string remainTime { get; set; }

        /// <summary>
        /// 服务端发送的顺序序号
        /// </summary>
        public long SendSeq { get; set; }
    }
}
