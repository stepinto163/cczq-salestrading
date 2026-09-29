import { endpointFor, requestProtocol } from './app-protocol.js';

export class FilteredView {
    constructor(transport, domain, collection, idField, { onChange = () => {}, onStale = () => {} } = {}) {
        Object.assign(this, { transport, domain, collection, idField, onChange, onStale });
        this.server = domain === 'notification' ? 'NotificationSvr' : 'QuoteSvr';
        this.records = new Map();
        this.ids = [];
        this.sequence = 0n;
        this.streamId = null;
        this.viewId = null;
        this.opening = false;
        this.buffer = [];
        this.stale = true;
        this.totalCount = 0;
        this.pageNumber = 1;
        this.pageSize = 50;
        this.requestQueue = Promise.resolve();
        this.browsing = false;
        this.changedIds = new Set();
        this.unavailableIds = new Set();
        this.generation = 0;
    }

    beginBrowsing() {
        if (this.browsing) return;
        this.browsing = true;
        clearTimeout(this.refillTimer);
        this.refillTimer = null;
        this.onChange();
    }

    resumeLive() {
        return this.enqueue(async () => {
            await this.load(() => this.call('view.query', {
                viewId: this.viewId, pageNumber: 1, pageSize: this.pageSize
            }), true);
        });
    }

    call(suffix, body = {}) {
        return requestProtocol(this.transport, endpointFor(`ficc.${this.domain}.${suffix}`), body);
    }

    async initialize(query = {}) {
        const stream = await this.call('stream.open');
        this.streamId = stream.streamId;
        this.opening = true;
        await this.transport.subscribe(stream.pushTopic, message => this.receive(message));
        await this.open(query);
    }

    enqueue(action) {
        const task = this.requestQueue.then(action);
        this.requestQueue = task.catch(() => {});
        return task;
    }

    open(query = this.query || {}) {
        return this.enqueue(async () => {
            this.query = { ...query };
            this.pageSize = query.pageSize || 50;
            await this.load(async () => {
                if (this.viewId) await this.call('view.close', { viewId: this.viewId });
                this.viewId = `WEB-V-${crypto.randomUUID()}`;
                return this.call('view.open', { ...this.query, viewId: this.viewId, pageSize: this.pageSize });
            }, true);
        });
    }

    queryPage(pageNumber) {
        const viewId = this.viewId;
        return this.enqueue(async () => {
            if (this.viewId !== viewId || this.stale || pageNumber < 1) return;
            await this.load(() => this.call('view.query', { viewId, pageNumber, pageSize: this.pageSize }));
        });
    }

    async load(request, resumeLive = false) {
        this.opening = true;
        this.buffer = [];
        this.overflow = false;
        try {
            this.install(await request());
            if (resumeLive) {
                this.browsing = false;
                this.changedIds.clear();
            }
            this.stale = false;
            if (this.overflow) this.invalidate('刷新期间更新过多，请重新同步');
        } catch (error) {
            this.invalidate(error.message);
            throw error;
        } finally {
            this.opening = false;
            const buffered = this.buffer;
            this.buffer = [];
            for (const message of buffered) this.receive(message);
            this.onChange();
        }
    }

    install(reply) {
        if (reply.streamId !== this.streamId || reply.view.viewId !== this.viewId) {
            throw new Error('视图回复不匹配');
        }
        this.records = new Map(reply[this.collection].map(row => [row[this.idField], row]));
        this.generation++;
        this.unavailableIds.clear();
        this.ids = [...reply.view.ids];
        this.totalCount = reply.view.totalCount;
        this.pageNumber = reply.view.pageNumber;
        this.pageSize = reply.view.pageSize;
        this.sequence = BigInt(reply.sequence);
    }

    invalidate(reason) {
        this.stale = true;
        this.onStale(reason);
    }

    requestRefill() {
        if (this.refillTimer || this.browsing || this.pageNumber !== 1 || this.stale) return;
        const viewId = this.viewId;
        this.refillTimer = setTimeout(() => {
            this.refillTimer = null;
            if (this.viewId === viewId && !this.browsing && this.pageNumber === 1 && !this.stale) {
                this.queryPage(1).catch(() => {});
            }
        }, 40);
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
        if (message.kind === 'SNAPSHOT' && this.domain === 'price') {
            const view = message.views?.find(view => view.viewId === this.viewId);
            if (next < this.sequence) return;
            if (this.browsing) return this.invalidate('行情重新同步，请点击返回实时');
            if (!view) return this.invalidate('行情视图需要重新建立');
            this.install({ ...message, view, books: message.books.filter(book => book.viewId === this.viewId) });
            this.onChange();
            return;
        }
        if (next <= this.sequence) return;
        if (message.kind === 'RESET_REQUIRED' || next !== this.sequence + 1n) {
            return this.invalidate('更新序号不连续或服务要求重新同步');
        }
        this.sequence = next;
        if (message.kind === 'HEARTBEAT') return;
        const incoming = new Map((message[this.collection] || [])
            .filter(row => this.domain !== 'price' || row.viewId === this.viewId)
            .map(row => [row[this.idField], row]));
        let refill = false;
        for (const change of message.views || []) {
            if (change.viewId !== this.viewId) continue;
            if (this.browsing) {
                for (const id of change.removeIds || []) {
                    this.changedIds.add(id);
                    if (this.records.has(id)) this.unavailableIds.add(id);
                }
                for (const id of change.upsertIds || []) {
                    this.changedIds.add(id);
                    if (!this.records.has(id)) continue;
                    const row = incoming.get(id);
                    if (!row) return this.invalidate('视图更新缺少数据');
                    this.records.set(id, row);
                    this.unavailableIds.delete(id);
                }
                this.totalCount = change.totalCount;
                continue;
            }
            for (const id of change.removeIds || []) {
                if (this.ids.includes(id)) refill = true;
                this.ids = this.ids.filter(existing => existing !== id);
            }
            for (const id of change.upsertIds || []) {
                if (this.pageNumber !== 1 && !this.ids.includes(id)) continue;
                const row = incoming.get(id);
                if (!row) return this.invalidate('视图更新缺少数据');
                const previous = this.records.get(id);
                if (previous && Date.parse(row.updateTime) < Date.parse(previous.updateTime)) refill = true;
                this.records.set(id, row);
                if (!this.ids.includes(id)) this.ids.push(id);
            }
            this.totalCount = change.totalCount;
        }
        if (this.pageNumber === 1 && !this.browsing) {
            this.ids.sort((left, right) => Date.parse(this.records.get(right).updateTime)
                - Date.parse(this.records.get(left).updateTime) || (left < right ? -1 : left > right ? 1 : 0));
            this.ids = this.ids.slice(0, this.pageSize);
        }
        const visible = new Set(this.ids);
        for (const id of this.records.keys()) if (!visible.has(id)) this.records.delete(id);
        this.onChange();
        if (refill) this.requestRefill();
    }

    rows() {
        return this.ids.map(id => this.records.get(id)).filter(Boolean);
    }
}
