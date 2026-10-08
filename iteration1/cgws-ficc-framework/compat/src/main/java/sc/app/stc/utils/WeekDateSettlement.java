// Recovered compatibility source; provenance: recovered-support.json.
package sc.app.stc.utils;

import org.apache.http.util.TextUtils;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import sc.app.stc.connect.resource.SettlementType;

import java.text.ParseException;
import java.text.SimpleDateFormat;
import java.util.ArrayList;
import java.util.Calendar;
import java.util.Comparator;
import java.util.Date;
import java.util.HashMap;
import java.util.List;
import java.util.SortedSet;
import java.util.concurrent.ConcurrentSkipListSet;
import java.util.concurrent.locks.ReadWriteLock;
import java.util.concurrent.locks.ReentrantReadWriteLock;

public class WeekDateSettlement {
    private Logger logger = LoggerFactory.getLogger(this.getClass());
    SortedSet<String> holidays = new ConcurrentSkipListSet<String>(new StrAscComparator());
    HashMap<String, String> htDayToWeek = new HashMap();
    HashMap<String, String> htWeekToDate = new HashMap();
    HashMap<String, String> htSettleToNextWorkDate = new HashMap();
    String baseDate;
    String nextWorkDate;
    public ReadWriteLock weeklock = new ReentrantReadWriteLock();

    public String getBaseDate() {
        return this.baseDate;
    }

    public void setBaseDate(String baseDate) {
        this.baseDate = baseDate;
    }

    public WeekDateSettlement() {}

    public WeekDateSettlement(String baseDate) {
        this.baseDate = baseDate;
    }

    public void clear() {
        try {
            this.weeklock.writeLock().lock();
            this.htDayToWeek.clear();
            this.htWeekToDate.clear();
            this.htSettleToNextWorkDate.clear();
            this.holidays.clear();
            this.baseDate = "";
            this.nextWorkDate = "";
        } finally {
            this.weeklock.writeLock().unlock();
        }
    }

    public String getSettlemntDate(String settlementTypes) throws Exception {
        List<String> lt = this.calTod0Tod1AndAll2(settlementTypes);
        if (lt != null && lt.size() == 1) {
            String[] strs = this.getDateBySettlementType(lt.get(0));
            return strs[1];
        }
        return "";
    }

    public String getDealDate(String settlementTypes) throws Exception {
        List<String> lt = this.calTod0Tod1AndAll2(settlementTypes);
        if (lt != null && lt.size() == 1) {
            String[] strs = this.getDateBySettlementType(lt.get(0));
            return strs[0];
        }
        return "";
    }

    private String getDateByWeek(String str) {
        String date;
        try {
            this.weeklock.readLock().lock();
            date = this.htWeekToDate.get(str);
        } finally {
            this.weeklock.readLock().unlock();
        }
        return date;
    }

    /*
     * WARNING - Removed try catching itself - possible behaviour change.
     */
    public String[] getDateBySettlementType(String str) throws Exception {
        String[] result = null;
        try {
            this.weeklock.readLock().lock();
            long begin = System.currentTimeMillis();
            this.initWeek();
            long end = System.currentTimeMillis();
            this.logger.debug("{}:\u8017\u65f6initWeek:{}", (Object) str, (Object) (end - begin));
            if (str != null) {
                begin = System.currentTimeMillis();
                result = new String[2];
                String[] strs = str.split("\\+");
                int day = 0;
                if (strs != null && strs.length > 1) {
                    day = Integer.parseInt(strs[1]);
                }
                str = strs[0];
                String tmp = this.getDateByWeek(str);
                Calendar cal = Calendar.getInstance();
                SimpleDateFormat sdf2 = new SimpleDateFormat("yyyyMMdd");
                Date date = sdf2.parse(this.baseDate);
                cal.setTime(date);
                if (!TextUtils.isBlank((CharSequence) tmp)) {
                    result[0] = str = tmp;
                    Date dt = sdf2.parse(str);
                    cal.setTime(dt);
                    cal.add(6, day);
                } else {
                    SimpleDateFormat sdf = new SimpleDateFormat("MM.dd");
                    String str1 = sdf.format(cal.getTime());
                    int year = cal.get(1);
                    if (str1.compareTo(str) > 0) {
                        ++year;
                    }
                    str = year + str;
                    SimpleDateFormat sdf1 = new SimpleDateFormat("yyyyMM.dd");
                    Date dt = sdf1.parse(str);
                    cal.setTime(dt);
                    cal.set(1, year);
                    result[0] = sdf2.format(cal.getTime());
                    cal.add(6, day);
                }
                tmp = sdf2.format(cal.getTime());
                begin = System.currentTimeMillis();
                if (this.IsHoliday(tmp)) {
                    tmp = this.GetNextWork(tmp);
                }
                end = System.currentTimeMillis();
                this.logger.debug(
                        "{}:\u8017\u65f6GetNextWork:{}", (Object) str, (Object) (end - begin));
                result[1] = tmp;
            }
        } finally {
            this.weeklock.readLock().unlock();
        }
        return result;
    }

