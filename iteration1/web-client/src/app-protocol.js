// Topic/服务目录；字段类型见 app-protocol.d.ts。
export const SalesTradingApi = Object.freeze({
    QuoteCreate: Object.freeze({ topic: 'ficc.quote.create', service: 'QuoteSvr' }),
    QuoteConfirm: Object.freeze({ topic: 'ficc.quote.confirm', service: 'QuoteSvr' }),
    QuoteUpdate: Object.freeze({ topic: 'ficc.quote.update', service: 'QuoteSvr' }),
    QuoteRefer: Object.freeze({ topic: 'ficc.quote.refer', service: 'QuoteSvr' }),
    QuoteUnrefer: Object.freeze({ topic: 'ficc.quote.unrefer', service: 'QuoteSvr' }),
    QuoteDelete: Object.freeze({ topic: 'ficc.quote.delete', service: 'QuoteSvr' }),
    QuoteClone: Object.freeze({ topic: 'ficc.quote.clone', service: 'QuoteSvr' }),
    QuoteGet: Object.freeze({ topic: 'ficc.quote.get', service: 'QuoteSvr' }),
    QuoteQuery: Object.freeze({ topic: 'ficc.quote.query', service: 'QuoteSvr' }),
    QuoteHistoryQuery: Object.freeze({ topic: 'ficc.quote.history.query', service: 'QuoteSvr' }),
    QuoteHistorySearch: Object.freeze({ topic: 'ficc.quote.history.search', service: 'QuoteSvr' }),
    QuoteSourceMessageQuery: Object.freeze({ topic: 'ficc.quote.sourceMessage.query', service: 'QuoteSvr' }),
    QuoteIntakeApply: Object.freeze({ topic: 'ficc.quote.intake.apply', service: 'QuoteSvr' }),
    QuoteStreamOpen: Object.freeze({ topic: 'ficc.quote.stream.open', service: 'QuoteSvr' }),
    QuoteViewOpen: Object.freeze({ topic: 'ficc.quote.view.open', service: 'QuoteSvr' }),
    QuoteViewQuery: Object.freeze({ topic: 'ficc.quote.view.query', service: 'QuoteSvr' }),
    QuoteViewClose: Object.freeze({ topic: 'ficc.quote.view.close', service: 'QuoteSvr' }),
    PriceStreamOpen: Object.freeze({ topic: 'ficc.price.stream.open', service: 'QuoteSvr' }),
    PriceViewOpen: Object.freeze({ topic: 'ficc.price.view.open', service: 'QuoteSvr' }),
    PriceViewQuery: Object.freeze({ topic: 'ficc.price.view.query', service: 'QuoteSvr' }),
    PriceViewClose: Object.freeze({ topic: 'ficc.price.view.close', service: 'QuoteSvr' }),
    NotificationStreamOpen: Object.freeze({ topic: 'ficc.notification.stream.open', service: 'NotificationSvr' }),
    NotificationViewOpen: Object.freeze({ topic: 'ficc.notification.view.open', service: 'NotificationSvr' }),
    NotificationViewQuery: Object.freeze({ topic: 'ficc.notification.view.query', service: 'NotificationSvr' }),
    NotificationViewClose: Object.freeze({ topic: 'ficc.notification.view.close', service: 'NotificationSvr' }),
    QuoteDictionaryQuery: Object.freeze({ topic: 'ficc.quote.dictionary.query', service: 'QuoteSvr' }),
    PriceDictionaryQuery: Object.freeze({ topic: 'ficc.price.dictionary.query', service: 'QuoteSvr' }),
    PriceQuery: Object.freeze({ topic: 'ficc.price.query', service: 'QuoteSvr' }),
    WatchlistQuery: Object.freeze({ topic: 'ficc.watchlist.query', service: 'QuoteSvr' }),
    WatchlistSave: Object.freeze({ topic: 'ficc.watchlist.save', service: 'QuoteSvr' }),
    WatchlistDelete: Object.freeze({ topic: 'ficc.watchlist.delete', service: 'QuoteSvr' }),
    FilterGroupQuery: Object.freeze({ topic: 'ficc.filterGroup.query', service: 'QuoteSvr' }),
    FilterGroupSave: Object.freeze({ topic: 'ficc.filterGroup.save', service: 'QuoteSvr' }),
    FilterGroupDelete: Object.freeze({ topic: 'ficc.filterGroup.delete', service: 'QuoteSvr' }),
    NotificationFilterGroupQuery: Object.freeze({ topic: 'ficc.notification.filterGroup.query', service: 'NotificationSvr' }),
    NotificationFilterGroupSave: Object.freeze({ topic: 'ficc.notification.filterGroup.save', service: 'NotificationSvr' }),
    NotificationFilterGroupDelete: Object.freeze({ topic: 'ficc.notification.filterGroup.delete', service: 'NotificationSvr' }),
    NotificationQuery: Object.freeze({ topic: 'ficc.notification.query', service: 'NotificationSvr' }),
    NotificationRead: Object.freeze({ topic: 'ficc.notification.read', service: 'NotificationSvr' }),
    NotificationPreferenceGet: Object.freeze({ topic: 'ficc.notification.preference.get', service: 'NotificationSvr' }),
    NotificationPreferenceSave: Object.freeze({ topic: 'ficc.notification.preference.save', service: 'NotificationSvr' }),
    BaseGet: Object.freeze({ topic: 'ficc.base.get', service: 'QuoteSvr' }),
    BaseQuery: Object.freeze({ topic: 'ficc.base.query', service: 'QuoteSvr' }),
    QuoteStateQuery: Object.freeze({ topic: 'ficc.quote.state.query', service: 'QuoteSvr' }),
    QuoteEventQuery: Object.freeze({ topic: 'ficc.quote.event.query', service: 'QuoteSvr' }),
    NotificationStateQuery: Object.freeze({ topic: 'ficc.notification.state.query', service: 'NotificationSvr' }),
    NotificationEventQuery: Object.freeze({ topic: 'ficc.notification.event.query', service: 'NotificationSvr' }),
    InternalQuoteState: Object.freeze({ topic: 'ficc.quote.internal.state.query', service: 'QuoteSvr' }),
    InternalPriceSnapshot: Object.freeze({ topic: 'ficc.price.internal.snapshot.query', service: 'QuoteSvr' }),
    CrmSearch: Object.freeze({ topic: 'stc.crm.search', service: 'CRMSvr' }),
    CrmGet: Object.freeze({ topic: 'stc.crm.get', service: 'CRMSvr' }),
    CrmLookup: Object.freeze({ topic: 'stc.crm.lookup', service: 'CRMSvr' }),
    CrmInstitutionQuery: Object.freeze({ topic: 'stc.crm.inst.query', service: 'CRMSvr' }),
    CrmContactQuery: Object.freeze({ topic: 'stc.crm.contact.query', service: 'CRMSvr' }),
    CrmAccountQuery: Object.freeze({ topic: 'stc.crm.account.query', service: 'CRMSvr' }),
    QtIntakeStatus: Object.freeze({ topic: 'ficc.qtintake.status.query', service: 'QtIntakeSvr' }),
});

const byTopic = new Map(Object.values(SalesTradingApi).map(endpoint => [endpoint.topic, endpoint]));
export function endpointFor(topic) {
    const endpoint = byTopic.get(topic);
    if (!endpoint) throw new Error(`Unknown application topic: ${topic}`);
    return endpoint;
}

export function requestProtocol(transport, endpoint, body = {}) {
    return transport.request(endpoint.service, endpoint.topic, body);
}
