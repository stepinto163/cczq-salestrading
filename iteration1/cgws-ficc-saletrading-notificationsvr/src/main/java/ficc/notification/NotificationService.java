package ficc.notification;

import com.alibaba.fastjson2.*;

import ficc.common.*;
import ficc.api.ProtocolBoundary;
import ficc.common.client.AccessPolicy;

import java.math.BigDecimal;
import java.util.*;
import java.util.function.BiConsumer;
import java.util.stream.Collectors;

/**
 * 提醒业务核心：结合用户偏好、关注/筛选分组、报价事实和价格簿生成提醒，并保存已读状态。
 * 报价事件与价格事件是两条独立输入，不能假设同时到达；生成最优价提醒前需核对报价版本。
 * 接收人可见性仍由 AccessPolicy 约束，内部全量同步不意味着用户能看见所有双边报价。
 */
final class NotificationService {
    static final List<String> SWITCHES =
            Arrays.asList(
                    "watchlistEnabled",
                    "filterGroupEnabled",
                    "counterQuoteEnabled",
                    "myBestEnabled");
    final JdbcStore store;
    final ReferenceData refs;
    final AccessPolicy access;
    final Map<String, JSONObject> quotes = new LinkedHashMap<>(),
            marketSettings = new LinkedHashMap<>(),
            books = new LinkedHashMap<>();
    final Map<String, JSONObject> notifications = new LinkedHashMap<>(),
            preferences = new LinkedHashMap<>(),
            groups = new LinkedHashMap<>();
    final Map<String, NotifyStream> streams = new LinkedHashMap<>();
    final Map<String, Page> pages = new LinkedHashMap<>();
    BiConsumer<String, JSONObject> publish = (t, m) -> {};
    String quoteEpoch;
    String priceStream;
    long quoteSequence;
    long priceSequence;
    long lastSequence;
    boolean ready;

    static class NotifyStream {
        String user;
        String sid;
        String id;
        long sequence;
        Map<String, NotifyView> views = new LinkedHashMap<>();
        Set<String> used = new HashSet<>();
    }

    static class NotifyView {
        String id;
        JSONObject request;
        final ViewPage page = new ViewPage();
        List<String> ids = new ArrayList<>();
        Map<String, JSONObject> loaded = new LinkedHashMap<>();
    }

    static final class Page {
        final String user;
        final String sid;
        final String topic;
        final String field;
        final List<JSONObject> rows;
        final int offset;
        final int limit;
        final long expires;
        final JSONObject extra;

        Page(
                String user,
                String sid,
                String topic,
                String field,
                List<JSONObject> rows,
                int offset,
                int limit,
                long expires,
                JSONObject extra) {
            this.user = user;
            this.sid = sid;
            this.topic = topic;
            this.field = field;
            this.rows = rows;
            this.offset = offset;
            this.limit = limit;
            this.expires = expires;
            this.extra = extra;
        }

        String user() {
            return user;
        }

        String sid() {
            return sid;
        }

        String topic() {
            return topic;
        }

        String field() {
            return field;
        }

        List<JSONObject> rows() {
            return rows;
        }

        int offset() {
            return offset;
        }

        int limit() {
            return limit;
        }

        long expires() {
            return expires;
        }

        JSONObject extra() {
            return extra;
        }
    }

    NotificationService(JdbcStore store) throws Exception {
        this(store, new ReferenceData(), new AccessPolicy(new Properties()));
    }

    NotificationService(JdbcStore store, ReferenceData refs, AccessPolicy access) throws Exception {
        this.refs = refs;
        this.access = access;
        this.store = store;
        store.exec(
                "CREATE TABLE IF NOT EXISTS notifications (id text PRIMARY KEY, user_id text NOT"
                        + " NULL, active boolean NOT NULL, payload jsonb NOT NULL)");
        store.exec(
                "CREATE INDEX IF NOT EXISTS notification_user_idx ON notifications(user_id,"
                        + " active)");
        store.exec(
                "CREATE TABLE IF NOT EXISTS notification_preferences"
                        + " (user_id text PRIMARY KEY, payload jsonb NOT NULL)");
        store.exec(
                "CREATE TABLE IF NOT EXISTS notification_reads (id text, content_token text,"
                        + " read_time text, PRIMARY KEY(id, content_token))");
        for (JSONObject notification : store.rows("SELECT payload FROM notifications")) {
            notifications.put(notification.getString("notificationId"), notification);
        }
        for (JSONObject preference : store.rows("SELECT payload FROM notification_preferences")) {
            preferences.put(preference.getString("userId"), preference);
        }
        for (JSONObject group : store.rows("SELECT payload FROM " + store.table("settings"))) {
            FilterDictionaries.migrateSavedFilters(group);
            groups.put(group.getString("filterGroupId"), group);
        }
        lastSequence = store.tail();
    }

    synchronized void replaceQuotes(
            JSONObject state, List<JSONObject> allQuotes, List<JSONObject> settings) {
        quotes.clear();
        marketSettings.clear();
        for (JSONObject q : allQuotes) quotes.put(q.getString("quoteId"), q);
        for (JSONObject g : settings)
            marketSettings.put(
                    g.getString(g.containsKey("watchlistId") ? "watchlistId" : "filterGroupId"), g);
        quoteEpoch = state.getString("epoch");
        quoteSequence = state.getLongValue("sequence");
        ready = true;
    }

