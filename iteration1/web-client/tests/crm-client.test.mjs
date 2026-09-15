import test from 'node:test';
import assert from 'node:assert/strict';
import { CrmClient } from '../src/crm-client.js';

test('CRM searches use DPS and preserve long string IDs and duplicate names', async () => {
    const calls = [];
    const client = new CrmClient({ request: async (server, topic, request) => {
        calls.push({ server, topic, request });
        return { records: request.kind === 'INSTITUTION'
            ? [{ institutionId: '9000000000000000001', institutionName: '测试银行' }]
            : ['c1', 'c2'].map(contactId => ({ contactId, contactName: '同名', institutionIds: ['9000000000000000001', 'other'] })) };
    } });
    const organizations = await client.search('INSTITUTION', '测试');
    const contacts = await client.search('CONTACT', '同名', organizations.items[0].value);
    assert.equal(calls[1].server, 'QuoteSvr');
    assert.equal(calls[1].topic, 'ficc.base.query');
    assert.equal(calls[1].request.institutionId, '9000000000000000001');
    assert.equal(calls[1].request.limit, 50);
    const resolved = client.resolve({ inst: organizations.items[0].value, tr: contacts.items[1].value });
    assert.equal(resolved.contactId, 'c2');
    assert.equal(resolved.institutionName, '测试银行');
    assert.equal(resolved.institutionId, '9000000000000000001');
    assert.equal(client.resolve({ inst: '手工机构', tr: contacts.items[1].value }).contactId, null);
    assert.equal(client.resolve({ inst: organizations.items[0].value, tr: '同名' }).contactId, null);
    await assert.rejects(client.search('CONTACT', '', '未选择机构'), /选择机构/);
});

test('editing names clears old IDs; unchanged saved quotes preserve their selected IDs', () => {
    const client = new CrmClient({});
    client.seed({ institutionId: 'i', institutionName: '甲机构', contactId: 'c', contactName: '张三' });
    assert.equal(client.resolve({ inst: '甲机构', tr: '张三', institutionId: 'i', contactId: 'c' }).contactId, 'c');
    assert.equal(client.resolve({ inst: '改名', tr: '张三', institutionId: 'i', contactId: 'c' }).contactId, null);
    client.clear();
    assert.equal(client.resolve({ inst: '甲机构', institutionId: 'i' }).institutionId, null);
});
