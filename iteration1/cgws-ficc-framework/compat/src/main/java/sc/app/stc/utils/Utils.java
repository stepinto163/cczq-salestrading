// Recovered compatibility source; provenance: recovered-support.json.
package sc.app.stc.utils;

import ats.foundation.utils.db.base.DBPoolParam;
import ats.foundation.utils.db.base.DBUtils;

import cn.hutool.core.util.NumberUtil;
import cn.hutool.core.util.StrUtil;

import com.app.common.db.DBPoolResource;

import org.apache.commons.configuration.AbstractConfiguration;
import org.apache.http.util.TextUtils;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import sc.app.stc.api.base.po.BondInfoView;

import java.io.BufferedReader;
import java.io.IOException;
import java.io.InputStreamReader;
import java.math.BigDecimal;
import java.math.RoundingMode;
import java.text.SimpleDateFormat;
import java.util.ArrayList;
import java.util.Calendar;
import java.util.Date;
import java.util.List;
import java.util.regex.Matcher;
import java.util.regex.Pattern;

public class Utils {
    private static Logger logger = LoggerFactory.getLogger(Utils.class);
    private static String macAddressStr = null;
    private static String computerName = System.getenv().get("COMPUTERNAME");
    private static final String[] windowsCommand = new String[] {"ipconfig", "/all"};
    private static final String[] linuxCommand = new String[] {"/sbin/ifconfig", "-a"};
    private static final Pattern macPattern =
            Pattern.compile(".*((:?[0-9a-f]{2}[-:]){5}[0-9a-f]{2}).*", 2);

    public static Boolean isCleanPrice(String price) {
        return TextUtil.isNumeric(price)
                && new BigDecimal(price).compareTo(new BigDecimal("30")) >= 0
                && new BigDecimal(price).compareTo(new BigDecimal("999")) != 0
                && new BigDecimal(price).compareTo(new BigDecimal("-1")) != 0;
    }

    public static String calcTenor(BondInfoView bond) {
        String optionDate;
        String result = "";
        boolean isOption = false;
        String string = optionDate = bond.Option_Date == null ? "" : bond.Option_Date + "";
        if (!TextUtils.isBlank((CharSequence) bond.Option_Type) && bond.Option_Type.equals("ETS")) {
            result = Utils.calcTenorDetail("", bond.Maturity_Date.toPlainString());
            result = String.format("%s + N", result);
        } else if (!TextUtils.isBlank((CharSequence) optionDate)) {
            SimpleDateFormat sdf = new SimpleDateFormat("yyyyMMdd");
            Date dtCur = new Date();
            String now = sdf.format(dtCur);
            boolean bl = isOption = now.compareTo(optionDate) <= 0;
            if (isOption) {
                result = Utils.calcTenorDetail("", optionDate);
                result =
                        String.format(
                                "%s + %s",
                                result,
                                Utils.calcTenorDetail(
                                        optionDate, bond.Maturity_Date.toPlainString()));
            } else {
                result = Utils.calcTenorDetail("", bond.Maturity_Date.toPlainString());
            }
        } else {
            result = Utils.calcTenorDetail("", bond.Maturity_Date.toPlainString());
        }
        return result;
    }

    public static String calcTenorDetail(String start, String end) {
        String result = "";
        try {
            SimpleDateFormat sdf = new SimpleDateFormat("yyyyMMdd");
            Date dtCur = new Date();
            dtCur =
                    !TextUtils.isBlank((CharSequence) start)
                            ? sdf.parse(start)
                            : sdf.parse(sdf.format(new Date()));
            Date dtDest = sdf.parse(end);
            long dateTime = dtDest.getTime();
            long todayTime = dtCur.getTime();
            long days = (dateTime - todayTime) / 86400000L;
            int restYear = dtDest.getYear() - dtCur.getYear();
            String curMMdd = sdf.format(dtCur).substring(4, 8);
            String matruMMdd = end.substring(4, 8);
            if (restYear == 0) {
                if (matruMMdd.compareTo(curMMdd) == 0) {
                    result = "0D";
                } else if (matruMMdd.compareTo(curMMdd) > 0) {
                    result = days + "D";
                }
            } else if (restYear > 0) {
                Calendar cal = Calendar.getInstance();
                cal.clear();
                cal.setTime(dtCur);
                if (matruMMdd.compareTo(curMMdd) >= 0) {
                    cal.add(1, restYear);
                } else {
                    cal.add(1, restYear - 1);
                    --restYear;
                }
                days = (dateTime - cal.getTime().getTime()) / 86400000L;
                BigDecimal bd =
                        new BigDecimal(days)
                                .divide(
                                        new BigDecimal(
                                                Utils.isLeapYear(dtDest.getYear()) ? 366 : 365),
                                        2,
                                        RoundingMode.HALF_UP);
                result =
                        restYear == 0
                                ? days + "D"
                                : (bd.compareTo(new BigDecimal(0)) == 0
                                        ? restYear + "Y"
                                        : ((bd = bd.add(new BigDecimal(restYear))).doubleValue()
                                                                % 1.0
                                                        == 0.0
                                                ? bd.intValue() + "Y"
                                                : bd.toPlainString() + "Y"));
            }
        } catch (Exception exception) {
            // empty catch block
        }
        return result;
    }

