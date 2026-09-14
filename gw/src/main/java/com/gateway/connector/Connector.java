 
package com.gateway.connector;

 
public interface Connector<T> {

    void init();

    void destroy();

    String send(String sessionId, T message) throws Exception;

    boolean exist(String sessionId) throws Exception;

}
