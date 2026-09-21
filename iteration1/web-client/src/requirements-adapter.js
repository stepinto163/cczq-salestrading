import { installFilterEditor } from './filter-editor.js';
import { GwClient } from './gw-client.js';
import { QuoteClient, PriceClient, NotificationClient } from './business-clients.js';
import { displayQuote, pendingQuote, priceRows, bestRow, quoteInput, historyRow } from './requirements-model.js';
import { CrmClient } from './crm-client.js';

// These bindings are the original app.js integration points, shared with its inline event handlers.
let quoteView;
let priceView;
let notificationView;
let securities = new Map();
let requestedVisibleSecurities = new Set();
let pendingRender = false;
let operationBusy = false;
let signedIn = false;
const quoteDetails = new Map();
const browsingProjections = new Map();
const originalMarketSort = sortMarketRows;
sortMarketRows = rows => {
    const view = rows?.some(row => row.alertId) ? notificationView
        : rows?.some(row => D.liveQuotes?.includes(row)) ? quoteView : priceView;
    return view?.browsing ? [...rows] : originalMarketSort(rows);
};
const originalTodaySort = sortTodayQuoteRows;
sortTodayQuoteRows = rows => quoteView?.browsing ? [...rows] : originalTodaySort(rows);

function stabilizeProjection(key, view, identity) {
    const previous = browsingProjections.get(key);
    if (view.browsing && previous?.generation === view.generation) {
        const current = new Map(D[key].map(row => [identity(row), row]));
        D[key] = previous.rows.map(row => current.get(identity(row)) || { ...row, browsingUnavailable: true });
    }
    browsingProjections.set(key, { generation: view.generation, rows: D[key] });
}

function quoteUnavailableInBrowsing(id) {
    if (quoteView.unavailableIds.has(id)) return true;
    return [...browsingProjections.values()].some(projection => projection.rows.some(row =>
        (row.quoteId === id || row.bidQuoteId === id || row.ofrQuoteId === id)
        && (row.browsingUnavailable || priceView.unavailableIds.has(row.c))));
}
let historyRequestVersion = 0;
let historyQueryKey = '';
let historyCursor = null;
let historyTotal = 0;
let historyLoading = false;
let historyFilter = { conditions: [] };
const marketFilters = new Map();
const gateway = new GwClient('ws://127.0.0.1:18601/gateway', {
    onDisconnect: () => doLogout(true)
});
const element = id => document.getElementById(id);
const crm = new CrmClient(gateway);
const inputWithCrm = row => ({ ...quoteInput(row), ...crm.resolve(row) });
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
    installViewScrolling();
    loadVisibleSecurities();
    quoteView.rows().forEach(quote => crm.seed(quote));
    for (const quote of quoteView.rows()) {
        const cached = quoteDetails.get(quote.quoteId);
        if (cached && BigInt(quote.revision) >= BigInt(cached.revision)) quoteDetails.delete(quote.quoteId);
    }
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
    stabilizeProjection('liveQuotes', quoteView, row => row.quoteId);
    for (const key of ['boardCreditAll', 'boardCreditBest', 'boardRateAll', 'boardRateBest']) {
        stabilizeProjection(key, priceView, row => row.quoteId || row.c);
    }
    stabilizeProjection('boardAlert', notificationView, row => `${row.alertId}:${row.alertType}`);
    syncMgmtToday();
    renderAll();
    renderInstQuoteRows('all');
    updatePendingQuoteStats();
    updateBadges();
    for (const node of document.querySelectorAll('[data-quote-id]')) {
        const id = node.dataset.quoteId;
        const unavailable = quoteUnavailableInBrowsing(id);
        if (!unavailable) continue;
        const row = node.closest('tr') || node;
        row.style.opacity = '0.45';
        row.title = '已失效或不再匹配，请返回实时后操作';
        row.setAttribute('aria-disabled', 'true');
    }
    notifyNewAlerts(previousAlerts);
    const stale = [quoteView, priceView, notificationView].some(view => view.stale);
    element('pendingSyncState').textContent = stale ? '同步中断，请点击行情刷新或重新登录' : 'GW 实时同步正常';
}

