package com.gateway.schedule;

import com.gateway.invoke.ceda.CedaApiProxy;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

public class GWUserListReloadTask {
    private static final Logger logger = LoggerFactory.getLogger(GWUserListReloadTask.class);

    public CedaApiProxy cedaProxy;

    public void reloadUserList() {
        logger.warn("reloadUserList start");
        cedaProxy.loadUserList();
    }

    public CedaApiProxy getCedaProxy() {
        return cedaProxy;
    }

    public void setCedaProxy(CedaApiProxy cedaProxy) {
        this.cedaProxy = cedaProxy;
    }
}
