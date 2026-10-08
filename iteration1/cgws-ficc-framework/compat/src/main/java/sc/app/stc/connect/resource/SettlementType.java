// Recovered compatibility source; provenance: recovered-support.json.
package sc.app.stc.connect.resource;

import sc.app.stc.utils.Utils;

import java.util.ArrayList;
import java.util.List;

public class SettlementType {
    public static final String All = "all";
    public static final String Tod0 = "+0";
    public static final String Tod1 = "+1";
    public static final String Tom0 = "\u660e\u5929+0";
    public static final String Tom0_1 = "\u660e+0";
    public static final String Tomy1 = "\u660e\u5929+1";
    public static final String Tomy1_1 = "\u660e+1";
    public static final String Tomy2 = "\u540e\u5929+0";
    public static final String Tomy2_2 = "\u540e\u5929+1";
    static List<String> nextWeekLt = new ArrayList<String>();
    static List<String> weekLt = new ArrayList<String>();
    static List<String> exceptionLt = new ArrayList<String>();
    public static final String Week1 = "\u5176\u5b83";
    public static final String AllSpot = "allSpot";
    public static final String AllFwd = "allFwd";

    public static boolean isException(String plainComment) {
        boolean flag = false;
        for (String str : exceptionLt) {
            if (!plainComment.contains(str)
                    || plainComment.contains("\u53ef\u8fdc\u671f")
                    || plainComment.contains("\u53ef\u4ee5\u8fdc\u671f")) continue;
            flag = true;
            break;
        }
        return flag;
    }

    public static boolean isWeek(String week) {
        return weekLt.contains(week);
    }

    public static boolean isNextWeek(String week) {
        return nextWeekLt.contains(week);
    }

    public static boolean t0IsT1(String str) {
        boolean flag = false;
        if ("\u4e0d+0".equals(str)) {
            flag = true;
        } else if ("\u975e+0".equals(str)) {
            flag = true;
        }
        return flag;
    }

    public static boolean t0CanT1(String str) {
        boolean flag = false;
        if ("\u53ef+0".equals(str)) {
            flag = true;
        } else if ("\u53ef\u4ee5+0".equals(str)) {
            flag = true;
        }
        return flag;
    }

    static {
        weekLt.add("\u5468\u4e00+0");
        weekLt.add("\u5468\u4e8c+0");
        weekLt.add("\u5468\u4e09+0");
        weekLt.add("\u5468\u56db+0");
        weekLt.add("\u5468\u4e94+0");
        weekLt.add("\u5468\u516d+0");
        weekLt.add("\u5468\u65e5+0");
        weekLt.add("\u5468\u4e00+1");
        weekLt.add("\u5468\u4e8c+1");
        weekLt.add("\u5468\u4e09+1");
        weekLt.add("\u5468\u56db+1");
        weekLt.add("\u5468\u4e94+1");
        weekLt.add("\u5468\u516d+1");
        weekLt.add("\u5468\u65e5+1");
        weekLt.add("\u54681+0");
        weekLt.add("\u54682+0");
        weekLt.add("\u54683+0");
        weekLt.add("\u54684+0");
        weekLt.add("\u54685+0");
        weekLt.add("\u54686+0");
        weekLt.add("\u5468\u672b+0");
        weekLt.add("\u54681+1");
        weekLt.add("\u54682+1");
        weekLt.add("\u54683+1");
        weekLt.add("\u54684+1");
        weekLt.add("\u54685+1");
        weekLt.add("\u54686+1");
        weekLt.add("\u5468\u672b+1");
        nextWeekLt.add("\u4e0b\u5468\u4e00+0");
        nextWeekLt.add("\u4e0b\u5468\u4e8c+0");
        nextWeekLt.add("\u4e0b\u5468\u4e09+0");
        nextWeekLt.add("\u4e0b\u5468\u56db+0");
        nextWeekLt.add("\u4e0b\u5468\u4e94+0");
        nextWeekLt.add("\u4e0b\u5468\u516d+0");
        nextWeekLt.add("\u4e0b\u5468\u65e5+0");
        nextWeekLt.add("\u4e0b\u5468\u4e00+1");
        nextWeekLt.add("\u4e0b\u5468\u4e8c+1");
        nextWeekLt.add("\u4e0b\u5468\u4e09+1");
        nextWeekLt.add("\u4e0b\u5468\u56db+1");
        nextWeekLt.add("\u4e0b\u5468\u4e94+1");
        nextWeekLt.add("\u4e0b\u5468\u516d+1");
        nextWeekLt.add("\u4e0b\u5468\u65e5+1");
        nextWeekLt.add("\u4e0b\u54681+0");
        nextWeekLt.add("\u4e0b\u54682+0");
        nextWeekLt.add("\u4e0b\u54683+0");
        nextWeekLt.add("\u4e0b\u54684+0");
        nextWeekLt.add("\u4e0b\u54685+0");
        nextWeekLt.add("\u4e0b\u54686+0");
        nextWeekLt.add("\u4e0b\u5468\u672b+0");
        nextWeekLt.add("\u4e0b\u54681+1");
        nextWeekLt.add("\u4e0b\u54682+1");
        nextWeekLt.add("\u4e0b\u54683+1");
        nextWeekLt.add("\u4e0b\u54684+1");
        nextWeekLt.add("\u4e0b\u54685+1");
        nextWeekLt.add("\u4e0b\u54686+1");
        nextWeekLt.add("\u4e0b\u5468\u672b+1");
        exceptionLt.addAll(
                Utils.StringToListStr(
                        "\u6708,\u65e5,\u53f7,/,-,\u5468,\u8fdc,\u660e,\u660e\u5929,\u540e,\u540e\u5929,\u5929"));
    }
}