    /** 忽略已处理事件；epoch 改变或 sequence 有缺口时返回 false，要求协调器重新同步。 */
    synchronized boolean quoteMessage(JSONObject message) throws Exception {
        if (!Objects.equals(quoteEpoch, message.getString("epoch"))) return false;
        if ("HEARTBEAT".equals(message.getString("kind")))
            return message.getLongValue("currentSequence") <= quoteSequence;
        for (JSONObject e : message.getJSONArray("events").toJavaList(JSONObject.class)) {
            long seq = e.getLongValue("sequence");
            if (seq <= quoteSequence) continue;
            if (seq != quoteSequence + 1) return false;
            if ("QUOTE".equals(e.getString("type")))
                quotes.put(e.getString("quoteId"), e.getJSONObject("quote"));
            else {
                String id = e.getString("id");
                if ("REMOVE".equals(e.getString("action"))) marketSettings.remove(id);
                else marketSettings.put(id, e.getJSONObject("record"));
            }
            quoteSequence = seq;
        }
        recompute();
        return true;
    }

    /** SNAPSHOT 替换全部价格簿，增量必须属于同一流且序列连续；RESET_REQUIRED 触发恢复。 */
    synchronized boolean priceMessage(JSONObject message) throws Exception {
        String kind = message.getString("kind");
        if ("RESET_REQUIRED".equals(kind)) {
            books.clear();
            recompute();
            return false;
        }
        boolean initial = "SNAPSHOT".equals(kind);
        String stream = message.getString("streamId");
        long seq = message.getLongValue("sequence");
        if (!initial && (!Objects.equals(stream, priceStream) || seq > priceSequence + 1))
            return false;
        if (!initial && seq <= priceSequence) return true;
        if (initial) books.clear();
        priceStream = stream;
        priceSequence = seq;
        for (JSONObject b : message.getJSONArray("prices").toJavaList(JSONObject.class))
            books.put(b.getString("securityId"), b);
        recompute();
        return true;
    }

    JSONObject preference(String user) {
        return preferences.getOrDefault(
                user,
                JsonUtils.object(
                        "userId",
                        user,
                        "revision",
                        "0",
                        "watchlistEnabled",
                        true,
                        "filterGroupEnabled",
                        true,
                        "counterQuoteEnabled",
                        true,
                        "myBestEnabled",
                        true,
                        "updateTime",
                        null));
    }

    /** 价格簿中的 quoteRefs 必须与当前有效报价及其 revision 一致，避免混用两个时点的数据。 */
    boolean bookMatchesQuotes(String sec, List<JSONObject> active) {
        JSONObject book = books.get(sec);
        if (book == null) return false;
        JSONObject refs = book.getJSONObject("quoteRefs");
        if (refs == null || refs.size() != active.size()) return false;
        for (JSONObject q : active) {
            JSONObject link = refs.getJSONObject("FICC:" + q.getString("quoteId"));
            if (link == null || !q.getString("revision").equals(link.getString("quoteRevision")))
                return false;
        }
        return true;
    }

    Set<String> bestIds(String sec, List<JSONObject> active) {
        return bestIds(sec, active, active);
    }

    Set<String> bestIds(String sec, List<JSONObject> allActive, List<JSONObject> visible) {
        Set<String> ids = new LinkedHashSet<>();
        if (!bookMatchesQuotes(sec, allActive)) return ids;
        Set<String> visibleIds = visible.stream().map(q -> q.getString("quoteId"))
                .collect(Collectors.toSet());
        Map<String, BigDecimal> first = new HashMap<>();
        // QuoteSvr already sorted the complete book. Preserve its side order and all ties.
        for (JSONObject entry :
                books.get(sec)
                        .getJSONObject("marketData")
                        .getJSONArray("NoMDEntries")
                        .toJavaList(JSONObject.class)) {
            String quoteId = entry.getString("MDEntryID").substring("FICC:".length());
            if (!visibleIds.contains(quoteId)) continue;
            JSONObject candidate = quotes.get(quoteId);
            String bucket = comparisonBucket(candidate);
            long comparableCount = visible.stream()
                    .filter(quote -> bucket.equals(comparisonBucket(quote))).count();
            if (comparableCount < 2) continue;
            String side = bucket;
            BigDecimal px = new BigDecimal(entry.getString("MDEntryPx"));
            first.putIfAbsent(side, px);
            if (first.get(side).compareTo(px) == 0)
                ids.add(entry.getString("MDEntryID").substring("FICC:".length()));
        }
        return ids;
    }

    /** 不把单条报价或不同方向、计价方式、清算速度的报价当作最优比较对象。 */
    private static String comparisonBucket(JSONObject quote) {
        return JsonUtils.toJson(Arrays.asList(quote.getString("side"), quote.getString("quoteType"),
                quote.getString("yieldType"), quote.getString("settlType")));
    }

    Set<String> interestedUsers() {
        Set<String> users = new LinkedHashSet<>(preferences.keySet());
        for (JSONObject quote : quotes.values()) {
            users.add(quote.getString("ownerUserId"));
        }
        for (JSONObject group : marketSettings.values()) {
            users.add(group.getString("userId"));
        }
        for (JSONObject notification : notifications.values()) {
            users.add(notification.getString("userId"));
        }
        users.remove(null);
        return users;
    }

