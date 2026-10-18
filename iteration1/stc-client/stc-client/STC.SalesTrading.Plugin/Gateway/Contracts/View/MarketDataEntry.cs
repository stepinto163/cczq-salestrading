using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace STC.SalesTrading.Plugin.Gateway.Contracts.View
{

    /// <summary>STC 行情条目（NoMDEntries[]）。编码陷阱集中在 Mapper：MDEntryType "0"=Bid；MDEntryPx 为比较用收益率。</summary>
    public class MarketDataEntry
    {
        /// <summary>建议 FICC:quoteId 前缀 + 报价 Id。</summary>
        [JsonProperty("MDEntryID")]
        public string MDEntryID { get; set; }

        /// <summary>"0"=Bid / "1"=Ofr（由 Quote.side 映射）。</summary>
        [JsonProperty("MDEntryType")]
        public string MDEntryType { get; set; }

        /// <summary>比较用收益率。</summary>
        [JsonProperty("MDEntryPx")]
        public string MDEntryPx { get; set; }

        /// <summary>报量，WAN 面值。</summary>
        [JsonProperty("MDEntrySize")]
        public string MDEntrySize { get; set; }

        /// <summary>到期收益率，%。</summary>
        [JsonProperty("Yield")]
        public string Yield { get; set; }

        /// <summary>行权收益率，%。</summary>
        [JsonProperty("YieldToExecution")]
        public string YieldToExecution { get; set; }

        /// <summary>净价，缺失省略。</summary>
        [JsonProperty("NetPrice")]
        public string NetPrice { get; set; }

        /// <summary>全价，缺失省略。</summary>
        [JsonProperty("DirtyPrice")]
        public string DirtyPrice { get; set; }

        /// <summary>STC PriceType："3"=收益率（与 FICC quoteType 编码相反）。</summary>
        [JsonProperty("PriceType")]
        public string PriceType { get; set; }

        /// <summary>"0"=行权 "1"=到期（与 FICC yieldType 编码相反）。</summary>
        [JsonProperty("ExerciseFlag")]
        public string ExerciseFlag { get; set; }

        [JsonProperty("SettlType")]
        public string SettlType { get; set; }

        /// <summary>报价归属用户（FICC 渠道映射 ownerUserId）。</summary>
        [JsonProperty("UserID")]
        public string UserID { get; set; }

        /// <summary>档位顺序号（服务端算完价格档后输出）。</summary>
        [JsonProperty("MDPriceLevel")]
        public string MDPriceLevel { get; set; }

        /// <summary>档内位置号（后端新增）。</summary>
        [JsonProperty("MDEntryPositionNo")]
        public string MDEntryPositionNo { get; set; }

        /// <summary>报价机构 Id（后端已随价格簿下发）。</summary>
        [JsonProperty("InstitutionID")]
        public string InstitutionID { get; set; }

        /// <summary>报价机构名称（决策 #3 落地：看板机构列数据源）。</summary>
        [JsonProperty("InstitutionName")]
        public string InstitutionName { get; set; }

        /// <summary>报价联系人 Id。</summary>
        [JsonProperty("TraderID")]
        public string TraderID { get; set; }

        /// <summary>报价联系人姓名（看板交易员列数据源）。</summary>
        [JsonProperty("TraderName")]
        public string TraderName { get; set; }

        /// <summary>yyyyMMdd（updateTime 业务时区拆分）。</summary>
        [JsonProperty("MDEntryDate")]
        public string MDEntryDate { get; set; }

        /// <summary>HH:mm:ss.SSS。</summary>
        [JsonProperty("MDEntryTime")]
        public string MDEntryTime { get; set; }
    }

}
