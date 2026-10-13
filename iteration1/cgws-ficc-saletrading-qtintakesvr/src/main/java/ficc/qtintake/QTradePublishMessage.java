package ficc.qtintake;

import ficc.api.QTradePublishRequest;
import ficc.common.ApiException;
import quickfix.Group;
import quickfix.Message;

import java.math.BigDecimal;
import java.time.LocalDate;
import java.time.LocalDateTime;
import java.time.format.DateTimeFormatter;
import java.util.Arrays;
import java.util.HashSet;
import java.util.Set;

/** 依据价格广播 FIX V1.9 编码 U41；不借用 QuoteSvr 创建协议或接收报价的字段语义。 */
public final class QTradePublishMessage {
    private QTradePublishMessage() {}

    public static Message create(QTradePublishRequest request, LocalDateTime businessTime) {
        require(request != null, "request", "Request is required");
        text(request.messageId, "messageId", 30, true);
        require(request.messageId.matches("[A-Za-z0-9_|*\\-]{9,30}"), "messageId", "Invalid QTrade MsgID");
        text(request.senderQtAccount, "senderQtAccount", 20, true);
        require(request.senderQtAccount.matches("[0-9]+"), "senderQtAccount", "QT account must be numeric");
        require("TEXT".equals(request.mode) || "STRUCTURED".equals(request.mode), "mode", "Use TEXT or STRUCTURED");
        text(request.preText, "preText", 300, false);
        text(request.postText, "postText", 300, false);
        require(request.recipients != null && !request.recipients.isEmpty() && request.recipients.size() <= 200,
                "recipients", "Provide 1 to 200 explicit recipients");
        Message message = new Message();
        message.getHeader().setString(35, "U41");
        message.setString(18044, request.messageId);
        message.setInt(18217, 1);
        message.setString(10176, "4");
        message.setInt(537, 0);
        message.setInt(18034, 3);
        message.setInt(18035, 1);
        message.setString(18004, request.senderQtAccount);
        message.setString(18045, "N");
        message.setString(60, businessTime.format(DateTimeFormatter.ofPattern("yyyyMMdd-HH:mm:ss")));
        put(message, 18115, request.preText);
        put(message, 18116, request.postText);
        Set<String> recipients = new HashSet<>();
        for (QTradePublishRequest.Recipient recipient : request.recipients) {
            require(recipient != null && Arrays.asList(51, 52, 53, 57, 58, 59).contains(recipient.routingType),
                    "recipients", "Unsupported recipient type; tags and ALL are not publishing targets in this API");
            text(recipient.routingId, "routingId", 64, true);
            require(!request.senderQtAccount.equals(recipient.routingId), "routingId", "Cannot publish to self");
            require(recipients.add(recipient.routingType + ":" + recipient.routingId), "recipients", "Duplicate recipient");
            Group routing = new Group(215, 216, new int[] {216, 217, 0});
            routing.setInt(216, recipient.routingType);
            routing.setString(217, recipient.routingId);
            message.addGroup(routing);
        }
        if ("TEXT".equals(request.mode)) {
            require(present(request.preText) || present(request.postText), "preText", "Text broadcast is empty");
            require(request.quotes == null || request.quotes.isEmpty(), "quotes", "TEXT must not contain structured quotes");
            return message;
        }
        require(request.quotes != null && !request.quotes.isEmpty() && request.quotes.size() <= 10,
                "quotes", "Provide 1 to 10 structured quotes per broadcast");
        Set<String> quoteIds = new HashSet<>();
        for (QTradePublishRequest.Quote quote : request.quotes) {
            require(quote != null, "quotes", "Null quote");
            text(quote.clientQuoteId, "clientQuoteId", 30, true);
            require(quoteIds.add(quote.clientQuoteId), "clientQuoteId", "Duplicate ClQuoteID");
            text(quote.securityId, "securityId", 16, true);
            require(quote.securityId.matches("[A-Za-z0-9]+"), "securityId", "Use code without market suffix");
            require(Arrays.asList("XCFE", "XSHG", "XSHE", "BJSE", "CNBM").contains(quote.exchange), "exchange", "Unknown exchange");
            text(quote.symbol, "symbol", 100, true);
            text(quote.remark, "remark", 100, false);
            require(Arrays.asList("1", "2", "y").contains(quote.side), "side", "Use 1, 2 or y");
            // 与 SZ quickfix.fix44.QuoteStructured.NoQuotes 的 ORDER 保持一致：
            // Symbol 为组首，双方价格在数量之前，清算速度在交易日期之前。
            Group item = new Group(18117, 55, new int[] {55, 18031, 117, 48, 100, 54,
                    18042, 132, 18043, 133, 134, 135, 58, 18167, 18169, 18166, 18168, 0});
            item.setString(18031, quote.clientQuoteId);
            item.setString(117, "-10000");
            item.setString(48, quote.securityId);
            item.setString(100, quote.exchange);
            item.setString(55, quote.symbol);
            item.setString(54, quote.side);
            side(item, !"2".equals(quote.side), true, quote.bidPriceType, quote.bidPrice,
                    quote.bidQuantityYuan, quote.bidTradeDate, quote.bidSettlementType);
            side(item, !"1".equals(quote.side), false, quote.offerPriceType, quote.offerPrice,
                    quote.offerQuantityYuan, quote.offerTradeDate, quote.offerSettlementType);
            put(item, 58, quote.remark);
            message.addGroup(item);
        }
        return message;
    }

