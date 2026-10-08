package ficc.qtintake;

import com.alibaba.fastjson2.JSONArray;
import com.alibaba.fastjson2.JSONObject;
import ficc.common.JsonUtils;
import java.nio.charset.StandardCharsets;
import java.nio.file.Files;
import java.nio.file.Paths;
import quickfix.Message;

/** Anonymized three-trader FIX shapes passed through the production mapper. No FIX connection. */
public final class QTradeNegotiationFixtures {
    public static void main(String[] args) throws Exception {
        JSONArray requests = new JSONArray();
        requests.add(request("B", "-10000", "A-B", "1", "2", 1));
        requests.add(request("C", "-10000", "A-C", "1", "2", 1));
        requests.add(request("A", "INTENTION-1", "A-B", "2", "1.99", 2));
        requests.add(request("A", "INTENTION-1", "A-C", "2", "2.01", 2));
        requests.add(request("A", "INTENTION-1", "A-C", "2", "2.02", 3));
        requests.add(request("A", "INTENTION-1", "A-C", "2", "2.03", 4));
        Files.write(Paths.get(args[0]), JsonUtils.toJson(requests).getBytes(StandardCharsets.UTF_8));
        System.out.println("PASS mapped six FIX messages; shared intention, separate conversations");
    }

    private static JSONObject request(String owner, String intention, String conversation,
            String side, String price, int second) throws Exception {
        Message message = QtIntakeTest.quote(side, "1");
        message.setString(117, intention);
        message.setString(18033, conversation);
        message.setString(18004, "QT-" + owner);
        message.setString(18058, "counterparty-" + conversation);
        message.setString(18039, "1");
        message.setString("1".equals(side) ? 132 : 133, price);
        message.setString(60, "20261008-14:36:0" + second);
        JSONObject request = new QTradeQuoteMapper().map(message);
        request.getJSONObject("intake").put("ownerUserId", owner);
        // Account resolution is a separate tested boundary; fixtures supply its resolved STC user.
        request.getJSONObject("intake").put("token", "negotiation-local-test-token-32-characters");
        return request;
    }
}
