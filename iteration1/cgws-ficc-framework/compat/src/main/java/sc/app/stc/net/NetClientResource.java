package sc.app.stc.net;

import ats.bbox.framework.LifecycleException;
import ats.bbox.framework.resource.ResourceAdapter;

public class NetClientResource extends ResourceAdapter {
    private NetClientWrapper client;

    @Override
    public void init() throws LifecycleException {
        client = new NetClientWrapper();
        super.init();
    }

    public NetClientWrapper getNetClientWrapper() {
        return client;
    }

    @Override
    public void stop() throws LifecycleException {
        if (client != null) {
            client.dispose();
        }
        super.stop();
    }

    @Override
    public void dispose() throws LifecycleException {
        if (client != null) {
            client.dispose();
        }
        super.dispose();
    }
}
