 
package com.gateway.connector.api.listener;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

 
public class LogSessionListener implements SessionListener {

    private final static Logger logger = LoggerFactory.getLogger(LogSessionListener.class);

    public void sessionCreated(SessionEvent se) {
        logger.info("session " + se.getSession().getSessionId() + " have been created!");
    }

    public void sessionDestroyed(SessionEvent se) {
        logger.info("session " + se.getSession().getSessionId() + " have been destroyed!");
    }
}
