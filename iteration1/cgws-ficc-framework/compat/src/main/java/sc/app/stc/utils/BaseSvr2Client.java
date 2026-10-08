// Recovered compatibility source; provenance: recovered-support.json.
package sc.app.stc.utils;

import ats.app.api.BaseApi;
import ats.app.ym.ceda.ICedaConnectionListener;
import ats.foundation.utils.exception.CodeException;
import ats.message2.CharsetConvertType;
import ats.message2.MsgHelper;

import cn.hutool.core.collection.CollUtil;
import cn.hutool.core.collection.ConcurrentHashSet;
import cn.hutool.core.convert.Convert;
import cn.hutool.core.date.DateUtil;
import cn.hutool.core.map.CaseInsensitiveMap;
import cn.hutool.core.map.multi.RowKeyTable;
import cn.hutool.core.map.multi.Table;
import cn.hutool.core.util.StrUtil;
import cn.hutool.json.JSONUtil;

import com.alibaba.fastjson.JSONObject;
import com.google.common.collect.ImmutableMap;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import sc.app.stc.api.HolidyResult;
import sc.app.stc.api.QueryChianBondResult;
import sc.app.stc.api.QueryChianBondlReq;
import sc.app.stc.api.QueryInstitutionResult;
import sc.app.stc.api.QueryPartyInfoReq;
import sc.app.stc.api.QueryPartyInfoResult;
import sc.app.stc.api.QueryPartydealerinfoReq;
import sc.app.stc.api.QueryPartydealerinfoResult;
import sc.app.stc.api.QuerySymbolReq;
import sc.app.stc.api.QuerySymbolResult;
import sc.app.stc.api.QuoteTagResult;
import sc.app.stc.api.base.po.BondInfoView;
import sc.app.stc.api.base.po.BondsInfoQueryResult;
import sc.app.stc.api.base.po.BrokerHolidayQueryResult;
import sc.app.stc.api.base.po.Holiday;
import sc.app.stc.api.base.po.TBondValuationView;
import sc.app.stc.api.base.po.TDealerInfoGrp;
import sc.app.stc.api.base.po.TInfoGrp;
import sc.app.stc.api.base.po.TInstitution;
import sc.app.stc.enums.BaseClearType;
import sc.app.stc.net.NetClientWrapper;
import sc.app.stc.tag.dto.QuoteTagDto;

import java.text.SimpleDateFormat;
import java.util.Comparator;
import java.util.Date;
import java.util.HashMap;
import java.util.Hashtable;
import java.util.List;
import java.util.Map;
import java.util.Optional;
import java.util.Set;
import java.util.concurrent.ConcurrentHashMap;
import java.util.concurrent.atomic.AtomicBoolean;
import java.util.concurrent.locks.ReadWriteLock;
import java.util.concurrent.locks.ReentrantReadWriteLock;

