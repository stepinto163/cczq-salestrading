package ficc.quote;

import com.alibaba.fastjson2.JSONObject;
import ficc.api.Filter;
import ficc.api.MarketEntry;
import ficc.api.PriceBook;
import ficc.api.PriceQueryRequest;
import ficc.api.ProtocolCodec;
import ficc.api.QuoteRecord;
import ficc.api.QuoteSource;
import ficc.common.ApiException;
import ficc.common.JsonUtils;
import ficc.common.ReferenceData;
import ficc.common.client.AccessPolicy;

import java.util.ArrayList;
import java.util.Arrays;
import java.util.Collections;
import java.util.LinkedHashMap;
import java.util.List;
import java.util.Map;
import java.util.Properties;
import java.util.stream.Collectors;

/** Small deterministic functional fixtures; no database, transport, UI or load test. */
public final class PriceSearchSmoke {
    private final ReferenceData references = new ReferenceData();
    private final Map<String, QuoteRecord> quotes = new LinkedHashMap<>();
    private PriceBookService books;
    private SearchSvrClient search;
    private int checks;

    private void check(boolean success, String description) {
        if (!success) {
            throw new AssertionError(description);
        }
        checks++;
    }

    private QuoteRecord quote(String id, String owner, String source, String side, String price) {
        QuoteRecord quote = new QuoteRecord();
        quote.quoteId = id;
        quote.ownerUserId = owner;
        quote.securityId = "250001.IB";
        quote.source = new QuoteSource();
        quote.source.type = source;
        quote.status = "ACTIVE";
        quote.side = side;
        quote.price = price;
        quote.quantity = "1000";
        quote.quoteType = "1";
        quote.yieldType = "1";
        quote.settlType = "1";
        quote.tradeDate = "2026-10-09";
        quote.revision = "1";
        quote.updateTime = "2026-10-09T08:00:00.000+08:00";
        quote.institutionId = "I-" + owner;
        quote.contactId = "C-" + owner;
        quote.institutionName = "机构-" + owner;
        quote.contactName = "对手-" + owner;
        quotes.put(id, quote);
        return quote;
    }

    private PriceQueryRequest request(String mode, Integer depth) {
        PriceQueryRequest request = new PriceQueryRequest();
        request.securityIds = Collections.singletonList("250001.IB");
        request.mode = mode;
        request.depth = depth;
        return request;
    }

    private PriceBook query(String user, String mode, Integer depth) {
        return search.queryPrices(user, request(mode, depth)).get(0);
    }

    private List<String> ids(PriceBook book) {
        return book.marketData.NoMDEntries.stream().map(entry -> entry.MDEntryID)
                .collect(Collectors.toList());
    }

    private void invalid(PriceQueryRequest request, String code) {
        try {
            search.queryPrices("A", request);
            throw new AssertionError("Invalid price query accepted");
        } catch (ApiException expected) {
            check(code.equals(expected.code), "Expected validation code " + code);
        }
    }

