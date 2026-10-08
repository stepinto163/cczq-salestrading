package sc.app.stc.parser.analyzer;

import ats.app.ym.utils.logger.AsyncSlf4jLoggerFactory;
import org.slf4j.Logger;

import java.util.HashMap;
import java.util.regex.Matcher;
import java.util.regex.Pattern;

public class Rating {
    private static final Logger logger = AsyncSlf4jLoggerFactory.getLogger(Rating.class);
    private static Pattern patternRating=Pattern.compile("a{1,3}(\\+|\\-)?(及)?(以上)?");
    private static Pattern patternRatingResult=Pattern.compile("a{1,3}(\\+|\\-)?(&)?(@)?");
    private static HashMap<String,Integer> mapRating=new HashMap<String,Integer>();
    static {
        mapRating.put("a-", 0);
        mapRating.put("a", 1);
        mapRating.put("a+", 2);
        mapRating.put("aa-", 3);
        mapRating.put("aa", 4);
        mapRating.put("aa+", 5);
        mapRating.put("aaa-", 6);
        mapRating.put("aaa", 7);
        mapRating.put("aaa+", 8);
    }


    public static String FindAndReplace(String src){
        Matcher m=patternRating.matcher(src.toLowerCase());
        while(m.find()){
            String str=m.group();
            str=str.replaceAll("及", "");
            str=str.replaceAll("以上", "");
            str=str.replaceAll(" ", "");
            return str;
        }
        return "";
    }


    public static String getRating(String cont){
        boolean matches = Pattern.matches("a{1,3}(\\+|\\-)?(及)?(以上)?", cont);
        if(matches){
            cont=cont.replaceAll("及", "");
            cont=cont.replaceAll("以上", "");
            cont=cont.replaceAll(" ", "");
            return cont;
        }
        return null;
    }


    public static void main(String[] args) {
        System.out.println(FindAndReplace("bid AA+及以上"));
        System.out.println(FindAndReplace("bid aa+以上"));
        System.out.println(FindAndReplace("bid aaa"));
    }


}
