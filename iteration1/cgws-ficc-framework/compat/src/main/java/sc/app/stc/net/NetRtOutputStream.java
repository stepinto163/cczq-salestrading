package sc.app.stc.net;

import ats.bbox.framework.EDataException;
import ats.bbox.framework.LifecycleException;
import ats.bbox.framework.Server;
import ats.bbox.framework.stream.StreamAdapter;
import ats.message2.CharsetConvertType;
import ats.message2.MsgHelper;

import java.util.Map;

/** Preserves FICC's immediate topic/data publication path. */
public class NetRtOutputStream extends StreamAdapter {
    private NetServerResource server;

    @Override
    public void init() throws LifecycleException {
        server = (NetServerResource) Server.getServer().getResource("NetServerResource");
        if (getConfig().getInt("publishSleepTime", -1) >= 0) {
            throw new LifecycleException("Coalesced market-data publishing has not been migrated");
        }
        server.initHandlers();
        super.init();
    }

    @Override
    public void start() throws LifecycleException {
        server.startServer();
        super.start();
    }

    @Override
    public void onData(Map<String, Object> data) throws EDataException {
        String topic = (String) data.get("topic");
        if (topic == null || topic.isEmpty()) {
            throw new EDataException("Publication requires topic");
        }
        Object payload = data.get("data");
        if (payload == null) {
            payload = data.get("Data");
        }
        try {
            server.publishMessage(topic, MsgHelper.Serializer(payload, CharsetConvertType.None));
        } catch (ats.message2.Exception.SerializerException error) {
            throw new EDataException("Cannot serialize publication: " + topic, error);
        }
    }

    @Override
    public void stop() throws LifecycleException {
        server.stop();
        super.stop();
    }
}
