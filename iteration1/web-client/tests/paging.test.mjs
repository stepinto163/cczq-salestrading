import test from 'node:test';
import assert from 'node:assert/strict';
import { FilteredView } from '../src/paged-view.js';

for (const [domain, collection, idField] of [
    ['quote', 'quotes', 'quoteId'], ['price', 'books', 'securityId'],
    ['notification', 'notifications', 'notificationId']
]) {
    test(`${domain}: browsing preserves positions, counts distinct changes and resumes live`, async () => {
        const row = (id, second) => ({ [idField]: id, viewId: 'view', updateTime: `2026-09-17T10:00:${second}Z` });
        const view = new FilteredView({ request: async () => ({
            streamId: 'stream', sequence: '3',
            view: { viewId: 'view', pageNumber: 1, pageSize: 2, totalCount: 2, ids: ['c', 'a'] },
            [collection]: [row('c', '09'), row('a', '08')]
        }) }, domain, collection, idField);
        Object.assign(view, { streamId: 'stream', viewId: 'view', stale: false, pageSize: 2,
            ids: ['a', 'b'], records: new Map([['a', row('a', '02')], ['b', row('b', '01')]]) });
        view.beginBrowsing();
        const push = (sequence, rows, upsertIds, removeIds = []) => view.receive({
            streamId: 'stream', sequence: String(sequence), [collection]: rows,
            views: [{ viewId: 'view', upsertIds, removeIds, totalCount: 2 }]
        });
        push(1, [row('c', '09'), row('a', '08')], ['c', 'a'], ['b']);
        push(2, [row('a', '09')], ['a']);
        assert.deepEqual(view.ids, ['a', 'b']);
        assert.equal(view.changedIds.size, 3);
        assert.equal(view.unavailableIds.has('b'), true);
        assert.equal(view.records.has('c'), false);
        assert.equal(view.records.get('a').updateTime, row('a', '09').updateTime);
        assert.equal(view.refillTimer, null);
        await view.resumeLive();
        assert.equal(view.browsing, false);
        assert.equal(view.changedIds.size, 0);
        assert.equal(view.unavailableIds.size, 0);
        assert.deepEqual(view.ids, ['c', 'a']);
        push(4, [row('d', '10')], ['d']);
        assert.deepEqual(view.ids, ['d', 'c']);
    });
    test(`${domain}: enter/leave, local order, refill and stable later pages`, async () => {
        const row = (id, second) => ({ [idField]: id, viewId: 'view', updateTime: `2026-09-17T10:00:${second}+08:00` });
        const requests = [];
        const view = new FilteredView({ request: async (server, topic, request) => {
            requests.push({ topic, request });
            return { streamId: 'stream', sequence: '3',
                view: { viewId: 'view', pageNumber: request.pageNumber, pageSize: 2, totalCount: 3, ids: ['c', 'b'] },
                [collection]: [row('c', '04'), row('b', '02')] };
        } }, domain, collection, idField);
        Object.assign(view, { streamId: 'stream', viewId: 'view', stale: false, pageSize: 2 });
        const push = (sequence, rows, upsertIds, removeIds = []) => view.receive({ streamId: 'stream',
            sequence: String(sequence), [collection]: rows,
            views: [{ viewId: 'view', upsertIds, removeIds, totalCount: 3 }] });
        push(1, [row('a', '03'), row('b', '02')], ['a', 'b']);
        push(2, [row('c', '04')], ['c']);
        assert.deepEqual(view.ids, ['c', 'a']);
        assert.equal(view.records.has('b'), false);
        push(3, [], [], ['a']);
        assert.deepEqual(view.ids, ['c']);
        await new Promise(resolve => setTimeout(resolve, 80));
        assert.equal(requests.length, 1);
        assert.equal(requests[0].topic, `ficc.${domain}.view.query`);
        assert.equal(requests[0].request.pageNumber, 1);
        assert.deepEqual(view.ids, ['c', 'b']);
        await view.queryPage(2);
        push(4, [row('x', '09'), row('b', '08')], ['x', 'b']);
        assert.deepEqual(view.ids, ['c', 'b']);
        assert.equal(view.records.has('x'), false);
        push(5, [], [], ['c']);
        await new Promise(resolve => setTimeout(resolve, 65));
        assert.equal(requests.length, 2);
        assert.deepEqual(view.ids, ['b']);
        view.receive({ streamId: 'stream', sequence: '7' });
        assert.equal(view.stale, true);
    });
}

test('query buffers newer events and does not replay the response baseline', async () => {
    let view;
    const transport = { request: async (server, topic, request) => {
        view.receive({ streamId: 'stream', sequence: '8', quotes: [{ quoteId: 'a', revision: '2', updateTime: '2026-09-17T10:00:02Z' }],
            views: [{ viewId: request.viewId, upsertIds: ['a'], removeIds: [], totalCount: 1 }] });
        return { streamId: 'stream', sequence: '7', quotes: [{ quoteId: 'a', revision: '1', updateTime: '2026-09-17T10:00:01Z' }],
            view: { viewId: request.viewId, pageNumber: 1, pageSize: 2, ids: ['a'], totalCount: 1 } };
    } };
    view = new FilteredView(transport, 'quote', 'quotes', 'quoteId');
    view.streamId = 'stream';
    await view.open({ pageSize: 2 });
    assert.equal(view.sequence, 8n);
    assert.equal(view.rows()[0].revision, '2');
    assert.equal(view.stale, false);
});

test('buffer overflow leaves a view stale even when response includes its last sequence', async () => {
    let view;
    view = new FilteredView({ request: async (server, topic, request) => {
        for (let sequence = 1; sequence <= 201; sequence++) view.receive({ streamId: 'stream', sequence: String(sequence) });
        return { streamId: 'stream', sequence: '201', quotes: [],
            view: { viewId: request.viewId, pageNumber: 1, pageSize: 2, ids: [], totalCount: 0 } };
    } }, 'quote', 'quotes', 'quoteId');
    view.streamId = 'stream';
    await view.open({ pageSize: 2 });
    assert.equal(view.stale, true);
    assert.equal(view.buffer.length, 0);
});
