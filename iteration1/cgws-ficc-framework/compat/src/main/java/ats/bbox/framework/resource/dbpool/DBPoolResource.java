package ats.bbox.framework.resource.dbpool;

import ats.bbox.framework.LifecycleException;
import ats.bbox.framework.resource.ResourceAdapter;
import ats.foundation.utils.db.IDBPool;
import ats.foundation.utils.db.impl.DBPool;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import java.io.File;
import java.sql.Connection;
import java.sql.SQLException;

/** Keeps the legacy DB resource API; connection management remains with the supplied DB library. */
public class DBPoolResource extends ResourceAdapter implements IDataBasePool, IDBPool {
    private static final Logger LOGGER = LoggerFactory.getLogger(DBPoolResource.class);
    private IDBPool pool;

    public void init() throws LifecycleException {
        String file = getConfig().getString("dbpool.cfg");
        String datasource = getConfig().getString("dbpool.default");
        if (file == null || datasource == null || !new File(file).isFile()) {
            throw new LifecycleException(
                    "dbpool.cfg must exist and dbpool.default must be configured");
        }
        try {
            pool = DBPool.instance(datasource, file);
            super.init();
        } catch (Exception error) {
            throw new LifecycleException(error);
        }
    }

    public void dispose() throws LifecycleException {
        try {
            if (pool != null) {
                pool.closeAll();
            }
        } finally {
            pool = null;
            super.dispose();
        }
    }

    private IDBPool requirePool() throws SQLException {
        if (pool == null) {
            throw new SQLException("DBPoolResource has not been initialized");
        }
        return pool;
    }

    public Connection getConnection() throws SQLException {
        return requirePool().getConnection();
    }

    public Connection getConnection(String datasource) throws SQLException {
        return requirePool().getConnection(datasource);
    }

    public Connection getLongConnection() throws SQLException {
        return requirePool().getLongConnection();
    }

    public Connection getLongConnection(String datasource) throws SQLException {
        return requirePool().getLongConnection(datasource);
    }

    public void freeConnection(Connection connection) throws SQLException {
        if (pool != null) {
            pool.freeConnection(connection);
        }
    }

    public void freeConnection(String datasource, Connection connection) throws SQLException {
        if (pool != null) {
            pool.freeConnection(datasource, connection);
        }
    }

    public void closeAll() {
        // Legacy callers do not own this resource; the container releases it through dispose().
        LOGGER.info("DBPoolResource closeAll ignored; lifecycle dispose owns pool shutdown");
    }
}
