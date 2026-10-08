// Recovered compatibility source; provenance: recovered-support.json.
package sc.app.stc.utils;

import ats.app.ym.utils.logger.AsyncSlf4jLoggerFactory;

import org.apache.http.util.TextUtils;
import org.slf4j.Logger;

import java.util.Map;
import java.util.concurrent.ConcurrentHashMap;

public class AsyncCacheBase<T> {
    private ConcurrentHashMap<String, T> queue;
    protected final Logger logger = AsyncSlf4jLoggerFactory.getLogger(this.getClass());
    public Thread thread;
    protected boolean isStop = false;
    public String className = null;
    protected int maxSize = Integer.MAX_VALUE;
    public int sleepTime = -1;

    public AsyncCacheBase(int maxSize) {
        this(true, maxSize);
    }

    public AsyncCacheBase(boolean paramBoolean, int maxSize) {
        this.queue = new ConcurrentHashMap();
        this.thread =
                new Thread(
                        new Runnable() {

                            /*
                             * WARNING - Removed try catching itself - possible behaviour change.
                             */
                            @Override
                            public final void run() {
                                while (!AsyncCacheBase.this.isStop) {
                                    try {
                                        ConcurrentHashMap concurrentHashMap =
                                                AsyncCacheBase.this.queue;
                                        synchronized (concurrentHashMap) {
                                            if (AsyncCacheBase.this.queue.size() == 0) {
                                                AsyncCacheBase.this.queue.wait();
                                            }
                                        }
                                        for (Map.Entry<String, T> entry :
                                                AsyncCacheBase.this.queue.entrySet()) {
                                            T obj = entry.getValue();
                                            String key = (String) entry.getKey();
                                            AsyncCacheBase.this.queue.remove(key);
                                            AsyncCacheBase.this.process(obj);
                                        }
                                        if (AsyncCacheBase.this.sleepTime <= 0) continue;
                                        Thread.sleep(AsyncCacheBase.this.sleepTime);
                                    } catch (InterruptedException interruptedException) {
                                    } catch (Exception localException) {
                                        AsyncCacheBase.this.logger.warn(
                                                "deliver process error",
                                                (Throwable) localException);
                                    }
                                }
                                AsyncCacheBase.this.logger.info(
                                        "{} exit", (Object) AsyncCacheBase.this.className);
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

    public ConcurrentHashMap<String, T> getQueue() {
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

    /*
     * WARNING - Removed try catching itself - possible behaviour change.
     */
    public void add(String key, T paramT) {
        this.queue.put(key, paramT);
        ConcurrentHashMap<String, T> concurrentHashMap = this.queue;
        synchronized (concurrentHashMap) {
            this.queue.notify();
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
            AsyncCacheBase.this.logger.error("An exception has been captured\n");
            AsyncCacheBase.this.logger.error("Thread: {}\n", (Object) t.getName());
            AsyncCacheBase.this.logger.error(
                    "Exception: {}: {}\n",
                    (Object) e.getClass().getSimpleName(),
                    (Object) e.getMessage());
            AsyncCacheBase.this.logger.error("Stack Trace: \n");
            AsyncCacheBase.this.logger.error("Thread status: {}\n", (Object) t.getState());
        }
    }
}