public class BaseSvr2Client extends BaseApi {
    private static Logger logger = LoggerFactory.getLogger(BaseSvr2Client.class);
    public static ConcurrentHashMap<String, BondInfoView> htBondinfos = new ConcurrentHashMap();
    public static ConcurrentHashMap<String, Hashtable<String, Holiday>> htAllHoliday =
            new ConcurrentHashMap();
    public static ConcurrentHashMap<String, TBondValuationView> htChianBondsviews =
            new ConcurrentHashMap();
    private static ConcurrentHashMap<String, TDealerInfoGrp> htPartydealerinfoviews =
            new ConcurrentHashMap();
    private static ConcurrentHashMap<String, TInfoGrp> htPartyinfoviews = new ConcurrentHashMap();
    private static ConcurrentHashMap<String, String> csiBondValMap = new ConcurrentHashMap();
    private static ConcurrentHashMap<String, TInstitution> institutionHashMap =
            new ConcurrentHashMap();
    public static Set<String> failedBonds = new ConcurrentHashSet();
    public static Set<String> failedChianBonds = new ConcurrentHashSet();
    public static Set<String> faildeCsiBonds = new ConcurrentHashSet();
    private static final Table<String, String, QuoteTagDto> tagValueTable =
            new RowKeyTable(new ConcurrentHashMap(), ConcurrentHashMap::new);
    private static final Table<String, String, String> tagNameTable =
            new RowKeyTable(new ConcurrentHashMap(), ConcurrentHashMap::new);
    private static final Table<String, String, String> tagTypeTable =
            new RowKeyTable(new ConcurrentHashMap(), ConcurrentHashMap::new);
    private static final Map<String, Long> queryTagFailTimestampMap =
            new ConcurrentHashMap<String, Long>();
    public static Table<String, String, Map<String, Object>> infoTable = new RowKeyTable();
    public static ReadWriteLock bondinfolock = new ReentrantReadWriteLock();
    public static ReadWriteLock holidaylock = new ReentrantReadWriteLock();
    public static ReadWriteLock chianBondlock = new ReentrantReadWriteLock();
    public static ReadWriteLock partyinfolock = new ReentrantReadWriteLock();
    public static ReadWriteLock partydealerinfolock = new ReentrantReadWriteLock();
    private static String server = "BaseSvr";
    public static String ServerKey = "SERVER.BaseSvr";
    public static String clear = "stc.base.clear";
    public static String baseClear = "stc.base.manualClear.**";
    public static String allholiday = "stc.base.allholiday";
    public static String topicLoadBondsInfo = "stc.base.loadBondsInfo";
    public static boolean isLoadAllHoliday = false;
    public static boolean isReloadBondInfos = false;
    public static boolean isSubBondBaseInfo = false;

