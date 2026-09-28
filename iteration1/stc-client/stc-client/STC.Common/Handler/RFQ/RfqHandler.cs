using netapi.Api;
using Newtonsoft.Json;
using STC.Common.Api;
using STC.Common.Dispatcher;
using STC.Common.Handler.Cal;
using STC.Common.Model.Cal;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.Position;
using STC.Common.Model.RFQ;
using STC.Common.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace STC.Common.Handler.RFQ
{
    public class RfqHandler
    {
        private static RfqHandler instance = new RfqHandler();
        public static RfqHandler Instance { get { return instance; } }

        public Dictionary<string, Action<QueryRfqOrderResult>> DictCallBack = new Dictionary<string, Action<QueryRfqOrderResult>>();
        /// <summary>
        /// Rfq列表
        /// </summary>
        private event Action<QueryRfqOrderResult> OnRfqOrder;

        public bool isSubscribeForRfqOrder;

        private object lockobj = new object();
        private bool isSubscribeForRfq = false;

        private event Action<RfqstgViewModel> On_Rfq;
        public RfqHandler()
        {
        }

        #region Rfq列表
        /// <summary>
        /// 3.7	询价查询接口
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public QueryRfqOrderResult QueryRfqOrder(QueryRfqOrderReq req)
        {
            return ApiManager.Instance.GetDefaultApi().Request<QueryRfqOrderResult>(ApiConsts.RFQSvr, ApiConsts.Topic_stc_RfqQueryOrder, req);
        }


        public void SubscribeRfqOrder(Action<QueryRfqOrderResult> callBack)
        {
            if (callBack != null)
            {
                string callbackkey = $"{callBack.Method.DeclaringType.FullName.ToString()}.{callBack.Method.Name.ToString()}";

                lock (lockobj)
                {

                    if (!DictCallBack.ContainsKey(callbackkey))
                    {
                        OnRfqOrder += callBack;
                        DictCallBack.Add(callbackkey, callBack);

                        if (isSubscribeForRfqOrder)
                        {
                            //callBack?.Invoke(CacheTradeStatisticModel.ToList());
                            return;
                        }
                    }
                }
            }

            lock (this)
            {
                if (!isSubscribeForRfqOrder)
                {
                    string[] topics = new string[1];
                    topics[0] = $"{ApiConsts.Topic_stc_RfqOrder}";
                    ApiManager.Instance.GetDefaultApi().SubScribe(DataBus.Instance.Instanse_OnMessage, topics, ApiConsts.RFQSvr, string.Empty);
                    isSubscribeForRfqOrder = true;
                }
            }
        }

        public void UnSubscribeRfqOrder(Action<QueryRfqOrderResult> callBack)
        {
            string callbackkey = $"{callBack.Method.DeclaringType.FullName.ToString()}.{callBack.Method.Name.ToString()}";
            DictCallBack.Remove(callbackkey);
            OnRfqOrder -= callBack;
            isSubscribeForRfqOrder = false;
        }

        #endregion

        /// <summary>
        /// 订阅当前用户下所有关注组债券
        /// </summary>
        /// <param name="onMsgCallBack"></param>
        public void SubscribeRfq(Action<RfqstgViewModel> onMsgCallBack)
        {
            if (onMsgCallBack != null && (On_Rfq == null || !On_Rfq.GetInvocationList().Contains(onMsgCallBack)))
                On_Rfq += onMsgCallBack;
            
            lock (this)
            {
                if (!isSubscribeForRfq)
                {
                    ApiManager.Instance.GetDefaultApi().SubScribe(OnRfq,
                        new string[] { string.Format(ApiConsts.Topic_stc_RfqstgView_Subscribe, LocalEnv.UserInfo.UserName) }, ApiConsts.RFQMSvr, string.Empty);

                    isSubscribeForRfq = true;
                }
            }
        }

        /// <summary>
        /// RFQ关注组债券处理
        /// </summary>
        /// <param name="obj"></param>
        private void OnRfq(object obj)
        {
            NetMessage netMsg = obj as NetMessage;
            if (netMsg == null)
                return;

            if (netMsg.Topic.StartsWith(ApiConsts.Topic_stc_RfqstgView) && netMsg.Topic.EndsWith(LocalEnv.UserInfo.UserName))
            {
                if (netMsg.Data.StartsWith(ApiConsts.BRACKET))
                {
                    List<RfqstgViewModel> data = JsonConvert.DeserializeObject<List<RfqstgViewModel>>(netMsg.Data);
                    if (data != null && data.Count > 0)
                    {
                        foreach (RfqstgViewModel mm in data)
                        {
                            On_Rfq?.Invoke(mm);
                        }
                    }
                }
                else
                {
                    RfqstgViewModel mm = JsonConvert.DeserializeObject<RfqstgViewModel>(netMsg.Data);

                    On_Rfq?.Invoke(mm);
                }
            }
        }


        public void UnSubscribeRfq(Action<RfqstgViewModel> onMsgCallBack)
        {
            if (onMsgCallBack != null && On_Rfq != null)
                On_Rfq -= onMsgCallBack;
        }

        /// <summary>
        /// RFQ订单回调
        /// </summary>
        /// <param name="netMsg"></param>
        public void ProcessRfqOrder(NetMessage netMsg)
        {
            if (netMsg != null)
            {
                if (netMsg.Data.StartsWith(ApiConsts.BRACKET))
                {
                    QueryRfqOrderResult mmSysStatus = BaseHelper.ConvertStcMessage<QueryRfqOrderResult>(new string[] { netMsg.Topic }, netMsg);
                    OnRfqOrder?.Invoke(mmSysStatus);
                }
                else
                {
                    ExecutionReport s = BaseHelper.ConvertStcMessage<ExecutionReport>(new string[] { netMsg.Topic }, netMsg);
                    QueryRfqOrderResult qror = new QueryRfqOrderResult();
                    qror.Datas = new List<ExecutionReport>() { s };
                    qror.Code = "0000";
                    OnRfqOrder?.Invoke(qror);
                }
            }
        }

        /// <summary>
        /// //(改，回) QuoteStatus 都是 33  区别是 QuoteID有无  撤单 撤40，拒38
        /// </summary>
        /// <param name="cancelOrder"></param>
        /// <returns></returns>
        public PResult ChangeAcceptOrder(RfqChangeAcceptOrderReq req)
        {
            return ApiManager.Instance.GetDefaultApi().Request<PResult>(ApiConsts.RFQSvr, ApiConsts.Topic_stc_RfqOrder_ChangeAccept, req);
        }


        /// <summary>
        /// Taker请求报价/撤销请求报价
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public PResult RfqNewOrder(RfqNewOrderReq req)
        {
            return ApiManager.Instance.GetDefaultApi().Request<PResult>(ApiConsts.RFQSvr, ApiConsts.Topic_stc_RfqQuoteRequest, req);
        }



        //private void OnRfq(object obj)
        //{
        //    NetMessage netMsg = obj as NetMessage;
        //    if (netMsg == null)
        //        return;
        //    if (netMsg.Topic.StartsWith(ApiConsts.Topic_stc_mm_regex))
        //        this.add(obj);
        //}


        // 关注组操作
        public PResult RfqGroupUpdate(RfqGroupReq request)
        {
           //return ApiManager.Instance.GetDefaultApi().Request<PResult>(ApiConsts.MMSvr, ApiConsts.Topic_stc_updateMMGroup, request);
           return ApiManager.Instance.GetDefaultApi().Request<PResult>(ApiConsts.RFQMSvr, ApiConsts.Topic_stc_updateRfqGroup, request);
        }

        // 关注组查询
        public RfqGroupReplyPo RfqGroupQuery()
        {
            return ApiManager.Instance.GetDefaultApi().Request<RfqGroupReplyPo>(ApiConsts.RFQMSvr, ApiConsts.Topic_stc_queryRfqGroup, new RfqGroupReq() { action = "0"});
        }

        /// <summary>
        /// 分组策略查询 请求/响应/订阅
        /// </summary>
        /// <returns></returns>
        public RfqstgViewResult RfqStgViewQuery(RfqstgViewReq req)
        {
            return ApiManager.Instance.GetDefaultApi().Request<RfqstgViewResult>(ApiConsts.RFQMSvr, ApiConsts.Topic_stc_RfqstgView, req);
        }

        /// <summary>
        /// 策略新增、删除、修改、启动、停止
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public PResult RfqUpdatestgAct(RfqstgActReq request)
        {
            return ApiManager.Instance.GetDefaultApi().Request<PResult>(ApiConsts.RFQMSvr, ApiConsts.Topic_stc_RfqstgAct, request);
        }


        // 单债券开关接口
        public PResult RfqUpdateEnable(RfqUpdateEnableReq request)
        {
            return ApiManager.Instance.GetDefaultApi().Request<PResult>(ApiConsts.RFQMSvr, ApiConsts.Topic_stc_RfqstgAct, request);
        }

        #region 计算净价
        /// <summary>
        /// 计算净价和成交价
        /// </summary>
        /// <param name="securityID"></param>
        /// <param name="calType">4 计算净价 2 计算收益率</param>
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
                else if (calType.Equals("2")) //根据净价计算收益率
                {
                    netPrice = (result.Data.Yield * 100).ToString("F4");
                    return netPrice;
                }

            }

            return string.Empty;
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
    }
}
