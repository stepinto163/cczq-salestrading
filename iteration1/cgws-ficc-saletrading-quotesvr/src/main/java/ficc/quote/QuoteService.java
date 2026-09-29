package ficc.quote;

import com.alibaba.fastjson2.*;

import ficc.common.*;
import ficc.common.client.AccessPolicy;
import ficc.common.client.QuoteSvrClient;

import java.math.BigDecimal;
import java.time.*;
import java.util.*;
import java.util.function.*;
import java.util.stream.Collectors;

/**
 * All QuoteSvr request handlers share one ordered standalone dispatcher, not a shared Quote/Price
 * DTO.
 */
final class QuoteService {
    static final List<String> EDIT =
            Arrays.asList(
                    "securityId",
                    "institutionId",
                    "institutionName",
                    "contactId",
                    "contactName",
                    "side",
                    "quoteType",
                    "yieldType",
                    "price",
                    "quantity",
                    "quantityUnit",
                    "currency",
                    "tradeDate",
                    "settlType",
                    "urgent",
                    "recommended",
                    "whole",
                    "oco",
                    "packing",
                    "remark");
    static final List<String> FLAGS =
            Arrays.asList("urgent", "recommended", "whole", "oco", "packing");
    final PgStore store;
    final ReferenceData refs;
    final AccessPolicy access;
    final Map<String, JSONObject> quotes = new LinkedHashMap<>();
    final Map<String, JSONObject> settings = new LinkedHashMap<>();
    final ViewService views;
    final SearchSvrClient search;
    final QuoteMatcher matcher;
    BiConsumer<String, JSONObject> publish = (t, v) -> {};
    final Map<String, Cursor> cursors = new LinkedHashMap<>();
    final Map<String, Function<Call, Object>> handlers = new LinkedHashMap<>();
    long lastSequence;

    static final class Call {
        final String user;
        final String session;
        final String topic;
        final JSONObject request;

        Call(String user, String session, String topic, JSONObject request) {
            this.user = user;
            this.session = session;
            this.topic = topic;
            this.request = request;
        }

        String user() {
            return user;
        }

        String session() {
            return session;
        }

        String topic() {
            return topic;
        }

        JSONObject request() {
            return request;
        }
    }

    static final class Cursor {
        final String user;
        final String session;
        final String topic;
        final String field;
        final List<JSONObject> rows;
        final int offset;
        final int limit;
        final long expires;
        final JSONObject extra;

