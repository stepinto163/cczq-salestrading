 
package com.gateway.connector.api;

import com.gateway.connector.Connector;
import com.gateway.connector.Session;
import com.gateway.connector.SessionManager;
import com.gateway.exception.DispatchException;
import com.gateway.exception.PushException;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

 
public abstract class ExchangeConnector<T> implements Connector<T> {

    private final static Logger logger = LoggerFactory.getLogger(ExchangeConnector.class);

    public String send(SessionManager sessionManager, String sessionId, T message) throws Exception {
        Session session = sessionManager.getSession(sessionId);
        String userName=null;
        if (session == null) {
            throw new Exception(String.format("session %s no exist.", sessionId));
        }
        try {
            session.getConnection().send(message);
            userName=session.getUserName();
        } catch (PushException e) {
            logger.warn("ExchangeConnector send occur PushException.", e);
            session.close();
            throw new DispatchException(e);
        } catch (Exception e) {
            logger.warn("ExchangeConnector send occur Exception.", e);
            session.close();
            throw new DispatchException(e);
        }
        return userName;
    }
    public boolean isWritable(SessionManager sessionManager, String sessionId) throws Exception {
        Session session = sessionManager.getSession(sessionId);
        if (session == null) {
            throw new Exception(String.format("session %s no exist.", sessionId));
        }
        return session.getConnection().isWritable();
    }
}
