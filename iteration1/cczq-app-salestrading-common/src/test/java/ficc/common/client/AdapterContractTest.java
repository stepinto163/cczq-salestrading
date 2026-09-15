package ficc.common.client;

import ats.app.api.BaseApi.IMessage;
import ats.app.ym.ceda.ICedaConnectionListener;
import ats.app.ym.ceda.OnlineStatus;
import com.alibaba.fastjson2.JSONObject;
import ficc.common.ApiException;
import ficc.common.JsonUtils;
import ficc.common.ReferenceData;
import org.junit.jupiter.api.Test;
import sc.app.stc.net.NetClientWrapper;
import java.util.*;
import java.util.function.Consumer;
import static org.junit.jupiter.api.Assertions.*;

final class AdapterContractTest {
    @Test
    void simulatorAndEmptyCrmAreDistinctAndModesAreValidated() throws Exception {
        Properties configuration = new Properties();
        LoginSessionClient identity = AdapterFactory.identity(configuration, null, true);
        String sid = ((LocalLoginSimulator) identity).login(JsonUtils.object(
                "username", "trader1", "password", "trader1")).getString("sid");
        assertEquals("U-001", identity.resolveUser(sid));
        assertNull(identity.resolveUser("unknown"));
        assertEquals(2, AdapterFactory.crm(configuration, null).institutions("U-001").size());
        configuration.setProperty("crm.mode", "empty");
        assertTrue(AdapterFactory.crm(configuration, null).institutions("U-001").isEmpty());
        assertNull(AdapterFactory.crm(configuration, null).contact("U-001", "C-001"));
        configuration.setProperty("identity.mode", "typo");
        assertThrows(IllegalArgumentException.class, () -> AdapterFactory.identity(configuration, null, true));
    }

    @Test
    void realIdentityReusesSourceClientCacheAndLogoutInvalidation() throws Exception {
        FakeNetwork network = new FakeNetwork();
        Properties configuration = new Properties();
        configuration.setProperty("identity.mode", "stc");
        LoginSessionClient identity = AdapterFactory.identity(configuration, network, true);
        assertEquals("STC-42", identity.resolveUser("sid-42"));
        assertEquals("STC-42", identity.resolveUser("sid-42"));
        assertEquals(1, network.requests);
        assertEquals("userInfo", network.lastRequest.getString("method"));
        assertEquals("sid-42", network.lastRequest.getJSONObject("parameter").getString("sid"));
        assertEquals(3000, network.timeoutMillis);
        network.reply = JsonUtils.object("Code", "1003");
        network.emit(JsonUtils.object("type", "logout", "sid", "sid-42"));
        assertNull(identity.resolveUser("sid-42"));
        network.online.onDisconnected("SERVER.LoginSvr");
        assertThrows(ApiException.class, () -> identity.resolveUser("sid-42"));
    }

    @Test
    void missingLoginSessionsAreRejectedWithoutCallingLoginSvr() {
        FakeNetwork network = new FakeNetwork();
        Properties configuration = new Properties();
        configuration.setProperty("identity.mode", "stc");
        LoginSessionClient identity = AdapterFactory.identity(configuration, network, true);
        for (String sessionId : new String[] {null, "", " ", "null", " NULL "}) {
            ApiException error = assertThrows(ApiException.class, () -> identity.resolveUser(sessionId));
            assertEquals("1003", error.code);
            assertTrue(error.getMessage().contains("valid session ID"));
        }
        assertEquals(0, network.requests);
    }

    @Test
    void aSessionWithoutABusinessUserCannotCreateAnAuditIdentity() {
        for (String userId : new String[] {null, "", "null", " NULL "}) {
            FakeNetwork network = new FakeNetwork();
            network.reply = JsonUtils.object("Code", "0001", "sid", "sid-42", "userid", userId);
            Properties configuration = new Properties();
            configuration.setProperty("identity.mode", "stc");
            LoginSessionClient identity = AdapterFactory.identity(configuration, network, true);
            ApiException error = assertThrows(ApiException.class, () -> identity.resolveUser("sid-42"));
            assertEquals("1003", error.code);
            assertTrue(error.getMessage().contains("business user ID"));
        }
    }

    @Test
    void realModeDoesNotGrantSimulatorPrivilegesOrRenameAuditUser() {
        Properties configuration = new Properties();
        configuration.setProperty("identity.mode", "stc");
        configuration.setProperty("identity.internalUserIds", "service-user-9");
        AccessPolicy policy = new AccessPolicy(configuration);
        assertFalse(policy.administrator("U-ADMIN"));
        assertFalse(policy.internal("S-FAKE"));
        assertTrue(policy.internal("service-user-9"));
        assertTrue(policy.owns("STC-42", "STC-42"));
        assertFalse(policy.owns("STC-42", "STC-43"));
    }

