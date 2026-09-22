import ficc.common.JdbcStore;
import ficc.common.JsonUtils;

import java.io.BufferedReader;
import java.nio.file.Files;
import java.nio.file.Paths;
import java.util.Properties;
import java.util.UUID;

/** Run with both services stopped. All test events and receipts are rolled back. */
public final class SharedSchemaSmoke {
    public static void main(String[] arguments) throws Exception {
        Properties configuration = new Properties();
        try (BufferedReader reader = Files.newBufferedReader(Paths.get(arguments[0]))) {
            configuration.load(reader);
        }
        try (JdbcStore quotes = new JdbcStore(configuration);
                JdbcStore notifications = new JdbcStore(configuration, "notification_")) {
            require(!quotes.epoch.equals(notifications.epoch), "Independent event epochs");
            require(
                    !quotes.table("settings").equals(notifications.table("settings")),
                    "Separate settings");
            requireDuplicateWriterRejected(configuration, "");
            requireDuplicateWriterRejected(configuration, "notification_");

            long quoteSequence = quotes.tail();
            long notificationSequence = notifications.tail();
            String requestId = "schema-test-" + UUID.randomUUID();
            try {
                quotes.begin();
                notifications.begin();
                quotes.event(JsonUtils.object("type", "SCHEMA_TEST"));
                require(notifications.tail() == notificationSequence, "Quote event isolation");
                notifications.event(JsonUtils.object("type", "SCHEMA_TEST"));
                require(quotes.tail() == quoteSequence + 1, "Notification event isolation");
                require(notifications.tail() == notificationSequence + 1, "Notification sequence");

                quotes.receipt("SCHEMA_TEST", "same-topic", requestId, "quote-hash");
                notifications.receipt("SCHEMA_TEST", "same-topic", requestId, "notification-hash");
                quotes.receipt(
                        "SCHEMA_TEST",
                        "same-topic",
                        requestId,
                        JsonUtils.array(JsonUtils.object("service", "quote")));
                notifications.receipt(
                        "SCHEMA_TEST",
                        "same-topic",
                        requestId,
                        JsonUtils.array(JsonUtils.object("service", "notification")));
                require(
                        "quote"
                                .equals(
                                        quotes.receipt(
                                                        "SCHEMA_TEST",
                                                        "same-topic",
                                                        requestId,
                                                        "quote-hash")
                                                .getJSONObject(0)
                                                .getString("service")),
                        "Quote receipt isolation");
                require(
                        "notification"
                                .equals(
                                        notifications
                                                .receipt(
                                                        "SCHEMA_TEST",
                                                        "same-topic",
                                                        requestId,
                                                        "notification-hash")
                                                .getJSONObject(0)
                                                .getString("service")),
                        "Notification receipt isolation");
            } finally {
                quotes.rollback();
                notifications.rollback();
            }
            require(quotes.tail() == quoteSequence, "Quote rollback");
            require(notifications.tail() == notificationSequence, "Notification rollback");
        }
        System.out.println(
                "PASS: shared schema, independent writers/events/receipts/settings, rollback; zero"
                        + " test quotes");
    }

    private static void requireDuplicateWriterRejected(Properties configuration, String prefix)
            throws Exception {
        boolean rejected = false;
        try (JdbcStore unexpected = new JdbcStore(configuration, prefix)) {
            // The existing same-service connection must retain its writer lock.
        } catch (IllegalStateException expected) {
            rejected = true;
        }
        require(rejected, "Duplicate writer rejected: " + prefix);
    }

    private static void require(boolean condition, String message) {
        if (!condition) {
            throw new AssertionError(message);
        }
    }
}
