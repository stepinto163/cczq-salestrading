// Recovered compatibility source; provenance: recovered-support.json.
package com.app.common.db;

import com.alibaba.druid.pool.DruidDataSource;

import java.sql.Connection;
import java.sql.SQLException;

import javax.sql.DataSource;

public interface IDBPool {
    public Connection getConnection() throws SQLException;

    public Connection getConnection(String var1) throws SQLException;

    @Deprecated
    public void freeConnection(Connection var1) throws SQLException;

    @Deprecated
    public void freeConnection(String var1, Connection var2) throws SQLException;

    public void closeAll();

    public Connection getLongConnection(String var1) throws SQLException;

    public Connection getLongConnection() throws SQLException;

    public DruidDataSource getDruidDataSource(String var1);

    public DataSource getDataSource(String var1);
}
