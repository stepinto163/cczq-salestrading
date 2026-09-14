 
package com.gateway.remoting;

import com.gateway.message.MessageWrapper;

public interface Sender {

    String sendMessage(MessageWrapper wrapper) throws RuntimeException;

    boolean existSession(MessageWrapper wrapper) throws RuntimeException;

}
