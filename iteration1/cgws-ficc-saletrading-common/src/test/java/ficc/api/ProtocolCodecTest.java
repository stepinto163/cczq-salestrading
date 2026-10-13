package ficc.api;

import com.alibaba.fastjson2.JSON;
import com.alibaba.fastjson2.JSONObject;
import org.junit.jupiter.api.Test;
import java.util.Arrays;
import java.util.Collections;
import static org.junit.jupiter.api.Assertions.*;

class ProtocolCodecTest {
    @Test
    void patchKeepsMissingNullFalseAndOriginalNumberTokens() {
        JSONObject input = JSON.parseObject("{\"requestId\":\"R\",\"quotes\":[{\"quoteId\":\"Q\","
                + "\"expectedRevision\":\"9007199254740993\",\"contactId\":null,\"urgent\":false,\"price\":2.50}]}");
        QuoteChangeRequest request = ProtocolCodec.decode(input, QuoteChangeRequest.class);
        assertEquals(input, request.toJson());
        assertFalse(request.toJson().getJSONArray("quotes").getJSONObject(0).containsKey("quantity"));
        request.quotes.get(0).price = "2.6";
        assertEquals("2.6", request.toJson().getJSONArray("quotes").getJSONObject(0).getString("price"));
        assertTrue(request.toJson().getJSONArray("quotes").getJSONObject(0).containsKey("contactId"));
    }

    @Test
    void newPatchRequiresExplicitClearAndIncludesFalse() {
        QuoteChangeItem item = new QuoteChangeItem();
        item.quoteId = "Q";
        item.urgent = false;
        item.clearField("contactId");
        QuoteChangeRequest request = new QuoteChangeRequest();
        request.quotes = Arrays.asList(item);
        JSONObject wire = request.toJson().getJSONArray("quotes").getJSONObject(0);
        assertTrue(wire.containsKey("contactId"));
        assertNull(wire.get("contactId"));
        assertFalse(wire.containsKey("institutionId"));
        assertEquals(false, wire.getBoolean("urgent"));
    }

    @Test
    void unknownFieldsFollowTheirRecordWhenRowsAreReordered() {
        QuoteViewData data = ProtocolCodec.decode(JSON.parseObject("{\"quotes\":["
                + "{\"quoteId\":\"A\",\"futureField\":1},{\"quoteId\":\"B\",\"futureField\":2}]}"), QuoteViewData.class);
        Collections.reverse(data.quotes);
        data.quotes.get(0).price = "3";
        assertEquals(2, data.toJson().getJSONArray("quotes").getJSONObject(0).getIntValue("futureField"));
        assertEquals("B", data.toJson().getJSONArray("quotes").getJSONObject(0).getString("quoteId"));
    }

    @Test
    void requestCatalogHasDistinctDomainsAndPreservesUnknownFieldsForValidation() {
        assertEquals(59, SalesTradingApi.all().size());
        assertEquals(QTradePublishData.class, SalesTradingApi.QTradeQuotePublish.dataType);
        assertEquals(QuoteViewData.class, SalesTradingApi.QuoteViewOpen.dataType);
        assertEquals(PriceViewData.class, SalesTradingApi.PriceViewOpen.dataType);
        assertEquals(NotificationViewData.class, SalesTradingApi.NotificationViewOpen.dataType);
        JSONObject input = JSON.parseObject("{\"requestId\":\"R\",\"notAllowed\":1,\"quotes\":[]}");
        assertEquals(input, ProtocolBoundary.request("ficc.quote.create", input));
    }

    @Test
    void streamAndStcMarketFieldCasingRoundTrip() {
        JSONObject input = JSON.parseObject("{\"Code\":\"0000\",\"kind\":\"DELTA\",\"streamId\":\"P\","
                + "\"sequence\":\"9007199254740993\",\"books\":[{\"securityId\":\"240210.IB\","
                + "\"marketData\":{\"NoMDEntries\":[{\"MDEntryType\":\"0\",\"MDEntryPx\":\"2.5000\"}]}}]}");
        assertEquals(input, ProtocolBoundary.push("ficc.price.report.SID", input));
    }
}
