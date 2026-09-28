using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Api
{
    /// <summary>
    /// 数据接口常量类
    /// </summary>
    public class ApiConsts
    {
        public const string CODE_SUCCESS = "0000";
        public const string BRACKET = "[";

        public const string API_KEY_CEDA = "2";
        public const string API_KEY_GW = "1";

        public const string JSON = "JSON";

        public const string LOGINSVR = "LoginSvr";
        public const string queryAccount_Topic = "queryAccount";
        #region MDHist

        public const string MDHISTSVR = "MDHistSvr";
        public const string Topic_MDHist_KLine_Search = "stc.mdhist.h.q2";
        public const string Topic_MDHist_D = "stc.mdhist.h.d";
        public const string Topic_MDHist_D2 = "stc.mdhist.h.d2";
        public const string Topic_MDHist_D3 = "stc.mdhist.h.d3";
        public const string Topic_MDHist_D4 = "stc.mdhist.h.d4";
        public const string Topic_MDHist_D5 = "stc.mdhist.h.d5";
        public const string Topic_MDHist_D6 = "stc.mdhist.h.d6";
        #endregion

        #region GW

        public const string Topic_GW_Nofity = "notify";
        public const string GateWay = "gateway";

        #endregion

        #region PTRSvr

        public const string PTRSvr = "PTRSvr";
        public const string Topic_ptr_reltime_deal_prefix = "stc.ptrs.trade2.";
        public const string Topic_ptr_reltime_deal_all = "stc.ptrs.trade2.**";
        public const string Topic_ptr_search = "queryTrade";
        public const string Topic_ptr_updateTradeRemark = "updateTradeRemark";
        public const string Topic_ptr_Historysearch = "queryTradeHistory";
        public const string Topic_ptr_deal_ack = "stc.ptrs.trade3.ack";

        #endregion

        #region DpsSvr

        public const string STCLITESVR = "StcLiteSvr";
        public const string DPS_TOPIC_SIMULATE_ORDER = "stclite.s.o";
        public const string DPS_TOPIC_QQ_S = "stclite.qq.s";
        public const string Topic_dps_QqQuery = "QqQuery";

        public const string Topic_dps_qtr_netStatus = "stc.qtr.t.";
        public const string Topic_dps_qtr_quote = "stc.qtr.p.";
        public const string Topic_dps_qtr_order = "stc.qtr.order.";
        public const string Topic_dps_qtr_trade = "stc.qtr.o.";
        #endregion

        #region EMSSvr
        public const string EMSSVR = "EMSSvr";
        public const string Topic_stc_ems_execOrderQuery = "execOrderQuery";
        public const string Topic_stc_ems_orderQuery = "orderQuery";
        //请求
        public const string Topic_stc_ems_placeorder = "stc.ems.o.n";
        public const string Topic_stc_ems_cancelorder = "stc.ems.o.c";
        //订阅
        public const string Topic_stc_ems_executionreport_sdn = "stc.ems.e.sdn.**";//老期货行情
        public const string Topic_stc_ems_executionreport_c_sdn = "stc.ems.c.sdn.**";//新期货行情
        public const string Topic_stc_ems_executionreport_c_sdn_prefix = "stc.ems.c.sdn.";//新期货行情prefix
        public const string Topic_stc_ems_executionreport_cdh = "stc.ems.c.cdh.**";
        public const string Topic_stc_ems_executionreport_cmds = "stc.ems.c.cmds.odm.**";
        public const string Topic_stc_ems_executionreport_cmds_qdmesp = "stc.ems.c.cmds.qdmesp.**";
        public const string Topic_stc_ems_executionreport_cdh_prefix = "stc.ems.c.cdh.";
        public const string Topic_stc_ems_executionreport_cmds_prefix = "stc.ems.c.cmds.odm.";
        public const string Topic_stc_ems_executionreport_cmds_qdmesp_prefix = "stc.ems.c.cmds.qdmesp.";
        public const string Topic_stc_ems_executionreport_sse = "stc.ems.c.sse.**";
        public const string Topic_stc_ems_executionreport_sse_prefix = "stc.ems.c.sse.";
        public const string Topic_stc_ems_executionreport_ssf = "stc.ems.c.ssf.**";
        public const string Topic_stc_ems_executionreport_ssf_prefix = "stc.ems.c.ssf.";
        public const string Topic_stc_ems_executionreport_sze = "stc.ems.c.sze.**";
        public const string Topic_stc_ems_executionreport_sze_prefix = "stc.ems.c.sze.";
        public const string Topic_stc_ems_executionreport_szf = "stc.ems.c.szf.**";
        public const string Topic_stc_ems_executionreport_szf_prefix = "stc.ems.c.szf.";
        public const string Topic_stc_ems_executionreport_own = "stc.ems.e.odm.**";
        public const string Topic_stc_ems_executionreport_prefix = "stc.ems.e.";
        public const string Topic_stc_ems_all = "stc.ems.e.**";
        public const string Topic_stc_ems_order = "stc.ems.v.**";
        public const string Topic_stc_ems_order_prefix = "stc.ems.v.";
        public const string Topic_stc_ems_updateExecOrder = "updateExecOrder";

        public const string Topic_stc_calSettlementDate = "calSettlementDate";

        #endregion

        #region TBookSvr
        public const string TBookSvr = "TBookSvr";
        public const string Topic_stc_tbook_orderQuery = "orderQuery";
        public const string Topic_stc_tbook_xbond = "stc.tbook.mm.xbond";
        public const string Topic_stc_tbook_order = "stc.tbook.s.**";
        public const string Topic_stc_tbook_prefix = "stc.tbook.s.";
        public const string Topic_stc_tbook_cancel = "stc.tbook.o.c";
        public const string Topic_stc_tbook_cancel_batch = "stc.tbook.o.m";
        public const string Topic_stc_tbook_placeOrder = "stc.tbook.o.n";
        public const string Topic_stc_tbook_conStatus = "stc.tbook.t.*";
        public const string Topic_stc_tbook_con_prefix = "stc.tbook.t.";
        public const string Topic_stc_tbook_update = "stc.tbook.o.r";

        public const string SORSvrName = "SOR";
        public const string CFETSTradeSvrName = "QDM,NDM";
        public const string XBondSvrName = "ODM";
        public const string CDHSvrName = "CDH";
        public const string Price = "PRICE";
        public const string Trade = "TRADE";
        public const string Inner = "INNER";
        public const string Topic_stc_tbook_system_param_prefix = "stc.tbook.systemparas";
        public const string Topic_stc_tbook_system_param = "stc.tbook.systemparas.*";
        public const string Topic_stc_tbook_system_param_all = "stc.tbook.systemparas.all";
        public const string Topic_systemParasUpdate = "systemParasUpdate";

        #region TableSvr
        public const string TableSvr = "TableSvr";
        public const string Topic_RecognizeIntentionQuote = "stc.table.parser";//"recognizeIntentionQuote";
        public const string Topic_QueryIntentionQuote = "stc.table.quote";//"queryIntentionQuote";
        public const string Topic_ConfirmIntentionQuote = "stc.table.process";//"confirmIntentionQuote";
        public const string Topic_UpdateIntentionQuote = "stc.table.update";//"updateIntentionQuote";
        public const string Topic_stc_table_iq = "stc.table.iq.";
        public const string Topic_stc_table_ir = "stc.table.ir.";
        public const string Topic_DeleteIntentionQuote = "deleteIntentionQuote";
        public const string Topic_InsertIntentionQuote = "stc.table.confirm";//"insertIntentionQuote";
        public const string Topic_QueryIntentionOrder = "stc.table.order";// 查询意向订单
        public const string Topic_IntentionOrder = "stc.table.s.";//订阅意向订单消息
        public const string Topic_QueryIntentionExec = "stc.table.exec";// 查询意向成交
        public const string Topic_IntentionExec = "stc.table.e.";// 订阅意向成交数据
        public const string Topic_relate = "stc.table.relate";// 关联意向成交数据入库
        public const string Topic_QueryOriginRequest = "stc.table.origin";//查询原始请求
        public const string Topic_stc_table_io = "stc.table.io.";
        public const string Topic_stc_QueryEmsOrder = "stc.table.queryEmsOrder";// 查询内部订单
        public const string Topic_stc_QueryRoles = "stc.table.queryRoles";//查询可交易角色
        public const string Topic_stc_QuoteTagGroup = "stc.table.quoteTagGroup";//内部看板筛选组接口
        
        public const string Topic_stc_QueryEmsOrder_Irs = "stc.table.queryEmsOrder.irs";// 查询irs内部订单

        public const string Topic_publishRecognizeIntentionQuote = "stc.table.publishRecognizeIntentionQuote";
        public const string Topic_publishIntentionQuote = "stc.table.publishIntentionQuote";

        public const string Topic_stc_TableUnrefOrder = "stc.table.unref.order"; //UNREF前一工作日订单
        public const string Topic_stc_QueryUnrefParam = "stc.table.sys.param.query";// 日初订单恢复提醒个人系统设置查询
        public const string Topic_stc_UpdateUnrefParam = "stc.table.sys.param.update";// 日初订单恢复提醒个人系统设置更新
        public const string Topic_stc_brokerQuery = "stc.table.broker.quote.query";
        public const string Topic_stc_brokerWhiteList = "stc.table.brokerWhiteList";

        public const string Topic_stc_Intention_Inquiry = "stc.table.intention.inquiry";// 增删改查意向报价
        public const string Topic_stc_table_inq = "stc.table.inq.";//stc.table.inq.{biztypeId}.{userName}推送询价消息
        public const string Topic_stc_table_queryIntentionMvt = "stc.table.queryIntentionMvt";//买方机构, 查询日志
        public const string Topic_stc_table_intention_execution_associate = "stc.table.intention.execution.associate";//关联成交
        public const string Topic_stc_table_queryEmsOrderHist = "stc.table.queryEmsOrderHist";// 内部看板历史查询
        #endregion

        #region old TableSvr
        public const string Topic_Old_RecognizeIntentionQuote = "recognizeIntentionQuote";
        public const string Topic_Old_QueryIntentionQuote = "queryIntentionQuote";
        public const string Topic_Old_ConfirmIntentionQuote = "confirmIntentionQuote";
        public const string Topic_Old_UpdateIntentionQuote = "updateIntentionQuote";
        public const string Topic_Old_stc_table_iq = "stc.table.iq.";
        public const string Topic_Old_stc_table_iqbiz = "stc.table.iqbiz.";
        public const string Topic_Old_stc_table_ir = "stc.table.ir.";
        public const string Topic_Old_DeleteIntentionQuote = "deleteIntentionQuote";
        public const string Topic_Old_InsertIntentionQuote = "insertIntentionQuote";
        public const string Topic_Old_QOR = "stc.table.qor";
        public const string Topic_Old_QOR_IRS = "stc.table.qor.irs";
        public const string Topic_batchPublishIntentionQuote = "stc.table.batchPublishIntentionQuote";//批量发内部
        public const string Topic_InsertIntentionQuote_new = "stc.table.create.intention.synchronously";
        #endregion

        #endregion

        #region MDSvr
        public const string SOR = "SOR";
        public const string CT = "CT";
        public const string H0 = "h0";
        public const string MDSvr = "MDSvr";
        public const string Topic_stc_md_prefix = "stc.md.";
        public const string Topic_stc_md_prefix_best = "stc.md.h0";
        public const string Topic_stc_md_prefix_ct = "stc.md.ct";
        public const string Topic_stc_md_securityidQuery = "mdSecurityIdQuery";
        public const string Topic_stc_md_prefix_n = "stc.md.p.n";
        public const string Topic_stc_md_sdn_all = "stc.md.ct.**";
        #endregion

        #region BaseSvr

        public const string BaseSvr = "BaseSvr";
        public const string Topic_stc_base_search = "stc.base.search";
        public const string Topic_stc_base_clear = "stc.base.clear";
        public const string Topic_stc_queryPartyDealerInfo = "queryPartyDealerInfo";
        public const string Topic_stc_base_inst_search = "stc.base.PartyInfo";
        public const string Topic_stc_queryChianSecurity = "queryChianSecurity";
        public const string Topic_stc_QuoteTag = "stc.base.tags.find";//获取内部看板中标签的分类
        public const string Topic_stc_Tags_Count = "stc.base.tags.count";// 获取下tags数量
        public const string Topic_stc_Tags_Load = "stc.base.tags.load";//获取tags数据
        public const string Topic_stc_base_TradeDate = "stc.base.tradeDate";// 获得交易日期
        #endregion

        #region AdminSvr
        public const string AdminSvr = "AdminSvr";
        public const string Topic_stc_admin_msg = "stc.admin.message";

        public const string Topic_stc_admin_search = "stc.sc.a.s";
        public const string Topic_stc_admin_searchmvt = "stc.sc.a.smvt";
        public const string Topic_stc_admin_update = "stc.sc.a.u";
        public const string Topic_stc_admin_prefix = "stc.sc.sysMessage.";
        public const string Topic_stc_usersQuery = "usersQuery";
        public const string Topic_stc_rolesQuery = "rolesQuery";
        public const string Topic_stc_roleUpdate = "roleUpdate";
        public const string Topic_stc_modulesQuery = "modulesQuery";
        public const string Topic_stc_modulesActionQuery = "modulesActionQuery";
        public const string Topic_stc_rolesModulesActionUpdate = "rolesModulesActionUpdate";
        public const string Topic_stc_roleModulesActionQuery = "roleModulesActionQuery";
        public const string Topic_stc_userQuery = "accountInfoQuery.enable";
        public const string Topic_stc_userRoleUpdate = "userRoleUpdate";
        public const string Topic_stc_homeGroupUpdate = "homeGroupUpdate";
        public const string Topic_stc_homeGroupQuery = "queryHomeGroup";
        public const string Topic_stc_homeGroupSecurityIdsUpdate = "homeSecurityidsUpdate";
        public const string Topic_stc_homeGroupSecurityIdsQuery = "homeGroupSecurityidsQuery";
        public const string Topic_stc_homeGroupUpdate2 = "homeGroupUpdate2";
        public const string Topic_stc_homeGroupQuery2 = "queryHomeGroup2";
        public const string Topic_stc_homeGroupSecurityIdsUpdate2 = "homeSecurityidsUpdate2";
        public const string Topic_stc_homeGroupSecurityIdsQuery2 = "homeGroupSecurityidsQuery2";
        public const string Topic_stc_userAndGroupsQuery = "accountAndGroupsQuery";
        public const string Topic_stc_admin_updateUser = "stc.admin.updateUser";
        public const string Topic_stc_safe = "stc.admin.safe";
        public const string Topic_stc_queryAccount = "accountQuery";
        public const string Topic_stc_queryAccountAndUsers = "accountAndUsersQuery";
        public const string Topic_stc_systemPara = "stc.admin.systemPara";
        public const string Topic_stc_updateSystemGlobalPara = "stc.admin.global.updateSystemGlobalPara";// 增删改
        public const string Topic_stc_queryPosition = "queryPosition";
        public const string Topic_stc_queryTrade = "queryTrade";
        public const string Topic_stc_queryUserGroup = "stc.admin.userGroup.queryUserGroup";
        public const string Topic_stc_admin_managerUser_queryManagerUser = "stc.admin.managerUser.queryManagerUser";

        public const string Topic_DB_Log = "stc.sc.a.l.i";
        #endregion

        #region RMQSvr
        public const string RMQSvr = "RMQSvr";
        public const string Topic_stc_BlockQuery = "blockQuery";
        public const string Topic_stc_BlockNumQuery = "blockNumQuery";
        public const string Topic_stc_BlockUpdate = "blockUpdate";
        public const string Topic_stc_BlockDelete = "blockDelete";
        public const string Topic_stc_BlockReSend = "blockReSend";
        public const string Topic_stc_SorExecAckQuery = "sorExecAckQuery";

        public const string Topic_stc_Block = "stc.rmq.blocknum.execorders";
        #endregion

        #region common
        public const string CDH = "CDH";
        public const string CMDS = "CMDS";
        public const string SDN = "SDN";
        public const string SSE = "SSE";
        public const string SSF = "SSF";
        public const string SZE = "SZE";
        public const string SZF = "SZF";
        public const string EXC = "EXC";
        #endregion

        #region CalSvr
        public const string CalSvr = "CalSvr";
        public const string Topic_stc_calbond = "stc.broker.calbond";
        /// <summary>
        /// Curve计算器计算接口
        /// </summary>
        public const string Topic_stc_calCurve = "calCurve";
        #endregion
        #region RiskSvr
        public const string RiskSvr = "RiskSvr";
        public const string Topic_stc_riskParaQuery = "queryRiskParas";
        public const string Topic_stc_riskParaUpdate = "updateRiskParas";

        public const string Topic_stc_risk_prefix = "stc.risk.r.";
        public const string Topic_stc_riskLogprefix = "queryRiskLog";

        public const string Topic_RiskSvr_BatchCheckRisk = "stc.risk.batchCheckRisk";

        #endregion

        #region BlackList

        public const string Topic_stc_BlackList_SetRisFilter = "setRisFilter";
        public const string Topic_stc_BlackList_QueryRisFilter = "queryRisFilter";
        public const string Topic_stc_BlackList_QueryRisBlack = "queryRisBlack";

        #endregion

        #region Inner

        public const string InnerSvr = "INNERSvr";
        public const string Topic_stc_inner_order = "stc.inner.canal";
        public const string Topic_stc_inner_queryInnerOrder = "queryInnerOrder";
        public const string Topic_stc_inner_transaction = "transaction";
        public const string Topic_stc_inner_cancelOrder = "cancelOrder";
        public const string Topic_stc_inner_fakTransaction = "fakTransaction";
        public const string Topic_stc_inner_fakCancel = "fakCancel";
        /// <summary>
        /// 定向回价 议价、拒绝
        /// </summary>
        public const string Topic_stc_inner_DRPOrder = "stc.inner.o.c";
        /// <summary>
        /// 定向回价 确认
        /// </summary>
        public const string Topic_stc_inner_PlaceOrder = "stc.inner.o.n";
        #endregion

        #region MMSvr

        public const string MMSvr = "MMSvr";
        public const string Topic_stc_updateMMGroup = "stc.mm.mmGroup";
        public const string Topic_stc_queryMMGroup = "stc.mm.queryMMGroup";
        public const string Topic_stc_updateMM = "stc.mm.mm";
        public const string Topic_stc_mm = "stc.mm.mm.*.{0}.*";
        public const string Topic_stc_mm_regex = "stc.mm.mm";
        public const string Topic_stc_mm_sysStatus = "stc.mm.{0}.sysStatus";
        public const string Topic_stc_mm_sysStatus_regex = "stc.mm.(.+).sysStatus";
        public const string Topic_stc_updateParams = "stc.mm.bond.updateParas";
        public const string Topic_stc_updateSecurityCdc = "stc.mm.bond.updateSecurityCdc";
        public const string Topic_stc_updateEnable = "stc.mm.bond.updateEnable";
        public const string Topic_stc_queryMMUserSetting = "stc.mm.user.queryMMUserSetting";
        public const string Topic_stc_updateMMUserSetting = "stc.mm.user.updateMMUserSetting";

        public const string Topic_stc_updateAllEnable = "stc.mm.bond.updateAllEnable";
        public const string Topic_stc_updateWidenSetting = "stc.mm.bond.updateWidenSetting";
        public const string Topic_stc_cancelWidenSetting = "stc.mm.bond.cancelWidenSetting";
        public const string Topic_stc_queryWidenSetting = "stc.mm.bond.QueryWidenSetting";
        public const string Topic_stc_checkSecurityUser = "stc.mm.bond.checkSecurityUser";
        public const string Topic_stc_querySingleBondSetting = "stc.mm.bond.querySingleBondSetting";

        public const string Topic_stc_QueryBondBalance = "stc.mm.add.queryBondBalance";
        public const string Topic_stc_QueryRiskQty = "stc.mm.add.queryRiskQty";
        public const string Topic_stc_mm_FilterQuote = "stc.mm.FilterQuote.updateFilterQuote";
        public const string Topic_stc_mm_QueryDealCloseEnable = "stc.mm.systemparas.dealCloseEnable";
        public const string Topic_stc_QueryMMSecurityidHist = "queryMMSecurityidHist";// 查询策略日志
        public const string Topic_stc_HandleUnifyGroup = "handleUnifyGroup";//整组开关

        #endregion

        #region ANASvr  做市义务
        public const string AnsSvr = "AnsSvr";
        public const string Topic_stc_ans_monitor_total = "stc.ans.monitor.total";//AnsSummary
        public const string Topic_stc_ans_username_detail = "stc.ans.monitor.{0}.detail";//做市业务
        public const string Topic_stc_ans_username_detail_regex = "stc.ans.monitor.(.+).detail";
        public const string Topic_stc_ans_queryANSDetail = "queryANSDetail";
        public const string Topic_stc_ans_queryANSOutline = "queryANSOutline";
        public const string Topic_stc_ans_ignoreBreak = "stc.ans.common.ignoreBreak";

        #endregion

        #region QuantSvr
        public const string QuantSvr = "QuantSvr"; 
        public const string Topic_stc_IUDquantGroup = "quantGroup";
        public const string Topic_stc_queryGroups = "queryGroups";
        public const string Topic_stc_Strategy = "stc.quant.status.";
        public const string Topic_stc_IUDquantStrategy = "quantStrategy";
        public const string Topic_stc_IUDquantParas = "quantParas";
        public const string Topic_stc_UpdateEnable = "updateEnable";//策略启停接口
        public const string Topic_stc_Paras = "stc.quant.paras.";//订阅策略参数
        public const string Topic_stc_ParasCheck = "stc.quant.paras.check.";//策略参数校验
        
        public const string Topic_stc_QuantLog = "stc.quant.logs.";
        public const string Topic_stc_SelQuantLog = "queryLog";
        public const string Topic_stc_Seltemplet = "querytemplet";
        public const string Topic_stc_SelIndicator = "queryindicator";
        public const string Topic_stc_Indicator = "stc.quant.indicator.";//订阅策略指标
        public const string Topic_stc_SelQuantStrategys = "quantAllStrategys";
        public const string Topic_stc_disableStrategys = "disableStrategys";//全撤全停
        
        #endregion

        #region Permission

        /// <summary>
        /// 渠道权限
        /// </summary>
        public const string MODULEID = "1002";
        /// <summary>
        /// 所有权限
        /// </summary>
        public const string CHECK_ALL_ACTIONID = "100203";
        /// <summary>
        /// 仅内部
        /// </summary>
        public const string CHECK_INNER_ACTIONID = "100201";
        /// <summary>
        /// 内部，X-Bond
        /// </summary>
        public const string CHECK_INNER_XBOND_ACTIONID = "100202";

        #endregion

        #region QTRS Test
        public const string QTRSSvr = "QTRSSvr";
        public const string Topic_stc_qtrs_order = "stc.qtrs.order";
        public const string Topic_stc_qtrs_di = "stc.qtrs.demandintention";
        #endregion

        #region OHLCSvr
        public const string OHLCSvr = "OHLCSvr";
        public const string Topic_stc_OHLC = "stc.ohlc.ems.e";
        public const string Topic_stc_OHLC_All = "stc.ohlc.CDH.**";
        public const string Topic_stc_OHLC_KLine = "stc.ohlc.CDH.{0}.{1}.{2}";
        public const string Topic_stc_OHLC_KLine_prefix = "stc.ohlc.CDH";
        #endregion


        #region CRMSvr
        public const string CRMSvr = "CRMSvr";
        public const string TOPIC_CRM_INST = "stc.crm.inst.query";
        public const string TOPIC_CRM_CONTACT = "stc.crm.contact.query";
        public const string TOPIC_BROKER_SEARCH = "stc.crm.account.query";
        public const string TOPIC_CRM_CONTACTDB = "stc.crm.contact.querydb";

        //Crm账号
        public const string TOPIC_CRM_ACCOUNTNOTIFY = "stc.crm.account.notify";
        //Crm交易员
        public const string TOPIC_CRM_CONTACTNOTIFY = "stc.crm.contact.notify";
        //Crm机构
        public const string TOPIC_CRM_INSTNOTIFY = "stc.crm.inst.notify";
        #endregion

        #region PriceWinSvr
        public const string LocalSvr = "LocalSvr";
        public const string Topic_PriceWin = "priceWin";
        public const string Topic_Clipboard = "clipboard";
        #endregion

        #region MMsheSvr

        public const string MMSHESvr = "MMSHESvr";
        public const string Topic_stc_updateMMsheGroup = "stc.mmshe.mmGroup";
        public const string Topic_stc_queryMMsheGroup = "stc.mmshe.queryMMGroup";
        public const string Topic_stc_updateMMshe = "stc.mmshe.mm";
        public const string Topic_stc_mmshe = "stc.mmshe.mm.*.{0}";
        public const string Topic_stc_mmshe_regex = "stc.mmshe.mm";
        public const string Topic_stc_mmshe_sysStatus = "stc.mmshe.{0}.sysStatus";
        public const string Topic_stc_mmshe_sysStatus_regex = "stc.mmshe.(.+).sysStatus";
        public const string Topic_stc_mmshe_updateParams = "stc.mmshe.bond.updateParas";
        public const string Topic_stc_mmshe_updateSecurityCdc = "stc.mmshe.bond.updateSecurityCdc";
        public const string Topic_stc_mmshe_updateEnable = "stc.mmshe.bond.updateEnable";
        public const string Topic_stc_mmshe_queryMMUserSetting = "stc.mmshe.user.queryMMUserSetting";
        public const string Topic_stc_mmshe_updateMMUserSetting = "stc.mmshe.user.updateMMUserSetting";

        public const string Topic_stc_mmshe_updateAllEnable = "stc.mmshe.bond.updateAllEnable";
        public const string Topic_stc_mmshe_updateWidenSetting = "stc.mmshe.bond.updateWidenSetting";
        public const string Topic_stc_mmshe_cancelWidenSetting = "stc.mmshe.bond.cancelWidenSetting";
        public const string Topic_stc_mmshe_queryWidenSetting = "stc.mmshe.bond.QueryWidenSetting";
        public const string Topic_stc_mmshe_checkSecurityUser = "stc.mmshe.bond.checkSecurityUser";
        public const string Topic_stc_mmshe_querySingleBondSetting = "stc.mmshe.bond.querySingleBondSetting";

        public const string Topic_stc_mmshe_QueryBondBalance = "stc.mmshe.add.queryBondBalance";
        public const string Topic_stc_mmshe_QueryRiskQty = "stc.mmshe.add.queryRiskQty";
        public const string Topic_stc_mmshe_FilterQuote = "stc.mmshe.FilterQuote.updateFilterQuote";
        #endregion

        #region PositionSvr
        public const string PositionSvr = "PositionSvr";
        public const string Topic_stc_PostionGroup = "stc.position.positionGroup";
        /// <summary>
        /// stc.position.tod.{SecurityId}.{BookId}
        /// </summary>
        public const string Topic_stc_PostionDetail = "stc.position.tod";
        public const string Topic_stc_PostionDetail_regex = "stc.position.tod.*.*";
        public const string Topic_stc_ManualInputOrder = "stc.position.o.n";
        public const string Topic_stc_HistoryPositionGroup = "queryPositionMvt";
        public const string Topic_stc_QueryPosition = "queryPosition";

        public const string Topic_stc_TradeStatistics = "stc.position.statistic";
        public const string Topic_stc_TradeStatistics_regex = "stc.position.statistic.*.*";
        #endregion

        #region HedgeSvr
        //风险敞口计算器
        public const string HedgeSvr = "HedgeSvr";
        public const string Topic_stc_RiskExposureCalculate = "stc.hedge.calculate";
        public const string Topic_stc_RiskExposure_regex = "stc.hedge.portfolio.";
        public const string Topic_stc_PositionSimExec = "positionSimExecHandler";
        #endregion

        #region PositionBook
        public const string BookSvr = "AdminSvr";
        public const string Topic_stc_PositionBook = "stc.admin.bookUser.queryBookUserByUser";
        public const string Topic_stc_QueryBook = "stc.admin.bookUser.queryBookUser";
        public const string Topic_stc_QueryUser = "stc.admin.bookUser.queryUser";
        public const string Topic_stc_ManualOrder = "stc.position.o.n";
        public const string Topic_stc_QueryExternal = "queryExternal";
        public const string Topic_stc_UpdatePosition = "updatePosition";
        public const string Topic_stc_ManualProcessingPosition = "manualProcessingPosition";
        public const string Topic_stc_QueryProcessingRecordPosition = "queryProcessingRecord";
        public const string Topic_stc_QueryProcessingRecordHistPosition = "queryProcessingRecordHist";
        public const string Topic_stc_SubscribeProcessingRecordStatus = "stc.position.record";
        public const string Topic_stc_QueryBookUserByExternal = "stc.admin.bookUser.queryBookUserByExternal";

        #endregion

        #region Monitor

        public const string MonitorSvr = "MonitorSvr";
        public const string Topic_stc_MonitorInfo = "stc.monitor.info";
        public const string Topic_stc_MonitorMsginfo = "stc.monitor.msginfo";
        public const string Topic_stc_MonitorSvrInfo = "stc.operation.*";
        #endregion
		
		#region CppStrategy C++策略启停
        public const string QuantSvr_cpp = "QuantSvr";
        public const string Topic_stc_IUDquantGroup_cpp = "quantGroup";
        public const string Topic_stc_queryGroups_cpp = "queryGroups";
        public const string Topic_stc_Strategy_cpp = "stc.quant.status.";
        public const string Topic_stc_IUDquantStrategy_cpp = "quantStrategy";
        public const string Topic_stc_IUDquantParas_cpp = "quantParas";
        public const string Topic_stc_UpdateEnable_cpp = "updateEnable";//策略启停接口
        public const string Topic_stc_Paras_cpp = "stc.quant.paras.";//订阅策略参数
        public const string Topic_stc_QuantLog_cpp = "stc.quant.logs.";
        public const string Topic_stc_SelQuantLog_cpp = "queryLog";
        public const string Topic_stc_Seltemplet_cpp = "querytemplet";
        public const string Topic_stc_SelIndicator_cpp = "queryindicator";
        public const string Topic_stc_Indicator_cpp = "stc.quant.indicator.";//订阅策略指标
        public const string Topic_stc_SelQuantStrategys_cpp = "quantAllStrategys";
        public const string Topic_stc_disableStrategys_cpp = "disableStrategys";//全撤全停

        #endregion


        #region RFQSvr
        public const string RFQSvr = "RFQSvr";
        public const string RFQMSvr = "RFQMSvr";
        /// <summary>
        /// 询价请求接口
        /// </summary>
        public const string Topic_stc_RfqQuoteRequest = "quoteRequest";
        public const string Topic_stc_RfqOrder = "stc.rfq.maker.order";
        public const string Topic_stc_RfqOrder_prefix = "stc.rfq.";
        public const string Topic_stc_RfqOrder_ChangeAccept = "quote";          //(改，回) QuoteStatus 都是 33  区别是 QuoteID有无 QuoteStatus 撤40，拒38
        public const string Topic_stc_RfqOrder_CancelReject = "quoteStatusReport"; //废弃
        /// <summary>
        ///	询价查询接口
        /// </summary>
        public const string Topic_stc_RfqQueryOrder = "rfqOrder";

        /// <summary>
        /// Rfq策略分组  RFQMSvr
        /// </summary>
        public const string Topic_stc_updateRfqGroup = "stc.rfqm.rfqmGroup";
        public const string Topic_stc_queryRfqGroup = "stc.rfqm.rfqmGroup";

        /// <summary>
        /// 分组策略查询  请求/响应/订阅  RFQMSvr 
        /// </summary>
        public const string Topic_stc_RfqstgView = "stc.rfqm.stgView";
        public const string Topic_stc_RfqstgView_Subscribe = "stc.rfqm.stgView.{0}";
        /// <summary>
        /// 策略接口 策略新增、删除、修改、启动、停止  方式:请求/响应  RFQMSvr
        /// </summary>
        public const string Topic_stc_RfqstgAct = "stc.rfqm.stgAct";
        #endregion

        #region CDHSvr  
        public const string CDHSvr = "CDHSvr";
        public const string Topic_qm_EnterMarket = "stc.cdh.manualQuote";


        #endregion

        #region DQSvr Use AdminSvr
        public const string Topic_stc_queryDQOrderLog = "stc.admin.orderLog.queryCallBackLog";
        #endregion

        #region OHLCSvr

        public const string OHLCSVR = "OHLCSvr";
        public const string Topic_stc_ohlc_1D = "stc.ohlc.ems.e";

        #endregion

        #region ParserSvr
        public const string ParserSvr = "ParserSvr";
        public const string Topic_ParserIRS = "stc.parser.irs";//"stc.parser.irs"
        public const string Topic_ParserVirtual = "stc.parser.virtual";//"stc.parser.virtual"
        public const string Topic_ParserBond = "stc.parser.bond";// 内部看板批量识别
        public const string Topic_ParserImage = "stc.parser.image";//图片识别
        public const string Topic_ParserTrade = "stc.parser.trade";//成交识别
        #endregion

        #region PTSvr  
        public const string PTSvr = "PTSvr";
        public const string Topic_ptExecOrderQuery = "ptExecOrderQuery";
        public const string Topic_stc_ptpeo = "stc.pt.peo.";

        public const string Topic_ptExecOrderSend = "stc.pt.execOrderSend";
        public const string Topic_ptExecOrderSave = "ptExecOrderSave";
        public const string Topic_ptExecOrderBatchSave = "ptExecOrderBatchSave";
        public const string Topic_ptExecOrderUpdate = "ptExecOrderUpdate";
        public const string Topic_ptExecOrderDelete = "ptExecOrderDelete";
        public const string Topic_ptExecOrderBatchUpdate = "ptExecOrderBatchUpdate";
        public const string Topic_ptExecOrderUnSendQuery = "ptExecOrderUnSendQuery";
        public const string Topic_ptExecOrderApprovalHistQuery = "ptExecOrderApprovalHistQuery";
        public const string Topic_ptExecOrderMvtQuery = "ptExecOrderMvtQuery";
        public const string Topic_ptExecOrderRiskCheck = "stc.pt.riskcheck";

        /// <summary>
        /// 交易处理查询
        /// </summary>
        public const string Topic_GrExecOrderQuery = "execOrderQuery";

        /// <summary>
        /// 历史未过期交易处理查询
        /// </summary>
        public const string Topic_GrExecOrderHistQuery = "execOrderHistQuery";

        /// <summary>
        /// 交易流水查询
        /// </summary>
        public const string Topic_GrExecOrderHistRecordQuery = "execOrderMvtQuery";

        /// <summary>
        /// 交易处理更新
        /// </summary>
        public const string Topic_GrExecOrderUpdate = "execOrderUpdate";

        /// <summary>
        /// 克隆订单
        /// </summary>
        public const string Topic_OnExecOrderClone = "execOrderClone";

        /// <summary>
        /// 批量确认
        /// </summary>
        public const string Topic_OnBatchExecOrderClone = "execOrderBatchUpdate";

        /// <summary>
        /// 交易处理保存
        /// </summary>
        public const string Topic_GrexecOrderSave = "execOrderSave";

        /// <summary>
        /// 推市场成交
        /// </summary>
        public const string Topic_GrexecPushOrderSave = "execOrderSync";

        /// <summary>
        /// 交易处理订阅接口
        /// </summary>
        public const string Topic_GrExecOrderSubscribe = "stc.pt.gr.exec.report.";

        /// <summary>
        /// 交易处理订阅接口
        /// </summary>
        public const string Topic_GrExecOtherOrderSubscribe = "stc.pt.gr.bridge.exec.report.";

        /// <summary>
        /// 交易处理订阅接口
        /// </summary>
        public const string Topic_GrUserBridge = "userBridge";

        /// <summary>
        /// 删除定单查询
        /// </summary>
        public const string Topic_GrExecOrderDelQuery = "execOrderDelQuery";

        /// <summary>
        /// 机构组管理
        /// </summary>
        public const string Topic_GrInstGroups = "instGroups";

        /// <summary>
        /// 机构设置
        /// </summary>
        public const string Topic_GrinstInfo = "instInfo";

        /// <summary>
        /// 代付机构设置
        /// </summary>
        public const string Topic_InstProxySetting = "instProxySetting";

        /// <summary>
        /// 代付机构费用设置
        /// </summary>
        public const string Topic_InstProxyFeeSetting = "instProxyFeeSetting";

        /// <summary>
        /// 关联机构接口
        /// </summary>
        public const string Topic_InstInstAssociated = "instAssociated";

        /// <summary>
        /// 权限设置接口
        /// </summary>
        public const string Topic_UserRoleSetting = "userRoleSetting";

        /// <summary>
        /// 发送QQ消息
        /// </summary>
        public const string Topic_SendQQMsg = "sendQQMsg";

        /// <summary>
        /// 查询订单代付费
        /// </summary>
        public const string Topic_GrExecIssueProxyInfoQuery = "instProxyFeeSetting";

        /// <summary>
        /// 查询订单清算速度是否修改
        /// </summary>
        public const string Topic_GrExecSettInfoQuery = "settltypeUpdateRecord";

        /// <summary>
        /// 查询用户权限
        /// </summary>
        public const string Topic_GrExecSystemPowerQuery = "userRoleSetting";

        /// <summary>
        /// 其他人桥订单
        /// </summary>
        public const string Topic_GrExecOtherOrderQuery = "queryExecOrderCache";

        /// <summary>
        /// 桥流水
        /// </summary>
        public const string Topic_BridgeMvtQuery = "bridgeMvtQuery";
        /// <summary>
        /// 桥查看更新
        /// </summary>
        public const string Topic_BridgeReview = "stc.pt.gr.bridge.";
        /// <summary>
        /// 桥状态更新
        /// </summary>
        public const string Topic_BridgeState = "stc.pt.gr.bridgeState.";

        /// <summary>
        /// 查询桥查看——某个桥
        /// </summary>
        public const string Topic_BridgeReviewsQuery = "bridgeReviewsQuery";

        /// <summary>
        /// 点击查看接口
        /// </summary>
        public const string Topic_BridgeReviewState = "bridgeReviewState";
        #endregion

        #region limitSvr
        public const string LimitSvr = "LimitSvr";
        public const string Topic_stc_limit_summary = "stc.limit.summary";
        public const string Topic_stc_limit_details = "stc.limit.details";
        #endregion

        #region FinQSvr
        public const string FinQSvr = "FinQSvr";
        public const string Topic_stc_finq_sendmessage = "stc.finq.sendmessage";
        public const string sendDesktopFinQ_Topic = "stc.finq.sendDesktopFinQ";
        public const string Topic_stc_finq_getUserConfig = "stc.finq.getUserConfig";
        public const string Topic_stc_find_sendGroupMessage = "stc.finq.sendGroupMessage";
        public const string Topic_stc_finq_getServerToken = "stc.finq.serverToken";
        #endregion

        #region QQSvr
        public const string QQSvr = "QQSvr";
        public const string sendFinQFriend_Topic = "sendFinQFriend";
        #endregion

        #region GLADAPTERSvr
        public const string GLADAPTERSvr = "GLADAPTERSvr";
        public const string Topic_stc_syncPosAndTrd = "stc.gladapter.SyncPosAndTrdHandler";
        public const string Topic_stc_blackWhiteBroadcast = "stc.gladapter.blackWhiteBroadcast";
        #endregion

        #region CustSvr
        public const string CustSvr = "CustSvr";
        public const string Topic_stc_cust_md_query = "stc.cust.md.query";
        #endregion

        #region OptimSvr
        public const string OptimSvr = "OptimSvr";
        /// <summary>
        /// 查询债券持仓
        /// </summary>
        public const string Topic_OptimSvr_QueryBondPosition = "stc.optim.QueryBondPosition";
        /// <summary>
        /// 查询质押券持仓
        /// </summary>
        public const string Topic_OptimSvr__QueryPledgePosition = "stc.optim.QueryPledgePosition";

        /// <summary>
        /// 风控接口 
        /// </summary>
        public const string Topic_OptimSvr_checkRisk = "stc.optim.checkRisk";

        /// <summary>
        /// 批量风控接口
        /// </summary>
        public const string Topic_OptimSvr_batchcheckRisk = "stc.optim.batchCheckRisk";

        /// <summary>
        /// 查询绩效经理
        /// </summary>
        public const string Topic_OptimSvr_queryPortfolio = "stc.optim.queryPortfolio";

        #endregion

    }
}
