 
package com.gateway.connector.api.listener;

import java.util.EventListener;

 
public interface SessionListener extends EventListener {

    /**
     * Notification that a session was created.
     *
     * @param se the notification event
     */
    void sessionCreated(SessionEvent se);

    /**
     * Notification that a session is about to be invalidated.
     *
     * @param se the notification event
     */
    void sessionDestroyed(SessionEvent se);
}
