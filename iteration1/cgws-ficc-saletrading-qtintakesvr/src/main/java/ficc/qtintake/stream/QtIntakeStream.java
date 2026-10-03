package ficc.qtintake.stream;

import ats.bbox.framework.LifecycleException;
import ats.bbox.framework.Server;
import ats.bbox.framework.stream.StreamAdapter;

import ficc.qtintake.QtIntakeResource;

import java.util.concurrent.*;

/** BBox owns the forward worker lifecycle. One worker drains pending requests in arrival order. */
public final class QtIntakeStream extends StreamAdapter {
    private ExecutorService worker;

    @Override
    public void start() throws LifecycleException {
        super.start();
        QtIntakeResource resource =
                (QtIntakeResource) Server.getServer().getResource("QtIntakeResource");
        worker =
                Executors.newSingleThreadExecutor(
                        task -> {
                            Thread thread = new Thread(task, "qtintake-forward");
                            thread.setDaemon(true);
                            return thread;
                        });
        worker.submit(resource::forward);
    }

    @Override
    public void stop() throws LifecycleException {
        if (worker != null) {
            worker.shutdownNow();
            try {
                if (!worker.awaitTermination(10, TimeUnit.SECONDS)) {
                    throw new LifecycleException("QtIntakeSvr forward worker did not stop");
                }
            } catch (InterruptedException error) {
                Thread.currentThread().interrupt();
                throw new LifecycleException(error);
            }
        }
        super.stop();
    }
}
