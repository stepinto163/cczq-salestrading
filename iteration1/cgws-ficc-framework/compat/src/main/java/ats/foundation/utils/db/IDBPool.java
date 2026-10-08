package ats.foundation.utils.db;

import java.sql.Connection;
import java.sql.SQLException;

/** Legacy resource signature retained for XML-created components. */
public interface IDBPool {
    Connection getConnection() throws SQLException;

    Connection getConnection(String datasource) throws SQLException;

    Connection getLongConnection() throws SQLException;

    Connection getLongConnection(String datasource) throws SQLException;

    void freeConnection(Connection connection) throws SQLException;

    void freeConnection(String datasource, Connection connection) throws SQLException;

    void closeAll();
}
