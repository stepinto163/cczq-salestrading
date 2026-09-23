package ficc.qtintake.stream;

import ats.bbox.framework.LifecycleException;
import ats.bbox.framework.Server;
import ats.bbox.framework.stream.StreamAdapter;

import ficc.qtintake.QtIntakeResource;

import java.util.concurrent.*;

/** BBox owns the forward worker lifecycle. Each tick submits at most one buffered quote. */
public final class QtIntakeStream extends StreamAdapter {
    private ScheduledExecutorService worker;

    @Override
    public void start() throws LifecycleException {
        super.start();
        QtIntakeResource resource =
                (QtIntakeResource) Server.getServer().getResource("QtIntakeResource");
        worker =
                Executors.newSingleThreadScheduledExecutor(
                        task -> {
                            Thread thread = new Thread(task, "qtintake-forward");
                            thread.setDaemon(true);
                            return thread;
                        });
        worker.scheduleWithFixedDelay(resource::forward, 1, 1, TimeUnit.SECONDS);
    }

    @Override
    public void stop() throws LifecycleException {
        if (worker != null) {
            worker.shutdownNow();
            try {
                if (!worker.awaitTermination(10, TimeUnit.SECONDS)) {
                    throw new LifecycleException("QtIntake forward worker did not stop");
                }
            } catch (InterruptedException error) {
                Thread.currentThread().interrupt();
                throw new LifecycleException(error);
            }
        }
        super.stop();
    }
}
