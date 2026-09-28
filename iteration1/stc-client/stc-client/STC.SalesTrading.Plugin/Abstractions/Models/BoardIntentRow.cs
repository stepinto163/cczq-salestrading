using System;

namespace STC.SalesTrading.Plugin.Abstractions.Models
{
    /// <summary>模糊报价行（原型 0910 由「意向报价」改名；board-intent-table，7 列）。</summary>
    public class BoardIntentRow
    {
        /// <summary>模糊报价 Id（原型字段名沿用 board_intent，未改名）。</summary>
        public string IntentId { get; set; }

        /// <summary>报价内容（可点击行内编辑）。</summary>
        public string Content { get; set; }

        /// <summary>识别结果（结构化条件，recognitionJson 或字符串回退解析）。</summary>
        public string RecognizedConditions { get; set; }

        /// <summary>报价机构（inst）。</summary>
        public string Institution { get; set; }

        /// <summary>报价交易员（tr）。</summary>
        public string Trader { get; set; }

        /// <summary>长城交易员。</summary>
        public string WallTrader { get; set; }

        /// <summary>报价时间（t）。</summary>
        public DateTime? QuoteTime { get; set; }

        /// <summary>状态：有效/撤销。</summary>
        public IntentStatus Status { get; set; }

        /// <summary>乐观锁数据版本（expectedDataVersion）。</summary>
        public long? DataVersion { get; set; }

        /// <summary>是否未同步（行前红色脉冲圆点）。</summary>
        public bool IsPendingSync { get; set; }

        /// <summary>识别质量（原型 /stats 行级口径：完整/部分/冲突/失败）；null = 未评估。</summary>
        public IntentRecognitionStatus? Recognition { get; set; }
    }

    /// <summary>模糊报价识别质量枚举（原型 renderIntentStatsBar：recognized/partial/conflict/failed）。</summary>
    public enum IntentRecognitionStatus
    {
        Recognized,
        Partial,
        Conflict,
        Failed,
    }
}
