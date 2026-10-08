// Recovered compatibility source; provenance: recovered-support.json.
package com.app.common.db;

import com.alibaba.druid.pool.DruidDataSource;
import com.app.common.utils.StringUtil;

import org.apache.commons.lang3.StringUtils;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import java.io.FileInputStream;
import java.io.InputStream;
import java.lang.reflect.Field;
import java.math.BigDecimal;
import java.sql.Connection;
import java.sql.Date;
import java.sql.PreparedStatement;
import java.sql.ResultSet;
import java.sql.RowId;
import java.sql.SQLException;
import java.sql.Timestamp;
import java.util.ArrayList;
import java.util.Hashtable;
import java.util.List;
import java.util.Properties;

public class DBUtils {
    private static IDBPool dbpool;
    private static Logger logger;
    private static Hashtable<Class<?>, Integer> htDataType;
    public static int Max_Batch_Count;
    private static IDatabaseConnection databaseConnection;
    private static final String dbKey = "DBPOOL.";

    public static void init(String dbSourceName, String configPath) throws Exception {
        dbpool = DBPool.instance(dbSourceName, configPath);
    }

    public static IDatabaseConnection getDatabaseConnection() {
        return databaseConnection;
    }

    public static void setDatabaseConnection(IDatabaseConnection databaseConnection) {
        DBUtils.databaseConnection = databaseConnection;
    }

    public static DruidDataSource getDruidDataSource(String sourceName) {
        return dbpool.getDruidDataSource(sourceName);
    }

    public static Connection getConnectionBySourceName(String sourceName) throws SQLException {
        return dbpool.getConnection(sourceName);
    }

    public static void freeConnectionBySourceName(String sourceName, Connection connection)
            throws SQLException {
        dbpool.freeConnection(sourceName, connection);
    }

    public static Object querySimpleObject(
            String sql, Object[] params, Class clazz, Connection... connections) throws Exception {
        Connection connection = null;
        PreparedStatement preparedStatement = null;
        ResultSet resultSet = null;
        Object result = null;
        try {
            connection = DBUtils.getOrOpenConnection(connections);
            preparedStatement = connection.prepareStatement(sql);
            if (params != null) {
                for (int i = 0; i < params.length; ++i) {
                    preparedStatement.setObject(i + 1, params[i]);
                }
            }
            if ((resultSet = preparedStatement.executeQuery()).next()) {
                int type = htDataType.get(clazz);
                switch (type) {
                    case 4:
                        {
                            result = resultSet.getInt(1);
                            break;
                        }
                    case 1:
                    case 12:
                        {
                            result = resultSet.getString(1);
                            break;
                        }
                    case 2:
                        {
                            result = resultSet.getBigDecimal(1);
                            break;
                        }
                    case 93:
                        {
                            result = resultSet.getTimestamp(1);
                            break;
                        }
                    case 91:
                        {
                            result = resultSet.getDate(1);
                            break;
                        }
                    default:
                        {
                            result = resultSet.getObject(1);
                        }
                }
            }
        } catch (Exception e) {
            try {
                throw e;
            } catch (Throwable throwable) {
                DBUtils.close(
                        DBUtils.getToCloseConnection(connection, connections),
                        preparedStatement,
                        resultSet);
                throw throwable;
            }
        }
        DBUtils.close(
                DBUtils.getToCloseConnection(connection, connections),
                preparedStatement,
                resultSet);
        return result;
    }

