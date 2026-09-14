(function(global){
  'use strict';
  function shanghaiParts(date){
    return new Intl.DateTimeFormat('en-CA',{timeZone:'Asia/Shanghai',year:'numeric',month:'2-digit',day:'2-digit',hour:'2-digit',minute:'2-digit',second:'2-digit',hourCycle:'h23'}).formatToParts(date||new Date()).reduce(function(out,item){out[item.type]=item.value;return out;},{});
  }
  function shanghaiDate(date){const p=shanghaiParts(date);return p.year+'-'+p.month+'-'+p.day;}
  function addDateDays(value,days){const p=String(value||'').split('-').map(Number),d=new Date(Date.UTC(p[0],p[1]-1,p[2]+Number(days||0)));return d.toISOString().slice(0,10);}
  function directionOf(row){return global.FICCQuoteCanonical?global.FICCQuoteCanonical.directionOf(row):(row&&row.op&&!row.bp?'ofr':'bid');}
  global.FICCQuoteUtils=Object.freeze({shanghaiParts,shanghaiDate,addDateDays,directionOf});
})(window);
