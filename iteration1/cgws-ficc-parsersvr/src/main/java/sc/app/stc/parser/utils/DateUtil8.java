package sc.app.stc.parser.utils;

import java.time.*;
import java.time.format.DateTimeFormatter;
import java.time.temporal.ChronoUnit;
import java.time.temporal.TemporalAdjusters;

/**
 * java1.8 的新特性，解决SimpleDateFormat的线程问题
 */
@SuppressWarnings("unused")
public class DateUtil8 {
    // 时间元素
    public static final String YEAR = "year";
    public static final String MONTH = "month";
    public static final String WEEK = "week";
    public static final String DAY = "day";
    public static final String HOUR = "hour";
    public static final String MINUTE = "minute";
    public static final String SECOND = "second";

    // 星期元素
    private static final String MONDAY = "MONDAY";// 星期一
    private static final String TUESDAY = "TUESDAY";// 星期二
    private static final String WEDNESDAY = "WEDNESDAY";// 星期三
    private static final String THURSDAY = "THURSDAY";// 星期四
    private static final String FRIDAY = "FRIDAY";// 星期五
    private static final String SATURDAY = "SATURDAY";// 星期六
    private static final String SUNDAY = "SUNDAY";// 星期日

    // 根据指定格式显示日期和时间
    /**
     * yyyy-MM-dd
     */
    private static final DateTimeFormatter yyyyMMdd = DateTimeFormatter.ofPattern("yyyyMMdd");
    /**
     * yyyy-MM-dd
     */
    private static final DateTimeFormatter yyyyMMdd_EN = DateTimeFormatter.ofPattern("yyyy-MM-dd");
    /**
     * yyyy-MM-dd HH
     */
    private static final DateTimeFormatter yyyyMMddHH_EN = DateTimeFormatter.ofPattern("yyyy-MM-dd HH");
    /**
     * yyyy-MM-dd HH:mm
     */
    private static final DateTimeFormatter yyyyMMddHHmm_EN = DateTimeFormatter.ofPattern("yyyy-MM-dd HH:mm");
    /**
     * yyyy-MM-dd HH:mm:ss
     */
    private static final DateTimeFormatter yyyyMMddHHmmss_EN = DateTimeFormatter.ofPattern("yyyy-MM-dd HH:mm:ss");
    /**
     * HH:mm:ss
     */
    private static final DateTimeFormatter HHmmss_EN = DateTimeFormatter.ofPattern("HH:mm:ss");
    /**
     * yyyy年MM月dd日
     */
    private static final DateTimeFormatter yyyyMMdd_CN = DateTimeFormatter.ofPattern("yyyy年MM月dd日");
    /**
     * yyyy年MM月dd日HH时
     */
    private static final DateTimeFormatter yyyyMMddHH_CN = DateTimeFormatter.ofPattern("yyyy年MM月dd日HH时");
    /**
     * yyyy年MM月dd日HH时mm分
     */
    private static final DateTimeFormatter yyyyMMddHHmm_CN = DateTimeFormatter.ofPattern("yyyy年MM月dd日HH时mm分");
    /**
     * yyyy年MM月dd日HH时mm分ss秒
     */
    private static final DateTimeFormatter yyyyMMddHHmmss_CN = DateTimeFormatter.ofPattern("yyyy年MM月dd日HH时mm分ss秒");
    /**
     * HH时mm分ss秒
     */
    private static final DateTimeFormatter HHmmss_CN = DateTimeFormatter.ofPattern("HH时mm分ss秒");

    /**
     * 获取当前日期（yyyyMMdd）
     */
    public static String getNowDate_yMd() {
        return LocalDate.now().format(yyyyMMdd);
    }

    /**
     * 获取当前日期（yyyy-MM-dd）
     */
    public static String getNowDate_EN_yMd() {
        return LocalDate.now().format(yyyyMMdd_EN);
    }

    /**
     * 获取当前日期（yyyy年MM月dd日）
     */
    public static String getNowDate_CN_yMd() {
        return LocalDate.now().format(yyyyMMdd_CN);
    }

