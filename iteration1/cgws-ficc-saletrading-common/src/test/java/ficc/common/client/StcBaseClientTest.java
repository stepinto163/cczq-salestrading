package ficc.common.client;

import static org.junit.jupiter.api.Assertions.*;

import com.alibaba.fastjson2.JSONObject;

import ficc.common.ApiException;
import ficc.common.JsonUtils;
import ficc.common.ReferenceData;

import org.junit.jupiter.api.Test;

import java.util.HashMap;
import java.util.Map;
import java.util.Properties;

final class StcBaseClientTest {
    @Test
    void nameSearchUsesTheSharedClientDictionaryBeforeAnyBondHasBeenLookedUp() {
        FakeSource source = new FakeSource();
        StcBaseDataClient adapter = new StcBaseDataClient(source, new Properties());
        assertEquals("240212.IB", adapter.querySecurities("国开").get(0).getString("securityId"));
        assertTrue(source.requests.isEmpty());
        assertTrue(adapter.querySecurities("does-not-match").isEmpty());
    }

    @Test
    void coldQuoteLookupAndExactSearchUseStcWithoutBulkLoadingOrAllowList() throws Exception {
        FakeSource source = new FakeSource();
        Properties configuration = new Properties();
        configuration.setProperty("base.securityIds", "250001.IB");
        StcBaseDataClient adapter = new StcBaseDataClient(source, configuration);
        adapter.start();
        ReferenceData references = new ReferenceData(adapter, new EmptyCrmClient());
        assertTrue(adapter.securities().isEmpty());
        assertEquals("240210.IB", references.require("240210.IB").getString("securityId"));
        assertEquals(
                "240211.IB",
                references.querySecurities("240211.IB").get(0).getString("securityId"));
        assertTrue(source.requests.containsKey("240210.IB"));
        assertTrue(source.requests.containsKey("240211.IB"));
        assertEquals("2.4500", references.require("240210.IB").getString("valuationYield"));
    }

    @Test
    void stcUpdatesAndRemovalAreVisibleWithoutAnIndependentRefreshCycle() {
        FakeSource source = new FakeSource();
        StcBaseDataClient adapter = new StcBaseDataClient(source, new Properties());
        JSONObject first = adapter.security("240210.IB");
        source.rating = "AA+";
        JSONObject updated = adapter.security("240210.IB");
        assertEquals("AA+", updated.getString("bondRating"));
        assertNotEquals(first.getString("referenceVersion"), updated.getString("referenceVersion"));
        source.missing = true;
        assertNull(adapter.security("240210.IB"));
        assertTrue(adapter.securities().isEmpty());
        assertTrue(adapter.records("DAILY_VALUATION").isEmpty());
    }

    @Test
    void missingIssuerSupplementDoesNotMakeAnExistingBondInvalid() {
        FakeSource source = new FakeSource();
        source.missingIssuer = true;
        StcBaseDataClient adapter = new StcBaseDataClient(source, new Properties());
        ReferenceData references = new ReferenceData(adapter, new EmptyCrmClient());
        JSONObject security = references.require("240210.IB");
        assertEquals("240210.IB", security.getString("securityId"));
        assertEquals("AAA", security.getString("bondRating"));
        assertEquals("2.4500", security.getString("valuationYield"));
        assertNull(security.getString("ownershipType"));
    }

    @Test
    void offlineIsNotReportedAsAnUnknownBond() {
        FakeSource source = new FakeSource();
        StcBaseDataClient adapter = new StcBaseDataClient(source, new Properties());
        ReferenceData references = new ReferenceData(adapter, new EmptyCrmClient());
        source.online = false;
        assertEquals(
                "1009",
                assertThrows(ApiException.class, () -> references.require("240210.IB")).code);
        assertTrue(source.requests.isEmpty());
        source.online = true;
        source.missing = true;
        assertEquals(
                "1001",
                assertThrows(ApiException.class, () -> references.require("240210.IB")).code);
    }

    private static final class FakeSource implements StcBaseDataClient.Source {
        final Map<String, Integer> requests = new HashMap<>();
        boolean online = true;
        boolean missing;
        boolean missingIssuer;
        String rating = "AAA";

        @Override
        public void start() {}

        @Override
        public boolean online() {
            return online;
        }

        @Override
        public JSONObject bond(String id) {
            requests.merge(id, 1, Integer::sum);
            return missing
                    ? null
                    : JsonUtils.object(
                            "Bond_ID",
                            id,
                            "Listed_Market",
                            "CIB",
                            "Option_Type",
                            "NON",
                            "Issuer_Code",
                            "ISS-1",
                            "Rating_Current",
                            rating);
        }

        @Override
        public JSONObject issuer(String id) {
            return missingIssuer
                    ? null
                    : JsonUtils.object("issuer_name", "Test issuer", "province", "北京");
        }

        @Override
        public JSONObject valuation(String id) {
            return JsonUtils.object("valuation_date", "20260915", "valuation_yield", "2.4500");
        }

        @Override
        public JSONObject searchPage(int page, int size) {
            return JsonUtils.object(
                    "Code",
                    "0000",
                    "Total",
                    1,
                    "Datas",
                    JsonUtils.array(
                            JsonUtils.object(
                                    "Bond_ID",
                                    "240212.IB",
                                    "Short_Name",
                                    "国开测试",
                                    "Option_Type",
                                    "NON")));
        }
    }
}
