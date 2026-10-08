package cgws.ficc.framework.dps;

import static org.junit.jupiter.api.Assertions.*;

import org.I0Itec.zkclient.ZkClient;
import org.I0Itec.zkclient.exception.ZkNodeExistsException;
import org.apache.zookeeper.data.Stat;
import org.apache.zookeeper.server.NIOServerCnxnFactory;
import org.apache.zookeeper.server.ZooKeeperServer;
import org.junit.jupiter.api.AfterEach;
import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.Test;
import org.junit.jupiter.api.Timeout;
import org.junit.jupiter.api.io.TempDir;

import java.net.InetSocketAddress;
import java.nio.file.Path;
import java.util.Collections;
import java.util.List;
import java.util.concurrent.CopyOnWriteArrayList;
import java.util.concurrent.atomic.AtomicInteger;
import java.util.concurrent.atomic.AtomicReference;
import java.util.function.BooleanSupplier;

@Timeout(20)
class ZookeeperDiscoveryTest {
    @TempDir Path directory;
    private ZooKeeperServer server;
    private NIOServerCnxnFactory factory;
    private ZkClient observer;
    private String hosts;
    private final List<Throwable> failures = new CopyOnWriteArrayList<>();

    @BeforeEach
    void startRegistry() throws Exception {
        server = new ZooKeeperServer(directory.toFile(), directory.toFile(), 200);
        factory = new NIOServerCnxnFactory();
        factory.configure(new InetSocketAddress("127.0.0.1", 0), 10);
        factory.startup(server);
        hosts = "127.0.0.1:" + factory.getLocalPort();
        observer = new ZkClient(hosts, 3000, 3000, new ZookeeperDiscovery.Utf8Serializer());
    }

    @AfterEach
    void stopRegistry() throws Exception {
        if (observer != null) {
            observer.close();
        }
        if (factory != null) {
            factory.shutdown();
        }
        if (server != null) {
            server.getTxnLogFactory().close();
        }
        assertTrue(failures.isEmpty(), failures.toString());
    }

    private ZookeeperDiscovery discovery() {
        return new ZookeeperDiscovery(hosts, 1500, 3000, null, null, failures::add);
    }

    @Test
    void registrationIsInvisibleUntilApplicationSignalsReady() throws Exception {
        AtomicReference<List<ServiceEndpoint>> endpoints = new AtomicReference<>();
        try (ZookeeperDiscovery discovery = discovery();
                AutoCloseable watch = discovery.watch("QuoteSvr", endpoints::set)) {
            ZookeeperDiscovery.Registration registration =
                    discovery.register(
                            "QuoteSvr",
                            new ServiceEndpoint("127.0.0.1", 28680, false),
                            false,
                            null);
            assertFalse(
                    ServiceEndpoint.decode(observer.readData(registration.getNodePath()))
                            .isReady());
            assertTrue(endpoints.get().isEmpty());
            registration.markReady();
            await(() -> endpoints.get().size() == 1);
            registration.close();
            await(() -> endpoints.get().isEmpty());
            assertThrows(IllegalStateException.class, registration::markReady);
        }
    }

    @Test
    void restoresExpiredSessionWithoutRestartingService() throws Exception {
        ServiceEndpoint endpoint = new ServiceEndpoint("127.0.0.1", 28680, true);
        try (ZookeeperDiscovery discovery = discovery()) {
            ZookeeperDiscovery.Registration registration =
                    discovery.register("QuoteSvr", endpoint, false, null);
            String original = registration.getNodePath();
            Stat stat = new Stat();
            assertEquals(endpoint.encode(), observer.readData(original, stat));
            server.closeSession(stat.getEphemeralOwner());
            factory.closeSession(stat.getEphemeralOwner());
            await(
                    () ->
                            !original.equals(registration.getNodePath())
                                    && observer.exists(registration.getNodePath()));
            assertEquals(
                    Collections.singletonList(
                            registration.getNodePath().substring("/MDTService/QuoteSvr/".length())),
                    observer.getChildren("/MDTService/QuoteSvr"));
            assertEquals(endpoint.encode(), observer.readData(registration.getNodePath()));
        }
        await(() -> observer.getChildren("/MDTService/QuoteSvr").isEmpty());
    }

