import assert from 'node:assert/strict';
import { mkdir, writeFile } from 'node:fs/promises';
import { performance } from 'node:perf_hooks';
import { GwClient } from '../src/gw-client.js';
import { QuoteClient, PriceClient, NotificationClient } from '../src/business-clients.js';

const output = new URL(`../../runtime/web-functional-${Date.now()}/`, import.meta.url);
await mkdir(output, { recursive: true });
const report = { startTime: new Date().toISOString(), type: 'functional (not a latency benchmark)', quoteLimit: 2, checks: [], createdQuoteIds: [], cleanup: [] };
const clients = [];
let preference;
const timeout = setTimeout(() => {
    console.error('Functional test exceeded 60 seconds. External runner will terminate it.');
    process.exitCode = 1;
    for (const client of clients) client.transport.close();
}, 60000);

function check(name, predicate) {
    assert.ok(predicate, name);
    report.checks.push({ name, passed: true });
    console.log(`PASS ${name}`);
}

async function waitFor(name, predicate) {
    const start = performance.now();
    while (!predicate() && performance.now() - start < 6000) await new Promise(resolve => setTimeout(resolve, 40));
    check(name, predicate());
}

async function login(user) {
    const transport = new GwClient('ws://127.0.0.1:18601/gateway');
    const client = { transport, quotes: new QuoteClient(transport), prices: new PriceClient(transport), notifications: new NotificationClient(transport) };
    clients.push(client);
    await transport.connect(user, 'trader1');
    await client.quotes.initialize({ ownerScope: 'MINE', status: ['PENDING_CONFIRM', 'ACTIVE', 'REFERRED'], pageSize: 50 });
    await client.prices.initialize({ mode: 'FULL', pageSize: 20 });
    await client.notifications.initialize({ pageSize: 50 });
    check(`${user}: authenticated and opened separate domain views`, !client.quotes.stale && !client.prices.stale && !client.notifications.stale);
    return client;
}

const containsPrice = (client, id) => client.prices.rows().some(book => book.marketData.NoMDEntries.some(entry => entry.MDEntryID === `FICC:${id}`));
const containsAlert = (client, id) => client.notifications.rows().some(row => row.quoteIds.includes(id) && row.reasons.some(reason => reason.type === 'MY_BEST'));

