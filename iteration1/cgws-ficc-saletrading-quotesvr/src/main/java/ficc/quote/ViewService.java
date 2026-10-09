package ficc.quote;

import com.alibaba.fastjson2.*;

import ficc.common.*;
import ficc.api.QuoteRecord;
import ficc.api.ProtocolCodec;
import ficc.api.QuoteQueryRequest;
import java.util.function.Predicate;

import java.time.*;
import java.util.*;
import java.util.stream.Collectors;

/** Separate QuoteView and PriceView state; shared list-position utility only. */
final class ViewService {
    final QuoteService svc;
    final Map<String, Stream> streams = new LinkedHashMap<>();
    final String marketStream = IdGenerator.newId("MDE-");
    long marketSequence = 0;

    static class Stream {
        String user;
        String session;
        String domain;
        String route;
        String id;
        long sequence = 0;
        final Map<String, View> views = new LinkedHashMap<>();
        final Set<String> used = new HashSet<>();
        boolean subscribed;
    }

    abstract static class View {
        String id;
        JSONObject request;
        final ViewPage page = new ViewPage();
        List<String> ids = new ArrayList<>();
        Map<String, JSONObject> loaded = new LinkedHashMap<>();
        String referenceVersion;
        boolean rebuildRequired;
    }

    static final class QuoteView extends View {
        List<QuoteRecord> selectedQuotes = Collections.emptyList();
    }

    static final class PriceView extends View {}

    ViewService(QuoteService svc) {
        this.svc = svc;
    }

    Stream find(String session, String domain) {
        Stream s = streams.get(session + ":" + domain);
        if (s == null)
            throw ApiException.forField("1009", "NOT_READY", "Call stream.open first", null);
        return s;
    }

    Object openStream(QuoteService.Call c, String domain) {
        RequestValidator.allowOnlyFields(c.request(), "requestId");
        Stream s =
                streams.computeIfAbsent(
                        c.session() + ":" + domain,
                        k -> {
                            Stream x = new Stream();
                            x.user = c.user();
                            x.session = c.session();
                            x.domain = domain;
                            x.route = c.session();
                            x.id = IdGenerator.newId(domain.equals("quote") ? "QE-" : "PE-");
                            return x;
                        });
        return JsonUtils.object("sessionId", s.route, "streamId", s.id, "pushTopic", topic(s));
    }

    String topic(Stream s) {
        return "ficc." + s.domain + ".report." + s.route;
    }

    void authorize(String session, String user, String topic) {
        if (topic.equals("ficc.quote.events") || topic.equals("ficc.price.snapshots")) {
            if (!svc.access.internal(user))
                throw ApiException.forField("1003", "FORBIDDEN", "Internal channel", null);
            return;
        }
        if (topic.startsWith("ficc.price.book.")) {
            if (!svc.access.internal(user))
                throw ApiException.forField(
                        "1003", "FORBIDDEN", "Raw STC channel is internal", null);
            return;
        }
        for (Stream s : streams.values())
            if (topic(s).equals(topic) && s.session.equals(session) && s.user.equals(user)) {
                s.subscribed = true;
                return;
            }
        throw ApiException.forField(
                "1003", "FORBIDDEN", "Private topic not owned by connection", null);
    }

    void unsubscribe(String sid, String topic) {
        for (Stream s : streams.values())
            if (s.session.equals(sid) && topic(s).equals(topic)) s.subscribed = false;
    }

    Map<String, Object> image(String topic, String sid) {
        if (topic.startsWith("ficc.price.report.")) {
            Stream s = find(sid, "price");
            return single(topic, snapshot(s));
        }
        if (topic.equals("ficc.price.snapshots"))
            return single(topic, feed(true, new ArrayList<>(svc.refs.securities().keySet())));
        if (topic.startsWith("ficc.price.book.")) {
            Map<String, Object> m = new LinkedHashMap<>();
            String id = topic.substring("ficc.price.book.".length());
            for (String sec : svc.refs.securities().keySet())
                if (id.equals("**") || id.equals(sec))
                    m.put(
                            "ficc.price.book." + sec,
                            book(null, sec, JsonUtils.object(), "FULL", 0).get("marketData"));
            return m;
        }
        return Collections.emptyMap(); // Quote ordinary subscription never manufactures an initial
        // collection.
    }

    private static Map<String, Object> single(String topic, Object value) {
        Map<String, Object> m = new LinkedHashMap<>();
        m.put(topic, value);
        return m;
    }

