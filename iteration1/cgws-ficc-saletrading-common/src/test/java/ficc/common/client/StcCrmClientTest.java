package ficc.common.client;

import com.alibaba.fastjson2.JSONArray;
import com.alibaba.fastjson2.JSONObject;

import ficc.common.ApiException;

import org.junit.jupiter.api.Test;

import java.util.ArrayList;
import java.util.List;
import java.util.function.Consumer;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.assertNull;
import static org.junit.jupiter.api.Assertions.assertThrows;
import static org.junit.jupiter.api.Assertions.assertTrue;

class StcCrmClientTest {

    @Test
    void queryUsesSearchTopicAndReturnsPage() {
        FakeTransport transport = new FakeTransport();
        transport.reply =
                page(
                        new JSONArray()
                                .fluentAdd(
                                        new JSONObject()
                                                .fluentPut("institutionId", "2087000000000000001")
                                                .fluentPut("institutionName", "测试银行")),
                        true,
                        Integer.valueOf(20));
        try (StcCrmClient client = new StcCrmClient(transport)) {
            JSONObject result = client.query("u1", "INSTITUTION", "测试", null, 0, 20);

            assertEquals(StcCrmClient.SEARCH_TOPIC, transport.topics.get(0));
            JSONObject payload = transport.payloads.get(0);
            assertEquals("INSTITUTION", payload.getString("kind"));
            assertEquals("测试", payload.getString("text"));
            assertEquals(0, payload.getIntValue("offset"));
            assertEquals(20, payload.getIntValue("limit"));
            JSONArray records = result.getJSONArray("records");
            assertEquals(1, records.size());
            assertEquals("测试银行", records.getJSONObject(0).getString("institutionName"));
            assertTrue(result.getBooleanValue("hasMore"));
            assertEquals(20, result.getIntValue("nextOffset"));
        } catch (Exception error) {
            throw new AssertionError(error);
        }
    }

    @Test
    void lastPageHasNoNextOffset() {
        FakeTransport transport = new FakeTransport();
        transport.reply = page(new JSONArray(), false, null);
        try (StcCrmClient client = new StcCrmClient(transport)) {
            JSONObject result = client.query("u1", "CONTACT", "", "2087000000000000001", 40, 20);
            assertNull(result.get("nextOffset"));
            assertEquals(40, transport.payloads.get(0).getIntValue("offset"));
            assertEquals(
                    "2087000000000000001", transport.payloads.get(0).getString("institutionId"));
        } catch (Exception error) {
            throw new AssertionError(error);
        }
    }

    @Test
    void missingNextOffsetFallsBackToOffsetPlusLimit() {
        FakeTransport transport = new FakeTransport();
        transport.reply = page(new JSONArray(), true, null);
        try (StcCrmClient client = new StcCrmClient(transport)) {
            assertEquals(30, client.query("u1", "INSTITUTION", "", null, 10, 20).getIntValue("nextOffset"));
        } catch (Exception error) {
            throw new AssertionError(error);
        }
    }

    @Test
    void institutionLookupUsesGetTopic() {
        FakeTransport transport = new FakeTransport();
        transport.reply =
                new JSONObject()
                        .fluentPut(
                                "record",
                                new JSONObject()
                                        .fluentPut("institutionId", "2087000000000000001")
                                        .fluentPut("institutionName", "测试银行"));
        try (StcCrmClient client = new StcCrmClient(transport)) {
            JSONObject record = client.institution("u1", "2087000000000000001");
            assertEquals(StcCrmClient.GET_TOPIC, transport.topics.get(0));
            assertEquals("INSTITUTION", transport.payloads.get(0).getString("kind"));
            assertEquals("2087000000000000001", transport.payloads.get(0).getString("id"));
            assertEquals("测试银行", record.getString("institutionName"));
        } catch (Exception error) {
            throw new AssertionError(error);
        }
    }

    @Test
    void absentRecordIsReportedAsMissing() {
        FakeTransport transport = new FakeTransport();
        transport.reply = new JSONObject().fluentPut("record", new JSONObject());
        try (StcCrmClient client = new StcCrmClient(transport)) {
            assertNull(client.institution("u1", "2087000000000000001"));
            assertNull(client.contact("u1", null));
            assertEquals(1, transport.topics.size());
        } catch (Exception error) {
            throw new AssertionError(error);
        }
    }

    @Test
    void serverFailureEnvelopeBecomesApiException() {
        FakeTransport transport = new FakeTransport();
        transport.reply =
                new JSONObject().fluentPut("Code", "1009").fluentPut("Msg", "CRMSvr unavailable");
        try (StcCrmClient client = new StcCrmClient(transport)) {
            ApiException error =
                    assertThrows(
                            ApiException.class,
                            () -> client.query("u1", "INSTITUTION", "x", null, 0, 10));
            assertEquals("1009", error.code);
            assertEquals("CRMSvr unavailable", error.getMessage());
        } catch (Exception error) {
            throw new AssertionError(error);
        }
    }

    @Test
    void transportFailureIsPropagated() {
        FakeTransport transport = new FakeTransport();
        transport.failure =
                ApiException.forField("1009", "NOT_READY", "STC request failed: OFFLINE", null);
        try (StcCrmClient client = new StcCrmClient(transport)) {
            ApiException error = assertThrows(ApiException.class, () -> client.contact("u1", "c1"));
            assertEquals("1009", error.code);
        } catch (Exception error) {
            throw new AssertionError(error);
        }
    }

    @Test
    void invalidPageIsRejectedBeforeAnyRequest() {
        FakeTransport transport = new FakeTransport();
        try (StcCrmClient client = new StcCrmClient(transport)) {
            ApiException error =
                    assertThrows(
                            ApiException.class,
                            () -> client.query("u1", "INSTITUTION", "", null, 0, 0));
            assertEquals("1001", error.code);
            assertTrue(transport.topics.isEmpty());
        } catch (Exception error) {
            throw new AssertionError(error);
        }
    }

    @Test
    void unboundedQueriesRemainUnsupportedLikeTheDatabaseClient() {
        FakeTransport transport = new FakeTransport();
        try (StcCrmClient client = new StcCrmClient(transport)) {
            assertThrows(UnsupportedOperationException.class, () -> client.institutions("u1"));
            assertThrows(UnsupportedOperationException.class, () -> client.contacts("u1"));
        } catch (Exception error) {
            throw new AssertionError(error);
        }
    }

    @Test
    void uninitializedClientRefusesToTalk() {
        StcCrmClient client = new StcCrmClient();
        ApiException error =
                assertThrows(
                        ApiException.class,
                        () -> client.query("u1", "INSTITUTION", "", null, 0, 10));
        assertEquals("1009", error.code);
    }

    private static JSONObject page(JSONArray records, boolean hasMore, Integer nextOffset) {
        JSONObject result =
                new JSONObject().fluentPut("records", records).fluentPut("hasMore", hasMore);
        result.put("nextOffset", nextOffset);
        return result;
    }

    private static final class FakeTransport implements StcTransport {
        private final List<String> topics = new ArrayList<>();
        private final List<JSONObject> payloads = new ArrayList<>();
        private JSONObject reply = new JSONObject();
        private RuntimeException failure;

        @Override
        public JSONObject request(String topic, String sessionId, JSONObject request) {
            topics.add(topic);
            payloads.add(request);
            if (failure != null) {
                throw failure;
            }
            return reply;
        }

        @Override
        public boolean online() {
            return true;
        }

        @Override
        public void subscribe(String topic, Consumer<JSONObject> listener) {}

        @Override
        public void connectionChanged(Runnable listener) {}
    }
}
