package cgws.ficc.parser.parse;

import cgws.ficc.parser.identify.ShortCodeResolver;
import cgws.ficc.parser.model.AllQuote;
import cgws.ficc.parser.model.BondQuote;
import cgws.ficc.parser.model.BondReference;
import cgws.ficc.parser.model.QuoteInfo;
import cgws.ficc.parser.text.TextNormalizer;

import java.math.BigDecimal;
import java.time.LocalDate;
import java.util.ArrayList;
import java.util.Collections;
import java.util.Comparator;
import java.util.LinkedHashSet;
import java.util.List;
import java.util.Locale;
import java.util.Set;
import java.util.regex.Matcher;
import java.util.regex.Pattern;

/** Message-level coordination; separate from the legacy native single-line comparison boundary. */
public final class QuoteMessageParser {
    private static final Pattern CLOCK = Pattern.compile("(?:[01]?\\d|2[0-3]):[0-5]\\d:[0-5]\\d");
    private static final Pattern CHINESE_WORD = Pattern.compile("[\\u4e00-\\u9fa5]{2,}");
    private static final Pattern SETTLEMENT = Pattern.compile("(?<![\\d.])(?:t)?\\+([01])(?!\\d)");
    private static final Pattern VALUATION = Pattern.compile("(?:中债|估值)([+-]\\d+(?:\\.\\d+)?)\\s*bp");
    private static final Pattern QUALIFIERS = Pattern.compile("行权|到期|全价|净价|收益率|oco|打包");
    private final MessageReferenceData reference;

    public QuoteMessageParser(MessageReferenceData reference) {
        if (reference == null) {
            throw new IllegalArgumentException("Parser reference snapshot is required");
        }
        this.reference = reference;
    }

    public AllQuote parse(String input, LocalDate businessDate, boolean shortCodesEnabled) {
        if (input == null || businessDate == null) {
            throw new IllegalArgumentException("Text and business date are required");
        }
        if (input.length() > 200000) {
            throw new IllegalArgumentException("Parser text exceeds 200000 characters");
        }
        String text = input.replace('\u00a0', ' ').replace('；', ';').toLowerCase(Locale.ROOT).trim();
        List<Segment> segments = segments(text);
        if (segments.size() > 1000) {
            throw new IllegalArgumentException("Parser text exceeds 1000 segments");
        }
        AllQuote result = new AllQuote();
        result.isDeal = text.contains("当日成交") || text.contains("今日成交");
        // Disable abbreviation interpretation for the entire message when a full code exists.
        // This avoids an earlier short-code row surviving a later full-code discovery.
        boolean hasFullCode = false;
        for (Segment segment : segments) {
            QuoteInfo fields = fields(segment.text);
            hasFullCode |= fields != null && !fields.lstCode.isEmpty();
        }
        result.isFullCode = hasFullCode;
        ShortCodeResolver shortCodes = new ShortCodeResolver(reference.shortCodes, businessDate);
        Title title = new Title();
        if (segments.size() > 1) {
            Segment last = segments.get(segments.size() - 1);
            QuoteInfo tail = fields(last.text);
            if (tail != null && tail.lstCode.isEmpty() && tail.Direction != null
                    && !(shortCodesEnabled && shortCodes.replaceFirst(last.text) != null)) {
                title.assign(tail, last.text);
                segments = new ArrayList<Segment>(segments);
                Collections.reverse(segments);
            }
        }
        for (Segment segment : segments) {
            if (readIdentity(segment.text, result)) {
                continue;
            }
            QuoteInfo fields = fields(segment.text);
            if (fields == null) {
                continue;
            }
            if (fields.lstCode.isEmpty() && shortCodesEnabled && !hasFullCode) {
                String expanded = shortCodes.replaceFirst(segment.text);
                if (expanded != null) {
                    fields = fields(expanded);
                    result.isShortCode = fields != null && !fields.lstCode.isEmpty() || result.isShortCode;
                }
            }
            if (fields == null) {
                continue;
            }
            if (fields.lstCode.isEmpty()) {
                // A suppressed short-code quote is not a message heading.
                if (hasFullCode && shortCodes.replaceFirst(segment.text) != null) {
                    continue;
                }
                if (fields.Direction != null || fields.tradeDay != null) {
                    title.assign(fields, segment.text);
                } else if (!result.lstQuote.isEmpty()) {
                    BondQuote previous = result.lstQuote.get(result.lstQuote.size() - 1);
                    if (fields.Volume != null) {
                        previous.Volume = fields.Volume;
                    }
                    if (previous.Price.isEmpty()) {
                        previous.Price = price(fields);
                    }
                }
                continue;
            }
            for (String code : fields.lstCode) {
                BondReference bond = reference.get(code);
                if (bond == null || bond.invalid) {
                    continue;
                }
                boolean paired = fields.BidPrice != null || fields.OfrPrice != null;
                if (paired && !title.appointsPrice()) {
                    if (fields.BidPrice != null && (fields.Direction == null || "bid".equals(fields.Direction))) {
                        result.lstQuote.add(quote(fields, bond, segment, title, "bid", fields.BidPrice, true));
                    }
                    if (fields.OfrPrice != null && (fields.Direction == null || "ofr".equals(fields.Direction))) {
                        result.lstQuote.add(quote(fields, bond, segment, title, "ofr", fields.OfrPrice, true));
                    }
                } else {
                    result.lstQuote.add(quote(fields, bond, segment, title,
                            fields.Direction == null ? title.direction : fields.Direction, price(fields), false));
                }
            }
        }
        Collections.sort(result.lstQuote, Comparator.comparingInt(quote -> quote.lineNo));
        return result;
    }

