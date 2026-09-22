package ficc.common.client;

import static org.junit.jupiter.api.Assertions.*;

import com.alibaba.fastjson2.JSONObject;

import ficc.common.JsonUtils;

import org.junit.jupiter.api.Test;

import java.util.ArrayList;
import java.util.List;

final class StcIssuerLookupTest {
    @Test
    void missingInstitutionPreservesIssuerAndDoesNotInventOwnership() {
        JSONObject issuer = JsonUtils.object("issuer_name", "测试发行人", "province", "北京");
        List<String> calls = new ArrayList<>();
        JSONObject result =
                StcBaseSvrClientBridge.loadIssuer(
                        "ISS-1",
                        (table, id) -> {
                            calls.add(table + ":" + id);
                            return "p_issuer_info".equals(table) ? issuer : null;
                        });
        assertEquals(java.util.Arrays.asList("p_issuer_info:ISS-1", "institution:ISS-1"), calls);
        assertEquals("测试发行人", result.getString("issuer_name"));
        assertFalse(result.containsKey("institution_subtype"));
        assertNotSame(issuer, result);
    }

    @Test
    void existingInstitutionUsesCaseInsensitiveSourceFields() {
        JSONObject issuer = JsonUtils.object("issuer_name", "测试发行人");
        JSONObject result =
                StcBaseSvrClientBridge.loadIssuer(
                        "ISS-1",
                        (table, id) ->
                                "institution".equals(table)
                                        ? JsonUtils.object("institution_subtype", "YQ")
                                        : issuer);
        assertEquals("YQ", result.getString("institution_subtype"));
        assertFalse(issuer.containsKey("institution_subtype"));
    }

    @Test
    void absentRecordsRemainMissingAndAreRetriedOnTheNextLookup() {
        assertNull(StcBaseSvrClientBridge.loadIssuer("ISS-1", (table, id) -> new JSONObject()));
        JSONObject recovered =
                StcBaseSvrClientBridge.loadIssuer(
                        "ISS-1",
                        (table, id) ->
                                "institution".equals(table)
                                        ? JsonUtils.object("Institution_Subtype", "YQ")
                                        : null);
        assertEquals("YQ", recovered.getString("institution_subtype"));
    }

    @Test
    void requestFailureIsNotTreatedAsMissingData() {
        RuntimeException failure = new IllegalStateException("BaseSvr unavailable");
        assertSame(
                failure,
                assertThrows(
                        IllegalStateException.class,
                        () ->
                                StcBaseSvrClientBridge.loadIssuer(
                                        "ISS-1",
                                        (table, id) -> {
                                            throw failure;
                                        })));
        assertNull(
                StcBaseSvrClientBridge.loadIssuer(
                        " ",
                        (table, id) -> {
                            fail("Blank issuer must not trigger a request");
                            return null;
                        }));
    }
}
