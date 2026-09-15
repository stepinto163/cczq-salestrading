package ficc.common.client;

import com.alibaba.fastjson2.JSONObject;
import ficc.common.ApiException;
import ficc.common.JsonUtils;
import ficc.common.ReferenceData;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Map;
import java.util.Properties;
import java.util.function.Consumer;
import org.junit.jupiter.api.Test;
import static org.junit.jupiter.api.Assertions.*;

final class StcBaseUniverseTest {
    @Test
    void noWhitelistLoadsAllPagesIncludingMaturedBondsAndUsesStcClassification() {
        BulkTransport transport = new BulkTransport();
        StcBaseDataClient base = new StcBaseDataClient(transport, configuration());
        assertThrows(ApiException.class, base::securities);
        base.refresh();
        ReferenceData references = new ReferenceData(base, new EmptyCrmClient());
        assertEquals(4, references.securities().size());
        assertEquals("RATE", references.require("250001.IB").getString("market"));
        assertEquals("CREDIT", references.require("123456.SH").getString("market"));
        assertNull(references.require("112233.SZ").getString("market"));
        assertEquals("12", references.require("112233.SZ").getString("stcGoodsType"));
        assertEquals("2020-01-01", references.require("000001.IB").getString("maturityDate"));
        assertEquals("2026-09-14", references.require("250001.IB").getString("valuationDate"));
        assertEquals("2.40", references.require("250001.IB").getString("valuationYield"));
        assertEquals("AAA", references.require("250001.IB").getString("bondRating"));
        assertEquals("AA+", references.require("250001.IB").getString("issuerRating"));
        assertNull(references.require("123456.SH").getString("bondRating"));
        assertEquals("AA+", references.require("123456.SH").getString("issuerRating"));
        assertEquals("北京", references.require("250001.IB").getString("region"));
        assertNull(base.records("DAILY_VALUATION").get(0).getString("valuationBasis"));
        int calls = transport.calls.size();
        references.enrich(JsonUtils.object("securityId", "123456.SH", "quoteId", "Q-1"));
        references.enrich(JsonUtils.object("securityId", "112233.SZ", "quoteId", "Q-2"));
        assertEquals(calls, transport.calls.size());
        assertEquals(2, transport.count("stc.query.page.securityInfo"));
        // Issuer and CDC handlers have no Total; a full last page requires an empty follow-up.
        assertEquals(2, transport.count("stc.query.page.issuerInfo"));
        assertEquals(3, transport.count("stc.query.page.cdcValuation"));
    }

    @Test
    void clearAndReconnectReloadAddedBondsAndLatestValuation() {
        BulkTransport transport = new BulkTransport();
        StcBaseDataClient base = new StcBaseDataClient(transport, configuration());
        base.refresh();
        transport.bonds.add(bond("260001", "CIB", "PBB"));
        transport.valuations.add(valuation("250001", "CIB", "2026-09-15", "2.30"));
        transport.listeners.get("stc.base.loadBondsInfo").accept(new JSONObject());
        assertThrows(ApiException.class, base::securities);
        base.refresh();
        assertEquals(5, base.securities().size());
        assertEquals("2.30", base.securities().get("250001.IB").getString("valuationYield"));
        transport.connected = false;
        transport.connectionListener.run();
        assertThrows(ApiException.class, base::securities);
        transport.connected = true;
        transport.connectionListener.run();
        base.refresh();
        assertEquals(5, base.securities().size());
    }

    @Test
    void interruptedGenerationCannotPublishPartiallyLoadedUniverse() {
        BulkTransport transport = new BulkTransport();
        StcBaseDataClient base = new StcBaseDataClient(transport, configuration());
        transport.duringNextRequest = () -> transport.listeners.get("stc.base.clear").accept(new JSONObject());
        assertThrows(ApiException.class, base::refresh);
        assertThrows(ApiException.class, base::securities);
        base.refresh();
        assertEquals(4, base.securities().size());
    }

    @Test
    void failedPageDoesNotReplaceAnExistingCompleteCache() {
        BulkTransport transport = new BulkTransport();
        StcBaseDataClient base = new StcBaseDataClient(transport, configuration());
        base.refresh();
        String originalVersion = base.version();
        transport.bonds.add(bond("260001", "CIB", "PBB"));
        transport.failTopic = "stc.query.page.cdcValuation";
        assertThrows(IllegalStateException.class, base::refresh);
        assertEquals(originalVersion, base.version());
        assertEquals(4, base.securities().size());
        transport.failTopic = null;
        base.refresh();
        assertEquals(5, base.securities().size());
    }

    @Test
    void incompleteOrRepeatedPagesAndRowLimitFailInsteadOfTruncating() {
        BulkTransport incomplete = new BulkTransport();
        incomplete.truncateSecurityPages = true;
        assertThrows(IllegalStateException.class,
                () -> new StcBaseDataClient(incomplete, configuration()).refresh());
        BulkTransport repeated = new BulkTransport();
        repeated.repeatIssuerPage = true;
        assertThrows(IllegalStateException.class,
                () -> new StcBaseDataClient(repeated, configuration()).refresh());
        Properties limited = configuration();
        limited.setProperty("base.maxRecordsPerTable", "3");
        assertThrows(IllegalStateException.class,
                () -> new StcBaseDataClient(new BulkTransport(), limited).refresh());
    }

