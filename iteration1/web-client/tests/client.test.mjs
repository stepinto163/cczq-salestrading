import test from 'node:test';
import assert from 'node:assert/strict';
import { encodePacket, decodePacket, GwClient } from '../src/gw-client.js';
import { FilteredView } from '../src/business-clients.js';

test('DPS frame round trip preserves Unicode and byte lengths', () => {
    const packet = encodePacket(3, 42, '会话', { text: '报价', price: '2.5000' });
    assert.equal(new DataView(packet).getInt32(16), 6);
    assert.deepEqual(decodePacket(packet), { command: 3, format: 0, sequence: 42, sessionId: '会话', body: { text: '报价', price: '2.5000' } });
});

test('reject truncated frames and invalid session offsets', () => {
    const packet = encodePacket(3, 1, '', {});
    assert.throws(() => decodePacket(packet.slice(0, -1)), /header/);
    new DataView(packet).setInt32(16, -1);
    assert.throws(() => decodePacket(packet), /header/);
    assert.throws(() => decodePacket(new ArrayBuffer(10)), /size/);
});

test('message logs redact credentials and session topics inside request bodies', () => {
    const gateway = new GwClient('unused');
    gateway.log('SEND', 'LOGIN', { pwd: 'secret', content: JSON.stringify({ sessionId: 'private' }) });
    gateway.log('PUSH', 'ficc.quote.report.private-session', { pushTopic: 'ficc.price.report.private-session' });
    const log = JSON.stringify(gateway.logs);
    assert.ok(!log.includes('secret'));
    assert.ok(!log.includes('private'));
});

function viewFixture() {
    const view = new FilteredView({}, 'quote', 'quotes', 'quoteId');
    view.streamId = 'stream';
    view.viewId = 'view';
    view.stale = false;
    return view;
}

test('incremental insertion, reordering, replacement and removal follow server positions', () => {
    const view = viewFixture();
    view.receive({ streamId: 'stream', sequence: '1', quotes: [{ quoteId: 'a', revision: '1' }, { quoteId: 'b', revision: '1' }],
        views: [{ viewId: 'view', totalCount: 2, place: [{ id: 'a', index: 0 }, { id: 'b', index: 1 }] }] });
    view.receive({ streamId: 'stream', sequence: '2', quotes: [{ quoteId: 'b', revision: '2' }],
        views: [{ viewId: 'view', totalCount: 2, place: [{ id: 'b', index: 0 }] }] });
    assert.deepEqual(view.ids, ['b', 'a']);
    assert.equal(view.rows()[0].revision, '2');
    view.receive({ streamId: 'stream', sequence: '3', views: [{ viewId: 'view', totalCount: 1, removeIds: ['a'] }] });
    assert.deepEqual(view.ids, ['b']);
    assert.equal(view.records.has('a'), false);
});

test('duplicates and other streams are ignored; a sequence gap blocks updates', () => {
    const view = viewFixture();
    view.receive({ streamId: 'other', sequence: '1' });
    assert.equal(view.sequence, 0n);
    view.receive({ streamId: 'stream', sequence: '1', kind: 'HEARTBEAT' });
    view.receive({ streamId: 'stream', sequence: '1', quotes: [{ quoteId: 'unwanted' }] });
    assert.equal(view.records.size, 0);
    view.receive({ streamId: 'stream', sequence: '3' });
    assert.equal(view.stale, true);
    assert.equal(view.sequence, 1n);
});

test('updates received while opening are replayed after the reply baseline', async () => {
    let view;
    const transport = { request: async () => {
        view.receive({ streamId: 'stream', sequence: '8', quotes: [{ quoteId: 'a', revision: '2' }] });
        return { sequence: '7', quotes: [{ quoteId: 'a', revision: '1' }], view: { ids: ['a'], totalCount: 1 } };
    } };
    view = new FilteredView(transport, 'quote', 'quotes', 'quoteId');
    view.streamId = 'stream';
    await view.open();
    assert.equal(view.sequence, 8n);
    assert.equal(view.rows()[0].revision, '2');
    assert.equal(view.stale, false);
});

test('opening-buffer overflow requires resynchronization', async () => {
    let view;
    const transport = { request: async () => {
        for (let sequence = 1; sequence <= 201; sequence++) view.receive({ streamId: 'stream', sequence: String(sequence) });
        return { sequence: '201', quotes: [], view: { ids: [], totalCount: 0 } };
    } };
    view = new FilteredView(transport, 'quote', 'quotes', 'quoteId');
    view.streamId = 'stream';
    await view.open();
    assert.equal(view.stale, true);
    assert.equal(view.buffer.length, 0);
});
