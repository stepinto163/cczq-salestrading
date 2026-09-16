// DOM/script compatibility check, not a browser visual test or performance test.
// Dependency is installed only in ignored runtime/dom-test, not in the deployed page.
import { JSDOM, VirtualConsole } from '../../runtime/dom-test/node_modules/jsdom/lib/api.js';
import { readFile, writeFile } from 'node:fs/promises';
import vm from 'node:vm';
import assert from 'node:assert/strict';
import { GwClient } from '../src/gw-client.js';
import { QuoteClient, PriceClient, NotificationClient } from '../src/business-clients.js';
import * as mapping from '../src/requirements-model.js';
import { CrmClient } from '../src/crm-client.js';

const root = new URL('../', import.meta.url);
const html = await readFile(new URL('index.html', root), 'utf8');
const sources = [...html.matchAll(/<script(?: type="module")? src="([^"]+)"/g)].map(match => match[1]);
const scripts = await Promise.all(sources.map(async source => ({ source, code: await readFile(new URL(source, root), 'utf8') })));
const errors = [];
const checks = [];
const consoleBridge = new VirtualConsole();
consoleBridge.on('jsdomError', error => {
    if (!error.message.includes('Could not parse CSS')) errors.push(error.message);
});
const dom = new JSDOM(html, { url: 'http://127.0.0.1:8088', runScripts: 'outside-only', pretendToBeVisual: true, virtualConsole: consoleBridge });
const context = dom.getInternalVMContext();
const securityRequests = [];
const quoteClients = [];
class TrackedQuoteClient extends QuoteClient {
    constructor(...args) {
        super(...args);
        quoteClients.push(this);
    }
}
class ExactSecurityGwClient extends GwClient {
    async request(server, topic, payload) {
        if (payload?.kind === 'SECURITY') {
            securityRequests.push({ topic, payload });
            if (topic === 'ficc.base.query') {
                throw new Error('1009: Bond search exceeded its time budget; use an exact securityId');
            }
            assert.equal(topic, 'ficc.base.get');
            assert.ok(payload.ids.length > 0 && payload.ids.length <= 20);
        }
        return super.request(server, topic, payload);
    }
}
Object.assign(dom.window, { fetch, Response, TextEncoder, TextDecoder, GwClient: ExactSecurityGwClient, QuoteClient: TrackedQuoteClient, PriceClient, NotificationClient, CrmClient, ...mapping });
let createdId;
let failed;
const watchdog = setTimeout(() => {
    dom.window.doLogout?.();
    dom.window.close();
    process.exitCode = 1;
}, 20000);

function check(name, result) {
    assert.ok(result, name);
    checks.push(name);
    console.log(`PASS ${name}`);
}
async function waitFor(name, predicate) {
    const end = Date.now() + 4000;
    while (!predicate() && Date.now() < end) await new Promise(resolve => setTimeout(resolve, 40));
    check(name, predicate());
}
const data = () => vm.runInContext('D', context);

