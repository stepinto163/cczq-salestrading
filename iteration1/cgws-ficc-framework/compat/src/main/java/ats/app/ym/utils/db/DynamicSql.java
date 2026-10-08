// Recovered compatibility source; provenance: recovered-support.json.
package ats.app.ym.utils.db;

import java.lang.reflect.Field;
import java.util.ArrayList;
import java.util.HashSet;
import java.util.Hashtable;

public class DynamicSql {
    public String sql;
    public Field[] fields;
    public Field[] keys;
    private static Hashtable<String, DynamicSql> hashtable = new Hashtable();

    public DynamicSql() {}

    public DynamicSql(String sql, Field[] fields) {
        this.sql = sql;
        this.fields = fields;
    }

    public DynamicSql(String sql, Field[] fields, Field[] keys) {
        this.sql = sql;
        this.fields = fields;
        this.keys = keys;
    }

    public String toString() {
        return this.sql;
    }

    public static DynamicSql generateInsert(Object obj, String table) {
        return DynamicSql.generateInsert(obj.getClass(), table);
    }

    public static DynamicSql generateInsert(Class<?> class1, String table) {
        String key = DynamicSql.getKey(class1, table);
        DynamicSql dynamicSql = hashtable.get(key);
        if (dynamicSql == null) {
            StringBuilder sbFields = new StringBuilder();
            sbFields.append("insert into ").append(table).append("(");
            StringBuilder sbValues = new StringBuilder();
            sbValues.append(" values(");
            Field[] fields = class1.getFields();
            int i = 0;
            while (i < fields.length) {
                if (i != 0) {
                    sbFields.append(",");
                    sbValues.append(",");
                }
                sbFields.append(fields[i].getName());
                sbValues.append("?");
                ++i;
            }
            sbFields.append(")");
            sbValues.append(")");
            sbFields.append((CharSequence) sbValues);
            dynamicSql = new DynamicSql(sbFields.toString(), fields);
            hashtable.put(key, dynamicSql);
        }
        return dynamicSql;
    }

    public static DynamicSql generateInsert(
            Object obj, String table, Hashtable<String, String> htValues) {
        String key = DynamicSql.getKey(obj.getClass(), table);
        DynamicSql dynamicSql = hashtable.get(key);
        if (dynamicSql == null) {
            StringBuilder sbFields = new StringBuilder();
            sbFields.append("insert into ").append(table).append("(");
            StringBuilder sbValues = new StringBuilder();
            sbValues.append(" values(");
            Field[] fields = obj.getClass().getFields();
            ArrayList<Field> list = new ArrayList<Field>();
            int i = 0;
            while (i < fields.length) {
                if (i != 0) {
                    sbFields.append(",");
                    sbValues.append(",");
                }
                String fieldName = fields[i].getName();
                sbFields.append(fieldName);
                String val = htValues.get(fieldName);
                if (val == null) {
                    sbValues.append("?");
                    list.add(fields[i]);
                } else {
                    sbValues.append(val);
                }
                ++i;
            }
            sbFields.append(")");
            sbValues.append(")");
            sbFields.append((CharSequence) sbValues);
            dynamicSql = new DynamicSql(sbFields.toString(), list.toArray(new Field[0]));
            hashtable.put(key, dynamicSql);
        }
        return dynamicSql;
    }

    private static String getKey(Class<?> class1, String table) {
        return (String.valueOf(class1.getName()) + "@" + table).toUpperCase();
    }

    public static DynamicSql generateUpdate(Object obj, String table, String[] keys) {
        return DynamicSql.generateUpdate(obj.getClass(), table, keys);
    }

    public static DynamicSql generateUpdate(Class<?> class1, String table, String[] keys) {
        HashSet<String> keySet = new HashSet<String>();
        String[] stringArray = keys;
        int n = keys.length;
        int n2 = 0;
        while (n2 < n) {
            String item = stringArray[n2];
            keySet.add(item.toUpperCase());
            ++n2;
        }
        StringBuilder sbFields = new StringBuilder();
        sbFields.append("update ").append(table).append(" set ");
        StringBuilder sbKeys = new StringBuilder();
        sbKeys.append(" where ");
        Field[] fields = class1.getFields();
        Field[] keyFields = new Field[keys.length];
        Field[] notKeyFields = new Field[fields.length - keys.length];
        int keyIndex = 0;
        int notKeyIndex = 0;
        int i = 0;
        while (i < fields.length) {
            String fieldName = fields[i].getName();
            if (keySet.contains(fieldName.toUpperCase())) {
                if (keyIndex != 0) {
                    sbKeys.append(" and ");
                }
                sbKeys.append(fieldName).append("=?");
                keyFields[keyIndex] = fields[i];
                ++keyIndex;
            } else {
                if (notKeyIndex != 0) {
                    sbFields.append(",");
                }
                sbFields.append(fieldName).append("=?");
                notKeyFields[notKeyIndex] = fields[i];
                ++notKeyIndex;
            }
            ++i;
        }
        sbFields.append((CharSequence) sbKeys);
        DynamicSql dynamicSql = new DynamicSql(sbFields.toString(), notKeyFields, keyFields);
        return dynamicSql;
    }
}
