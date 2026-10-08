package cgws.ficc.framework.compat;

import static org.junit.jupiter.api.Assertions.*;

import ats.bbox.framework.*;
import ats.bbox.framework.ConfigurationBuilder;
import ats.bbox.framework.inputstream.AbstractInputStream;
import ats.bbox.framework.outputstream.OutputStreamAdapter;

import org.apache.commons.configuration.*;
import org.apache.log4j.*;
import org.junit.jupiter.api.*;
import org.junit.jupiter.api.io.TempDir;

import java.nio.charset.StandardCharsets;
import java.nio.file.Files;
import java.nio.file.Path;
import java.util.*;

class FrameworkCompatibilityTest {
    @TempDir Path directory;

    @BeforeEach
    void reset() {
        LifecycleFixtures.reset();
    }

    Path file(String name, String content) throws Exception {
        Path result = directory.resolve(name);
        Files.write(result, content.getBytes(StandardCharsets.ISO_8859_1));
        return result;
    }

    String component(String name, String type, String extra) {
        return "<component name=\""
                + name
                + "\" config-type=\"this\" "
                + extra
                + ">"
                + LifecycleFixtures.class.getName()
                + "$"
                + type
                + "</component>";
    }

    Configuration configuration(String extra) throws Exception {
        PropertiesConfiguration configuration = new PropertiesConfiguration();
        configuration.setProperty(
                "resource.file",
                file(
                                "resource.xml",
                                "<resource>"
                                        + component("first", "Resource", "")
                                        + component("second", "Resource", extra)
                                        + "</resource>")
                        .toString());
        configuration.setProperty(
                "streams.file",
                file(
                                "streams.xml",
                                "<stream-engine version=\"1.0\">"
                                        + "<input-stream>"
                                        + component("input", "Input", "")
                                        + "<output>middle</output></input-stream>"
                                        + "<stream>"
                                        + component("middle", "Stream", "")
                                        + "<output>sink</output></stream>"
                                        + "<output-stream>"
                                        + component("sink", "Output", "")
                                        + "</output-stream></stream-engine>")
                        .toString());
        return configuration;
    }

    @Test
    void lifecycleOrderAndSynchronousDelivery() throws Exception {
        ServerImpl server = new ServerImpl(configuration(""));
        try {
            server.init();
            server.start();
            assertEquals(
                    Arrays.asList(
                            "first:init",
                            "first:start",
                            "second:init",
                            "second:start",
                            "sink:init",
                            "middle:init",
                            "input:init",
                            "sink:start",
                            "middle:start",
                            "input:start"),
                    LifecycleFixtures.EVENTS);
            Map<String, Object> message = new HashMap<>();
            message.put("sequence", 1);
            assertNull(((AbstractInputStream) server.getStream("input")).send(message));
            assertSame(message, LifecycleFixtures.received);
            assertSame(Thread.currentThread(), LifecycleFixtures.deliveryThread);
        } finally {
            server.stop();
        }
        assertEquals(
                Arrays.asList(
                        "input:stop",
                        "middle:stop",
                        "sink:stop",
                        "second:stop",
                        "first:stop",
                        "input:dispose",
                        "middle:dispose",
                        "sink:dispose",
                        "second:dispose",
                        "first:dispose"),
                LifecycleFixtures.EVENTS.subList(10, 20));
        server.stop();
        assertEquals(20, LifecycleFixtures.EVENTS.size());
    }

    @Test
    void failedInitCleansPartialResourceAndPreviouslyStartedResources() throws Exception {
        ServerImpl server = new ServerImpl(configuration("failInit=\"true\""));
        LifecycleException error = assertThrows(LifecycleException.class, server::init);
        assertTrue(error.getCause().getMessage().contains("injected"));
        assertEquals(
                Arrays.asList(
                        "first:init",
                        "first:start",
                        "second:init",
                        "second:stop",
                        "first:stop",
                        "second:dispose",
                        "first:dispose"),
                LifecycleFixtures.EVENTS);
        assertEquals(ILifecycle.STATUS.DISPOSED, server.getStatus());
    }