    Object openView(QuoteService.Call call, String domain) {
        JSONObject request = call.request();
        if (domain.equals("quote")) {
            RequestValidator.allowOnlyFields(
                    request, "requestId", "viewId", "filter", "pageSize", "status", "ownerScope");
        } else {
            RequestValidator.allowOnlyFields(
                    request, "requestId", "viewId", "filter", "pageSize", "mode", "depth");
        }
        Stream stream = find(call.session(), domain);
        if (!stream.subscribed) {
            throw ApiException.forField(
                    "1009", "NOT_READY", "Subscribe to this domain's channel first", null);
        }
        String id = RequestValidator.requireString(request, "viewId");
        if (stream.used.contains(id)) {
            throw ApiException.forField(
                    "1001", "VALIDATION_FAILED", "viewId cannot be reused", "viewId");
        }
        if (stream.views.size() >= 20) {
            throw ApiException.forField("1008", "RESOURCE_LIMIT", "View limit reached", null);
        }
        View view = domain.equals("quote") ? new QuoteView() : new PriceView();
        view.id = id;
        view.request = JsonUtils.deepCopy(request);
        view.page.configure(request, true);
        FilterEngine.validate(request.getJSONObject("filter"));
        if (domain.equals("price")) mode(request);
        refresh(stream, view);
        stream.used.add(id);
        stream.views.put(id, view);
        return replySnapshot(stream, view);
    }

    Object queryPage(QuoteService.Call call, String domain) {
        RequestValidator.allowOnlyFields(
                call.request(), "requestId", "viewId", "pageNumber", "pageSize");
        Stream stream = find(call.session(), domain);
        View view = stream.views.get(RequestValidator.requireString(call.request(), "viewId"));
        if (view == null) {
            throw ApiException.forField("1002", "NOT_FOUND", "View not found", null);
        }
        view.page.configure(call.request(), false);
        refresh(stream, view);
        return replySnapshot(stream, view);
    }

    private Object replySnapshot(Stream stream, View view) {
        if (!(view instanceof QuoteView)) {
            return result(stream, view);
        }
        JSONObject metadata = JsonUtils.object("streamId", stream.id,
                "sequence", Long.toString(stream.sequence), "view", viewRecord(stream, view));
        // Stored records are replaced, never edited, after commit. Capture references and
        // metadata under the service lock, then convert only this page outside that lock.
        return new QuotePageSnapshot(metadata, ((QuoteView) view).selectedQuotes, stream.user, svc.access);
    }

    Object close(QuoteService.Call c, String domain) {
        RequestValidator.allowOnlyFields(c.request(), "requestId", "viewId");
        Stream s = find(c.session(), domain);
        String id = RequestValidator.requireString(c.request(), "viewId");
        s.views.remove(id);
        return JsonUtils.object("viewId", id, "closed", true);
    }

    JSONObject result(Stream s, View v) {
        JSONObject out =
                JsonUtils.object(
                        "streamId",
                        s.id,
                        "sequence",
                        Long.toString(s.sequence),
                        "view",
                        viewRecord(s, v));
        if (v instanceof QuoteView) out.put("quotes", new ArrayList<>(v.loaded.values()));
        else out.put("books", books(v));
        return out;
    }

    JSONObject viewRecord(Stream stream, View view) {
        return JsonUtils.object(
                "viewId",
                view.id,
                "pageNumber",
                view.page.pageNumber,
                "pageSize",
                view.page.pageSize,
                "totalCount",
                total(stream, view),
                "ids",
                new ArrayList<>(view.ids));
    }

    int total(Stream s, View v) {
        return v.page.totalCount();
    }

    List<JSONObject> matchingRows(Stream stream, View view) {
        if (view instanceof QuoteView) {
            return svc.selectQuotes(stream.user, view.request).stream()
                    .map(quote -> svc.visible(quote, stream.user).toJson())
                    .collect(Collectors.toList());
        }
        return priceIds(stream.user, view.request).stream()
                .map(
                        id ->
                                book(
                                        stream.user,
                                        id,
                                        view.request.getJSONObject("filter"),
                                        mode(view.request),
                                        view.request.getIntValue("depth")))
                .collect(Collectors.toList());
    }

