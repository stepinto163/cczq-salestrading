// Recovered compatibility source; provenance: recovered-support.json.
package ats.app.ym.utils.db;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import java.lang.reflect.Field;
import java.math.BigDecimal;
import java.sql.Connection;
import java.sql.PreparedStatement;
import java.sql.ResultSet;
import java.sql.RowId;
import java.sql.SQLException;
import java.util.ArrayList;
import java.util.Hashtable;
import java.util.List;

public class DBUtils {
    private static Logger logger = LoggerFactory.getLogger(DBUtils.class);
    private static Hashtable<Class, Integer> htDataType = null;
    public static int Max_Batch_Count = 400;
    private static IDatabaseConnection databaseConnection;

    static {
        htDataType = new Hashtable();
        htDataType.put(Integer.TYPE, 4);
        htDataType.put(Long.TYPE, 4);
        htDataType.put(BigDecimal.class, 2);
        htDataType.put(String.class, 12);
        htDataType.put(Character.TYPE, 1);
        htDataType.put(RowId.class, -8);
    }

    public static IDatabaseConnection getDatabaseConnection() {
        return databaseConnection;
    }

    public static void setDatabaseConnection(IDatabaseConnection databaseConnection) {
        DBUtils.databaseConnection = databaseConnection;
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
                int i = 0;
                while (i < params.length) {
                    preparedStatement.setObject(i + 1, params[i]);
                    ++i;
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
                    default:
                        {
                            result = resultSet.getObject(1);
                            break;
                        }
                }
            }
        } catch (Throwable throwable) {
            DBUtils.close(
                    DBUtils.getToCloseConnection(connection, connections),
                    preparedStatement,
                    resultSet);
            throw throwable;
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
                int i = 0;
                while (i < params.length) {
                    preparedStatement.setObject(i + 1, params[i]);
                    ++i;
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
                    default:
                        {
                            result = resultSet.getObject(1);
                        }
                }
                if (result == null) continue;
                list.add((T) result);
            }
        } catch (Throwable throwable) {
            DBUtils.close(
                    DBUtils.getToCloseConnection(connection, connections),
                    preparedStatement,
                    resultSet);
            throw throwable;
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
                int i = 0;
                while (i < params.length) {
                    preparedStatement.setObject(i + 1, params[i]);
                    ++i;
                }
            }
            resultSet = preparedStatement.executeQuery();
            resultSet.setFetchSize(fecthSize);
            Field[] fields = type.getFields();
            while (resultSet.next()) {
                T obj = type.newInstance();
                Field[] fieldArray = fields;
                int n = fields.length;
                int n2 = 0;
                while (n2 < n) {
                    Field field = fieldArray[n2];
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
                    ++n2;
                }
                list.add(obj);
            }
        } catch (Throwable throwable) {
            DBUtils.close(
                    DBUtils.getToCloseConnection(connection, connections),
                    preparedStatement,
                    resultSet);
            throw throwable;
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
        int[] nArray;
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

    public static int updateNullable(String sql, Object[] args, Class[] types) throws Exception {
        int n;
        Connection connection = null;
        PreparedStatement preparedStatement = null;
        try {
            connection = databaseConnection.getConnection();
            preparedStatement = connection.prepareStatement(sql);
            int i = 0;
            while (i < args.length) {
                if (args[i] != null) {
                    preparedStatement.setObject(i + 1, args[i]);
                } else {
                    preparedStatement.setNull(i + 1, htDataType.get(types[i]));
                }
                ++i;
            }
            n = preparedStatement.executeUpdate();
        } catch (Exception e) {
            try {
                throw e;
            } catch (Throwable throwable) {
                DBUtils.close(connection, preparedStatement, null);
                throw throwable;
            }
        }
        DBUtils.close(connection, preparedStatement, null);
        return n;
    }

    public static int[] updateNullableList(
            String sql, List<Object[]> args, Class[] types, Connection... connections)
            throws Exception {
        int[] nArray;
        Connection connection = null;
        PreparedStatement preparedStatement = null;
        try {
            UpdateListResult listResult = new UpdateListResult(args.size());
            connection = DBUtils.getOrOpenConnection(connections);
            preparedStatement = connection.prepareStatement(sql);
            int count = 0;
            int i = 0;
            while (i < args.size()) {
                Object[] objs = args.get(i);
                int j = 0;
                while (j < objs.length) {
                    if (objs[j] != null) {
                        preparedStatement.setObject(j + 1, objs[j]);
                    } else {
                        preparedStatement.setNull(j + 1, htDataType.get(types[j]));
                    }
                    ++j;
                }
                preparedStatement.addBatch();
                if (++count % Max_Batch_Count == 0) {
                    listResult.add(preparedStatement.executeBatch());
                }
                ++i;
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
            connection = DBUtils.getOrOpenConnection(connections);
            preparedStatement = connection.prepareStatement(sql);
            int i = 0;
            while (i < args.length) {
                preparedStatement.setObject(i + 1, args[i]);
                ++i;
            }
            int n = preparedStatement.executeUpdate();
            return n;
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
            int i = 0;
            while (i < args.size()) {
                Object[] objs = args.get(i);
                int j = 0;
                while (j < objs.length) {
                    preparedStatement.setObject(j + 1, objs[j]);
                    ++j;
                }
                preparedStatement.addBatch();
                if (++count % Max_Batch_Count == 0) {
                    listResult.add(preparedStatement.executeBatch());
                }
                ++i;
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
        int i = 0;
        while (i < fields.length) {
            try {
                Field field = fields[i];
                Object val = fields[i].get(obj);
                if (val != null) {
                    preparedStatement.setObject(i + startIndex, val);
                } else {
                    preparedStatement.setNull(i + startIndex, htDataType.get(field.getType()));
                }
            } catch (Exception e) {
                logger.info("setValues: {},{}", obj, (Object) fields[i].getName());
                throw e;
            }
            ++i;
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
}
