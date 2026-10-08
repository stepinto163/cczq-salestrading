// Recovered compatibility source; provenance: recovered-support.json.
package sc.app.stc.utils;

import ats.foundation.utils.config.ATSROOTConfig;
import ats.foundation.utils.config.PropertiesHelper;

import org.apache.commons.lang3.StringUtils;

import sc.app.stc.po.system.MsgTag;

import java.nio.charset.Charset;
import java.util.HashMap;
import java.util.Hashtable;
import java.util.Map;

public class Consts {
    public static int DBType = 1;
    public static String NetType = "";
    public static boolean UseMonitor = false;
    public static boolean MonitorInterface = false;
    public static String priceTopic = "stc.md.";
    public static String ServerVERSION = "v1.0.0.0 (build20180108)";
    public static String venueGwAll = "*";
    public static String venueAll = "*";
    public static String STC_TBook = "SERVER.TBookSvr";
    public static String STC_Risk = "SERVER.RiskSvr";
    public static String STC_Base = "SERVER.BaseSvr";
    public static String STC_Cal = "SERVER.CalSvr";
    public static String STC_EMS = "SERVER.EMSSvr";
    public static String STC_MD = "SERVER.MDSvr";
    public static String STC_SOR = "SERVER.SORSvr";
    public static String STC_RFQ = "SERVER.RFQSvr";
    public static String STC_RFQM = "SERVER.RFQMSvr";
    public static String STC_PTRSvr = "SERVER.PTRSvr";
    public static String STC_FinQSvr = "SERVER.FinQSvr";
    public static String STC_CustSvr = "SERVER.CustSvr";
    public static String STC_Cust2Svr = "SERVER.Cust2Svr";
    public static String STC_RFQ_QuoteRequest = "quoteRequest";
    public static String STC_RFQ_QuoteStatusReport = "quoteStatusReport";
    public static String STC_RFQ_Quote = "quote";
    public static String STC_RFQ_QuoteResponse = "quoteResponse";
    public static String STC_MDHIST = "SERVER.MDHistSvr";
    public static String SXZQ_EMS = "SERVER.ExecutionManagerSvr";
    public static final String stc_venue_setting = "STC_Venues_Setting";
    public static String RMB_MD = "SERVER.RmbMdConnector";
    public static String RMB_Depth = "stc.bond.cfets.md.depth.*";
    public static String RMB_ConnStaus = "stc.bond.cfets.session.status";
    public static String RMB_MDSvrID = "ExecutionManagerSvr";
    public static final String STC_MD_MarketDataRequest = "stc.md.p.n";
    public static final String STC_TBook_MMSecurityIds = "stc.tbook.mm.xbond";
    public static String LoadLogonCron = "";
    public static String LoadLogoutCron = "";
    public static String STC_Risk_checkRisk = "checkRisk";
    public static String STC_Risk_riskCheckSub = "stc.riskcheck.%s";
    public static String STC_Admin = "SERVER.AdminSvr";
    public static String STC_Table = "SERVER.TableSvr";
    public static String STC_Parser = "SERVER.ParserSvr";
    public static String STC_Cal_calBond = "stc.broker.calbond";
    public static String STC_Admin_systemMessage = "stc.sc.a.i";
    public static String STC_Admin_systemMessageAll = "stc.sc.sysMessage.all";
    public static String STC_Admin_SystemAuditLogInsert = "stc.sc.a.l.i";
    public static String STC_TABLE_INNER_MD = "stc.table.inner.md";
    public static String STC_PT = "SERVER.PTSvr";
    public static String STC_PT_execOrderSave = "execOrderSave";
    public static String STC_Table_publishIntentionQuote = "stc.table.publishIntentionQuote";
    public static String STC_Table_placeBrokerQuote = "stc.table.place.broker.quote";
    public static String STC_Table_quoteSignQuery = "stc.table.quote.sign.query";
    public static String STC_Base_querySecurity = "querySecurity";
    public static final String STC_Base_Holiday = "stc.base.allholiday";
    public static String STC_TBook_placeOrder = "stc.tbook.o.n";
    public static String STC_TBook_cancelAllOrder = "stc.tbook.o.m";
    public static String STC_TBook_replaceOrder = "stc.tbook.o.r";
    public static String STC_TBook_queryOrder = "orderQuery";
    public static String STC_EMS_queryOrder = "orderQuery";
    public static String STC_EMS_queryExecOrder = "execOrderQuery";
    public static String STC_TBook_cancelOrder = "stc.tbook.o.c";
    public static String STC_SOR_ExecutionReport = "stc.sor.trade.a";
    public static String STC_DIST_ExecutionReport = "stc.dist.trade.a";
    public static String STC_SOR_tradeOrder = "stc.sor.trade.n";
    public static String STC_SOR_tradeAckOrder = "stc.sor.trade.ack";
    public static final String STC_TBook_ConStatus = "stc.tbook.t.*";
    public static final String STC_RFQ_TakerOrder = "stc.rfq.taker.order";
    public static final String STC_RFQ_MakerOrder = "stc.rfq.maker.order";
    public static final String homeSecurityIdTpoic = "stc.sc.modify.homeSecurityId";
    public static final String STC_TBook_CancelReject = "stc.tbook.c";
    public static String SXZQ_EMS_ExecutionReport = "stc.bond.report.execution.*";
    public static String STC_MDHIST_query = "stc.mdhist.h.q";
    public static String STC_EMS_placeOrder = "stc.ems.o.n";
    public static String STC_EMS_cancelOrder = "stc.ems.o.c";
    public static String STC_TBook_ExecutionReport = "stc.tbook.s.**";
    public static String STC_EMS_ExecutionReport = "stc.ems.v.**";
    public static String STC_EMS_ExecutionReport2 = "stc.ems.e.**";
    public static String STC_EMS_ExecutionReportToBook = "stc.ems.e.**";
    public static String STC_EMS_ExecutionReportODM = "stc.ems.e.odm.**";
    public static String STC_MD_RFQ = "stc.md.p.n";
    public static String STC_MD_TB = "stc.md.tbnp.%s";
    public static String STC_MD_IN = "stc.md.i0np.%s";
    public static String IMIX_MESSAGE = "imixMessage";
    public static String STC_ECNAdapter_cancelOrder = "stc.%s.o.c";
    public static String STC_ECNAdapter_replaceOrder = "stc.%s.o.r";
    public static String STC_ECNAdapter_placeOrder = "stc.%s.o.n";
    public static String STC_ECNAdapter_ExecutionReport = "stc.%s.o.**";
    public static final String STC_ECNAdapter_ConStatus = "stc.%s.t.*";
    public static final String STC_EMS_ConStatus = "stc.%s.t.*";
    public static final String STC_MD_ConStatus = "stc.%s.t.*";
    public static final String STC_ECNAdapter_CancelReject = "stc.%s.c";
    public static final String STC_EMS_CancelReject = "stc.%s.c";
    public static final String STC_EMS_SOROrder = "stc.ems.l.%s.*";
    public static final String STC_ECNAdapter_RFQ = "stc.%s.p.n";
    public static final String STC_SOR_TradeOrder = "stc.%s.trade.**";
    public static final String STC_ECNAdapter_URFQ = "stc.%s.c.p";
    public static final String STC_ECNAdapter_executionAcknowledgement = "stc.%s.o.a";
    public static final String STC_ECNAdapter_Quote = "stc.%s.p.**";
    public static String STC_MD_PRICE = "stc.md.**";
    public static String STC_CUST_QueryMarketData = "stc.cust.md.query";
    public static String STC_TABLE_QueryIntentionQuote = "queryIntentionQuote";
    public static String STC_TABLE_QueryTableEmsOrder = "stc.table.queryEmsOrder";
    public static String STC_PT_ExecOrderSend = "stc.pt.execOrderSend";
    public static String STC_PT_PtExecOrderUpdate = "ptExecOrderUpdate";
    public static String STC_PT_PtExecOrderQuery = "ptExecOrderQuery";
    public static final long OrderBookDispalyPeriodTime = 60000L;
    public static final String Sid = "EMS";
    public static int OrderBookThreadNum =
            Math.min(Runtime.getRuntime().availableProcessors() * 2, 16);
    public static int RFQThreadNum = 5;
    public static int PriceBookThreadNum =
            Math.min(Runtime.getRuntime().availableProcessors() * 2, 16);
    public static int EventThreadNum = 5;
    public static String ackPath = "";
    public static String ackTopic = "";
    public static String tradeackPath = "";
    public static String tradeackTopic = "";
    public static final String TOPIC = "topic";
    public static final String DATA = "data";
    public static final String DATAS = "Datas";
    public static String TotalSize = "totalSize";
    public static final String DATETIME_WITHMS = "yyyyMMdd-HH:mm:ss.SSS";
    public static final String DATETIME_FULLSTYLE = "yyyy-MM-dd HH:mm:ss.SSS";
    public static final String DATETIME_STYLE = "yyyy-MM-dd HH:mm:ss";
    public static String CientVERSION = "v1.0.0 (build20171101)";
    public static String CRMUnBindExecutionReport = "3007";
    public static String CRMUnExecutionReport = "3006";
    public static String OfrCRMUnExecutionReport = "3005";
    public static String BidCRMUnExecutionReport = "3004";
    public static String CRMUnBindQB = "3003";
    public static String BidCRMUnBindQB = "3001";
    public static String OfrCRMUnBindQB = "3002";
    public static String CRMMappingQBUnBindCode = "3008";
    public static String CRMUnFindBroker = "3009";
    public static String Whole = "\u6574\u91cf";
    public static String SuccessCode = "0000";
    public static String SuccessMsg = "Success";
    public static String JobIsRuningCode = "1000";
    public static String NoSupportCode = "1002";
    public static String NoKnowCode = "1003";
    public static String ArgErrorCode = "1004";
    public static String NoAnalyzerCode = "7004";
    public static String RiskFailedCode = "10000";
    public static String NoPermissionCode = "1005";
    public static String NoPermissionMsg = "\u6ca1\u6709\u767b\u5f55";
    public static String Nostrategy = "1008";
    public static String NostrategyMsg =
            "\u4e0d\u652f\u6301\u5bf9\u5e94\u7684\u7b56\u7565\u4ea4\u6613";
    public static String TimeOut = "1007";
    public static String TimeOutMsg = "\u7f51\u7edc\u8d85\u65f6";
    public static String NoOnline = "1015";
    public static String NoOnlineMsg = " is not Online";
    public static String NoPlaceOrderPermissionCode = "1017";
    public static String NoPlaceOrderPermissionMsg = "\u6ca1\u6709\u62a5\u4ef7\u6743\u9650";
    public static String LimitCheck = "10003";
    public static String LimitCheckMsg = "\u8d85\u8fc7\u9650\u6d41";
    public static String NoCrossVenuesPermissionCode = "1018";
    public static String NoCrossVenuesPermissionMsg =
            "\u6ca1\u6709\u6e20\u9053\u64ae\u5408\u6743\u9650";
    public static String CheckFailedCode = "1006";
    public static String IgnoreCode = "10006";
    public static String NoKnowMsg = "System error";
    public static String NoIDCPermission = "1009";
    public static String NoIDCPermissionMsg = "No IDCPermission";
    public static String NoConnectPermission = "1010";
    public static String NoConnectPermissionMsg = "No ConnectPermission";
    public static String BrokerSvrNoOnline = "1011";
    public static String BrokerSvrNoOnlineMsg = "BrokerSvr is not Online";
    public static String PriceSvrNoOnline = "1013";
    public static String PriceSvrNoOnlineMsg = "PriceSvr is not Online";
    public static String ConnectSvrNoOnline = "1015";
    public static String ConnectNoOnlineMsg = "ConnectSvr is not Online";
    public static String NoBrokerConf = "1012";
    public static String NoBrokerConfMsg = "BrokerConf is null";
    public static String BrokerSvrCloseSystem = "1013";
    public static String BrokerSvrCloseMsg = "BrokerSvr Close System";
    public static String PriceOffSet = "1014";
    public static String PriceOffSetMsg = "Price off Set";
    public static String IAMQBInfo = "2001";
    public static String IAMLogin = "2000";
    public static String IAMSyncCrm = "2002";
    public static String IAMUserAuthz = "2003";
    public static String IAMUserTab = "2004";
    public static String CRMAccountList = "4001";
    public static String CRMContactList = "4002";
    public static String CRMInstList = "4003";
    public static String CRMNoFindAccount = "4005";
    public static String CRMNoFindContact = "4006";
    public static String CRMNoFindAccountQQ = "4007";
    public static String CRMNoFindContactQQ = "4008";
    public static String CRMNoFindContactAccountQQ = "4009";
    public static String ErrorMsg1 = "1-\u53c2\u6570\u5f02\u5e38";
    public static String ErrorMsg2 = "2-Ip\u5730\u5740\u6ca1\u6709\u62a5\u5907";
    public static String ErrorMsg3 = "3-\u63a5\u53e3\u6743\u9650\u6ca1\u6709\u5f00\u901a";
    public static String ErrorMsg4 = "4-\u8bf7\u6c42\u8bc6\u522b\u7801\u4e0d\u5b58\u5728";
    public static String ErrorMsg5 = "5-\u65e0\u6548\u8bf7\u6c42";
    public static String UserorPassworderror = "5000";
    public static String UserorSessionnotExist = "5001";
    public static String loginipmacIsnotSame = "5002";
    public static String Accountnameisnull = "5005";
    public static String usernotexsit = "5006";
    public static String Noninterest = "5007";
    public static String NonProPermission = "5008";
    public static String UserLoginOverNum = "5009";
    public static String UserFroze = "5010";
    public static String FakTimeOutRejectCode = "10005";
    public static String FakTimeOutRejectMsg = "FAK\u8ba2\u5355\u8d85\u65f6";
    public static String DQBargainRejectCode = "11011";
    public static String DQRejectCode = "11012";
    public static String DQCancelCode = "11013";
    public static String DQLimitCancelCode = "11014";
    public static String DQTimeOut = "11015";
    public static String DQDeal = "11016";
    public static String RFQStrategyWrongCode = "6000";
    public static String RFQStrategyWrongMsg = "RFQMSvrError";
    public static String Code = "Code";
    public static String Msg = "Msg";
    public static final Charset DEFALUT_CHARSET = Charset.forName("UTF-8");
    public static final String brokerConfSelect = "select * from idc_broker_conf where enable='1'";
    public static final int Second30 = 180;
    public static String selectGoodsSql =
            "select * from  idc_connect_goods where status ='1' order by sorting asc";
    public static int ParasUpdatePeriodTime = 10000;
    public static String Host = "";
    public static String BrokerConnectIdcQuery = "idc.broker.connectOrderQuery";
    public static String BrokerConnectIdcPlaceOrderReq = "idc.broker.connectPlaceOrder";
    public static String BrokerConnectIdcCancelOrderReq = "idc.broker.connectCancelOrder";
    public static String BrokerExecSqlReq = "idc.broker.execsql";
    public static int DBThreadNum = Math.min(Runtime.getRuntime().availableProcessors() * 2, 16);
    public static String CompanyId;
    public static String SubCompanyId;
    public static String CompanyName;
    public static int CalPriceCount;
    public static int SendPriceCount;
    public static String Rule;
    public static String AppName;
    public static String RejectCodeOfPriceHasExpired;
    public static String RejectCodeOfSettlementType;
    public static String RejectCodeOfInstitutionID;
    public static String RejectCodeOfPriceLock;
    public static String RejectCodeOfPriceWhole;
    public static String RejectCodeOfOrderMatchSplit;
    public static String RejectCodeOfOrderExpired;
    public static String RejectCodeOfOrderManual;
    public static String RejectCodeOfInternalAndNormal;
    public static String RejectCodeOfForbit;
    static Hashtable<String, String> rejectHt;
    static Hashtable<String, String> reasonHt;
    static Hashtable<String, String> brokertHt;
    public static String RaftNoLeaderCode;
    public static String NoFindOrder;
    public static String NoFindMMSecurityId;
    public static String NoEnableMMSecurityId;
    public static String ChanelMonitorCode;
    public static String ChanelMonitorMsg;
    public static boolean isRaft;
    public static String BrokerPriceIdcBestReq;
    public static String PriceSvr_PriceBrokerBestNotify;
    public static String PriceSvr_PriceBrokerBestAck;
    public static String CanNotFindBond;
    public static String PriceDuplicate;
    public static String AccountTermIds;
    public static String ContactBizType;
    public static String InstTagList;
    public static String selectSystemParasSql;
    public static String selectUserSystemParasSql;
    public static String selectVenueSecurityIdsSql;
    public static final String CR = "CR";
    public static int LimitQps;
    public static boolean CheckLimit;
    public static int PendingNewdingTimeOut;
    public static int PendingCancelBySorXbondTimeOut;
    public static int PendingCancelByInnerTimeOut;
    public static int PendingNewdingByQDMESPTimeOut;
    public static int PendingCancelByQDMESPTimeOut;
    public static String dispenseMessagePath;
    public static String dispenseMessageTopic;
    public static String sqlitePath;
    public static boolean enableAck;
    public static int FailedLockTime;
    public static boolean HistCDHEnable;
    public static String simtopicP;
    public static String simtopicO;
    public static String ctpTopicP;
    public static String ctpTopicO;
    public static String sseTopicP;
    public static String sseTopicO;
    public static String szeTopicP;
    public static String szeTopicO;
    public static String securityIDPath;
    public static String SendStartCron;
    public static String SendStopCron;
    public static volatile boolean IsStopSend;
    @Deprecated public static final String BOND_TYPE_CREDIT = "CREDIT";
    public static final String BOND_TYPE_BCO = "BCO";
    @Deprecated public static final String BOND_TYPE_RATE = "RATE";
    public static final String BOND_TYPE_BNC = "BNC";
    public static final String BOND_TYPE_ABS = "ABS";
    public static final String BOND_TYPE_NCD = "NCD";
    public static final String BOND_TYPE_IRS = "IRS";
    public static final String BOND_TYPE_VIRTUAL = "VIRTUAL";
    public static final String VENUE_GW_QDMESP = "QDMESP";
    public static final String VENUE_GW_ODM = "ODM";
    public static final String VENUE_GW_CDH = "CDH";
    public static final String VENUE_GW_INNER = "INNER";
    public static final String VENUE_GW_XREPO = "XREPO";
    public static final String VENUE_GW_REPONDM = "RepoNDM";
    public static final String VENUE_GW_REPOQDM = "RepoQDM";
    public static final String VENUE_GW_SDN = "SDN";
    public static Map<String, MsgTag> CONNECTOR_TAG;
    public static final String queryTaskLogSql =
            "select * from stc_tbook_system_oplog where batchNum = ? and content like ?";
    public static final String HOME_Tag = "Home";
    public static final String QUANT_Tag = "Quant";
    public static final String MM_Tag = "MM";
    public static final String MMSHE_Tag = "MMSHE";
    public static final String INNER_Tag = "INNER";
    public static final String HOME_Tag_Msg = "\u4e3b\u9875";
    public static final String QUANT_Tag_Msg = "\u7b56\u7565";
    public static final String MM_Tag_Msg = "\u53cc\u8fb9\u505a\u5e02";
    public static final String MMSHE_Tag_Msg = "\u505a\u5e02\u5de5\u4f5c\u53f0";
    public static final String INNER_Tag_Msg = "\u5185\u90e8\u770b\u677f";

