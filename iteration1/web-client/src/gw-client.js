// Wire format taken from the supplied GW WebSocketProtoCodec and DPS ProtoUtils.
// The existing GW does framing, authentication, routing and subscriptions.
const encoder = new TextEncoder();
const decoder = new TextDecoder('utf-8', { fatal: true });
const MAX_PACKET_BYTES = 2 * 1024 * 1024;

export function encodePacket(command, sequence, sessionId, body, format = 0) {
    const session = encoder.encode(sessionId || '');
    const content = body == null ? new Uint8Array() : encoder.encode(JSON.stringify(body));
    const length = 20 + session.length + content.length;
    if (length > MAX_PACKET_BYTES) throw new Error('GW message exceeds the client limit');
    const buffer = new ArrayBuffer(length);
    const view = new DataView(buffer);
    view.setInt32(0, length);
    view.setInt16(4, 20);
    view.setInt32(6, command);
    view.setInt16(10, format);
    view.setInt32(12, sequence);
    view.setInt32(16, session.length);
    new Uint8Array(buffer, 20, session.length).set(session);
    new Uint8Array(buffer, 20 + session.length).set(content);
    return buffer;
}

export function decodePacket(buffer) {
    if (!(buffer instanceof ArrayBuffer) || buffer.byteLength < 20 || buffer.byteLength > MAX_PACKET_BYTES) {
        throw new Error('Invalid GW packet size');
    }
    const view = new DataView(buffer);
    const sessionLength = view.getInt32(16);
    if (view.getInt32(0) !== buffer.byteLength || view.getInt16(4) !== 20 || sessionLength < 0 || 20 + sessionLength > buffer.byteLength) {
        throw new Error('Invalid GW packet header');
    }
    const text = decoder.decode(new Uint8Array(buffer, 20 + sessionLength));
    return {
        command: view.getInt32(6), format: view.getInt16(10), sequence: view.getInt32(12),
        sessionId: decoder.decode(new Uint8Array(buffer, 20, sessionLength)),
        body: text ? JSON.parse(text) : null
    };
}

function redact(value) {
    if (Array.isArray(value)) return value.map(redact);
    if (value && typeof value === 'object') {
        return Object.fromEntries(Object.entries(value).map(([key, item]) => [key,
            /^(pwd|password|sid|sessionId|token)$/i.test(key) ? '[REDACTED]' : redact(item)]));
    }
    if (typeof value === 'string') {
        if (value.startsWith('{')) {
            try {
                return redact(JSON.parse(value));
            } catch {
                // Ordinary text is preserved.
            }
        }
        return value.replace(/(ficc\.(?:quote|price|notification)\.report\.).+/g, '$1[SESSION]');
    }
    return value;
}

export class GwClient {
    constructor(url, { timeoutMs = 5000, onLog = () => {}, onDisconnect = () => {} } = {}) {
        this.url = url;
        this.timeoutMs = timeoutMs;
        this.onLog = onLog;
        this.onDisconnect = onDisconnect;
        this.pending = new Map();
        this.listeners = new Map();
        this.sessionId = '';
        this.sequence = 0;
        this.socket = null;
        this.heartbeat = null;
        this.logs = [];
    }

    log(direction, topic, body) {
        const record = { time: new Date().toISOString(), direction, topic: redact(topic), body: redact(body) };
        this.logs.push(record);
        if (this.logs.length > 500) this.logs.shift();
        this.onLog(record);
    }

