package ficc.quote;

import ats.bbox.framework.ILifecycle;
import ats.bbox.framework.Server;
import sc.app.stc.utils.ServerWrapper;

/** Standard STC startup includes registry and BBox resource/stream lifecycle. */
public final class Main {
    public static void main(String[] args) throws Exception {
        String config = args.length == 0 ? "./control/QuoteSvr.ini" : args[0];
        ServerWrapper.Start("QuoteSvr", config);
        if (Server.getServer().getStatus() != ILifecycle.STATUS.STARTED) {
            throw new IllegalStateException("QuoteSvr failed to reach STARTED");
        }
        System.out.println("QuoteSvr READY (STC startup; registry configuration applied)");
    }
}