    /**
     * 获取当前时间（yyyy-MM-dd HH:mm:ss）
     */
    public static String getNowTime_EN_yMdHms() {
        return LocalDateTime.now().format(yyyyMMddHHmmss_EN);
    }

    /**
     * 获取当前时间（yyyy年MM月dd日HH时mm分ss秒）
     */
    public static String getNowTime_CN_yMdHms() {
        return LocalDateTime.now().format(yyyyMMddHHmmss_CN);
    }

    /**
     * 获取当前时间（yyyy-MM-dd HH）
     */
    public static String getNowTime_EN_yMdH() {
        return LocalDateTime.now().format(yyyyMMddHH_EN);
    }

    /**
     * 获取当前时间（yyyy年MM月dd日HH时）
     */
    public static String getNowTime_CN_yMdH() {
        return LocalDateTime.now().format(yyyyMMddHH_CN);
    }

    /**
     * 获取当前时间（yyyy-MM-dd HH:mm）
     */
    public static String getNowTime_EN_yMdHm() {
        return LocalDateTime.now().format(yyyyMMddHHmm_EN);
    }

    /**
     * 获取当前时间（yyyy年MM月dd日HH时mm分）
     */
    public static String getNowTime_CN_yMdHm() {
        return LocalDateTime.now().format(yyyyMMddHHmm_CN);
    }

    /**
     * 获取当前时间（HH:mm:ss）
     */
    public static String getNowTime_EN_Hms() {
        return LocalDateTime.now().format(HHmmss_EN);
    }

    /**
     * 获取当前时间（HH时mm分ss秒）
     */
    public static String getNowTime_CN_Hms() {
        return LocalDateTime.now().format(HHmmss_CN);
    }

    /**
     * 根据日期格式，获取当前时间
     */
    public static String getNowTime(String formatStr) {
        return LocalDateTime.now().format(DateTimeFormatter.ofPattern(formatStr));
    }

    /**
     * 获取当前日期的节点时间（年，月，周，日，时，分，秒）
     */
    public static Integer getNodeTime(String node) {
        LocalDateTime today = LocalDateTime.now();
        int resultNode;
        switch (node) {
            case YEAR:
                resultNode = today.getYear();
                break;
            case MONTH:
                resultNode = today.getMonthValue();
                break;
            case WEEK:
                resultNode = transformWeekEN2Num(String.valueOf(today.getDayOfWeek()));
                break;
            case DAY:
                resultNode = today.getDayOfMonth();
                break;
            case HOUR:
                resultNode = today.getHour();
                break;
            case MINUTE:
                resultNode = today.getMinute();
                break;
            case SECOND:
                resultNode = today.getSecond();
                break;
            default:
                // 当前日期是当前年的第几天。
                resultNode = today.getDayOfYear();
                break;
        }
        return resultNode;
    }

    /**
     * 将英文星期转换成数字
     */
    public static int transformWeekEN2Num(String enWeek) {
        if (MONDAY.equals(enWeek)) {
            return 1;
        } else if (TUESDAY.equals(enWeek)) {
            return 2;
        } else if (WEDNESDAY.equals(enWeek)) {
            return 3;
        } else if (THURSDAY.equals(enWeek)) {
            return 4;
        } else if (FRIDAY.equals(enWeek)) {
            return 5;
        } else if (SATURDAY.equals(enWeek)) {
            return 6;
        } else if (SUNDAY.equals(enWeek)) {
            return 7;
        } else {
            return -1;
        }
    }

