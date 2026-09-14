export class FilteredView {
    constructor(transport, domain, collection, idField, { onChange = () => {}, onStale = () => {} } = {}) {
        this.transport = transport;
        this.domain = domain;
        this.server = domain === 'notification' ? 'NotificationSvr' : 'QuoteSvr';
        this.collection = collection;
        this.idField = idField;
        this.onChange = onChange;
        this.onStale = onStale;
        this.records = new Map();
        this.ids = [];
        this.sequence = 0n;
        this.streamId = null;
        this.viewId = null;
        this.opening = false;
        this.buffer = [];
        this.stale = true;
        this.totalCount = 0;
    }

    call(suffix, body = {}) {
        return this.transport.request(this.server, `ficc.${this.domain}.${suffix}`, body);
    }

    async initialize(query = {}) {
        const stream = await this.call('stream.open');
        this.streamId = stream.streamId;
        this.opening = true;
        await this.transport.subscribe(stream.pushTopic, message => this.receive(message));
        await this.open(query);
    }

    async open(query = this.query || {}) {
        this.opening = true;
        this.stale = true;
        this.query = query;
        this.buffer = [];
        this.overflow = false;
        try {
            if (this.viewId) await this.call('view.close', { viewId: this.viewId });
            this.viewId = `WEB-V-${crypto.randomUUID()}`;
            const initial = await this.call('view.open', { ...query, viewId: this.viewId, limit: query.limit || 50 });
            this.records = new Map(initial[this.collection].map(row => [row[this.idField], row]));
            this.ids = [...initial.view.ids];
            this.totalCount = initial.view.totalCount;
            this.sequence = BigInt(initial.sequence);
            this.stale = false;
            if (this.overflow) this.invalidate('刷新期间更新过多，请重新同步');
        } catch (error) {
            this.invalidate(error.message);
            throw error;
        } finally {
            this.opening = false;
        }
        const buffered = this.buffer;
        this.buffer = [];
        for (const message of buffered) this.receive(message);
        this.onChange();
    }

    invalidate(reason) {
        this.stale = true;
        this.onStale(reason);
    }

    receive(message) {
        if (message.streamId !== this.streamId) return;
        if (this.opening) {
            if (this.buffer.length < 200) this.buffer.push(message);
            else this.overflow = true;
            return;
        }
        if (this.stale) return;
        const next = BigInt(message.sequence || '0');
        if (message.kind === 'RESET_REQUIRED') {
            this.invalidate('服务要求重新同步视图');
            return;
        }
        if (next <= this.sequence) return;
        if (next !== this.sequence + 1n) {
            this.invalidate('更新序号不连续，请重新同步');
            return;
        }
        this.sequence = next;
        if (message.kind === 'HEARTBEAT') return;
        for (const row of message[this.collection] || []) {
            if (this.domain !== 'price' || row.viewId === this.viewId) this.records.set(row[this.idField], row);
        }
        for (const change of message.views || []) {
            if (change.viewId !== this.viewId) continue;
            const removed = new Set(change.removeIds || []);
            this.ids = this.ids.filter(id => !removed.has(id));
            for (const placement of change.place || []) {
                this.ids = this.ids.filter(id => id !== placement.id);
                this.ids.splice(placement.index, 0, placement.id);
            }
            this.totalCount = change.totalCount;
        }
        const visible = new Set(this.ids);
        for (const id of this.records.keys()) if (!visible.has(id)) this.records.delete(id);
        if (this.ids.some(id => !this.records.has(id))) this.invalidate('视图缺少记录，请重新同步');
        this.onChange();
    }

    rows() {
        return this.ids.map(id => this.records.get(id)).filter(Boolean);
    }
}

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
