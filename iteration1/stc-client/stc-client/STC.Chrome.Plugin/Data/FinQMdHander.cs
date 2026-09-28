using com.gateway.log;
using Newtonsoft.Json;
using STC.Chrome.Plugin.ViewModel.Topo;
using STC.Common.Handler.Base;
using STC.Common.Model.Base;
using STC.Common.Model.FinQ;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.QQ;
using STC.Common.Utils;
using STC.Common.View.Component;
using STC.Common.View.UI;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Threading;

namespace STC.Chrome.Plugin.Data
{
    public class FinQMdHander
    {
        #region 构造
        /// <summary>
        /// FinQMdHander
        /// </summary>
        private static FinQMdHander instance = new FinQMdHander();

        public static FinQMdHander Instance { get { return instance; } }

        #region 中债估值
        private Visibility _CCDCVisible = Visibility.Collapsed;
        /// <summary>
        /// 中债估值 是否可见
        /// </summary>
        public Visibility CCDCVisible
        {
            get
            {
                return _CCDCVisible;
            }
            set
            {
                _CCDCVisible = value;
            }
        }

        private string _CCDCValuaDate;
        public string CCDCValuaDate
        {
            get { return _CCDCValuaDate; }

            set
            {
                if (_CCDCValuaDate != value)
                {
                    _CCDCValuaDate = value;
                }
            }
        }

        private string _CCDCValuation;
        public string CCDCValuation
        {
            get { return _CCDCValuation; }
            set
            {
                if (_CCDCValuation != value)
                {
                    _CCDCValuation = value;
                }
            }
        }

        private string _CCDCValuation2;
        public string CCDCValuation2
        {
            get { return _CCDCValuation2; }
            set
            {
                if (_CCDCValuation2 != value)
                {
                    _CCDCValuation2 = value;
                }
            }
        }

        private string _CCDCValuationToolTip;
        public string CCDCValuationToolTip
        {
            get { return _CCDCValuationToolTip; }
            set
            {
                if (_CCDCValuationToolTip != value)
                {
                    _CCDCValuationToolTip = value;
                }
            }
        }


        private string _SendingTime;
        public string SendingTime
        {
            get { return _SendingTime; }
            set
            {
                if (_SendingTime != value)
                {
                    _SendingTime = value;
                    LastSendingTime = !string.IsNullOrWhiteSpace(value) ?  DateTimeEx.ConvertStringToDateTime(value).ToLongTimeString() : "";
                }
            }
        }

        public string _LastSendingTime = "";
        public string LastSendingTime
        {
            get { return _LastSendingTime; }
            set
            {
                _LastSendingTime = value;
            }
        }


        #endregion

        private int _clickdatatype;
        /// <summary>
        /// 0或null：点击单家最优；1：点击综合最优；2：点击成交价
        /// </summary>
        public int ClickDataType
        {
            get { return _clickdatatype; }
            set
            {
                _clickdatatype = value;
            }
        }


        private string _brokeid;
        /// <summary>
        /// BrokeId
        /// </summary>
        public string BrokeId
        {
            get { return _brokeid; }
            set
            {
                _brokeid = value;
            }
        }

        private string _side;
        /// <summary>
        /// Side
        /// </summary>
        public string Side
        {
            get { return _side; }
            set
            {
                _side = value;
            }
        }

        private string _selSettle;
        /// <summary>
        /// 用户选择的清算方式，用于组合盘口数据，数据格式为 全,+1,+0,远
        /// </summary>
        public string SelSettle
        {
            get { return _selSettle; }
            set
            {
                _selSettle = value;
            }
        }

        private string _securityid;
        public string SecurityID
        {
            get { return _securityid; }
            set
            {
                if (_securityid == value)
                {
                    // 相同则不需要处理
                    return;
                }

                FQSetActionData = "{}";
                FQChangeBondData = "{}";
                FQBondInfo = null;
                DictCTPrice.Clear();

                // 更新并订阅新值
                _securityid = value;

                BondsInfoPo fqbondsInfo = CommonUtils.GetBondsInfoPoBySecurityID(value);
                if (fqbondsInfo == null)
                {
                    return;
                }
               
                STC.Common.Model.FinQ.BondInfo fdbondinfo = new STC.Common.Model.FinQ.BondInfo();
                BondsInfo = fqbondsInfo;
                fdbondinfo.bond_rating_cur_code = fqbondsInfo.Rating_Current;
                fdbondinfo.bond_typ2 = CommonUtils.GetBondCategory(fqbondsInfo.Bond_Subtype, fqbondsInfo.Ent_Cor);
                fdbondinfo.iss_sz = fqbondsInfo.Issue_Amount?.ToString();
                fdbondinfo.issuer_rating = fqbondsInfo.Rating_Current_NPY;
                fdbondinfo.nxt_strike_dt = fqbondsInfo.Option_Date?.ToString();
                fdbondinfo.otstd_amnt = ""; //债券余额
                fdbondinfo.pinyin_short = fqbondsInfo.pinYin;
                fdbondinfo.residual_maturity = fqbondsInfo.RemaingingTime;
                fdbondinfo.sec_code_par = fqbondsInfo.Bond_ID;
                fdbondinfo.sec_shrt_nm = fqbondsInfo.Short_Name;
                fdbondinfo.securityType = fqbondsInfo.SECURITY_TYPE;
                FQBondInfo = fdbondinfo;
                DictCTPrice.Clear();
                PriceHandler.Instance.RegisterCTPrice(_securityid, Instance_OnMDPrice);
            }
        }

        private BondsInfoPo _bondsInfo;
        /// <summary>
        /// 债券的原始信息
        /// </summary>
        public BondsInfoPo BondsInfo
        {
            get { return _bondsInfo; }
            set
            {
                _bondsInfo = value;
            }
        }

        private STC.Common.Model.FinQ.BondInfo fqbondinfo;
        /// <summary>
        /// 发送给FinQ债券信息
        /// </summary>
        public STC.Common.Model.FinQ.BondInfo FQBondInfo
        {
            get { return fqbondinfo; }
            set
            {
                fqbondinfo = value;
            }
        }


        private object lockbidObj = new object();
        private object lockofrObj = new object();

        public ObservableCollection<GoodsVolumeVoModel> _bidquotations = new ObservableCollection<GoodsVolumeVoModel>();
        /// <summary>
        /// 原始全部Bid深度行情
        /// </summary>
        public ObservableCollection<GoodsVolumeVoModel> BidQuotations
        {
            get
            {
                return _bidquotations;
            }
            set
            {
                _bidquotations = value;
            }
        }

        public ObservableCollection<GoodsVolumeVoModel> _ofrquotations = new ObservableCollection<GoodsVolumeVoModel>();

        /// <summary>
        /// 原始全部Ofr深度行情
        /// </summary>
        public ObservableCollection<GoodsVolumeVoModel> OfrQuotations
        {
            get
            {
                return _ofrquotations;
            }
            set
            {
                _ofrquotations = value;
            }
        }

        private ObservableCollection<GoodsVolumeVoModel> _BestBidQuotations = new ObservableCollection<GoodsVolumeVoModel>();
        /// <summary>
        /// Bid 最优价列表
        /// </summary>
        public ObservableCollection<GoodsVolumeVoModel> BestBidQuotations
        {
            get
            {
                return _BestBidQuotations;
            }
            set
            {
                _BestBidQuotations = value;
            }
        }


        private ObservableCollection<GoodsVolumeVoModel> _BestOfrQuotations = new ObservableCollection<GoodsVolumeVoModel>();
        /// <summary>
        /// Ofr 最优价列表
        /// </summary>
        public ObservableCollection<GoodsVolumeVoModel> BestOfrQuotations
        {
            get
            {
                return _BestOfrQuotations;
            }
            set
            {
                _BestOfrQuotations = value;
            }
        }

        private bool isNeedSend = false;

        private object fqSetActionData;

        /// <summary>
        /// 盘口数据
        /// </summary>
        public object FQSetActionData
        {
            get { return fqSetActionData; }
            set
            {
                if (value.ToString() == "{}")
                {
                    fqSetActionData = value;
                    return;
                }

                fqSetActionData = value;
                if (isNeedSend)
                {
                    isNeedSend = false;
                    Common.Handler.Base.BaseHandler.Instance.PublishAppMsg(new PResult() { Code = "getFQ", Info1 = "WebFQ", Info2 = "FQSetAction", Info3 = "" });
                }
            }
        }

        private object fqChangeBondData;

        /// <summary>
        /// 全量更新数据
        /// </summary>
        public object FQChangeBondData
        {
            get { return fqChangeBondData; }
            set
            {
                if (value.ToString() == "{}")
                {
                    fqChangeBondData = value;
                    return;
                }

                //触发更新
                if (value!= fqChangeBondData)
                {
                    fqChangeBondData = value;
                    Common.Handler.Base.BaseHandler.Instance.PublishAppMsg(new PResult() { Code = "getFQ", Info1 = "WebFQ", Info2 = "FQChangeBond", Info3 = "" });
                }
            }
        }

        private Dictionary<string, string> _DictVenues = new Dictionary<string, string>();
        /// <summary>
        /// 中介ID，1 = TPSC 国利; 2 = CNEX 国际; 3 = CCTB 中诚; 4 = PTCN 平安; 5 = CTTJ 信唐; 6 = UEDA 上田
        /// </summary>
        public Dictionary<string, string> DictVenues
        {
            get { return _DictVenues; }
            set
            {
                _DictVenues = value;
            }
        }

        #region 最优

        private string _BestBidPrice;
        // <summary>
        /// 价格  -1 999 = --
        /// </summary>
        public string BestBidPrice
        {
            get
            {
                return _BestBidPrice;
            }
            set
            {
                _BestBidPrice = value;
            }
        }

        private string _BestBidBroker;
        /// <summary>
        /// Bid最优Broker
        /// </summary>
        public string BestBidBroker
        {
            get
            {
                return _BestBidBroker;
            }
            set
            {
                _BestBidBroker = value;
            }
        }

        private string _BestBidSettlType;
        /// <summary>
        /// Bid 清算速度
        /// </summary>
        public string BestBidSettlType
        {
            get
            {
                return _BestBidSettlType;
            }
            set
            {
                _BestBidSettlType = value;
            }
        }
        private string _BestOfrSettlType;
        /// <summary>
        /// Bid 清算速度
        /// </summary>
        public string BestOfrSettlType
        {
            get
            {
                return _BestOfrSettlType;
            }
            set
            {
                _BestOfrSettlType = value;
            }
        }
        
        private string _BestOfrPrice;

        /// <summary>
        /// 价格  -1 999 = --
        /// </summary>
        public string BestOfrPrice
        {
            get
            {
                return _BestOfrPrice;
            }
            set
            {
                _BestOfrPrice = value;
            }
        }