    public static <T> List<T> querySimpleList(
            String sql, Object[] params, Class<T> clazz, Connection... connections)
            throws Exception {
        Connection connection = null;
        PreparedStatement preparedStatement = null;
        ResultSet resultSet = null;
        ArrayList<T> list = new ArrayList<T>();
        try {
            connection = DBUtils.getOrOpenConnection(connections);
            preparedStatement = connection.prepareStatement(sql);
            if (params != null) {
                for (int i = 0; i < params.length; ++i) {
                    preparedStatement.setObject(i + 1, params[i]);
                }
            }
            resultSet = preparedStatement.executeQuery();
            while (resultSet.next()) {
                Object result = null;
                int type = htDataType.get(clazz);
                switch (type) {
                    case 4:
                        {
                            result = resultSet.getInt(1);
                            break;
                        }
                    case 1:
                    case 12:
                        {
                            result = resultSet.getString(1);
                            break;
                        }
                    case 2:
                        {
                            result = resultSet.getBigDecimal(1);
                            break;
                        }
                    case 93:
                        {
                            result = resultSet.getTimestamp(1);
                            break;
                        }
                    case 91:
                        {
                            result = resultSet.getDate(1);
                            break;
                        }
                    default:
                        {
                            result = resultSet.getObject(1);
                        }
                }
                if (result == null) continue;
                list.add((T) result);
            }
        } catch (Exception e) {
            try {
                throw e;
            } catch (Throwable throwable) {
                DBUtils.close(
                        DBUtils.getToCloseConnection(connection, connections),
                        preparedStatement,
                        resultSet);
                throw throwable;
            }
        }
        DBUtils.close(
                DBUtils.getToCloseConnection(connection, connections),
                preparedStatement,
                resultSet);
        return list;
    }

    public static <T> List<T> queryList(
            String sql, Object[] params, Class<T> type, Connection... connections) {
        try {
            return DBUtils.queryListThrowsException(sql, params, type, connections);
        } catch (Exception e) {
            logger.error("queryList: {}, type={}", (Object) sql, type);
            logger.error("", (Throwable) e);
            return new ArrayList();
        }
    }

    public static <T> List<T> queryListThrowsException(
            String sql, Object[] params, Class<T> type, Connection... connections)
            throws Exception {
        return DBUtils.queryListThrowsException(sql, params, type, 100, connections);
    }

    public static <T> List<T> queryListThrowsException(
            String sql, Object[] params, Class<T> type, int fecthSize, Connection... connections)
            throws Exception {
        ArrayList<T> list = new ArrayList<T>();
        Connection connection = null;
        PreparedStatement preparedStatement = null;
        ResultSet resultSet = null;
        try {
            connection = DBUtils.getOrOpenConnection(connections);
            preparedStatement = connection.prepareStatement(sql);
            if (params != null) {
                for (int i = 0; i < params.length; ++i) {
                    preparedStatement.setObject(i + 1, params[i]);
                }
            }
            resultSet = preparedStatement.executeQuery();
            resultSet.setFetchSize(fecthSize);
            Field[] fields = type.getFields();
            while (resultSet.next()) {
                T obj = type.newInstance();
                for (Field field : fields) {
                    try {
                        Object val = null;
                        Integer jdbcType = htDataType.get(field.getType());
                        if (jdbcType == null) {
                            val = resultSet.getObject(field.getName());
                        } else {
                            switch (jdbcType) {
                                case 4:
                                    {
                                        val = resultSet.getInt(field.getName());
                                        break;
                                    }
                                case 1:
                                case 12:
                                    {
                                        val = resultSet.getString(field.getName());
                                        break;
                                    }
                                case 2:
                                    {
                                        val = resultSet.getBigDecimal(field.getName());
                                        break;
                                    }
                                case 93:
                                    {
                                        val = resultSet.getTimestamp(field.getName());
                                        break;
                                    }
                                case 91:
                                    {
                                        val = resultSet.getDate(field.getName());
                                        break;
                                    }
                                default:
                                    {
                                        val = resultSet.getObject(field.getName());
                                    }
                            }
                        }
                        field.set(obj, val);
                    } catch (Exception exception) {
                        // empty catch block
                    }
                }
                list.add(obj);
            }
        } catch (Exception e) {
            try {
                throw e;
            } catch (Throwable throwable) {
                DBUtils.close(
                        DBUtils.getToCloseConnection(connection, connections),
                        preparedStatement,
                        resultSet);
                throw throwable;
            }
        }
        DBUtils.close(
                DBUtils.getToCloseConnection(connection, connections),
                preparedStatement,
                resultSet);
        return list;
    }

    public static <T> int updateObject(
            T obj, String table, String[] keys, Connection... connections) throws Exception {
        return DBUtils.updateObject(obj, obj.getClass(), table, keys, new Connection[0]);
    }

