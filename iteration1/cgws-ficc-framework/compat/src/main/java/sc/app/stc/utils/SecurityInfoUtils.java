// Recovered compatibility source; provenance: recovered-support.json.
package sc.app.stc.utils;

import com.google.common.collect.Maps;

import org.apache.commons.codec.digest.DigestUtils;
import org.apache.commons.io.IOUtils;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import sc.app.stc.api.base.po.BondInfoView;
import sc.app.stc.po.SecurityIDPo;

import java.io.FileInputStream;
import java.io.InputStream;
import java.util.List;
import java.util.Map;
import java.util.concurrent.locks.ReentrantReadWriteLock;

public class SecurityInfoUtils {
    private static Logger logger = LoggerFactory.getLogger(SecurityInfoUtils.class);
    public static String OrderMd5 = "";
    public static String NewMd5 = "";
    public static boolean isBase = false;
    private static Map<String, SecurityIDPo> imixSecurityIds = Maps.newHashMap();
    private static Map<String, SecurityIDPo> securityIds = Maps.newHashMap();
    private static final ReentrantReadWriteLock READ_WRITE_LOCK = new ReentrantReadWriteLock();

    public static void load(String securityIdPath) {
        try {
            if (!isBase
                    && !OrderMd5.equals(
                            NewMd5 = SecurityInfoUtils.getSecurityMd5(securityIdPath))) {
                List<SecurityIDPo> list = CsvUtil.read(securityIdPath, SecurityIDPo.class);
                SecurityInfoUtils.load(list);
                OrderMd5 = NewMd5;
            }
        } catch (Exception e) {
            logger.error("csv read error:{}", (Object) (e.getMessage() + e));
        }
    }

    public static void load(List<SecurityIDPo> list) {
        READ_WRITE_LOCK.writeLock().lock();
        try {
            imixSecurityIds.clear();
            securityIds.clear();
            for (SecurityIDPo po : list) {
                imixSecurityIds.put(po.getImixSecurityId(), po);
                securityIds.put(po.getSecurityId(), po);
            }
        } catch (Exception e) {
            logger.error("load SecurityIDPo error:{}", (Object) (e.getMessage() + e));
        } finally {
            READ_WRITE_LOCK.writeLock().unlock();
        }
    }

    /*
     * WARNING - Removed try catching itself - possible behaviour change.
     */
    public static String getSecurityId(String imixSecurityId, String gw) {
        READ_WRITE_LOCK.readLock().lock();
        String securityId = imixSecurityId;
        try {
            if (imixSecurityIds.containsKey(imixSecurityId)) {
                securityId = imixSecurityIds.get(imixSecurityId).getSecurityId();
            } else {
                BondInfoView bondInfoView;
                if ("XXX".equals(gw)) {
                    securityId = securityId + ".SZ";
                } else if (!("CTP".equals(gw)
                        || "SSE".equals(gw)
                        || "SZE".equals(gw)
                        || "EXC".equals(gw))) {
                    securityId = securityId + ".IB";
                }
                if (isBase && (bondInfoView = BaseSvrClient.getBondInfo(securityId)) != null) {
                    SecurityIDPo securityIDPo = new SecurityIDPo();
                    securityIDPo.setFlag(bondInfoView.flag);
                    securityIDPo.setSecurityId(bondInfoView.getBond_ID());
                    securityIDPo.setImixSecurityId(bondInfoView.Bond_ID_Market);
                    securityIDPo.setSecurityDec(bondInfoView.SECURITY_DEC);
                    securityIDPo.setSymbol(bondInfoView.SYMBOL);
                    securityIDPo.setIssuer(bondInfoView.ISSUER);
                    securityIDPo.setSecurityTypeId(bondInfoView.SECURITY_TYPE_ID);
                    securityIDPo.setTermToMaturity(bondInfoView.TERM_TO_MATURITY_STRING);
                    securityIDPo.setTradePublishIndicator(bondInfoView.TRADE_PUBLISH_INDICATOR);
                    imixSecurityIds.put(securityIDPo.getImixSecurityId(), securityIDPo);
                    securityIds.put(securityIDPo.getSecurityId(), securityIDPo);
                    securityId = bondInfoView.Bond_ID;
                }
            }
        } finally {
            READ_WRITE_LOCK.readLock().unlock();
        }
        return securityId;
    }

