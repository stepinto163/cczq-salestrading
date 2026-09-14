 
package com.gateway.connector.api;

import com.gateway.connector.Connection;
import com.gateway.connector.Session;
 
public abstract class ExchangeConnection<T> implements Connection<T> {

    protected Session session = null;
    protected String connectionId = null;

    protected volatile boolean close = false;
    protected int connectTimeout = 60 * 60 * 1000; // ms

    public void fireError(RuntimeException e) {
        throw e;
    }

    public boolean isClosed() {
        return close;
    }

    public void setConnectionId(String connectionId) {
        this.connectionId = connectionId;
    }

    public String getConnectionId() {
        return connectionId;
    }

    public void setSession(Session session) {
        this.session = session;
    }

    public Session getSession() {
        return session;
    }

}
