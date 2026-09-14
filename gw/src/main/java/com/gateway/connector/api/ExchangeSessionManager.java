 
package com.gateway.connector.api;

import java.util.List;
import java.util.Map;
import java.util.Set;
import java.util.concurrent.ConcurrentHashMap;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import com.gateway.connector.Session;
import com.gateway.connector.SessionManager;
import com.gateway.connector.api.listener.SessionListener;
import com.gateway.connector.tcp.server.TopicManager;

 
public abstract class ExchangeSessionManager implements SessionManager {

    private final static Logger logger = LoggerFactory.getLogger(ExchangeSessionManager.class);

    protected List<SessionListener> sessionListeners = null;
    private TopicManager topicManager;

	public TopicManager getTopicManager() {
		return topicManager;
	}

	public void setTopicManager(TopicManager topicManager) {
		this.topicManager = topicManager;
	}
    public void setSessionListeners(List<SessionListener> sessionListeners) {
        this.sessionListeners = sessionListeners;
    }

    /**
     * The set of currently active Sessions for this Manager, keyed by session
     * identifier.
     */
    protected Map<String, Session> apisessions = new ConcurrentHashMap<String, Session>();
    protected Map<String, Session> sessions = new ConcurrentHashMap<String, Session>();

    public synchronized void addSession(Session session) {
        if (null == session) {
            return;
        }
        sessions.put(session.getSessionId(), session);
        if((session.getClientType()+"").contains("api")) {
        	apisessions.put(session.getSessionId(), session);
        }
        logger.info("put a session " + session.getSessionId() + " to sessions!");
    }

 

    /**
     * Remove this Session from the active Sessions for this Manager.
     */
    public synchronized void removeSession(Session session) {
        if (session == null) {
            throw new IllegalArgumentException("session is null!");
        }
        removeSession(session.getSessionId());
    }

    public synchronized void removeSession(String sessionId) {
        sessions.remove(sessionId);
        apisessions.remove(sessionId);
        topicManager.clear(sessionId);
        logger.info("remove the session " + sessionId + " from sessions!");
    }

    public Session getSession(String sessionId) {
        return sessions.get(sessionId+"");
    }

    public Session[] getSessions() {
        return sessions.values().toArray(new Session[0]);
    }

    public Set<String> getSessionKeys() {
        return sessions.keySet();
    }

    public int getSessionCount() {
        return sessions.size();
    }
    public int getApiSessionCount() {
        return apisessions.size();
    }
    /**
     * define timeout 5min
     */
    private int maxInactiveInterval = 5 * 60;

    public int getMaxInactiveInterval() {
        return maxInactiveInterval;
    }

    public void setMaxInactiveInterval(int maxInactiveInterval) {
        this.maxInactiveInterval = maxInactiveInterval;
    }

}
