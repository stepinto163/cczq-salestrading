package ats.app.api;

import ats.app.ym.ceda.ICedaConnectionListener;
import ats.foundation.utils.exception.CodeException;
import ats.message2.CharsetConvertType;
import ats.message2.MsgHelper;

import sc.app.stc.net.NetClientWrapper;
import sc.app.stc.utils.Consts;

import java.util.Collections;
import java.util.IdentityHashMap;
import java.util.List;
import java.util.Map;

/** Compatibility entry points used by the existing STC data clients. Transport is DPS only. */
public class BaseApi {
    public static String sid = "BaseApi";
    public static long timeTime = 30000L;
    public static NetClientWrapper cedaClientWrapper;
    protected static int connectionTimeout = 5000;
    private static final Map<IMessage<?>, IMessage<String>> callbacks =
            Collections.synchronizedMap(new IdentityHashMap<>());

    @FunctionalInterface
    public interface IMessage<T> {
        void onMessage(String topic, T value);
    }

    public static int getConnectionTimeout() {
        return connectionTimeout;
    }

    public static void setConnectionTimeout(int millis) {
        connectionTimeout = millis;
    }

    public static boolean isOnline(String server) {
        cedaClientWrapper.init(server);
        return cedaClientWrapper.isOnline(server);
    }

    public static void addCedaClientConnectListener(
            String server, ICedaConnectionListener listener) {
        cedaClientWrapper.addCedaClientConnectListener(server, listener);
    }

    public static <T> List<T> requestSync(
            String server,
            String topic,
            String content,
            Class<T> type,
            boolean list,
            boolean checkJson,
            boolean deserialize,
            boolean log)
            throws CodeException {
        try {
            // Preserve the deployed client's connection wait and request timeout, without retrying
            // writes.
            int waited = 0;
            while (!isOnline(server)) {
                if (++waited > 20) {
                    throw new CodeException(Consts.TimeOut, Consts.TimeOutMsg);
                }
                Thread.sleep(1000);
            }
            if (waited > 0) {
                Thread.sleep(2000);
            }
            String reply = cedaClientWrapper.requestSync(server, sid, topic, content, 30000);
            return decode(reply, type, list, deserialize);
        } catch (CodeException error) {
            throw error;
        } catch (Exception error) {
            if (error instanceof InterruptedException) {
                Thread.currentThread().interrupt();
            }
            CodeException failure = new CodeException(Consts.NoKnowCode, error.getMessage());
            failure.initCause(error);
            throw failure;
        }
    }

    public static <T> List<T> requestSync(
            String server,
            String topic,
            String content,
            Class<T> type,
            boolean list,
            boolean checkJson,
            boolean deserialize)
            throws CodeException {
        return requestSync(server, topic, content, type, list, checkJson, deserialize, true);
    }

    @SuppressWarnings("unchecked")
    private static <T> List<T> decode(
            String reply, Class<T> type, boolean list, boolean deserialize) throws Exception {
        if (!deserialize) {
            return Collections.singletonList((T) reply);
        }
        if (list) {
            return MsgHelper.Deserialize2(reply, type, CharsetConvertType.None);
        }
        return Collections.singletonList(
                MsgHelper.Deserialize(reply, type, CharsetConvertType.None));
    }

    public static <T> void requestAsnc(
            String server,
            String topic,
            String content,
            Class<T> type,
            boolean list,
            boolean checkJson,
            boolean deserialize,
            IMessage<List<T>> callback,
            boolean log)
            throws CodeException {
        try {
            cedaClientWrapper.requestAsync(
                    server,
                    topic,
                    content,
                    sid,
                    (replyTopic, reply) -> {
                        try {
                            callback.onMessage(replyTopic, decode(reply, type, list, deserialize));
                        } catch (Exception error) {
                            throw new IllegalStateException(
                                    "Cannot decode DPS reply: " + topic, error);
                        }
                    });
        } catch (Exception error) {
            CodeException failure = new CodeException(Consts.NoKnowCode, error.getMessage());
            failure.initCause(error);
            throw failure;
        }
    }

    public static <T> void requestAsnc(
            String server,
            String topic,
            String content,
            Class<T> type,
            boolean list,
            boolean checkJson,
            boolean deserialize,
            IMessage<List<T>> callback)
            throws CodeException {
        requestAsnc(server, topic, content, type, list, checkJson, deserialize, callback, true);
    }

    public static void subscribe(String server, String topic, IMessage<String> callback) {
        cedaClientWrapper.subscribe(server, topic, callback);
    }

    public static void unSubscribe(String server, String topic, IMessage<String> callback) {
        cedaClientWrapper.unSubscribe(server, topic, callback);
    }

    public static void subscribeWithImage(
            String server, String imageServer, String topic, IMessage<String> callback) {
        cedaClientWrapper.subscribeWithImage(server, topic, imageServer, sid, callback);
    }

    public static <T> void subscribeWithImage(
            String server,
            String imageServer,
            String topic,
            IMessage<T> callback,
            Class<T> type,
            boolean... debug) {
        IMessage<String> wrapped =
                (receivedTopic, content) -> {
                    try {
                        if (content == null || content.trim().isEmpty()) {
                            return;
                        }
                        for (T value : decode(content, type, content.startsWith("["), true)) {
                            callback.onMessage(receivedTopic, value);
                        }
                    } catch (Exception error) {
                        throw new IllegalStateException(
                                "Cannot decode DPS snapshot: " + receivedTopic, error);
                    }
                };
        callbacks.put(callback, wrapped);
        subscribeWithImage(server, imageServer, topic, wrapped);
    }

    public static <T> void unSubscribeWithImage(
            String server, String imageServer, String topic, IMessage<T> callback, Class type) {
        unSubscribeWithImage(server, imageServer, topic, callback);
    }

    public static <T> void unSubscribeWithImage(
            String server, String imageServer, String topic, IMessage<T> callback) {
        IMessage<String> wrapped = callbacks.remove(callback);
        if (wrapped != null) {
            cedaClientWrapper.unSubscribeWithImage(server, topic, imageServer, sid, wrapped);
        }
    }

    public static <T> void subscribe(
            String server,
            List<String> topics,
            IMessage<T> callback,
            Class<T> type,
            boolean... debug) {
        IMessage<String> wrapped =
                (topic, content) -> {
                    try {
                        callback.onMessage(
                                topic,
                                MsgHelper.Deserialize(content, type, CharsetConvertType.None));
                    } catch (Exception error) {
                        throw new IllegalStateException(
                                "Cannot decode DPS publication: " + topic, error);
                    }
                };
        callbacks.put(callback, wrapped);
        for (String topic : topics) {
            subscribe(server, topic, wrapped);
        }
    }
}