    private void run() {
        quote("bid1", "A", "QT", "1", "2.00");
        quote("bid2", "A", "QT", "1", "2.000").updateTime = "2026-10-09T08:01:00.000+08:00";
        quote("bid3", "A", "QT", "1", "2.10");
        quote("ofr1", "A", "QT", "2", "2.30");
        quote("ofr2", "A", "QT", "2", "2.20");
        quote("privateB", "B", "QT", "1", "2.50");
        quote("publicB", "B", "MANUAL", "2", "2.40");
        quote("pending", "A", "QT", "1", "0.10").status = "PENDING_CONFIRM";
        quote("referred", "A", "QT", "1", "0.20").status = "REFERRED";
        quote("deleted", "A", "QT", "1", "0.30").status = "DELETED";
        QuoteRecord unrelated = quote("unrelated", "A", "QT", "1", "not-a-number");
        unrelated.securityId = "250002.IB";
        QuoteMatcher matcher = new QuoteMatcher(references, new LinkedHashMap<>());
        AccessPolicy access = new AccessPolicy(new Properties());
        books = new PriceBookService(references, matcher, access, quotes.values());
        search = new LocalSearchSvrClient(null, references, quotes, matcher, access, books);

        PriceBook full = query("A", "FULL", null);
        check(ids(full).equals(Arrays.asList("FICC:bid2", "FICC:bid1", "FICC:bid3", "FICC:privateB",
                "FICC:publicB", "FICC:ofr1", "FICC:ofr2")), "FULL ordering and visibility");
        check(ids(query("A", "BEST", null)).equals(Arrays.asList("FICC:bid2", "FICC:bid1",
                "FICC:publicB")), "BEST retains all quotes at equal best price");
        check(ids(query("A", "DEPTH", 2)).equals(Arrays.asList("FICC:bid2", "FICC:bid1",
                "FICC:bid3", "FICC:publicB", "FICC:ofr1")), "DEPTH counts distinct prices");
        check(ids(query("B", "FULL", null)).equals(ids(full)),
                "All traders share the confirmed QT price projection");
        check(books.book(null, "250001.IB", null, "FULL", 0).marketData.NoMDEntries.size() == 7,
                "Authorized internal feed includes both recipients, only confirmed quotes");
        check(full.quoteRefs.size() == 7 && "1".equals(full.quoteRefs.get("FICC:bid2").quoteRevision),
                "Quote references preserve quote revision");
        check(JsonUtils.canonicalHash(full.toJson()).equals(JsonUtils.canonicalHash(
                books.book("A", "250001.IB", null, "FULL", 0).toJson())),
                "Search and subscription calculator yield identical payloads");

        JSONObject entry = full.marketData.NoMDEntries.get(0).toJson();
        JSONObject expected = JsonUtils.object("MDEntryID", "FICC:bid2", "MDEntryType", "0",
                "MDEntryPx", "2.000", "MDEntrySize", "1000", "PriceType", "3", "ExerciseFlag", "1",
                "SettlType", "1", "UserID", "A", "MDPriceLevel", "1", "MDEntryDate", "20261009",
                "MDEntryTime", "08:01:00.000", "Yield", "2.000", "InstitutionID", "I-A", "TraderID", "C-A",
                "InstitutionName", "机构-A", "TraderName", "对手-A");
        check(entry.equals(expected), "Exact existing market entry field names and scalar types");
        JSONObject publicEntry = full.marketData.NoMDEntries.get(4).toJson();
        check("I-B".equals(publicEntry.getString("InstitutionID")) && "C-B".equals(publicEntry.getString("TraderID")),
                "Shared quote includes institution and contact");
        check(publicEntry.getString("InstitutionName") != null && publicEntry.getString("TraderName") != null,
                "Shared quote includes counterparty names");
        check("B".equals(publicEntry.getString("UserID")), "Public quote retains its actual STC owner");
        for (String mode : Arrays.asList("FULL", "BEST", "DEPTH")) {
            MarketEntry ownEntry = query("A", mode, "DEPTH".equals(mode) ? 1 : null)
                    .marketData.NoMDEntries.get(0);
            check("机构-A".equals(ownEntry.InstitutionName) && "对手-A".equals(ownEntry.TraderName)
                    && "A".equals(ownEntry.UserID), mode + " keeps owner distinct from counterparty");
        }
        MarketEntry internalEntry = books.book(null, "250001.IB", null, "FULL", 0)
                .marketData.NoMDEntries.stream().filter(item -> "FICC:privateB".equals(item.MDEntryID))
                .findFirst().get();
        check("机构-B".equals(internalEntry.InstitutionName) && "对手-B".equals(internalEntry.TraderName),
                "Authorized internal projection carries names for the correct recipient");

        PriceQueryRequest filtered = request("FULL", null);
        filtered.filter = ProtocolCodec.decode(JsonUtils.object("conditions", JsonUtils.array(
                JsonUtils.object("field", "side", "operator", "EQ", "value", "1"))), Filter.class);
        check(search.queryPrices("A", filtered).get(0).marketData.NoMDEntries.size() == 4,
                "Typed condition filter applies before price selection");
        filtered.filter = new Filter();
        filtered.filter.market = "CREDIT";
        PriceBook empty = search.queryPrices("A", filtered).get(0);
        check(empty.marketData.NoMDEntries.isEmpty() && empty.quoteRefs.isEmpty(), "Empty filtered book");
        check(empty.toJson().containsKey("updateTime") && empty.toJson().get("updateTime") == null,
                "Empty book keeps explicit null updateTime");

        QuoteRecord pending = ProtocolCodec.decode(quotes.get("pending").toJson(), QuoteRecord.class);
        pending.price = "0.11";
        check(books.changed(pending, null).isEmpty(), "Pending edits do not change price pool");
        check("0".equals(query("A", "FULL", null).bookRevision), "Pending edits preserve book revision");
        pending = ProtocolCodec.decode(pending.toJson(), QuoteRecord.class);
        pending.status = "ACTIVE";
        check(books.changed(pending, null).equals(Collections.singleton("250001.IB")), "Confirm changes one security");
        check("1".equals(query("A", "FULL", null).bookRevision), "Confirm advances revision");
        check(ids(query("A", "BEST", null)).contains("FICC:pending"), "Confirmed quote enters BEST");
        QuoteRecord moved = ProtocolCodec.decode(pending.toJson(), QuoteRecord.class);
        moved.securityId = "240210.IB";
        check(books.changed(moved, "250001.IB").size() == 2, "Moving bond invalidates both securities");
        check(!ids(query("A", "FULL", null)).contains("FICC:pending"), "Old index removes moved quote");
        check("2".equals(query("A", "FULL", null).bookRevision), "Old book revision advances after move");
        check(ids(books.book("A", "240210.IB", null, "FULL", 0)).equals(Collections.singletonList("FICC:pending")),
                "New index contains moved quote");
        QuoteRecord removed = ProtocolCodec.decode(moved.toJson(), QuoteRecord.class);
        removed.status = "DELETED";
        books.changed(removed, null);
        PriceBook afterDelete = books.book("A", "240210.IB", null, "FULL", 0);
        check(afterDelete.marketData.NoMDEntries.isEmpty() && "2".equals(afterDelete.bookRevision),
                "Deleting last active quote emits empty revised book");

        invalid(request("OTHER", null), "1001");
        invalid(request("DEPTH", 0), "1001");
        invalid(request("DEPTH", 1001), "1001");
        invalid(request("BEST", 2), "1001");
        PriceQueryRequest missingIds = request("FULL", null);
        missingIds.securityIds = null;
        invalid(missingIds, "1008");
        PriceQueryRequest tooManyIds = request("FULL", null);
        tooManyIds.securityIds = new ArrayList<>(Collections.nCopies(101, "250001.IB"));
        invalid(tooManyIds, "1008");
        PriceQueryRequest emptyIds = request("FULL", null);
        emptyIds.securityIds = Collections.emptyList();
        check(search.queryPrices("A", emptyIds).isEmpty(), "Empty security list preserves empty response");

        QuoteRecord exercise = quote("exercise", "A", "QT", "1", "3.00");
        exercise.securityId = "102500002.IB";
        exercise.yieldType = "2";
        exercise.contactId = null;
        books.changed(exercise, null);
        MarketEntry exerciseEntry = books.book("A", exercise.securityId, null, "FULL", 0).marketData.NoMDEntries.get(0);
        check("3.00".equals(exerciseEntry.YieldToExecution) && exerciseEntry.Yield == null
                && "0".equals(exerciseEntry.ExerciseFlag), "Exercise-yield field selection");
        check(exerciseEntry.toJson().containsKey("TraderID") && exerciseEntry.TraderID == null,
                "Owned missing contact remains explicit null");
        QuoteRecord incomparable = ProtocolCodec.decode(exercise.toJson(), QuoteRecord.class);
        incomparable.quoteType = "2";
        books.changed(incomparable, null);
        PriceBook unavailable = books.book("A", exercise.securityId, null, "FULL", 0);
        check(unavailable.marketData.NoMDEntries.isEmpty() && unavailable.calculationIssues.size() == 1,
                "Unconvertible quote is explicit partial data, not a failed page or false yield");
        check(unavailable.calculationIssues.get(0).quoteId.equals(incomparable.quoteId),
                "Calculation issue identifies the quote");
        QuoteRecord cleanPrice = quote("clean", "A", "QT", "1", "101.25");
        cleanPrice.quoteType = "2";
        books.changed(cleanPrice, null);
        check(query("A", "FULL", null).marketData.NoMDEntries.size() == 7,
                "Healthy prices remain when a clean-price conversion is missing");
        check(query("A", "BEST", null).calculationIssues.size() == 1,
                "BEST reports excluded conversion without ranking clean price as yield");
        check(books.book(null, "250001.IB", null, "FULL", 0).calculationIssues.size() == 1,
                "Notification internal snapshot also tolerates a missing conversion");
        check(query("B", "FULL", null).calculationIssues.size() == 1,
                "Shared confirmed quote exposes the same calculation issue to every viewer");
        books.yieldResolver = (quote, yieldType, version) -> new QuoteYieldResolver.Result("1.95", null);
        PriceBook converted = query("A", "BEST", null);
        check(ids(converted).contains("FICC:clean") && converted.calculationIssues == null,
                "Converted quote participates in best-price selection");
        check("101.25".equals(cleanPrice.price) && "2".equals(cleanPrice.quoteType),
                "Conversion never changes original quote input");
    }