    public static <T> int updateObject(
            T obj, Class<?> class1, String table, String[] keys, Connection... connections)
            throws Exception {
        DynamicSql dynamicSql = DynamicSql.generateUpdate(class1, table, keys);
        Connection connection = null;
        PreparedStatement preparedStatement = null;
        try {
            connection = DBUtils.getOrOpenConnection(connections);
            preparedStatement = connection.prepareStatement(dynamicSql.sql);
            DBUtils.setValues(obj, preparedStatement, dynamicSql.fields, dynamicSql.keys);
            int n = preparedStatement.executeUpdate();
            return n;
        } catch (Exception e) {
            throw e;
        } finally {
            DBUtils.close(
                    DBUtils.getToCloseConnection(connection, connections), preparedStatement, null);
        }
    }

    public static <T> int[] updateObjectList(
            List<T> list, String table, String[] keys, Connection... connections) throws Exception {
        if (list.size() <= 0) {
            return null;
        }
        T obj = list.get(0);
        return DBUtils.updateObjectList(list, obj.getClass(), table, keys, connections);
    }

    public static <T> int[] updateObjectList(
            List<T> list, Class<?> class1, String table, String[] keys, Connection... connections)
            throws Exception {
        int[] object;
        DynamicSql dynamicSql = DynamicSql.generateUpdate(class1, table, keys);
        Connection connection = null;
        PreparedStatement preparedStatement = null;
        try {
            UpdateListResult listResult = new UpdateListResult(list.size());
            connection = DBUtils.getOrOpenConnection(connections);
            preparedStatement = connection.prepareStatement(dynamicSql.sql);
            int count = 0;
            for (T item : list) {
                DBUtils.setValues(item, preparedStatement, dynamicSql.fields, dynamicSql.keys);
                preparedStatement.addBatch();
                if (++count % Max_Batch_Count != 0) continue;
                listResult.add(preparedStatement.executeBatch());
            }
            listResult.add(preparedStatement.executeBatch());
            object = listResult.getResults();
        } catch (Exception e) {
            try {
                throw e;
            } catch (Throwable throwable) {
                DBUtils.close(
                        DBUtils.getToCloseConnection(connection, connections),
                        preparedStatement,
                        null);
                throw throwable;
            }
        }
        DBUtils.close(
                DBUtils.getToCloseConnection(connection, connections), preparedStatement, null);
        return object;
    }

    public static int updateNullable(String sql, Object[] args, Class<?>[] types) throws Exception {
        int i;
        Connection connection = null;
        PreparedStatement preparedStatement = null;
        try {
            connection = databaseConnection.getConnection();
            preparedStatement = connection.prepareStatement(sql);
            for (i = 0; i < args.length; ++i) {
                if (args[i] != null) {
                    preparedStatement.setObject(i + 1, args[i]);
                    continue;
                }
                preparedStatement.setNull(i + 1, htDataType.get(types[i]));
            }
            i = preparedStatement.executeUpdate();
        } catch (Exception e) {
            try {
                throw e;
            } catch (Throwable throwable) {
                DBUtils.close(connection, preparedStatement, null);
                throw throwable;
            }
        }
        DBUtils.close(connection, preparedStatement, null);
        return i;
    }

    public static int[] updateNullableList(
            String sql, List<Object[]> args, Class<?>[] types, Connection... connections)
            throws Exception {
        int[] nArray;
        Connection connection = null;
        PreparedStatement preparedStatement = null;
        try {
            UpdateListResult listResult = new UpdateListResult(args.size());
            connection = DBUtils.getOrOpenConnection(connections);
            preparedStatement = connection.prepareStatement(sql);
            int count = 0;
            for (int i = 0; i < args.size(); ++i) {
                Object[] objs = args.get(i);
                for (int j = 0; j < objs.length; ++j) {
                    if (objs[j] != null) {
                        preparedStatement.setObject(j + 1, objs[j]);
                        continue;
                    }
                    preparedStatement.setNull(j + 1, htDataType.get(types[j]));
                }
                preparedStatement.addBatch();
                if (++count % Max_Batch_Count != 0) continue;
                listResult.add(preparedStatement.executeBatch());
            }
            listResult.add(preparedStatement.executeBatch());
            nArray = listResult.getResults();
        } catch (Exception e) {
            try {
                throw e;
            } catch (Throwable throwable) {
                DBUtils.close(
                        DBUtils.getToCloseConnection(connection, connections),
                        preparedStatement,
                        null);
                throw throwable;
            }
        }
        DBUtils.close(
                DBUtils.getToCloseConnection(connection, connections), preparedStatement, null);
        return nArray;
    }

