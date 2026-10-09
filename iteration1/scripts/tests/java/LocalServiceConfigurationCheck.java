import java.io.Reader;
import java.lang.reflect.Method;
import java.nio.file.Files;
import java.nio.file.Path;
import java.nio.file.Paths;
import java.util.Properties;
import javax.xml.parsers.DocumentBuilderFactory;
import org.apache.commons.configuration.PropertiesConfiguration;
import org.w3c.dom.NodeList;

/** Reads real source-tree configs with the installed SDK, without starting network resources. */
public final class LocalServiceConfigurationCheck {
    public static void main(String[] args) throws Exception {
        String service = args[0];
        Path atsRoot = Paths.get(System.getenv("ATS_ROOT"));
        requireFile(atsRoot.resolve("control/ATSConfig.ini"));
        requireFile(atsRoot.resolve("control/DBPoolConfig.ini"));
        requireFile(atsRoot.resolve("control/server.lic"));
        PropertiesConfiguration startup = new PropertiesConfiguration("control/" + service + ".ini");
        requireFile(Paths.get(startup.getString("log4j.file")));
        requireFile(Paths.get(startup.getString("log.file")));
        for (String key : new String[] {"resource.file", "streams.file"}) {
            Path xml = Paths.get(startup.getString(key));
            requireFile(xml);
            NodeList components = DocumentBuilderFactory.newInstance().newDocumentBuilder()
                    .parse(xml.toFile()).getElementsByTagName("component");
            for (int index = 0; index < components.getLength(); index++) {
                org.w3c.dom.Element component = (org.w3c.dom.Element) components.item(index);
                if (component.hasAttribute("config-file")) {
                    requireFile(Paths.get(component.getAttribute("config-file")));
                }
                Class.forName(component.getTextContent().trim(), false,
                        Thread.currentThread().getContextClassLoader());
            }
        }
        PropertiesConfiguration resource = new PropertiesConfiguration("control/" + service + "/resourceCfg.ini");
        if (resource.containsKey("quote.properties") || resource.containsKey("crm.properties")) {
            String setting = resource.containsKey("quote.properties") ? "quote.properties" : "crm.properties";
            Class<?> configuration = Class.forName("ficc.common.ServiceConfiguration");
            Method load = configuration.getMethod("load", String.class, String.class, String.class);
            Properties properties = (Properties) load.invoke(null, service, resource.getString(setting),
                    resource.getString("adapter.properties", ""));
            if (!"CRMSvr".equals(service)) {
                for (String adapter : new String[] {"identity.mode", "base.mode", "crm.mode"}) {
                    if (!"stc".equals(properties.getProperty(adapter))) throw new AssertionError(adapter);
                }
            }
            checkSharedFile(properties.getProperty("intake.authFile"), atsRoot);
        }
        if (resource.containsKey("dbpool.cfg")) {
            Path database = Paths.get(resource.getString("dbpool.cfg")).toAbsolutePath().normalize();
            if (!database.equals(atsRoot.resolve("control/DBPoolConfig.ini").toAbsolutePath().normalize())) {
                throw new AssertionError("DBPoolResource did not resolve shared ATS_ROOT: " + database);
            }
            requireFile(database);
        }
        if ("QtIntakeSvr".equals(service)) {
            Properties intake = new Properties();
            try (Reader reader = Files.newBufferedReader(Paths.get(resource.getString("intake.properties")))) {
                intake.load(reader);
            }
            requireFile(Paths.get(intake.getProperty("fix.settings")));
            checkSharedFile(intake.getProperty("secrets.file"), atsRoot);
        }
        if ("ParserSvr".equals(service)) {
            Class.forName("com.hankcs.hanlp.utility.Predefine").getField("HANLP_PROPERTIES_PATH")
                    .set(null, "./config/hanlp.properties");
            Class.forName("sc.app.stc.parser.manager.ParserManager").getMethod("init").invoke(null);
            Class.forName("sc.app.stc.parser.analyzer.Direction").getMethod("init").invoke(null);
            requireFile(Paths.get("config/function.xml"));
            requireFile(Paths.get("config/database.xml"));
        }
        Class.forName(service, false, Thread.currentThread().getContextClassLoader());
        System.out.println("PASS " + service + ": source config, resources, SDK loading and shared paths; no service started");
    }

    private static void checkSharedFile(String value, Path atsRoot) {
        if (value != null) requireFile(Paths.get(value.replace("$ATS_ROOT", atsRoot.toString())));
    }

    private static void requireFile(Path file) {
        if (!Files.isRegularFile(file)) throw new AssertionError("Missing file: " + file.toAbsolutePath());
    }
}
