package sc.app.stc.parser.stream;

import ats.app.ym.utils.db.DBUtils;
import cn.hutool.core.collection.CollUtil;
import cn.hutool.core.convert.Convert;
import cn.hutool.core.map.multi.RowKeyTable;
import cn.hutool.core.map.multi.Table;
import cn.hutool.core.thread.ThreadUtil;
import cn.hutool.core.util.NumberUtil;
import cn.hutool.core.util.StrUtil;
import cn.hutool.json.JSONUtil;
import com.alibaba.fastjson.JSONObject;
import com.google.common.collect.ImmutableMap;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.util.CollectionUtils;
import sc.app.stc.api.ServerApi;
import sc.app.stc.api.base.po.BondInfoView;
import sc.app.stc.api.base.po.TBondValuationView;
import sc.app.stc.api.base.po.TProvinceInfo;
import sc.app.stc.parser.base.Constants;
import sc.app.stc.utils.*;

import java.math.BigDecimal;
import java.sql.Connection;
import java.util.*;
import java.util.concurrent.ConcurrentHashMap;
import java.util.concurrent.locks.ReadWriteLock;
import java.util.concurrent.locks.ReentrantReadWriteLock;

/**
 * 数据库管理
 *
 * @author deshuai.kong
 */
public class CommonDataManager extends CommonDataUtils {

    private static Logger logger = LoggerFactory.getLogger(CommonDataManager.class);
    public static Table<String, String, Map<String, Object>> infoTable = new RowKeyTable<>(new ConcurrentHashMap<>(), ConcurrentHashMap::new);

    private static ReadWriteLock lock = new ReentrantReadWriteLock();
    public static boolean isLoaded = false;

    public synchronized static void load() {
        isLoaded = false;
        if (mo == null) {
            mo = new ExecSqlBatchTask(Consts.DBThreadNum);// 多线程批量数据任务
        }
        if (moOps == null) {
            moOps = new ExecSqlOpLogTask(Constants.OpLogDBThreadNum);// 多线程日志任务
        }
        loadBase();// 加载基础数据
        isLoaded = true;
        try {
            ThreadUtil.sleep(10000);
            DataCallBackUtils.notifyCallBack(DataType.Load);
        } catch (Exception e) {
            e.printStackTrace();
        }
    }


    public static void loadBase() {
        timer = new Timer("System Paras Update", true);
        TimerTask task = new TimerTask() {

            @Override
            public void run() {
                try {
                    loadParas();
                } catch (Exception e) {
                    logger.error("{}", e);
                }
            }
        };

        timer.schedule(task, Consts.ParasUpdatePeriodTime, Consts.ParasUpdatePeriodTime);
        loadParas();
    }


    public static List<TProvinceInfo> selectProvinceInfo() {
        List<TProvinceInfo> list = DBUtils.queryList(Constants.selectProvinceInfoSql, new Object[]{}, TProvinceInfo.class);
        return list;
    }


    public static class ExecSql {

        public String Sql;
        public Object[] args;
    }

    static ExecSqlBatchTask mo = null;

    private static void onExecSql(String id, List<ExecSql> ess) {
        mo.add(id, ess);
    }

    public static void onExecSql(String id, ExecSql es) {
        List<ExecSql> ess = new ArrayList<ExecSql>();
        ess.add(es);
        onExecSql(id, ess);
    }

    public static class ExecSqlBatchTask extends AsyncSeqThreadGroup<List<ExecSql>> {

        public ExecSqlBatchTask(int asyncBaseNum) {
            super(asyncBaseNum);
            // TODO Auto-generated constructor stub
        }

        @Override
        protected void process(List<ExecSql> ess) {

            Connection con = null;

            if (ess.size() == 1) {
                ExecSql es = ess.get(0);
                String sql = es.Sql;
                Object[] args = es.args;
                try {

                    int i = DBUtils.update(sql, args);
                    logger.debug("sql:{} args:{} {}", sql, args, i > 0 ? "success" : "no found data");
                } catch (Exception e) {
                    try {
                        logger.warn("sql:{} args:{} {}", sql, args, "retry.");
                        int i = DBUtils.update(sql, args);
                        logger.debug("sql:{} args:{} {}", sql, args, i > 0 ? "success" : "no found data");
                    } catch (Exception e1) {

                        logger.error("sql:{} args:{},e:{}", sql, args, e);

                    }

                }
            } else {
                try {

                    con = DBUtils.getDatabaseConnection().getConnection();
                    con.setAutoCommit(false);
                    for (ExecSql es : ess) {
                        String sql = es.Sql;
                        Object[] args = es.args;
                        try {
                            int i = DBUtils.update(sql, args, con);
                            logger.debug("sql:{} args:{} {}", sql, args, i > 0 ? "success" : "no found data");
                        } catch (Exception e) {
                            try {
                                logger.warn("sql:{} args:{} {}", sql, args, "retry.");
                                int i = DBUtils.update(sql, args, con);
                                logger.debug("sql:{} args:{} {}", sql, args, i > 0 ? "success" : "no found data");
                            } catch (Exception e1) {

                                logger.error("sql:{} args:{},e:{}", sql, args, e);
                                throw e;
                            }

                        }
                    }

                    con.commit();
                } catch (Exception e) {

                    if (con != null) {
                        try {
                            con.rollback();
                        } catch (Exception e1) {

                        }
                    }

                } finally {
                    if (con != null) {
                        try {

                            DBUtils.getDatabaseConnection().freeConnection(con);
                        } catch (Exception e1) {

                        }
                    }
                }

            }

        }
    }

