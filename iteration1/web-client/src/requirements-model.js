// Boundary mapping only. The original page keeps its column names and renderers.
export function displayQuote(quote, security = {}, userId = '') {
    const bid = quote.side === '1';
    return {
        quoteId: quote.quoteId, sourceQuoteId: quote.quoteId,
        institutionId: quote.institutionId, contactId: quote.contactId,
        ownerAccountId: quote.ownerUserId, ownerDisplayName: quote.ownerUserId,
        canManage: quote.ownerUserId === userId, version: quote.revision, dataVersion: quote.revision,
        status: { ACTIVE: 'active', REFERRED: 'revoked', PENDING_CONFIRM: 'pending', DELETED: 'deleted' }[quote.status],
        c: quote.securityId, n: security.securityName || quote.securityId,
        market: security.market === 'RATE' ? 'rate' : 'credit', m: security.remainingYears || '',
        r: security.issuerRating || '', br: security.bondRating || '',
        direction: bid ? 'bid' : 'ofr', bp: bid ? quote.price : '', op: bid ? '' : quote.price,
        bv: quote.quantity, bvBid: bid ? quote.quantity : '', bvOfr: bid ? '' : quote.quantity,
        inst: quote.institutionName || '', counterpartyInstitution: quote.institutionName || '',
        trader: quote.contactName || '', counterpartyTrader: quote.contactName || '', tr: quote.ownerUserId,
        instBid: bid ? quote.institutionName || '' : '', instOfr: bid ? '' : quote.institutionName || '',
        trBid: bid ? quote.ownerUserId : '', trOfr: bid ? '' : quote.ownerUserId,
        cs: quote.settlType === '1' ? 'T+0' : quote.settlType === '2' ? 'T+1' : quote.settlType,
        priceType: quote.yieldType === '2' ? '行权收益率' : '到期收益率',
        tradeDate: quote.tradeDate, createdAt: quote.createTime, updatedAt: quote.updateTime,
        quotedAt: quote.quoteTime || quote.createTime, t: quote.updateTime?.slice(11, 19) || '',
        urgent: quote.urgent, recommend: quote.recommended, round: quote.whole, oco: quote.oco, packing: quote.packing,
        nzYTM: security.valuationYield || '', nzNet: '', csYTM: '', mode: '手工录入'
    };
}

export function pendingQuote(quote, security, userId) {
    const row = displayQuote(quote, security, userId);
    return {
        ...row, recognitionId: quote.quoteId, batchId: quote.quoteId, batchStatus: 'pending_review',
        src: quote.source?.type || 'MANUAL', receivedAt: quote.createTime,
        d: quote.side === '1' ? 'Bid' : 'Ofr', p: quote.price, vol: quote.quantity,
        date: quote.tradeDate, pt: row.priceType,
        inst: quote.institutionId ? `${quote.institutionName} [${quote.institutionId}]` : row.inst,
        tr: quote.contactId ? `${quote.contactName} [${quote.contactId}]` : quote.contactName || '',
        rec: quote.recommended, whole: quote.whole
    };
}

export function priceRows(book, security, userId) {
    return (book.marketData.NoMDEntries || []).map(entry => {
        const quoteId = book.quoteRefs?.[entry.MDEntryID]?.quoteId || entry.MDEntryID.replace(/^FICC:/, '');
        const date = entry.MDEntryDate || '';
        const time = date.length === 8 ? `${date.slice(0, 4)}-${date.slice(4, 6)}-${date.slice(6)}T${entry.MDEntryTime}+08:00` : '';
        return displayQuote({
            quoteId, ownerUserId: entry.UserID, side: entry.MDEntryType === '0' ? '1' : '2',
            price: entry.MDEntryPx, quantity: entry.MDEntrySize, settlType: entry.SettlType,
            securityId: book.securityId, status: 'ACTIVE', revision: book.bookRevision,
            yieldType: entry.YieldToExecution != null ? '2' : '1', updateTime: time, quoteTime: time
        }, security, userId);
    });
}

export function bestRow(rows) {
    if (!rows.length) return null;
    // Entries already follow the server's price/time ordering. Do not rank quotes again here.
    const bid = rows.find(row => row.direction === 'bid');
    const offer = rows.find(row => row.direction === 'ofr');
    return {
        ...rows[0], bp: bid?.bp || '', op: offer?.op || '',
        bvBid: bid?.bvBid || '', bvOfr: offer?.bvOfr || '',
        trBid: bid?.trBid || '', trOfr: offer?.trOfr || '',
        bidQuoteId: bid?.quoteId || '', ofrQuoteId: offer?.quoteId || ''
    };
}

export function quoteInput(row) {
    const direction = String(row.d || '').toLowerCase();
    if (!['bid', 'ofr'].includes(direction)) throw new Error('首轮请选择单边 Bid 或 Ofr');
    if (!['到期收益率', '行权收益率'].includes(row.pt)) throw new Error('首轮仅支持到期/行权收益率，价格换算尚未接入');
    if (!/^\d+(\.\d{1,4})?$/.test(String(row.p))) throw new Error('收益率最多四位小数');
    if (!['T+0', 'T+1'].includes(row.cs)) throw new Error('首轮交割速度请选择 T+0 或 T+1');
    return {
        securityId: row.c, side: direction === 'bid' ? '1' : '2', quoteType: '1',
        yieldType: row.pt === '行权收益率' ? '2' : '1', price: String(row.p), quantity: String(row.vol),
        tradeDate: row.date, settlType: row.cs === 'T+0' ? '1' : '2',
        institutionName: row.inst?.trim() || null, contactName: row.tr?.trim() || null,
        urgent: Boolean(row.urgent), recommended: Boolean(row.recommend || row.rec),
        whole: Boolean(row.round || row.whole), oco: Boolean(row.oco), packing: Boolean(row.packing)
    };
}
