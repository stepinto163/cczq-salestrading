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
