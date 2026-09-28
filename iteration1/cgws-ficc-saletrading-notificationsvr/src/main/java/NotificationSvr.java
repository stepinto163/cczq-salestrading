/**
 * QA / ATS entry point.
 *
 * <p>The ATS control script ({@code ~/scripts/startconsole}) derives the JVM main class from the
 * server name: it reads an {@code entrance:} field from {@code ~/scripts/serverlist} and falls back
 * to the server name itself when that field is absent. STC services follow the same convention —
 * {@code LoginSvr}, {@code OHLCSvr} and {@code BaseSvr} all live in the default package and are
 * named exactly like their server entries.
 *
 * <p>Do not rename or move this class: the QA serverlist entry {@code
 * stc/NotificationSvr|NotificationSvr|...} has no {@code entrance:} field, so {@code java ...
 * NotificationSvr} is what actually gets executed.
 */
public final class NotificationSvr {

    private NotificationSvr() {}

    public static void main(String[] args) throws Exception {
        ficc.notification.NotificationMain.main(args);
    }
}