    public static int update(String sql, Object[] args, Connection... connections)
            throws Exception {
        Connection connection = null;
        PreparedStatement preparedStatement = null;
        try {
            int i;
            connection = DBUtils.getOrOpenConnection(connections);
            preparedStatement = connection.prepareStatement(sql);
            for (i = 0; i < args.length; ++i) {
                preparedStatement.setObject(i + 1, args[i]);
            }
            i = preparedStatement.executeUpdate();
            return i;
        } catch (Exception e) {
            throw e;
        } finally {
            DBUtils.close(
                    DBUtils.getToCloseConnection(connection, connections), preparedStatement, null);
        }
    }

    public static int[] updateList(String sql, List<Object[]> args, Connection... connections)
            throws Exception {
        int[] nArray;
        Connection connection = null;
        PreparedStatement preparedStatement = null;
        try {
            UpdateListResult listResult = new UpdateListResult(args.size());
            connection = DBUtils.getOrOpenConnection(connections);
            preparedStatement = connection.prepareStatement(sql);
            int count = 0;
            for (int i = 0; i < args.size(); ++i) {
                Object[] objs = args.get(i);
                for (int j = 0; j < objs.length; ++j) {
                    preparedStatement.setObject(j + 1, objs[j]);
                }
                preparedStatement.addBatch();
                if (++count % Max_Batch_Count != 0) continue;
                listResult.add(preparedStatement.executeBatch());
            }
            listResult.add(preparedStatement.executeBatch());
            nArray = listResult.getResults();
        } catch (Exception e) {
            try {
                throw e;
            } catch (Throwable throwable) {
                DBUtils.close(
                        DBUtils.getToCloseConnection(connection, connections),
                        preparedStatement,
                        null);
                throw throwable;
            }
        }
        DBUtils.close(
                DBUtils.getToCloseConnection(connection, connections), preparedStatement, null);
        return nArray;
    }

    public static <T> int insert(T obj, String table, Connection... connections) throws Exception {
        return DBUtils.insert(obj, obj.getClass(), table, connections);
    }

    public static <T> int insert(T obj, Class<?> class1, String table, Connection... connections)
            throws Exception {
        DynamicSql dynamicSql = DynamicSql.generateInsert(class1, table);
        Connection connection = null;
        PreparedStatement preparedStatement = null;
        try {
            connection = DBUtils.getOrOpenConnection(connections);
            preparedStatement = connection.prepareStatement(dynamicSql.sql);
            Field[] fields = dynamicSql.fields;
            DBUtils.setValues(obj, preparedStatement, fields);
            int n = preparedStatement.executeUpdate();
            return n;
        } catch (Exception e) {
            throw e;
        } finally {
            DBUtils.close(
                    DBUtils.getToCloseConnection(connection, connections), preparedStatement, null);
        }
    }

    public static <T> int[] insertList(List<T> list, String table, Connection... connections)
            throws Exception {
        int[] nArray;
        if (list.size() <= 0) {
            return null;
        }
        T obj = list.get(0);
        DynamicSql dynamicSql = DynamicSql.generateInsert(obj, table);
        Connection connection = null;
        PreparedStatement preparedStatement = null;
        try {
            UpdateListResult listResult = new UpdateListResult(list.size());
            connection = DBUtils.getOrOpenConnection(connections);
            preparedStatement = connection.prepareStatement(dynamicSql.sql);
            Field[] fields = dynamicSql.fields;
            int count = 0;
            for (T item : list) {
                DBUtils.setValues(item, preparedStatement, fields);
                preparedStatement.addBatch();
                if (++count % Max_Batch_Count != 0) continue;
                listResult.add(preparedStatement.executeBatch());
            }
            int[] result = preparedStatement.executeBatch();
            listResult.add(result);
            nArray = listResult.getResults();
        } catch (Exception e) {
            try {
                throw e;
            } catch (Throwable throwable) {
                DBUtils.close(
                        DBUtils.getToCloseConnection(connection, connections),
                        preparedStatement,
                        null);
                throw throwable;
            }
        }
        DBUtils.close(
                DBUtils.getToCloseConnection(connection, connections), preparedStatement, null);
        return nArray;
    }