    public static void initialize(
            NetClientWrapper cedaClientWrapper, BaseApi.IMessage<HolidyResult> message)
            throws CodeException {
        htBondinfos.clear();
        htAllHoliday.clear();
        htChianBondsviews.clear();
        csiBondValMap.clear();
        htPartydealerinfoviews.clear();
        htPartyinfoviews.clear();
        institutionHashMap.clear();
        failedBonds.clear();
        failedChianBonds.clear();
        faildeCsiBonds.clear();
        infoTable.clear();
        isReloadBondInfos = BaseSvrClient.isLoadBond;
        BaseSvr2Client.tagTablesClear();
        final BaseApi.IMessage<HolidyResult> message1 =
                new BaseApi.IMessage<HolidyResult>() {

                    @Override
                    public void onMessage(String topic, HolidyResult t) {
                        htBondinfos.clear();
                        htChianBondsviews.clear();
                        csiBondValMap.clear();
                        htPartydealerinfoviews.clear();
                        htPartyinfoviews.clear();
                        institutionHashMap.clear();
                        failedBonds.clear();
                        failedChianBonds.clear();
                        faildeCsiBonds.clear();
                        infoTable.clear();
                        BaseSvr2Client.tagTablesClear();
                        logger.info("BaseSvr clear.");
                    }
                };
        BaseSvr2Client.cedaClientWrapper = cedaClientWrapper;
        ICedaConnectionListener cedaClientConnectListener =
                new ICedaConnectionListener() {

                    @Override
                    public void onDisconnected(String serverName) {
                        logger.info("{} onDisconnected", serverName);
                        message1.onMessage("", null);
                    }

                    @Override
                    public void onConnected(String arg0) {}
                };
        BaseSvr2Client.addCedaClientConnectListener(ServerKey, cedaClientConnectListener);
        BaseSvr2Client.cedaClientWrapper.init(ServerKey, new String[0]);
        BaseSvr2Client.subscribeWithImage(
                ServerKey, server, clear, message, HolidyResult.class, false);
        DataCallBackUtils.notifyCallBack(DataType.BondInfo);
        DataCallBackUtils.notifyCallBack(DataType.ChianondInfo);
        DataCallBackUtils.notifyCallBack(DataType.Partyinfo);
        DataCallBackUtils.notifyCallBack(DataType.Partydealerinfo);
        BaseSvr2Client.subscribeWithImage(
                ServerKey, server, clear, message1, HolidyResult.class, false);
        BaseApi.IMessage<BaseClearType> message2 =
                new BaseApi.IMessage<BaseClearType>() {

                    @Override
                    public void onMessage(String topic, BaseClearType type) {
                        logger.info("base clear topic: {}, type: {}", topic, type);
                        if (StringUtil.isNotBlank(topic)) {
                            String clearKey = topic.replace("stc.base.manualClear.", "");
                            logger.info("clearkey: {}", clearKey);
                            BaseSvr2Client.clearByType(type, clearKey);
                        }
                    }
                };
        BaseSvr2Client.subscribeWithImage(
                ServerKey, server, baseClear, message2, BaseClearType.class, false);
        logger.info("isReloadBondInfos : {}", isReloadBondInfos);
        if (isReloadBondInfos) {
            BaseApi.IMessage<String> loadBondsInfoMessage =
                    new BaseApi.IMessage<String>() {

                        @Override
                        public void onMessage(String topic, String t) {
                            try {
                                logger.info("loadBondsInfoMessage in, content is {} ", t);
                                if (StringUtil.isBlank(Consts.securityIDPath)) {
                                    logger.info("loadBondsInfoMessage in, clear htChianBondsviews");
                                    htBondinfos.clear();
                                    htChianBondsviews.clear();
                                    csiBondValMap.clear();
                                    failedBonds.clear();
                                    failedChianBonds.clear();
                                    infoTable.clear();
                                    faildeCsiBonds.clear();
                                }
                            } catch (Exception e) {
                                logger.error("loadBondsInfoMessage error!", (Throwable) e);
                            }
                        }
                    };
            BaseSvr2Client.subscribeWithImage(
                    ServerKey,
                    server,
                    topicLoadBondsInfo,
                    loadBondsInfoMessage,
                    String.class,
                    false);
        }
        logger.info("isSubBondBaseInfo : {}", isSubBondBaseInfo);
        if (isSubBondBaseInfo) {
            BaseApi.IMessage<BondsInfoQueryResult> bondBaseInfoMessage =
                    new BaseApi.IMessage<BondsInfoQueryResult>() {

                        @Override
                        public void onMessage(String topic, BondsInfoQueryResult queryResult) {
                            try {
                                logger.info(
                                        "bondBaseInfoMessage in, size is {} ",
                                        queryResult.totalSize);
                                if (queryResult.bondInfoViews != null
                                        && queryResult.bondInfoViews.size() == 1
                                        && StringUtil.isBlank(Consts.securityIDPath)) {
                                    String securityId =
                                            queryResult.bondInfoViews.get((int) 0).Bond_ID;
                                    logger.info(
                                            "{} before update  {} ",
                                            securityId,
                                            SecurityInfoUtils.getSecurityIDPo(securityId));
                                    SecurityInfoUtils.load(Consts.securityIDPath);
                                    logger.info(
                                            "{} after update {} ",
                                            securityId,
                                            SecurityInfoUtils.getSecurityIDPo(securityId));
                                }
                            } catch (Exception e) {
                                logger.error("bondBaseInfoMessage error!", (Throwable) e);
                            }
                        }
                    };
            BaseSvr2Client.subscribeWithImage(
                    ServerKey,
                    server,
                    BaseSvrClient.bondinfo,
                    bondBaseInfoMessage,
                    BondsInfoQueryResult.class,
                    false);
        }
        if (isLoadAllHoliday) {
            BaseApi.IMessage<BrokerHolidayQueryResult> message4 =
                    new BaseApi.IMessage<BrokerHolidayQueryResult>() {

                        /*
                         * WARNING - Removed try catching itself - possible behaviour change.
                         */
                        @Override
                        public void onMessage(String topic, BrokerHolidayQueryResult t) {
                            holidaylock.writeLock().lock();
                            try {
                                SimpleDateFormat sdf = new SimpleDateFormat("yyyyMMdd");
                                if (t != null && t.Datas != null && t.Datas.size() > 0) {
                                    htAllHoliday.clear();
                                }
                                for (Holiday h : t.Datas) {
                                    if (h.holiday_date != null) {
                                        String key =
                                                String.format("%s_%s", h.country, h.market_type)
                                                        .toUpperCase();
                                        Hashtable<String, Holiday> lt = htAllHoliday.get(key);
                                        if (lt == null) {
                                            lt = new Hashtable();
                                            htAllHoliday.put(key, lt);
                                        }
                                        lt.put(sdf.format(h.holiday_date), h);
                                        continue;
                                    }
                                    logger.warn("holiday_date can't null", h);
                                }
                            } finally {
                                holidaylock.writeLock().unlock();
                            }
                            DataCallBackUtils.notifyCallBack(DataType.HolidayQuery);
                            System.gc();
                            logger.warn("System.gc");
                        }
                    };
            BaseSvr2Client.subscribeWithImage(
                    ServerKey, server, allholiday, message4, BrokerHolidayQueryResult.class, false);
        }
    }