    static ExecSqlOpLogTask moOps = null;

    public static class ExecSqlOpLogTask extends AsyncCacheThreadGroup<ExecSql> {

        public ExecSqlOpLogTask(int maxSize) {

            super(maxSize);

        }

        protected void process(ExecSql es) {

            String sql = es.Sql;
            Object[] args = es.args;
            try {

                int i = DBUtils.update(sql, args);
                logger.debug("sql:{} args:{} {}", sql, args, i > 0 ? "success" : "no found data");
            } catch (Exception e) {
                try {
                    logger.warn("sql:{} args:{} {}", sql, args, "retry.");
                    int i = DBUtils.update(sql, args);
                    logger.debug("sql:{} args:{} {}", sql, args, i > 0 ? "success" : "no found data");
                } catch (Exception e1) {
                    logger.error("sql:{} args:{},e:{}", sql, args, e);
                }

            }
        }

    }

    public static void clear() {
        if (timer != null) {
            timer.cancel();
            timer = null;
        }
        isLoaded = false;
        if (mo != null) {
            mo.dispose();
            mo = null;
        }
        if (moOps != null) {
            moOps.dispose();
            moOps = null;
        }
    }

    public static BondInfoView getBondInfo(String securityId) {
        if (StrUtil.isBlank(securityId)) {
            return null;
        }
        return BaseSvr2Client.getBondInfo(securityId);
    }

    /**
     * 获取中债估值
     *
     * @param securityId
     * @return String
     */
    public static BigDecimal getCbv(String securityId) {
        try {
            TBondValuationView cBond = BaseSvrClient.getChianBondInfo(securityId);
            if (cBond == null) {
                cBond = BaseSvrClient.getChianBondInfo(securityId);
            }
            if (cBond != null && cBond.valuation_yield != null) {
                // cBond.
                if (logger.isDebugEnabled()) {
                    logger.debug("{} getCbv value {}", securityId, JSONObject.toJSONString(cBond));
                }
                String cvbStr = cBond.valuation_yield.split("\\|")[0];
                if (StrUtil.isBlank(cvbStr)) {
                    return null;
                }
                return NumberUtil.toBigDecimal(cvbStr);
            } else {
                return null;
            }
        } catch (Exception e) {
            logger.error("getCbv error", e.getMessage(), e);
            return null;
        }
    }

    public static Map<String, Object> getExtraParams(BondInfoView bondInfo) {
        if (null == bondInfo) {
            return new HashMap<>();
        }
        String bondType = BondUtils.getSymbolType(bondInfo);
        if (Consts.BOND_TYPE_VIRTUAL.equals(bondType)) {
            return new HashMap<>();
        }
        Integer dayCount = BondUtils.getTermDays(bondInfo.TimeToMaturity);
        BigDecimal chinaBondVal = getCbv(bondInfo.Bond_ID);
        String chinaBondValFull = getCbvFull(bondInfo.Bond_ID);
        TBondValuationView cBond = BaseSvrClient.getChianBondInfo(bondInfo.getBond_ID());
        Map<String, Object> paramMap = new HashMap<>();
        //剩余期限
        paramMap.put("termDays", BigDecimal.valueOf(dayCount));
        paramMap.put("tenor", bondInfo.getTimeToMaturity());
        //剩余期限(休N)
        paramMap.put("dayOffStr", bondInfo.getTimeToMaturityToWorkDay());
        paramMap.put("optionDate", StrUtil.toStringOrNull(bondInfo.Option_Date));
        paramMap.put("optionType", StrUtil.toStringOrNull(bondInfo.Option_Type));
        //中债估值
        paramMap.put("chinaBondValuation", chinaBondVal);
        //中债估值全价
        paramMap.put("chinaBondValuationFull", chinaBondValFull);
        //中证估值
        paramMap.put("csiBondValuation", getCsiBondValuation(bondInfo.Bond_ID));
        paramMap.put("csiBondValuationFull", getCsiBondValuationFull(bondInfo.Bond_ID));
        paramMap.put("bondSubType", bondInfo.Bond_Subtype);
        //发行人
        paramMap.put("issuerCode", bondInfo.Issuer_Code);
        if (StrUtil.isNotBlank(bondInfo.Issuer_Code)) {
            Map<String, Object> issuer = getInfoTableVal("institution", bondInfo.Issuer_Code);
            if (!CollectionUtils.isEmpty(issuer)) {
                paramMap.put("issuerName", StrUtil.toStringOrNull(issuer.get("Full_Name_C")));
            }
        }

        //担保人
        paramMap.put("warranterCode", bondInfo.Warranter);
        if (StrUtil.isNotBlank(bondInfo.Warranter)) {
            List<String> warranters = new ArrayList<>();
            StrUtil.split(bondInfo.Warranter, "|").forEach(i -> {
                Map<String, Object> warranter = getInfoTableVal("institution", i);
                if (CollUtil.isNotEmpty(warranter)) {
                    warranters.add(StrUtil.toStringOrNull(warranter.get("Full_Name_C")));
                }
            });
            CollUtil.removeBlank(warranters);
            if (CollUtil.isNotEmpty(warranters)) {
                paramMap.put("warranterName", CollUtil.join(warranters, ";"));
            }
        }
        //最新票面利率
        paramMap.put("couponRateCurrent", bondInfo.Coupon_Rate_Current);
        //发行总量
        paramMap.put("issueAmount", bondInfo.Issue_Amount);
        paramMap.put("inf3", getExhibitionBondField(bondInfo, cBond));
        if (logger.isDebugEnabled()) {
            logger.debug("getExtraParams paramMap:{}", paramMap);
        }
        return paramMap;
    }