    synchronized void recompute() throws Exception {
        if (!ready) return;
        Map<String, List<JSONObject>> byBond = new TreeMap<>();
        for (JSONObject q : quotes.values())
            if ("ACTIVE".equals(q.getString("status")))
                byBond.computeIfAbsent(q.getString("securityId"), k -> new ArrayList<>()).add(q);
        Map<String, JSONObject> desired = new LinkedHashMap<>();
        for (String user : interestedUsers()) {
            JSONObject pref = preference(user);
            for (Map.Entry<String, List<JSONObject>> bond : byBond.entrySet()) {
                String sec = bond.getKey();
                List<JSONObject> active = bond.getValue().stream().filter(quote -> {
                    JSONObject source = quote.getJSONObject("source");
                    return access.canViewQuote(user, quote.getString("ownerUserId"),
                            source == null ? null : source.getString("type"), quote.getString("status"));
                }).collect(Collectors.toList());
                if (active.isEmpty()) continue;
                JSONArray reasons = new JSONArray();
                Set<String> links = new TreeSet<>();
                for (String kind : Arrays.asList("watchlist", "filterGroup"))
                    if (pref.getBooleanValue(kind + "Enabled")) {
                        JSONArray matched = new JSONArray();
                        Set<String> reasonQuotes = new TreeSet<>();
                        for (JSONObject group : marketSettings.values()) {
                            String key = kind + "Id";
                            if (!group.containsKey(key)
                                    || !user.equals(group.getString("userId"))
                                    || !"MARKET".equals(group.getString("scope"))
                                    || !Objects.equals(
                                            group.getString("market"),
                                            refs.require(sec).getString("market"))) continue;
                            JSONArray cs = new JSONArray();
                            if (kind.equals("filterGroup"))
                                for (JSONObject sub :
                                        group.getJSONArray("subgroups")
                                                .toJavaList(JSONObject.class))
                                    cs.addAll(sub.getJSONArray("conditions"));
                            boolean hit = false;
                            for (JSONObject q : active) {
                                boolean match =
                                        kind.equals("watchlist")
                                                ? group.getJSONArray("securities").stream()
                                                        .anyMatch(
                                                                x ->
                                                                        sec.equals(
                                                                                ((JSONObject) x)
                                                                                        .getString(
                                                                                                "securityId")))
                                                : FilterEngine.match(
                                                        refs.enrich(q),
                                                        JsonUtils.object("conditions", cs));
                                if (match) {
                                    links.add(q.getString("quoteId"));
                                    reasonQuotes.add(q.getString("quoteId"));
                                    hit = true;
                                }
                            }
                            if (hit) matched.add(group.getString(key));
                        }
                        if (!matched.isEmpty())
                            reasons.add(
                                    JsonUtils.object(
                                            "type",
                                            kind.equals("watchlist") ? "WATCHLIST" : "FILTER_GROUP",
                                            "groupIds",
                                            matched,
                                            "quoteIds",
                                            new ArrayList<>(reasonQuotes),
                                            "text",
                                            kind.equals("watchlist") ? "命中关注债券分组" : "命中行情筛选分组"));
                    }
                if (pref.getBooleanValue("myBestEnabled")) {
                    Set<String> best = bestIds(sec, bond.getValue(), active);
                    List<String> mine =
                            active.stream()
                                    .filter(
                                            q ->
                                                    user.equals(q.getString("ownerUserId"))
                                                            && best.contains(
                                                                    q.getString("quoteId")))
                                    .map(q -> q.getString("quoteId"))
                                    .sorted()
                                    .collect(Collectors.toList());
                    if (!mine.isEmpty()) {
                        links.addAll(mine);
                        reasons.add(
                                JsonUtils.object(
                                        "type",
                                        "MY_BEST",
                                        "quoteIds",
                                        mine,
                                        "text",
                                        "我的报价位于同方向最优档（含并列）"));
                    }
                }
                if (!reasons.isEmpty()) putDesired(desired, user, sec, "NORMAL", reasons, links);
                if (pref.getBooleanValue("counterQuoteEnabled"))
                    for (JSONObject mine : active)
                        if (user.equals(mine.getString("ownerUserId")))
                            for (JSONObject other : active)
                                if (!user.equals(other.getString("ownerUserId"))
                                        && !Objects.equals(
                                                mine.getString("side"), other.getString("side"))) {
                                    String a = mine.getString("quoteId"),
                                            b = other.getString("quoteId");
                                    putDesired(
                                            desired,
                                            user,
                                            sec,
                                            "COUNTER:" + a + ":" + b,
                                            JsonUtils.array(
                                                    JsonUtils.object(
                                                            "type",
                                                            "COUNTER_QUOTE",
                                                            "ownQuoteId",
                                                            a,
                                                            "otherQuoteId",
                                                            b,
                                                            "text",
                                                            "同券、反方向的本人及他人有效报价")),
                                            new TreeSet<>(Arrays.asList(a, b)));
                                }
            }
        }
        List<JSONObject> events = new ArrayList<>();
        Map<String, JSONObject> changes = new LinkedHashMap<>();
        try {
            store.begin();
            for (Map.Entry<String, JSONObject> entry : desired.entrySet()) {
                JSONObject candidate = entry.getValue(), prev = notifications.get(entry.getKey());
                if (prev != null
                        && prev.getBooleanValue("active")
                        && Objects.equals(
                                candidate.getString("contentToken"), prev.getString("contentToken"))
                        && JsonUtils.canonicalHash(candidate.get("reasons"))
                                .equals(JsonUtils.canonicalHash(prev.get("reasons")))
                        && JsonUtils.canonicalHash(candidate.get("triggers"))
                                .equals(JsonUtils.canonicalHash(prev.get("triggers")))) continue;
                candidate.put(
                        "createTime",
                        prev == null ? BusinessClock.currentTime() : prev.getString("createTime"));
                candidate.put("updateTime", BusinessClock.currentTime());
                candidate.put("active", true);
                if (prev != null && prev.getString("readTime") != null && prev.getJSONArray("triggers") == null) {
                    for (JSONObject trigger : candidate.getJSONArray("triggers").toJavaList(JSONObject.class)) {
                        String triggerKey = trigger.getString("alertKey");
                        store.update("INSERT INTO notification_reads VALUES (?,?,?) ON CONFLICT(id,content_token) DO NOTHING",
                                triggerKey, triggerKey, prev.getString("readTime"));
                    }
                }
                candidate.put("readTime", triggerReadTime(candidate));
                save(candidate);
                changes.put(entry.getKey(), candidate);
                events.add(event("NOTIFICATION", "UPSERT", entry.getKey(), visible(candidate)));
            }
            for (JSONObject prev : notifications.values())
                if (prev.getBooleanValue("active")
                        && !desired.containsKey(prev.getString("notificationId"))) {
                    JSONObject n = JsonUtils.deepCopy(prev);
                    n.put("active", false);
                    save(n);
                    changes.put(n.getString("notificationId"), n);
                    events.add(
                            event("NOTIFICATION", "REMOVE", n.getString("notificationId"), null));
                }
            store.commit();
        } catch (Exception e) {
            store.rollback();
            throw e;
        }
        notifications.putAll(changes);
        if (!events.isEmpty()) {
            committed(events);
            pushViews(null);
        }
    }