    public static String getNetType() {
        PropertiesHelper atsrootConfig;
        if (StringUtils.isBlank((CharSequence) NetType)
                && StringUtil.isEmpty(
                        NetType =
                                (atsrootConfig = ATSROOTConfig.getATSROOTConfig())
                                        .getProperty("NetType", "ceda"))) {
            NetType = "ceda";
        }
        return NetType;
    }

    public static boolean getUseMonitor() {
        if (!UseMonitor) {
            PropertiesHelper atsrootConfig = ATSROOTConfig.getATSROOTConfig();
            String value = atsrootConfig.getProperty("UseMonitor", "false");
            UseMonitor = StringUtil.isEmpty(value) ? false : value.equalsIgnoreCase("true");
        }
        return UseMonitor;
    }

    public static boolean getMonitorInterface() {
        if (!MonitorInterface) {
            PropertiesHelper atsrootConfig = ATSROOTConfig.getATSROOTConfig();
            String value = atsrootConfig.getProperty("MonitorInterface", "false");
            MonitorInterface = StringUtil.isEmpty(value) ? false : value.equalsIgnoreCase("true");
        }
        return MonitorInterface;
    }

    public static String getBrokerName(String brokerid) {
        return brokertHt.get(brokerid);
    }

    public static String getRejectMsg(String code) {
        return rejectHt.get(code);
    }

