package cgws.ficc.framework.compat;

import static org.junit.jupiter.api.Assertions.*;

import ats.app.ym.utils.logger.AsyncSlf4jLoggerFactory;

import org.apache.commons.configuration.PropertiesConfiguration;
import org.apache.log4j.LogManager;
import org.junit.jupiter.api.Test;
import org.junit.jupiter.api.io.TempDir;

import java.io.ByteArrayOutputStream;
import java.io.PrintStream;
import java.nio.charset.StandardCharsets;
import java.nio.file.Files;
import java.nio.file.Path;

class AsyncLoggingTest {
    @TempDir Path directory;

    @Test
    void configuredAsyncLoggerWritesMessageAndExceptionToConsoleAndFile() throws Exception {
        Path file = directory.resolve("application.log");
        Path settings = directory.resolve("log4j.ini");
        String properties =
                "log4j.rootLogger=INFO,C,F\n"
                        + "log4j.appender.C=org.apache.log4j.ConsoleAppender\n"
                        + "log4j.appender.C.layout=org.apache.log4j.PatternLayout\n"
                        + "log4j.appender.C.layout.ConversionPattern=%p %m%n\n"
                        + "log4j.appender.F=org.apache.log4j.FileAppender\n"
                        + "log4j.appender.F.File="
                        + file.toString().replace('\\', '/')
                        + "\n"
                        + "log4j.appender.F.layout=org.apache.log4j.PatternLayout\n"
                        + "log4j.appender.F.layout.ConversionPattern=%p %m%n\n";
        Files.write(settings, properties.getBytes(StandardCharsets.UTF_8));
        PropertiesConfiguration configuration = new PropertiesConfiguration();
        configuration.setProperty("log4j.file", settings.toString());
        configuration.setProperty("log4j.async", true);
        configuration.setProperty("log4j.thread", 1);
        configuration.setProperty("log4j.size", 1);
        configuration.setProperty("log4j.writeTime", true);
        ByteArrayOutputStream captured = new ByteArrayOutputStream();
        PrintStream previous = System.out;
        try (PrintStream output = new PrintStream(captured, true, "UTF-8")) {
            System.setOut(output);
            AsyncSlf4jLoggerFactory.initialize(configuration);
            org.slf4j.Logger logger = AsyncSlf4jLoggerFactory.getLogger(AsyncLoggingTest.class);
            logger.info("initialized {}", "component");
            logger.error("failed component", new IllegalStateException("explicit failure"));
            AsyncSlf4jLoggerFactory.flush();
            String console = captured.toString("UTF-8");
            String saved = new String(Files.readAllBytes(file), StandardCharsets.UTF_8);
            for (String content : new String[] {console, saved}) {
                assertTrue(content.contains("initialized component"), content);
                assertTrue(content.contains("IllegalStateException: explicit failure"), content);
                assertTrue(
                        content.matches("(?s).*INFO \\d{2}:\\d{2}:\\d{2}\\.\\d{3} initialized.*"),
                        content);
            }
        } finally {
            System.setOut(previous);
            LogManager.resetConfiguration();
        }
    }
}