    private static void side(Group item, boolean enabled, boolean bid, Integer priceType,
            String price, String quantity, String date, String settlement) {
        String field = bid ? "bid" : "offer";
        if (!enabled) {
            require(priceType == null && !present(price) && !present(quantity) && !present(date) && !present(settlement),
                    field, "Fields supplied for the other direction");
            return;
        }
        require(present(price) || present(quantity), field, "Provide price or quantity for each direction");
        if (present(price)) {
            require(priceType != null && priceType >= 1 && priceType <= 4, field + "PriceType", "Price type required");
            item.setInt(bid ? 18042 : 18043, priceType);
            item.setString(bid ? 132 : 133, decimal(price, field + "Price", 4, 4));
        } else {
            require(priceType == null, field + "PriceType", "Price type without price");
        }
        if (present(quantity)) {
            item.setString(bid ? 134 : 135, decimal(quantity, field + "QuantityYuan", 13, 2));
        }
        if (present(date)) {
            try {
                item.setString(bid ? 18166 : 18168, LocalDate.parse(date).format(DateTimeFormatter.BASIC_ISO_DATE));
            } catch (RuntimeException error) {
                throw invalid(field + "TradeDate", "Use a valid yyyy-MM-dd date");
            }
        }
        if (present(settlement)) {
            require("1".equals(settlement) || "2".equals(settlement), field + "SettlementType", "Use 1 (T+0) or 2 (T+1)");
            item.setString(bid ? 18167 : 18169, settlement);
        }
    }

    private static String decimal(String value, String field, int integerDigits, int decimals) {
        try {
            require(value.matches("[0-9]+(\\.[0-9]+)?"), field, "Use plain decimal numbers");
            BigDecimal number = new BigDecimal(value);
            require(number.signum() > 0 && number.scale() <= decimals
                    && number.precision() - number.scale() <= integerDigits, field, "Number outside QTrade precision/range");
            return number.toPlainString();
        } catch (NumberFormatException error) {
            throw invalid(field, "Invalid number");
        }
    }

    private static boolean present(String value) {
        return value != null && !value.trim().isEmpty();
    }

    private static void text(String value, String field, int length, boolean required) {
        require(!required || present(value), field, "Required field: " + field);
        if (value != null) {
            require(value.length() <= length && value.indexOf('\u0001') < 0 && value.indexOf('\u0000') < 0,
                    field, "Invalid text length or FIX delimiter");
        }
    }

    private static void put(quickfix.FieldMap map, int tag, String value) {
        if (present(value)) {
            map.setString(tag, value);
        }
    }

    private static void require(boolean condition, String field, String message) {
        if (!condition) {
            throw invalid(field, message);
        }
    }

    private static ApiException invalid(String field, String message) {
        return ApiException.forField("1001", "VALIDATION_FAILED", message, field);
    }
}
