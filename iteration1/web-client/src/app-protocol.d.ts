// 应用消息字段；运行期 wire 数据不做数字转换。由 check-app-protocol.py 校验。
export interface ProtocolModel {}
export interface BaseData extends ProtocolModel {
    records?: ReferenceRecord[] | null;
    hasMore?: boolean | null;
    nextOffset?: number | null;
    totalCount?: number | null;
    nextCursor?: string | null;
}
export interface BaseGetRequest extends Request {
    kind?: string | null;
    ids?: string[] | null;
}
export interface BaseQueryRequest extends PageRequest {
    kind?: string | null;
    text?: string | null;
    institutionId?: string | null;
    offset?: number | null;
}
export interface CrmGetData extends ProtocolModel {
    record?: ReferenceRecord | null;
}
export interface CrmGetRequest extends Request {
    kind?: string | null;
    id?: string | null;
}
export interface CrmLegacyData extends ProtocolModel {
    Code?: string | null;
    Msg?: string | null;
    totalSize?: number | null;
    pageSize?: number | null;
    datas?: CrmLegacyRecord[] | null;
}
export interface CrmLegacyRecord extends ProtocolModel {
    idbkey?: string | null;
    crmid?: string | null;
    id?: string | null;
    shortnamecn?: string | null;
    fullnamecn?: string | null;
    pinyin?: string | null;
    pinyinfull?: string | null;
    instcode?: string | null;
    insttypevalue?: string | null;
    taglist?: string | null;
    contactcount?: number | null;
    namecn?: string | null;
    instid?: string | null;
    instname?: string | null;
    instfullnamecn?: string | null;
    ctctposition?: string | null;
    biztype?: string | null;
    commonuse?: string | null;
    isonboard?: string | null;
}
export interface CrmLookupRequest extends Request {
    kind?: string | null;
    name?: string | null;
    id?: string | null;
    match?: string | null;
    dealerStr?: string | null;
    PageSize?: number | null;
}
export interface CrmSearchRequest extends Request {
    kind?: string | null;
    text?: string | null;
    institutionId?: string | null;
    offset?: number | null;
    limit?: number | null;
}
export interface DictionaryData extends ProtocolModel {
    dictionaries?: Record<string, DictionaryOption[]> | null;
    filterFields?: FilterField[] | null;
}
export interface DictionaryOption extends ProtocolModel {
    value?: string | null;
    label?: string | null;
}
export interface DictionaryRequest extends Request {
    names?: string[] | null;
}
export interface EmptyData extends ProtocolModel {
}
export interface ErrorDetail extends ProtocolModel {
    type?: string | null;
    field?: string | null;
    quoteId?: string | null;
    expectedRevision?: string | null;
    currentRevision?: string | null;
}
export interface EventPosition extends ProtocolModel {
    epoch?: string | null;
    sequence?: string | null;
}
export interface EventQueryRequest extends Request {
    epoch?: string | null;
    afterSequence?: string | null;
    limit?: number | null;
}
export interface Filter extends ProtocolModel {
    conditions?: FilterCondition[] | null;
    market?: string | null;
    watchlistIds?: string[] | null;
    filterGroupIds?: string[] | null;
}
export interface FilterCondition extends ProtocolModel {
    conditionId?: string | null;
    field?: string | null;
    operator?: string | null;
    value?: unknown | null;
    displayOrder?: number | null;
}
export interface FilterField extends ProtocolModel {
    field?: string | null;
    label?: string | null;
    type?: string | null;
    operators?: string[] | null;
    enumValues?: DictionaryOption[] | null;
    unit?: string | null;
    source?: string | null;
    group?: string | null;
    scope?: string | null;
    enabled?: boolean | null;
    simulated?: boolean | null;
}
export interface FilterGroup extends ProtocolModel {
    filterGroupId?: string | null;
    name?: string | null;
    market?: string | null;
    scope?: string | null;
    revision?: string | null;
    userId?: string | null;
    createTime?: string | null;
    updateTime?: string | null;
    subgroups?: FilterSubgroup[] | null;
}
export interface FilterGroupData extends ProtocolModel {
    filterGroup?: FilterGroup | null;
    filterGroups?: FilterGroup[] | null;
    filterGroupId?: string | null;
    deleted?: boolean | null;
}
export interface FilterGroupDeleteRequest extends Request {
    filterGroupId?: string | null;
    expectedRevision?: string | null;
}
export interface FilterGroupQueryRequest extends Request {
    filterGroupId?: string | null;
    market?: string | null;
    scope?: string | null;
}
export interface FilterGroupSaveRequest extends Request {
    filterGroupId?: string | null;
    expectedRevision?: string | null;
    name?: string | null;
    market?: string | null;
    scope?: string | null;
    subgroups?: FilterSubgroup[] | null;
}
export interface FilterSubgroup extends ProtocolModel {
    subgroupId?: string | null;
    name?: string | null;
    displayOrder?: number | null;
    conditions?: FilterCondition[] | null;
}
export interface IntakeApplyData extends ProtocolModel {
    sourceMessageId?: string | null;
    results?: QuoteOperationResult[] | null;
}
export interface IntakeApplyRequest extends Request {
    sourceType?: string | null;
    externalMessageId?: string | null;
    sourceUserId?: string | null;
    sourceTime?: string | null;
    text?: string | null;
    quotes?: IntakeQuoteItem[] | null;
}
export interface IntakeIdentity extends QuoteSource {
    token?: string | null;
}
export interface IntakeQuoteItem extends QuoteInput {
    externalQuoteId?: string | null;
    externalRevision?: string | null;
    action?: string | null;
}
export interface MarketData extends ProtocolModel {
    SecurityID?: string | null;
    Symbol?: string | null;
    MarketIndicator?: string | null;
    VenueTypeGW?: string | null;
    Venues?: string | null;
    NoMDEntries?: MarketEntry[] | null;
}
export interface MarketEntry extends ProtocolModel {
    MDEntryID?: string | null;
    MDEntryType?: string | null;
    MDEntryPx?: string | null;
    MDEntrySize?: string | null;
    SettlType?: string | null;
    MDEntryPositionNo?: string | null;
    MDEntryDate?: string | null;
    MDEntryTime?: string | null;
    Yield?: string | null;
    YieldToExecution?: string | null;
    InstitutionID?: string | null;
    TraderID?: string | null;
}
export interface NotificationEvent extends ProtocolModel {
    sequence?: string | null;
    eventId?: string | null;
    type?: string | null;
    action?: string | null;
    notificationId?: string | null;
    record?: NotificationEventRecord | null;
}
export interface NotificationEventBatch extends ProtocolModel {
    Code?: string | null;
    Msg?: string | null;
    kind?: string | null;
    epoch?: string | null;
    sequence?: string | null;
    currentSequence?: string | null;
    fromSequence?: string | null;
    toSequence?: string | null;
    nextSequence?: string | null;
    seenSequence?: string | null;
    hasMore?: boolean | null;
    events?: NotificationEvent[] | null;
}
export interface NotificationEventRecord extends NotificationRecord {
    filterGroupId?: string | null;
    name?: string | null;
    market?: string | null;
    scope?: string | null;
    subgroups?: FilterSubgroup[] | null;
    watchlistEnabled?: boolean | null;
    filterGroupEnabled?: boolean | null;
    counterQuoteEnabled?: boolean | null;
    myBestEnabled?: boolean | null;
}
export interface NotificationPreference extends ProtocolModel {
    revision?: string | null;
    watchlistEnabled?: boolean | null;
    filterGroupEnabled?: boolean | null;
    counterQuoteEnabled?: boolean | null;
    myBestEnabled?: boolean | null;
    userId?: string | null;
    updateTime?: string | null;
}
export interface NotificationPreferenceData extends ProtocolModel {
    preference?: NotificationPreference | null;
}
export interface NotificationPreferenceSaveRequest extends Request {
    expectedRevision?: string | null;
    watchlistEnabled?: boolean | null;
    filterGroupEnabled?: boolean | null;
    counterQuoteEnabled?: boolean | null;
    myBestEnabled?: boolean | null;
}
export interface NotificationQueryData extends PageData {
    notifications?: NotificationRecord[] | null;
}
export interface NotificationQueryRequest extends PageRequest {
    readState?: string | null;
    types?: string[] | null;
    filter?: Filter | null;
}
export interface NotificationReadData extends ProtocolModel {
    results?: NotificationReadResult[] | null;
}
export interface NotificationReadRequest extends Request {
    notificationIds?: string[] | null;
    read?: boolean | null;
}
export interface NotificationReadResult extends ProtocolModel {
    notificationId?: string | null;
    Code?: string | null;
    Msg?: string | null;
    readTime?: string | null;
}
export interface NotificationReason extends ProtocolModel {
    type?: string | null;
    groupIds?: string[] | null;
    quoteIds?: string[] | null;
    ownQuoteId?: string | null;
    otherQuoteId?: string | null;
    text?: string | null;
}
export interface NotificationRecord extends ProtocolModel {
    notificationId?: string | null;
    userId?: string | null;
    securityId?: string | null;
    content?: string | null;
    contentToken?: string | null;
    alertKey?: string | null;
    active?: boolean | null;
    revision?: string | null;
    createTime?: string | null;
    updateTime?: string | null;
    readTime?: string | null;
    reasons?: NotificationReason[] | null;
    quoteIds?: string[] | null;
}
export interface NotificationStateData extends ProtocolModel {
    epoch?: string | null;
    sequence?: string | null;
    totalCount?: number | null;
    records?: NotificationStateRecords | null;
    nextCursor?: string | null;
    done?: boolean | null;
}
export interface NotificationStateRecords extends ProtocolModel {
    notifications?: NotificationRecord[] | null;
    preferences?: NotificationPreference[] | null;
    filterGroups?: FilterGroup[] | null;
}
export interface NotificationStreamBatch extends StreamMessage {
    notifications?: NotificationRecord[] | null;
    views?: ViewDelta[] | null;
    preference?: NotificationPreference | null;
}
export interface NotificationViewData extends ProtocolModel {
    streamId?: string | null;
    sequence?: string | null;
    view?: ViewPage | null;
    notifications?: NotificationRecord[] | null;
    preference?: NotificationPreference | null;
}
export interface NotificationViewOpenRequest extends Request {
    viewId?: string | null;
    pageSize?: number | null;
    readState?: string | null;
    types?: string[] | null;
    filter?: Filter | null;
}
export interface PageData extends ProtocolModel {
    nextCursor?: string | null;
    hasMore?: boolean | null;
    totalCount?: number | null;
}
export interface PageRequest extends Request {
    cursor?: string | null;
    limit?: number | null;
}
export interface ParserQuote extends ProtocolModel {
    chinaBondValuation?: string | null;
    institutionId?: string | null;
    institutionName?: string | null;
    price?: string | null;
    priceType?: string | null;
    qty?: string | null;
    securityId?: string | null;
    settlType?: string | null;
    side?: string | null;
    symbol?: string | null;
    text?: string | null;
    timeToMaturity?: string | null;
    traderId?: string | null;
    traderName?: string | null;
}
export interface ParserReply extends ProtocolModel {
    Code?: string | null;
    Msg?: string | null;
    Datas?: ParserQuote[] | null;
}
export interface ParserRequest extends ProtocolModel {
    userName?: string | null;
    content?: string | null;
}
export interface PriceBook extends ProtocolModel {
    securityId?: string | null;
    viewId?: string | null;
    updateTime?: string | null;
    bookRevision?: string | null;
    marketData?: MarketData | null;
    quoteRefs?: Record<string, QuoteReference> | null;
}
export interface PriceFeedBatch extends ProtocolModel {
    Code?: string | null;
    Msg?: string | null;
    kind?: string | null;
    epoch?: string | null;
    sequence?: string | null;
    currentSequence?: string | null;
    streamId?: string | null;
    quotePosition?: EventPosition | null;
    prices?: PriceBook[] | null;
}
export interface PriceQueryData extends PageData {
    books?: PriceBook[] | null;
}
export interface PriceQueryRequest extends PageRequest {
    securityIds?: string[] | null;
    mode?: string | null;
    depth?: number | null;
    filter?: Filter | null;
}
export interface PriceStreamBatch extends StreamMessage {
    books?: PriceBook[] | null;
    views?: ViewDelta[] | null;
}
export interface PriceViewData extends ProtocolModel {
    streamId?: string | null;
    sequence?: string | null;
    view?: ViewPage | null;
    books?: PriceBook[] | null;
}
export interface PriceViewOpenRequest extends Request {
    viewId?: string | null;
    pageSize?: number | null;
    mode?: string | null;
    depth?: number | null;
    filter?: Filter | null;
}
export interface QtIntakeStatusData extends ProtocolModel {
    fixConnected?: boolean | null;
    storageFailed?: boolean | null;
    quoteSvrOnline?: boolean | null;
}
export interface QuoteChangeItem extends QuoteInput {
    quoteId?: string | null;
    expectedRevision?: string | null;
    reason?: string | null;
}
export interface QuoteChangeRequest extends Request {
    quotes?: QuoteChangeItem[] | null;
}
export interface QuoteCreateItem extends QuoteInput {
    clientQuoteId?: string | null;
}
export interface QuoteCreateRequest extends Request {
    quotes?: QuoteCreateItem[] | null;
    intake?: IntakeIdentity | null;
}
export interface QuoteEvent extends ProtocolModel {
    sequence?: string | null;
    eventId?: string | null;
    type?: string | null;
    action?: string | null;
    quoteId?: string | null;
    quote?: QuoteRecord | null;
    record?: SettingRecord | null;
    requestId?: string | null;
}
export interface QuoteEventBatch extends ProtocolModel {
    Code?: string | null;
    Msg?: string | null;
    kind?: string | null;
    epoch?: string | null;
    sequence?: string | null;
    currentSequence?: string | null;
    fromSequence?: string | null;
    toSequence?: string | null;
    nextSequence?: string | null;
    seenSequence?: string | null;
    hasMore?: boolean | null;
    events?: QuoteEvent[] | null;
}
export interface QuoteGetData extends ProtocolModel {
    quote?: QuoteRecord | null;
}
export interface QuoteGetRequest extends Request {
    quoteId?: string | null;
}
export interface QuoteHistoryData extends PageData {
    quoteId?: string | null;
    records?: QuoteHistoryRecord[] | null;
}
export interface QuoteHistoryRecord extends ProtocolModel {
    eventId?: string | null;
    quoteId?: string | null;
    revision?: string | null;
    action?: string | null;
    operatorUserId?: string | null;
    operationTime?: string | null;
    before?: QuoteRecord | null;
    after?: QuoteRecord | null;
    reason?: string | null;
}
export interface QuoteHistoryRequest extends PageRequest {
    quoteId?: string | null;
}
export interface QuoteHistoryRow extends ProtocolModel {
    tradeDate?: string | null;
    quote?: QuoteRecord | null;
    reference?: ReferenceRecord | null;
}
export interface QuoteHistorySearchData extends PageData {
    rows?: QuoteHistoryRow[] | null;
}
export interface QuoteHistorySearchRequest extends PageRequest {
    fromDate?: string | null;
    toDate?: string | null;
    ownerScope?: string | null;
    filter?: Filter | null;
}
export interface QuoteInput extends ProtocolModel {
    securityId?: string | null;
    institutionId?: string | null;
    institutionName?: string | null;
    contactId?: string | null;
    contactName?: string | null;
    side?: string | null;
    quoteType?: string | null;
    yieldType?: string | null;
    price?: string | null;
    quantity?: string | null;
    quantityUnit?: string | null;
    currency?: string | null;
    tradeDate?: string | null;
    settlType?: string | null;
    urgent?: boolean | null;
    recommended?: boolean | null;
    whole?: boolean | null;
    oco?: boolean | null;
    packing?: boolean | null;
    remark?: string | null;
}
export interface QuoteOperationResult extends ProtocolModel {
    quoteId?: string | null;
    clientQuoteId?: string | null;
    revision?: string | null;
    Code?: string | null;
    Msg?: string | null;
    error?: ErrorDetail | null;
}
export interface QuoteQueryData extends PageData {
    quotes?: QuoteRecord[] | null;
    reference?: ReferenceRecord[] | null;
}
export interface QuoteQueryRequest extends PageRequest {
    ownerScope?: string | null;
    status?: string[] | null;
    filter?: Filter | null;
}
export interface QuoteRecord extends QuoteInput {
    quoteId?: string | null;
    clonedFromQuoteId?: string | null;
    clientQuoteId?: string | null;
    sourceMessageId?: string | null;
    source?: QuoteSource | null;
    ownerUserId?: string | null;
    status?: string | null;
    revision?: string | null;
    createdBy?: string | null;
    updatedBy?: string | null;
    confirmedBy?: string | null;
    createTime?: string | null;
    updateTime?: string | null;
    quoteTime?: string | null;
    confirmTime?: string | null;
}
export interface QuoteReference extends ProtocolModel {
    quoteId?: string | null;
    quoteRevision?: string | null;
}
export interface QuoteReport extends ProtocolModel {
    quoteId?: string | null;
    revision?: string | null;
    action?: string | null;
    requestId?: string | null;
    clientQuoteId?: string | null;
    quote?: QuoteRecord | null;
    eventId?: string | null;
    operatorUserId?: string | null;
    operationTime?: string | null;
}
export interface QuoteSource extends ProtocolModel {
    type?: string | null;
    originService?: string | null;
    sourceUserId?: string | null;
    ownerUserId?: string | null;
    externalMessageId?: string | null;
    originalMessageId?: string | null;
    externalQuoteId?: string | null;
    externalRevision?: string | null;
    sourceTime?: string | null;
    transType?: string | null;
    text?: string | null;
}
export interface QuoteStateData extends ProtocolModel {
    epoch?: string | null;
    sequence?: string | null;
    totalCount?: number | null;
    records?: QuoteStateRecords | null;
    nextCursor?: string | null;
    done?: boolean | null;
}
export interface QuoteStateRecords extends ProtocolModel {
    quotes?: QuoteRecord[] | null;
    watchlists?: Watchlist[] | null;
    filterGroups?: FilterGroup[] | null;
}
export interface QuoteStreamBatch extends StreamMessage {
    quotes?: QuoteRecord[] | null;
    reports?: QuoteReport[] | null;
    views?: ViewDelta[] | null;
    reference?: ReferenceRecord[] | null;
}
export interface QuoteViewData extends ProtocolModel {
    streamId?: string | null;
    sequence?: string | null;
    view?: ViewPage | null;
    quotes?: QuoteRecord[] | null;
    reference?: ReferenceRecord[] | null;
}
export interface QuoteViewOpenRequest extends Request {
    viewId?: string | null;
    pageSize?: number | null;
    ownerScope?: string | null;
    status?: string[] | null;
    filter?: Filter | null;
}
export interface QuoteWriteData extends ProtocolModel {
    results?: QuoteOperationResult[] | null;
}
export interface ReferenceRecord extends ProtocolModel {
    bidOfferSpread?: string | null;
    market?: string | null;
    valuationBasis?: string | null;
    id?: string | null;
    kind?: string | null;
    securityId?: string | null;
    securityName?: string | null;
    institutionId?: string | null;
    institutionName?: string | null;
    institutionShortName?: string | null;
    contactId?: string | null;
    contactName?: string | null;
    positionName?: string | null;
    institutionIds?: string[] | null;
    name?: string | null;
    shortName?: string | null;
    remainingYears?: string | null;
    bondType?: string | null;
    bondSubtype?: string | null;
    valuationYield?: string | null;
    valuationCleanPrice?: string | null;
    couponRate?: string | null;
    issuerId?: string | null;
    issuerName?: string | null;
    province?: string | null;
    city?: string | null;
    ownershipType?: string | null;
    rating?: string | null;
    isHighYield?: boolean | null;
    isPreListing?: boolean | null;
    outstandingAmount?: string | null;
    regionCategory?: string | null;
    localGovernmentRegion?: string | null;
    isTaxExempt?: boolean | null;
    couponFrequency?: string | null;
    productType?: string | null;
    hotTags?: string[] | null;
    ncdType?: string | null;
    listingStatus?: string | null;
    issueMethod?: string | null;
    csiValuationYield?: string | null;
    liquidityGrade?: string | null;
    marketMakerTags?: string[] | null;
    listedMarket?: string | null;
    issuerType?: string | null;
    bankType?: string | null;
    industry?: string | null;
    guaranteeType?: string | null;
    guarantorName?: string | null;
    issuerRating?: string | null;
    issuerCbrRating?: string | null;
    ratingOutlook?: string | null;
    bondRating?: string | null;
    bondCbrRating?: string | null;
    yyRating?: string | null;
    csiRating?: string | null;
    optionType?: string | null;
    hasExerciseOption?: boolean | null;
    repaymentType?: string | null;
    couponType?: string | null;
    perpetualType?: string | null;
    isPerpetual?: boolean | null;
    isSubordinated?: boolean | null;
    rateIndex?: string | null;
    issueYear?: string | null;
    isMunicipal?: boolean | null;
    region?: string | null;
    regionCode?: string | null;
    keyProvince?: string | null;
    municipalLevel?: string | null;
    regionalRating?: string | null;
    localGovernmentType?: string | null;
    localGovernmentUse?: string | null;
    isCrossMarket?: boolean | null;
    isMortgage?: boolean | null;
}
export interface Reply extends ProtocolModel {
    Code?: string | null;
    Msg?: string | null;
    requestId?: string | null;
    error?: ErrorDetail | null;
}
export interface Request extends ProtocolModel {
    requestId?: string | null;
}
export interface SettingRecord extends FilterGroup {
    watchlistId?: string | null;
    securities?: WatchlistSecurity[] | null;
}
export interface SourceMessage extends QuoteSource {
    sourceMessageId?: string | null;
    sourceType?: string | null;
    quoteIds?: string[] | null;
}
export interface SourceMessageData extends PageData {
    sourceMessages?: SourceMessage[] | null;
}
export interface SourceMessageRequest extends PageRequest {
    sourceMessageId?: string | null;
    quoteId?: string | null;
}
export interface StateQueryRequest extends PageRequest {
    collections?: string[] | null;
}
export interface StreamMessage extends ProtocolModel {
    Code?: string | null;
    Msg?: string | null;
    kind?: string | null;
    streamId?: string | null;
    sequence?: string | null;
    reason?: string | null;
}
export interface StreamOpenData extends ProtocolModel {
    sessionId?: string | null;
    streamId?: string | null;
    pushTopic?: string | null;
}
export interface ViewCloseData extends ProtocolModel {
    viewId?: string | null;
    closed?: boolean | null;
}
export interface ViewCloseRequest extends Request {
    viewId?: string | null;
}
export interface ViewDelta extends ProtocolModel {
    viewId?: string | null;
    totalCount?: number | null;
    upsertIds?: string[] | null;
    removeIds?: string[] | null;
}
export interface ViewPage extends ProtocolModel {
    viewId?: string | null;
    pageNumber?: number | null;
    pageSize?: number | null;
    totalCount?: number | null;
    ids?: string[] | null;
    hasMore?: boolean | null;
}
export interface ViewQueryRequest extends Request {
    viewId?: string | null;
    pageNumber?: number | null;
    pageSize?: number | null;
}
export interface Watchlist extends ProtocolModel {
    watchlistId?: string | null;
    name?: string | null;
    market?: string | null;
    scope?: string | null;
    revision?: string | null;
    userId?: string | null;
    createTime?: string | null;
    updateTime?: string | null;
    securities?: WatchlistSecurity[] | null;
}
export interface WatchlistData extends ProtocolModel {
    watchlist?: Watchlist | null;
    watchlists?: Watchlist[] | null;
    watchlistId?: string | null;
    deleted?: boolean | null;
}
export interface WatchlistDeleteRequest extends Request {
    watchlistId?: string | null;
    expectedRevision?: string | null;
}
export interface WatchlistQueryRequest extends Request {
    watchlistId?: string | null;
    market?: string | null;
    scope?: string | null;
}
export interface WatchlistSaveRequest extends Request {
    watchlistId?: string | null;
    name?: string | null;
    market?: string | null;
    scope?: string | null;
    expectedRevision?: string | null;
    securities?: WatchlistSecurity[] | null;
}
export interface WatchlistSecurity extends ProtocolModel {
    securityId?: string | null;
    displayOrder?: number | null;
}
export interface ApiReply<T extends ProtocolModel> extends Reply { Data?: T | null; }
export interface Endpoint<Q, R> { readonly topic: string; readonly service: string; }
export const SalesTradingApi: {
    readonly QuoteCreate: Endpoint<QuoteCreateRequest, QuoteWriteData>;
    readonly QuoteConfirm: Endpoint<QuoteChangeRequest, QuoteWriteData>;
    readonly QuoteUpdate: Endpoint<QuoteChangeRequest, QuoteWriteData>;
    readonly QuoteRefer: Endpoint<QuoteChangeRequest, QuoteWriteData>;
    readonly QuoteUnrefer: Endpoint<QuoteChangeRequest, QuoteWriteData>;
    readonly QuoteDelete: Endpoint<QuoteChangeRequest, QuoteWriteData>;
    readonly QuoteClone: Endpoint<QuoteChangeRequest, QuoteWriteData>;
    readonly QuoteGet: Endpoint<QuoteGetRequest, QuoteGetData>;
    readonly QuoteQuery: Endpoint<QuoteQueryRequest, QuoteQueryData>;
    readonly QuoteHistoryQuery: Endpoint<QuoteHistoryRequest, QuoteHistoryData>;
    readonly QuoteHistorySearch: Endpoint<QuoteHistorySearchRequest, QuoteHistorySearchData>;
    readonly QuoteSourceMessageQuery: Endpoint<SourceMessageRequest, SourceMessageData>;
    readonly QuoteIntakeApply: Endpoint<IntakeApplyRequest, IntakeApplyData>;
    readonly QuoteStreamOpen: Endpoint<Request, StreamOpenData>;
    readonly QuoteViewOpen: Endpoint<QuoteViewOpenRequest, QuoteViewData>;
    readonly QuoteViewQuery: Endpoint<ViewQueryRequest, QuoteViewData>;
    readonly QuoteViewClose: Endpoint<ViewCloseRequest, ViewCloseData>;
    readonly PriceStreamOpen: Endpoint<Request, StreamOpenData>;
    readonly PriceViewOpen: Endpoint<PriceViewOpenRequest, PriceViewData>;
    readonly PriceViewQuery: Endpoint<ViewQueryRequest, PriceViewData>;
    readonly PriceViewClose: Endpoint<ViewCloseRequest, ViewCloseData>;
    readonly NotificationStreamOpen: Endpoint<Request, StreamOpenData>;
    readonly NotificationViewOpen: Endpoint<NotificationViewOpenRequest, NotificationViewData>;
    readonly NotificationViewQuery: Endpoint<ViewQueryRequest, NotificationViewData>;
    readonly NotificationViewClose: Endpoint<ViewCloseRequest, ViewCloseData>;
    readonly QuoteDictionaryQuery: Endpoint<DictionaryRequest, DictionaryData>;
    readonly PriceDictionaryQuery: Endpoint<DictionaryRequest, DictionaryData>;
    readonly PriceQuery: Endpoint<PriceQueryRequest, PriceQueryData>;
    readonly WatchlistQuery: Endpoint<WatchlistQueryRequest, WatchlistData>;
    readonly WatchlistSave: Endpoint<WatchlistSaveRequest, WatchlistData>;
    readonly WatchlistDelete: Endpoint<WatchlistDeleteRequest, WatchlistData>;
    readonly FilterGroupQuery: Endpoint<FilterGroupQueryRequest, FilterGroupData>;
    readonly FilterGroupSave: Endpoint<FilterGroupSaveRequest, FilterGroupData>;
    readonly FilterGroupDelete: Endpoint<FilterGroupDeleteRequest, FilterGroupData>;
    readonly NotificationFilterGroupQuery: Endpoint<FilterGroupQueryRequest, FilterGroupData>;
    readonly NotificationFilterGroupSave: Endpoint<FilterGroupSaveRequest, FilterGroupData>;
    readonly NotificationFilterGroupDelete: Endpoint<FilterGroupDeleteRequest, FilterGroupData>;
    readonly NotificationQuery: Endpoint<NotificationQueryRequest, NotificationQueryData>;
    readonly NotificationRead: Endpoint<NotificationReadRequest, NotificationReadData>;
    readonly NotificationPreferenceGet: Endpoint<Request, NotificationPreferenceData>;
    readonly NotificationPreferenceSave: Endpoint<NotificationPreferenceSaveRequest, NotificationPreferenceData>;
    readonly BaseGet: Endpoint<BaseGetRequest, BaseData>;
    readonly BaseQuery: Endpoint<BaseQueryRequest, BaseData>;
    readonly QuoteStateQuery: Endpoint<StateQueryRequest, QuoteStateData>;
    readonly QuoteEventQuery: Endpoint<EventQueryRequest, QuoteEventBatch>;
    readonly NotificationStateQuery: Endpoint<StateQueryRequest, NotificationStateData>;
    readonly NotificationEventQuery: Endpoint<EventQueryRequest, NotificationEventBatch>;
    readonly InternalQuoteState: Endpoint<StateQueryRequest, QuoteStateData>;
    readonly InternalPriceSnapshot: Endpoint<Request, PriceFeedBatch>;
    readonly CrmSearch: Endpoint<CrmSearchRequest, BaseData>;
    readonly CrmGet: Endpoint<CrmGetRequest, CrmGetData>;
    readonly CrmLookup: Endpoint<CrmLookupRequest, CrmLegacyData>;
    readonly CrmInstitutionQuery: Endpoint<CrmLookupRequest, CrmLegacyData>;
    readonly CrmContactQuery: Endpoint<CrmLookupRequest, CrmLegacyData>;
    readonly CrmAccountQuery: Endpoint<CrmLookupRequest, CrmLegacyData>;
    readonly QtIntakeStatus: Endpoint<Request, QtIntakeStatusData>;
};
export function endpointFor(topic: string): Endpoint<ProtocolModel, ProtocolModel>;
export function requestProtocol<Q, R>(transport: { request(service: string, topic: string, body: Q): Promise<R> }, endpoint: Endpoint<Q, R>, body: Q): Promise<R>;
