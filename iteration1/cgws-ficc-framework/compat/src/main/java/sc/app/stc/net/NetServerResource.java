package sc.app.stc.net;

import ats.app.ym.ceda.server.CedaResultMap;
import ats.app.ym.ceda.server.ContentHandler;
import ats.bbox.framework.LifecycleException;
import ats.bbox.framework.resource.ResourceAdapter;
import ats.foundation.utils.config.PropertiesHelper;

import cgws.ficc.framework.LifecycleActions;
import cgws.ficc.framework.compat.HandlerRegistry;
import cgws.ficc.framework.compat.ServiceSettings;
import cgws.ficc.framework.dps.DpsServer;

import com.adaptiveMQ2.message.Message;
import com.gateway.connector.utils.JsonUtils;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import java.nio.charset.StandardCharsets;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Map;
import java.util.concurrent.ConcurrentHashMap;

/** XML-compatible host for the unchanged DPS server SDK. */
public class NetServerResource extends ResourceAdapter {
    private static final Logger PAYLOAD = LoggerFactory.getLogger("ficc.dps");
    private final HandlerRegistry handlers = new HandlerRegistry();
    private final ConcurrentHashMap<String, Boolean> onlineUsers = new ConcurrentHashMap<>();
    private ServiceSettings settings;
    private DpsServer server;

    @Override
    public void init() throws LifecycleException {
        settings = ServiceSettings.load(getConfig().getString("serverKey"));
        String mode = sc.app.stc.utils.Consts.getNetType();
        if (!"dps".equalsIgnoreCase(mode)) {
            throw new LifecycleException(
                    "This framework requires the existing DPS transport: " + mode);
        }
        server = new DpsServer(this::dispatch, this::sessionStatus);
        super.init();
    }

    public void initHandlers() {
        try {
            LifecycleActions.run(
                    settings.name,
                    "HANDLERS",
                    HandlerRegistry.class.getName(),
                    "STARTING",
                    "INITIALIZED",
                    () -> handlers.load(getConfig()));
        } catch (Exception error) {
            throw new IllegalStateException("Handler initialization failed", error);
        }
    }

    public void startServer() {
        try {
            LifecycleActions.run(
                    settings.name,
                    "DPS_LISTENER",
                    DpsServer.class.getName(),
                    "dps",
                    settings.endpoint.toString(),
                    "STARTING",
                    "LISTENING",
                    () ->
                            server.start(
                                    settings.endpoint,
                                    PropertiesHelper.getFullPath(
                                            PropertiesHelper.getATS_ROOT(),
                                            "/control/server.lic")));
        } catch (Exception error) {
            throw new IllegalStateException(
                    "DPS listener startup failed: " + settings.endpoint, error);
        }
    }

    public DpsServer.Reply dispatch(String session, String topic, String content) {
        ContentHandler handler = handlers.find(topic);
        if (handler == null) {
            if (content == null || content.isEmpty()) {
                return new DpsServer.Reply(null, null);
            }
            Map<String, Object> error = new HashMap<>();
            error.put("Code", "1003");
            error.put("Msg", "handler:" + topic + " does not exist.");
            return new DpsServer.Reply(JsonUtils.SerializerBytes(error), null);
        }
        Message message = new Message();
        message.setSignalID(session);
        Map<String, Object> values = new HashMap<>();
        values =
                content != null && !content.trim().isEmpty()
                        ? handler.handle(topic, message, content, values, false)
                        : handler.snapshot(topic, message, values, false);
        byte[] body;
        if (values instanceof CedaResultMap) {
            CedaResultMap result = (CedaResultMap) values;
            String serialized = result.data == null ? "{}" : JsonUtils.Serializer(result.data);
            if (!result.noPermitTopics.isEmpty()) {
                serialized += "=NOP=" + JsonUtils.Serializer(result.noPermitTopics);
            }
            body = serialized.getBytes(StandardCharsets.UTF_8);
        } else {
            body = JsonUtils.SerializerBytes(values);
        }
        return new DpsServer.Reply(body, values);
    }

    public List<String> publishMessage(String topic, String content) {
        if (PAYLOAD.isDebugEnabled()) {
            PAYLOAD.debug(
                    "DPS direction=PUBLISH topic={} body={}",
                    topic,
                    cgws.ficc.framework.dps.DpsPayloadLog.body(content));
        }
        return server.notify(topic, content);
    }

    @SuppressWarnings("unchecked")
    private void sessionStatus(String content) {
        Map<String, Object> values = JsonUtils.Deserialize(content, Map.class);
        List<String> users = (List<String>) values.get("userNames");
        onSession(
                users,
                Integer.parseInt(String.valueOf(values.get("status"))),
                Boolean.parseBoolean(String.valueOf(values.get("all"))));
    }

    public void onSession(List<String> users, int status, boolean all) {
        if (all) {
            onlineUsers.clear();
        }
        for (String user : users) {
            if (status == 1) {
                onlineUsers.put(user, Boolean.TRUE);
            } else {
                onlineUsers.remove(user);
            }
        }
    }

    public boolean isOnLine(String user) {
        return onlineUsers.containsKey(user);
    }

    public List<String> getAllOnLine() {
        return new ArrayList<>(onlineUsers.keySet());
    }

    @Override
    public void stop() throws LifecycleException {
        if (server != null) {
            server.close();
        }
        super.stop();
    }

    @Override
    public void dispose() throws LifecycleException {
        if (server != null) {
            server.close();
        }
        super.dispose();
    }
}