    public static QuoteTagDto getQuoteTag(
            String tagType, String childTag, String childTagName, String childTagValue) {
        String queryKey =
                StrUtil.format(
                        (CharSequence) "{}-{}-{}-{}",
                        (Object[]) new Object[] {tagType, childTag, childTagName, childTagValue});
        QuoteTagDto dto =
                BaseSvr2Client.getQuoteTagFromMap(tagType, childTag, childTagName, childTagValue);
        if (dto == null) {
            Long lastFailTime = queryTagFailTimestampMap.get(queryKey);
            if (lastFailTime != null && System.currentTimeMillis() - lastFailTime < 300000L) {
                return null;
            }
            dto =
                    BaseSvr2Client.getQuoteTagFromServer(
                            tagType, childTag, childTagName, childTagValue);
            if (dto != null) {
                BaseSvr2Client.cacheQuoteTag(dto);
            } else {
                queryTagFailTimestampMap.put(queryKey, System.currentTimeMillis());
            }
        }
        if (logger.isDebugEnabled()) {
            logger.debug(
                    "getQuoteTag tagType:{},childTag:{},childTagName:{},childTagValue:{},result:{}",
                    new Object[] {tagType, childTag, childTagName, childTagValue, dto});
        }
        return dto;
    }

    public static void tagTablesClear() {
        tagNameTable.clear();
        tagTypeTable.clear();
        tagValueTable.clear();
    }

    private static void cacheQuoteTag(QuoteTagDto dto) {
        if (dto == null
                || StrUtil.isBlank((CharSequence) dto.getTagType())
                || StrUtil.isBlank((CharSequence) dto.getChildTag())
                || StrUtil.isBlank((CharSequence) dto.getChildTagValue())) {
            return;
        }
        tagTypeTable.put(dto.getTagType(), dto.getChildTag(), dto.getChildTagValue());
        tagNameTable.put(dto.getTagType(), dto.getChildTagName(), dto.getChildTagValue());
        tagValueTable.put(dto.getTagType(), dto.getChildTagValue(), dto);
    }

    private static QuoteTagDto getQuoteTagFromServer(
            String tagType, String childTag, String childTagName, String childTagValue) {
        ImmutableMap reqMap =
                ImmutableMap.of(
                        "tagType",
                        Optional.ofNullable(tagType).orElse(""),
                        "childTag",
                        Optional.ofNullable(childTag).orElse(""),
                        "childTagName",
                        Optional.ofNullable(childTagName).orElse(""),
                        "childTagValue",
                        Optional.ofNullable(childTagValue).orElse(""));
        try {
            List<QuoteTagResult> results;
            if (BaseSvr2Client.isOnline(ServerKey)
                    && CollUtil.isNotEmpty(
                            results =
                                    BaseSvr2Client.requestSync(
                                            ServerKey,
                                            "stc.base.tags.query",
                                            JSONUtil.toJsonStr(reqMap),
                                            QuoteTagResult.class,
                                            false,
                                            false,
                                            true))
                    && null != results.get(0).getDatas()
                    && StrUtil.isNotBlank((CharSequence) results.get(0).getDatas().getTagType())) {
                return results.get(0).getDatas();
            }
        } catch (CodeException e) {
            logger.error("getQuoteTagFromServer error", (Throwable) e);
        }
        return null;
    }