    public static <T> void setValues(T obj, PreparedStatement preparedStatement, Field[] fields)
            throws Exception {
        int startIndex = 1;
        DBUtils.setValues(obj, preparedStatement, fields, startIndex);
    }

    public static <T> void setValues(
            T obj, PreparedStatement preparedStatement, Field[] fields, Field[] keys)
            throws Exception {
        int startIndex = 1;
        DBUtils.setValues(obj, preparedStatement, fields, startIndex);
        if (keys != null) {
            DBUtils.setValues(obj, preparedStatement, keys, startIndex += fields.length);
        }
    }

    public static <T> void setValues(
            T obj, PreparedStatement preparedStatement, Field[] fields, int startIndex)
            throws Exception {
        for (int i = 0; i < fields.length; ++i) {
            try {
                Field field = fields[i];
                Object val = fields[i].get(obj);
                if (val != null) {
                    preparedStatement.setObject(i + startIndex, val);
                    continue;
                }
                preparedStatement.setNull(i + startIndex, htDataType.get(field.getType()));
                continue;
            } catch (Exception e) {
                logger.info("setValues: {},{}", obj, (Object) fields[i].getName());
                throw e;
            }
        }
    }

    public static void close(
            Connection connection, PreparedStatement preparedStatement, ResultSet resultSet) {
        if (resultSet != null) {
            try {
                resultSet.close();
            } catch (SQLException sQLException) {
                // empty catch block
            }
        }
        if (preparedStatement != null) {
            try {
                preparedStatement.close();
            } catch (SQLException sQLException) {
                // empty catch block
            }
        }
        if (connection != null) {
            databaseConnection.freeConnection(connection);
        }
    }

    private static Connection getOrOpenConnection(Connection... connections) {
        if (connections != null && connections.length >= 1) {
            return connections[0];
        }
        return databaseConnection.getConnection();
    }

    private static Connection getToCloseConnection(
            Connection connection, Connection... connections) {
        if (connections != null && connections.length >= 1) {
            return null;
        }
        return connection;
    }