    static {
        CalPriceCount = 20;
        SendPriceCount = 10;
        Rule = "None";
        AppName = "ConnectSvr";
        RejectCodeOfPriceHasExpired = "6000";
        RejectCodeOfSettlementType = "6001";
        RejectCodeOfInstitutionID = "6002";
        RejectCodeOfPriceLock = "6003";
        RejectCodeOfPriceWhole = "6004";
        RejectCodeOfOrderMatchSplit = "6005";
        RejectCodeOfOrderExpired = "6006";
        RejectCodeOfOrderManual = "6007";
        RejectCodeOfInternalAndNormal = "6008";
        RejectCodeOfForbit = "6009";
        rejectHt = new Hashtable();
        reasonHt = new Hashtable();
        brokertHt = new Hashtable();
        rejectHt.put(RejectCodeOfPriceHasExpired, "price has expired.");
        rejectHt.put(RejectCodeOfSettlementType, "price settlementtype no match.");
        rejectHt.put(RejectCodeOfInstitutionID, "price institutionid no match.");
        rejectHt.put(RejectCodeOfPriceLock, "price has locked.");
        rejectHt.put(RejectCodeOfPriceWhole, "price whole.");
        rejectHt.put(RejectCodeOfOrderMatchSplit, "rule no match.");
        rejectHt.put(RejectCodeOfInternalAndNormal, "price internal and normal.");
        rejectHt.put(RejectCodeOfForbit, "venue forbit.");
        rejectHt.put(DQBargainRejectCode, "\u8fdb\u5165\u8bae\u4ef7\u64a4\u9500\u8ba2\u5355");
        rejectHt.put(DQRejectCode, "\u56de\u4ef7\u62d2\u7edd");
        rejectHt.put(DQCancelCode, "\u5df2\u64a4\u5355");
        rejectHt.put(DQLimitCancelCode, "\u62a5\u4ef7\u65b9\u5df2\u64a4\u5355");
        rejectHt.put(DQTimeOut, "\u56de\u4ef7\u5355\u8d85\u65f6");
        rejectHt.put(FakTimeOutRejectCode, "FAK\u8ba2\u5355\u8d85\u65f6");
        rejectHt.put(RejectCodeOfOrderManual, "\u4eba\u5de5\u64a4\u5355");
        rejectHt.put("10", "qb\u5e10\u53f7\u65e0\u6548");
        rejectHt.put("6000", "\u884c\u60c5\u5df2\u66f4\u65b0");
        rejectHt.put("6001", "\u6e05\u7b97\u4e0d\u5339\u914d");
        rejectHt.put("6002", "\u540c\u673a\u6784");
        rejectHt.put("6003", "\u4ef7\u683c\u9501\u5b9a ");
        rejectHt.put("6004", "\u6574\u91cf");
        rejectHt.put("6005", "\u4e0d\u7b26\u89c4\u5219");
        rejectHt.put("6006", "\u8ba2\u5355\u5df2\u8fc7\u671f");
        rejectHt.put("6009", "\u6e20\u9053\u7981\u6b62");
        rejectHt.put("1003", "\u7cfb\u7edf\u5f02\u5e38");
        rejectHt.put("1004", "\u53c2\u6570\u9519\u8bef");
        rejectHt.put("1007", "\u7f51\u7edc\u8d85\u65f6");
        rejectHt.put("10006", "\u6e20\u9053\u6267\u884c\u5ffd\u7565");
        rejectHt.put("10007", "\u6e20\u9053\u6267\u884c\u64a4\u9500");
        rejectHt.put("10008", "\u6e20\u9053\u6267\u884c\u62d2\u7edd");
        rejectHt.put("10010", "FAK\u70b9\u4ef7\u5355\u62d2\u7edd");
        rejectHt.put("10005", "FAK\u8ba2\u5355\u8d85\u65f6");
        rejectHt.put("1015", "\u670d\u52a1\u4e0d\u5728\u7ebf");
        rejectHt.put("9000", "\u65b0\u589e\u8ba2\u5355\u5f02\u5e38");
        brokertHt.put("1", "\u56fd\u5229");
        brokertHt.put("2", "\u56fd\u9645");
        brokertHt.put("3", "\u4e2d\u8bda");
        brokertHt.put("4", "\u5e73\u5b89");
        brokertHt.put("5", "\u4fe1\u5510");
        reasonHt.put(
                "ODM:Invalid trading account or trader!",
                "cfets\u4ea4\u6613\u5458\u8d26\u53f7\u65e0\u6548");
        reasonHt.put(
                "ODM:Bond is not apply to this quotation type!",
                "\u503a\u5238\u68c0\u67e5\u5931\u8d25");
        reasonHt.put("EMS:IocTimeOut", "\u8d85\u65f6");
        reasonHt.put("EMS:PendingNewing TimeOut", "\u8d85\u65f6");
        reasonHt.put("no permission", "\u65e0\u62a5\u4ef7\u6216\u70b9\u4ef7\u6743\u9650");
        reasonHt.put("price must be greater than 0", "\u6536\u76ca\u7387\u5fc5\u987b\u5927\u4e8e0");
        reasonHt.put("NOT_ENOUGH_QUANTITY", "\u65e0\u53ef\u6210\u4ea4\u884c\u60c5");
        reasonHt.put("TIF:Ioc Cancell", "\u5df2\u64a4\u5355");
        reasonHt.put("Validuntitime TimeOut", "\u8ba2\u5355\u6709\u6548\u671f\u5df2\u5230");
        reasonHt.put("FirmToNonTime TimeOut", "Firm\u8f6cNoFirm\u65f6\u95f4\u5df2\u5230");
        reasonHt.put("Manual Cancell", "\u5df2\u624b\u52a8\u64a4\u5355");
        reasonHt.put(":api cancel", "\u5df2\u64a4\u5355");
        reasonHt.put("system cancel[validUntilTime TimeOut]", "\u7cfb\u7edf\u5df2\u64a4\u5355");
        reasonHt.put(
                "FAK\u90e8\u5206\u6210\u4ea4\uff0c\u53d1\u8d77\u64a4\u9500",
                "FAK\u90e8\u5206\u6210\u4ea4\uff0c\u53d1\u8d77\u64a4\u9500");
        RaftNoLeaderCode = "7000";
        NoFindOrder = "8000";
        NoFindMMSecurityId = "8001";
        NoEnableMMSecurityId = "8002";
        ChanelMonitorCode = "8010";
        ChanelMonitorMsg = "\u6e20\u9053\u76d1\u63a7";
        isRaft = false;
        BrokerPriceIdcBestReq = "idc.broker.price.best";
        PriceSvr_PriceBrokerBestNotify = "idc.price.best.notify";
        PriceSvr_PriceBrokerBestAck = "idc.price.best.ack";
        CanNotFindBond = "9000";
        PriceDuplicate = "9001";
        AccountTermIds = "1,2,11,12";
        ContactBizType = "BNC,BCO,ABS,NCD";
        InstTagList = "BOND,NCD";
        selectSystemParasSql =
                "select * from  stc_tbook_system_parameter where param_key"
                    + " in('STC_TD_VenueTypeGWs','STC_MD_VenueTypeGWs','STC_Venues_Setting')";
        selectUserSystemParasSql =
                "select * from  stc_tbook_system_parameter where param_key =? and username =?";
        selectVenueSecurityIdsSql = "select * from  stc_tbook_venue_securityids ";
        LimitQps = 100;
        CheckLimit = true;
        PendingNewdingTimeOut = 10000;
        PendingCancelBySorXbondTimeOut = 10000;
        PendingCancelByInnerTimeOut = 3600000;
        PendingNewdingByQDMESPTimeOut = 30000;
        PendingCancelByQDMESPTimeOut = 30000;
        dispenseMessagePath = "";
        dispenseMessageTopic = "";
        sqlitePath = "";
        enableAck = false;
        FailedLockTime = 30;
        simtopicP = "stc.simulate.odm.%s.p";
        simtopicO = "stc.simulate.odm.%s.o";
        ctpTopicP = "stc.simulate.ctp.%s.p";
        ctpTopicO = "stc.simulate.ctp.%s.o";
        sseTopicP = "stc.simulate.sse.%s.p";
        sseTopicO = "stc.simulate.sse.%s.o";
        szeTopicP = "stc.simulate.sze.%s.p";
        szeTopicO = "stc.simulate.sze.%s.o";
        securityIDPath = "";
        IsStopSend = false;
        CONNECTOR_TAG =
                new HashMap<String, MsgTag>() {
                    {
                        this.put("INNER", MsgTag.TB_6_1_1);
                        this.put(Consts.VENUE_GW_QDMESP, MsgTag.SB_4_2);
                        this.put(Consts.VENUE_GW_ODM, MsgTag.XB_6_1_3);
                        this.put(Consts.VENUE_GW_CDH, MsgTag.JJS_6_1_4);
                        this.put(Consts.VENUE_GW_XREPO, MsgTag.REPO_6_1_9);
                        this.put(Consts.VENUE_GW_REPONDM, MsgTag.REPO_6_1_9);
                        this.put(Consts.VENUE_GW_REPOQDM, MsgTag.REPO_6_1_9);
                        this.put(Consts.VENUE_GW_SDN, MsgTag.SDN_6_1_8);
                    }
                };
    }
}