    /**
     * 获取当前日期之后（之后）的节点事件<br>
     * <ul>
     * 比如当前时间为：2019-03-30 10:20:30
     * </ul>
     * <li>node="hour",num=5L:2019-03-30 15:20:30</li>
     * <li>node="day",num=1L:2019-03-31 10:20:30</li>
     * <li>node="year",num=1L:2020-03-30 10:20:30</li>
     */
    public static String getAfterOrPreNowTime(String node, Long num) {
        LocalDateTime now = LocalDateTime.now();
        if (HOUR.equals(node)) {
            return now.plusHours(num).format(yyyyMMddHHmmss_EN);
        } else if (DAY.equals(node)) {
            return now.plusDays(num).format(yyyyMMddHHmmss_EN);
        } else if (WEEK.equals(node)) {
            return now.plusWeeks(num).format(yyyyMMddHHmmss_EN);
        } else if (MONTH.equals(node)) {
            return now.plusMonths(num).format(yyyyMMddHHmmss_EN);
        } else if (YEAR.equals(node)) {
            return now.plusYears(num).format(yyyyMMddHHmmss_EN);
        } else if (MINUTE.equals(node)) {
            return now.plusMinutes(num).format(yyyyMMddHHmmss_EN);
        } else if (SECOND.equals(node)) {
            return now.plusSeconds(num).format(yyyyMMddHHmmss_EN);
        } else {
            return "Node is Error!";
        }
    }

    /**
     * 获取与当前日期相距num个之后（之前）的日期<br>
     * <ul>
     * 比如当前时间为：2019-03-30 10:20:30的格式日期
     * <li>node="hour",num=5L:2019-03-30 15:20:30</li>
     * <li>node="day",num=1L:2019-03-31 10:20:30</li>
     * <li>node="year",num=1L:2020-03-30 10:20:30</li>
     * </ul>
     */
    public static String getAfterOrPreNowTimePlus(DateTimeFormatter dtf, String node, Long num) {
        LocalDateTime now = LocalDateTime.now();
        if (HOUR.equals(node)) {
            return now.plusHours(num).format(dtf);
        } else if (DAY.equals(node)) {
            return now.plusDays(num).format(dtf);
        } else if (WEEK.equals(node)) {
            return now.plusWeeks(num).format(dtf);
        } else if (MONTH.equals(node)) {
            return now.plusMonths(num).format(dtf);
        } else if (YEAR.equals(node)) {
            return now.plusYears(num).format(dtf);
        } else if (MINUTE.equals(node)) {
            return now.plusMinutes(num).format(dtf);
        } else if (SECOND.equals(node)) {
            return now.plusSeconds(num).format(dtf);
        } else {
            return "Node is Error!";
        }
    }

    /**
     * 当前时间的hour，minute，second之后（之前）的时刻
     */
    public static String getAfterOrPreNowTimeSimple(String node, Long num) {
        LocalTime now = LocalTime.now();
        if (HOUR.equals(node)) {
            return now.plusHours(num).format(HHmmss_EN);
        } else if (MINUTE.equals(node)) {
            return now.plusMinutes(num).format(HHmmss_EN);
        } else if (SECOND.equals(node)) {
            return now.plusSeconds(num).format(HHmmss_EN);
        } else {
            return "Node is Error!";
        }
    }

    /**
     * 获取当前日期第index日之后(之前)的日期（yyyy-MM-dd）
     */
    public static String getAfterOrPreDayDate(int index) {
        return LocalDate.now().plus(index, ChronoUnit.DAYS).format(yyyyMMdd_EN);
    }

    /**
     * 获取当前日期第index周之前（之后）的日期（yyyy-MM-dd）
     */
    public static String getAfterOrPreWeekDate(int index) {
        return LocalDate.now().plus(index, ChronoUnit.WEEKS).format(yyyyMMdd_EN);
    }

    /**
     * 获取当前日期第index月之前（之后）的日期（yyyy-MM-dd）
     */
    public static String getAfterOrPreMonthDate(int index) {
        return LocalDate.now().plus(index, ChronoUnit.MONTHS).format(yyyyMMdd_EN);
    }

    /**
     * 获取当前日期第index年之前（之后）的日期（yyyy-MM-dd）
     */
    public static String getAfterOrPreYearDate(int index) {
        return LocalDate.now().plus(index, ChronoUnit.YEARS).format(yyyyMMdd_EN);
    }

