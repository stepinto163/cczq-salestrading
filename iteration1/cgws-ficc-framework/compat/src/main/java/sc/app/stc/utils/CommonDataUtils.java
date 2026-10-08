// Recovered compatibility source; provenance: recovered-support.json.
package sc.app.stc.utils;

import ats.app.ym.utils.db.DBUtils;
import ats.app.ym.utils.db.IDatabaseConnection;
import ats.app.ym.utils.logger.AsyncSlf4jLoggerFactory;
import ats.foundation.utils.exception.CodeException;
import ats.message2.CharsetConvertType;
import ats.message2.Exception.SerializerException;
import ats.message2.MsgHelper;

import com.app.common.db.DBPoolResource;

import org.apache.http.util.TextUtils;
import org.slf4j.Logger;

import sc.app.stc.api.Venue;
import sc.app.stc.po.TTbookSystemParameter;
import sc.app.stc.po.TTbookVenueSecurityids;
import sc.app.stc.po.TbookHomeSecurityids;

import java.math.BigDecimal;
import java.sql.Connection;
import java.sql.SQLException;
import java.util.ArrayList;
import java.util.Hashtable;
import java.util.List;
import java.util.Timer;
import java.util.TimerTask;
import java.util.concurrent.locks.ReadWriteLock;
import java.util.concurrent.locks.ReentrantReadWriteLock;

public class CommonDataUtils {
    public static DBPoolResource dbPoolResource;
    private static Logger logger;
    protected static Timer timer;
    private static ReadWriteLock lock;
    static Hashtable<String, TTbookSystemParameter> htSystemParas;
    static Hashtable<String, Hashtable<String, Venue>> veunesHt;
    static Hashtable<String, TTbookVenueSecurityids> htVenueSecurityids;

    public static void init() {
        IDatabaseConnection databaseConnection =
                new IDatabaseConnection() {

                    @Override
                    public Connection getConnection() {
                        try {
                            return dbPoolResource.getConnection();
                        } catch (SQLException sQLException) {
                            return null;
                        }
                    }

                    @Override
                    public void freeConnection(Connection connection) {
                        try {
                            dbPoolResource.freeConnection(connection);
                        } catch (SQLException sQLException) {
                            // empty catch block
                        }
                    }
                };
        DBUtils.setDatabaseConnection(databaseConnection);
    }

    public static void clear() {
        if (timer != null) {
            timer.cancel();
            timer = null;
        }
    }

    public static void loadBase() {
        timer = new Timer("System Paras Update", true);
        TimerTask task =
                new TimerTask() {

                    @Override
                    public void run() {
                        CommonDataUtils.loadParas();
                    }
                };
        timer.schedule(task, Consts.ParasUpdatePeriodTime, (long) Consts.ParasUpdatePeriodTime);
        CommonDataUtils.loadParas();
    }

    public static void loadParas() {
        logger.debug("loadParas begin");
        CommonDataUtils.loadSystemParas();
        CommonDataUtils.loadVenueSecurityids();
        logger.debug("loadParas end");
    }

    public static String getTDVenueTypeGWs() {
        lock.readLock().lock();
        String venueTypeGWs = "";
        try {
            String key = String.format("%s_%s", "STC_TD_VenueTypeGWs", "*");
            TTbookSystemParameter sp = htSystemParas.get(key);
            if (sp != null) {
                venueTypeGWs = sp.param_value;
            }
        } finally {
            lock.readLock().unlock();
        }
        return venueTypeGWs;
    }

    public static String getMDVenueTypeGWs() {
        lock.readLock().lock();
        String venueTypeGWs = "";
        try {
            String key = String.format("%s_%s", "STC_MD_VenueTypeGWs", "*");
            TTbookSystemParameter sp = htSystemParas.get(key);
            if (sp != null) {
                venueTypeGWs = sp.param_value;
            }
        } finally {
            lock.readLock().unlock();
        }
        return venueTypeGWs;
    }