    void refresh(Stream stream, View view) {
        String referenceVersion = svc.refs.version();
        if (view instanceof QuoteView) {
            List<QuoteRecord> matches = svc.selectQuotes(stream.user, view.request);
            view.page.initializeVersions(quoteVersions(matches));
            long offset = ((long) view.page.pageNumber - 1) * view.page.pageSize;
            int start = (int) Math.min(offset, matches.size());
            int end = (int) Math.min(offset + view.page.pageSize, matches.size());
            QuoteView quoteView = (QuoteView) view;
            quoteView.selectedQuotes = new ArrayList<>(matches.subList(start, end));
            view.ids = quoteView.selectedQuotes.stream().map(quote -> quote.quoteId)
                    .collect(Collectors.toList());
            view.loaded.clear();
            view.referenceVersion = referenceVersion;
            view.rebuildRequired = false;
            return;
        }
        List<JSONObject> rows = matchingRows(stream, view);
        String idField = view instanceof QuoteView ? "quoteId" : "securityId";
        view.page.initialize(rows, idField);
        view.loaded = new LinkedHashMap<>();
        for (JSONObject row : view.page.rows(rows)) {
            view.loaded.put(row.getString(idField), row);
        }
        view.ids = new ArrayList<>(view.loaded.keySet());
        view.referenceVersion = referenceVersion;
        view.rebuildRequired = false;
    }

    private Map<String, String> quoteVersions(List<QuoteRecord> quotes) {
        Map<String, String> versions = new LinkedHashMap<>();
        for (QuoteRecord quote : quotes) {
            versions.put(quote.quoteId, quote.revision);
        }
        return versions;
    }

    List<JSONObject> books(View v) {
        return v.loaded.values().stream()
                .map(
                        p -> {
                            JSONObject b = JsonUtils.deepCopy(p);
                            b.put("viewId", v.id);
                            return b;
                        })
                .collect(Collectors.toList());
    }

    JSONObject snapshot(Stream s) {
        JSONArray views = new JSONArray(), books = new JSONArray();
        for (View v : s.views.values()) {
            refresh(s, v);
            views.add(viewRecord(s, v));
            books.addAll(books(v));
        }
        return JsonUtils.object(
                "Code",
                "0000",
                "Msg",
                "成功",
                "kind",
                "SNAPSHOT",
                "streamId",
                s.id,
                "sequence",
                Long.toString(s.sequence),
                "books",
                books,
                "views",
                views);
    }

    static String mode(JSONObject request) {
        return PriceBookService.wireMode(request);
    }

    List<String> priceIds(String user, JSONObject r) {
        JSONObject f = r.getJSONObject("filter");
        FilterEngine.validate(f);
        Map<String, String> latest = new HashMap<>();
        Predicate<QuoteRecord> matches = svc.matchingQuotes(user, f);
        for (QuoteRecord q : svc.quotes.values())
            if (q.status.equals("ACTIVE") && matches.test(q))
                latest.merge(
                        q.securityId,
                        q.updateTime,
                        (a, b) -> a.compareTo(b) >= 0 ? a : b);
        return latest.keySet().stream()
                .sorted(
                        Comparator.<String, String>comparing(latest::get)
                                .reversed()
                                .thenComparing(x -> x))
                .collect(Collectors.toList());
    }

    JSONObject book(String user, String security, JSONObject filter, String mode, int depth) {
        return svc.priceBooks.book(user, security,
                filter == null ? null : ProtocolCodec.decode(filter, ficc.api.Filter.class), mode, depth).toJson();
    }

    JSONObject feed(boolean initial, List<String> ids) {
        return JsonUtils.object(
                "Code",
                "0000",
                "Msg",
                "成功",
                "kind",
                initial ? "SNAPSHOT" : "UPDATE",
                "streamId",
                marketStream,
                "sequence",
                Long.toString(marketSequence),
                "quotePosition",
                JsonUtils.object(
                        "epoch", svc.store.epoch, "sequence", Long.toString(svc.lastSequence)),
                "prices",
                ids.stream()
                        .map(id -> book(null, id, JsonUtils.object(), "FULL", 0))
                        .collect(Collectors.toList()));
    }