    private String getWeekByDate(String str) {
        String date;
        try {
            this.weeklock.readLock().lock();
            date = this.htWeekToDate.get(str);
        } finally {
            this.weeklock.readLock().unlock();
        }
        return date;
    }

    public String getSimpleDate(String dateStr) {
        String day = this.getWeekByDate(dateStr);
        if (day == null) {
            day = dateStr;
        }
        if (day != null) {
            SimpleDateFormat sdf2 = new SimpleDateFormat("yyyyMMdd");
            SimpleDateFormat sdf = new SimpleDateFormat("MM.dd");
            try {
                day = sdf.format(sdf2.parse(day));
            } catch (ParseException e) {
                e.printStackTrace();
            }
        }
        return day;
    }

    public void calTom() throws Exception {
        String tom = this.nextWorkDate;
        if (TextUtils.isBlank((CharSequence) tom)) {
            tom = this.GetNextWork(this.baseDate);
            String tomStr = "\u660e\u5929";
            String tomStr1 = "\u660e";
            this.htWeekToDate.put(tomStr, tom);
            this.htWeekToDate.put(tomStr1, tom);
            this.logger.debug(tomStr + ":" + tom);
            tom = this.getSimpleDate(tom);
            this.htDayToWeek.put(tom, tomStr);
            this.logger.debug(tom + ":" + tomStr);
        }
    }

    public void initNextWeek(String dateStr) throws ParseException {
        String[] nextStrs =
                new String[] {
                    "\u4e0b\u5468\u65e5",
                    "\u4e0b\u5468\u4e00",
                    "\u4e0b\u5468\u4e8c",
                    "\u4e0b\u5468\u4e09",
                    "\u4e0b\u5468\u56db",
                    "\u4e0b\u5468\u4e94",
                    "\u4e0b\u5468\u516d"
                };
        String[] nextStrs2 =
                new String[] {
                    "\u4e0b\u5468\u672b",
                    "\u4e0b\u54681",
                    "\u4e0b\u54682",
                    "\u4e0b\u54683",
                    "\u4e0b\u54684",
                    "\u4e0b\u54685",
                    "\u4e0b\u54686"
                };
        SimpleDateFormat sdf2 = new SimpleDateFormat("yyyyMMdd");
        SimpleDateFormat sdf = new SimpleDateFormat("MM.dd");
        Date date = sdf2.parse(dateStr);
        Calendar cal = Calendar.getInstance();
        cal.setTime(date);
        int dayOfWeek = cal.get(7) - 1;
        if (dayOfWeek <= 0) {
            dayOfWeek = 7;
        }
        int offset1 = 1 - dayOfWeek;
        cal.add(5, offset1 + 7);
        for (int i = 0; i < 7; ++i) {
            String str12 = sdf2.format(cal.getTime());
            String str1 = sdf.format(cal.getTime());
            String str2 = "";
            String str3 = "";
            int w = cal.get(7) - 1;
            if (w < 0) {
                w = 0;
            }
            str2 = nextStrs[w];
            str3 = nextStrs2[w];
            this.logger.debug(str2 + ":" + str12);
            if (!this.htWeekToDate.containsKey(str2)) {
                this.htWeekToDate.put(str2, str12);
                this.htWeekToDate.put(str3, str12);
            }
            if (i < 0) {
                this.htDayToWeek.put(str1, str2);
                this.logger.debug(str1 + ":" + str2);
            }
            cal.add(6, 1);
        }
    }

