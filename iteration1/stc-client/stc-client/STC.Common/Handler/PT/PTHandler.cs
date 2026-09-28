using com.gateway.log;
using netapi.Api;
using STC.Common.Api;
using STC.Common.Dispatcher;
using STC.Common.Model;
using STC.Common.Model.Ans;
using STC.Common.Model.CDH;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.PT;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace STC.Common.Handler.PT
{
    public class PTHandler : AsyncBase<object>, IPTHandler
    {

        private static PTHandler ptHandler = new PTHandler();
        public static IPTHandler Instance { get { return ptHandler; } }
        private bool isSubscribeForExec = false;
        private event Action<List<TbookPtExecorders>> On_Exec;
        private string UserName = ApiManager.Instance.GetDefaultApi().UserName;

        #region 我的订单右键录入成交
        private event Action<DataRowView> OnSendPTEvent;
        private DataRowView ptExecorders = null;
        public void OnSendPT(DataRowView obj)
        {
            try
            {
                if (obj != null)
                {
                    ptExecorders = obj;
                    this.OnSendPTEvent?.Invoke(obj);
                }
            }
            catch (Exception ex)
            {

            }
        }
        public void SubscribeSendPT(Action<DataRowView> callData)
        {
            if (callData != null)
            {
                OnSendPTEvent += callData;
                if (ptExecorders != null)
                    callData.Invoke(ptExecorders);
            }
        }
        #endregion


        private PTHandler()
        {
            // this.start();
        }

        /// <summary>
        /// 订阅所有成交
        /// </summary>
        /// <param name="onMsgCallBack"></param>
        public void SubscribeExec(Action<List<TbookPtExecorders>> onMsgCallBack, string UserName)
        {
             this.UserName = UserName;
            if (onMsgCallBack != null && (On_Exec == null || !On_Exec.GetInvocationList().Contains(onMsgCallBack)))
                On_Exec += onMsgCallBack;

            lock (this)
            {
                if (!isSubscribeForExec)
                {
                    string[] topic = new string[] { $"{ApiConsts.Topic_stc_ptpeo}**" };
                    ApiManager.Instance.GetDefaultApi().SubScribeWithImage(DataBus.Instance.Instanse_OnMessage, topic
                       ,
                        // new string[] { ApiConsts.Topic_stc_ptpeo },
                        ApiConsts.PTSvr, string.Empty);
                    isSubscribeForExec = true;
                }
            }
        }

        public void ProcessPTExec(NetMessage netMsg)
        {
            if (netMsg != null)
            {
                // string[] topic = new string[] { $"{ApiConsts.Topic_stc_ptpeo}{UserName}." };
                // if (netMsg.Topic.Contains(ApiConsts.Topic_stc_ptpeo) && netMsg.Topic.Contains(UserName))
                //if (!string.IsNullOrWhiteSpace(this.UserName) && !"*".Equals(this.UserName))
                //{
                //    if (!netMsg.Topic.EndsWith(this.UserName))
                //        return;
                //}
               
                if (netMsg.Topic.StartsWith(ApiConsts.Topic_stc_ptpeo))
                {
                    if (netMsg.Data.StartsWith(ApiConsts.BRACKET))
                    {
                        List<TbookPtExecorders> data = BaseHelper.ConvertStcMessage<List<TbookPtExecorders>>(new string[] { netMsg.Topic }, netMsg);
                        if (data != null && data.Count > 0)
                        {
                            On_Exec?.Invoke(data);
                        }
                    }
                    else
                    {
                        List<TbookPtExecorders> data = new List<TbookPtExecorders>();
                        TbookPtExecorders mm = BaseHelper.ConvertStcMessage<TbookPtExecorders>(new string[] { netMsg.Topic }, netMsg);
                        /*if (!string.IsNullOrWhiteSpace(this.UserName) && !"*".Equals(this.UserName))
                        {
                            string[] str = netMsg.Topic.Replace(ApiConsts.Topic_stc_ptpeo, "").Split('.');
                            if (str == null || str.Length < 4)
                                return;
                            if (!str[1].Equals(this.UserName) && !(str[1] + "." + str[2]).Equals(this.UserName))
                            {
                                string partymanagerid = mm.Partymanagerid;
                                if (string.IsNullOrEmpty(partymanagerid))
                                    return;
                                else
                                {
                                    string[] partymanageridArr = partymanagerid.Split(',');
                                    if (!partymanageridArr.Contains(UserName))
                                    {
                                        return;
                                    }
                                }
                            }
                        }*/
                        data.Add(mm);
                        On_Exec?.Invoke(data);
                    }
                }
            }
        }



        /// <summary>
        /// 成交订单查询接口
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public PtExecordersReplyPo ExecOrderQuery(PtExecOrderQueryReq request)
        {
            return ApiManager.Instance.GetDefaultApi().Request<PtExecordersReplyPo>(ApiConsts.PTSvr, ApiConsts.Topic_ptExecOrderQuery, request);
        }

        /// <summary>
        /// 成交订单发送接口
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public PtExecSendReplyPo ExecOrderSend(PtExecOrderSaveReq req)
        {
            return ApiManager.Instance.GetDefaultApi().Request<PtExecSendReplyPo>(ApiConsts.PTSvr, ApiConsts.Topic_ptExecOrderSend, req);
        }

        /// <summary>
        /// 新增STC成交单
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public PResult ExecOrderSave(PtExecOrderSaveReq req)
        {
            return ApiManager.Instance.GetDefaultApi().Request<PResult>(ApiConsts.PTSvr, ApiConsts.Topic_ptExecOrderSave, req);
        }

        /// <summary>
        /// 新增STC成交单 批量
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public PResult ExecOrderBatchSave(List<PtExecOrderSaveReq> req)
        {
            return ApiManager.Instance.GetDefaultApi().Request<PResult>(ApiConsts.PTSvr, ApiConsts.Topic_ptExecOrderBatchSave, req);
        }
        /// <summary>
        /// 更新STC成交单
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public PtExecUpdateReplyPo ExecOrderUpdate(PtExecOrderSaveReq req)
        {
            return ApiManager.Instance.GetDefaultApi().Request<PtExecUpdateReplyPo>(ApiConsts.PTSvr, ApiConsts.Topic_ptExecOrderUpdate, req);
        }

        /// <summary>
        /// 删除STC成交单
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public PResult ExecOrderDelete(PtExecOrderSaveReq req)
        {
            return ApiManager.Instance.GetDefaultApi().Request<PResult>(ApiConsts.PTSvr, ApiConsts.Topic_ptExecOrderDelete, req);
        }

        public PtExecUpdateReplyPo ExecOrderBatchUpdate(List<PtExecOrderSaveReq> req)
        {
            return ApiManager.Instance.GetDefaultApi().Request<PtExecUpdateReplyPo>(ApiConsts.PTSvr, ApiConsts.Topic_ptExecOrderBatchUpdate, req);
        }

        public PtExecordersReplyPo ExecOrderUnSendQuery(PTExecOrderUnSendReq req)
        {
            return ApiManager.Instance.GetDefaultApi().Request<PtExecordersReplyPo>(ApiConsts.PTSvr, ApiConsts.Topic_ptExecOrderUnSendQuery, req);
        }

        public PtExecordersReplyPo ExecOrderApprovalHistQuery(List<string> req)
        {
            return ApiManager.Instance.GetDefaultApi().Request<PtExecordersReplyPo>(ApiConsts.PTSvr, ApiConsts.Topic_ptExecOrderApprovalHistQuery, req);
        }

        public PtExecordersReplyPo ExecOrderMvtQuery(PtExecOrderQueryReq request)
        {
            return ApiManager.Instance.GetDefaultApi().Request<PtExecordersReplyPo>(ApiConsts.PTSvr, ApiConsts.Topic_ptExecOrderMvtQuery, request);
        }

        public PResult ExecOrderRiskCheck(PtExecOrderQueryReq request)
        {
            return ApiManager.Instance.GetDefaultApi().Request<PResult>(ApiConsts.PTSvr, ApiConsts.Topic_ptExecOrderRiskCheck, request);
        }
    }
}
