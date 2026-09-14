 
package com.gateway.remoting;

 
import com.gateway.connector.tcp.TcpConnector;
import com.gateway.message.MessageWrapper;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

public class TcpSender implements Sender {

    private final static Logger logger = LoggerFactory.getLogger(TcpSender.class);

    private TcpConnector tcpConnector;

//    public TcpSender(TcpConnector tcpConnector) {
//        this.tcpConnector = tcpConnector;
//    }

  

	public void setTcpConnector(TcpConnector tcpConnector) {
		this.tcpConnector = tcpConnector;
	}

	public String sendMessage(MessageWrapper wrapper) throws RuntimeException {
        try {
            return tcpConnector.send(wrapper.getSessionId(), wrapper.getBody());
        } catch (Exception e) {
            logger.error("TcpSender sendMessage occur Exception!", e);
            throw new RuntimeException(e.getCause());
        }
    }

    public boolean existSession(MessageWrapper wrapper) throws RuntimeException {
        try {
            return tcpConnector.exist(wrapper.getSessionId());
        } catch (Exception e) {
            logger.error("TcpSender sendMessage occur Exception!", e);
            throw new RuntimeException(e.getCause());
        }
    }

}