    public static String getCbvFull(String securityId) {
        try {
            TBondValuationView cBond = BaseSvrClient.getChianBondInfo(securityId);
            if (cBond == null) {
                cBond = BaseSvrClient.getChianBondInfo(securityId);
            }
            if (cBond != null && cBond.valuation_yield != null) {
                // cBond.
                if (logger.isDebugEnabled()) {
                    logger.debug("{} getCbv value {}", securityId, JSONObject.toJSONString(cBond));
                }
                String cvbStr = cBond.valuation_yield2;
                if (StrUtil.isBlank(cvbStr)) {
                    return null;
                }
                return cvbStr;
            } else {
                return null;
            }
        } catch (Exception e) {
            logger.error("getCbvFull error", e.getMessage(), e);
            return null;
        }
    }

    public static BigDecimal getCsiBondValuation(String securityId) {

        String csiBondVal = BaseSvr2Client.getCsiBondValuation(securityId);
        if (logger.isDebugEnabled()) {
            logger.debug("get csiBondValuation:{}", csiBondVal);
        }
        if (StrUtil.isNotBlank(csiBondVal)) {
            return NumberUtil.toBigDecimal(StrUtil.splitToArray(csiBondVal, "|")[0]);
        }
        return BigDecimal.ZERO;
    }

    public static String getCsiBondValuationFull(String securityId) {
        String csiBondVal = BaseSvr2Client.getCsiBondValuation(securityId);
        if (StrUtil.isNotBlank(csiBondVal)) {
            return csiBondVal;
        } else {
            return "";
        }
    }

    public static Map<String, Object> getInfoTableVal(String tableName, String keyValue) {

        Map<String, Object> valMap = infoTable.get(tableName, keyValue);
        try {
            if (CollUtil.isEmpty(valMap)) {
                Map<String, String> reqMap = ImmutableMap.of("TableName", tableName, "KeyValue", keyValue);
                List<Map> response = ServerApi.requestSync("SERVER.BaseSvr", "stc.base.info.query", JSONObject.toJSONString(reqMap), Map.class, false, false, true);
                if (CollUtil.isNotEmpty(response)) {
                    Map<String, Object> map = response.get(0);
                    if (map.containsKey("Datas")) {
                        valMap = Convert.toMap(String.class, Object.class, map.get("Datas"));
                        infoTable.put(tableName, keyValue, valMap);
                    }
                }
            }
        } catch (Exception e) {
            throw new RuntimeException(e);
        }
        return valMap;


    }

    /**
     * 将拓展字段转换成JSON字符串
     *
     * @param bondInfo
     * @param cBond
     * @return
     */
    private static String getExhibitionBondField(BondInfoView bondInfo, TBondValuationView cBond) {
        Map<String, String> map = new HashMap<>();
        if (bondInfo.getOutstanding_Amount() != null) {
            map.put("Outstanding_Amount", bondInfo.getOutstanding_Amount().toPlainString());
        }
        if (cBond != null) {
            map.put("valuation_date", cBond.valuation_date);
            if (cBond.Val_Modified_Duration != null) {
                //修正久期
                map.put("Val_Modified_Duration", cBond.Val_Modified_Duration.toPlainString());
            }
        }
        if (bondInfo.getIssue_Amount() != null) {
            //发行量(万转换成亿）
            map.put("Issue_Amount", NumberUtil.div(bondInfo.getIssue_Amount(), new BigDecimal(10000)).toPlainString());
        }
        if (StrUtil.isNotBlank(BondUtils.getLocalBondType(bondInfo))) {
            //地方债类型
            map.put("LOCAL_GOV_BOND_TYPE", BondUtils.getLocalBondType(bondInfo));
        }

        return JSONUtil.toJsonStr(map);
    }

    public static void clearCache() {
        infoTable.clear();
    }

}
