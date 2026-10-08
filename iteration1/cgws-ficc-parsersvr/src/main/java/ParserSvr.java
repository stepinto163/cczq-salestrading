import sc.app.stc.utils.ServerWrapper;

/**
 * TODO
 *
 * @author fangfang.zhang
 * @date 2021/6/8
 */
public class ParserSvr {
    public static void main(String[] args) {
        final String configFile="./control/ParserSvr.ini";
        try {
//            System.loadLibrary("./javalib/ParseLib");
            ServerWrapper.Start("ParserSvr",configFile);
        } catch (Exception e) {
            System.out.println();
            e.printStackTrace();
        }
    }
}
