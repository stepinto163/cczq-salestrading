// Recovered compatibility source; provenance: recovered-support.json.
package sc.app.stc.utils;

import ats.app.ym.utils.logger.AsyncSlf4jLoggerFactory;

import org.slf4j.Logger;

import java.util.ArrayList;
import java.util.List;

public class AsyncSeqThreadGroup<T> {
    private List<AsyncBase<T>> htAsyncBase = new ArrayList<AsyncBase<T>>();
    private final Logger logger = AsyncSlf4jLoggerFactory.getLogger(this.getClass());
    protected String className = this.getClass().getSimpleName();

    public AsyncSeqThreadGroup(int asyncBaseNum) {
        for (int i = 0; i < asyncBaseNum; ++i) {
            AsyncBase asyncBase =
                    new AsyncBase<T>(false, Integer.MAX_VALUE) {

                        @Override
                        protected void process(T data) {
                            try {
                                AsyncSeqThreadGroup.this.process(data);
                            } catch (Exception e) {
                                this.logger.error(this.className + "process error.", (Throwable) e);
                            }
                        }
                    };
            asyncBase.className = this.className + i;
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
        AsyncBase<T> asyncBase = this.htAsyncBase.get(index);
        if (asyncBase != null) {
            if (!asyncBase.getThread().isAlive()) {
                this.htAsyncBase.remove(index);
                this.logger.info(
                        "{} add {} threadNum:{} remove", new Object[] {this.className, id, index});
                this.add(id, paramT);
            } else {
                asyncBase.add(paramT);
            }
            this.logger.debug(
                    "{} threadStatus:{},Queue:{}",
                    new Object[] {
                        this.className, asyncBase.getThread().isAlive(), asyncBase.getQueue().size()
                    });
            if (asyncBase.getQueue().size() > 100) {
                this.logger.warn(
                        "queue[{}] is critical[size:{}]",
                        (Object) (this.className + asyncBase.className),
                        (Object) asyncBase.getQueue().size());
            }
        } else {
            this.logger.error("{} add {} asyncBase is null", (Object) this.className, (Object) id);
        }
        this.logger.debug("{} add {} {} end", new Object[] {this.className, id, paramT});
    }

    public void dispose() {
        for (AsyncBase<T> asyncBase : this.htAsyncBase) {
            asyncBase.dispose();
        }
    }
}