        Cursor(
                String user,
                String session,
                String topic,
                String field,
                List<JSONObject> rows,
                int offset,
                int limit,
                long expires,
                JSONObject extra) {
            this.user = user;
            this.session = session;
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

        String session() {
            return session;
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

    QuoteService(PgStore store, ReferenceData refs) throws Exception {
        this(store, refs, new AccessPolicy(new Properties()));
    }

    QuoteService(PgStore store, ReferenceData refs, AccessPolicy access) throws Exception {
        this.access = access;
        this.store = store;
        this.refs = refs;
        for (JSONObject q : store.quotes()) quotes.put(q.getString("quoteId"), q);
        for (JSONObject group : store.rows("SELECT payload FROM settings")) {
            FilterDictionaries.migrateSavedFilters(group);
            settings.put(settingId(group), group);
        }
        matcher = new QuoteMatcher(refs, settings);
        search = new LocalSearchSvrClient(store, refs, quotes, matcher, access);
        lastSequence = store.tail();
        views = new ViewService(this);
        for (String op :
                Arrays.asList("create", "confirm", "update", "refer", "unrefer", "delete", "clone"))
            handlers.put("ficc.quote." + op, c -> unchecked(() -> write(c, op)));
        handlers.put(
                "ficc.quote.get",
                c ->
                        JsonUtils.object(
                                "quote",
                                visible(get(c.request.getString("quoteId"), c.user), c.user)));
        handlers.put(
                "ficc.quote.query",
                c ->
                        page(
                                c,
                                "quotes",
                                selectQuotes(c.user, c.request).stream()
                                        .map(q -> visible(q, c.user))
                                        .collect(Collectors.toList()),
                                JsonUtils.object(
                                        "reference",
                                        referenceFor(selectQuotes(c.user, c.request)))));
        handlers.put("ficc.quote.history.query", c -> unchecked(() -> history(c)));
        handlers.put("ficc.quote.history.search", c -> unchecked(() -> historySearch(c)));
        handlers.put("ficc.quote.sourceMessage.query", c -> unchecked(() -> sourceQuery(c)));
        handlers.put("ficc.quote.dictionary.query", c -> dictionary(false, c.request));
        handlers.put("ficc.price.dictionary.query", c -> dictionary(true, c.request));
        handlers.put("ficc.price.query", c -> views.priceQuery(c));
        for (String d : Arrays.asList("quote", "price")) {
            handlers.put("ficc." + d + ".stream.open", c -> views.openStream(c, d));
            handlers.put("ficc." + d + ".view.open", c -> views.openView(c, d));
            handlers.put("ficc." + d + ".view.query", c -> views.queryPage(c, d));
            handlers.put("ficc." + d + ".view.close", c -> views.close(c, d));
        }
        for (String kind : Arrays.asList("watchlist", "filterGroup")) {
            handlers.put("ficc." + kind + ".query", c -> groupQuery(c, kind));
            handlers.put("ficc." + kind + ".save", c -> unchecked(() -> saveGroup(c, kind, false)));
            handlers.put(
                    "ficc." + kind + ".delete", c -> unchecked(() -> saveGroup(c, kind, true)));
        }
        handlers.put("ficc.quote.intake.apply", c -> unchecked(() -> intake(c)));
        handlers.put("ficc.quote.state.query", c -> state(c));
        handlers.put("ficc.quote.event.query", c -> unchecked(() -> events(c)));
        handlers.put("ficc.base.get", c -> reference(c, false));
        handlers.put("ficc.base.query", c -> reference(c, true));
    }

    interface Checked {
        Object get() throws Exception;
    }

    static Object unchecked(Checked f) {
        try {
            return f.get();
        } catch (ApiException e) {
            throw e;
        } catch (Exception e) {
            throw new RuntimeException(e);
        }
    }

    synchronized JSONObject handle(String user, String sid, String topic, JSONObject r) {
        String requestId = r.getString("requestId");
        try {
            RequestValidator.requireString(r, "requestId");
            if (user == null)
                throw ApiException.forField(
                        "1003", "FORBIDDEN", "Unknown or expired STC session", null);
            Function<Call, Object> h = handlers.get(topic);
            if (h == null)
                throw ApiException.forField(
                        "1010", "UNSUPPORTED", "Unknown interface: " + topic, null);
            Call c = new Call(user, sid, topic, r);
            if (r.getString("cursor") != null && !topic.endsWith("state.query"))
                return ApiResponse.success(requestId, continuePage(c));
            return ApiResponse.success(requestId, h.apply(c));
        } catch (ApiException e) {
            return ApiResponse.failure(requestId, e);
        } catch (Exception e) {
            store.rollback();
            System.err.println(
                    "Request failed: " + topic + " (" + e.getClass().getSimpleName() + ")");
            return ApiResponse.failure(
                    requestId,
                    ApiException.forField(
                            "1099",
                            "INTERNAL_ERROR",
                            "Internal error; see local server log",
                            null));
        }
    }

    JSONObject get(String id, String user) {
        JSONObject q = quotes.get(id);
        if (q == null || (!"ACTIVE".equals(q.getString("status")) && !owns(user, q)))
            throw ApiException.forField("1002", "NOT_FOUND", "Quote not found", "quoteId");
        return q;
    }

    boolean owns(String user, JSONObject quote) {
        return access.owns(user, quote.getString("ownerUserId"));
    }

    JSONObject visible(JSONObject q, String user) {
        JSONObject v = JsonUtils.deepCopy(q);
        if (!owns(user, q)) {
            for (String field :
                    Arrays.asList("institutionId", "institutionName", "contactId", "contactName"))
                v.put(field, null);
        }
        return v;
    }

    void revision(JSONObject q, JSONObject in) {
        String expected = RequestValidator.requireString(in, "expectedRevision");
        if (!expected.equals(q.getString("revision")))
            throw new ApiException(
                    "1004",
                    "报价已被更新",
                    JsonUtils.object(
                            "type",
                            "REVISION_CONFLICT",
                            "quoteId",
                            q.getString("quoteId"),
                            "expectedRevision",
                            expected,
                            "currentRevision",
                            q.getString("revision")));
    }

    Object write(Call c, String op) throws Exception {
        return write(c, op, null);
    }

    Object write(Call c, String op, JSONObject source) throws Exception {
        return write(c, op, source, JsonUtils.canonicalHash(c.request));
    }

    Object write(Call c, String op, JSONObject source, String receiptHash) throws Exception {
        RequestValidator.allowOnlyFields(c.request, "requestId", "quotes");
        JSONArray inputs = c.request.getJSONArray("quotes");
        if (inputs == null || inputs.isEmpty() || inputs.size() > 100)
            throw ApiException.forField(
                    "1008", "RESOURCE_LIMIT", "quotes requires 1..100 entries", "quotes");
        Set<String> ids = new HashSet<>();
        for (Object x : inputs) {
            if (!(x instanceof JSONObject))
                throw ApiException.forField(
                        "1001", "VALIDATION_FAILED", "Invalid quote object", "quotes");
            JSONObject in = (JSONObject) x;
            String id =
                    in.getString(
                            op.equals("create") || op.equals("clone")
                                    ? "clientQuoteId"
                                    : "quoteId");
            if (id != null && !ids.add(id))
                throw ApiException.forField(
                        "1001", "VALIDATION_FAILED", "Duplicate quote in batch", "quotes");
        }
        JSONArray results =
                store.receipt(c.user, c.topic, c.request.getString("requestId"), receiptHash);
        for (int i = results.size(); i < inputs.size(); i++) {
            JSONObject in = inputs.getJSONObject(i), q = null, event = null, result;
            String previousSecurity = null;
            try {
                store.begin();
                JSONObject before = null;
                boolean create = op.equals("create") || op.equals("clone");
                if (create) {
                    String clientId = RequestValidator.requireString(in, "clientQuoteId");
                    if (quotes.values().stream()
                            .anyMatch(
                                    x ->
                                            c.user.equals(x.getString("ownerUserId"))
                                                    && clientId.equals(
                                                            x.getString("clientQuoteId"))))
                        throw ApiException.forField(
                                "1006",
                                "IDEMPOTENCY_CONFLICT",
                                "clientQuoteId already exists",
                                "clientQuoteId");
                    q = newQuote(c.user, clientId);
                    if (op.equals("clone")) {
                        JSONObject src =
                                get(RequestValidator.requireString(in, "sourceQuoteId"), c.user);
                        revision(src, in);
                        for (String k : EDIT) q.put(k, src.get(k));
                        q.put("clonedFromQuoteId", src.getString("quoteId"));
                    }
                } else {
                    before = get(RequestValidator.requireString(in, "quoteId"), c.user);
                    if (!owns(c.user, before))
                        throw ApiException.forField(
                                "1003", "FORBIDDEN", "Only quote owner/admin may change it", null);
                    revision(before, in);
                    q = JsonUtils.deepCopy(before);
                    previousSecurity = before.getString("securityId");
                    String st = q.getString("status");
                    boolean valid = allows(op, st);
                    if (!valid)
                        throw ApiException.forField(
                                "1005",
                                "INVALID_STATE",
                                "Action " + op + " is not allowed for " + st,
                                "status");
                    q.put("revision", Long.toString(q.getLongValue("revision") + 1));
                }
                Set<String> allowed =
                        new HashSet<>(
                                Arrays.asList(
                                        "quoteId",
                                        "expectedRevision",
                                        "clientQuoteId",
                                        "sourceQuoteId",
                                        "reason"));
                if (create || op.equals("update") || op.equals("confirm")) allowed.addAll(EDIT);
                for (String k : in.keySet())
                    if (!allowed.contains(k))
                        throw ApiException.forField(
                                "1001", "VALIDATION_FAILED", "Field cannot be changed", k);
                for (String k : EDIT) if (in.containsKey(k)) q.put(k, in.get(k));
                if (op.equals("confirm") || op.equals("unrefer")) q.put("status", "ACTIVE");
                else if (op.equals("refer")) q.put("status", "REFERRED");
                else if (op.equals("delete")) q.put("status", "DELETED");
                if (source != null) {
                    q.put("source", source.getJSONObject("source"));
                    q.put("sourceMessageId", source.getString("sourceMessageId"));
                }
                validateQuote(q, q.getString("status").equals("ACTIVE"), c.user);
                String now = BusinessClock.currentTime();
                q.put("updatedBy", c.user);
                q.put("updateTime", now);
                if (op.equals("confirm")) {
                    q.put("confirmedBy", c.user);
                    q.put("confirmTime", now);
                }
                if (q.getString("status").equals("ACTIVE") && !op.equals("refer"))
                    q.put("quoteTime", now);
                store.saveQuote(q);
                event =
                        store.event(
                                JsonUtils.object(
                                        "type",
                                        "QUOTE",
                                        "action",
                                        op.toUpperCase(Locale.ROOT),
                                        "quoteId",
                                        q.getString("quoteId"),
                                        "quote",
                                        q,
                                        "operatorUserId",
                                        c.user,
                                        "operationTime",
                                        now,
                                        "requestId",
                                        c.request.getString("requestId")));
                store.history(
                        JsonUtils.object(
                                "eventId",
                                event.getString("eventId"),
                                "quoteId",
                                q.getString("quoteId"),
                                "revision",
                                q.getString("revision"),
                                "action",
                                op.toUpperCase(Locale.ROOT),
                                "operatorUserId",
                                c.user,
                                "operationTime",
                                now,
                                "before",
                                before,
                                "after",
                                q,
                                "reason",
                                in.get("reason")));
                result =
                        JsonUtils.object(
                                "quoteId",
                                q.getString("quoteId"),
                                "revision",
                                q.getString("revision"),
                                "Code",
                                "0000",
                                "Msg",
                                "成功");
                if (create) result.put("clientQuoteId", q.getString("clientQuoteId"));
                results.add(result);
                store.receipt(c.user, c.topic, c.request.getString("requestId"), results);
                store.commit();
            } catch (ApiException e) {
                store.rollback();
                // Dependency failures must be retried, not saved as a permanent receipt.
                if ("1009".equals(e.code) || "1099".equals(e.code)) {
                    throw e;
                }
                result =
                        JsonUtils.object(
                                "quoteId",
                                in.get("quoteId"),
                                "clientQuoteId",
                                in.get("clientQuoteId"),
                                "Code",
                                e.code,
                                "Msg",
                                e.getMessage(),
                                "error",
                                e.details);
                results.add(result);
                store.receipt(c.user, c.topic, c.request.getString("requestId"), results);
                q = null;
                event = null;
            } catch (Exception e) {
                store.rollback();
                throw e;
            }
            if (q != null) {
                quotes.put(q.getString("quoteId"), q);
                committed(event);
                views.changed(q, event, previousSecurity);
            }
        }
        return JsonUtils.object("results", results);
    }

    JSONObject newQuote(String user, String clientId) {
        JSONObject q =
                JsonUtils.object(
                        "quoteId",
                        IdGenerator.newId("Q-"),
                        "clientQuoteId",
                        clientId,
                        "sourceMessageId",
                        null,
                        "source",
                        JsonUtils.object("type", "MANUAL"),
                        "ownerUserId",
                        user,
                        "revision",
                        "1",
                        "status",
                        "PENDING_CONFIRM",
                        "quantityUnit",
                        "WAN",
                        "currency",
                        "CNY",
                        "createdBy",
                        user,
                        "updatedBy",
                        user,
                        "confirmedBy",
                        null,
                        "createTime",
                        BusinessClock.currentTime(),
                        "updateTime",
                        BusinessClock.currentTime(),
                        "quoteTime",
                        null,
                        "confirmTime",
                        null);
        for (String k : EDIT) q.putIfAbsent(k, null);
        for (String f : FLAGS) q.put(f, false);
        return q;
    }

    void validateQuote(JSONObject quote, boolean active) {
        validateQuote(quote, active, quote.getString("ownerUserId"));
    }

    void validateQuote(JSONObject q, boolean active, String userId) {
        for (String f : FLAGS)
            if (!(q.get(f) instanceof Boolean))
                throw ApiException.forField("1001", "VALIDATION_FAILED", "Expected boolean", f);
        if (q.getBooleanValue("oco") && q.getBooleanValue("packing"))
            throw ApiException.forField(
                    "1001", "VALIDATION_FAILED", "OCO and packing are mutually exclusive", "oco");
        if (q.getString("securityId") != null) refs.require(q.getString("securityId"));
        for (Map.Entry<String, Set<String>> e : quoteDictionaries().entrySet()) {
            if (q.get(e.getKey()) != null && !e.getValue().contains(q.getString(e.getKey())))
                throw ApiException.forField(
                        e.getKey().equals("settlType") ? "1010" : "1001",
                        "VALIDATION_FAILED",
                        "Unsupported dictionary value",
                        e.getKey());
        }
        if (!"1".equals(q.getString("quoteType")) && q.get("yieldType") != null)
            throw ApiException.forField(
                    "1001",
                    "VALIDATION_FAILED",
                    "Non-yield quote must have null yieldType",
                    "yieldType");
        for (String f : Arrays.asList("price", "quantity"))
            if (q.get(f) != null) {
                if (!(q.get(f) instanceof String))
                    throw ApiException.forField(
                            "1001", "VALIDATION_FAILED", "Use decimal string", f);
                try {
                    BigDecimal n = new BigDecimal(q.getString(f));
                    if (f.equals("price") && "1".equals(q.getString("quoteType")) && n.scale() > 4)
                        throw new IllegalArgumentException();
                    if (f.equals("quantity") && n.signum() <= 0)
                        throw new IllegalArgumentException();
                } catch (Exception e) {
                    throw ApiException.forField(
                            "1001", "VALIDATION_FAILED", "Invalid decimal / yield precision", f);
                }
            }
        if (q.get("tradeDate") != null)
            try {
                LocalDate.parse(q.getString("tradeDate"));
            } catch (Exception e) {
                throw ApiException.forField(
                        "1001", "VALIDATION_FAILED", "Invalid tradeDate", "tradeDate");
            }
        for (String field : Arrays.asList("institutionName", "contactName"))
            if (q.get(field) != null && !(q.get(field) instanceof String))
                throw ApiException.forField("1001", "VALIDATION_FAILED", "Expected text", field);
        validateCounterparty(q, active, userId);
        if (!"WAN".equals(q.getString("quantityUnit")) || !"CNY".equals(q.getString("currency")))
            throw ApiException.forField("1010", "UNSUPPORTED", "Client supports CNY / WAN", null);
        if (active) {
            for (String f :
                    Arrays.asList(
                            "securityId",
                            "side",
                            "quoteType",
                            "price",
                            "quantity",
                            "tradeDate",
                            "settlType")) RequestValidator.requireString(q, f);
            if ("1".equals(q.getString("quoteType")))
                RequestValidator.requireString(q, "yieldType");
        }
    }

    void validateCounterparty(JSONObject quote, boolean active, String userId) {
        for (String field :
                Arrays.asList("institutionId", "institutionName", "contactId", "contactName")) {
            Object value = quote.get(field);
            if (value != null && !(value instanceof String)) {
                throw ApiException.forField("1001", "VALIDATION_FAILED", "Expected text", field);
            }
            if (value instanceof String) {
                String text = (String) value;
                quote.put(field, text.trim().isEmpty() ? null : text.trim());
            }
        }
        String institutionId = quote.getString("institutionId");
        String contactId = quote.getString("contactId");
        if (institutionId != null) {
            JSONObject institution = refs.institution(userId, institutionId);
            if (institution == null
                    || !institutionId.equals(institution.getString("institutionId"))) {
                throw ApiException.forField(
                        "1001",
                        "VALIDATION_FAILED",
                        "Institution is unavailable to this user",
                        "institutionId");
            }
            quote.put(
                    "institutionName",
                    RequestValidator.requireString(institution, "institutionName"));
        }
        if (contactId != null) {
            JSONObject contact = refs.contact(userId, contactId);
            if (contact == null || !contactId.equals(contact.getString("contactId"))) {
                throw ApiException.forField(
                        "1001",
                        "VALIDATION_FAILED",
                        "Contact is unavailable to this user",
                        "contactId");
            }
            if (!ficc.common.client.CrmClient.belongsTo(contact, institutionId)) {
                throw ApiException.forField(
                        "1001", "VALIDATION_FAILED", "Contact/institution mismatch", "contactId");
            }
            quote.put("contactName", RequestValidator.requireString(contact, "contactName"));
        }
        if (active) {
            if (quote.getString("institutionName") != null && institutionId == null) {
                throw ApiException.forField(
                        "1001",
                        "UNRESOLVED_COUNTERPARTY",
                        "Select an institution ID before confirmation",
                        "institutionId");
            }
            if (quote.getString("contactName") != null && contactId == null) {
                throw ApiException.forField(
                        "1001",
                        "UNRESOLVED_COUNTERPARTY",
                        "Select a contact ID before confirmation",
                        "contactId");
            }
        }
    }

    void committed(JSONObject event) {
        lastSequence = event.getLongValue("sequence");
        publish.accept(
                "ficc.quote.events",
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
                        Long.toString(lastSequence),
                        "toSequence",
                        Long.toString(lastSequence),
                        "events",
                        JsonUtils.array(event)));
    }

    List<JSONObject> selectQuotes(String user, JSONObject request) {
        long started = QuotePerformanceTrace.searchStart();
        try {
            return search.queryQuotes(user, request);
        } finally {
            QuotePerformanceTrace.searchFinished(started);
        }
    }

    boolean matches(String user, JSONObject q, JSONObject f) {
        return matcher.matches(user, q, f);
    }

    JSONArray referenceFor(List<JSONObject> qs) {
        JSONArray a = new JSONArray();
        Set<String> seen = new HashSet<>();
        for (JSONObject q : qs)
            if (q.getString("securityId") != null && seen.add(q.getString("securityId")))
                a.add(refs.require(q.getString("securityId")));
        return a;
    }

    int limit(JSONObject r, int max) {
        int n = r.getIntValue("limit", 200);
        if (n < 1 || n > max)
            throw ApiException.forField("1008", "RESOURCE_LIMIT", "limit out of range", "limit");
        return n;
    }

    Object page(Call c, String field, List<JSONObject> rows, JSONObject extra) {
        if (rows.size() > 10000)
            throw ApiException.forField(
                    "1008", "RESOURCE_LIMIT", "Client result cap: narrow the filter", "filter");
        int n = limit(c.request, 1000);
        return pageResult(
                new Cursor(
                        c.user,
                        c.session,
                        c.topic,
                        field,
                        rows.stream().map(JsonUtils::deepCopy).collect(Collectors.toList()),
                        0,
                        n,
                        System.currentTimeMillis() + 120000,
                        extra));
    }

    Object pageResult(Cursor cur) {
        int end = Math.min(cur.offset + cur.limit, cur.rows.size());
        JSONObject d = JsonUtils.deepCopy(cur.extra);
        d.put(cur.field, cur.rows.subList(cur.offset, end));
        d.put("totalCount", cur.rows.size());
        String next = null;
        if (end < cur.rows.size()) {
            cursors.entrySet().removeIf(e -> e.getValue().expires < System.currentTimeMillis());
            if (cursors.size() >= 64)
                throw ApiException.forField("1008", "RESOURCE_LIMIT", "Too many cursors", null);
            next = IdGenerator.newId("CUR-");
            cursors.put(
                    next,
                    new Cursor(
                            cur.user,
                            cur.session,
                            cur.topic,
                            cur.field,
                            cur.rows,
                            end,
                            cur.limit,
                            cur.expires,
                            cur.extra));
        }
        d.put("nextCursor", next);
        return d;
    }

    Object continuePage(Call c) {
        RequestValidator.allowOnlyFields(c.request, "requestId", "cursor");
        Cursor cur = cursors.get(c.request.getString("cursor"));
        if (cur == null
                || cur.expires < System.currentTimeMillis()
                || !cur.user.equals(c.user)
                || !cur.session.equals(c.session)
                || !cur.topic.equals(c.topic))
            throw ApiException.forField(
                    "1007",
                    "RESET_REQUIRED",
                    "Cursor expired or belongs to a different query",
                    "cursor");
        return pageResult(cur);
    }

    Object history(Call c) throws Exception {
        String id = RequestValidator.requireString(c.request, "quoteId");
        JSONObject q = get(id, c.user);
        if (!owns(c.user, q))
            throw ApiException.forField("1003", "FORBIDDEN", "History requires ownership", null);
        return page(
                c,
                "records",
                store.rows(
                        "SELECT payload FROM history WHERE quote_id=? ORDER BY revision DESC", id),
                JsonUtils.object("quoteId", id));
    }

    Object historySearch(Call c) throws Exception {
        return page(c, "rows", search.searchHistory(c.user, c.request), JsonUtils.object());
    }

    Object sourceQuery(Call c) throws Exception {
        String id = c.request.getString("sourceMessageId");
        List<JSONObject> a =
                id == null
                        ? store.rows("SELECT payload FROM sources")
                        : store.rows("SELECT payload FROM sources WHERE id=?", id);
        return page(
                c,
                "sourceMessages",
                a.stream()
                        .filter(
                                x ->
                                        access.administrator(c.user)
                                                || access.internal(c.user)
                                                || c.user.equals(x.getString("ownerUserId")))
                        .collect(Collectors.toList()),
                JsonUtils.object());
    }

    String settingId(JSONObject g) {
        return g.getString(g.containsKey("watchlistId") ? "watchlistId" : "filterGroupId");
    }

    Object groupQuery(Call c, String kind) {
        String key = kind + "Id";
        return JsonUtils.object(
                kind.equals("watchlist") ? "watchlists" : "filterGroups",
                settings.values().stream()
                        .filter(g -> g.containsKey(key) && c.user.equals(g.getString("userId")))
                        .filter(
                                g ->
                                        c.request.getString(key) == null
                                                || c.request
                                                        .getString(key)
                                                        .equals(g.getString(key)))
                        .filter(
                                g ->
                                        c.request.getString("market") == null
                                                || c.request
                                                        .getString("market")
                                                        .equals(g.getString("market")))
                        .filter(
                                g ->
                                        c.request.getString("scope") == null
                                                || c.request
                                                        .getString("scope")
                                                        .equals(g.getString("scope")))
                        .collect(Collectors.toList()));
    }

    Object saveGroup(Call c, String kind, boolean delete) throws Exception {
        String rid = RequestValidator.requireString(c.request, "requestId"), key = kind + "Id";
        JSONArray receipt = store.receipt(c.user, c.topic, rid, JsonUtils.canonicalHash(c.request));
        if (!receipt.isEmpty()) return receipt.getJSONObject(0);
        String id = c.request.getString(key);
        if (delete) RequestValidator.requireString(c.request, key);
        JSONObject prev = id == null ? null : settings.get(id);
        if (id != null && (prev == null || !c.user.equals(prev.getString("userId"))))
            throw ApiException.forField("1002", "NOT_FOUND", "Group not found", key);
        if (prev != null
                && !RequestValidator.requireString(c.request, "expectedRevision")
                        .equals(prev.getString("revision")))
            throw ApiException.forField(
                    "1004", "REVISION_CONFLICT", "Group changed", "expectedRevision");
        JSONObject g = JsonUtils.deepCopy(c.request);
        g.remove("requestId");
        g.remove("expectedRevision");
        if (id == null) id = IdGenerator.newId(kind.equals("watchlist") ? "W-" : "FG-");
        g.put(key, id);
        g.put("userId", c.user);
        g.put("revision", Long.toString(prev == null ? 1 : prev.getLongValue("revision") + 1));
        g.put(
                "createTime",
                prev == null ? BusinessClock.currentTime() : prev.getString("createTime"));
        g.put("updateTime", BusinessClock.currentTime());
        if (!delete) {
            RequestValidator.requireString(g, "name");
            if (!Arrays.asList("RATE", "CREDIT")
                            .contains(RequestValidator.requireString(g, "market"))
                    || !Arrays.asList("MARKET", "HISTORY")
                            .contains(RequestValidator.requireString(g, "scope")))
                throw ApiException.forField(
                        "1001", "VALIDATION_FAILED", "Invalid group scope", null);
            if (kind.equals("watchlist")) {
                JSONArray a = g.getJSONArray("securities");
                if (a == null)
                    throw ApiException.forField(
                            "1001", "VALIDATION_FAILED", "securities required", "securities");
                Set<String> seen = new HashSet<>();
                for (JSONObject x : a.toJavaList(JSONObject.class)) {
                    refs.require(RequestValidator.requireString(x, "securityId"));
                    if (!seen.add(x.getString("securityId")))
                        throw ApiException.forField(
                                "1001", "VALIDATION_FAILED", "Duplicate bond", "securityId");
                    x.putIfAbsent("displayOrder", seen.size() - 1);
                }
            } else {
                JSONArray subs = g.getJSONArray("subgroups");
                if (subs == null)
                    throw ApiException.forField(
                            "1001", "VALIDATION_FAILED", "subgroups required", "subgroups");
                JSONArray clean = new JSONArray();
                for (JSONObject sg : subs.toJavaList(JSONObject.class)) {
                    JSONArray cs = sg.getJSONArray("conditions");
                    if (cs == null || cs.isEmpty()) continue;
                    FilterEngine.validate(JsonUtils.object("conditions", cs));
                    sg.putIfAbsent("subgroupId", IdGenerator.newId("SG-"));
                    int i = 0;
                    for (JSONObject cond : cs.toJavaList(JSONObject.class)) {
                        cond.putIfAbsent("conditionId", IdGenerator.newId("C-"));
                        cond.putIfAbsent("displayOrder", i++);
                    }
                    clean.add(sg);
                }
                g.put("subgroups", clean);
            }
        }
        JSONObject
                out =
                        delete
                                ? JsonUtils.object(key, id, "deleted", true)
                                : JsonUtils.object(kind, g),
                event;
        try {
            store.begin();
            if (delete) store.update("DELETE FROM settings WHERE id=?", id);
            else
                store.update(
                        "INSERT INTO settings VALUES (?,?,?,?,?::jsonb) ON CONFLICT(id) DO UPDATE"
                                + " SET revision=excluded.revision,payload=excluded.payload",
                        id,
                        c.user,
                        kind,
                        g.getLongValue("revision"),
                        JsonUtils.toJson(g));
            event =
                    store.event(
                            JsonUtils.object(
                                    "type",
                                    kind.equals("watchlist") ? "WATCHLIST" : "FILTER_GROUP",
                                    "action",
                                    delete ? "REMOVE" : "UPSERT",
                                    "id",
                                    id,
                                    "record",
                                    delete ? null : g,
                                    "userId",
                                    c.user));
            store.receipt(c.user, c.topic, rid, JsonUtils.array(out));
            store.commit();
        } catch (Exception e) {
            store.rollback();
            throw e;
        }
        if (delete) settings.remove(id);
        else settings.put(id, g);
        committed(event);
        views.settingsChanged(c.user, kind, id, g.getString("revision"), delete);
        return out;
    }

    synchronized JSONObject handleInternalRead(String topic, JSONObject request) {
        String requestId = request.getString("requestId");
        try {
            RequestValidator.requireString(request, "requestId");
            Object data;
            if (QuoteSvrClient.INTERNAL_STATE_QUERY.equals(topic)) {
                RequestValidator.allowOnlyFields(request, "requestId", "limit", "cursor");
                // Cursor scope is internal transport context, never a user or audit identity.
                Call call = new Call("<internal-read>", "<internal-read>", topic, request);
                data = stateRecords(call);
            } else if (QuoteSvrClient.INTERNAL_PRICE_QUERY.equals(topic)) {
                RequestValidator.allowOnlyFields(request, "requestId");
                List<String> securities =
                        quotes.values().stream()
                                .map(quote -> quote.getString("securityId"))
                                .filter(Objects::nonNull)
                                .distinct()
                                .collect(Collectors.toList());
                data = views.feed(true, securities);
            } else {
                throw ApiException.forField(
                        "1003",
                        "FORBIDDEN",
                        "Only internal synchronization reads are allowed",
                        null);
            }
            return ApiResponse.success(requestId, data);
        } catch (ApiException error) {
            return ApiResponse.failure(requestId, error);
        } catch (Exception error) {
            System.err.println(
                    "Internal synchronization failed: "
                            + topic
                            + " ("
                            + error.getClass().getSimpleName()
                            + ")");
            return ApiResponse.failure(
                    requestId,
                    ApiException.forField(
                            "1099", "INTERNAL_ERROR", "Internal synchronization failed", null));
        }
    }

    Object state(Call call) {
        internal(call);
        return stateRecords(call);
    }

    private Object stateRecords(Call call) {
        if (call.request.getString("cursor") != null) {
            JSONObject data = (JSONObject) continuePage(call);
            finishState(data);
            return data;
        }
        List<JSONObject> entries = new ArrayList<>();
        quotes.values()
                .forEach(
                        quote ->
                                entries.add(
                                        JsonUtils.object("collection", "quotes", "record", quote)));
        settings.values()
                .forEach(
                        setting ->
                                entries.add(
                                        JsonUtils.object(
                                                "collection",
                                                setting.containsKey("watchlistId")
                                                        ? "watchlists"
                                                        : "filterGroups",
                                                "record",
                                                setting)));
        JSONObject position =
                JsonUtils.object("epoch", store.epoch, "sequence", Long.toString(lastSequence));
        Cursor cursor =
                new Cursor(
                        call.user,
                        call.session,
                        call.topic,
                        "entries",
                        entries.stream().map(JsonUtils::deepCopy).collect(Collectors.toList()),
                        0,
                        limit(call.request, 1000),
                        System.currentTimeMillis() + 120000,
                        position);
        JSONObject data = (JSONObject) pageResult(cursor);
        finishState(data);
        return data;
    }

    void finishState(JSONObject d) {
        JSONObject records =
                JsonUtils.object(
                        "quotes",
                        JsonUtils.array(),
                        "watchlists",
                        JsonUtils.array(),
                        "filterGroups",
                        JsonUtils.array());
        for (Object x : (List<?>) d.remove("entries")) {
            JSONObject e = (JSONObject) x;
            records.getJSONArray(e.getString("collection")).add(e.get("record"));
        }
        d.put("records", records);
        d.put("done", d.get("nextCursor") == null);
    }

    Object events(Call c) throws Exception {
        internal(c);
        String epoch = RequestValidator.requireString(c.request, "epoch");
        long after = Long.parseLong(RequestValidator.requireString(c.request, "afterSequence"));
        if (!epoch.equals(store.epoch) || after < 0 || after > lastSequence)
            throw ApiException.forField("1007", "RESET_REQUIRED", "Event position invalid", null);
        int n = limit(c.request, 1000);
        List<JSONObject> es =
                store.rows(
                        "SELECT payload FROM events WHERE sequence>? ORDER BY sequence LIMIT ?",
                        after,
                        n);
        long next = es.isEmpty() ? after : es.get(es.size() - 1).getLongValue("sequence");
        return JsonUtils.object(
                "epoch",
                epoch,
                "events",
                es,
                "nextSequence",
                Long.toString(next),
                "seenSequence",
                Long.toString(lastSequence),
                "hasMore",
                next < lastSequence);
    }

    void internal(Call c) {
        if (!access.internal(c.user))
            throw ApiException.forField(
                    "1003", "FORBIDDEN", "Internal service identity required", null);
    }

    Object reference(Call call, boolean query) {
        String kind = RequestValidator.requireString(call.request, "kind");
        String key = idField(kind);
        if (query) {
            String text = JsonUtils.getStringOrDefault(call.request, "text", "");
            if ("INSTITUTION".equals(kind) || "CONTACT".equals(kind)) {
                int offset = call.request.getIntValue("offset", 0);
                if (offset < 0 || offset > 1000000) {
                    throw ApiException.forField(
                            "1001", "VALIDATION_FAILED", "Invalid CRM offset", "offset");
                }
                return refs.queryCrm(
                        call.user,
                        kind,
                        text,
                        call.request.getString("institutionId"),
                        offset,
                        limit(call.request, 200));
            }
            List<JSONObject> records =
                    "SECURITY".equals(kind)
                            ? refs.querySecurities(text)
                            : refs.records(kind, call.user).stream()
                                    .filter(value -> JsonUtils.toJson(value).contains(text))
                                    .collect(Collectors.toList());
            return page(call, "records", records, JsonUtils.object());
        }
        JSONArray ids = call.request.getJSONArray("ids");
        if (ids == null) {
            throw ApiException.forField("1001", "VALIDATION_FAILED", "ids required", "ids");
        }
        List<JSONObject> records;
        if ("SECURITY".equals(kind) || "DAILY_VALUATION".equals(kind)) {
            List<JSONObject> securities =
                    ids.stream()
                            .map(Object::toString)
                            .distinct()
                            .map(refs::security)
                            .filter(Objects::nonNull)
                            .collect(Collectors.toList());
            records =
                    "SECURITY".equals(kind)
                            ? securities
                            : refs.records(kind, call.user).stream()
                                    .filter(value -> ids.contains(value.getString(key)))
                                    .collect(Collectors.toList());
        } else if ("INSTITUTION".equals(kind) || "CONTACT".equals(kind)) {
            if (ids.size() > 200) {
                throw ApiException.forField(
                        "1008", "RESOURCE_LIMIT", "At most 200 CRM IDs per request", "ids");
            }
            records =
                    ids.stream()
                            .map(Object::toString)
                            .distinct()
                            .map(
                                    id ->
                                            "INSTITUTION".equals(kind)
                                                    ? refs.institution(call.user, id)
                                                    : refs.contact(call.user, id))
                            .filter(Objects::nonNull)
                            .collect(Collectors.toList());
            return JsonUtils.object(
                    "records",
                    records,
                    "missingIds",
                    ids.stream()
                            .filter(
                                    id ->
                                            records.stream()
                                                    .noneMatch(
                                                            value ->
                                                                    id.equals(
                                                                            value.getString(key))))
                            .collect(Collectors.toList()));
        } else {
            records =
                    refs.records(kind, call.user).stream()
                            .filter(value -> ids.contains(value.getString(key)))
                            .collect(Collectors.toList());
        }
        return JsonUtils.object(
                "records",
                records,
                "missingIds",
                ids.stream()
                        .filter(
                                id ->
                                        records.stream()
                                                .noneMatch(
                                                        value -> id.equals(value.getString(key))))
                        .collect(Collectors.toList()),
                "referenceVersion",
                refs.version());
    }

    static boolean allows(String op, String status) {
        if ("confirm".equals(op)) return "PENDING_CONFIRM".equals(status);
        if ("update".equals(op)) return "PENDING_CONFIRM".equals(status) || "ACTIVE".equals(status);
        if ("refer".equals(op)) return "ACTIVE".equals(status);
        if ("unrefer".equals(op)) return "REFERRED".equals(status);
        if ("delete".equals(op))
            return "REFERRED".equals(status) || "PENDING_CONFIRM".equals(status);
        return false;
    }

    static String idField(String kind) {
        if ("SECURITY".equals(kind) || "DAILY_VALUATION".equals(kind)) return "securityId";
        if ("ISSUER".equals(kind)) return "issuerId";
        if ("CONTACT".equals(kind)) return "contactId";
        return "institutionId";
    }

    static Map<String, Set<String>> quoteDictionaries() {
        Map<String, Set<String>> dictionaries = new LinkedHashMap<>();
        dictionaries.put("side", new LinkedHashSet<>(Arrays.asList("1", "2")));
        dictionaries.put("quoteType", new LinkedHashSet<>(Arrays.asList("1", "2", "3")));
        dictionaries.put("yieldType", new LinkedHashSet<>(Arrays.asList("1", "2")));
        dictionaries.put("settlType", ficc.common.SettlementTypes.SUPPORTED);
        return dictionaries;
    }

    Object dictionary(boolean price, JSONObject request) {
        JSONArray fields = refs.filterCatalog();
        JSONObject dictionaries = FilterDictionaries.dictionaries();
        for (Object entry : fields) {
            JSONObject field = (JSONObject) entry;
            if (field.containsKey("enumValues")) {
                dictionaries.put(field.getString("field"), field.get("enumValues"));
            }
        }
        dictionaries.put(
                "market",
                JsonUtils.array(
                        JsonUtils.object("value", "RATE", "label", "利率"),
                        JsonUtils.object("value", "CREDIT", "label", "信用")));
        if (price) {
            dictionaries.put(
                    "mode",
                    JsonUtils.array(
                            JsonUtils.object("value", "BEST", "label", "最优"),
                            JsonUtils.object("value", "FULL", "label", "全部"),
                            JsonUtils.object("value", "DEPTH", "label", "档数")));
        }
        JSONArray names = request.getJSONArray("names");
        if (names != null) {
            dictionaries.keySet().retainAll(new HashSet<>(names.toJavaList(String.class)));
        }
        return JsonUtils.object("dictionaries", dictionaries, "filterFields", fields);
    }

    Object intake(Call c) throws Exception {
        return Intake.apply(this, c);
    }
}
