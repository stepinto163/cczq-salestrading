// Recovered compatibility source; provenance: recovered-support.json.
package sc.app.stc.utils;

import org.apache.commons.lang.math.NumberUtils;
import org.apache.http.util.TextUtils;

import sc.app.stc.api.CalWeekDateReq;
import sc.app.stc.api.CalWeekDateResult;
import sc.app.stc.api.WeekInfo;

import java.math.BigDecimal;
import java.text.DecimalFormat;
import java.text.NumberFormat;
import java.text.ParseException;
import java.text.SimpleDateFormat;
import java.util.Calendar;
import java.util.Date;
import java.util.List;

public class TextUtil {
    public static WeekDateSettlement wds = new WeekDateSettlement();
    static DecimalFormat dfPrice = new DecimalFormat("#0.00");
    private static NumberFormat nf = new DecimalFormat("0.00");

    public static boolean isNumeric(String str) {
        return NumberUtils.isNumber((String) str);
    }

    public static String getFormatCurrentTimeToMilSecond() {
        SimpleDateFormat formatter = new SimpleDateFormat("yyyy-MM-dd HH:mm:ss.SSS");
        return formatter.format(new Date());
    }

    public static WeekInfo getWeekInfoBySettleType(String settleType) {
        String settleType2 = TextUtil.getSettleType2(settleType);
        List<String> lt = TextUtil.calTod0Tod1AndAll2("", "", settleType2);
        WeekInfo wi = new WeekInfo();
        wi.settlementType = TextUtil.getSettleType(settleType2);
        wi.settlementType2 = Utils.ListStrToString(lt);
        try {
            String[] str2 = TextUtil.getDate(wi.settlementType2);
            wi.settlementType2 = TextUtil.getSettleType(wi.settlementType2);
            if (str2 != null && str2.length == 2) {
                wi.dealDate = str2[0];
                wi.settlementDate = str2[1];
            }
        } catch (Exception e) {
            e.printStackTrace();
        }
        return wi;
    }

    public static CalWeekDateResult calWeekDate(CalWeekDateReq req) throws Exception {
        WeekInfo wi;
        CalWeekDateResult result = new CalWeekDateResult();
        String baseDate = req.baseDate;
        String settlmentType = req.settlementType;
        settlmentType = TextUtil.getSettleType2(settlmentType);
        WeekDateSettlement wds = new WeekDateSettlement(baseDate);
        List<String> lt = wds.calTod0Tod1AndAll2(settlmentType);
        result.Data = wi = new WeekInfo();
        wi.baseDate = baseDate;
        wi.settlementType = req.settlementType;
        wi.settlementType2 = Utils.ListStrToString(lt);
        String[] str2 = wds.getDateBySettlementType(wi.settlementType2);
        wi.settlementType2 = TextUtil.getSettleType(wi.settlementType2);
        if (str2 != null && str2.length == 2) {
            wi.dealDate = str2[0];
            wi.settlementDate = str2[1];
        }
        return result;
    }

    public static void clear() {
        wds.clear();
    }

    public static String GetNextWork() throws Exception {
        return wds.GetNextWork("");
    }

    public static String[] getDate(String str) throws Exception {
        return wds.getDateBySettlementType(str);
    }

    public static List<String> calTod0Tod1AndAll2(String mdEntryType, String volume, String tmp) {
        return wds.calTod0Tod1AndAll2(mdEntryType, volume, tmp);
    }

    public static String getSettleType(String SettleType) {
        String str = SettleType;
        if ("+0".equals(SettleType)) {
            str = "1";
        }
        if ("+1".equals(SettleType)) {
            str = "2";
        }
        if ("\u660e\u5929+0".equals(SettleType)) {
            str = "3";
        }
        if ("\u660e\u5929+1".equals(SettleType)) {
            str = "4";
        }
        return str;
    }

    public static String getSettleType2(String SettleType) {
        String str = SettleType;
        if ("1".equals(SettleType)) {
            str = "+0";
        }
        if ("2".equals(SettleType)) {
            str = "+1";
        }
        if ("3".equals(SettleType)) {
            str = "\u660e\u5929+0";
        }
        if ("4".equals(SettleType)) {
            str = "\u660e\u5929+1";
        }
        return str;
    }

    public static BigDecimal calPriceFormat(BigDecimal price) {
        BigDecimal tmp = price;
        if (price != null) {
            boolean isFormat = false;
            String str = price.toPlainString();
            String[] strs = str.split("\\.");
            if (strs.length == 2) {
                if (strs[1].length() < 2) {
                    isFormat = true;
                }
            } else {
                isFormat = true;
            }
            if (isFormat) {
                tmp = new BigDecimal(dfPrice.format(price));
            }
        }
        return tmp;
    }

    public static BigDecimal calScale(BigDecimal value, int scale) {
        if (value != null) {
            value = value.setScale(scale, 4);
        }
        return value;
    }

    public static BigDecimal removeZero(BigDecimal qty) {
        BigDecimal tmp = null;
        if (qty != null) {
            tmp = new BigDecimal(qty.stripTrailingZeros().toPlainString());
        }
        return tmp;
    }

