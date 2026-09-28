using com.gateway.log;
using netapi.Api;
using STC.Common.Api;
using STC.Common.Handler.MarketExec;
using STC.Common.Handler.Price;
using STC.Common.Handler.TBook;
using STC.Common.Model.Base;
using STC.Common.Model.Table;
using STC.Common.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace STC.Common.Handler.Base
{
    public class BaseHandler : IBaseHandler
    {
        private bool isSubscribeForBase = false;

        private TPartydealerInfo _cacheDealerInfo = null;

        private BaseHandler() { }
        private static IBaseHandler baseHandler = new BaseHandler();


        public static IBaseHandler Instance { get { return baseHandler; } }

        //同步idc已经设置的债券信息完成信号
        private bool _syncIdc = true;

        public void SetSyncIdcStatus(bool status)
        {
            _syncIdc = status;
        }

        public bool GetSyncIdcStatus()
        {
            return _syncIdc;
        }


        #region 基础债券加载完成信号
        private bool _loadData = false;
        private event Action<bool> OnDataComplete;
        public bool IsLoad()
        {
            return _loadData;
        }
        public void SetLoadDataStatus(bool loadData)
        {
            _loadData = loadData;

            OnDataComplete?.Invoke(loadData);
        }

        public void SubscribeLoadDataStatus(Action<bool> onLoadData)
        {
            if (onLoadData != null)
            {
                OnDataComplete += onLoadData;
                if (_loadData)
                {
                    onLoadData.Invoke(_loadData);
                }
            }
        }

        public void UnSubscribeLoadDataStatus(Action<bool> onLoadData)
        {
            if (onLoadData != null)
                OnDataComplete -= onLoadData;
        }
        #endregion

        private event Action<ClearPo> OnClear;
        public void UnSubscribeAll(bool isSubscribe)
        {
            if (OnClear != null)
                OnClear = null;

            isSubscribeForBase = isSubscribe;
        }

        public void UnSubscribe(Action<ClearPo> onClearCallBack)
        {
            if (OnClear != null && onClearCallBack != null)
                OnClear -= onClearCallBack;
        }

        public void Init(Action<ClearPo> onOrderCallBack)
        {
            if (onOrderCallBack != null && (OnClear == null || !OnClear.GetInvocationList().Contains(onOrderCallBack)))
                OnClear += onOrderCallBack;

            lock (this)
            {
                if (!isSubscribeForBase)
                {
                    isSubscribeForBase = true;

                    ApiManager.Instance.GetDefaultApi().SubScribe(Instance_OnClear, new string[] { ApiConsts.Topic_stc_base_clear },
                        ApiConsts.BaseSvr, "");
                    //ClientApi.Instance.SubscribeBaseClear(Instance_OnClear);
                }
            }
        }

        public void InitSubscribe()
        {
            lock (this)
            {
                if (!isSubscribeForBase)
                {
                    isSubscribeForBase = true;
                    ApiManager.Instance.GetDefaultApi().SubScribe(Instance_OnClear, new string[] { ApiConsts.Topic_stc_base_clear },
                        ApiConsts.BaseSvr, "");//ClientApi.Instance.SubscribeBaseClear(Instance_OnClear);
                }
            }
        }

        private void Instance_OnClear(object clearPo)
        {
            ClearPo data = BaseHelper.ConvertStcMessage<ClearPo>(new string[] { ApiConsts.Topic_stc_base_clear }, clearPo);
            if (data != null)
                Instance_OnClear(data);
        }

        public void Reset()
        {
            try
            {
                Thread.Sleep(3000);

                ClearPo clearPo = new ClearPo();
                clearPo.IsClear = "Y";

                Instance_OnClear(clearPo);

            }
            catch (Exception e)
            {
                LogHelper.Error(e);
            }
        }

        private void Instance_OnClear(ClearPo clearPo)
        {
            LogHandler.Info("Base Clear");
            if (clearPo != null && "Y".Equals(clearPo.IsClear))
            {
                MarketExecHandler.Instance.OnClear();//.UnSubscribeAll(true);
                MDHandler.Instance.ClearCache();//UnSubscribeAll(true);
                TBookHandler.Instance.OnClear();//UnSubscribeAll(true);

                //BDB
                switch (Utils.LocalEnv.ServerInfo.AppName.ToLower())
                {
                    case "bdb":
                    case "idcclient":
                        MarketExecCache.Instance.OnClear();
                        break;
                }

                //MarketExecHandler.Instance.UnSubscribeAll(true);
                //MDHandler.Instance.UnSubscribeAll(true);
                //TBookHandler.Instance.UnSubscribeAll(true);
            }

            
            foreach(Action<ClearPo> d in OnClear.GetInvocationList())
            {
                try
                {
                    ThreadPoolHelper.InvokeThreadPool(o => {
                        d(clearPo);
                    });
                }
                catch (Exception ex)
                {
                    LogHelper.Error(ex);
                }
                
            }
            //OnClear?.Invoke(clearPo);
            
        }

        public BondsSearchReplyPo SearchBonds(BondsSearchRequestPo request)
        {
            return ApiManager.Instance.GetDefaultApi().Request<BondsSearchReplyPo>(ApiConsts.BaseSvr, ApiConsts.Topic_stc_base_search, request);
        }

        public TPartydealerInfo searchqueryPartyDealerInfoForBase(PartydealerReq request)
        {
            return ApiManager.Instance.GetDefaultApi().Request<TPartydealerInfo>(ApiConsts.BaseSvr, ApiConsts.Topic_stc_queryPartyDealerInfo, request);//ClientApi.Instance.searchqueryPartyDealerInfoForBase(request);
        }
        public TBondValuationReplyPo SearchChianSecurity(TBondValuationReq request)
        {
            return ApiManager.Instance.GetDefaultApi().Request<TBondValuationReplyPo>(ApiConsts.BaseSvr, ApiConsts.Topic_stc_queryChianSecurity, request);
        }

        public InstSearchReplyPo SearchInst(InstSearchParamPo request)
        {
            return ApiManager.Instance.GetDefaultApi().Request<InstSearchReplyPo>(ApiConsts.BaseSvr, ApiConsts.Topic_stc_base_inst_search, request);
        }

        #region 插件间消息通知
        private event Action<object> OnAppMsg;
        public Dictionary<string, Action<object>> DictCallBack = new Dictionary<string, Action<object>>();
        public void PublishAppMsg(object obj)
        {
            if (obj is Common.Model.Fix.Message.PResult)
            {
                OnAppMsg?.Invoke(obj);
            }
            
        }

        public void SubscribeAppMsg(Action<object> callBack)
        {
            if (callBack != null)
            {
                string callbackkey = $"{callBack.Method.DeclaringType.FullName.ToString()}.{callBack.Method.Name.ToString()}";

                if (!DictCallBack.ContainsKey(callbackkey))
                {
                    OnAppMsg += callBack;
                    DictCallBack.Add(callbackkey, callBack);
                }
            }
        }

        public void UnSubscribeAppMsg(Action<object> callBack)
        {
            if (callBack != null)
            {
                string callbackkey = $"{callBack.Method.DeclaringType.FullName.ToString()}.{callBack.Method.Name.ToString()}";

                if (DictCallBack.ContainsKey(callbackkey))
                {
                    DictCallBack.Remove(callbackkey);
                    OnAppMsg -= callBack;
                }
            }
        }


        #endregion


        #region  市场成交处理完成信号
        private bool _proctmarketexecutions = false;
        private event Action<bool> OnProcMarketExecutionsComplete;
        public void SubscribeProctMarketExecutionsStatus(Action<bool> onProcDataSuccess)
        {
            if (onProcDataSuccess != null)
            {
                OnProcMarketExecutionsComplete += onProcDataSuccess;
                if (_proctmarketexecutions)
                {
                    onProcDataSuccess.Invoke(_proctmarketexecutions);
                }
            }
        }

        public void SetProctMarketExecutionsStatus(bool proctmarketexecutions)
        {
            _proctmarketexecutions = proctmarketexecutions;

            OnProcMarketExecutionsComplete?.Invoke(proctmarketexecutions);
        }
        #endregion


        #region  市场成交是历史数据准备完成
        private bool _loadtmarketexecutions = false;
        private event Action<bool> OnLoadtMarketExecutionsComplete;
        public void SubscribeLoadtMarketExecutionsStatus(Action<bool> onLoadDataSuccess)
        {
            if (onLoadDataSuccess != null)
            {
                OnLoadtMarketExecutionsComplete += onLoadDataSuccess;
                if (_loadtmarketexecutions)
                {
                    OnLoadtMarketExecutionsComplete.Invoke(_loadtmarketexecutions);
                }
            }
        }

        public void SetLoadtMarketExecutionsStatus(bool loadtmarketexecutions)
        {
            _loadtmarketexecutions = loadtmarketexecutions;

            OnLoadtMarketExecutionsComplete?.Invoke(loadtmarketexecutions);
        }
        #endregion

        #region 债券标签信息
        public QueryQuoteTagReplyPo QueryQuoteTag()
        {
            //var x = ApiManager.Instance.GetDefaultApi().Request<QueryQuoteTagReplyPo>(ApiConsts.BaseSvr, ApiConsts.Topic_stc_QuoteTag, "{}");
            //string detail = Newtonsoft.Json.JsonConvert.SerializeObject(x);
            return ApiManager.Instance.GetDefaultApi().Request<QueryQuoteTagReplyPo>(ApiConsts.BaseSvr, ApiConsts.Topic_stc_QuoteTag, "{}");
        }

        public QueryTagsCountReplyPo QueryTagsCount(QueryTagsCountRequest req)
        {
            return ApiManager.Instance.GetDefaultApi().Request<QueryTagsCountReplyPo>(ApiConsts.BaseSvr, ApiConsts.Topic_stc_Tags_Count, req);
        }

        public QueryTagsLoadReplyPo QueryTagsLoad(QueryTagsLoadRequest req)
        {
            return ApiManager.Instance.GetDefaultApi().Request<QueryTagsLoadReplyPo>(ApiConsts.BaseSvr, ApiConsts.Topic_stc_Tags_Load, req);
        }

        public QueryQuoteTagReplyPo QueryMMSecurityidHist()
        {
            return ApiManager.Instance.GetDefaultApi().Request<QueryQuoteTagReplyPo>(ApiConsts.BaseSvr, ApiConsts.Topic_stc_QuoteTag, "{}");
        }
        #endregion

        #region 担保人加载完成信号
        private bool _loadData_Guarantor = false;
        private event Action<bool> OnGuarantorComplete;
        public void SetLoadDataStatus_Guarantor(bool loadData)
        {
            _loadData_Guarantor = loadData;

            OnGuarantorComplete?.Invoke(loadData);
        }

        public void SubscribeLoadData_Guarantor(Action<bool> onLoadData)
        {
            if (onLoadData != null)
            {
                OnGuarantorComplete += onLoadData;
                if (_loadData_Guarantor)
                {
                    onLoadData.Invoke(_loadData_Guarantor);
                }
            }
        }

        public void UnSubscribeLoadData_Guarantor(Action<bool> onLoadData)
        {
            if (onLoadData != null)
                OnGuarantorComplete -= onLoadData;
        }
        #endregion


        #region 发行人加载完成信号
        private bool _loadData_Issuer = false;
        private event Action<bool> OnIssuerComplete;
        public void SetLoadDataStatus_Issuer(bool loadData)
        {
            _loadData_Issuer = loadData;

            OnIssuerComplete?.Invoke(loadData);
        }

        public void SubscribeLoadData_Issuer(Action<bool> onLoadData)
        {
            if (onLoadData != null)
            {
                OnIssuerComplete += onLoadData;
                if (_loadData_Issuer)
                {
                    onLoadData.Invoke(_loadData_Issuer);
                }
            }
        }

        public void UnSubscribeLoadData_Issuer(Action<bool> onLoadData)
        {
            if (onLoadData != null)
                OnIssuerComplete -= onLoadData;
        }
        #endregion

        #region 获取Tags数据

        private Dictionary<string, Dictionary<string, string>> _tagsDic = new Dictionary<string, Dictionary<string, string>>();
        public Dictionary<string, Dictionary<string, string>> TagsDic
        {
            get { return _tagsDic; }
        }

        private Dictionary<string, QuoteTag> _QuoteTagDic = new Dictionary<string, QuoteTag>();

        private int retryCount = 0;

        public async void QueryTags(bool isCache = true)
        {
            try
            {
                lock (_QuoteTagDic)
                {
                    if (isCache && _QuoteTagDic != null && _QuoteTagDic.Count > 0)
                    {
                        return;
                    }


                    if (_QuoteTagDic != null)
                        _QuoteTagDic.Clear();

                    if (_tagsDic != null)
                        _tagsDic.Clear();
                }

                QueryQuoteTagReplyPo queryQuoteTagReplyPo = null;
                await Task.Run(() =>
                {
                    retryCount++;
                    queryQuoteTagReplyPo = BaseHandler.Instance.QueryQuoteTag();
                    LogHandler.Info("BaseSvr, QueryQuoteTag");
                    lock (_QuoteTagDic)
                    {
                        if (queryQuoteTagReplyPo != null && queryQuoteTagReplyPo.IsSuccess())
                        {
                            if (queryQuoteTagReplyPo.Datas != null && queryQuoteTagReplyPo.Datas.Count > 0)
                            {
                                foreach (QuoteTag qt in queryQuoteTagReplyPo.Datas)
                                {
                                    _QuoteTagDic[qt.tagType] = qt;

                                    if (!_tagsDic.ContainsKey(qt.tagType))
                                    {
                                        _tagsDic[qt.tagType] = new Dictionary<string, string>();
                                    }

                                    if (qt.tagType == "INDUSTRY_TYPE" || qt.tagType == "IRS_PRODUCT_TYPE")
                                    {
                                        if (qt.childTags != null)
                                        {
                                            foreach (ChildTag ct in qt.childTags)
                                            {
                                                if (ct.childTags != null)
                                                {
                                                    foreach (ChildTag sub_ct in ct.childTags)
                                                    {
                                                        if (sub_ct.childTagName == "无")
                                                            continue;

                                                        _tagsDic[qt.tagType][sub_ct.childTagValue] = sub_ct.childTagName;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    else if (qt.tagType == "ISSUER" || qt.tagType == "GUARANTOR")
                                    {

                                    }
                                    else
                                    {
                                        //if (qt.tagType == "ORG_TYPE"
                                        //    || qt.tagType == "BOND_SUB_TYPE"
                                        //    || qt.tagType == "RANKING_TYPE"
                                        //    || qt.tagType == "RATING_CURRENT"
                                        //    || qt.tagType == "LOCAL_GOV_BOND_TYPE"
                                        //    || qt.tagType == "ADMIN_LEVEL"
                                        //    || qt.tagType == "REGION_TYPE"
                                        //    || qt.tagType == "ENTERPRISE_TYPE"
                                        //    || qt.tagType == "ISSUE_TYPE")
                                        //{
                                        foreach (ChildTag ct in qt.childTags)
                                        {
                                            if (ct.childTagName == "无")
                                                continue;

                                            _tagsDic[qt.tagType][ct.childTagValue] = ct.childTagName;
                                        }
                                        //}
                                    }
                                }
                            }
                        }
                        else
                        {
                            // 如果超时未取到数据，重试一次
                            while ((_QuoteTagDic == null || _QuoteTagDic.Count == 0) && retryCount < 3)
                            {
                                Thread.Sleep(5000);
                               
                                LogHelper.Info($"重试获取标签信息,重试次数{retryCount}");
                                QueryTags();
                            }

                            if (retryCount >= 3)
                            {
                                LogHelper.Error("获取标签信息失败！部分功能将受到影响！");
                            }
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
            }
        }

        public bool FinishLoadTags()
        {
            return (_QuoteTagDic != null && _QuoteTagDic.Count > 0) || retryCount >= 3;
        }

        public Dictionary<string, QuoteTag> GetQuoteTagDic()
        {
            lock (_QuoteTagDic)
            {
                return _QuoteTagDic;
            }
        }

        #endregion

        #region 登录成功信号
        private bool _isloginOk = false;
        private event Action<bool> OnloginOk;
        public void SetloginOk(bool loadData)
        {
            _isloginOk = loadData;

            OnloginOk?.Invoke(loadData);
        }

        public void SubscribeLoginOk(Action<bool> onLoadData)
        {
            if (onLoadData != null)
            {
                OnloginOk += onLoadData;
                if (_isloginOk)
                {
                    onLoadData.Invoke(_isloginOk);
                }
            }
        }
        #endregion
    }
}