    /*
     * WARNING - Removed try catching itself - possible behaviour change.
     */
    private void initWeek() {
        try {
            if (this.htWeekToDate.size() == 0) {
                String[] strs =
                        new String[] {
                            "\u5468\u65e5",
                            "\u5468\u4e00",
                            "\u5468\u4e8c",
                            "\u5468\u4e09",
                            "\u5468\u56db",
                            "\u5468\u4e94",
                            "\u5468\u516d"
                        };
                String[] strs2 =
                        new String[] {
                            "\u5468\u672b",
                            "\u54681",
                            "\u54682",
                            "\u54683",
                            "\u54684",
                            "\u54685",
                            "\u54686"
                        };
                SimpleDateFormat sdf2 = new SimpleDateFormat("yyyyMMdd");
                SimpleDateFormat sdf = new SimpleDateFormat("MM.dd");
                Calendar cal = Calendar.getInstance();
                if (TextUtils.isBlank((CharSequence) this.baseDate)) {
                    this.baseDate = TextUtil.getFormatCurrentDate2();
                }
                Date date = sdf2.parse(this.baseDate);
                cal.setTime(date);
                for (int i = 0; i < 9; ++i) {
                    String str12 = sdf2.format(cal.getTime());
                    String str1 = sdf.format(cal.getTime());
                    String str2 = "";
                    String str3 = "";
                    int w = cal.get(7) - 1;
                    if (w < 0) {
                        w = 0;
                    }
                    str2 = strs[w];
                    str3 = strs2[w];
                    this.logger.debug(str2 + ":" + str12);
                    if (!this.htWeekToDate.containsKey(str2)) {
                        if (i != 0) {
                            this.htWeekToDate.put(str2, str12);
                            this.htWeekToDate.put(str3, str12);
                        }
                        if (i == 2) {
                            this.htWeekToDate.put("\u540e\u5929", str12);
                        }
                    }
                    if (i == 0) {
                        str2 = "";
                        this.htWeekToDate.put(str2, str12);
                    }
                    if (i < 1) {
                        this.htDayToWeek.put(str1, str2);
                        this.logger.debug(str1 + ":" + str2);
                    }
                    cal.add(6, 1);
                }
                this.initNextWeek(this.baseDate);
            }
            this.calTom();
        } catch (Exception e) {
            this.logger.error("init week:" + this.baseDate, (Throwable) e);
        }
    }

    private String getWeekByDay(String str) {
        String date = null;
        try {
            this.weeklock.readLock().lock();
            date = this.htDayToWeek.get(str);
        } finally {
            this.weeklock.readLock().unlock();
        }
        return date;
    }

    public String calDayToWeekDay(String str) {
        this.initWeek();
        String tmp = this.getWeekByDay(str);
        if (tmp == null) {
            tmp = str;
        }
        return tmp;
    }

    public String calTod0Tod1AndAll(String tmp) {
        List<String> lt = this.calTod0Tod1AndAll2(tmp);
        String str = null;
        if (lt.size() > 0) {
            str = lt.get(0);
        }
        return str;
    }