function installViewScrolling() {
    const panels = [
        ['#quoteTableWrap', quoteView],
        ['#mgmtTodayWrap', quoteView],
        ['#st-board-credit-all .tbl-wrap', priceView],
        ['#st-board-credit-best .tbl-wrap', priceView],
        ['#st-board-rate-all .tbl-wrap', priceView],
        ['#st-board-rate-best .tbl-wrap', priceView],
        ['#st-board-alert .tbl-wrap', notificationView]
    ];
    for (const [selector, view] of panels) {
        const wrapper = document.querySelector(selector);
        if (!wrapper || !view) continue;
        if (!wrapper.dataset.pagingInstalled) {
            wrapper.dataset.pagingInstalled = 'true';
            const controls = document.createElement('div');
            controls.className = 'ficc-view-navigation';
            controls.style.cssText = 'display:flex;gap:8px;align-items:center;padding:4px 8px;flex-shrink:0';
            let pending = false;
            let lastScrollTop = wrapper.scrollTop;
            let nextNavigationTime = 0;
            const currentView = () => selector.includes('board-alert') ? notificationView
                : selector.includes('board-') ? priceView : quoteView;
            const navigate = async direction => {
                const active = currentView();
                if (!signedIn || pending || active.opening || (active.stale && direction !== 0)
                    || (direction !== 0 && Date.now() < nextNavigationTime)) return;
                const page = direction === 0 ? 1 : active.pageNumber + direction;
                if (page < 1 || (direction > 0 && (page - 1) * active.pageSize >= active.totalCount)) return;
                pending = true;
                status.textContent = '正在加载…';
                wrapper.setAttribute('aria-busy', 'true');
                try {
                    if (direction === 0) await active.resumeLive();
                    else await active.queryPage(page);
                    // Wait for the existing table renderer before positioning the new window.
                    await new Promise(resolve => requestAnimationFrame(resolve));
                    wrapper.scrollTop = direction < 0 ? wrapper.scrollHeight : 0;
                    lastScrollTop = wrapper.scrollTop;
                } catch (error) {
                    showToast('error', error.message);
                } finally {
                    pending = false;
                    nextNavigationTime = Date.now() + 250;
                    wrapper.setAttribute('aria-busy', 'false');
                    status.textContent = scrollStatus(currentView());
                }
            };
            const latest = document.createElement('button');
            latest.textContent = '返回最新';
            latest.addEventListener('click', () => navigate(0));
            controls.appendChild(latest);
            const status = document.createElement('span');
            status.className = 'ficc-view-page-state';
            status.setAttribute('aria-live', 'polite');
            controls.appendChild(status);
            wrapper.before(controls);
            wrapper.tabIndex = wrapper.tabIndex < 0 ? 0 : wrapper.tabIndex;
            wrapper.addEventListener('scroll', () => {
                const top = wrapper.scrollTop;
                const direction = Math.sign(top - lastScrollTop);
                lastScrollTop = top;
                if (pending || wrapper.scrollHeight <= wrapper.clientHeight) return;
                if (direction !== 0 && top > 0) currentView().beginBrowsing();
                if (direction > 0 && top + wrapper.clientHeight >= wrapper.scrollHeight - 2) navigate(1);
                if (direction < 0 && top <= 0) navigate(-1);
            }, { passive: true });
            wrapper.addEventListener('wheel', event => {
                if (!pending && event.deltaY > 0) currentView().beginBrowsing();
                if (event.deltaY > 0 && wrapper.scrollTop + wrapper.clientHeight >= wrapper.scrollHeight - 2) navigate(1);
                if (event.deltaY < 0 && wrapper.scrollTop <= 0) navigate(-1);
            }, { passive: true });
            wrapper.addEventListener('keydown', event => {
                if (event.target !== wrapper) return;
                if (!pending && ['ArrowDown', 'PageDown'].includes(event.key)) currentView().beginBrowsing();
                if (['ArrowDown', 'PageDown'].includes(event.key)
                    && wrapper.scrollTop + wrapper.clientHeight >= wrapper.scrollHeight - 2) navigate(1);
                if (['ArrowUp', 'PageUp'].includes(event.key) && wrapper.scrollTop <= 0) navigate(-1);
            });
        }
        const status = wrapper.previousElementSibling?.querySelector('.ficc-view-page-state');
        const button = wrapper.previousElementSibling?.querySelector('button');
        if (button) {
            button.textContent = view.stale ? '重新同步' : view.changedIds.size
                ? `有 ${view.changedIds.size} 条变更` : view.browsing ? '浏览中 · 返回实时' : '实时更新中';
            button.disabled = view.opening || (!view.browsing && !view.stale);
        }
        if (status && wrapper.getAttribute('aria-busy') !== 'true') status.textContent = scrollStatus(view);
    }
}

