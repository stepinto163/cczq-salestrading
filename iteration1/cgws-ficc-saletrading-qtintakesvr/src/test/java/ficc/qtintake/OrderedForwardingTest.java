package ficc.qtintake;

import static org.junit.jupiter.api.Assertions.*;

import com.alibaba.fastjson2.JSONObject;
import ficc.common.JsonUtils;
import ficc.common.client.StcTransport;
import java.nio.file.Files;
import java.nio.file.Path;
import java.nio.file.attribute.FileTime;
import java.util.ArrayList;
import java.util.List;
import java.util.concurrent.CountDownLatch;
import java.util.concurrent.TimeUnit;
import java.util.function.Consumer;
import org.junit.jupiter.api.Test;
import org.junit.jupiter.api.Timeout;
import org.junit.jupiter.api.io.TempDir;
import quickfix.Message;

@Timeout(10)
class OrderedForwardingTest {
    @TempDir Path directory;

    private JSONObject request(String id) throws Exception {
        Message message = QtIntakeTest.quote("1", "1");
        message.setString(117, id);
        return new QTradeQuoteMapper().map(message);
    }

    private static JSONObject success() {
        return JsonUtils.object("Code", "0000", "Data", JsonUtils.object("results",
                JsonUtils.array(JsonUtils.object("Code", "0000"))));
    }

    private static final class Transport implements StcTransport {
        final List<String> requests = new ArrayList<>();
        JSONObject reply = success();
        boolean online = true;
        RuntimeException failure;
        CountDownLatch received;

        public JSONObject request(String topic, String session, JSONObject request) {
            requests.add(request.getString("requestId"));
            if (received != null) {
                received.countDown();
            }
            if (failure != null) {
                throw failure;
            }
            return reply;
        }

        public boolean online() { return online; }
        public void subscribe(String topic, Consumer<JSONObject> listener) {}
        public void connectionChanged(Runnable listener) {}
    }

    private QuoteForwarder forwarder(DurableInbox inbox, Transport transport) {
        return new QuoteForwarder(inbox, transport, "test-token", account -> "U-001");
    }

    @Test
    void timeoutAndRestartNeverAllowSecondRequestToOvertakeFirst() throws Exception {
        JSONObject first = request("FIRST");
        JSONObject second = request("SECOND");
        Transport transport = new Transport();
        try (DurableInbox inbox = new DurableInbox(directory, 10)) {
            inbox.enqueue(first);
            Path head = inbox.next();
            inbox.enqueue(second);
            transport.failure = new IllegalStateException(new java.net.SocketTimeoutException("test"));
            assertEquals(QuoteForwarder.Outcome.RETRY, forwarder(inbox, transport).tick());
            assertEquals(head, inbox.next());
            assertEquals(first.getString("requestId"), inbox.request(head).getString("requestId"));
            // mtime changes must not affect the persisted arrival sequence.
            Files.setLastModifiedTime(head, FileTime.fromMillis(System.currentTimeMillis() + 60000));
        }
        transport.failure = null;
        try (DurableInbox inbox = new DurableInbox(directory, 10)) {
            QuoteForwarder forwarder = forwarder(inbox, transport);
            assertEquals(first.getString("requestId"), inbox.request(inbox.next()).getString("requestId"));
            forwarder.tick();
            forwarder.tick();
            assertNull(inbox.next());
            assertEquals(java.util.Arrays.asList(first.getString("requestId"),
                    first.getString("requestId"), second.getString("requestId")), transport.requests);
        }
    }

    @Test
    void onlyExplicitFieldValidationIsRejected() throws Exception {
        Transport transport = new Transport();
        try (DurableInbox inbox = new DurableInbox(directory, 10)) {
            inbox.enqueue(request("FIRST"));
            inbox.enqueue(request("SECOND"));
            Path head = inbox.next();
            QuoteForwarder forwarder = forwarder(inbox, transport);
            for (String code : new String[] {"1003", "1006", "1001", "UNKNOWN"}) {
                transport.reply = JsonUtils.object("Code", code);
                assertEquals(QuoteForwarder.Outcome.BLOCKED, forwarder.tick());
                assertEquals(head, inbox.next());
            }
            transport.reply = JsonUtils.object("Code", "1001", "error",
                    JsonUtils.object("type", "VALIDATION_FAILED", "field", "ownerUserId"));
            assertEquals(QuoteForwarder.Outcome.BLOCKED, forwarder.tick());
            assertEquals(head, inbox.next());
            for (String code : new String[] {"1009", "1099"}) {
                transport.reply = JsonUtils.object("Code", code);
                assertEquals(QuoteForwarder.Outcome.RETRY, forwarder.tick());
                assertEquals(head, inbox.next());
            }
            transport.reply = JsonUtils.object("Code", "1001", "error",
                    JsonUtils.object("type", "VALIDATION_FAILED", "field", "price"));
            assertEquals(QuoteForwarder.Outcome.COMPLETED, forwarder.tick());
            assertNotEquals(head, inbox.next());
            assertTrue(Files.exists(directory.resolve("rejected").resolve(head.getFileName())));
        }
    }