    @Test
    void restoresDeletedActiveNode() throws Exception {
        try (ZookeeperDiscovery discovery = discovery()) {
            ZookeeperDiscovery.Registration registration =
                    discovery.register(
                            "CRMSvr", new ServiceEndpoint("127.0.0.1", 28690, true), false, null);
            String original = registration.getNodePath();
            observer.delete(original);
            await(() -> !original.equals(registration.getNodePath()));
            assertTrue(observer.exists(registration.getNodePath()));
        }
    }

    @Test
    void observesReadyTransitionRemovalAndInitiallyAbsentService() throws Exception {
        AtomicReference<List<ServiceEndpoint>> latest = new AtomicReference<>();
        try (ZookeeperDiscovery discovery = discovery();
                AutoCloseable watch = discovery.watch("ParserSvr", latest::set)) {
            assertEquals(Collections.emptyList(), latest.get());
            observer.createPersistent("/MDTService/ParserSvr", true);
            observer.createEphemeral(
                    "/MDTService/ParserSvr/ParserSvr", "Host:127.0.0.1,Port:28688,Stat:0");
            observer.writeData(
                    "/MDTService/ParserSvr/ParserSvr", "Host:127.0.0.1,Port:28688,Stat:1");
            await(() -> latest.get().size() == 1);
            observer.delete("/MDTService/ParserSvr/ParserSvr");
            await(() -> latest.get().isEmpty());
        }
    }

    @Test
    void standbyCannotStealClaimAndLosingLeaseFencesIt() throws Exception {
        AtomicInteger lost = new AtomicInteger();
        try (ZookeeperDiscovery first = discovery();
                ZookeeperDiscovery second = discovery()) {
            ZookeeperDiscovery.Registration registration =
                    first.register(
                            "NotificationSvr",
                            new ServiceEndpoint("127.0.0.1", 28681, true),
                            true,
                            lost::incrementAndGet);
            assertThrows(
                    ZkNodeExistsException.class,
                    () ->
                            second.register(
                                    "NotificationSvr",
                                    new ServiceEndpoint("127.0.0.1", 38681, true),
                                    true,
                                    () -> {}));
            assertTrue(observer.exists(registration.getNodePath()));
            observer.delete(registration.getNodePath());
            await(() -> lost.get() == 1);
            assertFalse(observer.exists(registration.getNodePath()));
        }
    }

    @Test
    void waitingStandbyTakesOverAfterPrimaryClosesAndPublishesOnlyWhenReady() throws Exception {
        AtomicReference<List<ServiceEndpoint>> endpoints = new AtomicReference<>();
        try (ZookeeperDiscovery primary = discovery();
                ZookeeperDiscovery backup = discovery();
                AutoCloseable watch = backup.watch("QuoteSvr", endpoints::set)) {
            ZookeeperDiscovery.Registration leader =
                    primary.standby(
                            "QuoteSvr", new ServiceEndpoint("127.0.0.1", 28680, false), () -> {});
            leader.awaitOwnership();
            leader.markReady();
            ZookeeperDiscovery.Registration follower =
                    backup.standby(
                            "QuoteSvr", new ServiceEndpoint("127.0.0.1", 38680, false), () -> {});
            assertNull(follower.getNodePath());
            assertThrows(IllegalStateException.class, follower::markReady);
            leader.close();
            await(() -> follower.getNodePath() != null);
            follower.awaitOwnership();
            await(() -> endpoints.get().isEmpty());
            follower.markReady();
            await(() -> endpoints.get().size() == 1 && endpoints.get().get(0).getPort() == 38680);
        }
    }

