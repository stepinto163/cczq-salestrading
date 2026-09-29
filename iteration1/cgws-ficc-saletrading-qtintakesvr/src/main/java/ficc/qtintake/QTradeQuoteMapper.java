package ficc.qtintake;

import com.alibaba.fastjson2.JSONObject;

import ficc.common.JsonUtils;

import quickfix.FieldNotFound;
import quickfix.Message;

import java.math.BigDecimal;
import java.time.LocalDate;
import java.time.LocalDateTime;
import java.time.format.DateTimeFormatter;
import java.time.format.DateTimeFormatterBuilder;
import java.time.temporal.ChronoField;

/** QTrade custom tags verified against the legacy quickfixj-messages-fix44.jar. */
public final class QTradeQuoteMapper {
    public JSONObject map(Message message) throws FieldNotFound {
        if (!"S".equals(message.getHeader().getString(35))) {
            return null;
        }
        String action = required(message, 18045);
        // Legacy sales-trading intake accepts new intentions and dialogue responses only.
        if (!"N".equals(action) && !"H".equals(action)) {
            return null;
        }
        String side = required(message, 54);
        if (!"1".equals(side) && !"2".equals(side)) {
            throw new IllegalArgumentException("Unsupported QTrade Side (54): " + side);
        }
        boolean bid = "1".equals(side);
        String priceType = required(message, bid ? 18042 : 18043);
        String quoteType;
        String yieldType = null;
        switch (priceType) {
            case "1":
            case "2":
                quoteType = "1";
                yieldType = priceType;
                break;
            case "3":
                quoteType = "2";
                break;
            case "4":
                quoteType = "3";
                break;
            default:
                throw new IllegalArgumentException("Unsupported QTrade price type: " + priceType);
        }
        String account = required(message, 18004);
        String quoteId = required(message, 117);
        String eventQuoteId = quoteId;
        if ("-10000".equals(quoteId)) {
            // The downlink specification uses -10000 as an empty QuoteID.
            eventQuoteId = "NEGOTIATION:" + required(message, 18033);
        }
        String time = required(message, 60);
        // QTrade sends business-local time in tag 60, unlike the UTC header SendingTime.
        // Preserve the source timestamp for replay identity; do not add eight hours.
        DateTimeFormatter timestamp =
                new DateTimeFormatterBuilder()
                        .appendPattern("yyyyMMdd-HH:mm:ss")
                        .optionalStart()
                        .appendFraction(ChronoField.NANO_OF_SECOND, 0, 9, true)
                        .optionalEnd()
                        .toFormatter();
        LocalDate tradeDate = LocalDateTime.parse(time, timestamp).toLocalDate();
        String suppliedTradeDate = sideFieldOrCommon(message, bid ? 18166 : 18168, 75);
        if (suppliedTradeDate != null) {
            tradeDate = LocalDate.parse(suppliedTradeDate, DateTimeFormatter.BASIC_ISO_DATE);
        }
        String price = decimal(required(message, bid ? 132 : 133));
        if ("1".equals(quoteType) && new BigDecimal(price).scale() > 4) {
            throw new IllegalArgumentException("QTrade yield exceeds four decimal places");
        }
        JSONObject quote =
                JsonUtils.object(
                        "securityId",
                        securityId(required(message, 48), optional(message, 100)),
                        "side",
                        side,
                        "quoteType",
                        quoteType,
                        "yieldType",
                        yieldType,
                        "price",
                        price,
                        "quantityUnit",
                        "WAN",
                        "currency",
                        "CNY",
                        "tradeDate",
                        tradeDate.toString());
        String quantity = optional(message, bid ? 134 : 135);
        if (quantity != null) {
            BigDecimal amount = new BigDecimal(quantity);
            if (amount.signum() <= 0) {
                throw new IllegalArgumentException("QTrade quantity must be positive");
            }
            quote.put("quantity", amount.movePointLeft(4).stripTrailingZeros().toPlainString());
        }
        // QTrade BidSettlType/OfrSettlType: 1 = T+0, 2 = T+1.
        putIfPresent(quote, "settlType", sideFieldOrCommon(message, bid ? 18167 : 18169, 63));
        String institution = optional(message, 18036);
        putIfPresent(
                quote,
                "institutionName",
                institution == null ? optional(message, 18037) : institution);
        putIfPresent(quote, "contactName", optional(message, 18006));
        putIfPresent(quote, "remark", optional(message, 18026));
        JSONObject source =
                JsonUtils.object(
                        "sourceUserId",
                        account,
                        "externalQuoteId",
                        quoteId,
                        "sourceTime",
                        time,
                        "transType",
                        action,
                        "text",
                        optional(message, 18026));
        // The same QTrade quote/time/account is the same source event. Changed content
        // under this identity is rejected, not silently turned into another quote.
        String id =
                JsonUtils.canonicalHash(
                        JsonUtils.object(
                                "account",
                                account,
                                "quoteId",
                                eventQuoteId,
                                "sourceTime",
                                time,
                                "action",
                                action));
        source.put("externalMessageId", id);
        source.put("originalMessageId", originalMessageId(message, account, time, action, id));
        quote.put("clientQuoteId", "QT-" + id);
        return JsonUtils.object(
                "requestId", "QT-" + id, "intake", source, "quotes", JsonUtils.array(quote));
    }

