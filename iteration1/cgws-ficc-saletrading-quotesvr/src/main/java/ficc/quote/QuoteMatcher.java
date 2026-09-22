package ficc.quote;

import com.alibaba.fastjson2.*;

import ficc.common.*;

import java.time.*;
import java.util.*;

/** Same enriched-row matching for local queries and QuoteSvr subscriptions. */
final class QuoteMatcher {
    final ReferenceData refs;
    final Map<String, JSONObject> settings;

    QuoteMatcher(ReferenceData refs, Map<String, JSONObject> settings) {
        this.refs = refs;
        this.settings = settings;
    }

    boolean matches(String user, JSONObject q, JSONObject f) {
        JSONObject row = refs.enrich(q);
        if (!FilterEngine.match(row, f)) return false;
        if (f == null) return true;
        for (String list : Arrays.asList("watchlistIds", "filterGroupIds")) {
            JSONArray ids = f.getJSONArray(list);
            if (ids == null || ids.isEmpty()) continue;
            boolean any = false;
            for (String id : ids.toJavaList(String.class)) {
                JSONObject g = settings.get(id);
                if (g == null || !user.equals(g.getString("userId")))
                    throw ApiException.forField("1002", "NOT_FOUND", "Group not found", list);
                if (f.getString("market") != null
                        && !Objects.equals(f.getString("market"), g.getString("market")))
                    throw ApiException.forField(
                            "1001", "VALIDATION_FAILED", "Group market mismatch", list);
                if (list.equals("watchlistIds")) {
                    JSONArray a = g.getJSONArray("securities");
                    if (a != null)
                        for (Object x : a)
                            if (q.getString("securityId")
                                    .equals(((JSONObject) x).getString("securityId"))) any = true;
                } else {
                    JSONArray cs = new JSONArray();
                    for (JSONObject sg : g.getJSONArray("subgroups").toJavaList(JSONObject.class))
                        cs.addAll(sg.getJSONArray("conditions"));
                    any |= FilterEngine.match(row, JsonUtils.object("conditions", cs));
                }
            }
            if (!any) return false;
        }
        return true;
    }
}
