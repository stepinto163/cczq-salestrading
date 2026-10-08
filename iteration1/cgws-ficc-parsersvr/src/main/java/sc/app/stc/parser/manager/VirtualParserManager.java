package sc.app.stc.parser.manager;

import ats.app.ym.utils.logger.AsyncSlf4jLoggerFactory;
import cn.hutool.core.util.StrUtil;
import com.alibaba.fastjson.JSON;
import org.apache.lucene.analysis.Analyzer;
import org.apache.lucene.analysis.TokenStream;
import org.apache.lucene.analysis.tokenattributes.CharTermAttribute;
import org.apache.lucene.analysis.tokenattributes.OffsetAttribute;
import org.apache.lucene.analysis.tokenattributes.TypeAttribute;
import org.slf4j.Logger;
import org.springframework.beans.BeanUtils;
import org.springframework.util.CollectionUtils;
import org.wltea.analyzer.lucene.IKAnalyzer;
import sc.app.stc.parser.analyzer.Direction;
import sc.app.stc.parser.analyzer.ICity;
import sc.app.stc.parser.analyzer.Rating;
import sc.app.stc.parser.analyzer.po.City;
import sc.app.stc.parser.analyzer.po.CityArgs;
import sc.app.stc.parser.po.VirtualPo;

import java.io.IOException;
import java.io.StringReader;
import java.util.*;
import java.util.regex.Pattern;
import java.util.stream.Collectors;

public class VirtualParserManager{
    private static final Logger logger = AsyncSlf4jLoggerFactory.getLogger(VirtualParserManager.class);
    private static Pattern patternRating=Pattern.compile("a{1,3}(\\+|\\-)?(及)?(以上)?");
    static ICity cityAnalyzer = new ICity();
    static Analyzer analyzer = new IKAnalyzer(true);


    public static List<VirtualPo> parseVirtual(List<String> contents) {
        List<VirtualPo> virtualPos = new ArrayList<>();
        logger.info("contents size:{}",contents.size());
        for(String content : contents){
            content.replace("\r\n|\r|\n"," ");
            logger.info("parseVirtual content:{}",content);
            List<VirtualPo> poList = parser(content);
            if(!CollectionUtils.isEmpty(poList)){
                virtualPos.addAll(poList);
            }
        }
        return virtualPos;
    }

    public static List<VirtualPo> parser(String content){
        if(StrUtil.isEmpty(content) || content.trim().isEmpty()){
            return null;
        }

        String resourceContent = content;

        String rating=null;
        String termPo = null;
        Set<String> areaPo = new HashSet<>();

        String direction = Direction.getDirection(content).get("action");
        content = Direction.getDirection(content).get("contents");

        logger.info("resourceContent changeTermDays:{}===={},direction:{},",resourceContent,content,direction);
        List<String> contList = Arrays.asList(content.split(" "));
        List<String> contents = contList.stream().filter(string -> StrUtil.isNotEmpty(string) && !string.trim().isEmpty()).map(String::trim).collect(Collectors.toList());
        logger.info("filt contents:{}",contents);

        List<VirtualPo> poList = new ArrayList<>();

        for(String cont : contents){
            List<String> contentItems = new ArrayList<>();

            TokenStream ts = null;
            try {
                ts = analyzer.tokenStream("", new StringReader(cont));
                //获取词元位置属性
                OffsetAttribute offset = ts.addAttribute(OffsetAttribute.class);
                //获取词元文本属性
                CharTermAttribute term = ts.addAttribute(CharTermAttribute.class);
                //获取词元文本属性
                TypeAttribute type = ts.addAttribute(TypeAttribute.class);
                //重置TokenStream（重置StringReader）
                ts.reset();
                //迭代获取分词结果
                while (ts.incrementToken()) {
                    //System.out.println(offset.startOffset() + " - " + offset.endOffset() + " : " + term.toString() + " | " + type.type());
                    contentItems.add(term.toString());
                }
                //关闭TokenStream（关闭StringReader）
                ts.end();
            } catch (IOException e) {
                logger.error(content, e);
            } finally {
                //释放TokenStream的所有资源
                if(ts != null){
                    try {
                        ts.close();
                    } catch (IOException e) {
                        e.printStackTrace();
                    }
                }
            }

            logger.info("contentItems:{}",contentItems);

            if(rating==null){
                rating = getRating(contentItems);
            }

            String term = getTerm(contentItems);
            Set<String> areas = getArea(null, contentItems);

            if(term == null && areas == null){
                continue;
            }else if(term != null && areas.size()>0){
                for(String a: areas){
                    VirtualPo po = new VirtualPo();
                    po.term = term;
                    po.area = a;
                    poList.add(po);
                }
            }else if(term != null){
                termPo = term;
            }else if(areas.size()>0){
                areaPo.addAll(areas);
            }
        }
        if(CollectionUtils.isEmpty(poList)){
            VirtualPo po = new VirtualPo();
            po.term = termPo;
            po.direction= direction;
            po.rating = rating;
            po.text = resourceContent;
            if(!CollectionUtils.isEmpty(areaPo)){
                for(String a : areaPo){
                    VirtualPo aPo = new VirtualPo();
                    BeanUtils.copyProperties(po,aPo);
                    aPo.area = a;
                    poList.add(aPo);
                }
            } else{
                poList.add(po);
            }
        } else {
            String finalDirection = direction;
            String finalRating = rating;
            poList.stream().forEach(item->{
                item.direction = finalDirection;
                item.rating= finalRating;
                item.text = resourceContent;
            });
        }
        logger.info("content po:{}===={}",resourceContent,CollectionUtils.isEmpty(poList)? null : JSON.toJSONString(poList));
        return poList;
    }


    static String getTerm(List<String> contents){
        for(String content : contents){
            String cont = content.toUpperCase();
            if(cont.contains("Y") || cont.contains("M") || cont.contains("D")){
                contents.remove(content);
                if(cont.contains("-")){
                    String contNum = cont.substring(0, cont.length() - 1);
                    String contUnit = cont.substring(cont.length()-1);
                    String[] contData = contNum.split("-");
                    return contData[0] + contUnit + "-" + contData[1] + contUnit;
                }else{
                    return cont;
                }

            }
        }
        return null;
    }

    static Set<String> getArea(String area,List<String> contents){
        Set<String> areaSet = new HashSet<>();
        List<String> contentsCopy = new ArrayList<>(contents);
        for(String cont : contentsCopy){
            CityArgs ca=new CityArgs();
            ca.type="";
            ca.content=cont;
            City city = cityAnalyzer.getCity(ca);
            if(city != null){
                contents.remove(cont);
                String analyCity = city.Province == null ? city.City : city.Province;
                if(StrUtil.isNotEmpty(analyCity)){
                    areaSet.add(analyCity);
                }
            }
        }
        return areaSet;
    }

    static String getRating(List<String> contents){
        for(String cont : contents){
            String rating = Rating.getRating(cont);
            if(StrUtil.isNotEmpty(rating)){
                contents.remove(cont);
                return rating.toUpperCase();
            }
        }
        return null;
    }

    public static void main(String[] args) {
        Direction.init();
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
                "询价再进广东15年 湖南10年 浙江10年陕西7年",
                "收6-7y中等以上地区一般债",
                "收山东债8-10年",
                "Bid 浙江，湖南，4%以上5年内的城投"
        };

        for (String content : testCase) {
            List<VirtualPo> parser = parser(content);
            System.out.println(content+ "---"+ JSON.toJSONString(parser));
        }

    }

}
