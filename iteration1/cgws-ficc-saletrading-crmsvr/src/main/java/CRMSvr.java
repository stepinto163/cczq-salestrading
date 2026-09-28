/**
 * QA / ATS entry point.
 *
 * <p>The ATS control script ({@code ~/scripts/startconsole}) derives the JVM main class from the
 * server name: it reads an {@code entrance:} field from {@code ~/scripts/serverlist} and falls
 * back to the server name itself when that field is absent. STC services follow the same
 * convention, so this class lives in the default package and is named exactly like its server
 * entry.
 *
 * <p>Do not rename or move this class: {@code java ... CRMSvr} is what the QA serverlist entry
 * executes.
 */
public final class CRMSvr {

    private CRMSvr() {
    }

    public static void main(String[] args) throws Exception {
        ficc.crm.Main.main(args);
    }
}