    /**
     * 获取指定日期之前之后的第index的日，周，月，年的日期
     */
    public static String getAfterOrPreDate(String date, String node, int index) {
        date = date.trim();
        if (DAY.equals(node)) {
            return LocalDate.parse(date).plus(index, ChronoUnit.DAYS).format(yyyyMMdd_EN);
        } else if (WEEK.equals(node)) {
            return LocalDate.parse(date).plus(index, ChronoUnit.WEEKS).format(yyyyMMdd_EN);
        } else if (MONTH.equals(node)) {
            return LocalDate.parse(date).plus(index, ChronoUnit.MONTHS).format(yyyyMMdd_EN);
        } else if (YEAR.equals(node)) {
            return LocalDate.parse(date).plus(index, ChronoUnit.YEARS).format(yyyyMMdd_EN);
        } else {
            return "Wrong date format!";
        }
    }

    /**
     * 检测：输入年份是否是闰年？
     */
    public static boolean isLeapYear(String date) {
        return LocalDate.parse(date.trim()).isLeapYear();
    }

    /**
     * 计算两个日期字符串之间相差多少个周期（天，月，年）
     */
    public static int periodCount(String date1, String date2, String node) {
        date1 = date1.trim();
        date2 = date2.trim();
        if (DAY.equals(node)) {
            return Period.between(LocalDate.parse(date1), LocalDate.parse(date2)).getDays();
        } else if (MONTH.equals(node)) {
            return Period.between(LocalDate.parse(date1), LocalDate.parse(date2)).getMonths();
        } else if (YEAR.equals(node)) {
            return Period.between(LocalDate.parse(date1), LocalDate.parse(date2)).getYears();
        } else {
            return 0;
        }
    }

    /**
     * 指定日期月的最后一天（yyyy-MM-dd）
     */
    public static String getLastDayOfMonth(String curDate, boolean firstOrLast) {
        if (firstOrLast) {
            return LocalDate.parse(curDate, yyyyMMdd_EN).with(TemporalAdjusters.firstDayOfMonth()).toString();
        } else {
            return LocalDate.parse(curDate, yyyyMMdd_EN).with(TemporalAdjusters.lastDayOfMonth()).toString();
        }
    }

    /**
     * 指定日期年的最后一天（yyyy-MM-dd）
     */
    public static String getLastDayOfYear(String curDate, boolean firstOrLast) {
        if (firstOrLast) {
            return LocalDate.parse(curDate, yyyyMMdd_EN).with(TemporalAdjusters.firstDayOfYear()).toString();
        } else {
            return LocalDate.parse(curDate, yyyyMMdd_EN).with(TemporalAdjusters.lastDayOfYear()).toString();
        }
    }

    /**
     * 获取下一个星期的日期
     */
    public static String getNextWeekDate(String curDay, int dayOfWeek, boolean isContainCurDay) {
        dayOfWeek = dayOfWeek < 1 || dayOfWeek > 7 ? 1 : dayOfWeek;
        if (isContainCurDay) {
            return LocalDate.parse(curDay).with(TemporalAdjusters.nextOrSame(DayOfWeek.of(dayOfWeek))).toString();
        } else {
            return LocalDate.parse(curDay).with(TemporalAdjusters.next(DayOfWeek.of(dayOfWeek))).toString();
        }
    }

    /**
     * 获取上一个星期的日期
     */
    public static String getPreWeekDate(String curDay, int dayOfWeek, boolean isCurDay) {
        dayOfWeek = dayOfWeek < 1 || dayOfWeek > 7 ? 1 : dayOfWeek;
        if (isCurDay) {
            return LocalDate.parse(curDay).with(TemporalAdjusters.previousOrSame(DayOfWeek.of(dayOfWeek))).toString();
        } else {
            return LocalDate.parse(curDay).with(TemporalAdjusters.previous(DayOfWeek.of(dayOfWeek))).toString();
        }
    }

    /**
     * 获取指定日期当月的最后或第一个星期日期
     */
    public static String getFirstOrLastWeekDate(String curDay, int dayOfWeek, boolean lastOrFirst) {
        dayOfWeek = dayOfWeek < 1 || dayOfWeek > 7 ? 1 : dayOfWeek;
        if (lastOrFirst) {
            return LocalDate.parse(curDay).with(TemporalAdjusters.lastInMonth(DayOfWeek.of(dayOfWeek))).toString();
        } else {
            return LocalDate.parse(curDay).with(TemporalAdjusters.firstInMonth(DayOfWeek.of(dayOfWeek))).toString();
        }
    }
}