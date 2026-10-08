package cgws.ficc.framework.dps;

import static org.junit.jupiter.api.Assertions.*;

import org.junit.jupiter.api.Test;
import org.junit.jupiter.api.Timeout;
import org.junit.jupiter.api.condition.EnabledIfSystemProperty;

import java.net.ServerSocket;
import java.util.List;
import java.util.concurrent.BlockingQueue;
import java.util.concurrent.CopyOnWriteArrayList;
import java.util.concurrent.ExecutorService;
import java.util.concurrent.Executors;
import java.util.concurrent.LinkedBlockingQueue;
import java.util.concurrent.TimeUnit;

@EnabledIfSystemProperty(named = "dps.licensePath", matches = ".+")
@Timeout(20)
class DpsTransportTest {
    @Test
    void occupiedPortPropagatesStartupFailure() throws Exception {
        try (ServerSocket occupied = new ServerSocket(0);
                DpsServer server = new DpsServer(
                        (session, topic, content) -> DpsServer.Reply.text("{}"), ignored -> {})) {
            ServiceEndpoint endpoint = new ServiceEndpoint("127.0.0.1", occupied.getLocalPort(), true);
            assertThrows(Exception.class,
                    () -> server.start(endpoint, System.getProperty("dps.licensePath")));
        }
    }

    @Test
    void offlineUnsubscribeDoesNotReappearAfterReconnect() throws Exception {
        ExecutorService callbacks = Executors.newSingleThreadExecutor();
        BlockingQueue<Boolean> connections = new LinkedBlockingQueue<>();
        BlockingQueue<String> messages = new LinkedBlockingQueue<>();
        List<Throwable> failures = new CopyOnWriteArrayList<>();
        int port;
        try (ServerSocket available = new ServerSocket(0)) {
            port = available.getLocalPort();
        }
        ServiceEndpoint endpoint = new ServiceEndpoint("127.0.0.1", port, true);
        DpsServer original =
                new DpsServer(
                        (session, topic, content) -> DpsServer.Reply.text("{}"), ignored -> {});
        try (DpsPeer peer =
                new DpsPeer("SERVER.FrameworkTest", callbacks, connections::add, failures::add)) {
            original.start(endpoint, System.getProperty("dps.licensePath"));
            peer.connect(endpoint, "FrameworkTest", "FrameworkTest");
            assertEquals(Boolean.TRUE, connections.poll(3, TimeUnit.SECONDS));
            com.gateway.connector.tcp.client.IMessage<String> callback =
                    (topic, content) -> messages.add(content);
            peer.subscribe("framework.removed", null, callback);
            original.close();
            assertEquals(Boolean.FALSE, connections.poll(3, TimeUnit.SECONDS));
            peer.unsubscribe("framework.removed", null, callback);
            try (DpsServer replacement =
                    new DpsServer(
                            (session, topic, content) -> DpsServer.Reply.text("{}"),
                            ignored -> {})) {
                replacement.start(endpoint, System.getProperty("dps.licensePath"));
                assertEquals(Boolean.TRUE, connections.poll(8, TimeUnit.SECONDS));
                peer.subscribe("framework.kept", null, callback);
                replacement.notify("framework.kept", "subscribed");
                assertEquals("subscribed", messages.poll(3, TimeUnit.SECONDS));
                replacement.notify("framework.removed", "must not arrive");
                assertNull(messages.poll(300, TimeUnit.MILLISECONDS));
            }
            assertTrue(failures.isEmpty(), failures.toString());
        } finally {
            original.close();
            callbacks.shutdownNow();
            assertTrue(callbacks.awaitTermination(3, TimeUnit.SECONDS));
        }
    }

