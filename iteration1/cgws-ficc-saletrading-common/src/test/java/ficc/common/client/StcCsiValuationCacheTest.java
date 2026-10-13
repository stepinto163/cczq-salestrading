package ficc.common.client;

import com.alibaba.fastjson2.JSONObject;
import ficc.common.JsonUtils;
import org.junit.jupiter.api.Test;
import java.util.Properties;
import java.util.concurrent.CountDownLatch;
import java.util.concurrent.TimeUnit;
import java.util.concurrent.atomic.AtomicInteger;
import java.util.function.Consumer;
import static org.junit.jupiter.api.Assertions.*;

class StcCsiValuationCacheTest {
    @Test
    void preservesSeparateBasesDatesUnitsAndMissingValues() {
        JSONObject value = StcBaseMapper.csiValuation(JsonUtils.object(
                "valuate_date", "20260724", "yld_to_maturity", "1.7050",
                "clean_price_maturity", "104.6547", "full_price_maturity", "105.1698",
                "yld_to_exercise", "2.1000", "recommendation", "1",
                "clean_price_exercise", " "));
        assertEquals("2026-07-24", value.getString("csiValuationDate"));
        assertEquals("1.7050", value.getString("csiValuationYield"));
        assertEquals("104.6547", value.getString("csiValuationCleanPrice"));
        assertEquals("105.1698", value.getString("csiValuationFullPrice"));
        assertEquals("2.1000", value.getString("csiExerciseYield"));
        assertEquals("EXERCISE", value.getString("csiRecommendedBasis"));
        assertNull(value.get("csiExerciseCleanPrice"));
        assertThrows(NumberFormatException.class, () -> StcBaseMapper.csiValuation(
                JsonUtils.object("yld_to_maturity", "bad")));
    }

    @Test
    void pagesAtomicallyIndexesMarketsAndReloadsWithoutQueryRpc() throws Exception {
        FakeTransport source = new FakeTransport();
        try (StcCsiValuationCache cache = new StcCsiValuationCache(source, new Properties(), 1)) {
            cache.refresh();
            assertTrue(source.started.await(2, TimeUnit.SECONDS));
            assertEquals("LOADING", cache.get("240210.IB").getString("csiValuationStatus"));
            assertNull(cache.get("240210.IB").get("csiValuationYield"));
            source.release.countDown();
            await(cache, "READY");
            assertEquals("1.7050", cache.get("240210.IB").getString("csiValuationYield"));
            assertEquals("1.7050", cache.get("123456.SH").getString("csiValuationYield"));
            assertEquals("1.7050", cache.get("123457.SZ").getString("csiValuationYield"));
            assertEquals("MISSING", cache.get("absent.IB").getString("csiValuationStatus"));
            int requests = source.requests.get();
            JSONObject editable = cache.get("240210.IB");
            editable.put("csiValuationYield", "999");
            assertEquals("1.7050", cache.get("240210.IB").getString("csiValuationYield"));
            assertEquals(requests, source.requests.get());
            long previous = cache.revision();
            source.fail = true;
            cache.refresh();
            await(cache, "FAILED");
            assertNull(cache.get("240210.IB").get("csiValuationYield"));
            assertTrue(cache.revision() > previous);
            source.fail = false;
            cache.refresh();
            await(cache, "READY");
            source.duplicate = true;
            cache.refresh();
            await(cache, "FAILED");
            assertNull(cache.get("240210.IB").get("csiValuationYield"));
        }
    }

    private static void await(StcCsiValuationCache cache, String status) throws Exception {
        long deadline = System.nanoTime() + TimeUnit.SECONDS.toNanos(3);
        while (!status.equals(cache.get("240210.IB").getString("csiValuationStatus"))
                && System.nanoTime() < deadline) {
            Thread.sleep(10);
        }
        assertEquals(status, cache.get("240210.IB").getString("csiValuationStatus"));
    }

    private static final class FakeTransport implements StcTransport {
        final CountDownLatch started = new CountDownLatch(1);
        final CountDownLatch release = new CountDownLatch(1);
        final AtomicInteger requests = new AtomicInteger();
        volatile boolean fail;
        volatile boolean duplicate;

        public JSONObject request(String topic, String session, JSONObject request) {
            assertEquals(StcCsiValuationCache.TOPIC, topic);
            requests.incrementAndGet();
            started.countDown();
            try {
                if (!release.await(2, TimeUnit.SECONDS)) {
                    throw new IllegalStateException("Test loader timeout");
                }
            } catch (InterruptedException error) {
                Thread.currentThread().interrupt();
                throw new IllegalStateException(error);
            }
            int page = request.getIntValue("PageNum");
            if (page == 2 && fail) {
                return JsonUtils.object("Code", "1009");
            }
            if (page == 2 && duplicate) {
                page = 1;
            }
            return JsonUtils.object("Code", "0000", "Datas", page <= 2
                    ? JsonUtils.array(JsonUtils.object("interbank_code", "240210",
                            "shh_code", "123456", "shz_code", "123457",
                            "valuate_date", page == 1 ? "20260724" : "20260723",
                            "yld_to_maturity", page == 1 ? "1.7050" : "9.9999"))
                    : JsonUtils.array());
        }

        public boolean online() { return true; }
        public void subscribe(String topic, Consumer<JSONObject> listener) { }
        public void connectionChanged(Runnable listener) { }
    }
}