    void putDesired(
            Map<String, JSONObject> desired,
            String user,
            String sec,
            String key,
            JSONArray reasons,
            Set<String> ids) {
        Map<String, String> versions = new TreeMap<>();
        for (String q : ids) versions.put(q, quotes.get(q).getString("revision"));
        String token = JsonUtils.canonicalHash(versions);
        String id =
                "N-"
                        + JsonUtils.canonicalHash(Arrays.asList(user, sec, key, token))
                                .substring(0, 32);
        desired.put(
                id,
                JsonUtils.object(
                        "notificationId",
                        id,
                        "userId",
                        user,
                        "securityId",
                        sec,
                        "reasons",
                        reasons,
                        "content",
                        String.join(
                                "；",
                                reasons.toJavaList(JSONObject.class).stream()
                                        .map(r -> r.getString("text"))
                                        .collect(Collectors.toList())),
                        "quoteIds",
                        new ArrayList<>(ids),
                        "contentToken",
                        token,
                        "alertKey",
                        id + ":" + token,
                        "triggers",
                        notificationTriggers(user, reasons, ids)));
    }

    /** 合并行 ID 仅用于列表；弹窗按类型和具体来源报价版本去重，移除同券其他报价不产生新实例。 */
    private JSONArray notificationTriggers(String user, JSONArray reasons, Set<String> linkedIds) {
        JSONArray triggers = new JSONArray();
        for (JSONObject reason : reasons.toJavaList(JSONObject.class)) {
            String type = reason.getString("type");
            if ("COUNTER_QUOTE".equals(type)) {
                triggers.add(notificationTrigger(user, type, linkedIds));
            } else {
                for (String quoteId : reason.getJSONArray("quoteIds").toJavaList(String.class)) {
                    triggers.add(notificationTrigger(user, type, Collections.singleton(quoteId)));
                }
            }
        }
        return triggers;
    }

    private JSONObject notificationTrigger(String user, String type, Set<String> quoteIds) {
        Map<String, String> versions = new TreeMap<>();
        for (String quoteId : quoteIds) {
            versions.put(quoteId, quotes.get(quoteId).getString("revision"));
        }
        JSONArray snapshots = new JSONArray();
        for (String quoteId : versions.keySet()) {
            JSONObject source = quotes.get(quoteId);
            JSONObject snapshot = new JSONObject();
            // 弹窗只携带触发报价的公共字段；不包含对手机构、联系人或备注。
            for (String field : Arrays.asList("quoteId", "revision", "securityId", "ownerUserId", "side",
                    "quoteType", "yieldType", "price", "quantity", "quantityUnit", "settlType", "updateTime")) {
                snapshot.put(field, source.get(field));
            }
            snapshots.add(snapshot);
        }
        return JsonUtils.object("alertKey", "A-" + JsonUtils.canonicalHash(Arrays.asList(user, type, versions)),
                "type", type, "quoteIds", new ArrayList<>(versions.keySet()), "quotes", snapshots);
    }

    /** 已读归属于独立实例，合并行缩减成员时不把剩余已读报价变回未读。 */
    private String triggerReadTime(JSONObject notification) throws Exception {
        String latest = null;
        for (JSONObject trigger : notification.getJSONArray("triggers").toJavaList(JSONObject.class)) {
            String key = trigger.getString("alertKey");
            String readTime = store.scalar("SELECT read_time FROM notification_reads WHERE id=? AND content_token=?", key, key);
            if (readTime == null) return null;
            if (latest == null || readTime.compareTo(latest) > 0) latest = readTime;
        }
        return latest;
    }

    JSONObject visible(JSONObject n) {
        JSONObject out = JsonUtils.deepCopy(n);
        out.remove("active");
        out.remove("contentToken");
        return out;
    }

    void save(JSONObject n) throws Exception {
        store.update(
                "INSERT INTO notifications VALUES (?,?,?,?::jsonb) ON CONFLICT(id) DO UPDATE SET"
                        + " active=excluded.active,payload=excluded.payload",
                n.getString("notificationId"),
                n.getString("userId"),
                n.getBooleanValue("active"),
                JsonUtils.toJson(n));
    }

    JSONObject event(String type, String action, String id, JSONObject record) throws Exception {
        return store.event(
                JsonUtils.object("type", type, "action", action, "id", id, "record", record));
    }

    void committed(List<JSONObject> events) {
        if (events.isEmpty()) return;
        long first = events.get(0).getLongValue("sequence");
        lastSequence = events.get(events.size() - 1).getLongValue("sequence");
        publish.accept(
                "ficc.notification.events",
                JsonUtils.object(
                        "Code",
                        "0000",
                        "Msg",
                        "成功",
                        "kind",
                        "EVENTS",
                        "epoch",
                        store.epoch,
                        "fromSequence",
                        Long.toString(first),
                        "toSequence",
                        Long.toString(lastSequence),
                        "events",
                        events));
    }

