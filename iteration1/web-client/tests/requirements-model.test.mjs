import test from 'node:test';
import assert from 'node:assert/strict';
import { readFile } from 'node:fs/promises';
import { createHash } from 'node:crypto';
import { displayQuote, pendingQuote, priceRows, bestRow, quoteInput, historyRow } from '../src/requirements-model.js';

test('copied original scripts and styles retain their source hashes', async () => {
    const root = new URL('../requirements/', import.meta.url);
    const manifest = JSON.parse(await readFile(new URL('source-manifest.json', root), 'utf8'));
    for (const [file, hash] of Object.entries(manifest.files)) {
        assert.equal(createHash('sha256').update(await readFile(new URL(file, root))).digest('hex'), hash, file);
    }
});

test('quote and pending columns retain ownership, revision, source and entered names', () => {
    const quote = { quoteId: 'q', ownerUserId: 'u', revision: '2', side: '1', securityId: '250001.IB', status: 'PENDING_CONFIRM',
        price: '2.5000', quantity: '1', yieldType: '1', settlType: '1', source: { type: 'QT' }, institutionName: '机构甲', contactName: '张三' };
    const security = { securityName: '国债', market: 'RATE' };
    const row = pendingQuote(quote, security, 'u');
    assert.equal(row.recognitionId, 'q');
    assert.equal(row.version, '2');
    assert.equal(row.src, 'QT');
    assert.equal(row.tr, '张三');
    assert.equal(row.canManage, true);
    assert.equal(displayQuote(quote, security, 'another').canManage, false);
});

test('price display uses independent book entries and preserves server order', () => {
    const rows = priceRows({ securityId: '250001.IB', bookRevision: '5', marketData: { NoMDEntries: [
        { MDEntryID: 'FICC:q1', MDEntryType: '0', MDEntryPx: '2.5000', MDEntrySize: '1', UserID: 'u' },
        { MDEntryID: 'FICC:q2', MDEntryType: '0', MDEntryPx: '2.4000', MDEntrySize: '2', UserID: 'u' }
    ] } }, { market: 'RATE' }, 'u');
    assert.equal(bestRow(rows).bidQuoteId, 'q1');
    assert.equal(bestRow(rows).bp, '2.5000');
    assert.equal(rows[1].quoteId, 'q2');
});

test('editable original columns map to QuoteSvr fields with strict yield precision', () => {
    const draft = { c: '250001.IB', d: 'Bid', p: '2.5000', vol: '1', date: '2026-09-14', pt: '到期收益率', cs: 'T+1', inst: '机构甲' };
    const input = quoteInput(draft);
    assert.equal(input.side, '1');
    assert.equal(input.settlType, '2');
    assert.equal(input.institutionName, '机构甲');
    assert.equal(input.contactName, null);
    assert.throws(() => quoteInput({ ...draft, p: '2.50001' }), /四位/);
    assert.throws(() => quoteInput({ ...draft, pt: '净价' }), /尚未接入/);
});

test('price actions use the referenced quote revision, never the book revision', () => {
    const rows = priceRows({ securityId: 'B', bookRevision: '800',
        quoteRefs: { E1: { quoteId: 'Q1', quoteRevision: '3' }, E2: { quoteId: 'Q2', quoteRevision: '7' } },
        marketData: { NoMDEntries: [
            { MDEntryID: 'E1', MDEntryType: '0', MDEntryPx: '2.5', UserID: 'u' },
            { MDEntryID: 'E2', MDEntryType: '1', MDEntryPx: '2.4', UserID: 'v' }
        ] }
    }, {}, 'u');
    assert.equal(rows[0].version, '3');
    assert.equal(rows[1].version, '7');
    assert.equal(rows[1].canManage, false);
    const best = bestRow(rows);
    assert.equal(best.bidQuoteId, 'Q1');
    assert.equal(best.ofrQuoteId, 'Q2');
    assert.equal(best.sourceQuoteId, '');
});

test('historical daily rows retain distinct dates, status, counterparty and source', () => {
    const quote = { quoteId: 'Q1', side: '2', ownerUserId: 'u', status: 'REFERRED', quoteType: '2',
        revision: '9', price: '101.2', quantity: '50', updateTime: '2026-09-15T10:20:30+08:00',
        source: { type: 'QT' }, contactName: '张三', institutionName: '机构甲', remark: '保留备注' };
    const row = historyRow({ tradeDate: '2026-09-15', quote, reference: { securityName: '债券甲' } }, 'u');
    assert.equal(row.eventId, '2026-09-15:Q1');
    assert.equal(row.t.split(' ')[0], '2026-09-15');
    assert.equal(row.status, 'revoked');
    assert.equal(row.mode, 'QT');
    assert.equal(row.trader, '张三');
    assert.equal(row.priceType, '净价');
    assert.equal(row.note, '保留备注');
});