function scrollStatus(view) {
    if (view.stale) return '同步中断，请刷新';
    if (!view.totalCount) return '暂无匹配记录';
    const atEnd = view.pageNumber * view.pageSize >= view.totalCount;
    return `匹配 ${view.totalCount} 条 · ${atEnd ? '已到末尾' : '向下滚动加载'}${view.pageNumber > 1 ? ' · 向上滚动可返回' : ''}`;
}

async function loadSecurities(ids) {
    const cache = securities;
    const missing = [...new Set(ids.filter(Boolean))].filter(id => !cache.has(id));
    for (let offset = 0; offset < missing.length; offset += 20) {
        if (cache !== securities) return;
        const data = await gateway.request('QuoteSvr', 'ficc.base.get', {
            kind: 'SECURITY', ids: missing.slice(offset, offset + 20)
        });
        if (cache !== securities) return;
        for (const record of data.records) cache.set(record.securityId, record);
    }
}

function loadVisibleSecurities() {
    const ids = [...new Set([
        ...quoteView.rows(), ...priceView.rows(), ...notificationView.rows()
    ].map(row => row.securityId).filter(Boolean))]
        .filter(id => !securities.has(id) && !requestedVisibleSecurities.has(id));
    if (!ids.length) return;
    const cache = securities;
    ids.forEach(id => requestedVisibleSecurities.add(id));
    loadSecurities(ids).catch(error => {
        if (cache === securities && signedIn) {
            showToast('error', `债券资料加载失败，暂显示债券代码：${error.message}`);
        }
    }).finally(() => {
        if (cache === securities) scheduleUpdate();
    });
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
        return await action();
    } catch (error) {
        showToast('error', error.message);
    } finally {
        operationBusy = false;
    }
}

async function readQuote(quoteId) {
    if (!quoteId) throw new Error('未找到报价ID，请重新选择报价');
    const connection = quoteView;
    const quote = (await connection.call('get', { quoteId })).quote;
    if (!signedIn || connection !== quoteView) throw new Error('登录会话已改变，请重新操作');
    crm.seed(quote);
    quoteDetails.set(quoteId, quote);
    return quote;
}

async function command(action, quoteId, fields = {}, expectedQuote) {
    if (quoteUnavailableInBrowsing(quoteId)) {
        throw new Error('该记录已失效或不再匹配，请返回实时后操作');
    }
    // A price book is not a quote revision, and a displayed quote need not be in the small QuoteView cache.
    const quote = expectedQuote || await readQuote(quoteId);
    const reply = await quoteView.call(action, { quotes: [{ quoteId, expectedRevision: quote.revision, ...fields }] });
    const result = quoteView.batchResult(reply);
    quoteDetails.delete(quoteId);
    return result;
}

async function changeQuotes(ids, action, selectionSet) {
    const unique = [...new Set(ids.filter(Boolean))];
    if (!unique.length) throw new Error('请先选择报价');
    if (unique.length > 100) throw new Error('每批最多操作100条报价');
    let completed = 0;
    const failures = [];
    for (const id of unique) {
        try {
            await command(action, id);
            selectionSet?.delete(id);
            completed++;
        } catch (error) {
            failures.push(`${id}: ${error.message}`);
        }
    }
    if (completed) {
        await quoteView.open();
        await priceView.open();
        refreshDisplay();
    }
    if (failures.length) throw new Error(`成功${completed}条，失败${failures.length}条；${failures.join('；')}`);
    showToast('success', `已处理 ${completed} 条报价`);
    return true;
}

const originalQuoteById = quoteById;
quoteById = function (id) {
    const detail = quoteDetails.get(id);
    if (detail) return displayQuote(detail, securities.get(detail.securityId), userId());
    const current = originalQuoteById(id);
    if (current) return current;
    for (const book of priceView?.rows() || []) {
        const row = priceRows(book, securities.get(book.securityId), userId()).find(item => item.quoteId === id);
        if (row) return row;
    }
    return null;
};

