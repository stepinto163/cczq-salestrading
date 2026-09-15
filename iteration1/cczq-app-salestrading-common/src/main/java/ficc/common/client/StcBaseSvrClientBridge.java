package ficc.common.client;

import ats.app.api.BaseApi.IMessage;
import com.alibaba.fastjson2.JSONObject;
import ficc.common.ApiException;
import ficc.common.JsonUtils;
import java.lang.reflect.InvocationTargetException;
import java.lang.reflect.Method;
import java.util.Properties;
import java.util.Set;
import org.apache.log4j.Logger;
import sc.app.stc.net.NetClientWrapper;

/** Reuses the deployment's STC client and adds dictionary search in our adapter. */
final class StcBaseSvrClientBridge implements StcBaseDataClient.Source {
    private static final Logger LOGGER = Logger.getLogger(StcBaseSvrClientBridge.class);
    private final NetClientWrapper network;
    private final String serverKey;
    private Class<?> requestClient;
    private Method getBond;
    private Method getValuation;
    private Method getIssuer;
    private final long searchTimeoutMillis;
    private StcTransport searchTransport;

    StcBaseSvrClientBridge(NetClientWrapper network, Properties configuration) {
        this.network = network;
        serverKey = configuration.getProperty("base.serverKey", "SERVER.BaseSvr");
        searchTimeoutMillis = Long.parseLong(configuration.getProperty("base.requestTimeoutMillis", "3000"));
        if (searchTimeoutMillis < 100 || searchTimeoutMillis > 10000) {
            throw new IllegalArgumentException("base.requestTimeoutMillis must be between 100 and 10000");
        }
    }

    @Override
    public void start() throws Exception {
        // The local compatibility runtime predates BaseSvr2Client. Reject it in
        // STC mode rather than silently falling back to the custom loader.
        try {
            Class<?> facade = Class.forName("sc.app.stc.utils.BaseSvrClient");
            requestClient = Class.forName("sc.app.stc.utils.BaseSvr2Client");
            getBond = facade.getMethod("getBondInfo", String.class);
            getValuation = facade.getMethod("getChianBondInfo", String.class);
            getIssuer = requestClient.getMethod("getInfoTableVal", String.class, String.class);
            facade.getField("isSubOrReq").set(null, "R");
            facade.getField("ServerKey").set(null, serverKey);
            requestClient.getField("ServerKey").set(null, serverKey);
            facade.getField("isLoadBond").setBoolean(null, true);
            IMessage<Object> callback = (topic, value) -> LOGGER.info("STC base callback: topic=" + topic);
            facade.getMethod("initialize", NetClientWrapper.class, IMessage.class).invoke(null, network, callback);
            searchTransport = new NetClientTransport(network, serverKey, searchTimeoutMillis);
            LOGGER.info("STC BaseSvr2Client loaded from " + requestClient.getProtectionDomain().getCodeSource().getLocation());
        } catch (ReflectiveOperationException error) {
            throw new IllegalStateException("STC base.mode requires the current sc-app-stc BaseSvrClient/BaseSvr2Client library", error);
        }
    }

    @Override
    public boolean online() {
        return network.isOnline(serverKey);
    }

    @Override
    public JSONObject bond(String securityId) {
        JSONObject value = invoke(getBond, securityId);
        if (value == null) {
            try {
                Set<?> missing = (Set<?>)requestClient.getField("failedBonds").get(null);
                if (!missing.contains(securityId)) {
                    throw ApiException.forField("1009", "NOT_READY",
                            "STC bond lookup did not complete; inspect BaseSvrClient logs", "securityId");
                }
            } catch (ReflectiveOperationException error) {
                throw new IllegalStateException("Cannot inspect STC failedBonds cache", error);
            }
        }
        return value;
    }

    @Override
    public JSONObject issuer(String issuerId) {
        return invoke(getIssuer, "p_issuer_info", issuerId);
    }

    @Override
    public JSONObject valuation(String securityId) {
        return invoke(getValuation, securityId);
    }

    @Override
    public JSONObject searchPage(int page, int pageSize) {
        if (page < 1 || pageSize < 1 || pageSize > 1000) {
            throw new IllegalArgumentException("Invalid bond search page");
        }
        // Additional capability in the FICC adapter; the upstream STC classes are unchanged.
        return searchTransport.request("stc.base.search", null, JsonUtils.object(
                "PageNum", page, "PageSize", pageSize,
                "BondModifyDate", "1970-01-01 00:00:00",
                "BondListInfoModifyDate", "1970-01-01 00:00:00",
                "BondMasterModifyDate", "1970-01-01 00:00:00"));
    }

    private JSONObject invoke(Method method, Object... arguments) {
        long started = System.nanoTime();
        LOGGER.info("STC base call: method=" + method.getName() + ", arguments=" + JsonUtils.toJson(arguments));
        try {
            Object result = method.invoke(null, arguments);
            JSONObject value = result == null ? null : JsonUtils.parseObject(JsonUtils.toJson(result));
            LOGGER.info("STC base result: method=" + method.getName() + ", found=" + (value != null)
                    + ", elapsedMillis=" + (System.nanoTime() - started) / 1_000_000);
            if (LOGGER.isDebugEnabled()) {
                LOGGER.debug("STC base result body: method=" + method.getName()
                        + ", body=" + DpsMessageLog.body(JsonUtils.toJson(value)));
            }
            return value;
        } catch (ReflectiveOperationException error) {
            Throwable cause = error instanceof InvocationTargetException ? error.getCause() : error;
            LOGGER.warn("STC base call failed: method=" + method.getName() + ", errorType=" + cause.getClass().getName());
            throw ApiException.forField("1009", "NOT_READY", "STC base call failed: " + method.getName(), null);
        }
    }
}
