 
package com.gateway.invoke;

import com.gateway.connector.proto.Proto;
import com.gateway.message.MessageWrapper;
import com.gateway.message.SystemMessage;

import ats.app.api.BaseApi.IMessage;

public interface ApiProxy {

    MessageWrapper invoke(SystemMessage sMsg, Proto message);
    MessageWrapper invokeAsync(SystemMessage sMsg, Proto message,IMessage<String> callback);

}