    void changed(QuoteRecord q, JSONObject event, String previousSecurity) {
        QuoteRecord previous = svc.priceBooks.previousQuote(q, previousSecurity);
        Set<String> securities = svc.priceBooks.changed(q, previousSecurity);
        boolean affectsPrice = !securities.isEmpty();
        marketSequence++;
        for (Stream s : streams.values())
            if (s.subscribed && (s.domain.equals("quote") || (affectsPrice
                    && (svc.canView(s.user, q) || (previous != null && svc.canView(s.user, previous)))))) {
                try {
                    publishChanges(s, q, event, null, previousSecurity);
                } catch (ApiException ex) {
                    for (View view : s.views.values()) {
                        view.rebuildRequired = true;
                    }
                    svc.publish.accept(
                            topic(s),
                            JsonUtils.object(
                                    "Code",
                                    ex.code,
                                    "Msg",
                                    ex.getMessage(),
                                    "kind",
                                    "RESET_REQUIRED",
                                    "streamId",
                                    s.id,
                                    "sequence",
                                    Long.toString(++s.sequence)));
                }
            }
        try {
            svc.publish.accept("ficc.price.snapshots", feed(false, new ArrayList<>(securities)));
            for (String id : securities)
                svc.publish.accept(
                        "ficc.price.book." + id,
                        book(null, id, JsonUtils.object(), "FULL", 0).getJSONObject("marketData"));
        } catch (ApiException e) {
            svc.publish.accept(
                    "ficc.price.snapshots",
                    JsonUtils.object(
                            "Code",
                            "1009",
                            "Msg",
                            e.getMessage(),
                            "kind",
                            "RESET_REQUIRED",
                            "streamId",
                            marketStream,
                            "sequence",
                            Long.toString(marketSequence)));
        }
    }

