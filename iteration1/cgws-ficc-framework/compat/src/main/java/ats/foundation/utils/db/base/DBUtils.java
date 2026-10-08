// Recovered compatibility source; provenance: recovered-support.json.
package ats.foundation.utils.db.base;

import ats.foundation.utils.config.PropertiesHelper;
import ats.foundation.utils.security.PasswordSecurityHandler;

import java.io.FileInputStream;
import java.io.FileNotFoundException;
import java.io.FileOutputStream;
import java.io.IOException;
import java.io.OutputStream;
import java.text.SimpleDateFormat;
import java.util.ArrayList;
import java.util.Date;
import java.util.List;

public class DBUtils {
    public static String _Configpath = "./control/DBPoolConfig.ini";
    public static int _logLevel = 5;
    private static OutputStream _OutputStream = null;

    public static void init() {
        if (_OutputStream == null) {
            String ATS_ROOT = System.getenv("ATS_ROOT") + "";
            if (ATS_ROOT.equalsIgnoreCase("null")) {
                ATS_ROOT = ".";
            }
            try {
                _OutputStream = new FileOutputStream(ATS_ROOT + "/log/DBPool.log");
            } catch (FileNotFoundException e) {
                e.printStackTrace();
            }
        }
    }

    public static String toString(Exception e) {
        String msg =
                e.getMessage()
                        + " : \n"
                        + "    Exception by: { \n"
                        + "        by "
                        + e.getLocalizedMessage()
                        + "\n";
        StackTraceElement[] se = e.getStackTrace();
        for (int i = 0; i < se.length; ++i) {
            msg = msg + "        at " + se[i].toString() + "\n";
        }
        msg = msg + "    }\n";
        return msg;
    }

    public static void debug(String str) {
        String log = String.format("***Debug***, [%s]:%s", DBUtils.CurrentDataTime(), str);
        if (_logLevel == 5) {
            DBUtils.init();
            try {
                _OutputStream.write(log.getBytes());
                _OutputStream.write(10);
                _OutputStream.flush();
            } catch (IOException e) {
                e.printStackTrace();
            }
        } else if (_logLevel >= 4) {
            System.out.println(log);
        }
    }

    public static void info(String str) {
        String log = String.format("***Info***, [%s]:%s", DBUtils.CurrentDataTime(), str);
        if (_logLevel == 5) {
            DBUtils.init();
            try {
                _OutputStream.write(log.getBytes());
                _OutputStream.write(10);
                _OutputStream.flush();
            } catch (IOException e) {
                e.printStackTrace();
            }
        } else if (_logLevel >= 3) {
            System.out.println(log);
        }
    }

    public static void waring(String str) {
        String log = String.format("***Waring***, [%s]:%s", DBUtils.CurrentDataTime(), str);
        if (_logLevel == 5) {
            DBUtils.init();
            try {
                _OutputStream.write(log.getBytes());
                _OutputStream.write(10);
                _OutputStream.flush();
            } catch (IOException e) {
                e.printStackTrace();
            }
        } else if (_logLevel >= 2) {
            System.out.println(log);
        }
    }

    public static void error(String str) {
        String log = String.format("***Error***, [%s]:%s", DBUtils.CurrentDataTime(), str);
        if (_logLevel == 5) {
            DBUtils.init();
            try {
                _OutputStream.write(log.getBytes());
                _OutputStream.write(10);
                _OutputStream.flush();
            } catch (IOException e) {
                e.printStackTrace();
            }
        } else if (_logLevel >= 1) {
            System.out.println(log);
        }
    }

    private static String CurrentDataTime() {
        SimpleDateFormat sDF = new SimpleDateFormat("yyyyMMdd HH:mm:ss");
        return sDF.format(new Date()) + "<" + System.currentTimeMillis() + ">";
    }

    public static List<DBPoolParam> readConfig(String path) throws Exception {
        ArrayList<DBPoolParam> temp = new ArrayList<DBPoolParam>();
        PropertiesHelper properties = new PropertiesHelper();
        properties.load(new FileInputStream(path));
        int dBCount = 0;
        String dBCountStr = properties.getProperty(DBPoolParam.KEY.DBCount.toString());
        try {
            dBCount = Integer.valueOf(dBCountStr);
            _logLevel =
                    Integer.valueOf(properties.getProperty(DBPoolParam.KEY.LogLevel.toString()));
            if (dBCount <= 0) {
                throw new Exception("dBCount <= 0, dBCount=" + dBCount);
            }
        } catch (Exception e) {
            e.printStackTrace();
            throw new Exception(
                    String.format("DBCount is Error, DBCount=%s, Exception=%s", dBCountStr, e));
        }
        PasswordSecurityHandler securityHdl = PasswordSecurityHandler.getHandler();
        for (int i = 0; i < dBCount; ++i) {
            try {
                DBPoolParam dbParams = new DBPoolParam();
                dbParams.SourceName =
                        properties.getProperty(
                                String.format(DBPoolParam.KEY.DBSourceName.toString(), i));
                dbParams.Driver =
                        properties.getProperty(
                                String.format(DBPoolParam.KEY.DBDriver.toString(), i));
                dbParams.Url =
                        properties.getProperty(String.format(DBPoolParam.KEY.DBUrl.toString(), i));
                dbParams.Username =
                        properties.getProperty(
                                String.format(DBPoolParam.KEY.DBUsername.toString(), i));
                dbParams.Passwd =
                        properties.getProperty(
                                String.format(DBPoolParam.KEY.DBPasswd.toString(), i));
                dbParams.MaxCount =
                        Integer.valueOf(
                                properties.getProperty(
                                        String.format(DBPoolParam.KEY.DBMaxCount.toString(), i)));
                dbParams.MinCount =
                        Integer.valueOf(
                                properties.getProperty(
                                        String.format(DBPoolParam.KEY.DBMinCount.toString(), i)));
                dbParams.CheckSql =
                        properties.getProperty(
                                String.format(DBPoolParam.KEY.DBCheckSql.toString(), i));
                dbParams.IsEncrypt =
                        Boolean.valueOf(
                                properties.getProperty(
                                        String.format(DBPoolParam.KEY.DBIsEncrypt.toString(), i)));
                if (securityHdl != null && dbParams.IsEncrypt) {
                    dbParams.Passwd = securityHdl.decrypt(dbParams.Passwd, true);
                }
                dbParams.DBConnOutTime =
                        Integer.valueOf(
                                properties.getProperty(
                                        String.format(
                                                DBPoolParam.KEY.DBConnOutTime.toString(), i)));
                dbParams.DBConnCheckNumber =
                        Integer.valueOf(
                                properties.getProperty(
                                        String.format(
                                                DBPoolParam.KEY.DBConnCheckNumber.toString(), i)));
                dbParams.DBStrategy =
                        Boolean.valueOf(
                                properties.getProperty(
                                        String.format(DBPoolParam.KEY.DBStrategy.toString(), i)));
                temp.add(dbParams);
                continue;
            } catch (Exception e) {
                e.printStackTrace();
            }
        }
        return temp;
    }
}