    @Test
    void baseMappingRefreshAndFilteringDoNotUseFakeDataOrPerRowRemoteCalls() throws Exception {
        FakeBase transport = new FakeBase();
        Properties configuration = new Properties();
        configuration.setProperty("base.securityIds", "250001.IB");
        configuration.setProperty("base.market.250001.IB", "RATE");
        configuration.setProperty("base.issuerType.GOV", "GOVERNMENT");
        StcBaseDataClient base = new StcBaseDataClient(transport, configuration);
        assertThrows(ApiException.class, base::securities);
        base.refresh();
        ReferenceData references = new ReferenceData(base, new EmptyCrmClient());
        assertEquals("AAA", references.require("250001.IB").getString("bondRating"));
        assertEquals("2026-09-14", references.require("250001.IB").getString("valuationDate"));
        assertEquals("GOVERNMENT", references.require("250001.IB").getString("issuerType"));
        assertFalse(references.require("250001.IB").getBooleanValue("hasExerciseOption"));
        int calls = transport.requests;
        references.enrich(JsonUtils.object("securityId", "250001.IB", "quoteId", "Q-1"));
        references.enrich(JsonUtils.object("securityId", "250001.IB", "quoteId", "Q-2"));
        assertEquals(calls, transport.requests);
        transport.rating = "AA+";
        transport.listeners.get("stc.base.clear").accept(new JSONObject());
        assertThrows(ApiException.class, base::securities);
        base.refresh();
        assertEquals("AA+", references.require("250001.IB").getString("bondRating"));
        transport.connected = false;
        assertThrows(ApiException.class, base::securities);
    }

    @Test
    void baseRejectsWrongBondAndInvalidOverrideWithoutGuessingUnknownClassification() {
        Properties configuration = new Properties();
        StcBaseMapper mapper = new StcBaseMapper(configuration);
        JSONObject bond = JsonUtils.object("Bond_ID", "250001", "Listed_Market", "CIB", "Option_Type", "NON");
        assertThrows(IllegalArgumentException.class, () -> mapper.security("250002.IB", bond, null, null, "v1"));
        assertNull(mapper.security("250001.IB", bond, null, null, "v1").getString("market"));
        configuration.setProperty("base.market.250001.IB", "INVALID");
        assertThrows(IllegalArgumentException.class, () -> mapper.security("250001.IB", bond, null, null, "v1"));
        configuration.setProperty("base.market.250001.IB", "RATE");
        bond.put("Option_Type", "PUT");
        assertTrue(mapper.security("250001.IB", bond, null, null, "v1").getBooleanValue("hasExerciseOption"));
    }

    @Test
    void baseSubsetAcceptsLegacyLowercaseRowsAndReportsMissingStcRows() {
        FakeBase transport = new FakeBase();
        Properties configuration = new Properties();
        configuration.setProperty("base.securityIds", "250001.IB");
        configuration.setProperty("base.market.250001.IB", "RATE");
        StcBaseDataClient base = new StcBaseDataClient(transport, configuration);
        transport.rowsField = "datas";
        base.refresh();
        assertEquals(1, base.securities().size());
        transport.rowsField = "wrongField";
        ApiException failure = assertThrows(ApiException.class, base::refresh);
        assertTrue(failure.getMessage().contains("topic=querySecurity"));
        assertTrue(failure.getMessage().contains("no Datas array"));
    }

    static final class FakeNetwork extends NetClientWrapper {
        final OnlineStatus online = new OnlineStatus();
        final List<IMessage<String>> listeners = new ArrayList<>();
        JSONObject reply = JsonUtils.object("Code", "0001", "sid", "sid-42", "userid", "STC-42", "username", "alice");
        JSONObject lastRequest;
        int requests;
        long timeoutMillis;

        FakeNetwork() {
            online.onConnected("SERVER.LoginSvr");
        }
        @Override
        public void init(String key, String... names) {}
        @Override
        public void init(String key, IMessage<String> listener) {}
        @Override
        public void subscribe(String key, String topic, IMessage<String> listener) {
            listeners.add(listener);
        }
        @Override
        public void addCedaClientConnectListener(String key, ICedaConnectionListener listener) {}
        @Override
        public OnlineStatus getOnlineStatus() {
            return online;
        }
        @Override
        public boolean isOnline(String key) {
            return online.isOnline(key);
        }
        @Override
        public String requestSync(String key, String sid, String topic, String content, long timeout) {
            assertEquals("SYS.ATS.LOGIN", topic);
            requests++;
            lastRequest = JsonUtils.parseObject(content);
            timeoutMillis = timeout;
            return reply.toJSONString();
        }
        void emit(JSONObject event) {
            for (IMessage<String> listener : listeners) {
                listener.onMessage("SYS.ATS.LOGIN", event.toJSONString());
            }
        }
    }

    static final class FakeBase implements StcTransport {
        final Map<String, Consumer<JSONObject>> listeners = new HashMap<>();
        boolean connected = true;
        String rating = "AAA";
        String rowsField = "Datas";
        int requests;
        @Override
        public JSONObject request(String topic, String sid, JSONObject request) {
            requests++;
            if (topic.equals("stc.base.info.query")) {
                assertEquals("p_issuer_info", request.getString("TableName"));
                return JsonUtils.object("Code", "0000", "Datas", JsonUtils.object(
                        "institution_code", "ISS-1", "issuer_name", "Test Issuer", "issuer_type", "GOV", "province", "北京"));
            }
            assertEquals("250001.IB", request.getString("SecurityID"));
            JSONObject data = topic.equals("querySecurity")
                    ? JsonUtils.object("Bond_ID", "250001", "Listed_Market", "CIB", "Short_Name", "Test Bond",
                            "Issuer_Code", "ISS-1", "Rating_Current", rating, "Issuer_Rating_Current", "AAA", "Option_Type", "NON")
                    : JsonUtils.object("Id", "VAL-1", "valuation_date", "20260914", "valuation_yield", "2.4500",
                            "remaining_year", "2.5", "bondTypeFlag", "2");
            return JsonUtils.object("Code", "0000", rowsField, JsonUtils.array(data));
        }
        @Override
        public boolean online() {
            return connected;
        }
        @Override
        public void subscribe(String topic, Consumer<JSONObject> listener) {
            listeners.put(topic, listener);
        }
        @Override
        public void connectionChanged(Runnable listener) {}
    }
}