    public static List<DBPoolParam> readConfig(String path) throws Exception {
        ArrayList<DBPoolParam> temp = new ArrayList<DBPoolParam>();
        Properties properties = new Properties();
        try (InputStream input = new FileInputStream(path)) {
            properties.load(input);
        }
        try (InputStream input =
                DBUtils.class.getClassLoader().getResourceAsStream("common.properties"); ) {
            if (input == null) {
                throw new IllegalStateException("Missing framework resource: common.properties");
            }
            properties.load(input);
        }
        String mysqlVersion = properties.getProperty("mysql.driver.version");
        logger.info("mysqlVersion: {}", (Object) mysqlVersion);
        int dBCount = 0;
        String dBCountStr = properties.getProperty(dbKey + DBPoolParam.KEY.DBCount.toString());
        try {
            dBCount = Integer.valueOf(dBCountStr);
            if (dBCount <= 0) {
                throw new Exception("dBCount <= 0, dBCount=" + dBCount);
            }
        } catch (Exception e) {
            e.printStackTrace();
            throw new Exception(
                    String.format("DBCount is Error, DBCount=%s, Exception=%s", dBCountStr, e));
        }
        for (int i = 0; i < dBCount; ++i) {
            try {
                String DBAsyncInsert =
                        properties.getProperty(
                                dbKey
                                        + String.format(
                                                DBPoolParam.KEY.DBAsyncInsert.toString() + "_" + i,
                                                new Object[0]));
                if (!StringUtil.isBlank(DBAsyncInsert)) continue;
                DBPoolParam dbParams = new DBPoolParam();
                dbParams.SourceName =
                        properties.getProperty(
                                dbKey
                                        + String.format(
                                                DBPoolParam.KEY.DBSourceName.toString() + "_" + i,
                                                new Object[0]));
                dbParams.Driver =
                        properties.getProperty(
                                dbKey
                                        + String.format(
                                                DBPoolParam.KEY.DBDriver.toString() + "_" + i,
                                                new Object[0]));
                String dbUrl =
                        properties.getProperty(
                                dbKey
                                        + String.format(
                                                DBPoolParam.KEY.DBUrl.toString() + "_" + i,
                                                new Object[0]));
                if (StringUtils.isNotBlank((CharSequence) mysqlVersion) && dbUrl.contains("?")) {
                    String[] split = StringUtils.split((String) dbUrl, (String) "?");
                    if (StringUtils.startsWith((CharSequence) mysqlVersion, (CharSequence) "5.")) {
                        dbUrl =
                                split[0]
                                        + "?charset=utf8&parseTime=True&loc=Local&rewriteBatchedStatements=true";
                    } else if (StringUtils.startsWith(
                            (CharSequence) mysqlVersion, (CharSequence) "8.")) {
                        dbUrl =
                                split[0]
                                        + "?useSSL=false&serverTimezone=Asia/Shanghai&characterEncoding=utf8&defaultAuthenticationPlugin=mysql_native_password";
                    }
                }
                logger.info("dbUrl: {}", (Object) dbUrl);
                dbParams.Url = dbUrl;
                dbParams.Username =
                        properties.getProperty(
                                dbKey
                                        + String.format(
                                                DBPoolParam.KEY.DBUsername.toString() + "_" + i,
                                                new Object[0]));
                dbParams.Passwd =
                        properties.getProperty(
                                dbKey
                                        + String.format(
                                                DBPoolParam.KEY.DBPasswd.toString() + "_" + i,
                                                new Object[0]));
                dbParams.MaxCount =
                        Integer.valueOf(
                                properties.getProperty(
                                        dbKey
                                                + String.format(
                                                        DBPoolParam.KEY.DBMaxCount.toString()
                                                                + "_"
                                                                + i,
                                                        new Object[0])));
                dbParams.MinCount =
                        Integer.valueOf(
                                properties.getProperty(
                                        dbKey
                                                + String.format(
                                                        DBPoolParam.KEY.DBMinCount.toString()
                                                                + "_"
                                                                + i,
                                                        new Object[0])));
                dbParams.CheckSql =
                        properties.getProperty(
                                dbKey
                                        + String.format(
                                                DBPoolParam.KEY.DBCheckSql.toString() + "_" + i,
                                                new Object[0]));
                dbParams.IsEncrypt =
                        Boolean.valueOf(
                                properties.getProperty(
                                        dbKey
                                                + String.format(
                                                        DBPoolParam.KEY.DBIsEncrypt.toString()
                                                                + "_"
                                                                + i,
                                                        new Object[0])));
                dbParams.DBConnOutTime =
                        Integer.valueOf(
                                properties.getProperty(
                                        dbKey
                                                + String.format(
                                                        DBPoolParam.KEY.DBConnOutTime.toString()
                                                                + "_"
                                                                + i,
                                                        new Object[0])));
                dbParams.DBConnCheckNumber =
                        Integer.valueOf(
                                properties.getProperty(
                                        dbKey
                                                + String.format(
                                                        DBPoolParam.KEY.DBConnCheckNumber.toString()
                                                                + "_"
                                                                + i,
                                                        new Object[0])));
                dbParams.DBStrategy =
                        Boolean.valueOf(
                                properties.getProperty(
                                        dbKey
                                                + String.format(
                                                        DBPoolParam.KEY.DBStrategy.toString()
                                                                + "_"
                                                                + i,
                                                        new Object[0])));
                temp.add(dbParams);
                continue;
            } catch (Exception e) {
                e.printStackTrace();
            }
        }
        return temp;
    }

    static {
        logger = LoggerFactory.getLogger(DBUtils.class);
        htDataType = null;
        Max_Batch_Count = 400;
        htDataType = new Hashtable();
        htDataType.put(Integer.TYPE, 4);
        htDataType.put(Long.TYPE, 4);
        htDataType.put(BigDecimal.class, 2);
        htDataType.put(String.class, 12);
        htDataType.put(Character.TYPE, 1);
        htDataType.put(RowId.class, -8);
        htDataType.put(Timestamp.class, 93);
        htDataType.put(java.util.Date.class, 91);
        htDataType.put(Date.class, 91);
        databaseConnection =
                new IDatabaseConnection() {

                    @Override
                    public Connection getConnection() {
                        try {
                            return dbpool.getConnection();
                        } catch (Exception e) {
                            return null;
                        }
                    }

                    @Override
                    public void freeConnection(Connection connection) {
                        try {
                            dbpool.freeConnection(connection);
                        } catch (Exception exception) {
                            // empty catch block
                        }
                    }
                };
    }
}
