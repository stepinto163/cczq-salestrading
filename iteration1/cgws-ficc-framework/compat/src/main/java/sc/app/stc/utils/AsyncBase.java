// Recovered compatibility source; provenance: recovered-support.json.
package sc.app.stc.utils;

import org.apache.http.util.TextUtils;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import java.util.concurrent.LinkedBlockingQueue;

public class AsyncBase<T> {
    private LinkedBlockingQueue<T> queue;
    protected final Logger logger = LoggerFactory.getLogger(this.getClass());
    public Thread thread;
    protected boolean isStop = false;
    public String className = null;
    protected static int maxSize = Integer.MAX_VALUE;

    public AsyncBase() {
        this(true, maxSize);
    }

    public AsyncBase(int maxSize) {
        this(true, maxSize);
    }

    public AsyncBase(boolean paramBoolean, int maxSize) {
        this.queue = new LinkedBlockingQueue(maxSize);
        this.thread =
                new Thread(
                        new Runnable() {

                            @Override
                            public final void run() {
                                while (!AsyncBase.this.isStop) {
                                    try {
                                        AsyncBase.this.logger.debug(
                                                "{} process begin",
                                                (Object) AsyncBase.this.className);
                                        T obj = AsyncBase.this.queue.take();
                                        AsyncBase.this.logger.debug(
                                                "{} process data[{}]",
                                                (Object) AsyncBase.this.className,
                                                obj);
                                        AsyncBase.this.process(obj);
                                        AsyncBase.this.logger.debug(
                                                "{} process end",
                                                (Object) AsyncBase.this.className);
                                    } catch (InterruptedException obj) {
                                    } catch (Exception localException) {
                                        AsyncBase.this.logger.warn(
                                                "deliver process error[{}]",
                                                (Throwable) localException);
                                    }
                                }
                                AsyncBase.this.logger.info(
                                        "{} exit", (Object) AsyncBase.this.className);
                            }
                        });
        this.thread.setUncaughtExceptionHandler(new ExceptionHandler());
        if (paramBoolean) {
            this.start();
            return;
        }
        this.isStop = true;
    }

    public Thread getThread() {
        return this.thread;
    }

    public LinkedBlockingQueue<T> getQueue() {
        return this.queue;
    }

    public void start() {
        this.className = this.className != null ? this.className : this.getClass().getSimpleName();
        this.logger.info("{} start", (Object) this.className);
        if (!TextUtils.isBlank((CharSequence) this.className)) {
            this.thread.setName(this.className);
        }
        this.isStop = false;
        if (!this.thread.isAlive()) {
            this.thread.start();
        }
    }

    protected void process(T paramT) {}

    public void add(T paramT) {
        boolean flag = this.queue.offer(paramT);
        if (!flag) {
            this.logger.warn(
                    "queue[{}] is full[size:{}],so drop[{}]",
                    new Object[] {this.className, this.queue.size(), paramT});
        }
    }

    public void dispose() {
        this.isStop = true;
        try {
            if (this.thread.isAlive()) {
                this.thread.interrupt();
            }
        } catch (Exception exception) {
            // empty catch block
        }
        this.logger.info("{} stop", (Object) this.className);
    }

    public class ExceptionHandler implements Thread.UncaughtExceptionHandler {
        @Override
        public void uncaughtException(Thread t, Throwable e) {
            AsyncBase.this.logger.error("An exception has been captured\n");
            AsyncBase.this.logger.error("Thread: {}\n", (Object) t.getName());
            AsyncBase.this.logger.error(
                    "Exception: {}: {}\n",
                    (Object) e.getClass().getSimpleName(),
                    (Object) e.getMessage());
            AsyncBase.this.logger.error("Stack Trace: {}\n", e);
            AsyncBase.this.logger.error("Thread status: {}\n", (Object) t.getState());
        }
    }
}
