import { GwClient } from './gw-client.js';
import { QuoteClient, PriceClient, NotificationClient } from './business-clients.js';
import { displayQuote, pendingQuote, priceRows, bestRow, quoteInput } from './requirements-model.js';

// These bindings are the original app.js integration points, shared with its inline event handlers.
let quoteView;
let priceView;
let notificationView;
let securities = new Map();
let pendingRender = false;
let operationBusy = false;
let signedIn = false;
const gateway = new GwClient('ws://127.0.0.1:18601/gateway', {
    onDisconnect: () => doLogout(true)
});
const element = id => document.getElementById(id);
const userId = () => currentAccountId();
const reasonNames = { MY_BEST: '我的最优', WATCHLIST: '关注债券', FILTER_GROUP: '筛选分组', COUNTER_QUOTE: '对价提醒' };

function scheduleUpdate() {
    if (!signedIn || pendingRender) return;
    pendingRender = true;
    requestAnimationFrame(() => {
        pendingRender = false;
        if (signedIn) refreshDisplay();
    });
}

function refreshDisplay() {
    const previousAlerts = allAlertKeys();
    const previousPending = new Set(D.quoteConfirm.map(row => row.recognitionId));
    D.liveQuotes = quoteView.rows().map(quote => displayQuote(quote, securities.get(quote.securityId), userId()));
    D.quoteConfirm = quoteView.rows().filter(quote => quote.status === 'PENDING_CONFIRM').map(quote =>
        mergePendingQuoteDraft(pendingQuote(quote, securities.get(quote.securityId), userId())));
    D.quoteConfirm.forEach((row, index) => {
        row._idx = index;
        if (!previousPending.has(row.recognitionId)) quoteSelected.add(row.recognitionId);
    });
    const pendingIds = new Set(D.quoteConfirm.map(row => row.recognitionId));
    quoteSelected = new Set([...quoteSelected].filter(id => pendingIds.has(id)));
    D.quoteConfirmStats = {
        batchCount: D.quoteConfirm.length, quoteCount: D.quoteConfirm.length,
        bidCount: D.quoteConfirm.filter(row => row.d === 'Bid').length,
        ofrCount: D.quoteConfirm.filter(row => row.d === 'Ofr').length
    };
    D.quoteStateLoaded = true;
    rebuildBoardProjections();
    syncMgmtToday();
    renderAll();
    renderInstQuoteRows('all');
    updatePendingQuoteStats();
    updateBadges();
    notifyNewAlerts(previousAlerts);
    const stale = [quoteView, priceView, notificationView].some(view => view.stale);
    element('pendingSyncState').textContent = stale ? '同步中断，请点击行情刷新或重新登录' : 'GW 实时同步正常';
}

rebuildBoardProjections = function () {
    if (!priceView) return;
    const all = [];
    const best = [];
    for (const book of priceView.rows()) {
        const rows = priceRows(book, securities.get(book.securityId), userId());
        all.push(...rows);
        const row = bestRow(rows);
        if (row) best.push(row);
    }
    for (const [market, label] of [['credit', 'Credit'], ['rate', 'Rate']]) {
        D[`board${label}All`] = applyBoardToolbarFilters(all.filter(row => row.market === market), `board-${market}-all`);
        D[`board${label}Best`] = applyBoardToolbarFilters(best.filter(row => row.market === market), `board-${market}-best`);
    }
    const byId = new Map(all.map(row => [row.quoteId, row]));
    D.boardAlert = notificationView.rows().flatMap(notification => notification.reasons.map(reason => ({
        ...(byId.get(notification.quoteIds[0]) || { c: notification.securityId, n: securities.get(notification.securityId)?.securityName }),
        alertId: notification.notificationId, alertKeys: [notification.notificationId],
        alertType: reasonNames[reason.type] || reason.type, updatedAt: notification.updateTime,
        dataVersion: notification.contentToken, groupNames: [], readTime: notification.readTime
    })));
    readAlertKeys = new Set(notificationView.rows().filter(row => row.readTime).map(row => row.notificationId));
    alertReadLoaded = true;
};

async function operate(action, allowStale = false) {
    if (operationBusy) return;
    if (!signedIn || (quoteView.stale && !allowStale)) {
        showToast('error', '请先登录并完成同步');
        return;
    }
    operationBusy = true;
    try {
        await action();
    } catch (error) {
        showToast('error', error.message);
    } finally {
        operationBusy = false;
    }
}

async function command(action, quoteId, fields = {}) {
    const quote = quoteView.records.get(quoteId);
    if (!quote) throw new Error('该报价不在本人已加载报价中，请刷新后重试');
    const reply = await quoteView.call(action, { quotes: [{ quoteId, expectedRevision: quote.revision, ...fields }] });
    return quoteView.batchResult(reply);
}

async function changeQuotes(ids, action) {
    const unique = [...new Set(ids)];
    if (!unique.length) throw new Error('请先选择报价');
    if (unique.length > 5) throw new Error('首轮联调每批最多 5 条');
    for (const id of unique) await command(action, id);
    showToast('success', `已处理 ${unique.length} 条，状态由服务端事件更新`);
}

