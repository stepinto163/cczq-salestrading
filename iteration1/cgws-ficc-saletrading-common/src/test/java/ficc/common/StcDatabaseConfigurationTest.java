package ficc.common;

import static org.junit.jupiter.api.Assertions.*;

import ats.foundation.utils.db.base.DBPoolParam;

import org.junit.jupiter.api.Test;
import org.junit.jupiter.api.io.TempDir;

import java.nio.charset.StandardCharsets;
import java.nio.file.Files;
import java.nio.file.Path;
import java.util.ArrayList;
import java.util.List;
import java.util.Properties;

final class StcDatabaseConfigurationTest {
    @TempDir Path directory;

    @Test
    void addingSalesTradingSourcesPreservesTheExistingStcSource() throws Exception {
        String original = source(0, "MYSQL0", "stc", "original-secret");
        Path before = write("before.ini", "DBPOOL.DBCount=1\nDBPOOL.LogLevel=0\n" + original);
        Path after =
                write(
                        "after.ini",
                        "DBPOOL.DBCount=3\nDBPOOL.LogLevel=0\n"
                                + original
                                + source(
                                        1,
                                        "FICC_SALESTRADING",
                                        "ficc_salestrading",
                                        "trading-secret")
                                + source(2, "FICC_CRM", "ficc_ibor", "crm-secret"));
        DBPoolParam existingBefore = StcDatabaseConfiguration.readSources(before).get(0);
        List<DBPoolParam> expanded = StcDatabaseConfiguration.readSources(after);
        assertEquals(3, expanded.size());
        DBPoolParam existingAfter = expanded.get(0);
        assertEquals(existingBefore.SourceName, existingAfter.SourceName);
        assertEquals(existingBefore.Driver, existingAfter.Driver);
        assertEquals(existingBefore.Url, existingAfter.Url);
        assertEquals(existingBefore.Username, existingAfter.Username);
        assertEquals(existingBefore.Passwd, existingAfter.Passwd);
        assertEquals(existingBefore.IsEncrypt, existingAfter.IsEncrypt);
        assertTrue(
                new String(Files.readAllBytes(after), StandardCharsets.UTF_8).contains(original));
        // The loader must not retain a file handle: operators can replace the config after startup.
        Path moved = directory.resolve("moved.ini");
        Files.move(before, moved);
        Files.delete(moved);

        Properties configuration = configuration(after);
        List<String> logs = new ArrayList<>();
        StcDatabaseConfiguration.apply(configuration, logs::add);
        assertEquals(
                "jdbc:postgresql://127.0.0.1:1/ficc_salestrading",
                configuration.getProperty("db.url"));
        assertEquals("trading-secret", configuration.getProperty("db.password"));
        assertEquals(
                "jdbc:postgresql://127.0.0.1:1/ficc_ibor",
                configuration.getProperty("crm.jdbcUrl"));
        assertEquals("crm-secret", configuration.getProperty("crm.password"));
        assertFalse(String.join("\n", logs).contains("secret"));
    }

    @Test
    void unknownDatasourceFailsWithoutFallingBackToMysql0() throws Exception {
        Path file =
                write(
                        "missing.ini",
                        "DBPOOL.DBCount=1\nDBPOOL.LogLevel=0\n"
                                + source(0, "MYSQL0", "stc", "original-secret"));
        java.io.IOException error =
                assertThrows(
                        java.io.IOException.class,
                        () -> StcDatabaseConfiguration.apply(configuration(file), message -> {}));
        assertTrue(error.getMessage().contains("FICC_SALESTRADING"));
    }

    @Test
    void duplicateNamesAndMissingFilesFail() throws Exception {
        Path file =
                write(
                        "duplicate.ini",
                        "DBPOOL.DBCount=2\nDBPOOL.LogLevel=0\n"
                                + source(0, "FICC_SALESTRADING", "one", "one-secret")
                                + source(1, "FICC_SALESTRADING", "two", "two-secret"));
        assertThrows(
                java.io.IOException.class,
                () -> StcDatabaseConfiguration.apply(configuration(file), message -> {}));
        assertThrows(
                java.io.IOException.class,
                () ->
                        StcDatabaseConfiguration.apply(
                                configuration(directory.resolve("absent.ini")), message -> {}));
    }

