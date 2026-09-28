using com.gateway.log;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using STC.Common.Api;
using STC.Common.Handler.Admin;
using STC.Common.Handler.Base;
using STC.Common.Handler.Cal;
using STC.Common.Handler.Table;
using STC.Common.Model.Base;
using STC.Common.Model.Cal;
using STC.Common.Model.FinQ;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.Table;
using STC.Common.Utils;
using STC.Common.View.Drp;
using STC.Common.View.Drp.Component;
using STC.Common.View.StrongNotify;
using STC.Common.ViewModel.Data;
using STC.Common.ViewModel.Drp.Component;
using STC.Common.ViewModel.Order.InnerPanel;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace STC.Common.Data
{
    public class IntentionOldHandler
    {
        private bool isPermission = false; //查看机构内部权限


        //需求录入:代他人录入需求
        private readonly string MODULEID_ORDEROTHER = "6002";
        private readonly string ORDEROTHER = "600201";
        private bool isOrderOther = false;//是否录入他人订单

        //执行权限
        private readonly string MODULEID_INNERTYPE = "6001";
        private readonly string MM = "600101";//做市
        private readonly string SALER = "600102";//销交


        private readonly object m_MarketQuoteLock = new object();
        private Action<TableIntentionInquiry> _OnMarketQuotes;
        public event Action<TableIntentionInquiry> OnMarketQuotes
        {
            add
            {
                lock (m_MarketQuoteLock)
                {
                    _OnMarketQuotes += value;
                }
            }
            remove
            {
                lock (m_MarketQuoteLock)
                {
                    _OnMarketQuotes -= value;
                }
            }
        }

        // 市场询价
        /// <summary>
        /// 缓存我的市场询价
        /// </summary>
        private ConcurrentDictionary<string, ConcurrentDictionary<string, TableIntentionInquiry>> _CacheMarketQuotes = new ConcurrentDictionary<string, ConcurrentDictionary<string, TableIntentionInquiry>>();

        private readonly object m_HistoryMarketQuoteLock = new object();
        private Action<List<TableIntentionInquiry>> _OnHistoryMarketQuotes;
        public event Action<List<TableIntentionInquiry>> OnHistoryMarketQuotes
        {
            add
            {
                lock (m_HistoryMarketQuoteLock)
                {
                    _OnHistoryMarketQuotes += value;
                }
                System.Threading.ThreadPool.QueueUserWorkItem(o =>
                {
                    List<TableIntentionInquiry> historyDatas = new List<TableIntentionInquiry>();
                    foreach(ConcurrentDictionary<string, TableIntentionInquiry> dic in _CacheMarketQuotes.Values)
                    {
                        historyDatas.AddRange(dic.Values);
                    }
                    value?.Invoke(historyDatas);
                });
            }
            remove
            {
                lock (m_HistoryMarketQuoteLock)
                {
                    _OnHistoryMarketQuotes -= value;
                }
            }
        }

        // 订阅意向单识别
        private readonly Object m_OnRecognitionEventLock = new Object();
        private Action<STC.Common.Model.Table.WorkBrench.IntentionQuoteInfo> _OnRecognition;
        public event Action<STC.Common.Model.Table.WorkBrench.IntentionQuoteInfo> OnRecognition
        {
            add
            {
                lock (m_OnRecognitionEventLock)
                {
                    _OnRecognition += value;
                }
            }
            remove
            {
                lock (m_OnRecognitionEventLock)
                {
                    _OnRecognition -= value;
                }
            }
        }


        // 订阅我的订单(需求工作台)
        private readonly Object m_OnIntentionQuoteEventLock = new Object();
        private Action<STC.Common.Model.Table.WorkBrench.IntentionQuoteInfo> _OnIntentionQuotes;
        public event Action<STC.Common.Model.Table.WorkBrench.IntentionQuoteInfo> OnIntentionQuotes
        {
            add
            {
                lock (m_OnIntentionQuoteEventLock)
                {
                    _OnIntentionQuotes += value;
                }
            }
            remove
            {
                lock (m_OnIntentionQuoteEventLock)
                {
                    _OnIntentionQuotes -= value;
                }
            }
        }

        private readonly object m_OnHistoryIntentionQuoteEventLock = new Object();
        private Action<List<STC.Common.Model.Table.WorkBrench.IntentionQuoteInfo>> _OnHistoryIntentionQuotes;
        public event Action<List<STC.Common.Model.Table.WorkBrench.IntentionQuoteInfo>> OnHistoryIntentionQuotes
        {
            add
            {
                lock (m_OnHistoryIntentionQuoteEventLock)
                {
                    _OnHistoryIntentionQuotes += value;
                }
                System.Threading.ThreadPool.QueueUserWorkItem(o =>
                {
                    List<STC.Common.Model.Table.WorkBrench.IntentionQuoteInfo> historyDatas = new List<Model.Table.WorkBrench.IntentionQuoteInfo>();
                    historyDatas.AddRange(_CacheIntentionQuotes.Values);
                    value?.Invoke(historyDatas);
                });
            }
            remove
            {
                lock (m_OnHistoryIntentionQuoteEventLock)
                {
                    _OnHistoryIntentionQuotes -= value;
                }
            }
        }

        /// <summary>
        /// 缓存我的意向单(订阅)
        /// </summary>
        private ConcurrentDictionary<string, STC.Common.Model.Table.WorkBrench.IntentionQuoteInfo> _CacheIntentionQuotes = new ConcurrentDictionary<string, STC.Common.Model.Table.WorkBrench.IntentionQuoteInfo>();


        // 订阅意向单识别
        private readonly Object m_OnQorEventLock = new Object();
        private Action<STC.Common.Model.Table.EmsOrderReport> _OnQor;
        public event Action<STC.Common.Model.Table.EmsOrderReport> OnQor
        {
            add
            {
                lock (m_OnQorEventLock)
                {
                    _OnQor += value;
                }
            }
            remove
            {
                lock (m_OnQorEventLock)
                {
                    _OnQor -= value;
                }
            }
        }

        private readonly object m_OnHistoryQorEventLock = new object();
        private Action<List<STC.Common.Model.Table.EmsOrderReport>> _OnHistoryQor;
        public event Action<List<STC.Common.Model.Table.EmsOrderReport>> OnHistoryQor
        {
            add
            {
                lock (m_OnHistoryQorEventLock)
                {
                    _OnHistoryQor += value;
                }
            }
            remove
            {
                lock (m_OnHistoryQorEventLock)
                {
                    _OnHistoryQor -= value;
                }
            }
        }


        private Dictionary<string, QuoteTag> _QuoteTagDic = new Dictionary<string, QuoteTag>();
        //private Dictionary<string, string> _orgDic = new Dictionary<string, string>();
        //public Dictionary<string, string> OrgDic
        //{
        //    get { return _orgDic; }
        //}

        //private Dictionary<string, string> _BondSubTypeDic = new Dictionary<string, string>();

        //public Dictionary<string, string> BondSubTypeDic
        //{
        //    get { return _BondSubTypeDic; }
        //}

        private Dictionary<string, Dictionary<string, string>> _tagsDic = new Dictionary<string, Dictionary<string, string>>();
        public Dictionary<string, Dictionary<string, string>> TagsDic
        {
            get { return _tagsDic; }
        }

        /// IntentionHandler实例
        /// </summary>
        private static IntentionOldHandler server = new IntentionOldHandler();
        public static IntentionOldHandler Instance { get { return server; } }

        /// <summary>
        /// 构造函数
        /// </summary>
        private IntentionOldHandler()
        {
            InitGroupTag();

            ThreadPoolHelper.InvokeThreadPool(o =>
            {
                BaseHandler.Instance.Init(Instance_OnClear);

                isPermission = AdminHandler.Instance.CheckPermisionById(CommonConstants.MODELID_5000, CommonConstants.CODE_5002);
            });

            isOrderOther = AdminHandler.Instance.CheckPermission(MODULEID_ORDEROTHER, ORDEROTHER);
        }

        public bool IsOrderOther
        {
            get { return isOrderOther; }
        }

        private volatile bool isInit = false;

        /// <summary>
        /// 初始化
        /// </summary>
        public void init()
        {
            ThreadPoolHelper.InvokeThreadPool(o =>
            {
                if (isInit == false)
                {
                    isInit = true;

                    string date = DateTimeEx.ServerTime.ToString("yyyy-MM-dd");
                    RequestMarketQuotes(date, date);

                    TableOldHandler.Instance.SubscribeRecognization(new string[] { $"{ApiConsts.Topic_Old_stc_table_ir}{LocalEnv.UserInfo.UserName}" }, Instance_OnRecognition);
                    //TBookHandler.Instance.SubscribeIntention(new string[] { $"{ApiConsts.Topic_stc_tbook_iq}{LocalEnv.UserInfo.UserName}.**" }, Instance_OnIntention);
                    TableOldHandler.Instance.SubscribeIntention(new string[] { $"{ApiConsts.Topic_Old_stc_table_iq}{LocalEnv.UserInfo.UserName}", $"{ApiConsts.Topic_Old_stc_table_iqbiz}{LocalEnv.UserInfo.Termids}.{LocalEnv.UserInfo.UserName}" }, Instance_OnIntention);

                    TableOldHandler.Instance.SubscribeInquiry(new string[] { $"{ApiConsts.Topic_stc_table_inq}{LocalEnv.UserInfo.Termids}.{LocalEnv.UserInfo.UserName}" }, Instance_OnInquiry);

                    if (LocalEnv.UserInfo.RoleType == RoleType.BUYER_ROLE)
                    {
                        RequestIntentionQuotes(null, null, "BUYER_INTENTION");
                        RequestIntentionQuotes(null, DateTimeEx.ServerTime.AddDays(-1).ToString("yyyy-MM-dd"), "BUYER_INTENTION", "80,81,82");
                        RequestIntentionQuotes(null, null, "DEMAND");
                    }
                    else
                    {
                        RequestIntentionQuotes(null, null, "INTENTION");
                        RequestIntentionQuotes(null, null, "DEMAND");
                    }
                    

                    //订阅App消息
                    Common.Handler.Base.BaseHandler.Instance.SubscribeAppMsg(OnAppMsg);
                }
            });
        }

        private volatile bool isInit_InnerBoard = false;
        public void initInnerBoard()
        {
            ThreadPoolHelper.InvokeThreadPool(o =>
            {
                if (isInit_InnerBoard == false)
                {
                    isInit_InnerBoard = true;

                    TableOldHandler.Instance.SubscribeQor(Instance_OnQor, isPermission);
                }
            });
        }

        /// <summary>
        /// 清理回调方法
        /// </summary>
        /// <param name="obj"></param>
        private void Instance_OnClear(ClearPo obj)
        {
            LogHelper.Info("IntentionHandler Clear");
            if (obj != null && "Y".Equals(obj.IsClear))
            {
                _CacheIntentionQuotes.Clear();
                if (_CacheMarketQuotes != null)
                {
                    _CacheMarketQuotes.Clear();
                }

                isInit = false;
                isInit_InnerBoard = false;

                init();
                initInnerBoard();
            }
        }

        //List<STC.Common.Model.Table.WorkBrench.IntentionQuoteInfo> quoteList = new List<STC.Common.Model.Table.WorkBrench.IntentionQuoteInfo>();
        public void ImportIntentionQuote(string content, DataTable dt = null)
        {

            try
            {
                STC.Common.Model.Table.WorkBrench.RecognizeIntentionQuoteRequest request = new STC.Common.Model.Table.WorkBrench.RecognizeIntentionQuoteRequest();
                request.userName = LocalEnv.UserInfo.UserName;
                request.content = content;

                //lock (quoteList)
                //{
                //    quoteList.Clear();
                //}
                ///获取订单 
                STC.Common.Model.Table.WorkBrench.RecognizeIntentionQuoteReplyPo reply = ParserHandler.Instance.RequestParserBond(request);

                if (reply == null)
                {
                    CommonUtils.Prompt2Second("警告", $"报价识别失败");
                }
                else
                {
                    if (!reply.IsSuccess())
                    {
                        CommonUtils.Prompt2Second("警告", reply.Msg);
                    }
                    else
                    {
                        //// 等待识别数据返回
                        //int i = 0;
                        //while (i < 5)
                        //{
                        //    int count = quoteList.Count;
                        //    Thread.Sleep(500);

                        //    if (count != quoteList.Count)
                        //    {
                        //        i = 0;
                        //    }
                        //    else
                        //    {
                        //        i++;
                        //    }
                        //}

                        // 插入数据
                        STC.Common.Model.Table.WorkBrench.InsertIntentionQuoteReq insertReq = new STC.Common.Model.Table.WorkBrench.InsertIntentionQuoteReq();
                        insertReq.quoteList = reply.Datas;
                        if (dt != null)
                        {
                            // 检查识别数据是否正确， 现只检测整量， 其他字段由报价识别获取
                            int index = 0;
                            foreach (STC.Common.Model.Table.WorkBrench.IntentionQuoteInfo intentionQuoteInfo in reply.Datas)
                            {
                                string originalWholeFlag = dt.Rows[index]["是否整量"].ConvertToString() == "整" ? "1" : "0";
                                if (intentionQuoteInfo.wholeFlag != originalWholeFlag)
                                {
                                    intentionQuoteInfo.wholeFlag = originalWholeFlag;
                                }
                                index++;
                            }

                        }
                        InsertIntentionQuote(insertReq);


                        //if (reply.Datas != null)
                        //{
                        //    foreach (IntentionQuoteInfo obj in reply.Datas)
                        //    {
                        //        _CacheIntentionQuotes[obj.intentionId] = obj;
                        //        _OnIntentionQuotes?.Invoke(obj);
                        //    }
                        //}
                    }
                }
            }
            catch (Exception e)
            {
                LogHelper.Error("RequestOrders", e);
            }

        }

        public void RequestIntentionQuotes(string startDate, string endDate, string quoteType = "INTENTION", string status = null)
        {
            try
            {
                STC.Common.Model.Table.WorkBrench.IntentionQuoteRequest request = new STC.Common.Model.Table.WorkBrench.IntentionQuoteRequest();
                request.quoteType = quoteType;
                request.userName = LocalEnv.UserInfo.UserName;//为closeby

                request.status = status;

                if (!string.IsNullOrEmpty(startDate) || !string.IsNullOrEmpty(endDate))
                {
                    request.startdate = startDate;
                    request.enddate = endDate;
                }
                else
                {
                    string today = DateTimeEx.ServerTime.ToString("yyyy-MM-dd");
                    request.date = today;
                }

                if (!string.IsNullOrEmpty(LocalEnv.UserInfo.Termids))
                {
                    request.biztypeid = LocalEnv.UserInfo.Termids;
                }

                ///获取订单 
                var reply = TableOldHandler.Instance.QueryIntentionQuote(request);

                if (reply != null && reply.Datas != null)
                {
                    foreach (STC.Common.Model.Table.WorkBrench.IntentionQuoteInfo obj in reply.Datas)
                    {
                        _CacheIntentionQuotes.AddOrUpdate(obj.intentionId, obj, (k,v) => obj);
                    }

                    _OnHistoryIntentionQuotes?.Invoke(reply.Datas);
                }
            }
            catch (Exception e)
            {
                LogHelper.Error("RequestOrders", e);
            }
        }

        public STC.Common.Model.Table.WorkBrench.ConfirmIntentionQuoteReplyPo ConfirmIntentionQuote(STC.Common.Model.Table.WorkBrench.ConfirmIntentionQuoteRequest request)
        {
            return TableOldHandler.Instance.ConfirmIntentionQuote(request);
        }

        public Common.Model.Table.WorkBrench.InsertIntentionQuoteReplyPo InsertIntentionQuote_Common(STC.Common.Model.Table.WorkBrench.InsertIntentionQuoteReq request)
        {
            if (LocalEnv.UserInfo.RoleType == RoleType.BUYER_ROLE)
            {
                if (request != null && request.quoteList != null && request.quoteList.Count > 0)
                {
                    foreach (STC.Common.Model.Table.WorkBrench.IntentionQuoteInfo record in request.quoteList)
                    {
                        TagData fqInfo = new TagData();
                        fqInfo.StrategyOrderId = $"{request.quoteType}_{record.partyTrader}_{record.securityId}_{record.side}";
                        record.info2 = JsonConvert.SerializeObject(fqInfo);
                    }
                }
            }
            Common.Model.Table.WorkBrench.InsertIntentionQuoteReplyPo result = TableOldHandler.Instance.InsertIntentionQuote(request);
            return result;
        }

        public Task<Common.Model.Table.WorkBrench.InsertIntentionQuoteReplyPo> InsertIntentionQuote(STC.Common.Model.Table.WorkBrench.InsertIntentionQuoteReq request)
        {
            return Task.Run(() =>
            {
                if (request.quoteList == null || request.quoteList.Count == 0)
                    return null;

                foreach (STC.Common.Model.Table.WorkBrench.IntentionQuoteInfo intentionQuoteInfo in request.quoteList)
                {
                    if (intentionQuoteInfo.price.HasValue)
                    {
                        if (intentionQuoteInfo.priceType == "1"
                        || intentionQuoteInfo.priceType == "2"
                        || intentionQuoteInfo.priceType == "7")
                        {
                            if (intentionQuoteInfo.tempPrice.HasValue)
                            {
                                CalResult calResult = CommonUtils.CalBond_PriceType(intentionQuoteInfo.securityId, intentionQuoteInfo.settlType, intentionQuoteInfo.tempPrice?.ToString(), intentionQuoteInfo.priceType);
                                if (calResult != null && calResult.Data != null)
                                {
                                    if (intentionQuoteInfo.priceType == "7")
                                    {
                                        intentionQuoteInfo.price = intentionQuoteInfo.customerPrice = calResult.Data.YieldToExecution * 100;
                                    }
                                    else
                                    {
                                        intentionQuoteInfo.price = intentionQuoteInfo.customerPrice = calResult.Data.Yield * 100;
                                    }
                                        
                                    intentionQuoteInfo.cleanPrice = calResult.Data.CleanPrice;
                                    intentionQuoteInfo.fullPrice = calResult.Data.FullPrice;
                                }
                            }
                        }
                    }
                }

                Common.Model.Table.WorkBrench.InsertIntentionQuoteReplyPo result = InsertIntentionQuote_Common(request);
                if (result == null)
                {
                    CommonUtils.Prompt2Second("警告", $"意向报价添加失败");
                }
                else
                {
                    if (!result.IsSuccess() && result.Code != "10000")
                    {
                        CommonUtils.Prompt2Second("警告", result.Msg);
                    }
                    else
                    {
                        return result;
                    }
                }
                return result;
            });
        }

        public RecognizeIntentionQuoteReplyPo PublishIntention(NewOrderSingle request)
        {
            request.StrategyOrderId = null;
            request.ClOrdID = DataTimeSequence.Instance.GetId();
            RecognizeIntentionQuoteReplyPo replyPo = TableHandler.Instance.PublishIntention(request);
            return replyPo;
        }

        public Task<bool> UpdateIntentionQuote(STC.Common.Model.Table.WorkBrench.UpdateIntentionQuoteRequest request)
        {
            return Task.Run(() =>
            {
                bool isSuccess = false;
                if (string.IsNullOrEmpty(request.intentionId))
                    return isSuccess;

                PResult result = TableOldHandler.Instance.UpdateIntentionQuote(request);
                if (result == null)
                {
                    CommonUtils.Prompt2Second("警告", $"意向id:[{request.intentionId}]修改失败");
                }
                else
                {
                    if (!result.IsSuccess())
                    {
                        CommonUtils.Prompt2Second("警告", result.Msg);
                    }
                    else
                    {
                        isSuccess = true;
                    }
                }
                return isSuccess;
            });
        }

        public void Instance_OnIntention(STC.Common.Model.Table.WorkBrench.IntentionQuoteInfo obj)
        {
            //if (!string.IsNullOrEmpty(obj.closeBy) && obj.closeBy != LocalEnv.UserInfo.UserName)
            //{
            //    return;
            //}

            lock (string.Intern(obj.intentionId))
            {
                _CacheIntentionQuotes.AddOrUpdate(obj.intentionId, obj, (key, value) => obj);
            }

            //if ((obj.channel == "FINQ" && (!CommonUtils.SaleDic.ContainsKey("IsAuto_FQ") || CommonUtils.SaleDic["IsAuto_FQ"] != "1"))
            //    || (obj.channel == "WECHAT" && (!CommonUtils.SaleDic.ContainsKey("IsAuto_WX") || CommonUtils.SaleDic["IsAuto_WX"] != "1"))
            //    || (obj.channel == "WEBCLIENT" && (!CommonUtils.SaleDic.ContainsKey("IsAuto") || CommonUtils.SaleDic["IsAuto"] != "1")))
            //{
            //    if (obj.status == "0" && obj.updateTime == obj.createTime)
            //    {
            //        ShowDealIntention(new Common.View.Drp.NotificationContent() { Id = obj.intentionId, ContentType = obj.channel, Detail = "", IsShow = true, DelFlag = "0", Message = JsonConvert.SerializeObject(obj), Title = "", Type = NotificationType.ORDERCOMFIRM });
            //    }
            //}
            if (obj.info2 != null)
            {
                Dictionary<string, object> pwdDic = JsonConvert.DeserializeObject<Dictionary<string, object>>(obj.info2);

                if (LocalEnv.UserInfo.RoleType == RoleType.BUYER_ROLE)
                {
                    if (obj.quotetype == "BUYER_INTENTION" && obj.status == "80" && pwdDic != null && pwdDic.ContainsKey("AUTO_INTENTION") && Convert.ToInt32(pwdDic["AUTO_INTENTION"]) == 0 && obj.partyTrader == LocalEnv.UserInfo.UserName)
                    {
                        if(obj.createTime.Length >= 19 && obj.updateTime.Length >= 19 && obj.createTime.Substring(0, 19) == obj.updateTime.Substring(0, 19))
                        {
                            ShowDealIntention(obj, false);
                        }
                    }
                }
                else
                {
                    if (pwdDic != null && pwdDic.ContainsKey("AUTO_PLACE") && Convert.ToInt32(pwdDic["AUTO_PLACE"]) == 0)
                    {
                        if (obj.updateTime == obj.createTime)
                        {
                            ShowDealIntention(obj, false);
                        }
                    }
                }
            }

            if (obj.quotetype != "BUYER_INTENTION")
            {
                if (!(obj.status == "0" || obj.status == "00"))
                {
                    ShowDealIntention(obj, true);
                }
            }
                

            if (LocalEnv.UserInfo.RoleType == RoleType.BUYER_ROLE && !CommonUtils.IsInvestor())
            {
                // 弹框确认是否ref

                if (obj.status == "82" && IntentionOldHandler.Instance.ExtractByKey(obj.subStatus, "REF_STATUS") == "1" && obj.partyTrader == LocalEnv.UserInfo.UserName)
                {
                    ShowTraderRefConfirm(obj, false);
                }
                else
                {
                    ShowTraderRefConfirm(obj, true);
                }
            }

            _OnIntentionQuotes?.Invoke(obj);
        }

        public string ExtractByKey(string subStatus, string key)
        {
            if (JsonSplit.IsJson(subStatus))
            {
                JObject jsonObj = JObject.Parse(subStatus);
                if (jsonObj.ContainsKey(key))
                {
                    return jsonObj[key].ToString();
                }
            }
            if (key == "MATCH_STATUS")
            {
                return "5";
            }
            else if (key == "EXEC_STATUS")
            {
                return "9";
            }
            else
            {
                return string.Empty;
            }
        }

        public STC.Common.Model.Table.WorkBrench.IntentionQuoteInfo GetIntentionByID(string id)
        {
            if (_CacheIntentionQuotes.ContainsKey(id))
            {
                return _CacheIntentionQuotes[id];
            }
            return null;
        }

        public STC.Common.Model.Table.WorkBrench.IntentionQuoteInfo GetSameIntentionInfo(string partymanagerid, string partyTraderid, string securityid, string side, string accountid)
        {
            if (_CacheIntentionQuotes != null)
            {
                return _CacheIntentionQuotes.Values.FirstOrDefault(e => e.quotetype == "BUYER_INTENTION" && (e.status == "80" || e.status == "81" || e.status == "82") && e.securityId == securityid && e.side == side 
                && ((string.IsNullOrEmpty(partymanagerid) && string.IsNullOrEmpty(e.partyManager)) || e.partyManager == partymanagerid) 
                && ((string.IsNullOrEmpty(partyTraderid) && string.IsNullOrEmpty(e.partyTrader)) || e.partyTrader == partyTraderid)
                && ((string.IsNullOrEmpty(accountid) && string.IsNullOrEmpty(e.accountID)) || e.accountID == accountid));
            }
            return null;
        }

        public IEnumerable<STC.Common.Model.Table.WorkBrench.IntentionQuoteInfo> GetSameGroupDataList(string quoteType, string partyTraderid, string securityid, string side)
        {
            if (_CacheIntentionQuotes != null)
            {
                return _CacheIntentionQuotes.Values.Where(e => e.quotetype == "BUYER_INTENTION" && e.partyTrader == partyTraderid && e.securityId == securityid && e.side == side);
            }
            return null;
        }

        public void Instance_OnRecognition(STC.Common.Model.Table.WorkBrench.IntentionQuoteInfo obj)
        {
            _OnRecognition?.Invoke(obj);

            //lock (quoteList)
            //{
            //    quoteList.Add(obj);
            //}
        }


        public void RequestQor()
        {
            ThreadPoolHelper.InvokeThreadPool((o) =>
            {
                try
                {
                    QueryEmsOrderReq request = new QueryEmsOrderReq();
                    request.dealTimeBegin = $"{DateTimeEx.ServerTime.ToString("yyyy-MM-dd")} 00:00:00.000";
                    //if (!string.IsNullOrEmpty(LocalEnv.UserInfo.Termids))
                    //{
                    //    request.reqParams = new Dictionary<string, string>();
                    //    request.reqParams["biztypeid"] = LocalEnv.UserInfo.Termids;
                    //}
                    StringBuilder biztypeids = new StringBuilder();
                    if (!string.IsNullOrEmpty(LocalEnv.UserInfo.Termids))
                    {
                        biztypeids.Append(LocalEnv.UserInfo.Termids);
                    }
                    if (LocalEnv.UserInfo.mdgroupids != null && LocalEnv.UserInfo.mdgroupids.Count > 0)
                    {
                        foreach (string otherBiztypeid in LocalEnv.UserInfo.mdgroupids)
                        {
                            if (biztypeids.Length > 0)
                            {
                                biztypeids.Append(",");
                            }
                            biztypeids.Append(otherBiztypeid);
                        }
                    }
                    if (biztypeids.Length > 0)
                    {
                        request.reqParams = new Dictionary<string, string>();
                        request.reqParams["biztypeid"] = biztypeids.ToString();
                    }
                    if (!isPermission)
                        request.userName = LocalEnv.UserInfo.UserName;
                    int index = 1;
                    request.pageSize = 2000;

                    bool flag = true;
                    while (flag)
                    {
                        request.pageNum = index;
                        ///获取订单 
                        QueryEmsOrderReplyPo reply = TableHandler.Instance.QueryEmsOrder(request);

                        if (reply == null)
                        {
                            flag = false;
                            break;
                        }

                        if (reply != null && reply.Datas != null)
                        {
                            _OnHistoryQor?.Invoke(reply.Datas);
                            if (reply.Datas.Count < request.pageSize)
                            {
                                flag = false;
                            }
                            else
                            {
                                index++;
                            }
                        }
                        else
                        {
                            flag = false;
                            break;
                        }
                    }
                    _OnHistoryQor?.Invoke(null);
                }
                catch (Exception ex)
                {
                    LogHelper.Error(ex);
                }
                finally
                {
                    //processQueryQorThreadQueue.Dispose();
                }
            });
        }

        private void Instance_OnQor(STC.Common.Model.Table.EmsOrderReport obj)
        {
            if (!isPermission && obj != null && obj.username != LocalEnv.UserInfo.UserName)
            {
                return;
            }

            _OnQor?.Invoke(obj);
        }

        public async void QueryTags(bool isCache = true)
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
                lock (_QuoteTagDic)
                {
                    queryQuoteTagReplyPo = TableOldHandler.Instance.QueryQuoteTag();

                    if (queryQuoteTagReplyPo != null && queryQuoteTagReplyPo.IsSuccess())
                    {
                        if (queryQuoteTagReplyPo.Datas != null && queryQuoteTagReplyPo.Datas.Count > 0)
                        {
                            foreach (QuoteTag qt in queryQuoteTagReplyPo.Datas)
                            {
                                if (!string.IsNullOrEmpty(qt.tagType))
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
                    }
                }
            });
        }


        public Dictionary<string, QuoteTag> GetQuoteTagDic()
        {
            lock (_QuoteTagDic)
            {
                return _QuoteTagDic;
            }
        }

        public bool isMM()
        {
            return AdminHandler.Instance.CheckPermission(MODULEID_INNERTYPE, MM);
        }

        public bool isSALER()
        {
            return AdminHandler.Instance.CheckPermission(MODULEID_INNERTYPE, SALER);
        }

        /// <summary>
        /// 销交确认（人工确认）
        /// </summary>
        /// <param name="data"></param>
        public void ShowDealIntention(STC.Common.Model.Table.WorkBrench.IntentionQuoteInfo obj, bool isDel)
        {
            if (obj == null)
            {
                return;
            }

            Application.Current.Dispatcher.InvokeAsync(() =>
            {
                string key = $"DealIntention-{obj.intentionId}";
                if (isDel)
                {
                    bool res = StrongNotifyHandler.Instance.ManagerInstance.IsShowNotifyContainsKey(key) != "-1";
                    if (res)
                    {
                        var existForm = StrongNotifyHandler.Instance.ManagerInstance.GetAndRefreshArea(key);
                        if (existForm != null)
                        {
                            var bui = existForm as OrderComfirmView;
                            bui.Close();
                        }
                    }
                }
                else
                {
                    bool res = StrongNotifyHandler.Instance.ManagerInstance.IsShowNotifyContainsKey(key) != "-1";
                    if (res)
                    {
                        var existForm = StrongNotifyHandler.Instance.ManagerInstance.GetAndRefreshArea(key);
                        if (existForm != null)
                        {
                            var bui = existForm as OrderComfirmView;
                            OrderComfirmViewModel updateModel = (OrderComfirmViewModel)bui.DataContext;
                            updateModel.UpdateData(obj);
                            bui.ShowNewMsgAlert();
                        }
                    }
                    else
                    {
                        StrongNotifyHandler.Instance.ManagerInstance.AddShowNotify(key, false);
                        OrderComfirmView ui = new OrderComfirmView();
                        ui.MsgId = key;
                        ui.WinTitle = "收到新报价";
                        OrderComfirmViewModel updateModel = (OrderComfirmViewModel)ui.DataContext;
                        updateModel.UpdateData(obj);
                        ui.NotificationClosed += StrongNotifyHandler.Instance.ManagerInstance.OnNotificationClosed;
                        ui.NotificationPopup += StrongNotifyHandler.Instance.ManagerInstance.NotificationNpUR_NotificationPopup;
                        StrongNotifyHandler.Instance.ManagerInstance.ToShowStrongNotify(ui);
                    }
                }


            }, System.Windows.Threading.DispatcherPriority.Send);
        }

        public void RefreshDemandData(string startDate, string endDate)
        {
            ThreadPoolHelper.InvokeThreadPool(o =>
            {
                _CacheIntentionQuotes.Clear();

                RequestIntentionQuotes(startDate, endDate, "DEMAND");
            });
        }

        public void RefreshIntentionData(string startDate, string endDate, string status = null)
        {
            ThreadPoolHelper.InvokeThreadPool(o =>
            {
                _CacheIntentionQuotes.Clear();

                if (LocalEnv.UserInfo.RoleType == RoleType.BUYER_ROLE)
                {
                    RequestIntentionQuotes(startDate, endDate, "BUYER_INTENTION", status);
                }
                else
                {
                    RequestIntentionQuotes(startDate, endDate, "INTENTION");
                }
            });
        }

        private ObservableCollection<FilterGroupItem> _groupList = new ObservableCollection<FilterGroupItem>();
        public ObservableCollection<FilterGroupItem> GroupList
        {
            get { return _groupList; }
            set { _groupList = value; }
        }

        private ObservableCollection<FilterGroupItem> _ppgroupList = new ObservableCollection<FilterGroupItem>();
        public ObservableCollection<FilterGroupItem> PPGroupList
        {
            get { return _ppgroupList; }
            set { _ppgroupList = value; }
        }

        /// <summary>
        /// 应用程序消息
        /// </summary>
        /// <param name="msg"></param>
        private void OnAppMsg(object msg)
        {
            Common.Model.Fix.Message.PResult obj = (Common.Model.Fix.Message.PResult)msg;
            
            if (obj.Code == "pubJS" && !string.IsNullOrEmpty(obj.Info2))
            {
                //收取网页端发送的订阅组更新通知
                if (obj.Info2 == "publishTagGroup")
                {
                    UpdateQuoteTagGroup();
                }
            }
            else if (obj.Code == "UpdateQuoteTagGroup" && string.IsNullOrEmpty(obj.Msg))
            {
                PublishQuoteTagGroup(STC.Common.Data.IntentionOldHandler.Instance.GroupList.ToList());
            }
            else if (obj.Code == "UpdateQuoteTagGroup" && obj.Msg == "*")
            {
                // Msg 为 * 表示需要先更新订阅组列表，然后再通知网页端订阅组更新
                UpdateQuoteTagGroup(true);
            }
        }


        private void InitGroupTag()
        {
            ThreadPoolHelper.InvokeThreadPool(o =>
            {
                Query();
            });
        }


        private void Query()
        {
            QueryQuoteTagGroupRequest req = new QueryQuoteTagGroupRequest();
            req.action = "0";
            req.username = LocalEnv.UserInfo.UserName;
            QueryQuoteTagGroupReplyPo reply = TableOldHandler.Instance.QueryQuoteTagGroup(req);

            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                if (reply != null && reply.IsSuccess())
                {
                    foreach (QuoteTagGroup group in reply.Datas)
                    {
                        if ((group.inf1 == "TAG" || group.inf1 == "DYNAMIC_PARAM" || group.inf1 == "GROUP") && (group.marketindicator == "4"))
                        {
                            FilterGroupItem item = new FilterGroupItem();
                            item.GroupID = group.id;
                            item.GroupName_Edit = item.GroupName = group.groupname;
                            item.Ordersn = group.ordersn;
                            item.UserName = group.username;
                            item.Terminal = group.terminal;
                            item.IP = group.ip;
                            item.CloseBy = group.closeby;
                            item.Remark = group.remark;
                            item.CreateTime = group.createtime;
                            item.UpdateTime = group.updatetime;
                            item.Inf1 = group.inf1;
                            item.Inf2 = group.inf2;
                            item.Inf3 = group.inf3;
                            item.Inf4 = group.inf4;
                            item.FocusTags_Edit = item.FocusTags = group.focusTags;

                            STC.Common.Utils.CommonUtils.ProcessInf4(group.inf4, group.inf1, item);
                            STC.Common.Utils.CommonUtils.ProcessInf3(group.inf3, group.inf1, item);

                            //Dictionary<string, List<string>> dic = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(group.focusTags);
                            //item.FocusTagsDic = dic;
                            item.IfSubscribe_Edit = item.IfSubscribe = group.ifSubscribe == "1" ? true : false;
                            this.GroupList.Add(item);
                        }
                        else if (group.inf1 == "PPGroup")
                        {
                            //平安养老持仓分组
                            FilterGroupItem item = new FilterGroupItem();
                            item.GroupID = group.id;
                            item.GroupName_Edit = item.GroupName = group.groupname;
                            item.Ordersn = group.ordersn;
                            item.UserName = group.username;
                            item.Terminal = group.terminal;
                            item.IP = group.ip;
                            item.CloseBy = group.closeby;
                            item.Remark = group.remark;
                            item.CreateTime = group.createtime;
                            item.UpdateTime = group.updatetime;
                            item.Inf1 = group.inf1;
                            item.Inf2 = group.inf2;
                            item.Inf3 = group.inf3;
                            item.Inf4 = group.inf4;
                            item.FocusTags_Edit = item.FocusTags = group.focusTags;
                            this.PPGroupList.Add(item);
                        }
                    }
                }
            }));
        }

        /// <summary>
        /// 更新现有的TagGroup
        /// </summary>
        private void UpdateQuoteTagGroup(bool isSend = false)
        {
            ThreadPoolHelper.InvokeThreadPool(o =>
            {
                QueryQuoteTagGroupRequest req = new QueryQuoteTagGroupRequest();
                req.action = "0";
                req.username = LocalEnv.UserInfo.UserName;
                QueryQuoteTagGroupReplyPo reply = TableOldHandler.Instance.QueryQuoteTagGroup(req);

                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    if (reply != null && reply.IsSuccess())
                    {
                        foreach (QuoteTagGroup group in reply.Datas)
                        {
                            if ((group.inf1 == "TAG" || group.inf1 == "DYNAMIC_PARAM" || group.inf1 == "GROUP") && (group.marketindicator == "4"))
                            {
                                FilterGroupItem updItem = IntentionOldHandler.Instance.GroupList.Where(a => a.GroupID == group.id)?.LastOrDefault();
                                if (updItem != null)
                                {
                                    //更新
                                    updItem.GroupName_Edit = updItem.GroupName = group.groupname;
                                    updItem.Ordersn = group.ordersn;
                                    updItem.UserName = group.username;
                                    updItem.Terminal = group.terminal;
                                    updItem.IP = group.ip;
                                    updItem.CloseBy = group.closeby;
                                    updItem.Remark = group.remark;
                                    updItem.CreateTime = group.createtime;
                                    updItem.UpdateTime = group.updatetime;
                                    updItem.Inf1 = group.inf1;
                                    updItem.Inf2 = group.inf2;
                                    updItem.Inf3 = group.inf3;
                                    updItem.Inf4 = group.inf4;
                                    updItem.FocusTags_Edit = updItem.FocusTags = group.focusTags;
                                    STC.Common.Utils.CommonUtils.ProcessInf4(group.inf4, group.inf1, updItem);
                                    STC.Common.Utils.CommonUtils.ProcessInf3(group.inf3, group.inf1, updItem);
                                    updItem.IfSubscribe_Edit = updItem.IfSubscribe = group.ifSubscribe == "1" ? true : false;
                                }
                                else
                                {
                                    //插入
                                    FilterGroupItem item = new FilterGroupItem();
                                    item.GroupID = group.id;
                                    item.GroupName_Edit = item.GroupName = group.groupname;
                                    item.Ordersn = group.ordersn;
                                    item.UserName = group.username;
                                    item.Terminal = group.terminal;
                                    item.IP = group.ip;
                                    item.CloseBy = group.closeby;
                                    item.Remark = group.remark;
                                    item.CreateTime = group.createtime;
                                    item.UpdateTime = group.updatetime;
                                    item.Inf1 = group.inf1;
                                    item.Inf2 = group.inf2;
                                    item.Inf3 = group.inf3;
                                    item.Inf4 = group.inf4;
                                    item.FocusTags_Edit = item.FocusTags = group.focusTags;

                                    STC.Common.Utils.CommonUtils.ProcessInf4(group.inf4, group.inf1, item);
                                    STC.Common.Utils.CommonUtils.ProcessInf3(group.inf3, group.inf1, item);

                                    item.IfSubscribe_Edit = item.IfSubscribe = group.ifSubscribe == "1" ? true : false;
                                    this.GroupList.Add(item);
                                }
                            }
                            else if (group.inf1 == "PPGroup")
                            {
                                //平安养老持仓
                                FilterGroupItem updItem = IntentionOldHandler.Instance.PPGroupList.Where(a => a.GroupID == group.id)?.LastOrDefault();
                                if (updItem != null)
                                {
                                    //更新
                                    updItem.GroupName_Edit = updItem.GroupName = group.groupname;
                                    updItem.Ordersn = group.ordersn;
                                    updItem.UserName = group.username;
                                    updItem.Terminal = group.terminal;
                                    updItem.IP = group.ip;
                                    updItem.CloseBy = group.closeby;
                                    updItem.Remark = group.remark;
                                    updItem.CreateTime = group.createtime;
                                    updItem.UpdateTime = group.updatetime;
                                    updItem.Inf1 = group.inf1;
                                    updItem.Inf2 = group.inf2;
                                    updItem.Inf3 = group.inf3;
                                    updItem.Inf4 = group.inf4;
                                    updItem.FocusTags_Edit = updItem.FocusTags = group.focusTags;
                                    STC.Common.Utils.CommonUtils.ProcessInf4(group.inf4, group.inf1, updItem);
                                    STC.Common.Utils.CommonUtils.ProcessInf3(group.inf3, group.inf1, updItem);
                                    updItem.IfSubscribe_Edit = updItem.IfSubscribe = group.ifSubscribe == "1" ? true : false;
                                }
                                else
                                {
                                    //插入
                                    FilterGroupItem item = new FilterGroupItem();
                                    item.GroupID = group.id;
                                    item.GroupName_Edit = item.GroupName = group.groupname;
                                    item.Ordersn = group.ordersn;
                                    item.UserName = group.username;
                                    item.Terminal = group.terminal;
                                    item.IP = group.ip;
                                    item.CloseBy = group.closeby;
                                    item.Remark = group.remark;
                                    item.CreateTime = group.createtime;
                                    item.UpdateTime = group.updatetime;
                                    item.Inf1 = group.inf1;
                                    item.Inf2 = group.inf2;
                                    item.Inf3 = group.inf3;
                                    item.Inf4 = group.inf4;
                                    item.FocusTags_Edit = item.FocusTags = group.focusTags;

                                    STC.Common.Utils.CommonUtils.ProcessInf4(group.inf4, group.inf1, item);
                                    STC.Common.Utils.CommonUtils.ProcessInf3(group.inf3, group.inf1, item);

                                    item.IfSubscribe_Edit = item.IfSubscribe = group.ifSubscribe == "1" ? true : false;
                                    this.PPGroupList.Add(item);
                                }
                            }
                        }

                        List<string> delList = GroupList.Select(item => item.GroupID).ToList();
                        List<string> inlist = reply.Datas.Select(item => item.id).ToList();
                        List<string> exceptlist = delList.Except(inlist).ToList();
                        if (exceptlist != null && exceptlist.Count > 0)
                        {
                            //删除
                            foreach (string id in exceptlist)
                            {
                                FilterGroupItem delItem = IntentionOldHandler.Instance.GroupList.Where(a => a.GroupID == id).LastOrDefault();
                                if (delItem != null)
                                {
                                    this.GroupList.Remove(delItem);
                                }
                            }
                        }

                        //平安持仓养老
                        List<string> ppdelList = PPGroupList.Select(item => item.GroupID).ToList();
                        List<string> ppinlist = reply.Datas.Select(item => item.id).ToList();
                        List<string> ppexceptlist = ppdelList.Except(ppinlist).ToList();
                        if (ppexceptlist != null && ppexceptlist.Count > 0)
                        {
                            //删除
                            foreach (string id in ppexceptlist)
                            {
                                FilterGroupItem delItem = IntentionOldHandler.Instance.PPGroupList.Where(a => a.GroupID == id).LastOrDefault();
                                if (delItem != null)
                                {
                                    this.PPGroupList.Remove(delItem);
                                }
                            }

                            //平安持仓不通知网页端
                            isSend = false;
                        }


                        //STC内部订阅组更新通知网页端
                        if (isSend)
                        {
                            PublishQuoteTagGroup(STC.Common.Data.IntentionOldHandler.Instance.GroupList.ToList());
                        }
                    }
                }));
            });
        }

        /// <summary>
        /// 发布订阅组列表给网页端
        /// </summary>
        /// <param name="groupList"></param>
        private void PublishQuoteTagGroup(List<FilterGroupItem> groupList)
        {
            // Msg 为 空 表示可直接通知网页端订阅组更新
            //List<FilterGroupItem> sd = STC.Common.Data.IntentionOldHandler.Instance.GroupList.Select(item => new FilterGroupItem { Aciton = item.Aciton, GroupID = item.GroupID, GroupName = item.GroupName, Ordersn = item.Ordersn, IsGroup = item.IsGroup, IfSubscribe = item.IfSubscribe }).ToList();
            Task.Factory.StartNew(() =>
            {
                foreach (FilterGroupItem item in groupList)
                {
                    item.FocusTags = item.FocusTags.Replace("\"", "");
                    item.FocusTags_Edit = item.FocusTags_Edit.Replace("\"", "");
                    item.FocusGroupId = item.FocusGroupId.Replace("\"", "");
                    item.FocusGroupId_Edit = item.FocusGroupId_Edit.Replace("\"", "");
                }

                string reply = JsonConvert.SerializeObject(groupList);
                Common.Handler.Base.BaseHandler.Instance.PublishAppMsg(new PResult() { Code = "pubJS", Info1 = "*", Info2 = "subscribeTagGroup", Info3 = reply });
            });
        }

        public void RequestMarketQuotes(string startDate, string endDate)
        {
            try
            {
                IntentionInquiryRequest request = new IntentionInquiryRequest();
                request.action = "0";
                request.biztypeid = LocalEnv.UserInfo.Termids;
                request.userName = LocalEnv.UserInfo.UserName;

                if (!string.IsNullOrEmpty(startDate) || !string.IsNullOrEmpty(endDate))
                {
                    request.updateTimeStart = startDate;
                    request.updateTimeEnd = endDate;
                }

                if (!string.IsNullOrEmpty(LocalEnv.UserInfo.Termids))
                {
                    request.biztypeid = LocalEnv.UserInfo.Termids;
                }

                if (LocalEnv.UserInfo.RoleType == RoleType.BUYER_ROLE)
                {
                    request.quoteType = "BUYER_INTENTION";
                }
                else
                {
                    request.quoteType = "INTENTION";
                }


                ///获取订单 
                var reply = TableOldHandler.Instance.IntentionInquiry(request);

                if (reply != null && reply.Datas != null)
                {
                    foreach (TableIntentionInquiry obj in reply.Datas)
                    {
                        if (!_CacheMarketQuotes.ContainsKey(obj.intentionId))
                        {
                            _CacheMarketQuotes[obj.intentionId] = new ConcurrentDictionary<string, TableIntentionInquiry>();
                        }
                        _CacheMarketQuotes[obj.intentionId][obj.id] = obj;
                    }

                    _OnHistoryMarketQuotes?.Invoke(reply.Datas);
                }
            }
            catch (Exception e)
            {
                LogHelper.Error("RequestMarketQuotes", e);
            }
        }

        public void RequestMarketQuotes_IntentionId(string intentionId, string startDate, string endDate)
        {
            try
            {
                IntentionInquiryRequest req = new IntentionInquiryRequest();
                //req.userName = LocalEnv.UserInfo.UserName;
                req.biztypeid = LocalEnv.UserInfo.Termids;
                req.action = "0";
                req.intentionId = intentionId;
                if (!string.IsNullOrEmpty(startDate) || !string.IsNullOrEmpty(endDate))
                {
                    req.updateTimeStart = startDate;
                    req.updateTimeEnd = endDate;
                }
                if (LocalEnv.UserInfo.RoleType == RoleType.BUYER_ROLE)
                {
                    req.quoteType = "BUYER_INTENTION";
                }
                else
                {
                    req.quoteType = "INTENTION";
                }

                IntentionInquiryPo reply = TableOldHandler.Instance.IntentionInquiry(req);

                if (reply != null && reply.Datas != null)
                {
                    foreach (TableIntentionInquiry obj in reply.Datas)
                    {
                        if (!_CacheMarketQuotes.ContainsKey(obj.intentionId))
                        {
                            _CacheMarketQuotes[obj.intentionId] = new ConcurrentDictionary<string, TableIntentionInquiry>();
                        }
                        _CacheMarketQuotes[obj.intentionId][obj.id] = obj;
                    }

                    _OnHistoryMarketQuotes?.Invoke(reply.Datas);
                }
            }
            catch (Exception e)
            {
                LogHelper.Error("RequestMarketQuotes_IntentionId", e);
            }
        }

        public void Instance_OnInquiry(TableIntentionInquiry obj)
        {
            if (obj.status == "1" || obj.status == "4")
            {
                if (!_CacheMarketQuotes.ContainsKey(obj.intentionId))
                {
                    _CacheMarketQuotes[obj.intentionId] = new ConcurrentDictionary<string, TableIntentionInquiry>();
                }
                _CacheMarketQuotes[obj.intentionId][obj.id] = obj;
            }
            else
            {
                if (_CacheMarketQuotes.ContainsKey(obj.intentionId))
                {
                    TableIntentionInquiry removeObj;
                    _CacheMarketQuotes[obj.intentionId].TryRemove(obj.id, out removeObj);
                }
            }
            _OnMarketQuotes?.Invoke(obj);
        }

        public TableIntentionInquiry GetInquiryById(string id)
        {
            foreach (string intentionid in _CacheMarketQuotes.Keys)
            {
                foreach (TableIntentionInquiry obj in _CacheMarketQuotes[intentionid].Values)
                {
                    if (obj.id == id)
                    {
                        return obj;
                    }
                }
            }
            return null;
        }

        public TableIntentionInquiry GetInquiryById(string intentionid, string id)
        {
            if (!string.IsNullOrEmpty(intentionid) && _CacheMarketQuotes.ContainsKey(intentionid))
            {
                foreach (TableIntentionInquiry obj in _CacheMarketQuotes[intentionid].Values)
                {
                    if (obj.id == id)
                    {
                        return obj;
                    }
                }
            }
            return null;
        }

        public TableIntentionInquiry GetSameToInquiry(string intentionid, ToOption to)
        {
            if (!string.IsNullOrEmpty(intentionid) && _CacheMarketQuotes.ContainsKey(intentionid) && to != null)
            {
                foreach (TableIntentionInquiry obj in _CacheMarketQuotes[intentionid].Values)
                {
                    if (obj.status == "1" && obj.sendType != 3)
                    {
                        if (JsonSplit.IsJson(obj.info5))
                        {
                            ToOption toOption = JsonConvert.DeserializeObject<ToOption>(obj.info5);
                            
                            if (toOption.OptionType == to.OptionType)
                            {
                                if (toOption.OptionType == "sendIntentBrokers")
                                {
                                    if (toOption.Broker.qq == to.Broker.qq)
                                    {
                                        return obj;
                                    }
                                }
                                else if (toOption.OptionType == "sendIntentContacts")
                                {
                                    if (toOption.Sender.qq == to.Sender.qq)
                                    {
                                        return obj;
                                    }
                                }
                                else if (toOption.OptionType == "sendIntentCounterparties")
                                {
                                    if (toOption.Sender.qq == to.Sender.qq)
                                    {
                                        return obj;
                                    }
                                }
                                else if (toOption.OptionType == "sendIntentQQGroups")
                                {
                                    if (toOption.QQGroups.groupOpenID == to.QQGroups.groupOpenID)
                                    {
                                        return obj;
                                    }
                                }
                                else if (toOption.OptionType == "sendIntentGroups")
                                {
                                    if (toOption.Groups.groupName == to.Groups.groupName)
                                    {
                                        return obj;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return null;
        }

        public List<TableIntentionInquiry> GetSameToInquiry(string intentionid, string receiver, string id)
        {
            List<TableIntentionInquiry> resultList = null;
            if (!string.IsNullOrEmpty(intentionid) && _CacheMarketQuotes.ContainsKey(intentionid))
            {
                foreach (TableIntentionInquiry obj in _CacheMarketQuotes[intentionid].Values)
                {
                    if (obj.status == "1")
                    {
                        
                        if (JsonSplit.IsJson(obj.info5))
                        {
                            ToOption toOption = JsonConvert.DeserializeObject<ToOption>(obj.info5);

                            if (toOption != null)
                            {
                                if (obj.id == id && receiver != toOption.DispayName)
                                {
                                    if (resultList == null)
                                    {
                                        resultList = new List<TableIntentionInquiry>();
                                    }
                                    if (!resultList.Any(x => x.id == obj.id))
                                    {
                                        resultList.Add(obj);
                                    }
                                }
                                else
                                {
                                    int oriIndex = toOption.DispayName.IndexOf("-");
                                    string preName_Ori = toOption.DispayName;
                                    if (oriIndex > -1)
                                    {
                                        preName_Ori = toOption.DispayName.Substring(0, oriIndex);
                                    }

                                    int editIndex = receiver.IndexOf("-");
                                    string preName_Edit = receiver;
                                    if (editIndex > -1)
                                    {
                                        preName_Edit = receiver.Substring(0, editIndex);
                                    }

                                    if (receiver == toOption.DispayName)
                                    {
                                        if (resultList == null)
                                        {
                                            resultList = new List<TableIntentionInquiry>();
                                        }
                                        if (!resultList.Any(x => x.id == obj.id))
                                        {
                                            resultList.Add(obj);
                                        }
                                    }
                                    else if (preName_Edit == preName_Ori && preName_Edit != "常用联系人" && preName_Edit != "对手方"
                                        && preName_Edit != "自定义分组" && preName_Edit != "QQ群组")
                                    {
                                        if (resultList == null)
                                        {
                                            resultList = new List<TableIntentionInquiry>();
                                        }
                                        if (!resultList.Any(x => x.id == obj.id))
                                        {
                                            resultList.Add(obj);
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (receiver == obj.receiverName)
                            {
                                if (resultList == null)
                                {
                                    resultList = new List<TableIntentionInquiry>();
                                }
                                if (!resultList.Any(x => x.id == obj.id))
                                {
                                    resultList.Add(obj);
                                }
                            }
                        }
                    }
                }
            }
            return resultList;
        }
    
        public List<TableIntentionInquiry> GetInquiries(string intentionid, string id = null)
        {
            List<TableIntentionInquiry> resultList = null;

            if (!string.IsNullOrEmpty(intentionid) && _CacheMarketQuotes.ContainsKey(intentionid))
            {
                if (!string.IsNullOrEmpty(id) && _CacheMarketQuotes[intentionid].ContainsKey(id))
                {
                    resultList = new List<TableIntentionInquiry>();
                    resultList.Add(_CacheMarketQuotes[intentionid][id]);
                }
                else
                {
                    resultList = _CacheMarketQuotes[intentionid].Values.ToList();
                }
            }

            return resultList;
        }

        public void ShowTraderRefConfirm(STC.Common.Model.Table.WorkBrench.IntentionQuoteInfo obj, bool isDel)
        {
            if (obj == null)
            {
                return;
            }
           
            Application.Current.Dispatcher.InvokeAsync(() =>
            {
                string key = $"TraderRefConfirm-{obj.intentionId}";
                if (isDel)
                {
                    bool res = StrongNotifyHandler.Instance.ManagerInstance.IsShowNotifyContainsKey(key) != "-1";
                    if (res)
                    {
                        var existForm = StrongNotifyHandler.Instance.ManagerInstance.GetAndRefreshArea(key);
                        if (existForm != null)
                        {
                            var bui = existForm as ConfirmRef_BuyTrader;
                            bui.Close();
                        }
                    }
                }
                else
                {
                    bool res = StrongNotifyHandler.Instance.ManagerInstance.IsShowNotifyContainsKey(key) != "-1";
                    if (res)
                    {
                        var existForm = StrongNotifyHandler.Instance.ManagerInstance.GetAndRefreshArea(key);
                        if (existForm != null)
                        {
                            var bui = existForm as ConfirmRef_BuyTrader;
                            ConfirmRef_BuyTrader_ViewModel updateModel = (ConfirmRef_BuyTrader_ViewModel)bui.DataContext;
                            updateModel.UpdateData(obj);
                            bui.ShowNewMsgAlert();
                        }
                    }
                    else
                    {
                        StrongNotifyHandler.Instance.ManagerInstance.AddShowNotify(key, false);
                        ConfirmRef_BuyTrader ui = new ConfirmRef_BuyTrader();
                        ui.MsgId = key;
                        ui.WinTitle = "申请撤销";
                        ConfirmRef_BuyTrader_ViewModel updateModel = (ConfirmRef_BuyTrader_ViewModel)ui.DataContext;
                        updateModel.UpdateData(obj);
                        ui.NotificationClosed += StrongNotifyHandler.Instance.ManagerInstance.OnNotificationClosed;
                        ui.NotificationPopup += StrongNotifyHandler.Instance.ManagerInstance.NotificationNpUR_NotificationPopup;
                        StrongNotifyHandler.Instance.ManagerInstance.ToShowStrongNotify(ui);
                    }
                }

                
            }, System.Windows.Threading.DispatcherPriority.Send);
        }

        public Common.Model.Table.WorkBrench.InsertIntentionQuoteReplyPo InsertIntentionQuote_Common_New(STC.Common.Model.Table.WorkBrench.InsertIntentionQuoteReq request)
        {
            if (LocalEnv.UserInfo.RoleType == RoleType.BUYER_ROLE)
            {
                if (request != null && request.quoteList != null && request.quoteList.Count > 0)
                {
                    foreach (STC.Common.Model.Table.WorkBrench.IntentionQuoteInfo record in request.quoteList)
                    {
                        TagData fqInfo = new TagData();
                        fqInfo.StrategyOrderId = $"{request.quoteType}_{record.partyTrader}_{record.securityId}_{record.side}";
                        record.info2 = JsonConvert.SerializeObject(fqInfo);
                    }
                }
            }
            Common.Model.Table.WorkBrench.InsertIntentionQuoteReplyPo result = TableOldHandler.Instance.InsertIntentionQuoteNew(request);
            return result;
        }
    }
}