    public static QuoteTagDto getQuoteTagFromMap(
            String tagType, String childTag, String childTagName, String childTagValue) {
        if (StrUtil.isBlank((CharSequence) tagType)) {
            return null;
        }
        String tagValue = null;
        if (StrUtil.isNotBlank((CharSequence) childTag)) {
            tagValue = (String) tagTypeTable.get(tagType, childTag);
        } else if (StrUtil.isNotBlank((CharSequence) childTagName)) {
            tagValue = (String) tagNameTable.get(tagType, childTagName);
        } else if (StrUtil.isNotBlank((CharSequence) childTagValue)) {
            tagValue = childTagValue;
        }
        if (StrUtil.isNotBlank((CharSequence) tagValue)) {
            return (QuoteTagDto) tagValueTable.get(tagType, tagValue);
        }
        return null;
    }

    public static BondInfoView getBondInfo(String securityID) {
        if (StrUtil.isBlank((CharSequence) securityID)) {
            return null;
        }
        QuerySymbolReq querySymbolReq = new QuerySymbolReq();
        querySymbolReq.SecurityID = securityID;
        QuerySymbolResult presult = new QuerySymbolResult();
        BondInfoView bondInfoView = htBondinfos.get(querySymbolReq.SecurityID);
        try {
            if (bondInfoView != null) {
                return bondInfoView;
            }
            String content = "";
            content =
                    MsgHelper.Serializer(
                            querySymbolReq, (CharsetConvertType) CharsetConvertType.None);
            if (BaseSvr2Client.isOnline(ServerKey)
                    && !failedBonds.contains(querySymbolReq.SecurityID)) {
                List<QuerySymbolResult> lt =
                        BaseSvr2Client.requestSync(
                                ServerKey,
                                "querySecurity",
                                content,
                                QuerySymbolResult.class,
                                false,
                                false,
                                true);
                if (lt != null && lt.size() > 0) {
                    presult = lt.get(0);
                }
                if (presult != null && presult.isSuccess()) {
                    if (presult.datas.size() > 0) {
                        bondInfoView = presult.datas.get(0);
                        htBondinfos.put(querySymbolReq.SecurityID, presult.datas.get(0));
                    } else {
                        failedBonds.add(querySymbolReq.SecurityID);
                    }
                }
            }
        } catch (Exception e) {
            logger.warn("getBondInfo fail:{}", e.getMessage());
        }
        return bondInfoView;
    }

    public static String getCsiBondValuation(String securityID) {
        if (StrUtil.isBlank((CharSequence) securityID)) {
            return null;
        }
        String val = csiBondValMap.get(securityID);
        if (StrUtil.isBlank((CharSequence) val)) {
            try {
                List<Map> datas;
                HashMap<String, String> reqMap = new HashMap<String, String>();
                reqMap.put("SecurityID", securityID);
                if (BaseSvr2Client.isOnline(ServerKey)
                        && !faildeCsiBonds.contains(securityID)
                        && CollUtil.isNotEmpty(
                                datas =
                                        BaseSvr2Client.requestSync(
                                                ServerKey,
                                                "queryCsiBondValue",
                                                JSONUtil.toJsonStr(reqMap),
                                                Map.class,
                                                false,
                                                false,
                                                true))) {
                    Map data = (Map) datas.get(0).get("Datas");
                    if (CollUtil.isNotEmpty((Map) data)) {
                        val = (String) data.get(securityID);
                        if (StrUtil.isNotBlank((CharSequence) val)) {
                            csiBondValMap.put(securityID, val);
                        }
                    } else {
                        faildeCsiBonds.add(securityID);
                    }
                }
            } catch (Exception e) {
                logger.error("getCsiBondValuation fail:{}", (Throwable) e);
            }
        }
        return val;
    }

