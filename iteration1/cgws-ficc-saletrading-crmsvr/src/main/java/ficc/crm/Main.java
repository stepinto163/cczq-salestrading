package ficc.crm;

import ats.bbox.framework.ILifecycle;
import ats.bbox.framework.Server;

import sc.app.stc.utils.ServerWrapper;

/** Standard STC startup includes registry and BBox resource lifecycle. */
public final class Main {
    public static void main(String[] args) throws Exception {
        String config = args.length == 0 ? "./control/CRMSvr.ini" : args[0];
        ServerWrapper.Start("CRMSvr", config);
        if (Server.getServer().getStatus() != ILifecycle.STATUS.STARTED) {
            throw new IllegalStateException("CRMSvr failed to reach STARTED");
        }
        System.out.println("CRMSvr READY (STC startup; registry configuration applied)");
    }
}
