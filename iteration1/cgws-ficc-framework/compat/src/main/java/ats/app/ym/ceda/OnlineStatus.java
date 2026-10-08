// Recovered compatibility source; provenance: recovered-support.json.
package ats.app.ym.ceda;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import java.util.Hashtable;

public class OnlineStatus implements ICedaConnectionListener {
    private final Hashtable<String, Integer> hashtable = new Hashtable();
    private final Logger logger = LoggerFactory.getLogger(this.getClass());

    @Override
    public void onConnected(String serverName) {
        this.addCount(serverName, 1);
    }

    @Override
    public void onDisconnected(String serverName) {
        this.addCount(serverName, -1);
    }

    private synchronized void addCount(String serverName, int num) {
        Integer count = this.hashtable.get(serverName);
        if (count == null) {
            count = 0;
        }
        if ((count = Integer.valueOf(count + num)) < 0) {
            count = 0;
        }
        this.hashtable.put(serverName, count);
        this.logger.debug("OnlineStatus: name={},count={}", (Object) serverName, (Object) count);
    }

    public boolean isOnline(String serverName) {
        Integer count = this.hashtable.get(serverName);
        this.logger.debug("{}:{}", (Object) serverName, (Object) count);
        if (count == null) {
            return false;
        }
        return count > 0;
    }
}