doLogin = async function () {
    const button = element('loginBtn');
    button.disabled = true;
    element('loginError').textContent = '';
    try {
        resetAccountState();
        const username = element('loginUser').value.trim();
        const login = await gateway.connect(username, element('loginPass').value);
        const base = await gateway.request('QuoteSvr', 'ficc.base.query', { kind: 'SECURITY' });
        securities = new Map(base.records.map(row => [row.securityId, row]));
        const callbacks = { onChange: scheduleUpdate, onStale: message => showToast('error', message) };
        quoteView = new QuoteClient(gateway, callbacks);
        priceView = new PriceClient(gateway, callbacks);
        notificationView = new NotificationClient(gateway, callbacks);
        await quoteView.initialize({ ownerScope: 'MINE', status: ['PENDING_CONFIRM', 'ACTIVE', 'REFERRED'], limit: 50 });
        await priceView.initialize({ mode: 'FULL', limit: 20 });
        await notificationView.initialize({ limit: 50 });
        const preference = await notificationView.preferences();
        D.alertSettings = { favorite: preference.watchlistEnabled, filterGroup: preference.filterGroupEnabled, priceMatch: preference.counterQuoteEnabled, bestQuote: preference.myBestEnabled };
        authToken = 'GW-SESSION'; // Legacy UI login predicate; actual session stays inside GwClient.
        signedIn = true;
        enterApplication({ username, accountId: login.userId || login.userid, name: username, role: 'trader' });
        element('loginPass').value = '';
        D.marketGroupReadOnly = true;
        refreshDisplay();
        showToast('success', '已连接 GW；原界面联调版，每批最多 5 条');
    } catch (error) {
        signedIn = false;
        gateway.close();
        authToken = '';
        currentUser = null;
        element('loginError').textContent = error.message;
    } finally {
        button.disabled = false;
    }
};
element('loginBtn').onclick = doLogin;

doLogout = function (expired) {
    signedIn = false;
    gateway.close();
    resetAccountState();
    authToken = '';
    currentUser = null;
    element('app').style.display = 'none';
    element('loginOverlay').classList.remove('hidden');
    element('loginPass').value = '';
    element('loginError').textContent = expired ? 'GW 已断开，请重新登录' : '';
};

fillPreciseExample = function () {
    element('preciseInput').value = 'Bid 2.5000 250001.IB 1000 T+0';
};
doPreciseIdentify = function () {
    try {
        const lines = element('preciseInput').value.trim().split(/\r?\n/).filter(Boolean);
        if (!lines.length || lines.length > 5) throw new Error('首轮请输入 1 至 5 行固定格式：Bid 2.5000 250001.IB 1000 T+0');
        const drafts = lines.map(line => {
            if (!/^(Bid|Ofr)\s+\d+(\.\d{1,4})?\s+\d+\.(IB|SH|SZ)\s+\d+(\.\d+)?\s+T\+[01]$/i.test(line)) {
                throw new Error('识别服务尚未接入；首轮仅支持“示例填充”的固定格式');
            }
            const row = parseQuoteLine(line);
            row.vol = line.split(/\s+/)[3];
            const security = securities.get(row.c);
            if (!security) throw new Error(`基础资料未找到 ${row.c}`);
            return { ...row, n: security.securityName, d: row.d.toLowerCase() === 'bid' ? 'Bid' : 'Ofr',
                m: security.remainingYears, date: quoteToday(), pt: security.hasExerciseOption ? '行权收益率' : '到期收益率',
                recognitionId: crypto.randomUUID(), quoteAction: 'NEW', quoteType: '新增', inst: '', tr: '' };
        });
        D.preciseData = drafts;
        preciseSelected = new Set(drafts.map((row, index) => index));
        renderPreciseTable();
        updatePreciseSelectedCount();
        showToast('info', '固定格式已填入原报价表，可编辑后提交；自然语言识别尚未接入');
    } catch (error) {
        showToast('error', error.message);
    }
};

doConfirmPrecise = () => operate(async () => {
    commitRecognitionTableControls('preciseTableWrap');
    const selected = [...preciseSelected].map(index => D.preciseData[index]).filter(Boolean);
    if (!selected.length || selected.length > 5) throw new Error('请选择 1 至 5 条报价');
    const inputs = selected.map(quoteInput);
    try {
        for (let index = 0; index < inputs.length; index++) {
            await quoteView.create(inputs[index]);
            D.preciseData = D.preciseData.filter(row => row !== selected[index]);
        }
    } finally {
        D.preciseData.forEach((row, index) => row._idx = index);
        preciseSelected = new Set(D.preciseData.map((row, index) => index));
        renderPreciseTable();
    }
    showToast('success', '已新建待确认报价，请到“报价确认”完成确认');
});
doConfirmQuotes = () => operate(async () => {
    commitQuoteConfirmTableControls();
    const rows = selectedQuoteRows();
    if (!rows.length || rows.length > 5) throw new Error('请选择 1 至 5 条报价');
    for (const row of rows) {
        await command('confirm', row.recognitionId, quoteInput(row));
        pendingQuoteDrafts.delete(row.recognitionId);
    }
    showToast('success', '确认成功；行情及提醒由服务端推送');
});
doDeleteQuotes = () => operate(() => changeQuotes(selectedQuoteRows().map(row => row.recognitionId), 'delete'));
setQuotesStatus = (ids, status) => operate(() => changeQuotes(ids, status === 'active' ? 'unrefer' : 'refer'));
restoreManagedQuotes = ids => operate(() => changeQuotes(ids, 'unrefer'));
deleteManagedQuotes = ids => operate(() => changeQuotes(ids, 'delete'));