    @Test
    void failedStartCleansUpAndPreservesStopFailure() throws Exception {
        ServerImpl server = new ServerImpl(configuration("failStart=\"true\" failStop=\"true\""));
        LifecycleException error = assertThrows(LifecycleException.class, server::init);
        assertEquals("injected startup failure", error.getCause().getMessage());
        assertEquals(1, error.getCause().getSuppressed().length);
        assertTrue(LifecycleFixtures.EVENTS.contains("first:dispose"));
    }

    @Test
    void stopFailureDoesNotPreventOtherCleanup() throws Exception {
        ServerImpl server = new ServerImpl(configuration("failStop=\"true\""));
        server.init();
        server.start();
        assertThrows(LifecycleException.class, server::stop);
        assertTrue(LifecycleFixtures.EVENTS.contains("first:dispose"));
        assertEquals(ILifecycle.STATUS.DISPOSED, server.getStatus());
    }

    @Test
    void inputCanDeliverBeforeStartAndCollectCheckedErrors() throws Exception {
        LifecycleFixtures.Input source = new LifecycleFixtures.Input();
        OutputStreamAdapter failing =
                new OutputStreamAdapter() {
                    public void onData(Map<String, Object> data) throws EDataException {
                        throw new EDataException("delivery failed");
                    }
                };
        source.addOutputStream(failing);
        source.addOutputStream(new LifecycleFixtures.Output());
        Map<String, Object> payload = new HashMap<>();
        assertEquals(1, source.send(payload).size());
        assertSame(payload, LifecycleFixtures.received);
    }

    @Test
    void invalidOutputAndDuplicateNamesFailBeforeStreamStart() throws Exception {
        Path invalid =
                file(
                        "invalid.xml",
                        "<stream-engine><stream>"
                                + component("one", "Stream", "")
                                + "<output>missing</output></stream></stream-engine>");
        assertThrows(EException.class, () -> new StreamEngine().load(invalid.toString()));
        Path duplicate =
                file(
                        "duplicate.xml",
                        "<resource>"
                                + component("same", "Resource", "")
                                + component("same", "Resource", "")
                                + "</resource>");
        assertThrows(
                EException.class,
                () -> ComponentBuilder.getComponents(duplicate.toString(), null, null));
    }

    @Test
    void compositionIncludesInterpolationAndEscapedComma() throws Exception {
        file("included.ini", "included=present\n");
        Path properties =
                file("values.ini", "include=included.ini\nname=base\ncopy=${name}\nlist=a,b\\,c\n");
        Map<String, String> inline = new LinkedHashMap<>();
        inline.put("name", "inline");
        AbstractConfiguration configuration =
                ConfigurationBuilder.getConfiguration(
                        new String[] {"this", "ini"},
                        new String[] {"null", properties.toString()},
                        inline);
        assertEquals("inline", configuration.getString("name"));
        assertEquals("inline", configuration.getString("copy"));
        assertEquals("present", configuration.getString("included"));
        assertArrayEquals(new String[] {"a", "b,c"}, configuration.getStringArray("list"));
        assertThrows(
                EException.class,
                () ->
                        ConfigurationBuilder.getConfiguration(
                                new String[] {"this", "ini"}, new String[0], inline));
    }

    @Test
    void existingDeferredStartModeAndGroupSyntax() throws Exception {
        Configuration configuration = configuration("");
        configuration.setProperty("unstart.mode", "custom");
        configuration.setProperty("unstart.list", "controller={input,middle},sink");
        ServerImpl server = new ServerImpl(configuration);
        try {
            server.init();
            server.start();
            assertArrayEquals(
                    new String[] {"input", "middle"}, server.getUnstartList("controller"));
            assertArrayEquals(new String[] {"sink"}, server.getUnstartList("other"));
            assertFalse(LifecycleFixtures.EVENTS.contains("middle:start"));
        } finally {
            server.stop();
        }
    }

