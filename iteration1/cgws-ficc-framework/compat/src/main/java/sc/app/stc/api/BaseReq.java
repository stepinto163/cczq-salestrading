// Recovered compatibility source; provenance: recovered-support.json.
package sc.app.stc.api;

import com.alibaba.fastjson.annotation.JSONField;

import sc.app.stc.utils.IdUtil;

import java.util.concurrent.CountDownLatch;
import java.util.concurrent.TimeUnit;

public class BaseReq {
    public String cid = "B" + IdUtil.getId();

    @JSONField(serialize = false)
    public long time;

    CountDownLatch count = new CountDownLatch(1);

    public boolean cWait(long timeout) throws InterruptedException {
        return this.count.await(timeout, TimeUnit.MILLISECONDS);
    }

    public void cNotify() {
        this.count.countDown();
    }
}