    @Test
    void multipleBackupsElectOneAndWaitingSurvivesItsOwnSessionExpiry() throws Exception {
        AtomicInteger primaryFenced = new AtomicInteger();
        try (ZookeeperDiscovery primary = discovery();
                ZookeeperDiscovery backup = discovery();
                ZookeeperDiscovery third = discovery()) {
            ZookeeperDiscovery.Registration leader =
                    primary.standby(
                            "QuoteSvr",
                            new ServiceEndpoint("127.0.0.1", 28680, false),
                            primaryFenced::incrementAndGet);
            ZookeeperDiscovery.Registration second =
                    backup.standby(
                            "QuoteSvr", new ServiceEndpoint("127.0.0.1", 38680, false), () -> {});
            ZookeeperDiscovery.Registration last =
                    third.standby(
                            "QuoteSvr", new ServiceEndpoint("127.0.0.1", 48680, false), () -> {});
            // A separate active registration exposes the waiting backup's session for expiry.
            ZookeeperDiscovery.Registration marker =
                    backup.register(
                            "SessionMarker",
                            new ServiceEndpoint("127.0.0.1", 1, false),
                            false,
                            null);
            String oldMarker = marker.getNodePath();
            Stat stat = new Stat();
            observer.readData(oldMarker, stat);
            server.closeSession(stat.getEphemeralOwner());
            factory.closeSession(stat.getEphemeralOwner());
            await(() -> !oldMarker.equals(marker.getNodePath()));
            assertNull(second.getNodePath());
            observer.readData(leader.getNodePath(), stat);
            server.closeSession(stat.getEphemeralOwner());
            factory.closeSession(stat.getEphemeralOwner());
            await(() -> primaryFenced.get() == 1);
            await(() -> second.getNodePath() != null || last.getNodePath() != null);
            assertNotEquals(second.getNodePath() != null, last.getNodePath() != null);
            assertEquals(1, observer.getChildren("/MDTService/QuoteSvr").size());
            assertThrows(IllegalStateException.class, leader::markReady);
            ZookeeperDiscovery.Registration winner = second.getNodePath() != null ? second : last;
            ZookeeperDiscovery.Registration remaining = winner == second ? last : second;
            winner.markReady();
            winner.close();
            await(() -> remaining.getNodePath() != null);
            remaining.markReady();
        }
    }

    @Test
    void closingWaitingStandbyUnblocksStartupAndCannotRemovePrimary() throws Exception {
        try (ZookeeperDiscovery primary = discovery();
                ZookeeperDiscovery backup = discovery()) {
            ZookeeperDiscovery.Registration leader =
                    primary.standby(
                            "QuoteSvr", new ServiceEndpoint("127.0.0.1", 28680, false), () -> {});
            ZookeeperDiscovery.Registration follower =
                    backup.standby(
                            "QuoteSvr", new ServiceEndpoint("127.0.0.1", 38680, false), () -> {});
            java.util.concurrent.CompletableFuture<Boolean> waiting =
                    java.util.concurrent.CompletableFuture.supplyAsync(
                            () -> {
                                try {
                                    follower.awaitOwnership();
                                    return false;
                                } catch (IllegalStateException expected) {
                                    return true;
                                } catch (InterruptedException error) {
                                    throw new RuntimeException(error);
                                }
                            });
            backup.close();
            assertTrue(waiting.get(2, java.util.concurrent.TimeUnit.SECONDS));
            assertTrue(observer.exists(leader.getNodePath()));
        }
    }

    private static void await(BooleanSupplier condition) throws InterruptedException {
        long deadline = System.nanoTime() + 8_000_000_000L;
        while (!condition.getAsBoolean() && System.nanoTime() < deadline) {
            Thread.sleep(20);
        }
        assertTrue(condition.getAsBoolean(), "Timed out waiting for registry transition");
    }
}
