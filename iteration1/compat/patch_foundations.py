"""Small, reviewable adapters for the supplied STC/DPS source revisions."""
from pathlib import Path
import difflib

def replace_once(text, old, new):
    if text.count(old)!=1:
        raise ValueError('Compatibility patch anchor changed: '+old[:90])
    return text.replace(old,new,1)

def patch(stage, src, patches):
    patches.mkdir(parents=True,exist_ok=True)
    edits={}
    def edit(relative,transform):
        path=src/relative
        before=path.read_text(encoding='utf-8') if path.exists() else ''
        after=transform(before)
        path.parent.mkdir(parents=True,exist_ok=True)
        path.write_text(after,encoding='utf-8')
        edits[relative]=(before,after)
    if stage=='dps-api-java':
        edit('com/gateway/connector/tcp/client/IMessageByte.java',lambda old: '''package com.gateway.connector.tcp.client;

/** Optional notification interception. Bytes are the complete decoded DPS envelope.
 * Treat bytes as read-only. Return true to continue ordinary string delivery;
 * return false when this callback has consumed the message. No caching/coalescing.
 */
@FunctionalInterface
public interface IMessageByte {
    boolean onMessage(String topic, byte[] body);
}
''')
        def gateway(text):
            text=replace_once(text,'public class GateWayApi {','''public class GateWayApi {
    private volatile IMessageByte notifyByteFilter;

    public void setNotifyByteFilter(IMessageByte filter) {
        notifyByteFilter = filter;
    }

    private boolean continueNotify(String topic, byte[] body) {
        IMessageByte filter = notifyByteFilter;
        if (filter == null) return true;
        try {
            return filter.onMessage(topic, body);
        } catch (RuntimeException error) {
            logger.error("Notify byte filter failed; falling back to ordinary delivery", error);
            return true;
        }
    }
''')
            text=replace_once(text,'\t\t\t\t\tonMessage(topic, content);','\t\t\t\t\tif (continueNotify(topic, body)) onMessage(topic, content);')
            return text
        edit('com/gateway/connector/tcp/client/GateWayApi.java',gateway)
        edit('com/gateway/connector/tcp/server/MessageWrapper.java',lambda text:replace_once(text,'    private Object body;','''    private Object body;
    // Compatibility accessor already present in cached origin/master (042d428).
    private java.util.Map<String, Object> bodyMap;
    public java.util.Map<String, Object> getBodyMap() { return bodyMap; }
    public void setBodyMap(java.util.Map<String, Object> bodyMap) { this.bodyMap = bodyMap; }
'''))
        def topic_index(text):
            text=replace_once(text,'public class TopicManager {','''public class TopicManager {
    private final ConcurrentHashMap<String, CopyOnWriteArraySet<String>> topicHm2 = new ConcurrentHashMap<>();
    public ConcurrentHashMap<String, CopyOnWriteArraySet<String>> getTopics2() { return topicHm2; }
    private void removeExactIndex(String sessionId, String topic) {
        CopyOnWriteArraySet<String> sessions = topicHm2.get(topic);
        if (sessions != null) {
            sessions.remove(sessionId);
            if (sessions.isEmpty()) topicHm2.remove(topic, sessions);
        }
    }
''')
            text=replace_once(text,'\t\t\t\tcas.add(topic);','\t\t\t\tcas.add(topic);\n                if (!isWildcard) topicHm2.computeIfAbsent(topic, key -> new CopyOnWriteArraySet<>()).add(sessionid);')
            text=replace_once(text,'\t\t\tcas.remove(topic);','\t\t\tcas.remove(topic);\n            if (!isWildcard) removeExactIndex(sessionid, topic);')
            text=replace_once(text,'\t\ttopicHm.remove(sessionid);','''        CopyOnWriteArraySet<String> exact = topicHm.get(sessionid);
        if (exact != null) for (String topic : exact) removeExactIndex(sessionid, topic);
        topicHm.remove(sessionid);''')
            return text
        edit('com/gateway/connector/tcp/server/TopicManager.java',topic_index)
    if stage=='sc-app-common':
        edit('com/gw/common/utils/ServerGwWrapper.java',lambda text:replace_once(text,
            '\t\tats.bbox.framework.Consts.isShowActiveThread = false;',
            '\t\t// Supplied BBox version has no active-thread diagnostic switch.'))
    if stage=='sc-app-stc':
        edit('sc/app/stc/net/ServerCedaWrapper.java',lambda text:replace_once(text,
            '\t\tats.bbox.framework.Consts.isShowActiveThread = false;',
            '\t\t// Supplied BBox version has no active-thread diagnostic switch.'))
        edit('sc/app/stc/net/NetClientWrapper.java',lambda text:replace_once(text,
            'cedaClientWrapper.requestAsync(serverName, message, messageListener);',
            'cedaClientWrapper.requestAsyn(serverName, message, messageListener);'))
    if stage=='gw':
        # Both revisions encode identical {topic,content} envelopes; only Java return type differs.
        for relative in ('com/gateway/invoke/ceda/CedaApi.java','com/gateway/notify/NotifyProxy.java'):
            def notify_map(text):
                text=text.replace('import com.gateway.connector.tcp.body.NotifyBody;','')
                text=text.replace('com.gateway.connector.tcp.body.NotifyBody notifyBody','java.util.Map<String,String> notifyBody')
                text=text.replace('NotifyBody notifyBody','java.util.Map<String,String> notifyBody')
                text=text.replace('notifyBody.getContent()', 'notifyBody.get("content")')
                text=text.replace('notifyBody.content', 'notifyBody.get("content")')
                return text
            edit(relative,notify_map)
    diffs=[]
    for relative,(before,after) in edits.items():
        diffs.extend(difflib.unified_diff(before.splitlines(True),after.splitlines(True),fromfile='a/src/main/java/'+relative,tofile='b/src/main/java/'+relative))
    (patches/(stage+'.patch')).write_text(''.join(diffs),encoding='utf-8')