    public static String calcLeftTenor(String date, boolean withD) {
        try {
            if (!TextUtils.isBlank((CharSequence) date)) {
                long todayTime;
                SimpleDateFormat sdf = new SimpleDateFormat("yyyyMMdd");
                Date dtCur = sdf.parse(sdf.format(new Date()));
                Date dtDest = sdf.parse(date);
                long dateTime = dtDest.getTime();
                long days = (dateTime - (todayTime = dtCur.getTime())) / 86400000L;
                if (days < 0L) {
                    return "0Y";
                }
                if (withD && days < 365L) {
                    return days + "D";
                }
                int restYear = dtDest.getYear() - dtCur.getYear();
                int restYear2 = dtDest.getYear() - dtCur.getYear();
                String curMMdd = sdf.format(dtCur).substring(4, 4);
                String matruMMdd = date.substring(4, 4);
                Calendar cal = Calendar.getInstance();
                cal.clear();
                cal.setTime(dtCur);
                if (matruMMdd.compareTo(curMMdd) >= 0) {
                    cal.add(1, restYear);
                } else {
                    cal.add(1, restYear - 1);
                    --restYear2;
                }
                days = (dateTime - cal.getTime().getTime()) / 86400000L;
                BigDecimal bd =
                        new BigDecimal(days)
                                .divide(
                                        new BigDecimal(
                                                Utils.isLeapYear(dtDest.getYear()) ? 366 : 365),
                                        2,
                                        RoundingMode.HALF_UP);
                bd = bd.add(new BigDecimal(restYear2));
                return bd.toPlainString() + "Y";
            }
        } catch (Exception exception) {
            // empty catch block
        }
        return "";
    }

    public static boolean isLeapYear(int year) {
        boolean b1 = year % 4 == 0;
        boolean b2 = year % 100 != 0;
        boolean b3 = year % 400 == 0;
        return b1 && b2 || b3;
    }

    public static String IpToHex(String ip) {
        String str = "";
        String[] strs = ip.split("\\.");
        if (strs.length == 4) {
            String temp = Utils.intStrToHex(strs[2]);
            str = str + temp;
            temp = Utils.intStrToHex(strs[3]);
            str = str + temp;
        }
        return str;
    }

    public static String ListStrToString(List<String> lt) {
        String str = "";
        for (String s : lt) {
            str = str + s + ",";
        }
        if (str.length() > 0) {
            str = str.substring(0, str.length() - 1);
        }
        return str;
    }

    public static List<String> StringToListStr(String str) {
        ArrayList<String> lt = new ArrayList<String>();
        if (str != null) {
            String[] strs;
            for (String s : strs = str.split(",")) {
                lt.add(s);
            }
        }
        return lt;
    }

    public static String intStrToHex(String str) {
        String temp = Integer.toHexString(Integer.valueOf(str));
        if (temp.length() < 2) {
            temp = "0" + temp;
        }
        return temp;
    }

    public static String getEmpty(String str) {
        return TextUtils.isBlank((CharSequence) str) ? "" : str;
    }

    public static String getEmpty(String str, int length) {
        String tmp;
        String string = tmp = TextUtils.isBlank((CharSequence) str) ? "" : str;
        if (tmp.length() > length) {
            tmp = tmp.substring(0, length);
        }
        return tmp;
    }

    public static BigDecimal getBigDecimal(BigDecimal b) {
        return b == null ? BigDecimal.ZERO : b;
    }

    public static BigDecimal getBigDecimal(String b) {
        if (StrUtil.isNotBlank((CharSequence) b) && NumberUtil.isNumber((CharSequence) b)) {
            return NumberUtil.toBigDecimal((String) b);
        }
        return BigDecimal.ZERO;
    }

    public static String getFormat(Object str, int length) {
        return String.format("%-" + length + "s", str);
    }

    private static final List<String> getMacAddressList() throws IOException {
        String[] command;
        ArrayList<String> macAddressList = new ArrayList<String>();
        String os = System.getProperty("os.name");
        if (os.startsWith("Windows")) {
            command = windowsCommand;
        } else if (os.startsWith("Linux")) {
            command = linuxCommand;
        } else {
            return macAddressList;
        }
        Process process = Runtime.getRuntime().exec(command);
        BufferedReader bufReader =
                new BufferedReader(new InputStreamReader(process.getInputStream()));
        String line = null;
        while ((line = bufReader.readLine()) != null) {
            Matcher matcher = macPattern.matcher(line);
            if (!matcher.matches()) continue;
            macAddressList.add(matcher.group(1));
        }
        process.destroy();
        bufReader.close();
        return macAddressList;
    }

    public static String getMacAddress() {
        if (macAddressStr == null || macAddressStr.equals("")) {
            StringBuffer sb = new StringBuffer();
            try {
                List<String> macList = Utils.getMacAddressList();
                for (String amac : macList) {
                    if (amac.equals("0000000000E0")) continue;
                    sb.append(amac);
                    break;
                }
            } catch (IOException e) {
                e.printStackTrace();
            }
            macAddressStr = sb.toString();
        }
        return macAddressStr;
    }

    public static int getDBType(DBPoolResource dbpool) {
        int dbType = 1;
        AbstractConfiguration config = dbpool.getConfig();
        String dbPoolCfgString = config.getString("dbpool.cfg");
        String dbDefaultName = config.getString("dbpool.default");
        try {
            List<DBPoolParam> lt = DBUtils.readConfig(dbPoolCfgString);
            DBPoolParam dbp = null;
            for (DBPoolParam dbPoolParam : lt) {
                if (!dbPoolParam.SourceName.equals(dbDefaultName)) continue;
                dbp = dbPoolParam;
                break;
            }
            if (dbp != null) {
                String url = dbp.Url.toLowerCase();
                if (url.contains("mysql")) {
                    dbType = 1;
                }
                if (url.contains("oracle")) {
                    dbType = 2;
                }
                if (url.contains("postgresql")) {
                    dbType = 3;
                }
            }
        } catch (Exception e) {
            logger.error("getDBType error", (Throwable) e);
        }
        return dbType;
    }
}
