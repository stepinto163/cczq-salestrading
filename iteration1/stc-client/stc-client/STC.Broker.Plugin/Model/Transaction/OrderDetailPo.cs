using STC.Broker.Plugin.Model.Transaction.BridgeModel;
using STC.Broker.Plugin.Model.Transaction.CrossBridge;
using STC.Broker.Plugin.Utils;
using STC.Common.Utils;
using STC.Frame.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace STC.Broker.Plugin.Model.Transaction
{

    /// <summary>
    /// 成交单
    /// </summary>
    public class OrderDetailPo
    {
        /// <summary>
        /// 流水可按此字段排序，按时间因为有同一时间生成多条数据的情况无法分清先后
        /// </summary>
        public string id { get; set; }
        /// <summary>
        /// 交易员B IDB唯一标识
        /// </summary>
        public string CounterPartyTraderId { get; set; }
        /// <summary>
        /// 交易员A名字
        /// </summary>
        public string TraderName { get; set; }
        /// <summary>
        /// 交易员B名字
        /// </summary>
        public string CounterPartyTraderName { get; set; }
        /// <summary>
        /// 机构A名称，注意虽然命名是code，但确实是名称
        /// </summary>
        public string InstitutionName { get; set; }
        public string InstitutionShowName { get; set; }
        /// <summary>
        /// 机构B名称
        /// </summary>
        public string CounterPartyInstitutionName { get; set; }
        public string CounterPartyInstitutionShowName { get; set; }
        /// <summary>
        /// 机构A唯一标识
        /// </summary>
        public string InstitutionID { get; set; }
        /// <summary>
        /// 机构B唯一标识
        /// </summary>
        public string CounterPartyInstitutionId { get; set; }
        /// <summary>
        /// 交易员A IDB唯一标识
        /// </summary>
        public string TraderID { get; set; }
        /// <summary>
        /// 议价标识
        /// </summary>
        public string BargainFlag { get; set; }
        /// <summary>
        /// BrokerA IDB唯一标识
        /// </summary>
        public string PartySubID { get; set; }
        /// <summary>
        /// BrokerB IDB唯一标识
        /// </summary>
        public string CounterPartySubID { get; set; }
        /// <summary>
        /// BrokerA名字
        /// </summary>
        public string PartySubName { get; set; }
        /// <summary>
        /// BrokerB名字
        /// </summary>
        public string CounterPartySubName { get; set; }
        /// <summary>
        /// 公司标识
        /// </summary>
        public string CompanyId { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public string createTime { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public string createTimeShow
        {
            get
            {
                string day = string.Empty;
                if (!string.IsNullOrEmpty(createTime))
                    day = createTime.Substring(0, 10);
                if (day == DateTime.Now.ToString("yyyy-MM-dd"))
                {
                    return createTime.Substring(11, 8);
                }
                else
                {
                    return createTime.Substring(0, 19);
                }
            }
        }

        ///////2.8按照成交时间排序
        /// <summary>
        /// 成交时间
        /// </summary>
        public DateTime? TransactionTime
        {
            get
            {
                DateTime dateTime;
                if (DateTime.TryParseExact(createTime, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTime))
                {
                    return dateTime;
                }
                else
                {
                    return null;
                }
            }
        }
        /// <summary>
        /// 成交单ID
        /// </summary>
        public string ExecID { get; set; }
        /// <summary>
        /// 债券代码
        /// </summary>
        public string SecurityID { get; set; }
        /// <summary>
        /// 债券代码显示 ofr
        /// </summary>
        public string goodsCodeShow2
        {
            get
            {
                if (string.IsNullOrEmpty(SecurityID))
                    return string.Empty;
                if (CopyDetailHelper.IsShowDate(SettlDate2, ConvertTextHelper.NextDate))
                {
                    string TradeDate = string.Empty;
                    if (!string.IsNullOrEmpty(DealDate2) && DealDate2.Length == 8)
                    {
                        TradeDate = DealDate2.Insert(4, "-").Insert(7, "-");
                    }
                    string[] s = TradeDate.Split('-');
                    if (s.Length == 3)
                        TradeDate = string.Format("{0}.{1}", s[1], s[2]);

                    return string.Format("{0}({1} 远)", SecurityID.Replace(".IB", ""), TradeDate);
                }
                else
                {
                    return SecurityID.Replace(".IB", "");
                }
            }
        }
        /// <summary>
        /// 债券代码显示 bid
        /// 远期交易的明细，原券码后面会有交易日的展示，现在交易日的后面增加“远”，强调为远期交易。
        /// </summary>
        public string goodsCodeShow
        {
            get
            {
                if (string.IsNullOrEmpty(SecurityID))
                    return string.Empty;
                if (CopyDetailHelper.IsShowDate(SettlDate, ConvertTextHelper.NextDate))
                {
                    string TradeDate = string.Empty;
                    if (!string.IsNullOrEmpty(dealdate) && dealdate.Length == 8)
                    {
                        TradeDate = dealdate.Insert(4, "-").Insert(7, "-");
                    }
                    string[] s = TradeDate.Split('-');
                    if (s.Length == 3)
                        TradeDate = string.Format("{0}.{1}", s[1], s[2]);

                    return string.Format("{0}({1} 远)", SecurityID.Replace(".IB", ""), TradeDate);
                }
                else
                {
                    return SecurityID.Replace(".IB", "");
                }
            }
        }
        /// 扩展预留字段1
        /// </summary>
        public string inf1 { get; set; }
        /// <summary>
        /// 扩展预留字段2 
        /// traderB qq号码
        /// </summary>
        public string inf2 { get; set; }
        /// <summary>
        /// 扩展预留字段3
        /// </summary>
        public string inf3 { get; set; }
        /// <summary>
        /// 扩展预留字段4
        /// </summary>
        public string inf4 { get; set; }
        /// <summary>
        /// 匹配数量
        /// </summary>
        public decimal? matchQty { get; set; }
        /// <summary>
        /// oco标识
        /// </summary>
        public string OCOFlag { get; set; }


        /// <summary>
        /// 换券标识
        /// </summary>
        public string ExchangeSignFlag { get; set; }


        /// <summary>
        /// 意向单ID
        /// </summary>
        public string OrderID { get; set; }
        /// <summary>
        /// 订单状态/意向单状态
        /// O：新意向 E&I&G:确认 F&H：拒绝 C：撤销（未成交删除的单子也是这个状态） D：删除
        /// </summary>
        public string OrdStatus { get; set; }

        /// <summary>
        /// 价格
        /// </summary>
        public decimal? price { get; set; }
        /// <summary>
        /// 价格描述
        /// </summary>
        public string priceDescription { get; set; }
        /// <summary>
        /// 去掉前面的逗号
        /// </summary>
        public string priceDescriptionShow
        {
            get
            {
                if (string.IsNullOrEmpty(priceDescription))
                    return "";
                else
                {
                    string temp = priceDescription;
                    temp.Replace(",", "");
                    temp.Replace("，", "");
                    return temp;
                }

            }
        }
        /// <summary>
        /// 报价数量
        /// </summary>
        public decimal? qty { get; set; }
        /// <summary>
        /// 报价ID
        /// </summary>
        public string QuoteID { get; set; }
        /// <summary>
        /// 拒绝者
        /// </summary>
        public string rejecter { get; set; }
        /// <summary>
        /// 拒绝原因
        /// </summary>
        public string rejecterReason { get; set; }
        /// <summary>
        /// 交易方向
        /// </summary>
        public string Side { get; set; }
        /// <summary>
        /// 更新时间
        /// </summary>
        public string updateTime { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public string updateTimeShow
        {
            get
            {
                string day = string.Empty;
                if (!string.IsNullOrEmpty(updateTime))
                    day = updateTime.Substring(0, 10);
                if (day == DateTime.Now.ToString("yyyy-MM-dd"))
                {
                    return string.IsNullOrEmpty(updateTime) ? null : updateTime.Substring(11, 8);
                }
                else
                {
                    return string.IsNullOrEmpty(updateTime) ? null : updateTime.Substring(0, 19);
                }
            }
        }
        /// <summary>
        /// 整量标识
        /// </summary>
        public string WholeFlag { get; set; }
        /// <summary>
        /// 内部报价标识
        /// </summary>
        public string quoteInternalFlag { get; set; }
        /// <summary>
        /// brokerB确认的数量
        /// </summary>
        public decimal? brokerbConfirmQty { get; set; }
        /// <summary>
        /// 报价数量
        /// </summary>
        public decimal? quoteqty { get; set; }
        /// <summary>
        /// 发送QQ标识
        /// 0 后台发送
        /// 1 前端发送
        /// </summary>
        public string SendQQMsgFlag { get; set; }
        /// <summary>
        /// 发送QQ内容
        /// </summary>
        public string sendQQMsgTem { get; set; }
        /// <summary>
        /// BID成交日期
        /// </summary>
        public string dealdate { get; set; }
        /// <summary>
        /// BID结算日期
        /// </summary>
        public string SettlDate { get; set; }
        /// <summary>
        /// bid 结算类型
        /// 结算方式
        /// </summary>
        public string SettlType { get; set; }
        /// <summary>
        /// 当日成交序号
        /// </summary>
        public decimal? seq { get; set; }
        /// <summary>
        ///2：需要加桥 1：不需要加桥
        /// </summary>
        public string BridgeFlag { get; set; }
        public string bridgeflagshow
        {
            get { return string.IsNullOrEmpty(BridgeFlag) ? "否" : BridgeFlag.Equals("2") ? "是" : "否"; }
        }
        /// <summary>
        ///成交单的bid发给 跟其他桥要素相反 桥机构+桥发给+桥费用  
        /// </summary>
        public string SendTo { get; set; }
        /// <summary>
        ///成交单的ofr发给  
        /// </summary>
        public string SendTo2 { get; set; }
        /// <summary>
        /// brokerA查看标识
        /// </summary>
        public string brokerreadflaga { get; set; }
        /// <summary>
        /// brokerB 查看标识
        /// </summary>
        public string brokerreadflagb { get; set; }
        /// <summary>
        /// 最后操作人
        /// 确认操作不更新closeby
        /// </summary>
        public string closeby { get; set; }
        /// <summary>
        /// 最后操作人
        /// 每次都更新closeby2
        /// </summary>
        public string closeby2 { get; set; }
        /// <summary>
        /// 是否有未读broker
        /// broker查看列表
        /// </summary>
        public string BrokerUnread { get; set; }
        /// <summary>
        /// 最后修改人（不包括确认操作）
        /// </summary>
        public string CloseById { get; set; }
        /// <summary>
        /// 最后操作人（包括确认操作）
        /// </summary>
        public string CloseById2 { get; set; }
        /// <summary>
        /// 有效标记位:0-有效,1-无效
        /// </summary>
        public string delflag { get; set; }
        /// <summary>
        /// dcs操作标记
        /// </summary>
        public string dcsflag { get; set; }
        /// <summary>
        /// 查询标记
        /// </summary>
        public string queryflag { get; set; }
        /// <summary>
        /// 机构A 简称  后台会根据债券的类型将crm中对应的简称（利率债简称，信用债简称）写进去
        /// </summary>
        public string PartyName { get; set; }
        /// <summary>
        /// 机构B 简称 
        /// </summary>
        public string CounterPartyName { get; set; }


        /// <summary>
        /// 内部交易标记
        /// </summary>
        public string InternalFlag { get; set; }
        /// <summary>
        /// OFR成交日期
        /// </summary>
        public string DealDate2 { get; set; }
        /// <summary>
        /// OFR结算日期
        /// </summary>
        public string SettlDate2 { get; set; }
        /// <summary>
        /// OFR结算类型
        /// </summary>
        public string SettlType2 { get; set; }
        /// <summary>
        /// ofr桥的机构简称
        /// </summary>
        public string BridgeInstitutionName { get; set; }
        /// <summary>
        /// ofr桥的机构名称
        /// </summary>
        public string bridgebankcode { get; set; }
        /// <summary>
        /// ofr桥的机构idbkey
        /// </summary>
        public string BridgeInstitutionId { get; set; }
        /// <summary>
        /// ofr桥的交易员idbkey
        /// </summary>
        public string BridgeTraderId { get; set; }
        /// <summary>
        /// ofr桥的交易员名称
        /// </summary>
        public string BridgeTraderName { get; set; }
        /// <summary>
        /// ofr桥的渠道：1:对话，2:请求,3:双边,4:xbond,5:ideal
        /// </summary>
        public string BridgeChannel { get; set; }
        /// <summary>
        /// ofr桥的发给
        /// </summary>
        public string BridgeSendTo { get; set; }
        /// <summary>
        /// ofr桥的费用
        /// </summary>
        public string BridgeFee { get; set; }
        /// <summary>
        /// ofr特殊佣金
        /// </summary>
        public string ofrSpecialBridgeFee { get; set; }
        /// <summary>
        /// ofr桥的备注
        /// </summary>
        public string BridgeRemark { get; set; }
        /// <summary>
        /// ofr桥备注是否在明细中显示
        /// </summary>
        public string BridgeRemarkShow { get; set; }
        /// <summary>
        /// ofr桥的桥类型：默认方式:bid,ofr共有桥:B,O bid的桥:B offer的桥:O
        /// </summary>
        public string BridgeType { get; set; }
        /// <summary>
        /// bid桥的机构简称
        /// </summary>
        public string BridgeInstitutionName2 { get; set; }
        /// <summary>
        /// bid桥的机构名称
        /// </summary>
        public string bridgebankcode2 { get; set; }
        /// <summary>
        /// bid桥的机构idbkey
        /// </summary>
        public string BridgeInstitutionId2 { get; set; }
        /// <summary>
        /// Bid桥的交易员idbkey
        /// </summary>
        public string BridgeTraderId2 { get; set; }
        /// <summary>
        ///// </summary>
        public string BridgeTraderName2 { get; set; }
        /// <summary>
        /// bid桥的渠道：1:对话，2:请求,3:双边,4:xbond,5:ideal
        /// </summary>
        public string BridgeChannel2 { get; set; }
        /// <summary>
        /// bid桥的发给
        /// </summary>
        public string BridgeSendTo2 { get; set; }
        /// <summary>
        /// bid桥的费用
        /// </summary>
        public string BridgeFee2 { get; set; }
        /// <summary>
        /// bid特殊佣金
        /// </summary>
        public string bidSpecialBridgeFee { get; set; }
        /// <summary>
        /// bid桥的备注
        /// </summary>
        public string BridgeRemark2 { get; set; }
        /// <summary>
        /// bid桥备注是否在明细中显示
        /// </summary>
        public string BridgeRemarkShow2 { get; set; }
        /// <summary>
        /// bid桥的桥类型：默认方式:bid,ofr共有桥:B,O bid的桥:B offer的桥:O
        /// </summary>
        public string BridgeType2 { get; set; }
        /// <summary>
        /// 添加双桥按钮的点击B,O状态
        /// </summary>
        public string BridgeStatus { get; set; }
        /// <summary>
        /// 最后加桥人
        /// </summary>
        public string BridgeCloseById { get; set; }
        /// <summary>
        /// 第一座桥创建时间  这里是第一座桥的床创建时间
        /// </summary>
        public string BridgeCreateTime { get; set; }
        /// <summary>
        /// 第一座桥更新时间  
        /// </summary>
        public string BridgeUpdateTime { get; set; }
        /// <summary>
        /// 第二座桥创建时间   这里是第二座桥的床创建时间
        /// </summary>
        public string BridgeCreateTime2 { get; set; }
        /// <summary>
        /// 第二座桥更新时间
        /// </summary>
        public string BridgeUpdateTime2 { get; set; }
        /// <summary>
        /// 双桥渠道 
        /// 1-对话 2-请求 双边 xbond ideal 固收 竞价 大宗
        /// </summary>
        public string DoubleBridgeChannel { get; set; }
        /// <summary>
        /// 双桥发给 
        /// </summary>
        public string DoubleBridgeSendTo { get; set; }
        /// <summary>
        /// 双桥费用
        /// </summary>
        public string DoubleBridgeFee { get; set; }
        /// <summary>
        /// 双桥备注
        /// </summary>
        public string DoubleBridgeRemark { get; set; }
        /// <summary>
        /// 交易要素备注
        /// </summary>
        public string tradingElementsRemark { get; set; }

        /// <summary>
        /// ofr桥合单备注
        /// </summary>
        public string combinedOrderRemark { get; set; }
        /// <summary>
        /// bid桥合单备注
        /// </summary>
        public string combinedOrderRemark2 { get; set; }
        /// <summary>
        /// 双桥off清算类型
        /// </summary>
        public string DoubleBridgeOffSettltype { get; set; }
        /// <summary>
        /// 双桥bid清算类型
        /// </summary>
        public string DoubleBridgeBidSettltype { get; set; }
        /// <summary>
        /// 双桥offer清算类型显示
        /// </summary>
        public string doubleBridgeSettleOfrDisplay
        {
            get
            {
                ///默认错期Ofr
                if (string.IsNullOrEmpty(doubleBridgeSettleSide))
                {
                    if (SettlType == SettlType2)
                        return SettleDisplay2;
                    else
                        return SettleDisplay2 + "/" + SettleDisplay;
                }
                else
                {
                    if (doubleBridgeSettleSide == "B")
                        return SettleDisplay2;
                    else
                        return SettleDisplay2 + "/" + SettleDisplay;
                }
            }
        }
        /// <summary>
        /// 双桥bid清算类型显示
        /// </summary>
        public string doubleBridgeSettleBidDisplay
        {
            get
            {
                ///默认错期Ofr
                if (string.IsNullOrEmpty(doubleBridgeSettleSide))
                {
                    return SettleDisplay;
                }
                else
                {
                    if (doubleBridgeSettleSide + "" == "B")
                        return SettleDisplay2 + "/" + SettleDisplay;
                    else
                        return SettleDisplay;
                }
            }
        }

        /// <summary>
        /// 双桥结算：显示为"Ofr +1，Bid +1/明天+0“”格式，分别把双桥详情中的Ofr结算和Bid结算拼在一起显示即可。
        /// </summary>
        public string DoubleBridgeSettle
        {
            get
            {
                if (string.IsNullOrEmpty(DoubleBridgeOffSettltype) && string.IsNullOrEmpty(DoubleBridgeBidSettltype))
                    return "";
                else if (string.IsNullOrEmpty(DoubleBridgeOffSettltype) && !string.IsNullOrEmpty(DoubleBridgeBidSettltype))
                    return string.Format("Bid {0}", DoubleBridgeBidSettltype);
                else if (!string.IsNullOrEmpty(DoubleBridgeOffSettltype) && string.IsNullOrEmpty(DoubleBridgeBidSettltype))
                    return string.Format("Ofr {0}", DoubleBridgeOffSettltype);
                else
                    return string.Format("Ofr {0},Bid {1}", DoubleBridgeOffSettltype, DoubleBridgeBidSettltype);
            }
        }

        Func<string, string> DoubleChanelConvert = delegate (string a)
        {
            switch (a)
            {
                case "1":
                    return "对话";
                case "2":
                    return "请求";
                case "3":
                    return "双边";
                case "4":
                    return "xbond";
                case "5":
                    return "ideal";
                case "6":
                    return "固收";
                case "7":
                    return "竞价";
                case "8":
                    return "大宗";
                case "9":
                    return "意向";
                case "10":
                    return "匿名";
                case "11":
                    return "xbargain";
                default:
                    return "对话";
            }
        };
        /// <summary>
        /// 双桥信息：显示为“发XXX，渠道费用，备注”格式，如“发国金证券，请求3厘，111”
        /// </summary>
        /// 
        public string DoubleBridgeInfo
        {

            get
            {

                if (string.IsNullOrEmpty(DoubleBridgeSendTo) && string.IsNullOrEmpty(DoubleBridgeChannel) && string.IsNullOrEmpty(DoubleBridgeFee) && string.IsNullOrEmpty(DoubleBridgeRemark))
                    return "";
                else if (string.IsNullOrEmpty(DoubleBridgeSendTo))
                {
                    if (string.IsNullOrEmpty(DoubleBridgeRemark))
                        return string.Format("{0}{1}", DoubleChanelConvert(DoubleBridgeChannel + ""), DoubleBridgeFee);
                    else
                        return string.Format("{0}{1}, {2}", DoubleChanelConvert(DoubleBridgeChannel + ""), DoubleBridgeFee, DoubleBridgeRemark);
                }
                else if (!string.IsNullOrEmpty(DoubleBridgeSendTo))
                {
                    if (string.IsNullOrEmpty(DoubleBridgeRemark))
                        return string.Format("发{0}, {1}{2}", DoubleBridgeSendTo, DoubleChanelConvert(DoubleBridgeChannel + ""), DoubleBridgeFee);
                    else
                        return string.Format("发{0}, {1}{2}, {3}", DoubleBridgeSendTo, DoubleChanelConvert(DoubleBridgeChannel + ""), DoubleBridgeFee, DoubleBridgeRemark);
                }
                else
                    return string.Format("发{0}, {1}{2}, {3}", DoubleBridgeSendTo, DoubleChanelConvert(DoubleBridgeChannel + ""), DoubleBridgeFee, DoubleBridgeRemark);
            }
        }
        /// <summary>
        /// 返点
        /// </summary>
        public string fandian { get; set; }
        /// <summary>
        /// brokerB自动确认时间
        /// </summary>
        public decimal? autoConfirmTimeB { get; set; }

        /// <summary>
        /// 2020.02.12 wym添加注释
        /// 5）操作记录中录入返点信息时，返点信息字段可输入文本。
        ///（1）若用户输入纯数字，则认为是返点信息，在明细窗口、过桥窗口、操作记录中展示时，均带有"F“开头。
        /// </summary>
        public string FandianShow
        {
            get
            {
                decimal d = 0;
                if (decimal.TryParse(fandian, out d))
                    return !string.IsNullOrEmpty(fandian) ? "F" + fandian : string.Empty;
                else
                    return fandian;
            }
        }
        /// <summary>
        /// 备注
        /// </summary>
        public string remarks { get; set; }
        /// <summary>
        /// 同步dcs标签
        /// </summary>
        public string syncdcsflag { get; set; }


        /// <summary>
        /// 1:Bid桥; 2:Ofr桥
        /// </summary>
        public string isBidBridge { get; set; }

        #region Display
        public string goodsShortName { get; set; }
        /// <summary>
        /// 剩余期限，目前前端计算得出
        /// V2.9.0 改为后台计算，前端计算功能去除
        /// </summary>
        public string timeToMaturity { get; set; }
        /// <summary>
        /// //1信用,2利率,11 ABS,12 NCD
        /// </summary>
        public string goodstype { get; set; }
        public string goodsSubType { get; set; }
        public string TraderAInfo
        {
            get
            {
                if (string.IsNullOrWhiteSpace(TraderName) && string.IsNullOrWhiteSpace(InstitutionName) && string.IsNullOrWhiteSpace(PartyName))
                    return "";

                if (!string.IsNullOrWhiteSpace(TraderName) && (!string.IsNullOrWhiteSpace(InstitutionName) || !string.IsNullOrEmpty(PartyName)))
                {
                    if (!string.IsNullOrWhiteSpace(PartyName))
                        return string.Format("{0}({1})", PartyName, TraderName.Trim());
                    else
                        return string.Format("{0}({1})", InstitutionName, TraderName.Trim());
                }

                if (string.IsNullOrWhiteSpace(TraderName) && (!string.IsNullOrWhiteSpace(InstitutionName) || !string.IsNullOrEmpty(PartyName)))
                {
                    if (!string.IsNullOrWhiteSpace(PartyName))
                        return PartyName;
                    else
                        return InstitutionName;
                }
                if (!string.IsNullOrWhiteSpace(TraderName) && string.IsNullOrWhiteSpace(InstitutionName) && string.IsNullOrWhiteSpace(PartyName))
                    return TraderName;

                return "";
            }
        }

        public string TraderBInfo
        {
            get
            {
                if (string.IsNullOrWhiteSpace(CounterPartyTraderName) && string.IsNullOrWhiteSpace(CounterPartyInstitutionName) && string.IsNullOrWhiteSpace(CounterPartyName))
                    return "";

                if (!string.IsNullOrWhiteSpace(CounterPartyTraderName) && (!string.IsNullOrWhiteSpace(CounterPartyInstitutionName) || !string.IsNullOrEmpty(CounterPartyName)))
                {
                    if (!string.IsNullOrWhiteSpace(CounterPartyName))
                        return string.Format("{0}({1})", CounterPartyName, CounterPartyTraderName.Trim());
                    else
                        return string.Format("{0}({1})", CounterPartyInstitutionName, CounterPartyTraderName.Trim());
                }


                if (string.IsNullOrWhiteSpace(CounterPartyTraderName) && (!string.IsNullOrWhiteSpace(CounterPartyInstitutionName) || !string.IsNullOrEmpty(CounterPartyName)))
                {
                    if (!string.IsNullOrWhiteSpace(CounterPartyName))
                        return CounterPartyName;
                    else
                        return CounterPartyInstitutionName;
                }

                if (!string.IsNullOrWhiteSpace(CounterPartyTraderName) && string.IsNullOrWhiteSpace(CounterPartyInstitutionName) && string.IsNullOrWhiteSpace(CounterPartyName))
                    return CounterPartyTraderName;

                return "";

            }
        }
        /// <summary>
        /// 区分OfrTrader的买卖方向
        /// </summary>
        public string OfrTraderInfo
        {
            get { return IsGVN ? TraderAInfo : TraderBInfo; }
        }
        /// <summary>
        /// 区分BidTrader的买卖方向
        /// </summary>
        public string BidTraderInfo
        {
            get { return IsGVN ? TraderBInfo : TraderAInfo; }
        }

        /// <summary>
        /// broker方向显示
        /// </summary>
        public string SideDisplay
        {
            get
            {
                string sideDisplay = "";
                //if (forbidRefreshFlag == "3" && SystemParamHandler.GlobSystemParameter.showSourceDealSide)
                //{
                //    sideDisplay = "TRD";
                //}
                //else
                //{
                //    sideDisplay = Online + "" == "3" ? IsGVN ? "出给" : "买入" : IsGVN ? "GVN" : "TKN";
                //}
                sideDisplay = Online + "" == "3" ? IsGVN ? "出给" : "买入" : IsGVN ? "GVN" : "TKN";
                return sideDisplay;
            }
        }

        public string VolDisplay
        {
            get
            {
                if (matchQty.HasValue)
                {
                    return matchQty.Value + "";
                }
                return "";
            }
        }

        public string ContentDisplay
        {
            get
            {
                string result = string.Empty;
                if (!string.IsNullOrEmpty(SettleDisplay2) && SettleDisplay.Equals(SettleDisplay2))
                {
                    if (!string.IsNullOrEmpty(remarks))
                    {
                        DateTime dtDeal = CopyDetailHelper.GetDate(dealdate);
                        result = (dtDeal.Month < 10 ? ("0" + dtDeal.Month) : dtDeal.Month.ToString()) + "."
                                + (dtDeal.Day < 10 ? ("0" + dtDeal.Day) : dtDeal.Day.ToString()) + remarks;
                    }
                    else
                        result = SettleDisplay;
                }
                else
                {
                    if (!string.IsNullOrEmpty(SettlType) && !string.IsNullOrEmpty(SettlType2))
                        result = string.Format("{0}/{1}", SettleDisplay2, SettleDisplay);
                    else if (!string.IsNullOrEmpty(SettlType) && string.IsNullOrEmpty(SettlType2))
                        result = SettleDisplay;
                    else if (string.IsNullOrEmpty(SettlType) && !string.IsNullOrEmpty(SettlType2))
                        result = SettleDisplay2;
                }

                return result;


                //return string.Format("{0} {1}", SettleDisplay, WholeDisplay);
            }
        }

        public string ContentBidDisplay
        {
            get
            {
                string result = string.Empty;
                if (!string.IsNullOrEmpty(SettleDisplay2) && SettleDisplay.Equals(SettleDisplay2))
                {
                    if (!string.IsNullOrEmpty(remarks))
                    {
                        DateTime dtDeal = CopyDetailHelper.GetDate(dealdate);
                        result = (dtDeal.Month < 10 ? ("0" + dtDeal.Month) : dtDeal.Month.ToString()) + "."
                                + (dtDeal.Day < 10 ? ("0" + dtDeal.Day) : dtDeal.Day.ToString()) + remarks;
                    }
                    else
                        result = SettleDisplay;
                }
                else
                {
                    if (!string.IsNullOrEmpty(SettlType) && !string.IsNullOrEmpty(SettlType2))
                        result = string.Format("{0}/{1}", SettleDisplay2, SettleDisplay);
                    else if (!string.IsNullOrEmpty(SettlType) && string.IsNullOrEmpty(SettlType2))
                        result = SettleDisplay;
                    else if (string.IsNullOrEmpty(SettlType) && !string.IsNullOrEmpty(SettlType2))
                        result = SettleDisplay2;
                }

                return result;


                //return string.Format("{0} {1}", SettleDisplay, WholeDisplay);
            }
        }


        public string ContentOfrDisplay
        {
            get
            {
                string result = string.Empty;
                if (!string.IsNullOrEmpty(SettleDisplay2) && SettleDisplay.Equals(SettleDisplay2))
                {
                    if (!string.IsNullOrEmpty(remarks))
                    {
                        DateTime dtDeal = CopyDetailHelper.GetDate(dealdate);
                        result = (dtDeal.Month < 10 ? ("0" + dtDeal.Month) : dtDeal.Month.ToString()) + "."
                                + (dtDeal.Day < 10 ? ("0" + dtDeal.Day) : dtDeal.Day.ToString()) + remarks;
                    }
                    else
                        result = SettleDisplay;
                }
                else
                {
                    if (!string.IsNullOrEmpty(SettlType) && !string.IsNullOrEmpty(SettlType2))
                        result = string.Format("{0}/{1}", SettleDisplay2, SettleDisplay);
                    else if (!string.IsNullOrEmpty(SettlType) && string.IsNullOrEmpty(SettlType2))
                        result = SettleDisplay;
                    else if (string.IsNullOrEmpty(SettlType) && !string.IsNullOrEmpty(SettlType2))
                        result = SettleDisplay2;
                }

                return result;


                //return string.Format("{0} {1}", SettleDisplay, WholeDisplay);
            }
        }

        /// <summary>
        /// 设置价格标签
        /// </summary>
        /// <param name="spTag"></param>
        public void SetTag(StackPanel spTag)
        {
            spTag.Children.Clear();
            if ("1".Equals(BargainFlag))
            {
                Image star = (Image)spTag.FindResource("price_x_Image");
                spTag.Children.Add(star);
            }
            else if ("2".Equals(BargainFlag))
            {
                Image star = (Image)spTag.FindResource("price_xx_Image");
                spTag.Children.Add(star);
            }
            if ("1".Equals(OCOFlag))
            {
                Image oco = (Image)spTag.FindResource("price_oco_Image");
                spTag.Children.Add(oco);
            }
            else if ("2".Equals(OCOFlag))
            {
                Image oco = (Image)spTag.FindResource("price_package_Image");
                spTag.Children.Add(oco);
            }

            //if (ExchangeSignFlag == "1" && Constant.BrokerFlags.Contains(LocalEnv.ConfInfo.BrokerNameFlag))
            //{
            //    Image oco = (Image)spTag.FindResource("ExchangeBond_Image");
            //    spTag.Children.Add(oco);

            //}
        }

        public string WholeDisplay
        {
            get
            {
                return "1".Equals(WholeFlag) ? "整量" : "";
            }
        }

        private const string DefaultSettle1 = "+1,明天+0";
        private const string DefaultSettle2 = "明天+0,+1";
        /// <summary>
        /// 成交列表使用
        /// 如果是默认结算类型，则不显示
        /// </summary>
        public string SettleDisplayForMain2
        {
            get
            {
                if (DefaultSettle2.Equals(SettlType2) && SettlType2.Contains(DefaultSettle2))
                {
                    return "";
                }
                return SettlType2;
            }
        }
        /// <summary>
        /// 成交列表使用
        /// 如果是默认结算类型，则不显示
        /// </summary>
        public string SettleDisplayForMain
        {
            get
            {
                if (DefaultSettle1.Equals(SettlType) && SettlType.Contains(DefaultSettle1))
                {
                    return "";
                }
                return SettlType;
            }
        }
        /// <summary>
        /// bid结算显示
        /// </summary>
        public string SettleDisplay
        {
            get
            {
                //return CopyDetailHelper.GetSettleShow(SettlType, dealdate);
                if (DefaultSettle2.Equals(SettlType) && SettlType.Contains(DefaultSettle2))
                {
                    return "";
                }
                return TransOrderHelper.Instance.GetSettleDisplay(SettlType, dealdate);
            }
        }

        /// <summary>
        /// ofr结算显示
        /// </summary>
        public string SettleDisplay2
        {
            get
            {
                //return CopyDetailHelper.GetSettleShow(SettlType2, DealDate2);
                if (DefaultSettle2.Equals(SettlType2) && SettlType2.Contains(DefaultSettle2))
                {
                    return "";
                }
                return TransOrderHelper.Instance.GetSettleDisplay(SettlType2, DealDate2);
            }
        }

        public bool IsGVN
        {
            get
            {
                return "2".Equals(Side);
            }
        }

        public bool IsInternalQuote
        {
            get { return "2".Equals(quoteInternalFlag); }
        }

        public bool IsPartlyConfirm
        {
            get
            {
                return brokerbConfirmQty.HasValue && matchQty.HasValue && brokerbConfirmQty.Value != matchQty.Value;
            }
        }

        public bool IsChangeAmount
        {
            get
            {
                return brokerbConfirmQty.HasValue && qty.HasValue && brokerbConfirmQty.Value != qty.Value;
            }
        }


        public bool IsChangePrice
        {
            get
            {
                //if(!string.IsNullOrWhiteSpace(inf4) && price.HasValue)
                //{
                //    string tmpPrice = MathEx.PriceFormat(price.Value);
                //    string tmpInf4 = "";
                //    decimal tmp;
                //    if(decimal.TryParse(inf4,out tmp))
                //    {
                //        tmpInf4 = MathEx.PriceFormat(tmp);
                //        if (!tmpPrice.Equals(tmpInf4))
                //            return true;
                //    }
                //}
                //return false;

                //return !string.IsNullOrWhiteSpace(inf4) && price.HasValue && !inf4.TrimEnd('0').TrimEnd('.').Equals((price + "").TrimEnd('0').TrimEnd('.'));


                decimal changPrice = decimal.Zero;
                if (decimal.TryParse(inf4, out changPrice))
                {
                    if (price.HasValue)
                        return price.Value == changPrice ? false : true;
                }
                return false;
            }
        }



        public string PriceFormat
        {
            get
            {
                return Common.Utils.MathEx.PriceFormat(price.HasValue ? price.Value : 0);
            }
        }
        public string OfrBroker
        {
            get
            {
                if (IsGVN)
                {
                    if (string.IsNullOrWhiteSpace(PartySubName))
                        PartySubName = null;
                    return PartySubName;
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(CounterPartySubName))
                        CounterPartySubName = null;
                    return CounterPartySubName;
                }
            }
        }
        public string BidBroker
        {
            get
            {
                if (IsGVN)
                {
                    if (string.IsNullOrWhiteSpace(CounterPartySubName))
                        CounterPartySubName = null;
                    return CounterPartySubName;
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(PartySubName))
                        PartySubName = null;
                    return PartySubName;
                }
            }
        }
        public string OfrInst
        {
            get
            {
                if (IsGVN)
                {
                    if (string.IsNullOrWhiteSpace(InstitutionName))
                        InstitutionName = null;
                    if (!string.IsNullOrWhiteSpace(PartyName))
                        return PartyName;
                    else
                        return InstitutionName;
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(CounterPartyInstitutionName))
                        CounterPartyInstitutionName = null;
                    if (!string.IsNullOrWhiteSpace(CounterPartyName))
                        return CounterPartyName;
                    else
                        return CounterPartyInstitutionName;
                }
            }
        }
        public string BidInst
        {
            get
            {
                if (IsGVN)
                {
                    if (string.IsNullOrWhiteSpace(CounterPartyInstitutionName))
                        CounterPartyInstitutionName = null;
                    if (!string.IsNullOrWhiteSpace(CounterPartyName))
                        return CounterPartyName;
                    else
                        return CounterPartyInstitutionName;
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(InstitutionName))
                        InstitutionName = null;
                    if (!string.IsNullOrWhiteSpace(PartyName))
                        return PartyName;
                    else
                        return InstitutionName;
                }
            }
        }
        public string OfrbankAgentName
        {
            get
            {
                if (IsGVN)
                {
                    if (string.IsNullOrWhiteSpace(TraderName))
                        TraderName = null;
                    return TraderName;
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(CounterPartyTraderName))
                        CounterPartyTraderName = null;
                    return CounterPartyTraderName;
                }
            }
        }
        public string BidbankAgentName
        {
            get
            {
                if (IsGVN)
                {
                    if (string.IsNullOrWhiteSpace(CounterPartyTraderName))
                        CounterPartyTraderName = null;
                    return CounterPartyTraderName;
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(TraderName))
                        TraderName = null;
                    return TraderName;
                }
            }
        }
        #endregion


        #region 操作流水显示
        public string OldMsg { get; set; }
        public string NewMsg { get; set; }

        public Brush ShowBrush;


        public string OperateType
        {
            get
            {
                string result = string.Empty;

                if ("0".Equals(OrdStatus))
                {
                    result = "新意向";

                }
                else if ("E".Equals(OrdStatus))
                {
                    if (IsPartlyConfirm)
                    {
                        result = "部分确认";

                    }
                    else
                    {
                        result = "全部确认";
                    }
                }
                else if ("F".Equals(OrdStatus))
                {
                    result = "拒绝";
                }
                else if ("H".Equals(OrdStatus))
                {
                    result = "拒绝";
                }
                else if ("C".Equals(OrdStatus))
                {
                    result = "交易撤销";
                }
                else if ("I".Equals(OrdStatus))
                {
                    result = "确认";
                }
                //客户端自定义状态显示
                else if ("a".Equals(OrdStatus))
                    result = "修改债券";
                else if ("b".Equals(OrdStatus))
                    result = "修改价格";
                else if ("c".Equals(OrdStatus))
                    result = "修改数量";
                else if ("d".Equals(OrdStatus))
                    result = "修改结算";
                else if ("e".Equals(OrdStatus))
                    result = "修改交易员";
                else if ("f".Equals(OrdStatus))
                    result = "修改Broker";
                else if ("g".Equals(OrdStatus))
                    result = "修改发给对象";
                else if ("h".Equals(OrdStatus))
                    result = "新增桥";
                else if ("i".Equals(OrdStatus))
                    result = "修改桥";
                else if ("j".Equals(OrdStatus))
                    result = "删除桥";
                else if ("k".Equals(OrdStatus))
                    result = "修改价格/量";
                else if ("l".Equals(OrdStatus))
                    result = "修改价格描述";
                else if ("m".Equals(OrdStatus))
                    result = "DCS";
                else if ("n".Equals(OrdStatus))
                    result = "修改桥状态";
                else if ("G".Equals(OrdStatus))
                {
                    //if ((IsChangeAmount || IsChangePrice) && string.IsNullOrEmpty(dcsflag))
                    //{
                    //    result = "修改";
                    //}
                    //if (string.IsNullOrEmpty(brokerunread) || (!brokerunread.Contains(brokerIdA + "") && !brokerunread.Contains(brokerIdB + "")))
                    //if (string.IsNullOrEmpty(brokerunread) || (!brokerunread.ContainsUserId(brokerIdA) && !brokerunread.ContainsUserId(brokerIdB)))
                    //{
                    //    result = "确认";
                    //}
                }

                return result;
            }
        }
        #endregion

        ///sendto展示
        ///当bridgeflag等于1的时候，只显示发给，当等于2的时候只显示ofr发给和bid发给
        public string SendToShow
        {
            get
            {
                if (!(BridgeFlag + "").Equals("2"))
                    return SendTo;
                else
                    return "";
            }
        }
        //操作流水展示ofr发给
        public string OfrSendToShow
        {
            get
            {
                if ((BridgeFlag + "").Equals("2"))
                    return SendTo2;
                else
                    return "";
            }
        }
        //操作流水展示bid发给
        public string BidSendToShow
        {
            get
            {
                if ((BridgeFlag + "").Equals("2"))
                    return SendTo;
                else
                    return "";
            }
        }

        /// <summary>
        /// 是否免佣
        /// 1：免佣 0：不免佣 
        /// </summary>
        public string brokeragea { get; set; }
        /// <summary>
        /// 操作记录中显示
        /// </summary>
        public string brokerageaShow
        {
            get
            {
                if (IsGVN)
                {

                    return brokeragea + "" == "1" ? "ofr是" : "ofr否";
                }
                else
                {

                    return brokeragea + "" == "1" ? "bid是" : "bid否";
                }

            }
        }

        /// <summary>
        /// 是否免佣
        /// 1：免佣 0：不免佣 
        /// </summary>
        public string brokerageb { get; set; }
        /// <summary>
        /// 操作记录中显示
        /// </summary>
        public string brokeragebShow
        {
            get
            {
                if (IsGVN)
                {
                    return brokerageb + "" == "1" ? "bid是" : "bid否";
                }
                else
                {
                    return brokerageb + "" == "1" ? "ofr是" : "ofr否";
                }

            }
        }
        /// <summary>
        /// bid主发给 可以去掉备注 注意sendto2 是ofr主发给
        /// </summary>
        public string Dsendto
        {
            get
            {
                //bool b = RemarksIsShow.不显示.EqualsToValue("1");
                return ""; // ERemarkShow.不显示.EqualsToValue(bridgeremarkshow2) ? string.IsNullOrEmpty(bridgeremark2) ? sendto : (sendto + "").Replace(", " + bridgeremark2 + "", "") : sendto;
            }
        }
        /// <summary>
        /// ofr主发给显示
        /// </summary>
        public string Dsendto2
        {
            get
            {
                return ""; // ERemarkShow.不显示.EqualsToValue(bridgeremarkshow) ? string.IsNullOrEmpty(bridgeremark) ? sendto2 : (sendto2 + "").Replace(", " + bridgeremark + "", "") : sendto2;
            }
        }

        /// <summary>
        /// 添加操作类型字段
        /// </summary>
        private int commandtype;
        public int commandType
        {
            get
            {
                return commandtype;
            }
            set
            {
                //if (value >= (int)ECommandType.默认)
                //    commandtype = value;
                //else
                //    commandtype = (int)ECommandType.默认;

            }
        }
       // public ECommandType EnumCommandType { get { return (ECommandType)commandtype; } }

        //******v2.5 预约桥功能添加字段*****
        /// <summary>
        /// 加桥单ID，bridgeorderID-ofr
        /// </summary>
        public string BorderId { get; set; }
        /// <summary>
        /// 加桥单-ofr
        /// </summary>
        public bridgeOrder bridgeOrder { get; set; }
        /// <summary>
        /// 加桥单ID，bridgeorderID-Bid
        /// </summary>
        public string BorderId2 { get; set; }
        /// <summary>
        /// 加桥单-Bid
        /// </summary>
        public bridgeOrder bridgeOrder2 { get; set; }
        //*****************END*****

        //******v2.6 债券信息，包括发行量，评级*****
        /// <summary>
        /// 债券类型 
        /// </summary>
        public string BondType { get; set; }
        /// <summary>
        /// 债券子类别
        /// </summary>
        public string BondSubtype { get; set; }
        /// <summary>
        /// 流通市场
        /// </summary>
        public string ListedMarket { get; set; }
        /// <summary>
        /// 债券发行量
        /// </summary>
        public decimal? IssueAmount { get; set; }
        /// <summary>
        /// 评级
        /// </summary>
        public string IssuerRatingCurrent { get; set; }
        /// <summary>
        /// 发行方代码
        /// 国开	PBB，PSB	Issuer_Code= G000124
        /// 进出口  PBB，PSB    Issuer_Code = Z000189
        /// 农发    PBB，PSB    Issuer_Code = Z000207
        /// </summary>
        public string IssuerCode { get; set; }
        /// <summary>
        /// 企业债/公司债
        /// 企业债：CCB，COB，RAB  当CEB和LEB且Ent_Cor=ENT
        /// 公司债：SCV，CVB       当CEB和LEB且Ent_Cor=COP
        /// </summary>
        public string EntCor { get; set; }

        /// <summary>
        /// 基础利率代码
        /// </summary>
        public string FRNIndexID { get; set; }

        /// <summary>
        /// 固定标识
        /// 3-匹配不上不会立刻撤销，改挂单
        /// A-FAK订单
        /// </summary>
        public string tif;
        /// <summary>
        /// 线上/线下
        /// 1-线下报价线下点价
        /// 2-线上报价
        /// 3-线下录入
        /// </summary>
        public string Online
        {
            get
            {
                if ("3".Equals(onlinea) && "3".Equals(onlineb))
                {
                    return "3";
                }
                else if ("2".Equals(onlinea) || "2".Equals(onlineb))
                {
                    return "2";
                }
                else
                {
                    return "1";
                }
            }
        }
        /// <summary>
        /// 点价方online
        /// </summary>
        public string onlinea;
        /// <summary>
        /// 报价方online
        /// </summary>
        public string onlineb;

        /// <summary>
        /// 双桥错期选择，默认是Ofr ；且只有订单结算类型是错期且添加双桥，此字段才有意义。
        /// O：Ofr方错期
        /// B：Bid方错期
        /// </summary>
        public string doubleBridgeSettleSide { get; set; }

        /// <summary>
        /// 当前登录是否为brokerA
        /// </summary>
        public string BrokerARole
        {

            get
            {
                if (LocalEnv.UserInfo.IsMyself(PartySubID))
                {
                    return "1";
                }
                else
                {
                    return "2";
                }
            }
        }
        /// <summary>
        /// 当前登录是否为brokerB
        /// </summary>
        public string BrokerBRole
        {
            get
            {
                if (LocalEnv.UserInfo.IsMyself(CounterPartySubID))
                {
                    return "1";
                }
                else
                {
                    return "2";
                }
            }
        }
        /********V2.11 添加***********/
        /// <summary>
        /// 约定号
        /// </summary>
        public string agreementNo { get; set; }

        /********V2.12 添加***********/
        /// <summary>
        /// 查看操作标识，如果为1 则为查看标识，不刷新明细
        /// </summary>
        public int isConfirmOperation { get; set; }

        /********V2.14 添加***********/
        /// <summary>
        /// 成交不刷标记 0:默认未标记 1:标记表示成交不刷
        /// </summary>
        public string forbidRefreshFlag { get; set; }

        /********V2.16 添加***********/
        /// <summary>
        /// 对应traderA 是否发送
        /// </summary>
        public string sendSorFlaga { get; set; }

        /// <summary>
        /// 对应traderB 是否发送
        /// </summary>
        public string sendSorFlagb { get; set; }



        /// <summary>
        /// 区分Ofr方向是否发送
        /// </summary>
        public string OfrsendSorFlag
        {
            get { return IsGVN ? sendSorFlaga : sendSorFlagb; }
        }
        /// <summary>
        /// 区分BidTrader是否发送
        /// </summary>
        public string BidsendSorFlag
        {
            get { return IsGVN ? sendSorFlagb : sendSorFlaga; }
        }




        public string BrokerShowStyle
        {
            get
            {
                string brokerShowKey = "";
                //brokerA
                if (BrokerARole == "1")
                {
                    if ("G".Equals(OrdStatus))
                    {
                        brokerShowKey = "1";
                    }
                    else if ("H".Equals(OrdStatus))
                    {
                        brokerShowKey = "2";
                    }
                }
                else
                {
                    if ("E".Equals(OrdStatus))
                    {
                        if (IsPartlyConfirm)
                        {
                            brokerShowKey = "3";
                        }
                        else
                        {
                            brokerShowKey = "1";
                        }
                    }
                    else if ("F".Equals(OrdStatus))
                    {
                        brokerShowKey = "2";
                    }
                    else if ("G".Equals(OrdStatus) || "H".Equals(OrdStatus))
                    {
                        if (IsPartlyConfirm)
                        {
                            brokerShowKey = "3";
                        }
                        else
                        {
                            brokerShowKey = "1";
                        }
                    }
                }
                return brokerShowKey;
            }
        }


        #region  20220214 -----2.17.0版本
        /// <summary>
        /// ofr方发单机构集合
        /// </summary>
        public List<SendInstSevieceModel> ofrIssueInfoReqList { get; set; }
        /// <summary>
        /// bid方发单机构集合
        /// </summary>
        public List<SendInstSevieceModel> bidIssueInfoReqList { get; set; }
        #endregion

        /// <summary>
        /// 意向单（一级半市场）标记。 1 二级成交单 2 意向单
        /// </summary>
        public string flagInner { get; set; }


        /// <summary>
        /// 同步状态
        /// </summary>
        public string ProcessStatus { get; set; }
        /// <summary>
        /// 失败原因
        /// </summary>
        public string FailedReason { get; set; }

        #region 3.0.0版本

        /// <summary>
        /// 发给方向 1：ofr发bid  2：bid发ofr
        /// </summary>
        public string issueDir { get; set; }
        /// <summary>
        /// 发给渠道
        /// </summary>
        public string issueChannel { get; set; }
        /// <summary>
        /// 发单机构id
        /// </summary>
        public string issueInstId { get; set; }
        /// <summary>
        /// 发单机构名称
        /// </summary>
        public string issueInstName { get; set; }
        /// <summary>
        /// 发单机构台子简称
        /// </summary>
        public string issueInstNameShort { get; set; }
        /// <summary>
        /// 发单人id
        /// </summary>
        public string issueTraderId { get; set; }
        /// <summary>
        /// 发单人名称
        /// </summary>
        public string issueTraderName { get; set; }
        /// <summary>
        /// 发给备注
        /// </summary>
        public string issueRemark { get; set; }
        /// <summary>
        /// 发给动作描述
        /// </summary>
        public string issueDesc { get; set; }
        /// <summary>
        /// 代付费机构id
        /// </summary>
        public string proxyInstId { get; set; }
        /// <summary>
        /// 代付费用
        /// </summary>
        public string proxyFee { get; set; }
        /// <summary>
        /// 代付费标记0：未开启代付费  1：开启代付费
        /// </summary>
        public string proxyFlag { get; set; }

        /// <summary>
        /// 发给标准化开启状态 0 未开启 1 开启
        /// </summary>
        public string issueState { get; set; }
        /// <summary>
        /// 1 展示全部  2 展示渠道和备注
        /// </summary>
        public string issueShowType { get; set; }
        #endregion

        #region 3.1.0

        /// <summary>
        /// 当前broker是否可以查看此订单
        /// </summary>
        public bool haveQueryPermission { get; set; }

        #endregion

        #region 3.2.0
        /// <summary>
        /// 剩余期限（天）
        /// </summary>
        public int maturityDays { get; set; }
        #endregion

        #region  V3.3.0
        /// <summary>
        /// 手动发送单条明细qq消息标记a  0 待发送 1 已发送
        /// </summary>
        public string SingleQQMsgSendFlaga { get; set; } = "0";

        /// <summary>
        /// 手动发送单条明细qq消息标记b  0 待发送 1 已发送
        /// </summary>
        public string SingleQQMsgSendFlagb { get; set; } = "0";

        #endregion

        #region  V3.5.0
        /// <summary>
        /// 明细标记  0 未标记 1  已标记
        /// </summary>
        public string DetailMark { get; set; } = "0";
        /// <summary>
        /// 关联机构  0-不是关联方机构；1-是关联方机构
        /// </summary>
        public string AssociatedFlag { get; set; } = "0";
        #endregion
    }
}

