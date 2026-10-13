package ficc.api;

import java.util.*;

/** 应用协议目录：每个 topic 明确对应请求与 Data 类型；不包含网关传输协议。 */
public final class SalesTradingApi {
    private SalesTradingApi() {}
    private static final Map<String, Endpoint<?, ?>> ENDPOINTS = new LinkedHashMap<>();
    public static final Endpoint<RecommendationQueryRequest, RecommendationQueryData> RecommendationQuery =
            register(new Endpoint<>("ficc.recommendation.query", RecommendationQueryRequest.class, RecommendationQueryData.class));
    public static final Endpoint<RecommendationFeedbackRequest, RecommendationFeedbackData> RecommendationFeedback =
            register(new Endpoint<>("ficc.recommendation.feedback", RecommendationFeedbackRequest.class, RecommendationFeedbackData.class));
    public static final Endpoint<QuoteCreateRequest, QuoteWriteData> QuoteCreate =
            register(new Endpoint<>("ficc.quote.create", QuoteCreateRequest.class, QuoteWriteData.class));
    public static final Endpoint<QuoteChangeRequest, QuoteWriteData> QuoteConfirm =
            register(new Endpoint<>("ficc.quote.confirm", QuoteChangeRequest.class, QuoteWriteData.class));
    public static final Endpoint<QuoteChangeRequest, QuoteWriteData> QuoteUpdate =
            register(new Endpoint<>("ficc.quote.update", QuoteChangeRequest.class, QuoteWriteData.class));
    public static final Endpoint<QuoteChangeRequest, QuoteWriteData> QuoteRefer =
            register(new Endpoint<>("ficc.quote.refer", QuoteChangeRequest.class, QuoteWriteData.class));
    public static final Endpoint<QuoteChangeRequest, QuoteWriteData> QuoteUnrefer =
            register(new Endpoint<>("ficc.quote.unrefer", QuoteChangeRequest.class, QuoteWriteData.class));
    public static final Endpoint<QuoteChangeRequest, QuoteWriteData> QuoteDelete =
            register(new Endpoint<>("ficc.quote.delete", QuoteChangeRequest.class, QuoteWriteData.class));
    public static final Endpoint<QuoteChangeRequest, QuoteWriteData> QuoteClone =
            register(new Endpoint<>("ficc.quote.clone", QuoteChangeRequest.class, QuoteWriteData.class));
    public static final Endpoint<QuoteGetRequest, QuoteGetData> QuoteGet =
            register(new Endpoint<>("ficc.quote.get", QuoteGetRequest.class, QuoteGetData.class));
    public static final Endpoint<QuoteQueryRequest, QuoteQueryData> QuoteQuery =
            register(new Endpoint<>("ficc.quote.query", QuoteQueryRequest.class, QuoteQueryData.class));
    public static final Endpoint<QuoteHistoryRequest, QuoteHistoryData> QuoteHistoryQuery =
            register(new Endpoint<>("ficc.quote.history.query", QuoteHistoryRequest.class, QuoteHistoryData.class));
    public static final Endpoint<QuoteHistorySearchRequest, QuoteHistorySearchData> QuoteHistorySearch =
            register(new Endpoint<>("ficc.quote.history.search", QuoteHistorySearchRequest.class, QuoteHistorySearchData.class));
    public static final Endpoint<SourceMessageRequest, SourceMessageData> QuoteSourceMessageQuery =
            register(new Endpoint<>("ficc.quote.sourceMessage.query", SourceMessageRequest.class, SourceMessageData.class));
    public static final Endpoint<IntakeApplyRequest, IntakeApplyData> QuoteIntakeApply =
            register(new Endpoint<>("ficc.quote.intake.apply", IntakeApplyRequest.class, IntakeApplyData.class));
    public static final Endpoint<Request, StreamOpenData> QuoteStreamOpen =
            register(new Endpoint<>("ficc.quote.stream.open", Request.class, StreamOpenData.class));
    public static final Endpoint<QuoteViewOpenRequest, QuoteViewData> QuoteViewOpen =
            register(new Endpoint<>("ficc.quote.view.open", QuoteViewOpenRequest.class, QuoteViewData.class));
    public static final Endpoint<ViewQueryRequest, QuoteViewData> QuoteViewQuery =
            register(new Endpoint<>("ficc.quote.view.query", ViewQueryRequest.class, QuoteViewData.class));
    public static final Endpoint<ViewCloseRequest, ViewCloseData> QuoteViewClose =
            register(new Endpoint<>("ficc.quote.view.close", ViewCloseRequest.class, ViewCloseData.class));
    public static final Endpoint<Request, StreamOpenData> PriceStreamOpen =
            register(new Endpoint<>("ficc.price.stream.open", Request.class, StreamOpenData.class));
    public static final Endpoint<PriceViewOpenRequest, PriceViewData> PriceViewOpen =
            register(new Endpoint<>("ficc.price.view.open", PriceViewOpenRequest.class, PriceViewData.class));
    public static final Endpoint<ViewQueryRequest, PriceViewData> PriceViewQuery =
            register(new Endpoint<>("ficc.price.view.query", ViewQueryRequest.class, PriceViewData.class));
    public static final Endpoint<ViewCloseRequest, ViewCloseData> PriceViewClose =
            register(new Endpoint<>("ficc.price.view.close", ViewCloseRequest.class, ViewCloseData.class));
    public static final Endpoint<Request, StreamOpenData> NotificationStreamOpen =
            register(new Endpoint<>("ficc.notification.stream.open", Request.class, StreamOpenData.class));
    public static final Endpoint<NotificationViewOpenRequest, NotificationViewData> NotificationViewOpen =
            register(new Endpoint<>("ficc.notification.view.open", NotificationViewOpenRequest.class, NotificationViewData.class));
    public static final Endpoint<ViewQueryRequest, NotificationViewData> NotificationViewQuery =
            register(new Endpoint<>("ficc.notification.view.query", ViewQueryRequest.class, NotificationViewData.class));
    public static final Endpoint<ViewCloseRequest, ViewCloseData> NotificationViewClose =
            register(new Endpoint<>("ficc.notification.view.close", ViewCloseRequest.class, ViewCloseData.class));
    public static final Endpoint<DictionaryRequest, DictionaryData> QuoteDictionaryQuery =
            register(new Endpoint<>("ficc.quote.dictionary.query", DictionaryRequest.class, DictionaryData.class));
    public static final Endpoint<DictionaryRequest, DictionaryData> PriceDictionaryQuery =
            register(new Endpoint<>("ficc.price.dictionary.query", DictionaryRequest.class, DictionaryData.class));
    public static final Endpoint<PriceQueryRequest, PriceQueryData> PriceQuery =
            register(new Endpoint<>("ficc.price.query", PriceQueryRequest.class, PriceQueryData.class));
    public static final Endpoint<WatchlistQueryRequest, WatchlistData> WatchlistQuery =
            register(new Endpoint<>("ficc.watchlist.query", WatchlistQueryRequest.class, WatchlistData.class));
    public static final Endpoint<WatchlistSaveRequest, WatchlistData> WatchlistSave =
            register(new Endpoint<>("ficc.watchlist.save", WatchlistSaveRequest.class, WatchlistData.class));
    public static final Endpoint<WatchlistDeleteRequest, WatchlistData> WatchlistDelete =
            register(new Endpoint<>("ficc.watchlist.delete", WatchlistDeleteRequest.class, WatchlistData.class));
    public static final Endpoint<FilterGroupQueryRequest, FilterGroupData> FilterGroupQuery =
            register(new Endpoint<>("ficc.filterGroup.query", FilterGroupQueryRequest.class, FilterGroupData.class));
    public static final Endpoint<FilterGroupSaveRequest, FilterGroupData> FilterGroupSave =
            register(new Endpoint<>("ficc.filterGroup.save", FilterGroupSaveRequest.class, FilterGroupData.class));
    public static final Endpoint<FilterGroupDeleteRequest, FilterGroupData> FilterGroupDelete =
            register(new Endpoint<>("ficc.filterGroup.delete", FilterGroupDeleteRequest.class, FilterGroupData.class));
    public static final Endpoint<FilterGroupQueryRequest, FilterGroupData> NotificationFilterGroupQuery =
            register(new Endpoint<>("ficc.notification.filterGroup.query", FilterGroupQueryRequest.class, FilterGroupData.class));
    public static final Endpoint<FilterGroupSaveRequest, FilterGroupData> NotificationFilterGroupSave =
            register(new Endpoint<>("ficc.notification.filterGroup.save", FilterGroupSaveRequest.class, FilterGroupData.class));
    public static final Endpoint<FilterGroupDeleteRequest, FilterGroupData> NotificationFilterGroupDelete =
            register(new Endpoint<>("ficc.notification.filterGroup.delete", FilterGroupDeleteRequest.class, FilterGroupData.class));
    public static final Endpoint<NotificationQueryRequest, NotificationQueryData> NotificationQuery =
            register(new Endpoint<>("ficc.notification.query", NotificationQueryRequest.class, NotificationQueryData.class));
    public static final Endpoint<NotificationReadRequest, NotificationReadData> NotificationRead =
            register(new Endpoint<>("ficc.notification.read", NotificationReadRequest.class, NotificationReadData.class));
    public static final Endpoint<Request, NotificationPreferenceData> NotificationPreferenceGet =
            register(new Endpoint<>("ficc.notification.preference.get", Request.class, NotificationPreferenceData.class));
    public static final Endpoint<NotificationPreferenceSaveRequest, NotificationPreferenceData> NotificationPreferenceSave =
            register(new Endpoint<>("ficc.notification.preference.save", NotificationPreferenceSaveRequest.class, NotificationPreferenceData.class));
    public static final Endpoint<BaseGetRequest, BaseData> BaseGet =
            register(new Endpoint<>("ficc.base.get", BaseGetRequest.class, BaseData.class));
    public static final Endpoint<BaseQueryRequest, BaseData> BaseQuery =
            register(new Endpoint<>("ficc.base.query", BaseQueryRequest.class, BaseData.class));
    public static final Endpoint<StateQueryRequest, QuoteStateData> QuoteStateQuery =
            register(new Endpoint<>("ficc.quote.state.query", StateQueryRequest.class, QuoteStateData.class));
    public static final Endpoint<EventQueryRequest, QuoteEventBatch> QuoteEventQuery =
            register(new Endpoint<>("ficc.quote.event.query", EventQueryRequest.class, QuoteEventBatch.class));
    public static final Endpoint<StateQueryRequest, NotificationStateData> NotificationStateQuery =
            register(new Endpoint<>("ficc.notification.state.query", StateQueryRequest.class, NotificationStateData.class));
    public static final Endpoint<EventQueryRequest, NotificationEventBatch> NotificationEventQuery =
            register(new Endpoint<>("ficc.notification.event.query", EventQueryRequest.class, NotificationEventBatch.class));
    public static final Endpoint<StateQueryRequest, QuoteStateData> InternalQuoteState =
            register(new Endpoint<>("ficc.quote.internal.state.query", StateQueryRequest.class, QuoteStateData.class));
    public static final Endpoint<Request, PriceFeedBatch> InternalPriceSnapshot =
            register(new Endpoint<>("ficc.price.internal.snapshot.query", Request.class, PriceFeedBatch.class));
    public static final Endpoint<CrmSearchRequest, BaseData> CrmSearch =
            register(new Endpoint<>("stc.crm.search", CrmSearchRequest.class, BaseData.class));
    public static final Endpoint<CrmGetRequest, CrmGetData> CrmGet =
            register(new Endpoint<>("stc.crm.get", CrmGetRequest.class, CrmGetData.class));
    public static final Endpoint<CrmLookupRequest, CrmLegacyData> CrmLookup =
            register(new Endpoint<>("stc.crm.lookup", CrmLookupRequest.class, CrmLegacyData.class));
    public static final Endpoint<CrmLookupRequest, CrmLegacyData> CrmInstitutionQuery =
            register(new Endpoint<>("stc.crm.inst.query", CrmLookupRequest.class, CrmLegacyData.class));
    public static final Endpoint<CrmLookupRequest, CrmLegacyData> CrmContactQuery =
            register(new Endpoint<>("stc.crm.contact.query", CrmLookupRequest.class, CrmLegacyData.class));
    public static final Endpoint<CrmLookupRequest, CrmLegacyData> CrmAccountQuery =
            register(new Endpoint<>("stc.crm.account.query", CrmLookupRequest.class, CrmLegacyData.class));
    public static final Endpoint<Request, QtIntakeStatusData> QtIntakeStatus =
            register(new Endpoint<>("ficc.qtintake.status.query", Request.class, QtIntakeStatusData.class));
    private static <Q extends Request, R extends ProtocolModel> Endpoint<Q, R> register(Endpoint<Q, R> endpoint) {
        ENDPOINTS.put(endpoint.topic, endpoint);
        return endpoint;
    }
    public static Endpoint<?, ?> find(String topic) { return ENDPOINTS.get(topic); }
    public static Collection<Endpoint<?, ?>> all() { return Collections.unmodifiableCollection(ENDPOINTS.values()); }
}
