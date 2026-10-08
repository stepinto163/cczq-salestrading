// Recovered compatibility source; provenance: recovered-support.json.
package com.app.common.db;

import ats.bbox.framework.ILifecycle;
import ats.bbox.framework.LifecycleException;
import ats.bbox.framework.resource.IResource;
import ats.bbox.framework.resource.ResourceAdapter;
import ats.bbox.framework.resource.dbpool.IDataBasePool;

import com.alibaba.druid.pool.DruidDataSource;

import org.apache.commons.configuration.AbstractConfiguration;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import java.io.File;
import java.sql.Connection;
import java.sql.SQLException;

import javax.sql.DataSource;

public class DBPoolResource implements IResource, IDataBasePool, IDBPool {
    private ResourceAdapter resourceAdapter = new ResourceAdapter();
    private Logger logger =
            LoggerFactory.getLogger(ats.bbox.framework.resource.dbpool.DBPoolResource.class);
    private DBPool dbPoolAdapter = null;

    @Override
    public Connection getLongConnection(String sourceName) throws SQLException {
        return this.dbPoolAdapter.getLongConnection(sourceName);
    }

    @Override
    public Connection getLongConnection() throws SQLException {
        return this.dbPoolAdapter.getLongConnection();
    }

    @Override
    public DruidDataSource getDruidDataSource(String sourceName) {
        return this.dbPoolAdapter.getDruidDataSource(sourceName);
    }

    @Override
    public DataSource getDataSource(String sourceName) {
        return this.dbPoolAdapter.getDataSource(sourceName);
    }

    public AbstractConfiguration getConfig() {
        return this.resourceAdapter.getConfig();
    }

    public String getName() {
        return this.resourceAdapter.getName();
    }

    public void setConfig(AbstractConfiguration config) {
        this.resourceAdapter.setConfig(config);
    }

    public void setName(String name) {
        this.resourceAdapter.setName(name);
    }

    public void dispose() throws LifecycleException {
        try {
            if (dbPoolAdapter != null) {
                dbPoolAdapter.closeAll();
            }
        } finally {
            dbPoolAdapter = null;
            this.resourceAdapter.dispose();
        }
    }

    public ILifecycle.STATUS getStatus() {
        return this.resourceAdapter.getStatus();
    }

    public void init() throws LifecycleException {
        String dbPoolCfgString = this.getConfig().getString("dbpool.cfg");
        String dbDefaultName = this.getConfig().getString("dbpool.default");
        if (dbPoolCfgString != null && dbDefaultName != null) {
            if (!new File(dbPoolCfgString).exists()) {
                this.logger.error("DBpool config is not exists '{}'", (Object) dbPoolCfgString);
                throw new LifecycleException(
                        "No dbpool config, it's not exists " + dbPoolCfgString);
            }
            try {
                this.dbPoolAdapter = DBPool.instance(dbDefaultName, dbPoolCfgString);
            } catch (Exception e) {
                this.logger.error("Can't init dbpool", (Throwable) e);
                this.dbPoolAdapter = null;
                throw new LifecycleException((Throwable) e);
            }
        } else {
            throw new LifecycleException(
                    "Pls set dbpool's config or the default resource name of db pool ");
        }
        this.resourceAdapter.init();
    }

    public void start() throws LifecycleException {
        this.resourceAdapter.start();
    }

    public void stop() throws LifecycleException {
        this.resourceAdapter.stop();
    }

    @Override
    public void closeAll() {
        this.logger.info("Don't use this method, pls use dispose(). Do nothing");
    }

    @Override
    public void freeConnection(Connection conn) throws SQLException {
        if (this.dbPoolAdapter != null) {
            this.dbPoolAdapter.freeConnection(conn);
        } else {
            this.logger.warn("Pls init DBPoolResource first. Do nothing");
        }
    }

    @Override
    public void freeConnection(String sourceName, Connection conn) throws SQLException {
        if (this.dbPoolAdapter != null) {
            this.dbPoolAdapter.freeConnection(sourceName, conn);
        } else {
            this.logger.warn("Pls init DBPoolResource first. Do nothing");
        }
    }

    @Override
    public Connection getConnection() throws SQLException {
        if (this.dbPoolAdapter != null) {
            return this.dbPoolAdapter.getConnection();
        }
        throw new SQLException("Pls init DBPoolResource first");
    }

    @Override
    public Connection getConnection(String sourceName) throws SQLException {
        if (this.dbPoolAdapter != null) {
            return this.dbPoolAdapter.getConnection(sourceName);
        }
        throw new SQLException("Pls init DBPoolResource first");
    }

    public DBPoolParam getDBPoolParam(String sourceName) {
        return this.dbPoolAdapter.getDBPoolParam(sourceName);
    }

    public DBPoolParam getDBPoolParam() {
        return this.dbPoolAdapter.getDBPoolParam();
    }
}