        private string _BestOfrBroker;

        /// <summary>
        /// Ofr最优Broker
        /// </summary>
        public string BestOfrBroker
        {
            get
            {
                return _BestOfrBroker;
            }
            set
            {
                _BestOfrBroker = value;
            }
        }

        #endregion


        public string BrokerSortOrder
        {
            get
            {
               return LocalEnv.UserInfo.BrokerSort ?? "2,4,3,5,6,1";
            }
        }
        #endregion


        public FinQMdHander()
        {

        }

        public void Initialize()
        {
            DictVenues = ComboSourceUtil.Instance.GetComboSource("venues_en_tonumber").ToDictionary(x => x.Key, x => x.Value);

            InitBestBidOfrQuotationData();

            BaseHandler.Instance.SubscribeLoadDataStatus(onloadData);

            //FinQ Md 更新深度行情
            Task.Factory.StartNew(() => DoProcERQueue(fqmdcancellationTokenSource.Token), TaskCreationOptions.LongRunning);
        }

        bool isLoadBond = false;
        private void onloadData(bool obj)
        {
            if (obj)
            {
               isLoadBond = true;
            }
        }

        /// <summary>
        /// 初始化显示数据
        /// </summary>
        private void InitBestBidOfrQuotationData()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                //最优
                ReSetData();
            }, DispatcherPriority.Normal);
        }

        private void ReSetData()
        {
            BidQuotations.Clear();
            OfrQuotations.Clear();
            BestBidQuotations.Clear();
            BestOfrQuotations.Clear();
            BestBidPrice = null;
            BestBidBroker = string.Empty;
            BestOfrPrice = null;
            BestOfrBroker = string.Empty;
            BestBidSettlType = string.Empty;
            BestOfrSettlType = string.Empty;
            CCDCValuaDate = string.Empty;
            CCDCValuation = string.Empty;
            CCDCValuation2 = string.Empty;
            CCDCValuationToolTip = string.Empty;
        }


        #region FinQ 订阅行情需要预处理
        private static CancellationTokenSource fqmdcancellationTokenSource = new CancellationTokenSource();
        private SemaphoreSlim fqmdsemaphore = new SemaphoreSlim(0, int.MaxValue); // 计数信号量
        private readonly ConcurrentDictionary<string, MarketDataSnapshotFullRefresh> DictCTPrice = new ConcurrentDictionary<string, MarketDataSnapshotFullRefresh>();
        private MarketDataSnapshotFullRefresh latestData;
        /// <summary>
        /// 深度行情回调 - 存储最新数据并触发信号
        /// </summary>
        /// <param name="obj"></param>
        private void Instance_OnMDPrice(MarketDataSnapshotFullRefresh obj)
        {
            if (obj != null && obj.SecurityID == this.SecurityID)
            {
                DictCTPrice.AddOrUpdate(obj.SecurityID, obj, (key, value) => obj);
                fqmdsemaphore.Release(); // 释放信号，通知有新数据
            }
        }

        //移除 lock (lockobj)，直接使用 ConcurrentDictionary 的线程安全特性。
        //TryRemove() 方式确保只处理最新的数据，避免重复处理。
        //这样可以 避免遗漏最新行情，同时 减少不必要的处理，提升高频行情处理的性能和响应速度
        private MarketDataSnapshotFullRefresh data;
        /// <summary>
        /// 更新深度市场行情
        /// </summary>
        private void DoProcERQueue(CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    // 使用非阻塞轮询，减少线程切换开销
                    fqmdsemaphore.Wait(cancellationToken); // 等待新数据到来
                    // 只取最新的数据，避免处理过时数据
                    if (DictCTPrice.TryRemove(this.SecurityID, out latestData))
                    {
                        if (latestData != null)
                        {
                            LogHelper.Info($"[处理] Worker {Task.CurrentId} 处理债券 {this.SecurityID} 最新数据: {latestData.SendingTime}");
                            UpdateMarketExecutions(latestData);
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // 任务取消时正常退出
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex.Message);
            }
        }


        /// <summary>
        /// 更新更新市场行情线程
        /// </summary>
        /// <param name="obj">原始报文</param>
        private void UpdateMarketExecutions(MarketDataSnapshotFullRefresh obj)
        {
            try
            {
                System.Windows.Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(() =>
                {
                    //Stopwatch stopwatch1 = new Stopwatch();
                    //stopwatch1.Start();
                    //List<GoodsVolumeVoModel> bidVolumeVoModels = new List<GoodsVolumeVoModel>(BidQuotations);
                    //List<GoodsVolumeVoModel> ofrVolumeVoModels = new List<GoodsVolumeVoModel>(OfrQuotations);
                    //BidQuotations.Clear();
                    //OfrQuotations.Clear();
                    try
                    {
                        if (obj != null)
                        {
                            if (obj.NoMDEntries == null)
                            {
                                return;
                            }

                            //MDEntryType 0 bid 1 ofr
                            ReSetData();

                            CCDCValuaDate = obj.CCDCValuaDate;
                            string tmpccdc = CommonUtils.formatPriceString(CommonUtils.ConvertToString(obj.CCDCValuation), 4);
                            string tmpccdc2 = CommonUtils.formatPriceString(CommonUtils.ConvertToString(obj.CCDCValuation2), 4);

                            CCDCValuation = string.IsNullOrEmpty(tmpccdc) ? "--" : tmpccdc;
                            CCDCValuation2 = string.IsNullOrEmpty(tmpccdc2) ? "--" : tmpccdc2.Replace("|", "/");
                            if (CCDCValuation == CCDCValuation2)
                            {
                                CCDCValuation2 = "--";
                            }
                            if (CCDCValuation != CCDCValuation2 && !string.IsNullOrEmpty(CCDCValuation) && !string.IsNullOrEmpty(CCDCValuation2) && CCDCValuation2 != "--")
                            {
                                CCDCValuation = "--";
                                CCDCValuation2 = $" {CCDCValuation2}";
                                CCDCValuationToolTip = "行权/到期";
                            }

                            SendingTime = obj.SendingTime;
                            //LogHelper.Info($"{obj.SendingTime} 收到 {obj.SecurityID} 行情. 收益率 {CCDCValuation} {CCDCValuation2} MarketDataSnapshotFullRefresh:{ CommonUtils.SerializeObject(obj.NoMDEntries)}");

                            // 添加和更新数据(普通)
                            foreach (GNoMDEntries entry in obj.NoMDEntries)
                            {

                                //只取中介的报价
                                if (!DictVenues.ContainsKey(entry.Venues))
                                {
                                    return;
                                }

                                string key = entry.MDEntryID + entry.MDEntryType;
                                string securityID = obj.SecurityID;
                                string venues = entry.Venues;
                                string venuesid = DictVenues[entry.Venues];

                                //string source = GenerateSourceByVenues(venues, entry.PartySubName);
                                bool isMe = LocalEnv.UserInfo.UserId == entry.PartySubID;
                                //string partySubName = GenerateSourceByVenues(venues, entry.PartySubName, entry.RoleName);

                                string settlType = entry.SettlType;
                                List<string> uniqeset = settlType.Split(',').ToList<string>();
                                Dictionary<string, string> dictuniqeset = new Dictionary<string, string>();
                                for (int i = 0; i < uniqeset.Count; i++)
                                {
                                    if (!dictuniqeset.ContainsKey(uniqeset[i]))
                                        dictuniqeset.Add(uniqeset[i], uniqeset[i]);
                                }


                                //string isIncomplete = IsIncomplete(entry, obj);
                                string isInner = entry.VenueTypeGW == "INNER" ? "1" : "0";
                                string marketIndicator = obj.MarketIndicator;

                                //新增处理显示字段
                                string yield = string.Empty;
                                string calTypeFieldTip = string.Empty; // 
                                string calTypeNetPriceTip = string.Empty; // 
                                string priceType = string.Empty;
                                string calFlag = string.Empty;

                                if (marketIndicator != "5")
                                {
                                    //设置PriceType
                                    if (!string.IsNullOrEmpty(priceType))
                                    {
                                        if (string.IsNullOrEmpty(CommonUtils.GetPriceType(obj.SecurityID)))
                                        {
                                            CommonUtils.SetPriceType(obj.SecurityID, priceType);
                                        }
                                    }

                                    //收益率
                                    yield = (entry.Yield ?? "--") == "--" ? "--" : CommonUtils.formatString(CommonUtils.ConvertToString(entry.Yield), 4);
                                    priceType = string.IsNullOrEmpty(entry.PriceType) ? "3" : entry.PriceType;
                                    //中介ID，1 = TPSC 国利; 2 = CNEX 国际; 3 = CCTB 中诚; 4 = PTCN 平安; 5 = CTTJ 信唐; 6 = UEDA 上田
                                    // priceType '0-意向;1-净价;2- 全价;3-收益率;4-利差;8-经纪商未指定',  当 priceType == 1   price=MDEntryPx 保存的是净价 yield保存是收益率 netPrice净价
                                    // exerciseFlag 0=行权收益率,1或空=到期收益率
                                    //calFlag 2-收益率反算净价  4-净价反算收益率  3-全价反算收益率和净价 5-利差反算收益率和净价 0 null
                                }
                                else if (marketIndicator == "5" && string.IsNullOrEmpty(yield))
                                {
                                    yield = CommonUtils.ConvertToString(entry.MDEntryPx);
                                }

                                GoodsVolumeVoModel findDr = null;

                                string side = entry.MDEntryType;

                                if (side == CommonConstants.SIDE_OFR)
                                {
                                    findDr = new GoodsVolumeVoModel();
                                    findDr.Key = key;
                                    findDr.IsBid = false;
                                    findDr.goodCode = securityID;
                                    findDr.Source = venues;
                                    findDr.SourceId = venuesid;
                                    findDr.VenueTypeGW = entry.VenueTypeGW;
                                    findDr.Side = side;
                                    findDr.FlashCount = 4;
                                    findDr.DeleteFlag = "0";
                                }
                                else if (side == CommonConstants.SIDE_BID)
                                {
                                    findDr = new GoodsVolumeVoModel();
                                    findDr.Key = key;
                                    findDr.IsBid = true;
                                    findDr.goodCode = securityID;
                                    findDr.Source = venues;
                                    findDr.SourceId = venuesid;
                                    findDr.VenueTypeGW = entry.VenueTypeGW;
                                    findDr.Side = side;
                                    findDr.FlashCount = 4;
                                    findDr.DeleteFlag = "0";
                                }

                                string netPrice = CommonUtils.ConvertToString(entry.NetPrice);
                                findDr.Yield = yield;
                                findDr.NetPrice = netPrice == "--" ? "--" : CommonUtils.formatPriceString(netPrice, 4);

                                findDr.YieldToExecution = entry.YieldToExecution ?? "";

                                //换券标识 true: 换券; false 不换券
                                findDr.ExchangeSignFlag = string.IsNullOrEmpty(entry.ExchangeSignFlag) ? false : entry.ExchangeSignFlag == "0" ? false : true;
                                //议价标识 0:无星; 1: 1星; 2: 2星
                                findDr.bargainFlag = entry.BargainFlag ?? "0";
                                //OCO 打包 1:oco 2:打包
                                findDr.ocoFlag = entry.OCOFlag ?? "";
                                findDr.brokerID = entry.PartySubID ?? "";
                                findDr.brokerName = entry.PartySubName ?? "";
                                findDr.traderID = entry.TraderID ?? "";
                                findDr.institutionID = entry.InstitutionID ?? "";
                                //findDr.institutionName = entry.InstitutionName;
                                //findDr.traderName = entry.TraderName;

                                //IDBPro用 notdo..
                                ////string temBroker = STC.Broker.Plugin.Utils.CommonUtils.GetBrokerTeamTagById(entry.PartySubID);
                                ////if (temBroker == CommonHelper.Instance.TeamTag)
                                ////{
                                ////    if (string.IsNullOrEmpty(entry.InstitutionName))
                                ////    {
                                ////        findDr.institutionName = CommonUtils.GetCrmInstInstiid(entry.InstitutionID)?.shortnamecn;
                                ////    }
                                ////    else
                                ////    {
                                ////        findDr.institutionName = entry.InstitutionName;
                                ////    }
                                ////    findDr.traderName = entry.TraderName;
                                ////}
                                ////else
                                ////{
                                ////    findDr.institutionName = STC.Broker.Plugin.Utils.Constant.DoubleDash;
                                ////    findDr.traderName = STC.Broker.Plugin.Utils.Constant.DoubleDash;
                                ////}

                                /// 内部报价标识
                                /// 2：内部报价，1：正常报价 0:待填写
                                //HandlInst":"2" 2明盘 1是暗盘
                                findDr.quoteInternalFlag = entry.HandlInst ?? "1";
                                findDr.HandlInst = entry.HandlInst ?? "1";

                                //市场标识 废弃
                                findDr.bondKeyListedMarket = "";

                                findDr.flagUrgent = entry.FirmFlag == "1" ? true : false;
                                findDr.FirmFlag = entry.FirmFlag;
                                //报价类型，2线上，1线下
                                findDr.OnLine = entry.OnLine ?? "2";

                                decimal qty = CommonUtils.ConvertToDecimal(entry.MDEntrySize);
                                findDr.qty = qty;
                                findDr.MDEntrySize = entry.MDEntrySize;

                                string price = string.Empty;

                                price = CommonUtils.formatPriceString(CommonUtils.ConvertToString(entry.MDEntryPx), 4);
                                findDr.price = price == "--" ? 0 : CommonUtils.ConvertToDecimal(price);
                                findDr.showprice = price;
                                findDr.MDEntryPx = entry.MDEntryPx;

                                findDr.time = entry.MDEntryTime;
                                findDr.settlementType = uniqeset.ToArray();
                                findDr.settlType = settlType;
                                findDr.DictSettleType = dictuniqeset;
                                findDr.Info9 = entry.Info9 ?? settlType;
                                //findDr.Info10 = entry.Info10;
                                findDr.id = entry.MDEntryID;

                                findDr.ExerciseFlag = entry.ExerciseFlag; // 0=行权收益率,1或空=到期收益率;
                                findDr.description = entry.Text ?? "";
                                findDr.FanDian = entry.FanDian ?? "";
                                findDr.FanDianFlag = entry.FanDianFlag ?? "0";
                                findDr.PriceType = priceType;
                                findDr.MDPriceLevel = CommonUtils.ConvertToInt(entry.MDPriceLevel);
                                findDr.TransactTime = string.Format("{0} {1}", entry.MDEntryDate, entry.MDEntryTime);
                                findDr.MDEntryDate = entry.MDEntryDate;
                                findDr.wholeFlag = entry.WholeFlag ?? "0";
                                /// 内部报价标识
                                /// 2：内部报价，1：正常报价 
                                //HandlInst":"2" 2明盘 1是暗盘
                                findDr.quoteInternalFlag = entry.HandlInst ?? "2";
                                findDr.HandlInst = entry.HandlInst ?? "2";

                                if (side == "1")
                                {
                                    OfrQuotations.Add(findDr);
                                }
                                else
                                {
                                    BidQuotations.Add(findDr);
                                }

                                //p*标识
                                findDr.Divided = entry.Divided ?? "";
                            }


                            //更新最优价和最优视图
                            UpdateMulitBestQuotations();

                            //刷新最优价
                            RefreshBestPrice();

                            //组合setAcitonData
                            //0 单家最优,SetActionData
                            if (this.ClickDataType == 0 )
                            {
                                GetAndSetSingleActionDataModel();
                            }

                            //1 点击综合最优,SetActionData
                            if (this.ClickDataType == 1)
                            {
                                GetAndSetMultiActionDataModel();
                            }

                            //组合 changeBond
                            if (this.ClickDataType == 0 || this.ClickDataType == 1)
                            {
                                GetAndSetFQUpdateDataModel();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Error(ex);
                    }

                    //stopwatch1.Stop();
                    //SetFormTop(priceWin);
                    //LogHelperAsyncExecute.Info($"===========报价刷新时间 {obj.SecurityID ?? ""} {obj.SendingTime ?? ""}：{stopwatch1.ElapsedMilliseconds}");
                    LogHelper.Info($"===========报价刷新时间 {obj.SecurityID ?? ""} {obj.SendingTime ?? ""}");

                }));
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
            }
        }

        /// <summary>
        /// 刷新最优价,次优 明细表
        /// </summary>
        public void RefreshBestPrice()
        {
            lock (lockbidObj)
            {
                if (this.BestBidQuotations.Count<GoodsVolumeVoModel>() > 0)
                {
                    GoodsVolumeVoModel best = BestBidQuotations.OrderBy(p => p.MDPriceLevel).FirstOrDefault();
                    if (best != null)
                    {
                        BestBidPrice = best.MDEntryPx;
                        BestBidBroker = best.Source;
                        BestBidSettlType = best.settlType;
                    }
                    else
                    {
                        BestBidPrice = null;
                        BestBidBroker = string.Empty;
                        BestBidSettlType = null;
                    }
                }
                else
                {
                    BestBidPrice = null;
                    BestBidBroker = string.Empty;
                    BestBidSettlType = null;
                }
            }

            lock (lockofrObj)
            {
                if (this.BestOfrQuotations.Count<GoodsVolumeVoModel>() > 0)
                {
                    GoodsVolumeVoModel best = BestOfrQuotations.OrderBy(p => p.MDPriceLevel).FirstOrDefault();
                    if (best != null)
                    {
                        BestOfrPrice = best.MDEntryPx;
                        BestOfrBroker = best.Source;
                        BestOfrSettlType = best.settlType;
                    }
                    else
                    {
                        BestOfrPrice = null;
                        BestOfrBroker = string.Empty;
                        BestOfrSettlType = null;
                    }
                }
                else
                {
                    BestOfrPrice = null;
                    BestOfrBroker = string.Empty;
                    BestOfrSettlType = null;
                }
            }

        }

        /// <summary>
        /// 更新多家最优价列表
        /// </summary>
        public void UpdateMulitBestQuotations()
        {
            lock (lockbidObj)
            {
                if (this.BidQuotations.Count<GoodsVolumeVoModel>() > 0)
                {
                    //priceType '0-意向;1-净价;2- 全价;3-收益率;4-利差;8-经纪商未指定',  当 priceType == 1   price=MDEntryPx 保存的是净价 yield保存是收益率 netPrice净价
                    #region  全 最优计算
                    List<GoodsVolumeVoModel> fullmultirec = BidQuotations.GroupBy(b => b.SourceId) // 1. 按 SourceId 分组
                        .SelectMany(g =>
                        {
                            // 2. 找到该 SourceId 最小的 MDPriceLevel
                            int minLevel = g.Min(x => x.MDPriceLevel);
                            // 3. 找到最优价格
                            decimal bestPrice = g.First(x => x.MDPriceLevel == minLevel).price;
                            // 4. 返回该组中价格等于最优价格的所有记录
                            return g.Where(x => x.price == bestPrice);
                        })
                        // 5. 按 SourceId 升序、再按 MDPriceLevel 升序 排序
                        .OrderBy(x => x.SourceId).ThenBy(x => x.MDPriceLevel).ToList();

                    foreach (GoodsVolumeVoModel gvvm in fullmultirec)
                    {
                        BestBidQuotations.Add(gvvm);
                    }

                    //LogHelper.Info($"{CommonUtils.SerializeObject(BestBidQuotations)}");
                    #endregion
                }
            }

            lock (lockofrObj)
            {
                if (this.OfrQuotations.Count<GoodsVolumeVoModel>() > 0)
                {
                    //priceType '0-意向;1-净价;2- 全价;3-收益率;4-利差;8-经纪商未指定',  当 priceType == 1   price=MDEntryPx 保存的是净价 yield保存是收益率 netPrice净价
                    #region  全 最优计算
                    List<GoodsVolumeVoModel> fullmultirec = OfrQuotations.GroupBy(b => b.SourceId) // 1. 按 SourceId 分组
                        .SelectMany(g =>
                        {
                            // 2. 找到该 SourceId 最小的 MDPriceLevel
                            int minLevel = g.Min(x => x.MDPriceLevel);
                            // 3. 找到最优价格
                            decimal bestPrice = g.First(x => x.MDPriceLevel == minLevel).price;
                            // 4. 返回该组中价格等于最优价格的所有记录
                            return g.Where(x => x.price == bestPrice);
                        })
                        // 5. 按 SourceId 升序、再按 MDPriceLevel 升序 排序
                        .OrderBy(x => x.SourceId).ThenBy(x => x.MDPriceLevel).ToList();

                    foreach (GoodsVolumeVoModel gvvm in fullmultirec)
                    {
                        BestOfrQuotations.Add(gvvm);
                    }

                    //LogHelper.Info($"{CommonUtils.SerializeObject(BestOfrQuotations)}");

                    #endregion
                }
            }
        }

        /// <summary>
        /// 全量数据 ChangeBond
        /// </summary>
        public void GetAndSetFQUpdateDataModel()
        {
            //组合全量数据
            STC.Common.Model.FinQ.FQUpdateDataModel fddm = new STC.Common.Model.FinQ.FQUpdateDataModel();
            fddm._uuid = Guid.NewGuid().ToString();
            fddm.bondkey = BondsInfo.Bond_Key;
            fddm.listedmarket = BondsInfo.listed_Market;
            fddm.fromQB = LocalEnv.UserInfo.UserName;
            fddm.fromQBAccountID = LocalEnv.UserInfo.UserId;
            fddm.openChat = true;
            fddm.bondinfo = this.FQBondInfo;
            fddm.dataType = this.ClickDataType;
            List<STC.Common.Model.FinQ.Data> lstfqdata = new List<STC.Common.Model.FinQ.Data>();

            string content = null;

            if (this.Side == "bid")
            {
                List<GoodsVolumeVoModel> FirstBidQuotations = GetFirstBySourceId(BestBidQuotations);
                foreach (GoodsVolumeVoModel item in FirstBidQuotations)
                {
                    STC.Common.Model.FinQ.Data fqdataitem = new STC.Common.Model.FinQ.Data();

                    fqdataitem.comp_id = item.SourceId;
                    fqdataitem.bestbroker = item.Source == BestBidBroker ? "Y" : "";
                    fqdataitem.code = item.goodCode;
                    fqdataitem.bond_type = BondsInfo.SECURITY_TYPE;
                    fqdataitem.name = BondsInfo.Short_Name;
                    fqdataitem.residual_maturity = BondsInfo.RemaingingTime;
                    fqdataitem.bond_rating = ""; //债项评级   //todo...
                    fqdataitem.issuer_rating = BondsInfo.Rating_Current;
                    fqdataitem.side = item.Side == "0" ? "bid" : "ofr";
                    fqdataitem.ext = item.id;
                    string price = item.MDEntryPx;
                    if (item.PriceType == "0")
                        price = item.Side == "0" ? "bid" : "ofr";
                    else
                        price = item.MDEntryPx;

                    fqdataitem.price = price;
                    fqdataitem.price_type = item.PriceType == "0" ? "" : item.PriceType; //价格类型：priceType '0-意向;1-净价;2- 全价;3-收益率;4-利差;8-经纪商未指定',  当 priceType == 1   price=MDEntryPx 保存的是净价 yield保存是收益率 netPrice净价
                    fqdataitem.yield_type = item.ExerciseFlag == "0" ? "EXERCISE" : "MATURITY"; // ExerciseFlag ?? "1"; // 0=行权收益率,1或空=到期收益率 当price_type=3时必传， * EXERCISE=行权，MATURITY=到期 <= 收益率类型：1=到期收益率；2=持有期收益率；3=回购收益率；4=票面收益率；5=浮动收益率；6=协议收益率；7=协定收益率
                    List<GoodsVolumeVoModel> currSourceId = GetBySourceIdSorted(BestBidQuotations, item.SourceId);
                    fqdataitem.vol_delivery = GetQBVolDeliveryFromGoodsVolumeVoModel(currSourceId, false);
                    fqdataitem.vol_delivery_qb = GetQBVolDeliveryFromGoodsVolumeVoModel(currSourceId);

                    List<GoodsVolumeVoModel> currOfrSourceId = GetBySourceIdSorted(BestOfrQuotations, item.SourceId);
                    //获取opp 
                    GoodsVolumeVoModel ofritem = new GoodsVolumeVoModel();
                    if (currOfrSourceId.Count > 0)
                    {
                        ofritem = currOfrSourceId[0];
                        if (ofritem.PriceType == "0")
                            price = ofritem.Side == "0" ? "bid" : "ofr";
                        else
                            price = ofritem.MDEntryPx;

                        fqdataitem.opp_price = price;
                        fqdataitem.opp_price_type = ofritem.PriceType == "0" ? "" : ofritem.PriceType;
                        fqdataitem.opp_yield_type = ofritem.ExerciseFlag == "0" ? "EXERCISE" : "MATURITY";
                        fqdataitem.opp_vol_delivery = GetQBVolDeliveryFromGoodsVolumeVoModel(currOfrSourceId);
                    }

                    fqdataitem.val_yield = CCDCValuation;
                    lstfqdata.Add(fqdataitem);
                }
            }

            if (this.Side == "ofr")
            {
                List<GoodsVolumeVoModel> FirstOfrQuotations = GetFirstBySourceId(BestOfrQuotations);
                foreach (GoodsVolumeVoModel item in FirstOfrQuotations)
                {
                    STC.Common.Model.FinQ.Data fqdataitem = new STC.Common.Model.FinQ.Data();

                    fqdataitem.comp_id = item.SourceId;
                    fqdataitem.bestbroker = item.Source == BestOfrBroker ? "Y" : "";
                    fqdataitem.code = item.goodCode;
                    fqdataitem.bond_type = BondsInfo.SECURITY_TYPE;
                    fqdataitem.name = BondsInfo.Short_Name;
                    fqdataitem.residual_maturity = BondsInfo.RemaingingTime;
                    fqdataitem.bond_rating = ""; //债项评级   //todo...
                    fqdataitem.issuer_rating = BondsInfo.Rating_Current;
                    fqdataitem.side = item.Side == "0" ? "bid" : "ofr";
                    fqdataitem.ext = item.id;
                    string price = item.MDEntryPx;
                    if (item.PriceType == "0")
                        price = item.Side == "0" ? "bid" : "ofr";
                    else
                        price = item.MDEntryPx;

                    fqdataitem.price = price;
                    fqdataitem.price_type = item.PriceType == "0" ? "" : item.PriceType; //价格类型：priceType '0-意向;1-净价;2- 全价;3-收益率;4-利差;8-经纪商未指定',  当 priceType == 1   price=MDEntryPx 保存的是净价 yield保存是收益率 netPrice净价
                    fqdataitem.yield_type = item.ExerciseFlag == "0" ? "EXERCISE" : "MATURITY"; // ExerciseFlag ?? "1"; // 0=行权收益率,1或空=到期收益率 当price_type=3时必传， * EXERCISE=行权，MATURITY=到期 <= 收益率类型：1=到期收益率；2=持有期收益率；3=回购收益率；4=票面收益率；5=浮动收益率；6=协议收益率；7=协定收益率
                    List<GoodsVolumeVoModel> currSourceId = GetBySourceIdSorted(BestOfrQuotations, item.SourceId);
                    fqdataitem.vol_delivery = GetQBVolDeliveryFromGoodsVolumeVoModel(currSourceId, false);
                    fqdataitem.vol_delivery_qb = GetQBVolDeliveryFromGoodsVolumeVoModel(currSourceId);

                    List<GoodsVolumeVoModel> currBidSourceId = GetBySourceIdSorted(BestBidQuotations, item.SourceId);
                    //获取opp 
                    GoodsVolumeVoModel biditem = new GoodsVolumeVoModel();
                    if (currBidSourceId.Count > 0)
                    {
                        biditem = currBidSourceId[0];
                        if (biditem.PriceType == "0")
                            price = biditem.Side == "0" ? "bid" : "ofr";
                        else
                            price = biditem.MDEntryPx;

                        fqdataitem.opp_price = price;
                        fqdataitem.opp_price_type = biditem.PriceType == "0" ? "" : biditem.PriceType;
                        fqdataitem.opp_yield_type = biditem.ExerciseFlag == "0" ? "EXERCISE" : "MATURITY";
                        fqdataitem.opp_vol_delivery = GetQBVolDeliveryFromGoodsVolumeVoModel(currBidSourceId);
                    }

                    fqdataitem.val_yield = CCDCValuation;

                    lstfqdata.Add(fqdataitem);
                }
            }

            //单家最优
            if (this.ClickDataType == 0)
            {
                List<GoodsVolumeVoModel> currChangeBidSourceId = GetBySourceIdSorted(BestBidQuotations, BrokeId);
                List<GoodsVolumeVoModel> currChangeOfrSourceId = GetBySourceIdSorted(BestOfrQuotations, BrokeId);
                string brokerCN = ComboSourceUtil.Instance.FindEnum(BrokeId, "venues_cn_tocn");
                string bidprice = string.Empty;
                string ofrprice = string.Empty;
                if (currChangeBidSourceId.Count > 0)
                {
                    GoodsVolumeVoModel biditem = currChangeBidSourceId[0];
                    bidprice = biditem.MDEntryPx;
                    if (biditem.PriceType == "0")
                        bidprice = biditem.Side == "0" ? "bid" : "ofr";
                    else
                        bidprice = biditem.MDEntryPx;
                }
                if (currChangeOfrSourceId.Count > 0)
                {
                    GoodsVolumeVoModel ofritem = currChangeOfrSourceId[0];
                    ofrprice = ofritem.MDEntryPx;
                    if (ofritem.PriceType == "0")
                        ofrprice = ofritem.Side == "0" ? "bid" : "ofr";
                    else
                        ofrprice = ofritem.MDEntryPx;
                }

                content = $"{BondsInfo.RemaingingTime} {BondsInfo.Bond_ID} {BondsInfo.Short_Name} {FormatAmount(bidprice)}/{FormatAmount(ofrprice)} {MergeDescriptions(currChangeBidSourceId)}/{MergeDescriptions(currChangeOfrSourceId)} {brokerCN} {GetCcdc()} {LastSendingTime}";
            }

            //综合最优
            if (this.ClickDataType == 1)
            {
                List<GoodsVolumeVoModel> currChangeBidSourceId = GetBestQuotationsBySettle(BidQuotations.ToList(), SelSettle);
                List<GoodsVolumeVoModel> currChangeOfrSourceId = GetBestQuotationsBySettle(OfrQuotations.ToList(), SelSettle);

                //全局最小价格档位
                int globalBidMinLevel = currChangeBidSourceId.Min(x => x.MDPriceLevel);
                decimal globalBidBestPrice = currChangeBidSourceId.First(x => x.MDPriceLevel == globalBidMinLevel).price;

                int globalOfrMinLevel = currChangeOfrSourceId.Min(x => x.MDPriceLevel);
                decimal globalOfrBestPrice = currChangeOfrSourceId.First(x => x.MDPriceLevel == globalOfrMinLevel).price;

                //List<GoodsVolumeVoModel> currChangeBidSourceId = BestBidQuotations.Where(p => p.MDEntryPx == BestBidPrice).ToList();
                //List<GoodsVolumeVoModel> currChangeOfrSourceId = BestOfrQuotations.Where(p => p.MDEntryPx == BestOfrPrice).ToList();
                content = $"{BondsInfo.RemaingingTime} {BondsInfo.Bond_ID} {BondsInfo.Short_Name} {FormatAmount(globalBidBestPrice.ToString())}/{FormatAmount(globalOfrBestPrice.ToString())} {MergeDescriptions(currChangeBidSourceId)}/{MergeDescriptions(currChangeOfrSourceId)} {GetCcdc()} {LastSendingTime}";
            }

            fddm.content = content;
            fddm.data = lstfqdata;

            FQChangeBondData = CommonUtils.SerializeObject(fddm);
        }

        /// <summary>
        /// 0 单家最优
        /// </summary>
        public void GetAndSetSingleActionDataModel(bool isSend = false)
        {
            // 发送到报价助手
            FQDataModel fddm = new FQDataModel();
            fddm.bondinfo = fqbondinfo;
            fddm._uuid = Guid.NewGuid().ToString();
            fddm.bondkey = BondsInfo.Bond_Key;
            fddm.listedmarket = BondsInfo.listed_Market;
            fddm.fromQB = LocalEnv.UserInfo.UserName;
            fddm.fromQBAccountID = LocalEnv.UserInfo.UserId;
            fddm.openChat = true;
            fddm.dataType = 0;  //0：点击单家最优1：点击综合最优；2：点击成交价 

            List<GoodsVolumeVoModel> currBidSourceId = GetBySourceIdSorted(BestBidQuotations, BrokeId);
            List<GoodsVolumeVoModel> currOfrSourceId = GetBySourceIdSorted(BestOfrQuotations, BrokeId);

            STC.Common.Model.FinQ.Data data = new STC.Common.Model.FinQ.Data();
            data.comp_id = BrokeId;
            data.code = BondsInfo.Bond_ID;
            data.bond_type = BondsInfo.SECURITY_TYPE;
            data.name = BondsInfo.Short_Name;
            data.residual_maturity = BondsInfo.RemaingingTime;
            data.bond_rating = ""; //债项评级   //todo...
            data.issuer_rating = BondsInfo.Rating_Current;
            string content = null;
            string BrokeCN = ComboSourceUtil.Instance.FindEnum(this.BrokeId, "venues_cn_tocn");

            if (this.Side == "bid")
            {
                if (currBidSourceId.Count > 0)
                {
                    GoodsVolumeVoModel item = currBidSourceId[0];
                    data.bestbroker = item.Source == BestBidBroker ? "Y" : "";
                    data.side = item.Side == "0" ? "bid" : "ofr";

                    string price = item.MDEntryPx;
                    if (item.PriceType == "0")
                        price = item.Side == "0" ? "bid" : "ofr";
                    else
                        price = item.MDEntryPx;

                    data.price = price;
                    data.price_type = item.PriceType == "0" ? "" : item.PriceType; //价格类型：priceType '0-意向;1-净价;2- 全价;3-收益率;4-利差;8-经纪商未指定',  当 priceType == 1   price=MDEntryPx 保存的是净价 yield保存是收益率 netPrice净价
                    data.vol_delivery = GetQBVolDeliveryFromGoodsVolumeVoModel(currBidSourceId, false);
                    data.vol_delivery_qb = GetQBVolDeliveryFromGoodsVolumeVoModel(currBidSourceId);
                    data.yield_type = item.ExerciseFlag == "0" ? "EXERCISE" : "MATURITY"; // ExerciseFlag ?? "1"; // 0=行权收益率,1或空=到期收益率 当price_type=3时必传， * EXERCISE=行权，MATURITY=到期 <= 收益率类型：1=到期收益率；2=持有期收益率；3=回购收益率；4=票面收益率；5=浮动收益率；6=协议收益率；7=协定收益率
                    //获取opp 
                    GoodsVolumeVoModel ofritem = new GoodsVolumeVoModel();
                    if (currOfrSourceId.Count > 0)
                    {
                        ofritem = currOfrSourceId[0];
                        if (ofritem.PriceType == "0")
                            price = ofritem.Side == "0" ? "bid" : "ofr";
                        else
                            price = ofritem.MDEntryPx;

                        data.opp_price = price;
                        data.opp_price_type = ofritem.PriceType == "0" ? "" : ofritem.PriceType;
                        data.opp_yield_type = ofritem.ExerciseFlag == "0" ? "EXERCISE" : "MATURITY";
                        data.opp_vol_delivery = GetQBVolDeliveryFromGoodsVolumeVoModel(currOfrSourceId);
                    }

                    data.val_yield = CCDCValuation;
                    content = $"{BondsInfo.RemaingingTime} {BondsInfo.Bond_ID} {BondsInfo.Short_Name} {FormatAmount(data.price)}/{FormatAmount(data.opp_price)} {MergeDescriptions(currBidSourceId)}/{MergeDescriptions(currOfrSourceId)} {BrokeCN} {GetCcdc()} {LastSendingTime}";
                    data.ext = item.id;
                }
                else
                {
                    data.side = "bid";
                    //获取opp 
                    GoodsVolumeVoModel ofritem = new GoodsVolumeVoModel();
                    string price = null;
                    if (currOfrSourceId.Count > 0)
                    {
                        ofritem = currOfrSourceId[0];
                        if (ofritem.PriceType == "0")
                            price = ofritem.Side == "0" ? "bid" : "ofr";
                        else
                            price = ofritem.MDEntryPx;

                        data.opp_price = price;
                        data.opp_price_type = ofritem.PriceType == "0" ? "" : ofritem.PriceType;
                        data.opp_yield_type = ofritem.ExerciseFlag == "0" ? "EXERCISE" : "MATURITY";
                        data.opp_vol_delivery = GetQBVolDeliveryFromGoodsVolumeVoModel(currOfrSourceId);
                    }

                    content = $"{BondsInfo.RemaingingTime} {BondsInfo.Bond_ID} {BondsInfo.Short_Name} {FormatAmount(data?.price)}/{FormatAmount(data?.opp_price)} {MergeDescriptions(currBidSourceId)}/{MergeDescriptions(currOfrSourceId)} {BrokeCN} {GetCcdc()} {LastSendingTime}";
                    data.val_yield = CCDCValuation;
                }
            }

            if (this.Side == "ofr")
            {
                if (currOfrSourceId.Count > 0)
                {
                    GoodsVolumeVoModel item = currOfrSourceId[0];
                    data.bestbroker = item.Source == BestOfrBroker ? "Y" : "";
                    data.side = item.Side == "0" ? "bid" : "ofr";
                    data.ext = item.id;

                    string price = item.MDEntryPx;
                    if (item.PriceType == "0")
                        price = item.Side == "0" ? "bid" : "ofr";
                    else
                        price = item.MDEntryPx;

                    data.price = price;
                    data.price_type = item.PriceType == "0" ? "" : item.PriceType; //价格类型：priceType '0-意向;1-净价;2- 全价;3-收益率;4-利差;8-经纪商未指定',  当 priceType == 1   price=MDEntryPx 保存的是净价 yield保存是收益率 netPrice净价
                    data.vol_delivery = GetQBVolDeliveryFromGoodsVolumeVoModel(currOfrSourceId, false);
                    data.vol_delivery_qb = GetQBVolDeliveryFromGoodsVolumeVoModel(currOfrSourceId);
                    data.yield_type = item.ExerciseFlag == "0" ? "EXERCISE" : "MATURITY"; // ExerciseFlag ?? "1"; // 0=行权收益率,1或空=到期收益率 当price_type=3时必传， * EXERCISE=行权，MATURITY=到期 <= 收益率类型：1=到期收益率；2=持有期收益率；3=回购收益率；4=票面收益率；5=浮动收益率；6=协议收益率；7=协定收益率
                    //获取opp 
                    GoodsVolumeVoModel biditem = new GoodsVolumeVoModel();
                    if (currBidSourceId.Count > 0)
                    {
                        biditem = currBidSourceId[0];
                        if (biditem.PriceType == "0")
                            price = biditem.Side == "0" ? "bid" : "ofr";
                        else
                            price = biditem.MDEntryPx;
                        data.opp_price = price;
                        data.opp_price_type = biditem.PriceType == "0" ? "" : biditem.PriceType;
                        data.opp_yield_type = biditem.ExerciseFlag == "0" ? "EXERCISE" : "MATURITY";
                        data.opp_vol_delivery = GetQBVolDeliveryFromGoodsVolumeVoModel(currBidSourceId);
                    }

                    data.val_yield = CCDCValuation;
                    content = $"{BondsInfo.RemaingingTime} {BondsInfo.Bond_ID} {BondsInfo.Short_Name} {FormatAmount(data.opp_price)}/{FormatAmount(data.price)} {MergeDescriptions(currBidSourceId)}/{MergeDescriptions(currOfrSourceId)} {BrokeCN} {GetCcdc()} {LastSendingTime}";
                    data.ext = item.id;
                }
                else
                {
                    data.side = "ofr";
                    //获取opp 
                    GoodsVolumeVoModel ofritem = new GoodsVolumeVoModel();
                    string price = null;
                    //获取opp 
                    GoodsVolumeVoModel biditem = new GoodsVolumeVoModel();
                    if (currBidSourceId.Count > 0)
                    {
                        biditem = currBidSourceId[0];
                        if (biditem.PriceType == "0")
                            price = biditem.Side == "0" ? "bid" : "ofr";
                        else
                            price = biditem.MDEntryPx;
                        data.opp_price = price;
                        data.opp_price_type = biditem.PriceType == "0" ? "" : biditem.PriceType;
                        data.opp_yield_type = biditem.ExerciseFlag == "0" ? "EXERCISE" : "MATURITY";
                        data.opp_vol_delivery = GetQBVolDeliveryFromGoodsVolumeVoModel(currBidSourceId);
                    }

                    content = $"{BondsInfo.RemaingingTime} {BondsInfo.Bond_ID} {BondsInfo.Short_Name} {FormatAmount(data?.opp_price)}/{FormatAmount(data?.price)} {MergeDescriptions(currBidSourceId)}/{MergeDescriptions(currOfrSourceId)} {BrokeCN} {GetCcdc()} {LastSendingTime}";

                    data.val_yield = CCDCValuation;
                }
            }

            fddm.content = content;
            fddm.data = data;
            this.FQSetActionData = CommonUtils.SerializeObject(fddm);
            if (isSend)
            {
                Common.Handler.Base.BaseHandler.Instance.PublishAppMsg(new PResult() { Code = "getFQ", Info1 = "WebFQ", Info2 = "FQSetAction", Info3 = "" });
                GetAndSetFQUpdateDataModel();
            }
        }

        /// <summary>
        /// 1 点击综合最优,需要考虑 SelSettle 类型  全,+1,+0,远
        /// </summary>
        /// <param name="isSend"></param>
        public void GetAndSetMultiActionDataModel(bool isSend = false)
        {
            // 发送到报价助手
            STC.Common.Model.FinQ.FQUpdateDataModel fddm = new STC.Common.Model.FinQ.FQUpdateDataModel();
            fddm.bondinfo = fqbondinfo;
            fddm._uuid = Guid.NewGuid().ToString();
            fddm.bondkey = BondsInfo.Bond_Key;
            fddm.listedmarket = BondsInfo.listed_Market;
            fddm.fromQB = LocalEnv.UserInfo.UserName;
            fddm.fromQBAccountID = LocalEnv.UserInfo.UserId;
            fddm.openChat = true;
            fddm.dataType = 1;  //0：点击单家最优 1：点击综合最优；2：点击成交价 
            List<STC.Common.Model.FinQ.Data> lstfqdata = new List<STC.Common.Model.FinQ.Data>();

            //获取当前最优价的所有记录
            //List<GoodsVolumeVoModel> currBidSourceId = BestBidQuotations.Where(p => p.MDEntryPx == BestBidPrice).ToList();
            //List<GoodsVolumeVoModel> currOfrSourceId = BestOfrQuotations.Where(p => p.MDEntryPx == BestOfrPrice).ToList();
            List<GoodsVolumeVoModel> currBidSourceId = GetBestQuotationsBySettle(BidQuotations.ToList(), SelSettle);
            List<GoodsVolumeVoModel> currOfrSourceId = GetBestQuotationsBySettle(OfrQuotations.ToList(), SelSettle);

            string content = null;
            // 指定排序规则
            var orderList = BrokerSortOrder.Split(',').Select(s => Convert.ToInt32(s.Trim())).ToList();
            List<GoodsVolumeVoModel> firstBidSourceId = currBidSourceId.GroupBy(x=> x.SourceId).Select(g => g.OrderBy(x => x.MDPriceLevel).First()).OrderBy(x => orderList.IndexOf(Convert.ToInt32(x.SourceId))).ToList();
            List<GoodsVolumeVoModel> firstOfrSourceId = currOfrSourceId.GroupBy(x => x.SourceId).Select(g => g.OrderBy(x => x.MDPriceLevel).First()).OrderBy(x => orderList.IndexOf(Convert.ToInt32(x.SourceId))).ToList();

            if (this.Side == "bid")
            {
                if (firstBidSourceId.Count > 0)
                {
                    foreach (GoodsVolumeVoModel item in firstBidSourceId)
                    {
                        STC.Common.Model.FinQ.Data data = new STC.Common.Model.FinQ.Data();
                        data.comp_id = item.SourceId;
                        data.code = BondsInfo.Bond_ID;
                        data.bond_type = BondsInfo.SECURITY_TYPE;
                        data.name = BondsInfo.Short_Name;
                        data.residual_maturity = BondsInfo.RemaingingTime;
                        data.bond_rating = ""; //债项评级   //todo...
                        data.issuer_rating = BondsInfo.Rating_Current;
                        data.bestbroker = item.Source == BestBidBroker ? "Y" : "";
                        data.side = item.Side == "0" ? "bid" : "ofr";
                        data.ext = "";
                        string price = item.MDEntryPx;
                        if (item.PriceType == "0")
                            price = item.Side == "0" ? "bid" : "ofr";
                        else
                            price = item.MDEntryPx;
                        data.price = price;
                        data.price_type = item.PriceType == "0" ? "" : item.PriceType; //价格类型：priceType '0-意向;1-净价;2- 全价;3-收益率;4-利差;8-经纪商未指定'
                        data.vol_delivery = GetQBVolDeliveryFromGoodsVolumeVoModel(GetBySourceIdSorted(currBidSourceId, item.SourceId), false);
                        data.vol_delivery_qb = GetQBVolDeliveryFromGoodsVolumeVoModel(GetBySourceIdSorted(currBidSourceId, item.SourceId));
                        data.yield_type = item.ExerciseFlag == "0" ? "EXERCISE" : "MATURITY";
                        //获取opp 
                        GoodsVolumeVoModel ofritem = new GoodsVolumeVoModel();
                        if (currOfrSourceId.Count > 0)
                        {
                            ofritem = currOfrSourceId[0];
                            if (ofritem.PriceType == "0")
                                price = ofritem.Side == "0" ? "bid" : "ofr";
                            else
                                price = ofritem.MDEntryPx;

                            data.opp_price = price;
                            data.opp_price_type = ofritem.PriceType == "0" ? "" : ofritem.PriceType;
                            data.opp_yield_type = ofritem.ExerciseFlag == "0" ? "EXERCISE" : "MATURITY";
                            data.opp_vol_delivery = GetQBVolDeliveryFromGoodsVolumeVoModel(GetBySourceIdSorted(new ObservableCollection<GoodsVolumeVoModel>(currOfrSourceId.ToList()), item.SourceId));
                        }

                        data.val_yield = CCDCValuation;
                        lstfqdata.Add(data);
                    }
                }
                else
                {
                    STC.Common.Model.FinQ.Data data = new STC.Common.Model.FinQ.Data();
                    //data.comp_id = bestbidbrokerid;
                    data.code = BondsInfo.Bond_ID;
                    data.bond_type = BondsInfo.SECURITY_TYPE;
                    data.name = BondsInfo.Short_Name;
                    data.residual_maturity = BondsInfo.RemaingingTime;
                    data.bond_rating = ""; //债项评级   //todo...
                    data.issuer_rating = BondsInfo.Rating_Current;
                    data.side = "bid";
                    data.ext = "";
                    data.val_yield = CCDCValuation;
                    lstfqdata.Add(data);
                }
            }

            if (this.Side == "ofr")
            {
                if (firstOfrSourceId.Count > 0)
                {
                    foreach (GoodsVolumeVoModel item in firstOfrSourceId)
                    {
                        STC.Common.Model.FinQ.Data data = new STC.Common.Model.FinQ.Data();
                        data.comp_id = item.SourceId;
                        data.code = BondsInfo.Bond_ID;
                        data.bond_type = BondsInfo.SECURITY_TYPE;
                        data.name = BondsInfo.Short_Name;
                        data.residual_maturity = BondsInfo.RemaingingTime;
                        data.bond_rating = ""; //债项评级   //todo...
                        data.issuer_rating = BondsInfo.Rating_Current;
                        data.bestbroker = item.Source == BestBidBroker ? "Y" : "";
                        data.side = item.Side == "0" ? "bid" : "ofr";
                        data.ext = "";
                        string price = item.MDEntryPx;
                        if (item.PriceType == "0")
                            price = item.Side == "0" ? "bid" : "ofr";
                        else
                            price = item.MDEntryPx;
                        data.price = price;
                        data.price_type = item.PriceType == "0" ? "" : item.PriceType; //价格类型：priceType '0-意向;1-净价;2- 全价;3-收益率;4-利差;8-经纪商未指定'
                        data.vol_delivery = GetQBVolDeliveryFromGoodsVolumeVoModel(GetBySourceIdSorted(currOfrSourceId, item.SourceId), false);
                        data.vol_delivery_qb = GetQBVolDeliveryFromGoodsVolumeVoModel(GetBySourceIdSorted(currOfrSourceId, item.SourceId));
                        data.yield_type = item.ExerciseFlag == "0" ? "EXERCISE" : "MATURITY";
                        //获取opp 
                        GoodsVolumeVoModel biditem = new GoodsVolumeVoModel();
                        if (currBidSourceId.Count > 0)
                        {
                            biditem = currBidSourceId[0];
                            if (biditem.PriceType == "0")
                                price = biditem.Side == "0" ? "bid" : "ofr";
                            else
                                price = biditem.MDEntryPx;

                            data.opp_price = price;
                            data.opp_price_type = biditem.PriceType == "0" ? "" : biditem.PriceType;
                            data.opp_yield_type = biditem.ExerciseFlag == "0" ? "EXERCISE" : "MATURITY";
                            data.opp_vol_delivery = GetQBVolDeliveryFromGoodsVolumeVoModel(GetBySourceIdSorted(currBidSourceId, item.SourceId));
                        }

                        data.val_yield = CCDCValuation;
                        lstfqdata.Add(data);
                    }
                }
                else
                {
                    STC.Common.Model.FinQ.Data data = new STC.Common.Model.FinQ.Data();
                    //data.comp_id = bestofrbrokerid;
                    data.code = BondsInfo.Bond_ID;
                    data.bond_type = BondsInfo.SECURITY_TYPE;
                    data.name = BondsInfo.Short_Name;
                    data.residual_maturity = BondsInfo.RemaingingTime;
                    data.bond_rating = ""; //债项评级   //todo...
                    data.issuer_rating = BondsInfo.Rating_Current;
                    data.side = "ofr";
                    data.ext = "";
                    data.val_yield = CCDCValuation;
                    lstfqdata.Add(data);
                }
            }

            //string BrokeBidCN = ComboSourceUtil.Instance.FindEnum(bestbidbrokerid, "venues_cn_tocn");
            //string BrokeOfrCN = ComboSourceUtil.Instance.FindEnum(bestofrbrokerid, "venues_cn_tocn");
            
            content = $"{BondsInfo.RemaingingTime} {BondsInfo.Bond_ID} {BondsInfo.Short_Name} {FormatAmount(BestBidPrice)}/{FormatAmount(BestOfrPrice)} {MergeDescriptions(currBidSourceId)}/{MergeDescriptions(currOfrSourceId)} {GetCcdc()} {LastSendingTime}";
            fddm.content = content;
            fddm.data = lstfqdata;

            this.FQSetActionData = CommonUtils.SerializeObject(fddm);

            if (isSend)
            {
                Common.Handler.Base.BaseHandler.Instance.PublishAppMsg(new PResult() { Code = "getFQ", Info1 = "WebFQ", Info2 = "FQSetAction", Info3 = "" });
                GetAndSetFQUpdateDataModel();
            }
        }

        #region 根据清算速度组合获取最优价
        //// 场景1：只要全部数据
        //GetBestQuotationsBySettle(BidQuotations, "全");
        //// 场景2：只要T+1清算的数据
        //GetBestQuotationsBySettle(BidQuotations, "+1");
        //// 场景3：要T+1或T+0清算的数据
        //GetBestQuotationsBySettle(BidQuotations, "+1,+0");
        //// 场景4：要T+1或远期清算的数据
        //GetBestQuotationsBySettle(BidQuotations, "+1,远");
        //// 场景5：全部类型（与"全"等效）
        //GetBestQuotationsBySettle(BidQuotations, "全,+1,+0,远");

        /// <summary>
        /// 根据清算速度组合获取最优价（全局最优价格版）
        /// </summary>
        /// <param name="srcQuotations">数据源(BidQuotations或OfrQuotations)</param>
        /// <param name="selSettle">清算速度组合，如"全,+1,+0"或"+1,+0,远"</param>
        /// <returns>最优价报价列表</returns>
        public List<GoodsVolumeVoModel> GetBestQuotationsBySettle(List<GoodsVolumeVoModel> srcQuotations, string selSettle)
        {
            // 参数验证
            if (srcQuotations == null || srcQuotations.Count == 0)
            {
                return new List<GoodsVolumeVoModel>();
            }

            // .NET 4.0: 使用 IsNullOrEmpty 而不是 IsNullOrWhiteSpace
            if (string.IsNullOrEmpty(selSettle))
            {
                return new List<GoodsVolumeVoModel>();
            }

            // .NET 4.0: Split 需要用 new[] { ',' } 语法
            string[] settleTypes = selSettle.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToArray();

            // 第一步：根据清算速度筛选数据
            List<GoodsVolumeVoModel> filteredQuotations = FilterBySettleType(srcQuotations, settleTypes);

            // 如果筛选后无数据，返回空列表
            if (filteredQuotations.Count == 0)
            {
                return new List<GoodsVolumeVoModel>();
            }

            // ============ 核心修改：全局最优价格 ============
            // 第二步：找到全局最小价格档位
            int globalMinLevel = filteredQuotations.Min(x => x.MDPriceLevel);

            // 第三步：获取最小档位的价格作为全局最优价
            decimal globalBestPrice = filteredQuotations.First(x => x.MDPriceLevel == globalMinLevel).price;

            // 第四步：筛选所有broker中价格等于全局最优价的记录
            List<GoodsVolumeVoModel> bestQuotations = filteredQuotations
                .Where(x => x.price == globalBestPrice)
                // 按 SourceId 升序、再按 MDPriceLevel 升序 排序
                .OrderBy(x => x.SourceId)
                .ThenBy(x => x.MDPriceLevel)
                .ToList();

            return bestQuotations;
        }
        /// <summary>
        /// 根据清算速度类型筛选数据（.NET 4.0兼容版）
        /// </summary>
        private List<GoodsVolumeVoModel> FilterBySettleType(List<GoodsVolumeVoModel> quotations, string[] settleTypes)
        {
            // 包含"全"，直接返回
            if (settleTypes.Contains("全"))
            {
                return quotations;
            }

            // 预判断需要检查的条件
            bool checkPlusOne = settleTypes.Contains("+1");
            bool checkPlusZero = settleTypes.Contains("+0");
            bool checkFar = settleTypes.Contains("远");

            // 如果没有有效条件
            if (!checkPlusOne && !checkPlusZero && !checkFar)
            {
                return new List<GoodsVolumeVoModel>();
            }

            // LINQ筛选
            return quotations.Where(q => (checkPlusOne && q.DictSettleType != null && (q.DictSettleType.ContainsKey("2,3") || q.DictSettleType.ContainsKey("2") || q.DictSettleType.ContainsKey("3"))) ||
            (checkPlusZero && q.DictSettleType != null && q.DictSettleType.ContainsKey("1")) || (checkFar && !string.IsNullOrEmpty(q.settlType) && q.settlType.Contains("+"))
            ).ToList();
        }
        #endregion

        #region Sub Funciton
        /// <summary>
        /// 获取 QB vol_delivery
        /// </summary>
        /// <param name="listMD"></param>
        /// <param name="is_vol_delivery_qb">true 返回 具体的日期</param>
        /// <returns></returns>
        public List<List<string>> GetQBVolDeliveryFromGoodsVolumeVoModel(List<GoodsVolumeVoModel> listMD, bool is_vol_delivery_qb = true)
        {
            List<List<string>> list = new List<List<string>>();
            try
            {
                for (int i = 0; i < listMD.Count; i++)
                {
                    GoodsVolumeVoModel item = listMD[i];
                    decimal itemqty = CommonUtils.ConvertToDecimal(item.MDEntrySize);
                    string itemqtyamount = itemqty == 0 ? CommonConstants.NO_PRICE : itemqty.ToString();
                    string itemsettl = item.settlType;
                    string itemsettl_convet = ResulfApiHelper.ConvertSettleType(itemsettl);
                    if (!string.IsNullOrWhiteSpace(itemsettl_convet))
                    {
                        if (int.Parse(itemsettl_convet) >= 4)
                        {
                            if (is_vol_delivery_qb)
                            {
                                List<string> sublist = new List<string>() { itemqtyamount, itemsettl_convet, itemsettl };
                                list.Add(sublist);
                            }
                            else
                            {
                                List<string> sublist = new List<string>() { itemqtyamount, itemsettl_convet };
                                list.Add(sublist);
                            }
                            
                        }
                        else
                        {
                            List<string> sublist = new List<string>() { itemqtyamount, itemsettl_convet };
                            list.Add(sublist);
                        }
                    }
                    else
                    {
                        List<string> sublist = new List<string>() { itemqtyamount, itemsettl_convet };
                        list.Add(sublist);
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex.Message);
            }

            return list;
        }

        public List<GoodsVolumeVoModel> GetFirstBySourceId(ObservableCollection<GoodsVolumeVoModel> _bestQuotations)
        {
            if (_bestQuotations == null) return new List<GoodsVolumeVoModel>();
            // 自定义排序规则
            var orderList = BrokerSortOrder.Split(',').ToList();

            var result = _bestQuotations
                .GroupBy(x => x.SourceId)
                .Select(g => g.OrderBy(x => x.MDPriceLevel).First())
                .OrderBy(x => orderList.IndexOf(x.SourceId))         // 按自定义顺序排序
                .ToList();

            return result;
        }

        public List<GoodsVolumeVoModel> GetBySourceIdSorted(ObservableCollection<GoodsVolumeVoModel> _bestQuotations, string _sourceId)
        {
            if (_bestQuotations == null) return new List<GoodsVolumeVoModel>();

            var result = _bestQuotations
                       .Where(x => x.SourceId == _sourceId)   // 只取指定 SourceId
                       .OrderBy(x => x.MDPriceLevel)         // 按 MDpriceLevel 升序
                       .ToList();

            return result;
        }

        public List<GoodsVolumeVoModel> GetBySourceIdSorted(List<GoodsVolumeVoModel> _bestQuotations, string _sourceId)
        {
            if (_bestQuotations == null) return new List<GoodsVolumeVoModel>();

            var result = _bestQuotations
                       .Where(x => x.SourceId == _sourceId)   // 只取指定 SourceId
                       .OrderBy(x => x.MDPriceLevel)         // 按 MDpriceLevel 升序
                       .ToList();

            return result;
        }

        public string MergeDescriptions(List<GoodsVolumeVoModel> listMD)
        {
            if (listMD == null || listMD.Count == 0)
                return string.Empty;

            var parts = new List<string>();

            foreach (var item in listMD)
            {
                string text = string.Empty;
                if (item.MDEntrySize.ToString() == "0")
                {
                    text = "";
                }
                else
                {
                    text = !string.IsNullOrEmpty(item.description) ? item.description : item.MDEntrySize.ToString();
                }

                parts.Add(text);
            }

            return string.Join("+", parts);
        }
        #endregion

        #endregion 双击行情看板 消息处理
        /// <summary>
        /// 转换FinQ单家最优,成交价 为WebFinQ请求  0：点击单家最优 2：点击成交价    ConvertSendFinQFriendReqToWebFinQ
        /// </summary>
        /// <param name="req"></param>
        public void ConvertSendFinQFriendReqToWebFinQ(SendFinQFriendReq req)
        {
            Dispatcher.CurrentDispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Send, new Action(() =>
            {
                if (string.IsNullOrWhiteSpace(req?.data?.code))
                {
                    return;
                }

                this.ClickDataType = req.dataType;

                if (this.SecurityID != req.data.code)
                {
                    FQSetActionData = "{}";
                    FQChangeBondData = "{}";
                }

                this.SecurityID = req.data.code;
                this.BrokeId = req.data.comp_id;
                this.Side = req.data.side;

                if (BondsInfo == null)
                {
                    if (!isLoadBond)
                        ShowWarningMessage("基础数据加载中，请耐心等待...");
                    else
                        ShowWarningMessage("未查到该债券基础信息...");

                    return;
                }

                //0 单家最优
                if (this.ClickDataType == 0)
                {
                    if ((FQSetActionData ?? "{}").ToString() == "{}")
                    {
                        isNeedSend = true;
                    }
                    else
                    {
                        GetAndSetSingleActionDataModel(true);
                    }
                    
                    //打开报价助手,等待数据更新
                    STC.Common.Handler.FinQ.FinQHandler.Instance.SendWebFQ();
                }

                //2：点击成交价
                if (ClickDataType == 2)
                {
                    // 发送到报价助手
                    FQDataModel fddm = new FQDataModel();
                    fddm.bondinfo = fqbondinfo;
                    if (BondsInfo.MarketIndicator == "5")
                        return;

                    fddm.bondkey = BondsInfo.Bond_Key;
                    fddm.listedmarket = BondsInfo.listed_Market;
                    fddm.fromQB = LocalEnv.UserInfo.UserName;
                    fddm.fromQBAccountID = LocalEnv.UserInfo.UserId;
                    fddm.openChat = true;
                    fddm.content = $"{GetNewContentWithReplceCcdc(GetInnerOrFirstLine(req.content))} {req.UpdateTime?.Replace(".000", "")}";
                    fddm.dataType = req.dataType;  //0：点击单家最优1：点击综合最优；2：点击成交价 

                    STC.Common.Model.FinQ.Data data = new STC.Common.Model.FinQ.Data();
                    data.comp_id = req.data.comp_id; //券商ID
                    data.bestbroker = ""; //是否最优经纪商 Y/N
                    data.broker_qq = "";// todo...  
                    data.code = req.data.code;
                    data.bond_type = req.data.bond_type;
                    data.name = req.data.name;
                    data.residual_maturity = BondsInfo.RemaingingTime;
                    data.bond_rating = ""; //债项评级   //todo...
                    data.issuer_rating = BondsInfo.Rating_Current;
                    data.side = req.data.side; //bid or ofr
                    data.price = req.data.price; //价格
                    data.price_type = string.IsNullOrEmpty(req.data.price_type) ? null : (req.data.price_type == "0" ? "" : req.data.price_type); //价格类型：0意向 1=净价；2=全价；3=收益率；4=利差
                    data.yield_type = data.opp_price_type = req.data.yield_type == "0" ? "EXERCISE" : "MATURITY"; //0=行权收益率,1或空=到期收益率
                    data.vol_delivery = ToListList(req.data.vol_delivery);
                    //data.vol_delivery_qb = req.data.vol_delivery_qb;
                    data.opp_price = req.data.opp_price;
                    data.opp_price_type = string.IsNullOrEmpty(req.data.opp_price_type) ? null : (req.data.opp_price_type == "0" ? "" : req.data.opp_price_type);
                    data.opp_vol_delivery = ToListList(req.data.opp_vol_delivery);
                    data.ext = "";// todo...
                    data.val_yield = CommonUtils.formatPriceString(CommonUtils.ConvertToString(req.data.val_yield), 4);
                    fddm.data = data;
                    fddm._uuid = Guid.NewGuid().ToString();
                    this.FQSetActionData = CommonUtils.SerializeObject(fddm);

                    Common.Handler.Base.BaseHandler.Instance.PublishAppMsg(new PResult() { Code = "getFQ", Info1 = "WebFQ", Info2 = "FQSetAction", Info3 = "" });
                    //打开报价助手
                    STC.Common.Handler.FinQ.FinQHandler.Instance.SendWebFQ();
                }
            }));
        }

        /// <summary>
        /// 转换FinQ综合最优为WebFinQ请求   1：点击综合最优   ConvertSendFinQFriendReq_BestToWebFinQ   fromQBAccountID临时保存用户选择的清算速度
        /// </summary>
        /// <param name="req"></param>
        public void ConvertSendFinQFriendReq_BestToWebFinQ(SendFinQFriendReq_Best req)
        {
            Dispatcher.CurrentDispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Send, new Action(() =>
            {
                if (req?.data?.Count == 0)
                    return;

                this.ClickDataType = req.dataType;//0或null：点击单家最优；1：点击综合最优；2：点击成交价

                if (this.SecurityID != req.data[0].code)
                {
                    FQSetActionData = "{}";
                    FQChangeBondData = "{}";
                }

                this.SecurityID = req.data[0].code;
                this.BrokeId = "";
                this.Side = req.data[0].side;
                this.SelSettle = req.fromQBAccountID; //临时保存用户选择的清算速度

                if (BondsInfo == null)
                {
                    if (!isLoadBond)
                        ShowWarningMessage("基础数据加载中，请耐心等待...");
                    else
                        ShowWarningMessage("未查到该债券基础信息...");
                    return;
                }

                // 发送到报价助手
                if (BondsInfo.MarketIndicator == "5")
                    return;

                //1 点击综合最优
                if (this.ClickDataType == 1)
                {
                    if ((FQSetActionData??"{}").ToString() == "{}")
                    {
                        isNeedSend = true;
                    }
                    else
                    {
                        GetAndSetMultiActionDataModel(true);
                    }

                    //打开报价助手,等待数据更新
                    STC.Common.Handler.FinQ.FinQHandler.Instance.SendWebFQ();
                }
            }));
        }

        #region Public Funciton
        /// <summary>
        /// 获取字符串中 [] 中的内容；
        /// 如果没有 []，则返回第一个 \r\n 之前的内容。
        /// </summary>
        public string GetInnerOrFirstLine(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            int start = input.IndexOf('[');
            int end = input.IndexOf(']');

            if (start >= 0 && end > start)
            {
                // 提取 [ ] 中的内容
                return input.Substring(start + 1, end - start - 1);
            }
            else
            {
                // 没有方括号，取第一个 \r\n 之前的部分
                int newlineIndex = input.IndexOf("\r\n", StringComparison.Ordinal);
                if (newlineIndex > 0)
                    return input.Substring(0, newlineIndex);
                else
                    return input; // 没有换行则返回整个字符串
            }
        }

        /// <summary>
        /// 获取字符串中第一个“中债估值:”之前的内容。
        /// </summary>
        public string GetTextBeforeZhongZhai(string input, string keyword)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            int index = input.IndexOf(keyword, StringComparison.Ordinal);

            if (index >= 0)
            {
                return input.Substring(0, index);
            }
            else
            {
                // 如果找不到，则返回原字符串
                return input;
            }
        }

        public string GetNewContentWithReplceCcdc(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            int index = input.IndexOf(CommonConstants.CDCValueTxt, StringComparison.Ordinal);
            string oldvalue = string.Empty;
            if (index >= 0)
            {
                oldvalue = input.Substring(index, input.Length - index).Replace(CommonConstants.CDCValueTxt, "");
            }

            input = GetTextBeforeZhongZhai(input, CommonConstants.CDCValueTxt);
            CCDCValuation = oldvalue;
            CCDCValuationToolTip = oldvalue.Contains("/") ? "行权/到期" : "";
            input = $"{input}   昨日中债 {oldvalue} {CCDCValuationToolTip}";
          
            return input;
        }

        public string GetCcdc()
        {
            string input = string.Empty;
            
            if (CCDCValuation2 == "--")
            {
                input = $"昨日中债 {CCDCValuation}";
            }
            else
            {
                input = $"昨日中债 {CCDCValuation2}  {CCDCValuationToolTip}";
            }

            return input;

        }


        public List<STC.Common.Model.QQ.DataMessage> GetBySourceIdSortedData(List<STC.Common.Model.QQ.DataMessage> _bestQuotations)
        {
            if (_bestQuotations == null) return new List<STC.Common.Model.QQ.DataMessage>();

            // 自定义排序规则
            var orderList = BrokerSortOrder.Split(',').ToList();

            // 数据处理逻辑
            var result = _bestQuotations
                .GroupBy(x => x.comp_id)                            // 按 SourceId 分组
                .Select(g => g.OrderBy(x => x.comp_id).First()) // 每组取 MDpriceLevel 最小的
                .OrderBy(x => orderList.IndexOf(x.comp_id))         // 按自定义顺序排序
                .ToList();

            return result;
        }

        public List<STC.Common.Model.QQ.DataMessage> GetBySourceIdSorted(List<STC.Common.Model.QQ.DataMessage> _bestQuotations, string _sourceId)
        {
            if (_bestQuotations == null) return new List<STC.Common.Model.QQ.DataMessage>();

            var result = _bestQuotations
                       .Where(x => x.comp_id == _sourceId)   // 只取指定 SourceId
                       .ToList();

            return result;
        }

        public List<List<string>> MergeVolDeliveryQB(List<STC.Common.Model.QQ.DataMessage> currSourceId)
        {
            if (currSourceId == null || currSourceId.Count == 0)
                return new List<List<string>>();

            var result = currSourceId
                .Where(x => x.vol_delivery_qb != null)  // 过滤掉空的
                .SelectMany(x => x.vol_delivery_qb)      // 扁平化合并所有子List
                .ToList();

            return result;
        }

        public List<List<string>> ToListList(string[,] array)
        {
            if (array == null)
                return new List<List<string>>();

            var result = new List<List<string>>();

            int rows = array.GetLength(0);
            int cols = array.GetLength(1);

            for (int i = 0; i < rows; i++)
            {
                var row = new List<string>();
                for (int j = 0; j < cols; j++)
                {
                    row.Add(array[i, j]);
                }
                result.Add(row);
            }

            return result;
        }

        // List<List<string>> → string[,]
        public string[,] To2DArray(List<List<string>> list)
        {
            if (list == null || list.Count == 0)
                return new string[0, 0];

            int rows = list.Count;
            int cols = list[0].Count; // 默认取第一行的列数

            var result = new string[rows, cols];

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    // 防止有的行比其他行短
                    if (j < list[i].Count)
                        result[i, j] = list[i][j];
                    else
                        result[i, j] = string.Empty; // 补空字符串
                }
            }

            return result;
        }

        // <summary>
        /// 计算 List<List<string>> 中第一个元素的累加和
        /// 如果元素是 "--"，按 0 处理
        /// </summary>
        public decimal SumFirstElements(List<List<string>> list)
        {
            if (list == null || list.Count == 0)
                return 0;

            decimal sum = 0;

            foreach (var row in list)
            {
                if (row == null || row.Count == 0)
                    continue;

                string value = row[0];

                decimal number = CommonUtils.ConvertToDecimal(value);

                sum += number;
            }

            return sum;
        }

        /// <summary>
        /// 获取 QB vol_delivery
        /// </summary>
        /// <param name="listMD"></param>
        /// <returns></returns>
        public static List<List<string>> GetQBVolDeliveryFromMD(List<GNoMDEntries> listMD)
        {
            List<List<string>> list = new List<List<string>>();
            try
            {
                for (int i = 0; i < listMD.Count; i++)
                {
                    GNoMDEntries item = listMD[i];
                    decimal itemqty = CommonUtils.ConvertToDecimal(item.MDEntrySize);
                    string itemqtyamount = itemqty == 0 ? CommonConstants.NO_PRICE : itemqty.ToString();
                    string itemsettl = item.SettlType;
                    string itemsettl_convet = ResulfApiHelper.ConvertSettleType(itemsettl);
                    if (!string.IsNullOrWhiteSpace(itemsettl_convet))
                    {
                        if (int.Parse(itemsettl_convet) >= 4)
                        {
                            List<string> sublist = new List<string>() { itemqtyamount, itemsettl_convet, itemsettl };
                            list.Add(sublist);
                        }
                        else
                        {
                            List<string> sublist = new List<string>() { itemqtyamount, itemsettl_convet };
                            list.Add(sublist);
                        }
                    }
                    else
                    {
                        List<string> sublist = new List<string>() { itemqtyamount, itemsettl_convet };
                        list.Add(sublist);
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex.Message);
            }

            return list;
        }


        /// <summary>
        /// 格式化小数
        /// 输入为字符串。
        ///如果输入为空或 null，返回 "--"。/尝试转换为 decimal；如果失败，返回 "--"。转换后：
        ///去掉小数点后多余的 0。
        ///根据小数部分的有效位数返回：
        ///有效小数 2 位 → 保留 2 位；
        ///有效小数 3 位 → 保留 3 位；
        ///有效小数 4 位 → 保留 4 位；
        ///没有小数 → 返回 .00。
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public string FormatAmount(string input)
        {
            // 判空
            if (input == null || input.Trim() == "")
                return "--";

            if (input == "bid" || input == "ofr")
                return "--";

            decimal value;
            if (!decimal.TryParse(input, out value))
                return "--";

            // 转成字符串（避免科学计数法）
            string str = value.ToString("0.################");

            // 判断是否有小数部分
            if (str.IndexOf('.') < 0)
                return str + ".00";

            string[] parts = str.Split('.');
            string integerPart = parts[0];
            string decimalPart = parts[1].TrimEnd('0');

            if (decimalPart.Length == 0)
                return integerPart + ".00";
            else if (decimalPart.Length <= 2)
                return integerPart + "." + decimalPart.PadRight(2, '0');
            else if (decimalPart.Length == 3)
                return integerPart + "." + decimalPart;
            else if (decimalPart.Length == 4)
                return integerPart + "." + decimalPart;
            else
                return integerPart + "." + decimalPart.Substring(0, 4);
        }

        /// <summary>
        /// 显示警告
        /// </summary>
        /// <param name="msg"></param>
        public void ShowWarningMessage(string msg)
        {
            Common.Handler.Base.BaseHandler.Instance.PublishAppMsg(new PResult() { Code = "getFQ", Info1 = "WebFQ", Info2 = "ShowFQMessage", Info3 = msg, Info4 = "FinQ提醒:" });
        }

        #endregion Funciton
    }
}