try {
    for (const script of scripts) {
        const code = script.source.includes('requirements-adapter') ? `(() => {\n${script.code.replace(/^import .*;\r?\n/gm, '')}\n})();` : script.code;
        vm.runInContext(code, context, { filename: script.source, timeout: 4000 });
    }
    dom.window.document.getElementById('loginUser').value = 'trader1';
    dom.window.document.getElementById('loginPass').value = 'trader1';
    await dom.window.doLogin();
    check('original login uses GW', dom.window.document.getElementById('app').style.display === 'flex');
    check('login does not depend on a broad bond search', !securityRequests.some(request => request.topic === 'ficc.base.query'));
    check('original quote tables and right panel exist', Boolean(dom.window.document.getElementById('quoteTableWrap') && dom.window.document.getElementById('instQuoteBody')));
    const previous = new Set(data().quoteConfirm.map(row => row.recognitionId));
    dom.window.document.getElementById('preciseInput').value = 'Bid 0.0001 250001.IB 1 T+0';
    await dom.window.doPreciseIdentify();
    check('entered bond is resolved by exact ID', securityRequests.some(request =>
        request.topic === 'ficc.base.get' && request.payload.ids.includes('250001.IB')));
    check('original precise table renders the draft', data().preciseData.length === 1 && dom.window.document.getElementById('preciseTableWrap').textContent.includes('示例国债'));
    await dom.window.doConfirmPrecise();
    await waitFor('original confirmation table receives pending quote', () => data().quoteConfirm.some(row => !previous.has(row.recognitionId)));
    createdId = data().quoteConfirm.find(row => !previous.has(row.recognitionId)).recognitionId;
    vm.runInContext(`quoteSelected = new Set([${JSON.stringify(createdId)}])`, context);
    if (process.env.CRM_LOCAL_TEST === '1') {
        const pending = data().quoteConfirm.find(row => row.recognitionId === createdId);
        const inputs = dom.window.document.querySelector(`[data-quote-index="${pending._idx}"]`);
        const institutionInput = inputs.querySelector('[data-quote-field="inst"]');
        const contactInput = inputs.querySelector('[data-quote-field="tr"]');
        // JSDOM outside-only does not compile inline HTML handlers; bind their existing functions explicitly.
        institutionInput.addEventListener('input', () => dom.window.setQuoteConfirmRowField(pending, 'inst', institutionInput.value, institutionInput));
        contactInput.addEventListener('input', () => dom.window.setQuoteConfirmRowField(pending, 'tr', contactInput.value, contactInput));
        institutionInput.value = '测试乙证券';
        dom.window.prepareCrmInstitutionSearch(institutionInput);
        await waitFor('CRM institution popup receives PostgreSQL records through GW', () =>
            vm.runInContext('crmSuggestionState.items.some(item => item.value.includes("9000000000000000002"))', context));
        const institutionIndex = vm.runInContext('crmSuggestionState.items.findIndex(item => item.value.includes("9000000000000000002"))', context);
        dom.window.selectCrmSuggestion(institutionIndex);
        contactInput.value = '测试张明';
        dom.window.prepareCrmTraderSearch(contactInput, institutionInput.value);
        await waitFor('CRM contact popup keeps duplicate names as separate choices', () =>
            vm.runInContext('crmSuggestionState.items.length === 2 && crmSuggestionState.kind === "trader"', context));
        const contactIndex = vm.runInContext('crmSuggestionState.items.findIndex(item => item.value.includes("c9000000000000000002"))', context);
        dom.window.selectCrmSuggestion(contactIndex);
    }
    await dom.window.doConfirmQuotes();
    await waitFor('original confirm button activates quote', () => data().liveQuotes.some(row => row.quoteId === createdId && row.status === 'active'));
    if (process.env.CRM_LOCAL_TEST === '1') {
        const quote = data().liveQuotes.find(row => row.quoteId === createdId);
        check('web confirmation persists CRM IDs and canonical names', quote.institutionId === '9000000000000000002'
            && quote.contactId === 'c9000000000000000002' && quote.inst === '测试乙证券股份有限公司');
    }
    await waitFor('original market table receives price entry', () => data().boardRateAll.some(row => row.quoteId === createdId));
    const row = data().boardRateAll.find(row => row.quoteId === createdId);
    dom.window.setRecommendationTarget(row, null, 'boardRateAllBody');
    check('original right panel renders price-book yield', dom.window.document.getElementById('instQuoteBody').textContent.includes('0.0001'));
    await waitFor('original notification area receives server reminder', () => data().boardAlert.some(row => row.quoteId === createdId));

    const readQuote = async () => (await quoteClients[0].call('get', { quoteId: createdId })).quote;
    const setContext = () => vm.runInContext(`ctxTarget = { quoteId: ${JSON.stringify(createdId)}, type: 'rate', idx: D.boardRateAll.findIndex(row => row.quoteId === ${JSON.stringify(createdId)}) }`, context);

    // Reproduce a displayed price row whose quote has fallen outside the loaded QuoteView window.
    quoteClients[0].records.delete(createdId);
    quoteClients[0].ids = quoteClients[0].ids.filter(id => id !== createdId);
    quoteClients[0].onChange();
    await waitFor('price row remains actionable outside the quote cache', () =>
        !data().liveQuotes.some(row => row.quoteId === createdId) && data().boardRateAll.some(row => row.quoteId === createdId));
    vm.runInContext(`boardSelectedQuoteIds.clear(); boardSelectedQuoteIds.add(${JSON.stringify(createdId)})`, context);
    await dom.window.referSelectedBoardQuotes('board-rate-all');
    check('market toolbar Refer uses DPS outside loaded quote cache', (await readQuote()).status === 'REFERRED');
    await dom.window.restoreManagedQuotes([createdId]);
    await waitFor('Unrefer restores the same quote into the market', () => data().boardRateAll.some(row => row.quoteId === createdId));

    setContext();
    await dom.window.ctxOCO();
    check('context OCO update uses DPS', (await readQuote()).oco === true);
    setContext();
    await dom.window.ctxPack();
    check('context packing clears OCO atomically', (await readQuote()).packing === true && (await readQuote()).oco === false);

    const rowIndex = data().boardRateAll.findIndex(row => row.quoteId === createdId);
    await dom.window.openQuoteForm('edit', rowIndex, 'rate', createdId);
    check('edit modal fetches full quote and preserves packing', vm.runInContext('quoteModalState.sourceRow.packing', context) === true);
    dom.window.document.getElementById('quotePrice').value = '0.0002';
    dom.window.document.getElementById('quoteNote').value = 'web action regression';
    await dom.window.saveEditQuote();
    const edited = await readQuote();
    check('edit saves price, flags and note through DPS', edited.price === '0.0002' && edited.packing === true && edited.remark === 'web action regression');

    await dom.window.openQuoteForm('edit', data().boardRateAll.findIndex(row => row.quoteId === createdId), 'rate', createdId);
    const concurrent = quoteClients[0].batchResult(await quoteClients[0].call('update', {
        quotes: [{ quoteId: createdId, expectedRevision: edited.revision, remark: 'concurrent edit' }]
    }));
    dom.window.document.getElementById('quotePrice').value = '0.0003';
    await dom.window.saveEditQuote();
    const afterConflict = await readQuote();
    check('stale edit is rejected without overwriting another update', afterConflict.revision === concurrent.revision
        && afterConflict.price === '0.0002' && afterConflict.remark === 'concurrent edit');
    check('conflicting edit remains open and displays server error', vm.runInContext('quoteModalState !== null', context)
        && dom.window.document.getElementById('toastContainer').textContent.includes('1004'));
    dom.window.closeModal();

    await waitFor('edited quote returns to market', () => data().boardRateAll.some(row => row.quoteId === createdId));
    setContext();
    dom.window.ctxRefer();
    await dom.window.confirmRefer();
    check('context Refer confirmation uses DPS', (await readQuote()).status === 'REFERRED');

    const historyDate = edited.updateTime.slice(0, 10);
    dom.window.document.getElementById('historyStartDate').value = historyDate;
    dom.window.document.getElementById('historyEndDate').value = historyDate;
    await dom.window.document.getElementById('historyQueryBtn').onclick();
    check('history query returns persisted quote with matching calendar date', data().mgmtHistoryFiltered.some(row => row.quoteId === createdId && row.status === 'revoked'));
    const actualCall = quoteClients[0].call.bind(quoteClients[0]);
    quoteClients[0].call = (suffix, body) => suffix === 'history.search'
        ? Promise.reject(new Error('history unavailable for test')) : actualCall(suffix, body);
    await dom.window.document.getElementById('historyQueryBtn').onclick();
    check('history failure displays an error instead of silently showing no results',
        dom.window.document.getElementById('mgmtHistoryStatusbar').textContent.includes('history unavailable for test'));
    quoteClients[0].call = actualCall;
    await dom.window.document.getElementById('historyQueryBtn').onclick();
    check('history can be queried again after failure', data().mgmtHistoryFiltered.some(row => row.quoteId === createdId));
    const history = data().mgmtHistory.find(row => row.quoteId === createdId);
    vm.runInContext(`mgmtHistorySelected = new Set([${JSON.stringify(history.eventId)}])`, context);
    await dom.window.unreferHistory();
    check('history Unrefer uses current revision and preserves quote ID', (await readQuote()).status === 'ACTIVE');
    vm.runInContext(`mgmtTodaySelected = new Set([${JSON.stringify(createdId)}])`, context);
    await dom.window.referFuzzy();
    check('management Refer awaits the asynchronous action', (await readQuote()).status === 'REFERRED');
    check('management action does not report a false synchronous failure', !dom.window.document.getElementById('toastContainer').textContent.includes('没有可操作的报价'));
    setContext();
    await dom.window.ctxLog();
    check('quote log is loaded from persisted history', dom.window.document.getElementById('modalBody').textContent.includes('0.0002'));
    check('original scripts and adapter have no uncaught DOM errors', errors.length === 0);
} catch (error) {
    failed = error.stack;
    process.exitCode = 1;
    console.error(error);
    console.error('DOM errors:', errors);
    console.error('Login error:', dom.window.document.getElementById('loginError')?.textContent);
    console.error('Toasts:', dom.window.document.getElementById('toastContainer')?.textContent);
} finally {
    try {
        if (createdId) {
            const current = (await quoteClients[0].call('get', { quoteId: createdId })).quote;
            if (current.status === 'ACTIVE') {
                await dom.window.setQuotesStatus([createdId], 'revoked');
                check('cleanup refer', (await quoteClients[0].call('get', { quoteId: createdId })).quote.status === 'REFERRED');
            }
            await dom.window.deleteManagedQuotes([createdId]);
            check('cleanup delete', (await quoteClients[0].call('get', { quoteId: createdId })).quote.status === 'DELETED');
        }
    } catch (error) {
        failed = `${failed || ''}\nCleanup: ${error.message}`;
        process.exitCode = 1;
    }
    clearTimeout(watchdog);
    await writeFile(new URL('../../runtime/requirements-dom-report.json', import.meta.url), JSON.stringify({ time: new Date().toISOString(), checks, createdId, errors, failed, passed: !process.exitCode }, null, 2));
    dom.window.doLogout?.();
    dom.window.close();
}