    synchronized JSONObject handle(String user, String sid, String topic, JSONObject request) {
        String rid = request.getString("requestId");
        try {
            request = ProtocolBoundary.request(topic, request);
            RequestValidator.requireString(request, "requestId");
            if (user == null)
                throw ApiException.forField("1003", "FORBIDDEN", "Unknown login session", null);
            String op = topic.replace("ficc.notification.", "");
            Object data;
            if (request.getString("cursor") != null)
                data = continuePage(user, sid, topic, request.getString("cursor"));
            if (request.getString("cursor") != null)
                data = continuePage(user, sid, topic, request.getString("cursor"));
            else data = dispatch(op, user, sid, topic, request);
            return ProtocolBoundary.reply(topic, ApiResponse.success(rid, data));
        } catch (ApiException e) {
            return ApiResponse.failure(rid, e);
        } catch (Exception e) {
            store.rollback();
            System.err.println("Notification request failed: " + topic + " " + e);
            return ApiResponse.failure(
                    rid,
                    ApiException.forField(
                            "1099", "INTERNAL_ERROR", "Notification operation failed", null));
        }
    }

    Object dispatch(String op, String user, String sid, String topic, JSONObject request)
            throws Exception {
        if ("preference.get".equals(op)) return JsonUtils.object("preference", preference(user));
        if ("preference.save".equals(op)) return savePreference(user, topic, request);
        if ("read".equals(op)) return read(user, topic, request);
        if ("query".equals(op))
            return page(
                    user,
                    sid,
                    topic,
                    "notifications",
                    select(user, request),
                    request,
                    JsonUtils.object());
        if ("stream.open".equals(op)) {
            NotifyStream s = stream(user, sid);
            return JsonUtils.object(
                    "sessionId",
                    sid,
                    "streamId",
                    s.id,
                    "pushTopic",
                    "ficc.notification.report." + sid);
        }
        if ("view.open".equals(op)) return openView(user, sid, request);
        if ("view.query".equals(op)) return queryViewPage(user, sid, request);
        if ("view.close".equals(op)) {
            NotifyStream s = stream(user, sid);
            String id = RequestValidator.requireString(request, "viewId");
            s.views.remove(id);
            return JsonUtils.object("viewId", id, "closed", true);
        }
        if ("filterGroup.query".equals(op))
            return JsonUtils.object(
                    "filterGroups",
                    groups.values().stream()
                            .filter(g -> user.equals(g.getString("userId")))
                            .collect(Collectors.toList()));
        if ("filterGroup.save".equals(op) || "filterGroup.delete".equals(op))
            return saveGroup(user, topic, request, op.endsWith("delete"));
        if ("state.query".equals(op)) return state(user, sid, topic, request);
        if ("event.query".equals(op)) return events(user, request);
        throw ApiException.forField("1010", "UNSUPPORTED", "Unknown notification interface", null);
    }

    List<JSONObject> select(String user, JSONObject r) {
        if (!ready)
            throw ApiException.forField("1009", "NOT_READY", "Quote source is synchronizing", null);
        String read = JsonUtils.getStringOrDefault(r, "readState", "ALL");
        if (!Arrays.asList("ALL", "READ", "UNREAD").contains(read))
            throw ApiException.forField("1001", "VALIDATION_FAILED", "Invalid readState", null);
        JSONObject filter = r.getJSONObject("filter");
        FilterEngine.validate(filter);
        return notifications.values().stream()
                .filter(n -> n.getBooleanValue("active") && user.equals(n.getString("userId")))
                .filter(
                        n ->
                                read.equals("ALL")
                                        || (read.equals("READ") == (n.get("readTime") != null)))
                .filter(n -> matches(user, n, filter))
                .sorted(
                        Comparator.<JSONObject, String>comparing(n -> n.getString("updateTime"))
                                .reversed()
                                .thenComparing(n -> n.getString("notificationId")))
                .map(this::visible)
                .collect(Collectors.toList());
    }

    boolean matches(String user, JSONObject n, JSONObject f) {
        List<JSONObject> related =
                n.getJSONArray("quoteIds").toJavaList(String.class).stream()
                        .map(quotes::get)
                        .filter(Objects::nonNull)
                        .collect(Collectors.toList());
        if (related.stream().noneMatch(q -> FilterEngine.match(refs.enrich(q), f))) return false;
        if (f != null
                && f.getJSONArray("filterGroupIds") != null
                && !f.getJSONArray("filterGroupIds").isEmpty()) {
            boolean any = false;
            for (String id : f.getJSONArray("filterGroupIds").toJavaList(String.class)) {
                JSONObject g = groups.get(id);
                if (g == null || !user.equals(g.getString("userId")))
                    throw ApiException.forField(
                            "1002", "NOT_FOUND", "Notification filter group not found", null);
                JSONArray cs = new JSONArray();
                for (JSONObject sub : g.getJSONArray("subgroups").toJavaList(JSONObject.class))
                    cs.addAll(sub.getJSONArray("conditions"));
                any |=
                        related.stream()
                                .anyMatch(
                                        q ->
                                                FilterEngine.match(
                                                        refs.enrich(q),
                                                        JsonUtils.object(
                                                                "market",
                                                                g.getString("market"),
                                                                "conditions",
                                                                cs)));
            }
            return any;
        }
        return true;
    }