    @Test
    void missingValuationDateAndConflictingSameDayValuesAreNotFabricated() {
        BulkTransport missingDate = new BulkTransport();
        missingDate.valuations.get(0).remove("valuate_date");
        assertThrows(IllegalStateException.class,
                () -> new StcBaseDataClient(missingDate, configuration()).refresh());
        BulkTransport conflict = new BulkTransport();
        conflict.valuations.add(valuation("250001", "CIB", "2026-09-14", "9.99"));
        assertThrows(IllegalStateException.class,
                () -> new StcBaseDataClient(conflict, configuration()).refresh());
    }

    @Test
    void emptyUniverseCompletesAndInvalidPageConfigurationIsRejected() {
        BulkTransport transport = new BulkTransport();
        transport.bonds.clear();
        transport.issuers.clear();
        transport.valuations.clear();
        StcBaseDataClient base = new StcBaseDataClient(transport, configuration());
        base.refresh();
        assertTrue(base.securities().isEmpty());
        assertEquals(3, transport.calls.size());
        Properties invalid = configuration();
        invalid.setProperty("base.pageSize", "0");
        assertThrows(IllegalArgumentException.class, () -> new StcBaseDataClient(transport, invalid));
    }

    private static Properties configuration() {
        Properties properties = new Properties();
        properties.setProperty("base.pageSize", "2");
        return properties;
    }

    private static JSONObject bond(String code, String market, String subtype) {
        return JsonUtils.object("Bond_ID", code, "Listed_Market", market, "Bond_Subtype", subtype,
                "Short_Name", "Test " + code, "Issuer_Code", "ISS-1", "Issuer_Rating_Current", "AA+",
                "Option_Type", "NON");
    }

    private static JSONObject valuation(String code, String market, String date, String yield) {
        return JsonUtils.object("bond_id", code, "listed_market", market, "valuate_date", date,
                "val_yield", yield, "val_clean_price", "100.25", "remaining_year", "2.5");
    }

    private static final class BulkTransport implements StcTransport {
        final List<JSONObject> bonds = new ArrayList<>(List.of(
                bond("250001", "CIB", "BGB"), bond("123456", "SSE", "CMN"),
                bond("112233", "SZE", "MCD"), bond("000001", "CIB", "BGB")));
        final List<JSONObject> issuers = new ArrayList<>(List.of(
                JsonUtils.object("institution_code", "ISS-1", "issuer_name", "Issuer", "province", "北京"),
                JsonUtils.object("institution_code", "ISS-2", "issuer_name", "Other Issuer")));
        final List<JSONObject> valuations = new ArrayList<>(List.of(
                valuation("250001", "CIB", "20260913", "2.50"),
                valuation("250001", "CIB", "2026-09-14 00:00:00", "2.40"),
                valuation("123456", "SSE", "2026-09-14", "3.50"),
                valuation("112233", "SZE", "2026-09-14", "2.00")));
        final Map<String, Consumer<JSONObject>> listeners = new HashMap<>();
        final List<String> calls = new ArrayList<>();
        Runnable connectionListener;
        Runnable duringNextRequest;
        boolean connected = true;
        boolean truncateSecurityPages;
        boolean repeatIssuerPage;
        String failTopic;

        BulkTransport() {
            bonds.get(0).put("Rating_Current", "AAA");
            bonds.get(3).put("Maturity_Date", "20200101");
        }

        int count(String topic) {
            return (int) calls.stream().filter(topic::equals).count();
        }

        @Override
        public JSONObject request(String topic, String sessionId, JSONObject request) {
            calls.add(topic);
            assertFalse(request.getBooleanValue("onlyValid"));
            assertNull(sessionId);
            if (duringNextRequest != null) {
                Runnable action = duringNextRequest;
                duringNextRequest = null;
                action.run();
            }
            if (topic.equals(failTopic)) {
                return JsonUtils.object("Code", "1009");
            }
            List<JSONObject> rows = switch (topic) {
                case "stc.query.page.securityInfo" -> bonds;
                case "stc.query.page.issuerInfo" -> issuers;
                case "stc.query.page.cdcValuation" -> valuations;
                default -> throw new AssertionError("Unexpected per-bond request: " + topic);
            };
            int size = request.getIntValue("pageSize");
            int start = Math.min((request.getIntValue("pageNum") - 1) * size, rows.size());
            if (repeatIssuerPage && topic.equals("stc.query.page.issuerInfo")) {
                start = 0;
            }
            int end = Math.min(start + size, rows.size());
            if (truncateSecurityPages && topic.equals("stc.query.page.securityInfo")) {
                end = start + 1;
            }
            JSONObject reply = JsonUtils.object("Code", "0000", "Datas", rows.subList(start, end));
            if (topic.equals("stc.query.page.securityInfo")) {
                reply.put("Total", rows.size());
            }
            // Preserve the real JSON wire boundary (JSONArray rather than a Java List).
            return JsonUtils.parseObject(reply.toJSONString());
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
        public void connectionChanged(Runnable listener) {
            connectionListener = listener;
        }
    }
}