    public static TBondValuationView getChianBondInfo(String securityID) {
        QueryChianBondlReq querySymbolReq = new QueryChianBondlReq();
        querySymbolReq.SecurityID = securityID;
        QueryChianBondResult presult = new QueryChianBondResult();
        TBondValuationView tBondValuationView = htChianBondsviews.get(querySymbolReq.SecurityID);
        try {
            if (tBondValuationView != null) {
                return tBondValuationView;
            }
            String content = "";
            content =
                    MsgHelper.Serializer(
                            querySymbolReq, (CharsetConvertType) CharsetConvertType.None);
            if (BaseSvr2Client.isOnline(ServerKey)
                    && !failedChianBonds.contains(querySymbolReq.SecurityID)) {
                List<QueryChianBondResult> lt =
                        BaseSvr2Client.requestSync(
                                ServerKey,
                                "queryChianSecurity",
                                content,
                                QueryChianBondResult.class,
                                false,
                                false,
                                true);
                if (lt != null && lt.size() > 0) {
                    presult = lt.get(0);
                }
                if (presult != null && presult.isSuccess()) {
                    if (presult.datas.size() > 0) {
                        tBondValuationView = presult.datas.get(0);
                        htChianBondsviews.put(querySymbolReq.SecurityID, presult.datas.get(0));
                        logger.info("{} getChianBondInfo from server success", securityID);
                    } else {
                        failedChianBonds.add(querySymbolReq.SecurityID);
                    }
                }
            }
        } catch (Exception e) {
            logger.warn("getChianBondInfo fail:{}", e.getMessage());
        }
        return tBondValuationView;
    }

    public static TInfoGrp getPartyInfo(String partyCode) {
        QueryPartyInfoReq queryPartyInfoReq = new QueryPartyInfoReq();
        queryPartyInfoReq.PartyCode6 = partyCode;
        QueryPartyInfoResult presult = new QueryPartyInfoResult();
        TInfoGrp tInfoGrp = htPartyinfoviews.get(queryPartyInfoReq.PartyCode6);
        try {
            if (tInfoGrp != null) {
                return tInfoGrp;
            }
            String content = "";
            content =
                    MsgHelper.Serializer(
                            queryPartyInfoReq, (CharsetConvertType) CharsetConvertType.None);
            if (BaseSvr2Client.isOnline(ServerKey)) {
                List<QueryPartyInfoResult> lt =
                        BaseSvr2Client.requestSync(
                                ServerKey,
                                "stc.base.PartyInfo",
                                content,
                                QueryPartyInfoResult.class,
                                false,
                                false,
                                true);
                if (lt != null && lt.size() > 0) {
                    presult = lt.get(0);
                }
                if (presult != null && presult.isSuccess() && presult.datas.size() > 0) {
                    tInfoGrp = presult.datas.get(0);
                    htPartyinfoviews.put(queryPartyInfoReq.PartyCode6, presult.datas.get(0));
                }
            }
        } catch (Exception e) {
            logger.warn("getPartyInfo fail:{}", e.getMessage());
        }
        return tInfoGrp;
    }

    public static TInstitution getInstitution(String instCode) {
        if (StrUtil.isBlank((CharSequence) instCode)) {
            return null;
        }
        HashMap<String, String> reqMap = new HashMap<String, String>();
        reqMap.put("Institution_Code", instCode);
        TInstitution institution = institutionHashMap.get(instCode);
        try {
            String content;
            List<QueryInstitutionResult> lt;
            if (institution == null
                    && CollUtil.isNotEmpty(
                            lt =
                                    BaseSvr2Client.requestSync(
                                            ServerKey,
                                            "stc.base.institution",
                                            content =
                                                    MsgHelper.Serializer(
                                                            reqMap,
                                                            (CharsetConvertType)
                                                                    CharsetConvertType.None),
                                            QueryInstitutionResult.class,
                                            false,
                                            false,
                                            true))
                    && CollUtil.isNotEmpty(lt.get((int) 0).datas)) {
                institution = lt.get((int) 0).datas.get(0);
                logger.info("{} getInstitution from server success", instCode);
                institutionHashMap.put(institution.getInstitution_Code(), institution);
            }
        } catch (Exception e) {
            logger.error("getInstitution fail:{}", (Throwable) e);
        }
        return institution;
    }

