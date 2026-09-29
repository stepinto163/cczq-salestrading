package ficc.qtintake;

import com.alibaba.fastjson2.JSONObject;
import ficc.api.ApiReply;
import ficc.api.QuoteCreateRequest;
import ficc.api.QuoteOperationResult;
import ficc.api.QuoteWriteData;
import ficc.api.SalesTradingApi;

import ficc.common.JsonUtils;
import ficc.common.client.StcTransport;

import org.apache.log4j.Logger;

import java.nio.file.Path;

/** A single worker submits one buffered quote at a time; no user login is fabricated. */
public final class QuoteForwarder {
    private static final Logger LOGGER = Logger.getLogger(QuoteForwarder.class);
    private final DurableInbox inbox;
    private final StcTransport transport;
    private final String token;
    private final OwnerResolver owners;

    public interface OwnerResolver {
        String resolve(String account) throws Exception;
    }

    public QuoteForwarder(
            DurableInbox inbox, StcTransport transport, String token, OwnerResolver owners) {
        this.inbox = inbox;
        this.transport = transport;
        this.token = token;
        this.owners = owners;
    }

    public void tick() throws Exception {
        if (!transport.online()) {
            LOGGER.warn("FORWARD state=WAITING target=SERVER.QuoteSvr reason=OFFLINE");
            return;
        }
        Path pending = inbox.next();
        if (pending == null) {
            return;
        }
        QuoteCreateRequest request = SalesTradingApi.QuoteCreate.request(inbox.request(pending));
        if (request.intake.ownerUserId == null) {
            request.intake.ownerUserId = owners.resolve(request.intake.sourceUserId);
            inbox.saveRequest(pending, request.toJson());
        }
        request.intake.token = token;
        JSONObject reply = transport.request(SalesTradingApi.QuoteCreate.topic, null, request.toJson());
        ApiReply<QuoteWriteData> response = SalesTradingApi.QuoteCreate.reply(reply);
        String code = response.Code;
        if ("1009".equals(code) || "1099".equals(code)) {
            LOGGER.warn(
                    "FORWARD state=RETRY requestId="
                            + request.requestId
                            + " code="
                            + code);
            return;
        }
        boolean success = "0000".equals(code);
        if (success) {
            QuoteWriteData data = response.Data;
            if (data == null || data.results == null || data.results.size() != request.quotes.size()) {
                throw new IllegalStateException("QuoteSvr returned an incomplete create reply");
            }
            for (QuoteOperationResult result : data.results) {
                String resultCode = result.Code;
                if ("1009".equals(resultCode) || "1099".equals(resultCode)) {
                    LOGGER.warn("FORWARD state=RETRY requestId="
                            + request.requestId + " code=" + resultCode);
                    return;
                }
                success &= "0000".equals(resultCode);
            }
        }
        inbox.finish(pending, reply, success);
        String summary =
                "FORWARD state="
                        + (success ? "SENT" : "REJECTED")
                        + " requestId="
                        + request.requestId
                        + " code="
                        + code;
        if (success) {
            LOGGER.info(summary);
        } else {
            LOGGER.error(summary + " reply=" + JsonUtils.toJson(reply));
        }
    }
}