    @Test
    void restoresSubscriptionAfterServerRestart() throws Exception {
        ExecutorService callbacks = Executors.newSingleThreadExecutor();
        BlockingQueue<Boolean> connections = new LinkedBlockingQueue<>();
        BlockingQueue<String> messages = new LinkedBlockingQueue<>();
        List<Throwable> failures = new CopyOnWriteArrayList<>();
        int port;
        try (ServerSocket available = new ServerSocket(0)) {
            port = available.getLocalPort();
        }
        ServiceEndpoint endpoint = new ServiceEndpoint("127.0.0.1", port, true);
        DpsServer original =
                new DpsServer(
                        (session, topic, content) -> DpsServer.Reply.text("{}"), ignored -> {});
        try (DpsPeer peer =
                new DpsPeer("SERVER.FrameworkTest", callbacks, connections::add, failures::add)) {
            original.start(endpoint, System.getProperty("dps.licensePath"));
            peer.connect(endpoint, "FrameworkTest", "FrameworkTest");
            assertEquals(Boolean.TRUE, connections.poll(3, TimeUnit.SECONDS));
            peer.subscribe("framework.reconnect", null, (topic, content) -> messages.add(content));
            original.notify("framework.reconnect", "before restart");
            assertEquals("before restart", messages.poll(3, TimeUnit.SECONDS));
            original.close();
            assertEquals(Boolean.FALSE, connections.poll(3, TimeUnit.SECONDS));
            try (DpsServer replacement =
                    new DpsServer(
                            (session, topic, content) -> DpsServer.Reply.text("{}"),
                            ignored -> {})) {
                replacement.start(endpoint, System.getProperty("dps.licensePath"));
                assertEquals(Boolean.TRUE, connections.poll(8, TimeUnit.SECONDS));
                // Login readiness and the SDK's asynchronous subscription restoration differ.
                long deadline = System.nanoTime() + TimeUnit.SECONDS.toNanos(3);
                boolean subscribed = false;
                while (!subscribed && System.nanoTime() < deadline) {
                    subscribed =
                            !replacement.notify("framework.reconnect", "after restart").isEmpty();
                    if (!subscribed) {
                        Thread.sleep(20);
                    }
                }
                assertTrue(subscribed, "SDK did not restore the subscription");
                assertEquals("after restart", messages.poll(3, TimeUnit.SECONDS));
            }
            assertTrue(failures.isEmpty(), failures.toString());
        } finally {
            original.close();
            callbacks.shutdownNow();
            assertTrue(callbacks.awaitTermination(3, TimeUnit.SECONDS));
        }
    }

    @Test
    void realSdkLoginRequestImagePushAndUnsubscribe() throws Exception {
        ExecutorService callbacks = Executors.newSingleThreadExecutor();
        BlockingQueue<String> messages = new LinkedBlockingQueue<>();
        BlockingQueue<Boolean> connections = new LinkedBlockingQueue<>();
        List<Throwable> failures = new CopyOnWriteArrayList<>();
        int port;
        try (ServerSocket available = new ServerSocket(0)) {
            port = available.getLocalPort();
        }
        ServiceEndpoint endpoint = new ServiceEndpoint("127.0.0.1", port, true);
        try (DpsServer server =
                        new DpsServer(
                                (session, topic, content) -> {
                                    if (content.isEmpty()) {
                                        // DPS image replies map each published topic to its
                                        // serialized snapshot.
                                        java.util.Map<String, Object> image =
                                                new java.util.HashMap<>();
                                        image.put(topic, "{\"value\":\"snapshot\"}");
                                        return new DpsServer.Reply(
                                                com.gateway.connector.utils.JsonUtils
                                                        .SerializerBytes(image),
                                                image);
                                    }
                                    return DpsServer.Reply.text("{\"value\":\"报价\"}");
                                },
                                ignored -> {});
                DpsPeer peer =
                        new DpsPeer(
                                "SERVER.FrameworkTest",
                                callbacks,
                                connections::add,
                                failures::add)) {
            server.start(endpoint, System.getProperty("dps.licensePath"));
            peer.connect(endpoint, "FrameworkTest", "FrameworkTest");
            assertEquals(Boolean.TRUE, connections.poll(5, TimeUnit.SECONDS));
            assertEquals(
                    "{\"value\":\"报价\"}",
                    peer.request("test-session", "framework.echo", "{}", 3000));
            com.gateway.connector.tcp.client.IMessage<String> callback =
                    (topic, content) -> messages.add(content);
            peer.subscribe("framework.push", "FrameworkTest", callback);
            assertEquals("{\"value\":\"snapshot\"}", messages.poll(3, TimeUnit.SECONDS));
            server.notify("framework.push", "{\"value\":\"live\"}");
            assertEquals("{\"value\":\"live\"}", messages.poll(3, TimeUnit.SECONDS));
            peer.unsubscribe("framework.push", "FrameworkTest", callback);
            server.notify("framework.push", "{\"value\":\"after unsubscribe\"}");
            assertNull(messages.poll(250, TimeUnit.MILLISECONDS));
            assertTrue(failures.isEmpty(), failures.toString());
        } finally {
            callbacks.shutdownNow();
            assertTrue(callbacks.awaitTermination(3, TimeUnit.SECONDS));
        }
    }
}
