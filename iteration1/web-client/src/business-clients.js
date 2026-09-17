import { FilteredView } from './paged-view.js';
export { FilteredView } from './paged-view.js';

export class QuoteClient extends FilteredView {
    constructor(transport, callbacks) {
        super(transport, 'quote', 'quotes', 'quoteId', callbacks);
    }

    async create(input) {
        if (this.stale) throw new Error('报价视图未就绪，请先同步');
        return this.batchResult(await this.call('create', { quotes: [{ ...input, clientQuoteId: input.clientQuoteId || `WEB-Q-${crypto.randomUUID()}` }] }));
    }

    async action(action, quote) {
        if (this.stale) throw new Error('报价视图未就绪，请先同步');
        return this.batchResult(await this.call(action, { quotes: [{ quoteId: quote.quoteId, expectedRevision: quote.revision }] }));
    }

    batchResult(reply) {
        const result = reply.results?.[0];
        if (result?.Code !== '0000') throw new Error(`${result?.Code}: ${result?.Msg || '报价操作失败'}`);
        return result;
    }
}

export class PriceClient extends FilteredView {
    constructor(transport, callbacks) {
        super(transport, 'price', 'books', 'securityId', callbacks);
    }
}

export class NotificationClient extends FilteredView {
    constructor(transport, callbacks) {
        super(transport, 'notification', 'notifications', 'notificationId', callbacks);
    }

    async preferences() {
        return (await this.call('preference.get')).preference;
    }

    async setMyBest(enabled) {
        const current = await this.preferences();
        return this.call('preference.save', {
            expectedRevision: current.revision,
            watchlistEnabled: current.watchlistEnabled,
            filterGroupEnabled: current.filterGroupEnabled,
            counterQuoteEnabled: current.counterQuoteEnabled,
            myBestEnabled: enabled
        });
    }

    async markRead(notificationId) {
        const reply = await this.call('read', { notificationIds: [notificationId], read: true });
        if (reply.results?.[0]?.Code !== '0000') throw new Error(reply.results?.[0]?.Msg || '标记已读失败');
    }
}
