
package com.gateway.common;

import com.gateway.connector.Connection;
import com.gateway.connector.SessionManager;
import com.gateway.connector.api.listener.SessionListener;

public interface Endpoint extends Node {

    /**
     * @param connection
     */
    void setConnection(Connection connection);

    Connection getConnection();

    /**
     * @param sessionId
     */
    void setSessionId(String sessionId);

    String getSessionId();

    String getUserName();

    void setUserName(String userName);

    String getNodeId();

    void setNodeId(String instId);

    int getMaxQueueSize();

    void setMaxQueueSize(int maxQueueSize);

    String getClientType();

    void setClientType(String clientType);

    String getInfo();

    void setInfo(String info);

    /**
     * @param sessionManager
     */
    void setSessionManager(SessionManager sessionManager);

    SessionManager getSessionManager();

    /**
     * Add a session event listener to this component.
     */
    void addSessionListener(SessionListener listener);

    /**
     * Remove a session event listener from this component.
     */
    void removeSessionListener(SessionListener listener);
}