    public static TInfoGrp getPartyInfo21(String partyCode21) {
        QueryPartyInfoReq queryPartyInfoReq = new QueryPartyInfoReq();
        queryPartyInfoReq.PartyCode21 = partyCode21;
        QueryPartyInfoResult presult = new QueryPartyInfoResult();
        TInfoGrp tInfoGrp = htPartyinfoviews.get(partyCode21);
        try {
            if (tInfoGrp != null) {
                return tInfoGrp;
            }
            String content = "";
            content =
                    MsgHelper.Serializer(
                            queryPartyInfoReq, (CharsetConvertType) CharsetConvertType.None);
            if (BaseSvr2Client.isOnline(ServerKey)) {
                List<QueryPartyInfoResult> lt =
                        BaseSvr2Client.requestSync(
                                ServerKey,
                                "stc.base.PartyInfo",
                                content,
                                QueryPartyInfoResult.class,
                                false,
                                false,
                                true);
                if (lt != null && lt.size() > 0) {
                    presult = lt.get(0);
                }
                if (presult != null && presult.isSuccess() && presult.datas.size() > 0) {
                    tInfoGrp = presult.datas.get(0);
                    htPartyinfoviews.put(queryPartyInfoReq.PartyCode21, presult.datas.get(0));
                }
            }
        } catch (Exception e) {
            logger.warn("getPartyInfo fail:{}", e.getMessage());
        }
        return tInfoGrp;
    }

    public static TDealerInfoGrp getPartydealerinfo(String partyCode, String dealerId) {
        String key = String.format("%s%s", partyCode, dealerId);
        QueryPartydealerinfoReq queryPartydealerinfoReq = new QueryPartydealerinfoReq();
        queryPartydealerinfoReq.PartyCode6 = partyCode;
        queryPartydealerinfoReq.dealerId = dealerId;
        QueryPartydealerinfoResult presult = new QueryPartydealerinfoResult();
        TDealerInfoGrp tDealerInfoGrp = htPartydealerinfoviews.get(key);
        try {
            if (tDealerInfoGrp != null) {
                return tDealerInfoGrp;
            }
            String content = "";
            content =
                    MsgHelper.Serializer(
                            queryPartydealerinfoReq, (CharsetConvertType) CharsetConvertType.None);
            if (BaseSvr2Client.isOnline(ServerKey)) {
                List<QueryPartydealerinfoResult> lt =
                        BaseSvr2Client.requestSync(
                                ServerKey,
                                "queryPartyDealerInfo",
                                content,
                                QueryPartydealerinfoResult.class,
                                false,
                                false,
                                true);
                if (lt != null && lt.size() > 0) {
                    presult = lt.get(0);
                }
                if (presult != null && presult.isSuccess() && presult.datas.size() > 0) {
                    tDealerInfoGrp = presult.datas.get(0);
                    htPartydealerinfoviews.put(key, presult.datas.get(0));
                }
            }
        } catch (Exception e) {
            logger.warn("getPartydealerinfo fail:{}", e.getMessage());
        }
        return tDealerInfoGrp;
    }

    public static Hashtable<String, Holiday> GetHoliday(String key) {
        holidaylock.readLock().lock();
        try {
            Hashtable<String, Holiday> lt;
            Hashtable<String, Holiday> hashtable = lt = htAllHoliday.get(key);
            return hashtable;
        } finally {
            holidaylock.readLock().unlock();
        }
    }

    public static boolean IsHoliday(String key, String date) {
        Hashtable<String, Holiday> lt = BaseSvr2Client.GetHoliday(key);
        boolean flag = false;
        if (lt != null) {
            flag = lt.containsKey(date);
        }
        return flag;
    }

    public static boolean IsHoliday(String date) {
        String key = "CNY_CIB";
        boolean flag = BaseSvr2Client.IsHoliday(key, date);
        return flag;
    }

