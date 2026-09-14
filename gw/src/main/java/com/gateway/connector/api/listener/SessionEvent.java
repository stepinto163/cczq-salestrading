 
package com.gateway.connector.api.listener;

import java.util.EventObject;

import com.gateway.connector.Session;

 
public class SessionEvent extends EventObject {

    /**
     * Constructs a prototypical Event.
     *
     * @param source The object on which the Event initially occurred.
     * @throws IllegalArgumentException if source is null.
     */
    public SessionEvent(Object source) {
        super(source);
    }

    /**
     * Return the session that changed.
     */
    public Session getSession() {
        return (Session) super.getSource();
    }
}
