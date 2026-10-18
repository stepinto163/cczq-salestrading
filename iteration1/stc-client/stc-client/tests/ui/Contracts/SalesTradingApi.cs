namespace SalesTrading.UiTests.Contracts
{
    /// <summary>Topic 与请求、Data 类型的编译期关联。</summary>
    public static class SalesTradingApi
    {
        public static readonly Endpoint<BaseGetRequest, BaseGetData> BaseGet =
            new Endpoint<BaseGetRequest, BaseGetData>("ficc.base.get");
        public static readonly Endpoint<QuoteViewOpenRequest, QuoteViewData> QuoteViewOpen = new Endpoint<QuoteViewOpenRequest, QuoteViewData>("ficc.quote.view.open");
        public static readonly Endpoint<ViewCloseRequest, ViewCloseData> QuoteViewClose = new Endpoint<ViewCloseRequest, ViewCloseData>("ficc.quote.view.close");
        public static readonly Endpoint<QTradePublishRequest, QTradePublishData> QTradeQuotePublish =
            new Endpoint<QTradePublishRequest, QTradePublishData>("ficc.qtrade.quote.publish");
        public static readonly Endpoint<QuoteCreateRequest, QuoteWriteData> QuoteCreate =
            new Endpoint<QuoteCreateRequest, QuoteWriteData>("ficc.quote.create");
        public static readonly Endpoint<QuoteChangeRequest, QuoteWriteData> QuoteConfirm =
            new Endpoint<QuoteChangeRequest, QuoteWriteData>("ficc.quote.confirm");
        public static readonly Endpoint<QuoteChangeRequest, QuoteWriteData> QuoteUpdate =
            new Endpoint<QuoteChangeRequest, QuoteWriteData>("ficc.quote.update");
        public static readonly Endpoint<QuoteChangeRequest, QuoteWriteData> QuoteRefer =
            new Endpoint<QuoteChangeRequest, QuoteWriteData>("ficc.quote.refer");
        public static readonly Endpoint<QuoteChangeRequest, QuoteWriteData> QuoteUnrefer =
            new Endpoint<QuoteChangeRequest, QuoteWriteData>("ficc.quote.unrefer");
        public static readonly Endpoint<QuoteChangeRequest, QuoteWriteData> QuoteDelete =
            new Endpoint<QuoteChangeRequest, QuoteWriteData>("ficc.quote.delete");
        public static readonly Endpoint<QuoteGetRequest, QuoteGetData> QuoteGet =
            new Endpoint<QuoteGetRequest, QuoteGetData>("ficc.quote.get");
        public static readonly Endpoint<QuoteQueryRequest, QuoteQueryData> QuoteQuery =
            new Endpoint<QuoteQueryRequest, QuoteQueryData>("ficc.quote.query");
        public static readonly Endpoint<QuoteHistoryRequest, QuoteHistoryData> QuoteHistoryQuery =
            new Endpoint<QuoteHistoryRequest, QuoteHistoryData>("ficc.quote.history.query");
        public static readonly Endpoint<WatchlistQueryRequest, WatchlistData> WatchlistQuery =
            new Endpoint<WatchlistQueryRequest, WatchlistData>("ficc.watchlist.query");
        public static readonly Endpoint<WatchlistSaveRequest, WatchlistData> WatchlistSave =
            new Endpoint<WatchlistSaveRequest, WatchlistData>("ficc.watchlist.save");
        public static readonly Endpoint<WatchlistDeleteRequest, WatchlistData> WatchlistDelete =
            new Endpoint<WatchlistDeleteRequest, WatchlistData>("ficc.watchlist.delete");
        public static readonly Endpoint<FilterGroupQueryRequest, FilterGroupData> FilterGroupQuery =
            new Endpoint<FilterGroupQueryRequest, FilterGroupData>("ficc.filterGroup.query");
        public static readonly Endpoint<FilterGroupDeleteRequest, FilterGroupData> FilterGroupDelete =
            new Endpoint<FilterGroupDeleteRequest, FilterGroupData>("ficc.filterGroup.delete");
        public static readonly Endpoint<NotificationQueryRequest, NotificationQueryData> NotificationQuery =
            new Endpoint<NotificationQueryRequest, NotificationQueryData>("ficc.notification.query");
        public static readonly Endpoint<Request, NotificationPreferenceData> NotificationPreferenceGet =
            new Endpoint<Request, NotificationPreferenceData>("ficc.notification.preference.get");
        public static readonly Endpoint<NotificationPreferenceSaveRequest, NotificationPreferenceData> NotificationPreferenceSave =
            new Endpoint<NotificationPreferenceSaveRequest, NotificationPreferenceData>("ficc.notification.preference.save");
    }
}
