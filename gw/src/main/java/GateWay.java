
import org.apache.log4j.BasicConfigurator;
import org.apache.log4j.PropertyConfigurator;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.context.ApplicationContext;
import org.springframework.context.support.FileSystemXmlApplicationContext;

public class GateWay {
	public static void main(String[] args) {

		BasicConfigurator.configure();
		PropertyConfigurator.configureAndWatch("./config/log4j.ini", 1000);
		Logger logger = LoggerFactory.getLogger(GateWay.class);

		try {
			Runtime.getRuntime().addShutdownHook(new Thread() {
				@Override
				public void run() {
					logger.info("Received quit notify....");
				}
			});
			ApplicationContext context = new FileSystemXmlApplicationContext(
					new String[] { "./config/spring-config.xml" });

			context.getApplicationName();

		} catch (Exception e) {
			logger.error("gateway start error。", e);
		}
	}
}