    public static String getFormatCurrentTime() {
        SimpleDateFormat formatter = new SimpleDateFormat("yyyy-MM-dd HH:mm:ss");
        return formatter.format(new Date());
    }

    public static String getFormatCurrentTimeByMillSecond() {
        SimpleDateFormat formatter = new SimpleDateFormat("yyyy-MM-dd HH:mm:ss.SSS");
        return formatter.format(new Date());
    }

    public static String getFormatCurrentTime3() {
        SimpleDateFormat formatter = new SimpleDateFormat("HH:mm:ss");
        return formatter.format(new Date());
    }

    public static String getFormatCurrentTime(Date date) {
        SimpleDateFormat formatter = new SimpleDateFormat("yyyy-MM-dd HH:mm:ss");
        return formatter.format(date);
    }

    public static String getFormatCurrentTimeAdd(String currenttime, int minute)
            throws ParseException {
        SimpleDateFormat formatter = new SimpleDateFormat("yyyy-MM-dd HH:mm:ss");
        Calendar cl = Calendar.getInstance();
        cl.clear();
        cl.setTime(formatter.parse(currenttime));
        cl.add(12, minute);
        return formatter.format(cl.getTime());
    }

    public static String getFormatCurrentTimeAdd(int minute) {
        SimpleDateFormat formatter = new SimpleDateFormat("yyyy-MM-dd HH:mm:ss");
        Calendar cl = Calendar.getInstance();
        cl.add(12, minute);
        return formatter.format(cl.getTime());
    }

    public static String getFormatCurrentTimeAddMillSecond(String currenttime, int millSecond)
            throws ParseException {
        SimpleDateFormat formatter = new SimpleDateFormat("yyyy-MM-dd HH:mm:ss.SSS");
        Calendar cl = Calendar.getInstance();
        cl.clear();
        cl.setTime(formatter.parse(currenttime));
        cl.add(14, millSecond);
        return formatter.format(cl.getTime());
    }

    public static long getFormatCurrentTime(String date) {
        if (!TextUtils.isBlank((CharSequence) date)) {
            SimpleDateFormat formatter = new SimpleDateFormat("yyyy-MM-dd HH:mm:ss");
            try {
                return formatter.parse(date).getTime();
            } catch (ParseException e) {
                e.printStackTrace();
            }
        }
        return 0L;
    }

    public static String getFormatCurrentTime2() {
        SimpleDateFormat formatter = new SimpleDateFormat("yyyyMMddHHmmssSSS");
        return formatter.format(new Date());
    }

    public static String getFormatCurrentTime4() {
        SimpleDateFormat formatter = new SimpleDateFormat("yyyyMMdd HH:mm:ss.SSS");
        return formatter.format(new Date());
    }

    public static String getFormatCurrentDate() {
        SimpleDateFormat formatter = new SimpleDateFormat("yyyy-MM-dd");
        return formatter.format(new Date());
    }

    public static String getFormatLastDate() {
        SimpleDateFormat formatter = new SimpleDateFormat("yyyyMMdd");
        Calendar cl = Calendar.getInstance();
        cl.add(6, -1);
        return formatter.format(cl.getTime());
    }

    public static String getFormatLastDate2() {
        SimpleDateFormat formatter = new SimpleDateFormat("yyyy-MM-dd");
        Calendar cl = Calendar.getInstance();
        cl.add(5, -1);
        return formatter.format(cl.getTime());
    }

    public static String getFormatCurrentDate2() {
        SimpleDateFormat formatter = new SimpleDateFormat("yyyyMMdd");
        return formatter.format(new Date());
    }

    public static String getFormatCurrentDate2(String str) {
        SimpleDateFormat formatter = new SimpleDateFormat("yyyyMMdd");
        String time = null;
        try {
            time = formatter.parse(str).getTime() + "";
        } catch (ParseException e) {
            e.printStackTrace();
        }
        return time;
    }

    public static String DateFormatConvert(String date) {
        String str = date;
        if (date.length() == 8) {
            str = date.substring(0, 4) + "-" + date.substring(4, 6) + "-" + date.substring(6, 8);
        }
        return str;
    }

    public static String getFormatCurrentDate3(String str) {
        String str2 = str;
        SimpleDateFormat formatter = new SimpleDateFormat("yyyyMMdd");
        SimpleDateFormat formatter2 = new SimpleDateFormat("yyyy-MM-dd");
        try {
            str2 = formatter2.format(formatter.parse(str));
        } catch (ParseException e) {
            e.printStackTrace();
        }
        return str2;
    }

    public static String toAmountString(float amount) {
        return nf.format(amount);
    }

    public static String toAmountString(BigDecimal amount) {
        return nf.format(amount);
    }

    public static String toAmountString(double amount) {
        return nf.format(amount);
    }

    public static boolean isNull(String str) {
        boolean flag = str == null || str.trim().length() == 0;
        return flag;
    }
}
