package ficc.quote;

import com.alibaba.fastjson2.JSONObject;
import ficc.api.QuoteInput;
import ficc.api.QuoteRecord;
import ficc.common.ApiException;

/** Validates wire patch types once and applies named business fields. */
final class QuoteEdits {
    private QuoteEdits() {}

    static void apply(QuoteRecord quote, JSONObject patch) {
        if (patch.containsKey("securityId")) {
            Object value = patch.get("securityId");
            if (value != null && !(value instanceof String)) {
                throw ApiException.forField("1001", "VALIDATION_FAILED", "Expected String", "securityId");
            }
            quote.securityId = (String) value;
            if (value == null) {
                quote.clearField("securityId");
            }
        }
        if (patch.containsKey("institutionId")) {
            Object value = patch.get("institutionId");
            if (value != null && !(value instanceof String)) {
                throw ApiException.forField("1001", "VALIDATION_FAILED", "Expected String", "institutionId");
            }
            quote.institutionId = (String) value;
            if (value == null) {
                quote.clearField("institutionId");
            }
        }
        if (patch.containsKey("institutionName")) {
            Object value = patch.get("institutionName");
            if (value != null && !(value instanceof String)) {
                throw ApiException.forField("1001", "VALIDATION_FAILED", "Expected String", "institutionName");
            }
            quote.institutionName = (String) value;
            if (value == null) {
                quote.clearField("institutionName");
            }
        }
        if (patch.containsKey("contactId")) {
            Object value = patch.get("contactId");
            if (value != null && !(value instanceof String)) {
                throw ApiException.forField("1001", "VALIDATION_FAILED", "Expected String", "contactId");
            }
            quote.contactId = (String) value;
            if (value == null) {
                quote.clearField("contactId");
            }
        }
        if (patch.containsKey("contactName")) {
            Object value = patch.get("contactName");
            if (value != null && !(value instanceof String)) {
                throw ApiException.forField("1001", "VALIDATION_FAILED", "Expected String", "contactName");
            }
            quote.contactName = (String) value;
            if (value == null) {
                quote.clearField("contactName");
            }
        }
        if (patch.containsKey("side")) {
            Object value = patch.get("side");
            if (value != null && !(value instanceof String)) {
                throw ApiException.forField("1001", "VALIDATION_FAILED", "Expected String", "side");
            }
            quote.side = (String) value;
            if (value == null) {
                quote.clearField("side");
            }
        }
        if (patch.containsKey("quoteType")) {
            Object value = patch.get("quoteType");
            if (value != null && !(value instanceof String)) {
                throw ApiException.forField("1001", "VALIDATION_FAILED", "Expected String", "quoteType");
            }
            quote.quoteType = (String) value;
            if (value == null) {
                quote.clearField("quoteType");
            }
        }
        if (patch.containsKey("yieldType")) {
            Object value = patch.get("yieldType");
            if (value != null && !(value instanceof String)) {
                throw ApiException.forField("1001", "VALIDATION_FAILED", "Expected String", "yieldType");
            }
            quote.yieldType = (String) value;
            if (value == null) {
                quote.clearField("yieldType");
            }
        }
        if (patch.containsKey("price")) {
            Object value = patch.get("price");
            if (value != null && !(value instanceof String)) {
                throw ApiException.forField("1001", "VALIDATION_FAILED", "Expected String", "price");
            }
            quote.price = (String) value;
            if (value == null) {
                quote.clearField("price");
            }
        }
        if (patch.containsKey("quantity")) {
            Object value = patch.get("quantity");
            if (value != null && !(value instanceof String)) {
                throw ApiException.forField("1001", "VALIDATION_FAILED", "Expected String", "quantity");
            }
            quote.quantity = (String) value;
            if (value == null) {
                quote.clearField("quantity");
            }
        }
        if (patch.containsKey("quantityUnit")) {
            Object value = patch.get("quantityUnit");
            if (value != null && !(value instanceof String)) {
                throw ApiException.forField("1001", "VALIDATION_FAILED", "Expected String", "quantityUnit");
            }
            quote.quantityUnit = (String) value;
            if (value == null) {
                quote.clearField("quantityUnit");
            }
        }
        if (patch.containsKey("currency")) {
            Object value = patch.get("currency");
            if (value != null && !(value instanceof String)) {
                throw ApiException.forField("1001", "VALIDATION_FAILED", "Expected String", "currency");
            }
            quote.currency = (String) value;
            if (value == null) {
                quote.clearField("currency");
            }
        }
        if (patch.containsKey("tradeDate")) {
            Object value = patch.get("tradeDate");
            if (value != null && !(value instanceof String)) {
                throw ApiException.forField("1001", "VALIDATION_FAILED", "Expected String", "tradeDate");
            }
            quote.tradeDate = (String) value;
            if (value == null) {
                quote.clearField("tradeDate");
            }
        }
        if (patch.containsKey("settlType")) {
            Object value = patch.get("settlType");
            if (value != null && !(value instanceof String)) {
                throw ApiException.forField("1001", "VALIDATION_FAILED", "Expected String", "settlType");
            }
            quote.settlType = (String) value;
            if (value == null) {
                quote.clearField("settlType");
            }
        }
        if (patch.containsKey("urgent")) {
            Object value = patch.get("urgent");
            if (value != null && !(value instanceof Boolean)) {
                throw ApiException.forField("1001", "VALIDATION_FAILED", "Expected Boolean", "urgent");
            }
            quote.urgent = (Boolean) value;
            if (value == null) {
                quote.clearField("urgent");
            }
        }
        if (patch.containsKey("recommended")) {
            Object value = patch.get("recommended");
            if (value != null && !(value instanceof Boolean)) {
                throw ApiException.forField("1001", "VALIDATION_FAILED", "Expected Boolean", "recommended");
            }
            quote.recommended = (Boolean) value;
            if (value == null) {
                quote.clearField("recommended");
            }
        }
        if (patch.containsKey("whole")) {
            Object value = patch.get("whole");
            if (value != null && !(value instanceof Boolean)) {
                throw ApiException.forField("1001", "VALIDATION_FAILED", "Expected Boolean", "whole");
            }
            quote.whole = (Boolean) value;
            if (value == null) {
                quote.clearField("whole");
            }
        }
        if (patch.containsKey("oco")) {
            Object value = patch.get("oco");
            if (value != null && !(value instanceof Boolean)) {
                throw ApiException.forField("1001", "VALIDATION_FAILED", "Expected Boolean", "oco");
            }
            quote.oco = (Boolean) value;
            if (value == null) {
                quote.clearField("oco");
            }
        }
        if (patch.containsKey("packing")) {
            Object value = patch.get("packing");
            if (value != null && !(value instanceof Boolean)) {
                throw ApiException.forField("1001", "VALIDATION_FAILED", "Expected Boolean", "packing");
            }
            quote.packing = (Boolean) value;
            if (value == null) {
                quote.clearField("packing");
            }
        }
        if (patch.containsKey("remark")) {
            Object value = patch.get("remark");
            if (value != null && !(value instanceof String)) {
                throw ApiException.forField("1001", "VALIDATION_FAILED", "Expected String", "remark");
            }
            quote.remark = (String) value;
            if (value == null) {
                quote.clearField("remark");
            }
        }
    }

    static void copy(QuoteInput source, QuoteRecord target) {
        target.securityId = source.securityId;
        target.institutionId = source.institutionId;
        target.institutionName = source.institutionName;
        target.contactId = source.contactId;
        target.contactName = source.contactName;
        target.side = source.side;
        target.quoteType = source.quoteType;
        target.yieldType = source.yieldType;
        target.price = source.price;
        target.quantity = source.quantity;
        target.quantityUnit = source.quantityUnit;
        target.currency = source.currency;
        target.tradeDate = source.tradeDate;
        target.settlType = source.settlType;
        target.urgent = source.urgent;
        target.recommended = source.recommended;
        target.whole = source.whole;
        target.oco = source.oco;
        target.packing = source.packing;
        target.remark = source.remark;
    }
}