    public static boolean IsHolidayOverMax(String key, String date) {
        AtomicBoolean flag = new AtomicBoolean(false);
        Hashtable<String, Holiday> lt = BaseSvr2Client.GetHoliday(key);
        if (lt != null) {
            flag.set(lt.containsKey(date));
            lt.keySet().stream()
                    .max(Comparator.comparing(String::valueOf))
                    .ifPresent(
                            max -> {
                                if (date.compareTo((String) max) > 0) {
                                    boolean weekend =
                                            DateUtil.isWeekend(
                                                    (Date)
                                                            DateUtil.parse(
                                                                    (CharSequence) date,
                                                                    (String) "yyyyMMdd"));
                                    flag.set(weekend);
                                }
                            });
        }
        return flag.get();
    }

    public static Map<String, Object> getInfoTableVal(String tableName, String keyValue) {
        Map valMap = (Map) infoTable.get(tableName, keyValue);
        try {
            Map map;
            ImmutableMap reqMap;
            List<Map> response;
            if (CollUtil.isEmpty((Map) valMap)
                    && CollUtil.isNotEmpty(
                            response =
                                    BaseSvr2Client.requestSync(
                                            ServerKey,
                                            "stc.base.info.query",
                                            JSONObject.toJSONString(
                                                    (reqMap =
                                                            ImmutableMap.of(
                                                                    "TableName",
                                                                    tableName,
                                                                    "KeyValue",
                                                                    keyValue))),
                                            Map.class,
                                            false,
                                            false,
                                            true))
                    && (map = response.get(0)).containsKey("Datas")) {
                valMap =
                        Convert.toMap(
                                CaseInsensitiveMap.class,
                                String.class,
                                Object.class,
                                map.get("Datas"));
                infoTable.put(tableName, keyValue, valMap);
            }
        } catch (Exception e) {
            throw new RuntimeException(e);
        }
        return valMap;
    }

    private static void clearByType(BaseClearType type, String key) {
        switch (type) {
            case ALL:
                {
                    htBondinfos.clear();
                    infoTable.clear();
                    htChianBondsviews.clear();
                    csiBondValMap.clear();
                    htPartydealerinfoviews.clear();
                    htPartyinfoviews.clear();
                    institutionHashMap.clear();
                    failedBonds.clear();
                    failedChianBonds.clear();
                    faildeCsiBonds.clear();
                    BaseSvr2Client.tagTablesClear();
                    SecurityInfoUtils.clear();
                }
            case BOND:
                {
                    if (!"**".equals(key)) {
                        BondInfoView removed = htBondinfos.remove(key);
                        SecurityInfoUtils.clearBySecurityId(key);
                        break;
                    }
                    htBondinfos.clear();
                    SecurityInfoUtils.clear();
                    break;
                }
            case CHINA_BOND_VALUATION:
                {
                    if (!"**".equals(key)) {
                        htChianBondsviews.remove(key);
                        break;
                    }
                    htChianBondsviews.clear();
                    break;
                }
            case CSI_BOND_VALUATION:
                {
                    if (!"**".equals(key)) {
                        csiBondValMap.remove(key);
                        break;
                    }
                    csiBondValMap.clear();
                    break;
                }
            case PARTY_DEALER_INFO:
                {
                    if (!"**".equals(key)) {
                        htPartydealerinfoviews.remove(key);
                        break;
                    }
                    htPartydealerinfoviews.clear();
                    break;
                }
            case PARTY_INFO:
                {
                    if (!"**".equals(key)) {
                        htPartyinfoviews.remove(key);
                        break;
                    }
                    htPartyinfoviews.clear();
                    break;
                }
            case INST_INFO:
                {
                    if (!"**".equals(key)) {
                        institutionHashMap.remove(key);
                        break;
                    }
                    institutionHashMap.clear();
                    break;
                }
        }
    }

    public static class HolidayAscComparator implements Comparator<Holiday> {
        @Override
        public int compare(Holiday o1, Holiday o2) {
            String x = o1.holiday_date.toString() + o1.country + o1.market_type;
            String y = o2.holiday_date.toString() + o1.country + o1.market_type;
            int t = x.compareTo(y);
            return t;
        }
    }
}
