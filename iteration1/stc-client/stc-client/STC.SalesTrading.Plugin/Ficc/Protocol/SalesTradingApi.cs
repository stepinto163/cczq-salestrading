namespace STC.SalesTrading.Plugin.Ficc.Protocol
{
    /// <summary>Topic 与请求、Data 类型的编译期关联。</summary>
    public static class SalesTradingApi
    {
        public static readonly Endpoint<QTradePublishRequest, QTradePublishData> QTradeQuotePublish =
            new Endpoint<QTradePublishRequest, QTradePublishData>("ficc.qtrade.quote.publish");
        public static readonly Endpoint<RecommendationQueryRequest, RecommendationQueryData> RecommendationQuery =
            new Endpoint<RecommendationQueryRequest, RecommendationQueryData>("ficc.recommendation.query");
        public static readonly Endpoint<RecommendationFeedbackRequest, RecommendationFeedbackData> RecommendationFeedback =
            new Endpoint<RecommendationFeedbackRequest, RecommendationFeedbackData>("ficc.recommendation.feedback");
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
        public static readonly Endpoint<QuoteChangeRequest, QuoteWriteData> QuoteClone =
            new Endpoint<QuoteChangeRequest, QuoteWriteData>("ficc.quote.clone");
        public static readonly Endpoint<QuoteGetRequest, QuoteGetData> QuoteGet =
            new Endpoint<QuoteGetRequest, QuoteGetData>("ficc.quote.get");
        public static readonly Endpoint<QuoteQueryRequest, QuoteQueryData> QuoteQuery =
            new Endpoint<QuoteQueryRequest, QuoteQueryData>("ficc.quote.query");
        public static readonly Endpoint<QuoteHistoryRequest, QuoteHistoryData> QuoteHistoryQuery =
            new Endpoint<QuoteHistoryRequest, QuoteHistoryData>("ficc.quote.history.query");
        public static readonly Endpoint<QuoteHistorySearchRequest, QuoteHistorySearchData> QuoteHistorySearch =
            new Endpoint<QuoteHistorySearchRequest, QuoteHistorySearchData>("ficc.quote.history.search");
        public static readonly Endpoint<SourceMessageRequest, SourceMessageData> QuoteSourceMessageQuery =
            new Endpoint<SourceMessageRequest, SourceMessageData>("ficc.quote.sourceMessage.query");
        public static readonly Endpoint<IntakeApplyRequest, IntakeApplyData> QuoteIntakeApply =
            new Endpoint<IntakeApplyRequest, IntakeApplyData>("ficc.quote.intake.apply");
        public static readonly Endpoint<Request, StreamOpenData> QuoteStreamOpen =
            new Endpoint<Request, StreamOpenData>("ficc.quote.stream.open");
        public static readonly Endpoint<QuoteViewOpenRequest, QuoteViewData> QuoteViewOpen =
            new Endpoint<QuoteViewOpenRequest, QuoteViewData>("ficc.quote.view.open");
        public static readonly Endpoint<ViewQueryRequest, QuoteViewData> QuoteViewQuery =
            new Endpoint<ViewQueryRequest, QuoteViewData>("ficc.quote.view.query");
        public static readonly Endpoint<ViewCloseRequest, ViewCloseData> QuoteViewClose =
            new Endpoint<ViewCloseRequest, ViewCloseData>("ficc.quote.view.close");
        public static readonly Endpoint<Request, StreamOpenData> PriceStreamOpen =
            new Endpoint<Request, StreamOpenData>("ficc.price.stream.open");
        public static readonly Endpoint<PriceViewOpenRequest, PriceViewData> PriceViewOpen =
            new Endpoint<PriceViewOpenRequest, PriceViewData>("ficc.price.view.open");
        public static readonly Endpoint<ViewQueryRequest, PriceViewData> PriceViewQuery =
            new Endpoint<ViewQueryRequest, PriceViewData>("ficc.price.view.query");
        public static readonly Endpoint<ViewCloseRequest, ViewCloseData> PriceViewClose =
            new Endpoint<ViewCloseRequest, ViewCloseData>("ficc.price.view.close");
        public static readonly Endpoint<Request, StreamOpenData> NotificationStreamOpen =
            new Endpoint<Request, StreamOpenData>("ficc.notification.stream.open");
        public static readonly Endpoint<NotificationViewOpenRequest, NotificationViewData> NotificationViewOpen =
            new Endpoint<NotificationViewOpenRequest, NotificationViewData>("ficc.notification.view.open");
        public static readonly Endpoint<ViewQueryRequest, NotificationViewData> NotificationViewQuery =
            new Endpoint<ViewQueryRequest, NotificationViewData>("ficc.notification.view.query");
        public static readonly Endpoint<ViewCloseRequest, ViewCloseData> NotificationViewClose =
            new Endpoint<ViewCloseRequest, ViewCloseData>("ficc.notification.view.close");
        public static readonly Endpoint<DictionaryRequest, DictionaryData> QuoteDictionaryQuery =
            new Endpoint<DictionaryRequest, DictionaryData>("ficc.quote.dictionary.query");
        public static readonly Endpoint<DictionaryRequest, DictionaryData> PriceDictionaryQuery =
            new Endpoint<DictionaryRequest, DictionaryData>("ficc.price.dictionary.query");
        public static readonly Endpoint<PriceQueryRequest, PriceQueryData> PriceQuery =
            new Endpoint<PriceQueryRequest, PriceQueryData>("ficc.price.query");
        public static readonly Endpoint<WatchlistQueryRequest, WatchlistData> WatchlistQuery =
            new Endpoint<WatchlistQueryRequest, WatchlistData>("ficc.watchlist.query");
        public static readonly Endpoint<WatchlistSaveRequest, WatchlistData> WatchlistSave =
            new Endpoint<WatchlistSaveRequest, WatchlistData>("ficc.watchlist.save");
        public static readonly Endpoint<WatchlistDeleteRequest, WatchlistData> WatchlistDelete =
            new Endpoint<WatchlistDeleteRequest, WatchlistData>("ficc.watchlist.delete");
        public static readonly Endpoint<FilterGroupQueryRequest, FilterGroupData> FilterGroupQuery =
            new Endpoint<FilterGroupQueryRequest, FilterGroupData>("ficc.filterGroup.query");
        public static readonly Endpoint<FilterGroupSaveRequest, FilterGroupData> FilterGroupSave =
            new Endpoint<FilterGroupSaveRequest, FilterGroupData>("ficc.filterGroup.save");
        public static readonly Endpoint<FilterGroupDeleteRequest, FilterGroupData> FilterGroupDelete =
            new Endpoint<FilterGroupDeleteRequest, FilterGroupData>("ficc.filterGroup.delete");
        public static readonly Endpoint<FilterGroupQueryRequest, FilterGroupData> NotificationFilterGroupQuery =
            new Endpoint<FilterGroupQueryRequest, FilterGroupData>("ficc.notification.filterGroup.query");
        public static readonly Endpoint<FilterGroupSaveRequest, FilterGroupData> NotificationFilterGroupSave =
            new Endpoint<FilterGroupSaveRequest, FilterGroupData>("ficc.notification.filterGroup.save");
        public static readonly Endpoint<FilterGroupDeleteRequest, FilterGroupData> NotificationFilterGroupDelete =
            new Endpoint<FilterGroupDeleteRequest, FilterGroupData>("ficc.notification.filterGroup.delete");
        public static readonly Endpoint<NotificationQueryRequest, NotificationQueryData> NotificationQuery =
            new Endpoint<NotificationQueryRequest, NotificationQueryData>("ficc.notification.query");
        public static readonly Endpoint<NotificationReadRequest, NotificationReadData> NotificationRead =
            new Endpoint<NotificationReadRequest, NotificationReadData>("ficc.notification.read");
        public static readonly Endpoint<Request, NotificationPreferenceData> NotificationPreferenceGet =
            new Endpoint<Request, NotificationPreferenceData>("ficc.notification.preference.get");
        public static readonly Endpoint<NotificationPreferenceSaveRequest, NotificationPreferenceData> NotificationPreferenceSave =
            new Endpoint<NotificationPreferenceSaveRequest, NotificationPreferenceData>("ficc.notification.preference.save");
        public static readonly Endpoint<BaseGetRequest, BaseData> BaseGet =
            new Endpoint<BaseGetRequest, BaseData>("ficc.base.get");
        public static readonly Endpoint<BaseQueryRequest, BaseData> BaseQuery =
            new Endpoint<BaseQueryRequest, BaseData>("ficc.base.query");
        public static readonly Endpoint<StateQueryRequest, QuoteStateData> QuoteStateQuery =
            new Endpoint<StateQueryRequest, QuoteStateData>("ficc.quote.state.query");
        public static readonly Endpoint<EventQueryRequest, QuoteEventBatch> QuoteEventQuery =
            new Endpoint<EventQueryRequest, QuoteEventBatch>("ficc.quote.event.query");
        public static readonly Endpoint<StateQueryRequest, NotificationStateData> NotificationStateQuery =
            new Endpoint<StateQueryRequest, NotificationStateData>("ficc.notification.state.query");
        public static readonly Endpoint<EventQueryRequest, NotificationEventBatch> NotificationEventQuery =
            new Endpoint<EventQueryRequest, NotificationEventBatch>("ficc.notification.event.query");
        public static readonly Endpoint<StateQueryRequest, QuoteStateData> InternalQuoteState =
            new Endpoint<StateQueryRequest, QuoteStateData>("ficc.quote.internal.state.query");
        public static readonly Endpoint<Request, PriceFeedBatch> InternalPriceSnapshot =
            new Endpoint<Request, PriceFeedBatch>("ficc.price.internal.snapshot.query");
        public static readonly Endpoint<CrmSearchRequest, BaseData> CrmSearch =
            new Endpoint<CrmSearchRequest, BaseData>("stc.crm.search");
        public static readonly Endpoint<CrmGetRequest, CrmGetData> CrmGet =
            new Endpoint<CrmGetRequest, CrmGetData>("stc.crm.get");
        public static readonly Endpoint<CrmLookupRequest, CrmLegacyData> CrmLookup =
            new Endpoint<CrmLookupRequest, CrmLegacyData>("stc.crm.lookup");
        public static readonly Endpoint<CrmLookupRequest, CrmLegacyData> CrmInstitutionQuery =
            new Endpoint<CrmLookupRequest, CrmLegacyData>("stc.crm.inst.query");
        public static readonly Endpoint<CrmLookupRequest, CrmLegacyData> CrmContactQuery =
            new Endpoint<CrmLookupRequest, CrmLegacyData>("stc.crm.contact.query");
        public static readonly Endpoint<CrmLookupRequest, CrmLegacyData> CrmAccountQuery =
            new Endpoint<CrmLookupRequest, CrmLegacyData>("stc.crm.account.query");
        public static readonly Endpoint<Request, QtIntakeStatusData> QtIntakeStatus =
            new Endpoint<Request, QtIntakeStatusData>("ficc.qtintake.status.query");
    }
}
