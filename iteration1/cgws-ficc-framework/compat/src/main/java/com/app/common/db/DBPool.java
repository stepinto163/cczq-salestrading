// Recovered compatibility source; provenance: recovered-support.json.
package com.app.common.db;

import com.alibaba.druid.pool.DruidDataSource;
import com.alibaba.druid.pool.DruidDataSourceFactory;
import com.alibaba.druid.pool.DruidPooledConnection;
import com.app.common.utils.StringUtil;

import org.apache.commons.lang3.StringUtils;

import java.sql.Connection;
import java.sql.DriverManager;
import java.sql.SQLException;
import java.util.ArrayList;
import java.util.Arrays;
import java.util.HashMap;
import java.util.Hashtable;
import java.util.List;
import java.util.Map;
import java.util.Properties;

import javax.sql.DataSource;

public class DBPool implements IDBPool {
    private static DBPool _oInstance = null;
    private String _configPath = null;
    private String _DefName = "ATS_ORACLE";
    private DruidDataSource _DefDBSource;
    private Hashtable<String, Properties> _DBParamTable = new Hashtable();
    private final Hashtable<String, DruidDataSource> _DBConnPool = new Hashtable();
    private DBPoolParam _DBPoolParam = null;
    private final HashMap<String, DBPoolParam> dbPoolParam = new HashMap();

    public static synchronized DBPool instance(String dbSourceName, String configPath)
            throws Exception {
        if (_oInstance == null) {
            _oInstance = new DBPool(dbSourceName, configPath);
            if (DBPool._oInstance._DefDBSource == null) {
                _oInstance = null;
                throw new Exception(
                        String.format("Can't find DBSource{%s} from DBPool.", dbSourceName));
            }
        }
        return _oInstance;
    }

    public DBPool() {}

    public DBPool(String dbSourceName, String configPath) throws Exception {
        this.init(dbSourceName, configPath);
    }

    public void init(String dbSourceName, String configPath) throws Exception {
        this._DefName = dbSourceName;
        this._configPath = configPath;
        List<DBPoolParam> list = null;
        try {
            list = DBUtils.readConfig(this._configPath);
            if (list == null || list.size() == 0) {
                throw new Exception("Not find any config about DBParams...");
            }
            this.cloneDBPoolParam(list);
        } catch (Exception e) {
            e.printStackTrace();
            throw e;
        }
        this.initDBPool(list, new String[] {dbSourceName});
    }

    private void initDBPool(List<DBPoolParam> params, String[] sourceNames) {
        List<Object> tempSourceNames = new ArrayList();
        if (sourceNames != null) {
            tempSourceNames = Arrays.asList(sourceNames);
        }
        for (DBPoolParam po : params) {
            Properties ps = new Properties();
            ps.put("driverClassName", po.Driver);
            ps.put("url", po.Url);
            ps.put("username", po.Username);
            ps.put("password", po.Passwd);
            ps.put("maxActive", Integer.toString(po.MaxCount));
            ps.put("minIdle", Integer.toString(po.MinCount));
            ps.put("initialSize", Integer.toString(po.MinCount / 2));
            if (po.CheckSql != null || "".equalsIgnoreCase(po.CheckSql)) {
                ps.put("validationQuery", po.CheckSql);
                ps.put("testWhileIdle", "true");
                ps.put("testOnBorrow", "false");
            }
            ps.put("filters", "stat");
            ps.put("removeAbandoned", "true");
            ps.put("removeAbandonedTimeout", Integer.toString(po.DBConnOutTime / 1000));
            ps.put("logAbandoned", "true");
            ps.put("maxWait", "60000");
            ps.put("timeBetweenEvictionRunsMillis", Integer.toString(po.DBConnCheckNumber * 1000));
            ps.put("minEvictableIdleTimeMillis", Integer.toString(po.DBConnCheckNumber * 1000));
            ps.put("numTestsPerEvictionRun", "10");
            this._DBParamTable.put(po.SourceName, ps);
            if (tempSourceNames.indexOf(po.SourceName) < 0) continue;
            this.initOneDataSource(ps, po.SourceName, true);
        }
    }

    private synchronized void initOneDataSource(
            Properties ps, String dbSourceName, boolean isGetConn) {
        if (ps != null) {
            DruidDataSource basicDS = null;
            try {
                basicDS =
                        (DruidDataSource) DruidDataSourceFactory.createDataSource((Properties) ps);
                if (isGetConn) {
                    try (DruidPooledConnection connection = basicDS.getConnection()) {
                        // Opening the connection verifies the configured default datasource.
                    }
                }
                this._DBConnPool.put(dbSourceName, basicDS);
                if (this._DefName.equalsIgnoreCase(dbSourceName)) {
                    this._DefDBSource = basicDS;
                }
            } catch (Exception exception) {
                if (basicDS != null) {
                    basicDS.close();
                }
                throw new IllegalStateException(
                        "Cannot initialize datasource: " + dbSourceName, exception);
            }
        }
    }

    public String get_configPath() {
        return this._configPath;
    }

