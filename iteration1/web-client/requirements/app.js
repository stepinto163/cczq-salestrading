const browserLocalStorage=window.localStorage;
const localStorage=window.desktopAPI&&window.desktopAPI.storage?{
  getItem(key){
    const desktopValue=window.desktopAPI.storage.getItem(key);
    if(desktopValue!==null&&desktopValue!==undefined)return desktopValue;
    const legacyValue=browserLocalStorage.getItem(key);
    if(legacyValue!==null)window.desktopAPI.storage.setItem(key,legacyValue);
    return legacyValue;
  },
  setItem(key,value){
    window.desktopAPI.storage.setItem(key,String(value));
    browserLocalStorage.removeItem(key);
  },
  removeItem(key){
    window.desktopAPI.storage.removeItem(key);
    browserLocalStorage.removeItem(key);
  }
}:browserLocalStorage;

function safeStoredArray(key){
  try{
    const value=JSON.parse(localStorage.getItem(key)||'[]');
    return Array.isArray(value)?value:[];
  }catch(error){
    console.warn('[storage] ignored invalid array',key,error);
    return[];
  }
}
const legacyMarketGroupState={
  favoriteCredit:safeStoredArray('ficc_favBondGroupsCredit'),
  favoriteRate:safeStoredArray('ficc_favBondGroupsRate'),
  filterCredit:safeStoredArray('ficc_filterGroupsCredit'),
  filterRate:safeStoredArray('ficc_filterGroupsRate'),
};

// 硬编码应急账号已按安全要求清理：所有登录必须通过后端认证服务（/api/auth/login）
let currentUser=null;
let authToken='';
let authSessionGeneration=0;
let accountRows=[];
let activeSubTabs={};
let alertViewedCount=0;
let readAlertKeys=new Set(),alertReadLoaded=false;
let platformTradeDate='';
const mgmtToolbarState={
  today:{bond:'',institution:'',gwTrader:'',mode:'',status:''},
  history:{bond:'',institution:'',gwTrader:'',mode:'',status:''},
};
const mgmtSuggestionState={
  today:{bond:{items:[],active:-1},institution:{items:[],active:-1},gwTrader:{items:[],active:-1}},
  history:{bond:{items:[],active:-1},institution:{items:[],active:-1},gwTrader:{items:[],active:-1}},
};
function isAdmin(){return!!currentUser&&currentUser.role==='admin';}
const PAGE_ROLE_ACCESS={accounts:['admin'],version:['admin'],requirements:['admin','technician'],'requirement-spec':['admin']};
function canAccessPage(page){const allowed=PAGE_ROLE_ACCESS[page];return!allowed||!!currentUser&&allowed.includes(currentUser.role);}
function currentAccountId(){return currentUser?(currentUser.accountId||currentUser.username):'';}
function canManageQuote(quote){return!!quote&&(isAdmin()||quote.canManage===true||quote.ownerAccountId===currentAccountId());}
function quoteIsMine(quote){return!!quote&&(quote.ownerAccountId===currentAccountId()||quote.ownerUsername===(currentUser&&currentUser.username));}
function authHeaders(extra){return Object.assign({'Content-Type':'application/json'},authToken?{'Authorization':'Bearer '+authToken}:{},extra||{});}
function authFetch(url,options){const opts=Object.assign({},options||{});opts.headers=authHeaders(opts.headers);return fetch(url,opts).then(r=>{if(r.status===401&&authToken){showToast('error','登录状态已失效，请重新登录');doLogout(true);throw new Error('HTTP 401');}return r;});}
function fuzzyQuoteFetch(path,options){
  const suffix=path||'';
  return authFetch(API_BASE+'/fuzzy-quotes'+suffix,options).then(response=>{
    if(![404,405,501].includes(response.status))return response;
    return authFetch(API_BASE+'/board-intents'+suffix,options);
  });
}
async function ensureApplicationServices(){
  if(window.desktopAPI&&window.desktopAPI.ensureServices)return window.desktopAPI.ensureServices();
  return{backend:true};
}

const D={
  quoteConfirm:[],
  quoteConfirmBatches:[],
  quoteConfirmStats:{batchCount:0,quoteCount:0,qtBatchCount:0,idealBatchCount:0,bidCount:0,ofrCount:0,errorCount:0},
  boardCreditAll:[],
  boardCreditBest:[],
  boardRateAll:[],
  boardRateBest:[],
  boardAlert:[],
  boardIntent:[],
  liveQuotes:[],
  quoteEvents:[],
  quoteStateLoaded:false,
  favBonds:[],
  favBondGroupsCredit:[],
  favBondGroupsRate:[],
  filterGroupsCredit:[],
  filterGroupsRate:[],
  marketGroupRevision:0,
  marketGroupLoaded:false,
  marketGroupReadOnly:false,
  alertSettings:{favorite:true,filterGroup:true,priceMatch:true,bestQuote:true},
  preciseData:[],fuzzyData:[],
  mgmtToday:[],mgmtTodayAll:[],
  mgmtHistory:[
    {date:'06-17',mode:'Firm',rs:'已成交',ts:'已成交',m:'4.50Y',c:'102480625.IB',n:'24晋能山西MTN002',d:'Bid',p:'2.26',vol:'1500',pt:'到期收益率',nzVal:'100.87',dev:'-0.890',cs:'T+1',t:'平安基金',tr:'赵明',time:'10:45',tags:['Firm']},
    {date:'06-17',mode:'Nonfirm',rs:'已成交',ts:'已成交',m:'1Y',c:'102381799.IB',n:'23西咸新发MTN002',d:'Bid',p:'1.9662',vol:'1500',pt:'到期收益率',nzVal:'100.32',dev:'-0.320',cs:'T+1',t:'招银理财',tr:'陈磊',time:'13:15',tags:['荐']},
    {date:'06-16',mode:'Firm',rs:'已成交',ts:'已成交',m:'2.50Y',c:'252980.SH',n:'23大理债',d:'Ofr',p:'4.00',vol:'5000',pt:'到期收益率',nzVal:'105.59',dev:'-0.879',cs:'T+1',t:'创金合信',tr:'丁茂国',time:'15:46',tags:['Firm']},
    {date:'06-16',mode:'Nonfirm',rs:'有效',ts:'未成交',m:'5.22Y',c:'210210.IB',n:'21国开10',d:'Bid',p:'1.90',vol:'1000',pt:'到期收益率',nzVal:'101.56',dev:'-0.330',cs:'T+1',t:'光大证券',tr:'刘明',time:'14:20',tags:[]},
    {date:'06-15',mode:'Firm',rs:'已成交',ts:'已成交',m:'8.13Y',c:'240210.IB',n:'24国开10',d:'Bid',p:'1.86',vol:'2000',pt:'到期收益率',nzVal:'102.18',dev:'-1.250',cs:'T+1',t:'中信证券',tr:'张伟',time:'16:16',tags:['Firm']},
    {date:'06-15',mode:'Nonfirm',rs:'已成交',ts:'已成交',m:'1.22Y+1Y',c:'188572.SH',n:'21柳发02',d:'Bid',p:'3.50',vol:'500',pt:'行权收益率',nzVal:'105.59',dev:'-2.786',cs:'T+1',t:'平安基金',tr:'赵明',time:'15:30',tags:['急']},
    {date:'06-14',mode:'Firm',rs:'已撤销',ts:'未成交',m:'4.92Y',c:'2021017.IB',n:'20青田农商二级01',d:'Ofr',p:'1.92',vol:'10000',pt:'到期收益率',nzVal:'100.15',dev:'-0.150',cs:'T+1',t:'中信证券',tr:'张伟',time:'14:20',tags:[]},
    {date:'06-14',mode:'Nonfirm',rs:'有效',ts:'部分成交',m:'1.13Y',c:'170010.IB',n:'17附息国债10',d:'Bid',p:'1.55',vol:'5000',pt:'到期收益率',nzVal:'99.82',dev:'0.820',cs:'T+0',t:'中信证券',tr:'张伟',time:'11:50',tags:['整']},
    {date:'06-13',mode:'Firm',rs:'已成交',ts:'已成交',m:'3.10Y',c:'102484688.IB',n:'25中鼎股份MTN001',d:'Ofr',p:'1.88',vol:'4000',pt:'到期收益率',nzVal:'100.52',dev:'-0.520',cs:'T+1',t:'创金合信',tr:'丁茂国',time:'09:50',tags:['Firm']},
    {date:'06-13',mode:'Nonfirm',rs:'有效',ts:'未成交',m:'58D+3Y',c:'032280666.IB',n:'22惠民城建PPN004',d:'Bid',p:'3.00',vol:'200',pt:'行权收益率',nzVal:'105.59',dev:'-11.691',cs:'T+1',t:'平安基金',tr:'赵明',time:'10:05',tags:[]},
    {date:'06-12',mode:'Firm',rs:'已成交',ts:'已成交',m:'2.14Y+2Y',c:'102484124.IB',n:'24首开MTN006',d:'Bid',p:'2.93',vol:'1000',pt:'到期收益率',nzVal:'100.08',dev:'-0.080',cs:'T+1',t:'中信证券',tr:'张伟',time:'08:50',tags:['Firm']},
    {date:'06-12',mode:'Nonfirm',rs:'已成交',ts:'已成交',m:'1.96Y',c:'255258.SH',n:'24康居01',d:'Bid',p:'1.92',vol:'10000',pt:'到期收益率',nzVal:'102.45',dev:'-0.450',cs:'T+1',t:'招银理财',tr:'陈磊',time:'09:15',tags:['整']},
    {date:'06-11',mode:'Firm',rs:'已撤销',ts:'未成交',m:'6.25Y',c:'102580247.IB',n:'25浙交投MTN001',d:'Ofr',p:'2.54',vol:'2000',pt:'到期收益率',nzVal:'101.31',dev:'-0.310',cs:'T+1',t:'招银理财',tr:'陈磊',time:'09:00',tags:['荐']},
    {date:'06-11',mode:'Nonfirm',rs:'有效',ts:'未成交',m:'0.75Y',c:'012582704.IB',n:'25南京铁建SCP004',d:'Bid',p:'1.58',vol:'1000',pt:'到期收益率',nzVal:'100.12',dev:'0.120',cs:'T+0',t:'平安基金',tr:'赵明',time:'09:30',tags:['Firm']},
    {date:'06-10',mode:'Firm',rs:'已成交',ts:'已成交',m:'1.25Y',c:'032000764.IB',n:'20清池投资PPN001',d:'Ofr',p:'4.50',vol:'200',pt:'到期收益率',nzVal:'105.59',dev:'-0.211',cs:'T+1',t:'创金合信',tr:'丁茂国',time:'10:20',tags:['Firm']},
    {date:'06-10',mode:'Nonfirm',rs:'已成交',ts:'已成交',m:'4.92Y',c:'2021017.IB',n:'20青田农商二级01',d:'Bid',p:'1.95',vol:'2500',pt:'到期收益率',nzVal:'100.15',dev:'-0.120',cs:'T+1',t:'光大证券',tr:'刘明',time:'08:40',tags:[]},
    {date:'06-09',mode:'Firm',rs:'有效',ts:'未成交',m:'37.17Y',c:'130010.IB',n:'13附息国债10',d:'Bid',p:'2.15',vol:'800',pt:'到期收益率',nzVal:'103.45',dev:'-0.180',cs:'T+1',t:'光大证券',tr:'刘明',time:'11:30',tags:['荐']},
    {date:'06-09',mode:'Nonfirm',rs:'已成交',ts:'已成交',m:'8.13Y',c:'240210.IB',n:'24国开10',d:'Ofr',p:'1.95',vol:'3000',pt:'到期收益率',nzVal:'102.18',dev:'-1.160',cs:'T+1',t:'光大证券',tr:'刘明',time:'08:30',tags:['Firm']},
    {date:'06-07',mode:'Firm',rs:'已成交',ts:'已成交',m:'1.61Y+N',c:'102480625.IB',n:'24晋能山西MTN002',d:'Bid',p:'2.2576',vol:'500',pt:'到期收益率',nzVal:'100.87',dev:'-0.670',cs:'T+1',t:'创金合信',tr:'丁茂国',time:'16:01',tags:['Firm']},
    {date:'06-07',mode:'Nonfirm',rs:'已成交',ts:'已成交',m:'1Y',c:'102381799.IB',n:'23西咸新发MTN002',d:'Ofr',p:'2.00',vol:'1500',pt:'到期收益率',nzVal:'100.32',dev:'-0.300',cs:'T+1',t:'招银理财',tr:'陈磊',time:'08:10',tags:['急']},
    {date:'06-06',mode:'Firm',rs:'已成交',ts:'已成交',m:'2.50Y',c:'252980.SH',n:'23大理债',d:'Bid',p:'3.80',vol:'1000',pt:'到期收益率',nzVal:'105.59',dev:'-1.079',cs:'T+1',t:'平安基金',tr:'赵明',time:'14:12',tags:['Firm']},
    {date:'06-06',mode:'Nonfirm',rs:'已成交',ts:'已成交',m:'4.50Y',c:'102480625.IB',n:'24晋能山西MTN002',d:'Ofr',p:'2.28',vol:'500',pt:'到期收益率',nzVal:'100.87',dev:'-0.670',cs:'T+1',t:'创金合信',tr:'丁茂国',time:'13:57',tags:['荐']},
  ],
  mgmtTrade:[
    {mode:'Firm',rs:'已成交',ts:'已成交',m:'4.50Y',c:'102480625.IB',n:'24晋能山西MTN002',d:'Bid',p:'2.26',vol:'1500',pt:'到期收益率',nzVal:'100.87',dev:'-0.890',cs:'T+1',t:'平安基金',tr:'赵明',time:'10:45',tags:['Firm']},
    {mode:'Nonfirm',rs:'已成交',ts:'已成交',m:'1Y',c:'102381799.IB',n:'23西咸新发MTN002',d:'Bid',p:'1.9662',vol:'1500',pt:'到期收益率',nzVal:'100.32',dev:'-0.320',cs:'T+1',t:'招银理财',tr:'陈磊',time:'13:15',tags:['荐']},
    {mode:'Firm',rs:'已成交',ts:'已成交',m:'2.50Y',c:'252980.SH',n:'23大理债',d:'Ofr',p:'4.00',vol:'5000',pt:'到期收益率',nzVal:'105.59',dev:'-0.879',cs:'T+1',t:'创金合信',tr:'丁茂国',time:'15:46',tags:['Firm']},
    {mode:'Firm',rs:'已成交',ts:'已成交',m:'8.13Y',c:'240210.IB',n:'24国开10',d:'Bid',p:'1.86',vol:'2000',pt:'到期收益率',nzVal:'102.18',dev:'-1.250',cs:'T+1',t:'中信证券',tr:'张伟',time:'16:16',tags:['Firm']},
    {mode:'Nonfirm',rs:'已成交',ts:'部分成交',m:'1.22Y+1Y',c:'188572.SH',n:'21柳发02',d:'Bid',p:'3.50',vol:'500',pt:'行权收益率',nzVal:'105.59',dev:'-2.786',cs:'T+1',t:'平安基金',tr:'赵明',time:'15:30',tags:['急']},
    {mode:'Firm',rs:'已成交',ts:'已成交',m:'3.10Y',c:'102484688.IB',n:'25中鼎股份MTN001',d:'Ofr',p:'1.88',vol:'4000',pt:'到期收益率',nzVal:'100.52',dev:'-0.520',cs:'T+1',t:'创金合信',tr:'丁茂国',time:'09:50',tags:['Firm']},
    {mode:'Nonfirm',rs:'已成交',ts:'已成交',m:'2.14Y+2Y',c:'102484124.IB',n:'24首开MTN006',d:'Bid',p:'2.93',vol:'1000',pt:'到期收益率',nzVal:'100.08',dev:'-0.080',cs:'T+1',t:'中信证券',tr:'张伟',time:'08:50',tags:['Firm']},
    {mode:'Firm',rs:'已成交',ts:'已成交',m:'1.96Y',c:'255258.SH',n:'24康居01',d:'Bid',p:'1.92',vol:'10000',pt:'到期收益率',nzVal:'102.45',dev:'-0.450',cs:'T+1',t:'招银理财',tr:'陈磊',time:'09:15',tags:['整']},
    {mode:'Nonfirm',rs:'已成交',ts:'已成交',m:'1.25Y',c:'032000764.IB',n:'20清池投资PPN001',d:'Ofr',p:'4.50',vol:'200',pt:'到期收益率',nzVal:'105.59',dev:'-0.211',cs:'T+1',t:'创金合信',tr:'丁茂国',time:'10:20',tags:['Firm']},
    {mode:'Firm',rs:'已成交',ts:'已成交',m:'4.92Y',c:'2021017.IB',n:'20青田农商二级01',d:'Bid',p:'1.95',vol:'2500',pt:'到期收益率',nzVal:'100.15',dev:'-0.120',cs:'T+1',t:'光大证券',tr:'刘明',time:'08:40',tags:[]},
    {mode:'Nonfirm',rs:'已成交',ts:'已成交',m:'8.13Y',c:'240210.IB',n:'24国开10',d:'Ofr',p:'1.95',vol:'3000',pt:'到期收益率',nzVal:'102.18',dev:'-1.160',cs:'T+1',t:'光大证券',tr:'刘明',time:'08:30',tags:['Firm']},
    {mode:'Firm',rs:'已成交',ts:'已成交',m:'1.61Y+N',c:'102480625.IB',n:'24晋能山西MTN002',d:'Bid',p:'2.2576',vol:'500',pt:'到期收益率',nzVal:'100.87',dev:'-0.670',cs:'T+1',t:'创金合信',tr:'丁茂国',time:'16:01',tags:['Firm']},
    {mode:'Nonfirm',rs:'已成交',ts:'已成交',m:'1Y',c:'102381799.IB',n:'23西咸新发MTN002',d:'Ofr',p:'2.00',vol:'1500',pt:'到期收益率',nzVal:'100.32',dev:'-0.300',cs:'T+1',t:'招银理财',tr:'陈磊',time:'08:10',tags:['急']},
    {mode:'Firm',rs:'已成交',ts:'已成交',m:'2.50Y',c:'252980.SH',n:'23大理债',d:'Bid',p:'3.80',vol:'1000',pt:'到期收益率',nzVal:'105.59',dev:'-1.079',cs:'T+1',t:'平安基金',tr:'赵明',time:'14:12',tags:['Firm']},
    {mode:'Nonfirm',rs:'已成交',ts:'已成交',m:'4.50Y',c:'102480625.IB',n:'24晋能山西MTN002',d:'Ofr',p:'2.28',vol:'500',pt:'到期收益率',nzVal:'100.87',dev:'-0.670',cs:'T+1',t:'创金合信',tr:'丁茂国',time:'13:57',tags:['荐']},
    {mode:'Firm',rs:'已成交',ts:'已成交',m:'37.17Y',c:'130010.IB',n:'13附息国债10',d:'Bid',p:'2.15',vol:'800',pt:'到期收益率',nzVal:'103.45',dev:'-0.180',cs:'T+1',t:'光大证券',tr:'刘明',time:'11:30',tags:['荐']},
    {mode:'Nonfirm',rs:'已成交',ts:'已成交',m:'1.13Y',c:'170010.IB',n:'17附息国债10',d:'Bid',p:'1.55',vol:'5000',pt:'到期收益率',nzVal:'99.82',dev:'0.820',cs:'T+0',t:'中信证券',tr:'张伟',time:'11:50',tags:['整']},
    {mode:'Firm',rs:'已成交',ts:'已成交',m:'0.75Y',c:'012582704.IB',n:'25南京铁建SCP004',d:'Bid',p:'1.58',vol:'1000',pt:'到期收益率',nzVal:'100.12',dev:'0.120',cs:'T+0',t:'平安基金',tr:'赵明',time:'09:30',tags:['Firm']},
    {mode:'Nonfirm',rs:'已成交',ts:'已成交',m:'6.25Y',c:'102580247.IB',n:'25浙交投MTN001',d:'Bid',p:'2.54',vol:'2000',pt:'到期收益率',nzVal:'101.31',dev:'-0.310',cs:'T+1',t:'招银理财',tr:'陈磊',time:'09:00',tags:['Firm','荐']},
    {mode:'Firm',rs:'已成交',ts:'已成交',m:'5.22Y',c:'210210.IB',n:'21国开10',d:'Bid',p:'1.90',vol:'1000',pt:'到期收益率',nzVal:'101.56',dev:'-0.330',cs:'T+1',t:'光大证券',tr:'刘明',time:'14:20',tags:['Firm']},
    {mode:'Nonfirm',rs:'已成交',ts:'部分成交',m:'1.96Y',c:'255258.SH',n:'24康居01',d:'Ofr',p:'1.95',vol:'5000',pt:'到期收益率',nzVal:'102.45',dev:'-0.420',cs:'T+1',t:'招银理财',tr:'陈磊',time:'10:00',tags:['急','整']},
    {mode:'Firm',rs:'已成交',ts:'已成交',m:'4.92Y',c:'2021017.IB',n:'20青田农商二级01',d:'Ofr',p:'1.98',vol:'3000',pt:'到期收益率',nzVal:'100.15',dev:'-0.090',cs:'T+1',t:'光大证券',tr:'刘明',time:'15:00',tags:['Firm']},
  ],
  tradePending:[
    {date:'06-17',pi:'国泰君安',ps:'陈彦霖',ci:'平安基金',cs:'丁茂国',c:'252980.SH',n:'23大理债',d:'Bid',p:'4.00',vol:'5000',s:'待确认',sc:'#d29922',tags:['Firm']},
    {date:'06-17',pi:'国泰君安',ps:'陈彦霖',ci:'中信证券',cs:'周明哲',c:'102001663.IB',n:'20南通城建MTN001',d:'Ofr',p:'2.00',vol:'4000',s:'待确认',sc:'#d29922',tags:['急']},
    {date:'06-17',pi:'国泰君安',ps:'林诗涵',ci:'创金合信',cs:'丁茂国',c:'072410238.IB',n:'24银河证券CP013',d:'Bid',p:'3.00',vol:'3000',s:'已确认',sc:'#3b82f6',tags:['荐']},
    {date:'06-17',pi:'国泰君安',ps:'陈彦霖',ci:'招银理财',cs:'陈磊',c:'102580247.IB',n:'25浙交投MTN001',d:'Bid',p:'2.54',vol:'2000',s:'待确认',sc:'#d29922',tags:['Firm']},
    {date:'06-16',pi:'国泰君安',ps:'林诗涵',ci:'光大证券',cs:'刘明',c:'210210.IB',n:'21国开10',d:'Bid',p:'1.90',vol:'1000',s:'已确认',sc:'#3b82f6',tags:[]},
    {date:'06-16',pi:'国泰君安',ps:'陈彦霖',ci:'平安基金',cs:'赵明',c:'188572.SH',n:'21柳发02',d:'Bid',p:'3.30',vol:'800',s:'待确认',sc:'#d29922',tags:['急']},
    {date:'06-16',pi:'国泰君安',ps:'陈彦霖',ci:'中信证券',cs:'张伟',c:'170010.IB',n:'17附息国债10',d:'Bid',p:'1.55',vol:'5000',s:'已确认',sc:'#3b82f6',tags:['整']},
    {date:'06-16',pi:'国泰君安',ps:'林诗涵',ci:'创金合信',cs:'丁茂国',c:'032000764.IB',n:'20清池投资PPN001',d:'Bid',p:'4.30',vol:'500',s:'待确认',sc:'#d29922',tags:['打包']},
    {date:'06-15',pi:'国泰君安',ps:'陈彦霖',ci:'招银理财',cs:'陈磊',c:'255258.SH',n:'24康居01',d:'Bid',p:'1.92',vol:'10000',s:'已确认',sc:'#3b82f6',tags:['Firm']},
    {date:'06-15',pi:'国泰君安',ps:'林诗涵',ci:'光大证券',cs:'刘明',c:'130010.IB',n:'13附息国债10',d:'Ofr',p:'2.15',vol:'800',s:'待确认',sc:'#d29922',tags:['荐']},
    {date:'06-15',pi:'国泰君安',ps:'陈彦霖',ci:'平安基金',cs:'赵明',c:'012582704.IB',n:'25南京铁建SCP004',d:'Bid',p:'1.58',vol:'1000',s:'已确认',sc:'#3b82f6',tags:[]},
    {date:'06-15',pi:'国泰君安',ps:'陈彦霖',ci:'中信证券',cs:'张伟',c:'102484688.IB',n:'25中鼎股份MTN001',d:'Bid',p:'1.88',vol:'4000',s:'待确认',sc:'#d29922',tags:['Firm']},
    {date:'06-14',pi:'国泰君安',ps:'林诗涵',ci:'创金合信',cs:'丁茂国',c:'252980.SH',n:'23大理债',d:'Ofr',p:'4.00',vol:'500',s:'已确认',sc:'#3b82f6',tags:['急']},
    {date:'06-14',pi:'国泰君安',ps:'陈彦霖',ci:'招银理财',cs:'陈磊',c:'102484124.IB',n:'24首开MTN006',d:'Bid',p:'2.93',vol:'1000',s:'待确认',sc:'#d29922',tags:['Firm']},
    {date:'06-14',pi:'国泰君安',ps:'林诗涵',ci:'光大证券',cs:'刘明',c:'032280666.IB',n:'22惠民城建PPN004',d:'Bid',p:'3.00',vol:'200',s:'已确认',sc:'#3b82f6',tags:['荐']},
    {date:'06-13',pi:'国泰君安',ps:'陈彦霖',ci:'平安基金',cs:'赵明',c:'240210.IB',n:'24国开10',d:'Bid',p:'1.86',vol:'2000',s:'待确认',sc:'#d29922',tags:['Firm']},
    {date:'06-13',pi:'国泰君安',ps:'林诗涵',ci:'中信证券',cs:'张伟',c:'2021017.IB',n:'20青田农商二级01',d:'Bid',p:'1.92',vol:'3000',s:'已确认',sc:'#3b82f6',tags:['整']},
    {date:'06-12',pi:'国泰君安',ps:'陈彦霖',ci:'创金合信',cs:'丁茂国',c:'280658.SH',n:'25溧水03',d:'Bid',p:'5.00',vol:'500',s:'待确认',sc:'#d29922',tags:['OCO']},
    {date:'06-12',pi:'国泰君安',ps:'林诗涵',ci:'招银理财',cs:'陈磊',c:'102381799.IB',n:'23西咸新发MTN002',d:'Bid',p:'1.9662',vol:'1500',s:'已确认',sc:'#3b82f6',tags:['Firm']},
    {date:'06-11',pi:'国泰君安',ps:'陈彦霖',ci:'光大证券',cs:'刘明',c:'210210.IB',n:'21国开10',d:'Ofr',p:'1.90',vol:'1000',s:'待确认',sc:'#d29922',tags:['急']},
    {date:'06-11',pi:'国泰君安',ps:'林诗涵',ci:'平安基金',cs:'赵明',c:'102480625.IB',n:'24晋能山西MTN002',d:'Bid',p:'2.26',vol:'1500',s:'已确认',sc:'#3b82f6',tags:['荐']},
    {date:'06-10',pi:'国泰君安',ps:'陈彦霖',ci:'中信证券',cs:'张伟',c:'252980.SH',n:'23大理债',d:'Bid',p:'3.85',vol:'2000',s:'待确认',sc:'#d29922',tags:['Firm']},
    {date:'06-10',pi:'国泰君安',ps:'林诗涵',ci:'创金合信',cs:'丁茂国',c:'188572.SH',n:'21柳发02',d:'Ofr',p:'3.50',vol:'500',s:'已确认',sc:'#3b82f6',tags:['打包']},
    {date:'06-09',pi:'国泰君安',ps:'陈彦霖',ci:'招银理财',cs:'陈磊',c:'032000764.IB',n:'20清池投资PPN001',d:'Bid',p:'4.30',vol:'500',s:'待确认',sc:'#d29922',tags:['急']},
  ],
  tradeDone:[
    {date:'06-17',pi:'国泰君安',ci:'创金合信',c:'072410238.IB',n:'24银河证券CP013',d:'Bid',p:'3.00',vol:'3000',cost:'12.5',profit:'3.2',time:'13:20',ts:'已成交',pkg:'PKG20260617001',book:'已推送'},
    {date:'06-16',pi:'国泰君安',ci:'平安基金',c:'252980.SH',n:'23大理债',d:'Ofr',p:'4.00',vol:'5000',cost:'8.3',profit:'5.1',time:'16:05',ts:'已成交',pkg:'PKG20260616003',book:'已推送'},
    {date:'06-16',pi:'国泰君安',ci:'中信证券',c:'170010.IB',n:'17附息国债10',d:'Bid',p:'1.55',vol:'5000',cost:'15.2',profit:'2.8',time:'14:30',ts:'已成交',pkg:'PKG20260616002',book:'已推送'},
    {date:'06-15',pi:'国泰君安',ci:'招银理财',c:'255258.SH',n:'24康居01',d:'Bid',p:'1.92',vol:'10000',cost:'22.0',profit:'4.5',time:'10:20',ts:'已成交',pkg:'PKG20260615001',book:'已推送'},
    {date:'06-15',pi:'国泰君安',ci:'平安基金',c:'188572.SH',n:'21柳发02',d:'Bid',p:'3.50',vol:'500',cost:'3.8',profit:'1.2',time:'15:45',ts:'已成交',pkg:'PKG20260615002',book:'已推送'},
    {date:'06-14',pi:'国泰君安',ci:'中信证券',c:'240210.IB',n:'24国开10',d:'Bid',p:'1.86',vol:'2000',cost:'6.5',profit:'2.0',time:'16:20',ts:'已成交',pkg:'PKG20260614001',book:'已推送'},
    {date:'06-14',pi:'国泰君安',ci:'创金合信',c:'102484688.IB',n:'25中鼎股份MTN001',d:'Bid',p:'1.88',vol:'4000',cost:'9.8',profit:'1.5',time:'11:30',ts:'已成交',pkg:'PKG20260614002',book:'已推送'},
    {date:'06-13',pi:'国泰君安',ci:'招银理财',c:'102484124.IB',n:'24首开MTN006',d:'Bid',p:'2.93',vol:'1000',cost:'4.2',profit:'0.8',time:'09:15',ts:'已成交',pkg:'PKG20260613001',book:'已推送'},
    {date:'06-13',pi:'国泰君安',ci:'光大证券',c:'210210.IB',n:'21国开10',d:'Bid',p:'1.90',vol:'1000',cost:'5.0',profit:'1.3',time:'14:50',ts:'已成交',pkg:'PKG20260613002',book:'已推送'},
    {date:'06-12',pi:'国泰君安',ci:'中信证券',c:'2021017.IB',n:'20青田农商二级01',d:'Bid',p:'1.95',vol:'2500',cost:'7.8',profit:'2.5',time:'10:40',ts:'已成交',pkg:'PKG20260612001',book:'已推送'},
    {date:'06-12',pi:'国泰君安',ci:'平安基金',c:'012582704.IB',n:'25南京铁建SCP004',d:'Bid',p:'1.58',vol:'1000',cost:'2.5',profit:'0.6',time:'09:30',ts:'已成交',pkg:'PKG20260612002',book:'已推送'},
    {date:'06-11',pi:'国泰君安',ci:'创金合信',c:'102580247.IB',n:'25浙交投MTN001',d:'Bid',p:'2.54',vol:'2000',cost:'6.0',profit:'1.8',time:'14:10',ts:'已成交',pkg:'PKG20260611001',book:'已推送'},
    {date:'06-11',pi:'国泰君安',ci:'招银理财',c:'102381799.IB',n:'23西咸新发MTN002',d:'Bid',p:'1.9662',vol:'1500',cost:'4.5',profit:'1.0',time:'08:50',ts:'已成交',pkg:'PKG20260611002',book:'已推送'},
    {date:'06-10',pi:'国泰君安',ci:'光大证券',c:'130010.IB',n:'13附息国债10',d:'Bid',p:'2.15',vol:'800',cost:'3.5',profit:'0.8',time:'11:30',ts:'已成交',pkg:'PKG20260610001',book:'已推送'},
    {date:'06-10',pi:'国泰君安',ci:'中信证券',c:'032280666.IB',n:'22惠民城建PPN004',d:'Bid',p:'3.00',vol:'200',cost:'1.2',profit:'0.3',time:'10:05',ts:'已成交',pkg:'PKG20260610002',book:'已推送'},
    {date:'06-09',pi:'国泰君安',ci:'平安基金',c:'102480625.IB',n:'24晋能山西MTN002',d:'Bid',p:'2.26',vol:'1500',cost:'5.2',profit:'1.5',time:'10:45',ts:'已成交',pkg:'PKG20260609001',book:'已推送'},
    {date:'06-09',pi:'国泰君安',ci:'创金合信',c:'032000764.IB',n:'20清池投资PPN001',d:'Ofr',p:'4.50',vol:'200',cost:'0.8',profit:'0.2',time:'10:20',ts:'已成交',pkg:'PKG20260609002',book:'已推送'},
    {date:'06-07',pi:'国泰君安',ci:'招银理财',c:'252980.SH',n:'23大理债',d:'Bid',p:'3.80',vol:'1000',cost:'3.5',profit:'1.0',time:'14:12',ts:'已成交',pkg:'PKG20260607001',book:'已推送'},
    {date:'06-07',pi:'国泰君安',ci:'中信证券',c:'240210.IB',n:'24国开10',d:'Ofr',p:'1.95',vol:'3000',cost:'8.0',profit:'2.2',time:'08:30',ts:'已成交',pkg:'PKG20260607002',book:'已推送'},
    {date:'06-06',pi:'国泰君安',ci:'光大证券',c:'210210.IB',n:'21国开10',d:'Bid',p:'1.90',vol:'1000',cost:'4.0',profit:'1.1',time:'14:20',ts:'已成交',pkg:'PKG20260606001',book:'已推送'},
    {date:'06-06',pi:'国泰君安',ci:'平安基金',c:'188572.SH',n:'21柳发02',d:'Bid',p:'3.30',vol:'800',cost:'2.8',profit:'0.8',time:'15:30',ts:'已成交',pkg:'PKG20260606002',book:'已推送'},
    {date:'06-05',pi:'国泰君安',ci:'创金合信',c:'102480625.IB',n:'24晋能山西MTN002',d:'Ofr',p:'2.28',vol:'500',cost:'2.0',profit:'0.5',time:'16:01',ts:'已成交',pkg:'PKG20260605001',book:'已推送'},
    {date:'06-05',pi:'国泰君安',ci:'招银理财',c:'252980.SH',n:'23大理债',d:'Bid',p:'3.85',vol:'2000',cost:'7.0',profit:'2.0',time:'12:40',ts:'已成交',pkg:'PKG20260605002',book:'已推送'},
    {date:'06-04',pi:'国泰君安',ci:'中信证券',c:'2021017.IB',n:'20青田农商二级01',d:'Bid',p:'1.92',vol:'3000',cost:'8.5',profit:'2.8',time:'15:13',ts:'已成交',pkg:'PKG20260604001',book:'已推送'},
  ],
  tradeCancel:[
    {date:'06-15',pi:'国泰君安',ci:'中信证券',c:'102001663.IB',n:'20南通城建MTN001',d:'Bid',p:'2.00',vol:'4000',reason:'对方取消'},
    {date:'06-14',pi:'国泰君安',ci:'平安基金',c:'280658.SH',n:'25溧水03',d:'Bid',p:'5.00',vol:'500',reason:'价格偏离过大'},
    {date:'06-13',pi:'国泰君安',ci:'光大证券',c:'130010.IB',n:'13附息国债10',d:'Ofr',p:'2.15',vol:'800',reason:'清算速度不符'},
    {date:'06-12',pi:'国泰君安',ci:'创金合信',c:'252980.SH',n:'23大理债',d:'Ofr',p:'4.00',vol:'500',reason:'量级不匹配'},
    {date:'06-11',pi:'国泰君安',ci:'招银理财',c:'102484124.IB',n:'24首开MTN006',d:'Bid',p:'2.93',vol:'1000',reason:'对方取消'},
    {date:'06-10',pi:'国泰君安',ci:'平安基金',c:'012582704.IB',n:'25南京铁建SCP004',d:'Bid',p:'1.58',vol:'1000',reason:'超时未确认'},
    {date:'06-09',pi:'国泰君安',ci:'中信证券',c:'032000764.IB',n:'20清池投资PPN001',d:'Bid',p:'4.30',vol:'500',reason:'对方取消'},
    {date:'06-07',pi:'国泰君安',ci:'光大证券',c:'210210.IB',n:'21国开10',d:'Bid',p:'1.90',vol:'1000',reason:'价格偏离过大'},
    {date:'06-06',pi:'国泰君安',ci:'创金合信',c:'032280666.IB',n:'22惠民城建PPN004',d:'Bid',p:'3.00',vol:'200',reason:'清算速度不符'},
    {date:'06-05',pi:'国泰君安',ci:'招银理财',c:'255258.SH',n:'24康居01',d:'Ofr',p:'1.95',vol:'5000',reason:'量级不匹配'},
    {date:'06-04',pi:'国泰君安',ci:'平安基金',c:'102480625.IB',n:'24晋能山西MTN002',d:'Bid',p:'2.26',vol:'1500',reason:'超时未确认'},
    {date:'06-03',pi:'国泰君安',ci:'中信证券',c:'240210.IB',n:'24国开10',d:'Bid',p:'1.86',vol:'2000',reason:'对方取消'},
    {date:'06-02',pi:'国泰君安',ci:'光大证券',c:'188572.SH',n:'21柳发02',d:'Ofr',p:'3.50',vol:'500',reason:'价格偏离过大'},
    {date:'06-01',pi:'国泰君安',ci:'创金合信',c:'170010.IB',n:'17附息国债10',d:'Bid',p:'1.55',vol:'5000',reason:'清算速度不符'},
    {date:'05-30',pi:'国泰君安',ci:'招银理财',c:'2021017.IB',n:'20青田农商二级01',d:'Bid',p:'1.92',vol:'3000',reason:'对方取消'},
    {date:'05-29',pi:'国泰君安',ci:'平安基金',c:'102580247.IB',n:'25浙交投MTN001',d:'Bid',p:'2.54',vol:'2000',reason:'量级不匹配'},
    {date:'05-28',pi:'国泰君安',ci:'中信证券',c:'102484688.IB',n:'25中鼎股份MTN001',d:'Bid',p:'1.88',vol:'4000',reason:'超时未确认'},
    {date:'05-27',pi:'国泰君安',ci:'光大证券',c:'032000764.IB',n:'20清池投资PPN001',d:'Bid',p:'4.30',vol:'500',reason:'价格偏离过大'},
    {date:'05-26',pi:'国泰君安',ci:'创金合信',c:'252980.SH',n:'23大理债',d:'Ofr',p:'4.00',vol:'500',reason:'对方取消'},
    {date:'05-25',pi:'国泰君安',ci:'招银理财',c:'210210.IB',n:'21国开10',d:'Bid',p:'1.90',vol:'1000',reason:'清算速度不符'},
    {date:'05-24',pi:'国泰君安',ci:'平安基金',c:'130010.IB',n:'13附息国债10',d:'Bid',p:'2.15',vol:'800',reason:'量级不匹配'},
    {date:'05-23',pi:'国泰君安',ci:'中信证券',c:'032280666.IB',n:'22惠民城建PPN004',d:'Bid',p:'3.00',vol:'200',reason:'对方取消'},
    {date:'05-22',pi:'国泰君安',ci:'光大证券',c:'102484124.IB',n:'24首开MTN006',d:'Bid',p:'2.93',vol:'1000',reason:'超时未确认'},
    {date:'05-21',pi:'国泰君安',ci:'创金合信',c:'255258.SH',n:'24康居01',d:'Bid',p:'1.92',vol:'10000',reason:'价格偏离过大'},
  ],
  tradePack:[
    {id:'PKG20260617001',pi:'国泰君安',ci:'创金合信',c:'072410238.IB',n:'24银河证券CP013',d:'Bid',p:'3.00',vol:'3000',s:'已成交'},
    {id:'PKG20260617002',pi:'国泰君安',ci:'平安基金',c:'252980.SH',n:'23大理债',d:'Bid',p:'4.00',vol:'5000',s:'待成交'},
    {id:'PKG20260616001',pi:'国泰君安',ci:'中信证券',c:'170010.IB',n:'17附息国债10',d:'Bid',p:'1.55',vol:'5000',s:'已成交'},
    {id:'PKG20260616002',pi:'国泰君安',ci:'招银理财',c:'255258.SH',n:'24康居01',d:'Bid',p:'1.92',vol:'10000',s:'已成交'},
    {id:'PKG20260616003',pi:'国泰君安',ci:'光大证券',c:'210210.IB',n:'21国开10',d:'Bid',p:'1.90',vol:'1000',s:'已成交'},
    {id:'PKG20260615001',pi:'国泰君安',ci:'创金合信',c:'102484688.IB',n:'25中鼎股份MTN001',d:'Bid',p:'1.88',vol:'4000',s:'已成交'},
    {id:'PKG20260615002',pi:'国泰君安',ci:'平安基金',c:'188572.SH',n:'21柳发02',d:'Bid',p:'3.50',vol:'500',s:'已成交'},
    {id:'PKG20260614001',pi:'国泰君安',ci:'中信证券',c:'240210.IB',n:'24国开10',d:'Bid',p:'1.86',vol:'2000',s:'已成交'},
    {id:'PKG20260614002',pi:'国泰君安',ci:'招银理财',c:'102484124.IB',n:'24首开MTN006',d:'Bid',p:'2.93',vol:'1000',s:'已成交'},
    {id:'PKG20260613001',pi:'国泰君安',ci:'光大证券',c:'2021017.IB',n:'20青田农商二级01',d:'Bid',p:'1.95',vol:'2500',s:'已成交'},
    {id:'PKG20260613002',pi:'国泰君安',ci:'创金合信',c:'032000764.IB',n:'20清池投资PPN001',d:'Bid',p:'4.30',vol:'500',s:'已成交'},
    {id:'PKG20260612001',pi:'国泰君安',ci:'平安基金',c:'012582704.IB',n:'25南京铁建SCP004',d:'Bid',p:'1.58',vol:'1000',s:'已成交'},
    {id:'PKG20260612002',pi:'国泰君安',ci:'中信证券',c:'130010.IB',n:'13附息国债10',d:'Bid',p:'2.15',vol:'800',s:'已成交'},
    {id:'PKG20260611001',pi:'国泰君安',ci:'招银理财',c:'102580247.IB',n:'25浙交投MTN001',d:'Bid',p:'2.54',vol:'2000',s:'已成交'},
    {id:'PKG20260611002',pi:'国泰君安',ci:'光大证券',c:'102381799.IB',n:'23西咸新发MTN002',d:'Bid',p:'1.9662',vol:'1500',s:'已成交'},
    {id:'PKG20260610001',pi:'国泰君安',ci:'创金合信',c:'252980.SH',n:'23大理债',d:'Bid',p:'3.80',vol:'1000',s:'已成交'},
    {id:'PKG20260610002',pi:'国泰君安',ci:'平安基金',c:'102480625.IB',n:'24晋能山西MTN002',d:'Bid',p:'2.26',vol:'1500',s:'已成交'},
    {id:'PKG20260609001',pi:'国泰君安',ci:'中信证券',c:'032280666.IB',n:'22惠民城建PPN004',d:'Bid',p:'3.00',vol:'200',s:'已成交'},
    {id:'PKG20260609002',pi:'国泰君安',ci:'招银理财',c:'240210.IB',n:'24国开10',d:'Bid',p:'1.86',vol:'2000',s:'已成交'},
    {id:'PKG20260608001',pi:'国泰君安',ci:'光大证券',c:'210210.IB',n:'21国开10',d:'Bid',p:'1.90',vol:'1000',s:'已成交'},
    {id:'PKG20260607001',pi:'国泰君安',ci:'创金合信',c:'188572.SH',n:'21柳发02',d:'Bid',p:'3.50',vol:'500',s:'已成交'},
    {id:'PKG20260607002',pi:'国泰君安',ci:'平安基金',c:'2021017.IB',n:'20青田农商二级01',d:'Bid',p:'1.92',vol:'3000',s:'已成交'},
  ],
  recommendationResults:[],
};

const BOND_FILTER_SCOPES={
  credit:{panelId:'filters-credit',baseProd:['信用债']},
  rate:{panelId:'filters-rate',baseProd:['利率债']},
  alert:{panelId:'filters-merged',baseProd:null},
  mgmt:{panelId:'filters-merged-mgmt',baseProd:null},
};
const MARKET_FILTER_GROUP_SECTIONS={
  credit:new Set(['债券筛选','流动性筛选','信用筛选','条款筛选','区域筛选']),
  rate:new Set(['债券筛选','流动性筛选','条款筛选','区域筛选']),
};
const RATE_FILTER_GROUP_EXCLUDED_KEYS=new Set(['lg_region','issuer_rating','market_maker','perp']);
const BOARD_VIEW_META={
  'board-credit-all':{subpageId:'st-board-credit-all',tbodyId:'boardCreditAllBody',field:'boardCreditAll',market:'credit',label:'信用全部',totalId:'creditAllTotal'},
  'board-credit-best':{subpageId:'st-board-credit-best',tbodyId:'boardCreditBestBody',field:'boardCreditBest',market:'credit',label:'信用最优',best:true,totalId:'creditBestTotal'},
  'board-rate-all':{subpageId:'st-board-rate-all',tbodyId:'boardRateAllBody',field:'boardRateAll',market:'rate',label:'利率全部',totalId:'rateAllTotal'},
  'board-rate-best':{subpageId:'st-board-rate-best',tbodyId:'boardRateBestBody',field:'boardRateBest',market:'rate',label:'利率最优',best:true,totalId:'rateBestTotal'},
  'board-alert':{subpageId:'st-board-alert',tbodyId:'boardAlertBody',field:'boardAlert',label:'报价提醒',totalId:'alertTotal'}
};

const BOARD_COL_STORAGE_KEY='ficc_board_column_order_v1';
const BOARD_COLUMN_DEFS={
  'board-credit-all':[
    {id:'m',label:'剩余期限',width:96,sticky:true,align:'left'},
    {id:'c',label:'代码',width:132,sticky:true,align:'left',type:'code'},
    {id:'n',label:'简称',width:190,sticky:true,align:'left'},
    {id:'direction',label:'方向',width:72,align:'left',render:'direction'},
    {id:'bp',label:'报价',width:150,align:'left',render:'quote'},
    {id:'bv',label:'报量(万)',width:82,align:'left',render:'volume'},
    {id:'inst',label:'报价机构',width:120},
    {id:'trader',label:'报价交易员',width:105},
    {id:'tr',label:'长城交易员',width:130},
    {id:'lastPrice',label:'最新成交价',width:92,render:'muted'},
    {id:'bondLib',label:'债券库',width:100},
    {id:'r',label:'主体评级',width:90},
    {id:'br',label:'债券评级',width:90},
    {id:'nzNet',label:'中债净价',width:105,render:'muted'},
    {id:'nzDev',label:'中债偏离',width:105,render:'nzDev'},
    {id:'nzYTM',label:'中债YTM(%)',width:105,render:'muted'},
    {id:'csNet',label:'中证净价',width:105,render:'muted'},
    {id:'csFull',label:'中证全价',width:105,render:'muted'},
    {id:'csYTM',label:'中证YTM(%)',width:105,render:'muted'},
    {id:'t',label:'报价时间',width:125,type:'muted'}
  ],
  'board-rate-all':null,
  'board-credit-best':[
    {id:'m',label:'剩余期限',width:96,sticky:true,align:'left'},
    {id:'c',label:'代码',width:132,sticky:true,align:'left',type:'code'},
    {id:'n',label:'简称',width:190,sticky:true,align:'left'},
    {id:'instBid',label:'报价机构.Bid',width:135},
    {id:'trBid',label:'长城交易员.Bid',width:120,render:'trBid'},
    {id:'bvBid',label:'Vol.Bid',width:100,align:'left',render:'volumeBid'},
    {id:'bp',label:'Bid',width:120,align:'left',render:'quoteBid',bg:'background:rgba(245,158,11,0.2)',headerClass:'bid-col'},
    {id:'op',label:'Ofr',width:120,align:'left',render:'quoteOfr',bg:'background:rgba(59,130,246,0.2)',headerClass:'ofr-col'},
    {id:'bvOfr',label:'Vol.Ofr',width:100,align:'left',render:'volumeOfr'},
    {id:'trOfr',label:'长城交易员.ofr',width:120,render:'trOfr'},
    {id:'instOfr',label:'报价机构.Ofr',width:135},
    {id:'lastPrice',label:'最新成交价',width:110,render:'muted'},
    {id:'bondLib',label:'债券库',width:100},
    {id:'r',label:'主体评级',width:90},
    {id:'br',label:'债券评级',width:90},
    {id:'nzNet',label:'中债净价',width:105,render:'muted'},
    {id:'nzDev',label:'中债偏离',width:105,render:'nzDev'},
    {id:'nzYTM',label:'中债YTM(%)',width:105,render:'muted'},
    {id:'csNet',label:'中证净价',width:105,render:'muted'},
    {id:'csFull',label:'中证全价',width:105,render:'muted'},
    {id:'csYTM',label:'中证YTM(%)',width:105,render:'muted'},
    {id:'t',label:'报价时间',width:170,type:'muted'}
  ],
  'board-rate-best':null,
  'board-alert':[
    {id:'alertType',label:'提醒类型',width:110,sticky:true,align:'left',render:'alertType'},
    {id:'m',label:'剩余期限',width:96,sticky:true,align:'left'},
    {id:'c',label:'代码',width:132,sticky:true,align:'left',type:'code'},
    {id:'n',label:'简称',width:190,sticky:true,align:'left'},
    {id:'instBid',label:'报价机构.Bid',width:135},
    {id:'trBid',label:'长城交易员.Bid',width:120,render:'trBid'},
    {id:'bvBid',label:'Vol.Bid',width:100,align:'left',render:'volumeBid'},
    {id:'bp',label:'Bid',width:120,align:'left',render:'quoteBid',bg:'background:rgba(245,158,11,0.2)',headerClass:'bid-col'},
    {id:'op',label:'Ofr',width:120,align:'left',render:'quoteOfr',bg:'background:rgba(59,130,246,0.2)',headerClass:'ofr-col'},
    {id:'bvOfr',label:'Vol.Ofr',width:100,align:'left',render:'volumeOfr'},
    {id:'trOfr',label:'销售.Ofr',width:120,render:'trOfr'},
    {id:'instOfr',label:'报价机构.Ofr',width:135},
    {id:'lastPrice',label:'最新成交价',width:110,render:'muted'},
    {id:'bondLib',label:'债券库',width:100},
    {id:'r',label:'主体评级',width:90},
    {id:'br',label:'债券评级',width:90},
    {id:'nzNet',label:'中债净价',width:105,render:'muted'},
    {id:'nzDev',label:'中债偏离',width:105,render:'nzDev'},
    {id:'nzYTM',label:'中债YTM(%)',width:105,render:'muted'},
    {id:'csNet',label:'中证净价',width:105,render:'muted'},
    {id:'csFull',label:'中证全价',width:105,render:'muted'},
    {id:'csYTM',label:'中证YTM(%)',width:105,render:'muted'},
    {id:'t',label:'报价时间',width:170,type:'muted'}
  ]
};
BOARD_COLUMN_DEFS['board-rate-all']=BOARD_COLUMN_DEFS['board-credit-all'].map(col=>({...col}));
BOARD_COLUMN_DEFS['board-rate-best']=BOARD_COLUMN_DEFS['board-credit-best'].map(col=>({...col}));
let boardColumnOrderState={};
try{boardColumnOrderState=JSON.parse(localStorage.getItem(BOARD_COL_STORAGE_KEY)||'{}')||{};}catch(e){boardColumnOrderState={};}
function boardColumnDefs(viewKey){return (BOARD_COLUMN_DEFS[viewKey]||[]).map(col=>({...col}));}
function boardFixedColumnIds(viewKey){return boardColumnDefs(viewKey).filter(col=>col.sticky).map(col=>col.id);}
function normalizeBoardColumnOrder(viewKey,order){
  const defs=boardColumnDefs(viewKey),defIds=defs.map(col=>col.id),fixedIds=boardFixedColumnIds(viewKey),fixedSet=new Set(fixedIds);
  const incoming=Array.isArray(order)?order.filter(id=>defIds.includes(id)):[];
  const movable=[];
  incoming.forEach(id=>{if(!fixedSet.has(id)&&!movable.includes(id))movable.push(id);});
  defIds.forEach(id=>{if(!fixedSet.has(id)&&!movable.includes(id))movable.push(id);});
  return fixedIds.concat(movable);
}
function getBoardColumnOrder(viewKey){
  const normalized=normalizeBoardColumnOrder(viewKey,boardColumnOrderState[viewKey]);
  boardColumnOrderState[viewKey]=normalized;
  return normalized.slice();
}
function saveBoardColumnOrder(viewKey,order){
  boardColumnOrderState[viewKey]=normalizeBoardColumnOrder(viewKey,order);
  try{localStorage.setItem(BOARD_COL_STORAGE_KEY,JSON.stringify(boardColumnOrderState));}catch(e){}
  return boardColumnOrderState[viewKey].slice();
}
function orderedBoardColumns(viewKey){
  const defs=boardColumnDefs(viewKey),map=Object.fromEntries(defs.map(col=>[col.id,col]));
  return getBoardColumnOrder(viewKey).map(id=>map[id]).filter(Boolean);
}
function boardColumnLeftMap(viewKey){
  const lefts={};let left=0;
  orderedBoardColumns(viewKey).forEach(col=>{if(col.sticky){lefts[col.id]=left;left+=col.width;}});
  return lefts;
}
function boardVolumeDisplay(value){
  const text=String(value==null?'':value).trim();
  if(!text)return'—';
  const number=Number(text.replace(/,/g,''));
  return Number.isFinite(number)&&number===0?'--':text;
}
function boardVolumeHtml(value){return `<span style="color:#22d3ee;font-weight:600">${escapeQuoteHtml(boardVolumeDisplay(value))}</span>`;}
function boardMutedHtml(value){return `<span class="muted">${value||'—'}</span>`;}
function boardNzDevHtml(r){const v=parseFloat(r.nzDev);return `<span style="color:${Math.abs(v)>16?'#ef4444':'#c9d1d9'}">${r.nzDev||'—'}</span>`;}
function boardAlertTypeHtml(r){const groups=(r.groupNames||[]).join('、');return `<span title="${escapeQuoteHtml(groups)}">${escapeQuoteHtml(r.alertType||'—')}</span>`;}
function boardColToRenderSpec(col){
  const spec={key:col.id,align:col.align,type:col.type,bg:col.bg,colId:col.id,sticky:!!col.sticky,width:col.width};
  if(col.render==='direction')spec.render=renderQuoteDirection;
  else if(col.render==='quote')spec.render=r=>renderBoardQuoteValue(r,null,'left');
  else if(col.render==='quoteBid')spec.render=r=>renderBoardQuoteValue(r,'bid','side');
  else if(col.render==='quoteOfr')spec.render=r=>renderBoardQuoteValue(r,'ofr','side');
  else if(col.render==='volume')spec.render=r=>boardVolumeHtml(r.bv);
  else if(col.render==='volumeBid')spec.render=r=>boardVolumeHtml(r.bvBid);
  else if(col.render==='volumeOfr')spec.render=r=>boardVolumeHtml(r.bvOfr);
  else if(col.render==='muted')spec.render=r=>boardMutedHtml(r[col.id]);
  else if(col.render==='nzDev')spec.render=boardNzDevHtml;
  else if(col.render==='trBid')spec.render=r=>boardMutedHtml(r.trBid||r.bidOwnerDisplayName||'');
  else if(col.render==='trOfr')spec.render=r=>boardMutedHtml(r.trOfr||r.ofrOwnerDisplayName||'');
  else if(col.render==='alertType')spec.render=boardAlertTypeHtml;
  return spec;
}
function boardRenderCols(viewKey){return orderedBoardColumns(viewKey).map(boardColToRenderSpec);}
function applyBoardTableLayout(viewKey){
  const meta=BOARD_VIEW_META[viewKey];if(!meta)return;
  const body=document.getElementById(meta.tbodyId),table=body&&body.closest('table');if(!table)return;
  const cols=orderedBoardColumns(viewKey),lefts=boardColumnLeftMap(viewKey);
  const total=cols.reduce((sum,col)=>sum+(col.width||0),0);
  table.style.width=total+'px';table.style.minWidth=total+'px';
  const headRow=table.tHead&&table.tHead.rows[0];if(!headRow)return;
  headRow.innerHTML=cols.map(col=>{
    const classes=['s'];
    if(col.headerClass)classes.push(col.headerClass);
    if(col.sticky)classes.push('board-col-sticky');
    if(!col.sticky)classes.push('board-col-draggable');
    const style=[`width:${col.width}px`,`min-width:${col.width}px`,`max-width:${col.width}px`];
    if(col.align)style.push(`text-align:${col.align}`);
    if(col.sticky)style.push(`left:${lefts[col.id]||0}px`);
    return `<th class="${classes.join(' ')}" data-col="${col.id}" data-view="${viewKey}" draggable="${col.sticky?'false':'true'}" style="${style.join(';')}">${col.label}</th>`;
  }).join('');
  bindBoardColumnDrag(table,viewKey);
}
let boardColDragState=null;
function clearBoardColumnDropMarks(table){if(!table)return;table.querySelectorAll('th.board-col-drop-before,th.board-col-drop-after,th.board-col-dragging').forEach(th=>{th.classList.remove('board-col-drop-before','board-col-drop-after','board-col-dragging');});}
function bindBoardColumnDrag(table,viewKey){
  if(!table||table.dataset.boardColDragBound==='1')return;
  table.dataset.boardColDragBound='1';
  table.addEventListener('dragstart',e=>{
    const th=e.target.closest('th[data-col]');if(!th||th.closest('table')!==table)return;
    if(th.getAttribute('draggable')!=='true'){e.preventDefault();return;}
    boardColDragState={viewKey,colId:th.getAttribute('data-col'),table};
    th.classList.add('board-col-dragging');
    try{e.dataTransfer.setData('text/plain',th.getAttribute('data-col'));e.dataTransfer.effectAllowed='move';}catch(err){}
  });
  table.addEventListener('dragover',e=>{
    if(!boardColDragState||boardColDragState.table!==table)return;
    const th=e.target.closest('th[data-col]');if(!th||th.closest('table')!==table)return;
    const targetId=th.getAttribute('data-col'),fixed=new Set(boardFixedColumnIds(viewKey));
    if(!targetId||fixed.has(targetId)||targetId===boardColDragState.colId)return;
    e.preventDefault();
    clearBoardColumnDropMarks(table);th.classList.add('board-col-dragging');
    const rect=th.getBoundingClientRect();
    const before=e.clientX<rect.left+rect.width/2;
    th.classList.add(before?'board-col-drop-before':'board-col-drop-after');
    boardColDragState.before=before;boardColDragState.targetId=targetId;
  });
  table.addEventListener('drop',e=>{
    if(!boardColDragState||boardColDragState.table!==table)return;
    e.preventDefault();
    const fromId=boardColDragState.colId,targetId=boardColDragState.targetId,before=boardColDragState.before;
    clearBoardColumnDropMarks(table);boardColDragState=null;
    if(!fromId||!targetId||fromId===targetId)return;
    const fixed=new Set(boardFixedColumnIds(viewKey));if(fixed.has(fromId)||fixed.has(targetId))return;
    const order=getBoardColumnOrder(viewKey).filter(id=>id!==fromId);
    let insertAt=order.indexOf(targetId);if(insertAt<0)return;if(!before)insertAt+=1;
    order.splice(insertAt,0,fromId);
    saveBoardColumnOrder(viewKey,order);
    applyBoardTableLayout(viewKey);
    renderAll();
  });
  table.addEventListener('dragend',()=>{clearBoardColumnDropMarks(table);boardColDragState=null;});
}
const boardToolbarState=Object.fromEntries(Object.keys(BOARD_VIEW_META).map(key=>[key,{direction:'',mine:false,urgent:false,recommend:false,round:false,unquoted:false}]));
const boardUnquotedState={
  credit:{rows:[],total:0,page:1,pageSize:200,loading:false,error:'',requestId:0},
  rate:{rows:[],total:0,page:1,pageSize:200,loading:false,error:'',requestId:0}
};
const boardSelectedQuoteIds=new Set();
// 行情中心选中状态：auto 表示系统进入页面后的首条选中，manual 表示用户点击后的冻结态。
let boardSelectionMode='none';
let boardPendingNewQuoteCount=0;
let boardFrozenProjection=null;
let boardAutoSelectSuppressed=false;
const intentSelectedIds=new Set();
const QUOTE_FILTER_LABELS={'清算速度':'settlement','计价方式':'priceType'};
const QUOTE_RANGE_LABELS={'Bid':'bid','Ofr':'ofr','Bid-中债':'bidDev','中债-Ofr':'ofrDev','Bid-Ofr':'spread','Vol.Bid':'volBid','Vol.Ofr':'volOfr'};
const bondFilterRuntime=Object.fromEntries(Object.keys(BOND_FILTER_SCOPES).map(scope=>[scope,{bondIds:null,total:null,loading:false,error:'',requestId:0,controller:null,timer:null}]));
const appliedQuoteFilterState=Object.create(null);
const bondFilterCache=new Map();
const BOND_FILTER_CACHE_TTL=5*60*1000;
const bondFilterCapabilities={byLabel:new Map(),contractVersion:'',profileAsOf:null,dataSource:'',mockEnabled:null,error:'',lastError:''};
let bondFilterConfigLoadPromise=null,bondFilterRecoveryTimer=null;
const chinaRegionsByCode=new Map();
const activeMarketGroupIds={favorite:new Set(),filter:new Set()};
const marketGroupMatchIds=new Map();
const marketGroupQueryErrors=new Map();
// P2: 缓存时间戳与过期机制
const marketGroupCacheTime=new Map();
const marketGroupAbortControllers=new Map();
const MARKET_GROUP_CACHE_TTL=5*60*1000; // 5分钟过期
let marketGroupQueryGeneration=0;

function activeBoardViewKey(){
  const tab=document.querySelector('#subtabs-board .subtab.active');
  const key=tab&&tab.dataset.st;
  return BOARD_VIEW_META[key]?key:'board-credit-all';
}
function boardViewKeyForElement(element){
  const subpage=element&&element.closest('.subpage');
  return Object.keys(BOARD_VIEW_META).find(key=>BOARD_VIEW_META[key].subpageId===(subpage&&subpage.id))||activeBoardViewKey();
}
function clearBoardSelection(){boardSelectedQuoteIds.clear();}
function captureBoardProjection(){
  return {boardCreditAll:(D.boardCreditAll||[]).slice(),boardCreditBest:(D.boardCreditBest||[]).slice(),boardRateAll:(D.boardRateAll||[]).slice(),boardRateBest:(D.boardRateBest||[]).slice(),boardAlert:(D.boardAlert||[]).slice()};
}
function restoreBoardProjection(snapshot){
  if(!snapshot)return;
  Object.keys(snapshot).forEach(key=>{D[key]=snapshot[key].slice();});
}
function activeBoardRows(){
  const meta=BOARD_VIEW_META[activeBoardViewKey()];
  return meta?(D[meta.field]||[]):[];
}
function setBoardAutoSelection(options){
  options=options||{};
  if(boardSelectionMode==='manual'||boardAutoSelectSuppressed)return;
  const rows=activeBoardRows().filter(row=>!row.unquoted);
  clearBoardSelection();
  const first=rows[0];
  if(first){
    setBoardRowSelected(first,true);
    boardSelectionMode='auto';
    setRecommendationTarget(first,null,BOARD_VIEW_META[activeBoardViewKey()].tbodyId);
  }else boardSelectionMode='none';
}
function updateBoardNewQuoteBanner(){
  const banner=document.getElementById('boardNewQuoteBanner');
  if(!banner)return;
  const count=Number(boardPendingNewQuoteCount||0);
  banner.textContent='有 '+count+' 条新报价，点击刷新';
  banner.style.display=boardSelectionMode==='manual'&&count>0?'flex':'none';
}
async function refreshFrozenBoardProjection(){
  boardSelectionMode='none';boardPendingNewQuoteCount=0;boardFrozenProjection=null;boardAutoSelectSuppressed=true;clearBoardSelection();updateBoardNewQuoteBanner();
  try{await loadQuoteState({forceBoardRefresh:true});}finally{boardAutoSelectSuppressed=false;updateBoardNewQuoteBanner();}
}
function quoteIdsForBoardRow(row){
  if(!row||row.unquoted)return[];
  return[row.bidQuoteId,row.ofrQuoteId,row.sourceQuoteId,row.quoteId].filter(Boolean).filter((value,index,list)=>list.indexOf(value)===index);
}
function setBoardRowSelected(row,selected){
  quoteIdsForBoardRow(row).forEach(quoteId=>selected?boardSelectedQuoteIds.add(quoteId):boardSelectedQuoteIds.delete(quoteId));
}
function selectedBoardQuotes(key){
  const meta=BOARD_VIEW_META[key||activeBoardViewKey()],rows=meta?(D[meta.field]||[]):[],result=[],seen=new Set();
  rows.forEach(row=>quoteIdsForBoardRow(row).forEach(quoteId=>{
    if(!boardSelectedQuoteIds.has(quoteId)||seen.has(quoteId))return;
    const quote=quoteById(quoteId);if(quote){seen.add(quoteId);result.push(quote);}
  }));
  return result;
}
function applyBoardToolbarFilters(rows,key){
  const state=boardToolbarState[key]||{},source=rows||[];
  return source.filter(row=>{
    const direction=quoteDirectionOf(row);
    if(state.direction&&direction!==state.direction)return false;
    if(state.mine&&row.ownerAccountId!==currentAccountId())return false;
    if(state.urgent&&!row.urgent)return false;
    if(state.recommend&&!(row.recommend||row.rec))return false;
    if(state.round&&!(row.round||row.whole))return false;
    return true;
  });
}
function resetBoardToolbarState(key){
  if(!boardToolbarState[key])return;
  Object.assign(boardToolbarState[key],{direction:'',mine:false,urgent:false,recommend:false,round:false,unquoted:false});
}
function syncBoardToolbarControls(){
  Object.entries(BOARD_VIEW_META).forEach(([key,meta])=>{
    const page=document.getElementById(meta.subpageId),state=boardToolbarState[key];if(!page||!state)return;
    page.querySelectorAll('.toolbar .filter-tag').forEach(tag=>{
      const direction=tag.textContent.trim().toLowerCase();
      tag.classList.toggle('active-'+direction,state.direction===direction);
    });
    page.querySelectorAll('.toolbar .tb-checkbox').forEach(label=>{
      const text=label.textContent.trim(),input=label.querySelector('input');if(!input)return;
      const map={'我的报价':'mine','紧急':'urgent','推荐':'recommend','整量':'round','未报价债券':'unquoted'},field=map[text];
      if(field)input.checked=!!state[field];
      const quoteOnly=field&&field!=='unquoted';
      input.disabled=!!(state.unquoted&&quoteOnly);
      label.style.opacity=input.disabled?'.45':'';
    });
    page.querySelectorAll('[data-board-action]').forEach(button=>{
      button.disabled=!!(state.unquoted&&button.dataset.boardAction!=='export');
      button.title=button.disabled?'未报价债券不是报价，当前操作不可用':'';
    });
  });
}
function setBoardFlag(key,field,checked){
  const state=boardToolbarState[key];if(!state)return;
  state[field]=!!checked;clearBoardSelection();renderFilteredViews();syncBoardToolbarControls();
}
async function setBoardUnquoted(key,checked){
  const meta=BOARD_VIEW_META[key],state=boardToolbarState[key];if(!meta||meta.best||!state)return;
  state.unquoted=!!checked;
  state.direction='';state.mine=false;state.urgent=false;state.recommend=false;state.round=false;
  setUnquotedQuoteControls(meta.market,state.unquoted);
  clearBoardSelection();syncBoardToolbarControls();
  if(state.unquoted)await loadUnquotedBonds(meta.market,1);
  else renderFilteredViews();
}
function setUnquotedQuoteControls(market,disabled){
  const panel=document.getElementById(BOND_FILTER_SCOPES[market].panelId);if(!panel)return;
  panel.querySelectorAll('[data-quote-filter-key],[data-quote-range-key]').forEach(group=>{
    if(disabled){group.querySelectorAll('.filter-tag.active').forEach(tag=>tag.classList.remove('active'));group.querySelectorAll('input').forEach(input=>input.value='');group.querySelectorAll('select').forEach(select=>select.selectedIndex=0);}
    group.querySelectorAll('input,select,button').forEach(control=>control.disabled=disabled);group.style.opacity=disabled?'.45':'';
  });
}
function initBoardActionToolbars(){
  Object.entries(BOARD_VIEW_META).forEach(([key,meta])=>{
    const page=document.getElementById(meta.subpageId),toolbar=page&&page.querySelector('.toolbar');if(!toolbar||toolbar.dataset.actionsReady==='1')return;
    const buttons=toolbar.querySelectorAll('button.tb-btn');
    if(buttons[0]){buttons[0].dataset.boardAction='copy';buttons[0].onclick=()=>copySelectedBoardQuotes(key);}
    if(buttons[1]){buttons[1].dataset.boardAction='refer';buttons[1].onclick=()=>referSelectedBoardQuotes(key);}
    if(buttons[2]){buttons[2].dataset.boardAction='export';buttons[2].textContent='导出';buttons[2].onclick=()=>exportBoardView(key);}
    toolbar.querySelectorAll('.tb-checkbox').forEach(label=>{
      const text=label.textContent.trim(),input=label.querySelector('input');if(!input)return;
      const map={'我的报价':'mine','紧急':'urgent','推荐':'recommend','整量':'round','未报价债券':'unquoted'},field=map[text];if(!field)return;
      input.onchange=()=>field==='unquoted'?setBoardUnquoted(key,input.checked):setBoardFlag(key,field,input.checked);
    });
    toolbar.dataset.actionsReady='1';
  });
  syncBoardToolbarControls();
}
function mapUnquotedBond(bond,market){
  const code=String(bond.security_id||'').trim();
  return{unquoted:true,market,c:code,n:bond.sec_short_name||bond.sec_full_name||'',m:bond.remaining_tenor||'',direction:'',bp:'',op:'',bv:'',volumeUnit:'',inst:'',trader:'',tr:'',
    lastPrice:'',bondLib:'在库',r:bond.issuer_rating||bond.com_rating||'—',br:bond.bond_rating||'—',
    nzNet:bond.cb_net_price||bond.china_bond_net_price||'—',nzDev:'—',nzYTM:bond.cb_ytm||bond.china_bond_ytm||'—',
    csNet:bond.cs_net_price||'—',csFull:bond.cs_full_price||'—',csYTM:bond.cs_ytm||'—',t:'',_bondData:bond};
}
function activeMarketGroupSecurityIds(market){
  let allowed=null;
  const favorites=(market==='rate'?D.favBondGroupsRate:D.favBondGroupsCredit).filter(group=>activeMarketGroupIds.favorite.has(group.id));
  if(favorites.length){const union=new Set();favorites.forEach(group=>(group.bonds||[]).forEach(bond=>{const code=normalizeBondCode(_fbmBondCode(bond));if(code)union.add(code);}));allowed=union;}
  const filters=(market==='rate'?D.filterGroupsRate:D.filterGroupsCredit).filter(group=>activeMarketGroupIds.filter.has(group.id));
  if(filters.length){const union=new Set();filters.forEach(group=>(marketGroupMatchIds.get(group.id)||new Set()).forEach(code=>union.add(normalizeBondCode(code))));allowed=allowed===null?union:new Set([...allowed].filter(code=>union.has(code)));}
  return allowed===null?null:Array.from(allowed);
}
function unquotedRequestPayload(market,page,pageSize){
  const scope=market,local=collectLocalFilterState(scope);
  return{market,filters:collectBondFilters(scope),search:local.search||'',include_security_ids:activeMarketGroupSecurityIds(market),page:page||1,page_size:pageSize||boardUnquotedState[market].pageSize};
}
async function fetchUnquotedPage(market,page,pageSize){
  const response=await authFetch(API_BASE+'/bond-filter/unquoted',{method:'POST',body:JSON.stringify(unquotedRequestPayload(market,page,pageSize))});
  let data={};try{data=await response.json();}catch(e){}
  if(!response.ok){const detail=data.detail;throw new Error(typeof detail==='string'?detail:(detail&&detail.message)||'未报价债券查询失败');}
  return data;
}
async function loadUnquotedBonds(market,page){
  const state=boardUnquotedState[market],key='board-'+market+'-all';if(!state||!boardToolbarState[key].unquoted)return;
  const requestId=++state.requestId;state.loading=true;state.error='';state.rows=[];renderFilteredViews();syncBoardToolbarControls();
  try{
    const data=await fetchUnquotedPage(market,page||1,state.pageSize);if(requestId!==state.requestId)return;
    state.page=Number(data.page)||1;state.total=Number(data.total)||0;state.rows=(data.bonds||[]).map(bond=>mapUnquotedBond(bond,market));
  }catch(error){
    if(requestId!==state.requestId)return;
    state.error=error.message||'未报价债券查询失败';showToast('error',state.error);
  }finally{
    if(requestId===state.requestId){state.loading=false;renderFilteredViews();syncBoardToolbarControls();}
  }
}
function changeUnquotedPage(market,delta){
  const state=boardUnquotedState[market],pages=Math.max(1,Math.ceil(state.total/state.pageSize)),next=Math.max(1,Math.min(pages,state.page+delta));
  if(next!==state.page)loadUnquotedBonds(market,next);
}
function updateUnquotedStatusbars(){
  ['credit','rate'].forEach(market=>{
    const key='board-'+market+'-all',meta=BOARD_VIEW_META[key],state=boardUnquotedState[market],page=document.getElementById(meta.subpageId);
    if(!page)return;
    const bar=page.querySelector('.statusbar'),existing=bar&&bar.querySelector('.unquoted-pager');if(existing)existing.remove();
    if(!boardToolbarState[key].unquoted)return;
    const total=document.getElementById(meta.totalId);if(total)total.textContent=state.total;
    const pages=Math.max(1,Math.ceil(state.total/state.pageSize)),pager=document.createElement('span');pager.className='unquoted-pager';
    pager.style.cssText='margin-left:auto;display:flex;align-items:center;gap:6px';
    pager.innerHTML='<span>'+(state.loading?'加载中...':'未报价模式')+'</span><button class="tb-btn tb-btn-default" '+(state.loading||state.page<=1?'disabled':'')+' onclick="changeUnquotedPage(\''+market+'\',-1)">上一页</button><span>'+state.page+' / '+pages+'</span><button class="tb-btn tb-btn-default" '+(state.loading||state.page>=pages?'disabled':'')+' onclick="changeUnquotedPage(\''+market+'\',1)">下一页</button>';
    if(bar)bar.appendChild(pager);
  });
}

function marketGroupCollections(){
  return[
    {kind:'favorite',market:'credit',field:'favBondGroupsCredit'},
    {kind:'favorite',market:'rate',field:'favBondGroupsRate'},
    {kind:'filter',market:'credit',field:'filterGroupsCredit'},
    {kind:'filter',market:'rate',field:'filterGroupsRate'},
  ];
}
function allMarketGroups(){
  return marketGroupCollections().flatMap(config=>(D[config.field]||[]).map(group=>({...group,kind:config.kind,market:config.market})));
}
function marketGroupById(groupId){
  return allMarketGroups().find(group=>group.id===groupId)||null;
}
function newLocalMarketGroupId(){
  return'local-'+Date.now().toString(36)+'-'+Math.random().toString(36).slice(2,10);
}
function normalizedMarketCondition(condition){
  if(condition&&typeof condition==='object'&&condition.key&&condition.label&&condition.value!==undefined){
    return{key:String(condition.key),label:String(condition.label),value:condition.value};
  }
  const raw=String(condition||''),separator=raw.indexOf(':');
  if(separator<=0)return null;
  const label=raw.slice(0,separator).trim(),value=raw.slice(separator+1).trim();
  const capability=bondFilterCapabilities.byLabel.get(label);
  if(!capability||capability.enabled!==true)return null;
  return{key:capability.key,label,value:normalizeBondFilterValue(capability.key,value)};
}
const marketFilterPanelLabelCache=new Map();
const marketFilterCatalogCache=new Map();
function marketFilterPanelLabels(market){
  if(marketFilterPanelLabelCache.has(market))return marketFilterPanelLabelCache.get(market);
  const config=BOND_FILTER_SCOPES[market],labels=new Set();
  if(typeof document==='undefined'||typeof document.getElementById!=='function')return labels;
  const panel=config&&document.getElementById(config.panelId);
  if(!panel)return labels;
  const sections=MARKET_FILTER_GROUP_SECTIONS[market]||new Set();
  panel.querySelectorAll(':scope > .filter-section').forEach(section=>{
    const title=section.querySelector(':scope > .filter-section-title');
    if(!title||!sections.has(title.textContent.trim()))return;
    section.querySelectorAll(':scope > .filter-group').forEach(group=>{
      const label=group.querySelector(':scope > .filter-label');
      if(label)labels.add(label.textContent.trim());
    });
  });
  marketFilterPanelLabelCache.set(market,labels);return labels;
}
function marketConditionAllowed(market,condition){
  const normalized=normalizedMarketCondition(condition);
  if(!normalized)return false;
  const capability=bondFilterCapabilities.byLabel.get(normalized.label);
  if(bondFilterCapabilities.byLabel.size&&( !capability||capability.enabled!==true||capability.key!==normalized.key))return false;
  if(market==='rate'&&RATE_FILTER_GROUP_EXCLUDED_KEYS.has(normalized.key))return false;
  const labels=marketFilterPanelLabels(market);
  return labels.size===0||labels.has(normalized.label);
}
// 筛选分组子分组工具：subgroups 为二维数组 [[条件...]...]，conditions 为拍平视图（查询与旧逻辑兼容）
function marketGroupSubgroupCount(raw){return Number.isFinite(Number(raw&&raw.subgroup))?Number(raw.subgroup):0;}
function normalizeMarketGroupSubgroups(raw){
  if(Array.isArray(raw&&raw.subgroups)){
    return raw.subgroups.map(list=>Array.isArray(list)?list:[]);
  }
  const conditions=Array.isArray(raw&&raw.conditions)?raw.conditions:[];
  if(conditions.some(condition=>condition&&condition.subgroup!=null)){
    const buckets=new Map();
    conditions.forEach(condition=>{
      const index=Number.isFinite(Number(condition&&condition.subgroup))?Math.max(0,Number(condition.subgroup)):0;
      if(!buckets.has(index))buckets.set(index,[]);
      buckets.get(index).push(condition);
    });
    return [...buckets.entries()].sort((a,b)=>a[0]-b[0]).map(([,list])=>list);
  }
  return [conditions];
}
function flattenMarketSubgroups(subgroups){
  return (Array.isArray(subgroups)?subgroups:[]).reduce((acc,list)=>acc.concat(Array.isArray(list)?list:[]),[]);
}
function normalizedRemoteMarketGroup(raw){
  const kind=raw&&raw.kind==='filter'?'filter':'favorite';
  const market=raw&&raw.market==='rate'?'rate':'credit';
  const group={
    id:String(raw&&raw.id||newLocalMarketGroupId()),
    name:String(raw&&raw.name||'未命名').trim().slice(0,30)||'未命名',
  };
  if(kind==='favorite'){
    const seen=new Set();
    group.bonds=(Array.isArray(raw&&raw.bonds)?raw.bonds:[]).map(bond=>{
      const code=normalizeBondCode(typeof bond==='string'?bond:bond&&bond.code);
      return{code,name:String(typeof bond==='object'&&bond&&bond.name||'')};
    }).filter(bond=>bond.code&&!seen.has(bond.code)&&seen.add(bond.code));
  }else{
    const subgroups=normalizeMarketGroupSubgroups(raw).map(list=>{
      const seen=new Set();
      return (Array.isArray(list)?list:[]).map(normalizedMarketCondition).filter(condition=>{
        if(!condition||!marketConditionAllowed(market,condition))return false;
        const identity=condition.key+'|'+JSON.stringify(condition.value);
        return!seen.has(identity)&&seen.add(identity);
      });
    }).filter(list=>list.length>0);
    group.subgroups=subgroups.length?subgroups:[[]];
    group.conditions=flattenMarketSubgroups(group.subgroups);
  }
  return{...group,kind,market};
}
function applyMarketGroupState(data){
  marketGroupCollections().forEach(config=>{D[config.field]=[];});
  (Array.isArray(data&&data.groups)?data.groups:[]).forEach(raw=>{
    const group=normalizedRemoteMarketGroup(raw);
    const config=marketGroupCollections().find(item=>item.kind===group.kind&&item.market===group.market);
    if(config)D[config.field].push(group);
  });
  D.marketGroupRevision=Number(data&&data.revision)||0;
  D.marketGroupLoaded=true;D.marketGroupReadOnly=false;
  D.alertSettings=Object.assign({favorite:true,filterGroup:true,priceMatch:true,bestQuote:true},data&&data.alertSettings||{});
  const validIds=new Set(allMarketGroups().map(group=>group.id));
  Object.values(activeMarketGroupIds).forEach(ids=>Array.from(ids).forEach(id=>{if(!validIds.has(id))ids.delete(id);}));
  D.favBonds=[...new Set(D.favBondGroupsCredit.concat(D.favBondGroupsRate).flatMap(group=>(group.bonds||[]).map(_fbmBondCode)))];
}
function marketGroupSavePayload(){
  return allMarketGroups().map(group=>({
    id:group.id&&group.id.startsWith('local-')?null:group.id,
    kind:group.kind,
    market:group.market,
    name:group.name,
    bonds:group.kind==='favorite'?(group.bonds||[]).map(bond=>({code:_fbmBondCode(bond),name:_fbmBondName(bond)})):[],
    conditions:group.kind==='filter'?normalizeMarketGroupSubgroups(group).flatMap((list,subgroup)=>list.map(normalizedMarketCondition).filter(condition=>condition&&marketConditionAllowed(group.market,condition)).map(condition=>({...condition,subgroup}))):[],
  }));
}
async function loadMarketGroupState(){
  if(!authToken){D.marketGroupLoaded=false;D.marketGroupReadOnly=true;return false;}
  const response=await authFetch(API_BASE+'/market-groups');
  let data={};try{data=await response.json();}catch(error){}
  if(!response.ok||!data.success)throw new Error(data.detail&&data.detail.message||data.detail||'分组加载失败');
  applyMarketGroupState(data);
  // P0: 初始加载不预查筛选，点击分组时再查
  return true;
}
async function saveMarketGroupState(){
  if(!authToken||!D.marketGroupLoaded||D.marketGroupReadOnly)throw new Error('当前分组为只读状态，请重新登录后操作');
  const response=await authFetch(API_BASE+'/market-groups',{method:'PUT',body:JSON.stringify({
    groups:marketGroupSavePayload(),
    alert_settings:D.alertSettings,
    expected_revision:D.marketGroupRevision,
  })});
  let data={};try{data=await response.json();}catch(error){}
  if(!response.ok){
    const detail=data.detail;
    if(response.status===409&&detail&&detail.code==='MARKET_GROUP_REVISION_CONFLICT'){
      await loadMarketGroupState();renderFilteredViews();
    }
    throw new Error(detail&&detail.message||detail||'分组保存失败');
  }
  applyMarketGroupState(data);
  // P0: 保存与重查解耦 —— 不再在保存路径 await 筛选重查
  return data;
}
function hasLegacyMarketGroups(){
  return Object.values(legacyMarketGroupState).some(groups=>Array.isArray(groups)&&groups.length);
}
function legacyGroupsForMigration(){
  const specs=[
    ['favorite','credit',legacyMarketGroupState.favoriteCredit],
    ['favorite','rate',legacyMarketGroupState.favoriteRate],
    ['filter','credit',legacyMarketGroupState.filterCredit],
    ['filter','rate',legacyMarketGroupState.filterRate],
  ];
  return specs.flatMap(([kind,market,groups])=>(groups||[]).map(group=>normalizedRemoteMarketGroup({...group,kind,market,id:newLocalMarketGroupId()})));
}
function maybeOfferLegacyMarketGroupImport(){
  if(!D.marketGroupLoaded||allMarketGroups().length||!hasLegacyMarketGroups()||!currentUser)return;
  const dismissedKey='ficc_marketGroupMigrationDismissed:'+currentAccountId();
  if(localStorage.getItem(dismissedKey)==='1')return;
  showModal('导入本机旧分组','<div style="line-height:1.7">检测到旧版本保存在本机的关注债券和筛选分组。旧数据无法判断原所属账户，是否导入到当前账户？</div>',{
    footerHtml:'<button class="tb-btn tb-btn-default" onclick="dismissLegacyMarketGroupImport()">暂不导入</button><button class="tb-btn tb-btn-primary" onclick="importLegacyMarketGroups()">导入当前账户</button>'
  });
}
function dismissLegacyMarketGroupImport(){
  localStorage.setItem('ficc_marketGroupMigrationDismissed:'+currentAccountId(),'1');closeModal();
}
async function importLegacyMarketGroups(){
  const previous=allMarketGroups();
  marketGroupCollections().forEach(config=>{D[config.field]=[];});
  legacyGroupsForMigration().forEach(group=>{
    const config=marketGroupCollections().find(item=>item.kind===group.kind&&item.market===group.market);
    if(config)D[config.field].push(group);
  });
  try{
    await saveMarketGroupState();
    ['ficc_favBondGroupsCredit','ficc_favBondGroupsRate','ficc_filterGroupsCredit','ficc_filterGroupsRate','ficc_favBonds'].forEach(key=>localStorage.removeItem(key));
    closeModal();renderFilteredViews();showToast('success','旧分组已导入当前账户');
  }catch(error){
    applyMarketGroupState({revision:D.marketGroupRevision,groups:previous,alertSettings:D.alertSettings});
    showToast('error',error.message||'旧分组导入失败');
  }
}
function marketGroupFilterPayload(group){
  const filters={};
  flattenMarketSubgroups(normalizeMarketGroupSubgroups(group)).map(normalizedMarketCondition).filter(condition=>condition&&marketConditionAllowed(group.market,condition)).forEach(condition=>{
    filters[condition.key]=filters[condition.key]||[];
    if(!filters[condition.key].some(value=>JSON.stringify(value)===JSON.stringify(condition.value)))filters[condition.key].push(condition.value);
  });
  if(Object.keys(filters).length)filters.prod=[group.market==='rate'?'利率债':'信用债'];
  return filters;
}
async function queryMarketFilterGroup(group,generation,signal){
  const filters=marketGroupFilterPayload(group);
  if(!Object.keys(filters).length){marketGroupMatchIds.set(group.id,new Set());marketGroupQueryErrors.delete(group.id);marketGroupCacheTime.set(group.id,Date.now());return;}
  try{
    const response=await authFetch(API_BASE+'/bond-filter/query',{method:'POST',body:JSON.stringify({filters}),signal});
    let data={};try{data=await response.json();}catch(error){}
    if(!response.ok||!Array.isArray(data.security_ids))throw new Error(data.detail&&data.detail.message||data.detail||'筛选分组查询失败');
    if(generation!==marketGroupQueryGeneration)return;
    marketGroupMatchIds.set(group.id,new Set(data.security_ids.map(normalizeBondCode).filter(Boolean)));
    marketGroupQueryErrors.delete(group.id);
    marketGroupCacheTime.set(group.id,Date.now());
  }catch(error){
    if(error&&error.name==='AbortError')return;
    if(generation!==marketGroupQueryGeneration)return;
    marketGroupMatchIds.set(group.id,new Set());
    marketGroupQueryErrors.set(group.id,error.message||'筛选分组查询失败');
  }finally{
    marketGroupAbortControllers.delete(group.id);
  }
}
async function refreshMarketGroupMatches(render){
  const generation=++marketGroupQueryGeneration;
  marketGroupMatchIds.clear();marketGroupQueryErrors.clear();
  const groups=D.filterGroupsCredit.concat(D.filterGroupsRate);
  await Promise.all(groups.map(group=>queryMarketFilterGroup({...group,kind:'filter',market:D.filterGroupsRate.includes(group)?'rate':'credit'},generation)));
  if(generation!==marketGroupQueryGeneration)return;
  if(render!==false)renderFilteredViews();
}
// P0: 保存筛选分组后标记缓存未就绪，点击时再加载
function invalidateFilterGroupCaches(){
  marketGroupMatchIds.clear();marketGroupQueryErrors.clear();marketGroupCacheTime.clear();
  marketGroupAbortControllers.forEach(ctrl=>ctrl.abort());marketGroupAbortControllers.clear();
}
// P0+P2: 单个筛选分组异步查询（点击时触发，支持过期重查与abort）
async function ensureFilterGroupMatches(group){
  const cached=marketGroupCacheTime.get(group.id);
  if(marketGroupMatchIds.has(group.id)&&cached&&(Date.now()-cached)<MARKET_GROUP_CACHE_TTL)return;
  // 取消上一次未完成的请求
  const prev=marketGroupAbortControllers.get(group.id);if(prev)prev.abort();
  const controller=new AbortController();
  marketGroupAbortControllers.set(group.id,controller);
  const generation=marketGroupQueryGeneration;
  await queryMarketFilterGroup(group,generation,controller.signal);
  renderFilteredViews();
}
function marketForScopeRow(row,scope){
  if(scope==='credit'||scope==='rate')return scope;
  if(row&&row.market)return row.market;
  return classifyBondMarket(row&&row.c,row&&row.n,row&&row._apiInfo,row&&row.bondType);
}
function rowMatchesActiveMarketGroups(row,scope){
  const market=marketForScopeRow(row,scope),code=normalizeBondCode(row&&row.c);
  const favoriteGroups=(market==='rate'?D.favBondGroupsRate:D.favBondGroupsCredit).filter(group=>activeMarketGroupIds.favorite.has(group.id));
  if(favoriteGroups.length&&!favoriteGroups.some(group=>(group.bonds||[]).some(bond=>normalizeBondCode(_fbmBondCode(bond))===code)))return false;
  const filterGroups=(market==='rate'?D.filterGroupsRate:D.filterGroupsCredit).filter(group=>activeMarketGroupIds.filter.has(group.id));
  if(filterGroups.length&&!filterGroups.some(group=>(marketGroupMatchIds.get(group.id)||new Set()).has(code)))return false;
  return true;
}
function matchedMarketFilterGroups(entry){
  const market=marketForScopeRow(entry,''),code=normalizeBondCode(entry&&entry.c);
  return(market==='rate'?D.filterGroupsRate:D.filterGroupsCredit).filter(group=>(marketGroupMatchIds.get(group.id)||new Set()).has(code));
}
function matchedFavoriteGroups(entry){
  const market=marketForScopeRow(entry,''),code=normalizeBondCode(entry&&entry.c);
  return(market==='rate'?D.favBondGroupsRate:D.favBondGroupsCredit).filter(group=>(group.bonds||[]).some(bond=>normalizeBondCode(_fbmBondCode(bond))===code));
}
async function toggleMarketGroup(groupId){
  const group=marketGroupById(groupId);if(!group)return;
  const ids=activeMarketGroupIds[group.kind];
  const activating=!ids.has(groupId);
  if(activating)ids.add(groupId);else ids.delete(groupId);
  const key='board-'+group.market+'-all';
  // P0: 点击筛选分组时，若命中集未就绪/已过期则异步查询，不冻结UI
  if(activating&&group.kind==='filter'){
    renderFilteredViews();
    await ensureFilterGroupMatches({...group,kind:'filter',market:group.market});
    return; // ensureFilterGroupMatches 内部已调用 renderFilteredViews
  }
  if(boardToolbarState[key].unquoted)loadUnquotedBonds(group.market,1);else renderFilteredViews();
}
document.addEventListener('click',event=>{
  const tag=event.target.closest('[data-market-group-id]');
  if(!tag)return;
  event.preventDefault();event.stopPropagation();
  toggleMarketGroup(tag.dataset.marketGroupId);
});

async function populateRegionSelectors(){
  const selects=Array.from(document.querySelectorAll('.region-select'));
  if(!selects.length)return;
  const desktopRegions=window.desktopAPI&&typeof window.desktopAPI.getChinaRegions==='function';
  if(!desktopRegions&&!authToken)return false;
  try{
    let regions;
    if(desktopRegions)regions=await window.desktopAPI.getChinaRegions();
    else{
      const response=await fetch(API_BASE+'/reference-data/regions');
      const payload=await response.json().catch(()=>({}));
      if(!response.ok||!Array.isArray(payload.regions))throw new Error(payload.detail||'全国行政区数据接口不可用');
      regions=payload.regions;
    }
    chinaRegionsByCode.clear();
    (regions||[]).forEach(region=>chinaRegionsByCode.set(String(region.code),region));
    selects.forEach(select=>{
      select.replaceChildren(new Option('不限',''));
      (regions||[]).forEach(region=>{
        const levelName=region.level==='province'?'省级':region.level==='city'?'市级':'区县级';
        const option=new Option(`[${levelName}] ${region.label}`,String(region.code));
        if(region.enabled===false)option.disabled=true;
        select.add(option);
      });
      select.disabled=false;
    });
  }catch(error){
    selects.forEach(select=>{select.replaceChildren(new Option('全国地区数据加载失败',''));select.disabled=true;select.title=error&&error.message||'地区数据加载失败';});
  }
}

function regionSelectionFromSelect(select){
  const region=select&&chinaRegionsByCode.get(String(select.value||''));
  if(!region)return null;
  return {code:String(region.code),level:region.level,province:region.province||'',city:region.city||'',district:region.district||'',descendants:Array.isArray(region.descendants)?region.descendants:[]};
}

function waitForBondFilterRetry(milliseconds){return new Promise(resolve=>setTimeout(resolve,milliseconds));}
async function loadBondFilterCapabilities(){
  if(bondFilterConfigLoadPromise)return bondFilterConfigLoadPromise;
  bondFilterConfigLoadPromise=(async()=>{
    const delays=[0,250,750,1500],hadConfig=bondFilterCapabilities.byLabel.size>0;
    let lastError=null;
    for(const delay of delays){
      if(delay)await waitForBondFilterRetry(delay);
      const controller=new AbortController(),timeout=setTimeout(()=>controller.abort(),3000);
      try{
        const response=await authFetch(API_BASE+'/bond-filter/config',{signal:controller.signal});
        let data={};try{data=await response.json();}catch(e){}
        if(!response.ok||!Array.isArray(data.groups))throw new Error(typeof data.detail==='string'?data.detail:'筛选能力配置格式异常');
        const items=data.groups.flatMap(group=>Array.isArray(group.items)?group.items:[]);
        bondFilterCapabilities.byLabel=new Map(items.filter(item=>item&&item.label).map(item=>[item.label,item]));
        bondFilterCapabilities.contractVersion=String(data.contract_version||'');
        bondFilterCapabilities.profileAsOf=data.profile_as_of||null;
        bondFilterCapabilities.dataSource=String(data.data_source||'');
        bondFilterCapabilities.mockEnabled=data.mock_enabled===true;
        marketFilterPanelLabelCache.clear();marketFilterCatalogCache.clear();
        bondFilterCapabilities.error='';bondFilterCapabilities.lastError='';
        if(D.marketGroupLoaded&&activeMarketGroupIds.filter.size>0)refreshMarketGroupMatches(true);
        return true;
      }catch(error){lastError=error;}
      finally{clearTimeout(timeout);}
    }
    const message=lastError&&lastError.message||'筛选能力配置不可用';
    bondFilterCapabilities.lastError=message;
    if(!hadConfig){bondFilterCapabilities.byLabel=new Map();bondFilterCapabilities.error=message;}
    return false;
  })().finally(()=>{bondFilterConfigLoadPromise=null;});
  return bondFilterConfigLoadPromise;
}

function clearFilterControlCapabilityState(group){
  group.hidden=false;group.classList.remove('filter-group-unsupported','filter-group-config-unavailable');group.removeAttribute('title');group.removeAttribute('aria-disabled');delete group.dataset.unavailableReason;
  delete group.dataset.bondFilterKey;delete group.dataset.bondFilterType;
  group.querySelectorAll('.filter-tag,select,input,button').forEach(control=>{control.hidden=false;control.classList.remove('filter-control-unsupported');control.removeAttribute('title');control.removeAttribute('aria-disabled');if('disabled'in control)control.disabled=false;});
}

function markFilterConfigUnavailable(group,reason){
  clearFilterControlCapabilityState(group);group.classList.add('filter-group-config-unavailable');
  group.removeAttribute('title');group.removeAttribute('data-unavailable-reason');
  group.querySelectorAll('select,input,button').forEach(control=>{control.disabled=true;});
}

function disableFilterControl(group,reason){
  showPendingFilterControl(group,reason||'当前债券筛选条件不可用');
}

function showPendingFilterControl(group,reason){
  clearFilterControlCapabilityState(group);
  group.classList.add('filter-group-unsupported');group.removeAttribute('title');group.removeAttribute('data-unavailable-reason');group.setAttribute('aria-disabled','true');
  group.querySelectorAll('.filter-tag,select,input,button').forEach(control=>{control.classList.add('filter-control-unsupported');control.removeAttribute('title');control.setAttribute('aria-disabled','true');if('disabled'in control)control.disabled=true;});
}

function applyBondCapability(group,capability){
  if(!capability){disableFilterControl(group,'当前债券筛选接口未声明此条件');return false;}
  if(capability.enabled!==true){
    showPendingFilterControl(group,capability.disabled_reason||bondFilterCapabilities.error||'当前债券筛选接口暂不支持此条件');
    return false;
  }
  clearFilterControlCapabilityState(group);
  group.dataset.bondFilterKey=capability.key;
  group.dataset.bondFilterType=capability.type||'tag';
  const optionMap=new Map((capability.options||[]).map(option=>[normalizeBondFilterValue(capability.key,option.value),option]));
  group.querySelectorAll('.filter-tag').forEach(tag=>{
    const value=normalizeBondFilterValue(capability.key,tag.textContent);tag.dataset.filterValue=value;
    const option=optionMap.size?optionMap.get(value):null;
    if(optionMap.size&&(!option||option.enabled!==true)){tag.classList.add('filter-control-unsupported');tag.removeAttribute('title');tag.removeAttribute('data-unavailable-reason');tag.setAttribute('aria-disabled','true');}
  });
  group.querySelectorAll('select').forEach(select=>{
    if(!optionMap.size)return;
    Array.from(select.options).forEach(option=>{
      const value=normalizeBondFilterValue(capability.key,option.value||option.textContent);
      if(!value||value==='不限')return;
      const state=optionMap.get(value),available=state&&state.enabled===true;
      option.hidden=false;option.disabled=!available;
      option.removeAttribute('title');
    });
    if(select.selectedOptions[0]&&select.selectedOptions[0].disabled)select.selectedIndex=0;
  });
  return true;
}

function normalizeBondCode(value){return String(value||'').trim().toUpperCase();}
function bondFilterScopeForPanel(panel){return Object.keys(BOND_FILTER_SCOPES).find(scope=>BOND_FILTER_SCOPES[scope].panelId===panel.id)||'';}
function normalizeBondFilterValue(key,value){
  const normalized=String(value||'').trim().replace(/[（）]/g,'');
  return normalized.replace(/^＞/,'>');
}
function addFilterValue(filters,key,value){
  if(value==null||value===''||(Array.isArray(value)&&!value.length))return;
  if(Array.isArray(value)){
    filters[key]=filters[key]||[];
    value.forEach(item=>{if(!filters[key].some(old=>JSON.stringify(old)===JSON.stringify(item)))filters[key].push(item);});
  }else filters[key]=value;
}
function selectedFilterTags(group,key){
  return Array.from(group.querySelectorAll('.filter-tag.active:not(.filter-control-unsupported)')).map(tag=>normalizeBondFilterValue(key,tag.dataset.filterValue||tag.textContent)).filter(value=>value&&value!=='全部');
}
function collectBondFilters(scope){
  const config=BOND_FILTER_SCOPES[scope],panel=document.getElementById(config.panelId),filters={};
  if(!panel)return filters;
  panel.querySelectorAll('.filter-grid-item[data-bond-filter-key]').forEach(group=>{const range=numericRangeFromGroup(group);if(range.min!=null||range.max!=null)addFilterValue(filters,group.dataset.bondFilterKey,range);});
  panel.querySelectorAll('.filter-group[data-bond-filter-key]').forEach(group=>{
    const key=group.dataset.bondFilterKey;
    if(key==='issuer'||key==='guarantor'){
      const input=group.querySelector('input');if(input&&input.value.trim())addFilterValue(filters,key,input.value.trim());
      return;
    }
    if(group.dataset.bondFilterType==='select'){
      const select=group.querySelector('select');
      if(key==='region'){const selected=regionSelectionFromSelect(select);if(selected)addFilterValue(filters,key,[selected]);}
      else if(select&&select.value&&select.value!=='不限')addFilterValue(filters,key,[select.value]);
      return;
    }
    if(key==='rt'){
      const values=selectedFilterTags(group,key),inputs=group.querySelectorAll('.filter-input'),unit=group.querySelector('select');
      const min=inputs[0]&&inputs[0].value!==''?Number(inputs[0].value):null,max=inputs[1]&&inputs[1].value!==''?Number(inputs[1].value):null;
      if(min!=null||max!=null){const factor=unit&&unit.value==='M'?1/12:unit&&unit.value==='D'?1/365:1;values.push({min:min==null?0:min*factor,max:max==null?99999:max*factor});}
      addFilterValue(filters,key,values);return;
    }
    addFilterValue(filters,key,selectedFilterTags(group,key));
  });
  if(Object.keys(filters).length&&config.baseProd&&!filters.prod){filters.prod=config.baseProd.slice();if(scope==='credit'&&Array.isArray(filters.type)&&filters.type.includes('NCD')&&!filters.prod.includes('NCD'))filters.prod.push('NCD');}
  return filters;
}
function filterCacheKey(filters){return JSON.stringify(Object.keys(filters).sort().reduce((out,key)=>{const value=filters[key];out[key]=Array.isArray(value)?value.slice().sort((a,b)=>JSON.stringify(a).localeCompare(JSON.stringify(b))):value;return out;},{}));}
// UI规范：筛选面板内不展示任何反馈文字（避免挤占搜索框宽度）；正常状态静默，异常弹toast浮层并按面板去重
const _bondFilterLastError=new Map();
function setBondFilterStatus(scope,status,text){
  const panel=document.getElementById(BOND_FILTER_SCOPES[scope].panelId);if(!panel)return;
  const el=panel.querySelector('.filter-query-status');if(el)el.textContent='';
  if(status==='error'&&text){
    if(_bondFilterLastError.get(scope)!==text){_bondFilterLastError.set(scope,text);showToast('error',text);}
  }else{
    _bondFilterLastError.delete(scope);
  }
}
function bondFilterMetaText(data){
  const source=String(data&&data.data_source||bondFilterCapabilities.dataSource||'').replace('DM_API','DM API');
  const asOf=data&&data.profile_as_of||bondFilterCapabilities.profileAsOf;
  const rule=data&&data.rule_version||data&&data.contract_version||bondFilterCapabilities.contractVersion;
  return[source,asOf?'数据 '+String(asOf).replace('T',' ').slice(0,19):'',rule?'规则 '+rule:''].filter(Boolean).join(' · ');
}
function renderFilteredViews(){rebuildBoardProjections();syncMgmtToday();syncMgmtHistory();renderAll();updateBadges();}
function invalidateBondFilterRequest(runtime){
  runtime.requestId++;
  if(runtime.controller)runtime.controller.abort();
  if(runtime.timer)clearTimeout(runtime.timer);
  runtime.controller=null;runtime.timer=null;runtime.loading=false;
}
async function executeBondFilter(scope){
  const runtime=bondFilterRuntime[scope],filters=collectBondFilters(scope),keys=Object.keys(filters).filter(key=>!(key==='prod'&&BOND_FILTER_SCOPES[scope].baseProd));
  invalidateBondFilterRequest(runtime);
  const unquotedKey=(scope==='credit'||scope==='rate')?'board-'+scope+'-all':'';
  if(unquotedKey&&boardToolbarState[unquotedKey].unquoted){runtime.bondIds=null;runtime.total=null;runtime.error='';setBondFilterStatus(scope,'','');await loadUnquotedBonds(scope,1);return;}
  if(!keys.length){runtime.bondIds=null;runtime.total=null;runtime.error='';setBondFilterStatus(scope,'','');renderFilteredViews();return;}
  const cacheKey=bondFilterCapabilities.contractVersion+'|'+scope+'|'+filterCacheKey(filters),cached=bondFilterCache.get(cacheKey);
  if(cached&&Date.now()-cached.time<BOND_FILTER_CACHE_TTL){runtime.bondIds=new Set(cached.ids);runtime.total=cached.total;runtime.error='';setBondFilterStatus(scope,'success','匹配 '+cached.total+' 只债券'+(cached.meta?' · '+cached.meta:''));renderFilteredViews();return;}
  runtime.controller=new AbortController();const requestId=runtime.requestId;runtime.loading=true;setBondFilterStatus(scope,'loading','查询中...');
  try{
    const response=await authFetch(API_BASE+'/bond-filter/query',{method:'POST',body:JSON.stringify({filters}),signal:runtime.controller.signal});
    let data={};try{data=await response.json();}catch(e){}
    if(!response.ok){const detail=data.detail;if(detail&&detail.code==='FILTER_CAPABILITY_UNAVAILABLE')throw new Error('筛选条件当前不可用');throw new Error(typeof detail==='string'?detail:detail&&detail.message||'筛选查询失败');}
    if(!Array.isArray(data.security_ids)||!Number.isFinite(Number(data.total)))throw new Error('筛选服务响应格式异常');
    if(requestId!==runtime.requestId)return;
    const ids=data.security_ids.map(normalizeBondCode).filter(Boolean),meta=bondFilterMetaText(data);runtime.bondIds=new Set(ids);runtime.total=Number(data.total);runtime.error='';bondFilterCache.set(cacheKey,{ids,total:runtime.total,meta,time:Date.now()});
    setBondFilterStatus(scope,'success','匹配 '+runtime.total+' 只债券'+(meta?' · '+meta:''));renderFilteredViews();
  }catch(error){
    if(error.name==='AbortError'||requestId!==runtime.requestId)return;
    runtime.bondIds=null;runtime.total=null;runtime.error=error.message||'债券筛选服务不可用';setBondFilterStatus(scope,'error',runtime.error);renderFilteredViews();
  }finally{if(requestId===runtime.requestId)runtime.loading=false;}
}
function scheduleBondFilter(scope,immediate){
  const runtime=bondFilterRuntime[scope];
  runtime.requestId++;
  if(runtime.controller)runtime.controller.abort();
  if(runtime.timer)clearTimeout(runtime.timer);
  runtime.controller=null;runtime.loading=false;
  runtime.timer=setTimeout(()=>executeBondFilter(scope),immediate?0:300);
}
function scheduleLocalQuoteFilter(){
  const key=activeBoardViewKey(),meta=BOARD_VIEW_META[key];
  if(meta&&!meta.best&&boardToolbarState[key].unquoted){setTimeout(()=>loadUnquotedBonds(meta.market,1),0);return;}
  setTimeout(renderFilteredViews,0);
}
function numericRangeFromGroup(group){
  const inputs=group?group.querySelectorAll('.filter-input'):[],min=inputs[0]&&inputs[0].value!==''?Number(inputs[0].value):null,max=inputs[1]&&inputs[1].value!==''?Number(inputs[1].value):null;
  return{min:Number.isFinite(min)?min:null,max:Number.isFinite(max)?max:null};
}
function quoteNumberInRange(value,range){const number=Number(value);return Number.isFinite(number)&&(range.min==null||number>=range.min)&&(range.max==null||number<=range.max);}
function emptyQuoteFilterState(){return{settlement:[],priceType:[],ranges:{}};}
function collectQuoteFilterDraft(scope){
  const panel=document.getElementById(BOND_FILTER_SCOPES[scope].panelId),state=emptyQuoteFilterState();if(!panel)return state;
  panel.querySelectorAll('.filter-group[data-quote-filter-key]').forEach(group=>{state[group.dataset.quoteFilterKey]=selectedFilterTags(group,group.dataset.quoteFilterKey);});
  panel.querySelectorAll('.filter-grid-item[data-quote-range-key]:not(.filter-page-unavailable)').forEach(group=>{const range=numericRangeFromGroup(group);if(range.min!=null||range.max!=null)state.ranges[group.dataset.quoteRangeKey]=range;});
  return state;
}
function markQuoteFilterDirty(scope){
  const panel=document.getElementById(BOND_FILTER_SCOPES[scope].panelId);if(!panel)return;
  panel.dataset.quoteFilterDirty='1';
  const button=panel.querySelector('.filter-group[data-quote-filter-key] .filter-btn-primary');
  if(button)button.dataset.dirty='1';
}
function applyQuoteFilters(scope){
  const panel=document.getElementById(BOND_FILTER_SCOPES[scope].panelId);if(!panel)return;
  appliedQuoteFilterState[scope]=collectQuoteFilterDraft(scope);
  delete panel.dataset.quoteFilterDirty;
  const button=panel.querySelector('.filter-group[data-quote-filter-key] .filter-btn-primary');
  if(button)delete button.dataset.dirty;
  renderFilteredViews();
}
function collectLocalFilterState(scope){
  const panel=document.getElementById(BOND_FILTER_SCOPES[scope].panelId),applied=appliedQuoteFilterState[scope]||emptyQuoteFilterState(),state={search:'',settlement:applied.settlement.slice(),priceType:applied.priceType.slice(),ranges:{...applied.ranges}};if(!panel)return state;
  const search=panel.querySelector('.filter-section-sticky .tb-input');state.search=search?search.value.trim().toLowerCase():'';
  return state;
}
function quoteMatchesLocalFilters(row,scope,state){
  state=state||collectLocalFilterState(scope);
  const direction=quoteDirectionOf(row),side=quoteSideValues(row,direction);
  if(state.search){const haystack=[row.c,row.n,row.issuer,row.issuerName,row.inst,row.trader,row.tr,row.counterpartyInstitution,row.counterpartyTrader,row.ownerDisplayName].join(' ').toLowerCase();if(!haystack.includes(state.search))return false;}
  if(state.settlement.length){const cs=String(row.cs||'');if(!state.settlement.some(value=>value==='远期'?cs&&!['T+0','T+1'].includes(cs):value==='非远期'?['T+0','T+1'].includes(cs):cs===value))return false;}
  if(state.priceType.length){const priceType=String(row.priceType||'到期收益率');if(!state.priceType.some(value=>value==='收益率'?priceType.includes('收益率'):priceType===value))return false;}
  const ranges=state.ranges;
  if(ranges.bid&&(!row.bp||!quoteNumberInRange(row.bp,ranges.bid)))return false;
  if(ranges.ofr&&(!row.op||!quoteNumberInRange(row.op,ranges.ofr)))return false;
  if(ranges.volBid&&(direction!=='bid'||!quoteNumberInRange(side.volume,ranges.volBid)))return false;
  if(ranges.volOfr&&(direction!=='ofr'||!quoteNumberInRange(side.volume,ranges.volOfr)))return false;
  const dev=Number(row.nzDev),yieldComparable=['YTM','YTC'].includes(quotePairPriceType(row));
  if(ranges.bidDev&&(direction!=='bid'||!yieldComparable||!quoteNumberInRange(dev,ranges.bidDev)))return false;
  if(ranges.ofrDev&&(direction!=='ofr'||!yieldComparable||!quoteNumberInRange(-dev,ranges.ofrDev)))return false;
  return true;
}
function applyScopeFilters(rows,scope){
  const runtime=bondFilterRuntime[scope],ids=runtime&&runtime.bondIds,state=collectLocalFilterState(scope),source=rows||[];
  const spreadIndex=state.ranges.spread?quoteSpreadIndex(source):null;
  return source.filter(row=>{
    if(ids!==null&&!ids.has(normalizeBondCode(row.c)))return false;
    if(!rowMatchesActiveMarketGroups(row,scope))return false;
    if(!quoteMatchesLocalFilters(row,scope,state))return false;
    const range=state.ranges.spread;if(!range)return true;
    const prices=spreadIndex.get(quotePairKey(row))||{bid:[],ofr:[]};
    return prices.bid.some(bid=>prices.ofr.some(ofr=>quoteNumberInRange((bid-ofr)*100,range)));
  });
}
function resetBondFilterPanel(scope,render){
  const panel=document.getElementById(BOND_FILTER_SCOPES[scope].panelId),runtime=bondFilterRuntime[scope];if(!panel)return;
  appliedQuoteFilterState[scope]=emptyQuoteFilterState();delete panel.dataset.quoteFilterDirty;
  const quoteButton=panel.querySelector('.filter-group[data-quote-filter-key] .filter-btn-primary');if(quoteButton)delete quoteButton.dataset.dirty;
  panel.querySelectorAll('.filter-tag.active').forEach(tag=>tag.classList.remove('active'));
  panel.querySelectorAll('input').forEach(input=>input.value='');panel.querySelectorAll('select').forEach(select=>select.selectedIndex=0);
  panel.querySelectorAll('.filter-group').forEach(group=>{const first=group.querySelector('.filter-tag');if(first&&first.textContent.trim()==='全部')first.classList.add('active');});
  invalidateBondFilterRequest(runtime);runtime.bondIds=null;runtime.total=null;runtime.error='';setBondFilterStatus(scope,'','');
  if(render!==false)renderFilteredViews();
}
function clearBondFilterScope(scope){
  const markets=scope==='credit'||scope==='rate'?[scope]:['credit','rate'];
  allMarketGroups().filter(group=>markets.includes(group.market)).forEach(group=>activeMarketGroupIds[group.kind].delete(group.id));
  const key=activeBoardViewKey(),meta=BOARD_VIEW_META[key];
  if(meta&&markets.includes(meta.market)){setUnquotedQuoteControls(meta.market,false);resetBoardToolbarState(key);}
  clearBoardSelection();syncBoardToolbarControls();
  if(scope==='mgmt')resetMgmtToolbarFilters(false);
  resetBondFilterPanel(scope,true);
}
function bindEarlyBondFilterInteraction(scope,panel){
  if(!panel||panel.dataset.bondFilterEarlyBound==='1')return;
  panel.dataset.bondFilterEarlyBound='1';
  panel.addEventListener('click',event=>{
    if(panel.dataset.bondFilterEarlyDone==='1')return;
    const tag=event.target.closest('.filter-tag'),button=event.target.closest('.filter-btn-primary');
    const group=(tag||button)&&((tag||button).closest('.filter-group'));
    if(!group)return;
    const label=(group.querySelector('.filter-label')||{}).textContent?.trim()||'';
    const quoteKey=QUOTE_FILTER_LABELS[label];
    if(quoteKey==='settlement'||quoteKey==='priceType'){
      setTimeout(()=>applyQuoteFilters(scope),0);
    }else panel.dataset.filterPending='1';
  },true);
  panel.addEventListener('input',event=>{
    if(panel.dataset.bondFilterEarlyDone==='1')return;
    const control=event.target.closest('input,select');if(!control)return;
    const group=control.closest('.filter-group,.filter-grid-item');if(!group)return;
    const label=(group.querySelector('.filter-label')||{}).textContent?.trim()||'';
    if(QUOTE_FILTER_LABELS[label])markQuoteFilterDirty(scope);else panel.dataset.filterPending='1';
  },true);
  panel.addEventListener('change',event=>{
    if(panel.dataset.bondFilterEarlyDone==='1')return;
    const control=event.target.closest('input,select');if(!control)return;
    const group=control.closest('.filter-group,.filter-grid-item');if(group)panel.dataset.filterPending='1';
  },true);
}
async function initBondFilterPanels(){
  Object.entries(BOND_FILTER_SCOPES).forEach(([scope,config])=>{const panel=document.getElementById(config.panelId);if(panel){panel.dataset.filterScope=scope;bindEarlyBondFilterInteraction(scope,panel);}});
  await loadBondFilterCapabilities();
  const configUnavailable=!!bondFilterCapabilities.error;
  Object.entries(BOND_FILTER_SCOPES).forEach(([scope,config])=>{
    const panel=document.getElementById(config.panelId);if(!panel)return;panel.dataset.filterScope=scope;
    const firstInit=panel.dataset.bondFilterInitialized!=='1';
    const sticky=panel.querySelector('.filter-section-sticky');if(sticky&&firstInit){const clear=sticky.querySelector('button');if(clear){clear.dataset.filterClear='1';clear.onclick=null;clear.addEventListener('click',()=>clearBondFilterScope(scope));}const search=sticky.querySelector('input');if(search)search.addEventListener('input',scheduleLocalQuoteFilter);}
    panel.querySelectorAll('.filter-group').forEach(group=>{
      const label=(group.querySelector('.filter-label')||{}).textContent;if(!label)return;const name=label.trim(),quoteKey=QUOTE_FILTER_LABELS[name],capability=bondFilterCapabilities.byLabel.get(name);
      if(quoteKey){group.dataset.quoteFilterKey=quoteKey;if(firstInit)group.querySelectorAll('input,select').forEach(control=>control.addEventListener(control.tagName==='SELECT'?'change':'input',()=>markQuoteFilterDirty(scope)));}
      else if(configUnavailable&&group.querySelector('.filter-tag,select,input'))markFilterConfigUnavailable(group,'筛选服务未就绪，正在自动重试');
      else if(capability)applyBondCapability(group,capability);
      else if(group.querySelector('.filter-tag,select,input'))disableFilterControl(group,'当前债券筛选接口未声明此条件');
      if(firstInit&&!quoteKey)group.querySelectorAll('input,select').forEach(control=>control.addEventListener(control.tagName==='SELECT'?'change':'input',()=>scheduleBondFilter(scope,control.tagName==='SELECT')));
    });
    panel.querySelectorAll('.filter-grid-item').forEach(group=>{const label=group.querySelector('.filter-label'),name=label&&label.textContent.trim(),quoteKey=QUOTE_RANGE_LABELS[name],capability=bondFilterCapabilities.byLabel.get(name);if(quoteKey){group.dataset.quoteRangeKey=quoteKey;if(firstInit)group.querySelectorAll('input').forEach(input=>input.addEventListener('input',()=>markQuoteFilterDirty(scope)));}else if(configUnavailable)markFilterConfigUnavailable(group,'筛选服务未就绪，正在自动重试');else if(capability)applyBondCapability(group,capability);else disableFilterControl(group,'当前债券筛选接口未声明此条件');if(firstInit&&!quoteKey)group.querySelectorAll('input').forEach(input=>input.addEventListener('input',()=>scheduleBondFilter(scope,false)));});
    if(firstInit){
      panel.querySelectorAll('.filter-btn-primary').forEach(button=>{button.onclick=null;const group=button.closest('.filter-group'),isQuoteGroup=group&&group.dataset.quoteFilterKey;button.addEventListener('click',()=>isQuoteGroup?applyQuoteFilters(scope):scheduleBondFilter(scope,true));});
       panel.addEventListener('click',event=>{if(event.target.closest('.filter-control-unsupported,.filter-group-unsupported,.filter-group-config-unavailable,.filter-page-unavailable')){event.preventDefault();event.stopImmediatePropagation();}},true);
       panel.addEventListener('click',event=>{const tag=event.target.closest('.filter-tag');if(!tag||tag.closest('.filter-control-unsupported,.filter-group-config-unavailable'))return;const group=tag.closest('.filter-group');if(!group)return;if(group.dataset.bondFilterKey)setTimeout(()=>scheduleBondFilter(scope,true),0);else if(group.dataset.quoteFilterKey){const key=group.dataset.quoteFilterKey;if(key==='settlement'||key==='priceType')applyQuoteFilters(scope);else markQuoteFilterDirty(scope);}});
       panel.dataset.bondFilterEarlyDone='1';
       panel.dataset.bondFilterInitialized='1';
    }
    if(configUnavailable)setBondFilterStatus(scope,'error','筛选配置加载失败，自动重试中');
    else setBondFilterStatus(scope,'','');
  });
   Object.entries(BOND_FILTER_SCOPES).forEach(([scope,config])=>{const panel=document.getElementById(config.panelId);if(!panel)return;if(panel.dataset.filterPending==='1'){delete panel.dataset.filterPending;const hasBondConditions=panel.querySelector('[data-bond-filter-key] .filter-tag.active,[data-bond-filter-key] input,[data-bond-filter-key] select');if(hasBondConditions)scheduleBondFilter(scope,true);}});
  syncBidOfrAvailability();
  marketFilterPanelLabelCache.clear();marketFilterCatalogCache.clear();
  availableMarketFilterGroupData('credit');availableMarketFilterGroupData('rate');
  if(configUnavailable&&!bondFilterRecoveryTimer)bondFilterRecoveryTimer=setTimeout(()=>{bondFilterRecoveryTimer=null;initBondFilterPanels();},5000);
  else if(!configUnavailable&&bondFilterRecoveryTimer){clearTimeout(bondFilterRecoveryTimer);bondFilterRecoveryTimer=null;}
}

function safeTableValue(value){
  if(typeof value==='string')return escapeQuoteHtml(value);
  if(Array.isArray(value))return value.map(safeTableValue);
  if(value&&typeof value==='object')return Object.fromEntries(Object.entries(value).map(([key,item])=>[key,safeTableValue(item)]));
  return value;
}
function renderTable(id,data,cols,options){
  const tbody=document.getElementById(id);
  if(!tbody||!data)return;
  const table=tbody.closest('table');
  const viewKey=Object.keys(BOARD_VIEW_META||{}).find(key=>BOARD_VIEW_META[key].tbodyId===id)||'';
  const lefts=viewKey?boardColumnLeftMap(viewKey):{};
  tbody.innerHTML=data.map((r,i)=>{
    const rowClasses=[];
    if(i%2===0)rowClasses.push('odd');
    if(options&&typeof options.rowSelected==='function'&&options.rowSelected(r,i))rowClasses.push('selected');
    const rowClass=rowClasses.join(' ');
    const safeRow=safeTableValue(r);
    let cells='';
    for(const c of cols){
      let v=c.render?c.render(safeRow,i):(safeRow[c.key]||'—');
      if(c.cls)v=`<span class="${c.cls}">${v}</span>`;
      if(c.type==='code')v=`<span class="code">${v}</span>`;
      if(c.type==='price')v=`<span class="price">${v}</span>`;
      if(c.type==='neg')v=`<span class="neg">${v}</span>`;
      if(c.type==='muted')v=`<span class="muted">${v}</span>`;
      if(c.type==='tag-bid')v=`<span class="tag tag-bid">${v}</span>`;
      if(c.type==='tag-ofr')v=`<span class="tag tag-ofr">${v}</span>`;
      if(c.type==='tag-firm')v=`<span class="tag tag-firm">${v}</span>`;
      if(c.type==='tag-nonfirm')v=`<span class="tag tag-nonfirm">${v}</span>`;
      if(c.type==='tag-success')v=`<span class="tag tag-success">${v}</span>`;
      if(c.type==='tag-warning')v=`<span class="tag tag-warning">${v}</span>`;
      if(c.type==='status')v=`<span style="color:${r.sc||'#7d8590'};font-weight:600">${v}</span>`;
      const styles=[];
      if(c.bg)styles.push(c.bg);
      if(c.align)styles.push(`text-align:${c.align} !important`);
      if(c.width){styles.push(`width:${c.width}px`);styles.push(`min-width:${c.width}px`);styles.push(`max-width:${c.width}px`);}
      if(c.sticky)styles.push(`left:${lefts[c.colId||c.key]||0}px`);
      const cls=c.sticky?' class="board-col-sticky"':'';
      const tdStyle=styles.length?` style="${styles.join(';')}"`:'';
      const dataCol=c.colId||c.key?` data-col="${c.colId||c.key}"`:'';
      cells+=`<td${cls}${dataCol}${tdStyle}>${v}</td>`;
    }
    const quoteId=r.quoteId||r.sourceQuoteId||'';
    const quoteAttr=quoteId?` data-quote-id="${escapeQuoteHtml(String(quoteId))}"`:'';
    const rowKey=options&&typeof options.rowKey==='function'?options.rowKey(r,i):'';
    const rowKeyAttr=rowKey!==''&&rowKey!=null?` data-row-key="${escapeQuoteHtml(String(rowKey))}"`:'';
    return `<tr class="${rowClass}"${quoteAttr}${rowKeyAttr}>${cells}</tr>`;
  }).join('');
}
function updateMgmtTodayStatusbar(){
  const el=document.getElementById('mgmtTodayStatusbar');
  if(!el||!D.mgmtToday)return;
  const data=D.mgmtToday;
  const total=data.length;
  let valid=0,cancelled=0;
  data.forEach(r=>{if(r.rs==='有效')valid++;else if(r.rs==='已撤销'||r.rs==='撤销')cancelled++;});
  el.innerHTML='<span>共'+total+'条</span><span class="sep">|</span><span>有效: '+valid+' | 已撤销: '+cancelled+'</span>';
}
function makeQuoteId(prefix='q'){
  if(globalThis.crypto&&typeof globalThis.crypto.randomUUID==='function')return prefix+'-'+globalThis.crypto.randomUUID();
  return prefix+'-'+Date.now().toString(36)+'-'+Math.random().toString(36).slice(2,10);
}
function quoteById(quoteId){return(D.liveQuotes||[]).find(r=>r.quoteId===quoteId)||null;}
function quoteEventSnapshot(quote,action,note){
  const dir=quoteDirectionOf(quote),side=quoteSideValues(quote,dir);
  return{eventId:makeQuoteId('evt'),quoteId:quote.quoteId,action,time:quoteNow(),user:currentUser?currentUser.name:'当前用户',direction:dir,price:side.price,volume:side.volume,note:note||'',snapshot:{...quote}};
}
function addQuoteEvent(quote,action,note){
  const event=quoteEventSnapshot(quote,action,note);
  D.quoteEvents=D.quoteEvents||[];D.quoteEvents.unshift(event);
  return event;
}
function quotePriceIsBetter(candidate,current,side,priceType){
  const a=parseFloat(candidate),b=parseFloat(current);if(!Number.isFinite(a))return false;if(!Number.isFinite(b))return true;
  const isPrice=['净价','全价','面值'].includes(priceType);
  return side==='bid'?(isPrice?a>b:a<b):(isPrice?a<b:a>b);
}
function quoteComparableYield(row,side){
  if(!row)return null;
  const value=Number(side==='ofr'?row.op:row.bp),type=normalizePriceTypeCode(row.priceType||row.pt||'到期收益率');
  if(!Number.isFinite(value))return null;
  if(type==='YTM'||type==='YTC')return value;
  const record=row._valuationRecord||row.valuation||{};
  const callable=type==='YTC'||row.isCallable===true||String(row.bondType||'').includes('含权');
  const basis=callable?'YTC':'YTM';
  const refYield=Number(record[basis==='YTC'?'cb_yte':'cb_ytm']??(basis==='YTC'?row.cbYte:row.cbYtm));
  const refClean=Number(record[basis==='YTC'?'cb_npte':'cb_nptm']??record.cb_net_price??row.nzPrice??row.nzNet);
  const refFull=Number(record[basis==='YTC'?'cb_fpte':'cb_fptm']??record.full_price??record.cb_full_price);
  const duration=Number(record[basis==='YTC'?'cb_mdte':'cb_mdtm']??(basis==='YTC'?row.cbMdte:row.cbMdtm));
  if(!Number.isFinite(refYield)||!Number.isFinite(duration)||duration<=0)return null;
  if(type==='CLEAN_PRICE'&&Number.isFinite(refClean)&&refClean!==0)return refYield-((value-refClean)/(refClean*duration))*100;
  if(type==='DIRTY_PRICE'&&Number.isFinite(refFull)&&refFull!==0)return refYield-((value-refFull)/(refFull*duration))*100;
  return null;
}
function quoteRowsYieldIsBetter(candidate,current,side){
  const a=quoteComparableYield(candidate,side),b=quoteComparableYield(current,side);
  if(a!==null&&b!==null)return side==='bid'?a<b:a>b;
  if(a!==null&&b===null)return true;
  if(a===null&&b!==null)return false;
  const candidateType=normalizePriceTypeCode(candidate.priceType||candidate.pt||'到期收益率');
  const currentType=normalizePriceTypeCode(current.priceType||current.pt||'到期收益率');
  if(candidateType!==currentType||!['YTM','YTC'].includes(candidateType))return false;
  return quotePriceIsBetter(side==='bid'?candidate.bp:candidate.op,side==='bid'?current.bp:current.op,side,candidate.priceType||candidate.pt||'到期收益率');
}
function buildBestRows(rows){
  const grouped={};
  rows.forEach(row=>{
    const key=normalizeBondCode(row.c);
    if(!grouped[key])grouped[key]={...row,bp:'',op:'',bv:'',inst:'',tr:'',instBid:'',trBid:'',bvBid:'',instOfr:'',trOfr:'',bvOfr:'',bidQuoteId:'',ofrQuoteId:'',sourceQuoteId:''};
    const best=grouped[key],pt=row.priceType||'到期收益率';
    if(row.bp&&(!best.bp||quoteRowsYieldIsBetter(row,best,'bid'))){
      best.bp=row.bp;best.bvBid=row.bvBid||row.bv;best.instBid=row.instBid||row.inst;best.trBid=row.ownerDisplayName||row.tr;best.bidQuoteId=row.sourceQuoteId;
    }
    if(row.op&&(!best.op||quoteRowsYieldIsBetter(row,best,'ofr'))){
      best.op=row.op;best.bvOfr=row.bvOfr||row.bv;best.instOfr=row.instOfr||row.inst;best.trOfr=row.ownerDisplayName||row.tr;best.ofrQuoteId=row.sourceQuoteId;
    }
  });
  return Object.values(grouped);
}
function rebuildBoardProjections(){
  const active=sortMarketRows((D.liveQuotes||[]).filter(q=>q.status==='active'));
  const credit=active.filter(q=>q.market==='credit').map(q=>({...q,sourceQuoteId:q.quoteId}));
  const rate=active.filter(q=>q.market==='rate').map(q=>({...q,sourceQuoteId:q.quoteId}));
  const creditScoped=applyScopeFilters(credit,'credit'),rateScoped=applyScopeFilters(rate,'rate');
  D.boardCreditAll=boardToolbarState['board-credit-all'].unquoted?boardUnquotedState.credit.rows:applyBoardToolbarFilters(creditScoped,'board-credit-all');
  D.boardRateAll=boardToolbarState['board-rate-all'].unquoted?boardUnquotedState.rate.rows:applyBoardToolbarFilters(rateScoped,'board-rate-all');
  D.boardCreditBest=buildBestRows(applyBoardToolbarFilters(creditScoped,'board-credit-best'));
  D.boardRateBest=buildBestRows(applyBoardToolbarFilters(rateScoped,'board-rate-best'));
 D.boardAlert=[];
 active.forEach(entry=>refreshAlertsForQuote(entry,entry));
 D.boardAlert=applyScopeFilters(D.boardAlert,'alert');
  appendPriceMatchAlerts(active);
  mergeNonPriceMatchAlerts();
  D.boardAlert=applyScopeFilters(D.boardAlert,'alert');
}

function setBidOfrPanelAvailability(panelId,enabled,reason){
  const panel=document.getElementById(panelId);if(!panel)return;
  panel.querySelectorAll('.filter-grid-item[data-quote-range-key="spread"]').forEach(group=>{
    group.classList.toggle('filter-page-unavailable',!enabled);group.removeAttribute('title');delete group.dataset.pageUnavailableReason;
    group.querySelectorAll('input').forEach(input=>{if(!enabled)input.value='';input.disabled=!enabled;input.removeAttribute('title');input.setAttribute('aria-disabled',enabled?'false':'true');});
  });
}
function syncBidOfrAvailability(){
  const view=activeBoardViewKey(),reason='Bid-Ofr 仅在信用最优、利率最优和报价提醒页面对同券同计价方式、同清算速度的真实双边报价生效';
  setBidOfrPanelAvailability('filters-credit',view==='board-credit-best',reason);
  setBidOfrPanelAvailability('filters-rate',view==='board-rate-best',reason);
  setBidOfrPanelAvailability('filters-merged',true,'');
  setBidOfrPanelAvailability('filters-merged-mgmt',false,reason);
}
function buildMgmtTodayRow(r){
  const dir=quoteDirectionOf(r),side=quoteSideValues(r,dir);
  return{
    quoteId:r.quoteId,sourceQuoteId:r.quoteId,ownerAccountId:r.ownerAccountId,ownerUsername:r.ownerUsername,ownerDisplayName:r.ownerDisplayName,canManage:r.canManage,mode:r.mode||'手工录入',status:r.status||'active',rs:r.status==='revoked'?'撤销':'有效',rc:r.status==='revoked'?'#7d8590':'#3fb950',updatedAt:r.updatedAt||'',quotedAt:r.quotedAt||r.sourceSentAt||r.createdAt||'',createdAt:r.createdAt||'',
    m:r.m,c:r.c,n:r.n,direction:dir,bp:dir==='bid'?side.price:'',op:dir==='ofr'?side.price:'',bv:side.volume||'',
    inst:r.counterpartyInstitution||side.institution||r.inst,trader:r.counterpartyTrader||side.trader||r.trader,gwtrader:r.ownerDisplayName||r.tr,
    oco:!!r.oco,packing:!!r.packing,urgent:!!r.urgent,recommend:!!r.recommend,round:!!r.round,
    lastPrice:r.lastPrice||'—',bondLib:r.bondLib||'—',r:r.r||'—',br:r.br||'—',
    nzNet:r.nzNet||'—',nzDev:r.nzDev||'—',nzYTM:r.nzYTM||'—',csNet:r.csNet||'—',csFull:r.csFull||'—',csYTM:r.csYTM||'—',t:r.t||'',priceType:r.priceType||'',cs:r.cs||r.settlement||'',market:r.market||''
  };
}
function isQuoteCreatedToday(r){
  const today=quoteToday();
  const raw=String(r.quotedAt||r.sourceSentAt||r.createdAt||'');
  if(!raw)return true;
  const parsed=Date.parse(raw.replace(' ','T'));
  if(Number.isFinite(parsed)){const d=new Date(parsed);return d.getFullYear()+'-'+String(d.getMonth()+1).padStart(2,'0')+'-'+String(d.getDate()).padStart(2,'0')===today;}
  return raw.slice(0,10)===today;
}
function mgmtFilterText(value){return String(value==null?'':value).trim().toLowerCase();}
function applyMgmtToolbarFilters(rows,view){
  const state=mgmtToolbarState[view]||mgmtToolbarState.today;
  const bond=mgmtFilterText(state.bond),institution=mgmtFilterText(state.institution),gwTrader=mgmtFilterText(state.gwTrader),mode=mgmtFilterText(state.mode);
  return(rows||[]).filter(function(r){
    if(bond&&!mgmtFilterText((r.c||'')+' '+(r.n||'')).includes(bond))return false;
    if(institution&&!mgmtFilterText(r.inst||r.counterpartyInstitution).includes(institution))return false;
    if(gwTrader&&!mgmtFilterText(r.gwtrader||r.ownerDisplayName).includes(gwTrader))return false;
    if(mode&&mgmtFilterText(r.mode)!==mode)return false;
    if(state.status&&String(r.status||'active')!==state.status)return false;
    return true;
  });
}
function mgmtSearchIds(view,key){
  const prefix=view==='today'?'mgmtToday':'mgmtHistory',suffix=key==='bond'?'BondFilter':key==='institution'?'InstitutionFilter':'TraderFilter';
  return{inputId:prefix+suffix,clearId:prefix+suffix+'Clear',suggestionsId:prefix+suffix+'Suggestions'};
}
function mgmtSuggestionItems(view,key,query){
  const rows=view==='today'?(D.mgmtTodayAll||[]):(D.mgmtHistory||[]),normalized=mgmtFilterText(query),seen=new Set(),items=[];
  rows.forEach(function(row){
    let value='',main='',sub='';
    if(key==='bond'){main=String(row.c||'').trim();sub=String(row.n||'').trim();value=[main,sub].filter(Boolean).join(' ');}
    else if(key==='institution'){value=main=String(row.inst||row.counterpartyInstitution||'').trim();}
    else{value=main=String(row.gwtrader||row.ownerDisplayName||'').trim();}
    const identity=mgmtFilterText(value);if(!identity||seen.has(identity)||normalized&&!identity.includes(normalized))return;
    seen.add(identity);items.push({value,main:main||value,sub});
  });
  return items.sort((left,right)=>left.value.localeCompare(right.value,'zh-CN')).slice(0,30);
}
function toggleMgmtSearchClear(view,key,input){
  const ids=mgmtSearchIds(view,key),button=document.getElementById(ids.clearId),hasValue=!!String(input&&input.value||'').trim();
  if(button)button.classList.toggle('show',hasValue);
}
function closeMgmtSearchSuggestions(view,key){
  const ids=mgmtSearchIds(view,key),panel=document.getElementById(ids.suggestionsId),state=mgmtSuggestionState[view]&&mgmtSuggestionState[view][key];
  if(panel){panel.classList.remove('show');panel.innerHTML='';}if(state){state.items=[];state.active=-1;}
}
function scheduleCloseMgmtSuggestions(view,key){setTimeout(()=>closeMgmtSearchSuggestions(view,key),120);}
function renderMgmtSuggestionActive(view,key){
  const ids=mgmtSearchIds(view,key),panel=document.getElementById(ids.suggestionsId),state=mgmtSuggestionState[view]&&mgmtSuggestionState[view][key];if(!panel||!state)return;
  panel.querySelectorAll('.mgmt-search-suggestion').forEach((button,index)=>button.classList.toggle('active',index===state.active));
  const active=panel.querySelector('.mgmt-search-suggestion.active');if(active&&active.scrollIntoView)active.scrollIntoView({block:'nearest'});
}
function showMgmtSearchSuggestions(view,key,input){
  toggleMgmtSearchClear(view,key,input);
  const query=String(input&&input.value||'').trim(),ids=mgmtSearchIds(view,key),panel=document.getElementById(ids.suggestionsId),state=mgmtSuggestionState[view]&&mgmtSuggestionState[view][key];
  if(!panel||!state||!query){closeMgmtSearchSuggestions(view,key);return;}
  state.items=mgmtSuggestionItems(view,key,query);state.active=-1;
  panel.innerHTML=state.items.map((item,index)=>'<button class="mgmt-search-suggestion" type="button" onmousedown="event.preventDefault()" onclick="selectMgmtSuggestion(\''+view+'\',\''+key+'\','+index+')"><span class="mgmt-search-suggestion-main">'+escapeQuoteHtml(item.main)+'</span>'+(item.sub?'<span class="mgmt-search-suggestion-sub">'+escapeQuoteHtml(item.sub)+'</span>':'')+'</button>').join('');
  panel.classList.toggle('show',state.items.length>0);
}
function handleMgmtSearchInput(view,key,input){
  // 输入阶段只展示联想结果；点击候选或按 Enter 后才提交筛选。
  toggleMgmtSearchClear(view,key,input);showMgmtSearchSuggestions(view,key,input);
}
function handleMgmtSearchKeydown(event,view,key,input){
  const state=mgmtSuggestionState[view]&&mgmtSuggestionState[view][key];
  if(event.key==='ArrowDown'||event.key==='ArrowUp'){
    if(!state||!state.items.length)return;event.preventDefault();
    const delta=event.key==='ArrowDown'?1:-1;state.active=(state.active+delta+state.items.length)%state.items.length;renderMgmtSuggestionActive(view,key);return;
  }
  if(event.key==='Escape'){closeMgmtSearchSuggestions(view,key);return;}
  if(event.key!=='Enter')return;
  event.preventDefault();
  if(state&&state.active>=0&&state.items[state.active]){selectMgmtSuggestion(view,key,state.active);return;}
  closeMgmtSearchSuggestions(view,key);updateMgmtToolbarFilter(view,key,input.value);
}
function selectMgmtSuggestion(view,key,index){
  const state=mgmtSuggestionState[view]&&mgmtSuggestionState[view][key],item=state&&state.items[index],ids=mgmtSearchIds(view,key),input=document.getElementById(ids.inputId);if(!item||!input)return;
  input.value=item.value;toggleMgmtSearchClear(view,key,input);closeMgmtSearchSuggestions(view,key);updateMgmtToolbarFilter(view,key,item.value);input.focus();
}
function clearMgmtSearchInput(view,key){
  const ids=mgmtSearchIds(view,key),input=document.getElementById(ids.inputId);if(input)input.value='';
  toggleMgmtSearchClear(view,key,input);closeMgmtSearchSuggestions(view,key);updateMgmtToolbarFilter(view,key,'');if(input)input.focus();
}
function refreshMgmtDatalists(view){
  // 联想候选直接取当前原始报价集合，避免筛选后候选被错误缩窄。
}
function filterMgmtToday(){
  D.mgmtToday=sortTodayQuoteRows(applyScopeFilters(applyMgmtToolbarFilters(D.mgmtTodayAll||[],'today'),'mgmt'));
  updateMgmtTodayStatusbar();
}
function syncMgmtToday(){
  D.mgmtTodayAll=(D.liveQuotes||[]).filter(function(r){return isQuoteCreatedToday(r)&&(r.status==='active'||r.status==='revoked');}).map(buildMgmtTodayRow);
  refreshMgmtDatalists('today');filterMgmtToday();
}
let mgmtTodayRequestSequence=0,appliedMgmtTodayRevision=0;
function loadMgmtToday(options){
  if(!currentUser||!authToken)return Promise.resolve(false);
  const generation=authSessionGeneration,requestSequence=++mgmtTodayRequestSequence,minimumRevision=Number(options&&options.minimumRevision||0);
  return authFetch(API_BASE+'/quotes/state?view=today&scope=all').then(r=>r.ok?r.json():Promise.reject(new Error('HTTP '+r.status))).then(d=>{
    if(!d.success||generation!==authSessionGeneration)return false;
    if(requestSequence!==mgmtTodayRequestSequence)return true;
    const revision=Number(d.revision||0);
    if(appliedMgmtTodayRevision&&(!revision||revision<appliedMgmtTodayRevision)||minimumRevision&&(!revision||revision<minimumRevision))return true;
    appliedMgmtTodayRevision=Math.max(appliedMgmtTodayRevision,revision);
    D.mgmtTodayAll=(d.quotes||[]).map(buildMgmtTodayRow);
    refreshMgmtDatalists('today');filterMgmtToday();renderAll();updateBadges();return true;
  }).catch(()=>false);
}
function syncMgmtHistory(){
  D.mgmtHistory=(D.quoteEvents||[]).map(function(event){
    const r=event.snapshot||quoteById(event.quoteId)||{};
    const dir=event.direction||quoteDirectionOf(r),side=quoteSideValues(r,dir);
    const statusMap={quote:'报价',edit:'编辑',revoke:'已撤销',restore:'已恢复'};
    return{eventId:event.eventId||event.quoteId+'-'+event.time,quoteId:event.quoteId,ownerAccountId:r.ownerAccountId,ownerUsername:r.ownerUsername,ownerDisplayName:r.ownerDisplayName,canManage:r.canManage,mode:r.mode||'手工录入',status:event.action==='revoke'?'revoked':event.action==='restore'?'active':(r.status||'active'),rs:statusMap[event.action]||event.action,rc:event.action==='revoke'?'#7d8590':'#3fb950',
      m:r.m,c:r.c,n:r.n,direction:dir,bp:dir==='bid'?(event.price||side.price):'',op:dir==='ofr'?(event.price||side.price):'',bv:event.volume||side.volume||'',
      inst:r.counterpartyInstitution||side.institution||r.inst,trader:r.counterpartyTrader||side.trader||r.trader,gwtrader:r.ownerDisplayName||r.tr,
      oco:!!r.oco,packing:!!r.packing,urgent:!!r.urgent,recommend:!!r.recommend,round:!!r.round,lastPrice:r.lastPrice||'—',bondLib:r.bondLib||'—',r:r.r||'—',br:r.br||'—',
      nzNet:r.nzNet||'—',nzDev:r.nzDev||'—',nzYTM:r.nzYTM||'—',csNet:r.csNet||'—',csFull:r.csFull||'—',csYTM:r.csYTM||'—',t:event.time||'',priceType:r.priceType||event.priceType||'',cs:r.cs||r.settlement||'',market:r.market||''};
  });
  refreshMgmtDatalists('history');filterMgmtHistory();
}
function updateMgmtHistoryStatusbar(){
  const el=document.getElementById('mgmtHistoryStatusbar');
  if(!el)return;
  var startEl=document.getElementById('historyStartDate');
  var endEl=document.getElementById('historyEndDate');
  var start=startEl?startEl.value:'';
  var end=endEl?endEl.value:'';
  var filtered=D.mgmtHistoryFiltered||D.mgmtHistory;
  var total=filtered.length;
  var dateInfo=(start||end)?' | 日期: '+(start||'...')+' ~ '+(end||'...'):'';
  el.innerHTML='<span>共'+total+'条</span>'+(dateInfo?'<span class="sep">|</span><span>'+dateInfo+'</span>':'');
}
function filterMgmtHistory(){
  var startEl=document.getElementById('historyStartDate');
  var endEl=document.getElementById('historyEndDate');
  var start=startEl?startEl.value:'';
  var end=endEl?endEl.value:'';
  D.mgmtHistoryFiltered=applyScopeFilters(applyMgmtToolbarFilters(D.mgmtHistory.filter(function(r){
    var d=r.t?r.t.split(' ')[0]:'';
    if(start&&d<start)return false;
    if(end&&d>end)return false;
    return true;
  }),'history'),'mgmt');
  renderTable('mgmtHistoryBody',D.mgmtHistoryFiltered,[
    {key:'mode',render:r=>{const cls=r.mode==='QT'?'firm':r.mode==='ideal'?'nonfirm':'tag-bid';return`<span class="tag tag-${cls}" style="font-size:13px">${r.mode}</span>`;}},
    {key:'m',align:'left'},{key:'c',type:'code',align:'left'},{key:'n',align:'left'},
    {key:'direction',align:'left',render:renderQuoteDirection},
    {key:'bp',align:'left',render:r=>{const dir=quoteDirectionOf(r),value=dir==='ofr'?r.op:r.bp;return(value?renderQuotePriceSpan(r,value,dir,'','left'):'<span class="muted">—</span>')+renderInlineQuoteTags(r);}},
    {key:"bv",align:"left",render:r=>`<span style="color:#22d3ee;font-weight:600;font-family:'JetBrains Mono',monospace">${r.bv||'—'}</span>`},{key:'operations',render:renderQuoteOperationTags},{key:'inst'},{key:'trader'},{key:'gwtrader'},
    {key:'lastPrice',render:r=>`<span class="muted">${r.lastPrice||'—'}</span>`},
    {key:'bondLib'},{key:'r'},{key:'br'},
    {key:'nzNet',render:r=>`<span class="muted">${r.nzNet||'—'}</span>`},
    {key:'nzDev',render:r=>{const v=parseFloat(r.nzDev);return`<span style="color:${Math.abs(v)>16?'#ef4444':'#c9d1d9'};font-family:'JetBrains Mono',monospace">${r.nzDev||'—'}</span>`;}},
    {key:'nzYTM',render:r=>`<span class="muted">${r.nzYTM||'—'}</span>`},
    {key:'csNet',render:r=>`<span class="muted">${r.csNet||'—'}</span>`},
    {key:'csFull',render:r=>`<span class="muted">${r.csFull||'—'}</span>`},
    {key:'csYTM',render:r=>`<span class="muted">${r.csYTM||'—'}</span>`},
    {key:'t',type:'muted'},
  ],{
    rowKey:r=>r.eventId,
    rowSelected:r=>mgmtHistorySelected.has(String(r.eventId)),
  });
  fillEmptyRows('mgmtHistoryBody');
  syncMgmtSelectionClasses();
  updateMgmtHistoryStatusbar();
}
function updateMgmtToolbarFilter(view,key,value){
  if(!mgmtToolbarState[view]||!(key in mgmtToolbarState[view]))return;
  mgmtToolbarState[view][key]=String(value==null?'':value).trim();
  if(view==='today'){
    mgmtTodaySelected.clear();filterMgmtToday();renderAll();
  }else{
    mgmtHistorySelected.clear();filterMgmtHistory();
  }
}
function resetMgmtToolbarFilters(render){
  Object.keys(mgmtToolbarState).forEach(view=>{
    Object.keys(mgmtToolbarState[view]).forEach(key=>{mgmtToolbarState[view][key]='';});
    ['bond','institution','gwTrader'].forEach(key=>{
      const ids=mgmtSearchIds(view,key),input=document.getElementById(ids.inputId);if(input)input.value='';
      toggleMgmtSearchClear(view,key,input);closeMgmtSearchSuggestions(view,key);
    });
  });
  ['mgmtTodayModeFilter','mgmtTodayStatusFilter','mgmtHistoryModeFilter','mgmtHistoryStatusFilter'].forEach(id=>{const control=document.getElementById(id);if(control)control.value='';});
  resetMgmtHistoryDates();mgmtTodaySelected.clear();mgmtHistorySelected.clear();
  if(render!==false){filterMgmtToday();syncMgmtHistory();renderAll();}
}
function fillEmptyRows(tbodyId){
  const tbody=document.getElementById(tbodyId);
  if(!tbody)return;
  const wrap=tbody.closest('.tbl-wrap');
  if(!wrap)return;
  const rowH=32, headerH=35, statusBarH=36;
  const availH=wrap.clientHeight-headerH-statusBarH;
  const existingRows=tbody.querySelectorAll('tr').length;
  const usedH=existingRows*rowH;
  const remain=availH-usedH;
  if(remain<=0)return;
  const thead=tbody.closest('table').querySelector('thead');
  const cols=thead?thead.querySelector('tr').children.length:17;
  let html='';
  for(let i=0;i<Math.floor(remain/rowH);i++){
    html+='<tr class="placeholder-row" aria-hidden="true"><td colspan="'+cols+'" style="border-bottom:1px solid #21262d;height:'+rowH+'px"></td></tr>';
  }
  tbody.insertAdjacentHTML('beforeend',html);
}
function updateStatusbar(tbodyId,totalId,bidId,ofrId,matchId){
  const dataMap={
    boardCreditAllBody:D.boardCreditAll,
    boardCreditBestBody:D.boardCreditBest,
    boardRateAllBody:D.boardRateAll,
    boardRateBestBody:D.boardRateBest,
    boardAlertBody:D.boardAlert
  };
  const data=dataMap[tbodyId];
  if(!data)return;
  let bidCnt=0,ofrCnt=0,matchCnt=0;
  const bondMap={};
  data.forEach(r=>{
    if(r.bidQuoteId||String(r.direction||'').toLowerCase()==='bid'||(r.bp&&r.bp!==''))bidCnt++;
    if(r.ofrQuoteId||String(r.direction||'').toLowerCase()==='ofr'||(r.op&&r.op!==''))ofrCnt++;
    const code=r.c;
    if(!bondMap[code])bondMap[code]={hasBid:false,hasOfr:false};
    if(r.bidQuoteId||String(r.direction||'').toLowerCase()==='bid'||(r.bp&&r.bp!==''))bondMap[code].hasBid=true;
    if(r.ofrQuoteId||String(r.direction||'').toLowerCase()==='ofr'||(r.op&&r.op!==''))bondMap[code].hasOfr=true;
  });
  Object.values(bondMap).forEach(v=>{if(v.hasBid&&v.hasOfr)matchCnt++;});
  if(totalId)document.getElementById(totalId).textContent=data.length;
  if(bidId)document.getElementById(bidId).textContent=bidCnt;
  if(ofrId)document.getElementById(ofrId).textContent=ofrCnt;
  if(matchId)document.getElementById(matchId).textContent=matchCnt;
}

let quoteSelected = new Set();
let quoteSeenIds = new Set();
let quoteManualDeselectedIds = new Set();
let editingCell = null;
const pendingQuoteDrafts=new Map();
let pendingDraftMergeEnabled=true;
const boardIntentDrafts=new Map();
let activeBoardIntentEditId=null;
let boardRefreshBusy=false;
let quoteConfirmEventSequence=0;
let quoteConfirmPollTimer=null;
let quoteConfirmPollBusy=false;

function quoteSelectionId(row,index){return row&&row.recognitionId?row.recognitionId:'legacy-'+index;}
function selectedQuoteRows(){return D.quoteConfirm.filter((row,index)=>quoteSelected.has(quoteSelectionId(row,index)));}
function formatInboundClock(value){const date=new Date(value);return Number.isNaN(date.getTime())?String(value||'—'):date.toLocaleTimeString('zh-CN',{hour:'2-digit',minute:'2-digit',second:'2-digit'});}
function updatePendingQuoteStats(){
  const stats=D.quoteConfirmStats||{};
  const set=(id,value)=>{const el=document.getElementById(id);if(el)el.textContent=value;};
  set('pendingBatchCount',stats.batchCount||0);set('pendingQuoteCount',stats.quoteCount||0);
  set('pendingSourceStats','IDEAL: '+(stats.idealBatchCount||0)+'组 | QT: '+(stats.qtBatchCount||0)+'组');
  const directions=document.getElementById('pendingDirectionStats');if(directions)directions.innerHTML='<span style="color:#3b82f6">Ofr: '+(stats.ofrCount||0)+'</span> | <span style="color:#f59e0b">Bid: '+(stats.bidCount||0)+'</span>';
}
async function loadQuoteConfirmBatches(showError){
  if(!currentUser||!authToken)return false;
  const focused=capturePendingQuoteFocus();
  try{
    const response=await authFetch(API_BASE+'/quote-confirm/batches?status=pending');
    const body=await response.json();if(!response.ok)throw new Error(body.detail||'HTTP '+response.status);
    pendingDraftMergeEnabled=body.draftMergeEnabled!==false;
    D.quoteConfirmBatches=body.batches||[];D.quoteConfirmStats=body.stats||{};
    const serverRows=D.quoteConfirmBatches.flatMap(batch=>(batch.quotes||[]).map(row=>({...row,batchStatus:batch.status,batchCustomerName:batch.customerName,batchCustomerTraderName:batch.customerTraderName,batchOwnerDisplayName:batch.ownerDisplayName,batchOwnerUsername:batch.ownerUsername,batchRawText:batch.rawText})));
    const serverIds=new Set(serverRows.map(row=>String(row.recognitionId||'')));
    pendingQuoteDrafts.forEach((draft,id)=>{if(!serverIds.has(id))pendingQuoteDrafts.delete(id);});
    D.quoteConfirm=serverRows.map(row=>mergePendingQuoteDraft(row));
    D.quoteConfirm.forEach((row,index)=>{row._idx=index; if(row.cbYtm||row.cbYte||row.nzVal||row.nzPrice) applyNzDevFields(row);});
    const activeIds=new Set(D.quoteConfirm.map(quoteSelectionId));
    // P0: 新到达且未手工取消的ID自动选中
    activeIds.forEach(id=>{if(!quoteSeenIds.has(id)&&!quoteManualDeselectedIds.has(id))quoteSelected.add(id);});
    // 清理不再可见的ID
    quoteSeenIds.forEach(id=>{if(!activeIds.has(id)){quoteSeenIds.delete(id);quoteManualDeselectedIds.delete(id);quoteSelected.delete(id);}});
    activeIds.forEach(id=>quoteSeenIds.add(id));
    quoteSelected=new Set(Array.from(quoteSelected).filter(id=>activeIds.has(id)));
    quoteConfirmEventSequence=Math.max(quoteConfirmEventSequence,Number(body.lastEventSequence||0));
    const state=document.getElementById('pendingSyncState');if(state){state.textContent='实时同步正常';state.style.color='#3fb950';}
    renderQuoteGrouped();restorePendingQuoteFocus(focused);updatePendingQuoteStats();updateBadges();return true;
  }catch(error){
    const state=document.getElementById('pendingSyncState');if(state){state.textContent='连接中断，自动重试';state.style.color='#ef4444';}
    if(showError)showToast('error',error.message||'待确认报价加载失败');return false;
  }
}
async function pollQuoteConfirmChanges(){
  if(quoteConfirmPollBusy||!currentUser||!authToken)return;
  quoteConfirmPollBusy=true;
  try{
    const response=await authFetch(API_BASE+'/quote-confirm/changes?since='+quoteConfirmEventSequence);
    const body=await response.json();if(!response.ok)throw new Error(body.detail||'HTTP '+response.status);
    quoteConfirmEventSequence=Math.max(quoteConfirmEventSequence,Number(body.lastEventSequence||0));
    const events=body.events||[];
    const endOfDay=events.some(event=>event.type==='end_of_day.completed'||event.type==='quote_batch.end_of_day_expired');
    if(events.length)await loadQuoteConfirmBatches(false);
    if(endOfDay){
      D.boardAlert=[];alertViewedCount=0;quoteSelected.clear();pendingQuoteDrafts.clear();
      await loadQuoteState();
      updateBadges();
    }
    else{const state=document.getElementById('pendingSyncState');if(state){state.textContent='实时同步正常';state.style.color='#3fb950';}}
  }catch(error){const state=document.getElementById('pendingSyncState');if(state){state.textContent='连接中断，自动重试';state.style.color='#ef4444';}}
  finally{quoteConfirmPollBusy=false;}
}
async function mockQtQuoteDownlink(){
  const button=document.getElementById('quoteConfirmMockBtn');
  if(!authToken){showToast('error','请先登录');return;}
  if(button&&button.disabled)return;
  if(button){button.disabled=true;button.textContent='下行中...';}
  try{
    const response=await authFetch(API_BASE+'/quote-confirm/mock',{method:'POST',body:JSON.stringify({count:5})});
    let body={};try{body=await response.json();}catch(error){}
    if(!response.ok||!body.success)throw new Error(body.detail&&body.detail.message||body.detail||'QT模拟下行失败');
    await loadQuoteConfirmBatches(false);
    showToast('success','QT已下行 '+Number(body.generatedCount||0)+' 条报价');
  }catch(error){showToast('error',error.message||'QT模拟下行失败');}
  finally{if(button){button.disabled=false;button.textContent='模拟';}}
}
function startQuoteConfirmPolling(){if(quoteConfirmPollTimer)clearInterval(quoteConfirmPollTimer);quoteConfirmPollTimer=setInterval(pollQuoteConfirmChanges,2500);}
function stopQuoteConfirmPolling(){if(quoteConfirmPollTimer)clearInterval(quoteConfirmPollTimer);quoteConfirmPollTimer=null;quoteConfirmPollBusy=false;}

function quoteEntryOwnerUsername(r){return r.ownerUsername||'';}
function canViewQuoteEntry(r){return isAdmin()||!r.ownerAccountId||r.ownerAccountId===currentAccountId();}
function applyQuoteFlagClasses(root){
  if(!root)return;
  const classes={'急':'quote-flag-urgent','荐':'quote-flag-recommend','整':'quote-flag-whole','OCO':'quote-flag-oco','打包':'quote-flag-packing'};
  root.querySelectorAll('.tag-interactive').forEach(tag=>{const cls=classes[tag.textContent.trim()];if(cls)tag.classList.add('quote-flag',cls);});
}
const QUOTE_CONFIRM_REQUIRED_FIELDS=[
  {key:'identity',label:'债券代码或简称'},
  {key:'d',label:'方向'},
];
function quoteConfirmHasRequiredValue(value){return value!==null&&value!==undefined&&String(value).trim()!=='';}
function quoteConfirmMissingRequiredFields(row){
  const missing=[];
  if(!quoteConfirmHasRequiredValue(row&&row.c)&&!quoteConfirmHasRequiredValue(row&&row.n))missing.push(QUOTE_CONFIRM_REQUIRED_FIELDS[0]);
  if(!quoteConfirmHasRequiredValue(row&&row.d))missing.push(QUOTE_CONFIRM_REQUIRED_FIELDS[1]);
  return missing;
}
function quoteConfirmFieldErrorClass(row,field){return row&&Array.isArray(row._confirmMissingFields)&&row._confirmMissingFields.includes(field)?'required-missing':'';}
function quoteConfirmCellErrorClass(row,field){return quoteConfirmFieldErrorClass(row,field)?'recognition-required-cell':'';}
function clearQuoteConfirmFieldError(row,field,control){
  if(row&&Array.isArray(row._confirmMissingFields))row._confirmMissingFields=row._confirmMissingFields.filter(key=>key!==field);
  if(control&&control.classList)control.classList.remove('required-missing');
}
function quoteConfirmEditableValue(row,field){
  if(row&&Array.isArray(row._clearedFields)&&row._clearedFields.includes(field))return'';
  const value=row&&row[field];return value===null||value===undefined?'':String(value);
}
function capturePendingQuoteFocus(){
  const active=document.activeElement,control=active&&active.closest?active.closest('[data-quote-field]'):null,tr=control&&control.closest('tr[data-quote-index]');
  if(!control||!tr)return null;
  const row=D.quoteConfirm[Number(tr.dataset.quoteIndex)];if(!row||!row.recognitionId)return null;
  return{recognitionId:String(row.recognitionId),field:control.dataset.quoteField||'',selectionStart:typeof active.selectionStart==='number'?active.selectionStart:null,selectionEnd:typeof active.selectionEnd==='number'?active.selectionEnd:null};
}
function restorePendingQuoteFocus(snapshot){
  if(!snapshot)return;
  requestAnimationFrame(()=>{
    const row=D.quoteConfirm.find(item=>String(item.recognitionId)===snapshot.recognitionId);if(!row)return;
    const tr=document.querySelector('#quoteTableWrap tr[data-quote-index="'+row._idx+'"]'),control=tr&&tr.querySelector('[data-quote-field="'+CSS.escape(snapshot.field)+'"]');if(!control)return;
    const target=control.matches('input,textarea,button')?control:control.querySelector('input,textarea,button');if(!target)return;target.focus();
    if(snapshot.selectionStart!==null&&typeof target.setSelectionRange==='function')target.setSelectionRange(snapshot.selectionStart,snapshot.selectionEnd);
  });
}
function mergePendingQuoteDraft(row){
  if(!pendingDraftMergeEnabled||!row||!row.recognitionId)return row;
  const draft=pendingQuoteDrafts.get(String(row.recognitionId));if(!draft)return row;
  draft.conflict=Number(row.version||1)!==Number(draft.baseVersion||1);
  Object.entries(draft.values||{}).forEach(([field,value])=>{if(draft.dirtyFields.has(field))row[field]=value;});
  row._draftConflict=!!draft.conflict;return row;
}
function setQuoteConfirmRowField(row,field,value,control){
  if(!row)return;
  const normalized=value===null||value===undefined?'':String(value);
  const previous=quoteConfirmEditableValue(row,field);
  row[field]=normalized;
  const cleared=new Set(Array.isArray(row._clearedFields)?row._clearedFields:[]);
  if(normalized==='')cleared.add(field);else cleared.delete(field);
  row._clearedFields=Array.from(cleared);
  if(pendingDraftMergeEnabled&&row.recognitionId&&normalized!==previous){
    const id=String(row.recognitionId),draft=pendingQuoteDrafts.get(id)||{baseVersion:Number(row.version||1),dirtyFields:new Set(),values:{},focusedField:'',selectionStart:null,selectionEnd:null,updatedAt:0,conflict:false};
    draft.dirtyFields.add(field);draft.values[field]=normalized;draft.focusedField=field;draft.updatedAt=Date.now();
    const target=control&&control.matches&&control.matches('input,textarea')?control:null;if(target){draft.selectionStart=target.selectionStart;draft.selectionEnd=target.selectionEnd;}
    pendingQuoteDrafts.set(id,draft);
  }
  clearQuoteConfirmFieldError(row,field,control);
}
const MOCK_QUOTE_CRM_DIRECTORY=[
  {id:'crm-pingan',name:'平安基金',traders:['赵明','王蕾']},
  {id:'crm-chj',name:'创金合信',traders:['丁茂国','李娜']},
  {id:'crm-cmbwm',name:'招银理财',traders:['陈磊','周妍']},
  {id:'crm-ebscn',name:'光大证券',traders:['刘明','孙浩']},
  {id:'crm-citic',name:'中信证券',traders:['张伟','周明哲']},
  {id:'crm-gtja',name:'国泰君安',traders:['陈晨','李杰']},
  {id:'crm-htsc',name:'华泰证券',traders:['王敏','吴昊']},
  {id:'crm-cmbc',name:'招商证券',traders:['赵宇','钱程']},
  {id:'crm-gf',name:'广发证券',traders:['何静','郑凯']},
  {id:'crm-csc',name:'中信建投',traders:['杨帆','徐晨']},
  {id:'crm-ht',name:'海通证券',traders:['宋佳','高远']},
  {id:'crm-cicc',name:'中金公司',traders:['林嘉','冯博']},
];
// 当前由 MOCK 目录提供数据；接入 CRM 后仅替换 provider，不改变三类报价页面的输入交互。
const quoteCrmDirectoryProvider={
  institutions(){return MOCK_QUOTE_CRM_DIRECTORY;},
  traders(institution){
    const name=String(institution||'').trim().toLowerCase();
    const matched=MOCK_QUOTE_CRM_DIRECTORY.find(item=>item.name.toLowerCase()===name);
    const institutions=matched?[matched]:MOCK_QUOTE_CRM_DIRECTORY;
    return institutions.flatMap(item=>item.traders.map(trader=>({name:trader,institution:item.name,institutionId:item.id})));
  },
};
let crmSuggestionState={input:null,kind:'',institution:'',items:[],activeIndex:-1};
const crmSuggestionBindings=new WeakSet();
function crmSuggestionPanel(){
  let panel=document.getElementById('crmSuggestionPanel');
  if(!panel){
    panel=document.createElement('div');panel.id='crmSuggestionPanel';panel.className='crm-suggest-panel';
    panel.setAttribute('role','listbox');panel.addEventListener('mousedown',event=>event.preventDefault());
    document.body.append(panel);
  }
  return panel;
}
function crmSuggestionOptions(kind,institution){
  if(kind==='institution')return quoteCrmDirectoryProvider.institutions().map(item=>({value:item.name,search:item.name+' '+item.id}));
  const seen=new Set();
  return quoteCrmDirectoryProvider.traders(institution).filter(item=>{
    const key=item.name+'|'+item.institution;if(seen.has(key))return false;seen.add(key);return true;
  }).map(item=>({value:item.name,search:item.name+' '+item.institution+' '+item.institutionId}));
}
function positionCrmSuggestionPanel(){
  const panel=crmSuggestionPanel(),input=crmSuggestionState.input;
  if(!input||!input.isConnected||!panel.classList.contains('show'))return;
  const rect=input.getBoundingClientRect(),width=Math.max(1,Math.min(rect.width,innerWidth-12));
  panel.style.width=width+'px';panel.style.left=Math.max(6,Math.min(innerWidth-width-6,rect.left))+'px';
  const below=innerHeight-rect.bottom-6,above=rect.top-6;
  panel.style.maxHeight=Math.max(96,Math.min(260,Math.max(below,above)))+'px';
  panel.style.top=(below>=Math.min(180,above)?rect.bottom+3:Math.max(6,rect.top-panel.offsetHeight-3))+'px';
}
function closeCrmSuggestions(){
  const panel=document.getElementById('crmSuggestionPanel');if(panel)panel.classList.remove('show');
  crmSuggestionState={input:null,kind:'',institution:'',items:[],activeIndex:-1};
}
function selectCrmSuggestion(index){
  const state=crmSuggestionState,item=state.items[index];if(!state.input||!item)return;
  state.input.value=item.value;
  state.input.dispatchEvent(new Event('input',{bubbles:true}));
  state.input.dispatchEvent(new Event('change',{bubbles:true}));
  closeCrmSuggestions();state.input.focus();
}
function renderCrmSuggestions(){
  const state=crmSuggestionState,panel=crmSuggestionPanel();if(!state.input)return;
  const query=String(state.input.value||'').trim().toLowerCase();
  state.items=crmSuggestionOptions(state.kind,state.institution).filter(item=>!query||item.search.toLowerCase().includes(query)).slice(0,12);
  state.activeIndex=state.items.length?0:-1;panel.replaceChildren();
  if(!state.items.length){
    const empty=document.createElement('div');empty.className='crm-suggest-empty';empty.textContent='未找到匹配的 CRM 数据';panel.append(empty);
  }else state.items.forEach((item,index)=>{
    const button=document.createElement('button');button.type='button';button.className='crm-suggest-item'+(index===state.activeIndex?' active':'');
    button.setAttribute('role','option');button.innerHTML='<span class="crm-suggest-main"></span>';
    button.querySelector('.crm-suggest-main').textContent=item.value;
    button.addEventListener('mouseenter',()=>{state.activeIndex=index;panel.querySelectorAll('.crm-suggest-item').forEach((node,i)=>node.classList.toggle('active',i===index));});
    button.addEventListener('click',()=>selectCrmSuggestion(index));panel.append(button);
  });
  panel.classList.add('show');requestAnimationFrame(positionCrmSuggestionPanel);
}
function bindCrmSuggestionInput(input){
  if(!input||crmSuggestionBindings.has(input))return;
  crmSuggestionBindings.add(input);
  input.addEventListener('input',()=>{if(crmSuggestionState.input===input)renderCrmSuggestions();});
  input.addEventListener('keydown',event=>{
    if(crmSuggestionState.input!==input)return;
    if(event.key==='ArrowDown'||event.key==='ArrowUp'){
      if(!crmSuggestionState.items.length)return;event.preventDefault();
      const step=event.key==='ArrowDown'?1:-1;
      crmSuggestionState.activeIndex=(crmSuggestionState.activeIndex+step+crmSuggestionState.items.length)%crmSuggestionState.items.length;
      crmSuggestionPanel().querySelectorAll('.crm-suggest-item').forEach((node,index)=>node.classList.toggle('active',index===crmSuggestionState.activeIndex));
      const active=crmSuggestionPanel().querySelector('.crm-suggest-item.active');if(active)active.scrollIntoView({block:'nearest'});
    }else if(event.key==='Enter'&&crmSuggestionState.activeIndex>=0){event.preventDefault();event.stopPropagation();selectCrmSuggestion(crmSuggestionState.activeIndex);}
    else if(event.key==='Escape'){event.preventDefault();event.stopPropagation();closeCrmSuggestions();}
  });
  input.addEventListener('blur',()=>setTimeout(()=>{if(crmSuggestionState.input===input)closeCrmSuggestions();},80));
}
function openCrmSuggestions(input,kind,institution){
  if(!input)return;bindCrmSuggestionInput(input);
  crmSuggestionState={input,kind,institution:String(institution||''),items:[],activeIndex:-1};
  renderCrmSuggestions();
}
function prepareCrmInstitutionSearch(input){openCrmSuggestions(input,'institution','');}
function prepareCrmTraderSearch(input,institution){openCrmSuggestions(input,'trader',institution);}
document.addEventListener('pointerdown',event=>{const panel=document.getElementById('crmSuggestionPanel');if(crmSuggestionState.input&&event.target!==crmSuggestionState.input&&(!panel||!panel.contains(event.target)))closeCrmSuggestions();});
window.addEventListener('resize',positionCrmSuggestionPanel);
document.addEventListener('scroll',positionCrmSuggestionPanel,true);
const QUOTE_BATCH_INPUT_IDS={
  confirm:['bpInput','amountInput','institutionInput','traderInput'],
  precise:['preciseBpInput','preciseAmountInput','preciseInstitutionInput','preciseTraderInput'],
  fuzzy:['fuzzyBatchInstInput','fuzzyBatchTraderInput'],
};
function clearQuoteBatchInputs(scope){
  (QUOTE_BATCH_INPUT_IDS[scope]||[]).forEach(id=>{const input=document.getElementById(id);if(input)input.value='';});
  if(crmSuggestionState.input&&(QUOTE_BATCH_INPUT_IDS[scope]||[]).includes(crmSuggestionState.input.id))closeCrmSuggestions();
}
const CLIENT_PRICE_TYPE_OPTIONS=[{value:'到期收益率',label:'到期收益率'},{value:'行权收益率',label:'行权收益率'},{value:'净价',label:'净价'},{value:'全价',label:'全价'}];
const CLIENT_SETTLEMENT_OPTIONS=[{value:'T+0',label:'T+0'},{value:'T+1',label:'T+1'}];
const CLIENT_PRECISE_SETTLEMENT_OPTIONS=[...CLIENT_SETTLEMENT_OPTIONS,{value:'T+2',label:'T+2'},{value:'T+3',label:'T+3'}];
const CLIENT_DIRECTION_OPTIONS=[{value:'Bid',label:'Bid'},{value:'Ofr',label:'Ofr'}];
function clientSelectHtml(value,options,handler,opts){
  const config=opts||{},current=String(value==null?'':value),selected=options.find(option=>String(option.value)===current),label=selected?selected.label:(config.placeholder||'请选择');
  const className='client-select '+(config.className||'client-select-row'),triggerClass=selected?'client-select-trigger':'client-select-trigger client-select-placeholder';
  const menu=options.map(option=>'<button type="button" class="client-select-option" data-value="'+escapeQuoteHtml(option.value)+'" data-label="'+escapeQuoteHtml(option.label)+'" onclick="clientSelectChoose(this,event)">'+escapeQuoteHtml(option.label)+'</button>').join('');
  return '<div class="'+className+'" data-client-select data-onchange="'+escapeQuoteHtml(handler||'')+'"><button type="button" class="'+triggerClass+'" data-value="'+escapeQuoteHtml(current)+'" aria-haspopup="listbox" aria-expanded="false" onclick="clientSelectToggle(this,event)">'+escapeQuoteHtml(label)+'</button><div class="client-select-menu" role="listbox">'+menu+'</div></div>';
}
function upgradeClientSelects(root){
  const scope=root||document;
  scope.querySelectorAll('select.client-select-source').forEach(select=>{
    if(!select.parentNode)return;
    const options=[...select.options].map(option=>({value:option.value,label:option.textContent}));
    const current=select.dataset.clientValue!=null?select.dataset.clientValue:select.value;
    const handler=select.getAttribute('onchange')||'';
    const cls=select.dataset.clientClass||'client-select-row';
    const host=document.createElement('div');host.innerHTML=clientSelectHtml(current,options,handler,{className:cls});
    const replacement=host.firstElementChild;
    if(!replacement)return;
    replacement.value=current;
    if(select.dataset.quoteField)replacement.dataset.quoteField=select.dataset.quoteField;
    if(select.dataset.recField)replacement.dataset.recField=select.dataset.recField;
    [...select.classList].filter(name=>name!=='client-select-source').forEach(name=>replacement.classList.add(name));
    select.replaceWith(replacement);
  });
}
function clientSelectCloseAll(except){
  document.querySelectorAll('.client-select.open').forEach(host=>{if(host!==except){host.classList.remove('open');const trigger=host.querySelector('.client-select-trigger');if(trigger)trigger.setAttribute('aria-expanded','false');}});
}
function clientSelectToggle(trigger,event){
  event.stopPropagation();const host=trigger.closest('.client-select');if(!host)return;
  const opening=!host.classList.contains('open');clientSelectCloseAll(host);host.classList.toggle('open',opening);trigger.setAttribute('aria-expanded',opening?'true':'false');
  if(opening){const option=host.querySelector('.client-select-option');if(option)option.classList.add('active');}
}
function clientSelectChoose(option,event){
  event.stopPropagation();const host=option.closest('.client-select');if(!host)return;
  const trigger=host.querySelector('.client-select-trigger'),value=option.dataset.value||'',handler=host.dataset.onchange||'';
  host.value=value;trigger.dataset.value=value;trigger.textContent=option.dataset.label||value;trigger.classList.remove('client-select-placeholder');
  host.classList.remove('open');trigger.setAttribute('aria-expanded','false');
  if(handler){try{const executable=handler.replace(/\bthis\.value\b/g,'value').replace(/\bthis\b/g,'control');new Function('value','control',executable)(value,host);}catch(error){console.error('[client-select] change failed',error);}}
}
document.addEventListener('keydown',event=>{
  const host=event.target&&event.target.closest?event.target.closest('.client-select'):null;if(!host||!host.classList.contains('open'))return;
  const options=[...host.querySelectorAll('.client-select-option')],active=options.indexOf(document.activeElement);
  if(event.key==='Escape'){event.preventDefault();clientSelectCloseAll();host.querySelector('.client-select-trigger').focus();return;}
  if(event.key==='ArrowDown'||event.key==='ArrowUp'){event.preventDefault();const next=(active+(event.key==='ArrowDown'?1:-1)+options.length)%options.length;options.forEach((item,index)=>item.classList.toggle('active',index===next));options[next].focus();return;}
  if(event.key==='Enter'&&document.activeElement.classList.contains('client-select-option')){event.preventDefault();clientSelectChoose(document.activeElement,event);}
});
document.addEventListener('pointerdown',event=>{if(!event.target.closest||!event.target.closest('.client-select'))clientSelectCloseAll();});
function sanitizeBatchNumericText(value){
  const raw=String(value==null?'':value),negative=raw.trimStart().startsWith('-');
  let body=raw.replace(/-/g,'').replace(/[^0-9.]/g,'');
  const dot=body.indexOf('.');if(dot>=0)body=body.slice(0,dot+1)+body.slice(dot+1).replace(/\./g,'');
  return(negative?'-':'')+body;
}
function sanitizeBatchNumericInput(input,resetState=true){if(input){input.value=sanitizeBatchNumericText(input.value);if(resetState){delete input.dataset.batchLastAction;delete input.dataset.batchButtonMode;}}}
function advanceBatchButtonInput(input,action,step){
  if(!input)return step;
  const current=Number(sanitizeBatchNumericText(input.value)),last=input.dataset.batchLastAction||'';
  const magnitude=Number.isFinite(current)&&last===action?Math.abs(current)+step:step;
  input.value=(action==='minus'?'-':'')+String(magnitude);
  input.dataset.batchLastAction=action;input.dataset.batchButtonMode='1';
  return action==='minus'?-step:step;
}
function batchNumericDelta(input,action,fallback){
  const raw=sanitizeBatchNumericText(input&&input.value),parsed=raw!==''&&raw!=='-'&&raw!=='.'&&raw!=='-.'?Number(raw):NaN;
  let value=Number.isFinite(parsed)?parsed:(action==='auto'?NaN:fallback);
  if(!Number.isFinite(value)||value===0)return null;
  if(action==='plus')value=Math.abs(value);else if(action==='minus')value=-Math.abs(value);
  return value;
}
function handleBatchNumericKeydown(event,inputId,kind){
  const input=event.currentTarget||document.getElementById(inputId),hasNumber=input&&Number.isFinite(Number(input.value))&&input.value.trim()!=='';
  let action='';
  if(event.key==='Enter')action='auto';
  else if(event.key==='+')action='plus';
  else if(event.key==='-'&&hasNumber)action='minus';
  if(!action)return;
  event.preventDefault();if(kind==='price')adjustBP(inputId,action);else adjustAmount(inputId,action);
}
function entryPriceCache(row){if(!row._priceValuesByType)row._priceValuesByType={};return row._priceValuesByType;}
function setEntryPrice(row,value,setter,control){
  setter(row,'p',value,control);const type=String(row.pt||'').trim();if(type)entryPriceCache(row)[type]=String(value==null?'':value);
}
function switchEntryPriceType(row,nextType,setter,control){
  if(!row)return;
  const current=String(row.pt||'').trim(),next=String(nextType||'').trim(),cache=entryPriceCache(row),cachedTarget=cache[next];
  if(current===next){setter(row,'pt',next,control);return;}
  const previousValue=String(row.p==null?'':row.p).trim();
  if(current)cache[current]=String(row.p==null?'':row.p);
  setter(row,'pt',next,control);
  // A price-type change is a semantic conversion, not a preference toggle.
  // Clear the target immediately so a failed calculator call can never leave a stale value.
  setEntryPrice(row,'',setter);
  applyNzDevFields(row);
  if(cachedTarget!==undefined&&cachedTarget!==null&&String(cachedTarget)!==''){
    setEntryPrice(row,cachedTarget,setter);
    applyNzDevFields(row);
    return Promise.resolve(true);
  }
  if(!previousValue||!row.c)return Promise.resolve(false);
  const render=setter===setQuoteConfirmRowField?renderQuoteGrouped:renderPreciseTable;
  return authFetch(API_BASE+'/quotes/price-conversion',{method:'POST',body:JSON.stringify({securityId:row.c,quoteValue:previousValue,priceType:current||'到期收益率',targetPriceType:next,direction:String(row.d||row.direction||'bid').toLowerCase(),tradeDate:row.date||row.tradeDate||'',settlementType:row.cs||''})})
    .then(async response=>{const body=await response.json().catch(()=>({}));if(!response.ok||body.success===false)throw new Error(body.message||body.detail||'债券价格换算失败');return body;})
    .then(body=>{
      const code=normalizePriceTypeCode(next),value=code==='YTM'||code==='YTC'?body.yieldValue:code==='CLEAN_PRICE'?body.cleanPrice:body.fullPrice;
      if(value===null||value===undefined||value==='')throw new Error(body.message||'债券计算器未返回目标价格');
      setEntryPrice(row,Number(value).toFixed(4),setter);applyNzDevFields(row);render();return true;
    })
    .catch(error=>{setEntryPrice(row,'',setter);applyNzDevFields(row);render();showToast('error',error.message||'债券价格换算失败');return false;});
}
function setQuoteConfirmPrice(row,value,control){setEntryPrice(row,value,setQuoteConfirmRowField,control);}
function switchQuoteConfirmPriceType(row,value,control){switchEntryPriceType(row,value,setQuoteConfirmRowField,control);}
function setRecognitionPrice(row,value,control){setEntryPrice(row,value,setRecognitionRowField,control);}
function switchRecognitionPriceType(row,value,control){switchEntryPriceType(row,value,setRecognitionRowField,control);}
function enforceQuoteTradeDate(row,value,setter,control){
  const today=quoteToday(),next=value&&value<today?today:value;
  if(control&&control.value!==next)control.value=next;
  setter(row,'date',next,control);
  if(value&&value<today)showToast('error','交易日期不能早于当日，已调整为 '+today);
}
function commitQuoteConfirmRowControls(idx,source){
  const row=D.quoteConfirm&&D.quoteConfirm[idx],tr=source&&source.closest?source.closest('tr'):null;
  if(!row||!tr||!tr.querySelectorAll)return;
  tr.querySelectorAll('[data-quote-field]').forEach(control=>control.dataset.quoteField==='p'?setQuoteConfirmPrice(row,control.value,control):setQuoteConfirmRowField(row,control.dataset.quoteField,control.value,control));
}
function commitQuoteConfirmTableControls(){
  const wrap=document.getElementById('quoteTableWrap');
  if(!wrap||!wrap.querySelectorAll)return;
  wrap.querySelectorAll('tbody tr[data-quote-index]').forEach(tr=>{
    const source=tr.querySelector('[data-quote-field]');
    if(source)commitQuoteConfirmRowControls(Number(tr.dataset.quoteIndex),source);
  });
}
function renderQuoteGrouped(){
  const wrap = document.getElementById('quoteTableWrap');
  if(!wrap) return;
  const groups = {};
  D.quoteConfirm.forEach((r,i)=>{
    if(!canViewQuoteEntry(r,i))return;
    const key = r.batchId||r.inst+'|'+r.src;
    if(!groups[key]) groups[key] = {batchId:key,inst:r.batchCustomerName||r.inst,trader:r.batchCustomerTraderName||r.tr,ownerAccountId:r.ownerAccountId,ownerDisplayName:r.batchOwnerDisplayName||'',ownerUsername:r.batchOwnerUsername||'',src:r.src,srcColor:r.srcColor,sourceSentAt:r.sourceSentAt,receivedAt:r.receivedAt,status:r.batchStatus,rows:[]};
    groups[key].rows.push({...r, _idx:i});
  });

  let html = '<table class="dt quote-confirm-table"><colgroup><col><col><col><col><col><col><col><col><col><col><col><col><col><col><col><col><col><col></colgroup><thead><tr>';
  const visibleCount=D.quoteConfirm.filter((r,i)=>canViewQuoteEntry(r,i)).length;
  html += '<th style="width:28px"><input type="checkbox" id="quoteSelectAll" '+(quoteSelected.size===visibleCount&&visibleCount>0?'checked':'')+' onchange="toggleSelectAll(this.checked)"></th>';
  html += '<th style="text-align:left">期限</th><th style="text-align:left">代码</th><th style="text-align:left">简称</th><th>方向</th><th>报价</th><th>价格类型</th><th>报量(万)</th><th>操作</th><th>报价机构</th><th>报价交易员</th><th>交易日期</th><th>清算速度</th><th>中债估值</th><th>中债净价</th><th>中债偏离</th><th>中证(行/到)</th><th>文本内容</th>';
  html += '</tr></thead><tbody>';

  for(const [key, g] of Object.entries(groups)){
    const srcBg = g.src==='ideal'?'rgba(59,130,246,0.12)':'rgba(63,185,80,0.12)';
    const srcColor = g.src==='ideal'?'#3b82f6':'#3fb950';
    const srcBorder = g.src==='ideal'?'rgba(59,130,246,0.3)':'rgba(63,185,80,0.3)';
    const allChecked = g.rows.every(r=>quoteSelected.has(quoteSelectionId(r,r._idx)));
    const statusLabel={pending_review:'待确认',partial_failed:'部分识别',recognition_failed:'识别失败',partially_confirmed:'部分确认'}[g.status]||g.status||'待确认';
    html += `<tr>
      <td colspan="18" style="padding:6px 10px;cursor:pointer;border-bottom:1px solid #30363d;background:#161b22" onclick="toggleGroup('${key}')">
        <div style="display:flex;align-items:center;gap:8px">
          <input type="checkbox" ${allChecked?'checked':''} onclick="event.stopPropagation();toggleGroupSelect('${key}',this.checked)" style="margin-right:4px">
          <span style="padding:1px 8px;border-radius:3px;font-size:10px;font-weight:600;background:${srcBg};color:${srcColor};border:1px solid ${srcBorder}">${String(g.src||'').toUpperCase()}</span>
          <span style="font-size:13px;font-weight:600;color:#e6edf3">${escapeQuoteHtml(g.inst)}</span>
          <span style="font-size:12px;color:#c9d1d9">${escapeQuoteHtml(g.trader)}</span>
          <span style="font-size:11px;color:#7d8590">${formatInboundClock(g.receivedAt)}</span>
          <span style="font-size:11px;color:#7d8590;margin-left:auto">${g.rows.length}条 · ${escapeQuoteHtml(statusLabel)}</span>
        </div>
      </td>
    </tr>`;

    for(const r of g.rows){
      const direction=quoteConfirmEditableValue(r,'d');
      const bidOfColor = direction === 'Bid' ? '#f59e0b' : direction === 'Ofr' ? '#3b82f6' : '#c9d1d9';
      const selectionId=quoteSelectionId(r,r._idx);
      html += `<tr data-group="${key}" data-quote-index="${r._idx}">
        <td><input type="checkbox" ${quoteSelected.has(selectionId)?'checked':''} onclick="event.stopPropagation();commitQuoteConfirmRowControls(${r._idx},this);toggleRowSelect('${selectionId}',this.checked)"></td>
        <td style="text-align:left" title="${recognitionEscapeHtml(r.m||'')}">${recognitionEscapeHtml(r.m||'')}</td>
        <td class="${quoteConfirmCellErrorClass(r,'identity')}" style="text-align:left" title="${recognitionEscapeHtml(r.c||'')}">${recognitionEscapeHtml(r.c||'')}</td>
        <td class="${quoteConfirmCellErrorClass(r,'identity')}" style="text-align:left" title="${recognitionEscapeHtml(r.n||'')}">${recognitionEscapeHtml(r.n||'')}</td>
        <td class="field-underline"><select data-quote-field="d" data-client-value="${recognitionEscapeHtml(direction)}" data-client-class="client-select-row client-select-direction" class="client-select-source ${quoteConfirmFieldErrorClass(r,'d')}" onchange="const row=D.quoteConfirm[${r._idx}];setQuoteConfirmRowField(row,'d',this.value,this);renderQuoteGrouped()"><option ${direction==='Bid'?'selected':''}>Bid</option><option ${direction==='Ofr'?'selected':''}>Ofr</option></select></td>
        <td class="field-underline ${quoteConfirmCellErrorClass(r,'p')}"><input data-quote-field="p" value="${recognitionEscapeHtml(formattedRecognitionPrice(quoteConfirmEditableValue(r,'p')))}" placeholder="待补" style="color:#d29922;font-family:'JetBrains Mono',monospace;text-align:right;font-weight:700" oninput="this.value=this.value.replace(/[^0-9.]/g,'');const row=D.quoteConfirm[${r._idx}];setQuoteConfirmPrice(row,this.value,this);applyNzDevFields(row)"></td>
        <td class="field-underline ${quoteConfirmCellErrorClass(r,'pt')}"><select data-quote-field="pt" data-client-value="${recognitionEscapeHtml(quoteConfirmEditableValue(r,'pt'))}" data-client-class="client-select-row client-select-price" class="client-select-source" onchange="const row=D.quoteConfirm[${r._idx}];switchQuoteConfirmPriceType(row,this.value,this);renderQuoteGrouped()"><option ${quoteConfirmEditableValue(r,'pt')==='到期收益率'?'selected':''}>到期收益率</option><option ${quoteConfirmEditableValue(r,'pt')==='行权收益率'?'selected':''}>行权收益率</option><option ${quoteConfirmEditableValue(r,'pt')==='净价'?'selected':''}>净价</option><option ${quoteConfirmEditableValue(r,'pt')==='全价'?'selected':''}>全价</option></select></td>
        <td class="field-underline ${quoteConfirmCellErrorClass(r,'vol')}"><input data-quote-field="vol" value="${recognitionEscapeHtml(quoteConfirmEditableValue(r,'vol'))}" placeholder="待补" style="color:#d29922;font-family:'JetBrains Mono',monospace;text-align:right;font-weight:700" oninput="this.value=this.value.replace(/[^0-9.]/g,'');setQuoteConfirmRowField(D.quoteConfirm[${r._idx}],'vol',this.value,this)"></td>
        <td style="white-space:nowrap"><span class="tag-interactive ${r.oco?'on':'off'}" style="background:rgba(121,192,255,0.12);color:#79c0ff;border:1px solid rgba(121,192,255,0.3);padding:1px 5px;border-radius:3px;font-size:10px;font-weight:600" onclick="event.stopPropagation();toggleTag(${r._idx},'oco',this)">OCO</span> <span class="tag-interactive ${r.packing?'on':'off'}" style="background:rgba(210,153,34,0.12);color:#d29922;border:1px solid rgba(210,153,34,0.3);padding:1px 5px;border-radius:3px;font-size:10px;font-weight:600" onclick="event.stopPropagation();toggleTag(${r._idx},'packing',this)">打包</span> <span class="tag-interactive ${r.urgent?'on':'off'}" style="background:rgba(239,68,68,0.12);color:#ef4444;border:1px solid rgba(239,68,68,0.3);padding:1px 5px;border-radius:3px;font-size:10px;font-weight:600" onclick="event.stopPropagation();toggleTag(${r._idx},'urgent',this)">急</span> <span class="tag-interactive ${r.rec?'on':'off'}" style="background:rgba(210,153,34,0.12);color:#d29922;border:1px solid rgba(210,153,34,0.3);padding:1px 5px;border-radius:3px;font-size:10px;font-weight:600" onclick="event.stopPropagation();toggleTag(${r._idx},'rec',this)">荐</span> <span class="tag-interactive ${r.whole?'on':'off'}" style="background:rgba(121,192,255,0.12);color:#79c0ff;border:1px solid rgba(121,192,255,0.3);padding:1px 5px;border-radius:3px;font-size:10px;font-weight:600" onclick="event.stopPropagation();toggleTag(${r._idx},'whole',this)">整</span></td>
        <td class="field-underline quote-party-cell ${quoteConfirmCellErrorClass(r,'inst')}"><input data-quote-field="inst" class="quote-party-input" autocomplete="off" value="${recognitionEscapeHtml(quoteConfirmEditableValue(r,'inst'))}" title="${recognitionEscapeHtml(quoteConfirmEditableValue(r,'inst'))}" style="color:#c9d1d9" onfocus="prepareCrmInstitutionSearch(this)" oninput="setQuoteConfirmRowField(D.quoteConfirm[${r._idx}],'inst',this.value,this)"></td>
        <td class="field-underline quote-party-cell ${quoteConfirmCellErrorClass(r,'tr')}"><input data-quote-field="tr" class="quote-party-input" autocomplete="off" value="${recognitionEscapeHtml(quoteConfirmEditableValue(r,'tr'))}" title="${recognitionEscapeHtml(quoteConfirmEditableValue(r,'tr'))}" style="color:#c9d1d9" onfocus="prepareCrmTraderSearch(this,D.quoteConfirm[${r._idx}].inst)" oninput="setQuoteConfirmRowField(D.quoteConfirm[${r._idx}],'tr',this.value,this)"></td>
        <td class="field-underline ${quoteConfirmCellErrorClass(r,'date')}"><input data-quote-field="date" type="date" class="quote-entry-date" min="${recognitionEscapeHtml(quoteToday())}" value="${recognitionEscapeHtml(quoteConfirmEditableValue(r,'date'))}" style="color:#c9d1d9;width:112px;background:transparent" onchange="enforceQuoteTradeDate(D.quoteConfirm[${r._idx}],this.value,setQuoteConfirmRowField,this)"></td>
        <td class="field-underline ${quoteConfirmCellErrorClass(r,'cs')}"><select data-quote-field="cs" data-client-value="${recognitionEscapeHtml(quoteConfirmEditableValue(r,'cs'))}" data-client-class="client-select-row client-select-settlement" class="client-select-source" onchange="setQuoteConfirmRowField(D.quoteConfirm[${r._idx}],'cs',this.value,this)"><option ${quoteConfirmEditableValue(r,'cs')==='T+0'?'selected':''}>T+0</option><option ${quoteConfirmEditableValue(r,'cs')==='T+1'?'selected':''}>T+1</option></select></td>
        <td style="color:#c9d1d9">${formatChinaBondValuation(r)}</td>
        <td style="color:#c9d1d9">${r.nzPrice||'—'}</td>
        <td class="neg">${formatNzDevDisplay(r)}</td>
        <td style="color:#c9d1d9">${r.csVal||'—'}</td>
        <td class="muted" style="max-width:200px;overflow:hidden;text-overflow:ellipsis" title="${recognitionEscapeHtml(r.txt||'')}">${recognitionEscapeHtml(r.txt||'')}</td>
      </tr>`;
    }
  }
  html += '</tbody></table>';
  wrap.innerHTML = html;
  if(typeof upgradeClientSelects==='function')upgradeClientSelects(wrap);
  applyQuoteFlagClasses(wrap);
  updateSelectedCount();
}

async function startEdit(idx, field){
  const r = D.quoteConfirm[idx];
  if(!r) return;
  let newVal = await systemPrompt('编辑报价',`修改${getFieldLabel(field)}：`,r[field]);
  if(newVal === null) return;
  if(field==='p'||field==='vol') {
    if(isNaN(parseFloat(newVal))) { showToast('error','请输入有效数值'); return; }
    newVal = parseFloat(newVal);
  }
  D.quoteConfirm[idx][field] = newVal;
  renderQuoteGrouped();
}
function getFieldLabel(f){return{c:'债券代码',n:'债券简称',p:'报价',vol:'报量(万)',t:'对手机构',tr:'交易员',cs:'清算速度',m:'剩余期限',q:'到期/行权',txt:'文本内容',dev:'中债偏离'}[f]||f}

function toggleTag(idx, tag, source){
  const r = D.quoteConfirm[idx];
  if(!r) return;
  commitQuoteConfirmRowControls(idx,source);
  if(['oco','packing'].includes(tag))toggleExclusiveQuoteFlag(r,tag);
  else r[tag] = !r[tag];
  renderQuoteGrouped();
}

function toggleGroup(key){
  const rows=document.querySelectorAll('tr[data-group="'+key+'"]');
  rows.forEach(r=>r.classList.toggle('group-hidden'));
}
function toggleGroupSelect(key,checked){
  commitQuoteConfirmTableControls();
  D.quoteConfirm.forEach((r,i)=>{if(canViewQuoteEntry(r,i)&&(r.batchId||r.inst+'|'+r.src)===key){const id=quoteSelectionId(r,i);if(checked){quoteSelected.add(id);quoteManualDeselectedIds.delete(id);}else{quoteSelected.delete(id);quoteManualDeselectedIds.add(id);}}});
  renderQuoteGrouped();
}
function toggleRowSelect(id,checked){if(checked){quoteSelected.add(id);quoteManualDeselectedIds.delete(id);}else{quoteSelected.delete(id);quoteManualDeselectedIds.add(id);}renderQuoteGrouped();}
function updateSelectedCount(){const el=document.getElementById('selectedCount');if(el)el.textContent=quoteSelected.size;}
function toggleSelectAll(checked){
  commitQuoteConfirmTableControls();
  D.quoteConfirm.forEach((r,i)=>{if(canViewQuoteEntry(r,i)){const id=quoteSelectionId(r,i);if(checked){quoteSelected.add(id);quoteManualDeselectedIds.delete(id);}else{quoteSelected.delete(id);quoteManualDeselectedIds.add(id);}}});
  renderQuoteGrouped();
}

// ===== 跨页面数据流转 - 债券类型判断 =====
const RATE_TAGS=['国债','政金债','政策性金融债','地方债','利率债','政策性银行普通债券'];
function classifyBondMarket(code,name,apiInfo,bondNameTag){
  apiInfo=apiInfo||{};
  const bt=Number(apiInfo.bond_type);
  if(Number.isFinite(bt))return bt===1||bt===10?'rate':'credit';
  const text=[bondNameTag,apiInfo.bond_type_desc,name].filter(Boolean).join(' ');
  if(RATE_TAGS.some(tag=>text.includes(tag))||/国开|口行|农发|央票|政策性|进出口银行/.test(text))return'rate';
  const rateKw=['国债','国开','口行','农发','央票','金融债','地方债','政策性金融债','国开债','进出口','同业存单'];
  if(rateKw.some(k=>(name||'').includes(k)))return'rate';
  const creditKw=['短融','中票','企业债','公司债','PPN','ABS','MTN','SCP','CP','二级','永续','城投','地产'];
  if(creditKw.some(k=>(name||'').includes(k)))return'credit';
  return(code||'').toUpperCase().endsWith('.SH')?'credit':'rate';
}
function isCreditBond(code,name,apiInfo,bondNameTag){return classifyBondMarket(code,name,apiInfo,bondNameTag)==='credit';}

// ===== 构建行情中心标准entry =====
function buildBoardEntry(r,src){
  const createdNow=new Date(),now=createdNow.toLocaleTimeString('zh-CN',{hour:'2-digit',minute:'2-digit',second:'2-digit'});
  const sourceTime=Date.parse(r.sourceSentAt||'');
  const quotedAt=Number.isFinite(sourceTime)?new Date(sourceTime).toISOString():createdNow.toISOString();
  const preserveEmpty=src==='precise';
  const preciseValue=(field,fallbackField)=>{
    if(preserveEmpty&&Array.isArray(r._clearedFields)&&r._clearedFields.includes(field))return'';
    const value=r[field]??(fallbackField?r[fallbackField]:'')??'';
    return String(value);
  };
  const counterpartyTrader=preserveEmpty?preciseValue('tr'):(r.counterpartyTrader||r.trader||r.tr||'');
  const loginName=currentUser?currentUser.name:'';
  const inst=preserveEmpty?preciseValue('inst'):(r.counterpartyInstitution||r.inst||'');
  const apiInfo=r._apiInfo||{};
  const bondType=r.bondType||apiInfo.bond_type_desc||'';
  var mode='手工录入';
  if(r.src==='ideal')mode='ideal';
  else if(r.src==='qt')mode='QT';
  const quoteId=r.quoteId||makeQuoteId(src==='precise'?'precise':'quote');
  const entry={quoteId:quoteId,sourceQuoteId:quoteId,sourceType:src||'confirm',status:r.status||'active',ownerAccountId:currentAccountId(),ownerUsername:currentUser?currentUser.username:'',ownerDisplayName:loginName,ownerOrgId:currentUser?currentUser.orgId||'':'',canManage:true,counterpartyInstitution:inst,counterpartyTrader:counterpartyTrader,direction:String(r.d||'').toLowerCase()==='ofr'?'ofr':'bid',m:r.m||'',c:r.c||'',n:r.n||'',bp:'',bv:'',bvBid:'',bvOfr:'',op:'',inst:inst,trader:counterpartyTrader,tr:loginName,
    instBid:'',trBid:'',bvBid:'',instOfr:'',trOfr:'',bvOfr:'',
    lastPrice:'',r:apiInfo.rating||'—',br:apiInfo.bond_rating||'—',t:now,quotedAt:quotedAt,sourceSentAt:r.sourceSentAt||'',
    nzNet:r.nzPrice||r.nzNet||'—',nzDev:r.dev||'—',bondLib:'在库',nzYTM:r.cbYtm||r.nzYTM||'—',
    csNet:r.csNet||'—',csFull:r.csFull||'—',csYTM:r.csYtm||'—',
    bondType:bondType,market:r.market||classifyBondMarket(r.c,r.n,apiInfo,bondType),mode:mode,
    priceType:preserveEmpty?preciseValue('pt','priceType'):(r.pt||r.priceType||'到期收益率'),
    volumeUnit:r.volumeUnit||(/\.(SH|SZ|BJ)$/i.test(r.c||'')?'万面额':'万'),
    tradeDate:preserveEmpty?preciseValue('date','tradeDate'):(r.date||r.tradeDate||quoteToday()),
    cs:preserveEmpty?preciseValue('cs'):(r.cs||'T+1'),
    valuationStatus:r.valuationStatus||'pending',valuationDate:r.valuationDate||'',recognitionId:r.recognitionId||'',
    oco:!!r.oco,packing:!!r.packing,urgent:!!r.urgent,recommend:!!(r.recommend||r.rec),round:!!(r.round||r.whole),note:preserveEmpty?(r.note||''):(r.note||r.txt||'')};
  if(r.d==='Bid'){
    entry.bp=preserveEmpty?preciseValue('p'):r.p||'';
    entry.bv=preserveEmpty?preciseValue('vol'):r.vol||'';
    entry.instBid=inst;
    entry.trBid=loginName;
    entry.bvBid=entry.bv;
  }else{
    entry.op=preserveEmpty?preciseValue('p'):r.p||'';
    entry.bv=preserveEmpty?preciseValue('vol'):r.vol||'';
    entry.instOfr=inst;
    entry.trOfr=loginName;
    entry.bvOfr=entry.bv;
  }
  return entry;
}

// ===== 去重更新（同code同方向更新，不同则新增） =====
function dedupAndUpdate(array,entry){
  const key=entry.c;
  const dir=entry.bp?'bid':'ofr';
  const existingIdx=array.findIndex(x=>x.c===key);
  if(existingIdx>=0){
    const existing=array[existingIdx];
    if(dir==='bid'){
      existing.bp=entry.bp||existing.bp;
      existing.bv=entry.bv||existing.bv;
      existing.instBid=entry.instBid||existing.instBid;
      existing.trBid=entry.trBid||existing.trBid;
      existing.bvBid=entry.bvBid||existing.bvBid;
    }else{
      existing.op=entry.op||existing.op;
      existing.bv=entry.bv||existing.bv;
      existing.instOfr=entry.instOfr||existing.instOfr;
      existing.trOfr=entry.trOfr||existing.trOfr;
      existing.bvOfr=entry.bvOfr||existing.bvOfr;
    }
    existing.t=entry.t;
    return false;
  }
  array.push(entry);
  return true;
}

// ===== 最优判断+推送 =====
function pushIfBest(bestArray,entry,type){
  const dir=entry.bp?'bid':'ofr';
  const price=parseFloat(entry.bp||entry.op);
  const sameBond=bestArray.filter(x=>x.c===entry.c);
  if(dir==='bid'){
    const existingBids=sameBond.filter(x=>x.bp);
    if(isNaN(price))return false;
    if(existingBids.length===0){dedupAndUpdate(bestArray,entry);return false;}
    const minBid=Math.min(...existingBids.map(x=>parseFloat(x.bp)));
    if(price<minBid){
      const idx=bestArray.findIndex(x=>x.c===entry.c&&x.bp);
      if(idx>=0)bestArray.splice(idx,1);
      dedupAndUpdate(bestArray,entry);
      return true;
    }
    return false;
  }else{
    const existingOfrs=sameBond.filter(x=>x.op);
    if(isNaN(price))return false;
    if(existingOfrs.length===0){dedupAndUpdate(bestArray,entry);return false;}
    const maxOfr=Math.max(...existingOfrs.map(x=>parseFloat(x.op)));
    if(price>maxOfr){
      const idx=bestArray.findIndex(x=>x.c===entry.c&&x.op);
      if(idx>=0)bestArray.splice(idx,1);
      dedupAndUpdate(bestArray,entry);
      return true;
    }
    return false;
  }
}

// ===== 统一确认报价核心逻辑 =====
function executeConfirm(dataArray,selectedSet,sourceName){
  if(selectedSet.size===0){showToast('error','请先选择报价数据');return;}
  const selected=Array.from(selectedSet).sort((a,b)=>b-a);
  const creditCount={total:0},rateCount={total:0};
  const triggeredAlerts=[];
  const alertItems={};
  selected.forEach(i=>{
    const r=dataArray[i];
    if(!r)return;
    const apiInfo=r._apiInfo||{};
    const bondNameTag=r.bondType||'';
    const rate=isCreditBond(r.c,r.n,apiInfo,bondNameTag)?false:true;
    const entry=buildBoardEntry(r);
    let isBest=false;
    if(rate){
      rateCount.total++;
      dedupAndUpdate(D.boardRateAll,entry);
      isBest=pushIfBest(D.boardRateBest,entry,'rate');
    }else{
      creditCount.total++;
      dedupAndUpdate(D.boardCreditAll,entry);
      isBest=pushIfBest(D.boardCreditBest,entry,'credit');
    }
    const alertBase={sourceQuoteId:entry.quoteId,quoteId:entry.quoteId,updatedAt:entry.updatedAt||entry.quotedAt||'',m:entry.m,c:entry.c,n:entry.n,
      instBid:entry.instBid,trBid:entry.trBid,bvBid:entry.bvBid,
      bp:entry.bp,op:entry.op,bvOfr:entry.bvOfr,trOfr:entry.trOfr,instOfr:entry.instOfr,
      lastPrice:entry.lastPrice,r:entry.r,br:entry.br,t:entry.t,
      nzNet:entry.nzNet,nzDev:entry.nzDev,bondLib:entry.bondLib,nzYTM:entry.nzYTM,
      csNet:entry.csNet,csFull:entry.csFull,csYTM:entry.csYTM,market:entry.market,cs:entry.cs||entry.settlement||'',priceType:entry.priceType||entry.pt||'到期收益率'};
    const favoriteMatches=D.alertSettings.favorite?matchedFavoriteGroups(entry):[];
    if(favoriteMatches.length){
      dedupAndUpdate(D.boardAlert,{...alertBase,alertType:'关注债券',groupNames:favoriteMatches.map(group=>group.name)});
      if(triggeredAlerts.indexOf('关注债券')<0)triggeredAlerts.push('关注债券');
      if(!alertItems['关注债券'])alertItems['关注债券']=[];
      alertItems['关注债券'].push(alertBase);
    }
    const filterMatches=D.alertSettings.filterGroup?matchedMarketFilterGroups(entry):[];
    if(filterMatches.length){
      dedupAndUpdate(D.boardAlert,{...alertBase,alertType:'筛选分组',groupNames:filterMatches.map(group=>group.name)});
      if(triggeredAlerts.indexOf('筛选分组')<0)triggeredAlerts.push('筛选分组');
      if(!alertItems['筛选分组'])alertItems['筛选分组']=[];
      alertItems['筛选分组'].push(alertBase);
    }
    if(D.alertSettings.priceMatch&&quoteIsMine(entry)){
      const currentBoard=D.boardCreditAll.concat(D.boardRateAll).concat(D.liveQuotes||[]);
      const matchExist=currentBoard.find(x=>x&&!quoteIsMine(x)&&(x.status?x.status==='active':true)&&quotesOppositeForAlert(entry,x));
      if(matchExist){
        const bid=r.d==='Bid'?entry:matchExist;
        const ofr=r.d==='Bid'?matchExist:entry;
        const pairKey='PRICE_MATCH:'+(bid.quoteId||'')+':'+(ofr.quoteId||'');
        const pairAlert={
          ...alertBase,
          alertType:'对价提醒',
          alertId:pairKey,
          pairKey,
          bidQuoteId:bid.quoteId||'',
          ofrQuoteId:ofr.quoteId||'',
          sourceQuoteId:entry.quoteId,
          bp:bid.bp||'',
          bvBid:bid.bvBid||bid.bv||'',
          instBid:bid.instBid||bid.counterpartyInstitution||bid.inst||'',
          trBid:bid.ownerDisplayName||bid.tr||'',
          counterpartyTraderBid:bid.counterpartyTrader||bid.trader||'',
          op:ofr.op||'',
          bvOfr:ofr.bvOfr||ofr.bv||'',
          instOfr:ofr.instOfr||ofr.counterpartyInstitution||ofr.inst||'',
          trOfr:ofr.ownerDisplayName||ofr.tr||'',
          counterpartyTraderOfr:ofr.counterpartyTrader||ofr.trader||'',
          bidOwnerDisplayName:bid.ownerDisplayName||bid.tr||'',
          ofrOwnerDisplayName:ofr.ownerDisplayName||ofr.tr||''
        };
        dedupAndUpdate(D.boardAlert,pairAlert);
        if(triggeredAlerts.indexOf('对价提醒')<0)triggeredAlerts.push('对价提醒');
        if(!alertItems['对价提醒'])alertItems['对价提醒']=[];
        alertItems['对价提醒'].push(pairAlert);
      }
    }
    if(isBest&&D.alertSettings.bestQuote&&triggeredAlerts.indexOf('我的最优')<0)triggeredAlerts.push('我的最优');
  });
  selected.forEach(i=>dataArray.splice(i,1));
  dataArray.forEach((r,i)=>{r._idx=i;});
  selectedSet.clear();
  renderAll();
  syncMgmtToday();
  const total=creditCount.total+rateCount.total;
  if(total>0){
    let msg='确认成功 '+total+' 条';
    if(creditCount.total)msg+='（信用债'+creditCount.total+'条';
    if(rateCount.total)msg+=(creditCount.total?'，':'')+'利率债'+rateCount.total+'条';
    if(creditCount.total||rateCount.total)msg+='）';
    showToast('success',msg);
  }
  if(triggeredAlerts.length>0){
    showAllAlerts(triggeredAlerts,alertItems);
  }
  const alertPage=document.getElementById('page-board');
  const alertTab=document.querySelector('[data-st="board-alert"]');
  const isOnAlertPage=alertPage&&alertPage.classList.contains('active-page')&&alertTab&&alertTab.classList.contains('active');
  if(isOnAlertPage)markAlertsViewed();
  updateBadges();
}

function updateBadges(){
  const quoteBadge=document.getElementById('quoteBadge');
  const boardBadge=document.getElementById('boardBadge');
  const quoteConfirmBadge=document.getElementById('quoteConfirmBadge');
  const alertTabBadge=document.getElementById('alertTabBadge');
  const quoteCount=D.quoteConfirm.length;
  if(quoteBadge){quoteBadge.textContent=quoteCount;quoteBadge.style.display=quoteCount>0?'inline-block':'none';}
  if(quoteConfirmBadge){quoteConfirmBadge.textContent=quoteCount;quoteConfirmBadge.style.display=quoteCount>0?'inline-block':'none';}
  const alertCount=(D.boardAlert||[]).reduce((count,alert)=>count+(alertInstanceKeys(alert).some(key=>!readAlertKeys.has(key))?1:0),0);
  if(boardBadge){boardBadge.textContent=alertCount;boardBadge.style.display=alertCount>0?'inline-block':'none';}
  if(alertTabBadge){alertTabBadge.textContent=alertCount;alertTabBadge.style.display=alertCount>0?'inline-block':'none';}
}
function markAlertsViewed(){
  alertViewedCount=D.boardAlert.length;
  const keys=Array.from(new Set((D.boardAlert||[]).flatMap(alertInstanceKeys)));
  markAlertKeysViewed(keys);
}
function persistQuoteState(quote,action,event){
  if(!currentUser||!authToken||!quote||!quote.quoteId)return Promise.reject(new Error('报价状态不可持久化'));
  D.quotePersistQueues=D.quotePersistQueues||{};
  const quoteId=quote.quoteId,payload=JSON.stringify({quote_id:quoteId,action:action,source_type:quote.sourceType||'manual',market:quote.market||'credit',status:quote.status||'active',data:quote,event:event||{}});
  quote.syncStatus='syncing';
  const task=Promise.resolve(D.quotePersistQueues[quoteId]).catch(()=>null)
    .then(()=>authFetch(API_BASE+'/quotes/state',{method:'POST',body:payload}))
    .then(async r=>{const data=await r.json().catch(()=>({}));if(!r.ok)throw new Error(typeof data.detail==='string'?data.detail:(data.detail&&data.detail.message)||data.error||'报价写入失败（HTTP '+r.status+'）');return data;})
    .then(d=>{
      if(!d.success)throw new Error(d.error||'报价写入失败');
      if(d.quoteId&&d.quoteId!==quote.quoteId){quote.quoteId=d.quoteId;quote.sourceQuoteId=d.quoteId;}
      if(d.quote&&d.quote.quoteId){
        Object.assign(quote,d.quote,{quoteId:d.quote.quoteId,sourceQuoteId:d.quote.quoteId});
      }
      if(Number.isFinite(Number(d.revision)))appliedQuoteStateRevision=Math.max(appliedQuoteStateRevision,Number(d.revision));
      quote.syncStatus='synced';return d;
    })
    .catch(error=>{quote.syncStatus='failed';throw error;});
  D.quotePersistQueues[quoteId]=task;return task;
}
async function persistQuoteWithRetry(quote,action,event){
  const delays=[3000,10000];
  for(let attempt=0;;attempt++){
    try{return await persistQuoteState(quote,action,event);}
    catch(error){if(attempt>=delays.length)throw error;await new Promise(resolve=>setTimeout(resolve,delays[attempt]));}
  }
}
function deleteQuotesRemote(quoteIds){
  if(!currentUser||!authToken||!quoteIds.length)return Promise.resolve(false);
  D.quotePersistQueues=D.quotePersistQueues||{};
  const pending=quoteIds.map(id=>D.quotePersistQueues[id]).filter(Boolean);
  return Promise.allSettled(pending).then(()=>authFetch(API_BASE+'/quotes/delete',{method:'POST',body:JSON.stringify({quote_ids:quoteIds})})).then(r=>r.ok?r.json():Promise.reject(new Error('HTTP '+r.status))).then(d=>{quoteIds.forEach(id=>delete D.quotePersistQueues[id]);return!!d.success;}).catch(()=>false);
}
function quoteAlertBase(entry){return{sourceQuoteId:entry.quoteId,quoteId:entry.quoteId,updatedAt:entry.updatedAt||entry.quotedAt||entry.sourceSentAt||'',dataVersion:entry.dataVersion||entry.version||'',ownerAccountId:entry.ownerAccountId,ownerUsername:entry.ownerUsername,ownerDisplayName:entry.ownerDisplayName,m:entry.m,c:entry.c,n:entry.n,market:entry.market,direction:quoteDirectionOf(entry),counterpartyInstitution:entry.counterpartyInstitution||entry.inst||'',counterpartyTrader:entry.counterpartyTrader||entry.trader||'',instBid:entry.bp?(entry.instBid||''):'',trBid:entry.bp?(entry.ownerDisplayName||entry.tr||''):'',bvBid:entry.bp?(entry.bvBid||''):'',bp:entry.bp||'',op:entry.op||'',bvOfr:entry.op?(entry.bvOfr||''):'',trOfr:entry.op?(entry.ownerDisplayName||entry.tr||''):'',instOfr:entry.op?(entry.instOfr||''):'',lastPrice:entry.lastPrice,r:entry.r,br:entry.br,t:entry.t,nzNet:entry.nzNet,nzDev:entry.nzDev,bondLib:entry.bondLib,nzYTM:entry.nzYTM,csNet:entry.csNet,csFull:entry.csFull,csYTM:entry.csYTM,cs:entry.cs||entry.settlement||'',priceType:entry.priceType||entry.pt||'到期收益率'};}
function alertVersionToken(alert,prefix){
  const key=prefix||'';
  return String(alert[key+'DataVersion']||alert[key+'Version']||alert[key+'UpdatedAt']||alert[key+'QuotedAt']||alert.dataVersion||alert.version||alert.updatedAt||alert.quotedAt||alert.sourceSentAt||'1');
}
function stableAlertKey(alert){
  if(!alert)return'';
  const type=String(alert.alertType||'提醒');
  if(type==='对价提醒'){
    const bid=String(alert.bidQuoteId||''),ofr=String(alert.ofrQuoteId||'');
    return['PRICE_MATCH',bid,alertVersionToken(alert,'bid'),ofr,alertVersionToken(alert,'ofr')].join(':');
  }
  return['ALERT',type,String(alert.sourceQuoteId||alert.quoteId||alert.c||''),alertVersionToken(alert,'')].join(':');
}
function alertInstanceKeys(alert){
  const provided=Array.isArray(alert&&alert.alertKeys)?alert.alertKeys:[];
  return Array.from(new Set(provided.concat(stableAlertKey(alert)).map(value=>String(value||'').trim()).filter(Boolean)));
}
function alertReadStorageKey(){return'ficc_alert_read_pending_'+String(currentAccountId()||'anonymous').replace(/[^A-Za-z0-9_.-]/g,'_');}
function localPendingAlertReads(){return new Set(safeStoredArray(alertReadStorageKey()).map(String).filter(Boolean));}
function saveLocalPendingAlertReads(keys){
  try{const values=Array.from(keys||[]).slice(-2000);if(values.length)localStorage.setItem(alertReadStorageKey(),JSON.stringify(values));else localStorage.removeItem(alertReadStorageKey());}catch(e){}
}
async function persistAlertReadKeys(keys){
  if(!currentUser||!authToken||!keys.length)return false;
  const pending=localPendingAlertReads();keys.forEach(key=>pending.add(key));saveLocalPendingAlertReads(pending);
  try{
    const response=await authFetch(API_BASE+'/alerts/read-state/bulk',{method:'POST',body:JSON.stringify({alertKeys:Array.from(pending).slice(0,1000)})});
    if(!response.ok)return false;
    saveLocalPendingAlertReads(new Set());return true;
  }catch(e){return false;}
}
async function loadAlertReadState(){
  readAlertKeys=new Set();alertReadLoaded=false;
  const pending=localPendingAlertReads();pending.forEach(key=>readAlertKeys.add(key));
  if(!currentUser||!authToken){alertReadLoaded=true;return false;}
  try{
    const response=await authFetch(API_BASE+'/alerts/read-state');
    const body=await response.json().catch(()=>({}));
    if(!response.ok)throw new Error('HTTP '+response.status);
    (body.readKeys||[]).forEach(key=>readAlertKeys.add(String(key)));
    alertReadLoaded=true;if(pending.size)await persistAlertReadKeys(Array.from(pending));updateBadges();return true;
  }catch(e){alertReadLoaded=true;updateBadges();return false;}
}
function appendPriceMatchAlerts(activeQuotes){
  if(!D.alertSettings.priceMatch)return;
  const active=(activeQuotes||[]).filter(q=>q&&q.status==='active'&&q.quoteId);
  const mine=active.filter(q=>quoteIsMine(q)),others=active.filter(q=>!quoteIsMine(q)),seen=new Set();
  mine.forEach(myQuote=>{
    others.forEach(other=>{
      if(!quotesOppositeForAlert(myQuote,other))return;
      const bid=quoteDirectionOf(myQuote)==='bid'?myQuote:other;
      const ofr=quoteDirectionOf(myQuote)==='ofr'?myQuote:other;
      const key='PRICE_MATCH:'+bid.quoteId+':'+ofr.quoteId;if(seen.has(key))return;seen.add(key);
      const bidBase=quoteAlertBase(bid),ofrBase=quoteAlertBase(ofr);
      D.boardAlert.push({...bidBase,sourceQuoteId:myQuote.quoteId,alertId:key,alertType:'对价提醒',pairKey:key,bidQuoteId:bid.quoteId,ofrQuoteId:ofr.quoteId,bidUpdatedAt:bid.updatedAt||bid.quotedAt||'',ofrUpdatedAt:ofr.updatedAt||ofr.quotedAt||'',bidDataVersion:bid.dataVersion||bid.version||'',ofrDataVersion:ofr.dataVersion||ofr.version||'',
        bp:bid.bp||'',bvBid:bid.bvBid||bid.bv||'',instBid:bid.instBid||bid.counterpartyInstitution||bid.inst||'',trBid:bid.ownerDisplayName||bid.tr||'',counterpartyTraderBid:bid.counterpartyTrader||bid.trader||'',
        op:ofr.op||'',bvOfr:ofr.bvOfr||ofr.bv||'',instOfr:ofr.instOfr||ofr.counterpartyInstitution||ofr.inst||'',trOfr:ofr.ownerDisplayName||ofr.tr||'',counterpartyTraderOfr:ofr.counterpartyTrader||ofr.trader||'',
        bidOwnerDisplayName:bid.ownerDisplayName||bid.tr||'',ofrOwnerDisplayName:ofr.ownerDisplayName||ofr.tr||''});
    });
  });
}
function priceMatchAlertKeys(){return new Set((D.boardAlert||[]).filter(a=>a.alertType==='对价提醒').map(a=>a.alertId||a.pairKey).filter(Boolean));}
// P0: 捕获所有类型提醒的key，用于检测新增
function allAlertKeys(){return new Set((D.boardAlert||[]).flatMap(alertInstanceKeys));}
function notifyNewPriceMatchAlerts(previousKeys){
  const fresh=(D.boardAlert||[]).filter(a=>a.alertType==='对价提醒'&&alertInstanceKeys(a).some(key=>!previousKeys.has(key)));
  if(fresh.length)showAllAlerts(['对价提醒'],{'对价提醒':fresh});
}
// P0: 四类提醒统一弹窗通知（只弹新增，不弹历史）
function notifyNewAlerts(previousKeys){
  const fresh=(D.boardAlert||[]).filter(a=>alertInstanceKeys(a).some(key=>!previousKeys.has(key)));
  if(!fresh.length)return;
  const byType={};
  fresh.forEach(a=>{const t=a.alertType||'对价提醒';if(!byType[t])byType[t]=[];byType[t].push(a);});
  const types=Object.keys(byType).filter(t=>D.alertSettings[t==='对价提醒'?'priceMatch':t==='关注债券'?'favorite':t==='筛选分组'?'filterGroup':'bestQuote']!==false);
  if(types.length)showAllAlerts(types,byType);
}
function refreshAlertsForQuote(entry,sourceRow){
  D.boardAlert=(D.boardAlert||[]).filter(a=>a.alertType==='对价提醒'?(a.bidQuoteId!==entry.quoteId&&a.ofrQuoteId!==entry.quoteId):a.sourceQuoteId!==entry.quoteId);
  const types=[];
  const favoriteGroups=D.alertSettings.favorite?matchedFavoriteGroups(entry):[];
  if(favoriteGroups.length)types.push({type:'关注债券',groups:favoriteGroups});
  const filterGroups=D.alertSettings.filterGroup?matchedMarketFilterGroups(entry):[];
  if(filterGroups.length)types.push({type:'筛选分组',groups:filterGroups});
  const best=(entry.market==='rate'?D.boardRateBest:D.boardCreditBest).find(r=>r.c===entry.c&&(r.bidQuoteId===entry.quoteId||r.ofrQuoteId===entry.quoteId));
  // “我的最优”必须存在另一条同券同方向行情报价；只有本人一条报价时不触发提醒。
  const hasSameSideOtherQuote=(D.liveQuotes||[]).some(q=>q.status==='active'&&q.c===entry.c&&q.quoteId!==entry.quoteId&&quoteDirectionOf(q)===quoteDirectionOf(entry)&&quotePairKey(q)===quotePairKey(entry));
  if(D.alertSettings.bestQuote&&quoteIsMine(entry)&&best&&hasSameSideOtherQuote)types.push({type:'我的最优',groups:[]});
  if(!types.length)return;
  // P1: 非对价类提醒使用最优页同券双边数据展示
  const bestRow=(entry.market==='rate'?D.boardRateBest:D.boardCreditBest).find(r=>r.c===entry.c);
  const base=quoteAlertBase(entry);
  const dualSide=bestRow?{
    bp:bestRow.bp||'',op:bestRow.op||'',
    bvBid:bestRow.bvBid||'',bvOfr:bestRow.bvOfr||'',
    instBid:bestRow.instBid||'',instOfr:bestRow.instOfr||'',
    trBid:bestRow.trBid||'',trOfr:bestRow.trOfr||'',
    bidQuoteId:bestRow.bidQuoteId||'',ofrQuoteId:bestRow.ofrQuoteId||''
  }:{};
  types.forEach(item=>D.boardAlert.push({...base,...dualSide,alertId:entry.quoteId+':'+item.type,alertType:item.type,groupNames:item.groups.map(group=>group.name)}));
}
// P1: 同券多类型非对价提醒合并为一行（减少重复堆多行）
function mergeNonPriceMatchAlerts(){
  const priceMatchAlerts=(D.boardAlert||[]).filter(a=>a.alertType==='对价提醒');
  const others=(D.boardAlert||[]).filter(a=>a.alertType!=='对价提醒');
  const grouped={};
  others.forEach(a=>{
    const key=(a.c||'')+'|'+(a.market||'');
    if(!grouped[key])grouped[key]=a;
    else{
      const existing=grouped[key];
      if(!existing.alertTypes)existing.alertTypes=[existing.alertType];
      if(!existing.alertTypes.includes(a.alertType))existing.alertTypes.push(a.alertType);
      existing.alertType=existing.alertTypes.join('/');
      existing.groupNames=[...new Set([...(existing.groupNames||[]),...(a.groupNames||[])])];
      existing.alertId=existing.alertId+'+'+a.alertId;
    }
  });
  D.boardAlert=[...priceMatchAlerts,...Object.values(grouped)];
}
const PRECISE_NEW_REQUIRED_FIELDS=[
  {key:'identity',label:'债券代码或简称'},
  {key:'d',label:'方向'},
];
function preciseHasRequiredValue(value){return value!==null&&value!==undefined&&String(value).trim()!=='';}
function preciseMissingRequiredFields(row){
  if(!row||(row.quoteAction||'NEW')!=='NEW')return[];
  const missing=[];
  if(!preciseHasRequiredValue(row.c)&&!preciseHasRequiredValue(row.n))missing.push(PRECISE_NEW_REQUIRED_FIELDS[0]);
  if(!preciseHasRequiredValue(row.d))missing.push(PRECISE_NEW_REQUIRED_FIELDS[1]);
  return missing;
}
function normalizedRecognitionCode(value){return String(value||'').trim().toUpperCase().replace(/\.(IB|SH|SZ|BJ)$/,'');}
function quoteLogicalText(value){return String(value||'').trim().replace(/\s+/g,' ').toUpperCase();}
function quoteLogicalKey(quote,sourceType){
  return[
    quoteLogicalText(quote&&quote.ownerAccountId||currentAccountId()),
    quoteLogicalText(sourceType||quote&&quote.sourceType),
    normalizedRecognitionCode(quote&&quote.c),
    quoteDirectionOf(quote).toUpperCase(),
    quoteLogicalText(quote&&quote.counterpartyInstitution||quote&&quote.inst),
    quoteLogicalText(quote&&quote.counterpartyTrader||quote&&quote.trader),
    quoteLogicalText(quote&&quote.priceType),
    quoteLogicalText(quote&&quote.tradeDate),
    quoteLogicalText(quote&&quote.cs)
  ].join('|');
}
function cancellationTargets(row){
  const code=normalizedRecognitionCode(row.c),scope=row.cancelScope||'ALL';
  const hasValue=value=>value!==null&&value!==undefined&&String(value).trim()!=='';
  return(D.liveQuotes||[]).filter(quote=>{
    if(!quote||quote.status!=='active')return false;
    if(row.referenceQuoteId&&quote.quoteId!==row.referenceQuoteId)return false;
    if(code&&normalizedRecognitionCode(quote.c)!==code)return false;
    if(!code&&!row.referenceQuoteId)return false;
    const direction=quoteDirectionOf(quote).toUpperCase();
    if(scope==='BID'&&direction!=='BID')return false;
    if(scope==='OFR'&&direction!=='OFR')return false;
    if(hasValue(row.p)||hasValue(row.vol)){
      const price=direction==='OFR'?quote.op:quote.bp;
      const volume=(direction==='OFR'?quote.bvOfr:quote.bvBid)||quote.bv;
      if(hasValue(row.p)&&String(price||'').trim()!==String(row.p).trim())return false;
      if(hasValue(row.vol)&&String(volume||'').trim()!==String(row.vol).trim())return false;
    }
    return true;
  });
}
function quoteCancelInstruction(row,index){
  const direction=String(row.d||((row.cancelScope==='BID'||row.cancelScope==='OFR')?row.cancelScope:'')).toUpperCase();
  const explicit=(field,value)=>preciseHasRequiredValue(value)&&(!row._fieldMeta||!!(row._fieldMeta[field]&&row._fieldMeta[field].explicit));
  return{
    instructionId:String(row.recognitionId||row._cancelInstructionId||(row._cancelInstructionId='ref-'+index+'-'+makeQuoteId('instruction'))),
    referenceQuoteId:row.referenceQuoteId||null,
    securityId:row.c||'',direction:direction,
    quoteValue:explicit('price',row.p)?row.p:null,
    volumeWan:explicit('quantity',row.vol)?row.vol:null,
    institution:preciseHasRequiredValue(row.inst)?row.inst:null,
    trader:preciseHasRequiredValue(row.tr)?row.tr:null,
    priceType:explicit('price_type',row.pt)?row.pt:null,
    settlement:explicit('settlement',row.cs)?row.cs:null
  };
}
let refCandidateModalResolve=null,refCandidateModalGroups=[];
function refGroupCheckElements(){return Array.prototype.slice.call(document.querySelectorAll('.ref-group-check'));}
function syncRefSelectAllState(){
  const master=document.getElementById('refSelectAll');if(!master)return;
  const checks=refGroupCheckElements();
  master.checked=checks.length>0&&checks.every(cb=>cb.checked);
  master.indeterminate=checks.some(cb=>cb.checked)&&!master.checked;
}
function setRefGroupEnabled(groupIndex,checked){
  const check=refGroupCheckElements()[groupIndex];if(check)check.checked=checked;
  const radios=Array.prototype.slice.call(document.querySelectorAll('input[name="ref-candidate-'+groupIndex+'"]'));
  radios.forEach(radio=>{radio.disabled=!checked;const label=radio.closest('label');if(label)label.style.opacity=checked?'1':'0.45';});
  if(checked&&radios.length&&!radios.some(radio=>radio.checked))radios[0].checked=true;
}
function toggleRefGroup(groupIndex,checked){setRefGroupEnabled(groupIndex,checked);syncRefSelectAllState();}
function toggleAllRefGroups(checked){
  refGroupCheckElements().forEach((cb,index)=>setRefGroupEnabled(index,checked));
  syncRefSelectAllState();
}
function formatLocalQuoteTime(value){
  const text=String(value||'').trim();if(!text)return '';
  // 无时区标记的时间按 UTC 处理，统一换算为客户端系统本地时间展示
  const source=/(Z|[+-]\d{2}:?\d{2})$/.test(text)?text:(/^\d{4}-\d{2}-\d{2}[ T]/.test(text)?text.replace(' ','T')+'Z':'');
  const parsed=source?new Date(source):null;
  if(!parsed||isNaN(parsed.getTime()))return text;
  const pad=n=>String(n).padStart(2,'0');
  return parsed.getFullYear()+'-'+pad(parsed.getMonth()+1)+'-'+pad(parsed.getDate())+' '+pad(parsed.getHours())+':'+pad(parsed.getMinutes())+':'+pad(parsed.getSeconds());
}
function chooseRefCancellationCandidates(groups){
  refCandidateModalGroups=groups;
  let html='<div style="display:flex;align-items:center;gap:10px;margin-bottom:10px;font-size:12px"><label style="display:flex;align-items:center;gap:6px;cursor:pointer;color:#e6edf3;user-select:none;white-space:nowrap"><input type="checkbox" id="refSelectAll" checked onchange="toggleAllRefGroups(this.checked)"> 全选</label><span style="color:#8b949e">勾选的 Ref 参与本次撤销，未勾选的保留在识别表；命中多条报价的 Ref 需为其选择一条目标。</span></div>';
  groups.forEach((group,groupIndex)=>{
    html+='<div data-ref-group-index="'+groupIndex+'" style="margin:10px 0;padding:10px;border:1px solid #30363d;border-radius:6px"><div style="font-weight:600;color:#e6edf3;margin-bottom:6px"><label style="display:flex;align-items:center;gap:8px;cursor:pointer;user-select:none"><input type="checkbox" class="ref-group-check" data-ref-group-index="'+groupIndex+'" checked onchange="toggleRefGroup('+groupIndex+',this.checked)"><span>参与本次撤销 · Ref '+(groupIndex+1)+'：'+escapeQuoteHtml(group.item.source.raw||group.item.source.txt||group.item.instruction.securityId)+'</span></label></div>';
    group.candidates.forEach((candidate,candidateIndex)=>{
      const checked=candidateIndex===0?' checked':'';
      html+='<label style="display:flex;gap:8px;align-items:center;padding:6px;cursor:pointer;border-top:1px solid #21262d"><input type="radio" data-ref-group-index="'+groupIndex+'" name="ref-candidate-'+groupIndex+'" value="'+candidateIndex+'"'+checked+'><span style="font-family:\'JetBrains Mono\',monospace;color:#79c0ff">'+escapeQuoteHtml(candidate.securityId)+'</span><span>'+escapeQuoteHtml(candidate.securityName)+'</span><span style="color:'+(candidate.direction==='BID'?'#f59e0b':'#58a6ff')+'">'+candidate.direction+'</span><span>'+escapeQuoteHtml(candidate.quoteValue)+'</span><span>'+escapeQuoteHtml(candidate.volumeWan)+'万</span><span style="color:#8b949e">'+escapeQuoteHtml(candidate.institution||'')+' '+escapeQuoteHtml(candidate.trader||'')+'</span><span style="margin-left:auto;color:#7d8590">'+escapeQuoteHtml(formatLocalQuoteTime(candidate.quotedAt))+'</span></label>';
    });
    html+='</div>';
  });
  return new Promise(resolve=>{
    refCandidateModalResolve=resolve;
    showModal('选择Ref撤销目标',html,{wide:true,footerHtml:'<button class="tb-btn tb-btn-default" type="button" onclick="finishRefCandidateSelection(false)">取消</button><button class="tb-btn tb-btn-primary" type="button" onclick="finishRefCandidateSelection(true)">确认选择</button>'});
  });
}
function finishRefCandidateSelection(confirmed){
  if(!refCandidateModalResolve){closeModal();return;}
  if(!confirmed){const resolve=refCandidateModalResolve;refCandidateModalResolve=null;refCandidateModalGroups=[];closeModal();resolve(null);return;}
  const selections=[],skipped=[],groupChecks=refGroupCheckElements();
  for(let i=0;i<refCandidateModalGroups.length;i++){
    const group=refCandidateModalGroups[i];
    if(!(groupChecks[i]&&groupChecks[i].checked)){skipped.push(group.item);continue;}
    const checked=document.querySelector('input[name="ref-candidate-'+i+'"]:checked');
    if(!checked){showToast('error','请为第 '+(i+1)+' 条Ref指令选择目标报价');return;}
    selections.push({item:group.item,candidate:group.candidates[Number(checked.value)]});
  }
  if(!selections.length){showToast('error','请至少勾选一条Ref指令参与撤销');return;}
  const quoteIds=selections.map(value=>value.candidate.quoteId);
  if(new Set(quoteIds).size!==quoteIds.length){showToast('error','不同Ref指令不能选择同一条正式报价');return;}
  const resolve=refCandidateModalResolve;refCandidateModalResolve=null;refCandidateModalGroups=[];closeModal();resolve({selections:selections,skipped:skipped});
}
async function resolveRefCancellationItems(items){
  const response=await authFetch(API_BASE+'/quotes/revoke/resolve',{method:'POST',body:JSON.stringify({instructions:items.map(item=>item.instruction)})});
  const body=await response.json();if(!response.ok||!body.success)throw new Error(typeof body.detail==='string'?body.detail:'Ref候选查询失败');
  const byInstruction=new Map((body.results||[]).map(result=>[String(result.instructionId),result.candidates||[]]));
  const localById=new Map((D.liveQuotes||[]).filter(quote=>quote&&quote.quoteId).map(quote=>[quote.quoteId,quote]));
  const failed=[],groups=[];
  items.forEach(item=>{
    // 弹窗只展示有效报价：服务端已只返回 active 报价，这里再剔除本地已标记撤销的目标
    const candidates=(byInstruction.get(String(item.instruction.instructionId))||[]).filter(candidate=>{const local=localById.get(candidate.quoteId);return !local||local.status!=='revoked';});
    if(!candidates.length){item.source._confirmError='未找到符合条件的有效报价（目标报价可能已被撤销）';failed.push(item);}
    else groups.push({item:item,candidates:candidates});
  });
  if(!groups.length)return{selections:[],failed:failed,cancelled:false,skipped:[]};
  const needsChoice=groups.some(group=>group.candidates.length>1);
  let choice;
  if(needsChoice)choice=await chooseRefCancellationCandidates(groups);
  else choice={selections:groups.map(group=>({item:group.item,candidate:group.candidates[0]})),skipped:[]};
  if(choice===null)return{selections:[],failed:failed,cancelled:true,skipped:[]};
  return{selections:choice.selections||[],failed:failed,cancelled:false,skipped:choice.skipped||[]};
}
async function commitRefCancellations(initialSelections,invalidRows){
  let selections=initialSelections;
  for(let attempt=0;attempt<2;attempt++){
    const confirmResponse=await authFetch(API_BASE+'/quotes/revoke/confirm',{method:'POST',body:JSON.stringify({selections:selections.map(value=>({instruction:value.item.instruction,quoteId:value.candidate.quoteId}))})});
    const confirmBody=await confirmResponse.json().catch(()=>({}));
    if(confirmResponse.ok&&confirmBody.success)return confirmBody;
    const stale=confirmResponse.status===409||confirmResponse.status===404;
    if(!stale||attempt===1)throw new Error(typeof confirmBody.detail==='string'?confirmBody.detail:'Ref撤销失败');
    // 目标报价状态已变化（被他人或别处撤销）：重新查询最新候选并重弹选择，无效报价不会再出现在弹窗
    showToast('info','部分目标报价状态已变化，正在刷新有效候选');
    const refreshed=await resolveRefCancellationItems(selections.map(value=>value.item));
    if(refreshed.cancelled)throw new Error('已取消Ref目标选择');
    refreshed.failed.forEach(item=>{if(invalidRows&&!invalidRows.includes(item.source))invalidRows.push(item.source);});
    selections=refreshed.selections;
    if(!selections.length)throw new Error('所有目标报价均已失效，无可撤销的有效报价');
  }
  throw new Error('Ref撤销失败');
}
function recognitionUpdateTargets(row){
  const code=normalizedRecognitionCode(row.c),direction=String(row.d||'').toLowerCase();
  return(D.liveQuotes||[]).filter(quote=>quote&&quote.status==='active'&&normalizedRecognitionCode(quote.c)===code&&(!direction||quoteDirectionOf(quote)===direction));
}
function applyRecognitionUpdate(target,row){
  const meta=row._fieldMeta||{},direction=quoteDirectionOf(target);let changed=false;
  if(row.p&&meta.price&&meta.price.explicit){if(direction==='bid')target.bp=row.p;else target.op=row.p;changed=true;}
  if(row.vol&&meta.quantity&&meta.quantity.explicit){target.bv=row.vol;if(direction==='bid')target.bvBid=row.vol;else target.bvOfr=row.vol;changed=true;}
  if(row.date&&meta.trade_date&&meta.trade_date.explicit){target.tradeDate=row.date;changed=true;}
  if(row.cs&&meta.settlement&&meta.settlement.explicit){target.cs=row.cs;changed=true;}
  if(row.pt&&meta.price_type&&meta.price_type.explicit){target.priceType=row.pt;changed=true;}
  if(!changed)return null;
  return addQuoteEvent(target,'edit','报价识别更新');
}
async function executeStableConfirm(dataArray,selectedSet,sourceName,sourceType){
  if(!authToken){showToast('error','应急模式为只读，认证服务恢复后才能发布报价');return;}
  if(selectedSet.size===0){showToast('error','请先选择报价数据');return;}
  const previousAlertKeys=allAlertKeys();
  const selected=Array.from(selectedSet).sort((a,b)=>a-b),published=[],cancelled=[],updated=[],publishedIndexes=[],invalidRows=[],deferredRows=[];
  const preparedNewEntries=new Map(),batchLogicalKeyFirstIndex=new Map();
  selected.forEach(i=>{
    const row=dataArray[i];if(!row||row.quoteAction==='CANCEL'||row.quoteAction==='UPDATE'||preciseMissingRequiredFields(row).length)return;
    const entry=buildBoardEntry(row,sourceType);entry.market=classifyBondMarket(row.c,row.n,row._apiInfo||{},row.bondType||'');
    const key=quoteLogicalKey(entry,sourceType);preparedNewEntries.set(i,entry);
    if(!batchLogicalKeyFirstIndex.has(key))batchLogicalKeyFirstIndex.set(key,i);
  });
  selected.forEach(i=>{
    const r=dataArray[i];if(!r)return;
    r._confirmError='';r._confirmMissingFields=[];
    if(r.quoteAction==='CANCEL'){
      cancelled.push({source:r,index:i,instruction:quoteCancelInstruction(r,i)});return;
    }
    if(r.quoteAction==='UPDATE'){
      const targets=recognitionUpdateTargets(r);
      if(targets.length!==1){r._confirmError='更新报价必须唯一匹配一条有效报价';invalidRows.push(r);return;}
      const event=applyRecognitionUpdate(targets[0],r);
      if(!event){r._confirmError='更新报价缺少明确的修改字段';invalidRows.push(r);return;}
      updated.push({entry:targets[0],event:event,source:r});publishedIndexes.push(i);return;
    }
    const missing=preciseMissingRequiredFields(r);
    if(missing.length){
      r._confirmMissingFields=missing.map(field=>field.key);
      r._confirmError='新增报价缺少：'+missing.map(field=>field.label).join('、');
      invalidRows.push(r);return;
    }
    const entry=preparedNewEntries.get(i)||buildBoardEntry(r,sourceType),logicalKey=quoteLogicalKey(entry,sourceType);
    if(batchLogicalKeyFirstIndex.get(logicalKey)!==i){deferredRows.push(r);return;}
    const logicalMatches=(D.liveQuotes||[]).filter(quote=>quote&&quote.status==='active'&&quoteLogicalKey(quote,quote.sourceType)===logicalKey);
    if(logicalMatches.length>1){
      r._confirmError='同一逻辑报价已存在多条有效记录，请先处理历史重复报价';invalidRows.push(r);return;
    }
    if(logicalMatches.length===1){
      const target=logicalMatches[0],quoteId=target.quoteId;
      Object.assign(target,entry,{quoteId:quoteId,sourceQuoteId:quoteId,createdAt:target.createdAt||entry.createdAt});
      r.quoteId=quoteId;
      const event=addQuoteEvent(target,'edit','精确报价覆盖更新');
      updated.push({entry:target,event:event,source:r,deduplicated:true});publishedIndexes.push(i);return;
    }
    r.quoteId=entry.quoteId;D.liveQuotes.push(entry);published.push({entry,source:r});publishedIndexes.push(i);
  });
  let revokedCount=0,refInstructionCount=0,refSkippedCount=0;
  if(cancelled.length){
    try{
      const resolved=await resolveRefCancellationItems(cancelled);
      refSkippedCount=(resolved.skipped||[]).length;
      resolved.failed.forEach(item=>{if(!invalidRows.includes(item.source))invalidRows.push(item.source);});
      if(resolved.cancelled){cancelled.forEach(item=>{item.source._confirmError='已取消Ref目标选择';if(!invalidRows.includes(item.source))invalidRows.push(item.source);});}
      if(resolved.selections.length){
        const confirmBody=await commitRefCancellations(resolved.selections,invalidRows);
        const revokedIds=new Set(confirmBody.quoteIds||[]);
        (D.liveQuotes||[]).forEach(target=>{if(revokedIds.has(target.quoteId)){target.status='revoked';addQuoteEvent(target,'revoke','精确报价Ref撤销');}});
        resolved.selections.forEach(value=>publishedIndexes.push(value.item.index));
        revokedCount=Number(confirmBody.revoked)||revokedIds.size;
        refInstructionCount=Number(confirmBody.instructionCount)||resolved.selections.length;
      }
    }catch(error){
      cancelled.forEach(item=>{item.source._confirmError=error.message||'Ref撤销失败';if(!invalidRows.includes(item.source))invalidRows.push(item.source);});
    }
  }
  rebuildBoardProjections();notifyNewAlerts(previousAlertKeys);
  const successfulQuoteIds=[];
  const persistence=published.map(item=>{
    const event=addQuoteEvent(item.entry,'quote','报价录入确认');
    return persistQuoteWithRetry(item.entry,'quote',event).then(result=>{item.source.quoteId=item.entry.quoteId;successfulQuoteIds.push(item.entry.quoteId);return result;});
  });
  updated.forEach(item=>persistence.push(persistQuoteWithRetry(item.entry,'edit',item.event).then(result=>{successfulQuoteIds.push(item.entry.quoteId);return result;})));
  publishedIndexes.sort((a,b)=>b-a).forEach(i=>dataArray.splice(i,1));dataArray.forEach((r,i)=>{r._idx=i;});
  const invalidCount=invalidRows.length;selectedSet.clear();
  invalidRows.concat(deferredRows).forEach(row=>{const index=dataArray.indexOf(row);if(index>=0)selectedSet.add(index);});
  syncMgmtToday();syncMgmtHistory();renderAll();updateBadges();
  const settled=await Promise.allSettled(persistence),persistedSuccessCount=settled.filter(result=>result.status==='fulfilled').length,successCount=persistedSuccessCount+revokedCount;
  if(sourceType==='precise'&&successCount>0&&dataArray.length===0)clearQuoteBatchInputs('precise');
  const skippedNote=refSkippedCount?'，另有 '+refSkippedCount+' 条Ref未勾选暂不处理':'';
  if(refInstructionCount&&persistedSuccessCount===0&&!updated.length)showToast('success','已处理'+refInstructionCount+'条Ref指令，成功撤销'+revokedCount+'条正式报价'+skippedNote);
  else if(successCount)showToast('success','确认成功 '+successCount+' 条'+(revokedCount?'（含Ref撤销 '+revokedCount+' 条）':'')+(updated.length?'（含更新 '+updated.length+' 条）':'')+(deferredRows.length?'，重复报价 '+deferredRows.length+' 条继续保留':'')+(invalidCount?'，未通过债券信息、方向或动作目标校验的记录仍保留在识别表':'')+skippedNote);
  if(invalidCount){
    const details=[...new Set(invalidRows.map(row=>row._confirmError).filter(Boolean))];
    showToast('error','未确认 '+invalidCount+' 条'+(details.length?'：'+details.join('；'):''));
  }
  if(persistedSuccessCount<persistence.length)showToast('error',(persistence.length-persistedSuccessCount)+' 条报价同步失败，请检查服务后重试');
  if(successfulQuoteIds.length)revealAndHighlightSavedQuotes(successfulQuoteIds,true);
  else{rebuildBoardProjections();renderAll();}
}
function pendingPriceTypeCode(label){return{'到期收益率':'YTM','行权收益率':'YTC','净价':'CLEAN_PRICE','全价':'DIRTY_PRICE'}[label]||label||'';}
function pendingQuoteRequest(row){return{recognitionId:row.recognitionId,version:row.version,securityId:quoteConfirmEditableValue(row,'c'),securityName:quoteConfirmEditableValue(row,'n'),direction:quoteConfirmEditableValue(row,'d').toUpperCase(),priceType:pendingPriceTypeCode(quoteConfirmEditableValue(row,'pt')),quoteValue:quoteConfirmEditableValue(row,'p'),volumeWan:quoteConfirmEditableValue(row,'vol'),tradeDate:quoteConfirmEditableValue(row,'date'),settlementType:quoteConfirmEditableValue(row,'cs'),counterpartyInstitutionName:quoteConfirmEditableValue(row,'inst'),counterpartyTraderName:quoteConfirmEditableValue(row,'tr'),oco:!!row.oco,packing:!!row.packing,urgent:!!row.urgent,recommend:!!(row.recommend||row.rec),round:!!(row.round||row.whole)};}
const PENDING_API_FIELD_MAP={securityId:['identity','债券代码'],securityName:['identity','债券简称'],direction:['d','方向'],quoteValue:['p','报价'],priceType:['pt','价格类型'],volumeWan:['vol','报量'],tradeDate:['date','交易日期'],settlementType:['cs','清算速度'],counterpartyInstitutionName:['inst','报价机构'],counterpartyTraderName:['tr','报价交易员']};
function applyPendingQuoteApiValidationErrors(detail,rows){
  if(!Array.isArray(detail))return typeof detail==='string'?detail:(detail&&detail.message)||'确认失败';
  const messages=detail.map(item=>{
    const loc=Array.isArray(item&&item.loc)?item.loc:[],itemsAt=loc.indexOf('items'),rowIndex=itemsAt>=0?Number(loc[itemsAt+1]):-1,apiField=String(loc[loc.length-1]||''),mapped=PENDING_API_FIELD_MAP[apiField],row=Number.isInteger(rowIndex)?rows[rowIndex]:null;
    if(row&&mapped){const invalid=new Set(Array.isArray(row._confirmMissingFields)?row._confirmMissingFields:[]);invalid.add(mapped[0]);row._confirmMissingFields=Array.from(invalid);row._confirmError=String(item.msg||'输入不合法');}
    return (row?'第 '+(row._idx+1)+' 行 ':'')+(mapped?mapped[1]+'：':'')+String(item&&item.msg||'输入不合法');
  });
  renderQuoteGrouped();
  return messages.join('；');
}
function pendingDuplicateKey(row){return[String(row.ownerAccountId||currentAccountId()),String(row.src||'').toUpperCase(),quoteConfirmEditableValue(row,'inst'),quoteConfirmEditableValue(row,'c')||quoteConfirmEditableValue(row,'n'),quoteConfirmEditableValue(row,'d').toUpperCase(),pendingPriceTypeCode(quoteConfirmEditableValue(row,'pt')),quoteConfirmEditableValue(row,'p').replace(/0+$/,'').replace(/\.$/,''),quoteConfirmEditableValue(row,'vol').replace(/0+$/,'').replace(/\.$/,''),quoteConfirmEditableValue(row,'date'),quoteConfirmEditableValue(row,'cs')].join('|');}
async function resolvePendingQuoteDraftConflicts(rows){
  const conflicts=(rows||[]).filter(row=>row&&row._draftConflict);if(!conflicts.length)return true;
  const reapply=await systemConfirmHtml('待确认报价版本冲突','<div style="color:#c9d1d9;line-height:1.7">有 '+conflicts.length+' 条报价在编辑期间被其他客户端更新。本地修改仍保留，但必须确认后才能覆盖冲突字段。</div><div style="margin-top:8px;color:#8b949e">选择“重新应用”将基于服务端最新版本提交本地修改；选择“放弃”将恢复服务端数据。</div>',{confirmText:'重新应用',cancelText:'放弃本地修改',wide:true});
  if(reapply){
    conflicts.forEach(row=>{const draft=pendingQuoteDrafts.get(String(row.recognitionId));if(draft){draft.baseVersion=Number(row.version||1);draft.conflict=false;}row._draftConflict=false;});return true;
  }
  conflicts.forEach(row=>pendingQuoteDrafts.delete(String(row.recognitionId)));await loadQuoteConfirmBatches(false);return false;
}
async function doConfirmQuotes(){
  if(!authToken){showToast('error','应急模式为只读，认证服务恢复后才能确认报价');return;}
  commitQuoteConfirmTableControls();
  const rows=selectedQuoteRows();if(!rows.length){showToast('error','请先选择报价数据');return;}
  if(!await resolvePendingQuoteDraftConflicts(rows))return;
  const confirmRows=rows;
  const invalidRows=[];
  confirmRows.forEach(row=>{
    const missing=quoteConfirmMissingRequiredFields(row);
    row._confirmMissingFields=missing.map(field=>field.key);
    if(missing.length)invalidRows.push({row,missing});
  });
  if(invalidRows.length){
    renderQuoteGrouped();
    const details=[...new Set(invalidRows.map(item=>item.missing.map(field=>field.label).join('、')))];
    showToast('error','未确认 '+invalidRows.length+' 条：缺少'+details.join('；缺少'));
    return;
  }
  try{
    const response=await authFetch(API_BASE+'/quote-confirm/confirm',{method:'POST',body:JSON.stringify({items:confirmRows.map(pendingQuoteRequest)})});
    const body=await response.json();if(!response.ok)throw new Error(applyPendingQuoteApiValidationErrors(body.detail,confirmRows));
    (body.confirmedRecognitionIds||[]).forEach(id=>pendingQuoteDrafts.delete(String(id)));
    const deferredIds=new Set(body.deferredRecognitionIds||[]);
    quoteSelected.clear();await loadQuoteConfirmBatches(false);
    D.quoteConfirm.forEach((row,index)=>{if(deferredIds.has(row.recognitionId))quoteSelected.add(quoteSelectionId(row,index));});
    if((body.confirmedRecognitionIds||[]).length>0&&D.quoteConfirm.length===0)clearQuoteBatchInputs('confirm');
    renderQuoteGrouped();await loadQuoteState({notify:true,highlightQuoteIds:body.quoteIds||[],reveal:true});
    showToast('success','确认成功 '+(body.confirmedRecognitionIds||[]).length+' 条'+(deferredIds.size?'，重复报价 '+deferredIds.size+' 条继续保留':''));
  }catch(error){showToast('error',error.message||'确认失败，原待确认数据已保留');renderQuoteGrouped();}
}
function doConfirmPrecise(){commitRecognitionTableControls('preciseTableWrap');executeStableConfirm(D.preciseData,preciseSelected,'精确','precise');}
function doConfirmFuzzy(){if(!authToken){showToast('error','应急模式为只读，认证服务恢复后才能确认报价');return;}executeConfirm(D.fuzzyData,fuzzySelected,'模糊');updateBadges();}

function fuzzyBatchApplyMeta(){
  var inst=document.getElementById('fuzzyBatchInstInput').value.trim();
  var trader=document.getElementById('fuzzyBatchTraderInput').value.trim();
  if(!inst&&!trader){showToast('error','请输入要批量设置的机构或交易员');return;}
  var count=0;
  fuzzySelected.forEach(function(i){
    if(D.fuzzyData[i]){
      if(inst)D.fuzzyData[i].inst=inst;
      if(trader)D.fuzzyData[i].tr=trader;
      count++;
    }
  });
  renderFuzzyTable();
  showToast('success','已批量设置 '+count+' 条报价');
}

function showFuzzyMsg(type,msg){
  var el=document.getElementById('fuzzyBatchMsg');if(!el)return;
  var isErr=type==='error';
  el.innerHTML='<span style="color:'+(isErr?'#ef4444':'#3fb950')+';background:'+(isErr?'rgba(239,68,68,0.12)':'rgba(63,185,80,0.12)')+';border:1px solid '+(isErr?'rgba(239,68,68,0.3)':'rgba(63,185,80,0.3)')+';border-radius:6px;padding:3px 10px;white-space:nowrap">'+msg+'</span>';
  setTimeout(function(){el.innerHTML='';},2000);
}

function fuzzyBatchSetInst(v){
  if(fuzzySelected.size===0){showFuzzyMsg('error','请先勾选报价数据');return;}
  let c=0;fuzzySelected.forEach(function(i){
    if(D.fuzzyData[i]){D.fuzzyData[i].inst=v;c++;}
  });
  _renderFuzzyLeftTable();
}

function fuzzyBatchSetTrader(v){
  if(fuzzySelected.size===0){showFuzzyMsg('error','请先勾选报价数据');return;}
  let c=0;fuzzySelected.forEach(function(i){
    if(D.fuzzyData[i]){D.fuzzyData[i].tr=v;c++;}
  });
  _renderFuzzyLeftTable();
  showToast('success','已批量设置 '+c+' 条报价交易员');
}

function legacyFuzzyConfirmToIntent(){
  if(!authToken){showFuzzyMsg('error','应急模式为只读，认证服务恢复后才能确认报价');return;}
  if(fuzzySelected.size===0){showFuzzyMsg('error','请先选择报价数据');return;}
  var count=0;
  var now=new Date();
  var timeStr=now.getHours().toString().padStart(2,'0')+':'+now.getMinutes().toString().padStart(2,'0')+':'+now.getSeconds().toString().padStart(2,'0');
  
  // 先添加到模糊报价
  fuzzySelected.forEach(function(i){
    var r=D.fuzzyData[i];
    if(!r)return;
    var dirObj=r.conditions?r.conditions.find(function(c){return c.label==='报价方向';}):null;
    var condText=r.conditions?r.conditions.filter(function(c){return c.label!=='报价方向';}).map(function(c){return c.value;}).join(' '):'';
    D.boardIntent.push({
      trader:currentUser?currentUser.name:'',
      content:r.raw||'',
      result:(dirObj?dirObj.value+' ':'')+condText,
      inst:r.inst||'',
      salesTr:r.tr||'',
      time:timeStr,
      status:'有效'
    });
    count++;
  });
  
  // 然后删除已确认的数据
  Array.from(fuzzySelected).sort(function(a,b){return b-a;}).forEach(function(i){D.fuzzyData.splice(i,1);});
  fuzzySelected.clear();
  D.fuzzyData.forEach(function(r,i){r._idx=i;});
  
  // 重新选中第一个数据，或者清空选中状态
  if(D.fuzzyData.length>0){
    _activeFuzzyIdx=0;
  }else{
    _activeFuzzyIdx=-1;
  }
  
  renderAll();
  showToast('success','已提交 '+count+' 条到模糊报价');
}
async function doDeleteQuotes(){
  if(quoteSelected.size===0){showToast('error','请先勾选要删除的数据');return;}
  const rows=selectedQuoteRows();
  try{
    const response=await authFetch(API_BASE+'/quote-confirm/delete',{method:'POST',body:JSON.stringify({recognitionIds:rows.map(row=>row.recognitionId),reason:'用户删除待确认报价'})});
    const body=await response.json();if(!response.ok)throw new Error(body.detail||'删除失败');
    quoteSelected.clear();await loadQuoteConfirmBatches(false);if(Number(body.deleted)>0&&D.quoteConfirm.length===0)clearQuoteBatchInputs('confirm');showToast('success','已删除 '+body.deleted+' 条待确认报价');
  }catch(error){showToast('error',error.message||'删除失败，原待确认数据已保留');await loadQuoteConfirmBatches(false);}
}
function doDeletePrecise(){
  if(preciseSelected.size===0){showToast('error','请先勾选要删除的数据');return;}
  const count=preciseSelected.size;
  Array.from(preciseSelected).sort((a,b)=>b-a).forEach(i=>D.preciseData.splice(i,1));
  preciseSelected.clear();
  D.preciseData.forEach((r,i)=>{r._idx=i;});
  if(D.preciseData.length===0)clearQuoteBatchInputs('precise');
  renderPreciseTable();
  updatePreciseSelectedCount();
  updateBadges();
}

function doDeleteFuzzy(){
  if(fuzzySelected.size===0){showFuzzyMsg('error','请先勾选要删除的数据');return;}
  var count=fuzzySelected.size;
  Array.from(fuzzySelected).sort(function(a,b){return b-a;}).forEach(function(i){D.fuzzyData.splice(i,1);});
  fuzzySelected.clear();
  D.fuzzyData.forEach(function(r,i){r._idx=i;});
  if(D.fuzzyData.length===0)clearQuoteBatchInputs('fuzzy');
  // 删除后自动选中第一个数据，或者清空选中状态
  if(D.fuzzyData.length>0){
    _activeFuzzyIdx=0;
  }else{
    _activeFuzzyIdx=-1;
  }
  _renderFuzzyLeftTable();
  _renderFuzzyRightTable();
  document.getElementById('fuzzyTotal').textContent=D.fuzzyData.length;
}

function quoteBatchDirection(dir){
  if(quoteSelected.size===0){showToast('error','请先勾选数据');return;}
  commitQuoteConfirmTableControls();
  let count=0;selectedQuoteRows().forEach(row=>{setQuoteConfirmRowField(row,'d',dir);count++;});
  renderQuoteGrouped();updateQuoteSelectedCount();
}
function preciseBatchDirection(dir){
  if(preciseSelected.size===0){showToast('error','请先勾选数据');return;}
  let count=0;
  preciseSelected.forEach(i=>{if(D.preciseData[i]){D.preciseData[i].d=dir;count++;}});
  renderPreciseTable();
  updatePreciseSelectedCount();
}

function batchSetPriceType(v){
  if(quoteSelected.size===0)return;
  commitQuoteConfirmTableControls();
  let c=0;selectedQuoteRows().forEach(row=>{switchQuoteConfirmPriceType(row,v);c++;});
  renderQuoteGrouped();updateQuoteSelectedCount();;
}
function isoDateAddDays(value,days){const parts=String(value||'').split('-').map(Number),date=new Date(Date.UTC(parts[0],parts[1]-1,parts[2]+days));return date.toISOString().slice(0,10);}
function isoDateWeekday(value){return new Date(String(value)+'T00:00:00Z').getUTCDay();}
function nextWeekday(value,minimumDays){let result=isoDateAddDays(value,minimumDays||0);while(isoDateWeekday(result)===0||isoDateWeekday(result)===6)result=isoDateAddDays(result,1);return result;}
function getTodayStr(){return nextWeekday(quoteToday(),0);}
function getNextTradingDay(){return nextWeekday(quoteToday(),1);}
function batchSetSettlement(v){
  if(quoteSelected.size===0)return;
  commitQuoteConfirmTableControls();
  let c=0;selectedQuoteRows().forEach(row=>{setQuoteConfirmRowField(row,'cs',v);c++;});
  renderQuoteGrouped();updateQuoteSelectedCount();
}
function batchSetInst(v){
  if(quoteSelected.size===0)return;
  commitQuoteConfirmTableControls();
  let c=0;selectedQuoteRows().forEach(row=>{setQuoteConfirmRowField(row,'inst',v);c++;});
  renderQuoteGrouped();updateQuoteSelectedCount();;
}
function batchSetTrader(v){
  if(quoteSelected.size===0)return;
  commitQuoteConfirmTableControls();
  let c=0;selectedQuoteRows().forEach(row=>{setQuoteConfirmRowField(row,'tr',v);c++;});
  renderQuoteGrouped();updateQuoteSelectedCount();;
}
function updateQuoteSelectedCount(){
  var el=document.getElementById('selectedCount');
  if(el)el.textContent=quoteSelected.size;
  var hdr=document.getElementById('quoteSelectAll');
  if(hdr)hdr.checked=D.quoteConfirm.length>0&&quoteSelected.size===D.quoteConfirm.length;
}

function preciseBatchSetPriceType(v){
  if(preciseSelected.size===0)return;
  commitRecognitionTableControls('preciseTableWrap');
  let c=0;preciseSelected.forEach(i=>{if(D.preciseData[i]){switchRecognitionPriceType(D.preciseData[i],v);c++;}});
  renderPreciseTable();updatePreciseSelectedCount();;
}
function preciseBatchSetSettlement(v){
  if(preciseSelected.size===0)return;
  let c=0;preciseSelected.forEach(i=>{if(D.preciseData[i]){setRecognitionRowField(D.preciseData[i],'cs',v);c++;}});
  renderPreciseTable();updatePreciseSelectedCount();
}
function preciseBatchSetInst(v){
  if(preciseSelected.size===0)return;
  let c=0;preciseSelected.forEach(i=>{if(D.preciseData[i]){setRecognitionRowField(D.preciseData[i],'inst',v);c++;}});
  renderPreciseTable();updatePreciseSelectedCount();;
}
function preciseBatchSetTrader(v){
  if(preciseSelected.size===0)return;
  let c=0;preciseSelected.forEach(i=>{if(D.preciseData[i]){setRecognitionRowField(D.preciseData[i],'tr',v);c++;}});
  renderPreciseTable();updatePreciseSelectedCount();;
}

function getActiveBatchContext(){
  const quotePage=document.getElementById('page-quote');
  const confirmPage=document.getElementById('st-quote-confirm');
  const precisePage=document.getElementById('st-quote-precise');
  const isQuoteContext=quotePage&&(quotePage.classList.contains('active-page')||isQuoteEntryModalOpen());
  const isConfirmVisible=isQuoteContext&&confirmPage&&!confirmPage.classList.contains('hidden');
  const isPreciseVisible=isQuoteContext&&precisePage&&!precisePage.classList.contains('hidden');
  if(isConfirmVisible&&quoteSelected.size>0){const indexes=new Set();D.quoteConfirm.forEach((row,index)=>{if(quoteSelected.has(quoteSelectionId(row,index)))indexes.add(index);});return{kind:'confirm',sel:indexes,data:D.quoteConfirm,renderFn:renderQuoteGrouped,setPrice:setQuoteConfirmPrice};}
  if(isPreciseVisible&&preciseSelected.size>0)return{kind:'precise',sel:preciseSelected,data:D.preciseData,renderFn:renderPreciseTable,setPrice:setRecognitionPrice};
  return null;
}
function entryPriceAdjustmentBase(row){
  const current=valuationNumber(row&&row.p);if(current!==null)return current;
  const record=row&&row._valuationRecord,code=normalizePriceTypeCode(row&&(row.pt||row.priceType)||'');
  if(code==='YTC')return firstValuationNumber(record,['cb_yte'])??valuationNumber(row&&row.cbYte);
  if(code==='CLEAN_PRICE')return firstValuationNumber(record,['cb_nptm','cb_npte','cb_net_price'])??valuationNumber(row&&(row.nzPrice||row.nzNet));
  if(code==='DIRTY_PRICE')return firstValuationNumber(record,['full_price','cb_full_price','cb_fptm','cb_fpte']);
  const recordYtm=firstValuationNumber(record,['cb_ytm']);if(recordYtm!==null)return recordYtm;
  const rowYtm=valuationNumber(row&&(row.cbYtm||row.nzYTM));if(rowYtm!==null)return rowYtm;
  return row&&!String(row.nzVal||'').includes('/')?valuationNumber(row.nzVal):null;
}
function batchAdjustedPrice(row,bpDelta){
  const base=entryPriceAdjustmentBase(row);if(base===null)return null;
  const code=normalizePriceTypeCode(row&&(row.pt||row.priceType)||'');
  const delta=code==='CLEAN_PRICE'||code==='DIRTY_PRICE'?bpDelta:bpDelta/100;
  return Math.max(0,base+delta).toFixed(4);
}
function batchAdjustedAmount(row,delta){
  const current=valuationNumber(row&&row.vol),next=Math.max(0,(current===null?0:current)+delta);
  return Number(next.toFixed(4)).toString();
}
function ensureBatchNumericInputValue(input,fallback){
  if(!input)return;
  sanitizeBatchNumericInput(input,false);
  const value=String(input.value||'').trim();
  if(!value||value==='-'||value==='.')input.value=String(fallback);
}
function adjustBP(inputId,action){
  const input=document.getElementById(inputId);
  if(action!=='auto')ensureBatchNumericInputValue(input,1);
  const ctx=getActiveBatchContext();
  if(!ctx){showToast('error','请先勾选数据');return;}
  const delta=action==='auto'?batchNumericDelta(input,action,1):advanceBatchButtonInput(input,action,1);
  if(delta===null){showToast('error','请输入有效的报价调整值');return;}
  if(ctx.kind==='confirm')commitQuoteConfirmTableControls();else commitRecognitionTableControls('preciseTableWrap');
  let count=0,skipped=0;
  ctx.sel.forEach(i=>{
    const row=ctx.data[i];if(!row)return;
    const next=batchAdjustedPrice(row,delta);if(next===null){skipped++;return;}
    ctx.setPrice(row,next);applyNzDevFields(row);
    count++;
  });
  ctx.renderFn();updateQuoteSelectedCount();updatePreciseSelectedCount();
}

function adjustAmount(inputId,action){
  const input=document.getElementById(inputId);
  if(action!=='auto')ensureBatchNumericInputValue(input,1000);
  const ctx=getActiveBatchContext();
  if(!ctx){showToast('error','请先勾选数据');return;}
  const delta=action==='auto'?batchNumericDelta(input,action,1000):advanceBatchButtonInput(input,action,1000);
  if(delta===null){showToast('error','请输入有效的报量调整值');return;}
  if(ctx.kind==='confirm')commitQuoteConfirmTableControls();else commitRecognitionTableControls('preciseTableWrap');
  let count=0;
  ctx.sel.forEach(i=>{
    const row=ctx.data[i];if(!row)return;
    (ctx.kind==='confirm'?setQuoteConfirmRowField:setRecognitionRowField)(row,'vol',batchAdjustedAmount(row,delta));
    count++;
  });
  ctx.renderFn();updateQuoteSelectedCount();updatePreciseSelectedCount();
}

const TAG_NAME_MAP={'急':'urgent','荐':'rec','整':'whole','OCO':'oco','打包':'packing'};
function toggleExclusiveQuoteFlag(row,field,forceValue){
  if(!row||!['oco','packing'].includes(field))return false;
  const next=forceValue===undefined?!row[field]:!!forceValue;
  row[field]=next;
  if(next)row[field==='oco'?'packing':'oco']=false;
  return next;
}
function quoteBatchTag(tag){
  if(quoteSelected.size===0){showToast('error','请先勾选数据');return;}
  const field=TAG_NAME_MAP[tag]||tag;
  const rows=selectedQuoteRows(),exclusive=['oco','packing'].includes(field),next=rows.some(row=>!row[field]);
  rows.forEach(row=>{if(exclusive)toggleExclusiveQuoteFlag(row,field,next);else row[field]=next;});
  renderQuoteGrouped();updateQuoteSelectedCount();
}
function preciseBatchTag(tag){
  if(preciseSelected.size===0){showToast('error','请先勾选数据');return;}
  const field=TAG_NAME_MAP[tag]||tag;
  const rows=Array.from(preciseSelected).map(i=>D.preciseData[i]).filter(Boolean),exclusive=['oco','packing'].includes(field),next=rows.some(row=>!row[field]);
  preciseSelected.forEach(i=>{
    if(D.preciseData[i]){
      if(exclusive)toggleExclusiveQuoteFlag(D.preciseData[i],field,next);
      else D.preciseData[i][field]=next;
    }
  });
  renderPreciseTable();
  updatePreciseSelectedCount();
}

function fuzzyBatchSetDirection(dir){}
function fuzzyDoBatchFirm(){}
function fuzzyAdjustBP(action){}
function fuzzyAdjustAmount(action){}
function fuzzyBatchToggleTag(tag){}

let ctxIdx=-1;
function showQuoteConfirmCtx(e,idx){
  e.preventDefault();ctxIdx=idx;
  const m=document.getElementById('ctxMenu');
  m.style.left=e.clientX+'px';m.style.top=e.clientY+'px';m.classList.add('show');
  setTimeout(()=>{const r=m.getBoundingClientRect();if(r.right>innerWidth)m.style.left=(e.clientX-r.width)+'px';if(r.bottom>innerHeight)m.style.top=(e.clientY-r.height)+'px';},10);
}
function hideQuoteConfirmCtx(){document.getElementById('ctxMenu').classList.remove('show');ctxIdx=-1;}
function quoteConfirmCtxEdit(){const idx=ctxIdx;hideQuoteConfirmCtx();if(idx<0)return;showEditModal(D.quoteConfirm[idx],idx);}
function quoteConfirmCtxDelete(){const idx=ctxIdx;hideQuoteConfirmCtx();if(idx<0||!D.quoteConfirm[idx])return;quoteSelected.clear();quoteSelected.add(quoteSelectionId(D.quoteConfirm[idx],idx));doDeleteQuotes();}

function showEditModal(r,idx){
  showModal('编辑报价 - '+r.c,`<div class="edit-grid">
    <div class="edit-field"><div class="edit-label">报价方向</div><select class="edit-select" id="em-d"><option ${r.d==='Bid'?'selected':''}>Bid</option><option ${r.d==='Ofr'?'selected':''}>Ofr</option></select></div>
    <div class="edit-field"><div class="edit-label">报价</div><input class="edit-input" id="em-p" value="${r.p}" maxlength="8"></div>
    <div class="edit-field"><div class="edit-label">价格类型</div><select class="edit-select" id="em-pt"><option ${r.pt==='到期收益率'?'selected':''}>到期收益率</option><option ${r.pt==='行权收益率'?'selected':''}>行权收益率</option><option ${r.pt==='净价'?'selected':''}>净价</option><option ${r.pt==='全价'?'selected':''}>全价</option></select></div>
    <div class="edit-field"><div class="edit-label">报量(万)</div><input class="edit-input" id="em-vol" value="${r.vol}" maxlength="7"></div>
    <div class="edit-field"><div class="edit-label">清算速度</div><select class="edit-select" id="em-cs"><option ${r.cs==='T+0'?'selected':''}>T+0</option><option ${r.cs==='T+1'?'selected':''}>T+1</option></select></div>
    <div class="edit-field"><div class="edit-label">对手机构</div><input class="edit-input" id="em-t" value="${r.t}"></div>
    <div class="edit-field"><div class="edit-label">交易员</div><input class="edit-input" id="em-tr" value="${r.tr||''}"></div>
    <div class="edit-field" style="grid-column:1/3"><div class="edit-label">备注</div><input class="edit-input" id="em-note" value="${r.note||''}" style="width:100%"></div>
  </div>
  <div style="margin-top:12px;display:flex;gap:8px;flex-wrap:wrap">
    <label class="tb-checkbox"><input type="checkbox" id="em-urgent" ${r.urgent?'checked':''}> 急</label>
    <label class="tb-checkbox"><input type="checkbox" id="em-rec" ${r.rec?'checked':''}> 荐</label>
    <label class="tb-checkbox"><input type="checkbox" id="em-whole" ${r.whole?'checked':''}> 整</label>
  </div>
  <div class="modal-footer" style="padding:12px 0 0;border-top:1px solid #21262d;margin-top:16px"><button class="tb-btn tb-btn-default" onclick="closeModal()">取消</button><button class="tb-btn tb-btn-primary" onclick="saveEdit(${idx})">保存</button></div>`);
}
function saveEdit(idx){
  const r=D.quoteConfirm[idx];
  r.d=document.getElementById('em-d').value;r.p=document.getElementById('em-p').value;r.pt=document.getElementById('em-pt').value;
  r.vol=document.getElementById('em-vol').value;r.cs=document.getElementById('em-cs').value;
  r.t=document.getElementById('em-t').value;r.tr=document.getElementById('em-tr').value;r.note=document.getElementById('em-note').value;
  r.urgent=document.getElementById('em-urgent').checked;r.rec=document.getElementById('em-rec').checked;
  r.whole=document.getElementById('em-whole').checked;
  closeModal();renderQuoteGrouped();;
}

const bondPool = [
  {m:'2.50Y',c:'252980.SH',n:'23大理债',d:'Ofr',p:'4.00',vol:'500',cs:'T+1',pt:'到期收益率',dev:'-0.879'},
  {m:'1.22Y+1Y',c:'188572.SH',n:'21柳发02',d:'Ofr',p:'3.50',vol:'500',cs:'T+1',pt:'行权收益率',dev:'-2.786'},
  {m:'1.25Y',c:'032000764.IB',n:'20清池投资PPN001',d:'Ofr',p:'4.50',vol:'200',cs:'T+1',pt:'到期收益率',dev:'-0.211'},
  {m:'58D+3Y',c:'032280666.IB',n:'22惠民城建PPN004',d:'Ofr',p:'3.20',vol:'300',cs:'T+1',pt:'行权收益率',dev:'-11.491'},
  {m:'3.10Y',c:'102484688.IB',n:'25中鼎股份MTN001',d:'Ofr',p:'1.88',vol:'4000',cs:'T+1',pt:'到期收益率',dev:'-0.520'},
  {m:'2.50Y',c:'252980.SH',n:'23大理债',d:'Bid',p:'3.80',vol:'1000',cs:'T+1',pt:'到期收益率',dev:'-1.079'},
  {m:'1.22Y+1Y',c:'188572.SH',n:'21柳发02',d:'Bid',p:'3.30',vol:'800',cs:'T+1',pt:'行权收益率',dev:'-2.986'},
  {m:'1.25Y',c:'032000764.IB',n:'20清池投资PPN001',d:'Bid',p:'4.30',vol:'500',cs:'T+0',pt:'到期收益率',dev:'-0.411'},
  {m:'58D+3Y',c:'032280666.IB',n:'22惠民城建PPN004',d:'Bid',p:'3.00',vol:'200',cs:'T+1',pt:'行权收益率',dev:'-11.691'},
  {m:'4.50Y',c:'102480625.IB',n:'24晋能山西MTN002',d:'Bid',p:'2.26',vol:'1500',cs:'T+1',pt:'到期收益率',dev:'-0.890'},
  {m:'4.92Y',c:'2021017.IB',n:'20青田农商二级01',d:'Bid',p:'1.92',vol:'3000',cs:'T+1',pt:'到期收益率',dev:'-0.150'},
  {m:'8.13Y',c:'240210.IB',n:'24国开10',d:'Bid',p:'1.86',vol:'2000',cs:'T+1',pt:'到期收益率',dev:'-1.250'},
  {m:'1.13Y',c:'170010.IB',n:'17附息国债10',d:'Bid',p:'1.55',vol:'5000',cs:'T+0',pt:'到期收益率',dev:'0.820'},
  {m:'5.22Y',c:'210210.IB',n:'21国开10',d:'Bid',p:'1.90',vol:'1000',cs:'T+1',pt:'到期收益率',dev:'-0.330'},
  {m:'37.17Y',c:'130010.IB',n:'13附息国债10',d:'Ofr',p:'2.15',vol:'800',cs:'T+1',pt:'到期收益率',dev:'-0.180'},
  {m:'2.14Y+2Y',c:'102484124.IB',n:'24首开MTN006',d:'Ofr',p:'2.93',vol:'1000',cs:'T+1',pt:'到期收益率',dev:'-0.080'},
  {m:'1.61Y+N',c:'102480625.IB',n:'24晋能山西MTN002',d:'Ofr',p:'2.28',vol:'500',cs:'T+1',pt:'到期收益率',dev:'-0.670'},
  {m:'0.75Y',c:'012582704.IB',n:'25南京铁建SCP004',d:'Bid',p:'1.58',vol:'1000',cs:'T+0',pt:'到期收益率',dev:'0.120'},
  {m:'1.96Y',c:'255258.SH',n:'24康居01',d:'Bid',p:'1.92',vol:'10000',cs:'T+1',pt:'到期收益率',dev:'-0.450'},
  {m:'6.25Y',c:'102580247.IB',n:'25浙交投MTN001',d:'Ofr',p:'2.54',vol:'2000',cs:'T+1',pt:'到期收益率',dev:'-0.310'},
];

let preciseSelected = new Set(), fuzzySelected = new Set(), mgmtTodaySelected = new Set(), mgmtHistorySelected = new Set();

function genIdentifiedData(count) {
  const result = [];
  const src = Math.random() > 0.5 ? 'ideal' : 'qt';
  const srcColor = src === 'ideal' ? '#3b82f6' : '#3fb950';
  const insts = ['创金合信','平安基金','中信证券','招银理财','光大证券'];
  const inst = insts[Math.floor(Math.random()*insts.length)];
  for (let i = 0; i < count; i++) {
    const b = bondPool[i % bondPool.length];
    result.push({...b, src, srcColor, inst, time: '14:'+String(Math.floor(Math.random()*60)).padStart(2,'0'), t: inst, tr: ['丁茂国','赵明','张伟','周明哲','林诗涵'][Math.floor(Math.random()*5)], _idx: i});
  }
  return result;
}

function recognitionEscapeHtml(value){return String(value==null?'':value).replace(/[&<>"']/g,ch=>({'&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;',"'":'&#39;'}[ch]));}
function recognitionSourceBadge(row,field){
  return '';
}
function recognitionActionBadge(row){
  const action=row&&row.quoteAction||'NEW';
  const label=action==='CANCEL'?'撤销':action==='UPDATE'?'更新':'新增';
  const color=action==='CANCEL'?'#f85149':action==='UPDATE'?'#d29922':'#3fb950';
  return '<span style="font-size:inherit;font-weight:700;color:'+color+';white-space:nowrap">'+label+'</span>';
}
function clearRecognitionFieldError(row,field,control){
  if(row&&Array.isArray(row._confirmMissingFields))row._confirmMissingFields=row._confirmMissingFields.filter(key=>key!==field);
  if(control&&control.classList)control.classList.remove('required-missing');
}
function recognitionFieldErrorClass(row,field){return row&&Array.isArray(row._confirmMissingFields)&&row._confirmMissingFields.includes(field)?'required-missing':'';}
function recognitionCellErrorClass(row,field){return recognitionFieldErrorClass(row,field)?'recognition-required-cell':'';}
function recognitionReadonlyField(value,style){
  return '<input type="text" readonly tabindex="-1" value="'+recognitionEscapeHtml(value||'')+'" style="width:100%;'+(style||'')+'">';
}
function formattedRecognitionPrice(value){
  if(value===null||value===undefined||value==='')return'';
  const number=Number(value);return Number.isFinite(number)?number.toFixed(4):String(value);
}
function recognitionExplicitValue(row,field,value){
  const meta=row&&row._fieldMeta&&row._fieldMeta[field];
  if(meta&&(meta.explicit===false||meta.source==='UI_PLACEHOLDER'||meta.source==='FRONTEND_POLICY_FALLBACK'))return'';
  return value===null||value===undefined?'':value;
}
function recognitionEditableValue(row,field){
  if(row&&Array.isArray(row._clearedFields)&&row._clearedFields.includes(field))return'';
  const value=row&&row[field];return value===null||value===undefined?'':String(value);
}
function setRecognitionRowField(row,field,value,control){
  if(!row)return;
  const normalized=value===null||value===undefined?'':String(value);
  row[field]=normalized;
  const cleared=new Set(Array.isArray(row._clearedFields)?row._clearedFields:[]);
  if(normalized==='')cleared.add(field);else cleared.delete(field);
  row._clearedFields=Array.from(cleared);
  clearRecognitionFieldError(row,field,control);
}
function commitRecognitionRowControls(wrapId,idx,source){
  const data=window['_'+wrapId+'Data'],row=data&&data[idx];
  const tr=source&&source.closest?source.closest('tr'):null;
  if(!row||!tr||!tr.querySelectorAll)return;
  tr.querySelectorAll('[data-rec-field]').forEach(control=>control.dataset.recField==='p'?setRecognitionPrice(row,control.value,control):setRecognitionRowField(row,control.dataset.recField,control.value,control));
}
function commitRecognitionTableControls(wrapId){
  const wrap=document.getElementById(wrapId);
  if(!wrap||!wrap.querySelectorAll)return;
  wrap.querySelectorAll('tbody tr').forEach((tr,idx)=>{
    const source=tr.querySelector('[data-rec-field]');
    if(source)commitRecognitionRowControls(wrapId,idx,source);
  });
}

function renderIdentifiedTable(wrapId, data, selectedSet, selectAllId, countId, ofrId, bidId) {
  const wrap = document.getElementById(wrapId);
  if (!wrap) return;
  let tbody = wrap.querySelector('tbody');
  if (!tbody) {
    const hdrs = '<tr><th style="width:28px"><input type="checkbox" id="'+selectAllId+'" onchange="window[\'_'+wrapId+'SelectAll\'](this.checked)"></th><th>类型</th><th style="text-align:left">期限</th><th style="text-align:left">代码</th><th style="text-align:left">简称</th><th>方向</th><th>报价</th><th>价格类型</th><th>报量(万)</th><th>操作</th><th>报价机构</th><th>报价交易员</th><th>交易日期</th><th>清算速度</th><th>中债估值</th><th>中债净价</th><th>中债偏离</th><th>中证(行/到)</th></tr>';
    const tableClass = wrapId === 'preciseTableWrap' ? 'dt precise-result-table' : 'dt';
    const colgroup = wrapId === 'preciseTableWrap' ? '<colgroup><col><col><col><col><col><col><col><col><col><col><col><col><col><col><col><col><col><col></colgroup>' : '';
    wrap.innerHTML = '<table class="' + tableClass + '" style="min-width:1480px">' + colgroup + '<thead>' + hdrs + '</thead><tbody id="' + wrapId.replace('Wrap','Body') + '"></tbody></table>';
    tbody = wrap.querySelector('tbody');
  }
  const cntEl = document.getElementById(countId); const ofrEl = document.getElementById(ofrId); const bidEl = document.getElementById(bidId);
  if (data.length === 0) {
    // 显示25行空表格（带横向分割线）
    var thead=wrap.querySelector('thead');
    var cols=thead?thead.querySelector('tr').children.length:17;
    var emptyRows='';
    for(var i=0;i<25;i++){
      emptyRows+='<tr><td colspan="'+cols+'" style="border-bottom:1px solid #21262d;height:32px"></td></tr>';
    }
    tbody.innerHTML=emptyRows;
    if (cntEl) cntEl.textContent = '0'; if (ofrEl) ofrEl.textContent = 'Ofr: 0'; if (bidEl) bidEl.textContent = 'Bid: 0';
    return;
  }

  let html = '';
  for (const r of data) {
    const bidOfColor = r.d === 'Bid' ? '#f59e0b' : '#3b82f6';
    const isCancel=r.quoteAction==='CANCEL';
    const cancelDirection=r.cancelScope==='BID'?'Bid':r.cancelScope==='OFR'?'Ofr':r.d==='Bid'?'Bid':'Ofr';
    const directionControl=isCancel
      ?recognitionReadonlyField(cancelDirection,'color:'+(cancelDirection==='Bid'?'#f59e0b':'#3b82f6')+';font-weight:600;')
      :'<select data-rec-field="d" data-client-value="'+recognitionEscapeHtml(r.d||'')+'" data-client-class="client-select-row client-select-direction" class="client-select-source '+recognitionFieldErrorClass(r,'d')+'" onchange="const row=window[\"_'+wrapId+'Data\"]['+r._idx+'];row.d=this.value;clearRecognitionFieldError(row,\'d\',this);window[\"_'+wrapId+'Render\"]()"><option '+(r.d==='Bid'?'selected':'')+'>Bid</option><option '+(r.d==='Ofr'?'selected':'')+'>Ofr</option></select>'+recognitionSourceBadge(r,'direction');
    const priceControl=isCancel
      ?recognitionReadonlyField(formattedRecognitionPrice(recognitionExplicitValue(r,'price',r.p)),'color:#d29922;font-family:JetBrains Mono,monospace;text-align:right;font-weight:700;')
      :'<input data-rec-field="p" class="'+recognitionFieldErrorClass(r,'p')+'" value="'+formattedRecognitionPrice(recognitionEditableValue(r,'p'))+'" placeholder="待补" style="color:#d29922;font-family:JetBrains Mono,monospace;text-align:right;font-weight:700" oninput="this.value=this.value.replace(/[^0-9.]/g,\'\');const row=window[\"_'+wrapId+'Data\"]['+r._idx+'];setRecognitionPrice(row,this.value,this);applyNzDevFields(row)">';
    const priceTypeControl=isCancel
      ?recognitionReadonlyField(recognitionExplicitValue(r,'price_type',r.pt))
      :'<select data-rec-field="pt" data-client-value="'+recognitionEscapeHtml(recognitionEditableValue(r,'pt'))+'" data-client-class="client-select-row client-select-price" class="client-select-source '+recognitionFieldErrorClass(r,'pt')+'" onchange="const row=window[\"_'+wrapId+'Data\"]['+r._idx+'];switchRecognitionPriceType(row,this.value,this);window[\"_'+wrapId+'Render\"]()"><option '+(recognitionEditableValue(r,'pt')==='到期收益率'?'selected':'')+'>到期收益率</option><option '+(recognitionEditableValue(r,'pt')==='行权收益率'?'selected':'')+'>行权收益率</option><option '+(recognitionEditableValue(r,'pt')==='净价'?'selected':'')+'>净价</option><option '+(recognitionEditableValue(r,'pt')==='全价'?'selected':'')+'>全价</option></select>';
    const quantityControl=isCancel
      ?recognitionReadonlyField(recognitionExplicitValue(r,'quantity',r.vol),'color:#d29922;font-family:JetBrains Mono,monospace;text-align:right;font-weight:700;')
      :'<input data-rec-field="vol" class="'+recognitionFieldErrorClass(r,'vol')+'" value="'+recognitionEscapeHtml(recognitionEditableValue(r,'vol'))+'" placeholder="待补" style="color:#d29922;font-family:JetBrains Mono,monospace;text-align:right;font-weight:700" oninput="this.value=this.value.replace(/[^0-9.]/g,\'\');const row=window[\"_'+wrapId+'Data\"]['+r._idx+'];setRecognitionRowField(row,\'vol\',this.value,this)">'+recognitionSourceBadge(r,'quantity');
    const tradeDateControl=isCancel
      ?recognitionReadonlyField(recognitionExplicitValue(r,'trade_date',r.date))
      :'<input data-rec-field="date" type="date" min="'+recognitionEscapeHtml(quoteToday())+'" class="quote-entry-date '+recognitionFieldErrorClass(r,'date')+'" value="'+recognitionEscapeHtml(recognitionEditableValue(r,'date'))+'" style="color:#c9d1d9;width:112px;background:transparent" onchange="const row=window[\"_'+wrapId+'Data\"]['+r._idx+'];enforceQuoteTradeDate(row,this.value,setRecognitionRowField,this)">'+recognitionSourceBadge(r,'trade_date');
    const settlementControl=isCancel
      ?recognitionReadonlyField(recognitionExplicitValue(r,'settlement',r.cs))
      :'<select data-rec-field="cs" data-client-value="'+recognitionEscapeHtml(recognitionEditableValue(r,'cs'))+'" data-client-class="client-select-row client-select-settlement" class="client-select-source '+recognitionFieldErrorClass(r,'cs')+'" onchange="const row=window[\"_'+wrapId+'Data\"]['+r._idx+'];setRecognitionRowField(row,\'cs\',this.value,this)"><option '+(recognitionEditableValue(r,'cs')==='T+0'?'selected':'')+'>T+0</option><option '+(recognitionEditableValue(r,'cs')==='T+1'?'selected':'')+'>T+1</option><option '+(recognitionEditableValue(r,'cs')==='T+2'?'selected':'')+'>T+2</option><option '+(recognitionEditableValue(r,'cs')==='T+3'?'selected':'')+'>T+3</option></select>'+recognitionSourceBadge(r,'settlement');
    html += `<tr>
      <td><input type="checkbox" ${selectedSet.has(r._idx)?'checked':''} onclick="commitRecognitionRowControls('${wrapId}',${r._idx},this);window._${wrapId}Toggle(${r._idx},this.checked)"></td>
      <td>${recognitionActionBadge(r)}</td>
      <td style="text-align:left" title="${recognitionEscapeHtml(r.m||'')}">${recognitionEscapeHtml(r.m||'')}</td><td class="${recognitionCellErrorClass(r,'identity')}" style="text-align:left" title="${recognitionEscapeHtml(r.c||'')}">${recognitionEscapeHtml(r.c||'')}</td><td class="${recognitionCellErrorClass(r,'identity')}" style="text-align:left" title="${recognitionEscapeHtml(r.n||'')}">${recognitionEscapeHtml(r.n||'')}</td>
      <td class="field-underline">${directionControl}</td>
      <td class="field-underline">${priceControl}</td>
      <td class="field-underline">${priceTypeControl}</td>
      <td class="field-underline">${quantityControl}</td>
      <td style="white-space:nowrap"><span class="tag-interactive ${r.oco?'on':'off'}" style="background:rgba(121,192,255,0.12);color:#79c0ff;border:1px solid rgba(121,192,255,0.3);padding:1px 5px;border-radius:3px;font-size:10px;font-weight:600" onclick="event.stopPropagation();toggleExclusiveQuoteFlag(window['_${wrapId}Data'][${r._idx}],'oco');window['_${wrapId}Render']()">OCO</span> <span class="tag-interactive ${r.packing?'on':'off'}" style="background:rgba(210,153,34,0.12);color:#d29922;border:1px solid rgba(210,153,34,0.3);padding:1px 5px;border-radius:3px;font-size:10px;font-weight:600" onclick="event.stopPropagation();toggleExclusiveQuoteFlag(window['_${wrapId}Data'][${r._idx}],'packing');window['_${wrapId}Render']()">打包</span> <span class="tag-interactive ${r.urgent?'on':'off'}" style="background:rgba(239,68,68,0.12);color:#ef4444;border:1px solid rgba(239,68,68,0.3);padding:1px 5px;border-radius:3px;font-size:10px;font-weight:600" onclick="event.stopPropagation();window['_${wrapId}Data'][${r._idx}].urgent=!window['_${wrapId}Data'][${r._idx}].urgent;window['_${wrapId}Render']()">急</span> <span class="tag-interactive ${r.rec?'on':'off'}" style="background:rgba(210,153,34,0.12);color:#d29922;border:1px solid rgba(210,153,34,0.3);padding:1px 5px;border-radius:3px;font-size:10px;font-weight:600" onclick="event.stopPropagation();window['_${wrapId}Data'][${r._idx}].rec=!window['_${wrapId}Data'][${r._idx}].rec;window['_${wrapId}Render']()">荐</span> <span class="tag-interactive ${r.whole?'on':'off'}" style="background:rgba(121,192,255,0.12);color:#79c0ff;border:1px solid rgba(121,192,255,0.3);padding:1px 5px;border-radius:3px;font-size:10px;font-weight:600" onclick="event.stopPropagation();window['_${wrapId}Data'][${r._idx}].whole=!window['_${wrapId}Data'][${r._idx}].whole;window['_${wrapId}Render']()">整</span></td>
      <td class="field-underline quote-party-cell"><input data-rec-field="inst" class="quote-party-input" autocomplete="off" value="${recognitionEscapeHtml(recognitionEditableValue(r,'inst'))}" title="${recognitionEscapeHtml(recognitionEditableValue(r,'inst'))}" style="color:#c9d1d9" onfocus="this.style.borderBottomColor='#79c0ff';prepareCrmInstitutionSearch(this)" oninput="setRecognitionRowField(window[\"_${wrapId}Data\"][${r._idx}],\'inst\',this.value,this)" onblur="this.style.borderBottomColor='#30363d'"></td>
      <td class="field-underline quote-party-cell"><input data-rec-field="tr" class="quote-party-input" autocomplete="off" value="${recognitionEscapeHtml(recognitionEditableValue(r,'tr'))}" title="${recognitionEscapeHtml(recognitionEditableValue(r,'tr'))}" style="color:#c9d1d9" onfocus="this.style.borderBottomColor='#79c0ff';prepareCrmTraderSearch(this,window[\"_${wrapId}Data\"][${r._idx}].inst)" oninput="setRecognitionRowField(window[\"_${wrapId}Data\"][${r._idx}],\'tr\',this.value,this)" onblur="this.style.borderBottomColor='#30363d'"></td>
      <td class="field-underline">${tradeDateControl}</td>
      <td class="field-underline">${settlementControl}</td>
      <td style="color:#c9d1d9">${isCancel?'':formatChinaBondValuation(r)}</td>
      <td style="color:#c9d1d9">${isCancel?'':(r.nzPrice||'—')}</td>
      <td class="neg">${isCancel?'':formatNzDevDisplay(r)}</td>
      <td style="color:#c9d1d9">${isCancel?'':(r.csVal||'—')}</td>
    </tr>`;
  }
  tbody.innerHTML = html;
  if(typeof upgradeClientSelects==='function')upgradeClientSelects(tbody);
  if(typeof applyQuoteFlagClasses==='function')applyQuoteFlagClasses(tbody);

  const ofrCnt = data.filter(r=>r.quoteAction!=='CANCEL'&&r.d==='Ofr').length;
  const bidCnt = data.filter(r=>r.quoteAction!=='CANCEL'&&r.d==='Bid').length;
  if (cntEl) cntEl.textContent = data.length;
  if (ofrEl) ofrEl.textContent = 'Ofr: ' + ofrCnt;
  if (bidEl) bidEl.textContent = 'Bid: ' + bidCnt;
  if (cntEl) cntEl.textContent = data.length;
  if (ofrEl) ofrEl.textContent = 'Ofr: ' + ofrCnt;
  if (bidEl) bidEl.textContent = 'Bid: ' + bidCnt;
}

window._preciseTableWrapSelectAll = function(checked) {
  commitRecognitionTableControls('preciseTableWrap');
  D.preciseData.forEach((r,i) => { if(checked) preciseSelected.add(i); else preciseSelected.delete(i); });
  renderPreciseTable();
  updatePreciseSelectedCount();
};
window._preciseTableWrapToggle = function(idx, checked) {
  if(checked) preciseSelected.add(idx); else preciseSelected.delete(idx);
  renderPreciseTable();
  updatePreciseSelectedCount();
};
function updatePreciseSelectedCount() {
  const el = document.getElementById('preciseSelectedCount');
  if (el) el.textContent = preciseSelected.size;
  const hdr = document.getElementById('preciseSelectAll');
  if (hdr) hdr.checked = D.preciseData.length > 0 && preciseSelected.size === D.preciseData.length;
}
window._preciseTableWrapData = D.preciseData;
window._preciseTableWrapRender = renderPreciseTable;

window._fuzzyTableWrapSelectAll = function(checked) {
  D.fuzzyData.forEach((r,i) => { r.selected = checked; if(checked) fuzzySelected.add(i); else fuzzySelected.delete(i); });
  _renderFuzzyLeftTable();
  _renderFuzzyRightTable();
};
window._fuzzyTableWrapToggle = function(idx, checked) {
  D.fuzzyData[idx].selected = checked;
  if(checked) fuzzySelected.add(idx); else fuzzySelected.delete(idx);
  _renderFuzzyLeftTable();
  _renderFuzzyRightTable();
};
window._fuzzyTableWrapData = D.fuzzyData;
window._fuzzyTableWrapRender = function(){ _renderFuzzyLeftTable(); _renderFuzzyRightTable(); };

function renderPreciseTable() { renderIdentifiedTable('preciseTableWrap', D.preciseData, preciseSelected, 'preciseSelectAll', 'preciseTotal', 'preciseOfrCnt', 'preciseBidCnt'); fillEmptyRows('preciseTableBody'); }
function renderFuzzyTable() { _renderFuzzyLeftTable(); fillEmptyRows('fuzzyLeftTableBody'); _renderFuzzyRightTable(); }

var _preciseStatusTimer=null;
function showPreciseStatus(msg,color,bg){
  var st=document.getElementById('preciseStatus');
  if(_preciseStatusTimer){clearTimeout(_preciseStatusTimer);_preciseStatusTimer=null;}
  st.textContent=msg;st.style.color=color;st.style.background=bg;
  st.style.top=(document.getElementById('preciseInputWrap').getBoundingClientRect().bottom+8)+'px';
  st.style.display='block';
  _preciseStatusTimer=setTimeout(function(){st.style.display='none';_preciseStatusTimer=null;},3000);
}
const _PRECISE_EXAMPLES=[
  'bid 240210.IB 2.9155 2000 T+1',
  'ref bid 240210.IB 2.9155 2000',
  '252980 3.80/3.85 1000/500',
  'refer 252980 3.80/3.85 1000/500',
  'bid 102580247.IB 净价 100.125 3000 T+0',
  'ref 102580247.IB',
  'ofr 252980.SH 全价 105.680 1500 T+1',
  '收 21柳发02 3.50 500',
  '24国开10 bid 2.9155 出 ofr 2.92 2000',
  'bid 210210.IB 净价 101.020 1000 T+1',
  'ofr 240210.IB 全价 102.380 2000 T+0'
];
const _FUZZY_EXAMPLES=[
  '收 城投 AAA 3到5Y 上市 公募',
  'bid 短融 中票 央企 有担保 煤炭',
  '出 银行间 银行二级 商金债 2025',
  'ofr 非上市 私募 含权 赎回权 化工',
  '收 大于20Y 高收益 投资级 AAA+ 省级'
];
function fillPreciseExample(){
  const input=document.getElementById('preciseInput');
  if(input){input.value=_PRECISE_EXAMPLES.join('\n');input.focus({preventScroll:true});}
}
function fillFuzzyExample(){
  const input=document.getElementById('fuzzyInput');
  if(input){input.value=_FUZZY_EXAMPLES.join('\n');}
}
/* valuationNumber moved */
/* firstValuationNumber moved */

function valuationNumber(value){
  if(value===null||value===undefined||value==='')return null;
  const n=Number(String(value).replace(/[,，]/g,'').replace(/bp$/i,'').trim());
  return Number.isFinite(n)?n:null;
}
function firstValuationNumber(record,keys){
  if(!record)return null;
  for(const key of keys){
    const n=valuationNumber(record[key]);
    if(n!==null)return n;
  }
  return null;
}
function isCallableBondRow(row,record){
  if((row&&row.isCallable===true)||(record&&(record.is_callable===true||record.isCallable===true)))return true;
  const optionDate=(record&&(record.next_option_date||record.nextOptionDate))||(row&&row.nextOptionDate);
  if(optionDate!==undefined&&optionDate!==null&&optionDate!==''&&optionDate!==0&&optionDate!=='0')return true;
  const blob=[row&&row.bondType,row&&row.n,row&&row.bond_type_desc,record&&record.special_item,record&&record.bond_type_desc,record&&record.sec_short_name,record&&record.sec_full_name].filter(Boolean).join(' ');
  return /永续|二级资本|可续期|含权|赎回|回售|行权|callable|option/i.test(blob);
}
function normalizePriceTypeCode(priceType){
  const raw=String(priceType||'').trim();
  const upper=raw.toUpperCase();
  const map={'到期收益率':'YTM','行权收益率':'YTC','净价':'CLEAN_PRICE','全价':'DIRTY_PRICE','YTM':'YTM','YTC':'YTC','CLEAN_PRICE':'CLEAN_PRICE','DIRTY_PRICE':'DIRTY_PRICE','NET':'CLEAN_PRICE','CLEAN':'CLEAN_PRICE','DIRTY':'DIRTY_PRICE','FULL':'DIRTY_PRICE'};
  if(map[raw]||map[upper])return map[raw]||map[upper];
  if(raw.includes('行权'))return 'YTC';
  if(raw.includes('到期')||raw.includes('收益'))return 'YTM';
  if(raw.includes('全价'))return 'DIRTY_PRICE';
  if(raw.includes('净价'))return 'CLEAN_PRICE';
  return upper;
}
function resolveYieldBasis(row,record){
  const code=normalizePriceTypeCode((row&&(row.pt||row.priceType||row.priceTypeCode))||'');
  if(code==='YTC')return 'YTC';
  if(code==='YTM')return 'YTM';
  if(code==='CLEAN_PRICE'||code==='DIRTY_PRICE'||code==='PAR')return isCallableBondRow(row,record)?'YTC':'YTM';
  return 'YTM';
}
function convertPriceToYield(quotePrice,priceType,basis,record,row){
  // 计算器：以中债估值价格/收益/修正久期为锚点做一阶换算
  // quoteYield% ≈ cbYield% - (P_quote - P_val) / (P_val * MD) * 100
  const code=normalizePriceTypeCode(priceType);
  let refYield, refPrice, modDur;
  if(basis==='YTC'){
    refYield=firstValuationNumber(record,['cb_yte']) ?? valuationNumber(row&&row.cbYte);
    refPrice=firstValuationNumber(record,['cb_npte','cb_nptm','cb_net_price']) ?? valuationNumber(row&&(row.nzPrice||row.nzNet||row.nzVal));
    modDur=firstValuationNumber(record,['cb_mdte','cb_mdtm']) ?? valuationNumber(row&&(row.cbMdte||row.cbMdtm));
  }else{
    refYield=firstValuationNumber(record,['cb_ytm']) ?? valuationNumber(row&&row.cbYtm);
    refPrice=firstValuationNumber(record,['cb_nptm','cb_net_price']) ?? valuationNumber(row&&(row.nzPrice||row.nzNet));
    modDur=firstValuationNumber(record,['cb_mdtm','cb_mdte']) ?? valuationNumber(row&&(row.cbMdtm||row.cbMdte));
  }
  if(code==='DIRTY_PRICE'){
    refPrice=firstValuationNumber(record,['full_price','cb_full_price']) ?? refPrice;
  }
  if(refYield===null||refPrice===null||modDur===null||!refPrice||!modDur)return null;
  const deltaY=-(Number(quotePrice)-refPrice)/(refPrice*modDur)*100;
  return refYield+deltaY;
}
function formatNzDev(bp){
  if(bp===null||bp===undefined||!Number.isFinite(Number(bp)))return '—';
  const n=Number(bp);
  const fixed=n.toFixed(2);
  return fixed.startsWith('+')?fixed.slice(1):fixed;
}
function nzDevUnitForRow(row){
  const code=normalizePriceTypeCode(row&&(row.pt||row.priceType||row.priceTypeCode)||'');
  if(code==='YTM'||code==='YTC')return 'bp';
  if(code==='CLEAN_PRICE'||code==='DIRTY_PRICE')return 'price_point';
  return '';
}
function formatNzDevDisplay(row){
  const value=row&&row.dev;
  if(value===null||value===undefined||value===''||value==='—')return '—';
  const unit=row.nzDevUnit||nzDevUnitForRow(row);
  return recognitionEscapeHtml(String(value)+(unit==='bp'?'bp':unit==='price_point'?'价格点':''));
}
function formatChinaBondValuation(row){
  if(!row)return '—';
  const maturity=row.cbYtm||'—';
  if(isCallableBondRow(row,row._valuationRecord))return recognitionEscapeHtml(maturity)+' / '+recognitionEscapeHtml(row.cbYte||'—');
  return recognitionEscapeHtml(maturity);
}
function computeNzDev(row,record){
  if(!row)return '—';
  const quote=valuationNumber(row.p!==undefined?row.p:(row.bp!==undefined?row.bp:row.price));
  if(quote===null)return '—';
  const code=normalizePriceTypeCode(row.pt||row.priceType||row.priceTypeCode||'');
  const rec=record||row._valuationRecord||null;
  if(code==='CLEAN_PRICE'){
    const cbNet=firstValuationNumber(rec,['cb_nptm','cb_net_price']);
    return cbNet===null?'—':formatNzDev(quote-cbNet);
  }
  if(code==='DIRTY_PRICE'){
    const cbFull=firstValuationNumber(rec,['full_price','cb_full_price','cb_fptm']);
    return cbFull===null?'—':formatNzDev(quote-cbFull);
  }
  if(code==='SPREAD')return '—';
  const basis=resolveYieldBasis(row,rec);
  let cbYield=null;
  if(basis==='YTC'){
    cbYield=firstValuationNumber(rec,['cb_yte']);
    if(cbYield===null)cbYield=valuationNumber(row.cbYte);
    if(cbYield===null)return '—'; // 严格不回退到期
  }else{
    cbYield=firstValuationNumber(rec,['cb_ytm']);
    if(cbYield===null)cbYield=valuationNumber(row.cbYtm);
    // confirm page may put YTM in nzVal when basis is maturity
    if(cbYield===null && !String(row.nzVal||'').includes('/'))cbYield=valuationNumber(row.nzYTM||row.nzVal);
    if(cbYield===null)return '—';
  }
  if(code==='YTM'||code==='YTC'||String(row.pt||row.priceType||'').includes('收益率')){
    return formatNzDev((quote-cbYield)*100);
  }
  return '—';
}
function applyNzDevFields(target,record){
  if(!target)return;
  if(record)target._valuationRecord=record;
  const dev=computeNzDev(target,record||target._valuationRecord||null);
  target.dev=dev;
  target.nzDev=dev;
  target.nzDevUnit=dev==='—'?'':nzDevUnitForRow(target);
}

function applyValuation(target,record){
  // 估值接口同时返回债券基础资料中的剩余期限。精确报价识别阶段
  // 可能因 DM 查询暂时失败而没有写入 m，这里必须把期限一并回填，
  // 否则后续保存到 live_quotes / 行情中心时会永久丢失该字段。
  const remainingTenor=record&&record.remaining_tenor!=null?String(record.remaining_tenor).trim():'';
  if(remainingTenor)target.m=remainingTenor;
  target.isCallable=isCallableBondRow(target,record);
  const basis=resolveYieldBasis(target,record);
  const exercise=basis==='YTC';
  const cbMaturity=firstValuationNumber(record,['cb_ytm']);
  const cbExercise=firstValuationNumber(record,['cb_yte']);
  const cbNet=firstValuationNumber(record,exercise?['cb_npte','cb_nptm','cb_net_price']:['cb_nptm','cb_npte','cb_net_price']);
  const csYtm=firstValuationNumber(record,exercise?['cs_yte','cs_ytm']:['cs_ytm','cs_yte']);
  const csNet=firstValuationNumber(record,exercise?['cs_npte','cs_nptm','cs_net_price']:['cs_nptm','cs_npte','cs_net_price']);
  const csFull=firstValuationNumber(record,exercise?['cs_fpte','cs_fptm','cs_full_price']:['cs_fptm','cs_fpte','cs_full_price']);
  const mdMaturity=firstValuationNumber(record,['cb_mdtm']);
  const mdExercise=firstValuationNumber(record,['cb_mdte','cb_mdtm']);
  if(cbMaturity!==null)target.cbYtm=cbMaturity.toFixed(4);
  if(cbExercise!==null)target.cbYte=cbExercise.toFixed(4);
  // 中债估值列按比较口径展示对应收益率；净价单独进 nzNet/nzPrice
  const showYtm=exercise?cbExercise:cbMaturity;
  if(showYtm!==null){target.nzYTM=showYtm.toFixed(4); if(!String(target.nzVal||'').match(/^\d/)) target.nzVal=showYtm.toFixed(4);}
  if(cbNet!==null){target.nzPrice=cbNet.toFixed(4);target.nzNet=cbNet.toFixed(4);}
  if(csYtm!==null){target.csYtm=csYtm.toFixed(4);target.csVal=(firstValuationNumber(record,['cs_yte'])!==null?Number(firstValuationNumber(record,['cs_yte'])).toFixed(4):'—')+'/'+(csYtm.toFixed(4));}
  if(csNet!==null){target.csNet=csNet.toFixed(4);}
  if(csFull!==null){target.csFull=csFull.toFixed(4);}
  if(mdMaturity!==null)target.cbMdtm=String(mdMaturity);
  if(mdExercise!==null)target.cbMdte=String(mdExercise);
  target.yieldBasis=basis;
  target._valuationRecord=record;
  applyNzDevFields(target,record);
  target.valuationDate=record.valuation_date||record.data_date||record.issue_date||record.trade_date||'';
  target.valuationStatus='success';
}
function valuationTargets(recognitionId){
  return[D.preciseData.find(row=>row.recognitionId===recognitionId),(D.liveQuotes||[]).find(row=>row.recognitionId===recognitionId)].filter(Boolean);
}
function requestPreciseValuations(rows,attempt){
  attempt=attempt||0;D.valuationRequests=D.valuationRequests||{};
  const requestRows=rows.filter(row=>row&&row.c&&row.quoteAction!=='CANCEL'&&row.valuationStatus!=='success');if(!requestRows.length)return;
  const requestId=makeQuoteId('val');
  requestRows.forEach(row=>{row.valuationStatus='loading';D.valuationRequests[row.recognitionId]=requestId;});
  authFetch(API_BASE+'/bond-valuation',{method:'POST',body:JSON.stringify({codes:requestRows.map(row=>row.c)})})
    .then(response=>response.ok?response.json():Promise.reject(new Error('HTTP '+response.status)))
    .then(data=>{
      if(!data.ok)throw new Error(data.error||'估值接口返回失败');
      const retryRows=[];
      requestRows.forEach(source=>{
        if(D.valuationRequests[source.recognitionId]!==requestId)return;
        const record=data.data&&data.data[String(source.c).toUpperCase()];
        const targets=valuationTargets(source.recognitionId);
        if(record&&targets.length){
          targets.forEach(target=>applyValuation(target,record));
          const confirmed=targets.find(target=>target.quoteId);
          if(confirmed){
            rebuildBoardProjections();syncMgmtToday();syncMgmtHistory();renderAll();
            const event=addQuoteEvent(confirmed,'edit','估值回填');
            persistQuoteWithRetry(confirmed,'edit',event).catch(()=>showToast('error',confirmed.c+' 估值已更新，但后端同步失败'));
          }
          delete D.valuationRequests[source.recognitionId];
        }else retryRows.push(source);
      });
      renderPreciseTable();
      if(retryRows.length)retryValuations(retryRows,attempt);
    }).catch(()=>retryValuations(requestRows,attempt));
}
function retryValuations(rows,attempt){
  const delays=[2000,5000,15000];
  if(attempt>=delays.length){
    rows.forEach(source=>valuationTargets(source.recognitionId).forEach(target=>target.valuationStatus='failed'));
    renderPreciseTable();return;
  }
  setTimeout(()=>requestPreciseValuations(rows,attempt+1),delays[attempt]);
}
function doPreciseIdentify() {
  const text=document.getElementById('preciseInput').value.trim();
  if(!text){showToast('error','请输入报价文本');return;}
  showPreciseStatus('正在识别...','#79c0ff','rgba(121,192,255,0.1)');
  var startTime=Date.now();
  authFetch(API_BASE+'/parse',{method:'POST',body:JSON.stringify({text:text,lookup:true})})
    .then(function(r){return r.json();})
    .then(function(d){
      var elapsed=((Date.now()-startTime)/1000).toFixed(1);
      if(!d.ok||!d.data||d.data.length===0){showPreciseStatus(d.error||'未能识别出有效报价','#ef4444','rgba(239,68,68,0.1)');return;}
      var keepChecked=document.getElementById('preciseKeepText');
      if(keepChecked&&!keepChecked.checked){document.getElementById('preciseInput').value='';}
      var PT_MAP={'YTM':'到期收益率','YTC':'行权收益率','NET':'净价','DIRTY':'全价','SPREAD':'利差','PAR':'面值'};
      var CS_MAP={'T+0':'T+0','T+1':'T+1','T+2':'T+2','T+3':'T+3','明+0':'T+0','明+1':'T+1','当天':'T+0','次日':'T+1'};
      var newResults=[];
      d.data.forEach(function(r){
        if(r._filtered||r.status==='REJECTED')return;
        var quoteAction=r.quote_action||'NEW';
        var dir=(r.direction&&r.direction.value)||'ofr';
        dir=dir.charAt(0).toUpperCase()+dir.slice(1).toLowerCase();
        if(dir!=='Bid'&&dir!=='Ofr'&&dir!=='Both')dir='Ofr';
        var pt=r.price_type&&r.price_type.value?PT_MAP[r.price_type.value]||'到期收益率':quoteAction==='CANCEL'?'':'到期收益率';
        var csRaw=r.settlement&&r.settlement.value?r.settlement.value:quoteAction==='CANCEL'?'':'T+1';
        var cs=CS_MAP[csRaw]||csRaw;
        var inst=r.institution&&r.institution.value?r.institution.value:'';
        var trader=r.trader&&r.trader.value?r.trader.value:'';
        var traderField=trader||inst;
        var confStr=r.confidence!=null?Math.round(r.confidence*100)+'%':'';
        var noteTxt=r._note||'';
        if(r._valuation&&r._valuation.offset!=null)noteTxt=(noteTxt?noteTxt+'；':'')+'估值'+(r._valuation.offset>=0?'+':'')+r._valuation.offset+'bp';
        if(confStr&&r.reason&&r.reason!=='ok')noteTxt=(noteTxt?noteTxt+'；':'')+confStr+' '+r.reason;
        else if(confStr)noteTxt=(noteTxt?noteTxt+'；':'')+confStr;
        if(r.completeness!=null)noteTxt=(noteTxt?noteTxt+'；':'')+'完整度'+Math.round(r.completeness*100)+'%';
        if(r.status&&r.status!=='ACCEPTED')noteTxt=(noteTxt?noteTxt+'；':'')+r.status;
        if(Array.isArray(r.warnings)&&r.warnings.length)noteTxt=(noteTxt?noteTxt+'；':'')+r.warnings.map(function(w){return w.message||w.code||w;}).join('；');
        var tagList=[];
        if(r.bond_name&&r.bond_name.tag)tagList.push(r.bond_name.tag);
        if(r.price&&r.price.type==='SPREAD')tagList.push('利差');
        if(tagList.length)noteTxt=(noteTxt?noteTxt+'；':'')+tagList.join('/');
        var api=r._apiInfo||{};
        var bondCode=r.bond_code&&r.bond_code.value?r.bond_code.value:'';
        if(api.security_id){bondCode=api.security_id;}
        else{
          var codeTags=r.bond_code&&r.bond_code.tags?r.bond_code.tags:[];
          if(bondCode&&codeTags.indexOf('GENERAL_6')>=0)bondCode=bondCode+'.IB';
          else if(bondCode&&(codeTags.indexOf('NCD_9')>=0||codeTags.indexOf('NCD_12')>=0))bondCode=bondCode+'.IB';
          else if(bondCode&&!bondCode.includes('.'))bondCode=bondCode+'.IB';
        }
        var bondName=(r.bond_name&&r.bond_name.value?r.bond_name.value:'')||api.sec_short_name||'';
        var bondTypeTag=api.bond_type_desc||(r.bond_name&&r.bond_name.tag?r.bond_name.tag:'');
        // Bond category is not a tenor. Missing tenor stays blank until master data resolves it.
        var term=api.remaining_tenor||'';
        newResults.push({
          m:term,
          c:bondCode,
          n:bondName,
          bondType:bondTypeTag,
          quoteAction:quoteAction,
          quoteType:quoteAction==='CANCEL'?'撤销':quoteAction==='UPDATE'?'更新':'新增',
          cancelScope:quoteAction==='CANCEL'&&(!r.cancel_scope||r.cancel_scope==='ALL')?(dir==='Bid'?'BID':'OFR'):(r.cancel_scope||'ALL'),
          actionConfidence:r.action_confidence,
          d:dir,
          p:r.price&&r.price.value?r.price.value:'',
          q:'到期',
          vol:r.quantity&&r.quantity.value?r.quantity.value:'',
          t:'',
          inst:inst,
          tr:traderField,
          date:r.trade_date&&r.trade_date.value?r.trade_date.value:quoteAction==='CANCEL'?'':quoteToday(),
          cs:cs,
          dev:'—',nzDevUnit:'',
          pt:pt,
          nzVal:'—',nzPrice:'—',csVal:'—',
          oco:false,packing:false,urgent:false,rec:false,whole:false,
          txt:noteTxt,
          recognitionId:makeQuoteId('recognition'),
          valuationStatus:quoteAction==='CANCEL'?'not_applicable':'pending',
          _apiInfo:api,
          _fieldMeta:r.field_states||{},
          _missingFields:r.missing_fields||[],
          _recognizedFields:r.recognized_fields||[],
          _resolvedFields:r.resolved_fields||[],
          _numericCandidates:r.numeric_candidates||[],
          _bondCandidates:r.bond_candidates||[],
          _completeness:r.completeness,
          _recognitionStatus:r.status,
          _confidence:r.confidence,
          _rules:r.reason,
          _raw:r.raw||''
        });
      });
      if(newResults.length===0){showToast('error','识别失败：未能匹配到有效的债券信息');return;}
      var totalCount=d.data.filter(function(r){return !r._filtered;}).length;
      var failCount=totalCount-newResults.length;
      var nextIdx=D.preciseData.length;
      newResults.forEach(function(r){r._idx=nextIdx++;});
      D.preciseData.splice(0,0,...newResults);
      window._preciseTableWrapData=D.preciseData;
      var reIdx=0;D.preciseData.forEach(function(r){r._idx=reIdx++;});
      preciseSelected=new Set(D.preciseData.map(function(_,i){return i;}));
      renderPreciseTable();
      updatePreciseSelectedCount();
      var ofrCnt=D.preciseData.filter(function(r){return r.quoteAction!=='CANCEL'&&r.d==='Ofr';}).length;
      var bidCnt=D.preciseData.filter(function(r){return r.quoteAction!=='CANCEL'&&r.d==='Bid';}).length;
      var totalEl=document.getElementById('preciseTotal');
      var ofrEl=document.getElementById('preciseOfrCnt');
      var bidEl=document.getElementById('preciseBidCnt');
      if(totalEl)totalEl.textContent=D.preciseData.length;
      if(ofrEl)ofrEl.textContent='Ofr: '+ofrCnt;
      if(bidEl)bidEl.textContent='Bid: '+bidCnt;
      var statusMsg='识别完成 · 成功'+newResults.length+'条';
      if(failCount>0)statusMsg+=' · 失败'+failCount+'条';
      statusMsg+=' · 共'+D.preciseData.length+'条 · 耗时'+elapsed+'s';
      showPreciseStatus(statusMsg,failCount>0?'#f59e0b':'#3fb950',failCount>0?'rgba(245,158,11,0.1)':'rgba(63,185,80,0.1)');
      requestPreciseValuations(newResults,0);
    }).catch(function(e){showPreciseStatus('识别服务异常: '+e.message,'#ef4444','rgba(239,68,68,0.1)');});
}

function parseQuoteLine(line){
  const parts=line.split(/[\s,，]+/);
  if(parts.length<3)return null;
  let dir='',price='',code='',name='',maturity='',vol='',rating='',cs='T+1';
  const first=parts[0].toLowerCase();
  if(first==='bid'||first==='ofr'){dir=first.toUpperCase();parts.shift();}
  else if(first.startsWith('bid')||first.startsWith('ofr')){dir=first.substring(0,3).toUpperCase();parts.shift();}
  else{dir='Bid';}
  for(const p of parts){
    if(/^\d+\.?\d*$/.test(p)&&!price){price=p;}
    else if(/\d+\.\d+\.SH|\.IB$/.test(p)){code=p;}
    else if(/[A-Z]\d|\.SH$|\.IB$/.test(p)){if(!code)code=p;}
    else if(/^\d+(\.\d+)?[YMymDd]+$/.test(p)){maturity=p;}
    else if(/^\d{2,6}$/.test(p)&&!vol){vol=p;}
    else if(/^(AAA|AA\+?|A\+?|BBB|BB|A-1|A-2|A-3)$/i.test(p)){rating=p;}
    else if(/^(T\+[01]|M\+[01])$/i.test(p)){cs=p;}
    else if(!maturity&&!vol){name=p;}
  }
  if(!code&&!name)return null;
  return{m:maturity,c:code,n:name||code,d:dir,p:price,q:'到期',vol:vol,t:'',tr:'',cs:cs,dev:'—',nzDevUnit:'',pt:'到期收益率',nzVal:'—',nzPrice:'—',csVal:'—',oco:false,packing:false,urgent:false,rec:false,whole:false,txt:line};
}

// ===== 拖拽调整输入框高度 =====
let isResizing=false,startY=0,startH=0;
function startPreciseResize(e){
  e.preventDefault();
  isResizing=true;startY=e.clientY;
  startH=document.getElementById('preciseInputWrap').offsetHeight;
  document.body.style.cursor='ns-resize';
  document.body.style.userSelect='none';
  document.addEventListener('mousemove',onPreciseResize);
  document.addEventListener('mouseup',stopPreciseResize);
}
function onPreciseResize(e){
  if(!isResizing)return;
  const h=Math.max(100,Math.min(500,startH+(e.clientY-startY)));
  document.getElementById('preciseInputWrap').style.height=h+'px';
}
function stopPreciseResize(){
  isResizing=false;
  document.body.style.cursor='';
  document.body.style.userSelect='';
  document.removeEventListener('mousemove',onPreciseResize);
  document.removeEventListener('mouseup',stopPreciseResize);
}

// ===== 粘贴文本 =====
async function readClipboardTextForPaste(){
  if(window.desktopAPI&&window.desktopAPI.readClipboardText)return String(await window.desktopAPI.readClipboardText()||'');
  const clipboard=globalThis.navigator&&globalThis.navigator.clipboard;
  if(clipboard&&typeof clipboard.readText==='function'){
    try{return String(await clipboard.readText()||'');}
    catch(error){const denied=new Error('浏览器未授予剪贴板读取权限，请允许权限后重试');denied.code='CLIPBOARD_DENIED';throw denied;}
  }
  const unavailable=new Error('局域网 HTTP 页面不支持自动读取剪贴板，请使用 EXE、HTTPS 页面或按 Ctrl+V 粘贴');
  unavailable.code='CLIPBOARD_UNAVAILABLE';throw unavailable;
}
async function doPasteText(targetId){
  const inputId=targetId||'preciseInput',ta=document.getElementById(inputId);if(!ta)return;
  try{
    const text=await readClipboardTextForPaste();
    if(!text){showToast('info','剪贴板中没有可粘贴的文本');ta.focus();return;}
    const start=Number.isInteger(ta.selectionStart)?ta.selectionStart:ta.value.length,end=Number.isInteger(ta.selectionEnd)?ta.selectionEnd:start;
    ta.value=ta.value.substring(0,start)+text+ta.value.substring(end);
    const caret=start+text.length;ta.focus();if(ta.setSelectionRange)ta.setSelectionRange(caret,caret);
    ta.dispatchEvent(new Event('input',{bubbles:true}));
  }catch(error){
    showToast('error',error.message||'无法读取剪贴板，请手动粘贴');
    ta.focus();
  }
}

// ===== 系统截图识别 =====
async function doSystemScreenshot(targetId){
  const inputId=targetId||'preciseInput';
  if(window.desktopAPI&&window.desktopAPI.capturePrimaryScreen){
    try{
      const image=await window.desktopAPI.capturePrimaryScreen();
      if(image){recognizeImage(image,inputId);return;}
    }catch(error){
      console.warn('桌面截图失败，回退到浏览器捕获:',error);
    }
  }
  if(!navigator.mediaDevices||!navigator.mediaDevices.getDisplayMedia){
    showToast('error','当前浏览器不支持屏幕捕获，请使用系统截图工具截图后粘贴(Ctrl+V)到输入框');
    return;
  }
  navigator.mediaDevices.getDisplayMedia({video:{cursor:'always'},audio:false}).then(function(stream){
    const video=document.createElement('video');
    video.srcObject=stream;
    video.play();
    video.onloadedmetadata=function(){
      setTimeout(function(){
        const canvas=document.createElement('canvas');
        canvas.width=video.videoWidth;
        canvas.height=video.videoHeight;
        canvas.getContext('2d').drawImage(video,0,0);
        stream.getTracks().forEach(function(t){t.stop();});
        canvas.toBlob(function(blob){
          if(blob){
            recognizeImage(blob,inputId);
          }
        },'image/png');
      },200);
    };
  }).catch(function(err){
    if(err.name!=='AbortError')showToast('error','截图失败: '+err.message);
  });
}

// ===== 图片OCR识别 =====
function handleImgImport(input,targetId){
  if(!input.files||!input.files[0])return;
  recognizeImage(input.files[0],targetId||'preciseInput');
  input.value='';
}
function recognizeImage(file,targetId){
  const inputId=targetId||'preciseInput';
  if(typeof Tesseract!=='undefined'){
    Tesseract.recognize(file,'eng+chi_sim',{
      workerPath:new URL('./vendor/tesseract/worker.min.js',window.location.href).href,
      corePath:new URL('./vendor/tesseract/core/',window.location.href).href,
      langPath:new URL('./vendor/tesseract/lang/',window.location.href).href,
      logger:m=>{}
    }).then(({data:{text}})=>{
      appendRecognizedText(text,inputId);
    }).catch(()=>{showToast('error','OCR识别失败，请手动输入');});
  }else{
    const reader=new FileReader();
    reader.onload=function(e){
      appendRecognizedText('[图片内容：'+file.name+']\n请手动识别并输入报价信息',inputId);
    };
    reader.readAsDataURL(file);
  }
}
function appendRecognizedText(text,targetId){
  const ta=document.getElementById(targetId||'preciseInput');
  if(!ta)return;
  const cleaned=text.split(String.fromCharCode(10)).map(l=>l.replace(/\s+/g,' ')).join(String.fromCharCode(10)).replace(new RegExp('('+String.fromCharCode(10)+'){3,}','g'),String.fromCharCode(10)+String.fromCharCode(10)).trim();
  if(ta.value)ta.value+=String.fromCharCode(10)+String.fromCharCode(10)+cleaned;
  else ta.value=cleaned;
}

// ===== 表格导入 =====
function handleXlsImport(input,targetId){
  if(!input.files||!input.files[0])return;
  const inputId=targetId||'preciseInput';
  const file=input.files[0];
  if(typeof XLSX!=='undefined'){
    const reader=new FileReader();
    reader.onload=function(e){
      try{
        const wb=XLSX.read(e.target.result,{type:'array'});
        const ws=wb.Sheets[wb.SheetNames[0]];
        const data=XLSX.utils.sheet_to_json(ws,{header:1,defval:''});
        let text='';
        data.forEach(row=>{
          const line=row.filter(c=>c!=='').join(' ');
          if(line.trim())text+=line.trim()+'\n';
        });
        appendRecognizedText(text,inputId);
      }catch(ex){showToast('error','表格解析失败：'+ex.message);}
    };
    reader.readAsArrayBuffer(file);
  }else{
    showToast('error','表格解析库未加载，请手动输入');
  }
  input.value='';
}
async function requestIntentRecognitions(items){
  const response=await authFetch(API_BASE+'/intent-recognition/parse',{method:'POST',body:JSON.stringify({items})});
  const body=await response.json();
  if(!response.ok||!body.ok)throw new Error(body.error||body.detail||'模糊报价识别失败');
  return Array.isArray(body.data)?body.data:[];
}
async function doFuzzyIdentify() {
  if(typeof fuzzyIntentSyncInFlight!=='undefined'&&fuzzyIntentSyncInFlight){showFuzzyMsg('error','报价正在同步，请稍候再识别');return;}
  const input = document.getElementById('fuzzyInput');
  const text = (input ? input.value : '').trim();
  if (!text) { showToast('error', '请输入文本'); return; }
  const lines = text.split('\n').map(s => s.trim()).filter(Boolean);
  try{
    const requestItems=lines.map((line,i)=>{const meta=_extractQuoteMeta(line);return{clientItemId:String(i),text:line,institution:meta.inst,trader:meta.trader};});
    const recognizedRows=await requestIntentRecognitions(requestItems);
    const byId=new Map(recognizedRows.map(row=>[String(row.clientItemId),row]));
    D.fuzzyData=lines.map((line,i)=>{
      const responseRow=byId.get(String(i));
      const recognized=responseRow&&responseRow.ok?responseRow:{result:'',inst:'',salesTr:'',recognition:{version:'intent-recognition-v3',status:'failed',conditions:[],unmatchedTokens:[line],conflicts:[],result:''}};
      const conditions=(recognized.recognition.conditions||[]).map(c=>({label:c.label,value:c.value,key:c.key,normalizedValue:c.normalizedValue,queryKey:c.queryKey,queryValue:c.queryValue,queryEnabled:c.queryEnabled,region:c.region,range:c.range}));
      return{_idx:i,raw:line,result:recognized.result,recognition:recognized.recognition,conditions,inst:recognized.inst||requestItems[i].institution,tr:recognized.salesTr||requestItems[i].trader,selected:true,modified:0};
    });
  }catch(error){showToast('error',error.message||'模糊报价识别失败');return;}
  fuzzySelected = new Set(D.fuzzyData.map((_, i) => i));
  _activeFuzzyIdx = D.fuzzyData.length > 0 ? 0 : -1;
  _renderFuzzyLeftTable();
  _renderFuzzyRightTable();
  const keepChecked = document.getElementById('fuzzyKeepText');
  if (keepChecked && !keepChecked.checked) { input.value = ''; }
  document.getElementById('fuzzyTotal').textContent = D.fuzzyData.length;
}
// 识别结果展示顺序契约：字段顺序跟随模糊报价左侧面板，后端同版本兜底。
const INTENT_DISPLAY_ORDER_VERSION='intent-display-order-v1';
const INTENT_DISPLAY_FIELD_ORDER=[
  'direction',
  'bond_subtype','hot_tag','tax_exempt','local_bond_type','ncd_type','remaining_tenor','listing_status','issue_method',
  'liquidity','market_maker','market',
  'bank_type','industry','enterprise_nature','guarantee','issuer_rating','cb_rating','rating_outlook','bond_rating','cb_implied','yy_score','cs_implied',
  'embedded_option','repayment_method','coupon_freq','perpetual','subordinated','rate_type','issue_year',
  'lgfv_attr','key_province','lgfv_level','region_grade','region_area'
];
const INTENT_DISPLAY_RESERVED_VALUES={
  direction:['Bid','Ofr'],liquidity:['S','A','B','C','D'],
  market_maker:['上证信用','深证信用','上证利率','深证利率'],market:['银行间','上交所','深交所','北交所']
};
const INTENT_DISPLAY_LABEL_KEYS={
  '报价方向':'direction','细分券种':'bond_subtype','热门标签':'hot_tag','是否免税':'tax_exempt','地方债类型':'local_bond_type',
  'NCD类型':'ncd_type','剩余期限':'remaining_tenor','上市状态':'listing_status','债券上市状态':'listing_status','发行方式':'issue_method',
  '债券流动性':'liquidity','做市标的':'market_maker','上市场所':'market','银行业':'bank_type','行业':'industry',
  '企业性质':'enterprise_nature','担保':'guarantee','主体评级':'issuer_rating','中债资信':'cb_rating','评级展望':'rating_outlook',
  '债项评级':'bond_rating','中债隐含':'cb_implied','YY评分':'yy_score','中证隐含':'cs_implied','含权属性':'embedded_option',
  '偿还方式':'repayment_method','付息方式':'coupon_freq','永续债':'perpetual','次级债':'subordinated','利率方式':'rate_type',
  '发行年份':'issue_year','城投':'lgfv_attr','城投属性':'lgfv_attr','重点省份':'key_province','城投层级':'lgfv_level','地区':'region_grade','地区等级':'region_grade','全国地区':'region_area'
};
let _intentDisplayOrderingCache=null;
function _intentDisplayNormalize(value){return String(value==null?'':value).trim().toUpperCase();}
function _intentDisplayCompareText(a,b){a=String(a||'');b=String(b||'');return a<b?-1:a>b?1:0;}
function _intentDisplayMergeMissingFields(fields){
  const result=[...fields];
  INTENT_DISPLAY_FIELD_ORDER.forEach(key=>{
    if(result.includes(key))return;
    const fallbackIndex=INTENT_DISPLAY_FIELD_ORDER.indexOf(key);
    const next=result.findIndex(existing=>INTENT_DISPLAY_FIELD_ORDER.indexOf(existing)>fallbackIndex);
    result.splice(next<0?result.length:next,0,key);
  });
  return result;
}
function _intentDisplayOrdering(){
  if(_intentDisplayOrderingCache)return _intentDisplayOrderingCache;
  const panel=document.getElementById('filters-intent');
  const domFields=[];
  const valuesByField=new Map();
  if(panel){
    panel.querySelectorAll('.filter-group').forEach(group=>{
      const firstTag=group.querySelector('.filter-tag[data-field-key][data-key]');
      let key=firstTag&&firstTag.getAttribute('data-field-key');
      if(!key&&group.querySelector('#intentRegionSelect'))key='region_area';
      if(key&&!domFields.includes(key))domFields.push(key);
    });
    panel.querySelectorAll('.filter-tag[data-field-key][data-key]').forEach(tag=>{
      const key=tag.getAttribute('data-field-key'),value=_intentDisplayNormalize(tag.getAttribute('data-key'));
      if(!key||!value)return;
      if(!valuesByField.has(key))valuesByField.set(key,[]);
      const values=valuesByField.get(key);if(!values.includes(value))values.push(value);
    });
  }
  const fields=_intentDisplayMergeMissingFields(domFields.length?['direction',...domFields]:[...INTENT_DISPLAY_FIELD_ORDER]);
  Object.entries(INTENT_DISPLAY_RESERVED_VALUES).forEach(([key,values])=>{
    if(!valuesByField.has(key))valuesByField.set(key,values.map(_intentDisplayNormalize));
  });
  const fieldPriority=new Map(fields.map((key,index)=>[key,index]));
  const valuePriority=new Map();
  valuesByField.forEach((values,key)=>valuePriority.set(key,new Map(values.map((value,index)=>[value,index]))));
  const ordering={fieldPriority,valuePriority};
  if(panel)_intentDisplayOrderingCache=ordering;
  return ordering;
}
function normalizeIntentConditionKey(condition){
  const key=condition&&condition.key||INTENT_DISPLAY_LABEL_KEYS[condition&&condition.label]||'';
  const value=String(condition&&(condition.normalizedValue!=null?condition.normalizedValue:condition.value)||'').toUpperCase();
  return key==='lgfv_attr'&&(value==='12重点省'||value==='非12重点省')?'key_province':key;
}
function _intentDisplayConditionKey(condition){
  return normalizeIntentConditionKey(condition);
}
function _intentDisplayTenorBounds(condition){
  const range=condition&&condition.range;
  if(range){const factor=String(range.unit||'Y').toUpperCase()==='M'?1/12:1;return[range.min==null?0:Number(range.min)*factor,range.max==null?Infinity:Number(range.max)*factor];}
  const value=_intentDisplayNormalize(condition&&(condition.normalizedValue!=null?condition.normalizedValue:condition.value));
  const factor=value.endsWith('M')?1/12:1,numbers=value.match(/\d+(?:\.\d+)?/g)||[];
  if(value.startsWith('<'))return[0,Number(numbers[0]||0)*factor];
  if(value.startsWith('>'))return[Number(numbers[0]||0)*factor,Infinity];
  return[Number(numbers[0]||0)*factor,Number(numbers[1]||numbers[0]||0)*factor];
}
function sortIntentConditionsForDisplay(conditions){
  const ordering=_intentDisplayOrdering();
  return(Array.isArray(conditions)?conditions:[]).map((condition,index)=>({condition,index})).sort((left,right)=>{
    const a=left.condition||{},b=right.condition||{},aKey=_intentDisplayConditionKey(a),bKey=_intentDisplayConditionKey(b);
    const aField=ordering.fieldPriority.has(aKey)?ordering.fieldPriority.get(aKey):Number.MAX_SAFE_INTEGER;
    const bField=ordering.fieldPriority.has(bKey)?ordering.fieldPriority.get(bKey):Number.MAX_SAFE_INTEGER;
    if(aField!==bField)return aField-bField;
    const keyOrder=_intentDisplayCompareText(aKey,bKey);if(keyOrder)return keyOrder;
    const aValue=_intentDisplayNormalize(a.normalizedValue!=null?a.normalizedValue:a.value),bValue=_intentDisplayNormalize(b.normalizedValue!=null?b.normalizedValue:b.value);
    const ranks=ordering.valuePriority.get(aKey),aRank=ranks&&ranks.has(aValue)?ranks.get(aValue):Number.MAX_SAFE_INTEGER,bRank=ranks&&ranks.has(bValue)?ranks.get(bValue):Number.MAX_SAFE_INTEGER;
    if(aRank!==bRank)return aRank-bRank;
    if(aKey==='remaining_tenor'&&aRank===Number.MAX_SAFE_INTEGER){
      const aBounds=_intentDisplayTenorBounds(a),bBounds=_intentDisplayTenorBounds(b);
      if(aBounds[0]!==bBounds[0])return aBounds[0]-bBounds[0];if(aBounds[1]!==bBounds[1])return aBounds[1]-bBounds[1];
    }
    if(aKey==='region_area'){
      const codeOrder=_intentDisplayCompareText(a.region&&a.region.code,b.region&&b.region.code);if(codeOrder)return codeOrder;
    }
    return _intentDisplayCompareText(aValue,bValue)||left.index-right.index;
  }).map(item=>item.condition);
}
function _sortMatchedConditions(conditions){return sortIntentConditionsForDisplay(conditions);}
function _smartMatchConditions(text){
  if(!text||!text.trim())return[];
  let t=text;
  // P1: 连写方向分离（bid2.5→bid 2.5，ofr2.3→ofr 2.3）
  t=t.replace(/\b(bid|ofr|offer|buy|sell|ask)(\d)/gi,'$1 $2');
  // 方向识别
  const bidKeywords=['b','买','bid','buy','bidding','收','入','求'];
  const ofrKeywords=['o','ofr','offer','sell','ask','卖','出','给','供'];
  let direction='';
  const words=t.split(/[\s,;，；、/|\\.。]+/).filter(Boolean);
  for(const w of words){
    const lw=w.toLowerCase();
    if(bidKeywords.includes(lw)){direction='Bid';break;}
    if(ofrKeywords.includes(lw)){direction='Ofr';break;}
  }
  // 停用词处理：仅删除独立的停用词，禁止单字符集合删除（保护“国有行”“有担保”“到期”“1到3Y”“3年以上”等业务词）
  t=t.replace(/(?<=^|[\s,;])的(?=[\s,;]|$)/g,' ').replace(/(?<=^|[\s,;])了(?=[\s,;]|$)/g,' ').replace(/(?<=^|[\s,;])是(?=[\s,;]|$)/g,' ').replace(/(?<=^|[\s,;])在(?=[\s,;]|$)/g,' ').replace(/(?<=^|[\s,;])和(?=[\s,;]|$)/g,' ').replace(/(?<=^|[\s,;])或(?=[\s,;]|$)/g,' ').replace(/(?<=^|[\s,;])等(?=[\s,;]|$)/g,' ').replace(/\s+/g,' ').trim();
  // 中文标点→英文
  t=t.replace(/（/g,'(').replace(/）/g,')').replace(/，/g,',').replace(/：/g,':').replace(/；/g,';').replace(/。/g,' ');
  // P1: 全角/半角统一
  t=t.replace(/[\uFF01-\uFF5E]/g,ch=>String.fromCharCode(ch.charCodeAt(0)-0xFEE0));
  // 中文数字表达→符号
  t=t.replace(/小于(\d+)/g,'<$1');
  t=t.replace(/大于(\d+)/g,'>$1');
  t=t.replace(/不到(\d+)/g,'<$1');
  t=t.replace(/超过(\d+)/g,'>$1');
  // P1: “X年以下”“X年以内”→<XY
  t=t.replace(/(\d+)\s*年以[下内]/g,'<$1Y');
  t=t.replace(/(\d+)\s*个?月以[下内]/g,'<$1M');
  // “以下”“以上”单独出现（无数字在前）
  t=t.replace(/以下(\d+)/g,'<$1');
  t=t.replace(/以上(\d+)/g,'>$1');
  // 期限区间中文→符号
  t=t.replace(/(\d+)\s*到\s*(\d+)\s*([YMym年])/g,(m,a,b,u)=>a+'-'+b+(u==='年'?'Y':u));
  t=t.replace(/(\d+)\s*至\s*(\d+)\s*([YMym年])/g,(m,a,b,u)=>a+'-'+b+(u==='年'?'Y':u));
  t=t.replace(/(\d+)\s*年以上?/g,'>$1Y');
  t=t.replace(/(\d+)\s*个月以上?/g,'>$1M');
  // 年份
  t=t.replace(/(\d{4})\s*年(?:发行|上市|成立)?/g,'$1');
  // P1: 别名规范化（小写变体→规范值）
  const aliasKeys=Object.keys(INTENT_ALIAS_MAP).sort((a,b)=>b.length-a.length);
  for(const alias of aliasKeys){
    const normalized=INTENT_ALIAS_MAP[alias];
    if(alias===normalized)continue;
    const re=new RegExp('(?<=^|[\\s,;])'+alias.replace(/[.*+?^${}()|[\]\\]/g,'\\$&')+'(?=[\\s,;]|$)','gi');
    t=t.replace(re,normalized);
  }
  // 收集所有条件，按 value 长度降序
  const allItems=[];
  availableFilterGroupData().forEach(sec=>sec.groups.forEach(fg=>fg.items.forEach(item=>{
    allItems.push({label:fg.l,value:item,sec:sec.sec});
  })));
  allItems.sort((a,b)=>b.value.length-a.value.length);
  // 贪心匹配（P1: 大小写不敏感）
  const matched=[];
  const seen=new Set();
  let remaining=t;
  const remainingUpper=()=>remaining.toUpperCase();
  allItems.forEach(({label,value,sec})=>{
    const key=label+':'+value;
    if(seen.has(key))return;
    const idx=remainingUpper().indexOf(value.toUpperCase());
    if(idx>=0){
      matched.push({label,value});
      seen.add(key);
      remaining=remaining.substring(0,idx)+' '+remaining.substring(idx+value.length);
    }
  });
  // 方向作为第一个结果插入
  if(direction){
    matched.unshift({label:'报价方向',value:direction});
  }
  // 按筛选条件类型分类统一排序
  return _sortMatchedConditions(matched);
}
function _fuzzyMatchConditions(text){
  return _smartMatchConditions(text);
}
// P1: 标签→fieldKey映射
const INTENT_LABEL_TO_FIELDKEY={
  '细分券种':'bond_subtype','热门标签':'hot_tag','是否免税':'tax_exempt','地方债类型':'local_bond_type',
  'NCD类型':'ncd_type','剩余期限':'remaining_tenor','债券上市状态':'listing_status','发行方式':'issue_method',
  '银行业':'bank_type','行业':'industry','企业性质':'enterprise_nature','担保':'guarantee',
  '主体评级':'issuer_rating','中债资信':'cb_rating','评级展望':'rating_outlook','债项评级':'bond_rating',
  '中债隐含':'cb_implied','YY评分':'yy_score','中证隐含':'cs_implied',
  '含权属性':'embedded_option','偿还方式':'repayment_method','付息方式':'coupon_freq',
  '永续债':'perpetual','次级债':'subordinated','利率方式':'rate_type','发行年份':'issue_year',
  '城投':'lgfv_attr','城投属性':'lgfv_attr','重点省份':'key_province','城投层级':'lgfv_level','地区':'region_area','地区等级':'region_grade',
  '报价方向':'direction','产品分类':'product_class','债券流动性':'liquidity','做市标的':'market_maker','上市场所':'market'
};
// P1: 别名映射（小写/变体→规范值）
const INTENT_ALIAS_MAP={
  'ncd':'NCD','cd':'NCD','存单':'NCD','同业存单':'NCD',
  'aaa':'AAA','aa+':'AA+','aa':'AA','aa-':'AA-',
  '国债':'国债','国开':'国开','国开债':'国开','政策行国开':'国开',
  '农发':'农发','农发债':'农发','口行':'口行','进出口':'口行',
  '国有银行':'国有行','国有行':'国有行',
  'shibor':'SHIBOR','depo':'DEPO','lpr':'LPR',
  'ppn':'PPN','abs':'ABS',
  'ytm':'收益率','收益率':'收益率','净价':'净价'
};
const INTENT_RECOGNITION_VERSION='intent-recognition-v2';
function _extractQuoteMeta(text){
  let inst='',trader='';
  const instEl=document.getElementById('institutionList');
  const traderEl=document.getElementById('traderList');
  if(instEl){instEl.querySelectorAll('option').forEach(o=>{if(text.includes(o.value))inst=o.value;});}
  if(traderEl){traderEl.querySelectorAll('option').forEach(o=>{if(text.includes(o.value))trader=o.value;});}
  return{inst,trader};
}
function recognizeIntentText(text){
  const conditions=_fuzzyMatchConditions(text);
  const direction=conditions.find(c=>c.label==='报价方向');
  const detailConditions=conditions.filter(c=>c.label!=='报价方向');
  const detail=detailConditions.map(c=>c.value).join(' ');
  const meta=_extractQuoteMeta(text);
  // P1: 结构化识别结果
  const structuredConditions=detailConditions.map(c=>{
    const fieldKey=INTENT_LABEL_TO_FIELDKEY[c.label]||'unknown';
    return{key:fieldKey,label:c.label,value:c.value,normalizedValue:c.value.toUpperCase(),source:'explicit',confidence:1.0};
  });
  if(direction){structuredConditions.unshift({key:'direction',label:'报价方向',value:direction.value,normalizedValue:direction.value.toUpperCase(),source:'explicit',confidence:1.0});}
  // 未识别片段
  let remaining=text||'';
  const consumed=[...conditions.map(c=>c.value),meta.inst,meta.trader].filter(Boolean).sort((a,b)=>b.length-a.length);
  consumed.forEach(value=>{remaining=remaining.replace(new RegExp(value.replace(/[.*+?^${}()|[\]\\]/g,'\\$&'),'gi'),' ');});
  remaining=remaining.replace(/\b(?:bid|buy|bidding|ofr|offer|sell|ask)\b/gi,' ').replace(/(?:买|卖|收|出|给|供|求)/g,' ');
  const unmatchedTokens=remaining.split(/[\s,;，；、/|。]+/).filter(token=>{
    const value=token.trim();
    if(value.length<=1)return false;
    if(/^[\d.]+(?:万|亿|手|张|%|bp|y|m)?$/i.test(value))return false;
    return !/^(?:报价|要|需要|帮我|一下|左右|大概|机构|交易员)$/.test(value);
  });
  const status=structuredConditions.length===0?'failed':(unmatchedTokens.length>0?'partial':'recognized');
  const recognition={
    version:INTENT_RECOGNITION_VERSION,
    status:status,
    conditions:structuredConditions,
    unmatchedTokens:unmatchedTokens,
    conflicts:[],
    result:(direction?direction.value+' ':'')+detail
  };
  return{result:recognition.result,inst:meta.inst,salesTr:meta.trader,recognition:recognition};
}
async function createBoardIntentRecord(content,institution,quoteTrader,storedRecognition){
  let recognized=storedRecognition&&storedRecognition.recognition?storedRecognition:null;
  if(!recognized){
    const rows=await requestIntentRecognitions([{clientItemId:'single',text:content,institution:institution||'',trader:quoteTrader||''}]);
    if(!rows[0]||!rows[0].ok)throw new Error(rows[0]&&rows[0].error||'模糊报价识别失败');
    recognized=rows[0];
  }
  const response=await fuzzyQuoteFetch('',{method:'POST',body:JSON.stringify({content:content,result:recognized.result,institution:institution||recognized.inst,quote_trader:quoteTrader||recognized.salesTr,status:'有效',recognition_json:JSON.stringify(recognized.recognition)})});
  const body=await response.json();if(!response.ok||!body.success)throw new Error(body.detail||'模糊报价保存失败');
  return body.intent;
}
async function createBoardIntentRecords(rows){
  const items=rows.map(row=>{const content=row.raw||'',recognized={result:row.result||'',inst:row.inst||'',salesTr:row.tr||'',recognition:row.recognition||{}};return{clientItemId:row._clientItemId,content,result:recognized.result,institution:row.inst||recognized.inst,quote_trader:row.tr||recognized.salesTr,status:'有效',recognition_json:JSON.stringify(recognized.recognition)};});
  const response=await fuzzyQuoteFetch('/bulk',{method:'POST',body:JSON.stringify({items})});
  const body=await response.json();if(!response.ok||!body.success)throw new Error(body.detail||'模糊报价批量保存失败');return body;
}
let fuzzyIntentSyncInFlight=false;
let fuzzyIntentTempId=-1;
function _refreshFuzzyIntentTables(){
  D.fuzzyData.forEach((row,index)=>{row._idx=index;});
  fuzzySelected=new Set();
  D.fuzzyData.forEach((row,index)=>{if(row.selected)fuzzySelected.add(index);});
  _renderFuzzyLeftTable();
  _renderFuzzyRightTable();
  const total=document.getElementById('fuzzyTotal');if(total)total.textContent=D.fuzzyData.length;
}
function _optimisticBoardIntent(row,clientItemId){
  const now=new Date(),time=now.getHours().toString().padStart(2,'0')+':'+now.getMinutes().toString().padStart(2,'0')+':'+now.getSeconds().toString().padStart(2,'0');
  return{id:fuzzyIntentTempId--,trader:currentUser?currentUser.name:'',content:row.raw||'',result:row.result||'',inst:row.inst||'',salesTr:row.tr||'',time,status:'同步中',recognitionJson:JSON.stringify(row.recognition||{}),updatedAt:now.toISOString(),canManage:false,_syncPending:true,_clientItemId:clientItemId};
}
function fuzzyDuplicateKey(row){
  const conditions=(row&&row.recognition&&row.recognition.conditions)||row&&row.conditions||[];
  const normalizedConditions=conditions.map(item=>[
    quoteLogicalText(item.key||item.label),
    quoteLogicalText(item.normalizedValue||item.value)
  ].join(':')).sort().join('|');
  return[
    quoteLogicalText(row&&row.inst),
    quoteLogicalText(row&&row.tr),
    normalizedConditions||quoteLogicalText(row&&row.result)||quoteLogicalText(row&&row.raw)
  ].join('|');
}
function legacyFuzzyConfirmIntent(idx){
  const r=D.fuzzyData[idx];
  if(!r)return;
  const meta=_extractQuoteMeta(r.raw);
  const dirObj=r.conditions.find(c=>c.label==='报价方向');
  const condText=r.conditions.filter(c=>c.label!=='报价方向').map(c=>c.value).join(' ');
  const now=new Date();
  const timeStr=now.getHours().toString().padStart(2,'0')+':'+now.getMinutes().toString().padStart(2,'0')+':'+now.getSeconds().toString().padStart(2,'0');
  D.boardIntent.push({
    trader:currentUser?currentUser.name:'',
    content:r.raw,
    result:(dirObj?dirObj.value+' ':'')+condText,
    inst:meta.inst,
    salesTr:meta.trader,
    time:timeStr,
    status:'有效'
  });
  D.fuzzyData.splice(idx,1);
  D.fuzzyData.forEach((r,i)=>{r._idx=i;});
  fuzzySelected=new Set(D.fuzzyData.map((_,i)=>i));
  _renderFuzzyLeftTable();
  document.getElementById('fuzzyTotal').textContent=D.fuzzyData.length;
  showToast('success','已添加到模糊报价');
}
async function fuzzyConfirmToIntent(){
  if(!authToken){showFuzzyMsg('error','应急模式为只读，认证服务恢复后才能确认报价');return;}
  if(fuzzyIntentSyncInFlight){showFuzzyMsg('error','上一批报价正在同步，请稍候');return;}
  if(fuzzySelected.size===0){showFuzzyMsg('error','请先选择报价数据');return;}
  const seenDuplicateKeys=new Set(),deferredRows=[];
  const selectedRows=Array.from(fuzzySelected).sort((a,b)=>a-b).map(index=>D.fuzzyData[index]).filter(Boolean).filter(row=>{
    const key=fuzzyDuplicateKey(row);if(seenDuplicateKeys.has(key)){deferredRows.push(row);return false;}seenDuplicateKeys.add(key);return true;
  });
  if(!selectedRows.length){showFuzzyMsg('error','请先选择报价数据');return;}
  const snapshotRows=D.fuzzyData.slice();
  const snapshotSelected=new Set(fuzzySelected);
  const snapshotActive=_activeFuzzyIdx;
  const savedRows=new Set(selectedRows);
  const batchId=Date.now().toString(36)+'-'+Math.random().toString(36).slice(2,8);
  const rowByClientId=new Map();
  selectedRows.forEach((row,index)=>{row._intentSubmitting=true;row._clientItemId='fuzzy-'+batchId+'-'+index;rowByClientId.set(row._clientItemId,row);});
  const pendingIntents=selectedRows.map(row=>_optimisticBoardIntent(row,row._clientItemId));
  fuzzyIntentSyncInFlight=true;
  const confirmButton=document.getElementById('fuzzyConfirmIntentBtn');
  if(confirmButton){confirmButton.disabled=true;confirmButton.textContent='同步中...';}
  D.fuzzyData=D.fuzzyData.filter(row=>!savedRows.has(row));
  _activeFuzzyIdx=D.fuzzyData.length?Math.min(Math.max(snapshotActive,0),D.fuzzyData.length-1):-1;
  D.boardIntent=pendingIntents.concat(D.boardIntent||[]);
  _refreshFuzzyIntentTables();
  refreshIntentView();
  showToast('success','已确认 '+selectedRows.length+' 条报价，正在后台落库'+(deferredRows.length?'；重复报价 '+deferredRows.length+' 条继续保留':''));
  try{
    const result=await createBoardIntentRecords(selectedRows);
    const createdItems=Array.isArray(result.createdItems)?result.createdItems:[];
    const failures=Array.isArray(result.failures)?result.failures:[];
    const successByClientId=new Map();
    createdItems.forEach(item=>{if(item&&rowByClientId.has(item.clientItemId)&&item.intent)successByClientId.set(item.clientItemId,item.intent);});
    // 兼容升级期间的旧服务响应；旧响应只能按请求顺序对应全部成功。
    if(!createdItems.length&&!failures.length&&Array.isArray(result.intents)&&result.intents.length===selectedRows.length){
      selectedRows.forEach((row,index)=>successByClientId.set(row._clientItemId,result.intents[index]));
    }
    const failureByClientId=new Map();
    failures.forEach(item=>{if(item&&rowByClientId.has(item.clientItemId))failureByClientId.set(item.clientItemId,item.detail||'保存失败');});
    selectedRows.forEach(row=>{if(!successByClientId.has(row._clientItemId)&&!failureByClientId.has(row._clientItemId))failureByClientId.set(row._clientItemId,'服务未返回该报价的保存结果');});
    const failedRows=new Set(selectedRows.filter(row=>failureByClientId.has(row._clientItemId)));
    const savedIntents=selectedRows.map(row=>successByClientId.get(row._clientItemId)).filter(Boolean);
    D.boardIntent=savedIntents.concat((D.boardIntent||[]).filter(item=>!item._syncPending||!rowByClientId.has(item._clientItemId)));
    selectedRows.forEach(row=>{delete row._intentSubmitting;delete row._clientItemId;});
    D.fuzzyData=snapshotRows.filter(row=>!savedRows.has(row)||failedRows.has(row));
    failedRows.forEach(row=>{row.selected=true;});
    const previousActiveRow=snapshotRows[snapshotActive];
    _activeFuzzyIdx=failedRows.size?D.fuzzyData.findIndex(row=>failedRows.has(row)):D.fuzzyData.indexOf(previousActiveRow);
    if(_activeFuzzyIdx<0)_activeFuzzyIdx=D.fuzzyData.length?0:-1;
    _refreshFuzzyIntentTables();
    if(savedIntents.length&&D.fuzzyData.length===0)clearQuoteBatchInputs('fuzzy');
    refreshIntentView();
    if(savedIntents.length){
      showToast('success','已实时同步 '+savedIntents.length+' 条到行情中心模糊报价'+(deferredRows.length?'，重复报价 '+deferredRows.length+' 条继续保留':''));
    }
    if(failedRows.size){
      const firstReason=failureByClientId.values().next().value||'保存失败';
      showFuzzyMsg('error',failedRows.size+' 条保存失败并已恢复：'+firstReason);
    }
  }catch(error){
    selectedRows.forEach(row=>{delete row._intentSubmitting;delete row._clientItemId;});
    D.boardIntent=(D.boardIntent||[]).filter(item=>!item._syncPending||!rowByClientId.has(item._clientItemId));
    D.fuzzyData=snapshotRows;
    fuzzySelected=snapshotSelected;
    _activeFuzzyIdx=snapshotActive;
    _refreshFuzzyIntentTables();
    refreshIntentView();
    showFuzzyMsg('error',(error.message||'模糊报价保存失败')+'，本批报价已恢复');
  }finally{
    fuzzyIntentSyncInFlight=false;
    if(confirmButton){confirmButton.disabled=false;confirmButton.textContent='确认报价';}
  }
}
async function _fuzzyConfirmIntent(idx){
  const row=D.fuzzyData[idx];if(!row)return;
  try{const intent=await createBoardIntentRecord(row.raw||'',row.inst||'',row.tr||'',{result:row.result||'',inst:row.inst||'',salesTr:row.tr||'',recognition:row.recognition||{}});D.boardIntent.unshift(intent);D.fuzzyData.splice(idx,1);D.fuzzyData.forEach((item,index)=>{item._idx=index;});fuzzySelected=new Set(D.fuzzyData.map((_,index)=>index));_renderFuzzyLeftTable();renderAll();document.getElementById('fuzzyTotal').textContent=D.fuzzyData.length;showToast('success','已添加到模糊报价');}
  catch(error){showToast('error',error.message||'模糊报价保存失败');}
}
let _activeFuzzyIdx=-1;
function _renderFuzzyLeftTable(){
  const wrap=document.getElementById('fuzzyLeftTableWrap');
  if(!wrap)return;
  let html='<table class="dt quote-fuzzy-table" style="width:100%;min-width:600px"><thead><tr><th style="width:28px"><input type="checkbox" '+(D.fuzzyData.length>0&&D.fuzzyData.every(r=>r.selected)?'checked':'')+' onchange="_fuzzyToggleAllLeft(this.checked)"></th><th style="text-align:left">报价内容</th><th style="text-align:left">识别结果</th><th style="text-align:center">报价机构</th><th style="text-align:center">报价交易员</th></tr></thead><tbody id="fuzzyLeftTableBody">';
  if(D.fuzzyData.length===0){
    for(let k=0;k<25;k++)html+='<tr><td colspan="5" style="border-bottom:1px solid #21262d;height:32px"></td></tr>';
  }else{
    D.fuzzyData.forEach((r,i)=>{
      const recognitionHtml=renderStructuredRecognitionTags(r.recognition||{conditions:[],unmatchedTokens:[]},{compact:true});
      const meta=_extractQuoteMeta(r.raw);
      const isActive=i===_activeFuzzyIdx;
      html+='<tr style="cursor:pointer;'+(isActive?'background:rgba(59,130,246,0.12);border-left:3px solid #3b82f6':r.selected?'background:rgba(59,130,246,0.04)':'')+'" onclick="_fuzzySelectRow('+i+')">';
      html+='<td style="text-align:center"><input type="checkbox" '+(r.selected?'checked':'')+' onclick="event.stopPropagation();_fuzzyToggleCheck('+i+')"></td>';
      html+='<td style="text-align:left;font-family:\'JetBrains Mono\',monospace;font-size:12px;color:#c9d1d9;max-width:200px;overflow:hidden;text-overflow:ellipsis;white-space:nowrap" title="'+r.raw+'">'+r.raw+'</td>';
      html+='<td style="text-align:left">'+(recognitionHtml||'<span style="color:#484f58">无匹配</span>')+'</td>';
      html+='<td class="field-underline quote-party-cell"><input class="quote-party-input" autocomplete="off" value="'+(r.inst||meta.inst||'')+'" style="color:#c9d1d9" onclick="event.stopPropagation()" onfocus="this.style.borderBottomColor=\'#79c0ff\';prepareCrmInstitutionSearch(this)" oninput="D.fuzzyData['+i+'].inst=this.value" onblur="D.fuzzyData['+i+'].inst=this.value;this.style.borderBottomColor=\'#30363d\'"></td>';
      html+='<td class="field-underline quote-party-cell"><input class="quote-party-input" autocomplete="off" value="'+(r.tr||meta.trader||'')+'" style="color:#c9d1d9" onclick="event.stopPropagation()" onfocus="this.style.borderBottomColor=\'#79c0ff\';prepareCrmTraderSearch(this,D.fuzzyData['+i+'].inst||\'\')" oninput="D.fuzzyData['+i+'].tr=this.value" onblur="D.fuzzyData['+i+'].tr=this.value;this.style.borderBottomColor=\'#30363d\'"></td>';
      html+='</tr>';
    });
    // 添加空数据行，平铺到页面底部
    const remainingRows = 25 - D.fuzzyData.length;
    if(remainingRows > 0){
      for(let k=0; k<remainingRows; k++){
        html+='<tr><td colspan="5" style="border-bottom:1px solid #21262d;height:32px"></td></tr>';
      }
    }
  }
  html+='</tbody></table>';
  wrap.innerHTML=html;
}
function _fuzzySelectRow(idx){
  _activeFuzzyIdx=idx;
  _renderFuzzyLeftTable();
  _renderFuzzyRightTable();
}
function _fuzzyToggleCheck(idx){
  D.fuzzyData[idx].selected=!D.fuzzyData[idx].selected;
  if(D.fuzzyData[idx].selected)fuzzySelected.add(idx);else fuzzySelected.delete(idx);
  _renderFuzzyLeftTable();
  // 勾选checkbox不触发右侧数据更新
}
function _fuzzyToggleAllLeft(checked){
  D.fuzzyData.forEach((r,i)=>{r.selected=checked;if(checked)fuzzySelected.add(i);else fuzzySelected.delete(i);});
  _renderFuzzyLeftTable();
  _renderFuzzyRightTable();
}
const _RIGHT_PAGE_SIZE=50;
let _rightAllData=[];
let _rightPage=1;
let _rightCache={};  // 缓存对象
let _rightConditionsChanged=0;  // 筛选条件是否修改标识：0=没有修改，1=修改了
let _rightCacheKey='';  // 当前缓存的key
let _rightRequestSeq=0;
let _rightAbortController=null;
function _cancelFuzzyRightRequest(){
  _rightRequestSeq++;
  if(_rightAbortController){_rightAbortController.abort();_rightAbortController=null;}
}
function _fuzzyRightConditionKey(row){
  if(!row||!Array.isArray(row.conditions))return'';
  return JSON.stringify(_fuzzyRecognitionFilters(row));
}
function _fuzzyRecognitionFilters(row){
  const filters={};
  if(!row||!Array.isArray(row.conditions))return filters;
  row.conditions.forEach(condition=>{
    const conditionValue=String(condition&&(condition.normalizedValue!=null?condition.normalizedValue:condition.value)||'').toUpperCase();
    const normalizedKey=condition&&condition.key==='lgfv_attr'&&(conditionValue==='12重点省'||conditionValue==='非12重点省')?'key_province':condition&&condition.key;
    const queryKey=normalizedKey==='key_province'?'key_province':condition.queryKey;
    if(condition.label==='报价方向'||!queryKey)return;
    const capability=bondFilterCapabilities.byLabel.get(condition.label);
    if(bondFilterCapabilities.byLabel.size&&(!capability||capability.enabled!==true))return;
    if(capability&&Array.isArray(capability.options)&&capability.options.length&&typeof condition.queryValue!=='object'){
      const option=capability.options.find(item=>String(item.value).toUpperCase()===String(condition.queryValue==null?condition.value:condition.queryValue).toUpperCase());
      if(option&&option.enabled===false)return;
    }
    const value=condition.queryValue===undefined?condition.value:condition.queryValue;
    if(value==null||value==='')return;
    if(!Array.isArray(filters[queryKey]))filters[queryKey]=[];
    const serialized=JSON.stringify(value);
    if(!filters[queryKey].some(item=>JSON.stringify(item)===serialized))filters[queryKey].push(value);
  });
  return filters;
}
function _renderFuzzyRightTable(){
  const wrap=document.getElementById('fuzzyRightTableWrap');
  if(!wrap)return;
  // 左侧切换、删除或重新识别时，旧请求不再拥有右侧列表的写入权。
  _cancelFuzzyRightRequest();
  let conds=[],filters={};
  if(_activeFuzzyIdx>=0&&D.fuzzyData[_activeFuzzyIdx]){
    conds=D.fuzzyData[_activeFuzzyIdx].conditions.filter(c=>c.label!=='报价方向');
    filters=_fuzzyRecognitionFilters(D.fuzzyData[_activeFuzzyIdx]);
  }
  if(conds.length===0||Object.keys(filters).length===0){
    _rightAllData=[];_rightPage=1;
    _rightCache={};_rightCacheKey='';  // 清空缓存
    let h='<table class="dt" style="width:100%;min-width:400px"><thead><tr><th style="text-align:left">剩余期限</th><th style="text-align:left">债券代码</th><th style="text-align:left">债券简称</th><th style="text-align:left">债券库</th><th style="text-align:left">中债估值</th></tr></thead><tbody id="fuzzyRightTableBody">';
    if(conds.length&&Object.keys(filters).length===0)h+='<tr><td colspan="5" style="text-align:center;padding:20px;color:#d29922">条件已识别，但当前找券接口暂不支持这些条件</td></tr>';
    for(let k=0;k<(conds.length?24:25);k++)h+='<tr><td colspan="5" style="border-bottom:1px solid #21262d;height:32px"></td></tr>';
    h+='</tbody></table>';
    wrap.innerHTML=h;
    // 更新固定的数据统计模块
    document.getElementById('fuzzyRightTotal').textContent=0;
    document.getElementById('fuzzyRightPage').textContent=0;
    document.getElementById('fuzzyRightTotalPages').textContent=0;
    document.getElementById('fuzzyRightPrevBtn').disabled=true;
    document.getElementById('fuzzyRightNextBtn').disabled=true;
    return;
  }
  // 生成缓存key
  const cacheKey=JSON.stringify(filters);
  // 检查当前行的modified标识
  const currentRow=D.fuzzyData[_activeFuzzyIdx];
  if(currentRow && currentRow.modified===0 && _rightCache[cacheKey]){
    // 筛选条件没有修改，使用缓存
    _rightAllData=_rightCache[cacheKey];
    _rightPage=1;
    _renderRightPage();
    return;
  }
  // 没有缓存，请求接口
  const rHdr='<table class="dt" style="width:100%;min-width:400px"><thead><tr><th style="text-align:left">剩余期限</th><th style="text-align:left">债券代码</th><th style="text-align:left">债券简称</th><th style="text-align:left">债券库</th><th style="text-align:left">中债估值</th></tr></thead><tbody>';
  wrap.innerHTML=rHdr+'<tr><td colspan="5" style="text-align:center;padding:20px;color:#7d8590">正在查询债券信息...</td></tr></tbody></table>';
  _rightAllData=[];_rightPage=1;
  const requestSeq=_rightRequestSeq;
  const requestedRow=currentRow;
  const controller=typeof AbortController!=='undefined'?new AbortController():null;
  _rightAbortController=controller;
  authFetch(API_BASE+'/bond-filter/query',{method:'POST',body:JSON.stringify({filters,include_records:true,record_limit:500}),...(controller?{signal:controller.signal}:{})})
    .then(r=>r.json()).then(d=>{
      if(requestSeq!==_rightRequestSeq||D.fuzzyData[_activeFuzzyIdx]!==requestedRow||_fuzzyRightConditionKey(requestedRow)!==cacheKey)return;
      if(!d.success||!d.records||d.records.length===0){wrap.innerHTML=rHdr+'<tr><td colspan="5" style="text-align:center;padding:20px;color:#484f58">未找到匹配的债券</td></tr></tbody></table>';return;}
      _rightAllData=d.records;
      _rightCache[cacheKey]=d.records;  // 存入缓存
      _rightCacheKey=cacheKey;  // 更新当前缓存key
      _renderRightPage();
    }).catch(error=>{
      if(requestSeq!==_rightRequestSeq||(error&&error.name==='AbortError'))return;
      if(D.fuzzyData[_activeFuzzyIdx]!==requestedRow||_fuzzyRightConditionKey(requestedRow)!==cacheKey)return;
      wrap.innerHTML=rHdr+'<tr><td colspan="5" style="text-align:center;padding:20px;color:#ef4444">查询失败</td></tr></tbody></table>';
    }).finally(()=>{if(requestSeq===_rightRequestSeq)_rightAbortController=null;});
}
function _renderRightPage(){
  const wrap=document.getElementById('fuzzyRightTableWrap');
  if(!wrap)return;
  const totalPages=_rightAllData.length>0?Math.ceil(_rightAllData.length/_RIGHT_PAGE_SIZE):0;
  if(_rightPage>totalPages)_rightPage=totalPages;
  const start=(_rightPage-1)*_RIGHT_PAGE_SIZE;
  const end=Math.min(start+_RIGHT_PAGE_SIZE,_rightAllData.length);
  const pageData=_rightAllData.slice(start,end);
  let h='<table class="dt" style="width:100%;min-width:400px"><thead><tr><th style="text-align:left">剩余期限</th><th style="text-align:left">债券代码</th><th style="text-align:left">债券简称</th><th style="text-align:left">债券库</th><th style="text-align:left">中债估值</th></tr></thead><tbody id="fuzzyRightTableBody">';
  pageData.forEach(r=>{
    h+='<tr><td style="text-align:left;color:#c9d1d9">'+(r.remaining_tenor||'—')+'</td><td style="text-align:left;color:#79c0ff;font-family:\'JetBrains Mono\',monospace">'+(r.security_id||'')+'</td><td style="text-align:left;color:#c9d1d9">'+(r.sec_short_name||'')+'</td><td style="text-align:left;color:#8b949e">'+(r.bond_type_desc||'')+'</td><td style="text-align:left;color:#c9d1d9;font-family:\'JetBrains Mono\',monospace">'+(r.cb_ytm||'—')+'</td></tr>';
  });
  h+='</tbody></table>';
  wrap.innerHTML=h;
  // 补空行填满剩余空间（确保至少有25行）
  var tbl=wrap.querySelector('table');
  if(tbl){
    var tb=tbl.querySelector('tbody');
    if(tb){
      var rowH=32,usedH=tb.querySelectorAll('tr').length*rowH;
      var minRows = 25;
      if(usedH < minRows * rowH){
        var extra='';
        for(var ei=0;ei<minRows-Math.floor(usedH/rowH);ei++){
          extra+='<tr><td colspan="5" style="border-bottom:1px solid #21262d;height:'+rowH+'px"></td></tr>';
        }
        tb.insertAdjacentHTML('beforeend',extra);
      }
    }
  }
  // 更新固定的数据统计模块
  document.getElementById('fuzzyRightTotal').textContent=_rightAllData.length;
  document.getElementById('fuzzyRightPage').textContent=_rightPage;
  document.getElementById('fuzzyRightTotalPages').textContent=totalPages;
  document.getElementById('fuzzyRightPrevBtn').disabled=(_rightPage<=1);
  document.getElementById('fuzzyRightNextBtn').disabled=(_rightPage>=totalPages);
}
function _fuzzyRightGoPage(p){
  const totalPages=_rightAllData.length>0?Math.ceil(_rightAllData.length/_RIGHT_PAGE_SIZE):0;
  if(totalPages===0)return;
  _rightPage=Math.max(1,Math.min(p,totalPages));
  _renderRightPage();
}

function marketTimeValue(row){
  if(!row)return 0;
  const candidates=[row.updatedAt,row.quotedAt,row.sourceSentAt,row.createdAt,row.time,row.t];
  for(const raw of candidates){
    if(!raw)continue;
    const text=String(raw),value=Date.parse(/^\d{4}-\d{2}-\d{2}/.test(text)?text.replace(' ','T'):quoteToday()+'T'+text);
    if(Number.isFinite(value))return value;
  }
  return 0;
}
function sortMarketRows(rows){return(rows||[]).slice().sort((a,b)=>marketTimeValue(b)-marketTimeValue(a)||String(b.quoteId||b.id||'').localeCompare(String(a.quoteId||a.id||'')));}
function formatBoardTime(row){const value=row&&row.time||row&&row.t||'';if(!value)return'—';const match=String(value).match(/^(?:\d{4}-\d{2}-\d{2}[ T])?(\d{2}:\d{2}:\d{2})/);return match?match[1]:value;}
function configureBoardTables(){
  const classes={boardCreditAllBody:'board-all-table',boardRateAllBody:'board-all-table',boardCreditBestBody:'board-best-table',boardRateBestBody:'board-best-table',boardAlertBody:'board-alert-table',boardIntentBody:'board-intent-table'};
  Object.entries(classes).forEach(([id,cls])=>{const body=document.getElementById(id),table=body&&body.closest('table');if(!table)return;table.classList.add('board-table',cls);});
  Object.keys(BOARD_VIEW_META).forEach(applyBoardTableLayout);
}
function boardIntentKey(row){return String(row&&row.id||'');}
function renderBoardIntentContent(_safeRow,index){
  const rowId=_safeRow&&_safeRow.id;const row=(rowId!=null)?D.boardIntent.find(r=>String(r.id)===String(rowId)):D.boardIntent[index];
  if(!row)return'<span class="muted">—</span>';
  const key=boardIntentKey(row),draft=boardIntentDrafts.get(key),editing=draft&&activeBoardIntentEditId===key;
  if(editing)return`<textarea class="board-intent-editor" data-intent-editor="${escapeQuoteHtml(key)}" onmousedown="event.stopPropagation()" ondblclick="event.stopPropagation()" oninput="setBoardIntentDraft('${escapeQuoteHtml(key)}',this.value)">${escapeQuoteHtml(draft.value)}</textarea>`;
  const changed=draft&&String(draft.value||'').trim()!==String(draft.original||'').trim();
  const content=changed?draft.value:row.content,pending=changed?'<span class="board-intent-pending">待更新</span>':'';
  const editable=row.content&&row.canManage!==false,handler=editable?` ondblclick="beginBoardIntentEditById('${escapeQuoteHtml(key)}',event)"`:'';
  return`<div class="board-intent-content" title="${escapeQuoteHtml(content||'')}"${handler}>${pending}${escapeQuoteHtml(content||'')}</div>`;
}
function beginBoardIntentEditById(id,event){
  if(event){event.preventDefault();event.stopPropagation();}const row=D.boardIntent.find(r=>String(r.id)===String(id));if(!row||!String(row.content||'').trim())return;if(row.canManage===false){showToast('error','只能更新自己的模糊报价');return;}
  const key=boardIntentKey(row);if(!boardIntentDrafts.has(key))boardIntentDrafts.set(key,{original:row.content,value:row.content,baseVersion:Number(row.dataVersion||1)});activeBoardIntentEditId=key;renderAll();requestAnimationFrame(()=>{const editor=document.querySelector('[data-intent-editor="'+CSS.escape(key)+'"]');if(editor){editor.focus();editor.select();}});
}
function beginBoardIntentEdit(index,event){
  if(event){event.preventDefault();event.stopPropagation();}const row=D.boardIntent[index];if(!row)return;beginBoardIntentEditById(String(row.id),event);
}
function setBoardIntentDraft(key,value){const draft=boardIntentDrafts.get(String(key));if(draft)draft.value=value;}
function finishBoardIntentEdit(discard){
  const key=activeBoardIntentEditId;if(key==null)return;
  const draft=boardIntentDrafts.get(key),editor=document.querySelector('[data-intent-editor="'+CSS.escape(key)+'"]'),cell=editor&&editor.closest('td');
  if(discard||(draft&&String(draft.value||'').trim()===String(draft.original||'').trim()))boardIntentDrafts.delete(key);
  activeBoardIntentEditId=null;
  const row=D.boardIntent.find(item=>boardIntentKey(item)===key);
  if(cell&&row)cell.innerHTML=renderBoardIntentContent(row,D.boardIntent.indexOf(row));else renderAll();
}
async function updateBoardIntents(){
  const drafts=[];let hasEmpty=false;boardIntentDrafts.forEach((draft,key)=>{const row=D.boardIntent.find(item=>boardIntentKey(item)===key),content=String(draft.value||'').trim(),original=String(draft.original||'').trim();if(row&&content!==original){if(!content){hasEmpty=true;return;}const meta=_extractQuoteMeta(content);drafts.push({row,content,meta});}});
  if(hasEmpty){showToast('error','报价内容不能为空，编辑内容已保留');return;}
  if(!drafts.length){boardIntentDrafts.clear();activeBoardIntentEditId=null;renderAll();showToast('info','没有需要更新的报价内容');return;}
  const button=document.getElementById('boardIntentUpdateBtn');if(button)button.disabled=true;
  try{
    const recognizedRows=await requestIntentRecognitions(drafts.map(item=>({clientItemId:String(item.row.id),text:item.content,institution:item.meta.inst,trader:item.meta.trader})));
    const byId=new Map(recognizedRows.filter(item=>item.ok).map(item=>[String(item.clientItemId),item]));
    const items=drafts.map(item=>{const recognized=byId.get(String(item.row.id)),draft=boardIntentDrafts.get(String(item.row.id));if(!recognized)throw new Error('报价 '+item.row.id+' 识别失败');return{id:item.row.id,expectedDataVersion:Number(draft&&draft.baseVersion||item.row.dataVersion||1),content:item.content,result:recognized.result,institution:recognized.inst||item.meta.inst,quote_trader:recognized.salesTr||item.meta.trader,recognition_json:JSON.stringify(recognized.recognition)};});
    const response=await fuzzyQuoteFetch('',{method:'PUT',body:JSON.stringify({items})});const body=await response.json();
    if(response.status===409&&body.detail&&body.detail.code==='BOARD_INTENT_VERSION_CONFLICT'){
      const latestRows=Array.isArray(body.detail.latest)?body.detail.latest:[],latestById=new Map(latestRows.map(row=>[String(row.id),row]));
      const conflictDetails=latestRows.map(latest=>{const draft=boardIntentDrafts.get(String(latest.id)),localText=String(draft&&draft.value||''),serverText=String(latest.content||''),updated=String(latest.updatedAt||latest.time||'—');return '<div style="margin-top:8px;padding:8px;background:#0d1117;border:1px solid #30363d;border-radius:4px"><div style="color:#8b949e;margin-bottom:5px">ID '+escapeQuoteHtml(latest.id)+' · 服务端更新 '+escapeQuoteHtml(updated)+'</div><div style="color:#d29922;white-space:pre-wrap;word-break:break-word">本地：'+escapeQuoteHtml(localText)+'</div><div style="color:#79c0ff;white-space:pre-wrap;word-break:break-word;margin-top:3px">最新：'+escapeQuoteHtml(serverText)+'</div></div>';}).join('');
      const reapply=await systemConfirmHtml('模糊报价版本冲突','<div style="color:#c9d1d9;line-height:1.7">以下模糊报价已被其他客户端更新：'+escapeQuoteHtml((body.detail.conflictIds||[]).join('、'))+'。本地草稿已保留。</div>'+conflictDetails+'<div style="margin-top:8px;color:#8b949e">重新应用将以服务端最新版本为基准再次提交；放弃将删除这些本地草稿。</div>',{confirmText:'重新应用',cancelText:'放弃本地草稿',wide:true});
      await loadBoardIntents(false);
      (body.detail.conflictIds||[]).forEach(id=>{const key=String(id),draft=boardIntentDrafts.get(key),latest=latestById.get(key);if(reapply&&draft&&latest){draft.baseVersion=Number(latest.dataVersion||1);draft.original=latest.content||'';}else if(!reapply)boardIntentDrafts.delete(key);});
      activeBoardIntentEditId=null;renderAll();showToast('info',reapply?'本地草稿已基于最新版本保留，请再次点击更新':'已放弃冲突记录的本地草稿');return;
    }
    if(!response.ok||!body.success){const detail=body.detail;throw new Error(typeof detail==='string'?detail:(detail&&detail.message)||'模糊报价更新失败');}boardIntentDrafts.clear();activeBoardIntentEditId=null;await loadBoardIntents(false);showToast('success','已更新 '+body.updated+' 条模糊报价');
  }
  catch(error){showToast('error',error.message||'模糊报价更新失败，编辑内容已保留');}finally{if(button)button.disabled=false;}
}
// 点击外部退出编辑，保留修改为“待更新”；刷新或 Escape 才放弃
document.addEventListener('pointerdown',function(e){
  if(activeBoardIntentEditId==null)return;
  const editor=document.querySelector('[data-intent-editor="'+CSS.escape(activeBoardIntentEditId)+'"]');
  const updateBtn=document.getElementById('boardIntentUpdateBtn');
  if(editor&&editor.contains(e.target))return;
  if(updateBtn&&updateBtn.contains(e.target))return;
  finishBoardIntentEdit(false);
},true);
document.addEventListener('keydown',function(e){
  if(e.key==='Escape'&&activeBoardIntentEditId!=null)finishBoardIntentEdit(true);
});
function renderAll(){
  configureBoardTables();D.boardCreditAll=sortMarketRows(D.boardCreditAll);D.boardCreditBest=sortMarketRows(D.boardCreditBest);D.boardRateAll=sortMarketRows(D.boardRateAll);D.boardRateBest=sortMarketRows(D.boardRateBest);D.boardAlert=sortMarketRows(D.boardAlert);D.boardIntent=sortMarketRows(D.boardIntent);
  _updateSidebarFavBonds();
  _updateSidebarFilterGroups();
  syncAlertSettingControls();
  renderQuoteGrouped();
  renderPreciseTable();
  renderFuzzyTable();
  // 行情中心 - 信用全部
  renderTable('boardCreditAllBody',D.boardCreditAll,boardRenderCols('board-credit-all'));
  fillEmptyRows('boardCreditAllBody');
  updateStatusbar('boardCreditAllBody','creditAllTotal','creditAllBidCnt','creditAllOfrCnt');
  renderTable('boardCreditBestBody',D.boardCreditBest,boardRenderCols('board-credit-best'));
  fillEmptyRows('boardCreditBestBody');
  updateStatusbar('boardCreditBestBody','creditBestTotal','creditBestBidCnt','creditBestOfrCnt','creditBestMatch');
  renderTable('boardRateAllBody',D.boardRateAll,boardRenderCols('board-rate-all'));
  fillEmptyRows('boardRateAllBody');
  updateStatusbar('boardRateAllBody','rateAllTotal','rateAllBidCnt','rateAllOfrCnt');
  renderTable('boardRateBestBody',D.boardRateBest,boardRenderCols('board-rate-best'));
  fillEmptyRows('boardRateBestBody');
  updateStatusbar('boardRateBestBody','rateBestTotal','rateBestBidCnt','rateBestOfrCnt','rateBestMatch');
  renderTable('boardAlertBody',D.boardAlert,boardRenderCols('board-alert'));
  fillEmptyRows('boardAlertBody');
  updateStatusbar('boardAlertBody','alertTotal','alertBidCnt','alertOfrCnt','alertMatch');
  refreshIntentView();
  bindTableEvents();
  setBoardAutoSelection();
  syncBoardSelectionClasses();
  updateBoardNewQuoteBanner();
  updateUnquotedStatusbars();
  initBoardActionToolbars();
  // 推荐机构
  renderRecommendations();
  renderMarketTradeRows();
  // 报价管理 - 今日报价
  renderTable('mgmtTodayBody',D.mgmtToday,[
    {key:'mode',render:r=>{const cls=r.mode==='QT'?'firm':r.mode==='ideal'?'nonfirm':'tag-bid';return`<span class="tag tag-${cls}" style="font-size:13px">${r.mode}</span>`;}},
    {key:'rs',render:r=>`<span style="color:${r.rs==='有效'?'#c9d1d9':r.rc};font-size:13px">${r.rs}</span>`},
    {key:'m',align:'left'},{key:'c',type:'code',align:'left'},{key:'n',align:'left'},
    {key:'direction',align:'left',render:renderQuoteDirection},
    {key:'bp',align:'left',render:r=>{const dir=quoteDirectionOf(r),value=dir==='ofr'?r.op:r.bp;return(value?renderQuotePriceSpan(r,value,dir,'','left'):'<span class="muted">—</span>')+renderInlineQuoteTags(r);}},
    {key:"bv",align:"left",render:r=>`<span style="color:#22d3ee;font-weight:600;font-family:'JetBrains Mono',monospace">${r.bv||'—'}</span>`},{key:'operations',render:renderQuoteOperationTags},{key:'inst'},{key:'trader'},{key:'gwtrader'},
    {key:'lastPrice',render:r=>`<span class="muted">${r.lastPrice||'—'}</span>`},
    {key:'bondLib'},{key:'r'},{key:'br'},
    {key:'nzNet',render:r=>`<span class="muted">${r.nzNet||'—'}</span>`},
    {key:'nzDev',render:r=>{const v=parseFloat(r.nzDev);return`<span style="color:${Math.abs(v)>16?'#ef4444':'#c9d1d9'};font-family:'JetBrains Mono',monospace">${r.nzDev||'—'}</span>`;}},
    {key:'nzYTM',render:r=>`<span class="muted">${r.nzYTM||'—'}</span>`},
    {key:'csNet',render:r=>`<span class="muted">${r.csNet||'—'}</span>`},
    {key:'csFull',render:r=>`<span class="muted">${r.csFull||'—'}</span>`},
    {key:'csYTM',render:r=>`<span class="muted">${r.csYTM||'—'}</span>`},
    {key:'t',type:'muted'},
  ],{
    rowKey:r=>r.quoteId,
    rowSelected:r=>mgmtTodaySelected.has(String(r.quoteId)),
  });
  fillEmptyRows('mgmtTodayBody');
  updateMgmtTodayStatusbar();
  updateTraderDatalist();
  // 报价管理 - 历史报价
  syncMgmtHistory();
  bindMgmtSelectionEvents();
  // 报价管理 - 成交报价
  renderTable('mgmtTradeBody',D.mgmtTrade,[
    {key:'checkbox',render:()=>' <input type="checkbox">'},
    {key:'mode',render:r=>`<span class="tag tag-${r.mode==='Firm'?'firm':'nonfirm'}">${r.mode}</span>`},
    {key:'rs',render:r=>`<span style="color:${r.rc||'#3fb950'}">${r.rs}</span>`},
    {key:'ts',render:r=>`<span style="color:${r.tc||'#3fb950'}">${r.ts}</span>`},
    {key:'m',align:'left'},{key:'c',type:'code',align:'left'},{key:'n',align:'left'},
    {key:'d',render:r=>`<span class="tag tag-${r.d.toLowerCase()}">${r.d}</span>`},
    {key:'p',type:'price'},{key:'vol'},
    {key:'pt',render:r=>`<span class="muted">${r.pt}</span>`},
    {key:'nzVal',render:r=>`<span class="muted">${r.nzVal}</span>`},
    {key:'dev',render:r=>`<span class="neg">${r.dev}</span>`},
    {key:'cs'},
    {key:'t'},{key:'tr',align:'left'},{key:'time',type:'muted'},
    {key:'tags',render:r=>r.tags&&r.tags.length?r.tags.map(t=>`<span class="tag tag-firm" style="margin-right:2px">${t}</span>`).join(''):'<span class="muted">—</span>'},
  ]);
  // 成交管理 - 待确认
  renderTable('tradePendingBody',D.tradePending,[
    {key:'checkbox',render:()=>' <input type="checkbox">'},
    {key:'date',type:'muted'},{key:'pi'},{key:'ps'},{key:'ci'},{key:'cs'},
    {key:'c',type:'code',align:'left'},{key:'n',align:'left'},
    {key:'d',render:r=>`<span class="tag tag-${r.d.toLowerCase()}">${r.d}</span>`},
    {key:'p',type:'price'},{key:'vol'},{key:'s',type:'status'},
    {key:'tags',render:r=>r.tags&&r.tags.length?r.tags.map(t=>`<span class="tag tag-firm" style="margin-right:2px">${t}</span>`).join(''):'<span class="muted">—</span>'},
    {key:'ops',render:()=>'<span style="color:#7d8590;cursor:pointer">⋯</span>'},
  ]);
  // 成交管理 - 已成交
  renderTable('tradeDoneBody',D.tradeDone,[
    {key:'date',type:'muted'},{key:'pi'},{key:'ci'},
    {key:'c',type:'code',align:'left'},{key:'n',align:'left'},
    {key:'d',render:r=>`<span class="tag tag-${r.d.toLowerCase()}">${r.d}</span>`},
    {key:'p',type:'price'},{key:'vol'},{key:'cost',type:'muted'},{key:'profit',render:r=>`<span style="color:#3fb950">+${r.profit}</span>`},
    {key:'time',type:'muted'},
    {key:'ts',render:r=>`<span class="tag tag-success">${r.ts}</span>`},
    {key:'pkg',render:r=>`<span class="muted" style="font-size:10px">${r.pkg}</span>`},
    {key:'book',render:r=>`<span class="tag tag-success">${r.book}</span>`},
  ]);
  // 成交管理 - 已取消
  renderTable('tradeCancelBody',D.tradeCancel,[
    {key:'date',type:'muted'},{key:'pi'},{key:'ci'},
    {key:'c',type:'code',align:'left'},{key:'n',align:'left'},
    {key:'d',render:r=>`<span class="tag tag-${r.d.toLowerCase()}">${r.d}</span>`},
    {key:'p',type:'price'},{key:'vol'},{key:'reason',type:'muted'},
  ]);
  // 成交管理 - 交易包管理
  renderTable('tradePackBody',D.tradePack,[
    {key:'checkbox',render:()=>' <input type="checkbox">'},
    {key:'id',render:r=>`<span style="font-family:'JetBrains Mono',monospace;font-size:11px">${r.id}</span>`},
    {key:'pi'},{key:'ci'},
    {key:'c',type:'code',align:'left'},{key:'n',align:'left'},
    {key:'d',render:r=>`<span class="tag tag-${r.d.toLowerCase()}">${r.d}</span>`},
    {key:'p',type:'price'},{key:'vol'},
    {key:'s',render:r=>`<span class="tag tag-success">${r.s}</span>`},
    {key:'ops',render:()=>'<span style="color:#7d8590;cursor:pointer">⋯</span>'},
  ]);
  // Mini chart - 债券K线图
  const mc=document.getElementById('miniChart');
  if(mc && typeof echarts!=='undefined'){
    const klineChart=echarts.init(mc);
    // 债券收益率K线数据 [开盘收益率, 收盘收益率, 最低收益率, 最高收益率]
    const dates=['06/03','06/04','06/05','06/06','06/07','06/10','06/11','06/12','06/13','06/14','06/17','06/18','06/19','06/20','06/21'];
    const klineData=[
      [2.845,2.862,2.838,2.868],
      [2.862,2.855,2.848,2.870],
      [2.855,2.871,2.850,2.878],
      [2.871,2.865,2.858,2.875],
      [2.865,2.882,2.860,2.888],
      [2.882,2.875,2.868,2.890],
      [2.875,2.890,2.870,2.895],
      [2.890,2.885,2.878,2.898],
      [2.885,2.898,2.880,2.905],
      [2.898,2.910,2.892,2.915],
      [2.910,2.905,2.898,2.918],
      [2.905,2.918,2.900,2.925],
      [2.918,2.912,2.905,2.922],
      [2.912,2.925,2.908,2.930],
      [2.925,2.920,2.915,2.928]
    ];
    // 成交量（万元）
    const volumes=[8500,12000,9500,11000,13500,15000,11500,14000,12500,16000,13000,14500,11000,15500,12000];
    // 均线数据
    const ma5=[2.859,2.865,2.871,2.869,2.875,2.878,2.884,2.888,2.890,2.898,2.904,2.911,2.915,2.918,2.920];
    const ma10=[2.862,2.865,2.868,2.869,2.873,2.876,2.880,2.884,2.887,2.892,2.897,2.903,2.908,2.913,2.916];
    
    klineChart.setOption({
      backgroundColor:'transparent',
      animation:false,
      tooltip:{
        trigger:'axis',
        axisPointer:{type:'cross'},
        backgroundColor:'rgba(22,27,34,0.95)',
        borderColor:'#30363d',
        textStyle:{color:'#e6edf3',fontSize:11},
        formatter:function(params){
          const item=params[0];
          if(!item||!item.data)return '';
          const d=item.data;
          return `<div style="font-size:11px">
            <div style="color:#7d8590;margin-bottom:4px">${item.axisValue}</div>
            <div>开盘: <span style="color:#e6edf3;font-weight:600">${d[1].toFixed(3)}%</span></div>
            <div>收盘: <span style="color:${d[2]>=d[1]?'#ef4444':'#3fb950'};font-weight:600">${d[2].toFixed(3)}%</span></div>
            <div>最低: <span style="color:#3fb950">${d[3].toFixed(3)}%</span></div>
            <div>最高: <span style="color:#ef4444">${d[4].toFixed(3)}%</span></div>
          </div>`;
        }
      },
      axisPointer:{link:{xAxisIndex:'all'}},
      grid:[{left:'10%',right:'5%',top:'1.5500*',height:'55%'},{left:'10%',right:'5%',top:'1.7200*',height:'20%'}],
      xAxis:[
        {type:'category',data:dates,gridIndex:0,axisLine:{lineStyle:{color:'#30363d'}},axisLabel:{color:'#7d8590',fontSize:9},splitLine:{show:false},axisTick:{show:false}},
        {type:'category',data:dates,gridIndex:1,axisLine:{lineStyle:{color:'#30363d'}},axisLabel:{show:false},splitLine:{show:false},axisTick:{show:false}}
      ],
      yAxis:[
        {type:'value',gridIndex:0,scale:true,axisLine:{lineStyle:{color:'#30363d'}},axisLabel:{color:'#7d8590',fontSize:9,formatter:'{value}%'},splitLine:{lineStyle:{color:'#21262d',type:'dashed'}},min:function(value){return (value.min-0.01).toFixed(3)},max:function(value){return (value.max+0.01).toFixed(3)}},
        {type:'value',gridIndex:1,axisLine:{lineStyle:{color:'#30363d'}},axisLabel:{show:false},splitLine:{show:false}}
      ],
      dataZoom:[{type:'inside',xAxisIndex:[0,1],start:0,end:100}],
      series:[
        {
          name:'K线',
          type:'candlestick',
          xAxisIndex:0,
          yAxisIndex:0,
          data:klineData,
          itemStyle:{
            color:'#ef4444',
            color0:'#3fb950',
            borderColor:'#ef4444',
            borderColor0:'#3fb950',
            borderWidth:1
          },
          barWidth:'60%'
        },
        {
          name:'MA5',
          type:'line',
          xAxisIndex:0,
          yAxisIndex:0,
          data:ma5,
          smooth:true,
          lineStyle:{color:'#d29922',width:1},
          symbol:'none'
        },
        {
          name:'MA10',
          type:'line',
          xAxisIndex:0,
          yAxisIndex:0,
          data:ma10,
          smooth:true,
          lineStyle:{color:'#79c0ff',width:1},
          symbol:'none'
        },
        {
          name:'成交量',
          type:'bar',
          xAxisIndex:1,
          yAxisIndex:1,
          data:volumes,
          itemStyle:{
            color:function(params){
              const idx=params.dataIndex;
              if(idx>0){
                return klineData[idx][1]>=klineData[idx-1][1]?'rgba(239,68,68,0.5)':'rgba(63,185,80,0.5)';
              }
              return 'rgba(121,192,255,0.3)';
            }
          },
          barWidth:'60%'
        }
      ]
    });
    window.addEventListener('resize',()=>klineChart.resize());
  }
}

const recommendationState={securityId:'240210.IB',securityName:'24国开10',productType:'利率债',triggerQuoteId:null,selectedQuoteSide:'ofr',side:'bid',task:'find_buyers',scope:'combined',window:'30d',pageSize:10,sidePages:{bid:1,ofr:1},sidePagination:{bid:null,ofr:null},results:[],response:null,sideResponses:{bid:null,ofr:null},sideResults:{bid:[],ofr:[]},sideCounts:{bid:null,ofr:null},feedbackById:{},selectedRecommendationIds:new Set(),selectionKey:'',sendBusy:false,loading:false,requestId:0,controllers:[]};
const recommendationTaskBySide={bid:'find_buyers',ofr:'find_sellers'};
const recommendationBondSearchState={items:[],activeIndex:-1,requestId:0,controller:null,timer:null,restoreSnapshot:null,selected:null,showingHistory:false};
function recommendationBondHistoryKey(){return'ficc_recBondSearchHistory:'+(currentAccountId()||'anonymous');}
function recommendationBondHistory(){
  return safeStoredArray(recommendationBondHistoryKey()).map(item=>({
    securityId:String(item&&item.securityId||''),securityName:String(item&&item.securityName||''),productType:String(item&&item.productType||'')
  })).filter(item=>item.securityId).slice(0,5);
}
function rememberRecommendationBond(item){
  if(!item||!item.securityId)return;
  const securityId=String(item.securityId),history=recommendationBondHistory().filter(entry=>entry.securityId!==securityId);
  history.unshift({securityId,securityName:String(item.securityName||securityId),productType:String(item.productType||'')});
  try{localStorage.setItem(recommendationBondHistoryKey(),JSON.stringify(history.slice(0,5)));}catch(error){console.warn('[recommendation] bond history save failed',error);}
}
function recommendationSearchSnapshot(){
  return{
    securityId:recommendationState.securityId,securityName:recommendationState.securityName,productType:recommendationState.productType,
    triggerQuoteId:recommendationState.triggerQuoteId,selectedQuoteSide:recommendationState.selectedQuoteSide,side:recommendationState.side,task:recommendationState.task,
    sidePages:{...recommendationState.sidePages},sidePagination:{...recommendationState.sidePagination},results:[...(recommendationState.results||[])],response:recommendationState.response,
    sideResponses:{...recommendationState.sideResponses},sideResults:{bid:[...(recommendationState.sideResults.bid||[])],ofr:[...(recommendationState.sideResults.ofr||[])]},sideCounts:{...recommendationState.sideCounts},
    selectedRecommendationIds:new Set(recommendationState.selectedRecommendationIds),selectionKey:recommendationState.selectionKey
  };
}
function recommendationBondSearchElements(){return{input:document.getElementById('recBondSearchInput'),clear:document.getElementById('recBondSearchClear'),panel:document.getElementById('recBondSuggestions')};}
function updateRecommendationBondSearchClear(){const elements=recommendationBondSearchElements();if(elements.clear)elements.clear.classList.toggle('show',!!String(elements.input&&elements.input.value||'').trim());}
function closeRecommendationBondSuggestions(){const panel=document.getElementById('recBondSuggestions');if(panel)panel.classList.remove('show');}
function scheduleCloseRecommendationBondSuggestions(){setTimeout(closeRecommendationBondSuggestions,100);}
function showRecommendationBondSuggestions(){const panel=document.getElementById('recBondSuggestions');if(panel&&panel.childElementCount)panel.classList.add('show');}
function showRecommendationBondHistory(){
  recommendationBondSearchState.items=recommendationBondHistory();recommendationBondSearchState.activeIndex=recommendationBondSearchState.items.length?0:-1;recommendationBondSearchState.showingHistory=true;renderRecommendationBondSuggestions();
}
function handleRecommendationBondSearchFocus(){
  const elements=recommendationBondSearchElements(),query=String(elements.input&&elements.input.value||'').trim();
  if(!query){showRecommendationBondHistory();return;}
  if(!recommendationBondSearchState.selected)showRecommendationBondSuggestions();
}
function renderRecommendationBondSuggestions(message){
  const panel=document.getElementById('recBondSuggestions');if(!panel)return;
  panel.replaceChildren();
  if(message){const empty=document.createElement('div');empty.className='rec-bond-suggestion-empty';empty.textContent=message;panel.append(empty);panel.classList.add('show');return;}
  if(recommendationBondSearchState.showingHistory&&recommendationBondSearchState.items.length){const label=document.createElement('div');label.className='rec-bond-suggestion-label';label.textContent='最近搜索';panel.append(label);}
  recommendationBondSearchState.items.forEach((item,index)=>{
    const button=document.createElement('button');button.type='button';button.className='rec-bond-suggestion'+(index===recommendationBondSearchState.activeIndex?' active':'');button.setAttribute('role','option');button.setAttribute('aria-selected',index===recommendationBondSearchState.activeIndex?'true':'false');
    const code=document.createElement('span');code.className='rec-bond-suggestion-code';code.textContent=item.securityId||'';
    const name=document.createElement('span');name.className='rec-bond-suggestion-name';name.textContent=item.securityName||'';name.title=item.securityName||'';
    button.append(code,name);button.addEventListener('mousedown',event=>event.preventDefault());button.addEventListener('mouseenter',()=>{recommendationBondSearchState.activeIndex=index;renderRecommendationBondSuggestions();});button.addEventListener('click',()=>selectRecommendationBondSuggestion(index));panel.append(button);
  });
  if(recommendationBondSearchState.items.length)panel.classList.add('show');
  else if(recommendationBondSearchState.showingHistory)panel.classList.remove('show');
  else{const empty=document.createElement('div');empty.className='rec-bond-suggestion-empty';empty.textContent='未找到匹配债券';panel.append(empty);panel.classList.add('show');}
}
async function loadRecommendationBondSuggestions(query){
  const requestId=++recommendationBondSearchState.requestId;
  recommendationBondSearchState.showingHistory=false;if(recommendationBondSearchState.controller)recommendationBondSearchState.controller.abort();
  const controller=new AbortController();recommendationBondSearchState.controller=controller;renderRecommendationBondSuggestions('正在查询…');
  try{
    const response=await authFetch(API_BASE+'/recommendations/bond-suggestions?q='+encodeURIComponent(query)+'&limit=10',{signal:controller.signal});
    let data={};try{data=await response.json();}catch(error){}
    if(!response.ok)throw new Error(recommendationErrorText(data,response.status));
    if(requestId!==recommendationBondSearchState.requestId)return;
    recommendationBondSearchState.items=Array.isArray(data.results)?data.results:[];recommendationBondSearchState.activeIndex=recommendationBondSearchState.items.length?0:-1;renderRecommendationBondSuggestions();
  }catch(error){
    if(error&&error.name==='AbortError')return;
    if(requestId===recommendationBondSearchState.requestId){recommendationBondSearchState.items=[];recommendationBondSearchState.activeIndex=-1;renderRecommendationBondSuggestions('债券关联查询失败');}
  }
}
function handleRecommendationBondSearchInput(input){
  updateRecommendationBondSearchClear();recommendationBondSearchState.selected=null;
  const query=String(input&&input.value||'').trim();clearTimeout(recommendationBondSearchState.timer);
  if(!query){clearRecommendationBondSearch(true);showRecommendationBondHistory();return;}
  recommendationBondSearchState.timer=setTimeout(()=>loadRecommendationBondSuggestions(query),200);
}
function handleRecommendationBondSearchKeydown(event){
  const items=recommendationBondSearchState.items;if(event.key==='Escape'){event.preventDefault();closeRecommendationBondSuggestions();return;}
  if(event.key==='ArrowDown'||event.key==='ArrowUp'){
    if(!items.length)return;event.preventDefault();const step=event.key==='ArrowDown'?1:-1;recommendationBondSearchState.activeIndex=(recommendationBondSearchState.activeIndex+step+items.length)%items.length;renderRecommendationBondSuggestions();return;
  }
  if(event.key==='Enter'){
    event.preventDefault();if(items.length&&recommendationBondSearchState.activeIndex>=0)selectRecommendationBondSuggestion(recommendationBondSearchState.activeIndex);
  }
}
function selectRecommendationBondSuggestion(index){
  const item=recommendationBondSearchState.items[index];if(!item)return;
  if(!recommendationBondSearchState.restoreSnapshot)recommendationBondSearchState.restoreSnapshot=recommendationSearchSnapshot();
  recommendationState.controllers.forEach(controller=>controller.abort());recommendationState.controllers=[];recommendationState.requestId++;recommendationState.loading=false;
  recommendationState.securityId=item.securityId;recommendationState.securityName=item.securityName||item.securityId;recommendationState.productType=item.productType||'';recommendationState.triggerQuoteId=null;
  recommendationState.response=null;recommendationState.results=[];recommendationState.sideResponses={bid:null,ofr:null};recommendationState.sideResults={bid:[],ofr:[]};recommendationState.sideCounts={bid:null,ofr:null};recommendationState.selectedRecommendationIds=new Set();recommendationState.selectionKey='';resetRecommendationPagination();
  rememberRecommendationBond(item);recommendationBondSearchState.selected=item;recommendationBondSearchState.items=[];recommendationBondSearchState.activeIndex=-1;recommendationBondSearchState.showingHistory=false;
  const elements=recommendationBondSearchElements();if(elements.input)elements.input.value=(item.securityId+' '+item.securityName).trim();updateRecommendationBondSearchClear();closeRecommendationBondSuggestions();
  const panel=document.getElementById('recBondSuggestions');if(panel)panel.replaceChildren();
  D.recommendationResults=[];updateRecommendationContext();renderRecommendations();loadRecommendations(true);
}
function clearRecommendationBondSearch(restore){
  clearTimeout(recommendationBondSearchState.timer);recommendationBondSearchState.requestId++;if(recommendationBondSearchState.controller)recommendationBondSearchState.controller.abort();recommendationBondSearchState.controller=null;
  recommendationBondSearchState.items=[];recommendationBondSearchState.activeIndex=-1;recommendationBondSearchState.selected=null;recommendationBondSearchState.showingHistory=false;closeRecommendationBondSuggestions();
  const panel=document.getElementById('recBondSuggestions');if(panel)panel.replaceChildren();
  const elements=recommendationBondSearchElements();if(elements.input)elements.input.value='';updateRecommendationBondSearchClear();
  const snapshot=recommendationBondSearchState.restoreSnapshot;recommendationBondSearchState.restoreSnapshot=null;
  if(!restore||!snapshot)return;
  recommendationState.controllers.forEach(controller=>controller.abort());recommendationState.controllers=[];recommendationState.requestId++;recommendationState.loading=false;
  recommendationState.securityId=snapshot.securityId;recommendationState.securityName=snapshot.securityName;recommendationState.productType=snapshot.productType;recommendationState.triggerQuoteId=snapshot.triggerQuoteId;recommendationState.selectedQuoteSide=snapshot.selectedQuoteSide;recommendationState.side=snapshot.side;recommendationState.task=snapshot.task;
  recommendationState.sidePages={...snapshot.sidePages};recommendationState.sidePagination={...snapshot.sidePagination};recommendationState.results=[...snapshot.results];recommendationState.response=snapshot.response;recommendationState.sideResponses={...snapshot.sideResponses};recommendationState.sideResults={bid:[...snapshot.sideResults.bid],ofr:[...snapshot.sideResults.ofr]};recommendationState.sideCounts={...snapshot.sideCounts};recommendationState.selectedRecommendationIds=new Set(snapshot.selectedRecommendationIds);recommendationState.selectionKey=snapshot.selectionKey;
  D.recommendationResults=recommendationState.results;updateRecommendationContext();renderRecommendations();
}
function recommendationPrice(value){const number=Number(value);return value==null||value===''||!Number.isFinite(number)?'—':number.toLocaleString('zh-CN',{minimumFractionDigits:2,maximumFractionDigits:4});}
function recommendationLatestVolume(value){const number=Number(value);return value==null||value===''||!Number.isFinite(number)?'—':number.toLocaleString('zh-CN',{maximumFractionDigits:0});}
function recommendationErrorText(payload,status){const detail=payload&&payload.detail;return(typeof detail==='string'?detail:detail&&detail.message)||(payload&&payload.message)||('推荐服务返回 HTTP '+status);}
function recommendationIsMock(value){return!!(value&&(value.is_mock||String(value.recommendation_id||'').startsWith('mock-')));}
function recommendationSelectionKey(rows){
  const page=recommendationState.sidePages[recommendationState.side]||1;
  return[recommendationState.securityId,recommendationState.scope,recommendationState.side,page,(rows||[]).map(row=>row.recommendation_id).join(',')].join('|');
}
function ensureRecommendationSelection(rows){
  const key=recommendationSelectionKey(rows);
  if(recommendationState.selectionKey===key)return;
  recommendationState.selectionKey=key;
  recommendationState.selectedRecommendationIds=new Set((rows||[]).map(row=>String(row.recommendation_id||'')).filter(Boolean));
}
function selectedRecommendationRows(){
  return(recommendationState.results||[]).filter(row=>recommendationState.selectedRecommendationIds.has(String(row.recommendation_id||'')));
}
function syncRecommendationSelectAll(){
  const box=document.getElementById('recSelectAll'),rows=recommendationState.results||[];
  if(!box)return;
  const selected=rows.filter(row=>recommendationState.selectedRecommendationIds.has(String(row.recommendation_id||''))).length;
  box.checked=rows.length>0&&selected===rows.length;
  box.indeterminate=selected>0&&selected<rows.length;
  box.disabled=recommendationState.loading||!rows.length;
}
function toggleRecommendationSelection(recommendationId,checked){
  const id=String(recommendationId||'');if(!id)return;
  if(checked)recommendationState.selectedRecommendationIds.add(id);else recommendationState.selectedRecommendationIds.delete(id);
  syncRecommendationSelectAll();updateRecommendationSendButton();
}
function toggleRecommendationSelectAll(checked){
  (recommendationState.results||[]).forEach(row=>{
    const id=String(row.recommendation_id||'');if(!id)return;
    if(checked)recommendationState.selectedRecommendationIds.add(id);else recommendationState.selectedRecommendationIds.delete(id);
  });
  document.querySelectorAll('#recInstBody .rec-select-check').forEach(box=>{box.checked=checked;});
  syncRecommendationSelectAll();updateRecommendationSendButton();
}
function updateRecommendationSendButton(){
  const button=document.getElementById('recSendQuotesBtn');if(!button)return;
  const allowed=!!currentUser&&['admin','trader'].includes(currentUser.role);
  button.style.display='inline-flex';
  button.disabled=!allowed||recommendationState.sendBusy||recommendationState.loading||!selectedRecommendationRows().length;
  button.title=allowed?'将已选报价发送给勾选的机构交易员':'当前角色没有发送报价权限';
  button.textContent=recommendationState.sendBusy?'发送中...':'发送报价';
}
function recommendationCustomerTraderText(row){
  const traders=Array.isArray(row&&row.traders)?row.traders.map(value=>String(value||'').trim()).filter(Boolean):[];
  return traders.length?traders.join('、'):'—';
}
let recommendationSendDraftText='';
let recommendationSendRetryState={signature:'',idempotencyKey:''};
function saveRecommendationSendDraft(){
  const editor=document.getElementById('recSendQuoteEditor');if(editor)recommendationSendDraftText=editor.value;
  return recommendationSendDraftText;
}
function resizeRecommendationSendEditor(editor){
  if(!editor)return;editor.style.height='auto';
  const height=Math.min(84,Math.max(34,editor.scrollHeight));editor.style.height=height+'px';editor.style.overflowY=editor.scrollHeight>84?'auto':'hidden';
}
function enterRecommendationSendEdit(editor){
  if(!editor)return;editor.readOnly=false;editor.classList.add('editing');
}
function exitRecommendationSendEdit(editor){
  saveRecommendationSendDraft();if(!editor)return;resizeRecommendationSendEditor(editor);editor.readOnly=true;editor.classList.remove('editing');
}
function acceptRecommendationSendConfirm(){
  const text=saveRecommendationSendDraft().trim();
  if(!text){showToast('error','报价内容不能为空');const editor=document.getElementById('recSendQuoteEditor');if(editor)editor.focus();return;}
  recommendationSendDraftText=text;finishSystemModal(true);
}
function recommendationSendConfirmHtml(quotePayload,rows){
  const quoteText=(quotePayload||[]).map(item=>String(item&&item.text||'').trim()).filter(Boolean).join('\n');
  recommendationSendDraftText=quoteText;
  const customerRows=(rows||[]).map((row,index)=>'<tr><td>'+(index+1)+'</td><td>'+escapeQuoteHtml(row.institution_name||'—')+'</td><td>'+escapeQuoteHtml(recommendationCustomerTraderText(row))+'</td></tr>').join('');
  return '<div class="rec-send-question">是否将报价发送给客户？</div><textarea id="recSendQuoteEditor" class="rec-send-quote-editor" aria-label="报价内容" readonly onmousedown="enterRecommendationSendEdit(this)" oninput="resizeRecommendationSendEditor(this)" onblur="exitRecommendationSendEdit(this)">'+escapeQuoteHtml(quoteText)+'</textarea>'
    +'<div class="rec-send-customer-title">客户信息</div><div class="rec-send-result"><table><thead><tr><th>序号</th><th>机构</th><th>交易员</th></tr></thead><tbody>'+customerRows+'</tbody></table></div>';
}
function confirmRecommendationQuoteSend(quotePayload,rows){
  const confirmation=systemConfirmHtml('确认发送报价',recommendationSendConfirmHtml(quotePayload,rows),{confirmText:'确定发送报价',confirmHandler:acceptRecommendationSendConfirm,wide:true});
  requestAnimationFrame(()=>resizeRecommendationSendEditor(document.getElementById('recSendQuoteEditor')));
  return confirmation;
}
function recommendationSendFailureHtml(data){
  const results=(Array.isArray(data&&data.results)?data.results:[]).filter(item=>['FAILED','UNKNOWN'].includes(item.status)||(!item.success&&!item.status));
  const successCount=Number(data&&data.successRecommendationCount||0),failedCount=Number(data&&data.failedRecommendationCount||0);
  const summary='<div style="margin-bottom:10px;color:#c9d1d9">成功 <span class="rec-send-ok">'+successCount+'</span> 个客户，失败 <span class="rec-send-fail">'+failedCount+'</span> 个客户</div>';
  if(!results.length)return summary;
  return summary+'<div class="rec-send-result"><table><thead><tr><th>机构</th><th>交易员</th><th>QT号</th><th>失败原因</th></tr></thead><tbody>'+results.map(item=>'<tr><td>'+escapeQuoteHtml(item.institutionName||'—')+'</td><td>'+escapeQuoteHtml(item.traderName||'—')+'</td><td>'+escapeQuoteHtml(item.qtNumber||'—')+'</td><td class="rec-send-fail">'+escapeQuoteHtml(item.message||item.code||'未知错误')+'</td></tr>').join('')+'</tbody></table></div>';
}
function showRecommendationSendResult(data){
  const successCount=Number(data&&data.successRecommendationCount||0),failedCount=Number(data&&data.failedRecommendationCount||0),pendingCount=Number(data&&data.pendingRecommendationCount||0);
  if(successCount>0)showToast('success','报价已发送给'+successCount+'个客户');
  if(failedCount>0)showModal('报价发送失败明细',recommendationSendFailureHtml(data),{wide:true});
  if(pendingCount>0)showToast('info','报价发送已受理，'+pendingCount+'个客户正在处理');
}
async function pollRecommendationSendBatch(batchId,attempt){
  if(!batchId||attempt>30)return;
  await new Promise(resolve=>setTimeout(resolve,1000));
  try{
    const response=await authFetch(API_BASE+'/recommendations/quote-send/'+encodeURIComponent(batchId)),data=await response.json();
    if(!response.ok)return;
    if(Number(data.pendingRecommendationCount||0)>0)return pollRecommendationSendBatch(batchId,attempt+1);
    showRecommendationSendResult(data);
  }catch(error){}
}
function recommendationSendExceptionHtml(rows,message){
  const detail=String(message||'QT发送接口调用失败');
  return '<div style="margin-bottom:10px;color:#c9d1d9">成功 <span class="rec-send-ok">0</span> 个客户，失败 <span class="rec-send-fail">'+(rows||[]).length+'</span> 个客户</div>'
    +'<div class="rec-send-result"><table><thead><tr><th>机构</th><th>交易员</th><th>失败原因</th></tr></thead><tbody>'+(rows||[]).map(row=>'<tr><td>'+escapeQuoteHtml(row.institution_name||'—')+'</td><td>'+escapeQuoteHtml(recommendationCustomerTraderText(row))+'</td><td class="rec-send-fail">'+escapeQuoteHtml(detail)+'</td></tr>').join('')+'</tbody></table></div>';
}
// 发送门禁：测试环境走QT模拟适配器，生产环境仅允许显式启用的真实发送适配器。
let _mockCapabilitiesCache=null;
async function fetchMockCapabilities(){
  if(_mockCapabilitiesCache)return _mockCapabilitiesCache;
  try{
    const r=await fetch(API_BASE+'/health');
    const d=await r.json();
    if(d&&d.clock&&/^\d{4}-\d{2}-\d{2}$/.test(d.clock.tradeDate||''))platformTradeDate=d.clock.tradeDate;
    _mockCapabilitiesCache={...((d&&d.mock_capabilities)||{}),production_send_enabled:!!(d&&d.production_send_enabled),api_contract_version:d&&d.api_contract_version||''};
  }catch(e){_mockCapabilitiesCache={};}
  return _mockCapabilitiesCache;
}
async function sendSelectedRecommendationQuotes(){
  if(recommendationState.sendBusy)return;
  if(!currentUser||!['admin','trader'].includes(currentUser.role)){showToast('error','当前角色没有发送报价权限');return;}
  const mockCaps=await fetchMockCapabilities();
  if(!mockCaps.stc_qt_mock_mode&&!mockCaps.production_send_enabled){showToast('error','报价发送能力尚未启用');return;}
  const quotes=selectedBoardQuotes(activeBoardViewKey()).filter(quote=>quote&&quote.quoteId);
  if(!quotes.length){showToast('error','请先在行情中心数据列表中勾选需要发送的报价');return;}
  const selected=selectedRecommendationRows(),realRows=selected.filter(row=>!recommendationIsMock(row));
  if(!selected.length){showToast('error','请先勾选需要发送的推荐数据');return;}
  if(!realRows.length){showToast('error','当前勾选项均为模拟推荐数据，不能发起真实QT发送');return;}
  if(mockCaps.recommendation_mock&&realRows.length<selected.length){showToast('info','已自动剔除模拟推荐数据，仅发送真实推荐');}
  recommendationState.sendBusy=true;updateRecommendationSendButton();
  try{
    const enrichMap=await fetchCopyEnrichMap(quotes);
    const quotePayload=quotes.map(quote=>({quote_id:quote.quoteId,text:quoteCopyText(quote,enrichMap[String(quote.c||'').toUpperCase()]||{})}));
    if(!await confirmRecommendationQuoteSend(quotePayload,realRows))return;
    const editedQuoteText=recommendationSendDraftText;
    const signature=JSON.stringify({recommendationIds:realRows.map(row=>row.recommendation_id),quotes:quotePayload,messageText:editedQuoteText});
    if(recommendationSendRetryState.signature!==signature)recommendationSendRetryState={signature,idempotencyKey:'rec-quote-send-'+Date.now()+'-'+Math.random().toString(16).slice(2)};
    const response=await authFetch(API_BASE+'/recommendations/quote-send',{method:'POST',body:JSON.stringify({idempotency_key:recommendationSendRetryState.idempotencyKey,recommendation_ids:realRows.map(row=>row.recommendation_id),quotes:quotePayload,message_text:editedQuoteText})});
    let data={};try{data=await response.json();}catch(error){}
    if(!response.ok)throw new Error(recommendationErrorText(data,response.status));
    recommendationSendRetryState={signature:'',idempotencyKey:''};showRecommendationSendResult(data);
    if(Number(data.pendingRecommendationCount||0)>0)pollRecommendationSendBatch(data.batchId,0);
  }catch(error){
    showModal('报价发送失败明细',recommendationSendExceptionHtml(realRows,error&&error.message),{wide:true});
  }finally{
    recommendationSendDraftText='';
    recommendationState.sendBusy=false;updateRecommendationSendButton();
  }
}
function generatedMockTraderName(index,side){
  const surnames=['陈','李','王','张','刘','周','赵','吴','徐','孙','胡','朱'];
  const givenNames=['嘉宁','思远','子涵','明轩','雨桐','浩然','欣怡','俊杰','雅雯','宇辰','诗涵','博文'];
  const offset=side==='ofr'?4:0;
  return surnames[(index+offset)%surnames.length]+givenNames[(index*3+offset)%givenNames.length];
}
function generatedMockOwnerTraderName(index,side){
  const loadedNames=accountRows.filter(account=>account.status==='active'&&account.name).map(account=>account.name);
  const names=[...new Set([currentUser&&currentUser.name,...loadedNames,'管理员','王昕','颜鑫'].filter(Boolean))];
  return names[(index+(side==='ofr'?2:0))%names.length];
}
function recommendationMockRows(side,scope,needed,page){
  if(needed<=0)return[];
  const targetId=recommendationState.securityId,targetName=recommendationState.securityName;
  const bonds=[
    ['240205.IB','24国开05'],['240215.IB','24国开15'],['240410.IB','24农发10'],['230210.IB','23国开10'],
    ['102481234.IB','24中票1234'],['102382771.IB','23城投MTN027'],['2420012.IB','24国债12'],['240303.IB','24进出03']
  ];
  const institutions=['浦发银行','中信证券','华泰证券','招商银行','国泰君安','平安银行','兴业银行','中金公司','广发证券','江苏银行','浙商银行','申万宏源'];
  const paths=scope==='combined'?['exact_bond','similar_bond','similar_institution']:scope==='exact_bond'?['exact_bond']:scope==='similar_bond'?['similar_bond']:['similar_institution'];
  const base=side==='bid'?88:86;
  return Array.from({length:needed},(_,index)=>{
    const rowIndex=(Math.max(1,page||1)-1)*recommendationState.pageSize+index;
    const path=paths[rowIndex%paths.length],bond=bonds[rowIndex%bonds.length],score=Math.max(63,base-rowIndex*2);
    const related=path==='exact_bond'?[targetId,targetName]:bond;
    return{
      recommendation_id:'mock-'+side+'-'+scope+'-'+targetId.replace(/[^a-zA-Z0-9]/g,'')+'-'+rowIndex,
      is_mock:true,quality_flags:['MOCK_DATA'],recommendation_grade:score>=85?'A':score>=72?'B':'C',recommendation_score:score,
      institution_name:institutions[(rowIndex+(side==='ofr'?3:0))%institutions.length],traders:[generatedMockTraderName(rowIndex,side)],
      evidence_confidence:rowIndex<3?'medium':'low',evidence_confidence_reason:'模拟样本，仅用于展示',
      primary_path:path,related_security:{security_id:related[0],security_name:related[1]},
      bond_similarity:path==='exact_bond'?null:{grade:rowIndex%3===0?'A':'B',score:Math.max(68,91-rowIndex*2)},
      quote_intent_count:Math.max(2,9-rowIndex),current_quote_intent_count:rowIndex%3,
      latest_quote_intent_at:new Date(Date.now()-(rowIndex+1)*86400000).toISOString(),
      latest_quote_price:(side==='bid'?2.01:2.03)+(rowIndex%5)*0.002,
      latest_quote_volume:500+(rowIndex%5)*500,
      latest_owner_trader:generatedMockOwnerTraderName(rowIndex,side),
      typical_volume_range:{min:500+(rowIndex%3)*500,max:2000+(rowIndex%4)*1000},
      evidence_direction:side==='bid'?'Bid':'Ofr',
      reason_text:path==='exact_bond'?'本券历史方向匹配':path==='similar_bond'?'相似期限与品种偏好匹配':'相似机构画像匹配',
      recommendation_summary:path==='exact_bond'?'本券同方向报价活跃，机构方向偏好较稳定。':path==='similar_bond'?'相似期限及品种存在多次报价，机构在该类债券上具有报价偏好。':'机构报价偏好与高匹配样本相似，方向表现较稳定。',
      limitations:['模拟数据，不代表真实交易意向']
    };
  });
}
function mergeRecommendationRows(realRows,side,scope,page){
  const real=(realRows||[]).map((row,index)=>({...row,is_mock:false,rank:index+1}));
  const allowMock=!!(_mockCapabilitiesCache&&_mockCapabilitiesCache.recommendation_mock);
  const supplemental=allowMock?recommendationMockRows(side,scope,Math.max(0,recommendationState.pageSize-real.length),page):[];
  return real.concat(supplemental).slice(0,recommendationState.pageSize).map((row,index)=>({...row,rank:index+1}));
}
function updateRecommendationSideControls(){
  document.querySelectorAll('.rec-toggle-btn').forEach(button=>{
    const active=button.dataset.side===recommendationState.side;
    button.classList.toggle('active-bid',active&&recommendationState.side==='bid');
    button.classList.toggle('active-ofr',active&&recommendationState.side==='ofr');
  });
  const bid=document.getElementById('recBidCount'),ofr=document.getElementById('recOfrCount');
  if(bid)bid.textContent=recommendationState.sideCounts.bid==null?'—':String(recommendationState.sideCounts.bid);
  if(ofr)ofr.textContent=recommendationState.sideCounts.ofr==null?'—':String(recommendationState.sideCounts.ofr);
}
function resetRecommendationPagination(){
  recommendationState.sidePages={bid:1,ofr:1};
  recommendationState.sidePagination={bid:null,ofr:null};
}
function renderRecommendationPagination(){
  const summary=document.getElementById('recPageSummary'),buttons=document.getElementById('recPageButtons');if(!summary||!buttons)return;
  const meta=recommendationState.sidePagination[recommendationState.side];
  if(!meta){summary.textContent=recommendationState.loading?'正在加载…':'等待加载';buttons.innerHTML='';return;}
  const total=Math.max(0,Number(meta.total_count)||0),totalPages=Math.max(1,Number(meta.total_pages)||0);
  const page=Math.min(totalPages,Math.max(1,Number(meta.page)||recommendationState.sidePages[recommendationState.side]||1));
  summary.textContent='共 '+total+' 条 · 第 '+page+'/'+totalPages+' 页';
  const pageNumbers=[];
  if(totalPages<=7){for(let value=1;value<=totalPages;value++)pageNumbers.push(value);}
  else{
    pageNumbers.push(1);
    const start=Math.max(2,page-1),end=Math.min(totalPages-1,page+1);
    if(start>2)pageNumbers.push('ellipsis-left');
    for(let value=start;value<=end;value++)pageNumbers.push(value);
    if(end<totalPages-1)pageNumbers.push('ellipsis-right');
    pageNumbers.push(totalPages);
  }
  const pageButton=(label,target,disabled,active,title)=>'<button class="rec-page-btn'+(active?' active':'')+'" type="button"'+(disabled?' disabled':'')+' title="'+title+'" onclick="changeRecommendationPage('+target+')">'+label+'</button>';
  buttons.innerHTML=pageButton('上一页',page-1,page<=1,false,'上一页')
    +pageNumbers.map(value=>typeof value==='number'?pageButton(String(value),value,false,value===page,'第'+value+'页'):'<span class="rec-page-ellipsis">…</span>').join('')
    +pageButton('下一页',page+1,page>=totalPages,false,'下一页');
}
function changeRecommendationPage(page){
  const meta=recommendationState.sidePagination[recommendationState.side],totalPages=Math.max(1,Number(meta&&meta.total_pages)||0);
  const next=Math.min(totalPages,Math.max(1,Number(page)||1));
  if(next===recommendationState.sidePages[recommendationState.side]&&recommendationState.results.length)return;
  recommendationState.sidePages[recommendationState.side]=next;loadRecommendations(true);
}
function resizeRecommendationTable(){
  const wrap=document.querySelector('#boardDetail .rec-table-wrap');if(!wrap)return;
  const top=wrap.getBoundingClientRect().top,pager=document.getElementById('recPagination');
  wrap.style.height=Math.max(220,window.innerHeight-top-(pager?pager.offsetHeight:0)-18)+'px';
}
function handleRecommendationTableKeydown(event){
  if(!event||!['ArrowLeft','ArrowRight'].includes(event.key))return;
  const target=event.target,tag=target&&target.tagName;
  if(target&&target.isContentEditable||['INPUT','TEXTAREA','SELECT','BUTTON'].includes(tag))return;
  const wrap=event.currentTarget;if(!wrap)return;
  wrap.scrollLeft+=event.key==='ArrowRight'?96:-96;
  event.preventDefault();
}
function scheduleRecommendationResize(){requestAnimationFrame(resizeRecommendationTable);}
window.addEventListener('resize',scheduleRecommendationResize);
const recommendationDetailPanel=document.getElementById('boardDetail');if(recommendationDetailPanel)recommendationDetailPanel.addEventListener('scroll',scheduleRecommendationResize,{passive:true});
function showRecommendationExplanation(){
  showModal('智能推荐逻辑','<div style="font-size:12px;line-height:1.75;color:#c9d1d9"><div><strong>数据来源：</strong>使用所有用户已确认的正式报价，并进行去重、字段完整性和数据质量过滤；QT模拟下行只有人工确认后才会进入推荐。</div><div><strong>字段权限：</strong>本人报价可显示对应机构和客户交易员，非本人报价的这两个字段留空，其他推荐字段正常展示。</div><div><strong>推荐路径：</strong>从本券直接证据出发，在满足核心风险条件时扩展至相似券报价偏好和相似机构画像。</div><div><strong>排序规则：</strong>综合本券证据、债券或机构相似度、报价次数、方向稳定性、数据质量与时间新鲜度计算推荐度。</div><div><strong>时间规则：</strong>越新的报价权重越高；自定义窗口最长365日，历史报价仅表示历史偏好，不代表当前仍可成交。</div><div><strong>反馈学习：</strong>点赞表示推荐正确，点踩及其理由用于调整后续同类推荐。</div><div><strong>使用边界：</strong>推荐度仅用于线索排序，不代表成交概率，实际询价前仍需人工确认。</div></div>');
}
let recommendationReasonTooltip=null;
function positionRecommendationTooltip(event){
  if(!recommendationReasonTooltip)return;
  const gap=12,rect=recommendationReasonTooltip.getBoundingClientRect();
  let left=event.clientX+gap,top=event.clientY+gap;
  if(left+rect.width>window.innerWidth-8)left=Math.max(8,event.clientX-rect.width-gap);
  if(top+rect.height>window.innerHeight-8)top=Math.max(8,event.clientY-rect.height-gap);
  recommendationReasonTooltip.style.left=left+'px';recommendationReasonTooltip.style.top=top+'px';
}
function showRecommendationTooltip(event,element){
  const text=element&&element.dataset.fullText;if(!text)return;
  if(!recommendationReasonTooltip){recommendationReasonTooltip=document.createElement('div');recommendationReasonTooltip.className='rec-reason-tooltip';document.body.appendChild(recommendationReasonTooltip);}
  recommendationReasonTooltip.textContent=text;recommendationReasonTooltip.style.display='block';positionRecommendationTooltip(event);
}
function hideRecommendationTooltip(){if(recommendationReasonTooltip)recommendationReasonTooltip.style.display='none';}
function recommendationFeedbackIcon(type){
  return type==='like'
    ?'<svg viewBox="0 0 24 24" aria-hidden="true"><path d="M7 10v11H3V10h4Zm0 9c3 1.4 5.6 2 8.5 2a2 2 0 0 0 1.9-1.4l2.4-7A2 2 0 0 0 17.9 10H14l.7-3.2A3.1 3.1 0 0 0 11.8 3L7 10Z"/></svg>'
    :'<svg viewBox="0 0 24 24" aria-hidden="true"><path d="M7 14V3H3v11h4Zm0-9c3-1.4 5.6-2 8.5-2a2 2 0 0 1 1.9 1.4l2.4 7a2 2 0 0 1-1.9 2.6H14l.7 3.2a3.1 3.1 0 0 1-2.9 3.8L7 14Z"/></svg>';
}
function renderRecommendations(){
  const body=document.getElementById('recInstBody');if(!body)return;
  const rows=recommendationState.results||[];
  if(recommendationState.loading){body.innerHTML='<tr><td colspan="7"><div class="rec-empty">正在计算 Bid / Ofr 推荐…</div></td></tr>';syncRecommendationSelectAll();updateRecommendationSendButton();renderRecommendationPagination();scheduleRecommendationResize();return;}
  ensureRecommendationSelection(rows);
  if(!rows.length){const reason=recommendationState.response&&recommendationState.response.empty_reason||'当前没有满足方向、权限和数据质量条件的推荐线索。';body.innerHTML='<tr><td colspan="7"><div class="rec-empty">'+escapeQuoteHtml(reason)+'</div></td></tr>';syncRecommendationSelectAll();updateRecommendationSendButton();renderRecommendationPagination();return;}
  body.innerHTML=rows.map(r=>{
    const related=r.related_security||{security_id:recommendationState.securityId,security_name:recommendationState.securityName};
    const relatedCode=related.security_id||recommendationState.securityId||'—',relatedName=related.security_name||recommendationState.securityName||'—';
    const traders=(r.traders||[]).join('、')||'机构级线索';
    const mock=recommendationIsMock(r);
    const similar=!!r.bond_similarity||r.primary_path==='similar_bond'||r.primary_path==='double_expansion';
    const price=recommendationPrice(r.latest_quote_price),volume=recommendationLatestVolume(r.latest_quote_volume);
    const ownerTrader=r.latest_owner_trader||r.owner_display_name||r.gwtrader||'—';
    const recommendationId=String(r.recommendation_id||''),checked=recommendationState.selectedRecommendationIds.has(recommendationId);
    return '<tr>'
      +'<td class="rec-select-cell"><input type="checkbox" class="rec-select-check" aria-label="选择 '+escapeQuoteHtml(r.institution_name||'推荐数据')+'" '+(checked?'checked':'')+' onchange="toggleRecommendationSelection(\''+escapeQuoteHtml(recommendationId)+'\',this.checked)" onclick="event.stopPropagation()"></td>'
      +'<td><span class="rec-score">'+escapeQuoteHtml(String(r.recommendation_score??'—'))+'</span>'+(mock?'<span class="rec-mock-badge">模拟</span>':'')+'</td>'
      +'<td class="rec-left"><strong style="color:#e6edf3">'+escapeQuoteHtml(r.institution_name||'—')+'</strong><br><span class="muted">'+escapeQuoteHtml(traders)+'</span></td>'
      +'<td class="code rec-left"><span>'+escapeQuoteHtml(relatedCode)+'</span>'+(similar?'<span class="rec-similar-badge">相似</span>':'')+'<br><span class="muted">'+escapeQuoteHtml(relatedName)+'</span></td>'
      +'<td class="s"><strong style="color:#79c0ff">'+escapeQuoteHtml(String(r.quote_intent_count||0))+'</strong></td>'
      +'<td><div class="rec-price-volume"><strong>'+escapeQuoteHtml(price)+'</strong><br><span>'+escapeQuoteHtml(volume)+'</span></div></td>'
      +'<td class="rec-owner-trader" title="'+escapeQuoteHtml(ownerTrader)+'">'+escapeQuoteHtml(ownerTrader)+'</td>'
      +'</tr>';
  }).join('');
  syncRecommendationSelectAll();updateRecommendationSendButton();
  renderRecommendationPagination();
  scheduleRecommendationResize();
}
function updateRecommendationContext(){updateRecommendationSideControls();updateRecommendationSendButton();scheduleRecommendationResize();}
function recommendationWindowPayload(){
  if(recommendationState.window!=='custom')return recommendationState.window;
  return{start:document.getElementById('recStartDate').value,end:document.getElementById('recEndDate').value};
}
async function loadRecommendations(force){
  if(!authToken){recommendationState.results=[];recommendationState.response=null;recommendationState.sideCounts={bid:null,ofr:null};resetRecommendationPagination();updateRecommendationSideControls();renderRecommendations();return false;}
  if(recommendationState.loading&&!force)return false;
  recommendationState.controllers.forEach(controller=>controller.abort());
  recommendationState.controllers=[];recommendationState.loading=true;recommendationState.requestId++;
  const requestId=recommendationState.requestId;renderRecommendations();
  try{
    const settled=await Promise.allSettled(['bid','ofr'].map(async side=>{
      const controller=new AbortController();recommendationState.controllers.push(controller);
      const response=await authFetch(API_BASE+'/recommendations/quote-leads',{method:'POST',signal:controller.signal,body:JSON.stringify({security_id:recommendationState.securityId,task:recommendationTaskBySide[side],trigger_quote_id:recommendationState.triggerQuoteId,window:recommendationWindowPayload(),result_scope:recommendationState.scope,page:recommendationState.sidePages[side],page_size:recommendationState.pageSize})});
      let data={};try{data=await response.json();}catch(error){}
      if(!response.ok)throw new Error(recommendationErrorText(data,response.status));
      return{side,data};
    }));
    if(requestId!==recommendationState.requestId)return false;
    settled.forEach((result,index)=>{
      const side=['bid','ofr'][index];
      if(result.status==='fulfilled'){
        const data=result.value.data,meta=data.pagination||{page:recommendationState.sidePages[side],page_size:recommendationState.pageSize,total_count:(data.results||[]).length,total_pages:(data.results||[]).length?1:0};
        recommendationState.sidePages[side]=Math.max(1,Number(meta.page)||1);
        recommendationState.sidePagination[side]=meta;
        const rows=mergeRecommendationRows(data.results||[],side,recommendationState.scope,recommendationState.sidePages[side]);
        recommendationState.sideResponses[side]=data;recommendationState.sideResults[side]=rows;recommendationState.sideCounts[side]=Math.max(0,Number(meta.total_count)||0);
        if(data.target){recommendationState.securityName=data.target.security_name||recommendationState.securityName;recommendationState.productType=data.target.product_type||recommendationState.productType;}
      }else{
        recommendationState.sideResponses[side]={empty_reason:result.reason&&result.reason.message||'推荐服务不可用'};
        recommendationState.sideResults[side]=[];recommendationState.sidePagination[side]=null;recommendationState.sideCounts[side]=null;
      }
    });
    recommendationState.response=recommendationState.sideResponses[recommendationState.side];
    recommendationState.results=recommendationState.sideResults[recommendationState.side]||[];
    D.recommendationResults=recommendationState.results;
    updateRecommendationContext();renderRecommendations();return true;
  }catch(error){
    if(error&&error.name==='AbortError')return false;
    if(requestId===recommendationState.requestId){recommendationState.results=[];recommendationState.response={empty_reason:error.message||'推荐服务不可用'};renderRecommendations();}
    return false;
  }finally{if(requestId===recommendationState.requestId)recommendationState.loading=false;renderRecommendations();}
}
function selectRecommendationSide(side){
  recommendationState.side=side;recommendationState.task=recommendationTaskBySide[side];
  recommendationState.response=recommendationState.sideResponses[side];recommendationState.results=recommendationState.sideResults[side]||[];
  D.recommendationResults=recommendationState.results;updateRecommendationContext();renderRecommendations();
}
function toggleRecSide(el,side){selectRecommendationSide(side);}
function setRecommendationWindow(el,value){document.querySelectorAll('.rec-window-btn').forEach(btn=>btn.classList.remove('active'));el.classList.add('active');recommendationState.window=value;resetRecommendationPagination();updateRecommendationContext();loadRecommendations(true);}
function setRecommendationCustomWindow(){const start=document.getElementById('recStartDate').value,end=document.getElementById('recEndDate').value;if(!start||!end)return;if(start>end){showToast('error','开始日期不能晚于结束日期');return;}document.querySelectorAll('.rec-window-btn').forEach(btn=>btn.classList.remove('active'));recommendationState.window='custom';resetRecommendationPagination();updateRecommendationContext();loadRecommendations(true);}
function initializeRecommendationDates(){const end=quoteToday(),start=isoDateAddDays(end,-30);const startEl=document.getElementById('recStartDate'),endEl=document.getElementById('recEndDate');if(startEl&&!startEl.value)startEl.value=start;if(endEl&&!endEl.value)endEl.value=end;updateRecommendationContext();}
function inferSelectedQuoteSide(row,cell,tbodyId){
  if(cell&&cell.classList.contains('bid-col'))return'bid';
  if(cell&&cell.classList.contains('ofr-col'))return'ofr';
  const colId=boardCellColId(cell);
  if(['instBid','trBid','bvBid','bp'].includes(colId))return'bid';
  if(['instOfr','trOfr','bvOfr','op'].includes(colId))return'ofr';
  const index=cell?cell.cellIndex:-1;
  if(tbodyId==='boardCreditBestBody'||tbodyId==='boardRateBestBody'){if(index>=3&&index<=6)return'bid';if(index>=7&&index<=10)return'ofr';}
  if(tbodyId==='boardAlertBody'){if(index>=4&&index<=7)return'bid';if(index>=8&&index<=11)return'ofr';}
  const direction=String(row.direction||'').toLowerCase();
  if(direction.includes('ofr')||direction.includes('offer')||direction.includes('卖'))return'ofr';
  if(direction.includes('bid')||direction.includes('买'))return'bid';
  if(row.bp&&!row.op)return'bid';if(row.op&&!row.bp)return'ofr';
  return recommendationState.selectedQuoteSide||'ofr';
}
function setRecommendationTarget(row,cell,tbodyId){
  if(!row||!row.c)return;
  clearRecommendationBondSearch(false);
  const nextTrigger=row.quoteId||row.sourceQuoteId||null,quoteSide=inferSelectedQuoteSide(row,cell,tbodyId),nextSide=quoteSide==='bid'?'ofr':'bid';
  const targetChanged=recommendationState.securityId!==row.c||recommendationState.triggerQuoteId!==nextTrigger;
  recommendationState.securityId=row.c;recommendationState.securityName=row.n||row.c;recommendationState.triggerQuoteId=nextTrigger;recommendationState.selectedQuoteSide=quoteSide;
  recommendationState.productType=(row.market==='rate'||String(row.bondType||'').includes('政策性')||String(row.n||'').includes('国债'))?'利率债':'信用债';
  const code=document.querySelector('#boardDetail .detail-code'),name=document.querySelector('#boardDetail .detail-name');if(code)code.textContent=row.c;if(name)name.textContent=row.n||row.c;
  if(targetChanged){recommendationState.sideResponses={bid:null,ofr:null};recommendationState.sideResults={bid:[],ofr:[]};recommendationState.sideCounts={bid:null,ofr:null};resetRecommendationPagination();}
  selectRecommendationSide(nextSide);
  if(targetChanged)loadRecommendations(true);
}
function showRecommendationDislike(recommendationId){
  if(String(recommendationId||'').startsWith('mock-')){showToast('info','模拟数据不记录反馈');return;}
  if(recommendationState.feedbackById[recommendationId]){showToast('info','该推荐结果已反馈');return;}
  showModal('推荐结果不准确','<div style="font-size:12px;color:#c9d1d9;line-height:1.6"><label for="recDislikeReason" style="display:block;margin-bottom:6px">点踩理由 <span style="color:#7d8590">（选填，最多120字）</span></label><textarea id="recDislikeReason" class="tb-input" maxlength="120" placeholder="例如：方向不匹配、机构偏好不准确、债券相似度不足等" style="width:100%;height:96px;padding:8px;resize:vertical;line-height:1.5"></textarea><div style="margin-top:6px;color:#7d8590">理由将用于改进后续智能推荐结果。</div></div>',{initialFocus:'#recDislikeReason',footerHtml:'<button class="tb-btn tb-btn-default" type="button" onclick="closeModal()">关闭</button><button class="tb-btn tb-btn-primary" type="button" onclick="submitRecommendationDislike(\''+escapeQuoteHtml(recommendationId)+'\')">提交</button>'});
}
function submitRecommendationDislike(recommendationId){const input=document.getElementById('recDislikeReason');submitRecommendationFeedback(recommendationId,'dislike',input?input.value:'');}
async function submitRecommendationFeedback(recommendationId,action,reason){
  if(String(recommendationId||'').startsWith('mock-')){showToast('info','模拟数据不记录反馈');return;}
  if(recommendationState.feedbackById[recommendationId]){showToast('info','该推荐结果已反馈');return;}
  try{
    const response=await authFetch(API_BASE+'/recommendations/'+encodeURIComponent(recommendationId)+'/feedback',{method:'POST',body:JSON.stringify({action,reason:(reason||'').trim()||null})});
    const data=await response.json();if(!response.ok)throw new Error(recommendationErrorText(data,response.status));
    recommendationState.feedbackById[recommendationId]=action;renderRecommendations();
    if(action==='dislike')closeModal();
    showToast('success',action==='like'?'已记录：推荐正确':'点踩反馈已提交');
  }catch(error){showToast('error',error.message||'反馈保存失败');}
}
function copyRecommendationInquiry(recommendationId){const item=(recommendationState.results||[]).find(row=>row.recommendation_id===recommendationId);if(!item)return;if(recommendationIsMock(item)){showToast('info','模拟数据不能生成询价任务');return;}const text='待确认询价任务\n债券：'+recommendationState.securityId+' '+recommendationState.securityName+'\n推荐方向：'+(recommendationState.side==='bid'?'Bid':'Ofr')+'\n机构：'+item.institution_name+'\n证据：'+item.reason_text+'\n请发送前重新确认方向、价格、数量和结算。';if(navigator.clipboard&&navigator.clipboard.writeText)navigator.clipboard.writeText(text).then(()=>showToast('success','待确认询价任务已复制')).catch(()=>showModal('待确认询价任务','<pre style="white-space:pre-wrap">'+escapeQuoteHtml(text)+'</pre>'));else showModal('待确认询价任务','<pre style="white-space:pre-wrap">'+escapeQuoteHtml(text)+'</pre>');}
function toggleDirection(el, dir){
  const key=boardViewKeyForElement(el),state=boardToolbarState[key];if(!state||state.unquoted)return;
  state.direction=state.direction===dir?'':dir;
  clearBoardSelection();renderFilteredViews();syncBoardToolbarControls();
}

function toggleSidebar(){const s=document.getElementById('boardSidebar');if(s){s.classList.toggle('collapsed');const toggle=document.getElementById('sidebarToggle');if(toggle)toggle.innerHTML=s.classList.contains('collapsed')?'<svg viewBox="0 0 24 24" width="14" height="14" stroke="currentColor" stroke-width="2" fill="none"><polyline points="9 18 15 12 9 6"/></svg>':'<svg viewBox="0 0 24 24" width="14" height="14" stroke="currentColor" stroke-width="2" fill="none"><polyline points="15 18 9 12 15 6"/></svg>';}}
function toggleMgmtSidebar(){const s=document.getElementById('mgmtSidebar');if(s){s.classList.toggle('collapsed');const btn=s.querySelector('.collapse-toggle-sidebar');if(btn)btn.textContent=s.classList.contains('collapsed')?'展开':'收起';}}
function toggleDetail(){const d=document.getElementById('boardDetail');if(d){d.classList.toggle('collapsed');const toggle=document.getElementById('detailToggle');if(toggle)toggle.innerHTML=d.classList.contains('collapsed')?'<svg viewBox="0 0 24 24" width="14" height="14" stroke="currentColor" stroke-width="2" fill="none"><polyline points="15 18 9 12 15 6"/></svg>':'<svg viewBox="0 0 24 24" width="14" height="14" stroke="currentColor" stroke-width="2" fill="none"><polyline points="9 18 15 12 9 6"/></svg>';scheduleRecommendationResize();}}

function initBoardSubtabs(){
  const boardPage=document.getElementById('page-board');
  if(!boardPage)return;
  const subtabsEl=boardPage.querySelector('.subtabs');
  if(subtabsEl){
    subtabsEl.querySelectorAll('.subtab').forEach(st=>{
      st.onclick=function(){switchSubTab(this)};
    });
  }
}

let _quoteEntryState=null;
function isQuoteEntryModalOpen(){
  const overlay=document.getElementById('quoteEntryOverlay');
  return !!(overlay&&overlay.classList.contains('show'));
}
function initQuoteSubtabs(){
  const page=document.getElementById('page-quote');
  if(!page)return;
  const tabs=page.querySelectorAll('#subtabs-quote .subtab');
  const savedKey=activeSubTabs[page.id]||'quote-confirm';
  let targetIdx=0;
  tabs.forEach((tab,i)=>{if(tab.dataset.st===savedKey)targetIdx=i;});
  tabs.forEach((tab,i)=>{
    tab.classList.toggle('active',i===targetIdx);
    tab.onclick=function(){switchSubTab(this)};
  });
  page.querySelectorAll('.subpage').forEach((subpage,i)=>{
    subpage.style.display=i===targetIdx?'flex':'none';
    subpage.classList.toggle('hidden',i!==targetIdx);
  });
}
function openQuoteEntryModal(){
  if(isQuoteEntryModalOpen())return;
  const page=document.getElementById('page-quote');
  const mount=document.getElementById('quoteEntryMount');
  const overlay=document.getElementById('quoteEntryOverlay');
  if(!page||!mount||!overlay)return;
  const placeholder=document.createComment('page-quote-home');
  page.parentNode.insertBefore(placeholder,page);
  _quoteEntryState={placeholder:placeholder,parent:placeholder.parentNode,style:page.getAttribute('style')};
  mount.appendChild(page);
  initQuoteSubtabs();
  renderQuoteGrouped();
  renderPreciseTable();
  renderFuzzyTable();
  overlay.classList.add('show');
  overlay.setAttribute('aria-hidden','false');
  requestAnimationFrame(function(){
    const closeBtn=overlay.querySelector('.quote-entry-header .modal-close');
    if(closeBtn)closeBtn.focus();
    fillEmptyRows('preciseTableBody');
    fillEmptyRows('fuzzyLeftTableBody');
    fillEmptyRows('fuzzyRightTableBody');
  });
}
function closeQuoteEntryModal(){
  if(!isQuoteEntryModalOpen())return;
  stopPreciseResize();
  const page=document.getElementById('page-quote');
  const overlay=document.getElementById('quoteEntryOverlay');
  if(page&&_quoteEntryState&&_quoteEntryState.parent&&_quoteEntryState.placeholder){
    _quoteEntryState.parent.insertBefore(page,_quoteEntryState.placeholder);
    _quoteEntryState.placeholder.remove();
    if(_quoteEntryState.style===null)page.removeAttribute('style');
    else page.setAttribute('style',_quoteEntryState.style);
  }
  _quoteEntryState=null;
  if(overlay){overlay.classList.remove('show');overlay.setAttribute('aria-hidden','true');}
  const trigger=document.getElementById('boardQuoteEntryBtn');
  if(trigger)trigger.focus();
}

function switchRecTab(el){
  const tabs=el.parentElement;
  tabs.querySelectorAll('.rec-tab').forEach(t=>t.classList.remove('active'));
  el.classList.add('active');
  recommendationState.scope=el.dataset.scope||'combined';
  resetRecommendationPagination();
  loadRecommendations(true);
}

const INST_SOURCE_COLORS={guoli:'#1e40af',guoji:'#ef4444',pingan:'#f97316',zhongcheng:'#22c55e',shangtian:'#06b6d4',xintang:'#9333ea',cfets:'#a16207'};
const INST_SOURCE_NAMES={guoli:'国利',guoji:'国际',pingan:'平安',zhongcheng:'中诚',shangtian:'上田',xintang:'信唐',cfets:'CFETS'};
const INST_QUOTE_MOCK=[
  {bidSource:'pingan',bidVol:5000,bid:2.9150,ofr:2.9220,ofrVol:3000,ofrSource:'pingan'},
  {bidSource:'guoji',bidVol:3000,bid:2.9160,ofr:2.9210,ofrVol:2000,ofrSource:'guoji'},
  {bidSource:'zhongcheng',bidVol:2000,bid:2.9170,ofr:2.9200,ofrVol:1500,ofrSource:'zhongcheng'},
  {bidSource:'shangtian',bidVol:4000,bid:2.9180,ofr:2.9230,ofrVol:2500,ofrSource:'shangtian'},
  {bidSource:'guoli',bidVol:1500,bid:2.9190,ofr:2.9240,ofrVol:1000,ofrSource:'guoli'},
  {bidSource:'xintang',bidVol:2500,bid:2.9200,ofr:2.9250,ofrVol:1800,ofrSource:'xintang'},
  {bidSource:'cfets',bidVol:8000,bid:2.9155,ofr:2.9195,ofrVol:6000,ofrSource:'cfets'},
  {bidSource:'pingan',bidVol:2800,bid:2.9145,ofr:2.9225,ofrVol:1500,ofrSource:'shangtian'},
  {bidSource:'guoji',bidVol:1200,bid:2.9158,ofr:2.9208,ofrVol:900,ofrSource:'xintang'},
  {bidSource:'zhongcheng',bidVol:3200,bid:2.9172,ofr:2.9198,ofrVol:2000,ofrSource:'guoji'},
  {bidSource:'guoli',bidVol:900,bid:2.9188,ofr:2.9242,ofrVol:600,ofrSource:'cfets'},
  {bidSource:'xintang',bidVol:1800,bid:2.9165,ofr:2.9235,ofrVol:1200,ofrSource:'guoli'}
];
const MARKET_TRADE_MOCK=[
  {direction:'TRD',c:'240210.IB',n:'24国开10',price:'2.9200',volume:'2000',time:'10:32:15'},
  {direction:'TKN',c:'250205.IB',n:'25国开05',price:'1.8425',volume:'1500',time:'10:31:48'},
  {direction:'GVN',c:'2400004.IB',n:'24特别国债04',price:'1.9650',volume:'3000',time:'10:30:26'},
  {direction:'TRD',c:'250004.IB',n:'25附息国债04',price:'1.7125',volume:'1000',time:'10:29:57'},
  {direction:'TKN',c:'102580247.IB',n:'25浙交投MTN001',price:'2.5400',volume:'2000',time:'10:28:43'},
  {direction:'GVN',c:'210210.IB',n:'21国开10',price:'1.9000',volume:'1000',time:'10:27:19'},
  {direction:'TRD',c:'102480625.IB',n:'24晋能山西MTN002',price:'2.2576',volume:'500',time:'10:26:52'},
  {direction:'TKN',c:'252980.SH',n:'23大理债',price:'3.8000',volume:'1000',time:'10:25:34'},
  {direction:'GVN',c:'255258.SH',n:'24康居01',price:'1.9200',volume:'10000',time:'10:24:11'},
  {direction:'TRD',c:'012582704.IB',n:'25南京铁建SCP004',price:'1.5800',volume:'1000',time:'10:22:46'}
];
function instBadgeHtml(key){
  const name=INST_SOURCE_NAMES[key]||key,color=INST_SOURCE_COLORS[key]||'#6b7280';
  return '<span class="inst-badge" style="background:'+color+'">'+name+'</span>';
}
function renderInstQuoteRows(sourceKey){
  const tbody=document.getElementById('instQuoteBody');
  if(!tbody)return;
  if(!(_mockCapabilitiesCache&&_mockCapabilitiesCache.market_tape_demo)){
    tbody.innerHTML='<tr><td colspan="6" style="text-align:center;color:#8b949e;padding:20px">暂无多档行情</td></tr>';
    const emptyList=tbody.closest('.inst-quote-list');if(emptyList)emptyList.scrollTop=0;
    return;
  }
  let html='',bestBidIdx=-1,bestOfrIdx=-1,bestBidVal=Infinity,bestOfrVal=-Infinity;
  const visible=[];
  INST_QUOTE_MOCK.forEach(function(row,idx){
    const bidHit=(sourceKey==='all'||row.bidSource===sourceKey);
    const ofrHit=(sourceKey==='all'||row.ofrSource===sourceKey);
    if(!bidHit&&!ofrHit)return;
    visible.push({row:row,bidHit:bidHit,ofrHit:ofrHit});
  });
  if(visible.length>10)visible.length=10;
  if(!visible.length){
    tbody.innerHTML='<tr><td colspan="6" style="text-align:center;color:#8b949e;padding:20px">暂无该来源行情</td></tr>';
    const list=tbody.closest('.inst-quote-list');if(list)list.scrollTop=0;
    return;
  }
  // 找最优
  visible.forEach(function(v,vi){
    if(v.bidHit&&v.row.bid<bestBidVal){bestBidVal=v.row.bid;bestBidIdx=vi;}
    if(v.ofrHit&&v.row.ofr>bestOfrVal){bestOfrVal=v.row.ofr;bestOfrIdx=vi;}
  });
  visible.forEach(function(v,vi){
    const bg=(vi===bestBidIdx)?'background:rgba(245,158,11,0.08);':(vi===bestOfrIdx)?'background:rgba(59,130,246,0.08);':'';
    html+='<tr style="'+bg+'">';
    html+='<td>'+(v.bidHit?instBadgeHtml(v.row.bidSource):'<span style="color:#484f58">--</span>')+'</td>';
    html+='<td style="text-align:center">'+(v.bidHit?v.row.bidVol:'--')+'</td>';
    html+='<td style="text-align:center;color:#f59e0b;font-weight:600" class="bid-col">'+(v.bidHit?v.row.bid.toFixed(4):'--')+'</td>';
    html+='<td style="text-align:center;color:#3b82f6;font-weight:600" class="ofr-col">'+(v.ofrHit?v.row.ofr.toFixed(4):'--')+'</td>';
    html+='<td style="text-align:center">'+(v.ofrHit?v.row.ofrVol:'--')+'</td>';
    html+='<td class="text-right">'+(v.ofrHit?instBadgeHtml(v.row.ofrSource):'<span style="color:#484f58">--</span>')+'</td>';
    html+='</tr>';
  });
  tbody.innerHTML=html;
  const list=tbody.closest('.inst-quote-list');if(list)list.scrollTop=0;
}
function renderMarketTradeRows(){
  const tbody=document.getElementById('marketTradeBody');if(!tbody)return;
  const rows=(_mockCapabilitiesCache&&_mockCapabilitiesCache.market_tape_demo?MARKET_TRADE_MOCK:[]).slice().sort((a,b)=>String(b.time||'').localeCompare(String(a.time||''))).slice(0,10);
  if(!rows.length){tbody.innerHTML='<tr><td colspan="6" style="text-align:center;color:#8b949e">暂无成交行情</td></tr>';const list=tbody.closest('.market-trade-list');if(list)list.scrollTop=0;return;}
  tbody.innerHTML=rows.map(row=>{
    const direction=['TRD','TKN','GVN'].includes(String(row.direction||'').toUpperCase())?String(row.direction).toUpperCase():'TRD';
    return '<tr>'
      +'<td><span class="market-trade-dir '+direction.toLowerCase()+'">'+direction+'</span></td>'
      +'<td title="'+escapeQuoteHtml(row.c||'')+'">'+escapeQuoteHtml(row.c||'--')+'</td>'
      +'<td title="'+escapeQuoteHtml(row.n||'')+'">'+escapeQuoteHtml(row.n||'--')+'</td>'
      +'<td class="market-trade-price">'+escapeQuoteHtml(row.price||'--')+'</td>'
      +'<td class="market-trade-volume">'+escapeQuoteHtml(boardVolumeDisplay(row.volume))+'</td>'
      +'<td class="market-trade-time">'+escapeQuoteHtml(String(row.time||'--').slice(0,5))+'</td>'
      +'</tr>';
  }).join('');
  const list=tbody.closest('.market-trade-list');if(list)list.scrollTop=0;
}
function filterInst(el,type){
  el.parentElement.querySelectorAll('.inst-tag').forEach(t=>t.classList.remove('active'));
  el.classList.add('active');
  renderInstQuoteRows(type);
}

function highlightBestBid(){
  renderInstQuoteRows(document.querySelector('.inst-tag.active')?document.querySelector('.inst-tag.active').getAttribute('onclick').match(/'([^']+)'/)[1]:'all');
}

function highlightBestOfr(){
  highlightBestBid();
}

// 筛选模块拖拽排序功能
function initDragSort(){
  const sidebar=document.getElementById('boardSidebar');
  if(!sidebar)return;
  const filterGroups=sidebar.querySelectorAll('.filter-section .filter-group');
  let draggedItem=null;
  
  // 加载保存的排序
  const savedOrder=localStorage.getItem('filterOrder');
  if(savedOrder){
    try{
      const order=JSON.parse(savedOrder);
      const section=sidebar.querySelector('.filter-section');
      if(section){
        order.forEach(id=>{
          const el=section.querySelector(`[data-filter-id="${id}"]`);
          if(el)section.appendChild(el);
        });
      }
    }catch(e){}
  }
  
  // 为每个filter-group添加data-filter-id
  filterGroups.forEach((group,index)=>{
    if(!group.getAttribute('data-filter-id')){
      group.setAttribute('data-filter-id','filter-'+index);
    }
    
    group.setAttribute('draggable','true');
    
    group.addEventListener('dragstart',function(e){
      draggedItem=this;
      this.classList.add('dragging');
      e.dataTransfer.effectAllowed='move';
    });
    
    group.addEventListener('dragend',function(){
      this.classList.remove('dragging');
      document.querySelectorAll('.filter-group').forEach(g=>g.classList.remove('drag-over'));
      draggedItem=null;
      saveFilterOrder();
    });
    
    group.addEventListener('dragover',function(e){
      e.preventDefault();
      e.dataTransfer.dropEffect='move';
      if(this!==draggedItem){
        this.classList.add('drag-over');
      }
    });
    
    group.addEventListener('dragleave',function(){
      this.classList.remove('drag-over');
    });
    
    group.addEventListener('drop',function(e){
      e.preventDefault();
      this.classList.remove('drag-over');
      if(draggedItem&&this!==draggedItem){
        const parent=this.parentNode;
        const allItems=[...parent.querySelectorAll('.filter-group')];
        const draggedIndex=allItems.indexOf(draggedItem);
        const targetIndex=allItems.indexOf(this);
        if(draggedIndex<targetIndex){
          parent.insertBefore(draggedItem,this.nextSibling);
        }else{
          parent.insertBefore(draggedItem,this);
        }
      }
    });
  });
}

function saveFilterOrder(){
  const sidebar=document.getElementById('boardSidebar');
  if(!sidebar)return;
  const section=sidebar.querySelector('.filter-section');
  if(!section)return;
  const groups=section.querySelectorAll('.filter-group');
  const order=[];
  groups.forEach(g=>{
    const id=g.getAttribute('data-filter-id');
    if(id)order.push(id);
  });
  localStorage.setItem('filterOrder',JSON.stringify(order));
}

// 页面加载时初始化拖拽排序
document.addEventListener('DOMContentLoaded',async function(){
  upgradeClientSelects();
  setTimeout(initDragSort,500);
  initBoardActionToolbars();
  initFilterInteraction();
  const bondFilterInitTask=initBondFilterPanels();
  renderInstQuoteRows('all');
  renderMarketTradeRows();
  await populateRegionSelectors();
  await bondFilterInitTask;
  initInputValidation();
});

// 筛选项交互功能
function initFilterInteraction(){
  document.querySelectorAll('.filter-tag[data-group]').forEach(tag=>{
    tag.addEventListener('click',function(){
      const group=this.getAttribute('data-group');
      const sub=this.getAttribute('data-sub');
      const isAll=this.textContent.trim()==='全部';
      const section=this.closest('.filter-section')||this.closest('.filter-group');
      if(!section)return;
      
      // 含权属性特殊处理
      if(group==='option'){
        const optionTags=section.querySelectorAll('.filter-tag[data-group="option"]');
        const hasCall=section.querySelector('.filter-tag[data-sub="call"].active');
        const hasPut=section.querySelector('.filter-tag[data-sub="put"].active');
        const hasOther=section.querySelector('.filter-tag[data-sub="other"].active');
        const hasOption=section.querySelector('.filter-tag[data-group="option"]:not([data-sub]).active');
        
        if(sub){
          // 点击括号内的选项
          this.classList.toggle('active');
          // 如果点击了任一含权类型，自动选中"含权"
          if(this.classList.contains('active')&&!hasOption){
            const optionMain=section.querySelector('.filter-tag[data-group="option"]:not([data-sub])');
            if(optionMain)optionMain.classList.add('active');
          }
          // 如果所有含权类型都取消选中，取消"含权"
          const anySubActive=section.querySelector('.filter-tag[data-sub].active');
          if(!anySubActive&&hasOption){
            const optionMain=section.querySelector('.filter-tag[data-group="option"]:not([data-sub])');
            if(optionMain)optionMain.classList.remove('active');
          }
        }else{
          // 点击"含权"
          this.classList.toggle('active');
          // 如果选中含权，不自动选中子项
          // 如果取消含权，取消所有子项
          if(!this.classList.contains('active')){
            optionTags.forEach(t=>{if(t.getAttribute('data-sub'))t.classList.remove('active');});
          }
        }
        return;
      }
      
      if(isAll){
        // 点击全部：取消同组所有选中，选中全部
        section.querySelectorAll(`.filter-tag[data-group="${group}"]`).forEach(t=>t.classList.remove('active'));
        this.classList.add('active');
      }else{
        // 点击其他：取消全部的选中状态，切换当前项
        const allTag=section.querySelector(`.filter-tag[data-group="${group}"]`);
        if(allTag)allTag.classList.remove('active');
        this.classList.toggle('active');
        
        // 检查是否所有项都未选中，如果是则选中全部
        const anyActive=section.querySelectorAll(`.filter-tag[data-group="${group}"]`).length>0;
        const hasActive=section.querySelector(`.filter-tag[data-group="${group}"].active`);
        if(!hasActive&&allTag){
          allTag.classList.add('active');
        }
      }
    });
  });
  
  // 非分组的filter-tag点击交互
  document.querySelectorAll('.filter-section .filter-tag:not([data-group]):not([data-field-key])').forEach(tag=>{
    if(tag.closest('#filters-intent'))return;
    tag.addEventListener('click',function(){
      const parent=this.closest('.filter-group');
      if(!parent)return;
      const tags=parent.querySelectorAll('.filter-tag');
      const isAll=this.textContent.trim()==='全部';
      
      if(isAll){
        tags.forEach(t=>t.classList.remove('active'));
        this.classList.add('active');
      }else{
        const allTag=parent.querySelector('.filter-tag');
        if(allTag&&allTag.textContent.trim()==='全部'){
          allTag.classList.remove('active');
        }
        this.classList.toggle('active');
        
        const hasActive=parent.querySelector('.filter-tag.active');
        if(!hasActive&&allTag&&allTag.textContent.trim()==='全部'){
          allTag.classList.add('active');
        }
      }
    });
  });
}

// 输入框验证
function initInputValidation(){
  document.querySelectorAll('.filter-input[data-rule]').forEach(input=>{
    input.addEventListener('input',function(e){
      const rule=this.getAttribute('data-rule');
      let value=this.value;
      
      const container=this.closest('.filter-grid-item,.filter-group');
      const label=container&&container.querySelector('.filter-label');
      const allowNegative=['Bid-中债','中债-Ofr','Bid-Ofr'].includes(String(label&&label.textContent||'').trim());
      const negative=allowNegative&&value.trim().startsWith('-');
      value=value.replace(/[^0-9.]/g,'');
      if(negative)value='-'+value;
      
      // 防止多个小数点
      const unsigned=value.startsWith('-')?value.slice(1):value;
      const parts=unsigned.split('.');
      if(parts.length>2){
        value=(negative?'-':'')+parts[0]+'.'+parts.slice(1).join('');
      }
      
      if(rule==='price3_4'){
        // 3位整数+4位小数
        if(parts[0].length>3){
          value=(negative?'-':'')+parts[0].substring(0,3)+(parts.length>1?'.'+parts[1]:'');
        }
        if(parts.length>1&&parts[1].length>4){
          value=(negative?'-':'')+parts[0]+'.'+parts[1].substring(0,4);
        }
      }else if(rule==='price6_2'){
        // 6位整数+2位小数
        if(parts[0].length>6){
          value=(negative?'-':'')+parts[0].substring(0,6)+(parts.length>1?'.'+parts[1]:'');
        }
        if(parts.length>1&&parts[1].length>2){
          value=(negative?'-':'')+parts[0]+'.'+parts[1].substring(0,2);
        }
      }
      
      this.value=value;
    });
  });
}

function clearTermInput(){
  // 历史 inline onclick 保留兼容；实际查询由初始化时注册的按钮事件执行。
}

function switchPage(page,el){
  if(!canAccessPage(page)){showToast('error','当前账号无权访问该页面');return;}
  document.querySelectorAll('.page').forEach(p=>{p.classList.remove('active-page');p.style.display='none';});
  const t=document.getElementById('page-'+page);
  if(t){t.style.display='flex';t.classList.add('active-page');}
  document.querySelectorAll('.nav-btn').forEach(n=>n.classList.remove('active'));
  if(el)el.classList.add('active');
  if(page==='mgmt'){
    loadMgmtToday();
    requestAnimationFrame(function(){fillEmptyRows('mgmtTodayBody');fillEmptyRows('mgmtHistoryBody');});
  }
  syncBidOfrAvailability();
  const subtabsEl=t?t.querySelector('.subtabs'):null;
  if(subtabsEl){
    const savedKey=activeSubTabs[t.id]||'';
    const tabs=subtabsEl.querySelectorAll('.subtab');
    let targetIdx=0;
    if(savedKey){
      tabs.forEach((st,i)=>{if(st.dataset.st===savedKey)targetIdx=i;});
    }
    tabs.forEach((st,i)=>{
      st.classList.toggle('active',i===targetIdx);
      st.onclick=function(){switchSubTab(this)};
    });
    const subpages=t?t.querySelectorAll('.subpage'):[];
    subpages.forEach((sp,i)=>{sp.style.display=i===targetIdx?'flex':'none';sp.classList.toggle('hidden',i!==targetIdx);});
  }
  if(page==='board'){
    setTimeout(()=>{
      fillEmptyRows('boardCreditAllBody');
      fillEmptyRows('boardCreditBestBody');
      fillEmptyRows('boardRateAllBody');
      fillEmptyRows('boardRateBestBody');
      fillEmptyRows('boardAlertBody');
    },50);
  }
  if(page==='version')loadGitLog();
  if(page==='accounts')loadAccounts();
  if(page==='requirements'&&typeof openRequirementsPage==='function')openRequirementsPage();
  if(page==='requirement-spec'&&typeof openRequirementSpecPage==='function')openRequirementSpecPage();
}
function restoreHashDeepLink(){
  const hash=window.location&&window.location.hash||'';
  if(typeof window.isRequirementSpecDeepLink!=='function'||!window.isRequirementSpecDeepLink(hash))return false;
  if(!currentUser)return false;
  if(!canAccessPage('requirement-spec')){showToast('error','当前账号无权访问需求规格页面');return false;}
  const nav=document.querySelector('.nav-btn[data-page="requirement-spec"]');
  switchPage('requirement-spec',nav);
  return true;
}
window.addEventListener('hashchange',function(){if(currentUser)restoreHashDeepLink();});
(function(){
  var ticking=false;
  window.addEventListener('resize',function(){
    if(ticking)return;ticking=true;
    requestAnimationFrame(function(){
      ticking=false;
      var mgmt=document.getElementById('page-mgmt');
      if(mgmt&&mgmt.classList.contains('active-page')){
        fillEmptyRows('mgmtTodayBody');
        fillEmptyRows('mgmtHistoryBody');
      }
    });
  });
})();
function mgmtDefaultHistoryDates(){
  const today=new Date(),format=date=>date.getFullYear()+'-'+String(date.getMonth()+1).padStart(2,'0')+'-'+String(date.getDate()).padStart(2,'0'),d=new Date(today),dow=d.getDay();
  if(dow===0)d.setDate(d.getDate()-2);else if(dow===1)d.setDate(d.getDate()-3);else d.setDate(d.getDate()-1);
  return{today:format(today),previousBusinessDay:format(d)};
}
function resetMgmtHistoryDates(){
  const defaults=mgmtDefaultHistoryDates(),startEl=document.getElementById('historyStartDate'),endEl=document.getElementById('historyEndDate');
  if(startEl){startEl.min='';startEl.max=defaults.today;startEl.value=defaults.previousBusinessDay;}
  if(endEl){endEl.min=defaults.previousBusinessDay;endEl.max=defaults.today;endEl.value=defaults.previousBusinessDay;}
}
(function(){
  resetMgmtHistoryDates();
  var startEl=document.getElementById('historyStartDate');
  var endEl=document.getElementById('historyEndDate');
  if(startEl&&endEl){
    startEl.addEventListener('change',function(){if(startEl.value)endEl.min=startEl.value;filterMgmtHistory();});
    endEl.addEventListener('change',function(){if(endEl.value)startEl.max=endEl.value;filterMgmtHistory();});
  }
})();

function switchSubTab(el){
  const tabs=el.parentElement;
  tabs.querySelectorAll('.subtab').forEach(t=>t.classList.remove('active'));
  el.classList.add('active');
  const page=el.closest('.page');
  if(!page)return;
  const subpages=page.querySelectorAll('.subpage');
  const idx=Array.from(tabs.querySelectorAll('.subtab')).indexOf(el);
  subpages.forEach((sp,i)=>{
    if(i===idx){sp.style.display='flex';sp.classList.remove('hidden');}
    else{sp.style.display='none';sp.classList.add('hidden');}
  });
  if(page.id==='page-board'){const activeSubpage=subpages[idx];requestAnimationFrame(()=>{const wrap=activeSubpage&&activeSubpage.querySelector('.tbl-wrap');if(wrap)wrap.scrollLeft=0;});}
  activeSubTabs[page.id]=el.dataset.st||String(idx);
  try{localStorage.setItem('ficc_activeSubTabs',JSON.stringify(activeSubTabs));}catch(e){}
  const st=el.getAttribute('data-st');
  syncBidOfrAvailability();
  if(BOARD_VIEW_META[st]){clearBoardSelection();syncBoardToolbarControls();}
  if(BOARD_VIEW_META[st]){boardSelectionMode='none';boardPendingNewQuoteCount=0;boardFrozenProjection=null;boardAutoSelectSuppressed=false;updateBoardNewQuoteBanner();}
  const filterMap={
    'board-credit-all':'filters-credit',
    'board-credit-best':'filters-credit',
    'board-rate-all':'filters-rate',
    'board-rate-best':'filters-rate',
    'board-intent':'filters-intent',
    'board-alert':'filters-merged'
  };
  const filterId=filterMap[st];
  if(filterId){
    ['filters-credit','filters-rate','filters-merged','filters-intent'].forEach(id=>{
      const el=document.getElementById(id);
      if(el)el.style.display=id===filterId?'':'none';
    });
  }
  if(st==='board-alert'){
    renderAll();
    if(suppressNextAlertPageRead)suppressNextAlertPageRead=false;else markAlertsViewed();
  }
  if(st==='board-intent'){refreshIntentView();loadIntentStats();}
  setTimeout(()=>{
    const tbMap={'board-credit-all':'boardCreditAllBody','board-credit-best':'boardCreditBestBody','board-rate-all':'boardRateAllBody','board-rate-best':'boardRateBestBody','board-alert':'boardAlertBody','board-intent':'boardIntentBody','mgmt-today':'mgmtTodayBody','mgmt-history':'mgmtHistoryBody'};
    if(tbMap[st])fillEmptyRows(tbMap[st]);
    if(st==='quote-precise')fillEmptyRows('preciseTableBody');
    if(st==='quote-fuzzy'){
      fillEmptyRows('fuzzyLeftTableBody');
      fillEmptyRows('fuzzyRightTableBody');
    }
  },50);
}

function initLoginBg(){
  const sc=document.getElementById('loginStars');if(!sc)return;sc.innerHTML='';
  for(let i=0;i<120;i++){const s=document.createElement('div');s.className='star';s.style.left=Math.random()*100+'%';s.style.top=Math.random()*100+'%';s.style.animationDuration=(2+Math.random()*4)+'s';s.style.animationDelay=Math.random()*5+'s';const sz=(0.5+Math.random()*2);s.style.width=sz+'px';s.style.height=sz+'px';sc.appendChild(s);}
  const pc=document.getElementById('loginParticles');if(!pc)return;pc.innerHTML='';
  for(let i=0;i<15;i++){const p=document.createElement('div');p.className='login-particle';p.style.left=Math.random()*100+'%';p.style.animationDuration=(8+Math.random()*12)+'s';p.style.animationDelay=Math.random()*10+'s';pc.appendChild(p);}
}
initLoginBg();

function resetAccountState(){
  authSessionGeneration++;
  clearRecommendationBondSearch(false);
  stopQuoteConfirmPolling();quoteConfirmEventSequence=0;
  closeQuoteEntryModal();
  Object.keys(boardToolbarState).forEach(resetBoardToolbarState);clearBoardSelection();boardSelectionMode='none';boardPendingNewQuoteCount=0;boardFrozenProjection=null;boardAutoSelectSuppressed=false;updateBoardNewQuoteBanner();setUnquotedQuoteControls('credit',false);setUnquotedQuoteControls('rate',false);
  Object.values(boardUnquotedState).forEach(state=>Object.assign(state,{rows:[],total:0,page:1,loading:false,error:'',requestId:state.requestId+1}));
  Object.keys(BOND_FILTER_SCOPES).forEach(scope=>resetBondFilterPanel(scope,false));
  bondFilterCache.clear();
  D.quoteConfirm=[];D.quoteConfirmBatches=[];D.quoteConfirmStats={batchCount:0,quoteCount:0,qtBatchCount:0,idealBatchCount:0,bidCount:0,ofrCount:0,errorCount:0};D.liveQuotes=[];D.quoteEvents=[];D.boardCreditAll=[];D.boardCreditBest=[];D.boardRateAll=[];D.boardRateBest=[];D.boardAlert=[];D.boardIntent=[];D.preciseData=[];D.fuzzyData=[];D.mgmtToday=[];D.mgmtTodayAll=[];D.mgmtHistory=[];D.mgmtHistoryFiltered=[];D.quoteLogs={};D.quotePersistQueues={};D.quoteStateLoaded=false;
  boardIntentDrafts.clear();activeBoardIntentEditId=null;boardRefreshBusy=false;pendingQuoteDrafts.clear();
  quoteStateRequestSequence++;appliedQuoteStateRevision=0;quoteRevisionRefreshPending=false;
  mgmtTodayRequestSequence++;appliedMgmtTodayRevision=0;
  quoteSelected.clear();preciseSelected.clear();fuzzySelected.clear();mgmtTodaySelected.clear();mgmtHistorySelected.clear();intentSelectedIds.clear();accountRows=[];alertViewedCount=0;readAlertKeys=new Set();alertReadLoaded=false;platformTradeDate='';
  resetMgmtToolbarFilters(false);
  activeSubTabs={};try{localStorage.removeItem('ficc_activeSubTabs');}catch(e){}
  marketGroupQueryGeneration++;marketGroupMatchIds.clear();marketGroupQueryErrors.clear();
  activeMarketGroupIds.favorite.clear();activeMarketGroupIds.filter.clear();
  D.favBonds=[];D.favBondGroupsCredit=[];D.favBondGroupsRate=[];D.filterGroupsCredit=[];D.filterGroupsRate=[];
  D.marketGroupRevision=0;D.marketGroupLoaded=false;D.marketGroupReadOnly=false;
  D.alertSettings={favorite:true,filterGroup:true,priceMatch:true,bestQuote:true};
  recommendationState.results=[];recommendationState.response=null;recommendationState.sideResponses={bid:null,ofr:null};recommendationState.sideResults={bid:[],ofr:[]};recommendationState.sideCounts={bid:null,ofr:null};resetRecommendationPagination();recommendationState.feedbackById={};recommendationState.selectedRecommendationIds=new Set();recommendationState.selectionKey='';recommendationState.sendBusy=false;recommendationState.loading=false;recommendationState.triggerQuoteId=null;D.recommendationResults=[];
  _mockCapabilitiesCache=null;
  recommendationState.controllers.forEach(controller=>controller.abort());recommendationState.controllers=[];
  quotePriceConversionCache.clear();hideQuotePriceTooltip();
}
function normalizeSessionUser(user){
  const role=['admin','technician','trader'].includes(user.role)?user.role:'trader';
  const roleName={admin:'系统管理员',technician:'技术员',trader:'销售交易员'}[role];
  return{...user,accountId:user.accountId||user.username,username:user.username,name:user.name||user.displayName||user.username,role,roleName:user.roleName||roleName,dept:user.dept||user.department||'',orgId:user.orgId||'',email:user.email||'',phone:user.phone||''};
}
function applyRolePermissions(){
  document.querySelectorAll('[data-page-roles]').forEach(el=>{el.style.display=canAccessPage(el.dataset.page)?'flex':'none';});
}
function enterApplication(user){
  currentUser=normalizeSessionUser(user);
  document.getElementById('loginOverlay').classList.add('hidden');document.getElementById('app').style.display='flex';
  document.getElementById('userName').textContent=currentUser.name;document.getElementById('userAvatar').textContent=currentUser.name.charAt(0);applyRolePermissions();
  if(window.desktopAPI&&window.desktopAPI.toggleFullscreen)window.desktopAPI.toggleFullscreen();
  document.querySelectorAll('.page').forEach(p=>{p.style.display='none';p.classList.remove('active-page');});
  const boardPage=document.getElementById('page-board');boardPage.style.display='flex';boardPage.classList.add('active-page');
  document.querySelectorAll('.nav-btn').forEach(n=>n.classList.remove('active'));document.querySelector('.nav-btn[data-page=board]').classList.add('active');
  renderAll();updateBadges();initializeRecommendationDates();initIntentDateInputs();restoreHashDeepLink();setTimeout(()=>{initBoardSubtabs();},300);
}
let quoteStateRequestSequence=0,appliedQuoteStateRevision=0,quoteRevisionRefreshPending=false;
function mergeCanonicalQuote(quote){
  if(!quote||!quote.quoteId)return null;
  const normalized=window.FICCQuoteCanonical?window.FICCQuoteCanonical.normalizeQuoteRow(quote,quote.sourceType||'state'):quote;
  const idx=(D.liveQuotes||[]).findIndex(item=>item.quoteId===normalized.quoteId);
  const merged={...(idx>=0?D.liveQuotes[idx]:{}),...normalized,quoteId:normalized.quoteId,sourceQuoteId:normalized.quoteId};
  if(idx>=0)D.liveQuotes[idx]=merged;else D.liveQuotes.push(merged);
  return merged;
}
function loadQuoteState(options){
  if(!currentUser||!authToken)return Promise.resolve(false);
  const previousAlertKeys=allAlertKeys(),notify=!!(options&&options.notify);
  const previousPriceMatchKeys=new Set((D.boardAlert||[]).filter(a=>a.alertType==='对价提醒').map(a=>a.alertId||a.pairKey||'').filter(Boolean));
  const generation=authSessionGeneration,requestSequence=++quoteStateRequestSequence;
  return authFetch(API_BASE+'/quotes/state?view=board&scope=all').then(r=>r.ok?r.json():Promise.reject(new Error('HTTP '+r.status))).then(d=>{
    if(!d.success||generation!==authSessionGeneration)return false;
    if(requestSequence!==quoteStateRequestSequence)return true;
    const revision=Number(d.revision||0);
    if(appliedQuoteStateRevision&&(!revision||revision<appliedQuoteStateRevision))return true;
    appliedQuoteStateRevision=Math.max(appliedQuoteStateRevision,revision);
    const manualFreeze=boardSelectionMode==='manual'&&!options.forceBoardRefresh;
    const previousQuoteIds=new Set((D.liveQuotes||[]).map(q=>String(q.quoteId)));
    const frozen=manualFreeze?(boardFrozenProjection||captureBoardProjection()):null;
    D.liveQuotes=sortMarketRows((d.quotes||[]).map(q=>({...q,sourceQuoteId:q.quoteId})));
    D.quoteEvents=(d.events||[]).map(event=>({...event,eventId:event.eventId||makeQuoteId('evt'),snapshot:event.snapshot||{}}));
    D.quoteStateLoaded=true;rebuildBoardProjections();
    if(manualFreeze){
      boardFrozenProjection=frozen;
      const incoming=(d.quotes||[]).filter(q=>q&&q.quoteId&&!previousQuoteIds.has(String(q.quoteId))&&q.status==='active');
      if(incoming.length){boardPendingNewQuoteCount+=incoming.length;}
      restoreBoardProjection(frozen);
    }else if(options&&options.forceBoardRefresh){boardFrozenProjection=null;boardPendingNewQuoteCount=0;}
    syncMgmtHistory();renderAll();updateBadges();loadMgmtToday();if(notify){notifyNewAlerts(previousAlertKeys);notifyNewPriceMatchAlerts(previousPriceMatchKeys);}
    const highlightIds=options&&options.highlightQuoteIds||[];if(highlightIds.length)revealAndHighlightSavedQuotes(highlightIds,options.reveal!==false);
    if(!manualFreeze)loadRecommendations(true);return true;
  }).catch(()=>{if(generation===authSessionGeneration&&requestSequence===quoteStateRequestSequence)D.quoteStateLoaded=false;return false;});
}
function getIntentDateParams(){
  const s=document.getElementById('intentStartDate'),e=document.getElementById('intentEndDate');
  const today=new Date().toISOString().slice(0,10);
  const sv=s&&s.value?s.value:today, ev=e&&e.value?e.value:today;
  return {start_date:sv,end_date:ev};
}
function onIntentDateChange(){loadBoardIntents(false);loadIntentStats();}
function initIntentDateInputs(){
  const today=new Date().toISOString().slice(0,10);
  const s=document.getElementById('intentStartDate'),e=document.getElementById('intentEndDate');
  if(s){s.value=today;s.max=today;}
  if(e){e.value=today;e.max=today;}
}
function loadBoardIntents(discardDrafts){
  if(!currentUser||!authToken)return Promise.resolve(false);if(discardDrafts){boardIntentDrafts.clear();activeBoardIntentEditId=null;}const generation=authSessionGeneration;
  const dp=getIntentDateParams();
  const qs='?start_date='+encodeURIComponent(dp.start_date)+'&end_date='+encodeURIComponent(dp.end_date);
  return fuzzyQuoteFetch(qs).then(async response=>{const body=await response.json();if(!response.ok)throw new Error(body.detail||'HTTP '+response.status);return body;}).then(body=>{if(!body.success||generation!==authSessionGeneration)return false;D.boardIntent=sortMarketRows(body.intents||[]);renderAll();return true;}).catch(()=>false);
}
async function refreshBoardData(){
  if(boardRefreshBusy)return;boardRefreshBusy=true;boardIntentDrafts.clear();activeBoardIntentEditId=null;const button=document.getElementById('boardRefreshBtn');if(button)button.disabled=true;
  try{const quoteTask=boardSelectionMode==='manual'?refreshFrozenBoardProjection():loadQuoteState();const results=await Promise.all([quoteTask,loadBoardIntents(true)]);if(results.every(Boolean))showToast('success','行情数据已刷新');else showToast('error','部分行情数据刷新失败，请重试');}finally{boardRefreshBusy=false;if(button)button.disabled=false;}
}
async function doLogin(){
  const u=document.getElementById('loginUser').value.trim(),p=document.getElementById('loginPass').value,err=document.getElementById('loginError'),btn=document.getElementById('loginBtn');
  err.textContent='';document.getElementById('loginUser').classList.remove('error');document.getElementById('loginPass').classList.remove('error');
  if(!u||!p){err.textContent='请输入用户名和密码';return;}
  btn.disabled=true;btn.textContent='登录中...';resetAccountState();authToken='';currentUser=null;
  try{
    await ensureApplicationServices();
    const response=await fetch(API_BASE+'/auth/login',{method:'POST',headers:{'Content-Type':'application/json'},body:JSON.stringify({username:u,password:p})});
    let data={};try{data=await response.json();}catch(e){}
    if(!response.ok){const detail=data&&data.detail;err.textContent=(typeof detail==='string'?detail:detail&&detail.message)||'用户名或密码错误';document.getElementById('loginUser').classList.add('error');document.getElementById('loginPass').classList.add('error');return;}
    authToken=data.token;
    // 登录提速：分组状态与筛选配置互不依赖，认证成功后并行加载（原为串行两次往返）
    const groupTask=loadMarketGroupState().catch(groupError=>{D.marketGroupLoaded=false;D.marketGroupReadOnly=true;showToast('error','分组加载失败，当前为只读状态');});
    const bondFilterTask=(bondFilterCapabilities.error||!bondFilterCapabilities.contractVersion)?initBondFilterPanels().catch(()=>false):Promise.resolve(true);
    const optionMapTask=loadBondOptionMap();
    const regionTask=populateRegionSelectors();
    const capabilityTask=fetchMockCapabilities().then(()=>{renderInstQuoteRows('all');renderMarketTradeRows();}).catch(()=>false);
    await Promise.all([groupTask,bondFilterTask,optionMapTask,regionTask,capabilityTask]);
    enterApplication(data.user);
    if(D.marketGroupLoaded)refreshMarketGroupMatches(true).catch(()=>false);
    await loadAlertReadState();
    await Promise.all([loadQuoteState(),loadBoardIntents(true),loadQuoteConfirmBatches(true)]);
    if(data.user&&data.user.mustChangePassword)setTimeout(()=>showChangePasswordModal(!!data.user.forceInitialPasswordChange),150);
    setTimeout(maybeOfferLegacyMarketGroupImport,0);
    startQuoteConfirmPolling();
    connectQuoteSSE();
  }catch(networkError){
    err.textContent='认证服务不可用，请检查后端服务';
  }finally{
    btn.disabled=false;btn.textContent='登 录';
    if(currentUser&&document.getElementById('rememberMe').checked){try{localStorage.setItem('ficc_remember_user',u);localStorage.removeItem('ficc_remember_pass');}catch(e){}}
    else if(!document.getElementById('rememberMe').checked){try{localStorage.removeItem('ficc_remember_user');localStorage.removeItem('ficc_remember_pass');}catch(e){}}
  }
}
function doLogout(sessionExpired){
  disconnectQuoteSSE();
  const token=authToken;if(token&&!sessionExpired)fetch(API_BASE+'/auth/logout',{method:'POST',headers:{'Authorization':'Bearer '+token}}).catch(()=>false);
  resetAccountState();authToken='';currentUser=null;document.getElementById('headerUser').classList.remove('open');document.getElementById('app').style.display='none';document.getElementById('loginOverlay').classList.remove('hidden');
  document.getElementById('loginPass').value='';document.getElementById('loginError').textContent=sessionExpired?'登录状态已失效，请重新登录':'';initLoginBg();document.getElementById('loginUser').focus();
}

// ===== SSE 实时报价推送 =====
let quoteEventSource=null,quotePollTimer=null,quoteSseConnectionId=makeQuoteId('sse'),quoteSseGeneration=0;
async function connectQuoteSSE(){
  if(quoteEventSource){quoteEventSource.close();quoteEventSource=null;}
  if(quotePollTimer){clearInterval(quotePollTimer);quotePollTimer=null;}
  const generation=++quoteSseGeneration;
  if(!authToken)return false;
  try{
    const ticketResponse=await authFetch(API_BASE+'/quotes/events/ticket',{method:'POST',body:JSON.stringify({connectionId:quoteSseConnectionId})});
    const ticketBody=await ticketResponse.json().catch(()=>({}));
    if(!ticketResponse.ok||!ticketBody.ticket)throw new Error('SSE ticket unavailable');
    if(generation!==quoteSseGeneration||!authToken)return false;
    quoteEventSource=new EventSource(API_BASE+'/quotes/events?ticket='+encodeURIComponent(ticketBody.ticket)+'&connection_id='+encodeURIComponent(quoteSseConnectionId));
    quoteEventSource.addEventListener('new_quote',handleRemoteQuoteEvent);
    quoteEventSource.addEventListener('edit_quote',handleRemoteQuoteEvent);
    quoteEventSource.addEventListener('revoke_quote',handleRemoteRevokeEvent);
    quoteEventSource.onerror=function(){
      if(generation!==quoteSseGeneration)return;
      quoteEventSource.close();quoteEventSource=null;
      startQuotePollFallback();
    };
    return true;
  }catch(e){if(generation===quoteSseGeneration)startQuotePollFallback();return false;}
}
function disconnectQuoteSSE(){
  quoteSseGeneration++;
  if(quoteEventSource){quoteEventSource.close();quoteEventSource=null;}
  if(quotePollTimer){clearInterval(quotePollTimer);quotePollTimer=null;}
}
function acceptRemoteQuoteRevision(data){
  const incoming=Number(data&&data.stateRevision||0);
  if(!incoming)return true;
  if(incoming<=appliedQuoteStateRevision)return false;
  if(incoming>appliedQuoteStateRevision+1){
    if(!quoteRevisionRefreshPending){
      quoteRevisionRefreshPending=true;
      loadQuoteState({notify:true}).finally(()=>{quoteRevisionRefreshPending=false;});
    }
    return false;
  }
  appliedQuoteStateRevision=incoming;
  return true;
}
function handleRemoteQuoteEvent(e){
  let data;try{data=JSON.parse(e.data);}catch(err){return;}
  const quote=data&&data.quote||data;if(!quote||!quote.quoteId||!acceptRemoteQuoteRevision(data))return;
  const previousKeys=allAlertKeys();
  const manualFreeze=boardSelectionMode==='manual';
  const frozen=manualFreeze?(boardFrozenProjection||captureBoardProjection()):null;
  const existed=!!quoteById(quote.quoteId);
  mergeCanonicalQuote(quote);
  rebuildBoardProjections();
  if(manualFreeze){boardFrozenProjection=frozen;if(!existed)boardPendingNewQuoteCount+=1;restoreBoardProjection(frozen);}
  syncMgmtToday();
  notifyNewAlerts(previousKeys);
  renderAll();updateBadges();
}
function handleRemoteRevokeEvent(e){
  let data;try{data=JSON.parse(e.data);}catch(err){return;}
  if(!data||!data.quoteId||!acceptRemoteQuoteRevision(data))return;
  const previousKeys=allAlertKeys();
  const manualFreeze=boardSelectionMode==='manual';
  const frozen=manualFreeze?(boardFrozenProjection||captureBoardProjection()):null;
  const target=mergeCanonicalQuote(data);
  if(!target)loadQuoteState({notify:true});
  rebuildBoardProjections();
  if(manualFreeze){boardFrozenProjection=frozen;restoreBoardProjection(frozen);}
  syncMgmtToday();
  notifyNewAlerts(previousKeys);
  renderAll();updateBadges();
}
function startQuotePollFallback(){
  if(quotePollTimer)return;
  quotePollTimer=setInterval(async function(){
    if(!currentUser||!authToken)return;
    try{
      await loadQuoteState({notify:true});
      clearInterval(quotePollTimer);quotePollTimer=null;
      connectQuoteSSE();
    }catch(e){}
  },5000);
}

// 记住账号：仅回填用户名，不再存储/回填密码（P0-A3：已删除Base64可逆密码存储，
// 登录页加载时同步清理历史存量键ficc_remember_pass，防止遗留可逆密码残留）
(function(){
  const userInput=document.getElementById('loginUser'),rememberCb=document.getElementById('rememberMe');let savedUser='';
  try{savedUser=localStorage.getItem('ficc_remember_user')||'';localStorage.removeItem('ficc_remember_pass');}catch(e){}
  if(savedUser){userInput.value=savedUser;rememberCb.checked=true;}
})();

document.getElementById('loginBtn').onclick=doLogin;
document.getElementById('loginPass').onkeydown=e=>{if(e.key==='Enter')doLogin();};
document.getElementById('loginUser').onkeydown=e=>{if(e.key==='Enter')document.getElementById('loginPass').focus();};
document.querySelectorAll('#st-quote-precise button').forEach(button=>{button.type='button';});

function toggleUserDropdown(e){e.stopPropagation();document.getElementById('headerUser').classList.toggle('open');}
document.addEventListener('click',()=>document.getElementById('headerUser').classList.remove('open'));

function showProfile(){
  document.getElementById('headerUser').classList.remove('open');
  const u=currentUser||{name:'-',roleName:'',dept:'-',email:'-',phone:'-'};
  showModal('个人中心',`
    <div style="display:flex;align-items:center;gap:16px;margin-bottom:20px;padding-bottom:16px;border-bottom:1px solid #21262d">
      <div style="width:56px;height:56px;border-radius:50%;background:linear-gradient(135deg,#3b82f6,#79c0ff);display:flex;align-items:center;justify-content:center;font-size:22px;font-weight:700;color:#fff">${u.name.charAt(0)}</div>
      <div><div style="font-size:16px;font-weight:600;color:#e6edf3">${u.name}</div><div style="font-size:12px;color:#7d8590;margin-top:2px">${u.roleName||u.role}</div></div>
    </div>
    <div class="form-row"><div class="form-label">部门</div><div class="form-value">${u.dept}</div></div>
    <div class="form-row"><div class="form-label">邮箱</div><div class="form-value">${u.email}</div></div>
    <div class="form-row"><div class="form-label">手机</div><div class="form-value">${u.phone}</div></div>
  `,{footerHtml:'<button class="tb-btn tb-btn-default" type="button" onclick="closeModal()">关闭</button><button class="tb-btn tb-btn-primary" type="button" onclick="showChangePasswordModal(false)">修改密码</button>'});
}
let changePasswordBusy=false;
function passwordChangeErrorText(data,fallback){
  const detail=data&&data.detail;
  if(typeof detail==='string')return detail;
  if(detail&&detail.message)return detail.message;
  if(Array.isArray(detail)&&detail.length)return detail.map(item=>item.msg||'输入不合法').join('；');
  return fallback||'密码修改失败';
}
function changePasswordFormHtml(force){
  const notice=force?'当前账号需要先修改初始密码。':'修改成功后，新密码将在下次登录时使用。';
  return '<div style="font-size:12px;color:#8b949e;margin-bottom:12px">'+notice+'</div>'
    +'<label class="form-label" for="changePasswordOld">当前密码</label><input id="changePasswordOld" class="tb-input" type="password" autocomplete="current-password" style="width:100%;margin:6px 0 12px">'
    +'<label class="form-label" for="changePasswordNew">新密码</label><input id="changePasswordNew" class="tb-input" type="password" autocomplete="new-password" maxlength="128" style="width:100%;margin:6px 0 6px">'
    +'<div style="font-size:11px;color:#7d8590;margin-bottom:12px">至少8位，且不能与当前密码或用户名相同。</div>'
    +'<label class="form-label" for="changePasswordConfirm">确认新密码</label><input id="changePasswordConfirm" class="tb-input" type="password" autocomplete="new-password" maxlength="128" style="width:100%;margin-top:6px">'
    +'<div id="changePasswordError" style="min-height:18px;color:#f85149;font-size:12px;margin-top:10px"></div>';
}
function showChangePasswordModal(force){
  const cancel=force?'<button class="tb-btn tb-btn-default" type="button" onclick="doLogout(false)">退出登录</button>':'<button class="tb-btn tb-btn-default" type="button" onclick="closeModal()">稍后修改</button>';
  showModal('修改密码',changePasswordFormHtml(force),{initialFocus:'#changePasswordOld',footerHtml:cancel+'<button id="changePasswordSubmit" class="tb-btn tb-btn-primary" type="button" onclick="submitPasswordChange('+!!force+')">确认修改</button>'});
}
async function submitPasswordChange(force){
  if(changePasswordBusy)return;
  const oldInput=document.getElementById('changePasswordOld'),newInput=document.getElementById('changePasswordNew'),confirmInput=document.getElementById('changePasswordConfirm'),error=document.getElementById('changePasswordError'),button=document.getElementById('changePasswordSubmit');
  if(!oldInput||!newInput||!confirmInput)return;
  const oldPassword=oldInput.value,newPassword=newInput.value,confirmation=confirmInput.value;
  error.textContent='';
  if(newPassword.length<8){error.textContent='新密码至少需要8位';newInput.focus();return;}
  if(newPassword!==confirmation){error.textContent='两次输入的新密码不一致';confirmInput.focus();return;}
  changePasswordBusy=true;if(button){button.disabled=true;button.textContent='修改中...';}
  try{
    const response=await authFetch(API_BASE+'/auth/change-password',{method:'POST',body:JSON.stringify({old_password:oldPassword,new_password:newPassword})});
    let data={};try{data=await response.json();}catch(e){}
    if(!response.ok)throw new Error(passwordChangeErrorText(data));
    if(currentUser)currentUser.mustChangePassword=false;
    closeModal();showToast('success','密码修改成功');
  }catch(e){error.textContent=e&&e.message||'密码修改失败';}
  finally{changePasswordBusy=false;if(button){button.disabled=false;button.textContent='确认修改';}}
}
function showSettingsModal(){
  document.getElementById('headerUser').classList.remove('open');
  showModal('系统设置',`
    <h3 style="font-size:13px;font-weight:600;color:#e6edf3;margin-bottom:12px;border-left:3px solid #79c0ff;padding-left:10px">偏好设置</h3>
    <div style="margin-bottom:10px"><label class="tb-checkbox"><input type="checkbox" checked> 识别后报价文本留在文本框</label></div>
    <div style="margin-bottom:10px"><label class="tb-checkbox"><input type="checkbox"> 识别报价输入框按Shift+回车换行</label></div>
    <div style="margin-bottom:10px"><label class="tb-checkbox"><input type="checkbox" data-alert-setting="favorite" onchange="setAlertSetting('favorite',this.checked)"> 弹窗提醒 - 关注分组报价提醒</label></div>
    <div style="margin-bottom:10px"><label class="tb-checkbox"><input type="checkbox" data-alert-setting="priceMatch" onchange="setAlertSetting('priceMatch',this.checked)"> 弹窗提醒 - 对价提醒</label></div>
    <div style="margin-bottom:10px"><label class="tb-checkbox"><input type="checkbox" data-alert-setting="filterGroup" onchange="setAlertSetting('filterGroup',this.checked)"> 弹窗提醒 - 筛选分组报价提醒</label></div>
    <div style="margin-bottom:10px"><label class="tb-checkbox"><input type="checkbox" data-alert-setting="bestQuote" onchange="setAlertSetting('bestQuote',this.checked)"> 弹窗提醒 - 我的最优报价</label></div>
    <h3 style="font-size:13px;font-weight:600;color:#e6edf3;margin:16px 0 12px;border-left:3px solid #d29922;padding-left:10px">QT/IDEAL 连接</h3>
    <div style="display:flex;align-items:center;gap:8px;margin-bottom:8px"><span style="width:6px;height:6px;border-radius:50%;background:#3fb950;display:inline-block"></span><span style="color:#3fb950">QT 已连接</span><span style="margin-left:auto;font-size:12px;color:#7d8590">v4.1.4.6</span></div>
    <div style="display:flex;align-items:center;gap:8px"><span style="width:6px;height:6px;border-radius:50%;background:#3fb950;display:inline-block"></span><span style="color:#3fb950">IDEAL 已连接</span><span style="margin-left:auto;font-size:12px;color:#7d8590">v2.1.0</span></div>
  `);
  syncAlertSettingControls();
}
function syncAlertSettingControls(){
  document.querySelectorAll('[data-alert-setting]').forEach(input=>{input.checked=D.alertSettings[input.dataset.alertSetting]!==false;});
}
async function setAlertSetting(key,value){
  if(!Object.prototype.hasOwnProperty.call(D.alertSettings,key))return;
  const previous=D.alertSettings[key];D.alertSettings[key]=!!value;
  rebuildBoardProjections();renderAll();updateBadges();
  try{await saveMarketGroupState();}
  catch(error){D.alertSettings[key]=previous;rebuildBoardProjections();renderAll();showToast('error',error.message||'提醒设置保存失败');}
}

function showToast(type,msg){const c=document.getElementById('toastContainer');const t=document.createElement('div');t.className='toast toast-'+type;t.textContent=msg;c.appendChild(t);setTimeout(()=>{t.style.opacity='0';t.style.transform='translateX(100%)';t.style.transition='all .15s';setTimeout(()=>t.remove(),150);},3000);}
let _alertWrap=null;
const alertPopupState=new Map();
let suppressNextAlertPageRead=false;
function getAlertWrap(){
  if(!_alertWrap){
    _alertWrap=document.createElement('div');
    _alertWrap.className='alert-popup-wrap';
    document.body.appendChild(_alertWrap);
  }
  return _alertWrap;
}
function closeAlertPopup(type){
  const el=document.getElementById('alert-'+type);
  if(el)el.remove();
  alertPopupState.delete(type);
}
function markAlertKeysViewed(keys){
  const normalized=Array.from(new Set((keys||[]).map(value=>String(value||'').trim()).filter(Boolean)));
  normalized.forEach(key=>readAlertKeys.add(key));
  updateBadges();
  if(normalized.length)persistAlertReadKeys(normalized);
}
function viewAlertPopup(type){
  const state=alertPopupState.get(type)||{keys:[],items:[]};
  markAlertKeysViewed(state.keys);
  closeAlertPopup(type);
  const boardPage=document.getElementById('page-board');
  if(boardPage&&!boardPage.classList.contains('active-page')){
    document.querySelectorAll('.nav-btn').forEach(n=>n.classList.remove('active'));
    const boardBtn=document.querySelector('[data-page="board"]');
    if(boardBtn)boardBtn.classList.add('active');
    switchPage('board',boardBtn);
  }
  setTimeout(()=>{
    const alertTab=document.querySelector('[data-st="board-alert"]');
    if(alertTab&&!alertTab.classList.contains('active')){suppressNextAlertPageRead=true;switchSubTab(alertTab);}
    const target=(state.items||[])[0];
    if(!target)return;
    const rows=D.boardAlert||[];
    const targetKeys=new Set(state.keys||[]);
    const exactIndex=rows.findIndex(row=>alertInstanceKeys(row).some(key=>targetKeys.has(key)));
    const index=exactIndex>=0?exactIndex:rows.findIndex(row=>normalizeBondCode(row.c)===normalizeBondCode(target.c));
    if(index<0)return;
    clearBoardSelection();setBoardRowSelected(rows[index],true);boardSelectionMode='manual';
    setRecommendationTarget(rows[index],null,'boardAlertBody');syncBoardSelectionClasses();
    const tableRow=document.querySelectorAll('#boardAlertBody tr:not(.placeholder-row)')[index];
    if(tableRow){tableRow.classList.add('quote-row-reveal');tableRow.scrollIntoView({block:'center',behavior:'smooth'});setTimeout(()=>tableRow.classList.remove('quote-row-reveal'),2600);}
  },100);
}
function showAllAlerts(types,alertsByType){
  const wrap=getAlertWrap();
  types.forEach(type=>{
    let el=document.getElementById('alert-'+type);
    if(!el){el=document.createElement('div');el.id='alert-'+type;el.className='alert-popup';wrap.appendChild(el);}
    const typeClass={'关注债券':'alert-type-fav','对价提醒':'alert-type-match','筛选分组':'alert-type-filter','我的最优':'alert-type-best'}[type]||'alert-type-fav';
    if(type==='对价提醒'){el.classList.add('alert-match-popup');}
    const items=alertsByType[type]||[],showItems=items.slice(-3);
    alertPopupState.set(type,{keys:Array.from(new Set(items.flatMap(alertInstanceKeys))),items:items.slice()});
    let itemsHtml='';
    showItems.forEach(rawItem=>{
      const bidQuote=quoteForRenderedSide(rawItem,'bid'),ofrQuote=quoteForRenderedSide(rawItem,'ofr');
      const item=safeTableValue(rawItem);
      const head='<div style="display:flex;justify-content:space-between;align-items:baseline"><span>'+(item.r?'<span class="tenor">'+item.r+'</span> ':'')+'<span class="bond">'+item.c+'</span> <span style="color:#c9d1d9">'+(item.n||'')+'</span></span><span style="color:#7d8590;font-size:11px;font-family:\'JetBrains Mono\',monospace">'+(item.t||'')+'</span></div>';
      const bidPrice=rawItem.bp?priceTypeMarkHtml(bidQuote)+'<span class="price price-bid">'+escapeQuoteHtml(rawItem.bp)+'</span>'+renderInlineQuoteTags(bidQuote):'<span class="side-na">\u2014</span>';
      const ofrPrice=rawItem.op?priceTypeMarkHtml(ofrQuote)+'<span class="price price-ofr">'+escapeQuoteHtml(rawItem.op)+'</span>'+renderInlineQuoteTags(ofrQuote):'<span class="side-na">\u2014</span>';
      const bidSide=(item.bp||item.trBid||item.bvBid)?'<span class="dir-bid">Bid</span> <span class="trader">'+(item.trBid||'\u2014')+'</span> <span class="vol">'+(item.bvBid||'\u2014')+'</span> '+bidPrice:'<span class="side-na">Bid \u2014 \u2014 \u2014</span>';
      const ofrSide=(item.op||item.trOfr||item.bvOfr)?ofrPrice+' <span class="vol">'+(item.bvOfr||'\u2014')+'</span> <span class="trader">'+(item.trOfr||'\u2014')+'</span> <span class="dir-ofr">Ofr</span>':'<span class="side-na">\u2014 \u2014 \u2014 Ofr</span>';
      itemsHtml+='<div class="alert-item">'+head+'<div style="display:flex;align-items:center;justify-content:space-between;margin-top:3px">'+bidSide+'<span class="vs">VS</span>'+ofrSide+'</div></div>';
    });
    el.innerHTML='<div class="alert-header"><span class="alert-type '+typeClass+'">'+type+' ('+items.length+')</span><div class="alert-actions"><button class="alert-btn alert-btn-view" onclick="viewAlertPopup(\''+type+'\')">查看</button><button class="alert-btn alert-btn-close" onclick="closeAlertPopup(\''+type+'\')">关闭</button></div></div><div class="alert-items">'+itemsHtml+'</div>';
  });
}
function originalQuoteTimeValue(row){
  if(!row)return Number.NEGATIVE_INFINITY;
  for(const raw of [row.updatedAt,row.quotedAt,row.sourceSentAt,row.createdAt]){
    if(!raw)continue;
    const parsed=Date.parse(String(raw).replace(' ','T'));
    if(Number.isFinite(parsed))return parsed;
  }
  const display=row.t||row.time||'',parsed=Date.parse(/^\d{4}-\d{2}-\d{2}/.test(display)?String(display).replace(' ','T'):quoteToday()+'T'+display);
  return Number.isFinite(parsed)?parsed:Number.NEGATIVE_INFINITY;
}
function sortTodayQuoteRows(rows){return(rows||[]).slice().sort((a,b)=>originalQuoteTimeValue(b)-originalQuoteTimeValue(a)||String(b.quoteId||'').localeCompare(String(a.quoteId||'')));}
let _modalReturnFocus=null;
let systemModalState=null;
function systemModalMessageHtml(message){return '<div class="system-modal-message">'+escapeQuoteHtml(String(message||'')).replace(/\n/g,'<br>')+'</div>';}
function finishSystemModal(value){
  const state=systemModalState;if(!state)return;
  systemModalState=null;closeModal();state.resolve(value);
}
function acceptSystemPrompt(){
  const input=document.getElementById('systemModalInput');finishSystemModal(input?input.value:'');
}
function acceptSystemConfirm(){
  const state=systemModalState;
  if(state&&typeof state.confirmHandler==='function'){state.confirmHandler();return;}
  finishSystemModal(true);
}
function systemAlert(title,message){
  return new Promise(resolve=>{
    systemModalState={kind:'alert',resolve,cancelValue:true};
    showModal(title||'提示',systemModalMessageHtml(message),{footerHtml:'<button class="tb-btn tb-btn-primary" type="button" data-system-primary onclick="finishSystemModal(true)">知道了</button>'});
  });
}
function systemConfirm(title,message,options){
  const opts=options||{};
  return new Promise(resolve=>{
    systemModalState={kind:'confirm',resolve,cancelValue:false};
    showModal(title||'请确认',systemModalMessageHtml(message),{footerHtml:'<button class="tb-btn tb-btn-default" type="button" onclick="finishSystemModal(false)">'+escapeQuoteHtml(opts.cancelText||'取消')+'</button><button class="tb-btn tb-btn-primary" type="button" data-system-primary onclick="finishSystemModal(true)">'+escapeQuoteHtml(opts.confirmText||'确定')+'</button>'});
  });
}
function systemConfirmHtml(title,bodyHtml,options){
  const opts=options||{};
  return new Promise(resolve=>{
    systemModalState={kind:'confirm',resolve,cancelValue:false,confirmHandler:opts.confirmHandler};
    showModal(title||'请确认',String(bodyHtml||''),{wide:!!opts.wide,footerHtml:'<button class="tb-btn tb-btn-default" type="button" onclick="finishSystemModal(false)">'+escapeQuoteHtml(opts.cancelText||'取消')+'</button><button class="tb-btn tb-btn-primary" type="button" data-system-primary onclick="acceptSystemConfirm()">'+escapeQuoteHtml(opts.confirmText||'确定')+'</button>'});
  });
}
function systemPrompt(title,message,defaultValue){
  return new Promise(resolve=>{
    systemModalState={kind:'prompt',resolve,cancelValue:null};
    const body=systemModalMessageHtml(message)+'<input class="system-modal-input" id="systemModalInput" autocomplete="off" value="'+escapeQuoteHtml(defaultValue==null?'':defaultValue)+'">';
    showModal(title||'请输入',body,{initialFocus:'#systemModalInput',footerHtml:'<button class="tb-btn tb-btn-default" type="button" onclick="finishSystemModal(null)">取消</button><button class="tb-btn tb-btn-primary" type="button" data-system-primary onclick="acceptSystemPrompt()">确定</button>'});
    requestAnimationFrame(()=>{const input=document.getElementById('systemModalInput');if(input)input.select();});
  });
}
if(window.desktopAPI&&window.desktopAPI.onSystemNotice){
  window.desktopAPI.onSystemNotice(notice=>{if(notice)systemAlert(notice.title||'系统提示',notice.message||'');});
}
function showModal(title,body,opts){
  opts=opts||{};
  _modalReturnFocus=document.activeElement;
  const m=document.getElementById('modalTitle');m.textContent=title;
  const mb=document.getElementById('modalBody');mb.innerHTML=body;mb.className='modal-body'+(opts.fill?' modal-body-fill':'');
  const md=m.closest('.modal');md.className='modal'+(opts.wide?' modal-wide':'');
  const footer=document.getElementById('modalFooter');
  footer.innerHTML=opts.footerHtml||'<button class="tb-btn tb-btn-default" type="button" onclick="closeModal()">关闭</button>';
  const overlay=document.getElementById('modalOverlay');
  overlay.classList.add('show');overlay.setAttribute('aria-hidden','false');
  requestAnimationFrame(()=>{
    const target=(opts.initialFocus&&document.querySelector(opts.initialFocus))||mb.querySelector('input:not([readonly]),select,textarea,button');
    if(target)target.focus();
  });
}
function closeModal(){
  if(systemModalState){const state=systemModalState;systemModalState=null;state.resolve(state.cancelValue);}
  if(refCandidateModalResolve){const resolve=refCandidateModalResolve;refCandidateModalResolve=null;refCandidateModalGroups=[];resolve(null);}
  const overlay=document.getElementById('modalOverlay');
  overlay.classList.remove('show');overlay.setAttribute('aria-hidden','true');
  quoteModalState=null;
  if(_modalReturnFocus&&typeof _modalReturnFocus.focus==='function')_modalReturnFocus.focus();
  _modalReturnFocus=null;
}
document.addEventListener('keydown',e=>{
  if(e.key==='Escape'){
    const commonModal=document.getElementById('modalOverlay');
    if(commonModal&&commonModal.classList.contains('show'))closeModal();
    else closeQuoteEntryModal();
    return;
  }
  const commonModal=document.getElementById('modalOverlay');
  if(e.key==='Enter'&&systemModalState&&commonModal&&commonModal.classList.contains('show')){
    if(e.target&&e.target.id==='recSendQuoteEditor')return;
    e.preventDefault();e.stopPropagation();
    if(systemModalState.kind==='prompt')acceptSystemPrompt();else acceptSystemConfirm();
    return;
  }
  // 全局 Enter 键监控：精确/模糊报价页面或报价录入弹窗
  if(e.key==='Enter'&&!e.shiftKey){
    if(quoteModalState&&document.getElementById('modalOverlay').classList.contains('show')&&e.target.tagName!=='TEXTAREA'){
      saveEditQuote();e.preventDefault();return;
    }
    var quotePage=document.getElementById('page-quote');
    if(!quotePage||(!quotePage.classList.contains('active-page')&&!isQuoteEntryModalOpen()))return;
    // 检查当前子页面
    var precisePage=document.getElementById('st-quote-precise');
    var fuzzyPage=document.getElementById('st-quote-fuzzy');
    var isPrecise=precisePage&&!precisePage.classList.contains('hidden');
    var isFuzzy=fuzzyPage&&!fuzzyPage.classList.contains('hidden');
    if(isPrecise){
      var input=document.getElementById('preciseInput');
      var v=input?input.value.trim():'';
      if(v){doPreciseIdentify();e.preventDefault();}
      else if(D.preciseData&&D.preciseData.length>0){doConfirmPrecise();e.preventDefault();}
    }else if(isFuzzy){
      var input=document.getElementById('fuzzyInput');
      var v=input?input.value.trim():'';
      if(v){doFuzzyIdentify();e.preventDefault();}
      else if(D.fuzzyData&&D.fuzzyData.length>0){doConfirmFuzzy();e.preventDefault();}
    }
  }
});

// ===== 关注债券弹窗 =====
function showFavBondModal(ctx){
  const field=ctx==='rate'?'favBondGroupsRate':'favBondGroupsCredit';
  const saved=D[field]||[];
  let groups;
  if(saved.length===0){
    groups=[];
  }else{
    groups=JSON.parse(JSON.stringify(saved));
    groups.forEach(g=>{g.bonds=g.bonds.map(b=>typeof b==='string'?{code:b,name:''}:b);});
  }
  window._fbm={groups,sIdx:saved.length===0?-1:0,favResults:[],field,market:ctx};
  _fbmRender();
}
function _fbmBondCode(b){return typeof b==='string'?b:b.code;}
function _fbmBondName(b){return typeof b==='string'?'':b.name;}
function _fbmRender(){
  const S=window._fbm,g=S.groups,sIdx=S.sIdx;
  const delSvg='<svg viewBox="0 0 24 24" width="12" height="12" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/></svg>';
  let groupListHtml='';
  g.forEach((gr,i)=>{
    groupListHtml+=`<div class="fbm-group-item${i===sIdx?' active':''}" title="双击重命名" onclick="_fbmGroupClick(${i},event)" ondblclick="event.preventDefault();event.stopPropagation();_fbmRename(${i})"><span class="gname">${escapeQuoteHtml(gr.name)}<span style="color:#7d8590;font-size:10px;margin-left:4px">(${gr.bonds.length})</span></span><span class="gdel" onclick="event.stopPropagation();_fbmDeleteGroup(${i})">${delSvg}</span></div>`;
  });
  groupListHtml+=`<div class="fbm-group-item" id="fbmNewGroupRow" style="display:none;cursor:default;background:rgba(59,130,246,0.06)"><input id="fbmNewGroupInput" style="flex:1;background:transparent;border:none;color:#e6edf3;font-size:12px;outline:none;padding:0;font-family:inherit" maxlength="10" onblur="_fbmConfirmNewGroup(this)" onkeydown="if(event.key==='Enter'){event.preventDefault();this.blur();}else if(event.key==='Escape'){this.dataset.cancel='1';this.blur();}"></div>`;
  let bondListHtml='';
  if(sIdx>=0&&g[sIdx]){
    if(g[sIdx].bonds.length===0){
      bondListHtml='<div style="text-align:center;padding:16px 0;color:#484f58;font-size:12px">暂无关注债券</div>';
    }else{
      g[sIdx].bonds.forEach((b,bi)=>{
        const code=_fbmBondCode(b),name=_fbmBondName(b);
        const display=escapeQuoteHtml(name?code+' '+name:code),title=escapeQuoteHtml(name||code);
        bondListHtml+=`<div style="display:flex;align-items:center;justify-content:space-between;padding:6px 10px;border-bottom:1px solid #21262d;font-size:12px;overflow:hidden"><span style="color:#79c0ff;font-family:'JetBrains Mono',monospace;overflow:hidden;text-overflow:ellipsis;white-space:nowrap;flex:1;min-width:0" title="${title}">${display}</span><span style="color:#7d8590;font-size:10px;cursor:pointer;padding:2px 6px;border-radius:3px;flex-shrink:0" onmouseover="this.style.background='rgba(248,81,73,0.15)';this.style.color='#f85149'" onmouseout="this.style.background='';this.style.color='#7d8590'" onclick="window._fbm.groups[${sIdx}].bonds.splice(${bi},1);_fbmRenderLeft()"><svg viewBox="0 0 24 24" width="10" height="10" fill="none" stroke="currentColor" stroke-width="2"><line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/></svg></span></div>`;
      });
    }
  }else{
    bondListHtml='<div style="text-align:center;padding:16px 0;color:#484f58;font-size:12px">请先选择分组</div>';
  }
  let resultHtml='<table class="fbm-table"><thead><tr><th style="width:32px;text-align:center"><input type="checkbox" onchange="_fbmToggleAll(this.checked)"></th><th>剩余期限</th><th>债券代码</th><th>债券简称</th><th>债券库</th><th>中债估值</th></tr></thead><tbody>';
  if(S.favResults.length>0){
    const curBonds=(sIdx>=0&&g[sIdx])?g[sIdx].bonds.map(b=>_fbmBondCode(b)):[];
    S.favResults.forEach((r,i)=>{
      const isAdded=curBonds.includes(r.code);
      resultHtml+=`<tr style="${r.found?'':'opacity:0.4'}${isAdded?' background:rgba(63,185,80,0.06)':''}"><td style="text-align:center"><input type="checkbox" data-idx="${i}" ${isAdded?'checked':''} onchange="_fbmAutoAdd(${i},this.checked)"></td><td>${escapeQuoteHtml(r.tenor||'—')}</td><td class="code-cell">${escapeQuoteHtml(r.code)}</td><td>${escapeQuoteHtml(r.name)}</td><td>${escapeQuoteHtml(r.lib||'—')}</td><td>${escapeQuoteHtml(r.val||'—')}</td></tr>`;
    });
  }else{
    for(let k=0;k<12;k++){resultHtml+='<tr style="height:32px"><td></td><td></td><td></td><td></td><td></td><td></td></tr>';}
  }
  resultHtml+='</tbody></table>';
  const html=`<div class="fbm-layout">
    <div id="fbmLeft" class="fbm-left" style="width:260px;display:flex;flex-direction:column">
      <div style="flex:0 0 50%;display:flex;flex-direction:column;overflow:hidden">
        <div class="fbm-group-list" style="flex:1;overflow-y:auto;padding:2px 0">${groupListHtml}</div>
        <div style="padding:6px 8px"><div style="display:flex;align-items:center;justify-content:center;padding:5px 0;font-size:11px;color:#58a6ff;cursor:pointer;border:1px dashed rgba(59,130,246,0.3);border-radius:4px;background:rgba(59,130,246,0.05)" onclick="_fbmStartNewGroup()">+ 新建</div></div>
      </div>
      <div style="flex:0 0 50%;overflow-y:auto;border-top:1px solid #30363d">${bondListHtml}</div>
    </div>
    <div class="fbm-right" style="display:flex;flex-direction:column">
      <div style="flex:0 0 auto;position:relative;background:#0d1117;border-bottom:1px solid #30363d;padding-bottom:10px">
        <textarea id="fbmInput" style="width:100%;height:150px;resize:none;font-size:12px;font-family:'JetBrains Mono',monospace;background:#0d1117;border:none;border-radius:0;padding:10px 60px 10px 12px;color:#e6edf3;outline:none" placeholder="粘贴债券代码，多个用空格、逗号或顿号分隔&#10;例：252980、240210 188572" onkeydown="if(event.key==='Enter'&&!event.shiftKey){event.preventDefault();_fbmIdentify();}"></textarea>
        <div style="position:absolute;right:12px;bottom:10px;display:flex;gap:6px"><div style="display:flex;align-items:center;justify-content:center;padding:4px 14px;font-size:11px;font-weight:600;border:1px solid #30363d;border-radius:4px;background:#21262d;color:#8b949e;cursor:pointer" onclick="_fbmClear()">清空</div><div id="fbmIdentifyBtn" style="display:flex;align-items:center;justify-content:center;padding:4px 14px;font-size:11px;font-weight:600;border:1px solid rgba(59,130,246,0.3);border-radius:4px;background:rgba(59,130,246,0.1);color:#58a6ff;cursor:pointer" onclick="_fbmIdentify()">识别</div></div>
      </div>
      <div class="fbm-right-body" style="flex:1;overflow-y:auto">${resultHtml}</div>
    </div>
  </div>`;
  showModal('关注债券管理',html,{wide:true,fill:true});
  document.getElementById('modalFooter').innerHTML='<button class="tb-btn tb-btn-default" onclick="closeModal()">取消</button><button class="tb-btn tb-btn-primary" style="background:#3b82f6;color:#fff;border:none" onclick="_fbmSave()">保存</button>';
}
function _fbmRenderLeft(){
  const S=window._fbm,g=S.groups,sIdx=S.sIdx;
  const delSvg='<svg viewBox="0 0 24 24" width="12" height="12" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/></svg>';
  let groupListHtml='';
  g.forEach((gr,i)=>{
    groupListHtml+=`<div class="fbm-group-item${i===sIdx?' active':''}" title="双击重命名" onclick="_fbmGroupClick(${i},event)" ondblclick="event.preventDefault();event.stopPropagation();_fbmRename(${i})"><span class="gname">${escapeQuoteHtml(gr.name)}<span style="color:#7d8590;font-size:10px;margin-left:4px">(${gr.bonds.length})</span></span><span class="gdel" onclick="event.stopPropagation();_fbmDeleteGroup(${i})">${delSvg}</span></div>`;
  });
  groupListHtml+=`<div class="fbm-group-item" id="fbmNewGroupRow" style="display:none;cursor:default;background:rgba(59,130,246,0.06)"><input id="fbmNewGroupInput" style="flex:1;background:transparent;border:none;color:#e6edf3;font-size:12px;outline:none;padding:0;font-family:inherit" maxlength="10" onblur="_fbmConfirmNewGroup(this)" onkeydown="if(event.key==='Enter'){event.preventDefault();this.blur();}else if(event.key==='Escape'){this.dataset.cancel='1';this.blur();}"></div>`;
  let bondListHtml='';
  if(sIdx>=0&&g[sIdx]){
    if(g[sIdx].bonds.length===0){
      bondListHtml='<div style="text-align:center;padding:16px 0;color:#484f58;font-size:12px">暂无关注债券</div>';
    }else{
      g[sIdx].bonds.forEach((b,bi)=>{
        const code=_fbmBondCode(b),name=_fbmBondName(b);
        const display=escapeQuoteHtml(name?code+' '+name:code),title=escapeQuoteHtml(name||code);
        bondListHtml+=`<div style="display:flex;align-items:center;justify-content:space-between;padding:6px 10px;border-bottom:1px solid #21262d;font-size:12px;overflow:hidden"><span style="color:#79c0ff;font-family:'JetBrains Mono',monospace;overflow:hidden;text-overflow:ellipsis;white-space:nowrap;flex:1;min-width:0" title="${title}">${display}</span><span style="color:#7d8590;font-size:10px;cursor:pointer;padding:2px 6px;border-radius:3px;flex-shrink:0" onmouseover="this.style.background='rgba(248,81,73,0.15)';this.style.color='#f85149'" onmouseout="this.style.background='';this.style.color='#7d8590'" onclick="window._fbm.groups[${sIdx}].bonds.splice(${bi},1);_fbmRenderLeft()"><svg viewBox="0 0 24 24" width="10" height="10" fill="none" stroke="currentColor" stroke-width="2"><line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/></svg></span></div>`;
      });
    }
  }else{
    bondListHtml='<div style="text-align:center;padding:16px 0;color:#484f58;font-size:12px">请先选择分组</div>';
  }
  const leftEl=document.getElementById('fbmLeft');
  if(leftEl){
    leftEl.children[0].querySelector('.fbm-group-list').innerHTML=groupListHtml;
    leftEl.children[1].innerHTML=bondListHtml;
    _fbmSyncCheckState();
  }
}
function _fbmRenderRight(){
  const S=window._fbm,g=S.groups,sIdx=S.sIdx;
  let resultHtml='<table class="fbm-table"><thead><tr><th style="width:32px;text-align:center"><input type="checkbox" onchange="_fbmToggleAll(this.checked)"></th><th>剩余期限</th><th>债券代码</th><th>债券简称</th><th>债券库</th><th>中债估值</th></tr></thead><tbody>';
  if(S.favResults.length>0){
    const curBonds=(sIdx>=0&&g[sIdx])?g[sIdx].bonds.map(b=>_fbmBondCode(b)):[];
    S.favResults.forEach((r,i)=>{
      const isAdded=curBonds.includes(r.code);
      resultHtml+=`<tr style="${r.found?'':'opacity:0.4'}${isAdded?' background:rgba(63,185,80,0.06)':''}"><td style="text-align:center"><input type="checkbox" data-idx="${i}" ${isAdded?'checked':''} onchange="_fbmAutoAdd(${i},this.checked)"></td><td>${escapeQuoteHtml(r.tenor||'—')}</td><td class="code-cell">${escapeQuoteHtml(r.code)}</td><td>${escapeQuoteHtml(r.name)}</td><td>${escapeQuoteHtml(r.lib||'—')}</td><td>${escapeQuoteHtml(r.val||'—')}</td></tr>`;
    });
  }else{
    for(let k=0;k<12;k++){resultHtml+='<tr style="height:32px"><td></td><td></td><td></td><td></td><td></td><td></td></tr>';}
  }
  resultHtml+='</tbody></table>';
  const rightBody=document.querySelector('.fbm-right-body');
  if(rightBody)rightBody.innerHTML=resultHtml;
}
function _fbmSyncCheckState(){
  const S=window._fbm,sIdx=S.sIdx,g=S.groups;
  const curBonds=(sIdx>=0&&g[sIdx])?g[sIdx].bonds.map(b=>_fbmBondCode(b)):[];
  document.querySelectorAll('.fbm-right-body input[type=checkbox][data-idx]').forEach(cb=>{
    const r=S.favResults[parseInt(cb.dataset.idx)];
    if(r){const isAdded=curBonds.includes(r.code);cb.checked=isAdded;cb.closest('tr').style.background=isAdded?'rgba(63,185,80,0.06)':r.found?'':'opacity:0.4';}
  });
}
function _updateSidebarFilterGroups(){
  const creditGroups=D.filterGroupsCredit||[];
  const rateGroups=D.filterGroupsRate||[];
  renderMarketGroupTags('sidebar-filter-groups-credit',creditGroups,'filter','credit');
  renderMarketGroupTags('sidebar-filter-groups-rate',rateGroups,'filter','rate');
  renderMarketGroupTags('sidebar-filter-groups-merged',creditGroups.concat(rateGroups),'filter');
  renderMarketGroupTags('sidebar-filter-groups-merged-mgmt',creditGroups.concat(rateGroups),'filter');
}
function _updateSidebarFavBonds(){
  const creditGroups=D.favBondGroupsCredit||[];
  const rateGroups=D.favBondGroupsRate||[];
  renderMarketGroupTags('sidebar-fav-bonds-credit',creditGroups,'favorite','credit');
  renderMarketGroupTags('sidebar-fav-bonds-rate',rateGroups,'favorite','rate');
  renderMarketGroupTags('sidebar-fav-bonds-merged',creditGroups.concat(rateGroups),'favorite');
  renderMarketGroupTags('sidebar-fav-bonds-merged-mgmt',creditGroups.concat(rateGroups),'favorite');
  D.favBonds=[...new Set(creditGroups.concat(rateGroups).flatMap(group=>(group.bonds||[]).map(_fbmBondCode)))];
}
function renderMarketGroupTags(elementId,groups,kind,addMarket){
  const element=document.getElementById(elementId);if(!element)return;
  const fragment=document.createDocumentFragment();
  groups.filter(g=>!(g.name==='默认'&&(kind==='favorite'?(g.bonds||[]).length===0:(g.conditions||[]).length===0))).forEach(group=>{
    const button=document.createElement('button');
    button.type='button';button.className='filter-tag';
    button.dataset.marketGroupId=group.id;
    button.textContent=group.name;
    const count=kind==='favorite'?(group.bonds||[]).length:(group.conditions||[]).length;
    button.title=group.name+' · '+count+(kind==='favorite'?'只债券':'个条件');
    if(activeMarketGroupIds[kind].has(group.id))button.classList.add('active');
    if(kind==='filter'&&marketGroupQueryErrors.has(group.id)){
      button.classList.add('market-group-error');button.title=marketGroupQueryErrors.get(group.id);
    }
    fragment.appendChild(button);
  });
  if(addMarket){
    const add=document.createElement('button');
    add.type='button';add.className='filter-tag-add';add.textContent='+';
    add.title=kind==='favorite'?'管理关注债券分组':'管理筛选分组';
    add.setAttribute('aria-label',add.title);
    add.addEventListener('click',event=>{
      event.preventDefault();event.stopPropagation();
      if(kind==='favorite')showFavBondModal(addMarket);else showFilterGroupModal(addMarket);
    });
    fragment.appendChild(add);
  }
  element.replaceChildren(fragment);
}
function _fbmClear(){
  const input=document.getElementById('fbmInput');
  if(input)input.value='';
  window._fbm.favResults=[];
  _fbmRenderRight();
}
function _fbmGroupClick(idx,e){
  if(e)e.stopPropagation();
  const S=window._fbm;
  S.sIdx=idx;_fbmRenderLeft();_fbmSyncCheckState();
}
function _fbmStartNewGroup(){
  const row=document.getElementById('fbmNewGroupRow');
  const input=document.getElementById('fbmNewGroupInput');
  if(!row||!input)return;
  row.style.display='flex';
  input.value='';
  input.dataset.cancel='';
  input.focus();
}
function _fbmConfirmNewGroup(input){
  if(input.dataset.cancel==='1'){document.getElementById('fbmNewGroupRow').style.display='none';return;}
  const val=input.value.trim();
  if(val){
    if(window._fbm.groups.some(group=>group.name.toLocaleLowerCase()===val.toLocaleLowerCase())){showToast('error','分组名称不能重复');_fbmRenderLeft();return;}
    window._fbm.groups.push({id:newLocalMarketGroupId(),name:val,bonds:[]});
    window._fbm.sIdx=window._fbm.groups.length-1;
  }
  _fbmRenderLeft();
}
function _fbmDeleteGroup(idx){
  const g=window._fbm.groups;
  if(g[idx].bonds.length>0){
    _fbmConfirm('分组中有关注的债券，是否确认删除？',()=>{
      g.splice(idx,1);
      if(window._fbm.sIdx>=g.length)window._fbm.sIdx=g.length-1;
      _fbmRenderLeft();_fbmSyncCheckState();
    });
  }else{
    g.splice(idx,1);
    if(window._fbm.sIdx>=g.length)window._fbm.sIdx=g.length-1;
    _fbmRenderLeft();_fbmSyncCheckState();
  }
}
function _fbmConfirm(msg,onOk){
  const overlay=document.createElement('div');
  overlay.style.cssText='position:fixed;inset:0;background:rgba(0,0,0,0.5);z-index:2000;display:flex;align-items:center;justify-content:center';
  overlay.innerHTML=`<div style="background:#161b22;border:1px solid #30363d;border-radius:8px;width:360px;padding:20px;text-align:center"><div style="font-size:13px;color:#c9d1d9;margin-bottom:16px;line-height:1.6">${escapeQuoteHtml(msg)}</div><div style="display:flex;justify-content:center;gap:10px"><button style="padding:6px 20px;font-size:12px;border:1px solid #30363d;border-radius:4px;background:#21262d;color:#c9d1d9;cursor:pointer" id="fbmConfirmCancel">取消</button><button style="padding:6px 20px;font-size:12px;border:none;border-radius:4px;background:#ef4444;color:#fff;cursor:pointer" id="fbmConfirmOk">确认</button></div></div>`;
  document.body.appendChild(overlay);
  overlay.querySelector('#fbmConfirmCancel').onclick=()=>overlay.remove();
  overlay.querySelector('#fbmConfirmOk').onclick=()=>{overlay.remove();onOk();};
  overlay.onclick=e=>{if(e.target===overlay)overlay.remove();};
}
function _fbmRename(idx){
  const g=window._fbm.groups;
  const items=document.querySelectorAll('#fbmLeft .fbm-group-item');
  const el=items[idx];
  if(!el)return;
  const nameEl=el.querySelector('.gname');
  const oldName=g[idx].name;
  const inp=document.createElement('input');
  inp.id='fbmRenameInput';inp.maxLength=30;inp.value=oldName;
  inp.style.cssText='background:transparent;border:none;color:#e6edf3;font-size:12px;outline:none;padding:0;width:120px;font-family:inherit';
  inp.addEventListener('blur',()=>_fbmConfirmRename(idx,inp));
  inp.addEventListener('keydown',event=>{if(event.key==='Enter'){event.preventDefault();inp.blur();}else if(event.key==='Escape'){inp.value=oldName;inp.blur();}});
  nameEl.replaceChildren(inp);
  inp.focus();inp.select();
}
function _fbmConfirmRename(idx,input){
  const val=input.value.trim();
  if(val&&val!==window._fbm.groups[idx].name){
    if(window._fbm.groups.some((group,groupIdx)=>groupIdx!==idx&&group.name.toLocaleLowerCase()===val.toLocaleLowerCase()))showToast('error','分组名称不能重复');
    else window._fbm.groups[idx].name=val;
  }
  _fbmRenderLeft();
}
function _fbmAutoAdd(idx,checked){
  const S=window._fbm,sIdx=S.sIdx;
  if(sIdx<0||!S.groups[sIdx])return;
  const r=S.favResults[idx];
  if(!r)return;
  if(!checked){
    S.groups[sIdx].bonds=S.groups[sIdx].bonds.filter(bond=>normalizeBondCode(_fbmBondCode(bond))!==normalizeBondCode(r.code));
    _fbmRenderLeft();_fbmSyncCheckState();return;
  }
  const expectedMarket=S.market==='rate'?'rate':'credit';
  if(r.market&&r.market!==expectedMarket){showToast('error','该债券不属于当前'+(expectedMarket==='rate'?'利率债':'信用债')+'分组');_fbmSyncCheckState();return;}
  if(r&&r.found&&!S.groups[sIdx].bonds.some(b=>_fbmBondCode(b)===r.code)){
    S.groups[sIdx].bonds.push({code:r.code,name:r.name});
    _fbmRenderLeft();_fbmSyncCheckState();
  }
}
function _fbmIdentify(){
  const input=document.getElementById('fbmInput');
  const text=(input?input.value:'').trim();
  if(!text){showToast('error','请输入债券代码');return;}
  const btn=document.getElementById('fbmIdentifyBtn');
  if(btn)btn.textContent='识别中...';
  authFetch(API_BASE+'/parse',{method:'POST',body:JSON.stringify({text:text,lookup:true,mode:'bond'})})
    .then(r=>r.json()).then(d=>{
      if(btn)btn.textContent='识别';
      if(!d.ok||!d.data||d.data.length===0){window._fbm.favResults=[];_fbmRenderRight();showToast('error','未能识别出有效债券');return;}
      window._fbm.favResults=d.data.map(r=>({code:normalizeBondCode(r.code),name:r.name||'',found:!!r.found,tenor:r.tenor||'—',lib:'',val:r.valuation||'—',market:classifyBondMarket(r.code,r.name,r._apiInfo,r.bondType)}));
      _fbmRenderRight();
    }).catch(()=>{if(btn)btn.textContent='识别';showToast('error','识别服务异常');});
}
function _fbmToggleAll(checked){
  const cbs=document.querySelectorAll('.fbm-right-body input[type=checkbox][data-idx]');
  const S=window._fbm,sIdx=S.sIdx;if(sIdx<0||!S.groups[sIdx])return;
  if(!checked){
    const resultCodes=new Set(S.favResults.map(result=>normalizeBondCode(result.code)));
    S.groups[sIdx].bonds=S.groups[sIdx].bonds.filter(bond=>!resultCodes.has(normalizeBondCode(_fbmBondCode(bond))));
    _fbmRenderLeft();_fbmSyncCheckState();return;
  }
  cbs.forEach(cb=>{const idx=parseInt(cb.dataset.idx);if(!cb.checked)_fbmAutoAdd(idx,true);});
}
async function _fbmSave(){
  const field=window._fbm.field||'favBondGroupsCredit';
  const previous=D[field];
  D[field]=window._fbm.groups.slice();
  const btn=document.querySelector('#modalFooter .tb-btn-primary');if(btn){btn.disabled=true;btn.textContent='保存中…';}
  try{
    await saveMarketGroupState();
    // P0: 关注债券保存只刷新关注相关视图，绝不触发筛选重查
    _updateSidebarFavBonds();renderFilteredViews();closeModal();showToast('success','关注债券分组已保存');
  }catch(error){
    D[field]=previous;_updateSidebarFavBonds();showToast('error',error.message||'保存失败');
  }finally{if(btn){btn.disabled=false;btn.textContent='保存';}
  }
}

// ===== 筛选分组弹窗 =====
const _FG_DATA=[
  {sec:'债券筛选',groups:[
    {l:'细分券种',items:['短融','中票','企业债','公司债','金融债','PPN','ABS','可转债','NCD','其他']},
    {l:'热门标签',items:['商金债','银行二级','银行永续','政策性次级','保险次级','券商债','保险债','铁道债','可质押','跨市场','城投','房地产','产能过剩','公用事业','上市公司','科创债','TLAC债','新上市','不含假期','绿色债']},
    {l:'NCD类型',items:['国有','股份','城商','农商','外资','其他']},
    {l:'剩余期限',items:['<3M','3-6M','6-9M','9-12M','1-3Y','3-5Y','5-7Y','7-10Y','10-15Y','15-20Y','>20Y']},
    {l:'债券上市状态',items:['上市','非上市']},
    {l:'发行方式',items:['公募','小公募','私募']}
  ]},
  {sec:'流动性筛选',groups:[
    {l:'债券流动性',items:['S','A','B','C','D']},
    {l:'做市标的',items:['上证信用','深证信用']},
    {l:'上市场所',items:['银行间','上交所','深交所','北交所']}
  ]},
  {sec:'信用筛选',groups:[
    {l:'银行业',items:['国有行','股份制','城商行','农商行']},
    {l:'行业',items:['城投','房地产','煤炭','钢铁','化工','建筑','交运','公用事业']},
    {l:'企业性质',items:['央企','国企','民企','其他']},
    {l:'担保',items:['有担保','无担保','全国性担保','地方性担保']},
    {l:'主体评级',items:['AAA','AA+','AA','AA-','A+','A']},
    {l:'中债资信',items:['AAA+','AAA','AAA-','AA+']},
    {l:'评级展望',items:['正面','稳定','观察','负面']},
    {l:'债项评级',items:['AAA','A-1','AA+','AA','AA-']},
    {l:'中债隐含',items:['AAA+','AAA','AAA-','AA+']},
    {l:'YY评分',items:['投资级','投机级','风险级']},
    {l:'中证隐含',items:['AAA+','AAA','AA+','AA','AA2']}
  ]},
  {sec:'条款筛选',groups:[
    {l:'含权属性',items:['不含权','含权','赎回权','回售权','其他权']},
    {l:'偿还方式',items:['到期','提前还本']},
    {l:'付息方式',items:['按月','按季','半年','按年','其他']},
    {l:'永续债',items:['城投永续','产业永续','金融永续','非永续']},
    {l:'次级债',items:['次级','非次级']},
    {l:'利率方式',items:['SHIBOR','DEPO','LPR','固息']},
    {l:'发行年份',items:['2026','2025','2024','2023']}
  ]},
  {sec:'区域筛选',groups:[
    {l:'城投',items:['城投','非城投']},
    {l:'重点省份',items:['12重点省','非12重点省']},
    {l:'城投层级',items:['省级','市级','区县级','其他']},
    {l:'地区',items:['好地区','中部地区','弱地区','差地区']}
  ]}
];
function _fgmConditionFull(condition){
  const normalized=normalizedMarketCondition(condition);
  return normalized?normalized.label+':'+String(normalized.value):String(condition||'');
}
function _fgmConditionFromFull(full){
  return normalizedMarketCondition(full);
}
function availableFilterGroupData(){
  if(!bondFilterCapabilities.byLabel.size)return[];
  return _FG_DATA.map(section=>({
    sec:section.sec,
    groups:section.groups.map(group=>{
      const capability=bondFilterCapabilities.byLabel.get(group.l);
      if(!capability||capability.visible===false||capability.enabled!==true)return null;
      const optionMap=new Map((capability.options||[]).map(option=>[normalizeBondFilterValue(capability.key,option.value),option]));
      const items=group.items.filter(item=>{
        if(!optionMap.size)return true;
        const option=optionMap.get(normalizeBondFilterValue(capability.key,item));
        return option&&option.enabled===true;
      });
      return items.length?{...group,items}:null;
    }).filter(Boolean),
  })).filter(section=>section.groups.length);
}
function availableMarketFilterGroupData(market){
  const normalizedMarket=market==='rate'?'rate':'credit';
  if(marketFilterCatalogCache.has(normalizedMarket))return marketFilterCatalogCache.get(normalizedMarket);
  const config=BOND_FILTER_SCOPES[normalizedMarket],panel=config&&document.getElementById(config.panelId);
  if(!panel)return[];
  const allowedSections=MARKET_FILTER_GROUP_SECTIONS[normalizedMarket]||new Set();
  const sections=[];
  panel.querySelectorAll(':scope > .filter-section').forEach(section=>{
    const title=section.querySelector(':scope > .filter-section-title'),sectionName=title&&title.textContent.trim();
    if(!sectionName||!allowedSections.has(sectionName))return;
    const groups=[];
    section.querySelectorAll(':scope > .filter-group').forEach(group=>{
      const label=group.querySelector(':scope > .filter-label'),name=label&&label.textContent.trim();
      const capability=name&&bondFilterCapabilities.byLabel.get(name);
      if(!name)return;
      const key=capability&&capability.key||'';
      const enabled=!!(capability&&capability.enabled===true&&marketConditionAllowed(normalizedMarket,{key,label:name,value:''}));
      const type=String(capability&&capability.type||'tag'),items=[];
      group.querySelectorAll('.filter-tag:not(.filter-tag-add)').forEach(tag=>{
        const value=normalizeBondFilterValue(key,tag.dataset.filterValue||tag.textContent);
        const itemEnabled=enabled&&!tag.classList.contains('filter-control-unsupported');
        if(value&&value!=='全部'&&!items.some(item=>JSON.stringify(item.value)===JSON.stringify(value)))items.push({label:value,value,enabled:itemEnabled});
      });
      group.querySelectorAll('select option').forEach(option=>{
        let value=normalizeBondFilterValue(key,option.value||option.textContent),itemLabel=option.textContent.trim();
        if(key==='region')value=regionSelectionFromSelect(group.querySelector('select'))&&String(option.value)===String(group.querySelector('select').value)?regionSelectionFromSelect(group.querySelector('select')):chinaRegionsByCode.get(String(option.value));
        if(value&&itemLabel!=='不限'&&!items.some(item=>JSON.stringify(item.value)===JSON.stringify(value)))items.push({label:itemLabel,value,enabled:enabled&&!option.disabled});
      });
      const hasText=!!group.querySelector('input:not(.filter-input)'),hasRange=!!group.querySelector('.filter-input');
      const hasSelect=!!group.querySelector('select');
      if(items.length||hasText||hasRange||hasSelect)groups.push({l:name,key,type,items,hasText,hasRange,enabled,disabledReason:capability&&capability.disabled_reason||'当前筛选条件不可用'});
    });
    if(groups.length)sections.push({sec:sectionName,groups});
  });
  marketFilterCatalogCache.set(normalizedMarket,sections);return sections;
}
function marketGroupConditionIdentity(condition){
  const normalized=normalizedMarketCondition(condition);
  return normalized?normalized.key+'|'+JSON.stringify(normalized.value):'';
}
function marketGroupConditionDisplay(condition){
  const normalized=normalizedMarketCondition(condition);
  if(!normalized)return String(condition||'');
  const value=normalized.value;
  if(value&&typeof value==='object'){
    if(value.code)return normalized.label+':'+(value.district||value.city||value.province||value.code);
    const min=value.min==null?'':value.min,max=value.max==null?'':value.max;
    return normalized.label+':'+min+'-'+max+(value.unit||'');
  }
  return normalized.label+':'+String(value);
}
function marketGroupConditionValueDisplay(condition){
  const normalized=normalizedMarketCondition(condition);if(!normalized)return String(condition||'');
  const value=normalized.value;
  if(value&&typeof value==='object'){
    if(value.code)return value.district||value.city||value.province||value.code;
    const min=value.min==null?'':value.min,max=value.max==null?'':value.max;
    return min+'-'+max+(value.unit||'');
  }
  return String(value);
}
function filterGroupCatalog(){return window._fgm&&window._fgm.catalog||[];}
// 返回当前活跃子分组的条件数组（不存在时自动补齐，默认第一个子分组）
function _fgmActiveSubgroup(){
  const S=window._fgm;if(!S||S.sIdx<0||!S.groups[S.sIdx])return null;
  const gr=S.groups[S.sIdx];
  if(!Array.isArray(gr.subgroups))gr.subgroups=[Array.isArray(gr.conditions)&&gr.conditions.length?[...gr.conditions]:[]];
  if(!gr.subgroups.length)gr.subgroups.push([]);
  if(S.sgIdx==null||S.sgIdx<0||S.sgIdx>=gr.subgroups.length)S.sgIdx=0;
  return gr.subgroups[S.sgIdx];
}
function _fgmCondListHtml(){
  const S=window._fgm,gr=S.sIdx>=0&&S.groups[S.sIdx];
  if(!gr)return'';
  if(!Array.isArray(gr.subgroups))gr.subgroups=[Array.isArray(gr.conditions)&&gr.conditions.length?[...gr.conditions]:[]];
  if(!gr.subgroups.length)gr.subgroups.push([]);
  if(S.sgIdx==null||S.sgIdx<0||S.sgIdx>=gr.subgroups.length)S.sgIdx=0;
  const delSvg='<svg viewBox="0 0 24 24" width="11" height="11" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/></svg>';
  let html='<div class="fgm-subgroup-scroll">';
  gr.subgroups.forEach((conds,si)=>{
    html+=`<div class="fgm-subgroup${si===S.sgIdx?' active':''}" onclick="_fgmSetSubgroup(${si})">`;
    if(conds.length===0){
      html+='<div style="padding:8px 10px;color:#484f58;font-size:11px">暂无条件，点击右侧目录或识别文本添加</div>';
    }else{
      html+='<div class="fgm-subgroup-items">';
      conds.forEach((c,ci)=>{
        const full=marketGroupConditionDisplay(c),value=marketGroupConditionValueDisplay(c);
        html+=`<span class="fgm-condition-chip" title="${escapeQuoteHtml(full)}"><span>${escapeQuoteHtml(value)}</span><button type="button" aria-label="删除筛选项" onclick="event.stopPropagation();_fgmRemoveCondition(${si},${ci})">${delSvg}</button></span>`;
      });
      html+='</div>';
    }
    html+=`<span class="fgm-subgroup-del" title="删除子分组" onclick="event.stopPropagation();_fgmDeleteSubgroup(${si})">${delSvg}</span></div>`;
  });
  html+='</div>';
  html+='<div class="fgm-add-subgroup"><button type="button" onclick="_fgmAddSubgroup()">+ 新增分组</button></div>';
  return html;
}
function _fgmSetSubgroup(si){
  const S=window._fgm;if(!S)return;
  S.sgIdx=si;_fgmRenderLeft();
}
function _fgmAddSubgroup(){
  const S=window._fgm,gr=S&&S.sIdx>=0&&S.groups[S.sIdx];
  if(!gr)return;
  if(!Array.isArray(gr.subgroups))gr.subgroups=[Array.isArray(gr.conditions)&&gr.conditions.length?[...gr.conditions]:[]];
  gr.subgroups.push([]);
  S.sgIdx=gr.subgroups.length-1;
  _fgmRenderLeft();
}
function _fgmRemoveCondition(si,ci){
  const S=window._fgm,gr=S&&S.sIdx>=0&&S.groups[S.sIdx];
  if(!gr||!Array.isArray(gr.subgroups)||!gr.subgroups[si])return;
  gr.subgroups[si].splice(ci,1);
  _fgmRenderLeft();
}
function _fgmDeleteSubgroup(si){
  const S=window._fgm,gr=S&&S.sIdx>=0&&S.groups[S.sIdx];
  if(!gr||!Array.isArray(gr.subgroups)||!gr.subgroups[si])return;
  const doDelete=()=>{
    gr.subgroups.splice(si,1);
    if(!gr.subgroups.length)gr.subgroups=[[]];
    S.sgIdx=0;
    _fgmRenderLeft();
  };
  if(gr.subgroups[si].length>0){_fbmConfirm('子分组中有已选条件，是否确认删除？',doDelete);}
  else doDelete();
}
function _fgmAddStructuredCondition(condition){
  const S=window._fgm,sIdx=S.sIdx;if(sIdx<0||!S.groups[sIdx])return false;
  const normalized=normalizedMarketCondition(condition);
  if(!normalized||!marketConditionAllowed(S.market,normalized))return false;
  const catalogGroup=(S.catalog||[]).flatMap(section=>section.groups).find(group=>group.key===normalized.key&&group.l===normalized.label);
  if(!catalogGroup||catalogGroup.enabled!==true)return false;
  if(!catalogGroup.hasText&&!catalogGroup.hasRange){
    const catalogItem=catalogGroup.items.find(item=>marketGroupConditionIdentity({key:catalogGroup.key,label:catalogGroup.l,value:item.value})===marketGroupConditionIdentity(normalized));
    if(!catalogItem||catalogItem.enabled===false)return false;
  }
  const identity=marketGroupConditionIdentity(normalized);
  const conditions=_fgmActiveSubgroup();
  if(!conditions)return false;
  if(conditions.some(item=>marketGroupConditionIdentity(item)===identity))return false;
  conditions.push(normalized);return true;
}
function _fgmToggleCatalog(sectionIndex,groupIndex,itemIndex,label){
  const group=filterGroupCatalog()[sectionIndex]?.groups[groupIndex],item=group&&group.items[itemIndex],S=window._fgm;
  if(!item||group.enabled!==true||item.enabled===false||S.sIdx<0||!S.groups[S.sIdx])return;
  const condition={key:group.key,label:group.l,value:item.value},identity=marketGroupConditionIdentity(condition),conditions=_fgmActiveSubgroup();
  if(!conditions)return;
  const index=conditions.findIndex(entry=>marketGroupConditionIdentity(entry)===identity);
  if(index>=0)conditions.splice(index,1);else _fgmAddStructuredCondition(condition);
  _fgmRenderLeft();if(label)_fgmSyncCheckState();
}
function _fgmAddCatalogValue(sectionIndex,groupIndex,controlId){
  const group=filterGroupCatalog()[sectionIndex]?.groups[groupIndex],control=document.getElementById(controlId);
  if(!group||group.enabled!==true||!control)return;
  let value;
  if(control.tagName==='SELECT'){
    const item=group.items[Number(control.value)];
    if(!item||item.enabled===false)return;
    value=item.value;
  }else value=control.value.trim();
  if(value==null||value===''){showToast('error','请输入或选择'+group.l);return;}
  if(_fgmAddStructuredCondition({key:group.key,label:group.l,value})){control.value='';_fgmRenderLeft();showToast('success','已添加'+group.l);}
}
function _fgmAddCatalogRange(sectionIndex,groupIndex,prefix){
  const group=filterGroupCatalog()[sectionIndex]?.groups[groupIndex],minEl=document.getElementById(prefix+'Min'),maxEl=document.getElementById(prefix+'Max'),unitEl=document.getElementById(prefix+'Unit');
  if(!group||group.enabled!==true||!group.hasRange||!minEl||!maxEl)return;
  const min=minEl.value===''?null:Number(minEl.value),max=maxEl.value===''?null:Number(maxEl.value);
  if(min==null&&max==null){showToast('error','请输入'+group.l+'范围');return;}
  if((min!=null&&!Number.isFinite(min))||(max!=null&&!Number.isFinite(max))||(min!=null&&max!=null&&min>max)){showToast('error',group.l+'范围不合法');return;}
  const factor=group.key==='rt'&&unitEl&&unitEl.value==='M'?1/12:group.key==='rt'&&unitEl&&unitEl.value==='D'?1/365:1;
  const value={min:min==null?null:min*factor,max:max==null?null:max*factor};
  if(_fgmAddStructuredCondition({key:group.key,label:group.l,value})){minEl.value='';maxEl.value='';_fgmRenderLeft();showToast('success','已添加'+group.l+'范围');}
}
// ===== 筛选分组弹窗 =====
function showFilterGroupModal(ctx){
  const market=ctx==='rate'?'rate':'credit';
  const field=market==='rate'?'filterGroupsRate':'filterGroupsCredit';
  const saved=D[field]||[];
  let groups;
  if(saved.length===0){
    groups=[];
  }else{
    groups=JSON.parse(JSON.stringify(saved));
  }
  const catalog=availableMarketFilterGroupData(market);
  const actualSections=new Set(catalog.map(section=>section.sec));
  const expectedSections=MARKET_FILTER_GROUP_SECTIONS[market];
  if([...actualSections].some(section=>!expectedSections.has(section))){
    showToast('error','筛选分组目录与当前市场不一致，请刷新页面后重试');
    return;
  }
  window._fgm={groups,sIdx:saved.length===0?-1:0,sgIdx:0,field,market,matchedText:'',matchedTokens:[],catalog};
  _fgmRender();
}
function _fgmRender(){
  const S=window._fgm,g=S.groups,sIdx=S.sIdx;
  const delSvg='<svg viewBox="0 0 24 24" width="12" height="12" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/></svg>';
  let groupListHtml='';
  g.forEach((gr,i)=>{
    groupListHtml+=`<div class="fbm-group-item${i===sIdx?' active':''}" title="双击重命名" onclick="_fgmGroupClick(${i},event)" ondblclick="event.preventDefault();event.stopPropagation();_fgmRename(${i})"><span class="gname">${escapeQuoteHtml(gr.name)}<span style="color:#7d8590;font-size:10px;margin-left:4px">(${flattenMarketSubgroups(normalizeMarketGroupSubgroups(gr)).length})</span></span><span class="gdel" onclick="event.stopPropagation();_fgmDeleteGroup(${i})">${delSvg}</span></div>`;
  });
  groupListHtml+=`<div class="fbm-group-item" id="fgmNewGroupRow" style="display:none;cursor:default;background:rgba(59,130,246,0.06)"><input id="fgmNewGroupInput" style="flex:1;background:transparent;border:none;color:#e6edf3;font-size:12px;outline:none;padding:0;font-family:inherit" maxlength="10" onblur="_fgmConfirmNewGroup(this)" onkeydown="if(event.key==='Enter'){event.preventDefault();this.blur();}else if(event.key==='Escape'){this.dataset.cancel='1';this.blur();}"></div>`;
  let condListHtml='';
  if(sIdx>=0&&g[sIdx]){
    condListHtml=_fgmCondListHtml();
  }else{
    condListHtml='<div style="text-align:center;padding:16px 0;color:#484f58;font-size:12px">请先选择分组</div>';
  }
  const selectedIdentities=new Set((sIdx>=0&&g[sIdx])?(_fgmActiveSubgroup()||[]).map(marketGroupConditionIdentity):[]);
  const matchedTokens=S.matchedTokens||[];
  let condCheckHeader='';
  if(matchedTokens.length>0){
    condCheckHeader='<div style="padding:8px 12px 6px;font-size:11px;color:#7d8590;font-weight:600;border-bottom:1px solid #30363d;background:#0d1117">识别结果：<div style="display:flex;flex-wrap:wrap;gap:4px;margin-top:4px">';
    matchedTokens.forEach(t=>{
      const capability=bondFilterCapabilities.byLabel.get(t.label),condition={key:t.key||capability&&capability.key,label:t.label,value:t.value};
      const identity=marketGroupConditionIdentity(condition),isSel=selectedIdentities.has(identity);
      condCheckHeader+=`<span class="fbm-check${isSel?' checked':''}" data-condition-id="${escapeQuoteHtml(identity)}">${escapeQuoteHtml(t.value)}</span>`;
    });
    condCheckHeader+='</div></div>';
  }
  let condCheckHtml=condCheckHeader;
  S.catalog.forEach((sec,sectionIndex)=>{
    condCheckHtml+=`<div class="fbm-cond-section"><div class="fbm-cond-label" style="font-size:11px;font-weight:700;color:#58a6ff;border-bottom:1px solid #21262d;padding-bottom:3px;margin:10px 0 6px">${sec.sec}</div>`;
    sec.groups.forEach((fg,groupIndex)=>{
      const groupDisabled=fg.enabled!==true,disabledTitle=groupDisabled?` title="${escapeQuoteHtml(fg.disabledReason||'当前筛选条件不可用')}"`:'';
      condCheckHtml+=`<div class="fgm-condition-group${groupDisabled?' disabled':''}"${disabledTitle}><div style="font-size:11px;color:#7d8590;margin-bottom:4px;font-weight:600">${fg.l}</div><div class="fbm-cond-items">`;
      (fg.type==='select'?[]:fg.items).forEach((item,itemIndex)=>{
        const itemEnabled=!groupDisabled&&item.enabled!==false,condition={key:fg.key,label:fg.l,value:item.value},identity=itemEnabled?marketGroupConditionIdentity(condition):'',isSel=itemEnabled&&selectedIdentities.has(identity);
        condCheckHtml+=`<label class="fbm-check${isSel?' checked':''}${itemEnabled?'':' disabled'}"${itemEnabled?` data-condition-id="${escapeQuoteHtml(identity)}" onclick="event.preventDefault();_fgmToggleCatalog(${sectionIndex},${groupIndex},${itemIndex},this)"`:' aria-disabled="true"'}><input type="checkbox"${isSel?' checked':''}${itemEnabled?'':' disabled'}>${escapeQuoteHtml(item.label)}</label>`;
      });
      const controlId='fgmCustom'+sectionIndex+'_'+groupIndex;
      if(fg.hasText)condCheckHtml+=`<span class="fbm-custom-condition"><input id="${controlId}" class="tb-input" placeholder="输入${escapeQuoteHtml(fg.l)}"${groupDisabled?' disabled':''}><button class="tb-btn tb-btn-primary" type="button" onclick="_fgmAddCatalogValue(${sectionIndex},${groupIndex},'${controlId}')"${groupDisabled?' disabled':''}>添加</button></span>`;
      if(fg.type==='select'&&fg.items.length)condCheckHtml+=`<span class="fbm-custom-condition"><select id="${controlId}Select" class="filter-select"${groupDisabled?' disabled':''}><option value="">请选择</option>${fg.items.map((item,index)=>`<option value="${index}"${item.enabled===false?' disabled':''}>${escapeQuoteHtml(item.label)}</option>`).join('')}</select><button class="tb-btn tb-btn-primary" type="button" onclick="_fgmAddCatalogValue(${sectionIndex},${groupIndex},'${controlId}Select')"${groupDisabled?' disabled':''}>添加</button></span>`;
      if(fg.hasRange){const prefix=controlId+'Range';condCheckHtml+=`<span class="fbm-custom-condition"><input id="${prefix}Min" class="tb-input" type="number" placeholder="最小"${groupDisabled?' disabled':''}><span>-</span><input id="${prefix}Max" class="tb-input" type="number" placeholder="最大"${groupDisabled?' disabled':''}>${fg.key==='rt'?`<select id="${prefix}Unit" class="filter-select"${groupDisabled?' disabled':''}><option>Y</option><option>M</option><option>D</option></select>`:''}<button class="tb-btn tb-btn-primary" type="button" onclick="_fgmAddCatalogRange(${sectionIndex},${groupIndex},'${prefix}')"${groupDisabled?' disabled':''}>添加</button></span>`;}
      condCheckHtml+='</div></div>';
    });
    condCheckHtml+='</div>';
  });
  const html=`<div class="fbm-layout">
    <div id="fgmLeft" class="fbm-left" style="width:260px;display:flex;flex-direction:column">
      <div style="flex:0 0 50%;display:flex;flex-direction:column;overflow:hidden">
        <div class="fbm-group-list" style="flex:1;overflow-y:auto;padding:2px 0">${groupListHtml}</div>
        <div style="padding:6px 8px"><div style="display:flex;align-items:center;justify-content:center;padding:5px 0;font-size:11px;color:#58a6ff;cursor:pointer;border:1px dashed rgba(59,130,246,0.3);border-radius:4px;background:rgba(59,130,246,0.05)" onclick="_fgmStartNewGroup()">+ 新建</div></div>
      </div>
      <div class="fgm-selected-pane">${condListHtml}</div>
    </div>
    <div class="fbm-right" style="display:flex;flex-direction:column">
      <div style="flex:0 0 auto;position:relative;background:#0d1117;border-bottom:1px solid #30363d;padding-bottom:10px">
        <textarea id="fgmInput" style="width:100%;height:150px;resize:none;font-size:12px;font-family:'JetBrains Mono',monospace;background:#0d1117;border:none;border-radius:0;padding:10px 120px 10px 12px;color:#e6edf3;outline:none" placeholder="粘贴筛选条件文本，自动识别匹配条件&#10;例：城投 AAA T+0 1-3Y" onkeydown="if(event.key==='Enter'&&!event.shiftKey){event.preventDefault();_fgmMatch();}"></textarea>
        <div style="position:absolute;right:12px;bottom:10px;display:flex;gap:6px"><div style="display:flex;align-items:center;justify-content:center;padding:4px 14px;font-size:11px;font-weight:600;border:1px solid #30363d;border-radius:4px;background:#21262d;color:#8b949e;cursor:pointer" onclick="_fgmClear()">清空</div><div id="fgmIdentifyBtn" style="display:flex;align-items:center;justify-content:center;padding:4px 14px;font-size:11px;font-weight:600;border:1px solid rgba(59,130,246,0.3);border-radius:4px;background:rgba(59,130,246,0.1);color:#58a6ff;cursor:pointer" onclick="_fgmMatch()">识别</div></div>
      </div>
      <div class="fbm-right-body" style="flex:1;overflow-y:auto;padding:0 5px 0 12px">${condCheckHtml}</div>
    </div>
  </div>`;
  showModal((S.market==='rate'?'利率':'信用')+'筛选分组管理',html,{wide:true,fill:true});
  document.getElementById('modalFooter').innerHTML='<button class="tb-btn tb-btn-default" onclick="closeModal()">取消</button><button class="tb-btn tb-btn-primary" style="background:#3b82f6;color:#fff;border:none" onclick="_fgmSave()">保存</button>';
}
function _fgmRenderLeft(){
  const S=window._fgm,g=S.groups,sIdx=S.sIdx;
  const delSvg='<svg viewBox="0 0 24 24" width="12" height="12" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/></svg>';
  let groupListHtml='';
  g.forEach((gr,i)=>{
    groupListHtml+=`<div class="fbm-group-item${i===sIdx?' active':''}" title="双击重命名" onclick="_fgmGroupClick(${i},event)" ondblclick="event.preventDefault();event.stopPropagation();_fgmRename(${i})"><span class="gname">${escapeQuoteHtml(gr.name)}<span style="color:#7d8590;font-size:10px;margin-left:4px">(${flattenMarketSubgroups(normalizeMarketGroupSubgroups(gr)).length})</span></span><span class="gdel" onclick="event.stopPropagation();_fgmDeleteGroup(${i})">${delSvg}</span></div>`;
  });
  groupListHtml+=`<div class="fbm-group-item" id="fgmNewGroupRow" style="display:none;cursor:default;background:rgba(59,130,246,0.06)"><input id="fgmNewGroupInput" style="flex:1;background:transparent;border:none;color:#e6edf3;font-size:12px;outline:none;padding:0;font-family:inherit" maxlength="10" onblur="_fgmConfirmNewGroup(this)" onkeydown="if(event.key==='Enter'){event.preventDefault();this.blur();}else if(event.key==='Escape'){this.dataset.cancel='1';this.blur();}"></div>`;
  let condListHtml='';
  if(sIdx>=0&&g[sIdx]){
    condListHtml=_fgmCondListHtml();
  }else{
    condListHtml='<div style="text-align:center;padding:16px 0;color:#484f58;font-size:12px">请先选择分组</div>';
  }
  const leftEl=document.getElementById('fgmLeft');
  if(leftEl){
    leftEl.children[0].querySelector('.fbm-group-list').innerHTML=groupListHtml;
    leftEl.children[1].innerHTML=condListHtml;
    _fgmSyncCheckState();
  }
}
function _fgmSyncCheckState(){
  const S=window._fgm,sIdx=S.sIdx,g=S.groups;
  const selConds=new Set((sIdx>=0&&g[sIdx])?(_fgmActiveSubgroup()||[]).map(marketGroupConditionIdentity):[]);
  document.querySelectorAll('.fbm-right-body .fbm-check').forEach(el=>{
    const full=el.dataset.conditionId||'';
    const isSel=selConds.has(full);
    if(isSel){el.classList.add('checked');}else{el.classList.remove('checked');}
    const cb=el.querySelector('input[type=checkbox]');
    if(cb)cb.checked=isSel;
  });
}
function _fgmGroupClick(idx,e){
  if(e)e.stopPropagation();
  const S=window._fgm;
  S.sIdx=idx;_fgmRenderLeft();
}
function _fgmStartNewGroup(){
  const row=document.getElementById('fgmNewGroupRow');
  const input=document.getElementById('fgmNewGroupInput');
  if(!row||!input)return;
  row.style.display='flex';
  input.value='';
  input.dataset.cancel='';
  input.focus();
}
function _fgmConfirmNewGroup(input){
  if(input.dataset.cancel==='1'){document.getElementById('fgmNewGroupRow').style.display='none';return;}
  const val=input.value.trim();
  if(val){
    if(window._fgm.groups.some(group=>group.name.toLocaleLowerCase()===val.toLocaleLowerCase())){showToast('error','分组名称不能重复');_fgmRenderLeft();return;}
    window._fgm.groups.push({id:newLocalMarketGroupId(),name:val,conditions:[],subgroups:[[]]});
    window._fgm.sIdx=window._fgm.groups.length-1;
  }
  _fgmRenderLeft();
}
function _fgmDeleteGroup(idx){
  const g=window._fgm.groups;
  if(flattenMarketSubgroups(normalizeMarketGroupSubgroups(g[idx])).length>0){
    _fbmConfirm('分组中有已选条件，是否确认删除？',()=>{
      g.splice(idx,1);
      if(window._fgm.sIdx>=g.length)window._fgm.sIdx=g.length-1;
      _fgmRenderLeft();
    });
  }else{
    g.splice(idx,1);
    if(window._fgm.sIdx>=g.length)window._fgm.sIdx=g.length-1;
    _fgmRenderLeft();
  }
}
function _fgmRename(idx){
  const g=window._fgm.groups;
  const items=document.querySelectorAll('#fgmLeft .fbm-group-item');
  const el=items[idx];
  if(!el)return;
  const nameEl=el.querySelector('.gname');
  const oldName=g[idx].name;
  const inp=document.createElement('input');
  inp.maxLength=30;inp.value=oldName;
  inp.style.cssText='background:transparent;border:none;color:#e6edf3;font-size:12px;outline:none;padding:0;width:120px;font-family:inherit';
  inp.addEventListener('blur',()=>_fgmConfirmRename(idx,inp));
  inp.addEventListener('keydown',event=>{if(event.key==='Enter'){event.preventDefault();inp.blur();}else if(event.key==='Escape'){inp.value=oldName;inp.blur();}});
  nameEl.replaceChildren(inp);
  inp.focus();inp.select();
}
function _fgmConfirmRename(idx,input){
  const val=input.value.trim();
  if(val&&val!==window._fgm.groups[idx].name){
    if(window._fgm.groups.some((group,groupIdx)=>groupIdx!==idx&&group.name.toLocaleLowerCase()===val.toLocaleLowerCase()))showToast('error','分组名称不能重复');
    else window._fgm.groups[idx].name=val;
  }
  _fgmRenderLeft();
}
function _fgmToggleCond(full,label){
  const S=window._fgm,sIdx=S.sIdx;
  if(sIdx<0||!S.groups[sIdx])return;
  const conds=_fgmActiveSubgroup();
  if(!conds)return;
  const idx=conds.findIndex(condition=>_fgmConditionFull(condition)===full);
  if(idx>=0){conds.splice(idx,1);label.classList.remove('checked');const cb=label.querySelector('input[type=checkbox]');if(cb)cb.checked=false;}
  else{const condition=_fgmConditionFromFull(full);if(condition)conds.push(condition);label.classList.add('checked');const cb=label.querySelector('input[type=checkbox]');if(cb)cb.checked=true;}
  _fgmRenderLeft();
}
function _fgmMatch(){
  const input=document.getElementById('fgmInput');
  const text=(input?input.value:'').trim();
  if(!text){showToast('error','请输入筛选条件文本');return;}
  const S=window._fgm,sIdx=S.sIdx;
  if(sIdx<0||!S.groups[sIdx]){showToast('error','请先选择左侧分组');return;}
  const allowed=new Set(availableMarketFilterGroupData(S.market).flatMap(section=>section.groups.flatMap(group=>group.items.map(item=>marketGroupConditionIdentity({key:group.key,label:group.l,value:item.value})))));
  const matchedConds=_smartMatchConditions(text).filter(condition=>{
    const capability=bondFilterCapabilities.byLabel.get(condition.label);
    return capability&&capability.enabled===true&&allowed.has(marketGroupConditionIdentity({key:capability.key,label:condition.label,value:condition.value}))&&marketConditionAllowed(S.market,condition);
  });
  const conditions=_fgmActiveSubgroup();
  if(!conditions){showToast('error','请先选择左侧分组');return;}
  let added=0;
  matchedConds.forEach(c=>{
    const full=c.label+':'+c.value;
    if(!conditions.some(condition=>_fgmConditionFull(condition)===full)){
      const normalized=_fgmConditionFromFull(full);if(!normalized)return;
      conditions.push(normalized);
      added++;
    }
  });
  S.matchedTokens=matchedConds;
  window._fgm.matchedText=matchedConds.map(c=>c.value).join('、');
  _fgmRender();
  if(added>0)showToast('success','已识别 '+added+' 个条件');
  else showToast('info','未匹配到条件');
}
function _fgmClear(){
  const input=document.getElementById('fgmInput');
  if(input)input.value='';
  window._fgm.matchedTokens=[];
  window._fgm.matchedText='';
  document.querySelectorAll('.fbm-right-body .fbm-check.checked').forEach(el=>{
    el.classList.remove('checked');
    const cb=el.querySelector('input[type=checkbox]');if(cb)cb.checked=false;
  });
  _fgmRender();
}
async function _fgmSave(){
  const field=window._fgm.field||'filterGroupsCredit';
  const previous=D[field];
  D[field]=window._fgm.groups.map(group=>normalizedRemoteMarketGroup({...group,kind:'filter',market:window._fgm.market}));
  const btn=document.querySelector('#modalFooter .tb-btn-primary');if(btn){btn.disabled=true;btn.textContent='保存中…';}
  try{
    await saveMarketGroupState();
    // P0: 保存成功立即关窗；标记受影响分组缓存为未就绪，点击时再查
    invalidateFilterGroupCaches();
    _updateSidebarFilterGroups();renderFilteredViews();closeModal();showToast('success','筛选分组已保存');
  }catch(error){
    D[field]=previous;_updateSidebarFilterGroups();showToast('error',error.message||'保存失败');
  }finally{if(btn){btn.disabled=false;btn.textContent='保存';}
  }
}

// ===== 行情中心右键与报价弹窗 =====
const BOARD_DATA_MAP={credit:'boardCreditAll',rate:'boardRateAll',creditBest:'boardCreditBest',rateBest:'boardRateBest',alert:'boardAlert'};
let ctxTarget=null;
let quoteModalState=null;
function boardDataByType(type){const key=BOARD_DATA_MAP[type];return key?D[key]:null;}
function escapeQuoteHtml(value){return String(value==null?'':value).replace(/[&<>"']/g,ch=>({'&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;',"'":'&#39;'}[ch]));}
function boardCellColId(cell){
  if(!cell)return'';
  return cell.getAttribute('data-col')||'';
}
function quoteIdFromBoardCell(row,cellOrIndex){
  if(!row)return'';
  let colId='';
  if(cellOrIndex&&typeof cellOrIndex==='object'&&cellOrIndex.nodeType){colId=boardCellColId(cellOrIndex);}
  else if(typeof cellOrIndex==='number'&&cellOrIndex>=0){
    // legacy numeric index fallback for best/alert default order
    if(cellOrIndex>=7&&cellOrIndex<=10&&row.ofrQuoteId)return row.ofrQuoteId;
    if(cellOrIndex>=3&&cellOrIndex<=6&&row.bidQuoteId)return row.bidQuoteId;
  }
  if(['instOfr','trOfr','bvOfr','op'].includes(colId)&&row.ofrQuoteId)return row.ofrQuoteId;
  if(['instBid','trBid','bvBid','bp'].includes(colId)&&row.bidQuoteId)return row.bidQuoteId;
  return row.sourceQuoteId||row.bidQuoteId||row.ofrQuoteId||row.quoteId||'';
}
function syncCtxQuoteFlagItems(row,quoteId){
  const quote=quoteById(quoteId),manageable=!!(authToken&&quote&&!row.unquoted&&canManageQuote(quote));
  [['ctxPackItem','packing'],['ctxOcoItem','oco']].forEach(([id,field])=>{
    const item=document.getElementById(id);if(!item)return;
    item.classList.toggle('active',!!(quote&&quote[field]));item.classList.toggle('disabled',!manageable);
    item.title=manageable?(quote&&quote[field]?'再次点击取消该标签':'直接更新该报价'):(row.unquoted?'未报价债券不能设置标签':'只能更新自己的报价');
  });
}
function showCtxMenu(e,rowIdx,dataType){
  e.preventDefault();
  const data=boardDataByType(dataType);
  if(!data||!data[rowIdx])return;
  const cell=e.target.closest('td');
  ctxTarget={idx:rowIdx,type:dataType,quoteId:quoteIdFromBoardCell(data[rowIdx],cell)};
  syncCtxQuoteFlagItems(data[rowIdx],ctxTarget.quoteId);
  renderCtxFavoriteGroupMenu(data[rowIdx]);
  const m=document.getElementById('ctxMenu');
  m.style.left=Math.min(e.clientX,window.innerWidth-200)+'px';
  m.style.top=Math.min(e.clientY,window.innerHeight-260)+'px';
  m.classList.add('show');
}
function hideCtx(){document.getElementById('ctxMenu').classList.remove('show');ctxTarget=null;}
document.addEventListener('click',hideCtx);
function ctxEdit(){
  if(!ctxTarget)return;
  const target={...ctxTarget};hideCtx();showEditQuoteModal(target.idx,target.type,target.quoteId,'context-menu');
}
function ctxFirm(){if(!ctxTarget)return;hideCtx();}
function ctxDelete(){if(!ctxTarget)return;hideCtx();}
async function ctxCopy(){
  if(!ctxTarget)return;
  const target={...ctxTarget},data=boardDataByType(target.type),source=data&&data[target.idx];
  const quote=source&&(quoteById(target.quoteId)||source);hideCtx();
  if(!quote){showToast('error','未找到需要复制的报价');return;}
  const enrichMap=await fetchCopyEnrichMap([quote]);
  try{
    const copied=await writeBoardClipboard(quoteCopyText(quote,enrichMap[String(quote.c||'').toUpperCase()]||{}));
    if(copied)showToast('success','已复制 1 条报价');
  }catch(error){showToast('error',error.message||'复制失败');}
}
function ctxSendToClient(){if(!ctxTarget)return;hideCtx();}
function ctxFollowBest(){if(!ctxTarget)return;hideCtx();}
function shanghaiDateParts(date){const parts=new Intl.DateTimeFormat('en-CA',{timeZone:'Asia/Shanghai',year:'numeric',month:'2-digit',day:'2-digit',hour:'2-digit',minute:'2-digit',second:'2-digit',hourCycle:'h23'}).formatToParts(date||new Date()).reduce((out,item)=>(out[item.type]=item.value,out),{});return parts;}
function quoteToday(){if(platformTradeDate)return platformTradeDate;const p=shanghaiDateParts(new Date());return p.year+'-'+p.month+'-'+p.day;}
function quoteNow(){const p=shanghaiDateParts(new Date());return quoteToday()+' '+p.hour+':'+p.minute+':'+p.second;}
function quoteDirectionOf(r){
  if(typeof window!=='undefined'&&window.FICCQuoteUtils)return window.FICCQuoteUtils.directionOf(r);
  const declared=String(r&&r.direction||'').toLowerCase();
  if(declared==='bid'||declared==='ofr')return declared;
  return r&&r.bp?'bid':r&&r.op?'ofr':'bid';
}
function renderQuoteDirection(r){
  if(r&&r.unquoted)return'<span class="muted">—</span>';
  const direction=String(r&&r.direction||quoteDirectionOf(r)).toLowerCase()==='ofr'?'ofr':'bid';
  return`<span class="board-direction-value">${direction==='ofr'?'Ofr':'Bid'}</span>`;
}
function quoteOperationLabels(r){
  const labels=[];
  if(r&&r.oco)labels.push('OCO');
  if(r&&r.packing)labels.push('打包');
  if(r&&r.urgent)labels.push('急');
  if(r&&(r.recommend||r.rec))labels.push('荐');
  if(r&&(r.round||r.whole))labels.push('整');
  return labels;
}
function renderQuoteOperationTags(r){
  const labels=quoteOperationLabels(r);
  return labels.length?labels.map(label=>renderQuoteFlag(label,'margin-right:3px;font-size:11px')).join(''):'<span class="muted">—</span>';
}
function renderInlineQuoteTags(r){
  const icons=[];if(r&&r.oco)icons.push(renderInlineQuoteOperationIcon('oco'));if(r&&r.packing)icons.push(renderInlineQuoteOperationIcon('packing'));
  const labels=[];if(r&&r.urgent)labels.push('急');if(r&&(r.recommend||r.rec))labels.push('荐');if(r&&(r.round||r.whole))labels.push('整');
  const content=icons.join('')+labels.map(label=>renderQuoteFlag(label,'margin-left:3px;font-size:10px')).join('');
  return content?' <span class="quote-inline-tags">'+content+'</span>':'';
}
function renderInlineQuoteOperationIcon(field){
  if(field==='oco')return'<span class="quote-inline-operation-icon" role="img" aria-label="OCO" title="OCO"><svg viewBox="0 0 24 24"><circle cx="9" cy="12" r="5"/><circle cx="15" cy="12" r="5"/></svg></span>';
  return'<span class="quote-inline-operation-icon" role="img" aria-label="打包" title="打包"><svg viewBox="0 0 24 24"><path d="M4 7l8-4 8 4v10l-8 4-8-4z"/><path d="M4 7l8 4 8-4M12 11v10"/></svg></span>';
}
function renderQuoteFlag(label,style){const classes={'急':'quote-flag-urgent','荐':'quote-flag-recommend','整':'quote-flag-whole','OCO':'quote-flag-oco','打包':'quote-flag-packing'};return'<span class="quote-flag '+(classes[label]||'')+'" style="'+(style||'')+'">'+label+'</span>';}
function quoteForRenderedSide(r,side){
  const quoteId=side==='bid'?r&&r.bidQuoteId:r&&r.ofrQuoteId;
  return quoteId?(quoteById(quoteId)||r):r;
}
// ===== 价格类型统一标识（净/全/到/行）：含权债默认行权、非含权债默认到期，仅非默认口径加标识 =====
let bondCallableSet=null;
async function loadBondOptionMap(){
  try{
    const response=await authFetch(API_BASE+'/bonds/option-map');
    const data=await response.json().catch(()=>({}));
    if(response.ok&&data.success&&Array.isArray(data.callableCodes)){bondCallableSet=new Set(data.callableCodes.map(normalizeBondCode));return true;}
  }catch(e){}
  bondCallableSet=null;
  return false;
}
function bondCallableStatus(code){
  if(!bondCallableSet||!code)return null;
  const key=normalizeBondCode(code);
  if(!key)return null;
  return bondCallableSet.has(key)?'callable':'plain';
}
function priceTypeMarkHtml(quote){
  const priceType=String((quote&&quote.priceType)||'').trim();
  if(priceType==='净价')return '<span class="pt-mark pt-mark-net">净</span>';
  if(priceType==='全价')return '<span class="pt-mark pt-mark-full">全</span>';
  if(!priceType.includes('收益率'))return '';
  const callable=bondCallableStatus(quote&&quote.c);
  if(callable===null)return '';
  if(priceType.includes('行权'))return callable==='plain'?'<span class="pt-mark pt-mark-ytc">行</span>':'';
  return callable==='callable'?'<span class="pt-mark pt-mark-ytm">到</span>':'';
}
function renderQuotePriceSpan(quote,value,side,extraClass,placement){
  const direction=side==='ofr'?'ofr':'bid',quoteId=quote&&(quote.quoteId||quote.sourceQuoteId)||'';
  const color=direction==='ofr'?'#3b82f6':'#f59e0b',classes='board-quote-price '+(extraClass||''),placementMode=placement==='left'?'left':'side';
  return'<span class="'+classes.trim()+'" data-quote-id="'+escapeQuoteHtml(quoteId)+'" data-side="'+direction+'" data-tooltip-placement="'+placementMode+'" style="color:'+color+';font-weight:700">'+escapeQuoteHtml(value)+'</span>';
}
function renderBoardQuoteValue(r,side,placement){
  if(!r||r.unquoted)return'<span class="muted">—</span>';
  const direction=side||quoteDirectionOf(r),value=direction==='ofr'?r.op:r.bp;
  const sideQuote=quoteForRenderedSide(r,direction),price=value?renderQuotePriceSpan(sideQuote,value,direction,'',placement):'<span class="muted">—</span>';
  return price+renderInlineQuoteTags(sideQuote);
}
const quotePriceConversionCache=new Map();
let quotePriceHoverTimer=null,quotePriceHoverTarget=null,quotePriceHoverSequence=0,quotePriceTooltip=null;
function ensureQuotePriceTooltip(){
  if(!quotePriceTooltip){quotePriceTooltip=document.createElement('div');quotePriceTooltip.className='quote-price-tooltip';document.body.appendChild(quotePriceTooltip);}
  return quotePriceTooltip;
}
function quotePriceTargetData(target){
  const quote=quoteById(target&&target.dataset.quoteId),side=target&&target.dataset.side==='ofr'?'ofr':'bid',placement=target&&target.dataset.tooltipPlacement==='left'?'left':'side';
  if(!quote)return null;
  const values=quoteSideValues(quote,side);
  return{quote,side,placement,value:values.price};
}
function quotePriceConversionKey(data){
  const quote=data.quote;
  return[quote.quoteId,data.side,data.value,quote.priceType||'',quote.tradeDate||'',quote.cs||'',quote.updatedAt||quote.t||''].join('|');
}
function loadQuotePriceConversion(data){
  const key=quotePriceConversionKey(data);
  if(quotePriceConversionCache.has(key))return quotePriceConversionCache.get(key);
  const task=authFetch(API_BASE+'/quotes/price-conversion',{method:'POST',body:JSON.stringify({securityId:data.quote.c,quoteValue:data.value,priceType:data.quote.priceType||'到期收益率',direction:data.side,tradeDate:data.quote.tradeDate||'',settlementType:data.quote.cs||''})})
    .then(async response=>{const body=await response.json().catch(()=>({}));if(!response.ok)throw new Error(typeof body.detail==='string'?body.detail:'价格换算服务不可用');return body;})
    .catch(error=>({success:false,yieldType:String(data.quote.priceType||'').includes('行权')?'YTC':'YTM',yieldLabel:String(data.quote.priceType||'').includes('行权')?'行权收益率':'到期收益率',yieldValue:String(data.quote.priceType||'').includes('收益率')?Number(data.value):null,cleanPrice:data.quote.priceType==='净价'?Number(data.value):null,fullPrice:data.quote.priceType==='全价'?Number(data.value):null,calculationMode:'unavailable',message:error.message||'价格换算服务不可用'}));
  quotePriceConversionCache.set(key,task);return task;
}
function quoteTooltipNumber(value,suffix){
  if(value===null||value===undefined||String(value).trim()==='')return'--';
  const number=Number(value);return Number.isFinite(number)?number.toFixed(4)+(suffix||''):'--';
}
function quotePriceTooltipHtml(data,result,loading){
  const yieldLabel=result&&result.yieldLabel||(String(data.quote.priceType||'').includes('行权')?'行权收益率':'到期收益率');
  if(loading)return'<div class="quote-price-tooltip-loading">换算中...</div>';
  const hasFull=result&&result.fullPrice!==null&&result.fullPrice!==undefined&&String(result.fullPrice).trim()!=='';
  const rows=[[yieldLabel,quoteTooltipNumber(result&&result.yieldValue,'%')],hasFull?['全价',quoteTooltipNumber(result.fullPrice)]:['净价',quoteTooltipNumber(result&&result.cleanPrice)]];
  return rows.map(row=>'<div class="quote-price-tooltip-row"><span class="quote-price-tooltip-label">'+escapeQuoteHtml(row[0])+'</span><span class="quote-price-tooltip-value '+data.side+'">'+escapeQuoteHtml(row[1])+'</span></div>').join('');
}
function positionQuotePriceTooltip(target,side,placement){
  const tooltip=ensureQuotePriceTooltip(),rect=target.getBoundingClientRect(),gap=8,width=tooltip.offsetWidth,height=tooltip.offsetHeight;
  const positionSide=placement==='left'?'left':side==='ofr'?'right':'left',targetHeight=rect.height||Math.max(0,rect.bottom-rect.top);
  let left=positionSide==='right'?rect.right+gap:rect.left-width-gap,top=rect.top+(targetHeight-height)/2;
  left=Math.min(Math.max(8,left),Math.max(8,window.innerWidth-width-8));
  top=Math.min(Math.max(8,top),Math.max(8,window.innerHeight-height-8));
  tooltip.style.left=left+'px';tooltip.style.top=top+'px';
}
function showQuotePriceTooltip(target){
  const data=quotePriceTargetData(target);if(!data||data.value==='')return;
  quotePriceHoverTarget=target;const sequence=++quotePriceHoverSequence,tooltip=ensureQuotePriceTooltip();
  tooltip.innerHTML=quotePriceTooltipHtml(data,null,true);tooltip.classList.add('show');positionQuotePriceTooltip(target,data.side,data.placement);
  loadQuotePriceConversion(data).then(result=>{
    if(sequence!==quotePriceHoverSequence||quotePriceHoverTarget!==target||!document.body.contains(target))return;
    tooltip.innerHTML=quotePriceTooltipHtml(data,result,false);positionQuotePriceTooltip(target,data.side,data.placement);
  });
}
function hideQuotePriceTooltip(){
  clearTimeout(quotePriceHoverTimer);quotePriceHoverTarget=null;quotePriceHoverSequence+=1;
  if(quotePriceTooltip)quotePriceTooltip.classList.remove('show');
}
document.addEventListener('pointerover',event=>{
  const target=event.target.closest&&event.target.closest('.board-quote-price');if(!target||target.contains(event.relatedTarget))return;
  clearTimeout(quotePriceHoverTimer);quotePriceHoverTimer=setTimeout(()=>showQuotePriceTooltip(target),150);
});
document.addEventListener('pointerout',event=>{
  const target=event.target.closest&&event.target.closest('.board-quote-price');if(!target||target.contains(event.relatedTarget))return;
  hideQuotePriceTooltip();
});
window.addEventListener('scroll',hideQuotePriceTooltip,true);window.addEventListener('resize',hideQuotePriceTooltip);
document.addEventListener('keydown',event=>{if(event.key==='Escape')hideQuotePriceTooltip();});
function quotePriceLabel(quote){
  const priceType=String(quote.priceType||'到期收益率');
  if(priceType.includes('收益率'))return{label:priceType==='行权收益率'?'行权YTM':'YTM',unit:'%'};
  if(priceType==='全价')return{label:'全价',unit:'元'};
  return{label:'净价',unit:'元'};
}
function quoteCopyText(quote,enrich){
  enrich=enrich||{};
  const isRate=(quote.market==='rate')||Number(enrich.bondCategory)===1||Number(quote.bondCategory)===1;
  const isCredit=!isRate;
  // 剩余期限（优先DM含休X标注）
  let tenor=enrich.remainingTenor||quote.m||'--';
  if(!/[\(（].*休.*[\)）]/.test(tenor)){const cs=(quote.cs||'').toUpperCase();if(cs==='T+1')tenor+='(休1)';else if(cs==='T+2')tenor+='(休2)';}
  // 报价 Bid/Ofr
  const bp=quote.bp||'--',op=quote.op||'--';
  // 报量 + nonfirm(*)
  const bvBid=quote.bvBid||quote.bv||'--',bvOfr=quote.bvOfr||quote.bv||'--';
  const volStr=bvBid+'(*)/'+bvOfr+'(*)';
  // 票面利率
  const coupon=enrich.coupon?'票面'+enrich.coupon+'%':'票面--';
  // 评级
  const rating=isCredit?(quote.r||'--')+'/'+(quote.br||'--'):'--/--';
  // 中债估值（信用债:到期/行权，利率债:仅到期）
  const cbYtm=enrich.cbYtm||(quote.nzYTM&&quote.nzYTM!=='—'?quote.nzYTM:'--');
  const valuation=isCredit?'中债'+cbYtm+'/'+(enrich.cbExYtm||'--'):'中债'+cbYtm;
  // 到期日
  const maturity=enrich.maturity?'到期日'+enrich.maturity:'到期日--';
  // 操作标签（仅整/OCO/打包）
  const flags=[];
  if(quote.round||quote.whole)flags.push('整');if(quote.oco)flags.push('OCO');if(quote.packing)flags.push('打包');
  // 组装（双空格分隔，不含报价机构）
  const parts=[tenor,quote.c||'',quote.n||''];
  if(isCredit&&enrich.issueType)parts.push(enrich.issueType);
  parts.push(bp+'/'+op,volStr,coupon,rating,valuation,maturity);
  if(flags.length)parts.push(flags.join(' '));
  return parts.filter(Boolean).join('  ');
}
async function fetchCopyEnrichMap(quotes){
  const codes=[...new Set(quotes.map(q=>String(q.c||'').toUpperCase()).filter(Boolean))];
  if(!codes.length)return{};
  try{
    const resp=await authFetch(API_BASE+'/bond-copy-enrich',{method:'POST',headers:{'Content-Type':'application/json'},body:JSON.stringify({codes})});
    const d=await resp.json();
    if(d.ok&&d.data)return d.data;
  }catch(e){/* 富化失败降级 */}
  return{};
}
async function writeBoardClipboard(text){
  if(window.desktopAPI&&window.desktopAPI.writeClipboardText){await window.desktopAPI.writeClipboardText(text);return true;}
  const clipboard=globalThis.navigator&&globalThis.navigator.clipboard;
  if(clipboard&&clipboard.writeText){
    try{await clipboard.writeText(text);return true;}catch(error){/* 局域网 HTTP 或权限受限时继续降级 */}
  }
  if(typeof document!=='undefined'&&document.body){
    const textarea=document.createElement('textarea');
    textarea.value=text;textarea.setAttribute('readonly','');textarea.style.position='fixed';textarea.style.left='-9999px';textarea.style.opacity='0';
    document.body.appendChild(textarea);textarea.focus();textarea.select();
    let copied=false;try{copied=!!document.execCommand&&document.execCommand('copy');}catch(error){copied=false;}textarea.remove();
    if(copied)return true;
    showModal('手工复制','<div style="color:#8b949e;margin-bottom:8px">浏览器未授予剪贴板权限，请在下方文本框中按 Ctrl+C 复制：</div><textarea readonly style="width:100%;height:220px;resize:vertical" onclick="this.select()">'+escapeQuoteHtml(text)+'</textarea>');
    return false;
  }
  throw new Error('当前环境不支持写入剪贴板');
}
async function copySelectedBoardQuotes(key){
  const quotes=selectedBoardQuotes(key);if(!quotes.length){showToast('error','请先选择需要复制的报价');return;}
  const enrichMap=await fetchCopyEnrichMap(quotes);
  try{const copied=await writeBoardClipboard(quotes.map(q=>quoteCopyText(q,enrichMap[String(q.c||'').toUpperCase()]||{})).join('\r\n'));if(copied)showToast('success','已复制 '+quotes.length+' 条报价');}
  catch(error){showToast('error',error.message||'复制失败');}
}
async function copySelectedIntents(){
  const rows=(intentFilterState.filteredRows||[]).filter(r=>intentSelectedIds.has(String(r.id)));
  if(!rows.length){showToast('error','请先选择需要复制的报价');return;}
  const text=rows.map(r=>String(r.content||'').trim()).filter(Boolean).join('\r\n');
  if(!text){showToast('error','选中行没有报价内容');return;}
  try{const copied=await writeBoardClipboard(text);if(copied)showToast('success','已复制 '+rows.length+' 条报价内容');}
  catch(error){showToast('error',error.message||'复制失败');}
}
function intentExportRow(row){
  let recognition='';
  try{recognition=row&&row.recognitionJson?JSON.stringify(JSON.parse(row.recognitionJson)):'';}catch(error){recognition=String(row&&row.recognitionJson||'');}
  return{
    '模糊报价ID':row&&row.id||'',
    '报价内容':row&&row.content||'',
    '识别结果':row&&row.result||'',
    '报价机构':row&&row.inst||'',
    '报价交易员':row&&row.salesTr||'',
    '长城交易员':row&&row.trader||'',
    '报价时间':row&&row.time||'',
    '状态':row&&row.status||'',
    '所有者账号ID':row&&row.ownerAccountId||'',
    '所有者账号':row&&row.ownerUsername||'',
    '创建时间':row&&row.createdAt||'',
    '更新时间':row&&row.updatedAt||'',
    '识别版本':row&&row.recognitionVersion||'',
    '识别状态':row&&row.recognitionStatus||'',
    '结构化识别条件':recognition,
  };
}
async function exportBoardIntents(){
  if(typeof XLSX==='undefined'){showToast('error','Excel 组件未加载');return;}
  const rows=intentFilterState.filteredRows||[];
  if(!rows.length){showToast('error','当前筛选列表没有可导出的数据');return;}
  const button=document.getElementById('boardIntentExportBtn');if(button)button.disabled=true;
  try{
    const exportRows=rows.map(intentExportRow),sheet=XLSX.utils.json_to_sheet(exportRows),headers=Object.keys(exportRows[0]);
    sheet['!autofilter']={ref:'A1:'+XLSX.utils.encode_col(headers.length-1)+'1'};
    sheet['!freeze']={xSplit:0,ySplit:1,topLeftCell:'A2',activePane:'bottomLeft',state:'frozen'};
    sheet['!cols']=headers.map(header=>({wch:Math.min(42,Math.max(10,String(header).length*2,...exportRows.slice(0,200).map(row=>String(row[header]??'').length)))}));
    const workbook=XLSX.utils.book_new();XLSX.utils.book_append_sheet(workbook,sheet,'模糊报价');
    const now=new Date(),stamp=now.getFullYear()+String(now.getMonth()+1).padStart(2,'0')+String(now.getDate()).padStart(2,'0')+'_'+String(now.getHours()).padStart(2,'0')+String(now.getMinutes()).padStart(2,'0')+String(now.getSeconds()).padStart(2,'0'),name='模糊报价_'+stamp+'.xlsx';
    if(window.desktopAPI&&window.desktopAPI.saveXlsxFile){const result=await window.desktopAPI.saveXlsxFile({defaultName:name,base64:workbookBase64(workbook)});if(!result.cancelled)showToast('success','已导出当前筛选全部 '+exportRows.length+' 条模糊报价到 '+result.filePath);}
    else{XLSX.writeFile(workbook,name);showToast('success','已导出当前筛选全部 '+exportRows.length+' 条模糊报价');}
  }catch(error){showToast('error',error.message||'模糊报价导出失败');}
  finally{if(button)button.disabled=false;}
}
async function changeSelectedIntentStatus(action){
  if(!authToken){showToast('error','应急模式为只读，认证服务恢复后才能操作模糊报价');return;}
  const rows=(intentFilterState.filteredRows||[]).filter(row=>intentSelectedIds.has(String(row.id)));
  if(!rows.length){showToast('error','请先选择模糊报价');return;}
  const button=document.getElementById(action==='refer'?'boardIntentReferBtn':'boardIntentUnreferBtn');if(button)button.disabled=true;
  try{
    const response=await fuzzyQuoteFetch('/status',{method:'POST',body:JSON.stringify({items:rows.map(row=>({id:Number(row.id),expectedDataVersion:Number(row.dataVersion||1)})),action})});
    let body={};try{body=await response.json();}catch(error){}
    if(response.status===404)throw new Error('当前后端版本不支持模糊报价撤销/恢复，请关闭旧版平台并重新启动当前客户端');
    if(response.status===409&&body.detail&&String(body.detail.code||'').includes('CONFLICT')){await loadBoardIntents(false);intentSelectedIds.clear();refreshIntentView();throw new Error(body.detail.message||'模糊报价已变化，请按最新状态重新选择');}
    if(!response.ok||!body.success){const detail=body.detail;throw new Error(typeof detail==='string'?detail:(detail&&detail.message)||'模糊报价状态更新失败');}
    const skipped=Array.isArray(body.skipped)?body.skipped:[],updated=Number(body.updated||0),details=[];
    if(updated)details.push('已'+(action==='refer'?'撤销':'恢复')+' '+updated+' 条模糊报价');
    if(skipped.length)details.push('已保留 '+skipped.length+' 条未满足条件的模糊报价');
    await loadBoardIntents(false);
    intentSelectedIds.clear();
    skipped.forEach(item=>intentSelectedIds.add(String(item.id)));
    refreshIntentView();
    showToast(updated?'success':'error',details.join('；')||'没有可操作的模糊报价');
  }catch(error){showToast('error',error.message||'模糊报价状态更新失败');}
  finally{if(button)button.disabled=false;}
}
async function copyMgmtTodayQuotes(){
  if(mgmtTodaySelected.size===0){showToast('error','请先选择需要复制的报价');return;}
  const quotes=Array.from(mgmtTodaySelected).map(id=>quoteById(id)).filter(Boolean);
  if(!quotes.length){showToast('error','未找到选中的报价数据');return;}
  const enrichMap=await fetchCopyEnrichMap(quotes);
  try{const copied=await writeBoardClipboard(quotes.map(q=>quoteCopyText(q,enrichMap[String(q.c||'').toUpperCase()]||{})).join('\r\n'));if(copied)showToast('success','已复制 '+quotes.length+' 条报价');}
  catch(error){showToast('error',error.message||'复制失败');}
}
async function copyMgmtHistoryQuotes(){
  const visible=D.mgmtHistoryFiltered||D.mgmtHistory||[];
  const rows=visible.filter(r=>mgmtHistorySelected.has(String(r.eventId)));
  if(!rows.length){showToast('error','请先选择需要复制的报价');return;}
  const quotes=rows.map(r=>quoteById(r.quoteId)||r);
  const enrichMap=await fetchCopyEnrichMap(quotes);
  try{const copied=await writeBoardClipboard(quotes.map(q=>quoteCopyText(q,enrichMap[String(q.c||'').toUpperCase()]||{})).join('\r\n'));if(copied)showToast('success','已复制 '+quotes.length+' 条报价');}
  catch(error){showToast('error',error.message||'复制失败');}
}
async function referSelectedBoardQuotes(key){
  if(!authToken){showToast('error','应急模式为只读，认证服务恢复后才能撤销报价');return;}
  const quotes=selectedBoardQuotes(key);if(!quotes.length){showToast('error','请先选择需要撤销的报价');return;}
  const foreign=quotes.filter(quote=>quote.ownerAccountId!==currentAccountId());if(foreign.length){showToast('error','选中报价中有 '+foreign.length+' 条不是本人报价，已阻止整批撤销');return;}
  const inactive=quotes.filter(quote=>quote.status!=='active');if(inactive.length){showToast('error','选中报价中有 '+inactive.length+' 条已不是有效状态');return;}
  const quoteIds=quotes.map(quote=>quote.quoteId),page=document.getElementById(BOARD_VIEW_META[key].subpageId),button=page&&page.querySelector('[data-board-action="refer"]');
  if(button)button.disabled=true;
  try{
    const response=await authFetch(API_BASE+'/quotes/revoke',{method:'POST',body:JSON.stringify({quote_ids:quoteIds})});
    let data={};try{data=await response.json();}catch(e){}
    if(!response.ok){const detail=data.detail;throw new Error(typeof detail==='string'?detail:(detail&&detail.message)||'撤销报价失败');}
    clearBoardSelection();await loadQuoteState();showToast('success','已撤销 '+quoteIds.length+' 条报价');
  }catch(error){showToast('error',error.message||'撤销报价失败');}
  finally{if(button)button.disabled=false;syncBoardToolbarControls();}
}
function quoteExportRow(quote){
  const direction=quoteDirectionOf(quote),side=quoteSideValues(quote,direction);
  return{'报价ID':quote.quoteId||quote.sourceQuoteId||'','市场':quote.market||'','债券代码':String(quote.c||''),'债券简称':quote.n||'','剩余期限':quote.m||'','方向':direction.toUpperCase(),'计价方式':quote.priceType||'到期收益率','报价':side.price||'','数量':side.volume||'','数量单位':quote.volumeUnit||(/\.IB$/i.test(quote.c||'')?'万':'万面额'),'交易日':quote.tradeDate||'','清算速度':quote.cs||'','OCO':quote.oco?'是':'否','打包':quote.packing?'是':'否','紧急':quote.urgent?'是':'否','推荐':quote.recommend||quote.rec?'是':'否','整量':quote.round||quote.whole?'是':'否','报价机构':side.institution||'','报价交易员':side.trader||'','我方交易员':quote.ownerDisplayName||quote.tr||'','最新成交价':quote.lastPrice||'','债券库':quote.bondLib||'','主体评级':quote.r||'','债项评级':quote.br||'','中债净价':quote.nzNet||'','中债偏离':quote.nzDev||'','中债YTM':quote.nzYTM||'','中证净价':quote.csNet||'','中证全价':quote.csFull||'','中证YTM':quote.csYTM||'','报价时间':quote.t||'','报价状态':quote.status||'active','报价来源':quote.mode||quote.sourceType||'','备注':quote.note||''};
}
function bestExportRow(row){
  const bid=row.bidQuoteId&&quoteById(row.bidQuoteId),ofr=row.ofrQuoteId&&quoteById(row.ofrQuoteId);
  return{'市场':row.market||'','债券代码':String(row.c||''),'债券简称':row.n||'','剩余期限':row.m||'','计价方式':row.priceType||'到期收益率','Bid价格':row.bp||'','Bid数量':row.bvBid||'','Bid数量单位':bid&&bid.volumeUnit||'万','Bid报价机构':row.instBid||'','Bid交易员':row.trBid||'','Bid报价ID':row.bidQuoteId||'','Ofr价格':row.op||'','Ofr数量':row.bvOfr||'','Ofr数量单位':ofr&&ofr.volumeUnit||'万','Ofr报价机构':row.instOfr||'','Ofr交易员':row.trOfr||'','Ofr报价ID':row.ofrQuoteId||'','最新成交价':row.lastPrice||'','债券库':row.bondLib||'','主体评级':row.r||'','债项评级':row.br||'','中债净价':row.nzNet||'','中债偏离':row.nzDev||'','中债YTM':row.nzYTM||'','中证净价':row.csNet||'','中证全价':row.csFull||'','中证YTM':row.csYTM||'','报价时间':row.t||''};
}
function unquotedExportRow(row){
  const bond=row._bondData||{},result={'市场':row.market||'','债券代码':String(row.c||''),'债券简称':row.n||'','剩余期限':row.m||'','方向':'','报价':'','数量':'','数量单位':'','报价机构':'','报价交易员':'','我方交易员':'','债券库':row.bondLib||'','主体评级':row.r==='—'?'':row.r||'','债项评级':row.br==='—'?'':row.br||'','中债净价':row.nzNet==='—'?'':row.nzNet||'','中债YTM':row.nzYTM==='—'?'':row.nzYTM||'','中证净价':row.csNet==='—'?'':row.csNet||'','中证全价':row.csFull==='—'?'':row.csFull||'','中证YTM':row.csYTM==='—'?'':row.csYTM||'','报价时间':''};
  Object.keys(bond).sort().forEach(key=>{if(!(key in result))result['DM_'+key]=bond[key]==null?'':(typeof bond[key]==='object'?JSON.stringify(bond[key]):bond[key]);});
  return result;
}
async function allUnquotedRows(market){
  const payload=unquotedRequestPayload(market,1,500);payload.export_all=true;
  const response=await authFetch(API_BASE+'/bond-filter/unquoted',{method:'POST',body:JSON.stringify(payload)});
  let data={};try{data=await response.json();}catch(e){}
  if(!response.ok){const detail=data.detail;throw new Error(typeof detail==='string'?detail:(detail&&detail.message)||'未报价债券导出查询失败');}
  return(data.bonds||[]).map(bond=>mapUnquotedBond(bond,market));
}
function workbookBase64(workbook){
  const output=XLSX.write(workbook,{bookType:'xlsx',type:'array'}),bytes=output instanceof Uint8Array?output:new Uint8Array(output);let binary='';
  for(let index=0;index<bytes.length;index+=0x8000)binary+=String.fromCharCode.apply(null,bytes.subarray(index,index+0x8000));
  return btoa(binary);
}
function mgmtExportRow(row,history){
  const direction=quoteDirectionOf(row),price=direction==='ofr'?row.op:row.bp;
  return{
    '报价ID':row.quoteId||'','记录类型':history?(row.rs||''):'今日报价','报价方式':row.mode||'','报价状态':row.status==='revoked'?'撤销':'有效',
    '剩余期限':row.m||'','债券代码':String(row.c||''),'债券简称':row.n||'','方向':direction.toUpperCase(),'计价方式':row.priceType||'到期收益率',
    '报价':price||'','报量(万)':row.bv||'','操作标签':quoteOperationLabels(row).join(' '),'报价机构':row.inst||'','报价交易员':row.trader||'','长城交易员':row.gwtrader||row.ownerDisplayName||'',
    '最新成交价':row.lastPrice==='—'?'':row.lastPrice||'','债券库':row.bondLib==='—'?'':row.bondLib||'','主体评级':row.r==='—'?'':row.r||'','债券评级':row.br==='—'?'':row.br||'',
    '中债净价':row.nzNet==='—'?'':row.nzNet||'','中债偏离':row.nzDev==='—'?'':row.nzDev||'','中债YTM(%)':row.nzYTM==='—'?'':row.nzYTM||'',
    '中证净价':row.csNet==='—'?'':row.csNet||'','中证全价':row.csFull==='—'?'':row.csFull||'','中证YTM(%)':row.csYTM==='—'?'':row.csYTM||'','报价时间':row.t||row.quotedAt||''
  };
}
async function exportMgmtQuotes(view){
  if(typeof XLSX==='undefined'){showToast('error','Excel 组件未加载');return;}
  const history=view==='history',rows=history?(D.mgmtHistoryFiltered||[]):(D.mgmtToday||[]);
  if(!rows.length){showToast('error','当前列表没有可导出的数据');return;}
  const button=document.getElementById(history?'mgmtHistoryExportBtn':'mgmtTodayExportBtn');if(button)button.disabled=true;
  try{
    // 明确导出当前列表的全部结果；筛选后导出全部筛选结果，不读取行选中集合。
    const exportRows=rows.map(row=>mgmtExportRow(row,history));
    const sheet=XLSX.utils.json_to_sheet(exportRows),headers=Object.keys(exportRows[0]);
    sheet['!autofilter']={ref:'A1:'+XLSX.utils.encode_col(headers.length-1)+'1'};sheet['!freeze']={xSplit:0,ySplit:1,topLeftCell:'A2',activePane:'bottomLeft',state:'frozen'};
    sheet['!cols']=headers.map(header=>({wch:Math.min(36,Math.max(10,String(header).length*2,...exportRows.slice(0,200).map(row=>String(row[header]??'').length)))}));
    const workbook=XLSX.utils.book_new(),label=history?'历史报价':'今日报价';XLSX.utils.book_append_sheet(workbook,sheet,label);
    const now=new Date(),stamp=now.getFullYear()+String(now.getMonth()+1).padStart(2,'0')+String(now.getDate()).padStart(2,'0')+'_'+String(now.getHours()).padStart(2,'0')+String(now.getMinutes()).padStart(2,'0')+String(now.getSeconds()).padStart(2,'0'),name=label+'_'+stamp+'.xlsx';
    if(window.desktopAPI&&window.desktopAPI.saveXlsxFile){const result=await window.desktopAPI.saveXlsxFile({defaultName:name,base64:workbookBase64(workbook)});if(!result.cancelled)showToast('success','已导出当前列表全部 '+exportRows.length+' 条数据到 '+result.filePath);}
    else{XLSX.writeFile(workbook,name);showToast('success','已导出当前列表全部 '+exportRows.length+' 条数据');}
  }catch(error){showToast('error',error.message||'导出失败');}
  finally{if(button)button.disabled=false;}
}
async function exportBoardView(key){
  const meta=BOARD_VIEW_META[key],state=boardToolbarState[key];if(!meta||typeof XLSX==='undefined'){showToast('error','Excel 组件未加载');return;}
  const button=document.querySelector('#'+meta.subpageId+' [data-board-action="export"]');if(button)button.disabled=true;
  try{
    let rows=state.unquoted?await allUnquotedRows(meta.market):(D[meta.field]||[]);
    if(key==='board-alert')rows=rows.map(row=>quoteById(row.sourceQuoteId)||row);
    const exportRows=state.unquoted?rows.map(unquotedExportRow):meta.best?rows.map(bestExportRow):rows.map(quoteExportRow);
    if(!exportRows.length){showToast('error','当前列表没有可导出的数据');return;}
    const sheet=XLSX.utils.json_to_sheet(exportRows),headers=Object.keys(exportRows[0]);
    sheet['!autofilter']={ref:'A1:'+XLSX.utils.encode_col(headers.length-1)+'1'};sheet['!freeze']={xSplit:0,ySplit:1,topLeftCell:'A2',activePane:'bottomLeft',state:'frozen'};
    sheet['!cols']=headers.map(header=>({wch:Math.min(36,Math.max(10,String(header).length*2,...exportRows.slice(0,200).map(row=>String(row[header]??'').length)))}));
    const workbook=XLSX.utils.book_new();XLSX.utils.book_append_sheet(workbook,sheet,(state.unquoted?'未报价债券':meta.label).slice(0,31));
    const now=new Date(),stamp=now.getFullYear()+String(now.getMonth()+1).padStart(2,'0')+String(now.getDate()).padStart(2,'0')+'_'+String(now.getHours()).padStart(2,'0')+String(now.getMinutes()).padStart(2,'0')+String(now.getSeconds()).padStart(2,'0'),name='行情中心_'+(state.unquoted?'未报价债券':meta.label)+'_'+stamp+'.xlsx';
    if(window.desktopAPI&&window.desktopAPI.saveXlsxFile){const result=await window.desktopAPI.saveXlsxFile({defaultName:name,base64:workbookBase64(workbook)});if(!result.cancelled)showToast('success','已导出到 '+result.filePath);}
    else{XLSX.writeFile(workbook,name);showToast('success','导出成功');}
  }catch(error){showToast('error',error.message||'导出失败');}
  finally{if(button){button.disabled=!!state.unquoted;syncBoardToolbarControls();}}
}
function quoteSideValues(r,dir){
  const candidate=r.counterpartyTrader||r.trader||'',ownerTrader=r.ownerDisplayName||r.tr||'';
  const counterpartyTrader=candidate&&ownerTrader&&String(candidate).trim()===String(ownerTrader).trim()?'':candidate;
  if(dir==='bid')return{price:r.bp||'',volume:r.bvBid||r.bv||'',institution:r.counterpartyInstitution||r.instBid||r.inst||'',trader:counterpartyTrader};
  return{price:r.op||'',volume:r.bvOfr||r.bv||'',institution:r.counterpartyInstitution||r.instOfr||r.inst||'',trader:counterpartyTrader};
}
function ensureQuoteLogs(row){
  D.quoteLogs=D.quoteLogs||{};
  const key=row&&(row.quoteId||row.sourceQuoteId)||'';
  if(!key)return[];
  if(!D.quoteLogs[key]){
    const eventLogs=(D.quoteEvents||[]).filter(event=>event.quoteId===key).map(event=>({time:event.time,user:event.user||row.ownerDisplayName||row.tr||'系统',action:event.action,direction:event.direction,oldPrice:event.oldPrice||'',price:event.price||'',volume:event.volume||'',note:event.note||''}));
    if(eventLogs.length)D.quoteLogs[key]=eventLogs;
    else{
      const dir=quoteDirectionOf(row),side=quoteSideValues(row,dir);
      D.quoteLogs[key]=side.price?[{time:row.t||quoteNow(),user:row.ownerDisplayName||row.tr||'系统',action:'quote',direction:dir,oldPrice:'',price:side.price,volume:side.volume,note:'当前行情初始记录'}]:[];
    }
  }
  return D.quoteLogs[key];
}
function quoteValuationNumber(value){
  if(value===null||value===undefined||String(value).trim()===''||['—','--'].includes(String(value).trim()))return'—';
  const text=String(value).trim(),number=Number(text.replace('%',''));
  return Number.isFinite(number)?number.toFixed(4)+'%':escapeQuoteHtml(text)+(text.includes('%')?'':'%');
}
function quoteValuationValuesHtml(r,enrich){
  enrich=enrich||{};
  const callable=enrich.isCallable===true||r.isCallable===true||bondCallableStatus(r.c)==='callable';
  const maturity=enrich.cbYtm??r.cbYtm??(r.nzYTM&&r.nzYTM!=='—'?r.nzYTM:null);
  const exercise=enrich.cbExYtm??r.cbYte??null;
  const maturityHtml='<span class="quote-cb-value">'+quoteValuationNumber(maturity)+'</span>';
  return callable?maturityHtml+'<span class="quote-cb-sep">｜</span><span class="quote-cb-value">'+quoteValuationNumber(exercise)+'</span>':maturityHtml;
}
async function hydrateQuoteModalValuation(row){
  const target=document.getElementById('quoteChinaBondValuation');if(!target||!row||!row.c)return;
  const code=String(row.c).toUpperCase(),state=quoteModalState;
  try{
    const enrichMap=await fetchCopyEnrichMap([row]);
    if(!document.body.contains(target)||quoteModalState!==state)return;
    const enrich=enrichMap[code]||Object.values(enrichMap).find(item=>item&&normalizeBondCode(item.securityId)===normalizeBondCode(code))||{};
    if(quoteModalState===state)state.valuation={cbYtm:enrich.cbYtm,cbYte:enrich.cbExYtm,isCallable:enrich.isCallable===true};
    target.innerHTML=quoteValuationValuesHtml(row,enrich);
  }catch(error){
    if(document.body.contains(target)&&quoteModalState===state)target.innerHTML=quoteValuationValuesHtml(row,{});
  }
}
function quoteBondSummaryHtml(r,emptyMarket=false,showValuation=false){
  const maturity=escapeQuoteHtml(r.m||'—'),rating=escapeQuoteHtml(r.r||'—'),bondRating=escapeQuoteHtml(r.br||'—');
  const bid=escapeQuoteHtml(emptyMarket?'--':r.bp||'--'),ofr=escapeQuoteHtml(emptyMarket?'--':r.op||'--');
  const marketBest='<div class="quote-market-best"><span class="quote-market-label">市场最优</span><span class="quote-market-price quote-market-bid">'+bid+'</span><span class="quote-market-divider">｜</span><span class="quote-market-price quote-market-ofr">'+ofr+'</span></div>';
  const metrics=showValuation?'<div class="quote-market-metrics">'+marketBest+'<div class="quote-cb-valuation"><span class="quote-market-label">中债估值</span><span id="quoteChinaBondValuation">'+quoteValuationValuesHtml(r,{})+'</span></div></div>':marketBest;
  return `<div class="quote-bond-summary">
    <div class="quote-bond-main"><div class="quote-bond-name"><strong>${escapeQuoteHtml(r.n||'未命名债券')}</strong><span class="quote-bond-code">${escapeQuoteHtml(r.c||'—')}</span></div><div class="quote-bond-meta"><span>剩余期限 ${maturity}</span><span>主体评级 ${rating}</span><span>债项评级 ${bondRating}</span></div></div>
    ${metrics}
  </div>`;
}
function quoteFormHtml(r,mode){
  const dir=mode==='new'?'bid':quoteDirectionOf(r),side=mode==='new'?{price:'',volume:'',institution:'',trader:''}:quoteSideValues(r,dir);
  const settlement=mode==='new'?'T+1':(r.cs==='T+0'?'T+0':'T+1');
  return `<div class="quote-modal">
    ${quoteBondSummaryHtml(r,mode==='new',true)}
    <div class="quote-form-section">
      <div class="quote-direction"><button class="quote-dir-btn ${dir==='bid'?'active-bid':''}" type="button" data-dir="bid" onclick="setQuoteDirection('bid')">Bid 买入</button><button class="quote-dir-btn ${dir==='ofr'?'active-ofr':''}" type="button" data-dir="ofr" onclick="setQuoteDirection('ofr')">Ofr 卖出</button></div>
    </div>
    <div class="quote-fields">
      <div class="quote-field span-6"><label for="quotePrice">价格</label><div class="quote-input-group"><div class="quote-step-wrap"><input class="quote-control mono" id="quotePrice" inputmode="decimal" autocomplete="off" min="0" value="${escapeQuoteHtml(side.price)}" placeholder="0.0000"><div class="quote-step-buttons"><button class="quote-step-btn" type="button" onclick="adjustQuoteInput('quotePrice',0.01,4)">+1bp</button><button class="quote-step-btn" type="button" onclick="adjustQuoteInput('quotePrice',-0.01,4)">-1bp</button></div></div><select class="quote-control" id="quotePriceType"><option ${mode==='new'||!r.priceType||r.priceType==='到期收益率'?'selected':''}>到期收益率</option><option ${mode!=='new'&&r.priceType==='行权收益率'?'selected':''}>行权收益率</option><option ${mode!=='new'&&r.priceType==='净价'?'selected':''}>净价</option><option ${mode!=='new'&&r.priceType==='全价'?'selected':''}>全价</option></select></div><span class="quote-field-error" id="quotePriceError"></span></div>
      <div class="quote-field span-6"><label for="quoteVolume">报量</label><div class="quote-input-group"><div class="quote-step-wrap"><input class="quote-control mono" id="quoteVolume" inputmode="numeric" autocomplete="off" min="0" value="${escapeQuoteHtml(side.volume)}" placeholder="例如 1000"><div class="quote-step-buttons"><button class="quote-step-btn" type="button" onclick="adjustQuoteInput('quoteVolume',1000,0)">+1k</button><button class="quote-step-btn" type="button" onclick="adjustQuoteInput('quoteVolume',-1000,0)">-1k</button></div></div><select class="quote-control" id="quoteVolumeUnit"><option ${!r.volumeUnit||r.volumeUnit==='万'?'selected':''}>万</option><option ${r.volumeUnit==='万面额'?'selected':''}>万面额</option><option ${r.volumeUnit==='手'?'selected':''}>手</option><option ${r.volumeUnit==='张'?'selected':''}>张</option></select></div><span class="quote-field-error" id="quoteVolumeError"></span></div>
      <div class="quote-field span-3"><label for="quoteTradeDate">交易日期</label><input class="quote-control" id="quoteTradeDate" type="date" value="${escapeQuoteHtml(r.tradeDate||quoteToday())}"></div>
      <div class="quote-field span-2"><label for="quoteSettlement">清算速度</label><select class="quote-control" id="quoteSettlement"><option value="">请选择</option><option ${settlement==='T+0'?'selected':''}>T+0</option><option ${settlement==='T+1'?'selected':''}>T+1</option></select></div>
      <div class="quote-field span-4"><label for="quoteInstitution">报价机构</label><input class="quote-control" id="quoteInstitution" value="${escapeQuoteHtml(side.institution)}" placeholder="机构简称"></div>
      <div class="quote-field span-3"><label for="quoteTrader">报价交易员</label><input class="quote-control" id="quoteTrader" value="${escapeQuoteHtml(side.trader)}" placeholder="交易员姓名"></div>
      <div class="quote-tags-note-row"><div class="quote-tags-field"><label>报价标签</label><div class="quote-tags"><label class="quote-tag-check"><input id="quoteUrgent" type="checkbox" ${mode!=='new'&&r.urgent?'checked':''}>急</label><label class="quote-tag-check"><input id="quoteRecommend" type="checkbox" ${mode!=='new'&&r.recommend?'checked':''}>荐</label><label class="quote-tag-check"><input id="quoteRound" type="checkbox" ${mode!=='new'&&r.round?'checked':''}>整</label></div></div><div class="quote-note-field"><label for="quoteNote">备注</label><textarea class="quote-control" id="quoteNote" maxlength="120" placeholder="补充报价约束、有效期或沟通事项">${escapeQuoteHtml(mode==='new'?'':r.note||'')}</textarea></div></div>
    </div>
  </div>`;
}
function openQuoteForm(mode,idx,dataType,requestedQuoteId,origin){
  const data=boardDataByType(dataType),source=data&&data[idx];
  if(!source){showToast('error','未找到对应行情数据');return;}
  const sourceId=requestedQuoteId||source.sourceQuoteId||(source.bp?source.bidQuoteId:source.ofrQuoteId)||source.quoteId;
  const master=quoteById(sourceId);
  if(mode==='edit'&&master&&!canManageQuote(master)){showToast('error','只能编辑自己的报价');return;}
  const row={...(mode==='edit'&&master?master:source)};
  quoteModalState={mode,type:dataType,idx,quoteId:sourceId,sourceRow:row,direction:mode==='new'?'bid':quoteDirectionOf(row),silentSuccess:origin==='context-menu'};
  if(mode==='edit')ensureQuoteLogs(row);
  const saveLabel=mode==='new'?'发布报价':'保存修改';
  const footer=`<div class="quote-footer-note"><kbd>Enter</kbd><span>${saveLabel}</span><kbd>Esc</kbd><span>取消</span></div><button class="tb-btn tb-btn-default" type="button" onclick="closeModal()">取消</button><button class="tb-btn tb-btn-primary" type="button" id="quoteSaveBtn" onclick="saveEditQuote()">${saveLabel}</button>`;
  showModal(mode==='new'?'新报价':'编辑报价',quoteFormHtml(row,mode),{wide:true,footerHtml:footer,initialFocus:'#quotePrice'});
  document.getElementById('modalBody').classList.add('quote-workflow-body');
  hydrateQuoteModalValuation(row);
}
function showEditQuoteModal(idx,dataType,quoteId,origin){openQuoteForm('edit',idx,dataType,quoteId,origin);}
function showNewQuoteModal(idx,dataType,origin){openQuoteForm('new',idx,dataType,'',origin);}
function setQuoteDirection(dir){
  if(!quoteModalState)return;
  quoteModalState.direction=dir;
  document.querySelectorAll('.quote-dir-btn').forEach(btn=>btn.className='quote-dir-btn'+(btn.dataset.dir===dir?' active-'+dir:''));
  if(quoteModalState.mode==='edit'){
    const side=quoteSideValues(quoteModalState.sourceRow,dir);
    document.getElementById('quotePrice').value=side.price;
    document.getElementById('quoteVolume').value=side.volume;
    document.getElementById('quoteInstitution').value=side.institution;
    document.getElementById('quoteTrader').value=side.trader;
  }
  document.getElementById('quotePrice').focus();
}
function quoteFieldValue(id){const el=document.getElementById(id);return el?el.value.trim():'';}
function adjustQuoteInput(id,delta,decimals){
  const input=document.getElementById(id);if(!input)return;
  const current=parseFloat(input.value);
  let base=Number.isFinite(current)?current:NaN;
  if(!Number.isFinite(base)&&id==='quotePrice'&&quoteModalState&&quoteModalState.valuation){
    const type=quoteFieldValue('quotePriceType'),valuation=quoteModalState.valuation;
    base=Number(type==='行权收益率'?valuation.cbYte:valuation.cbYtm);
  }
  if(!Number.isFinite(base)){showToast('error','中债估值尚未获取，无法按1bp调整');return;}
  const next=Math.max(0,base+delta);
  input.value=decimals>0?next.toFixed(decimals):String(Math.round(next));
  input.classList.remove('invalid');
  const error=document.getElementById(id==='quotePrice'?'quotePriceError':'quoteVolumeError');if(error)error.textContent='';
  input.focus();
}
function validateQuoteForm(){
  let valid=true;
  [['quotePrice','quotePriceError','请输入不小于0的有效价格'],['quoteVolume','quoteVolumeError','请输入不小于0的有效报量']].forEach(([id,errorId,msg])=>{
    const el=document.getElementById(id),value=el.value.trim(),number=Number(value),ok=value===''||(Number.isFinite(number)&&number>=0);
    el.classList.toggle('invalid',!ok);document.getElementById(errorId).textContent=ok?'':msg;if(!ok)valid=false;
  });
  return valid;
}
function recordQuoteLog(row,action,dir,oldPrice,newPrice,volume,note){
  const logs=ensureQuoteLogs(row);
  logs.unshift({time:quoteNow(),user:currentUser?currentUser.name:'当前用户',action,direction:dir,oldPrice:oldPrice||'',price:newPrice||'',volume:volume||'',note:note||''});
}
function updateMatchingQuoteCopies(original,updated,sourceType){
  const quoteId=original.quoteId||original.sourceQuoteId;
  const master=quoteById(quoteId);if(!master)return null;
  const previousAlertKeys=allAlertKeys();
  Object.assign(master,updated,{quoteId:quoteId,sourceQuoteId:quoteId});
  rebuildBoardProjections();syncMgmtToday();notifyNewAlerts(previousAlertKeys);
  return master;
}
function manualQuoteMarket(source,stateType){
  if(source&&['credit','rate'].includes(source.market))return source.market;
  if(String(stateType||'').startsWith('credit'))return'credit';
  if(String(stateType||'').startsWith('rate'))return'rate';
  return classifyBondMarket(source&&source.c,source&&source.n,source&&source._apiInfo,source&&source.bondType);
}
function buildManualQuoteFromForm(source,state,dir,form){
  const quoteId=makeQuoteId('quote'),createdAt=new Date().toISOString(),ownerName=currentUser?currentUser.name:'';
  const quote={
    quoteId,sourceQuoteId:quoteId,sourceType:'manual',status:'active',unquoted:false,direction:dir,
    ownerAccountId:currentAccountId(),ownerUsername:currentUser?currentUser.username:'',ownerDisplayName:ownerName,ownerOrgId:currentUser?currentUser.orgId||'':'',canManage:true,
    market:manualQuoteMarket(source,state.type),mode:'手工录入',m:source.m||'',c:source.c||'',n:source.n||'',bondType:source.bondType||'',
    lastPrice:source.lastPrice||'',bondLib:source.bondLib||'在库',r:source.r||'—',br:source.br||'—',
    nzNet:source.nzNet||'—',nzDev:source.nzDev||'—',nzYTM:source.nzYTM||'—',csNet:source.csNet||'—',csFull:source.csFull||'—',csYTM:source.csYTM||'—',
    priceType:form.priceType,volumeUnit:form.volumeUnit,tradeDate:form.tradeDate,cs:form.cs,
    urgent:form.urgent,recommend:form.recommend,round:form.round,oco:false,packing:false,note:form.note,
    t:new Date().toLocaleTimeString('zh-CN',{hour:'2-digit',minute:'2-digit',second:'2-digit'}),createdAt,quotedAt:createdAt,
    bp:'',op:'',bv:'',bvBid:'',bvOfr:'',inst:'',instBid:'',instOfr:'',tr:'',trBid:'',trOfr:'',trader:'',
    counterpartyInstitution:form.institution,counterpartyTrader:form.trader
  };
  if(dir==='bid'){quote.bp=form.price;quote.bvBid=form.volume;quote.instBid=form.institution;quote.trBid=ownerName;}
  else{quote.op=form.price;quote.bvOfr=form.volume;quote.instOfr=form.institution;quote.trOfr=ownerName;}
  quote.bv=form.volume;quote.inst=form.institution;quote.tr=ownerName;quote.trader=form.trader;
  return quote;
}
function prepareSavedQuoteReveal(quote){
  const market=quote.market==='rate'?'rate':'credit',key='board-'+market+'-all',meta=BOARD_VIEW_META[key],toolbar=boardToolbarState[key],unquoted=boardUnquotedState[market];
  if(toolbar)Object.assign(toolbar,{direction:'',mine:false,urgent:false,recommend:false,round:false,unquoted:false});
  setUnquotedQuoteControls(market,false);
  if(unquoted){
    const code=normalizeBondCode(quote.c),before=unquoted.rows.length;
    unquoted.requestId+=1;unquoted.loading=false;unquoted.rows=unquoted.rows.filter(row=>normalizeBondCode(row.c)!==code);
    unquoted.total=Math.max(0,unquoted.total-(before-unquoted.rows.length));
  }
  syncBoardToolbarControls();
  const tab=document.querySelector('#page-board .subtab[data-st="'+key+'"]');
  if(tab&&!tab.classList.contains('active'))switchSubTab(tab);
  return meta&&meta.tbodyId;
}
function ensureSavedQuoteVisible(quote){
  const market=quote.market==='rate'?'rate':'credit',meta=BOARD_VIEW_META['board-'+market+'-all'];
  const containsQuote=()=>(D[meta.field]||[]).some(row=>(row.quoteId||row.sourceQuoteId)===quote.quoteId);
  if(containsQuote())return true;
  allMarketGroups().filter(group=>group.market===market).forEach(group=>activeMarketGroupIds[group.kind].delete(group.id));
  resetBondFilterPanel(market,false);rebuildBoardProjections();
  return containsQuote();
}
function highlightSavedQuote(quoteId,tbodyId){
  setTimeout(()=>{
    const tbody=document.getElementById(tbodyId);if(!tbody)return;
    const row=Array.from(tbody.querySelectorAll('tr[data-quote-id]')).find(item=>item.dataset.quoteId===quoteId);
    if(!row)return;
    row.classList.remove('quote-row-reveal');void row.offsetWidth;row.classList.add('quote-row-reveal');
    row.scrollIntoView({block:'center',behavior:'smooth'});
    setTimeout(()=>row.classList.remove('quote-row-reveal'),2600);
  },80);
}
function revealAndHighlightSavedQuotes(quoteIds,shouldReveal){
  const ids=[...new Set((quoteIds||[]).map(String).filter(Boolean))];
  const quotes=ids.map(id=>quoteById(id)).filter(Boolean);
  if(!quotes.length)return;
  const focus=sortMarketRows(quotes)[0],market=focus.market==='rate'?'rate':'credit';
  const tbodyId=prepareSavedQuoteReveal(focus);
  if(shouldReveal!==false)ensureSavedQuoteVisible(focus);
  rebuildBoardProjections();renderAll();
  if(shouldReveal!==false)ids.filter(id=>{const q=quoteById(id);return q&&(q.market==='rate'?'rate':'credit')===market;}).forEach(id=>highlightSavedQuote(id,tbodyId));
}
async function saveEditQuote(){
  if(!authToken){showToast('error','应急模式为只读，认证服务恢复后才能保存报价');return;}
  if(!quoteModalState||!validateQuoteForm())return;
  const state=quoteModalState,button=document.getElementById('quoteSaveBtn');if(button&&button.disabled)return;
  const target=state.mode==='edit'?quoteById(state.quoteId):null;
  if(state.mode==='edit'&&!target){showToast('error','未找到对应报价主记录');return;}
  if(target&&!canManageQuote(target)){showToast('error','只能编辑自己的报价');return;}
  const original=state.sourceRow,base=target||original,dir=state.direction;
  const price=quoteFieldValue('quotePrice'),volume=quoteFieldValue('quoteVolume'),institution=quoteFieldValue('quoteInstitution'),trader=quoteFieldValue('quoteTrader'),note=quoteFieldValue('quoteNote');
  const form={price,volume,institution,trader,note,priceType:quoteFieldValue('quotePriceType'),volumeUnit:quoteFieldValue('quoteVolumeUnit'),tradeDate:quoteFieldValue('quoteTradeDate'),cs:quoteFieldValue('quoteSettlement'),urgent:document.getElementById('quoteUrgent').checked,recommend:document.getElementById('quoteRecommend').checked,round:document.getElementById('quoteRound').checked};
  const updated={...base,direction:dir,priceType:form.priceType,volumeUnit:form.volumeUnit,tradeDate:form.tradeDate,cs:form.cs,urgent:form.urgent,recommend:form.recommend,round:form.round,note,t:new Date().toLocaleTimeString('zh-CN',{hour:'2-digit',minute:'2-digit',second:'2-digit'}),bp:'',op:'',bv:'',bvBid:'',bvOfr:'',instBid:'',instOfr:'',trBid:'',trOfr:''};
  const ownerName=updated.ownerDisplayName||(currentUser?currentUser.name:'');
  if(dir==='bid'){updated.bp=price;updated.bvBid=volume;updated.instBid=institution;updated.trBid=ownerName;}
  else{updated.op=price;updated.bvOfr=volume;updated.instOfr=institution;updated.trOfr=ownerName;}
  updated.bv=volume;updated.inst=institution||updated.inst;updated.counterpartyInstitution=institution;updated.counterpartyTrader=trader;updated.trader=trader;updated.tr=ownerName;
  const oldPrice=quoteSideValues(base,dir).price;
  if(button){button.disabled=true;button.textContent=state.mode==='new'?'发布中...':'保存中...';}
  try{
    let revealTbody='',savedQuote=null,persistResult=null;
    if(state.mode==='edit'){
      const event=quoteEventSnapshot(updated,'edit',note||'修改报价');event.oldPrice=oldPrice;event.price=price;event.volume=volume;
      persistResult=await persistQuoteState(updated,'edit',event);
      const saved=updateMatchingQuoteCopies(target,updated,state.type);
      recordQuoteLog(saved,'edit',dir,oldPrice,price,volume,note||'修改报价');
      D.quoteEvents=D.quoteEvents||[];D.quoteEvents.unshift(event);savedQuote=saved;
    }else{
      const clean=buildManualQuoteFromForm(original,state,dir,form);
      if(dir==='bid')clean.trBid=clean.ownerDisplayName||(currentUser?currentUser.name:'');
      else clean.trOfr=clean.ownerDisplayName||(currentUser?currentUser.name:'');
      const event=quoteEventSnapshot(clean,'quote',note||'新建报价');
      persistResult=await persistQuoteState(clean,'quote',event);
      const previousAlertKeys=allAlertKeys();
      savedQuote=mergeCanonicalQuote(clean);recordQuoteLog(savedQuote,'quote',dir,'',price,volume,note||'新建报价');
      D.quoteEvents=D.quoteEvents||[];D.quoteEvents.unshift(event);
      syncMgmtToday();notifyNewAlerts(previousAlertKeys);
    }
    const hasCanonicalState=!!(persistResult&&persistResult.quote&&persistResult.quote.quoteId&&Number(persistResult.revision)>0);
    if(!hasCanonicalState){
      const refreshed=await loadQuoteState({notify:true});
      if(!refreshed)throw new Error('报价已保存，但行情刷新失败，请手动刷新');
      savedQuote=quoteById(savedQuote.quoteId)||savedQuote;
    }
    syncMgmtToday();
    await loadMgmtToday({minimumRevision:Number(persistResult&&persistResult.revision||0)});
    revealTbody=prepareSavedQuoteReveal(savedQuote);
    ensureSavedQuoteVisible(savedQuote);
    rebuildBoardProjections();
    syncMgmtHistory();renderAll();closeModal();quoteModalState=null;
    if(revealTbody)highlightSavedQuote(savedQuote.quoteId,revealTbody);
    if(!state.silentSuccess)showToast('success',state.mode==='new'?'报价已发布':'报价已更新');
  }catch(error){
    showToast('error',error.message||'报价保存失败，请重试');
  }finally{
    if(button&&quoteModalState===state){button.disabled=false;button.textContent=state.mode==='new'?'发布报价':'保存修改';}
  }
}

// 保留旧调用入口
function switchDir(el,dir){setQuoteDirection(dir);}

// ===== 给数据列表绑定右键、双击和选中事件 =====
let dragSelecting=false, dragStartIdx=-1, dragTbody=null;
function boardRowsForTbody(tbodyId){
  if(tbodyId==='boardIntentBody')return intentFilterState.filteredRows||[];
  const meta=Object.values(BOARD_VIEW_META).find(item=>item.tbodyId===tbodyId);
  return meta?(D[meta.field]||[]):[];
}
function syncBoardSelectionClasses(){
  Object.values(BOARD_VIEW_META).forEach(meta=>{
    const tbody=document.getElementById(meta.tbodyId);if(!tbody)return;
    const rows=boardRowsForTbody(meta.tbodyId),elements=tbody.querySelectorAll('tr:not(.placeholder-row)');
    elements.forEach((element,index)=>{const ids=quoteIdsForBoardRow(rows[index]);element.classList.toggle('selected',ids.length>0&&ids.every(id=>boardSelectedQuoteIds.has(id)));});
  });
  const intentTbody=document.getElementById('boardIntentBody');
  if(intentTbody){
    const rows=intentFilterState.filteredRows||[],elements=intentTbody.querySelectorAll('tr:not(.placeholder-row)');
    elements.forEach((element,index)=>{const row=rows[index];element.classList.toggle('selected',!!row&&intentSelectedIds.has(String(row.id)));});
  }
}
function bindTableEvents(){
  const tbodies=['boardCreditAllBody','boardRateAllBody','boardCreditBestBody','boardRateBestBody','boardAlertBody','boardIntentBody'];
  tbodies.forEach(tid=>{
    const tbody=document.getElementById(tid);
    if(!tbody)return;
    tbody.onmousedown=function(e){
      if(e.button!==0)return;
      const row=e.target.closest('tr:not(.placeholder-row)');if(!row)return;
      dragSelecting=true;
      dragTbody=tbody;
      const realRows=Array.from(tbody.querySelectorAll('tr:not(.placeholder-row)'));
      dragStartIdx=realRows.indexOf(row);
      const selected=boardRowsForTbody(tid)[dragStartIdx];
      if(tid!=='boardIntentBody'){
        boardSelectionMode='manual';
        boardFrozenProjection=boardFrozenProjection||captureBoardProjection();
        updateBoardNewQuoteBanner();
      }
      if(!e.shiftKey&&!e.ctrlKey&&!e.metaKey){
        clearBoardSelection();intentSelectedIds.clear();
      }
      if(selected){
        if(tid==='boardIntentBody'){
          const key=String(selected.id);
          if(intentSelectedIds.has(key))intentSelectedIds.delete(key);else intentSelectedIds.add(key);
          syncBoardSelectionClasses();
        }else{
          const ids=quoteIdsForBoardRow(selected),willSelect=!ids.length||!ids.every(id=>boardSelectedQuoteIds.has(id));
          setBoardRowSelected(selected,willSelect);syncBoardSelectionClasses();
          setRecommendationTarget(selected,e.target.closest('td'),tid);
        }
      }
    };
    tbody.onmouseover=function(e){
      if(!dragSelecting||!dragTbody||dragTbody!==tbody)return;
      const row=e.target.closest('tr');
      if(!row)return;
      const realRows=Array.from(tbody.querySelectorAll('tr:not(.placeholder-row)')),curIdx=realRows.indexOf(row);if(curIdx<0)return;
      const start=dragStartIdx>=0?dragStartIdx:curIdx;
      const minI=Math.min(start,curIdx), maxI=Math.max(start,curIdx);
      const data=boardRowsForTbody(tid);for(let i=minI;i<=maxI;i++){if(tid==='boardIntentBody'){if(data[i])intentSelectedIds.add(String(data[i].id));}else setBoardRowSelected(data[i],true);}
      syncBoardSelectionClasses();
    };
  });
  document.addEventListener('mouseup',function(){dragSelecting=false;dragTbody=null;dragStartIdx=-1;});
  document.querySelectorAll('#boardCreditAllBody tr:not(.placeholder-row),#boardRateAllBody tr:not(.placeholder-row),#boardCreditBestBody tr:not(.placeholder-row),#boardRateBestBody tr:not(.placeholder-row),#boardAlertBody tr:not(.placeholder-row)').forEach(tr=>{
    tr.oncontextmenu=function(e){
      const idx=Array.from(this.parentElement.querySelectorAll('tr:not(.placeholder-row)')).indexOf(this);
      const tbodyId=this.parentElement.id;
      let dataType='credit';
      if(tbodyId==='boardRateAllBody')dataType='rate';
      if(tbodyId==='boardCreditBestBody')dataType='creditBest';
      if(tbodyId==='boardRateBestBody')dataType='rateBest';
      if(tbodyId==='boardAlertBody')dataType='alert';
      showCtxMenu(e,idx,dataType);
    };
    tr.ondblclick=function(e){
      const idx=Array.from(this.parentElement.querySelectorAll('tr:not(.placeholder-row)')).indexOf(this);
      const tbodyId=this.parentElement.id;
      let dataType='credit';
      if(tbodyId==='boardRateAllBody')dataType='rate';
      if(tbodyId==='boardCreditBestBody')dataType='creditBest';
      if(tbodyId==='boardRateBestBody')dataType='rateBest';
      if(tbodyId==='boardAlertBody')dataType='alert';
      const cell=e.target.closest('td');
      const data=boardDataByType(dataType),row=data&&data[idx];
      showEditQuoteModal(idx,dataType,quoteIdFromBoardCell(row,cell));
    };
  });
  syncBoardSelectionClasses();
}
setTimeout(()=>document.getElementById('loginUser').focus(),300);

// ===== 模糊报价筛选引擎（P0：组内OR、组间AND、结构化精确匹配） =====
const INTENT_FILTER_MODULES=['bond','credit','terms','region'];
const INTENT_FILTER_EXCLUDED=['favorite','filterGroup','quote','liquidity'];
// 值→fieldKey映射表（从面板HTML的data-field-key/data-key自动生成）
const INTENT_VALUE_TO_FIELDS=new Map();
function buildIntentValueMap(){
  INTENT_VALUE_TO_FIELDS.clear();
  const panel=document.getElementById('filters-intent');
  if(!panel)return;
  panel.querySelectorAll('.filter-tag[data-field-key][data-key]').forEach(tag=>{
    const fk=tag.getAttribute('data-field-key'),v=tag.getAttribute('data-key');
    if(!fk||!v)return;
    const norm=v.toUpperCase();
    if(!INTENT_VALUE_TO_FIELDS.has(norm))INTENT_VALUE_TO_FIELDS.set(norm,[]);
    const arr=INTENT_VALUE_TO_FIELDS.get(norm);
    if(!arr.includes(fk))arr.push(fk);
  });
}
// 独立筛选状态
const intentFilterState={search:'',selectedByField:new Map(),filteredRows:[],total:0,filteredTotal:0};
// 将行数据解析为结构化条件集合 Set<fieldKey:normalizedValue>
// P1: 优先使用recognitionJson结构化数据，回退到result字符串解析
function parseIntentRowConditions(row){
  const conditions=new Set();
  if(!row)return conditions;
  // 优先使用结构化识别结果
  const recJson=row.recognitionJson||row.recognition_json||'';
  if(recJson){
    try{
      const rec=typeof recJson==='string'?JSON.parse(recJson):recJson;
      if(rec.conditions&&Array.isArray(rec.conditions)){
        rec.conditions.forEach(c=>{
          const key=normalizeIntentConditionKey(c);
          if(key&&key!=='direction'&&c.normalizedValue){
            conditions.add(key+':'+c.normalizedValue.toUpperCase());
          }
        });
        return conditions;
      }
    }catch(e){/* 回退到字符串解析 */}
  }
  // 回退：从 result 字符串解析
  const resultStr=row.result||'';
  if(!resultStr)return conditions;
  const tokens=resultStr.split(/[\s,;]+/).filter(Boolean);
  tokens.forEach(token=>{
    const norm=token.toUpperCase();
    const fields=INTENT_VALUE_TO_FIELDS.get(norm);
    if(fields){fields.forEach(fk=>conditions.add(fk+':'+norm));}
  });
  return conditions;
}
// P1: 结构化识别结果渲染（带标签条件 + 未识别片段提示 + P2点击对照）
function renderStructuredRecognitionTags(rec,options){
  options=options||{};
  if(!rec||!Array.isArray(rec.conditions)||!rec.conditions.length)return'';
  const hasUnmatched=(rec.unmatchedTokens&&rec.unmatchedTokens.length>0)||rec.status!=='recognized';
  const detailRowId=Number(options.detailRowId),interactive=options.enableDetail&&hasUnmatched&&Number.isFinite(detailRowId);
  let html='<div style="display:flex;flex-wrap:wrap;gap:3px;align-items:center'+(interactive?';cursor:pointer':'')+'"'+(interactive?' data-has-unmatched="1" onclick="showRecognitionDetail('+detailRowId+',event)"':'')+'>';
  sortIntentConditionsForDisplay(rec.conditions).forEach(c=>{
    const direction=c.key==='direction';
    html+='<span style="background:'+(direction?'#1f6feb33':'#23863633')+';color:'+(direction?'#58a6ff':'#3fb950')+';font-size:10px;padding:1px 5px;border-radius:3px'+(direction?';font-weight:600':'')+'" title="'+escapeQuoteHtml(c.label||c.key)+'">'+escapeQuoteHtml(c.value)+'</span>';
  });
  if(rec.unmatchedTokens&&rec.unmatchedTokens.length){
    html+='<span style="background:#da363333;color:#f85149;font-size:10px;padding:1px 5px;border-radius:3px" title="未识别片段">'+escapeQuoteHtml(rec.unmatchedTokens.join(' '))+'</span>';
  }
  html+='</div>';
  return html;
}
function renderIntentResultCell(safeRow){
  const rowId=safeRow&&safeRow.id;
  const row=(rowId!=null)?(D.boardIntent.find(r=>String(r.id)===String(rowId))||safeRow):safeRow;
  const recJson=row.recognitionJson||row.recognition_json||'';
  if(recJson){
    try{
      const rec=typeof recJson==='string'?JSON.parse(recJson):recJson;
      if(rec.conditions&&rec.conditions.length>0){
        return renderStructuredRecognitionTags(rec,{enableDetail:true,detailRowId:rowId});
      }
    }catch(e){/* 回退 */}
  }
  return'<span style="color:#79c0ff;font-size:11px;font-family:\'JetBrains Mono\',monospace">'+(safeRow.result||'')+'</span>';
}
// P2: 识别详情弹出层（原文对照）
function showRecognitionDetail(rowId,event){
  if(event){event.stopPropagation();}
  closeRecognitionDetail();
  const row=D.boardIntent.find(r=>String(r.id)===String(rowId));
  if(!row)return;
  const recJson=row.recognitionJson||row.recognition_json||'';
  let rec=null;
  try{rec=typeof recJson==='string'?JSON.parse(recJson):recJson;}catch(e){}
  if(!rec)return;
  const content=row.content||'';
  // 构建原文标注
  let annotated=escapeQuoteHtml(content);
  const condValues=(rec.conditions||[]).map(c=>c.value).filter(Boolean).sort((a,b)=>b.length-a.length);
  condValues.forEach(v=>{
    const escaped=escapeQuoteHtml(v);
    const isDir=(rec.conditions||[]).some(c=>c.value===v&&c.key==='direction');
    const cls=isDir?'rec-hit':'rec-hit';
    annotated=annotated.replace(escaped,'<mark class="'+cls+'">'+escaped+'</mark>');
  });
  (rec.unmatchedTokens||[]).forEach(t=>{
    const escaped=escapeQuoteHtml(t);
    annotated=annotated.replace(escaped,'<mark class="rec-miss">'+escaped+'</mark>');
  });
  const statusMap={recognized:'完整识别',partial:'部分识别',conflict:'冲突',failed:'识别失败'};
  const statusColor={recognized:'#3fb950',partial:'#d29922',conflict:'#f85149',failed:'#ef4444'};
  const st=rec.status||'recognized';
  let html='<div class="rec-detail-popover" id="recDetailPopover" style="left:'+Math.min(event.clientX,window.innerWidth-440)+'px;top:'+(event.clientY+10)+'px">';
  html+='<div style="margin-bottom:8px;font-weight:600;color:#e6edf3">识别详情 <span style="font-size:10px;color:'+(statusColor[st]||'#8b949e')+';background:'+(statusColor[st]||'#8b949e')+'22;padding:1px 6px;border-radius:3px;margin-left:6px">'+(statusMap[st]||st)+'</span></div>';
  html+='<div style="margin-bottom:6px;color:#8b949e;font-size:10px">版本: '+(rec.version||'legacy')+'</div>';
  html+='<div style="margin-bottom:8px;padding:8px;background:#0d1117;border-radius:4px;word-break:break-all">'+annotated+'</div>';
  if(rec.unmatchedTokens&&rec.unmatchedTokens.length>0){
    html+='<div style="color:#f85149;font-size:11px;margin-bottom:4px">未识别: '+escapeQuoteHtml(rec.unmatchedTokens.join(', '))+'</div>';
  }
  html+='</div>';
  document.body.insertAdjacentHTML('beforeend',html);
  setTimeout(()=>{document.addEventListener('click',closeRecognitionDetail,{once:true});},50);
}
function closeRecognitionDetail(){
  const el=document.getElementById('recDetailPopover');
  if(el)el.remove();
}
// 核心筛选：组内OR、组间AND
function applyIntentFilters(){
  const rows=D.boardIntent||[];
  intentFilterState.total=rows.length;
  const sbf=intentFilterState.selectedByField;
  const search=intentFilterState.search.trim().toUpperCase();
  const onlyUnmatched=intentFilterState.onlyUnmatched||false;
  const filtered=rows.filter(row=>{
    // P2: 仅显示未完整识别
    if(onlyUnmatched){
      const recJson=row.recognitionJson||row.recognition_json||'';
      let isRecognized=false;
      if(recJson){try{const rec=typeof recJson==='string'?JSON.parse(recJson):recJson;if(rec.status==='recognized')isRecognized=true;}catch(e){}}
      if(isRecognized)return false;
    }
    const conditions=parseIntentRowConditions(row);
    // 组间AND：每个已选fieldKey都必须满足
    for(const[fieldKey,selectedValues]of sbf.entries()){
      if(selectedValues.size===0)continue;
      // 组内OR：只需命中其中一个
      let hit=false;
      for(const v of selectedValues){if(conditions.has(fieldKey+':'+v.toUpperCase())){hit=true;break;}}
      if(!hit)return false;
    }
    // 搜索词AND
    if(search){
      const haystack=((row.content||'')+(row.result||'')+(row.inst||'')+(row.salesTr||'')+(row.trader||'')).toUpperCase();
      if(!haystack.includes(search))return false;
    }
    return true;
  });
  intentFilterState.filteredRows=filtered;
  intentFilterState.filteredTotal=filtered.length;
}
// P2: 仅显示未完整识别 toggle
function toggleIntentUnmatchedFilter(){
  intentFilterState.onlyUnmatched=!intentFilterState.onlyUnmatched;
  const btn=document.getElementById('intentUnmatchedToggle');
  if(btn){
    btn.style.background=intentFilterState.onlyUnmatched?'rgba(210,153,34,0.2)':'#21262d';
    btn.style.color=intentFilterState.onlyUnmatched?'#d29922':'#8b949e';
    btn.style.borderColor=intentFilterState.onlyUnmatched?'#d29922':'#30363d';
  }
  refreshIntentView();
}
// 收集面板选中条件
function collectIntentSelectedConditions(){
  intentFilterState.selectedByField.clear();
  const panel=document.getElementById('filters-intent');
  if(!panel)return;
  panel.querySelectorAll('.filter-tag.active[data-field-key][data-key]').forEach(tag=>{
    const fk=tag.getAttribute('data-field-key'),v=tag.getAttribute('data-key');
    if(!fk||!v)return;
    if(!intentFilterState.selectedByField.has(fk))intentFilterState.selectedByField.set(fk,new Set());
    intentFilterState.selectedByField.get(fk).add(v);
  });
  const regionSelect=document.getElementById('intentRegionSelect'),region=regionSelectionFromSelect(regionSelect);
  if(region){
    const values=region.level==='province'?[region.province,...region.descendants]:region.level==='city'?[region.city,...region.descendants]:[region.district];
    if(!intentFilterState.selectedByField.has('region_area'))intentFilterState.selectedByField.set('region_area',new Set());
    values.filter(Boolean).forEach(value=>intentFilterState.selectedByField.get('region_area').add(value));
  }
}
// P2: 互斥值配置
const INTENT_MUTEX_VALUES={
  'lgfv_attr':[['城投','非城投']],
  'tax_exempt':[['免税','非免税']],
  'embedded_option':[['含权','不含权']],
  'perpetual':[['城投永续','产业永续','金融永续','非永续']],
  'subordinated':[['次级','非次级']],
  'listing_status':[['上市','非上市']],
};
function checkIntentMutexWarning(){
  const warnings=[];
  for(const[fk,groups]of Object.entries(INTENT_MUTEX_VALUES)){
    const selected=intentFilterState.selectedByField.get(fk);
    if(!selected)continue;
    for(const group of groups){
      if(group.every(v=>selected.has(v))){
        warnings.push({fieldKey:fk,values:group});
      }
    }
  }
  return warnings;
}
function renderIntentMutexBanner(){
  const panel=document.getElementById('filters-intent');
  if(!panel)return;
  let banner=panel.querySelector('.intent-mutex-banner');
  const warnings=checkIntentMutexWarning();
  // 互斥标签样式
  panel.querySelectorAll('.filter-tag.mutex-warn').forEach(t=>t.classList.remove('mutex-warn'));
  if(warnings.length===0){if(banner)banner.remove();return;}
  warnings.forEach(w=>{
    w.values.forEach(v=>{
      const tag=panel.querySelector('.filter-tag[data-field-key="'+w.fieldKey+'"][data-key="'+v+'"]');
      if(tag)tag.classList.add('mutex-warn');
    });
  });
  const msgs=warnings.map(w=>'“'+w.values.join(' + ')+'” 已覆盖全部取值，等价于不筛选该条件');
  if(!banner){banner=document.createElement('div');banner.className='intent-mutex-banner';panel.appendChild(banner);}
  banner.innerHTML=msgs.join('<br>')+' <span style="cursor:pointer;margin-left:6px;opacity:.7" onclick="this.parentElement.remove()">✕</span>';
}
// 统一刷新入口
function refreshIntentView(){
  syncIntentDynamicTenorTags();
  buildIntentValueMap();
  collectIntentSelectedConditions();
  applyIntentFilters();
  renderTable('boardIntentBody',intentFilterState.filteredRows,[
    {key:'content',align:'left',render:renderBoardIntentContent},
    {key:'result',align:'left',render:renderIntentResultCell},
    {key:'inst'},{key:'salesTr'},{key:'trader'},
    {key:'time',type:'muted',render:formatBoardTime},
    {key:'status',render:r=>{const c=r.status==='有效'?'#3fb950':(r.status==='同步中'?'#58a6ff':'#ef4444');return'<span style="color:'+c+';font-weight:600;font-size:11px">'+(r.status||'')+'</span>';}},
  ]);
  fillEmptyRows('boardIntentBody');
  bindTableEvents();
  const ftEl=document.getElementById('intentFilteredTotal'),ttEl=document.getElementById('intentTotal');
  if(ftEl)ftEl.textContent=intentFilterState.filteredTotal;
  if(ttEl)ttEl.textContent=intentFilterState.total;
  updateIntentTagCounts();
  renderIntentMutexBanner();
}
// 分面计数
function updateIntentTagCounts(){
  const panel=document.getElementById('filters-intent');
  if(!panel)return;
  const rows=D.boardIntent||[];
  const parsedRows=rows.map(row=>({row,conditions:parseIntentRowConditions(row)}));
  panel.querySelectorAll('.filter-tag[data-field-key][data-key]').forEach(tag=>{
    const fk=tag.getAttribute('data-field-key'),v=tag.getAttribute('data-key');
    if(!fk||!v)return;
    // 标签数量只表示当前日期数据集中该条件自身的命中数，不受其他标签选择状态影响。
    const conditionKey=fk+':'+v.toUpperCase();
    const count=parsedRows.reduce((total,parsed)=>total+(parsed.conditions.has(conditionKey)?1:0),0);
    let badge=tag.querySelector('.filter-count-badge');
    if(!badge){badge=document.createElement('span');badge.className='filter-count-badge';badge.style.cssText='background:#3b82f6;color:#fff;border-radius:8px;padding:0 5px;font-size:9px;margin-left:3px;line-height:16px;font-weight:600';tag.appendChild(badge);}
    badge.textContent=count;
    tag.classList.toggle('has-results',count>0);
    if(count===0&&!tag.classList.contains('active')){tag.style.opacity='0.4';}
    else{tag.style.opacity='';}
  });
}
// 标签点击事件
function initIntentFilterPanel(){
  buildIntentValueMap();
  const panel=document.getElementById('filters-intent');
  if(!panel)return;
  panel.querySelectorAll('.filter-tag[data-field-key][data-key]').forEach(tag=>{tag.style.cursor='pointer';});
  if(panel.getAttribute('data-intent-filter-bound')==='1')return;
  panel.setAttribute('data-intent-filter-bound','1');
  panel.addEventListener('click',function(event){
    const tag=event.target.closest('.filter-tag[data-field-key][data-key]');
    if(!tag||!panel.contains(tag))return;
    event.preventDefault();event.stopPropagation();
    tag.classList.toggle('active');
    refreshIntentView();
  });
}
const INTENT_FIXED_TENORS=new Set(['<3M','3-6M','6-9M','9-12M','1-3Y','3-5Y','5-7Y','7-10Y','10-15Y','15-20Y','>20Y']);
function syncIntentDynamicTenorTags(){
  const panel=document.getElementById('filters-intent');if(!panel)return;
  const fixedLast=panel.querySelector('.filter-tag[data-field-key="remaining_tenor"][data-key=">20Y"]');
  const container=fixedLast&&fixedLast.parentElement;if(!container)return;
  const active=new Set(Array.from(container.querySelectorAll('.intent-dynamic-tenor.active')).map(tag=>tag.getAttribute('data-key')));
  container.querySelectorAll('.intent-dynamic-tenor').forEach(tag=>tag.remove());
  const values=new Set();
  (D.boardIntent||[]).forEach(row=>{
    const raw=row.recognitionJson||row.recognition_json||'';if(!raw)return;
    try{const rec=typeof raw==='string'?JSON.parse(raw):raw;(rec.conditions||[]).forEach(c=>{if(c.key==='remaining_tenor'&&c.normalizedValue&&!INTENT_FIXED_TENORS.has(c.normalizedValue.toUpperCase()))values.add(c.value||c.normalizedValue);});}catch(error){}
  });
  const rank=value=>{const match=String(value).match(/[\d.]+/);return match?Number(match[0]):99999;};
  [...values].sort((a,b)=>rank(a)-rank(b)||String(a).localeCompare(String(b))).forEach(value=>{
    const tag=document.createElement('span');tag.className='filter-tag intent-dynamic-tenor'+(active.has(value)?' active':'');tag.setAttribute('data-field-key','remaining_tenor');tag.setAttribute('data-key',value);tag.style.cursor='pointer';tag.textContent=value;tag.title='从当前日期范围的模糊报价中动态识别';container.appendChild(tag);
  });
}
// 清除过滤
function clearIntentFilters(){
  intentFilterState.search='';
  intentFilterState.selectedByField.clear();
  intentFilterState.onlyUnmatched=false;
  const panel=document.getElementById('filters-intent');
  if(panel){panel.querySelectorAll('.filter-tag.active').forEach(t=>t.classList.remove('active'));}
  const input=document.getElementById('intentSearchInput');
  if(input)input.value='';
  const regionSelect=document.getElementById('intentRegionSelect');if(regionSelect)regionSelect.value='';
  const unmatchedButton=document.getElementById('intentUnmatchedToggle');
  if(unmatchedButton){unmatchedButton.style.background='#21262d';unmatchedButton.style.color='#8b949e';unmatchedButton.style.borderColor='#30363d';}
  refreshIntentView();
}
// 搜索输入
function onIntentSearchInput(val){
  intentFilterState.search=val||'';
  refreshIntentView();
}
// P2: 管理员识别质量统计条
let intentStatsCache=null;
async function loadIntentStats(){
  const bar=document.getElementById('intentStatsBar');
  if(!bar)return;
  if(!isAdmin()){bar.style.display='none';return;}
  try{
    const dp=getIntentDateParams();
    const qs='?start_date='+encodeURIComponent(dp.start_date)+'&end_date='+encodeURIComponent(dp.end_date);
    const response=await fuzzyQuoteFetch('/stats'+qs);
    const body=await response.json();
    if(!response.ok||!body.success){bar.style.display='none';return;}
    intentStatsCache=body;
    renderIntentStatsBar(body);
  }catch(e){bar.style.display='none';}
}
function renderIntentStatsBar(s){
  const bar=document.getElementById('intentStatsBar');
  if(!bar)return;
  const accColor=s.accuracyRate>=90?'#3fb950':(s.accuracyRate>=70?'#d29922':'#f85149');
  let html='<span style="color:#8b949e;font-weight:600">识别质量</span>';
  html+='<span class="intent-stat-item"><span class="intent-stat-dot" style="background:#58a6ff"></span><span>已评估 <b style="color:#58a6ff">'+s.evaluated+'/'+s.total+'（'+s.coverageRate+'%）</b></span></span>';
  html+='<span class="intent-stat-item"><span class="intent-stat-dot" style="background:'+accColor+'"></span><span>完整 <b style="color:'+accColor+'">'+s.recognized+'/'+s.evaluated+'（'+s.accuracyRate+'%）</b></span></span>';
  html+='<span class="intent-stat-item"><span class="intent-stat-dot" style="background:#d29922"></span><span>部分 <b style="color:#d29922">'+s.partial+'/'+s.evaluated+'（'+s.partialRate+'%）</b></span></span>';
  html+='<span class="intent-stat-item"><span class="intent-stat-dot" style="background:#f0883e"></span><span>冲突 <b style="color:#f0883e">'+s.conflictRate+'%</b></span></span>';
  html+='<span class="intent-stat-item"><span class="intent-stat-dot" style="background:#f85149"></span><span>失败 <b style="color:#f85149">'+s.failedRate+'%</b></span></span>';
  html+='<span class="intent-stat-item" style="margin-left:auto"><span style="color:#8b949e">未评估 <b style="color:#e6edf3">'+s.unassessed+'</b>条</span></span>';
  bar.innerHTML=html;
  bar.style.display='flex';
}
// 初始化（页面加载后绑定事件）
if(document.readyState==='loading')document.addEventListener('DOMContentLoaded',initIntentFilterPanel);
else initIntentFilterPanel();

// ===== 关注债券 / 筛选分组 / 对价提醒 弹窗系统 =====
const favBondAlerts=[];
const filterGroupAlerts=[];
const priceMatchAlerts=[];

// 模拟：新增报价触发提醒
function checkAlerts(){
  favBondAlerts.length=0;filterGroupAlerts.length=0;priceMatchAlerts.length=0;
  (D.boardAlert||[]).forEach(alert=>{
    if(alert.alertType==='关注债券'){
      if(alert.bp)favBondAlerts.push({...alert,price:alert.bp,dir:'Bid',inst:alert.instBid,trader:alert.trBid,alertTime:alert.t});
      if(alert.op)favBondAlerts.push({...alert,price:alert.op,dir:'Ofr',inst:alert.instOfr,trader:alert.trOfr,alertTime:alert.t});
    }else if(alert.alertType==='筛选分组'){
      filterGroupAlerts.push(alert);
    }else if(alert.alertType==='对价提醒'){
      const bid=Number(alert.bp),ofr=Number(alert.op);
      priceMatchAlerts.push({...alert,alertTime:alert.t,spread:Number.isFinite(bid)&&Number.isFinite(ofr)?(bid-ofr).toFixed(2):'—'});
    }
  });
}

function showFavBondAlertModal(){
  checkAlerts();
  if(favBondAlerts.length===0){return;}
  let html='<div style="max-height:400px;overflow-y:auto">';
  html+='<table style="width:100%;border-collapse:collapse;font-size:11px">';
  html+='<thead><tr style="border-bottom:1px solid #30363d"><th style="padding:6px 8px;text-align:left;color:#8b949e">时间</th><th style="padding:6px;text-align:left;color:#8b949e">债券</th><th style="padding:6px;text-align:left;color:#8b949e">方向</th><th style="padding:6px;text-align:center;color:#8b949e">价格</th><th style="padding:6px;text-align:left;color:#8b949e">机构</th><th style="padding:6px;text-align:left;color:#8b949e">交易员</th></tr></thead><tbody>';
  favBondAlerts.slice(-10).reverse().forEach(rawAlert=>{
    const a=safeTableValue(rawAlert);
    const dirColor=a.dir==='Bid'?'#f59e0b':'#3b82f6';
    html+='<tr style="border-bottom:1px solid #21262d">';
    html+=`<td style="padding:6px 8px;color:#8b949e">${a.alertTime}</td>`;
    html+=`<td style="padding:6px;color:#79c0ff;font-weight:600">${a.n||a.c}</td>`;
    html+=`<td style="padding:6px;color:${dirColor};font-weight:600">${a.dir||'—'}</td>`;
    html+=`<td style="padding:6px;text-align:center;color:${dirColor};font-weight:700;font-family:'JetBrains Mono',monospace">${a.price||'—'}</td>`;
    html+=`<td style="padding:6px;color:#c9d1d9">${a.inst||'—'}</td>`;
    html+=`<td style="padding:6px;color:#c9d1d9">${a.trader||'—'}</td>`;
    html+='</tr>';
  });
  html+='</tbody></table></div>';
  html+='<div style="margin-top:8px;font-size:10px;color:#484f58">共 '+favBondAlerts.length+' 条关注债券提醒</div>';
  showModal('关注债券提醒',html);
}

function showFilterGroupAlertModal(){
  checkAlerts();
  const groupAlerts=D.boardAlert.filter(r=>r.alertType==='筛选分组');
  if(groupAlerts.length===0){return;}
  let html='<div style="max-height:400px;overflow-y:auto">';
  html+='<table style="width:100%;border-collapse:collapse;font-size:11px">';
  html+='<thead><tr style="border-bottom:1px solid #30363d"><th style="padding:6px 8px;text-align:left;color:#8b949e">债券</th><th style="padding:6px;text-align:center;color:#8b949e">Bid</th><th style="padding:6px;text-align:center;color:#8b949e">Ofr</th><th style="padding:6px;text-align:center;color:#8b949e">报量</th><th style="padding:6px;text-align:left;color:#8b949e">时间</th></tr></thead><tbody>';
  groupAlerts.forEach(rawAlert=>{
    const r=safeTableValue(rawAlert);
    html+='<tr style="border-bottom:1px solid #21262d">';
    html+=`<td style="padding:6px 8px"><span style="color:#79c0ff;font-weight:600">${r.n}</span><br><span style="color:#484f58;font-size:10px">${r.c}</span></td>`;
    html+=`<td style="padding:6px;text-align:center;color:#f59e0b;font-weight:700;font-family:'JetBrains Mono',monospace">${r.bp||'—'}</td>`;
    html+=`<td style="padding:6px;text-align:center;color:#3b82f6;font-weight:700;font-family:'JetBrains Mono',monospace">${r.op||'—'}</td>`;
    html+=`<td style="padding:6px;text-align:center;color:#22d3ee">${r.bvBid||r.bvOfr||'—'}</td>`;
    html+=`<td style="padding:6px;color:#8b949e">${r.t}</td>`;
    html+='</tr>';
  });
  html+='</tbody></table></div>';
  showModal('筛选分组提醒',html);
}

function showPriceMatchAlertModal(){
  checkAlerts();
  if(priceMatchAlerts.length===0){return;}
  let html='<div style="max-height:400px;overflow-y:auto">';
  html+='<table style="width:100%;border-collapse:collapse;font-size:11px">';
  html+='<thead><tr style="border-bottom:1px solid #30363d"><th style="padding:6px 8px;text-align:left;color:#8b949e">债券</th><th style="padding:6px;text-align:center;color:#8b949e">Bid</th><th style="padding:6px;text-align:center;color:#8b949e">Bid方</th><th style="padding:6px;text-align:center;color:#8b949e">Ofr</th><th style="padding:6px;text-align:center;color:#8b949e">Ofr方</th><th style="padding:6px;text-align:center;color:#ef4444">价差(BP)</th></tr></thead><tbody>';
  priceMatchAlerts.forEach(rawAlert=>{
    const a=safeTableValue(rawAlert);
    html+='<tr style="border-bottom:1px solid #21262d">';
    html+=`<td style="padding:6px 8px"><span style="color:#79c0ff;font-weight:600">${a.n}</span></td>`;
    html+=`<td style="padding:6px;text-align:center;color:#f59e0b;font-weight:700;font-family:'JetBrains Mono',monospace">${a.bp}</td>`;
    html+=`<td style="padding:6px;text-align:center;color:#c9d1d9;font-size:10px">${a.instBid||'—'} ${a.trBid||''}</td>`;
    html+=`<td style="padding:6px;text-align:center;color:#3b82f6;font-weight:700;font-family:'JetBrains Mono',monospace">${a.op}</td>`;
    html+=`<td style="padding:6px;text-align:center;color:#c9d1d9;font-size:10px">${a.instOfr||'—'} ${a.trOfr||''}</td>`;
    html+=`<td style="padding:6px;text-align:center;color:#ef4444;font-weight:700">${a.spread}</td>`;
    html+='</tr>';
  });
  html+='</tbody></table></div>';
  html+='<div style="margin-top:8px;font-size:10px;color:#8b949e">⚠️ 当Bid报价≥Ofr报价时触发对价提醒，可撮合成交</div>';
  showModal('对价提醒',html);
}

// ===== API配置 =====
function resolveApiBase(){
  // EXE 中央服务模式：由客户端注入的中央服务器地址（如 https://ficc.company.local）
  if(window.FICC_SERVER_URL){return String(window.FICC_SERVER_URL).replace(/\/+$/,'')+'/api';}
  // 浏览器局域网模式：页面由中央服务器提供时，用同源相对地址，API 请求自动发往同一服务器
  if(location.protocol==='http:'||location.protocol==='https:'){
    return '/api';
  }
  // file://（EXE 本机模式）或其他协议：连接本机后端
  return 'http://127.0.0.1:8080/api';
}
const API_BASE=resolveApiBase();

// ===== 债券资料弹出（通过后端API查询） =====
function showBondInfoPopup(e,code,name){
  e.stopPropagation();
  const popup=document.getElementById('bondInfoPopup');
  popup.style.display='block';
  document.getElementById('bondPopupTitle').textContent=name||code;
  document.getElementById('bondPopupCode').textContent=code;
  document.getElementById('bondPopupIssue').textContent='加载中...';
  document.getElementById('bondPopupOutstanding').textContent='...';
  document.getElementById('bondPopupMaturity').textContent='...';
  document.getElementById('bondPopupCoupon').textContent='...';
  document.getElementById('bondPopupRating').textContent='...';
  const rect=e.target.getBoundingClientRect();
  let left=rect.right+8;
  let top=rect.top;
  if(left+280>window.innerWidth)left=rect.left-280;
  if(top+200>window.innerHeight)top=window.innerHeight-210;
  popup.style.left=left+'px';
  popup.style.top=top+'px';
  authFetch(API_BASE+'/bond/info',{method:'POST',body:JSON.stringify({code:code})})
    .then(r=>r.json()).then(d=>{
      if(d.success&&d.data){
        const b=d.data;
        document.getElementById('bondPopupTitle').textContent=b.name||code;
        document.getElementById('bondPopupCode').textContent=b.code||code;
        document.getElementById('bondPopupIssue').textContent=b.issue_scale||'—';
        document.getElementById('bondPopupOutstanding').textContent=b.outstanding_scale||'—';
        document.getElementById('bondPopupMaturity').textContent=b.maturity_date||'—';
        document.getElementById('bondPopupCoupon').textContent=b.coupon_rate||'—';
        document.getElementById('bondPopupRating').textContent=b.rating||'—';
      }
    }).catch(()=>{
      document.getElementById('bondPopupIssue').textContent='查询失败';
    });
}
document.addEventListener('click',function(){document.getElementById('bondInfoPopup').style.display='none';});

// 事件委托：代码列点击
function bindCodeClickEvents(){
  ['boardCreditAllBody','boardRateAllBody','boardCreditBestBody','boardRateBestBody','boardAlertBody'].forEach(tid=>{
    const tbody=document.getElementById(tid);
    if(!tbody)return;
    tbody.addEventListener('click',function(e){
      const codeEl=e.target.closest('.code');
      if(!codeEl)return;
      e.stopPropagation();
      const tr=codeEl.closest('tr');
      if(!tr)return;
      const idx=Array.from(tbody.children).indexOf(tr);
      let data=D.boardCreditAll;
      if(tid==='boardRateAllBody')data=D.boardRateAll;
      else if(tid==='boardCreditBestBody')data=D.boardCreditBest;
      else if(tid==='boardRateBestBody')data=D.boardRateBest;
      else if(tid==='boardAlertBody')data=D.boardAlert;
      const r=data[idx];
      if(r)showBondInfoPopup(e,r.c,r.n);
    });
  });
}
bindCodeClickEvents();

// ===== 右键菜单全部操作实现 =====
function getCtxRowData(){
  if(!ctxTarget)return null;
  const map={'credit':D.boardCreditAll,'rate':D.boardRateAll,'creditBest':D.boardCreditBest,'rateBest':D.boardRateBest,'alert':D.boardAlert};
  const data=map[ctxTarget.type];
  return data?data[ctxTarget.idx]:null;
}
function ctxNewQuote(){
  if(!ctxTarget)return;
  const target={...ctxTarget};hideCtx();showNewQuoteModal(target.idx,target.type,'context-menu');
}
let pendingCtxGroupTarget=null,ctxReferTarget=null;
function contextFavoriteTarget(){
  if(!ctxTarget)return null;
  const row=getCtxRowData(),quote=quoteById(ctxTarget.quoteId)||row;if(!row&&!quote)return null;
  const source=quote||row,market=source.market==='rate'||ctxTarget.type&&ctxTarget.type.startsWith('rate')?'rate':'credit';
  return{market:market,c:normalizeBondCode(source.c||row&&row.c),n:String(source.n||row&&row.n||''),quoteId:ctxTarget.quoteId||source.quoteId||''};
}
function renderCtxFavoriteGroupMenu(row){
  const menu=document.getElementById('ctxSubGroup');if(!menu)return;
  const market=row&&row.market==='rate'||ctxTarget&&ctxTarget.type&&ctxTarget.type.startsWith('rate')?'rate':'credit';
  const groups=(market==='rate'?D.favBondGroupsRate:D.favBondGroupsCredit)||[];
  const groupHtml=groups.length?groups.map(group=>'<div class="ctx-item" onclick="addToFavGroup(\''+encodeURIComponent(group.id)+'\')">'+escapeQuoteHtml(group.name)+'</div>').join(''):'<div class="ctx-item disabled" style="color:#7d8590">暂无关注分组</div>';
  menu.innerHTML=groupHtml+'<div class="ctx-divider"></div><div class="ctx-item" onclick="showNewGroupModal()" style="color:#79c0ff">+ 新建分组</div>';
}
function refreshContextFavoriteViews(){
  D.favBonds=[...new Set((D.favBondGroupsCredit||[]).concat(D.favBondGroupsRate||[]).flatMap(group=>(group.bonds||[]).map(_fbmBondCode)))];
  _updateSidebarFavBonds();renderFilteredViews();
}
async function addToFavGroup(encodedGroupId){
  const target=contextFavoriteTarget();hideCtx();
  if(!target){showToast('error','未找到要添加的债券');return;}
  if(!authToken||!D.marketGroupLoaded||D.marketGroupReadOnly){showToast('error','当前分组为只读状态，请重新登录后操作');return;}
  const field=target.market==='rate'?'favBondGroupsRate':'favBondGroupsCredit',groupId=decodeURIComponent(encodedGroupId),group=(D[field]||[]).find(item=>item.id===groupId);
  if(!group){showToast('error','目标分组不存在或已更新');return;}
  if((group.bonds||[]).some(bond=>normalizeBondCode(_fbmBondCode(bond))===target.c))return;
  const previous=JSON.parse(JSON.stringify(D[field]||[]));
  group.bonds=group.bonds||[];group.bonds.push({code:target.c,name:target.n});
  try{await saveMarketGroupState();refreshContextFavoriteViews();}
  catch(error){D[field]=previous;refreshContextFavoriteViews();showToast('error',error.message||'添加到分组失败');}
}
function showNewGroupModal(){
  pendingCtxGroupTarget=contextFavoriteTarget();
  hideCtx();
  showModal('新建分组',`
    <div style="padding:8px 0">
      <label style="font-size:12px;color:#8b949e;display:block;margin-bottom:6px">分组名称</label>
      <input class="tb-input" style="width:100%" placeholder="输入分组名称" id="newGroupName">
    </div>
    <div class="modal-footer" style="padding:12px 0 0;border-top:1px solid #21262d;margin-top:12px;display:flex;gap:8px;justify-content:flex-end">
      <button class="tb-btn tb-btn-default" onclick="closeModal()">取消</button>
      <button class="tb-btn tb-btn-primary" style="background:#3b82f6;color:#fff" onclick="confirmNewGroup()">确定</button>
    </div>
  `);
}
async function confirmNewGroup(){
  const name=document.getElementById('newGroupName').value.trim();
  if(!name){showToast('error','请输入分组名称');return;}
  const target=pendingCtxGroupTarget;
  if(!target){showToast('error','未找到要添加的债券');return;}
  if(!authToken||!D.marketGroupLoaded||D.marketGroupReadOnly){showToast('error','当前分组为只读状态，请重新登录后操作');return;}
  const field=target.market==='rate'?'favBondGroupsRate':'favBondGroupsCredit';
  if((D[field]||[]).some(group=>String(group.name||'').trim()===name)){showToast('error','同名分组已存在');return;}
  const previous=JSON.parse(JSON.stringify(D[field]||[]));
  D[field].push({id:newLocalMarketGroupId(),name:name,bonds:target.c?[{code:target.c,name:target.n}]:[],kind:'favorite',market:target.market});
  try{await saveMarketGroupState();pendingCtxGroupTarget=null;refreshContextFavoriteViews();closeModal();}
  catch(error){D[field]=previous;refreshContextFavoriteViews();showToast('error',error.message||'新建分组失败');}
}
function ctxRefer(){
  if(!ctxTarget)return;
  const r=getCtxRowData();ctxReferTarget={...ctxTarget,row:r};
  hideCtx();
  showModal('Refer报价',`
    <div style="padding:8px 0">
      <div style="font-size:13px;color:#e6edf3;margin-bottom:12px">确认撤销以下报价？</div>
      <div style="background:#0d1117;border:1px solid #30363d;border-radius:6px;padding:12px;font-size:12px">
        <div style="margin-bottom:6px"><span style="color:#8b949e">债券：</span><span style="color:#79c0ff">${r?r.n:'—'}</span> <span style="color:#484f58;margin:0 4px">|</span> <span style="color:#484f58">${r?r.c:'—'}</span></div>
        <div><span style="color:#8b949e">报价：</span><span style="color:#e6edf3;font-weight:600">${r?(r.bp||r.op||'—'):'—'}</span> <span style="color:#8b949e;margin-left:8px">报量：</span><span style="color:#e6edf3">${r?r.bv||r.bvBid||r.bvOfr||'—':'—'}</span>万</div>
      </div>
    </div>
    <div class="modal-footer" style="padding:12px 0 0;border-top:1px solid #21262d;margin-top:12px;display:flex;gap:8px;justify-content:flex-end">
      <button class="tb-btn tb-btn-default" onclick="closeModal()">取消</button>
      <button class="tb-btn" style="background:#F53F3F;color:#fff" onclick="confirmRefer()">确认撤销</button>
    </div>
  `);
}
async function confirmRefer(){
  const target=ctxReferTarget;ctxReferTarget=null;
  if(!target||!target.quoteId){closeModal();showToast('error','未找到可撤销的正式报价');return;}
  if(!authToken){closeModal();showToast('error','应急模式为只读，认证服务恢复后才能撤销报价');return;}
  const quote=quoteById(target.quoteId);
  if(!quote||!canManageQuote(quote)){closeModal();showToast('error',quote?'只能撤销自己的报价':'未找到可撤销的正式报价');return;}
  try{
    const response=await authFetch(API_BASE+'/quotes/revoke',{method:'POST',body:JSON.stringify({quote_ids:[target.quoteId]})});
    const body=await response.json().catch(()=>({}));
    if(!response.ok||!body.success)throw new Error(typeof body.detail==='string'?body.detail:(body.detail&&body.detail.message)||'报价撤销失败');
    closeModal();
    const refreshed=await loadQuoteState();
    if(!refreshed)showToast('error','报价已撤销，但行情刷新失败，请手动刷新');
  }catch(error){showToast('error',error.message||'报价撤销失败，请重试');}
}
async function updateCtxQuoteFlag(field){
  if(!ctxTarget)return;
  const target={...ctxTarget},row=getCtxRowData(),quote=quoteById(target.quoteId);hideCtx();
  if(!authToken){showToast('error','应急模式为只读，认证服务恢复后才能更新报价');return;}
  if(!quote||row&&row.unquoted){showToast('error','未找到可更新的正式报价');return;}
  if(!canManageQuote(quote)){showToast('error','只能更新自己的报价');return;}
  const updated={...quote},enabled=toggleExclusiveQuoteFlag(updated,field);
  const label=field==='oco'?'OCO':'打包',dir=quoteDirectionOf(updated),side=quoteSideValues(updated,dir);
  const note=(enabled?'设置':'取消')+label+(enabled&&field==='oco'&&quote.packing?'（自动取消打包）':enabled&&field==='packing'&&quote.oco?'（自动取消OCO）':'');
  const event=quoteEventSnapshot(updated,'edit',note);event.oldPrice=side.price;event.price=side.price;event.volume=side.volume;
  try{
    await persistQuoteState(updated,'edit',event);
    const saved=updateMatchingQuoteCopies(quote,updated,target.type);
    recordQuoteLog(saved,'edit',dir,side.price,side.price,side.volume,note);
    D.quoteEvents=D.quoteEvents||[];D.quoteEvents.unshift(event);
    syncMgmtHistory();renderAll();
  }catch(error){
    showToast('error',error.message||label+'更新失败，请重试');
  }
}
function ctxPack(){return updateCtxQuoteFlag('packing');}
function ctxOCO(){return updateCtxQuoteFlag('oco');}
function quoteLogActionBadge(action){
  if(['cancel','revoke','withdraw'].includes(action))return['cancel','撤销'];
  if(action==='edit')return['edit','编辑'];
  return['quote','报价'];
}
function quoteLogRowsHtml(logs){
  if(!logs.length)return'<div class="quote-log-empty"><strong>暂无报价操作记录</strong><span>报价、编辑或撤销后，操作轨迹将显示在这里</span></div>';
  return `<div class="quote-log-wrap"><table class="quote-log-table"><thead><tr><th>时间</th><th>操作人</th><th>操作</th><th>方向</th><th>价格变动</th><th>报量</th><th>备注</th></tr></thead><tbody>${logs.map(log=>{
    const badge=quoteLogActionBadge(log.action);
    const direction=log.direction==='ofr'?'<span style="color:#58a6ff;font-weight:600">Ofr</span>':'<span style="color:#f59e0b;font-weight:600">Bid</span>';
    const price=log.oldPrice&&log.oldPrice!==log.price?escapeQuoteHtml(log.oldPrice)+' → '+escapeQuoteHtml(log.price):escapeQuoteHtml(log.price||'—');
    return `<tr><td class="quote-log-time">${escapeQuoteHtml(log.time)}</td><td>${escapeQuoteHtml(log.user||'—')}</td><td><span class="quote-log-badge ${badge[0]}">${badge[1]}</span></td><td>${direction}</td><td class="quote-log-price">${price}</td><td class="quote-log-price">${escapeQuoteHtml(log.volume||'—')}</td><td style="color:#8b949e">${escapeQuoteHtml(log.note||'—')}</td></tr>`;
  }).join('')}</tbody></table></div>`;
}
function ctxLog(){
  if(!ctxTarget)return;
  const target={...ctxTarget},data=boardDataByType(target.type),source=data&&data[target.idx];hideCtx();
  if(!source){showToast('error','未找到对应行情数据');return;}
  const quoteId=target.quoteId||source.sourceQuoteId||(source.bp?source.bidQuoteId:source.ofrQuoteId)||source.quoteId;
  const r=quoteById(quoteId)||source;
  const logs=ensureQuoteLogs(r);
  const body=`<div class="quote-log-modal">${quoteBondSummaryHtml(r)}${quoteLogRowsHtml(logs)}</div>`;
  showModal('报价日志',body,{wide:true,footerHtml:'<button class="tb-btn tb-btn-default" type="button" onclick="closeModal()">关闭</button>'});
  document.getElementById('modalBody').classList.add('quote-workflow-body');
}

// ===== 提醒入口按钮（挂在行情中心toolbar中触发） =====
function triggerFavBondAlerts(){showFavBondAlertModal();}
function triggerFilterGroupAlerts(){showFilterGroupAlertModal();}
function triggerPriceMatchAlerts(){showPriceMatchAlertModal();}

// 自动初始化一次提醒检查
setTimeout(()=>{checkAlerts();},1000);

// 报价管理页面功能函数
let mgmtDragSelecting=false,mgmtDragStartIdx=-1,mgmtDragTbody=null,mgmtSelectionMouseupBound=false;
function syncMgmtSelectionClasses(){
  const configs=[
    {tbodyId:'mgmtTodayBody',selected:mgmtTodaySelected},
    {tbodyId:'mgmtHistoryBody',selected:mgmtHistorySelected},
  ];
  configs.forEach(config=>{
    const tbody=document.getElementById(config.tbodyId);if(!tbody)return;
    tbody.querySelectorAll('tr[data-row-key]').forEach(element=>{
      const key=String(element.getAttribute('data-row-key')||'');
      const selected=key!==''&&config.selected.has(key);
      element.classList.toggle('selected',selected);
    });
  });
}
function bindMgmtSelectionEvents(){
  const configs=[
    {tbodyId:'mgmtTodayBody',rows:()=>D.mgmtToday||[],selected:mgmtTodaySelected,key:'quoteId'},
    {tbodyId:'mgmtHistoryBody',rows:()=>D.mgmtHistoryFiltered||D.mgmtHistory||[],selected:mgmtHistorySelected,key:'eventId'},
  ];
  configs.forEach(config=>{
    const tbody=document.getElementById(config.tbodyId);if(!tbody)return;
    tbody.onmousedown=function(event){
      if(event.button!==0||event.target.closest('input,button,a,select,textarea,[data-no-row-select]'))return;
      const tr=event.target.closest('tr[data-row-key]');if(!tr||!tbody.contains(tr))return;
      const key=String(tr.getAttribute('data-row-key')||'');
      const row=config.rows().find(item=>String(item[config.key])===key);
      if(!row)return;
      const realRows=Array.from(tbody.querySelectorAll('tr[data-row-key]'));
      mgmtDragSelecting=true;mgmtDragTbody=tbody;mgmtDragStartIdx=realRows.indexOf(tr);
      const additive=event.ctrlKey||event.metaKey||event.shiftKey;
      if(!additive)config.selected.clear();
      if(additive&&config.selected.has(key))config.selected.delete(key);else config.selected.add(key);
      syncMgmtSelectionClasses();event.preventDefault();
    };
    tbody.onmouseover=function(event){
      if(!mgmtDragSelecting||mgmtDragTbody!==tbody)return;
      const tr=event.target.closest('tr[data-row-key]');if(!tr||!tbody.contains(tr))return;
      const realRows=Array.from(tbody.querySelectorAll('tr[data-row-key]')),currentIdx=realRows.indexOf(tr);if(currentIdx<0)return;
      const start=mgmtDragStartIdx>=0?mgmtDragStartIdx:currentIdx,min=Math.min(start,currentIdx),max=Math.max(start,currentIdx);
      for(let index=min;index<=max;index++){
        const key=String(realRows[index].getAttribute('data-row-key')||'');if(key)config.selected.add(key);
      }
      syncMgmtSelectionClasses();
    };
  });
  if(!mgmtSelectionMouseupBound){
    document.addEventListener('mouseup',function(){mgmtDragSelecting=false;mgmtDragTbody=null;mgmtDragStartIdx=-1;});
    mgmtSelectionMouseupBound=true;
  }
}
function updateTraderDatalist(){
  refreshMgmtDatalists('today');
}
function setQuotesStatus(quoteIds,status,selectionSet){
  if(!authToken){showToast('error','应急模式为只读，认证服务恢复后才能操作报价');return null;}
  const result={changed:0,denied:0,unchanged:0,missing:0};
  [...new Set((quoteIds||[]).map(String))].forEach(function(quoteId){
    const quote=quoteById(quoteId);
    if(!quote){result.missing++;return;}
    if(!canManageQuote(quote)){result.denied++;return;}
    if(quote.status===status){result.unchanged++;return;}
    quote.status=status;const action=status==='revoked'?'revoke':'restore';
    const event=addQuoteEvent(quote,action,status==='revoked'?'报价撤销':'报价恢复');
    recordQuoteLog(quote,action,quoteDirectionOf(quote),quoteSideValues(quote,quoteDirectionOf(quote)).price,quoteSideValues(quote,quoteDirectionOf(quote)).price,quoteSideValues(quote,quoteDirectionOf(quote)).volume,event.note);
    persistQuoteState(quote,action,event);result.changed++;
  });
  if(selectionSet)selectionSet.clear();
  rebuildBoardProjections();syncMgmtToday();syncMgmtHistory();renderAll();
  return result;
}
function showMgmtStatusResult(result,actionText,unchangedText){
  if(!result)return;
  const details=[];
  if(result.changed)details.push('已'+actionText+' '+result.changed+' 条报价');
  if(result.denied)details.push('已跳过 '+result.denied+' 条无权限报价');
  if(result.unchanged)details.push(result.unchanged+' 条'+unchangedText);
  if(result.missing)details.push(result.missing+' 条报价已不存在');
  showToast(result.changed?'success':'error',details.join('；')||'没有可操作的报价');
}
function referFuzzy(){
  if(mgmtTodaySelected.size===0){showToast('error','请先选择报价数据');return;}
  showMgmtStatusResult(setQuotesStatus(Array.from(mgmtTodaySelected),'revoked',mgmtTodaySelected),'撤销','已处于撤销状态');
}
function unreferFuzzy(){
  if(mgmtTodaySelected.size===0){showToast('error','请先选择报价数据');return;}
  restoreManagedQuotes(Array.from(mgmtTodaySelected),mgmtTodaySelected);
}
async function restoreManagedQuotes(quoteIds,selectionSet){
  if(!authToken){showToast('error','应急模式为只读，认证服务恢复后才能恢复报价');return;}
  const ids=[...new Set((quoteIds||[]).map(String).filter(Boolean))];if(!ids.length){showToast('error','请先选择报价数据');return;}
  try{
    const response=await authFetch(API_BASE+'/quotes/restore',{method:'POST',body:JSON.stringify({quote_ids:ids,request_id:makeQuoteId('restore-request')})});
    let data={};try{data=await response.json();}catch(error){}
    if(!response.ok){
      if(response.status===404)throw new Error('恢复报价接口未加载：后端版本过旧或更新后尚未重启');
      const detail=data.detail;throw new Error(typeof detail==='string'?detail:(detail&&detail.message)||('恢复报价失败（HTTP '+response.status+'）'));
    }
    if(selectionSet)selectionSet.clear();
    await loadQuoteState();await loadMgmtToday();
    const restored=Number(data.restored||0),skipped=Array.isArray(data.skipped)?data.skipped:[];
    const details=[];if(restored)details.push('已生成 '+restored+' 条新报价');
    if(skipped.length){const reasons=[...new Set(skipped.map(item=>item.message).filter(Boolean))].slice(0,3);details.push('已跳过 '+skipped.length+' 条不可恢复报价'+(reasons.length?'（'+reasons.join('、')+'）':''));}
    showToast(restored?'success':'error',details.join('；')||'没有可恢复的报价');
  }catch(error){showToast('error',error.message||'恢复报价失败');}
}
async function deleteManagedQuotes(quoteIds,selectionSet){
  if(!authToken){showToast('error','应急模式为只读，认证服务恢复后才能删除报价');return null;}
  const result={deleted:0,denied:0,notRevoked:0,missing:0},deletable=[],retained=[];
  [...new Set((quoteIds||[]).map(String))].forEach(function(quoteId){
    const quote=quoteById(quoteId);
    if(!quote){result.missing++;return;}
    if(!canManageQuote(quote)){result.denied++;retained.push(quoteId);return;}
    if(quote.status!=='revoked'){result.notRevoked++;retained.push(quoteId);return;}
    deletable.push(quoteId);
  });
  if(deletable.length){
    const remoteDeleted=await deleteQuotesRemote(deletable);
    if(!remoteDeleted){showToast('error','删除失败，报价已保留，请检查网络或权限后重试');return result;}
    const ids=new Set(deletable);
    D.liveQuotes=D.liveQuotes.filter(q=>!ids.has(String(q.quoteId)));
    D.quoteEvents=D.quoteEvents.filter(e=>!ids.has(String(e.quoteId)));
    D.boardAlert=D.boardAlert.filter(a=>!ids.has(String(a.sourceQuoteId)));
    deletable.forEach(id=>{if(D.quoteLogs)delete D.quoteLogs[id];});
    result.deleted=deletable.length;
    if(selectionSet){selectionSet.clear();retained.forEach(id=>selectionSet.add(id));}
    rebuildBoardProjections();syncMgmtToday();syncMgmtHistory();renderAll();
  }
  if(result.deleted)showToast('success','已删除 '+result.deleted+' 条撤销报价');
  const failures=[];
  if(result.notRevoked)failures.push('仅支持删除撤销的报价数据（已保留 '+result.notRevoked+' 条）');
  if(result.denied)failures.push('已保留 '+result.denied+' 条无权限报价');
  if(result.missing)failures.push(result.missing+' 条报价已不存在');
  if(failures.length)showToast('error',failures.join('；'));
  else if(!result.deleted)showToast('error','没有可删除的报价');
  return result;
}
function deleteFuzzy(){
  if(mgmtTodaySelected.size===0){showToast('error','请先选择报价数据');return;}
  deleteManagedQuotes(Array.from(mgmtTodaySelected),mgmtTodaySelected);
}
function selectedHistoryQuoteIds(){
  return[...new Set((D.mgmtHistory||[]).filter(r=>mgmtHistorySelected.has(String(r.eventId))).map(r=>r.quoteId).filter(Boolean))];
}
function referHistory(){
  if(mgmtHistorySelected.size===0){showToast('error','请先选择报价数据');return;}
  showMgmtStatusResult(setQuotesStatus(selectedHistoryQuoteIds(),'revoked',mgmtHistorySelected),'撤销','已处于撤销状态');
}
function unreferHistory(){
  if(mgmtHistorySelected.size===0){showToast('error','请先选择报价数据');return;}
  restoreManagedQuotes(selectedHistoryQuoteIds(),mgmtHistorySelected);
}
function deleteHistory(){
  if(mgmtHistorySelected.size===0){showToast('error','请先选择报价数据');return;}
  deleteManagedQuotes(selectedHistoryQuoteIds(),mgmtHistorySelected);
}

function accountStatusText(status){return status==='active'?'启用':status==='disabled'?'停用':'锁定';}
function accountRoleText(role){return{admin:'管理员',technician:'技术员',trader:'交易员'}[role]||'交易员';}
function renderAccountTable(){
  const body=document.getElementById('accountTableBody');if(!body)return;
  const query=(document.getElementById('accountSearchInput')||{}).value||'',role=(document.getElementById('accountRoleFilter')||{}).value||'',status=(document.getElementById('accountStatusFilter')||{}).value||'';
  const rows=accountRows.filter(a=>(!query||(a.username+' '+a.name).toLowerCase().includes(query.toLowerCase()))&&(!role||a.role===role)&&(!status||a.status===status));
  body.innerHTML=rows.map(a=>`<tr><td style="text-align:left;font-family:'JetBrains Mono',monospace">${escapeQuoteHtml(a.username)}</td><td>${escapeQuoteHtml(a.name)}</td><td><span class="tag tag-${a.role==='admin'?'firm':'bid'}">${accountRoleText(a.role)}</span></td><td><span style="color:${a.status==='active'?'#3fb950':'#d29922'}">${accountStatusText(a.status)}</span></td><td>${a.source==='external'?'外部同步':'Mock'}</td><td class="muted">${escapeQuoteHtml(a.externalAccountId||'—')}</td><td>${escapeQuoteHtml(a.department||'—')}</td><td class="muted">${escapeQuoteHtml(a.lastSyncedAt||'—')}</td><td class="muted">${escapeQuoteHtml(a.lastLoginAt||'—')}</td><td><button class="tb-btn tb-btn-default" onclick="showEditAccountModal('${escapeQuoteHtml(a.accountId)}')">编辑</button></td></tr>`).join('');
  if(!rows.length)body.innerHTML='<tr><td colspan="10" class="muted" style="height:80px">暂无符合条件的账户</td></tr>';
  const count=document.getElementById('accountCountText');if(count)count.textContent=rows.length+' 个账户';
}
function loadAccounts(){
  if(!isAdmin())return Promise.resolve(false);
  const body=document.getElementById('accountTableBody');if(body)body.innerHTML='<tr><td colspan="10" class="muted" style="height:80px">正在加载账户数据...</td></tr>';
  return authFetch(API_BASE+'/accounts').then(async r=>{let d={};try{d=await r.json();}catch(e){}if(!r.ok)throw new Error(d.detail||'HTTP '+r.status);return d;}).then(d=>{accountRows=d.accounts||[];renderAccountTable();return true;}).catch(error=>{accountRows=[];if(body)body.innerHTML='<tr><td colspan="10" style="height:80px;color:#ef4444">账户服务不可用，请退出后重新登录；系统会自动恢复后端服务。</td></tr>';const count=document.getElementById('accountCountText');if(count)count.textContent='加载失败';showToast('error',error.message||'账户数据加载失败');return false;});
}
function accountFormHtml(account){
  const a=account||{},isNew=!account,isExternal=a.source==='external';
  const role=a.role||'trader';
  return`<div style="display:grid;grid-template-columns:1fr 1fr;gap:14px 18px;padding:4px 0"><div class="quote-field"><label>登录账号</label><input class="quote-control" id="accountUsername" value="${escapeQuoteHtml(a.username||'')}" ${isNew?'':'disabled'}></div><div class="quote-field"><label>姓名</label><input class="quote-control" id="accountDisplayName" value="${escapeQuoteHtml(a.name||'')}" ${isExternal?'disabled':''}></div><div class="quote-field"><label>角色</label><select class="quote-control" id="accountRole"><option value="trader" ${role==='trader'?'selected':''}>交易员</option><option value="technician" ${role==='technician'?'selected':''}>技术员</option><option value="admin" ${role==='admin'?'selected':''}>管理员</option></select></div><div class="quote-field"><label>状态</label><select class="quote-control" id="accountStatus"><option value="active" ${a.status==='active'||isNew?'selected':''}>启用</option><option value="disabled" ${a.status==='disabled'?'selected':''}>停用</option><option value="locked" ${a.status==='locked'?'selected':''}>锁定</option></select></div><div class="quote-field"><label>部门</label><input class="quote-control" id="accountDepartment" value="${escapeQuoteHtml(a.department||'FICC销售部')}"></div><div class="quote-field"><label>${isNew?'初始密码':'重置密码'}</label><input class="quote-control" id="accountPassword" type="password" placeholder="${isNew?'至少6位':'留空表示不修改'}" ${isExternal?'disabled':''}></div><div class="quote-field"><label>邮箱</label><input class="quote-control" id="accountEmail" value="${escapeQuoteHtml(a.email||'')}"></div><div class="quote-field"><label>手机</label><input class="quote-control" id="accountPhone" value="${escapeQuoteHtml(a.phone||'')}"></div></div>`;
}
function showCreateAccountModal(){
  const footer='<button class="tb-btn tb-btn-default" onclick="closeModal()">取消</button><button class="tb-btn tb-btn-primary" onclick="saveAccountForm()">创建账户</button>';
  showModal('新增 Mock 账户',accountFormHtml(null),{wide:true,footerHtml:footer,initialFocus:'#accountUsername'});
}
function showEditAccountModal(accountId){
  const account=accountRows.find(a=>a.accountId===accountId);if(!account)return;
  window._editingAccountId=accountId;const footer='<button class="tb-btn tb-btn-default" onclick="closeModal()">取消</button><button class="tb-btn tb-btn-primary" onclick="saveAccountForm(\''+escapeQuoteHtml(accountId)+'\')">保存</button>';
  showModal('编辑账户',accountFormHtml(account),{wide:true,footerHtml:footer,initialFocus:'#accountDisplayName'});
}
function saveAccountForm(accountId){
  const username=quoteFieldValue('accountUsername'),displayName=quoteFieldValue('accountDisplayName'),password=quoteFieldValue('accountPassword');
  if(!accountId&&(!username||!displayName||password.length<6)){showToast('error','账号、姓名不能为空，初始密码至少6位');return;}
  const existing=accountId?accountRows.find(a=>a.accountId===accountId):null;
  const payload={role:quoteFieldValue('accountRole'),status:quoteFieldValue('accountStatus'),department:quoteFieldValue('accountDepartment'),email:quoteFieldValue('accountEmail'),phone:quoteFieldValue('accountPhone')};if(!existing||existing.source!=='external')payload.display_name=displayName;if(password)payload.password=password;if(!accountId){payload.username=username;payload.password=password;}
  authFetch(API_BASE+'/accounts'+(accountId?'/'+encodeURIComponent(accountId):''),{method:accountId?'PATCH':'POST',body:JSON.stringify(payload)}).then(async r=>{const d=await r.json();if(!r.ok)throw new Error(d.detail||'保存失败');return d;}).then(()=>{closeModal();showToast('success',accountId?'账户已更新':'Mock 账户已创建');loadAccounts();}).catch(e=>showToast('error',e.message));
}

function showCommitModal(){
  showModal('提交变更','<div style="padding:8px 0"><label style="font-size:12px;color:#8b949e;display:block;margin-bottom:6px">提交类型</label><select class="tb-select" style="width:100%;margin-bottom:8px" id="commitType"><option value="feat">feat: 新增功能</option><option value="fix">fix: 修复Bug</option><option value="refactor">refactor: 代码重构</option><option value="docs">docs: 文档更新</option><option value="chore">chore: 工程配置</option></select><label style="font-size:12px;color:#8b949e;display:block;margin-bottom:6px">提交信息</label><input class="tb-input" style="width:100%;margin-bottom:8px" id="commitMsg" placeholder="描述变更内容"><label style="font-size:12px;color:#8b949e;display:block;margin-bottom:6px">用户需求</label><textarea class="tb-input" style="width:100%;height:60px;resize:vertical;margin-bottom:8px;font-size:12px" id="commitUserReq" placeholder="用户提出的优化需求或问题描述"></textarea><label style="font-size:12px;color:#8b949e;display:block;margin-bottom:6px">优化方案 & 改动内容</label><textarea class="tb-input" style="width:100%;height:80px;resize:vertical;font-size:12px" id="commitChanges" placeholder="本次优化的方案、改动的文件和具体内容"></textarea></div><div style="padding:12px 0 0;border-top:1px solid #21262d;margin-top:12px;display:flex;gap:8px;justify-content:flex-end"><button class="tb-btn tb-btn-default" onclick="closeModal()">取消</button><button class="tb-btn" style="background:#1d4ed8;color:#fff;font-weight:600" onclick="confirmCommit()">确认提交</button></div>');
}
function confirmCommit(){
  var type=document.getElementById('commitType').value;
  var msg=document.getElementById('commitMsg').value.trim();
  if(!msg){showToast('error','请输入提交信息');return;}
  var userReq=document.getElementById('commitUserReq').value.trim();
  var changes=document.getElementById('commitChanges').value.trim();
  var body='';
  if(userReq)body+='【用户需求】\n'+userReq;
  if(changes)body+=(body?'\n\n':'')+'【优化方案 & 改动内容】\n'+changes;
  closeModal();
  authFetch(API_BASE+'/git/commit',{method:'POST',headers:{'Content-Type':'application/json'},body:JSON.stringify({message:type+': '+msg,body:body})}).then(function(r){return r.json();}).then(function(d){if(d.success){loadGitLog();}else showToast('error',d.error||'提交失败');}).catch(function(){showToast('error','提交失败');});
}
function doGitBackup(){
  authFetch(API_BASE+'/git/backup',{method:'POST'}).then(function(r){return r.json();}).then(function(d){if(d.success){loadGitLog();}else showToast('error',d.error||'备份失败');}).catch(function(){showToast('error','备份失败');});
}
var _gitLogCount=20;
var _gitReadOnly=false;
// 版本页存储型XSS防护：commit的message/author/date/body/hash均为外部输入，
// 全部经escapeQuoteHtml转义后再进入innerHTML；回滚按钮不再拼接inline onclick，
// 改用data-revert-hash属性+事件委托（见bindGitLogEvents），杜绝属性注入。
function _renderGitCommit(c){
  var esc=escapeQuoteHtml;
  var hash=String(c.hash||'');
  var safeHash=esc(hash.substring(0,7));
  var html='<div style="padding:10px 0;border-bottom:1px solid #21262d">';
  html+='<div style="display:flex;align-items:center;gap:8px;margin-bottom:4px">';
  html+='<code style="background:#21262d;color:#3b82f6;padding:2px 6px;border-radius:4px;font-size:11px">'+safeHash+'</code>';
  html+='<span style="font-size:12px;color:#e6edf3;font-weight:600">'+esc(c.message)+'</span></div>';
  if(c.body){
    var bodyLines=String(c.body).split('\n').filter(function(l){return l.trim();});
    var userReq='';var changes='';var section='';
    bodyLines.forEach(function(line){
      if(line.indexOf('【用户需求】')>=0){section='req';return;}
      if(line.indexOf('【优化方案')>=0){section='changes';return;}
      if(section==='req')userReq+=(userReq?'\n':'')+line;
      else if(section==='changes')changes+=(changes?'\n':'')+line;
    });
    if(!userReq&&!changes)userReq=String(c.body);
    html+='<div style="margin:6px 0 4px;padding:8px 10px;background:#0d1117;border:1px solid #21262d;border-radius:6px;font-size:11px">';
    if(userReq){html+='<div style="margin-bottom:6px"><span style="color:#f59e0b;font-weight:600;font-size:10px">用户需求</span><div style="color:#c9d1d9;white-space:pre-wrap;margin-top:3px;line-height:1.6">'+esc(userReq)+'</div></div>';}
    if(changes){html+='<div><span style="color:#3fb950;font-weight:600;font-size:10px">优化方案 & 改动</span><div style="color:#c9d1d9;white-space:pre-wrap;margin-top:3px;line-height:1.6">'+esc(changes)+'</div></div>';}
    html+='</div>';
  }
  html+='<div style="display:flex;justify-content:space-between;align-items:center;font-size:11px;color:#8b949e">';
  html+='<span>'+esc(c.author)+'</span><span>'+esc(String(c.date||'').substring(0,19))+'</span>';
  if(!_gitReadOnly)html+='<button type="button" class="tb-btn" data-revert-hash="'+esc(hash)+'" style="background:rgba(239,68,68,0.12);color:#ef4444;border:1px solid rgba(239,68,68,0.3);font-size:10px;padding:2px 8px;cursor:pointer">回滚到此版本</button>';
  html+='</div></div>';
  return html;
}
// 事件委托绑定版本列表回滚按钮（只绑一次，列表重渲染后仍生效）
var _gitLogEventsBound=false;
function bindGitLogEvents(){
  if(_gitLogEventsBound)return;
  var el=document.getElementById('gitLogList');
  if(!el)return;
  el.addEventListener('click',function(e){
    var target=e.target&&e.target.closest?e.target.closest('[data-revert-hash],[data-git-loadmore]'):null;
    if(!target)return;
    if(target.hasAttribute('data-git-loadmore')){_gitLogCount+=20;loadGitLog(true);return;}
    doGitRevert(target.getAttribute('data-revert-hash')||'');
  });
  _gitLogEventsBound=true;
}
function _renderVersionChart(counts,labels){
  var canvas=document.getElementById('versionChart');if(!canvas)return;
  var dpr=window.devicePixelRatio||1;
  var rect=canvas.getBoundingClientRect();
  canvas.width=rect.width*dpr;canvas.height=rect.height*dpr;
  var ctx=canvas.getContext('2d');ctx.scale(dpr,dpr);
  var w=rect.width,h=rect.height;
  var padL=40,padR=20,padT=30,padB=30;
  var chartW=w-padL-padR,chartH=h-padT-padB;
  ctx.clearRect(0,0,w,h);
  var maxVal=Math.max.apply(null,counts.concat([1]));
  if(maxVal<5)maxVal=5;
  var stepY=chartH/maxVal;
  ctx.strokeStyle='#30363d';ctx.lineWidth=1;
  for(var i=0;i<=4;i++){
    var y=padT+chartH-i*(chartH/4);
    ctx.beginPath();ctx.moveTo(padL,y);ctx.lineTo(w-padR,y);ctx.stroke();
    ctx.fillStyle='#484f58';ctx.font='10px sans-serif';ctx.textAlign='right';
    ctx.fillText(Math.round(maxVal/4*i),padL-6,y+3);
  }
  var pts=[];
  counts.forEach(function(v,idx){
    var x=padL+idx*(chartW/(counts.length-1||1));
    var y=padT+chartH-v*stepY;
    pts.push({x:x,y:y,v:v});
  });
  if(pts.length>1){
    ctx.beginPath();ctx.moveTo(pts[0].x,pts[0].y);
    for(var i=1;i<pts.length;i++)ctx.lineTo(pts[i].x,pts[i].y);
    ctx.strokeStyle='#3b82f6';ctx.lineWidth=2;ctx.stroke();
    ctx.lineTo(pts[pts.length-1].x,padT+chartH);ctx.lineTo(pts[0].x,padT+chartH);ctx.closePath();
    ctx.fillStyle='rgba(59,130,246,0.1)';ctx.fill();
  }
  pts.forEach(function(p){
    ctx.beginPath();ctx.arc(p.x,p.y,5,0,Math.PI*2);ctx.fillStyle='#3b82f6';ctx.fill();
    ctx.beginPath();ctx.arc(p.x,p.y,2.5,0,Math.PI*2);ctx.fillStyle='#0d1117';ctx.fill();
    ctx.fillStyle='#e6edf3';ctx.font='bold 11px sans-serif';ctx.textAlign='center';
    ctx.fillText(p.v,p.x,p.y-10);
  });
  ctx.fillStyle='#8b949e';ctx.font='10px sans-serif';ctx.textAlign='center';
  labels.forEach(function(l,idx){
    var x=padL+idx*(chartW/(labels.length-1||1));
    ctx.fillText(l,x,h-6);
  });
}
function _loadChartStats(){
  authFetch(API_BASE+'/git/log?count=9999').then(function(r){return r.json();}).then(function(d){
    if(!d.success||!d.commits)return;
    var uc=document.getElementById('versionUpdateCount');if(uc)uc.textContent=d.count;
    var dayMap={};var workDays=[];var dd=new Date();
    while(workDays.length<5){
      var dow=dd.getDay();
      if(dow!==0&&dow!==6){
        var key=dd.getFullYear()+'-'+String(dd.getMonth()+1).padStart(2,'0')+'-'+String(dd.getDate()).padStart(2,'0');
        dayMap[key]=0;
        workDays.push({key:key,label:String(dd.getMonth()+1)+'/'+String(dd.getDate())});
      }
      dd.setDate(dd.getDate()-1);
    }
    d.commits.forEach(function(c){
      var ds=c.date?c.date.substring(0,10):'';
      if(dayMap.hasOwnProperty(ds))dayMap[ds]++;
    });
    workDays.reverse();
    var counts=workDays.map(function(d){return dayMap[d.key];});
    var labels=workDays.map(function(d){return d.label;});
    _renderVersionChart(counts,labels);
  });
}
function loadGitLog(append){
  var el=document.getElementById('gitLogList');if(el&&!append)el.innerHTML='<div style="text-align:center;padding:20px;color:#484f58">正在加载版本记录...</div>';
  authFetch(API_BASE+'/git/log?count='+_gitLogCount).then(async function(r){var d={};try{d=await r.json();}catch(e){}if(!r.ok)throw new Error(d.detail||'HTTP '+r.status);return d;}).then(function(d){
    var el=document.getElementById('gitLogList');if(!el)return;
    if(!d.success){throw new Error(d.error||'加载失败');}
    _gitReadOnly=!!d.read_only;
    var backupButton=document.getElementById('versionBackupButton'),commitButton=document.getElementById('versionCommitButton'),modeBadge=document.getElementById('versionModeBadge');
    if(backupButton)backupButton.style.display=_gitReadOnly?'none':'';
    if(commitButton)commitButton.style.display=_gitReadOnly?'none':'';
    if(modeBadge)modeBadge.textContent=_gitReadOnly?'只读版本':'Git';
    var buildInfo=document.getElementById('versionBuildInfo');
    if(buildInfo){var b=d.build;buildInfo.textContent=b&&b.buildId?('Build '+b.buildId+' · '+(b.signed?'已签名':'测试未签名')+(b.dirty?' · dirty':'')):'源码运行模式 · 构建后生成SHA-256清单';}
    var cc=document.getElementById('versionCommitCount');if(cc)cc.textContent=d.count;
    _loadChartStats();
    if(!d.commits||d.commits.length===0){
      if(append)return;
      el.innerHTML='<div style="padding:16px;text-align:center;color:#484f58">暂无提交记录</div>';
      return;
    }
    var html='';
    d.commits.forEach(function(c){html+=_renderGitCommit(c);});
    if(d.commits.length>=_gitLogCount){
      html+='<div style="padding:12px 0;text-align:center"><button type="button" class="tb-btn tb-btn-default" data-git-loadmore style="font-size:12px">加载更多</button></div>';
    }
    if(append){
      var btn=el.querySelector('[data-git-loadmore]');
      if(btn)btn.parentElement.remove();
      el.insertAdjacentHTML('beforeend',html);
    }else{
      el.innerHTML=html;
    }
    bindGitLogEvents();
  }).catch(function(error){var el=document.getElementById('gitLogList');if(el)el.innerHTML='<div style="padding:16px;color:#ef4444;font-size:12px">版本服务不可用，请退出后重新登录；系统会自动恢复后端服务。<div style="margin-top:6px;color:#8b949e">'+escapeQuoteHtml(error.message||'加载失败')+'</div></div>';var cc=document.getElementById('versionCommitCount');if(cc)cc.textContent='加载失败';});
}
async function doGitRevert(hash){
  // hash格式白名单：仅允许7~40位十六进制，拒绝任何非法值（防参数注入）
  hash=String(hash||'').trim();
  if(!/^[0-9a-fA-F]{7,40}$/.test(hash)){showToast('error','无效的提交哈希');return;}
  if(!await systemConfirm('版本回滚','确认回滚到版本 '+hash.substring(0,7)+' ？\n回滚前会自动备份当前状态。',{confirmText:'确认回滚'}))return;
  authFetch(API_BASE+'/git/revert',{method:'POST',headers:{'Content-Type':'application/json'},body:JSON.stringify({hash:hash})}).then(function(r){return r.json();}).then(function(d){if(d.success){loadGitLog();location.reload();}else showToast('error',d.error||'回滚失败');}).catch(function(){showToast('error','回滚失败');});
}