    /*
     * WARNING - Removed try catching itself - possible behaviour change.
     */
    public static String getImixSecurityId(String securityId, String gw) {
        READ_WRITE_LOCK.readLock().lock();
        try {
            if (securityIds.containsKey(securityId)) {
                securityId = securityIds.get(securityId).getImixSecurityId();
            } else if (isBase) {
                BondInfoView bondInfoView = BaseSvrClient.getBondInfo(securityId);
                if (bondInfoView != null) {
                    SecurityIDPo securityIDPo = new SecurityIDPo();
                    securityIDPo.setFlag(bondInfoView.flag);
                    securityIDPo.setSecurityId(bondInfoView.getBond_ID());
                    securityIDPo.setImixSecurityId(bondInfoView.Bond_ID_Market);
                    securityIDPo.setSecurityDec(bondInfoView.SECURITY_DEC);
                    securityIDPo.setSymbol(bondInfoView.SYMBOL);
                    securityIDPo.setIssuer(bondInfoView.ISSUER);
                    securityIDPo.setSecurityTypeId(bondInfoView.SECURITY_TYPE_ID);
                    securityIDPo.setTermToMaturity(bondInfoView.TERM_TO_MATURITY_STRING);
                    securityIDPo.setTradePublishIndicator(bondInfoView.TRADE_PUBLISH_INDICATOR);
                    imixSecurityIds.put(securityIDPo.getImixSecurityId(), securityIDPo);
                    securityIds.put(securityIDPo.getSecurityId(), securityIDPo);
                } else if ("XXX".equals(gw)) {
                    securityId = securityId.replace(".SZ", "");
                } else if ("CTP".equals(gw)
                        || "SSE".equals(gw)
                        || "SZE".equals(gw)
                        || "EXC".equals(gw)) {
                } else {
                    securityId = securityId.replace(".IB", "");
                }
            }
        } finally {
            READ_WRITE_LOCK.readLock().unlock();
        }
        return securityId;
    }

    /*
     * WARNING - Removed try catching itself - possible behaviour change.
     */
    public static SecurityIDPo getSecurityIDPo(String securityId) {
        READ_WRITE_LOCK.readLock().lock();
        try {
            BondInfoView bondInfoView;
            if (securityIds.containsKey(securityId)) {
                SecurityIDPo securityIDPo = securityIds.get(securityId);
                return securityIDPo;
            }
            if (isBase && (bondInfoView = BaseSvrClient.getBondInfo(securityId)) != null) {
                SecurityIDPo securityIDPo = new SecurityIDPo();
                securityIDPo.setFlag(bondInfoView.flag);
                securityIDPo.setSecurityId(bondInfoView.getBond_ID());
                securityIDPo.setImixSecurityId(bondInfoView.Bond_ID_Market);
                securityIDPo.setSecurityDec(bondInfoView.SECURITY_DEC);
                securityIDPo.setSymbol(bondInfoView.SYMBOL);
                securityIDPo.setIssuer(bondInfoView.ISSUER);
                securityIDPo.setSecurityTypeId(bondInfoView.SECURITY_TYPE_ID);
                securityIDPo.setTermToMaturity(bondInfoView.TERM_TO_MATURITY_STRING);
                securityIDPo.setTradePublishIndicator(bondInfoView.TRADE_PUBLISH_INDICATOR);
                imixSecurityIds.put(securityIDPo.getImixSecurityId(), securityIDPo);
                securityIds.put(securityIDPo.getSecurityId(), securityIDPo);
                SecurityIDPo securityIDPo2 = securityIDPo;
                return securityIDPo2;
            }
        } finally {
            READ_WRITE_LOCK.readLock().unlock();
        }
        return null;
    }

    public static String getSecurityMd5(String path) {
        FileInputStream fis = null;
        String md5 = null;
        try {
            fis = new FileInputStream(path);
            md5 = DigestUtils.md5Hex((byte[]) IOUtils.toByteArray((InputStream) fis));
            IOUtils.closeQuietly((InputStream) fis);
        } catch (Exception e) {
            e.printStackTrace();
        }
        return md5;
    }

    public static void clear() {
        imixSecurityIds.clear();
        securityIds.clear();
    }

    public static void clearBySecurityId(String securityId) {
        READ_WRITE_LOCK.readLock().lock();
        try {
            if (securityIds.containsKey(securityId)) {
                String imixSecurityId = securityIds.get(securityId).getImixSecurityId();
                if (imixSecurityIds.containsKey(imixSecurityId)) {
                    imixSecurityIds.remove(imixSecurityId);
                }
                securityIds.remove(securityId);
            }
        } finally {
            READ_WRITE_LOCK.readLock().unlock();
        }
    }
}
