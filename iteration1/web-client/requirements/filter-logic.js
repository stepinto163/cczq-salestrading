(function exposeFilterLogic(root, factory) {
  const api = factory();
  if (typeof module === 'object' && module.exports) module.exports = api;
  if (root) Object.assign(root, api);
})(typeof globalThis !== 'undefined' ? globalThis : this, function createFilterLogic() {
  function normalizeCode(value) {
    return String(value || '').trim().toUpperCase();
  }

  function nonEmptyFiniteQuote(value) {
    return value !== '' && value !== null && value !== undefined && Number.isFinite(Number(value));
  }

  function quoteDirection(row) {
    const explicit = String(row && (row.direction || row.d) || '').trim().toLowerCase();
    if (explicit === 'bid') return 'bid';
    if (explicit === 'ofr' || explicit === 'offer') return 'ofr';
    if (nonEmptyFiniteQuote(row && row.bp) && !nonEmptyFiniteQuote(row && row.op)) return 'bid';
    if (nonEmptyFiniteQuote(row && row.op) && !nonEmptyFiniteQuote(row && row.bp)) return 'ofr';
    return '';
  }

  function quotePairPriceType(row) {
    const value = String(row && (row.priceType || row.pt) || '到期收益率').trim();
    if (value.includes('行权')) return 'YTC';
    if (value.includes('净价')) return 'NET';
    if (value.includes('全价')) return 'FULL';
    return 'YTM';
  }

  function quotePairSettlement(row) {
    return String(row && (row.cs || row.settlement) || '').trim().toUpperCase();
  }

  function quotePairKey(row) {
    return normalizeCode(row && row.c) + '|' + quotePairPriceType(row) + '|' + quotePairSettlement(row);
  }

  function quotesComparableForPair(left, right) {
    if (!left || !right || normalizeCode(left.c) !== normalizeCode(right.c) || quotePairKey(left) !== quotePairKey(right)) return false;
    const leftDirection = quoteDirection(left), rightDirection = quoteDirection(right);
    if (!['YTM', 'YTC'].includes(quotePairPriceType(left)) || !leftDirection || !rightDirection || leftDirection === rightDirection) return false;
    const leftPrice = leftDirection === 'bid' ? left.bp : left.op;
    const rightPrice = rightDirection === 'bid' ? right.bp : right.op;
    return nonEmptyFiniteQuote(leftPrice) && nonEmptyFiniteQuote(rightPrice);
  }

  function quotesOppositeForAlert(left, right) {
    if (!left || !right || normalizeCode(left.c) !== normalizeCode(right.c)) return false;
    const leftDirection = quoteDirection(left), rightDirection = quoteDirection(right);
    return Boolean(leftDirection && rightDirection && leftDirection !== rightDirection);
  }

  function quoteSpreadIndex(rows) {
    const index = new Map();
    (rows || []).forEach(row => {
      if (!['YTM', 'YTC'].includes(quotePairPriceType(row))) return;
      const key = quotePairKey(row), entry = index.get(key) || { bid: [], ofr: [] };
      if (nonEmptyFiniteQuote(row.bp)) entry.bid.push(Number(row.bp));
      if (nonEmptyFiniteQuote(row.op)) entry.ofr.push(Number(row.op));
      index.set(key, entry);
    });
    return index;
  }

  return { nonEmptyFiniteQuote, quotePairPriceType, quotePairSettlement, quotePairKey, quotesComparableForPair, quotesOppositeForAlert, quoteSpreadIndex };
});