    private static String originalMessageId(
            Message message, String account, String time, String action, String eventId)
            throws FieldNotFound {
        String text = optional(message, 18026);
        if (text == null) {
            // Never merge unrelated quotes when their original text is unavailable.
            return "QT-EVENT-" + eventId;
        }
        // Quote(S) V1.4 has no original-message ID. Keep business time and the
        // conversation in this fallback; content alone would merge repeated messages.
        JSONObject identity = JsonUtils.object(
                "account", account,
                "sourceTime", time,
                "action", action,
                "operator", optional(message, 18035),
                "chatType", optional(message, 18039),
                "chatGroupId", optional(message, 18474) == null
                        ? optional(message, 18136) : optional(message, 18474),
                "senderId", optional(message, 18058),
                "text", text);
        if (identity.getString("senderId") == null) {
            identity.put("senderName", optional(message, 18006));
            identity.put("institution", optional(message, 18036));
            identity.put("institutionShortName", optional(message, 18037));
        }
        return "QT-MESSAGE-" + JsonUtils.canonicalHash(identity);
    }

    private static String sideFieldOrCommon(Message message, int sideTag, int commonTag)
            throws FieldNotFound {
        String value = optional(message, sideTag);
        return value == null ? optional(message, commonTag) : value;
    }

    private static String securityId(String code, String exchange) {
        String suffix;
        if (exchange == null && code.matches(".+\\.(IB|SH|SZ|BJ|BC)")) {
            return code;
        }
        if (exchange == null) {
            throw new IllegalArgumentException(
                    "QTrade ExDestination (100) is required for a bare bond code");
        }
        switch (exchange) {
            case "XCFE":
                suffix = ".IB";
                break;
            case "XSHG":
                suffix = ".SH";
                break;
            case "XSHE":
                suffix = ".SZ";
                break;
            case "BJSE":
                suffix = ".BJ";
                break;
            case "CNBM":
                suffix = ".BC";
                break;
            default:
                throw new IllegalArgumentException("Unsupported QTrade exchange: " + exchange);
        }
        if (code.contains(".")) {
            if (!code.endsWith(suffix)) {
                throw new IllegalArgumentException(
                        "QTrade security suffix conflicts with exchange");
            }
            return code;
        }
        return code + suffix;
    }

    private static String decimal(String value) {
        return new BigDecimal(value).stripTrailingZeros().toPlainString();
    }

    private static void putIfPresent(JSONObject object, String field, String value) {
        if (value != null) {
            object.put(field, value);
        }
    }

    private static String optional(Message message, int tag) throws FieldNotFound {
        if (!message.isSetField(tag)) {
            return null;
        }
        String value = message.getString(tag).trim();
        return value.isEmpty() ? null : value;
    }

    private static String required(Message message, int tag) throws FieldNotFound {
        String value = optional(message, tag);
        if (value == null) {
            throw new IllegalArgumentException("Missing QTrade field " + tag);
        }
        return value;
    }
}