saveEditQuote = () => operate(async () => {
    if (!quoteModalState || !validateQuoteForm()) return;
    const state = quoteModalState;
    const input = quoteInput({
        c: state.sourceRow.c, d: state.direction, p: quoteFieldValue('quotePrice'),
        vol: quoteFieldValue('quoteVolume'), pt: quoteFieldValue('quotePriceType'),
        cs: quoteFieldValue('quoteSettlement'), date: quoteFieldValue('quoteTradeDate'),
        inst: quoteFieldValue('quoteInstitution'), tr: quoteFieldValue('quoteTrader'),
        urgent: element('quoteUrgent').checked, recommend: element('quoteRecommend').checked,
        round: element('quoteRound').checked, oco: state.sourceRow.oco, packing: state.sourceRow.packing
    });
    input.remark = quoteFieldValue('quoteNote');
    if (state.mode === 'edit') await command('update', state.quoteId, input);
    else await quoteView.create(input);
    closeModal();
    quoteModalState = null;
    showToast('success', state.mode === 'edit' ? '修改已提交' : '已新建待确认报价，请到报价确认完成确认');
});

loadQuoteState = async function () {
    if (!signedIn) return false;
    await quoteView.open();
    return true;
};
loadMgmtToday = loadQuoteState;
loadQuoteConfirmBatches = loadQuoteState;
refreshBoardData = () => operate(async () => {
    for (const view of [quoteView, priceView, notificationView]) await view.open();
    showToast('success', 'GW 同步完成');
}, true);

setAlertSetting = (key, enabled) => operate(async () => {
    const fields = { favorite: 'watchlistEnabled', filterGroup: 'filterGroupEnabled', priceMatch: 'counterQuoteEnabled', bestQuote: 'myBestEnabled' };
    const previous = await notificationView.preferences();
    const request = { expectedRevision: previous.revision };
    for (const field of Object.values(fields)) request[field] = previous[field];
    request[fields[key]] = enabled;
    await notificationView.call('preference.save', request);
    D.alertSettings[key] = enabled;
    syncAlertSettingControls();
});
stableAlertKey = alert => alert.alertId || '';
persistAlertReadKeys = async function (keys) {
    for (const id of new Set(keys)) {
        if (notificationView.records.has(id)) await notificationView.markRead(id);
    }
    return true;
};

const originalTarget = setRecommendationTarget;
setRecommendationTarget = function (...args) {
    originalTarget(...args);
    renderInstQuoteRows('all');
};
renderInstQuoteRows = function () {
    const body = element('instQuoteBody');
    if (!body) return;
    const book = priceView?.records.get(recommendationState.securityId);
    const entries = book?.marketData.NoMDEntries || [];
    const bids = entries.filter(entry => entry.MDEntryType === '0');
    const offers = entries.filter(entry => entry.MDEntryType === '1');
    const rows = [];
    for (let index = 0; index < Math.min(50, Math.max(bids.length, offers.length)); index++) {
        const bid = bids[index];
        const offer = offers[index];
        const values = [bid?.UserID, bid?.MDEntrySize, bid?.MDEntryPx, offer?.MDEntryPx, offer?.MDEntrySize, offer?.UserID];
        rows.push(`<tr>${values.map(value => `<td>${escapeQuoteHtml(value ?? '—')}</td>`).join('')}</tr>`);
    }
    body.innerHTML = rows.join('') || '<tr><td colspan="6">选择债券查看 FICC 盘口</td></tr>';
};

// Preserve remaining controls, but do not pretend their old HTTP backend is connected.
initBondFilterPanels = async () => false;
populateRegionSelectors = async () => false;
loadRecommendations = async () => false;
loadBoardIntents = async () => false;
connectQuoteSSE = async () => false;
startQuoteConfirmPolling = () => {};
startQuotePollFallback = () => {};
requestPreciseValuations = () => {};

element('app').style.display = 'none';
element('loginOverlay').classList.remove('hidden');
element('loginUser').value = 'demo';
element('loginPass').value = '';
window.ficcMessageLog = () => gateway.logs;
for (const panel of document.querySelectorAll('[id^="filters-"]')) {
    panel.title = '复杂条件筛选将在后续接入；本轮可使用对价板债券搜索';
    panel.addEventListener('click', event => {
        event.preventDefault();
        event.stopImmediatePropagation();
        showToast('info', panel.title);
    }, true);
    panel.querySelectorAll('input,select,button').forEach(control => control.disabled = true);
}