    async connect(userName, password) {
        this.close();
        const socket = new WebSocket(this.url);
        this.socket = socket;
        socket.binaryType = 'arraybuffer';
        socket.onmessage = event => {
            if (socket !== this.socket) return;
            try {
                this.receive(decodePacket(event.data));
            }
            catch (error) {
                this.log('ERROR', 'protocol', { message: error.message });
                this.close();
                this.onDisconnect();
            }
        };
        socket.onclose = () => {
            if (socket !== this.socket) return;
            this.clearConnection('GW connection closed');
            this.onDisconnect();
        };
        try {
            await new Promise((resolve, reject) => {
                const timeout = setTimeout(() => reject(new Error('GW WebSocket connection timed out')), this.timeoutMs);
                socket.onopen = () => {
                    clearTimeout(timeout);
                    resolve();
                };
                socket.onerror = () => {
                    clearTimeout(timeout);
                    reject(new Error('Cannot connect to GW WebSocket'));
                };
            });
            const reply = await this.exchange(0, { userName, pwd: password, extendParam: { clientType: 'client', param1: 'salestrading-web' } }, 'LOGIN');
            if (!['0000', '0001'].includes(reply?.Code) || !this.sessionId) {
                throw new Error(reply?.Msg || 'GW login failed');
            }
            this.heartbeat = setInterval(() => {
                if (socket.readyState === WebSocket.OPEN) {
                    this.log('SEND', 'HEARTBEAT', null);
                    socket.send(encodePacket(2, ++this.sequence, this.sessionId, null, 3));
                }
            }, 15000);
            return reply;
        } catch (error) {
            this.close();
            throw error;
        }
    }

    exchange(command, body, topic) {
        if (!this.socket || this.socket.readyState !== WebSocket.OPEN) return Promise.reject(new Error('GW is disconnected'));
        if (this.pending.size >= 64) return Promise.reject(new Error('Too many pending GW requests'));
        const sequence = ++this.sequence;
        this.log('SEND', topic, body);
        return new Promise((resolve, reject) => {
            const timer = setTimeout(() => {
                this.pending.delete(sequence);
                reject(new Error(`${topic} timed out; do not blindly repeat a write`));
            }, this.timeoutMs);
            this.pending.set(sequence, { resolve, reject, timer, topic });
            try {
                this.socket.send(encodePacket(command, sequence, this.sessionId, body));
            } catch (error) {
                clearTimeout(timer);
                this.pending.delete(sequence);
                reject(error);
            }
        });
    }

    receive(packet) {
        if (packet.format === 2) {
            const topic = packet.body?.topic;
            const raw = packet.body?.content;
            const content = typeof raw === 'string' ? JSON.parse(raw) : raw;
            this.log('PUSH', topic, content);
            for (const listener of this.listeners.get(topic) || []) listener(content);
            return;
        }
        const pending = this.pending.get(packet.sequence);
        if (!pending) {
            this.log('REPLY', `COMMAND-${packet.command}`, packet.body);
            return;
        }
        clearTimeout(pending.timer);
        this.pending.delete(packet.sequence);
        if (packet.command === 0 && ['0000', '0001'].includes(packet.body?.Code)) this.sessionId = packet.sessionId;
        this.log('REPLY', pending.topic, packet.body);
        pending.resolve(packet.body);
    }

    async request(serverName, topic, body = {}) {
        if (!this.sessionId) throw new Error('Login is required');
        const request = { ...body, requestId: body.requestId || `WEB-${crypto.randomUUID()}` };
        const reply = await this.exchange(3, { serverName, method: topic, content: JSON.stringify(request) }, topic);
        if (reply?.Code !== '0000') throw new Error(`${reply?.Code || 'ERROR'}: ${reply?.Msg || 'Request failed'}`);
        return reply.Data;
    }

    async subscribe(topic, listener) {
        if (this.listeners.has(topic)) throw new Error('Topic is already subscribed');
        this.listeners.set(topic, new Set([listener]));
        try {
            const reply = await this.exchange(3, { serverName: 'gateway', method: 'subscribe', content: topic }, topic);
            if (reply?.Code !== '0000') throw new Error(reply?.Msg || 'Subscription denied');
        } catch (error) {
            this.listeners.delete(topic);
            throw error;
        }
    }

    clearConnection(reason) {
        clearInterval(this.heartbeat);
        this.heartbeat = null;
        this.sessionId = '';
        this.listeners.clear();
        for (const pending of this.pending.values()) {
            clearTimeout(pending.timer);
            pending.reject(new Error(reason));
        }
        this.pending.clear();
    }

    close() {
        const socket = this.socket;
        this.socket = null;
        this.clearConnection('GW connection was closed');
        socket?.close();
    }
}
