package ficc.quote;

import com.alibaba.fastjson2.JSONObject;
import ficc.api.QuoteRecord;
import ficc.common.BusinessClock;
import ficc.common.JsonUtils;
import ficc.common.ReferenceData;
import ficc.common.client.CrmClient;
import ficc.common.client.SimulatorBaseDataClient;
import ficc.common.client.SimulatorCrmClient;
import java.io.Reader;
import java.nio.charset.StandardCharsets;
import java.nio.file.Files;
import java.nio.file.Paths;
import java.util.ArrayList;
import java.util.Arrays;
import java.util.List;
import java.util.Properties;

/** 独立数据库 schema 验证完整业务键、人工激活边界、事务失败及重启持久化。 */
public final class QuoteBusinessKeySmoke {
    private QuoteService service;
    private int sequence;
    private int checks;

    private void check(boolean condition, String message) {
        if (!condition) {
            throw new AssertionError(message);
        }
        checks++;
        System.out.println("PASS " + message);
    }

    private JSONObject call(String user, String action, JSONObject item, String expectedCode) {
        JSONObject reply = service.handle(user, "SESSION-" + user, "ficc.quote." + action,
                JsonUtils.object("requestId", "business-key-" + (++sequence),
                        "quotes", JsonUtils.array(item)));
        check("0000".equals(reply.getString("Code")), action + " envelope: " + reply);
        JSONObject result = reply.getJSONObject("Data").getJSONArray("results").getJSONObject(0);
        check(expectedCode.equals(result.getString("Code")), action + " item: " + result);
        return result;
    }

    private JSONObject fields() {
        return JsonUtils.object("securityId", "250001.IB", "side", "1", "settlType", "2",
                "institutionId", "I-001", "contactId", "C-001", "quoteType", "1",
                "yieldType", "1", "price", "2", "quantity", "1000",
                "tradeDate", BusinessClock.currentTime().substring(0, 10));
    }

    private String create(String user, JSONObject fields) {
        fields.put("clientQuoteId", "business-key-client-" + (++sequence));
        return call(user, "create", fields, "0000").getString("quoteId");
    }

    private void action(String id, String action) {
        QuoteRecord quote = service.quotes.get(id);
        call(quote.ownerUserId, action, JsonUtils.object("quoteId", id,
                "expectedRevision", quote.revision), "0000");
    }

    private void status(String id, String expected) {
        check(expected.equals(service.quotes.get(id).status), id + " is " + expected);
    }

