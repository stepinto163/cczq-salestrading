using com.gateway.log;
using netapi.Api;
using Newtonsoft.Json;
using STC.Common.Api;
using STC.Common.Dispatcher;
using STC.Common.Handler.Cal;
using STC.Common.Model.Cal;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.Position;
using STC.Common.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace STC.Common.Handler.Position
{

    public class PositionHandler : IPositionHandler
    {
        private static IPositionHandler instance = new PositionHandler();

        private bool isSubscribeForPositionDetail = false;
        private bool isSubscribeForTradeStatistics = false;
        public static IPositionHandler Instance { get { return instance; } }

        public Dictionary<string, List<string>> DictCallBack = new Dictionary<string, List<string>>();

        /// <summary>
        /// 所有的组和组ID字典表
        /// </summary>
        public QueryPositionBookResult CachePostionBook;
        private PositionHandler()
        {

        }

        /// <summary>
        /// 订阅持仓明细
        /// </summary>
        /// <param name="onMsgCallBack"></param>
        public void SubscribePositionDetail(List<string> lstBookId, Action<List<PositionDetailModel>> callBack)
        {
            if (callBack != null)
            {
                //STC.Position.Plugin.ViewModel.MainViewModel.OnPostionDetail
                string callbackkey = $"{callBack.Method.DeclaringType.FullName.ToString()}.{callBack.Method.Name.ToString()}";

                if (!DictCallBack.ContainsKey(callbackkey))
                {
                    OnPosition += callBack;
                    DictCallBack.Add(callbackkey, lstBookId);

                    if (isSubscribeForPositionDetail)
                    {
                        callBack?.Invoke(CachePositionDetailModel.ToList());
                        return;
                    }
                }
            }

            lock (this)
            {
                if (!isSubscribeForPositionDetail)
                {
                    string[] topics = new string[1];
                    topics[0] = $"{ApiConsts.Topic_stc_PostionDetail}.**";
                    ApiManager.Instance.GetDefaultApi().SubScribeWithImage(DataBus.Instance.Instanse_OnMessage, topics, ApiConsts.PositionSvr, string.Empty);

                    isSubscribeForPositionDetail = true;
                }
            }
        }

        /// <summary>
        /// 取消订阅
        /// </summary>
        /// <param name="onMsgCallBack"></param>
        public void UnSubscribePositionDetail(Action<List<PositionDetailModel>> callBack)
        {
            if (callBack != null && OnPosition != null)
            {
                string key = $"{callBack.Method.DeclaringType.FullName.ToString()}.{callBack.Method.Name.ToString()}";
                if (DictCallBack.ContainsKey(key))
                {
                    DictCallBack.Remove(key);
                    OnPosition -= callBack;
                }
            }

            if (OnPosition == null)
            {
                ApiManager.Instance.GetDefaultApi().UnSubScribe(ApiConsts.PositionSvr, new string[] { $"{ApiConsts.Topic_stc_PostionDetail}.**" });
                isSubscribeForPositionDetail = false;
                CachePositionDetailModel.Clear();
            }
        }

        private event Action<List<PositionDetailModel>> OnPosition;

        /// <summary>
        /// 缓存数据
        /// </summary>
        private ObservableCollection<PositionDetailModel> CachePositionDetailModel = new ObservableCollection<PositionDetailModel>();

        public void SubscribeTradeStatistics(List<string> lstBookId, Action<List<TradeStatisticsModel>> callBack)
        {
            if (callBack != null)
            {
                string callbackkey = $"{callBack.Method.DeclaringType.FullName.ToString()}.{callBack.Method.Name.ToString()}";

                if (!DictCallBack.ContainsKey(callbackkey))
                {
                    OnStatistics += callBack;
                    DictCallBack.Add(callbackkey, lstBookId);

                    if (isSubscribeForTradeStatistics)
                    {
                        callBack?.Invoke(CacheTradeStatisticModel.ToList());
                        return;
                    }
                }
            }

            lock (this)
            {
                if (!isSubscribeForTradeStatistics)
                {
                    string[] topics = new string[1];
                    topics[0] = $"{ApiConsts.Topic_stc_TradeStatistics}.**";
                    ApiManager.Instance.GetDefaultApi().SubScribeWithImage(DataBus.Instance.Instanse_OnMessage, topics, ApiConsts.PositionSvr, string.Empty);

                    isSubscribeForTradeStatistics = true;
                }
            }
        }

        public void UnSubscribeTradeStatistics(Action<List<TradeStatisticsModel>> callBack)
        {
            if (callBack != null && OnStatistics != null)
            {
                string key = $"{callBack.Method.DeclaringType.FullName.ToString()}.{callBack.Method.Name.ToString()}";
                if (DictCallBack.ContainsKey(key))
                {
                    DictCallBack.Remove(key);
                    OnStatistics -= callBack;
                }
            }

            if (OnStatistics == null)
            {
                ApiManager.Instance.GetDefaultApi().UnSubScribe(ApiConsts.PositionSvr, new string[] { $"{ApiConsts.Topic_stc_TradeStatistics}.**" });
                isSubscribeForTradeStatistics = false;
                CacheTradeStatisticModel.Clear();
            }
        }

        private event Action<List<TradeStatisticsModel>> OnStatistics;

        /// <summary>
        /// 缓存成交统计数据
        /// </summary>
        private ObservableCollection<TradeStatisticsModel> CacheTradeStatisticModel = new ObservableCollection<TradeStatisticsModel>();

        /// <summary>
        /// 当日持仓明细回调
        /// </summary>
        /// <param name="obj"></param>
        private void OnTradeStatistics(object obj)
        {
            DataBus.Instance.add(obj);
        }

        public void ProcessTradeStatistics(NetMessage netMsg)
        {
            if (netMsg != null)
            {
                if (netMsg.Topic.Contains(ApiConsts.Topic_stc_TradeStatistics))
                {
                    if (netMsg.Data.StartsWith(ApiConsts.BRACKET))
                    {
                        List<TradeStatisticsModel> mmSysStatus = BaseHelper.ConvertStcMessage<List<TradeStatisticsModel>>(new string[] { netMsg.Topic }, netMsg);
                        CacheTradeStatisticModel = new ObservableCollection<TradeStatisticsModel>(mmSysStatus);
                        OnStatistics?.Invoke(mmSysStatus);
                    }
                    else
                    {
                        TradeStatisticsModel s = BaseHelper.ConvertStcMessage<TradeStatisticsModel>(new string[] { netMsg.Topic }, netMsg);

                        TradeStatisticsModel pm = CacheTradeStatisticModel.FirstOrDefault(o => o.bookId == s.bookId && o.securityId == s.securityId);
                        if (pm != null)
                        {
                            pm = s;
                        }

                        OnStatistics?.Invoke(new List<TradeStatisticsModel>() { s });
                    }
                }
            }
        }

        Regex reg = new Regex(ApiConsts.Topic_stc_PostionDetail_regex);

        /// <summary>
        /// 当日持仓明细回调
        /// </summary>
        /// <param name="obj"></param>
        private void OnPositionDetail(object obj)
        {
            DataBus.Instance.add(obj);
        }

        public void ProcessPositionDetail(NetMessage netMsg)
        {
            if (netMsg != null)
            {
                if (netMsg.Topic.Contains(ApiConsts.Topic_stc_PostionDetail))
                {
                    if (netMsg.Data.StartsWith(ApiConsts.BRACKET))
                    {
                        List<PositionDetailModel> mmSysStatus = BaseHelper.ConvertStcMessage<List<PositionDetailModel>>(new string[] { netMsg.Topic }, netMsg);
                        CachePositionDetailModel = new ObservableCollection<PositionDetailModel>(mmSysStatus);
                        OnPosition?.Invoke(mmSysStatus);
                    }
                    else
                    {
                        PositionDetailModel s = BaseHelper.ConvertStcMessage<PositionDetailModel>(new string[] { netMsg.Topic }, netMsg);

                        PositionDetailModel pm = CachePositionDetailModel.FirstOrDefault(o => o.bookid == s.bookid && o.securityid == s.securityid);
                        if (pm != null)
                        {
                            pm = s;
                        }

                        OnPosition?.Invoke(new List<PositionDetailModel>() { s });
                    }
                }
            }
        }


        //private void Instance_OnPostion(object risk)
        //{
        //    List<PositionDetailModel> data = BaseHelper.ConvertStcMessage<List<PositionDetailModel>>(new string[] { $"{ApiConsts.Topic_stc_PostionDetail}.**" }, risk);
        //    if (data != null)
        //        Instance_OnPostion(data);
        //}


        /// <summary>
        /// 查询历史持仓
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public QureyHistoryDetailResult QueryHistoryPositionDetail(QueryPositionMvtReq req)
        {
            return ApiManager.Instance.GetDefaultApi().Request<QureyHistoryDetailResult>(ApiConsts.PositionSvr, ApiConsts.Topic_stc_HistoryPositionGroup, req);
        }

        /// <summary>
        /// 查询有权限查看的组合
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public QueryPositionBookResult QueryPositionBook(QueryPositionBookReq req)
        {
            return ApiManager.Instance.GetDefaultApi().Request<QueryPositionBookResult>(ApiConsts.AdminSvr, ApiConsts.Topic_stc_PositionBook, req);
        }

        /// <summary>
        /// 查询所有的组合和组合ID
        /// </summary>
        /// <returns></returns>
        public QueryPositionBookResult QueryBookUser()
        {
            if (CachePostionBook == null)
            {
                CachePostionBook = ApiManager.Instance.GetDefaultApi().Request<QueryPositionBookResult>(ApiConsts.AdminSvr, ApiConsts.Topic_stc_QueryBook, "{}");
            }

            return CachePostionBook;
        }

        /// <summary>
        /// 手工录入订单
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public PResult ManualInputOrder(ExecutionReportReq req)
        {
            return ApiManager.Instance.GetDefaultApi().Request<PResult>(ApiConsts.PositionSvr, ApiConsts.Topic_stc_ManualOrder, req);
        }

        /// <summary>
        /// 历史成交查询
        /// </summary>
        /// <param name="secid"></param>
        /// <returns></returns>
        public QueryExternalResult QueryExternal(QueryExternalReq req)
        {
            return ApiManager.Instance.GetDefaultApi().Request<QueryExternalResult>(ApiConsts.PositionSvr, ApiConsts.Topic_stc_QueryExternal, req);
        }

        /// <summary>
        /// 修改手工订单请求/修改或增加组合接口请求
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public PResult UpdatePosition(UpdatePositionReq req)
        {
            return ApiManager.Instance.GetDefaultApi().Request<PResult>(ApiConsts.PositionSvr, ApiConsts.Topic_stc_UpdatePosition, req);
        }

        /// <summary>
        /// 查询合约组合持仓
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public QueryPositionResult QueryPositionDetail(QueryPositionReq req)
        {
            return ApiManager.Instance.GetDefaultApi().Request<QueryPositionResult>(ApiConsts.PositionSvr, ApiConsts.Topic_stc_QueryPosition, req);
        }

        #region 计算净价
        /// <summary>
        /// 计算净价和成交价
        /// </summary>
        /// <param name="securityID"></param>
        /// <param name="calType">4 计算净价 2 计算成交价</param>
        /// <param name="settleType"></param>
        /// <param name="price"></param>
        /// <returns></returns>
        public string GetNetPrice(string securityID, string calType, string settleType, string price)
        {
            string netPrice = string.Empty;

            CalReq requst = new CalReq();
            requst.SecurityID = securityID;
            requst.CalType = calType;//"4";
            requst.SettlType = settleType;// "1";
            requst.Value = price;// customPrice;
            if (!string.IsNullOrEmpty(LocalEnv.ConfInfo.CalType))
                requst.CalPath = LocalEnv.ConfInfo.CalType;

            CalResult result = CalHandler.Instance.CalBond(requst);
            if (result != null && result.Data != null && result.IsSuccess())
            {
                if (calType.Equals("4"))  //计算净价
                {
                    netPrice = result.Data.CleanPrice.ToString("F4");
                    return netPrice;
                }
                else if (calType.Equals("2")) //根据净价计算价格
                {
                    netPrice = (result.Data.Yield * 100).ToString("F4");
                    return netPrice;
                }

            }

            return string.Empty;
        }

        /// <summary>
        /// 计算收益率或者净价
        /// </summary>
        /// <param name="calType"></param>
        /// <param name="settleType"></param>
        /// <param name="val"></param>
        public string CalBond(string securityId, string calType, string settleType, string val, bool isExeBond)
        {
            string resultYield = "";
            decimal d = 0;
            if (string.IsNullOrEmpty(val) || string.IsNullOrEmpty(settleType) || !decimal.TryParse(val, out d))
                return resultYield;
            try
            {
                CalReq requst = new CalReq();
                requst.SecurityID = securityId;
                requst.CalType = calType;//"4";
                requst.SettlType = settleType;// "1";
                requst.Scale = 6;
                requst.Value = val;
                if (!string.IsNullOrEmpty(LocalEnv.ConfInfo.CalType))
                    requst.CalPath = LocalEnv.ConfInfo.CalType;

                CalResult result = CalHandler.Instance.CalBond(requst);
                if (result != null && result.Data != null && result.IsSuccess())
                {
                    if (calType.Equals("4"))//收益率算净价
                    {
                        resultYield = result.Data.CleanPrice.ToString("F4");
                    }
                    else if (calType.Equals("7"))//根据行权收益率算净价
                    {
                        resultYield = result.Data.CleanPrice.ToString("F4");
                    }
                    else if(calType.Equals("2"))//根据净价计算收益率
                    {
                        if (isExeBond)
                        {
                            resultYield = decimal.Multiply(result.Data.YieldToExecution, 100).ToString("F4");
                        }
                        else
                        {
                            resultYield = decimal.Multiply(result.Data.Yield, 100).ToString("F4");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                LogHelper.Error(e);
            }
            finally
            {

            }
            return resultYield;
        }

        /// <summary>
        /// 金额除以100
        /// </summary>
        /// <param name="price"></param>
        /// <returns></returns>
        public string NumChange(string price)
        {
            decimal d = 0;
            if (decimal.TryParse(price, out d))
            {
                d = d / 100;
                price = d.ToString();
            }

            return price;
        }
        #endregion


        /// <summary>
        /// 重新跑批执行
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public PResult ManualProcessingPosition(ManualProcessingPositionReq req)
        {
            return ApiManager.Instance.GetDefaultApi().Request<PResult>(ApiConsts.PositionSvr, ApiConsts.Topic_stc_ManualProcessingPosition, req);
        }

        /// <summary>
        /// 当前跑批任务
        /// </summary>
        /// <returns></returns>
        public QueryProcessingRecordResult QueryProcessingPosition()
        {
            return ApiManager.Instance.GetDefaultApi().Request<QueryProcessingRecordResult>(ApiConsts.PositionSvr, ApiConsts.Topic_stc_QueryProcessingRecordPosition, string.Empty);
        }

        /// <summary>
        /// 查询跑批历史
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public QueryProcessingRecordResult QueryProcessingPositionHist(QueryProcessingRecordHistReq req)
        {
            return ApiManager.Instance.GetDefaultApi().Request<QueryProcessingRecordResult>(ApiConsts.PositionSvr, ApiConsts.Topic_stc_QueryProcessingRecordHistPosition, req);
        }


        private bool isSubscribeProcessingRecordStatus = false;
        public Dictionary<string, Action<QueryProcessingRecordResult>> DictSubscribeProcessingRecordStatusCallBack = new Dictionary<string, Action<QueryProcessingRecordResult>>();

        private event Action<QueryProcessingRecordResult> OnProcessingRecordStatus;

        /// <summary>
        /// 订阅手动跑批通知
        /// </summary>
        /// <param name="onMsgCallBack"></param>
        public void SubscribeProcessingRecordStatus(Action<QueryProcessingRecordResult> callBack)
        {
            if (callBack != null)
            {
                string callbackkey = $"{callBack.Method.DeclaringType.FullName.ToString()}.{callBack.Method.Name.ToString()}";

                if (!DictSubscribeProcessingRecordStatusCallBack.ContainsKey(callbackkey))
                {
                    OnProcessingRecordStatus += callBack;
                    DictSubscribeProcessingRecordStatusCallBack.Add(callbackkey, callBack);
                }

                if (isSubscribeProcessingRecordStatus)
                {
                    return;
                }
            }

            lock (this)
            {
                if (!isSubscribeProcessingRecordStatus)
                {
                    string[] topics = new string[1];
                    topics[0] = $"{ApiConsts.Topic_stc_SubscribeProcessingRecordStatus}";
                    ApiManager.Instance.GetDefaultApi().SubScribeWithImage(DataBus.Instance.Instanse_OnMessage, topics, ApiConsts.PositionSvr, string.Empty);

                    isSubscribeProcessingRecordStatus = true;
                }
            }
        }

        public void UnSubscribeProcessingRecordStatus(Action<QueryProcessingRecordResult> callBack)
        {
            if (callBack != null && OnProcessingRecordStatus != null)
            {
                string key = $"{callBack.Method.DeclaringType.FullName.ToString()}.{callBack.Method.Name.ToString()}";
                if (DictSubscribeProcessingRecordStatusCallBack.ContainsKey(key))
                {
                    DictSubscribeProcessingRecordStatusCallBack.Remove(key);
                    OnProcessingRecordStatus -= callBack;
                }
            }

            if (OnProcessingRecordStatus == null)
            {
                ApiManager.Instance.GetDefaultApi().UnSubScribe(ApiConsts.PositionSvr, new string[] { $"{ApiConsts.Topic_stc_SubscribeProcessingRecordStatus}.**" });
                isSubscribeProcessingRecordStatus = false;
            }
        }


        public void ProcessProcessingRecordStatus(NetMessage netMsg)
        {
            if (netMsg != null)
            {
                if (netMsg.Topic.Contains(ApiConsts.Topic_stc_SubscribeProcessingRecordStatus))
                {
                    QueryProcessingRecord s = BaseHelper.ConvertStcMessage<QueryProcessingRecord>(new string[] { netMsg.Topic }, netMsg);
                    OnProcessingRecordStatus?.Invoke(new QueryProcessingRecordResult() { totalSize = 1 , Datas = new List<QueryProcessingRecord>() { s} });
                }
            }
        }

    }
}
