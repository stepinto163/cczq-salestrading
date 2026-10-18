using System;

namespace STC.SalesTrading.Plugin.Gateway
{
    /// <summary>
    /// FICC 后端 topic 常量与路由。topic 取值依据后端
    /// docs/ficc-api-spec.zh-CN.md §4.0「请求 Handler 与发布频道」。
    /// 历史遗留：早期规范稿用过 side:BID/OFR 等不同命名，一律以主规范为准。
    /// </summary>
    internal static class GatewayTopics
    {
        #region 报价生命周期（QuoteSvr）

        public const string QuoteCreate = "ficc.quote.create";
        public const string QuoteConfirm = "ficc.quote.confirm";
        public const string QuoteUpdate = "ficc.quote.update";
        public const string QuoteRefer = "ficc.quote.refer";
        public const string QuoteUnrefer = "ficc.quote.unrefer";
        public const string QuoteDelete = "ficc.quote.delete";
        public const string QuoteClone = "ficc.quote.clone";
        public const string RecommendationQuery = "ficc.recommendation.query";
        public const string RecommendationFeedback = "ficc.recommendation.feedback";
        public const string QuoteGet = "ficc.quote.get";
        public const string QuoteQuery = "ficc.quote.query";
        public const string QuoteHistoryQuery = "ficc.quote.history.query";
        public const string QuoteHistorySearch = "ficc.quote.history.search";
        public const string QuoteSourceMessageQuery = "ficc.quote.sourceMessage.query";
        public const string QuoteIntakeApply = "ficc.quote.intake.apply";
        public const string QuoteRecognize = "ficc.quote.recognize";

        public const string QuoteDictionaryQuery = "ficc.quote.dictionary.query";

        #endregion

        #region 报价视图协议（QuoteSvr，LiveView quote 域）

        public const string QuoteStreamOpen = "ficc.quote.stream.open";
        public const string QuoteViewOpen = "ficc.quote.view.open";
        public const string QuoteViewQuery = "ficc.quote.view.query";
        public const string QuoteViewClose = "ficc.quote.view.close";

        #endregion

        #region 行情（QuoteSvr，LiveView price 域）

        public const string PriceDictionaryQuery = "ficc.price.dictionary.query";
        public const string PriceQuery = "ficc.price.query";
        public const string PriceStreamOpen = "ficc.price.stream.open";
        public const string PriceViewOpen = "ficc.price.view.open";
        public const string PriceViewQuery = "ficc.price.view.query";
        public const string PriceViewClose = "ficc.price.view.close";

        #endregion

        #region 提醒（NotificationSvr，LiveView notification 域）

        public const string NotificationStreamOpen = "ficc.notification.stream.open";
        public const string NotificationViewOpen = "ficc.notification.view.open";
        public const string NotificationViewQuery = "ficc.notification.view.query";
        public const string NotificationViewClose = "ficc.notification.view.close";
        public const string NotificationQuery = "ficc.notification.query";
        public const string NotificationRead = "ficc.notification.read";
        public const string NotificationPreferenceGet = "ficc.notification.preference.get";
        public const string NotificationPreferenceSave = "ficc.notification.preference.save";

        #endregion

        #region 关注债券 / 筛选分组（QuoteSvr）

        public const string WatchlistQuery = "ficc.watchlist.query";
        public const string WatchlistSave = "ficc.watchlist.save";
        public const string WatchlistDelete = "ficc.watchlist.delete";
        public const string FilterGroupQuery = "ficc.filterGroup.query";
        public const string FilterGroupSave = "ficc.filterGroup.save";
        public const string FilterGroupDelete = "ficc.filterGroup.delete";

        #endregion

        #region 基础资料 / CRM（QuoteSvr）

        public const string BaseGet = "ficc.base.get";
        public const string BaseQuery = "ficc.base.query";

        #endregion

        #region 服务路由

        /// <summary>
        /// 依 topic 前缀路由到目标服务（规范 §2.2）：提醒域归 NotificationSvr，
        /// STC 老识别服务归 ParserSvr（stc.parser.bond，现存已上线；H40 ficc.quote.recognize
        /// 亦应归 ParserSvr），其余归 QuoteSvr。GW 服务注册名已确认为 <c>QuoteSvr</c>（决策 #7）。
        /// </summary>
        public static string ServiceFor(string topic)
        {
            if (topic != null && topic.StartsWith("ficc.notification.", StringComparison.Ordinal)) return "NotificationSvr";
            if (topic != null && topic.StartsWith("stc.parser.", StringComparison.Ordinal)) return "ParserSvr";
            return "QuoteSvr";
        }

        #endregion
    }
}