    /*
     * WARNING - Removed try catching itself - possible behaviour change.
     */
    public static List<Venue> getVenuesSetting() {
        lock.readLock().lock();
        List<Venue> lt = new ArrayList<Venue>();
        String venueTypeGWs = "";
        try {
            String key = String.format("%s_%s", "STC_Venues_Setting", "*");
            TTbookSystemParameter sp = htSystemParas.get(key);
            if (sp != null && !TextUtils.isBlank((CharSequence) (venueTypeGWs = sp.param_value))) {
                try {
                    lt =
                            MsgHelper.Deserialize2(
                                    (String) venueTypeGWs,
                                    Venue.class,
                                    (CharsetConvertType) CharsetConvertType.None);
                } catch (SerializerException e) {
                    logger.error("getVenuesSetting error.", (Throwable) e);
                }
            }
        } finally {
            lock.readLock().unlock();
        }
        return lt;
    }

    /*
     * WARNING - Removed try catching itself - possible behaviour change.
     */
    public static void loadSystemParas() {
        List<TTbookSystemParameter> lt =
                DBUtils.queryList(
                        Consts.selectSystemParasSql,
                        null,
                        TTbookSystemParameter.class,
                        new Connection[0]);
        lock.writeLock().lock();
        try {
            for (TTbookSystemParameter sp : lt) {
                String key = String.format("%s_%s", sp.param_key, sp.username);
                htSystemParas.put(key, sp);
                CommonDataUtils.addVenue(sp);
            }
        } finally {
            lock.writeLock().unlock();
        }
    }

    public static TTbookSystemParameter getSystemParas(String key, String username) {
        List<TTbookSystemParameter> lt;
        String paramKey = String.format("%s_%s", key, username);
        TTbookSystemParameter systemParameter = htSystemParas.get(paramKey);
        if (systemParameter == null
                && (lt =
                                        DBUtils.queryList(
                                                Consts.selectUserSystemParasSql,
                                                new Object[] {key, username},
                                                TTbookSystemParameter.class,
                                                new Connection[0]))
                                .size()
                        > 0) {
            systemParameter = lt.get(0);
        }
        return systemParameter;
    }

    public static void cacheSystemParas(List<TTbookSystemParameter> lt) {
        for (TTbookSystemParameter sp : lt) {
            String key = String.format("%s_%s", sp.param_key, sp.username);
            htSystemParas.put(key, sp);
            CommonDataUtils.addVenue(sp);
        }
    }

    /*
     * WARNING - Removed try catching itself - possible behaviour change.
     */
    public static int getSN(String type, String venuetypeGW, String venue) {
        int sn = 10000;
        lock.readLock().lock();
        try {
            Venue v;
            Hashtable<String, Venue> vt = veunesHt.get(type + "");
            if (vt != null && (v = vt.get(venue)) != null) {
                sn = v.sn;
            }
        } finally {
            lock.readLock().unlock();
        }
        return sn;
    }

    /*
     * WARNING - Removed try catching itself - possible behaviour change.
     */
    public static boolean isForbitVenue(String type, String venuetypegw, String venue) {
        lock.readLock().lock();
        try {
            Venue v;
            boolean flag = false;
            Hashtable<String, Venue> ht = veunesHt.get(type + "");
            if (ht != null
                    && (v = ht.get(venue + "")) != null
                    && (0 == v.level || !v.onoffenable && "ODM".equals(venuetypegw))) {
                flag = true;
            }
            boolean bl = flag;
            return bl;
        } finally {
            lock.readLock().unlock();
        }
    }

    /*
     * WARNING - Removed try catching itself - possible behaviour change.
     */
    public static boolean isOnOffEnableVenue(String type, String venuetypegw, String venue) {
        lock.readLock().lock();
        try {
            Venue v;
            boolean flag = true;
            Hashtable<String, Venue> ht = veunesHt.get(type + "");
            if (ht != null && (v = ht.get(venue + "")) != null) {
                flag = v.onoffenable;
            }
            boolean bl = flag;
            return bl;
        } finally {
            lock.readLock().unlock();
        }
    }

    public static void addVenue(TTbookSystemParameter vs) {
        block5:
        {
            List<Venue> venues = null;
            try {
                if (!vs.param_key.equals("STC_Venues_Setting")
                        || TextUtils.isBlank((CharSequence) vs.param_value)) break block5;
                String venueTypeGWs = vs.param_value;
                try {
                    venues =
                            MsgHelper.Deserialize2(
                                    (String) venueTypeGWs,
                                    Venue.class,
                                    (CharsetConvertType) CharsetConvertType.None);
                    veunesHt.clear();
                    for (Venue venue : venues) {
                        Hashtable<String, Venue> venueht =
                                veunesHt.getOrDefault(venue.type, new Hashtable());
                        venueht.put(venue.venue, venue);
                        veunesHt.put(venue.type, venueht);
                    }
                } catch (SerializerException e) {
                    logger.error("getSystemVenuesSetting error.", (Throwable) e);
                }
            } catch (Exception e) {
                logger.error("addVenue", (Throwable) e);
            }
        }
    }

