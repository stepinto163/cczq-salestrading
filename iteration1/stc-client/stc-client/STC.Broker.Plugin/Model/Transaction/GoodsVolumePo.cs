using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Broker.Plugin.Model.Transaction
{
    public class GoodsVolumePo : IComparable<GoodsVolumePo>
    {
        public string bargainFlag;
        public string brokerID;
        public string brokerName;
        public string description;
        public string description2;
        public string id;
        public string institutionID;
        public string institutionName;
        public string ocoFlag;
        public decimal price;
        public decimal qty;
        public string[] settlementType;
        public string time;
        public string traderID;
        public string traderName;
        public string wholeFlag;
        public string quoteInternalFlag;
        public string bondKeyListedMarket;


        /// <summary>
        /// 紧急标志
        /// </summary>
        public bool flagUrgent { get; set; }

        /// <summary>
        /// 换券标志
        /// </summary>
        public bool ExchangeSignFlag { get; set; }

        /// <summary>
        /// 返费
        /// </summary>
        public string FanDian { get; set; }

        /// <summary>
        /// 结算展示 V2.10.0
        /// </summary>
        public string settlementTypeShow;

        /// <summary>
        /// 报价类型，2线上，1线下
        /// </summary>
        public string onLine { get; set; }

        /// <summary>
        /// 混排
        /// </summary>
        public bool isCheck;
        /// <summary>
        /// 详情（远期界面）
        /// </summary>
        public bool isFwdCheck;

        public string goodCode;

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


        public int wholeInt
        {
            get
            {
                return "1".Equals(wholeFlag) ? 1 : 0;
            }
        }

        public int SettlementInt
        {
            get
            {
                return settlementType != null && settlementType.Length > 0 ? 1 : 0;
            }
        }

        public int TagInt
        {
            get
            {
                return "1".Equals(bargainFlag) || "2".Equals(bargainFlag) || "1".Equals(ocoFlag) || "2".Equals(ocoFlag) ? 1 : 0;
            }
        }

        public int DescriptionInt
        {
            get
            {
                return string.IsNullOrWhiteSpace(description) ? 0 : 1;
            }
        }

        public int TagCount
        {
            get
            {
                return SettlementInt + TagInt + DescriptionInt + wholeInt;
            }
        }
        /// <summary>
        /// 比较
        /// 用于排序
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public int CompareTo(GoodsVolumePo other)
        {
            if (TagCount.CompareTo(other.TagCount) != 0)
            {
                return TagCount.CompareTo(other.TagCount);
            }
            else if (wholeInt.CompareTo(other.wholeInt) != 0)
            {
                return -wholeInt.CompareTo(other.wholeInt);
            }
            else if (SettlementInt.CompareTo(other.SettlementInt) != 0)
            {
                return -SettlementInt.CompareTo(other.SettlementInt);
            }
            else if (TagInt.CompareTo(other.TagInt) != 0)
            {
                return -TagInt.CompareTo(other.TagInt);
            }
            else if (DescriptionInt.CompareTo(other.DescriptionInt) != 0)
            {
                return -DescriptionInt.CompareTo(other.DescriptionInt);
            }

            return time.CompareTo(other.time);
        }
    }
}