doLogin = async function () {
    const button = element('loginBtn');
    button.disabled = true;
    element('loginError').textContent = '';
    try {
        resetAccountState();
        quoteDetails.clear();
        securities = new Map();
        requestedVisibleSecurities = new Set();
        const username = element('loginUser').value.trim();
        const login = await gateway.connect(username, element('loginPass').value);
        const callbacks = { onChange: scheduleUpdate, onStale: message => showToast('error', message) };
        quoteView = new QuoteClient(gateway, callbacks);
        priceView = new PriceClient(gateway, callbacks);
        notificationView = new NotificationClient(gateway, callbacks);
        await quoteView.initialize({ ownerScope: 'MINE', status: ['PENDING_CONFIRM', 'ACTIVE', 'REFERRED'], pageSize: 50 });
        await priceView.initialize({ mode: 'FULL', pageSize: 20 });
        await notificationView.initialize({ pageSize: 50 });
        const preference = await notificationView.preferences();
        D.alertSettings = { favorite: preference.watchlistEnabled, filterGroup: preference.filterGroupEnabled, priceMatch: preference.counterQuoteEnabled, bestQuote: preference.myBestEnabled };
        authToken = 'GW-SESSION'; // Legacy UI login predicate; actual session stays inside GwClient.
        signedIn = true;
        enterApplication({ username, accountId: login.userId || login.userid, name: username, role: 'trader' });
        element('loginPass').value = '';
        D.marketGroupReadOnly = true;
        await installProductFilters();
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
    quoteDetails.clear();
    marketFilters.clear();
    historyFilter = { conditions: [] };
    historyRequestVersion++;
    historyQueryKey = '';
    historyCursor = null;
    historyLoading = false;
    crm.clear();
    securities = new Map();
    requestedVisibleSecurities = new Set();
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
doPreciseIdentify = async function () {
    try {
        const lines = element('preciseInput').value.trim().split(/\r?\n/).filter(Boolean);
        if (!lines.length || lines.length > 5) throw new Error('首轮请输入 1 至 5 行固定格式：Bid 2.5000 250001.IB 1000 T+0');
        const parsed = lines.map(line => {
            if (!/^(Bid|Ofr)\s+\d+(\.\d{1,4})?\s+\d+\.(IB|SH|SZ)\s+\d+(\.\d+)?\s+T\+[01]$/i.test(line)) {
                throw new Error('识别服务尚未接入；首轮仅支持“示例填充”的固定格式');
            }
            const row = parseQuoteLine(line);
            row.vol = line.split(/\s+/)[3];
            return row;
        });
        const cache = securities;
        await loadSecurities(parsed.map(row => row.c));
        if (!signedIn || cache !== securities) return;
        const drafts = parsed.map(row => {
            const security = securities.get(row.c);
            if (!security) throw new Error(`基础资料未找到 ${row.c}`);
            return { ...row, n: security.securityName, d: row.d.toLowerCase() === 'bid' ? 'Bid' : 'Ofr',
                m: security.remainingYears, date: quoteToday(), pt: security.hasExerciseOption ? '行权收益率' : '到期收益率',
                recognitionId: crypto.randomUUID(), quoteAction: 'NEW', quoteType: '新增', inst: '', tr: '' };
        });
        D.preciseData = drafts;
        scheduleUpdate();
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
    const inputs = selected.map(inputWithCrm);
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
        await command('confirm', row.recognitionId, inputWithCrm(row));
        pendingQuoteDrafts.delete(row.recognitionId);
    }
    showToast('success', '确认成功；行情及提醒由服务端推送');
});
doDeleteQuotes = () => operate(() => changeQuotes(selectedQuoteRows().map(row => row.recognitionId), 'delete'));
setQuotesStatus = (ids, status, selection) => operate(() => changeQuotes(ids, status === 'active' ? 'unrefer' : 'refer', selection));
restoreManagedQuotes = (ids, selection) => operate(() => changeQuotes(ids, 'unrefer', selection));
deleteManagedQuotes = (ids, selection) => operate(() => changeQuotes(ids, 'delete', selection));
// The original caller expects a synchronous result object; the DPS action is asynchronous.
referFuzzy = () => setQuotesStatus([...mgmtTodaySelected], 'revoked', mgmtTodaySelected);

referSelectedBoardQuotes = key => operate(async () => {
    const quotes = selectedBoardQuotes(key);
    if (quotes.some(quote => !canManageQuote(quote))) throw new Error('选中报价包含他人报价，不能撤销');
    await changeQuotes(quotes.map(quote => quote.quoteId), 'refer', boardSelectedQuoteIds);
    syncBoardToolbarControls();
});

confirmRefer = () => operate(async () => {
    const target = ctxReferTarget;
    if (!target?.quoteId) throw new Error('请重新选择要撤销的报价');
    if (await changeQuotes([target.quoteId], 'refer')) {
        ctxReferTarget = null;
        closeModal();
    }
});

updateCtxQuoteFlag = field => {
    const id = ctxTarget?.quoteId;
    hideCtx();
    return operate(async () => {
        if (!['oco', 'packing'].includes(field)) throw new Error('不支持的报价标记');
        const quote = await readQuote(id);
        const enabled = !quote[field];
        const fields = { [field]: enabled };
        if (enabled) fields[field === 'oco' ? 'packing' : 'oco'] = false;
        await command('update', id, fields, quote);
        await quoteView.open();
        refreshDisplay();
        showToast('success', '报价标记已更新');
    });
};

saveEditQuote = () => operate(async () => {
    if (!quoteModalState || !validateQuoteForm()) return;
    const state = quoteModalState;
    const originalQuote = state.originalQuote;
    const input = inputWithCrm({
        institutionId: originalQuote?.institutionId, contactId: originalQuote?.contactId,
        c: state.sourceRow.c, d: state.direction, p: quoteFieldValue('quotePrice'),
        vol: quoteFieldValue('quoteVolume'), pt: quoteFieldValue('quotePriceType'),
        cs: quoteFieldValue('quoteSettlement'), date: quoteFieldValue('quoteTradeDate'),
        inst: quoteFieldValue('quoteInstitution'), tr: quoteFieldValue('quoteTrader'),
        urgent: element('quoteUrgent').checked, recommend: element('quoteRecommend').checked,
        round: element('quoteRound').checked, oco: state.sourceRow.oco, packing: state.sourceRow.packing
    });
    input.remark = quoteFieldValue('quoteNote');
    if (state.mode === 'edit') await command('update', state.quoteId, input, originalQuote);
    else await quoteView.create(input);
    closeModal();
    quoteModalState = null;
    showToast('success', state.mode === 'edit' ? '修改已提交' : '已新建待确认报价，请到报价确认完成确认');
});

const originalOpenQuoteForm = openQuoteForm;
openQuoteForm = function (mode, index, dataType, requestedQuoteId, origin) {
    return operate(async () => {
        const source = boardDataByType(dataType)?.[index];
        if (!source) throw new Error('未找到对应行情数据');
        const id = requestedQuoteId || source.sourceQuoteId || (source.bp ? source.bidQuoteId : source.ofrQuoteId) || source.quoteId;
        let quote;
        if (mode === 'edit') {
            quote = await readQuote(id);
            if (quote.status !== 'ACTIVE' && quote.status !== 'PENDING_CONFIRM') throw new Error('仅有效或待确认报价可以修改');
        }
        quoteModalState = null;
        originalOpenQuoteForm(mode, index, dataType, id, origin);
        if (!quoteModalState) return;
        quoteModalState.originalQuote = quote;
        if (quote?.institutionId) element('quoteInstitution').value = `${quote.institutionName} [${quote.institutionId}]`;
        if (quote?.contactId) element('quoteTrader').value = `${quote.contactName} [${quote.contactId}]`;
        for (const option of element('quoteVolumeUnit').options) {
            option.disabled = !['万', '万面额'].includes(option.value);
        }
    });
};

loadQuoteState = async function () {
    if (!signedIn) return false;
    await quoteView.open();
    return true;
};
loadMgmtToday = loadQuoteState;
loadQuoteConfirmBatches = loadQuoteState;

// History is an explicit, bounded query, separate from live Quote/Price subscriptions.
syncMgmtHistory = function () {
    refreshMgmtDatalists('history');
    filterMgmtHistory();
};

updateMgmtHistoryStatusbar = function () {
    const status = element('mgmtHistoryStatusbar');
    if (historyLoading) {
        status.textContent = '正在查询历史报价…';
        return;
    }
    if (!historyQueryKey) {
        status.textContent = '请选择日期并查询历史报价';
        return;
    }
    status.textContent = `已加载${D.mgmtHistory.length} / ${historyTotal}条；筛选后${D.mgmtHistoryFiltered.length}条；查询日期 ${historyQueryKey}`;
};

async function loadHistory(append = false) {
    if (!signedIn) return;
    const fromDate = element('historyStartDate').value;
    const toDate = element('historyEndDate').value;
    if (!fromDate || !toDate || fromDate > toDate) {
        showToast('error', '请选择有效日期区间，开始日期不能晚于结束日期');
        return;
    }
    const queryKey = `${fromDate}/${toDate}/${JSON.stringify(historyFilter)}`;
    if (append && (!historyCursor || historyQueryKey !== queryKey)) return;
    if (append && D.mgmtHistory.length >= 1000) {
        showToast('info', '当前界面最多加载1000条历史记录，请缩小日期范围');
        return;
    }
    if (historyLoading && append) return;
    const version = ++historyRequestVersion;
    historyLoading = true;
    element('historyQueryBtn').disabled = true;
    element('historyMoreBtn').disabled = true;
    element('mgmtHistoryStatusbar').textContent = '正在查询历史报价…';
    try {
        const request = append ? { cursor: historyCursor } : { fromDate, toDate, limit: 200, filter: historyFilter };
        const data = await quoteView.call('history.search', request);
        if (!signedIn || version !== historyRequestVersion) return;
        for (const item of data.rows) {
            if (item.reference) securities.set(item.quote.securityId, item.reference);
        }
        const rows = data.rows.map(item => historyRow(item, userId()));
        D.mgmtHistory = append ? [...D.mgmtHistory, ...rows] : rows;
        historyCursor = data.nextCursor;
        historyTotal = data.totalCount;
        historyQueryKey = queryKey;
        mgmtHistorySelected.clear();
        syncMgmtHistory();
        element('mgmtHistoryStatusbar').textContent = `已加载${D.mgmtHistory.length} / ${historyTotal}条；筛选后${D.mgmtHistoryFiltered.length}条`;
    } catch (error) {
        if (!signedIn || version !== historyRequestVersion) return;
        if (!append) {
            D.mgmtHistory = [];
            D.mgmtHistoryFiltered = [];
            historyQueryKey = '';
            historyCursor = null;
            syncMgmtHistory();
        }
        element('mgmtHistoryStatusbar').textContent = `历史查询失败：${error.message}`;
        showToast('error', `历史查询失败：${error.message}`);
    } finally {
        if (version === historyRequestVersion) {
            historyLoading = false;
            element('historyQueryBtn').disabled = false;
            element('historyMoreBtn').disabled = !historyCursor;
        }
    }
}

const historyQueryButton = document.createElement('button');
historyQueryButton.id = 'historyQueryBtn';
historyQueryButton.className = 'tb-btn tb-btn-primary';
historyQueryButton.textContent = '查询';
historyQueryButton.onclick = () => loadHistory();
const historyMoreButton = document.createElement('button');
historyMoreButton.id = 'historyMoreBtn';
historyMoreButton.className = 'tb-btn tb-btn-default';
historyMoreButton.textContent = '加载更多';
historyMoreButton.disabled = true;
historyMoreButton.onclick = () => loadHistory(true);
element('historyEndDate').after(historyQueryButton, historyMoreButton);

const originalSwitchSubTab = switchSubTab;
switchSubTab = function (tab) {
    originalSwitchSubTab(tab);
    if (tab.dataset.st === 'mgmt-history' && !historyQueryKey && !historyLoading) return loadHistory();
};

// A historical row is immutable. Restore/delete targets its current quote, with a fresh revision.
unreferHistory = () => restoreManagedQuotes(selectedHistoryQuoteIds());
deleteHistory = () => deleteManagedQuotes(selectedHistoryQuoteIds());

ctxLog = function () {
    const id = ctxTarget?.quoteId;
    hideCtx();
    return operate(async () => {
        const data = await quoteView.call('history.query', { quoteId: id, limit: 200 });
        const actions = { CREATE: 'quote', CONFIRM: 'quote', UPDATE: 'edit', REFER: 'revoke', UNREFER: 'restore', DELETE: 'delete' };
        const logs = data.records.map(record => ({
            time: record.operationTime, user: record.operatorUserId, action: actions[record.action] || record.action,
            direction: record.after?.side === '1' ? 'bid' : 'ofr', oldPrice: record.before?.price,
            price: record.after?.price, volume: record.after?.quantity, note: record.reason || ''
        }));
        showModal('报价操作记录', quoteLogRowsHtml(logs));
    });
};

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
// Keep the original CRM popup and keyboard interaction; replace its mock data lookup.
let crmSearchTimer;
let crmSearchVersion = 0;
renderCrmSuggestions = function () {
    clearTimeout(crmSearchTimer);
    const version = ++crmSearchVersion;
    const state = crmSuggestionState;
    if (!state.input) return;
    const text = state.input.value;
    const panel = crmSuggestionPanel();
    state.items = [];
    state.activeIndex = -1;
    panel.replaceChildren();
    const message = document.createElement('div');
    message.className = 'crm-suggest-empty';
    message.textContent = '正在查询 CRM…';
    panel.append(message);
    panel.classList.add('show');
    requestAnimationFrame(positionCrmSuggestionPanel);
    crmSearchTimer = setTimeout(async () => {
        const current = () => signedIn && version === crmSearchVersion && crmSuggestionState === state
            && state.input.isConnected && state.input.value === text;
        if (!current()) return;
        try {
            const data = await crm.search(state.kind === 'institution' ? 'INSTITUTION' : 'CONTACT', text, state.institution);
            if (!current()) return;
            state.items = data.items;
            state.activeIndex = data.items.length ? 0 : -1;
            panel.replaceChildren();
            for (const [index, item] of data.items.entries()) {
                const button = document.createElement('button');
                button.type = 'button';
                button.className = 'crm-suggest-item' + (index === 0 ? ' active' : '');
                button.setAttribute('role', 'option');
                button.textContent = item.value;
                button.addEventListener('click', () => selectCrmSuggestion(index));
                panel.append(button);
            }
            if (!data.items.length || data.hasMore) {
                message.textContent = data.items.length ? '显示前 50 条，请细化搜索' : '无匹配记录，可保留名称待确认';
                panel.append(message);
            }
            // Long IDs must remain readable in the existing popup.
            requestAnimationFrame(() => {
                positionCrmSuggestionPanel();
                panel.style.width = Math.min(460, innerWidth - 12) + 'px';
                panel.style.left = Math.max(6, Math.min(innerWidth - panel.offsetWidth - 6, state.input.getBoundingClientRect().left)) + 'px';
            });
        } catch (error) {
            if (current()) message.textContent = 'CRM 查询失败：' + error.message;
        }
    }, 250);
};

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
element('loginUser').value = 'trader1';
element('loginPass').value = '';
window.ficcMessageLog = () => gateway.logs;
async function installProductFilters() {
    const { filterFields } = await quoteView.call('dictionary.query', {});
    for (const [id, domain, market] of [
        ['filters-credit', 'price', 'CREDIT'], ['filters-rate', 'price', 'RATE'],
        ['filters-merged', 'notification', null], ['filters-merged-mgmt', 'quote', null]
    ]) {
        const panel = element(id);
        if (!panel) continue;
        installFilterEditor(panel, filterFields, async filter => {
            if (market) {
                filter.market = market;
                marketFilters.set(market, filter);
            }
            if (domain === 'quote') {
                historyFilter = filter;
                historyQueryKey = '';
            }
            const view = domain === 'price' ? priceView : domain === 'quote' ? quoteView : notificationView;
            await view.open({ ...view.query, filter });
            if (domain === 'quote' && document.querySelector('#st-mgmt-history.active')) await loadHistory();
            refreshDisplay();
        }, message => showToast('error', message));
    }
}

document.addEventListener('click', event => {
    const key = event.target.closest('[data-st]')?.dataset.st;
    if (!signedIn || !['board-credit-all', 'board-credit-best', 'board-rate-all', 'board-rate-best'].includes(key)) return;
    const market = key.includes('credit') ? 'CREDIT' : 'RATE';
    const filter = marketFilters.get(market) || { market, conditions: [] };
    priceView.open({ ...priceView.query, filter }).catch(error => showToast('error', error.message));
});