try {
    const first = await login('trader1');
    const second = await login('trader2');
    preference = await first.notifications.preferences();
    await first.notifications.setMyBest(true);
    const otherSessionTopic = `ficc.quote.report.${first.transport.sessionId}`;
    if (process.argv.includes('--standard-gw')) {
        let rejected = false;
        try {
            await second.transport.subscribe(otherSessionTopic, () => {});
        } catch {
            rejected = true;
        }
        if (rejected) {
            check('standard GW rejects another session topic', true);
        } else {
            const reply = await second.transport.exchange(3,
                { serverName: 'gateway', method: 'unsubscribe', content: otherSessionTopic }, otherSessionTopic);
            assert.equal(reply?.Code, '0000', 'temporary capability probe unsubscribed');
            second.transport.listeners.delete(otherSessionTopic);
            report.securityLimitations = ['Standard GW accepts subscriptions to another known session topic; session-topic ownership enforcement is not provided by this configuration.'];
            console.log('LIMITATION ' + report.securityLimitations[0]);
        }
    } else {
        await assert.rejects(second.transport.subscribe(otherSessionTopic, () => {}));
        check('another user cannot subscribe to the first user private topic', true);
    }

    for (const [client, securityId] of [[first, '250001.IB'], [second, '250002.IB']]) {
        const result = await client.quotes.create({ securityId, side: '1', quoteType: '1', yieldType: '1', price: '0.0001', quantity: '1', settlType: '1',
            tradeDate: new Intl.DateTimeFormat('sv-SE', { timeZone: 'Asia/Shanghai' }).format(new Date()) });
        report.createdQuoteIds.push({ quoteId: result.quoteId, user: client === first ? 'trader1' : 'trader2' });
        check('server quote ID uses STC Snowflake format as a string',
            typeof result.quoteId === 'string' && /^Q-[0-9]+$/.test(result.quoteId));
        await waitFor('new pending quote arrives by push', () => client.quotes.records.get(result.quoteId)?.status === 'PENDING_CONFIRM');
        check('pending quote is absent from market book', !containsPrice(first, result.quoteId) && !containsPrice(second, result.quoteId));
        await client.quotes.action('confirm', client.quotes.records.get(result.quoteId));
        await waitFor('confirmation arrives by quote event', () => client.quotes.records.get(result.quoteId)?.status === 'ACTIVE');
        await waitFor('both users receive the confirmed market entry', () => containsPrice(first, result.quoteId) && containsPrice(second, result.quoteId));
        const other = client === first ? second : first;
        check('private quote is absent from the other user quote view', !other.quotes.records.has(result.quoteId));
    }

    const firstId = report.createdQuoteIds[0].quoteId;
    await waitFor('best quote notification arrives independently', () => containsAlert(first, firstId));
    const notification = first.notifications.rows().find(row => row.quoteIds.includes(firstId) && row.reasons.some(reason => reason.type === 'MY_BEST'));
    await first.notifications.markRead(notification.notificationId);
    await waitFor('read status arrives by notification push', () => Boolean(first.notifications.records.get(notification.notificationId)?.readTime));
    check('market shows multiple securities', first.prices.ids.includes('250001.IB') && first.prices.ids.includes('250002.IB'));
    await first.prices.open({ mode: 'FULL', pageSize: 20, filter: { conditions: [{ field: 'securityId', operator: 'EQ', value: '250001.IB' }] } });
    check('server filter selects one security', first.prices.ids.length === 1 && first.prices.ids[0] === '250001.IB');
    await first.prices.open({ mode: 'FULL', pageSize: 20 });
    check('clearing filter restores multiple securities', first.prices.ids.includes('250002.IB'));
    await first.quotes.action('refer', first.quotes.records.get(firstId));
    await waitFor('refer removes market entry for both users', () => !containsPrice(first, firstId) && !containsPrice(second, firstId) && first.quotes.records.get(firstId)?.status === 'REFERRED');
    await first.quotes.action('unrefer', first.quotes.records.get(firstId));
    await waitFor('unrefer restores same quote ID', () => containsPrice(first, firstId) && first.quotes.records.get(firstId)?.status === 'ACTIVE');
    for (const view of [first.quotes, first.prices, first.notifications]) {
        const originalQuery = { ...view.query };
        await view.open({ ...originalQuery, pageSize: 1 });
        const firstIds = [...view.ids];
        const total = view.totalCount;
        await view.queryPage(2);
        check(`${view.domain}: second page returns one batch, not a prefix`, view.pageNumber === 2
            && view.ids.length === Math.min(1, Math.max(0, total - 1))
            && !view.ids.some(id => firstIds.includes(id)));
        await view.queryPage(2147483647);
        check(`${view.domain}: out-of-range page is empty without integer overflow`, view.ids.length === 0 && view.totalCount === total);
        await assert.rejects(view.call('view.query', { viewId: view.viewId, pageNumber: 0, pageSize: 1 }), /1001/);
        await assert.rejects(view.call('view.query', { viewId: view.viewId, pageNumber: 1, pageSize: 1001 }), /1008/);
        await view.open(originalQuery);
    }
    const urgentFilter = { conditions: [{ field: 'urgent', operator: 'EQ', value: true }] };
    await first.quotes.open({ ownerScope: 'MINE', status: ['ACTIVE'], pageSize: 2, filter: urgentFilter });
    await first.prices.open({ mode: 'BEST', pageSize: 2, filter: urgentFilter });
    for (const urgent of [true, false]) {
        const quote = (await first.quotes.call('get', { quoteId: firstId })).quote;
        const result = await first.quotes.call('update', { quotes: [{ quoteId: firstId, expectedRevision: quote.revision, urgent }] });
        check(`update urgent=${urgent} succeeds`, result.results[0].Code === '0000');
        await waitFor(`quote enters/leaves matching scope: ${urgent}`, () => first.quotes.ids.includes(firstId) === urgent);
        await waitFor(`price enters/leaves matching scope: ${urgent}`, () => containsPrice(first, firstId) === urgent);
    }
    await first.quotes.open({ ownerScope: 'MINE', status: ['PENDING_CONFIRM', 'ACTIVE', 'REFERRED'], pageSize: 50 });
    await first.prices.open({ mode: 'FULL', pageSize: 20 });
    check('all six views remain synchronized', clients.every(client => [client.quotes, client.prices, client.notifications].every(view => !view.stale)));
    report.passed = true;
} catch (error) {
    report.passed = false;
    report.error = error.stack;
    process.exitCode = 1;
    console.error(error);
} finally {
    for (const item of report.createdQuoteIds) {
        const client = clients[item.user === 'trader1' ? 0 : 1];
        try {
            await client.quotes.open({ ownerScope: 'MINE', status: ['PENDING_CONFIRM', 'ACTIVE', 'REFERRED'], pageSize: 50 });
            let quote = client.quotes.records.get(item.quoteId);
            if (quote?.status === 'ACTIVE') {
                await client.quotes.action('refer', quote);
                await waitFor('cleanup refer event', () => client.quotes.records.get(item.quoteId)?.status === 'REFERRED');
                quote = client.quotes.records.get(item.quoteId);
            }
            if (quote) await client.quotes.action('delete', quote);
            await waitFor('cleanup delete event', () => !client.quotes.records.has(item.quoteId));
            report.cleanup.push({ quoteId: item.quoteId, deleted: true });
        } catch (error) {
            report.cleanup.push({ quoteId: item.quoteId, error: error.message });
            process.exitCode = 1;
        }
    }
    if (preference) {
        try { await clients[0].notifications.setMyBest(preference.myBestEnabled); }
        catch (error) { report.cleanup.push({ preferenceRestoreError: error.message }); process.exitCode = 1; }
    }
    for (const [index, client] of clients.entries()) {
        client.transport.close();
        await writeFile(new URL(`client-${index + 1}.jsonl`, output), client.transport.logs.map(record => JSON.stringify(record)).join('\n'));
    }
    clearTimeout(timeout);
    report.endTime = new Date().toISOString();
    report.passed = report.passed && !process.exitCode;
    await writeFile(new URL('report.json', output), JSON.stringify(report, null, 2));
    console.log(`Report: ${output.pathname}`);
}