    private BondQuote quote(QuoteInfo fields, BondReference bond, Segment segment, Title title,
            String direction, String price, boolean paired) {
        BondQuote quote = new BondQuote();
        quote.Code = bond.code;
        quote.Name = bond.name;
        quote.Key = bond.key;
        quote.BondType = bond.bondType;
        quote.hasOption = bond.hasOption;
        quote.netPrice = bond.valuationYield;
        quote.Text = segment.source;
        quote.lineNo = segment.lineNumber;
        quote.Direction = direction.isEmpty() ? "ofr" : direction;
        quote.Price = price;
        quote.Volume = value(fields.Volume);
        quote.settlementDay = value(fields.tradeDay);
        quote.IsInternal = segment.text.contains("暗盘");
        quote.probolity = paired || fields.lstCode.size() > 1 ? 50 : 100;
        if (title.appointsPrice()) {
            quote.Direction = title.direction;
            if (!title.price.isEmpty()) {
                quote.Price = title.price;
            }
            if (!title.volume.isEmpty()) {
                quote.Volume = title.volume;
            }
        }
        if (!title.settlement.isEmpty()) {
            quote.settlementDay = title.settlement;
        }
        Set<String> tags = new LinkedHashSet<String>();
        if (segment.text.contains("行权")) {
            tags.add("行权");
            quote.Notes = "行权";
            quote.expiryType = 1;
        }
        if (segment.text.contains("到期")) {
            tags.add("到期");
        }
        if (segment.source.contains("oco")) {
            tags.add("oco");
        }
        if (segment.source.contains("打包")) {
            tags.add("打包");
        }
        quote.PriceTag = String.join("|", tags);
        Matcher spread = VALUATION.matcher(segment.text);
        if (spread.find()) {
            if (bond.valuationYield.isEmpty()) {
                throw new IllegalStateException("Valuation is unavailable for " + bond.code);
            }
            quote.Price = new BigDecimal(bond.valuationYield).add(
                    new BigDecimal(spread.group(1)).movePointLeft(2)).stripTrailingZeros().toPlainString();
        }
        if (!quote.Price.isEmpty()) {
            quote.priceType = segment.text.contains("全价") ? 2
                    : segment.text.contains("净价") || new BigDecimal(quote.Price).compareTo(BigDecimal.valueOf(50)) > 0 ? 1 : 0;
        }
        return quote;
    }

    private QuoteInfo fields(String text) {
        String normalized = ChineseQuantityNormalizer.normalize(TextNormalizer.javaStage(text));
        normalized = normalized.replaceAll("[，,:：\\t]", " ");
        // Qualifiers are product fields, never CRM trader names.
        String parseText = QUALIFIERS.matcher(normalized).replaceAll(" ");
        QuoteInfo fields = QuoteFieldParser.parseLine(parseText, reference.dictionary);
        if (fields != null) {
            Matcher settlement = SETTLEMENT.matcher(normalized);
            if (settlement.find()) {
                fields.tradeDay = "+" + settlement.group(1);
            }
        }
        return fields;
    }

    private static String price(QuoteInfo fields) {
        for (String candidate : fields.lstSingle) {
            BigDecimal value = new BigDecimal(candidate);
            if (value.compareTo(new BigDecimal("0.1")) > 0 && value.compareTo(BigDecimal.valueOf(200)) < 0) {
                return candidate;
            }
        }
        return "";
    }

    private static boolean readIdentity(String text, AllQuote result) {
        String withoutTime = CLOCK.matcher(text).replaceAll("").trim();
        if (withoutTime.matches(".*[0-9].*") || withoutTime.split("\\s+").length > 5) {
            return false;
        }
        Matcher words = CHINESE_WORD.matcher(withoutTime);
        List<String> names = new ArrayList<String>();
        String company = "";
        while (words.find()) {
            String word = words.group();
            if (word.matches(".*(证券|银行|基金|资管|信托|保险|农商|人寿|养老).*")) {
                company = word;
            } else if (word.length() <= 4 && !QUALIFIERS.matcher(word).matches()) {
                names.add(word);
            }
        }
        if (company.isEmpty()) {
            return false;
        }
        result.company = company;
        if (!names.isEmpty()) {
            result.trader = names.get(0);
        }
        return true;
    }

    private static List<Segment> segments(String text) {
        List<Segment> result = new ArrayList<Segment>();
        String[] lines = text.split("\\r\\n|\\n|\\r", -1);
        for (int line = 0; line < lines.length; line++) {
            String source = lines[line];
            Matcher timestamp = CLOCK.matcher(source);
            String expanded = timestamp.find() ? source.substring(0, timestamp.end()) + ";"
                    + source.substring(timestamp.end()) : source;
            for (String part : expanded.split(";")) {
                if (!part.trim().isEmpty()) {
                    result.add(new Segment(part.trim(), source, line));
                }
            }
        }
        return result;
    }

    private static String value(String text) {
        return text == null ? "" : text;
    }

    private static final class Segment {
        final String text;
        final String source;
        final int lineNumber;

        Segment(String text, String source, int lineNumber) {
            this.text = text;
            this.source = source;
            this.lineNumber = lineNumber;
        }
    }

    private static final class Title {
        String direction = "";
        String price = "";
        String volume = "";
        String settlement = "";

        void assign(QuoteInfo fields, String text) {
            direction = value(fields.Direction);
            price = price(fields);
            volume = text.contains("oco") ? "" : value(fields.Volume);
            settlement = value(fields.tradeDay);
        }

        boolean appointsPrice() {
            return !direction.isEmpty() && (!price.isEmpty() || !volume.isEmpty());
        }
    }
}
