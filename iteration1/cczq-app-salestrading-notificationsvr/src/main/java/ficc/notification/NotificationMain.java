package ficc.notification;

import ats.bbox.framework.ILifecycle;
import ats.bbox.framework.Server;
import sc.app.stc.utils.ServerWrapper;

/** Standard STC startup includes registry and BBox resource/stream lifecycle. */
public final class NotificationMain {
    public static void main(String[] args) throws Exception {
        String config = args.length == 0 ? "./control/NotificationSvr.ini" : args[0];
        ServerWrapper.Start("NotificationSvr", config);
        if (Server.getServer().getStatus() != ILifecycle.STATUS.STARTED) {
            throw new IllegalStateException("NotificationSvr failed to reach STARTED");
        }
        System.out.println("NotificationSvr READY (STC startup; registry configuration applied)");
    }
}