    public String isDateFormat(String dateStr, String str2) {
        String str = dateStr;
        try {
            SimpleDateFormat sdf = new SimpleDateFormat("MM.dd");
            sdf.setLenient(false);
            SimpleDateFormat sdf1 = new SimpleDateFormat("yyyyMM.dd");
            sdf1.setLenient(false);
            SimpleDateFormat sdf3 = new SimpleDateFormat("yyyyMM/dd");
            sdf3.setLenient(false);
            SimpleDateFormat sdf4 = new SimpleDateFormat("yyyyMM-dd");
            sdf4.setLenient(false);
            SimpleDateFormat sdf5 = new SimpleDateFormat("yyyyMM\u6708dd\u53f7");
            sdf5.setLenient(false);
            SimpleDateFormat sdf6 = new SimpleDateFormat("yyyyMM\u6708dd\u65e5");
            sdf6.setLenient(false);
            Date date = null;
            int year = Calendar.getInstance().get(1);
            dateStr = year + dateStr;
            if (!dateStr.contains("\u4ebf")) {
                SimpleDateFormat sdf2;
                if (dateStr.contains(".") && TextUtil.isNumeric(dateStr)) {
                    date = sdf1.parse(dateStr);
                } else if (dateStr.contains("/")) {
                    date = sdf3.parse(dateStr);
                } else if (dateStr.contains("-")) {
                    date = sdf4.parse(dateStr);
                } else if (dateStr.contains("\u53f7") && dateStr.contains("\u6708")) {
                    date = sdf5.parse(dateStr);
                } else if (dateStr.contains("\u53f7") && dateStr.length() < 5) {
                    sdf2 = new SimpleDateFormat("yyyyMMdd");
                    Calendar cal = Calendar.getInstance();
                    cal.setTime(sdf2.parse(this.baseDate));
                    String dayMon = cal.get(2) + 1 + "\u6708";
                    date = sdf5.parse(dayMon + dateStr);
                } else if (dateStr.contains("\u65e5") && dateStr.contains("\u6708")) {
                    date = sdf6.parse(dateStr);
                } else if (dateStr.contains("\u65e5") && dateStr.length() < 5) {
                    sdf2 = new SimpleDateFormat("yyyyMMdd");
                    Calendar cal = Calendar.getInstance();
                    cal.setTime(sdf2.parse(this.baseDate));
                    String dayMon = cal.get(2) + 1 + "\u6708";
                    date = sdf6.parse(dayMon + dateStr);
                }
                String str3 = sdf.format(date);
                str = this.calDayToWeekDay(str3);
                if (str.equals("\u660e\u5929") && str2.equals("1")) {
                    str = str3;
                }
            } else {
                str = null;
            }
        } catch (Exception e) {
            str = null;
        }
        return str;
    }

    public List<String> calTod0Tod1AndAll2(String tmp) {
        return this.calTod0Tod1AndAll2("", "", tmp);
    }

    public List<String> calTod0Tod1AndAll2(String mdEntryType, String volume, String tmp) {
        List<String> lt = this.calTod0Tod1AndAll2(tmp, 0);
        return lt;
    }

    private boolean isContainFwd(List<String> result) {
        boolean flag = false;
        for (String str : result) {
            if (!str.contains(".")) continue;
            flag = true;
        }
        return flag;
    }

