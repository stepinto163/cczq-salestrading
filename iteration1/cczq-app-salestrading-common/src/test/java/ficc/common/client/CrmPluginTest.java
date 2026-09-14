package ficc.common.client;

import org.junit.jupiter.api.Test;
import org.junit.jupiter.api.io.TempDir;
import javax.tools.ToolProvider;
import java.nio.file.Files;
import java.nio.file.Path;
import java.util.Properties;
import java.util.jar.JarEntry;
import java.util.jar.JarOutputStream;
import static org.junit.jupiter.api.Assertions.*;

final class CrmPluginTest {
    @TempDir
    Path directory;

    @Test
    void loadsAnIndependentAdapterJarAndPassesCurrentUser() throws Exception {
        Path source = directory.resolve("QaCrmClient.java");
        Files.writeString(source, """
                package qa;
                import ficc.common.client.CrmClient;
                import com.alibaba.fastjson2.JSONObject;
                import java.util.List;
                public class QaCrmClient implements CrmClient {
                    public List<JSONObject> institutions(String userId) {
                        JSONObject value = new JSONObject();
                        value.put("institutionId", userId);
                        value.put("institutionName", "Internal CRM");
                        return List.of(value);
                    }
                    public List<JSONObject> contacts(String userId) { return List.of(); }
                }
                """);
        int result = ToolProvider.getSystemJavaCompiler().run(null, null, null,
                "-proc:none", "-classpath", System.getProperty("java.class.path"),
                "-d", directory.toString(), source.toString());
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
            assertEquals("STC-42", client.institution("STC-42", "STC-42").getString("institutionId"));
            assertNull(client.institution("STC-42", "STC-43"));
            assertTrue(client.contacts("STC-42").isEmpty());
        }
        configuration.setProperty("crm.adapterClass", "qa.DoesNotExist");
        assertThrows(ClassNotFoundException.class, () -> AdapterFactory.crm(configuration, null));
    }
}