    private void checkBestTimePriority() {
        quotes.clear();
        for (String side : Arrays.asList("1", "2")) {
            quote(side + "old", "A", "QT", side, "2.00");
            QuoteRecord latest = quote(side + "new", "A", "QT", side, "2.000");
            latest.updateTime = "2026-10-09T08:00:00.001+08:00";
            latest.institutionName = "latest-" + side;
        }
        QuoteMatcher matcher = new QuoteMatcher(references, new LinkedHashMap<>());
        AccessPolicy access = new AccessPolicy(new Properties());
        books = new PriceBookService(references, matcher, access, quotes.values());
        PriceBook best = books.book("A", "250001.IB", null, "BEST", 0);
        check(ids(best).equals(Arrays.asList("FICC:1new", "FICC:1old", "FICC:2new", "FICC:2old")),
                "Both BEST sides prioritize latest update, including millisecond differences");
        check("latest-1".equals(best.marketData.NoMDEntries.get(0).InstitutionName)
                        && "latest-2".equals(best.marketData.NoMDEntries.get(2).InstitutionName),
                "First entry on each side carries latest quote identity");
        QuoteRecord updated = ProtocolCodec.decode(quotes.get("1old").toJson(), QuoteRecord.class);
        updated.updateTime = "2026-10-09T08:01:00.000+08:00";
        books.changed(updated, null);
        check("FICC:1old".equals(ids(books.book("A", "250001.IB", null, "BEST", 0)).get(0)),
                "Updating an older quote moves it ahead at equal price");
        updated.status = "DELETED";
        books.changed(updated, null);
        check("FICC:1new".equals(ids(books.book("A", "250001.IB", null, "BEST", 0)).get(0)),
                "Deleting representative promotes next latest quote");
    }

