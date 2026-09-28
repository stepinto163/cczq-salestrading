using com.gateway.log;
using netapi.Api;
using STC.Common.Api;
using STC.Common.Handler.Admin;
using STC.Common.Handler.CRM;
using STC.Common.Handler.Inner;
using STC.Common.Handler.MarketExec;
using STC.Common.Handler.Monitor;
using STC.Common.Handler.OHLC;
using STC.Common.Handler.Position;
using STC.Common.Handler.PT;
using STC.Common.Handler.Ptr;
using STC.Common.Handler.Quant;
using STC.Common.Handler.RFQ;
using STC.Common.Handler.Risk;
using STC.Common.Handler.Table;
using STC.Common.Handler.TBook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace STC.Common.Dispatcher
{
    public class DataBus : AsyncBase<object>
    {
        private DataBus()
        {
            this.start();
        }

        private static DataBus _instance = new DataBus();

        public static DataBus Instance
        {
            get
            {
                return _instance;
            }
        }

        public void Instanse_OnMessage(object obj)
        {
            this.add(obj);
        }

        public override void process(object t)
        {
            try
            {
                NetMessage netMsg = t as NetMessage;
                if (netMsg == null)
                    return;
                // EmsSvr
                if (netMsg.Topic.StartsWith(ApiConsts.Topic_stc_ems_executionreport_cdh_prefix)
                    || netMsg.Topic.StartsWith(ApiConsts.Topic_stc_ems_executionreport_c_sdn_prefix)
                    || netMsg.Topic.StartsWith(ApiConsts.Topic_stc_ems_executionreport_cmds_prefix)
                    || netMsg.Topic.StartsWith(ApiConsts.Topic_stc_ems_executionreport_cmds_qdmesp_prefix)
                    || netMsg.Topic.StartsWith(ApiConsts.Topic_stc_ems_executionreport_sse_prefix)
                    || netMsg.Topic.StartsWith(ApiConsts.Topic_stc_ems_executionreport_ssf_prefix)
                    || netMsg.Topic.StartsWith(ApiConsts.Topic_stc_ems_executionreport_sze_prefix)
                    || netMsg.Topic.StartsWith(ApiConsts.Topic_stc_ems_executionreport_szf_prefix)
                    || netMsg.Topic.StartsWith(ApiConsts.Topic_stc_ems_executionreport_prefix))
                {
                    MarketExecHandler.Instance.ProcessExecuteOrder(netMsg);
                }
                else if (netMsg.Topic.StartsWith(ApiConsts.Topic_stc_ems_order_prefix))
                {
                    MarketExecHandler.Instance.ProcessEmsOrder(netMsg);
                }
                // TBookSvr
                else if (netMsg.Topic.StartsWith(ApiConsts.Topic_stc_tbook_prefix))
                {
                    TBookHandler.Instance.ProcessOrder(netMsg);
                    //策略订单(个人订单或全部订单)
                    TBookHandler.Instance.ProcessQuantOrder(netMsg);
                }
                else if (netMsg.Topic.StartsWith(ApiConsts.Topic_stc_tbook_con_prefix))
                {
                    TBookHandler.Instance.ProcessConStatus(netMsg);
                }
                else if (netMsg.Topic.StartsWith(ApiConsts.Topic_stc_tbook_system_param_prefix))
                {
                    TBookHandler.Instance.ProcessSystemParam(netMsg);
                }
                // OhlcHandler
                else if (netMsg.Topic.StartsWith(ApiConsts.Topic_stc_OHLC_KLine_prefix))
                {
                    OhlcHandler.Instance.ProcessKlineMsg(netMsg);
                }
                else if (netMsg.Topic.StartsWith(ApiConsts.Topic_stc_OHLC))
                {
                    OhlcHandler.Instance.ProcessOHLC(netMsg);
                }
                // AdminHandler
                else if (netMsg.Topic.StartsWith(ApiConsts.Topic_stc_admin_prefix))
                {
                    AdminHandler.Instance.ProcessAdminMsg(netMsg);
                }
                // InnerHandler
                else if (netMsg.Topic.StartsWith(ApiConsts.Topic_stc_inner_order))
                {
                    InnerHandler.Instance.ProcessInnerOrder(netMsg);
                }
                // PositionHandler
                else if (netMsg.Topic.StartsWith(ApiConsts.Topic_stc_PostionDetail))
                {
                    PositionHandler.Instance.ProcessPositionDetail(netMsg);
                }
                else if (netMsg.Topic.StartsWith(ApiConsts.Topic_stc_TradeStatistics))
                {
                    PositionHandler.Instance.ProcessTradeStatistics(netMsg);
                }
                // QuantHandler
                else if (netMsg.Topic.StartsWith(ApiConsts.Topic_stc_Indicator))
                {
                    QuantHandler.Instance.ProcessIndicator(netMsg);
                }
                else if (netMsg.Topic.StartsWith(ApiConsts.Topic_stc_QuantLog))
                {
                    QuantHandler.Instance.ProcessQuantLog(netMsg);
                }
                else if (netMsg.Topic.StartsWith(ApiConsts.Topic_stc_Paras))
                {
                    QuantHandler.Instance.ProcessParas(netMsg);
                }
                else if (netMsg.Topic.StartsWith(ApiConsts.Topic_stc_Strategy))
                {
                    QuantHandler.Instance.ProcessMM(netMsg);
                }
                // RiskHandler
                else if (netMsg.Topic.StartsWith(ApiConsts.Topic_stc_risk_prefix))
                {
                    RiskHandler.Instance.ProcessRisk(netMsg);
                }
                // TableOldHandler
                else if (netMsg.Topic.StartsWith(ApiConsts.Topic_Old_stc_table_iq)
                    || netMsg.Topic.StartsWith(ApiConsts.Topic_Old_stc_table_iqbiz))
                {
                    TableOldHandler.Instance.ProcessIntention(netMsg);
                }
                else if (netMsg.Topic.StartsWith(ApiConsts.Topic_Old_stc_table_ir))
                {
                    TableOldHandler.Instance.ProcessRecognition(netMsg);
                }
                else if (netMsg.Topic == ApiConsts.Topic_Old_QOR)
                {
                    TableOldHandler.Instance.ProcessQor(netMsg);
                }
                else if (netMsg.Topic == ApiConsts.Topic_Old_QOR_IRS)
                {
                    TableOldHandler.Instance.ProcessQor_Irs(netMsg);
                }
                // TableHandler
                else if (netMsg.Topic.StartsWith(ApiConsts.Topic_IntentionExec))
                {
                    TableHandler.Instance.ProcessIntentionExec(netMsg);
                }
                else if (netMsg.Topic.StartsWith(ApiConsts.Topic_IntentionOrder))
                {
                    TableHandler.Instance.ProcessIntentionOrder(netMsg);
                }
                else if (netMsg.Topic.StartsWith(ApiConsts.Topic_stc_table_ir))
                {
                    TableHandler.Instance.ProcessRecognition(netMsg);
                }
                else if (netMsg.Topic.StartsWith(ApiConsts.Topic_stc_table_iq))
                {
                    TableHandler.Instance.ProcessIntention(netMsg);
                }
                else if (netMsg.Topic.StartsWith(ApiConsts.Topic_stc_table_io))
                {
                    TableHandler.Instance.ProcessOrigin(netMsg);
                }
                // MonitorHandler
                else if (netMsg.Topic.StartsWith(ApiConsts.Topic_stc_MonitorMsginfo))
                {
                    MonitorHandler.Instance.ProcessMonitorMsgInfo(netMsg);
                }
                else if (netMsg.Topic.StartsWith(ApiConsts.Topic_stc_MonitorInfo))
                {
                    MonitorHandler.Instance.ProcessMonitorInfo(netMsg);
                }
                // PtrHandler
                else if (netMsg.Topic.StartsWith(ApiConsts.Topic_ptr_reltime_deal_prefix))
                {
                    PtrHandler.Instance.ProcessPtrOrder(netMsg);
                }
                else if (netMsg.Topic.StartsWith(ApiConsts.Topic_stc_RfqOrder_prefix))
                {
                    RfqHandler.Instance.ProcessRfqOrder(netMsg);
                }

                // GWHandler
                //PTSvr
                else if (netMsg.Topic.StartsWith(ApiConsts.Topic_stc_ptpeo))
                {
                    PTHandler.Instance.ProcessPTExec(netMsg);
                }
                else if (netMsg.Topic.StartsWith(ApiConsts.Topic_GrExecOrderSubscribe) || netMsg.Topic.StartsWith(ApiConsts.Topic_GrExecOtherOrderSubscribe))
                {
                    //Model.Fix.Message.ExecutionReport data = BaseHelper.ConvertStcMessage<Model.Fix.Message.ExecutionReport> (new string[] { netMsg.Topic }, netMsg);
                    //LogHelper.Info($"Topic:{netMsg.Topic} {Common.Utils.DateTimeEx.GetNow().ToString("yyyy-mm-dd HH:mm:ss.ffff")}");
                    GRHandler.Instance.ProcessGrExec(netMsg);
                }
                else if (netMsg.Topic.StartsWith(ApiConsts.Topic_BridgeReview))
                {
                    GRHandler.Instance.ProcessGrBridgeReview(netMsg);
                }
                else if (netMsg.Topic.StartsWith(ApiConsts.Topic_BridgeState))
                {
                    GRHandler.Instance.ProcessGrBridgeStateChange(netMsg);
                }
                else if (netMsg.Topic.StartsWith(ApiConsts.TOPIC_CRM_ACCOUNTNOTIFY) || netMsg.Topic.StartsWith(ApiConsts.TOPIC_CRM_CONTACTNOTIFY) || netMsg.Topic.StartsWith(ApiConsts.TOPIC_CRM_INSTNOTIFY))
                {
                    CRMHandler.Instance.ProcessCrmContactReplyModel(netMsg);
                }
                else if (netMsg.Topic.StartsWith(ApiConsts.Topic_stc_SubscribeProcessingRecordStatus))
                {
                    PositionHandler.Instance.ProcessProcessingRecordStatus(netMsg);
                }

                Common.Utils.AppUtils.Application.PluginManager.Application.BroadCast("GateWayNetMessage", netMsg);
            }
            catch (Exception e)
            {
                LogHelper.Error(e);
            }
        }
    }
}
