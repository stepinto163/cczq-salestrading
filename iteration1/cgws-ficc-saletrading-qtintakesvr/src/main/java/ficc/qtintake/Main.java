package ficc.qtintake;

import ats.bbox.framework.ILifecycle;
import ats.bbox.framework.Server;

import sc.app.stc.utils.ServerWrapper;

public final class Main {
    public static void main(String[] args) throws Exception {
        ServerWrapper.Start("QtIntakeSvr", args.length == 0 ? "./control/QtIntakeSvr.ini" : args[0]);
        if (Server.getServer().getStatus() != ILifecycle.STATUS.STARTED) {
            throw new IllegalStateException("QtIntakeSvr failed to reach STARTED");
        }
        System.out.println("QtIntakeSvr STARTED; FIX/QuoteSvr connectivity is reported separately");
    }
}
