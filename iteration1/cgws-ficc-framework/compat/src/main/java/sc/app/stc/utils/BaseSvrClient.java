// Recovered compatibility source; provenance: recovered-support.json.
package sc.app.stc.utils;

import ats.app.api.BaseApi;
import ats.foundation.utils.exception.CodeException;

import cn.hutool.core.collection.CollUtil;
import cn.hutool.core.date.DateTime;
import cn.hutool.core.date.DateUtil;
import cn.hutool.core.date.LocalDateTimeUtil;
import cn.hutool.core.lang.Assert;
import cn.hutool.core.util.BooleanUtil;
import cn.hutool.core.util.StrUtil;

import org.apache.http.util.TextUtils;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import reactor.core.publisher.Mono;
import reactor.util.retry.Retry;

import sc.app.stc.api.HolidyResult;
import sc.app.stc.api.base.po.BondInfoView;
import sc.app.stc.api.base.po.BondsInfoQueryResult;
import sc.app.stc.api.base.po.BrokerHolidayQueryResult;
import sc.app.stc.api.base.po.ChinaBondValuationviewQueryResult;
import sc.app.stc.api.base.po.Holiday;
import sc.app.stc.api.base.po.PartyDealerQueryResult;
import sc.app.stc.api.base.po.PartyInfoQueryResult;
import sc.app.stc.api.base.po.TBondValuationView;
import sc.app.stc.api.base.po.TDealerInfoGrp;
import sc.app.stc.api.base.po.TInfoGrp;
import sc.app.stc.api.base.po.TInstitution;
import sc.app.stc.net.NetClientWrapper;
import sc.app.stc.tag.dto.QuoteTagDto;

import java.text.SimpleDateFormat;
import java.time.Duration;
import java.time.LocalDate;
import java.time.format.DateTimeFormatter;
import java.util.Comparator;
import java.util.Date;
import java.util.Hashtable;
import java.util.Map;
import java.util.concurrent.ConcurrentHashMap;
import java.util.concurrent.locks.ReadWriteLock;
import java.util.concurrent.locks.ReentrantReadWriteLock;

public class BaseSvrClient extends BaseApi {
    private static final Logger logger = LoggerFactory.getLogger(BaseSvrClient.class);
    public static Hashtable<String, BondInfoView> htBondinfos = new Hashtable();
    public static Hashtable<String, BondInfoView> tempBondinfos = new Hashtable();
    public static Hashtable<String, Hashtable<String, Holiday>> htAllHoliday = new Hashtable();
    public static Hashtable<String, TBondValuationView> htChianBondsviews = new Hashtable();
    public static Hashtable<String, TBondValuationView> tempChianBondsviews = new Hashtable();
    private static Hashtable<String, TDealerInfoGrp> htPartydealerinfoviews = new Hashtable();
    private static Hashtable<String, TDealerInfoGrp> tempPartydealerinfoviews = new Hashtable();
    private static Hashtable<String, TInfoGrp> htPartyinfoviews = new Hashtable();
    private static Hashtable<String, TInfoGrp> tempPartyinfoviews = new Hashtable();
    public static ReadWriteLock bondinfolock = new ReentrantReadWriteLock();
    public static ReadWriteLock holidaylock = new ReentrantReadWriteLock();
    public static ReadWriteLock chianBondlock = new ReentrantReadWriteLock();
    public static ReadWriteLock partyinfolock = new ReentrantReadWriteLock();
    public static ReadWriteLock partydealerinfolock = new ReentrantReadWriteLock();
    private static final String server = "BaseSvr";
    public static String ServerKey = "SERVER.BaseSvr";
    public static String clear = "stc.base.clear";
    public static String bondinfo = "stc.base.bondsinfo";
    public static String chianbondinfo = "stc.base.chianBond";
    public static String allholiday = "stc.base.allholiday";
    public static String partyInfo = "stc.base.PartyInfo";
    public static String partyDealerInfo = "stc.base.PartyDealerInfo";
    public static boolean isLoadBond = false;
    public static boolean isLoadChianBond = false;
    public static boolean isLoadAllHoliday = false;
    public static boolean isLoadPartyInfo = false;
    public static boolean isLoadPartyDealerInfo = false;
    public static String isSubOrReq = "R";
    public static final Map<String, LocalDate> OFFSET_TRADING_DAY_CACHE =
            new ConcurrentHashMap<String, LocalDate>();

