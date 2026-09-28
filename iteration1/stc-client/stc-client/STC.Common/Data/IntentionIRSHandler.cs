using com.gateway.log;
using STC.Common.Api;
using STC.Common.Handler.Admin;
using STC.Common.Handler.Base;
using STC.Common.Handler.Table;
using STC.Common.Model.Base;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.Table;
using STC.Common.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace STC.Common.Data
{
    public class IntentionIRSHandler
    {
        private bool isPermission = false; //查看机构内部权限

        //执行权限
        private readonly string MODULEID_INNERTYPE = "6001";
        private readonly string MM = "600101";//做市
        private readonly string SALER = "600102";//销交

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


        // 订阅我的订单
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
                System.Threading.ThreadPool.QueueUserWorkItem(o =>
                {
                    foreach (var obj in _CacheIntentionQuotes)
                    {
                        _OnIntentionQuotes?.Invoke(obj.Value);
                    }
                });
            }
            remove
            {
                lock (m_OnIntentionQuoteEventLock)
                {
                    _OnIntentionQuotes -= value;
                }
            }
        }

        /// <summary>
        /// 缓存我的意向单(订阅)
        /// </summary>
        private ConcurrentDictionary<string, STC.Common.Model.Table.WorkBrench.IntentionQuoteInfo> _CacheIntentionQuotes = new ConcurrentDictionary<string, STC.Common.Model.Table.WorkBrench.IntentionQuoteInfo>();


        // 订阅意向单识别
        private readonly Object m_OnQorEventLock = new Object();
        private Action<STC.Common.Model.Table.EmsOrderReport, bool> _OnQor;
        public event Action<STC.Common.Model.Table.EmsOrderReport, bool> OnQor
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
        private static IntentionIRSHandler server = new IntentionIRSHandler();
        public static IntentionIRSHandler Instance { get { return server; } }

        /// <summary>
        /// 构造函数
        /// </summary>
        private IntentionIRSHandler()
        {
            ThreadPoolHelper.InvokeThreadPool(o =>
            {
                BaseHandler.Instance.Init(Instance_OnClear);

                isPermission = AdminHandler.Instance.CheckPermisionById(CommonConstants.MODELID_5000, CommonConstants.CODE_5002);
            });

        }

        private volatile bool isInit = false;

        /// <summary>
        /// 处理内部看板实时消息的线程队列
        /// </summary>
        private AsyncThreadQueue<EmsOrderReport> _ProcessQorThreadQueue;

        private volatile bool isInit_InnerBoard = false;
        public void initInnerBoard()
        {
            ThreadPoolHelper.InvokeThreadPool(o => {
                if (isInit_InnerBoard == false)
                {
                    if (_ProcessQorThreadQueue == null)
                    {
                        _ProcessQorThreadQueue = new AsyncThreadQueue<EmsOrderReport>(Instance_OnQor_RealTime);
                    }
                    isInit_InnerBoard = true;


                    TableOldHandler.Instance.SubscribeQor_Irs(Instance_OnQor);
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

                isInit = false;
                isInit_InnerBoard = false;


                if (_ProcessQorThreadQueue != null)
                {
                    _ProcessQorThreadQueue.Clear();
                }

                initInnerBoard();
            }
        }

        public void RequestQor()
        {
            ThreadPoolHelper.InvokeThreadPool((o) =>
            {
                QueryEmsOrderReq request = new QueryEmsOrderReq();
                request.dealTimeBegin = $"{DateTimeEx.ServerTime.ToString("yyyy-MM-dd")} 00:00:00.000";
                if (!isPermission)
                    request.userName = LocalEnv.UserInfo.UserName;
                int index = 1;
                request.pageSize = 2000;

                bool flag = true;
                while (flag)
                {
                    request.pageNum = index;
                    ///获取订单 
                    QueryEmsOrderReplyPo reply = TableOldHandler.Instance.QueryEmsOrder_Irs(request);

                    if (reply == null)
                    {
                        flag = false;
                        break;
                    }

                    if (reply != null && reply.Datas != null)
                    {
                        foreach (EmsOrderReport er in reply.Datas)
                        {
                            try
                            {
                                InvokeQor(er, false);
                            }
                            catch (Exception e)
                            {
                                LogHelper.Error(e);
                            }
                        }
                        if (reply.Datas.Count < request.pageSize)
                        {
                            flag = false;
                        }
                        else
                        {
                            index++;
                        }
                    }
                }
                InvokeQor(null, false);
            });
        }

        private void Instance_OnQor(STC.Common.Model.Table.EmsOrderReport obj)
        {
            if (_ProcessQorThreadQueue != null)
            {
                _ProcessQorThreadQueue.Enqueue(obj);
            }
        }

        private void Instance_OnQor_RealTime(STC.Common.Model.Table.EmsOrderReport obj)
        {
            InvokeQor(obj, true);
        }

        public void InvokeQor(STC.Common.Model.Table.EmsOrderReport obj, bool isNew)
        {
            if (!isPermission && obj != null && obj.username != LocalEnv.UserInfo.UserName)
            {
                return;
            }

            _OnQor?.Invoke(obj, isNew);
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
                                _QuoteTagDic[qt.tagType] = qt;

                                if (qt.tagType == "IRS_TYPE"
                                    || qt.tagType == "IRS_PRODUCT_TYPE"
                                    || qt.tagType == "IRS_TERM"
                                    || qt.tagType == "ORG_TYPE")
                                {
                                    if (!_tagsDic.ContainsKey(qt.tagType))
                                    {
                                        _tagsDic[qt.tagType] = new Dictionary<string, string>();
                                    }
                                    foreach (ChildTag ct in qt.childTags)
                                    {
                                        if (ct.childTagName == "无")
                                            continue;

                                        if (qt.tagType == "IRS_PRODUCT_TYPE")
                                        {
                                            foreach (ChildTag sub_ct in ct.childTags)
                                            {
                                                if (sub_ct.childTagName == "无")
                                                    continue;

                                                _tagsDic[qt.tagType][sub_ct.childTagValue] = sub_ct.childTagName;
                                            }
                                        }
                                        else
                                        {
                                            _tagsDic[qt.tagType][ct.childTagValue] = ct.childTagName;
                                        }

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

    }
}
