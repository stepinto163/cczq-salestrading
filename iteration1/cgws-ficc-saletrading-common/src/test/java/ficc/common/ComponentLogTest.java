package ficc.common;

import static org.junit.jupiter.api.Assertions.*;

import ficc.common.client.AdapterFactory;

import org.apache.log4j.Level;
import org.apache.log4j.Logger;
import org.apache.log4j.PatternLayout;
import org.apache.log4j.WriterAppender;
import org.junit.jupiter.api.Test;

import java.io.StringWriter;
import java.util.Properties;

final class ComponentLogTest {
    @Test
    void initializationFailureIsLoggedWithItsStackAndRethrown() throws Exception {
        Logger logger = Logger.getLogger("ficc.startup");
        Level previous = logger.getLevel();
        StringWriter output = new StringWriter();
        WriterAppender appender = new WriterAppender(new PatternLayout("%p %m%n"), output);
        logger.addAppender(appender);
        logger.setLevel(Level.INFO);
        Properties configuration = new Properties();
        configuration.setProperty("diagnostics.serviceName", "QuoteSvr");
        configuration.setProperty("crm.password", "PRIVATE-PASSWORD");
        try {
            IllegalStateException failure =
                    new IllegalStateException(
                            "Invalid configuration PRIVATE-PASSWORD",
                            new IllegalArgumentException("root cause PRIVATE-PASSWORD"));
            Exception actual =
                    assertThrows(
                            IllegalStateException.class,
                            () ->
                                    ComponentLog.initialize(
                                            configuration,
                                            "CRM",
                                            "postgresql",
                                            "PostgresCrmClient",
                                            "local",
                                            () -> {
                                                throw failure;
                                            }));
            assertSame(failure, actual);
            String log = output.toString();
            assertTrue(log.contains("\"state\":\"STARTING\""));
            assertTrue(log.contains("ERROR COMPONENT"));
            assertTrue(log.contains("\"state\":\"FAILED\""));
            assertFalse(log.contains("INITIALIZED"));
            assertTrue(log.contains("Caused by: java.lang.IllegalArgumentException"));
            assertTrue(log.contains("ComponentLogTest.java:"));
            assertFalse(log.contains("PRIVATE-PASSWORD"));

            output.getBuffer().setLength(0);
            configuration.setProperty("crm.mode", "empty");
            AdapterFactory.crm(configuration, null).close();
            assertTrue(output.toString().contains("\"implementation\":\"EmptyCrmClient\""));
            assertTrue(output.toString().contains("\"state\":\"INITIALIZED\""));
            output.getBuffer().setLength(0);
            configuration.setProperty("crm.mode", "postgresql");
            assertThrows(
                    IllegalArgumentException.class, () -> AdapterFactory.crm(configuration, null));
            assertTrue(output.toString().contains("crm.jdbcUrl is required"));
            assertTrue(output.toString().contains("\"state\":\"FAILED\""));
        } finally {
            logger.removeAppender(appender);
            appender.close();
            logger.setLevel(previous);
        }
    }

    @Test
    void databaseTargetExcludesPasswordAndConnectionOptions() {
        Properties configuration = new Properties();
        configuration.setProperty(
                "crm.jdbcUrl",
                "jdbc:postgresql://name:PRIVATE@localhost:55432/ficc_ibor?password=PRIVATE");
        configuration.setProperty("crm.schema", "public");
        assertEquals(
                "localhost:55432/ficc_ibor schema=public",
                ComponentLog.databaseTarget(configuration, "crm.jdbcUrl", "crm.schema"));
    }
}