    Object savePreference(String user, String topic, JSONObject r) throws Exception {
        RequestValidator.allowOnlyFields(
                r,
                "requestId",
                "expectedRevision",
                "watchlistEnabled",
                "filterGroupEnabled",
                "counterQuoteEnabled",
                "myBestEnabled");
        JSONArray prior =
                store.receipt(user, topic, r.getString("requestId"), JsonUtils.canonicalHash(r));
        if (!prior.isEmpty()) return prior.get(0);
        JSONObject prev = preference(user);
        if (!RequestValidator.requireString(r, "expectedRevision")
                .equals(prev.getString("revision")))
            throw ApiException.forField("1004", "REVISION_CONFLICT", "Preferences changed", null);
        JSONObject p =
                JsonUtils.object(
                        "userId",
                        user,
                        "revision",
                        Long.toString(prev.getLongValue("revision") + 1),
                        "updateTime",
                        BusinessClock.currentTime());
        for (String key : SWITCHES) {
            if (!(r.get(key) instanceof Boolean))
                throw ApiException.forField("1001", "VALIDATION_FAILED", "Boolean required", key);
            p.put(key, r.get(key));
        }
        JSONObject out = JsonUtils.object("preference", p), event;
        try {
            store.begin();
            store.update(
                    "INSERT INTO notification_preferences VALUES (?,?::jsonb) ON CONFLICT(user_id)"
                            + " DO UPDATE SET payload=excluded.payload",
                    user,
                    JsonUtils.toJson(p));
            event = event("PREFERENCE", "UPSERT", user, p);
            store.receipt(user, topic, r.getString("requestId"), JsonUtils.array(out));
            store.commit();
        } catch (Exception e) {
            store.rollback();
            throw e;
        }
        preferences.put(user, p);
        committed(Collections.singletonList(event));
        recompute();
        pushViews(p);
        return out;
    }

    Object read(String user, String topic, JSONObject r) throws Exception {
        RequestValidator.allowOnlyFields(r, "requestId", "notificationIds", "read");
        if (!(r.get("read") instanceof Boolean))
            throw ApiException.forField("1001", "VALIDATION_FAILED", "read must be boolean", null);
        JSONArray ids = r.getJSONArray("notificationIds");
        if (ids == null || ids.isEmpty() || ids.size() > 200)
            throw ApiException.forField(
                    "1008", "RESOURCE_LIMIT", "1..200 notification IDs required", null);
        JSONArray prior =
                store.receipt(user, topic, r.getString("requestId"), JsonUtils.canonicalHash(r));
        if (!prior.isEmpty()) return prior.get(0);
        JSONArray results = new JSONArray();
        Map<String, JSONObject> changed = new LinkedHashMap<>();
        List<JSONObject> events = new ArrayList<>();
        try {
            store.begin();
            for (String id : ids.toJavaList(String.class)) {
                JSONObject before = notifications.get(id);
                if (before == null
                        || !user.equals(before.getString("userId"))
                        || !before.getBooleanValue("active")) {
                    results.add(
                            JsonUtils.object("notificationId", id, "Code", "1002", "Msg", "通知不存在"));
                    continue;
                }
                JSONObject n = JsonUtils.deepCopy(before);
                String time = r.getBooleanValue("read") ? BusinessClock.currentTime() : null;
                n.put("readTime", time);
                n.put("updateTime", BusinessClock.currentTime());
                save(n);
                store.update(
                        "INSERT INTO notification_reads VALUES (?,?,?) ON"
                                + " CONFLICT(id,content_token) DO UPDATE SET"
                                + " read_time=excluded.read_time",
                        id,
                        n.getString("contentToken"),
                        time);
                JSONArray triggers = n.getJSONArray("triggers");
                if (triggers != null) {
                    for (JSONObject trigger : triggers.toJavaList(JSONObject.class)) {
                        String key = trigger.getString("alertKey");
                        store.update("INSERT INTO notification_reads VALUES (?,?,?) ON CONFLICT(id,content_token)"
                                + " DO UPDATE SET read_time=excluded.read_time", key, key, time);
                    }
                }
                changed.put(id, n);
                events.add(event("NOTIFICATION", "UPSERT", id, visible(n)));
                results.add(
                        JsonUtils.object(
                                "notificationId",
                                id,
                                "Code",
                                "0000",
                                "Msg",
                                "成功",
                                "readTime",
                                time));
            }
            JSONObject out = JsonUtils.object("results", results);
            store.receipt(user, topic, r.getString("requestId"), JsonUtils.array(out));
            store.commit();
            notifications.putAll(changed);
            committed(events);
            pushViews(null);
            return out;
        } catch (Exception e) {
            store.rollback();
            throw e;
        }
    }

