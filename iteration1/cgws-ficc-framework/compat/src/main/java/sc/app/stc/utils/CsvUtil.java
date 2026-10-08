// Recovered compatibility source; provenance: recovered-support.json.
package sc.app.stc.utils;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import sc.app.stc.annotation.Column4Reader;
import sc.app.stc.annotation.Column4Writer;
import sc.app.stc.csvreader.CsvReader;
import sc.app.stc.csvreader.CsvWriter;

import java.io.File;
import java.io.FileInputStream;
import java.io.FileOutputStream;
import java.io.IOException;
import java.lang.reflect.Field;
import java.nio.charset.Charset;
import java.util.ArrayList;
import java.util.List;

public class CsvUtil {
    private static Logger logger = LoggerFactory.getLogger(CsvUtil.class);
    public static String charset = "UTF-8";

    public static boolean createFile(String file) throws IOException {
        File f = new File(file);
        if (!f.exists()) {
            f.getParentFile().mkdirs();
            return f.createNewFile();
        }
        return true;
    }

    private static void checkWriteFile(String filename) {
        int index = filename.indexOf(File.separator);
        System.out.println(File.separator);
        if (index == -1) {
            throw new RuntimeException();
        }
        File dir = new File(filename.substring(0, index));
        if (!dir.exists() && !dir.mkdirs()) {
            throw new RuntimeException("checkWriteFile failed, filename:" + filename);
        }
    }

    public static <T> List<T> read(String file, Class<T> clazz) throws Exception {
        ArrayList<T> list = new ArrayList<T>();
        Field[] tmp = clazz.getDeclaredFields();
        ArrayList<Field> fields = new ArrayList<Field>();
        for (Field field : tmp) {
            if (!field.isAnnotationPresent(Column4Reader.class)) continue;
            fields.add(field);
        }
        CsvReader csvReader = new CsvReader(new FileInputStream(file), Charset.forName(charset));
        try {
            String[] head = null;
            if (!csvReader.readHeaders()) {
                throw new Exception("No head in the file" + file);
            }
            head = csvReader.getHeaders();
            int column = head.length;
            while (csvReader.readRecord()) {
                T bean = null;
                try {
                    bean = clazz.newInstance();
                } catch (Exception e) {
                    throw new Exception("Can't instance class - " + clazz, e);
                }
                block9:
                for (int i = 0; i < column; ++i) {
                    String key = CsvUtil.specialUnicode(head[i].trim());
                    String val = CsvUtil.specialUnicode(csvReader.get(head[i]));
                    for (Field field : fields) {
                        Column4Reader col;
                        if (!field.isAnnotationPresent(Column4Reader.class)
                                || !(col = field.getAnnotation(Column4Reader.class))
                                        .value()
                                        .equals(key)
                                || val == null) continue;
                        CsvUtil.setValue(field, bean, val);
                        continue block9;
                    }
                }
                list.add(bean);
            }
        } finally {
            csvReader.close();
        }
        return list;
    }

    public static <T> void write(List<T> list, String file) throws Exception {
        CsvUtil.checkWriteFile(file);
        boolean isFirst = true;
        int headCount = -1;
        ArrayList<String[]> wlist = new ArrayList<String[]>();
        for (T bean : list) {
            Field[] fields = bean.getClass().getDeclaredFields();
            if (fields == null) {
                throw new RuntimeException("Not found fields in bean:" + bean.getClass());
            }
            int rowCount = fields.length;
            ArrayList<String> head = new ArrayList<String>();
            ArrayList<String> rowVal = new ArrayList<String>();
            for (int i = 0; i < rowCount; ++i) {
                Field field = fields[i];
                if (!field.isAnnotationPresent(Column4Writer.class)) continue;
                if (isFirst) {
                    Column4Writer col = field.getAnnotation(Column4Writer.class);
                    head.add(col.value());
                }
                try {
                    rowVal.add(CsvUtil.getValue(field, bean));
                    continue;
                } catch (Exception e) {
                    e.printStackTrace();
                }
            }
            if (isFirst) {
                headCount = head.size();
                if (headCount <= 0) {
                    throw new RuntimeException();
                }
                wlist.add(CsvUtil.toStrArr(headCount, head));
                isFirst = false;
            }
            wlist.add(CsvUtil.toStrArr(headCount, rowVal));
        }
        CsvUtil.write2(file, wlist);
    }

    private static String[] toStrArr(int headCount, List<String> rowData) {
        String[] rowDataArr = new String[headCount];
        rowData.toArray(rowDataArr);
        return rowDataArr;
    }

    public static void write2(String file, List<String[]> wlist) throws Exception {
        CsvUtil.write2(file, wlist, charset);
    }

    public static void write2(String keyName, String file, List<String[]> wlist) {
        try {
            CsvUtil.write2(file, wlist);
            logger.info(
                    "keyName:{} Output info succeed to file:{}", (Object) keyName, (Object) file);
        } catch (Exception e) {
            logger.info("keyName:{} Write to file exception:" + file, (Throwable) e);
        }
    }

    public static void write2(String file, List<String[]> wlist, String charset) throws Exception {
        CsvUtil.createFile(file);
        CsvWriter csvWriter =
                new CsvWriter(new FileOutputStream(file), ',', Charset.forName(charset));
        try {
            for (String[] strs : wlist) {
                csvWriter.writeRecord(strs);
            }
            csvWriter.flush();
        } finally {
            csvWriter.close();
        }
    }

    private static String getValue(Field field, Object entity)
            throws IllegalArgumentException, IllegalAccessException {
        field.setAccessible(true);
        return (String) field.get(entity);
    }

    private static void setValue(Field field, Object entity, Object value)
            throws IllegalArgumentException, IllegalAccessException {
        field.setAccessible(true);
        String type = field.getType().toString();
        if (type.endsWith("String")) {
            field.set(entity, DataTypeConverter.toString(value));
        } else if (type.endsWith("double") || type.endsWith("Double")) {
            field.set(entity, DataTypeConverter.toDouble(value));
        } else if (type.endsWith("long") || type.endsWith("Long")) {
            field.set(entity, DataTypeConverter.toLong(value));
        } else if (type.endsWith("float") || type.endsWith("Float")) {
            field.set(entity, DataTypeConverter.toFloat(value));
        } else if (type.endsWith("int") || type.endsWith("Integer")) {
            field.set(entity, DataTypeConverter.toInt(value));
        } else if (type.endsWith("boolean") || type.endsWith("Boolean")) {
            field.set(entity, DataTypeConverter.toBoolean(value));
        } else if (type.endsWith("char") || type.endsWith("Character")) {
            field.set(entity, DataTypeConverter.toChar(value));
        } else if (type.endsWith("BigDecimal")) {
            field.set(entity, DataTypeConverter.toBigDecimal(value));
        } else {
            throw new IllegalArgumentException("Unknown type : " + type + ",value : " + value);
        }
    }

    public static String specialUnicode(String str) {
        if (str.startsWith("\ufeff")) {
            str = str.replace("\ufeff", "");
        } else if (str.endsWith("\ufeff")) {
            str = str.replace("\ufeff", "");
        }
        return str;
    }
}
