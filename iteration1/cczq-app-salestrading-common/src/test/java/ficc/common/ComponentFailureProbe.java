package ficc.common;

import ficc.common.client.AdapterFactory;
import java.nio.file.Files;
import java.nio.file.Path;
import java.util.Properties;
import org.apache.log4j.PropertyConfigurator;

/** A deliberately failing process for verifying deployed Log4j routing and exit status. */
public final class ComponentFailureProbe {
    public static void main(String[] args) throws Exception {
        Properties logging = new Properties();
        try (var reader = Files.newBufferedReader(Path.of(args[0]))) {
            logging.load(reader);
        }
        logging.setProperty("log4j.appender.startupFile.File", args[1]);
        logging.setProperty("log4j.appender.dpsMessages.File", args[1] + ".dps");
        PropertyConfigurator.configure(logging);
        Properties configuration = new Properties();
        configuration.setProperty("diagnostics.serviceName", "FailureProbe");
        configuration.setProperty("crm.mode", "postgresql");
        AdapterFactory.crm(configuration, null);
    }
}
