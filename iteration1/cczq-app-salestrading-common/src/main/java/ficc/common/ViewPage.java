package ficc.common;

import com.alibaba.fastjson2.JSONArray;
import com.alibaba.fastjson2.JSONObject;
import java.util.ArrayList;
import java.util.LinkedHashMap;
import java.util.List;
import java.util.Map;

/** Paging coordinates and matching fingerprints; never stores client row positions. */
public final class ViewPage {
    public int pageNumber = 1;
    public int pageSize = 200;
    private Map<String, String> matchingHashes = new LinkedHashMap<>();

    public void configure(JSONObject request, boolean initial) {
        int requestedPage = initial ? 1 : positiveInteger(request, "pageNumber", 0);
        int requestedSize = positiveInteger(request, "pageSize", initial ? 200 : 0);
        if (requestedSize > 1000) {
            throw ApiException.forField("1008", "RESOURCE_LIMIT", "pageSize must be 1..1000", "pageSize");
        }
        pageNumber = requestedPage;
        pageSize = requestedSize;
    }

    private static int positiveInteger(JSONObject request, String field, int defaultValue) {
        Object value = request.get(field);
        String text = value == null ? Integer.toString(defaultValue) : value.toString();
        try {
            if (!text.matches("[1-9][0-9]*")) {
                throw new NumberFormatException();
            }
            return Integer.parseInt(text);
        } catch (NumberFormatException error) {
            throw ApiException.forField("1001", "VALIDATION_FAILED", field + " must be a positive integer", field);
        }
    }

    public List<JSONObject> rows(List<JSONObject> allRows) {
        long offset = ((long) pageNumber - 1) * pageSize;
        int start = (int) Math.min(offset, allRows.size());
        int end = (int) Math.min(offset + pageSize, allRows.size());
        return new ArrayList<>(allRows.subList(start, end));
    }

    public JSONObject changes(String viewId, List<JSONObject> allRows, String idField,
                              Map<String, JSONObject> changedRows) {
        Map<String, String> nextHashes = new LinkedHashMap<>();
        JSONArray upsertIds = new JSONArray();
        JSONArray removeIds = new JSONArray();
        for (JSONObject row : allRows) {
            String id = row.getString(idField);
            String hash = JsonUtils.canonicalHash(row);
            nextHashes.put(id, hash);
            if (!hash.equals(matchingHashes.get(id))) {
                upsertIds.add(id);
                changedRows.put(id, row);
            }
        }
        for (String id : matchingHashes.keySet()) {
            if (!nextHashes.containsKey(id)) {
                removeIds.add(id);
            }
        }
        matchingHashes = nextHashes;
        return JsonUtils.object("viewId", viewId, "totalCount", allRows.size(),
                "upsertIds", upsertIds, "removeIds", removeIds);
    }

    public void initialize(List<JSONObject> rows, String idField) {
        changes("", rows, idField, new LinkedHashMap<>());
    }
}
