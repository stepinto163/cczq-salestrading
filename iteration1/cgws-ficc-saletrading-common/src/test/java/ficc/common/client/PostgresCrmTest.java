package ficc.common.client;

import static org.junit.jupiter.api.Assertions.*;
import static org.junit.jupiter.api.Assumptions.assumeTrue;

import com.alibaba.fastjson2.JSONObject;

import org.junit.jupiter.api.Test;

import java.io.BufferedReader;
import java.nio.file.Files;
import java.nio.file.Paths;
import java.util.Properties;

final class PostgresCrmTest {
    @Test
    void localPostgresSearchAndRelationships() throws Exception {
        String path = System.getProperty("crm.test.config");
        assumeTrue(path != null, "Supply crm.test.config to test the local PostgreSQL fixture");
        Properties configuration = new Properties();
        try (BufferedReader reader = Files.newBufferedReader(Paths.get(path))) {
            configuration.load(reader);
        }
        assertEquals(
                "jdbc:postgresql://127.0.0.1:55432/ficc_ibor",
                configuration.getProperty("crm.jdbcUrl"));
        try (CrmClient client = AdapterFactory.crm(configuration, null)) {
            JSONObject first = client.query("any-user", "INSTITUTION", "测试", null, 0, 1);
            assertEquals(1, first.getJSONArray("records").size());
            assertTrue(first.getBooleanValue("hasMore"));
            assertEquals(
                    "9000000000000000001",
                    first.getJSONArray("records").getJSONObject(0).getString("institutionId"));
            assertEquals(
                    "9000000000000000002",
                    client.query("any-user", "INSTITUTION", "测试", null, 1, 1)
                            .getJSONArray("records")
                            .getJSONObject(0)
                            .getString("institutionId"));
            assertEquals(
                    1,
                    client.query("any-user", "INSTITUTION", "csjyh", null, 0, 10)
                            .getJSONArray("records")
                            .size());
            assertTrue(
                    client.query("any-user", "INSTITUTION", "%' OR 1=1 --", null, 0, 10)
                            .getJSONArray("records")
                            .isEmpty());
            assertNull(client.institution("any-user", "9000000000000000003"));
            assertNull(client.contact("any-user", "c9000000000000000003"));
            JSONObject contact = client.contact("not-the-owner", "c9000000000000000001");
            assertTrue(CrmClient.belongsTo(contact, "9000000000000000001"));
            assertTrue(CrmClient.belongsTo(contact, "9000000000000000002"));
            assertFalse(CrmClient.belongsTo(contact, "9000000000000000003"));
            assertEquals(
                    1,
                    client.query("any-user", "CONTACT", "测试张明", "9000000000000000001", 0, 50)
                            .getJSONArray("records")
                            .size());
            assertEquals(
                    2,
                    client.query("any-user", "CONTACT", "测试张明", "9000000000000000002", 0, 50)
                            .getJSONArray("records")
                            .size());
            assertFalse(
                    CrmClient.belongsTo(
                            client.contact("any-user", "c9000000000000000004"),
                            "9000000000000000001"));
            assertFalse(
                    CrmClient.belongsTo(
                            client.contact("any-user", "c9000000000000000002"),
                            "9000000000000000001"));
        }
    }
}