    @Test
    void databaseResourceKeepsLifecycleOwnershipAndFailsBeforeInitialization() throws Exception {
        ats.bbox.framework.resource.dbpool.DBPoolResource resource =
                new ats.bbox.framework.resource.dbpool.DBPoolResource();
        assertThrows(java.sql.SQLException.class, resource::getConnection);
        java.util.concurrent.atomic.AtomicInteger closes =
                new java.util.concurrent.atomic.AtomicInteger();
        ats.foundation.utils.db.IDBPool pool =
                (ats.foundation.utils.db.IDBPool)
                        java.lang.reflect.Proxy.newProxyInstance(
                                getClass().getClassLoader(),
                                new Class<?>[] {ats.foundation.utils.db.IDBPool.class},
                                (proxy, method, arguments) -> {
                                    if ("closeAll".equals(method.getName())) {
                                        closes.incrementAndGet();
                                    }
                                    return null;
                                });
        java.lang.reflect.Field field = resource.getClass().getDeclaredField("pool");
        field.setAccessible(true);
        field.set(resource, pool);
        resource.closeAll();
        assertEquals(0, closes.get());
        resource.dispose();
        resource.dispose();
        assertEquals(1, closes.get());
    }

    @Test
    void sectionConfigurationAndCustomProvider() throws Exception {
        Path sections = file("sections.ini", "[one]\na=first\n[two]\na=second\n[one]\na=third\n");
        assertEquals(
                "first",
                NINIConfigurationHelper.getPropertiesConfiguration("one", sections.toString())
                        .getString("a"));
        assertEquals(
                "second",
                NINIConfigurationHelper.getPropertiesConfiguration("two", sections.toString())
                        .getString("a"));
        Map<String, String> parameter = Collections.singletonMap("hello", "world");
        assertEquals(
                "world",
                ConfigurationBuilder.getConfiguration(
                                new String[] {Provider.class.getName()}, new String[0], parameter)
                        .getString("hello"));
    }

    public static class Provider extends AbstractConfigProvider {
        public AbstractConfiguration getConfiguration() {
            return new MapConfiguration(new HashMap<String, Object>(getParameter()));
        }
    }

    @Test
    void initializationFailureIsVisibleInConsoleAndFileWithStackTrace() throws Exception {
        Logger logger = Logger.getLogger("ficc.startup");
        Level oldLevel = logger.getLevel();
        logger.setLevel(Level.INFO);
        java.io.ByteArrayOutputStream console = new java.io.ByteArrayOutputStream();
        java.io.PrintStream originalOutput = System.out;
        java.io.PrintStream capturedOutput = new java.io.PrintStream(console, true, "UTF-8");
        System.setOut(capturedOutput);
        ConsoleAppender consoleAppender = new ConsoleAppender(new PatternLayout("%p %m%n"));
        Path log = directory.resolve("startup.log");
        FileAppender fileAppender = new FileAppender(new PatternLayout("%p %m%n"), log.toString());
        logger.addAppender(consoleAppender);
        logger.addAppender(fileAppender);
        try {
            ServerImpl server = new ServerImpl(configuration("failInit=\"true\""));
            assertThrows(LifecycleException.class, server::init);
        } finally {
            logger.removeAppender(consoleAppender);
            logger.removeAppender(fileAppender);
            fileAppender.close();
            logger.setLevel(oldLevel);
            System.setOut(originalOutput);
            capturedOutput.close();
        }
        for (String output :
                Arrays.asList(
                        console.toString("UTF-8"),
                        new String(Files.readAllBytes(log), StandardCharsets.UTF_8))) {
            assertTrue(output.contains("INFO COMPONENT"));
            assertTrue(output.contains("ERROR COMPONENT"));
            assertTrue(output.contains("\"state\":\"FAILED\""));
            assertTrue(output.contains("\"elapsedMillis\":"));
            assertTrue(output.contains("injected initialization failure"));
            assertTrue(output.contains("at "));
        }
    }
}