    Object saveGroup(String user, String topic, JSONObject request, boolean delete)
            throws Exception {
        JSONArray prior =
                store.receipt(
                        user,
                        topic,
                        request.getString("requestId"),
                        JsonUtils.canonicalHash(request));
        if (!prior.isEmpty()) {
            return prior.get(0);
        }
        String id = request.getString("filterGroupId");
        if (delete) {
            RequestValidator.requireString(request, "filterGroupId");
        }
        JSONObject previous = id == null ? null : groups.get(id);
        if (id != null && (previous == null || !user.equals(previous.getString("userId")))) {
            throw ApiException.forField("1002", "NOT_FOUND", "Filter group not found", null);
        }
        if (previous != null
                && !RequestValidator.requireString(request, "expectedRevision")
                        .equals(previous.getString("revision"))) {
            throw ApiException.forField("1004", "REVISION_CONFLICT", "Filter group changed", null);
        }

        JSONObject group = JsonUtils.deepCopy(request);
        group.remove("requestId");
        group.remove("expectedRevision");
        if (id == null) {
            id = IdGenerator.newId("NFG-");
        }
        group.put("filterGroupId", id);
        group.put("userId", user);
        group.put("scope", "NOTIFICATION");
        group.put(
                "revision",
                Long.toString(previous == null ? 1 : previous.getLongValue("revision") + 1));
        group.put(
                "createTime",
                previous == null ? BusinessClock.currentTime() : previous.getString("createTime"));
        group.put("updateTime", BusinessClock.currentTime());
        if (!delete) {
            RequestValidator.requireString(group, "name");
            if (!Arrays.asList("RATE", "CREDIT")
                    .contains(RequestValidator.requireString(group, "market"))) {
                throw ApiException.forField("1001", "VALIDATION_FAILED", "Invalid market", null);
            }
            JSONArray subgroups = group.getJSONArray("subgroups");
            if (subgroups == null) {
                throw ApiException.forField(
                        "1001", "VALIDATION_FAILED", "subgroups required", null);
            }
            JSONArray cleanSubgroups = new JSONArray();
            for (JSONObject subgroup : subgroups.toJavaList(JSONObject.class)) {
                JSONArray conditions = subgroup.getJSONArray("conditions");
                if (conditions == null || conditions.isEmpty()) {
                    continue;
                }
                FilterEngine.validate(JsonUtils.object("conditions", conditions));
                subgroup.putIfAbsent("subgroupId", IdGenerator.newId("NSG-"));
                cleanSubgroups.add(subgroup);
            }
            group.put("subgroups", cleanSubgroups);
        }
        JSONObject result =
                delete
                        ? JsonUtils.object("filterGroupId", id, "deleted", true)
                        : JsonUtils.object("filterGroup", group);
        JSONObject changeEvent;
        try {
            store.begin();
            if (delete) {
                store.update("DELETE FROM " + store.table("settings") + " WHERE id=?", id);
            } else {
                store.update(
                        "INSERT INTO "
                                + store.table("settings")
                                + " VALUES (?, ?, ?, ?, ?::jsonb) ON CONFLICT(id) DO UPDATE SET"
                                + " revision=excluded.revision, payload=excluded.payload",
                        id,
                        user,
                        "filterGroup",
                        group.getLongValue("revision"),
                        JsonUtils.toJson(group));
            }
            changeEvent =
                    event("FILTER_GROUP", delete ? "REMOVE" : "UPSERT", id, delete ? null : group);
            store.receipt(user, topic, request.getString("requestId"), JsonUtils.array(result));
            store.commit();
        } catch (Exception error) {
            store.rollback();
            throw error;
        }
        if (delete) {
            groups.remove(id);
        } else {
            groups.put(id, group);
        }
        committed(Collections.singletonList(changeEvent));
        pushViews(null);
        return result;
    }

    NotifyStream stream(String user, String sid) {
        return streams.computeIfAbsent(
                sid,
                k -> {
                    NotifyStream s = new NotifyStream();
                    s.user = user;
                    s.sid = sid;
                    s.id = IdGenerator.newId("NE-");
                    return s;
                });
    }

    int limit(JSONObject r) {
        int n = r.getIntValue("limit", 200);
        if (n < 1 || n > 1000)
            throw ApiException.forField("1008", "RESOURCE_LIMIT", "limit must be 1..1000", null);
        return n;
    }

    Object openView(String user, String sid, JSONObject request) {
        RequestValidator.allowOnlyFields(
                request, "requestId", "viewId", "readState", "filter", "pageSize");
        NotifyStream stream = stream(user, sid);
        String id = RequestValidator.requireString(request, "viewId");
        if (stream.used.contains(id) || stream.views.size() >= 20) {
            throw ApiException.forField(
                    "1008", "RESOURCE_LIMIT", "View exists or limit reached", null);
        }
        NotifyView view = new NotifyView();
        view.id = id;
        view.request = JsonUtils.deepCopy(request);
        view.page.configure(request, true);
        refresh(stream, view);
        stream.used.add(id);
        stream.views.put(id, view);
        return viewResult(stream, view);
    }

    Object queryViewPage(String user, String sid, JSONObject request) {
        RequestValidator.allowOnlyFields(request, "requestId", "viewId", "pageNumber", "pageSize");
        NotifyStream stream = stream(user, sid);
        NotifyView view = stream.views.get(RequestValidator.requireString(request, "viewId"));
        if (view == null) {
            throw ApiException.forField("1002", "NOT_FOUND", "View not found", null);
        }
        view.page.configure(request, false);
        refresh(stream, view);
        return viewResult(stream, view);
    }

    void refresh(NotifyStream stream, NotifyView view) {
        List<JSONObject> rows = select(stream.user, view.request);
        view.page.initialize(rows, "notificationId");
        view.loaded = new LinkedHashMap<>();
        for (JSONObject row : view.page.rows(rows)) {
            view.loaded.put(row.getString("notificationId"), row);
        }
        view.ids = new ArrayList<>(view.loaded.keySet());
    }

    JSONObject viewResult(NotifyStream stream, NotifyView view) {
        return JsonUtils.object(
                "streamId",
                stream.id,
                "sequence",
                Long.toString(stream.sequence),
                "view",
                JsonUtils.object(
                        "viewId",
                        view.id,
                        "pageNumber",
                        view.page.pageNumber,
                        "pageSize",
                        view.page.pageSize,
                        "totalCount",
                        select(stream.user, view.request).size(),
                        "ids",
                        new ArrayList<>(view.ids)),
                "notifications",
                new ArrayList<>(view.loaded.values()));
    }