    void publishChanges(Stream stream, QuoteRecord changed, JSONObject event, JSONObject setting,
            String previousSecurity) {
        JSONArray deltas = new JSONArray();
        JSONArray bookUpdates = new JSONArray();
        JSONArray reports = new JSONArray();
        Map<String, JSONObject> quoteUpdates = new LinkedHashMap<>();
        for (View view : stream.views.values()) {
            if (setting != null && !usesSetting(view, setting)) {
                continue;
            }
            Map<String, JSONObject> changedRows = new LinkedHashMap<>();
            JSONObject delta = updateView(stream, view, changed, previousSecurity, setting, changedRows);
            // Matching fingerprints are retained; displayed positions are not.
            view.loaded.clear();
            view.ids.clear();
            if (view instanceof QuoteView) {
                ((QuoteView) view).selectedQuotes = Collections.emptyList();
            }
            if (delta.getJSONArray("upsertIds").isEmpty()
                    && delta.getJSONArray("removeIds").isEmpty()
                    && setting == null) {
                continue;
            }
            deltas.add(delta);
            if (view instanceof QuoteView) {
                quoteUpdates.putAll(changedRows);
            } else {
                for (JSONObject row : changedRows.values()) {
                    JSONObject book = JsonUtils.deepCopy(row);
                    book.put("viewId", view.id);
                    bookUpdates.add(book);
                }
            }
        }
        if (stream.domain.equals("quote") && changed != null && svc.owns(stream.user, changed)) {
            quoteUpdates.put(changed.quoteId, svc.visible(changed, stream.user).toJson());
            reports.add(
                    JsonUtils.object(
                            "eventId",
                            event.getString("eventId"),
                            "requestId",
                            event.getString("requestId"),
                            "clientQuoteId",
                            changed.clientQuoteId,
                            "quoteId",
                            changed.quoteId,
                            "revision",
                            changed.revision,
                            "action",
                            event.getString("action"),
                            "operatorUserId",
                            event.getString("operatorUserId"),
                            "operationTime",
                            event.getString("operationTime")));
        }
        if (deltas.isEmpty() && reports.isEmpty() && setting == null) return;
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
                        "views",
                        deltas);
        if (stream.domain.equals("quote")) {
            message.put("quotes", new ArrayList<>(quoteUpdates.values()));
            message.put("reports", reports);
        } else {
            message.put("books", bookUpdates);
        }
        if (setting != null) message.put("settingsChanged", JsonUtils.array(setting));
        svc.publish.accept(topic(stream), message);
    }

    private boolean usesSetting(View view, JSONObject setting) {
        JSONObject filter = view.request.getJSONObject("filter");
        if (filter == null) {
            return false;
        }
        String field = "WATCHLIST".equals(setting.getString("type")) ? "watchlistIds" : "filterGroupIds";
        JSONArray ids = filter.getJSONArray(field);
        return ids != null && ids.contains(setting.getString("id"));
    }

    private JSONObject updateView(Stream stream, View view, QuoteRecord changed,
            String previousSecurity, JSONObject setting, Map<String, JSONObject> changedRows) {
        String referenceVersion = svc.refs.version();
        boolean rebuild = setting != null || changed == null || view.rebuildRequired
                || !Objects.equals(view.referenceVersion, referenceVersion);
        // A failed calculation must never leave a partially advanced view treated as current.
        view.rebuildRequired = true;
        JSONObject delta;
        if (view instanceof QuoteView) {
            delta = updateQuoteVersions(stream, view, changed, rebuild, changedRows);
        } else if (rebuild) {
            delta = view.page.changes(view.id, matchingRows(stream, view),
                    "securityId", changedRows);
        } else {
            Map<String, JSONObject> upserts = new LinkedHashMap<>();
            Set<String> removals = new LinkedHashSet<>();
            Set<String> affectedSecurities = new LinkedHashSet<>();
            if (previousSecurity != null) {
                affectedSecurities.add(previousSecurity);
            }
            if (changed.securityId != null) {
                affectedSecurities.add(changed.securityId);
            }
            for (String securityId : affectedSecurities) {
                JSONObject book = book(stream.user, securityId,
                        view.request.getJSONObject("filter"), mode(view.request),
                        view.request.getIntValue("depth"));
                if (book.getJSONObject("marketData").getJSONArray("NoMDEntries").isEmpty()) {
                    removals.add(securityId);
                } else {
                    upserts.put(securityId, book);
                }
            }
            delta = view.page.applyChanges(view.id, upserts, removals, changedRows);
        }
        view.referenceVersion = referenceVersion;
        view.rebuildRequired = false;
        return delta;
    }

    private JSONObject updateQuoteVersions(Stream stream, View view, QuoteRecord changed,
            boolean rebuild, Map<String, JSONObject> changedRows) {
        JSONObject delta;
        if (rebuild) {
            delta = view.page.replaceVersions(view.id,
                    quoteVersions(svc.selectQuotes(stream.user, view.request)));
        } else {
            QuoteQueryRequest query = ProtocolCodec.decode(view.request, QuoteQueryRequest.class);
            long started = QuotePerformanceTrace.searchStart();
            boolean matches;
            try {
                matches = svc.matcher.prepareQuery(stream.user, query, svc.access).test(changed);
            } finally {
                QuotePerformanceTrace.matchFinished(started);
            }
            delta = view.page.applyVersions(view.id,
                    matches ? Collections.singletonMap(changed.quoteId, changed.revision)
                            : Collections.emptyMap(),
                    matches ? Collections.emptySet() : Collections.singleton(changed.quoteId));
        }
        for (Object id : delta.getJSONArray("upsertIds")) {
            QuoteRecord quote = svc.quotes.get(id.toString());
            changedRows.put(quote.quoteId, QuotePageSnapshot.visibleJson(quote, stream.user, svc.access));
        }
        return delta;
    }

    void settingsChanged(String user, String kind, String id, String rev, boolean deleted) {
        for (Stream s : streams.values())
            if (s.user.equals(user) && s.subscribed) {
                try {
                    publishChanges(
                            s,
                            null,
                            null,
                            JsonUtils.object(
                                    "type",
                                    kind.equals("watchlist") ? "WATCHLIST" : "FILTER_GROUP",
                                    "id",
                                    id,
                                    "revision",
                                    rev,
                                    "action",
                                    deleted ? "REMOVE" : "UPSERT"),
                            null);
                } catch (ApiException e) {
                    for (View view : s.views.values()) {
                        if (usesSetting(view, JsonUtils.object("type",
                                kind.equals("watchlist") ? "WATCHLIST" : "FILTER_GROUP", "id", id))) {
                            view.rebuildRequired = true;
                        }
                    }
                    svc.publish.accept(
                            topic(s),
                            JsonUtils.object(
                                    "Code",
                                    "1007",
                                    "Msg",
                                    "Selected group changed or was deleted; reopen view",
                                    "kind",
                                    "RESET_REQUIRED",
                                    "streamId",
                                    s.id,
                                    "sequence",
                                    Long.toString(++s.sequence)));
                }
            }
    }

    void heartbeat() {
        for (Stream s : streams.values())
            if (s.subscribed)
                svc.publish.accept(
                        topic(s),
                        JsonUtils.object(
                                "Code",
                                "0000",
                                "Msg",
                                "成功",
                                "kind",
                                "HEARTBEAT",
                                "streamId",
                                s.id,
                                "sequence",
                                Long.toString(++s.sequence)));
        svc.publish.accept(
                "ficc.quote.events",
                JsonUtils.object(
                        "Code",
                        "0000",
                        "Msg",
                        "成功",
                        "kind",
                        "HEARTBEAT",
                        "epoch",
                        svc.store.epoch,
                        "currentSequence",
                        Long.toString(svc.lastSequence)));
    }

    void disconnected(String sid) {
        streams.values().removeIf(s -> s.session.equals(sid));
    }
}
