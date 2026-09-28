using com.gateway.log;
using netapi.Api;
using Newtonsoft.Json;
using STC.Common.Api;
using STC.Common.Dispatcher;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.Table;
using STC.Common.Model.Table.WorkBrench;
using STC.Common.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Handler.Table
{
    public class TableOldHandler : AsyncBase<object>, ITableOldHandler
    {
        private static TableOldHandler tableHandler = new TableOldHandler();
        private bool isSubscribeForTBookIQ = false;
        private event Action<STC.Common.Model.Table.WorkBrench.IntentionQuoteInfo> OnIntentionQuoteInfo;

        private bool isSubscribeForTBookIR = false;
        private event Action<STC.Common.Model.Table.WorkBrench.IntentionQuoteInfo> OnRecognition;

        private bool isSubscribeForQOR = false;
        private event Action<STC.Common.Model.Table.EmsOrderReport> OnQor;

        private bool isSubscribeForQOR_Irs = false;
        private event Action<STC.Common.Model.Table.EmsOrderReport> OnQor_Irs;

        private bool isSubscribeForInquiry = false;
        private event Action<TableIntentionInquiry> OnInquiry;

        public static ITableOldHandler Instance { get { return tableHandler; } }

        private TableOldHandler()
        {
            this.start();
        }

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


        public STC.Common.Model.Table.WorkBrench.RecognizeIntentionQuoteReplyPo RequestIntention(STC.Common.Model.Table.WorkBrench.RecognizeIntentionQuoteRequest request)
        {
            return ApiManager.Instance.GetDefaultApi().Request<STC.Common.Model.Table.WorkBrench.RecognizeIntentionQuoteReplyPo>(ApiConsts.TableSvr, ApiConsts.Topic_publishRecognizeIntentionQuote, request);
        }

        public STC.Common.Model.Table.WorkBrench.RecognizeIntentionQuoteReplyPo RecognizeIntentionQuote(STC.Common.Model.Table.WorkBrench.RecognizeIntentionQuoteRequest request)
        {
            return ApiManager.Instance.GetDefaultApi().Request<STC.Common.Model.Table.WorkBrench.RecognizeIntentionQuoteReplyPo>(ApiConsts.TableSvr, ApiConsts.Topic_Old_RecognizeIntentionQuote, request);
        }
        public STC.Common.Model.Table.WorkBrench.IntentionQuoteReplyPo QueryIntentionQuote(STC.Common.Model.Table.WorkBrench.IntentionQuoteRequest request)
        {
            return ApiManager.Instance.GetDefaultApi().Request<STC.Common.Model.Table.WorkBrench.IntentionQuoteReplyPo>(ApiConsts.TableSvr, ApiConsts.Topic_Old_QueryIntentionQuote, request);
        }

        public STC.Common.Model.Table.WorkBrench.ConfirmIntentionQuoteReplyPo ConfirmIntentionQuote(STC.Common.Model.Table.WorkBrench.ConfirmIntentionQuoteRequest request)
        {
            return ApiManager.Instance.GetDefaultApi().Request<STC.Common.Model.Table.WorkBrench.ConfirmIntentionQuoteReplyPo>(ApiConsts.TableSvr, ApiConsts.Topic_Old_ConfirmIntentionQuote, request);
        }

        public PResult UpdateIntentionQuote(STC.Common.Model.Table.WorkBrench.UpdateIntentionQuoteRequest request)
        {
            return ApiManager.Instance.GetDefaultApi().Request<PResult>(ApiConsts.TableSvr, ApiConsts.Topic_Old_UpdateIntentionQuote, request);
        }

        public InsertIntentionQuoteReplyPo InsertIntentionQuote(STC.Common.Model.Table.WorkBrench.InsertIntentionQuoteReq request)
        {
            return ApiManager.Instance.GetDefaultApi().Request<InsertIntentionQuoteReplyPo>(ApiConsts.TableSvr, ApiConsts.Topic_Old_InsertIntentionQuote, request);
        }

        public PResult DeleteIntentionQuote(STC.Common.Model.Table.WorkBrench.DeleteIntentionQuoteReq request)
        {
            return ApiManager.Instance.GetDefaultApi().Request<PResult>(ApiConsts.TableSvr, ApiConsts.Topic_Old_DeleteIntentionQuote, request);
        }

        public QueryEmsOrderReplyPo QueryEmsOrder(QueryEmsOrderReq request)
        {
            return ApiManager.Instance.GetDefaultApi().Request<QueryEmsOrderReplyPo>(ApiConsts.TableSvr, ApiConsts.Topic_stc_QueryEmsOrder, request);
        }

        public QueryEmsOrderReplyPo QueryEmsOrder_Irs(QueryEmsOrderReq request)
        {
            return ApiManager.Instance.GetDefaultApi().Request<QueryEmsOrderReplyPo>(ApiConsts.TableSvr, ApiConsts.Topic_stc_QueryEmsOrder_Irs, request);
        }

        public QueryRolesReplyPo QueryRoles()
        {
            return ApiManager.Instance.GetDefaultApi().Request<QueryRolesReplyPo>(ApiConsts.TableSvr, ApiConsts.Topic_stc_QueryRoles, "{}");
        }

        public QueryQuoteTagGroupReplyPo QueryQuoteTagGroup(QueryQuoteTagGroupRequest req)
        {
            return ApiManager.Instance.GetDefaultApi().Request<QueryQuoteTagGroupReplyPo>(ApiConsts.TableSvr, ApiConsts.Topic_stc_QuoteTagGroup, req);
        }

        public QueryQuoteTagReplyPo QueryQuoteTag()
        {
            var x = ApiManager.Instance.GetDefaultApi().Request<QueryQuoteTagReplyPo>(ApiConsts.TableSvr, ApiConsts.Topic_stc_QuoteTag, "{}");
            string detail = JsonConvert.SerializeObject(x);
            return ApiManager.Instance.GetDefaultApi().Request<QueryQuoteTagReplyPo>(ApiConsts.TableSvr, ApiConsts.Topic_stc_QuoteTag, "{}");
        }

        public QueryTagsCountReplyPo QueryTagsCount(QueryTagsCountRequest req)
        {
            return ApiManager.Instance.GetDefaultApi().Request<QueryTagsCountReplyPo>(ApiConsts.TableSvr, ApiConsts.Topic_stc_Tags_Count, req);
        }

        public QueryTagsLoadReplyPo QueryTagsLoad(QueryTagsLoadRequest req)
        {
            return ApiManager.Instance.GetDefaultApi().Request<QueryTagsLoadReplyPo>(ApiConsts.TableSvr, ApiConsts.Topic_stc_Tags_Load, req);
        }

        public QueryQuoteTagReplyPo QueryMMSecurityidHist()
        {
            return ApiManager.Instance.GetDefaultApi().Request<QueryQuoteTagReplyPo>(ApiConsts.TableSvr, ApiConsts.Topic_stc_QuoteTag, "{}");
        }

        /// <summary>
        /// 意向报价状态订阅
        /// </summary>
        /// <param name="onIntentionallback"></param>
        public void SubscribeIntention(string[] topics, Action<STC.Common.Model.Table.WorkBrench.IntentionQuoteInfo> onIntentionallback)
        {
            if (onIntentionallback != null && (OnIntentionQuoteInfo == null || !OnIntentionQuoteInfo.GetInvocationList().Contains(onIntentionallback)))
            {
                OnIntentionQuoteInfo += onIntentionallback;
            }

            lock(this)
            {
                if (!isSubscribeForTBookIQ)
                {
                    isSubscribeForTBookIQ = true;
                    ApiManager.Instance.GetDefaultApi().SubScribeWithImage(Instance_OnIntention,
                         topics, ApiConsts.TableSvr, "");
                }
            }
        }

        /// <summary>
        /// Cef订阅TableSvr
        /// </summary>
        /// <param name="topic"></param>
        public void CefSubScribeWithImage(string[] topic)
        {
            ApiManager.Instance.GetDefaultApi().SubScribeWithImage(Instance_OnIntention, topic.ToArray(), ApiConsts.TableSvr, "");
        }

        /// <summary>
        /// Tbook意向报价回调函数
        /// </summary>
        /// <param name="us"></param>
        private void Instance_OnIntention(object info)
        {
            NetMessage netMsg = info as NetMessage;
            if (netMsg == null)
                return;
            if (netMsg.Topic.StartsWith(ApiConsts.Topic_Old_stc_table_iq)
                || netMsg.Topic.StartsWith(ApiConsts.Topic_Old_stc_table_iqbiz))
            {
                this.add(netMsg);
                //Cef订阅深度行情广播消息
                Common.Utils.AppUtils.Application.PluginManager.Application.BroadCast("GateWayNetMessage", netMsg);
            }

        }

        public void ProcessIntention(NetMessage netMsg)
        {
            if (netMsg.Topic.StartsWith($"{ApiConsts.Topic_Old_stc_table_iq}{LocalEnv.UserInfo.UserName}"))
            {
                STC.Common.Model.Table.WorkBrench.IntentionQuoteInfo data = BaseHelper.ConvertStcMessage<STC.Common.Model.Table.WorkBrench.IntentionQuoteInfo>(new string[] { $"{ApiConsts.Topic_Old_stc_table_iq}{LocalEnv.UserInfo.UserName}" }, netMsg);
                if (data != null)
                    OnIntentionQuoteInfo?.Invoke(data);
            }
            else if (netMsg.Topic.StartsWith($"{ApiConsts.Topic_Old_stc_table_iqbiz}{LocalEnv.UserInfo.Termids}.{LocalEnv.UserInfo.UserName}"))
            {
                STC.Common.Model.Table.WorkBrench.IntentionQuoteInfo data = BaseHelper.ConvertStcMessage<STC.Common.Model.Table.WorkBrench.IntentionQuoteInfo>(new string[] { $"{ApiConsts.Topic_Old_stc_table_iqbiz}{LocalEnv.UserInfo.Termids}.{LocalEnv.UserInfo.UserName}" }, netMsg);
                if (data != null)
                    OnIntentionQuoteInfo?.Invoke(data);
            }
        }

        /// <summary>
        /// 意向报价识别订阅
        /// </summary>
        /// <param name="onIntentionallback"></param>
        public void SubscribeRecognization(string[] topics, Action<STC.Common.Model.Table.WorkBrench.IntentionQuoteInfo> onRecognitionCallback)
        {
            if (onRecognitionCallback != null && (OnRecognition == null || !OnRecognition.GetInvocationList().Contains(onRecognitionCallback)))
                OnRecognition += onRecognitionCallback;

            lock(this)
            {
                if (!isSubscribeForTBookIR)
                {
                    isSubscribeForTBookIR = true;
                    ApiManager.Instance.GetDefaultApi().SubScribeWithImage(Instance_OnRecognition,
                         topics, ApiConsts.TableSvr, "");
                }
            }
        }

        /// <summary>
        /// Tbook意向报价识别回调函数
        /// </summary>
        /// <param name="us"></param>
        private void Instance_OnRecognition(object info)
        {
            NetMessage netMsg = info as NetMessage;
            if (netMsg == null)
                return;
            if (netMsg.Topic.StartsWith(ApiConsts.Topic_Old_stc_table_ir))
                this.add(netMsg);
        }

        public void ProcessRecognition(NetMessage netMsg)
        {
            STC.Common.Model.Table.WorkBrench.IntentionQuoteInfo data = BaseHelper.ConvertStcMessage<STC.Common.Model.Table.WorkBrench.IntentionQuoteInfo>(new string[] { $"{ApiConsts.Topic_Old_stc_table_ir}{LocalEnv.UserInfo.UserName}" }, netMsg);
            if (data != null)
                OnRecognition?.Invoke(data);
        }

        /// <summary>
        /// 时时更新接口
        /// </summary>
        /// <param name="onRecognitionCallback"></param>
        public void SubscribeQor(Action<STC.Common.Model.Table.EmsOrderReport> onRecognitionCallback, bool isPermission = true)
        {
            if (onRecognitionCallback != null && (OnQor == null || !OnQor.GetInvocationList().Contains(onRecognitionCallback)))
                OnQor += onRecognitionCallback;

            lock(this)
            {
                if (!isSubscribeForQOR)
                {
                    isSubscribeForQOR = true;

                    

                    if (isPermission)
                    {
                        if (string.IsNullOrEmpty(LocalEnv.UserInfo.Termids))
                        {
                            ApiManager.Instance.GetDefaultApi().SubScribeWithImage(Instance_OnQor,
                                new string[] { $"{ApiConsts.Topic_Old_QOR}.**" }, ApiConsts.TableSvr, "");
                        }
                        else
                        {
                            List<string> list = new List<string>();
                            list.Add($"{ApiConsts.Topic_Old_QOR}.{LocalEnv.UserInfo.Termids}.**");
                            if (LocalEnv.UserInfo.mdgroupids != null && LocalEnv.UserInfo.mdgroupids.Count > 0)
                            {
                                foreach (string otherGroupId in LocalEnv.UserInfo.mdgroupids)
                                {
                                    if (otherGroupId != LocalEnv.UserInfo.Termids)
                                    {
                                        list.Add($"{ApiConsts.Topic_Old_QOR}.{otherGroupId}.**");
                                    }
                                }
                            }
                            
                            ApiManager.Instance.GetDefaultApi().SubScribeWithImage(Instance_OnQor,
                                list.ToArray(), ApiConsts.TableSvr, "");
                        }
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(LocalEnv.UserInfo.Termids))
                        {
                            ApiManager.Instance.GetDefaultApi().SubScribeWithImage(Instance_OnQor,
                                new string[] { $"{ApiConsts.Topic_Old_QOR}.{LocalEnv.UserInfo.UserName}" }, ApiConsts.TableSvr, "");
                        }
                        else
                        {
                            ApiManager.Instance.GetDefaultApi().SubScribeWithImage(Instance_OnQor,
                                new string[] { $"{ApiConsts.Topic_Old_QOR}.{LocalEnv.UserInfo.Termids}.{LocalEnv.UserInfo.UserName}" }, ApiConsts.TableSvr, "");
                        }
                    }
                }
            }
        }

        private void Instance_OnQor(object info)
        {
            NetMessage netMsg = info as NetMessage;
            if (netMsg == null)
                return;
            if (netMsg.Topic.StartsWith(ApiConsts.Topic_Old_QOR)
                && !netMsg.Topic.StartsWith(ApiConsts.Topic_Old_QOR_IRS))
                this.add(netMsg);
        }

        public void SubscribeQor_Irs(Action<STC.Common.Model.Table.EmsOrderReport> onQorIrsCallback, bool isPermission = true)
        {
            if (onQorIrsCallback != null && (OnQor_Irs == null || !OnQor_Irs.GetInvocationList().Contains(onQorIrsCallback)))
                OnQor_Irs += onQorIrsCallback;

            lock (this)
            {
                if (!isSubscribeForQOR_Irs)
                {
                    isSubscribeForQOR_Irs = true;
                    if (isPermission)
                    {
                        ApiManager.Instance.GetDefaultApi().SubScribeWithImage(Instance_OnQor_Irs,
                        new string[] { $"{ApiConsts.Topic_Old_QOR_IRS}.**" }, ApiConsts.TableSvr, "");
                    }
                    else
                    {
                        ApiManager.Instance.GetDefaultApi().SubScribeWithImage(Instance_OnQor_Irs,
                        new string[] { $"{ApiConsts.Topic_Old_QOR_IRS}.{LocalEnv.UserInfo.UserName}" }, ApiConsts.TableSvr, "");
                    }
                }
            }
        }
        private void Instance_OnQor_Irs(object info)
        {
            NetMessage netMsg = info as NetMessage;
            if (netMsg == null)
                return;
            if (netMsg.Topic.StartsWith(ApiConsts.Topic_Old_QOR_IRS))
                this.add(netMsg);
        }


        public void ProcessQor(NetMessage netMsg)
        {
            STC.Common.Model.Table.EmsOrderReport data = BaseHelper.ConvertStcMessage<STC.Common.Model.Table.EmsOrderReport>(new string[] { $"{ApiConsts.Topic_Old_QOR}.**" }, netMsg);
            if (data != null)
                OnQor?.Invoke(data);
        }

        public void ProcessQor_Irs(NetMessage netMsg)
        {
            STC.Common.Model.Table.EmsOrderReport data = BaseHelper.ConvertStcMessage<STC.Common.Model.Table.EmsOrderReport>(new string[] { $"{ApiConsts.Topic_Old_QOR_IRS}.**" }, netMsg);
            if (data != null)
                OnQor_Irs?.Invoke(data);
        }

        public override void process(object t)
        {
            try
            {
                NetMessage netMsg = t as NetMessage;
                if (netMsg == null)
                    return;
                if (netMsg.Topic.StartsWith(ApiConsts.Topic_Old_QOR_IRS))
                {
                    ProcessQor_Irs(netMsg);
                }
                else if (netMsg.Topic.StartsWith(ApiConsts.Topic_Old_QOR))
                {
                    ProcessQor(netMsg);
                }
                else if (netMsg.Topic.StartsWith(ApiConsts.Topic_Old_stc_table_iq)
                    || netMsg.Topic.StartsWith(ApiConsts.Topic_Old_stc_table_iqbiz))
                {
                    ProcessIntention(netMsg);
                }
                else if (netMsg.Topic.StartsWith(ApiConsts.Topic_Old_stc_table_ir))
                {
                    ProcessRecognition(netMsg);
                }
                else if (netMsg.Topic.StartsWith(ApiConsts.Topic_stc_table_inq))
                {
                    ProcessInquiry(netMsg);
                }
            }
            catch (Exception e)
            {
                LogHelper.Error(e);
            }
        }

        public IntentionInquiryPo IntentionInquiry(IntentionInquiryRequest request)
        {
            return ApiManager.Instance.GetDefaultApi().Request<IntentionInquiryPo>(ApiConsts.TableSvr, ApiConsts.Topic_stc_Intention_Inquiry, request);
        }

        /// <summary>
        /// 询价订阅
        /// </summary>
        /// <param name="onCallback"></param>
        public void SubscribeInquiry(string[] topics, Action<TableIntentionInquiry> onCallback)
        {
            if (onCallback != null && (OnInquiry == null || !OnInquiry.GetInvocationList().Contains(onCallback)))
            {
                OnInquiry += onCallback;
            }

            lock (this)
            {
                if (!isSubscribeForInquiry)
                {
                    isSubscribeForInquiry = true;
                    ApiManager.Instance.GetDefaultApi().SubScribeWithImage(Instance_OnInquiry,
                         topics, ApiConsts.TableSvr, "");
                }
            }
        }

        /// <summary>
        /// 询价回调函数
        /// </summary>
        /// <param name="us"></param>
        private void Instance_OnInquiry(object info)
        {
            NetMessage netMsg = info as NetMessage;
            if (netMsg == null)
                return;
            if (netMsg.Topic.StartsWith(ApiConsts.Topic_stc_table_inq))
                this.add(netMsg);
        }

        public void ProcessInquiry(NetMessage netMsg)
        {
            if (netMsg.Topic.StartsWith($"{ApiConsts.Topic_stc_table_inq}{LocalEnv.UserInfo.Termids}.{LocalEnv.UserInfo.UserName}"))
            {
                TableIntentionInquiry data = BaseHelper.ConvertStcMessage<TableIntentionInquiry>(new string[] { $"{ApiConsts.Topic_stc_table_inq}{LocalEnv.UserInfo.Termids}.{LocalEnv.UserInfo.UserName}" }, netMsg);
                if (data != null)
                    OnInquiry?.Invoke(data);
            }
        }

        public PResult BatchPublishIntention(Model.Table.WorkBrench.BatchPublishIntentionQuoteReq req)
        {
            return ApiManager.Instance.GetDefaultApi().Request<PResult>(ApiConsts.TableSvr, ApiConsts.Topic_batchPublishIntentionQuote, req);
        }

        public STC.Common.Model.Table.WorkBrench.IntentionQuoteReplyPo GetBuyerIntentionLogs(string intentionid)
        {
            return ApiManager.Instance.GetDefaultApi().Request<STC.Common.Model.Table.WorkBrench.IntentionQuoteReplyPo>(ApiConsts.TableSvr, ApiConsts.Topic_stc_table_queryIntentionMvt, new { intentionId = intentionid });
        }

        public IntentionExecutionAssociateReplyPo ProcessIntentionExecutionAssociate(IntentionExecutionAssociateReq req)
        {
            return ApiManager.Instance.GetDefaultApi().Request<STC.Common.Model.Table.WorkBrench.IntentionExecutionAssociateReplyPo>(ApiConsts.TableSvr, ApiConsts.Topic_stc_table_intention_execution_associate, req);
        }

        public InsertIntentionQuoteReplyPo InsertIntentionQuoteNew(STC.Common.Model.Table.WorkBrench.InsertIntentionQuoteReq request)
        {
            return ApiManager.Instance.GetDefaultApi().Request<InsertIntentionQuoteReplyPo>(ApiConsts.TableSvr, ApiConsts.Topic_InsertIntentionQuote_new, request);
        }
    }
}