    /*
     * WARNING - Removed try catching itself - possible behaviour change.
     */
    public List<String> calTod0Tod1AndAll2(String tmp, int count) {
        ArrayList<String> result = new ArrayList<String>();
        if (++count >= 3) {
            this.logger.warn("calTod0Tod1AndAll2:{}", (Object) tmp);
            return result;
        }
        try {
            this.weeklock.readLock().lock();
            this.initWeek();
            if (tmp != null) {
                char[] strs = tmp.toCharArray();
                for (int i = 0; i < strs.length; ++i) {
                    char str2;
                    int endIndex;
                    char str = strs[i];
                    if (str != '+'
                            || (endIndex = i + 1) >= strs.length
                            || (str2 = strs[endIndex]) != '0' && str2 != '1') continue;
                    int start = 5;
                    int end = 1;
                    boolean flag = false;
                    for (int j = start; j >= end; --j) {
                        int perIndex = i - j;
                        if (perIndex < 0) continue;
                        String dateStr = tmp.substring(perIndex, i).trim();
                        String fStr = this.isDateFormat(dateStr, str2 + "");
                        if (fStr != null) {
                            result.add(fStr + "+" + str2);
                            flag = true;
                            break;
                        }
                        String settlementType = dateStr + "+" + str2;
                        if (SettlementType.isNextWeek(settlementType)
                                && TextUtils.isBlank((CharSequence) this.getWeekByDate(dateStr)))
                            continue;
                        if (SettlementType.isWeek(settlementType)
                                || SettlementType.isNextWeek(settlementType)
                                || "\u660e\u5929+1".equals(settlementType)
                                || "\u660e+1".equals(settlementType)
                                || "\u540e\u5929+0".equals(settlementType)
                                || "\u540e\u5929+1".equals(settlementType)) {
                            String day = this.getSimpleDate(dateStr);
                            if (day != null) {
                                List<String> lts = this.calTod0Tod1AndAll2(day + "+" + str2, count);
                                if (lts.size() != 0) {
                                    settlementType = lts.get(0);
                                } else {
                                    System.err.println(day + "+" + str2);
                                }
                            }
                            if (!result.contains(settlementType)) {
                                result.add(settlementType);
                            }
                            flag = true;
                        }
                        if (SettlementType.t0IsT1(settlementType)) {
                            if (!result.contains("+1") && !this.isContainFwd(result)) {
                                result.add("+1");
                            }
                            flag = true;
                        }
                        if (SettlementType.t0CanT1(settlementType)) {
                            if (!result.contains("+1") && !this.isContainFwd(result)) {
                                result.add("+1");
                            }
                            if (!result.contains("+0")) {
                                result.add("+0");
                            }
                            flag = true;
                        }
                        if ("\u660e\u5929+0".equals(settlementType)) {
                            if (!result.contains("\u660e\u5929+0")) {
                                result.add("\u660e\u5929+0");
                            }
                            flag = true;
                        } else if ("\u660e+0".equals(settlementType)) {
                            if (!result.contains("\u660e+0")) {
                                result.add("\u660e\u5929+0");
                            }
                            flag = true;
                        } else if ("\u660e\u5929+1".equals(settlementType)) {
                            if (!result.contains("\u660e\u5929+1")) {
                                result.add("\u660e\u5929+1");
                            }
                            flag = true;
                        } else if ("\u660e+1".equals(settlementType)) {
                            if (!result.contains("\u660e+1")) {
                                result.add("\u660e\u5929+1");
                            }
                            flag = true;
                        }
                        if (flag) break;
                    }
                    if (flag) continue;
                    if (str2 == '0' && !result.contains("+0")) {
                        result.add("+0");
                    }
                    if (str2 != '1' || result.contains("+1")) continue;
                    result.add("+1");
                }
            }
        } finally {
            this.weeklock.readLock().unlock();
        }
        return result;
    }

    public void addHoliday(String holiday) {
        this.holidays.add(holiday);
    }

    public boolean IsHoliday(String date) {
        boolean flag = false;
        flag =
                BaseSvrClient.isLoadAllHoliday
                        ? BaseSvrClient.IsHoliday(date)
                        : this.holidays.contains(date);
        return flag;
    }

    public String GetNextWork(String dateStr) throws Exception {
        String nextDay = "";
        SimpleDateFormat sdf2 = new SimpleDateFormat("yyyyMMdd");
        Calendar cal = Calendar.getInstance();
        if (TextUtils.isBlank((CharSequence) dateStr)) {
            dateStr = TextUtil.getFormatCurrentDate2();
        }
        Date date = sdf2.parse(dateStr);
        cal.setTime(date);
        do {
            cal.add(6, 1);
        } while (this.IsHoliday(nextDay = sdf2.format(cal.getTime())));
        return nextDay;
    }

    public static class StrAscComparator implements Comparator<String> {
        @Override
        public int compare(String o1, String o2) {
            String x = o1;
            String y = o2;
            int t = x.compareTo(y);
            return t;
        }
    }
}
