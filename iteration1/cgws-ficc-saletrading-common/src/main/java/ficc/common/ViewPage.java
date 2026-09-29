package ficc.common;

import com.alibaba.fastjson2.JSONArray;
import com.alibaba.fastjson2.JSONObject;

import java.util.ArrayList;
import java.util.LinkedHashMap;
import java.util.List;
import java.util.Map;
import java.util.Set;

/** Paging coordinates and matching fingerprints; never stores client row positions. */
public final class ViewPage {
    public int pageNumber = 1;
    public int pageSize = 200;
    private Map<String, String> matchingHashes = new LinkedHashMap<>();

    public void configure(JSONObject request, boolean initial) {
        int requestedPage = initial ? 1 : positiveInteger(request, "pageNumber", 0);
        int requestedSize = positiveInteger(request, "pageSize", initial ? 200 : 0);
        if (requestedSize > 1000) {
            throw ApiException.forField(
                    "1008", "RESOURCE_LIMIT", "pageSize must be 1..1000", "pageSize");
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
            throw ApiException.forField(
                    "1001", "VALIDATION_FAILED", field + " must be a positive integer", field);
        }
    }

    public List<JSONObject> rows(List<JSONObject> allRows) {
        long offset = ((long) pageNumber - 1) * pageSize;
        int start = (int) Math.min(offset, allRows.size());
        int end = (int) Math.min(offset + pageSize, allRows.size());
        return new ArrayList<>(allRows.subList(start, end));
    }

    public JSONObject changes(
            String viewId,
            List<JSONObject> allRows,
            String idField,
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
        return JsonUtils.object(
                "viewId",
                viewId,
                "totalCount",
                allRows.size(),
                "upsertIds",
                upsertIds,
                "removeIds",
                removeIds);
    }

    public void initialize(List<JSONObject> rows, String idField) {
        changes("", rows, idField, new LinkedHashMap<>());
    }

    public int totalCount() {
        return matchingHashes.size();
    }

    /** Quote revisions already identify changes; do not serialize entire rows to hash them. */
    public void initializeVersions(Map<String, String> versions) {
        matchingHashes = new LinkedHashMap<>(versions);
    }

    public JSONObject replaceVersions(String viewId, Map<String, String> versions) {
        Set<String> removals = new java.util.LinkedHashSet<>(matchingHashes.keySet());
        removals.removeAll(versions.keySet());
        return applyVersions(viewId, versions, removals);
    }

    public JSONObject applyVersions(String viewId, Map<String, String> versions, Set<String> removals) {
        for (Map.Entry<String, String> entry : versions.entrySet()) {
            if (entry.getValue() == null || removals.contains(entry.getKey())) {
                throw new IllegalArgumentException("Invalid or conflicting row version");
            }
        }
        JSONArray upsertIds = new JSONArray();
        JSONArray removeIds = new JSONArray();
        for (String id : removals) {
            if (matchingHashes.remove(id) != null) {
                removeIds.add(id);
            }
        }
        for (Map.Entry<String, String> entry : versions.entrySet()) {
            String previous = matchingHashes.put(entry.getKey(), entry.getValue());
            if (!entry.getValue().equals(previous)) {
                upsertIds.add(entry.getKey());
            }
        }
        return JsonUtils.object("viewId", viewId, "totalCount", totalCount(),
                "upsertIds", upsertIds, "removeIds", removeIds);
    }

    /** Apply only affected IDs. Unmentioned rows retain their membership and fingerprint. */
    public JSONObject applyChanges(
            String viewId,
            Map<String, JSONObject> upserts,
            Set<String> removals,
            Map<String, JSONObject> changedRows) {
        Map<String, String> hashes = new LinkedHashMap<>();
        for (Map.Entry<String, JSONObject> entry : upserts.entrySet()) {
            if (removals.contains(entry.getKey())) {
                throw new IllegalArgumentException("A row cannot be both upserted and removed");
            }
            hashes.put(entry.getKey(), JsonUtils.canonicalHash(entry.getValue()));
        }
        JSONArray upsertIds = new JSONArray();
        JSONArray removeIds = new JSONArray();
        for (String id : removals) {
            if (matchingHashes.remove(id) != null) {
                removeIds.add(id);
            }
        }
        for (Map.Entry<String, String> entry : hashes.entrySet()) {
            String previous = matchingHashes.put(entry.getKey(), entry.getValue());
            if (!entry.getValue().equals(previous)) {
                upsertIds.add(entry.getKey());
                changedRows.put(entry.getKey(), upserts.get(entry.getKey()));
            }
        }
        return JsonUtils.object(
                "viewId", viewId,
                "totalCount", totalCount(),
                "upsertIds", upsertIds,
                "removeIds", removeIds);
    }
}