    /*
     * WARNING - Removed try catching itself - possible behaviour change.
     */
    public static void loadVenueSecurityids() {
        List<TTbookVenueSecurityids> lt =
                DBUtils.queryList(
                        Consts.selectVenueSecurityIdsSql,
                        null,
                        TTbookVenueSecurityids.class,
                        new Connection[0]);
        lock.writeLock().lock();
        try {
            for (TTbookVenueSecurityids sp : lt) {
                String key = String.format("%s_%s_%s", sp.venuetypegw, sp.venues, sp.securityid);
                htVenueSecurityids.put(key, sp);
            }
            if (!htVenueSecurityids.containsKey("IMM")) {
                String key = String.format("%s_%s_%s", "IMM", "*", "*");
                htVenueSecurityids.put(key, CommonDataUtils.getIMMVenueSecurityIds());
            }
        } finally {
            lock.writeLock().unlock();
        }
    }

    public static TTbookVenueSecurityids getIMMVenueSecurityIds() {
        return new TTbookVenueSecurityids(
                "999",
                "IMM",
                "*",
                "*",
                "\u6240\u6709",
                new BigDecimal("1.00000000"),
                new BigDecimal("10000000000.00000000"),
                new BigDecimal("1.00000000"),
                new BigDecimal("1.00000000"),
                "1");
    }

    /*
     * WARNING - Removed try catching itself - possible behaviour change.
     */
    public static TTbookVenueSecurityids getVenueSecurityids(
            String venueTypeGw, String venue, String securityid) {
        lock.readLock().lock();
        TTbookVenueSecurityids vs = null;
        try {
            String key = String.format("%s_%s_%s", venueTypeGw, venue, securityid);
            vs = htVenueSecurityids.get(key);
            if (vs == null) {
                key = String.format("%s_%s_%s", venueTypeGw, venue, "*");
                vs = htVenueSecurityids.get(key);
            }
        } finally {
            lock.readLock().unlock();
        }
        return vs;
    }

    public static void checkOrdQty(
            String venueTypeGw, String venue, String securityid, String ordQty)
            throws CodeException {
        TTbookVenueSecurityids vs =
                CommonDataUtils.getVenueSecurityids(venueTypeGw, venue, securityid);
        if (vs != null) {
            BigDecimal qty = new BigDecimal(ordQty);
            BigDecimal maxQty = vs.contractsize.multiply(vs.maxsize);
            BigDecimal minQty = vs.contractsize.multiply(vs.minsize);
            BigDecimal stepQty = vs.contractsize.multiply(vs.stepvalue);
            if (qty.compareTo(minQty) < 0) {
                throw new CodeException(Consts.ArgErrorCode, "OrdQty can't be less than " + minQty);
            }
            if (qty.compareTo(maxQty) > 0) {
                throw new CodeException(
                        Consts.ArgErrorCode, "OrdQty can't be more  than " + maxQty);
            }
            BigDecimal re = qty.divideAndRemainder(stepQty)[1];
            if (re.compareTo(BigDecimal.ZERO) != 0) {
                throw new CodeException(Consts.ArgErrorCode, "OrdQty can't divide " + minQty);
            }
        }
    }

    public static List<TbookHomeSecurityids> getUserGroupSecurityIds(String focusId) {
        List<TbookHomeSecurityids> list =
                DBUtils.queryList(
                        "select * from stc_tbook_home_securityids where groupid = ?",
                        new Object[] {focusId},
                        TbookHomeSecurityids.class,
                        new Connection[0]);
        return list;
    }

    static {
        logger = AsyncSlf4jLoggerFactory.getLogger(CommonDataUtils.class);
        timer = null;
        lock = new ReentrantReadWriteLock();
        htSystemParas = new Hashtable();
        veunesHt = new Hashtable();
        htVenueSecurityids = new Hashtable();
    }
}