    @Test
    void offlineOwnerFailureAndMalformedRepliesRetainHead() throws Exception {
        Transport transport = new Transport();
        try (DurableInbox inbox = new DurableInbox(directory, 10)) {
            inbox.enqueue(request("FIRST"));
            inbox.enqueue(request("SECOND"));
            Path head = inbox.next();
            transport.online = false;
            assertEquals(QuoteForwarder.Outcome.RETRY, forwarder(inbox, transport).tick());
            assertTrue(transport.requests.isEmpty());
            transport.online = true;
            QuoteForwarder missingOwner = new QuoteForwarder(inbox, transport, "", account -> {
                throw new IllegalStateException("Missing mapping");
            });
            assertThrows(IllegalStateException.class, missingOwner::tick);
            assertTrue(transport.requests.isEmpty());
            transport.reply = JsonUtils.object("Code", "0000");
            assertThrows(IllegalStateException.class, forwarder(inbox, transport)::tick);
            transport.reply = success();
            transport.reply.put("requestId", "WRONG");
            assertThrows(IllegalStateException.class, forwarder(inbox, transport)::tick);
            assertEquals(head, inbox.next());
            assertEquals(2, inbox.pendingCount());
        }
    }

    @Test
    void legacyFilesReceivePersistentOrderBeforeForwarding() throws Exception {
        Files.createDirectories(directory.resolve("pending"));
        Path first = directory.resolve("pending/legacy-first.json");
        Path second = directory.resolve("pending/legacy-second.json");
        Files.write(first, JsonUtils.toJson(JsonUtils.object("request", request("FIRST")))
                .getBytes(java.nio.charset.StandardCharsets.UTF_8));
        Files.write(second, JsonUtils.toJson(JsonUtils.object("request", request("SECOND")))
                .getBytes(java.nio.charset.StandardCharsets.UTF_8));
        Files.setLastModifiedTime(first, FileTime.fromMillis(1000));
        Files.setLastModifiedTime(second, FileTime.fromMillis(2000));
        try (DurableInbox inbox = new DurableInbox(directory, 10)) {
            assertEquals(first, inbox.next());
            inbox.saveRequest(first, inbox.request(first));
        }
        try (DurableInbox inbox = new DurableInbox(directory, 10)) {
            assertEquals(first, inbox.next());
        }
    }

    @Test
    void idleWorkerWakesAndDrainsTwoRequestsWithoutOneSecondPolling() throws Exception {
        Transport transport = new Transport();
        transport.received = new CountDownLatch(2);
        try (DurableInbox inbox = new DurableInbox(directory, 10)) {
            Thread worker = new Thread(() -> forwarder(inbox, transport).run(), "test-forward");
            worker.setDaemon(true);
            worker.start();
            try {
                inbox.enqueue(request("FIRST"));
                inbox.enqueue(request("SECOND"));
                // Tiny functional scheduler check, not a throughput benchmark.
                assertTrue(transport.received.await(900, TimeUnit.MILLISECONDS),
                        "Two requests must not incur a one-second polling delay");
            } finally {
                worker.interrupt();
                worker.join(2000);
                assertFalse(worker.isAlive());
            }
        }
    }

    @Test
    void connectionWarningLogsOnlyOnTransitionsAndRetryContinues() throws Exception {
        java.util.List<String> messages = new java.util.ArrayList<>();
        org.apache.log4j.Logger logger = org.apache.log4j.Logger.getLogger(QuoteForwarder.class);
        org.apache.log4j.AppenderSkeleton appender = new org.apache.log4j.AppenderSkeleton() {
            protected void append(org.apache.log4j.spi.LoggingEvent event) {
                messages.add(event.getRenderedMessage());
            }
            public void close() {}
            public boolean requiresLayout() { return false; }
        };
        org.apache.log4j.Level previousLevel = logger.getLevel();
        logger.setLevel(org.apache.log4j.Level.INFO);
        logger.addAppender(appender);
        try (DurableInbox inbox = new DurableInbox(directory, 10)) {
            inbox.enqueue(request("FIRST"));
            inbox.enqueue(request("SECOND"));
            Transport transport = new Transport();
            QuoteForwarder worker = forwarder(inbox, transport);
            transport.online = false;
            for (int attempt = 0; attempt < 5; attempt++) {
                assertEquals(QuoteForwarder.Outcome.RETRY, worker.tick());
            }
            assertEquals(1, messages.stream().filter(message -> message.contains("CLIENT_NOT_CONNECTED")).count());
            assertEquals(2, inbox.pendingCount());
            transport.online = true;
            assertEquals(QuoteForwarder.Outcome.COMPLETED, worker.tick());
            assertEquals(1, messages.stream().filter(message -> message.contains("state=CONNECTED")).count());
            transport.online = false;
            assertEquals(QuoteForwarder.Outcome.RETRY, worker.tick());
            assertEquals(2, messages.stream().filter(message -> message.contains("CLIENT_NOT_CONNECTED")).count());
        } finally {
            logger.removeAppender(appender);
            logger.setLevel(previousLevel);
        }
    }

    @Test
    void closingInboxWakesIdleWorker() throws Exception {
        DurableInbox inbox = new DurableInbox(directory, 10);
        Thread worker = new Thread(() -> forwarder(inbox, new Transport()).run());
        worker.setDaemon(true);
        try {
            worker.start();
            inbox.close();
            worker.join(2000);
            assertFalse(worker.isAlive());
        } finally {
            worker.interrupt();
            inbox.close();
        }
    }
}
