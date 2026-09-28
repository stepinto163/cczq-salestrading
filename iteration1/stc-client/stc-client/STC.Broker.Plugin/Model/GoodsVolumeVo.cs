using STC.Frame.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace STC.Broker.Plugin.Model.Transaction
{
    /// <summary>
    /// 报价方向
    /// </summary>
    public enum EDirection
    {
        //ask
        Offer = 0,
        Bid = 1
    }

    /// <summary>
    /// 这是一个报价小方块中对应的报价
    /// </summary>
    public class GoodsVolumeVo : IComparable<GoodsVolumeVo>
    {
        /// <summary>
        /// 方向
        /// 0 offer
        /// 1 bid
        /// </summary>
        public EDirection Direction = EDirection.Offer;//public int type = 0;
        public bool IsBid
        {
            get
            {
                return Direction == EDirection.Bid;//return type == 1;
            }
        }
        /// <summary>
        /// 只show即期报价
        /// 是否只展示即期
        /// </summary>
        public bool onlyShowSpot = true;
        /// <summary>
        /// 议价标识 0:无星; 1: 1星; 2: 2星
        /// </summary>
        public string bargainFlag;

        /// <summary>
        /// 议价标识复制展示
        /// </summary>
        public string BargainFlagShow
        {
            get
            {
                string show = string.Empty;
                if (bargainFlag == "1")
                {
                    show = "*";
                }
                else if (bargainFlag == "2")
                {
                    show = "**";
                }
                else
                {

                }
                return show;
            }
        }

        /// <summary>
        /// 换券标识 true: 换券; false 不换券
        /// </summary>
        public bool ExchangeSignFlag { get; set; }


        /// <summary>
        /// 返费
        /// </summary>
        public string FanDian { get; set; }

        /// <summary>
        /// 返费复制展示
        /// </summary>
        public string FanDianShow { get { return string.IsNullOrEmpty(FanDian) ? string.Empty : $"F{FanDian}"; } }


        /// <summary>
        /// 换券标识复制展示
        /// </summary>
        public string ExchangeSignFlagShow
        {
            get
            {
                return ExchangeSignFlag ? "换券" : string.Empty;
            }
        }

        /// <summary>
        /// oco flag 复制展示
        /// </summary>
        public string OcoFlagShow
        {
            get
            {
                string show = string.Empty;
                if (ocoFlag == "1")
                {
                    show = "oco";
                }
                else if (ocoFlag == "2")
                {
                    show = "打包";
                }
                else
                {

                }
                return show;
            }
        }

        /// <summary>
        /// 经纪商ID
        /// Broker唯一标识
        /// </summary>
        public string brokerID { get; set; }
        /// <summary>
        /// 经纪商名称
        /// </summary>
        public string brokerName { get; set; }
        /// <summary>
        /// 描述
        /// </summary>
        public string description { get; set; }
        /// <summary>
        /// 描述2
        /// </summary>
        public string description2 { get; set; }
        /// <summary>
        /// 唯一标识
        /// </summary>
        public string id { get; set; }
        /// <summary>
        /// 机构ID
        /// </summary>
        public string institutionID { get; set; }
        /// <summary>
        /// 机构名称
        /// </summary>
        public string institutionName { get; set; }
        /// <summary>
        /// OCO标识
        /// QQQQQQQQQQQQQQQQQQQ 
        /// </summary>
        public string ocoFlag { get; set; }
        /// <summary>
        /// 价格
        /// </summary>
        public decimal price { get; set; }
        /// <summary>
        /// 数量
        /// </summary>
        public decimal qty { get; set; }
        /// <summary>
        /// 结算方式
        /// </summary>
        public string[] settlementType { get; set; }
        /// <summary>
        /// 时间
        /// </summary>
        public string time { get; set; }
        /// <summary>
        /// 交易员ID
        /// </summary>
        public string traderID { get; set; }
        /// <summary>
        /// 交易员名字
        /// </summary>
        public string traderName { get; set; }
        /// <summary>
        /// 整量标志
        /// </summary>
        public string wholeFlag { get; set; }
        /// <summary>
        /// 内部报价标识
        /// 2：内部报价，1：正常报价
        /// </summary>
        public string quoteInternalFlag { get; set; }
        /// <summary>
        /// 市场标识
        /// </summary>
        public string bondKeyListedMarket { get; set; }
        /// <summary>
        /// 混排
        /// </summary>
        public bool isCheck { get; set; }
        /// <summary>
        /// 是否是最优价
        /// </summary>
        public bool isBestPrice { get; set; }
        /// <summary>
        /// 详情（远期界面）
        /// </summary>
        public bool isFwdCheck { get; set; }
        /// <summary>
        /// 债券代码
        /// </summary>
        public string goodCode { get; set; }

        /// <summary>
        /// 紧急标志
        /// </summary>
        public bool flagUrgent { get; set; }

        public string FwdId
        {
            get { return string.Format("{0}-{1}", "Fwd", id); }
        }

        public bool IsSpot
        {
            get
            {

                return settlementType.Contains("+0") || settlementType.Contains("+1") || settlementType.Contains("明天+0");
            }
        }
        /// <summary>
        /// V2.10.0 丢弃
        /// </summary>
        public string SettlementDisplay
        {
            get
            {

                string retval = "";
                if (settlementType == null || !settlementType.Any())
                {
                    return retval;
                }
                if (settlementType.Contains("+1") && settlementType.Length == 1)
                {
                    if (string.IsNullOrWhiteSpace(description2))
                    {
                        retval = "";
                    }
                    else
                    {
                        retval = getSettlementType();
                    }
                }
                else
                {
                    retval = getSettlementType();
                }
                return retval;
            }
        }

        private string getSettlementType()
        {
            string retval = "";

            if (settlementType != null && settlementType.Any())
            {
                for (int i = 0; i < settlementType.Length; i++)
                {
                    if (settlementType.Length - 1 == i)
                    {
                        retval += settlementType[i];
                    }
                    else
                    {
                        retval += settlementType[i] + " ";
                    }
                }
            }
            return retval;
        }

        public string PriceFormat
        {
            get
            {
                return MathEx.PriceFormat(price);
                //int i = MathEx.decimalCount(price);
                //if (i < 2)
                //{
                //    return price.ToString("N2");
                //}

                //return price + "";
            }
        }

        public string PriceInt
        {
            get
            {
                return PriceFormat.Substring(0, PriceFormat.IndexOf('.') + 1);
            }
        }

        public string PriceDecimal
        {
            get
            {
                return PriceFormat.Substring(PriceFormat.IndexOf('.') + 1);
            }
        }


        public int CompareTo(GoodsVolumeVo other)
        {
            return other.time.CompareTo(time);
        }

        /// <summary>
        /// 报价类型，2线上，1线下
        /// </summary>
        public string onLine { get; set; }
        /// <summary>
        /// 结算展示 V2.10.0
        /// </summary>
        public string settlementTypeShow { get; set; }

    }
}