    private void run(ReferenceData references) throws Exception {
        String original = create("owner-base", fields());
        action(original, "confirm");
        String replacement = create("owner-base", fields());
        status(original, "ACTIVE");
        call("owner-base", "confirm", JsonUtils.object("quoteId", replacement,
                "expectedRevision", "0"), "1004");
        call("owner-base", "confirm", JsonUtils.object("quoteId", replacement,
                "expectedRevision", service.quotes.get(replacement).revision, "price", "invalid"), "1001");
        status(original, "ACTIVE");
        status(replacement, "PENDING_CONFIRM");
        action(replacement, "confirm");
        status(original, "REFERRED");
        status(replacement, "ACTIVE");
        check(service.store.scalar("SELECT payload->>'reason' FROM history WHERE quote_id=?"
                        + " AND payload->>'action'='REFER'", original).contains(replacement),
                "retired quote history links to replacing quote");
        action(original, "unrefer");
        status(replacement, "REFERRED");
        status(original, "ACTIVE");

        // 每个键字段单独变化都不能替换原报价；一个联系人可属于多个机构。
        String[][] variants = {{"securityId", "240210.IB"}, {"side", "2"}, {"settlType", "1"},
                {"institutionId", "I-002"}, {"contactId", "C-003"}, {"owner", "other-owner"}};
        for (String[] variant : variants) {
            JSONObject changed = fields();
            String owner = "owner-base";
            if ("owner".equals(variant[0])) {
                owner = variant[1];
            } else {
                changed.put(variant[0], variant[1]);
            }
            String independent = create(owner, changed);
            action(independent, "confirm");
            status(original, "ACTIVE");
            status(independent, "ACTIVE");
            if ("contactId".equals(variant[0])) {
                call(owner, "update", JsonUtils.object("quoteId", independent,
                        "expectedRevision", service.quotes.get(independent).revision,
                        "contactId", "C-001"), "0000");
                status(original, "REFERRED");
                status(independent, "ACTIVE");
                action(original, "unrefer");
                status(independent, "REFERRED");
            }
        }
        for (boolean institutionPresent : Arrays.asList(false, true)) {
            JSONObject incomplete = fields();
            incomplete.put("contactId", null);
            if (!institutionPresent) {
                incomplete.put("institutionId", null);
            }
            String first = create("owner-base", incomplete);
            JSONObject secondFields = JsonUtils.deepCopy(incomplete);
            String second = create("owner-base", secondFields);
            action(first, "confirm");
            action(second, "confirm");
            status(first, "ACTIVE");
            status(second, "ACTIVE");
            status(original, "ACTIVE");
        }

        // 第二次保存失败必须整体回滚，包括旧价撤出、事件和历史；内存不得提前变更。
        String failed = create("owner-base", fields());
        long beforeTail = service.store.tail();
        service.store.exec("CREATE FUNCTION reject_test_quote() RETURNS trigger LANGUAGE plpgsql AS $$"
                + " BEGIN IF NEW.id='" + failed + "' AND NEW.status='ACTIVE'"
                + " THEN RAISE EXCEPTION 'injected replacement failure'; END IF; RETURN NEW; END $$");
        service.store.exec("CREATE TRIGGER reject_test_quote BEFORE INSERT OR UPDATE ON quotes"
                + " FOR EACH ROW EXECUTE PROCEDURE reject_test_quote()");
        JSONObject failure = service.handle("owner-base", "SESSION-owner-base", "ficc.quote.confirm",
                JsonUtils.object("requestId", "failure-" + (++sequence), "quotes", JsonUtils.array(
                        JsonUtils.object("quoteId", failed, "expectedRevision", "1"))));
        check(!"0000".equals(failure.getString("Code")), "storage failure reaches caller");
        status(original, "ACTIVE");
        status(failed, "PENDING_CONFIRM");
        check(service.store.tail() == beforeTail, "rollback removes replacement events");
        service.store.exec("DROP TRIGGER reject_test_quote ON quotes");
        service = new QuoteService(service.store, references);
        status(original, "ACTIVE");
        status(failed, "PENDING_CONFIRM");
        action(failed, "confirm");
        service = new QuoteService(service.store, references);
        status(original, "REFERRED");
        status(failed, "ACTIVE");
    }

    public static void main(String[] args) throws Exception {
        Properties configuration = new Properties();
        try (Reader reader = Files.newBufferedReader(Paths.get(args[0]), StandardCharsets.UTF_8)) {
            configuration.load(reader);
        }
        String schema = "ficc_business_key_check_" + System.currentTimeMillis();
        configuration.setProperty("db.schema", schema);
        CrmClient crm = new CrmClient() {
            public List<JSONObject> institutions(String user) {
                return new SimulatorCrmClient().institutions(user);
            }

            public List<JSONObject> contacts(String user) {
                List<JSONObject> contacts = new ArrayList<>(new SimulatorCrmClient().contacts(user));
                contacts.get(0).put("institutionIds", JsonUtils.array("I-001", "I-002"));
                contacts.add(JsonUtils.object("contactId", "C-003", "institutionId", "I-001",
                        "contactName", "Another contact"));
                return contacts;
            }
        };
        ReferenceData references = new ReferenceData(new SimulatorBaseDataClient(), crm);
        QuoteBusinessKeySmoke test = new QuoteBusinessKeySmoke();
        try (PgStore store = new PgStore(configuration)) {
            try {
                test.service = new QuoteService(store, references);
                test.run(references);
            } finally {
                if (!schema.matches("ficc_business_key_check_[0-9]+")) {
                    throw new IllegalStateException(schema);
                }
                store.exec("DROP SCHEMA " + schema + " CASCADE");
            }
        }
        System.out.println("PASS business-key integration: " + test.checks + " checks");
    }
}
