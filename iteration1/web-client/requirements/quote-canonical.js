(function(global){
  'use strict';
  const VERSION='quote-canonical-v1';
  function text(value){return value===null||value===undefined?'':String(value);}
  function directionOf(row){
    const declared=text(row&&row.direction||row&&row.d).toLowerCase();
    if(declared==='ofr'||declared==='offer')return'ofr';
    if(declared==='bid')return'bid';
    return row&&row.op&&!row.bp?'ofr':'bid';
  }
  function normalizeQuoteRow(input,source){
    const row=input&&typeof input==='object'?input:{};
    const direction=directionOf(row),quoteId=text(row.quoteId||row.quote_id||row.id);
    const normalized={...row,
      quoteId,sourceQuoteId:text(row.sourceQuoteId||quoteId),
      source:text(source||row.source||row.sourceType||'unknown'),
      direction,c:text(row.c||row.code||row.securityId||row.security_id),
      n:text(row.n||row.name||row.securityName||row.security_name),
      priceType:text(row.priceType||row.pt||row.price_type||'到期收益率'),
      tradeDate:text(row.tradeDate||row.date||row.trade_date),
      cs:text(row.cs||row.settlement||row.settlementType||row.settlement_type),
      ownerAccountId:text(row.ownerAccountId||row.owner_account_id),
      counterpartyInstitution:text(row.counterpartyInstitution||row.inst||row.institution),
      counterpartyTrader:text(row.counterpartyTrader||row.trader),
      canonicalModelVersion:VERSION,
    };
    if(direction==='bid'){
      normalized.bp=text(row.bp||row.p||row.price);
      normalized.bvBid=text(row.bvBid||row.bv||row.vol||row.volume);
    }else{
      normalized.op=text(row.op||row.p||row.price);
      normalized.bvOfr=text(row.bvOfr||row.bv||row.vol||row.volume);
    }
    normalized.raw=row.raw&&typeof row.raw==='object'?row.raw:{...row};
    return normalized;
  }
  global.FICCQuoteCanonical=Object.freeze({VERSION,directionOf,normalizeQuoteRow});
})(window);
