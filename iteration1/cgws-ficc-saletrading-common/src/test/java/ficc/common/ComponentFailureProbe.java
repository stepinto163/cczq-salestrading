package ficc.common;

import ficc.common.client.AdapterFactory;

import org.apache.log4j.PropertyConfigurator;

import java.io.BufferedReader;
import java.nio.file.Files;
import java.nio.file.Paths;
import java.util.Properties;

/** A deliberately failing process for verifying deployed Log4j routing and exit status. */
public final class ComponentFailureProbe {
    public static void main(String[] args) throws Exception {
        Properties logging = new Properties();
        try (BufferedReader reader = Files.newBufferedReader(Paths.get(args[0]))) {
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
