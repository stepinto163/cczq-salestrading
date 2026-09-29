package ficc.qtintake;

import com.alibaba.fastjson2.JSONArray;
import com.alibaba.fastjson2.JSONObject;

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
        JSONObject request = JsonUtils.deepCopy(inbox.request(pending));
        JSONObject source = request.getJSONObject("intake");
        if (source.getString("ownerUserId") == null) {
            source.put("ownerUserId", owners.resolve(source.getString("sourceUserId")));
            inbox.saveRequest(pending, request);
        }
        request.getJSONObject("intake").put("token", token);
        JSONObject reply = transport.request("ficc.quote.create", null, request);
        String code = reply.getString("Code");
        if ("1009".equals(code) || "1099".equals(code)) {
            LOGGER.warn(
                    "FORWARD state=RETRY requestId="
                            + request.getString("requestId")
                            + " code="
                            + code);
            return;
        }
        boolean success = "0000".equals(code);
        if (success) {
            JSONObject data = reply.getJSONObject("Data");
            JSONArray results = data == null ? null : data.getJSONArray("results");
            if (results == null || results.size() != request.getJSONArray("quotes").size()) {
                throw new IllegalStateException("QuoteSvr returned an incomplete create reply");
            }
            for (Object value : results) {
                JSONObject result = (JSONObject) value;
                String resultCode = result.getString("Code");
                if ("1009".equals(resultCode) || "1099".equals(resultCode)) {
                    LOGGER.warn("FORWARD state=RETRY requestId="
                            + request.getString("requestId") + " code=" + resultCode);
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
                        + request.getString("requestId")
                        + " code="
                        + code;
        if (success) {
            LOGGER.info(summary);
        } else {
            LOGGER.error(summary + " reply=" + JsonUtils.toJson(reply));
        }
    }
}