    void pushViews(JSONObject preference) {
        for (NotifyStream stream : streams.values()) {
            JSONArray views = new JSONArray();
            Map<String, JSONObject> changedRows = new LinkedHashMap<>();
            for (NotifyView view : stream.views.values()) {
                try {
                    JSONObject delta =
                            view.page.changes(
                                    view.id,
                                    select(stream.user, view.request),
                                    "notificationId",
                                    changedRows);
                    view.loaded.clear();
                    view.ids.clear();
                    if (!delta.getJSONArray("upsertIds").isEmpty()
                            || !delta.getJSONArray("removeIds").isEmpty()) {
                        views.add(delta);
                    }
                } catch (ApiException error) {
                    publish.accept(
                            "ficc.notification.report." + stream.sid,
                            JsonUtils.object(
                                    "Code",
                                    "1007",
                                    "kind",
                                    "RESET_REQUIRED",
                                    "streamId",
                                    stream.id,
                                    "sequence",
                                    Long.toString(++stream.sequence)));
                }
            }
            boolean ownPreference =
                    preference != null && stream.user.equals(preference.getString("userId"));
            if (views.isEmpty() && !ownPreference) {
                continue;
            }
            JSONObject message =
                    JsonUtils.object(
                            "Code",
                            "0000",
                            "Msg",
                            "Success",
                            "kind",
                            "UPDATE",
                            "streamId",
                            stream.id,
                            "sequence",
                            Long.toString(++stream.sequence),
                            "notifications",
                            new ArrayList<>(changedRows.values()),
                            "views",
                            views);
            if (ownPreference) {
                message.put("preference", preference);
            }
            publish.accept("ficc.notification.report." + stream.sid, message);
        }
    }

    synchronized void heartbeat() {
        for (NotifyStream s : streams.values())
            publish.accept(
                    "ficc.notification.report." + s.sid,
                    JsonUtils.object(
                            "Code",
                            "0000",
                            "kind",
                            "HEARTBEAT",
                            "streamId",
                            s.id,
                            "sequence",
                            Long.toString(++s.sequence)));
    }

    Object page(
            String user,
            String sid,
            String topic,
            String field,
            List<JSONObject> rows,
            JSONObject r,
            JSONObject extra) {
        return pageResult(
                new Page(
                        user,
                        sid,
                        topic,
                        field,
                        rows.stream().map(JsonUtils::deepCopy).collect(Collectors.toList()),
                        0,
                        limit(r),
                        System.currentTimeMillis() + 120000,
                        extra));
    }

    Object pageResult(Page p) {
        int end = Math.min(p.rows.size(), p.offset + p.limit);
        JSONObject result = JsonUtils.deepCopy(p.extra);
        result.put(p.field, p.rows.subList(p.offset, end));
        String cursor = null;
        if (end < p.rows.size()) {
            pages.entrySet().removeIf(e -> e.getValue().expires < System.currentTimeMillis());
            if (pages.size() >= 64)
                throw ApiException.forField("1008", "RESOURCE_LIMIT", "Too many cursors", null);
            cursor = IdGenerator.newId("NC-");
            pages.put(
                    cursor,
                    new Page(
                            p.user, p.sid, p.topic, p.field, p.rows, end, p.limit, p.expires,
                            p.extra));
        }
        result.put("nextCursor", cursor);
        result.put("totalCount", p.rows.size());
        if (p.topic.endsWith("state.query")) finishState(result);
        return result;
    }

    Object continuePage(String user, String sid, String topic, String id) {
        Page p = pages.get(id);
        if (p == null
                || !p.user.equals(user)
                || !p.sid.equals(sid)
                || !p.topic.equals(topic)
                || p.expires < System.currentTimeMillis())
            throw ApiException.forField("1007", "RESET_REQUIRED", "Cursor expired", null);
        return pageResult(p);
    }

    Object state(String user, String sid, String topic, JSONObject r) {
        internal(user);
        List<JSONObject> rows = new ArrayList<>();
        notifications.values().stream()
                .filter(n -> n.getBooleanValue("active"))
                .forEach(
                        n ->
                                rows.add(
                                        JsonUtils.object(
                                                "collection",
                                                "notifications",
                                                "record",
                                                visible(n))));
        preferences
                .values()
                .forEach(p -> rows.add(JsonUtils.object("collection", "preferences", "record", p)));
        groups.values()
                .forEach(
                        g -> rows.add(JsonUtils.object("collection", "filterGroups", "record", g)));
        return page(
                user,
                sid,
                topic,
                "entries",
                rows,
                r,
                JsonUtils.object("epoch", store.epoch, "sequence", Long.toString(lastSequence)));
    }

    void finishState(JSONObject result) {
        JSONObject records =
                JsonUtils.object(
                        "notifications",
                        JsonUtils.array(),
                        "preferences",
                        JsonUtils.array(),
                        "filterGroups",
                        JsonUtils.array());
        for (Object x : (List<?>) result.remove("entries")) {
            JSONObject row = (JSONObject) x;
            records.getJSONArray(row.getString("collection")).add(row.get("record"));
        }
        result.put("records", records);
        result.put("done", result.get("nextCursor") == null);
    }

    Object events(String user, JSONObject request) throws Exception {
        internal(user);
        long after;
        try {
            after = Long.parseLong(RequestValidator.requireString(request, "afterSequence"));
        } catch (NumberFormatException error) {
            throw ApiException.forField("1001", "VALIDATION_FAILED", "Invalid afterSequence", null);
        }
        if (!Objects.equals(store.epoch, request.getString("epoch"))
                || after < 0
                || after > lastSequence) {
            throw ApiException.forField("1007", "RESET_REQUIRED", "Invalid event position", null);
        }
        List<JSONObject> events =
                store.rows(
                        "SELECT payload FROM "
                                + store.table("events")
                                + " WHERE sequence>? ORDER BY sequence LIMIT ?",
                        after,
                        limit(request));
        long nextSequence =
                events.isEmpty() ? after : events.get(events.size() - 1).getLongValue("sequence");
        return JsonUtils.object(
                "epoch",
                store.epoch,
                "events",
                events,
                "nextSequence",
                Long.toString(nextSequence),
                "seenSequence",
                Long.toString(lastSequence),
                "hasMore",
                nextSequence < lastSequence);
    }

    void internal(String user) {
        if (!access.internal(user))
            throw ApiException.forField("1003", "FORBIDDEN", "Internal service required", null);
    }
}
