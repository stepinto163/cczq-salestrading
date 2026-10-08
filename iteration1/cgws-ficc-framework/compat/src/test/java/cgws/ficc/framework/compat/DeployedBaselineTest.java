package cgws.ficc.framework.compat;

import static org.junit.jupiter.api.Assertions.*;

import ats.bbox.framework.ConfigurationBuilder;
import ats.bbox.framework.ServerImpl;

import org.apache.commons.configuration.*;
import org.junit.jupiter.api.*;
import org.junit.jupiter.api.io.TempDir;

import java.lang.reflect.Method;
import java.net.URL;
import java.net.URLClassLoader;
import java.nio.charset.StandardCharsets;
import java.nio.file.*;
import java.util.*;

/** Differential checks against the deployed binary, in a separate class loader (no network). */
class DeployedBaselineTest {
    @TempDir Path directory;
    private URLClassLoader legacy;

    @BeforeEach
    void loadBaseline() throws Exception {
        String folder = System.getProperty("legacy.framework.directory");
        Assumptions.assumeTrue(
                folder != null, "Supply -Dlegacy.framework.directory=<deployed javalib>");
        List<URL> urls = new ArrayList<>();
        try (DirectoryStream<Path> jars = Files.newDirectoryStream(Paths.get(folder), "*.jar")) {
            for (Path jar : jars) {
                urls.add(jar.toUri().toURL());
            }
        }
        urls.add(LifecycleFixtures.class.getProtectionDomain().getCodeSource().getLocation());
        legacy =
                new URLClassLoader(
                        urls.toArray(new URL[0]), ClassLoader.getSystemClassLoader().getParent());
        legacy.loadClass("ats.bbox.framework.Consts")
                .getField("isShowActiveThread")
                .set(null, false);
        assertTrue(
                legacy.loadClass("ats.bbox.framework.ServerImpl")
                        .getProtectionDomain()
                        .getCodeSource()
                        .getLocation()
                        .toString()
                        .contains("ats-bbox-framework-1.2.3"));
    }

    @AfterEach
    void closeBaseline() throws Exception {
        if (legacy != null) {
            legacy.close();
        }
    }

    @Test
    void composedConfigurationMatchesDeployedBinary() throws Exception {
        Path ini = directory.resolve("values.ini");
        Files.write(
                ini,
                "name=file\ncopy=${name}\nlist=a,b\\,c\nflag=true\n"
                        .getBytes(StandardCharsets.UTF_8));
        String[] types = {"this", "ini"};
        String[] files = {"null", ini.toString()};
        Map<String, String> parameters = Collections.singletonMap("name", "inline");
        Object expected =
                legacy.loadClass("ats.bbox.framework.ConfigurationBuilder")
                        .getMethod("getConfiguration", String[].class, String[].class, Map.class)
                        .invoke(null, types, files, parameters);
        AbstractConfiguration actual =
                ConfigurationBuilder.getConfiguration(types, files, parameters);
        for (String key : Arrays.asList("name", "copy", "flag")) {
            assertEquals(
                    expected.getClass().getMethod("getString", String.class).invoke(expected, key),
                    actual.getString(key));
        }
        assertArrayEquals(
                (String[])
                        expected.getClass()
                                .getMethod("getStringArray", String.class)
                                .invoke(expected, "list"),
                actual.getStringArray("list"));
    }

    @Test
    void lifecycleOrderMatchesDeployedBinary() throws Exception {
        FrameworkCompatibilityTest fixtures = new FrameworkCompatibilityTest();
        fixtures.directory = directory;
        Configuration configuration = fixtures.configuration("");
        List<String> expected = runLegacy(configuration);
        LifecycleFixtures.reset();
        ServerImpl server = new ServerImpl(configuration);
        try {
            server.init();
            server.start();
        } finally {
            server.stop();
        }
        assertEquals(expected, LifecycleFixtures.EVENTS);
    }

    private List<String> runLegacy(Configuration configuration) throws Exception {
        Class<?> configurationType =
                legacy.loadClass("org.apache.commons.configuration.Configuration");
        Object properties =
                legacy.loadClass("org.apache.commons.configuration.BaseConfiguration")
                        .getDeclaredConstructor()
                        .newInstance();
        Method setProperty = configurationType.getMethod("setProperty", String.class, Object.class);
        for (String key : Arrays.asList("resource.file", "streams.file")) {
            setProperty.invoke(properties, key, configuration.getString(key));
        }
        Class<?> serverType = legacy.loadClass("ats.bbox.framework.ServerImpl");
        Object server = serverType.getConstructor(configurationType).newInstance(properties);
        Class<?> fixture = legacy.loadClass(LifecycleFixtures.class.getName());
        fixture.getMethod("reset").invoke(null);
        try {
            serverType.getMethod("init").invoke(server);
            serverType.getMethod("start").invoke(server);
        } finally {
            serverType.getMethod("stop").invoke(server);
        }
        return new ArrayList<>((List<String>) fixture.getField("EVENTS").get(null));
    }

    @Test
    void exposedApplicationApisKeepTheirDescriptors() throws Exception {
        for (String name :
                Arrays.asList(
                        "IComponent",
                        "ILifecycle",
                        "IServer",
                        "SimpleComponent",
                        "resource.ResourceAdapter",
                        "stream.StreamAdapter",
                        "inputstream.AbstractInputStream",
                        "ServerShell",
                        "ConfigurationBuilder",
                        "ComponentBuilder")) {
            Class<?> baseline = legacy.loadClass("ats.bbox.framework." + name);
            Class<?> replacement = Class.forName("ats.bbox.framework." + name);
            Set<String> actual = new HashSet<>();
            for (Method method : replacement.getMethods()) {
                actual.add(descriptor(method));
            }
            for (Method method : baseline.getMethods()) {
                assertTrue(
                        actual.contains(descriptor(method)),
                        name + " missing " + descriptor(method));
            }
        }
    }

    private String descriptor(Method method) {
        List<String> parameters = new ArrayList<>();
        for (Class<?> type : method.getParameterTypes()) {
            parameters.add(type.getName());
        }
        return method.getName() + parameters + ":" + method.getReturnType().getName();
    }
}
