package sc.app.stc.parser.analyzer;

import ats.app.ym.utils.logger.AsyncSlf4jLoggerFactory;
import com.sumscope.TenorDataItem;
import com.sumscope.WordMap;
import org.slf4j.Logger;

import java.text.SimpleDateFormat;
import java.util.Calendar;
import java.util.HashMap;
import java.util.Map;
import java.util.regex.Matcher;
import java.util.regex.Pattern;

public class TermDays {
    private static final Logger logger = AsyncSlf4jLoggerFactory.getLogger(TermDays.class);
    private static Pattern patternRating = Pattern.compile("(一|二|三|[1-9])(年内|y内|Y内|年以内|y以内|Y以内)$");
    static WordMap wm = WordMap.getInstance();
    private static final Map<String, Integer> Arab2Chinese = new HashMap<String, Integer>() {
        {
            put("零", 0);
            put("一", 1);
            put("二", 2);
            put("三", 3);
            put("四", 4);
            put("五", 5);
            put("六", 6);
            put("七", 7);
            put("八", 8);
            put("九", 9);
            put("十", 10);
        }
    };
    public static String changeTermDays(String content){
        content = content.toLowerCase();
        Pattern compile = Pattern.compile("(一|二|三|四|五|六|七|八|九|十|[1-9])(年内|年以内|y内|y以内)");
        Matcher matcher = compile.matcher(content);
        while (matcher.find()){
            String group = matcher.group();
            logger.debug("changeTermDays match term :{}",group);
            String replace = group.replaceAll("年", "y");
            String[] ys = replace.split("y");
            if(ys[0].matches("^[0-9]*$")){
                content = content.replace(group," 0-" + ys[0] + "y");
            } else if(ys[0].matches("^一|二|三|四|五|六|七|八|九|十$")){
                content = content.replace(group," 0-" + Arab2Chinese.get(ys[0]) + "y");
            }
            logger.debug("changeTermDays content:{}",content);
        }

        content=content.replaceAll(" Y", "y");
        content=content.replaceAll("y、", "-");
        content=content.replaceAll("年", "年 ");
        content=content.replaceAll("y", "y ");
        content=content.replaceAll(" y", "y");

        Calendar cal = Calendar.getInstance();
        SimpleDateFormat sdf = new SimpleDateFormat("yyyy-MM-dd");
        String dateStr = sdf.format(cal.getTime());
        String termContent = TenorDataItem.FindAndReplace(content, dateStr);
        return termContent;
    }


    public static void main(String[] args) {
        String[] testCase = new String[]{
                "bid 20-30Y 高估值地方债",
                "bid 20-30 Y 高估值地方债",
                "bid 7y、8y 地方债一般债 票面3+",
                "bid16+城投债交易所AA左右",
                "bid: 9-10年 山东和青岛债",
                "bid 13y 地方债 票面3.5+",
                "bid 2-3Y好地方一般债",
                "bid 巴中 济宁天津一年以内",
                "bid:湖南城投，担保AA+以上",
                "bid 5-10y 青岛债",
                "bid 天津",
                "bid 交易所地方债 3y内",
                "bid 山东地区非潍坊，高密",
                "bid 1年内，14+",
                "bid 江苏和江西一年以内，收益在7以上",
                "询价再进广东15年 湖南10年 浙江10年陕西7年"
        };
        for (String content : testCase) {
            System.out.println(content + " ----- " + changeTermDays(content));
        }
    }
}
