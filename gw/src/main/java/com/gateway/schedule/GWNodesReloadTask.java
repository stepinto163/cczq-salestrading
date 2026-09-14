package com.gateway.schedule;

import com.gateway.invoke.ceda.CedaApi;
import com.gateway.invoke.ceda.CedaProxy;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

public class GWNodesReloadTask {

    private static Logger logger = LoggerFactory.getLogger(GWNodesReloadTask.class);

    public CedaProxy cedaProxy;

    public void reloadNodes() {
        logger.warn("reloadNodes start");
        cedaProxy.reloadNodes();
    }

    public CedaProxy getCedaProxy() {
        return cedaProxy;
    }

    public void setCedaProxy(CedaProxy cedaProxy) {
        this.cedaProxy = cedaProxy;
    }
}
