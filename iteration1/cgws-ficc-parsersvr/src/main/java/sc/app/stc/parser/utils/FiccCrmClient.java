package sc.app.stc.parser.utils;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import sc.app.stc.api.TBrokerCrmContact;
import sc.app.stc.api.TBrokerCrmInst;
import sc.app.stc.net.NetClientWrapper;

import java.util.Collections;
import java.util.List;

/**
 * CRM lookups for parsersvr, served by CRMSvr over the STC bus.
 *
 * <p>Replaces {@code sc.app.stc.utils.CRMSvrClient}. That client subscribed to the STC CRM image,
 * kept every institution and trader in this JVM and dropped rows whose {@code taglist}/
 * {@code biztype} missed the bond tags — with the FICC CRM behind it, nothing survived those
 * filters, so company and trader stayed unresolved. This client asks CRMSvr for the rows it needs,
 * which keeps the CRM schema and the tag filtering on the CRMSvr side.
 *
 * <p>Every lookup degrades to "not found": when CRMSvr is unreachable the parsed text is kept as
 * it is, the same as before the CRM connection existed.
 */
public final class FiccCrmClient {

    private static final Logger logger = LoggerFactory.getLogger(FiccCrmClient.class);

    private static final String DEFAULT_SERVER_KEY = "SERVER.CRMSvr";
    private static final long DEFAULT_TIMEOUT_MILLIS = 3000L;
    private static final int INSTITUTION_PAGE_SIZE = 50;
    private static final int CONTACT_PAGE_SIZE = 500;

    private static NetClientWrapper network;
    private static String serverKey = DEFAULT_SERVER_KEY;
    private static long timeoutMillis = DEFAULT_TIMEOUT_MILLIS;

    private FiccCrmClient() {}

    /** Binds the client to CRMSvr; called once when {@code CRMConnSwitch=1}. */
    public static synchronized void initialize(
            NetClientWrapper wrapper, String key, long timeout) {
        network = wrapper;
        serverKey = key == null || key.trim().isEmpty() ? DEFAULT_SERVER_KEY : key.trim();
        timeoutMillis = timeout <= 0 ? DEFAULT_TIMEOUT_MILLIS : timeout;
        if (network != null) {
            try {
                network.init(serverKey);
            } catch (Exception error) {
                logger.warn("CRM client could not pre-connect " + serverKey, error);
            }
        }
        logger.info(
                "CRM client ready: serverKey="
                        + serverKey
                        + ", timeoutMillis="
                        + timeoutMillis
                        + ", clientBound="
                        + (wrapper != null)
                        + ", online="
                        + isOnline());
    }

    /** Institution whose short name equals {@code name}; null when CRM has no such institution. */
    public static TBrokerCrmInst getInstByName(String name) {
        if (isBlank(name)) {
            return null;
        }
        List<TBrokerCrmInst> rows =
                institutions(null, name, FiccCrmProtocol.EXACT, INSTITUTION_PAGE_SIZE);
        if (rows.size() > 1) {
            logger.info("CRM institution name is not unique: name={}, matches={}", name, rows.size());
        }
        return rows.isEmpty() ? null : rows.get(0);
    }

    public static TBrokerCrmInst getInstByIDBkey(String idbkey) {
        if (isBlank(idbkey)) {
            return null;
        }
        List<TBrokerCrmInst> rows =
                institutions(idbkey, null, FiccCrmProtocol.EXACT, INSTITUTION_PAGE_SIZE);
        return rows.isEmpty() ? null : rows.get(0);
    }

    /** Traders whose name contains {@code name}; null when none matches, as CRMSvrClient did. */
    public static List<TBrokerCrmContact> getContactByNameMatch(String name) {
        if (isBlank(name)) {
            return null;
        }
        List<TBrokerCrmContact> rows =
                contacts(null, name, FiccCrmProtocol.CONTAINS, CONTACT_PAGE_SIZE);
        return rows.isEmpty() ? null : rows;
    }

    public static TBrokerCrmContact getContact(String idbkey) {
        if (isBlank(idbkey)) {
            return null;
        }
        List<TBrokerCrmContact> rows =
                contacts(idbkey, null, FiccCrmProtocol.CONTAINS, CONTACT_PAGE_SIZE);
        return rows.isEmpty() ? null : rows.get(0);
    }

    /** True only when CRMSvr is actually reachable on the bus; false while it is down. */
    public static boolean isOnline() {
        if (network == null) {
            return false;
        }
        try {
            return network.isOnline(serverKey);
        } catch (Exception error) {
            return false;
        }
    }

    private static List<TBrokerCrmInst> institutions(
            String id, String name, String match, int pageSize) {
        String reply =
                request(
                        FiccCrmProtocol.LOOKUP_TOPIC,
                        FiccCrmProtocol.lookupRequest(
                                FiccCrmProtocol.INSTITUTION, name, id, match, pageSize));
        return reply == null
                ? Collections.<TBrokerCrmInst>emptyList()
                : FiccCrmProtocol.institutions(reply);
    }

    private static List<TBrokerCrmContact> contacts(
            String id, String name, String match, int pageSize) {
        String reply =
                request(
                        FiccCrmProtocol.LOOKUP_TOPIC,
                        FiccCrmProtocol.lookupRequest(
                                FiccCrmProtocol.CONTACT, name, id, match, pageSize));
        return reply == null
                ? Collections.<TBrokerCrmContact>emptyList()
                : FiccCrmProtocol.contacts(reply);
    }

    private static String request(String topic, String content) {
        if (network == null) {
            logger.warn("CRM lookup skipped, client is not initialized: topic=" + topic);
            return null;
        }
        long started = System.currentTimeMillis();
        try {
            String reply = network.requestSync(serverKey, null, topic, content, timeoutMillis);
            long elapsed = System.currentTimeMillis() - started;
            if (elapsed > 200) {
                logger.info(
                        "CRM lookup slow: topic=" + topic + ", elapsedMillis=" + elapsed
                                + ", content=" + content);
            }
            return reply;
        } catch (Exception error) {
            logger.warn(
                    "CRM lookup failed: serverKey="
                            + serverKey
                            + ", topic="
                            + topic
                            + ", content="
                            + content
                            + ", elapsedMillis="
                            + (System.currentTimeMillis() - started)
                            + ", error="
                            + error.toString());
            return null;
        }
    }

    private static boolean isBlank(String value) {
        return value == null || value.trim().isEmpty();
    }
}
