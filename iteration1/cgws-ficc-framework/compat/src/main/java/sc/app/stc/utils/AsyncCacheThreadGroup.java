// Recovered compatibility source; provenance: recovered-support.json.
package sc.app.stc.utils;

import ats.app.ym.utils.logger.AsyncSlf4jLoggerFactory;

import org.slf4j.Logger;

import java.util.ArrayList;
import java.util.List;

public class AsyncCacheThreadGroup<T> {
    private List<AsyncCacheBase<T>> htAsyncBase = new ArrayList<AsyncCacheBase<T>>();
    protected int sleepTime = -1;
    private final Logger logger = AsyncSlf4jLoggerFactory.getLogger(this.getClass());
    protected String className = this.getClass().getSimpleName();

    public AsyncCacheThreadGroup(int asyncBaseNum, int sleepTime) {
        this.sleepTime = sleepTime;
        this.initCacheThreadGroup(asyncBaseNum);
    }

    public AsyncCacheThreadGroup(int asyncBaseNum) {
        this.initCacheThreadGroup(asyncBaseNum);
    }

    private void initCacheThreadGroup(int asyncBaseNum) {
        for (int i = 0; i < asyncBaseNum; ++i) {
            AsyncCacheBase asyncBase =
                    new AsyncCacheBase<T>(false, Integer.MAX_VALUE) {

                        @Override
                        protected void process(T data) {
                            try {
                                AsyncCacheThreadGroup.this.process(data);
                            } catch (Exception e) {
                                this.logger.error(
                                        "{} process error[{}].",
                                        (Object) this.className,
                                        (Object) e);
                            }
                        }
                    };
            asyncBase.className = this.className + i;
            asyncBase.sleepTime = this.sleepTime;
            asyncBase.start();
            this.htAsyncBase.add(asyncBase);
        }
    }

    protected void process(T paramT) {
        this.logger.debug("{} process {}", (Object) this.className, paramT);
    }

    public void add(String id, T paramT) {
        this.logger.debug("{} add {} {} begin", new Object[] {this.className, id, paramT});
        int index = Math.abs((id + "").hashCode()) % this.htAsyncBase.size();
        this.logger.debug("{} add {} threadNum:{} ", new Object[] {this.className, id, index});
        AsyncCacheBase<T> asyncBase = this.htAsyncBase.get(index);
        if (asyncBase != null) {
            if (!asyncBase.getThread().isAlive()) {
                this.htAsyncBase.remove(index);
                this.logger.debug(
                        "{} add {} threadNum:{} remove", new Object[] {this.className, id, index});
                this.add(id, paramT);
            } else {
                asyncBase.add(id, paramT);
            }
        } else {
            this.logger.error("{} add {} asyncBase is null", (Object) this.className, (Object) id);
        }
        this.logger.debug("{} add {} {} end", new Object[] {this.className, id, paramT});
    }

    public void dispose() {
        for (AsyncCacheBase<T> asyncBase : this.htAsyncBase) {
            asyncBase.dispose();
        }
    }
}
