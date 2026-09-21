// One quote, sequential operations, no pressure load. Run only against the local simulator.
import assert from 'node:assert/strict';
import { GwClient } from '../src/gw-client.js';
import { QuoteClient, PriceClient, NotificationClient } from '../src/business-clients.js';

const transport = new GwClient('ws://127.0.0.1:18601/gateway');
const quotes = new QuoteClient(transport);
const prices = new PriceClient(transport);
const notifications = new NotificationClient(transport);
let quoteId;
const watchdog = setTimeout(() => { transport.close(); process.exitCode = 1; }, 20000);
async function waitFor(predicate) {
    for (let attempt = 0; attempt < 40; attempt++) {
        if (predicate()) return;
        await new Promise(resolve => setTimeout(resolve, 50));
    }
    throw new Error('Filtered subscription did not converge');
}
async function mutate(action, fields = {}) {
    const { quote } = await quotes.call('get', { quoteId });
    quotes.batchResult(await quotes.call(action, { quotes: [{ quoteId, expectedRevision: quote.revision, ...fields }] }));
}
try {
    await transport.connect('trader1', 'trader1');
    const { records } = await transport.request('QuoteSvr', 'ficc.base.get', { kind: 'SECURITY', ids: ['102500001.IB'] });
    const security = records[0];
    assert.equal(security.filterDataSources.liquidityGrade, 'SIMULATOR');
    const conditions = ['securityId', 'region', 'bondRating', 'liquidityGrade', 'yyRating', 'couponType', 'isSubordinated', 'couponFrequency', 'isTaxExempt']
        .map(field => ({ field, operator: 'EQ', value: security[field] }));
    conditions.push({ field: 'price', operator: 'BETWEEN', value: ['2', '3'] });
    const filter = { conditions };
    await quotes.initialize({ ownerScope: 'MINE', status: ['ACTIVE'], pageSize: 5, filter });
    await prices.initialize({ mode: 'FULL', pageSize: 5, filter });
    await notifications.initialize({ pageSize: 5, filter });
    await notifications.queryPage(1);
    console.log('PASS all three domains accept the full filter contract');
    const created = quotes.batchResult(await quotes.call('create', { quotes: [{
        clientQuoteId: 'FILTER-' + crypto.randomUUID(), securityId: security.securityId,
        side: '1', quoteType: '1', yieldType: '1', price: '2.5', quantity: '100',
        tradeDate: new Date().toLocaleDateString('sv-SE'), settlType: '2'
    }] }));
    quoteId = created.quoteId;
    await mutate('confirm');
    await waitFor(() => quotes.rows().some(row => row.quoteId === quoteId)
        && prices.rows().some(book => Object.values(book.quoteRefs).some(ref => ref.quoteId === quoteId)));
    console.log('PASS combined credit/clause/liquidity/region/range filter enters quote and price views');
    await mutate('update', { price: '3.1' });
    await waitFor(() => !quotes.rows().some(row => row.quoteId === quoteId)
        && !prices.rows().some(book => Object.values(book.quoteRefs).some(ref => ref.quoteId === quoteId)));
    console.log('PASS update outside range removes quote and price references');
    await mutate('update', { price: '2.6' });
    await waitFor(() => quotes.rows().some(row => row.quoteId === quoteId));
    await quotes.queryPage(1);
    assert.ok(quotes.rows().some(row => row.quoteId === quoteId));
    console.log('PASS re-entry and paged query use the same conditions');
    const forwardFilter = { conditions: [{ field: 'securityId', operator: 'EQ', value: security.securityId },
        { field: 'settlType', operator: 'IN', value: ['3', '4', '5', '9', '10', '11', '12', '13', '14', '15', '16'] }] };
    await quotes.open({ ownerScope: 'MINE', status: ['ACTIVE'], pageSize: 5, filter: forwardFilter });
    assert.ok(!quotes.rows().some(row => row.quoteId === quoteId));
    await mutate('update', { settlType: '9' });
    await waitFor(() => quotes.rows().some(row => row.quoteId === quoteId));
    await mutate('update', { settlType: '1' });
    await waitFor(() => !quotes.rows().some(row => row.quoteId === quoteId));
    console.log('PASS STC T+5 enters forward filter; T+0 leaves it');
} finally {
    try {
        if (quoteId) {
            const { quote } = await quotes.call('get', { quoteId });
            if (quote.status === 'ACTIVE') await mutate('refer');
            await mutate('delete');
            console.log('PASS test quote cleanup');
        }
    } finally {
        clearTimeout(watchdog);
        transport.close();
    }
}