    public static void initialize(
            NetClientWrapper cedaClientWrapper, BaseApi.IMessage<HolidyResult> message)
            throws CodeException {
        htBondinfos.clear();
        tempBondinfos.clear();
        htAllHoliday.clear();
        htChianBondsviews.clear();
        tempChianBondsviews.clear();
        htPartydealerinfoviews.clear();
        tempPartydealerinfoviews.clear();
        htPartyinfoviews.clear();
        tempPartyinfoviews.clear();
        if (isSubOrReq.equals("R")) {
            BaseSvr2Client.isLoadAllHoliday = isLoadAllHoliday;
            BaseSvr2Client.initialize(cedaClientWrapper, message);
        } else {
            BaseSvrClient.cedaClientWrapper = cedaClientWrapper;
            BaseSvrClient.cedaClientWrapper.init(ServerKey, new String[0]);
            BaseSvrClient.subscribeWithImage(
                    ServerKey, server, clear, message, HolidyResult.class, false);
            BaseSvrClient.subBasicsInfo();
        }
    }

    public static void subBasicsInfo() {
        BaseApi.IMessage message2;
        if (isLoadBond) {
            message2 =
                    new BaseApi.IMessage<BondsInfoQueryResult>() {

                        /*
                         * WARNING - Removed try catching itself - possible behaviour change.
                         */
                        @Override
                        public void onMessage(String topic, BondsInfoQueryResult t) {
                            if (t.flag) {
                                if (tempBondinfos.size() != 0) {
                                    tempBondinfos.put(
                                            t.bondInfoViews.get((int) 0).Bond_ID,
                                            t.bondInfoViews.get(0));
                                } else {
                                    bondinfolock.writeLock().lock();
                                    htBondinfos.put(
                                            t.bondInfoViews.get((int) 0).Bond_ID,
                                            t.bondInfoViews.get(0));
                                    bondinfolock.writeLock().unlock();
                                    DataCallBackUtils.notifyCallBack(DataType.BondInfo);
                                }
                            } else {
                                for (BondInfoView bond : t.bondInfoViews) {
                                    tempBondinfos.put(bond.Bond_ID, bond);
                                }
                            }
                            if (t != null && t.lastpage && !t.flag) {
                                bondinfolock.writeLock().lock();
                                try {
                                    htBondinfos.clear();
                                    htBondinfos.putAll(tempBondinfos);
                                    tempBondinfos.clear();
                                    logger.info(
                                            "Bondlistinfo size:{}", (Object) htBondinfos.size());
                                } finally {
                                    bondinfolock.writeLock().unlock();
                                }
                                DataCallBackUtils.notifyCallBack(DataType.BondInfo);
                            }
                        }
                    };
            BaseSvrClient.subscribeWithImage(
                    ServerKey, server, bondinfo, message2, BondsInfoQueryResult.class, false);
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
                                System.gc();
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
                                    logger.warn("holiday_date can't null", (Object) h);
                                }
                            } finally {
                                holidaylock.writeLock().unlock();
                            }
                            DataCallBackUtils.notifyCallBack(DataType.HolidayQuery);
                        }
                    };
            BaseSvrClient.subscribeWithImage(
                    ServerKey, server, allholiday, message4, BrokerHolidayQueryResult.class, false);
        }
        if (isLoadChianBond) {
            message2 =
                    new BaseApi.IMessage<ChinaBondValuationviewQueryResult>() {

                        /*
                         * WARNING - Removed try catching itself - possible behaviour change.
                         */
                        @Override
                        public void onMessage(String topic, ChinaBondValuationviewQueryResult t) {
                            for (TBondValuationView bond : t.Datas) {
                                String key =
                                        BaseSvrClient.produceKey(bond.Bond_ID, bond.Listed_Market);
                                tempChianBondsviews.put(key, bond);
                            }
                            if (t != null && t.lastpage) {
                                bondinfolock.writeLock().lock();
                                try {
                                    htChianBondsviews.clear();
                                    htChianBondsviews.putAll(tempChianBondsviews);
                                    tempChianBondsviews.clear();
                                    logger.info(
                                            "ChianBondlistinfo size:{}",
                                            (Object) htChianBondsviews.size());
                                } finally {
                                    bondinfolock.writeLock().unlock();
                                }
                                DataCallBackUtils.notifyCallBack(DataType.ChianondInfo);
                            }
                        }
                    };
            BaseSvrClient.subscribeWithImage(
                    ServerKey,
                    server,
                    chianbondinfo,
                    message2,
                    ChinaBondValuationviewQueryResult.class,
                    false);
        }
        if (isLoadPartyInfo) {
            message2 =
                    new BaseApi.IMessage<PartyInfoQueryResult>() {

                        /*
                         * WARNING - Removed try catching itself - possible behaviour change.
                         */
                        @Override
                        public void onMessage(String topic, PartyInfoQueryResult t) {
                            for (TInfoGrp tInfoGrp : t.tInfoGrps) {
                                tempPartyinfoviews.put(tInfoGrp.PARTY_CODE6, tInfoGrp);
                            }
                            if (t != null && t.lastpage) {
                                partyinfolock.writeLock().lock();
                                try {
                                    htPartyinfoviews.clear();
                                    htPartyinfoviews.putAll(tempPartyinfoviews);
                                    tempPartyinfoviews.clear();
                                    logger.info(
                                            "htPartyinfoviews size:{}",
                                            (Object) htPartyinfoviews.size());
                                } finally {
                                    partyinfolock.writeLock().unlock();
                                }
                                DataCallBackUtils.notifyCallBack(DataType.Partyinfo);
                            }
                        }
                    };
            BaseSvrClient.subscribeWithImage(
                    ServerKey, server, partyInfo, message2, PartyInfoQueryResult.class, false);
        }
        if (isLoadPartyDealerInfo) {
            message2 =
                    new BaseApi.IMessage<PartyDealerQueryResult>() {

                        /*
                         * WARNING - Removed try catching itself - possible behaviour change.
                         */
                        @Override
                        public void onMessage(String topic, PartyDealerQueryResult t) {
                            for (TDealerInfoGrp tDealerInfoGrp : t.tDealerInfoGrps) {
                                String key =
                                        String.format(
                                                "%s%s",
                                                tDealerInfoGrp.PARTY_CODE6,
                                                tDealerInfoGrp.DEALER_ID);
                                tempPartydealerinfoviews.put(key, tDealerInfoGrp);
                            }
                            if (t != null && t.lastpage) {
                                partydealerinfolock.writeLock().lock();
                                try {
                                    htPartydealerinfoviews.clear();
                                    htPartydealerinfoviews.putAll(tempPartydealerinfoviews);
                                    tempPartydealerinfoviews.clear();
                                    logger.info(
                                            "htPartydealerinfoviews size:{}",
                                            (Object) htPartydealerinfoviews.size());
                                } finally {
                                    partydealerinfolock.writeLock().unlock();
                                }
                                DataCallBackUtils.notifyCallBack(DataType.Partydealerinfo);
                            }
                        }
                    };
            BaseSvrClient.subscribeWithImage(
                    ServerKey,
                    server,
                    partyDealerInfo,
                    message2,
                    PartyDealerQueryResult.class,
                    false);
        }
    }

    public static String produceKey(String bondId, String listedMarket) {
        String key = null;
        if (!TextUtils.isBlank((CharSequence) bondId)) {
            key = bondId;
            if (!bondId.contains(".")) {
                if (listedMarket.equals("CIB")) {
                    key = String.format("%s.%s", bondId, "IB");
                } else if (listedMarket.equals("SSE")) {
                    key = String.format("%s.%s", bondId, "SH");
                } else if (listedMarket.equals("SZE")) {
                    key = String.format("%s.%s", bondId, "SZ");
                }
            }
        } else {
            logger.error("bondinfo is null,so drop[{}]", (Object) bondinfo);
        }
        return key;
    }

    public static QuoteTagDto getQuoteTag(
            String tagType, String childTag, String childTagName, String childTagValue) {
        return BaseSvr2Client.getQuoteTag(tagType, childTag, childTagName, childTagValue);
    }

    public static BondInfoView getBondInfo(String bondId) {
        if (isSubOrReq.equals("R")) {
            return BaseSvr2Client.getBondInfo(bondId);
        }
        bondinfolock.readLock().lock();
        try {
            BondInfoView bondInfoView = htBondinfos.get(bondId);
            return bondInfoView;
        } finally {
            bondinfolock.readLock().unlock();
        }
    }

    public static TBondValuationView getChianBondInfo(String bondId) {
        if (isSubOrReq.equals("R")) {
            return BaseSvr2Client.getChianBondInfo(bondId);
        }
        chianBondlock.readLock().lock();
        try {
            TBondValuationView tBondValuationView = htChianBondsviews.get(bondId);
            return tBondValuationView;
        } finally {
            chianBondlock.readLock().unlock();
        }
    }

    public static TInfoGrp getPartyInfo(String partyCode) {
        if (isSubOrReq.equals("R")) {
            return BaseSvr2Client.getPartyInfo(partyCode);
        }
        partyinfolock.readLock().lock();
        try {
            TInfoGrp tInfoGrp = htPartyinfoviews.get(partyCode);
            return tInfoGrp;
        } finally {
            partyinfolock.readLock().unlock();
        }
    }

    public static TInfoGrp getPartyInfo21(String partyCode21) {
        if (isSubOrReq.equals("R")) {
            return BaseSvr2Client.getPartyInfo21(partyCode21);
        }
        partyinfolock.readLock().lock();
        try {
            TInfoGrp tInfoGrp = htPartyinfoviews.get(partyCode21);
            return tInfoGrp;
        } finally {
            partyinfolock.readLock().unlock();
        }
    }

    /*
     * WARNING - Removed try catching itself - possible behaviour change.
     */
    public static TDealerInfoGrp getPartydealerinfo(String partyCode, String dealerId) {
        if (isSubOrReq.equals("R")) {
            return BaseSvr2Client.getPartydealerinfo(partyCode, dealerId);
        }
        partydealerinfolock.readLock().lock();
        try {
            String key = String.format("%s%s", partyCode, dealerId);
            TDealerInfoGrp tDealerInfoGrp = htPartydealerinfoviews.get(key);
            return tDealerInfoGrp;
        } finally {
            partydealerinfolock.readLock().unlock();
        }
    }

    public static Hashtable<String, Holiday> GetHoliday(String key) {
        if (isSubOrReq.equals("R")) {
            return BaseSvr2Client.GetHoliday(key);
        }
        holidaylock.readLock().lock();
        try {
            Hashtable<String, Holiday> lt;
            Hashtable<String, Holiday> hashtable = lt = htAllHoliday.get(key);
            return hashtable;
        } finally {
            holidaylock.readLock().unlock();
        }
    }

    public static TInstitution getInstitution(String instCode) {
        return BaseSvr2Client.getInstitution(instCode);
    }

    public static boolean IsHoliday(String key, String date) {
        if (isSubOrReq.equals("R")) {
            return BaseSvr2Client.IsHoliday(key, date);
        }
        Hashtable<String, Holiday> lt = BaseSvrClient.GetHoliday(key);
        boolean flag = false;
        if (lt != null) {
            flag = lt.containsKey(date);
        }
        return flag;
    }

    public static boolean IsHoliday(String date) {
        if (isSubOrReq.equals("R")) {
            return BaseSvr2Client.IsHoliday(date);
        }
        String key = "CNY_CIB";
        boolean flag = BaseSvrClient.IsHoliday(key, date);
        return flag;
    }

    public static void clear() {
        htBondinfos.clear();
        tempBondinfos.clear();
        htAllHoliday.clear();
        htChianBondsviews.clear();
        tempChianBondsviews.clear();
        htPartydealerinfoviews.clear();
        tempPartydealerinfoviews.clear();
        htPartyinfoviews.clear();
        tempPartyinfoviews.clear();
        cedaClientWrapper.dispose();
        BaseSvr2Client.cedaClientWrapper.dispose();
        BaseSvr2Client.tagTablesClear();
    }

    public static LocalDate getNextTradeDay(LocalDate date) {
        return BaseSvrClient.getOffsetTradingDay(date, 1, "CNY_CIB");
    }

    public static LocalDate getPrevTradeDay(LocalDate date) {
        return BaseSvrClient.getOffsetTradingDay(date, -1, "CNY_CIB");
    }

    public static LocalDate getOffsetTradingDay(LocalDate date, int offset) {
        if (offset == 0) {
            if (BaseSvrClient.isTradeDay(date).booleanValue()) {
                return date;
            }
            return BaseSvrClient.getPrevTradeDay(date);
        }
        return BaseSvrClient.getOffsetTradingDay(date, offset, "CNY_CIB");
    }

    public static LocalDate getOffsetTradingDay(LocalDate date, int offset, String market) {
        return OFFSET_TRADING_DAY_CACHE.computeIfAbsent(
                StrUtil.format(
                        (CharSequence) "{}-{}", (Object[]) new Object[] {date.toString(), offset}),
                k -> {
                    Map holidayMap =
                            (Map)
                                    Mono.fromCallable(() -> BaseSvrClient.GetHoliday(market))
                                            .switchIfEmpty(
                                                    Mono.just(new Hashtable<String, Holiday>()))
                                            .doOnNext(Assert::notEmpty)
                                            .retryWhen(
                                                    (Retry)
                                                            Retry.backoff(
                                                                            (long) 5L,
                                                                            (Duration)
                                                                                    Duration
                                                                                            .ofSeconds(
                                                                                                    2L))
                                                                    .maxBackoff(
                                                                            Duration.ofSeconds(
                                                                                    10L)))
                                            .onErrorResume(
                                                    e -> {
                                                        logger.warn(
                                                                "\u83b7\u53d6\u8282\u5047\u65e5\u5931\u8d25",
                                                                e);
                                                        return Mono.empty();
                                                    })
                                            .block();
                    if (CollUtil.isEmpty((Map) holidayMap)) {
                        return null;
                    }
                    int limit = Math.abs(offset) + 30;
                    int unitOffset = offset > 0 ? 1 : -1;
                    int count = Math.abs(offset);
                    LocalDate tmpDate = date;
                    while (count > 0) {
                        Holiday holiday =
                                (Holiday)
                                        holidayMap.get(
                                                (tmpDate = tmpDate.plusDays(unitOffset))
                                                        .format(DateTimeFormatter.BASIC_ISO_DATE));
                        if (holiday == null) {
                            --count;
                        }
                        if (--limit > 0) continue;
                        logger.warn(
                                "\u83b7\u53d6\u4ea4\u6613\u65e5\u5931\u8d25\uff0c\u8d85\u8fc7\u6700\u5927\u67e5\u627e\u6b21\u6570\uff0cdate={},"
                                    + " offset={}, market={}",
                                new Object[] {date, offset, market});
                        return null;
                    }
                    return tmpDate;
                });
    }

    public static Boolean isTradeDay(LocalDate date, boolean defaultVal) {
        Boolean res =
                (Boolean)
                        Mono.fromCallable(() -> BaseSvrClient.GetHoliday("CNY_CIB"))
                                .switchIfEmpty(Mono.just(new Hashtable<String, Holiday>()))
                                .doOnNext(Assert::notEmpty)
                                .retryWhen(
                                        (Retry)
                                                Retry.backoff(
                                                                (long) 5L,
                                                                (Duration) Duration.ofSeconds(2L))
                                                        .maxBackoff(Duration.ofSeconds(10L)))
                                .map(
                                        holidayMap ->
                                                !holidayMap.containsKey(
                                                        date.format(
                                                                DateTimeFormatter.BASIC_ISO_DATE)))
                                .onErrorResume(
                                        e -> {
                                            logger.warn(
                                                    "\u83b7\u53d6\u4ea4\u6613\u65e5\u5931\u8d25\uff0c\u9ed8\u8ba4\u8fd4\u56detrue");
                                            return Mono.just(defaultVal);
                                        })
                                .block();
        return BooleanUtil.isTrue((Boolean) res);
    }

    public static Integer getBetweenWorkDay(
            Date startDate, Date endDate, String market, boolean leftClose) {
        if (startDate == null || endDate == null) {
            return null;
        }
        startDate = DateUtil.beginOfDay((Date) startDate);
        int compare =
                DateUtil.compare(
                        (Date) (endDate = DateUtil.beginOfDay((Date) endDate)), (Date) startDate);
        if (compare < 0) {
            return null;
        }
        if (compare == 0) {
            if (BaseSvrClient.isTradeDay(LocalDateTimeUtil.of((Date) startDate).toLocalDate())
                    .booleanValue()) {
                return 0;
            }
            return null;
        }
        long days = DateUtil.betweenDay((Date) startDate, (Date) endDate, (boolean) true);
        int count = 0;
        int index = 0;
        String string = market = StrUtil.isNotBlank((CharSequence) market) ? market : "CNY_CIB";
        if (!leftClose) {
            ++index;
            ++days;
        }
        int i = index;
        while ((long) i < days) {
            DateTime offsetDay = DateUtil.offsetDay((Date) startDate, (int) i);
            if (BaseSvrClient.isTradeDay(LocalDateTimeUtil.of((Date) offsetDay).toLocalDate())
                    .booleanValue()) {
                ++count;
            }
            ++i;
        }
        if (count == 0) {
            Boolean ifTrade1 =
                    BaseSvrClient.isTradeDay(LocalDateTimeUtil.of((Date) startDate).toLocalDate());
            Boolean ifTrade2 =
                    BaseSvrClient.isTradeDay(LocalDateTimeUtil.of((Date) endDate).toLocalDate());
            if (!ifTrade1.booleanValue() && !ifTrade2.booleanValue()) {
                return null;
            }
        }
        logger.info(
                "getBetweenWorkDay startDate:{},endDate:{},market:{},leftClose:{},count:{}",
                new Object[] {startDate, endDate, market, leftClose, count});
        return count;
    }

    public static Integer getBetweenWorkDay(Date startDate, Date endDate, String market) {
        return BaseSvrClient.getBetweenWorkDay(startDate, endDate, market, true);
    }

    public static Boolean isTradeDay(LocalDate date) {
        return BaseSvrClient.isTradeDay(date, false);
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
