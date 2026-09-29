package cgws.ficc.parser.parse;

import cgws.ficc.parser.model.QuoteInfo;

import java.util.ArrayList;
import java.util.LinkedHashMap;
import java.util.List;
import java.util.Map;

/**
 * Codec for the wire format the native parser returns: {@code key:value;key:value;}
 *
 * <p>Keeping this codec lets the replacement produce the exact legacy string, which is what the
 * comparison runner diffs against the working binary. Field order follows the observed order:
 * Direction, BidPrice, OfrPrice, Code, Name, CodeList, Volume, Single, Number, TraderName, debug.
 */
public final class LegacyResultCodec {

    public static final String HEADER = "header";

    private LegacyResultCodec() {
    }

    /** Renders a quote into the legacy string; empty string when nothing was recognised. */
    public static String render(QuoteInfo q) {
        if (q == null) {
            return "";
        }
        StringBuilder b = new StringBuilder();
        append(b, "Direction", q.Direction);
        append(b, "Code", q.Code);
        append(b, "Name", q.Name);
        append(b, "BidPrice", q.BidPrice);
        append(b, "OfrPrice", q.OfrPrice);
        if (q.lstCode != null && !q.lstCode.isEmpty()) {
            append(b, "CodeList", join(q.lstCode));
        }
        append(b, "Volume", q.Volume);
        if (q.lstSingle != null && !q.lstSingle.isEmpty()) {
            append(b, "Single", join(q.lstSingle));
        }
        if (q.lstNumber != null && !q.lstNumber.isEmpty()) {
            append(b, "Number", join(q.lstNumber));
        }
        append(b, "TradeDay", q.tradeDay);
        append(b, "TraderName", q.traderName);
        append(b, "debug", q.debug);
        return b.toString();
    }

    /** Parses a legacy result string into a quote; used to diff baseline output field by field. */
    public static QuoteInfo parse(String raw) {
        QuoteInfo q = new QuoteInfo();
        if (raw == null || raw.isEmpty()) {
            return q;
        }
        String[] parts = raw.split(";");
        for (String part : parts) {
            int colon = part.indexOf(':');
            if (colon < 0) {
                continue;
            }
            String key = part.substring(0, colon).trim();
            String value = part.substring(colon + 1);
            assign(q, key, value);
        }
        return q;
    }

    public static Map<String, String> flatten(QuoteInfo q) {
        Map<String, String> out = new LinkedHashMap<String, String>();
        if (q == null) {
            return out;
        }
        put(out, "Direction", q.Direction);
        put(out, "BidPrice", q.BidPrice);
        put(out, "OfrPrice", q.OfrPrice);
        put(out, "Code", q.Code);
        put(out, "Name", q.Name);
        put(out, "CodeList", q.lstCode == null ? null : join(q.lstCode));
        put(out, "Volume", q.Volume);
        put(out, "Single", q.lstSingle == null ? null : join(q.lstSingle));
        put(out, "Number", q.lstNumber == null ? null : join(q.lstNumber));
        put(out, "TradeDay", q.tradeDay);
        put(out, "TraderName", q.traderName);
        put(out, "debug", q.debug);
        return out;
    }

    private static void put(Map<String, String> map, String key, String value) {
        if (value != null) {
            map.put(key, value);
        }
    }

    private static void assign(QuoteInfo q, String key, String value) {
        if ("Direction".equals(key)) {
            q.Direction = value;
        } else if ("Code".equals(key)) {
            q.Code = value;
        } else if ("CodeList".equals(key)) {
            q.lstCode = splitList(value);
        } else if ("Name".equals(key)) {
            q.Name = value;
        } else if ("IsPrice".equals(key)) {
            q.IsPrice = value;
        } else if ("Tenor".equals(key)) {
            q.Tenor = value;
        } else if ("BidPrice".equals(key)) {
            q.BidPrice = value;
        } else if ("OfrPrice".equals(key)) {
            q.OfrPrice = value;
        } else if ("BidTag".equals(key)) {
            q.BidTag = value;
        } else if ("OfrTag".equals(key)) {
            q.OfrTag = value;
        } else if ("Price".equals(key)) {
            q.lstPrice = splitList(value);
        } else if ("BidVolume".equals(key)) {
            q.BidVolume = value;
        } else if ("OfrVolume".equals(key)) {
            q.OfrVolume = value;
        } else if ("Volume".equals(key)) {
            q.Volume = value;
        } else if ("Single".equals(key)) {
            q.lstSingle = splitList(value);
        } else if ("Number".equals(key)) {
            q.lstNumber = splitList(value);
        } else if ("Valuation".equals(key)) {
            q.tradeDay = value;
        } else if ("BigNumPrice".equals(key)) {
            q.BigNumPrice = value;
        } else if ("TradeDay".equals(key)) {
            q.tradeDay = value;
        } else if ("TraderName".equals(key)) {
            q.traderName = value;
        } else if ("debug".equals(key)) {
            q.debug = value;
        } else if ("IsDeal".equals(key)) {
            q.isDeal = true;
        }
    }

    private static List<String> splitList(String value) {
        List<String> out = new ArrayList<String>();
        for (String item : value.split(",")) {
            if (!item.isEmpty()) {
                out.add(item);
            }
        }
        return out;
    }

    private static String join(List<String> values) {
        StringBuilder b = new StringBuilder();
        for (String v : values) {
            b.append(v).append(',');
        }
        return b.toString();
    }

    private static void append(StringBuilder b, String key, String value) {
        if (value == null || value.isEmpty()) {
            return;
        }
        b.append(key).append(':').append(value).append(';');
    }
}
