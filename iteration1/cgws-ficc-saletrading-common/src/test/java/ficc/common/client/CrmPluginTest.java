package ficc.common.client;

import static org.junit.jupiter.api.Assertions.*;

import org.junit.jupiter.api.Test;
import org.junit.jupiter.api.io.TempDir;

import java.nio.charset.StandardCharsets;
import java.nio.file.Files;
import java.nio.file.Path;
import java.util.Properties;
import java.util.jar.JarEntry;
import java.util.jar.JarOutputStream;

import javax.tools.ToolProvider;

final class CrmPluginTest {
    @TempDir Path directory;

    @Test
    void loadsAnIndependentAdapterJarAndPassesCurrentUser() throws Exception {
        Path source = directory.resolve("QaCrmClient.java");
        Files.write(
                source,
                ("package qa;\n"
                                + "import ficc.common.client.CrmClient;\n"
                                + "import com.alibaba.fastjson2.JSONObject;\n"
                                + "import java.util.List;\n"
                                + "public class QaCrmClient implements CrmClient {\n"
                                + "    public List<JSONObject> institutions(String userId) {\n"
                                + "        JSONObject value = new JSONObject();\n"
                                + "        value.put(\"institutionId\", userId);\n"
                                + "        value.put(\"institutionName\", \"Internal CRM\");\n"
                                + "        return java.util.Collections.singletonList(value);\n"
                                + "    }\n"
                                + "    public List<JSONObject> contacts(String userId) { return"
                                + " java.util.Collections.emptyList(); }\n"
                                + "}\n")
                        .getBytes(StandardCharsets.UTF_8));
        int result =
                ToolProvider.getSystemJavaCompiler()
                        .run(
                                null,
                                null,
                                null,
                                "-proc:none",
                                "-classpath",
                                System.getProperty("java.class.path"),
                                "-d",
                                directory.toString(),
                                source.toString());
        assertEquals(0, result);
        Path jar = directory.resolve("qa-crm.jar");
        try (JarOutputStream output = new JarOutputStream(Files.newOutputStream(jar))) {
            output.putNextEntry(new JarEntry("qa/QaCrmClient.class"));
            output.write(Files.readAllBytes(directory.resolve("qa/QaCrmClient.class")));
            output.closeEntry();
        }
        Properties configuration = new Properties();
        configuration.setProperty("crm.mode", "custom");
        configuration.setProperty("crm.adapterClass", "qa.QaCrmClient");
        configuration.setProperty("crm.adapterJar", jar.toString());
        try (CrmClient client = AdapterFactory.crm(configuration, null)) {
            assertEquals(
                    "STC-42", client.institution("STC-42", "STC-42").getString("institutionId"));
            assertNull(client.institution("STC-42", "STC-43"));
            assertTrue(client.contacts("STC-42").isEmpty());
        }
        configuration.setProperty("crm.adapterClass", "qa.DoesNotExist");
        assertThrows(ClassNotFoundException.class, () -> AdapterFactory.crm(configuration, null));
    }
}
