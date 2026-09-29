package ficc.common;

import com.alibaba.fastjson2.JSONObject;
import org.junit.jupiter.api.Test;

import java.util.Arrays;
import java.util.Collections;
import java.util.LinkedHashMap;
import java.util.Map;

import static org.junit.jupiter.api.Assertions.*;

class IncrementalViewPageTest {
    @Test
    void tracksRevisionsWithoutSerializingRowsAndReconcilesMembership() {
        ViewPage page = new ViewPage();
        Map<String, String> initial = new LinkedHashMap<>();
        initial.put("A", "1");
        initial.put("B", "2");
        page.initializeVersions(initial);
        initial.clear();
        assertEquals(2, page.totalCount());
        JSONObject unchanged = page.applyVersions("V", Collections.singletonMap("A", "1"),
                Collections.emptySet());
        assertTrue(unchanged.getJSONArray("upsertIds").isEmpty());
        JSONObject updated = page.applyVersions("V", Collections.singletonMap("A", "2"),
                Collections.emptySet());
        assertEquals(JsonUtils.array("A"), updated.getJSONArray("upsertIds"));
        assertEquals(2, page.totalCount());
        JSONObject reconciled = page.replaceVersions("V", Collections.singletonMap("A", "2"));
        assertEquals(JsonUtils.array("B"), reconciled.getJSONArray("removeIds"));
        assertTrue(reconciled.getJSONArray("upsertIds").isEmpty());
        assertThrows(IllegalArgumentException.class, () -> page.applyVersions("V",
                Collections.singletonMap("A", "3"), Collections.singleton("A")));
        assertEquals(1, page.totalCount());
    }

    @Test
    void retainsUnmentionedRowsAndSuppressesUnchangedRows() {
        ViewPage page = new ViewPage();
        JSONObject first = JsonUtils.object("quoteId", "A", "price", "2.5");
        page.initialize(Arrays.asList(first, JsonUtils.object("quoteId", "B")), "quoteId");
        Map<String, JSONObject> changed = new LinkedHashMap<>();
        JSONObject delta = page.applyChanges("V", Collections.singletonMap("A", first),
                Collections.singleton("absent"), changed);
        assertEquals(2, delta.getIntValue("totalCount"));
        assertTrue(delta.getJSONArray("upsertIds").isEmpty());
        assertTrue(delta.getJSONArray("removeIds").isEmpty());
        assertTrue(changed.isEmpty());
        delta = page.applyChanges("V", Collections.singletonMap("C", JsonUtils.object("quoteId", "C")),
                Collections.singleton("A"), changed);
        assertEquals(2, page.totalCount());
        assertEquals(JsonUtils.array("C"), delta.getJSONArray("upsertIds"));
        assertEquals(JsonUtils.array("A"), delta.getJSONArray("removeIds"));
        assertEquals(Collections.singleton("C"), changed.keySet());
    }

    @Test
    void updatesExistingRowWithoutChangingCountAndCanReconcileFullSnapshot() {
        ViewPage page = new ViewPage();
        page.initialize(Collections.singletonList(JsonUtils.object("quoteId", "A", "price", "2.5")), "quoteId");
        JSONObject updated = JsonUtils.object("quoteId", "A", "price", "2.6");
        Map<String, JSONObject> changed = new LinkedHashMap<>();
        JSONObject delta = page.applyChanges("V", Collections.singletonMap("A", updated),
                Collections.emptySet(), changed);
        assertEquals(1, delta.getIntValue("totalCount"));
        assertEquals(updated, changed.get("A"));
        assertTrue(page.changes("V", Collections.singletonList(updated), "quoteId",
                new LinkedHashMap<>()).getJSONArray("upsertIds").isEmpty());
        delta = page.applyChanges("V", Collections.emptyMap(), Collections.singleton("A"),
                new LinkedHashMap<>());
        assertEquals(0, delta.getIntValue("totalCount"));
    }

    @Test
    void rejectsConflictingPatchBeforeChangingMembership() {
        ViewPage page = new ViewPage();
        page.initialize(Collections.singletonList(JsonUtils.object("quoteId", "A")), "quoteId");
        assertThrows(IllegalArgumentException.class, () -> page.applyChanges("V",
                Collections.singletonMap("A", JsonUtils.object("quoteId", "A")),
                Collections.singleton("A"), new LinkedHashMap<>()));
        assertEquals(1, page.totalCount());
    }
}
