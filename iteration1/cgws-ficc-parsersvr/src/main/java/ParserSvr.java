import sc.app.stc.utils.ServerWrapper;

/**
 * TODO
 *
 * @author fangfang.zhang
 * @date 2021/6/8
 */
public class ParserSvr {
    public static void main(String[] args) throws Exception {
        String hanlpConfig = System.getProperty("hanlp.properties");
        if (hanlpConfig != null && !hanlpConfig.trim().isEmpty()) {
            com.hankcs.hanlp.utility.Predefine.HANLP_PROPERTIES_PATH = hanlpConfig;
        }
        String configFile = args.length == 0 ? "./control/ParserSvr.ini" : args[0];
        ServerWrapper.Start("ParserSvr", configFile);
        if (ats.bbox.framework.Server.getServer().getStatus()
                != ats.bbox.framework.ILifecycle.STATUS.STARTED) {
            throw new IllegalStateException("ParserSvr failed to reach STARTED");
        }
    }
}