    private Properties configuration(Path file) {
        Properties configuration = new Properties();
        configuration.setProperty("dbpool.cfg", file.toString());
        configuration.setProperty("db.sourceName", "FICC_SALESTRADING");
        configuration.setProperty("crm.sourceName", "FICC_CRM");
        return configuration;
    }

    @Test
    void startupReportsTheMissingSourceToConsoleAndFileAndPropagatesFailure() throws Exception {
        Path pool =
                write(
                        "startup.ini",
                        "DBPOOL.DBCount=1\nDBPOOL.LogLevel=0\n"
                                + source(0, "MYSQL0", "stc", "original-secret"));
        Path service =
                write(
                        "service.properties",
                        "dbpool.cfg="
                                + pool.toString().replace('\\', '/')
                                + "\ndb.sourceName=FICC_SALESTRADING\n");
        Path logfile = directory.resolve("startup.log");
        java.io.ByteArrayOutputStream consoleOutput = new java.io.ByteArrayOutputStream();
        java.io.PrintStream previousOutput = System.out;
        org.apache.log4j.Logger logger = org.apache.log4j.Logger.getLogger("ficc.startup");
        org.apache.log4j.Level previousLevel = logger.getLevel();
        org.apache.log4j.PatternLayout layout = new org.apache.log4j.PatternLayout("%p %m%n");
        org.apache.log4j.FileAppender file =
                new org.apache.log4j.FileAppender(layout, logfile.toString());
        org.apache.log4j.ConsoleAppender console = new org.apache.log4j.ConsoleAppender(layout);
        try {
            System.setOut(new java.io.PrintStream(consoleOutput, true, "UTF-8"));
            console.setFollow(true);
            console.activateOptions();
            logger.setLevel(org.apache.log4j.Level.INFO);
            logger.addAppender(file);
            logger.addAppender(console);
            assertThrows(
                    java.io.IOException.class,
                    () -> ServiceConfiguration.load("QuoteSvr", service.toString(), ""));
        } finally {
            logger.removeAppender(file);
            logger.removeAppender(console);
            file.close();
            console.close();
            logger.setLevel(previousLevel);
            System.setOut(previousOutput);
        }
        for (String log :
                java.util.Arrays.asList(
                        consoleOutput.toString("UTF-8"),
                        new String(Files.readAllBytes(logfile), StandardCharsets.UTF_8))) {
            assertTrue(log.contains("\"state\":\"STARTING\""));
            assertTrue(log.contains("\"state\":\"FAILED\""));
            assertTrue(log.contains("FICC_SALESTRADING"));
            assertTrue(log.contains("StcDatabaseConfiguration.java:"));
            assertFalse(log.contains("original-secret"));
            assertFalse(log.contains("\"state\":\"INITIALIZED\""));
        }
    }

    private Path write(String name, String content) throws Exception {
        return Files.write(directory.resolve(name), content.getBytes(StandardCharsets.UTF_8));
    }

    private String source(int index, String name, String database, String password) {
        String prefix = "DBPOOL.";
        return prefix
                + "DBSourceName_"
                + index
                + "="
                + name
                + "\n"
                + prefix
                + "DBDriver_"
                + index
                + "=org.postgresql.Driver\n"
                + prefix
                + "DBUrl_"
                + index
                + "=jdbc:postgresql://127.0.0.1:1/"
                + database
                + "\n"
                + prefix
                + "DBUsername_"
                + index
                + "=test-user\n"
                + prefix
                + "DBPasswd_"
                + index
                + "="
                + password
                + "\n"
                + prefix
                + "DBCheckSql_"
                + index
                + "=select 1\n"
                + prefix
                + "DBIsEncrypt_"
                + index
                + "=false\n"
                + prefix
                + "DBMaxCount_"
                + index
                + "=10\n"
                + prefix
                + "DBMinCount_"
                + index
                + "=1\n"
                + prefix
                + "DBConnOutTime_"
                + index
                + "=1000000\n"
                + prefix
                + "DBConnCheckNumber_"
                + index
                + "=1000\n"
                + prefix
                + "DBStrategy_"
                + index
                + "=false\n";
    }
}