    public void set_configPath(String _configPath) {
        this._configPath = _configPath;
    }

    @Override
    public Connection getConnection() throws SQLException {
        return this.getConnection(this._DefName);
    }

    @Override
    public Connection getConnection(String sourceName) throws SQLException {
        DruidPooledConnection con = null;
        if (!this._DBConnPool.containsKey(sourceName)
                && this._DBParamTable.containsKey(sourceName)) {
            Properties ps = this._DBParamTable.get(sourceName);
            this.initOneDataSource(ps, sourceName, false);
        }
        if (this._DBConnPool.containsKey(sourceName)) {
            DruidDataSource basicDS = this._DBConnPool.get(sourceName);
            con = this._DBConnPool.get(sourceName).getConnection();
        }
        if (con == null) {
            throw new SQLException("Datasource is not configured: " + sourceName);
        }
        return con;
    }

    @Override
    public DruidDataSource getDruidDataSource(String sourceName) {
        DruidDataSource basicDS = null;
        if (this._DBConnPool.containsKey(sourceName)) {
            basicDS = this._DBConnPool.get(sourceName);
        }
        return basicDS;
    }

    @Override
    public DataSource getDataSource(String sourceName) {
        DruidDataSource basicDS = null;
        if (this._DBConnPool.containsKey(sourceName)) {
            basicDS = this._DBConnPool.get(sourceName);
        }
        return basicDS;
    }

    @Override
    public void freeConnection(Connection conn) throws SQLException {
        this.freeConnection(this._DefName, conn);
    }

    @Override
    public void freeConnection(String sourceName, Connection conn) throws SQLException {
        if (conn != null) {
            try {
                conn.close();
            } catch (Exception exception) {
                // empty catch block
            }
            conn = null;
        }
    }

    @Override
    public void closeAll() {
        RuntimeException failure = null;
        for (Map.Entry<String, DruidDataSource> kv : this._DBConnPool.entrySet()) {
            if (kv.getValue() == null) continue;
            try {
                kv.getValue().close();
            } catch (Exception exception) {
                if (failure == null) {
                    failure =
                            new IllegalStateException("Could not close database pools", exception);
                } else {
                    failure.addSuppressed(exception);
                }
            }
        }
        this._DBConnPool.clear();
        synchronized (DBPool.class) {
            if (_oInstance == this) {
                _oInstance = null;
            }
        }
        if (failure != null) {
            throw failure;
        }
    }

    private void cloneDBPoolParam(List<DBPoolParam> list) throws Exception {
        String partyDBPwd = PartyDBPwdManager.getInstance().getPartyDBPwd();
        for (DBPoolParam param : list) {
            String string =
                    param.Passwd = StringUtil.isNotEmpty(partyDBPwd) ? partyDBPwd : param.Passwd;
            if (this._DefName.equalsIgnoreCase(param.SourceName)) {
                this._DBPoolParam = param;
                continue;
            }
            DBPoolParam dbParams = this.cloneDBPoolParam(param);
            this.dbPoolParam.put(dbParams.SourceName, dbParams);
        }
    }

    private DBPoolParam cloneDBPoolParam(DBPoolParam poolParam) throws SecurityException {
        DBPoolParam dbParams = new DBPoolParam();
        dbParams.SourceName = poolParam.SourceName;
        dbParams.Driver = poolParam.Driver;
        dbParams.Url = poolParam.Url;
        dbParams.Username = poolParam.Username;
        dbParams.Passwd = poolParam.Passwd;
        dbParams.MaxCount = poolParam.MaxCount;
        dbParams.MinCount = poolParam.MinCount;
        dbParams.CheckSql = poolParam.CheckSql;
        dbParams.IsEncrypt = poolParam.IsEncrypt;
        dbParams.DBConnOutTime = poolParam.DBConnOutTime;
        dbParams.DBConnCheckNumber = poolParam.DBConnCheckNumber;
        dbParams.DBStrategy = poolParam.DBStrategy;
        return dbParams;
    }

    @Override
    public Connection getLongConnection() throws SQLException {
        return this.getLongConn0(this._DBPoolParam.SourceName);
    }

    @Override
    public Connection getLongConnection(String sourceName) throws SQLException {
        return this.getLongConn0(sourceName);
    }

    private Connection getLongConn0(String sourceName) throws SQLException {
        DBPoolParam dbParams = this._DBPoolParam;
        String name = dbParams.SourceName;
        if (!name.equals(sourceName) && (dbParams = this.dbPoolParam.get(sourceName)) == null) {
            throw new SQLException(
                    "Can't get long connection - unknown db source name " + sourceName);
        }
        return DriverManager.getConnection(dbParams.Url, dbParams.Username, dbParams.Passwd);
    }

    public DBPoolParam getDBPoolParam() {
        return this._DBPoolParam;
    }

    public DBPoolParam getDBPoolParam(String sourceName) {
        if (StringUtils.isNotBlank((CharSequence) sourceName)
                && this.dbPoolParam.containsKey(sourceName)) {
            return this.dbPoolParam.get(sourceName);
        }
        return null;
    }
}