    private void checkPriceBeforeTime() {
        quotes.clear();
        quote("bid-best", "A", "QT", "1", "1.90").updateTime = "2026-10-09T07:00:00+08:00";
        quote("bid-old", "A", "QT", "1", "2.00").updateTime = "2026-10-09T08:00:00+08:00";
        quote("bid-new", "A", "QT", "1", "2.000").updateTime = "2026-10-09T00:00:00.001Z";
        quote("ofr-best", "A", "QT", "2", "2.30").updateTime = "2026-10-09T07:00:00+08:00";
        quote("ofr-old", "A", "QT", "2", "2.20").updateTime = "2026-10-09T08:00:00+08:00";
        quote("ofr-new", "A", "QT", "2", "2.200").updateTime = "2026-10-09T00:00:00.001Z";
        books = new PriceBookService(references, new QuoteMatcher(references, new LinkedHashMap<>()),
                new AccessPolicy(new Properties()), quotes.values());
        check(ids(books.book("A", "250001.IB", null, "FULL", 0)).equals(Arrays.asList(
                "FICC:bid-best", "FICC:bid-new", "FICC:bid-old",
                "FICC:ofr-best", "FICC:ofr-new", "FICC:ofr-old")),
                "FULL: yield priority precedes time; equal yields use actual instant across offsets");
        check(ids(books.book("A", "250001.IB", null, "BEST", 0)).equals(Arrays.asList(
                "FICC:bid-best", "FICC:ofr-best")), "BEST: later inferior prices never displace better prices");
        PriceBook full = books.book("A", "250001.IB", null, "FULL", 0);
        check("08:00:00.001".equals(full.marketData.NoMDEntries.get(1).MDEntryTime)
                        && "2026-10-09T00:00:00.001Z".equals(full.updateTime),
                "Quote time displays in Shanghai; book timestamp selects the actual latest instant");
        QuoteRecord better = ProtocolCodec.decode(quotes.get("bid-best").toJson(), QuoteRecord.class);
        better.price = "2.00";
        better.updateTime = "2026-10-09T00:00:00.002Z";
        books.changed(better, null);
        check(ids(books.book("A", "250001.IB", null, "BEST", 0)).equals(Arrays.asList(
                "FICC:bid-best", "FICC:bid-new", "FICC:bid-old", "FICC:ofr-best")),
                "Live update: tied best-price popup members retain newest-first order");
        QuoteRecord sameInstant = ProtocolCodec.decode(quotes.get("bid-old").toJson(), QuoteRecord.class);
        sameInstant.updateTime = "2026-10-09T08:00:00.002000+08:00";
        books.changed(sameInstant, null);
        check(ids(books.book("A", "250001.IB", null, "BEST", 0)).equals(Arrays.asList(
                "FICC:bid-best", "FICC:bid-old", "FICC:bid-new", "FICC:ofr-best")),
                "Equivalent instants use stable quoteId ordering, regardless of precision or offset");
    }

    public static void main(String[] args) {
        PriceSearchSmoke test = new PriceSearchSmoke();
        test.run();
        test.checkBestTimePriority();
        test.checkPriceBeforeTime();
        System.out.println("PASS PriceSearchSmoke: " + test.checks + " functional checks; no load or transport test");
    }
}
